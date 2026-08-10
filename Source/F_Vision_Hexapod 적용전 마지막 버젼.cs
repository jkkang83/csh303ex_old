using Basler.Pylon;
using FAutoLearn;
using Matrox.MatroxImagingLibrary;
using OpenCvSharp;
using OpenCvSharp.Extensions;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static CSH030Ex.FManage;
using static FAutoLearn.FAutoLearn;
using static FAutoLearn.FZMath;
using static S2System.Vision.MILlib;

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

        public delegate void DelegateMotorHome6D();
        public delegate void DelegateMotorSetHome6D();
        public delegate void DelegateMotorMove6D(double x, double y, double z, double tx, double ty, double tz);
        public delegate void DelegateMotorSpeedSet(int x, int y, int z, int tx, int ty, int tz);
        public delegate void DelegateMotorMoveAxisAbs(int axis, double pos);

        public delegate double DelegateMotorCurPos(int axis);
        public delegate double[] DelegateMotorCurPos6axis();
        public delegate void DelegateMotorJogRun(int axis, bool dir, int speed);
        public delegate void DelegateMotorJogStop(int axis);

        DelegateMotorHome6D MotorHome6D;
        DelegateMotorSetHome6D MotorSetHome6D;
        DelegateMotorMove6D MotorMove6D;
        DelegateMotorMoveAxisAbs MotorMoveAxisAbs;
        DelegateMotorCurPos MotorCurPos;
        DelegateMotorCurPos6axis MotorCurPos6axis;
        DelegateMotorJogRun MotorJogRun;
        DelegateMotorJogStop MotorJogStop;
        DelegateMotorSpeedSet MotorSpeedSet;
        //  public void PrepareRemoteCalibration()
        //  public void SingleFindMark()
        //  public void RemoteCalibration(string strAxis, int skipCount)
        //  MotorHome
        //  MotorMove6D
        //  MotorMoveAxisAbs

        public void RegisterMotorDelegates(
            DelegateMotorHome6D fmotorHome6D,
            DelegateMotorSetHome6D fmotorSetHome6D,
            DelegateMotorMove6D fmotorMove6d,
            DelegateMotorMoveAxisAbs fmotorMoveAxisAbs, 
            DelegateMotorCurPos fmotorCurPos,
            DelegateMotorCurPos6axis fmotorCurPos6axis,
            DelegateMotorJogRun fmotorJogRun,
            DelegateMotorJogStop fmotorJogStop,
            DelegateMotorSpeedSet fmotorSpeedSet
            )
        {
            MotorHome6D = fmotorHome6D;
            MotorSetHome6D = fmotorSetHome6D;
            MotorMove6D = fmotorMove6d;
            MotorMoveAxisAbs = fmotorMoveAxisAbs;
            MotorCurPos = fmotorCurPos;
            MotorCurPos6axis = fmotorCurPos6axis;
            MotorJogRun = fmotorJogRun;
            MotorJogStop = fmotorJogStop;
            MotorSpeedSet = fmotorSpeedSet;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public FVision()
        {
            InitializeComponent();
            //ReadVisionParam();
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
                cbContinuosMode.Enabled = false;
                //string which = "true";
                //int i = 0;

                Thread threadInitBalser = new Thread(() => InitBaslerCam());
                threadInitBalser.Start();
                
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

            rbCalZ.Checked = true;
            //ChartMTF.Hide();
            //LoadscaleNTheta();
            if (m__G.oCam[0].mFAL != null)
                if (m__G.oCam[0].mFAL.mFZM != null)
                    m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            groupBox4.Hide();
            btnChangeCrop.Show();
            cbZaxis.Hide();
            cbTiltAxis.Hide();
            btnSaveOrgPosition.Hide();
            rbCalZ.Checked = true;

            tbXrange.Text = "1000";
            tbYrange.Text = "1000";
            tbZrange.Text = "800";
            tbTXrange.Text = "120";
            tbTYrange.Text = "120";
            tbTZrange.Text = "120";

            tbXstep.Text  = "3";
            tbYstep.Text  = "3";
            tbZstep.Text  = "3";
            tbTXstep.Text = "3";
            tbTYstep.Text = "3";
            tbTZstep.Text = "3";

            //if (m__G.mGageCounter != null)    // 주석 처리
            //    m__G.mGageCounter.OpenAllport();
        }

        public string camID0 = "";
        public string camID1 = "";
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
                foreach ( string lstr in camIDs)
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
                if ( camID0 =="")
                {
                    MessageBox.Show("Camera ID is not found. Check Camera ID and Restart Application.");
                    return;
                }
                else
                {
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

                BaslerCam[0].Parameters[PLCamera.TriggerMode].SetValue("On");

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


                for (i = 0; i < m__G.mCamCount; i++)
                    SetNewROIXY(i, v_OrgROIH_min[i], v_OrgROIH_min[i] + v_OrgROIH_width[i], v_OrgROIV_min[i], v_OrgROIV_min[i] + v_OrgROIV_height[i]);

                //ReadZeroGap(m__G.mCamCount);
                //ReadCalibrationTiltData();

                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)delegate
                    {
                        m__G.oCam[0].SelectWindow(panelCam0.Handle);
                        //panelCam0.Size = new Size((int)(mHROI*1.833), 627);
                        //panelCam0.Location = new Point(1940 - (int)(mHROI * 1.833), 4);
                    });
                else
                {
                    m__G.oCam[0].SelectWindow(panelCam0.Handle);
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
            TransferModelFileList();
            SetRawGainNGamma(m__G.sRecipe.iRawGain, m__G.sRecipe.iGamma);
            SetExposure(0, m__G.sRecipe.iExposure);
            LoadBackbroundNoise();
            LoadscaleNTheta();
            //string ZLUTfile = m__G.m_RootDirectory + "\\DoNotTouch\\ZLUT_" + camID0 + ".txt";
            //GetZLUT(ZLUTfile);
            string strXYOffset = LoadTXTYZeroOffset();
            m__G.fManage.AddViewLog("CSH ID " + camID0 + " " + strXYOffset);
            m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            mLoaded = true;

            m__G.oCam[0].RegisterDelegates(m__G.fGraph.mDriverIC.AckSignal);


            // Crop Pos File Load & Init
            bool isLoad = m__G.oCam[0].LoadCropPosFromXml(camID0);
            m__G.oCam[0].InitCrop(!isLoad);
            CropCgap = m__G.oCam[0].CropCgap;

            string motorFilePath = m__G.m_RootDirectory + $"\\DoNotTouch\\StageHomePos{camID0}.txt";
            //m__G.mMotion = new F_Motion(motorFilePath);
            //RegisterMotorDelegates(m__G.mMotion.LogicalOrg, m__G.mMotion.SetLogicalOrg, m__G.mMotion.MoveABS6D, m__G.mMotion.MoveABS,m__G.mMotion.GetCurPos, m__G.mMotion.GetCurPos6D,m__G.mMotion.JogMove, m__G.mMotion.JogStop, m__G.mMotion.SetSpeed6D);
            
            m__G.f_PIMotion = new F_PIMotion(motorFilePath);
            RegisterMotorDelegates(F_PIMotion._pi.MoveLogicHome6D, F_PIMotion._pi.SetLogicHome6D, F_PIMotion._pi.MoveAbs6D, F_PIMotion._pi.MoveAbsAxis, F_PIMotion._pi.GetCurPosAxis, F_PIMotion._pi.GetCurPos6D, F_PIMotion._pi.JogRun, F_PIMotion._pi.JogStop, null);

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void FVision_FormClosing(object sender, FormClosingEventArgs e)
        {
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

            button6.Enabled = false;
            button3.Enabled = false;
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
            cbContinuosMode.Checked = true;
            Thread.Sleep(200);

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
            bHaltLive = false;
            m__G.mbSuddenStop[0] = true;        //  왜 ?
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
                Task.Run(() => LiveFindMark());
            else
            {
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
            if (!bLiveFindMark)
                return;

            m__G.mDoingStatus = "LiveFindMark";

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

                string lstr = tbInfo.Text;
                string[] lineStr = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (lineStr.Length>6)
                {
                    lstr = "";
                    for (int i = 0; i < 6; i++)
                        lstr += lineStr[lineStr.Length - 6 + i] + "\r\n";

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbInfo.Text = lstr;
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        });
                    }
                    else
                    {
                        tbInfo.Text = lstr;
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                }

                m__G.oCam[0].mFAL.RecoverFromBackupFMI();
                Thread.Sleep(180);
                if (!cbLiveWithMarks.Checked)
                    break;
            }
            bHaltLive = false;
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
        bool bHaltLive = false;
        private void btnHalt2_Click(object sender, EventArgs e)
        {
            //label6.Text = "";
            m__G.oCam[0].HaltA();
            bHaltLive = true;
            btnAllLEDOn.ForeColor = Color.SlateGray;
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
            Thread.Sleep(100);
            m__G.fGraph.Drive_LEDs(0, 0);

            //m__G.oCam[0].ClearDisp();
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

            if ( ch == 1 )
            {
                if ( tbLedLeft.Text.Length > 0 )
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
            m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
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

        public List<double[]> mCalibrationFullData = new List<double[]>();
        
        public void MotorizedFindMarks(int Nth, bool IsOrg, bool IsSave = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 4;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = 3000;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////
            /// 다음 HEXAPOD 로 대체 필요
            //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                gageData = m__G.mGageCounter.ReadPortAll();
            }
            ///////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

            if (IsOrg)
            {
                m__G.oCam[0].mFAL.LoadFMICandidate();
                m__G.oCam[0].mFAL.BackupFMI();
                SetDefaultMarkConfig(true);
            }

            double minscale = (180 / Math.PI * 60) / mavNum;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) / mavNum;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(mavNum + 1);

            //int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            string[] strtmp = new string[2] { "", "" };
            m__G.mbSuddenStop[0] = false;
            TextBox[] ltextBox = new TextBox[2] { tbInfo, tbVsnLog };

            double[] lCalibrationData = new double[23];

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
            m__G.oCam[0].FineCOG(false, 1, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 2, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 3, 0);    // 마크찾기
            m__G.oCam[0].FineCOG(false, 4, 0);    // 마크찾기

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
                if (gageData.Length == 7)
                {
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
                    //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-32.45 + gageData[0] / 1000, -32.45 + gageData[1] / 1000, gageData[2] / 1000 + 32.3, gageData[3] / 1000 + 32.3, 39.9, 32.30, 32.406);;
                    double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 39.9, 32.30, 32.306);
                    //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 39.85, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);
                    //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-32.30 + gageData[0] / 1000, -32.30 + gageData[1] / 1000, gageData[2] / 1000 + 32.406, gageData[3] / 1000 + 32.406, 39.9, 32.30, 32.406);
                    double[] TxTyZ = m__G.oCam[0].mFAL.CalcTXTYZfromProbes(gageData[5] / 1000, gageData[6] / 1000, gageData[4] / 1000, XYTz[0], XYTz[1], XYTz[2]);


                    double ofx = m__G.oCam[0].mFAL.mFZM.mProbeYRy - 32;   //  Fiducial Mark Position relative to Probe Position
                    double ofy = m__G.oCam[0].mFAL.mFZM.mProbeXRx - 32;   //  Fiducial Mark Position relative to Probe Position
                    lCalibrationData[16] = (ofx - XYTz[0]) * 1000; // um
                    lCalibrationData[17] = (ofy + XYTz[1]) * 1000; // um
                    lCalibrationData[18] = -TxTyZ[2] * 1000; // um
                    lCalibrationData[19] = TxTyZ[0];       // TX   radian
                    lCalibrationData[20] = -TxTyZ[1];       // TY   radian
                    lCalibrationData[21] = XYTz[2];       // TZ   radian


                    strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                        + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");
                }
            if (ci == 0 && IsSave)
                mCalibrationFullData.Add(lCalibrationData);


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);
                    if (ltextBox[0].Text.Length > 7000)
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

                if (ltextBox[0].Text.Length > 7000)
                    ltextBox[0].Text = strtmp[0] + "\r\n";
                else
                    ltextBox[0].Text += strtmp[0] + "\r\n";

                ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                ltextBox[0].ScrollToCaret();
            }
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (ltextBox[1].Text.Length > 7000)
                        ltextBox[1].Text = strtmp[1] + "\r\n";
                    else
                        ltextBox[1].Text += strtmp[1] + "\r\n";

                    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                    ltextBox[1].ScrollToCaret();
                });
            }
            else
            {
                if (ltextBox[1].Text.Length > 2000)
                    ltextBox[1].Text = strtmp[1] + "\r\n";
                else
                    ltextBox[1].Text += strtmp[1] + "\r\n";

                ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                ltextBox[1].ScrollToCaret();
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }
        public void ManualFindMarks(int Nth, bool IsShowResult = true)
        {
            m__G.mDoingStatus = "Checking Vision";

            int mavNum = 4;

            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].dAFZM_FrameCount = 9;
            m__G.oCam[0].mTrgBufLength = 3000;
            double[] gageData = null;

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[mavNum + 1];

            //  Grab 과 가장 가까운 시점에 gage 를 읽어들인다.
            if (m__G.mGageCounter != null)
            {
                m__G.mGageCounter.m__G = m__G;
                gageData = m__G.mGageCounter.ReadPortAll(); //  gageData[6] = { X1,X2,Y1,Y2,TX,TY1,TY2 }
            }

            for (int i = 0; i < mavNum; i++)
                m__G.oCam[0].GrabB(i + 1, true);    //  1 ~ mavNum 영상이 바뀜, 0번 영상은 그대로 유지

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
                    //NthMeasure(findex, true);

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";

                    sx += m__G.oCam[0].mC_pX[findex] * umscale;
                    sy += m__G.oCam[0].mC_pY[findex] * umscale;
                    sz += m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx += m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty += m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz += m__G.oCam[0].mC_pTZ[findex] * minscale;
                }
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
                if (gageData != null)
                    if (gageData.Length == 7)
                    {
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
                        //lCalibrationData[21] = -(gageData[1] - gageData[0] - ( gageData[3] - gageData[2])) / (80000 * 0.999325049); //  TZ, radian
                        //lCalibrationData[22] = gageData[3]; //  Y

                        // 
                        //  New Formula
                        //
                        //  32mm : X probe offset from center along X axis
                        //  47mm : Y probe offset from center along Y axis
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-32.45 + gageData[0] / 1000, -32.45 + gageData[1] / 1000, gageData[2] / 1000 + 32.3, gageData[3] / 1000 + 32.3, 39.9, 32.30, 32.406);
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 39.85, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);
                        double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 39.9, 32.30, 32.306);
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[0] / 1000, -m__G.oCam[0].mFAL.mFZM.mProbeXRx + gageData[1] / 1000, gageData[2] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, gageData[3] / 1000 + m__G.oCam[0].mFAL.mFZM.mProbeYRy, 39.85, m__G.oCam[0].mFAL.mFZM.mProbeXRx, m__G.oCam[0].mFAL.mFZM.mProbeYRy);
                        //double[] XYTz = m__G.oCam[0].mFAL.CalcXYTZfromProbes(-32.30 + gageData[0] / 1000, -32.30 + gageData[1] / 1000, gageData[2] / 1000 + 32.406, gageData[3] / 1000 + 32.406, 39.9, 32.30, 32.406);
                        double[] TxTyZ = m__G.oCam[0].mFAL.CalcTXTYZfromProbes(gageData[5] / 1000, gageData[6] / 1000, gageData[4] / 1000, XYTz[0], XYTz[1], XYTz[2]);


                        double ofx = m__G.oCam[0].mFAL.mFZM.mProbeYRy - 32;   //  Fiducial Mark Position relative to Probe Position
                        double ofy = m__G.oCam[0].mFAL.mFZM.mProbeXRx - 32;   //  Fiducial Mark Position relative to Probe Position
                        lCalibrationData[16] = XYTz[0] * 1000; // um
                        lCalibrationData[17] = XYTz[1] * 1000; // um
                        lCalibrationData[18] = -TxTyZ[2] * 1000; // um
                        lCalibrationData[19] = TxTyZ[0];       // TX   radian
                        lCalibrationData[20] = -TxTyZ[1];       // TY   radian
                        lCalibrationData[21] = XYTz[2];       // TZ   radian



                        if (IsShowResult)
                            strtmp[ci] += lCalibrationData[16].ToString("F1") + "\t" + lCalibrationData[17].ToString("F1") + "\t" + lCalibrationData[18].ToString("F1") + "\t" + (3437.7 * lCalibrationData[19]).ToString("F1") + "\t"
                                + (3437.7 * lCalibrationData[20]).ToString("F1") + "\t" + (3437.7 * lCalibrationData[21]).ToString("F1");
                    }
                if (ci == 0)
                    mCalibrationFullData.Add(lCalibrationData);
            }


            if (IsShowResult)
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        DrawMarkDetected();
                        //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

                        ltextBox[0].Text += strtmp[0] + "\r\n";
                        ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                        ltextBox[0].ScrollToCaret();
                    });
                }
                else
                {
                    DrawMarkDetected();
                    //pictureBox2.Image = BitmapConverter.ToBitmap(m__G.oCam[0].mFAL.mSourceImg[0]);

                    ltextBox[0].Text += strtmp[0] + "\r\n";
                    ltextBox[0].SelectionStart = ltextBox[0].Text.Length;
                    ltextBox[0].ScrollToCaret();
                }
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        ltextBox[1].Text += strtmp[1] + "\r\n";
                        ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                        ltextBox[1].ScrollToCaret();
                    });
                }
                else
                {
                    ltextBox[1].Text += strtmp[1] + "\r\n";
                    ltextBox[1].SelectionStart = ltextBox[1].Text.Length;
                    ltextBox[1].ScrollToCaret();
                }
            }

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;
        }

        public void DrawMarkDetected()
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
            for (int i = 0; i < lMarkCount; i++)
            {
                if (fMarkPos[i].X == 0)
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

                OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();
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
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(lOverlayedImg);
            pictureBox2.Image = myImage;
        }

        bool bThreadManualFindMarks = false;
        bool bFinishThreadManualFindMarks = true;

        private void btnFindMarks_Click(object sender, EventArgs e)
        {
            if (!bThreadManualFindMarks)
            {
                btnLive2.Enabled= false;
                m__G.oCam[0].HaltA();
                bHaltLive = true;
                button10.Enabled= false;
                IsLiveCropStop = true;
                bThreadManualFindMarks = true;
                mCalibrationFullData = new List<double[]>();
                if (m__G.mGageCounter != null)
                    m__G.mGageCounter.OpenAllport();

                Task.Run(() => ThreadManualFindMarks(1000));
            }
            else
            {
                btnLive2.Enabled = true;
                button10.Enabled = true;
                bThreadManualFindMarks = false;

                while (!bFinishThreadManualFindMarks)
                    Thread.Sleep(100);

                if (m__G.mGageCounter != null)
                    m__G.mGageCounter.CloseAllport();

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

        public void RemoteCalibration(string strAxis, int skipCount)
        {
            //  strAxis 은 "Z", "Y', "X", "TX", "TY" 를 넣을 수 있다.
            //  Calibraition 순서는 "Z" -> "Z"  -> "Y" -> "Y" -> "X" -> "X" -> "TX" -> "TY"
            //  각 보정결과를 확인해야 하므로 보전 전 - 후 로 실시한다.

            //  ORG 위치에서 한쪽 끝으로 이동하면서 얻어진 데이터( skipCount )는 삭제한다.
            for ( int i=0; i< skipCount; i++)
                mCalibrationFullData.RemoveAt(0);

            CreateLUTfromMeasuredData(mCalibrationFullData.ToArray(), strAxis, m__G.mCamID0, true);
        }
        public void SingleFindMark(bool IsSave = true)
        {
            //  TCP/IP 를 통한 원격 Calibration 또는 모션제어를 통한 자동 Calibration 시에 활용할 함수
            //  SingleFindMark is used for External Calibration
            m__G.fGraph.mDriverIC.SetLEDpower(1, (int)((mLEDcurrent[0]) * 500));
            m__G.fGraph.mDriverIC.SetLEDpower(2, (int)((mLEDcurrent[1]) * 500));
            Thread.Sleep(100);   //  Wait LED Power Up.
            if (mAutoCalibrationIndex == 0)
                MotorizedFindMarks(mAutoCalibrationIndex, true, IsSave);
            else
                MotorizedFindMarks(mAutoCalibrationIndex, false, IsSave);

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
                ManualFindMarks(i);

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
        public void FindMarks( int index = 1)
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

            m__G.oCam[0].PointTo6DMotion(index, mpts);  //  이건 뭐지? 

            double lx, ly, lz;
            double ltx, lty, ltz;
            lx = m__G.oCam[0].mC_pX[index] * 5.5 / Global.LensMag;    //  Pixel to um
            ly = m__G.oCam[0].mC_pY[index] * 5.5 / Global.LensMag;    //  Pixel to um
            lz = m__G.oCam[0].mC_pZ[index] * 5.5 / Global.LensMag;    //  Pixel to um ////////////////////////////////////////// ZLUT 적용 검토
            
            ltx = m__G.oCam[0].mC_pTX[index] * 180 * 60 / Math.PI;    //  radian to min
            lty = m__G.oCam[0].mC_pTY[index] * 180 * 60 / Math.PI;    //  radian to min
            ltz = m__G.oCam[0].mC_pTZ[index] * 180 * 60 / Math.PI;    //  radian to min

            strtmp += lx.ToString("F2") + "\t" + ly.ToString("F2") + "\t" + lz.ToString("F2") + "\t| " + ltx.ToString("F2") + "\t" + lty.ToString("F2") + "\t" + ltz.ToString("F2") + "\t| ";


            //for (int i = 0; i < 12; i++)
            //{
            //    if (m__G.oCam[0].mAzimuthPts[1][i].X == 0) continue;
            //    strtmp += m__G.oCam[0].mAzimuthPts[1][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[1][i].Y.ToString("F3") + "\t";
            //}
            double XfromNtoS            = Math.Abs(  m__G.oCam[0].mAzimuthPts[index][8].X - m__G.oCam[0].mAzimuthPts[index][10].X);
            double YfromNStoE           = Math.Abs( (m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 -  m__G.oCam[0].mAzimuthPts[index][6].Y);
            double YfromSideNStoTopNS   = Math.Abs( (m__G.oCam[0].mAzimuthPts[index][0].Y + m__G.oCam[0].mAzimuthPts[index][4].Y) / 2 - (m__G.oCam[0].mAzimuthPts[index][8].Y + m__G.oCam[0].mAzimuthPts[index][10].Y) / 2);

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
            string sFilePath = Path.GetFullPath(m__G.m_RootDirectory + "\\Result\\RawData");
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "bmp";
            openFile.InitialDirectory = sFilePath;
            openFile.Multiselect = true;

            openFile.Filter = "BMP(*.bmp)|*.bmp";
            if (openFile.ShowDialog() != DialogResult.OK)
                return;

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
                
                
                
                
                
                m__G.oCam[0].LoadBMPtoBuf0(openFile.FileNames[0]);






                m__G.oCam[0].DrawCSHCross(Brushes.OrangeRed);

                int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
                for ( int ci = 0; ci< numFMIcandidate; ci++)
                {
                    m__G.mFAL.mCandidateIndex = ci;
                    ChangeFiducialMark(ci);

                    m__G.oCam[0].mFAL.mbGetHistogram = true;
                    m__G.oCam[0].FineCOG(true, 0, 0, false,  true, false, true);
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
                    m__G.oCam[0].LoadBMPtoBufN(filename, i++);
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
                    m__G.fVision.ProcessVisionData(numFile, maxThread);
                    m__G.mbSuddenStop[0] = false;
                    m__G.oCam[0].mTargetTriggerCount = orgmTargetTriggerCount;

                    double umscale = 5.5 / Global.LensMag;                           //  rad to min
                    double minscale = 180 / Math.PI * 60;                           //  rad to min

                    for (int fileCnt = 0; fileCnt < numFile; fileCnt++)
                    {
                        strtmp += fileCnt.ToString() + "\t" + (umscale * m__G.oCam[0].mC_pX[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pY[fileCnt]).ToString("F2") + "\t" + (umscale * m__G.oCam[0].mC_pZ[fileCnt]).ToString("F2")
                             + "\t" + (minscale * m__G.oCam[0].mC_pTX[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTY[fileCnt]).ToString("F2") + "\t" + (minscale * m__G.oCam[0].mC_pTZ[fileCnt]).ToString("F2") + "\t";
                        for (i = 0; i < 12; i++)
                        {
                            if (m__G.oCam[0].mAzimuthPts[fileCnt][i].X == 0) continue;
                            strtmp += m__G.oCam[0].mAzimuthPts[fileCnt][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[fileCnt][i].Y.ToString("F3") + "\t";
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
            m__G.oCam[0].DrawMarkPos(Brushes.Lime, markPos);
            tbVsnLog.Text += "Finish\r\n";
            tbVsnLog.SelectionStart = tbVsnLog.Text.Length;
            tbVsnLog.ScrollToCaret();
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


        public void CalcVisionData(int cam, int ci, int ce, int cstep, int iBuf)
        {
            int i = 0;
            mb_FinishCalcVision[iBuf] = false;
            try
            {
                bool res = false;
                int si = ci;
                int se = ce;
                int sstep = cstep;
                if ( ci>ce)
                {
                    for (i = ci; i >= ce; i -= cstep)    //  Skip 0 ~ 59
                    {
                        if (m__G.mbSuddenStop[0])   //  연산도 중단함.
                            break;
                        try
                        {
                            res = m__G.oCam[cam].FineCOG(false, i, iBuf);    // 마크찾기
                        }
                        catch(Exception ex)
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

                        res = m__G.oCam[cam].FineCOG(false, i, iBuf);    // 마크찾기
                        if (res)
                            mDebugCalcVisionCount[iBuf]++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
            cbContinuosMode.Checked = false;
            cbSaveNthImg.Checked = false;
            IsLiveCropStop = true;
            bThreadManualFindMarks = false;

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
                strtmp += m__G.oCam[0].mAzimuthPts[imgIndex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[imgIndex][i].Y.ToString("F3") + "\t" ;
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

            if (!bAccu)
                tbVsnLog.Text = "";

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

            if (tbBreakIndex.Text.Length > 0)
                m__G.oCam[0].mFAL.mBreakIndex = int.Parse(tbBreakIndex.Text);

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
            UptoNthMesure();
        }

        public void ChangeFiducialMark(int mID)
        {
            System.Drawing.Point[] markPos = null;

            if ( mID != 0)
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
        public void UptoNthMesure(int extfrmCnt = 0)
        {
            int imgIndex = 0;
            try
            {
                if (extfrmCnt == 0 )
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
            //m__G.oCam[0].mFAL.mFastMode = m__G.m_bFastMode;   //  FastMode 에서는 계단(튐)현상이 나타나므로 사용하지 않기로 함. 2023.2.23

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
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double umscale = 5.5 / Global.LensMag;                           //  rad to min

            SetDefaultMarkConfig(false);

            int lmaxThread = m__G.mMaxThread;
            int frmCnt = m__G.oCam[0].mTargetTriggerCount;

            //tbVsnLog.Text += "Target Trigger Count = " + frmCnt.ToString() + "\r\n";
            
            m__G.mbSuddenStop[0] = false;
            m__G.oCam[0].SetTriggeredframeCount(frmCnt);
            m__G.oCam[0].SetSaveLostMarkFrame(false);
            if ( extfrmCnt  == 0 )
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
                m__G.mFAL.mCandidateIndex = ci;
                ChangeFiducialMark(ci);

                if ( ci != 0)
                    m__G.mFAL.mFZM.mbCompY = ci;
                else
                    m__G.mFAL.mFZM.mbCompY = 0;

                SupremeTimer.QueryPerformanceCounter(ref beginTime);
                m__G.fVision.ProcessVisionData(imgIndex, lmaxThread);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                double ellapsedTime = (endTime - beginTime) / (double)(lTimerFrequency);

                strtmp = strGrp[ci%2] + "#\tLed\tX\tY\tZ\tTX\tTY\tTZ\tX1\tY1\tX2\tY2\tX3\tY3\tX4\tY4\tX5\tY5";
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

                    //strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mAvgLED[findex].ToString("F3") + "\t";
                    strtmp += "\r\n" + findex.ToString() + "\t" + m__G.oCam[0].mGrabAbsTiming[findex].ToString("F5") + "\t";
                    
                    sx = m__G.oCam[0].mC_pX[findex] * umscale;
                    sy = m__G.oCam[0].mC_pY[findex] * umscale;
                    sz = m__G.oCam[0].mC_pZ[findex] * umscale;
                    tx = m__G.oCam[0].mC_pTX[findex] * minscale;
                    ty = m__G.oCam[0].mC_pTY[findex] * minscale;
                    tz = m__G.oCam[0].mC_pTZ[findex] * minscale;
                    strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t";
                    for (int i = 0; i < 12; i++)
                    {
                        if (m__G.oCam[0].mAzimuthPts[findex][i].X == 0) continue;
                        strtmp += m__G.oCam[0].mAzimuthPts[findex][i].X.ToString("F3") + "\t" + m__G.oCam[0].mAzimuthPts[findex][i].Y.ToString("F3") + "\t";
                    }
                    if (findex % 100 == 99)
                    {
                        if ( ci== 0)
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

            int speed;

            if (radioButton10Step.Checked)
            {
                speed = 2; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speed = 1; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speed = 0;   // Slow
            }
            else
            {
                return;
            }

            if ( mbMotorizedStage )
            {
                if (cbZaxis.Checked)
                {
                    MotorJogRun(2, true, speed);   // Z
                }
                else if ( cbTiltAxis.Checked)
                {
                    MotorJogRun(3, true, speed);   // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(5, true, speed);   // TZ
                }
                else
                {
                    MotorJogRun(1, false, speed);   // Y
                }
            }
        }

        private void btnFOVUp_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVUp.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrU;
            if (mbMotorizedStage)
            {
                if (cbZaxis.Checked)
                {
                    MotorJogStop(2);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(3);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(5);
                }
                else
                {
                    MotorJogStop(1);
                }
            }
        }

        private void btnFOVLeft_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL2;
            int speed;

            if (radioButton10Step.Checked)
            {
                speed = 2; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speed = 1; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speed = 0;   // Slow
            }
            else
            {
                return;
            }

            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(4, false, speed);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(5, false, speed);  // TZ
                }
                else
                {
                    MotorJogRun(0, false, speed);   // X
                }
            }
        }

        private void btnFOVLeft_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVLeft.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrL;
            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogStop(4);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(5);
                }
                else
                {
                    MotorJogStop(0);
                }
            }
        }

        private void btnFOVDown_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD2;

            int speed;

            if (radioButton10Step.Checked)
            {
                speed = 2; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speed = 1; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speed = 0;   // Slow
            }
            else
            {
                return;
            }

            if (mbMotorizedStage)
            {

                if (cbZaxis.Checked)
                {
                    MotorJogRun(2, false, speed);  // Z
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogRun(3, false, speed);  // TX
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(5, false, speed);  // TZ
                }
                else
                {
                    MotorJogRun(1, true, speed);  // Y
                }
                
            }
        }

        private void btnFOVDown_MouseUp(object sender, MouseEventArgs e)
        {
            btnFOVDown.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrD;
            if (mbMotorizedStage)
            {
                if (cbZaxis.Checked)
                {
                    MotorJogStop(2);
                }
                else if (cbTiltAxis.Checked)
                {
                    MotorJogStop(3);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(5);
                }
                else
                {
                    MotorJogStop(1);
                }
            }
        }

        private void btnFOVRight_MouseDown(object sender, MouseEventArgs e)
        {
            btnFOVRight.BackgroundImage = global::CSH030Ex.Properties.Resources.ArrR2;

            int speed;

            if (radioButton10Step.Checked)
            {
                speed = 2; // Fast
            }
            else if (radioButton1Step.Checked)
            {
                speed = 1; // Normal
            }
            else if (radioButtonSlowStep.Checked)
            {
                speed = 0;   // Slow
            }
            else
            {
                return;
            }

            if (mbMotorizedStage)
            {
                if (cbTiltAxis.Checked)
                {
                    MotorJogRun(4, true, speed);   // TY
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogRun(5, true, speed);   // TZ
                }
                else
                {
                    MotorJogRun(0, true, speed);   // X
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
                    MotorJogStop(4);
                }
                else if (cbRaxis.Checked)
                {
                    MotorJogStop(5);
                }
                else
                {
                    MotorJogStop(0);
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
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex/2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
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
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex/2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
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
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex/2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
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
            else if (lbtn.TabIndex == 139 || lbtn.TabIndex == 442 || lbtn.TabIndex == 339 || lbtn.TabIndex == 123 || lbtn.TabIndex == 131 || lbtn.TabIndex == 132 || lbtn.TabIndex == 137 || lbtn.TabIndex == 238 || lbtn.TabIndex == 279 || lbtn.TabIndex == 280 || lbtn.TabIndex == 345 || lbtn.TabIndex == 348 || lbtn.TabIndex/2 == 210 || lbtn.TabIndex == 346 || lbtn.TabIndex == 422 || lbtn.Text.Contains("Scale"))
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

            m__G.mbSuddenStop[0] = true;        //  왜 ?
            //MessageBox.Show("m__G.mbSuddenStop[0] = true in button11()");
            int lgrabbedFrame = 0;
            double frameRate = 0;

            //SetOrgExposure(0);

            if (cbContinuosMode.Checked)
            {
                tbVsnLog.Text += "Target Trigger Count = " + m__G.oCam[0].mTargetTriggerCount.ToString();
                for ( int i=0; i< m__G.oCam[0].mTargetTriggerCount; i++)
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


            m__G.oCam[0].FineCOG(true, 0, 0);    // 마크찾기

            if (count< HowManyThread)
            {
                mb_FinishCalcVision[0] = false;
                mDebugCalcVisionCount[0] = 0;
                m__G.oCam[0].FineCOG(true, 0, 0, true, false, IsFile);    // 마크찾기
                if ( count > 1)
                {
                    CalcVisionData(0, 0, count, 1, 0);
                }
                mb_FinishCalcVision[0] = true;
                m__G.oCam[0].mFinishVisionData = true;    //  맞다
                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                SupremeTimer.QueryPerformanceCounter(ref endTime);
                ltime = (endTime - startTime) / (double)(lTimerFrequency);
                return ltime;
            }

            Task[] Task_CalcVisionData = new Task[HowManyThread];

            int halfCnt = count / 2;
            int lastIndx = count - 1;
            int halfThread = HowManyThread / 2;
            HowManyThread = halfThread * 2;

            List<int> taskIndices = new List<int>();
            List<int> lastImgIndex = new List<int>();
            for (int i = 0; i < halfThread; i++)
            {
                mb_FinishCalcVision[i] = false;
                mDebugCalcVisionCount[i] = 0;
                mb_FinishCalcVision[halfThread + i] = false;
                mDebugCalcVisionCount[halfThread + i] = 0;

                taskIndices.Add(i);
                lastImgIndex.Add(lastIndx - i);
            }

            if (HowManyThread<=1)
                CalcVisionData(0, 0, count, 1, 1);
            else
            {
                Parallel.ForEach(taskIndices, taskIndex =>
                {
                    CalcVisionData(0, taskIndex, halfCnt, halfThread, taskIndex*2);
                    CalcVisionData(0, lastIndx - taskIndex, halfCnt, halfThread, taskIndex * 2 + 1);
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
                    panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, 627);
                });
            else
            {
                cbContinuosMode.Checked = true;
                panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, 627);
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
                    panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, (int)(panelCam0.Size.Height * mVROI / 360.0));
                });
            else
            {
                tbInfo.Text += "FOV Shift Delta Y = " + fovShiftY.ToString() + "\r\n";
                tbInfo.SelectionStart = tbInfo.Text.Length;
                tbInfo.ScrollToCaret();
                cbContinuosMode.Checked = false;
                panelCam0.Size = new System.Drawing.Size(panelCam0.Size.Width, (int)(panelCam0.Size.Height * mVROI / 360.0));
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

        public double[] ms_scaleX = new double[3] { 0, 1, 0};
        public double[] ms_scaleY = new double[3] { 0, 1, 0 };
        public double ms_scaleZ = 1;
        public double ms_sinTheta = 0.64278761;    //  sin(40deg)
        public double[] ms_scaleTX = new double[3] { 0, 1, 0 };
        public double ms_scaleTY = 1;
        public double ms_ZtoXbyView = 0;
        public double ms_ZtoYbyView = 0;
        public double ms_YtoXbyView = 0;
        public double[] ms_YtoZbyView = new double[3];
        public double ms_XtoYbyView = 0;
        public double[] ms_XtoZbyView = new double[3];
        public double ms_EastViewYPscale = 1.0;
        public double ms_XtoTXbyView = 0;

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
            m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);
            if (ms_sinTheta > 0)
                m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
            else
                m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

            SaveScaleNTheta();

            tbInfo.Size         = new System.Drawing.Size(tbInfo.Size.Width + 400, tbInfo.Size.Height);
            tbInfo.Location     = new System.Drawing.Point(tbInfo.Location.X - 400, tbInfo.Location.Y);
            tbVsnLog.Size       = new System.Drawing.Size(tbVsnLog.Size.Width + 400, tbVsnLog.Size.Height);
            tbVsnLog.Location   = new System.Drawing.Point(tbVsnLog.Location.X - 400, tbVsnLog.Location.Y);
            bScaleUpdating = false;

            m__G.oCam[0].DrawClear();
            DrawMarkPositions();
        }

        public void SaveScaleNTheta()
        {
            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine( ms_scaleX.ToString());
            wr.WriteLine( ms_scaleY.ToString());
            wr.WriteLine( ms_scaleZ.ToString());
            wr.WriteLine( ms_sinTheta.ToString());
            wr.Close();

        }
        public bool LoadscaleNTheta()
        {
            //MessageBox.Show("LoadscaleNTheta called ");
            string scaleFile = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            if (!File.Exists(scaleFile))
            {
                //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
                StreamWriter orgwr = new StreamWriter(scaleFile);
                string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
                              "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
                              "1.00\t// Tab 분리, Z scale\r\n" +
                              "0.64278761\r\n1.00\t// Tab 분리, TX scale\r\n" +
                              "1.00\t// Tab 분리, TY scale\r\n" +
                              "0.00\t// Tab 분리, Z to X coef\r\n" +
                              "0.00\t// Tab 분리, Z to Y coef\r\n" +
                              "0.00\t// Tab 분리, Y to X coef\r\n" +
                              "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
                              "0.00\t// Tab 분리, X to Y coef\r\n" +
                              "0.00\t// Tab 분리, X to Z coef\r\n" +
                              "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
                              "0.00\t// Tab 분리, X to TX coef\r\n" +
                              "0.00\t// Tab 분리, FX0 of 6 axis stage, default value = 0, X Offset of the center of Fiducial Mark\r\n" +
                              "0.00\t// Tab 분리, FY0 of 6 axis stage, default value = 0, Y Offset of the center of Fiducial Mark\r\n" +
                              "55.00\t// / Tab 분리, L1 of 6 axis stage TY\r\n" +
                              "55.00\t// / Tab 분리, L2 of 6 axis stage TY\r\n" +
                              "0.00\t// / Tab 분리, Y Offset of Probe TY\r\n" +
                              "55.00\t// / Tab 분리, Y Offset of Probe TX\r\n"+
                              "32.30\t// / Tab 분리, Probe X Rx\r\n"+
                              "32.30\t// / Tab 분리, Probe Y Ry\r\n";
                orgwr.Write(istr);
                orgwr.Close();

                //double[] defaultScale = new double[3] { 0, 1, 0 };
                //m__G.oCam[0].mFAL.mFZM.SetScales(defaultScale, defaultScale, 1);
                //m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);
                //return;
            }

            try
            {
                StreamReader rd = new StreamReader(scaleFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 4)
                    return false;
                string[] eachData = new string[eachLine.Length];
                string[] strXX = new string[2];
                string[] strYY = new string[2];
                string[] strX2Z= new string[2];
                string[] strY2Z = new string[2];
                string[] strTX = new string[2];
                string strX2TX = "";
                bool Xis2nd = false;
                bool Yis2nd = false;
                bool X2Zis2nd = false;
                bool Y2Zis2nd = false;
                bool TXis2nd = false;
                for (int i=0; i < eachLine.Length; i++)
                {
                    string[] strdata = eachLine[i].Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    eachData[i] = strdata[0];
                    if ( i==0)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            strXX[0] = strdata[1];
                            strXX[1] = strdata[2];
                            Xis2nd = true;
                        }

                    }
                    else if ( i==1)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            strYY[0] = strdata[1];
                            strYY[1] = strdata[2];
                            Yis2nd = true;
                        }

                    }
                    else if (i == 4)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            strTX[0] = strdata[1];
                            strTX[1] = strdata[2];
                            TXis2nd = true;
                        }
                    }
                    else if ( i==9)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            strY2Z[0] = strdata[1];
                            strY2Z[1] = strdata[2];
                            Y2Zis2nd = true;
                        }
                    }
                    else if (i == 11)
                    {
                        if (strdata.Length >= 3 && !strdata[1].Contains("//"))
                        {
                            strX2Z[0] = strdata[1];
                            strX2Z[1] = strdata[2];
                            X2Zis2nd = true;
                        }
                    }else if ( i== 13)
                    {
                        //  XtoTX
                        if (strdata.Length >= 1 && !strdata[0].Contains("//"))
                        {
                            strX2TX = strdata[0];
                        }
                    }
                }

                ms_scaleX[1] = double.Parse(eachData[0]);
                ms_scaleY[1] = double.Parse(eachData[1]);

                if ( Xis2nd )
                {
                    ms_scaleX[0] = double.Parse(eachData[0]);
                    ms_scaleX[1] = double.Parse(strXX[0]);
                    ms_scaleX[2] = double.Parse(strXX[1]);
                }
                if (Yis2nd)
                {
                    ms_scaleY[0] = double.Parse(eachData[1]);
                    ms_scaleY[1] = double.Parse(strYY[0]);
                    ms_scaleY[2] = double.Parse(strYY[1]);
                }
                if (strX2TX != "")
                {
                    ms_XtoTXbyView = double.Parse(strX2TX);
                }
                
                ms_scaleZ = double.Parse(eachData[2]);
                ms_sinTheta = double.Parse(eachData[3]);

                if (eachLine.Length >= 6)
                {
                    if (TXis2nd)
                    {
                        ms_scaleTX[0] = double.Parse(eachData[4]);
                        ms_scaleTX[1] = double.Parse(strTX[0]);
                        ms_scaleTX[2] = double.Parse(strTX[1]);

                    }
                    else
                        ms_scaleTX[1] = double.Parse(eachData[4]);

                    ms_scaleTY = double.Parse(eachData[5]);
                    m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY);
                }
                else
                    m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);

                if (eachLine.Length >= 8)
                {
                    if (eachData[6].Length > 1)
                    {
                        ms_ZtoXbyView = double.Parse(eachData[6]);  //  Z calibration 할 때 Stabilized Data 에서 [ X 총이동거리 / Z 총이동거리 ] 를 기록해놔야 한다.
                        ms_ZtoYbyView = double.Parse(eachData[7]);  //  Z calibration 할 때 Stabilized Data 에서 [ Y 총이동거리 / Z 총이동거리 ] 를 기록해놔야 한다.
                        m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_ZtoXbyView, ms_ZtoYbyView);
                    }
                }
                if (eachLine.Length >= 10)
                {
                    if (eachData[8].Length > 1)
                    {
                        ms_YtoXbyView = double.Parse(eachData[8]);  //  Y calibration 할 때 Stabilized Data 에서 [ X 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                        if(!Y2Zis2nd)
                        {
                            ms_YtoZbyView[0] = 0;
                            ms_YtoZbyView[1] = double.Parse(eachData[9]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                            ms_YtoZbyView[2] = 0;
                        }
                        else
                        {
                            ms_YtoZbyView[0] = double.Parse(eachData[9]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                            ms_YtoZbyView[1] = double.Parse(strY2Z[0]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                            ms_YtoZbyView[2] = double.Parse(strY2Z[1]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                        }
                        m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_ZtoXbyView, ms_ZtoYbyView, ms_YtoXbyView, ms_YtoZbyView);
                    }
                }
                if (eachLine.Length >= 12)
                {
                    if (eachData[10].Length > 1)
                    {
                        ms_XtoYbyView = double.Parse(eachData[10]);  //  X calibration 할 때 Stabilized Data 에서 [ Y 총이동거리 / X 총이동거리 ] 를 기록해놔야 한다.
                        if (!X2Zis2nd)
                        {
                            ms_XtoZbyView[0] = 0;
                            ms_XtoZbyView[1] = double.Parse(eachData[11]);  //  X calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / X 총이동거리 ] 를 기록해놔야 한다.
                            ms_XtoZbyView[2] = 0;
                        }
                        else
                        {
                            ms_XtoZbyView[0] = double.Parse(eachData[11]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                            ms_XtoZbyView[1] = double.Parse(strX2Z[0]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                            ms_XtoZbyView[2] = double.Parse(strX2Z[1]);  //  Y calibration 할 때 Stabilized Data 에서 [ Z 총이동거리 / Y 총이동거리 ] 를 기록해놔야 한다.
                        }

                        m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_ZtoXbyView, ms_ZtoYbyView, ms_YtoXbyView, ms_YtoZbyView, ms_XtoYbyView, ms_XtoZbyView);
                    }
                }
                if (eachLine.Length >= 13)
                {
                    if (eachData[12].Length > 1)
                    {
                        ms_EastViewYPscale = double.Parse(eachData[12]);  //  
                        m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ, ms_scaleTX, ms_scaleTY, ms_ZtoXbyView, ms_ZtoYbyView, ms_YtoXbyView, ms_YtoZbyView, ms_XtoYbyView, ms_XtoZbyView, ms_EastViewYPscale, ms_XtoTXbyView);
                    }
                }
                if (eachLine.Length >= 15)
                {
                    if (eachData[14].Length > 1)
                    {
                        double Fx = double.Parse(eachData[14]);  //  
                        double Fy = double.Parse(eachData[15]);  //  
                        if (eachLine.Length >= 17)
                        {
                            if (eachData[16].Length > 1)
                            {
                                double L1 = double.Parse(eachData[16]);  //  
                                double L2 = double.Parse(eachData[17]);  //  
                                if (eachLine.Length >= 19)
                                {
                                    if (eachData[18].Length > 1)
                                    {
                                        double txL1 = double.Parse(eachData[18]);  //  
                                        double txL2 = double.Parse(eachData[19]);  //  
                                        if (eachData.Length >= 21)
                                        {
                                            if (eachData[20].Length >1)
                                            {
                                                double Rx = double.Parse(eachData[20]);
                                                double Ry = double.Parse(eachData[21]);
                                                m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2, txL1, txL2, Rx, Ry);
                                            }
                                            else
                                                m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2, txL1, txL2);

                                        }
                                        else
                                            m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2, txL1, txL2);
                                    }
                                    else
                                        m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2);
                                }
                                else
                                    m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy, L1, L2);
                            }
                            else
                                m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy);
                        }else 
                            m__G.oCam[0].mFAL.SetCenterOfFiducialMarkOffset(Fx, Fy);


                    }
                }


                if (ms_sinTheta > 0)
                    m__G.oCam[0].SetSideviewTheta(Math.Asin(ms_sinTheta));
                else
                    m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);

                m__G.fManage.AddViewLog("Scale Z : " + ms_scaleZ.ToString("F5") + "\r\n");

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
                tbInfo.Font = new Font("Calibri", 14, FontStyle.Bold);
                bLiveFindMark = true;
                Task.Run(() => LiveFindMark());
            }
            else
            {
                bLiveFindMark = false;
                tbInfo.Font = new Font("Calibri", 8, FontStyle.Regular);
                m__G.mDoingStatus = "IDLE";
                m__G.mIDLEcount = 0;
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
            if ( m__G== null)
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


        private void button3_Click(object sender, EventArgs e)
        {
            m__G.mDoingStatus = "Checking Vision";
            //m__G.mDoingStatus = "IDLE";
            DrawMarkPositions();

            m__G.oCam[0].GrabB(1);

            string fname = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\SetZeroGrab.bmp";
            m__G.oCam[0].SaveImageBuf(fname);

            m__G.oCam[0].GrabB(2);
            m__G.oCam[0].GrabB(3);
            m__G.oCam[0].GrabB(4);
            m__G.oCam[0].GrabB(5);

            m__G.oCam[0].mFAL.LoadFMICandidate();
            m__G.oCam[0].mFAL.BackupFMI();

            double ltx = 0;
            double lty = 0;
            double ltz = 0;

            //MessageBox.Show("Call SetTXTYOffset 1");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(false, false);

            for ( int i=1; i<6; i++)
            {
                FindMarks(i);
                //lx = m__G.oCam[0].mC_pX[i] * 5.5 / Global.LensMag;    //  Pixel to um
                //ly = m__G.oCam[0].mC_pY[i] * 5.5 / Global.LensMag;    //  Pixel to um
                //lz = m__G.oCam[0].mC_pZ[i] * 5.5 / Global.LensMag;    //  Pixel to um
                ltx += m__G.oCam[0].mC_pTX[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
                lty += m__G.oCam[0].mC_pTY[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
                ltz += m__G.oCam[0].mC_pTZ[i];// radian  // * 180 * 60 / Math.PI;    //  radian to min
            }
            ltx = ltx / 5;
            lty = lty / 5;
            ltz = ltz / 5;
            double masterTx = 0;
            double masterTy = 0;
            double orgMasterTx = double.Parse(tbMasterTX.Text);
            double orgMasterTy = double.Parse(tbMasterTY.Text);
            try
            {
                if (m__G.m_bXTiltReverse)
                    masterTx = -orgMasterTx;
                else
                    masterTx = orgMasterTx;
            }
            catch (Exception err)
            {
                tbMasterTX.Text = "0";
            }
            try
            {
                if (m__G.m_bYTiltReverse)
                    masterTy = -orgMasterTy;
                else
                    masterTy = orgMasterTy;
            }
            catch (Exception err)
            {
                tbMasterTY.Text = "0";
            }

            //if (m__G.m_bXTiltReverse)
            //    masterTx = -masterTx;
            //if (m__G.m_bXTiltReverse)
            //    masterTy = -masterTy;

            SaveTXTYZeroOffset(ltx, lty, ltz, masterTx, masterTy, orgMasterTx, orgMasterTy);
            m__G.oCam[0].mFAL.mFZM.SetSignTXTY(m__G.m_bXTiltReverse, m__G.m_bYTiltReverse);

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();

        }
        public void SaveTXTYZeroOffset(double tx, double ty, double tz, double masterTx, double masterTy, double orgMasterTx, double orgMasterTy )
        {
            //MessageBox.Show("Call SetTXTYOffset 2");
            m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(tx - masterTx / (180 / Math.PI * 60), ty - masterTy / (180 / Math.PI * 60), tz);

            string filename = m__G.m_RootDirectory + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";

            StreamWriter wr = new StreamWriter(filename);
            wr.WriteLine(tx.ToString());
            wr.WriteLine(ty.ToString());
            wr.WriteLine(tz.ToString());

            wr.WriteLine(orgMasterTx.ToString());
            wr.WriteLine(orgMasterTy.ToString());

            wr.Close();
        }
        public string LoadTXTYZeroOffset()
        {
            //MessageBox.Show("LoadscaleNTheta called ");
            string txtyZeroFile = m__G.m_RootDirectory + "\\DoNotTouch\\TXTYTZoffset_" + camID0 + ".txt";
            if (!File.Exists(txtyZeroFile))
            {
                //MessageBox.Show("Call SetTXTYOffset 3");
                m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(0, 0, 0);
                //m__G.oCam[0].SetSideviewTheta(40.0 / 180 * Math.PI);
                return "TX TY offset = 0,0";
            }
            try
            {
                StreamReader rd = new StreamReader(txtyZeroFile);
                string fullstr = rd.ReadToEnd();
                rd.Close();
                string[] eachLine = fullstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (eachLine.Length < 3)
                    return "TX TY offset = 0,0";
                double tx = double.Parse(eachLine[0]);
                double ty = double.Parse(eachLine[1]);
                double tz = double.Parse(eachLine[2]);
                double masterTx = 0;
                double masterTy = 0;
                double orgMasterTx = 0;
                double orgMasterTy = 0;
                if (eachLine.Length > 3)
                {
                    orgMasterTx = double.Parse(eachLine[3]);
                    orgMasterTy = double.Parse(eachLine[4]);
                    if (m__G.m_bXTiltReverse)
                        masterTx = -orgMasterTx;
                    else
                        masterTx = orgMasterTx;
                    if (m__G.m_bYTiltReverse)
                        masterTy = -orgMasterTy;
                    else
                        masterTy = orgMasterTy;

                    m__G.oCam[0].mFAL.mFZM.SetTXTYOffset(tx - masterTx / (180 / Math.PI * 60), ty - masterTy / (180 / Math.PI * 60), tz);
                }
                tbMasterTX.Text = orgMasterTx.ToString("F2");
                tbMasterTY.Text = orgMasterTy.ToString("F2");
                return "TX TY offset = " + orgMasterTx.ToString("F2") + "," + orgMasterTy.ToString("F2");
                //m__G.oCam[0].mFAL.mFZM.SetScales(ms_scaleX, ms_scaleY, ms_scaleZ);
            }
            catch (Exception e)
            {

            }
            return "TX TY offset = 0,0";
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
            SaveTXTYZeroOffset(0, 0, 0, 0, 0,0,0);
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
            if ( cbSetTXTYwithMaster.Checked )
            {
                button6   .Enabled = true;
                button3   .Enabled = true;
                tbMasterTX.Enabled = true;
                tbMasterTY.Enabled = true;
            }
            else
            {
                button6   .Enabled = false;
                button3   .Enabled = false;
                tbMasterTX.Enabled = false;
                tbMasterTY.Enabled = false;
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

        private void button7_Click(object sender, EventArgs e)
        {
            LoadscaleNTheta();
            LoadTXTYZeroOffset();
            m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);

            for (int mi = 0; mi < 5; mi++)
                m__G.oCam[0].mMarkPosRes[mi] = new FAutoLearn.FZMath.Point2D[1];

            pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);
            string fileName = m__G.m_RootDirectory + "\\Result\\RawData\\Image\\LastGrab.bmp";
            m__G.oCam[0].SaveGrabbedImage(0, fileName);

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
            double minscale = (180 / Math.PI * 60) ;                           //  rad to min
            double umscale = (5.5 / Global.LensMag) ;                           //  rad to min

            m__G.oCam[0].SetTriggeredframeCount(1);
            m__G.oCam[0].SetSaveLostMarkFrame(false);

            int numFMIcandidate = m__G.mFAL.GetNumFMICandidate();
            strtmp = "";
            m__G.mbSuddenStop[0] = false;

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

                m__G.oCam[0].PrepareFineCOG();
                m__G.oCam[0].mFAL.mbGetHistogram = true;
                NthMeasure(0);
                m__G.oCam[0].mFAL.mbGetHistogram = false;

                sx = m__G.oCam[0].mC_pX[0] * umscale;
                sy = m__G.oCam[0].mC_pY[0] * umscale;
                sz = m__G.oCam[0].mC_pZ[0] * umscale;
                tx = m__G.oCam[0].mC_pTX[0] * minscale;
                ty = m__G.oCam[0].mC_pTY[0] * minscale;
                tz = m__G.oCam[0].mC_pTZ[0] * minscale;
                strtmp += sx.ToString("F2") + "\t" + sy.ToString("F2") + "\t" + sz.ToString("F2") + "\t" + tx.ToString("F2") + "\t" + ty.ToString("F2") + "\t" + tz.ToString("F2") + "\t\tContrast\t";
                for (int i = 0; i < 5; i++)
                    strtmp += m__G.oCam[0].mFAL.mEffectiveContrast[i].ToString() + "\t";

                strtmp += "( > 20 )";


            }
            DrawMarkDetected();

            tbInfo.Text += strtmp + "\r\n";

            m__G.oCam[0].mFAL.RecoverFromBackupFMI();
            m__G.mDoingStatus = "IDLE";
            m__G.mIDLEcount = 0;

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
                    m__G.oCam[0].SelectWindow(panelCam0.Handle);
                });
            else
                m__G.oCam[0].SelectWindow(panelCam0.Handle);

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
                if ( !bValidLine )
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

        public void CreateLUTfromMeasuredData(double[][] measure, string axis, string cameraID, bool IsRemote = false)
        {
            if (m__G.oCam[0].mFAL.mFZM == null)
            {
                MessageBox.Show("mFZM not loaded.");
                return;
            }

            //string orgfilename = m__G.m_RootDirectory + "\\DoNotTouch\\ScaleNTheta" + camID0 + ".txt";
            //if ( !File.Exists(orgfilename))
            //{
            //    //  파일이 없으면 기본값을 저장한 기본 파일을 생성해준다.
            //    StreamWriter orgwr = new StreamWriter(orgfilename);
            //    string istr = "1.00\t// Tab 분리, X scale : aX^2 + bX + c\r\n" +
            //                  "1.00\t// Tab 분리, Y scalea : aY^2 + bY + c\r\n" +
            //                  "1.00\t// Tab 분리, Z scale\r\n" +
            //                  "0.64278761\r\n1.00\t// Tab 분리, TX scale\r\n" +
            //                  "1.00\t// Tab 분리, TY scale\r\n" +
            //                  "0.00\t// Tab 분리, Z to X coef\r\n" +
            //                  "0.00\t// Tab 분리, Z to Y coef\r\n" +
            //                  "0.00\t// Tab 분리, Y to X coef\r\n" +
            //                  "0.00\t// Tab 분리, Y to Z coef : aY^2 + bY + c\r\n" +
            //                  "0.00\t// Tab 분리, X to Y coef\r\n" +
            //                  "0.00\t// Tab 분리, X to Z coef\r\n" +
            //                  "1.00\t// Tab 분리, EastView Y pixel Scale\r\n" +
            //                  "0.00\t// Tab 분리, X to TX coef\r\n" +
            //                  "0.00\t// Tab 분리, FX0 of 6 axis stage, default value = 0, X Offset of the center of Fiducial Mark\r\n" +
            //                  "0.00\t// Tab 분리, FY0 of 6 axis stage, default value = 0, Y Offset of the center of Fiducial Mark\r\n" +
            //                  "55.00\t// / Tab 분리, L1 of 6 axis stage TY\r\n" +
            //                  "55.00\t// / Tab 분리, L2 of 6 axis stage TY\r\n" +
            //                  "0.00\t// / Tab 분리, Y Offset of Probe TY\r\n" +
            //                  "55.00\t// / Tab 분리, Y Offset of Probe TX\r\n";
            //    orgwr.Write(istr);
            //    orgwr.Close();
            //}

            string AdminPathName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            string DoNotTouchPathName = m__G.m_RootDirectory + "\\DoNotTouch\\";
            if (!Directory.Exists(AdminPathName))
                Directory.CreateDirectory(AdminPathName);

            int fullLength = measure.Length;
            StreamWriter wr = null;
            //  measure[i] 에는 X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2, ... , X5,Y5, SZ, SX, SY, Stx, Sty 의 총 21개 데이터가 들어있다.

            //  안정화 유효데이터를 추출한다.
            //  각 유효Index 에서의 데이터배열을 별도 List 에 저장한다.

            List<double[]> stablizedData = new List<double[]>();

            int effLength = 0;
            double a = 0;
            double b = 0;
            int[] effIndex = null;
            FZMath.Point2D[] szz = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy1 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy2 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] szy3 = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sXX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sYY = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTXTX = new FZMath.Point2D[effLength];
            FZMath.Point2D[] sTYTY = new FZMath.Point2D[effLength];

            if (!IsRemote)
            {
                switch (axis)
                {
                    case "Z":
                        effIndex = ExtractStablizedIndex(measure, 2);
                        break;

                    case "X":
                        effIndex = ExtractStablizedIndex(measure, 0);
                        break;
                    case "Y":
                        effIndex = ExtractStablizedIndex(measure, 1);
                        break;
                    case "TX":
                        effIndex = ExtractStablizedIndex(measure, 3);
                        break;
                    case "TY":
                        effIndex = ExtractStablizedIndex(measure, 4);
                        break;
                    default:
                        break;
                }
                effLength = effIndex.Length;

                szz = new FZMath.Point2D[effLength];
                szy1 = new FZMath.Point2D[effLength];
                szy2 = new FZMath.Point2D[effLength];
                szy3 = new FZMath.Point2D[effLength];
                sXX = new FZMath.Point2D[effLength];
                sYY = new FZMath.Point2D[effLength];
                sTXTX = new FZMath.Point2D[effLength];
                sTYTY = new FZMath.Point2D[effLength];
                for (int i = 0; i < effLength; i++)
                {
                    szz[i] = new FZMath.Point2D();
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();
                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                }

                if (effLength == 0)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbInfo.Text += "Stabilized data not found\r\n";
                            tbInfo.SelectionStart = tbInfo.Text.Length;
                            tbInfo.ScrollToCaret();
                        });
                    }
                    else
                    {
                        tbInfo.Text += "Stabilized data not found\r\n";
                        tbInfo.SelectionStart = tbInfo.Text.Length;
                        tbInfo.ScrollToCaret();
                    }
                    return;

                }
                for (int i = 0; i < effLength; i++)
                {
                    double[] lstbData = new double[22];
                    for (int j = 0; j < 22; j++)
                        lstbData[j] = measure[effIndex[i]][j];

                    stablizedData.Add(lstbData);
                }
            }
            else
            {
                //  Remote or Auto Calibration
                effLength = measure.Length;

                szz = new FZMath.Point2D[effLength];
                szy1 = new FZMath.Point2D[effLength];
                szy2 = new FZMath.Point2D[effLength];
                szy3 = new FZMath.Point2D[effLength];
                sXX = new FZMath.Point2D[effLength];
                sYY = new FZMath.Point2D[effLength];
                sTXTX = new FZMath.Point2D[effLength];
                sTYTY = new FZMath.Point2D[effLength];
                for (int i = 0; i < effLength; i++)
                {
                    szz[i] = new FZMath.Point2D();
                    szy1[i] = new FZMath.Point2D();
                    szy2[i] = new FZMath.Point2D();
                    szy3[i] = new FZMath.Point2D();
                    sXX[i] = new FZMath.Point2D();
                    sYY[i] = new FZMath.Point2D();
                    sTXTX[i] = new FZMath.Point2D();
                    sTYTY[i] = new FZMath.Point2D();
                }

                for (int i = 0; i < effLength; i++)
                    stablizedData.Add(measure[i]);
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////
            //  전체 데이터를 다 저장하는 파일을 하나 만들어야한다.
            StreamWriter lwr = null;
            double rad2min = 180 * 60 / Math.PI;
            double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

            if ( !IsRemote )
            {
                lwr = new StreamWriter(AdminPathName + "FullData.csv");
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                int k = 0;
                for (int i = 0; i < fullLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                        slstr += measure[i][j].ToString("F5") + ",";
                    if (i == effIndex[k])
                    {
                        slstr += "*";
                        k++;
                    }
                    lwr.WriteLine(slstr);
                    if (k == effLength)
                        break;
                }
                lwr.Close();
            }

            string calName = axis;
            if (isAutoCalibrationEastView)
            {
                calName += "_EastView";
            }

            string strStabilizedFile = "";
            if (mAutoCalibrationCount % 2 == 0)
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before.csv";
            else
                strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After.csv";

            try
            {
                lwr = new StreamWriter(strStabilizedFile);
            }
            catch (Exception e)
            {
                Int64 lnow = (DateTime.Now.ToBinary())%1000000;
                if (mAutoCalibrationCount % 2 == 0)
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_Before" + lnow.ToString() + ".csv";
                else
                    strStabilizedFile = AdminPathName + "StabilizedData_" + calName + "_After" + lnow.ToString() + ".csv";

                lwr = new StreamWriter(strStabilizedFile);
            }
            if (lwr!=null)
            {
                lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,X1,Y1,X2,Y2,X3,Y3,X4,Y4,X5,Y5,pX,pY,pZ,pTX,pTY,pTZ,pY2");
                for (int i = 0; i < effLength; i++)
                {
                    string slstr = i.ToString() + ",";
                    for (int j = 0; j < 23; j++)
                    {
                        if (j < 19 || j == 22)
                            slstr += stablizedData[i][j].ToString("F5") + ",";
                        else
                        {
                            slstr += (rad2min * stablizedData[i][j]).ToString("F5") + ",";
                        }
                    }

                    lwr.WriteLine(slstr);
                }
                lwr.Close();
            }

            //////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////

            //  axis 따라서 List 에 저장된 데이터를 처리한다.
            double[] p2ndCoef = new double[3];
            double[] p2ndCoef2 = new double[3];
            double[] p2ndCoef3 = new double[3];
            int fovYoffset = GetROIY(0);
            string lstr = "";
            switch (axis)
            {
                case "Z":
                    //  axis == "Z" : YLUT 의 경우 
                    //  SZ vs Y1 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT1_tmp[i] = a * SZ[i] - ( Y1[i] - b )
                    //  LUT1[i] = ( LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT1[0] = ( 2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3 ;
                    //  LUT1[N-1] = ( 2 * LUT1_tmp[N-1] + LUT1_tmp[N-2]) / 3 ;

                    //  Z scale 도 여기서 구해야 한다. 현재 빠져있다. 2024.3.5

                    double a1 = 0;
                    double b1 = 0;
                    double a2 = 0;
                    double b2 = 0;
                    double a3 = 0;
                    double b3 = 0;

                    mZCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mZCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);

                    //  stablizedData[i][16] : X
                    //  stablizedData[i][17] : Y
                    //  stablizedData[i][18] : Z
                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        //szy1[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy1[i].Y = stablizedData[i][7] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy1[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy1[i].Y = stablizedData[i][7] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y1 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy1, effLength, ref a1, ref b1);
                    double[] LUT1_tmp = new double[effLength];
                    double[] LUT1 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT1_tmp[i] = a1 * szy1[i].X - (szy1[i].Y - b1);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT1[i] = (LUT1_tmp[i - 1] + LUT1_tmp[i] + LUT1_tmp[i + 1]) / 3;

                    LUT1[0] = (2 * LUT1_tmp[0] + LUT1_tmp[1]) / 3;
                    LUT1[effLength - 1] = (2 * LUT1_tmp[effLength - 1] + LUT1_tmp[effLength - 2]) / 3;

                    //  SZ vs Y2 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT2_tmp[i] = a * SZ[i] - ( Y2[i] - b )
                    //  LUT2[i] = ( LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT2[0] = ( 2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3 ;
                    //  LUT2[N-1] = ( 2 * LUT2_tmp[N-1] + LUT2_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy2[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy2[i].Y = stablizedData[i][9] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy2[i].X = stablizedData[i][18];   //  Z from 6 axis stage
                        szy2[i].Y = stablizedData[i][9] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y2 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy2, effLength, ref a2, ref b2);
                    double[] LUT2_tmp = new double[effLength];
                    double[] LUT2 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT2_tmp[i] = a2 * szy2[i].X - (szy2[i].Y - b2);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT2[i] = (LUT2_tmp[i - 1] + LUT2_tmp[i] + LUT2_tmp[i + 1]) / 3;

                    LUT2[0] = (2 * LUT2_tmp[0] + LUT2_tmp[1]) / 3;
                    LUT2[effLength - 1] = (2 * LUT2_tmp[effLength - 1] + LUT2_tmp[effLength - 2]) / 3;

                    //  axis == 0 : YLUT 의 경우 Z scale 도 같이 저장
                    //  SZ vs Y3 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUT3_tmp[i] = a * SZ[i] - ( Y3[i] - b )
                    //  LUT3[i] = ( LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUT3[0] = ( 2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3 ;
                    //  LUT3[N-1] = ( 2 * LUT3_tmp[N-1] + LUT3_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        //szy3[i].X = ( stablizedData[i][16] + stablizedData[i][19] ) / 2;   //  ( Z1 + Z2 ) / 2
                        //szy3[i].Y = stablizedData[i][11] - stablizedData[i][18] * ProbeYtoSideViewPixel;
                        szy3[i].X = stablizedData[i][18];      //  Z from 6 axis stage
                        szy3[i].Y = stablizedData[i][11] - stablizedData[i][17] * ProbeYtoSideViewPixel; // Y3 - probe Y in pixel unit ; from 6 axis stage
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szy3, effLength, ref a3, ref b3);
                    double[] LUT3_tmp = new double[effLength];
                    double[] LUT3 = new double[effLength];
                    for (int i = 0; i < effLength; i++)
                        LUT3_tmp[i] = a3 * szy3[i].X - (szy3[i].Y - b3);

                    for (int i = 1; i < effLength - 1; i++)
                        LUT3[i] = (LUT3_tmp[i - 1] + LUT3_tmp[i] + LUT3_tmp[i + 1]) / 3;

                    LUT3[0] = (2 * LUT3_tmp[0] + LUT3_tmp[1]) / 3;
                    LUT3[effLength - 1] = (2 * LUT3_tmp[effLength - 1] + LUT3_tmp[effLength - 2]) / 3;

                    //  Z scale
                    for (int i = 0; i < effLength; i++)
                    {
                        szz[i].Y = stablizedData[i][18];        //  Z from 6 axis stage
                        szz[i].X = stablizedData[i][2];         //  Z 변위의 CSHead 측정값
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(szz, effLength, ref a, ref b);
                    a = a * 0.9992;
                    //a = (a - 1) * 0.4 + 1; 
                    //  YLUT 에 의한 Scale 보상이 있으므로 측정된 Scale 의 40% 만 보상해준다. 40% 는 실험적으로 확인됬으나,
                    //  정규 Calibration 시에는 얻어진 결과에 따라 Z Scale 을 직접 조정해줘야 할 것으로 예상.
                    //  Scale 만 조정해가면서 수차례 반복 필요
                    //  LUT 가 PP를 최소화하는 방식이 아니고 LMS 오차가 최소화되는 방향이므로 Z scale 수작업 조정 필요 


                    if ( mAutoCalibrationCount % 2 == 0 && !isAutoCalibrationEastView )
                    {
                        string srcFile = AdminPathName + "YLUT" + cameraID + ".csv";
                        string destFile = DoNotTouchPathName + "YLUT" + cameraID + ".csv";
                        wr = new StreamWriter(srcFile);
                        wr.WriteLine("Y Index," + fovYoffset.ToString() + ",Z Scale," + a.ToString());
                        wr.WriteLine("Y1," + a1.ToString() + ",Y2," + a2.ToString() + ",Y3," + a3.ToString());
                        for (int i = 0; i < effLength; i++)
                        {
                            wr.WriteLine(szy1[i].Y.ToString() + "," + LUT1[i].ToString() + "," + szy2[i].Y.ToString() + "," + LUT2[i].ToString() + "," + szy3[i].Y.ToString() + "," + LUT3[i].ToString());
                        }
                        wr.Close();
                        System.IO.File.Copy(srcFile, destFile, true);
                    }

                    /////////////////////////////////////////////////////////////////////////////////
                    //  Z to X 계산
                    //  Z vs X - Xprobe , Z vs Y - Yprobe
                    FZMath.Point2D[] sZtoX = new FZMath.Point2D[effLength];
                    FZMath.Point2D[] sZtoY = new FZMath.Point2D[effLength];
                    for (int i = 0; i < effLength; i++)
                    {
                        //sZtoX[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][0] - stablizedData[i][17]);   //  X - probe X
                        //sZtoY[i] = new FZMath.Point2D(szy1[i].X, stablizedData[i][1] - stablizedData[i][18]);   //  Y - probe Y
                        sZtoX[i] = new FZMath.Point2D(szz[i].X, stablizedData[i][0] - stablizedData[i][16]);   //  X - probe X from 6 axis stage
                        sZtoY[i] = new FZMath.Point2D(szz[i].X, stablizedData[i][1] - stablizedData[i][17]);   //  Y - probe Y from 6 axis stage
                    }

                    if (!isAutoCalibrationEastView)
                    {
                        lstr = "ZZ Scale,\t" + a.ToString("E5") + "\r\n";
                        // ZtoX, ZtoY 수정
                        double aZtoX = 0;
                        double aZtoY = 0;
                        m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoX, effLength, ref aZtoX, ref b);
                        lstr += "ZtoX,\t" + aZtoX.ToString("E5") + "\r\n";
                        m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sZtoY, effLength, ref aZtoY, ref b);
                        lstr += "ZtoY,\t" + aZtoY.ToString("E5") + "\r\n";
                    

                        if ( mAutoCalibrationCount % 2 == 0)
                        {
                            string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                            StreamReader sr = new StreamReader(scaleNthetaFile);
                            string allstr = sr.ReadToEnd();
                            sr.Close();
                            string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string[] strZscaleLine = allLines[2].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[2] = a.ToString("E5") + "\t//";
                            for (int i = 1; i < strZscaleLine.Length; i++)
                                allLines[2] += strZscaleLine[i];

                            string[] strZtoXLine = allLines[6].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[6] = aZtoX.ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoXLine.Length; i++)
                                allLines[6] += strZtoXLine[i];

                            string[] strZtoYLine = allLines[7].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[7] = aZtoY.ToString("E5") + "\t//";
                            for (int i = 1; i < strZtoYLine.Length; i++)
                                allLines[7] += strZtoYLine[i];

                            wr = new StreamWriter(scaleNthetaFile);
                            for (int i = 0; i < allLines.Length; i++)
                            {
                                wr.WriteLine(allLines[i]);
                            }
                            wr.Close();
                        }
                    }


                    //////////////////////////////////////////////////////////////////////////////////

                    break;
                case "X":
                    //  Axis = 1 : X scale 확인 및 저장
                    //  SX vs Xavg ( = (X4+X5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTX_tmp[i] = a * SX[i] - ( Xavg[i] - b )
                    //  LUTX[i] = ( LUTX_tmp[i - 1] + LUTX_tmp[i] + LUTX_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTX[0] = ( 2 * LUTX_tmp[0] + LUTX_tmp[1]) / 3 ;
                    //  LUTX[N-1] = ( 2 * LUTX_tmp[N-1] + LUTX_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sXX[i].Y = stablizedData[i][16];    //  X 변위의 Displacement Sensor 측정값   6 axis stage
                        sXX[i].X = stablizedData[i][0];     //  X 변위의 CSHead 측정값
                    }
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXX, effLength, ref a, ref b);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXX, effLength, ref p2ndCoef);  //  A[0]X^2 + A[1]X + A[2]

                    lstr = "XX Scale,\t" + p2ndCoef[0].ToString("E5") + ",\t" + p2ndCoef[1].ToString("E5") + ",\t" + p2ndCoef[2].ToString("E5") + "\r\n";

                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe
                    FZMath.Point2D[] sXtoY = new FZMath.Point2D[effLength];
                    FZMath.Point2D[] sXtoZ = new FZMath.Point2D[effLength];
                    for (int i = 0; i < effLength; i++)
                    {
                        //sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][18]);
                        //sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);
                        sXtoY[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][1] - stablizedData[i][17]);    //  Y - probe Y     from 6axis stage
                        sXtoZ[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z      from 6axis stage
                    }
                    double aXtoY = 0;
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoY, effLength, ref aXtoY, ref b);
                    lstr += "XtoY,\t" + aXtoY.ToString("E5") + "\r\n";
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sXtoZ, effLength, ref p2ndCoef2);
                    lstr += "XtoZ,\t" + p2ndCoef2[0].ToString("E5") + ",\t" + p2ndCoef2[1].ToString("E5") + ",\t" + p2ndCoef2[2].ToString("E5") + "\r\n";

                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe
                    FZMath.Point2D[] sXtoTX = new FZMath.Point2D[effLength];
                    for (int i = 0; i < effLength; i++)
                    {
                        sXtoTX[i] = new FZMath.Point2D(sXX[i].X, stablizedData[i][3]);
                    }
                    double aXtoTX = 0;
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sXtoTX, effLength, ref aXtoTX, ref b);
                    lstr += "XtoTX,\t" + aXtoTX.ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "XXLUT" + cameraID + ".csv");
                    //for (int i = 0; i < effLength; i++)
                    //    lstr += sXtoZ[i].X.ToString("F4") + "," + sXtoZ[i].Y.ToString("F4") + "\r\n";
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////
                    if (mAutoCalibrationCount%2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strXXscaleLine = allLines[0].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[0] = p2ndCoef[0].ToString("E5") + "\t" + p2ndCoef[1].ToString("E5") + "\t" + p2ndCoef[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXXscaleLine.Length; i++)
                            allLines[0] += strXXscaleLine[i];

                        string[] strXtoYLine = allLines[10].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[10] = aXtoY.ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoYLine.Length; i++)
                            allLines[10] += strXtoYLine[i];

                        string[] strXtoZLine = allLines[11].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[11] = p2ndCoef2[0].ToString("E5") + "\t" + p2ndCoef2[1].ToString("E5") + "\t" + p2ndCoef2[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoZLine.Length; i++)
                            allLines[11] += strXtoZLine[i];

                        string[] strXtoTXLine = allLines[13].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[13] = aXtoTX.ToString("E5") + "\t//";
                        for (int i = 1; i < strXtoTXLine.Length; i++)
                            allLines[13] += strXtoTXLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                case "Y":
                    mYCalAvgY1Y2pp = Math.Abs(stablizedData[0][7] - stablizedData[effLength - 1][7] + stablizedData[0][9] - stablizedData[effLength - 1][9]) / 2;
                    mYCalY3pp = Math.Abs(stablizedData[0][11] - stablizedData[effLength - 1][11]);
                    mEstimatedEastViewYscale = (mYCalAvgY1Y2pp + mZCalAvgY1Y2pp) / (mYCalY3pp + mZCalY3pp);

                    //  Axis = 2 : Y scale 확인 및 저장
                    //  SY vs Yavg ( = (Y4+Y5) / 2 ) 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sYY[i].Y = stablizedData[i][17];    //  Y 변위의 Displacement Sensor 측정값   from 6 axis stage
                        sYY[i].X = stablizedData[i][1];
                    }
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYY, effLength, ref a, ref b);
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYY, effLength, ref p2ndCoef);

                    lstr = "YY Scale,\t" + p2ndCoef[0].ToString("E5") + ",\t" + p2ndCoef[1].ToString("E5") + ",\t" + p2ndCoef[2].ToString("E5") + "\r\n";
                    /////////////////////////////////////////////////////////////////////////////////
                    //  X to Y ／Ｚ　계산
                    //  X vs Y - Yprobe , X vs Z - Zprobe
                    FZMath.Point2D[] sYtoX = new FZMath.Point2D[effLength];
                    FZMath.Point2D[] sYtoZ = new FZMath.Point2D[effLength];
                    for (int i = 0; i < effLength; i++)
                    {
                        //sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][17]);    //  X - probe X
                        //sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - (stablizedData[i][16] + stablizedData[i][19]) / 2);   //  Z - probe Z
                        sYtoX[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][0] - stablizedData[i][16]);    //  X - probe X     from 6 axis stage
                        sYtoZ[i] = new FZMath.Point2D(sYY[i].X, stablizedData[i][2] - stablizedData[i][18]);   //  Z - probe Z     from 6 axis stage
                    }
                    double aYtoX = 0;
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoX, effLength, ref aYtoX, ref b);
                    lstr += "YtoX,\t" + aYtoX.ToString("E5") + "\r\n";
                    //m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sYtoZ, effLength, ref a, ref b);
                    //lstr += a.ToString("F6");
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sYtoZ, effLength, ref p2ndCoef2);
                    lstr += "YtoZ,\t" + p2ndCoef2[0].ToString("E5") + ",\t" + p2ndCoef2[1].ToString("E5") + ",\t" + p2ndCoef2[2].ToString("E5") + "\r\n";

                    if (isAutoCalibrationEastView)
                    {
                        lstr = "EastViewYscale,\t" + mEstimatedEastViewYscale.ToString("F6") + "\r\n";
                    }

                    wr = new StreamWriter(AdminPathName + "YYLUT" + cameraID + ".csv");
                    wr.Write(lstr);
                    wr.Close();

                    //////////////////////////////////////////////////////////////////////////////////                    
                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        sr.Close();
                        if (isAutoCalibrationEastView)
                        {
                            string[] strEastScaleLine = allLines[12].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[12] = mEstimatedEastViewYscale.ToString("E5") + "\t//";
                            for (int i = 1; i < strEastScaleLine.Length; i++)
                                allLines[12] += strEastScaleLine[i];
                        }
                        else
                        {
                            string[] strYYscaleLine = allLines[1].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[1] = p2ndCoef[0].ToString("E5") + "\t" + p2ndCoef[1].ToString("E5") + "\t" + p2ndCoef[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYYscaleLine.Length; i++)
                                allLines[1] += strYYscaleLine[i];

                            string[] strYtoXLine = allLines[8].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[8] = aYtoX.ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoXLine.Length; i++)
                                allLines[8] += strYtoXLine[i];

                            string[] strYtoZLine = allLines[9].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            allLines[9] = p2ndCoef2[0].ToString("E5") + "\t" + p2ndCoef2[1].ToString("E5") + "\t" + p2ndCoef2[2].ToString("E5") + "\t//";
                            for (int i = 1; i < strYtoZLine.Length; i++)
                                allLines[9] += strYtoZLine[i];
                        }

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;

                //  이하는 YLUTs , X Scale 적용한 후에 수행해야 함.
                case "TX":
                    //  Axis = 3 : TXLUT 의 경우 TX scale 확인 및 저장
                    //  SY vs TX 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;

                    //  stablizedData[i][19] : TX   rad
                    //  stablizedData[i][20] : TY   rad
                    //  stablizedData[i][21] : TZ   rad

                    for (int i = 0; i < effLength; i++)
                    {
                        sTXTX[i].Y = stablizedData[i][19] * rad2min ;    //  Tilt X 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTXTX[i].X = stablizedData[i][3];
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS2ndPoly(sTXTX, effLength, ref p2ndCoef2);
                    lstr += "TX Scale,\t" + p2ndCoef2[0].ToString("E5") + ",\t" + p2ndCoef2[1].ToString("E5") + ",\t" + p2ndCoef2[2].ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TXLUT" + cameraID + ".csv");
                    wr.WriteLine("TX Scale,\t" + p2ndCoef2[0].ToString("E5") + ",\t" + p2ndCoef2[1].ToString("E5") + ",\t" + p2ndCoef2[2].ToString("E5") + "\r\n");
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strTXscaleLine = allLines[4].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[4] = p2ndCoef2[0].ToString("E5") + "\t" + p2ndCoef2[1].ToString("E5") + "\t" + p2ndCoef2[2].ToString("E5") + "\t//";
                        for (int i = 1; i < strTXscaleLine.Length; i++)
                            allLines[4] += strTXscaleLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 1; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                case "TY":
                    //  Axis = 4 : TYLUT 의 경우 TY scale 확인 및 저장
                    //  SY vs TY 배열을 생성하여 LMS 의 a,b 를 구한다.
                    //  데이터 개수가 N 개일 때
                    //  LUTY_tmp[i] = a * SY[i] - ( Yavg[i] - b )
                    //  LUTY[i] = ( LUTY_tmp[i - 1] + LUTY_tmp[i] + LUTY_tmp[i + 1] ) / 3 ; 0 < i < N-1
                    //  LUTY[0] = ( 2 * LUTY_tmp[0] + LUTY_tmp[1]) / 3 ;
                    //  LUTY[N-1] = ( 2 * LUTY_tmp[N-1] + LUTY_tmp[N-2]) / 3 ;
                    for (int i = 0; i < effLength; i++)
                    {
                        sTYTY[i].Y = stablizedData[i][20] * rad2min;    //  Tilt Y 를 위한 Z 변위의 Displacement Sensor 측정값에서 CSH Z 변위를 소거된 값이어야 함
                        sTYTY[i].X = stablizedData[i][4];
                    }
                    m__G.oCam[0].mFAL.mFZM.mcLMS1stPoly(sTYTY, effLength, ref a, ref b);

                    lstr += "TY Scale,\t" + a.ToString("E5") + "\r\n";

                    wr = new StreamWriter(AdminPathName + "TYLUT" + cameraID + ".csv");
                    wr.WriteLine("TY Scale," + a.ToString());
                    wr.Close();

                    if (mAutoCalibrationCount % 2 == 0)
                    {
                        string scaleNthetaFile = DoNotTouchPathName + "ScaleNTheta" + cameraID + ".txt";
                        StreamReader sr = new StreamReader(scaleNthetaFile);
                        string allstr = sr.ReadToEnd();
                        sr.Close();
                        string[] allLines = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] strTYscaleLine = allLines[5].Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        allLines[5] = a.ToString("E5") + "\t//";
                        for (int i = 0; i < strTYscaleLine.Length; i++)
                            allLines[5] += strTYscaleLine[i];

                        wr = new StreamWriter(scaleNthetaFile);
                        for (int i = 1; i < allLines.Length; i++)
                        {
                            wr.WriteLine(allLines[i]);
                        }
                        wr.Close();
                    }
                    break;
                default:
                    break;
            }
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbCalibration.Text += lstr;
                });
            else
                tbCalibration.Text += lstr;

        }
        private void btnSingleCalibration_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
                btnSaveCalibration.Text = "Stop Single Cal.";
            }
            else
            {
                mAutoCalibrationRun = false;
                isAutoCalibrationEastView = false;
                btnSaveCalibration.Text = "Single Cal.";
                return;
            }

            mAutoCalibrationCount = 1;  // Sngle Cal.은 수동으로 적용하고 결과 확인용

            tbCalibration.Text = "";
            int iAxis = 5;
            double onewayStroke = 1900;
            if (tbMaxStroke.Text.Length > 1)
                onewayStroke = double.Parse(tbMaxStroke.Text);

            if (rbCalZ.Checked)
                iAxis = 2;
            else if (rbCalX.Checked)
                iAxis = 0;
            else if (rbCalY.Checked)
                iAxis = 1;
            else if (rbCalTX.Checked)
                iAxis = 3;
            else if (rbCalTY.Checked)
                iAxis = 4;
            else if (rbCalTZ.Checked)
                iAxis = 5;


            if (rbCalEastView.Checked)
            {
                isAutoCalibrationEastView = true;

                double yOnewayStroke = onewayStroke;

                double zOnewayStroke = 1750;

                if (tbZMaxStroke.Text.Length > 1)
                    zOnewayStroke = double.Parse(tbZMaxStroke.Text);

                mCalibrationFullData.Clear();
                AutoCalibration(2, zOnewayStroke);
                mCalibrationFullData.Clear();
                AutoCalibration(1, yOnewayStroke);

                isAutoCalibrationEastView = false;
            }
            else
            {
                mCalibrationFullData.Clear();
                AutoCalibration(iAxis, onewayStroke);
            }

            mAutoCalibrationRun = false;
            btnSaveCalibration.Text = "Single Cal.";
        }

        private void btnCheckFovBalance_Click(object sender, EventArgs e)
        {
            m__G.oCam[0].mTrgBufLength = 3000;
            m__G.oCam[0].mTargetTriggerCount = 3000;
            m__G.oCam[0].GrabB(0, true);
            m__G.oCam[0].ShowBalancingImage();
            SetDefaultMarkConfig(true);

        }
        bool IsLiveCropStop = false;
        private void button10_Click_1(object sender, EventArgs e)
        {
            IsLiveCropStop = false;
            m__G.oCam[0].DrawAllRectangles();
            
            Task.Factory.StartNew(() => {
                while (true)
                {
                    Thread.Sleep(100);
                    if (pictureBox2.InvokeRequired)
                        BeginInvoke((MethodInvoker)delegate
                        {
                            pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);
                        });
                    else
                        pictureBox2.Image = m__G.oCam[0].LoadCropImg(0);

                    if (IsLiveCropStop)
                        break;
                }

            });
        }


        
        private void button12_Click(object sender, EventArgs e)
        {
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
            groupBox4.Hide();
            btnChangeCrop.Show();
        }

        private void btnChangeCrop_Click(object sender, EventArgs e)
        {
            btnChangeCrop.Hide();
            groupBox4.Show();
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
            if (m__G.f_PIMotion == null) return;

            m__G.f_PIMotion.Show();
            m__G.f_PIMotion.BringToFront();
        }

        public int mAutoCalibrationCount = 0;
        public bool mAutoCalibrationRun = false;
        public bool isAutoCalibrationEastView = false;
        private void button13_Click(object sender, EventArgs e)
        {
            if (!mAutoCalibrationRun)
            {
                mAutoCalibrationRun = true;
                button13.Text = "Stop Auto Calibration";
            }
            else
            {
                mAutoCalibrationRun = false;
                isAutoCalibrationEastView = false;
                button13.Text = "Auto Cal Before-After";
                return;
            }

            mAutoCalibrationCount = 0;
            tbCalibration.Text = "";

            int iAxis = 5;
            double onewayStroke = 1900;

            if (rbCalZ.Checked)
                iAxis = 2;
            else if (rbCalX.Checked)
                iAxis = 0;
            else if (rbCalY.Checked)
                iAxis = 1;
            else if (rbCalTX.Checked)
                iAxis = 3;
            else if (rbCalTY.Checked)
                iAxis = 4;
            else if (rbCalTZ.Checked)
                iAxis = 5;

            if ( tbMaxStroke.Text.Length > 1 )
                onewayStroke = double.Parse( tbMaxStroke.Text );

            if (rbCalEastView.Checked)
            {
                isAutoCalibrationEastView = true;

                double yOnewayStroke = onewayStroke;

                double zOnewayStroke = 1750;
                if (tbZMaxStroke.Text.Length > 1)
                    zOnewayStroke = double.Parse(tbZMaxStroke.Text);

                AutoCalibrationEastView(yOnewayStroke, zOnewayStroke);
                isAutoCalibrationEastView = false;
            }
            else
            {
                AutoCalibrationWrapper(iAxis, onewayStroke);    
            }

            mAutoCalibrationRun = false;
            button13.Text = "Auto Cal Before-After";

            //Task.Run(() => AutoCalibrationWrapper(iAxis, onewayStroke));
        }

        public void InitializeScaleNTheta()
        {
            ms_scaleX = new double[3] { 0, 1, 0 };
            ms_scaleY = new double[3] { 0, 1, 0 };
            ms_scaleZ = 1;
            ms_scaleTX = new double[3] { 0, 1, 0 };
            ms_scaleTY = 1;
            ms_ZtoXbyView = 0;
            ms_ZtoYbyView = 0;
            ms_YtoXbyView = 0;
            ms_YtoZbyView = new double[3] { 0, 0, 0 };
            ms_XtoYbyView = 0;
            ms_XtoZbyView = new double[3] { 0, 0, 0 };
            ms_EastViewYPscale = 1;
            ms_XtoTXbyView = 0;
        }
        public void AutoCalibrationWrapper(int iAxis, double onewayStrokeUm)
        {
            mCalibrationFullData.Clear();
            AutoCalibration(iAxis, onewayStrokeUm);

            mAutoCalibrationCount++;

            mCalibrationFullData.Clear();
            string smgzYLUT = "";
            if (LoadscaleNTheta())
                smgzYLUT = "ScaleNTheta" + m__G.mCamID0.ToString() + " is loaded\r\n";
            else
                smgzYLUT = "Failt to load ScaleNTheta" + m__G.mCamID0.ToString() + " \r\n";

            if (m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]))
                smgzYLUT += "YLUT" + m__G.mCamID0.ToString() + " is loaded\r\n";
            else
                smgzYLUT += "Failt to load YLUT" + m__G.mCamID0.ToString() + " \r\n";

            if (tbInfo.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbInfo.Text += smgzYLUT;
                });
            else
                tbInfo.Text += smgzYLUT;

            AutoCalibration(iAxis, onewayStrokeUm);
        }

       
        public void AutoCalibrationEastView(double yOnewayStrokeUm, double zOnewayStrokeUm)
        {
            mCalibrationFullData.Clear();
            AutoCalibration(2, zOnewayStrokeUm);
            mCalibrationFullData.Clear();
            AutoCalibration(1, yOnewayStrokeUm);

            mAutoCalibrationCount++;

            mCalibrationFullData.Clear();
            LoadscaleNTheta();
            m__G.mFAL.GetYLUT(m__G.mCamID0, v_OrgROIV_min[0]);
            AutoCalibration(2, zOnewayStrokeUm);
            mCalibrationFullData.Clear();
            AutoCalibration(1, yOnewayStrokeUm);
        }


        bool mbUseZ123 = false;
        public void AutoCalibration(int iAxis, double onewayStrokeUm)
        {
            if (!mAutoCalibrationRun)
                return;

            if (iAxis == 2)
                mbUseZ123 = true;
            else
                mbUseZ123 = false;
            //  초기위치가 ORG 인 것으로 가정한다.

            int onewayStrokePulse = (int)(100 * onewayStrokeUm);
            mAutoCalibrationIndex = 0;

            MotorSpeedSet(1, 1, 1, 1, 1, 1);
            Thread.Sleep(10);
            //  X,Y,Z 1pulse = 0.01um

            double orgPos = MotorCurPos(iAxis);
            double pos = orgPos - (onewayStrokePulse) / 3;


            if (m__G.mGageCounter != null)
                m__G.mGageCounter.OpenAllport();

            SingleFindMark();
            MotorMoveAxisAbs(iAxis, pos);

            if (!mAutoCalibrationRun)
                return;

            Thread.Sleep(300);
            SingleFindMark();
            pos = orgPos - 2 * onewayStrokePulse / 3;
            MotorMoveAxisAbs(iAxis, pos);

            if (!mAutoCalibrationRun)
                return;

            Thread.Sleep(300);
            SingleFindMark();
            if ( iAxis < 3)
                pos = orgPos - onewayStrokePulse - 30000;
            else
                pos = orgPos - onewayStrokePulse - 3000;

            MotorMoveAxisAbs(iAxis, pos);
            if (!mAutoCalibrationRun)
                return;

            if (iAxis < 3)
                pos = orgPos - onewayStrokePulse - 1000;
            else
                pos = orgPos - onewayStrokePulse - 100;

            Thread.Sleep(500);
            MotorMoveAxisAbs(iAxis, pos);
            Thread.Sleep(300);
            SingleFindMark();
            Thread.Sleep(200);
            SingleFindMark();
            if (!mAutoCalibrationRun)
                return;

            pos = orgPos - onewayStrokePulse;
            MotorMoveAxisAbs(iAxis, pos);
            Thread.Sleep(200);
            SingleFindMark();
            if (!mAutoCalibrationRun)
                return;

            // dStrokeUm : tx,ty일 경우 추가
            double dStrokeUm = onewayStrokePulse / (onewayStrokeUm * 0.01);

            if (rbCalTX.Checked)
                dStrokeUm = 2040;   // TX : 1pulse /0.2 deg
            else if (rbCalTY.Checked)
                dStrokeUm = 1745;   // TY : 1pulse /0.2 deg
            else if (rbCalTZ.Checked)
                dStrokeUm = 2000;   // TZ : 1pulse /0.2 deg
            else
                dStrokeUm = onewayStrokePulse / (onewayStrokeUm * 0.01);

            double movingStroke = -onewayStrokePulse;
            while (movingStroke < onewayStrokePulse)
            {
                if (!mAutoCalibrationRun)
                    return;

                pos += dStrokeUm;
                movingStroke += dStrokeUm;
                MotorMoveAxisAbs(iAxis, pos);
                Thread.Sleep(200);
                SingleFindMark();
            }

            if (m__G.mGageCounter != null)
                m__G.mGageCounter.CloseAllport();

            MotorMoveAxisAbs(iAxis, orgPos);
            string sAxis = "";
            switch (iAxis)
            {
                case 0:
                    sAxis = "X";
                    break;
                case 1:
                    sAxis = "Y";
                    break;
                case 2:
                    sAxis = "Z";
                    break;
                case 3:
                    sAxis = "TX";
                    break;
                case 4:
                    sAxis = "TY";
                    break;
                case 5:
                    sAxis = "TZ";
                    break;
            }
            RemoteCalibration(sAxis, 5);
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

            //double[] curpos = MotorCurPos6axis();
            //StreamWriter wr = new StreamWriter(sFilePath);
            //for ( int i=0; i< curpos.Length; i++)
            //{
            //    wr.WriteLine(curpos[i].ToString());
            //}
            //wr.Close();
        }
        public void LoadStageOrg()
        {
            //string sFilePath = m__G.m_RootDirectory + "\\DoNotTouch\\StageOrg.txt";
            //double[] curpos = new double[6];
            //StreamReader rd = new StreamReader(sFilePath);
            //string lstr = rd.ReadToEnd();
            //rd.Close();

            //string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < allLines.Length; i++)
            //{
            //    curpos[i] = double.Parse(allLines[i]);
            //}
            //return curpos;
        }

        private void cbZaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbZaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbRaxis.Checked = false;
            }
            //else
            //{
            //    cbTiltAxis.Enabled = true;
            //    cbRaxis.Enabled = true;
            //}
        }

        private void cbRaxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRaxis.Checked)
            {
                cbTiltAxis.Checked = false;
                cbZaxis.Checked = false;
            }
            //else
            //{
            //    cbTiltAxis.Enabled = true;
            //    cbZaxis.Enabled = true;
            //}
        }

        private void cbTiltAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTiltAxis.Checked)
            {
                cbRaxis.Checked = false;
                cbZaxis.Checked = false;
            }
            //else
            //{
            //    cbRaxis.Enabled = true;
            //    cbZaxis.Enabled = true;
            //}
        }

        private void btnGobackOrg_Click(object sender, EventArgs e)
        {
            MotorHome6D();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

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
        public VolumetricTP[] mVMPts = null;
        public VolumetricTP[] mVMTPts = null;

        public int mAutoFullRange = 0;  // 0 ~ 100, 0 은 Auto 가 아닌 경우
        private void btnApplyVolumetricMeasure_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 0;
            ApplyVolumetricMeasure();
        }

        public void ApplyVolumetricMeasure()
        {
            double timeEst = 0;

            int x = int.Parse(tbXstep.Text);
            int y = int.Parse(tbYstep.Text);
            int z = int.Parse(tbZstep.Text);
            int tx = int.Parse(tbTXstep.Text);
            int ty = int.Parse(tbTYstep.Text);
            int tz = int.Parse(tbTZstep.Text);
            double xr = (int)(200 * double.Parse(tbXrange.Text)); //  100 입력 시 100um
            double yr = (int)(200 * double.Parse(tbYrange.Text)); //  100 입력 시 100um
            double zr = (int)(200 * double.Parse(tbZrange.Text)); //  100 입력 시 100um
            double txr = (int)(100 * double.Parse(tbTXrange.Text)); //  100 입력 시 ~0.6deg
            double tyr = (int)(100 * double.Parse(tbTYrange.Text)); //  100 입력 시 ~0.6deg
            double tzr = (int)(100 * double.Parse(tbTZrange.Text)); //  100 입력 시 ~0.6deg

            if (x < 2)
                x = 2;
            if (y < 2)
                y = 2;
            if (z < 2)
                z = 2;
            if (tx < 2)
                tx = 2;
            if (ty < 2)
                ty = 2;
            if (tz < 2)
                tz = 2;

            timeEst = x * y * z * tx * ty * tz;
            mVMPts = Generate3dTrajectory(xr / (x - 1), yr / (y - 1), zr / (z - 1), xr, yr, zr);

            List<VolumetricTP> pTList = new List<VolumetricTP>();
            VolumetricTP pT3d_dummy = new VolumetricTP();
            VolumetricTP pT3d = new VolumetricTP();

            if (mAutoFullRange > 0)
            {
                txr = mAutoFullRange;   //  1% ~ 100% -> 100 ~ 10000
                tyr = mAutoFullRange * 0.8725;   //  1% ~ 100% -> 100 ~ 10000   Y 축은 약간 작다
                tzr = mAutoFullRange;   //  1% ~ 100% -> 100 ~ 10000
            }

            double curtx = -txr;
            double curty = -tyr;
            double curtz = -tzr;
            double oldtx = 0;
            double oldty = 0;
            double oldtz = 0;



            pT3d_dummy = new VolumetricTP(curtx / 4, curty / 4, curtz / 4, false);
            pTList.Add(pT3d_dummy);
            pT3d_dummy = new VolumetricTP(curtx / 2, curty / 2, curtz / 2, false);
            pTList.Add(pT3d_dummy);
            pT3d_dummy = new VolumetricTP(3 * curtx / 4, 3 * curty / 4, 3 * curtz / 4, false);
            pTList.Add(pT3d_dummy);
            while (curtz <= tzr)
            {
                curtx = -txr;
                curty = -tyr;

                while (curty <= tyr)
                {
                    curtx = -txr;
                    while (curtx <= txr)
                    {
                        pT3d = new VolumetricTP(curtx, curty, curtz, true);
                        pTList.Add(pT3d);

                        curtx += (2 * txr) / (tx - 1);
                    }
                    curtx -= (2 * txr) / (tx - 1);  // 마지막에 영역 넘어 추가로 더해진 부분 소거

                    oldtx = curtx;  // 현재 점
                    curtx = -txr;   //  다음 목표점
                    oldty = curty;
                    curty += (2 * tyr) / (ty - 1);

                    if (curty <= tyr)
                    {
                        pT3d_dummy = new VolumetricTP(oldtx + 1 * (curtx - oldtx) / 3, oldty + 1 * (curty - oldty) / 3, curtz, false);
                        pTList.Add(pT3d_dummy);
                        pT3d_dummy = new VolumetricTP(oldtx + 2 * (curtx - oldtx) / 3, oldty + 2 * (curty - oldty) / 3, curtz, false);
                        pTList.Add(pT3d_dummy);
                    }
                }
                curty -= (2 * tyr) / (ty - 1);  // 마지막에 영역 넘어 추가로 더해진 부분 소거

                oldtx = curtx;  // 현재 점
                oldty = curty;  // 현재 점
                oldtz = curtz;  // 현재 점
                curtx = -txr;   //  다음 목표점
                curty = -tyr;   //  다음 목표점
                curtz += (2 * tzr) / (tz - 1);   //  다음 목표점
                if (curtz <= tzr)
                {
                    pT3d_dummy = new VolumetricTP(oldtx + 1 * (curtx - oldtx) / 5, oldty + 1 * (curty - oldty) / 5, oldtz + 1 * (curtz - oldtz) / 5, false);
                    pTList.Add(pT3d_dummy);
                    pT3d_dummy = new VolumetricTP(oldtx + 2 * (curtx - oldtx) / 5, oldty + 2 * (curty - oldty) / 5, oldtz + 2 * (curtz - oldtz) / 5, false);
                    pTList.Add(pT3d_dummy);
                    pT3d_dummy = new VolumetricTP(oldtx + 3 * (curtx - oldtx) / 5, oldty + 3 * (curty - oldty) / 5, oldtz + 3 * (curtz - oldtz) / 5, false);
                    pTList.Add(pT3d_dummy);
                    pT3d_dummy = new VolumetricTP(oldtx + 4 * (curtx - oldtx) / 5, oldty + 4 * (curty - oldty) / 5, oldtz + 4 * (curtz - oldtz) / 5, false);
                    pTList.Add(pT3d_dummy);
                }
            }
            curtz = 0;
            curtx = 0;
            curty = 0;
            pT3d_dummy = new VolumetricTP(oldtx + 1 * (curtx - oldtx) / 5, oldty + 1 * (curty - oldty) / 5, oldtz + 1 * (curtz - oldtz) / 5, false);
            pTList.Add(pT3d_dummy);
            pT3d_dummy = new VolumetricTP(oldtx + 2 * (curtx - oldtx) / 5, oldty + 2 * (curty - oldty) / 5, oldtz + 2 * (curtz - oldtz) / 5, false);
            pTList.Add(pT3d_dummy);
            pT3d_dummy = new VolumetricTP(oldtx + 3 * (curtx - oldtx) / 5, oldty + 3 * (curty - oldty) / 5, oldtz + 3 * (curtz - oldtz) / 5, false);
            pTList.Add(pT3d_dummy);
            pT3d_dummy = new VolumetricTP(oldtx + 4 * (curtx - oldtx) / 5, oldty + 4 * (curty - oldty) / 5, oldtz + 4 * (curtz - oldtz) / 5, false);
            pTList.Add(pT3d_dummy);
            mVMTPts = pTList.ToArray();

            StreamWriter wr = new StreamWriter("XYZ.csv");
            for (int i = 0; i < mVMTPts.Length; i++)
            {
                if (mVMTPts[i].bSubOn)
                    wr.WriteLine(mVMTPts[i].Pt.X.ToString("F0") + "," + mVMTPts[i].Pt.Y.ToString("F0") + "," + mVMTPts[i].Pt.Z.ToString("F0"));
            }
            wr.Close();


            tbVMEstTime.Text = (mVMPts.Length * mVMTPts.Length).ToString() + "sec expected.";
        }
        private void button14_Click(object sender, EventArgs e)
        {
            mbStopVolumetricMeasure = false;
            Task.Run(() => VolumetricMeasure());

        }

        public bool mbStopVolumetricMeasure = false;
        public void VolumetricMeasure()
        {
            mAutoCalibrationIndex = 0;
            MotorSpeedSet(1, 1, 1, 1, 1, 1);

            int pi = 0;
            int fullCnt = 0;
            double[] orgPos = MotorCurPos6axis();

            if (m__G.mGageCounter != null)
                m__G.mGageCounter.OpenAllport();

            mCalibrationFullData.Clear();

            int VMcount = mVMPts.Length;
            int stopCount = 0;

            double tAmplitude = 1;

            while (true)
            {
                if (mbStopVolumetricMeasure)
                    break;

                int iEnd = mVMTPts.Length;

                if (mAutoFullRange > 0)
                {
                    tAmplitude = (1900 - Math.Abs(mVMPts[pi].Pt.Y)/100 ) / 100 * 40.2 + (240 - Math.Abs(mVMPts[pi].Pt.Z)/100 ) / 100 * 48;  // min
                    tAmplitude = tAmplitude * 100 / 60; //  1deg = 100
                    if (tAmplitude > 400)
                        tAmplitude = 400;
                }

                if (mVMPts[pi].bSubOn)
                {
                    for (int i = 0; i < iEnd; i++)
                    {
                        if (mbStopVolumetricMeasure)
                            break;

                        MotorMove6D(mVMPts[pi].Pt.X + orgPos[0],
                                    mVMPts[pi].Pt.Y + orgPos[1],
                                    mVMPts[pi].Pt.Z + orgPos[2],
                                    mVMTPts[i].Pt.X * tAmplitude + orgPos[3],
                                    mVMTPts[i].Pt.Y * tAmplitude + orgPos[4],
                                    mVMTPts[i].Pt.Z * tAmplitude + orgPos[5]);

                        if (mVMTPts[i].bSubOn)
                            Thread.Sleep(100);
                        else
                            Thread.Sleep(10);

                        SingleFindMark(mVMTPts[i].bSubOn);

                        if (mVMTPts[i].bSubOn)
                            fullCnt++;

                        stopCount++;
                    }
                }
                else
                {
                    //if (stopCount > 1437)
                    //{
                        MotorMove6D(mVMPts[pi].Pt.X + orgPos[0],
                                mVMPts[pi].Pt.Y + orgPos[1],
                                mVMPts[pi].Pt.Z + orgPos[2],
                                orgPos[3],
                                orgPos[4],
                                orgPos[5]);
                        Thread.Sleep(10);
                        SingleFindMark(mVMPts[pi].bSubOn);
                    //}
                    if (mVMPts[pi].bSubOn)
                        fullCnt++;

                    stopCount++;
                }
                pi++;
                if (VMcount == pi)
                    break;

            }

            string fileName = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\";
            double[][] stablizedData = mCalibrationFullData.ToArray();
            double rad2min = 180 * 60 / Math.PI;
            double ProbeYtoSideViewPixel = Math.Sin(40 / 180 * Math.PI) / (5.5 / 0.3);

            //StreamWriter lwr = new StreamWriter(fileName + "Log" + fullCnt.ToString() + ".csv");
            //lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,pX,pY,pZ,pTX,pTY,pTZ");
            //lwr.Write(tbInfo.Text);
            //lwr.Close();

            StreamWriter lwr = new StreamWriter(fileName + "VolumetrixMeasure" + fullCnt.ToString() + ".csv");
            lwr.WriteLine("#,X,Y,Z,TX,TY,TZ,pX,pY,pZ,pTX,pTY,pTZ");
            for (int i = 0; i < fullCnt; i++)
            {
                string slstr = i.ToString() + ",";
                for (int j = 0; j < 22; j++)
                {
                    if (j > 5 && j < 16)
                        continue;

                    if (j < 19)
                        slstr += stablizedData[i][j].ToString("F5") + ",";
                    else
                    {
                        slstr += (rad2min * stablizedData[i][j]).ToString("F5") + ",";
                    }
                }
                lwr.WriteLine(slstr);
            }
            lwr.Close();

            if (m__G.mGageCounter != null)
                m__G.mGageCounter.CloseAllport();

        }

        private void button15_Click(object sender, EventArgs e)
        {
            mbStopVolumetricMeasure = true;
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
            int yinterval = (int)(yFullRange / pitchY+0.01);

            for (int j = -m; j <= m; j++)
            {
                VolumetricTP p0 = new VolumetricTP(0, 0, j * pitchZ);
                pList.Add(p0);
                if (mAutoFullRange > 0)
                {
                    pitchY = 2*( 1900 - (Math.Abs(p0.Pt.Z/100) - 240) / 0.8391 - 120 ) / yinterval;
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
            int x  = 100*int.Parse(tbXrange.Text);
            int y  = 100*int.Parse(tbYrange.Text);
            int z  = 100*int.Parse(tbZrange.Text);
            int tx = 100*int.Parse(tbTXrange.Text);
            int ty = 100*int.Parse(tbTYrange.Text);
            int tz = 100*int.Parse(tbTZrange.Text);
            MotorHome6D();
            Thread.Sleep(100);
            double[] orgPos = MotorCurPos6axis();


            MotorMove6D(x + orgPos[0],
                        y + orgPos[1],
                        z + orgPos[2],
                        tx + orgPos[3],
                        ty + orgPos[4],
                        tz + orgPos[5]);
        }

        private void rbCalTX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "350";   // 408
        }

        private void rbCalTY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "349";
        }

        private void rbCalTZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "400";
        }

        private void rbCalZ_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1750";
        }

        private void rbCalX_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";
        }

        private void rbCalY_CheckedChanged(object sender, EventArgs e)
        {
            tbMaxStroke.Text = "1900";
        }

        private void button16_Click(object sender, EventArgs e)
        {
            mAutoFullRange = 95;
            ApplyVolumetricMeasure();
        }

        private void rbCalEastView_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCalEastView.Checked)
            {
                lblYMaxStroke.Visible = true;
                lblZMaxStroke.Visible = true;
                tbZMaxStroke.Visible = true;
                tbMaxStroke.Text = "1900";
                tbZMaxStroke.Text = "1750";
            }
            else
            {
                lblYMaxStroke.Visible = false;
                lblZMaxStroke.Visible = false;
                tbZMaxStroke.Visible = false;
            }
        }
    }
}