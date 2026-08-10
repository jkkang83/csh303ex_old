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
using TZAF;

namespace ProgramMonitor
{
    public partial class Form1 : Form
    {
        public Network Network = new Network();
        public bool IsConneted = false;
        public bool IsSendMonitor = false;
        public Form1()
        {
            InitializeComponent();
            Network.LogEvened += Network_LogEvened;
            Network.ReciveEvened += Network_ReciveEvened;
            Network.StartServer(5001);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if(Network.Tcp.Connected)
                    {
                        byte[] buffs = Encoding.ASCII.GetBytes("Monitor");
                        Network.SendData(buffs);
                        IsConneted = false;
                        Thread.Sleep(5000);
                        if(!IsConneted)
                        {
                            if (InvokeRequired)
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    LogNetwork.AppendText("Client PGM Restart.." + "\r\n");
                                });
                            }
                            else
                            {
                                LogNetwork.AppendText("Client PGM Restart.." + "\r\n");
                            }
                            System.Diagnostics.Process[] List = System.Diagnostics.Process.GetProcessesByName("TZAF");
                            if (List.Length > 0)
                            {
                                List[0].Kill();
                            }
                            Thread.Sleep(5000);
                            System.Diagnostics.Process.Start("C:\\Users\\ACTRO1\\Desktop\\CSH035\\Source\\bin\\Debug\\TZAF.exe");
                        }
                    }
                }
            });
        }

        private void Network_ReciveEvened(object sender, byte[] e)
        {
            string cmd = Encoding.ASCII.GetString(e);
            switch (cmd)
            {
                case "OK":
                    IsConneted = true;
                    break;
            }
        }

        private void Network_LogEvened(object sender, string e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if(e.Contains("Client") || e.Contains("Server"))
                        LogNetwork.AppendText(e + "\r\n");
                });
            }
            else
            {
                if (e.Contains("Client") || e.Contains("Server"))
                    LogNetwork.AppendText(e + "\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process[] List = System.Diagnostics.Process.GetProcessesByName("TZAF");
            if(List.Length > 0)
            {
                List[0].Kill();
            }
            Thread.Sleep(5000);
            System.Diagnostics.Process.Start("C:\\Users\\ACTRO1\\Desktop\\CSH035\\Source\\bin\\Debug\\TZAF.exe");
        }
    }
}
