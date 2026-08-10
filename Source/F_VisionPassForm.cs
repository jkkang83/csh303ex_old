using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Matrox.MatroxImagingLibrary;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace CSH030Ex
{
    public partial class F_VisionPassForm : Form
    {
        public string mPath = "C:\\CSHTest\\DoNotTouch\\VisionPassWord.txt";
        public F_VisionPassForm()
        {
            InitializeComponent();

            if (!File.Exists(mPath))
            {
                StreamWriter sw = new StreamWriter(mPath);
                sw.WriteLine("1234");
                sw.Close();
            }
        }
        
        private void Enter_Click(object sender, EventArgs e)
        {
            if (tbPassword.Text == GetPassword())
            {
                tbPassword.Text = "";
                DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                tbPassword.Text = "";
                DialogResult = DialogResult.Cancel;
                this.Hide();
                MessageBox.Show("Incorrect Password");
            }
        }
        private string GetPassword()
        {
            StreamReader sr = new StreamReader(mPath);
            string pw = sr.ReadLine();
            sr.Close();
            return pw;
        }
        private void SetPassword(string pw)
        {
        }
        private void btnEnter_VisibleChanged(object sender, EventArgs e)
        {
        }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                btnEnter.PerformClick();
            }

        }
    }
}
