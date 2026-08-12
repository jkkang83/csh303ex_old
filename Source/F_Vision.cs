using Basler.Pylon;
using Dln;
using FAutoLearn;
using Matrox.MatroxImagingLibrary;
using MotorizedStage_SK_PI;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using OpenCvSharp.Flann;
//using OpenCvSharp;
using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
//using static alglib;
//using static alglib.apserv;
using static CSH030Ex.FManage;
using static CSH030Ex.FVision;
using static FAutoLearn.FAutoLearn;
using static FAutoLearn.FZMath;
using static MotorizedStage_SK_PI.F_Motion_SK_PI;
using static S2System.Vision.MILlib;
using Axis = MotorizedStage_SK_PI.Axis;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
//using MyEmailer;
using OpenCvSharp.Dnn;
using System.Runtime.InteropServices;
using Dln.Led;
using System.Security.Cryptography;
using System.Runtime.InteropServices.ComTypes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using System.Management;
using System.Globalization;
using System.Net.NetworkInformation;

namespace CSH030Ex
{
    public partial class FVision : Form
    {

        private Global m__G;
        private Camera[] BaslerCam = new Camera[2];
        public F_Main MyOwner = null;

        public int mTmpCount = 0;

        //public int m_TiltLUTcount = 100; //  Default Value
        //public double[] m_AFYawLUTL   = new double[100];
        //public double[] m_ZMYawLUTL   = new double[100];
        //public double[] m_AFPitchLUTL = new double[100];
        //public double[] m_ZMPitchLUTL = new double[100];
        //public double[] m_AFYawLUTR   = new double[100];
        //public double[] m_ZMYawLUTR   = new double[100];
        //public double[] m_AFPitchLUTR = new double[100];
        //public double[] m_ZMPitchLUTR = new double[100];

        private const int CAM1 = 0;
        private const int CAM2 = 1;
        private const int MODEL0 = 0;
        private const int MODEL1 = 1;
        private const int MODEL2 = 2;
        private const int MODEL3 = 3;
        private const int MODEL4 = 4;
        private const int MODEL5 = 5;
        private const int MODEL6 = 6;
        private const int MODEL7 = 7;
        private const int MODEL8 = 8;
        public string MarkPatternFile = "C90_140.bmp";
        //int m_replayIndex = 0;

        // 마크 위치 저장 변수
        //public Point[] m_iCam1_Mark_BoxP1 = new Point[6];
        //public Point[] m_iCam1_Mark_BoxP2 = new Point[6];

        //public Point m_iCam2_Mark1_BoxP1;
        //public Point m_iCam2_Mark1_BoxP2;
        //public Point m_iCam2_Mark2_BoxP1;
        //public Point m_iCam2_Mark2_BoxP2;
        int[] ROIVerticalRange = new int[6];
        //public double ZoomFactor = 1.8; //  960 x 360 => 1.8 배로 표시, 860 x 342 => 1.895 배 표시 
        public double ZoomFactor = 0.795;
        public bool bRefresh = false;
        public double[] mLEDcurrent = new double[4] { 0, 0, 0, 0 };   //  4.56 for 220usec exposure
        public bool[] m_bSettlingROI = new bool[2] { false, false };
        public int mOptThresh = 23;
        public int mStepResponseThresh = 29;    //  mStepResponseThresh = mOptThresh - 16;ㄹ
        bool[] m_IdleSearchROI = new bool[2];
        public int m_FocusedLED = 0;
        public bool m_bSaveOrgROI = false;
        public bool m_bAllLEDOn = false;

        public int m_TopViewThresh = 10;
        public int m_TopViewMarkMax = 30;
        public int m_SideViewThresh = 5;
        public int m_SideViewMarkMin = 8;
        public int m_BlobAreaMin = 100;
        public int m_BlobAreaMax = 800;

        public double m_FakeMark = 0.38;
        public int m_FovStep = 10;

        int mTriggerGrabbedFrame = 0;
        double mTriggerGrabbedFPS = 0;
        double mHowLongItTook = 0;
        public bool m_bPrismCoordinateSystem = false;
        public struct VROI
        {
            public int top;
            public int bottom;
        };
        public struct AREA
        {
            public int top;
            public int bottom;
            public int left;
            public int right;
        };

        public VROI[] mOrgROI = new VROI[2];
        public AREA[] mAbsMark = new AREA[24];
        public AREA[] mCurMark = new AREA[24];

        public const int mHROI = 1800; // hor-ROI
        public const int mVROI = 342; // hor-ROI

        public int[] m_curBoxYmin = new int[24];        //  current Box Y min of Model,2 found
        public int[] m_curBoxYmax = new int[24];        //  current Box Y max of Model,2 found
        public int[] v_OrgROIH_min = new int[2] { 600, 600 };  //  800 => 8.8mm
        public int[] v_OrgROIH_width = new int[2] { mHROI - 1, mHROI - 1 };    //hor-ROI
        public int[] v_OrgROIV_min = new int[2] { mVROI, mVROI };  //  280 => 3.08mm
        public int[] v_OrgROIV_height = new int[2] { mVROI - 1, mVROI - 1 };
        //public int[] v_OrgROIV_min       = new int[2] { 380 , 380 };  //  280 => 3.08mm
        //public int[] v_OrgROIV_height       = new int[2] { 380 - 1 , 380 - 1  };

        public const int ABSEXPOSURE = 73;
        public int[] v_OrgExposure = new int[2] { ABSEXPOSURE, ABSEXPOSURE };     //  1000 FPS 달성을 위한 최대 노출시간 
        public int[] v_OrgStlExposure = new int[2] { ABSEXPOSURE, ABSEXPOSURE };
        public int[] v_OrgGain = new int[2] { 66, 66 };          //  40mm 1, 68mm 12
        public int[] v_OrgStlGain = new int[2] { 66, 66 };       //  40mm 20, 68mm 46
        public double[] m_Yscale = new double[4] { 1, 1, 1, 1 };
        public bool m_bPrepareCLAFCal = false;
        public bool mLoaded = false;
        public bool[] mSocketLoaded = new bool[2] { false, false };
        public bool m_bLEDMarkCheck = false;
        public bool mbSaveFailImage = false;

        public delegate void DelegateMotorSetHome6D();
        public delegate void DelegateMotorMoveHome6D();

        public delegate void DelegateMotorMoveOrigin6D();
        public delegate void DelegateMotorMoveOriginHexapod();

        public delegate void DelegateMotorMoveAbsAxis(Axis axis, double pos);
        public delegate void DelegateMotorMoveAbs6D(double x, double y, double z, double tx, double ty, double tz);

        public delegate void DelegateMotorMoveRelAxis(Axis axis, double pos);
        public delegate void DelegateMotorMoveRel6D(double x, double y, double z, double tx, double ty, double tz);

        public delegate void DelegateMotorJogRun(Axis axis, bool dir, SpeedLevel speedLevel);
        public delegate void DelegateMotorJogStop(Axis axis);

        public delegate void DelegateMotorSetSpeedAxis(Axis axis, SpeedLevel speedlevel);
        public delegate void DelegateMotorSetSpeed6D(SpeedLevel speedlevel);


        public delegate void DelegateMotorSetPivot(double x, double y, double z);
        public delegate void DelegateMotorSetHCS(double tx, double ty, double tz);

        public delegate double DelegateMotorCurPosAxis(Axis axis);
        public delegate double[] DelegateMotorCurPos6D();
        public delegate double[] DelegateMotorCurPosHexpod();

        public delegate void DelegateHexapodRotate(double tx, double ty, double tz);
        public delegate void DelegateMotorXYZ(double x, double y, double z);


        DelegateMotorSetHome6D MotorSetHome6D;
        DelegateMotorMoveHome6D MotorMoveHome6D;

        DelegateMotorMoveOrigin6D MotorMoveOrigin6D;
        DelegateMotorMoveOriginHexapod MotorMoveOriginHexapod;

        DelegateMotorMoveAbsAxis MotorMoveAbsAxis;
        DelegateMotorMoveAbs6D MotorMoveAbs6D;  //  mm, arcmin

        DelegateMotorMoveAbsAxis MotorMoveRelAxis;
        DelegateMotorMoveAbs6D MotorMoveRel6D;  //  mm, arcmin

        DelegateMotorJogRun MotorJogRun;
        DelegateMotorJogStop MotorJogStop;

        DelegateMotorSetSpeedAxis MotorSetSpeedAxis;
        DelegateMotorSetSpeed6D MotorSetSpeed6D;

        DelegateMotorSetPivot MotorSetPivot;
        DelegateMotorSetHCS MotorSetHCS;

        DelegateMotorCurPosAxis MotorCurPosAxis;
        DelegateMotorCurPos6D MotorCurPos6D;
        DelegateMotorCurPosHexpod MotorCurPosHexapod;

        DelegateHexapodRotate HexapodRotate;
        DelegateMotorXYZ MotorXYZ;

        List<Button> CalBtnGroup;

        //  public void PrepareRemoteCalibration()
        //  public void SingleFindMark()
        //  public void RemoteCalibration(string strAxis, int skipCount)
        //  MotorHome
        //  MotorMove6D
        //  MotorMoveAxisAbs

        public void RegisterMotorDelegates(
            DelegateMotorSetHome6D fmotorSetHome6D,
            DelegateMotorMoveHome6D fmotorMoveHome6D,

            DelegateMotorMoveOrigin6D fmotorMoveOrigin6D,
            DelegateMotorMoveOriginHexapod fmotorMoveOrigin6Hexapod,

            DelegateMotorMoveAbsAxis fmotorMoveAbsAxis,
            DelegateMotorMoveAbs6D fmotorMoveAbs6D,

            DelegateMotorMoveAbsAxis fmotorMoveRelAxis,
            DelegateMotorMoveAbs6D fmotorMoveRel6D,

            DelegateMotorJogRun fmotorJogRun,
            DelegateMotorJogStop fmotorJogStop,

            DelegateMotorSetSpeedAxis fmotorSetSpeedAxis,
            DelegateMotorSetSpeed6D fmotorSetSpeed6D,

            DelegateMotorSetPivot fmotorSetPivot,
            DelegateMotorSetHCS fmotorSetHCS,

            DelegateMotorCurPosAxis fmotorCurPosAxis,
            DelegateMotorCurPos6D fmotorCurPos6D,
            DelegateMotorCurPosHexpod fmotorCurPosHexapod,

            DelegateHexapodRotate fhexapodRotate,
            DelegateMotorXYZ fmotorXYZ

            )
        {
            MotorSetHome6D = fmotorSetHome6D;
            MotorMoveHome6D = fmotorMoveHome6D;

            MotorMoveOrigin6D = fmotorMoveOrigin6D;
            MotorMoveOriginHexapod = fmotorMoveOrigin6Hexapod;

            MotorMoveAbsAxis = fmotorMoveAbsAxis;
            MotorMoveAbs6D = fmotorMoveAbs6D;

            MotorMoveRelAxis = fmotorMoveRelAxis;
            MotorMoveRel6D = fmotorMoveRel6D;

            MotorJogRun = fmotorJogRun;
            MotorJogStop = fmotorJogStop;

            MotorSetSpeedAxis = fmotorSetSpeedAxis;
            MotorSetSpeed6D = fmotorSetSpeed6D;

            MotorSetPivot = fmotorSetPivot;
            MotorSetHCS = fmotorSetHCS;

            MotorCurPosAxis = fmotorCurPosAxis;
            MotorCurPos6D = fmotorCurPos6D;
            MotorCurPosHexapod = fmotorCurPosHexapod;

            HexapodRotate = fhexapodRotate;
            MotorXYZ = fmotorXYZ;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public FVision()
        {
            InitializeComponent();
            //ReadVisionParam();
        }
        private void BtnAdminMode_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;
            if (bt.Text == "Operate Mode")
            {
                F_VisionPassForm dg = new F_VisionPassForm();
                if (dg.ShowDialog() == DialogResult.OK)
                {
                    VisibleAdmin(true);
                }
                else
                {
                    VisibleAdmin(false);
                    return;
                }

            }
            else
            {
                VisibleAdmin(false);
            }
        }
        private void VisibleAdmin(bool isOn)
        {
            if (isOn)
            {
                BtnAdminMode.Text = "Amdin Mode";
                tbInfo.Location = new System.Drawing.Point(724, 736);
                tbInfo.Size = new System.Drawing.Size(1192, 185);
                tbInfo.Font = new Font("Malgun Gothic", 8, FontStyle.Bold);
            }
            else
            {
                BtnAdminMode.Text = "Operate Mode";
                tbInfo.BringToFront();
                tbInfo.Location = new System.Drawing.Point(5, 736);
                tbInfo.Size = new System.Drawing.Size(1920, 300);
                tbInfo.Font = new Font("Malgun Gothic", 16, FontStyle.Bold);
            }
            PanelAdmin0.Visible = isOn;
            PanelAdmin1.Visible = isOn;
            PanelAdmin2.Visible = isOn;
            grpAdjust.Visible = isOn;
            grpCrop.Visible = isOn;
        }
        public int GetTriggerGrabbedFrame()
        {
            return mTriggerGrabbedFrame;
        }
        public double GetTriggerGrabbedFPS()
        {
            return mTriggerGrabbedFPS;
        }
        public double GetHowLongItTook()
        {
            return mHowLongItTook;
        }
        public void SetTriggerGrabbedFrame(int fn)
        {
            mTriggerGrabbedFrame = fn;
        }
        public void SetTriggerGrabbedFPS(double fps)
        {
            mTriggerGrabbedFPS = fps;
        }
        public void SetHowLongItTook(double time)
        {
            mHowLongItTook = time;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void FVision_Load(object sender, EventArgs e)
        {
            try
            {
                m__G = Global.GetInstance();
                cbContinuosMode.Enabled = true;
                //string which = "true";
                //int i = 0;

                //Thread threadInitBalser = new Thread(() => InitBaslerCam());
                //threadInitBalser.Start();
                InitBaslerCam();

                System.Drawing.Point[] pts =  {
                                new System.Drawing.Point( 2,  2),
                                new System.Drawing.Point(69,  2),
                                new System.Drawing.Point(69,  25),
                                new System.Drawing.Point(35,  48),
                                new System.Drawing.Point(35,  48),
                                new System.Drawing.Point(2, 25),
                          };
                // Make the GraphicsPath.
                GraphicsPath polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(pts);
                Region polygon_region = new Region(polygon_path);
                btnFOVDown.Region = polygon_region;
                btnFOVDown.SetBounds(
                    btnFOVDown.Location.X,
                    btnFOVDown.Location.Y, pts[2].X + 4, pts[3].Y + 4);

                System.Drawing.Point[] ptsU =  {
                                new System.Drawing.Point(2,  48),
                                new System.Drawing.Point(2,  24),
                                new System.Drawing.Point(35,  2),
                                new System.Drawing.Point(35,  2),
                                new System.Drawing.Point(69,  25),
                                new System.Drawing.Point(69, 48),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsU);
                polygon_region = new Region(polygon_path);
                btnFOVUp.Region = polygon_region;
                btnFOVUp.SetBounds(
                    btnFOVUp.Location.X,
                    btnFOVUp.Location.Y, ptsU[4].X + 4, ptsU[5].Y + 4);

                System.Drawing.Point[] ptsL =  {
                                new System.Drawing.Point(2,  35),
                                new System.Drawing.Point(24,  2),
                                new System.Drawing.Point(48,  2),
                                new System.Drawing.Point(48,  69),
                                new System.Drawing.Point(24,  69),
                                new System.Drawing.Point(2, 35),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsL);
                polygon_region = new Region(polygon_path);
                btnFOVLeft.Region = polygon_region;
                btnFOVLeft.SetBounds(
                    btnFOVLeft.Location.X,
                    btnFOVLeft.Location.Y, ptsL[2].X + 4, ptsL[4].Y + 4);

                System.Drawing.Point[] ptsR =  {
                                new System.Drawing.Point(2,  2),
                                new System.Drawing.Point(24,  2),
                                new System.Drawing.Point(48,  35),
                                new System.Drawing.Point(48,  35),
                                new System.Drawing.Point(24,  69),
                                new System.Drawing.Point(2, 69),
                          };
                // Make the GraphicsPath.
                polygon_path = new GraphicsPath(FillMode.Winding);
                polygon_path.AddPolygon(ptsR);
                polygon_region = new Region(polygon_path);
                btnFOVRight.Region = polygon_region;
                btnFOVRight.SetBounds(
                    btnFOVRight.Location.X,
                    btnFOVRight.Location.Y, ptsR[2].X + 4, ptsR[5].Y + 4);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            radioButton10Step.Checked = true;
            cbLiveWithMarks.Checked = false;
            rb1step.Checked = true;
            this.BackColor = Color.FromArgb(96, 96, 100);
            //MessageBox.Show("aaa");
            //if (m__G!=null)
            SetEdgeBand(m__G.sRecipe.iEdgeBand);

            //else
            //    SetEdgeBand();

            //MessageBox.Show("bbb");
            tbInfo.BringToFront();
            tbVsnLog.BringToFront();

            //ChartMTF.Hide();
            //LoadscaleNTheta();
            if (m__G.oCam[0].mFAL != null)
                if (m__G.oCam[0].mFAL.mFZM != null)
                {
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);
                }

            grpCrop.Hide();
            cbZaxis.Hide();
            cbTiltAxis.Hide();
            btnSaveOrgPosition.Hide();
            CalBtnGroup = new List<Button> { btnFindCSHorg, btnRangeTest, btnScan, btnRepeatTest, btnEastView, btnAutoCal };
            cboAxis.DataSource = Enum.GetValues(typeof(Axis));
            rbFromOrg.Checked = true;
            cbBench.Checked = true;
            cbBench.Checked = false;

            tbXrange.Text = "0";
            tbYrange.Text = "0";
            tbZrange.Text = "0";
            tbTXrange.Text = "0";
            tbTYrange.Text = "0";
            tbTZrange.Text = "0";

            tbXstep.Text = "3";
            tbYstep.Text = "3";
            tbZstep.Text = "3";
            tbTXstep.Text = "3";
            tbTYstep.Text = "3";
            tbTZstep.Text = "3";

            //cbSkipFindFidOrg.Checked = false;
            InitializeHexpodPivot();
            if (m__G.m_bCalibrationModel)
            {
                if (m__G.oCam[0].IsInit) m__G.mGageCounter?.OpenAllport();
                MAX_TRGGRAB_COUNT = 3000;
            }
            else
            {
                grbCalibration.Visible = false;
                grbVolumetric.Visible = false;
            }
            if(m__G.fMotion == null)
            {
                MotionStageBtn.Enabled = false;
            }
            //if (!mHybridStageAvailable)
            //{
            //    grbCalibration.Visible = false;
            //    grbVolumetric.Visible = false;
            //}
            VisibleAdmin(false);
        }

        public string camID0 = "";
        public string camID1 = "";
        public bool mHybridStageAvailable = true;
        public void InitBaslerCam()
        {
            //string which = "true";
            int i = 0;

            int[] lMarkGap = new int[4] { 11000, 11000, 11000, 11000 };
            if (!mLoaded)
            {
                if (m__G == null)
                    m__G = Global.GetInstance();

                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = false;
                        btnAllLEDOn.Enabled = false;
                        btnLEDDOWN.Enabled = false;
                        btnLEDUP.Enabled = false;
                        btnHalt2.Enabled = false;
                        button11.Enabled = false;
                    });

                int camCount = 2;


                if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt"))
                {
                    MessageBox.Show("Camera ID not exists.");
                    return;
                }
                StreamReader sr = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt");
                string fullText = sr.ReadToEnd();
                sr.Close();
                string[] camIDs = fullText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                camID0 = "";
                if (m__G.oCam[0].IsInit)
                {
                    foreach (string lstr in camIDs)
                    {
                        try
                        {
                            BaslerCam[0] = new Camera(lstr);  //"22727087"
                                                              //BaslerCam[0].CameraOpened += Configuration.AcquireSingleFrame;
                            BaslerCam[0].Open();
                            camID0 = lstr;
                            break;
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
                if (camID0 == "")
                {
                    MessageBox.Show("Camera ID is not found. Check Camera ID and Restart Application.");
                    System.Diagnostics.Process.GetCurrentProcess().Kill(); //배포시 활성화 
                }
                else
                {
                    //Cam ID Mac Adress 매칭 관련 =====
                    //

                    string macAddress = NetworkInterface.GetAllNetworkInterfaces()
                           .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                           .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();

                    string macPath = m__G.m_RootDirectory + string.Format("\\DoNotTouch\\MacAddr_{0}.txt", camID0);
                    if(!File.Exists(macPath))
                    {
                        StreamWriter wrm = new StreamWriter(macPath);
                        wrm.WriteLine(macAddress);
                        wrm.Close();
                    }
                    else
                    {
                        StreamReader srm = new StreamReader(macPath);
                        string rMacAddr = srm.ReadToEnd();
                        srm.Close();
                        if (!rMacAddr.Contains(macAddress))
                        {
                            MessageBox.Show("Camera ID and PC ID was Not Matching. Check PC ID.");
                            System.Diagnostics.Process.GetCurrentProcess().Kill(); //배포시 활성화 
                        }
                    }





                    StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt");
                    wr.WriteLine(camID0);
                    foreach (string lstr in camIDs)
                    {
                        if (lstr != camID0)
                            wr.WriteLine(lstr);
                    }
                    wr.Close();
                }
                //StreamReader sr = new StreamReader("CameraID.txt");
                //camID0 = sr.ReadLine();
                //sr.Close();
                //try
                //{
                //    BaslerCam[0] = new Camera(camID0);  //"22727087"
                //                                        //BaslerCam[0].CameraOpened += Configuration.AcquireSingleFrame;
                //    BaslerCam[0].Open();
                //}
                //catch
                //{
                //    MessageBox.Show("Camera ID is not correct. Check Camera ID and Restart Application.");
                //    return;
                //}
                m__G.SetTesterID(camID0);
                ReadOrgROI(camCount);

                if(BaslerCam[0] != null )BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                //BaslerCam[1] = null;
                m__G.mCamCount = 1;
                //tbVsnLog.Text += "\t m__G.mCamCount  = " + m__G.mCamCount;
                //BaslerCam[0].Parameters[PLCamera.UserSetLoad].Execute();
                //BaslerCam[0].Parameters[PLCamera.UserSetSave].Execute();

                if (rbLED1.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        rbLED1.Checked = true;
                    });
                }


                m_FocusedLED = 0;

                if (BaslerCam[0] != null)
                {
                    for (i = 0; i < m__G.mCamCount; i++)
                        SetNewROIXY(i, v_OrgROIH_min[i], v_OrgROIH_min[i] + v_OrgROIH_width[i], v_OrgROIV_min[i], v_OrgROIV_min[i] + v_OrgROIV_height[i]);



                    //ReadZeroGap(m__G.mCamCount);
                    //ReadCalibrationTiltData();

                    if (InvokeRequired)
                        BeginInvoke((MethodInvoker)delegate
                        {
                            m__G.oCam[0].SelectWindow(LiveView.Handle);
                            //panelCam0.Size = new Size((int)(mHROI*1.833), 627);
                            //panelCam0.Location = new Point(1940 - (int)(mHROI * 1.833), 4);
                        });
                    else
                    {
                        m__G.oCam[0].SelectWindow(LiveView.Handle);
                        //panelCam0.Size = new Size((int)(mHROI * 1.833), 627);
                        //panelCam0.Location = new Point(1940 - (int)(mHROI * 1.833), 4);
                    }

                    m__G.oCam[0].DisplayZoom(ZoomFactor, ZoomFactor);

                    BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
                    BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
                    //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
                    BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[0]);
                    BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);

                    m__G.oCam[0].SetBlobAreaMinMax(m_BlobAreaMin, m_BlobAreaMax);
                    string strScaleRotation = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt";
                    double stop = 0;
                    double sside = 0;
                    double rtop = 0;
                    double rside = 0;

                    m__G.oCam[0].LoadScaleNOpticalRotation(strScaleRotation, ref stop, ref sside, ref rtop, ref rside);

                    if (InvokeRequired)
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnLive2.Enabled = true;
                            btnAllLEDOn.Enabled = true;
                            btnLEDDOWN.Enabled = true;
                            btnLEDUP.Enabled = true;
                            btnHalt2.Enabled = true;
                            button11.Enabled = true;

                            radioButton10Step.Checked = true;
                            rb1step.Checked = true;
                            cbContinuosMode.Enabled = true;
                        });
                }
            }
            TransferModelFileList();
            SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            SetExposure(0, m__G.sRecipe.iExposure);
            LoadBackbroundNoise();
            LoadScaleNTheta();
            //LoadTXTYZeroOffset();
            SetDefaultMarkConfig();
            //string ZLUTfile = m__G.m_RootDirectory + "\\DoNotTouch\\ZLUT_" + camID0 + ".txt";
            //GetZLUT(ZLUTfile);
            string strTXTYTZoffset = LoadTXTYZeroOffset();
            m__G.fManage.AddViewLog("CSH ID " + camID0 + "\t" + strTXTYTZoffset);

            //Regstry Write ====
            Registry.LocalMachine.CreateSubKey("SOFTWARE").CreateSubKey("CSHTest");
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CSHTest", true);

            if (reg.GetValue(camID0) == null)
                reg.SetValue(camID0, DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss"));
            reg.Close();
            //==================

            // 241206   // YLUT 적용안함
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            mLoaded = true;

            //m__G.oCam[0].RegisterDelegates(m__G.fGraph.mDriverIC.AckSignal);


            // Crop Pos File Load & Init
            bool isLoad = m__G.oCam[0].LoadCropPosFromXml(camID0);
            m__G.oCam[0].InitCrop(!isLoad);
            CropCgap = m__G.oCam[0].CropCgap;

            string motorHomeFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageHomePos{camID0}.txt";
            string motorCSFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageCSPos{camID0}.txt";
            string motorSppedFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageSpeed{camID0}.txt";
            // 모션
            //
            // SK 스테이지
            //m__G.mMotion = new F_Motion(motorFilePath);
            //RegisterMotorDelegates(m__G.mMotion.LogicalOrg, m__G.mMotion.SetLogicalOrg, m__G.mMotion.MoveABS6D, m__G.mMotion.MoveABS,m__G.mMotion.GetCurPos, m__G.mMotion.GetCurPos6D,m__G.mMotion.JogMove, m__G.mMotion.JogStop, m__G.mMotion.SetSpeed6D);
            //
            // PI 스테이지
            //m__G.f_PIMotion = new F_PIMotion(motorFilePath);
            //RegisterMotorDelegates(F_PIMotion._pi.SetLogicHome6D,
            //                        F_PIMotion._pi.MoveLogicHome6D,
            //                        F_PIMotion._pi.MoveAbsAxis,
            //                        F_PIMotion._pi.MoveAbs6D,
            //                        F_PIMotion._pi.GetCurMechaPosAxis,
            //                        F_PIMotion._pi.GetCurMechaPos6D,
            //                        F_PIMotion._pi.GetCurLogicPosAxis,
            //                        F_PIMotion._pi.GetCurLogicPos6D,
            //                        F_PIMotion._pi.JogRun,
            //                        F_PIMotion._pi.JogStop,
            //                        F_PIMotion._pi.SetPivot3D,
            //                        null);
            //LoadHexpodPivots();
            // SK + PI 스테이지
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    m__G.fMotion = new F_Motion_SK_PI(motorHomeFilePath, motorCSFilePath, motorSppedFilePath);
                }));
            }
            else
            {
                m__G.fMotion = new F_Motion_SK_PI(motorHomeFilePath, motorCSFilePath, motorSppedFilePath);
            }

            RegisterMotorDelegates(m__G.fMotion.SetHome6DFormCurPos,
                                   m__G.fMotion.MoveHome6D,

                                   m__G.fMotion.MoveOrigin6D,
                                   m__G.fMotion.MoveOriginHexapod,

                                   m__G.fMotion.MoveAbsAxis,
                                   m__G.fMotion.MoveAbs6D,

                                   m__G.fMotion.MoveRelAxis,
                                   m__G.fMotion.MoveRel6D,

                                   m__G.fMotion.JogRun,
                                   m__G.fMotion.JogStop,

                                   m__G.fMotion.SetSpeedAxis,
                                   m__G.fMotion.SetSpeed6D,

                                   m__G.fMotion.SetPivot,
                                   m__G.fMotion.SetCoordinateSystem,

                                   m__G.fMotion.GetCurPosAxis,
                                   m__G.fMotion.GetCurPos6D,
                                   m__G.fMotion.GetCurPosHexapod,

                                   m__G.fMotion.MoveHexapodRotation,
                                   m__G.fMotion.MoveSkXYZ

                                   );
            if (m__G.m_bCalibrationModel)
            {
                if (m__G.fMotion.ConnectSKPI())
                {
                    m__G.fMotion.mInitialMsg = "Connection completed.";
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbVsnLog.Text = "Hybrid Stage connection complete.";
                        });
                    }
                    else
                        tbVsnLog.Text = "Hybrid Stage connection complete.";
                }
                //else
                //mHybridStageAvailable = false;
            }
            // JH 스테이지
            //RegisterMotorDelegates(m__G.fManage.M_C,
            //            m__G.fManage.M_H,
            //            m__G.fManage.M_M,
            //            m__G.fManage.M_A,
            //            m__G.fManage.M_G,
            //            m__G.fManage.M_P,
            //            null,
            //            null,
            //            m__G.fManage.M_J,
            //            m__G.fManage.M_E,
            //            null,
            //            null);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //private void FVision_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //}
        public int GetMasterZeroCount()
        {
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero";
            return Directory.GetFiles(cPath).Length;
        }
        public int GetMasterZeroIndex()
        {
            int curIndex;
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\PreviousOffsetIndex.txt";
            StreamReader rd = new StreamReader(cPath);
            curIndex = int.Parse(rd.ReadLine());
            rd.Close();
            return curIndex;
        }
        public void SetMasterZeroIndex(int index)
        {
            string cPath = m__G.m_RootDirectory + "\\DoNotTouch\\PreviousOffsetIndex.txt";
            StreamWriter sw = new StreamWriter(cPath);
            sw.WriteLine(index);
            sw.Close();
        }
        public void InitMasterZeroList()
        {
            string folder = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string[] files = Directory.GetFiles(folder);

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    MasterList.Items.Clear();
                    for (int j = 0; j < files.Length; j++)
                    {
                        MasterList.Items.Add(Path.GetFileName(files[j]));
                    }

                });
            else
            {
                MasterList.Items.Clear();
                for (int j = 0; j < files.Length; j++)
                {
                    MasterList.Items.Add(Path.GetFileName(files[j]));
                }
            }
        }
        public void SetEdgeBand(int nband = 7)
        {
            if (m__G == null) return;
            if (m__G.oCam[0] == null) return;
            if (m__G.oCam[0].mFAL == null) return;
            m__G.oCam[0].mFAL.mCalcEffBand = nband;
        }
        public void SetRawGainNGamma(int lGain, double lGamma)
        {
            if (BaslerCam[0] == null) return;
            BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(lGain);
            BaslerCam[0].Parameters[PLCamera.Gamma].SetValue(lGamma);
            BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);
        }
        public void EnableBtns(bool bEnable)
        {
            if (bEnable)
            {
                //btnSetAbsZero.Enabled = false;
            }
            else
            {
                //btnSetAbsZero.Enabled = true;
            }

            //SlowlyChk.Checked = false; //2021.08.31 added
            rbLED2.Checked = true; //2021.08.31 added
            rb1step.Checked = true;
            cbSetTXTYwithMaster.Checked = false;

            MasterList.Enabled = false;
            btnDeleteMaster.Enabled = false;
            btnAddMaster.Enabled = false;
            btnInitialTilt.Enabled = false;
            btnSetMasterTilt.Enabled = false;
            tbMasterTX.Enabled = false;
            tbMasterTY.Enabled = false;

            m_FocusedLED = 0;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[0].DrawClear();
            //    m__G.oCam[0].nBoxP1.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[0].nBoxP1.Y = (int)(e.Y / ZoomFactor);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[0].nBoxP2.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[0].nBoxP2.Y = (int)(e.Y / ZoomFactor);

            //    m__G.oCam[0].DrawClear();
            //    m__G.oCam[0].DrawDCCross(Brushes.Red);
            //    m__G.oCam[0].DrawDCBox(m__G.oCam[0].nBoxP1, m__G.oCam[0].nBoxP2, Brushes.Yellow);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam2_MouseDown(object sender, MouseEventArgs e)
        {
            //if (m__G.mCamCount < 2) return;
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[1].DrawClear();
            //    m__G.oCam[1].nBoxP1.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[1].nBoxP1.Y = (int)(e.Y / ZoomFactor);
            //}
        }

        //public void ClearImgBuf()
        //{
        //    m__G.oCam[0].DrawClear();
        //    m__G.oCam[CAM2].DrawClear();
        //}


        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void panelCam2_MouseMove(object sender, MouseEventArgs e)
        {
            //if (m__G.mCamCount < 2) return;
            //if (e.Button == MouseButtons.Left)
            //{
            //    m__G.oCam[1].nBoxP2.X = (int)(e.X / ZoomFactor);
            //    m__G.oCam[1].nBoxP2.Y = (int)(e.Y / ZoomFactor);
            //    m__G.oCam[1].DrawClear();
            //    m__G.oCam[1].DrawDCCross(Brushes.Red);
            //    m__G.oCam[1].DrawDCBox(m__G.oCam[1].nBoxP1, m__G.oCam[1].nBoxP2, Brushes.Yellow);
            //}
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnLive2_Click(object sender, EventArgs e)
        {
            //label1.Text = "Number of Camera : " + m__G.mCamCount.ToString();
            //cbContinuosMode.Checked = true;
            //Thread.Sleep(200);

            StartLive();

            //m__G.oCam[1].DrawDC_Circle(Brushes.Red, 200);    // DrawCircle khkim_170920
            //Imae Crop === 
            //Task.Factory.StartNew(() => 
            //{
            //});

            //m__G.oCam[0].CropImage(0);

        }

        public void StartLive()
        {
            if (!m__G.oCam[0].IsInit) return;
            cbContinuosMode.Checked = true;
            Thread.Sleep(200);

            bHaltLive = false;
            m__G.mbSuddenStop[0] = true;
            m__G.mDoingStatus = "Checking Vision";

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            m__G.fGraph.Drive_LEDs(mLEDcurrent[0], mLEDcurrent[1]);
            btnAllLEDOn.ForeColor = Color.White;

            m_bAllLEDOn = true;
            //label6.Location = new Point(10, 140);
            //label6.Text = "Live On";
            //m__G.oCam[0].DrawDC_Circle(Brushes.Red, 200);    // DrawCircle khkim_170920
            m__G.oCam[0].DrawAllRectangles();

            if (cbLiveWithMarks.Checked && bLiveFindMark == false)
            {
                // 250326 폰트 설정 cbLiveWithMarks_CheckedChanged()에서 이동
                //tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                Task.Run(() => LiveFindMark());
            }
            else
            {
                //tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);

                if (m__G.mCamCount > 1)
                {
                    m__G.oCam[1].ClearDisp();
                    m__G.oCam[1].LiveA();
                }
                else
                {
                    m__G.oCam[0].ClearDisp();
                    m__G.oCam[0].LiveA();

                }
            }
        }

        bool bLiveFindMark = false;

        public void LiveFindMark()
        {
            //if (!bLiveFindMark)
            //    return;

            // 250326 이미 LiveFindMark 중 이라면 return 으로 조건 변경
            if (bLiveFindMark)
                return;

            m__G.mDoingStatus = "LiveFindMark";

            // 데이터 mCalibrationFullData에 저장해서 SetMasterTilt할때 마지막 데이터 사용
            mCalibrationFullData.Clear();
            double[] lCalibrationData = new double[23];

            m__G.fGraph.Drive_LEDs(m__G.sRecipe.iLEDcurrentLR, m__G.sRecipe.iLEDcurrentLL);
            Thread.Sleep(10);

            bLiveFindMark = true;
            int fcnt = 0;

            int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            int frmCnt = 3000;
            //for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
            //    m__G.oCam[0].GrabB(i);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[frmCnt + 1];


            m__G.oCam[0].SetTriggeredframeCount(frmCnt);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.oCam[0].ForceTriggerTime();
            m__G.oCam[0].mTrgBufLength = 3000;

            ChangeFiducialMark(m__G.mFAL.mCandidateIndex);
            SetDefaultMarkConfig(true);

            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            //m__G.fVision.ProcessVisionData(frmCnt, m__G.mMaxThread);
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;


            while (!bHaltLive)
            {
                m__G.oCam[0].DrawClear();
                DrawMarkPositions();
                m__G.oCam[0].DrawAllRectangles();

                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                m__G.oCam[0].GrabB(fcnt);
                FindMarks(fcnt++);
                if (fcnt == 10000)
                    fcnt = 0;

                m__G.oCam[0].mFAL.RecoverFromBackupFMI();


                if (tbInfo.InvokeRequired)
                {
                    tbInfo.BeginInvoke(new Action(() =>
                    {
                        string lstr = tbInfo.Text;
                        string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        if (lineStr.Length > 6)
                        {
                            lstr = "";
                            for (int i = 0; i < 6; i++)
                                lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";


                            tbInfo.Text = lstr;
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        }
                    }));
                }
                else
                {
                    string lstr = tbInfo.Text;
                    string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (lineStr.Length > 6)
                    {
                        lstr = "";
                        for (int i = 0; i < 6; i++)
                            lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";


                        tbInfo.Text = lstr;
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                }

                Thread.Sleep(180);
                if (!cbLiveWithMarks.Checked)
                    break;
            }
            // bHaltLive = false;
            bLiveFindMark = false;

        }
        public double mExpectedYfromSideNStpTopNS = 0;
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void btnGrab2_Click(object sender, EventArgs e)
        {


            //SetDefaultMarkConfig(true);
            //DrawMarkPositions();
        }

        public void DrawMarkPositions()
        {
            //  Default Mark Position

            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in DrawMarkPositions()");
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            mExpectedYfromSideNStpTopNS = markPos[5].Y - markPos[0].Y;
            //m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

            if (m__G.mFAL != null)
            {

                //string markPosFile = m__G.mFAL.GetFileNameOfMarkPosOnPanel();
                //if (File.Exists(markPosFile))
                //{
                //    StreamReader sr = new StreamReader(markPosFile);
                //    string allLines = sr.ReadToEnd();
                //    sr.Close();
                //    List<Point> mPos = new List<Point>();
                //    string[] eachLine = allLines.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //    for (int i = 0; i < eachLine.Length; i++)
                //    {
                //        if (eachLine[i].Length < 3)
                //            continue;
                //        string[] xypos = eachLine[i].Split(',');
                //        if (xypos.Length < 2)
                //            continue;
                //        Point lp = new Point();
                //        lp.X = int.Parse(xypos[0]);
                //        lp.Y = int.Parse(xypos[1]);
                //        mPos.Add(lp);
                //    }
                //    mExpectedYfromSideNStpTopNS = Math.Abs(mPos[0].Y - mPos[mPos.Count - 1].Y);
                //    if (mPos.Count > 0)
                //    {
                //        markPos = mPos.ToArray();
                //    }
                //}
                m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
                m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            }
            //m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
        }
        public System.Drawing.Point[] mStdMarkPos = new System.Drawing.Point[6];


        public void Wait1ms(int ms = 1)
        {
            double res = 0;
            for (int i = 0; i < 4000000 * ms; i++)
            {
                res = Math.Abs(Math.Sin(i));
                res = Math.Sqrt(Math.Sin(Math.Log10(res)));
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        // 처음 시작할때 Live 상태 아니므로 bHaltLive는 true
        //bool bHaltLive = false;
        public bool bHaltLive = true;
        public void GrabHalt()
        {
            //label6.Text = "";
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].HaltA();
            bHaltLive = true;
            btnAllLEDOn.ForeColor = Color.SlateGray;
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            Thread.Sleep(100);
            m__G.fGraph.Drive_LEDs(0, 0);

            //m__G.oCam[0].ClearDisp();
            bThreadManualFindMarks = false;
        }
        private void btnHalt2_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            GrabHalt();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        //Graphics gr;
        private void btnClear1_Click(object sender, EventArgs e)
        {
            tbVsnLog.Text = "";
            tbInfo.Text = "";
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].DrawClear();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public bool RestoreROI()
        {
            for (int i = 0; i < 2; i++)
            {
                SetNewROIY(i, v_OrgROIV_min[i], (v_OrgROIV_min[i] + v_OrgROIV_height[i]), v_OrgExposure[i]);   //  재시도??
            }
            return true;
        }

        public bool AdjustROI(int Cam)
        {
            if (mAbsMark[Cam].top - mAbsMark[Cam].bottom < m__G.mVROI[Cam])
                return SetNewROIY(Cam, mAbsMark[Cam].bottom, mAbsMark[Cam].top, v_OrgExposure[Cam]);
            else
                return SetNewROIY(Cam, mAbsMark[Cam].bottom, mAbsMark[Cam].bottom + m__G.mVROI[Cam], v_OrgExposure[Cam]);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        private void btnOISXReplay_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }

            btnOISXReplay.Enabled = false;
            btnOISXStepReplay.Enabled = false;

            Thread ThreadReplayL = new Thread(() => Process_OISXReplay(0, 0));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Process_OISXReplay(int port, int interval = 10)
        {
            //for (int n = 0; n < m__G.m_AFPeakTimeIndex+1; n += 1)

            int nOldinterval = interval;///2021.08.31 Slowlychk added

            if (m__G.oCam[0].dAFZM_FrameCount > 0)
            {
                for (int n = 0; n < m__G.oCam[0].dAFZM_FrameCount; n++)
                {
                    m__G.oCam[0].BufCopy2Disp_OISX(n);
                    if (m__G.mCamCount > 1)
                        m__G.oCam[1].BufCopy2Disp_OISX(n);

                    Thread.Sleep(interval);
                }
            }
            else
            {
                for (int n = 0; n < m__G.fGraph.mAF_FrameCount; n++)
                {
                    m__G.oCam[0].BufCopy2Disp_OISX(n);

                    Thread.Sleep(interval);
                }
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnOISXReplay.Enabled = true;
                    btnOISXStepReplay.Enabled = true;
                });
            }
            else
            {
                btnOISXReplay.Enabled = true;
                btnOISXStepReplay.Enabled = true;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        private void btnOISYReplay_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }
            tbVsnLog.Text += "AF Step Total : " + m__G.oCam[0].dAFStep_FrameCount.ToString() + "frame \r\n";
            //btnOISYReplay.Enabled = false;

            int stepInterval = 20;
            if (m__G.oCam[0].dAFStep_FrameCount < 50)
                stepInterval = 200;

            //if (SlowlyChk.Checked) stepInterval *= 10;

            Thread ThreadReplayL = new Thread(() => Process_AFStepReplay(stepInterval));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

            //Thread ThreadReplayL = new Thread(() => Process_OISYReplay(0));
            //ThreadReplayL.Start();
            //if (m__G.mCamCount > 1)
            //{
            //    Thread ThreadReplayR = new Thread(() => Process_OISYReplay(1));
            //    ThreadReplayR.Start();
            //}
            //m_replayIndex = 0;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Process_AFStepReplay(int delay)
        {

            for (int n = 0; n < m__G.oCam[0].dAFStep_FrameCount; n++)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbVsnLog.Text += "AF Step : " + n.ToString() + "frame Show \r\n";
                    });
                }
                else
                    tbVsnLog.Text += "AF Step : " + n.ToString() + "frame Show \r\n";

                m__G.oCam[0].BufCopy2Disp_XStep(n);
                if (m__G.mCamCount > 1)
                    m__G.oCam[1].BufCopy2Disp_XStep(n);
                Thread.Sleep(1 + delay);
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void GetCurrentROIY(int Cam, ref int ROImin, ref int ROImax, ref int exposureTime)
        {
            long tmp = 0;
            tmp = BaslerCam[Cam].Parameters[PLCamera.Width].GetValue();
            ROImin = (int)tmp;


            tmp = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();
            ROImax = (int)tmp;


            double tmpb = BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].GetValue();
            exposureTime = (int)tmpb;
        }



        public bool RefreshBasler(int port)
        {
            BaslerCam[port].Close();
            Thread.Sleep(100);
            BaslerCam[port].Open();
            Thread.Sleep(200);

            BaslerCam[port].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            //if (m__G.mCamCount < 2)
            //    BaslerCam[port].Parameters[PLCamera.ReverseX].SetValue(true);
            //else
            //    BaslerCam[port].Parameters[PLCamera.ReverseX].SetValue(false);

            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            return true;
        }
        public bool RefreshBasler()
        {
            BaslerCam[0].Close();
            if (m__G.mCamCount > 1)
                BaslerCam[1].Close();

            Thread.Sleep(100);
            BaslerCam[0].Open();
            if (m__G.mCamCount > 1)
                BaslerCam[1].Open();
            Thread.Sleep(200);

            BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            if (m__G.mCamCount > 1)
                BaslerCam[1].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");

            //BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            //if (m__G.mCamCount > 1)
            //{
            //    BaslerCam[1].Parameters[PLCamera.ReverseX].SetValue(true);
            //    BaslerCam[1].Parameters[PLCamera.ReverseY].SetValue(false);
            //}
            //else
            //{
            //    BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(false);
            //}

            return true;
        }

        public bool ShiftROI(int Cam, int dx, int dy)
        {
            v_OrgROIH_min[Cam] = ((v_OrgROIH_min[Cam] + dx) / 8) * 8;

            if (v_OrgROIH_min[Cam] < 1)
            {
                v_OrgROIH_min[Cam] = 0;
            }
            int lROIHmax = v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam];
            if (lROIHmax > 2078)
            {
                v_OrgROIH_min[Cam] = 2079 - v_OrgROIH_width[Cam];
            }

            v_OrgROIV_min[Cam] = v_OrgROIV_min[Cam] + dy;
            if (v_OrgROIV_min[Cam] < 1)
            {
                v_OrgROIV_min[Cam] = 1;
            }
            int lROIVmax = v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam];
            if (lROIVmax > 1078)
            {
                v_OrgROIV_min[Cam] = 1024 - v_OrgROIV_height[Cam];
            }

            SetNewROIXY(Cam, v_OrgROIH_min[Cam], v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam], v_OrgROIV_min[Cam], v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]);
            SaveOrgROI(1);

            return true;
        }

        public bool SetNewROIXY(int Cam)
        {
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam], v_OrgROIV_min[Cam], v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]);
            return true;
        }

        public int GetROIY(int Cam)
        {
            long res = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            return (int)res;
        }
        public bool SetNewROIXY(int Cam, int fovL, int fovR, int fovU, int fovD /* L < R , U < D */)
        {
            int height = fovD - fovU + 1;
            int width = fovR - fovL + 1;
            int top = fovU;
            int left = fovL;

            if (width + left > 2040)
                left = 2040 - width;
            if (height + fovU > 1079)
                top = 1079 - height;

            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(0);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(0);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(width);           // 사이즈 width 조절      
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(height);         // 사이즈 Height 조절       
            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(left);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(top);       // 사이즈 Height 조절

            //BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(v_OrgExposure[Cam]);            // 노출시간 조절, usec
            return true;
        }
        public void ChangeROIHeight(int Cam, int newHeight)
        {
            long oldOffsetY = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            if (oldOffsetY + newHeight >= 1080)
                oldOffsetY -= (oldOffsetY + newHeight - 1080);

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(oldOffsetY);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(newHeight);           // 사이즈 width 조절      khkim191106
        }
        public void ChangeROIYOffsetY(int Cam, int offsetY)
        {
            long oldHeight = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();           // 사이즈 width 조절
            if (offsetY + oldHeight >= 1080)
                offsetY = (int)(1080 - oldHeight);

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(offsetY);       // 사이즈 Height 조절
        }
        public void ChangeROIYOffsetDeltaY(int Cam, int offsetDeltaY)
        {
            long oldHeight = BaslerCam[Cam].Parameters[PLCamera.Height].GetValue();           // 사이즈 width 조절
            long oldOffsetY = BaslerCam[Cam].Parameters[PLCamera.OffsetY].GetValue();           // 사이즈 width 조절
            if (oldOffsetY + offsetDeltaY + oldHeight >= 1080)
                oldOffsetY = (int)(1080 - oldHeight);
            else
                oldOffsetY += offsetDeltaY;

            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(oldOffsetY);       // 사이즈 Height 조절
        }


        public void SetExposure(int Cam, int expTime, int gainRaw = -1)
        {
            if (BaslerCam[Cam] == null) return;
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절
            //if (gainRaw > 33 && gainRaw < 61)
            //    BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(gainRaw);
        }

        public void SetOrgExposure(int Cam)
        {
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(v_OrgExposure[Cam]);            // 노출시간 조절, usec
            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
        }

        public void SetExpGain(int Cam, int expTime/*, int analog_gain = 29*/)
        {
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절, usec
            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
        }

        public bool SetNewROIY(int Cam, int ROImin, int ROImax, int expTime/*, int analog_gain = 29*/)
        {
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(40);           // 사이즈 width 조절      khkim191106
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(10);         // 사이즈 Height 조절       khkim191106

            //BaslerCam[Cam].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[Cam]);           // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.Width].SetValue(v_OrgROIH_width[Cam] + 1);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.Height].SetValue(v_OrgROIV_height[Cam] + 1);         // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetX].SetValue(v_OrgROIH_min[Cam]);           // 사이즈 width 조절
            BaslerCam[Cam].Parameters[PLCamera.OffsetY].SetValue(ROImin);       // 사이즈 Height 조절
            BaslerCam[Cam].Parameters[PLCamera.ExposureTimeAbs].SetValue(expTime);            // 노출시간 조절, usec

            return true;
        }

        public void ShowCalParam()
        {
            //tb_AutoCalPXX1.Text = m__G.Cal_xx[0].ToString("F4");
            //tb_AutoCalPYX1.Text = m__G.Cal_yx[0].ToString("F4");
            //tb_AutoCalPXY1.Text = m__G.Cal_xy[0].ToString("F4");
            //tb_AutoCalPYY1.Text = m__G.Cal_yy[0].ToString("F4");

        }

        public void ClearCam(int numCam = 1) // 17년 1월 6일 이전버전
        {
            m__G.oCam[0].ClearDisp();
            if (m__G.mCamCount > 1)
                m__G.oCam[1].ClearDisp();
        }

        private void btnOISXStepReplay_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            m__G.oCam[0].ClearDisp();
            m__G.oCam[0].DrawClear();
            if (m__G.mCamCount > 1)
            {
                m__G.oCam[1].ClearDisp();
                m__G.oCam[1].DrawClear();
            }
            btnOISXReplay.Enabled = false;
            btnOISXStepReplay.Enabled = false;


            Thread ThreadReplayL = new Thread(() => Process_OISXReplay(0, 1));
            ThreadReplayL.Start();
            //m_replayIndex = 0;

        }

        ////private void btnOISYStepReplay_Click(object sender, EventArgs e)
        ////{
        ////    m__G.oCam[0].ClearDisp();
        ////    m__G.oCam[0].DrawClear();
        ////    if (m__G.mCamCount > 1)
        ////    {
        ////        m__G.oCam[1].ClearDisp();
        ////        m__G.oCam[1].DrawClear();
        ////    }
        ////    tbVsnLog.Text += "Zoom Step : " + m__G.oCam[0].dAFStep_FrameCount.ToString() + "frame \r\n";

        ////    Thread ThreadReplayL = new Thread(() => Process_ZMStepReplay(20));
        ////    ThreadReplayL.Start();
        ////    m_replayIndex = 0;

        ////}
        //private void Process_ZMStepReplay(int delay)
        //{

        //    for (int n = 0; n < m__G.oCam[0].dZoomStep_FrameCount; n++)
        //    {
        //        m__G.oCam[0].BufCopy2Disp_ZMStep(n);
        //        if (m__G.mCamCount > 1)
        //            m__G.oCam[1].BufCopy2Disp_ZMStep(n);
        //        Thread.Sleep(1 + delay);
        //    }
        //}

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        public void LEDOn(int port)
        {
            m__G.fGraph.Drive_LED(0, mLEDcurrent[0]);
            m__G.fGraph.Drive_LED(1, mLEDcurrent[1]);
        }

        public void LEDOff(int port)
        {
            m__G.fGraph.Drive_LED(0, 0);
            m__G.fGraph.Drive_LED(1, 0);
        }
        private void btnLEDUP_Click(object sender, EventArgs e)
        {
            //m_FocusedLED = 0;
            //MessageBox.Show("Focus Led : " + m_FocusedLED.ToString());
            m__G.mDoingStatus = "Checking Vision";

            int ch = m_FocusedLED;

            if (ch == 1)
            {
                if (tbLedLeft.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedLeft.Text);
            }
            else
            {
                if (tbLedRight.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedRight.Text);
            }

            if (mLEDcurrent[ch] > 0)
                mLEDcurrent[ch] -= 0.01;

            //m__G.oCam[0].HaltA();
            //if (m__G.mCamCount > 1)
            //    m__G.oCam[1].HaltA();

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");

            //StartLive();

        }

        private void btnLEDDOWN_Click(object sender, EventArgs e)
        {

            //m_FocusedLED = 0;
            //MessageBox.Show("Focus Led : " + m_FocusedLED.ToString());
            m__G.mDoingStatus = "Checking Vision";

            int ch = m_FocusedLED;

            if (ch == 1)
            {
                if (tbLedLeft.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedLeft.Text);
            }
            else
            {
                if (tbLedRight.Text.Length > 0)
                    mLEDcurrent[ch] = double.Parse(tbLedRight.Text);
            }


            if (mLEDcurrent[ch] < 5)
                mLEDcurrent[ch] += 0.01;

            //m__G.oCam[0].HaltA();
            //if (m__G.mCamCount > 1)
            //    m__G.oCam[1].HaltA();

            //m__G.fGraph.mDriverIC.SetLEDpowers((int)((mLEDcurrent[0] - 0.07) * 5000), (int)((mLEDcurrent[1] - 0.07) * 5000), m__G.mCamCount);
            m__G.fGraph.Drive_LED(ch, mLEDcurrent[ch]);

            tbLedLeft.Text = mLEDcurrent[1].ToString("F3");   //  Left
            tbLedRight.Text = mLEDcurrent[0].ToString("F3");   //  Right

            //StartLive();
        }

        private void btnFOVUp_Click(object sender, EventArgs e)
        {
            if (cbMotorized.Checked) return;    // Motor JOG Move

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();


            if (v_OrgROIV_min[Cam] > m_FovStep)
            {
                v_OrgROIV_min[Cam] -= m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVDown_Click(object sender, EventArgs e)
        {
            if (cbMotorized.Checked) return;    // Motor JOG Move

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int lROIVmax = v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam];
            if (lROIVmax < (1088 - m_FovStep))
            {
                v_OrgROIV_min[Cam] += m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVLeft_Click(object sender, EventArgs e)
        {
            if (cbMotorized.Checked) return;    // Motor JOG Move

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            if (v_OrgROIH_min[Cam] > m_FovStep)
            {
                v_OrgROIH_min[Cam] -= m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }

        private void btnFOVRight_Click(object sender, EventArgs e)
        {
            if (cbMotorized.Checked) return;    // Motor JOG Move

            int Cam = m_FocusedLED;
            if (m__G.mCamCount == 1)
                Cam = 0;

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int lROIHmax = v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam];
            if (lROIHmax < (2088 - m_FovStep))
            {
                v_OrgROIH_min[Cam] += m_FovStep;
            }
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
            SaveOrgROI(1);
        }
        public void SaveOrgROI(int camCount = 2)
        {
            while (m_bSaveOrgROI)
                Thread.Sleep(10);

            m_bSaveOrgROI = true;

            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastOrgROI" + camID0 + ".txt";
            StreamWriter wr = new StreamWriter(filename);
            for (int i = 0; i < camCount; i++)
            {
                wr.WriteLine(v_OrgROIH_min[i].ToString());
                wr.WriteLine((v_OrgROIH_min[i] + v_OrgROIH_width[i]).ToString());
                wr.WriteLine(v_OrgROIV_min[i].ToString());
                wr.WriteLine((v_OrgROIV_min[i] + v_OrgROIV_height[i]).ToString());
            }
            wr.Close();
            // YLUT 적용안함.
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            m_bSaveOrgROI = false;
        }

        //public void ReadVisionParam()
        //{
        //    string filename = "VisionParam.txt";
        //    if (!File.Exists(filename)) return;
        //    StreamReader sr = new StreamReader(filename);
        //    string allData = sr.ReadToEnd();
        //    sr.Close();
        //    string[] mylines = allData.Split("\n".ToCharArray());

        //    string[] figures = mylines[0].Split('\t');
        //    m_TopViewThresh = Convert.ToInt32(figures[0]);
        //    figures = mylines[1].Split('\t');
        //    m_TopViewMarkMax = Convert.ToInt32(figures[0]);
        //    figures = mylines[2].Split('\t');
        //    m_SideViewThresh = Convert.ToInt32(figures[0]);
        //    figures = mylines[3].Split('\t');
        //    m_SideViewMarkMin = Convert.ToInt32(figures[0]);
        //    figures = mylines[4].Split('\t');
        //    m_BlobAreaMin = Convert.ToInt32(figures[0]);
        //    figures = mylines[5].Split('\t');
        //    m_BlobAreaMax = Convert.ToInt32(figures[0]);
        //    figures = mylines[6].Split('\t');
        //    m_FakeMark = Convert.ToDouble(figures[0]);
        //}

        public void ReadOrgROI(int camCount = 2)
        {
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastOrgROI" + camID0 + ".txt";
            if (!File.Exists(filename)) return;
            StreamReader sr = new StreamReader(filename);
            int tmp = 0;
            for (int i = 0; i < camCount; i++)
            {
                v_OrgROIH_min[i] = Convert.ToInt32(sr.ReadLine());
                tmp = Convert.ToInt32(sr.ReadLine());
                v_OrgROIV_min[i] = Convert.ToInt32(sr.ReadLine());
                tmp = Convert.ToInt32(sr.ReadLine());
            }
            sr.Close();
        }
        //public void SaveZeroGap(int camCount = 2)
        //{
        //    string filename = m__G.m_RootDirectory + "\\DoNotTouch\\LastGap.txt";
        //    StreamWriter wr = new StreamWriter(filename);

        //    for (int i = 0; i < camCount; i++)
        //    {
        //        //m__G.mZeroXgap[0, 0] = mxL1gap[0] = LcamL2xgap;    //  Left Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[0, 1] = mxL2gap[0] = LcamL3xgap;    //  Left Cam Lens 3, X gap between Mark
        //        //m__G.mZeroXgap[1, 0] = mxL1gap[1] = RcamL2xgap;    //  Right Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[1, 1] = mxL2gap[1] = RcamL3xgap;    //  Right Cam Lens 3, X gap between Mark

        //        wr.WriteLine(m__G.mZeroXgap[i, 0].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroYgap[i, 0].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroXgap[i, 1].ToString("F4"));
        //        wr.WriteLine(m__G.mZeroYgap[i, 1].ToString("F4"));
        //    }
        //    wr.Close();
        //}
        //public void ReadZeroGap(int camCount = 2)
        //{
        //    string filename = "LastGap.txt";
        //    if (!File.Exists(filename)) return;
        //    StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);
        //    for (int i = 0; i < camCount; i++)
        //    {
        //        //m__G.mZeroXgap[0, 0] = mxL1gap[0] = LcamL2xgap;    //  Left Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[0, 1] = mxL2gap[0] = LcamL3xgap;    //  Left Cam Lens 3, X gap between Mark
        //        //m__G.mZeroXgap[1, 0] = mxL1gap[1] = RcamL2xgap;    //  Right Cam Lens 2, X gap between Mark
        //        //m__G.mZeroXgap[1, 1] = mxL2gap[1] = RcamL3xgap;    //  Right Cam Lens 3, X gap between Mark

        //        m__G.mZeroXgap[i, 0] = Convert.ToDouble(sr.ReadLine()); //  Lens 2  dx
        //        m__G.mZeroYgap[i, 0] = Convert.ToDouble(sr.ReadLine()); //  Lens 2  dy
        //        m__G.mZeroXgap[i, 1] = Convert.ToDouble(sr.ReadLine()); //  Lens 3  dx
        //        m__G.mZeroYgap[i, 1] = Convert.ToDouble(sr.ReadLine()); //  Lens 3  dy
        //    }
        //    sr.Close();

        //}

        private void btnLoadUnloadR_Click(object sender, EventArgs e)
        {

        }

        public List<double[]> mCalibrationFullData = new List<double[]>();  //  (um, min)
        public List<double[]> mGageFullData = new List<double[]>();         //  (um)
        public List<double[]> mPrismTXTYTZ = new List<double[]>();         //  (um)
        public List<double[]> mStdevTXTYTZ = new List<double[]>();  //  (um, min)
        //public List<double[]> mProbeTZ = new List<double[]>();

        //public void JHMotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        //{
        //    m__G.mDoingStatus = "Checking Vision";

        //    int mavNum = 4;

        //    m__G.oCam[0].mTargetTriggerCount = 3000;
        //    m__G.oCam[0].dAFZM_FrameCount = 9;
        //    m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
        //    double[] gageData = null;

        //    for (int mi = 0; mi < 5; mi++)
        //        m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

        //    /////////////////////////////////////////////////////////////////////////
        //    // 자화로 대체
        //    // gageData = 현재 값 읽어오기;
        //    ///////////////////////////////////////////////////////////////////////////////

        //    for (int i = 0; i < mavNum; i++)
        //        m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

        //    if (IsOrg)
        //    {
        //        m__G.oCam[0].mFAL.LoadFMICandidate();
        //        m__G.oCam[0].mFAL.BackupFMI();
        //        SetDefaultMarkConfig(true);
        //    }

        //    double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
        //    double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

        //    m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

        //    //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
        //    string[] strtmp = new string[2] { "", "" };
        //    m__G.mbSuddenStop[0] = false;
        //    TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

        //    double[] lCalibrationData = new double[23];

        //    int ci = 0;
        //    strtmp[ci] = "";
        //    m__G.mFAL.mCandidateIndex = ci;

        //    if (IsOrg)
        //        ChangeFiducialMark(ci);

        //    if (IsOrg)
        //    {

        //        m__G.oCam[0].PrepareFineCOG();
        //        m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
        //        m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
        //    }
        //    m__G.oCam[0].FineCOG(false, 1, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 2, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 3, 0);    // 마크찾기
        //    m__G.oCam[0].FineCOG(false, 4, 0);    // 마크찾기

        //    if (ci != 0)
        //        m__G.mFAL.mFZM.mbCompY = ci;
        //    else
        //        m__G.mFAL.mFZM.mbCompY = 0;
        //    double sx = 0;
        //    double sy = 0;
        //    double sz = 0;
        //    double tx = 0;
        //    double ty = 0;
        //    double tz = 0;

        //    for (int findex = 1; findex < mavNum + 1; findex++)
        //    {
        //        //NthMeasure(findex, true);

        //        //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

        //        sx += m__G.oCam[0].mC_pX[findex] * umscale;
        //        sy += m__G.oCam[0].mC_pY[findex] * umscale;
        //        sz += m__G.oCam[0].mC_pZ[findex] * umscale;
        //        tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  radian to minute
        //        ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  radian to minute
        //        tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  radian to minute
        //    }
        //    lCalibrationData[0] = sx;
        //    lCalibrationData[1] = sy;
        //    lCalibrationData[2] = sz;
        //    lCalibrationData[3] = tx;
        //    lCalibrationData[4] = ty;
        //    lCalibrationData[5] = tz;

        //    strtmp[ci] = Nth.ToString() + "\t"
        //           + lCalibrationData[0].ToString("F2") + "\t"
        //           + lCalibrationData[1].ToString("F2") + "\t"
        //           + lCalibrationData[2].ToString("F2") + "\t"
        //           + lCalibrationData[3].ToString("F2") + "\t"
        //           + lCalibrationData[4].ToString("F2") + "\t"
        //           + lCalibrationData[5].ToString("F2") + "\t";

        //    double[] xavg = new double[12];
        //    double[] yavg = new double[12];
        //    for (int findex = 1; findex < mavNum + 1; findex++)
        //    {
        //        for (int i = 0; i < 12; i++)
        //        {
        //            if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
        //            xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
        //            yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
        //        }
        //    }
        //    int kk = 0;
        //    for (int i = 0; i < 12; i++)
        //    {
        //        if (xavg[i] == 0) continue;
        //        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

        //        lCalibrationData[6 + 2 * kk] = xavg[i];
        //        lCalibrationData[6 + 2 * kk + 1] = yavg[i];
        //        kk++;
        //    }
        //    if (gageData != null)
        //    {
        //        if (gageData.Length == 6)
        //        {
        //            // 자화 부호 맞춰야함.
        //            lCalibrationData[16] = gageData[0];   // um
        //            lCalibrationData[17] = gageData[1];   // um
        //            lCalibrationData[18] = gageData[2];   // um
        //            lCalibrationData[19] = gageData[3]; // min          // TX   acr min
        //            lCalibrationData[20] = gageData[4]; // min          // TY   acr min
        //            lCalibrationData[21] = gageData[5]; // min          // TZ   acr min

        //            //  출력은 um, arcmin
        //            strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1")
        //                 + "\t" + lCalibrationData[19].ToString("F1") + "\t" + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1");
        //        }
        //    }
        //    if (ci == 0 && IsSave)
        //    {
        //        mCalibrationFullData.Add(lCalibrationData);
        //        mGageFullData.Add(gageData);
        //    }


        //    if (InvokeRequired)
        //    {
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            DrawMarkDetected();
        //            //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
        //            if (ltextBox[0].Text.Length > 7000)
        //                ltextBox[0].Text = strtmp[0] + "\r\n";
        //            else
        //                ltextBox[0].Text += strtmp[0] + "\r\n";

        //            ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
        //            ltextBox[0].ScrollToCaret();
        //        });
        //    }
        //    else
        //    {
        //        DrawMarkDetected();
        //        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

        //        if (ltextBox[0].Text.Length > 7000)
        //            ltextBox[0].Text = strtmp[0] + "\r\n";
        //        else
        //            ltextBox[0].Text += strtmp[0] + "\r\n";

        //        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
        //        ltextBox[0].ScrollToCaret();
        //    }
        //    //if (InvokeRequired)
        //    //{
        //    //    BeginInvoke((MethodInvoker)delegate
        //    //    {
        //    //        if (ltextBox[1].Text.Length > 7000)
        //    //            ltextBox[1].Text = strtmp[1] + "\r\n";
        //    //        else
        //    //            ltextBox[1].Text += strtmp[1] + "\r\n";

        //    //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
        //    //        ltextBox[1].ScrollToCaret();
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    if (ltextBox[1].Text.Length > 2000)
        //    //        ltextBox[1].Text = strtmp[1] + "\r\n";
        //    //    else
        //    //        ltextBox[1].Text += strtmp[1] + "\r\n";

        //    //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
        //    //    ltextBox[1].ScrollToCaret();
        //    //}

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();
        //    m__G.mDoingStatus = "IDLE";
        //    m__G.mIDLEcount = 0;
        //}

        public double GetStdevOfArray(double[] x, int startIndex, int count)
        {
            double res = 0;
            double mean = 0;
            for ( int i= startIndex; i < startIndex+count; i++ )
            {
                mean += x[i];
            }
            mean /= count;
            double SumOfSquare = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                SumOfSquare += Math.Pow(x[i] - mean,2);
            }
            res = Math.Sqrt(SumOfSquare/count);
            return res;
        }
        public void MotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 16;  //4
            if ( m__G.m_bPrismCS)
                mavNum = 4;  //4

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;
            //double[] gageData2 = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            //double[] lProbeTZ = new double[2];
                  //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;

                if (m__G.m_bCalibrationModel)
                {
                    gageData = m__G.mGageCounter.ReadPortAll();
                               // Probe TZ 튀는 현상 디버깅
                    //lProbeTZ[0] = m__G.mGageCounter.probeTZ[0];  
                    //lProbeTZ[1] = m__G.mGageCounter.probeTZ[1];
                    //if (mGageFullData.Count != 0)
                    //{
                    //    if (m__G.m_bPrismCS)
                    //    {
                    //        double error = Math.Abs(mGageFullData[mGageFullData.Count - 1][3] - gageData[3]);

                    //        if (error > 2.62)
                    //        { 
                    //            gageData = m__G.mGageCounter.ReadPortAll();
                    //        }
                    //    }
                    //}
                }
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            //if ( m__G.m_bPrismCS )
            //{
            //    if (m__G.mGageCounter != null)
            //    {
            //        m__G.mGageCounter.m__G = m__G;
            //        if (m__G.m_bCalibrationModel)
            //            gageData2 = m__G.mGageCounter.ReadPortAll();
            //    }
            //}

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  pixel to um

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[25]; // 33
            double[] lStdevTXTYTZ = new double[6];
            double[] lPrismTXTYTZ = new double[3];

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [0. Probe TY1, TY2 평균] , [1. X거리, Y거리에 따른 Z 보정 (최종)]
            //double[] probeZ = new double[2];
            //***********************************//
            

            int ci = 0;
            strtmp[ci] = "";
            m__G.mFAL.mCandidateIndex = ci;

            if (IsOrg)
                ChangeFiducialMark(ci);

            if (IsOrg)
            {

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
                m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
            }

            //if (m__G.m_bPrismCS)
            //{
            //    Task[] taskArray = new Task[5];

            //    m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));
            //    int count = mavNum + 1;
            //    Task task1 = Task.Run(() => {
            //        for (int findex = 1; findex < mavNum + 1; findex+=5)
            //            m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            //    });
            //    taskArray[0] = task1;
            //    Task task2 = Task.Run(() => {
            //        for (int findex = 2; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 1);    // 마크찾기
            //    });
            //    taskArray[1] = task2;
            //    Task task3 = Task.Run(() => {
            //        for (int findex = 3; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 2);    // 마크찾기
            //    });
            //    taskArray[2] = task3;
            //    Task task4 = Task.Run(() => {
            //        for (int findex = 4; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 3);    // 마크찾기
            //    });
            //    taskArray[3] = task4;
            //    Task task5 = Task.Run(() => {
            //        for (int findex = 5; findex < mavNum + 1; findex += 5)
            //            m__G.oCam[0].FineCOG(false, findex, 4);    // 마크찾기
            //    });
            //    taskArray[4] = task5;
            //    Task.WaitAll(taskArray);
            //}
            //else
            //{
            for (int findex = 1; findex < mavNum + 1; findex++)
                m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            //}


            if (ci != 0)
                m__G.mFAL.mFZM.mbCompY = ci;
            else
                m__G.mFAL.mFZM.mbCompY = 0;

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;

            //double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            //double[] lErrorPrismTXTYTZ = new double[3];
            bool bGageOn = false;

            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                //NthMeasure(findex, true);

                //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                sx += m__G.oCam[0].mC_pX[findex] * umscale;
                sy += m__G.oCam[0].mC_pY[findex] * umscale;
                sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  Radian to minute / mavNum
                ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  Radian to minute / mavNum
                tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  Radian to minute / mavNum
            }
            if (m__G.m_bPrismCS)
            {
                lStdevTXTYTZ[0] = GetStdevOfArray(m__G.oCam[0].mC_pTX, 1, mavNum) * RAD_To_MIN; //  Radian to Arcminute
                lStdevTXTYTZ[1] = GetStdevOfArray(m__G.oCam[0].mC_pTY, 1, mavNum) * RAD_To_MIN;
                lStdevTXTYTZ[2] = GetStdevOfArray(m__G.oCam[0].mC_pTZ, 1, mavNum) * RAD_To_MIN;

                //  CSHead 좌표계 기준으로 계산된 평균치만 Prism 좌표계로 변환해서 lPrismTXTYTZ 에 저장
                lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                tx = lPrismTXTYTZ[1];
                lCalibrationData[22] = lPrismTXTYTZ[0];
                lCalibrationData[23] = lPrismTXTYTZ[1];
                lCalibrationData[24] = lPrismTXTYTZ[2];

            }

            lCalibrationData[0] = sx;//-sx;
            lCalibrationData[1] = sy;//sy;
            lCalibrationData[2] = sz;//-sz;
            lCalibrationData[3] = tx;//tx;
            lCalibrationData[4] = ty;//-ty;
            lCalibrationData[5] = tz;//-tz;

            //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
            strtmp[ci] = Nth.ToString() + "\t"
                   + lCalibrationData[0].ToString("F2") + "\t"
                   + lCalibrationData[1].ToString("F2") + "\t"
                   + lCalibrationData[2].ToString("F2") + "\t"
                   + lCalibrationData[3].ToString("F2") + "\t"
                   + lCalibrationData[4].ToString("F2") + "\t"
                   + lCalibrationData[5].ToString("F2") + "\t";

            double[] xavg = new double[12];
            double[] yavg = new double[12];
            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                    xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                    yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                }
            }
            int kk = 0;
            for (int i = 0; i < 12; i++)
            {
                if (xavg[i] == 0) continue;
                strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                lCalibrationData[6 + 2 * kk] = xavg[i];
                lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                kk++;
            }
            if (gageData != null)
            {
                if (gageData.Length == 7)
                {
                    bGageOn = true;

                    double[] XYTz = new double[3];
                    XYTz[0] = gageData[0];
                    XYTz[1] = gageData[1];
                    XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                    double[] TxTyZ = new double[3];
                    TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                    TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                    TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                    double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);

                    lCalibrationData[16] = XYTz[0]; // um
                    lCalibrationData[17] = XYTz[1]; // um
                    lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                    lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian -> min으로 통일
                    lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian -> min으로 통일
                    lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian -> min으로 통일  241216 부호 변경

                    //if (m__G.m_bPrismCS)
                    //{
                    //    double[] XYTz2 = new double[3];
                    //    double[] TxTyZ2 = new double[3];
                    //    XYTz2[0] = gageData2[0];
                    //    XYTz2[1] = gageData2[1];
                    //    XYTz2[2] = Math.Atan((gageData2[2] - gageData2[3]) / 45000);  //  45mm
                    //    TxTyZ2[0] = Math.Atan((gageData2[4] - (gageData2[5] + gageData2[6]) / 2) / 83000);  //  83mm
                    //    TxTyZ2[1] = Math.Atan((gageData2[5] - gageData2[6]) / 120000);  //  120mm
                    //    TxTyZ2[2] = (gageData2[5] + gageData2[6]) / 2;  //  120mm
                    //    double compZ2 = ProbeZcompensationForTXTY(XYTz2[0], XYTz2[1], TxTyZ2[2], TxTyZ2[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ2[1] /*- mPorg.TY * MIN_To_RAD*/);

                    //    //double[] lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(TxTyZ2[0], TxTyZ2[1], XYTz2[2]);

                    //    lCalibrationData[16] = (XYTz[0]   + XYTz2[0] )/2     ; // um
                    //    lCalibrationData[17] = (XYTz[1]   + XYTz2[1] )/2     ; // um
                    //    lCalibrationData[18] = (compZ     + compZ2   )/2     ;// TxTyZ[2]; // um
                    //    lCalibrationData[19] = (TxTyZ[0]  + TxTyZ2[0]) * RAD_To_MIN / 2;       // TX   radian -> min으로 통일
                    //    lCalibrationData[20] = (TxTyZ[1]  + TxTyZ2[1]) * RAD_To_MIN / 2;       // TY   radian -> min으로 통일
                    //    lCalibrationData[21] = (-XYTz[2]  - XYTz2[2] ) * RAD_To_MIN / 2;       // TZ   radian -> min으로 통일  241216 부호 변경

                    //    lStdevTXTYTZ[3] = Math.Abs(0.7071*(TxTyZ[0] - TxTyZ2[0])) * RAD_To_MIN; //  Radian to Arcminute
                    //    lStdevTXTYTZ[4] = Math.Abs(0.7071*(TxTyZ[1] - TxTyZ2[1])) * RAD_To_MIN;
                    //    lStdevTXTYTZ[5] = Math.Abs(0.7071*(XYTz[2] - XYTz2[2])) * RAD_To_MIN;
                    //}

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [0. Probe TY1, TY2 평균]
                    //probeZ[0] = TxTyZ[2];               //  저장 0
                    //***********************************//

                    //
                    // Hexapod
                    //
                    //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                    //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                    //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                    //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                    //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                    //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                    //
                    //

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [1. X거리, Y거리에 따른 Z 보정 (최종)]
                    //probeZ[1] = compZ;               //  저장 1
                    //lCalibrationData[22] = m__G.mGageCounter.probeTY1[0];  // [0.첫 Probe 값] 
                    //lCalibrationData[23] = m__G.mGageCounter.probeTY2[0];
                    //lCalibrationData[24] = m__G.mGageCounter.probeTY1[1];  // [1.X, Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정]
                    //lCalibrationData[25] = m__G.mGageCounter.probeTY2[1];
                    //lCalibrationData[26] = m__G.mGageCounter.probeTY1[2];  //  [2.Glass 두께에 따른 Probe보정]
                    //lCalibrationData[27] = m__G.mGageCounter.probeTY2[2];
                    //lCalibrationData[28] = probeZ[0];  // [0.Probe TY1, TY2 평균] 
                    //lCalibrationData[29] = probeZ[1];  // [1.X거리, Y거리에 따른 Z 보정(최종)]
                    //lCalibrationData[30] = m__G.mGageCounter.probeTX[0];
                    //lCalibrationData[31] = m__G.mGageCounter.probeTX[1];
                    //lCalibrationData[32] = m__G.mGageCounter.probeTX[2];
                    //***********************************//

                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + lCalibrationData[19].ToString("F1") + "\t"
                        + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1") + "\t";


                    //double[] motorCurpos = MotorCurPos6D();
                    //strtmp[ci] += $"{motorCurpos[0]:F1}\t{motorCurpos[1]:F1}\t{motorCurpos[2]:F1}\t{motorCurpos[3]:F1}\t{motorCurpos[4]:F1}\t{motorCurpos[5]:F1}";


                }

                if (m__G.m_bPrismCS)
                {
                    // lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[3], lCalibrationData[4], lCalibrationData[5], true);

                    if (bGageOn)
                    {
                        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                        lProbePrismTXTYTZ[1] = lCalibrationData[19];

                        lCalibrationData[22] = lProbePrismTXTYTZ[0];    //  p45 TX
                        lCalibrationData[23] = lProbePrismTXTYTZ[1];    //  p45 TY
                        lCalibrationData[24] = lProbePrismTXTYTZ[2];    //  p45 TZ
                        /////////////////////////////////////////////////////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////////////////////////////////////////////////////
                        ////  개별데이터  저장목적
                        //if ( File.Exists(mDataFile100))
                        //{
                        //    string saveStr = "";
                        //    StreamWriter writer = File.AppendText(mDataFile100);
                        //    for (int findex = 1; findex < mavNum + 1; findex++)
                        //    {
                        //        tx = m__G.oCam[0].mC_pTX[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        ty = m__G.oCam[0].mC_pTY[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        tz = m__G.oCam[0].mC_pTZ[findex] * minscale * mavNum;   //  Radian to minute / mavNum
                        //        double [] lPrismTXTYTZ100 = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                        //        tx = lPrismTXTYTZ100[1];
                        //        saveStr = lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                        //                    lPrismTXTYTZ100[0].ToString("F5") + "," + lPrismTXTYTZ100[1].ToString("F5") + "," + lPrismTXTYTZ100[2].ToString("F5") + ",";
                        //        writer.WriteLine(saveStr);
                        //    }
                        //    writer.Close();
                        //    ///////////////////////////////////////////////////////////////////////////////////////////////////
                        //    ///////////////////////////////////////////////////////////////////////////////////////////////////

                        //}

                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                    }
                    else
                    {
                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                    }
                }
            }
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
                mStdevTXTYTZ.Add(lStdevTXTYTZ);
                mPrismTXTYTZ.Add(lPrismTXTYTZ);
                        // 디버깅
                //mProbeTZ.Add(lProbeTZ);

            }


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    if (ltextBox[0].Text.Length > 10000)
                        ltextBox[0].Text = strtmp[0] + "\r\n";
                    else
                        ltextBox[0].Text += strtmp[0] + "\r\n";

                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                });
            }
            else
            {
                DrawMarkDetected();

                if (ltextBox[0].Text.Length > 10000)
                    ltextBox[0].Text = strtmp[0] + "\r\n";
                else
                    ltextBox[0].Text += strtmp[0] + "\r\n";

                ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                ltextBox[0].ScrollToCaret();
            }
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void MotorizedFindMarksAnosis(ref double [] lCalibrationData, int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 16;  //4
            if (m__G.m_bPrismCS)
                mavNum = 100;  //4

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;
            double[] gageData2 = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                if (m__G.m_bCalibrationModel)
                    gageData = m__G.mGageCounter.ReadPortAll();
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            if (m__G.m_bPrismCS)
            {
                if (m__G.mGageCounter != null)
                {
                    m__G.mGageCounter.m__G = m__G;
                    if (m__G.m_bCalibrationModel)
                        gageData2 = m__G.mGageCounter.ReadPortAll();
                }
            }

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  pixel to um

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lStdevTXTYTZ = new double[6];
            double[] lPrismTXTYTZ = new double[3];

            //* 250404 Probe Z(TY1, TY2) 디버깅 *//
            // [0. Probe TY1, TY2 평균] , [1. X거리, Y거리에 따른 Z 보정 (최종)]
            //double[] probeZ = new double[2];
            //***********************************//

            int ci = 0;
            strtmp[ci] = "";
            m__G.mFAL.mCandidateIndex = ci;

            if (IsOrg)
                ChangeFiducialMark(ci);

            if (IsOrg)
            {

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
                m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기
            }

            if (m__G.m_bPrismCS)
            {
                Task[] taskArray = new Task[5];

                m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));
                int count = mavNum + 1;
                Task task1 = Task.Run(() => {
                    for (int findex = 1; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
                });
                taskArray[0] = task1;
                Task task2 = Task.Run(() => {
                    for (int findex = 2; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 1);    // 마크찾기
                });
                taskArray[1] = task2;
                Task task3 = Task.Run(() => {
                    for (int findex = 3; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 2);    // 마크찾기
                });
                taskArray[2] = task3;
                Task task4 = Task.Run(() => {
                    for (int findex = 4; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 3);    // 마크찾기
                });
                taskArray[3] = task4;
                Task task5 = Task.Run(() => {
                    for (int findex = 5; findex < mavNum + 1; findex += 5)
                        m__G.oCam[0].FineCOG(false, findex, 4);    // 마크찾기
                });
                taskArray[4] = task5;
                Task.WaitAll(taskArray);
            }
            else
            {
                for (int findex = 1; findex < mavNum + 1; findex++)
                    m__G.oCam[0].FineCOG(false, findex, 0);    // 마크찾기
            }


            if (ci != 0)
                m__G.mFAL.mFZM.mbCompY = ci;
            else
                m__G.mFAL.mFZM.mbCompY = 0;

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;

            //double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            //double[] lErrorPrismTXTYTZ = new double[3];
            //bool bGageOn = false;

            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                //NthMeasure(findex, true);

                //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                sx += m__G.oCam[0].mC_pX[findex] * umscale;
                sy += m__G.oCam[0].mC_pY[findex] * umscale;
                sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                tx += m__G.oCam[0].mC_pTX[findex] * minscale;   //  Radian to minute / mavNum
                ty += m__G.oCam[0].mC_pTY[findex] * minscale;   //  Radian to minute / mavNum
                tz += m__G.oCam[0].mC_pTZ[findex] * minscale;   //  Radian to minute / mavNum
            }
            if (m__G.m_bPrismCS)
            {
                lStdevTXTYTZ[0] = GetStdevOfArray(m__G.oCam[0].mC_pTX, 1, mavNum) * RAD_To_MIN; //  Radian to Arcminute
                lStdevTXTYTZ[1] = GetStdevOfArray(m__G.oCam[0].mC_pTY, 1, mavNum) * RAD_To_MIN;
                lStdevTXTYTZ[2] = GetStdevOfArray(m__G.oCam[0].mC_pTZ, 1, mavNum) * RAD_To_MIN;

                lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                tx = lPrismTXTYTZ[1];
                lCalibrationData[22] = lPrismTXTYTZ[0];
                lCalibrationData[23] = lPrismTXTYTZ[1];
                lCalibrationData[24] = lPrismTXTYTZ[2];
            }


            lCalibrationData[0] = sx;//-sx;
            lCalibrationData[1] = sy;//sy;
            lCalibrationData[2] = sz;//-sz;
            lCalibrationData[3] = tx;//tx;
            lCalibrationData[4] = ty;//-ty;
            lCalibrationData[5] = tz;//-tz;

            //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
            strtmp[ci] = Nth.ToString() + "\t"
                   + lCalibrationData[0].ToString("F2") + "\t"
                   + lCalibrationData[1].ToString("F2") + "\t"
                   + lCalibrationData[2].ToString("F2") + "\t"
                   + lCalibrationData[3].ToString("F2") + "\t"
                   + lCalibrationData[4].ToString("F2") + "\t"
                   + lCalibrationData[5].ToString("F2") + "\t";

            double[] xavg = new double[12];
            double[] yavg = new double[12];
            for (int findex = 1; findex < mavNum + 1; findex++)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                    xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                    yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                }
            }
            int kk = 0;
            for (int i = 0; i < 12; i++)
            {
                if (xavg[i] == 0) continue;
                strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                lCalibrationData[6 + 2 * kk] = xavg[i];
                lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                kk++;
            }
            if (gageData != null)
            {
                if (gageData.Length == 7)
                {
                    double[] XYTz = new double[3];
                    XYTz[0] = gageData[0];
                    XYTz[1] = gageData[1];
                    XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                    double[] TxTyZ = new double[3];
                    TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                    TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                    TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                    double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);

                    lCalibrationData[16] = XYTz[0]; // um
                    lCalibrationData[17] = XYTz[1]; // um
                    lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                    lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian -> min으로 통일
                    lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian -> min으로 통일
                    lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian -> min으로 통일  241216 부호 변경

                    if (m__G.m_bPrismCS)
                    {
                        double[] XYTz2 = new double[3];
                        double[] TxTyZ2 = new double[3];
                        XYTz2[0] = gageData2[0];
                        XYTz2[1] = gageData2[1];
                        XYTz2[2] = Math.Atan((gageData2[2] - gageData2[3]) / 45000);  //  45mm
                        TxTyZ2[0] = Math.Atan((gageData2[4] - (gageData2[5] + gageData2[6]) / 2) / 83000);  //  83mm
                        TxTyZ2[1] = Math.Atan((gageData2[5] - gageData2[6]) / 120000);  //  120mm
                        TxTyZ2[2] = (gageData2[5] + gageData2[6]) / 2;  //  120mm
                        double compZ2 = ProbeZcompensationForTXTY(XYTz2[0], XYTz2[1], TxTyZ2[2], TxTyZ2[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ2[1] /*- mPorg.TY * MIN_To_RAD*/);

                        //double[] lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(TxTyZ2[0], TxTyZ2[1], XYTz2[2]);

                        lCalibrationData[16] = (XYTz[0] + XYTz2[0]) / 2; // um
                        lCalibrationData[17] = (XYTz[1] + XYTz2[1]) / 2; // um
                        lCalibrationData[18] = (compZ + compZ2) / 2;// TxTyZ[2]; // um
                        lCalibrationData[19] = (TxTyZ[0] + TxTyZ2[0]) * RAD_To_MIN / 2;       // TX   radian -> min으로 통일
                        lCalibrationData[20] = (TxTyZ[1] + TxTyZ2[1]) * RAD_To_MIN / 2;       // TY   radian -> min으로 통일
                        lCalibrationData[21] = (-XYTz[2] - XYTz2[2]) * RAD_To_MIN / 2;       // TZ   radian -> min으로 통일  241216 부호 변경

                        lStdevTXTYTZ[3] = Math.Abs(0.7071 * (TxTyZ[0] - TxTyZ2[0])) * RAD_To_MIN; //  Radian to Arcminute
                        lStdevTXTYTZ[4] = Math.Abs(0.7071 * (TxTyZ[1] - TxTyZ2[1])) * RAD_To_MIN;
                        lStdevTXTYTZ[5] = Math.Abs(0.7071 * (XYTz[2] - XYTz2[2])) * RAD_To_MIN;
                    }

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [0. Probe TY1, TY2 평균]
                    //probeZ[0] = TxTyZ[2];               //  저장 0
                    //***********************************//






                    //
                    // Hexapod
                    //
                    //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                    //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                    //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                    //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                    //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                    //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                    //
                    //

                    //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                    // [1. X거리, Y거리에 따른 Z 보정 (최종)]
                    //probeZ[1] = compZ;               //  저장 1
                    //lCalibrationData[22] = m__G.mGageCounter.probeTY1[0];  // [0.첫 Probe 값] 
                    //lCalibrationData[23] = m__G.mGageCounter.probeTY2[0];
                    //lCalibrationData[24] = m__G.mGageCounter.probeTY1[1];  // [1.X, Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정]
                    //lCalibrationData[25] = m__G.mGageCounter.probeTY2[1];
                    //lCalibrationData[26] = m__G.mGageCounter.probeTY1[2];  //  [2.Glass 두께에 따른 Probe보정]
                    //lCalibrationData[27] = m__G.mGageCounter.probeTY2[2];
                    //lCalibrationData[28] = probeZ[0];  // [0.Probe TY1, TY2 평균] 
                    //lCalibrationData[29] = probeZ[1];  // [1.X거리, Y거리에 따른 Z 보정(최종)]
                    //lCalibrationData[30] = m__G.mGageCounter.probeTX[0];
                    //lCalibrationData[31] = m__G.mGageCounter.probeTX[1];
                    //lCalibrationData[32] = m__G.mGageCounter.probeTX[2];
                    //***********************************//

                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + lCalibrationData[19].ToString("F1") + "\t"
                        + lCalibrationData[20].ToString("F1") + "\t" + lCalibrationData[21].ToString("F1") + "\t";


                    //double[] motorCurpos = MotorCurPos6D();
                    //strtmp[ci] += $"{motorCurpos[0]:F1}\t{motorCurpos[1]:F1}\t{motorCurpos[2]:F1}\t{motorCurpos[3]:F1}\t{motorCurpos[4]:F1}\t{motorCurpos[5]:F1}";


                }

                //if (m__G.m_bPrismCS)
                //{
                //    lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[3], lCalibrationData[4], lCalibrationData[5], true);
                //    if (bGageOn)
                //    {
                //        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                //        lProbePrismTXTYTZ[1] = lCalibrationData[19];


                //        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                //                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                //    }
                //    else
                //    {
                //        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                //    }
                //}
            }

            //mCalibrationFullData[0] = lCalibrationData;
            if (ci == 0 && IsSave)
            {
                mCalibrationFullData.Add(lCalibrationData);
                mGageFullData.Add(gageData);
                mStdevTXTYTZ.Add(lStdevTXTYTZ);
                mPrismTXTYTZ.Add(lPrismTXTYTZ);

            }

            if (!m__G.m_bHideAllGraph)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();
                        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                        if (ltextBox[0].Text.Length > 10000)
                            ltextBox[0].Text = strtmp[0] + "\r\n";
                        else
                            ltextBox[0].Text += strtmp[0] + "\r\n";

                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

                    if (ltextBox[0].Text.Length > 10000)
                        ltextBox[0].Text = strtmp[0] + "\r\n";
                    else
                        ltextBox[0].Text += strtmp[0] + "\r\n";

                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void RemoteManualFindMark()
        {
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);
            ManualFindMarks(0, false, false);

            m__G.fGraph.Drive_LEDs(0, 0);
        }

        public void JHManualFindMarks(int Nth, bool IsShowResult = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 4;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            /// 다음 자화로 대체 필요
            // gageData = 현재 값 읽어오기;
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[23];

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                strtmp[ci] = "";
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);
                ProcessVisionData(mavNum + 1, 2);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    sx += m__G.oCam[0].mC_pX[findex] * umscale;
                    sy += m__G.oCam[0].mC_pY[findex] * umscale;
                    sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx += m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty += m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz += m__G.oCam[0].mC_pTZ[findex] * minscale;
                }
                m__G.oCam[0].mC_pX[0] = sx;
                m__G.oCam[0].mC_pY[0] = sy;
                m__G.oCam[0].mC_pZ[0] = sz;
                m__G.oCam[0].mC_pTX[0] = tx;
                m__G.oCam[0].mC_pTY[0] = ty;
                m__G.oCam[0].mC_pTZ[0] = tz;

                //  부호 원상복귀
                lCalibrationData[0] = sx;//-sx;
                lCalibrationData[1] = sy;//sy;
                lCalibrationData[2] = sz;//-sz;
                lCalibrationData[3] = tx;//tx;
                lCalibrationData[4] = ty;//-ty;
                lCalibrationData[5] = tz;//-tz;
                if (IsShowResult)
                    strtmp[ci] = Nth.ToString() + "\t"
                                + lCalibrationData[0].ToString("F2") + "\t"
                                + lCalibrationData[1].ToString("F2") + "\t"
                                + lCalibrationData[2].ToString("F2") + "\t"
                                + lCalibrationData[3].ToString("F2") + "\t"
                                + lCalibrationData[4].ToString("F2") + "\t"
                                + lCalibrationData[5].ToString("F2") + "\t";
                double[] xavg = new double[12];
                double[] yavg = new double[12];
                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                        yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                    }
                }
                int kk = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (xavg[i] == 0) continue;
                    if (IsShowResult)
                        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                    lCalibrationData[6 + 2 * kk] = xavg[i];
                    lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                    kk++;
                }
                if (gageData != null && gageData.Length > 0)
                {
                    if (gageData.Length == 6)
                    {
                        // 자화 부호, 단위 맞춰야함.
                        lCalibrationData[16] = gageData[0];     // um
                        lCalibrationData[17] = -gageData[1];    // um
                        lCalibrationData[18] = -gageData[2];   // um
                        lCalibrationData[19] = -gageData[3];    // * Math.PI / 180; //xTyZ[0];           // TX   radian
                        lCalibrationData[20] = gageData[4]; // * Math.PI / 180; //TxTyZ[1];          // TY   radian
                        lCalibrationData[21] = -gageData[5];    // * Math.PI / 180; //XYTz[2];           // TZ   radian

                        if (IsShowResult)
                            strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                                + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");
                    }
                }

                if (ci == 0)
                {
                    mCalibrationFullData.Add(lCalibrationData);
                    mGageFullData.Add(lCalibrationData);
                }
            }


            if (IsShowResult)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();

                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();

                    ltextBox[0].Text += strtmp[0] + "\r\n";
                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
                //if (InvokeRequired)
                //{
                //    BeginInvoke((MethodInvoker)delegate
                //    {
                //        ltextBox[1].Text += strtmp[1] + "\r\n";
                //        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //        ltextBox[1].ScrollToCaret();
                //    });
                //}
                //else
                //{
                //    ltextBox[1].Text += strtmp[1] + "\r\n";
                //    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                //    ltextBox[1].ScrollToCaret();
                //}
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void ManualFindMarks(int Nth, bool IsShowResult = true, bool fromFirst = false)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 2;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            /// 다음 HEXAPOD 로 대체 필요
            /// 
            //gageData = MotorCurLogicPos6D();

            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                if (m__G.m_bCalibrationModel)
                    gageData = m__G.mGageCounter.ReadPortAll(); //  gageData[6] = { X1,X2,Y1,Y2,TX,TY1,TY2 }
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            if (fromFirst)
            {
                for (int i = 0; i < mavNum + 1; i++)
                    m__G.oCam[0].GrabB(i, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지
            }
            else
            {
                for (int i = 0; i < mavNum; i++)
                    m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지
            }

            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname, -1);
            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[25];
            double[] lPrismTXTYTZ = new double[3];
            double[] lProbePrismTXTYTZ = new double[3];
            double[] lErrorPrismTXTYTZ = new double[3];
            bool bGageOn = false;

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                strtmp[ci] = "";
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);
                ProcessVisionData(mavNum + 1, 2);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    //NthMeasure(findex, true);

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                    sx += m__G.oCam[0].mC_pX[findex] * umscale;
                    sy += m__G.oCam[0].mC_pY[findex] * umscale;
                    sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx += m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty += m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz += m__G.oCam[0].mC_pTZ[findex] * minscale;
                }
                m__G.oCam[0].mC_pX[0] = sx;
                m__G.oCam[0].mC_pY[0] = sy;
                m__G.oCam[0].mC_pZ[0] = sz;
                m__G.oCam[0].mC_pTX[0] = tx;
                m__G.oCam[0].mC_pTY[0] = ty;
                m__G.oCam[0].mC_pTZ[0] = tz;

                //  부호 원상복귀
                lCalibrationData[0] = sx;//-sx;
                lCalibrationData[1] = sy;//sy;
                lCalibrationData[2] = sz;//-sz;
                lCalibrationData[3] = tx;//tx;
                lCalibrationData[4] = ty;//-ty;
                lCalibrationData[5] = tz;//-tz;


                if (IsShowResult)
                    //strtmp[ci] = Nth.ToString() + "\t" + sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
                    strtmp[ci] = Nth.ToString() + "\t"
                                + lCalibrationData[0].ToString("F2") + "\t"
                                + lCalibrationData[1].ToString("F2") + "\t"
                                + lCalibrationData[2].ToString("F2") + "\t"
                                + lCalibrationData[3].ToString("F2") + "\t"
                                + lCalibrationData[4].ToString("F2") + "\t"
                                + lCalibrationData[5].ToString("F2") + "\t";

                double[] xavg = new double[12];
                double[] yavg = new double[12];
                for (int findex = 1; findex < mavNum + 1; findex++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        xavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].X / mavNum;
                        yavg[i] += m__G.oCam[0].mAzimuthPts[findex][i].Y / mavNum;
                    }
                }
                int kk = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (xavg[i] == 0) continue;
                    if (IsShowResult)
                        strtmp[ci] += xavg[i].ToString("F3") + "\t" + yavg[i].ToString("F3") + "\t";

                    lCalibrationData[6 + 2 * kk] = xavg[i];
                    lCalibrationData[6 + 2 * kk + 1] = yavg[i];
                    kk++;
                }
                if (gageData != null && gageData.Length > 0)
                {
                    if (gageData.Length == 7)
                    {
                        bGageOn = true;
                        //
                        //  Old Formula
                        //
                        //lCalibrationData[16] = -gageData[0]; //  X
                        //lCalibrationData[17] = gageData[2]; //  Y
                        //if (mbUseZ123)
                        //    lCalibrationData[18] = -(gageData[4] + gageData[5] + gageData[6]) / 3; //  Z
                        //else
                        //    lCalibrationData[18] = -(gageData[5] + gageData[6]) / 2; //  Z

                        //lCalibrationData[19] = -(gageData[4] - (gageData[5] + gageData[6]) / 2) / 55000; //  TX, gageData[3] is inversed, radian
                        //lCalibrationData[20] = -(gageData[5] - gageData[6]) / 110000; //  TY, radian
                        //lCalibrationData[21] = -(gageData[1] - gageData[0] - (gageData[3] - gageData[2])) / (80000 * 0.999325049); //  TZ, radian
                        //lCalibrationData[22] = gageData[3]; //  Y

                        //
                        //  New Formula
                        //
                        //  32mm : X probe offset from center along X axis
                        //  47mm : Y probe offset from center along Y axis
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 40, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);    // 32.3 //32.306);
                        //double[] TxTyZ = m__G.oCam[0].mFAL.CalcTXTYZfromProbes(gageData[5] / 1000, gageData[6] / 1000, gageData[4] / 1000, XYTz[0], XYTz[1], XYTz[2]);
                        double[] XYTz = new double[3];
                        XYTz[0] = gageData[0];
                        XYTz[1] = gageData[1];
                        XYTz[2] = Math.Atan((gageData[2] - gageData[3]) / 45000);  //  45mm
                        double[] TxTyZ = new double[3];
                        TxTyZ[0] = Math.Atan((gageData[4] - (gageData[5] + gageData[6]) / 2) / 83000);  //  83mm
                        TxTyZ[1] = Math.Atan((gageData[5] - gageData[6]) / 120000);  //  120mm
                        TxTyZ[2] = (gageData[5] + gageData[6]) / 2;  //  120mm

                        //double ofx = m__G.oCam[0].mFAL.mFZM.mProbeYRy - 32;   //  Fiducial Mark Position relative to Probe Position
                        //double ofy = m__G.oCam[0].mFAL.mFZM.mProbeXRx - 32;   //  Fiducial Mark Position relative to Probe Position


                        //lCalibrationData[16] = (ofx - XYTz[0]) * 1000; // um
                        //lCalibrationData[17] = (ofy + XYTz[1]) * 1000; // um
                        //lCalibrationData[18] = -TxTyZ[2] * 1000; // um
                        //lCalibrationData[19] = TxTyZ[0];       // TX   radian
                        //lCalibrationData[20] = -TxTyZ[1];       // TY   radian
                        //lCalibrationData[21] = XYTz[2];       // TZ   radian

                        //double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0], TxTyZ[1]);
                        double compZ = ProbeZcompensationForTXTY(XYTz[0], XYTz[1], TxTyZ[2], TxTyZ[0] /*- mPorg.TX * MIN_To_RAD*/, TxTyZ[1] /*- mPorg.TY * MIN_To_RAD*/);
                        lCalibrationData[16] = XYTz[0]; // um
                        lCalibrationData[17] = XYTz[1]; // um
                        lCalibrationData[18] = compZ;// TxTyZ[2]; // um
                        //Point3d compRes = XYZcompensationAboutZPivots(new Point3d(XYTz[0], XYTz[1], TxTyZ[2]), TxTyZ[0], TxTyZ[1]);
                        //lCalibrationData[16] = compRes.X;//XYTz[0]; // um
                        //lCalibrationData[17] = compRes.Y;//XYTz[1]; // um
                        //lCalibrationData[18] = compRes.Z;// compZ;// TxTyZ[2]; // um
                        lCalibrationData[19] = TxTyZ[0] * RAD_To_MIN;       // TX   radian
                        lCalibrationData[20] = TxTyZ[1] * RAD_To_MIN;       // TY   radian
                        lCalibrationData[21] = -XYTz[2] * RAD_To_MIN;       // TZ   radian  // 241216 TZ 부호변경
                    }

                    if (IsShowResult)
                        strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (lCalibrationData[19]).ToString("F1") + "\t"
                            + (lCalibrationData[20]).ToString("F1") + "\t" + (lCalibrationData[21]).ToString("F1");
                }


                if (IsShowResult && m__G.m_bPrismCS )
                {
                    if (bGageOn)
                    {
                        lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(lCalibrationData[19], lCalibrationData[20], lCalibrationData[21], true, true);
                        lProbePrismTXTYTZ[1] = lCalibrationData[19];
                        lCalibrationData[22] = lPrismTXTYTZ[0];
                        lCalibrationData[23] = lPrismTXTYTZ[1];
                        lCalibrationData[24] = lPrismTXTYTZ[2];

                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5") + "\t" +
                                        lProbePrismTXTYTZ[0].ToString("F5") + "\t" + lProbePrismTXTYTZ[1].ToString("F5") + "\t" + lProbePrismTXTYTZ[2].ToString("F5");
                    }
                    else
                    {
                        strtmp[ci] += "\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                    }
                }

                    //
                    //HexaPod Data
                    //
                    //
                    //
                    //lCalibrationData[16] = gageData[0] * 1000; //ofx - XYTz[0]) * 1000;     // um
                    //lCalibrationData[17] = -gageData[1] * 1000; //ofy + XYTz[1]) * 1000;     // um
                    //lCalibrationData[18] = -gageData[2] * 1000; //TxTyZ[2] * 1000;   // um
                    //lCalibrationData[19] = -gageData[3] * Math.PI / 180; //xTyZ[0];           // TX   radian
                    //lCalibrationData[20] = gageData[4] * Math.PI / 180; //TxTyZ[1];          // TY   radian
                    //lCalibrationData[21] = -gageData[5] * Math.PI / 180; //XYTz[2];           // TZ   radian
                    //
                    //
                    //if (IsShowResult)
                    //        strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                    //            + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");


                if (ci == 0)
                {
                    mCalibrationFullData.Add(lCalibrationData);
                    mGageFullData.Add(lCalibrationData);
                    mPrismTXTYTZ.Add(lPrismTXTYTZ);

                }
            }


            if (IsShowResult)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();
                        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                        //tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    //tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                    ltextBox[0].Text += strtmp[0] + "\r\n";
                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }

        public void DrawMarkDetected(bool withID = false)
        {
            try
            {
                Mat tmpImg = new Mat();
                Mat lOverlayedImg = new Mat();
                Cv2.CvtColor(m__G.oCam[0].mFAL.mSourceImg[0], lOverlayedImg, ColorConversionCodes.GRAY2RGB);

                sFiducialMark lfMark = null;
                ref OpenCvSharp.Point[] fMarkPos = ref m__G.oCam[0].mDetectedMarkPos[0];
                int lMarkCount = fMarkPos.Length;
                int lModelScale = m__G.oCam[0].mFAL.mFidMarkTop[0].MScale;
                int lwidth = 0;
                int lheight = 0;
                OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();

                for (int i = 0; i < lMarkCount; i++)
                {
                    if (fMarkPos[i].X == 0 && fMarkPos[i].Y == 0)
                        continue;

                    if (i < 3)
                    {
                        lwidth = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Width;
                        lheight = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Height;
                    }
                    else
                    {
                        lwidth = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Width;
                        lheight = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Height;
                    }

                    lrc.X = fMarkPos[i].X * lModelScale;
                    lrc.Y = fMarkPos[i].Y * lModelScale;
                    lrc.Width = lModelScale * lwidth;
                    lrc.Height = lModelScale * lheight;

                    lOverlayedImg.Rectangle(lrc, Scalar.Cyan, 1);

                    if (m__G.oCam[0].mbDrawReference)
                    {
                        int x = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].X;
                        int y = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].Y;
                        Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                        Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                    }
                }
                if (withID)
                {
                    // ID Mark Position
                    lrc.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 38 + 13);
                    lrc.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 3);
                    lrc.Width = 39;
                    lrc.Height = 36;// (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2 - 10);
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                    lrc.X += 49;
                    lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                }


                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(lOverlayedImg);
                CropView.Image = myImage;
            }
            catch(Exception ex)
            {
                AddVsnLog("DrawMarkDected:" + ex.Message);
            }
            
        }

        public void DrawMarkDetectedWithDummyShift(int dummyX, int dummyY, bool withID = false)
        {
            Mat tmpImg = new Mat();
            Mat lOverlayedImg = new Mat();
            Cv2.CvtColor(m__G.oCam[0].mFAL.mSourceImg[0], lOverlayedImg, ColorConversionCodes.GRAY2RGB);

            sFiducialMark lfMark = null;
            ref OpenCvSharp.Point[] fMarkPos = ref m__G.oCam[0].mDetectedMarkPos[0];
            int lMarkCount = fMarkPos.Length;
            int lModelScale = m__G.oCam[0].mFAL.mFidMarkTop[0].MScale;
            int lwidth = 0;
            int lheight = 0;
            OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();

            for (int i = 0; i < lMarkCount; i++)
            {
                if (fMarkPos[i].X == 0 && fMarkPos[i].Y == 0)
                    continue;

                if (i < 3)
                {
                    lwidth = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Width;
                    lheight = m__G.oCam[0].mFAL.mFidMarkSide[0].modelSize.Height;
                }
                else
                {
                    lwidth = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Width;
                    lheight = m__G.oCam[0].mFAL.mFidMarkTop[0].modelSize.Height;
                }

                lrc.X = fMarkPos[i].X * lModelScale;
                lrc.Y = fMarkPos[i].Y * lModelScale;
                lrc.Width = lModelScale * lwidth;
                lrc.Height = lModelScale * lheight;

                //lOverlayedImg.Rectangle(lrc, Scalar.Cyan, 1);

                if (m__G.oCam[0].mbDrawReference)
                {
                    int x = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].X;
                    int y = (int)m__G.oCam[0].mFAL.mMarkPosOnPanel[i].Y;
                    Cv2.Line(lOverlayedImg, x - 10, y, x + 10, y, Scalar.OrangeRed, 1, LineTypes.Link4);
                    Cv2.Line(lOverlayedImg, x, y - 10, x, y + 10, Scalar.OrangeRed, 1, LineTypes.Link4);
                }
                Cv2.Line(lOverlayedImg, dummyX - 8, dummyY, dummyX + 8, dummyY, Scalar.Lime, 1, LineTypes.Link4);
                Cv2.Line(lOverlayedImg, dummyX, dummyY - 8, dummyX, dummyY + 8, Scalar.Lime, 1, LineTypes.Link4);
            }
            if (withID)
            {
                // ID Mark Position
                lrc.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 39 + 13);
                lrc.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 5);
                lrc.Width = 39;
                lrc.Height = (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2 - 10);
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
                lrc.X += 49;
                lOverlayedImg.Rectangle(lrc, Scalar.Magenta, 1);
            }


            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(lOverlayedImg);
            CropView.Image = myImage;
        }

        bool bThreadManualFindMarks = false;
        bool bFinishThreadManualFindMarks = true;

        private void btnFindMarks_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            GrabToFindMark();
        }

        public void GrabToFindMark(bool isContinue = true)
        {
            if (isContinue)
            {
                if (!bThreadManualFindMarks)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnLive2.Enabled = false;
                            button10.Enabled = false;
                        });
                    }
                    else
                    {
                        btnLive2.Enabled = false;
                        button10.Enabled = false;
                    }
                    m__G.oCam[0].HaltA();
                    bHaltLive = true;
                    IsLiveCropStop = true;
                    bThreadManualFindMarks = true;
                    mCalibrationFullData = new List<double[]>();

                    //if (m__G.mGageCounter != null)
                    //    m__G.mGageCounter.OpenAllport();  // 250210 주석처리

                    Task.Run(() => ThreadManualFindMarks(1000));
                }
                else
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnLive2.Enabled = true;
                            button10.Enabled = true;
                        });
                    }
                    else
                    {
                        btnLive2.Enabled = true;
                        button10.Enabled = true;
                    }

                    bThreadManualFindMarks = false;

                    while (!bFinishThreadManualFindMarks)
                        Thread.Sleep(100);

                    //if (m__G.mGageCounter != null)
                    //    m__G.mGageCounter.CloseAllport();// 250210 주석처리

                    btnFindMarks.Text = "Grab to Find Marks";
                }
            }
            else
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = false;
                        button10.Enabled = false;
                    });
                }
                else
                {
                    btnLive2.Enabled = false;
                    button10.Enabled = false;
                }


                m__G.oCam[0].HaltA();
                bHaltLive = true;
                IsLiveCropStop = true;
                bThreadManualFindMarks = true;
                mCalibrationFullData = new List<double[]>();
                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.OpenAllport(); // 250210 주석처리

                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
                ManualFindMarks(0, true, false);

                m__G.fGraph.Drive_LEDs(0, 0);
                Thread.Sleep(500);

                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLive2.Enabled = true;
                        button10.Enabled = true;
                    });
                }
                else
                {
                    btnLive2.Enabled = true;
                    button10.Enabled = true;
                }

                bThreadManualFindMarks = false;

                //if (m__G.mGageCounter != null)
                //    m__G.mGageCounter.CloseAllport(); // 250210 주석처리

                btnFindMarks.Text = "Grab to Find Marks";
            }
        }
        public int mAutoCalibrationIndex = 0;
        public void PrepareRemoteCalibration()
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  mAutoCalibrationIndex 에 data index 가 들어감.
            //  원격 Calibration 하려는 경우 본 함수 가장 먼저 호출 한 뒤, 매 이동 직후 SingleFindMark() 호출.
            //  마지막 SingleFindMark() 호출 뒤 RemoteCalibration() 호출
            mCalibrationFullData = new List<double[]>();
            mAutoCalibrationIndex = 0;
        }

        //public void RemoteCalibration(string strAxis, int skipCount)
        //{
        //    //  strAxis 은 "Z", "Y', "X", "TX", "TY" 를 넣을 수 있다.
        //    //  Calibraition 순서는 "Z" -> "Z"  -> "Y" -> "Y" -> "X" -> "X" -> "TX" -> "TY"
        //    //  각 보정결과를 확인해야 하므로 보전 전 - 후 로 실시한다.

        //    //  ORG 위치에서 한쪽 끝으로 이동하면서 얻어진 데이터( skipCount )는 삭제한다.
        //    for (int i = 0; i < skipCount; i++)
        //        mCalibrationFullData.RemoveAt(0);

        //    //JH_SK_CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        //    CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        //}

        public void SingleFindMark(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);   //  Wait LED Power Up.
            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarks(mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarks(mAutoCalibrationIndex, false, IsSave);

            mAutoCalibrationIndex++;
            m__G.fGraph.Drive_LEDs(0, 0);
        }
        public double[] AnosisCalData = new double[25]; // 33
        public void SingleFindMarkAnosis(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(100);   //  Wait LED Power Up.
            
            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarksAnosis(ref AnosisCalData, mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarksAnosis(ref AnosisCalData, mAutoCalibrationIndex, false, IsSave);

            mAutoCalibrationIndex++;
            m__G.fGraph.Drive_LEDs(0, 0);
        }

        public void ThreadManualFindMarks(int maxCnt = 500)
        {
            bFinishThreadManualFindMarks = false;

            for (int i = 0; i < maxCnt; i++)
            {
                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
                ManualFindMarks(i, true, true);

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////   임시
                //string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab" + i.ToString() + "_1.bmp";
                //m__G.oCam[0].SaveGrabbedImage(1, fileName);
                //fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab" + i.ToString() + "_2.bmp";
                //m__G.oCam[0].SaveGrabbedImage(2, fileName);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                m__G.fGraph.Drive_LEDs(0, 0);
                Thread.Sleep(500);
                if (!bThreadManualFindMarks)
                    break;
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnFindMarks.Text = "Grab to Find Marks";
                });
            }
            else
            {
                btnFindMarks.Text = "Grab to Find Marks";
            }

            bThreadManualFindMarks = false;
            bFinishThreadManualFindMarks = true;
        }
        public string RemoteGrab()
        {
            DrawMarkPositions();

            m__G.oCam[0].GrabB(0);

            string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            m__G.oCam[0].SaveGrabbedImage(0, fname);
            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname);
            return fname;
        }
        public void FindMarks(int index = 1)
        {
            string strtmp = "";
            m__G.oCam[0].PrepareFineCOG();

            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            m__G.oCam[0].FineCOG(true, index, 0, true);

            if (index == 0)
                return;

            FAutoLearn.FZMath.Point2D[] mpts = new FAutoLearn.FZMath.Point2D[5];

            mpts[0] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][0].X, m__G.oCam[0].mAzimuthPts[index][0].Y);

            mpts[1] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][4].X, m__G.oCam[0].mAzimuthPts[index][4].Y);

            mpts[2] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][6].X, m__G.oCam[0].mAzimuthPts[index][6].Y);

            mpts[3] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][8].X, m__G.oCam[0].mAzimuthPts[index][8].Y);

            mpts[4] = new FAutoLearn.FZMath.Point2D(m__G.oCam[0].mAzimuthPts[index][10].X, m__G.oCam[0].mAzimuthPts[index][10].Y);

            m__G.oCam[0].PointTo6DMotion(index, mpts);

            double lx, ly, lz;
            double ltx, lty, ltz;
            lx = m__G.oCam[0].mC_pX[index] * 5.5 / Global.LensMag;    //  Pixel to um
            ly = m__G.oCam[0].mC_pY[index] * 5.5 / Global.LensMag;    //  Pixel to um
            lz = m__G.oCam[0].mC_pZ[index] * 5.5 / Global.LensMag;    //  Pixel to um ////////////////////////////////////////// ZLUT 적용 검토

            ltx = m__G.oCam[0].mC_pTX[index] * 180 * 60 / Math.PI;    //  radian to min
            lty = m__G.oCam[0].mC_pTY[index] * 180 * 60 / Math.PI;    //  radian to min
            ltz = m__G.oCam[0].mC_pTZ[index] * 180 * 60 / Math.PI;    //  radian to min

            strtmp += lx.ToString("F2") + "\t" + ly.ToString("F2") + "\t" + lz.ToString("F2") + "\t| " + ltx.ToString("F2") + "\t" + lty.ToString("F2") + "\t" + ltz.ToString("F2") + "\t| ";


            double[] lCalibrationData = new double[23];

            lCalibrationData[0] = lx;
            lCalibrationData[1] = ly;
            lCalibrationData[2] = lz;
            lCalibrationData[3] = ltx;
            lCalibrationData[4] = lty;
            lCalibrationData[5] = ltz;

            mCalibrationFullData.Add(lCalibrationData);



            //for (int i = 0; i < 12; i++)
            //{
            //    if (m__G.oCam[0].mAzimuthPts[1][i].X == 0) continue;
            //    strtmp += m__G.oCam[0].mAzimuthPts[1][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[1][i].Y.ToString("F3") + "\t";
            //}
            double XfromNtoS = Math.Abs(m__G.oCam[0].mAzimuthPts[index][8].X - m__G.oCam[0].mAzimuthPts[index][10].X);
            double YfromNStoE = Math.Abs((m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 - m__G.oCam[0].mAzimuthPts[index][6].Y);
            double YfromSideNStoTopNS = Math.Abs((m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 - (m__G.oCam[0].mAzimuthPts[index][8].Y + m__G.oCam[0].mAzimuthPts[index][10].Y) / 2);

            if (lz > 0)
                strtmp += "X N-S\t" + XfromNtoS.ToString("F3") + "\tY NS-E\t" + YfromNStoE.ToString("F3") + "\tMove close by " + lz.ToString("F0") + "um\r\n";
            else
                strtmp += "X N-S\t" + XfromNtoS.ToString("F3") + "\tY NS-E\t" + YfromNStoE.ToString("F3") + "\tMove away by " + (-lz).ToString("F0") + "um\r\n";

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += strtmp;
                    tbInfo.SelectionStart = tbInfo.Text.Length;
                    tbInfo.ScrollToCaret();
                });
            }
            else
            {
                tbInfo.Text += strtmp;
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
            }
        }

        public void SetDefaultMarkConfig(bool IsDraw = false)
        {
            System.Drawing.Point[] markPos = null;

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);        //  CropGap 이 적용되지 않은 상태의 결과를 반환한다.
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);   //  CropGap 이 적용되지 않은 상태의 데이터
            m__G.mFAL.SetMarkNorm();                                //  CropGap 이 적용되지 않은 상태의 데이터
            //if (IsDraw)
            //{
            //    m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
            //    m__G.oCam[0].DrawCSHCross(Brushes.Red);
            //}
        }
        private void btnLoadBmpFindMark_Click(object sender, EventArgs e)
        {
            m__G.mFAL.mCandidateIndex = 0;
            m__G.oCam[0].DrawClear();

            string defaultPath = Path.GetFullPath(
                m__G.m_RootDirectory + "\\Result\\RawData");

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "bmp";
            openFile.Multiselect = true;
            openFile.Filter = "BMP(*.bmp)|*.bmp";

            if (openFile.ShowDialog() != DialogResult.OK)
                return;

            m__G.oCam[0].mFAL.ClearCommonImgFile();
            //  영상들의 처리에 앞서서 반드시 들어가야 한다.
            //  Mark 가 업데이트 되면 반드시 수행, 영상크기가 확정되어야 한다.
            int findex = 0;
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].ForceTriggerTime();

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.

            if (tbMaxThread.Text.Length > 0)
                m__G.mMaxThread = int.Parse(tbMaxThread.Text);

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[5000];

            if (openFile.FileNames.Length == 1)
            {
                m__G.oCam[0].mFAL.LoadBMPtoBufN(openFile.FileNames[0], 0);
                m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

                int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
                for (int ci = 0; ci < numFMIcandidate; ci++)
                {
                    m__G.mFAL.mCandidateIndex = ci;
                    ChangeFiducialMark(ci);

                    m__G.oCam[0].mFAL.mbGetHistogram = true;
                    m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, true);
                    m__G.oCam[0].mFAL.mbGetHistogram = false;

                    string strtmp = openFile.FileNames[0] + ">> \r\n";

                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[0][i].X == 0) continue;
                        strtmp += m__G.oCam[0].mAzimuthPts[0][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[0][i].Y.ToString("F3") + "\t";
                    }
                    strtmp += "\r\nContrast\t";
                    for (int i = 0; i < 5; i++)
                        strtmp += m__G.oCam[0].mFAL.mEffectiveContrast[i].ToString() + "\t";

                    tbVsnLog.Text += strtmp + "( > 20 )\r\n";
                }
                // ScaleNOpticalRotation();
            }
            else
            {
                long startTime = 0;
                long endTime = 0;
                SupremeTimer.QueryPerformanceFrequency(ref m__G.TimerFrequency);
                int i = 0;
                m__G.oCam[0].mTrgBufLength = openFile.FileNames.Length;
                foreach (string filename in openFile.FileNames)
                {
                    //m__G.oCam[0].LoadBMPtoBufN(filename, i++);
                    m__G.oCam[0].mFAL.LoadBMPtoBufN(filename, i++);
                }
                m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

                int numFile = i;

                SupremeTimer.QueryPerformanceCounter(ref startTime);
                string strtmp = "";

                int maxThread = m__G.mMaxThread;
                if (numFile < 20)
                    maxThread = 1;
                //else if (numFile < 26)
                //    maxThread = numFile/2;

                int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
                strtmp = "";
                for (int ci = 0; ci < numFMIcandidate; ci++)
                {
                    //////////////////////////////////////////////////////////////
                    /////   모델 2개 추적하기위한 모델 변경 관련 코드
                    //////////////////////////////////////////////////////////////
                    m__G.mFAL.mCandidateIndex = ci;
                    if (ci == 1)
                    {
                        m__G.mFAL.GetMarkPosOnPanel(out markPos);
                        m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
                        m__G.mFAL.SetMarkNorm();
                    }
                    else if (ci == 0)
                        m__G.mFAL.SetDefaultMarkNorm();

                    //////////////////////////////////////////////////////////////
                    /////   아래로는 공통
                    //////////////////////////////////////////////////////////////
                    m__G.mbSuddenStop[0] = false;
                    int orgmTargetTriggerCount = m__G.oCam[0].mTargetTriggerCount;
                    m__G.oCam[0].mTargetTriggerCount = numFile;
                    //m__G.oCam[0].mTrgBufLength = 3000;
                    m__G.oCam[0].SetTriggeredframeCount(numFile);



                    m__G.fVision.ProcessVisionData(numFile, maxThread, true);



                    m__G.mbSuddenStop[0] = false;
                    m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;

                    double umscale = 5.5 / Global.LensMag;                           //  rad to min
                    double minscale = 180 / Math.PI * 60;                           //  rad to min


                    //public double[] mPOMM_X = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_Y = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_Z = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_TX = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_TY = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_TZ = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_sX = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_sY = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_tX = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view
                    //public double[] mPOMM_tY = new double[10000];  //  Left 0,1 ;; Right 2,3 in a camera view

                    for (int fileCnt = 0; fileCnt < numFile; fileCnt++)
                    {
                        strtmp += fileCnt.ToString() + "\t" + (umscale * m__G.oCam[0].mC_pX[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pY[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pZ[fileCnt]).ToString("F2")
                             + "\t" + (minscale * m__G.oCam[0].mC_pTX[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTY[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTZ[fileCnt]).ToString("F2") + "\t";
                        if (!m__G.m_bPseudoOMM)
                        {
                            for (i = 0; i < 12; i++)
                            {
                                if (m__G.oCam[0].mAzimuthPts[fileCnt][i].X == 0) continue;
                                strtmp += m__G.oCam[0].mAzimuthPts[fileCnt][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[fileCnt][i].Y.ToString("F3") + "\t";
                            }
                        }
                        else
                        {
                            strtmp += (umscale * m__G.oCam[0].mPOMM_X[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mPOMM_Y[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mPOMM_Z[fileCnt]).ToString("F2")
                                 + "\t" /*+ (minscale * m__G.oCam[0].mPOMM_TX[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mPOMM_TY[fileCnt]).ToString("F2") + "\t"*/ + (minscale * m__G.oCam[0].mPOMM_TZ[fileCnt]).ToString("F2")
                                 //+ "\t" + (m__G.oCam[0].mPOMM_sX[fileCnt]).ToString("F3") + "\t" + (m__G.oCam[0].mPOMM_sY[fileCnt]).ToString("F3") + "\t" + (m__G.oCam[0].mPOMM_tX[fileCnt]).ToString("F3") + "\t" + (m__G.oCam[0].mPOMM_tY[fileCnt]).ToString("F3")
                                 + "\t" + (umscale * m__G.oCam[0].mPOMM_rX[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mPOMM_rY[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mPOMM_rZ[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mPOMM_rTZ[fileCnt]).ToString("F2")
                                 + "\t";
                        }
                        strtmp += "\r\n";
                    }
                    strtmp += "\r\n";
                    //////////////////////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////

                }

                m__G.mFAL.SetDefaultMarkNorm();
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();


                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapse = 1000 * (endTime - startTime) / (double)(m__G.TimerFrequency);
                tbVsnLog.Text += strtmp;
                tbVsnLog.Text += "\r\nEllapsed Time : " + ellapse.ToString("F3") + " msec for processing " + numFile.ToString() + " Frames, LED : " + mLEDcurrent[0].ToString("F2") + " " + mLEDcurrent[1].ToString("F2") + "\r\n";

                mTriggerGrabbedFrame = numFile;
                MyOwner.WriteResultBin();
                //MyOwner.WriteResult();

            }
            //  Default Mark Position
            DrawMarkDetected();
            m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
            tbVsnLog.Text += "Finish\r\n";
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();

            //string lstr = "";
            //for ( int i=0; i< m__G.oCam[0].mFAL.mVerifyThresholds.Count; i++)
            //{
            //    if (m__G.oCam[0].mFAL.mVerifyThresholds[i] == null)
            //        continue;
            //    lstr += m__G.oCam[0].mFAL.mVerifyThresholds[i][0].ToString() + "," + m__G.oCam[0].mFAL.mVerifyThresholds[i][1].ToString() + "," + m__G.oCam[0].mFAL.mVerifyThresholds[i][2].ToString() + "," + m__G.oCam[0].mFAL.mVerifyThresholds[i][3].ToString() + "\r\n";
            //}
            //File.AppendAllText("D:\\JH\\FACA\\SlopeNrSide.csv", lstr);

        }

        //private string ParallelFindMark(int istart, int iend, int iBuf)
        //{
        //    //for (int findex = istart; findex < iend; findex++)
        //    //    m__G.oCam[0].FineCOG(findex, iBuf);

        //    m__G.oCam[0].FineCOG(true, istart, iBuf);
        //    for (int findex = istart + 1; findex < iend; findex++)
        //        m__G.oCam[0].FineCOG(false, findex, iBuf);

        //    return iend.ToString();
        //}


        public void CalcVisionData(int cam, int ci, int ce, int cstep, int iBuf, bool IsFile = false)
        {
            int i = 0;
            mb_FinishCalcVision[iBuf] = false;
            try
            {
                bool res = false;
                int si = ci;
                int se = ce;
                int sstep = cstep;
                if (ci > ce)
                {
                    for (i = ci; i >= ce; i -= cstep)    //  Skip 0 ~ 59
                    {
                        if (m__G.mbSuddenStop[0])   //  연산도 중단함.
                            break;
                        try
                        {
                            res = m__G.oCam[cam].FineCOG(false, i, iBuf, false, true, false, IsFile);    // 마크찾기
                        }
                        catch (Exception ex)
                        {
                            ;
                        }
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
                else
                {
                    for (i = ci; i < ce; i += cstep)    //  Skip 0 ~ 59
                    {
                        if (i == 0)
                            continue;

                        if (m__G.mbSuddenStop[0])   //  연산도 중단함.
                            break;

                        res = m__G.oCam[cam].FineCOG(false, i, iBuf, false, true, false, IsFile);    // 마크찾기
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                m__G.fManage.AddViewLog(ex.ToString() + "\r\n");
            }
            mb_FinishCalcVision[iBuf] = true;
        }


        private void ScanFOV()
        {
            int Cam = m_FocusedLED;
            int l_OrgROIV_min = v_OrgROIV_min[Cam];

            //label6.Text = "Live On";
            m__G.oCam[0].LiveA();

            if (m__G.mCamCount > 1)
                m__G.oCam[1].LiveA();

            int i = 1;
            for (; i < 1069 - v_OrgROIV_height[Cam]; i += 10)
            {
                v_OrgROIV_min[Cam] = i;
                tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
                SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
                Thread.Sleep(30);
            }
            for (; i > 10; i -= 10)
            {
                v_OrgROIV_min[Cam] = i;
                tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
                SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
                Thread.Sleep(30);
            }
            v_OrgROIV_min[Cam] = l_OrgROIV_min;
            tbVsnLog.Text = "X " + v_OrgROIH_min[Cam] + "-" + (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]) + ": Y " + v_OrgROIV_min[Cam] + "-" + (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]) + "\r\n";
            SetNewROIXY(Cam, v_OrgROIH_min[Cam], (v_OrgROIH_min[Cam] + v_OrgROIH_width[Cam]), v_OrgROIV_min[Cam], (v_OrgROIV_min[Cam] + v_OrgROIV_height[Cam]));
        }

        private void btnLEDDOWN_R_Click(object sender, EventArgs e)
        {
        }

        private void btnLEDUP_R_Click(object sender, EventArgs e)
        {
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            //m__G.fManage.ManualToPlot();
            if (!bHaltLive)
                GrabHalt();

            cbContinuosMode.Checked = false;
            cbSaveNthImg.Checked = false;
            IsLiveCropStop = true;
            bThreadManualFindMarks = false;
            VisibleAdmin(false);

            this.Hide();
            if (MyOwner.m_bAdmin)
            {
                MyOwner.ShowAdminMode();
            }
            else
            {
                MyOwner.ShowOperatorMode();
            }
        }

        private void rbLED1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLED1.Checked)
            {
                m_FocusedLED = 1;
            }
        }

        private void rbLED2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLED2.Checked)
            {
                m_FocusedLED = 0;

            }

        }

        private void rbLED3_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void rbLED4_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void btnLoadUnloadAll_Click(object sender, EventArgs e)
        {
            if (!mSocketLoaded[0])
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "01LD";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.fGraph.socket_IN(0);
                mSocketLoaded[0] = true;
                if (m__G.mCamCount > 1)
                {
                    m__G.fGraph.socket_IN(1);
                    mSocketLoaded[1] = true;
                }
            }
            else
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "01UD";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.fGraph.socket_OUT(0);
                mSocketLoaded[0] = false;
                if (m__G.mCamCount > 1)
                {
                    m__G.fGraph.socket_OUT(1);
                    mSocketLoaded[1] = false;
                }
            }
        }

        private void btnFailLED1_Click(object sender, EventArgs e)
        {
            if (m__G.mChannelCount < 2)
                return;
            m__G.fGraph.mDriverIC.SetFailLED(0, true);
            Thread.Sleep(1000);
            m__G.fGraph.mDriverIC.SetFailLED(0, false);
        }

        private void btnFailLED2_Click(object sender, EventArgs e)
        {
            if (m__G.mChannelCount < 2)
                return;
            m__G.fGraph.mDriverIC.SetFailLED(1, true);
            Thread.Sleep(1000);
            m__G.fGraph.mDriverIC.SetFailLED(1, false);
        }

        private void btnFailLED3_Click(object sender, EventArgs e)
        {

        }

        private void btnFailLED4_Click(object sender, EventArgs e)
        {

        }

        public void LEDMarkCheck()
        {
            m_bLEDMarkCheck = true;

            int i = 0;
            while (m_bLEDMarkCheck)
            {
                if ((i % 2) == 0)
                    m__G.fGraph.Drive_LED(0, mLEDcurrent[0]);
                else
                    m__G.fGraph.Drive_LED(0, 0);
                if (m__G.mChannelCount > 1)
                {
                    if (((i / 2) % 2) == 0)
                        m__G.fGraph.Drive_LED(1, mLEDcurrent[1]);
                    else
                        m__G.fGraph.Drive_LED(1, 0);
                }

                if (m__G.mCamCount > 1)
                {
                    if (((i / 4) % 2) == 0)
                        m__G.fGraph.Drive_LED(2, mLEDcurrent[2]);
                    else
                        m__G.fGraph.Drive_LED(2, 0);

                    if (((i / 8) % 2) == 0)
                        m__G.fGraph.Drive_LED(3, mLEDcurrent[3]);
                    else
                        m__G.fGraph.Drive_LED(3, 0);
                }

                Thread.Sleep(100);
                i++;
            }
            m__G.fGraph.Drive_LED(0, 0);
            if (m__G.mChannelCount > 1)
                m__G.fGraph.Drive_LED(1, 0);
            if (m__G.mCamCount > 1)
            {
                m__G.fGraph.Drive_LED(2, 0);
                m__G.fGraph.Drive_LED(3, 0);
            }
        }

        private void btnLoadUnloadL_Click(object sender, EventArgs e)
        {
            if (m_FocusedLED > 1)
            {
                m_FocusedLED = 0;
            }

            int port = m_FocusedLED / 2;
            if (!mSocketLoaded[port])
            {
                m__G.fGraph.socket_IN(port);
                mSocketLoaded[port] = true;
                mDoneWriteRun = false;
            }
            else
            {
                m__G.fGraph.socket_OUT(port);
                mSocketLoaded[port] = false;
                mDoneWriteRun = false;
            }

        }
        public int saveCount = 0;
        private void btnAllLEDOn_Click(object sender, EventArgs e)
        {
            if (!m_bAllLEDOn)
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "02ON";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                m__G.mDoingStatus = "Checking Vision";

                //  CSH035 적용 시 
                m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
                m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));

                m_bAllLEDOn = true;

                btnAllLEDOn.ForeColor = Color.White;
            }
            else
            {
                if (F_Main.MachineType == (int)MachineType.Master)
                {
                    string tmpstr = "02OF";
                    m__G.fManage.PC2SendData("VBC", tmpstr, tmpstr.Length, 2);
                }
                //m__G.oCam[0].GrabA();
                //if (m__G.mCamCount > 1)
                //    m__G.oCam[1].GrabA();

                //  TZAF 검사기 적용 시
                //m__G.fGraph.mDriverIC.SetLEDpowers(0, 0, m__G.mCamCount);

                //  CSH035 적용 시 
                m__G.fGraph.Drive_LEDs(0, 0);

                //m__G.fVision.SetOrgExposure(0);
                //if (m__G.mCamCount > 1)
                //    m__G.fVision.SetOrgExposure(1);
                m_bAllLEDOn = false;
                btnAllLEDOn.ForeColor = Color.SlateGray;
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
            }
        }
        public int MeasureTXTY(ref double[] l_StrokeL, ref double[] l_StrokeR, ref double[] l_YawL, ref double[] l_YawR, ref double[] l_PitchL, ref double[] l_PitchR)
        {
            double[] sYaw = new double[4];
            double[] sPitch = new double[4];
            double[] cx = new double[8];
            double[] cy = new double[8];
            double[] sYaw2 = new double[4];
            double[] sPitch2 = new double[4];
            double[] cx2 = new double[8];
            double[] cy2 = new double[8];
            int res0 = 0;

            m__G.oCam[0].GrabA();

            m__G.oCam[0].FineCOG(true, 0, 0);

            return res0;
        }

        public void ToViet(bool IsToViet = true)
        {
            if (IsToViet)
            {
                btnBack.Text = "lùi lại sau";
                btnLive2.Text = "Video trực tiếp";
                btnHalt2.Text = "tạm dừng lại";
                btnClear1.Text = "khởi tạo";
                btnOISXReplay.Text = "phát lại X";
                btnOISXStepReplay.Text = "phát lại Step X";
                btnAllLEDOn.Text = "All LED OnOff";
                btnFOVUp.Text = "lên FOV";
                btnFOVLeft.Text = "Bên trái FOV";
                btnFOVRight.Text = "bên phải FOV";
                btnFOVDown.Text = "phía dưới FOV";
                btnFindMarks.Text = "tìm kiếm Marks";
                //btnSetAbsZero.Text = "đặt số không with Master Spl";

                rbLED1.Text = "Bên trái LED";
                rbLED2.Text = "bên phải LED";
            }
            else
            {
                btnBack.Text = "Back";
                btnLive2.Text = "Live";
                btnHalt2.Text = "Halt";
                btnClear1.Text = "Clear";
                btnOISXReplay.Text = "X Replay";
                btnOISXStepReplay.Text = "AF Step Replay	";
                btnAllLEDOn.Text = "All LED OnOff";
                btnFOVUp.Text = "FOV Up";
                btnFOVLeft.Text = "FOV Right";
                btnFOVRight.Text = "FOV Left";
                btnFOVDown.Text = "FOV Down";
                btnFindMarks.Text = "Find Marks";
                //btnSetAbsZero.Text = "Set Zero with Master Spl";

                rbLED1.Text = "LED Left";
                rbLED2.Text = "LED Right";
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (tbImgNumber.Text == "")
            {
                MessageBox.Show("Please input the image number!");
                return;
            }

            m__G.oCam[0].mFAL.LoadFMICandidate();

            int imgIndex = Convert.ToInt16(tbImgNumber.Text);

            if (imgIndex == 0)
            {
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.oCam[0].mFAL.BackupFMI();
            }
            m__G.mFAL.mCandidateIndex = 0;
            ChangeFiducialMark(0);


            string strtmp = NthMeasure(imgIndex);

            strtmp += "\r\n" + imgIndex.ToString() + "\t";
            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[imgIndex][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t";
            }
            strtmp += ">\t";
            for (int i = 0; i < 5; i++)
                strtmp += m__G.oCam[0].m_sMRinstant[i].mMTF.ToString("F0") + "\t";

            tbInfo.Text += strtmp + "\r\n";/*+ m__G.oCam[0].mGrabAbsTiming[imgIndex].ToString("F5")*/

            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();

            int nextFrame = 0;
            if (rb1step.Checked)
                nextFrame = imgIndex + 1;
            else
                nextFrame = imgIndex + 5;
            //if (nextFrame < 0)
            //    nextFrame = 0;

            tbImgNumber.Text = nextFrame.ToString();
        }

        public string NthMeasure(int imgIndex, bool bAccu = false)
        {
            //if ((m__G.fGraph.mAF_FrameCount-1) < imgIndex) return;

            double[] sYaw = new double[4];
            double[] sPitch = new double[4];
            double[] cx = new double[8];
            double[] cy = new double[8];

            double[] strokeL = new double[2];
            double[] yawL = new double[2];
            double[] pitchL = new double[2];

            double[] strokeR = new double[2];
            double[] yawR = new double[2];
            double[] pitchR = new double[2];
            System.Drawing.Point lptLT = new System.Drawing.Point(0, 0);
            System.Drawing.Point lptRB = new System.Drawing.Point(0, 0);

            //long nFound = 0;

            if (!bAccu)
            {
                m__G.oCam[0].DrawClear();
            }

            m__G.oCam[0].mTrgBufLength = m__G.oCam[0].mTargetTriggerCount;
            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();
            m__G.oCam[0].DispCommonImage(imgIndex);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (!bAccu)
                        tbVsnLog.Text = "";
                });
            }
            else
            {
                if (!bAccu)
                    tbVsnLog.Text = "";
            }

            //tbVsnLog.Text += "Target Length = " + m__G.oCam[0].mTrgBufLength.ToString();
            bool IsShow = cbSaveNthImg.Checked;

            m__G.oCam[0].mMatroxMsg = "";
            //m__G.oCam[0].DispCommonImage(imgIndex);


            //m__G.oCam[0].ResizeGrab(imgIndex);

            if (IsShow)
            {
                string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                if (!Directory.Exists(fileName))
                    Directory.CreateDirectory(fileName);

                //string compressedFileName = fileName + "c" + imgIndex.ToString() + ".bmp";
                fileName += "Ana" + imgIndex.ToString() + ".bmp";
                m__G.oCam[0].SaveGrabbedImage(imgIndex, fileName);
                //m__G.oCam[0].SaveCompressedImage(imgIndex, compressedFileName);
            }

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (tbBreakIndex.Text.Length > 0)
                        m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);
                });
            }
            else
            {
                if (tbBreakIndex.Text.Length > 0)
                    m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);
            }

            if (m__G.oCam[0].mFAL.mBreakIndex == imgIndex)
                IsShow = cbSaveNthImg.Checked;

            if (imgIndex == 0)
                m__G.oCam[0].FineCOG(true, imgIndex, 0, IsShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기
            else
                m__G.oCam[0].FineCOG(false, imgIndex, 0, IsShow, true, true);// ref sYaw, ref sPitch, ref cx, ref cy, ref nFound, cbSaveNthImg.Checked , 0, m__G.sModelName);   // 마크찾기

            //for (int p = 0; p < 12; p++)
            //{
            //    //if (p == 4) strtmp += "\r\n";
            //    //strtmp += cx[p].ToString("F3") + "\t" + cy[p].ToString("F3") + " \t";
            //    if (m__G.oCam[0].mAzimuthPts[imgIndex][p].X < 1) continue;

            //    lptLT.X = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].X - 5);
            //    lptLT.Y = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].Y - 5);
            //    lptRB.X = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].X + 5);
            //    lptRB.Y = (int)(m__G.oCam[0].mAzimuthPts[imgIndex][p].Y + 5);
            //    m__G.oCam[0].DrawDCBox(lptLT, lptRB, Brushes.Red);
            //}
            string strtmp = m__G.oCam[0].mMatroxMsg;

            return strtmp;
        }

        private void radioButton10Step_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10Step.Checked)
                m_FovStep = 10;

        }

        private void radioButton1Step_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1Step.Checked)
                m_FovStep = 1;

        }

        private void cbSaveNthImg_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void Process_AutoLED(bool IsFlipped = false)
        {

            double[] sPitch = new double[4];
            double[] sYaw = new double[4];
            double[] cx = new double[4];
            double[] cy = new double[4];

            double[] strokeL = new double[4];
            double[] yawL = new double[4];
            double[] pitchL = new double[4];
            double[] refBin = new double[4];
            double leftBin = 0;
            double rightBin = 0;

            //long nFound = 0;

            double Lpower = 2.63;
            double Rpower = 2.63;
            int limit = 16;
            while (limit-- > 0)
            {
                m__G.oCam[0].DrawClear();
                if (m__G.mCamCount > 1)
                {
                    m__G.oCam[1].DrawClear();
                }

                m__G.fGraph.Drive_LED(0, Lpower);
                if (m__G.mCamCount > 1)
                    m__G.fGraph.Drive_LED(1, Rpower);

                Thread.Sleep(20);

                if (m__G.mCamCount > 1)
                    m__G.oCam[1].GrabA(1);

                m__G.oCam[0].GrabA(1);
                m__G.fGraph.Drive_LEDs(0, 0);

                refBin = new double[4];
                m__G.oCam[0].GetRefBrightness(cx, cy, ref refBin);

                leftBin = (refBin[0] + refBin[1]) / 2.0;
                rightBin = (refBin[2] + refBin[3]) / 2.0;
                tbVsnLog.Text += Lpower.ToString("F2") + " - " + leftBin.ToString("F3") + " : " + Rpower.ToString("F2") + " - " + rightBin.ToString("F3") + "\r\n";
                if (!IsFlipped)
                {
                    if (leftBin < 23 && Lpower < 2.78)
                        Lpower += 0.01;
                    if (rightBin < 23 && Rpower < 2.78)
                        Rpower += 0.01;
                }
                else
                {
                    if (leftBin < 23 && Rpower < 2.78)
                        Rpower += 0.01;
                    if (rightBin < 23 && Lpower < 2.78)
                        Lpower += 0.01;
                }

                if (leftBin > 23 && rightBin > 23)
                    break;

                if (Lpower > 2.74 && Rpower > 2.74)
                    break;
            }
            tbVsnLog.Text += "Auto LED Power : " + Lpower.ToString("F2") + "\t" + Rpower.ToString("F2") + "\r\n";
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }


        private void btnUptoNthMesure_Click(object sender, EventArgs e)
        {
            UptoNthMeasure();
        }

        public void ChangeFiducialMark(int mID)
        {
            System.Drawing.Point[] markPos = null;

            if (mID != 0)
            {
                m__G.mFAL.GetMarkPosOnPanel(out markPos);
                m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
                m__G.mFAL.SetMarkNorm();
            }
            else
            {
                m__G.mFAL.SetDefaultMarkNorm();
            }
        }
        public void UptoNthMeasure(int extfrmCnt = 0)
        {
            int imgIndex = 0;
            try
            {
                if (extfrmCnt == 0)
                    imgIndex = Convert.ToInt16(tbImgNumber.Text);
            }
            catch
            {
                MessageBox.Show("Input Correct Image Number, Then Retry.");
                return;
            }

            string strtmp = "";
            tbInfo.Text = "";
            if (tbMaxThread.Text.Length > 0)
                m__G.mMaxThread = int.Parse(tbMaxThread.Text);

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

            if (cbCompatibility.Checked == true)
                m__G.oCam[0].mFAL.mCheckCompatibility = true;
            else
                m__G.oCam[0].mFAL.mCheckCompatibility = false;

            m__G.oCam[0].mFAL.LoadFMICandidate();

            m__G.oCam[0].mFAL.BackupFMI();
            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bEulerRotation;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23

            long beginTime = 0;
            long endTime = 0;
            long lTimerFrequency = 0;
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            double sx = 0;
            double sy = 0;
            double sz = 0;
            double tx = 0;
            double ty = 0;
            double tz = 0;

            double pommx = 0;
            double pommy = 0;
            double pommz = 0;
            double pommtx = 0;
            double pommty = 0;
            double pommtz = 0;

            double pscsX = 0;
            double pscsY = 0;
            double psctX = 0;
            double psctY = 0;

            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double umscale = 5.5 / Global.LensMag;                           //  rad to min

            SetDefaultMarkConfig(false);

            int lmaxThread = m__G.mMaxThread;
            int frmCnt = m__G.oCam[0].mTargetTriggerCount;

            //tbVsnLog.Text += "Target Trigger Count = " + frmCnt.ToString() + "\r\n";

            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].SetTriggeredframeCount(frmCnt);
            m__G.oCam[0].SetSaveLostMarkFrame(false);
            if (extfrmCnt == 0)
            {
                if (imgIndex > m__G.oCam[0].mTargetTriggerCount)
                    imgIndex = m__G.oCam[0].mTargetTriggerCount;
            }
            else
                imgIndex = frmCnt;


            bool IsShow = cbSaveNthImg.Checked;

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            //System.Drawing.Point[] markPos = null;
            strtmp = "";
            string[] strGrp = new string[2] { "conc", "Std" };
            for (int ci = 0; ci < numFMIcandidate; ci++)
            //for (int ci = 0; ci < 1; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                ///

                //Save 위치 변경

                for (int findex = 0; findex < imgIndex; findex++)
                {
                    if (IsShow)
                    {
                        string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                        if (!Directory.Exists(fileName))
                            Directory.CreateDirectory(fileName);

                        fileName += "Ana" + findex.ToString() + ".bmp";
                        m__G.oCam[0].SaveGrabbedImage(findex, fileName);
                    }
                }

                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;

                SupremeTimer.QueryPerformanceCounter(ref beginTime);
                m__G.fVision.ProcessVisionData(imgIndex, lmaxThread);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapsedTime = (endTime - beginTime) / (double)(lTimerFrequency);

                strtmp = strGrp[ci % 2] + "#\tLed\tX\tY\tZ\tTX\tTY\tTZ\tX1\tY1\tX2\tY2\tX3\tY3\tX4\tY4\tX5\tY5";
                for (int findex = 0; findex < imgIndex; findex++)
                {
                    //if (IsShow)
                    //{
                    //    string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\ImgAna\\";
                    //    if (!Directory.Exists(fileName))
                    //        Directory.CreateDirectory(fileName);

                    //    fileName += "Ana" + findex.ToString() + ".bmp";
                    //    m__G.oCam[0].SaveGrabbedImage(findex, fileName);
                    //}

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";
                    strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mGrabAbsTiming[findex].ToString("F5") + "\t";

                    sx = m__G.oCam[0].mC_pX[findex] * umscale;
                    sy = m__G.oCam[0].mC_pY[findex] * umscale;
                    sz = m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx = m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty = m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz = m__G.oCam[0].mC_pTZ[findex] * minscale;

                    pommx = m__G.oCam[0].mPOMM_X[findex] * umscale;
                    pommy = m__G.oCam[0].mPOMM_Y[findex] * umscale;
                    pommz = m__G.oCam[0].mPOMM_Z[findex] * umscale;
                    pommtx = m__G.oCam[0].mPOMM_TX[findex] * minscale;
                    pommty = m__G.oCam[0].mPOMM_TY[findex] * minscale;
                    pommtz = m__G.oCam[0].mPOMM_TZ[findex] * minscale;

                    pscsX = m__G.oCam[0].mPOMM_sX[findex];
                    pscsY = m__G.oCam[0].mPOMM_sY[findex];
                    psctX = m__G.oCam[0].mPOMM_tX[findex];
                    psctY = m__G.oCam[0].mPOMM_tY[findex];


                    strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";

                    if (!m__G.m_bPseudoOMM)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                            strtmp += m__G.oCam[0].mAzimuthPts[findex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[findex][i].Y.ToString("F3") + "\t";
                        }
                    }
                    else
                    {
                        strtmp += pommx.ToString("F2") + "\t" + pommy.ToString("F2") + "\t" + pommz.ToString("F2") + "\t" + pommtx.ToString("F2") + "\t" + pommty.ToString("F2") + "\t" + pommtz.ToString("F2") + "\t";
                        strtmp += pscsX.ToString("F3") + "\t" + pscsY.ToString("F3") + "\t" + psctX.ToString("F3") + "\t" + psctY.ToString("F3") + "\t";
                    }
                    if (findex % 100 == 99)
                    {
                        if (ci == 0)
                        {
                            tbInfo.Text += strtmp;
                        }
                        else
                        {
                            tbVsnLog.Text += strtmp;
                        }
                        strtmp = "";
                    }
                }
                tbInfo.Text += strtmp + "\r\n" + ellapsedTime.ToString("F3") + "sec";
            }

            m__G.mFAL.mCandidateIndex = 0;
            ChangeFiducialMark(0);
            m__G.oCam[0].mFAL.RecoverFromBackupFMI();

            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();
            //tbImgNumber.Text = (imgIndex + 1).ToString();
        }

        public bool mDoneWriteRun = false;


        private void btnFOVUp_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrU2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }

            if (mbMotorizedStage)
            {
                if (cbZaxis.Checked)
                {
                    MotorJogRun(Axis.Z, false, speedLevel);   // Z
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TX, true, speedLevel);   // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, true, speedLevel);   // TZ
                }
                else
                {
                    MotorJogRun(Axis.Y, true, speedLevel);   // Y
                }
            }
        }

        private void btnFOVUp_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrU;
            if (mbMotorizedStage)
            {
                //MotorJogStop();

                if (cbZaxis.Checked)
                {
                    MotorJogStop(Axis.Z);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TX);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.Y);
                }
            }
        }

        private void btnFOVLeft_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL2;
            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TY, false, speedLevel);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, false, speedLevel);  // TZ
                }
                else
                {
                    MotorJogRun(Axis.X, true, speedLevel);   // X
                }
            }
        }

        private void btnFOVLeft_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL;
            if (mbMotorizedStage)
            {
                //MotorJogStop();

                if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TY);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.X);
                }
            }
        }

        private void btnFOVDown_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {

                if (cbZaxis.Checked)
                {
                    MotorJogRun(Axis.Z, true, speedLevel);  // Z
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TX, false, speedLevel);  // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, false, speedLevel);  // TZ
                }
                else
                {
                    MotorJogRun(Axis.Y, false, speedLevel);  // Y
                }

            }
        }

        private void btnFOVDown_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD;
            if (mbMotorizedStage)
            {
                // MotorJogStop();

                if (cbZaxis.Checked)
                {
                    MotorJogStop(Axis.Z);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TX);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.Y);
                }
            }
        }

        private void btnFOVRight_MouseDown(object sender, MouseEventArgs e)
        {

            btnFOVRight.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrR2;

            SpeedLevel speedLevel;

            if (radioButton10Step.Checked)
            {
                speedLevel = SpeedLevel.Fast; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speedLevel = SpeedLevel.Normal; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speedLevel = SpeedLevel.Slow;   // Slow
            }
            else
            {
                return;
            }


            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(Axis.TY, true, speedLevel);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(Axis.TZ, true, speedLevel);   // TZ
                }
                else
                {
                    MotorJogRun(Axis.X, false, speedLevel);   // X
                }
            }
        }

        private void btnFOVRight_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVRight.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrR;
            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogStop(Axis.TY);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(Axis.TZ);
                }
                else
                {
                    MotorJogStop(Axis.X);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (tbImgNumber.Text == "")
            {
                MessageBox.Show("Please input the image number!");
                return;
            }

            m__G.oCam[0].mFAL.LoadFMICandidate();

            int imgIndex = Convert.ToInt16(tbImgNumber.Text);

            if (imgIndex == 0)
            {
                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                m__G.oCam[0].mFAL.BackupFMI();
            }

            string strtmp = NthMeasure(imgIndex);
            strtmp += "\r\n" + imgIndex.ToString() + "\t";
            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[imgIndex][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t";
            }

            tbInfo.Text += strtmp;

            tbInfo.SelectionStart = tbVsnLog.Text.Length;
            tbInfo.ScrollToCaret();

            int nextFrame = 0;
            if (rb1step.Checked)
                nextFrame = imgIndex - 1;
            else
                nextFrame = imgIndex - 5;
            if (nextFrame < 0)
                nextFrame = 0;

            tbImgNumber.Text = nextFrame.ToString();
        }
        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueP;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMP;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.Text.Contains("Replay"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex / 2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
                lbtn.BackgroundImage = Properties.Resources.BtnLightBlueN;
            else if (lbtn.Text.Contains("N'th") || lbtn.Text.Contains("Mark") || lbtn.Text.Contains("Meas") || lbtn.Text.Contains("Noise"))
                lbtn.BackgroundImage = Properties.Resources.BtnMN;
            else if (lbtn.TabIndex == 440 || lbtn.TabIndex == 456)
                lbtn.BackgroundImage = Properties.Resources.BtnGN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        private void btnAutoLearn_Click(object sender, EventArgs e)
        {
            if (m__G.mFAL == null)
            {
                return;
            }
            m__G.mFAL.Show();
            m__G.mFAL.BringToFront();
            m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //mFAL.Size = new Size(1920, 1045);
            m__G.mFAL.Location = new System.Drawing.Point(0, 0);

            Thread ThreadWaitFALConfirmed = new Thread(() => WaitFALConfirmed());
            ThreadWaitFALConfirmed.Start();

        }
        private void WaitFALConfirmed()
        {
            m__G.mDoingStatus = "Model Configuration";

            while (true)
            {
                Thread.Sleep(50);
                if (m__G.mFAL.mbConfirmed)
                    break;
            }
            m__G.mFAL.mbConfirmed = false;

            m__G.oCam[0].ResetModelScale(1.0 / m__G.mFAL.mModelScale);
            m__G.mDoingStatus = "Checking Vision";
        }
        private void button11_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].HaltA();
            m__G.mDoingStatus = "Checking Vision";

            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            //cbContinuosMode.Checked = true;
            Thread.Sleep(100);

            if (tbFrameToGrab.Text.Length > 0)
                m__G.oCam[0].mTargetTriggerCount = Convert.ToInt32(tbFrameToGrab.Text);
            else
                m__G.oCam[0].mTargetTriggerCount = 1000;

            if (m__G.oCam[0].mTargetTriggerCount > 3000)
                m__G.oCam[0].mTargetTriggerCount = 3000;

            m__G.oCam[0].mRequestedTriggerCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].dAFZM_FrameCount = m__G.oCam[0].mTargetTriggerCount;
            m__G.oCam[0].mTrgBufLength = m__G.oCam[0].mTargetTriggerCount;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[m__G.oCam[0].dAFZM_FrameCount];

            m__G.mbSuddenStop[0] = true;
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in button11()");
            int lgrabbedFrame = 0;
            double frameRate = 0;

            //SetOrgExposure(0);

            if (cbContinuosMode.Checked)
            {
                tbVsnLog.Text += "Target Trigger Count = " + m__G.oCam[0].mTargetTriggerCount.ToString();
                for (int i = 0; i < m__G.oCam[0].mTargetTriggerCount; i++)
                    m__G.oCam[0].GrabB(i, true);

                tbGrabbedFrame.Text = m__G.oCam[0].mTargetTriggerCount.ToString();
                label10.Text = " ~ frame/sec";
                //m__G.fVision.SetOrgExposure(0);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;

                tbVsnLog.Text = "Continuous Mode Grab.\r\n";
                tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                tbVsnLog.ScrollToCaret();
                return;
            }


            //m__G.oCam[0].mFinishVisionData = true;
            m__G.oCam[0].mFinishVisionData = false;
            m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);
            m__G.oCam[0].mFinishVisionData = false;

            m__G.fGraph.Drive_LEDs(0, 0);

            tbGrabbedFrame.Text = lgrabbedFrame.ToString();
            tbImgNumber.Text = m__G.oCam[0].mTargetTriggerCount.ToString();
            label10.Text = frameRate.ToString("F1") + " frame/sec";

            //m__G.fVision.SetOrgExposure(0);
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

            string lstr = "";
            for (int i = 0; i < lgrabbedFrame; i++)
                lstr += m__G.oCam[0].mGrabAbsTiming[i].ToString("F4") + "\r\n";

            tbVsnLog.Text = lstr;
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
        }

        public void btnTriggerGrab()
        {
            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in btnTriggerGrab()");
            int lgrabbedFrame = 0;
            double frameRate = 0;

            //SetOrgExposure(0);

            m__G.oCam[0].ExternalTriggerOrg(ref frameRate, ref lgrabbedFrame);

            //lgrabbedFrame = m__G.oCam[1].ExternalTriggerOrg(ref frameRate);

            tbGrabbedFrame.Text = lgrabbedFrame.ToString();
            label10.Text = frameRate.ToString("F1") + " frame/sec";

            //m__G.fVision.SetOrgExposure(0);

            string lstr = "";
            for (int i = 0; i < lgrabbedFrame; i++)
                lstr += m__G.oCam[0].mGrabAbsTiming[i].ToString("F4") + "\r\n";

            tbInfo.Text = lstr;
            tbInfo.SelectionStart = tbInfo.Text.Length;
            tbInfo.ScrollToCaret();
        }

        //private void button13_Click(object sender, EventArgs e)
        //{
        //    m__G.oCam[0].ClearDisp();
        //    m__G.oCam[0].DrawClear();
        //    if (m__G.mCamCount > 1)
        //    {
        //        m__G.oCam[1].ClearDisp();
        //        m__G.oCam[1].DrawClear();
        //    }

        //    //button13.Enabled = false;

        //    Thread ThreadReplayL = new Thread(() => Process_TriggerReplay(0));
        //    ThreadReplayL.Start();
        //    //m_replayIndex = 0;
        //}
        //public void Process_TriggerReplay(int num)
        //{
        //    int max = m__G.oCam[0].dAFZM_FrameCount;
        //    for (int n = 0; n < max; n++)
        //    {
        //        m__G.oCam[0].BufCopy2Disp_TRG(n);
        //    }
        //    //button13.Enabled = true;
        //}

        private void tbVsnLog_TextChanged(object sender, EventArgs e)
        {

        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    m__G.fGraph.Drive_LEDs(mLEDcurrent[0], mLEDcurrent[1]);
        //    m__G.fVision.SetOrgExposure(0);

        //    SupremeTimer.QueryPerformanceFrequency(ref m__G.TimerFrequency);

        //    long startTime = 0;
        //    long endTime = 0;
        //    double Ellapsed = 0;

        //    SupremeTimer.QueryPerformanceCounter(ref startTime);
        //    double[] avgBin = new double[1000];
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        m__G.oCam[0].GrabA(i);
        //        m__G.oCam[0].GetAvgBin(i, ref avgBin[i]);
        //    }

        //    m__G.fGraph.Drive_LEDs(0, 0);
        //    SupremeTimer.QueryPerformanceCounter(ref endTime);
        //    Ellapsed = 1000000 * (endTime - startTime) / (double)(m__G.TimerFrequency);
        //    string lstr = "";
        //    for (int i = 0; i < 1000; i++)
        //        lstr += avgBin[i].ToString("F3") + "\r\n";

        //    tbVsnLog.Text = lstr;
        //    tbVsnLog.Text += "Ellapsed " + Ellapsed.ToString("F0") + "usec " + mLEDcurrent[0].ToString("F2") + " " + mLEDcurrent[1].ToString("F2") + "\r\n";
        //    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
        //    tbVsnLog.ScrollToCaret();
        //}

        bool mStopMonitorMTF = false;
        //private void btnMonitorMTF_Click(object sender, EventArgs e)
        //{
        //    if (btnMonitorMTF.Text == "Stop Monitor MTF")
        //    {
        //        mStopMonitorMTF = true;
        //        btnMonitorMTF.Text = "Monitor MTF";
        //        ChartMTF.Hide();
        //        m__G.mDoingStatus = "IDLE";
        //        m__G.mIDLEcount = 0;

        //    }
        //    else
        //    {
        //        mStopMonitorMTF = false;
        //        btnMonitorMTF.Text = "Stop Monitor MTF";
        //        ChartMTF.Show();
        //        Thread threadMonitorMTF = new Thread(() => MonitorMTF());
        //        threadMonitorMTF.Start();
        //        m__G.mDoingStatus = "Checking Vision";
        //    }
        //}
        double[][] mSeriesMTF = new double[12][];

        //public void MonitorMTF()
        //{
        //    for (int i = 0; i < 12; i++)
        //        mSeriesMTF[i] = new double[1000];

        //    int waitTime = 0;
        //    while (true)
        //    {
        //        if (m__G.fVision.mLoaded)
        //            break;
        //        Thread.Sleep(200);
        //        if (waitTime++ > 50)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    mStopMonitorMTF = true;
        //                    btnMonitorMTF.Text = "Monitor MTF";
        //                });
        //            }
        //            return;
        //        }
        //    }
        //    waitTime = 0;
        //    while (true)
        //    {
        //        if (m__G.oCam[0].mFAL.mFAutoLearnLoaded)
        //            break;
        //        Thread.Sleep(200);
        //        if (waitTime++ > 50)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    mStopMonitorMTF = true;
        //                    btnMonitorMTF.Text = "Monitor MTF";
        //                });
        //            }
        //            return;
        //        }
        //    }
        //    //m__G.fVision.SetOrgExposure(0);

        //    int index = 0;
        //    //int cindex = 0;
        //    int effFrameCount = 0;
        //    bool[] res = new bool[12] { true, true, true, true, true, true, true, true, true, true, true, true };
        //    Thread[] threadRealTimeVisionData = new Thread[12];

        //    m__G.oCam[0].GrabA_User(0);
        //    m__G.oCam[0].FineCOG(true, 0, 0);
        //    index++;
        //    //int oldindex = 0;
        //    //int indexThread = 0;
        //    int updatePlot = 0;
        //    long lTimerFrequency = 0;
        //    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

        //    //double lRealTimeUnit = 8.0 / 500.0;    //  Default Value
        //    double[] maMTF = new double[7];
        //    while (!mStopMonitorMTF)
        //    {
        //        int indexNow = index % 12;
        //        int index9999 = index % 1000 + 1;

        //        m__G.oCam[0].GrabB(3000);

        //        if (mStopMonitorMTF)
        //            break;

        //        m__G.oCam[0].FineMTF(false, index9999, 0);

        //        updatePlot++;
        //        //    //  MTF 그래프를 그리기 위해 측정된 데이터를 그래프 버퍼에 복사해준다.
        //        //if (index < 1000)
        //        //{
        //        //    for (int i = 0; i < 12; i++)
        //        //        Array.Copy(m__G.oCam[0].mBufMTF[i], 0, mSeriesMTF[i], 0, index);
        //        //}
        //        //else
        //        //{
        //        //    //Array.Copy(m__G.oCam[0].mC_pX, index9999, mStroke[0], 0, 1000 - (index9999));
        //        //    for (int i = 0; i < 12; i++)
        //        //        Array.Copy(m__G.oCam[0].mBufMTF[i], index9999, mSeriesMTF[i], 0, 1000 - (index9999));

        //        //}

        //        for (int i = 0; i < 6; i++)
        //            maMTF[i] += m__G.oCam[0].mBufMTF[i][index9999];

        //        if (mStopMonitorMTF)
        //            break;

        //        if (updatePlot % 20 == 1)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    //PlotChartMTF(effFrameCount);
        //                    string lstr = "";
        //                    for (int i = 0; i < 6; i++)
        //                    {
        //                        lstr += maMTF[i].ToString("F0") + "\t";
        //                        maMTF[6] += maMTF[i];
        //                    }

        //                    tbVsnLog.Text += lstr + "\t" + maMTF[6].ToString("F0") + "\r\n";
        //                    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
        //                    tbVsnLog.ScrollToCaret();
        //                    for (int i = 0; i < 7; i++)
        //                        maMTF[i] = 0;

        //                });
        //            }
        //        }
        //        Thread.Sleep(1);
        //        if (mStopMonitorMTF)
        //            break;

        //        index++;
        //        if (effFrameCount < 1000)
        //            effFrameCount++;
        //        else
        //            effFrameCount = 1000;

        //    }
        //    m__G.fGraph.Drive_LEDs(0, 0);
        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();
        //    m__G.oCam[0].dAFZM_FrameCount = index % 1000;
        //    m__G.mbSuddenStop[0] = false;
        //}

        //public void PlotChartMTF(int frameCnt)
        //{
        //    if (ChartMTF.InvokeRequired)
        //    {
        //        ChartMTF.Invoke(new MethodInvoker(delegate ()
        //        {
        //            _PlotChartMTF(frameCnt);
        //        }));
        //    }
        //    else
        //        _PlotChartMTF(frameCnt);
        //    //ChartMTF
        //}
        //public void _PlotChartMTF(int frameCnt)
        //{
        //    ChartMTF.Series.Clear();


        //    if (frameCnt == 0)
        //    {
        //        ChartMTF.Series.Add("Default");
        //        ChartMTF.Series[0].Points.AddXY(1, 0);
        //        return;
        //    }
        //    for (int i = 0; i < frameCnt; i++)
        //    {
        //        for (int which = 0; which < 12; which++)
        //        {
        //            if (mSeriesMTF[which][i] == 0) continue;
        //            ChartMTF.Series[0].Points.AddXY(i / 1000.0, mSeriesMTF[which][i]); //  FWD
        //        }
        //    }
        //    ChartMTF.ChartAreas[0].AxisX.Minimum = 0;
        //    ChartMTF.ChartAreas[0].AxisX.Maximum = (frameCnt / 1000.0);
        //    ChartMTF.ChartAreas[0].AxisX.Interval = 0.1;
        //    ChartMTF.ChartAreas[0].AxisY.Minimum = 0;
        //    ChartMTF.ChartAreas[0].AxisY.Maximum = 1;
        //    ChartMTF.ChartAreas[0].AxisY.Interval = 0.1;

        //    ChartMTF.Titles.Clear();
        //    ChartMTF.Titles.Add("MTF");


        //}
        private void button4_Click_1(object sender, EventArgs e)
        {

            ScaleNOpticalRotation();

        }
        public void ScaleNOpticalRotation()
        {

            double scaleTop = 0;
            double scaleSide = 0;
            double rotTop = 0;
            double rotSide = 0;

            if (!m__G.oCam[0].FineCOG(true, 0, 0))
            {
                tbVsnLog.Text += "fail to detect marks.\r\n";
            }
            else
            {
                //  획득된 마크 좌표와, 마크모델에 저장되어있는 실측마크좌표를 이용해 Scale 을 구하고 \\DoNotTouch\\ScaleNOpticalR.txt 에 저장한다.
                m__G.oCam[0].FindScaleNOpticalRotation(0, m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt");
                m__G.oCam[0].GetScaleNOpticalR(ref scaleTop, ref scaleSide, ref rotTop, ref rotSide);
                //  Master Mockup 의 설계치 기준으로 Scale 및 광학계 회전각도를 계산한다.

                string lstr = "";
                lstr += "Scale Top\t" + scaleTop.ToString("F4") + "\r\n";
                lstr += "Scale Side\t" + scaleSide.ToString("F4") + "\r\n";
                lstr += "Rotation Top\t" + rotTop.ToString("F4") + "\r\n";
                lstr += "Rotation Side\t" + rotSide.ToString("F4") + "\r\n";

                tbVsnLog.Text += lstr;
            }
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
        }

        bool[] mb_FinishCalcVision = new bool[30];
        public int[] mDebugCalcVisionCount = new int[30];
        public double ProcessVisionData(int count, int HowManyThread = 10 /* Max 16 */, bool IsFile = false)
        {
            if (m__G.mbSuddenStop[0])   //  연산 시작도 안함.
            {
                m__G.oCam[0].mFinishVisionData = true;  //  맞다
                return 0;
            }

            m__G.oCam[0].mFinishVisionData = false;
            double ltime = 0;
            long lTimerFrequency = 1000;
            long startTime = 0;
            long endTime = 0;
            SupremeTimer.QueryPerformanceCounter(ref startTime);

            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.


            m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, IsFile);    // 마크찾기
            if (!m__G.m_bHideAllGraph) m__G.fManage.AddViewLog(string.Format("FineCOG 0\r\n"));
            if (count < HowManyThread)
            {
                mb_FinishCalcVision[0] = false;
                mDebugCalcVisionCount[0] = 0;
                m__G.oCam[0].FineCOG(true, 0, 0, false, true, false, IsFile);    // 마크찾기
                if (count > 1)
                {
                    CalcVisionData(0, 0, count, 1, 0, IsFile);
                }
                mb_FinishCalcVision[0] = true;
                m__G.oCam[0].mFinishVisionData = true;    //  맞다
                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                ltime = (endTime - startTime) / (double)(lTimerFrequency);
                return ltime;
            }


            Task[] Task_CalcVisionData = new Task[HowManyThread];

            //int halfCnt = count / 2;
            //int lastIndx = count - 1;
            //int halfThread = HowManyThread / 2;
            //HowManyThread = halfThread * 2;

            List<int> taskIndices = new List<int>();
            List<int> lastImgIndex = new List<int>();
            for (int i = 0; i < HowManyThread; i++)
            {
                mb_FinishCalcVision[i] = false;
                mDebugCalcVisionCount[i] = 0;

                taskIndices.Add(i);
            }

            if (HowManyThread <= 1)
                CalcVisionData(0, 0, count, 1, 1, IsFile);
            else
            {
                m__G.fManage.AddViewLog(string.Format("FineCOG Parallel\r\n"));

                Parallel.ForEach(taskIndices, taskIndex =>
                {
                    CalcVisionData(0, taskIndex, count, HowManyThread, taskIndex, IsFile);
                });
            }

            m__G.oCam[0].mFinishVisionData = true;   //  맞다

            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
            SupremeTimer.QueryPerformanceCounter(ref endTime);
            ltime = (endTime - startTime) / (double)(lTimerFrequency);

            return ltime;
        }


        //public void CalcVisionData(int cam, int ci, int ce, int iBuf, out double[] dPosX, out double[] dPosY, out double[] dPosZ, out double[] dTX, out double[] dTY, out double[] dTZ)
        //{
        //    dPosX = new double[ce - ci];
        //    dPosY = new double[ce - ci];
        //    dPosZ = new double[ce - ci];
        //    dTX = new double[ce - ci];
        //    dTY = new double[ce - ci];
        //    dTZ = new double[ce - ci];
        //    int i = 0;
        //    mb_FinishCalcVision[iBuf] = false;
        //    try
        //    {
        //        for (i = ci; i < ce; i++)    //  Skip 0 ~ 59
        //            m__G.oCam[cam].FineCOG(i, iBuf, ref dPosX[i], ref dPosY[i], ref dPosZ[i], ref dTX[i], ref dTX[i], ref dTX[i]);    // 마크찾기

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //    mb_FinishCalcVision[iBuf] = true;
        //}

        private void cbContinuosMode_CheckedChanged(object sender, EventArgs e)
        {
            m__G.oCam[0].HaltA();

            btnLive2.Enabled = false;
            btnAllLEDOn.Enabled = false;
            btnLEDDOWN.Enabled = false;
            btnLEDUP.Enabled = false;
            btnHalt2.Enabled = false;
            button11.Enabled = false;
            cbContinuosMode.Enabled = false;

            if (cbContinuosMode.Checked)
                CameraReset(2, true);
            else
                CameraReset(2, false);

            btnLive2.Enabled = true;
            btnAllLEDOn.Enabled = true;
            btnLEDDOWN.Enabled = true;
            btnLEDUP.Enabled = true;
            btnHalt2.Enabled = true;
            button11.Enabled = true;
            cbContinuosMode.Enabled = true;

            Process currentProc = Process.GetCurrentProcess();
            long memoryUsed = currentProc.PrivateMemorySize64;
            tbVsnLog.Text += "Memory\t" + (memoryUsed / 1000000.0).ToString("F3") + "\tMB is used by the Application\r\n";
        }

        public void CameraReset(int grabgerType = 1, bool IsContinuous = true)
        {
            if (BaslerCam[0] == null) return;
            String[] strFileName = new string[2] { "", "" };
            //if (m__G.oCam[0].IsInit == true)
            //    m__G.oCam[0].Free();

            //m__G.oCam[0] = new MILlib(1.0);

            //Thread.Sleep(100);

            string strPath = m__G.m_RootDirectory + "\\RunData\\";
            int lHroi = (m__G.mHROI / 10) * 10;

            strFileName[0] = strPath + "Continuous_10tap_" + m__G.mVROI[1].ToString() + "_" + lHroi.ToString() + "R.dcf";
            strFileName[1] = strPath + "ExtTrg_10tap_" + m__G.mVROI[1].ToString() + "_" + lHroi.ToString() + "R.dcf";

            if (!File.Exists(strFileName[0]))
            {
                tbVsnLog.Text += strFileName[0] + " not found. Fail to change mode.";
                return;
            }


            string lSystemName = "M_SYSTEM_SOLIOS";
            if (grabgerType == 2)
                lSystemName = "M_SYSTEM_RADIENTEVCL";

            if (!m__G.m_bSwap)
            {
                //MessageBox.Show("Camera Normal");
                if (IsContinuous)
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("Off");


                    m__G.oCam[0].ChangeDataFormat(0, strFileName[0]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[0]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
                else
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                    m__G.oCam[0].ChangeDataFormat(0, strFileName[1]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[1]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
            }
            else
            {
                //MessageBox.Show("Camera Swapped");
                if (IsContinuous)
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("Off");
                    m__G.oCam[0].ChangeDataFormat(0, strFileName[0]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 1, 0, strFileName[0]);
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
                else
                {
                    BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

                    m__G.oCam[0].ChangeDataFormat(0, strFileName[1]);
                    //m__G.oCam[0].Init(m__G.mVROI[0], m__G.mHROI, m__G.mVROIstep, 0, lSystemName, 0, 0, strFileName[1]);  //  COM4  
                    ChangeROIHeight(0, m__G.mVROI[0]);
                }
            }
            //m__G.oCam[0].SelectWindow(panelCam0.Handle);

            //m__G.mFAL.Show();
            //m__G.mFAL.ShowMarkDGV();
            //m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //m__G.mFAL.Location = new Point(0, 0);
            //m__G.mFAL.Hide();
        }

        private void btnFitFOV_Click(object sender, EventArgs e)
        {
            /////////////////////////////////////////////////////
            /////////////////////////////////////////////////////
            //  To Check Memory Leakage 
            //for ( int i=0; i< 100; i++)
            //{
            //    cbContinuosMode.Checked = true;
            //    Thread.Sleep(1000);
            //    while (btnGrab2.Enabled != true)
            //        Thread.Sleep(100);

            //    Thread.Sleep(500);

            //    cbContinuosMode.Checked = false;
            //    Thread.Sleep(1000);
            //    while (btnGrab2.Enabled != true)
            //        Thread.Sleep(100);

            //    Thread.Sleep(500);

            //}
            //return;
            /////////////////////////////////////////////////////
            /////////////////////////////////////////////////////


            //  960 - 360 continuous 모드로 변경한다
            //  여기서 각 마크의 위치를 찾는다. 
            //  마크 위치만 찾 Extract6D() 는 수행하지 않는다.
            //  마크 위치에 따라 FOV 위치가 342 pixel 상/하단에서 마진을 동일하게 가지도록 미세 조정한다.
            //  FOV DYOffet = YOffsetCur - YOffsetOld
            //  960 - 342 trigger mode 로 변경한다.
            Thread threadFitFOV = new Thread(() => FitFOV());
            threadFitFOV.Start();

        }

        public void FitFOV()
        {
            //  960 - 360 continuous 모드로 변경한다
            //  여기서 각 마크의 위치를 찾는다. 
            //  마크 위치만 찾 Extract6D() 는 수행하지 않는다.
            //  마크 위치에 따라 FOV 위치가 342 pixel 상/하단에서 마진을 동일하게 가지도록 미세 조정한다.
            //  FOV DYOffet = YOffsetCur - YOffsetOld
            //  960 - 342 trigger mode 로 변경한다.
            //  영상 획득해서 보여준다.

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    cbContinuosMode.Checked = true;
                    LiveView.Size = new System.Drawing.Size(LiveView.Size.Width, 627);
                });
            else
            {
                cbContinuosMode.Checked = true;
                LiveView.Size = new System.Drawing.Size(LiveView.Size.Width, 627);
            }

            Thread.Sleep(1000);

            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);
            m__G.oCam[0].GrabB(0);

            m__G.oCam[0].FineCOG(true, 0, 0, true, false);

            string strtmp = "";

            List<double> markYtop = new List<double>();
            List<double> markYbtm = new List<double>();

            for (int i = 0; i < 12; i++)
            {
                if (m__G.oCam[0].mAzimuthPts[0][i].X == 0) continue;
                strtmp += m__G.oCam[0].mAzimuthPts[0][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[0][i].Y.ToString("F3") + "\t";


            }
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < m__G.oCam[0].mFAL.mFidMarkSide.Count; j++)
                {
                    if (m__G.oCam[0].m_sMR[i].Azimuth == m__G.oCam[0].mFAL.mFidMarkSide[j].Azimuth)
                    {
                        double ltop = m__G.oCam[0].m_sMR[i].pos.Y - m__G.oCam[0].mFAL.mFidMarkSide[j].modelSize.Height / 2;
                        markYtop.Add(ltop);
                        double lbtm = m__G.oCam[0].m_sMR[i].pos.Y + m__G.oCam[0].mFAL.mFidMarkSide[j].modelSize.Height / 2;
                        markYbtm.Add(lbtm);
                    }
                }

                for (int j = 0; j < m__G.oCam[0].mFAL.mFidMarkTop.Count; j++)
                {
                    if (m__G.oCam[0].m_sMR[i].Azimuth == (m__G.oCam[0].mFAL.mFidMarkTop[j].Azimuth + 8))
                    {
                        double lbtm = m__G.oCam[0].m_sMR[i].pos.Y + m__G.oCam[0].mFAL.mFidMarkTop[j].modelSize.Height / 2;
                        markYbtm.Add(lbtm);
                    }
                }
            }
            double[] lmarkYtop = markYtop.ToArray();
            double[] lmarkYbtm = markYbtm.ToArray();
            Array.Sort(lmarkYtop);
            Array.Sort(lmarkYbtm);
            double ytop = lmarkYtop[0];
            double ybtm = lmarkYbtm[lmarkYbtm.Length - 1];

            //  영상 획득은 YROI = 360 pixel 으로 획득한 뒤 mVROI 에 맞춰서 FOV 를 이동한다.
            int fovShiftY = (int)((ytop - 10) / Math.Sin(40 / 180.0 * Math.PI) - (mVROI - ybtm - 10)) / 2;
            v_OrgROIV_min[0] += fovShiftY;
            ChangeROIYOffsetDeltaY(0, fovShiftY);
            SaveOrgROI(1);

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += "FOV Shift Delta Y = " + fovShiftY.ToString() + "\r\n";
                    tbInfo.SelectionStart = tbInfo.Text.Length;
                    tbInfo.ScrollToCaret();
                    cbContinuosMode.Checked = false;
                    LiveView.Size = new System.Drawing.Size(LiveView.Size.Width, (int)(LiveView.Size.Height * mVROI / 360.0));
                });
            else
            {
                tbInfo.Text += "FOV Shift Delta Y = " + fovShiftY.ToString() + "\r\n";
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
                cbContinuosMode.Checked = false;
                LiveView.Size = new System.Drawing.Size(LiveView.Size.Width, (int)(LiveView.Size.Height * mVROI / 360.0));
            }

            Thread.Sleep(1000);

            m__G.oCam[0].GrabB(0);
            SaveOrgROI(1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            m__G.mDoingStatus = "Checking Vision";
            for (int i = 0; i < 50; i++)
                m__G.oCam[0].GrabB(i);



            m__G.oCam[0].CalcBackgroundNoise(0, 50, 0);

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv");
            for (int i = 0; i < m__G.oCam[0].nSizeX; i++)
                wr.WriteLine(m__G.oCam[0].mBackgroundNoise[i].ToString());
            wr.Close();

            wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv");
            for (int i = 0; i < m__G.oCam[0].nSizeY; i++)
                wr.WriteLine(m__G.oCam[0].mBackgroundNoiseY[i].ToString());
            wr.Close();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void LoadBackbroundNoise()
        {
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv")) return;

            try
            {
                StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoise.csv");
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                m__G.oCam[0].mBackgroundNoise = new int[eachLine.Length];
                for (int i = 0; i < eachLine.Length; i++)
                    m__G.oCam[0].mBackgroundNoise[i] = byte.Parse(eachLine[i]);

            }
            catch (Exception e)
            {
                return;
            }
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv")) return;

            try
            {
                StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\BgNoiseY.csv");
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                m__G.oCam[0].mBackgroundNoiseY = new int[eachLine.Length];
                for (int i = 0; i < eachLine.Length; i++)
                    m__G.oCam[0].mBackgroundNoiseY[i] = byte.Parse(eachLine[i]);

            }
            catch (Exception e)
            {
                return;
            }
            m__G.oCam[0].mFAL.SetBackgroundNoise(m__G.oCam[0].mBackgroundNoise, m__G.oCam[0].mBackgroundNoiseY);
        }

        string[] mModelFileList = null;
        public void SetModelFileList(string[] flist)
        {
            mModelFileList = new string[flist.Length];
            for (int i = 0; i < flist.Length; i++)
            {
                mModelFileList[i] = flist[i];
            }
        }
        public void TransferModelFileList()
        {
            if (m__G == null)
                return;

            if (m__G.oCam[0] == null)
                return;

            if (m__G.oCam[0].mFAL == null)
                return;

            m__G.oCam[0].mFAL.SetFMICandidates(mModelFileList);
        }

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    //  Default Mark Position
        //    System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
        //        new System.Drawing.Point( 730, 78 ),
        //        new System.Drawing.Point( 234, 93 ),
        //        new System.Drawing.Point( 730, 255 ),
        //        new System.Drawing.Point( 234, 275 ),
        //        new System.Drawing.Point( 439, 294 ),
        //        new System.Drawing.Point( 532, 294 ) };

        //    m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

        //    if (m__G.mFAL != null)
        //    {
        //        string markPosFile = m__G.mFAL.GetFileNameOfMarkPosOnPanel();
        //        if (File.Exists(markPosFile))
        //        {
        //            StreamReader sr = new StreamReader(markPosFile);
        //            string allLines = sr.ReadToEnd();
        //            sr.Close();
        //            List<System.Drawing.Point> mPos = new List<System.Drawing.Point>();
        //            string[] eachLine = allLines.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //            for (int i = 0; i < eachLine.Length; i++)
        //            {
        //                if (eachLine[i].Length < 3)
        //                    continue;
        //                string[] xypos = eachLine[i].Split(',');
        //                if (xypos.Length < 2)
        //                    continue;
        //                System.Drawing.Point lp = new System.Drawing.Point();
        //                lp.X = int.Parse(xypos[0]);
        //                lp.Y = int.Parse(xypos[1]);
        //                mPos.Add(lp);
        //            }
        //            if (mPos.Count > 0)
        //            {
        //                markPos = mPos.ToArray();
        //            }
        //        }
        //    }

        //    m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
        //}

        private void cbEnhancedLED_CheckedChanged(object sender, EventArgs e)
        {

        }

        public double ms_sinTheta = 0.64278761;    //  sin(40deg)
        public double[] ms_scaleX = new double[3] { 0, 1, 0 };
        public double[] ms_scaleY = new double[3] { 0, 1, 0 };
        public double[] ms_scaleZ = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTX = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTY = new double[3] { 0, 1, 0 };
        public double[] ms_scaleTZ = new double[3] { 0, 1, 0 };
        public double ms_EastViewYPscale = 1.0;
        public double[] ms_XtoYbyView = new double[3];
        public double[] ms_XtoZbyView = new double[3];
        public double[] ms_XtoTXbyView = new double[3];
        public double[] ms_XtoTYbyView = new double[3];
        public double[] ms_XtoTZbyView = new double[3];
        public double[] ms_YtoXbyView = new double[3];
        public double[] ms_YtoZbyView = new double[3];
        public double[] ms_YtoTXbyView = new double[3];
        public double[] ms_YtoTYbyView = new double[3];
        public double[] ms_YtoTZbyView = new double[3];
        public double[] ms_ZtoXbyView = new double[3];
        public double[] ms_ZtoYbyView = new double[3];
        public double[] ms_ZtoTXbyView = new double[3];
        public double[] ms_ZtoTYbyView = new double[3];
        public double[] ms_ZtoTZbyView = new double[3];
        public double[] ms_TXtoTYbyView = new double[3];
        public double[] ms_TXtoTZbyView = new double[3];
        public double[] ms_TYtoTXbyView = new double[3];
        public double[] ms_TYtoTZbyView = new double[3];
        public double[] ms_TZtoTXbyView = new double[3];
        public double[] ms_TZtoTYbyView = new double[3];
        public double[] ms_XJtoXbyView = new double[2];
        public double[] ms_YJtoYbyView = new double[2];
        public double[] ms_ZJtoZbyView = new double[2];
        public double[] ms_TZtoZbyView = new double[3];
        //private void btnCalcScales_Click(object sender, EventArgs e)
        //{
        //    double rX_NtoS = 0;
        //    //double rY_NStoE = 0;
        //    double rY_dY = 0;
        //    double rZ_dZ = 0;

        //    double mX_NtoS = 0;
        //    double mYside_dY = 0;
        //    double mY_dY = 0;
        //    double mYside_dZ = 0;


        //    try
        //    {
        //        if (tb_rX_NtoS.Text.Length > 0)
        //            rX_NtoS = double.Parse(tb_rX_NtoS.Text);
        //        if (tb_rY_dY.Text.Length > 0)
        //            rY_dY = double.Parse(tb_rY_dY.Text);
        //        if (tb_rZ_dZ.Text.Length > 0)
        //            rZ_dZ = double.Parse(tb_rZ_dZ.Text);

        //        if (tb_mX_NtoS.Text.Length > 0)
        //            mX_NtoS = double.Parse(tb_mX_NtoS.Text);        //  Top View 에서 N 과 S 마크간 X 거리 입력할 것, scaleX 무시하고 pixel 단위로 입력할 것
        //        if (tb_mYside_dY.Text.Length > 0)
        //            mYside_dY = double.Parse(tb_mYside_dY.Text);  //  Side View 에서 N-S 와 E 마크간 Y 거리 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tb_mY_dY.Text.Length > 0)
        //            mY_dY = double.Parse(tb_mY_dY.Text);            //  Y 이동에 따른 Top View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tb_mYside_dZ.Text.Length > 0)
        //            mYside_dZ = double.Parse(tb_mYside_dZ.Text);        //  Z 이동에 따른 Side View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것

        //        mX_NtoS = mX_NtoS * 0.0055 / Global.LensMag;
        //        mYside_dY = mYside_dY * 0.0055 / Global.LensMag;
        //        mY_dY = mY_dY * 0.0055 / Global.LensMag;
        //        mYside_dZ = mYside_dZ * 0.0055 / Global.LensMag;

        //        ms_scaleX = rX_NtoS / mX_NtoS;
        //        ms_scaleY = rY_dY / mY_dY;
        //        ms_sinTheta = mYside_dY / rY_dY;
        //        double cosTheta = Math.Sqrt(1 - ms_sinTheta * ms_sinTheta);
        //        ms_scaleZ = rZ_dZ / (mYside_dZ * ms_scaleY / cosTheta);

        //        tbScaleX.ForeColor = Color.Black;
        //        tbScaleY.ForeColor = Color.Black;
        //        tbScaleZ.ForeColor = Color.Black;
        //        tbSinTheta.ForeColor = Color.Black;

        //        tbScaleX.Text = ms_scaleX.ToString("F4");
        //        tbScaleY.Text = ms_scaleY.ToString("F4");
        //        tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //        tbSinTheta.Text = ms_sinTheta.ToString("F4");
        //    }
        //    catch (Exception exc)
        //    {
        //        ;
        //    }

        //}

        private void btnUpdateScales_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                 ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                 ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                 ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                 ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                 ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
            if (ms_sinTheta > 0)
                m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
            else
                m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

            SaveScaleNTheta();

            tbInfo.Size = new System.Drawing.Size(tbInfo.Size.Width + 400, tbInfo.Size.Height);
            tbInfo.Location = new System.Drawing.Point(tbInfo.Location.X - 400, tbInfo.Location.Y);
            tbVsnLog.Size = new System.Drawing.Size(tbVsnLog.Size.Width + 400, tbVsnLog.Size.Height);
            tbVsnLog.Location = new System.Drawing.Point(tbVsnLog.Location.X - 400, tbVsnLog.Location.Y);
            bScaleUpdating = false;

            m__G.oCam[0].DrawClear();
            DrawMarkPositions();
        }

        public void SaveScaleNTheta()
        {
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);

            wr.WriteLine($"{ms_sinTheta}");

            wr.WriteLine($"{ms_scaleX[0]:E5}\t{ms_scaleX[1]:E5}\t{ms_scaleX[2]:E5}\t// Tab 분리, X scale : aX^2 + bX + c");
            wr.WriteLine($"{ms_scaleY[0]:E5}\t{ms_scaleY[1]:E5}\t{ms_scaleY[2]:E5}\t// Tab 분리. Y scale");
            wr.WriteLine($"{ms_scaleZ[0]:E5}\t{ms_scaleZ[1]:E5}\t{ms_scaleZ[2]:E5}\t// Tab 분리, Z scale");
            wr.WriteLine($"{ms_scaleTX[0]:E5}\t{ms_scaleTX[1]:E5}\t{ms_scaleTX[2]:E5}\t// Tab 분리, TX scale");
            wr.WriteLine($"{ms_scaleTY[0]:E5}\t{ms_scaleTY[1]:E5}\t{ms_scaleTY[2]:E5}\t// Tab 분리, TY scale");
            wr.WriteLine($"{ms_scaleTZ[0]:E5}\t{ms_scaleTZ[1]:E5}\t{ms_scaleTZ[2]:E5}\t// Tab 분리, TZ scale");

            wr.WriteLine($"{ms_EastViewYPscale:E5}\t// Tab 분리, EastView Y pixel Scale");

            wr.WriteLine($"{ms_XtoYbyView[0]:E5}\t{ms_XtoYbyView[1]:E5}\t{ms_XtoYbyView[2]:E5}\t// Tab 분리, X to Y coef");
            wr.WriteLine($"{ms_XtoZbyView[0]:E5}\t{ms_XtoZbyView[1]:E5}\t{ms_XtoZbyView[2]:E5}\t// Tab 분리, X to Z coef");
            wr.WriteLine($"{ms_XtoTXbyView[0]:E5}\t{ms_XtoTXbyView[1]:E5}\t{ms_XtoTXbyView[2]:E5}\t// Tab 분리, X to TX coef");
            wr.WriteLine($"{ms_XtoTYbyView[0]:E5}\t{ms_XtoTYbyView[1]:E5}\t{ms_XtoTYbyView[2]:E5}\t// Tab 분리, X to TY coef");
            wr.WriteLine($"{ms_XtoTZbyView[0]:E5}\t{ms_XtoTZbyView[1]:E5}\t{ms_XtoTZbyView[2]:E5}\t// Tab 분리, X to TZ coef");

            wr.WriteLine($"{ms_YtoXbyView[0]:E5}\t{ms_YtoXbyView[1]:E5}\t{ms_YtoXbyView[2]:E5}\t// Tab 분리, Y to X coef");
            wr.WriteLine($"{ms_YtoZbyView[0]:E5}\t{ms_YtoZbyView[1]:E5}\t{ms_YtoZbyView[2]:E5}\t// Tab 분리, Y to Z coef");
            wr.WriteLine($"{ms_YtoTXbyView[0]:E5}\t{ms_YtoTXbyView[1]:E5}\t{ms_YtoTXbyView[2]:E5}\t// Tab 분리, Y to TX coef");
            wr.WriteLine($"{ms_YtoTYbyView[0]:E5}\t{ms_YtoTYbyView[1]:E5}\t{ms_YtoTYbyView[2]:E5}\t// Tab 분리, Y to TY coef");
            wr.WriteLine($"{ms_YtoTZbyView[0]:E5}\t{ms_YtoTZbyView[1]:E5}\t{ms_YtoTZbyView[2]:E5}\t// Tab 분리, Y to TZ coef");

            wr.WriteLine($"{ms_ZtoXbyView[0]:E5}\t{ms_ZtoXbyView[1]:E5}\t{ms_ZtoXbyView[2]:E5}\t// Tab 분리, Z to X coef");
            wr.WriteLine($"{ms_ZtoYbyView[0]:E5}\t{ms_ZtoYbyView[1]:E5}\t{ms_ZtoYbyView[2]:E5}\t// Tab 분리, Z to Y coef");
            wr.WriteLine($"{ms_ZtoTXbyView[0]:E5}\t{ms_ZtoTXbyView[1]:E5}\t{ms_ZtoTXbyView[2]:E5}\t// Tab 분리, Z to TX coef");
            wr.WriteLine($"{ms_ZtoTYbyView[0]:E5}\t{ms_ZtoTYbyView[1]:E5}\t{ms_ZtoTYbyView[2]:E5}\t// Tab 분리, Z to TY coef");
            wr.WriteLine($"{ms_ZtoTZbyView[0]:E5}\t{ms_ZtoTZbyView[1]:E5}\t{ms_ZtoTZbyView[2]:E5}\t// Tab 분리, Z to TZ coef");

            wr.WriteLine($"{ms_TXtoTYbyView[0]:E5}\t{ms_TXtoTYbyView[1]:E5}\t{ms_TXtoTYbyView[2]:E5}\t// Tab 분리, TX to TY coef");
            wr.WriteLine($"{ms_TXtoTZbyView[0]:E5}\t{ms_TXtoTZbyView[1]:E5}\t{ms_TXtoTZbyView[2]:E5}\t// Tab 분리, TX to TZ coef");
            wr.WriteLine($"{ms_TYtoTXbyView[0]:E5}\t{ms_TYtoTXbyView[1]:E5}\t{ms_TYtoTXbyView[2]:E5}\t// Tab 분리, TY to TX coef");
            wr.WriteLine($"{ms_TYtoTZbyView[0]:E5}\t{ms_TYtoTZbyView[1]:E5}\t{ms_TYtoTZbyView[2]:E5}\t// Tab 분리, TY to TZ coef");
            wr.WriteLine($"{ms_TZtoTXbyView[0]:E5}\t{ms_TZtoTXbyView[1]:E5}\t{ms_TZtoTXbyView[2]:E5}\t// Tab 분리, TZ to TX coef");
            wr.WriteLine($"{ms_TZtoTYbyView[0]:E5}\t{ms_TZtoTYbyView[1]:E5}\t{ms_TZtoTYbyView[2]:E5}\t// Tab 분리, TZ to TY coef");

            wr.WriteLine($"{ms_XJtoXbyView[0]:E5}\t{ms_XJtoXbyView[1]:E5}\t// Tab 분리, XY XZ to X coef");
            wr.WriteLine($"{ms_YJtoYbyView[0]:E5}\t{ms_YJtoYbyView[1]:E5}\t// Tab 분리, YX Y to X coef");
            wr.WriteLine($"{ms_ZJtoZbyView[0]:E5}\t{ms_ZJtoZbyView[1]:E5}\t// Tab 분리, ZX ZY to X coef");
            wr.WriteLine($"{ms_TZtoZbyView[0]:E5}\t{ms_TZtoZbyView[1]:E5}\t{ms_TZtoZbyView[2]:E5}\t// Tab 분리, TZ to Z coef");
            wr.Close();

            AddVsnLog("Saved scales");

        }
        // 옛날 스테이지 Scale
        public bool SKLoadScaleNTheta()
        {
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
                StreamWriter orgwr = new StreamWriter(scaleFile);
                string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
                              "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
                              "1.00\t// Tab 분리, Z scale\r\n" +
                              "0.64278761\r\n" +
                              "1.00\t// Tab 분리, TX scale\r\n" +
                              "1.00\t// Tab 분리, TY scale\r\n" +
                              "0.00\t// Tab 분리, Z to X coef\r\n" +
                              "0.00\t// Tab 분리, Z to Y coef\r\n" +
                              "0.00\t// Tab 분리, Y to X coef\r\n" +
                              "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
                              "0.00\t// Tab 분리, X to Y coef\r\n" +
                              "0.00\t// Tab 분리, X to Z coef\r\n" +
                              "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
                              "0.00\t// Tab 분리, X to TX coef\r\n" +
                              "0,00\t// Tab 분리, Y to TY coef\r\n" +
                              "0.00\t// Tab 분리, FX0 of 6 axis stage, default value = 0, X Offset of the center of Fiducial Mark\r\n" +
                              "0.00\t// Tab 분리, FY0 of 6 axis stage, default value = 0, Y Offset of the center of Fiducial Mark\r\n" +
                              "55.00\t// / Tab 분리, L1 of 6 axis stage TY\r\n" +
                              "55.00\t// / Tab 분리, L2 of 6 axis stage TY\r\n" +
                              "0.00\t// / Tab 분리, Y Offset of Probe TY\r\n" +
                              "55.00\t// / Tab 분리, Y Offset of Probe TX\r\n" +
                              "32.30\t// / Tab 분리, Probe X Rx\r\n" +
                              "32.30\t// / Tab 분리, Probe Y Ry\r\n";
                orgwr.Write(istr);
                orgwr.Close();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 4)
                    return false;

                double[][] dScales = new double[][]
                {
                    ms_scaleX, ms_scaleY, ms_scaleZ, new double[3], ms_scaleTX, ms_scaleTY,
                    ms_ZtoXbyView, ms_ZtoYbyView,
                    ms_YtoXbyView, ms_YtoZbyView,
                    ms_XtoYbyView, ms_XtoZbyView, new double[3], ms_XtoTXbyView, ms_YtoTXbyView,
                };

                double[] dCenterOfFiducialMarkOffset = new double[8]
                {
                    0.0, 0.0, 55.0, 55.0, 0.0, 55.0, 32.0, 32.0
                };

                for (int i = 0; i < eachLine.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (i == 3)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 12)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);
                    }
                    else if (i < 15)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                    else if (i < 23)
                    {

                        dCenterOfFiducialMarkOffset[i - 15] = double.Parse(strdata[0]);
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );

                double Fx = dCenterOfFiducialMarkOffset[0];
                double Fy = dCenterOfFiducialMarkOffset[1];
                double L1 = dCenterOfFiducialMarkOffset[2];
                double L2 = dCenterOfFiducialMarkOffset[3];
                double txL1 = dCenterOfFiducialMarkOffset[4];
                double txL2 = dCenterOfFiducialMarkOffset[5];
                double Rx = dCenterOfFiducialMarkOffset[6];
                double Ry = dCenterOfFiducialMarkOffset[7];

                m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2, txL1, txL2, Rx, Ry);

                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        // 자화 스테이지 Scale
        public bool JHLoadScaleNtheta()
        {
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
                StreamWriter orgwr = new StreamWriter(scaleFile);
                string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
                                "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
                                "1.00\t// Tab 분리, Z scale\r\n" +
                                "0.64278761\r\n" +
                                "1.00\t// Tab 분리, TX scale: aTX^2 + bTX + c\r\n" +
                                "1.00\t// Tab 분리, TY scale\r\n" +
                                "0.00\t// Tab 분리, Z to X coef\r\n" +
                                "0.00\t// Tab 분리, Z to Y coef\r\n" +
                                "0.00\t// Tab 분리, Y to X coef\r\n" +
                                "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
                                "0.00\t// Tab 분리, X to Y coef\r\n" +
                                "0.00\t// Tab 분리, X to Z coef: aX^2 + bX + c\r\n" +
                                "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
                                "0.00\t// Tab 분리, X to TX coef\r\n" +
                                "0.00\t// Tab 분리, Y to TX coef\r\n";
                orgwr.Write(istr);
                orgwr.Close();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 4)
                    return false;

                double[][] dScales = new double[][]
                {
                    ms_scaleX, ms_scaleY, ms_scaleZ, new double[3], ms_scaleTX, ms_scaleTY,
                    ms_ZtoXbyView, ms_ZtoYbyView,
                    ms_YtoXbyView, ms_YtoZbyView,
                    ms_XtoYbyView, ms_XtoZbyView, new double[3], ms_XtoTXbyView, ms_YtoTXbyView
                };

                for (int i = 0; i < dScales.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (i == 3)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 12)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);
                    }
                    else
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void AnosisInitial(bool isRemote = false)
        {
            //if (InvokeRequired)
            //{
            //    BeginInvoke((MethodInvoker)delegate
            //    {
            //        AnosisIndex.Text = "0";
            //        mAutoCalibrationIndex = 0;
            //    });

            //}
            //else
            //{
            //    AnosisIndex.Text = "0";
            //    mAutoCalibrationIndex = 0;
            //}
            mAutoCalibrationIndex = 0;
            AnosisisStop = false;
            AnosisisResume = false;

            //MotorMoveHome6D();
            m__G.fManage.M_H();

            StartLive();
            GrabInitalMark();
            GrabToFindMark(false);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbVsnLog.Text = "";
                    tbInfo.Text = "";
                });

            }
            else
            {
                tbVsnLog.Text = "";
                tbInfo.Text = "";
            }

            if (isRemote) m__G.fManage.A_I();
        }
        public class Anosismeasure
        {
            public double[] X;
            public double[] Y;
            public double[] Z;
            public double[] TX;
            public double[] TY;
            public double[] TZ;
            public int count;

            public Anosismeasure(int cnt)
            {
                count = cnt;
                X = new double[cnt];
                Y = new double[cnt];
                Z = new double[cnt];
                TX = new double[cnt];
                TY = new double[cnt];
                TZ = new double[cnt];
            }
        }
        public bool AnosisisStop = false;
        public bool AnosisisResume = false;
        public Anosismeasure Mval;
        // 하이드리드 스테이지 Scale
        public bool LoadScaleNTheta()
        {
            //MessageBox.Show("LoadscaleNTheta called ");
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                InitializeScaleNTheta();
                SaveScaleNTheta();
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 29)
                    return false;
                string[] eachData = new string[eachLine.Length];

                double[][] dScales = new double[33][]
                {
                    new double[3] ,ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, new double[3],
                    ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                    ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                    ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                    ms_TXtoTYbyView, ms_TXtoTZbyView,
                    ms_TYtoTXbyView, ms_TYtoTZbyView,
                    ms_TZtoTXbyView, ms_TZtoTYbyView,
                    ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                    ms_TZtoZbyView
                };

                for (int i = 0; i < eachLine.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (i == 0)
                    {
                        ms_sinTheta = double.Parse(strdata[0]);
                    }
                    else if (i == 7)
                    {
                        ms_EastViewYPscale = double.Parse(strdata[0]);

                    }
                    else if (i < 32 && i > 28)
                    {
                        dScales[i][0] = double.Parse(strdata[0]);
                        dScales[i][1] = double.Parse(strdata[1]);
                    }
                    else
                    {
                        if (strdata.Length > 3)
                        {
                            dScales[i][0] = double.Parse(strdata[0]);
                            dScales[i][1] = double.Parse(strdata[1]);
                            dScales[i][2] = double.Parse(strdata[2]);
                        }
                        else
                        {
                            dScales[i][0] = 0.0;
                            dScales[i][1] = double.Parse(strdata[0]);
                            dScales[i][2] = 0.0;
                        }
                    }
                }
                m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                 ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                 ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                 ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                 ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                 ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                 ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                 ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                 ms_TZtoZbyView
                                                 );
                AddVsnLog("Loaded scales");

                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                long compressedCalV = (long)(Math.Pow(10, 15) * (1 - 7.0 / (dScales.SelectMany(arr => arr).Sum() + ms_sinTheta + ms_EastViewYPscale)));

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\t" + "CompressedCalibrationValue : " + compressedCalV + "\r\n");
                //m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ[1].ToString("F5") + "\r\n");

                bool isUncalibrated = false;
                // default scaleNtheta
                //if ((ms_scaleX[0] == 0 && ms_scaleX[1] == 1 && ms_scaleX[0] == 0) ||
                //    ms_EastViewYPscale == 1 || ms_TZtoZbyView[1] == 0)
                if ((ms_scaleX[0] == 0 && ms_scaleX[1] == 1 && ms_scaleX[0] == 0) ||
                    ms_EastViewYPscale == 1)
                {
                    isUncalibrated = true;
                }

                SettbUnCalibratedInfoVisible(isUncalibrated);
                m__G.fManage.SettbUncalibratedInfoVisible(isUncalibrated);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        bool bScaleUpdateValid = false;
        bool bDedicatedScaleUpdateValid = false;
        bool bScaleUpdating = false;
        private void tbInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = true;

            if (e.KeyCode == Keys.D)
                bDedicatedScaleUpdateValid = true;
        }

        //private void tbInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (bScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X + 410, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X + 410, tbVsnLog.Location.Y);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //            //tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X - 410, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X - 410, tbVsnLog.Location.Y);
        //            bScaleUpdating = false;
        //        }
        //    }else if (bDedicatedScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //            //tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = false;
        //        }
        //    }
        //}

        private void tbVsnLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = true;
        }

        //private void tbVsnLog_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (bScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 400, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X + 400, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 400, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X + 400, tbVsnLog.Location.Y);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");
        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 400, tbInfo.Size.Height);
        //            tbInfo.Location = new Point(tbInfo.Location.X - 400, tbInfo.Location.Y);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 400, tbVsnLog.Size.Height);
        //            tbVsnLog.Location = new Point(tbVsnLog.Location.X - 400, tbVsnLog.Location.Y);
        //            bScaleUpdating = false;
        //        }
        //    }
        //    else if (bDedicatedScaleUpdateValid)
        //    {
        //        if (!bScaleUpdating)
        //        {
        //            tbInfo.BringToFront();
        //            tbVsnLog.BringToFront();
        //            tbInfo.Size = new Size(tbInfo.Size.Width - 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width - 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = true;

        //            //tbScaleX.ForeColor = Color.LightGray;
        //            //tbScaleY.ForeColor = Color.LightGray;
        //            //tbScaleZ.ForeColor = Color.LightGray;
        //            //tbSinTheta.ForeColor = Color.LightGray;
        //            //tbScaleX.Text = ms_scaleX.ToString("F4");
        //            //tbScaleY.Text = ms_scaleY.ToString("F4");
        //            //tbScaleZ.Text = ms_scaleZ.ToString("F4");
        //            //tbSinTheta.Text = ms_sinTheta.ToString("F4");

        //        }
        //        else
        //        {
        //            tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //            tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //            bScaleUpdating = false;
        //        }
        //    }
        //}

        private void tbInfo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = false;

        }

        private void tbVsnLog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                bScaleUpdateValid = false;

        }

        private void cbLiveWithMarks_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLiveWithMarks.Checked)
            {

                // 250326 Live with Mark 타이밍 수정
                // tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                // bLiveFindMark = true;
                //Task.Run(() => LiveFindMark());              
            }
            else
            {
                // 250326 Live with Mark 타이밍 수정
                // bLiveFindMark = false;
                //tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
            }

            // 250326 Live with Mark 타이밍 수정
            // 이미 Live 중 cbLiveWithMarks를 변경했을 때
            if (!bHaltLive)
            {
                StartLive();
            }

        }

        public double[] mDYforScale = new double[10];
        //private void btnYNmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Y 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Y 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Side View N/S 마크의 Y 좌표 평균을 저장한다.

        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    double tX = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    for ( int i=0; i<5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //    }
        //    mDYforScale[0] = tY / 10;
        //    mDYforScale[1] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    tb_mX_NtoS.Text = (tX / 5).ToString("F3");

        //    StartLive();

        //}

        //private void btnYPmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Y 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Y 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Side View N/S 마크의 Y 좌표 평균을 저장한다.

        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    double tX = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //    }
        //    mDYforScale[2] = tY / 10;
        //    mDYforScale[3] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double dYonTop = Math.Abs(mDYforScale[2] - mDYforScale[0]);
        //    double dYonSide = Math.Abs(mDYforScale[3] - mDYforScale[1]);

        //    double ntX = double.Parse(tb_mX_NtoS.Text);
        //    tX = (ntX + tX / 5) / 2;
        //    tb_mX_NtoS.Text = tX.ToString("F3");
        //    tb_mYside_dY.Text = dYonSide.ToString("F3");
        //    tb_mY_dY.Text = dYonTop.ToString("F3");

        //    StartLive();

        //}

        //private void btnZNmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Z 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Z 축으로 - value 만큼 이동한 상태에 대한 측정치로서 Side View N/S/E 마크의 Y 좌표 평균을 저장한다.
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //    }
        //    mDYforScale[4] = tY / 10;
        //    mDYforScale[5] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    StartLive();

        //}

        //private void btnZPmeasure_Click(object sender, EventArgs e)
        //{
        //    //  Z 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Top View N/S 마크의 Y 좌표 평균을 저장한다.
        //    //  Z 축으로 + value 만큼 이동한 상태에 대한 측정치로서 Side View N/S/E 마크의 Y 좌표 평균을 저장한다.
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double tY = 0;
        //    double sY = 0;
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        FindMarks();
        //        tY += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //    }
        //    mDYforScale[6] = tY / 10;
        //    mDYforScale[7] = sY / 10;

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double dYforDZonTop = Math.Abs(mDYforScale[6] - mDYforScale[4]);
        //    double dYforDZonSide = Math.Abs(mDYforScale[7] - mDYforScale[5]);

        //    dYforDZonSide = dYforDZonSide - dYforDZonTop * Math.Sin(40 / 180.0 * Math.PI);
        //    tb_mYside_dZ.Text = dYforDZonSide.ToString("F3");

        //    StartLive();

        //}

        //private void btnPairedMarkUpdateScale_Click(object sender, EventArgs e)
        //{
        //    ms_scaleX = double.Parse(tbPairedMarkScaleX.Text);
        //    ms_scaleY = double.Parse(tbPairedMarkScaleY.Text);
        //    ms_scaleZ = double.Parse(tbPairedMarkScaleZ.Text);
        //    ms_sinTheta = double.Parse(tbPairedMarkSinTheta.Text);

        //    m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);
        //    if (ms_sinTheta > 0)
        //        m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
        //    else
        //        m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

        //    SaveScaleNTheta();

        //    tbInfo.Size = new Size(tbInfo.Size.Width + 410, tbInfo.Size.Height);
        //    tbVsnLog.Size = new Size(tbVsnLog.Size.Width + 410, tbVsnLog.Size.Height);
        //    bScaleUpdating = false;
        //    m__G.oCam[0].DrawClear();
        //    DrawMarkPositions();
        //}

        //private void btnMeasurePairedMark_Click(object sender, EventArgs e)
        //{
        //    //  1차 각 ROI 를 상측으로 50% 이동해서 5회 측정 평균 구한다.
        //    //  2차 각 ROI 를 하측으로 50% 이동해서 5회 측정 평균 구한다.
        //    //   ROI 를 원상복귀 시킨다.
        //    //   Top View 에서 N-S 마크들 간 X 거리의 평균치를 계산한다.
        //    //   Top View 에서 Paired 마크들 간 Y 거리의 평균치를 계산한다.
        //    //   Side View 에서 Paired 마크들 간 Y 거리의 평균치를 계산한다.
        //    //   결과치를 tbAvgXfromNtoSTop, tbAvgYbetweenPairTop, tbAvgYbetweenPairSide 에 표시한다.

        //    m__G.mDoingStatus = "Checking Vision";
        //    DrawMarkPositions();

        //    m__G.oCam[0].mFAL.LoadFMICandidate();
        //    m__G.oCam[0].mFAL.BackupFMI();

        //    double[] tY = new double[2];
        //    double[] sY = new double[2];
        //    double[] tX = new double[2];
        //    m__G.oCam[0].GrabB(1);
        //    FindMarks();
        //    m__G.oCam[0].DrawClear();
        //    //  상측으로 ROI 이동한 상태에서 상측 mark 만 측정
        //    for (int i = 0; i < 5; i++)
        //    {
        //        m__G.oCam[0].GrabB(1);
        //        //foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        //{
        //        //    OpenCvSharp.Rect rc = lFmark.searchRoi;
        //        //    rc.Y = rc.Y - rc.Height / 5;
        //        //    lFmark.searchRoi.Y = rc.Y;
        //        //}
        //        //foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        //{
        //        //    OpenCvSharp.Rect rc = lFmark.searchRoi;
        //        //    rc.Y = rc.Y - rc.Height / 5;
        //        //    lFmark.searchRoi.Y = rc.Y;
        //        //}
        //        FindMarks();
        //        tY[0] += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;
        //        sY[0] += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX[0] += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;

        //        //  하측으로 ROI 이동한 상태에서 상측 mark 만 측정

        //        OpenCvSharp.Rect[] rc = new OpenCvSharp.Rect[12];
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        {
        //            rc[lFmark.Azimuth] = new OpenCvSharp.Rect();
        //            rc[lFmark.Azimuth] = lFmark.searchRoi;
        //            lFmark.searchRoi.Y = (int)(m__G.oCam[0].mAzimuthPts[1][lFmark.Azimuth].Y  + 3);
        //        }
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        {
        //            rc[lFmark.Azimuth + 8] = new OpenCvSharp.Rect();
        //            rc[lFmark.Azimuth + 8] = lFmark.searchRoi;
        //            lFmark.searchRoi.Y = (int)(m__G.oCam[0].mAzimuthPts[1][lFmark.Azimuth + 8].Y  + 3);
        //        }
        //        FindMarks();
        //        tY[1] += m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][10].Y;  
        //        sY[1] += m__G.oCam[0].mAzimuthPts[1][0].Y + m__G.oCam[0].mAzimuthPts[1][4].Y;
        //        tX[1] += m__G.oCam[0].mAzimuthPts[1][8].X - m__G.oCam[0].mAzimuthPts[1][10].X;
        //                //  ROI 복구
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkSide)
        //        {
        //            lFmark.searchRoi.Y = rc[lFmark.Azimuth].Y;
        //        }
        //        foreach (FAutoLearn.FAutoLearn.sFiducialMark lFmark in m__G.oCam[0].mFAL.mFidMarkTop)
        //        {
        //            lFmark.searchRoi.Y = rc[lFmark.Azimuth + 8].Y;
        //        }
        //    }

        //    m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        //    double avgXfromXtoS = (tX[0] + tX[1]) / 10;
        //    double avgYbetweenPairTop = Math.Abs((tY[0] - tY[1]) / 10);
        //    double avgYbetweenPairSide = Math.Abs((sY[0] - sY[1]) / 10);

        //    tbAvgXfromNtoSTop.Text = avgXfromXtoS.ToString("F4");
        //    tbAvgYbetweenPairTop.Text = avgYbetweenPairTop.ToString("F4");
        //    tbAvgYbetweenPairSide.Text = avgYbetweenPairSide.ToString("F4");

        //    m__G.mDoingStatus = "IDLE";

        //    StartLive();
        //}

        //private void btnPairedMarkCalcScale_Click(object sender, EventArgs e)
        //{
        //    double rAvgX_NtoS = 0;
        //    double rAvgY_betweenPair = 0;

        //    double mX_NtoS = 0;
        //    double mY_betweenPairTop = 0;
        //    double mY_betweenPairSide = 0;

        //    try
        //    {
        //        if (tbRealAvgXfromNtoS.Text.Length > 0)
        //            rAvgX_NtoS = double.Parse(tbRealAvgXfromNtoS.Text);
        //        if (tbRealAvgYbetweenPair.Text.Length > 0)
        //            rAvgY_betweenPair = double.Parse(tbRealAvgYbetweenPair.Text);

        //        if (tbAvgXfromNtoSTop.Text.Length > 0)
        //            mX_NtoS = double.Parse(tbAvgXfromNtoSTop.Text);        //  Top View 에서 N 과 S 마크간 X 거리 입력할 것, scaleX 무시하고 pixel 단위로 입력할 것
        //        if (tbAvgYbetweenPairTop.Text.Length > 0)
        //            mY_betweenPairTop = double.Parse(tbAvgYbetweenPairTop.Text);  //  Side View 에서 N-S 와 E 마크간 Y 거리 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것
        //        if (tbAvgYbetweenPairSide.Text.Length > 0)
        //            mY_betweenPairSide = double.Parse(tbAvgYbetweenPairSide.Text);            //  Y 이동에 따른 Top View 에서의 Y 변동량 입력할 것, scaleY 무시하고 pixel 단위로 입력할 것

        //        mX_NtoS = mX_NtoS * 0.0055 / Global.LensMag;
        //        mY_betweenPairTop = mY_betweenPairTop * 0.0055 / Global.LensMag;
        //        mY_betweenPairSide = mY_betweenPairSide * 0.0055 / Global.LensMag;

        //        ms_scaleX = rAvgX_NtoS / mX_NtoS;
        //        ms_scaleY = rAvgY_betweenPair / mY_betweenPairTop;
        //        ms_sinTheta = mY_betweenPairSide / mY_betweenPairTop;
        //        double cosTheta = Math.Sqrt(1 - ms_sinTheta * ms_sinTheta);
        //        double cos40 = Math.Cos(40 / 180.0 * Math.PI);
        //        ms_scaleZ = ms_scaleY;  // = cos40 / cosTheta ;   //  ( 1/cosTheta ) / ( 1/cos40 ) = cos40 / cosTheta
        //                                                    // ms_sinTheta 를 Update 하고나면 Zscale 이 별도 없다.
        //                                                    // 이론적으로보면 Theta 와 Y scale 로 결정된다.
        //                                                    // 이후 발생하는 오차는 영상의 Z 축과 제품의 Z 축이 서로 달라서 발생하는 오차뿐이다.

        //        tbPairedMarkScaleX.ForeColor = Color.Black;
        //        tbPairedMarkScaleY.ForeColor = Color.Black;
        //        tbPairedMarkScaleZ.ForeColor = Color.Black;
        //        tbPairedMarkSinTheta.ForeColor = Color.Black;

        //        tbPairedMarkScaleX.Text = ms_scaleX.ToString("F4");
        //        tbPairedMarkScaleY.Text = ms_scaleY.ToString("F4");
        //        tbPairedMarkScaleZ.Text = ms_scaleZ.ToString("F4");
        //        tbPairedMarkSinTheta.Text = ms_sinTheta.ToString("F4");
        //    }
        //    catch (Exception exc)
        //    {
        //        ;
        //    }
        //}
        public string SaveScreenShot(string strHost)
        {
            if (m__G == null)
                return "";  //  초기화이전

            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            string pngname = strHost + dtNow.ToString("yyMMddhhmmss") + ".png";
            string sScreenCapturePath = m__G.m_RootDirectory + "\\User_ScreenShot\\" + pngname;
            string sDir = m__G.m_RootDirectory + "\\User_ScreenShot";
            Bitmap memoryImage;
            memoryImage = new Bitmap(1920, 1080);
            System.Drawing.Size s = new System.Drawing.Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);


            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
            memoryImage.Save(sScreenCapturePath);
            return sScreenCapturePath;
        }


        private void btnSetMasterTilt_Click(object sender, EventArgs e)
        {
            if (!bLiveFindMark && !bThreadManualFindMarks)
            {
                MessageBox.Show("This button is only available in 'Live with Marks' or 'Grab to Find Marks'", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m__G.mDoingStatus = "Checking Vision";
            m__G.mDoingStatus = "IDLE";

            // 250326 Live with Mark에서만 가능 -> Grab to Find Mark에서도 가능으로 변경
            // 모두 SetOffSet, SignTXTY 리셋하고 GrabToFindMark나 LivewithMark를 다시 찍고 그결과로 OFFSET 값을 구하기. 로 변경

            double orgMasterX = double.Parse(tbMasterX.Text);   // um
            double orgMasterY = double.Parse(tbMasterY.Text);   // um
            double orgMasterZ = double.Parse(tbMasterZ.Text);   // um
            double orgMasterTx = double.Parse(tbMasterTX.Text); // min
            double orgMasterTy = double.Parse(tbMasterTY.Text); // min
            double orgMasterTz = double.Parse(tbMasterTZ.Text); // min

            // 현재 SetSignTXTY의 부호 고려. SignTX가 - 일때 사용자가 tbMasterTX.Text에 n을 입력하면 singn 초기화 값 기준인 orgMasterX -n으로 적용해야함.
            if (m__G.m_bXTiltReverse)
            {
                orgMasterTx *= -1;
            }

            if (m__G.m_bYTiltReverse)
            {
                orgMasterTy *= -1;
            }

            // offset, sign 초기화
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);


            int cntData = mCalibrationFullData.Count;

            while (mCalibrationFullData.Count <= cntData)
            {
                Thread.Sleep(200);
            }

            int lastIdx = mCalibrationFullData.Count - 1;
            double[] lastData = mCalibrationFullData[lastIdx];

            double lx = lastData[0];
            double ly = lastData[1];
            double lz = lastData[2];
            double ltx = lastData[3];
            double lty = lastData[4];
            double ltz = lastData[5];

            ////  ltx, lty, ltz 는 측정값 (min)
            ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)
            ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
            SaveTXTYZeroOffset(lx, ly, lz, ltx, lty, ltz, orgMasterX, orgMasterY, orgMasterZ, orgMasterTx, orgMasterTy, orgMasterTz, true);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);



            //DrawMarkPositions();

            //m__G.oCam[0].GrabB(1);

            //string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\SetZeroGrab.bmp";
            //m__G.oCam[0].SaveImageBuf(fname);

            //m__G.oCam[0].GrabB(2);
            //m__G.oCam[0].GrabB(3);
            //m__G.oCam[0].GrabB(4);
            //m__G.oCam[0].GrabB(5);

            //m__G.oCam[0].mFAL.LoadFMICandidate();
            //m__G.oCam[0].mFAL.BackupFMI();

            // X, Y, Z 추가 25.03.25
            //double lx = 0;
            //double ly = 0;
            //double lz = 0;  
            //double ltx = 0;
            //double lty = 0;
            //double ltz = 0;

            //MessageBox.Show("Call SetTXTYOffset 1");
            // m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
            // m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);

            //for (int i = 1; i < 6; i++)
            //{
            //    FindMarks(i);

            //    lx += m__G.oCam[0].mC_pX[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    ly += m__G.oCam[0].mC_pY[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    lz += m__G.oCam[0].mC_pZ[i]; /** 5.5 / Global.LensMag;    //  Pixel to um*/
            //    ltx += m__G.oCam[0].mC_pTX[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //    lty += m__G.oCam[0].mC_pTY[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //    ltz += m__G.oCam[0].mC_pTZ[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            //}

            //lx = lx / 5;
            //ly = ly / 5;
            //lz = lz / 5;
            //ltx = ltx / 5;
            //lty = lty / 5;
            //ltz = ltz / 5;

            //double masterTx = 0;
            //double masterTy = 0;
            //double masterTz = 0;

            //double orgMasterX = double.Parse(tbMasterX.Text);
            //double orgMasterY = double.Parse(tbMasterY.Text);
            //double orgMasterZ = double.Parse(tbMasterZ.Text);
            //double orgMasterTx = double.Parse(tbMasterTX.Text);
            //double orgMasterTy = double.Parse(tbMasterTY.Text);
            //double orgMasterTz = double.Parse(tbMasterTZ.Text);

            //masterTz = orgMasterTz;
            //try
            //{
            //    if (m__G.m_bXTiltReverse)
            //        masterTx = -orgMasterTx;
            //    else
            //        masterTx = orgMasterTx;
            //}
            //catch (Exception err)
            //{
            //    tbMasterTX.Text = "0";
            //}
            //try
            //{
            //    if (m__G.m_bYTiltReverse)
            //        masterTy = -orgMasterTy;
            //    else
            //        masterTy = orgMasterTy;
            //}
            //catch (Exception err)
            //{
            //    tbMasterTY.Text = "0";
            //}

            ////if (m__G.m_bXTiltReverse)
            ////    masterTx = -masterTx;
            ////if (m__G.m_bXTiltReverse)
            ////    masterTy = -masterTy;

            ////  ltx, lty, ltz 는 측정값 (radian)
            ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)  
            ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
            //SaveTXTYZeroOffset(lx, ly, lz, ltx, lty, ltz, masterTx, masterTy, masterTz, orgMasterX, orgMasterY, orgMasterZ, orgMasterTx, orgMasterTy, orgMasterTz);
            //m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            //m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        }
        // offset X, Y, Z 추가 25.03.25
        public void SaveTXTYZeroOffset(double x, double y, double z, double tx, double ty, double tz, double orgMasterX, double orgMasterY, double orgMasterZ, double orgMasterTx, double orgMasterTy, double orgMasterTz, bool isNew = false)
        {

            //MessageBox.Show("Call SetTXTYOffset 2");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset((x - orgMasterX) * Um_To_Pixel, (y - orgMasterY) * Um_To_Pixel, (z - orgMasterZ) * Um_To_Pixel, (tx - orgMasterTx) * MIN_To_RAD, (ty - orgMasterTy) * MIN_To_RAD, (tz - orgMasterTz) * MIN_To_RAD);

            string filename = "";
            int Index = GetMasterZeroIndex();
            filename = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero\\" + Index.ToString() + "_" + camID0 + ".txt";



            //string filename = sFileDir + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine(x.ToString());
            wr.WriteLine(y.ToString());
            wr.WriteLine(z.ToString());
            wr.WriteLine(tx.ToString());
            wr.WriteLine(ty.ToString());
            wr.WriteLine(tz.ToString());

            wr.WriteLine(orgMasterX.ToString());
            wr.WriteLine(orgMasterY.ToString());
            wr.WriteLine(orgMasterZ.ToString());
            wr.WriteLine(orgMasterTx.ToString());
            wr.WriteLine(orgMasterTy.ToString());
            wr.WriteLine(orgMasterTz.ToString());
            wr.Close();
        }

        // offset X, Y, Z 추가 25.03.25
        public string LoadTXTYZeroOffset()
        {
            try
            {
                string sPath = m__G.m_RootDirectory + "\\DoNotTouch";
                if (!Directory.Exists(sPath)) Directory.CreateDirectory(sPath);
                string cPath = sPath + "\\PreviousOffsetIndex.txt";

                if (!File.Exists(cPath))
                {
                    StreamWriter sw = new StreamWriter(cPath);
                    sw.WriteLine("0");
                    sw.Close();
                }

                int curIndex = GetMasterZeroIndex();

                string sFile = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero";
                if (!Directory.Exists(sFile)) Directory.CreateDirectory(sFile);
                int count = GetMasterZeroCount();
                if (count < 1)
                {
                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
                    SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
                    return "X Y Z TX TY TZ offset = 0, 0, 0, 0, 0, 0";
                }

                sFile += "\\" + curIndex.ToString() + "_" + camID0.ToString() + ".txt";
                StreamReader rd = new StreamReader(sFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                double x = double.Parse(eachLine[0]);   // um
                double y = double.Parse(eachLine[1]);   // um
                double z = double.Parse(eachLine[2]);   // um
                double tx = double.Parse(eachLine[3]);  // min
                double ty = double.Parse(eachLine[4]);  // min
                double tz = double.Parse(eachLine[5]);  // min

                double orgMasterX = double.Parse(eachLine[6]);  // um
                double orgMasterY = double.Parse(eachLine[7]);  // um
                double orgMasterZ = double.Parse(eachLine[8]);  // um
                double orgMasterTx = double.Parse(eachLine[9]); // min
                double orgMasterTy = double.Parse(eachLine[10]);    // min
                double orgMasterTz = double.Parse(eachLine[11]);    // min

                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset((x - orgMasterX) * Um_To_Pixel, (y - orgMasterY) * Um_To_Pixel, (z - orgMasterZ) * Um_To_Pixel, (tx - orgMasterTx) * MIN_To_RAD, (ty - orgMasterTy) * MIN_To_RAD, (tz - orgMasterTz) * MIN_To_RAD);

                tbMasterX.Text = orgMasterX.ToString("F2");
                tbMasterY.Text = orgMasterY.ToString("F2");
                tbMasterZ.Text = orgMasterZ.ToString("F2");
                tbMasterTX.Text = (m__G.m_bXTiltReverse ? -orgMasterTx : orgMasterTx).ToString("F2");
                tbMasterTY.Text = (m__G.m_bYTiltReverse ? -orgMasterTy : orgMasterTy).ToString("F2");
                tbMasterTZ.Text = orgMasterTz.ToString("F2");

                return $"X Y Z TX TY TZ offset = {orgMasterX:F2}, {orgMasterY:F2}, {orgMasterZ:F2}, {orgMasterTx:F2}, {orgMasterTy:F2}, {orgMasterTz:F2}";
            }
            catch
            {
                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);

                return "X Y Z TX TY TZ offset = 0, 0, 0, 0, 0, 0";
            }
        }

        ////public OpenCvSharp.Point2d[] mZLUT = null;

        ////public double ApplyZLUT(double Z)
        ////{
        ////    return Z;

        ////    //  Obsolete since 231024
        ////    //if (mZLUT == null)
        ////    //    return Z;

        ////    //int len = mZLUT.Length;
        ////    //for (int i = 0; i < len - 1; i++)
        ////    //{
        ////    //    //  Linear Interpolation
        ////    //    if ( (mZLUT[i].X - Z) * (mZLUT[i + 1].X - Z) < 0)
        ////    //    {
        ////    //        double delta = mZLUT[i].Y + (Z - mZLUT[i].X) * (mZLUT[i + 1].Y - mZLUT[i].Y) / (mZLUT[i + 1].X - mZLUT[i].X);
        ////    //        //MessageBox.Show(mZLUT[i].X.ToString("F3") + " " + Z.ToString() + " " + mZLUT[i + 1].X.ToString("F3") + " " + delta.ToString("F3"));
        ////    //        return Z - delta;
        ////    //    }
        ////    //}

        ////    //if (Z < mZLUT[0].X)
        ////    //    return Z - mZLUT[0].Y;

        ////    //if (Z > mZLUT[len - 1].X)
        ////    //    return Z - mZLUT[len - 1].Y;

        ////    //return Z;
        ////}

        ////public bool GetZLUT(string filename)
        ////{
        ////    mZLUT = null;

        ////    //MessageBox.Show("ZLUTfile = " + filename);    //  파일명 제대로 들어오는지 디버깅
        ////    if (!File.Exists(filename))
        ////        return false;

        ////    List<OpenCvSharp.Point2d> lp = new List<OpenCvSharp.Point2d>();
        ////    StreamReader sr = new StreamReader(filename);
        ////    string allstr = sr.ReadToEnd();
        ////    sr.Close();
        ////    string[] eachLine = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        ////    string debuglstr = "";
        ////    foreach (string lstr in eachLine)
        ////    {
        ////        string[] strElements = lstr.Split('\t');
        ////        double x = double.Parse(strElements[0]);
        ////        double y = double.Parse(strElements[1]);
        ////        OpenCvSharp.Point2d pt = new OpenCvSharp.Point2d(x, y);
        ////        lp.Add(pt);
        ////        debuglstr += pt.X.ToString("F3") + "," + pt.Y.ToString("F3") + "\r\n";
        ////    }
        ////    mZLUT = lp.ToArray();
        ////    //MessageBox.Show(debuglstr);   //  제대로 읽었는지 디버깅


        ////    //double res = ApplyZLUT(700);


        ////    DateTime datetime = DateTime.Now;
        ////    DateTimeOffset datetimeOffset = new DateTimeOffset(datetime);
        ////    long unixTime = datetimeOffset.ToUnixTimeSeconds();

        ////    return true;

        ////}

        private void button6_Click_1(object sender, EventArgs e)
        {
            SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
        }

        string mBinPath = "";
        private void btnOpenResultBin_Click(object sender, EventArgs e)
        {
            string sFilePath = m__G.m_RootDirectory + "\\Data\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "dat";
            openFile.Multiselect = true;
            if (mBinPath == "")
                openFile.InitialDirectory = sFilePath;
            else
                openFile.InitialDirectory = mBinPath;

            openFile.Filter = "Dat(*.dat)|*.dat";
            TextBox[] lTBX = new TextBox[2];
            lTBX[0] = tbInfo;
            lTBX[1] = tbVsnLog;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (openFile.FileNames.Length == 0)
                    return;

                tbVsnLog.Text = "";
                string[] sFileName = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                {
                    sFileName[i] = openFile.FileNames[i];
                    if (!File.Exists(sFileName[i]))
                        return;
                    string lstr = MyOwner.ReadResultBin(sFileName[i]);
                    if (sFileName[i].Contains("_0_"))
                        lTBX[0].Text += lstr + "\r\n";
                    else
                        lTBX[1].Text += lstr + "\r\n";
                }
                mBinPath = sFileName[0].Substring(0, sFileName[0].LastIndexOf("\\"));
            }
        }

        private void cbSetTXTYwithMaster_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSetTXTYwithMaster.Checked)
            {
                MasterList.Enabled = true;
                btnDeleteMaster.Enabled = true;
                btnAddMaster.Enabled = true;
                btnInitialTilt.Enabled = true;
                btnSetMasterTilt.Enabled = true;
                tbMasterX.Enabled = true;
                tbMasterY.Enabled = true;
                tbMasterZ.Enabled = true;
                tbMasterTX.Enabled = true;
                tbMasterTY.Enabled = true;
                tbMasterTZ.Enabled = true;

            }
            else
            {
                MasterList.Enabled = false;
                btnDeleteMaster.Enabled = false;
                btnAddMaster.Enabled = false;
                btnInitialTilt.Enabled = false;
                btnSetMasterTilt.Enabled = false;
                tbMasterX.Enabled = false;
                tbMasterY.Enabled = false;
                tbMasterZ.Enabled = false;
                tbMasterTX.Enabled = false;
                tbMasterTY.Enabled = false;
                tbMasterTZ.Enabled = false;
            }
        }

        private void tbInfo_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sFilePath = m__G.m_RootDirectory + "\\Data\\";
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "dat";
            openFile.Multiselect = true;
            openFile.InitialDirectory = sFilePath;

            openFile.Filter = "Dat(*.dat)|*.dat";
            TextBox[] lTBX = new TextBox[2];
            lTBX[0] = tbInfo;
            lTBX[1] = tbVsnLog;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if (openFile.FileNames.Length == 0)
                    return;

                tbVsnLog.Text = "";
                string[] sFileName = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                {
                    sFileName[i] = openFile.FileNames[i];
                    if (!File.Exists(sFileName[i]))
                        return;
                    string lstr = MyOwner.ReadResultPos(sFileName[i]);
                    if (sFileName[i].Contains("_1_"))
                        lTBX[1].Text += lstr + "\r\n";
                    else
                        lTBX[0].Text += lstr + "\r\n";
                }
            }
        }
        public void GrabInitalMark()
        {
            if (!bHaltLive)
                GrabHalt();

            LoadScaleNTheta();
            LoadTXTYZeroOffset();
            // 241206 YLUT 적용안함.
            //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[1];

            m__G.fGraph.Drive_LEDs(mLEDcurrent[0], mLEDcurrent[1]);
            Thread.Sleep(50);

            Mat lCropImg = m__G.oCam[0].GrabLoadCropImg(0, false);
            CropView.Image = BitmapConverter.ToBitmap(lCropImg);    //  Grab & Crop
            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            lCropImg.SaveImage(fileName);     //  Crop & Save

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();
            SetDefaultMarkConfig(true);
            //DrawMarkPositions();

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].PrepareFineCOG();
            m__G.oCam[0].mFAL.BackupFMI();
            m__G.oCam[0].ForceTriggerTime();

            int findex = 0;
            System.Drawing.Point[] markPos = new System.Drawing.Point[6] {
                new System.Drawing.Point( 730, 78 ),
                new System.Drawing.Point( 234, 93 ),
                new System.Drawing.Point( 730, 255 ),
                new System.Drawing.Point( 234, 275 ),
                new System.Drawing.Point( 439, 294 ),
                new System.Drawing.Point( 532, 294 ) };

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].PointTo6DMotion(-1, mStdMarkPos);  //  초기 세팅한 절대좌표 기준으로 좌표값이 추출되도록 한다.
            string strtmp = "";
            //for (int i = 0;i< markPos.Length; i++)
            //{
            //    if (markPos[i] == null)
            //        continue;
            //    if (markPos[i].X == 0)
            //        continue;

            //    strtmp += markPos[i].X.ToString("F2") + "\t" + markPos[i].Y.ToString("F2") + "\t";
            //}
            //tbVsnLog.Text = strtmp + "\r\n";
            //MessageBox.Show("AAAA");
            double minscale = (180 / Math.PI * 60);                           //  rad to min
            double umscale = (5.5 / Global.LensMag);                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            strtmp = "";
            m__G.mbSuddenStop[0] = false;

            // Constrast 50보다 작을때 MsgBox
            bool isLowContrast = false;

            for (int ci = 0; ci < numFMIcandidate; ci++)
            {
                //////////////////////////////////////////////////////////////
                /////   모델 2개 추적하기위한 모델 변경 관련 코드
                //////////////////////////////////////////////////////////////
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if (ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;
                double sx = 0;
                double sy = 0;
                double sz = 0;
                double tx = 0;
                double ty = 0;
                double tz = 0;

                double[] lPrismTXTYTZ = new double[3];
                double[] lProbePrismTXTYTZ = new double[3];
                double[] lErrorPrismTXTYTZ = new double[3];

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].mFAL.mbGetHistogram = true;
                NthMeasure(0);
                FindIDmark(lCropImg);
                SetMasterZeroIndex(mMarkID);
                //txtMsaterNum.Text = mMarkID.ToString();
                if (txtMsaterNum.InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        txtMsaterNum.Text = mMarkID.ToString();
                    });
                else
                {
                    txtMsaterNum.Text = mMarkID.ToString();
                }


                m__G.oCam[0].mFAL.mbGetHistogram = false;

                sx = m__G.oCam[0].mC_pX[0] * umscale;
                sy = m__G.oCam[0].mC_pY[0] * umscale;
                sz = m__G.oCam[0].mC_pZ[0] * umscale;
                tx = m__G.oCam[0].mC_pTX[0] * minscale;
                ty = m__G.oCam[0].mC_pTY[0] * minscale;
                tz = m__G.oCam[0].mC_pTZ[0] * minscale;

                strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t\tContrast\t";




                for (int i = 0; i < 5; i++)
                {
                    var constrast = m__G.oCam[0].mFAL.mEffectiveContrast[i];

                    if (constrast < 50)
                    {
                        isLowContrast = true;
                    }

                    strtmp += constrast.ToString() + "\t";

                }

                strtmp += "( > 20 )";

                if (m__G.m_bPrismCS)
                {
                    m__G.oCam[0].mFAL.mFZM.SetPrismZeroTXTZ(0, 0, 0);  // init에서는 ConvertTXTYTZofCSHtoPrism를 하기전 PrismZeroTXTZ를 0으로 초기화
                    lPrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(tx, ty, tz, true);
                    m__G.oCam[0].mFAL.mFZM.SetPrismZeroTXTZ(lPrismTXTYTZ[0], lPrismTXTYTZ[1], lPrismTXTYTZ[2]);

                    strtmp += "\tPCS\t" + lPrismTXTYTZ[0].ToString("F5") + "\t" + lPrismTXTYTZ[1].ToString("F5") + "\t" + lPrismTXTYTZ[2].ToString("F5");
                }
            }
            DrawMarkDetected(true);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += strtmp + "\r\n";
                });
            }
            else
            {
                tbInfo.Text += strtmp + "\r\n";
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

            bHaltLive = true;
            IsLiveCropStop = true;

            if (isLowContrast)
            {
                //MessageBox.Show($"Contrast below 50 detected. Please check the Recipe.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public int mMarkID = 0;
        public void FindIDmark(Mat cropImg)
        {
            Rect IDmarkRegion = new Rect();
            IDmarkRegion.X = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].X * m__G.oCam[0].mFAL.mModelScale + 38);
            IDmarkRegion.Y = (int)(m__G.oCam[0].mDetectedMarkPos[0][1].Y * m__G.oCam[0].mFAL.mModelScale + 3);
            IDmarkRegion.Width = (int)(m__G.oCam[0].mDetectedMarkPos[0][0].X - m__G.oCam[0].mDetectedMarkPos[0][1].X - 9) * m__G.oCam[0].mFAL.mModelScale;
            IDmarkRegion.Height = 36;// (int)(180 - (m__G.oCam[0].mDetectedMarkPos[0][0].Y + m__G.oCam[0].mDetectedMarkPos[0][1].Y) * m__G.oCam[0].mFAL.mModelScale / 2);
            if (IDmarkRegion.X < 1 || IDmarkRegion.Y < 1 || IDmarkRegion.Width < 1 || IDmarkRegion.Height < 1)
                mMarkID = 0;
            else
            {
                Mat IDmarkImg = cropImg.SubMat(IDmarkRegion);
                //Cv2.ImShow("A", IDmarkImg);
                mMarkID = m__G.oCam[0].mFAL.FindMarkID(IDmarkImg);
            }
        }

        public Point2d[] mMarkShift = new Point2d[3];
        // public Point2d[] mMarkShift = new Point2d[3]; 
        public byte[] MakeMarkShift()
        {
            byte[] dataBuf = new byte[8 * 6];
            int curCount = 0;
            byte[] data;
            data = BitConverter.GetBytes(mMarkShift[0].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[0].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[1].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[1].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[2].X);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            curCount += data.Length;

            data = BitConverter.GetBytes(mMarkShift[2].Y);
            Array.Copy(data, 0, dataBuf, curCount, data.Length);
            //curCount += data.Length;

            return dataBuf;
        }
        public void FindCarrierToDummyShift()
        {
            CameraReset(2, true);
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(50);

            ManualFindMarks(0, false, false);   //  Fiducial Mark 찾기

            m__G.fVision.SetExposure(0, 700);

            Thread.Sleep(10);
            Point2d lCarrierRefPt = FineCarrierRef();   //   기준점 찾기
            //Point2d lCarrierRefPt = new Point2d();

            m__G.fVision.SetOrgExposure(0); //  노출시간 원상복귀

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    m__G.fVision.DrawMarkDetectedWithDummyShift((int)lCarrierRefPt.X, (int)lCarrierRefPt.Y);
                });
            }
            else
            {
                m__G.fVision.DrawMarkDetectedWithDummyShift((int)lCarrierRefPt.X, (int)lCarrierRefPt.Y);
            }
            Thread.Sleep(10);

            mMarkShift[0].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][8].X + m__G.oCam[0].mAzimuthPts[1][8].X) / 2 - lCarrierRefPt.X);
            mMarkShift[0].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][8].Y + m__G.oCam[0].mAzimuthPts[1][8].Y) / 2 - lCarrierRefPt.Y);
            mMarkShift[1].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][10].X + m__G.oCam[0].mAzimuthPts[1][10].X) / 2 - lCarrierRefPt.X + 260);
            mMarkShift[1].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][10].Y + m__G.oCam[0].mAzimuthPts[1][10].Y) / 2 - lCarrierRefPt.Y);
            //mMarkShift[2].X = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleX[1] * ((m__G.oCam[0].mAzimuthPts[1][6].X + m__G.oCam[0].mAzimuthPts[1][6].X) / 2 - lCarrierRefPt.X + 130);
            //mMarkShift[2].Y = -18.3333 * m__G.oCam[0].mFAL.mFZM.mScaleY[1] * ((m__G.oCam[0].mAzimuthPts[1][6].Y + m__G.oCam[0].mAzimuthPts[1][6].Y) / 2 - lCarrierRefPt.Y);
            string lstr = "CarrierShift(um) >\t" + mMarkShift[0].X.ToString("F1") + "\t " + mMarkShift[0].Y.ToString("F1") + "\t "
                        + mMarkShift[1].X.ToString("F1") + "\t " + mMarkShift[1].Y.ToString("F1") + "\t "
                        + mMarkShift[2].X.ToString("F1") + "\t " + mMarkShift[2].Y.ToString("F1") + "\tfrom (" + lCarrierRefPt.X.ToString("F3") + "," + lCarrierRefPt.Y.ToString("F3") + ")\r\n";
            tbInfo.Text += lstr;

            CameraReset(2, false);
        }
        public int FineCarrierCount = 0;
        public Point2d FineCarrierRef()
        {
            // 예전 기준점 찾기
            ////int TopNX = (int)m__G.oCam[0].mAzimuthPts[1][8].X;
            ////int TopNY = (int)m__G.oCam[0].mAzimuthPts[1][8].Y;
            ////Rect refRoiRegion = new Rect();
            ////refRoiRegion.X = TopNX + 51;
            ////refRoiRegion.Y = TopNY + 6;
            ////refRoiRegion.Width = 44;
            ////refRoiRegion.Height = 44;

            Point2d res = new Point2d();
            double[] cx = new double[2];
            double[] cy = new double[2];
            bool cornerFound = false;
            bool cornerFound2 = false;

            Rect refRoiRegion = new Rect();
            refRoiRegion.X = 665;
            refRoiRegion.Y = 240;
            refRoiRegion.Width = 60;
            refRoiRegion.Height = 90;

            m__G.oCam[0].GrabB(1, true);
            m__G.oCam[0].GrabB(2, true);
            //   영상 2개 확보해서 Avg 취한다.

            Mat lCropImg1 = m__G.oCam[0].LoadCropImgWide(1);
            Mat lCropImg2 = m__G.oCam[0].LoadCropImgWide(2);

            Mat refRoiImg1 = lCropImg1.SubMat(refRoiRegion);
            Mat refRoiImg2 = lCropImg2.SubMat(refRoiRegion);

            string fPath = string.Format("D:\\TestImg_lCropImg1_{0}.bmp", FineCarrierCount);
            lCropImg1.SaveImage(fPath);
            FineCarrierCount++;
            //lCropImg2.SaveImage("D:\\TestImg_lCropImg2.bmp");
            //Cv2.ImShow("A", lCropImg1);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("A");

            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////
            ///
            ////   다음은 저장된 시험용 영상을 이용하는 경우
            ///
            //string sFilePath = Path.GetFullPath("C:\\CSHTest\\Result\\RawData");
            //if (!Directory.Exists(sFilePath))
            //    Directory.CreateDirectory(sFilePath);

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            //openFileDialog1.Multiselect = true;
            //openFileDialog1.InitialDirectory = sFilePath;
            //openFileDialog1.FilterIndex = 2;
            //if (openFileDialog1.ShowDialog() != DialogResult.OK)
            //    return new Point2d();

            //for ( int i=0; i< openFileDialog1.FileNames.Length; i++ )
            //{
            //    lCropImg1 = new Mat(openFileDialog1.FileNames[i], ImreadModes.Grayscale);
            //    lCropImg2 = new Mat();
            //    lCropImg1.CopyTo(lCropImg2);
            //    lCropImg1.CopyTo(m__G.oCam[0].mFAL.mSourceImg[0]);

            //    refRoiImg1 = lCropImg1.SubMat(refRoiRegion);
            //    refRoiImg2 = lCropImg2.SubMat(refRoiRegion);

            //    cornerFound = CalcTopRight(refRoiImg1, ref cx[0], ref cy[0]);

            //    if (cornerFound )
            //        res = new Point2d((refRoiRegion.X + cx[0]), (refRoiRegion.Y + cy[0]));

            //    string lstr = i.ToString() + "\t" + res.X.ToString("F3") + "\t" + res.Y.ToString("F3") + "\r\n";
            //    tbInfo.Text += lstr;
            //}

            ////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////
            //Cv2.ImShow("B", refRoiImg1);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("B");
            ////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////


            //  우측 경계 - 기울기 최대 X 좌표,   상측 경계 - 기울기 최대 Y 좌표
            cornerFound = CalcTopRight(refRoiImg1, ref cx[0], ref cy[0]);
            cornerFound2 = CalcTopRight(refRoiImg2, ref cx[1], ref cy[1]);

            if (cornerFound && cornerFound2)
                res = new Point2d((refRoiRegion.X + (cx[0] + cx[1]) / 2), (refRoiRegion.Y + (cy[0] + cy[1]) / 2));

            return res;
        }

        public bool CalcTopRight(Mat lSourceImg, ref double cx, ref double cy)
        {
            byte[] imgBuf = null;
            lSourceImg.GetArray(out imgBuf);
            //Cv2.ImShow("A", lSourceImg);
            //Cv2.WaitKey();
            //Cv2.DestroyWindow("A");

            int nSizeX = lSourceImg.Width;
            int nSizeY = lSourceImg.Height;
            int xmin = 0;
            int xmax = nSizeX - 1;
            int ymin = 0;
            int ymax = nSizeY - 1;
            int xmid = (xmin + xmax) / 2;
            int ymid = (ymin + ymax) / 2;
            int i = 0;
            int j = ymid;
            bool returnValue = true;
            try
            {
                //  사각형의 각 Edge 영역을 대략 찾은 뒤 
                //  각 영역의 중심점을 계산하여 영역을 보정하고, 
                //  보정한 영역에 대해서 중심점을 다시 계산하기를 반복한다.
                //  Top View 의 경우 X 중심점은 X영역 결정 및 Stroke 기준 역할을 하고, y 중심점은 Yaw 계산에 활용된다,.
                double sum = 0;
                double psum = 0;
                double pv = 0;


                //cx = xmid;
                //cy = ymid;
                //return;
                int maxdP = 0;
                int dP = 0;
                int ddP = 0;
                int roiR = 0;  //  Right
                int roiT = 0;  //  Top
                double[] mx = new double[2];
                double[] my = new double[2];
                double[] residueH = new double[2];  //  0 : Left,   1: Right
                double[] residueV = new double[2];  //  0 : Top,    1: Bottom
                //  Edge Search 영역 획득
                int Pleft = 0;
                int Pright = 0;
                int upperArea = 0;
                int lowerArea = 0;
                double middleArea = 0;

                //  Right
                maxdP = 0;
                int xmidR = xmax - 20;

                for (i = xmax - 2; i > xmidR; i--)
                {
                    dP = 0;
                    Pleft = 0;
                    Pright = 0;
                    for (j = ymin; j < ymax; j++)
                    {
                        Pleft += imgBuf[i - 2 + j * nSizeX] + imgBuf[i - 1 + j * nSizeX];
                        Pright += imgBuf[i + 2 + j * nSizeX] + imgBuf[i + 1 + j * nSizeX];
                    }

                    //dP = Pleft - Pright; //  왼쪽이 밝으므로 양수  -> 기존 기준
                    dP = Pright - Pleft; //  오른쪽이 밝으므로 양수  -> 수정 기준 250609

                    if (dP > maxdP)
                    {
                        maxdP = dP;
                        roiR = i - 1;
                    }
                }
                //  Top
                maxdP = 0;
                //int[] dpY = new int[ymid - ymin + 2];
                //string llstr = "";
                for (j = ymin + 4; j < ymin + 34; j++)
                {
                    dP = 0;
                    upperArea = 0;
                    lowerArea = 0;
                    middleArea = 0;
                    for (i = 0; i <= roiR; i++)
                    {
                        //middleArea += imgBuf[i + j * nSizeX];
                        upperArea += imgBuf[i + (j - 1) * nSizeX] + imgBuf[i + (j - 2) * nSizeX] + imgBuf[i + (j - 3) * nSizeX] + imgBuf[i + (j - 4) * nSizeX];
                        lowerArea += imgBuf[i + (j + 1) * nSizeX] + imgBuf[i + (j + 2) * nSizeX] + imgBuf[i + (j + 3) * nSizeX] + imgBuf[i + (j + 4) * nSizeX];
                        ddP = upperArea - lowerArea;
                        dP += ddP;
                    }
                    //upperArea = upperArea / roiR;
                    //lowerArea = lowerArea / roiR;
                    //middleArea = middleArea / roiR;

                    if (dP > maxdP) // 내측 밝고 외측 어두운 경계
                    {
                        maxdP = dP;
                        roiT = j;
                    }
                }

                ///////////////////////
                ///////////////////////
                //  Edge 영역별 COG 계산
                maxdP = 0;

                double weight = 1;
                for (int k = 0; k < 3; k++)
                {
                    //  Right
                    psum = 0;
                    sum = 0;
                    xmax = Math.Min(roiR + 5, nSizeX - 3);
                    for (j = roiT + 3; j <= roiT + 53; j++)
                    {
                        //if (mIsDebug && k == 4)
                        //    lstr = j.ToString() + ",";
                        for (i = roiR - 4; i <= xmax; i++)
                        {
                            if (i == (roiR - 4))
                                weight = 1 - residueH[1];
                            else if (i == (roiR + 4))
                                weight = residueH[1];
                            else
                                weight = 1;

                            //pv = imgBuf[i - 1 + j * nSizeX] - imgBuf[i + 1 + j * nSizeX];   //  왼쪽이 밝을 때
                            pv = imgBuf[i + 1 + j * nSizeX] - imgBuf[i - 1 + j * nSizeX] + imgBuf[i + 2 + j * nSizeX] - imgBuf[i - 2 + j * nSizeX];   //  오른쪽이 밝을 때

                            if (pv < 0)
                                pv = 0;

                            psum += weight * pv * i;
                            sum += weight * pv;
                            //if (mIsDebug && k == 4)
                            //    lstr += p_Value[iBuf][i + j * nSizeX].ToString() + ",";
                        }
                        //if (mIsDebug && k == 4)
                        //    wr.WriteLine(lstr);
                    }
                    //if (mIsDebug && k == 4)
                    //    wr.Close();
                    if (sum > 0)
                    {
                        mx[1] = psum / sum;
                        roiR = (int)(mx[1] + 0.5);
                        residueH[1] = mx[1] - roiR;
                    }
                    else
                        mx[1] = roiR;

                    //  Top
                    psum = 0;
                    sum = 0;
                    ymin = Math.Max(2, roiT - 7);

                    for (j = ymin; j <= roiT + 7; j++)
                    {
                        if (j >= nSizeY || j < 1)
                            continue;
                        //if (mIsDebug && k == 4)
                        //    lstr = j.ToString() + ",";

                        if (j == (roiT - 7))
                            weight = 1 - residueV[0];
                        else if (j == (roiT + 7))
                            weight = residueV[0];
                        else
                            weight = 1;

                        for (i = 0; i < roiR; i++)
                        {
                            if (i == nSizeX)
                                break;

                            pv = imgBuf[i + (j - 2) * nSizeX] + imgBuf[i + (j - 1) * nSizeX] - imgBuf[i + (j + 1) * nSizeX] - imgBuf[i + (j + 2) * nSizeX];
                            if (pv < 0) pv = 0;
                            psum += weight * pv * j;
                            sum += weight * pv;
                            //if (mIsDebug && k == 4)
                            //    lstr += p_Value[iBuf][i + j * nSizeX].ToString() + ",";
                        }
                        //if (mIsDebug && k == 4)
                        //    wr.WriteLine(lstr);
                    }
                    //if (mIsDebug && k == 4)
                    //    wr.Close();
                    if (sum > 0)
                    {
                        my[0] = psum / sum;
                        roiT = (int)(my[0] + 0.5);
                        residueV[0] = my[0] - roiT;
                    }
                    else
                        my[0] = roiT;

                    cx = mx[1];
                    cy = my[0];
                }
                //MessageBox.Show("m_nCam=" + m_nCam.ToString() + " X " + mx[0].ToString("F1") + "-" + mx[1].ToString("F1") + " Y " + my[0].ToString("F1") + "-" + my[1].ToString("F1") + "\r\nL R T B"
                //    + roiL.ToString() + "-" + roiR.ToString() + " " + roiT.ToString() + " " + roiB.ToString());
                //  cx cy 저장
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void Grab_Initial_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            GrabInitalMark();

            //FindCarrierToDummyShift();
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            int camCount = 2;


            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\CameraID.txt"))
            {
                MessageBox.Show("Camera ID not exists.");
                return;
            }
            try
            {
                BaslerCam[0].Close();
                Thread.Sleep(100);
                BaslerCam[0].Open();
            }
            catch
            {
                ;
            }
            ReadOrgROI(camCount);

            BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

            m__G.mCamCount = 1;

            m_FocusedLED = 0;


            for (int i = 0; i < m__G.mCamCount; i++)
                SetNewROIXY(i, v_OrgROIH_min[i], v_OrgROIH_min[i] + v_OrgROIH_width[i], v_OrgROIV_min[i], v_OrgROIV_min[i] + v_OrgROIV_height[i]);

            //ReadZeroGap(m__G.mCamCount);
            //ReadCalibrationTiltData();

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    m__G.oCam[0].SelectWindow(LiveView.Handle);
                });
            else
                m__G.oCam[0].SelectWindow(LiveView.Handle);

            m__G.oCam[0].DisplayZoom(ZoomFactor, ZoomFactor);

            BaslerCam[0].Parameters[PLCamera.ClTapGeometry].SetValue("Geometry1X10_1Y");
            BaslerCam[0].Parameters[PLCamera.ReverseX].SetValue(true);
            //BaslerCam[0].Parameters[PLCamera.ReverseY].SetValue(false);
            BaslerCam[0].Parameters[PLCamera.GainRaw].SetValue(v_OrgGain[0]);
            BaslerCam[0].Parameters[PLCamera.GammaEnable].SetValue(true);

            m__G.oCam[0].SetBlobAreaMinMax(m_BlobAreaMin, m_BlobAreaMax);
            string strScaleRotation = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNOpticalR.txt";
            double stop = 0;
            double sside = 0;
            double rtop = 0;
            double rside = 0;

            m__G.oCam[0].LoadScaleNOpticalRotation(strScaleRotation, ref stop, ref sside, ref rtop, ref rside);
            SetExposure(0, m__G.sRecipe.iExposure);
            SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            SetEdgeBand(m__G.sRecipe.iEdgeBand);

            cbContinuosMode.Enabled = true;

        }
        public void ReadSerialPort()
        {
            //BinaryReader br = new BinaryReader(fs);


            // Create a new SerialPort object with default settings.
            SerialPort _serialPort = new SerialPort();
            _serialPort.PortName = "COM1";
            _serialPort.BaudRate = 19200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            List<byte> allBytes = null;

            string cmdReadPosition = "GA01\r\n";
            char[] data = cmdReadPosition.ToCharArray();
            _serialPort.Write(data, 0, data.Length);
            Thread.Sleep(1);

            _serialPort.BaseStream.Flush();
            _serialPort.DiscardInBuffer();
            int c = 1;
            while (c != 0)
            {
                c = _serialPort.ReadByte();
                allBytes.Add((byte)c);
            }
            string resStr = allBytes.ToString();

            _serialPort.Close();
        }

        public int[] ExtractStablizedIndex(double[][] measure, int focusedAxis)
        {
            List<int> resIndex = new List<int>();
            int numLine = measure.Length;
            int i = 2;
            double preDelta = 0;
            double oldDelta = 0;
            double postDelta = 0;
            bool settled = false;
            while (i < numLine - 1)
            {
                //  측정이 제대로 안된 점이 있는 경우 Skip
                bool bValidLine = true;
                for (int di = 6; di < 16; di++)
                {
                    if (measure[i][di] == 0)
                    {
                        bValidLine = false;
                        break;
                    }
                }
                if (!bValidLine)
                {
                    i += 2;
                    continue;
                }

                oldDelta = Math.Abs(measure[i - 2][focusedAxis] - measure[i][focusedAxis]);
                preDelta = Math.Abs(measure[i - 1][focusedAxis] - measure[i][focusedAxis]);
                postDelta = Math.Abs(measure[i][focusedAxis] - measure[i + 1][focusedAxis]);
                if (oldDelta < 0.4 && preDelta < 0.4)
                {
                    if (postDelta > 0.5)
                    {
                        resIndex.Add(i);
                        i += 2;
                        settled = false;
                        continue;
                    }
                    else if (postDelta < 0.4)
                    {
                        i++;
                        settled = true;
                        continue;
                    }
                    else if (postDelta > 0.4)
                    {
                        if (Math.Abs(measure[i - 1][focusedAxis] - measure[i + 1][focusedAxis]) > 0.4)
                        {
                            resIndex.Add(i);
                            i += 2;
                            settled = false;
                            continue;
                        }
                        else
                        {
                            i++;
                            settled = true;
                            continue;
                        }
                    }
                }
                else
                    settled = false;

                i++;
            }
            if (settled == true)
                resIndex.Add(numLine - 1);

            return resIndex.ToArray();
        }

        double mZCalAvgY1Y2pp = 0;
        double mZCalY3pp = 0;
        double mYCalAvgY1Y2pp = 0;
        double mYCalY3pp = 0;
        double mEstimatedEastViewYscale = 0;

        //public void JH_SK_CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        //{
        //    if (m__G.oCam[0].mFAL.mFZM == null)
        //    {
        //        MessageBox.Show("mFZM not loaded.");
        //        return;
        //    }

        //    string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
        //    string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
        //    if (!Directory.Exists(AdminPathName))
        //        Directory.CreateDirectory(AdminPathName);

        //    int fullLength = measure.Length;
        //    StreamWriter wr = null;
        //    //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

        //    //  안정화 유효데이터를 추출한다.
        //    //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

        //    List<double[]> stablizedData = new List<double[]>();

        //    int effLength = 0;
        //    //double a = 0;
        //    //double b = 0;
        //    int[] effIndex = null;
        //    FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

        //    double[] XtoXab = new double[3];
        //    double[] YtoYab = new double[3];
        //    double[] ZtoZab = new double[3];
        //    double[] TXtoTXab = new double[3];
        //    double[] TYtoTYab = new double[3];
        //    double[] TZtoTZab = new double[3];

        //    double[] XtoYab = new double[3];
        //    double[] XtoZab = new double[3];
        //    double[] XtoTXab = new double[2];
        //    double[] XtoTYab = new double[2];
        //    double[] XtoTZab = new double[2];

        //    double[] YtoXab = new double[3];
        //    double[] YtoZab = new double[3];
        //    double[] YtoTXab = new double[2];
        //    double[] YtoTYab = new double[2];
        //    double[] YtoTZab = new double[2];

        //    double[] ZtoXab = new double[3];
        //    double[] ZtoYab = new double[3];
        //    double[] ZtoTXab = new double[3];
        //    double[] ZtoTYab = new double[3];
        //    double[] ZtoTZab = new double[3];

        //    double[] TXtoTYab = new double[3];
        //    double[] TXtoTZab = new double[3];

        //    double[] TYtoTXab = new double[3];
        //    double[] TYtoTZab = new double[3];

        //    double[] TZtoTXab = new double[3];
        //    double[] TZtoTYab = new double[3];

        //    if (!IsRemote)
        //    {
        //        switch (axis)
        //        {
        //            case "Z":
        //                effIndex = ExtractStablizedIndex(measure, 2);
        //                break;

        //            case "X":
        //                effIndex = ExtractStablizedIndex(measure, 0);
        //                break;
        //            case "Y":
        //                effIndex = ExtractStablizedIndex(measure, 1);
        //                break;
        //            case "TX":
        //                effIndex = ExtractStablizedIndex(measure, 3);
        //                break;
        //            case "TY":
        //                effIndex = ExtractStablizedIndex(measure, 4);
        //                break;
        //            default:
        //                break;
        //        }
        //        effLength = effIndex.Length;

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sZZ[i] = new FZMath.Point2D();
        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();
        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();
        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        if (effLength == 0)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    tbInfo.Text += "Stabilized data not found\r\n";
        //                    tbInfo.SelectionStart = tbInfo.Text.Length;
        //                    tbInfo.ScrollToCaret();
        //                });
        //            }
        //            else
        //            {
        //                tbInfo.Text += "Stabilized data not found\r\n";
        //                tbInfo.SelectionStart = tbInfo.Text.Length;
        //                tbInfo.ScrollToCaret();
        //            }
        //            return;

        //        }
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            double[] lstbData = new double[22];
        //            for (int j = 0; j < 22; j++)
        //                lstbData[j] = measure[effIndex[i]][j];

        //            stablizedData.Add(lstbData);
        //        }
        //    }
        //    else
        //    {
        //        //  Remote or Auto Calibration
        //        effLength = measure.Length;

        //        szy1 = new FZMath.Point2D[effLength];
        //        szy2 = new FZMath.Point2D[effLength];
        //        szy3 = new FZMath.Point2D[effLength];

        //        sZZ = new FZMath.Point2D[effLength];
        //        sXX = new FZMath.Point2D[effLength];
        //        sYY = new FZMath.Point2D[effLength];
        //        sTXTX = new FZMath.Point2D[effLength];
        //        sTYTY = new FZMath.Point2D[effLength];
        //        sTZTZ = new FZMath.Point2D[effLength];

        //        sXtoY = new FZMath.Point2D[effLength];
        //        sXtoZ = new FZMath.Point2D[effLength];
        //        sXtoTX = new FZMath.Point2D[effLength];
        //        sXtoTY = new FZMath.Point2D[effLength];
        //        sXtoTZ = new FZMath.Point2D[effLength];

        //        sYtoX = new FZMath.Point2D[effLength];
        //        sYtoZ = new FZMath.Point2D[effLength];
        //        sYtoTX = new FZMath.Point2D[effLength];
        //        sYtoTY = new FZMath.Point2D[effLength];
        //        sYtoTZ = new FZMath.Point2D[effLength];

        //        sZtoX = new FZMath.Point2D[effLength];
        //        sZtoY = new FZMath.Point2D[effLength];
        //        sZtoTX = new FZMath.Point2D[effLength];
        //        sZtoTY = new FZMath.Point2D[effLength];
        //        sZtoTZ = new FZMath.Point2D[effLength];

        //        sTXtoTY = new FZMath.Point2D[effLength];
        //        sTXtoTZ = new FZMath.Point2D[effLength];

        //        sTYtoTX = new FZMath.Point2D[effLength];
        //        sTYtoTZ = new FZMath.Point2D[effLength];

        //        sTZtoTX = new FZMath.Point2D[effLength];
        //        sTZtoTY = new FZMath.Point2D[effLength];

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sZZ[i] = new FZMath.Point2D();
        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoY[i] = new FZMath.Point2D();
        //            sXtoZ[i] = new FZMath.Point2D();
        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();

        //            sYtoX[i] = new FZMath.Point2D();
        //            sYtoZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();

        //            sZtoX[i] = new FZMath.Point2D();
        //            sZtoY[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();

        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();

        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        for (int i = 0; i < effLength; i++)
        //            stablizedData.Add(measure[i]);
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////
        //    //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
        //    StreamWriter lwr = null;
        //    double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

        //    if (!IsRemote)
        //    {
        //        lwr = new StreamWriter(AdminPathName + "FullData.csv");
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        int k = 0;
        //        for (int i = 0; i < fullLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //                slstr += measure[i][j].ToString("F5") + ",";
        //            if (i == effIndex[k])
        //            {
        //                slstr += "*";
        //                k++;
        //            }
        //            lwr.WriteLine(slstr);
        //            if (k == effLength)
        //                break;
        //        }
        //        lwr.Close();
        //    }

        //    string calName = axis;
        //    if (isAutoCalibrationEastView)
        //    {
        //        calName += "_EastView";
        //    }

        //    string strStabilizedFile = "";
        //    if (mAutoCalibrationCount % 2 == 0)
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
        //    else
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

        //    try
        //    {
        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    catch (Exception e)
        //    {
        //        Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
        //        if (mAutoCalibrationCount % 2 == 0)
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
        //        else
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    if (lwr != null)
        //    {
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //            {
        //                //if (j < 19 || j == 22)
        //                slstr += stablizedData[i][j].ToString("F5") + ",";
        //                //else
        //                //{
        //                //    //slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
        //                //    slstr += (stablizedData[i][j]).ToString("F5") + ",";
        //                //}
        //            }

        //            lwr.WriteLine(slstr);
        //        }
        //        lwr.Close();
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////

        //    //  axis 따라서 List 에 저장된 데이터를 처리한다.
        //    double[] p2ndCoef = new double[3];
        //    double[] p2ndCoef2 = new double[3];
        //    double[] p2ndCoef3 = new double[3];
        //    int fovYoffset = GetROIY(0);
        //    string lstr = "";
        //    switch (axis)
        //    {
        //        case "Z":
        //            //  axis == "Z" : YLUT 의 경우 
        //            //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
        //            //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
        //            //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

        //            //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

        //            double a1 = 0;
        //            double b1 = 0;
        //            double a2 = 0;
        //            double b2 = 0;
        //            double a3 = 0;
        //            double b3 = 0;

        //            mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

        //            //  stablizedData[i][16] : X
        //            //  stablizedData[i][17] : Y
        //            //  stablizedData[i][18] : Z
        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
        //            double[] LUT1_tmp = new double[effLength];
        //            double[] LUT1 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

        //            LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
        //            LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

        //            //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
        //            //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
        //            //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
        //            double[] LUT2_tmp = new double[effLength];
        //            double[] LUT2 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

        //            LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
        //            LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

        //            //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
        //            //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
        //            //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
        //            //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
        //                szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
        //            double[] LUT3_tmp = new double[effLength];
        //            double[] LUT3 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

        //            LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
        //            LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

        //            //  Z scale
        //            // 241206 YLUT 제거 후, Z scale 2차로 변경
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
        //                sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szz, effLength, ref a, ref b);
        //            //a = a * 0.9993; // 0.9992; // 헥사포드 Cal 변경
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);

        //            //a = (a - 1) * 0.4 + 1; 
        //            //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
        //            //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
        //            //  Scale 만 조정해가면서 수차례 반복 필요
        //            //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


        //            if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
        //            {
        //                string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
        //                string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
        //                wr = new StreamWriter(srcFile);
        //                wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
        //                wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
        //                for (int i = 0; i < effLength; i++)
        //                {
        //                    wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
        //                }
        //                wr.Close();
        //                System.IO.File.Copy(srcFile, destFile, true);
        //            }

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  Z to X 계산
        //            //  Z vs X - Xprobe , Z vs Y - Yprobe                 
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
        //                //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
        //                sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
        //                sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage

        //                sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref ZtoXab[0], ref ZtoXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref ZtoYab[0], ref ZtoYab[1]);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTX, effLength, ref ZtoTXab[0], ref ZtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTY, effLength, ref ZtoTYab[0], ref ZtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoTZ, effLength, ref ZtoTZab[0], ref ZtoTZab[1]);

        //            if (!isAutoCalibrationEastView)
        //            {
        //                // ZtoX, ZtoY 수정
        //                //double aZtoX = 0;
        //                //double aZtoY = 0;
        //                //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref aZtoX, ref b);
        //                //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref aZtoY, ref b);
        //                lstr = $"ZZ Scale\t{ZtoZab[0]:E5}\t{ZtoZab[1]:E5}\t{ZtoZab[2]:E5}\r\n";
        //                lstr += $"ZtoX\t{ZtoXab[0]:E5}\r\n";
        //                lstr += $"ZtoY\t{ZtoYab[0]:E5}\r\n";


        //                if (mAutoCalibrationCount % 2 == 0)
        //                {
        //                    string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                    StreamReader sr = new StreamReader(scaleNthetaFile);
        //                    string allstr = sr.ReadToEnd();
        //                    sr.Close();
        //                    string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    string[] strZscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[2] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZscaleLine.Length; i++)
        //                        allLines[2] += strZscaleLine[i];

        //                    string[] strZtoXLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[6] = ZtoXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoXLine.Length; i++)
        //                        allLines[6] += strZtoXLine[i];

        //                    string[] strZtoYLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[7] = ZtoYab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoYLine.Length; i++)
        //                        allLines[7] += strZtoYLine[i];

        //                    wr = new StreamWriter(scaleNthetaFile);
        //                    for (int i = 0; i < allLines.Length; i++)
        //                    {
        //                        wr.WriteLine(allLines[i]);
        //                    }
        //                    wr.Close();
        //                }
        //            }


        //            //////////////////////////////////////////////////////////////////////////////////

        //            break;
        //        case "X":
        //            //  Axis = 1 : X scale 확인 및 저장
        //            //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
        //            //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
        //            //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
        //                sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref a, ref b);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]

        //            lstr = "XX Scale,\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][18]);
        //                //sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);
        //                sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
        //                sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

        //                sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoY, effLength, ref XtoYab[0], ref XtoYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTX, effLength, ref XtoTXab[0], ref XtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTY, effLength, ref XtoTYab[0], ref XtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTZ, effLength, ref XtoTZab[0], ref XtoTZab[1]);

        //            lstr += "XtoY,\t" + XtoYab[0].ToString("E5") + "\r\n";
        //            lstr += "XtoZ,\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTX,\t" + XtoTXab[0].ToString("E5") + "\r\n";

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe
        //            //////FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //            //////for (int i = 0; i < effLength; i++)
        //            //////{
        //            //////    sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3]);
        //            //////}

        //            wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
        //            //for (int i = 0; i < effLength; i++)
        //            //    lstr += sXtoZ[i].X.ToString("F4") + "," + sXtoZ[i].Y.ToString("F4") + "\r\n";
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strXXscaleLine = allLines[0].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[0] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXXscaleLine.Length; i++)
        //                    allLines[0] += strXXscaleLine[i];

        //                string[] strXtoYLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[10] = XtoYab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoYLine.Length; i++)
        //                    allLines[10] += strXtoYLine[i];

        //                string[] strXtoZLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[11] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoZLine.Length; i++)
        //                    allLines[11] += strXtoZLine[i];

        //                string[] strXtoTXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[13] = XtoTXab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTXLine.Length; i++)
        //                    allLines[13] += strXtoTXLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        case "Y":
        //            mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
        //            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

        //            //  Axis = 2 : Y scale 확인 및 저장
        //            //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
        //                sYY[i].X = stablizedData[i][1];
        //            }
        //            //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref a, ref b);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);

        //            lstr = "YY Scale,\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  X to Y ／Ｚ　계산
        //            //  X vs Y - Yprobe , X vs Z - Zprobe

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][17]);    //  X - probe X
        //                //sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);   //  Z - probe Z
        //                sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
        //                sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

        //                sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoX, effLength, ref YtoXab[0], ref YtoXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTX, effLength, ref YtoTXab[0], ref YtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTY, effLength, ref YtoTYab[0], ref YtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoTZ, effLength, ref YtoTZab[0], ref YtoTZab[1]);


        //            lstr += "YtoX,\t" + YtoXab[0].ToString("E5") + "\r\n";
        //            lstr += "YtoZ,\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTX,\t" + YtoTXab[0].ToString("E5") + "\r\n";

        //            if (isAutoCalibrationEastView)
        //            {
        //                lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
        //            }

        //            wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////                    
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        //                sr.Close();
        //                if (isAutoCalibrationEastView)
        //                {
        //                    string[] strEastScaleLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[12] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
        //                    for (int i = 1; i < strEastScaleLine.Length; i++)
        //                        allLines[12] += strEastScaleLine[i];
        //                }
        //                else
        //                {
        //                    string[] strYYscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[1] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYYscaleLine.Length; i++)
        //                        allLines[1] += strYYscaleLine[i];

        //                    string[] strYtoXLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[8] = YtoXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoXLine.Length; i++)
        //                        allLines[8] += strYtoXLine[i];

        //                    string[] strYtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[9] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoZLine.Length; i++)
        //                        allLines[9] += strYtoZLine[i];

        //                    string[] strYtoTXLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[14] = YtoTXab[0].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTXLine.Length; i++)
        //                        allLines[14] += strYtoTXLine[i];
        //                }

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
        //        case "TX":
        //            //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
        //            //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTXTX[i].Y = stablizedData[i][19]; // RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTXTX[i].X = stablizedData[i][3];

        //                sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTY, effLength, ref TXtoTYab[0], ref TXtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTXtoTZ, effLength, ref TXtoTZab[0], ref TXtoTZab[1]);


        //            lstr += "TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
        //            wr.WriteLine("TX Scale,\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n");
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXscaleLine.Length; i++)
        //                    allLines[4] += strTXscaleLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TY":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTYTY[i].Y = stablizedData[i][20];// * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTYTY[i].X = stablizedData[i][4];

        //                sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYTY, effLength, ref TYtoTYab[0], ref TYtoTYab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTX, effLength, ref TYtoTXab[0], ref TYtoTXab[1]);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYtoTZ, effLength, ref TYtoTZab[0], ref TYtoTZab[1]);

        //            lstr += "TY Scale,\t" + TYtoTYab[0].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.WriteLine("TY Scale," + TYtoTYab[0].ToString());
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[5] = TYtoTYab[0].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYscaleLine.Length; i++)
        //                    allLines[5] += strTYscaleLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            tbCalibration.Text += lstr;
        //        });
        //    else
        //        tbCalibration.Text += lstr;

        //}

        //public void CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        //{
        //    if (m__G.oCam[0].mFAL.mFZM == null)
        //    {
        //        MessageBox.Show("mFZM not loaded.");
        //        return;
        //    }

        //    string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
        //    string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
        //    if (!Directory.Exists(AdminPathName))
        //        Directory.CreateDirectory(AdminPathName);

        //    int fullLength = measure.Length;
        //    StreamWriter wr = null;
        //    //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

        //    //  안정화 유효데이터를 추출한다.
        //    //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

        //    List<double[]> stablizedData = new List<double[]>();

        //    int effLength = 0;
        //    int[] effIndex = null;
        //    FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sXtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sYtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

        //    FZMath.Point2D[] sTXtoTY = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTXtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTYtoTZ = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTX = new FZMath.Point2D[effLength];
        //    FZMath.Point2D[] sTZtoTY = new FZMath.Point2D[effLength];

        //    double[] XtoXab = new double[3];
        //    double[] YtoYab = new double[3];
        //    double[] ZtoZab = new double[3];
        //    double[] TXtoTXab = new double[3];
        //    double[] TYtoTYab = new double[3];
        //    double[] TZtoTZab = new double[3];

        //    double[] XtoYab = new double[3];
        //    double[] XtoZab = new double[3];
        //    double[] XtoTXab = new double[3];
        //    double[] XtoTYab = new double[3];
        //    double[] XtoTZab = new double[3];

        //    double[] YtoXab = new double[3];
        //    double[] YtoZab = new double[3];
        //    double[] YtoTXab = new double[3];
        //    double[] YtoTYab = new double[3];
        //    double[] YtoTZab = new double[3];

        //    double[] ZtoXab = new double[3];
        //    double[] ZtoYab = new double[3];
        //    double[] ZtoTXab = new double[3];
        //    double[] ZtoTYab = new double[3];
        //    double[] ZtoTZab = new double[3];

        //    double[] TXtoTYab = new double[3];
        //    double[] TXtoTZab = new double[3];

        //    double[] TYtoTXab = new double[3];
        //    double[] TYtoTZab = new double[3];

        //    double[] TZtoTXab = new double[3];
        //    double[] TZtoTYab = new double[3];

        //    if (!IsRemote)
        //    {
        //        switch (axis)
        //        {
        //            case "Z":
        //                effIndex = ExtractStablizedIndex(measure, 2);
        //                break;

        //            case "X":
        //                effIndex = ExtractStablizedIndex(measure, 0);
        //                break;
        //            case "Y":
        //                effIndex = ExtractStablizedIndex(measure, 1);
        //                break;
        //            case "TX":
        //                effIndex = ExtractStablizedIndex(measure, 3);
        //                break;
        //            case "TY":
        //                effIndex = ExtractStablizedIndex(measure, 4);
        //                break;
        //            case "TZ":
        //                effIndex = ExtractStablizedIndex(measure, 5);
        //                break;
        //            default:
        //                break;
        //        }
        //        effLength = effIndex.Length;

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sZZ[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();
        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();
        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        if (effLength == 0)
        //        {
        //            if (InvokeRequired)
        //            {
        //                BeginInvoke((MethodInvoker)delegate
        //                {
        //                    tbInfo.Text += "Stabilized data not found\r\n";
        //                    tbInfo.SelectionStart = tbInfo.Text.Length;
        //                    tbInfo.ScrollToCaret();
        //                });
        //            }
        //            else
        //            {
        //                tbInfo.Text += "Stabilized data not found\r\n";
        //                tbInfo.SelectionStart = tbInfo.Text.Length;
        //                tbInfo.ScrollToCaret();
        //            }
        //            return;

        //        }
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            double[] lstbData = new double[22];
        //            for (int j = 0; j < 22; j++)
        //                lstbData[j] = measure[effIndex[i]][j];

        //            stablizedData.Add(lstbData);
        //        }
        //    }
        //    else
        //    {
        //        //  Remote or Auto Calibration
        //        effLength = measure.Length;

        //        szy1 = new FZMath.Point2D[effLength];
        //        szy2 = new FZMath.Point2D[effLength];
        //        szy3 = new FZMath.Point2D[effLength];

        //        sXX = new FZMath.Point2D[effLength];
        //        sYY = new FZMath.Point2D[effLength];
        //        sZZ = new FZMath.Point2D[effLength];
        //        sTXTX = new FZMath.Point2D[effLength];
        //        sTYTY = new FZMath.Point2D[effLength];
        //        sTZTZ = new FZMath.Point2D[effLength];

        //        sXtoY = new FZMath.Point2D[effLength];
        //        sXtoZ = new FZMath.Point2D[effLength];
        //        sXtoTX = new FZMath.Point2D[effLength];
        //        sXtoTY = new FZMath.Point2D[effLength];
        //        sXtoTZ = new FZMath.Point2D[effLength];

        //        sYtoX = new FZMath.Point2D[effLength];
        //        sYtoZ = new FZMath.Point2D[effLength];
        //        sYtoTX = new FZMath.Point2D[effLength];
        //        sYtoTY = new FZMath.Point2D[effLength];
        //        sYtoTZ = new FZMath.Point2D[effLength];

        //        sZtoX = new FZMath.Point2D[effLength];
        //        sZtoY = new FZMath.Point2D[effLength];
        //        sZtoTX = new FZMath.Point2D[effLength];
        //        sZtoTY = new FZMath.Point2D[effLength];
        //        sZtoTZ = new FZMath.Point2D[effLength];

        //        sTXtoTY = new FZMath.Point2D[effLength];
        //        sTXtoTZ = new FZMath.Point2D[effLength];

        //        sTYtoTX = new FZMath.Point2D[effLength];
        //        sTYtoTZ = new FZMath.Point2D[effLength];

        //        sTZtoTX = new FZMath.Point2D[effLength];
        //        sTZtoTY = new FZMath.Point2D[effLength];

        //        for (int i = 0; i < effLength; i++)
        //        {
        //            szy1[i] = new FZMath.Point2D();
        //            szy2[i] = new FZMath.Point2D();
        //            szy3[i] = new FZMath.Point2D();

        //            sXX[i] = new FZMath.Point2D();
        //            sYY[i] = new FZMath.Point2D();
        //            sZZ[i] = new FZMath.Point2D();
        //            sTXTX[i] = new FZMath.Point2D();
        //            sTYTY[i] = new FZMath.Point2D();
        //            sTZTZ[i] = new FZMath.Point2D();

        //            sXtoY[i] = new FZMath.Point2D();
        //            sXtoZ[i] = new FZMath.Point2D();
        //            sXtoTX[i] = new FZMath.Point2D();
        //            sXtoTY[i] = new FZMath.Point2D();
        //            sXtoTZ[i] = new FZMath.Point2D();

        //            sYtoX[i] = new FZMath.Point2D();
        //            sYtoZ[i] = new FZMath.Point2D();
        //            sYtoTX[i] = new FZMath.Point2D();
        //            sYtoTY[i] = new FZMath.Point2D();
        //            sYtoTZ[i] = new FZMath.Point2D();

        //            sZtoX[i] = new FZMath.Point2D();
        //            sZtoY[i] = new FZMath.Point2D();
        //            sZtoTX[i] = new FZMath.Point2D();
        //            sZtoTY[i] = new FZMath.Point2D();
        //            sZtoTZ[i] = new FZMath.Point2D();

        //            sTXtoTY[i] = new FZMath.Point2D();
        //            sTXtoTZ[i] = new FZMath.Point2D();

        //            sTYtoTX[i] = new FZMath.Point2D();
        //            sTYtoTZ[i] = new FZMath.Point2D();

        //            sTZtoTX[i] = new FZMath.Point2D();
        //            sTZtoTY[i] = new FZMath.Point2D();
        //        }

        //        for (int i = 0; i < effLength; i++)
        //            stablizedData.Add(measure[i]);
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////
        //    //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
        //    StreamWriter lwr = null;
        //    double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

        //    if (!IsRemote)
        //    {
        //        lwr = new StreamWriter(AdminPathName + "FullData.csv");
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        int k = 0;
        //        for (int i = 0; i < fullLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //                slstr += measure[i][j].ToString("F5") + ",";
        //            if (i == effIndex[k])
        //            {
        //                slstr += "*";
        //                k++;
        //            }
        //            lwr.WriteLine(slstr);
        //            if (k == effLength)
        //                break;
        //        }
        //        lwr.Close();
        //    }

        //    string calName = axis;
        //    if (isAutoCalibrationEastView)
        //    {
        //        calName += "_EastView";
        //    }

        //    string strStabilizedFile = "";
        //    if (mAutoCalibrationCount % 2 == 0)
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
        //    else
        //        strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

        //    try
        //    {
        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    catch (Exception e)
        //    {
        //        Int64 lnow = (DateTime.Now.ToBinary()) % 1000000;
        //        if (mAutoCalibrationCount % 2 == 0)
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
        //        else
        //            strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

        //        lwr = new StreamWriter(strStabilizedFile);
        //    }
        //    if (lwr != null)
        //    {
        //        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
        //        for (int i = 0; i < effLength; i++)
        //        {
        //            string slstr = i.ToString() + ",";
        //            for (int j = 0; j < 23; j++)
        //            {
        //                //if (j < 19 || j == 22)
        //                slstr += stablizedData[i][j].ToString("F5") + ",";
        //                //else
        //                //{
        //                //    slstr += (RAD_To_MIN * stablizedData[i][j]).ToString("F5") + ",";
        //                //}
        //            }

        //            lwr.WriteLine(slstr);
        //        }
        //        lwr.Close();
        //    }

        //    //////////////////////////////////////////////////////////////
        //    //////////////////////////////////////////////////////////////

        //    //  axis 따라서 List 에 저장된 데이터를 처리한다.
        //    //double[] p2ndCoef = new double[3];
        //    //double[] p2ndCoef2 = new double[3];
        //    //double[] p2ndCoef3 = new double[3];
        //    int fovYoffset = GetROIY(0);
        //    string lstr = "";
        //    switch (axis)
        //    {
        //        case "Z":
        //            //  axis == "Z" : YLUT 의 경우 
        //            //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
        //            //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
        //            //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

        //            //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

        //            double a1 = 0;
        //            double b1 = 0;
        //            double a2 = 0;
        //            double b2 = 0;
        //            double a3 = 0;
        //            double b3 = 0;

        //            mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

        //            //  stablizedData[i][16] : X
        //            //  stablizedData[i][17] : Y
        //            //  stablizedData[i][18] : Z
        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
        //            double[] LUT1_tmp = new double[effLength];
        //            double[] LUT1 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

        //            LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
        //            LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

        //            //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
        //            //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
        //            //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
        //                szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
        //            double[] LUT2_tmp = new double[effLength];
        //            double[] LUT2 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

        //            LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
        //            LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

        //            //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
        //            //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
        //            //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
        //            //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
        //                //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
        //                szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
        //                szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
        //            double[] LUT3_tmp = new double[effLength];
        //            double[] LUT3 = new double[effLength];
        //            for (int i = 0; i < effLength; i++)
        //                LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

        //            for (int i = 1; i < effLength - 1; i++)
        //                LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

        //            LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
        //            LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

        //            //  Z scale
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sZZ[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
        //                sZZ[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZZ, effLength, ref ZtoZab);    // 2차로 변경
        //            //ZtoZab[0] = ZtoZab[0] * 0.9993; // 0.9992; // 헥사포드 Cal 변경
        //            //a = (a - 1) * 0.4 + 1; 
        //            //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
        //            //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
        //            //  Scale 만 조정해가면서 수차례 반복 필요
        //            //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


        //            if (mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView)
        //            {
        //                string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
        //                string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
        //                wr = new StreamWriter(srcFile);
        //                wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + ZtoZab[1].ToString());
        //                wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
        //                for (int i = 0; i < effLength; i++)
        //                {
        //                    wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
        //                }
        //                wr.Close();
        //                System.IO.File.Copy(srcFile, destFile, true);
        //            }

        //            /////////////////////////////////////////////////////////////////////////////////
        //            //  Z to X 계산
        //            //  Z vs X - Xprobe , Z vs Y - Yprobe
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
        //                //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
        //                sZtoX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
        //                sZtoY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage
        //                sZtoTX[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sZtoTY[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sZtoTZ[i] = new FZMath.Point2D(sZZ[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoX, effLength, ref ZtoXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoY, effLength, ref ZtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTX, effLength, ref ZtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTY, effLength, ref ZtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sZtoTZ, effLength, ref ZtoTZab);

        //            if (!isAutoCalibrationEastView)
        //            {
        //                lstr = "ZZ Scale\t" + ZtoZab[0].ToString("E5") + ",\r\n" + ZtoZab[1].ToString("E5") + ",\t" + ZtoZab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoX\t" + ZtoXab[0].ToString("E5") + ",\t" + ZtoXab[1].ToString("E5") + ",\t" + ZtoXab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoY\t" + ZtoYab[0].ToString("E5") + ",\t" + ZtoYab[1].ToString("E5") + ",\t" + ZtoYab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTX\t" + ZtoTXab[0].ToString("E5") + ",\t" + ZtoTXab[1].ToString("E5") + ",\t" + ZtoTXab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTY\t" + ZtoTYab[0].ToString("E5") + ",\t" + ZtoTYab[1].ToString("E5") + ",\t" + ZtoTYab[2].ToString("E5") + "\r\n";
        //                lstr += "ZtoTZ\t" + ZtoTZab[0].ToString("E5") + ",\t" + ZtoTZab[1].ToString("E5") + ",\t" + ZtoTZab[2].ToString("E5") + "\r\n";

        //                if (mAutoCalibrationCount % 2 == 0)
        //                {
        //                    string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                    StreamReader sr = new StreamReader(scaleNthetaFile);
        //                    string allstr = sr.ReadToEnd();
        //                    sr.Close();
        //                    string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    // Z
        //                    string[] strZscaleLine = allLines[3].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[3] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZscaleLine.Length; i++)
        //                        allLines[3] += strZscaleLine[i];
        //                    // Z TO X
        //                    string[] strZtoXLine = allLines[18].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[18] = ZtoXab[0].ToString("E5") + "\t" + ZtoXab[1].ToString("E5") + "\t" + ZtoXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoXLine.Length; i++)
        //                        allLines[18] += strZtoXLine[i];
        //                    // Z TO Y
        //                    string[] strZtoYLine = allLines[19].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[19] = ZtoYab[0].ToString("E5") + "\t" + ZtoYab[1].ToString("E5") + "\t" + ZtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoYLine.Length; i++)
        //                        allLines[19] += strZtoYLine[i];
        //                    // Z TO TX
        //                    string[] strZtoTXLine = allLines[20].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[20] = ZtoTXab[0].ToString("E5") + "\t" + ZtoTXab[1].ToString("E5") + "\t" + ZtoTXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTXLine.Length; i++)
        //                        allLines[20] += strZtoTXLine[i];
        //                    // Z TO TY
        //                    string[] strZtoTYLine = allLines[21].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[21] = ZtoTYab[0].ToString("E5") + "\t" + ZtoTYab[1].ToString("E5") + "\t" + ZtoTYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTYLine.Length; i++)
        //                        allLines[21] += strZtoTYLine[i];
        //                    // Z TO TZ
        //                    string[] strZtoTZLine = allLines[22].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[22] = ZtoTZab[0].ToString("E5") + "\t" + ZtoTZab[1].ToString("E5") + "\t" + ZtoTZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strZtoTZLine.Length; i++)
        //                        allLines[22] += strZtoTZLine[i];

        //                    wr = new StreamWriter(scaleNthetaFile);
        //                    for (int i = 0; i < allLines.Length; i++)
        //                    {
        //                        wr.WriteLine(allLines[i]);
        //                    }
        //                    wr.Close();
        //                }
        //            }


        //            //////////////////////////////////////////////////////////////////////////////////

        //            break;
        //        case "X":
        //            //  Axis = 1 : X scale 확인 및 저장
        //            //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
        //            //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
        //            //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
        //                sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값

        //                sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
        //                sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage

        //                sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sXtoTY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sXtoTZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref XtoXab);  //  A[0]X^2 + A[1]X + A[2]
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoY, effLength, ref XtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref XtoZab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTX, effLength, ref XtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTY, effLength, ref XtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoTZ, effLength, ref XtoTZab);

        //            lstr = "XX Scale\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoY\t" + XtoYab[0].ToString("E5") + ",\t" + XtoYab[1].ToString("E5") + ",\t" + XtoYab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoZ\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTX\t" + XtoTXab[0].ToString("E5") + ",\t" + XtoTXab[1].ToString("E5") + ",\t" + XtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTY\t" + XtoTYab[0].ToString("E5") + ",\t" + XtoTYab[1].ToString("E5") + ",\t" + XtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "XtoTZ\t" + XtoTZab[0].ToString("E5") + ",\t" + XtoTZab[1].ToString("E5") + ",\t" + XtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // X
        //                string[] strXXscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[1] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXXscaleLine.Length; i++)
        //                    allLines[1] += strXXscaleLine[i];
        //                // X TO Y
        //                string[] strXtoYLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[8] = XtoYab[0].ToString("E5") + "\t" + XtoYab[1].ToString("E5") + "\t" + XtoYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoYLine.Length; i++)
        //                    allLines[8] += strXtoYLine[i];
        //                // X TO Z
        //                string[] strXtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[9] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoZLine.Length; i++)
        //                    allLines[9] += strXtoZLine[i];
        //                // X TO TX
        //                string[] strXtoTXLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[10] = XtoTXab[0].ToString("E5") + "\t" + XtoTXab[1].ToString("E5") + "\t" + XtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTXLine.Length; i++)
        //                    allLines[10] += strXtoTXLine[i];
        //                // X TO TY
        //                string[] strXtoTYLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[11] = XtoTYab[0].ToString("E5") + "\t" + XtoTYab[1].ToString("E5") + "\t" + XtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTYLine.Length; i++)
        //                    allLines[11] += strXtoTYLine[i];
        //                // X TO TZ
        //                string[] strXtoTZLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[12] = XtoTZab[0].ToString("E5") + "\t" + XtoTZab[1].ToString("E5") + "\t" + XtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strXtoTZLine.Length; i++)
        //                    allLines[12] += strXtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        case "Y":
        //            mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
        //            mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
        //            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

        //            //  Axis = 2 : Y scale 확인 및 저장
        //            //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
        //                sYY[i].X = stablizedData[i][1];

        //                sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
        //                sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage

        //                sYtoTX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][3] - stablizedData[i][19]);   //  X - probe X from 6 axis stage
        //                sYtoTY[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sYtoTZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref YtoYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoX, effLength, ref YtoXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref YtoZab);

        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTX, effLength, ref YtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTY, effLength, ref YtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoTZ, effLength, ref YtoTZab);

        //            lstr = "YY Scale\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoX\t" + YtoXab[0].ToString("E5") + ",\t" + YtoXab[1].ToString("E5") + ",\t" + YtoXab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoZ\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";

        //            lstr += "YtoTX\t" + YtoTXab[0].ToString("E5") + ",\t" + YtoTXab[1].ToString("E5") + ",\t" + YtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTY\t" + YtoTYab[0].ToString("E5") + ",\t" + YtoTYab[1].ToString("E5") + ",\t" + YtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "YtoTZ\t" + YtoTZab[0].ToString("E5") + ",\t" + YtoTZab[1].ToString("E5") + ",\t" + YtoTZab[2].ToString("E5") + "\r\n";

        //            if (isAutoCalibrationEastView)
        //            {
        //                lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
        //            }

        //            wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            //////////////////////////////////////////////////////////////////////////////////                    
        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        //                sr.Close();
        //                if (isAutoCalibrationEastView)
        //                {
        //                    // East View Sclae
        //                    string[] strEastScaleLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[7] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
        //                    for (int i = 1; i < strEastScaleLine.Length; i++)
        //                        allLines[7] += strEastScaleLine[i];
        //                }
        //                else
        //                {
        //                    // Y
        //                    string[] strYYscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[2] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYYscaleLine.Length; i++)
        //                        allLines[2] += strYYscaleLine[i];
        //                    // Y TO X
        //                    string[] strYtoXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[13] = YtoXab[0].ToString("E5") + "\t" + YtoXab[1].ToString("E5") + "\t" + YtoXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoXLine.Length; i++)
        //                        allLines[13] += strYtoXLine[i];
        //                    // Y TO Z
        //                    string[] strYtoZLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[14] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoZLine.Length; i++)
        //                        allLines[14] += strYtoZLine[i];
        //                    // Y TO TX
        //                    string[] strYtoTXLine = allLines[15].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[15] = YtoTXab[0].ToString("E5") + "\t" + YtoTXab[1].ToString("E5") + "\t" + YtoTXab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTXLine.Length; i++)
        //                        allLines[15] += strYtoTXLine[i];
        //                    // Y TO TY
        //                    string[] strYtoTYLine = allLines[16].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[16] = YtoTYab[0].ToString("E5") + "\t" + YtoTYab[1].ToString("E5") + "\t" + YtoTYab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTYLine.Length; i++)
        //                        allLines[16] += strYtoTYLine[i];
        //                    // Y TO TZ
        //                    string[] strYtoTZLine = allLines[17].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                    allLines[17] = YtoTZab[0].ToString("E5") + "\t" + YtoTZab[1].ToString("E5") + "\t" + YtoTZab[2].ToString("E5") + "\t//";
        //                    for (int i = 1; i < strYtoTZLine.Length; i++)
        //                        allLines[17] += strYtoTZLine[i];
        //                }

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;

        //        //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
        //        case "TX":
        //            //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
        //            //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

        //            //  stablizedData[i][19] : TX   rad
        //            //  stablizedData[i][20] : TY   rad
        //            //  stablizedData[i][21] : TZ   rad

        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTXTX[i].Y = stablizedData[i][19]; // * RAD_To_MIN;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTXTX[i].X = stablizedData[i][3];

        //                sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][4] - stablizedData[i][20]);   //  Y - probe Y from 6 axis stage
        //                sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stablizedData[i][5] - stablizedData[i][21]);   //  Y - probe Y from 6 axis stage
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref TXtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTY, effLength, ref TXtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXtoTZ, effLength, ref TXtoTZab);

        //            lstr += "TX Scale\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TXtoTY\t" + TXtoTYab[0].ToString("E5") + ",\t" + TXtoTYab[1].ToString("E5") + ",\t" + TXtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "TXtoTZ\t" + TXtoTZab[0].ToString("E5") + ",\t" + TXtoTZab[1].ToString("E5") + ",\t" + TXtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TX
        //                string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXscaleLine.Length; i++)
        //                    allLines[4] += strTXscaleLine[i];
        //                // TX TO TY
        //                string[] strTXtoTYLine = allLines[23].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[23] = TXtoTYab[0].ToString("E5") + "\t" + TXtoTYab[1].ToString("E5") + "\t" + TXtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXtoTYLine.Length; i++)
        //                    allLines[23] += strTXtoTYLine[i];
        //                // TX TO TZ
        //                string[] strTXtoTZLine = allLines[24].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[24] = TXtoTZab[0].ToString("E5") + "\t" + TXtoTZab[1].ToString("E5") + "\t" + TXtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTXtoTZLine.Length; i++)
        //                    allLines[24] += strTXtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TY":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTYTY[i].Y = stablizedData[i][20]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTYTY[i].X = stablizedData[i][4];

        //                sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stablizedData[i][5] - stablizedData[i][21]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYTY, effLength, ref TYtoTYab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTX, effLength, ref TYtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTYtoTZ, effLength, ref TYtoTZab);

        //            lstr += "TY Scale\t" + TYtoTYab[0].ToString("E5") + ",\t" + TYtoTYab[1].ToString("E5") + ",\t" + TYtoTYab[2].ToString("E5") + "\r\n";
        //            lstr += "TYtoTX\t" + TYtoTXab[0].ToString("E5") + ",\t" + TYtoTXab[1].ToString("E5") + ",\t" + TYtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TYtoTZ\t" + TYtoTZab[0].ToString("E5") + ",\t" + TYtoTZab[1].ToString("E5") + ",\t" + TYtoTZab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TY
        //                string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[5] = TYtoTYab[0].ToString("E5") + "\t" + TYtoTYab[1].ToString("E5") + "\t" + TYtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYscaleLine.Length; i++)
        //                    allLines[5] += strTYscaleLine[i];
        //                // TY TO TX
        //                string[] strTYtoTXLine = allLines[25].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[25] = TYtoTXab[0].ToString("E5") + "\t" + TYtoTXab[1].ToString("E5") + "\t" + TYtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYtoTXLine.Length; i++)
        //                    allLines[25] += strTYtoTXLine[i];
        //                // TY TO TZ
        //                string[] strTYtoTZLine = allLines[26].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[26] = TYtoTZab[0].ToString("E5") + "\t" + TYtoTZab[1].ToString("E5") + "\t" + TYtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTYtoTZLine.Length; i++)
        //                    allLines[26] += strTYtoTZLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        case "TZ":
        //            //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
        //            //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
        //            //  데이터 개수가 N 개일 때
        //            //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
        //            //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
        //            //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
        //            //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
        //            for (int i = 0; i < effLength; i++)
        //            {
        //                sTZTZ[i].Y = stablizedData[i][21]; // * RAD_To_MIN;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
        //                sTZTZ[i].X = stablizedData[i][5];

        //                sTZtoTX[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][3] - stablizedData[i][19]);
        //                sTZtoTY[i] = new FZMath.Point2D(sTZTZ[i].X, stablizedData[i][4] - stablizedData[i][20]);
        //            }
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZTZ, effLength, ref TZtoTZab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTX, effLength, ref TZtoTXab);
        //            m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTZtoTY, effLength, ref TZtoTYab);

        //            lstr += "TZ Scale\t" + TZtoTZab[0].ToString("E5") + ",\t" + TZtoTZab[1].ToString("E5") + ",\t" + TZtoTZab[2].ToString("E5") + "\r\n";
        //            lstr += "TZtoTX\t" + TZtoTXab[0].ToString("E5") + ",\t" + TZtoTXab[1].ToString("E5") + ",\t" + TZtoTXab[2].ToString("E5") + "\r\n";
        //            lstr += "TZtoTY\t" + TZtoTYab[0].ToString("E5") + ",\t" + TZtoTYab[1].ToString("E5") + ",\t" + TZtoTYab[2].ToString("E5") + "\r\n";

        //            wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
        //            wr.Write(lstr);
        //            wr.Close();

        //            if (mAutoCalibrationCount % 2 == 0)
        //            {
        //                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
        //                StreamReader sr = new StreamReader(scaleNthetaFile);
        //                string allstr = sr.ReadToEnd();
        //                sr.Close();
        //                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                // TZ
        //                string[] strTZscaleLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[6] = TZtoTZab[0].ToString("E5") + "\t" + TZtoTZab[1].ToString("E5") + "\t" + TZtoTZab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZscaleLine.Length; i++)
        //                    allLines[6] += strTZscaleLine[i];
        //                // TZ TO TX
        //                string[] strTZtoTXLine = allLines[27].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[27] = TZtoTXab[0].ToString("E5") + "\t" + TZtoTXab[1].ToString("E5") + "\t" + TZtoTXab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZtoTXLine.Length; i++)
        //                    allLines[27] += strTZtoTXLine[i];
        //                // TZ TO TY
        //                string[] strTZtoTYLine = allLines[28].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                allLines[28] = TZtoTYab[0].ToString("E5") + "\t" + TZtoTYab[1].ToString("E5") + "\t" + TZtoTYab[2].ToString("E5") + "\t//";
        //                for (int i = 1; i < strTZtoTYLine.Length; i++)
        //                    allLines[28] += strTZtoTYLine[i];

        //                wr = new StreamWriter(scaleNthetaFile);
        //                for (int i = 0; i < allLines.Length; i++)
        //                {
        //                    wr.WriteLine(allLines[i]);
        //                }
        //                wr.Close();
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)delegate
        //        {
        //            tbCalibration.Text += lstr;
        //        });
        //    else
        //        tbCalibration.Text += lstr;

        //}

        //public enum Axis { X, Y, Z, TX, TY, TZ }

        //private void btnSingleCal_Click(object sender, EventArgs e)
        //{
        //    if (!mAutoCalibrationRun)
        //    {
        //        mAutoCalibrationRun = true;
        //        btnSingleCal.Text = "Stop Single Cal.";
        //    }
        //    else
        //    {
        //        mAutoCalibrationRun = false;
        //        btnSingleCal.Text = "Single Cal.";
        //        return;
        //    }

        //    mAutoCalibrationCount = 1;  // Sngle Cal.은 수동으로 적용하고 결과 확인용
        //    tbCalibration.Text = "";

        //    isAutoCalibrationEastView = false;
        //    if (rbCalEastView.Checked)
        //    {
        //        // East View Translation (Z -> Y)
        //        isAutoCalibrationEastView = true;

        //        // East View에서 Z Oneway Stroke
        //        double zOnewayStroke = 1750;
        //        if (tbZMaxStroke.Text.Length > 1)
        //            zOnewayStroke = double.Parse(tbZMaxStroke.Text);

        //        // East View에서 Y Oneway Stroke
        //        double yOnewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            yOnewayStroke = double.Parse(tbMaxStroke.Text);


        //        //LoadscaleNTheta();
        //        LoadScaleNTheta();

        //        // 241206 YLUT 적요안함.
        //        // m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

        //        Task.Run(() =>
        //        {
        //            // Z Translation
        //            mCalibrationFullData.Clear();
        //            mGageFullData.Clear();
        //            AutoCalibrationOld(Axis.Z, zOnewayStroke);
        //            // Y Translation
        //            mCalibrationFullData.Clear();
        //            mGageFullData.Clear();
        //            AutoCalibrationOld(Axis.Y, yOnewayStroke);

        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnSingleCal.Text = "Single Cal.";
        //            }));
        //        });
        //    }
        //    else
        //    {
        //        // Selected Aixs Translation
        //        Axis axis;
        //        if (rbCalZ.Checked)
        //            axis = Axis.Z;
        //        else if (rbCalX.Checked)
        //            axis = Axis.X;
        //        else if (rbCalY.Checked)
        //            axis = Axis.Y;
        //        else if (rbCalTX.Checked)
        //            axis = Axis.TX;
        //        else if (rbCalTY.Checked)
        //            axis = Axis.TY;
        //        else if (rbCalTZ.Checked)
        //            axis = Axis.TZ;
        //        else
        //            return;

        //        // One Way Stroke (X,Y,Z :um), (TX,TY,TZ : ?)
        //        double onewayStroke = 1900;
        //        if (tbMaxStroke.Text.Length > 1)
        //            onewayStroke = double.Parse(tbMaxStroke.Text);


        //        LoadScaleNTheta();
        //        // 241206 YLUT 적용안함.
        //        //m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
        //        Task.Run(() =>
        //        {
        //            mCalibrationFullData.Clear();
        //            AutoCalibrationOld(axis, onewayStroke);

        //            mAutoCalibrationRun = false;
        //            this.Invoke(new Action(() =>
        //            {
        //                btnSingleCal.Text = "Single Cal.";
        //            }));
        //        });
        //    }
        //}

        private void btnCheckFovBalance_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].mTrgBufLength = MILlib.MAX_TRGGRAB_COUNT;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].GrabB(0, true);
            m__G.oCam[0].ShowBalancingImage();
            SetDefaultMarkConfig(true);

        }
        bool IsLiveCropStop = false;
        private void button10_Click_1(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            IsLiveCropStop = false;
            m__G.oCam[0].DrawAllRectangles();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (CropView.InvokeRequired)
                        BeginInvoke((MethodInvoker)delegate
                        {
                            CropView.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropImgFromLive(0, m__G.oCam[0].mbDrawReference));
                        });
                    else
                        CropView.Image = BitmapConverter.ToBitmap(m__G.oCam[0].LoadCropImgFromLive(0, m__G.oCam[0].mbDrawReference));

                    if (IsLiveCropStop)
                        break;
                }

            });
        }



        private void button12_Click(object sender, EventArgs e)
        {
            if (!m__G.oCam[0].IsInit) return;
            m__G.oCam[0].DrawClear();
            IsLiveCropStop = true;
        }


        #region Crop Pos
        private (int, int) CheckSelectedPos()
        {
            int index;
            if (rdoPosA.Checked) index = 1;
            else if (rdoPosB.Checked) index = 0;
            else index = 2;

            int step = 1;
            if (chkPixel5.Checked) step = 5;

            return (index, step);
        }

        private void btnUpPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].UpPos(index, step);
        }
        private void btnDownPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].DownPos(index, step);
        }
        private void btnLeftPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].LeftPos(index, step);
        }

        private void btnRightPos_Click(object sender, EventArgs e)
        {
            var (index, step) = CheckSelectedPos();
            m__G.oCam[0].RightPos(index, step);
        }

        private int CropABgap
        {
            get
            {
                return m__G.oCam[0].CropABgap;
            }
        }
        private int CropCgap
        {
            get
            {
                try
                {
                    return Convert.ToInt32(tbDistancePosCD.Text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return m__G.oCam[0].CropCgap;
                }
            }
            set
            {
                if (tbDistancePosCD.InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbDistancePosCD.Text = value.ToString();
                    });
                else
                    tbDistancePosCD.Text = value.ToString();
            }
        }

        private void btnWidenPosCD_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (chkPixel5.Checked) step = 5;

            m__G.oCam[0].WidenPos(step);
            CropCgap = m__G.oCam[0].CropCgap;
        }

        private void btnNarrowPosCD_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (chkPixel5.Checked) step = 5;

            m__G.oCam[0].NarrowPos(step);
            CropCgap = m__G.oCam[0].CropCgap;
        }


        private void tbDistancePosCD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                m__G.oCam[0].AdjustDistancePos(CropCgap);
                CropCgap = m__G.oCam[0].CropCgap;
            }
        }

        private void tbDistancePosCD_Leave(object sender, EventArgs e)
        {
            m__G.oCam[0].AdjustDistancePos(CropCgap);
            CropCgap = m__G.oCam[0].CropCgap;
        }
        #endregion

        private void rdoPosA_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            m__G.oCam[0].SaveCropPosToXml();
            grpCrop.Hide();
        }

        private void btnChangeCrop_Click(object sender, EventArgs e)
        {
            grpCrop.Show();
        }

        private void cbDrawReference_CheckedChanged(object sender, EventArgs e)
        {
            System.Drawing.Point[] markPos = null;

            m__G.mFAL.GetDefaultMarkPosOnPanel(out markPos);        //  CropGap 이 적용되지 않은 상태의 결과를 반환한다.
            m__G.oCam[0].SetStdMarkPos(markPos, ref mStdMarkPos, Global.mMergeImgWidth, Global.mMergeImgHeight);   //  CropGap 이 적용되지 않은 상태의 데이터
            m__G.mFAL.SetMarkNorm();
            m__G.oCam[0].mbDrawReference = cbDrawReference.Checked;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void PogoPinUnloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(0, false);
        }

        private void BaseDownBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
            m__G.fGraph.mDriverIC.SocketTest(0, false);
            Thread.Sleep(300);

            m__G.fGraph.mDriverIC.SocketTest(1, false);
        }

        private void SidePushUnloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
        }

        private void PogoPinloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(0, true);
        }

        private void SidePushloadBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, true);
        }

        private void BaseUpBtn_Click(object sender, EventArgs e)
        {
            m__G.fGraph.mDriverIC.SocketTest(2, false);
            m__G.fGraph.mDriverIC.SocketTest(0, false);
            Thread.Sleep(300);

            m__G.fGraph.mDriverIC.SocketTest(1, true);
        }

        private void MotionStageBtn_Click(object sender, EventArgs e)
        {
            //if (m__G.mMotion == null)
            //{
            //    return;
            //}
            ////m__G.mMotion.Show();
            ////m__G.mMotion.BringToFront();
            ///
            //if (m__G.f_PIMotion == null) return;

            //m__G.f_PIMotion.Show();
            //m__G.f_PIMotion.BringToFront();

            if (m__G.fMotion == null) return;
            m__G.fMotion.Show();
            m__G.fMotion.BringToFront();
        }

        public int mAutoCalibrationCount = 0;
        public bool isAutoCalibrationEastView = false;       
        bool motorizedMeasurementRun = false;
        bool motorizedMeasurementAbort = false;

        public void InitializeScaleNTheta()
        {
            ms_scaleX = new double[3] { 0, 1, 0 };
            ms_scaleY = new double[3] { 0, 1, 0 };
            ms_scaleZ = new double[3] { 0, 1, 0 };
            ms_scaleTX = new double[3] { 0, 1, 0 };
            ms_scaleTY = new double[3] { 0, 1, 0 };
            ms_scaleTZ = new double[3] { 0, 1, 0 };
            ms_EastViewYPscale = 1.0;
            ms_XtoYbyView = new double[3];
            ms_XtoZbyView = new double[3];
            ms_XtoTXbyView = new double[3];
            ms_XtoTYbyView = new double[3];
            ms_XtoTZbyView = new double[3];
            ms_YtoXbyView = new double[3];
            ms_YtoZbyView = new double[3];
            ms_YtoTXbyView = new double[3];
            ms_YtoTYbyView = new double[3];
            ms_YtoTZbyView = new double[3];
            ms_ZtoXbyView = new double[3];
            ms_ZtoYbyView = new double[3];
            ms_ZtoTXbyView = new double[3];
            ms_ZtoTYbyView = new double[3];
            ms_ZtoTZbyView = new double[3];
            ms_TXtoTYbyView = new double[3];
            ms_TXtoTZbyView = new double[3];
            ms_TYtoTXbyView = new double[3];
            ms_TYtoTZbyView = new double[3];
            ms_TZtoTXbyView = new double[3];
            ms_TZtoTYbyView = new double[3];
            ms_XJtoXbyView = new double[2];
            ms_YJtoYbyView = new double[2];
            ms_ZJtoZbyView = new double[2];
            ms_TZtoZbyView = new double[3];
        }

        private void AutoCalibration()
        {
            AddVsnLog("Start auto calibration");

            // ScaleNTheta 초기화 (EastViewScale은 유지)
            double eastViewYPscale = m__G.oCam[0].mFAL.mFZM.mEastviewYPscale;
            InitializeScaleNTheta();
            ms_EastViewYPscale = eastViewYPscale;
            m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                             ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                             ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                             ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                             ms_TXtoTYbyView, ms_TXtoTZbyView,
                                             ms_TYtoTXbyView, ms_TYtoTZbyView,
                                             ms_TZtoTXbyView, ms_TZtoTYbyView,
                                             ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                             ms_TZtoZbyView);
            AddVsnLog("Reset all scales except for EastViewYP scale");
            SaveScaleNTheta();  //  초기화목적


            // OQC 
            AddVsnLog("Start to find CSHorg.");
            FindCSHorg();   // 엉뚱한 위치에서 FindPorg시작하는거 방지용

            for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
            {
                if (motorizedMeasurementAbort) return;
                AddVsnLog($"Start to find {pivotAxis} pivot.");
                FindPivot(pivotAxis);
            }

            SavePivots();


            if (motorizedMeasurementAbort) return;
            AddVsnLog("Start to find CSHorg, Reset Probe.");
            FindCSHorg(true);   // Probe 리셋

            if (motorizedMeasurementAbort) return;
            AddVsnLog("Start to find Fidorg");
            FindFidorg();

            SaveOQCCondition();

            // 측정 시작
            AddVsnLog("Start baseline measurement");
            AxisCalibration(Axis.Z, 1400, false, true, false); // 1750 //1550 //4line 1500
            AxisCalibration(Axis.Y, 1500, false, true, false);  // 1900 //1700 //4line 1500
            AxisCalibration(Axis.X, 1500, false, true, false);  // 1900 // 1700 // 4line 1500
            AxisCalibration(Axis.TY, 180, false, true, false); //200
            AxisCalibration(Axis.TX, 160, false, true, false);  // 160 //148
            AxisCalibration(Axis.TZ, 180, false, true, false); //200
            AddVsnLog("Finish  baseline measurement");

            AddVsnLog("Start verification measurement");
            AxisCalibration(Axis.Z, 1400, true, false, false);
            AxisCalibration(Axis.Y, 1500, true, false, false);
            AxisCalibration(Axis.X, 1500, true, false, false);
            AxisCalibration(Axis.TY, 180, true, false, false);
            AxisCalibration(Axis.TX, 160, true, false, false);
            AxisCalibration(Axis.TZ, 180, true, false, false);
            AddVsnLog("Finsh verification measurement");

            AddVsnLog("Finsh auto calibration");
        }
        private void EastViewCalibration(bool isRemote)
        {

            // East View에서 Z Oneway Stroke
            double zOnewayStroke = 1550;    // 1750
            double yOnewayStroke = 1700;    // 1900

            List<List<double[]>> stabilizedDataList = new List<List<double[]>>();

            // Z Translation
            if (motorizedMeasurementAbort) return;
            stabilizedDataList.Add( ScanAxis(Axis.Z, zOnewayStroke, 155, false) );

            // Y Translation
            if (motorizedMeasurementAbort) return;
            stabilizedDataList.Add( ScanAxis(Axis.Y, yOnewayStroke, 200, false) );

            CalculateESViewYPCalibration(stabilizedDataList, isRemote);

            //  여기까지가 East View Scale Scalibration
            /////////////////////////////////////////////////////////////////////////////////////////

            /////////////////////////////////////////////////////////////////////////////////////////
            //  여기는 Y1, Y2, Y3 선형화 LUT 측정
            //  Additional Scan for fine TX calibration
            //  ScanAxis(Axis , onewayStroke, step, isSaveImg, bool isCal = false, bool isRemote = false, Axis? axis2 = null, double posAxis2 = 0, int cntRepeat = 1, double xOffset = 0)
            //stabilizedDataList.Add(ScanAxis(Axis.Z, zOnewayStroke, 155, false, false, false, null, 0, 1, -1400 ));
            //stabilizedDataList.Add(ScanAxis(Axis.Z, zOnewayStroke, 155, false, false, false, null, 0, 1, -700));
            //stabilizedDataList.Add(ScanAxis(Axis.Z, zOnewayStroke, 155, false, false, false, null, 0, 1, +700));
            //stabilizedDataList.Add(ScanAxis(Axis.Z, zOnewayStroke, 155, false, false, false, null, 0, 1, +1400));
            //  stabilizedDataList[][][6 ~ 15] 가 Mark 좌표
            //  1) 임시로 stabilizedDataList 를 파일로 저장
            //  2) 각 Scan 으로부터 Z 위치별로 Y1, Y2, Y3 의 평균치계산
            //  3) Avg Y1, Y2, Y3 각각에 대해서 선형보간 식 L1, L2, L3 추출
            //  4) Avg Y1, Y2, Y3 에서 L1, L2, L3 를 빼준 Residual Error 를 추출
            //  5) -1300 ~ -1100, -900 ~ -700, -500 ~ -300, ... , 1100 ~ 1300 의 7개 위치에서의 Error 로 Y1LUT, Y2LUT, Y3LUT 를 생성
            //     Point3D Y1LUT[0] = (Y1, Yerror) 형태가 되어야함.
            //  6) ScaleNTheta 파일에 Y1LUT, Y2LUT, Y3LUT 를 저장

            //  Y1, Y2, Y3 선형화 LUT 보정의 적용
            //  7) ScaleNTheta 파일에서 Y1LUT, Y2LUT, Y3LUT 를 읽어들임
            //  8) Y1,Y2, Y3 측정치 확보됬으면 Y1, Y2, Y3 값에 따라 Y1LUT, Y2LUT, Y3LUT 적용
        }
        public void AxisCalibration(Axis axis, double onewayStrokeUm, bool isSingle, bool isRemote, bool isReCal)
        {
            // isRecal = true, isRemote = true : 원점에서 axis축 이동 1회 측정, 기존 scaleNtheta에 1차만 업데이트(scaleNthe 로드 필요)
            // isRecal = true, isRemote = false : 원점에서 axis축 이동 1회 측정
            // isRecal = false, isRemote = true : 다른 위치에서 axis축 이동 5회 측정, scaleNtheta에 업데이트(scaleNthe 초기화 필요)
            // isRecal = false, isRemote = false : 다른 위치에서 axis축 이동 5회 측정

            if (motorizedMeasurementAbort)
                return;

            AddVsnLog($"Start {axis}-axis Measurement");
            List<List<double[]>> stabilizedDataList = new List<List<double[]>>();
            
            switch (axis)
            {
                case Axis.X:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (motorizedMeasurementAbort) break;

                            double step = 100;

                            switch (i)
                            {
                                case 0:
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote));
                                    break;
                                case 1:
                                    // y 1000에서 x 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.Y, 900));    // 1000
                                    break;
                                case 2:
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.Y, -900));
                                    break;
                                case 3:
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.Z, 900));    // 1000
                                    break;
                                case 4:
                                    // z 1000에서 x 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.Z, -900));
                                    break;
                            }
                            if (isSingle) break;   // 재 Cal은 원점에서 x 이동만
                        }
                        break;
                    }
                case Axis.Y:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (motorizedMeasurementAbort) break;

                            double step = 100;

                            switch (i)
                            {
                                case 0:
                                    // y 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote));
                                    break;
                                case 1:
                                    // x 1000에서 y 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.X, 900));    // 1000
                                    break;
                                case 2:
                                    // x -1000에서 y 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.X, -900));
                                    break;
                                case 3:
                                    stabilizedDataList.Add(ScanAxis(axis, 700, step, true, true, isRemote, Axis.Z, 600));    // 700 600
                                    break;
                                case 4:
                                    // z 1000에서 x 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, 700, step, true, true, isRemote, Axis.Z, -600));
                                    break;
                            }
                            if (isSingle) break;
                        }
                        break;
                    }
                case Axis.Z:
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (motorizedMeasurementAbort) break;

                            double step = 100;

                            switch (i)
                            {
                                case 0:
                                    // z 이동하면서 측정       
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote));
                                    break;
                                case 1:
                                    // x 1000에서 z 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.X, 900));    // 1000
                                    break;
                                case 2:
                                    // x -1000에서 z 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote, Axis.X, -900));
                                    break;
                                case 3:
                                    // y 600에서 z 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, 700, step, true, true, isRemote, Axis.Y, 600));    // 600
                                    break;
                                case 4:
                                    // y -600에서 z 이동하면서 측정
                                    stabilizedDataList.Add(ScanAxis(axis, 700, step, true, true, isRemote, Axis.Y, -600));
                                    break;
                            }
                            if (isSingle) break;
                        }
                        break;
                    }
                case Axis.TX:
                case Axis.TY:
                case Axis.TZ:
                    {
                        double step = 12;

                        stabilizedDataList.Add(ScanAxis(axis, onewayStrokeUm, step, true, true, isRemote));
                        break;
                    }
            }
            RemoteAxisCalibration(axis, stabilizedDataList, isRemote, isReCal);
            AddVsnLog($"End {axis}-axis Measurement");
        }
        public List<double[]> ScanAxis(Axis axis, double onewayStroke, double step, bool isSaveImg, bool isCal = false, bool isRemote = false, Axis? axis2 = null, double posAxis2 = 0, int cntRepeat = 1, double xOffset = 0)
        {
            List<double[]> measuredData = new List<double[]>();

            // 6축 Stage 준비
            if (motorizedMeasurementAbort) { return measuredData; }
            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveAbs6D(mCSHorg.X + xOffset, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);


            if (axis > Axis.Z)  // TX, TY, TZ
            {
                int pivotAxis = (int)axis - 3;
                MotorSetPivot(mHexapodPivots[pivotAxis].X, mHexapodPivots[pivotAxis].Y, mHexapodPivots[pivotAxis].Z);
            }
            else
            {
                MotorSetPivot(0, 0, 0);
            }

            mAutoCalibrationIndex = 0;
            SingleFindMark();

            // Axis2의 posAxis2로 1/3씩 이동
            
            if (axis2 != null && posAxis2 != 0)
            {
                double orgPosAxis2 = MotorCurPosAxis((Axis)axis2);
                if (posAxis2 > 150)
                {
                    for (int i = 0; i < 3; i++)    // axis2 1/3씩 이동
                    {
                        if (motorizedMeasurementAbort) { return measuredData; }
                        double pos2 = orgPosAxis2 + (i + 1) * (posAxis2 / 3);
                        MotorMoveAbsAxis((Axis)axis2, pos2);
                        SingleFindMark();
                    }
                }
                else
                {
                    if (motorizedMeasurementAbort) { return measuredData; }
                    double pos2 = orgPosAxis2 + posAxis2;
                    MotorMoveAbsAxis((Axis)axis2, pos2);
                    SingleFindMark();
                }
            }

            // Axis1  onewayStroke 1/3씩 이동
            double orgPos = MotorCurPosAxis(axis);
            double pos = orgPos;
            if (onewayStroke > 150)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (motorizedMeasurementAbort) { return measuredData; }
                    pos -= onewayStroke / 10;
                    MotorMoveAbsAxis(axis, pos);
                    SingleFindMark();
                }
            }
            else
            {
                if (motorizedMeasurementAbort) { return measuredData; }
                pos -= onewayStroke;
                MotorMoveAbsAxis(axis, pos);
                SingleFindMark();
            }

            // backlash 제거를 위한 이동
            double[] backlashPos = axis < Axis.TX ? new double[] { 300, 200, 100 } : new double[] { 15, 10, 5 };
            
            foreach (var backlash in backlashPos)
            {
				if (motorizedMeasurementAbort) { return measuredData; }
                pos = orgPos - onewayStroke - backlash;
                MotorMoveAbsAxis(axis, pos);
                Thread.Sleep(300);
            }

            // 누적된 데이터 Clear
            mGageFullData.Clear();
            mCalibrationFullData.Clear();
            mPrismTXTYTZ.Clear();
            mStdevTXTYTZ.Clear();
            //mProbe_raw_Data.Clear();
            //mProbe_glass_compensation_Data.Clear();


            double movingStroke = -onewayStroke;
            pos = orgPos - onewayStroke;

            // 진짜 측정 시작
            while (movingStroke <= onewayStroke)
            {
                if (motorizedMeasurementAbort)
                {
                    measuredData = mCalibrationFullData.ToList();
                    return measuredData;
                }

                MotorMoveAbsAxis(axis, pos);
                if (axis < Axis.TX)
                {
                    Thread.Sleep(1000); 
                }
                else
                {
                    Thread.Sleep(600);  
                };

                for (int cnt = 0; cnt < cntRepeat; cnt++)
                {
                    SingleFindMark();
                    if (cntRepeat != 1)
                    {
                        Thread.Sleep(300);
                    }
                }

                // 이미지 저장               
                if (isSaveImg)
                {
                    string imgFileName = "";
                    if (isCal)
                    {
                        if (isRemote)
                        {
                            imgFileName = $"{m__G.m_RootDirectory}\\Result\\Calibration\\Image\\Before\\{axis}\\";
                            if (axis2 != null && posAxis2 != 0)
                            {
                                imgFileName += $"{axis2}{(int)posAxis2}\\";
                            }
                            else
                            {
                                imgFileName += "Org\\";
                            }
                        }
                        else
                        {
                            imgFileName = $"{m__G.m_RootDirectory}\\Result\\Calibration\\Image\\After\\{axis}\\";
                        }

                    }
                    else
                    {
                        imgFileName = $"{m__G.m_RootDirectory}\\Result\\Scan\\Image\\{axis}\\Org\\";
                    }
                    if (!Directory.Exists(imgFileName))
                    {
                        Directory.CreateDirectory(imgFileName);
                    }

                    imgFileName += $"{movingStroke}.bmp";
                    m__G.oCam[0].SaveGrabbedImage(1, imgFileName);
                }
                pos += step;
                movingStroke += step;
            }

            // CSH 0,0,0 위치로 복귀
            MotorMoveAbs6D(mCSHorg.X + xOffset, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
            MotorSetPivot(0, 0, 0);

            ////Probe 초기값 저장
            //string probeRawDataPathName = $"{m__G.m_RootDirectory}\\Result\\Glass\\";
            //string strProbeRawDataFile = $"{probeRawDataPathName}ProbeRawData_{axis}_{DateTime.Now:yyMMdd_HHmmss}.csv";

            //if (!Directory.Exists(probeRawDataPathName))
            //    Directory.CreateDirectory(probeRawDataPathName);

            //string slstr = "#,X,Y,TZ1,TZ2,TX,TY1,TY2\r\n";

            //for (int i = 0; i < mProbe_raw_Data.Count; i++)
            //{
            //    slstr += i.ToString() + "," +
            //                string.Join(",", mProbe_raw_Data[i].Take(7)
            //                .Select(x => x.ToString("F5"))) + "\r\n";
            //}

            //using (StreamWriter writer = new StreamWriter(strProbeRawDataFile))
            //{
            //    writer.WriteLine(slstr);
            //}

            ////Probe Glass 보정값 저장
            //string probeGlassCompensationDataPathName = $"{m__G.m_RootDirectory}\\Result\\Glass\\";
            //string strprobeGlassCompensationDataFile = $"{probeGlassCompensationDataPathName}ProbeGlassCompensationData_{axis}_{DateTime.Now:yyMMdd_HHmmss}.csv";

            //if (!Directory.Exists(probeGlassCompensationDataPathName))
            //    Directory.CreateDirectory(probeGlassCompensationDataPathName);

            //slstr = "#,X,Y,TZ1,TZ2,TX,TY1,TY2\r\n";

            //for (int i = 0; i < mProbe_glass_compensation_Data.Count; i++)
            //{
            //    slstr += i.ToString() + "," +
            //                string.Join(",", mProbe_glass_compensation_Data[i].Take(7)
            //                .Select(x => x.ToString("F5"))) + "\r\n";
            //}

            //using (StreamWriter writer = new StreamWriter(strprobeGlassCompensationDataFile))
            //{
            //    writer.WriteLine(slstr);
            //}

            // 측정 데이터 반환         
            measuredData = mCalibrationFullData.ToList();
            return measuredData;
        }

        public void CalculateESViewYPCalibration(List<List<double[]>> stabilizedDataList, bool isRemote)
        {
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;
            SaveMeasuredData(stabilizedDataList, $"EastViewYP_{(isRemote ? "Before" : "After")}","Calibration");

            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            string lstr = "";

            //  stablizedData[i][0] : X
            //  stablizedData[i][1] : Y
            //  stablizedData[i][2] : Z
            //  stablizedData[i][3 : TX   
            //  stablizedData[i][4 : TY   
            //  stablizedData[i][5] : TZ   
            //  stablizedData[i][16] : X
            //  stablizedData[i][17] : Y
            //  stablizedData[i][18] : Z
            //  stablizedData[i][19] : TX   
            //  stablizedData[i][20] : TY   
            //  stablizedData[i][21] : TZ   


            if (stabilizedDataList.Count < 2) return;

            // Z drive
            List<double[]> stabilizedData = stabilizedDataList[0];
            int effLength = stabilizedData.Count;
            // Ypp of the center of N-S Mark 
            mZCalAvgY1Y2pp = Math.Abs(stabilizedData[0][7] - stabilizedData[effLength - 1][7] + stabilizedData[0][9] - stabilizedData[effLength - 1][9]) / 2;
            // Ypp of the E Mark
            mZCalY3pp = Math.Abs(stabilizedData[0][11] - stabilizedData[effLength - 1][11]);

            // Y drive
            stabilizedData = stabilizedDataList[1];
            effLength = stabilizedData.Count;
            // Ypp of the center of N-S Mark 
            mYCalAvgY1Y2pp = Math.Abs(stabilizedData[0][7] - stabilizedData[effLength - 1][7] + stabilizedData[0][9] - stabilizedData[effLength - 1][9]) / 2;
            // Ypp of the E Mark
            mYCalY3pp = Math.Abs(stabilizedData[0][11] - stabilizedData[effLength - 1][11]);

            //  eastSideViewAngle 는 ( Z 구동 시 [Ypp of E Mark] - [Ypp of the center of N-S Mark] ) - ( Y 구동 시 [Ypp of E Mark] - [Ypp of the center of N-S Mark] ) 
            double eastSideViewAngle = (mZCalY3pp - mZCalAvgY1Y2pp) - (mYCalY3pp - mYCalAvgY1Y2pp); 

            mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

            lstr = $"E-S View YP : {eastSideViewAngle:E5}\r\n";
            lstr += "E-S View YP Scale\t" + mEstimatedEastViewYscale.ToString("E5") + "\r\n";

            AddVsnLog(lstr);

            if (isRemote)
            {
                // 저장
                string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                StreamReader sr = new StreamReader(scaleNthetaFile);
                string allstr = sr.ReadToEnd();
                string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                sr.Close();

                string[] strEastScaleLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                allLines[7] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
                for (int i = 1; i < strEastScaleLine.Length; i++)
                    allLines[7] += strEastScaleLine[i];

                StreamWriter wr = new StreamWriter(scaleNthetaFile);
                for (int i = 0; i < allLines.Length; i++)
                {
                    wr.WriteLine(allLines[i]);
                }
                wr.Close();
                AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");

                LoadScaleNTheta();
            }
        }
        public void RemoteAxisCalibration(Axis axis, List<List<double[]>> stabilizedDataList, bool isRemote, bool IsRecal)
        {
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;
            SaveMeasuredData(stabilizedDataList, $"{axis}_{(isRemote ? "Before" : "After")}", "Calibration");

            List<double[]> stabilizedData = null;
            int effLength = 0;

            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";

            string lstr = "";

            switch (axis)
            {
                //  stablizedData[i][0] : X
                //  stablizedData[i][1] : Y
                //  stablizedData[i][2] : Z
                //  stablizedData[i][3 : TX   
                //  stablizedData[i][4 : TY   
                //  stablizedData[i][5] : TZ   
                //  stablizedData[i][16] : X
                //  stablizedData[i][17] : Y
                //  stablizedData[i][18] : Z
                //  stablizedData[i][19] : TX   
                //  stablizedData[i][20] : TY   
                //  stablizedData[i][21] : TZ   
                case Axis.X:
                    {
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        var sXX = new FZMath.Point2D[effLength];
                        var sXtoY = new FZMath.Point2D[effLength];
                        var sXtoZ = new FZMath.Point2D[effLength];
                        var sXtoTX = new FZMath.Point2D[effLength];
                        var sXtoTY = new FZMath.Point2D[effLength];
                        var sXtoTZ = new FZMath.Point2D[effLength];

                        double[] sY = new double[effLength];
                        double[] sZ = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            double x = stabilizedData[i][0];    // CSH X 값
                            sXX[i] = new FZMath.Point2D(x, stabilizedData[i][16]); // probe X
                            sXtoY[i] = new FZMath.Point2D(x, stabilizedData[i][1] - stabilizedData[i][17]);   //  CSH Y - probe Y from 6 axis stage
                            sXtoZ[i] = new FZMath.Point2D(x, stabilizedData[i][2] - stabilizedData[i][18]);   //  CSH Z - probe Z from 6 axis stage
                            sXtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sXtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sXtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sY[i] = stabilizedData[i][1];
                            sZ[i] = stabilizedData[i][2];
                        }

                        // Sclae 계수 변수
                        double[] XtoXab = new double[3];
                        double[] XtoYab = new double[3];
                        double[] XtoZab = new double[3];
                        double[] XtoTXab = new double[3];
                        double[] XtoTYab = new double[3];
                        double[] XtoTZab = new double[3];

                        double Yavg = sY.Average();
                        double Zavg = sZ.Average();


                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXX, effLength, ref XtoXab[1], ref XtoXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoY, effLength, ref XtoYab[1], ref XtoYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoZ, effLength, ref XtoZab[1], ref XtoZab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTX, effLength, ref XtoTXab[1], ref XtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTY, effLength, ref XtoTYab[1], ref XtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTZ, effLength, ref XtoTZab[1], ref XtoTZab[2]);

                            lstr = $"XX Scale : {XtoXab[1]:E5}\r\n";
                            lstr += $"XtoY : {XtoYab[1]:E5}\r\n";
                            lstr += $"XtoZ : {XtoZab[1]:E5}\r\n";
                            lstr += $"XtoT : {XtoTXab[1]:E5}\r\n";
                            lstr += $"XtoTY : {XtoTYab[1]:E5}\r\n";
                            lstr += $"XtoTZ : {XtoTZab[1]:E5}\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    XtoXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleX[i] * XtoXab[i];
                                    XtoYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoYbyView[i] + XtoYab[i];
                                    XtoZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoZbyView[i] + XtoZab[i];
                                    XtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTXbyView[i] + XtoTXab[i];
                                    XtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTYbyView[i] + XtoTYab[i];
                                    XtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTZbyView[i] + XtoTZab[i];
                                }
                                else
                                {
                                    XtoXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleX[i];
                                    XtoYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoYbyView[i];
                                    XtoZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoZbyView[i];
                                    XtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTXbyView[i];
                                    XtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTYbyView[i];
                                    XtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mXtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXX, effLength, ref XtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXtoY, effLength, ref XtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sXtoZ, effLength, ref XtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTX, effLength, ref XtoTXab[1], ref XtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTY, effLength, ref XtoTYab[1], ref XtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sXtoTZ, effLength, ref XtoTZab[1], ref XtoTZab[2]);

                            lstr = "XX Scale\t" + XtoXab[0].ToString("E5") + ",\t" + XtoXab[1].ToString("E5") + ",\t" + XtoXab[2].ToString("E5") + "\r\n";
                            lstr += "XtoY\t" + XtoYab[0].ToString("E5") + ",\t" + XtoYab[1].ToString("E5") + ",\t" + XtoYab[2].ToString("E5") + "\r\n";
                            lstr += "XtoZ\t" + XtoZab[0].ToString("E5") + ",\t" + XtoZab[1].ToString("E5") + ",\t" + XtoZab[2].ToString("E5") + "\r\n";
                            lstr += "XtoTX\t" + XtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTY\t" + XtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "XtoTZ\t" + XtoTZab[1].ToString("E5") + "\r\n";
                        }

                        // XJtoX 구하기
                        double[] XJtoXab = null;
                        if (!IsRecal)
                        {
                            XJtoXab = new double[2];

                            // XY to X
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] XYtoXab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sY = new double[effLength];
                                    sXX = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sY[j] = stabilizedData[j][1];    // CSH Y 값
                                        sXX[j] = new FZMath.Point2D(stabilizedData[j][0], stabilizedData[j][16]); // probe X
                                    }

                                    double YavgfromXY = sY.Average();
                                    double[] XtoXabfromeXY = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref XtoXabfromeXY[1], ref XtoXabfromeXY[0]);

                                    XYtoXab[i] = (XtoXabfromeXY[1] - XtoXab[1]) / (YavgfromXY - Yavg);
                                }

                                XJtoXab[0] = XYtoXab.Average();
                            }

                            // XZ to X
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] XZtoXab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sZ = new double[effLength];
                                    sXX = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sZ[j] = stabilizedData[j][2];    // CSH Z 값
                                        sXX[j] = new FZMath.Point2D(stabilizedData[j][0], stabilizedData[j][16]); // probe Z
                                    }

                                    double ZavgfromXZ = sZ.Average();
                                    double[] XtoXabfromeXZ = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref XtoXabfromeXZ[1], ref XtoXabfromeXZ[0]);

                                    XZtoXab[i] = (XtoXabfromeXZ[1] - XtoXab[1]) / (ZavgfromXZ - Zavg);
                                }

                                XJtoXab[1] = XZtoXab.Average();
                            }

                            lstr += "XYtoX\t" + XJtoXab[0].ToString("E5") + "\r\n";
                            lstr += "XZtoX\t" + XJtoXab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // X
                            string[] strXXscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[1] = XtoXab[0].ToString("E5") + "\t" + XtoXab[1].ToString("E5") + "\t" + XtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXXscaleLine.Length; i++)
                                allLines[1] += strXXscaleLine[i];
                            // X TO Y
                            string[] strXtoYLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[8] = XtoYab[0].ToString("E5") + "\t" + XtoYab[1].ToString("E5") + "\t" + XtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXtoYLine.Length; i++)
                                allLines[8] += strXtoYLine[i];
                            // X TO Z
                            string[] strXtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[9] = XtoZab[0].ToString("E5") + "\t" + XtoZab[1].ToString("E5") + "\t" + XtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strXtoZLine.Length; i++)
                                allLines[9] += strXtoZLine[i];
                            // X TO TX
                            string[] strXtoTXLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[10] = $"{0:E5}\t{XtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTXLine.Length; i++)
                                allLines[10] += strXtoTXLine[i];
                            // X TO TY
                            string[] strXtoTYLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[11] = $"{0:E5}\t{XtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTYLine.Length; i++)
                                allLines[11] += strXtoTYLine[i];
                            // X TO TZ
                            string[] strXtoTZLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[12] = $"{0:E5}\t{XtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strXtoTZLine.Length; i++)
                                allLines[12] += strXtoTZLine[i];

                            if (XJtoXab != null && !IsRecal)
                            {
                                string[] strXJtoXLine = allLines[29].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[29] = XJtoXab[0].ToString("E5") + "\t" + XJtoXab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strXJtoXLine.Length; i++)
                                    allLines[29] += strXJtoXLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.Y:
                    {
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        var sYY = new FZMath.Point2D[effLength];
                        var sYtoX = new FZMath.Point2D[effLength];
                        var sYtoZ = new FZMath.Point2D[effLength];
                        var sYtoTX = new FZMath.Point2D[effLength];
                        var sYtoTY = new FZMath.Point2D[effLength];
                        var sYtoTZ = new FZMath.Point2D[effLength];

                        double[] sX = new double[effLength];
                        double[] sZ = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            double x = stabilizedData[i][1];    // CSH Y 값
                            sYY[i] = new FZMath.Point2D(x, stabilizedData[i][17]); // probe X
                            sYtoX[i] = new FZMath.Point2D(x, stabilizedData[i][0] - stabilizedData[i][16]);   //  CSH X - probe X from 6 axis stage
                            sYtoZ[i] = new FZMath.Point2D(x, stabilizedData[i][2] - stabilizedData[i][18]);   //  CSH Z - probe Z from 6 axis stage
                            sYtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sYtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sYtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sX[i] = stabilizedData[i][0];
                            sZ[i] = stabilizedData[i][2];
                        }

                        double[] YtoYab = new double[3];
                        double[] YtoXab = new double[3];
                        double[] YtoZab = new double[3];
                        double[] YtoTXab = new double[3];
                        double[] YtoTYab = new double[3];
                        double[] YtoTZab = new double[3];

                        double Xavg = sX.Average();
                        double Zavg = sZ.Average();

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYY, effLength, ref YtoYab[1], ref YtoYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoX, effLength, ref YtoXab[1], ref YtoXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoZ, effLength, ref YtoZab[1], ref YtoZab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTX, effLength, ref YtoTXab[1], ref YtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTY, effLength, ref YtoTYab[1], ref YtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTZ, effLength, ref YtoTZab[1], ref YtoTZab[2]);

                            lstr = "YY Scale\t" + YtoYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoX\t" + YtoXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoZ\t" + YtoZab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTX\t" + YtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTY\t" + YtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTZ\t" + YtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    YtoYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleY[i] * YtoYab[i];
                                    YtoXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoXbyView[i] + YtoXab[i];
                                    YtoZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoZbyView[i] + YtoZab[i];
                                    YtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTXbyView[i] + YtoTXab[i];
                                    YtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTYbyView[i] + YtoTYab[i];
                                    YtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTZbyView[i] + YtoTZab[i];
                                }
                                else
                                {
                                    YtoYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleY[i];
                                    YtoXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoXbyView[i];
                                    YtoZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoZbyView[i];
                                    YtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTXbyView[i];
                                    YtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTYbyView[i];
                                    YtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mYtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYY, effLength, ref YtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYtoX, effLength, ref YtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sYtoZ, effLength, ref YtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTX, effLength, ref YtoTXab[1], ref YtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTY, effLength, ref YtoTYab[1], ref YtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sYtoTZ, effLength, ref YtoTZab[1], ref YtoTZab[2]);

                            lstr = "YY Scale\t" + YtoYab[0].ToString("E5") + ",\t" + YtoYab[1].ToString("E5") + ",\t" + YtoYab[2].ToString("E5") + "\r\n";
                            lstr += "YtoX\t" + YtoXab[0].ToString("E5") + ",\t" + YtoXab[1].ToString("E5") + ",\t" + YtoXab[2].ToString("E5") + "\r\n";
                            lstr += "YtoZ\t" + YtoZab[0].ToString("E5") + ",\t" + YtoZab[1].ToString("E5") + ",\t" + YtoZab[2].ToString("E5") + "\r\n";

                            lstr += "YtoTX\t" + YtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTY\t" + YtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "YtoTZ\t" + YtoTYab[1].ToString("E5") + "\r\n";
                        }

                        // YJtoY 구하기
                        double[] YJtoYab = null;
                        if (!IsRecal)
                        {
                            YJtoYab = new double[2];

                            // YX to Y
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] YXtoYab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sX = new double[effLength];
                                    sYY = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sX[j] = stabilizedData[j][0];    // CSH X 값
                                        sYY[j] = new FZMath.Point2D(stabilizedData[j][1], stabilizedData[j][17]); // probe Y
                                    }

                                    double XavgfromYX = sX.Average();
                                    double[] YtoYabfromeYX = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref YtoYabfromeYX[1], ref YtoYabfromeYX[0]);

                                    YXtoYab[i] = (YtoYabfromeYX[1] - YtoYab[1]) / (XavgfromYX - Xavg);
                                }

                                YJtoYab[0] = YXtoYab.Average();
                            }

                            // YZ to Y
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] YZtoYab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sZ = new double[effLength];
                                    sYY = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sZ[j] = stabilizedData[j][2];    // CSH Z 값
                                        sYY[j] = new FZMath.Point2D(stabilizedData[j][1], stabilizedData[j][17]); // probe Y
                                    }

                                    double ZavgfromYZ = sZ.Average();
                                    double[] YtoYabfromeYZ = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref YtoYabfromeYZ[1], ref YtoYabfromeYZ[0]);

                                    YZtoYab[i] = (YtoYabfromeYZ[1] - YtoYab[1]) / (ZavgfromYZ - Zavg);
                                }

                                YJtoYab[1] = YZtoYab.Average();
                            }

                            lstr += "YXtoY\t" + YJtoYab[0].ToString("E5") + "\r\n";
                            lstr += "YZtoY\t" + YJtoYab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            // Y
                            string[] strYYscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[2] = YtoYab[0].ToString("E5") + "\t" + YtoYab[1].ToString("E5") + "\t" + YtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYYscaleLine.Length; i++)
                                allLines[2] += strYYscaleLine[i];
                            // Y TO X
                            string[] strYtoXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[13] = YtoXab[0].ToString("E5") + "\t" + YtoXab[1].ToString("E5") + "\t" + YtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoXLine.Length; i++)
                                allLines[13] += strYtoXLine[i];
                            // Y TO Z
                            string[] strYtoZLine = allLines[14].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[14] = YtoZab[0].ToString("E5") + "\t" + YtoZab[1].ToString("E5") + "\t" + YtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoZLine.Length; i++)
                                allLines[14] += strYtoZLine[i];
                            // Y TO TX
                            string[] strYtoTXLine = allLines[15].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[15] = $"{0:E5}\t{YtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTXLine.Length; i++)
                                allLines[15] += strYtoTXLine[i];
                            // Y TO TY
                            string[] strYtoTYLine = allLines[16].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[16] = $"{0:E5}\t{YtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTYLine.Length; i++)
                                allLines[16] += strYtoTYLine[i];
                            // Y TO TZ
                            string[] strYtoTZLine = allLines[17].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[17] = $"{0:E5}\t{YtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strYtoTZLine.Length; i++)
                                allLines[17] += strYtoTZLine[i];

                            if (YJtoYab != null && !IsRecal)
                            {
                                string[] strYJtoYLine = allLines[30].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[30] = YJtoYab[0].ToString("E5") + "\t" + YJtoYab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strYJtoYLine.Length; i++)
                                    allLines[30] += strYJtoYLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.Z:
                    {
                        //  Z scale 구하기
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        FZMath.Point2D[] sZZ = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTX = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTY = new FZMath.Point2D[effLength];
                        FZMath.Point2D[] sZtoTZ = new FZMath.Point2D[effLength];

                        double[] sX = new double[effLength];
                        double[] sY = new double[effLength];

                        for (int i = 0; i < effLength; i++)
                        {

                            double x = stabilizedData[i][2];    // CSH Z 값
                            sZZ[i] = new FZMath.Point2D(x, stabilizedData[i][18]); // probe Z
                            sZtoX[i] = new FZMath.Point2D(x, stabilizedData[i][0] - stabilizedData[i][16]);   //  CSH X - probe X from 6 axis stage
                            sZtoY[i] = new FZMath.Point2D(x, stabilizedData[i][1] - stabilizedData[i][17]);   //  CSH Y - probe Y from 6 axis stage
                            sZtoTX[i] = new FZMath.Point2D(x, stabilizedData[i][3] - stabilizedData[i][19]);   //  CSH TX - probe TX from 6 axis stage
                            sZtoTY[i] = new FZMath.Point2D(x, stabilizedData[i][4] - stabilizedData[i][20]);   //  CSH TY - probe TY from 6 axis stage
                            sZtoTZ[i] = new FZMath.Point2D(x, stabilizedData[i][5] - stabilizedData[i][21]);   //  CSH TZ - probe TZ from 6 axis stage

                            sX[i] = stabilizedData[i][0];
                            sY[i] = stabilizedData[i][1];
                        }

                        // Scale 변수
                        double[] ZtoZab = new double[3];
                        double[] ZtoXab = new double[3];
                        double[] ZtoYab = new double[3];
                        double[] ZtoTXab = new double[3];
                        double[] ZtoTYab = new double[3];
                        double[] ZtoTZab = new double[3];

                        double Xavg = sX.Average();
                        double Yavg = sY.Average();

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZZ, effLength, ref ZtoZab[1], ref ZtoZab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoX, effLength, ref ZtoXab[1], ref ZtoXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoY, effLength, ref ZtoYab[1], ref ZtoXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTX, effLength, ref ZtoTXab[1], ref ZtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTY, effLength, ref ZtoTYab[1], ref ZtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTZ, effLength, ref ZtoTZab[1], ref ZtoTZab[2]);

                            lstr = "ZZ Scale\t" + ZtoZab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoX\t" + ZtoXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoY\t" + ZtoYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTX\t" + ZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTY\t" + ZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTZ\t" + ZtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    ZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleZ[i] * ZtoZab[i];
                                    ZtoXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoXbyView[i] + ZtoXab[i];
                                    ZtoYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoYbyView[i] + ZtoYab[i];
                                    ZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTXbyView[i] + ZtoTXab[i];
                                    ZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTYbyView[i] + ZtoTYab[i];
                                    ZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTZbyView[i] + ZtoTZab[i];
                                }
                                else
                                {
                                    ZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleZ[i];
                                    ZtoXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoXbyView[i];
                                    ZtoYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoYbyView[i];
                                    ZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTXbyView[i];
                                    ZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTYbyView[i];
                                    ZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mZtoTZbyView[i];
                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZZ, effLength, ref ZtoZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZtoX, effLength, ref ZtoXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sZtoY, effLength, ref ZtoYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTX, effLength, ref ZtoTXab[1], ref ZtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTY, effLength, ref ZtoTYab[1], ref ZtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sZtoTZ, effLength, ref ZtoTZab[1], ref ZtoTZab[2]);

                            lstr = "ZZ Scale\t" + ZtoZab[0].ToString("E5") + ",\r\n" + ZtoZab[1].ToString("E5") + ",\t" + ZtoZab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoX\t" + ZtoXab[0].ToString("E5") + ",\t" + ZtoXab[1].ToString("E5") + ",\t" + ZtoXab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoY\t" + ZtoYab[0].ToString("E5") + ",\t" + ZtoYab[1].ToString("E5") + ",\t" + ZtoYab[2].ToString("E5") + "\r\n";
                            lstr += "ZtoTX\t" + ZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTY\t" + ZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "ZtoTZ\t" + ZtoTZab[1].ToString("E5") + "\r\n";
                        }



                        // ZJtoZ 구하기
                        double[] ZJtoZab = null;
                        if (!IsRecal)
                        {
                            ZJtoZab = new double[2];

                            // ZX to Z
                            if (stabilizedDataList.Count > 2)
                            {

                                double[] ZXtoZab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 1];
                                    effLength = stabilizedData.Count;

                                    sX = new double[effLength];
                                    sZZ = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sX[j] = stabilizedData[j][0];    // CSH X 값
                                        sZZ[j] = new FZMath.Point2D(stabilizedData[j][2], stabilizedData[j][18]); // probe Z
                                    }

                                    double XavgfromZX = sX.Average();
                                    double[] ZtoZabfromeZX = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZZ, effLength, ref ZtoZabfromeZX[1], ref ZtoZabfromeZX[0]);

                                    ZXtoZab[i] = (ZtoZabfromeZX[1] - ZtoZab[1]) / (XavgfromZX - Xavg);
                                }

                                ZJtoZab[0] = ZXtoZab.Average();
                            }

                            // ZY to Z
                            if (stabilizedDataList.Count == 5)
                            {
                                double[] ZYtoZab = new double[2];

                                for (int i = 0; i < 2; i++)
                                {
                                    stabilizedData = stabilizedDataList[i + 3];
                                    effLength = stabilizedData.Count;

                                    sY = new double[effLength];
                                    sZZ = new FZMath.Point2D[effLength];

                                    for (int j = 0; j < effLength; j++)
                                    {
                                        sY[j] = stabilizedData[j][1];    // CSH Y 값
                                        sZZ[j] = new FZMath.Point2D(stabilizedData[j][2], stabilizedData[j][18]); // probe Z
                                    }

                                    double ZavgfromZY = sY.Average();
                                    double[] ZtoZabfromeZY = new double[3];
                                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZZ, effLength, ref ZtoZabfromeZY[1], ref ZtoZabfromeZY[0]);

                                    ZYtoZab[i] = (ZtoZabfromeZY[1] - ZtoZab[1]) / (ZavgfromZY - Yavg);
                                }

                                ZJtoZab[1] = ZYtoZab.Average();
                            }

                            lstr += "ZXtoZ\t" + ZJtoZab[0].ToString("E5") + "\r\n";
                            lstr += "ZXtoZ\t" + ZJtoZab[1].ToString("E5") + "\r\n";
                        }

                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // Z
                            string[] strZscaleLine = allLines[3].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[3] = ZtoZab[0].ToString("E5") + "\t" + ZtoZab[1].ToString("E5") + "\t" + ZtoZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZscaleLine.Length; i++)
                                allLines[3] += strZscaleLine[i];
                            // Z TO X
                            string[] strZtoXLine = allLines[18].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[18] = ZtoXab[0].ToString("E5") + "\t" + ZtoXab[1].ToString("E5") + "\t" + ZtoXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoXLine.Length; i++)
                                allLines[18] += strZtoXLine[i];
                            // Z TO Y
                            string[] strZtoYLine = allLines[19].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[19] = ZtoYab[0].ToString("E5") + "\t" + ZtoYab[1].ToString("E5") + "\t" + ZtoYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoYLine.Length; i++)
                                allLines[19] += strZtoYLine[i];
                            // Z TO TX
                            string[] strZtoTXLine = allLines[20].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[20] = $"{0:E5}\t{ZtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTXLine.Length; i++)
                                allLines[20] += strZtoTXLine[i];
                            // Z TO TY
                            string[] strZtoTYLine = allLines[21].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[21] = $"{0:E5}\t{ZtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTYLine.Length; i++)
                                allLines[21] += strZtoTYLine[i];
                            // Z TO TZ
                            string[] strZtoTZLine = allLines[22].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[22] = $"{0:E5}\t{ZtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strZtoTZLine.Length; i++)
                                allLines[22] += strZtoTZLine[i];

                            if (ZJtoZab != null && !IsRecal)
                            {
                                string[] strZJtoZLine = allLines[31].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                allLines[31] = ZJtoZab[0].ToString("E5") + "\t" + ZJtoZab[1].ToString("E5") + "\t//";
                                for (int i = 1; i < strZJtoZLine.Length; i++)
                                    allLines[31] += strZJtoZLine[i];
                            }

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.TX:
                    {
                        //  TX scale 구하기
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        var sTXTX = new FZMath.Point2D[effLength];
                        var sTXtoTY = new FZMath.Point2D[effLength];
                        var sTXtoTZ = new FZMath.Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTXTX[i] = new FZMath.Point2D(stabilizedData[i][3], stabilizedData[i][19]);
                            sTXtoTY[i] = new FZMath.Point2D(sTXTX[i].X, stabilizedData[i][4] - stabilizedData[i][20]);   //  TY - probe TY from 6 axis stage
                            sTXtoTZ[i] = new FZMath.Point2D(sTXTX[i].X, stabilizedData[i][5] - stabilizedData[i][21]);   //  TZ - probe TZ from 6 axis stage
                        }

                        double[] TXtoTXab = new double[3];
                        double[] TXtoTYab = new double[3];
                        double[] TXtoTZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXTX, effLength, ref TXtoTXab[1], ref TXtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTY, effLength, ref TXtoTYab[1], ref TXtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTZ, effLength, ref TXtoTZab[1], ref TXtoTZab[2]);

                            lstr = "TX Scale\t" + TXtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTY\t" + TXtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTZ\t" + TXtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TXtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTX[i] * TXtoTXab[i];
                                    TXtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTYbyView[i] + TXtoTYab[i];
                                    TXtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTZbyView[i] + TXtoTZab[i];

                                }
                                else
                                {
                                    TXtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTX[i];
                                    TXtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTYbyView[i];
                                    TXtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTXtoTZbyView[i];

                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTXTX, effLength, ref TXtoTXab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTY, effLength, ref TXtoTYab[1], ref TXtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTXtoTZ, effLength, ref TXtoTZab[1], ref TXtoTZab[2]);

                            lstr = "TX Scale\t" + TXtoTXab[0].ToString("E5") + ",\t" + TXtoTXab[1].ToString("E5") + ",\t" + TXtoTXab[2].ToString("E5") + "\r\n";
                            lstr += "TXtoTY\t" + TXtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TXtoTZ\t" + TXtoTZab[1].ToString("E5") + "\r\n";
                        }



                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // TX
                            string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[4] = TXtoTXab[0].ToString("E5") + "\t" + TXtoTXab[1].ToString("E5") + "\t" + TXtoTXab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTXscaleLine.Length; i++)
                                allLines[4] += strTXscaleLine[i];
                            // TX TO TY
                            string[] strTXtoTYLine = allLines[23].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[23] = $"{0:E5}\t{TXtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTXtoTYLine.Length; i++)
                                allLines[23] += strTXtoTYLine[i];
                            // TX TO TZ
                            string[] strTXtoTZLine = allLines[24].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[24] = $"{0:E5}\t{TXtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTXtoTZLine.Length; i++)
                                allLines[24] += strTXtoTZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
                case Axis.TY:
                    {
                        //  TY scale 구하기
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        var sTYTY = new FZMath.Point2D[effLength];
                        var sTYtoTX = new FZMath.Point2D[effLength];
                        var sTYtoTZ = new FZMath.Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTYTY[i] = new FZMath.Point2D(stabilizedData[i][4], stabilizedData[i][20]);
                            sTYtoTX[i] = new FZMath.Point2D(sTYTY[i].X, stabilizedData[i][3] - stabilizedData[i][19]);
                            sTYtoTZ[i] = new FZMath.Point2D(sTYTY[i].X, stabilizedData[i][5] - stabilizedData[i][21]);
                        }


                        double[] TYtoTYab = new double[3];
                        double[] TYtoTXab = new double[3];
                        double[] TYtoTZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYTY, effLength, ref TYtoTYab[1], ref TYtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTX, effLength, ref TYtoTXab[1], ref TYtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTZ, effLength, ref TYtoTZab[1], ref TYtoTZab[2]);
                            lstr = "TY Scale\t" + TYtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTX\t" + TYtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTZ\t" + TYtoTZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TYtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTY[i] * TYtoTYab[i];
                                    TYtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTXbyView[i] + TYtoTXab[i];
                                    TYtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTZbyView[i] + TYtoTZab[i];

                                }
                                else
                                {
                                    TYtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTY[i];
                                    TYtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTXbyView[i];
                                    TYtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mTYtoTZbyView[i];

                                }
                            }
                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTYTY, effLength, ref TYtoTYab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTX, effLength, ref TYtoTXab[1], ref TYtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTYtoTZ, effLength, ref TYtoTZab[1], ref TYtoTZab[2]);

                            lstr = "TY Scale\t" + TYtoTYab[0].ToString("E5") + ",\t" + TYtoTYab[1].ToString("E5") + ",\t" + TYtoTYab[2].ToString("E5") + "\r\n";
                            lstr += "TYtoTX\t" + TYtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TYtoTZ\t" + TYtoTZab[1].ToString("E5") + "\r\n";
                        }


                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            // TY
                            string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[5] = TYtoTYab[0].ToString("E5") + "\t" + TYtoTYab[1].ToString("E5") + "\t" + TYtoTYab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTYscaleLine.Length; i++)
                                allLines[5] += strTYscaleLine[i];
                            // TY TO TX
                            string[] strTYtoTXLine = allLines[25].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[25] = $"{0:E5}\t{TYtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTYtoTXLine.Length; i++)
                                allLines[25] += strTYtoTXLine[i];
                            // TY TO TZ
                            string[] strTYtoTZLine = allLines[26].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[26] = $"{0:E5}\t{TYtoTZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTYtoTZLine.Length; i++)
                                allLines[26] += strTYtoTZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");

                        }
                        break;
                    }
                case Axis.TZ:
                    {
                        //  TZ scale 구하기
                        stabilizedData = stabilizedDataList[0];
                        effLength = stabilizedData.Count;

                        var sTZTZ = new FZMath.Point2D[effLength];
                        var sTZtoTX = new FZMath.Point2D[effLength];
                        var sTZtoTY = new FZMath.Point2D[effLength];
                        var sTZtoZ = new Point2D[effLength];

                        for (int i = 0; i < effLength; i++)
                        {
                            sTZTZ[i] = new FZMath.Point2D(stabilizedData[i][5], stabilizedData[i][21]);
                            sTZtoTX[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][3] - stabilizedData[i][19]);
                            sTZtoTY[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][4] - stabilizedData[i][20]);
                            sTZtoZ[i] = new FZMath.Point2D(sTZTZ[i].X, stabilizedData[i][2] - stabilizedData[i][18]);
                        }

                        double[] TZtoTZab = new double[3];
                        double[] TZtoTXab = new double[3];
                        double[] TZtoTYab = new double[3];
                        double[] TZtoZab = new double[3];

                        if (IsRecal)
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZTZ, effLength, ref TZtoTZab[1], ref TZtoTZab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTX, effLength, ref TZtoTXab[1], ref TZtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTY, effLength, ref TZtoTYab[1], ref TZtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoZ, effLength, ref TZtoZab[1], ref TZtoZab[2]);

                            lstr = "TZ Scale\t" + TZtoTZab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTX\t" + TZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoZ\t" + TZtoZab[1].ToString("E5") + "\r\n";

                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 1)
                                {
                                    TZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTZ[i] * TZtoTZab[i];
                                    TZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTXbyView[i] + TZtoTXab[i];
                                    TZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTYbyView[i] + TZtoTYab[i];
                                    TZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoZbyView[i] + TZtoTYab[i];

                                }
                                else
                                {
                                    TZtoTZab[i] = m__G.oCam[0].mFAL.mFZM.mScaleTZ[i];
                                    TZtoTXab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTXbyView[i];
                                    TZtoTYab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoTYbyView[i];
                                    TZtoZab[i] = m__G.oCam[0].mFAL.mFZM.mTZtoZbyView[i];


                                }
                            }

                        }
                        else
                        {
                            m__G.oCam[0].mFAL.mFZM.mcLP2ndPoly(sTZTZ, effLength, ref TZtoTZab);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTX, effLength, ref TZtoTXab[1], ref TZtoTXab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoTY, effLength, ref TZtoTYab[1], ref TZtoTYab[2]);
                            m__G.oCam[0].mFAL.mFZM.mcLP1stPoly(sTZtoZ, effLength, ref TZtoZab[1], ref TZtoZab[2]);

                            lstr = "TZ Scale\t" + TZtoTZab[0].ToString("E5") + ",\t" + TZtoTZab[1].ToString("E5") + ",\t" + TZtoTZab[2].ToString("E5") + "\r\n";
                            lstr += "TZtoTX\t" + TZtoTXab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoTYab[1].ToString("E5") + "\r\n";
                            lstr += "TZtoTY\t" + TZtoZab[1].ToString("E5") + "\r\n";
                        }


                        // ScaleNTheta 업데이트
                        if (isRemote)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + camID0 + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();

                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            // TZ
                            string[] strTZscaleLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[6] = TZtoTZab[0].ToString("E5") + "\t" + TZtoTZab[1].ToString("E5") + "\t" + TZtoTZab[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strTZscaleLine.Length; i++)
                                allLines[6] += strTZscaleLine[i];
                            // TZ TO TX
                            string[] strTZtoTXLine = allLines[27].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[27] = $"{0:E5}\t{TZtoTXab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoTXLine.Length; i++)
                                allLines[27] += strTZtoTXLine[i];
                            // TZ TO TY
                            string[] strTZtoTYLine = allLines[28].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[28] = $"{0:E5}\t{TZtoTYab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoTYLine.Length; i++)
                                allLines[28] += strTZtoTYLine[i];

                            // TZ TO Z
                            string[] strTZtoZLine = allLines[32].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[32] = $"{0:E5}\t{TZtoZab[1]:E5}\t{0:E5}\t//";
                            for (int i = 1; i < strTZtoZLine.Length; i++)
                                allLines[32] += strTZtoZLine[i];

                            StreamWriter wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                            AddVsnLog($"Scale factor updated in the file 'ScaleNTheta{camID0}'");
                        }
                        break;
                    }
            }

            if (!isRemote)
            {
                // Error Range
                stabilizedData = stabilizedDataList[0];
                effLength = stabilizedData.Count;

                double[] sEX = new double[effLength];
                double[] sEY = new double[effLength];
                double[] sEZ = new double[effLength];
                double[] sETX = new double[effLength];
                double[] sETY = new double[effLength];
                double[] sETZ = new double[effLength];

                for (int i = 0; i < effLength; i++)
                {
                    sEX[i] = stabilizedData[i][0] - stabilizedData[i][16];
                    sEY[i] = stabilizedData[i][1] - stabilizedData[i][17];
                    sEZ[i] = stabilizedData[i][2] - stabilizedData[i][18];
                    sETX[i] = stabilizedData[i][3] - stabilizedData[i][19];
                    sETY[i] = stabilizedData[i][4] - stabilizedData[i][20];
                    sETZ[i] = stabilizedData[i][5] - stabilizedData[i][21];
                }

                double eX = sEX.Max() - sEX.Min();
                double eY = sEY.Max() - sEY.Min();
                double eZ = sEZ.Max() - sEZ.Min();
                double eTX = sETX.Max() - sETX.Min();
                double eTY = sETY.Max() - sETY.Min();
                double eTZ = sETZ.Max() - sETZ.Min();

                AddVsnLog("ERROR Range");
                AddVsnLog("eX\teY\teZ\teTX\teTY\teTZ");
                AddVsnLog($"{eX:F5}\t{eY:F5}\t{eZ:F5}\t{eTX:F5}\t{eTY:F5}\t{eTZ:F5}");
            }
            else
            {
                LoadScaleNTheta();
            }

            AddVsnLog(lstr);
        }
        public void SaveMeasuredData(List<List<double[]>> stabilizedDataList, string fileName, string dirName)
        {
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;

            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            string resultPathName = m__G.m_RootDirectory + "\\Result\\" + dirName + "\\";
            if (!Directory.Exists(resultPathName))
                Directory.CreateDirectory(resultPathName);

            // 결과 파일 저장
            string strStabilizedFile = $"{resultPathName}StabilizedData_{camID0}_{fileName}_{DateTime.Now:yyMMdd_HHmmss}.csv";

            StreamWriter lwr = new StreamWriter(strStabilizedFile);

            for (int i = 0; i < stabilizedDataList.Count; i++)
            {
                List<double[]> stabilizedData = stabilizedDataList[i];
                if (stabilizedData != null)
                {
                    if(!m__G.m_bPrismCS)
                        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ"); //, ,pTY1_0,pTY2_0,pTY1_1,pTY2_1,pTY1_2,TY2_2,pZ_1,pZ2_2,pTX_0,pTX_1,pTX_2");
                    else
                        lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ,prismTX,prismTY,prismTZ,pprismTX,pprismTY,pprismTZ,epTX,epTY,epTZ,stdTX,stdTY,stdTZ,stdpTX,stdpTY,stdpTZ"); //, ,pTY1_0,pTY2_0,pTY1_1,pTY2_1,pTY1_2,TY2_2,pZ_1,pZ2_2,pTX_0,pTX_1,pTX_2");

                    for (int j = 0; j < stabilizedData.Count; j++)
                    {

                        string slstr = j.ToString() + "," +
                                   string.Join(",", stabilizedData[j].Take(22)
                                   .Select(x => x.ToString("F5"))) + ",";

                        slstr += (stabilizedData[j][0] - stabilizedData[j][16]).ToString("F5") + "," +
                                 (stabilizedData[j][1] - stabilizedData[j][17]).ToString("F5") + "," +
                                 (stabilizedData[j][2] - stabilizedData[j][18]).ToString("F5") + "," +
                                 (stabilizedData[j][3] - stabilizedData[j][19]).ToString("F5") + "," +
                                 (stabilizedData[j][4] - stabilizedData[j][20]).ToString("F5") + "," +
                                 (stabilizedData[j][5] - stabilizedData[j][21]).ToString("F5") + ",";

                        if (m__G.m_bPrismCS && stabilizedDataList.Count == 1)
                        {
                            double[] lProbePrismTXTYTZ = new double[3];
                            double[] lErrorPrismTXTYTZ = new double[3];
                            lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(stabilizedData[j][19], stabilizedData[j][20], stabilizedData[j][21], true, true);
                            lProbePrismTXTYTZ[1] = stabilizedData[j][19];   //  원래 있어야하는 코드

                            lErrorPrismTXTYTZ[0] = mPrismTXTYTZ[j][0] - lProbePrismTXTYTZ[0];
                            lErrorPrismTXTYTZ[1] = mPrismTXTYTZ[j][1] - lProbePrismTXTYTZ[1];  //  CSH_Prism_TY - Probe_TX
                            lErrorPrismTXTYTZ[2] = mPrismTXTYTZ[j][2] - lProbePrismTXTYTZ[2];

                            slstr += mPrismTXTYTZ[j][0].ToString("F5") + "," + mPrismTXTYTZ[j][1].ToString("F5") + "," + mPrismTXTYTZ[j][2].ToString("F5") + "," +
                                     lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                                     lErrorPrismTXTYTZ[0].ToString("F5") + "," + lErrorPrismTXTYTZ[1].ToString("F5") + "," + lErrorPrismTXTYTZ[2].ToString("F5");
                        }
                        lwr.WriteLine(slstr);
                    }
                }
            }

            lwr.Close();
        }
        public void AppendMeasuredData(List<List<double[]>> stabilizedDataList, string fileName, string dirName)
        {
            if (stabilizedDataList == null || stabilizedDataList.Count == 0) return;

            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            string resultPathName = m__G.m_RootDirectory + "\\Result\\" + dirName +"\\";
            if (!Directory.Exists(resultPathName))
                Directory.CreateDirectory(resultPathName);
            
            // 결과 파일 저장
            string strStabilizedFile = $"{resultPathName}StabilizedData_{camID0}_{fileName}.csv";

            string slstr = "";
            if (!File.Exists(strStabilizedFile))
            {
                if (!m__G.m_bPrismCS)
                    slstr += "#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ\r\n";
                else
                    slstr += "#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ,prismTX,prismTY,prismTZ,pprismTX,pprismTY,pprismTZ,epTX,epTY,epTZ\r\n";
            }

            double[] lProbePrismTXTYTZ = new double[3];
            double[] lErrorPrismTXTYTZ = new double[3];
            for (int i = 0; i < stabilizedDataList.Count; i++)
            {
                List<double[]> stabilizedData = stabilizedDataList[i];
                if (stabilizedData != null)
                {
                    for (int j = 0; j < stabilizedData.Count; j++)
                    {
                        slstr += j.ToString() + "," +
                                   string.Join(",", stabilizedData[j].Take(22)
                                   .Select(x => x.ToString("F5"))) + ",";

                        slstr += (stabilizedData[j][0] - stabilizedData[j][16]).ToString("F5") + "," +
                                 (stabilizedData[j][1] - stabilizedData[j][17]).ToString("F5") + "," +
                                 (stabilizedData[j][2] - stabilizedData[j][18]).ToString("F5") + "," +
                                 (stabilizedData[j][3] - stabilizedData[j][19]).ToString("F5") + "," +
                                 (stabilizedData[j][4] - stabilizedData[j][20]).ToString("F5") + "," +
                                 (stabilizedData[j][5] - stabilizedData[j][21]).ToString("F5") + ",";

                        if (m__G.m_bPrismCS)
                        {
                            lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(stabilizedData[j][19], stabilizedData[j][20], stabilizedData[j][21], true, true);
                            lProbePrismTXTYTZ[1] = stabilizedData[j][19];   //  원래 있어야하는 코드

                            lErrorPrismTXTYTZ[0] = mPrismTXTYTZ[j][0] - lProbePrismTXTYTZ[0];
                            lErrorPrismTXTYTZ[1] = mPrismTXTYTZ[j][1] - lProbePrismTXTYTZ[1];  //  CSH_Prism_TY - Probe_TX
                            lErrorPrismTXTYTZ[2] = mPrismTXTYTZ[j][2] - lProbePrismTXTYTZ[2];

                            slstr += mPrismTXTYTZ[j][0].ToString("F5") + "," + mPrismTXTYTZ[j][1].ToString("F5") + "," + mPrismTXTYTZ[j][2].ToString("F5") + "," +
                                     lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                                     lErrorPrismTXTYTZ[0].ToString("F5") + "," + lErrorPrismTXTYTZ[1].ToString("F5") + "," + lErrorPrismTXTYTZ[2].ToString("F5") + ",";

                            if(j == 0)
                            {
                                slstr += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",";
                            }
                            
                        }

                        slstr += "\r\n";
                    }
                }
            }
            File.AppendAllText(strStabilizedFile, slstr);

        }

        public bool mbMotorizedStage = false;

        private void cbMotorized_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMotorized.Checked)
            {
                mbMotorizedStage = true;
                cbZaxis.Show();
                cbRaxis.Show();
                cbTiltAxis.Show();
                btnSaveOrgPosition.Show();
            }
            else
            {
                mbMotorizedStage = false;
                cbZaxis.Hide();
                cbRaxis.Hide();
                cbTiltAxis.Hide();
                btnSaveOrgPosition.Hide();
            }
        }

        private void btnSaveOrgPosition_Click(object sender, EventArgs e)
        {
            MotorSetHome6D();
        }
        private void cbZaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbZaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbRaxis.Checked = false;
            }
        }

        private void cbRaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbZaxis.Checked = false;
            }
        }

        private void cbTiltAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTiltAxis.Checked)
            {
                cbRaxis.Checked = false;
                cbZaxis.Checked = false;
            }
        }

        private void btnGobackOrg_Click(object sender, EventArgs e)
        {
            MotorMoveHome6D();
            MotorSetPivot(0, 0, 0);
        }

        public struct VolumetricTP
        {
            public Point3d Pt;
            public bool bSubOn;
            public VolumetricTP(double x, double y, double z, bool subOn = true)
            {
                Pt.X = x; Pt.Y = y; Pt.Z = z;
                bSubOn = subOn;
            }
        }
        public struct VolumetricTP6D
        {
            public double X;
            public double Y;
            public double Z;
            public double TX;
            public double TY;
            public double TZ;
            public int pivotAxis;   //  0 는 측정, 1은 TX, 2 는 TY, 3 은 TZ
            //  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
            //  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
            //  pivotAxis 가 0 이면 이동후 측정한다.
            public VolumetricTP6D(double x, double y, double z, double tx, double ty, double tz, int pivot = 0)
            {
                X = x; Y = y; Z = z; TX = tx; TY = ty; TZ = tz;
                pivotAxis = pivot;

            }
        }
        public VolumetricTP[] mVMPts = null;
        public VolumetricTP[] mVMTPts = null;

        public int mAutoFullRange = 0;  // 0 ~ 100, 0 은 Auto 가 아닌 경우
        private void btnApplyVolumetricMeasure_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 0;
            ApplyVolumetricMeasureOld();
        }

        public VolumetricTP6D[] mVMPts6d = null;
        public void ApplyVolumetricMeasureOld()
        {
            double timeEst = 0;
            double[][] tpList = new double[15][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[11] { -1400, -1350, -1300, -1250, -1200, -1150, -1100, -1050, -1000, -950, 0 };

            //	Y		Z		T1		T2		T3		T4		T5	
            // 아예 회전이 없는 경우

            tpList[0] = new double[5] { -0.080, -0.070, 0, 0, 0 };
            tpList[1] = new double[5] { -0.040, -0.070, 0, 0, 0 };
            tpList[2] = new double[5] { 0, -0.070, 0, 0, 0 };
            tpList[3] = new double[5] { 0.040, -0.070, 0, 0, 0 };
            tpList[4] = new double[5] { 0.080, -0.070, 0, 0, 0 };

            tpList[5] = new double[5] { -0.0140, 0, 0, 0, 0 };
            tpList[6] = new double[5] { -0.070, 0, 0, 0, 0 };
            tpList[7] = new double[5] { 0, 0, 0, 0, 0 };
            tpList[8] = new double[5] { 0.070, 0, 0, 0, 0 };
            tpList[9] = new double[5] { 0.0140, 0, 0, 0, 0 };

            tpList[10] = new double[5] { -0.080, 0.070, 0, 0, 0 };
            tpList[11] = new double[5] { -0.040, 0.070, 0, 0, 0 };
            tpList[12] = new double[5] { 0, 0.070, 0, 0, 0 };
            tpList[13] = new double[5] { 0.040, 0.070, 0, 0, 0 };
            tpList[14] = new double[5] { 0.080, 0.070, 0, 0, 0 };

            //tpList[0] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[1] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[2] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[3] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[4] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[5] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[6] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[7] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[8] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[10] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[11] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[12] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[13] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[14] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 3;
            int tpListLast = tpListLength - 1;
            for (int i = 0; i < 11; i++)
            {
                //lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) / 3, tpList[0][0] / 3, tpList[0][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                //lmVMPts6d.Add(lpmid);
                //lpmid = new VolumetricTP6D(tXprev + 2 * (tpX[i] - tXprev) / 3, 2 * tpList[0][0] / 3, 2 * tpList[0][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                //lmVMPts6d.Add(lpmid);
                if (i == 0)
                {
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) / 3, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + 2 * (tpX[i] - tXprev) / 3, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) - 300, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                    lpmid = new VolumetricTP6D(tXprev + (tpX[i] - tXprev) - 10, 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                    lmVMPts6d.Add(lpmid);
                }

                //for (int j = 0; j < tpList.Length; j++)
                for (int j = 0; j < 1; j++)
                {
                    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0);
                    lmVMPts6d.Add(lp1);
                    if (j == 5 || j == 10)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    //VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    //lmVMPts6d.Add(lpmid1);
                    //lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    //lmVMPts6d.Add(lpmid1);
                    //for (int ti = 2; ti < tpListLength; ti++)
                    //{
                    //    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                    //    lmVMPts6d.Add(lp1);
                    //}
                    //VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    //lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////  TY
                    //////lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    //////lmVMPts6d.Add(lpmid2);
                    //////lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    //////lmVMPts6d.Add(lpmid2);
                    //////for (int ti = 2; ti < tpListLength; ti++)
                    //////{
                    //////    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                    //////    lmVMPts6d.Add(lp1);
                    //////}
                    //////VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    //////lmVMPts6d.Add(lpmid3);
                    ////////  End of TX
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ////////  TZ
                    //////lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    //////lmVMPts6d.Add(lpmid3);
                    //////lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    //////lmVMPts6d.Add(lpmid3);
                    //////for (int ti = 2; ti < tpListLength; ti++)
                    //////{
                    //////    VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                    //////    lmVMPts6d.Add(lp1);
                    //////}

                    if (j == 4 || j == 9 || j == 14)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                }
                //lpmid = new VolumetricTP6D((2 * tpX[i] + tpX[i + 1]) / 3, 0, 0, 0, 0, 0, -3);
                //tXprev = (2 * tpX[i] + tpX[i + 1]) / 3;
                //lmVMPts6d.Add(lpmid);
            }
            lpmid = new VolumetricTP6D(tXprev / 2, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text = (mVMPts6d.Length).ToString() + " points.";
        }

        public void ApplyVolumetricMeasure()
        {
            double timeEst = 0;
            double[][] tpList = new double[21][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[10] { -550, -1100, -1700, -1550, -1400, -700, 0, 700, 1400, 0 };
            //double[] tpX = new double[10] { 550, 1100, 1700, 1550, 1400, 700, 0, -700, -1400, 0 }; // 거꾸로

            //	Y		Z		T1		T2		T3		T4		T5	
            // y 거꾸로
            tpList[0] = new double[7] { 1000, -800, 0, 0, 0, 0, 0 };
            tpList[1] = new double[7] { 900, -750, 0, 0, 0, 0, 0 };
            tpList[2] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };
            tpList[3] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            tpList[4] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            tpList[5] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            tpList[6] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };

            tpList[7] = new double[7] { 1600, 0, 0, 0, 0, 0, 0 };
            tpList[8] = new double[7] { 1500, 0, 0, 0, 0, 0, 0 };
            tpList[9] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };
            tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            tpList[11] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            tpList[12] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            tpList[13] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };

            tpList[14] = new double[7] { 1000, 700, 0, 0, 0, 0, 0 };
            tpList[15] = new double[7] { 900, 700, 0, 0, 0, 0, 0 };
            tpList[16] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };
            tpList[17] = new double[7] { 400, 700, -76, -38, 0, 38, 76 };
            tpList[18] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            tpList[19] = new double[7] { -400, 700, -76, -38, 0, 38, 76 };
            tpList[20] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };

            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[1] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[2] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[3] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[4] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[5] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[6] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            //tpList[7] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[8] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[11] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[12] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            //tpList[13] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[14] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[15] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[16] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[17] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 7;
            int tpListLast = tpListLength - 1;

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    lmVMPts6d.Add(lpmid1);
                    lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid1);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TY
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    lmVMPts6d.Add(lpmid2);
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid3);
                    ////////  End of TY
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TZ
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    lmVMPts6d.Add(lpmid3);
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    lmVMPts6d.Add(lpmid3);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                        lmVMPts6d.Add(lp1);
                    }
                    ////////  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                }
                if (i < 8)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text += "\r\n" + (mVMPts6d.Length).ToString() + " points.";
        }
        public void ApplyVolumetricMeasure3step()
        {
            double timeEst = 0;
            double[][] tpList = new double[21][];

            ////////////////////////////////////////////////////////////////////////////////////////
            //  Hybrid Stage
            double[] tpX = new double[10] { -550, -1100, -1700, -1550, -1400, -700, 0, 700, 1400, 0 };

            //	Y		Z		T1		T2		T3		T4		T5	

            tpList[0] = new double[7] { -1000, -700, 0, 0, 0, 0, 0 };
            tpList[1] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            tpList[2] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            tpList[3] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            tpList[4] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            tpList[5] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            tpList[6] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            tpList[7] = new double[7] { -1600, 0, 0, 0, 0, 0, 0 };
            tpList[8] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            tpList[9] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            tpList[10] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            tpList[11] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            tpList[12] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            tpList[13] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            tpList[14] = new double[7] { -1000, 700, 0, 0, 0, 0, 0 };
            tpList[15] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            tpList[16] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            tpList[17] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            tpList[18] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            tpList[19] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            tpList[20] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };

            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[0] = new double[7] { -900, -700, 0, 0, 0, 0, 0 };
            //tpList[1] = new double[7] { -800, -700, -50, -25, 0, 25, 50 };
            //tpList[2] = new double[7] { -400, -700, -100, -50, 0, 50, 100 };
            //tpList[3] = new double[7] { 0, -700, -150, -75, 0, 75, 150 };
            //tpList[4] = new double[7] { 400, -700, -100, -50, 0, 50, 100 };
            //tpList[5] = new double[7] { 800, -700, -50, -25, 0, 25, 50 };

            //tpList[6] = new double[7] { -1500, 0, 0, 0, 0, 0, 0 };
            //tpList[7] = new double[7] { -1400, 0, -50, -25, 0, 25, 50 };
            //tpList[8] = new double[7] { -700, 0, -100, -50, 0, 50, 100 };
            //tpList[9] = new double[7] { 0, 0, -150, -75, 0, 75, 150 };
            //tpList[10] = new double[7] { 700, 0, -100, -50, 0, 50, 100 };
            //tpList[11] = new double[7] { 1400, 0, -50, -25, 0, 25, 50 };

            //tpList[12] = new double[7] { -900, 700, 0, 0, 0, 0, 0 };
            //tpList[13] = new double[7] { -800, 700, -50, -25, 0, 25, 50 };
            //tpList[14] = new double[7] { -400, 700, 76, 38, 0, -38, -76 };
            //tpList[15] = new double[7] { 0, 700, -100, -50, 0, 50, 100 };
            //tpList[16] = new double[7] { 400, 700, 76, 38, 0, -38, -76 };
            //tpList[17] = new double[7] { 800, 700, -50, -25, 0, 25, 50 };


            List<VolumetricTP6D> lmVMPts6d = new List<VolumetricTP6D>();
            VolumetricTP6D lpmid = new VolumetricTP6D();
            double tXprev = 0;
            int tpListLength = 7;
            int tpListLast = tpListLength - 1;

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //  TX
                    VolumetricTP6D lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 1);
                    lmVMPts6d.Add(lpmid1);
                    lpmid1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][2] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid1);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][ti], 0, 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], tpList[j][tpListLast] / 2, 0, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    //  End of TX
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -8);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TY
                    VolumetricTP6D lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 2);
                    lmVMPts6d.Add(lpmid2);
                    lpmid2 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][2] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid2);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][ti], 0);
                        lmVMPts6d.Add(lp1);
                    }
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, tpList[j][tpListLast] / 2, 0, -1);
                    lmVMPts6d.Add(lpmid3);
                    ////////  End of TY
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -7);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            for (int i = 0; i < 4; i++)
            {
                lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                lmVMPts6d.Add(lpmid);
            }

            for (int i = 4; i < 9; i++)
            {
                for (int j = 0; j < tpList.Length; j++)
                {
                    if (j % 7 == 0)
                    {
                        //  Common
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 크면 이동 후 pivot 을 변경해주고, 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 0, -1);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        lmVMPts6d.Add(lpmid);
                        continue;
                    }
                    else if (j % 7 == 1)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, -11);//  pivotAxis 가 0 보다 작으면 이동 후 측정결과는 저장하지 않는다.
                        continue;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //////////////  TZ
                    VolumetricTP6D lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, 0, 3);
                    lmVMPts6d.Add(lpmid3);
                    lpmid3 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][2] / 2, -1);
                    lmVMPts6d.Add(lpmid3);
                    for (int ti = 2; ti < tpListLength; ti++)
                    {
                        VolumetricTP6D lp1 = new VolumetricTP6D(tpX[i], tpList[j][0], tpList[j][1], 0, 0, tpList[j][ti]);
                        lmVMPts6d.Add(lp1);
                    }
                    ////////  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (j == 6 || j == 13 || j == 20)
                    {
                        lpmid = new VolumetricTP6D(tpX[i], 2 * tpList[j][0] / 3, 2 * tpList[j][1] / 3, 0, 0, 2 * tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], tpList[j][0] / 3, tpList[j][1] / 3, 0, 0, tpList[j][tpListLast] / 3, -1);
                        lmVMPts6d.Add(lpmid);
                        lpmid = new VolumetricTP6D(tpX[i], 0, 0, 0, 0, 0, -2);
                        lmVMPts6d.Add(lpmid);
                    }
                    //  End of TZ
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                if (i < 9)
                {
                    lpmid = new VolumetricTP6D((tpX[i] + tpX[i + 1]) / 2, 0, 0, 0, 0, 0, -3);
                    tXprev = (tpX[i] + tpX[i + 1]) / 2;
                    lmVMPts6d.Add(lpmid);
                }
            }
            lpmid = new VolumetricTP6D(2 * tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(tpX[8] / 3, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            lpmid = new VolumetricTP6D(0, 0, 0, 0, 0, 0, -1);
            lmVMPts6d.Add(lpmid);
            mVMPts6d = lmVMPts6d.ToArray();

            StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\" + "XYZ.csv");
            for (int i = 0; i < mVMPts6d.Length; i++)
            {
                wr.WriteLine(mVMPts6d[i].X.ToString("F3") + "," + mVMPts6d[i].Y.ToString("F3") + "," + mVMPts6d[i].Z.ToString("F3") + "," + mVMPts6d[i].TX.ToString("F3") + "," + mVMPts6d[i].TY.ToString("F3") + "," + mVMPts6d[i].TZ.ToString("F3") + "," + mVMPts6d[i].pivotAxis.ToString("F0"));
            }
            wr.Close();


            tbVsnLog.Text = (mVMPts6d.Length).ToString() + " points.";
        }

        private CancellationTokenSource volumetricCts;

        private  async void button14_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun) return;
            motorizedMeasurementRun = true;

            //tbVsnLog.Text = "⚠ Volumetric measurement is in progress.\r\nTo perform other tasks, please click 'Stop V-measure'.";
            //tbVsnLog.ForeColor = Color.Red;
            //tbVsnLog.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            //tbVsnLog.TextAlign = HorizontalAlignment.Center;

            volumetricCts = new CancellationTokenSource();
            var token = volumetricCts.Token;

            try
            {
                await Task.Run(() => VolumetricMeasure(), token);

                //while (!token.IsCancellationRequested)
                //{
                //    await Task.Run(() => VolumetricMeasure(), token);
                //    await Task.Delay(600000, token);   // 7200000 : 2시간
                //}
            }
            catch (Exception ex)
            { 
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;

                //tbVsnLog.Text = "✅ Volumetric measurement has been stopped.";
                //tbVsnLog.ForeColor = Color.LemonChiffon;
                //tbVsnLog.Font = new Font("맑은 고딕", 9, FontStyle.Regular);
                //tbVsnLog.TextAlign = HorizontalAlignment.Left;
            }            
        }

        public Point3d[] mHexapodPivots = new Point3d[3];
        public Point2d mHexapodHplane = new Point2d();  //  TX by degree, TY by degree
        public Point3d[] mPivoterror = new Point3d[3];

        public struct Pivot18D // 앞에 3개는 pivot의 실제 값 뒤에 3개는 dx, dy, dz
        {
            public Point3d Position;
            public Point3d Delta;
            public Point3d First1;
            public Point3d First2;
            public Point3d Second1;
            public Point3d Second2;

            public double[] ToArray()
            {
                return new double[]
                {
                    Position.X, Position.Y, Position.Z,
                    Delta.X, Delta.Y, Delta.Z,
                    First1.X, First1.Y, First1.Z,
                    First2.X, First2.Y, First2.Z,
                    Second1.X, Second1.Y, Second1.Z,
                    Second2.X, Second2.Y, Second2.Z
                };
            }
        }

        public List<Pivot18D> mXPivots = new List<Pivot18D>();
        public List<Pivot18D> mYPivots = new List<Pivot18D>();
        public List<Pivot18D> mZPivots = new List<Pivot18D>();
        public void InitializeHexpodPivot()
        {
            mHexapodPivots[0] = new Point3d(0, 1.439438857, -14.10421867); //  TX PIVOT
            mHexapodPivots[1] = new Point3d(-0.019249611, 0, -14.03564737); //  TY PIVOT
            mHexapodPivots[2] = new Point3d(0.096426953, 0.384410326, 0);  //  TZ PIVOT
        }
        public void LoadHexpodPivots()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\Pivot" + camID0 + ".txt";
            if (!File.Exists(pivotFile))
                return;

            StreamReader rr = new StreamReader(pivotFile);
            string lstr = rr.ReadToEnd();
            rr.Close();

            string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < 3; i++)
            {
                string[] elements = allLines[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mHexapodPivots[i].X = double.Parse(elements[0]);
                mHexapodPivots[i].Y = double.Parse(elements[1]);
                mHexapodPivots[i].Z = double.Parse(elements[2]);
            }
            if (allLines.Length < 4)
            {
                mCommonPivot.X = (mHexapodPivots[1].X + mHexapodPivots[2].X) / 2;
                mCommonPivot.Y = (mHexapodPivots[2].Y + mHexapodPivots[0].Y) / 2;
                mCommonPivot.Z = (mHexapodPivots[0].Z + mHexapodPivots[1].Z) / 2;
                return;
            }

            string[] commonElements = allLines[4].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mCommonPivot.X = double.Parse(commonElements[0]);
            mCommonPivot.Y = double.Parse(commonElements[1]);
            mCommonPivot.Z = double.Parse(commonElements[2]);
        }

        public void VolumetricMeasure()
        {            
            int fullCnt = 0;
            LoadScaleNTheta();

            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            FindCSHorg();
            for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
            {
                if (motorizedMeasurementAbort) return;
                AddVsnLog($"Start to find {pivotAxis} pivot.");
                FindPivot(pivotAxis);
            }
            SavePivots();
            FindCSHorg(true);
            FindFidorg();
            SingleFindMark(true);
            SaveOQCCondition();

            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            mPrismTXTYTZ.Clear();
            mStdevTXTYTZ.Clear();

            mAutoCalibrationIndex = 0;
            int i = 0;
            int imax = mVMPts6d.Length;
            bool IsSave = true;
            bool bPivotChanged = false;
            int LastPivot = 0;

            AddVsnLog("Test Points : " + imax.ToString());

            while (true)
            {
                if (motorizedMeasurementAbort) break;

                MotorXYZ(mVMPts6d[i].X + mCSHorg.X,
                         mVMPts6d[i].Y + mCSHorg.Y,
                         mVMPts6d[i].Z + mCSHorg.Z);

                if (mVMPts6d[i].TX == 0 && mVMPts6d[i].TY == 0 && mVMPts6d[i].TZ == 0)
                {
                    //  (TX, TY, TZ) == (0, 0,0) 이면 Calibration 때와 같은 조건이 되도록 만든다.
                    HexapodRotate(0, 0, 0);
                    MotorSetPivot(0, 0, 0);
                    bPivotChanged = true;
                }
                else
                {
                    if (bPivotChanged == true && LastPivot > 0)
                    {
                        HexapodRotate(0, 0, 0);
                        MotorSetPivot(mHexapodPivots[LastPivot - 1].X, mHexapodPivots[LastPivot - 1].Y, mHexapodPivots[LastPivot - 1].Z);
                        bPivotChanged = false;
                    }
                    HexapodRotate(mVMPts6d[i].TX, mVMPts6d[i].TY, mVMPts6d[i].TZ);
                }
                Thread.Sleep(600);

                IsSave = mVMPts6d[i].pivotAxis == 0 ? true : false;

                if (mVMPts6d[i].pivotAxis > -10)
                {
                    if (mVMPts6d[i].X == 0 && mVMPts6d[i].Y == 0 && mVMPts6d[i].Z == 0)
                    {
                        Console.WriteLine("초기위치");
                    }
                    SingleFindMark(IsSave);
                }

                if (IsSave)
                {
                    fullCnt++;
                }
                else
                {
                    //  다음은 Hexapod 에서만 유효
                    switch (mVMPts6d[i].pivotAxis)
                    {
                        case 1: //  TX pivot
                            HexapodRotate(0, 0, 0);
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);
                            LastPivot = 1;
                            break;
                        case 2: //  TY pivot
                            HexapodRotate(0, 0, 0);
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                            LastPivot = 2;
                            break;
                        case 3: //  TZ pivot
                            HexapodRotate(0, 0, 0);
                            Thread.Sleep(200);
                            MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                            LastPivot = 3;
                            break;
                        default:
                            break;
                    }
                }

                i++;
                if (i >= imax) break;
            }

            string fileName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            double[][] stabilizedData = mCalibrationFullData.ToArray();  //  um, min

            double[] lProbePrismTXTYTZ = new double[3];
            double[] lErrorPrismTXTYTZ = new double[3];

            StreamWriter lwr = null;
            DateTime lnow = DateTime.Now;
            try
            {
                lwr = new StreamWriter(fileName + "VolumetrixMeasure" + fullCnt.ToString() + "_" + m__G.mCamID0 + "_"+lnow.ToString("yyMMddHH") + ".csv");
            }
            catch (Exception e)
            {
                lwr = new StreamWriter(fileName + "VolumetrixMeasure" + fullCnt.ToString() + "_" + m__G.mCamID0 + "_" + lnow.ToString("hhmmss") + ".csv");
            }

            if (!m__G.m_bPrismCS)
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ"); 
            else
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,eX,eY,eZ,eTX,eTY,eTZ,prismTX,prismTY,prismTZ,pprismTX,pprismTY,pprismTZ,epTX,epTY,epTZ,stdTX,stdTY,stdTZ,stdpTX,stdpTY,stdpTZ"); //, ,pTY1_0,pTY2_0,pTY1_1,pTY2_1,pTY1_2,TY2_2,pZ_1,pZ2_2,pTX_0,pTX_1,pTX_2");



            for (i = 0; i < fullCnt; i++)
            {
                string slstr = i.ToString() + ",";

                for (int j = 0; j < 22; j++)
                {
                    slstr += stabilizedData[i][j].ToString("F5") + ",";  //  stablizedData[i][19 ~ 21] Probe TX, TY, TZ 가 radian 으로 제공될 때
                }

                // 오차
                slstr += (stabilizedData[i][0] - stabilizedData[i][16]).ToString("F5") + "," +
                         (stabilizedData[i][1] - stabilizedData[i][17]).ToString("F5") + "," +
                         (stabilizedData[i][2] - stabilizedData[i][18]).ToString("F5") + "," +
                         (stabilizedData[i][3] - stabilizedData[i][19]).ToString("F5") + "," +
                         (stabilizedData[i][4] - stabilizedData[i][20]).ToString("F5") + "," +
                         (stabilizedData[i][5] - stabilizedData[i][21]).ToString("F5") + ",";

                if (m__G.m_bPrismCS)
                {
                    lProbePrismTXTYTZ = m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(stabilizedData[i][19], stabilizedData[i][20], stabilizedData[i][21], true, true);
                    lProbePrismTXTYTZ[1] = stabilizedData[i][19];   //  PrismTY == ProbeTX

                    lErrorPrismTXTYTZ[0] = mPrismTXTYTZ[i][0] - lProbePrismTXTYTZ[0];
                    lErrorPrismTXTYTZ[1] = mPrismTXTYTZ[i][1] - lProbePrismTXTYTZ[1];  //  CSH_Prism_TY - Probe_TX
                    lErrorPrismTXTYTZ[2] = mPrismTXTYTZ[i][2] - lProbePrismTXTYTZ[2];

                    slstr += mPrismTXTYTZ[i][0].ToString("F5") + "," + mPrismTXTYTZ[i][1].ToString("F5") + "," + mPrismTXTYTZ[i][2].ToString("F5") + "," +
                             lProbePrismTXTYTZ[0].ToString("F5") + "," + lProbePrismTXTYTZ[1].ToString("F5") + "," + lProbePrismTXTYTZ[2].ToString("F5") + "," +
                             lErrorPrismTXTYTZ[0].ToString("F5") + "," + lErrorPrismTXTYTZ[1].ToString("F5") + "," + lErrorPrismTXTYTZ[2].ToString("F5") + "," +
                             mStdevTXTYTZ[i][0].ToString("F5") + "," + mStdevTXTYTZ[i][1].ToString("F5") + "," + mStdevTXTYTZ[i][2].ToString("F5") + "," +
                             mStdevTXTYTZ[i][3].ToString("F5") + "," + mStdevTXTYTZ[i][4].ToString("F5") + "," + mStdevTXTYTZ[i][5].ToString("F5") + "," ;
                }

                lwr.WriteLine(slstr);
            }
            lwr.Close();
        }

        public void ClearHexapodPivots()
        {
            mHexapodPivots[0] = new Point3d(0, 0, -14000);
            mHexapodPivots[1] = new Point3d(0, 0, -14000);
            mHexapodPivots[2] = new Point3d(0, 0, -14000);
        }

        public Point3d FindPivot(Axis axis)
        {

            mHexapodPivots[(int)axis - 3] = new Point3d(0, 0, -14000);
            if (axis == Axis.TY)
                mHexapodPivots[(int)axis - 3] = new Point3d(0, 0, mHexapodPivots[0].Z);
            if (axis == Axis.TZ)
                mHexapodPivots[(int)axis - 3] = new Point3d(mHexapodPivots[1].X, mHexapodPivots[0].Y, -14000);

            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorMoveHome6D();
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);
            Thread.Sleep(500);
            SingleFindMark(true);

            Point3d lPivot = new Point3d();

            double[] orgPos = MotorCurPos6D();
            double[] tagetPos;

            double angle;
            double dx = 0;
            double dy = 0;
            double dz = 0;

            double[] Xref = new double[2];
            double[] X0 = new double[2];
            double[] X1 = new double[2];
            double[,] RX = new double[2, 2];
            double[] RX0_X1 = new double[2];
            double[] resPivot = new double[2];

            int itrCnt = 0;

            switch (axis) //  X axis
            {
                case Axis.TX:


                    mPivoterror[0] = new Point3d(0, 0, 0); //differential reset for Tx

                    //tagetPos = new double[5] { -155, -150, 0, 150, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.

                    mXPivots.Clear();

                    while (itrCnt++ < 10)
                    {
                        if (motorizedMeasurementAbort)
                        {
                            AddVsnLog("Pivot Finding Stopped by User");
                            break;
                        }
                        MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);   // Pivot (0,0,-14000)에서 시작  
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();

                        if (itrCnt < 2)
                        {
                            // 260320
                            // 284 Head: Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
                            // 289 Head: Probe 200까지 측정 가능 (초점 거리 차이로 스테이지의 Z위치(Head)가 284보다 아래라  Probe TY1 TY2 Stroke 이내)
                            tagetPos = new double[5] { -155, -150, 0, 150, 0 };   //  min  
                        }
                        else
                        {
                            tagetPos = new double[5] { -165, -160, 0, 160, 0 };
                        }


                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbsAxis(Axis.TX, orgPos[3] + tagetPos[i]);
                            if (i > 0 && i < 4)
                            {
                                Thread.Sleep(600);
                            }
                            SingleFindMark(true);
                            // 임시 디버깅 저장
                            string imgFilePath = $"C:\\CSHTest\\Result\\Pivot\\{Axis.TX}\\{itrCnt}_{tagetPos[i]}.bmp";
                            string dir = Path.GetDirectoryName(imgFilePath);

                            if (!string.IsNullOrEmpty(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            m__G.oCam[0].SaveSourceImage(0, imgFilePath);
                        }

                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[1][1];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[1][2];

                        var row1 = mCalibrationFullData[1];
                        var p1 = new Point3d(row1[0], row1[1], row1[2]);
                        var p1t = new Point3d(row1[3], row1[4], row1[5]);
                        var row2 = mCalibrationFullData[3];
                        var p2 = new Point3d(row2[0], row2[1], row2[2]);
                        var p2t = new Point3d(row2[3], row2[4], row2[5]);


                        mXPivots.Add(new Pivot18D
                        {
                            Position = mHexapodPivots[0],
                            Delta = new Point3d(dx, dy, dz),
                            First1 = p1,
                            First2 = p1t,
                            Second1 = p2,
                            Second2 = p2t
                        });

                        AddVsnLog($"itr:{itrCnt}\tdy:{dy:F3}\tdz:{dz:F3}");
                        if (Math.Abs(dy) < 0.2 && Math.Abs(dz) < 0.2)
                        {
                            mPivoterror[0].Y = dy;
                            mPivoterror[0].Z = dz;
                            break;
                        }
                        else if (itrCnt == 10 && (Math.Abs(dy) >= 0.1 || Math.Abs(dz) >= 0.1)) // 260315 Pivot이 10회 안에도 수렴안하면 test stop
                        {
                            motorizedMeasurementAbort = true;
                            AddVsnLog($"dx:{dx:F3}\tdz:{dz:F3}\tPivot Finding Failed");

                            break;
                        }
                        ///////////////////////////////////////////////////////////////////
                        ///
                        Xref[0] = mCalibrationFullData[2][1];
                        Xref[1] = -mCalibrationFullData[2][2];

                        // Pivto XA 계산 
                        angle = mCalibrationFullData[3][19] - mCalibrationFullData[1][19];  //  min // Probe TX


                        X0[0] = mCalibrationFullData[1][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um

                        X1[0] = mCalibrationFullData[3][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  2차원 회전행렬
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - X0[1]);

                        lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - Xref[0]);
                        lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - Xref[1]);

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // Pivto XA 계산 
                        angle = mCalibrationFullData[2][19] - mCalibrationFullData[1][19];  //  min

                        X0[0] = mCalibrationFullData[1][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um
                        X1[0] = mCalibrationFullData[2][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[2][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - X0[1]);

                        lPivot.Y += mHexapodPivots[0].Y - (resPivot[0] - Xref[0]);
                        lPivot.Z += mHexapodPivots[0].Z + (resPivot[1] - Xref[1]);

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // Pivto XA 계산 
                        angle = mCalibrationFullData[3][19] - mCalibrationFullData[2][19];  //  min

                        X0[0] = mCalibrationFullData[2][1]; //  Y pos um
                        X0[1] = -mCalibrationFullData[2][2]; //  Z pos um

                        X1[0] = mCalibrationFullData[3][1]; //  Y pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.Y = mHexapodPivots[0].Y - (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[0].Z + (resPivot[1] - X0[1]);

                        lPivot.Y += mHexapodPivots[0].Y - (resPivot[0] - Xref[0]);
                        lPivot.Z += mHexapodPivots[0].Z + (resPivot[1] - Xref[1]);

                        mHexapodPivots[0] = new Point3d(0, lPivot.Y / 3, lPivot.Z / 3); // 250616
                        mHexapodPivots[0].X = Math.Abs(mHexapodPivots[0].X) > 4000 ? 4000 * Math.Sign(mHexapodPivots[0].X) : mHexapodPivots[0].X;
                        mHexapodPivots[0].Y = Math.Abs(mHexapodPivots[0].Y) > 4000 ? 4000 * Math.Sign(mHexapodPivots[0].Y) : mHexapodPivots[0].Y;

                        ///////////////////////////////////////////////////////////////////
                        //
                        //  출발 피봇이 가까와지니 오차가 줄어들기는 함.
                        //  1회차에서 
                        //  Y 는 7.6% 과보정
                        //  Z 는 8.1% 미보정
                    }
                    break;
                case Axis.TY:

                    mPivoterror[1] = new Point3d(0, 0, 0); //differential reset for Ty

                    tagetPos = new double[5] { -235, -230, 0, 230, 0 };   //  min
                    mYPivots.Clear();

                    while (itrCnt++ < 10)
                    {
                        if (motorizedMeasurementAbort)
                        {
                            AddVsnLog("Pivot Finding Stopped by User");
                            break;
                        }
                        MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();

                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbsAxis(Axis.TY, orgPos[4] + tagetPos[i]);
                            if (i > 0 && i < 4)
                            {
                                Thread.Sleep(600);
                            }
                            SingleFindMark(true);
                            string imgFilePath = $"C:\\CSHTest\\Result\\Pivot\\{Axis.TY}\\{itrCnt}_{tagetPos[i]}.bmp";
                            string dir = Path.GetDirectoryName(imgFilePath);

                            if (!string.IsNullOrEmpty(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            m__G.oCam[0].SaveSourceImage(0, imgFilePath);
                        }

                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[1][0];
                        dz = mCalibrationFullData[3][2] - mCalibrationFullData[1][2];

                        var row1 = mCalibrationFullData[1];
                        var p1 = new Point3d(row1[0], row1[1], row1[2]);
                        var p1t = new Point3d(row1[3], row1[4], row1[5]);
                        var row2 = mCalibrationFullData[3];
                        var p2 = new Point3d(row2[0], row2[1], row2[2]);
                        var p2t = new Point3d(row2[3], row2[4], row2[5]);


                        mYPivots.Add(new Pivot18D
                        {
                            Position = mHexapodPivots[1],
                            Delta = new Point3d(dx, dy, dz),
                            First1 = p1,
                            First2 = p1t,
                            Second1 = p2,
                            Second2 = p2t
                        });


                        AddVsnLog($"itr:{itrCnt}\tdx:{dx:F3}\tdz:{dz:F3}");
                        if (Math.Abs(dx) < 0.2 && Math.Abs(dz) < 0.2)
                        {
                            mPivoterror[1].X = dx;
                            mPivoterror[1].Z = dz;
                            break;
                        }
                        else if (itrCnt == 10 && (Math.Abs(dx) >= 0.1 || Math.Abs(dz) >= 0.1)) // 260315 Pivot이 10회 안에도 수렴안하면 test stop
                        {
                            motorizedMeasurementAbort = true;
                            AddVsnLog($"dx:{dx:F3}\tdz:{dz:F3}\tPivot Finding Failed");

                            break;
                        }

                        ///////////////////////////////////////////////////////////////////
                        ///
                        Xref[0] = -mCalibrationFullData[2][0];
                        Xref[1] = -mCalibrationFullData[2][2];

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[3][20] - mCalibrationFullData[1][20];  //  min

                        X0[0] = -mCalibrationFullData[1][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[1].Z + (resPivot[1] - X0[1]);

                        lPivot.X = mHexapodPivots[1].X + (resPivot[0] - Xref[0]);
                        lPivot.Z = mHexapodPivots[1].Z + (resPivot[1] - Xref[1]);

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[2][20] - mCalibrationFullData[1][20];  //  min

                        X0[0] = -mCalibrationFullData[1][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[1][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[2][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[2][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[1].Z + (resPivot[1] - X0[1]);

                        lPivot.X += mHexapodPivots[1].X + (resPivot[0] - Xref[0]);
                        lPivot.Z += mHexapodPivots[1].Z + (resPivot[1] - Xref[1]);

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[3][20] - mCalibrationFullData[2][20];  //  min

                        X0[0] = -mCalibrationFullData[2][0]; //  X pos um
                        X0[1] = -mCalibrationFullData[2][2]; //  Z pos um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos um
                        X1[1] = -mCalibrationFullData[3][2]; //  Z pos um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[1].X + (resPivot[0] - X0[0]);
                        //lPivot.Z = mHexapodPivots[1].Z + (resPivot[1] - X0[1]);

                        lPivot.X += mHexapodPivots[1].X + (resPivot[0] - Xref[0]);
                        lPivot.Z += mHexapodPivots[1].Z + (resPivot[1] - Xref[1]);

                        mHexapodPivots[1] = new Point3d(lPivot.X / 3, 0, lPivot.Z / 3);
                        mHexapodPivots[1].X = Math.Abs(mHexapodPivots[1].X) > 4000 ? 4000 * Math.Sign(mHexapodPivots[1].X) : mHexapodPivots[1].X;
                        mHexapodPivots[1].Y = Math.Abs(mHexapodPivots[1].Y) > 4000 ? 4000 * Math.Sign(mHexapodPivots[1].Y) : mHexapodPivots[1].Y;
                    }
                    break;
                case Axis.TZ:

                    mPivoterror[2] = new Point3d(0, 0, 0); //differential reset for Tz
                    tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min
                    mZPivots.Clear();


                    while (itrCnt++ < 10)
                    {
                        if (motorizedMeasurementAbort)
                        {
                            AddVsnLog("Pivot Finding Stopped by User");
                            break;
                        }
                        MotorSetPivot(mHexapodPivots[2].X, mHexapodPivots[2].Y, mHexapodPivots[2].Z);
                        mCalibrationFullData.Clear();
                        mGageFullData.Clear();

                        for (int i = 0; i < 5; i++)
                        {
                            MotorMoveAbsAxis(Axis.TZ, orgPos[5] + tagetPos[i]);
                            if (i > 0 && i < 4)
                            {
                                Thread.Sleep(600);
                            }
                            SingleFindMark(true);
                            string imgFilePath = $"C:\\CSHTest\\Result\\Pivot\\{Axis.TZ}\\{itrCnt}_{tagetPos[i]}.bmp";
                            string dir = Path.GetDirectoryName(imgFilePath);

                            if (!string.IsNullOrEmpty(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            m__G.oCam[0].SaveSourceImage(0, imgFilePath);

                        }

                        dx = mCalibrationFullData[3][0] - mCalibrationFullData[1][0];
                        dy = mCalibrationFullData[3][1] - mCalibrationFullData[1][1];

                        var row1 = mCalibrationFullData[1];
                        var p1 = new Point3d(row1[0], row1[1], row1[2]);
                        var p1t = new Point3d(row1[3], row1[4], row1[5]);
                        var row2 = mCalibrationFullData[3];
                        var p2 = new Point3d(row2[0], row2[1], row2[2]);
                        var p2t = new Point3d(row2[3], row2[4], row2[5]);


                        mZPivots.Add(new Pivot18D
                        {
                            Position = mHexapodPivots[2],
                            Delta = new Point3d(dx, dy, dz),
                            First1 = p1,
                            First2 = p1t,
                            Second1 = p2,
                            Second2 = p2t
                        });


                        AddVsnLog($"itr:{itrCnt}\tdx:{dx:F3}\tdy:{dy:F3}");
                        if (Math.Abs(dx) < 0.2 && Math.Abs(dy) < 0.2)
                        {
                            mPivoterror[2].X = dx;
                            mPivoterror[2].Y = dy;
                            break;
                        }
                        else if (itrCnt == 10 && (Math.Abs(dx) >= 0.1 || Math.Abs(dy) >= 0.1)) // 260315 Pivot이 10회 안에도 수렴안하면 test stop
                        {
                            motorizedMeasurementAbort = true;
                            AddVsnLog($"dx:{dx:F3}\tdz:{dz:F3}\tPivot Finding Failed");

                            break;
                        }

                        ///////////////////////////////////////////////////////////////////
                        ///

                        Xref[0] = -mCalibrationFullData[2][0];
                        Xref[1] = mCalibrationFullData[2][1];

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[3][21] - mCalibrationFullData[1][21];  //  min

                        X0[0] = -mCalibrationFullData[1][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[1][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[3][1]; //  Y pos   um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        //lPivot.Y = mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.

                        lPivot.X = mHexapodPivots[2].X + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                        lPivot.Y = mHexapodPivots[2].Y - (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[2][21] - mCalibrationFullData[1][21];  //  min

                        X0[0] = -mCalibrationFullData[1][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[1][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[2][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[2][1]; //  Y pos   um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        //lPivot.Y = mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.

                        lPivot.X += mHexapodPivots[2].X + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                        lPivot.Y += mHexapodPivots[2].Y - (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                        ///////////////////////////////////////////////////////////////////
                        ///

                        // XA 계산 및 출력
                        angle = mCalibrationFullData[3][21] - mCalibrationFullData[2][21];  //  min

                        X0[0] = -mCalibrationFullData[2][0]; //  X pos   um
                        X0[1] = mCalibrationFullData[2][1]; //  Y pos   um

                        X1[0] = -mCalibrationFullData[3][0]; //  X pos   um
                        X1[1] = mCalibrationFullData[3][1]; //  Y pos   um

                        m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                        RX0_X1[0] -= X1[0];
                        RX0_X1[1] -= X1[1];

                        RX[0, 0] -= 1;
                        RX[1, 1] -= 1;

                        m__G.mFAL.mFZM.InverseU(ref RX, 2);
                        m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                        //lPivot.X = mHexapodPivots[2].X + (resPivot[0] - X0[0]); //  dy 에 영향 준다.
                        //lPivot.Y = mHexapodPivots[2].Y - (resPivot[1] - X0[1]); //  dx 에 영향 준다.

                        lPivot.X += mHexapodPivots[2].X + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                        lPivot.Y += mHexapodPivots[2].Y - (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                        mHexapodPivots[2] = new Point3d(lPivot.X / 3, lPivot.Y / 3, -14000);
                        mHexapodPivots[2].X = Math.Abs(mHexapodPivots[2].X) > 4000 ? 4000 * Math.Sign(mHexapodPivots[2].X) : mHexapodPivots[2].X;
                        mHexapodPivots[2].Y = Math.Abs(mHexapodPivots[2].Y) > 4000 ? 4000 * Math.Sign(mHexapodPivots[2].Y) : mHexapodPivots[2].Y;

                        ///////////////////////////////////////////////////////////////////
                    }
                    break;
                default:
                    break;
            }

            lPivot = mHexapodPivots[(int)axis - 3];

            // 피봇 초기화
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            return lPivot;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementAbort) return;
            motorizedMeasurementAbort = true;
            volumetricCts?.Cancel();
                    //tbVsnLog.Text = "✅ Volumetric measurement is stopping...";
            //tbVsnLog.ForeColor = Color.LemonChiffon;
                  //tbVsnLog.Font = new Font("맑은 고딕", 9, FontStyle.Regular);
            //tbVsnLog.TextAlign = HorizontalAlignment.Left;
        }

        public VolumetricTP[] Generate3dTrajectory(double pitchX, double pitchY, double pitchZ, double xFullRange, double yFullRange, double zFullRange)
        {
            //  xFullRange ~ yFullRange 이고 pitchX ~ pitchY 인 경우
            //  회오리 궤적

            List<VolumetricTP> pList = new List<VolumetricTP>();
            int m = (int)((zFullRange / 2) / pitchZ);
            int nxMax = (int)((xFullRange / 2) / pitchX);
            int nyMax = (int)((yFullRange / 2) / pitchY);

            double residueX = xFullRange - 2 * nxMax * pitchX;
            int yinterval = (int)(yFullRange / pitchY + 0.01);

            for (int j = -m; j <= m; j++)
            {
                VolumetricTP p0 = new VolumetricTP(0, 0, j * pitchZ);
                pList.Add(p0);
                if (mAutoFullRange > 0)
                {
                    pitchY = 2 * (1900 - (Math.Abs(p0.Pt.Z / 100) - 240) / 0.8391 - 120) / yinterval;
                }

                for (int k = 1; k <= nxMax; k++)
                {
                    int nx = k;
                    int ny = k;

                    for (int i = 0; i < ny; i++)
                    {
                        VolumetricTP p = new VolumetricTP(nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = nx; i > -nx; i--)
                    {
                        VolumetricTP p = new VolumetricTP(i * pitchX, ny * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = ny; i > -ny; i--)
                    {
                        VolumetricTP p = new VolumetricTP(-nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = -nx; i < nx; i++)
                    {
                        VolumetricTP p = new VolumetricTP(i * pitchX, -ny * pitchY, j * pitchZ);
                        pList.Add(p);
                    }

                    for (int i = -ny; i < 0; i++)
                    {
                        VolumetricTP p = new VolumetricTP(nx * pitchX, i * pitchY, j * pitchZ);
                        pList.Add(p);

                    }
                }
                VolumetricTP pOld = pList[pList.Count - 1];
                VolumetricTP p1 = new VolumetricTP(0.75 * pOld.Pt.X, 0.75 * pOld.Pt.Y, 0.75 * pOld.Pt.Z + pitchZ / 4, false);
                VolumetricTP p2 = new VolumetricTP(0.50 * pOld.Pt.X, 0.50 * pOld.Pt.Y, 0.50 * pOld.Pt.Z + pitchZ / 2, false);
                VolumetricTP p3 = new VolumetricTP(0.25 * pOld.Pt.X, 0.25 * pOld.Pt.Y, 0.25 * pOld.Pt.Z + 0.75 * pitchZ, false);
                pList.Add(p1);
                pList.Add(p2);
                pList.Add(p3);
            }

            //double[] x = new double[pList.Count];
            //double[] y = new double[pList.Count];
            //double[] z = new double[pList.Count];
            //for (int i = 0; i < pList.Count; i++)
            //{
            //    x[i] = pList[i].X;
            //    y[i] = pList[i].Y;
            //    z[i] = pList[i].Z;
            //}
            return pList.ToArray();
        }

        private void btnMoveTo_Click(object sender, EventArgs e)
        {
            double x = double.Parse(tbXrange.Text);
            double y = double.Parse(tbYrange.Text);
            double z = double.Parse(tbZrange.Text);
            double tx = double.Parse(tbTXrange.Text);
            double ty = double.Parse(tbTYrange.Text);
            double tz = double.Parse(tbTZrange.Text);
            MotorSetPivot(0, 0, 0);
            if (rbFromOrg.Checked)
            {
                MotorMoveHome6D();
            }
            else
            {
                if (LoadOQCcondition())
                {
                    MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
                }
                else
                {
                    MessageBox.Show("OQCcondition File could not be loaded.");
                }
            }

            Thread.Sleep(100);

            double[] orgPos = MotorCurPos6D();

            MotorXYZ(x + orgPos[0], y + orgPos[1], z + orgPos[2]);
            HexapodRotate(tx + orgPos[3], ty + orgPos[4], tz + orgPos[5]);

            //MotorMoveAbs6D(x + orgPos[0],
            //               y + orgPos[1],
            //               z + orgPos[2],
            //            tx + orgPos[3],
            //            ty + orgPos[4],
            //            tz + orgPos[5]);
        }

        private void rbCalTX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "160"; //210
        }

        private void rbCalTY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "200";//"240";
        }

        private void rbCalTZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "200";//240
        }

        private void rbCalZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1700";  // 1750
        }

        private void rbCalX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";   // 1900
        }

        private void rbCalY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";   // 1900
        }

        private void button16_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 95;
            //ApplyVolumetricMeasureOld();

            ApplyVolumetricMeasure();    // TXTYTZ -> TXTYTZ -> ... 

            // ApplyVolumetricMeasure3step();  //  TX -> TY -> TZ
        }

        //private void rbCalEastView_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (rbCalEastView.Checked)
        //    {
        //        lblYMaxStroke.Visible = true;
        //        lblZMaxStroke.Visible = true;
        //        tbZMaxStroke.Visible = true;
        //        tbMaxStroke.Text = "1900";
        //        tbZMaxStroke.Text = "1700";
        //    }
        //    else
        //    {
        //        lblYMaxStroke.Visible = false;
        //        lblZMaxStroke.Visible = false;
        //        tbZMaxStroke.Visible = false;
        //    }
        //}

        public Point3d mCommonPivot = new Point3d(0, 0, 0);


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        ///    OQC Procedure
        ///    
        /// </summary>

        public VolumetricTP6D mPorg = new VolumetricTP6D();     //  um
        public VolumetricTP6D mCSHorg = new VolumetricTP6D();     //  um
        public Point3d mCSHorgProbe = new Point3d();     //  um
        public Point3d mFidorg = new Point3d();     //  um
        public Point3d mHCSrotation = new Point3d();    //  min

        public double[] FindPorg()
        {
            //  Porg.X, Porg.Y, Porg.TX, Porg.TY 를 찾고 저장한다.  --> 이 값들은 Position Command 값이다. 항상 - 에서 + 로 이동하여 정지시켜야 한다.

            // Set Motion Speed Normal
            MotorSetSpeed6D(SpeedLevel.Normal);
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            double[] lastError = new double[4];
            double X = 0;
            double Xold = 0;
            double Y = 0;
            double Yold = 0;
            double[] curPos = new double[6];
            double[] orgPos = MotorCurPos6D();
            double[] movingTable = new double[10] { 0, -100, -165, -160, -80, 0, 80, 160, 80, 0 };   //  arc minute
            FZMath.Point2D[] lmsData = new FZMath.Point2D[5];

            // double orgTY = orgPos[3];
            double orgZ = orgPos[2];
            int itrCnt = 0;
            mCalibrationFullData.Clear();
            mGageFullData.Clear();
            double a = 0, b = 0;

            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {

                    MotorMoveAbs6D(orgPos[0] + X, orgPos[1], orgPos[2], orgPos[3], orgPos[4] + movingTable[i], orgPos[5]);  //  Move TY (mm, arcmin)
                    Thread.Sleep(400);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double dZN = mGageFullData[7][5] - mGageFullData[3][5]; // TY -160~160 에서 Probe
                double dZP = mGageFullData[7][6] - mGageFullData[3][6];
                X = 1.04 * 60000 * (dZN + dZP) / (dZN - dZP);
                //  Porg 정의 다시 확인 필요 20241213.

                if (Math.Abs(X) < 0.2 || (Math.Abs(dZN + dZP)) < 0.21)   //  um
                    break;
                X = X + Xold;
                Xold = X;
            }
            lastError[0] = X;
            mPorg.X = orgPos[0] + Xold;   //  um
            AddVsnLog("Porg X error " + X.ToString("F3") + " um");

            orgPos = MotorCurPos6D();   // mPorg.X 적용한 현재 위치
            // double orgTX = orgPos[4];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1] - Y, orgPos[2], orgPos[3] + movingTable[i], orgPos[4], orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(X, Y, 0, movingTable[i], 0, 0);  //  Move TX (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double dZ4 = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                double dZ2 = (mGageFullData[7][4] - mGageFullData[3][4]);

                Y = 1.04 * 83000 * dZ4 / (dZ2 - dZ4);
                if (Math.Abs(Y) < 0.2 || Math.Abs(dZ4) < 0.11) //    (um)
                    break;
                Y = Y + Yold;
                Yold = Y;
            }
            lastError[1] = Y;
            mPorg.Y = orgPos[1] - Yold;   //  mm
            AddVsnLog("Porg Y error " + Y.ToString("F3") + " um");



            movingTable = new double[10] { 0, -400, -850, -800, -400, 0, 400, 800, 400, 0 };
            double TX = 0;
            double TXold = 0;
            double TY = 0;
            double TYold = 0;


            //////////////////////////////////////////////////////////////////////////
            //  Y 구동, Z 변동 측정, TX 조정
            orgPos = MotorCurPos6D();
            double orgY = orgPos[1];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            double ZshiftWhileMoveingY = 0;
            double ZshiftWhileMoveingX = 0;
            double[] ZshiftWhileMoveingYDebug = new double[11];
            double[] ZshiftWhileMoveingXDebug = new double[11];
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1] + movingTable[i], orgPos[2], orgPos[3] - TX, orgPos[4], orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(0, movingTable[i], 0, TX, 0, 0);  //  Move Y at the TX (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double[][] stablizedData = mCalibrationFullData.ToArray();
                for (int i = 0; i < 5; i++)
                {
                    // orgY, orgZ는 오프셋이라 기울기 구할때 의미없음 -> 그리고 모터 명령값이 아니라 gage 데이터를 넣어야함. -> 의미없으므로 그대로 둠.
                    // stablizedData가 아닌 mGageFullData를 사용해서 LMS 구해야함. 수정완료.
                    lmsData[i] = new FZMath.Point2D(mGageFullData[i + 3][1] - orgY, (mGageFullData[i + 3][5] + mGageFullData[i + 3][6]) / 2 - orgZ);    //  TY(min) measured, Z(um) measured // -800~800um
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(lmsData, 5, ref a, ref b);
                ZshiftWhileMoveingY = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                ZshiftWhileMoveingYDebug[itrCnt] = ZshiftWhileMoveingY;
                TX = Math.Atan(a) * RAD_To_MIN;    //  rad -> min
                AddVsnLog("TX " + itrCnt.ToString() + "th : " + TX.ToString("F3"));
                if (Math.Abs(TX) < 0.2)
                    break;
                TX = TX + TXold;
                TXold = TX;
            }
            lastError[2] = TX;
            mPorg.TX = orgPos[3] - TXold;//  acr min
            AddVsnLog("Porg TX error " + TX.ToString("F3") + " min. Zshift while moving Y = " + ZshiftWhileMoveingY.ToString("F2"));


            //////////////////////////////////////////////////////////////////////////
            //  X 구동, Z 변동 측정, TY 조정
            orgPos = MotorCurPos6D();
            double orgX = orgPos[0];
            orgZ = orgPos[2];
            itrCnt = 0;
            mCalibrationFullData.Clear();
            while (itrCnt++ < 10)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(orgPos[0] + movingTable[i], orgPos[1], orgPos[2], mPorg.TX, orgPos[4] + TY, orgPos[5]);  //  Move TY (mm, arcmin)
                    //MotorMoveAbs6D(movingTable[i], 0, 0, TX, TY, 0);  //  Move X at the TY (mm, arcmin)
                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }
                double[][] stablizedData = mCalibrationFullData.ToArray();
                for (int i = 0; i < 5; i++)
                {
                    //lmsData[i] = new FZMath.Point2D(stablizedData[i + 3][16] - orgX, stablizedData[i + 3][18] - orgZ);    //  TX(min) measured, Z(um) measured
                    lmsData[i] = new FZMath.Point2D(mGageFullData[i + 3][0] - orgX, (mGageFullData[i + 3][5] + mGageFullData[i + 3][6]) / 2 - orgZ);
                }
                m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(lmsData, 5, ref a, ref b);
                //ZshiftWhileMoveingX = stablizedData[7][18] - stablizedData[3][18];
                ZshiftWhileMoveingX = (mGageFullData[7][5] + mGageFullData[7][6]) / 2 - (mGageFullData[3][5] + mGageFullData[3][6]) / 2;
                ZshiftWhileMoveingXDebug[itrCnt] = ZshiftWhileMoveingX;
                TY = Math.Atan(a) * RAD_To_MIN;    //  rad -> min
                if (Math.Abs(TY) < 0.2)
                    break;
                TY = TY + TYold;
                TYold = TY;
            }
            lastError[3] = TY;
            mPorg.TY = orgPos[4] + TYold;//  acr min
            AddVsnLog("Porg TY error " + TY.ToString("F3") + " min. Zshift while moving X = " + ZshiftWhileMoveingX.ToString("F2"));
            return lastError;

        }
        public void FindHCSrotationPsi()
        {
            //  Hexapod X, Y 축(회전축으로서의 X 축 및 Y 축) 과 Probe 로 정의된 X 축, Y축이 Z 축에 대해서 회전된 형태일 때 이 회전각도를 측정해서 Hexapod Coordinate System 을 변환한다.
            //  Porg 상태에서는 TX 가 변할 때 pZ1 + pZ3 는 변하지 않는다.
            double TZ = 0;
            double TZold = 0;
            double[] curPos = new double[6];
            double[] orgPos = MotorCurPos6D();
            double[] movingTable = new double[10] { 0, -60, -130, -120, -60, 0, 60, 120, 60, 0 };   //  TX min
            FZMath.Point2D[] lmsData = new FZMath.Point2D[5];

            double orgTY = orgPos[3];
            double orgZ = orgPos[2];
            int itrCnt = 0;
            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            orgPos = MotorCurPos6D();
            double orgTX = orgPos[4];
            orgZ = orgPos[2];
            itrCnt = 0;
            double a = 0, b = 0;

            MotorMoveHome6D();
            Thread.Sleep(600);
            orgPos = MotorCurPos6D();

            while (itrCnt++ < 10)
            {
                if (itrCnt > 0)
                {
                    MotorMoveHome6D();

                    Thread.Sleep(600);
                }
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                MotorSetHCS(0, 0, TZ);

                for (int i = 0; i < 10; i++)
                {
                    MotorMoveAbs6D(mPorg.X, mPorg.Y, orgPos[2], mPorg.TX + movingTable[i], mPorg.TY, orgPos[5]);  //  Move TX (arcmin)

                    Thread.Sleep(600);
                    curPos = MotorCurPos6D();
                    SingleFindMark(true);
                }

                double dpZ1 = mGageFullData[7][5] - mGageFullData[3][5];                //  um
                double dpZ3 = mGageFullData[7][6] - mGageFullData[3][6];                //  um
                double dTX = (mCalibrationFullData[7][19] - mCalibrationFullData[3][19]) * MIN_To_RAD; //  min -> rad

                TZ = Math.Asin((dpZ1 - dpZ3) / (120000 * Math.Tan(dTX))) * RAD_To_MIN;  //  Full Angluar Stroke (min)

                if (Math.Abs(TZ) < 0.3 || Math.Abs(dpZ1 - dpZ3) < 0.11) // 
                    break;
                //MotorSetHCS(0, 0, 0);

                TZ = TZ + TZold;
                TZold = TZ;
            }
            mHCSrotation.Z = TZ;    //  min
        }
        public void FindHCSrotationAB() //  HCS : Hexapod Coordinate System
        {
            //  Z pivot 을 Porg 위치로 보낸다.
            //  이를 위해서는 CSHorg 상태에서 Z pivot 을 구해서 적용하고(적용하면 Z pivot 이 Center of Fiducial Mark 와 일치하게 됨),
            //  이어서 Porg -> CSHorg 벡터만큼 뒤로 보내서 CSHorg 가 Porg 에 오도록 한다.
            //  
            //  Z 축에 대해서 -2 -> +2 deg 회전시킨다. 
            //  이때 pZ2 의 변화량 -> Beta 구한다
            //  Hexapod 의 회전축으로서의 Z 축이 Probe 로 정의되는 XYZ 좌표계에서 YZ 면과 이루는 각도를 β , 
            //  Hexapod 의 회전축으로서의 Z 축이 Probe 로 정의되는 XYZ 좌표계에서 XZ 면과 이루는 각도를 α ,
            //  α 및 β  를 측정해서 Hexapod Coordinate System 을 변환한다.

            double[] tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min
            double angle = tagetPos[3] - tagetPos[1];
            int itrCnt = 0;
            double[] orgPos = MotorCurPos6D();

            double hcsX = 0;
            double hcsY = 0;
            double hcsXold = 0;
            double hcsYold = 0;

            while (itrCnt++ < 10)
            {

                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                MotorMoveHome6D();
                MotorSetPivot(0, 0, 0);
                Thread.Sleep(100);
                MotorMoveAbs6D(mFidorg.X, mFidorg.Y, mFidorg.Z, mPorg.TX, mPorg.TY, mPorg.TZ);
                Thread.Sleep(100);

                MotorSetHCS(hcsX, hcsY, mHCSrotation.Z);

                for (int i = 0; i < 5; i++)
                {
                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5] + tagetPos[i]);
                    if (i < 4)
                    {
                        if (i == 1 || i == 3)
                            Thread.Sleep(600);
                        SingleFindMark(true);
                    }
                }
                double dZ1 = mGageFullData[3][5] - mGageFullData[1][5];
                double dZ2 = mGageFullData[3][4] - mGageFullData[1][4];
                double dZ3 = mGageFullData[3][6] - mGageFullData[1][6];
                angle = mCalibrationFullData[3][21] - mCalibrationFullData[1][21];
                double beta = Math.Atan(dZ2 / (83000 * Math.Sin(angle / 180 * Math.PI)));   //  angle to YZ plane
                double alpha = Math.Atan((dZ1 - dZ3) / (120000 * Math.Sin(angle / 180 * Math.PI))); //  anlge to XZ plane

                hcsX = -alpha * RAD_To_MIN;
                hcsY = -beta * RAD_To_MIN;
                if (Math.Abs(hcsX) < 1 && Math.Abs(hcsY) < 1)
                    break;
                hcsX += hcsXold;
                hcsY += hcsYold;
                hcsXold = hcsX;
                hcsYold = hcsY;
            }

            mHCSrotation.X = hcsXold;
            mHCSrotation.Y = hcsYold;

            MotorSetHCS(0, 0, 0);
            MotorMoveHome6D();

        }
        public void FindCSHorg(bool resetProbe = false)
        {
            if (motorizedMeasurementAbort) return;

            //  CSHorg 로서 CSHead 의 X, Y, Z 측정값이 (0,0,0)  이 되는 XYZ stage 의 위치값을 찾아 저장한다.  --> stage command 값 - 따라서 반복편차 있음.
            //  CSHead 로 측정된  TX, TY, TZ 를 저장하고 Set TX, TY, TZ Zero 한다.

            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            // 동작전 Hexapod 기본설정
            MotorSetSpeed6D(SpeedLevel.Normal);
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            double X = 0;
            double Y = 0;
            double Z = 0;
            double Xnext = 0;
            double Ynext = 0;
            double Znext = 0;
            double TX = 0;
            double TY = 0;
            double TZ = 0;

            int itrCnt = 0;
            double[] orgPos = MotorCurPos6D();

            while (itrCnt < 10)
            {
                if (motorizedMeasurementAbort) return;

                MotorXYZ(orgPos[0] + Xnext, orgPos[1] + Ynext, orgPos[2] + Znext);  //  Move (mm , arcmin)
                Thread.Sleep(600);
                SingleFindMark(true);
                X = mCalibrationFullData[itrCnt][0]; //   um 
                Y = mCalibrationFullData[itrCnt][1]; //   um
                Z = mCalibrationFullData[itrCnt][2]; //   um
                TX = mCalibrationFullData[itrCnt][3]; //   min 
                TY = mCalibrationFullData[itrCnt][4]; //   min 
                TZ = mCalibrationFullData[itrCnt][5]; //   min 

                if (Math.Abs(X) < 0.1 && Math.Abs(Y) < 0.1 && Math.Abs(Z) < 0.1) break; //  Z  200um 상승된 상태를 기준으로 정한다.

                Xnext -= X;
                Ynext -= Y;
                Znext -= Z;
                itrCnt++;
            }

            mCSHorg.X = orgPos[0] + Xnext;
            mCSHorg.Y = orgPos[1] + Ynext;
            mCSHorg.Z = orgPos[2] + Znext;

            // 현재는 Porg 상태 아님 -> 헥사포드 0인 상태에서, CSH 에서 측정된 각도
            mCSHorg.TX = TX;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값 
            mCSHorg.TY = TY;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값 
            mCSHorg.TZ = TZ;    //  Porg 상태에서,  CSH 에서 측정된 각도 -> SetTXTYZero() 으로 저장해줘야 하는 값 

            if (resetProbe)
            {
                m__G.mGageCounter.SetAllPortZero();
                SingleFindMark(true);
            }

            MotorSetHome6D();
        }

        public void FindFidorg()
        {
            //  mFidorg 는 mCSHorg 에 있을 때
            //  mFidorg.Y = Center of Fiducial Mark 로부터 pZ1, Pz3 를 연결하는 직선까지의 거리
            //  mFidorg.X = Center of Fiducial Mark 로부터 pZ1-pZ3 의 중점과 pZ2 를 연결하는 직선까지의 거리

            //  찾는 방법은, CSHorg 로 이동한 상태에서 

            //  X pivot 중심으로 X 회전하면 Probe Z 에 변위가 발생한다. 이것은 Probe Z 가 Center of Fiducial Mark 에서 Y방향으로 떨어져있기 때문이다.
            //  Probe Z 와 Center of Fiducial Mark 간 Y 방향으로의 이격거리는 Z변위 / tan(phi) 가 된다. 
            //  이때 X pivot 은 Center of Fiducial Mark 에 일치하게 설정되었음을 가정한다

            //  Y pivot 중심으로 Y 회전하면 Probe Z 에 변위가 발생한다. 이것은 Probe Z 가 Center of Fiducial Mark 에서 X방향으로 떨어져있기 때문이다.
            //  Probe Z 와 Center of Fiducial Mark 간 X 방향으로의 이격거리는 Z변위 / tan(theta) 가 된다. 
            //  이때 Y pivot 은 Center of Fiducial Mark 에 일치하게 설정되었음을 가정한다

            mCalibrationFullData.Clear();
            mGageFullData.Clear();

            MotorSetSpeed6D(SpeedLevel.Normal);
            MotorXYZ(mCSHorg.X, mCSHorg.Y, mCSHorg.Z);
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);
            Thread.Sleep(200);
            SingleFindMark();

            mCSHorgProbe.X = mGageFullData[0][0];
            mCSHorgProbe.Y = mGageFullData[0][1];
            mCSHorgProbe.Z = (mGageFullData[0][5] + mGageFullData[0][6]) / 2;

            double[] orgPos = MotorCurPos6D();
            double[] tagetPos = new double[5] { -160, -150, 0, 150, 0 };   //  min
            double angle = tagetPos[3] - tagetPos[1];

            mFidorg.Y = 0;
            double Y = 0;
            double dZup = 0;
            double dZdown = 0;
            double dZupSum = 0;
            double dZdownSum = 0;
            int itrCnt = 0;

            MotorSetPivot(mHexapodPivots[0].X, mHexapodPivots[0].Y, mHexapodPivots[0].Z);

            while (itrCnt++ < 4)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();

                for (int i = 0; i < 5; i++)
                {
                    MotorMoveAbsAxis(Axis.TX, orgPos[3] + tagetPos[i]);
                    if (i > 0 && i < 4)
                    {
                        Thread.Sleep(400);
                    }
                    SingleFindMark(true);
                }

                double phi_up = mCalibrationFullData[3][19] - mCalibrationFullData[2][19]; //  dTXup    // Probe
                double phi_down = mCalibrationFullData[2][19] - mCalibrationFullData[1][19]; //  dTXdown    // Probe


                dZup = (mGageFullData[3][5] + mGageFullData[3][6] - mGageFullData[2][5] - mGageFullData[2][6]) / 2;//+ 1510*(1/Math.Cos(phi_up * MIN_To_RAD)-1);
                dZdown = (mGageFullData[2][5] + mGageFullData[2][6] - mGageFullData[1][5] - mGageFullData[1][6]) / 2;// + 1510 * (1 / Math.Cos(phi_down * MIN_To_RAD) - 1);

                double Lup = dZup / Math.Tan(phi_up * MIN_To_RAD);  //  um
                double Ldown = dZdown / Math.Tan(phi_down * MIN_To_RAD);  //  um

                dZupSum += Lup;
                dZdownSum += Ldown;

                Y += (Lup + Ldown) / 2;
            }

            mFidorg.Y = Y / 4;   // 4회 평균
            AddVsnLog("Fidorg Y " + mFidorg.Y.ToString("F3"));


            tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
            angle = tagetPos[3] - tagetPos[1];

            mFidorg.X = 0;
            double X = 0;
            dZupSum = 0;
            dZdownSum = 0;
            itrCnt = 0;

            MotorSetPivot(mHexapodPivots[1].X, mHexapodPivots[1].Y, mHexapodPivots[1].Z);

            while (itrCnt++ < 4)
            {
                mCalibrationFullData.Clear();
                mGageFullData.Clear();
                for (int i = 0; i < 5; i++)
                {
                    MotorMoveAbsAxis(Axis.TY, orgPos[4] + tagetPos[i]);

                    if (i > 0 && i < 4)
                    {
                        Thread.Sleep(400);
                    }
                    SingleFindMark(true);
                }
                double phi_up = mCalibrationFullData[3][20] - mCalibrationFullData[2][20]; //  dTYup
                double phi_down = mCalibrationFullData[2][20] - mCalibrationFullData[1][20]; //  dTYdown

                dZup = (mGageFullData[3][5] + mGageFullData[3][6] - mGageFullData[2][5] - mGageFullData[2][6]) / 2;// + 1510 * (1 / Math.Cos(phi_up * MIN_To_RAD) - 1);
                dZdown = (mGageFullData[2][5] + mGageFullData[2][6] - mGageFullData[1][5] - mGageFullData[1][6]) / 2;// + 1510 * (1 / Math.Cos(phi_down * MIN_To_RAD) - 1);


                double Lup = dZup / Math.Tan(phi_up * MIN_To_RAD);  //  um
                double Ldown = dZdown / Math.Tan(phi_down * MIN_To_RAD);  //  um
                dZupSum += Lup;
                dZdownSum += Ldown;
                X += (Lup + Ldown) / 2;
            }
            mFidorg.X = X / 4;   // 4회 반복 평균,
            mFidorg.Z = 0;

            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);

            AddVsnLog("Fidorg X " + mFidorg.X.ToString("F3"));
            mbFigorgLoaded = true;
        }

        public double ZcompensationAboutTXTY(double Xmm, double Ymm, double Zmm, double TXmin, double TYmin)
        {
            //  TX 및 TY 의 회전중심이 Center of Fiducial Mark 일 때
            //  아래 적용 시 Probe 측정 Z 값과 CSH 측정 Z 값이 같게 된다.
            double res = Zmm
                + (Ymm + mFidorg.Y / 1000 - mCSHorg.Y) * Math.Tan(TXmin / (60 * 180 / Math.PI))
                + (Xmm + mFidorg.X / 1000 - mCSHorg.X) * Math.Tan(TYmin / (60 * 180 / Math.PI));

            //  TX 및 TY 의 회전중심이 Center of Fiducial Mark 가 아닐 때는
            //  TX 회전중심부터 Center of Fiducial Mark 까지의 벡터를 알아야 한다.
            //  아래 적용 시 Probe 측정 Z 값과 CSH 측정 Z 값이 같게 된다.

            return res;
        }

        const double Pixel_To_Um = 5.5 / Global.LensMag;
        const double Um_To_Pixel = Global.LensMag / 5.5;

        double MIN_To_RAD = Math.PI / (180 * 60);
        double RAD_To_MIN = 180 * 60 / Math.PI;

        public double ProbeZcompensationForTXTY(double px, double py, double pz, double pTXrad, double pTYrad)
        {
            double resZ = 0;
            //double resZold = 0;

            if (!mbFigorgLoaded)
                //return resZ;
                return pz;  // 보정 적용 안된 초기값 반환: EastView할때는 mbFigorgLoaded false임.

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋하지 않고 그 이후로도 Probe 값을 리셋하지 않는 경우
            //  즉 mCSHorg 를 찾기 전에 Probe 값을 리셋하고 이후로는 Probe 값을 리셋하지 않는 경우 다음 식 적용
            //double curXold = - px - mCSHorgProbe.X + mFidorg.X;   //  um   //  수정 전
            //double curX = (px - mCSHorgProbe.X) + mFidorg.X;   //  um    //  수정 후
            double curX = -(px - mCSHorgProbe.X) + mFidorg.X;   //  um    //  수정 후
            double curY = py - mCSHorgProbe.Y + mFidorg.Y;   //  um

            //  mCSHorgProbe 에서 Probe 값을 (0,0,0) 으로 리셋했고 그 이후에 mFidorg 를 측정했으므로 mFidorg 로부터 현재위치까지의 거리는 다음 식이 맞다.
            //double curX = - px + mFidorg.X;   //  um   
            //double curY = py + mFidorg.Y;   //  um

            double pT = Math.Sqrt(pTXrad * pTXrad + pTYrad * pTYrad);

            //resZ -= Math.Sin(pTYrad) * curX;
            //resZ -= Math.Sin(pTXrad) * curY;

            resZ -= Math.Tan(pTYrad) * curX;
            resZ -= Math.Tan(pTXrad) * curY;
            resZ += 1510 * (1 / Math.Cos(pT) - 1);

            //resZold -= Math.Sin(pTYrad) * curXold;
            //resZold -= Math.Sin(pTXrad) * curY;
            //resZold += 1510 * (1 / Math.Cos(pT) - 1);

            //return resZold + pz;
            return resZ + pz;
        }
        public Point3d XYZcompensationAboutZPivots(Point3d ProbeXYZ, double TXrad, double TYrad)
        {
            //mPorg = new VolumetricTP6D(0, 0, 0, 0, 0, 0);
            //mFidorg = new Point3d(3, 0, 0);    //  (TX,TY,TZ) = (mPorg.TX, mPorg.TY, mPorg.TZ) , mFidorg 는 XYZ stage 의 이동에 따라 변하거나 하지 않는다.
            //                                   //  이 좌표는 XYZ stage 가 Porg 일 때 Center of fiducial mark 의 XYZ stage 에서의 좌표이다.
            //mHexapodPivots[0] = new Point3d(0, -1, 0);   //  X pivot
            //mHexapodPivots[1] = new Point3d(-1, 0, 0);   //  Y pivot
            //mHexapodPivots[2] = new Point3d(1, 1, 0);   //  Y pivot


            //  Y 회전 먼저, X 회전 나중에
            double TX = TXrad;// * MIN_To_RAD;
            double TY = TYrad;// * MIN_To_RAD;
            Point3d Vy = new Point3d();
            Point3d Vy1 = new Point3d();
            Vy.X = mFidorg.X + mHexapodPivots[1].X - mHexapodPivots[2].X;   //  Pivot Y 의 물리적 위치
            Vy.Y = mFidorg.Y + mHexapodPivots[1].Y - mHexapodPivots[2].Y;
            Vy.Z = mFidorg.Z + mHexapodPivots[1].Z - mHexapodPivots[2].Z;

            Vy1.X = Vy.X + ProbeXYZ.X;
            Vy1.Y = Vy.Y;// + shift.Y;
            Vy1.Z = Vy.Z;// + shift.Z;

            Point3d F2 = new Point3d();
            double[,] Rty = new double[3, 3];
            double[] Pz_Py = new double[3];
            Pz_Py[0] = mHexapodPivots[2].X - mHexapodPivots[1].X;
            Pz_Py[1] = mHexapodPivots[2].Y - mHexapodPivots[1].Y;
            Pz_Py[2] = mHexapodPivots[2].Z - mHexapodPivots[1].Z;
            m__G.oCam[0].mFAL.mFZM.RotationXYZ(0, TYrad, 0, ref Rty);
            //  ~ 여기까지 검증됨
            double[] RtyPz_Py = new double[3];
            m__G.oCam[0].mFAL.mFZM.MatrixCross(ref Rty, ref Pz_Py, ref RtyPz_Py, 3);
            F2.X = RtyPz_Py[0] + Vy1.X;
            F2.Y = RtyPz_Py[1] + Vy1.Y;
            F2.Z = RtyPz_Py[2] + Vy1.Z;
            double dZty = Vy1.Z - Vy1.Z / Math.Cos(TY) + Vy1.X * Math.Tan(TY);



            Point3d Vx = new Point3d();
            Point3d Vx1 = new Point3d();

            //  Pivot X 가 앞선 TY 회전에 의해 이동하는 것을 가정한 경우 : 가능성 낮음
            //  순수한 회전에 의해 fiducial mark 중심이 이동한 만큼 pivot 도 이동했다고 보는 경우
            //  이것은 Y 축 회전각도별 X Pivot 의 좌표를 측정함으로써 검증 가능.
            //Vx.X = F2.X + mHexapodPivots[0].X - mHexapodPivots[2].X;
            //Vx.Y = F2.Y + mHexapodPivots[0].Y - mHexapodPivots[2].Y;
            //Vx.Z = F2.Z + mHexapodPivots[0].Z - mHexapodPivots[2].Z;

            //  Pivot 은 어떤 회전운동에도 관계없이 일정하게 유지되는 것을 가정한 경우 : HEXAPOD 구동 원리상 회전운동이 회전중심을 병진이동시키지 않는다는 전제.
            Vx.X = mFidorg.X + mHexapodPivots[0].X - mHexapodPivots[2].X;
            Vx.Y = mFidorg.Y + mHexapodPivots[0].Y - mHexapodPivots[2].Y;
            Vx.Z = mFidorg.Z + mHexapodPivots[0].Z - mHexapodPivots[2].Z;

            Vx1.X = Vx.X;// + shift.X;
            Vx1.Y = Vx.Y + ProbeXYZ.Y;
            Vx1.Z = Vx.Z;// + shift.Z;

            Point3d F3 = new Point3d();
            Point3d F4 = new Point3d();
            double[,] Rtx = new double[3, 3];
            double[] F2_F_Pz_Px = new double[3];    //  F2 - F + Pz - Px
            F2_F_Pz_Px[0] = F2.X - mFidorg.X + mHexapodPivots[2].X - mHexapodPivots[0].X;
            F2_F_Pz_Px[1] = F2.Y - mFidorg.Y + mHexapodPivots[2].Y - mHexapodPivots[0].Y;
            F2_F_Pz_Px[2] = F2.Z - mFidorg.Z + mHexapodPivots[2].Z - mHexapodPivots[0].Z;
            m__G.oCam[0].mFAL.mFZM.RotationXYZ(TXrad, 0, 0, ref Rtx);
            double[] RtxPz_Px = new double[3];
            m__G.oCam[0].mFAL.mFZM.MatrixCross(ref Rtx, ref F2_F_Pz_Px, ref RtxPz_Px, 3);
            F4.X = RtxPz_Px[0] + Vx1.X;
            F4.Y = RtxPz_Px[1] + Vx1.Y;
            F4.Z = RtxPz_Px[2] + Vx1.Z;

            double dZtytx = (dZty - Vx1.Y * Math.Sin(TX) + Vx1.Z * Math.Cos(TX) - Vx1.Z) / Math.Cos(TX);
            //double dZtytx = (dZty - Vx1.Y * Math.Sin(TX) + Vx1.Z * Math.Cos(TX) - Vx1.Z) / Math.Cos(TX);

            Point3d res = new Point3d();
            res.X = F4.X;
            res.Y = F4.Y;
            res.Z = ProbeXYZ.Z + F4.Z - dZtytx;
            double pT = Math.Sqrt(TXrad * TXrad + TYrad * TYrad);
            res.Z += 1510 * (1 / Math.Cos(pT) - 1);

            return res;
        }

        public bool LoadOQCcondition()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";
            if (!File.Exists(oqcFile))
                return false;
            StreamReader rd = new StreamReader(oqcFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] oqcElements = allLines[0].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mPorg.X = double.Parse(oqcElements[0]);
            mPorg.Y = double.Parse(oqcElements[1]);
            mPorg.Z = double.Parse(oqcElements[2]);
            mPorg.TX = double.Parse(oqcElements[3]);
            mPorg.TY = double.Parse(oqcElements[4]);
            mPorg.TZ = double.Parse(oqcElements[5]);
            oqcElements = allLines[1].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHCSrotation.X = double.Parse(oqcElements[0]);
            mHCSrotation.Y = double.Parse(oqcElements[1]);
            mHCSrotation.Z = double.Parse(oqcElements[2]);
            oqcElements = allLines[2].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mCSHorg.X = double.Parse(oqcElements[0]);
            mCSHorg.Y = double.Parse(oqcElements[1]);
            mCSHorg.Z = double.Parse(oqcElements[2]);
            mCSHorg.TX = double.Parse(oqcElements[3]);
            mCSHorg.TY = double.Parse(oqcElements[4]);
            mCSHorg.TZ = double.Parse(oqcElements[5]);
            if (allLines.Length > 4 && allLines[3].Length > 3)
            {
                oqcElements = allLines[3].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mCSHorgProbe.X = double.Parse(oqcElements[0]);
                mCSHorgProbe.Y = double.Parse(oqcElements[1]);
                mCSHorgProbe.Z = double.Parse(oqcElements[2]);

                oqcElements = allLines[4].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                mFidorg.X = double.Parse(oqcElements[0]);
                mFidorg.Y = double.Parse(oqcElements[1]);
                mFidorg.Z = double.Parse(oqcElements[2]);

                mbFigorgLoaded = true;
            }
            else
            {
                mbFigorgLoaded = false;
            }
            return true;
        }
        public bool LoadPivotXYZ()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            if (!File.Exists(pivotFile))
                return false;
            StreamReader rd = new StreamReader(pivotFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] pivotElements = allLines[0].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[0].X = double.Parse(pivotElements[0]);
            mHexapodPivots[0].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[0].Z = double.Parse(pivotElements[2]);
            pivotElements = allLines[1].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[1].X = double.Parse(pivotElements[0]);
            mHexapodPivots[1].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[1].Z = double.Parse(pivotElements[2]);
            pivotElements = allLines[2].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mHexapodPivots[2].X = double.Parse(pivotElements[0]);
            mHexapodPivots[2].Y = double.Parse(pivotElements[1]);
            mHexapodPivots[2].Z = double.Parse(pivotElements[2]);
            return true;
        }
        public bool ChangePivotXYZ(int axis)
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            if (!File.Exists(pivotFile))
                return false;
            StreamReader rd = new StreamReader(pivotFile);
            string strAll = rd.ReadToEnd();
            rd.Close();
            string[] allLines = strAll.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            axis -= 1;
            allLines[axis] = mHexapodPivots[axis].X.ToString("F7") + "\t" + mHexapodPivots[axis].Y.ToString("F7") + "\t" + mHexapodPivots[axis].Z.ToString("F7");
            StreamWriter wr = new StreamWriter(pivotFile);
            wr.WriteLine(allLines[0]);
            wr.WriteLine(allLines[1]);
            wr.WriteLine(allLines[2]);
            wr.Close();

            return true;

        }

        public bool mbFigorgLoaded = false;

        public void SaveFidorg()
        {
            string FidorgFile = m__G.m_RootDirectory + "\\DoNotTouch\\Fidorg.txt";
            string mstr = /*"Fidorg \t" + */mFidorg.X.ToString("F7") + "\t" + mFidorg.Y.ToString("F7") + "\t" + mFidorg.Z.ToString("F7") + "\r\n";
            StreamWriter wr = new StreamWriter(FidorgFile);
            wr.Write(mstr);
            wr.Close();
        }
        private void button19_Click(object sender, EventArgs e)
        {
        }


        private void SaveCSHorg()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";
            if (!File.Exists(oqcFile))
                return;
            StreamReader rd = new StreamReader(oqcFile);
            string lstr = rd.ReadToEnd();
            rd.Close();

            string[] allLine = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allLine[2] = mCSHorg.X.ToString() + "\t" + mCSHorg.Y.ToString() + "\t" + mCSHorg.Z.ToString() + "\t" +
                    mCSHorg.TX.ToString() + "\t" + mCSHorg.TY.ToString() + "\t" + mCSHorg.TZ.ToString();
            string strCSHProbe = mCSHorgProbe.X.ToString() + "\t" + mCSHorgProbe.Y.ToString() + "\t" + mCSHorgProbe.Z.ToString();

            StreamWriter wr = new StreamWriter(oqcFile);
            wr.WriteLine(allLine[0]);
            wr.WriteLine(allLine[1]);
            wr.WriteLine(allLine[2]);
            wr.WriteLine(strCSHProbe);
            wr.Close();
        }

        private void SaveOQCCondition()
        {
            string oqcFile = m__G.m_RootDirectory + "\\DoNotTouch\\OQCcondition" + m__G.mCamID0 + ".txt";

            using (StreamWriter wr = new StreamWriter(oqcFile))
            {
                wr.WriteLine($"{mPorg.X}\t{mPorg.Y}\t{mPorg.Z}\t{mPorg.TX}\t{mPorg.TY}\t{mPorg.TZ}");
                wr.WriteLine($"{mHCSrotation.X}\t{mHCSrotation.Y}\t{mHCSrotation.Z}");
                wr.WriteLine($"{mCSHorg.X}\t{mCSHorg.Y}\t{mCSHorg.Z}\t{mCSHorg.TX}\t{mCSHorg.TY}\t{mCSHorg.TZ}");
                wr.WriteLine($"{mCSHorgProbe.X}\t{mCSHorgProbe.Y}\t{mCSHorgProbe.Z}");
                wr.WriteLine($"{mFidorg.X:F7}\t{mFidorg.Y:F7}\t{mFidorg.Z:F7}");
            }
        }
        private async void btnFindCSHorg_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnFindCSHorg.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnFindCSHorg.Text = "Stop";

            try
            {
                LoadOQCcondition();
                await Task.Run(() => { FindCSHorg(); });
                SaveCSHorg();
            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnFindCSHorg.Enabled = true;
                btnFindCSHorg.Text = "Find CSHorg";
            }
        }

        public void SavePivots()
        {
            string pivotFile = m__G.m_RootDirectory + "\\DoNotTouch\\PivotXYZ.txt";
            using (StreamWriter wr = new StreamWriter(pivotFile))
            {
                // X Pivot
                wr.WriteLine($"{mHexapodPivots[0].X:F7}\t{mHexapodPivots[0].Y:F7}\t{mHexapodPivots[0].Z:F7}");
                // Y Pivot
                wr.WriteLine($"{mHexapodPivots[1].X:F7}\t{mHexapodPivots[1].Y:F7}\t{mHexapodPivots[1].Z:F7}");
                // Z Pivot
                wr.WriteLine($"{mHexapodPivots[2].X:F7}\t{mHexapodPivots[2].Y:F7}\t{mHexapodPivots[2].Z:F7}");
            }
        }
        private void cbBench_CheckedChanged(object sender, EventArgs e)
        {
            if (cbBench.Checked)
            {
                PogoPinUnloadBtn.Show();
                PogoPinloadBtn.Show();
                SidePushUnloadBtn.Show();
                SidePushloadBtn.Show();
                BaseDownBtn.Show();
                BaseUpBtn.Show();
            }
            else
            {
                PogoPinUnloadBtn.Hide();
                PogoPinloadBtn.Hide();
                SidePushUnloadBtn.Hide();
                SidePushloadBtn.Hide();
                BaseDownBtn.Hide();
                BaseUpBtn.Hide();
            }
        }

        private void FVision_Shown(object sender, EventArgs e)
        {
            //InitMasterZeroList();
            //MasterList.SelectedIndex = GetMasterZeroIndex();
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            LoadOQCcondition();
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnScan.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnScan.Text = "Stop";

            try
            {
                if (cboAxis.SelectedItem != null &&
                    double.TryParse(tbMaxStroke.Text, out double onewayStroke) &&
                    double.TryParse(tbStep.Text, out double step) &&
                    onewayStroke * 2 >= step)
                {
                    Axis axis = (Axis)cboAxis.SelectedItem;
                    bool isCheckedProbeReset = chkProbeReset.Checked;
                    bool isCheckedSaveImg = chkSaveImg.Checked;
                    
                    if(LoadPivotXYZ() == false)
                    {
                        MessageBox.Show("Fail to Load Pivot XYZ");
                    }
                    if (LoadOQCcondition() == false)
                    {
                        MessageBox.Show("Fail to Load OQC Condition");
                    }

                    await Task.Run(() =>
                    {
						if (motorizedMeasurementAbort) return;
                        AddVsnLog("Start to find CSHorg");
                        FindCSHorg();

                        if (isCheckedProbeReset)
                        {


                            for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
                            {
                                if (motorizedMeasurementAbort) return;
                                AddVsnLog($"Start to find {pivotAxis} pivot.");
                                FindPivot(pivotAxis);
                            }

                            SavePivots();


                            if (motorizedMeasurementAbort) return;
                            AddVsnLog("Start to find CSHorg, Reset Probe.");
                            FindCSHorg(true);   // Probe 리셋

                            if (motorizedMeasurementAbort) return;
                            AddVsnLog("Start to find Fidorg");
                            FindFidorg();

                            SaveOQCCondition();
                        }

                        // 측정
                        //AxisCalibration(Axis.Z, 1750, true, false, false);
                        //AxisCalibration(Axis.Y, 1900, true, false, false);
                        //AxisCalibration(Axis.X, 1900, true, false, false);
                        //AxisCalibration(Axis.TY, 160, true, false, false);
                        //AxisCalibration(Axis.TX, 160, true, false, false);
                        //AxisCalibration(Axis.TZ, 160, true, false, false);
                        for (int i = 0; i < 5; i++)
                        {
                            var stabilizedDataList = new List<List<double[]>> { ScanAxis(axis, onewayStroke, step, isCheckedSaveImg, false, false) };
                            SaveMeasuredData(stabilizedDataList, $"{axis}_Scan", "Scan");
                            AddVsnLog($"--- {i + 1}번째 {axis} Axis Scan Completed ---");             
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Please check the axis, stroke, and step values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnScan.Enabled = true;
                btnScan.Text = "Scan";
                AddVsnLog("Scan Finished");
            }


        }

        private async void btnAutoCal_Click(object sender, EventArgs e)
        {
            AutoCal();
        }
        public async void AutoCal()
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnAutoCal.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnAutoCal.Text = "Stop";

            AddVsnLog("Start Calibration");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await Task.Run(() =>
                {
                    AutoCalibration();
                });
            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                sw.Stop();
                AddVsnLog($"Ellapsed Time : {sw.ElapsedMilliseconds}ms");
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnAutoCal.Enabled = true;
                btnAutoCal.Text = "Auto Cal";
            }
        }
        private async void btnEastView_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnEastView.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnEastView.Text = "Stop";

            AddVsnLog("Start East-Side View YP Calibration");

            try
            {
                await Task.Run(() =>
                {
                    InitializeScaleNTheta();
                    m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_scaleTZ, ms_EastViewYPscale,
                                                     ms_XtoYbyView, ms_XtoZbyView, ms_XtoTXbyView, ms_XtoTYbyView, ms_XtoTZbyView,
                                                     ms_YtoXbyView, ms_YtoZbyView, ms_YtoTXbyView, ms_YtoTYbyView, ms_YtoTZbyView,
                                                     ms_ZtoXbyView, ms_ZtoYbyView, ms_ZtoTXbyView, ms_ZtoTYbyView, ms_ZtoTZbyView,
                                                     ms_TXtoTYbyView, ms_TXtoTZbyView,
                                                     ms_TYtoTXbyView, ms_TYtoTZbyView,
                                                     ms_TZtoTXbyView, ms_TZtoTYbyView,
                                                     ms_XJtoXbyView, ms_YJtoYbyView, ms_ZJtoZbyView,
                                                     ms_TZtoZbyView);
                    AddVsnLog("Reset scales");
                    SaveScaleNTheta();

                    AddVsnLog("Find CSHorg");
                    FindCSHorg(true);

                    AddVsnLog("Start baseline measurement");
                    EastViewCalibration(true);
                    AddVsnLog("Finish East-Side View YP Calibration");
                });

            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnEastView.Enabled = true;
                btnEastView.Text = "EastView";
            }
        }

        private void AddTbInfo(string lstr)
        {
            if (tbVsnLog.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += lstr + "\r\n";
                    tbInfo.SelectionStart = tbInfo.Text.Length;
                    tbInfo.ScrollToCaret();
                });
            else
            {
                tbInfo.Text += lstr + "\r\n";
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
            }
        }

        private void AddVsnLog(string lstr)
        {
            if (tbVsnLog.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbVsnLog.Text += lstr + "\r\n";
                    tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                    tbVsnLog.ScrollToCaret();
                });
            else
            {
                tbVsnLog.Text += lstr + "\r\n";
                tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
                tbVsnLog.ScrollToCaret();
            }

        }

        private void SettbUnCalibratedInfoVisible(bool visible)
        {
            tbUncalibratedInfo.BeginInvoke(new Action(() => { tbUncalibratedInfo.Visible = visible; }));
        }

        

        private void FVision_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private async void btnRangeTest_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnRangeTest.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnRangeTest.Text = "Stop";

            try
            {
                await Task.Run(() =>
                {
                    // Stroke 범위 내에서 측정 가능한지 확인
                    FindCSHorg();
                    // X
                    double[] orgPos = MotorCurPos6D();
                    for (int i = 0; i < 5; i++)
                    {
                        if (motorizedMeasurementAbort) return;

                        switch (i)
                        {
                            case 0:
                                break;
                            case 1:
                                MotorMoveAbsAxis(Axis.Y, orgPos[1] + 900);
                                break;
                            case 2:
                                MotorMoveAbsAxis(Axis.Y, orgPos[1] - 900);
                                break;
                            case 3:
                                MotorMoveAbsAxis(Axis.Z, orgPos[2] + 800);
                                break;
                            case 4:
                                MotorMoveAbsAxis(Axis.Z, orgPos[2] - 800);
                                break;
                        }
                        // 이미지 저장
                        double[] curpos = MotorCurPos6D();
                        string fileName = $"{m__G.m_RootDirectory}\\Result\\RawData\\Image\\RangeTest_X{curpos[0]}_Y{curpos[1]}_Z{curpos[2]}.bmp";
                        m__G.oCam[0].SaveGrabbedImage(1, fileName);
                        SingleFindMark();

                        double[] targetPos = new double[] { 950, 1900, 0, -950, -1900, 0 };

                        foreach (double pos in targetPos)
                        {
                            if (motorizedMeasurementAbort) return;

                            MotorMoveAbsAxis(Axis.X, orgPos[0] + pos);
                            SingleFindMark();

                            // 이미지 저장
                            curpos = MotorCurPos6D();
                            fileName = $"{m__G.m_RootDirectory}\\Result\\RawData\\Image\\RangeTest_X{curpos[0]}_Y{curpos[1]}_Z{curpos[2]}.bmp";
                            m__G.oCam[0].SaveGrabbedImage(1, fileName);
                        }

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                    }
                    // Y
                    for (int i = 0; i < 5; i++)
                    {
                        if (motorizedMeasurementAbort) return;

                        switch (i)
                        {
                            case 1:
                                MotorMoveAbsAxis(Axis.X, orgPos[0] + 900);
                                break;
                            case 2:
                                MotorMoveAbsAxis(Axis.X, orgPos[0] - 900);
                                break;
                            case 3:
                                MotorMoveAbsAxis(Axis.Z, orgPos[2] + 600);
                                break;
                            case 4:
                                MotorMoveAbsAxis(Axis.Z, orgPos[2] - 600);
                                break;
                        }
                        SingleFindMark();

                        List<double> targetPos;
                        if (i < 3)
                        {
                            targetPos = new List<double> { 950, 1900, 0, -950, -1900, 0 };
                        }
                        else
                        {
                            targetPos = new List<double> { 700, 0, -700, 0 };
                        }

                        foreach (double pos in targetPos)
                        {
                            if (motorizedMeasurementAbort) return;

                            MotorMoveAbsAxis(Axis.Y, orgPos[1] + pos);
                            SingleFindMark();
                        }

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                    }
                    // Z
                    for (int i = 0; i < 5; i++)
                    {
                        if (motorizedMeasurementAbort) return;

                        switch (i)
                        {
                            case 1:
                                MotorMoveAbsAxis(Axis.X, orgPos[0] + 900);
                                break;
                            case 2:
                                MotorMoveAbsAxis(Axis.X, orgPos[0] - 900);
                                break;
                            case 3:
                                MotorMoveAbsAxis(Axis.Y, orgPos[1] + 600);
                                break;
                            case 4:
                                MotorMoveAbsAxis(Axis.Y, orgPos[1] - 600);
                                break;
                        }
                        SingleFindMark();

                        List<double> targetPos;
                        if (i < 3)
                        {
                            targetPos = new List<double> { 800, 1750, 0, -800, -1750, 0 };
                        }
                        else
                        {
                            targetPos = new List<double> { 700, 0, -700, 0 };
                        }

                        foreach (double pos in targetPos)
                        {
                            if (motorizedMeasurementAbort) return;

                            MotorMoveAbsAxis(Axis.Z, orgPos[2] + pos);
                            SingleFindMark();
                        }

                        MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                    }
                });
            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnRangeTest.Enabled = true;
                btnRangeTest.Text = "Range Test";
            }
        }

        private void FVision_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m__G.mGageCounter != null)
            {
                if (m__G.m_bCalibrationModel)
                {
                    m__G.mGageCounter.CloseAllport();
                    m__G.mGageCounter.DisposAllport();
                }
            }
        }

        private void btnAddMaster_Click(object sender, EventArgs e)
        {
            //if (MasterList.Items.Count >= 3) { return; }
            SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
            InitMasterZeroList();
            MasterList.SelectedIndex = GetMasterZeroCount() - 1;
        }

        private void MasterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MasterList.SelectedItem == null) return;
            string selectedItem = MasterList.SelectedItem.ToString();
            int index = MasterList.SelectedIndex;
            if (index == 0)
                SetMasterZeroIndex(0);
            else
                SetMasterZeroIndex(3);

            txtMsaterNum.Text = GetMasterZeroIndex().ToString();
        }

        private void btnDeleteMaster_Click(object sender, EventArgs e)
        {
            if (MasterList.SelectedItem == null || MasterList.Items.Count <= 0) return;
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\OffsetZero\\" + MasterList.SelectedItem.ToString();
            File.Delete(filename);
            InitMasterZeroList();
            MasterList.SelectedIndex = GetMasterZeroCount() - 1;
        }

        private void FVision_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                InitMasterZeroList();
                //MasterList.SelectedIndex = GetMasterZeroIndex();
            }
        }

        private async void btnRepeatTest_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnRepeatTest.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btnRepeatTest.Text = "Stop";

            try
            {
                if (cboAxis.SelectedItem != null &&
                double.TryParse(tbMaxStroke.Text, out double onewayStroke) &&
                double.TryParse(tbStep.Text, out double step) &&
                (onewayStroke * 2 >= step))
                {
                    Axis axis = (Axis)cboAxis.SelectedItem;
                    bool isCheckedProbeReset = chkProbeReset.Checked;

                    if (LoadOQCcondition() == false)
                    {
                        MessageBox.Show("Fail to Load OQC Condition");
                    }

                    await Task.Run(() =>
                    {

                        if (motorizedMeasurementAbort) return;
                        AddVsnLog("Start to find CSHorg");
                        FindCSHorg();
						if (isCheckedProbeReset)
						{
	                        for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
	                        {
	                            if (motorizedMeasurementAbort) return;
	                            AddVsnLog($"Start to find {pivotAxis} pivot.");
	                            FindPivot(pivotAxis);
	                        }
							SavePivots();

	                        AddVsnLog("Start to find CSHorg");
	                        FindCSHorg(true);

	                        if (motorizedMeasurementAbort) return;
	                        AddVsnLog("Start to find Fidorg");
	                        FindFidorg();

	                        SaveOQCCondition();
						}

                        var stabilizedData = ScanAxis(axis, onewayStroke, step, false, false, false, null, 0, 20);
                        var stabilizedDataList = new List<List<double[]>> { stabilizedData };
                        SaveMeasuredData(stabilizedDataList, $"{axis}_Repeat", "Repeat");
                        
                    });
                }
                else
                {
                    MessageBox.Show("Please check the axis, stroke, and step values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                AddVsnLog($"failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btnRepeatTest.Enabled = true;
                btnRepeatTest.Text = "Repeat Test";
            }
        }

        private void cboAxis_SelectedIndexChanged(object sender, EventArgs e)
        {
            Axis axis = (Axis)cboAxis.SelectedItem;

            switch (axis)
            {
                case Axis.X:
                    {
                        tbMaxStroke.Text = "1600";
                        tbStep.Text = "100";
                        break;
                    }
                case Axis.Y:
                    {
                        tbMaxStroke.Text = "1700";
                        tbStep.Text = "100";
                        break;
                    }
                case Axis.Z:
                    {
                        tbMaxStroke.Text = "1550";
                        tbStep.Text = "100";
                        break;
                    }
                case Axis.TX:
                    {
                        tbMaxStroke.Text = "160";
                        tbStep.Text = "12";
                        break;
                    }
                case Axis.TY:
                case Axis.TZ:
                    {
                        tbMaxStroke.Text = "200";
                        tbStep.Text = "12";
                        break;
                    }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cbContinuosMode.Checked = false;

            FindCarrierToDummyShift();

            CameraReset(2, true);
        }

        public double[] mPhiThetaPsi = new double[3];
        private void btnApplyEulerAngle_Click(object sender, EventArgs e)
        {
            double sX = double.Parse(tbEulerSX.Text);   // um
            double sY = double.Parse(tbEulerSY.Text);   // um
            double sZ = double.Parse(tbEulerSZ.Text);   // um

            if (Math.Abs(sY) > Math.Abs(sX) && Math.Abs(sY) > Math.Abs(sZ))
            {
                mPhiThetaPsi[0] = -Math.Atan(sZ / sY);
                mPhiThetaPsi[1] = 0;
                mPhiThetaPsi[2] = -Math.Atan(-sX / sY);
            }
            else if (Math.Abs(sX) > Math.Abs(sY) && Math.Abs(sX) > Math.Abs(sZ))
            {
                mPhiThetaPsi[0] = 0;
                mPhiThetaPsi[1] = -Math.Atan(-sZ / sX);
                mPhiThetaPsi[2] = -Math.Atan(sY / sX);
            }
            else if (Math.Abs(sZ) > Math.Abs(sY) && Math.Abs(sZ) > Math.Abs(sX))
            {
                mPhiThetaPsi[0] = -Math.Atan(-sY / sZ);
                mPhiThetaPsi[1] = -Math.Atan(sX / sZ);
                mPhiThetaPsi[2] = 0;
            }
            tbEulerPhi.Text = (mPhiThetaPsi[0] * 180 / Math.PI).ToString("F2");
            tbEulerTheta.Text = (mPhiThetaPsi[1] * 180 / Math.PI).ToString("F2");
            tbEulerPsi.Text = (mPhiThetaPsi[2] * 180 / Math.PI).ToString("F2");

            m__G.oCam[0].mFAL.SetEulerAngle(mPhiThetaPsi);
            string eulerFile = m__G.m_RootDirectory + "\\DoNotTouch\\EulerAngle" + camID0 + ".txt";
            StreamWriter wr = new StreamWriter(eulerFile);
            wr.WriteLine(mPhiThetaPsi[0].ToString("F8"));
            wr.WriteLine(mPhiThetaPsi[1].ToString("F8"));
            wr.WriteLine(mPhiThetaPsi[2].ToString("F8"));
            wr.Close();



            //  Debugging : Verification
            //double[,] EulerR = new double[3,3];
            //m__G.oCam[0].mFAL.mFZM.RotationEuler(mPhiThetaPsi[0], mPhiThetaPsi[1], mPhiThetaPsi[2], ref EulerR);
            //double[] curPos = new double[3] ;
            //curPos[0] = sX;
            //curPos[1] = sY;
            //curPos[2] = sZ;

            //double[] RotatedCurPos = new double[3] { 0,0,0 };
            //m__G.oCam[0].mFAL.mFZM.MatrixCross(ref EulerR, ref curPos, ref RotatedCurPos,3);

            //tbVsnLog.Text = RotatedCurPos[0].ToString("F2") + "\t" + RotatedCurPos[1].ToString("F1") + "\t" + RotatedCurPos[2].ToString("F1") + "\r\n";
            //tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            //tbVsnLog.ScrollToCaret();
        }
        public void UpdateEulerAngle()
        {
            tbEulerPhi.Text = (mPhiThetaPsi[0] * 180 / Math.PI).ToString("F2");
            tbEulerTheta.Text = (mPhiThetaPsi[1] * 180 / Math.PI).ToString("F2");
            tbEulerPsi.Text = (mPhiThetaPsi[2] * 180 / Math.PI).ToString("F2");
        }

        public string mDataFile100 = "";
        private CancellationTokenSource prism45Cts;

        private async void btn45Test_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                prism45Cts?.Cancel();
                motorizedMeasurementAbort = true;
                btn45Test.Enabled = false;
                return;
            }

            motorizedMeasurementRun = true;
            btn45Test.Text = "Stop";

            try
            {
                TimeSpan targetTime = new TimeSpan(8, 0, 0);   // 오전 8시
                DateTime now = DateTime.Now;
                DateTime todayTarget = DateTime.Today.Add(targetTime);

                // 이미 목표 시각이 지났으면 내일로 넘김
                if (now > todayTarget)
                    todayTarget = todayTarget.AddDays(1);

                TimeSpan waitTime = todayTarget - now;

                prism45Cts = new CancellationTokenSource();
                var token = prism45Cts.Token;

                // 시간 예약 해제
                //await Task.Delay(waitTime, token);

                await Task.Run(() =>
                {
                    Prism45SeparationTest();
                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0); // Prism45SeparationTest사용할때는 SetTXTYOffset써야함.
                    //Prism45Test();
                }, token);
            }
            catch (Exception ex)
            {
                AddVsnLog($"Failed: {ex.Message}");
            }
            finally
            {
                motorizedMeasurementRun = false;
                motorizedMeasurementAbort = false;
                btn45Test.Enabled = true;
                btn45Test.Text = "Prism45 Test";
            }
        }

        // 3 -> -3 deg 구동   // xyz stage 만
        public void Prism45SeparationTest()
        {
            if (motorizedMeasurementAbort) return;

            var psweeXYZPoints = LoadPsweepXYZPoints();

            for (int iAxis = 3; iAxis < 6; iAxis++)
            {
                mDataFile100 = "";
                Axis axis = (Axis)iAxis;
                Axis prismAxis = Axis.TX;

                // 헥사 포드 TZ 생략(= Prism45 TX 생략)
                if (axis == Axis.TZ || axis == Axis.TY) continue;

                switch (axis)
                {
                    case Axis.TX:
                        prismAxis = Axis.TY;
                        break;
                    case Axis.TY:
                        prismAxis = Axis.TZ;
                        break;

                    case Axis.TZ:
                        prismAxis = Axis.TX;
                        break;
                }

                m__G.oCam[0].mFAL.mFZM.SetPrismZeroTXTZ(0, 0, 0);
                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
                SingleFindMark();

                AddVsnLog("Start to find CSHorg");
                FindCSHorg();

                for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
                {
                    if (motorizedMeasurementAbort) return;
                    AddVsnLog($"Start to find {pivotAxis} pivot.");
                    FindPivot(pivotAxis);
                }
                SavePivots();

                //if (motorizedMeasurementAbort) return;
                //AddVsnLog($"Start to find PrismCS {axis} Rotation");
                //FindPrismCSRotation(axis, 7000);
                //AddVsnLog($"Start to find PrismCS {axis} Pivot");
                //FindPrismCSPivot(axis);

                if (motorizedMeasurementAbort) return;
                AddVsnLog("Start to find CSHorg");
                FindCSHorg(true);

                if (motorizedMeasurementAbort) return;
                FindFidorg();
                SaveOQCCondition();

                mGageFullData.Clear();
                mCalibrationFullData.Clear();
                mPrismTXTYTZ.Clear();
                mStdevTXTYTZ.Clear();

                for (int i = 0; i < 5; i++)
                {
                    MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z - i * 100, 0, 0, 0);
                    Thread.Sleep(300);
                    SingleFindMark();
                }
                //GrabInitalMark();   //  앞에 프리즘오프셋 추가되었으므로 주석처리

                double[] orgPos = MotorCurPos6D();

                mDataFile100 = $"PrismDrv_{prismAxis}_{DateTime.Now:yyMMdd_HHmmss}";

                //MotorSetPivot(PrismCSPivots[iAxis - 3].X, PrismCSPivots[iAxis - 3].Y, PrismCSPivots[iAxis - 3].Z);
                //MotorSetHCS(PrismCSRotations[iAxis - 3].X, PrismCSRotations[iAxis - 3].Y, PrismCSRotations[iAxis - 3].Z);
                var XYZPoints = psweeXYZPoints[iAxis - 3];

                AddVsnLog("Start to Measure");
                for (int repeatN = 0; repeatN < 5; repeatN++)   // 100회
                {
                    double onewayStroke = 3.2 * 60; // 3.0
                    double step = 0.01 * 60;    // 0.01

                    mAutoCalibrationIndex = 0;

                    // 시작 위치로 이동 + 백래쉬 제거를 위한 구동
                    // 회전
                    //for (int i = 0; i < 7; i++)
                    //{
                    //    if (motorizedMeasurementAbort) return;

                    //    if (i < 3)
                    //    {
                    //        MotorMoveAbsAxis(axis, orgPos[(int)axis] + (i + 1) * onewayStroke / 3);
                    //    }
                    //    else
                    //    {
                    //        MotorMoveAbsAxis(axis, orgPos[(int)axis] + onewayStroke + 15 - 5 * (i - 3));
                    //    }

                    //    Thread.Sleep(300);
                    //    SingleFindMark();
                    //}

                    // 병진
                    double[] xyzStartPoint = XYZPoints[640];
                    double[] curPos = new double[6];
                    for (int i = 0; i < 7; i++)
                    {
                        if (motorizedMeasurementAbort) return;


                        if (i < 3)
                        {
                            MotorXYZ(orgPos[0] + (i + 1) * xyzStartPoint[0] / 3,
                                     orgPos[1] + (i + 1) * xyzStartPoint[1] / 3,
                                     orgPos[2] + (i + 1) * xyzStartPoint[2] / 3);
                        }
                        else
                        {
                            if(i == 3)
                            {
                                curPos = MotorCurPos6D();
                            }

                            int dirX = curPos[0] > orgPos[0] ? 1 : -1;
                            int dirY = curPos[1] > orgPos[1] ? 1 : -1;
                            int dirZ = curPos[2] > orgPos[2] ? 1 : -1;

                            MotorXYZ(curPos[0] + dirX*(300 - 100 * (i - 3)),
                                     curPos[1] + dirY*(300 - 100 * (i - 3)),
                                     curPos[2] + dirZ*(300 - 100 * (i - 3)));
                        }


                        Thread.Sleep(300);
                        SingleFindMark();
                    }

                    // Error OFFSET 발생 방지
                    var orgMaster = mCalibrationFullData[mCalibrationFullData.Count() - 1];
                    double orgMasterX = orgMaster[16];   // um
                    double orgMasterY = orgMaster[17];   // um
                    double orgMasterZ = orgMaster[18];   // um
                    double orgMasterTx = orgMaster[19]; // min
                    double orgMasterTy = orgMaster[20]; // min
                    double orgMasterTz = orgMaster[21]; // min

                    // 현재 SetSignTXTY의 부호 고려. SignTX가 - 일때 사용자가 tbMasterTX.Text에 n을 입력하면 singn 초기화 값 기준인 orgMasterX -n으로 적용해야함.
                    if (m__G.m_bXTiltReverse)
                    {
                        orgMasterTx *= -1;
                    }

                    if (m__G.m_bYTiltReverse)
                    {
                        orgMasterTy *= -1;
                    }

                    // offset, sign 초기화
                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);
                    SingleFindMark();

                    var lastData = mCalibrationFullData[mCalibrationFullData.Count() - 1];

                    double lx = lastData[0];
                    double ly = lastData[1];
                    double lz = lastData[2];
                    double ltx = lastData[3];
                    double lty = lastData[4];
                    double ltz = lastData[5];

                    ////  ltx, lty, ltz 는 측정값 (min)
                    ////  masterTx, masterTy, masterTz  는 부호반전을 고려한 희망하는 값    (min)
                    ////  orgMasterTx, orgMasterTy, orgMasterTz 는 희망하는 값  (min)
                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset((lx - orgMasterX) * Um_To_Pixel, (ly - orgMasterY) * Um_To_Pixel, (lz - orgMasterZ) * Um_To_Pixel, (ltx - orgMasterTx) * MIN_To_RAD, (lty - orgMasterTy) * MIN_To_RAD, (ltz - orgMasterTz) * MIN_To_RAD);
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);
                    Thread.Sleep(300);
                    SingleFindMark();

                    mGageFullData.Clear();
                    mCalibrationFullData.Clear();
                    mPrismTXTYTZ.Clear();
                    mStdevTXTYTZ.Clear();

                    // 측정 시작
                    // -3.2도 -> +3.2도 
                    for (int i = 0; i < XYZPoints.Count(); i++)
                    {
                        if (motorizedMeasurementAbort) return;

                        var XYZpoint = XYZPoints[640-i];
                        MotorXYZ(orgPos[0] + XYZpoint[0], orgPos[1] + XYZpoint[1], orgPos[2]); //+ XYZpoint[2]);
                        // MotorMoveAbsAxis(axis, orgPos[(int)axis] + onewayStroke - step * i);

                        Thread.Sleep(600);
                        SingleFindMark();
                    }

                    // Data
                    var stabilizedDataList = new List<List<double[]>> { mCalibrationFullData.ToList() };
                    AppendMeasuredData(stabilizedDataList, mDataFile100, "Prism45");

                    MotorMoveAbs6D(orgPos[0], orgPos[1], orgPos[2], orgPos[3], orgPos[4], orgPos[5]);
                    Thread.Sleep(300);
                    SingleFindMark();
                }

                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0, 0, 0, 0);


                // 한 축 측정완료시 rawData 메일 전송
                //string attachFilePath = $"{m__G.m_RootDirectory}\\DoNotTouch\\Admin\\StabilizedData_{camID0}_{mDataFile100}.csv";
                //CWilliamEmailer.SendMailToWilliam($"Prism45 {prismAxis}축 구동측정", "Mail Test", attachFilePath);
            }

            MotorSetPivot(0, 0, 0);
            MotorMoveHome6D(); // = csh 위치 (스테이지 + 헥사포드 홈 복귀)
        }
		
        public void Prism45Test()
        {
            if (motorizedMeasurementAbort) return;
            AddVsnLog("Start to find CSHorg");
            FindCSHorg();

            for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
            {
                if (motorizedMeasurementAbort) return;
                AddVsnLog($"Start to find {pivotAxis} pivot.");
                FindPivot(pivotAxis);
            }
            SavePivots();

            if (motorizedMeasurementAbort) return;
            AddVsnLog("Start to find CSHorg");
            FindCSHorg(true);

            if (motorizedMeasurementAbort) return;
            AddVsnLog("Start to find Fidorg");
            FindFidorg();

            SaveOQCCondition();

            for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
            {
                if (motorizedMeasurementAbort) return;
                AddVsnLog($"Start to find PrismCS {pivotAxis} Rotation");
                FindPrismCSRotation(pivotAxis, 6565);
                AddVsnLog($"Start to find PrismCS {pivotAxis} Pivot");
                FindPrismCSPivot(pivotAxis, 6565);
            }

            MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
            GrabInitalMark();
            AddVsnLog("Start to Measure");

            // 측정
            for (int i = 3; i < 6; i++)
            {
                mDataFile100 = "";
                Axis axis = (Axis)i;
                Axis prismAxis = Axis.TX;

                if(axis == Axis.TZ) return;

                switch (axis)
                {
                    case Axis.TX:
                        prismAxis = Axis.TY;
                        break;
                    case Axis.TY:
                        prismAxis = Axis.TZ;
                        break;

                    case Axis.TZ:
                        prismAxis = Axis.TX;
                        break;
                }
                mDataFile100 = $"PrismDrv_{prismAxis}_{DateTime.Now:yyMMdd_HHmmss}";

                MotorSetPivot(PrismCSPivots[i-3].X, PrismCSPivots[i - 3].Y, PrismCSPivots[i - 3].Z);
                MotorSetHCS(PrismCSRotations[i - 3].X, PrismCSRotations[i - 3].Y, PrismCSRotations[i - 3].Z);

                for (int repeatN = 0; repeatN < 1; repeatN++)   // 100회
                {
                    double onewayStroke = 3.0 * 60;
                    double step = 0.01 * 60;    // 0.01

                    mGageFullData.Clear();
                    mCalibrationFullData.Clear();
                    mPrismTXTYTZ.Clear();
                    mStdevTXTYTZ.Clear();

                    if (motorizedMeasurementAbort) return;
                    double orgPos = MotorCurPosAxis(axis);
                    Thread.Sleep(300);
                    mAutoCalibrationIndex = 0;
                    SingleFindMark();

                    // 회전
                    double[] targetPositions = new double[] { -(onewayStroke) / 3, -(onewayStroke) * 2 / 3, -onewayStroke - 15, -onewayStroke - 10, -onewayStroke - 5 };
                    foreach (double targetPos in targetPositions)
                    {
                        if (motorizedMeasurementAbort) return;
                        MotorMoveAbsAxis(axis, targetPos);
                        Thread.Sleep(300);
                        SingleFindMark();
                    }

                    // 누적된 데이터 Clear
                    mGageFullData.Clear();
                    mCalibrationFullData.Clear();
                    mPrismTXTYTZ.Clear();
                    mStdevTXTYTZ.Clear();

                    // 측정 시작
                    double movingStroke = -onewayStroke;
                    double pos = orgPos - onewayStroke;

                    while (movingStroke <= onewayStroke)
                    {
                        if (motorizedMeasurementAbort) return;
                        MotorMoveAbsAxis(axis, pos);
                        Thread.Sleep(300);
                        SingleFindMark();

                        pos += step;
                        movingStroke += step;
                    }
                    MotorMoveAbsAxis(axis, orgPos); // 헥사포드 복귀

                    // Data
                    var stabilizedDataList = new List<List<double[]>> { mCalibrationFullData.ToList() };
                    AppendMeasuredData(stabilizedDataList, mDataFile100, "Prism45");
                }

                // 한 축 측정완료시 rawData 메일 전송
                //string attachFilePath = $"{m__G.m_RootDirectory}\\DoNotTouch\\Admin\\StabilizedData_{camID0}_{mDataFile100}.csv";
                //CWilliamEmailer.SendMailToWilliam($"Prism45 {prismAxis}축 구동측정", "Mail Test", attachFilePath);
            }

            MotorSetPivot(0, 0, 0);
        }

        private List<List<double[]>> LoadPsweepXYZPoints()
        {
            string filePath = @"C:\CSHTest\DoNotTouch\Admin\psweep_points.csv";
            string[] lines = File.ReadAllLines(filePath);

            List<double[]> psweepTXPoints = new List<double[]>();
            List<double[]> psweepTYPoints = new List<double[]>();
            List<double[]> psweepTZPoints = new List<double[]>();

            for (int row = 1; row < lines.Length; row++)
            {
                string[] values = lines[row].Split(',');

                psweepTXPoints.Add(new double[]
                {
                    double.Parse(values[1], CultureInfo.InvariantCulture),
                    double.Parse(values[2], CultureInfo.InvariantCulture),
                    double.Parse(values[3], CultureInfo.InvariantCulture)
                });

                psweepTYPoints.Add(new double[]
                {
                double.Parse(values[7], CultureInfo.InvariantCulture),
                double.Parse(values[8], CultureInfo.InvariantCulture),
                double.Parse(values[9], CultureInfo.InvariantCulture)
                });

                psweepTZPoints.Add(new double[]
                {
                double.Parse(values[13], CultureInfo.InvariantCulture),
                double.Parse(values[14], CultureInfo.InvariantCulture),
                double.Parse(values[15], CultureInfo.InvariantCulture)
                });
            }

            return new List<List<double[]>> { psweepTXPoints, psweepTYPoints, psweepTZPoints };
        }


        public Point3d[] PrismCSRotations = new Point3d[3];
        public Point3d[] PrismCSPivots = new Point3d[3];

        public void FindPrismCSRotation(Axis axis, double zOffsetPivot)
        {
            FindCSHorg();

            if (motorizedMeasurementAbort) return;

            List<double> targetList = new List<double>();
            double stroke = 3.0;
            double temp = -stroke;

            while (temp <= stroke)
            {
                targetList.Add(temp);
                temp += 0.5;
            }

            int m = targetList.Count;
            int n = 3;
            // 회전축 방향 벡터
            double[] normal0 = new double[n];   // 기준 법선 벡터
            double[] normalm = new double[n];   // 실제 법선 벡터
            double[] normal1 = new double[n];   // 설정 법선 벡터

            double rad45 = 45 * Math.PI / 180;
            switch (axis)
            {
                case Axis.TX:   //  Prism Y == CSH X == Hexapod X
                    normal0 = new double[] { 1, 0, 0 };
                    break;

                case Axis.TY:   //  Prism Z == CSH (0, 1 , -1) == Hexapod (0, 1 , -1) 
                    normal0 = new double[] { 0, Math.Cos(rad45), -Math.Sin(rad45) };
                    break;

                case Axis.TZ:   //  Prism X == CSH (0, 1 , 1) == Hexapod (0, 1 , 1) 
                    normal0 = new double[] { 0, Math.Cos(rad45), Math.Sin(rad45) };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, "지원하지 않는 axis입니다.");
            }
            normal1 = new double[] { normal0[0], normal0[1], normal0[2] };

            // 디버깅

            List<Point3d> fullNormal1 = new List<Point3d>();   // 법선
            List<Point3d> fullArcmin = new List<Point3d>();   // 설정할 벡터
            List<Point3d> fullNormalm = new List<Point3d>();   // 설정할 벡터 -> 각도
            List<Point3d[]> fullApoints = new List<Point3d[]>();
            List<double> fullErrors = new List<double>();

            // 행렬 A
            double[,] A = new double[m, n];

            int itrCnt = 0;
            while (itrCnt++ < 15)
            {
                if (motorizedMeasurementAbort) return;

                AddVsnLog($"itrCnt: {itrCnt}");
                double txRad;
                double tyRad;
                double tzRad;

                switch (axis)
                {
                    //case Axis.TX:
                    //    {
                    //        txRad = 0;
                    //        tyRad = Math.Atan2(-normal1[2], Math.Sqrt(1 - normal1[2] * normal1[2])); // Y / X
                    //        tzRad = Math.Atan2(normal1[1], normal1[0]);  // Z
                    //        break;
                    //    }

                    //case Axis.TY:
                    //    {
                    //        txRad = Math.Atan2(normal1[2], Math.Sqrt(1 - normal1[2] * normal1[2]));
                    //        tyRad = 0;
                    //        tzRad = Math.Atan2(-normal1[0], normal1[1]);
                    //        break;
                    //    }

                    //case Axis.TZ:
                    //    {
                    //        txRad = Math.Atan2(-normal1[1], Math.Sqrt(1 - normal1[1] * normal1[1]));
                    //        tyRad = Math.Atan2(normal1[0], normal1[2]);
                    //        tzRad = 0;
                    //        break;
                    //    }

                    case Axis.TX:
                        {
                            txRad = 0;
                            tyRad = Math.Atan2(-normal1[2], normal1[0]); // Y / X
                            tzRad = Math.Atan2(normal1[1], Math.Sqrt(normal1[0] * normal1[0] + normal1[2] * normal1[2]));  // Z
                            break;
                        }

                    case Axis.TY:
                        {
                            txRad = Math.Atan2(normal1[2], normal1[1]);
                            tyRad = 0;
                            tzRad = Math.Atan2(-normal1[0], Math.Sqrt(normal1[1] * normal1[1] + normal1[2] * normal1[2]));
                            break;
                        }

                    case Axis.TZ:
                        {
                            txRad = Math.Atan2(-normal1[1], normal1[2]);
                            tyRad = Math.Atan2(normal1[0], Math.Sqrt(normal1[1] * normal1[1] + normal1[2] * normal1[2]));
                            tzRad = 0;
                            break;
                        }
                    default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, "지원하지 않는 axis입니다.");
                }

                double txArcmin = -(txRad * 180.0 / Math.PI * 60.0);
                double tyArcmin = -(tyRad * 180.0 / Math.PI * 60.0);
                double tzArcmin = -(tzRad * 180.0 / Math.PI * 60.0);

                MotorSetPivot(mHexapodPivots[(int)(axis - 3)].X, mHexapodPivots[(int)(axis - 3)].Y, mHexapodPivots[(int)(axis - 3)].Z + zOffsetPivot);
                // 축 각도 설정
                MotorSetHCS(txArcmin, tyArcmin, tzArcmin);  
                PrismCSRotations[(int)(axis - 3)] = new Point3d(txArcmin, tyArcmin, tzArcmin);

                // 디버깅
                fullNormal1.Add(new Point3d(normal1[0], normal1[1], normal1[2]));
                fullArcmin.Add(new Point3d(-txArcmin, -tyArcmin, -tzArcmin));
                 
                //

                mCalibrationFullData.Clear();
                mGageFullData.Clear();

                double orgPos = MotorCurPosAxis(axis);

                foreach (var target in targetList)
                {
                    MotorMoveAbsAxis(axis, orgPos + target * 60);
                    Thread.Sleep(500);
                    SingleFindMark();
                }
                MotorMoveAbsAxis(axis, orgPos);                

                // bZero
                double[] bZero = new double[m];

                // M 행렬: [x, y, z, 1]
                Point3d[] Apoints = new Point3d[m];
                var M = Matrix<double>.Build.Dense(m, 4);
                for (int i = 0; i < m; i++)
                {
                    M[i, 0] = mCalibrationFullData[i][0];   // um
                    M[i, 1] = mCalibrationFullData[i][1];   // um
                    M[i, 2] = mCalibrationFullData[i][2];   // um
                    M[i, 3] = 1.0;
                    Apoints[i] = new Point3d(M[i, 0], M[i, 1], M[i, 2]);
                }
                fullApoints.Add(Apoints);
                // SVD 수행
                var svd = M.Svd(true);
                MathNet.Numerics.LinearAlgebra.Vector<double> plane = svd.VT.Row(svd.VT.RowCount - 1);

                // 정규화 (optional)
                var normal = plane.SubVector(0, 3);
                plane = plane / normal.L2Norm();
                normalm = new double[] { plane[0], plane[1], plane[2]};
                double L = Math.Sqrt(normalm[0] * normalm[0] + normalm[1] * normalm[1] + normalm[2] * normalm[2]);

                //////for (int i = 0; i < m; i++)
                //////{
                //////    //  기존  ax + by + cz + 1 = 0;
                //////    //A[i, 0] = mCalibrationFullData[i][0];   // um
                //////    //A[i, 1] = mCalibrationFullData[i][1];   // um
                //////    //A[i, 2] = mCalibrationFullData[i][2];   // um

                //////    //bZero[i] = -1.0;

                //////    //  수정식 z = ax + by + d
                //////    A[i, 0] = mCalibrationFullData[i][0];   // um
                //////    A[i, 1] = mCalibrationFullData[i][1];   // um
                //////    A[i, 2] = 1;   // um

                //////    bZero[i] = mCalibrationFullData[i][2];

                //////}

                //////// 디버깅
                //////fullApoints.Add(Apoints);
                ////////

                //////// 평면방정식(a, b, c, d)를 구함 ( Least Squares 방식)
                //////double[,] AT = new double[n, m];
                //////m__G.mFAL.mFZM.MatrixTranspose(A, ref AT, n, m);

                //////double[,] AT_A_Inverse = new double[n, n];
                //////m__G.mFAL.mFZM.MatrixCross(ref AT, ref A, ref AT_A_Inverse, n, m);
                //////m__G.mFAL.mFZM.InverseU(ref AT_A_Inverse, n);

                //////double[] AT_bZero = new double[n];
                //////m__G.mFAL.mFZM.MatrixCross(ref AT, ref bZero, ref AT_bZero, n, m);

                //////// 법선벡터 기존
                ////////normalm = new double[n];
                ////////m__G.mFAL.mFZM.MatrixCross(ref AT_A_Inverse, ref AT_bZero, ref normalm, n);

                //////// 법선벡터 수정식
                //////double[] normalVector = new double[n];
                //////m__G.mFAL.mFZM.MatrixCross(ref AT_A_Inverse, ref AT_bZero, ref normalVector, n);
                //////normalm = new double[3] { normalVector[0], normalVector[1], -1 };

                //////double L = Math.Sqrt(normalm[0] * normalm[0] + normalm[1] * normalm[1] + normalm[2] * normalm[2]);
                //////normalm = new double[] { normalm[0] / L, normalm[1] / L, normalm[2] / L };

                // 두 벡터의 내적 계산
                double dot = normal0[0] * normalm[0] + normal0[1] * normalm[1] + normal0[2] * normalm[2];
                // 방향이 반대이면 내적이 < 0 → 뒤집어야 함
                if (dot < 0)
                {
                    for (int i = 0; i < 3; i++)
                        normalm[i] *= -1;  // 방향 반전
                }
                //  방향 오차 계산 ( arcmin )
                double errNmin = Math.Asin(Math.Sqrt((normal0[0] - normalm[0])* (normal0[0] - normalm[0]) + (normal0[1] - normalm[1]) * (normal0[1] - normalm[1]) + (normal0[2] - normalm[2]) * (normal0[2] - normalm[2]))/2) *180 *60/Math.PI; //  min
                fullErrors.Add(errNmin);    
                // 디버깅
                fullNormalm.Add(new Point3d(normalm[0], normalm[1], normalm[2]));
                //
                // 벡터 차: N0 - Nm
                double[] delta = new double[n];
                for (int i = 0; i < n; i++)
                {
                    delta[i] = normal0[i] - normalm[i];
                }

                // 오차 0.1deg  이하
                if (errNmin < 6) 
                    break;

                for (int i = 0; i < n; i++)
                {
                    if (errNmin>40)
                        normal1[i] += 0.5 * delta[i];
                    else if (errNmin > 10)
                        normal1[i] += 0.25 * delta[i];
                    else 
                        normal1[i] += 0.15 * delta[i];
                }

                L = Math.Sqrt(normal1[0] * normal1[0] + normal1[1] * normal1[1] + normal1[2] * normal1[2]);
                normal1 = new double[] { normal1[0] / L, normal1[1] / L, normal1[2] / L };
            }

            MotorSetPivot(0, 0, 0);

            string strStabilizedFile = $"C:\\CSHTest\\DoNotTouch\\Admin\\normal.csv";
            //  Reset File
            //StreamWriter wr = new StreamWriter(strStabilizedFile);
            //wr.Close();

            string slstr = $"Normal1,X,Y,Z,Arcmin,X,Y,Z,Normalm,X,Y,Z,,,,,Error(min)\r\n";
            for(int i = 0; i < fullNormal1.Count; i++)
            {
                slstr += $"{i},{fullNormal1[i].X:F7},{fullNormal1[i].Y:F7},{fullNormal1[i].Z:F7}," +
                         $"{i},{fullArcmin[i].X:F7},{fullArcmin[i].Y:F7},{fullArcmin[i].Z:F7}," +
                         $"{i},{fullNormalm[i].X:F7},{fullNormalm[i].Y:F7},{fullNormalm[i].Z:F7},,,,,{fullErrors[i]:F7}\r\n";
            }

            slstr += "A,X,Y,Z\r\n";
            for (int i = 0; i < fullApoints.Count; i++)
            {
                for (int j=0; j< fullApoints[i].Length; j++)
                {
                    slstr += $"{j},{fullApoints[i][j].X:F7},{fullApoints[i][j].Y:F7},{fullApoints[i][j].Z:F7}\r\n";

                }
            }
            File.AppendAllText(strStabilizedFile, slstr);
        }

        public void FindPrismCSPivot(Axis axis)
        {
            if (!m__G.m_bPrismCS) return;

            FindCSHorg();

            Point3d pivot = new Point3d(mHexapodPivots[(int)(axis - 3)].X, mHexapodPivots[(int)(axis - 3)].Y, mHexapodPivots[(int)(axis - 3)].Z );
            Point3d prismCSRotations = PrismCSRotations[(int)(axis - 3)];
            double[] orgPos = MotorCurPos6D();

            double rad45 = 45 * Math.PI / 180;

            switch (axis)
            {
                case Axis.TX:
                    {
                        double[,] T = BuildLocalFrameByAxis(axis, 0);

                        double[] tagetPos = new double[5] { -165, -160, 0, 160, 0 };   //  min   //  Probe 비대칭성때문에 임시로 범위조정함 160 이상 측정 불가.
                        
                        double dy = 0;
                        double dz = 0;
                        double angle = 0;

                        double[] Xref = new double[2];
                        double[] X0 = new double[2];
                        double[] X1 = new double[2];
                        double[,] RX = new double[2, 2];
                        double[] RX0_X1 = new double[2];
                        double[] resPivot = new double[2];

                        int itrCnt = 0;

                        while (itrCnt++ < 10)
                        {
                            MotorSetPivot(pivot.X, pivot.Y, pivot.Z);
                            PrismCSPivots[(int)(axis - 3)] = new Point3d(pivot.X, pivot.Y, pivot.Z);
                            MotorSetHCS(prismCSRotations.X, prismCSRotations.Y, prismCSRotations.Z) ;

                            mCalibrationFullData.Clear();
                            mGageFullData.Clear();

                            for (int i = 0; i < 5; i++)
                            {
                                MotorMoveAbsAxis(Axis.TX, orgPos[3] + tagetPos[i]);
                                if (i > 0 && i < 4)
                                {
                                    Thread.Sleep(600);
                                }
                                SingleFindMark(true);
                            }

                            ///////////////////////////////////////////////////////////////////
                            double[] Pref = { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            double[] Pref_local = Transform(T, Pref, true);

                            double[] P0 = { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            double[] P1 = { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            double[] P0_local = Transform(T, P0, true);
                            double[] P1_local = Transform(T, P1, true);

                            // Pivot 결과 확인
                            dy = P1_local[1] - P0_local[1];
                            dz = P1_local[2] - P0_local[2];

                            AddVsnLog($"itr:{itrCnt}\tdy:{dy}\tdz:{dz}");
                            if (Math.Abs(dy) < 0.1 && Math.Abs(dz) < 0.1) break;
                            ///////////////////////////////////////////////////////////////////

                            // 0도일때 CSH (X,Y)
                            Xref[0] = Pref_local[1];
                            Xref[1] = -Pref_local[2];

                            ///////////////////////////////////////////////////////////////////

                            // Pivto XA 계산 
                            angle = mCalibrationFullData[3][19] - mCalibrationFullData[1][19];  //  min // Probe TX

                            X0[0] = P0_local[1]; //  Y pos um
                            X0[1] = -P0_local[2]; //  Z pos um

                            X1[0] = P1_local[1]; //  Y pos um
                            X1[1] = -P1_local[2]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  2차원 회전행렬
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.Y = - (resPivot[0] - Xref[0]);
                            pivot.Z =  (resPivot[1] - Xref[1]);

                            ///////////////////////////////////////////////////////////////////
                            P0 = new double[] { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            P1 = new double[] { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            // Pivto XA 계산 
                            angle = mCalibrationFullData[2][19] - mCalibrationFullData[1][19];  //  min

                            X0[0] = P0_local[1]; //  Y pos um
                            X0[1] = -P0_local[2]; //  Z pos um

                            X1[0] = P1_local[1]; //  Y pos um
                            X1[1] = -P1_local[2]; //  Z pos um
                            
                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.Y += - (resPivot[0] - Xref[0]);
                            pivot.Z +=  (resPivot[1] - Xref[1]);

                            ///////////////////////////////////////////////////////////////////
                            ///
                            P0 = new double[] { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P1 = new double[] { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            // Pivto XA 계산 
                            angle = mCalibrationFullData[3][19] - mCalibrationFullData[2][19];  //  min

                            X0[0] = P0_local[1]; //  Y pos um
                            X0[1] = -P0_local[2]; //  Z pos um

                            X1[0] = P1_local[1]; //  Y pos um
                            X1[1] = -P1_local[2]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);  //  
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.Y +=  - (resPivot[0] - Xref[0]);
                            pivot.Z += (resPivot[1] - Xref[1]);

                            pivot = new Point3d(0, pivot.Y/3, pivot.Z/3);
                            double[] pivot_world = Transform(T, new double[] { pivot.X, pivot.Y, pivot.Z } , false);
                            pivot = new Point3d(pivot_world[0]+ PrismCSPivots[0].X, pivot_world[1] + PrismCSPivots[0].Y, pivot_world[2] + PrismCSPivots[0].Z);
                            pivot.X = Math.Abs(pivot.X) > 4000 ? 4000 * Math.Sign(pivot.X) : pivot.X;
                            pivot.Y = Math.Abs(pivot.Y) > 4000 ? 4000 * Math.Sign(pivot.Y) : pivot.Y;
                        }
                        
                        break;
                    }
                case Axis.TY:
                    {
                        double[,] T = BuildLocalFrameByAxis(axis, 45 * Math.PI / 180);
                        double[] tagetPos = new double[5] { -235, -230, 0, 230, 0 };   //  min

                        double dx = 0;
                        double dz = 0;
                        double angle = 0;

                        double[] Xref = new double[2];
                        double[] X0 = new double[2];
                        double[] X1 = new double[2];
                        double[,] RX = new double[2, 2];
                        double[] RX0_X1 = new double[2];
                        double[] resPivot = new double[2];

                        int itrCnt = 0;

                        while (itrCnt++ < 10)
                        {
                            MotorSetPivot(pivot.X, pivot.Y, pivot.Z);
                            PrismCSPivots[(int)(axis - 3)] = new Point3d(pivot.X, pivot.Y, pivot.Z);
                            MotorSetHCS(prismCSRotations.X, prismCSRotations.Y, prismCSRotations.Z);

                            mCalibrationFullData.Clear();
                            mGageFullData.Clear();

                            for (int i = 0; i < 5; i++)
                            {
                                MotorMoveAbsAxis(Axis.TY, orgPos[4] + tagetPos[i]);
                                if (i > 0 && i < 4)
                                {
                                    Thread.Sleep(600);
                                }
                                SingleFindMark(true);
                            }

                            double[] Pref = { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            double[] Pref_local = Transform(T, Pref, true);

                            double[] P0 = { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            double[] P1 = { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            double[] P0_local = Transform(T, P0, true);
                            double[] P1_local = Transform(T, P1, true);

                            // Pivot 결과 확인
                            dx = P1_local[0] - P0_local[0];
                            dz = P1_local[2] - P0_local[2];

                            AddVsnLog($"itr:{itrCnt}\tdx:{dx}\tdz:{dz}");
                            if (Math.Abs(dx) < 0.1 && Math.Abs(dz) < 0.1) break;
                            ///////////////////////////////////////////////////////////////////
                            ///
                            Xref[0] = Pref_local[0];
                            Xref[1] = Pref_local[2];

                            // XA 계산 및 출력
                            var angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[1][19], mCalibrationFullData[1][20], mCalibrationFullData[1][21], true, true)[2];
                            var angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[3][19], mCalibrationFullData[3][20], mCalibrationFullData[3][21], true, true)[2];
                            angle = angle1 - angle0;  //  min

                            X0[0] = P0_local[0]; //  X pos um
                            X0[1] = P0_local[2]; //  Z pos um

                            X1[0] = P1_local[0]; //  X pos um
                            X1[1] = P1_local[2]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X = -(resPivot[0] - Xref[0]);
                            pivot.Z = -(resPivot[1] - Xref[1]);

                            ///////////////////////////////////////////////////////////////////

                            // XA 계산 및 출력
                            P0 = new double[] { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            P1 = new double[] { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[1][19], mCalibrationFullData[1][20], mCalibrationFullData[1][21], true, true)[2];
                            angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[2][19], mCalibrationFullData[2][20], mCalibrationFullData[2][21], true, true)[2];
                            angle = angle1 - angle0;  //  min


                            X0[0] = P0_local[0]; //  X pos um
                            X0[1] = P0_local[2]; //  Z pos um

                            X1[0] = P1_local[0]; //  X pos um
                            X1[1] = P1_local[2]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X += -(resPivot[0] - Xref[0]);
                            pivot.Z += -(resPivot[1] - Xref[1]);

                            ///////////////////////////////////////////////////////////////////

                            // XA 계산 및 출력
                            P0 = new double[]{ mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P1 = new double[] { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[2][19], mCalibrationFullData[2][20], mCalibrationFullData[2][21], true, true)[2];
                            angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[3][19], mCalibrationFullData[3][20], mCalibrationFullData[3][21], true, true)[2];
                            angle = angle1 - angle0;  //  min

                            X0[0] = P0_local[0]; //  X pos um
                            X0[1] = P0_local[2]; //  Z pos um

                            X1[0] = P1_local[0]; //  X pos um
                            X1[1] = P1_local[2]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X += -(resPivot[0] - Xref[0]);
                            pivot.Z += -(resPivot[1] - Xref[1]);

                            pivot = new Point3d(pivot.X / 3, 0, pivot.Z / 3);
                            double[] pivot_world = Transform(T, new double[] { pivot.X, pivot.Y, pivot.Z }, false);
                            pivot = new Point3d(pivot_world[0] + PrismCSPivots[1].X, pivot_world[1]+ PrismCSPivots[1].Y, pivot_world[2]+ PrismCSPivots[1].Z);
                            pivot.X = Math.Abs(pivot.X) > 4000 ? 4000 * Math.Sign(pivot.X) : pivot.X;
                            pivot.Y = Math.Abs(pivot.Y) > 4000 ? 4000 * Math.Sign(pivot.Y) : pivot.Y;
                        }
                        break;
                    }
                case Axis.TZ:
                    {
                        double[,] T = BuildLocalFrameByAxis(axis, 45 * Math.PI / 180);
                        double[] tagetPos = new double[5] { -245, -240, 0, 240, 0 };   //  min

                        double dx = 0;
                        double dy = 0;
                        double angle = 0;

                        double[] Xref = new double[2];
                        double[] X0 = new double[2];
                        double[] X1 = new double[2];
                        double[,] RX = new double[2, 2];
                        double[] RX0_X1 = new double[2];
                        double[] resPivot = new double[2];

                        int itrCnt = 0;

                        while (itrCnt++ < 10)
                        {
                            MotorSetPivot(pivot.X, pivot.Y, pivot.Z);
                            PrismCSPivots[(int)(axis - 3)] = new Point3d(pivot.X, pivot.Y, pivot.Z);
                            MotorSetHCS(prismCSRotations.X, prismCSRotations.Y, prismCSRotations.Z);

                            mCalibrationFullData.Clear();
                            mGageFullData.Clear();

                            for (int i = 0; i < 5; i++)
                            {
                                MotorMoveAbsAxis(Axis.TZ, orgPos[5] + tagetPos[i]);
                                if (i > 0 && i < 4)
                                {
                                    Thread.Sleep(600);
                                }
                                SingleFindMark(true);
                            }

                            double[] Pref = { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            double[] Pref_local = Transform(T, Pref, true);

                            double[] P0 = { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            double[] P1 = { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            double[] P0_local = Transform(T, P0, true);
                            double[] P1_local = Transform(T, P1, true);

                            // Pivot 결과 확인
                            dx = P1_local[0] - P0_local[0];
                            dy = P1_local[1] - P0_local[1];

                            AddVsnLog($"itr:{itrCnt}\tdx:{dx}\tdy:{dy}");
                            if (Math.Abs(dx) < 0.1 && Math.Abs(dy) < 0.1) break;

                            ///////////////////////////////////////////////////////////////////
                            ///

                            Xref[0] = -Pref_local[0];
                            Xref[1] = -Pref_local[1];

                            // XA 계산 및 출력
                            var angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[1][19], mCalibrationFullData[1][20], mCalibrationFullData[1][21], true, true)[0];
                            var angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[3][19], mCalibrationFullData[3][20], mCalibrationFullData[3][21], true, true)[0];
                            angle = angle1 - angle0;  //  min

                            X0[0] = -P0_local[0]; //  X pos um
                            X0[1] = -P0_local[1]; //  Z pos um

                            X1[0] = -P1_local[0]; //  X pos um
                            X1[1] = -P1_local[1]; //  Z pos um


                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X = + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                            pivot.Y = + (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                            ///////////////////////////////////////////////////////////////////
                            ///

                            // XA 계산 및 출력
                            // XA 계산 및 출력
                            P0 = new double[] { mCalibrationFullData[1][0], mCalibrationFullData[1][1], mCalibrationFullData[1][2] };
                            P1 = new double[] { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[1][19], mCalibrationFullData[1][20], mCalibrationFullData[1][21], true, true)[0];
                            angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[2][19], mCalibrationFullData[2][20], mCalibrationFullData[2][21], true, true)[0];
                            angle = angle1 - angle0;  //  min


                            X0[0] = -P0_local[0]; //  X pos um
                            X0[1] = -P0_local[1]; //  Y pos um

                            X1[0] = -P1_local[0]; //  X pos um
                            X1[1] = -P1_local[1]; //  Y pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X +=  + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                            pivot.Y +=  + (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                            ///////////////////////////////////////////////////////////////////
                            ///

                            // XA 계산 및 출력
                            P0 = new double[] { mCalibrationFullData[2][0], mCalibrationFullData[2][1], mCalibrationFullData[2][2] };
                            P1 = new double[] { mCalibrationFullData[3][0], mCalibrationFullData[3][1], mCalibrationFullData[3][2] };
                            P0_local = Transform(T, P0, true);
                            P1_local = Transform(T, P1, true);

                            angle0 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[2][19], mCalibrationFullData[2][20], mCalibrationFullData[2][21], true, true)[0];
                            angle1 = -m__G.oCam[0].mFAL.mFZM.ConvertTXTYTZofCSHtoPrism(mCalibrationFullData[3][19], mCalibrationFullData[3][20], mCalibrationFullData[3][21], true, true)[0];
                            angle = angle1 - angle0;  //  min

                            X0[0] = - P0_local[0]; //  X pos um
                            X0[1] = - P0_local[1]; //  Z pos um

                            X1[0] = - P1_local[0]; //  X pos um
                            X1[1] = - P1_local[1]; //  Z pos um

                            m__G.mFAL.mFZM.RotationZ2x2(angle * MIN_To_RAD, ref RX);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref X0, ref RX0_X1, 2);

                            RX0_X1[0] -= X1[0];
                            RX0_X1[1] -= X1[1];

                            RX[0, 0] -= 1;
                            RX[1, 1] -= 1;

                            m__G.mFAL.mFZM.InverseU(ref RX, 2);
                            m__G.mFAL.mFZM.MatrixCross(ref RX, ref RX0_X1, ref resPivot, 2);

                            pivot.X += + (resPivot[0] - Xref[0]); //  dy 에 영향 준다.
                            pivot.Y += + (resPivot[1] - Xref[1]); //  dx 에 영향 준다.

                            pivot = new Point3d(pivot.X / 3, pivot.Y / 3, 0);
                            double[] pivot_world = Transform(T, new double[] { pivot.X, pivot.Y, pivot.Z }, false);
                            pivot = new Point3d(pivot_world[0] + PrismCSPivots[2].X, pivot_world[1] + PrismCSPivots[2].Y, pivot_world[2] + PrismCSPivots[2].Z);

                            pivot.X = Math.Abs(pivot.X) > 4000 ? 4000 * Math.Sign(pivot.X) : pivot.X;
                            pivot.Y = Math.Abs(pivot.Y) > 4000 ? 4000 * Math.Sign(pivot.Y) : pivot.Y;

                            ///////////////////////////////////////////////////////////////////
                        }
                        break;
                    }
                default:
                    break;
            }

            // 피봇 초기화
            HexapodRotate(0, 0, 0);
            MotorSetPivot(0, 0, 0);
        }

        // 유틸리티. FZCoefficient에서 찾아서 바꿔놓을것.
        double[] Normalize(double[] v)
        {
            double len = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            return new double[] { v[0] / len, v[1] / len, v[2] / len };
        }

        // 내적
        double Dot(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        // 외적
        double[] Cross(double[] a, double[] b)
        {
            return new double[]
            {
            a[1]*b[2] - a[2]*b[1],
            a[2]*b[0] - a[0]*b[2],
            a[0]*b[1] - a[1]*b[0]
                };
        }

        // 케이스별 프레임 구성: 열이 [X Y Z]
        // TX:  U=X=(1,0,0),  Y≈(0,1,0)
        // TY:  U=Y=(0,cos45,-sin45),  X≈(1,0,0)
        // TZ:  U=Z=(0,cos45, sin45),  X≈(1,0,0)
        double[,] BuildLocalFrameByAxis(Axis axis, double rad45)
        {
            double[] X, Y, Z;

            switch (axis)
            {
                case Axis.TX:
                    {
                        var U = Normalize(new double[] { 1, 0, 0 }); // U = X
                        X = U;

                        var prefY = new double[] { 0, 1, 0 };       // Y를 (0,1,0)에 가깝게
                        var proj = Dot(prefY, U);
                        var Y0 = new double[] { prefY[0] - proj * U[0], prefY[1] - proj * U[1], prefY[2] - proj * U[2] };
                        Y = Normalize(Y0);

                        Z = Normalize(Cross(X, Y));                // X×Y=Z (오른손)
                        break;
                    }

                case Axis.TY:
                    {
                        var U = Normalize(new double[] { 0, Math.Cos(rad45), -Math.Sin(rad45) }); // U = Y
                        Y = U;

                        var prefX = new double[] { 1, 0, 0 };       // X를 (1,0,0)에 가깝게
                        var proj = Dot(prefX, U);
                        var X0 = new double[] { prefX[0] - proj * U[0], prefX[1] - proj * U[1], prefX[2] - proj * U[2] };
                        X = Normalize(X0);

                        Z = Normalize(Cross(X, Y));                // X×Y=Z
                        break;
                    }

                case Axis.TZ:
                    {
                        var U = Normalize(new double[] { 0, Math.Cos(rad45), Math.Sin(rad45) }); // U = Z
                        Z = U;

                        var prefX = new double[] { 1, 0, 0 };       // X를 (1,0,0)에 가깝게
                        var proj = Dot(prefX, U);
                        var X0 = new double[] { prefX[0] - proj * U[0], prefX[1] - proj * U[1], prefX[2] - proj * U[2] };
                        X = Normalize(X0);

                        Y = Normalize(Cross(Z, X));                // X×Y=Z → Y=Z×X
                        break;
                    }

                default:
                    {
                        X = new double[] { 1, 0, 0 };
                        Y = new double[] { 0, 1, 0 };
                        Z = new double[] { 0, 0, 1 };
                        break;
                    }
            }

            return new double[,]
            {
                { X[0], Y[0], Z[0] },
                { X[1], Y[1], Z[1] },
                { X[2], Y[2], Z[2] }
            };
        }

        double[] Transform(double[,] T, double[] p, bool toLocal)
        {
            double[] res = new double[3];

            if (toLocal)
            {
                // 로컬 = Tᵀ * 월드
                for (int i = 0; i < 3; i++)
                    res[i] = T[0, i] * p[0] + T[1, i] * p[1] + T[2, i] * p[2];
            }
            else
            {
                // 월드 = T * 로컬
                for (int i = 0; i < 3; i++)
                    res[i] = T[i, 0] * p[0] + T[i, 1] * p[1] + T[i, 2] * p[2];
            }

            return res;
        }

        public void FindPrismCSPivot(Axis axis, double zOffsetPivot)
        {
            FindCSHorg();

            double offsetPivot = zOffsetPivot;
            List<double> targetList = new List<double>();
            double stroke = 3.0;
            double temp = -stroke;

            while (temp <= stroke)
            {
                targetList.Add(temp);
                temp += 0.5;
            }
            int m = targetList.Count;
            int n = 3;

            double[] normalm = new double[n];   // 실제 법선 벡터

            // 행렬 A
            double[,] A = new double[m, n];
            Point3d pivot = new Point3d(mHexapodPivots[(int)(axis - 3)].X, mHexapodPivots[(int)(axis - 3)].Y, mHexapodPivots[(int)(axis - 3)].Z+ offsetPivot);

            // 디버깅                       
            List<Point3d> fullPivot = new List<Point3d>();
            List<Point3d> fullCircle = new List<Point3d>();
            List<Point3d[]> fullApoints = new List<Point3d[]>();
            List<Point3d> fullNormalm = new List<Point3d>();   // 설정할 벡터 -> 각도
            List<double> fullErrors = new List<double>();
            List<double> fullRadius = new List<double>();

            bool isVert = false;
            int itrCnt = 0;
            while (itrCnt++ < 10)
            {
                if (motorizedMeasurementAbort) return;
                mCalibrationFullData.Clear();
                mGageFullData.Clear();

                MotorSetPivot(pivot.X, pivot.Y, pivot.Z);
                PrismCSPivots[(int)(axis - 3)] = new Point3d(pivot.X, pivot.Y, pivot.Z);
                // 디버깅
                fullPivot.Add(pivot);
                //
                MotorSetHCS(PrismCSRotations[(int)(axis - 3)].X, PrismCSRotations[(int)(axis - 3)].Y, PrismCSRotations[(int)(axis - 3)].Z);

                double orgPos = MotorCurPosAxis(axis);

                foreach (var target in targetList)
                {
                    MotorMoveAbsAxis(axis, orgPos + target * 60);
                    Thread.Sleep(500);
                    SingleFindMark();
                }
                MotorMoveAbsAxis(axis, orgPos);

                // bZero
                Point3d[] Apoints = new Point3d[m];
                double[,] ApointsM = new double[m, 3];
                var M = Matrix<double>.Build.Dense(m, 4);
                for (int i = 0; i < m; i++)
                {
                    M[i, 0] = mCalibrationFullData[i][0];   // um
                    M[i, 1] = mCalibrationFullData[i][1];   // um
                    M[i, 2] = mCalibrationFullData[i][2];   // um
                    ApointsM[i, 0] = mCalibrationFullData[i][0];   // um
                    ApointsM[i, 1] = mCalibrationFullData[i][1];   // um
                    ApointsM[i, 2] = mCalibrationFullData[i][2];   // um
                    M[i, 3] = 1.0;
                    Apoints[i] = new Point3d(M[i, 0], M[i, 1], M[i, 2]);
                }
                fullApoints.Add(Apoints);

                normalm = new double[3];
                double[] directCircle = m__G.mFAL.mFZM.PseudoCircle(ApointsM, ref normalm);

                fullNormalm.Add(new Point3d(normalm[0], normalm[1], normalm[2]));
                fullCircle.Add(new Point3d(directCircle[0], directCircle[1], directCircle[2]));

                // center
                Point3d center0;
                switch (axis)
                {
                    case Axis.TX:
                        center0 = new Point3d(0, 0, offsetPivot);
                        break;
                        
                    case Axis.TY:
                        center0 = new Point3d(0, offsetPivot / 2, offsetPivot / 2);
                        break;
                    case Axis.TZ:
                        center0 = new Point3d(0, -offsetPivot / 2, offsetPivot / 2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axis), axis, "지원하지 않는 axis입니다.");
                }
            
                double h = center0.X- directCircle[0];
                double k = center0.Y - directCircle[1];
                double l = center0.Z - directCircle[2];
                double r = directCircle[3];
                fullRadius.Add(r);

                double errPos = Math.Sqrt(h * h + k * k + l * l);
                fullErrors.Add(errPos);

                if (!isVert)
                {
                    if (errPos <= 30 || itrCnt == 10)
                    {
                        isVert = true;
                        itrCnt = 6;
                    }
                    else
                    {
                        pivot = new Point3d(pivot.X + h * 0.4, pivot.Y + k * 0.4, pivot.Z + l * 0.4);
                    }
                }
            }

            MotorSetPivot(0, 0, 0);

            string strStabilizedFile = $"C:\\CSHTest\\DoNotTouch\\Admin\\circle.csv";
            //StreamWriter wr = new StreamWriter(strStabilizedFile);
            //wr.Close();

            string slstr = $"Pivot, X, Y, Z, Circle, X, Y, Z,Normalm,,,,,Radius(um),,,Error(um) \r\n";
            for (int i = 0; i < fullPivot.Count; i++)
            {
                slstr += $"{i},{fullPivot[i].X:F7},{fullPivot[i].Y:F7},{fullPivot[i].Z:F7}," +
                         $"{i},{fullCircle[i].X:F7},{fullCircle[i].Y:F7},{fullCircle[i].Z:F7}," +
                         $"{i},{fullNormalm[i].X:F7},{fullNormalm[i].Y:F7},{fullNormalm[i].Z:F7},,{fullRadius[i]:F7},,,{fullErrors[i]:F7}," + "\r\n";
            }
            slstr += "A,X,Y,Z\r\n";
            for (int i = 0; i < fullApoints.Count; i++)
            {
                for (int j = 0; j < fullApoints[i].Length; j++)
                {
                    slstr += $"{j},{fullApoints[i][j].X:F7},{fullApoints[i][j].Y:F7},{fullApoints[i][j].Z:F7}\r\n";
                }
            }
            File.AppendAllText(strStabilizedFile, slstr);
        }

        bool lgTest = false;
        bool manuelLED = false;

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (motorizedMeasurementRun)
            {
                motorizedMeasurementAbort = true;
                btnTest.Enabled = false;
                return;
            }
            motorizedMeasurementRun = true;
            btnTest.Text = "Stop";

            await Task.Run(() =>
                       {
                           Axis axis = Axis.Y;

                           string imgFilePath = $"{m__G.m_RootDirectory}\\Result\\Test\\";
                           string imgFileName = "";
                           if (!Directory.Exists(imgFilePath))
                           {
                               Directory.CreateDirectory(imgFilePath);
                           }

                           //MotorMoveAbs6D(mCSHorg.X, mCSHorg.Y, mCSHorg.Z, 0, 0, 0);
                           double orgPos = MotorCurPosAxis(axis);
                           double pos = orgPos;
                           double step = 100;
                           double movingStroke = 0;
                           while (movingStroke >= -1900)
                           {
                               if (motorizedMeasurementAbort) return;
                               MotorMoveAbsAxis(axis, pos);
                               Thread.Sleep(1000);
                               SingleFindMark();

                               imgFileName = imgFilePath + $"Y_{movingStroke}.bmp";
                               m__G.oCam[0].SaveGrabbedImage(1, imgFileName);                          

                               pos -= step;
                               movingStroke -= step;
                           }

                           if (motorizedMeasurementAbort) return;
                           pos = orgPos - 1400;
                           MotorMoveAbsAxis(axis, pos);
                           Thread.Sleep(1000);
                           SingleFindMark();
                           imgFileName = imgFilePath + $"Y_-1000_check.bmp";
                           m__G.oCam[0].SaveGrabbedImage(1, imgFileName);

                           if (motorizedMeasurementAbort) return;
                           pos = orgPos;
                           MotorMoveAbsAxis(axis, pos);

                           //Thread.Sleep(1000);
                           //SingleFindMark();
                           //imgFileName = imgFilePath + $"Y_0_check.bmp";
                           //m__G.oCam[0].SaveGrabbedImage(1, imgFileName);

                       });

            motorizedMeasurementRun = false;
            motorizedMeasurementAbort = false;
            btnTest.Enabled = true;
            btnTest.Text = "Test";

        }

        private void cboAxis_RightToLeftChanged(object sender, EventArgs e)
        {

        }



        //private async void btnTest_Click(object sender, EventArgs e)
        //{
        //    if (motorizedMeasurementRun)
        //    {
        //        motorizedMeasurementAbort = true;
        //        btnTest.Enabled = false;
        //        lgTest= false;
        //        return;
        //    }

        //    motorizedMeasurementRun = true;
        //    btnTest.Text = "Stop";
        //    lgTest = true;

        //    await Task.Run(() =>
        //           {
        //               if (motorizedMeasurementAbort) return;

        //               if (chkProbeReset.Checked)
        //               {
        //                   AddVsnLog("Start to find CSHorg.");
        //                   FindCSHorg();  

        //                   for (Axis pivotAxis = Axis.TX; pivotAxis <= Axis.TZ; pivotAxis++)
        //                   {
        //                       if (motorizedMeasurementAbort) return;
        //                       AddVsnLog($"Start to find {pivotAxis} pivot.");
        //                       FindPivot(pivotAxis);
        //                   }

        //                   SavePivots();


        //                   if (motorizedMeasurementAbort) return;
        //                   AddVsnLog("Start to find CSHorg, Reset Probe.");
        //                   FindCSHorg(true);   // Probe 리셋

        //                   if (motorizedMeasurementAbort) return;
        //                   AddVsnLog("Start to find Fidorg");
        //                   FindFidorg();

        //                   SaveOQCCondition();
        //               }


        //               Axis axis = Axis.TZ;

        //               LoadOQCcondition();
        //               LoadPivotXYZ();
        //               MotorSetPivot(mHexapodPivots[(int)axis-3].X-500, mHexapodPivots[(int)axis - 3].Y-500, mHexapodPivots[(int)axis - 3].Z);  // 마크중심에서 y -100 떨어진 점.
        //               mAutoCalibrationIndex = 0;
        //               SingleFindMark();

        //               var orgPos = MotorCurPosAxis(axis); // tz축 회전
        //               double onewayStroke = 0.2 * 60; //0.2
        //               double step = 0.005 * 60;
        //               int cntRepeat = 35;  // 35
        //               double pos = 0;

        //               mDataFile100 = $"Test_{DateTime.Now:yyMMdd_HHmmss}";

        //               for (int i = 0; i < 3; i++)
        //               {
        //                   // backlash 제거를 위한 이동
        //                   double[] backlashPos = new double[] { 15, 10, 5 };
        //                   foreach (var backlash in backlashPos)
        //                   {
        //                       pos = orgPos - onewayStroke - backlash;
        //                       MotorMoveAbsAxis(axis, pos);
        //                       Thread.Sleep(300);
        //                       SingleFindMark();
        //                   }

        //                   // 누적된 데이터 Clear
        //                   mGageFullData.Clear();
        //                   mCalibrationFullData.Clear();
        //                   mPrismTXTYTZ.Clear();
        //                   mStdevTXTYTZ.Clear();

        //                   double movingStroke = -onewayStroke;
        //                   pos = orgPos - onewayStroke;



        //                   // 진짜 측정 시작
        //                   while (Math.Round(movingStroke, 6) <= onewayStroke)
        //                   {
        //                       MotorMoveAbsAxis(axis, pos);
        //                       Thread.Sleep(600);
        //                       if (i == 0)
        //                       {
        //                           SingleFindMark();
        //                       }
        //                       else
        //                       {
        //                           manuelLED = true;
        //                           m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
        //                           m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
        //                           Thread.Sleep(50);   //  Wait LED Power Up.

        //                           for (int repeat = 0; repeat < cntRepeat; repeat++)
        //                           {
        //                               if (motorizedMeasurementAbort) return;

        //                               if (repeat < 5)
        //                               {
        //                                   SingleFindMark(false);
        //                               }
        //                               else
        //                               {
        //                                   SingleFindMark();
        //                               }

        //                           }
        //                           m__G.fGraph.Drive_LEDs(0, 0);
        //                           manuelLED = false;
        //                       }
        //                       pos += step;
        //                       movingStroke += step;
        //                   }

        //                   var stabilizedDataList = new List<List<double[]>> { mCalibrationFullData.ToList() };

        //                   if (i != 0)
        //                   {
        //                       AppendMeasuredData(stabilizedDataList, mDataFile100, "lgInnotek");
        //                   }

        //                   foreach (var backlash in backlashPos)
        //                   {
        //                       pos = orgPos + onewayStroke + backlash;
        //                       MotorMoveAbsAxis(axis, pos);
        //                       Thread.Sleep(300);
        //                       SingleFindMark();
        //                   }

        //                   mGageFullData.Clear();
        //                   mCalibrationFullData.Clear();
        //                   mPrismTXTYTZ.Clear();
        //                   mStdevTXTYTZ.Clear();

        //                   movingStroke = onewayStroke;
        //                   pos = orgPos + onewayStroke;
        //                   while (Math.Round(movingStroke, 6) >= -onewayStroke)
        //                   {
        //                       MotorMoveAbsAxis(axis, pos);
        //                       Thread.Sleep(600);
        //                       if(i == 0)
        //                       {
        //                           SingleFindMark();
        //                       }
        //                       else
        //                       {
        //                           manuelLED = true;
        //                           m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
        //                           m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
        //                           Thread.Sleep(50);   //  Wait LED Power Up.

        //                           for (int repeat = 0; repeat < cntRepeat; repeat++)
        //                           {
        //                               if (motorizedMeasurementAbort) return;

        //                               if (repeat < 5)
        //                               {
        //                                   SingleFindMark(false);
        //                               }
        //                               else
        //                               {
        //                                   SingleFindMark();
        //                               }                               
        //                           }
        //                           m__G.fGraph.Drive_LEDs(0, 0);
        //                           manuelLED = false;
        //                       }
        //                       pos -= step;
        //                       movingStroke -= step;
        //                   }


        //                    stabilizedDataList = new List<List<double[]>> { mCalibrationFullData.ToList() };

        //                   if (i != 0)
        //                   {
        //                       AppendMeasuredData(stabilizedDataList, mDataFile100, "gInnotek");
        //                   }
        //               }



        //               MotorMoveHome6D();
        //               MotorSetPivot(0, 0, 0);
        //           }
        //    );

        //    motorizedMeasurementRun = false;
        //    motorizedMeasurementAbort = false;
        //    btnTest.Enabled = true;
        //    btnTest.Text = "Test";
        //    lgTest = false;

        //}
    }
}