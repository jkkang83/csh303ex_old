
using System;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.IO.Ports;
using System.Globalization;//check170123
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
//using static alglib;
using System.Xml.Serialization;
using static FAutoLearn.FAutoLearn;
using S2System.Vision;

namespace CSH030Ex
{
    public enum MachineType
    { Normal, Master, Slave, Handler}

    public partial class F_Main : Form
    {
        #region 3720 2PC 관련 변수
        public static int MachineType = 0;
        public static bool ConnectionStatus = false;
        public static string SendLogDataCh3 = string.Empty;
        public static string SendLogDataCh4 = string.Empty;
        public static string Remainder = string.Empty;
        public static int DataLength = 0;
        public static int ReceivedChannel = 0;
        public static string ReceivedCommand = string.Empty;
        public static string ReceivedData = string.Empty;
        public static bool ReceiveCodeHallXch3 = false;
        public static bool ReceiveCodeHallYch3 = false;
        public static bool ReceiveCodeCurrentXch3 = false;
        public static bool ReceiveCodeCurrentYch3 = false;
        public static bool ReceiveCodeHallXch4 = false;
        public static bool ReceiveCodeHallYch4 = false;
        public static bool ReceiveCodeCurrentXch4 = false;
        public static bool ReceiveCodeCurrentYch4 = false;
        public static bool ReceivedCodeStrokeXch3 = false;
        public static bool ReceivedCodeStrokeYch3 = false;
        public static bool ReceivedCodeStrokeXch4 = false;
        public static bool ReceivedCodeStrokeYch4 = false;
        public static string SendParameterState = string.Empty;
        public static System.Timers.Timer UIControltmr = null;
        public static bool isSlaveChanged = false;
        #endregion
        #region 3720자세차 변수
        public static string CurrentInspPosBlock = "Unknown";
        public static string CurrentInspPosSocket = "Unknown";
        public static bool iscommand = false;
        public static bool isTesting = false;
        public static bool MotionUsed = false;
        public static bool isButtonPushed = false;
        public static bool isUIButton = false;
        #endregion

        private Global m__G = null;
        private Form[] m_form = null;
        private const bool m_bNewCreateSubForm = false;     //** 중요 : Sub Form 전환시 Form 이벤트 발생하지 않는다. 대신 전환 속도 빠르다. **//
        //private int m_CurrentForm = (int)Page.CALIBRATE;    // 최초 항상 enum의 0번 항목 셋팅됨

        public string mInitialMsg = "";

        public int m_OrgROI_min;
        public int m_OrgROI_max;
        public int m_OrgExposure;
        public int MaxHistory = 10000;

        public int TimeToAFRes;
        public bool m_IsRepeat = false;
        public bool[] m_StopRepeat = new bool[2] { true, true };
        public int m_InvChannel = 0;

        public bool m_bAdmin = true;
        public bool[] IsTesting = new bool[2] { false, false };
        public bool m_bScopeMode = false;
        public bool m_bCLXYCALfinish = false;
        public bool m_bCBreading = false;
        public bool mbSpecChanged = false;

        public double mMarkToPrism = 11316 - 3390; //  um Typitcal value, 
        public string[] mUserScriptFile = new string[5] { "", "", "", "", "" };
        public string mBU252FWfile = "";
        //   public string m2810UpdateFWfile = "";
        public string UpdateFWFile = "";
        public string UpdatePreFWFile = "";
        public string[] mUpdatePIDfile = new string[1] { "" };
        public int[] mRumbaS4_FWLength = new int[2] { 28 * 1024, 32 * 1024 };
        public int mRumbaS6_FWLength = 45056;
        public bool m_bFinishLoadMain = false;
        public bool m_bFWFileReading = false;
        public bool m_bSaveMovieHallCal = false;
        public bool m_bSaveResIndexCount = true;
        public bool m_bBarcodeUse = false;
        public bool[] m_bHallCalPass = new bool[4] { false, false, false, false };

        public int[] m_RepeatProcess = new int[2] { 0, 0 };
        public string[] m_RepeatRawFile = null;

        double[,] m_ItrStroke_L2 = new double[5, 5000];
        double[,] m_ItrStroke_L3 = new double[5, 5000];
        double[,] m_ItrStroke_L2R = new double[5, 5000];
        double[,] m_ItrStroke_L3R = new double[5, 5000];

        public int[] m_bProcess_FWupdate_OK = new int[4];
        public int[] m_bProcess_PIDupdate_OK = new int[4];
        public char m_BarcodeMaker = 'A';
        public byte m_BarcodeRevNo;
        public byte m_ProductLine;
        public byte m_PrismSupplier;
        public byte m_ICCheck = 0;
        public int[] m_nIndexSkipped = new int[4];
        public int m_Mutex = 0;
        float[,] mLinearity = new float[4, 4];

        public static bool FWStatusCh1 = true;
        public static bool FWStatusCh2 = true;
        public static bool FWStatusCh3 = true;
        public static bool FWStatusCh4 = true;

        public static bool isFirstPosition = false;
        public bool mbDlnAvailable = true;

        public Form2 sForm;

        public string mBU252Calfile = "";

        public byte[][] mNTBRSTdata = new byte[1000][];
        public UInt16[] mNTBRSTaddr = new UInt16[1000];
        public byte[][] mBU252CALdata = new byte[100][];
        public UInt16[] mBU252CALaddr = new UInt16[100];


        Thread ThreadServer = null;

        private string[] mToDoList = {
                                        "Triggered Grab",
                                        "Bulk Grab",
                                        "Continuous Grab",
                                        };
        private enum Page
        { MANAGE, VISION, STATISTICS, NUM_OF_PAGE, GRAPH, DRAW, RUN, CALIBRATE, FORMAT };  // yjh 2016/10.31
        private enum Type
        { NONE = 0, AF, AFRes, OISX, OISY, XSTEP, YSTEP, LENSAFZM, LENSAF, LENSZM, SPECIAL, NUM_OF_TYPE };

        public static List<DeviceCompactInfo> GetPnPDeviceInfo(string captionLikeCondition)
        {
            var selectQuery = "SELECT Caption, Description, Manufacturer, SystemName, DeviceID From Win32_PnPEntity ";
            var query = string.Format("{0} WHERE ConfigManagerErrorCode = 0 and Caption like '{1}' ", selectQuery, captionLikeCondition);
            var searcher = new System.Management.ManagementObjectSearcher(query);
            var pnpList = searcher.Get().Cast<System.Management.ManagementBaseObject>()
                .Select(x => new DeviceCompactInfo
                {
                    Name = Convert.ToString(x["Caption"]),
                    Description = Convert.ToString(x["Description"]),
                    Manufacturer = Convert.ToString(x["Manufacturer"]),
                    SystemName = Convert.ToString(x["SystemName"]),
                    DeviceID = Convert.ToString(x["DeviceID"]),
                })
                .ToList();

            return pnpList;
        }

        [System.Diagnostics.DebuggerDisplay("Name:{Name}, Description:{Description}, Manufacturer:{Manufacturer}, SystemName:{SystemName}, DeviceID:{DeviceID}", Name = "DeviceCompactInfo")]
        public class DeviceCompactInfo
        {
            public string Name
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public string Manufacturer
            {
                get;
                set;
            }

            public string SystemName
            {
                get;
                set;
            }

            public string DeviceID
            {
                get;
                set;
            }
        }

        public int mImageGrabberType = 0;   //  SOLIOS = 1, RADIENT = 2, ...
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public F_Main()
        {
            try
            {
                sForm = new Form2();
                sForm.Show();
                sForm.BringToFront();

                m__G = Global.GetInstance();
                //            m__G.m_RootDirectory = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
                InitializeComponent();

                m__G.fManage.MyOwner = this;
                m__G.fManage.On_LoadData += FManage_On_LoadData;
                m__G.fManage.On_SaveData += FManage_On_SaveData;
                m__G.fManage.On_Save_CBstate += FManage_On_Save_CBstate;

                UIControltmr = new System.Timers.Timer();
                UIControltmr.Interval = 3000;
                UIControltmr.Enabled = false;
                UIControltmr.Elapsed += UIControltmr_Elapsed;


                List<string> lDeviceList = GetPnPDeviceInfo("%Matrox%").Select(x => x.Name).ToList();
                string[] lDeviceStr = lDeviceList.ToArray();
                string lDeviceAll = "";
                for (int i = 0; i < lDeviceStr.Length; i++)
                    lDeviceAll += lDeviceStr[i] + "\r\n";

                Splasher.Show();
                if (lDeviceAll.Contains("Solios"))
                {
                    Splasher.Status = "Solios Detected!";
                    mImageGrabberType = 1;
                }
                else if (lDeviceAll.Contains("Radient"))
                {
                    Splasher.Status = "Radient Detected!";
                    mImageGrabberType = 2;
                }
                else
                {
                    Splasher.Status = lDeviceAll + " Detected!";
                    mImageGrabberType = 1;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UIControltmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UIControltmr.Enabled = false;
            if (!isSlaveChanged)
                MessageBox.Show(new Form() { TopMost = true }, "Check the Slave PC", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                isSlaveChanged = false;
        }

        private void FManage_On_Save_CBstate(string data)
        {
            try
            {
                bool tmpres = false;
                string[] results = data.Split(',');
                UpdateFWFile = results[0];

                mUpdatePIDfile[0] = results[1];
                mBU252FWfile = results[2];
                mBU252Calfile = results[3];
                SetPrevFWupdateFile();
                //SetPrevPIDupdateFile();

                String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "MakerBarcodeRev.txt";
                StreamWriter wr = new StreamWriter(strFileName);
                wr.WriteLine(results[4]);
                wr.WriteLine(results[5]);
                wr.WriteLine(results[6]);
                wr.WriteLine(results[7]);
                wr.WriteLine(results[9]);
                wr.Close();

                strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TesterNumber.txt";
                wr = new StreamWriter(strFileName);
                wr.WriteLine(results[8]);
                wr.Close();

                ReadProductMaker();


                strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "CBstates.txt";
                StreamWriter sw = new StreamWriter(strFileName);
                for (int i = 10; i < results.Length; i++)
                {
                    sw.WriteLine(results[i]);
                }
                sw.Close();
                tmpres = ReadPrevCBstates();
                if (tmpres)
                    m__G.fManage.PC2SendData("ACK", "SCB", 3, 2);
                else
                    m__G.fManage.PC2SendData("NAK", "SCB", 3, 2);
            }
            catch
            {
                m__G.fManage.PC2SendData("NAK", "SCB", 3, 2);
            }


        }

        private void FManage_On_SaveData(int type, string data)
        {
            try
            {
                string[] results = data.Split(',');

                if (type == 0)
                {
                    bool tmpres = false;
                    m__G.fGraph.mDriverIC.SetI2CClock(m__G.sRecipe.iI2Cclock, m__G.mCamCount, m__G.mChannelCount);
                    string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";
                    StreamWriter sw = new StreamWriter(sFilePath + results[0]);
                    sw.WriteLine(results[1]);
                    int i = 0;
                    for (i = 0; i < this.dataGridView2.Rows.Count; i++)
                    {
                        if (this.dataGridView2[1, i].Value.ToString().Contains("LED")) break;
                        sw.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + results[i + 2]);

                    }
                    sw.Close();

                    sw = new StreamWriter(m__G.m_RootDirectory + "DoNotTouch\\LEDPower.txt");
                    sw.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + m__G.sRecipe.iLEDcurrentLL);
                    sw.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + m__G.sRecipe.iLEDcurrentLR);
                    //sw.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + m__G.sRecipe.iLEDcurrentRL);
                    //sw.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + m__G.sRecipe.iLEDcurrentRR);
                    sw.Close();

                    tmpres = bLoadRecipe(results[0]);

                    if (!tmpres)
                        m__G.fManage.PC2SendData("NAK", "RSV", 3, 2);
                    else
                        m__G.fManage.PC2SendData("ACK", "RSV", 3, 2);



                }
                else
                {
                    string sFilePath = m__G.m_RootDirectory + "\\Spec\\" + results[0];
                    StreamWriter sw = new StreamWriter(sFilePath);
                    for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
                    {
                        sw.WriteLine(m__G.mTestItem[i, 1].ToString() + "\t" + results[i + 1]);
                    }
                    sw.Close();

                    ReadSpecFile(results[0]);
                    WritePrevSpecFile(results[0]);
                    setSpecFile(results[0]);
                    m__G.fManage.PC2SendData("ACK", "SSV", 3, 2);
                }
            }
            catch
            {
                if (type == 0)
                    m__G.fManage.PC2SendData("NAK", "RSV", 3, 2);
                else
                    m__G.fManage.PC2SendData("NAK", "SSV", 3, 2);

            }


        }

