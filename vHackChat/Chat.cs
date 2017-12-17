using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using vHackApi.Chat;

namespace vHackChat
{
    public partial class Chat : Form
    {
        private vhChat _chat;
        public Chat()
        {
            InitializeComponent();

            textBoxLog.BackColor = Color.Black;
            textBoxLog.Text = textBoxWrite.Text = "";

            var cfg = new vHackBot.Config();
            
            var api = new vHackApi.Api.vhAPI(cfg);
            //vHackApi.Chat.ChatTest.Test(api);

            //vHackApi.Chat.ChatUtils.ConnectToChat(api);

            /*vHackApi.Chat.ChatUtils.RunChat(api);*/
            _chat = new vHackApi.Chat.vhChat(cfg, api);
            _chat.PrivateMessage += Chat_PrivateMessage;
            _chat.MessageReceived += Chat_MessageReceived;
            _chat.Run();
        }

        private void Chat_MessageReceived(string msg)
        {
            richTextBox1.Invoke(new Action(delegate
            {
                richTextBox1.AppendRichText($"{DateTime.Now}: {msg}", Color.Yellow);
                richTextBox1.AppendText("\r\n");
                richTextBox1.ScrollToCaret();
            }));
        }

        private void Chat_PrivateMessage(vhChat.Rule rule, string arg1, string arg2, string arg3)
        {
            textBoxLog.Invoke(new Action(delegate
            {
                if (arg1 != null)
                {
                    textBoxLog.AppendRichText($"[{DateTime.Now}][", Color.White);
                    var userColor = Color.DarkCyan;
                    var textColor = Color.GhostWhite;
                    if (rule == vhChat.Rule.Admin)
                        userColor = Color.Red;
                    else if (rule == vhChat.Rule.Mod)
                        userColor = Color.DarkViolet;
                    else if (rule == vhChat.Rule.Bot)
                        userColor = Color.DarkRed;
                    else if (rule == vhChat.Rule.Vip)
                        userColor = Color.Yellow;
                    else if (rule == vhChat.Rule.Me)
                    {
                        userColor = Color.LimeGreen;
                        textColor = Color.IndianRed;
                    }

                    textBoxLog.AppendRichText($"({rule}) {arg2}@{arg1}", userColor);
                    textBoxLog.AppendRichText("]: ", Color.White);
                    textBoxLog.AppendRichText(arg3, textColor);

                }
                else
                {
                    textBoxLog.AppendRichText($"[{DateTime.Now}]: ", Color.White);
                    textBoxLog.AppendRichText(arg3, Color.Red);
                }
                textBoxLog.AppendText("\r\n");
                richTextBox1.ScrollToCaret();

            }));
        }

        private void textBoxWrite_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;


            textBoxWrite.Invoke(new Action(delegate
            {
                //ChatUtils.SendChatMessage(textBoxWrite.Text);
                _chat.SendChat(textBoxWrite.Text);
                
                textBoxWrite.Clear();
            }));
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendRichText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
