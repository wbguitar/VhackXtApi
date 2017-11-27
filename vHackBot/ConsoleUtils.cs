using System.Runtime.InteropServices;

namespace vHackBot
{
    public class ConsoleUtils
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        public delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static event EventHandler OnConsole
        {
            add
            {
                SetConsoleCtrlHandler(value, true);
            }

            remove
            {
                SetConsoleCtrlHandler(value, false);
            }
        }
        
    }
}