        private void FManage_On_LoadData(int type, string data)
        {
            try
            {
                if (type == 0)
                {
                    bool tmpres = false;
                    tmpres = bLoadRecipe(data);
                    if (!tmpres)
                        m__G.fManage.PC2SendData("NAK", "RLD", 3, 2);
                    else
                        m__G.fManage.PC2SendData("ACK", "RLD", 3, 2);

                }
                else
                {
                    // MessageBox.Show(data);
                    ReadSpecFile(data);
                    WritePrevSpecFile(data);
                    setSpecFile(data);
                    m__G.fManage.PC2SendData("ACK", "SLD", 3, 2);
                }
            }
            catch
            {
                if (type == 0)
                    m__G.fManage.PC2SendData("NAK", "RLD", 3, 2);
                else
                    m__G.fManage.PC2SendData("NAK", "SLD", 3, 2);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void F_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WriteCurrentCBstates();
                //SaveProductMaker();
                //m__G.fGraph.mDriverIC.CheckDACExists(0);

                //m__G.fGraph.mDriverIC.CheckDACExists(1);
                if (mbDlnAvailable)
                {
                    m__G.fGraph.mDriverIC.SetLEDpower(1, 0);
                    if (m__G.mChannelCount > 1)
                    {
                        m__G.fGraph.mDriverIC.SetLEDpower(2, 0);
                        m__G.fGraph.mDriverIC.SetFailLED(0, false);
                        m__G.fGraph.mDriverIC.SetFailLED(1, false);
                    }
                }

                m__G.CloseVision();

                for (int i = 0; i < (int)Page.NUM_OF_PAGE; i++)
                {
                    if (m_form[i]!= null)
                        m_form[i].Close();
                }

                bool isExit = false;

                if (MachineType == (int)CSH030Ex.MachineType.Normal)
                {
                    Process[] procList = Process.GetProcessesByName("Motion");
                    if (procList.Length > 0)
                    {
                        procList[0].CloseMainWindow();
                        isExit = procList[0].WaitForExit(1000);
                        if (isExit)
                            procList[0].Close();
                        else
                            procList[0].Kill();

                        ThreadServer.Abort();
                        ThreadServer = null;
                    }
                }
                else if (MachineType == (int)CSH030Ex.MachineType.Handler)
                {
                    ThreadServer.Abort();
                    ThreadServer = null;
                }
                else
                {
                    //m__G.fManage.PC2Disconnection();
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }


        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void F_Main_Load(object sender, EventArgs e)
        {
            this.Hide();
            try
            {
                // 출력시작 시점 지정.
                sForm.BringToFront();

                long frequency = 1000;
                //SupremeTimer.QueryPerformanceCounter(ref m_dStartTime);
                SupremeTimer.QueryPerformanceFrequency(ref frequency);
                m__G.TimerFrequency = frequency;

                // Initialize History buffer
                m__G.sSpec.AF_Fullstroke = new double[3];
                m__G.sSpec.AF_Ratedstroke = new double[3];
                m__G.sSpec.AF_FwdStroke = new double[3];
                m__G.sSpec.AF_BwdStroke = new double[3];
                m__G.sSpec.AF_Resolution = new double[3];
                m__G.sSpec.AF_FullSensitivity = new double[3];
                m__G.sSpec.AF_FullLinearity = new double[3];
                m__G.sSpec.AF_FullHysteresis = new double[3];
                m__G.sSpec.AF_FullCrosstalk = new double[3];
                m__G.sSpec.AF_FwdSensitivity = new double[3];
                m__G.sSpec.AF_FwdLinearity = new double[3];
                m__G.sSpec.AF_FwdHysteresis = new double[3];
                m__G.sSpec.AF_FwdCrosstalk = new double[3];
                m__G.sSpec.AF_FwdResolution = new double[3];
                m__G.sSpec.AF_BwdSensitivity = new double[3];
                m__G.sSpec.AF_BwdLinearity = new double[3];
                m__G.sSpec.AF_BwdHysteresis = new double[3];
                m__G.sSpec.AF_BwdCrosstalk = new double[3];
                m__G.sSpec.AF_BwdResolution = new double[3];
                m__G.sSpec.AF_MaxCodeCurrent = new double[3];
                m__G.sSpec.AF_2ndSettlingTime = new double[3];
                m__G.sSpec.AF_2ndOvershoot = new double[3];
                m__G.sSpec.AF_SettlingTime = new double[3];
                m__G.sSpec.AF_Overshoot = new double[3];
                m__G.sSpec.AF_StepStroke = new double[3];
                m__G.sSpec.AF_TotalTilt = new double[3];
                m__G.sSpec.AF_YawTilt = new double[3];
                m__G.sSpec.AF_PitchTilt = new double[3];
                m__G.sSpec.AF_Start_Cut = new double[3];
                m__G.sSpec.AF_End_Cut = new double[3];

                m__G.sSpec.ZM_Fullstroke = new double[3];
                m__G.sSpec.ZM_Ratedstroke = new double[3];
                m__G.sSpec.ZM_FwdStroke = new double[3];
                m__G.sSpec.ZM_BwdStroke = new double[3];
                m__G.sSpec.ZM_Resolution = new double[3];
                m__G.sSpec.ZM_FullSensitivity = new double[3];
                m__G.sSpec.ZM_FullLinearity = new double[3];
                m__G.sSpec.ZM_FullHysteresis = new double[3];
                m__G.sSpec.ZM_FullCrosstalk = new double[3];
                m__G.sSpec.ZM_FwdSensitivity = new double[3];
                m__G.sSpec.ZM_FwdLinearity = new double[3];
                m__G.sSpec.ZM_FwdHysteresis = new double[3];
                m__G.sSpec.ZM_FwdCrosstalk = new double[3];
                m__G.sSpec.ZM_FwdResolution = new double[3];
                m__G.sSpec.ZM_BwdSensitivity = new double[3];
                m__G.sSpec.ZM_BwdLinearity = new double[3];
                m__G.sSpec.ZM_BwdHysteresis = new double[3];
                m__G.sSpec.ZM_BwdCrosstalk = new double[3];
                m__G.sSpec.ZM_BwdResolution = new double[3];
                m__G.sSpec.ZM_MaxCodeCurrent = new double[3];
                m__G.sSpec.ZM_2ndSettlingTime = new double[3];
                m__G.sSpec.ZM_2ndOvershoot = new double[3];
                m__G.sSpec.ZM_SettlingTime = new double[3];
                m__G.sSpec.ZM_Overshoot = new double[3];
                m__G.sSpec.ZM_StepStroke = new double[3];
                m__G.sSpec.ZM_TotalTilt = new double[3];
                m__G.sSpec.ZM_YawTilt = new double[3];
                m__G.sSpec.ZM_PitchTilt = new double[3];
                m__G.sSpec.ZM_Start_Cut = new double[3];
                m__G.sSpec.ZM_End_Cut = new double[3];

                m__G.sSpec.FRAAF_PMFreq = new double[3];
                m__G.sSpec.FRAAF_PhaseMargin = new double[3];
                m__G.sSpec.FRAAF_PMFreq2nd = new double[3];
                m__G.sSpec.FRAAF_PhaseMargin2nd = new double[3];
                m__G.sSpec.FRAAF_Gain10Hz = new double[3];
                m__G.sSpec.FRAZM_PMFreq = new double[3];
                m__G.sSpec.FRAZM_PhaseMargin = new double[3];
                m__G.sSpec.FRAZM_PMFreq2nd = new double[3];
                m__G.sSpec.FRAZM_PhaseMargin2nd = new double[3];
                m__G.sSpec.FRAZM_Gain10Hz = new double[3];

                //// 각 Form이 생성되기전 필요한 하드웨어 초기 셋팅하기
                string strCPU = "";// System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    strCPU += " " + obj["Name"];
                }
                //if (strCPU.Contains("10400"))
                //    m__G.mMaxThread = 8;
                //else if (strCPU.Contains("12700"))
                //    m__G.mMaxThread = 12;

                Splasher.Status = "CPU : " + strCPU;// + " max Thread = " + m__G.mMaxThread.ToString();
                Splasher.Status = "Load CameraID.txt";
                //bool IsSwap = true;// CheckCameraSwap();
                Splasher.Status = "Initializing Vision";
                if ( !m__G.Initial_Vision(mImageGrabberType))
                    Splasher.Status = m__G.oCam[0].mMatroxMsg;

                // Form 배열에 담기 For Loop 돌리기 위해
                m_form = new Form[(int)Page.NUM_OF_PAGE];

                m_form[(int)Page.VISION] = m__G.fVision;
                m_form[(int)Page.MANAGE] = m__G.fManage;

                m__G.fManage.m__G = m__G;
                m__G.fVision.MyOwner = this;
                m__G.fManage.MyOwner = this;
                m__G.fGraph.MyOwner = this;

                //m_form[(int)Page.STATISTICS] = m__G.fStat;
                //m__G.fStat.m__G = m__G;
                //m__G.fStat.MyOwner = this;

                m__G.oCam[0].uTimer = m__G.fGraph.microTimer_AO[0];

                m__G.fTestItemOnOff.m__G = m__G;
                m__G.fTestItemOnOff.MyOwner = this;

                //m__G.fSetupReg.m__G = m__G;
                //m__G.fSetupReg.MyOwner = this;

                // Form 모양 정의하고 Display할 Sub Panel에 담기
                //this.Size = new System.Drawing.Size(1300, 1000);
                //P_Sub.Size = new System.Drawing.Size(1300, 1000);

                for (int i = 0; i < (int)Page.NUM_OF_PAGE - 1; i++)
                {
                    m_form[i].TopLevel = false;
                    m_form[i].BackColor = System.Drawing.SystemColors.ControlDarkDark; //  모든 창의 Backcolor 결정
                    m_form[i].FormBorderStyle = FormBorderStyle.None;
                    m_form[i].Size = P_Sub.Size;
                    P_Sub.Controls.Add(m_form[i]);
                }
                // Form Load 순서대로 실행하기 


                //m__G.fVision.Hide();

                //m__G.fTestItemOnOff.Show();
                //m__G.fTestItemOnOff.Hide();

                //m__G.fSetupReg.Show();
                //m__G.fSetupReg.Hide();

                //            m__G.fManage.Hide();

                string prevRecipe = ReadPrevRecipe();                      /////////////////////////////////////////////////////////////////
                string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";
                string sPathFileName = sFilePath + prevRecipe;
                Splasher.Status = "Load Previous Recipe : " + sPathFileName;
                if (File.Exists(sPathFileName) == false)
                {
                    prevRecipe = "Default.rcp";
                    Splasher.Status = "Recipe File Replacement : " + prevRecipe;
                }
                setJobFile(prevRecipe);                                     /////////////////////////////////////////////////////////////////

                // 이 다음에 Calibration File 을 열어야 한다.
                //  For HIPER-T 3K dual camera system : Calibration.txt
                //  For HIPER-T 4K single camera system : CalSingle.txt
                //  For HIPER-T D4K dual camera system : CalDual.txt
                //sFilePath = m__G.m_RootDirectory + "\\RunData\\CalSingle.txt";   
                //m__G.fCalibrate.ReadCalParameter(sFilePath);

                string prevSpecFileName = ReadPrevSpec();                       /////////////////////////////////////////////////////////////////
                //            sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Spec\\";
                sFilePath = m__G.m_RootDirectory + "\\Spec\\";

                sPathFileName = sFilePath + prevSpecFileName;
                Splasher.Status = "Load Previous Spec. : " + sPathFileName;
                if (File.Exists(sPathFileName) == false)
                {
                    prevSpecFileName = "Default.spc";
                    Splasher.Status = "Spec File Replacement : " + prevSpecFileName;
                }

                Splasher.Status = "Load Operator Window";
                m__G.fManage.Show();
                //m__G.fManage.ReadDailyStatisitics();                /////////////////////////////////////////////////////////////////

                //m__G.fSetupReg.LoadRegisterSetting();
                Splasher.Status = "Load TestItemSetting.txt ";
                m__G.fTestItemOnOff.LoadTestItemSetting();                /////////////////////////////////////////////////////////////////
                Splasher.Status = "Initialize Test Item Table ";
                InitDataGridView();
                Splasher.Status = "Initialize Spec Item Table ";
                opInitDataGridView2();
                Splasher.Status = "Initialize Recipe ";
                bLoadRecipe(prevRecipe);
                Splasher.Status = "Initialize Spec ";
                setSpecFile(prevSpecFileName);
                ReadSpecFile(prevSpecFileName);                /////////////////////////////////////////////////////////////////

                Splasher.Status = "Load MakerBarcodeRev.txt, TesterNumber.txt ";
                ReadProductMaker();
                //ReadLastSampleNumber();

                ReadModel();


                sForm.BringToFront();


                Splasher.Status = "Initializing I2C & GPIO ";
                if (!m__G.fGraph.mDriverIC.InitI2CnGPIO(m__G.sRecipe.iI2Cclock, 2, m__G.mChannelCount))
                {
                    mbDlnAvailable = false;
                    Splasher.Status = "Fail to Initialize I2C & GPIO.";
                }else
                    Splasher.Status = "Success.";


                ChkCLAF();

                Splasher.Status = "Load Check Box Status : CBstates.txt";
                ReadPrevCBstates();
                ShowOperatorMode();
                Splasher.Status = "Loading Last Model File List.";
                if (!LoadLastModelFileList())
                    Splasher.Status = "Fail.";
                else
                    Splasher.Status = "Success.";


                m__G.fManage.CalcShowYield();

                m__G.fGraph.StartMonitor();

                try
                {
                    m__G.ResetwrSystemLog(m__G.sHistIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                m_bFinishLoadMain = true;

                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                string lstr = "";
                foreach (ManagementObject item in moc)
                {
                    lstr = Convert.ToString(Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 1048576, 0)) + " MB";
                    this.Text += "    \t\tSystem Memory : " + lstr;
                }
                #region 자세차 모터& 통신 활성화 or 2PC 통신 활성화 
                if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\MachineType.txt"))
                {
                    StreamWriter sw = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\MachineType.txt");
                    sw.WriteLine("Normal");
                    sw.Close();
                }
                StreamReader sr = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\MachineType.txt");
                string temp = sr.ReadLine();
                //  m__G.fManage.lblMachineType.Text = temp;
                sr.Close();
                // bool conStatus = false;
                #endregion
                if (MachineType == (int)CSH030Ex.MachineType.Slave)
                {
                    btnOpen.Enabled = false;
                    //   btnSave.Enabled = false;
                    btnSaveAs.Enabled = false;
                    btn_OpenSpecFile.Enabled = false;
                    btn_SaveSpecFile.Enabled = false;
                    btn_SaveAsSpecFile.Enabled = false;
                    // cb_Edit.Enabled = false;
                    cb_EditSpec.Enabled = false;
                    //btnUserPIDUpdate.Enabled = false;
                    btnApplyTesterNo.Enabled = false;
                    lbxBarcodeMaker.Enabled = false;
                    tbBarcodeRevNo.Enabled = false;
                    tbTesterNumber.Enabled = false;
                    tbProductLine.Enabled = false;
                    lbxPrismSupplier.Enabled = false;
                    cbPassword.Enabled = false;
                    cb_ScreenCapture.Enabled = false;
                    cbSaveRawData.Enabled = false;
                    cbYTiltReverse.Enabled = false;
                    cbCalibrationModel.Enabled = false;
                    cbHideAllGraph.Enabled = false;
                    listBox1.Enabled = false;
                    btn_ToLeft.Enabled = false;
                    btn_ToRight.Enabled = false;
                    btn_Up.Enabled = false;
                    btn_Down.Enabled = false;
                    listBox2.Enabled = false;
                    cbXTiltReverse.Enabled = false;
                    cbReverseTX.Enabled = false;
                    lbxProductModel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Splasher.Status = "Unknown Error occurs.";
                //Thread.Sleep(400);
            }

            string strMOTID = "";
            int numMOT = m__G.fGraph.mDriverIC.MOTID / 32;
            int numWndMask = m__G.fGraph.mDriverIC.MOTID % 32;
            switch(numMOT)
            {
                case 0:
                    strMOTID = "A";
                    break;
                case 1:
                    strMOTID = "B";
                    break;
                case 2:
                    strMOTID = "C";
                    break;
                case 3:
                    strMOTID = "D";
                    break;
                default:
                    strMOTID = "-";
                    break;
            }
            labelMOTID.Text = strMOTID + numWndMask.ToString();
            m__G.mMOTID = labelMOTID.Text;

            mInitialMsg = Splasher.Status;
            Thread.Sleep(100);

            tbMonitoringTestSetCount.Text = "129";

            Splasher.Close();
            sForm.Close();
            this.Show();
            lblDefaultModel.Text = m__G.mFAL.GetLastFMI();

            //m__G.fVision.StartLive();
            //Thread.Sleep(100);
            //m__G.fVision.GrabHalt();

            Splasher.Close();
            sForm.Close();
        }
        public bool ReadPrevCBstates()
        {
            m_bCBreading = true;

            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "CBstates.txt";
            if (!File.Exists(strFileName))
            {
                m_bCBreading = false;
                return false;
            }
            StreamReader sr = new StreamReader(strFileName);
            string allData = sr.ReadToEnd();
            sr.Close();
            m_bCBreading = false;
            string[] rows = allData.Split("\r".ToCharArray());

            //   다음은 항상 순서를 지켜야 한다.
            int i = 0;

            string IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
                cb_ScreenCapture.Checked = true;
            else
                cb_ScreenCapture.Checked = false;

            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
                cbSaveRawData.Checked = true;
            else
                cbSaveRawData.Checked = false;

            if (i == rows.Length) return true;
            IsChecked = rows[i++];

            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbPassword.Checked = true;
                m__G.m_bPasswordOn = true;
            }
            else
            {
                cbPassword.Checked = false;
                m__G.m_bPasswordOn = false;
            }

            if (i == rows.Length) return true;
            IsChecked = rows[i++];

            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbYTiltReverse.Checked = true;
                m__G.m_bYTiltReverse = true;
            }
            else
            {
                cbYTiltReverse.Checked = false;
                m__G.m_bYTiltReverse = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];

            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbCalibrationModel.Checked = true;
                m__G.m_bCalibrationModel = true;
            }
            else
            {
                cbCalibrationModel.Checked = false;
                m__G.m_bCalibrationModel = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbHideAllGraph.Checked = true;
                m__G.m_bHideAllGraph = true;
            }
            else
            {
                cbHideAllGraph.Checked = false;
                m__G.m_bHideAllGraph = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbXTiltReverse.Checked = true;
                m__G.m_bXTiltReverse = true;
            }
            else
            {
                cbXTiltReverse.Checked = false;
                m__G.m_bXTiltReverse = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbReverseTX.Checked = true;
                m__G.m_bTXDirReverse = true;
            }
            else
            {
                cbReverseTX.Checked = false;
                m__G.m_bTXDirReverse = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbReverseTY.Checked = true;
                m__G.m_bTYDirReverse = true;
            }
            else
            {
                cbReverseTY.Checked = false;
                m__G.m_bTYDirReverse = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbReverseX.Checked = true;
                m__G.m_bXDirReverse = true;
            }
            else
            {
                cbReverseX.Checked = false;
                m__G.m_bXDirReverse = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbReverseY.Checked = true;
                m__G.m_bYDirReverse = true;
            }
            else
            {
                cbReverseY.Checked = false;
                m__G.m_bYDirReverse = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbEulerRotation.Checked = true;
                m__G.m_bEulerRotation = true;
            }
            else
            {
                cbEulerRotation.Checked = false;
                m__G.m_bEulerRotation = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbPrismCoordinateSystem.Checked = true;
                m__G.m_bPrismCS = true;
            }
            else
            {
                cbPrismCoordinateSystem.Checked = false;
                m__G.m_bPrismCS = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbAutoLastFrame.Checked = true;
                m__G.m_bAutoLastFrame = true;
            }
            else
            {
                cbAutoLastFrame.Checked = false;
                m__G.m_bAutoLastFrame = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbSaveLostTestSet.Checked = true;
                m__G.m_bSaveLostTestSet = true;
            }
            else
            {
                cbSaveLostTestSet.Checked = false;
                m__G.m_bSaveLostTestSet = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbNoHost.Checked = true;
                m__G.m_bNoHostPC = true;
            }
            else
            {
                cbNoHost.Checked = false;
                m__G.m_bNoHostPC = false;
            }
            if (i == rows.Length) return true;
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbDebugMode.Checked = true;
                m__G.m_bDebugMode = true;
            }
            else
            {
                cbDebugMode.Checked = false;
                m__G.m_bDebugMode = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbTestStage.Checked = true;
                m__G.m_bTestStage = true;
            }
            else
            {
                cbTestStage.Checked = false;
                m__G.m_bTestStage = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbSaveNGImage.Checked = true;
                m__G.m_bSaveNgImage = true;
            }
            else
            {
                cbSaveNGImage.Checked = false;
                m__G.m_bSaveNgImage = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                chOISOption.Checked = true;
                m__G.m_bOISOption = true;
            }
            else
            {
                chOISOption.Checked = false;
                m__G.m_bOISOption = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                chSaveUserImage.Checked = true;
                m__G.m_bSaveFImage = true;
            }
            else
            {
                chSaveUserImage.Checked = false;
                
                m__G.m_bSaveFImage = false;
            }
            IsChecked = rows[i++];
            if (IsChecked.Contains("t") || IsChecked.Contains("T"))
            {
                cbPseudoOMM.Checked = true;
                m__G.m_bPseudoOMM = true;
            }
            else
            {
                cbPseudoOMM.Checked = false;
                m__G.m_bPseudoOMM = false;
            }
            return true;
        }
        public bool WriteCurrentCBstates()
        {
            if (m_bCBreading)
            {
                return false;
            }

            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "CBstates.txt";

            StreamWriter sr = new StreamWriter(strFileName);

            //   다음은 항상 순서를 지켜야 한다.
            sr.WriteLine(cb_ScreenCapture.Checked.ToString());
            sr.WriteLine(cbSaveRawData.Checked.ToString());
            sr.WriteLine(cbPassword.Checked.ToString());
            sr.WriteLine(cbYTiltReverse.Checked.ToString());
            sr.WriteLine(cbCalibrationModel.Checked.ToString());
            sr.WriteLine(cbHideAllGraph.Checked.ToString());
            sr.WriteLine(cbXTiltReverse.Checked.ToString());
            sr.WriteLine(cbReverseTX.Checked.ToString());
            sr.WriteLine(cbReverseTY.Checked.ToString());
            sr.WriteLine(cbReverseX.Checked.ToString());
            sr.WriteLine(cbReverseY.Checked.ToString());
            sr.WriteLine(cbEulerRotation.Checked.ToString());
            sr.WriteLine(cbPrismCoordinateSystem.Checked.ToString());
            sr.WriteLine(cbAutoLastFrame.Checked.ToString());//cbSaveLostTestSet
            sr.WriteLine(cbSaveLostTestSet.Checked.ToString());
            sr.WriteLine(cbNoHost.Checked.ToString());
            sr.WriteLine(cbDebugMode.Checked.ToString());
            sr.WriteLine(cbTestStage.Checked.ToString());
            sr.WriteLine(cbSaveNGImage.Checked.ToString());
            sr.WriteLine(chOISOption.Checked.ToString());
            sr.WriteLine(chSaveUserImage.Checked.ToString());
            sr.WriteLine(cbPseudoOMM.Checked.ToString());
            sr.Close();
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                SendParameterState = SendParameterState + "," + cb_ScreenCapture.Checked + "," + cbSaveRawData.Checked + "," + cbPassword.Checked + "," +
                    cbYTiltReverse.Checked + "," +
                    cbCalibrationModel.Checked + "," + cbHideAllGraph.Checked + "," +  
                    cbXTiltReverse.Checked + "," + cbReverseTX.Checked;
            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void SubForm_Show(int formNo)
        {
            //if (m_CurrentForm == formNo) return;
            //m_CurrentForm = formNo;
            //MessageBox.Show(formNo.ToString());
            P_Sub.Location = new Point(0, 0);
            P_Sub.Size = new Size(1920, 1040);

            // Sub Form 보여주기
            for (int i = 0; i < (int)Page.NUM_OF_PAGE; i++)
                if (m_form[i] != null) m_form[i].Hide();

            if (formNo!= (int)Page.MANAGE)
            {
                label2.Hide();
                labelMOTID.Hide();
            }else
            {
                label2.Show();
                labelMOTID.Show();
            }

            m_form[formNo].Show();
            m_form[formNo].BringToFront();


        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnOpen_Click(object sender, EventArgs e)
        {
            //            string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Recipe\\";
            string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "rcp";
            openFile.InitialDirectory = sFilePath;

            openFile.Filter = "Recipe(*.rcp)|*.rcp";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (MachineType == (int)CSH030Ex.MachineType.Master)
                {
                    m__G.fManage.PC2SendData("RLD", openFile.SafeFileName, openFile.SafeFileName.Length, 2);
                    UIControltmr.Enabled = true;
                }
                string sFileName = openFile.SafeFileName;
                bLoadRecipe(sFileName);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnSave_Click(object sender, EventArgs e)
        {

            // laserjet
            if (m__G.sRecipe.sRecipeName == "Default.rcp")
            {
                if (MessageBox.Show("Defult 파일입니다. 저장하시겠습니까?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bSaveCurrentRecipe(m__G.sRecipe.sRecipeName);
                }
            }
            else
            {
                bSaveCurrentRecipe(m__G.sRecipe.sRecipeName);
            }
            WritePrevRecipe(m__G.sRecipe.sRecipeName);
            DataUpdateToGUI();
            //if (MachineType == (int)TZAF.MachineType.Master)
            //    btnSave.Enabled = false;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            //            string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Recipe\\";
            string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "rcp";
            saveFile.InitialDirectory = sFilePath;

            saveFile.Title = "Save as rcp file";
            saveFile.Filter = "Recipe(*.rcp)|*.rcp";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string sFileName = saveFile.FileName;
                string result_fileName = sFileName.Substring(sFileName.LastIndexOf("\\") + 1);

                setJobFile(result_fileName);
                bSaveCurrentRecipe(result_fileName);
                WritePrevRecipe(result_fileName);

                //m__G.fGraph.MakeZMOnlyWaveform();
                //m__G.fGraph.MakeAFOnlyWaveform();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void WritePrevRecipe(string prevRecipe)
        {
            string FilePath = m__G.m_RootDirectory + "\\DoNotTouch\\" + "PreviousRecipe.txt";
            StreamWriter writer = new StreamWriter(FilePath);

            writer.WriteLine(prevRecipe);

            writer.Close();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private string ReadPrevRecipe()
        {
            string FilePath = m__G.m_RootDirectory + "\\DoNotTouch\\" + "PreviousRecipe.txt";
            if (!File.Exists(FilePath)) return "";
            StreamReader sr = new StreamReader(FilePath, System.Text.Encoding.Default);

            string file = sr.ReadLine();

            sr.Close();

            return file;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_OpenSpecFile_Click(object sender, EventArgs e)
        {
            //            string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Spec\\";
            string sFilePath = m__G.m_RootDirectory + "\\Spec\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "spc";
            openFile.InitialDirectory = sFilePath;

            openFile.Filter = "Spec(*.spc)|*.spc";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (MachineType == (int)CSH030Ex.MachineType.Master)
                {
                    m__G.fManage.PC2SendData("SLD", openFile.SafeFileName, openFile.SafeFileName.Length, 2);
                    UIControltmr.Enabled = true;
                }
                string sFileName = openFile.SafeFileName;
                m__G.strSpecFile = sFileName;
                ReadSpecFile(m__G.strSpecFile);
                WritePrevSpecFile(m__G.strSpecFile);
                setSpecFile(m__G.strSpecFile);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_SaveSpecFile_Click(object sender, EventArgs e)
        {
            WriteSpec(m__G.strSpecFile);
            WritePrevSpecFile(m__G.strSpecFile);
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btn_SaveSpecFile.Enabled = false;

        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_SaveAsSpecFile_Click(object sender, EventArgs e)
        {
            //string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Spec\\";
            string sFilePath = m__G.m_RootDirectory + "\\Spec\\";
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "spc";
            saveFile.InitialDirectory = sFilePath;

            saveFile.Title = "Save as spc file";
            saveFile.Filter = "Spec(*.spc)|*.spc";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string sFileName = saveFile.FileName;
                string result_fileName = sFileName.Substring(sFileName.LastIndexOf("\\") + 1);
                WriteSpec(result_fileName);
                WritePrevSpecFile(result_fileName);
                setSpecFile(result_fileName);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void WritePrevSpecFile(string prevRecipe)
        {
            string FilePath = m__G.m_RootDirectory + "\\DoNotTouch\\" + "PreviousSpec.txt";
            StreamWriter writer = new StreamWriter(FilePath);

            writer.WriteLine(prevRecipe);

            writer.Close();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private string ReadPrevSpec()
        {
            string FilePath = m__G.m_RootDirectory + "\\DoNotTouch\\" + "PreviousSpec.txt";

            if (!File.Exists(FilePath))
            {
                if (!File.Exists("PreviousResult.txt"))
                    return "";
                else
                    File.Copy("PreviousResult.txt", FilePath);
            }
            StreamReader sr = new StreamReader(FilePath, System.Text.Encoding.Default);

            string file = sr.ReadLine(); ;
            m__G.strSpecFile = file;

            sr.Close();

            return file;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        ////public string WriteResult(int modelIndex=0, bool bSavePositionData = true)
        ////{
        ////    DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
        ////    string filename = "res" + modelIndex.ToString() + "_" + dtNow.ToString("yyMMddHHmmss") + ".csv";
        ////    string sFilePath = "";
        ////    string sLotName = m__G.fManage.GetLotName();
        ////    m__G.mNowLotName = sLotName;

        ////    string sLotDir = m__G.fManage.CheckResultFolder();
        ////    if (sLotName != "")
        ////        sLotDir = sLotDir + sLotName;

        ////    if (!Directory.Exists(sLotDir))
        ////        Directory.CreateDirectory(sLotDir);

        ////    sFilePath = sLotDir + "\\" + filename;

        ////    StreamWriter wr = new StreamWriter(sFilePath);

        ////    int fCount = m__G.fVision.GetTriggerGrabbedFrame();
        ////    fCount = Math.Min(fCount, m__G.sRecipe.iTriggeredGrabImageCount);

        ////    DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();

        ////    string lstr = "";
        ////    //lstr = "Test Date Time," + startDateTime.ToString("yy//MM//dd//HH:mm:ss") + "\r\nFrame Grabbed," + fCount.ToString() + "\r\nFPS," + m__G.fVision.GetTriggerGrabbedFPS().ToString() + "\r\n";
        ////    wr.WriteLine("Test Date Time," + startDateTime.ToString("yy//MM//dd//HH:mm:ss") + "\r\nFrame Grabbed," + fCount.ToString() + "\r\nFPS," + m__G.fVision.GetTriggerGrabbedFPS().ToString());
        ////    //lstr += "LED Left," + m__G.fVision.mLEDcurrent[0].ToString("F2") + ",LED Right," + m__G.fVision.mLEDcurrent[1].ToString("F2") + "\r\n";
        ////    wr.WriteLine("LED Left," + m__G.fVision.mLEDcurrent[0].ToString("F2") + ",LED Right," + m__G.fVision.mLEDcurrent[1].ToString("F2"));
        ////    //lstr += "Time To Test," + m__G.fVision.GetHowLongItTook().ToString("F3") + "\r\n\r\n";
        ////    wr.WriteLine("Time To Test," + m__G.fVision.GetHowLongItTook().ToString("F3") + "\r\n");
        ////    //lstr += "Frame No.,X(um),Y(um),Z(um),TX(min),TY(min),TZ(min),,N1 X,N1 Y,N2 X,N2 Y,W1 X,W1 Y, W2 X, W2 Y,S1 X,S1 Y,S2 X,S2 Y,E1 X,E1 Y,E2 X,E2 Y,tN1 X,tN1 Y,tN2 X,tN2 Y,tS1 X,tS1 Y,tS2 X,tS2 Y\r\n";
        ////    if (bSavePositionData)
        ////        wr.WriteLine("Frame No.,Time(sec),X(um),Y(um),Z(um),TX(min),TY(min),TZ(min),,N1 X,N1 Y,N2 X,N2 Y,W1 X,W1 Y, W2 X, W2 Y,S1 X,S1 Y,S2 X,S2 Y,E1 X,E1 Y,E2 X,E2 Y,tN1 X,tN1 Y,tN2 X,tN2 Y,tS1 X,tS1 Y,tS2 X,tS2 Y");
        ////    else
        ////        wr.WriteLine("Frame No.,X(um),Y(um),Z(um),TX(min),TY(min),TZ(min),,");

        ////    //lstr = "";

        ////    //double scaleTop = 1;
        ////    //double scaleSide = 1;
        ////    //double rotTop = 0;
        ////    //double rotSide = 0;

        ////    //m__G.oCam[0].GetScaleNOpticalR(ref scaleTop, ref scaleSide, ref rotTop, ref rotSide);

        ////    double umscale = 5.5 / Global.LensMag;       //  Pixel to um
        ////    double minscale = 180 / Math.PI * 60;                           //  rad to min
        ////    for ( int i=0; i< fCount; i++)
        ////    {
        ////        lstr = i.ToString() + "," + m__G.oCam[0].mGrabAbsTiming[i].ToString("F4") + "," + (m__G.oCam[0].mC_pX[i] * umscale).ToString("F2") + "," + (m__G.oCam[0].mC_pY[i] * umscale).ToString("F2") + "," + (m__G.oCam[0].mC_pZ[i] * umscale).ToString("F2")
        ////                             + "," + (m__G.oCam[0].mC_pTX[i] * minscale).ToString("F2") + "," + (m__G.oCam[0].mC_pTY[i] * minscale).ToString("F2") + "," + (m__G.oCam[0].mC_pTZ[i] * minscale).ToString("F2") + ",,";
        ////        if ( bSavePositionData)
        ////        {
        ////            for (int j = 0; j < 12; j++)
        ////                lstr += m__G.oCam[0].mAzimuthPts[i][j].X.ToString("F4") + "," + m__G.oCam[0].mAzimuthPts[i][j].Y.ToString("F4") + ",";
        ////            lstr += ",";
        ////            for (int j = 0; j < 12; j++)
        ////                lstr += m__G.oCam[0].mAzimuthPtsUpper[i][j].X.ToString("F4") + "," + m__G.oCam[0].mAzimuthPtsUpper[i][j].Y.ToString("F4") + ",";
        ////            lstr += ",";
        ////            for (int j = 0; j < 12; j++)
        ////                lstr += m__G.oCam[0].mAzimuthPtsLower[i][j].X.ToString("F4") + "," + m__G.oCam[0].mAzimuthPtsLower[i][j].Y.ToString("F4") + ",";
        ////        }
        ////        wr.WriteLine(lstr);
        ////        //lstr += "\r\n";
        ////    }
        ////    //wr.Write(lstr);
        ////    wr.Close();

        ////    return sFilePath;
        ////}

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] X ;                           
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] Y ;                           
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] Z ;                           
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TX;                           
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TY;                           
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult5
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult3
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult1
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult0
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TZ;
        }
        [Serializable]
        public class sSaveResultBin
        {
            public Int64 sTime;             
            public int frameCount;
            public double fps;
            public double ledLeft;
            public double ledRight;
            public double testTime;
            public double[] X;
            public double[] Y;
            public double[] Z;
            public double[] TX;
            public double[] TY;
            public double[] TZ;
            public double[] CoX;
            public double[] CoY;
            public double[] CoZ;

            public double[] pOmmX;
            public double[] pOmmY;
            public double[] pOmmZ;
            public double[] pOmmTX;
            public double[] pOmmTY;
            public double[] pOmmTZ;
            public double[] pOmmrX;
            public double[] pOmmrY;
            public double[] pOmmrZ;
            public double[] pOmmrTX;
            public double[] pOmmrTY;
            public double[] pOmmrTZ;
        }
        public class sSaveResultPos
        {
            public Int64 sTime;
            public int frameCount;
            public double fps;
            public double ledLeft;
            public double ledRight;
            public double testTime;
            public double[] X1 = new double [5000];
            public double[] Y1 = new double [5000];
            public double[] X2 = new double [5000];
            public double[] Y2 = new double [5000];
            public double[] X3 = new double [5000];
            public double[] Y3 = new double [5000];
            public double[] X4 = new double [5000];
            public double[] Y4 = new double [5000];
            public double[] X5 = new double [5000];
            public double[] Y5 = new double [5000];
        }

        public string WriteResultPos(int modelIndex = 0)
        {
            string sLotName = m__G.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            string sLotDir = m__G.fManage.CheckResultFolder();
            if (sLotName != "")
                sLotDir = sLotDir + sLotName;

            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);

            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            int framCnt = m__G.fVision.GetTriggerGrabbedFrame();

            if (framCnt > m__G.oCam[0].mTargetTriggerCount)
                framCnt = m__G.oCam[0].mTargetTriggerCount;

            string filename = framCnt.ToString() + "_" + modelIndex.ToString() + "_" + dtNow.ToString("yyMMddHHmmss.fff") + "Pos.dat";
            string sFilePath = sLotDir + filename;

            sSaveResultPos sResult = new sSaveResultPos();

            DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
            DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
            long unixTime = datetimeOffset.ToUnixTimeSeconds();
            //sResult.sTime = startDateTime.ToBinary();
            sResult.sTime = unixTime;
            sResult.frameCount = framCnt;
            sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
            sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
            sResult.ledRight = m__G.fVision.mLEDcurrent[1];
            sResult.testTime = m__G.fVision.GetHowLongItTook();

            for (int i = 0; i < framCnt; i++)
            {
                sResult.X1[i] = m__G.oCam[0].mMarkPosRes[0][i].X;  //  um
                sResult.Y1[i] = m__G.oCam[0].mMarkPosRes[0][i].Y;  //  um
                sResult.X2[i] = m__G.oCam[0].mMarkPosRes[1][i].X;  //  um
                sResult.Y2[i] = m__G.oCam[0].mMarkPosRes[1][i].Y;  //  um
                sResult.X3[i] = m__G.oCam[0].mMarkPosRes[2][i].X;  //  um
                sResult.Y3[i] = m__G.oCam[0].mMarkPosRes[2][i].Y;  //  um
                sResult.X4[i] = m__G.oCam[0].mMarkPosRes[3][i].X;  //  um
                sResult.Y4[i] = m__G.oCam[0].mMarkPosRes[3][i].Y;  //  um
                sResult.X5[i] = m__G.oCam[0].mMarkPosRes[4][i].X;  //  um
                sResult.Y5[i] = m__G.oCam[0].mMarkPosRes[4][i].Y;  //  um
            }

            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(sSaveResultPos));

            System.IO.FileStream file = System.IO.File.Create(sFilePath);
            writer.Serialize(file, sResult);
            file.Close();

            return sFilePath;
        }
        public string WriteResultBin(int modelIndex = 0)
        {
            string sLotName = m__G.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            string sLotDir = m__G.fManage.CheckResultFolder();
            if (sLotName != "")
                sLotDir = sLotDir + sLotName;

            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);

            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            int framCnt = m__G.fVision.GetTriggerGrabbedFrame();

            if (framCnt > m__G.oCam[0].mTargetTriggerCount)
                framCnt = m__G.oCam[0].mTargetTriggerCount;

            string filename = framCnt.ToString() + "_" + modelIndex.ToString() + "_" + dtNow.ToString("yyMMddHHmmss.fff") + ".dat";
            string sFilePath = sLotDir + filename;

            double umscale = 5.5/ Global.LensMag;                           //  rad to min
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double preZ = 0;
            int i = 0;

            if (framCnt >5000)
            {
                sSaveResult sResult = new sSaveResult();

                sResult.X = new double[10000];
                sResult.Y = new double[10000];
                sResult.Z = new double[10000];
                sResult.TX = new double[10000];
                sResult.TY = new double[10000];
                sResult.TZ = new double[10000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
                sResult.ledRight = m__G.fVision.mLEDcurrent[1];
                sResult.testTime = m__G.fVision.GetHowLongItTook();

                //////  임시  230924
                ////double tx0 = m__G.oCam[0].mC_pTX[0];
                ////double ty0 = m__G.oCam[0].mC_pTY[0];
                ////double tz0 = m__G.oCam[0].mC_pTZ[0];

                for (i = 0; i < 5; i++)
                {
                    sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                    sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                    sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                    sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                    sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                    sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                }
                
                for (i = 1; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                    sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                    sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                    sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                    sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                    sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                }

                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                m__G.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, m__G.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(m__G.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }
            else if ( framCnt > 3000)
            {
                sSaveResult5 sResult = new sSaveResult5();

                sResult.X = new double[5000];
                sResult.Y = new double[5000];
                sResult.Z = new double[5000];
                sResult.TX = new double[5000];
                sResult.TY = new double[5000];
                sResult.TZ = new double[5000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
                sResult.ledRight = m__G.fVision.mLEDcurrent[1];
                sResult.testTime = m__G.fVision.GetHowLongItTook();

                for (i = 0; i < 5; i++)
                {
                    sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                    sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                    sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                    sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                    sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                    sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                }

                for (i = 1; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                    sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                    sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                    sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                    sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                    sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                }

                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                m__G.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, m__G.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(m__G.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }
            else
            {
                sSaveResult3 sResult = new sSaveResult3();

                sResult.X = new double[3000];
                sResult.Y = new double[3000];
                sResult.Z = new double[3000];
                sResult.TX = new double[3000];
                sResult.TY = new double[3000];
                sResult.TZ = new double[3000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
                sResult.ledRight = m__G.fVision.mLEDcurrent[1];
                sResult.testTime = m__G.fVision.GetHowLongItTook();

                if (framCnt>1000)
                {
                    for (i = 0; i < 5; i++)
                    {
                        sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                        sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                        sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                                                                              //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                              //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                        sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                        sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                        sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                    }

                    for (i = 1; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }
                else
                {
                    for (i = 0; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }


                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                m__G.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, m__G.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(m__G.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }

            //  Verify Read process
            //Thread.Sleep(100);
            //sSaveResult3 sRes = ReadsSaveResult3(sFilePath);
            //DateTime now = DateTime.FromBinary(sRes.sTime);

            return sFilePath;
        }
        public bool DebugStop = false;
        public byte[] MakeSaveResult(bool isManual = false)
        {
            string sLotName = m__G.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            int framCnt = m__G.oCam[0].mTargetTriggerCount;

            if (framCnt > m__G.oCam[0].mTargetTriggerCount)
                framCnt = m__G.oCam[0].mTargetTriggerCount;

            int structCnt = 44;
            int databufCnt = 9;
            if (m__G.m_bPseudoOMM) databufCnt = 17;
            byte[] dataBuf = new byte[structCnt + framCnt * 8 * databufCnt];

            double umscale = 5.5 / Global.LensMag;                           //  rad to min
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double preZ = 0;
            int i = 0;

            sSaveResultBin sResult = new sSaveResultBin();
            int curCount = 0;
            byte[] data;

            DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
            DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
            long unixTime = datetimeOffset.ToUnixTimeSeconds();
            //sResult.sTime = startDateTime.ToBinary();
            sResult.sTime = unixTime;
            data = BitConverter.GetBytes(sResult.sTime);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.frameCount = framCnt;
            data = BitConverter.GetBytes(sResult.frameCount);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
            data = BitConverter.GetBytes(sResult.fps);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
            data = BitConverter.GetBytes(sResult.ledLeft);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.ledRight = m__G.fVision.mLEDcurrent[1];
            data = BitConverter.GetBytes(sResult.ledRight);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.testTime = m__G.fVision.GetHowLongItTook();
            data = BitConverter.GetBytes(sResult.testTime);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.X = new double[framCnt];
            sResult.Y = new double[framCnt];
            sResult.Z = new double[framCnt];
            sResult.TX = new double[framCnt];
            sResult.TY = new double[framCnt];
            sResult.TZ = new double[framCnt];
            sResult.CoX = new double[framCnt];
            sResult.CoY = new double[framCnt];
            sResult.CoZ = new double[framCnt];

            sResult.pOmmX = new double[framCnt];
            sResult.pOmmY = new double[framCnt];
            sResult.pOmmZ = new double[framCnt];
            //sResult.pOmmTX = new double[framCnt];
            //sResult.pOmmTY = new double[framCnt];
            sResult.pOmmTZ = new double[framCnt];

            sResult.pOmmrX = new double[framCnt];
            sResult.pOmmrY = new double[framCnt];
            sResult.pOmmrZ = new double[framCnt];
            sResult.pOmmrTZ = new double[framCnt];


            for (i = 0; i < framCnt; i++)
            {
                sResult.CoX[i] = 0;
                sResult.CoY[i] = 0;
                sResult.CoZ[i] = 0;
            }
            if (!isManual)
            {
                if (framCnt > 1000)
                {
                    for (i = 0; i < 5; i++)
                    {
                        sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                        sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                        sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                                                                              //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                              //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                        sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                        sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                        sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min

                        sResult.CoX[0] += m__G.oCam[0].mPrism_pTX[i] * minscale / 5; //  min
                        sResult.CoY[0] += m__G.oCam[0].mPrism_pTY[i] * minscale / 5; //  min
                        sResult.CoZ[0] += m__G.oCam[0].mPrism_pTZ[i] * minscale / 5; //  min

                        if (m__G.m_bPseudoOMM)
                        {
                            sResult.pOmmX[0] += m__G.oCam[0].mPOMM_X[i] * umscale / 5;  //  um
                            sResult.pOmmY[0] += m__G.oCam[0].mPOMM_Y[i] * umscale / 5;  //  um
                            sResult.pOmmZ[0] += m__G.oCam[0].mPOMM_Z[i] * umscale / 5;  //  um
                            //sResult.pOmmTX[0] += m__G.oCam[0].mPOMM_TX[i] * minscale / 5; //  min
                            //sResult.pOmmTY[0] += m__G.oCam[0].mPOMM_TY[i] * minscale / 5; //  min
                            sResult.pOmmTZ[0] += m__G.oCam[0].mPOMM_TZ[i] * minscale / 5; //  min
                            //==
                            sResult.pOmmrX[0] += m__G.oCam[0].mPOMM_rX[i] * umscale / 5; //  min
                            sResult.pOmmrY[0] += m__G.oCam[0].mPOMM_rY[i] * umscale / 5; //  min
                            sResult.pOmmrZ[0] += m__G.oCam[0].mPOMM_rZ[i] * umscale / 5; //  min
                            sResult.pOmmrTZ[0] += m__G.oCam[0].mPOMM_rTZ[i] * minscale / 5; //  min
                        }
                    }

                    for (i = 1; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min

                        sResult.CoX[i] = m__G.oCam[0].mPrism_pTX[i] * minscale; //  min
                        sResult.CoY[i] = m__G.oCam[0].mPrism_pTY[i] * minscale; //  min
                        sResult.CoZ[i] = m__G.oCam[0].mPrism_pTZ[i] * minscale; //  min
                        if (m__G.m_bPseudoOMM)
                        {
                            sResult.pOmmX[i] += m__G.oCam[0].mPOMM_X[i] * umscale;  //  um
                            sResult.pOmmY[i] += m__G.oCam[0].mPOMM_Y[i] * umscale;  //  um
                            sResult.pOmmZ[i] += m__G.oCam[0].mPOMM_Z[i] * umscale;  //  um
                            //sResult.pOmmTX[i] += m__G.oCam[0].mPOMM_TX[i] * minscale ; //  min
                            //sResult.pOmmTY[i] += m__G.oCam[0].mPOMM_TY[i] * minscale ; //  min
                            sResult.pOmmTZ[i] += m__G.oCam[0].mPOMM_TZ[i] * minscale; //  min
                            //==
                            sResult.pOmmrX[i] += m__G.oCam[0].mPOMM_rX[i] * umscale; //  min
                            sResult.pOmmrY[i] += m__G.oCam[0].mPOMM_rY[i] * umscale; //  min
                            sResult.pOmmrZ[i] += m__G.oCam[0].mPOMM_rZ[i] * umscale; //  min
                            sResult.pOmmrTZ[i] += m__G.oCam[0].mPOMM_rTZ[i] * minscale; //  min
                        }
                    }
                }
                else
                {
                    for (i = 0; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min

                        sResult.CoX[i] = m__G.oCam[0].mPrism_pTX[i] * minscale; //  min
                        sResult.CoY[i] = m__G.oCam[0].mPrism_pTY[i] * minscale; //  min
                        sResult.CoZ[i] = m__G.oCam[0].mPrism_pTZ[i] * minscale; //  min
                        if (m__G.m_bPseudoOMM)
                        {
                            sResult.pOmmX[i] += m__G.oCam[0].mPOMM_X[i] * umscale;  //  um
                            sResult.pOmmY[i] += m__G.oCam[0].mPOMM_Y[i] * umscale;  //  um
                            sResult.pOmmZ[i] += m__G.oCam[0].mPOMM_Z[i] * umscale;  //  um
                            //sResult.pOmmTX[i] += m__G.oCam[0].mPOMM_TX[i] * minscale; //  min
                            //sResult.pOmmTY[i] += m__G.oCam[0].mPOMM_TY[i] * minscale; //  min
                            sResult.pOmmTZ[i] += m__G.oCam[0].mPOMM_TZ[i] * minscale; //  min
                            //==
                            sResult.pOmmrX[i] += m__G.oCam[0].mPOMM_rX[i] * umscale; //  min
                            sResult.pOmmrY[i] += m__G.oCam[0].mPOMM_rY[i] * umscale; //  min
                            sResult.pOmmrZ[i] += m__G.oCam[0].mPOMM_rZ[i] * umscale; //  min
                            sResult.pOmmrTZ[i] += m__G.oCam[0].mPOMM_rTZ[i] * minscale; //  min
                        }
                    }
                }
            }
            else
            {
                for (i = 0; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.oCam[0].mC_pX[i];
                    sResult.Y[i] = m__G.oCam[0].mC_pY[i];
                    sResult.Z[i] = m__G.oCam[0].mC_pZ[i];
                    sResult.TX[i] = m__G.oCam[0].mC_pTX[i];
                    sResult.TY[i] = m__G.oCam[0].mC_pTY[i];
                    sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i];

                    sResult.CoX[i] = m__G.oCam[0].mPrism_pTX[i]; //  min
                    sResult.CoY[i] = m__G.oCam[0].mPrism_pTY[i]; //  min
                    sResult.CoZ[i] = m__G.oCam[0].mPrism_pTZ[i]; //  min
                    if (m__G.m_bPseudoOMM)
                    {
                        sResult.pOmmX[i] += m__G.oCam[0].mPOMM_X[i] * umscale;  //  um
                        sResult.pOmmY[i] += m__G.oCam[0].mPOMM_Y[i] * umscale;  //  um
                        sResult.pOmmZ[i] += m__G.oCam[0].mPOMM_Z[i] * umscale;  //  um
                        //sResult.pOmmTX[i] += m__G.oCam[0].mPOMM_TX[i] * minscale; //  min
                        //sResult.pOmmTY[i] += m__G.oCam[0].mPOMM_TY[i] * minscale; //  min
                        sResult.pOmmTZ[i] += m__G.oCam[0].mPOMM_TZ[i] * minscale; //  min
                                                                                  //==
                        sResult.pOmmrX[i] += m__G.oCam[0].mPOMM_rX[i] * umscale; //  min
                        sResult.pOmmrY[i] += m__G.oCam[0].mPOMM_rY[i] * umscale; //  min
                        sResult.pOmmrZ[i] += m__G.oCam[0].mPOMM_rZ[i] * umscale; //  min
                        sResult.pOmmrTZ[i] += m__G.oCam[0].mPOMM_rTZ[i] * minscale; //  min
                    }
                }
            }


            DateTime dt = DateTime.Now;
            string sLotDir = "C:\\CSHTest\\Data\\" + dt.Year + "\\" + dt.Month + "\\" + dt.Day + "\\" + "A_RData\\";
            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);

            sLotDir += string.Format("ActroRawData_{0}_{1}.csv", framCnt, DateTime.Now.ToString("HHmmss"));

            for (i = 0; i < framCnt; i++)
            {
                if (sResult.X[i] == 0) sResult.X[i] = 99999;
                data = BitConverter.GetBytes(sResult.X[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                if (sResult.Y[i] == 0) sResult.Y[i] = 99999;
                data = BitConverter.GetBytes(sResult.Y[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                if (sResult.Z[i] == 0) sResult.Z[i] = 99999;
                data = BitConverter.GetBytes(sResult.Z[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            if (m__G.m_bOISOption)
            {
                //COMP_TX,COMP_TY,COMP_TZ 추후에 RawData 값 추가 현재 0 채워서 전송
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TX[i] == 0) sResult.TX[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TY[i] == 0) sResult.TY[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TZ[i] == 0) sResult.TZ[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
            }
            else
            {
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TX[i] == 0) sResult.TX[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TY[i] == 0) sResult.TY[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    if (sResult.TZ[i] == 0) sResult.TZ[i] = 99999;
                    data = BitConverter.GetBytes(sResult.TZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                //COMP_TX,COMP_TY,COMP_TZ 추후에 RawData 값 추가 현재 0 채워서 전송
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.CoZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
            }
            if (m__G.m_bPseudoOMM)
            {
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(-sResult.pOmmZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmTZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmrX[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmrY[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmrZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
                for (i = 0; i < framCnt; i++)
                {
                    data = BitConverter.GetBytes(sResult.pOmmrTZ[i]);
                    Array.Copy(data, 0, dataBuf, curCount, data.Length);
                    curCount += data.Length;
                }
            }

            ////////////////////////////////////////////////////////////////
            StreamWriter wr = null;
            if (m__G.m_bSaveRawData)
            {
                wr = new StreamWriter(sLotDir);
                if (m__G.m_bOISOption)
                {

                    if (m__G.m_bPseudoOMM)
                    {
                        wr.WriteLine("X,Y,Z,COMP_TX,COMP_TY,COMP_TZ,TX,TY,TZ,OmmX,OmmY,OmmZ,OmmTZ,OmmrX,OmmrY,OmmrZ,OmmrTZ");
                        for (i = 0; i < framCnt; i++)
                        {
                            wr.WriteLine(string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00},{12:0.00},{13:0.00},{14:0.00},{15:0.00},{16:0.00}",
                                sResult.X[i], sResult.Y[i], sResult.Z[i]
                                , sResult.CoX[i], sResult.CoY[i], sResult.CoZ[i], sResult.TX[i], sResult.TY[i], sResult.TZ[i]
                                , sResult.pOmmX[i], sResult.pOmmY[i], sResult.pOmmZ[i], sResult.pOmmTZ[i]
                                , sResult.pOmmrX[i], sResult.pOmmrY[i], sResult.pOmmrZ[i], sResult.pOmmrTZ[i]
                                ));
                        }
                    }
                    else
                    {
                        wr.WriteLine("X,Y,Z,COMP_TX,COMP_TY,COMP_TZ,TX,TY,TZ");
                        for (i = 0; i < framCnt; i++)
                        {
                            wr.WriteLine(string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00}",
                                sResult.X[i], sResult.Y[i], sResult.Z[i]
                                , sResult.CoX[i], sResult.CoY[i], sResult.CoZ[i], sResult.TX[i], sResult.TY[i], sResult.TZ[i]
                                ));
                        }
                    }
                }
                else
                {
                    if (m__G.m_bPseudoOMM)
                    {
                        wr.WriteLine("X,Y,Z,TX,TY,TZ,COMP_TX,COMP_TY,COMP_TZ,OmmX,OmmY,OmmZ,OmmTZ,OmmrX,OmmrY,OmmrZ,OmmrTZ");
                        for (i = 0; i < framCnt; i++)
                        {
                            wr.WriteLine(string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00},{12:0.00},{13:0.00},{14:0.00},{15:0.00},{16:0.00}",
                                sResult.X[i], sResult.Y[i], sResult.Z[i],
                                sResult.TX[i], sResult.TY[i], sResult.TZ[i], sResult.CoX[i], sResult.CoY[i], sResult.CoZ[i]
                                , sResult.pOmmX[i], sResult.pOmmY[i], sResult.pOmmZ[i], sResult.pOmmTZ[i]
                                , sResult.pOmmrX[i], sResult.pOmmrY[i], sResult.pOmmrZ[i], sResult.pOmmrTZ[i]
                                ));
                        }
                    }
                    else
                    {
                        wr.WriteLine("X,Y,Z,TX,TY,TZ,COMP_TX,COMP_TY,COMP_TZ");
                        for (i = 0; i < framCnt; i++)
                        {
                            wr.WriteLine(string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00}",
                                sResult.X[i], sResult.Y[i], sResult.Z[i],
                                sResult.TX[i], sResult.TY[i], sResult.TZ[i], sResult.CoX[i], sResult.CoY[i], sResult.CoZ[i]
                                ));
                        }
                    }
                }

                wr.Close();
            }

            return dataBuf;
        }
        public byte[] MakeSaveResultAnosis(bool isManual = false)
        {
            string sLotName = m__G.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            int framCnt = m__G.fVision.GetTriggerGrabbedFrame();

            if (framCnt > m__G.oCam[0].mTargetTriggerCount)
                framCnt = m__G.oCam[0].mTargetTriggerCount;

            int structCnt = 44;

            byte[] dataBuf = new byte[structCnt + framCnt * 8 * 6];

            double umscale = 5.5 / Global.LensMag;                           //  rad to min
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double preZ = 0;
            int i = 0;

            sSaveResultBin sResult = new sSaveResultBin();
            int curCount = 0;
            byte[] data;

            DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
            DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
            long unixTime = datetimeOffset.ToUnixTimeSeconds();
            //sResult.sTime = startDateTime.ToBinary();
            sResult.sTime = unixTime;
            data = BitConverter.GetBytes(sResult.sTime);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.frameCount = framCnt;
            data = BitConverter.GetBytes(sResult.frameCount);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.fps = m__G.fVision.GetTriggerGrabbedFPS();
            data = BitConverter.GetBytes(sResult.fps);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.ledLeft = m__G.fVision.mLEDcurrent[0];
            data = BitConverter.GetBytes(sResult.ledLeft);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.ledRight = m__G.fVision.mLEDcurrent[1];
            data = BitConverter.GetBytes(sResult.ledRight);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.testTime = m__G.fVision.GetHowLongItTook();
            data = BitConverter.GetBytes(sResult.testTime);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            sResult.X = new double[framCnt];
            sResult.Y = new double[framCnt];
            sResult.Z = new double[framCnt];
            sResult.TX = new double[framCnt];
            sResult.TY = new double[framCnt];
            sResult.TZ = new double[framCnt];

            if (!isManual)
            {
                if (framCnt > 1000)
                {
                    for (i = 0; i < 5; i++)
                    {
                        sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                        sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                        sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                                                                              //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                              //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                        sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                        sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                        sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                    }

                    for (i = 1; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }
                else
                {
                    for (i = 0; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }
            }
            else
            {
                for (i = 0; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.fVision.AnosisCalData[0];
                    sResult.Y[i] = m__G.fVision.AnosisCalData[1];
                    sResult.Z[i] = m__G.fVision.AnosisCalData[2];
                    sResult.TX[i] = m__G.fVision.AnosisCalData[3];
                    sResult.TY[i] = m__G.fVision.AnosisCalData[4];
                    sResult.TZ[i] = m__G.fVision.AnosisCalData[5];
                }
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.X[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.Y[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.Z[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.TX[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.TY[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }
            for (i = 0; i < framCnt; i++)
            {
                data = BitConverter.GetBytes(sResult.TZ[i]);
                Array.Copy(data, 0, dataBuf, curCount, data.Length);
                curCount += data.Length;
            }

            return dataBuf;
        }
        public static sSaveResultPos ReadsSaveResultPos(string FileName)
        {
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(sSaveResultPos));

            System.IO.StreamReader file = new System.IO.StreamReader(FileName);
            sSaveResultPos res = (sSaveResultPos)reader.Deserialize(file);
            file.Close();

            return res;
        }

        public static sSaveResult ReadsSaveResult(string FileName)
        {
            sSaveResult sRes = new sSaveResult();
            int size = Marshal.SizeOf(typeof(sSaveResult));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult)Marshal.PtrToStructure(ptr, typeof(sSaveResult));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }
        public static sSaveResult5 ReadsSaveResult5(string FileName)
        {
            sSaveResult5 sRes = new sSaveResult5();
            int size = Marshal.SizeOf(typeof(sSaveResult5));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult5)Marshal.PtrToStructure(ptr, typeof(sSaveResult5));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }
        public static sSaveResult3 ReadsSaveResult3(string FileName)
        {
            sSaveResult3 sRes = new sSaveResult3();
            int size = Marshal.SizeOf(typeof(sSaveResult3));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult3)Marshal.PtrToStructure(ptr, typeof(sSaveResult3));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }

        public static object ReadsSaveResultBin(string FileName)
        {
            Stream rs = new FileStream(FileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            object sRes = bf.Deserialize(rs); 
            rs.Close();

            return sRes;
        }

        public string ReadResultPos(string sFileName)
        {
            //Check if result file is readable
            string filename = Path.GetFileName(sFileName);
            string[] strframeCount = filename.Split('_');

            int framCnt = int.Parse(strframeCount[0]);
            string lstr = "";
            sSaveResultPos result = ReadsSaveResultPos(sFileName);
            int len = result.frameCount;
            DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result.sTime);
            DateTime TimeTested = ltime.DateTime;
            if ( sFileName.Contains("_0_"))
                lstr = TimeTested.ToLocalTime().ToString() + "\t" + "X1 smt" + "\t" + "Y1 smt" + "\t" + "X2 smt" + "\t" + "Y2 smt" + "\t" + "X3 smt" + "\t" + "Y3 smt" + "\t" + "X4 smt" + "\t" + "Y4 smt" + "\t" + "X5 smt" + "\t" + "Y5 smt" + "\r\n";
            else
                lstr = TimeTested.ToLocalTime().ToString() + "\t" + "X1 std" + "\t" + "Y1 std" + "\t" + "X2 std" + "\t" + "Y2 std" + "\t" + "X3 std" + "\t" + "Y3 std" + "\t" + "X4 std" + "\t" + "Y4 std" + "\t" + "X5 std" + "\t" + "Y5 std" + "\r\n";

            for (int i = 0; i < len; i++)
            {
                lstr += i.ToString() + "\t" + result.X1[i].ToString("F3") + "\t" + result.Y1[i].ToString("F3")
                                     + "\t" + result.X2[i].ToString("F3") + "\t" + result.Y2[i].ToString("F3")
                                     + "\t" + result.X3[i].ToString("F3") + "\t" + result.Y3[i].ToString("F3")
                                     + "\t" + result.X4[i].ToString("F3") + "\t" + result.Y4[i].ToString("F3")
                                     + "\t" + result.X5[i].ToString("F3") + "\t" + result.Y5[i].ToString("F3") + "\r\n";


            }
            return lstr;
        }
        public string ReadResultBin(string sFileName)
        {
            //Check if result file is readable
            string filename = Path.GetFileName(sFileName);
            string[] strframeCount = filename.Split('_');

            int framCnt = int.Parse(strframeCount[0]);
            string lstr = "";
            string strHead = "";
            if (sFileName.Contains("_0_"))
                strHead = "\tX SMT\tY SMT\tZ SMT\tTX SMT\tTY SMT\tTZ SMT";
            else
                strHead = "\tX std\tY std\tZ std\tTX std\tTY std\tTZ std";

            if (framCnt > 5000)
            {
                sSaveResult result = ReadsSaveResult(sFileName);
                int len = result.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for ( int i=0; i<len;i++)
                {
                    lstr += i.ToString() + "\t" + result.X[i].ToString("F2") + "\t" + result.Y[i].ToString("F2") + "\t" + result.Z[i].ToString("F2") + "\t" + result.TX[i].ToString("F2") + "\t" + result.TY[i].ToString("F2") + "\t" + result.TZ[i].ToString("F2") + "\r\n";
                }
            }
            else if (framCnt > 3000)
            {
                sSaveResult5 result5 = ReadsSaveResult5(sFileName);
                int len = result5.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result5.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for (int i = 0; i < len; i++)
                {
                    lstr += i.ToString() + "\t" + result5.X[i].ToString("F2") + "\t" + result5.Y[i].ToString("F2") + "\t" + result5.Z[i].ToString("F2") + "\t" + result5.TX[i].ToString("F2") + "\t" + result5.TY[i].ToString("F2") + "\t" + result5.TZ[i].ToString("F2") + "\r\n";
                }
            }
            else
            {
                sSaveResult3 result3 = F_Main.ReadsSaveResult3(sFileName);
                int len = result3.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result3.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for (int i = 0; i < len; i++)
                {
                    lstr += i.ToString() + "\t" + result3.X[i].ToString("F2") + "\t" + result3.Y[i].ToString("F2") + "\t" + result3.Z[i].ToString("F2") + "\t" + result3.TX[i].ToString("F2") + "\t" + result3.TY[i].ToString("F2") + "\t" + result3.TZ[i].ToString("F2") + "\r\n";
                }
            }
            return lstr;
        }
        public void AddHeadLineToResult(string sFilePath)
        {
            mbSpecChanged = false;
            StreamWriter writer;
            writer = File.AppendText(sFilePath);
            string[] tmpstr = new string[2] { "A,", "B" };
            string[] lstrItem = new string[63] {
                                                    "Time,",
                                                    "Index,",
                                                    "BarCode,",
                                                    "Channel,",
                                                    "PassFail,",
                                                    "1st Fail Item,",
                                                    "AFZM_Linearity_Comp,",  //  6
                                                    "AF1_Amp_Gain,",
                                                    "AF2_Amp_Gain,",
                                                    "AF1_Offset,",
                                                    "AF2_Offset,",
                                                    "AF1_Bias,",
                                                    "AF2_Bias,",
                                                    "AF1_MIN,",
                                                    "AF1_MAX,",
                                                    "AF2_MIN,",
                                                    "AF2_MAX,",
                                                    "AF1_MID,",
                                                    "AF2_MID,",
                                                    "AF1_RANGE,",
                                                    "AF2_RANGE,",
                                                    "AF_ATT3_MIN,",
                                                    "AF_ATT3_MAX,",
                                                    "AF_ATT3_range,",
                                                    "AF_ATT2_MIN,",
                                                    "AF_ATT2_MAX,",
                                                    "AF_INVDR,",
                                                    "AF_EPA_MIN,",
                                                    "AF_EPA_MAX,",
                                                    "AF Drv min,",
                                                    "AF Drv max,",
                                                    "AF measure min,",
                                                    "AF measure max,",
                                                    "AF Step offset,",
                                                    "AF Step,",
                                                    "ZOOM1_Amp_Gain,",  //  35
                                                    "ZOOM2_Amp_Gain,",
                                                    "ZOOM1_Offset,",
                                                    "ZOOM2_Offset,",
                                                    "ZOOM1_Bias,",
                                                    "ZOOM2_Bias,",
                                                    "ZOOM1_MIN,",
                                                    "ZOOM1_MAX,",
                                                    "ZOOM2_MIN,",
                                                    "ZOOM2_MAX,",
                                                    "ZOOM1_MID,",
                                                    "ZOOM2_MID,",
                                                    "ZOOM1_RANGE,",
                                                    "ZOOM2_RANGE,",
                                                    "ZOOM_ATT3_MIN,",
                                                    "ZOOM_ATT3_MAX,",
                                                    "ZOOM_ATT3_range,",
                                                    "ZOOM_ATT2_MIN,",
                                                    "ZOOM_ATT2_MAX,",
                                                    "ZOOM_INVDR,",
                                                    "ZOOM_EPA_MIN,",
                                                    "ZOOM_EPA_MAX,",
                                                    "ZOOM Drv min,",
                                                    "ZOOM Drv max,",
                                                    "ZOOM measure min,",
                                                    "ZOOM measure max,",
                                                    "ZOOM Step offset,",
                                                    "ZOOM Step,",
                                                };

            string lHeader = "";
            int i = 0;


            for (i = 0; i < 35; i++)
                lHeader += lstrItem[i];
            writer.Write(lHeader);

            for (i = 0; i < (int)Global.SpecItem.ZM_Fullstroke; i++)
            {
                writer.Write("AF " + m__G.mTestItem[i, 1] + ",");
            }


            lHeader = "";
            for (i = 35; i < 63; i++)
                lHeader += lstrItem[i];
            writer.Write(lHeader);


            for (i = (int)Global.SpecItem.ZM_Fullstroke; i < (int)Global.SpecItem.FRAAF_PMFreq; i++)
            {
                writer.Write("Zoom " + m__G.mTestItem[i, 1] + ",");
            }
            for (; i < (int)Global.SpecItem.FRAZM_PMFreq; i++)
            {
                writer.Write("AF " + m__G.mTestItem[i, 1] + ",");
            }
            for (; i < m__G.sNUM_TESTITEM; i++)
            {
                writer.Write("Zoom " + m__G.mTestItem[i, 1] + ",");
            }
            writer.Write("F/W W Time" + "," + "Hall Cal Time" + "," + "Hall NVM Read Time" + "," + "PID Update Time" + "," + "AF Sweep Time" + "," + "AF Step Time" + "," + "AF FRA Time" + "," + "Zoom Sweep Time" + "," + "Zoom Step Time" + "," + "Zoom FRA Time" + "," + "XY PM Time" + "," + "Total Test Time" + "," + " FW ver" + "," + " IC Type" + "," + "SW ver" + "," + "Block Pos" + "," + "Socket Pos" + ",");
            writer.Write("\r\n");

            double lvalue = 0;

            //////////////////////////////////////////////////////////////////////////////////
            //  Write Unit
            writer.Write("unit" + ",");
            for (i = 0; i < 34; i++)   //  1 + 12 + 2
                writer.Write(" " + ",");

            for (i = 0; i < (int)Global.SpecItem.ZM_Fullstroke; i++)
            {
                writer.Write("(" + m__G.mTestItem[i, 9] + ")" + ",");   //  Write Unit
            }
            for (i = 0; i < 28; i++)     //  9 + 2
                writer.Write(" " + ",");

            for (i = (int)Global.SpecItem.ZM_Fullstroke; i < m__G.sNUM_TESTITEM; i++)
            {
                writer.Write("(" + m__G.mTestItem[i, 9] + ")" + ",");   //  Write Unit
            }
            for (i = 0; i < 12; i++)
                writer.Write("(sec)" + ",");   //  Write Unit

            writer.Write("\r\n");
            //////////////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////////////
            //  Write Spec Min
            writer.Write("Spec Min" + ",");
            for (i = 0; i < 34; i++)    //   12 + 2
                writer.Write(" " + ",");

            for (i = 0; i < (int)Global.SpecItem.ZM_Fullstroke; i++)
            {
                lvalue = Convert.ToDouble(m__G.mTestItem[i, 2]);
                writer.Write(lvalue.ToString("F2") + ",");   //  
            }
            for (i = 0; i < 28; i++)     //  9 + 2
                writer.Write(" " + ",");

            for (i = (int)Global.SpecItem.ZM_Fullstroke; i < m__G.sNUM_TESTITEM; i++)
            {
                lvalue = Convert.ToDouble(m__G.mTestItem[i, 2]);
                writer.Write(lvalue.ToString("F2") + ",");   //  
            }
            for (i = 0; i < 12; i++)
                writer.Write(" " + ",");   //  Write Unit

            writer.Write("\r\n");
            //////////////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////////////
            //  Write Spec Max
            writer.Write("Spec Max" + ",");
            for (i = 0; i < 34; i++)    //  12 + 2
                writer.Write(" " + ",");

            for (i = 0; i < (int)Global.SpecItem.ZM_Fullstroke; i++)
            {
                lvalue = Convert.ToDouble(m__G.mTestItem[i, 3]);
                writer.Write(lvalue.ToString("F2") + ",");   //  
            }
            for (i = 0; i < 28; i++)    //  9 + 2
                writer.Write(" " + ",");

            for (i = (int)Global.SpecItem.ZM_Fullstroke; i < m__G.sNUM_TESTITEM; i++)
            {
                lvalue = Convert.ToDouble(m__G.mTestItem[i, 3]);
                writer.Write(lvalue.ToString("F2") + ",");   //  
            }
            for (i = 0; i < 12; i++)
                writer.Write(" " + ",");   //  Write Unit

            writer.Write("\r\n");
            writer.Close();
            //////////////////////////////////////////////////////////////////////////////////
        }

        public bool IsAccessAbleFile(String filePath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                //에러가 발생한 이유는 이미 다른 프로세서에서 점유중이거나.
                //혹은 파일이 존재하지 않기 때문이다.
                //MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                if (fs != null)
                    //만약에 파일이 정상적으로 열렸다면 점유중이 아니다.
                    //다시 파일을 닫아줘야 한다.
                    fs.Close();
            }
            return true;
        }


        private void WriteSpec(string filename)
        {
            //string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Spec\\" + filename;
            string sFilePath = m__G.m_RootDirectory + "\\Spec\\" + filename;
            string tmpStr = string.Empty;
            m__G.strSpecFile = filename;
            tmpStr = filename;
            StreamWriter writer = new StreamWriter(sFilePath);

            for (int i = 0; i < this.dataGridView1.Rows.Count - 1; i++)
            {
                m__G.mTestItem[m__G.mGridToTestItem[i], 2] = this.dataGridView1.Rows[i].Cells[3].Value.ToString();
                m__G.mTestItem[m__G.mGridToTestItem[i], 3] = this.dataGridView1.Rows[i].Cells[4].Value.ToString();
            }
            string strtmp = "";
            for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
            {
                strtmp = m__G.mTestItem[i, 1] + "\t" + m__G.mTestItem[i, 2] + "\t" + m__G.mTestItem[i, 3];
                //MessageBox.Show(strtmp + "End of Line : " + i.ToString());
                writer.WriteLine(strtmp);
                if (MachineType == (int)CSH030Ex.MachineType.Master)
                    tmpStr = tmpStr + "," + m__G.mTestItem[i, 2].ToString() + "\t" + m__G.mTestItem[i, 3].ToString();
            }
            writer.Close();
            if (MachineType == (int)CSH030Ex.MachineType.Master)
            {
                m__G.fManage.PC2SendData("SSV", tmpStr, tmpStr.Length, 2);
                UIControltmr.Enabled = true;
            }

            SetTestSpec();
        }

        private void ReadSpecFile(string filename)
        {
            //string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Spec\\" + filename;
            string sFilePath = m__G.m_RootDirectory + "\\Spec\\" + filename;
            if (!File.Exists(sFilePath)) return;
            StreamReader sr = new StreamReader(sFilePath, System.Text.Encoding.Default);
            string allData = sr.ReadToEnd();
            sr.Close();
            string[] mylines = allData.Split("\n".ToCharArray());

            int specItemCount = m__G.sNUM_TESTITEM;

            if (m__G.sNUM_TESTITEM > (mylines.Length - 1))
                specItemCount = mylines.Length - 1;

            for (int i = 0; i < specItemCount; i++)
            {
                string[] figures = mylines[i].Split('\t');
                m__G.mTestItem[i, 2] = figures[1];
                m__G.mTestItem[i, 3] = figures[2];
            }
            SetTestSpec(true);
            ShowSpec();
        }

        public void ShowSpec()
        {
            int effRowNum = 0;
            for (int i = 0; i < m__G.sNUM_TESTITEM; i++)
            {
                if (m__G.mTestItem[i, 10].Contains('t') || m__G.mTestItem[i, 10].Contains('T'))
                {
                    this.dataGridView1.Rows[effRowNum].Cells[3].Value = m__G.mTestItem[i, 2];
                    this.dataGridView1.Rows[effRowNum].Cells[4].Value = m__G.mTestItem[i, 3];
                    effRowNum++;
                }
            }
        }

        public void SetTestSpec(bool bInit = false)
        {
            //int iChanged = 0;

            int i = 0;
            m__G.sSpec.AF_Fullstroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_Ratedstroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_Resolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FullSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FullLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FullHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FullCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_FwdResolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_BwdResolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_MaxCodeCurrent[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_2ndSettlingTime[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_2ndOvershoot[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_SettlingTime[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_Overshoot[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_StepStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_TotalTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_YawTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_PitchTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_Start_Cut[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.AF_End_Cut[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);

            m__G.sSpec.ZM_Fullstroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_Ratedstroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_Resolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FullSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FullLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FullHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FullCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_FwdResolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdSensitivity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdLinearity[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdHysteresis[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdCrosstalk[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_BwdResolution[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_MaxCodeCurrent[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_2ndSettlingTime[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_2ndOvershoot[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_SettlingTime[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_Overshoot[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_StepStroke[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_TotalTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_YawTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_PitchTilt[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_Start_Cut[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.ZM_End_Cut[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);

            m__G.sSpec.FRAAF_PMFreq[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAAF_PhaseMargin[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAAF_PMFreq2nd[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAAF_PhaseMargin2nd[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAAF_Gain10Hz[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAZM_PMFreq[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAZM_PhaseMargin[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAZM_PMFreq2nd[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAZM_PhaseMargin2nd[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);
            m__G.sSpec.FRAZM_Gain10Hz[1] = Convert.ToDouble(m__G.mTestItem[i++, 2]);

            i = 0;
            m__G.sSpec.AF_Fullstroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_Ratedstroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_Resolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FullSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FullLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FullHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FullCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_FwdResolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_BwdResolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_2ndSettlingTime[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_2ndOvershoot[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_SettlingTime[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_Overshoot[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_StepStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_TotalTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_YawTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_PitchTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_Start_Cut[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.AF_End_Cut[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);

            m__G.sSpec.ZM_Fullstroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_Ratedstroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_Resolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FullSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FullLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FullHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FullCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_FwdResolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdSensitivity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdLinearity[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdHysteresis[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdCrosstalk[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_BwdResolution[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_2ndSettlingTime[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_2ndOvershoot[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_SettlingTime[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_Overshoot[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_StepStroke[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_TotalTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_YawTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_PitchTilt[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_Start_Cut[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.ZM_End_Cut[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);

            m__G.sSpec.FRAAF_PMFreq[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAAF_PhaseMargin[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAAF_PMFreq2nd[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAAF_PhaseMargin2nd[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAAF_Gain10Hz[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAZM_PMFreq[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAZM_PhaseMargin[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAZM_PMFreq2nd[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAZM_PhaseMargin2nd[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
            m__G.sSpec.FRAZM_Gain10Hz[2] = Convert.ToDouble(m__G.mTestItem[i++, 3]);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_Vision_Click(object sender, EventArgs e)
        {
            //tb_DisplayName.Text = "Vision";
            //m__G.fVision.Show();
            if (!m__G.mFAL.mFAutoLearnLoaded)
            {
                m__G.mFAL.Show();
                m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                //mFAL.Size = new Size(1920, 1045);
                m__G.mFAL.Location = new Point(0, 0);
                m__G.mFAL.Hide();
            }

            ShowOperatorMode();
            SubForm_Show((int)Page.VISION);
            m__G.fVision.EnableBtns(false);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------   Load/Save   ----------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DataUpdateToParam()
        {
            int i = 0;



            m__G.sRecipe.iI2Cclock = Convert.ToInt32(this.dataGridView2[2, i++].Value);//

            string lstring = this.dataGridView2[2, i++].Value.ToString();
            string[] dataword;
            byte result = 0;
            if (lstring.Contains('x'))
            {
                dataword = lstring.Split("x".ToCharArray());   //  표시가 0x.. 의 HEX 로 표시된 경우
                result = (byte)(Int16.Parse(dataword[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            else
            {
                result = (byte)(Int16.Parse(lstring, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            m__G.sRecipe.iI2CslaveAddr = result;

            m__G.sRecipe.iGrabTimeLimit = Convert.ToInt32(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iMaxWaitAfterLastTrigger = Convert.ToInt32(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iTriggeredGrabImageCount = Convert.ToInt32(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iRawGain = Convert.ToInt32(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iGamma = Convert.ToDouble(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iExposure = Convert.ToInt32(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iEdgeBand = Convert.ToInt32(this.dataGridView2[2, i++].Value);//

            m__G.sRecipe.iLEDcurrentLL = Convert.ToDouble(this.dataGridView2[2, i++].Value);//
            m__G.sRecipe.iLEDcurrentLR = Convert.ToDouble(this.dataGridView2[2, i++].Value);//
            //m__G.sRecipe.iLEDcurrentRL = Convert.ToDouble(this.dataGridView2[2, i++].Value);//
            //m__G.sRecipe.iLEDcurrentRR = Convert.ToDouble(this.dataGridView2[2, i++].Value);//

            m__G.fVision.mLEDcurrent[1] = m__G.sRecipe.iLEDcurrentLL;
            m__G.fVision.mLEDcurrent[0] = m__G.sRecipe.iLEDcurrentLR;
            //m__G.fVision.mLEDcurrent[2] = m__G.sRecipe.iLEDcurrentRL;
            //m__G.fVision.mLEDcurrent[3] = m__G.sRecipe.iLEDcurrentRR;

            m__G.fGraph.mDriverIC.SetSlaveAddr(m__G.sRecipe.iI2CslaveAddr);
            m__G.oCam[0].mWaitLimitForNextTrigger   = m__G.sRecipe.iMaxWaitAfterLastTrigger;
            m__G.oCam[0].mTargetTriggerCount        = m__G.sRecipe.iTriggeredGrabImageCount;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void DataUpdateToGUI()
        {
            int i = 0;

            //General ======================


            this.dataGridView2[2, i++].Value = m__G.sRecipe.iI2Cclock.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iI2CslaveAddr.ToString("X");
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iGrabTimeLimit.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iMaxWaitAfterLastTrigger.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iTriggeredGrabImageCount.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iRawGain.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iGamma.ToString("F2");
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iExposure.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iEdgeBand.ToString();
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iLEDcurrentLL.ToString("F2");
            this.dataGridView2[2, i++].Value = m__G.sRecipe.iLEDcurrentLR.ToString("F2");
            //this.dataGridView2[2, i++].Value = m__G.sRecipe.iLEDcurrentRL.ToString("F2");
            //this.dataGridView2[2, i++].Value = m__G.sRecipe.iLEDcurrentRR.ToString("F2");
            m__G.fVision.SetExposure(0, m__G.sRecipe.iExposure);
            m__G.fVision.SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            m__G.fVision.SetEdgeBand(m__G.sRecipe.iEdgeBand);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private string CheckDataValidate()
        {
            return "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private bool bSaveCurrentRecipe(string sFileName)
        {

            //string msg = CheckDataValidate();
            string tmpStr = string.Empty;
            //if (msg != "") { MessageBox.Show(msg); return false; }

            DataUpdateToParam();

            m__G.fGraph.mDriverIC.SetI2CClock(m__G.sRecipe.iI2Cclock, m__G.mCamCount, m__G.mChannelCount);

            //string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Recipe\\";
            string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";

            m__G.sRecipe.sRecipeName = sFileName;
            tmpStr = sFileName;
            StreamWriter Writer = new StreamWriter(sFilePath + sFileName);
            string lToDoList = "";
            int i = 0;
            for (i = 0; i < listBox2.Items.Count; i++)
            {
                lToDoList = lToDoList + listBox2.Items[i].ToString();
                if (i < listBox2.Items.Count - 1)
                    lToDoList = lToDoList + "\t";
            }
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                tmpStr = tmpStr + "," + lToDoList;
            Writer.WriteLine(lToDoList);

            for (i = 0; i < this.dataGridView2.Rows.Count; i++)
            {
                if (this.dataGridView2[1, i].Value.ToString().Contains("LED")) break;
                Writer.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + this.dataGridView2[2, i].Value.ToString());
                if (MachineType == (int)CSH030Ex.MachineType.Master)
                    tmpStr = tmpStr + "," + this.dataGridView2[2, i].Value.ToString();
            }

            Writer.Close();
            //if (MachineType == (int)TZAF.MachineType.Master)
            //{
            //    m__G.fManage.PC2SendData("RSV", tmpStr, tmpStr.Length, 2);
            //    UIControltmr.Enabled = true;
            //}

            Writer = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\LEDPower.txt");
            Writer.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + this.dataGridView2[2, i++].Value.ToString());
            Writer.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + this.dataGridView2[2, i++].Value.ToString());
            //Writer.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + this.dataGridView2[2, i++].Value.ToString());
            //Writer.WriteLine(this.dataGridView2[1, i].Value.ToString() + "\t" + this.dataGridView2[2, i++].Value.ToString());
            Writer.Close();


            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private bool bLoadRecipe(string sFileName)
        {
            try
            {
                //string sFilePath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Recipe\\";
                string sFilePath = m__G.m_RootDirectory + "\\Recipe\\";

                string sPathFileName = sFilePath + sFileName;
                string[] StrArr;
                string allData;
                StreamReader Reader;

                if (File.Exists(sPathFileName) == false)
                {
                    return false;
                }
                else
                {

                    if (File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\LEDPower.txt"))
                    {
                        Reader = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\LEDPower.txt");
                        string LEDallData = Reader.ReadToEnd();
                        Reader.Close();
                        string[] LEDrows = LEDallData.Split("\r".ToCharArray());

                        StrArr = LEDrows[0].Split('\t');
                        m__G.sRecipe.iLEDcurrentLL = Convert.ToDouble(StrArr[1]);
                        m__G.fVision.mLEDcurrent[1] = m__G.sRecipe.iLEDcurrentLL;
                        StrArr = LEDrows[1].Split('\t');
                        m__G.sRecipe.iLEDcurrentLR = Convert.ToDouble(StrArr[1]);
                        m__G.fVision.mLEDcurrent[0] = m__G.sRecipe.iLEDcurrentLR;
                        //StrArr = LEDrows[2].Split('\t'); m__G.sRecipe.iLEDcurrentRL = Convert.ToDouble(StrArr[1]); m__G.fVision.mLEDcurrent[2] = m__G.sRecipe.iLEDcurrentRL;
                        //StrArr = LEDrows[3].Split('\t'); m__G.sRecipe.iLEDcurrentRR = Convert.ToDouble(StrArr[1]); m__G.fVision.mLEDcurrent[3] = m__G.sRecipe.iLEDcurrentRR;
                    }
                    Reader = new StreamReader(sPathFileName);
                    allData = Reader.ReadToEnd();
                    Reader.Close();
                    string[] rows = allData.Split("\r".ToCharArray());

                    //MessageBox.Show("bLoadRecipe() rows.Length=" + rows.Length);

                    bool[] bBox2Item = new bool[mToDoList.Length];

                    for (int m = 0; m < mToDoList.Length; m++)
                        bBox2Item[m] = false;

                    listBox1.Items.Clear();
                    listBox2.Items.Clear();


                    int i = 0;
                    StrArr = rows[i++].Split('\t');
                    for (int j = 0; j < StrArr.Length; j++)
                    {
                        for (int m = 0; m < mToDoList.Length; m++)
                        {
                            if (StrArr[j] == mToDoList[m])
                            {
                                listBox2.Items.Add(StrArr[j]);
                                bBox2Item[m] = true;
                                break;
                            }
                        }
                    }
                    for (int m = 0; m < mToDoList.Length; m++)
                    {
                        if (!bBox2Item[m])
                            listBox1.Items.Add(mToDoList[m]);
                    }
                    StrArr = rows[i++].Split('\t');
                    m__G.sRecipe.iI2Cclock = Convert.ToInt32(StrArr[1]);
                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t');
                    string lstring = StrArr[1];
                    string[] dataword;
                    byte result = 0;

                    if (lstring.Contains('x'))
                    {
                        dataword = lstring.Split("x".ToCharArray());   //  표시가 0x.. 의 HEX 로 표시된 경우
                        result = (byte)(Int16.Parse(dataword[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        result = (byte)(Int16.Parse(lstring, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    }
                    m__G.sRecipe.iI2CslaveAddr = result;
                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t'); 
                    m__G.sRecipe.iGrabTimeLimit = Convert.ToInt32(StrArr[1]);
                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t'); 
                    m__G.sRecipe.iMaxWaitAfterLastTrigger = Convert.ToInt32(StrArr[1]);
                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t'); 
                    m__G.sRecipe.iTriggeredGrabImageCount = Convert.ToInt32(StrArr[1]);

                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t');
                    if (StrArr.Length < 2) return true;
                    m__G.sRecipe.iRawGain = Convert.ToInt32(StrArr[1]);

                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t');
                    if (StrArr.Length < 2) return true;
                    m__G.sRecipe.iGamma = Convert.ToDouble(StrArr[1]);

                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t');
                    if (StrArr.Length < 2) return true;
                    m__G.sRecipe.iExposure = Convert.ToInt32(StrArr[1]);

                    if (i == rows.Length) return true;
                    StrArr = rows[i++].Split('\t');
                    if (StrArr.Length < 2) return true;
                    m__G.sRecipe.iEdgeBand = Convert.ToInt32(StrArr[1]);

                    m__G.fGraph.mDriverIC.SetSlaveAddr(m__G.sRecipe.iI2CslaveAddr);

                    m__G.oCam[0].mWaitLimitForNextTrigger   = m__G.sRecipe.iMaxWaitAfterLastTrigger;
                    m__G.oCam[0].mTargetTriggerCount        = m__G.sRecipe.iTriggeredGrabImageCount;
                }
                WritePrevRecipe(sFileName);
                setJobFile(sFileName);

                DataUpdateToGUI();
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;

            }


        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void setJobFile(string jobname)
        {
            m__G.sRecipe.sRecipeName = jobname;
            tb_CurrJob.Text = jobname;
            //            tb_CurrJob.ForeColor = Color.Black;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void setSpecFile(string resultname)
        {
            m__G.strSpecFile = resultname;
            tb_CurrResult.Text = resultname;
            //            tb_CurrResult.ForeColor = Color.LightGray;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void cb_Edit_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Edit.Checked)
            {

                this.dataGridView2.ReadOnly = false;

                for (int row = 0; row < this.dataGridView2.Rows.Count; row++)
                {
                    {
                        this.dataGridView2[2, row].Style.BackColor = Color.White;
                        this.dataGridView2[0, row].ReadOnly = true;
                        this.dataGridView2[1, row].ReadOnly = true;
                        this.dataGridView2[3, row].ReadOnly = true;
                        this.dataGridView2[4, row].ReadOnly = true;
                    }
                }
                if (MachineType == (int)CSH030Ex.MachineType.Slave)
                {
                    for (int i = 0; i < this.dataGridView2.Rows.Count - 5; i++)
                    {
                        this.dataGridView2[2, i].ReadOnly = true;
                        this.dataGridView2[2, i].Style.BackColor = Color.LightGray;


                    }

                }
                //this.dataGridView2[2, 28].ReadOnly = true;
            }
            else
            {
                this.dataGridView2.ReadOnly = true;
                for (int row = 0; row < this.dataGridView2.Rows.Count; row++)
                {
                    this.dataGridView2[2, row].Style.BackColor = Color.LightGray;
                }
            }
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnSave.Enabled = true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void cb_EditSpec_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_EditSpec.Checked == true)
            {
                this.dataGridView1.ReadOnly = false;
                for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
                {
                    {
                        this.dataGridView1[3, row].Style.BackColor = Color.White;
                        this.dataGridView1[4, row].Style.BackColor = Color.White;
                        this.dataGridView1[0, row].ReadOnly = true;
                        this.dataGridView1[1, row].ReadOnly = true;
                        this.dataGridView1[2, row].ReadOnly = true;
                        this.dataGridView1[5, row].ReadOnly = true;
                    }
                }
            }
            else
            {
                this.dataGridView1.ReadOnly = true;
                for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
                {
                    this.dataGridView1[3, row].Style.BackColor = Color.LightGray;
                    this.dataGridView1[4, row].Style.BackColor = Color.LightGray;
                }
            }
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btn_SaveSpecFile.Enabled = true;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //                                             Result 표시
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void InitDataGridView()
        {

            panel1.Controls.Add(dataGridView1);

            int i = 0;

            this.dataGridView1.ColumnCount = 6;
            this.dataGridView1.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView1.ColumnCount; i++)
            {
                this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.BackgroundColor = Color.LightGray;


            // Column
            this.dataGridView1.Columns[0].Name = "Axis";
            this.dataGridView1.Columns[1].Name = "Test Item";
            this.dataGridView1.Columns[2].Name = "Result";
            this.dataGridView1.Columns[3].Name = "Min";
            this.dataGridView1.Columns[4].Name = "Max";
            this.dataGridView1.Columns[5].Name = "unit";
            for (i = 0; i < 6; i++)
                this.dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);


            this.dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            this.dataGridView1.Columns[0].Width = 80;
            this.dataGridView1.Columns[1].Width = 140;
            this.dataGridView1.Columns[2].Width = 0;
            this.dataGridView1.Columns[3].Width = 80;
            this.dataGridView1.Columns[4].Width = 80;
            this.dataGridView1.Columns[5].Width = 87;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";
            for (i = 0; i < m__G.sNUM_TESTITEM; i++)
            {
                if (m__G.mTestItem[i, 0] != "")
                    colTitle = m__G.mTestItem[i, 0];

                if (m__G.mTestItem[i, 10].Contains('t') || m__G.mTestItem[i, 10].Contains('T'))
                {
                    this.dataGridView1.Rows.Add(colTitle, m__G.mTestItem[i, 1], "", m__G.mTestItem[i, 2], m__G.mTestItem[i, 3], m__G.mTestItem[i, 9]);
                    if (colTitle != "")
                    {
                        colTitle = "";
                        bColorChange = !bColorChange;
                    }

                    if (bColorChange)
                    {
                        this.dataGridView1[0, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView1[1, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView1[5, effRowNum].Style.BackColor = Color.Lavender;
                    }
                    else
                    {
                        this.dataGridView1[0, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView1[1, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView1[5, effRowNum].Style.BackColor = Color.White;
                    }

                    m__G.mTestItemToGrid[i] = effRowNum;
                    m__G.mGridToTestItem[effRowNum] = i;
                    effRowNum++;
                }
                else
                    m__G.mTestItemToGrid[i] = -1;
            }

            this.dataGridView1.Rows.Add("", "", "", "", "");
            m__G.wrSytemLog("InitDataGridView effRowNum:" + effRowNum.ToString());

            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.ColumnHeadersHeight = 22;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView1.Rows[i].Height = 16;
                this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView1[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[5, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }
            this.dataGridView1.Rows[i].Height = 16;
            this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            for (int colum = 3; colum < this.dataGridView1.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
                {
                    this.dataGridView1[colum, row].Style.BackColor = Color.LightGray;
                    this.dataGridView1.ReadOnly = true;
                }
            }
            this.dataGridView1.ReadOnly = true;

        }
        public void UpdateDataGridView()
        {
            try
            {
                int i = 0;

                int delNum = this.dataGridView1.RowCount;
                for (i = 0; i < delNum; i++)
                {
                    this.dataGridView1.Rows.Remove(this.dataGridView1.Rows[0]);
                }

                this.dataGridView1.Font = new Font("Calibri", 10, FontStyle.Bold);
                for (i = 0; i < this.dataGridView1.ColumnCount; i++)
                {
                    this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                this.dataGridView1.RowHeadersVisible = false;
                this.dataGridView1.BackgroundColor = Color.LightGray;

                //// Row
                int effRowNum = 0;
                bool bColorChange = true;
                string colTitle = "";
                for (i = 0; i < m__G.sNUM_TESTITEM; i++)
                {
                    if (m__G.mTestItem[i, 0] != "")
                        colTitle = m__G.mTestItem[i, 0];

                    if (m__G.mTestItem[i, 10].Contains('t') || m__G.mTestItem[i, 10].Contains('T'))
                    {
                        this.dataGridView1.Rows.Add(colTitle, m__G.mTestItem[i, 1], "", m__G.mTestItem[i, 2], m__G.mTestItem[i, 3], m__G.mTestItem[i, 9]);
                        if (colTitle != "") colTitle = "";
                        if (m__G.mTestItem[i, 0].Length > 0)
                            bColorChange = !bColorChange;

                        if (bColorChange)
                        {
                            this.dataGridView1[0, effRowNum].Style.BackColor = Color.Lavender;
                            this.dataGridView1[1, effRowNum].Style.BackColor = Color.Lavender;
                            this.dataGridView1[5, effRowNum].Style.BackColor = Color.Lavender;
                        }
                        else
                        {
                            this.dataGridView1[0, effRowNum].Style.BackColor = Color.White;
                            this.dataGridView1[1, effRowNum].Style.BackColor = Color.White;
                            this.dataGridView1[5, effRowNum].Style.BackColor = Color.White;
                        }

                        m__G.mTestItemToGrid[i] = effRowNum;
                        m__G.mGridToTestItem[effRowNum] = i;
                        effRowNum++;
                    }
                    else
                        m__G.mTestItemToGrid[i] = -1;
                }

                this.dataGridView1.Rows.Add("", "", "", "", "");

                this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                this.dataGridView1.ColumnHeadersHeight = 22;

                for (i = 0; i < effRowNum; i++)
                {
                    this.dataGridView1.Rows[i].Height = 16;
                    this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                    this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                    this.dataGridView1[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                    this.dataGridView1[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                    this.dataGridView1[5, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
                }
                this.dataGridView1.Rows[i].Height = 16;
                this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.White;

                for (int colum = 3; colum < this.dataGridView1.ColumnCount - 1; colum++)
                {
                    for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
                    {
                        this.dataGridView1[colum, row].Style.BackColor = Color.LightGray;
                        this.dataGridView1.ReadOnly = true;
                    }
                }
                this.dataGridView1.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // 세로줄 병합
            if (e.RowIndex < 1 || e.ColumnIndex < 0)
                return;

            if (e.ColumnIndex == 0)
            {
                e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

                if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex))
                    e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
                else
                    e.AdvancedBorderStyle.Top = this.dataGridView1.AdvancedCellBorderStyle.Top;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------
        bool IsTheSameCellValue(int column, int row)
        {
            DataGridViewCell cell1 = this.dataGridView1[column, row];
            DataGridViewCell cell2 = this.dataGridView1[column, row - 1];
            if (cell1.Value == null || cell2.Value == null)
                return false;
            if (cell1.Value.ToString() == cell2.Value.ToString())
                return true;
            else
                return false;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == 0)
                return;
            if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex))
            {
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void ChkCLAF()
        {
            //LST_002_01: First add the items to the Assigned List. 
            //btnInitAFDrvIC.Show();
            foreach (string item in listBox1.SelectedItems)
            {
                listBox2.Items.Add(item);
            }

            int total = listBox1.SelectedItems.Count;
            for (int x = 0; x < total; x++)
                listBox1.Items.Remove(listBox1.SelectedItems[0]);
        }

        private void btn_ToRight_Click(object sender, EventArgs e)
        {
            ChkCLAF();
            btnSave.Enabled = true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_ToLeft_Click(object sender, EventArgs e)
        {
            //LST_002_01: First add the items to the Assigned List. 
            foreach (string item in listBox2.SelectedItems)
            {
                listBox1.Items.Add(item);
            }

            //LST_002_02:Remove the selected items from the Area List
            int total = listBox2.SelectedItems.Count;
            for (int x = 0; x < total; x++)
                listBox2.Items.Remove(listBox2.SelectedItems[0]);
            btnSave.Enabled = true;

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_Up_Click(object sender, EventArgs e)
        {
            if (this.listBox2.SelectedIndex == -1 || this.listBox2.SelectedIndex == 0)
                return;

            Object select, previous, temp;
            select = listBox2.Items[listBox2.SelectedIndex];
            previous = listBox2.Items[listBox2.SelectedIndex - 1];
            temp = select;
            select = previous;
            previous = temp;
            listBox2.Items[listBox2.SelectedIndex] = select;
            listBox2.Items[listBox2.SelectedIndex - 1] = previous;
            listBox2.SelectedIndex--;
            btnSave.Enabled = true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btn_Down_Click(object sender, EventArgs e)
        {
            if (this.listBox2.SelectedIndex == -1 || this.listBox2.SelectedIndex == listBox2.Items.Count - 1)
                return;

            Object select, next, temp;
            select = listBox2.Items[listBox2.SelectedIndex];
            next = listBox2.Items[listBox2.SelectedIndex + 1];
            temp = select;
            select = next;
            next = temp;
            listBox2.Items[listBox2.SelectedIndex] = select;
            listBox2.Items[listBox2.SelectedIndex + 1] = next;
            listBox2.SelectedIndex++;
            btnSave.Enabled = true;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------             Measurement Start              ------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //public void mRepeatTest(int port)
        //{
        //    m_StopRepeat[port] = false;
        //    Thread Thread_Repeat = new Thread(() => Process_Repeat(port));
        //    Thread_Repeat.Start();
        //    //ShowOperatorMode();
        //}
        public void HoldMutex(int me)
        {
            while (true)
            {
                if (m_Mutex + me < 5)
                {
                    m_Mutex += me;
                    return;
                }
                Thread.Sleep(30);
            }
        }
        public void FreeMutex(int me)
        {
            m_Mutex -= me;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //private void Process_Start(int port)
        //{
        //    //  To Do List 에 있는 항목들을 순차적으로 Test 하기 위해 항목개수를 확인한다.
        //    //  비어있는 경우 바로 Return


        //    if (m__G.oCam[port].IsLiveA == true)
        //        m__G.oCam[port].HaltA();

        //    //if (m__G.oCam[0].IsLiveA == true)
        //    //    m__G.oCam[0].HaltA();
        //    //if (m__G.oCam[1].IsLiveA == true)
        //    //    m__G.oCam[1].HaltA();

        //    IsTesting[port] = true;
        //    if (m__G.sHistIndex % 1000 == 0)
        //        m__G.ResetwrSystemLog(m__G.sHistIndex);

        //    int count = listBox2.Items.Count;
        //    if (count == 0)
        //    {
        //        Thread.Sleep(40000);
        //        m__G.sHistIndex++;
        //        m_StopRepeat[port] = true;
        //        IsTesting[port] = false;
        //        return;
        //    }
        //    //int isMarks = 0;

        //    //m__G.fManage.AddOperatorLog(0, "Process_Start Port " + port.ToString(), false);
        //    //m__G.fManage.AddOperatorLog(port * 2+1, "Process_Start");

        //    int ch = port * 2;

        //    m__G.fManage.SaveLogNClear(ch);
        //    //m__G.fManage.SaveLogNClear(ch + 1);

        //    m__G.m_ChannelOn[ch] = true;
        //    m__G.m_ChannelEff[ch] = true;
        //    if (m__G.mChannelCount > 1)
        //    {
        //        m__G.m_ChannelOn[ch + 1] = true;
        //        m__G.m_ChannelEff[ch + 1] = true;
        //    }
        //    else
        //    {
        //        m__G.m_ChannelOn[ch + 1] = false;
        //        m__G.m_ChannelEff[ch + 1] = false;
        //    }

        //    m__G.fManage.ClearLog(ch);
        //    //m__G.fManage.ClearLog(ch + 1);

        //    m__G.errMsg[ch] = "";
        //    //m__G.errMsg[ch + 1] = "";

        //    if (!m__G.m_bSkipVisionCheck)
        //    {
        //        m__G.fVision.FindMarks(port);

        //        if (!m__G.m_ChannelOn[ch])
        //            m__G.errMsg[ch] = "Vision Check : Socket Empty";

        //        //if (!m__G.m_ChannelOn[ch + 1])
        //        //    m__G.errMsg[ch + 1] = "Vision Check : Socket Empty";
        //    }

        //    if (!m__G.m_ChannelOn[ch] && !m__G.m_ChannelOn[ch + 1])
        //    {

        //        m__G.sCIndex[ch] = 0;
        //        m__G.sCIndex[ch + 1] = 0;
        //        if (m__G.mForcedSampleNumber > 0)
        //        {
        //            if (m__G.mForcedApplied == 1)
        //            {
        //                m__G.mForcedApplied = 0;
        //                m__G.mForcedSampleNumber = 0;
        //            }
        //        }

        //        m_StopRepeat[port] = true;
        //        IsTesting[port] = false;
        //        if (m__G.errMsg[ch] == "")
        //            m__G.errMsg[ch] = "Socket Empty";
        //        //if (m__G.errMsg[ch + 1] == "")
        //        //    m__G.errMsg[ch + 1] = "Socket Empty";

        //        m__G.fManage.AddOperatorLog(ch, m__G.errMsg[ch], true);
        //        //m__G.fManage.AddOperatorLog(ch + 1, m__G.errMsg[ch + 1], true);
        //        return;
        //    }

        //    int lToDoItem = 0;
        //    long startTime = 0;
        //    long startTimeOrg = 0;
        //    SupremeTimer.QueryPerformanceCounter(ref startTime);
        //    SupremeTimer.QueryPerformanceFrequency(ref m__G.TimerFrequency);

        //    startTimeOrg = startTime;
        //    long endTime = 0;
        //    double ellipse;
        //    int i = 0;

        //    DataUpdateToParam();


        //    m__G.mbHavePH = false;
        //    m__G.mbEPAMode = false;
        //    //bool bSkipWriteToDriverIC = false;
        //    m__G.mIsBU24532 = false;

        //    for (i = 0; i < count; i++)
        //    {
        //        string TmpItem = listBox2.Items[i].ToString();
        //        if (TmpItem.Contains("Phase"))
        //            m__G.mbHavePH = true;

        //        if (TmpItem.Contains("BU"))
        //        {
        //            //bSkipWriteToDriverIC = true;
        //            m__G.mIsBU24532 = true;
        //        }
        //    }

        //    bool loopContinue = true;
        //    //bool res = false;
        //    //string logText = "";
        //    i = 0;

        //    //  다음은 Hal Cal 을 하지 않는 경우 Default 값으로 적용된다.
        //    //////////////////////////////////////////////////////////////////////////
        //    //m__G.fManage.ClearHallPolarity();
        //    DateTime lnow = DateTime.Now;


        //    if (m_RepeatProcess[port] == 0)
        //    {
        //        HoldMutex(3);
        //        if (lnow.DayOfYear != m__G.mLastDateTime.DayOfYear)
        //        {
        //            m__G.sHistIndex = 1;
        //            m__G.sCIndex[0] = 0;
        //            m__G.sCIndex[1] = 0;
        //            m__G.sCIndex[2] = 0;
        //            m__G.sCIndex[3] = 0;
        //            m__G.mDailyTotalTested = 0;
        //            m__G.mDailyTotalFail = 0;
        //            m__G.fManage.m_LastSampleNumber = 0;
        //        }
        //        else
        //        {
        //            int maxIndex = Math.Max(m__G.sCIndex[0], m__G.sCIndex[1]);
        //            //if (MachineType != (int)TZAF.MachineType.Master)
        //            //{
        //            //    maxIndex = Math.Max(maxIndex, m__G.sCIndex[2]);
        //            //    maxIndex = Math.Max(maxIndex, m__G.sCIndex[3]);
        //            //}

        //            if (maxIndex != 0)
        //                m__G.sHistIndex = maxIndex + 1;
        //            else
        //                m__G.sHistIndex++;

        //            if (m__G.sHistIndex > 4996) m__G.sHistIndex = 1;

        //            m__G.fManage.AddOperatorLog(ch, "maxIndex = " + m__G.sCIndex[0].ToString() + ":" + m__G.sCIndex[1].ToString() + " >> " + maxIndex + " sHistIndex=" + m__G.sHistIndex);
        //        }

        //        if (m__G.mForcedSampleNumber > 0)
        //        {
        //            m__G.sHistIndex = m__G.mForcedSampleNumber;
        //            if (m__G.mForcedApplied == 1)
        //            {
        //                m__G.sHistIndex += 2; //190123 인덱스 정상증가                    
        //                                      // m__G.sHistIndex++;
        //                m__G.mForcedApplied = 0;
        //                m__G.mForcedSampleNumber = 0;
        //                m__G.fManage.AddOperatorLog(ch, "mForcedApplied m__G.sHistIndex=" + m__G.sHistIndex.ToString());
        //            }
        //            else
        //            {
        //                m__G.mForcedApplied = 1;
        //            }
        //            m__G.fManage.AddOperatorLog(ch, "Forced Sample Number = " + m__G.sHistIndex.ToString());
        //        }
        //        m__G.mLastDateTime = lnow;

        //        if (m__G.m_ChannelOn[ch])
        //        {

        //            if (m_nIndexSkipped[ch] > 0)
        //            {
        //                m__G.sCIndex[ch] = m_nIndexSkipped[ch];
        //                m_nIndexSkipped[ch] = 0;
        //                m__G.fManage.AddOperatorLog(ch, "m_nIndexSkipped = " + m__G.sCIndex[ch].ToString());
        //            }
        //            else
        //            {
        //                m__G.sCIndex[ch] = m__G.sHistIndex++;
        //                m__G.fManage.AddOperatorLog(ch, "standard index m__G.sCIndex[" + ch.ToString() + "] = " + m__G.sCIndex[ch].ToString());
        //            }
        //            m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.PassFail] = 0;
        //        }
        //        //if (m__G.m_ChannelOn[ch + 1])
        //        //{
        //        //    if (m_nIndexSkipped[ch + 1] > 0)
        //        //    {
        //        //        m__G.sCIndex[ch + 1] = m_nIndexSkipped[ch + 1];
        //        //        m_nIndexSkipped[ch + 1] = 0;
        //        //        m__G.fManage.AddOperatorLog(ch + 1, "m_nIndexSkipped = " + m__G.sCIndex[ch + 1].ToString());
        //        //    }
        //        //    else
        //        //    {
        //        //        m__G.sCIndex[ch + 1] = m__G.sHistIndex++;
        //        //        m__G.fManage.AddOperatorLog(ch + 1, "standard index m__G.sCIndex[" + (ch + 1).ToString() + "] = " + m__G.sCIndex[ch + 1].ToString());
        //        //    }
        //        //    m__G.sHistArray[m__G.sCIndex[ch + 1], (int)Global.SpecItem.PassFail] = 0;
        //        //}
        //        FreeMutex(3);
        //    }

        //    //int iWait = 0;

        //    m__G.fVision.SetOrgExposure(0);
        //    //if (m__G.mCamCount > 1)
        //    //    m__G.fVision.SetOrgExposure(1);

        //    while (i < count)
        //    {

        //        if (listBox2.Items.Count < i) break;
        //        m__G.mCurrentSequence[port] = i;
        //        string testItem = listBox2.Items[i].ToString();

        //        if (m__G.m_ChannelOn[ch])
        //        {
        //            //m__G.fGraph.mDriverIC.AFWakeUp(ch);
        //            m__G.fManage.AddOperatorLog(ch, m__G.sCIndex[ch].ToString() + ">> " + testItem, true);
        //        }
        //        switch (testItem)
        //        {
        //            case "Triggered Grab":
        //                break;
        //            case "Bulk Grab":
        //                break;
        //            case "Continuous Grab":
        //                break;
        //            default:
        //                break;
        //        }
        //        if (!m__G.m_ChannelOn[ch] )
        //            loopContinue = false;
        //        if (!m__G.m_ChannelEff[ch] )
        //            loopContinue = false;

        //        if (m__G.errMsg[ch] != "" )
        //        {
        //            loopContinue = false;
        //            m__G.fManage.AddOperatorLog(ch, m__G.errMsg[ch], false);
        //            //m__G.fManage.AddOperatorLog(ch + 1, m__G.errMsg[ch + 1], false);
        //            break;
        //        }
        //        if (!m__G.m_ChannelOn[ch] )
        //        {
        //            m__G.fManage.AddOperatorLog(ch, "Socket Empty", false);
        //            //m__G.fManage.AddOperatorLog(ch + 1, "Socket Empty", false);
        //            loopContinue = false;
        //            break;
        //        }
        //        if (m__G.mbSuddenStop[port])
        //        {
        //            m__G.fManage.AddOperatorLog(ch, "Forced Stop!", true);
        //            loopContinue = false;
        //            break;
        //        }
        //        SupremeTimer.QueryPerformanceCounter(ref endTime);
        //        ellipse = (endTime - startTime) / (double)(m__G.TimerFrequency);
        //        m__G.fManage.AddOperatorLog(ch, testItem + "\t" + ellipse.ToString("F3") + "sec", true);
        //        m__G.sHistArray[m__G.sCIndex[ch], lToDoItem] = ellipse;
        //        //m__G.sHistArray[m__G.sCIndex[ch + 1], lToDoItem] = ellipse;
        //        startTime = endTime;
        //        if (!loopContinue)
        //        {
        //            TestItemTotalFail(port, testItem);
        //            m_StopRepeat[port] = true;
        //            break;
        //        }
        //        else
        //        {

        //            i++;
        //        }

        //        Thread.Sleep(100);
        //    }
        //    m__G.fVision.LEDOff(port);
        //    ellipse = (endTime - startTimeOrg) / (double)(m__G.TimerFrequency);

        //    m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.Time_Total] = ellipse;
        //    //m__G.sHistArray[m__G.sCIndex[ch + 1], (int)Global.SpecItem.Time_Total] = ellipse;

        //    m__G.fManage.AddOperatorLog(ch, "Total Test Time\t" + ellipse.ToString("F3") + "sec", true);
        //    //m__G.fManage.AddOperatorLog(ch + 1, "Total Test Time\t" + ellipse.ToString("F3") + "sec", true);

        //    if (m__G.mbSuddenStop[port])
        //    {

        //        //MessageBox.Show("m__G.mbSuddenStop = " + m__G.mbSuddenStop.ToString());
        //        m__G.mbSuddenStop[port] = false;
        //        m__G.errMsg[port * 2] = "Forced Stop";
        //        //m__G.errMsg[port * 2 + 1] = "Forced Stop";
        //        m_StopRepeat[port] = true;
        //    }
        //    else
        //    {
        //        HoldMutex(3);
        //        WriteResultBin(); // 정상적으로 시험이 끝난 경우 결과 저장
        //        FreeMutex(3);

        //        int[] lPassFail = new int[2] { 0, 0 };
        //        if (m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.PassFail] == 0)
        //            lPassFail[0] = 2;
        //        else
        //            lPassFail[0] = 9;
        //        //if (m__G.sHistArray[m__G.sCIndex[ch + 1], (int)Global.SpecItem.PassFail] == 0)
        //        //    lPassFail[1] = 2;
        //        //else
        //        //    lPassFail[1] = 9;

        //        //m__G.wrSytemLog("Port " + port.ToString() + ">> WriteResultToDriverIC() Finish");

        //        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    }
        //    //            m_StopRepeat[port] = true;
        //    IsTesting[port] = false;
        //    return;
        //    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //}

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
  
        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (MachineType != (int)CSH030Ex.MachineType.Slave)
            {
                if (e.RowIndex < 0)
                {
                    m__G.fTestItemOnOff.ShowDialog();
                    DataUpdateToGUI();
                }
            }

        }

        public void SetPerformanceSpec()
        {
            //m__G.sSpec.sAF_Stroke_min = Convert.ToDouble(this.dataGridView1.Rows[0].Cells[3].Value);
            //m__G.sRecipe.spec_AF_Stroke_min = Convert.ToDouble(this.dataGridView1.Rows[0].Cells[3].Value);
            //m__G.sRecipe.spec_AF_Stroke_min = Convert.ToDouble(this.dataGridView1.Rows[0].Cells[3].Value);
            //m__G.sRecipe.spec_AF_Stroke_min = Convert.ToDouble(this.dataGridView1.Rows[0].Cells[3].Value);
            //m__G.sRecipe.spec_AF_Stroke_min = Convert.ToDouble(this.dataGridView1.Rows[0].Cells[3].Value);
        }

        //private void Process_Repeat(int port = 0)
        //{
        //    try
        //    {
        //        m_RepeatProcess[port] = 0;
        //        int showItrCount = 0;
        //        m__G.sRepeatRunIndex++; //  화면에 표시할 것.

        //        for (int j = 0; j < 5; j++)
        //        {
        //            for (int i = 0; i < 5000; i++)
        //            {
        //                m_ItrStroke_L2[j, i] = 0;
        //                m_ItrStroke_L3[j, i] = 0;
        //                m_ItrStroke_L2R[j, i] = 0;
        //                m_ItrStroke_L3R[j, i] = 0;
        //            }
        //        }

        //        int ch = port * 2;

        //        //m__G.wrSytemLog("Process_Repeat(" + port.ToString() + ")");
        //        m_RepeatRawFile = new string[m__G.sRecipe.iTriggeredGrabImageCount];

        //        while (!m_StopRepeat[port])
        //        {
        //            showItrCount = m_RepeatProcess[port] + 1;

        //            Process_Start(port);

        //            if (++m_RepeatProcess[port] == m__G.sRecipe.iTriggeredGrabImageCount) break;
        //        }
        //        //m__G.fManage.AddOperatorLog(ch, " Finish Process_Repeat(" + ch.ToString() + ")");
        //        m_StopRepeat[port] = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }

        //}

        private void btnInitAFDrvIC_Click(object sender, EventArgs e)
        {

        }




        void TestItemTotalFail(int port, string item)
        {
            int ch = port * 2;
            //case "AF":  
            //case "AF Step Response": 
            //case "CL AF": 
            //case "CL AF Step Response":
            //case "OIS X": 
            //case "OIS Y": 
            //case "OIS FRA X": 
            //case "OIS FRA Y": 
            int lHindex = m__G.sCIndex[ch];

            switch (item)
            {
                case "2nd AF Step Response":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_2ndSettlingTime] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_2ndOvershoot] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_2ndStepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "02" + m__G.sCIndex[ch].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_2ndSettlingTime] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_2ndOvershoot] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_2ndStepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "02" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "AF Step Response":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_SettlingTime] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Overshoot] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_StepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "02" + m__G.sCIndex[ch].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_SettlingTime] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Overshoot] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_StepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "02" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "2nd Zoom Step Response":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_2ndSettlingTime] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_2ndOvershoot] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_2ndStepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "03" + m__G.sCIndex[ch].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_2ndSettlingTime] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_2ndOvershoot] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_2ndStepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "03" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "Zoom Step Response":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_SettlingTime] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Overshoot] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_StepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "03" + m__G.sCIndex[ch].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_SettlingTime] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Overshoot] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_StepStroke] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "03" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "Scan AF":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Fullstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_BwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Resolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdResolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_YawTilt] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_TotalTilt] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Start_Cut] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_End_Cut] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallCenter] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "04" + m__G.sCIndex[ch].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Fullstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_BwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Resolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdResolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_YawTilt] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_TotalTilt] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Start_Cut] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_End_Cut] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallCenter] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "04" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "Scan Zoom":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_BwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Resolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdResolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_YawTilt] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Start_Cut] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_End_Cut] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallCenter] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "05" + m__G.sCIndex[ch].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Fullstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_BwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Resolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdResolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_YawTilt] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Start_Cut] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_End_Cut] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallCenter] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "05" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;
                case "Scan AF-Zoom : No AF Lens":
                case "Scan AF-Zoom":
                case "Scan AF-Zoom : Fixed Zoom Lens":
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Fullstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_BwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_Resolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_FwdResolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_TotalTilt] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_YawTilt] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallCenter] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.AF_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "04" + m__G.sCIndex[ch].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Fullstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_BwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_Resolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_FwdResolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_TotalTilt] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_YawTilt] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallCenter] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.AF_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "04" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_BwdStroke] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_Resolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_FwdResolution] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_TotalTilt] = -1;
                    m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_YawTilt] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallCenter] = -1;
                    //m__G.sHistArray[lHindex, (int)Global.SpecItem.ZM_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "05" + m__G.sCIndex[ch].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 2);
                        Thread.Sleep(50);
                    }
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Fullstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Ratedstroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_BwdStroke] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_Resolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FullHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdSensitivity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdLinearity] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdHysteresis] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_FwdResolution] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_MaxCodeCurrent] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_TotalTilt] = -1;
                    m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_YawTilt] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallLinearity] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallCenter] = -1;
                    //m__G.sHistArray[lHindex + 1, (int)Global.SpecItem.ZM_HallSlope] = -1;
                    if (MachineType == (int)CSH030Ex.MachineType.Slave)
                    {
                        string tmpStr = "05" + m__G.sCIndex[ch + 1].ToString("D4") + "-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1";
                        m__G.fManage.PC2SendData("SRD", tmpStr, tmpStr.Length, 3);
                        Thread.Sleep(50);
                    }
                    break;

            }
        }

        public void ShowOperatorMode()
        {
            m_bAdmin = false;
            
            btnOperation.Text = "Supervisor";
            //btnInitAFDrvIC.Hide();
            btn_Vision.Hide();
            btnOperation.Hide();
            btn_Down.Hide();
            groupBox2.Hide();
            panel4.Hide();
            tb_CurrResult.Hide();
            panel1.Hide();
            lblMaker.Hide();
            lblRevisionNumber.Hide();
            tbBarcodeRevNo.Hide();
            lbxBarcodeMaker.Hide();
            lbxProductModel.Hide();

            btn_Vision         .Hide();
            btnOperation       .Hide();
            btnAutoLearn       .Hide();
            btnSaveScreenAdmin .Hide();
            cbPassword         .Hide();
            cb_ScreenCapture   .Hide();
            cbSaveRawData.Hide();
            cbDebugMode.Hide();
            cbSaveImage.Hide();
            lblSaveImage.Hide();
            txtSaveImage.Hide();
            //btnUserPIDUpdate.Hide();
            //btnHallCalPreset.Hide();
            //tbHallCalPreset.Hide();

            btnApplyTesterNo.Hide();
            tbTesterNumber.Hide();

            //tbPIDFile.Hide();
            groupBox1.Hide();

            SubForm_Show((int)Page.MANAGE);
            //if (!m__G.fManage.Text.Contains(m__G.mTesterID))
            //    m__G.fManage.Text += "    " + m__G.mTesterID;

        }

        public void ShowAdminMode()
        {
            P_Sub.Location = new Point(840, 48);
            P_Sub.Size = new Size(1920, 1040);

            panel4.Show();
            tb_CurrResult.Show();
            panel1.Show();
            //m__G.fManage.m__G = m__G;
            //m__G.fManage.MyOwner = this;
            m_bAdmin = true;
            btnOperation.Text = "Operator";
            btn_Down.Show();
            groupBox1.Show();

            btn_Vision         .Show();
            btnOperation       .Show();
            btnAutoLearn       .Show();
            btnSaveScreenAdmin .Show();
            cbPassword         .Show();
            cb_ScreenCapture   .Show();
            cbSaveRawData      .Show();
            cbDebugMode.Show();
            cbSaveImage.Show();
            lblSaveImage.Show();
            txtSaveImage.Show();
            int sPort = 0;
            int cPort = 0;
            m__G.fManage.GetPorts(ref sPort, ref cPort);
            tbHostPort.Text = sPort.ToString();
            tbAppPort.Text = cPort.ToString();
            groupBox2.Show();
            lblMaker.Show();
            lblRevisionNumber.Show();
            tbBarcodeRevNo.Show();
            lbxBarcodeMaker.Show();
            lbxProductModel.Show();
            btnApplyTesterNo.Show();
            tbTesterNumber.Show();

            //if (!this.Text.Contains(m__G.mTesterID))
            //    this.Text += "    " + m__G.mTesterID;

        }
        private void btnToAdmin_Click(object sender, EventArgs e)
        {
            //btnOperation_Click(sender, e);
        }

        private void btnToVision_Click(object sender, EventArgs e)
        {
            m__G.fVision.ShowDialog();
        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////
        /// Scoope Mode 추가에 따라 추가함.
        /// </summary>
        private void opInitDataGridView2()
        {
            int i = 0;
            panel5.Controls.Add(dataGridView2);

            this.dataGridView2.ColumnCount = 5;
            this.dataGridView2.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (i = 0; i < this.dataGridView2.ColumnCount; i++)
            {
                this.dataGridView2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dataGridView2.RowHeadersVisible = false;
            this.dataGridView2.BackgroundColor = Color.LightGray;


            //// Column
            this.dataGridView2.Columns[0].Name = "Class";
            this.dataGridView2.Columns[1].Name = "Condition Item";
            this.dataGridView2.Columns[2].Name = "Value";
            this.dataGridView2.Columns[3].Name = "Unit";
            this.dataGridView2.Columns[4].Name = "En/Dis";

            this.dataGridView2.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView2.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridView2.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridView2.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridView2.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.dataGridView2.Columns[0].Width = 100;
            this.dataGridView2.Columns[1].Width = 170;
            this.dataGridView2.Columns[2].Width = 62;
            this.dataGridView2.Columns[3].Width = 66;
            this.dataGridView2.Columns[4].Width = 75;

            this.dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView2.ColumnHeadersHeight = 22;

            int effRowNum = 0;
            bool bColorChange = true;
            for (i = 0; i < m__G.sNUM_TESTCONDITION; i++)
            {

                if (m__G.mTestCondition[i, 4].Contains('t') || m__G.mTestCondition[i, 4].Contains('T'))
                {
                    this.dataGridView2.Rows.Add(m__G.mTestCondition[i, 0], m__G.mTestCondition[i, 1], m__G.mTestCondition[i, 2], m__G.mTestCondition[i, 3], m__G.mTestCondition[i, 4]);
                    if (m__G.mTestCondition[i, 0].Length > 0)
                        bColorChange = !bColorChange;

                    if (bColorChange)
                    {
                        this.dataGridView2[0, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView2[1, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView2[3, effRowNum].Style.BackColor = Color.Lavender;
                    }
                    else
                    {
                        this.dataGridView2[0, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView2[1, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView2[3, effRowNum].Style.BackColor = Color.White;
                    }

                    m__G.mTestConditionToGrid[i] = effRowNum;
                    effRowNum++;
                }
            }
            //m__G.wrSytemLog("opInitDataGridView2 A");
            this.dataGridView2.Rows.Add("", "", "", "", "");
            //m__G.wrSytemLog("opInitDataGridView2 B");

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView2.Rows[i].Height = 16;
                this.dataGridView2.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView2[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView2[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView2[3, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }
            this.dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            this.dataGridView2.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            for (int colum = 2; colum < this.dataGridView2.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < effRowNum; row++)
                {
                    this.dataGridView2[colum, row].Style.BackColor = Color.LightGray;
                    this.dataGridView2.ReadOnly = true;
                }
            }
        }
        private void btnNVMwrite_Click(object sender, EventArgs e)
        {
            // 20170616 NVM WRite
            //byte[] buffer = new byte[1];
            //buffer[0] = 1;
            //m__G.fGraph.mDriverIC.WriteToSlave( ch, 3, buffer);
            string sDir = m__G.m_RootDirectory + "\\Result\\CreateDir";

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

        }

        private void btnNVMread_Click(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            if (!m__G.fGraph.mDriverIC.InitI2CnGPIO(m__G.sRecipe.iI2Cclock, m__G.mCamCount, m__G.mChannelCount)) return;
        }


        private void btnNVMerase_Click(object sender, EventArgs e)
        {
            // 20170616 NVM Erase
            //byte[] buffer = new byte[1];
            //buffer[0] = 1;
            //int memAddr = Convert.ToInt32("3500", 16);

        }

      

        private void SetPrevFWupdateFile(int type = 0)
        {
            string FilePath = m__G.m_RootDirectory + "\\DoNotTouch\\" + "FWupdateFileName.txt";

            StreamWriter sr = new StreamWriter(FilePath);
            sr.WriteLine(UpdateFWFile); ; //  32K FW File Name
            sr.WriteLine(mBU252FWfile); //  FW File Name
            sr.WriteLine(mBU252Calfile); //  Cal File Name
            sr.Close();
        }
       

        private void P_Sub_Paint(object sender, PaintEventArgs e)
        {

        }

        //public void ApplyCutValue(int ch)
        //{
        //    int lCutData = 0;
        //    int lresData = 0;
        //    //lCutData = Convert.ToInt16( this.dataGridView2[2, 7].Value.ToString());
        //    lresData = m__G.sRecipe.iFwdDrvCodeStart + lCutData;
        //                                this.dataGridView2[2, 9].Value = lresData.ToString();
        //    m__G.sRecipe.iFwdDrvCodeStart = lresData;//_XCodeMin

        //    //lCutData = Convert.ToInt16( this.dataGridView2[2, 8].Value.ToString());
        //    lresData = m__G.sRecipe.iFwdDrvCodeEnd - lCutData;
        //                                this.dataGridView2[2,10].Value = lresData.ToString();
        //    m__G.sRecipe.iFwdDrvCodeEnd = lresData;//_XCodeMax

        //    //lCutData = Convert.ToInt16( this.dataGridView2[2, 13].Value.ToString());
        //    lresData = m__G.sRecipe.iBwdDrvCodeStart + lCutData;
        //                                this.dataGridView2[2, 15].Value = lresData.ToString();
        //    m__G.sRecipe.iBwdDrvCodeStart = lresData;//_YCodeMin

        //    //lCutData = Convert.ToInt16( this.dataGridView2[2, 14].Value.ToString());
        //    lresData = m__G.sRecipe.iBwdDrvCodeEnd - lCutData;
        //                                this.dataGridView2[2, 16].Value = lresData.ToString();
        //    m__G.sRecipe.iBwdDrvCodeEnd = lresData;//_YCodeMax

        //    m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.AFDrv_Min] = m__G.sRecipe.iFwdDrvCodeStart;
        //    m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.AFDrv_Max] = m__G.sRecipe.iFwdDrvCodeEnd;
        //    m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.ZMDrv_Min] = m__G.sRecipe.iBwdDrvCodeStart;
        //    m__G.sHistArray[m__G.sCIndex[ch], (int)Global.SpecItem.ZMDrv_Max] = m__G.sRecipe.iBwdDrvCodeEnd;
        //}

        private void cb_ScreenCapture_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_ScreenCapture.Checked) m__G.m_bScreenCapture = true;
            else m__G.m_bScreenCapture = false;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbSaveRawData_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSaveRawData.Checked) 
                m__G.m_bSaveRawData = true;
            else 
                m__G.m_bSaveRawData = false;
            
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bFinishLoadMain)
            {
                m__G.m_bPasswordOn = cbPassword.Checked;
                if (m__G.m_bPasswordOn)
                {
                    PassFailInfromation mPWdialog = null;
                    mPWdialog = new PassFailInfromation(true);

                    mPWdialog.m__G = m__G;
                    mPWdialog.SetDesktopLocation(100, 500);
                    mPWdialog.ShowDialog();
                }
            }
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            m__G.mFAL.Show();
            m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //mFAL.Size = new Size(1920, 1045);
            m__G.mFAL.Location = new Point(0, 0);
        }
        private void btnApplyTesterNo_Click(object sender, EventArgs e)
        {
            SendParameterState = string.Empty;
            SendParameterState = UpdateFWFile + "," + mUpdatePIDfile[0] + "," + mBU252FWfile + "," + mBU252Calfile;
            //SaveProductMaker();
            SaveModel();
            WriteCurrentCBstates();
            if (m__G.oCam[0].mFAL!=null)
                if (m__G.oCam[0].mFAL.mFZM!= null)
                {
                    m__G.m_bXTiltReverse = cbXTiltReverse.Checked;
                    m__G.m_bYTiltReverse = cbYTiltReverse.Checked;
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);
                }

            if (MachineType == (int)CSH030Ex.MachineType.Master)
            {
                m__G.fManage.PC2SendData("SCB", SendParameterState, SendParameterState.Length, 2);
                UIControltmr.Enabled = true;
                btnApplyTesterNo.Enabled = false;
            }
            try
            {
                m__G.mMonitoringTestSet = int.Parse(tbMonitoringTestSetCount.Text);
            }
            catch(Exception exs)
            {
                m__G.mMonitoringTestSet = 129;
                tbMonitoringTestSetCount.Text = "129";
            }
        }
        public void ReadLastSampleNumber()
        {
            if (IsAccessAbleFile(m__G.m_RootDirectory + "\\DoNotTouch\\LastDayResults.txt"))
            {
                StreamReader rr = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\LastDayResults.txt");
                string lall = rr.ReadToEnd();
                string[] lstr = lall.Split("\n".ToCharArray());

                rr.Close();

                m__G.sHistIndex = Convert.ToInt32(lstr[0]); //190128

                string timeStr = "";
                if (lstr.Length > 1)
                {
                    if (lstr[1].Length > 1)
                    {
                        //MessageBox.Show("|" + timeStr + "|");
                        timeStr = lstr[1].Substring(0, lstr[1].Length - 1);
                        m__G.mLastDateTime = DateTime.ParseExact(timeStr, "yy-MM-dd-HH-mm-ss", null);
                    }
                }
                m__G.mDailyTotalTested = 0;
                m__G.mDailyTotalFail = 0;
                DateTime lnow = DateTime.Now;
                if (lnow.DayOfYear == m__G.mLastDateTime.DayOfYear)
                {
                    if (lstr.Length > 2)
                    {
                        if (lstr[2].Length > 1)
                            m__G.mDailyTotalTested = Convert.ToInt32(lstr[2]);
                    }
                    if (lstr.Length > 3)
                    {
                        if (lstr[3].Length > 1)
                            m__G.mDailyTotalFail = Convert.ToInt32(lstr[3]);
                    }
                    for (int i = 4; i < (m__G.sNUM_TESTITEM + 4); i++)
                    {
                        if (lstr.Length > i)
                        {
                            if (lstr[i].Length > 1)
                                m__G.iFailCountPerItemAll[i - 4] = Convert.ToInt32(lstr[i]);
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
                else
                {
                    string bkFile = "LastDayResults" + m__G.mLastDateTime.ToString("yyMMdd") + ".txt";
                    if (File.Exists(bkFile))
                        File.Delete(bkFile);
                    File.Copy("LastDayResults.txt", bkFile);
                }
            }

            m__G.fManage.SetCurrentSampleNumber();
        }

        private void lbxBarcodeMaker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
            //SaveProductMaker();
        }
        public void SaveProductMaker()
        {
            Object select = lbxBarcodeMaker.Items[lbxBarcodeMaker.SelectedIndex];
            if (select.ToString().Contains("V"))
                m_BarcodeMaker = 'S';
            else
                m_BarcodeMaker = 'M';

            m__G.mTesterNumber = tbTesterNumber.Text;

            String lstr = tbBarcodeRevNo.Text;
            if (lstr != "")
                m_BarcodeRevNo = (byte)(lstr.ToCharArray()[0]);


            string lstr1 = tbProductLine.Text;
            if (lstr1 != "")
                m_ProductLine = (byte)(lstr1.ToCharArray()[0]);
            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "MakerBarcodeRev.txt";

            StreamWriter wr = new StreamWriter(strFileName);
            wr.WriteLine(lbxBarcodeMaker.SelectedIndex.ToString());
            wr.WriteLine(((char)m_BarcodeRevNo).ToString());
            wr.WriteLine(((char)m_ProductLine).ToString());
            wr.WriteLine(lbxPrismSupplier.SelectedIndex.ToString());
            wr.WriteLine(lbxProductModel.SelectedIndex.ToString());
            wr.Close();
            if (lbxPrismSupplier.SelectedIndex == 0) m_PrismSupplier = 1;
            else m_PrismSupplier = 2;
            m_ICCheck = (byte)lbxProductModel.SelectedIndex;

            strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TesterNumber.txt";
            wr = new StreamWriter(strFileName);
            wr.WriteLine(m__G.mTesterNumber.ToString());
            wr.Close();
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                SendParameterState = SendParameterState + "," + lbxBarcodeMaker.SelectedIndex + "," + (char)m_BarcodeRevNo + "," + (char)m_ProductLine + "," + lbxPrismSupplier.SelectedIndex + "," + m__G.mTesterNumber + "," + lbxProductModel.SelectedIndex;

        }
        public void ReadProductMaker()
        {
            String strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "MakerBarcodeRev.txt";
            if (File.Exists(strFileName))
            {
                StreamReader rr = new StreamReader(strFileName);
                string tstr = rr.ReadToEnd();
                rr.Close();
                string[] lstr = tstr.Split("\n".ToCharArray());

                //MessageBox.Show("lstr.Length = " + lstr.Length.ToString());

                lbxBarcodeMaker.SelectedIndex = Convert.ToInt32(lstr[0]);
                if (lbxBarcodeMaker.SelectedIndex == 0)
                    m_BarcodeMaker = 'M';
                else
                    m_BarcodeMaker = 'S';

                if (lstr.Length > 1)
                {
                    //MessageBox.Show("lstr[1].Length = " + lstr[1].Length.ToString());
                    if (lstr[1].Length > 0)
                    {
                        m_BarcodeRevNo = (byte)(lstr[1].ToCharArray()[0]);
                        tbBarcodeRevNo.Text = ((char)m_BarcodeRevNo).ToString();
                    }
                    else
                    {

                        m_BarcodeRevNo = (byte)'A';
                        tbBarcodeRevNo.Text = ((char)m_BarcodeRevNo).ToString();
                    }

                }
                if (lstr.Length > 2)
                {
                    //MessageBox.Show("lstr[2].Length = " + lstr[2].Length.ToString());
                    if (lstr[2].Length > 0)
                    {
                        m_ProductLine = (byte)(lstr[2].ToCharArray()[0]);
                        tbProductLine.Text = ((char)m_ProductLine).ToString();
                    }
                    else
                    {
                        m_ProductLine = 0;
                        tbProductLine.Text = ((char)m_ProductLine).ToString();
                    }
                }
                if (lstr.Length > 3)
                {
                    //MessageBox.Show("lstr[3].Length = " + lstr[3].Length.ToString());
                    if (lstr[3].Length < 2)
                        lbxPrismSupplier.SelectedIndex = 0;
                    else
                        lbxPrismSupplier.SelectedIndex = Convert.ToInt32(lstr[3]);

                    if (lbxPrismSupplier.SelectedIndex == 0)
                        m_PrismSupplier = 1;
                    else
                        m_PrismSupplier = 2;
                }
                if (lstr.Length > 4)
                {
                    //MessageBox.Show("lstr[3].Length = " + lstr[3].Length.ToString());
                    if (lstr[4].Length < 2)
                        lbxProductModel.SelectedIndex = 0;
                    else
                        lbxProductModel.SelectedIndex = Convert.ToInt32(lstr[4]);

                    m_ICCheck = (byte)lbxProductModel.SelectedIndex;
                }
            }
            //MessageBox.Show("___AAA");
            strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "TesterNumber.txt";
            if (File.Exists(strFileName))
            {
                StreamReader rr = new StreamReader(strFileName);
                string tstr = rr.ReadLine();
                rr.Close();
                m__G.mTesterNumber = tstr;
                tbTesterNumber.Text = m__G.mTesterNumber.ToString();
            }
        }
        public void ReadModel()
        {
            string strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "Model.txt";
            if (!File.Exists(strFileName)) //Default
            {
                StreamWriter wr = new StreamWriter(strFileName);
                wr.WriteLine("1");
                wr.WriteLine("New-A");
                wr.Close();
            }
            if (File.Exists(strFileName))
            {
                StreamReader rr = new StreamReader(strFileName);
                List<string> line = new List<string>();
                while(true)
                {
                    string rd = rr.ReadLine();
                    if (rd == null) break;
                    line.Add(rd);
                }
                rr.Close();

                if (line.Count < 1)
                {
                    StreamWriter wr = new StreamWriter(strFileName);
                    wr.WriteLine("1");
                    wr.WriteLine("New-A");
                    wr.Close();
                    ReadModel();
                }

                lbxProductModel.Items.Clear();
                for (int i = 1; i < line.Count; i++)
                {
                    lbxProductModel.Items.Add(line[i]);
                }
                lbxProductModel.SelectedIndex = int.Parse(line[0]);
                m__G.sModelName = line[lbxProductModel.SelectedIndex + 1];
            }
        }
        public void SaveModel()
        {
            string strFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "Model.txt";
            if (!File.Exists(strFileName)) //Default
            {
                StreamWriter wr = new StreamWriter(strFileName);
                wr.WriteLine("1");
                wr.WriteLine("New 1");
                wr.WriteLine("");
                wr.Close();
                ReadModel();
            }
            if (File.Exists(strFileName))
            {
                List<string> line = new List<string>();
                line.Add(lbxProductModel.SelectedIndex.ToString());

                for (int i = 0; i < lbxProductModel.Items.Count; i++)
                {
                    line.Add(lbxProductModel.Items[i].ToString());
                }

                StreamWriter wr = new StreamWriter(strFileName);
                for (int i = 0; i < line.Count; i++)
                {
                    wr.WriteLine(line[i]);
                }
                wr.Close();
                m__G.sModelName = line[lbxProductModel.SelectedIndex + 1];
            }
        }
        private void cbVietnamese_CheckedChanged(object sender, EventArgs e)
        {
            //m__G.mbVet = cbVietnamese.Checked;
            if (m__G.mbVet)
            {
                ToViet(true);
            }
            else
            {
                ToViet(false);
            }
            m__G.fManage.ToViet(m__G.mbVet);
            //m__G.fStat.ToViet(m__G.mbVet);
            m__G.fVision.ToViet(m__G.mbVet);
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }
        public void ToViet(bool IsToViet = true)
        {
            if (IsToViet)
            {
                btnOpen.Text = "mở";
                btnSave.Text = "Lưu tệp";
                btnSaveAs.Text = "Lưu tệp as";
                btn_OpenSpecFile.Text = "mở thiên vị";
                btn_SaveSpecFile.Text = "Lưu tệp thiên vị";
                btn_SaveAsSpecFile.Text = "Lưu tệp as thiên vị";
                tbActionList.Text = "hoạt động sổ";
                tbToDoList.Text = "Làm gì đây";

                btnOperation.Text = "nhà điều hành Mode";
                btn_Vision.Text = "cài đặt máy ảnh";
                btnApplyTesterNo.Text = "phóng tác Máy móc con số";
                //btnUserPIDUpdate.Text = "Cài đặt PID Update File";
                lblMaker.Text = "người tạo ra";
                lblRevisionNumber.Text = "sửa đổi No.";
                cb_ScreenCapture.Text = "Lưu Screen";
                cbSaveRawData.Text = "LưuRaw Data";
                cb_Edit.Text = "sửa đổi";
                cb_EditSpec.Text = "sửa đổi thiên vị";
                cbYTiltReverse.Text = "Y sự đảo ngược";

            }
            else
            {
                btnOpen.Text = "Open";
                btnSave.Text = "Save";
                btnSaveAs.Text = "Save as";
                btn_OpenSpecFile.Text = "Open";
                btn_SaveSpecFile.Text = "Save";
                btn_SaveAsSpecFile.Text = "Save As";
                tbActionList.Text = "Action List";
                tbToDoList.Text = "To Do List";
                btnOperation.Text = "Operator";
                btn_Vision.Text = "Vision";
                btnApplyTesterNo.Text = "Apply Tester No.";
                //btnUserPIDUpdate.Text = "PID Update File";
                lblMaker.Text = "Maker";
                lblRevisionNumber.Text = "Revision No.";
                cb_ScreenCapture.Text = "Save Screen";
                cbSaveRawData.Text = "Save Raw Data";
                cb_Edit.Text = "Edit";
                cb_EditSpec.Text = "Edit";
                cbYTiltReverse.Text = "Y Direction Reversal";
            }
        }

        private void cbPitchDirReverse_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            // m__G.m_bYTiltReverse = cbYTiltReverse.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbBarcodeOverwriteDisable_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            //m__G.m_bBarcodeOverwriteDisable = cbBarcodeOverwriteDisable.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbSkipVisionCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            m__G.m_bCalibrationModel = cbCalibrationModel.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void btnSaveScreenAdmin_Click(object sender, EventArgs e)
        {
            SaveScreenShot("Admin");
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

        private void cbHideYieldGraph_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            m__G.m_bHideAllGraph = cbHideAllGraph.Checked;
            m__G.fManage.CalcShowYield();
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbPhaseInterpolation_CheckedChanged(object sender, EventArgs e)
        {
            //if (m__G == null) return;
            //m__G.m_bPhaseInterpolation = cbPhaseInterpolation.Checked;
        }


        #region 자세차 통신 컨트롤 이벤트
        private void btnSendTCP_Click(object sender, EventArgs e)
        {
            if (tbTCPSent.Text != null)
            {

                m__G.fManage.ClientMain(tbTCPSent.Text);
            }

        }

        private void btnSaveNStartLan_Click(object sender, EventArgs e)
        {
            //IPAddress[] MY_IP;
            //MY_IP = Dns.GetHostAddresses(Dns.GetHostName());
            //labelMyIP.Text = MY_IP[1].ToString();
            //m__G.fManage.StopRunning();
            //Thread.Sleep(10);
            //m__G.fManage.SetPorts(Convert.ToInt32(tbHostPort.Text), Convert.ToInt32(tbAppPort.Text));
            //if (!m__G.fManage.IsRunning())
            //{
            //    Thread ThreadServer;
            //    ThreadServer = new Thread(() => m__G.fManage.ServerMain());
            //    ThreadServer.Start();
            //}
        }
        public void SetTCPReceived(string text)
        {
          //  tbTCPReceived.Text = text;
        }

        #endregion

        private void cbYawDirReverse_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            // m__G.m_bXTiltReverse = cbXTiltReverse.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void lbxProductModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;

        }
        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnSave.Enabled = true;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btn_SaveSpecFile.Enabled = true;
        }

        private void lbxPrismSupplier_SelectedIndexChanged(object sender, EventArgs e)
        { //Maker prism 같이 묶어놈
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void tbProductLine_TextChanged(object sender, EventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void tbBarcodeRevNo_TextChanged(object sender, EventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void tbTesterNumber_TextChanged(object sender, EventArgs e)
        {
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        // Rumba S6 
        public enum S6_DEFINE
        {
            TIMEOUT_FIRMWAREUPDATE = 5000,
            //OpimizeLevel = -Os, /* TJ Soft Optional Define OpimizeLevel (-Os,-O3,-O2,-O1,-Ox) */
        }

        public struct __FWUP_NEW_Data
        {
            public UInt32 EndIndexDataCount;
            public UInt16 DataSize;
            public byte[] Data;
            public UInt16 CheckSum;
            public UInt16 ErrorCode;
        }

        public enum __FWUPERR
        {
            FWUPERR_DATASEQ0 = 0x0080,
            FWUPERR_CHKSUM0 = 0x0040,
            FWUPERR_VERIFY0 = 0x0020,
            FWUPERR_WRITE0 = 0x0010,
            FWUPERR_ERASE0 = 0x0004,
            FWUPERR_NOROM = 0x0001,
            FWUPERR_NO_ERR = 0x0000
        }

        public struct __Common_Calibration_Parameter
        {
            volatile public Byte Mode;
            volatile public Byte Porarity;
            volatile public Byte HCRetry;
            volatile public UInt16 HDRange;
        }

        public struct __HallSensor_Calibration_Parameter
        {
            volatile public UInt16 H_MaxLMT;
            volatile public UInt16 H_MinLMT;
            volatile public UInt16 H_Max;
            volatile public UInt16 H_Min;
            volatile public Byte HC_MaxLMT;
            volatile public Byte HC_MinLMT;
            volatile public UInt16 MECHACENTER;
            volatile public Byte H_Gain;
            volatile public Byte H_Offset;
            volatile public Byte H_Bais;
        }

        public struct __SEM_AFHallSensor_Calibration_Parameter
        {
            volatile public Byte HAF_Offset;
            volatile public Byte HZM_Offset;
            volatile public Byte HAF_Bais;
            volatile public Byte HZM_Bais;
            volatile public UInt16 HAF_Max;
            volatile public UInt16 HZM_Max;
            volatile public UInt16 HAF_Min;
            volatile public UInt16 HZM_Min;
            volatile public UInt16 HAF_Mid;
            volatile public UInt16 HZM_Mid;
            volatile public UInt16 HAF_EPA_Max;
            volatile public UInt16 HZM_EPA_Max;
            volatile public UInt16 HAF_EPA_Min;
            volatile public UInt16 HZM_EPA_Min;
            volatile public UInt16 HAF_EPA_Mid;
            volatile public UInt16 HZM_EPA_Mid;
        }

        public struct __SEM_OISHallSensor_Calibration_Parameter
        {
            volatile public UInt16 HX_MaxLMT;
            volatile public UInt16 HY_MaxLMT;
            volatile public UInt16 HX_MinLMT;
            volatile public UInt16 HY_MinLMT;
            volatile public UInt16 HX_Max;
            volatile public UInt16 HY_Max;
            volatile public UInt16 HX_Min;
            volatile public UInt16 HY_Min;
            volatile public UInt16 HX_Mid;
            volatile public UInt16 HY_Mid;
            volatile public Byte HCX_MaxLMT;
            volatile public Byte HCY_MaxLMT;
            volatile public Byte HCX_MinLMT;
            volatile public Byte HCY_MinLMT;
            volatile public Byte HX_Gain;
            volatile public Byte HY_Gain;
            volatile public Byte HX_Offset;
            volatile public Byte HY_Offset;
            volatile public Byte HX_Bais;
            volatile public Byte HY_Bais;
        }

        public struct __PIDPrameter
        {
            volatile public UInt32 Kp;
            volatile public UInt32 Ki;
            volatile public UInt32 Kd;
            volatile public UInt32 K9b;
            volatile public UInt32 CutOffFreq;
            volatile public UInt32 SampleFreq;
        }

        public struct __PhaseMargin_parameter
        {
            volatile public Byte LGFREQ0;
            volatile public Byte LGNUM0;
            volatile public Byte LGSKIPNUM0;
            volatile public UInt16 LGAMP0;
            volatile public UInt16 MC_ADDR;
            volatile public UInt16 LGMCRES0;
            volatile public UInt16 LGMCRES1;
            volatile public UInt16 LGMCRES2;
            volatile public UInt16 LGMCRES3;
        }

        public struct PhaseMargin_parameter
        {
            public Byte LGFREQ0, LGNUM0, LGSKIPNUM0, LGCTRL_VAL;
            public UInt16 LGAMP0, MC_ADDR, LGMCRES0, LGMCRES1, LGMCRES2, LGMCRES3;
        }

        //__FWUP_NEW_Data FWUP_NEW_Data;

        //__Common_Calibration_Parameter Recv_CommonCal;
        __HallSensor_Calibration_Parameter[] Recv_Cal = new __HallSensor_Calibration_Parameter[2];
        __SEM_OISHallSensor_Calibration_Parameter[] SEM_OISRead_Cal = new __SEM_OISHallSensor_Calibration_Parameter[2];
        __SEM_AFHallSensor_Calibration_Parameter[] SEM_AFRead_Cal = new __SEM_AFHallSensor_Calibration_Parameter[2];
        __PIDPrameter[] Recv_PID = new __PIDPrameter[2];

        //SByte ErrorCode = 0;
        //Byte RumbaS6_ch = 2;
        public UInt16[] ois_cur_code = { 0, 0 };
        public UInt16[] ois_min_code = { 0, 0 };
        public UInt16[] ois_max_code = { 0, 0 };
        public Int16[] ois_ctr_code = { 0, 0 };
        public Int16[] ois_tag_code = { 0, 0 };


        public void MemoryCopy(byte[] Value, byte[] Data, UInt16 Length, int iStartIndex)
        {
            int i = 0;
            for (i = 0; i < Length; i++) Value[i] = Data[iStartIndex + i];
        }

        public void FWUP_ErrorPrint(int ch, UInt16 ErrorCode)
        {
            if (ErrorCode == (UInt16)(__FWUPERR.FWUPERR_NO_ERR)) m__G.fManage.AddOperatorLog(ch, "Success : 0x0000 - NO ERROR", false);
            else
            {
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_NOROM)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0001 - NO ROM", false);
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_ERASE0)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0004 - ERASE", false);
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_WRITE0)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0010 - E_WRITE", false);
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_VERIFY0)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0020 - E_VERIFY", false);
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_CHKSUM0)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0040 - E_CHECKSUM", false);
                if ((ErrorCode & (UInt16)(__FWUPERR.FWUPERR_DATASEQ0)) > 0)
                    m__G.fManage.AddOperatorLog(ch, "ErrorCode : 0x0080 - E_DATASEQ", false);
            }
        }

        public UInt16 CaluCheckSum_WORD_LitleEndian(Byte[] Data, UInt16 Length)
        {
            int i = 0;
            UInt16 Temp = 0;
            UInt16 TempCheckSum = 0;

            for (i = 0; i < Length; i++)
            {
                if (i % 2 == 0) Temp = (UInt16)(Data[i] & 0xff);
                else if (i % 2 == 1)
                {
                    Temp |= (UInt16)((Data[i] << 8) & 0xFF00);
                    //Temp |= ((UInt16)(Data[i] << 8) & 0xFF00);
                    TempCheckSum += Temp;
                }
            }
            return TempCheckSum;
        }


        public void GetOISdata(Byte[] pData)
        // Use over 60 array
        {
            //ch로 연결해야함... 테스트할때 고민해서 바꺼야함. 아직 구조를 다 이해못함...
            pData[60] = Recv_Cal[0].H_Bais;                     // ois x hall bias
            pData[61] = Recv_Cal[1].H_Bais;                     // ois y hall bias
            pData[62] = Recv_Cal[0].H_Offset;                   // ois x hall offset
            pData[63] = Recv_Cal[1].H_Offset;                   // ois y hall offset
        }

        public void Hallsensor_Calibration_ParameterSet(int ch)
        {
            m__G.fManage.AddOperatorLog(ch, "Skip hall cal option write", false);
        }	// 161006 for rumba s6 update



        private void button1_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.DriverICPower(0, false);
            m__G.fGraph.mDriverIC.OISDriverICReset(1, true);
            Thread.Sleep(5); // 5ms 이상 100%
            m__G.fGraph.mDriverIC.DriverICPower(1, true);
            Thread.Sleep(5); // 5ms 이상 100%
            m__G.fGraph.mDriverIC.OISDriverICReset(0, false);
            Thread.Sleep(150); // 150 이상 100%
        }

        
        private void cbReverseAF_CheckedChanged(object sender, EventArgs e)
        {
            if (m__G == null) return;
            m__G.m_bTXDirReverse = cbReverseTX.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            if (listBox.SelectedItem == null) return;

            string action = listBox.SelectedItem.ToString();

            int i = 0;
            int effRowNum = 0;
            bool bColorChange = true;
            for (i = 0; i < m__G.sNUM_TESTCONDITION; i++)
            {
                this.dataGridView2[4, i].Style.BackColor = Color.White;
                if (m__G.mTestCondition[i, 4].Contains('t') || m__G.mTestCondition[i, 4].Contains('T'))
                {
                    if (m__G.mTestCondition[i, 0].Length > 0)
                        bColorChange = !bColorChange;

                    if (bColorChange)
                    {
                        this.dataGridView2[0, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView2[1, effRowNum].Style.BackColor = Color.Lavender;
                        this.dataGridView2[3, effRowNum].Style.BackColor = Color.Lavender;
                    }
                    else
                    {
                        this.dataGridView2[0, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView2[1, effRowNum].Style.BackColor = Color.White;
                        this.dataGridView2[3, effRowNum].Style.BackColor = Color.White;
                    }

                    m__G.mTestConditionToGrid[i] = effRowNum;
                    effRowNum++;
                }
            }

            this.dataGridView2.Rows[i].DefaultCellStyle.ForeColor = Color.White;

            for (int colum = 2; colum < this.dataGridView2.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < effRowNum; row++)
                {
                    this.dataGridView2[colum, row].Style.BackColor = Color.LightGray;
                }
            }

            Color color = Color.Orange;
            switch (action)
            {
                case "Read FW Version SEM":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                        dataGridView2[k, (int)Global.RcpItem.iI2Cclock].Style.BackColor = color;
                        dataGridView2[k, (int)Global.RcpItem.iI2CslaveAddr].Style.BackColor = color;
                    }
                    break;
                case "Read AFZM Cal Verify":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                        dataGridView2[k, (int)Global.RcpItem.iI2Cclock].Style.BackColor = color;
                        dataGridView2[k, (int)Global.RcpItem.iI2CslaveAddr].Style.BackColor = color;
                    }
                    break;
                case "Open Loop Test SEM":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                    }
                    break;

                case "Run User Script":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                    }
                    break;
                case "FW Download SEM":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                        dataGridView2[k, (int)Global.RcpItem.iI2Cclock].Style.BackColor = color;
                        dataGridView2[k, (int)Global.RcpItem.iI2CslaveAddr].Style.BackColor = color;
                    }
                    break;
                case "Hall Calibration SEM":
                    for (int k = 0; k < dataGridView2.ColumnCount; k++)
                    {
                        dataGridView2[k, (int)Global.RcpItem.iGrabTimeLimit].Style.BackColor = color;
                    }
                    break;

                default:
                    break;
            }
        }

        private void btnAutoLearn_Click(object sender, EventArgs e)
        {
            if (m__G.mFAL == null)
            {
                return;
            }
            m__G.mFAL.Show();
            m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //mFAL.Size = new Size(1920, 1045);
            m__G.mFAL.Location = new Point(0, 0);
        }
        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if ( lbtn.Text.Contains("Open") || lbtn.Text.Contains("Save") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnDBP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }

        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Open") || lbtn.Text.Contains("Save") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnDBN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Open") || lbtn.Text.Contains("Save") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnDBP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }

        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Open") || lbtn.Text.Contains("Save") || lbtn.Text.Contains("Apply"))
                lbtn.BackgroundImage = Properties.Resources.BtnDBN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        private void btnOperation_Click(object sender, EventArgs e)
        {
            ShowOperatorMode();
        }

        public string mFile4ModelFileList = "ModelFileList.txt";
        private void button1_Click_1(object sender, EventArgs e)
        {
            //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));
            //string sFilePath = folder;
            string sFilePath = m__G.m_RootDirectory + "\\DoNotTouch\\Training";
            if (!Directory.Exists(sFilePath))
                Directory.CreateDirectory(sFilePath);

            List<string> lPreparedModelFile = new List<string>();

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "xml";
            openFile.InitialDirectory = sFilePath;
            openFile.Multiselect = true;
            openFile.Filter = "XML(*.xml)|*.xml";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                lbxModelFiles.Items.Clear();
                StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList);
                foreach (string filename in openFile.FileNames)
                {
                    lPreparedModelFile.Add(filename);
                    lbxModelFiles.Items.Add(filename);
                    wr.WriteLine(filename);
                }
                wr.Close();
                m__G.fVision.SetModelFileList(lPreparedModelFile.ToArray());
                m__G.fVision.TransferModelFileList();
            }
            else
                return;

        }

        bool bLoadLastModelFile = true;
        public bool LoadLastModelFileList()
        {
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList))
                return false;

            StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList);
            string allstring = rd.ReadToEnd();
            rd.Close();
            string[] eachLine = allstring.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> lPreparedModelFile = new List<string>();
            foreach (string filename in eachLine)
            {
                lPreparedModelFile.Add(filename);
                lbxModelFiles.Items.Add(filename);
            }
            lbxModelFiles.SelectedIndex = eachLine.Length - 1;
            m__G.fVision.SetModelFileList(lPreparedModelFile.ToArray());
            bLoadLastModelFile = false;
            return true;
        }

        private void lbxModelFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m__G == null)
                return;
            if (m__G.oCam[0] == null)
                return;
            if (m__G.oCam[0].mFAL == null)
                return;
            if (lbxModelFiles.SelectedItem == null)
                return;

            if (!m__G.mFAL.mFAutoLearnLoaded)
            {
                m__G.mFAL.Show();
                m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                //mFAL.Size = new Size(1920, 1045);
                m__G.mFAL.Location = new Point(0, 0);
                m__G.mFAL.Hide();
            }

            if (!bLoadLastModelFile)
            {
                string sFile = lbxModelFiles.SelectedItem.ToString();
                int lModelScale = m__G.oCam[0].mFAL.ExternalLoadFMIFile(sFile);
                if (m__G != null)
                    if (m__G.oCam[0] != null)
                        m__G.oCam[0].ResetModelScale(1.0 / lModelScale);

                lblDefaultModel.Text = sFile;
            }
        }

        private void lbxModelFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Delete )
            {
                //  해당항목 삭제
                int selectedIndex = lbxModelFiles.SelectedIndex;
                lbxModelFiles.Items.RemoveAt(lbxModelFiles.SelectedIndex);
                if (selectedIndex > 0)
                    lbxModelFiles.SelectedIndex = selectedIndex - 1;
                else if (lbxModelFiles.Items.Count > 0)
                    lbxModelFiles.SelectedIndex = lbxModelFiles.Items.Count-1;

                //  리스트파일 업데이트
                StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList);
                foreach (string filename in lbxModelFiles.Items)
                    wr.WriteLine(filename);

                wr.Close();
            }
        }

        [DllImport("User32.dll")]   // =>  System.Runtime.InteropServices.[DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]   // =>  System.Runtime.InteropServices.[DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int state);
        [DllImport("User32.dll")]   // =>  System.Runtime.InteropServices.[DllImport("User32.dll")]
        public static extern bool IsIconic(IntPtr handle);
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            //[MarshalAs(UnmanagedType.LPStr)]
            public IntPtr lpData;
        }

        COPYDATASTRUCT cds = new COPYDATASTRUCT();
        public const int WM_COPYDATA = 0x004A;

        private void button2_Click(object sender, EventArgs e)
        {
            string sengMsg = "[" + m__G.mTesterID + "]";

            sengMsg += "#" + m__G.oCam[0].mFAL.mLastFMIfile;
            sengMsg += "#" + m__G.m_RootDirectory + "\\DoNotTouch\\" +  mFile4ModelFileList;
            sengMsg += "#" + m__G.m_RootDirectory + "\\DoNotTouch\\" +  "OpticsConfig.txt";
            sengMsg += "#" + m__G.m_RootDirectory + "\\DoNotTouch\\" +  "LastDesignInfo.png";
            sengMsg += "#" + m__G.m_RootDirectory + "\\DoNotTouch\\" +  "LastFMIfile.txt";
            sengMsg += "#" + m__G.oCam[0].mFAL.mLastSchematicFile;
            SendCopyDataMessage(sengMsg);

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
            IntPtr hWndShell = Process.GetProcessesByName("ShellTest")[0].MainWindowHandle;
            if (hWndShell != null)
            {
                // Close Window
                SendMessage(hWndShell, WM_COPYDATA, (IntPtr)0, iPtr);
            }
            Marshal.FreeCoTaskMem(cds.lpData);
            Marshal.FreeCoTaskMem(iPtr);
        }
        //  WM_COPYDATA 메세지 받기
        protected override void WndProc(ref Message m)
        {
                //  Socket 프로그램이 WM_COPYDATA 메세지를 보내면 아래와 같이 처리해줘야 한다.
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
                    string recievedText = new string(myString);

                    DecodeCopyDataMsg(recievedText);
                    //tbReceived.Text = returnText;
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        public bool DecodeCopyDataMsg(string cpMsg)
        {
            string tmpFileName = "";
            cpMsg = cpMsg.Substring(0,cpMsg.LastIndexOf("\0"));
            if (cpMsg.Contains("$") || !cpMsg.Contains("!"))
            {
                ////////////////////////////////////////////////////////////////////////////////
                //  Client App 의 요청을 Decoding 하여 각 요청에 맞는 회신 보내기
                ////////////////////////////////////////////////////////////////////////////////
                string[] rqstStr = cpMsg.Split("$".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                switch (rqstStr[0])
                {
                    case "TstID":
                        //  Tester ID 를 Return 해주면 됨.
                        //  m__G.mTesterID 를  return 해줄 것
                        SendCopyDataMessage("TstID@" + m__G.mTesterID);
                        break;
                    case "MOTID":
                        //  MOT ID 를 Return 해주면 됨.
                        // m__G.mMOTID 를 Return 해줄 것
                        SendCopyDataMessage("MOTID@" + m__G.mMOTID);
                        break;
                    case "WhRUD":
                        //  지금 뭐하는 중인지를 Return 해주면 됨.
                        //  m__G.DoingStatus 를 Return 해주면 됨.
                        SendCopyDataMessage("WhRUD@" + m__G.mDoingStatus);
                        break;
                    case "Fpath":
                        //  각각의 파일의 Full Path 를 Return 해주면 됨.
                        switch (rqstStr[1])
                        {
                            case "OpticsConfig":
                                tmpFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + "\\OpticsConfig.txt";
                                break;
                            case "ModelFileList":
                                tmpFileName = m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList;
                                break;
                            case "ModelFile":
                                tmpFileName = m__G.oCam[0].mFAL.mLastFMIfile;
                                break;
                            case "SchematicFile":
                                tmpFileName = m__G.oCam[0].mFAL.mLastSchematicFile;
                                break;
                            case "LastDesignInfoFile":
                                tmpFileName = m__G.m_RootDirectory + "\\DoNotTouch\\LastDesignInfo.png";
                                break;
                            case "RecipeFile":
                                tmpFileName = m__G.m_RootDirectory + "\\Recipe\\" + m__G.sRecipe.sRecipeName;
                                break;
                            case "DcfFile":
                                tmpFileName = m__G.mDcfFilePathT + "#" + m__G.mDcfFilePathC;
                                break;
                            case "ScaleFile":
                                tmpFileName = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + m__G.mCamID0 + ".txt";
                                break;
                            default:
                                break;
                        }
                        if (!tmpFileName.Contains('#'))
                            if (!File.Exists(tmpFileName))
                                return false;

                        SendCopyDataMessage("Fpath@" + tmpFileName);
                        break;
                    case "ScrCp":
                        //  각각의 영상파일을 임시저장하고, Full Path 를 Return 해주면 됨.
                        switch (rqstStr[1])
                        {
                            case "Admin":
                                ShowAdminMode();
                                tmpFileName = SaveScreenShot("Admin");
                                break;
                            case "Operator":
                                ShowOperatorMode();
                                tmpFileName = m__G.fManage.SaveScreenShot("Operator");
                                break;
                            case "Vision":
                                ShowOperatorMode();
                                SubForm_Show((int)Page.VISION);
                                tmpFileName = m__G.fVision.SaveScreenShot("Vision");
                                break;
                            case "ModelConfig":
                                m__G.mFAL.Show();
                                m__G.mFAL.BringToFront();
                                Thread.Sleep(1000);
                                tmpFileName = m__G.fVision.SaveScreenShot("ModelConfig");
                                m__G.mFAL.Hide();
                                break;
                            case "MotSim":
                                break;
                            case "Grab":
                                //  한장 찍어서 LastGrab 에 저장되면 그것을 보낸다.
                                ShowOperatorMode();
                                SubForm_Show((int)Page.VISION);
                                tmpFileName = m__G.fVision.RemoteGrab();
                                break;
                            default:
                                break;
                        }
                        SendCopyDataMessage("ScrCp@" + tmpFileName);
                        break;
                    case "Exct":
                        //  각각의 영상파일을 임시저장하고, Full Path 를 Return 해주면 됨.
                        switch (rqstStr[1])
                        {
                            case "StartTest":
                                m__G.mDoingStatus = "Triggered Measure";
                                m__G.fManage.RepeatStartTest(m__G.sRecipe.iTriggeredGrabImageCount);
                                break;
                            case "StopTest":
                                m__G.fManage.SuddenStop();
                                break;
                            default:
                                break;
                        }
                        break;
                    case "TMode":
                        //  각각의 영상파일을 임시저장하고, Full Path 를 Return 해주면 됨.
                        switch (rqstStr[1])
                        {
                            case "Norm":
                                cbEulerRotation.Checked = false;
                                cbPrismCoordinateSystem.Checked = false;
                                break;
                            case "Fast":
                                cbEulerRotation.Checked = true;
                                break;
                            case "SuperFast":
                                cbEulerRotation.Checked = true;
                                cbPrismCoordinateSystem.Checked = true;
                                break;
                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }
            else if (cpMsg.Contains("!"))
            {
                ////////////////////////////////////////////////////////////////////////////////
                //  Client App 의 명령을 Decoding 하여 각 명령에 맞는 작업 수행하기
                ////////////////////////////////////////////////////////////////////////////////
                string[] rqstStr = cpMsg.Split("!".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string FilePath = rqstStr[1];
                switch (rqstStr[0])
                {
                    case "Updat":
                        //  파일명에 따라 필요한 작업을 각각 해줘야 한다.
                        // 기존에 활용했던 파일을 백업해야 한다.
                        //  기존파일에 대한 백업은 Client Socket App 에서 해준다.
                        //if ( File.Exists(FilePath))
                        //{
                        //    string nameBody = FilePath.Substring(0, FilePath.LastIndexOf("."));
                        //    string orgExtension = FilePath.Substring(FilePath.LastIndexOf(".")+1);
                        //    string bkFile = nameBody + "_bk" + orgExtension;
                        //    if ( File.Exists(bkFile) )
                        //    {
                        //        try
                        //        {
                        //            File.Delete(bkFile);
                        //        }
                        //        catch
                        //        {
                        //            return false;
                        //        }
                        //    }
                        //    File.Copy(FilePath, bkFile);
                        //}

                        if (FilePath.Contains("OpticsConfig"))
                        {
                            m__G.oCam[0].mFAL.LoadOpticsConfig();
                        }
                        else if (FilePath.Contains("ModelFileList"))
                        {
                            LoadLastModelFileList();
                        }
                        else if (FilePath.Contains("ModelFile"))
                        {
                            m__G.oCam[0].mFAL.ExternalLoadFMIFile(FilePath);
                        }
                        else if (FilePath.Contains("SchematicFile"))
                        {
                            m__G.oCam[0].mFAL.ExternalUpdateSchmatics(FilePath);
                        }
                        else if (FilePath.Contains("LastDesignInfo"))
                        {
                            ;// Nothing to do.
                        }
                        else if (FilePath.Contains("RecipeFile"))
                        {
                            bLoadRecipe(FilePath);
                        }
                        else if (FilePath.Contains("OrgRoi"))
                        {
                            ;
                        }
                        break;
                    case "Recvr":
                        string target = rqstStr[1];
                        //  target 명에 따라 필요한 작업을 각각 해줘야 한다.
                        // 이전에 활용했던 파일을 Current 파일로 변경하고 적용해야 한다.
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Task.Run(() => ShowMessageBox(cpMsg));

            }
            return true;
        }
        public void ShowMessageBox(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    MessageBox.Show(msg + " received.");
                });

            }
            else
            {
                MessageBox.Show(msg + " received.");
            }
        }

        private void F_Main_Shown(object sender, EventArgs e)
        {
        }

        private void cbEulerRotation_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
            {
                m__G.m_bEulerRotation = true;
                //  저장되어있는 Euler Angle 을 읽어들여서 EulerMatrix 를 설정하고, Validate 한다.
                //  저장되어있는 Euler Angle 이 없으면 Validate 만 한다.
                //  EulerMatrix 는 사용자에 의해 변경될 수 있고 변경될 때 저장된다.
                string eulerFile = m__G.m_RootDirectory + "\\DoNotTouch\\EulerAngle" + m__G.fVision.camID0 + ".txt";
                if (File.Exists(eulerFile))
                {
                    StreamReader rd = new StreamReader(eulerFile);
                    string fullstr = rd.ReadToEnd();
                    rd.Close();
                    string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    m__G.fVision.mPhiThetaPsi[0] = double.Parse(eachLine[0]);
                    m__G.fVision.mPhiThetaPsi[1] = double.Parse(eachLine[1]);
                    m__G.fVision.mPhiThetaPsi[2] = double.Parse(eachLine[2]);
                    m__G.oCam[0].mFAL.SetEulerAngle(m__G.fVision.mPhiThetaPsi);
                }
                m__G.oCam[0].mFAL.ValidateEulerRotation(true);
                m__G.fVision.UpdateEulerAngle();
            }
            else
            {
                m__G.m_bEulerRotation = false;
                m__G.oCam[0].mFAL.ValidateEulerRotation(false);
            }
        }

        private void cbSuperFastMode_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bPrismCS = true;
            else
                m__G.m_bPrismCS = false;
        }

        private void cbAutoLastFrame_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bAutoLastFrame = true;
            else
                m__G.m_bAutoLastFrame = false;
        }

        private void cbSaveLostTestSet_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bSaveLostTestSet = true;
            else
                m__G.m_bSaveLostTestSet = false;
        }

        private void cbNoHost_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bNoHostPC = true;
            else
                m__G.m_bNoHostPC = false;

            m__G.oCam[0].bNoHostPC = m__G.m_bNoHostPC;
        }

        private void cbReverseX_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (m__G == null) return;
            m__G.m_bXDirReverse = ch.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbReverseY_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (m__G == null) return;
            m__G.m_bYDirReverse = ch.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbReverseTY_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (m__G == null) return;
            m__G.m_bTYDirReverse = ch.Checked;
            if (MachineType == (int)CSH030Ex.MachineType.Master)
                btnApplyTesterNo.Enabled = true;
        }

        private void cbDebugMode_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bDebugMode = true;
            else
                m__G.m_bDebugMode = false;

        }

        private void cbSaveImage_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bSaveImage = true;
            else
                m__G.m_bSaveImage = false;

        }

        private void txtSaveImage_TextChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            try
            {
                m__G.m_SaveImageCount = int.Parse(ch.Text);
            }
            catch
            {

            }
        }

        private void chTestStage_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bTestStage = true;
            else
                m__G.m_bTestStage = false;
        }

        private void chSaveNGImage_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bSaveNgImage = true;
            else
                m__G.m_bSaveNgImage = false;
        }

        private void chOISOption_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bOISOption = true;
            else
                m__G.m_bOISOption = false;
        }

        private void chSaveFImage_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bSaveFImage = true;
            else
                m__G.m_bSaveFImage = false;
        }

        private void cbPseudoOMM_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ch = (sender as CheckBox);
            if (ch.Checked)
                m__G.m_bPseudoOMM = true;
            else
                m__G.m_bPseudoOMM = false;

            m__G.oCam[0].bPseudoOMM = m__G.m_bPseudoOMM;
        }
    }
}

