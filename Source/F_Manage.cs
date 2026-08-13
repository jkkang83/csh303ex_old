using System;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using static CSH030Ex.F_Main;
using OpenCvSharp.Flann;
using S2System.Vision;
using Dln;
using static MotorizedStage_SK_PI.F_Motion_SK_PI;

namespace CSH030Ex
{
    public partial class FManage : Form
    {

        public delegate void LoadData(int type, string data); //rcp : 0, spec : 1
        public event LoadData On_LoadData;
        public delegate void SaveData(int type, string data); //rcp : 0, spec : 1
        public event SaveData On_SaveData;
        public delegate void SaveCBState(string data);
        public event SaveCBState On_Save_CBstate;
        public Global m__G = null;

        public F_Main MyOwner = null;
        PassFailInfromation mPWdialog = null;
        public Button[] mInfoBtn = null;
        private TextBox[] mViewLog = null;
        private Chart[] mychart = null;


        private string[] mChartTitle = new string[10] { "", "", "", "", "", "", "", "", "", "", };
        double[] mFailRatio = new double[10];
        string[] mFailItem = new string[10] { "", "", "", "", "", "", "", "", "", "", };
        bool[] m_FlagChart = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        bool[] m_bViewLogLarge = new bool[4] { false, false, false, false };

        double[] mChart_xMax = new double[4];
        double[] mChart_xMin = new double[4];
        double[] mChart_yMax = new double[4];
        double[] mChart_yMin = new double[4];
        double[] mChart_y2Max = new double[4];
        double[] mChart_y2Min = new double[4];
        double[] mPlotYMax = new double[4];
        double[] mPlotYGrid = new double[4];
        double[] mPlotY2Max = new double[4];
        double[] mPlotY2Grid = new double[4];

        public int[] moisY_Count = new int[4];
        public int[] moisY_BWD_Delay = new int[4];

        //public double[] mTime = new double[10000];
        public double[][] mStroke = new double[6][];
        public string[] mStrStroke = new string[6] { "X", "Y", "Z", "TX", "TY", "TZ"};
        //public double[] mXstroke = new double[10000];     //  Code Output Time
        //public double[] mYstroke = new double[10000];     //  Code Output Time
        //public double[] mZstroke = new double[10000];     //  Code Output Time
        //public double[] mTXstroke = new double[10000];     //  Code Output Time
        //public double[] mTYstroke = new double[10000];     //  Code Output Time
        //public double[] mTZstroke = new double[10000];     //  Code Output Time

        public string[] mStrAzimuth = new string[12] { "N1", "N2", "W1", "W2", "S1", "S2", "E1", "E2", "topN1", "topN2", "topS1", "topS2" };
        public OpenCvSharp.Point2d[][] mPosAzimuth = new OpenCvSharp.Point2d[12][];
        //public Point[] mtN1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtN2 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtW1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtW2 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtS1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtS2 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtE1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] mtE2 = new Point[10000];     //  1 -> 0.1um

        //public Point[] msN1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] msN2 = new Point[10000];     //  1 -> 0.1um
        //public Point[] msS1 = new Point[10000];     //  1 -> 0.1um
        //public Point[] msS2 = new Point[10000];     //  1 -> 0.1um

        public int mCommonDataCount = 0;

        public int[] m_olderrsum = new int[4];
        public int[] m_errsum = new int[4];

        public int[] mOldHistoryIndex = new int[4];
        public int[] mTestFinishState = new int[4] { 0, 0, 0, 0 };
        //public bool[,] mConsecutiveFail = new bool[4, 10];

        bool[] m_bChartFront = new bool[10] { false, false, false, false, false, false, false, false, false, false };
        bool[] m_bChartWheel = new bool[10] { false, false, false, false, false, false, false, false, false, false };

        //double mChartY2OrgYGrid = 0.2;
        //double mChartX2OrgYGrid = 0.2;
        bool mbMagTable = false;
        bool mRepeatLuL = false;
        public bool[] mProcessMonitorEscaped = new bool[2] { false, false };

        public int m_LastSampleNumber = 1;
        //double mChartY3OrgYGrid = 0.1;
        //double mChartX3OrgYGrid = 0.1;

        Rectangle[] mChartArea = new Rectangle[10];

        public string[] mSplStateText = new string[4] { " ", "Fail", "Pass", "Fail" };

        Complex cR = new Complex(1, 0);
        Complex cI = new Complex(0, 1);
        Complex cX = new Complex(0, 0);
        #region 자세차 검사기 통신 변수
        int CLIENT_PORT_NO = 5001;
        int SERVER_PORT_NO = 5000;
        IPAddress[] MY_IP;
        //Thread ThreadSever;
        //bool IsRun = true;
        //bool IsClosed = false;
        //bool bRunning = false;
        public bool m_bClientAvailable = false;
        #endregion
        public Network Network = new Network();

        public bool mbEulerMeasureApply = false;
        public bool mbEulerSimpleApply = false;

