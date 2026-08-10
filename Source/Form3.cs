using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSH030Ex
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        public void AddLog(string lstr)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    mLog.Text += lstr + "\r\n";
                    //mLog.SelectionStart = mLog.Text.Length;
                    //mLog.ScrollToCaret();
                });
            }
        }
        public string GetLog()
        {
            return mLog.Text;
        }
        private void mLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
