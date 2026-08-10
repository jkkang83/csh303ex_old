using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSH030Ex
{
    public partial class TestItemOnOff : Form
    {
        public Global m__G = null;
        public F_Main MyOwner = null;
        private bool mbInit = false;

        public bool[] mTestItemOnOff = new bool[200];

        public TestItemOnOff()
        {
            InitializeComponent();
            for (int i = 0; i < 200; i++ )
                mTestItemOnOff[i] = true;

        }

        private void TestItemOnOff_Load(object sender, EventArgs e)
        {
            InitTestItemOnOff();
            //this.Location = new Point(480, 224);
            this.Location = new Point(843, 178);
            this.Size = new Size(474, 865);
        }

        private void InitTestItemOnOff()
        {
            if (!mbInit)
            {
                this.dataGridView1.Rows.Clear();

                //this.SetDesktopLocation(480, 224);
                this.SetDesktopLocation(843, 177);

                this.dataGridView1.Columns[0].Width = 100;
                this.dataGridView1.Columns[1].Width = 273;
                this.dataGridView1.Columns[2].Width = 100;

                this.dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dataGridView1.ColumnHeadersHeight = 20;
                this.dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                Color[] pColor = new Color[2] { Color.White, Color.Lavender };
                int i_color = 1;
                for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
                {
                    //MessageBox.Show("InitTestItemOnOff " + i.ToString());
                    this.dataGridView1.Rows.Add(m__G.mTestItem[i, 0], m__G.mTestItem[i, 1], m__G.mTestItem[i, 10]);
                    this.dataGridView1.Rows[i].Height = 16;
                    this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                    this.dataGridView1[0, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                    this.dataGridView1[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                    this.dataGridView1[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                    this.dataGridView1[0, i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    this.dataGridView1[2, i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    if (m__G.mTestItem[i, 0] != "")
                        i_color++;
                    this.dataGridView1[0, i].Style.BackColor = pColor[ i_color % 2];
                }
                this.dataGridView1.Rows.Add("", "", "");
                this.dataGridView1.Rows[m__G.sNUM_TESTITEM].Height = 16;
                this.dataGridView1.Rows[m__G.sNUM_TESTITEM].Resizable = DataGridViewTriState.False;
                this.dataGridView1[0, m__G.sNUM_TESTITEM].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[1, m__G.sNUM_TESTITEM].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[2, m__G.sNUM_TESTITEM].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[0, m__G.sNUM_TESTITEM].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dataGridView1[2, m__G.sNUM_TESTITEM].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                this.dataGridView1.ReadOnly = true;
                this.dataGridView1.RowHeadersVisible = false;
                mbInit = true;
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //MessageBox.Show("e.ColumnIndex = " + e.ColumnIndex.ToString() + " e.RowIndex = " + e.RowIndex.ToString());
            if (e.RowIndex >= 0)
            {
                if ( e.ColumnIndex == 0 && e.RowIndex < m__G.sNUM_TESTITEM)
                {
                    if ( this.dataGridView1[0, e.RowIndex].Value.ToString().Length > 0 )
                    {
                        if (this.dataGridView1[2, e.RowIndex].Value.ToString().Contains('t') || this.dataGridView1[2, e.RowIndex].Value.ToString().Contains('T'))
                        {
                            for (int i = e.RowIndex; i < m__G.sNUM_TESTITEM; i++)
                            {
                                this.dataGridView1[2, i].Value = "false";
                                this.dataGridView1[1, i].Style.BackColor = Color.Gray;
                                this.dataGridView1[2, i].Style.BackColor = Color.Gray;
                                mTestItemOnOff[i] = false;
                                m__G.mTestItem[i, 10] = (mTestItemOnOff[i] == true ? "true" : "false");
                                if (this.dataGridView1[0, i + 1].Value.ToString().Length > 0) break;
                            }
                        }else
                        {
                            for (int i = e.RowIndex; i < m__G.sNUM_TESTITEM; i++)
                            {
                                this.dataGridView1[2, i].Value = "true";
                                this.dataGridView1[1, i].Style.BackColor = Color.White;
                                this.dataGridView1[2, i].Style.BackColor = Color.White;
                                mTestItemOnOff[i] = true;
                                m__G.mTestItem[i, 10] = (mTestItemOnOff[i] == true ? "true" : "false");
                                if (this.dataGridView1[0, i + 1].Value.ToString().Length > 0) break;
                            }
                        }
                    }
                }
                else
                {
                    string OnOffState = this.dataGridView1[2, e.RowIndex].Value.ToString();
                    if (OnOffState.Contains('T') || OnOffState.Contains('t'))
                    {
                        this.dataGridView1[2, e.RowIndex].Value = "false";
                        mTestItemOnOff[e.RowIndex] = false;
                        //MessageBox.Show(e.RowIndex.ToString() + ":" + mTestItemOnOff[e.RowIndex].ToString());
                        this.dataGridView1[1, e.RowIndex].Style.BackColor = Color.Gray;
                        this.dataGridView1[2, e.RowIndex].Style.BackColor = Color.Gray;
                    }
                    else
                    {
                        this.dataGridView1[2, e.RowIndex].Value = "true";
                        mTestItemOnOff[e.RowIndex] = true;
                        //MessageBox.Show(e.RowIndex.ToString() + ":" + mTestItemOnOff[e.RowIndex].ToString());
                        this.dataGridView1[1, e.RowIndex].Style.BackColor = Color.White;
                        this.dataGridView1[2, e.RowIndex].Style.BackColor = Color.White;
                    }
                    m__G.mTestItem[e.RowIndex, 10] = (mTestItemOnOff[e.RowIndex] == true ? "true" : "false");
                }
            }
        }

        private void TestItemSaveBtn_Click(object sender, EventArgs e)
        {
            SaveTestItemList();
            MyOwner.UpdateDataGridView();
            this.Hide();
        }

        public void SaveTestItemList()
        {

            string tmpStr = string.Empty;          
            int effItemNum = 0;
            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TestItemSetting.txt";
            StreamWriter sr = new StreamWriter(strFileName);

            for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
            {
                sr.Write(mTestItemOnOff[i].ToString() + "\r\n");
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    if (tmpStr == string.Empty)
                        tmpStr = mTestItemOnOff[i].ToString();
                    else
                        tmpStr = tmpStr + "," + mTestItemOnOff[i].ToString();
                }
                    
                if (mTestItemOnOff[i])
                {
                    m__G.mGridToTestItem[effItemNum] = i;
                    m__G.mTestItemToGrid[i] = effItemNum++;
                    m__G.mTestItem[i,10] = "true";
                }else
                {
                    m__G.mTestItemToGrid[i] = -1;
                    m__G.mTestItem[i, 10] = "false";
                }
            }
            sr.Close();
            if (F_Main.MachineType == (int)MachineType.Master)
            {
                m__G.fManage.PC2SendData("STI", tmpStr, tmpStr.Length, 2);
                F_Main.UIControltmr.Enabled = true;
            }
                

        }
        public void LoadTestItemSetting()
        {
            if (!mbInit)
                InitTestItemOnOff();

            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TestItemSetting.txt";

            if ( File.Exists(strFileName))
            {
                StreamReader sr = new StreamReader(strFileName);
                string allData = sr.ReadToEnd();
                sr.Close();
                string[] rows = allData.Split("\n".ToCharArray());
                int len = rows.Length-1;
                if (len < 1) return;

                int preNUM_TESTITEM = m__G.sNUM_TESTITEM;
                //MessageBox.Show(len.ToString() + ":" + preNUM_TESTITEM.ToString());

                if (len < preNUM_TESTITEM)
                    preNUM_TESTITEM = len;
                if (len > preNUM_TESTITEM)
                    len = preNUM_TESTITEM;

                int k = 0;
                int i = 0;
                for (i = 0; i < len; i++)
                {
                    if (rows[i].Contains('T') || rows[i].Contains('t'))
                        mTestItemOnOff[i] = true;
                    else
                        mTestItemOnOff[i] = false;

                    m__G.mTestItem[i, 10] = rows[i];
                    this.dataGridView1[2, i].Value = rows[i];
                    if (rows[k].Contains('T'))
                    {
                        this.dataGridView1[1, i].Style.BackColor = Color.White;
                        this.dataGridView1[2, i].Style.BackColor = Color.White;
                    }
                    else
                    {
                        this.dataGridView1[1, i].Style.BackColor = Color.Gray;
                        this.dataGridView1[2, i].Style.BackColor = Color.Gray;
                    }
                    if (i == rows.Length) break;
                }
                for (; i < preNUM_TESTITEM; i++)
                {
                        mTestItemOnOff[i] = false;

                    m__G.mTestItem[i, 10] = "false";
                    this.dataGridView1[2, i].Value = 0;
                    this.dataGridView1[1, i].Style.BackColor = Color.Gray;
                    this.dataGridView1[2, i].Style.BackColor = Color.Gray;
                    if (i == rows.Length) return;
                }
            }
            else
            {
                for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
                {
                    m__G.mTestItem[i, 10] = ( mTestItemOnOff[i]==true ? "true" : "false" );
                    this.dataGridView1[2, i].Value = mTestItemOnOff[i];
                    this.dataGridView1[1, i].Style.BackColor = Color.White;
                    this.dataGridView1[2, i].Style.BackColor = Color.White;
                }
            }
            bool bFind = false;
            int startStep = 0;
            for (int k = 0; k < m__G.sNUM_TESTITEM; k++)
            {
                try
                {
                    if (dataGridView1[1, k].Value.ToString() == "SettlingTime" && !bFind)
                    { startStep = k; bFind = true; }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}
