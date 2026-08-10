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
    public partial class PassFailInfromation : Form
    {
        public Global m__G = null;
        public F_Main MyOwner = null;
        public bool mForPW = false;

        public PassFailInfromation(bool forPW=false)
        {
            InitializeComponent();
            mForPW = forPW;
        }

        private void Enter_Click(object sender, EventArgs e)
        {
            if ( !mForPW )
            {
                if (tbPassword.Text == m__G.GetPassword())
                {
                    tbPassword.Text = "";
                    this.Hide();
                    MyOwner.ShowAdminMode();

                }
                else
                {
                    tbPassword.Text = "";
                    this.Hide();
                    MyOwner.ShowOperatorMode();
                    MessageBox.Show("Incorrect Password");
                }
            }
            else
            {
                m__G.SetPassword(tbPassword.Text);
                this.Hide();
            }
        }

        private void btnEnter_VisibleChanged(object sender, EventArgs e)
        {
            if (mForPW)
                btnEnter.Text = "Save Password";
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
