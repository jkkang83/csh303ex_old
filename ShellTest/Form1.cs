using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShellTest
{
    public partial class Form1 : Form
    {
        public const int WM_COPYDATA = 0x004A;
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            //[MarshalAs(UnmanagedType.LPStr)]
            public IntPtr lpData;
        }

        public COPYDATASTRUCT cds;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            InitializeComponent();
            lbxSendMessage.Items.Add("TstID");
            lbxSendMessage.Items.Add("MOTID");
            lbxSendMessage.Items.Add("WhRUD");
            lbxSendMessage.Items.Add("Fpath$OpticsConfig");
            lbxSendMessage.Items.Add("Fpath$ModelFileList");
            lbxSendMessage.Items.Add("Fpath$ModelFile");
            lbxSendMessage.Items.Add("Fpath$SchematicFile");
            lbxSendMessage.Items.Add("Fpath$RecipeFile");
            lbxSendMessage.Items.Add("Fpath$DcfFile");
            lbxSendMessage.Items.Add("Fpath$ScaleFile");
            lbxSendMessage.Items.Add("ScrCp$Admin");
            lbxSendMessage.Items.Add("ScrCp$Operator");
            lbxSendMessage.Items.Add("ScrCp$Vision");
            lbxSendMessage.Items.Add("ScrCp$ModelConfig");
            lbxSendMessage.Items.Add("ScrCp$MotSim");
            lbxSendMessage.Items.Add("ScrCp$Grab");
            lbxSendMessage.Items.Add("");
            lbxSendMessage.Items.Add("Updat!OpticsConfig");
            lbxSendMessage.Items.Add("Updat!ModelFileList");
            lbxSendMessage.Items.Add("Updat!ModelFile");
            lbxSendMessage.Items.Add("Updat!SchematicFile");
            lbxSendMessage.Items.Add("Updat!RecipeFile");
            lbxSendMessage.Items.Add("Updat!OrgRoi");
            lbxSendMessage.Items.Add("Recvr!OpticsConfig");
            lbxSendMessage.Items.Add("Recvr!ModelFileList");
            lbxSendMessage.Items.Add("Recvr!ModelFile");
            lbxSendMessage.Items.Add("Recvr!SchematicFile");
            lbxSendMessage.Items.Add("Recvr!RecipeFile");
            lbxSendMessage.Items.Add("Recvr!OrgRoi");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tbSendMessage.Text.Length > 0 )
                SendCopyDataMessage(tbSendMessage.Text);
        }

        void SendCopyDataMessage(string str)
        {
            //char[] data = str.ToCharArray();
            str = str + "\0";
            cds.dwData = (IntPtr)0;
            cds.cbData = str.Length + 1;
            cds.lpData = Marshal.AllocCoTaskMem(str.Length);
            cds.lpData = Marshal.StringToCoTaskMemAnsi(str);
            IntPtr iPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(cds));
            Marshal.StructureToPtr(cds, iPtr, true);

            //  Socket 프로그램이 완성되면, ShellTest 는 이름을 해당 프로그램 이름으로 바꿔줘야 한다.
            IntPtr hWndShell = Process.GetProcessesByName("TZAF")[0].MainWindowHandle;
            if (hWndShell != null)
            {
                // Close Window
                SendMessage(hWndShell, WM_COPYDATA, (IntPtr)0, iPtr);
            }
            Marshal.FreeCoTaskMem(cds.lpData);
            Marshal.FreeCoTaskMem(iPtr);
        }
        // containing a geocoordinate rectangle



        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //
                case (int)WM_COPYDATA:
                    COPYDATASTRUCT cds = new COPYDATASTRUCT();
                    cds = Marshal.PtrToStructure<COPYDATASTRUCT>(m.LParam);
                    byte[] data = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, data, 0, cds.cbData);
                    Encoding unicodeStr = Encoding.ASCII;
                    char[] myString = unicodeStr.GetChars(data);
                    string returnText = new string(myString);
                    
                    tbReceived.Text = returnText;
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string lstr = lbxSendMessage.Items[lbxSendMessage.SelectedIndex].ToString();
            if (lstr.Length > 0)
                SendCopyDataMessage(lstr);

        }
    }
}
