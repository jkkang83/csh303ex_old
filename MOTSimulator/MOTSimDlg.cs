using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MOTSimulator
{
    public partial class MOTSimDlg : Form
    {
        public double mSNt = 0;
        public double mSNb = 0;
        public double mSSb = 0;
        public double mSSt = 0;
        public double mTNb = 0;
        public double mTNt = 0;

        public double sin40 = Math.Sin(40 / 180.0 * Math.PI);
        public double cos40 = Math.Cos(40 / 180.0 * Math.PI);
        public double tan40 = Math.Tan(40 / 180.0 * Math.PI);

        const double LensMag = 0.30;
        public void SetSideviewTheta(double rad)
        {
            sin40 = Math.Sin(rad);
            cos40 = Math.Cos(rad);
            tan40 = Math.Tan(rad);
        }

        public double Nt = 0;   //  N-S 마크의 상단
        public double Nb = 0;   //  N-S 마크의 하단
        public double St = 0;   //  E 마크 상단
        public double Sb = 0;   //  E 마크 하단

        public double W = 0;    //  Dist. from Side View Center to Actuator Center, + means Top View Center Direction.
        public double S = 0;    //  Focus Translation Window Edge Offset, + means right down
        public double Oh = 0;   //  Target Top View Offset from Side View Center
        public double M = 1.475;    //  Dist. from Side View Center to Top view center on Image Plane
        public double O = 0;

        public string[] MOTtype = new string[6] { "A", "B", "C", "D", "E", "F" };

        public MOTSimDlg()
        {
            InitializeComponent();
            InitDataGridView();
            InitDataGridView2();
            InitDataGridView3();
            InitDataGridView4();
        }

        public double FOV_YSHIFT = 0.4714 ;  //  + 는 영상에서 SIde View 를 줄이는 방향 , Top View 는 넓어짐 기본 30pixel 이동한 것으로 가정.
        public double planeHeight = 0;
        private void btnOptimizing_Click(object sender, EventArgs e)
        {
            double min = 0;
            double H = 0;
            double[] array = new double[6];
            double[] arrayRes = new double[6];
            double rS = 0;
            double rH = 0;
            double rOh = 0;

            //  FOV TopView SideView 가 Y방향으로 영상을 2등분하면 Top View 영역에서 측정 가능 Stroke 범위가 줄어들므로
            //  FOV Shift 로 TopVIew 영역을 30pixel ( = 471.42857 um) 넓힌 상태를 가정한다.

            try
            {
                Nt = double.Parse(this.dataGridView1[1, 0].Value.ToString()); //  마크중심(=Side View Center) 부터 N-S 마크 Top 까지 거리
                Nb = double.Parse(this.dataGridView1[1, 1].Value.ToString()); //  마크중심(=Side View Center) 부터 N-S 마크 Top 까지 거리
                St = Math.Abs(double.Parse(this.dataGridView1[1, 2].Value.ToString())); //  마크중심(=Side View Center) 부터 E 마크 Top 까지 거리
                Sb = Math.Abs(double.Parse(this.dataGridView1[1, 3].Value.ToString())); //  마크중심(=Side View Center) 부터 E 마크 Btm 까지 거리

                M = double.Parse(this.dataGridView2[1, 0].Value.ToString());
                O = double.Parse(this.dataGridView2[1, 2].Value.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Input Data is not correct.");
                return;
            }

            W = (Sb - Nt) / 2;  //  N-S top ~ E btm 간 거리의 차이의 절반 : Side View Center 로부터 N-S top ~ E btm 중심까지의 거리, 0 이 가장 적합.
            for (int i = 0; i < 200; i++)
            {
                S = -0.5 + i / 200.0;    //  S 값으로 어떤 값을 취하면 가장 좋을지 나중에 판단한다. -0.5 ~ +0.5 범위에서 검색 : Side View 중심의 Y Shift 량, 화면 상단으로 이동 시 S > 0
                for (int j = 0; j <= 600; j++)
                {
                    H = -0.3 + 0.6 * j / 600.0;    //  H 값으로 어떤 값을 취하면 가장 좋을 지 판단. -0.3 ~ +0.3 범위에서 검색 : 측정면의 높이 , - 방향은 하강, + 방향은 상승
                    Oh = 1.95 + H / tan40;  //  측정면에서 Top View Center 와 Side View Center 간 거리로서 기본 거리에서 H 상승에 따라 더 멀어진다.
                                            //  설계상 초기치는 1.85

                    //pos = (2.687 - FOV_YSHIFT - (mMarkNorth.Y + S) * sin40) / sin40;

                    mSNt = ( 2.687 - FOV_YSHIFT  - ( Nt + S ) * sin40 ) / sin40;  //  Side View 에서 N-S 마크의 +Y stroke Margin : Side View 상단 끝부터 N-S Top 까지의 ICS 에서의 거리를 WCS 에서의 거리로 환산

                    mSNb = ( (Nb + S) * sin40 - 0.7 ) / sin40;    //  Side View 에서 N-S 마크의 -Y stroke Margin : Side View 중심에서 N-S btm 으로부터 Invalid Region 까지의 거리를 WCS 에서의 거리로 환산

                    mSSt = ( (St - S) * sin40 - 0.7 ) / sin40;    //  Side View 에서 E 마크의 +Y stroke margin
                                                                //  마크중심(=Side View Center) 부터 E 마크 상단까지의 거리에서 Y Shift 량을 뺀 거리를 영상거리로 환산하고 거기에 Invalid Region 까지의 거리를 뺀 거리를 WCS 거리로 환산
                    mSSb = ( 2.687 + FOV_YSHIFT - ( Sb - S ) * sin40) / sin40;  //  Side View 에서 E 마크의 -Y stroke margin
                                                                                     //  Side View 하단 끝 부터 E 마크 하단까지의 거리에서 Y Shift 량을 뺀 거리를 영상거리로 환산하고 거기에 Invalid Region 까지의 거리를 뺀 거리를 WCS 거리로 환산
                    //  Oh : 측정면에서 Top View Center 와 Side View Center 간 거리
                    //  M  : Image Sensor 면에서 Side View Center 와 Top view center 간 거리 1.475mm

                    mTNb = 2.687 + FOV_YSHIFT - M - Oh + Nb + S;  //  Top View 에서 N-S 마크의 -Y stroke margin    
                    mTNt = M - 0.7 + Oh - Nt - S;                 //  Top View 에서 N-S 마크의 +Y stroke margin

                    array[0] = mSNt;
                    array[1] = mSNb;
                    array[2] = mSSt;
                    array[3] = mSSb;
                    array[4] = mTNb;
                    array[5] = mTNt;
                    Array.Sort(array);
                    if (array[0] > min)
                    {
                        min = array[0];
                        arrayRes[0] = mSNt;
                        arrayRes[1] = mSNb;
                        arrayRes[2] = mSSt;
                        arrayRes[3] = mSSb;
                        arrayRes[4] = mTNt;
                        arrayRes[5] = mTNb;
                        rS = S;
                        rH = H;
                        rOh = Oh;
                    }
                }
            }

            S = rS;
            H = rH;
            Oh = rOh;
            planeHeight = H;

            mSNt = arrayRes[0];
            mSNb = arrayRes[1];
            mSSt = arrayRes[2];
            mSSb = arrayRes[3];
            mTNt = arrayRes[4];
            mTNb = arrayRes[5];

            this.dataGridView2[1, 1].Value = W.ToString("F3");
            this.dataGridView2[1, 3].Value = H.ToString("F3");
            this.dataGridView2[1, 4].Value = Oh.ToString("F3");
            this.dataGridView2[1, 5].Value = rS.ToString("F3");
            //S = ((St - Nb ) / 2 - W ) * sin40;
            //Oh = (1 - sin40 / 2) * W - M + 3.237 / 2 + sin40 * (St - Nb) / 4 + (Nt + Nb) / 2;
            this.dataGridView3[1, 0].Value = arrayRes[0].ToString("F3");
            this.dataGridView3[1, 1].Value = arrayRes[1].ToString("F3");
            this.dataGridView3[1, 2].Value = arrayRes[2].ToString("F3");
            this.dataGridView3[1, 3].Value = arrayRes[3].ToString("F3");
            this.dataGridView3[1, 4].Value = arrayRes[4].ToString("F3");
            this.dataGridView3[1, 5].Value = arrayRes[5].ToString("F3");

            double[] SideviewPathOptimizer = new double[2] { 1.479, 1.726 };
            double[] FocusTranslationDist = new double[3] { 2.392, 3.078, 3.737 };

            int ri = 0;
            int rj = 0;
            double minOF = 9999;
            double lSOF = 0;
            double lNOF = 0;
            double rSOF = 0;
            double rNOF = 0;
            for (int i = 0; i < 2; i++)
            {
                double lSPO = SideviewPathOptimizer[i];
                for (int j = 0; j < 3; j++)
                {
                    lSOF = lSPO + H * (1 / sin40 - 1) - cos40 * (St + Sb + Nt + Nb) / 4;
                    lNOF = lSPO + H * (1 / sin40 - 1) + cos40 * (St + Sb + Nt + Nb) / 4 - FocusTranslationDist[j];
                    if (minOF > Math.Max(Math.Abs(lSOF), Math.Abs(lNOF)))
                    {
                        minOF = Math.Max(Math.Abs(lSOF), Math.Abs(lNOF));
                        ri = i;
                        rj = j;
                        rSOF = lSOF;
                        rNOF = lNOF;
                    }
                }
            }
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    lbSideViewPathOptimizer.SelectedIndex = ri;
                    lbFocusTranslatorWindow.SelectedIndex = rj;
                    tbSOF.Text = rSOF.ToString("F3");
                    tbNOF.Text = rNOF.ToString("F3");
                    labelMOTtype.Text = MOTtype[ri * 3 + rj];
                });

            }
            else
            {
                lbSideViewPathOptimizer.SelectedIndex = ri;
                lbFocusTranslatorWindow.SelectedIndex = rj;
                tbSOF.Text = rSOF.ToString("F3");
                tbNOF.Text = rNOF.ToString("F3");
                labelMOTtype.Text = MOTtype[ri * 3 + rj];
            }
            TransferDataToPositionGridView();
        }

        private void InitDataGridView()
        {
            int i = 0;

            this.dataGridView1.ColumnCount = 5;
            this.dataGridView1.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView1.ColumnCount; i++)
            {
                this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.BackgroundColor = Color.LightGray;
            this.dataGridView1.ScrollBars = ScrollBars.Vertical;


            // Column
            this.dataGridView1.Columns[0].Name = "Item";
            this.dataGridView1.Columns[1].Name = "Value";
            this.dataGridView1.Columns[2].Name = "Type";
            this.dataGridView1.Columns[3].Name = "unit";
            this.dataGridView1.Columns[4].Name = "Remark";
            for (i = 0; i < 5; i++)
                this.dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);


            this.dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView1.Columns[0].Width = 80;
            this.dataGridView1.Columns[1].Width = 50;
            this.dataGridView1.Columns[2].Width = 50;
            this.dataGridView1.Columns[3].Width = 40;
            this.dataGridView1.Columns[4].Width = 300;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            i = 0;
            this.dataGridView1.ReadOnly = false;

            this.dataGridView1.Rows.Add("Nt", "0", "input", "mm", "Top Y of North/Souht Mark w.r.t. Dummy Lens Center");
            this.dataGridView1[0, i].ReadOnly = true;
            this.dataGridView1[2, i].ReadOnly = true;
            this.dataGridView1[3, i].ReadOnly = true;
            this.dataGridView1[4, i].ReadOnly = true;
            i++;
            this.dataGridView1.Rows.Add("Nb", "0", "input", "mm", "Btm Y of North/Souht Mark w.r.t. Dummy Lens Center");
            this.dataGridView1[0, i].ReadOnly = true;
            this.dataGridView1[2, i].ReadOnly = true;
            this.dataGridView1[3, i].ReadOnly = true;
            this.dataGridView1[4, i].ReadOnly = true;
            i++;
            this.dataGridView1.Rows.Add("St", "0", "input", "mm", "Top Y of East Mark w.r.t. Dummy Lens Center");
            this.dataGridView1[0, i].ReadOnly = true;
            this.dataGridView1[2, i].ReadOnly = true;
            this.dataGridView1[3, i].ReadOnly = true;
            this.dataGridView1[4, i].ReadOnly = true;
            i++;
            this.dataGridView1.Rows.Add("Sb", "0", "input", "mm", "Btm Y of East Mark w.r.t. Dummy Lens Center");
            this.dataGridView1[0, i].ReadOnly = true;
            this.dataGridView1[2, i].ReadOnly = true;
            this.dataGridView1[3, i].ReadOnly = true;
            this.dataGridView1[4, i].ReadOnly = true;
            i++;

            this.dataGridView1.Rows.Add("", "", "", "", "");

            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.ColumnHeadersHeight = 22;
            effRowNum = i;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView1.Rows[i].Height = 16;
                this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView1[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }
            this.dataGridView1.Rows[i].Height = 16;
            this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            //for (int colum = 3; colum < this.dataGridView1.ColumnCount - 1; colum++)
            //{
            //    for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
            //    {
            //        this.dataGridView1[colum, row].Style.BackColor = Color.LightGray;
            //        this.dataGridView1.ReadOnly = true;
            //    }
            //}
        }

        public void ExternalSetData(double paramNt, double paramNb, double paramSt, double paramSb, double paramM = 1.185, double paramO = 2.696)
        {
            dataGridView1[1, 0].Value = paramNt.ToString("F3");
            dataGridView1[1, 1].Value = paramNb.ToString("F3");
            dataGridView1[1, 2].Value = paramSt.ToString("F3");
            dataGridView1[1, 3].Value = paramSb.ToString("F3");

            dataGridView2[1, 0].Value = paramM.ToString("F3");
            dataGridView2[1, 2].Value = paramO.ToString("F3");

        }

        PointF mMarkNorth = new PointF();
        PointF mMarkSouth = new PointF();
        PointF mMarkEast = new PointF();

        public void ExternalSetData(PointF pNorth, PointF pNsize, PointF pSouth, PointF pSsize, PointF pEast, PointF pEsize, double paramM = 1.475, double paramO = 1.95)
        {
            mMarkNorth = new PointF(pNorth.X, pNorth.Y);
            mMarkSouth = new PointF(pSouth.X, pSouth.Y);
            mMarkEast = new PointF(pEast.X, pEast.Y);

            double lNt = pNorth.Y + pNsize.Y / 2;   //  Top
            double lNb = pNorth.Y - pNsize.Y / 2;   //  Bottom
            double lSt = 0;
            double lSb = 0;

            if (pEast.Y > 0)
            {
                lSt = -pEast.Y + pEsize.Y / 2;
                lSb = -pEast.Y - pEsize.Y / 2;
            }
            else
            {
                lSt = pEast.Y + pEsize.Y / 2;
                lSb = pEast.Y - pEsize.Y / 2;
            }

            dataGridView1[1, 0].Value = lNt.ToString("F3");
            dataGridView1[1, 1].Value = lNb.ToString("F3");
            dataGridView1[1, 2].Value = lSt.ToString("F3");
            dataGridView1[1, 3].Value = lSb.ToString("F3");

            dataGridView2[1, 0].Value = paramM.ToString("F3");
            dataGridView2[1, 2].Value = paramO.ToString("F3");

            dataGridView4[1, 0].Value = pNorth.X.ToString("F3");
            if (pSouth.X < 0)
                dataGridView4[1, 1].Value = pSouth.X.ToString("F3");
            else
                dataGridView4[1, 1].Value = (-pSouth.X).ToString("F3");

            dataGridView4[1, 2].Value = pEast.X.ToString("F3");
        }

        private void InitDataGridView2()
        {
            int i = 0;

            this.dataGridView2.ColumnCount = 5;
            this.dataGridView2.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView2.ColumnCount; i++)
            {
                this.dataGridView2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView2.RowHeadersVisible = false;
            this.dataGridView2.BackgroundColor = Color.LightGray;
            this.dataGridView2.ScrollBars = ScrollBars.Vertical;


            // Column
            this.dataGridView2.Columns[0].Name = "Item";
            this.dataGridView2.Columns[1].Name = "Value";
            this.dataGridView2.Columns[2].Name = "Type";
            this.dataGridView2.Columns[3].Name = "unit";
            this.dataGridView2.Columns[4].Name = "Remark";
            this.dataGridView2.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            this.dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dataGridView2.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            for (i = 0; i < 5; i++)
                this.dataGridView2.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);


            this.dataGridView2.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView2.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView2.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView2.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView2.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView2.Columns[0].Width = 80;
            this.dataGridView2.Columns[1].Width = 50;
            this.dataGridView2.Columns[2].Width = 50;
            this.dataGridView2.Columns[3].Width = 40;
            this.dataGridView2.Columns[4].Width = 300;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            i = 0;
            this.dataGridView2.ReadOnly = false;

            this.dataGridView2.Rows.Add("M", "1.185", "const", "mm", "Dist. from Side View Center to Top view center on Image Plane");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = true;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;
            this.dataGridView2.Rows.Add("W", "0", "result", "mm", "Dist. from Side View Center to Actuator Center, + means Top View Center Direction.");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = false;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;
            this.dataGridView2.Rows.Add("O", "2.696", "const", "mm", "Standard Top View Offset from Side View Center.");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = true;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;
            this.dataGridView2.Rows.Add("H", "0", "input", "mm", "Moving Dist downward");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = false;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;
            this.dataGridView2.Rows.Add("Oh", "2.696", "result", "mm", "Target Top View Offset from Side View Center.");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = true;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;
            this.dataGridView2.Rows.Add("S", "0", "input", "mm", "Focus Translation Window Edge Offset, + means lower right.");
            this.dataGridView2[0, i].ReadOnly = true;
            this.dataGridView2[1, i].ReadOnly = false;
            this.dataGridView2[2, i].ReadOnly = true;
            this.dataGridView2[3, i].ReadOnly = true;
            this.dataGridView2[4, i].ReadOnly = true;
            i++;

            this.dataGridView2.Rows.Add("", "", "", "", "");

            this.dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView2.ColumnHeadersHeight = 22;
            effRowNum = i;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView2.Rows[i].Height = 16;
                this.dataGridView2.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView2[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView2[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView2[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
                if (i == 3 || i == 5)
                    this.dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.White;
                else
                    this.dataGridView2.Rows[i].DefaultCellStyle.BackColor = Color.LightSteelBlue;
            }
            this.dataGridView2.Rows[i].Height = 16;
            this.dataGridView2.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView2.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            //for (int colum = 3; colum < this.dataGridView2.ColumnCount - 1; colum++)
            //{
            //    for (int row = 0; row < this.dataGridView2.Rows.Count; row++)
            //    {
            //        this.dataGridView2[colum, row].Style.BackColor = Color.LightGray;
            //        this.dataGridView2.ReadOnly = true;
            //    }
            //}
        }
        private void InitDataGridView3()
        {
            int i = 0;

            this.dataGridView3.ColumnCount = 5;
            this.dataGridView3.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView3.ColumnCount; i++)
            {
                this.dataGridView3.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView3.RowHeadersVisible = false;
            this.dataGridView3.BackgroundColor = Color.LightGray;
            this.dataGridView3.ScrollBars = ScrollBars.Vertical;


            // Column
            this.dataGridView3.Columns[0].Name = "Item";
            this.dataGridView3.Columns[1].Name = "Value";
            this.dataGridView3.Columns[2].Name = "Type";
            this.dataGridView3.Columns[3].Name = "unit";
            this.dataGridView3.Columns[4].Name = "Remark";
            for (i = 0; i < 5; i++)
                this.dataGridView3.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);


            this.dataGridView3.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView3.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView3.Columns[0].Width = 80;
            this.dataGridView3.Columns[1].Width = 50;
            this.dataGridView3.Columns[2].Width = 50;
            this.dataGridView3.Columns[3].Width = 40;
            this.dataGridView3.Columns[4].Width = 300;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            i = 0;
            this.dataGridView3.ReadOnly = true;

            this.dataGridView3.Rows.Add("mSideNt", "0", "result", "mm", "Margin to Top Y of North/South Mark on Side View");
            this.dataGridView3[1, i++].ReadOnly = true;
            this.dataGridView3.Rows.Add("mSideNb", "0", "result", "mm", "Margin to Btm Y of North/South Mark on Side View");
            this.dataGridView3[1, i++].ReadOnly = true;
            this.dataGridView3.Rows.Add("mSideSt", "0", "result", "mm", "Margin to Top Y of East Mark on Side View");
            this.dataGridView3[1, i++].ReadOnly = true;
            this.dataGridView3.Rows.Add("mSideSb", "0", "result", "mm", "Margin to Btm Y of East Mark on Side View");
            this.dataGridView3[1, i++].ReadOnly = true;
            this.dataGridView3.Rows.Add("mTopNt", "0", "result", "mm", "Margin to Top Y of North/South Mark on Top View");
            this.dataGridView3[1, i++].ReadOnly = true;
            this.dataGridView3.Rows.Add("mTopNb", "0", "result", "mm", "Margin to Btm Y of North/South Mark on Top View");
            this.dataGridView3[1, i++].ReadOnly = true;

            this.dataGridView3.Rows.Add("", "", "", "", "");

            this.dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView3.ColumnHeadersHeight = 22;
            effRowNum = i;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView3.Rows[i].Height = 16;
                this.dataGridView3.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView3.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView3[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView3[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView3[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }
            this.dataGridView3.Rows[i].Height = 16;
            this.dataGridView3.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView3.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView3.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            //for (int colum = 3; colum < this.dataGridView3.ColumnCount - 1; colum++)
            //{
            //    for (int row = 0; row < this.dataGridView3.Rows.Count; row++)
            //    {
            //        this.dataGridView3[colum, row].Style.BackColor = Color.LightGray;
            //        this.dataGridView3.ReadOnly = true;
            //    }
            //}
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lbSideViewPathOptimizer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lbFocusTranslatorWindow_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnApplyUserSet_Click(object sender, EventArgs e)
        {
            double min = 0;
            double H = 0;
            double[] array = new double[6];
            double[] arrayRes = new double[6];
            double rS = 0;
            double rH = 0;
            double rOh = 0;

            try
            {
                Nt = double.Parse(this.dataGridView1[1, 0].Value.ToString());
                Nb = double.Parse(this.dataGridView1[1, 1].Value.ToString());
                St = double.Parse(this.dataGridView1[1, 2].Value.ToString());
                Sb = double.Parse(this.dataGridView1[1, 3].Value.ToString());

                M = double.Parse(this.dataGridView2[1, 0].Value.ToString());
                O = double.Parse(this.dataGridView2[1, 2].Value.ToString());
                H = double.Parse(this.dataGridView2[1, 3].Value.ToString());
                S = double.Parse(this.dataGridView2[1, 5].Value.ToString());
            }
            catch
            {
                MessageBox.Show("Input Data is not crrect.");
                return;
            }

            W = (Sb - Nt) / 2;
            Oh = O - H / tan40;

            mSNt = (2.687 - (Nt + W) * sin40) / sin40;
            mSNb = ((Nb + W) * sin40 - (0.55 - S)) / sin40;
            mSSb = (2.687 - (Sb - W) * sin40) / sin40;
            mSSt = ((St - W) * sin40 - (0.55 + S)) / sin40;
            mTNb = 2.687 - (Oh - W - Nb) - M;
            mTNt = Oh - W - Nt + M - (0.55 + S);

            arrayRes[0] = mSNt;
            arrayRes[1] = mSNb;
            arrayRes[2] = mSSt;
            arrayRes[3] = mSSb;
            arrayRes[4] = mTNb;
            arrayRes[5] = mTNt;

            this.dataGridView2[1, 1].Value = W.ToString("F3");
            this.dataGridView2[1, 4].Value = Oh.ToString("F3");
            //S = ((St - Nb ) / 2 - W ) * sin40;
            //Oh = (1 - sin40 / 2) * W - M + 3.237 / 2 + sin40 * (St - Nb) / 4 + (Nt + Nb) / 2;
            this.dataGridView3[1, 0].Value = arrayRes[0].ToString("F3");
            this.dataGridView3[1, 1].Value = arrayRes[1].ToString("F3");
            this.dataGridView3[1, 2].Value = arrayRes[2].ToString("F3");
            this.dataGridView3[1, 3].Value = arrayRes[3].ToString("F3");
            this.dataGridView3[1, 4].Value = arrayRes[4].ToString("F3");
            this.dataGridView3[1, 5].Value = arrayRes[5].ToString("F3");

            double[] SideviewPathOptimizer = new double[2] { 1.479, 1.726 };
            double[] FocusTranslationDist = new double[3] { 2.392, 3.078, 3.737 };

            int ri = 0;
            int rj = 0;
            double minOF = 9999;
            double lSOF = 0;
            double lNOF = 0;
            double rSOF = 0;
            double rNOF = 0;
            for (int i = 0; i < 2; i++)
            {
                double lSPO = SideviewPathOptimizer[i];
                for (int j = 0; j < 3; j++)
                {
                    lSOF = lSPO + H * (1 / sin40 - 1) - cos40 * (St + Sb + Nt + Nb) / 4;
                    lNOF = lSPO + H * (1 / sin40 - 1) + cos40 * (St + Sb + Nt + Nb) / 4 - FocusTranslationDist[j];
                    if (minOF > Math.Max(Math.Abs(lSOF), Math.Abs(lNOF)))
                    {
                        minOF = Math.Max(Math.Abs(lSOF), Math.Abs(lNOF));
                        ri = i;
                        rj = j;
                        rSOF = lSOF;
                        rNOF = lNOF;
                    }
                }
            }
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    lbSideViewPathOptimizer.SelectedIndex = ri;
                    lbFocusTranslatorWindow.SelectedIndex = rj;
                    tbSOF.Text = rSOF.ToString("F3");
                    tbNOF.Text = rNOF.ToString("F3");
                    labelMOTtype.Text = MOTtype[ri * 3 + rj];
                });

            }
            else
            {
                lbSideViewPathOptimizer.SelectedIndex = ri;
                lbFocusTranslatorWindow.SelectedIndex = rj;
                tbSOF.Text = rSOF.ToString("F3");
                tbNOF.Text = rNOF.ToString("F3");
                labelMOTtype.Text = MOTtype[ri * 3 + rj];
            }
            TransferDataToPositionGridView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double H = 0;

            M = double.Parse(this.dataGridView2[1, 0].Value.ToString());
            O = double.Parse(this.dataGridView2[1, 2].Value.ToString());
            H = double.Parse(this.dataGridView2[1, 3].Value.ToString());
            S = double.Parse(this.dataGridView2[1, 5].Value.ToString());
            Oh = O - H / tan40;

            double[] SideviewPathOptimizer = new double[2] { 1.479, 1.726 };
            double[] FocusTranslationDist = new double[3] { 2.392, 3.078, 3.737 };

            double lSOF = 0;
            double lNOF = 0;
            double lSPO = SideviewPathOptimizer[lbSideViewPathOptimizer.SelectedIndex];
            lSOF = lSPO + H * (1 / sin40 - 1) - cos40 * (Sb + St + Nt + Nb) / 4;
            lNOF = lSPO + H * (1 / sin40 - 1) + cos40 * (Sb + St + Nt + Nb) / 4 - FocusTranslationDist[lbFocusTranslatorWindow.SelectedIndex];

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbSOF.Text = lSOF.ToString("F3");
                    tbNOF.Text = lNOF.ToString("F3");
                    labelMOTtype.Text = MOTtype[lbSideViewPathOptimizer.SelectedIndex * 3 + lbFocusTranslatorWindow.SelectedIndex];
                });

            }
            else
            {
                tbSOF.Text = lSOF.ToString("F3");
                tbNOF.Text = lNOF.ToString("F3");
                labelMOTtype.Text = MOTtype[lbSideViewPathOptimizer.SelectedIndex * 3 + lbFocusTranslatorWindow.SelectedIndex];
            }
        }

        private void InitDataGridView4()
        {
            int i = 0;

            this.dataGridView4.ColumnCount = 5;
            this.dataGridView4.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView4.ColumnCount; i++)
            {
                this.dataGridView4.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView4.RowHeadersVisible = false;
            this.dataGridView4.BackgroundColor = Color.LightGray;
            this.dataGridView4.ScrollBars = ScrollBars.Vertical;


            // Column
            this.dataGridView4.Columns[0].Name = "Mark";
            this.dataGridView4.Columns[1].Name = "X (mm)";
            this.dataGridView4.Columns[2].Name = "Y (mm)";
            this.dataGridView4.Columns[3].Name = "X pix";
            this.dataGridView4.Columns[4].Name = "Y pix";
            for (i = 0; i < 5; i++)
                this.dataGridView4.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);


            this.dataGridView4.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView4.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView4.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView4.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView4.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView4.Columns[0].Width = 180;
            this.dataGridView4.Columns[1].Width = 60;
            this.dataGridView4.Columns[2].Width = 60;
            this.dataGridView4.Columns[3].Width = 60;
            this.dataGridView4.Columns[4].Width = 60;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            i = 0;
            this.dataGridView4.ReadOnly = false;

            this.dataGridView4.Rows.Add("Left Mark on Side View", "0", "0", "0", "0");
            this.dataGridView4[0, i].ReadOnly = true;
            this.dataGridView4[2, i].ReadOnly = true;
            this.dataGridView4[3, i].ReadOnly = true;
            this.dataGridView4[4, i].ReadOnly = true;
            i++;
            this.dataGridView4.Rows.Add("Right Mark on Side View", "0", "0", "0", "0");
            this.dataGridView4[0, i].ReadOnly = true;
            this.dataGridView4[2, i].ReadOnly = true;
            this.dataGridView4[3, i].ReadOnly = true;
            this.dataGridView4[4, i].ReadOnly = true;
            i++;
            this.dataGridView4.Rows.Add("Bottom Mark on Side View", "0", "0", "0", "0");
            this.dataGridView4[0, i].ReadOnly = true;
            this.dataGridView4[2, i].ReadOnly = true;
            this.dataGridView4[3, i].ReadOnly = true;
            this.dataGridView4[4, i].ReadOnly = true;
            i++;
            this.dataGridView4.Rows.Add("Left Mark on Top View", "0", "0", "0", "0");
            this.dataGridView4[0, i].ReadOnly = true;
            this.dataGridView4[1, i].ReadOnly = true;
            this.dataGridView4[2, i].ReadOnly = true;
            this.dataGridView4[3, i].ReadOnly = true;
            this.dataGridView4[4, i].ReadOnly = true;
            this.dataGridView4[1, i].Style.BackColor = Color.BlanchedAlmond;
            i++;
            this.dataGridView4.Rows.Add("Right Mark on Top View", "0", "0", "0", "0");
            this.dataGridView4[0, i].ReadOnly = true;
            this.dataGridView4[1, i].ReadOnly = true;
            this.dataGridView4[2, i].ReadOnly = true;
            this.dataGridView4[3, i].ReadOnly = true;
            this.dataGridView4[4, i].ReadOnly = true;
            this.dataGridView4[1, i].Style.BackColor = Color.BlanchedAlmond;
            i++;

            this.dataGridView4.Rows.Add("", "", "", "", "");

            this.dataGridView4.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView4.ColumnHeadersHeight = 22;
            effRowNum = i;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView4.Rows[i].Height = 16;
                this.dataGridView4.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView4.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView4[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView4[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView4[3, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
                this.dataGridView4[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
                this.dataGridView4[2, i].Style.BackColor = Color.BlanchedAlmond;
                this.dataGridView4[3, i].Style.BackColor = Color.BlanchedAlmond;
                this.dataGridView4[4, i].Style.BackColor = Color.BlanchedAlmond;
            }
            this.dataGridView4.Rows[i].Height = 16;
            this.dataGridView4.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView4.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView4.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            //for (int colum = 3; colum < this.dataGridView4.ColumnCount - 1; colum++)
            //{
            //    for (int row = 0; row < this.dataGridView4.Rows.Count; row++)
            //    {
            //        this.dataGridView4[colum, row].Style.BackColor = Color.LightGray;
            //        this.dataGridView4.ReadOnly = true;
            //    }
            //}
        }
        public void TransferDataToPositionGridView()
        {
            double pos = 0;

            //Rows[0] : "Left Mark on Side View" 
            //Rows[1] : "Right Mark on Side View"
            //Rows[2] : "Bottom Mark on Side View"
            //Rows[3] : "Left Mark on Top View" "
            //Rows[4] : "Right Mark on Top View" 

            //  mSNt :  Side View 에서 N-S 마크의 +Y stroke Margin : Side View 상단 끝부터 N-S Top 까지의 ICS 에서의 거리를 WCS 에서의 거리로 환산
            //  mSNb :  Side View 에서 N-S 마크의 -Y stroke Margin : Side View 중심에서 N-S btm 으로부터 Invalid Region 까지의 거리를 WCS 에서의 거리로 환산
            //  mSSt :  Side View 에서 E 마크의 +Y stroke margin
            //  mSSb :  Side View 에서 E 마크의 -Y stroke margin
            //  mTNb :  Top View 에서 N-S 마크의 -Y stroke margin    
            //  mTNt :  Top View 에서 N-S 마크의 +Y stroke margin

            //  Stroke + (N-S 마크 상단 N-S 마크 하단간 거리의 절반) = 측정 가능 Ymax 에서 N-S 마크의 중심까지 거리
            pos =  (2.687 - FOV_YSHIFT - (mMarkNorth.Y + S) * sin40);
            this.dataGridView4[2, 0].Value = pos.ToString("F3");    //  거리
            this.dataGridView4[4, 0].Value = (pos * sin40 / (0.0055 / LensMag)).ToString("F1");    //  pixel

            pos = (2.687 - FOV_YSHIFT - (mMarkSouth.Y + S) * sin40);
            this.dataGridView4[2, 1].Value = pos.ToString("F3");
            this.dataGridView4[4, 1].Value = (pos * sin40 / (0.0055 / LensMag)).ToString("F1");

            pos = (2.687 - FOV_YSHIFT + ( mMarkEast.Y - S ) * sin40);
            this.dataGridView4[2, 2].Value = pos.ToString("F3");
            this.dataGridView4[4, 2].Value = (pos * sin40 / (0.0055 / LensMag)).ToString("F1");

            pos = 2.687 - FOV_YSHIFT + M + Oh - mMarkNorth.Y - S;
            this.dataGridView4[2, 3].Value = pos.ToString("F3");
            this.dataGridView4[4, 3].Value = (pos / (0.0055 / LensMag)).ToString("F1");

            pos = 2.687 - FOV_YSHIFT + M + Oh - mMarkSouth.Y - S;
            this.dataGridView4[2, 4].Value = pos.ToString("F3");
            this.dataGridView4[4, 4].Value = (pos / (0.0055 / LensMag)).ToString("F1");

            //   모델 저장 시 다음 검토 필요
            //   M 은 광학계 설계치에 따른 상수이므로 저장 불필요
            //   Oh : 모델에 따라 최적화 된 값이므로 저장 필요
            //   S  : 모델에 따라 최적화 된 값이므로 저장 필요
            //   planeHeight : 모델에 따라 최적화 된 값이므로 저장 필요, 그런데 Oh = 1.95 + planeHeight / tan40 로써 planeHeight 를 저장하면 Oh 는 저장 불필요
            //
            //   planeHeight,S 를 저장하고, Oh = 1.95 + planeHeight / tan40 로 계산한다.
            //   planeHeight : -0.3 ~ +0.3, S : -0.5 ~ 0.5 이므로 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double pos = 0;

            try
            {
                pos = double.Parse(this.dataGridView4[1, 0].Value.ToString());
                this.dataGridView4[1, 3].Value = pos.ToString("F3");
                this.dataGridView4[3, 0].Value = (320 + pos / (0.0055 / LensMag)).ToString("F1");
                this.dataGridView4[3, 3].Value = (320 + pos / (0.0055 / LensMag)).ToString("F1");

                pos = double.Parse(this.dataGridView4[1, 1].Value.ToString());
                this.dataGridView4[1, 4].Value = pos.ToString("F3");
                this.dataGridView4[3, 1].Value = (320 + pos / (0.0055 / LensMag)).ToString("F1");
                this.dataGridView4[3, 4].Value = (320 + pos / (0.0055 / LensMag)).ToString("F1");

                pos = double.Parse(this.dataGridView4[1, 2].Value.ToString());
                this.dataGridView4[3, 2].Value = (320 + pos / (0.0055 / LensMag)).ToString("F1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void MOTSimDlg_Load(object sender, EventArgs e)
        {
            this.Size = new Size(960, 686);
            this.Location = new Point(950, 335);
        }

        public bool mbConfirmed = false;
        private void button3_Click(object sender, EventArgs e)
        {
            mbConfirmed = true;
            this.Hide();
        }

        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            //if (lbtn.TabIndex == 911)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.AddP;
            //}
            //else if (lbtn.TabIndex == 912)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.SubP;
            //}
            //else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnCP;
            //else
            lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            //if (lbtn.TabIndex == 911)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.AddP;
            //}
            //else if (lbtn.TabIndex == 912)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.SubP;
            //}
            //else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnCP;
            //else
            lbtn.BackgroundImage = Properties.Resources.BtnKP;

        }
        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            //if (lbtn.TabIndex == 911)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.AddN;
            //}
            //else if (lbtn.TabIndex == 912)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.SubN;
            //}
            //else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnCN;
            //else
            lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            //if (lbtn.TabIndex == 911)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.AddN;
            //}
            //else if (lbtn.TabIndex == 912)
            //{
            //    lbtn.BackgroundImage = Properties.Resources.SubN;
            //}
            //else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnCN;
            //else
            lbtn.BackgroundImage = Properties.Resources.BtnKN;

        }
    }
}
