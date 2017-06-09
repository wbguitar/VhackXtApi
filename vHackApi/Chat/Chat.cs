using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace vHackApi
{
    class RemoteClient
    {
        Queue<string> outbox = new Queue<string>();

        public RemoteClient()
        {

        }

        public void say()
        {

        }
    }

    public class Client
    {
        Socket client;
        Queue<string> outbox = new Queue<string>();
        // ManualResetEvent instances signal completion.
        readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private string host;
        private int port;

        public Client(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Connect()
        {
            //if (sock != null)
            //{
            //    sock.Dispose();
            //    sock = null;
            //}

            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(host, port, callback, null);
        }

        public void WaitConnect() { connectDone.WaitOne(); }
        public void WaitSend() { sendDone.WaitOne(); }
        public void WaitReceive() { receiveDone.WaitOne(); }

        private void callback(IAsyncResult ar)
        {
            try
            {
                //var cli = (Socket)ar.AsyncState;
                //cli.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                sendCallback, client);
            sendDone.WaitOne();
        }

        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                System.Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }


        public class State
        {
            static int ID = 0;
            public State() { id = ID++; }

            public int id;
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();

            public override int GetHashCode()
            {
                return id;
            }
        }

        public void Receive()
        {
            try
            {
                // Create the state object.
                State state = new State();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, State.BufferSize, 0,
                    receiveCallback, state);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

        //List<StateObject> states = new List<StateObject>();

        //private String response = String.Empty;
        public event Action<State> OnResponse = (s) => { };

        private void receiveCallback(IAsyncResult ar)
        {
            //lock (client)
            {
                try
                {
                    // Retrieve the state object and the client socket 
                    // from the asynchronous state object.
                    State state = (State)ar.AsyncState;
                    Socket cli = state.workSocket;

                    // Read data from the remote device.
                    int bytesRead = cli.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                        // Get the rest of the data.
                        cli.BeginReceive(state.buffer, 0, State.BufferSize, SocketFlags.None, receiveCallback, state);
                    }
                    else
                    {
                        // All the data has arrived; put it in response.
                        if (state.sb.Length > 1)
                        {
                            //response = state.sb.ToString();
                            OnResponse(state);
                        }
                        // Signal that all bytes have been received.
                        receiveDone.Set();
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                } 
            }
        }
    }
}
