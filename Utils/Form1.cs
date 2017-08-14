using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    Invoke(new Action(() => toolStripStatusLabel1.Text = ""));
                    var txt64 = textBox1.Text;
                    var arr = Convert.FromBase64String(txt64);
                    var txt = Encoding.ASCII.GetString(arr);
                    textBox2.Invoke(new Action(() => textBox2.Text = txt));
                }
                catch (Exception exc)
                {
                    Invoke(new Action(() => toolStripStatusLabel1.Text = exc.Message));
                }
            });
        }
    }
}