        public FManage()
        {
            InitializeComponent();

            mPWdialog = new PassFailInfromation();
            mPWdialog.MyOwner = MyOwner;

            for (int i = 0; i < 6; i++)
                mStroke[i] = new double[10000];

            for (int i = 0; i < 12; i++)
                mPosAzimuth[i] = new OpenCvSharp.Point2d[10000];
            string strFileName = "C:\\CSHTest\\DoNotTouch\\PortNumber.txt";
            if (!File.Exists(strFileName))
            {
                StreamWriter wr = new StreamWriter(strFileName);
                wr.WriteLine("5000");
                wr.Close();
            }

            StreamReader sr = new StreamReader(strFileName);
            string temp = sr.ReadLine();
            sr.Close();
            ServerPort.Text = temp;
            Network.StartServer(int.Parse(temp));
            Network.RecieveEvened += Network_RecieveEvened;
            Network.LogEvened += Network_LogEvened;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        bool anyAlive = false;
                        List<TcpClient> clients = Network.GetConnectedClients();

                        foreach (TcpClient client in clients)
                        {
                            if (Network.IsClientAlive(client))
                            {
                                anyAlive = true;
                                break;
                            }
                        }

                        if (InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                lblNetwork.BackColor = anyAlive ? Color.Blue : Color.Red;
                            });
                        }
                    }
                    catch
                    {
                    }

                    Thread.Sleep(1000); // 1초 주기 변경 추천
                }
            });
        }

        private void Network_LogEvened(object sender, string e)
        {
            AddViewLog(string.Format("{0}\r\n",e));
        }

        public byte[] sDataBuff = null;
        public int RunNum = 1;

        private void Network_RecieveEvened(object sender, byte[] e)
        {
            string[] arry = Encoding.ASCII.GetString(e).Split('@');
            string cmd = arry[0];
            int frmCnt = 0;
            int index = 0;
            byte[] sCmdBuf = null;
            byte[] sRnBuf = null;
            byte[] sendBuf = null;

            int lfrmCnt = 0;
            switch (cmd)
            {
                case "P_S":
                    if(arry.Length > 2) lfrmCnt = int.Parse(arry[1]);
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (!m__G.m_bHideAllGraph)
                            {
                                AddViewLog(string.Format("P_S Recieve, Data Length :{0}\r\n", lfrmCnt));
                            }
                            if (lfrmCnt == 17)
                            {
                                MyOwner.SetPseudoOMM(true);
                            }
                            else
                            {
                                MyOwner.SetPseudoOMM(false);
                            }
                            UpdateCheckStates();
                        });

                    }
                    else
                    {
                        if (!m__G.m_bHideAllGraph)
                        {
                            AddViewLog(string.Format("P_S Recieve, Data Length :{0}\r\n", lfrmCnt));
                        }
                        if (lfrmCnt == 17)
                        {
                            MyOwner.SetPseudoOMM(true);
                        }
                        else
                        {
                            MyOwner.SetPseudoOMM(false);
                        }
                        UpdateCheckStates();
                    }

                    break;
                case "R_S": //Request Inspection
                            //Thread.Sleep(10);
                    m__G.mDoingStatus = "Triggered Measure";
                    lfrmCnt = int.Parse(arry[1]);
                    if (!m__G.m_bHideAllGraph) 
                        AddViewLog(string.Format("[{0}] - R_S Recieve, trg :{1}\r\n", RunNum++, lfrmCnt));

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
                            //Task.Run(() => StartTriggerTest(frmCnt));
                            Task.Run(() => StartTriggerTestWrapper(lfrmCnt));
                        });

                    }
                    else
                    {
                        m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
                        //Task.Run(() => StartTriggerTest(frmCnt));
                        Task.Run(() => StartTriggerTestWrapper(lfrmCnt));
                    }

                    break;
                case "R_C": //Request Continuous Inspection
                    Thread.Sleep(1);
                    frmCnt = int.Parse(arry[1]);
                    if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("[{0}] - R_C Recieve, trg :{1}\r\n", RunNum++, frmCnt));
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            Task.Run(() => StartContinuousTest(frmCnt));
                        });

                    }
                    else
                        Task.Run(() => StartContinuousTest(frmCnt));
                    break;
                case "R_D": //Remote Manual Test 
                    //if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("R_D Recieve==\r\n"));
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            Task.Run(() => AnosisRemoteData());
                            //AnosisRemoteData();
                        });

                    }
                    else
                    {
                        Task.Run(() => AnosisRemoteData());
                        //AnosisRemoteData();
                    }
                    break;
                case "R_Z": // Reset Position

                    if (arry.Length >= 3)
                    {
                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0} Recieve,\r\n", cmd)));
                        string[] splitData = arry[2].Split(',');
                        double[] orgMaster = new double[6];

                        for (int i = 0; i < 6; i++)
                        {
                            if (i < splitData.Length)
                            {
                                double.TryParse(splitData[i], out orgMaster[i]);
                            }
                        }
                        // 현재 SetSignTXTY의 부호 고려. SignTX가 - 일때 사용자가 tbMasterTX.Text에 n을 입력하면 singn 초기화 값 기준인 orgMasterX -n으로 적용해야함.
                        if (m__G.m_bXTiltReverse)
                        {
                            orgMaster[3] *= -1;
                        }

                        if (m__G.m_bYTiltReverse)
                        {
                            orgMaster[4] *= -1;
                        }

                        // offset, sign 초기화
                        m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
                        m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);


                        int cntData = m__G.fVision.mCalibrationFullData.Count;

                        // 1. 타임아웃 설정을 위한 변수 추가 (예: 5초)
                        int timeoutMs = 3000;
                        int elapsedMs = 0;
                        int sleepStep = 200;

                        // 2. 조건문에 타임아웃 체크 추가
                        while (m__G.fVision.mCalibrationFullData.Count <= cntData)
                        {
                            if (elapsedMs >= timeoutMs)
                            {
                                AddViewLog("[WARN] R_Z Vision Data Update Timeout! Simulation mode check required.\r\n");
                                break; // 루프 탈출
                            }

                            Thread.Sleep(sleepStep);
                            elapsedMs += sleepStep;
                        }
                        if (m__G.fVision.mCalibrationFullData.Count > cntData)
                        {
                            int lastIdx = m__G.fVision.mCalibrationFullData.Count - 1;
                            double[] lastData = m__G.fVision.mCalibrationFullData[lastIdx];

                            double lx = lastData[0];
                            double ly = lastData[1];
                            double lz = lastData[2];
                            double ltx = lastData[3];
                            double lty = lastData[4];
                            double ltz = lastData[5];

                            ////  ltx, lty, ltz 는 측정값 (min)
                            ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)
                            ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
                            m__G.fVision.SaveTXTYZeroOffset(lx, ly, lz, ltx, lty, ltz, orgMaster[0], orgMaster[1], orgMaster[2], orgMaster[3], orgMaster[4], orgMaster[5], true);
                            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);
                        }

                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("Set Zero {0:0.000}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}===,\r\n",
          orgMaster[0], orgMaster[1],
          orgMaster[2], orgMaster[3],
          orgMaster[4], orgMaster[5])));

                        sendBuf = Encoding.ASCII.GetBytes("A_Z@\r\n");
                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_Z Send\r\n"));
                        Network.SendData(sendBuf);
                    }
                    break;
                case "R_I": // Initial Position
                    if(m__G.m_bTestStage)
                    {
                        if (InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                Task.Run(() => m__G.fVision.AnosisInitial(true));
                            });

                        }
                        else
                        {
                            Task.Run(() => m__G.fVision.AnosisInitial(true));
                        }
                    }
                    else
                    {
                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("R_I Recieve==\r\n"));
                        m__G.fVision.SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
                    }
                    break;
                case "D_S": //Detect Shift Length
                    if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("D_S Recieve==\r\n"));
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            m__G.fVision.FindCarrierToDummyShift();
                              if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0:0.000}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}===,\r\n",
                                    m__G.fVision.mMarkShift[0].X, m__G.fVision.mMarkShift[0].Y, 
                                    m__G.fVision.mMarkShift[1].X, m__G.fVision.mMarkShift[1].Y, 
                                    m__G.fVision.mMarkShift[2].X, m__G.fVision.mMarkShift[2].Y)));

                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0:0.000}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t===,\r\n",
                                    m__G.oCam[0].mAzimuthPts[1][8].X,
                                    m__G.oCam[0].mAzimuthPts[1][8].Y,
                                    m__G.oCam[0].mAzimuthPts[1][10].X,
                                    m__G.oCam[0].mAzimuthPts[1][10].Y)));

                            byte[] sDatabuffer = m__G.fVision.MakeMarkShift();
                            sCmdBuf = Encoding.ASCII.GetBytes("A_D@6@");
                            sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

                            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
                            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
                            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_D Send\r\n"));
                            Network.SendData(sendBuf);
                        });

                    }
                    else
                    {
                        m__G.fVision.FindCarrierToDummyShift();
                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0:0.000}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}===,\r\n",
                                m__G.fVision.mMarkShift[0].X, m__G.fVision.mMarkShift[0].Y,
                                m__G.fVision.mMarkShift[1].X, m__G.fVision.mMarkShift[1].Y,
                                m__G.fVision.mMarkShift[2].X, m__G.fVision.mMarkShift[2].Y)));

                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0:0.000}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t===,\r\n",
                                m__G.oCam[0].mAzimuthPts[1][8].X,
                                m__G.oCam[0].mAzimuthPts[1][8].Y,
                                m__G.oCam[0].mAzimuthPts[1][10].X,
                                m__G.oCam[0].mAzimuthPts[1][10].Y)));

                        byte[] sDatabuffer = m__G.fVision.MakeMarkShift();
                        sCmdBuf = Encoding.ASCII.GetBytes("A_D@6@");
                        sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                        sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

                        Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
                        Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
                        Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_D Send\r\n"));
                        Network.SendData(sendBuf);
                    }
                    break;
                case "M_S":
                    ClearAllLog();
                    RunNum = 0;
                    m__G.fVision.FineCarrierCount = 0;
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            //배포시 주의 ===
                            m__G.fVision.GrabInitalMark();
                            m__G.fVision.LoadTXTYZeroOffset();
                            txtMsaterNuma.Text = mStrDummyIndex = m__G.fVision.GetMasterZeroIndex().ToString();

                            byte[] sDatabuffer = Encoding.ASCII.GetBytes("A_M@" + mStrDummyIndex + "@\r\n");
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Mark ID : " + mStrDummyIndex + " Send\r\n"));
                            Network.SendData(sDatabuffer);
                        });
                    }
                    else
                    {
                        m__G.fVision.GrabInitalMark();
                        m__G.fVision.LoadTXTYZeroOffset();
                        txtMsaterNuma.Text = mStrDummyIndex = m__G.fVision.GetMasterZeroIndex().ToString();

                        byte[] sDatabuffer = Encoding.ASCII.GetBytes("A_M@" + mStrDummyIndex + "@\r\n");
                        if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Mark ID : " + mStrDummyIndex + " Send\r\n"));
                        Network.SendData(sDatabuffer);
                    }
                    break;
                case "R_R": //Request Result
                    break;
                case "R_V": //Request Vision
                    if (InvokeRequired)
                    {
                        index = int.Parse(arry[1]);

                        sCmdBuf = Encoding.ASCII.GetBytes("A_V@");
                        string imgPath = "Fimg0_00.bmp";
                        MemoryStream mMemoryStream = new MemoryStream();
                        Bitmap iImage = new Bitmap(imgPath);
                        iImage.Save(mMemoryStream, System.Drawing.Imaging.ImageFormat.Png);

                        sDataBuff = mMemoryStream.ToArray();
                        sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                        sendBuf = new byte[sCmdBuf.Length + sDataBuff.Length + sRnBuf.Length];

                        Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
                        Array.Copy(sDataBuff, 0, sendBuf, sCmdBuf.Length, sDataBuff.Length);
                        Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDataBuff.Length, sRnBuf.Length);

                        Network.SendData(sendBuf);
                    }
                    break;
                case "R_P": //Request Program Restart
                    //Application.Exit();
                    //Thread.Sleep(5000);
                    //Application.Restart();
                    break;
                case "B_U":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);
                            if (index == 0)
                            {
                                m__G.fGraph.mDriverIC.SocketTest(1, false);
                                AddViewLog("Base Down\r\n");
                            }
                            else
                            {
                                m__G.fGraph.mDriverIC.SocketTest(1, true);
                                AddViewLog("Base Up\r\n");
                            }
                        });
                    }
                    break;
                case "P_L":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);
                            if (index == 0)
                            {
                                m__G.fGraph.mDriverIC.SocketTest(0, false);
                                AddViewLog("Pogo Pin Unload\r\n");
                            }
                            else
                            {
                                m__G.fGraph.mDriverIC.SocketTest(0, true);
                                AddViewLog("Pogo Pin load\r\n");
                            }
                        });
                    }
                    break;
                case "S_L":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);
                            if (index == 0)
                            {
                                m__G.fGraph.mDriverIC.SocketTest(2, false);
                                AddViewLog("Side Push Unload\r\n");
                            }
                            else
                            {
                                m__G.fGraph.mDriverIC.SocketTest(2, true);
                                AddViewLog("Side Push load\r\n");
                            }
                        });
                    }
                    break;
                case "C_L":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);
                            if (index == 0)
                            {
                                m__G.fGraph.mDriverIC.SocketTest(3, false);
                                AddViewLog("Cam Side Push Unload\r\n");
                            }
                            else
                            {
                                m__G.fGraph.mDriverIC.SocketTest(3, true);
                                AddViewLog("Cam Side Push load\r\n");
                            }
                        });
                    }
                    break;
                case "D_L":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);
                            if (index == 0)
                            {
                                Network.IsMonitorStart = true;
                                m__G.oCam[0].mAbort = true;
                                m__G.fGraph.mDriverIC.SocketTest(0, false);
                                Thread.Sleep(300);
                                m__G.fGraph.mDriverIC.SocketTest(2, false);
                                Thread.Sleep(300);
                                m__G.fGraph.mDriverIC.SocketTest(1, false);
                                Thread.Sleep(500);
                                m__G.fGraph.mDriverIC.SocketTest(3, false);
                                Thread.Sleep(300);
                                AddViewLog("All Dln Unload\r\n");
                            }
                            else
                            {
                                m__G.oCam[0].mAbort = true;
                                ClearAllLog();
                                Network.IsMonitorStart = false;
                                m__G.fGraph.mDriverIC.SocketTest(1, true);
                                Thread.Sleep(500);
                                m__G.fGraph.mDriverIC.SocketTest(2, true);
                                Thread.Sleep(300);
                                m__G.fGraph.mDriverIC.SocketTest(3, true);
                                Thread.Sleep(300);
                                m__G.fGraph.mDriverIC.SocketTest(0, true);
                                Thread.Sleep(300);
                                AddViewLog("All Dln load\r\n");
                            }
                        });
                    }
                    break;
                case "A_P":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0} Recieve,\r\n", cmd)));

                            for (int i = 0; i < 6; i++)
                            {
                                byte[] datas = new byte[8];
                                Array.Copy(e, 6 + i * 8, datas, 0, datas.Length);
                                motionPos[i] = BitConverter.ToDouble(datas, 0);
                            }
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("X\tY\tZ\tTX\tTY\tTZ===,\r\n")));
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}===,\r\n", motionPos[0], motionPos[1],
                                motionPos[2], motionPos[3], motionPos[4], motionPos[5])));
                            isReceivedG = true;
                        });
                    }
                    break;
                case "A_B":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0} Recieve,\r\n", cmd)));

                            for (int i = 0; i < 6; i++)
                            {
                                byte[] datas = new byte[8];
                                Array.Copy(e, 6 + i * 8, datas, 0, datas.Length);
                                probeData[i] = BitConverter.ToDouble(datas, 0);
                            }
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("PX\tPY\tPZ\tPTX\tPTY\tPTZ===,\r\n")));
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}===,\r\n", probeData[0], probeData[1],
                                probeData[2], probeData[3], probeData[4], probeData[5])));
                            isReceivedP = true;
                        });
                    }
                    break;
                case "A_C":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            index = int.Parse(arry[1]);

                            //AddViewLog(string.Format(string.Format("{0} Recieve {1} Axis,\r\n", cmd, index)));

                            isReceivedM[index] = true;
                        });
                    }
                    break;
                case "A_H":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            //index = int.Parse(arry[1]);
                            //AddViewLog(string.Format(string.Format("{0} Recieve {1} Axis,\r\n", cmd, index)));

                            isReceivedH = true;
                        });
                    }
                    break;
                case "A_G":
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0} Recieve,\r\n", cmd)));

                            byte[] datas = new byte[8];
                            Array.Copy(e, 6, datas, 0, datas.Length);
                            index = int.Parse(arry[1]);
                            motionPos[index] = BitConverter.ToDouble(datas, 0);
                            if (index == 0)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("X===,\r\n")));
                            }
                            else if (index == 1)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("Y===,\r\n")));
                            }
                            else if (index == 2)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("Z===,\r\n")));
                            }
                            else if (index == 3)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("TX===,\r\n")));
                            }
                            else if (index == 4)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("TY===,\r\n")));
                            }
                            else if (index == 5)
                            {
                                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("TZ===,\r\n")));
                            }
                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format(string.Format("{0}===,\r\n", motionPos[index])));
                            isReceivedG = true;
                        });
                    }
                    break;
            }
        }
        public void AnosisRemoteData()
        {
            byte[] sCmdBuf = null;
            byte[] sRnBuf = null;
            byte[] sendBuf = null;

            if(m__G.fVision.bHaltLive)
            {
                m__G.fVision.StartLive();
            }

            m__G.fVision.SetTriggerGrabbedFrame(1);
            //m__G.fVision.RemoteManualFindMark();
            m__G.fVision.SingleFindMarkAnosis(false);
            m__G.oCam[0].ForceTriggerTime();
            byte[] sDatabuffer = MyOwner.MakeSaveResultAnosis(true);
            sCmdBuf = Encoding.ASCII.GetBytes("A_D@1@");
            sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);

            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);

            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

            //if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_D Send\r\n"));
            Network.SendData(sendBuf);
            isReceivedMD = true;
        }
        private void btnRepeatStartTest_Click(object sender, EventArgs e)
        {
            RepeatStartTest();
        }

        public void RepeatStartTest(int frmCnt = 0)
        {
            if (btnStartTriggeredGrab.Enabled == false)
                return;

            btnStartTriggeredGrab.Enabled = false;

            m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);

            //m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            //m__G.oCam[0].mFAL.BackupFMI();

            if (!m__G.fVision.mLoaded)
            {
                m__G.fVision.InitBaslerCam();
            }

            if (!m__G.oCam[0].mFAL.mFAutoLearnLoaded)
            {
                m__G.mFAL.Show();
                m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                //mFAL.Size = new Size(1920, 1045);
                m__G.mFAL.Location = new Point(0, 0);
                m__G.mFAL.Hide();
            }
            else
            {
                m__G.mFAL.LoadLastFMI();
                m__G.mFAL.ShowMarkDGV();
            }
            Task.Run(() => m__G.fVision.LoadScaleNTheta());
            Task.Run(() => m__G.oCam[0].HaltA());

            m__G.mbSuddenStop[0] = false;

            if (frmCnt>0)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbConsecutiveTest.Text = "1";
                    });
                }
                else
                {
                    tbConsecutiveTest.Text = "1";
                }
            }
            else if (tbConsecutiveTest.Text.Length > 0)
            {
                m_LastSampleNumber = int.Parse(tbConsecutiveTest.Text);
                tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                frmCnt = MILlib.MAX_TRGGRAB_COUNT;
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                    });
                }
                else
                {
                    tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                }
            }
            else
            {
                m_LastSampleNumber = -1;
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbResidualTestNumber.Text = "";
                    });
                }
                else
                {
                    tbResidualTestNumber.Text = "";
                }
            }

            Task.Run(() => m__G.oCam[0].mFAL.LoadFMICandidate());


            if (cbRealTimeMeasure.Checked && frmCnt == 0)
                Task.Run(() => RealTimeMeasure());
            else
                Task.Run(() => { StartTriggerTest(frmCnt); });
        }

        public void WatchDog(int frameCnt)
        {
            int watchCnt = 0;
            Thread.Sleep(500);

            while(true)
            {
                if (S2System.Vision.MILlib.mTriggeredFrameCount > 0)

                Thread.Sleep(500);
                //AddViewLog(" " + S2System.Vision.MILlib.mTriggeredFrameCount.ToString());
                if (m__G.oCam[0].mExternalTriggerOrg == false && watchCnt++ > 20)
                    break;
            }
        }
        //int mRealTimeProcessedIndex = 0;
        double mRealTimeUnit = 0;
        public void RealTimeMeasure()
        {
            m__G.fVision.CameraReset(2, true);
            int waitTime = 0;
            while(true)
            {
                if (m__G.fVision.mLoaded)
                    break;
                Thread.Sleep(200);
                if (waitTime++ > 50)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    return;
                }
            }
            waitTime = 0;
            while (true)
            {
                if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
                    break;
                Thread.Sleep(200);
                if (waitTime++ > 50)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    return;
                }
            }

            m__G.mDoingStatus = "Realtime Measure";

            //m__G.fVision.SetOrgExposure(0);
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].PointTo6DMotion(-1, m__G.fVision.mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            int index = 0;
            //int cindex = 0;
            int effFrameCount = 0;
            bool[] res = new bool[12] { true, true, true, true, true, true, true, true, true, true, true, true };
            Thread[] threadRealTimeVisionData = new Thread[12];

            m__G.oCam[0].GrabA_User(0);
            m__G.oCam[0].FineCOG(true, 0, 0);
            index++;
            int oldindex = 0;
            //int indexThread = 0;
            int updatePlot = 0;
            long lTimerFrequency = 0;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            mRealTimeUnit = 8.0 / 500.0;    //  Default Value
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            while (!m__G.mbSuddenStop[0])   //  여기는 Stop 시 즉각 탈출한다.
            {
                int indexNow = index % 12;
                int index9999 = index % 1000 + 1;

                m__G.oCam[0].GrabB(index9999);

                if (m__G.mbSuddenStop[0])   //  여기는 Stop 시 즉각 탈출한다.
                    break;

                m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
                m__G.oCam[0].FineCOG(false, index9999, 0);
                //res[indexThread] = false;
                //res[indexThread] = m__G.oCam[0].FineCOG(false, index9999, indexThread);

                //while (threadRealTimeVisionData[indexThread]!= null)
                //{
                //if (effFrameCount > 24 && oldindex != index)
                //{
                updatePlot++;
                if (updatePlot%3 == 1)
                {
                    if (index < 1000)
                    {
                        Array.Copy(m__G.oCam[0].mC_pX, 1, mStroke[0], 0, index);
                        Array.Copy(m__G.oCam[0].mC_pY, 1, mStroke[1], 0, index);
                        Array.Copy(m__G.oCam[0].mC_pZ, 1, mStroke[2], 0, index);
                        Array.Copy(m__G.oCam[0].mC_pTX, 1, mStroke[3], 0, index);
                        Array.Copy(m__G.oCam[0].mC_pTY, 1, mStroke[4], 0, index);
                        Array.Copy(m__G.oCam[0].mC_pTZ, 1, mStroke[5], 0, index);

                        for (int az = 0; az < 12; az++)
                            for (int i = 0; i < index; i++)
                                mPosAzimuth[az][i] = m__G.oCam[0].mAzimuthPts[i][az];
                    }
                    else
                    {
                        Array.Copy(m__G.oCam[0].mC_pX, index9999, mStroke[0], 0, 1000 - (index9999));
                        Array.Copy(m__G.oCam[0].mC_pY, index9999, mStroke[1], 0, 1000 - (index9999));
                        Array.Copy(m__G.oCam[0].mC_pZ, index9999, mStroke[2], 0, 1000 - (index9999));
                        Array.Copy(m__G.oCam[0].mC_pTX, index9999, mStroke[3], 0, 1000 - (index9999));
                        Array.Copy(m__G.oCam[0].mC_pTY, index9999, mStroke[4], 0, 1000 - (index9999));
                        Array.Copy(m__G.oCam[0].mC_pTZ, index9999, mStroke[5], 0, 1000 - (index9999));

                        Array.Copy(m__G.oCam[0].mC_pX, 1, mStroke[0], 1000 - (index9999), index9999);
                        Array.Copy(m__G.oCam[0].mC_pY, 1, mStroke[1], 1000 - (index9999), index9999);
                        Array.Copy(m__G.oCam[0].mC_pZ, 1, mStroke[2], 1000 - (index9999), index9999);
                        Array.Copy(m__G.oCam[0].mC_pTX, 1, mStroke[3], 1000 - (index9999), index9999);
                        Array.Copy(m__G.oCam[0].mC_pTY, 1, mStroke[4], 1000 - (index9999), index9999);
                        Array.Copy(m__G.oCam[0].mC_pTZ, 1, mStroke[5], 1000 - (index9999), index9999);

                        if (mRealTimeUnit== 6.1 / 500.0)
                            mRealTimeUnit = ((m__G.oCam[0].GrabT1[900] - m__G.oCam[0].GrabT1[200]) / (double)(lTimerFrequency))/700;


                        for (int az = 0; az < 12; az++)
                        {
                            for (int i = index9999; i <= 1000; i++)
                                mPosAzimuth[az][i - index9999] = m__G.oCam[0].mAzimuthPts[i][az];

                            for (int i = 0; i <= index9999; i++)
                                mPosAzimuth[az][1000 - index9999 + i] = m__G.oCam[0].mAzimuthPts[i][az];
                        }
                    }


                    if (m__G.mbSuddenStop[0])   //  여기는 Stop 시 즉각 탈출한다.
                        break;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            oldindex = index;
                            int[] selectedIndices1 = new int[listBox1.SelectedIndices.Count];
                            int[] selectedIndices2 = new int[listBox2.SelectedIndices.Count];
                            listBox1.SelectedIndices.CopyTo(selectedIndices1, 0);
                            listBox2.SelectedIndices.CopyTo(selectedIndices2, 0);

                            if (selectedIndices1.Length > 0 && selectedIndices2.Length > 0)
                                PlotChartStroke(0, selectedIndices1, selectedIndices2, effFrameCount);

                            selectedIndices1 = new int[listBox3.SelectedIndices.Count];
                            selectedIndices2 = new int[listBox4.SelectedIndices.Count];
                            listBox3.SelectedIndices.CopyTo(selectedIndices1, 0);
                            listBox4.SelectedIndices.CopyTo(selectedIndices2, 0);

                            if (selectedIndices1.Length > 0 && selectedIndices2.Length > 0)
                                PlotChartStroke(1, selectedIndices1, selectedIndices2, effFrameCount);

                            PlotChartStroke(2, listBox5.SelectedIndex, listBox6.SelectedIndex, -1, effFrameCount);
                            mCommonDataCount = effFrameCount;
                            PlotChartMarkFromListBox7();
                        });
                    }
                }
                //}
                //if (!threadRealTimeVisionData[indexThread].IsAlive) 
                //    break;
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbConsecutiveTest.Text = index9999.ToString();
                    });
                }

                Thread.Sleep(10);
                if (m__G.mbSuddenStop[0])   //  여기는 Stop 시 즉각 탈출한다.
                    break;
                //}
                //indexThread = indexNow;
                //threadRealTimeVisionData[indexThread] = new Thread(delegate () { res[indexThread] = m__G.oCam[0].FineCOG(false, index9999, indexThread); });
                //threadRealTimeVisionData[indexThread].Start();

                index++;
                if (effFrameCount < 1000)
                    effFrameCount++;
                else
                    effFrameCount = 1000;
                
            }
            m__G.fVision.CameraReset(2, false);
            m__G.fGraph.Drive_LEDs(0, 0);
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].dAFZM_FrameCount = index % 1000;
            mRealTimeUnit = 0;
            m__G.mbSuddenStop[0] = false;
            //if (InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate
            //    {
            //        btnStartTriggeredGrab.Enabled = true;
            //    });
            //}else
            //    btnStartTriggeredGrab.Enabled = true;
        }

        public List<String> mResultFileList = new List<string>();
        int watchNetwork = 0;
        byte[] mA_S_GrabBegin = Encoding.ASCII.GetBytes("A_S@\r\n");
        byte[] mA_F_buffs = Encoding.ASCII.GetBytes("A_F@\r\n");

        //public void StartTriggerTest(int frmCnt = 0)
        //{
        //    long triggeredTime = 0;

        //    SupremeTimer.QueryPerformanceCounter(ref triggeredTime);

        //    int waitTime = 0;
        //    while (true)
        //    {
        //        if (m__G.fVision.mLoaded)
        //            break;
        //        Thread.Sleep(100);
        //        if (waitTime++ > 80)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    btnStartTriggeredGrab.Enabled = true;
        //                });
        //            }
        //            m__G.mDoingStatus = "IDLE";
        //            m__G.mIDLEcount = 0;
        //            return;
        //        }
        //    }
        //    waitTime = 0;

        //    //m__G.oCam[0].mFAL.mFastMode = m__G.m_bEulerRotation;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23

        //    while (true)
        //    {
        //        if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
        //            break;
        //        Thread.Sleep(100);
        //        if (waitTime++ > 80)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    btnStartTriggeredGrab.Enabled = true;
        //                });
        //            }
        //            m__G.mDoingStatus = "IDLE";
        //            m__G.mIDLEcount = 0;
        //            return;
        //        }
        //    }
        //    //m__G.fVision.SetOrgExposure(0);
        //    //m__G.mDoingStatus = "Triggered Measure";

        //    m__G.oCam[0].mFAL.BackupFMI();

        //    if ( frmCnt > 0 )
        //        m__G.oCam[0].mTargetTriggerCount = frmCnt;

        //    long lTimerFrequency = 1000;
        //    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

        //    byte[] buffs = null;
        //    double timeForTrigger = 0;

        //    int lmaxThread = m__G.mMaxThread;
        //    if (frmCnt < 1000)
        //        lmaxThread = 12;
        //    if (frmCnt < 20)
        //        lmaxThread = 5;

        //    double frameRate = 0;
        //    int lgrabbedFrame = 0;
        //    double resTime = 0;

        //    Task taskVsData;
        //    string strlostTrigger = "";
        //    while (!m__G.mbSuddenStop[0])
        //    {
        //        //m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL, true);
        //        m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);

        //        m__G.oCam[0].mFAL.RecoverFromBackupFMI();


        //        //////////////////////////////////////////////////////////////////////////
        //        //////////////////////////////////////////////////////////////////////////
        //        //mViewLog1.Text += "Call ProcessVisionData\r\n";
        //        long startTime = 0;
        //        long endTime = 0;


        //        //                SupremeTimer.QueryPerformanceCounter(ref startTime);
        //        //  Thread 방식이 약간 시간 더걸림 10 msec 더 걸림.
        //        SupremeTimer.QueryPerformanceCounter(ref startTime);

        //        //mA_S_GrabBegin = Encoding.ASCII.GetBytes("A_S@\r\n");
        //        Network.SendData(mA_S_GrabBegin);

        //        if (InvokeRequired)
        //        {
        //            BeginInvoke((MethodInvoker)delegate
        //            {
        //                mViewLog1.Text += "Send : A_S  - Waiting Trigger\r\n";
        //            });
        //        }

        //        m__G.oCam[0].SetSaveLostMarkFrame(m__G.m_bSaveLostTestSet);
        //        if ( m__G.m_bPrismCS )
        //        {

        //            taskVsData = new Task(() => { m__G.fVision.ProcessVisionData(m__G.oCam[0].mTargetTriggerCount, lmaxThread); });
        //            taskVsData.Start();
        //            Task taskExtTrigger = new Task(() => { m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame); });

        //            taskExtTrigger.Start();

        //            m__G.fVision.SetTriggerGrabbedFrame(S2System.Vision.MILlib.mTriggeredFrameCount);
        //            //m__G.fVision.SetTriggerGrabbedFPS(frameRate);

        //            if (m__G.mbSuddenStop[0])
        //                break;


        //            taskVsData.Wait();
        //            SupremeTimer.QueryPerformanceCounter(ref endTime);
        //            resTime = (endTime - startTime) / (double)(lTimerFrequency);
        //            timeForTrigger = (m__G.oCam[0].mCurTime - startTime) / (double)(lTimerFrequency);

        //            taskExtTrigger.Wait();
        //            m__G.fGraph.Drive_LEDs(0, 0);
        //        }
        //        else
        //        {
        //            m__G.oCam[0].mFinishVisionData = true;
        //            m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);

        //            if (m__G.m_bAutoLastFrame)
        //            {
        //                if (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
        //                {
        //                    m__G.fVision.CameraReset(2, true);
        //                    while (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
        //                    {
        //                        m__G.oCam[0].GrabB(lgrabbedFrame, true);  //  milCommonImageGrab 가 아니라 다른데에 Grab 해야 한다. 수정 필요
        //                        lgrabbedFrame++;
        //                    }
        //                    strlostTrigger = "            >> Lost Trigger @ " + m__G.oCam[0].mTargetTriggerCount.ToString() + "Test\r\n";
        //                    m__G.fVision.CameraReset(2, false);
        //                    m__G.oCam[0].SetTriggeredframeCount(lgrabbedFrame);
        //                    if (m__G.oCam[0].mTargetTriggerCount > 50 && m__G.m_bSaveLostTestSet)
        //                    {
        //                        string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\" + m__G.oCam[0].mTargetTriggerCount.ToString() + "\\";
        //                        if (!Directory.Exists(fileName))
        //                            Directory.CreateDirectory(fileName);
        //                        for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
        //                        {
        //                            string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
        //                            m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
        //                        }
        //                    }
        //                }
        //            }

        //            m__G.fVision.ProcessVisionData(m__G.oCam[0].mTargetTriggerCount, lmaxThread);

        //            SupremeTimer.QueryPerformanceCounter(ref endTime);
        //            resTime = (endTime - startTime) / (double)(lTimerFrequency);
        //            timeForTrigger = (m__G.oCam[0].mCurTime - startTime) / (double)(lTimerFrequency);

        //            m__G.fVision.SetTriggerGrabbedFrame(lgrabbedFrame);
        //            m__G.fVision.SetTriggerGrabbedFPS(frameRate);

        //            m__G.fGraph.Drive_LEDs(0, 0);

        //            if (m__G.mbSuddenStop[0])
        //                break;
        //        }
        //        ///////////////////////////////////////////////////////////////////////////////////////
        //        //int EffframeCount = Math.Min(lgrabbedFrame, m__G.oCam[0].mTargetTriggerCount);
        //        int EffframeCount =  m__G.oCam[0].mTargetTriggerCount;
        //        mCommonDataCount = EffframeCount;

        //        //  그래프 그리기 추가 필요, Thread.Sleep(500) 대신 그래프 그리기 할 것.
        //        if ( cb1stLFP.Checked )
        //        {
        //            for ( int i= EffframeCount-1; i>0; i--)
        //            {
        //                m__G.oCam[0].mC_pTX[i] = (m__G.oCam[0].mC_pTX[i] + m__G.oCam[0].mC_pTX[i - 1]) / 2;
        //                m__G.oCam[0].mC_pTY[i] = (m__G.oCam[0].mC_pTY[i] + m__G.oCam[0].mC_pTY[i - 1]) / 2;
        //                m__G.oCam[0].mC_pTZ[i] = (m__G.oCam[0].mC_pTZ[i] + m__G.oCam[0].mC_pTZ[i - 1]) / 2;

        //                m__G.oCam[0].mC_pX[i] = (m__G.oCam[0].mC_pX[i] + m__G.oCam[0].mC_pX[i - 1]) / 2;
        //                m__G.oCam[0].mC_pY[i] = (m__G.oCam[0].mC_pY[i] + m__G.oCam[0].mC_pY[i - 1]) / 2;
        //                m__G.oCam[0].mC_pZ[i] = (m__G.oCam[0].mC_pZ[i] + m__G.oCam[0].mC_pZ[i - 1]) / 2;
        //            }
        //        }

        //        SupremeTimer.QueryPerformanceCounter(ref startTime);

        //        //MyOwner.WriteResult(); //  결과파일 Full Path 
        //        string lResultFile = MyOwner.WriteResultBin(); //  결과파일 Full Path 

        //        //mResultFileList.Add(lResultFile);
        //        mResultFileList.Insert(0, lResultFile);
        //        if (mResultFileList.Count > 100)
        //            mResultFileList.RemoveAt(100);

        //        SupremeTimer.QueryPerformanceCounter(ref endTime);
        //        double ltime = (endTime - startTime) / (double)(lTimerFrequency);

        //        double totalTime = (endTime - triggeredTime) / (double)(lTimerFrequency);

        //        try
        //        {
        //            Network.SendData(mA_F_buffs);
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    mViewLog1.Text += strlostTrigger + "Send : A_F " + frmCnt.ToString() + " : " + lgrabbedFrame.ToString() + " " + lResultFile + "\r\n";
        //                });
        //            }
        //            else
        //            {
        //                mViewLog1.Text += strlostTrigger + "Send : A_F " + frmCnt.ToString() + " : " + lgrabbedFrame.ToString() + " " + lResultFile + "\r\n";
        //            }
        //        }
        //        catch
        //        {
        //            watchNetwork = 2;
        //        }
        //        /////////////////////////////////////////////////////////////////

        //        m_LastSampleNumber--;
        //        if (InvokeRequired)
        //        {
        //            BeginInvoke((MethodInvoker)delegate
        //            {
        //                if ( mViewLog1.Text.Length > 10000)
        //                {
        //                    string[] llines = mViewLog1.Text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    mViewLog1.Text = llines[llines.Length - 2] + "\r\n" + llines[llines.Length - 1] + "\r\n";
        //                }
        //                //mViewLog1.Text += m__G.oCam[0].mMatroxMsg;
        //                mViewLog1.Text += m__G.oCam[0].GetLastTriggerTime().ToString("yy/MM/dd/HH:mm:ss") + "\t" + timeForTrigger.ToString("F3") + "sec Wait. ImageProcessingTime\t" + resTime.ToString("F3")  + /* "\tsec\tTime SaveResult\t" + ltime.ToString("F3") +*/ "\tsec\r\n";
        //                mViewLog1.SelectionStart = mViewLog1.Text.Length;
        //                mViewLog1.ScrollToCaret();
        //                if (m_LastSampleNumber>=0)
        //                    tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
        //            });
        //        }
        //        else
        //        {
        //            //mViewLog1.Text += m__G.oCam[0].mMatroxMsg;
        //            mViewLog1.Text += m__G.oCam[0].GetLastTriggerTime().ToString("yy/MM/dd/HH:mm:ss") + "\t" + timeForTrigger.ToString("F3") + "sec Wait. ImageProcessingTime \t" + resTime.ToString("F3") + /*"\tsec\tTime SaveResult\t" + ltime.ToString("F3") + */"\tsec\r\n";
        //            mViewLog1.SelectionStart = mViewLog1.Text.Length;
        //            mViewLog1.ScrollToCaret();
        //            if (m_LastSampleNumber >= 0)
        //                tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
        //        }

        //        if  (!m__G.m_bHideAllGraph)
        //        {
        //            Array.Copy(m__G.oCam[0].mC_pTX, mStroke[3], EffframeCount);
        //            Array.Copy(m__G.oCam[0].mC_pTY, mStroke[4], EffframeCount);
        //            Array.Copy(m__G.oCam[0].mC_pTZ, mStroke[5], EffframeCount);
        //            Array.Copy(m__G.oCam[0].mC_pX, mStroke[0], EffframeCount);
        //            Array.Copy(m__G.oCam[0].mC_pY, mStroke[1], EffframeCount);
        //            Array.Copy(m__G.oCam[0].mC_pZ, mStroke[2], EffframeCount);

        //            for (int az = 0; az < 12; az++)
        //                for (int i = 0; i < EffframeCount; i++)
        //                    mPosAzimuth[az][i] = m__G.oCam[0].mAzimuthPts[i][az];

        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    PlotMeasureData(EffframeCount);
        //                });
        //            }
        //            else
        //            {
        //                PlotMeasureData(EffframeCount);
        //            }
        //        }

        //        if (m_LastSampleNumber <= 0)
        //        {
        //            //m__G.mbSuddenStop[0] = true;
        //            //m__G.oCam[0].mAbort = true;
        //            break;
        //        }
        //    }
        //    if (m__G.mbSuddenStop[0])
        //    {
        //        if (InvokeRequired)
        //        {
        //            BeginInvoke((MethodInvoker)delegate
        //            {
        //                mViewLog1.Text += "Stop Waiting Trigger. " + timeForTrigger.ToString("F3") + "sec Wait\r\n";
        //                mViewLog1.SelectionStart = mViewLog1.Text.Length;
        //                mViewLog1.ScrollToCaret();

        //            });
        //        }
        //    }

        //    m__G.fGraph.Drive_LEDs(0, 0);
        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();
        //    m__G.mbSuddenStop[0] = false;
        //    if (InvokeRequired)
        //    {
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            btnStartTriggeredGrab.Enabled = true;
        //        });
        //    }
        //    else
        //        btnStartTriggeredGrab.Enabled = true;

        //    m__G.mDoingStatus = "IDLE";
        //    m__G.mIDLEcount = 0;
        //}

        byte[] mA_E_buffs = Encoding.ASCII.GetBytes("A_E@\r\n");    //   Error
        bool mbStartTriggerTest = false;
        public void StartTriggerTestWrapper(int frmCnt = 0)
        {
            //if (mbStartTriggerTest)
            //{
            //    SuddenStop();
            //}
            //m__G.oCam[0].ResetmCpXY();

            //// Debug Mode =====================================================
            if (m__G.m_bDebugMode)
            {
                MyOwner.DebugStop = false;
                Task.WaitAll(Task.Factory.StartNew(() =>
                {
                    while (!MyOwner.DebugStop)
                    {
                        Thread.Sleep(1000);
                    }
                }));
            }
            ////===================================================================
            ///
            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 99];

            while (true)
            {
                if (m__G.oCam[0].mRequestedTriggerCount == 0)
                    break;

                Thread.Sleep(1);
            }
            m__G.oCam[0].mRequestedTriggerCount = frmCnt;

            if (!mbStartTriggerTest)
                StartTriggerTestNew();

        }

        public void SendAStoHost()
        {
            Network.SendData(mA_S_GrabBegin);
            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_S Send, Trigger Waiting Start\r\n"));
        }
        public void StartTriggerTestNew()
        {
            mbStartTriggerTest = true;
            long triggeredTime = 0;


            int waitTime = 0;
            while (true)
            {
                if (m__G.fVision.mLoaded)
                    break;
                Thread.Sleep(1);
                if (waitTime++ > 80)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                    mbStartTriggerTest = false;
                    return;
                }
            }
            waitTime = 0;

            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bEulerRotation;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23
            //m__G.fGraph.mDriverIC.AckSignal(0, false);
            while (true)
            {
                if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
                    break;
                Thread.Sleep(1);
                if (waitTime++ > 80)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                    mbStartTriggerTest = false;
                    return;
                }
            }
            m__G.mDoingStatus = "Trigger Testing";
            //m__G.oCam[0].mFAL.BackupFMI();

            if (m__G.oCam[0].mRequestedTriggerCount > 0)
                m__G.oCam[0].mTargetTriggerCount = m__G.oCam[0].mRequestedTriggerCount;

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////

            long lTimerFrequency = 1000;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            double timeForTrigger = 0;
            int lmaxThread = m__G.mMaxThread;
            double frameRate = 0;
            int lgrabbedFrame = 0;
            double resTime = 0;

            //int loopCnt = 0;

            //if (m__G.m_bNoHostPC)
            //    loopCnt = 0;

            long startTime = 0;
            long endTime = 0;
            long endTriggerTime = 0;


            //int MaxLoopCnt = int.Parse(tbConsecutiveTest.Text);
            int triggergrabbedCnt = 0;
            string strAutoLastFrame = "";
            bool HasAutoLastFrame = false;


            //m__G.fVision.SetDefaultMarkConfig(false);


            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            ////////for (int mi = 0; mi < 5; mi++)
            ////////    m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 99];

            ////////m__G.oCam[0].mTargetTriggerCount = frmCnt;
            ////////m__G.oCam[0].mRequestedTriggerCount = frmCnt;
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////

            //mA_S_GrabBegin = Encoding.ASCII.GetBytes("A_S@\r\n");
            m__G.oCam[0].SetSaveLostMarkFrame(m__G.m_bSaveLostTestSet);

            m__G.oCam[0].mAbort = false;

            while (true)
            {
                TriggerLossClear();
                timeForTrigger = 0;
                lmaxThread = m__G.mMaxThread;
                frameRate = 0;
                lgrabbedFrame = 0;
                resTime = 0;
                startTime = 0;
                endTime = 0;
                endTriggerTime = 0;
                triggergrabbedCnt = 0;
                strAutoLastFrame = "";
                HasAutoLastFrame = false;
                //m__G.oCam[0].mFAL.BackupFMI();
                m__G.fVision.SetDefaultMarkConfig(false);
                m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
                mbStartTriggerTest = true;
                m__G.oCam[0].mFinishVisionData = true;
                //if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Trigger grab Start\r\n"));


                if (!m__G.m_bNoHostPC)
                {
                    //if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_S Send, Trigger Waiting Start\r\n"));
                    //while (m__G.oCam[0].mRequestedTriggerCount == 0)
                    //{
                    //    Thread.Sleep(1);
                    //    if ( m__G.oCam[0].mAbort )
                    //        break;
                    //}
                    //if (!m__G.oCam[0].mAbort)
                    //    Network.SendData(mA_S_GrabBegin);
                }
                SupremeTimer.QueryPerformanceCounter(ref startTime);
                //m__G.oCam[0].mTargetTriggerCount = m__G.oCam[0].mRequestedTriggerCount;








                m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);
                if (m__G.oCam[0].mAbort)
                {
                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                    m__G.fGraph.Drive_LEDs(0, 0);
                    mbStartTriggerTest = false;
                    m__G.oCam[0].mAbort = false;
                    if (!m__G.m_bHideAllGraph) AddViewLog("Abort waiting trigger." + m__G.oCam[0].mMatroxMsg + "\r\n");
                    return;
                }
                if (m__G.mbSuddenStop[0])
                {
                    AddViewLog(string.Format("SuddenStop =="));
                    m__G.mbSuddenStop[0] = false;
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.
                        });
                    }
                    else
                        btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.

                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                  break;
                  //return;
                }
                else
                {
                    if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("mMatroxMsg : {0}", m__G.oCam[0].mMatroxMsg));
                }
                //if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Trigger grab End\r\n"));
                //if (!m__G.m_bHideAllGraph) AddViewLog("MatroxMsg " + m__G.oCam[0].mMatroxMsg);

                triggergrabbedCnt = lgrabbedFrame;
                //AddViewLog("MSG\t" + frmCnt.ToString() + "\t>>\t" + m__G.oCam[0].mMatroxMsg);
                //if (endTriggerTime != 0)
                //{
                //    timeinterval = (S2System.Vision.MILlib.mGrabTiming[0] - endTriggerTime) / (double)(lTimerFrequency);
                //    AddViewLog("\t\tTime Interval\t " + timeinterval.ToString("F3") + "\tsec\r\n");
                //}

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                endTriggerTime = endTime;

                resTime = (m__G.oCam[0].mBeforeTime - startTime) / (double)(lTimerFrequency);
                if (m__G.oCam[0].mMatroxMsg.Contains("Timeout 3"))
                {
                    //m__G.mbSuddenStop[0] = true;
                    //MessageBox.Show("m__G.mbSuddenStop[0] = true");
                    try
                    {
                        //////Network.SendData(mA_E_buffs);
                        AddViewLog("    A_E Trigger Timeout Error " + m__G.oCam[0].mMatroxMsg + "\r\n");
                        //SaveViewLog();
                    }
                    catch
                    {
                        AddViewLog("Network Error while sending A_E\r\n");
                    }
                }
                if (m__G.m_bAutoLastFrame)
                {
                    HasAutoLastFrame = false;
                    if (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
                    {
                        HasAutoLastFrame = true;
                        int framefromtrigger = lgrabbedFrame;
                        long modeChangeFrom = 0;
                        long modeChangeTo = 0;
                        SupremeTimer.QueryPerformanceCounter(ref modeChangeFrom);
                        m__G.fVision.CameraReset(2, true);
                        SupremeTimer.QueryPerformanceCounter(ref modeChangeTo);
                        while (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
                        {
                            m__G.oCam[0].GrabB(lgrabbedFrame, true);  //  milCommonImageGrab 가 아니라 다른데에 Grab 해야 한다. 수정 필요
                            lgrabbedFrame++;
                        }
                        m__G.fVision.CameraReset(2, false);
                        m__G.oCam[0].SetTriggeredframeCount(lgrabbedFrame);
                        resTime = (modeChangeTo - modeChangeFrom) / (double)(lTimerFrequency);

                        strAutoLastFrame = framefromtrigger.ToString();
                        //AddViewLog("\tStart Auto Last Frame from " + framefromtrigger.ToString() + " change took " + resTime.ToString("F4") + " sec\r\n");

                        if (m__G.oCam[0].mTargetTriggerCount > 50 && m__G.m_bSaveLostTestSet)
                        {
                            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\" + m__G.oCam[0].mTargetTriggerCount.ToString() + "\\";
                            if (!Directory.Exists(fileName))
                                Directory.CreateDirectory(fileName);
                            for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
                            {
                                string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
                                m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
                            }
                        }
                    }
                }

                //Save Image Test
                if(m__G.m_bSaveImage && m__G.oCam[0].mTargetTriggerCount == m__G.m_SaveImageCount)
                {
                    string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\TestIamge\\";
                    if (!Directory.Exists(fileName))
                        Directory.CreateDirectory(fileName);
                    for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
                    {
                        string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
                        m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
                    }
                }
                m__G.fGraph.Drive_LEDs(0, 0);
                //AddViewLog("Process vision data " + lgrabbedFrame.ToString());






                //  Use Default Model by setting m__G.mFAL.mCandidateIndex = 0;
                m__G.mFAL.mCandidateIndex = 0;
                SupremeTimer.QueryPerformanceCounter(ref startTime);

                //    임시코드 - 테스트 하나에 집중해서 마크 모델이 유효하도록 조정해야함.
                //if (loopCnt < MaxLoopCnt - 1)
                //{
                //    loopCnt++;
                //    continue;
                //}
                //if (loopCnt == MaxLoopCnt - 1)    //    임시코드
                //AddViewLog("Processing .. \t");
                m__G.oCam[0].mFAL.mGotoLoopCount = 0;
                m__G.oCam[0].mFAL.mAccuShiftX = 0;
                m__G.oCam[0].mFAL.mAccuShiftY = 0;
                //lgrabbedFrame--;
                //m__G.fGraph.mDriverIC.AckSignal(0, true);
                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Vision Process Start\r\n"));

                if (m__G.oCam[0].mTargetTriggerCount < 20)
                    lmaxThread = 1;

                ///////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////
                //  Euler Angle Rotation 측정 및 적용
                if (mbEulerMeasureApply)
                    m__G.oCam[0].mFAL.ValidateEulerRotation(false); //  측정시에는 Euler Anlge Rotation 적용하지 않은상태에서 RAw Data 추출.
                else
                    m__G.oCam[0].mFAL.ValidateEulerRotation(mbEulerSimpleApply); //  측정시에는 Euler Anlge Rotation 적용하지 않은상태에서 RAw Data 추출.
                ///////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////


                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Vision Process Start.. Thread Count = " + lmaxThread.ToString() + "\r\n"));

                if (lgrabbedFrame >= m__G.oCam[0].mTargetTriggerCount)
                    m__G.fVision.ProcessVisionData(m__G.oCam[0].mTargetTriggerCount, lmaxThread);
                else
                    m__G.fVision.ProcessVisionData(lgrabbedFrame, lmaxThread);

                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Vision Process End\r\n"));
                //m__G.fGraph.mDriverIC.AckSignal(0, false);

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                resTime = (endTime - startTime) / (double)(lTimerFrequency);

                int gotoCount = m__G.oCam[0].mFAL.mGotoLoopCount;
                int accShiftX = m__G.oCam[0].mFAL.mAccuShiftX;
                int accShiftY = m__G.oCam[0].mFAL.mAccuShiftY;
                //AddViewLog(lgrabbedFrame.ToString() + " frames with " + lmaxThread.ToString() + " threads" + " took " + resTime.ToString("F3") + " sec." + "GotoLoop Called " + gotoCount.ToString() + " accXS=" + accShiftX.ToString() + " accYS=" + accShiftY.ToString() +  "\r\n");
                //AddViewLog(lgrabbedFrame.ToString() + " frames with " + lmaxThread.ToString() + " threads" + " took " + resTime.ToString("F3") + " sec.\r\n");
                string strRes = "\t" + m__G.oCam[0].mTargetTriggerCount.ToString() + "\t>>\t" + triggergrabbedCnt.ToString() + "\t" + lgrabbedFrame.ToString() + " " + HasAutoLastFrame.ToString() + "\t" + resTime.ToString("F3") + " sec.\r\n";
                //if (!m__G.m_bHideAllGraph) AddViewLog(strRes);

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                resTime = (endTime - startTime) / (double)(lTimerFrequency);
                timeForTrigger = (m__G.oCam[0].mCurTime - startTime) / (double)(lTimerFrequency);

                m__G.fVision.SetTriggerGrabbedFrame(lgrabbedFrame);
                m__G.fVision.SetTriggerGrabbedFPS(frameRate);

                if (m__G.mbSuddenStop[0])
                {
                    AddViewLog("m__G.mbSuddenStop[0] = " + m__G.mbSuddenStop[0].ToString() + "\r\n");
                }



                ///////////////////////////////////////////////////////////////////////////////////////
                //int EffframeCount = Math.Min(lgrabbedFrame, m__G.oCam[0].mTargetTriggerCount);
                int EffframeCount = m__G.oCam[0].mTargetTriggerCount;
                mCommonDataCount = EffframeCount;

                //  그래프 그리기 추가 필요, Thread.Sleep(500) 대신 그래프 그리기 할 것.
                if (cb1stLFP.Checked)
                {
                    for (int i = EffframeCount - 1; i > 0; i--)
                    {
                        m__G.oCam[0].mC_pTX[i] = (m__G.oCam[0].mC_pTX[i] + m__G.oCam[0].mC_pTX[i - 1]) / 2;
                        m__G.oCam[0].mC_pTY[i] = (m__G.oCam[0].mC_pTY[i] + m__G.oCam[0].mC_pTY[i - 1]) / 2;
                        m__G.oCam[0].mC_pTZ[i] = (m__G.oCam[0].mC_pTZ[i] + m__G.oCam[0].mC_pTZ[i - 1]) / 2;

                        m__G.oCam[0].mC_pX[i] = (m__G.oCam[0].mC_pX[i] + m__G.oCam[0].mC_pX[i - 1]) / 2;
                        m__G.oCam[0].mC_pY[i] = (m__G.oCam[0].mC_pY[i] + m__G.oCam[0].mC_pY[i - 1]) / 2;
                        m__G.oCam[0].mC_pZ[i] = (m__G.oCam[0].mC_pZ[i] + m__G.oCam[0].mC_pZ[i - 1]) / 2;
                    }
                }
                SupremeTimer.QueryPerformanceCounter(ref startTime);

                string lResultFile = "";

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ltime = (endTime - startTime) / (double)(lTimerFrequency);

                double totalTime = (endTime - triggeredTime) / (double)(lTimerFrequency);

                // 다음은 자화에서 검증완료
                if (!m__G.m_bNoHostPC)
                {
                    try
                    {
                        ///////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////////////////////////
                        //  Euler Angle Rotation 측정 및 적용
                        if (mbEulerMeasureApply)
                        {
                            EulerMeasureApply(EffframeCount);
                            mbEulerMeasureApply = false;
                        }
                        ///////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////////////////////////

                        m__G.oCam[0].SetTriggeredframeCount(m__G.oCam[0].mTargetTriggerCount);
                        if (!m__G.m_bHideAllGraph) AddViewLog("MakeSaveResult " + m__G.oCam[0].mTargetTriggerCount.ToString() + "\r\n");
                        byte[] sDatabuffer = MyOwner.MakeSaveResult();
                        //MyOwner.WriteResultBin(0);
                        int framCnt = m__G.oCam[0].mTargetTriggerCount;

                        //Task.Factory.StartNew(() =>
                        //{
                        byte[] sCmdBuf = null;
                        byte[] sRnBuf = null;
                        byte[] sendBuf = null;

                        sCmdBuf = Encoding.ASCII.GetBytes("A_R@" + framCnt.ToString() + "@");
                        sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                        sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

                        Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);

                        Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);

                        Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

                        Network.SendData(sendBuf);

                        if (!m__G.m_bHideAllGraph)
                        {
                            AddViewLog(string.Format("A_R Send\r\n"));
                        }

                        //});
                    }
                    catch
                    {
                        AddViewLog("Network Error while sending A_F\r\n");
                    }
                }
                /////////////////////////////////////////////////////////////////

                m_LastSampleNumber--;

                if(m__G.m_bSaveFImage)
                {
                    string fileName = m__G.m_RootDirectory + string.Format("\\Result\\RawData\\User\\Image{0}\\", m__G.oCam[0].mTargetTriggerCount);

                    if (!Directory.Exists(fileName))
                        Directory.CreateDirectory(fileName);
                    for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
                    {
                        string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
                        m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
                    }
                }
                if (m__G.m_bSaveNgImage)
                {
                    bool NeedToSaveImage = false;
                    double min = 9999;
                    double max = -9999;
                    for (int pi = 0; pi < m__G.oCam[0].mTargetTriggerCount; pi++)
                    {
                        if (m__G.oCam[0].mC_pY[pi] < min)
                            min = m__G.oCam[0].mC_pY[pi];
                        if (m__G.oCam[0].mC_pY[pi] > max)
                            max = m__G.oCam[0].mC_pY[pi];

                    }
                    if ((max - min) * (5.5 / Global.LensMag) > 100.0)  //  300msec ~ 330msec 에서 최대최소의 변위차가 5um 이상인 경우 영상 저장 필요. 정상적인 경우 변위 1um 이하
                        NeedToSaveImage = true;
                    for (int pi = 0; pi < m__G.oCam[0].mTargetTriggerCount; pi++)
                    {
                        if (m__G.oCam[0].mC_pY[pi] == 0)
                            NeedToSaveImage = true;
                    }
                    if (NeedToSaveImage)
                    {
                        string sDate = DateTime.Now.ToString("yyMMddHHmmss");
                        string fileName = m__G.m_RootDirectory + string.Format("\\Result\\RawData\\NG\\{0}\\Image{1}\\", sDate, m__G.oCam[0].mTargetTriggerCount);

                        if (!Directory.Exists(fileName))
                            Directory.CreateDirectory(fileName);
                        for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
                        {
                            string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
                            m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
                        }
                    }
                }
                /////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////
                ///
                if (m_LastSampleNumber >= 0)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                        });
                    }
                    else
                    {
                        tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                    }
                }

                int leffFrameCount = EffframeCount;
                if (!m__G.m_bHideAllGraph)
                {
                    Array.Copy(m__G.oCam[0].mC_pTX, mStroke[3], leffFrameCount);
                    Array.Copy(m__G.oCam[0].mC_pTY, mStroke[4], leffFrameCount);
                    Array.Copy(m__G.oCam[0].mC_pTZ, mStroke[5], leffFrameCount);
                    Array.Copy(m__G.oCam[0].mC_pX, mStroke[0], leffFrameCount);
                    Array.Copy(m__G.oCam[0].mC_pY, mStroke[1], leffFrameCount);
                    Array.Copy(m__G.oCam[0].mC_pZ, mStroke[2], leffFrameCount);

                    for (int az = 0; az < 12; az++)
                        for (int i = 0; i < leffFrameCount; i++)
                            mPosAzimuth[az][i] = m__G.oCam[0].mAzimuthPts[i][az];

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            PlotMeasureData(leffFrameCount);
                        });
                    }
                    else
                    {
                        PlotMeasureData(leffFrameCount);
                    }
                }

                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////
                //  Cal with Second Mark Set
                //  Use Second Model by setting m__G.mFAL.mCandidateIndex = 1;
                for (int mi = 1; mi < m__G.mFAL.GetNumFMICandidate(); mi++)
                {
                    //////////////////////////////////////////////////////////////
                    /////   다중 모델 추적하기위한 모델 변경 관련 코드
                    //////////////////////////////////////////////////////////////
                    m__G.mFAL.mCandidateIndex = mi;
                    m__G.mFAL.mFZM.mbCompY = m__G.mFAL.mCandidateIndex;
                    m__G.fVision.ChangeFiducialMark(mi);
                    lmaxThread = m__G.mMaxThread;
                    if (m__G.oCam[0].mTargetTriggerCount < 20)
                        lmaxThread = 1;

                    if (lgrabbedFrame >= m__G.oCam[0].mTargetTriggerCount)
                        m__G.fVision.ProcessVisionData(m__G.oCam[0].mTargetTriggerCount, lmaxThread);
                    else
                        m__G.fVision.ProcessVisionData(lgrabbedFrame, lmaxThread);

                    lResultFile = MyOwner.WriteResultBin(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                                                                                     //lResultFile = MyOwner.WriteResultPos(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                }
                m__G.mFAL.mFZM.mbCompY = 0;
                m__G.fVision.ChangeFiducialMark(0);

                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////
                ///

                //if (m_LastSampleNumber < 0 && !m__G.m_bNoHostPC)
                //{
                //    if (!m__G.m_bHideAllGraph) AddViewLog("Out of loop at loopCnt=" + loopCnt.ToString() + "\r\n");
                //}

                if (m__G.mbSuddenStop[0])
                {
                    AddViewLog("Stop Waiting Trigger. " + timeForTrigger.ToString("F3") + "sec Wait\r\n");
                }

                m__G.mFAL.mCandidateIndex = 0;
                m__G.fVision.ChangeFiducialMark(0);

                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.mbSuddenStop[0] = false;
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.
                    });
                }
                else
                    btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.

                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;

            if (m__G.mbSuddenStop[0])
                break;
                //return;
            }
            m__G.fGraph.Drive_LEDs(0, 0);
            mbStartTriggerTest = false;
        }

        public void InitializeEulerMatrix()
        {
            mbEulerSimpleApply = false;
            m__G.oCam[0].mFAL.ValidateEulerRotation(mbEulerSimpleApply); //  측정시에는 Euler Anlge Rotation 적용하지 않은상태에서 RAw Data 추출.

            m__G.fVision.mPhiThetaPsi[0] = 0;
            m__G.fVision.mPhiThetaPsi[1] = 0;
            m__G.fVision.mPhiThetaPsi[2] = 0;

            m__G.oCam[0].mFAL.SetEulerAngle(m__G.fVision.mPhiThetaPsi);
            string eulerFile = m__G.m_RootDirectory + "\\DoNotTouch\\EulerAngle" + m__G.fVision.camID0 + ".txt";
            StreamWriter wr = new StreamWriter(eulerFile);
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[0].ToString("F8"));
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[1].ToString("F8"));
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[2].ToString("F8"));
            wr.Close();
            m__G.fVision.UpdateEulerAngle();
        }
        public void EulerMeasureApply(int EffframeCount)
        {
            double Xmin = 9999;
            double Xmax = -9999;
            double Ymin = 9999;
            double Ymax = -9999;
            double Zmin = 9999;
            double Zmax = -9999;

            for (int i = 0; i < EffframeCount; i++)
            {
                if ( m__G.oCam[0].mC_pX[i] < Xmin)
                    Xmin = m__G.oCam[0].mC_pX[i];
                if ( m__G.oCam[0].mC_pY[i] < Ymin)
                    Ymin = m__G.oCam[0].mC_pY[i];
                if ( m__G.oCam[0].mC_pZ[i] < Zmin)
                    Zmin = m__G.oCam[0].mC_pZ[i];

                if (m__G.oCam[0].mC_pX[i] > Xmax)
                    Xmax = m__G.oCam[0].mC_pX[i];
                if (m__G.oCam[0].mC_pY[i] > Ymax)
                    Ymax = m__G.oCam[0].mC_pY[i];
                if (m__G.oCam[0].mC_pZ[i] > Zmax)
                    Zmax = m__G.oCam[0].mC_pZ[i];
            }
            double Xmargin = (Xmax - Xmin) / 100;
            double Ymargin = (Ymax - Ymin) / 100;
            double Zmargin = (Zmax - Zmin) / 100;
            Xmin = Xmin + Xmargin;
            Xmax = Xmax - Xmargin;
            Ymin = Ymin + Ymargin;
            Ymax = Ymax - Ymargin;
            Zmin = Zmin + Zmargin;
            Zmax = Zmax + Zmargin;

            int Xstart = 0;
            int Xend = 0;
            int Ystart = 0;
            int Yend = 0;
            int Zstart = 0;
            int Zend = 0;

            string principalAxis = "";
            if (Xmargin > Ymargin && Xmargin > Zmargin)
            {
                principalAxis = "X";
                for (int i = 0; i < EffframeCount - 1; i++)
                {
                    if (Xstart == 0 && m__G.oCam[0].mC_pX[i] < Xmin && m__G.oCam[0].mC_pX[i + 1] >= Xmin)
                        Xstart = i + 1;
                    if (Xstart > 0)
                    {
                        if (Xend == 0 && m__G.oCam[0].mC_pX[i] < Xmax && m__G.oCam[0].mC_pX[i + 1] >= Xmax)
                            Xend = i + 1;
                    }
                    if (Xstart > 0 && Xend > 0)
                        break;
                }
            }
            else if (Ymargin > Xmargin && Ymargin > Zmargin)
            {
                principalAxis = "Y";
                for (int i = 0; i < EffframeCount - 1; i++)
                {
                    if (Ystart == 0 && m__G.oCam[0].mC_pY[i] < Ymin && m__G.oCam[0].mC_pY[i + 1] >= Ymin)
                        Ystart = i + 1;
                    if (Ystart > 0)
                    {
                        if (Yend == 0 && m__G.oCam[0].mC_pY[i] < Ymax && m__G.oCam[0].mC_pY[i + 1] >= Ymax)
                            Yend = i + 1;
                    }
                    if (Ystart > 0 && Yend > 0)
                        break;
                }
            }
            else
            {
                principalAxis = "Z";
                for (int i = 0; i < EffframeCount - 1; i++)
                {
                    if (Zstart == 0 && m__G.oCam[0].mC_pZ[i] < Zmin && m__G.oCam[0].mC_pZ[i + 1] >= Zmin)
                        Zstart = i + 1;
                    if (Zstart > 0)
                    {
                        if (Zend == 0 && m__G.oCam[0].mC_pZ[i] < Zmax && m__G.oCam[0].mC_pZ[i + 1] >= Zmax)
                            Zend = i + 1;
                    }
                    if (Zstart > 0 && Zend > 0)
                        break;
                }
            }
            FAutoLearn.FZMath.Point2D[] luv = new FAutoLearn.FZMath.Point2D[EffframeCount];
            FAutoLearn.FZMath.Point2D[] luw = new FAutoLearn.FZMath.Point2D[EffframeCount];
            double slopeV = 0;
            double slopeW = 0;
            double offset = 0;

            if (principalAxis=="X")
            {
                for (int i = Xstart; i < Xend; i++)
                {
                    luv[i - Xstart].X = m__G.oCam[0].mC_pX[i];
                    luv[i - Xstart].Y = m__G.oCam[0].mC_pY[i];
                    luw[i - Xstart].X = m__G.oCam[0].mC_pX[i];
                    luw[i - Xstart].Y = m__G.oCam[0].mC_pZ[i];
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luv, Xend - Xstart, ref slopeV, ref offset);
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luw, Xend - Xstart, ref slopeW, ref offset);
                
                m__G.fVision.mPhiThetaPsi[0] = 0;
                m__G.fVision.mPhiThetaPsi[1] = Math.Atan(slopeW);
                m__G.fVision.mPhiThetaPsi[2] = -Math.Atan(slopeV);

            }
            else if (principalAxis == "Y")
            {
                for (int i = Ystart; i < Yend; i++)
                {
                    luv[i - Xstart].X = m__G.oCam[0].mC_pY[i];
                    luv[i - Xstart].Y = m__G.oCam[0].mC_pX[i];
                    luw[i - Xstart].X = m__G.oCam[0].mC_pY[i];
                    luw[i - Xstart].Y = m__G.oCam[0].mC_pZ[i];
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luv, Xend - Xstart, ref slopeV, ref offset);
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luw, Xend - Xstart, ref slopeW, ref offset);

                m__G.fVision.mPhiThetaPsi[0] = -Math.Atan(slopeW);
                m__G.fVision.mPhiThetaPsi[1] = 0;
                m__G.fVision.mPhiThetaPsi[2] = Math.Atan(slopeV);
            }
            else if (principalAxis == "Z")
            {
                for (int i = Xstart; i < Xend; i++)
                {
                    luv[i - Xstart].X = m__G.oCam[0].mC_pZ[i];
                    luv[i - Xstart].Y = m__G.oCam[0].mC_pX[i];
                    luw[i - Xstart].X = m__G.oCam[0].mC_pZ[i];
                    luw[i - Xstart].Y = m__G.oCam[0].mC_pY[i];
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luv, Xend - Xstart, ref slopeV, ref offset);
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(luw, Xend - Xstart, ref slopeW, ref offset);

                m__G.fVision.mPhiThetaPsi[0] = Math.Atan(slopeW);
                m__G.fVision.mPhiThetaPsi[1] = -Math.Atan(slopeV);
                m__G.fVision.mPhiThetaPsi[2] = 0;
            }
            m__G.oCam[0].mFAL.SetEulerAngle(m__G.fVision.mPhiThetaPsi);
            string eulerFile = m__G.m_RootDirectory + "\\DoNotTouch\\EulerAngle" + m__G.fVision.camID0 + ".txt";
            StreamWriter wr = new StreamWriter(eulerFile);
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[0].ToString("F8"));
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[1].ToString("F8"));
            wr.WriteLine(m__G.fVision.mPhiThetaPsi[2].ToString("F8"));
            wr.Close();

            m__G.oCam[0].mFAL.mFZM.TransferByEulerMatrix(EffframeCount, ref m__G.oCam[0].mC_pX, ref m__G.oCam[0].mC_pY, ref m__G.oCam[0].mC_pZ);
            m__G.fVision.UpdateEulerAngle();
        }
        public void StartTriggerTest(int frmCnt = 0)
        {
            mbStartTriggerTest = true;
            long triggeredTime = 0;

            SupremeTimer.QueryPerformanceCounter(ref triggeredTime);

            int waitTime = 0;
            while (true)
            {
                if (m__G.fVision.mLoaded)
                    break;
                Thread.Sleep(1);
                if (waitTime++ > 80)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                    mbStartTriggerTest = false;
                    return;
                }
            }
            waitTime = 0;

            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bEulerRotation;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23
            //m__G.fGraph.mDriverIC.AckSignal(0, false);
            while (true)
            {
                if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
                    break;
                Thread.Sleep(1);
                if (waitTime++ > 80)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartTriggeredGrab.Enabled = true;
                        });
                    }
                    m__G.mDoingStatus = "IDLE";
                    m__G.mIDLEcount = 0;
                    mbStartTriggerTest = false;
                    return;
                }
            }
            m__G.mDoingStatus = "Trigger Testing";
            //m__G.fVision.SetOrgExposure(0);
            //m__G.mDoingStatus = "Triggered Measure";
            m__G.oCam[0].mFAL.BackupFMI();

            if (frmCnt > 0)
                m__G.oCam[0].mTargetTriggerCount = frmCnt;

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////

            long lTimerFrequency = 1000;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            byte[] buffs = null;
            double timeForTrigger = 0;

            int lmaxThread = m__G.mMaxThread;
            if (frmCnt < 20)
                lmaxThread = 1;

            double frameRate = 0;
            int lgrabbedFrame = 0;
            double resTime = 0;

            Task taskVsData;
            string strlostTrigger = "";
            int loopCnt = 0;

            if (m__G.m_bNoHostPC)
                loopCnt = 0;

            double timeinterval = 0;
            long startTime = 0;
            long endTime = 0;
            long endTriggerTime = 0;

            ///////////////////////////////////////////////////////////////////////////////////
            //  Only for Internal Test Bench
            //int[] frmCntSequence = new int[8] { 4101, 1950, 3850, 2950, 2950, 1701, 1701, 4800 };   //  for 1Khz
            int[] frmCntSequence = new int[8] { 2050, 975, 1925, 1475, 1475, 850, 850, 2400 };   //  for 500Hz
            ///////////////////////////////////////////////////////////////////////////////////

            int MaxLoopCnt = int.Parse(tbConsecutiveTest.Text);
            Point[] markPos = null;
            string strAutoLastFrame = "";
            int triggergrabbedCnt = 0;
            bool HasAutoLastFrame = false;
            if (frmCnt > 0)
                MaxLoopCnt = 1;

            int requestedFrmCnt = frmCnt;

            while (loopCnt < MaxLoopCnt)
            {
                //if (loopCnt ==7 )
                //    lmaxThread = 12;

                strAutoLastFrame = "";
                //if (!m__G.m_bHideAllGraph) AddViewLog(">> Count down " + (loopCnt+1).ToString() + "/" + MaxLoopCnt.ToString() + "\r\n");
                //m__G.fGraph.mDriverIC.AckSignal(0, false);

                if (requestedFrmCnt == 0)
                {
                    Thread.Sleep(1500);
                    if ((loopCnt % 8 == 0) && (loopCnt > 0))
                        Thread.Sleep(2000);
                }

                ///////////////////////////////////////////////////////////////////////////////////
                //  Only for Internal Test Bench
                if (requestedFrmCnt == 0)
                    frmCnt = frmCntSequence[loopCnt % 8];
                ///////////////////////////////////////////////////////////////////////////////////


                //m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL, true);
                m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);

                //m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                //mViewLog1.Text += "Call ProcessVisionData\r\n";


                //                SupremeTimer.QueryPerformanceCounter(ref startTime);
                //  Thread 방식이 약간 시간 더걸림 10 msec 더 걸림.
                m__G.fVision.SetDefaultMarkConfig(false);

                SupremeTimer.QueryPerformanceCounter(ref startTime);


                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                for (int mi = 0; mi < 5; mi++)
                    m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 99];

                m__G.oCam[0].mTargetTriggerCount = frmCnt;
                m__G.oCam[0].mRequestedTriggerCount = frmCnt;
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////

                //mA_S_GrabBegin = Encoding.ASCII.GetBytes("A_S@\r\n");
                if (!m__G.m_bNoHostPC)
                {
                    if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_S Send, Trigger Waiting {0} Start\r\n", frmCnt));
                    m__G.oCam[0].ResetmCpXY();
                    Network.SendData(mA_S_GrabBegin);
                }

                m__G.oCam[0].SetSaveLostMarkFrame(m__G.m_bSaveLostTestSet);
                m__G.oCam[0].mFinishVisionData = true;
                //m__G.oCam[0].mFinishVisionData = false;
                SupremeTimer.QueryPerformanceCounter(ref startTime);

                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Trigger Capture Start Start\r\n"));

                m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);

                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Trigger Capture Start End\r\n"));
                if (!m__G.m_bHideAllGraph) AddViewLog("MatroxMsg " + m__G.oCam[0].mMatroxMsg);

                triggergrabbedCnt = lgrabbedFrame;
                //AddViewLog("MSG\t" + frmCnt.ToString() + "\t>>\t" + m__G.oCam[0].mMatroxMsg);
                //if (endTriggerTime != 0)
                //{
                //    timeinterval = (S2System.Vision.MILlib.mGrabTiming[0] - endTriggerTime) / (double)(lTimerFrequency);
                //    AddViewLog("\t\tTime Interval\t " + timeinterval.ToString("F3") + "\tsec\r\n");
                //}

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                endTriggerTime = endTime;

                resTime = (m__G.oCam[0].mBeforeTime - startTime) / (double)(lTimerFrequency);
                if (m__G.oCam[0].mMatroxMsg.Contains("Timeout 3"))
                {
                    //m__G.mbSuddenStop[0] = true;
                    //MessageBox.Show("m__G.mbSuddenStop[0] = true");
                    try
                    {
                        //////Network.SendData(mA_E_buffs);
                        AddViewLog("    A_E Trigger Timeout Error " + m__G.oCam[0].mMatroxMsg + "\r\n");
                        //SaveViewLog();
                    }
                    catch
                    {
                        AddViewLog("Network Error while sending A_E\r\n");
                    }
                }
                if (m__G.m_bAutoLastFrame)
                {
                    HasAutoLastFrame = false;
                    if (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
                    {
                        HasAutoLastFrame = true;
                        int framefromtrigger = lgrabbedFrame;
                        long modeChangeFrom = 0;
                        long modeChangeTo = 0;
                        SupremeTimer.QueryPerformanceCounter(ref modeChangeFrom);
                        m__G.fVision.CameraReset(2, true);
                        SupremeTimer.QueryPerformanceCounter(ref modeChangeTo);
                        while (lgrabbedFrame < m__G.oCam[0].mTargetTriggerCount)
                        {
                            m__G.oCam[0].GrabB(lgrabbedFrame, true);  //  milCommonImageGrab 가 아니라 다른데에 Grab 해야 한다. 수정 필요
                            lgrabbedFrame++;
                        }
                        m__G.fVision.CameraReset(2, false);
                        m__G.oCam[0].SetTriggeredframeCount(lgrabbedFrame);
                        resTime = (modeChangeTo - modeChangeFrom) / (double)(lTimerFrequency);

                        strAutoLastFrame = framefromtrigger.ToString();
                        //AddViewLog("\tStart Auto Last Frame from " + framefromtrigger.ToString() + " change took " + resTime.ToString("F4") + " sec\r\n");

                        if (m__G.oCam[0].mTargetTriggerCount > 50 && m__G.m_bSaveLostTestSet)
                        {
                            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\" + m__G.oCam[0].mTargetTriggerCount.ToString() + "\\";
                            if (!Directory.Exists(fileName))
                                Directory.CreateDirectory(fileName);
                            for (int imgIndex = 0; imgIndex < m__G.oCam[0].mTargetTriggerCount; imgIndex++)
                            {
                                string savefilename = fileName + "Ana" + imgIndex.ToString() + ".bmp";
                                m__G.oCam[0].SaveGrabbedImage(imgIndex, savefilename);
                            }
                        }
                    }
                }
                //AddViewLog("Process vision data " + lgrabbedFrame.ToString());

                //  Use Default Model by setting m__G.mFAL.mCandidateIndex = 0;
                m__G.mFAL.mCandidateIndex = 0;
                SupremeTimer.QueryPerformanceCounter(ref startTime);

                //    임시코드 - 테스트 하나에 집중해서 마크 모델이 유효하도록 조정해야함.
                //if (loopCnt < MaxLoopCnt - 1)
                //{
                //    loopCnt++;
                //    continue;
                //}
                //if (loopCnt == MaxLoopCnt - 1)    //    임시코드
                //AddViewLog("Processing .. \t");
                m__G.oCam[0].mFAL.mGotoLoopCount = 0;
                m__G.oCam[0].mFAL.mAccuShiftX = 0;
                m__G.oCam[0].mFAL.mAccuShiftY = 0;
                //lgrabbedFrame--;
                //m__G.fGraph.mDriverIC.AckSignal(0, true);
                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Vision Process Start\r\n"));
                if (lgrabbedFrame >= frmCnt)
                    m__G.fVision.ProcessVisionData(frmCnt, lmaxThread);
                else
                    m__G.fVision.ProcessVisionData(lgrabbedFrame, lmaxThread);
                if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("Vision Process End\r\n"));
                //m__G.fGraph.mDriverIC.AckSignal(0, false);

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                resTime = (endTime - startTime) / (double)(lTimerFrequency);

                int gotoCount = m__G.oCam[0].mFAL.mGotoLoopCount;
                int accShiftX = m__G.oCam[0].mFAL.mAccuShiftX;
                int accShiftY = m__G.oCam[0].mFAL.mAccuShiftY;
                //AddViewLog(lgrabbedFrame.ToString() + " frames with " + lmaxThread.ToString() + " threads" + " took " + resTime.ToString("F3") + " sec." + "GotoLoop Called " + gotoCount.ToString() + " accXS=" + accShiftX.ToString() + " accYS=" + accShiftY.ToString() +  "\r\n");
                //AddViewLog(lgrabbedFrame.ToString() + " frames with " + lmaxThread.ToString() + " threads" + " took " + resTime.ToString("F3") + " sec.\r\n");
                string strRes = "\t" + m__G.oCam[0].mTargetTriggerCount.ToString() + "\t>>\t" + triggergrabbedCnt.ToString() + "\t" + lgrabbedFrame.ToString() + " " + HasAutoLastFrame.ToString() + "\t" + resTime.ToString("F3") + " sec.\r\n";
                if (!m__G.m_bHideAllGraph) AddViewLog(strRes);

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                resTime = (endTime - startTime) / (double)(lTimerFrequency);
                timeForTrigger = (m__G.oCam[0].mCurTime - startTime) / (double)(lTimerFrequency);

                m__G.fVision.SetTriggerGrabbedFrame(lgrabbedFrame);
                m__G.fVision.SetTriggerGrabbedFPS(frameRate);

                if (m__G.mbSuddenStop[0])
                {
                    AddViewLog("m__G.mbSuddenStop[0] = " + m__G.mbSuddenStop[0].ToString() + "\r\n");
                    break;
                }

                m__G.fGraph.Drive_LEDs(0, 0);


                ///////////////////////////////////////////////////////////////////////////////////////
                //int EffframeCount = Math.Min(lgrabbedFrame, m__G.oCam[0].mTargetTriggerCount);
                int EffframeCount = m__G.oCam[0].mTargetTriggerCount;
                mCommonDataCount = EffframeCount;

                //  그래프 그리기 추가 필요, Thread.Sleep(500) 대신 그래프 그리기 할 것.
                if (cb1stLFP.Checked)
                {
                    for (int i = EffframeCount - 1; i > 0; i--)
                    {
                        m__G.oCam[0].mC_pTX[i] = (m__G.oCam[0].mC_pTX[i] + m__G.oCam[0].mC_pTX[i - 1]) / 2;
                        m__G.oCam[0].mC_pTY[i] = (m__G.oCam[0].mC_pTY[i] + m__G.oCam[0].mC_pTY[i - 1]) / 2;
                        m__G.oCam[0].mC_pTZ[i] = (m__G.oCam[0].mC_pTZ[i] + m__G.oCam[0].mC_pTZ[i - 1]) / 2;

                        m__G.oCam[0].mC_pX[i] = (m__G.oCam[0].mC_pX[i] + m__G.oCam[0].mC_pX[i - 1]) / 2;
                        m__G.oCam[0].mC_pY[i] = (m__G.oCam[0].mC_pY[i] + m__G.oCam[0].mC_pY[i - 1]) / 2;
                        m__G.oCam[0].mC_pZ[i] = (m__G.oCam[0].mC_pZ[i] + m__G.oCam[0].mC_pZ[i - 1]) / 2;
                    }
                }

                SupremeTimer.QueryPerformanceCounter(ref startTime);

                string lResultFile = "";

                //lResultFile = MyOwner.WriteResultBin(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                //lResultFile = MyOwner.WriteResultPos(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 

                //string strSaveAutoLastFrameTime = lResultFile.Substring(0, lResultFile.LastIndexOf('\\')) + "AccumALF.csv";
                //StreamWriter wwr = File.AppendText(strSaveAutoLastFrameTime);
                //wwr.WriteLine(lResultFile + "," + strAutoLastFrame);
                //wwr.Close();

                //m__G.fGraph.mDriverIC.AckSignal(0, true);

                //if ( !m__G.m_bNoHostPC)
                //    lResultFile = MyOwner.WriteResultBin(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                //else
                //    MyOwner.WriteResult(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 

                //mResultFileList.Add(lResultFile);
                //mResultFileList.Insert(0, lResultFile);
                //if (mResultFileList.Count > 100)
                //    mResultFileList.RemoveAt(100);

                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ltime = (endTime - startTime) / (double)(lTimerFrequency);

                double totalTime = (endTime - triggeredTime) / (double)(lTimerFrequency);

                // 다음은 자화에서 검증완료
                if (!m__G.m_bNoHostPC)
                {
                    try
                    {
                        byte[] sDatabuffer = MyOwner.MakeSaveResult();
                        int framCnt = m__G.fVision.GetTriggerGrabbedFrame();

                        Task.Factory.StartNew(() =>
                        {
                            byte[] sCmdBuf = null;
                            byte[] sRnBuf = null;
                            byte[] sendBuf = null;

                            //int framCnt = m__G.fVision.GetTriggerGrabbedFrame();
                            sCmdBuf = Encoding.ASCII.GetBytes("A_R@" + framCnt.ToString() + "@");
                            sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

                            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);

                            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);

                            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

                            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_R Send\r\n"));
                            Network.SendData(sendBuf);
                        });
                    }
                    catch
                    {
                        AddViewLog("Network Error while sending A_F\r\n");
                    }
                }
                /////////////////////////////////////////////////////////////////

                m_LastSampleNumber--;

                if (m_LastSampleNumber >= 0)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                        });
                    }
                    else
                    {
                        tbResidualTestNumber.Text = m_LastSampleNumber.ToString();
                    }
                }

                int leffFrameCnt = EffframeCount;

                if (!m__G.m_bHideAllGraph)
                {
                    Array.Copy(m__G.oCam[0].mC_pTX, mStroke[3], leffFrameCnt);
                    Array.Copy(m__G.oCam[0].mC_pTY, mStroke[4], leffFrameCnt);
                    Array.Copy(m__G.oCam[0].mC_pTZ, mStroke[5], leffFrameCnt);
                    Array.Copy(m__G.oCam[0].mC_pX, mStroke[0], leffFrameCnt);
                    Array.Copy(m__G.oCam[0].mC_pY, mStroke[1], leffFrameCnt);
                    Array.Copy(m__G.oCam[0].mC_pZ, mStroke[2], leffFrameCnt);

                    for (int az = 0; az < 12; az++)
                        for (int i = 0; i < leffFrameCnt; i++)
                            mPosAzimuth[az][i] = m__G.oCam[0].mAzimuthPts[i][az];

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            PlotMeasureData(leffFrameCnt);
                        });
                    }
                    else
                    {
                        PlotMeasureData(leffFrameCnt);
                    }
                }

                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////
                //  Cal with Second Mark Set
                //  Use Second Model by setting m__G.mFAL.mCandidateIndex = 1;
                for (int mi = 1; mi < m__G.mFAL.GetNumFMICandidate(); mi++)
                {
                    //////////////////////////////////////////////////////////////
                    /////   다중 모델 추적하기위한 모델 변경 관련 코드
                    //////////////////////////////////////////////////////////////
                    m__G.mFAL.mCandidateIndex = mi;
                    m__G.mFAL.mFZM.mbCompY = m__G.mFAL.mCandidateIndex;
                    m__G.fVision.ChangeFiducialMark(mi);

                    if (lgrabbedFrame >= frmCnt)
                        m__G.fVision.ProcessVisionData(frmCnt, lmaxThread);
                    else
                        m__G.fVision.ProcessVisionData(lgrabbedFrame, lmaxThread);

                    lResultFile = MyOwner.WriteResultBin(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                    //lResultFile = MyOwner.WriteResultPos(m__G.mFAL.mCandidateIndex); //  결과파일 Full Path 
                }
                m__G.mFAL.mFZM.mbCompY = 0;
                m__G.fVision.ChangeFiducialMark(0);

                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////
                ///


                if (m_LastSampleNumber < 0 && !m__G.m_bNoHostPC)
                {
                    if (!m__G.m_bHideAllGraph) AddViewLog("Out of loop at loopCnt=" + loopCnt.ToString() + "\r\n");
                    break;
                }

                loopCnt++;
            }
            if (m__G.mbSuddenStop[0])
            {
                AddViewLog("Stop Waiting Trigger. " + timeForTrigger.ToString("F3") + "sec Wait\r\n");
            }

            m__G.mFAL.mCandidateIndex = 0;
            m__G.fVision.ChangeFiducialMark(0);

            m__G.fGraph.Drive_LEDs(0, 0);
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mbSuddenStop[0] = false;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.
                });
            }
            else
                btnStartTriggeredGrab.Enabled = true;   //  새로운 StartTest 요청을 받을 수 있게 함.

            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            mbStartTriggerTest = false;
        }

        public void AddViewLog(string lstr)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (mViewLog1.Text.Length > 10000)
                        mViewLog1.Text = "";
                    DateTime dtNow = DateTime.Now;

                    mViewLog1.Text += "DummyID " + mStrDummyIndex + "\t" + string.Format("{0}, ", dtNow.ToString("yyyy-MM-dd HH:mm:ss.fff")) + lstr;
                    mViewLog1.SelectionStart = mViewLog1.Text.Length;
                    mViewLog1.ScrollToCaret();
                });
            }
            else
            {
                if (mViewLog1.Text.Length > 10000)
                    mViewLog1.Text = "";
                DateTime dtNow = DateTime.Now;

                mViewLog1.Text += "DummyID " + mStrDummyIndex + "\t" + string.Format("{0}, ", dtNow.ToString("yyyy-MM-dd HH:mm:ss.fff")) + lstr;
                mViewLog1.SelectionStart = mViewLog1.Text.Length;
                mViewLog1.ScrollToCaret();
            }
        }
        public void SaveViewLog()
        {
            DateTime dtNow = DateTime.Now;
            string sDir = m__G.m_RootDirectory + "\\Result";
            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);
            string filename = m__G.m_RootDirectory + "\\Result\\log\\Log" + dtNow.ToString("yyMMddHHmmss") + ".txt";
            StreamWriter wr = new StreamWriter(filename);
            wr.Write(mViewLog1.Text);
            wr.Close();
        }

        public void SettbUncalibratedInfoVisible(bool visible)
        {
            tbUncalibratedInfo.BeginInvoke(new Action(() => { tbUncalibratedInfo.Visible = visible; }));
        }

        public void StartContinuousTest(int frmCnt = 1)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart(); sw.Start();
            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("StartContinuousTest \r\n"));

            if (mbStartTriggerTest)
            {
                m__G.mbSuddenStop[0] = true;    //  StopTest Message 받으면 즉시 중단, 또는 fManage 에서 Stop Test Button 누르면 즉시 중단.
                m__G.oCam[0].mAbort = true;
            }

            while ( mbStartTriggerTest )
            {
                Thread.Sleep(1);
            }


            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt];

            m__G.mDoingStatus = "Continuous Measure";

            m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
            m__G.fVision.CameraReset(2, true);
            m__G.oCam[0].mTargetTriggerCount = frmCnt;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;

            m__G.fVision.SetDefaultMarkConfig(false);
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].PointTo6DMotion(-1, m__G.fVision.mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            Thread.Sleep(10);

            Network.SendData(mA_S_GrabBegin);
            if (!m__G.m_bHideAllGraph)
                AddViewLog(string.Format("A_S Send, Continues Waiting Start\r\n"));

            long startTime = 0;
            long endTime = 0;
            long lTimerFrequency = 0;
            
            SupremeTimer.QueryPerformanceCounter(ref startTime);
            
            for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
                m__G.oCam[0].GrabB(i);

            SupremeTimer.QueryPerformanceCounter(ref endTime);
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
            double ellapse = (endTime - startTime)/(double)lTimerFrequency;

            m__G.fGraph.Drive_LEDs(0, 0);
            //m__G.oCam[0].ResizeImgs(0, m__G.oCam[0].mTargetTriggerCount);

            m__G.oCam[0].dAFZM_FrameCount = frmCnt;

            int maxThread = m__G.mMaxThread;
            if (frmCnt < 20)
                maxThread = 1;

            m__G.mbSuddenStop[0] = false;
            //int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            //m__G.oCam[0].mTargetTriggerCount = 0;
            int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].SetTriggeredframeCount(frmCnt);
            
            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].ForceTriggerTime();
            
            m__G.fVision.ProcessVisionData(frmCnt, maxThread);
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;

            //m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;

            m__G.fVision.SetTriggerGrabbedFrame(frmCnt);

            string lResultFile = MyOwner.WriteResultBin(); //  결과파일 Full Path 

            //mResultFileList.Add(lResultFile);
            mResultFileList.Insert(0, lResultFile);
            if (mResultFileList.Count > 100)
                mResultFileList.RemoveAt(100);

            m__G.fVision.CameraReset(2, false);
            try
            {
                byte[] sDatabuffer = MyOwner.MakeSaveResult();
                //Task.Factory.StartNew(() =>
                //{
                    byte[] sCmdBuf = null;
                    byte[] sRnBuf = null;
                    byte[] sendBuf = null;

                    int framCnt = m__G.fVision.GetTriggerGrabbedFrame();
                    sCmdBuf = Encoding.ASCII.GetBytes("A_R@" + framCnt.ToString() + "@");
                    sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
                    sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];

                    Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);

                    Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);

                    Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

                    if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("A_R Send\r\n"));
                    Network.SendData(sendBuf);
                //});
            }
            catch
            {
                watchNetwork = 2;
            }



            if (!m__G.m_bHideAllGraph)
            {
                Array.Copy(m__G.oCam[0].mC_pTX, mStroke[3], frmCnt);
                Array.Copy(m__G.oCam[0].mC_pTY, mStroke[4], frmCnt);
                Array.Copy(m__G.oCam[0].mC_pTZ, mStroke[5], frmCnt);
                Array.Copy(m__G.oCam[0].mC_pX, mStroke[0], frmCnt);
                Array.Copy(m__G.oCam[0].mC_pY, mStroke[1], frmCnt);
                Array.Copy(m__G.oCam[0].mC_pZ, mStroke[2], frmCnt);

                for (int az = 0; az < 12; az++)
                    for (int i = 0; i < frmCnt; i++)
                        mPosAzimuth[az][i] = m__G.oCam[0].mAzimuthPts[i][az];

                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        PlotMeasureData(frmCnt);
                    });
                }else
                {
                    PlotMeasureData(frmCnt);
                }
            }
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mbSuddenStop[0] = false;

            if (!m__G.m_bHideAllGraph) AddViewLog(string.Format("StartContinuousTest end : {0} ms\r\n", sw.ElapsedMilliseconds));
        }

        public void PlotMeasureData(int frmCnt)
        {

            int[] selectedIndices1 = new int[listBox1.SelectedIndices.Count];
            int[] selectedIndices2 = new int[listBox2.SelectedIndices.Count];
            listBox1.SelectedIndices.CopyTo(selectedIndices1, 0);
            listBox2.SelectedIndices.CopyTo(selectedIndices2, 0);

            if (selectedIndices1.Length > 0 && selectedIndices2.Length > 0)
            {
                //PlotChartStroke(0, selectedIndices1, selectedIndices2, EffframeCount);
                Thread threadPlotStroke1 = new Thread(delegate () { PlotChartStroke(0, selectedIndices1, selectedIndices2, frmCnt); });
                threadPlotStroke1.Start();
            }

            int[] selectedIndices3 = new int[listBox3.SelectedIndices.Count];
            int[] selectedIndices4 = new int[listBox4.SelectedIndices.Count];
            listBox3.SelectedIndices.CopyTo(selectedIndices3, 0);
            listBox4.SelectedIndices.CopyTo(selectedIndices4, 0);

            if (selectedIndices3.Length > 0 && selectedIndices4.Length > 0)
            {
                //    PlotChartStroke(1, selectedIndices3, selectedIndices4, EffframeCount);
                Thread threadPlotStroke2 = new Thread(delegate () { PlotChartStroke(1, selectedIndices3, selectedIndices4, frmCnt); });
                threadPlotStroke2.Start();
            }

            //PlotChartStroke(2, listBox5.SelectedIndex, listBox6.SelectedIndex, -1, EffframeCount);
            int selectedIndices5 = 0;
            int selectedIndices6 = 0;
            selectedIndices5 = listBox5.SelectedIndex;
            selectedIndices6 = listBox6.SelectedIndex;
            Thread threadPlotStroke3 = new Thread(delegate () { PlotChartStroke(2, selectedIndices5, selectedIndices6, -1, frmCnt); });
            threadPlotStroke3.Start();

            //PlotChartMarkFromListBox7();
            Thread threadPlotStroke4 = new Thread(delegate () { PlotChartMarkFromListBox7(); });
            threadPlotStroke4.Start();
        }
        public void Process_Monitor(int port)
        {
            mProcessMonitorEscaped[port] = false;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    mInfoBtn[0].Hide();
                    mInfoBtn[0].Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                    mInfoBtn[0].ForeColor = Color.Transparent;
                    mInfoBtn[0].Text = "Start Test";
                });
            }
          
            int ch = port * 2;
            int k = 0;
            //int q = 0;
            int i = 0;
            //m__G.wrSytemLog("Call ClearDataResults(" + port.ToString() + "()");
            ClearDataResults(ch);
            ClearDataResults(ch + 1);

            m__G.wrSytemLog("Process_Monitor(" + port.ToString() + ")");
            //m__G.fManage.AddOperatorLog(ch, "Process_Monitor() start");
            while (true)
            {
                k = 0;
                while (MyOwner.IsTesting[port])
                {
                    Thread.Sleep(250);
                    k++;
                    //m__G.fManage.AddOperatorLog(ch + 1, " MyOwner.IsTesting[" + port.ToString() + "] : " + k.ToString());
                }

                //m__G.fManage.AddOperatorLog(ch + 1, " Process_start() Finish");

                if (mOldHistoryIndex[ch] == m__G.sCIndex[ch])
                {
                    mTestFinishState[ch] = 3;   //  Result : ND
                }
                else
                {
                    m_errsum[ch] = 0;
                    for (i = 0; i < m__G.sNUM_TESTITEM; i++)
                    {
                        m_errsum[ch] += m__G.iFailCountPerItem[ch, i];
                    }

                    if (m_errsum[ch] > m_olderrsum[ch])
                    {
                        mTestFinishState[ch] = 1;
                    }
                    else
                    {
                        mTestFinishState[ch] = 2;
                    }
                }

                if (mOldHistoryIndex[ch + 1] == m__G.sCIndex[ch + 1])
                {
                    mTestFinishState[ch + 1] = 3;   //  Result : ND
                }
                else
                {
                    m_errsum[ch + 1] = 0;
                    for (i = 0; i < m__G.sNUM_TESTITEM; i++)
                    {
                        m_errsum[ch + 1] += m__G.iFailCountPerItem[ch + 1, i];
                    }

                    if (m_errsum[ch + 1] > m_olderrsum[ch + 1])
                    {
                        mTestFinishState[ch + 1] = 1;
                    }
                    else
                    {
                        mTestFinishState[ch + 1] = 2;
                    }
                }

                string[] tmp = new string[2] { "", "" };


                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (m__G.m_ChannelOn[0])
                        {
                            if (m__G.errMsg[0] == "")
                            {
                                mInfoBtn[0].Text = "PASS";
                                //mConsecutiveFail[ch + i, 9] = false;
                                mInfoBtn[0].Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                                mInfoBtn[0].ForeColor = Color.Cyan;
                            }
                            else
                            {
                                mInfoBtn[0].Text = m__G.errMsg[0];
                                //mConsecutiveFail[ch + i, 9] = true;
                                mInfoBtn[0].Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                                mInfoBtn[0].ForeColor = Color.OrangeRed;
                            }
                        }
                    });
                }


                //for (q = 0; q < 9; q++)
                //{
                //    mConsecutiveFail[ch + i, q] = mConsecutiveFail[ch + i, q + 1];
                //    mConsecutiveFail[ch + i, q] = mConsecutiveFail[ch + i, q + 1];
                //}
                //mInfoBtn[ch + i].Show();
                if (MyOwner.m_StopRepeat[port]) break;
            }
            m__G.fManage.AddOperatorLog(ch, "Process_Monitor() escaped", true);

            /////////////////////////////////////////////////////////////
            mChangeViewLog(0);
            mChangeViewLog(1);

            SaveLogNClear(ch);
            SaveLogNClear(ch + 1);

            CalcShowYield();
            /////////////////////////////////////////////////////////////
            //for (i = 0; i < 2; i++)
            //{
            //    for (q = 0; q < 10; q++)
            //        if (!mConsecutiveFail[ch + i, q]) break;

            //    if (q == 10)
            //        mInfoBtn[ch + i].Text += "\r\nConsecutive 10ea Fail CHECK System !!!";
            //}

            mProcessMonitorEscaped[port] = true;

            if (mProcessMonitorEscaped[0] && mProcessMonitorEscaped[1])
            {

                int maxIndex = Math.Max(m__G.sCIndex[0], m__G.sCIndex[1]);
                if (F_Main.MachineType != (int)MachineType.Master)
                {
                    maxIndex = Math.Max(maxIndex, m__G.sCIndex[2]);
                    maxIndex = Math.Max(maxIndex, m__G.sCIndex[3]);
                }

                tbResidualTestNumber.Text = maxIndex.ToString();
                tbConsecutiveTest.Text = (maxIndex + 1).ToString();

            }

            if (m__G.m_bScreenCapture)
            {
                DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
                string pngname = "SC_" + dtNow.ToString("MM-dd HH/mm/ss");

                if (m__G.m_ChannelOn[0])
                    pngname += "_" + m__G.sCIndex[0].ToString(); 
                if (m__G.m_ChannelOn[1])
                    pngname += "_" + m__G.sCIndex[1].ToString();

                if (F_Main.CurrentInspPosBlock != "Unknown" || F_Main.CurrentInspPosSocket != "Unknown")
                    pngname += "_" + F_Main.CurrentInspPosBlock + "_" + F_Main.CurrentInspPosSocket;


                pngname += ".png";

                string sScreenCapturePath = m__G.m_RootDirectory + "\\Result\\ScreenCapture\\" + pngname;
                string sDir = m__G.m_RootDirectory + "\\Result\\ScreenCapture";
                Bitmap memoryImage;
                memoryImage = new Bitmap(1906, 1080);
                Size s = new Size(memoryImage.Width, memoryImage.Height);
                Graphics memoryGraphics = Graphics.FromImage(memoryImage);


                if (!Directory.Exists(sDir))
                    Directory.CreateDirectory(sDir);

                Thread.Sleep(300);
                memoryGraphics.CopyFromScreen(7, 31, 0, 0, s);
                memoryImage.Save(sScreenCapturePath);
            }

            if (F_Main.MachineType == (int)MachineType.Slave)
            {
                string tmpstr = string.Empty;
                tmpstr = mInfoBtn[ch].Text;// + "\t" + mInfoBtn[ch + 1].Text;
                PC2SendData("RBT", tmpstr, tmpstr.Length);
            }
            this.Invoke(new MethodInvoker(
                 delegate ()
                 {
                     mInfoBtn[ch].Show();
                     //mInfoBtn[ch + 1].Show();
                 })
             );
        }

        public string GetLotName()
        {

            return tbLotName.Text;
        }

        public void ClearAllDataResults(int port = 0)
        {
        }

        public void ClearDataResults(int ch)
        {
        }

        public void AddExtendedResult(int ch, int failitemID, int lHistIndex, bool IsPass = true)
        {
            if (!IsPass)
            {
                m__G.iFailCountPerItem[ch, failitemID]++;
                m__G.iFailCountPerItemAll[failitemID]++;
                m__G.sHistArray[lHistIndex, (int)Global.SpecItem.PassFail] = (failitemID + 1) + m__G.sHistArray[lHistIndex, (int)Global.SpecItem.PassFail] * 100;  //  0번 불량 시 번호가 없어지는 것을 방지하기 위해 1 더한다.
            }
        }

        public void ShowCPKResults(int cpkIndex)
        {
            this.Invoke(new MethodInvoker(
                            delegate ()
                            {
                                int i = 0;
                                int effRowNum = 0;

                                for (i = 0; i < m__G.sNUM_TESTITEM; i++)
                                {
                                    if (m__G.mTestItem[i, 10].Contains('t') || m__G.mTestItem[i, 10].Contains('T'))
                                    {
                                        effRowNum = m__G.mTestItemToGrid[i];
                                    }
                                }
                            })
                        );
        }

        //  나중에 사용할지 몰라서 코드 유지 181109
        //public void SaveScreenShot(int number)
        //{
        //    int n = number % 5;
        //    string pngname = "InstSC_" + n.ToString() + ".png";
        //    string sScreenCapturePath = m__G.m_RootDirectory + "\\RunData\\" + pngname;

        //    Bitmap memoryImage;
        //    memoryImage = new Bitmap(1326, 1044);
        //    Size s = new Size(memoryImage.Width, memoryImage.Height);
        //    Graphics memoryGraphics = Graphics.FromImage(memoryImage);
        //    memoryGraphics.CopyFromScreen(584, 34, 0, 0, s);
        //    memoryImage.Save(sScreenCapturePath);
        //}

        //  나중에 사용할지 몰라서 코드 유지 181109
        //public void ShowScreenShot(int number)
        //{
        //    string pngname;
        //    string sScreenCapturePath;
        //    if (number < 0)
        //    {
        //        pngname = "InstSC_Default.png";
        //        sScreenCapturePath = m__G.m_RootDirectory + "\\RunData\\" + pngname;
        //        if (!File.Exists(sScreenCapturePath)) return;
        //    }
        //    else
        //    {
        //        int n = number % 5;
        //        pngname = "InstSC_" + n.ToString() + ".png";
        //        sScreenCapturePath = m__G.m_RootDirectory + "\\RunData\\" + pngname;
        //        if (!File.Exists(sScreenCapturePath)) return;
        //    }
        //}

        public void InitializeChart()
        {
            if (mychart == null)
            {
                mychart = new Chart[4];

                mychart[0] = chart1;    //  Top
                mychart[1] = chart2;        //  Btm
                mychart[2] = chart3;    //  Top
                mychart[3] = chart4;        //  Btm
            }

            //Color Linen = Color.FromName("Linen");
            //Color LavenderBlush = Color.FromName("LightCyan");
            Color LightGray = Color.FromName("LightGray");
            Color DarkGray = Color.FromName("DarkGray");

            //Color DarkOrange = Color.FromName("DarkOrange");
            //Color crimson = Color.FromName("crimson");
            //Color Red = Color.FromName("red");
            //Color firebrick = Color.FromName("firebrick");
            //Color tomato = Color.FromName("tomato");

            //Color DodgerBlue = Color.FromName("DodgerBlue");
            //Color Blue = Color.FromName("blue");
            //Color navy = Color.FromName("navy");
            //Color mediumblue = Color.FromName("mediumblue");

            int i = 0;
            for (i = 0; i < 4; i++)
            {
                mychart[i].ChartAreas[0].Position.X = 0;
                mychart[i].ChartAreas[0].Position.Y = 0;
                mychart[i].ChartAreas[0].Position.Height = 99;
                mychart[i].ChartAreas[0].Position.Width = 100;

                mychart[i].Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                mychart[i].ChartAreas[0].AxisX.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                mychart[i].ChartAreas[0].AxisY.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                mychart[i].ChartAreas[0].AxisX.ScaleView.Position = 0;
                mychart[i].ChartAreas[0].AxisY.ScaleView.Position = 0;

                mychart[i].ChartAreas[0].AxisX.MajorGrid.LineColor = LightGray;
                mychart[i].ChartAreas[0].AxisY.MajorGrid.LineColor = LightGray;

                mychart[i].ChartAreas[0].AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
                mychart[i].ChartAreas[0].AxisY.MinorGrid.LineColor = Color.WhiteSmoke;
                mychart[i].ChartAreas[0].AxisX.LineColor = DarkGray;
                mychart[i].ChartAreas[0].AxisY.LineColor = DarkGray;

                mychart[i].BackColor = System.Drawing.SystemColors.ControlLightLight;
                //mychart[i].BackColor = System.Drawing.SystemColors.ControlDarkDark;

                if (i<2)
                {
                    String tmp =  "Time ";
                    mychart[i].ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                    mychart[i].ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    mychart[i].ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    mychart[i].ChartAreas[0].AxisY.Minimum = -6000;
                    mychart[i].ChartAreas[0].AxisY.Maximum = 6000;
                    mychart[i].ChartAreas[0].AxisY.Interval = 1000;
                    mychart[i].ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                    mychart[i].ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.OrangeRed;
                    mychart[i].Series[6].YAxisType = AxisType.Secondary;
                    mychart[i].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
                    mychart[i].ChartAreas[0].AxisX.IsLabelAutoFit = false;
                    mychart[i].ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                    mychart[i].ChartAreas[0].AxisY.LabelStyle.Format = "0";
                    mychart[i].ChartAreas[0].AxisY2.LabelStyle.Format = "0";
                    mychart[i].Titles.Clear();
                    mychart[i].Titles.Add(tmp + " vs Any");
                    mychart[i].Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                }
                else
                {
                    String tmp = "Any ";
                    mychart[i].ChartAreas[0].AxisY.MinorGrid.Interval = 1;
                    mychart[i].ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    mychart[i].ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    mychart[i].ChartAreas[0].AxisX.Minimum = 0;
                    mychart[i].ChartAreas[0].AxisX.Maximum = 16400;
                    mychart[i].ChartAreas[0].AxisX.Interval = 2000;
                    mychart[i].ChartAreas[0].AxisY.Minimum = -30;
                    mychart[i].ChartAreas[0].AxisY.Maximum = 30;
                    mychart[i].ChartAreas[0].AxisY.Interval = 6;
                    mychart[i].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.0}";     //
                    mychart[i].ChartAreas[0].AxisY.LabelStyle.Format = "0";
                    mychart[i].ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                    mychart[i].Titles.Clear();
                    mychart[i].Titles.Add(tmp + " vs Any");
                    mychart[i].Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                }

                mychart[i].Series.Clear();
            }
            int[] numSeries = new int[8];
            int[] LnumSeries = new int[8];
            int[] chIndex = new int[4] { 0, 1, 2,3};
            string[] seriesHead = new string[4] { "Time to ", "Time to ", "Any to ", "Any to " };
            string lHead = "";
            i = 0;
            for (int w = 0; w < 4; w++)
            {
                SeriesChartType ltype = SeriesChartType.FastLine;
                if (w == 3)
                    ltype = SeriesChartType.FastPoint;

                i = chIndex[w];
                lHead = seriesHead[w];
                //  Stroke, Current, Cross Stroke
                mychart[i].Series.Add(lHead + "X");
                mychart[i].Series[numSeries[i]].Label = lHead + "X";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Red;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = true;
                numSeries[i]++; //  1
                mychart[i].Series.Add(lHead + "Y");
                mychart[i].Series[numSeries[i]].Label = lHead + "Y";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.OrangeRed;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                numSeries[i]++; //  2
                mychart[i].Series.Add(lHead + "Z");
                mychart[i].Series[numSeries[i]].Label = lHead + "Z";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Orange;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                numSeries[i]++; //  3
                mychart[i].Series.Add(lHead + "TX");
                mychart[i].Series[numSeries[i]].Label = lHead + "TX";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Orange;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                numSeries[i]++; //  4
                mychart[i].Series.Add(lHead + "TY");
                mychart[i].Series[numSeries[i]].Label = lHead + "TY";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.LightPink;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  5
                mychart[i].Series.Add(lHead + "TZ");
                mychart[i].Series[numSeries[i]].Label = lHead + "TZ";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.LightPink;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = true;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  6
                mychart[i].Series.Add("sN1");
                mychart[i].Series[numSeries[i]].Label = "side N1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = true;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  7
                mychart[i].Series.Add("sN2");
                mychart[i].Series[numSeries[i]].Label = "side N2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = true;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  8
                mychart[i].Series.Add("sW1");
                mychart[i].Series[numSeries[i]].Label = "side W1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  9
                mychart[i].Series.Add("sW2");
                mychart[i].Series[numSeries[i]].Label = "side W2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Coral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  10
                mychart[i].Series.Add("sS1");
                mychart[i].Series[numSeries[i]].Label = "side S1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.LightCoral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  11
                mychart[i].Series.Add("sS2");
                mychart[i].Series[numSeries[i]].Label = "side S2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  12
                mychart[i].Series.Add("sE1");
                mychart[i].Series[numSeries[i]].Label = "side E1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Coral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  13
                mychart[i].Series.Add("sE2");
                mychart[i].Series[numSeries[i]].Label = "side E2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.LightCoral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  14
                mychart[i].Series.Add("tN1");
                mychart[i].Series[numSeries[i]].Label = "top N1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = true;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  15
                mychart[i].Series.Add("tN2");
                mychart[i].Series[numSeries[i]].Label = "top N2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.SlateBlue;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  16
                mychart[i].Series.Add("tS1");
                mychart[i].Series[numSeries[i]].Label = "top S1";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.Coral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
                numSeries[i]++; //  17
                mychart[i].Series.Add("tS2");
                mychart[i].Series[numSeries[i]].Label = "top S2";
                mychart[i].Series[numSeries[i]].ChartType = ltype;
                mychart[i].Series[numSeries[i]].Color = Color.LightCoral;
                mychart[i].Series[numSeries[i]].IsVisibleInLegend = false;
                mychart[i].Series[numSeries[i]].YAxisType = AxisType.Secondary;
            }
        }
        public void ClearGraph(int port = 0)
        {
            double[] tmp = new double[2];

            PlotChartStroke(0, 0, 0, 0, 0);
            PlotChartStroke(1, 0, 0, 0, 0);
            PlotChartStroke(2, 0, 0, 0, 0);
            PlotChartMark(3, null, 0);
        }

        private void AllChart_MouseWheel(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                if (m_bChartWheel[i])
                {
                    if (i % 2 == 0)
                        _topMouseWheel(i, e);
                    else
                        _btmMouseWheel(i, e);
                    break;
                }
            }
        }
        private void _topMouseWheel(int ch, MouseEventArgs e)
        {
            mychart[ch].Focus();
            try
            {
                if (e.Delta < 0)
                {
                    mychart[ch].ChartAreas[0].AxisX.Minimum = 0;
                    mychart[ch].ChartAreas[0].AxisX.Maximum = 4096;
                    mychart[ch].ChartAreas[0].AxisX.Interval = 400;
                    mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = 100;
                    mychart[ch].ChartAreas[0].AxisX.MinorGrid.Enabled = false;

                    mychart[ch].ChartAreas[0].AxisY2.Minimum = 0;
                    mychart[ch].ChartAreas[0].AxisY2.Maximum = 160;
                    mychart[ch].ChartAreas[0].AxisY2.Interval = 30;
                    mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = 30;

                    mychart[ch].ChartAreas[0].AxisY.Minimum = -4;
                    mychart[ch].ChartAreas[0].AxisY.Maximum = 4;
                    mychart[ch].ChartAreas[0].AxisY.Interval = 1;
                    mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = 1;
                    mychart[ch].ChartAreas[0].AxisY.MinorGrid.Interval = 0.2;
                    mychart[ch].ChartAreas[0].AxisY.MinorGrid.Enabled = true;

                    mychart[ch].ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    //mychart[ch].ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    mychart[ch].ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9);
                    mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    mychart[ch].ChartAreas[0].AxisY2.MinorGrid.Enabled = false;
                    mychart[ch].Series[6].YAxisType = AxisType.Secondary;

                    mychart[ch].ChartAreas[0].AxisX.ScaleView.ZoomReset();
                    mychart[ch].ChartAreas[0].AxisY.ScaleView.ZoomReset();
                    mychart[ch].ChartAreas[0].AxisY2.ScaleView.ZoomReset();
                }

                if (e.Delta > 0)
                {
                    mychart[ch].ChartAreas[0].AxisX.ScrollBar.Enabled = false;
                    mychart[ch].ChartAreas[0].AxisY.ScrollBar.Enabled = false;
                    mychart[ch].ChartAreas[0].AxisY2.ScrollBar.Enabled = false;
                    double xMin = mychart[ch].ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                    double xMax = mychart[ch].ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                    double yMin = mychart[ch].ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                    double yMax = mychart[ch].ChartAreas[0].AxisY.ScaleView.ViewMaximum;
                    double y2Min = mychart[ch].ChartAreas[0].AxisY2.ScaleView.ViewMinimum;
                    double y2Max = mychart[ch].ChartAreas[0].AxisY2.ScaleView.ViewMaximum;

                    double posXStart = (mychart[ch].ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) - (xMax - xMin) * 9.5 / 20);
                    double posXFinish = (mychart[ch].ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) + (xMax - xMin) * 9.5 / 20);
                    double posYStart = (int)(mychart[ch].ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) - (yMax - yMin) * 9.5 / 20);
                    double posYFinish = (int)(mychart[ch].ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) + (yMax - yMin) * 9.5 / 20);
                    double posY2Start = (int)(mychart[ch].ChartAreas[0].AxisY2.PixelPositionToValue(e.Location.Y) - (y2Max - y2Min) * 9.5 / 20);
                    double posY2Finish = (int)(mychart[ch].ChartAreas[0].AxisY2.PixelPositionToValue(e.Location.Y) + (y2Max - y2Min) * 9.5 / 20);

                    if (Math.Abs(posYStart - posYFinish) > 0)
                    {
                        mychart[ch].ChartAreas[0].AxisX.ScaleView.Zoom(posXStart, posXFinish);
                        mychart[ch].ChartAreas[0].AxisY.ScaleView.Zoom(posYStart, posYFinish);
                        mychart[ch].ChartAreas[0].AxisY2.ScaleView.Zoom(posY2Start, posY2Finish);

                        mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = Math.Abs(posXStart - posXFinish) / 10;
                        mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = Math.Abs(posYStart - posYFinish) / 10;
                        mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = Math.Abs(posY2Start - posY2Finish) / 10;

                        mychart[ch].ChartAreas[0].AxisX.Interval = mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval;
                        mychart[ch].ChartAreas[0].AxisY.Interval = mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval;
                        mychart[ch].ChartAreas[0].AxisY2.Interval = mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval;
                    }
                }
            }
            catch { }
        }
        private void _btmMouseWheel(int ch, MouseEventArgs e)
        {
            mychart[ch].Focus();
            try
            {
                if (e.Delta < 0)
                {
                    mychart[ch].ChartAreas[0].AxisX.MinorGrid.Interval = 1;
                    mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = 1;
                    mychart[ch].ChartAreas[0].AxisX.LabelStyle.Interval = 1;
                    mychart[ch].ChartAreas[0].AxisX.Minimum = 10;
                    mychart[ch].ChartAreas[0].AxisX.Maximum = 2000;

                    mychart[ch].ChartAreas[0].AxisY.Minimum = -60;
                    mychart[ch].ChartAreas[0].AxisY.Maximum = 40;
                    mychart[ch].ChartAreas[0].AxisY.Interval = 20;
                    mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = 20;
                    mychart[ch].ChartAreas[0].AxisY.MinorGrid.Interval = 2;

                    mychart[ch].ChartAreas[0].AxisY2.Minimum = -270;
                    mychart[ch].ChartAreas[0].AxisY2.Maximum = 180;
                    mychart[ch].ChartAreas[0].AxisY2.LabelStyle.Interval = 45;
                    mychart[ch].ChartAreas[0].AxisY2.Interval = 45;
                    mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = 45;

                    mychart[ch].ChartAreas[0].AxisX.ScaleView.ZoomReset();
                    mychart[ch].ChartAreas[0].AxisY.ScaleView.ZoomReset();
                    mychart[ch].ChartAreas[0].AxisY2.ScaleView.ZoomReset();
                }

                if (e.Delta > 0)
                {
                    mychart[ch].ChartAreas[0].AxisX.ScrollBar.Enabled = false;
                    mychart[ch].ChartAreas[0].AxisY.ScrollBar.Enabled = false;
                    mychart[ch].ChartAreas[0].AxisY2.ScrollBar.Enabled = false;
                    double xMin = mychart[ch].ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                    double xMax = mychart[ch].ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                    double yMin = mychart[ch].ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                    double yMax = mychart[ch].ChartAreas[0].AxisY.ScaleView.ViewMaximum;
                    double y2Min = mychart[ch].ChartAreas[0].AxisY2.ScaleView.ViewMinimum;
                    double y2Max = mychart[ch].ChartAreas[0].AxisY2.ScaleView.ViewMaximum;

                    double posXStart = (mychart[ch].ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) - (xMax - xMin) * 9.5 / 20);
                    double posXFinish = (mychart[ch].ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) + (xMax - xMin) * 9.5 / 20);
                    double posYStart = (int)(mychart[ch].ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) - (yMax - yMin) * 9.5 / 20);
                    double posYFinish = (int)(mychart[ch].ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) + (yMax - yMin) * 9.5 / 20);
                    double posY2Start = (int)(mychart[ch].ChartAreas[0].AxisY2.PixelPositionToValue(e.Location.Y) - (y2Max - y2Min) * 9.5 / 20);
                    double posY2Finish = (int)(mychart[ch].ChartAreas[0].AxisY2.PixelPositionToValue(e.Location.Y) + (y2Max - y2Min) * 9.5 / 20);

                    if (Math.Abs(posYStart - posYFinish) > 0)
                    {
                        mychart[ch].ChartAreas[0].AxisX.ScaleView.Zoom(posXStart, posXFinish);
                        mychart[ch].ChartAreas[0].AxisY.ScaleView.Zoom(posYStart, posYFinish);
                        mychart[ch].ChartAreas[0].AxisY2.ScaleView.Zoom(posY2Start, posY2Finish);

                        mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = Math.Abs(posXStart - posXFinish) / 10;
                        mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = Math.Abs(posYStart - posYFinish) / 10;
                        mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = Math.Abs(posY2Start - posY2Finish) / 10;

                        mychart[ch].ChartAreas[0].AxisX.Interval = mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval;
                        mychart[ch].ChartAreas[0].AxisY.Interval = mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval;
                        mychart[ch].ChartAreas[0].AxisY2.Interval = mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval;
                    }
                }
            }
            catch { }
        }

        public void PlotChartStroke(int ch,int[] yItems, int[] y2Items, int count)
        {
            if (mychart[ch].InvokeRequired)
            {
                mychart[ch].Invoke(new MethodInvoker(delegate ()
                {
                    _topPlotChart(ch, yItems, y2Items, count);
                }));
            }
            else
                _topPlotChart(ch, yItems, y2Items, count);
        }
        public void PlotChartStroke(int ch, int xItem, int yItem, int y2Item, int count)
        {
            if (mychart[ch].InvokeRequired)
            {
                mychart[ch].Invoke(new MethodInvoker(delegate ()
                {
                    _topPlotChart(ch, xItem, yItem, y2Item, count);
                }));
            }
            else
                _topPlotChart(ch, xItem, yItem, y2Item, count);
        }
        public void PlotChartMark(int ch, int[] items, int count)
        {
            if (mychart[ch].InvokeRequired)
            {
                mychart[ch].Invoke(new MethodInvoker(delegate ()
                {
                    _locusPlotChart(ch, items,  count);
                }));
            }
            else
                _locusPlotChart(ch, items, count);
        }

        public void _topPlotChart(int ch, int xItem, int yItem, int y2Item, int count)
        {
            int chartID = ch; // mychart[0,1,4,5]
            while (m_FlagChart[chartID])
                Thread.Sleep(10);

            m_FlagChart[chartID] = true;

            mychart[chartID].Series.Clear();


            if (count == 0)
            {
                mychart[chartID].Series.Add("Default");
                mychart[chartID].Series[0].Points.AddXY(1, 0);
                m_FlagChart[chartID] = false;
                return;
            }
            //  mTime = new double[10000];
            //  mXstroke = new double[10000];     //  Code Output Time
            //  mYstroke = new double[10000];     //  Code Output Time
            //  mZstroke = new double[10000];     //  Code Output Time
            //  mTXstroke = new double[10000];     //  Code Output Time
            //  mTYstroke = new double[10000];     //  Code Output Time
            //  mTZstroke = new double[10000];     //  Code Output Time

            //  mtN1 = new Point[10000];     //  1 -> 0.1um
            //  mtN2 = new Point[10000];     //  1 -> 0.1um
            //  mtW1 = new Point[10000];     //  1 -> 0.1um
            //  mtW2 = new Point[10000];     //  1 -> 0.1um
            //  mtS1 = new Point[10000];     //  1 -> 0.1um
            //  mtS2 = new Point[10000];     //  1 -> 0.1um
            //  mtE1 = new Point[10000];     //  1 -> 0.1um
            //  mtE2 = new Point[10000];     //  1 -> 0.1um

            //  msN1 = new Point[10000];     //  1 -> 0.1um
            //  msN2 = new Point[10000];     //  1 -> 0.1um
            //  msS1 = new Point[10000];     //  1 -> 0.1um
            //  msS2 = new Point[10000];     //  1 -> 0.1um

            //double scaleTop = 1;
            //double scaleSide = 1;
            //double rotTop = 0;
            //double rotSide = 0;

            //m__G.oCam[0].GetScaleNOpticalR(ref scaleTop, ref scaleSide, ref rotTop, ref rotSide);

            double umscale = 5.5 / Global.LensMag;// * ((scaleTop + scaleSide) / 2);       //  Pixel to um
            double minscale = 180 / Math.PI * 60;                           //  rad to min

            double y1Inverval = 1;
            double y2Inverval = 1;
            double xMin = 0;
            double xMax = 0;
            double yMin = 0;
            double yMax = 0;
            double y2Min = 0;
            double y2Max = 0;
            double gMin = 0;
            double gMax = 0;
            double gInterval = 0;
            int debugIndex = 0;

            if (xItem == -1)
            {
                try
                {
                    //  Time To Stroke
                    double[] lArray = new double[count];
                    Array.Copy(mStroke[yItem], lArray, count);
                    Array.Sort(lArray);
                    yMin = lArray[0];
                    yMax = lArray[count - 1];

                    if (y2Item >= 0)
                    {
                        Array.Copy(mStroke[y2Item], lArray, count);
                        Array.Sort(lArray);
                    }
                    y2Min = lArray[0];
                    y2Max = lArray[count - 1];

                    debugIndex = 1;
                    mychart[chartID].Series.Add(mStrStroke[yItem]);
                    mychart[chartID].Series[0].ChartType = SeriesChartType.FastLine;
                    mychart[chartID].Series[0].MarkerSize = 0;
                    mychart[chartID].Series[0].Color = mLineColor[3];

                    debugIndex = 2;
                    double curScale = umscale;
                    if (yItem > 2)
                        curScale = minscale;

                    for (int i = 0; i < count; i++)
                        mychart[chartID].Series[0].Points.AddXY(i / 1000.0, mStroke[yItem][i] * curScale); //  FWD

                    double curScale2 = umscale;
                    if (y2Item >= 0 && y2Item != yItem)
                    {
                        if (y2Item > 2)
                            curScale2 = minscale;

                        mychart[chartID].Series.Add(mStrStroke[y2Item]);
                        mychart[chartID].Series[1].ChartType = SeriesChartType.FastLine;
                        mychart[chartID].Series[1].MarkerSize = 0;
                        mychart[chartID].Series[1].Color = mLineColor[5];
                        for (int i = 0; i < count; i++)
                            mychart[chartID].Series[1].Points.AddXY(i / 1000.0, mStroke[y2Item][i] * curScale2); //  FWD

                        mychart[chartID].Series[1].YAxisType = AxisType.Secondary;
                    }
                    debugIndex = 3;

                    double xInterval = 0.1;
                    if (count < 200)
                        xInterval = 0.01;
                    else if (count < 500)
                        xInterval = 0.02;
                    else if (count < 2000)
                        xInterval = 0.1;
                    else if (count < 5000)
                        xInterval = 0.2;
                    else if (count <= 10000)
                        xInterval = 0.5;
                    else
                        xInterval = 1.0;

                    mychart[chartID].ChartAreas[0].AxisX.Minimum = 0;
                    mychart[chartID].ChartAreas[0].AxisX.Maximum = (count / 1000.0);
                    mychart[chartID].ChartAreas[0].AxisX.Interval = xInterval;

                    string strMsg = chartID.ToString() + "]\t Y1 : ";
                    debugIndex = 4;

                    AutoChartGrid(yMin * curScale, yMax * curScale, ref gMin, ref gMax, ref gInterval);
                    debugIndex = 5;
                    strMsg += yMin.ToString("F3") + " - " + yMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2");
                    mychart[chartID].ChartAreas[0].AxisY.Minimum = gMin;
                    mychart[chartID].ChartAreas[0].AxisY.Maximum = gMax;
                    mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;
                    y1Inverval = gInterval;
                    mychart[chartID].Titles.Clear();
                    debugIndex = 6;

                    if (y2Item >= 0)
                    {
                        AutoChartGrid(y2Min * curScale2, y2Max * curScale2, ref gMin, ref gMax, ref gInterval);
                        strMsg += "\t\t Y2 : " + y2Min.ToString("F3") + " - " + y2Max.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2") + "\r\n";
                        mychart[chartID].ChartAreas[0].AxisY2.Minimum = gMin;
                        mychart[chartID].ChartAreas[0].AxisY2.Maximum = gMax;
                        mychart[chartID].ChartAreas[0].AxisY2.Interval = gInterval;
                        y2Inverval = gInterval;
                        mychart[chartID].Titles.Add("Time vs " + mStrStroke[yItem] + " / " + mStrStroke[y2Item]);
                    }
                    else
                        mychart[chartID].Titles.Add("Time vs " + mStrStroke[yItem]);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString() + "\r\n debugIndex = " + debugIndex.ToString());
                }
                //mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;

                //if (InvokeRequired)
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        mViewLog1.Text += strMsg;
                //    });
                //}
                //else
                //    mViewLog1.Text += strMsg;
            }
            else
            {
                //  Stroke to Stroke
                try
                {
                    double[] lArray = new double[count];
                    Array.Copy(mStroke[xItem], lArray, count);
                    Array.Sort(lArray);
                    xMin = lArray[0];
                    xMax = lArray[count - 1];
                    Array.Copy(mStroke[yItem], lArray, count);
                    Array.Sort(lArray);
                    yMin = lArray[0];
                    yMax = lArray[count - 1];
                    debugIndex = 10;
                    mychart[chartID].Series.Add(mStrStroke[xItem] + "vs" + mStrStroke[yItem]);
                    mychart[chartID].Series[0].ChartType = SeriesChartType.FastLine;
                    mychart[chartID].Series[0].MarkerSize = 0;
                    mychart[chartID].Series[0].Color = mLineColor[3];

                    double xscale = umscale;
                    double yscale = umscale;

                    if (xItem > 2)
                        xscale = minscale;
                    if (yItem > 2)
                        yscale = minscale;

                    debugIndex = 11;
                    for (int i = 0; i < count; i++)
                    {
                        if (mStroke[xItem][i] == double.NaN || mStroke[yItem][i] == double.NaN)
                            continue;

                        mychart[chartID].Series[0].Points.AddXY(mStroke[xItem][i] * xscale, mStroke[yItem][i] * yscale); //  FWD
                    }
                    debugIndex = 12;

                    gMin = 0;
                    gMax = 0;
                    gInterval = 0;
                    string strMsg = chartID.ToString() + "]\t X : ";

                    AutoChartGrid(xMin * xscale, xMax * xscale, ref gMin, ref gMax, ref gInterval);
                    strMsg += xMin.ToString("F3") + " - " + xMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2");
                    debugIndex = 13;

                    if (gMin >= gMax)
                    {
                        gMax = gMin + 100;
                        gInterval = 10;
                    }

                    mychart[chartID].ChartAreas[0].AxisX.Minimum = gMin;
                    mychart[chartID].ChartAreas[0].AxisX.Maximum = gMax;
                    mychart[chartID].ChartAreas[0].AxisX.Interval = gInterval;

                    AutoChartGrid(yMin * yscale, yMax * yscale, ref gMin, ref gMax, ref gInterval);
                    debugIndex = 14;
                    if (gMin >= gMax)
                    {
                        gMax = gMin + 100;
                        gInterval = 10;
                    }
                    mychart[chartID].ChartAreas[0].AxisY.Minimum = gMin;
                    mychart[chartID].ChartAreas[0].AxisY.Maximum = gMax;
                    mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;
                    y1Inverval = gInterval;
                    strMsg += "\t\t Y : " + yMin.ToString("F3") + " - " + yMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2") + "\r\n";

                    mychart[chartID].Titles.Clear();
                    mychart[chartID].Titles.Add(mStrStroke[xItem] + " vs " + mStrStroke[yItem]);
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString() + "\r\n debugIndex = " + debugIndex.ToString());
                }

                //if (InvokeRequired)
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        mViewLog1.Text += strMsg;
                //    });
                //}
                //else
                //    mViewLog1.Text += strMsg;
            }
            mychart[chartID].Legends[0].Position.X = 75;
            mychart[chartID].Legends[0].Position.Y = 0;
            mychart[chartID].Legends[0].Position.Height = 10;
            mychart[chartID].Legends[0].Position.Width = 18;

            mychart[chartID].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisX.Interval;
            mychart[chartID].ChartAreas[0].AxisX.MinorGrid.Enabled = false;

            mychart[chartID].ChartAreas[0].AxisY.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisY.Interval/5;
            mychart[chartID].ChartAreas[0].AxisY2.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisY2.Interval/5;

            if (chartID<2)
                mychart[chartID].ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            else
                mychart[chartID].ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            //mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
            mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            mychart[chartID].ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            mychart[chartID].ChartAreas[0].AxisY2.MinorGrid.Enabled = false;
            //mychart[chartID].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            //mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
            if (y1Inverval >= 0.2 && y1Inverval < 1)
                mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0}";
            else if (y1Inverval < 0.2)
                mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
            else if (y1Inverval >= 1)
                mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0}";

            if (y2Inverval >= 0.2 && y2Inverval<1)
                mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0.0}";
            else if (y2Inverval < 0.2 )
                mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0.00}";
            else if (y2Inverval >=1)
                mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0}";

            m_FlagChart[chartID] = false;
        }
        public void _topPlotChart(int ch, int[] yItems, int[] y2Items, int count)
        {
            if (yItems == null || y2Items == null)
                return;

            int chartID = ch; // mychart[0,1,4,5]
            while (m_FlagChart[chartID])
                Thread.Sleep(10);

            m_FlagChart[chartID] = true;

            mychart[chartID].Series.Clear();

            if (count == 0)
            {
                mychart[chartID].Series.Add("Default");
                mychart[chartID].Series[0].Points.AddXY(1, 0);
                m_FlagChart[chartID] = false;
                return;
            }
            //  mTime = new double[10000];
            //  mXstroke = new double[10000];     //  Code Output Time
            //  mYstroke = new double[10000];     //  Code Output Time
            //  mZstroke = new double[10000];     //  Code Output Time
            //  mTXstroke = new double[10000];     //  Code Output Time
            //  mTYstroke = new double[10000];     //  Code Output Time
            //  mTZstroke = new double[10000];     //  Code Output Time

            //  mtN1 = new Point[10000];     //  1 -> 0.1um
            //  mtN2 = new Point[10000];     //  1 -> 0.1um
            //  mtW1 = new Point[10000];     //  1 -> 0.1um
            //  mtW2 = new Point[10000];     //  1 -> 0.1um
            //  mtS1 = new Point[10000];     //  1 -> 0.1um
            //  mtS2 = new Point[10000];     //  1 -> 0.1um
            //  mtE1 = new Point[10000];     //  1 -> 0.1um
            //  mtE2 = new Point[10000];     //  1 -> 0.1um

            //  msN1 = new Point[10000];     //  1 -> 0.1um
            //  msN2 = new Point[10000];     //  1 -> 0.1um
            //  msS1 = new Point[10000];     //  1 -> 0.1um
            //  msS2 = new Point[10000];     //  1 -> 0.1um

            //double scaleTop = 1;
            //double scaleSide = 1;
            //double rotTop = 0;
            //double rotSide = 0;

            //m__G.oCam[0].GetScaleNOpticalR(ref scaleTop, ref scaleSide, ref rotTop, ref rotSide);

            double umscale = 5.5 / Global.LensMag;// * ((scaleTop + scaleSide) / 2);       //  Pixel to um
            double minscale = 180 / Math.PI * 60;                           //  rad to min

            double y1Inverval = 1;
            double y2Inverval = 1;


            //  Time To Stroke
            double[] lArray = new double[count];

            if (yItems.Length >= 0)
                Array.Copy(mStroke[yItems[0]], lArray, count);
            Array.Sort(lArray);
            double yMin = lArray[0];
            double yMax = lArray[count - 1];

            if (y2Items.Length >= 0)
            {
                Array.Copy(mStroke[y2Items[0]], lArray, count);
                Array.Sort(lArray);
            }
            double y2Min = lArray[0];
            double y2Max = lArray[count - 1];

            double curScale = umscale;
            double curScale2 = umscale;

            int seriesNum = 0;
            int yItems_iltem = 0;
            for ( int iItem = 0; iItem < yItems.Length; iItem++ )
            {
                yItems_iltem = yItems[iItem];
                if (mychart[chartID].Series.FindByName(mStrStroke[yItems_iltem]) != null) 
                    continue;
                mychart[chartID].Series.Add(mStrStroke[yItems_iltem]);
                mychart[chartID].Series[seriesNum].ChartType = SeriesChartType.FastLine;
                mychart[chartID].Series[seriesNum].MarkerSize = 0;
                mychart[chartID].Series[seriesNum].Color = mLineColor[seriesNum];

                if (yItems_iltem > 2)
                    curScale = minscale;

                if(mRealTimeUnit==0)
                {
                    for (int i = 0; i < count; i++)
                        mychart[chartID].Series[seriesNum].Points.AddXY(i / 1000.0, mStroke[yItems_iltem][i] * curScale); //  FWD
                }else
                {
                    for (int i = 0; i < count; i++)
                        mychart[chartID].Series[seriesNum].Points.AddXY(i* mRealTimeUnit, mStroke[yItems_iltem][i] * curScale); //  FWD
                }

                if (yItems.Length >= 0)
                    Array.Copy(mStroke[yItems_iltem], lArray, count);
                Array.Sort(lArray);
                if (yMin > lArray[0])
                    yMin = lArray[0];

                if (yMax < lArray[count - 1])
                    yMax = lArray[count - 1];

                seriesNum++;
            }
            int yI2tems_iltem = 0;

            for (int iItem = 0; iItem < y2Items.Length; iItem++)
            {
                yI2tems_iltem = y2Items[iItem];
                if (mychart[chartID].Series.FindByName(mStrStroke[yI2tems_iltem]) != null)
                    continue;

                if (yI2tems_iltem >= 0)
                {
                    if (yI2tems_iltem > 2)
                        curScale2 = minscale;

                    mychart[chartID].Series.Add(mStrStroke[yI2tems_iltem]);
                    mychart[chartID].Series[seriesNum].ChartType = SeriesChartType.FastLine;
                    mychart[chartID].Series[seriesNum].MarkerSize = 0;
                    mychart[chartID].Series[seriesNum].Color = mLineColor[iItem+6];
                    if ( mRealTimeUnit == 0)
                    {
                        for (int i = 0; i < count; i++)
                            mychart[chartID].Series[seriesNum].Points.AddXY(i / 1000.0, mStroke[yI2tems_iltem][i] * curScale2); //  FWD
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                            mychart[chartID].Series[seriesNum].Points.AddXY( i * mRealTimeUnit, mStroke[yI2tems_iltem][i] * curScale2); //  FWD
                    }

                    mychart[chartID].Series[seriesNum].YAxisType = AxisType.Secondary;

                    seriesNum++;

                    if (yItems.Length >= 0)
                        Array.Copy(mStroke[yI2tems_iltem], lArray, count);
                    Array.Sort(lArray);
                    if (y2Min > lArray[0])
                        y2Min = lArray[0];

                    if (y2Max < lArray[count - 1])
                        y2Max = lArray[count - 1];
                }
            }

            double xInterval = 0.1;
            if (count < 200)
                xInterval = 0.01;
            else if (count < 500)
                xInterval = 0.02;
            else if (count < 2000)
                xInterval = 0.1;
            else if (count < 5000)
                xInterval = 0.2;
            else if (count <= 10000)
                xInterval = 0.5;
            else
                xInterval = 1.0;

            mychart[chartID].ChartAreas[0].AxisX.Minimum = 0;
            if (mRealTimeUnit==0)
            {
                mychart[chartID].ChartAreas[0].AxisX.Maximum = (count / 1000.0);
                mychart[chartID].ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            }
            else
            {
                mychart[chartID].ChartAreas[0].AxisX.Maximum = (mRealTimeUnit * 1000.0);
                xInterval = 1;
                mychart[chartID].ChartAreas[0].AxisX.MinorGrid.Enabled = true;
                mychart[chartID].ChartAreas[0].AxisX.MinorGrid.Interval = 0.2;
            }

            mychart[chartID].ChartAreas[0].AxisX.Interval = xInterval;

            double gMin = 0;
            double gMax = 0;
            double gInterval = 0;
            string strMsg = chartID.ToString() + "]\t Y1 : ";

            //strMsg += yMin.ToString("F3") + " - " + yMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2");
            if (!cbRealTimeFixedYScale.Checked)
            {
                AutoChartGrid(yMin * curScale, yMax * curScale, ref gMin, ref gMax, ref gInterval);
                mychart[chartID].ChartAreas[0].AxisY.Minimum = gMin;
                mychart[chartID].ChartAreas[0].AxisY.Maximum = gMax;
                mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;
                y1Inverval = gInterval;
            }

            mychart[chartID].Titles.Clear();
            if (y2Items[0] >= 0)
            {
                if (!cbRealTimeFixedYScale.Checked)
                {
                    AutoChartGrid(y2Min * curScale2, y2Max * curScale2, ref gMin, ref gMax, ref gInterval);
                    //strMsg += "\t\t Y2 : " + y2Min.ToString("F3") + " - " + y2Max.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2") + "\r\n";
                    mychart[chartID].ChartAreas[0].AxisY2.Minimum = gMin;
                    mychart[chartID].ChartAreas[0].AxisY2.Maximum = gMax;
                    mychart[chartID].ChartAreas[0].AxisY2.Interval = gInterval;
                    y2Inverval = gInterval;
                }
                mychart[chartID].Titles.Add("Time vs " + mStrStroke[yItems[0]] + " / " + mStrStroke[y2Items[0]]);
            }
            else
                mychart[chartID].Titles.Add("Time vs " + mStrStroke[yItems[0]]);

            //mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;

            //if (InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate
            //    {
            //        mViewLog1.Text += strMsg;
            //    });
            //}
            //else
            //    mViewLog1.Text += strMsg;
            mychart[chartID].Legends[0].Position.X = 75;
            mychart[chartID].Legends[0].Position.Y = 0;
            mychart[chartID].Legends[0].Position.Height = 10;
            mychart[chartID].Legends[0].Position.Width = 18;

            mychart[chartID].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisX.Interval;

            mychart[chartID].ChartAreas[0].AxisY.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisY.Interval/5;
            mychart[chartID].ChartAreas[0].AxisY2.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisY2.Interval/5;

            if (chartID < 2)
                mychart[chartID].ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            else
                mychart[chartID].ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            //mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
            mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            mychart[chartID].ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            mychart[chartID].ChartAreas[0].AxisY2.MinorGrid.Enabled = false;
            //mychart[chartID].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            //mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
            if (!cbRealTimeFixedYScale.Checked)
            {
                if (y1Inverval >= 0.2 && y1Inverval < 1)
                    mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.0}";
                else if (y1Inverval < 0.2)
                    mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
                else if (y1Inverval >= 1)
                    mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0}";

                if (y2Inverval >= 0.2 && y2Inverval < 1)
                    mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0.0}";
                else if (y2Inverval < 0.2)
                    mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0.00}";
                else if (y2Inverval >= 1)
                    mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Format = "{0:0}";
            }


            m_FlagChart[chartID] = false;
        }
        public Color[] mLineColor = new Color[12] { Color.DodgerBlue, Color.Aqua, Color.SkyBlue, Color.DeepSkyBlue, Color.CornflowerBlue, Color.CadetBlue, Color.OrangeRed, Color.HotPink, Color.IndianRed, Color.PaleVioletRed, Color.SaddleBrown, Color.SandyBrown };
        public void _locusPlotChart(int ch, int[] items, int count)
        {
            int chartID = ch; // mychart[0,1,4,5]
            while (m_FlagChart[chartID])
                Thread.Sleep(10);

            m_FlagChart[chartID] = true;

            mychart[chartID].Titles.Clear();
            mychart[chartID].Titles.Add("Mark Locus");
            mychart[chartID].Series.Clear(); ;

            if (count == 0 || items == null)
            {
                mychart[chartID].Series.Add("Default");
                mychart[chartID].Series[0].Points.AddXY(1, 0);
                m_FlagChart[chartID] = false;
                return;
            }

            //  mTime = new double[10000];
            //  mXstroke = new double[10000];     //  Code Output Time
            //  mYstroke = new double[10000];     //  Code Output Time
            //  mZstroke = new double[10000];     //  Code Output Time
            //  mTXstroke = new double[10000];     //  Code Output Time
            //  mTYstroke = new double[10000];     //  Code Output Time
            //  mTZstroke = new double[10000];     //  Code Output Time

            //  mtN1 = new Point[10000];     //  1 -> 0.1um
            //  mtN2 = new Point[10000];     //  1 -> 0.1um
            //  mtW1 = new Point[10000];     //  1 -> 0.1um
            //  mtW2 = new Point[10000];     //  1 -> 0.1um
            //  mtS1 = new Point[10000];     //  1 -> 0.1um
            //  mtS2 = new Point[10000];     //  1 -> 0.1um
            //  mtE1 = new Point[10000];     //  1 -> 0.1um
            //  mtE2 = new Point[10000];     //  1 -> 0.1um

            //  msN1 = new Point[10000];     //  1 -> 0.1um
            //  msN2 = new Point[10000];     //  1 -> 0.1um
            //  msS1 = new Point[10000];     //  1 -> 0.1um
            //  msS2 = new Point[10000];     //  1 -> 0.1um

            double yMin = 99999999;
            double yMax = -99999999;
            double xMin = 99999999;
            double xMax = -99999999;



            for ( int iItem = 0; iItem< items.Length; iItem ++)
            {
                int items_iltem = items[iItem];

                mychart[chartID].Series.Add(mStrAzimuth[items_iltem]);
                mychart[chartID].Series[iItem].Name = mStrAzimuth[items_iltem];
                mychart[chartID].Series[iItem].ChartType = SeriesChartType.FastPoint;
                mychart[chartID].Series[iItem].MarkerSize = 2;
                mychart[chartID].Series[iItem].BorderWidth = 2;
                mychart[chartID].Series[iItem].Color = mLineColor[iItem];

                if (items_iltem < 0) continue;
                for (int i = 0; i < count; i++)
                {
                    if (xMin > mPosAzimuth[items_iltem][i].X)
                        xMin = mPosAzimuth[items_iltem][i].X;
                    if (xMax < mPosAzimuth[items_iltem][i].X)
                        xMax = mPosAzimuth[items_iltem][i].X;
                    if (yMin > mPosAzimuth[items_iltem][i].Y)
                        yMin = mPosAzimuth[items_iltem][i].Y;
                    if (yMax < mPosAzimuth[items_iltem][i].Y)
                        yMax = mPosAzimuth[items_iltem][i].Y;
                    if (mPosAzimuth[items_iltem][i].X == 0 && mPosAzimuth[items_iltem][i].Y == 0)
                        continue;
                    if (mPosAzimuth[items_iltem][i].X == double.NaN || mPosAzimuth[items_iltem][i].Y == double.NaN)
                        continue;
                    mychart[chartID].Series[iItem].Points.AddXY(mPosAzimuth[items_iltem][i].X, mPosAzimuth[items_iltem][i].Y); //  convert nm to um
                }
            }
            mychart[chartID].Legends[0].Position.X = 75;
            mychart[chartID].Legends[0].Position.Y = 0;
            mychart[chartID].Legends[0].Position.Height = 6 + (items.Length-1) * 4;
            mychart[chartID].Legends[0].Position.Width = 18;
            double gMin = 0;
            double gMax = 0;
            double gInterval = 0;
            string strMsg = chartID.ToString() +"]\t X : ";
            double xInvertval = 0;
            double yInvertval = 0;
            if (!cbRealTimeFixedYScale.Checked)
            {
                AutoChartGrid(xMin, xMax, ref gMin, ref gMax, ref gInterval);
                if ( gMin >= gMax)
                {
                    gMax = gMin + 100;
                    gInterval = 10;
                }
                mychart[chartID].ChartAreas[0].AxisX.Minimum = gMin;
                mychart[chartID].ChartAreas[0].AxisX.Maximum = gMax;
                mychart[chartID].ChartAreas[0].AxisX.Interval = gInterval;
                xInvertval = gInterval;
                //strMsg += xMin.ToString("F3") + " - " + xMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2");
                AutoChartGrid(yMin, yMax, ref gMin, ref gMax, ref gInterval);
                if (gMin >= gMax)
                {
                    gMax = gMin + 100;
                    gInterval = 10;
                }
                mychart[chartID].ChartAreas[0].AxisY.Minimum = gMin;
                mychart[chartID].ChartAreas[0].AxisY.Maximum = gMax;
                mychart[chartID].ChartAreas[0].AxisY.Interval = gInterval;
                yInvertval = gInterval;
            }

            //strMsg += "\t\t Y : " + yMin.ToString("F3") + " - " + yMax.ToString("F3") + " >> " + gMin.ToString("F2") + " - " + gMax.ToString("F2") + "\r\n";


            mychart[chartID].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisX.Interval;
            mychart[chartID].ChartAreas[0].AxisX.MinorGrid.Enabled = false;

            mychart[chartID].ChartAreas[0].AxisY.MajorGrid.Interval = mychart[chartID].ChartAreas[0].AxisY.Interval;
            mychart[chartID].ChartAreas[0].AxisY.MinorGrid.Enabled = false;

            mychart[chartID].ChartAreas[0].AxisY2.Enabled = AxisEnabled.False;
            //mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
            //mychart[chartID].ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            //mychart[chartID].ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            //mychart[chartID].ChartAreas[0].AxisY2.MinorGrid.Enabled = false;

            if (!cbRealTimeFixedYScale.Checked)
            {
                if (xInvertval < 1)
                    mychart[chartID].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
                else
                    mychart[chartID].ChartAreas[0].AxisX.LabelStyle.Format = "{0:0}";

                if (yInvertval < 1)
                    mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
                else
                    mychart[chartID].ChartAreas[0].AxisY.LabelStyle.Format = "{0:0}";
            }

            //if (InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate
            //    {
            //        mViewLog1.Text += strMsg;
            //    });
            //}else
            //    mViewLog1.Text += strMsg;

            m_FlagChart[chartID] = false;
        }
        public void AutoChartGrid(double xMin, double xMax, ref double gMin, ref double gMax, ref double gInterval)
        {
            double xspan = xMax - xMin;
            if ( xspan == 0)
            {
                gMin = 0;
                gMax = 10;
                gInterval = 1;
                return;
            }
            double xA = Math.Log10(xspan);
            if (xA < 0)
                xA = (int)xA - 1;
            else
                xA = (int)xA;


            double xB = Math.Log10(xspan) * 10;
            if (xB > 0)
                xB = (int)xB + 1;
            else
                xB = (int)xB;

            double xC = Math.Pow(10, xB / 10);
            double xZ = Math.Pow(10, xA);
            double xD = xC / xZ;
            //double xE = (int)xD + 1;
            double xE = (int)xD + 1.5;
            double xF = xE * xZ;
            double xG = (2*xMin / xZ);
            if (xG > 0)
                xG = (int)xG;
            else
                xG = (int)xG - 1;
            
            xG = xG / 2;

            gMin = xG * xZ;
            gMax = xG * xZ + xF;
            gInterval = xZ / 2;

            if (gInterval < 0.02)
                gInterval = 0.02;

            if (gInterval<=0.5)
            {
                gMin = gMin - 2*gInterval;
                gMax = gMax + 2*gInterval;
            }
        }

        private void _arrayMouseEnter(int ch)
        {
            if (mychart[ch].Focused) mychart[ch].Parent.Focus();
            for (int i = 0; i < 8; i++)
            {
                if (i == ch)
                    m_bChartWheel[i] = true;
                else
                    m_bChartWheel[i] = false;
            }
        }
        private void chart1_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(0);
        }
        private void chart2_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(1);
        }
        private void chart3_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(2);
        }
        private void chart4_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(3);
        }
        private void chart5_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(4);
        }
        private void chart6_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(5);
        }
        private void chart7_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(6);
        }
        private void chart8_MouseEnter(object sender, EventArgs e)
        {
            _arrayMouseEnter(7);
        }

        private void chart1_MouseLeave(object sender, EventArgs e)
        {
            if (!chart1.Focused) chart1.Focus();
            m_bChartWheel[0] = false;
        }
        private void chart2_MouseLeave(object sender, EventArgs e)
        {
            if (!chart2.Focused) chart2.Focus();
            m_bChartWheel[1] = false;
        }
        private void chart3_MouseLeave(object sender, EventArgs e)
        {
            if (!chart3.Focused) chart3.Focus();
            m_bChartWheel[2] = false;
        }
        private void chart4_MouseLeave(object sender, EventArgs e)
        {
            if (!chart4.Focused) chart4.Focus();
            m_bChartWheel[3] = false;
        }


        private void chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int ch = 0;
            _topMouseDoubleClick(ch, e);
        }
        private void chart2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int ch = 1;
            _topMouseDoubleClick(ch, e);
        }
        private void chart3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int ch = 2;
            _topMouseDoubleClick(ch, e);
        }
        private void chart4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int ch = 3;
            _topMouseDoubleClick(ch, e);
        }

        private void ChartlayoutRealtimeMeasure(bool IsRealtime)
        {
            int ch = 0;

            if (IsRealtime)
            {
                m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR , m__G.sRecipe.iLEDcurrentLL );

                m_bChartFront[ch] = true;
                mChartArea[0].Width = mychart[0].Width;
                mChartArea[0].Height = mychart[0].Height;
                mChartArea[0].X = mychart[0].Left;
                mChartArea[0].Y = mychart[0].Top;

                mychart[0].Width = 1916 - 480;

                mChartTitle[0] = mychart[ch].Titles[0].Text;
                mychart[0].Titles[0].Text = "Real Time";
                mychart[0].Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                mychart[0].ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                mychart[0].ChartAreas[0].AxisY.MinorGrid.Interval = mychart[0].ChartAreas[0].AxisY.MajorGrid.Interval / 5;
                mychart[0].BringToFront();

                mychart[0].ChartAreas[0].AxisX.Interval = mychart[0].ChartAreas[0].AxisX.Interval / 2;
                mychart[0].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[0].ChartAreas[0].AxisX.Interval;

                mChartArea[2].X = mychart[2].Left;
                mychart[2].Left = mychart[3].Left;
                mychart[2].BringToFront();
            }
            else
            {
                mychart[0].Width  = 475;

                mychart[0].SendToBack();
                mychart[0].Titles[0].Text = mChartTitle[ch];
                mychart[0].Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                mychart[0].ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                        
                mychart[0].ChartAreas[0].AxisX.Interval = mychart[0].ChartAreas[0].AxisX.Interval * 2;
                mychart[0].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[0].ChartAreas[0].AxisX.Interval;

                mychart[2].Left = mChartArea[2].X;
                mychart[2].SendToBack();
            }


        }
        private void _topMouseDoubleClick(int ch, MouseEventArgs e)
        {
            if (cbRealTimeMeasure.Checked)
                return;

            if (!m_bChartFront[ch])
            {
                m_bChartFront[ch] = true;
                mChartArea[ch].Width = mychart[ch].Width;
                mChartArea[ch].Height = mychart[ch].Height;
                mChartArea[ch].X = mychart[ch].Left;
                mChartArea[ch].Y = mychart[ch].Top;

                mychart[ch].Width = 1916;
                mychart[ch].Height = 619;
                mychart[ch].Left = mychart[0].Left;

                mChartTitle[ch] = mychart[ch].Titles[0].Text;
                mychart[ch].Titles[0].Text = mChartTitle[ch];
                mychart[ch].Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                mychart[ch].ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                mychart[ch].ChartAreas[0].AxisY.MinorGrid.Interval = mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval / 5;
                //mychart[ch].ChartAreas[0].AxisY.LabelStyle.Format = "0";
                //mychart[ch].Legends[0].Position = new ElementPosition(80, 0, 20, 12);
                mychart[ch].BringToFront();

                mychart[ch].ChartAreas[0].AxisX.Interval = mychart[ch].ChartAreas[0].AxisX.Interval/2;
                mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisX.Interval;
                //mychart[ch].ChartAreas[0].AxisY.Interval = mychart[ch].ChartAreas[0].AxisY.Interval/2;
                //mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisY.Interval;
                //mychart[ch].ChartAreas[0].AxisY2.Interval = mychart[ch].ChartAreas[0].AxisY2.Interval / 2;
                //mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisY2.Interval;
                //mychart[ch].ChartAreas[0].AxisX.LabelStyle.Format = "0";
            }
            else
            {
                m_bChartFront[ch] = false;

                mychart[ch].Width = mChartArea[ch].Width;
                mychart[ch].Height = mChartArea[ch].Height;
                mychart[ch].Left = mChartArea[ch].X;
                mychart[ch].Top = mChartArea[ch].Y;
                //mychart[ch].Legends[0].Position = new ElementPosition(5, 0, 40, 24);
                mychart[ch].SendToBack();
                mychart[ch].Titles[0].Text = mChartTitle[ch];
                mychart[ch].Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                mychart[ch].ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                //mychart[ch].ChartAreas[0].AxisY.LabelStyle.Format = "0";

                mychart[ch].ChartAreas[0].AxisX.Interval = mychart[ch].ChartAreas[0].AxisX.Interval * 2;
                mychart[ch].ChartAreas[0].AxisX.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisX.Interval;
                //mychart[ch].ChartAreas[0].AxisY.Interval = mychart[ch].ChartAreas[0].AxisY.Interval * 2;
                //mychart[ch].ChartAreas[0].AxisY.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisY.Interval;
                //mychart[ch].ChartAreas[0].AxisY2.Interval = mychart[ch].ChartAreas[0].AxisY2.Interval * 2;
                //mychart[ch].ChartAreas[0].AxisY2.MajorGrid.Interval = mychart[ch].ChartAreas[0].AxisY2.Interval;
            }
            //  Plot
        }
        public bool IsTitleDblClicked(int ch, MouseEventArgs e)
        {
            if (ch == 0) return false;
            if (!m_bChartFront[ch])
            {
                //  When Small Window
                if (e.X > 200 && e.X < 270 && e.Y < 30)
                    return true;
            }
            else
            {
                //  When Large Window
                if (e.X > 400 && e.X < 540 && e.Y < 60)
                    return true;
            }
            return false;
        }

        public void SaveOperatorLog(int ch)
        {
            DateTime lnow = DateTime.Now;
            string sDir = m__G.m_RootDirectory + "\\Log";
            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);
            string filename = m__G.m_RootDirectory + "\\Log" + ch.ToString() + "_" + lnow.ToString("yyMMddHHmmss") + ".txt";
            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine("#" + ch.ToString());
            wr.Write(mViewLog[ch].Text);
            wr.Close();
            mViewLog[ch].Text = "";
        }
        public void ClearAllLog()
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    mViewLog[0].Text = "";
                });
            }
            else
            {
                mViewLog[0].Text = "";
            }
        }
        public void AddOperatorLog(int ch, string str, bool dataSendFlag = false)
        {
            #region 2PC Log Send/Receive
            if (F_Main.MachineType == (int)MachineType.Slave)
            {
                if (ch == 0 || ch == 2)
                {
                    if (str != "")
                    {
                        if (F_Main.SendLogDataCh3 == string.Empty)
                            F_Main.SendLogDataCh3 = str;
                        else
                            F_Main.SendLogDataCh3 = F_Main.SendLogDataCh3 + "\r\n" + str;
                    }

                    if (dataSendFlag && F_Main.SendLogDataCh3 != "")
                    {
                        PC2SendData("LOG", F_Main.SendLogDataCh3, F_Main.SendLogDataCh3.Length, 2);
                        F_Main.SendLogDataCh3 = string.Empty;
                        Thread.Sleep(50);
                    }
                }
                else if (ch == 1 || ch == 3)
                {
                    if (str != "")
                    {
                        if (F_Main.SendLogDataCh4 == string.Empty)
                            F_Main.SendLogDataCh4 = str;
                        else
                            F_Main.SendLogDataCh4 = F_Main.SendLogDataCh4 + "\r\n" + str;
                    }
                    if (dataSendFlag && F_Main.SendLogDataCh4 != "")
                    {
                        PC2SendData("LOG", F_Main.SendLogDataCh4, F_Main.SendLogDataCh4.Length, 3);
                        F_Main.SendLogDataCh4 = string.Empty;
                        Thread.Sleep(50);
                    }
                }
            }

            #endregion
            if (ch == 1)
                return;


            try
            {
                this.Invoke(new MethodInvoker(delegate ()
                {

                    if (mViewLog[ch].Text.Length > 50000)
                    {
                        if (F_Main.MachineType == (int)MachineType.Normal || F_Main.MachineType == (int)MachineType.Handler)
                        {
                            string strPath = m__G.m_RootDirectory + "\\Result\\";
                            string fn = strPath + "Log_" + ch.ToString() + ".txt";
                            StreamWriter wr = new StreamWriter(fn);
                            wr.Write(mViewLog[ch].Text);
                            wr.Close(); //190709
                        }

                        mViewLog[ch].Text = mViewLog[ch].Text.Substring(19500);
                        int sIndex = mViewLog[ch].Text.IndexOf('\n');
                        mViewLog[ch].Text = mViewLog[ch].Text.Substring(sIndex);
                    }

                    mViewLog[ch].AppendText(str + "\r\n");
                }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        bool mbFManageLoaded = false;
        private void FManage_Load(object sender, EventArgs e)
        {
            try
            {
                InitializeChart();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error in InitializeChart() " + e.ToString());
            }
            //m__G.wrSytemLog("FManage_Load : opInitDataGridView");
            if (mInfoBtn == null)
            {
                mInfoBtn = new Button[2];
                mInfoBtn[0] = btnInfo1;
                //mInfoBtn[2] = btnInfo3;
                //mInfoBtn[3] = btnInfo4;
            }
            if (mViewLog == null)
            {
                mViewLog = new TextBox[2];
                mViewLog[0] = mViewLog1;
                //mViewLog[2] = mViewLog3;
                //mViewLog[3] = mViewLog4;
            }

            mInfoBtn[0].Hide();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            UpdateCheckStates();
            InitListBoxes();
            CalcShowYield();

            LoadChartListBoxChange();
            mbFManageLoaded = true;

            mViewLog1.Text = MyOwner.mInitialMsg;
            MoveToVision();
            Thread.Sleep(5500);
            MyOwner.ShowOperatorMode();
            //m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR - 0.01, m__G.sRecipe.iLEDcurrentLL - 0.01);
            m__G.oCam[0].RegisterDelegatesStandbyTrigger(SendAStoHost);
            m__G.oCam[0].RegisterDelegatesTriggerLoss(TriggerLossDetected);
        }

        public void TriggerLossDetected()
        {
            m__G.fGraph.mDriverIC.TrigggerFaultDetected();
        }
        public void TriggerLossClear()
        {
            m__G.fGraph.mDriverIC.TriggerFaultClear();
        }
        public void InitListBoxes()
        {
            ListBox[] tListBox = new ListBox[7] { listBox1, listBox2, listBox3, listBox4, listBox5, listBox6, listBox7 };
            string[] axisItems = new string[6] { "X", "Y", "Z", "TX", "TY", "TZ" };
            string[] axisMarkItems = new string[12] { "N1", "N2", "W1", "W2", "S1", "S2", "E1", "E2", "topN1", "topN2", "topS1", "topS2" };

            for ( int i=0; i< 6; i++)
                for ( int j=0; j< 6; j++)
                {
                    tListBox[i].Items.Add(axisItems[j]);
                }

            for (int j = 0; j < 12; j++)
                tListBox[6].Items.Add(axisMarkItems[j]);
        }

        public void UpdateCheckStates()
        {
            if (!m__G.m_bSaveRawData) lblSaveRawData.ForeColor = Color.LightGray; else lblSaveRawData.ForeColor = Color.Aqua;
            if (!m__G.m_bHideAllGraph) lblHideAllGraph.ForeColor = Color.LightGray; else lblHideAllGraph.ForeColor = Color.Aqua;
            if (!m__G.m_bSaveNgImage) lblSaveNG.ForeColor = Color.LightGray; else lblSaveNG.ForeColor = Color.Aqua;
            if (!m__G.m_bOISOption) lblOISOption.ForeColor = Color.LightGray; else lblOISOption.ForeColor = Color.Aqua;
            if (!m__G.m_bSaveFImage) lblSaveUserImage.ForeColor = Color.LightGray; else lblSaveUserImage.ForeColor = Color.Aqua;
            if (!m__G.m_bPseudoOMM) lblPSeudoOmm.ForeColor = Color.LightGray; else lblPSeudoOmm.ForeColor = Color.Aqua;
        }

        private void btnToAdmin_Click(object sender, EventArgs e)
        {
            this.Hide();
            if (!m__G.m_bPasswordOn)
                MyOwner.ShowAdminMode();
            else
            {
                //  다음은 Password 적용 시 Open 할 것.
                mPWdialog.MyOwner = MyOwner;
                mPWdialog.m__G = m__G;
                mPWdialog.SetDesktopLocation(100, 500);
                mPWdialog.ShowDialog();
            }
        }

        public void ManualToPlot()
        {
            for (int ch = 0; ch < 4; ch++)
            {
                mychart[ch * 2 + 1].Width = mChartArea[2 * ch + 1].Width;
                mychart[ch * 2 + 1].Height = mChartArea[2 * ch + 1].Height;
                mychart[ch * 2 + 1].Left = mChartArea[2 * ch + 1].X;
                mychart[ch * 2 + 1].Top = mChartArea[2 * ch + 1].Y;

                mychart[ch * 2 + 1].ChartAreas[0].AxisX.Minimum = -0.01;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.Maximum = 0.05;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.Interval = 0.005;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.MajorGrid.Interval = 0.005;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.MinorGrid.Interval = 0.001;
                mychart[ch * 2 + 1].ChartAreas[0].AxisX.MinorGrid.Enabled = true;

                mychart[ch * 2 + 1].ChartAreas[0].AxisY.Minimum = -0.2;
                mychart[ch * 2 + 1].ChartAreas[0].AxisY.Maximum = 2;
                mychart[ch * 2 + 1].ChartAreas[0].AxisY.Interval = 0.2;
                mychart[ch * 2 + 1].ChartAreas[0].AxisY.MajorGrid.Enabled = true;
                mychart[ch * 2 + 1].ChartAreas[0].AxisY.MajorGrid.Interval = 0.2;

                mychart[ch * 2 + 1].Series[0].Points.Clear();
                mychart[ch * 2 + 1].Series[1].Points.Clear();
            }
        }

        private void btnToVision_Click(object sender, EventArgs e)
        {

            MoveToVision();
        }
        private void MoveToVision()
        {
            if (MyOwner.IsTesting[0] || MyOwner.IsTesting[1])
                return;
            if (!m__G.mFAL.mFAutoLearnLoaded)
            {
                m__G.mFAL.Show();
                m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                //mFAL.Size = new Size(1920, 1045);
                m__G.mFAL.Location = new Point(0, 0);
                m__G.mFAL.Hide();
            }
            //P_Sub.Location = new Point(0, 0);
            //P_Sub.Size = new Size(1920, 1040);

            //m__G.fVision.Show();

            MyOwner.SubForm_Show(1);
            m__G.fVision.EnableBtns(true);

            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
                mInfoBtn[i].Hide();
        }
        private void btnInfo2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
                mInfoBtn[i].Hide();
        }
        private void btnInfo3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
                mInfoBtn[i].Hide();
        }

        private void btnInfo4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
                mInfoBtn[i].Hide();
        }


        public void SetHallPolarity(int x, int y)
        {
            //  각 Channel 별로 Polarity 표시해줘야 한다.
            //lbHallPolarity.Text = "Hall Polarity X : " + x + " Y : " + y;
        }
        public void ClearHallPolarity()
        {
            //  각 Channel 별로 Polarity 표시해줘야 한다.
            //lbHallPolarity.Text = "Hall Polarity X :   Y : ";
        }

        /// <summary>
        /// ////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        bool mViewLog1Maximized = false;
        private void mViewLog1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //mChangeViewLog(0);
            if (!mViewLog1Maximized)
            {
                mViewLog1.Location = new Point(965, 44);
                mViewLog1.Size = new Size(955, 965);
                mViewLog1.BringToFront();
                mViewLog1Maximized = true;
            }
            else
            {
                mViewLog1.Location = new Point(481, 745);
                mViewLog1.Size = new Size(959, 269);
                mViewLog1Maximized = false;
            }
        }
        private void mViewLog2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            mChangeViewLog(1);
        }
        private void mViewLog3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //mChangeViewLog(2);
        }
        private void mViewLog4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //mChangeViewLog(3);
        }
        private void mChangeViewLog(int ch)
        {
            if (mViewLog[ch] == null)
                return;

            this.Invoke(new MethodInvoker(
                 delegate ()
                 {
                     if (!m_bViewLogLarge[ch])
                     {
                         mViewLog[ch].Size = new Size(476, 654);
                         m_bViewLogLarge[ch] = true;
                     }
                     else
                     {
                         mViewLog[ch].Size = new Size(476, 36);
                         m_bViewLogLarge[ch] = false;
                     }
                 })
                 );
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                if (!mbMagTable)
                {
                    mbMagTable = true;
                }
                else
                {
                    mbMagTable = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime thisTime = DateTime.ParseExact("18100305", "yyMMddHH", null);
            DateTime thatTime = DateTime.ParseExact("18111511", "yyMMddHH", null);

            TimeSpan dTime = thatTime - thisTime;

            MessageBox.Show("hours = " + dTime.Hours + " \t days = " + dTime.Days);

        }

        private void FManage_Paint(object sender, PaintEventArgs e)
        {
            UpdateCheckStates();
            this.BackColor = Color.FromArgb(96, 96, 100);
        }

        private void tbRepeatLULdone_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbRepeatLULcount_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            ClearAllLog();
        }

        private void btnSetSampleNumber_Click(object sender, EventArgs e)
        {
            m__G.sCIndex[0] = 0;
            m__G.sCIndex[1] = 0;
            m__G.sCIndex[2] = 0;
            m__G.sCIndex[3] = 0;
            m__G.mForcedSampleNumber = Convert.ToInt32(tbConsecutiveTest.Text);
            HoldCurrentSampleNumber(false);
        }
        public void SetCurrentSampleNumber(int index = -3)
        {
            if (tbResidualTestNumber.InvokeRequired)
            {
                tbResidualTestNumber.Invoke(new MethodInvoker(delegate ()
                {
                    if (index > -3)

                        tbResidualTestNumber.Text = index.ToString();
                    else
                        tbResidualTestNumber.Text = m__G.sHistIndex.ToString();
                }));
            }
            else
            {
                if (index > -3)

                    tbResidualTestNumber.Text = index.ToString();
                else
                    tbResidualTestNumber.Text = m__G.sHistIndex.ToString();
            }
            if (tbConsecutiveTest.InvokeRequired)
            {
                tbConsecutiveTest.Invoke(new MethodInvoker(delegate ()
                {
                    tbConsecutiveTest.Text = (Convert.ToInt32(tbResidualTestNumber.Text) - 1).ToString();
                }));
            }
            else
            {
                tbConsecutiveTest.Text = (Convert.ToInt32(tbResidualTestNumber.Text) - 1).ToString();
            }
        }
        public void HoldCurrentSampleNumber(bool IsHold = true)
        {
            if (m__G.mForcedSampleNumber > 0)
            {
                tbResidualTestNumber.Text = m__G.mForcedSampleNumber.ToString();
                m_LastSampleNumber = m__G.mForcedSampleNumber;
            }
            else
            {
                m_LastSampleNumber = -1;
                m__G.mForcedSampleNumber = -1;
            }
        }
        public void SaveCurrentSampleNumber(int index)
        {
            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\LastDayResults.txt");
            DateTime lnow = DateTime.Now;
            //m__G.fManage.AddOperatorLog(0, ">--->" + index + " " + m_LastSampleNumber + " " + m__G.mDailyTotalTested );

            wr.WriteLine(index.ToString());
            string time = lnow.ToString("yy-MM-dd-HH-mm-ss");
            wr.WriteLine(time);
            wr.WriteLine(m__G.mDailyTotalTested.ToString());
            wr.WriteLine(m__G.mDailyTotalFail.ToString());
            for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
            {
                wr.WriteLine(m__G.iFailCountPerItemAll[i].ToString());
            }
            wr.Close();

            string filePath = m__G.m_RootDirectory + "\\DoNotTouch\\LastSampleNumber.txt";
            string alline = "";
            string[] lines;
            string lastData = "";
            string[] lastColumn;
            int lstartLine = 1;
            int llastLine = 0;

            if (File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath);
                alline = sr.ReadToEnd();
                sr.Close();
                lines = alline.Split("\r".ToCharArray());
                //m__G.fManage.AddOperatorLog(3, "lineLength = " + lines.Length.ToString());
                llastLine = lines.Length;
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    if (lines[i].Length > 6)
                    {
                        lastData = lines[i];
                        break;
                    }
                }
                lastColumn = lastData.Split("\t".ToCharArray());

                //m__G.fManage.AddOperatorLog(3, ("lastColumn[] = " + lastColumn[0]));

                //timeStr = lstr[1].Substring(0, lstr[1].Length - 1);
                DateTime lLastDay = DateTime.ParseExact(lastColumn[0].Substring(1, 6), "yyMMdd", null);
                //m__G.fManage.AddOperatorLog(3, ("lLastDay = " + lLastDay.ToString("yyMMdd")));

                if (lLastDay.DayOfYear == lnow.DayOfYear)
                    lstartLine = 2;
            }
            else
            {
                lines = new string[2];
                lines[1] = "";
                lstartLine = 0;
            }
            time = lnow.ToString("yyMMdd");
            wr = new StreamWriter(filePath);
            wr.WriteLine("Last Tested SPL No : " + index.ToString());

            //for (int i=1; i< llastLine ; i++)
            wr.WriteLine(time + "\t: " + m__G.mDailyTotalTested.ToString());
            for (int i = lstartLine; i < llastLine; i++)
            {
                if (lines[i].Length > 6)
                    wr.WriteLine(lines[i]);
            }

            wr.Close();
        }

        public void ToViet(bool IsToViet)
        {
            if (IsToViet)
            {
                btnToAdmin.Text = "người quản lý";
                btnToVision.Text = "cài đặt máy ảnh";
                btnClearLogs.Text = "loại ra Log";
                //btnSetSampleNumber.Text = "thiết lập số Sản phẩm.";
                btnStartTriggeredGrab.Text = "Bắt đầu lặp lại";
            }
            else
            {
                btnToAdmin.Text = "Admin";
                btnToVision.Text = "Vision";
                btnClearLogs.Text = "Clear Log";
                //btnSetSampleNumber.Text = "Set Sample Number";
                btnStartTriggeredGrab.Text = "Repeat Start";
            }
        }
        private void btnSuddenStop_Click(object sender, EventArgs e)
        {
            //MyOwner.Process_CalcCLXY_TMP(0, 0);
            SuddenStop();
        }
        public void SuddenStop()
        {
            m__G.mbSuddenStop[0] = true;    //  StopTest Message 받으면 즉시 중단, 또는 fManage 에서 Stop Test Button 누르면 즉시 중단.
            m__G.oCam[0].mAbort = true;
            //m__G.mbSuddenStop[1] = true;
            mRepeatLuL = false;
            int count = 0;
            while (count < 100)
            {
                if (!m__G.mbSuddenStop[0])
                    break;
                Thread.Sleep(50);
                count++;
            }
            m__G.fGraph.Drive_LEDs(0, 0);
            m__G.oCam[0].mAbort = false;

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnStartTriggeredGrab.Enabled = true;
                });
            }
            else
                btnStartTriggeredGrab.Enabled = true;

        }
        public void CalcShowYield()
        {
#if DEBUG
            return;
#endif
            double[] lyield = new double[10];
            int[] lindex = new int[200];
            int[] lcount = new int[200];
            int i = 0;
            for (i = 0; i < 200; i++)
                lindex[i] = i;
            Array.Copy(m__G.iFailCountPerItemAll, lcount, m__G.sNUM_TESTITEM);
            //m__G.sHistIndex = 100;
            //m__G.iFailCountPerItemAll[0] = 0;
            //m__G.iFailCountPerItemAll[1] = 5;
            //m__G.iFailCountPerItemAll[2] = 8;
            //m__G.iFailCountPerItemAll[3] = 3;
            //m__G.iFailCountPerItemAll[4] = 22;
            //m__G.iFailCountPerItemAll[5] = 15;
            //m__G.iFailCountPerItemAll[6] = 0;

            Array.Sort(lcount, lindex, Comparer<int>.Create((x, y) => y.CompareTo(x)));

            string lhead = "";
            for (i = 0; i < 10; i++)
            {
                if (lindex[i] > (m__G.sNUM_TESTITEM - 1))
                    break;
                for (int j = lindex[i]; j >= 0; j--)
                {
                    if (m__G.mTestItem[j, 0] != "")
                    {
                        lhead = m__G.mTestItem[j, 0];
                        break;
                    }
                }
                if (m__G.mDailyTotalTested > 0)
                {
                    mFailItem[i] = lhead + "_" + m__G.mTestItem[lindex[i], 1];
                    //mFailRatio[i] = m__G.iFailCountPerItemAll[lindex[i]] / (m__G.sHistIndex + 1.0);
                    mFailRatio[i] = m__G.iFailCountPerItemAll[lindex[i]] / (double)m__G.mDailyTotalTested;
                }
                //AddOperatorLog(3, i.ToString() + ":" + mFailItem[i] + "-" + mFailRatio[i].ToString("F2"));
            }
            if (!m__G.m_bHideAllGraph)
            {
                PlotChartYield(i);
                chart9.Show();

                chart1.Show();
                chart2.Show();
                chart3.Show();
                chart4.Show();
            }
            else
            {
                chart9.Hide();
                chart1.Hide();
                chart2.Hide();
                chart3.Hide();
                chart4.Hide();
            }

        }
        public void PlotChartYield(int count)
        {
            //MessageBox.Show("___" + m__G.m_bHideAllGraph.ToString());
            int lcount = 10;

            if (count > 10) lcount = 10;
            else
                lcount = count;

            double[] lratio = new double[count];
            string[] litem = new string[count];

            for (int i = 0; i < count; i++)
            {
                litem[i] = mFailItem[i];
                lratio[i] = mFailRatio[i];
            }
            //  ch = 0,1,2,3
            if (chart9.InvokeRequired)
            {
                chart9.Invoke(new MethodInvoker(delegate ()
                {
                    _plotChartYield(litem, lratio);
                }));
            }
            else
                _plotChartYield(litem, lratio);
        }
        private void _plotChartYield(string[] litem, double[] lratio)
        {
            double lyield = 100;
            if (m__G.mDailyTotalTested > 0)
                lyield = (1 - m__G.mDailyTotalFail / (double)m__G.mDailyTotalTested) * 100;

            int gNum = m__G.mDailyTotalTested - m__G.mDailyTotalFail;
            chart9.Titles[0].Text = "Yield " + lyield.ToString("F2") + "% \t" + gNum.ToString() + " / " + m__G.mDailyTotalTested.ToString();

            chart9.Series[0].Points.DataBindXY(litem, lratio); //  FWD
            chart9.DataManipulator.Sort(PointSortOrder.Descending, chart9.Series[0]);
            Color[] lcolor = new Color[10] { Color.DimGray, Color.DodgerBlue, Color.OrangeRed, Color.Lime, Color.BlueViolet, Color.LightPink, Color.Green, Color.Red, Color.Yellow, Color.SkyBlue };
            for (int i = 0; i < lratio.Length; i++)
            {
                chart9.Series[0].Points[i].Color = lcolor[i];
            }
        }
        private void chart9_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you wan to reset Yield Data?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                //code for Yes
                m__G.mDailyTotalTested = 0;
                m__G.mDailyTotalFail = 0;
                for (int i = 0; i < 100; i++)
                    m__G.iFailCountPerItemAll[i] = 0;
                for (int i = 0; i < 10; i++)
                {
                    mFailItem[i] = "";
                    mFailRatio[i] = 0;
                }
                CalcShowYield();
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            //SaveLogNClear();
        }
        public void SaveLogNClear(int ch = -1)
        {
            DateTime lnow = DateTime.Now;
            //string sDir = m__G.m_RootDirectory + "\\Result\\Log";
            //if (!Directory.Exists(sDir))
            //    Directory.CreateDirectory(sDir);
            string newDir = m__G.fManage.CheckResultFolder();

            newDir = newDir + "Log Data\\";
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            if (ch < 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    string filename = newDir + "\\Log" + i.ToString() + "_" + lnow.ToString("yyMMddHHmmss") + ".txt";
                    StreamWriter wr = new StreamWriter(filename);
                    string saveStr = mViewLog[i].Text.ToString() + "\r\n";
                    wr.Write(saveStr);
                    wr.Close();
                    //Thread.Sleep(1000);
                    //mViewLog[i].Text = "";
                }
            }
            else
            {
                string filename = newDir + "\\Log" + ch.ToString() + "_" + m__G.sCIndex[ch].ToString() + ".txt";
                StreamWriter wr = new StreamWriter(filename);
                string saveStr = mViewLog[ch].Text;
                wr.Write(saveStr);
                wr.Close();
                //Thread.Sleep(1000);
                //if (saveStr.Length > 1000)
                //    MessageBox.Show(saveStr.Substring(0, 1000);

                //mViewLog[ch].Text = "";
            }
        }
        public void ClearLog(int ch = -1)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (ch < 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            mViewLog[i].Text = "";
                        }
                    }
                    else
                    {
                        mViewLog[ch].Text = "";
                    }
                });
            }
        }
        private void btnSaveScreenOperator_Click(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            string pngname = "Oper" + dtNow.ToString("yyMMddhhmmss") + ".png";
            string sScreenCapturePath = m__G.m_RootDirectory + "\\User_ScreenShot\\" + pngname;
            string sDir = m__G.m_RootDirectory + "\\User_ScreenShot";
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);
            Size s = new Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);


            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
            memoryImage.Save(sScreenCapturePath);

        }
        private void btnCheckContact_Click(object sender, EventArgs e)
        {

        }

        #region 3720 2PC
        SocketInterface COMM = null;
        SocketInterface MonitoringCOMM = null;
        const string ipAddress = "192.168.0.2";
        const int port = 5000;
        const int MonitoringPort = 5002;



        //public bool PC2Connection()
        //{
        //    try
        //    {
        //        if (F_Main.MachineType == (int)MachineType.Master)
        //        {
        //            COMM = MySocketServerClass.CreateServerSocket(port, SocketInterface.TypeOfString.UTF8);
        //            MonitoringCOMM = MySocketServerClass.CreateServerSocket(MonitoringPort, SocketInterface.TypeOfString.UTF8);
        //        }

        //        else if (F_Main.MachineType == (int)MachineType.Slave)
        //            COMM = MySocketClientClass.CreateClientSocket(ipAddress, port, SocketInterface.TypeOfString.UTF8);

        //        COMM.StartSocket();

        //        COMM.ConnectedEvent += COMM_ConnectedEvent;
        //        COMM.DisconnectedEvent += COMM_DisconnectedEvent;
        //        COMM.ReceiveEvent += COMM_ReceiveEvent;

        //        if (F_Main.MachineType == (int)MachineType.Master)
        //        {
        //            MonitoringCOMM.StartSocket();
        //            MonitoringCOMM.ConnectedEvent += MonitoringCOMM_ConnectedEvent;
        //            MonitoringCOMM.DisconnectedEvent += MonitoringCOMM_DisconnectedEvent;
        //            MonitoringCOMM.ReceiveEvent += MonitoringCOMM_ReceiveEvent;
        //            //lblMachineType.BackColor = Color.Yellow;
        //            //lblSlaveMonitor.BackColor = Color.Yellow;

        //        }


        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //private void MonitoringCOMM_ReceiveEvent(string IPAddress, string sMessage)
        //{

        //}

        //private void MonitoringCOMM_DisconnectedEvent(string IPAddress)
        //{
        //    //if (lblSlaveMonitor.InvokeRequired)
        //    //{
        //    //    lblSlaveMonitor.Invoke(new MethodInvoker(delegate ()
        //    //    {
        //    //        lblSlaveMonitor.BackColor = Color.Red;


        //    //    }));
        //    //}
        //    //else
        //    //{
        //    //    lblSlaveMonitor.BackColor = Color.Red;

        //    //}

        //    if (F_Main.MachineType == (int)MachineType.Master)
        //    {
        //        MonitoringCOMM.EndSocket();
        //        MonitoringCOMM = null;
        //        MonitoringCOMM = MySocketServerClass.CreateServerSocket(MonitoringPort, SocketInterface.TypeOfString.UTF8);
        //        MonitoringCOMM.StartSocket();
        //        MonitoringCOMM.ConnectedEvent += MonitoringCOMM_ConnectedEvent;
        //        MonitoringCOMM.DisconnectedEvent += MonitoringCOMM_DisconnectedEvent;
        //        MonitoringCOMM.ReceiveEvent += MonitoringCOMM_ReceiveEvent;
        //        //if (lblSlaveMonitor.InvokeRequired)
        //        //{
        //        //    lblSlaveMonitor.Invoke(new MethodInvoker(delegate ()
        //        //    {
        //        //        lblSlaveMonitor.BackColor = Color.Yellow;


        //        //    }));
        //        //}
        //        //else
        //        //{
        //        //    lblSlaveMonitor.BackColor = Color.Yellow;

        //        //}

        //    }
        //}

        //private void MonitoringCOMM_ConnectedEvent(string IPAddress)
        //{

        //    //if (lblSlaveMonitor.InvokeRequired)
        //    //{
        //    //    lblSlaveMonitor.Invoke(new MethodInvoker(delegate ()
        //    //    {
        //    //        lblSlaveMonitor.BackColor = Color.YellowGreen;


        //    //    }));
        //    //}
        //    //else
        //    //{
        //    //    lblSlaveMonitor.BackColor = Color.YellowGreen;

        //    //}
        //}

        //public bool PC2Disconnection()
        //{
        //    try
        //    {
        //        COMM.EndSocket();
        //        COMM = null;
        //        if (F_Main.MachineType == (int)MachineType.Master)
        //        {
        //            MonitoringCOMM.EndSocket();
        //            MonitoringCOMM = null;
        //        }
        //        return true;

        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        public void PC2SendData(string pre, string Message, int datalength, int channel = 2)
        {

            string data = string.Empty;
            data = (char)0x02 + pre + datalength.ToString("D8") + channel.ToString("D1") + Message + (char)0x03;

            COMM.SendMessage(data);
        }
        private void COMM_ReceiveEvent(string IPAddress, string sMessage)
        {
            try
            {
                sMessage = sMessage.Trim('\0');
                if (F_Main.Remainder != string.Empty)
                {

                    sMessage = F_Main.Remainder + sMessage;
                    F_Main.Remainder = string.Empty;
                }

                if (sMessage.Substring(0, 1).Contains((char)0x02))
                {

                    F_Main.ReceivedCommand = sMessage.Substring(1, 3);
                    F_Main.DataLength = Convert.ToInt32(sMessage.Substring(4, 8));
                    F_Main.ReceivedChannel = Convert.ToInt32(sMessage.Substring(12, 1));
                    F_Main.ReceivedData = sMessage.Substring(13, sMessage.Length - 13);
                }
                else
                {

                    F_Main.ReceivedData = F_Main.ReceivedData + sMessage;
                }

                switch (F_Main.ReceivedCommand)
                {
                    case "LOG":  //show Log
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {

                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);


                            Thread testThr = new Thread(() => AddOperatorLog(F_Main.ReceivedChannel, F_Main.ReceivedData, false));
                            testThr.Start();

                        }
                        break;
                    case "RBT": //Show Result Btn
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            string[] tmpstr = new string[2];
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            tmpstr = F_Main.ReceivedData.Split('\t');
                            //mInfoBtn[2].Text = tmpstr[0];
                            //mInfoBtn[3].Text = tmpstr[1];

                            F_Main.ReceivedCommand = string.Empty;
                            F_Main.DataLength = 0;
                            F_Main.ReceivedChannel = 0;
                            F_Main.ReceivedData = string.Empty;

                            //for (int i = 2; i < tmpstr.Length + 2; i++)
                            //{
                            //    if (mInfoBtn[i].Text == "PASS")
                            //    {
                            //        mInfoBtn[i].Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                            //        mInfoBtn[i].ForeColor = Color.Cyan;
                            //    }
                            //    else
                            //    {
                            //        mInfoBtn[i].Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                            //        mInfoBtn[i].ForeColor = Color.OrangeRed;
                            //    }
                            //}

                            //mInfoBtn[2].Show();
                            //mInfoBtn[3].Show();
                        }
                        break;
                    case "SRD": //show Result Data
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveResultData(F_Main.ReceivedChannel, F_Main.ReceivedData));
                            testThr.Start();
                        }
                        break;
                    case "CDX": //Code Current Graph X
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeCurrent(F_Main.ReceivedChannel, F_Main.ReceivedData, 0));
                            testThr.Start();



                        }
                        break;
                    case "CDY"://Code Current Graph Y
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeCurrent(F_Main.ReceivedChannel, F_Main.ReceivedData, 1));
                            testThr.Start();

                        }
                        break;
                    case "HDX"://Code Hall Graph X
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeHall(F_Main.ReceivedChannel, F_Main.ReceivedData, 0));
                            testThr.Start();


                        }
                        break;
                    case "HDY"://Code Hall Graph Y
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeHall(F_Main.ReceivedChannel, F_Main.ReceivedData, 1));
                            testThr.Start();

                        }
                        break;
                    case "SDX"://Code Stroke Graph X
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeStroke(F_Main.ReceivedChannel, F_Main.ReceivedData, 0));
                            testThr.Start();

                        }
                        break;
                    case "SDY"://Code Stroke Graph Y
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ReceiveCodeStroke(F_Main.ReceivedChannel, F_Main.ReceivedData, 1));
                            testThr.Start();

                        }
                        break;
                    case "WRD": //write Result Data
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.Remainder = F_Main.ReceivedData.Substring(F_Main.DataLength + 1, F_Main.ReceivedData.Length - F_Main.DataLength - 1);
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_WriteResult(F_Main.ReceivedData));
                            testThr.Start();
                        }
                        break;
                    case "RSV": // recipe save
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            if (On_SaveData != null)
                                On_SaveData(0, F_Main.ReceivedData);


                        }
                        break;
                    case "RLD": // recipe load
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {

                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            if (On_LoadData != null)
                                On_LoadData(0, F_Main.ReceivedData);
                        }
                        break;
                    case "SSV": // spec save
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            if (On_SaveData != null)
                                On_SaveData(1, F_Main.ReceivedData);


                        }
                        break;
                    case "SLD": // spec load
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            if (On_LoadData != null)
                                On_LoadData(1, F_Main.ReceivedData);


                        }
                        break;
                    case "SCB": // save CBstate
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            if (On_Save_CBstate != null)
                                On_Save_CBstate(F_Main.ReceivedData);
                        }
                        break;
                    case "GDX":
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            //Thread testThr = new Thread(() => Process_ReceivedGraphData(F_Main.ReceivedChannel, F_Main.ReceivedData, 0));
                            //testThr.Start();
                        }
                        break;
                    case "GDY":
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            //Thread testThr = new Thread(() => Process_ReceivedGraphData(F_Main.ReceivedChannel, F_Main.ReceivedData, 1));
                            //testThr.Start();
                        }
                        break;

                    case "VBC": //vision button click
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            switch (F_Main.ReceivedData.Substring(0, 2))
                            {
                                case "01":
                                    if (F_Main.ReceivedData.Length > 2)
                                    {
                                        if (F_Main.ReceivedData.Substring(2, 2) == "LD")
                                        {
                                            m__G.fGraph.socket_IN(0);
                                            m__G.fVision.mSocketLoaded[0] = true;
                                        }
                                        else if (F_Main.ReceivedData.Substring(2, 2) == "UD")
                                        {
                                            m__G.fGraph.socket_OUT(0);
                                            m__G.fVision.mSocketLoaded[0] = false;
                                        }
                                    }
                                    else
                                    {
                                        if (!m__G.fVision.mSocketLoaded[0])
                                        {
                                            m__G.fGraph.socket_IN(0);
                                            m__G.fVision.mSocketLoaded[0] = true;
                                        }
                                        else
                                        {
                                            m__G.fGraph.socket_OUT(0);
                                            m__G.fVision.mSocketLoaded[0] = false;
                                        }
                                    }

                                    break;
                                case "02":
                                    if (F_Main.ReceivedData.Substring(2, 2) == "ON")
                                    {
                                        m__G.fGraph.Drive_LED(0, m__G.fVision.mLEDcurrent[0]);
                                        m__G.fGraph.Drive_LED(1, m__G.fVision.mLEDcurrent[1]);
                                        m__G.fVision.m_bAllLEDOn = true;

                                    }
                                    else if (F_Main.ReceivedData.Substring(2, 2) == "OF")
                                    {
                                        m__G.fGraph.Drive_LED(0, 0);
                                        m__G.fGraph.Drive_LED(1, 0);
                                        m__G.fVision.m_bAllLEDOn = false;
                                    }
                                    break;
                                case "03":
                                    if (F_Main.ReceivedData.Substring(2, 2) == "ON")
                                    {
                                        Thread threadLEDMark = new Thread(() => m__G.fVision.LEDMarkCheck());
                                        threadLEDMark.Start();
                                    }
                                    else if (F_Main.ReceivedData.Substring(2, 2) == "OF")
                                    {
                                        m__G.fVision.m_bLEDMarkCheck = false;
                                    }
                                    break;
                                case "04":
                                    if (F_Main.ReceivedData.Substring(2, 3) == "CH3")
                                    {
                                        m__G.fGraph.mDriverIC.SetFailLED(0, true);
                                        Thread.Sleep(1000);
                                        m__G.fGraph.mDriverIC.SetFailLED(0, false);
                                    }
                                    else if (F_Main.ReceivedData.Substring(2, 3) == "CH4")
                                    {
                                        m__G.fGraph.mDriverIC.SetFailLED(1, true);
                                        Thread.Sleep(1000);
                                        m__G.fGraph.mDriverIC.SetFailLED(1, false);
                                    }
                                    break;
                            }

                        }
                        break;
                    case "CPC": //check pin contact
                        break;
                    case "STI":  //Save Test Item 
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            string[] results = F_Main.ReceivedData.Split(',');
                            int effItemNum = 0;
                            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TestItemSetting.txt";
                            StreamWriter sw = new StreamWriter(strFileName);

                            for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
                            {
                                sw.Write(results[i].ToString() + "\r\n");
                                if (results[i] == "true" || results[i] == "True")
                                {
                                    m__G.mGridToTestItem[effItemNum] = i;
                                    m__G.mTestItemToGrid[i] = effItemNum++;
                                    m__G.mTestItem[i, 10] = "true";
                                }
                                else
                                {
                                    m__G.mTestItemToGrid[i] = -1;
                                    m__G.mTestItem[i, 10] = "false";
                                }
                            }
                            sw.Close();
                            MyOwner.btnOperation.PerformClick();
                            MyOwner.UpdateDataGridView();
                            PC2SendData("ACK", "STI", 3, 2);

                        }
                        break;
                    case "ACK":
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            F_Main.UIControltmr.Enabled = false;
                            MessageBox.Show(new Form() { TopMost = true }, "Change Complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    case "NAK":
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            F_Main.UIControltmr.Enabled = false;
                            MessageBox.Show(new Form() { TopMost = true }, "Check the Slave PC", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;

                    case "CAD": //clear all data
                        if (F_Main.ReceivedData.Contains((char)0x03))
                        {
                            F_Main.ReceivedData = F_Main.ReceivedData.Substring(0, F_Main.DataLength);
                            Thread testThr = new Thread(() => Process_ClearData());
                            testThr.Start();



                        }
                        break;

                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                if (F_Main.ReceivedCommand == "STI")
                    PC2SendData("NAK", "STI", 3, 2);
                MessageBox.Show(ex.ToString());
            }
        }
        void Process_ClearData()
        {
            ClearGraph(1);
            //mInfoBtn[2].Hide();
            //mInfoBtn[3].Hide();
            //ClearDataResults(2);
            //ClearDataResults(3);
        }
        //private void COMM_DisconnectedEvent(string IPAddress)
        //{
        //    //if (lblMachineType.InvokeRequired)
        //    //{
        //    //    lblMachineType.Invoke(new MethodInvoker(delegate ()
        //    //    {
        //    //        lblMachineType.BackColor = Color.Red;


        //    //    }));
        //    //}
        //    //else
        //    //{
        //    //    lblMachineType.BackColor = Color.Red;

        //    //}

        //    if (F_Main.MachineType == (int)MachineType.Master)
        //    {
        //        COMM.EndSocket();
        //        COMM = null;
        //        COMM = MySocketServerClass.CreateServerSocket(port, SocketInterface.TypeOfString.UTF8);
        //        COMM.StartSocket();
        //        COMM.ConnectedEvent += COMM_ConnectedEvent;
        //        COMM.DisconnectedEvent += COMM_DisconnectedEvent;
        //        COMM.ReceiveEvent += COMM_ReceiveEvent;
        //        //if (lblMachineType.InvokeRequired)
        //        //{
        //        //    lblMachineType.Invoke(new MethodInvoker(delegate ()
        //        //    {
        //        //        lblMachineType.BackColor = Color.Yellow;


        //        //    }));
        //        //}
        //        //else
        //        //{
        //        //    lblMachineType.BackColor = Color.Yellow;

        //        //}

        //    }
        //    F_Main.ConnectionStatus = false;
        //}
        //private void COMM_ConnectedEvent(string IPAddress)
        //{

        //    //if (lblMachineType.InvokeRequired)
        //    //{
        //    //    lblMachineType.Invoke(new MethodInvoker(delegate ()
        //    //    {
        //    //        lblMachineType.BackColor = Color.YellowGreen;


        //    //    }));
        //    //}
        //    //else
        //    //{
        //    //    lblMachineType.BackColor = Color.YellowGreen;

        //    //}

        //    F_Main.ConnectionStatus = true;

        //}

        void Process_WriteResult(string Data)
        {
            string[] results = Data.Split('\t');

            DateTime dtNow = DateTime.Now;
            string fileName = "res_" + dtNow.ToString("yyMMdd") + ".csv";
            string sFilePath = string.Empty;

            string sLotName = m__G.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            //string sLotDir = m__G.m_RootDirectory + "\\Result\\" + sLotName;
            //if (!Directory.Exists(sLotDir))
            //    Directory.CreateDirectory(sLotDir);
            //if (sLotName != "")
            //{
            //    sFilePath = m__G.m_RootDirectory + "\\Result\\" + sLotName + "\\" + fileName;
            //}
            //else
            //    sFilePath = m__G.m_RootDirectory + "\\Result\\" + fileName;

            string sLotDir = CheckResultFolder();
            if (sLotName != "")
                sLotDir = sLotDir + sLotName;


            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);
            sFilePath = sLotDir + "\\" + fileName;

            StreamWriter writer;
            if (!File.Exists(sFilePath))
            {
                MyOwner.AddHeadLineToResult(sFilePath);
            }

            if (!MyOwner.IsAccessAbleFile(sFilePath))
            {
                MessageBox.Show("Unable to Write Result File. Check if it is open! : \n" + sFilePath);
            }
            else
            {
                MyOwner.HoldMutex(3);
                writer = File.AppendText(sFilePath);
                if (results[0] != "")
                    writer.WriteLine(results[0]);
                if (results[1] != "")
                    writer.WriteLine(results[1]);
                writer.Close();
                MyOwner.FreeMutex(3);
            }
        }
        void Process_ReceiveResultData(int channel, string Data)
        {
            return;
            try
            {
                string[] results;
                double[] dResults;
                string testItem = Data.Substring(0, 2);
                m__G.sCIndex[channel] = Convert.ToInt32(Data.Substring(2, 4)) + 5000;

                Data = Data.Substring(6, Data.Length - 6);
                if (Convert.ToInt32(testItem) > -1)
                {
                    results = Data.Split(',');
                    dResults = new double[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        dResults[i] = double.Parse(results[i]);
                    }
                }
                else
                {
                    results = new string[1];
                    dResults = new double[1];
                }

                switch (testItem)
                {
                    case "-1":
                        break;
                    case "02":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_SettlingTime] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_Overshoot] = dResults[1];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_StepStroke] = dResults[2];
                        break;
                    case "03":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_SettlingTime] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_Overshoot] = dResults[1];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_StepStroke] = dResults[2];
                        break;
                    case "04":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_Fullstroke] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FwdStroke] = dResults[1];   //  Positive Stroke
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_BwdStroke] = dResults[2];    //  Negative Stroke
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_Resolution] = dResults[16];    //  Negative Stroke
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FullSensitivity] = dResults[12];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FullLinearity] = dResults[13];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FullHysteresis] = dResults[14];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FullCrosstalk] = dResults[15];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FwdSensitivity] = dResults[3];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FwdLinearity] = dResults[4];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FwdHysteresis] = dResults[5];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_FwdCrosstalk] = dResults[6];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_MaxCodeCurrent] = dResults[7];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_BwdSensitivity] = dResults[8];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_BwdLinearity] = dResults[9];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_BwdHysteresis] = dResults[10];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.AF_BwdCrosstalk] = dResults[11];
                        break;
                    case "05":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_Fullstroke] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FwdStroke] = dResults[1];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_BwdStroke] = dResults[2];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_Resolution] = dResults[16];    //  Negative Stroke
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FullSensitivity] = dResults[12];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FullLinearity] = dResults[13];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FullHysteresis] = dResults[14];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FullCrosstalk] = dResults[15];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FwdSensitivity] = dResults[3];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FwdLinearity] = dResults[4];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FwdHysteresis] = dResults[5];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_FwdCrosstalk] = dResults[6];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_MaxCodeCurrent] = dResults[7];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_BwdSensitivity] = dResults[8];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_BwdLinearity] = dResults[9];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_BwdHysteresis] = dResults[10];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.ZM_BwdCrosstalk] = dResults[11];
                        break;
                    case "07":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PMFreq] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PhaseMargin] = dResults[1];
                        break;
                    case "08":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PMFreq] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PhaseMargin] = dResults[1];
                        break;
                    case "09":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PMFreq] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PhaseMargin] = dResults[1];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PMFreq] = dResults[3];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PhaseMargin] = dResults[4];
                        break;
                    case "19":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PMFreq2nd] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_PhaseMargin2nd] = dResults[1];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PMFreq2nd] = dResults[3];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_PhaseMargin2nd] = dResults[4];
                        break;
                    case "10":
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAAF_Gain10Hz] = dResults[0];
                        m__G.sHistArray[m__G.sCIndex[channel], (int)Global.SpecItem.FRAZM_Gain10Hz] = dResults[1];
                        break;
                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        void Process_ReceiveCodeCurrent(int channel, string Data, int axis)
        {
            try
            {
                string[] results = Data.Split(',');

                //if (axis == 0)
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            AF_CodeCurrent[channel, i] = double.NaN;
                //        else
                //            AF_CodeCurrent[channel, i] = double.Parse(results[i]);



                //    }

                //    if (channel == 2)
                //        F_Main.ReceiveCodeCurrentXch3 = true;
                //    else
                //        F_Main.ReceiveCodeCurrentXch4 = true;
                //}
                //else
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            ZM_CodeCurrent[channel, i] = double.NaN;
                //        else
                //            ZM_CodeCurrent[channel, i] = double.Parse(results[i]);
                //    }
                //    if (channel == 2)
                //        F_Main.ReceiveCodeCurrentYch3 = true;
                //    else
                //        F_Main.ReceiveCodeCurrentYch4 = true;
                //}




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        void Process_ReceiveCodeHall(int channel, string Data, int axis)
        {
            try
            {
                string[] results = Data.Split(',');
                //if (axis == 0)
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            AF_CodeHall[channel, i] = double.NaN;
                //        else
                //            AF_CodeHall[channel, i] = double.Parse(results[i]);
                //    }
                //    if (channel == 2)
                //        F_Main.ReceiveCodeHallXch3 = true;
                //    else
                //        F_Main.ReceiveCodeHallXch4 = true;
                //}
                //else
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            ZM_CodeHall[channel, i] = double.NaN;
                //        else
                //            ZM_CodeHall[channel, i] = double.Parse(results[i]);
                //    }
                //    if (channel == 2)
                //        F_Main.ReceiveCodeHallYch3 = true;
                //    else
                //        F_Main.ReceiveCodeHallYch4 = true;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        void Process_ReceiveCodeStroke(int channel, string Data, int axis)
        {
            try
            {

                string[] results = Data.Split(',');
                //if (axis == 0)
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            AF_CodeStroke[channel, i] = double.NaN;
                //        else
                //            AF_CodeStroke[channel, i] = double.Parse(results[i]);
                //    }
                //    if (channel == 2)
                //        F_Main.ReceivedCodeStrokeXch3 = true;
                //    else
                //        F_Main.ReceivedCodeStrokeXch4 = true;
                //}
                //else
                //{
                //    for (int i = 0; i < results.Length; i++)
                //    {
                //        if (results[i] == "NaN")
                //            AF_CodeStroke[channel, i] = double.NaN;
                //        else
                //            ZM_CodeStroke[channel, i] = double.Parse(results[i]);
                //    }
                //    if (channel == 2)
                //        F_Main.ReceivedCodeStrokeYch3 = true;
                //    else
                //        F_Main.ReceivedCodeStrokeYch4 = true;

                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
        #region 자세차 검사기 통신 함수 
        public void SetPorts(int serverPort, int clientPort)
        {
            SERVER_PORT_NO = serverPort;
            CLIENT_PORT_NO = clientPort;
        }
        public void GetPorts(ref int serverPort, ref int clientPort)
        {
            serverPort = SERVER_PORT_NO;
            clientPort = CLIENT_PORT_NO;
        }

        //public void ServerMain() //hch
        //{
        //    bRunning = true;
        //    IsRun = true;
        //    //---listen at the specified IP and port no.---
        //    MY_IP = Dns.GetHostAddresses(Dns.GetHostName());

        //    //---listen at the specified IP and port no.---
        //    IPAddress localAdd = MY_IP[1];
        //    //MessageBox.Show(localAdd.ToString() + " " + SERVER_PORT_NO.ToString());
        //    TcpListener listener = new TcpListener(localAdd, SERVER_PORT_NO);
        //    if ( InvokeRequired)
        //    {
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            this.Text = localAdd.ToString();
        //        });
        //    }
        //    else
        //        this.Text = localAdd.ToString();


        //    listener.Server.ReceiveTimeout = 1000;
        //    listener.Start();
        //    //MessageBox.Show(localAdd.ToString());
        //    TcpClient client;

        //    while (true)
        //    {
        //        // Connect
        //        if (!listener.Pending())
        //        {
        //            if (!IsRun) //  Stop Listening
        //                break;

        //            Thread.Sleep(100);
        //            continue;
        //        }
        //        client = listener.AcceptTcpClient();
        //        // MessageBox.Show("___aaa");

        //        // Receive
        //        NetworkStream nwStream = client.GetStream();
        //        byte[] buffer = new byte[client.ReceiveBufferSize];
        //        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        //        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        //        // Decode Receoved Message
        //        string[] separator = { ",", "__ ", "\r\n" };
        //        string[] sepratedData = dataReceived.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        //        int wordCount = sepratedData.Length;

        //        // Send
        //        byte[] returnData = new byte[2] { (byte)('O'), (byte)('K') };
        //        nwStream.Write(returnData, 0, 2);
        //        client.Close();
        //        MyOwner.SetTCPReceived(sepratedData[0]);
        //        // Show
        //        //tbReceive.Text = wordCount.ToString() + ": ";
        //        //for (int i = 0; i < wordCount; i++ )
        //        //    tbReceive.Text += sepratedData[i] + "\t";
        //        //MessageBox.Show(dataReceived + " : " + sepratedData[0]);
        //        switch (sepratedData[0])
        //        {
        //            case "ForcedStop":
        //                btnSuddenStop.PerformClick();
        //                break;
        //            case "Start_Test":
        //                //if (F_Main.MachineType == (int)MachineType.Handler)
        //                //    m__G.fGraph.RunTest();
        //                break;
        //            case "UserBreak":
        //                F_Main.isButtonPushed = false;
        //                break;

        //            case "Position":
        //                Thread.Sleep(200);
        //                F_Main.CurrentInspPosBlock = sepratedData[1];
        //                F_Main.CurrentInspPosSocket = sepratedData[2];
        //                if (F_Main.isUIButton)
        //                {

        //                }

        //                else
        //                {
        //                    m__G.fGraph.RunTest();
        //                }

        //                // m__G.fGraph.RunSequence_auto_single();
        //                break;
        //            case "MoveComplete":
        //                Thread.Sleep(300);
        //                F_Main.CurrentInspPosBlock = sepratedData[1];
        //                F_Main.CurrentInspPosSocket = sepratedData[2];
        //                F_Main.isTesting = true;
        //                //m__G.fGraph.RunTestOnCommand();
        //                break;
        //            case "MoveFail":
        //                break;
        //            case "Finish":
        //                // m__G.fGraph.NowTesting = false;
        //                F_Main.isButtonPushed = false;

        //                break;
        //            case "Results":
        //                //  After receiving Test Result from Counter PC, Save results 
        //                break;
        //            case "Listen":
        //                break;
        //            case "CounterIP":
        //                break;

        //            default:
        //                break;
        //        }

        //        Thread.Sleep(100);
        //        if (dataReceived == "Stop") //  Stop Listening
        //            break;
        //        if (!IsRun) //  Stop Listening
        //            break;
        //    }
        //    listener.Stop();
        //    IsClosed = true;
        //    bRunning = false;
        //    bRunning = false;
        //}
        public void ClientMain(string sendText) //hch
        {
            //---data to send to the server---
            if (!m_bClientAvailable) return;

            string textToSend = sendText;
            String YOUR_IP = MY_IP[1].ToString();
            //   MessageBox.Show("CLIENT_PORT_NO : " + CLIENT_PORT_NO);
            try
            {
                //      MessageBox.Show(YOUR_IP);
                //  Send
                TcpClient client = new TcpClient(YOUR_IP, CLIENT_PORT_NO);   //  상대방
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //  Receive
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                client.Close();
            }
            catch (SocketException e)
            {
                AddOperatorLog(0, "Fail to connect server : Server Down \r\rTurn On server and restart Tester Program", false);
                m_bClientAvailable = false;
            }
        }


        #endregion

        private void btnSlaveStart_Click(object sender, EventArgs e)
        {
            string tmpData = "RunTester";

            string data = (char)0x02 + tmpData.Length.ToString("D2") + tmpData + (char)0x03;

            MonitoringCOMM.SendMessage(data);
        }

        private void btnSlaveTerminate_Click(object sender, EventArgs e)
        {
            string tmpData = "TerminateTester";
            MonitoringCOMM.SendMessage((char)0x02 + tmpData.Length.ToString("D2") + tmpData + (char)0x03);
        }

        public string CheckResultFolder()
        {
            DateTime dt = DateTime.Now;
            string resDirectory = "C:\\CSHTest\\Data\\" + dt.Year + "\\" + dt.Month + "\\" + dt.Day + "\\";
            if (!Directory.Exists(resDirectory))
                Directory.CreateDirectory(resDirectory);
            return resDirectory;
        }

        private void FManage_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                ModelBtn.Text = "Model " + m__G.sModelName;
                txtMsaterNuma.Text = m__G.fVision.GetMasterZeroIndex().ToString();
               //DebugStopBtn.Visible = m__G.m_bDebugMode;
            }
        }
        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Admin") || lbtn.Text.Contains("Vision") || lbtn.Text.Contains("Statistic") || lbtn.Text.Contains("Screen"))
                lbtn.BackgroundImage = Properties.Resources.BtnAP;
            else if (lbtn.Text.Contains("Clear") || lbtn.Text.Contains("Model"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.Text.Contains("Start") || lbtn.Text.Contains("Stop") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("Set") || lbtn.Text.Contains("Check"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
        }

        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Admin") || lbtn.Text.Contains("Vision") || lbtn.Text.Contains("Statistic") || lbtn.Text.Contains("Screen"))
                lbtn.BackgroundImage = Properties.Resources.BtnAN;
            else if (lbtn.Text.Contains("Clear") || lbtn.Text.Contains("Model"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.Text.Contains("Start") || lbtn.Text.Contains("Stop") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("Set") || lbtn.Text.Contains("Check"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Admin") || lbtn.Text.Contains("Vision") || lbtn.Text.Contains("Statistic") || lbtn.Text.Contains("Screen"))
                lbtn.BackgroundImage = Properties.Resources.BtnAP;
            else if (lbtn.Text.Contains("Clear") || lbtn.Text.Contains("Model"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.Text.Contains("Start") || lbtn.Text.Contains("Stop") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("Set") || lbtn.Text.Contains("Check"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
        }

        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Admin") || lbtn.Text.Contains("Vision") || lbtn.Text.Contains("Statistic") || lbtn.Text.Contains("Screen"))
                lbtn.BackgroundImage = Properties.Resources.BtnAN;
            else if (lbtn.Text.Contains("Clear") || lbtn.Text.Contains("Model"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.Text.Contains("Start") || lbtn.Text.Contains("Stop") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("Set") || lbtn.Text.Contains("Check"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxT_SelectedIndexChanged(sender, e);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxT_SelectedIndexChanged(sender, e);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxT_SelectedIndexChanged(sender, e);
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxT_SelectedIndexChanged(sender, e);
        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox_SelectedIndexChanged(sender, e);
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox_SelectedIndexChanged(sender, e);
        }

        private void listBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        //private void listBox8_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    listBoxM_SelectedIndexChanged(sender, e);
        //}
        public void listBoxT_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  Time to any Chart 1 / 2
            if (sender == listBox1 || sender == listBox2)
            {
                int[] selectedIndices1 = new int[listBox1.SelectedIndices.Count];
                int[] selectedIndices2 = new int[listBox2.SelectedIndices.Count];
                listBox1.SelectedIndices.CopyTo(selectedIndices1, 0);
                listBox2.SelectedIndices.CopyTo(selectedIndices2, 0);
                if (selectedIndices1.Length == 0 || selectedIndices2.Length == 0)
                    return;

                PlotChartStroke(0, selectedIndices1, selectedIndices2, mCommonDataCount);
            }
            else if (sender == listBox3 || sender == listBox4)
            {
                int[] selectedIndices1 = new int[listBox3.SelectedIndices.Count];
                int[] selectedIndices2 = new int[listBox4.SelectedIndices.Count];
                listBox3.SelectedIndices.CopyTo(selectedIndices1, 0);
                listBox4.SelectedIndices.CopyTo(selectedIndices2, 0);

                if (selectedIndices1.Length == 0 || selectedIndices2.Length == 0)
                    return;

                PlotChartStroke(1, selectedIndices1, selectedIndices2, mCommonDataCount);
            }
            SaveChartListBoxChange();
        }
        public void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  Any to any  for Chart 3
            SaveChartListBoxChange();
            PlotChartStroke(2, listBox5.SelectedIndex, listBox6.SelectedIndex, -1, mCommonDataCount);
        }
        public void PlotChartMarkFromListBox7()
        {
            //  Marks for Chart 4
            int[] selectedIndices = null;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    selectedIndices = new int[listBox7.SelectedIndices.Count];
                    listBox7.SelectedIndices.CopyTo(selectedIndices, 0);
                    SaveChartListBoxChange();
                    PlotChartMark(3, selectedIndices, mCommonDataCount);
                });
            }else
            {
                selectedIndices = new int[listBox7.SelectedIndices.Count];
                listBox7.SelectedIndices.CopyTo(selectedIndices, 0);
                SaveChartListBoxChange();
                PlotChartMark(3, selectedIndices, mCommonDataCount);
            }

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            PlotChartMarkFromListBox7();
        }
        public void SaveChartListBoxChange()
        {
            if (!mbFManageLoaded)
                return;

            string filePath = m__G.m_RootDirectory + "\\DoNotTouch\\ChartConfig.txt";
            StreamWriter wr = new StreamWriter(filePath);
            int[] selectedIndices = null;
            //wr.WriteLine(listBox1.SelectedIndex);
            //wr.WriteLine(listBox2.SelectedIndex);
            //wr.WriteLine(listBox3.SelectedIndex);
            //wr.WriteLine(listBox4.SelectedIndex);
            if (listBox1.SelectedIndices.Count > 0)
            {
                selectedIndices = new int[listBox1.SelectedIndices.Count];
                listBox1.SelectedIndices.CopyTo(selectedIndices, 0);
                wr.WriteLine(string.Join(",", selectedIndices));
            }
            else
                wr.WriteLine("0");

            if (listBox2.SelectedIndices.Count > 0)
            {
                selectedIndices = new int[listBox2.SelectedIndices.Count];
                listBox2.SelectedIndices.CopyTo(selectedIndices, 0);
                wr.WriteLine(string.Join(",", selectedIndices));
            }
            else
                wr.WriteLine("0");

            if (listBox3.SelectedIndices.Count > 0)
            {
                selectedIndices = new int[listBox3.SelectedIndices.Count];
                listBox3.SelectedIndices.CopyTo(selectedIndices, 0);
                wr.WriteLine(string.Join(",", selectedIndices));
            }
            else
                wr.WriteLine("0");

            if (listBox4.SelectedIndices.Count > 0)
            {
                selectedIndices = new int[listBox4.SelectedIndices.Count];
                listBox4.SelectedIndices.CopyTo(selectedIndices, 0);
                wr.WriteLine(string.Join(",", selectedIndices));
            }
            else
                wr.WriteLine("0");

            wr.WriteLine(listBox5.SelectedIndex);
            wr.WriteLine(listBox6.SelectedIndex);

            if (listBox4.SelectedIndices.Count > 0)
            {
                selectedIndices = new int[listBox7.SelectedIndices.Count];
                listBox7.SelectedIndices.CopyTo(selectedIndices, 0);
                wr.WriteLine(string.Join(",", selectedIndices));
            }
            else
                wr.WriteLine("0");

            wr.Close();
        }
        public void LoadChartListBoxChange()
        {
            string filePath = m__G.m_RootDirectory + "\\DoNotTouch\\ChartConfig.txt";
            if (!File.Exists(filePath))
                return;

            StreamReader rr = new StreamReader(filePath);

            string all = rr.ReadToEnd();
            rr.Close();
            string[] eachItem = null;

            string[] lines = all.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            eachItem = lines[0].Split(",".ToCharArray());
            foreach (string lstr in eachItem)
                listBox1.SelectedIndices.Add(int.Parse(lstr));

            eachItem = lines[1].Split(",".ToCharArray());
            foreach (string lstr in eachItem)
                listBox2.SelectedIndices.Add(int.Parse(lstr));

            eachItem = lines[2].Split(",".ToCharArray());
            foreach (string lstr in eachItem)
                listBox3.SelectedIndices.Add(int.Parse(lstr));

            eachItem = lines[3].Split(",".ToCharArray());
            foreach (string lstr in eachItem)
                listBox4.SelectedIndices.Add(int.Parse(lstr));

            //listBox1.SelectedIndex = int.Parse(lines[0]);
            //if (lines.Length < 2)
            //    return;
            //listBox2.SelectedIndex = int.Parse(lines[1]);
            //if (lines.Length < 3)
            //    return;
            //listBox3.SelectedIndex = int.Parse(lines[2]);
            //if (lines.Length < 4)
            //    return;
            //listBox4.SelectedIndex = int.Parse(lines[3]);
            if (lines.Length < 5)
                return;
            listBox5.SelectedIndex = int.Parse(lines[4]);
            if (lines.Length < 6)
                return;
            listBox6.SelectedIndex = int.Parse(lines[5]);
            if (lines.Length < 7)
                return;

            eachItem = lines[6].Split(",".ToCharArray());
            foreach (string lstr in eachItem)
                listBox7.SelectedIndices.Add(int.Parse(lstr));

        }

        private void cbRealTimeMeasure_CheckedChanged(object sender, EventArgs e)
        {
            ChartlayoutRealtimeMeasure(cbRealTimeMeasure.Checked);
        }
        public string SaveScreenShot(string strHost)
        {
            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            string pngname = strHost + dtNow.ToString("yyMMddhhmmss") + ".png";
            string sScreenCapturePath = m__G.m_RootDirectory + "\\User_ScreenShot\\" + pngname;
            string sDir = m__G.m_RootDirectory + "\\User_ScreenShot";
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);
            Size s = new Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);


            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
            memoryImage.Save(sScreenCapturePath);
            return sScreenCapturePath;
        }
        //============= Anosis Motion ==================
        public bool isReceivedH = false; //home position done
        public bool isReceivedG = false; //get position done
        public bool[] isReceivedM = new bool[6]; //motion done
        public bool isReceivedP = false; //probe data done
        public bool isReceivedMD = false; //measure data done

        public bool isReceived = false;
        public double[] motionPos = new double[6];
        public double[] probeData = new double[6];
        public byte[] MakeMotionData(double x, double y, double z, double tx, double ty, double tz)
        {
            byte[] sDatabuffer = new byte[48];
            int curCount = 0;

            byte[] data = BitConverter.GetBytes(x);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            curCount += data.Length;
            data = BitConverter.GetBytes(y);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            curCount += data.Length;
            data = BitConverter.GetBytes(z);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            curCount += data.Length;
            data = BitConverter.GetBytes(tx);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            curCount += data.Length;
            data = BitConverter.GetBytes(ty);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            curCount += data.Length;
            data = BitConverter.GetBytes(tz);
            Array.Copy(data, 0, sDatabuffer, curCount, data.Length);

            return sDatabuffer;
        }
        public void M_H()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_H@\r\n"));
            Network.SendData(sendBuf);

            int timeout = 0;
            isReceivedH = false;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 100 || isReceivedH)
                {
                    break;
                }
                timeout++;
            }
        }
        public void M_C()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_C@\r\n"));
            Network.SendData(sendBuf);
        }
        public void A_I()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("A_I@\r\n"));
            Network.SendData(sendBuf);
        }
        public void M_A(double x, double y, double z, double tx, double ty, double tz)
        {
            byte[] sCmdBuf = null;
            byte[] sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
            byte[] sendBuf = null;
            byte[] sDatabuffer;

            sCmdBuf = Encoding.ASCII.GetBytes(string.Format("M_A@6@"));
            sDatabuffer = MakeMotionData(x, y, z, tx, ty, tz);
            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];
            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);


            for (int i = 0; i < 6; i++) isReceivedM[i] = false;

            Network.SendData(sendBuf);

            int timeout = 0;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 200 || (isReceivedM[0] && isReceivedM[1] && isReceivedM[2] && isReceivedM[3] && isReceivedM[4] && isReceivedM[5]))
                {
                    if (timeout > 200)
                    {
                        if (InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                AddViewLog(string.Format(string.Format("timeout measure,\r\n")));
                            });
                        }
                    }
                    break;
                }
                timeout++;
            }
        }
        public void M_M(int axis, double val)
        {
            byte[] sCmdBuf = null;
            byte[] sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
            byte[] sendBuf = null;
            byte[] sDatabuffer;
            byte[] dataBef;

            sCmdBuf = Encoding.ASCII.GetBytes(string.Format("M_M@{0}@", axis));
            sDatabuffer = new byte[8];
            dataBef = BitConverter.GetBytes(val);
            Array.Copy(dataBef, 0, sDatabuffer, 0, dataBef.Length);
            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];
            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

            Network.SendData(sendBuf);

            isReceivedM[axis] = false;
            int timeout = 0;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 1000 || isReceivedM[axis])
                {
                    break;
                }
                timeout++;
            }
        }
        public double M_G(int axis)
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_G@{0}@\r\n", axis));
            Network.SendData(sendBuf);

            int timeout = 0;
            isReceivedG = false;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 100 || isReceivedG)
                {
                    break;
                }
                timeout++;
            }
            return motionPos[axis];
        }
        public double[] M_P()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_P@\r\n"));
            Network.SendData(sendBuf);

            int timeout = 0;
            isReceivedG = false;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 100 || isReceivedG)
                {
                    break;
                }
                timeout++;
            }

            double[] pos = new double[6];
            for (int i = 0; i < 6; i++)
            {
                pos[i] = motionPos[i];
            }
            return pos;
        }
        public double[] P_B()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("P_B@\r\n"));
            Network.SendData(sendBuf);

            int timeout = 0;
            isReceivedP = false;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 100 || isReceivedP)
                {
                    break;
                }
                timeout++;
            }
            return probeData;
        }
        public void M_J(int axis, bool dir, SpeedLevel speed)
        {
            M_S(speed);

            byte[] sCmdBuf = null;
            byte[] sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
            byte[] sendBuf = null;
            byte[] sDatabuffer;
            byte[] dataBef;

            sCmdBuf = Encoding.ASCII.GetBytes(string.Format("M_J@{0}@", axis));
            sDatabuffer = new byte[8];
            dataBef = BitConverter.GetBytes(dir);
            Array.Copy(dataBef, 0, sDatabuffer, 0, dataBef.Length);
            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];
            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

            Network.SendData(sendBuf);
        }
        public void M_E(int axis)
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_E@{0}@\r\n", axis));
            Network.SendData(sendBuf);
        }
        public void M_D()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_D@\r\n"));
            Network.SendData(sendBuf);
            isReceivedMD = false;
            int timeout = 0;
            while (true)
            {
                Thread.Sleep(100);
                if (timeout > 100 || isReceivedMD)
                {
                    break;
                }
                timeout++;
            }
        }
        public void M_O()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_O@\r\n"));
            Network.SendData(sendBuf);
        }
        public void M_Z()
        {
            byte[] sendBuf = Encoding.ASCII.GetBytes(string.Format("M_Z@\r\n"));
            Network.SendData(sendBuf);
        }
        public void M_S(SpeedLevel level)
        {
            byte[] sCmdBuf = null;
            byte[] sRnBuf = Encoding.ASCII.GetBytes("@\r\n");
            byte[] sendBuf = null;
            byte[] sDatabuffer = null;

            sCmdBuf = Encoding.ASCII.GetBytes(string.Format("M_S@6@"));
            switch (level)
            {
                case SpeedLevel.Slow:
                    sDatabuffer = MakeMotionData(10, 10, 10, 10, 10, 10);
                    break;
                case SpeedLevel.Normal:
                    sDatabuffer = MakeMotionData(100, 100, 100, 100, 100, 100);
                    break;
                case SpeedLevel.Fast:
                    sDatabuffer = MakeMotionData(1000, 1000, 1000, 1000, 1000, 1000);
                    break;
            }

            sendBuf = new byte[sCmdBuf.Length + sDatabuffer.Length + sRnBuf.Length];
            Array.Copy(sCmdBuf, 0, sendBuf, 0, sCmdBuf.Length);
            Array.Copy(sDatabuffer, 0, sendBuf, sCmdBuf.Length, sDatabuffer.Length);
            Array.Copy(sRnBuf, 0, sendBuf, sCmdBuf.Length + sDatabuffer.Length, sRnBuf.Length);

            Network.SendData(sendBuf);
        }

        public void GetPositionTask(int mode, int axis)
        {
            if (mode == 0)
            {
                double val = M_G(axis);
            }
            else
            {
                double[] val = M_P();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MyOwner.DebugStop = true;

            //TriggerLossDetected();

            //Thread.Sleep(2000);

            //TriggerLossClear();
        }

        public string mStrDummyIndex = "0";
        private void button3_Click(object sender, EventArgs e)
        {
            m__G.fVision.SetMasterZeroIndex(3);
            m__G.fVision.LoadTXTYZeroOffset();
            txtMsaterNuma.Text = mStrDummyIndex = m__G.fVision.GetMasterZeroIndex().ToString();

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            m__G.fVision.SetMasterZeroIndex(0);
            m__G.fVision.LoadTXTYZeroOffset();
            txtMsaterNuma.Text = mStrDummyIndex = m__G.fVision.GetMasterZeroIndex().ToString();

        }

        private void BtnPortSet_Click(object sender, EventArgs e)
        {
            string strFileName = "C:\\CSHTest\\DoNotTouch\\PortNumber.txt";

            StreamWriter wr = new StreamWriter(strFileName);
            wr.WriteLine(ServerPort.Text);
            wr.Close();
            Network.StopServer();
            Thread.Sleep(1000);
            Network.StartServer(int.Parse(ServerPort.Text));
        }
    }
}
