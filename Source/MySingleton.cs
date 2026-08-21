using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using S2System.Vision;
using System.Runtime.InteropServices;
using System.IO;
using Dln;
using System.Globalization;
using System.Management;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using Microsoft.Win32;
using System.IO.Ports;          // using for QR_Reader
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using MotorizedStage_SK_PI;

// from FZ4P19110401_EPA-I_2PC_S6_AddHandler
namespace CSH030Ex
{
    public class SupremeTimer
    {
        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceCounter(ref Int64 count);

        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceFrequency(ref Int64 frequency);
    }

    class MySingleton
    {
    }

    public class Global
    {
        private static Global thisObject = null;
        private bool isDispose = false;
        public string strMsg = "";
        public StreamWriter DebugWriter;
        //bool mbSystemLogging = false;
        public string[] errMsg = new string[4] { "", "", "", "" };
        public string[] errSpecialMsg = new string[4] { "", "", "", "" };
        public string[] errMsgBackcup = new string[4] { "", "", "", "" };
        public string mNowLotName = "";
        public string m_LogText = "";
        string m_strPassword = "";
        public string mTesterNumber = "";
        public string mSystemLogFile = "";
        public bool mbVet = false;
        public bool[] mbSuddenStop = new bool[2] { false, false };
        public DateTime mLastDateTime;
        public int mForcedSampleNumber = 0;
        public int mForcedApplied = 0;
        public bool[] mbDontInterrupt = new bool[2] { false, false };
        public int[] mCurrentSequence = new int[2] { 0, 0 };
        public bool mbHavePH = false;
        public int mCamCount = 2;
        public int mChannelCount = 4;
        public bool[] m_bHallTestDone = new bool[4];
        public bool[] m_bHallTestFail = new bool[4];
        public DateTime[] mIdleTime = new DateTime[2];
        public bool mIsBU24532 = false;
        public bool mbEPAMode = false;
        public bool m_b2ndSettling = false;
        public bool m_b2ndPM = false;
        public bool m_bLC20C = true;
        public bool m_bLC20D = true;
        public bool m_bSC20C = true;
        public bool m_bSC20D = true;
        public bool m_bAutoLastFrame = false;
        public bool m_bAutoRemoveWrongDetection = false;
        public bool m_bYawTiltWihtTopEdge = false;
        public bool m_FirstFailMotionStop = false;
        public bool m_FullStrokeDuringHallCal = false;
        public bool m_ScriptLinearityCal = false;
        public bool m_bSaveLostTestSet = false;
        public bool m_bNoHostPC = false;
        public bool m_bTestStage = false;
        public bool m_bSaveNgImage = false;
        public bool m_bOISOption = false;
        public bool m_bSaveFImage = false;
        public bool m_bPseudoOMM = false;

        public bool m_bcb3Line = false;
        public string mCamID0 = "";
        public string mCamID1 = "";

        public string mTesterID = "";
        public string mMOTID = "";
        public string mDoingStatus = "IDLE";
        public int mIDLEcount = 0;
        public int mMaxThread = 10; // 13700K : 18, 12700 : 16
        public int mMonitoringTestSet = 129;
        public const double LensMag = 0.30;// CSH030Ex
        //public const int mMergeImgWidth = 750;
        //public const int mMergeImgHeight = 440;
        public const int mMergeImgWidth = 780;
        public const int mMergeImgHeight = 450;
        public GageCounter mGageCounter = null;

        public static Global GetInstance()
        {
            if (thisObject == null)
            {
                thisObject = new Global();
                return thisObject;
            }
            else
                return thisObject;

        }

        //-------------------------------------------------------------------------------------------------------------
        public void ExitInstance()
        {
            if (thisObject == null) return;

        }

        //-------------------------------------------------------------------------------------------------------------
        private Global()
        {
            m_RootDirectory = "C:\\CSHTest";
            //string src = "";
            string sLotDir = m_RootDirectory + "\\DoNotTouch";
            //string destine = "";
            //if (!Directory.Exists(sLotDir))
            //{
            //    Directory.CreateDirectory(sLotDir);

            //    src = "LastGap.txt";
            //    destine = sLotDir + "\\" + src;
            //    File.Copy(src, destine);

            //    src = "PreviousRecipe.txt";
            //    destine = sLotDir + "\\" + src;
            //    File.Copy(src, destine);

            //    src = "PreviousResult.txt";
            //    destine = sLotDir + "\\" + "PreviousSpec.txt";
            //    File.Copy(src, destine);

            //    src = "LastDayResults.txt";
            //    destine = sLotDir + "\\" + src;
            //    File.Copy(src, destine);

            //    //src = "PIDupdateFileName.txt";
            //    //destine = sLotDir + "\\" + src;
            //    //File.Copy(src, destine);

            //    src = "CBstates.txt";
            //    destine = sLotDir + "\\" + src;
            //    File.Copy(src, destine);
            //}

            sLotDir = m_RootDirectory + "\\Package";
            string sLotDirFW = m_RootDirectory + "\\Package" + "\\FW";
            string sLotDirPreFW = m_RootDirectory + "\\Package" + "\\PreFW";
            

            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);
            if (!Directory.Exists(sLotDirFW))
                Directory.CreateDirectory(sLotDirFW);
            if (!Directory.Exists(sLotDirPreFW))
                Directory.CreateDirectory(sLotDirPreFW);

           
            string DestFile = m_RootDirectory + "\\DriverIC\\FW";
           
            string[] lFWfile = new string[2] { "", "" };

            int nameIndex = sLotDirFW.Length;
            string[] lBinArray = Directory.GetFiles(sLotDirFW, "*.bin");
            for (int i = 0; i < lBinArray.Length; i++)
            {
               
                FileInfo fInfo = new FileInfo(lBinArray[i]);
                DestFile = m_RootDirectory + "\\DriverIC\\FW" + lBinArray[i].Substring(nameIndex);   //  32K FW                                                                                                     //MessageBox.Show(DestFile);
                string FilePath = m_RootDirectory + "\\DoNotTouch\\" + "FWupdateFileName.txt";

                StreamWriter sw = new StreamWriter(FilePath);
                sw.WriteLine(DestFile);     //  32K FW File Name
                sw.WriteLine(" "); //bu fw
                sw.WriteLine(" "); //bu cal
                sw.Close();
            }
            nameIndex = sLotDirPreFW.Length;
            lBinArray = Directory.GetFiles(sLotDirPreFW, "*.bin");
            for (int i = 0; i < lBinArray.Length; i++)
            {

                FileInfo fInfo = new FileInfo(lBinArray[i]);
                DestFile = m_RootDirectory + "\\DriverIC\\FW" + lBinArray[i].Substring(nameIndex);   //  32K FW                                                                                                     //MessageBox.Show(DestFile);
                string FilePath = m_RootDirectory + "\\DoNotTouch\\" + "Pre_FWupdateFileName.txt";
                StreamWriter sw = new StreamWriter(FilePath);
                sw.WriteLine(DestFile);     //  32K FW File Name               
                sw.Close();
            }
            nameIndex = sLotDir.Length;
            string[] lTxtArray = Directory.GetFiles(sLotDir, "*.spc");
            DateTime lrefTime = DateTime.Now;
            lrefTime = lrefTime.AddYears(-1);
            int latestFileIndex = -1;
            for (int i = 0; i < lTxtArray.Length; i++)
            {
                FileInfo fInfo = new FileInfo(lTxtArray[i]);
                //MessageBox.Show(lTxtArray[i] + " " + lrefTime.ToString("yyMMdd") + " " + fInfo.CreationTime.ToString("yyMMdd"));
                if (fInfo.CreationTime >= lrefTime)
                    latestFileIndex = i;

            }
            if (latestFileIndex >= 0)
            {
                DestFile = m_RootDirectory + "\\Spec" + lTxtArray[latestFileIndex].Substring(nameIndex);   //  32K FW
                //MessageBox.Show(DestFile);
                if (File.Exists(DestFile))
                    File.Delete(DestFile);

                File.Move(lTxtArray[latestFileIndex], DestFile);
                string FilePath = m_RootDirectory + "\\DoNotTouch\\" + "PreviousSpec.txt";
                StreamWriter writer = new StreamWriter(FilePath);
                writer.WriteLine(DestFile.Substring(DestFile.LastIndexOf("\\") + 1));
                writer.Close();
            }

            lTxtArray = Directory.GetFiles(sLotDir, "*.rcp");
            lrefTime = DateTime.Now;
            lrefTime = lrefTime.AddYears(-1);
            latestFileIndex = -1;
            for (int i = 0; i < lTxtArray.Length; i++)
            {
                FileInfo fInfo = new FileInfo(lTxtArray[i]);
                //MessageBox.Show(lTxtArray[i] + " " + lrefTime.ToString("yyMMdd") + " " + fInfo.CreationTime.ToString("yyMMdd"));
                if (fInfo.CreationTime >= lrefTime)
                    latestFileIndex = i;

            }
            if (latestFileIndex >= 0)
            {
                DestFile = m_RootDirectory + "\\Recipe" + lTxtArray[latestFileIndex].Substring(nameIndex);   //  32K FW
                //MessageBox.Show(DestFile);
                if (File.Exists(DestFile))
                    File.Delete(DestFile);

                File.Move(lTxtArray[latestFileIndex], DestFile);
                string FilePath = m_RootDirectory + "\\DoNotTouch\\" + "PreviousRecipe.txt";
                StreamWriter writer = new StreamWriter(FilePath);
                writer.WriteLine(DestFile.Substring(DestFile.LastIndexOf("\\") + 1));
                writer.Close();
            }

            lTxtArray = Directory.GetFiles(sLotDir, "*.txt");
            lrefTime = DateTime.Now;
            lrefTime = lrefTime.AddYears(-1);
            latestFileIndex = -1;
            for (int i = 0; i < lTxtArray.Length; i++)
            {
                DestFile = m_RootDirectory + "\\DoNotTouch" + lTxtArray[i].Substring(nameIndex);   //  32K FW
                if (File.Exists(DestFile))
                    File.Delete(DestFile);

                File.Move(lTxtArray[i], DestFile);
            }

            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            /////   다음 HEXAPOD 대체 시 삭제 필요
            mGageCounter = new GageCounter();
            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////

            if (thisObject == null)
            {
                return;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        ~Global()
        {
            if (!isDispose)
            {
                Dispose();
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            isDispose = true;
            GC.SuppressFinalize(this);
        }
        //---------------------------------------------------------------------------------------------------------------
        public void AddMessage(string msg)
        {
            strMsg += msg + "\r\n";
        }

        public void ResetwrSystemLog(int index = 0)
        {
            try
            {
                int lindex = index / 100;

                mSystemLogFile = m_RootDirectory + "\\Result\\" + "SystemLog_" + lindex.ToString() + ".txt";

                StreamWriter lwriter;
                lwriter = new StreamWriter(mSystemLogFile);
                lwriter.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
           
            
        }

        public void wrSytemLog(string msg)
        {
            return;
            //while (mbSystemLogging)
            //    Thread.Sleep(1);

            //mbSystemLogging = true;
            //StreamWriter lwriter = File.AppendText(mSystemLogFile);
            //lwriter.WriteLine(msg);
            //lwriter.Close();
            //mbSystemLogging = false;
        }

        public string GetPassword()
        {
            string PWfilename = m_RootDirectory + "\\RunData\\PWtext.txt";
            if (File.Exists(PWfilename))
            {
                StreamReader pwReader = new StreamReader(PWfilename);
                m_strPassword = pwReader.ReadLine();
                pwReader.Close();
            }
            else
                m_strPassword = "1234";

            return m_strPassword;
        }

        public void SetPassword(string str)
        {
            string PWfilename = m_RootDirectory + "\\RunData\\PWtext.txt";
            m_strPassword = str;
            StreamWriter pwWriter = new StreamWriter(PWfilename);
            pwWriter.WriteLine(m_strPassword);
            pwWriter.Close();
        }
        //-------------------------------------------------------------------------------------------------------------

        //===================================================================================================
        //===================================================================================================
        //===================================================================================================
        //===================================================================================================

        // User Sub Form 객체 생성 
        //public FCalibrate fCalibrate = new FCalibrate();
        //public FSaveFormat fSaveFormat = new FSaveFormat();
        public FGraph fGraph = new FGraph();
        public FVision fVision = new FVision();
        public FManage fManage = new FManage();
        //public FStatistics fStat = new FStatistics();
        public TestItemOnOff fTestItemOnOff = new TestItemOnOff();
        public FAutoLearn.FAutoLearn mFAL = null;
        // 모션
        //public F_Motion mMotion = null;
        //public F_PIMotion f_PIMotion = null;
        public F_Motion_SK_PI fMotion = null;

        //public F_SetupRegister fSetupReg = new F_SetupRegister();

        // Objects
        public MILlib[] oCam = new MILlib[2];

        // User Global 변수 선언
        public Parameter sRecipe;
        public TestSpec sSpec;
        //public double[,] sHistArrayCPK = new double[10004, 200];
        public double[,] sHistArray = new double[10000, 200];
        public int sHistIndex = 1;
        //public int sSaveResIndex = -1;
        public int sRepeatRunIndex = 0;
        public int[,] iFailCountPerItem = new int[4, 200];
        public int[] iFailCountPerItemAll = new int[200];
        public double[] sCPKs = new double[200];
        public SemcoI2Cprotocol[] sRegisterI2C = new SemcoI2Cprotocol[31];
        public int[] sCIndex = new int[4];
        public string[] m_StrBarcode = new string[4] { "", "", "", "" };
        public int mDailyTotalTested = 0;
        public int mDailyTotalFail = 0;



        public enum RcpItem
        {
            iI2Cclock,
            iI2CslaveAddr,
            iGrabTimeLimit,
            iMaxWaitAfterLastTrigger,
            iTriggeredGrabImageCount,
            iLEDcurrentLL,
            iLEDcurrentLR,

        };
        public enum ICDataID
        {
            AFZM_Linearity_Comp,
            AF1_Amp_Gain,
            AF2_Amp_Gain,
            AF1_Offset,
            AF2_Offset,
            AF1_Bias,
            AF2_Bias,
            AF1_MIN,
            AF1_MAX,
            AF2_MIN,
            AF2_MAX,
            AF1_MID,
            AF2_MID,
            AF1_RANGE,
            AF2_RANGE,
            AF_ATT3_MIN,
            AF_ATT3_MAX,
            AF_ATT3_range,
            AF_ATT2_MIN,
            AF_ATT2_MAX,
            AF_INVDR,
            AF_EPA_MIN,
            AF_EPA_MAX, 
            ZOOM1_Amp_Gain, //  23
            ZOOM2_Amp_Gain,
            ZOOM1_Offset,
            ZOOM2_Offset,
            ZOOM1_Bias,
            ZOOM2_Bias,
            ZOOM1_MIN,
            ZOOM1_MAX,
            ZOOM2_MIN,
            ZOOM2_MAX,
            ZOOM1_MID,
            ZOOM2_MID,
            ZOOM1_RANGE,
            ZOOM2_RANGE,
            ZOOM_ATT3_MIN,
            ZOOM_ATT3_MAX,
            ZOOM_ATT3_range,
            ZOOM_ATT2_MIN,
            ZOOM_ATT2_MAX,
            ZOOM_INVDR,
            ZOOM_EPA_MIN,
            ZOOM_EPA_MAX,
        };

        public enum SpecItem
        {
            AF_Fullstroke,         // min-max
            AF_Ratedstroke,
            AF_FwdStroke,      // Near Stroke
            AF_BwdStroke,     // Far Stroke
            AF_Resolution,     // Far Stroke
            AF_FullSensitivity,        // min-max
            AF_FullLinearity,          // max only
            AF_FullHysteresis,         // max only
            AF_FullCrosstalk,          // max only
            AF_FwdSensitivity,        // min-max
            AF_FwdLinearity,          // max only
            AF_FwdHysteresis,         // max only
            AF_FwdCrosstalk,          // max only
            AF_FwdResolution,     // Far Stroke
            AF_BwdSensitivity,        // min-max
            AF_BwdLinearity,          // max only
            AF_BwdHysteresis,         // max only
            AF_BwdCrosstalk,          // max only
            AF_BwdResolution,     // Far Stroke
            AF_MaxCodeCurrent,      // max only
            AF_2ndSettlingTime,
            AF_2ndOvershoot,
            AF_2ndStepStroke,
            AF_SettlingTime,
            AF_Overshoot,        
            AF_StepStroke,       
            AF_SettlingTime2,    
            AF_Overshoot2,       
            AF_StepStroke2,      
            AF_SettlingTime3,    
            AF_Overshoot3,       
            AF_StepStroke3,      
            AF_SettlingTime4,    
            AF_Overshoot4,       
            AF_StepStroke4,      
            AF_SettlingTime5,    
            AF_Overshoot5,       
            AF_StepStroke5,      
            AF_SettlingTime6,    
            AF_Overshoot6,       
            AF_StepStroke6,
            AF_SettlingTime7,
            AF_Overshoot7,
            AF_StepStroke7,
            AF_SettlingTime8,
            AF_Overshoot8,
            AF_StepStroke8,

            AF_TotalTilt,
            AF_YawTilt,
            AF_PitchTilt,
            AF_Start_Cut,
            AF_End_Cut,
            //AF_HallLinearity,
            //AF_HallCenter,
            //AF_HallSlope,

            ZM_Fullstroke,
            ZM_Ratedstroke,
            ZM_FwdStroke,
            ZM_BwdStroke,
            ZM_Resolution,     // Far Stroke
            ZM_FullSensitivity,
            ZM_FullLinearity,
            ZM_FullHysteresis,
            ZM_FullCrosstalk,
            ZM_FwdSensitivity,
            ZM_FwdLinearity,
            ZM_FwdHysteresis,
            ZM_FwdCrosstalk,
            ZM_FwdResolution,     // Far Stroke
            ZM_BwdSensitivity,
            ZM_BwdLinearity,
            ZM_BwdHysteresis,
            ZM_BwdCrosstalk,
            ZM_BwdResolution,     // Far Stroke
            ZM_MaxCodeCurrent,
            ZM_2ndSettlingTime,
            ZM_2ndOvershoot,
            ZM_2ndStepStroke,
            ZM_SettlingTime,
            ZM_Overshoot,
            ZM_StepStroke,
            ZM_SettlingTime2,
            ZM_Overshoot2,
            ZM_StepStroke2,
            ZM_SettlingTime3,
            ZM_Overshoot3,
            ZM_StepStroke3,
            ZM_SettlingTime4,
            ZM_Overshoot4,
            ZM_StepStroke4,
            ZM_SettlingTime5,
            ZM_Overshoot5,
            ZM_StepStroke5,
            ZM_SettlingTime6,
            ZM_Overshoot6,
            ZM_StepStroke6,
            ZM_SettlingTime7,
            ZM_Overshoot7,
            ZM_StepStroke7,
            ZM_SettlingTime8,
            ZM_Overshoot8,
            ZM_StepStroke8,

            ZM_TotalTilt,
            ZM_YawTilt,
            ZM_PitchTilt,
            ZM_Start_Cut,
            ZM_End_Cut,
            //ZM_HallLinearity,
            //ZM_HallCenter,
            //ZM_HallSlope,

            FRAAF_PMFreq,
            FRAAF_PhaseMargin,
            FRAAF_PMFreq2nd,
            FRAAF_PhaseMargin2nd,
            FRAAF_Gain10Hz,
            FRAZM_PMFreq,
            FRAZM_PhaseMargin,
            FRAZM_PMFreq2nd,
            FRAZM_PhaseMargin2nd,
            FRAZM_Gain10Hz,
            /// <summary>
            /// 다음부터는 순수하게 결과 출력 용
            /// </summary>
            PassFail,
            FailItem1st,
            AFHALL_Offset,
            AFHALL_Bias,
            AFHALL_min,
            AFHALL_max,
            AFHALL_mid,

            AFHALLCut_H_min,
            AFHALLCut_H_max,
            AFHALLCut_H_mid,
            //XEPA_Gain,
            //XEPA_Offset,

            AFDrv_Min,
            AFDrv_Max,
            AFMeasure_Min,
            AFMeasure_Max,
            ZMHALL_Offset,
            ZMHALL_Bias,
            ZMHALL_min,
            ZMHALL_max,
            ZMHALL_mid,

            ZMHALLCut_Hmin,
            ZMHALLCut_Hmax,
            ZMHALLCut_Hmid,
            //YEPA_Gain,
            //YEPA_Offset,

            ZMDrv_Min,
            ZMDrv_Max,
            ZMMeasure_Min,
            ZMMeasure_Max,
            Time_FWwite,
            Time_HallCal,
            Time_HallNVMRead,
            Time_PIDUpdate,
            Time_Xsweep,
            Time_Xstep,
            Time_Ysweep,
            Time_Ystep,
            Time_XYPhasemargin,
            Time_Total              //  98th Item
        };
        //        [StructLayout(LayoutKind.Explicit)]
        public struct TestSpec
        {
            //  index 0 : test result       index 1 : spec min      index 2 : spec max
            public double[] AF_Fullstroke;                        //  0 ~ 4       -> Y1
            public double[] AF_Ratedstroke;                        //  0 ~ 4       -> Y1
            public double[] AF_FwdStroke;                     //  0 ~ 4       -> Y1
            public double[] AF_BwdStroke;                    //  0 ~ 4       -> Y1
            public double[] AF_Resolution;                    //  0 ~ 4       -> Y1
            public double[] AF_FullSensitivity;                       //  0 ~ 1       -> Y1
            public double[] AF_FullLinearity;                         //  0 ~ 0.2     -> Y1
            public double[] AF_FullHysteresis;                        //  0 ~ 0.2     -> Y1
            public double[] AF_FullCrosstalk;                         //  0 ~ 1       -> Y1
            public double[] AF_FwdSensitivity;                       //  0 ~ 1       -> Y1
            public double[] AF_FwdLinearity;                         //  0 ~ 0.2     -> Y1
            public double[] AF_FwdHysteresis;                        //  0 ~ 0.2     -> Y1
            public double[] AF_FwdCrosstalk;                         //  0 ~ 1       -> Y1
            public double[] AF_FwdResolution;                    //  0 ~ 4       -> Y1
            public double[] AF_BwdSensitivity;                       //  0 ~ 1       -> Y1
            public double[] AF_BwdLinearity;                         //  0 ~ 0.2     -> Y1
            public double[] AF_BwdHysteresis;                        //  0 ~ 0.2     -> Y1
            public double[] AF_BwdCrosstalk;                         //  0 ~ 1       -> Y1
            public double[] AF_BwdResolution;                    //  0 ~ 4       -> Y1
            public double[] AF_MaxCodeCurrent;                     //  0 ~ 80      -> Y2
            //  OISX_Tilt                                                                    
            public double[] AF_2ndSettlingTime;                      //  0 ~ 50      -> Y2
            public double[] AF_2ndOvershoot;                         //  0 ~ 100     -> Y2
            public double[] AF_2ndStepStroke;                         //  0 ~ 100     -> Y2
            public double[] AF_SettlingTime;                      //  0 ~ 50      -> Y2
            public double[] AF_Overshoot;                         //  0 ~ 100     -> Y2
            public double[] AF_StepStroke;                         //  0 ~ 100     -> Y2
            public double[] AF_TotalTilt;                    //  0 ~ 2
            public double[] AF_YawTilt;                    //  0 ~ 2
            public double[] AF_PitchTilt;                    //  0 ~ 2
            public double[] AF_Start_Cut;                    //  0 ~ 2
            public double[] AF_End_Cut;                    //  0 ~ 2
            //public double[] AF_HallLinearity;                     //  0 ~ 40
            //public double[] AF_HallCenter;                        //  0 ~ 3000
            //public double[] AF_HallSlope;                         //  0 ~ 4

            public double[] ZM_Fullstroke;
            public double[] ZM_Ratedstroke;
            public double[] ZM_FwdStroke;
            public double[] ZM_BwdStroke;
            public double[] ZM_Resolution;                    //  0 ~ 4       -> Y1
            public double[] ZM_FullSensitivity;
            public double[] ZM_FullLinearity;
            public double[] ZM_FullHysteresis;
            public double[] ZM_FullCrosstalk;
            public double[] ZM_FwdSensitivity;
            public double[] ZM_FwdLinearity;
            public double[] ZM_FwdHysteresis;
            public double[] ZM_FwdCrosstalk;
            public double[] ZM_FwdResolution;                    //  0 ~ 4       -> Y1
            public double[] ZM_BwdSensitivity;
            public double[] ZM_BwdLinearity;
            public double[] ZM_BwdHysteresis;
            public double[] ZM_BwdCrosstalk;
            public double[] ZM_BwdResolution;                    //  0 ~ 4       -> Y1
            public double[] ZM_MaxCodeCurrent;

            public double[] ZM_2ndSettlingTime;                      //  0 ~ 50      -> Y2
            public double[] ZM_2ndOvershoot;                         //  0 ~ 100     -> Y2
            public double[] ZM_2ndStepStroke;                         //  0 ~ 100     -> Y2
            public double[] ZM_SettlingTime;
            public double[] ZM_Overshoot;
            public double[] ZM_StepStroke;                         //  0 ~ 100     -> Y2
            public double[] ZM_TotalTilt;
            public double[] ZM_YawTilt;
            public double[] ZM_PitchTilt;
            public double[] ZM_Start_Cut;                    //  0 ~ 2
            public double[] ZM_End_Cut;                    //  0 ~ 2
            //public double[] ZM_HallLinearity;
            //public double[] ZM_HallCenter;
            //public double[] ZM_HallSlope;

            public double[] FRAAF_PMFreq;                           //  30 ~ 200    -> Y2
            public double[] FRAAF_PhaseMargin;                      //  0 ~ 180     -> Y2
            public double[] FRAAF_PMFreq2nd;                           //  30 ~ 200    -> Y2
            public double[] FRAAF_PhaseMargin2nd;                      //  0 ~ 180     -> Y2
            public double[] FRAAF_Gain10Hz;                      //  ??          -> Y2
            public double[] FRAZM_PMFreq;
            public double[] FRAZM_PhaseMargin;
            public double[] FRAZM_PMFreq2nd;
            public double[] FRAZM_PhaseMargin2nd;
            public double[] FRAZM_Gain10Hz;
        };

        public int sNUM_TESTITEM = 0;
        public string[,] mTestItem = new string[114, 11] {   //  Axis    Item            Min     Max     P0      P1      P2      P3      cpk     unit    OnOff //
                                                            {  "AF"   ,   "Full stroke"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Rated stroke"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd stroke"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd stroke"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Full Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Fwd Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Bwd Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Max Current"   ,"0"    ,"120"  ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"mA"   ,"true"},
                                                            {  ""   ,    "2nd SettlingTime" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "2nd Overshoot"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "2nd Step Stroke"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime2" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot2"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke2"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime3" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot3"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke3"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime4" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot4"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke4"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime5" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot5"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke5"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime6" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot6"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke6"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime7" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot7"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke7"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime8" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot8"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke8"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Total Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Yaw Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Pitch Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Start_Cut" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "End_Cut" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  "Zoom"   ,   "Full stroke"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Rated stroke"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd stroke"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd stroke"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Full Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Full Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Fwd Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Fwd Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Sensitivity"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um/code"  ,"true"},
                                                            {  ""   ,    "Bwd Linearity"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Hysteresis"   ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Crosstalk"        ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Bwd Resolution"  ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "Max Current"   ,"0"    ,"120"  ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"mA"   ,"true"},
                                                            {  ""   ,    "2nd SettlingTime" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "2nd Overshoot"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "2nd Step Stroke"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime2" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot2"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke2"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime3" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot3"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke3"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime4" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot4"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke4"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime5" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot5"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke5"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime6" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot6"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke6"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime7" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot7"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke7"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "SettlingTime8" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"msec" ,"true"},
                                                            {  ""   ,    "Overshoot8"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Step Stroke8"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"%"    ,"true"},
                                                            {  ""   ,    "Total Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Yaw Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Pitch Tilt" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"min"  ,"true"},
                                                            {  ""   ,    "Start_Cut" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  ""   ,    "End_Cut" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"um"  ,"true"},
                                                            {  "FRA AF",  "PM Frequency" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"Hz"   ,"true"},
                                                            {  ""   ,    "Phase margin" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "2nd PM Frequency" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "2nd Phase margin" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "Gain @ 10Hz"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"db"   ,"true"},
                                                            {  "FRA Zoom",  "PM Frequency" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"Hz"   ,"true"},
                                                            {  ""   ,    "Phase margin" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "2nd PM Frequency" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "2nd Phase margin" ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"deg"  ,"true"},
                                                            {  ""   ,    "Gain @ 10Hz"    ,"-1"   ,"1"    ,"0"    ,"0"    ,"0"    ,"0"    ,"0"    ,"db"   ,"true"},
                                                        };

        public string[,] mStsData = new string[119, 7] {   //  Axis    Item            Yield     F Ratio     Total   Gcount OnOff //			
                                                            {  "General"   ,  "Avg Yield"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,   "P1 Yield"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "P2 Yield"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "P3 Yield"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "P4 Yield"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  "AF"   ,   "Full stroke"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Rated stroke"       ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Fwd stroke"       ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Bwd stroke"      ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Resolution"      ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Full Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Resolution"  ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Resolution"  ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Max Current"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd SettlingTime"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Overshoot"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Step Stroke"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime2"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot2"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke2"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime3"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot3"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke3"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime4"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot4"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke4"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime5"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot5"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke5"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime6"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot6"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke6"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime7"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot7"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke7"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime8"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot8"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke8"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Total Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Yaw Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Pitch Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Start_Cut"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "End_Cut"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  "Zoom"   ,   "Full stroke"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Rated stroke"       ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Fwd stroke"       ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Bwd stroke"      ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Resolution"      ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Full Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Full Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Fwd Resolution"  ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Sensitivity"      ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Linearity"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Hysteresis"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Crosstalk"            ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Bwd Resolution"  ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Max Current"       ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd SettlingTime"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Overshoot"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Step Stroke"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime2"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot2"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke2"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime3"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot3"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke3"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime4"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot4"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke4"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime5"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot5"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke5"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime6"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot6"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke6"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime7"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot7"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke7"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "SettlingTime8"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Overshoot8"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Step Stroke8"        ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Total Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Yaw Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Pitch Tilt"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "Start_Cut"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  ""   ,    "End_Cut"     ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  "FRA AF",  "PM Frequency"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Phase margin"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd PM Frequency"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Phase margin"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Gain @ 10Hz"        ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                            {  "FRA Zoom",  "PM Frequency"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Phase margin"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd PM Frequency"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "2nd Phase margin"     ,"0"        ,"0"        ,"0"        ,"0"        ,"T"},
                                                            {  ""   ,    "Gain @ 10Hz"        ,"0"        ,"0"        ,"0"        ,"0"        ,"F"},
                                                        };

        public struct Parameter
        {
            public string sRecipeName;

            public int iI2Cclock;
            public int iI2CslaveAddr;
            public int iGrabTimeLimit;
            public int iMaxWaitAfterLastTrigger;
            public int iTriggeredGrabImageCount;
            public int iRawGain;
            public double iGamma;
            public int iExposure;
            public int iEdgeBand;
            public double iLEDcurrentLL;
            public double iLEDcurrentLR;
            //public double iLEDcurrentRL;
            //public double iLEDcurrentRR;

        };
        public int sNUM_TESTCONDITION = 11;
        public string[,] mTestCondition = new string[11, 5] {  
                                                                    { "I2C" ,    "I2C Clock"    ,"400","KHz","true"},
                                                                    { ""    ,    "Slave Addr"   ,"0","Hex","true"},
                                                                    { "Grab Control"    ,    "Grab Time Limit"    ,"10","sec","true"},
                                                                    { ""    ,    "Max Wait after Last Trigger"    ,"1000","msec","true"},
                                                                    { ""    ,    "Triggered Grab Image Count"    ,"10000","#","true"},
                                                                    { ""    ,    "Raw Gain"    ,"35","30~512","true"},
                                                                    { ""    ,    "Gamma"    ,"0.85","0.1~3.99","true"},
                                                                    { ""    ,    "Exposure"    ,"74","usec","true"},
                                                                    { "ImageProcessing"    ,    "Edge Band"    ,"7","5,7,9,11","true"},
                                                                    { "Illumination"    ,    "LED Power Left"  ,"0","V","true"},
                                                                    { ""    ,    "LED Power Right"  ,"0","V","true"},
                                                                    };

        public struct SemcoI2Cprotocol
        {
            public Int16 addr;
            public Int16 dataLen;
            public byte data1;
            public byte data2;
            public byte data3;
            public byte data4;
        }

        public struct Calibrate
        {
            public int iCount;
            public double[] dPlace;
            public double[] dMeasX;
            public double[] dMeasY;
            public double[] dSharp;

            public double[,] I2R_AF;
            public double[,] I2R_OIS;
            public double[,] I2R;
            //double[,] I2R_AF = new double[4, 8] { { 0, }, };
            //double[,] I2R_OIS   = new double[4, 8] { { 0, }, };
        }

        public int[] mTestItemToGrid = new int[114];
        public int[] mGridToTestItem = new int[114];
        public int[] mTestConditionToGrid = new int[200];
        public int[] mGridToTestCondition = new int[200];
        public string[] mFirstFailItemText = new string[4] { "", "", "", "" };
        public string strPath;
        public string strSpecFile;

        public double dPatternCenterX1;
        public double dPatternCenterY1;
        public double dPatternCenterX2;
        public double dPatternCenterY2;
        public double dPatternCenterX3;
        public double dPatternCenterY3;

        public double dCam1_PatternCntX1;
        public double dCam1_PatternCntY1;
        public double dCam1_PatternCntX2;
        public double dCam1_PatternCntY2;
        public double dCam1_PatternCntX3;
        public double dCam1_PatternCntY3;

        public double dCam2_PatternCntX1;
        public double dCam2_PatternCntY1;
        public double dCam2_PatternCntX2;
        public double dCam2_PatternCntY2;
        public double dCam2_PatternCntX3;
        public double dCam2_PatternCntY3;

        public double dUnit_AF_V = 0;
        public double dUnit_STEPRESPONSE_V = 0;
        public double dUnit_OISX_V = 0;
        public double dUnit_OISY_V = 0;
        public double dUnit_FRAAF_V = 0;
        public double dUnit_FRAZM_V = 0;

        //public int      nAF_DataCount       = 0;
        //public int      nStepResponse_DataCount    = 0;
        public int nCLXYROI_DataCount = 0;
        public int nDataCount_All = 0;
        public int nDataCount_AF = 0;
        public int nDataCount_ZM = 0;
        public int nCLXYCAL_DataCount = 0;
        public int nCLXCAL_DataCount = 0;
        public int nCLYCAL_DataCount = 0;
        public int nCLXYStep_DataCount = 0;
        public int nStepDataCount_AF = 0;
        public int nStepDataCount_ZM = 0;
        //public int      nOISX_DataCount  = 0;
        //public int      nOISY_DataCount  = 0;

        //public int dAFZM_FrameCount = 0;
        //public int dAF_FrameCount = 0;
        //public int dZoom_FrameCount = 0;
        //public int      dAF_FrameCount = 0;
        //public int      dZM_FrameCount = 0;

        public int[] mXCrossOffset = new int[4];
        public int[] mYCrossOffset = new int[4];
        public int[] Cal_xy = new int[4];
        public int[] Cal_yy = new int[4];

        //public bool     CamChannelInverted = false;   //  For Dual Camera System
        public bool m_InvI2Cchannel = false;

        //-------------------------------------------------------------------------------------------------------------
        //public int      AF_Count    = 120000;
        //public int      AFRes_Count =   4000;
        //public int      AF_Count  = 160000;
        //public int      ZM_Count  = 160000;
        //public int FRAAF_Count = 131072;
        //public int FRAZM_Count = 131072;

        public double[] FPS = new double[2] { 1000, 1000 };   //  Default Value
        //  Camera 1
        public long TimerFrequency = 0;
        public bool m_CLAFTesting = true;
        public bool m_bPasswordOn = false;
        public int m_CLAFPeakTimeIndex = 0;
        public int m_AFPeakTimeIndex = 0;
        public int m_L3PeakTimeIndex = 0;
        public string m_RootDirectory = "C:\\CSHTest";
        public string m_SaveDirectory = "D:\\CSHTest";
        public string[] m_FWVersion = new string[4];

        //  20170703    Camera Default ROI Definition
        //public int mHROI = 740; // hor-ROI
        public int mHROI = 1800; // hor-ROI

        public int[] mVROI = new int[2] { 342, 342 };
        //public int[] mVROI = new int[2] { 380, 380 };
        public int mVROIstep = 150;

        //public double[,] mZeroXgap = new double[2, 2] { { 190.1, 190.1 }, { 190.1, 190.1 } };
        //public double[,] mZeroYgap = new double[2, 2] { { 0, 0 }, { 0, 0 } };

        //public bool m_bDriveAlongHall = false;
        public bool m_bSkipWriteHallPolarity = false;
        public bool m_bSkipErr0x0004 = false;
        public bool m_bWriteResultToDriverIC = false;
        public bool m_bUpdateHallCalData = false;
        public bool m_bScreenCapture = false;
        public bool m_bSaveRawData = true;
        public bool m_bSafeSensor = false;
        public bool m_bXTiltReverse = false;
        public bool m_bYTiltReverse = false;
        public bool m_bBarcodeOverwriteDisable = false;
        public bool m_bCalibrationModel = false;
        public bool m_bHideAllGraph = false;
        public bool m_bPhaseInterpolation = true;
        public bool m_bDFTphasemargin = true;
        public bool m_bTXDirReverse = false;
        public bool m_bTYDirReverse = false;
        public bool m_bXDirReverse = false;
        public bool m_bYDirReverse = false;
        public bool m_bEulerRotation = false;
        public bool m_bPrismCS = false;
        public bool m_bUserZeroSet = false;
        public bool m_bDebugMode = false;
        public bool m_bSaveImage = false;
        public int m_SaveImageCount = 0;

        public bool m_bSwap = false;

        public bool[] m_ChannelOn = new bool[4] {true, true, true, true};
        public bool[] m_ChannelEff = new bool[4] { false, false, false, false };
        public string sModelName = "";

        //public const double LensMag = 0.35;
        //public EPA_SETTING_T[] mEPAsetting = new EPA_SETTING_T[2];
        //-------------------------------------------------------------------------------------------------------------
        //-------------------------------  비전  ----------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------
        public string mDcfFilePathC = "";
        public string mDcfFilePathT = "";
        public bool Initial_Vision(int grabberType, bool IsSwap = false)
        {
            oCam[0] = new MILlib(1.0);

            if (mCamCount > 1)
                oCam[1] = new MILlib(1.0);

            strPath = m_RootDirectory + "\\RunData\\";
            //String[] strFileName = new string[2] { "", "" };
            int lHroi = (mHROI / 10) * 10;
            mDcfFilePathC = strPath + "Continuous_10tap_" + mVROI[0].ToString() + "_" + lHroi.ToString() + "R.dcf";
            mDcfFilePathT = strPath + "ExtTrg_10tap_" + mVROI[0].ToString() + "_" + lHroi.ToString() + "R.dcf";

            m_bSwap = IsSwap;

            //int i = 0;

            string lSystemName = "M_SYSTEM_SOLIOS";
            if (grabberType == 2)
                lSystemName = "M_SYSTEM_RADIENTEVCL";
            
            bool res = true;
            if (!IsSwap)
            {
                res = oCam[0].Init(mVROI[0], mHROI, mVROIstep, 0, lSystemName, 0, 0, mDcfFilePathT);
            }
            else
            {
                res = oCam[0].Init(mVROI[0], mHROI, mVROIstep, 0, lSystemName, 1, 0, mDcfFilePathT);
            }
            mFAL = oCam[0].mFAL;
            return res;
        }

        public void SetTesterID(string strCamID)
        {
            mCamID0 = strCamID;
            string macAddress = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();

            mTesterID = macAddress + "_" + mCamID0;

        }
        //-------------------------------------------------------------------------------------------------------------
        public void CloseVision()
        {
            if(oCam[0] != null)
            {
                if (oCam[0].IsInit == true)
                    oCam[0].Free();
                if (mCamCount > 1)
                {
                    if (oCam[1].IsInit == true)
                        oCam[1].Free();
                }
            }
        }

        public struct sPoint
        {
            public double x;
            public double y;
        };

        public struct sLine
        {
            public double dSlope;
            public double dYintercept;
        };

        public sLine Line_fitting(sPoint[] data, int dataSize)
        {
            sLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < dataSize; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / dataSize;
            AVGx = SUMx / dataSize;

            //slope or a1
            slope = (dataSize * SUMxy - SUMx * SUMy) / (dataSize * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }
    }

    public class GageCounter
    {
        public Global m__G = null;

        List<SerialPort> PortList = new List<SerialPort>();

        List<string> strMsg = new List<string>();

        const double TXDist = 52.9; //mm
        const double TYDist = 86.5; //mm

        public GageCounter()
        {
            int index = 0;
            for (int i = 0; i < 7; i++)
            {
                PortList.Add(new SerialPort());
                strMsg.Add("");
            }

            PortList[index].PortName = "COM4"; index++;    //       //   -> center of stage                      "COM15";  Probe X
            PortList[index].PortName = "COM8"; index++;         //X2    -> 40mm off from center of stage       "COM11";  Porb Y
            PortList[index].PortName = "COM11"; index++;        //Y1    -> center of stage                      "COM7";  Porb TZ1
            PortList[index].PortName = "COM6"; index++;        //Y2    -> 40mm off from center of stage       "COM10";  Porb TZ2
            PortList[index].PortName = "COM5"; index++;        //TX    -> 55mm off from center of stage        "COM5";  Porb TX
            PortList[index].PortName = "COM7"; index++;        //TY1   -> 55mm off from center of stage        "COM6";  Porb TY1
            PortList[index].PortName = "COM9";                 //TY2   -> 55mm off from center of stage        "COM14"; Porb TY2

            for (int i = 0; i< PortList.Count;i++)
            {
                //if(i == 3)
                //    PortList[i].BaudRate = 9600;
                //else
                //    PortList[i].BaudRate = 19200;

                PortList[i].BaudRate = 19200;
                PortList[i].Parity = Parity.Even;
                PortList[i].DataBits = 7;
                PortList[i].StopBits = StopBits.Two;
                PortList[i].Handshake = Handshake.None;
                PortList[i].ReadTimeout = 1000;
                PortList[i].WriteTimeout = 1000;
            }
        }

        // 디버깅 초기 Probe 값
        //public double[] probe_raw_values = new double[7];  // X, Y, TZ1, TZ2 , TX, TY1, TY2
        //public double[] probe_glass_compensation_value = new double[7];

        //* 250404 Probe Z(TY1, TY2) 디버깅 *//
        // [0. 첫 Probe 값] , [1. X,Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정], [2. Glass 두께에 따른 Probe보정]
        //public double[] probeTY1 = new double[3];   
        //public double[] probeTY2 = new double[3];
        //public double[] probeTX = new double[3];

        // Probe TZ 튀는 현상 디버깅
        //public double[] probeTZ = new double[2];
        //***********************************//
        public double[] ReadPortAll()
        {
            int signX = 1;
            int signY = -1;
            int signTZ1 = -1;
            int signTZ2 = -1;
            int signTX = -1;
            int signTY1 = -1;
            int signTY2 = -1;

            //if (m__G.m_bTXDirReverse) signTX = -1;
            //if (m__G.m_bTYDirReverse) signTY = -1;
            //if (m__G.m_bXDirReverse) signX = -1;
            //if (m__G.m_bYDirReverse) signY = -1;

            double[] res = new double[7];
            string resStr;
            if(PortList.Count > 0)
            {
                resStr = WriteNReadPort("GA01\r\n", 0);    // Porb X
                if (resStr.Length > 1)
                    res[0] = signX * StrToUm(resStr);
                else
                    res[0] = -9990;
            }

            if (PortList.Count > 1)
            {
                resStr = WriteNReadPort("GA01\r\n", 1);    // Porb Y
                if (resStr.Length > 1)
                    res[1] = signY * StrToUm(resStr);
                else
                    res[1] = -9990;
            }
            if (PortList.Count > 2)
            {
                resStr = WriteNReadPort("GA01\r\n", 2);    // Porb TZ1
                if (resStr.Length > 1)
                    res[2] = signTZ1 * StrToUm(resStr);
                else
                    res[2] = -9990;
            }
            if (PortList.Count > 3)
            {
                resStr = WriteNReadPort("GA01\r\n", 3);    // Porb TZ2
                if (resStr.Length > 1)
                    res[3] = signTZ2 * StrToUm(resStr);
                else
                    res[3] = -9990;
            }
            if (PortList.Count > 4)
            {
                resStr = WriteNReadPort("GA01\r\n", 4);    // Porb TX
                if (resStr.Length > 1)
                    res[4] = signTX * StrToUm(resStr);
                else
                    res[4] = -9990;
            }
            if (PortList.Count > 5)
            {
                resStr = WriteNReadPort("GA01\r\n", 5);    // Porb TY1
                if (resStr.Length > 1)
                    res[5] = signTY1 * StrToUm(resStr);
                else
                    res[5] = -9990;
            }
            if (PortList.Count > 6)
            {
                resStr = WriteNReadPort("GA01\r\n", 6);    // Porb TY2
                if (resStr.Length > 1)
                    res[6] = signTY2 * StrToUm(resStr);
                else
                    res[6] = -9990;
            }

            //probe_raw_values[0] = res[0];
            //probe_raw_values[1] = res[1];
            //probe_raw_values[2] = res[2];
            //probe_raw_values[3] = res[3];
            //probe_raw_values[4] = res[4];
            //probe_raw_values[5] = res[5];
            //probe_raw_values[6] = res[6];

            //probeTZ[0] = res[2];
            //probeTZ[1] = res[3];

            //string filename = m__G.m_RootDirectory + "\\DoNotTouch\\Admin\\ProbeTZ.csv";
            //if (!File.Exists(filename))
            //{
            //    File.AppendAllText(filename, $"TZ1, TZ2\r\n");
            //}
            //File.AppendAllText(filename, $"{probeTZ[0]},{probeTZ[1]}\r\n");

            //* 250404 Probe Z(TY1, TY2) 디버깅 *//
            // 0. 첫 Probe 값
            //probeTX[0] = res[4];
            //probeTY1[0] = res[5];   
            //probeTY2[0] = res[6];
            //***********************************//



            // 251223 마크 Plate 교체
            double XtoTX = -0.000373422; // -0.001953968;
            double XtoTY1 = -0.000772438;// - 0.001961178;
            double XtoTY2 = 0.000029633;//-0.001791774;
            double YtoTX = 0.003253190; //0.002494516;
            double YtoTY1 = 0.000138473; // 0.003180553;
            double YtoTY2 = 0.000669175; // 0.002568352;

            double ltz = Math.Atan((res[3] - res[2]) / 45000);   // rad

            //res[4] = res[4] - XtoTX * (res[0] + ltz*83000 ) - YtoTX * res[1];  //  TX
            //res[5] = res[5] - XtoTY1 * res[0] - YtoTY1 * (res[1] + ltz*60000 );  //  TY1

                  //  TY2 기준으로 TX, TY1기울기 보정한 경우 다음 식 사용 -> TZ cal 에서 Z 오차 2.35um
            //res[4] = res[4] - XtoTX * (res[0] - Math.Sin(ltz) * 83000) - YtoTX * (res[1] - Math.Sin(ltz) * 60000);  //  TX relative to TY2 -> TX cal 에 영향, 식변경 시 Cal 다시할 것
            //res[5] = res[5] - XtoTY1 * res[0] - YtoTY1 * (res[1] - Math.Sin(ltz) * 120000);  //  TY1 relative to TY2 따라서 120000 이 맞음 -> TY Cal 에 영향, 식변경 시 Cal 다시할 것

                  // 절대 0 기준으로 TX, TY1, TY2 기울기 보정한 경우 다음 식 사용 >> TZ cal 에서 Z 오차 더 적어야 함.
            res[4] = res[4] - XtoTX * (res[0] + Math.Sin(ltz) * 83000) - YtoTX * res[1];  // (-) TX relative to TY2 -> TX cal 에 영향, 식변경 시 Cal 다시할 것  
            res[5] = res[5] - XtoTY1 * (res[0] + (1-Math.Cos(ltz)) *60000) - YtoTY1 * (res[1] + Math.Sin(ltz) * 60000);  // (+,-) TY1 relative to TY2 따라서 120000 이 맞음 -> TY Cal 에 영향, 식변경 시 Cal 다시할 것 2.5
            res[6] = res[6] - XtoTY2 * (res[0] - (1-Math.Cos(ltz)) *60000) - YtoTY2 * (res[1] - Math.Sin(ltz) * 60000);  //  (-,+) TY1 relative to TY2 따라서 120000 이 맞음 -> TY Cal 에 영향, 식변경 시 Cal 다시할 것 2.5

            //probe_glass_compensation_value[0] = res[0];
            //probe_glass_compensation_value[1] = res[1];
            //probe_glass_compensation_value[2] = res[2];
            //probe_glass_compensation_value[3] = res[3];
            //probe_glass_compensation_value[4] = res[4];
            //probe_glass_compensation_value[5] = res[5];
            //probe_glass_compensation_value[6] = res[6];

            //* 250404 Probe Z(TY1, TY2) 디버깅 *//
            // 1.X,Y이동, Z회전에 따른 Glass 기울기에 대한 Probe 보정
            //probeTX[1] = res[4];
            //probeTY1[1] = res[5];  
            //probeTY2[1] = res[6];
            //***********************************//

            //  Glass Offset 적용
            double ltx = Math.Atan((res[4] - (res[5] + res[6]) / 2) / 83000);    //  rad
            double lty = Math.Atan((res[5] - res[6]) / 120000);   // rad
            double ldt = Math.Sqrt(ltx * ltx + lty * lty);
            double ldComp = -670 * (1 / Math.Cos(ldt) - 1);
            res[4] += ldComp;
            res[5] += ldComp;
            res[6] += ldComp;

                  //* 250404 Probe Z(TY1, TY2) 디버깅 *//
                  // 2.Glass 두께에 따른 Probe보정
            //probeTX[2] = res[4];
            //probeTY1[2] = res[5];
            //probeTY2[2] = res[6];
            //***********************************//

            return res;
        }

        public bool OpenAllport()
        {
            bool res = true;
            for (int i = 0; i < PortList.Count; i++)
            {
                try
                {
                    if (PortList[i] != null && !PortList[i].IsOpen)
                        PortList[i].Open();
                }
                catch (Exception ex)
                {
                    res = false;
                    //                        PortList[i] = null;
                    strMsg[i] = $"Fail to open Port {PortList[i].PortName}";
                }
            }
            return res;
        }
        public bool CloseAllport()
        {
            bool res = true;
            for (int i = 0; i < PortList.Count; i++)
            {
                try
                {
                    if (PortList[i] != null)
                        PortList[i].Close();
                }
                catch (Exception ex)
                {
                    res = false;
                    PortList[i] = null;
                    strMsg[i] = "Fail to open Port 1";
                }
            }
            return res;
        }
        public bool DisposAllport()
        {
            bool res = true;
            for (int i = 0; i < PortList.Count; i++)
            {
                try
                {
                    if (PortList[i] != null)
                        PortList[i].Dispose();
                }
                catch (Exception ex)
                {
                    res = false;
                    PortList[i] = null;
                    strMsg[i] = "Fail to open Port 1";
                }
            }
            return res;
        }
        public void SetAllPortZero()
        {
            string cmdReadPosition = "\r01\r\n";
            char[] data = cmdReadPosition.ToCharArray();
            string resStr = "";
            for (int i = 0; i < PortList.Count; i++)
            {
                resStr = WriteNReadPort("CR01\r\n", i);
                //PortList[i].Write(data, 0, data.Length);
                //PortList[i].BaseStream.Flush();
                //PortList[i].DiscardInBuffer();
            }
        }
        string WriteNReadPort(string lstr, int port)
        {
            //  probe Z1 
            if (PortList[port] == null)
                return "";


            List<byte> allBytes = new List<byte>();

            string cmdReadPosition = lstr;
            char[] data = cmdReadPosition.ToCharArray();

            PortList[port].Write(data, 0, data.Length);
            PortList[port].BaseStream.Flush();
            PortList[port].DiscardInBuffer();
            Thread.Sleep(40);

            try
            {

                byte[] buffer = new byte[1024];
                int bytesRead = PortList[port].Read(buffer, 0, buffer.Length);
                string resStr = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                //_serialPort1.Close();
                //strMsg[0] = "Reply from EH-101P :\t" + resStr + "\r\n";
                return resStr;
            }
            catch (Exception e)
            {
                strMsg[port] = "RS-232 Read Timeout\r\n";
                //_serialPort1.Close();
            }
            return "";
        }
        public double StrToUm(string lstr)
        {
            double res = 0;

            int signIndex = 0;
            if (lstr.Contains("+"))
            {
                signIndex = lstr.IndexOf("+");
                string figStr = lstr.Substring(signIndex + 1);
                res = double.Parse(figStr) * 1000;
            }
            else if (lstr.Contains("-"))
            {
                signIndex = lstr.IndexOf("-");
                string figStr = lstr.Substring(signIndex + 1);
                res = -double.Parse(figStr) * 1000;
            }
            else
                res = -9999;
            return res;
        }
    }

    public partial class AFDrvIC
    {
        private System.IO.Ports.SerialPort[] mSR700 = new System.IO.Ports.SerialPort[4];

        public Device[] DLNdevice = new Device[4];
        public Dln.I2cMaster.Port[] DLNi2c = new Dln.I2cMaster.Port[4];
        Dln.Gpio.Module[] DLNgpio = new Dln.Gpio.Module[4];
        public FGraph MyOwner = null;
        public int m_DrvIC_Addr = 0x34;
        public int m_BU242_Addr = 0x3E; //   0x70  0x32/0x50/0x40
        public int m_BU242_EEPROM_Addr = 0x50; //   0x70  0x32/0x50/0x40       //190701, 190701 성창규C가 공유한 Command List 부분 추가
        public int m_CS_Addr = 0x40;
        //public int m_DAC_Addr = 0x4C; //  0x4C, 0x4D
        public int m_DAC_Addr = 0x4F; //  0x4C, 0x4D
        public int m_PortCount = 0;
        public bool[] m_DACexists = new bool[4];
        public bool[] m_CurSensexists = new bool[4];
        public bool mSwitchOn = false;
        public bool mUnsafeOn = false;
        public bool m_bEngageSafeEvent = false;
        public bool m_bEngageSwitchEvent = false;

        public bool m_StartRun = false;
        public bool m_bUnsafe = false;
        public int m_nUnsafe = 0;
        public bool m_bWorking = false;
        public string[] m_errMsg = new string[4] { "", "", "", "" };
        bool m_bOccupied = false;
        public byte[] m_0x0004 = new byte[4];
        public string[] mStrLog = new string[4] { "", "", "", "" };
        public bool[] m_bBU252HallCalFinish = new bool[4];
        public VariBU252[] mVariBU252 = new VariBU252[4] {null, null, null, null};

        public void ClearLog(int ch)
        {
            mStrLog[ch] = "";
        }
        public string GetLog(int ch)
        {
            return mStrLog[ch];
        }
        public void AddLog(int ch, string str)
        {
            if (mStrLog[ch].Length < 10000)
                mStrLog[ch] += str + "\r\n";
            else
                mStrLog[ch] = str + "\r\n";
        }

        //        int Frequency = 900000;   // for AK7371
        int MaxReplyCount = 10;

        public AFDrvIC()
        {
            //InitSR700();
        }
        /// <summary>
        /// Bar Code Reader
        /// </summary>
        //public void InitSR700()
        //{
        //    StreamReader Barcodeinit = new StreamReader("BarcodeReaderChannel.txt");

        //    string BarCodePort1, BarCodePort2;//, BarCodePort3, BarCodePort4;
        //    BarCodePort1 = Barcodeinit.ReadLine();
        //    BarCodePort2 = Barcodeinit.ReadLine();
        //    //BarCodePort3 = Barcodeinit.ReadLine();
        //    //BarCodePort4 = Barcodeinit.ReadLine();


        //    mSR700[0] = new System.IO.Ports.SerialPort(BarCodePort1);
        //    mSR700[1] = new System.IO.Ports.SerialPort(BarCodePort2);
        //    //mSR700[2] = new System.IO.Ports.SerialPort(BarCodePort3);
        //    //mSR700[3] = new System.IO.Ports.SerialPort(BarCodePort4);


        //    //mSR700[0] = new System.IO.Ports.SerialPort("COM5");
        //    //mSR700[1] = new System.IO.Ports.SerialPort("COM6");
        //    //mSR700[2] = new System.IO.Ports.SerialPort("COM7");
        //    //mSR700[3] = new System.IO.Ports.SerialPort("COM8");

        //    for ( int i=0; i<2; i++)
        //    {
        //        mSR700[i].BaudRate = 115200;         // 9600, 19200, 38400, 57600 or 115200
        //        mSR700[i].DataBits = 8;              // 7 or 8
        //        mSR700[i].Parity = Parity.Even;    // Even or Odd
        //        mSR700[i].StopBits = StopBits.One;   // One or Two
        //    }
        //}

        public bool laserOff(int ch)
        {
            string loff = "\x02LOFF\x03";   // <STX>LOFF<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(loff);

            try
            {
                if (mSR700[ch].IsOpen)
                    mSR700[ch].Close();

                mSR700[ch].Open();
                mSR700[ch].ReadTimeout = 100;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(mSR700[ch].PortName + "\r\n" + ex.Message);  // non-existent or disappeared
                return false;
            }

            if (mSR700[ch].IsOpen)
            {
                try
                {
                    mSR700[ch].Write(sendBytes, 0, sendBytes.Length);
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(mSR700[ch].PortName + "\r\n" + ex.Message);    // disappeared
                    return false;
                }
            }
            else
            {
                //MessageBox.Show(mSR700[ch].PortName + " is disconnected.");
                return false;
            }
            return true;
        }

        public bool RequestBarcode(int ch)
        {
            string lon = "\x02LON\x03";   // <STX>LON<ETX>
            Byte[] sendBytes = ASCIIEncoding.ASCII.GetBytes(lon);

            try
            {
                if (mSR700[ch].IsOpen)
                    mSR700[ch].Close();

                mSR700[ch].Open();
                mSR700[ch].ReadTimeout = 100;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(mSR700[ch].PortName + "\r\n" + ex.Message);  // non-existent or disappeared
                return false;
            }

            if (mSR700[ch].IsOpen)
            {
                try
                {
                    mSR700[ch].Write(sendBytes, 0, sendBytes.Length);
                }
                catch (IOException ex)
                {
                    //MessageBox.Show(mSR700[ch].PortName + "\r\n" + ex.Message);    // disappeared
                    return false;
                }
            }
            else
            {
                //MessageBox.Show(mSR700[ch].PortName + " is disconnected.");
                return false;
            }
            return true;
        }
        private int readDataSub(int ch, Byte[] recvBytes)
        {
            int recvSize = 0;
            bool isCommandRes = false;
            Byte d;
            Byte STX = 0x02;
            Byte ETX = 0x03;
            Byte CR = 0x0d;

            //
            // Distinguish between command response and read data.
            //
            try
            {
                d = (Byte)mSR700[ch].ReadByte();
                recvBytes[recvSize++] = d;
                if (d == STX)
                {
                    isCommandRes = true;    // Distinguish between command response and read data.
                }
            }
            catch (TimeoutException)
            {
                return 0;   //  No data received.
            }

            //
            // Receive data until the terminator character.
            //
            for (; ; )
            {
                try
                {
                    d = (Byte)mSR700[ch].ReadByte();
                    recvBytes[recvSize++] = d;

                    if (isCommandRes && (d == ETX))
                    {
                        break;  // Command response is received completely.
                    }
                    else if (d == CR)
                    {
                        break;  // Read data is received completely.
                    }
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(ex.Message);
                    return 0;
                }
            }
            return recvSize;
        }
        /// <summary>
        /// // End Of Barcode Reader
        /// </summary>


        /// <summary>
        /// //////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="nI2Cclock"></param>
        public void SetConfig(int ch, int nI2Cclock = 400)
        {
            try
            {
                if (DLNi2c[ch].Restrictions.MaxReplyCount != Restriction.NotSupported)
                    DLNi2c[ch].MaxReplyCount = MaxReplyCount;

                if (DLNi2c[ch].Restrictions.Frequency == Restriction.MustBeDisabled)
                    DLNi2c[ch].Enabled = false;

                DLNi2c[ch].Frequency = nI2Cclock * 1000;
                DLNi2c[ch].Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SetConfig() " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetI2CClock(int nI2Cclock, int camCnt, int channelcnt)
        {
            DLNi2c[0].Enabled = false;
            if (camCnt > 1)
                DLNi2c[1].Enabled = false;

            Thread.Sleep(10);
            DLNi2c[0].Frequency = nI2Cclock * 1000;
            if (camCnt > 1)
                DLNi2c[1].Frequency = nI2Cclock * 1000;

            Thread.Sleep(10);
            DLNi2c[0].Enabled = true;
            if (camCnt > 1)
                DLNi2c[1].Enabled = true;
            //DLNi2c[0].Enabled = false;
            //if (channelcnt > 1)
            //    DLNi2c[1].Enabled = false;
            //if(camCnt > 1)
            //{
            //    DLNi2c[2].Enabled = false;
            //    DLNi2c[3].Enabled = false;
            //}


            //Thread.Sleep(10);
            //DLNi2c[0].Frequency = nI2Cclock * 1000;
            //if (channelcnt > 1)
            //    DLNi2c[1].Frequency = nI2Cclock * 1000;
            //DLNi2c[0].Enabled = true;
            //if (channelcnt > 1)
            //    DLNi2c[1].Enabled = true;
            //if (camCnt > 1)
            //{
            //    DLNi2c[2].Frequency = nI2Cclock * 1000;
            //    DLNi2c[3].Frequency = nI2Cclock * 1000;
            //    DLNi2c[2].Enabled = true;
            //    DLNi2c[3].Enabled = true;
            //}

        }

        public int MOTID = 0;
        public bool InitI2CnGPIO(int nI2Cclock, int camCnt, int channelCnt, bool retry = false) //    Device 개수확인 및 Slave Address 확인
        {
            if (m_PortCount > 1 && !retry) return true;
            //Library.Disconnect("localhost");
            int j;
            Library.Connect("localhost", Connection.DefaultPort);

            // Open device
            int ldeviceCount = (int)(Device.Count());
            //MessageBox.Show("ldeviceCount = " + ldeviceCount);
            m_PortCount = ldeviceCount;
            if (ldeviceCount == 0)
            {
                MessageBox.Show("--- No DLN-adapters ---.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
           
            // temporary erased by khkim 191106
            //if (ldeviceCount < 4 && camCnt > 1)                
            //    MessageBox.Show("Only " + ldeviceCount + " channel(s) are Found. Check USB Cable!");    // disappeared
            //else if (ldeviceCount < 2 && channelCnt >1)
            //    MessageBox.Show("Only " + ldeviceCount + " channel(s) are Found. Check USB Cable!");
            //else if (channelCnt <1)
            //    MessageBox.Show("Only " + channelCnt + " channel(s) are Found. Check USB Cable!"); //hch    

            for ( j=0; j< ldeviceCount; j++ )
            {
                try
                {
                    DLNdevice[j] = Device.Open(j);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Port " + j +" : " + ex.Message + "\n Re-Connect USB Cable!");    // disappeared
                    return false;
                }
                ////////////////////////////////////////////////////////////////
                //  Initialize I2C
                try
                {
                    if (DLNdevice[j].I2cMaster.Ports[0].Restrictions.MaxReplyCount != Restriction.NotSupported)
                        DLNdevice[j].I2cMaster.Ports[0].MaxReplyCount = MaxReplyCount;

                    if (DLNdevice[j].I2cMaster.Ports[0].Restrictions.Frequency == Restriction.MustBeDisabled)
                        DLNdevice[j].I2cMaster.Ports[0].Enabled = false;

                    DLNdevice[j].I2cMaster.Ports[0].Frequency = nI2Cclock * 1000;
                    DLNdevice[j].I2cMaster.Ports[0].Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SetConfig() " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ////////////////////////////////////////////////////////////////
                //  Initialize GPIO 
                //int pinCount = DLNdevice[j].Gpio.Pins.Count;
                int pinCount = 32;
                if (pinCount == 0)
                {
                    MessageBox.Show("Current DLN-series adapter doesn't support GPIO interface.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return false;
                }
                // ID
                for ( int dio=1; dio<8; dio++)
                {
                    //  PA0 ~ PA7 (0 ~ 7) : in
                    if ( dio < 6)
                    {
                        //  PA1 ~ PA5 (1 ~ 5) : in 0
                        DLNdevice[j].Gpio.Pins[dio].Enabled = true;
                        DLNdevice[j].Gpio.Pins[dio].Direction = 0;
                        DLNdevice[j].Gpio.Pins[dio].PullupEnabled = true;
                    }
                    else
                    {
                        //  PortID
                        //  PA6 ~ PA7 (6 ~ 7) : in 1
                        DLNdevice[j].Gpio.Pins[dio].Enabled = true;
                        DLNdevice[j].Gpio.Pins[dio].Direction = 0;
                        DLNdevice[j].Gpio.Pins[dio].PulldownEnabled = true;
                    }
                    //  
                }

                for (int dio = 28; dio < 32; dio++)
                {
                    //  PD4 ~ PD7 (28 ~ 31) : 0(in)
                    DLNdevice[j].Gpio.Pins[dio].Enabled = true;
                    DLNdevice[j].Gpio.Pins[dio].Direction = 0;
                    DLNdevice[j].Gpio.Pins[dio].PullupEnabled = true;
                    //  
                }
                ////  PC3 ( 19 ) : in 1
                //DLNdevice[j].Gpio.Pins[19].Enabled = true;
                //DLNdevice[j].Gpio.Pins[19].Direction = 0;
                //DLNdevice[j].Gpio.Pins[19].PullupEnabled = true;

                // 스위치
                //DLNdevice[j].Gpio.Pins[8].Enabled = true;
                //DLNdevice[j].Gpio.Pins[8].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //DLNdevice[j].Gpio.Pins[8].PulldownEnabled = true;
                //DLNdevice[j].Gpio.Pins[8].Enabled = false;

                //// 안전센서
                //DLNdevice[j].Gpio.Pins[9].Enabled = true;
                //DLNdevice[j].Gpio.Pins[9].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //DLNdevice[j].Gpio.Pins[9].PulldownEnabled = true;
                //DLNdevice[j].Gpio.Pins[9].Enabled = false;

                //I2C 0x24관련    OIS_RESET
                //DLNdevice[j].Gpio.Pins[14].Enabled = true;
                //DLNdevice[j].Gpio.Pins[14].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //DLNdevice[j].Gpio.Pins[14].PulldownEnabled = true;
                //DLNdevice[j].Gpio.Pins[14].Enabled = false;

                for (int dio = 16; dio < 22; dio++)
                {
                    DLNdevice[j].Gpio.Pins[dio].Enabled = true;
                    DLNdevice[j].Gpio.Pins[dio].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                    DLNdevice[j].Gpio.Pins[dio].PulldownEnabled = true;
                }
                DLNdevice[j].Gpio.Pins[28].Enabled = true;
                DLNdevice[j].Gpio.Pins[28].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[j].Gpio.Pins[28].PulldownEnabled = true;

                DLNdevice[j].Gpio.Pins[29].Enabled = true;
                DLNdevice[j].Gpio.Pins[29].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                DLNdevice[j].Gpio.Pins[29].PulldownEnabled = true;

                // 실린더
                //DLNdevice[j].Gpio.Pins[24].Enabled = true;
                //DLNdevice[j].Gpio.Pins[24].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

                if ( j==1)
                {
                    DLNdevice[j].Gpio.Pins[24].Enabled = true;
                    DLNdevice[j].Gpio.Pins[24].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                }

                //DLNdevice[j].Gpio.Pins[25].Enabled = true;
                //DLNdevice[j].Gpio.Pins[25].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)

                //// FailLED
                //DLNdevice[j].Gpio.Pins[26].Enabled = true;
                //DLNdevice[j].Gpio.Pins[26].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //// FailLED
                //DLNdevice[j].Gpio.Pins[27].Enabled = true;
                //DLNdevice[j].Gpio.Pins[27].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)


                //DLNdevice[0].Gpio.Pins[31].Enabled = true;  //   Main Power On / Off
                //DLNdevice[0].Gpio.Pins[31].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //DLNdevice[0].Gpio.Pins[31].PulldownEnabled = true;



                //  I2C : 하기 불필요, SetConfig()에서 설정.
                //DLNdevice[j].Gpio.Pins[21].Enabled = false; //  for I2C
                //DLNdevice[j].Gpio.Pins[22].Enabled = false; //  for I2C

                //  GPIO PC0, PC1 핀이 각각 0bit 1bit 가 되어 디바이스 번호가 정해진다.
                //  PC1 PC1
                //  0   0   => ID = 0
                //  0   1   => ID = 1
                //  1   0   => ID = 2
                //  1   1   => ID = 3
                Thread.Sleep(100);

                int[] res = new int[2];
                res[0] = DLNdevice[j].Gpio.Pins[6].Value;
                int portID = 0;
                if (res[0] == 1)
                    portID++;

                res[1] = DLNdevice[j].Gpio.Pins[7].Value;
                if (res[1] == 1)
                    portID += 2;


                //if (portID == 0 || portID == 2) //안전센서는 0만 연결
                //{
                //    DLNdevice[j].Gpio.Pins[9].Enabled = true;
                //    DLNdevice[j].Gpio.Pins[9].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //    DLNdevice[j].Gpio.Pins[9].PulldownEnabled = true;
                //}
                //else
                //{
                //    //  Driver IC Power On
                //    DLNdevice[j].Gpio.Pins[9].Enabled = true;
                //    DLNdevice[j].Gpio.Pins[9].Direction = 1;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                //    DLNdevice[j].Gpio.Pins[9].PulldownEnabled = true;
                //}
                ////////////////////////////////////////////////////////////////
                //  Initialize I2C to Driver IC 
                int portCount = DLNdevice[j].I2cMaster.Ports.Count;
                if (portCount == 0)
                {
                    MessageBox.Show("Current DLN-series adapter doesn't support I2C Master interface.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                DLNi2c[portID] = DLNdevice[j].I2cMaster.Ports[0];
                DLNgpio[portID] = DLNdevice[j].Gpio;

                //MessageBox.Show("PortID = " + portID.ToString() + " pin6=" + res[0].ToString() + "pin7=" + res[1].ToString());

                //bool IsHere = CheckCurSensExists(portID);
                ///////////////////////////////////////////////////
                //SetConfig(portID, nI2Cclock);     //  Initialize I2C
            }

            //  Test Start Switch
            //DLNgpio[0].Pins[8].ConditionMetThreadSafe += SWEventHandler;
            //DLNgpio[0].Pins[8].SetEventConfiguration(Dln.Gpio.EventType.LevelHigh, 50); //  검증완료
            //m_bEngageSwitchEvent = true;
            //

            /////////////////////////////////////////////////////////////////////////////////////
            /////   안전센서
            //DLNgpio[0].Pins[9].ConditionMetThreadSafe += SafeEventHandler;
            //DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 100);  ////////////////////////////////////////////////////////////////
            //m_bEngageSafeEvent = true;
            //DLNgpio[0].Pins[9].ConditionMetThreadSafe -= SafeEventHandler;
            //m_bEngageSafeEvent = false;
            j = 0;
            int[] motIDbit = new int[10];
            motIDbit[5] = DLNdevice[j].Gpio.Pins[1].Value;  //  A1
            motIDbit[6] = DLNdevice[j].Gpio.Pins[2].Value;  //  A2
            motIDbit[7] = DLNdevice[j].Gpio.Pins[3].Value;  //  A3
            motIDbit[8] = DLNdevice[j].Gpio.Pins[4].Value;  //  A4
            motIDbit[9] = DLNdevice[j].Gpio.Pins[5].Value;  //  A5

            //motIDbit[0] = DLNdevice[j].Gpio.Pins[19].Value; //  C3
            //motIDbit[1] = DLNdevice[j].Gpio.Pins[28].Value; //  D4
            //motIDbit[2] = DLNdevice[j].Gpio.Pins[29].Value; //  D5
            //motIDbit[3] = DLNdevice[j].Gpio.Pins[30].Value; //  D6
            //motIDbit[4] = DLNdevice[j].Gpio.Pins[31].Value; //  D7

            int motID0 = 0;
            for (int q = 0; q < 10; q++)
                motID0 += (1-motIDbit[q]) << q;

            if (ldeviceCount>1)
            {
                j = 1;
                motIDbit[5] = DLNdevice[j].Gpio.Pins[1].Value;  //  A1
                motIDbit[6] = DLNdevice[j].Gpio.Pins[2].Value;  //  A2
                motIDbit[7] = DLNdevice[j].Gpio.Pins[3].Value;  //  A3
                motIDbit[8] = DLNdevice[j].Gpio.Pins[4].Value;  //  A4
                motIDbit[9] = DLNdevice[j].Gpio.Pins[5].Value;  //  A5

                //motIDbit[0] = DLNdevice[j].Gpio.Pins[19].Value; //  C3
                //motIDbit[1] = DLNdevice[j].Gpio.Pins[28].Value; //  D4
                //motIDbit[2] = DLNdevice[j].Gpio.Pins[29].Value; //  D5
                //motIDbit[3] = DLNdevice[j].Gpio.Pins[30].Value; //  D6
                //motIDbit[4] = DLNdevice[j].Gpio.Pins[31].Value; //  D7
            }
            int motID1 = 0;
            for (int q = 0; q < 10; q++)
                motID1 += (1-motIDbit[q]) << q;

            MOTID = Math.Max(motID0, motID1);
            return true;
        }
        public bool[] bSocketOn = new bool[4];

        public void TrigggerFaultDetected()
        {
            if (m_PortCount>1)
                DLNgpio[1].Pins[24].OutputValue = 1;
        }
        public void TriggerFaultClear()
        {
            if (m_PortCount > 1)
                DLNgpio[1].Pins[24].OutputValue = 0;
        }
        public void SocketTest(int mode, bool bOn)
        {
            try
            {
                //mode 0, 1, 2 
                if (mode == 3)
                {
                    if (bOn)
                    {
                        DLNgpio[1].Pins[28].OutputValue = 1;
                        bSocketOn[mode] = true;
                    }
                    else
                    {
                        DLNgpio[1].Pins[28].OutputValue = 0;
                        bSocketOn[mode] = false;
                    }
                }
                else
                {
                    if (bOn)
                    {
                        DLNgpio[1].Pins[16 + mode * 2].OutputValue = 1;
                        DLNgpio[1].Pins[17 + mode * 2].OutputValue = 0;
                        bSocketOn[mode] = true;
                    }
                    else
                    {
                        DLNgpio[1].Pins[16 + mode * 2].OutputValue = 0;
                        DLNgpio[1].Pins[17 + mode * 2].OutputValue = 1;
                        bSocketOn[mode] = false;
                    }
                }
            }
            catch
            {
                CheckDrvICToActive();
                try
                {
                    //mode 0, 1, 2 
                    if (mode == 3)
                    {
                        if (bOn)
                        {
                            DLNgpio[1].Pins[28].OutputValue = 1;
                            bSocketOn[mode] = true;
                        }
                        else
                        {
                            DLNgpio[1].Pins[28].OutputValue = 0;
                            bSocketOn[mode] = false;
                        }
                    }
                    else
                    {
                        if (bOn)
                        {
                            DLNgpio[1].Pins[16 + mode * 2].OutputValue = 1;
                            DLNgpio[1].Pins[17 + mode * 2].OutputValue = 0;
                            bSocketOn[mode] = true;
                        }
                        else
                        {
                            DLNgpio[1].Pins[16 + mode * 2].OutputValue = 0;
                            DLNgpio[1].Pins[17 + mode * 2].OutputValue = 1;
                            bSocketOn[mode] = false;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Fail to LED Power :: Please Check USB Cable");
                }
            }
        }
        public int ReadID(int ch)
        {
            int res = 0;
            if ( DLNgpio[ch].Pins[6].Value == 1)
                res++;
            if ( DLNgpio[ch].Pins[7].Value == 1)
                res += 2;
            return res;
        }
        
        public void AckSignal(int ch, bool IsHigh)
        {
            DLNgpio[ch].Pins[16].OutputValue = (IsHigh ? 1 : 0);
        }
        public void SetFailLED(int ch, bool IsFail)
        {
            //try
            //{
            //    int lch = (ch / 2) * 2 + 1; //  0,1 --> 1   ;; 2, 3 --> 3
            //                                //  ch0, ch1 -> ch1 ;   ch2, ch3 -> ch3
            //                                //int dio = 26 + (ch % 2);
            //                                //MessageBox.Show(ch.ToString() + ":" + dio.ToString());
            //    DLNgpio[lch].Pins[26 + (ch % 2)].OutputValue = (IsFail ? 1 : 0);
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        public void DriverICPower(int port, bool IsOn=true )
        {
            if ( IsOn )
            {
                DLNgpio[0].Pins[9].OutputValue = 1;
                DLNgpio[0].Pins[31].OutputValue = 1;
                //DLNgpio[port * 2].Pins[9].OutputValue = 1;
                //DLNgpio[port * 2].Pins[31].OutputValue = 1;
            }
            else
            {
                DLNgpio[0].Pins[9].OutputValue = 0;
                DLNgpio[0].Pins[31].OutputValue = 0;
                //DLNgpio[port * 2].Pins[9].OutputValue = 0;
                //DLNgpio[port * 2].Pins[31].OutputValue = 0;
            }
        }
        public void OISDriverICReset(int port, bool IsOn=true )
        {
            //int ch = 3;
            if (IsOn)
            {
                DLNgpio[0].Pins[14].OutputValue = 1;
                //DLNgpio[port * 2].Pins[14].OutputValue = 1;
            }
            else
            {
                DLNgpio[0].Pins[14].OutputValue = 0;
                //DLNgpio[port * 2].Pins[14].OutputValue = 0;
            }
        }
        public void UnloadSocket(int port)
        {
            int ch = port * 2;
            //  port0 ->ch0 ; port1 -> ch2
            //MessageBox.Show("In ch = " + ch.ToString());
            DLNgpio[ch].Pins[24].OutputValue = 1;
            DLNgpio[ch].Pins[25].OutputValue = 0;
        }
        public void LoadSocket(int port)
        {
            int ch = port * 2;
            //  port0 ->ch0 ; port1 -> ch2
            //MessageBox.Show("Out ch = " + ch.ToString());
            DLNgpio[ch].Pins[24].OutputValue = 0;
            DLNgpio[ch].Pins[25].OutputValue = 1;
        }

        public void EngageSafeEvent(bool IsEngage)
        {
            if ( IsEngage  && !m_bEngageSafeEvent )
            {
                DLNgpio[0].Pins[9].ConditionMetThreadSafe += SafeEventHandler;
                m_bEngageSafeEvent = true;
                
            }
            else if (!IsEngage && m_bEngageSafeEvent)
            {
                DLNgpio[0].Pins[9].ConditionMetThreadSafe -= SafeEventHandler;
                m_bEngageSafeEvent = false;
            }
        }
        public void EngageSwitchEvent(bool IsEngage)
        {
            if (IsEngage && !m_bEngageSwitchEvent)
            {
                DLNgpio[0].Pins[8].ConditionMetThreadSafe += SWEventHandler;
                m_bEngageSwitchEvent = true;
            }
            else if (!IsEngage && m_bEngageSwitchEvent)
            {
                DLNgpio[0].Pins[8].ConditionMetThreadSafe -= SWEventHandler;
                m_bEngageSwitchEvent = false;
            }
        }
        private void SWEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {
            //  검증완료됨.
            // 양수버튼이 눌렸을 때
            if (e.Value == 1 && !mSwitchOn)
            {
                mSwitchOn = true;
            }
            DLNgpio[0].Pins[8].ConditionMetThreadSafe -= SWEventHandler;
            m_bEngageSwitchEvent = false;

            m_StartRun = true;
            Thread.Sleep(200);
            m_StartRun = false;

            DLNgpio[0].Pins[8].ConditionMetThreadSafe += SWEventHandler;
            m_bEngageSwitchEvent = true;
            //string data;

            //data = String.Format("Pin={0}, Value={1}, EventType={2}", e.Pin, e.Value, e.EventType.ToString());
            //data += Environment.NewLine;
        }
        public void ClearSWEvent()
        {
            mSwitchOn = false;
        }
        private void SafeEventHandler(object sender, Dln.Gpio.ConditionMetEventArgs e)
        {

            if (e.Value == 1 && !mUnsafeOn)
            {
                mUnsafeOn = true;
             
            }
            DLNgpio[0].Pins[9].ConditionMetThreadSafe -= SafeEventHandler;
            //DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 2000);  ////////////////////////////////////////////////////////////////
            m_bEngageSafeEvent = false;

            m_bUnsafe = true;
            m_nUnsafe++;

            Thread.Sleep(200);

            m_bUnsafe = false;
            //DLNgpio[0].Pins[9].SetEventConfiguration(Dln.Gpio.EventType.LevelLow, 100);  ////////////////////////////////////////////////////////////////
            DLNgpio[0].Pins[9].ConditionMetThreadSafe += SafeEventHandler;
            m_bEngageSafeEvent = true;
         
        }
        public void ClearUnsafe()
        {
            mUnsafeOn = false;
        }

        /// <summary>
        /// I2C Functions.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="value"></param>

        public bool CheckCurSensExists(int ch)
        {
            try
            {
                int[] list = DLNi2c[ch].ScanDevices();

                m_CurSensexists[ch] = false;

                if (list.Length > 0)
                {
                    foreach (int addr in list)
                    {
                        if (addr == 0x40) m_CurSensexists[ch] = true;
                    }
                }
                return m_CurSensexists[ch];
            }
            catch (Exception ex)
            {
                MessageBox.Show("CheckCurSensExists() " + "," + ch.ToString() + ","  + ex.Message);
                return false;
            }
        }

        public bool CheckDriverICExists(int ch)
        {
            try
            {
                int[] list = DLNi2c[ch].ScanDevices();

                m_CurSensexists[ch] = false;

                if (list.Length > 0)
                {
                    foreach (int addr in list)
                    {
                        if (addr == m_DrvIC_Addr) return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("CheckDriverICExists() " + "," + ch.ToString() + "," + ex.Message);
                return false;
            }
        }

        public void CheckDACExists(int port)
        {
            int[] list = DLNi2c[port*2 + 1].ScanDevices();

            m_DACexists[port * 2] = m_DACexists[port * 2] = false;

            if (list.Length > 0)
            {
                foreach (int addr in list)
                {
                    if (addr == 0x4C) m_DACexists[port * 2] = true;

                    if (addr == 0x4D) m_DACexists[port * 2 + 1] = true;
                }
            }
        }

        //public void SetLEDpower(int id, int value, bool check=true)
        //{
        //    //            if (!m_DACexists[ch]) return;
        //    byte bufferH = 0;
        //    byte[] bufferL = new byte[1];

        //    int lDACaddr = 0x4F;        // A0,A1상태에 따라 ID 변경, 지금은  A0,A1 pull up


        //    if (value > 1500)
        //        value = 1500;
        //    //  기존 single channel dac code
        //    //   | XXXX | XXXX |  
        //    //   | XXXX | XXXX | XXXX | 0000 |
        //    //   | Address | CtrlByte | Value(12bit) |
        //    bufferH = (byte)(value / 16);
        //    bufferL[0] = (byte)(value << 4);



        //    //  기존 single channel dac code
        //    //bufferH = (byte)(value / 256);
        //    //bufferL[0] = (byte)(value % 256);


        //    byte[] left_side = { 0x10 };      //1
        //    byte[] left_center = { 0x12 };    //2
        //    byte[] right_side = { 0x14 };     //3
        //    byte[] right_center = { 0x16 };   //4


        //    int ch = 0;

        //    try
        //    {
        //        //DLNi2c[ch].Write(0x73, new byte[1] { 0x07 });
        //        if (id == 1)
        //        {
        //            byte[] datas = { left_side[0], bufferH, bufferL[0] };
        //            DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
        //        }
        //        else if (id == 2)
        //        {
        //            byte[] datas = { left_center[0], bufferH, bufferL[0] };
        //            DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
        //        }
        //        else if (id == 3)
        //        {
        //            byte[] datas = { right_side[0], bufferH, bufferL[0] };
        //            DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
        //        }
        //        else if (id == 4)
        //        {
        //            byte[] datas = { right_center[0], bufferH, bufferL[0] };
        //            DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
        //        }
        //        //DLNi2c[ch].Write(0x73, new byte[1] { 0x08 });
        //    }
        //    catch (Exception ex)
        //    {
        //        //MessageBox.Show("Fail to Set LED Power :: " + ex.Message);
        //    }
        //}
        public void SetLEDpower(int id, int value, bool check = true)
        {
            //            if (!m_DACexists[ch]) return;
            byte bufferH = 0;
            byte[] bufferL = new byte[1];

            int lDACaddr = 0x4F;        // A0,A1상태에 따라 ID 변경, 지금은  A0,A1 pull up


            if (value > 4095)
                value = 4095;
            //  기존 single channel dac code
            //   | XXXX | XXXX |  
            //   | XXXX | XXXX | XXXX | 0000 |
            //   | Address | CtrlByte | Value(12bit) |
            bufferH = (byte)(value / 16);
            bufferL[0] = (byte)(value << 4);



            //  기존 single channel dac code
            //bufferH = (byte)(value / 256);
            //bufferL[0] = (byte)(value % 256);


            byte[] left_side = { 0x10 };      //1
            byte[] left_center = { 0x12 };    //2
            byte[] right_side = { 0x14 };     //3
            byte[] right_center = { 0x16 };   //4


            int ch = 0;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;

            try
            {
                if (DLNi2c[ch] == null)
                {
                    m_bOccupied = false;
                    return;
                }
                //DLNi2c[ch].Write(0x73, new byte[1] { 0x07 });
                if (id == 1)
                {
                    byte[] datas = { left_side[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 2)
                {
                    byte[] datas = { left_center[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 3)
                {
                    byte[] datas = { right_side[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                else if (id == 4)
                {
                    byte[] datas = { right_center[0], bufferH, bufferL[0] };
                    DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                }
                m_bOccupied = false;
            }
            catch 
            {
                CheckDrvICToActive();

                try
                {
                    if (id == 1)
                    {
                        byte[] datas = { left_side[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 2)
                    {
                        byte[] datas = { left_center[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 3)
                    {
                        byte[] datas = { right_side[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                    else if (id == 4)
                    {
                        byte[] datas = { right_center[0], bufferH, bufferL[0] };
                        DLNi2c[ch].Write(lDACaddr, datas); // diolan(0,1기준) 1번에서  LED control
                    }
                }
                catch
                {
                    MessageBox.Show("Fail to LED Power :: Please Check USB Cable");
                    m_bOccupied = false;
                }
                m_bOccupied = false;
            }
        }
        public double GetCurrent(int ch)
        {
            double res = 0;
            int RegAddr = 0x01;
            byte[] buffer2 = new byte[2];
            try
            {
                DLNi2c[ch].Read(m_CS_Addr, 1, RegAddr, buffer2);
                res = (buffer2[0] * 256 + buffer2[1]) / 10.0;
                if ( res < 0.2)
                {
                    DLNi2c[ch].Read(m_CS_Addr, 1, RegAddr, buffer2);
                    res = (buffer2[0] * 256 + buffer2[1]) / 10.0;
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show("Fail to Set LED Power " + ex.Message);
            }
            return res;
        }

        public void SetSlaveAddr(int newAddr)
        {
            m_DrvIC_Addr = newAddr;
        }

        public int GetSlaveAddr()
        {
            return m_DrvIC_Addr;
        }

        public byte RetryRead(int ch, int memAddr, byte toBe, int maxRetry = 1)
        {
            byte[] buffer = new byte[1];
            int MaxRetry = maxRetry;
            int iRetry = 0;

            try
            {
                while (iRetry++ < MaxRetry)
                {
                    Thread.Sleep(20);
                    while (m_bOccupied)
                    {
                        Thread.Sleep(1);
                    }
                    m_bOccupied = true;
                    DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer);
                    m_bOccupied = false;
                    if (buffer[0] == toBe)
                        return buffer[0];

                }
                return buffer[0];
            }
            catch (Exception ex)
            {
               // MessageBox.Show("Retry Read : " + ch.ToString() + " " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return 0xFF;
            }
        }
        public byte FRetryRead(int ch, int memAddr, byte toBe, int maxRetry = 1)
        {
            byte[] buffer = new byte[1];
            int MaxRetry = maxRetry;
            int iRetry = 0;

            try
            {
                while (iRetry++ < MaxRetry)
                {
                    Thread.Sleep(20);
                    m_bOccupied = true;
                    DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer);
                    m_bOccupied = false;
                    if (buffer[0] == toBe)
                        return buffer[0];

                }
                return buffer[0];
            }
            catch (Exception ex)
            {
             //   MessageBox.Show("Retry Read : " + ch.ToString() + " " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return 0xFF;
            }
        }

        public bool CheckDrvICToActive(int nI2Cclock = 400, int camCnt =2, int channelCnt = 4)
        {
            return InitI2CnGPIO(nI2Cclock, camCnt, channelCnt, true);
        }

        public bool ChangeLoopGain(bool IsTest=true, int mFactor=150 )
        {
            return true;
        }

        public bool ChangeULmargin(bool IsTest = true, int du = 0, int dl = 0)
        {
            return true;
        }

        public bool FWriteToSlave(int ch, int memAddr, byte[] data)
        {
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
        }

        public bool FReadFromSlave(int ch, int memAddr, byte[] data)
        {
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
        }

        public bool WriteToSlave( int ch, int memAddr, byte[] data)
        {
            while(m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
        }

        public bool ReadFromSlave( int ch, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                mDrvICchannel[ch] = true;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                mDrvICchannel[ch] = false;
                return false;
            }
        }

        public byte CheckIfOISSTS_IDLE(int ch, bool bCheck0000 = true)
        {
            return 0;

            //byte[] wBuffer = new Byte[1];
            //wBuffer[0] = 0;
            //if (bCheck0000)
            //    WriteToSlave( ch, 0x0, wBuffer);

            //int memAddr = 0x0001;
            //byte lToBe = 0x01;

            ////Thread.Sleep(30);
            //Thread.Sleep(10);
            //byte result = 0;
            //result = RetryRead(ch, memAddr, lToBe, 10);

            //if (result != lToBe)
            //{
            //    //MessageBox.Show("CheckIfOISSTS_IDLE:0x01 but " + result.ToString("X"));
            //    if (bCheck0000)
            //    {
            //        WriteToSlave(ch, 0x0, wBuffer);
            //    }

            //    Thread.Sleep(100);
            //    result = RetryRead(ch, memAddr, lToBe, 10);
            //    //MessageBox.Show("Retry CheckIfOISSTS_IDLE:0x01 but " + result.ToString("X"));
            //}
            //if (result != lToBe)
            //{
            //    return result;
            //}

            //return 0;
        }

        public string FReadHWFWversion(int ch)
        {
            string txtReadData = "";
            try
            {
                //////////////////////////////////////////////////////////////////////////////////
                //  Read HW Ver

                byte[] buffer = new byte[4];
                int memAddr = 0x100C;
                //int memAddr = 0x00F8;

                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;


                for (int i = 0; i < 4; i++)
                {
                    string hex = String.Format("{0:X02} ", buffer[i]);
                    txtReadData += hex;
                }
                txtReadData += " \t";

                //////////////////////////////////////////////////////////////////////////////////
                //  Read FW Ver

                memAddr = 0x1008;
                //memAddr = 0x00FC;
                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;

                for (int i = 0; i < 4; i++)
                {
                    string hex = String.Format("{0:X02} ", buffer[i]);
                    txtReadData += hex;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return "error";
            }
            return txtReadData;
        }

        public string ReadHWFWversion(int ch)
        {
            string txtReadData = "";
            try
            {
                //////////////////////////////////////////////////////////////////////////////////
                //  Read HW Ver

                byte[] buffer = new byte[4];
                int memAddr = 0x100C;
                //int memAddr = 0x00F8;

                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;


                for (int i = 0; i < 4; i++)
                {
                    string hex = String.Format("{0:X02} ", buffer[i]);
                    txtReadData += hex;
                }
                txtReadData += " \t";

                //////////////////////////////////////////////////////////////////////////////////
                //  Read FW Ver

                memAddr = 0x1008;
                //memAddr = 0x00FC;
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;

                for (int i = 0; i < 4; i++)
                {
                    string hex = String.Format("{0:X02} ", buffer[i]);
                    txtReadData += hex;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return "error";
            }
            return txtReadData;
        }

        public bool FWriteHallPolarity(int ch)
        {
            int memAddr = 0x0400;
            int memData = 0x01;
            byte[] buffer = new byte[1];

            m_bOccupied = true;
            try
            {
                // Target Position Write 이외에는 1byte 씩만 Write 할 것.
                buffer[0] = (byte)(memData);
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                //MessageBox.Show("My " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return false;
            }

            return true;
        }
        public bool WriteHallPolarity(int ch)
        {
            int memAddr = 0x0400;
            int memData = 0x01;
            byte[] buffer = new byte[1];

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                // Target Position Write 이외에는 1byte 씩만 Write 할 것.
                buffer[0] = (byte)(memData);
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                //MessageBox.Show("My " + ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bOccupied = false;
                return false;
            }

            return true;
        }
        public int CheckIfOISSTS_HALLPOLARITY(int ch, bool bSkip = false)
        {
            int memAddr = 0x0001;
            byte lToBe = 0x01;
            byte result = 0;

            result = RetryRead(ch, memAddr, lToBe, 10);

            Thread.Sleep(30);
            memAddr = 0x0400;
            lToBe = 0x00;
            result = RetryRead(ch, memAddr, lToBe, 10);

            Thread.Sleep(30);
            byte[] buffer = new byte[2];
            m_errMsg[ch] = "";
            try
            {
                memAddr = 0x0004;
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;

                if ((buffer[1] & 0x03) != 0)  // 0 bit 1 bit 가 모두 0 이어야 Pass
                {
                    if (!bSkip)
                    {
                        m_errMsg[ch] = "Error : Read 0x0004 0x" + buffer[1].ToString("x");
                        return -10;
                    }
                }
            }
            catch (Exception ex)
            {
                m_errMsg[ch] = "Error : IO Exception when reading 0x0004";
                m_bOccupied = false;
                //return -9;
            }

            byte[] sbuffer = new byte[1];

            memAddr = 0x0001;
            lToBe = 0x01;
            result = RetryRead(ch, memAddr, lToBe, 50);
            if (result != lToBe)
            {
                m_errMsg[ch] = "Error : 0x0001 " + result.ToString("X");
                return -6;
            }

            try
            {
                memAddr = 0x0405;
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, sbuffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;
                m_errMsg[ch] = "Not Error";
                return (int)(sbuffer[0]);
            }
            catch (Exception ex)
            {
                m_errMsg[ch] = "Error : IO Exception when reading 0x0405";
                m_bOccupied = false;
                return -5;
            }
        }

        public bool FWriteHallCalibration(int ch)
        {
            int memAddr = 0x0401;
            int memData = 0x01;
            byte[] buffer = new byte[1];

            try
            {
                buffer[0] = (byte)(memData);
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
            return true;
        }

        public bool WriteHallCalibration(int ch)
        {
            int memAddr = 0x0401;
            int memData = 0x01;
            byte[] buffer = new byte[1];

            try
            {
                buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
            return true;
        }

        public bool CheckIfOISSTS_HallCAL(int ch )
        {
            int memAddr = 0x0001;
            byte lToBe = 0x07;
            byte result = 0;
            result = RetryRead(ch, memAddr, lToBe, 50);
            if (result != lToBe) return false;
            return true;
        }

        public byte CheckIffinish_HALLCAL(int ch, int timeLimit = 200 )
        {
            int memAddr = 0x0401;
            byte lToBe = 0x00;
            byte result = 0;
            result = RetryRead(ch, memAddr, lToBe, timeLimit);
            if (result != lToBe) return result;

            return 0;
        }

        public int CheckOISERR(int ch)
        {
            int memAddr = 0x0004;
            int res = 0;
            byte[] buffer = new byte[2];
            Thread.Sleep(50);
            try
            {
                //  기존에 한번만 점검해서 Fail 로 종료했으나 반복하는 것으로 변경 ==> 반복하면 SrcIndex 가 Length 초과한다는 희안한 Error 발생으로 반복 삭제 20181123
                //for (int i = 0; i < 2; i++)
                //{
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                m_0x0004[ch] = 0x0;
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, buffer); //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 읽어들임.
                m_bOccupied = false;

                m_0x0004[ch] = buffer[0];

                if ((buffer[0] & 0x1) != 0)
                    res = 1;
                if ((buffer[0] & 0x2) != 0)
                    res = 2;
                if ((buffer[0] & 0x4) != 0)
                    res = 3;
                if ((buffer[0] & 0x8) != 0)
                    res = 4;
                //MessageBox.Show("0x0004 : " + buffer[0].ToString("X"));

                return res;
            }
            catch (Exception ex)
            {
                m_errMsg[ch] = "I2C Err during CheckOISERR : " + ex.Message;
                m_bOccupied = false;
                return -1;
            }
        }

        public void OISOn(int ch)
        {
            int memAddr;
            int memData;
            byte[] buffer = new byte[1];

            memAddr = 0x0000;
            memData = 0x01;
            try
            {
                buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                m_bOccupied = false;
                return;
            }

            //  Hall Read Enable
            memAddr = 0x0B00;
            memData = 0x01;
            try
            {
                buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                return;
            }

            Thread.Sleep(10);
            memAddr = 0x0001;
            //byte lToBe = 0x01;  
            byte lToBe = 0x02;
            byte result = 0;
            result = RetryRead(ch, memAddr, lToBe, 10);
            //MessageBox.Show("OIS On : 0x" + result.ToString("X"));


            return;
        }
        public void OISOff(int ch)
        {
            int memAddr;
            int memData;
            byte[] buffer = new byte[1];

            memAddr = 0x0000;
            memData = 0x00;
            try
            {
                buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return;
            }
            Thread.Sleep(30);
        }
        public bool WriteRun(int ch)
        {
            mDrvICchannel[ch] = true;
            int memAddr;
            int memData;
            byte[] buffer = new byte[1];
            byte[] buffer2 = new byte[2];

            memAddr = 0x0002;
            memData = 0x0B;
            try
            {
                buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                m_errMsg[ch] = "W 0x0002 0x0B " + ex.Message ;
                m_bOccupied = false;
                mDrvICchannel[ch] = false;
                return false;
            }

            //memAddr = 0x0001;
            byte lToBe = 0x02;
            byte result;

            memAddr = 0x0000;
            memData = 0x01;
            try
            {
                buffer[0] = 0x01;
                //buffer[0] = (byte)(memData);
                while (m_bOccupied)
                {
                    Thread.Sleep(1);
                }
                m_bOccupied = true;
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                Thread.Sleep(10);
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, buffer);    //  memLen 은 무조건 1, buffer 는 buffer.Length 만큼 자동으로 모조리 전송
                m_bOccupied = false;
            }

            catch (Exception ex)
            {
                m_errMsg[ch] = "W 0x0000 0x01 " + ex.Message;
                m_bOccupied = false;
                mDrvICchannel[ch] = false;
                return false;
            }

            //////////////////////////////////////////////////////////////////////////////////
            //  Read 0x02
            Thread.Sleep(200);  // 0x0000 에 0x01 을 쓰고 최소 100msec 이상 기다렸다가 다음커맨드를 실행해야 정상동작한다.
            memAddr = 0x0001;
            lToBe = 0x02;
            result = RetryRead(ch, memAddr, lToBe, 50);
            if (result == 0x02)
            {
                //MessageBox.Show("0x0001 is 0x02");
                return true;
            }
            else
            {
                m_errMsg[ch] = "0x0001 is not 0x02 but 0x" + result.ToString("X");
                return false;
            }
        }
        /// <summary>
        /// BU242
        /// </summary>
        public byte[] mBU242_FW1 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW2 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW3 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW4 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW5 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW6 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW7 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW8 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_FW9 = new byte[1024]; //  TBD, FW required
        public byte[] mBU242_CAL1 = new byte[1024]; //  TBD, FW required

        public bool WriteToBU242(int ch, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_BU242_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("WriteToBU242() : " + ex.Message);
                m_bOccupied = false;
                return false;
            }
        }


        public bool WriteToSEM(int ch, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
        }

        public bool WriteToSEM_1BYTE(int ch, int memAddr, byte data)
        {
            byte[] Temp = new byte[1];
            Temp[0] = data;
            return WriteToSEM(ch, memAddr, Temp);
        }

        public bool WriteToSEM_2BYTE(int ch, int memAddr, UInt16 SendData)
        {
            byte[] Temp = { 0, 0 };

            Temp[0] = (byte)(SendData & 0xff);             
            Temp[1] = (byte)((SendData >> 8) & 0xff);

            return WriteToSEM(ch, memAddr, Temp);
        }

        public bool WriteToSEM_4BYTE(int ch, int memAddr, int SendData)
        {
            byte[] Temp = { 0, 0, 0, 0 };

            Temp[0] = (byte)(SendData & 0xff);             // LSB
            Temp[1] = (byte)((SendData >> 8) & 0xff);
            Temp[2] = (byte)((SendData >> 16) & 0xff);
            Temp[3] = (byte)((SendData >> 24) & 0xff);     // MSB

            return WriteToSEM(ch, memAddr, Temp);
        }

        public bool ReadFromSEM(int ch, int memAddr, byte[] data)
        {
            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, memAddr, data);
                m_bOccupied = false;
                return true;
            }
            catch (Exception ex)
            {
                m_bOccupied = false;
                return false;
            }
        }

        public byte ReadFromSEM_1BYTE(int ch, int memAddr)
        {
            byte[] Temp = new byte[1];
            ReadFromSEM(ch, memAddr, Temp);
            return Temp[0];
        }

        public UInt16 ReadFromSEM_2BYTE(int ch, int memAddr)
        {
            byte[] Temp = new byte[2];
            ReadFromSEM(ch, memAddr, Temp);

            // return BitConverter.ToUInt16(Temp, 0);
            return (UInt16)((Byte)Temp[0] | ((Byte)Temp[1] << 8));
        }


        public bool[] mDrvICchannel = new bool[4] { false, false, false, false };


        ~AFDrvIC()
        {
            Library.DisconnectAll();
        }
        #region OIS_IDAC_RMS_Test & Hall Test
        public bool AF_Driver_On(int ch)
        {
            byte[] tmpbuf = new byte[1];
            tmpbuf[0] = 0x01;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, OIS_Variables.AF_CTRL, tmpbuf);
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, OIS_Variables.AF_STS, tmpbuf);

                m_bOccupied = false;

                if (tmpbuf[0] == 0x02)
                    return true;
                else
                    return false;

            }
            catch
            {
                m_bOccupied = false;
                return false;
            }

        }
        public bool AF_Driver_Off(int ch)
        {
            byte[] tmpbuf = new byte[1];
            tmpbuf[0] = 0x00;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, OIS_Variables.AF_CTRL, tmpbuf);
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, OIS_Variables.AF_STS, tmpbuf);

                m_bOccupied = false;

                if (tmpbuf[0] == 0x01)
                    return true;
                else
                    return false;

            }
            catch
            {
                m_bOccupied = false;
                return false;
            }

        }
        public bool OISMode_ServoOff(int ch)
        {

            byte[] tmpbuf = new byte[1];
            tmpbuf[0] = 0x00;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, OIS_Variables.OIS_CTRL, tmpbuf);
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, OIS_Variables.OIS_STS, tmpbuf);

                m_bOccupied = false;

                if (tmpbuf[0] == 0x01)
                    return true;
                else
                    return false;
            }
            catch
            {
                m_bOccupied = false;
                return false;
            }

        }
        public bool OISMode_ServoOn(int ch)
        {

            byte[] tmpbuf = new byte[1];
            tmpbuf[0] = 0x01;

            while (m_bOccupied)
            {
                Thread.Sleep(1);
            }
            m_bOccupied = true;
            try
            {
                DLNi2c[ch].Write(m_DrvIC_Addr, 2, OIS_Variables.OIS_CTRL, tmpbuf);
                DLNi2c[ch].Read(m_DrvIC_Addr, 2, OIS_Variables.OIS_STS, tmpbuf);

                m_bOccupied = false;

                if (tmpbuf[0] == 0x02)
                    return true;
                else
                    return false;
            }
            catch
            {
                m_bOccupied = false;
                return false;
            }
        }
        public void ReadAMVForHallTest(int ch)
        {
            byte[] tmpbuffer = new byte[128];

            DLNi2c[ch].Read(m_DrvIC_Addr, 2, 0xAE00, tmpbuffer);   //  190531  0xDF00 -> 0xBF00 으로 변경됨.
            Thread.Sleep(100);

            for (int i = 0; i < 128; i++)
            {
                OIS_Variables.ReadDriverICResultStatus[ch, i] = tmpbuffer[i];
            }

        }
        #endregion
    }

    public static class OIS_Variables
    {
        public const int AF_CTRL = 0x0200;
        public const int AF_STS = 0x0201;
        public const int OIS_CTRL = 0x0000;
        public const int OIS_STS = 0x0001;
        public static byte[ , ] ReadDriverICResultStatus = new byte[2, 256];
        public static bool[] HallTestResult = new bool[2];


        public const int OIS_MCSTH = 0x0A02;
        public const int OIS_MCSERRC = 0x0A04;
        public const int OIS_MCSFREQ = 0x0A05;
        public const int OIS_MCSDEG = 0x0A18;
        public const int OIS_MCSNUM = 0x0A08;
        public const int OIS_MCCTRL = 0x0A00;
        public const int OIS_MCERR = 0x0A01;
        public const int OIS_MCRTH = 0x0A09;
        public const int OIS_MCRERRTM = 0x0A0A;
        public const int OIS_MCRPOS = 0x0A0B;
        public const int OIS_MCLOG0 = 0x0A0C;
        public const int OIS_MCLOG1 = 0x0A0E;
        public const int OIS_MCLOG2 = 0x0A10;
        public const int OIS_MCLOG3 = 0x0A12;

        public const int TIMEOUT_AUTO_TEST_COMPLETE = 3000;
        public const int REGESTER_CHECK_INTERVAL = 100;


    }
    public static class ConstBU242
    {
        public const UInt16 CONTROL = 0x6020;// RW	1
        public const UInt16 MODE = 0x6021;// RW	1
        public const UInt16 HALL_ADJ_COND = 0x60A0;// RW	1
        public const UInt16 HALL_ADJ_STA = 0x60A1;// RW	1
        public const UInt16 STATUS = 0x6024;// R	1
        public const UInt16 ACT_MOVE_X = 0x6026;// RW	1
        public const UInt16 ACT_MOVE_Y = 0x6027;// RW	1
        public const UInt16 VCM_DRV_X = 0x6070;// W	2
        public const UInt16 VCM_DRV_Y = 0x6072;// W	2
        public const UInt16 HALL_X = 0x6058;// R	1
        public const UInt16 HALL_Y = 0x6059;// R	1
        public const UInt16 ADC_REQ = 0x6060;// W	1
        public const UInt16 ADC = 0x6062;// R	2
        public const UInt16 HALL_CURR_X_ADJ = 0x60A2;	// R	1
        public const UInt16 HALL_CURR_Y_ADJ = 0x60A3;	// R	1
        public const UInt16 Hall_OFS_X_DAC = 0x60A4;	// R	2
        public const UInt16 Hall_OFS_Y_DAC = 0x60A6;	// R	2
        public const byte IC_SETTING1_219 = 0x39;// Setting Value for Hall adjustment (160223 : 42 -> 33), hhoon181210
        public const int WAIT_STATUS = 3000;
        public const UInt16 AFTER_HallCAL = 0x6050;// RW	1
        // 18.08.12 BU24532 FW
        public const UInt16 FW1_STA = 0x0000;// W
        public const UInt16 FW2_STA = 0x8A40;// W
        public const UInt16 FW3_STA = 0x9000;// W
        public const UInt16 FW4_STA = 0x9400;// W
        public const UInt16 FW5_STA = 0x9800;// W
        public const UInt16 FW6_STA = 0x9C00;// W
        public const UInt16 FW7_STA = 0xA000;// W
        public const UInt16 FW8_STA = 0xA400;// W
        public const UInt16 FW9_STA = 0x1500;// W, to reduce transfer size
        public const UInt16 CAL1_STA = 0x8DC0;// W

        public const UInt16 COMPLETE_DL = 0xF006;	// W	1
        public const UInt16 SUM_CHECK = 0xF008;	// R	4
        public const UInt16 START_DN = 0xF010;   // W	1

        public const byte BU24219_GXW_Z_ID = 0x7C;
    }

    public struct sROHM_GLB_VAL
    {

        public Int16 curr_x, curr_y, dac_ofs_x, dac_ofs_y;
        public Int16 opc_ofs_x, opc_ofs_y;
        public Int16 srv_tun_x, srv_tun_y;
    }
    public class VariBU252
    {
        public Int16 X_Range_Max, X_Range_Min, Y_Range_Max, Y_Range_Min;
        public byte X_ACT_Max, X_ACT_Min;
        public byte Y_ACT_Max, Y_ACT_Min;
        public Int16 x_position, y_position;
        public byte axis, x, y;

        public Int16[] XYHallCenter = new Int16[2];
        public Int16[] XYHallMin = new Int16[2];
        public Int16[] XYHallMax = new Int16[2];

        public sROHM_GLB_VAL glb_val = new sROHM_GLB_VAL();

    }

public static class ConstRUMBA_S6
    {
        public const UInt16 OISCTRL					= 0x0000;	// 1 byte	OIS control Register
        public const UInt16 OISSTS					= 0x0001;	// 1 byte	OIS Status Register
        public const UInt16 OISMODE					= 0x0002;	// 1 byte	OIS Mode select Register
        public const UInt16 OISDATAWRITE			= 0x0003;	// 1 byte	OIS Data Writing control Register
        public const UInt16 OISERR					= 0x0004;	// 2 byte	OIS Error Register
        public const UInt16 FWUPERR					= 0x0006;	// 2 byte	F/WUpdate Error Status Register
        public const UInt16 FWUPCHKSUM				= 0x0008;	// 4 byte	F/WUpdate Check Sum Register
        public const UInt16 FWUPCTRL				= 0x000C;	// 1 byte	F/WUpdate run control Register
        public const UInt16 DFLSCTRL				= 0x000D;	// 1 byte	User Data Area Control Register
        public const UInt16 DFLSCMD					= 0x000E;	// 1 byte	User Data Area Command Register
        public const UInt16 DFLSSIZE_W				= 0x000F;	// 1 byte	User Data Area Read/Write Size Register
        public const UInt16 DFLSADR					= 0x0010;	// 2 byte	User Data Area Read/Write Address Register
        public const UInt16 HPCTRL					= 0x0012;	// 1 byte	Hall polarity check control Register
        public const UInt16 HCCTRL					= 0x0013;	// 1 byte	Hall Calibration control Register
        public const UInt16 GCCTRL					= 0x0014;	// 1 byte	Gyro Calibration control Register
        public const UInt16 GGACTRL                 = 0x0015;	// 1 byte	Gyro Gain coef calibration control Register

        public const UInt16 ADCSEL					= 0x0017;	// 1 byte	DSP AD Channel set Register
        public const UInt16 SINCTRL					= 0x0018;	// 1 byte	sine control Register
        public const UInt16 SINFREQ					= 0x0019;	// 1 byte	sine freq set Register
        public const UInt16 SINAMP					= 0x001A;	// 1 byte	sine amp set Register
        public const UInt16 SQCTRL					= 0x001B;	// 1 byte	rect control Register
        public const UInt16 SQFREQ					= 0x001C;	// 1 byte	rect freq set Register
        public const UInt16 SQAMP					= 0x001D;	// 1 byte	rect amp set Register
        public const UInt16 DFIXCTRL				= 0x001E;	// 1 byte	PWM Duty const Mode control Register
        public const UInt16 XPWMDUTY_M1				= 0x001F;	// 1 byte	XaxisVCM PWM Duty set Module#1 Register
        public const UInt16 YPWMDUTY_M1				= 0x0020;	// 1 byte	YaxisVCM PWM Duty set Module#1 Register
                                                            
        public const UInt16 XTARGET					= 0x0022;	// 2 byte	Xaxis const Mode Target set Register
        public const UInt16 YTARGET					= 0x0024;	// 2 byte	Yaxis const Mode Target set Register
                                                            
        public const UInt16 FLSWRTRESULT			= 0x0027;	// 1 byte	FlashROM Writing result check Register
                                                            
        public const UInt16 POWERCTRL				= 0x0030;	// 1 byte	low power status control Register
                                                            
        public const UInt16 HPOLAERRLVL				= 0x0032;	// 2 byte	Hall Polarity Error find Level set Register
                                                            
        public const UInt16 VCMCTRL					= 0x0035;	// 1 byte	VCM output set Register
        public const UInt16 PIDPARAMINIT			= 0x0036;	// 1 byte	PID Parameter init set Register
                                                            
        public const UInt16 CACTRL					= 0x0039;	// 1 byte	Center reset control Register
        public const UInt16 XPWMDUTY_M2				= 0x0040;	// 1 byte	Xaxis VCM PWM DUTY set Module#2 Register
        public const UInt16 YPWMDUTY_M2				= 0x0041;	// 1 byte	Yaxis VCM PWM DUTY set Module#2 Register
                                                            
        public const UInt16 CASTEP_M1				= 0x0048;	// 4 byte	Center reset coef Module#1 Register
        public const UInt16 CAOFSX_M1				= 0x004C;	// 2 byte	Xaxis Center reset Offset set Module#1 Register
        public const UInt16 CAOFSY_M1				= 0x004E;	// 2 byte	Yaxis Center reset Offset set Module#1 Register
        public const UInt16 MCCTRL					= 0x0050;	// 1 byte	ModuleCheck control Register
        public const UInt16 MCERR					= 0x0051;	// 1 byte	ModuleCheckErrorRegister
        public const UInt16 MCSTH_M1				= 0x0052;	// 1 byte	SinewaveCheckError decision Threshold set Module#1 Register
        public const UInt16 MCSERRC					= 0x0053;	// 1 byte	SinewaveCheckError decision Count set Register
        public const UInt16 MCSFREQ					= 0x0054;	// 1 byte	SinewaveCheck check freq set Register
        public const UInt16 MCSAMP					= 0x0055;	// 1 byte	SinewaveCheck check amp set Register
        public const UInt16 MCSSKIPNUM				= 0x0056;	// 1 byte	SinewaveCheck checkstartwaitset Register
        public const UInt16 MCSNUM					= 0x0057;	// 1 byte	SinewaveCheck check cycle set Register
        public const UInt16 MCRTH					= 0x0058;	// 1 byte	RingingCheckError decision Threshold set Register
        public const UInt16 MCRERRTM				= 0x0059;	// 1 byte	RingingCheckError decision time set Register
        public const UInt16 MCRPOS					= 0x005A;	// 1 byte	RingingCheck initial Lens position set Register
        public const UInt16 MCSTH_M2				= 0x005B;	// 1 byte	SinewaveCheckError decision limit set Module#2 Register
        public const UInt16 HALLTEMP_TEMP			= 0x005C;	// 2 byte	RUMBA internal temperature information Monitor Register
        public const UInt16 GGFADE					= 0x005E;	// 1 byte	GyroGainFade set Register
                                                            
        public const UInt16 MCSTHMUL				= 0x0060;	// 1 byte	SinewaveCheckError decision limit  rate set Register
                                                            
        public const UInt16 GSTEMP					= 0x0070;	// 2 byte	Gyro Sensor temperature  information Monitor Register
                                                            
        public const UInt16 IPCHECKSUM				= 0x007A;	// 2 byte	individual defference calibration value CheckSum Register
        public const UInt16 VDRINFO					= 0x007C;	// 4 byte	Vendor information Register
        public const UInt16 FWINFO_CTRL				= 0x0080;	// 1 byte	F/W inside information reset Register
        public const UInt16 TESTMON_CTRL			= 0x0081;	// 1 byte	Hall output control Register
        public const UInt16 GETGRX					= 0x0082;	// 2 byte	Xaxis Gyro output Data Register
        public const UInt16 GETGRY					= 0x0084;	// 2 byte	Yaxis Gyro output Data Register
        public const UInt16 GRX_M1					= 0x0086;	// 2 byte	Xaxis Gyro cal result Module#1 Register
        public const UInt16 GRY_M1					= 0x0088;	// 2 byte	Yaxis Gyro cal result Module#1 Register
        public const UInt16 XVCM_DUTY_M1			= 0x008A;	// 2 byte	XaxisVCM DUTY set valueMonitor Module#1 Register
        public const UInt16 YVCM_DUTY_M1			= 0x008C;	// 2 byte	YaxisVCM DUTY set valueMonitor Module#1 Register
        public const UInt16 HAX_OUT_M1				= 0x008E;	// 2 byte	Xaxis Hall Amp output Data Module#1 Register
        public const UInt16 HAY_OUT_M1				= 0x0090;	// 2 byte	Yaxis Hall Amp output Data Module#1 Register
        public const UInt16 EXT_AD					= 0x0092;	// 2 byte	outside AD input Data Register
        public const UInt16 CASTEP_M2				= 0x0094;	// 4 byte	Center reset coef Module#2 Register
        public const UInt16 CAOFSX_M2				= 0x0098;	// 2 byte	Xaxis Center reset Offset set Module#2 Register
        public const UInt16 CAOFSY_M2				= 0x009A;	// 2 byte	Yaxis Center reset Offset set Module#2 Register
        public const UInt16 VENDORID				= 0x009C;	// 2 byte	Vendor ID Register
                                                            
        public const UInt16 GRX_M2					= 0x00AC;	// 2 byte	Xaxis Gyro cal result Module#2 Register
        public const UInt16 GRY_M2					= 0x00AE;	// 2 byte	Yaxis Gyro cal result Module#2 Register
        public const UInt16 XVCM_DUTY_M2			= 0x00B0;	// 2 byte	XaxisVCM DUTY set valueMonitor Module#2 Register
        public const UInt16 YVCM_DUTY_M2			= 0x00B2;	// 2 byte	YaxisVCM DUTY set valueMonitor Module#2 Register
        public const UInt16 HAX_OUT_M2				= 0x00B4;	// 2 byte	Xaxis Hall Amp output Data Module#2 Register
        public const UInt16 HAY_OUT_M2				= 0x00B6;	// 2 byte	Yaxis Hall Amp output Data Module#2 Register
                                                            
        public const UInt16 OISSEL 					= 0x00BE;	// 1 byte	OIS Driver output select Register
        public const UInt16 GYROCALCEN				= 0x00BF;	// 1 byte	OIS suppression runcontrol Register
                                                            
        public const UInt16 LGMCRES0_M1				= 0x00C0;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 0 Module#1 Register
        public const UInt16 LGMCRES1_M1				= 0x00C2;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 1 Module#1 Register
        public const UInt16 LGMCRES2_M1				= 0x00C4;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 2 Module#1 Register
        public const UInt16 LGMCRES3_M1				= 0x00C6;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 3 Module#1 Register
        public const UInt16 LGTGX					= 0x00C8;	// 4 byte	LoopGain calibration Xaxis target Gain Register
        public const UInt16 LGTGY					= 0x00CC;	// 4 byte	LoopGain calibration Yaxis target Gain Register
                                                            
        public const UInt16 LGCTRL					= 0x00D0;	// 1 byte	LoopGain calibration control Register
                                                            
        public const UInt16 LGFREQ					= 0x00D1;	// 1 byte	LoopGain calibration freq set Register
        public const UInt16 LGNUM					= 0x00D2;	// 1 byte	LoopGain calibration action  times set Register
        public const UInt16 LGSKIPNUM				= 0x00D3;	// 1 byte	LoopGain calibration check start wait set Register
        public const UInt16 LGAMP					= 0x00D4;	// 2 byte	LoopGain calibration input amp set Register
        public const UInt16 AGMCTRL					= 0x00D6;	// 1 byte	ActuatorGain check control Register
        public const UInt16 AGMFREQ					= 0x00D7;	// 1 byte	ActuatorGain check freq set Register
        public const UInt16 AGMNUM					= 0x00D8;	// 1 byte	ActuatorGain check cycle set Register
        public const UInt16 AGMAMP					= 0x00D9;	// 1 byte	ActuatorGain check amp set Register
        public const UInt16 AGMOFSX					= 0x00DA;	// 1 byte	ActuatorGain check Xaxis Offset set Register
        public const UInt16 AGMOFSY					= 0x00DB;	// 1 byte	ActuatorGain check Yaxis Offset set Register
        public const UInt16 AGMMAX					= 0x00DC;	// 2 byte	ActuatorGain check max value Register
        public const UInt16 AGMMIN					= 0x00DE;	// 2 byte	ActuatorGain check min value Register
                                                            
        public const UInt16 LGMCRES0_M2				= 0x00E4;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 0 Module#2 Register
        public const UInt16 LGMCRES1_M2				= 0x00E6;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 1 Module#2 Register
        public const UInt16 LGMCRES2_M2				= 0x00E8;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 2 Module#2 Register
        public const UInt16 LGMCRES3_M2				= 0x00EA;	// 2 byte	LoopGain checkcalibration ModuleCheck action result 3 Module#2 Register
        public const UInt16 GSTLOG0					= 0x00EC;	// 2 byte	GyroSelfTestLog0Register
        public const UInt16 GSTLOG1					= 0x00EE;	// 2 byte	GyroSelfTestLog1Register
                                                            
        public const UInt16 GYROREGCTRL				= 0x00F4;	// 1 byte	Gyro Sensor Register Read control Register
        public const UInt16 GYROREGADR				= 0x00F5;	// 1 byte	Gyro Sensor Register Read Address set Register
        public const UInt16 GYROREGDATA				= 0x00F6;	// 1 byte	Gyro Sensor Register Read result Register
                                                            
        public const UInt16 HWVER					= 0x00F8;	// 4 byte	H/W VERSION information Register
        public const UInt16 FW_REVISION				= 0x00FC;	// 4 byte	F/W Revision Register
        public const UInt16 FLS_DATA				= 0x0100;	// 2 byte	56 CodeFlashDataBuffer
                                                            
                                                            
                //------------------------------------------;--------------------------------------------------------------------------
        public const UInt16 HPOLA_M1				= 0x0200;	// 1 byte	Hall polarity set Module#1 Register
        public const UInt16 HCRETRY					= 0x0201;	// 1 byte	Hall Calibration action  times set Register
        public const UInt16 HDRANGE_M1				= 0x0202;	// 2 byte	Hall Dynamic Range set Module#1 Register
        public const UInt16 XHMAXLMT_M1				= 0x0204;	// 2 byte	Xaxis Hall high limit value set Module#1 Register
        public const UInt16 XHMINLMT_M1				= 0x0206;	// 2 byte	Xaxis Hall low limit value set Module#1 Register
        public const UInt16 YHMAXLMT_M1				= 0x0208;	// 2 byte	Yaxis Hall high limit value set Module#1 Register
        public const UInt16 YHMINLMT_M1				= 0x020A;	// 2 byte	Yaxis Hall low limit value set Module#1 Register
        public const UInt16 XHGAIN_M1				= 0x020C;	// 1 byte	Xaxis Hall internal  Gain set Module#1 Register
        public const UInt16 YHGAIN_M1				= 0x020D;	// 1 byte	Yaxis Hall internal  Gain set Module#1 Register
        public const UInt16 XHOFS_M1				= 0x020E;	// 1 byte	Xaxis Hall Offset set Module#1 Register
        public const UInt16 YHOFS_M1				= 0x020F;	// 1 byte	Yaxis Hall Offset set Module#1 Register
        public const UInt16 XHG_M1					= 0x0210;	// 1 byte	Xaxis Hall Bias set Module#1 Register
        public const UInt16 YHG_M1					= 0x0211;	// 1 byte	Yaxis Hall Bias set Module#1 Register
        public const UInt16 XHMAX_M1				= 0x0212;	// 2 byte	Xaxis Hall max value set Module#1 Register
        public const UInt16 XHMIN_M1				= 0x0214;	// 2 byte	Xaxis Hall min value set Module#1 Register
        public const UInt16 YHMAX_M1				= 0x0216;	// 2 byte	Yaxis Hall max value set Module#1 Register
        public const UInt16 YHMIN_M1				= 0x0218;	// 2 byte	Yaxis Hall min value set Module#1 Register
        public const UInt16 XMECHACENTER_M1			= 0x021A;	// 2 byte	XaxisLens physical CenterpositionsetModule#1 Register
        public const UInt16 YMECHACENTER_M1			= 0x021C;	// 2 byte	YaxisLens physical CenterpositionsetModule#1 Register
        public const UInt16 REFCHECKSUM				= 0x021E;	// 2 byte	individual defference calibration value Reference CheckSum set Register
                                                            
        public const UInt16 XHCMAXLMT_M1			= 0x0230;	// 1 byte	Xaxis Hall Calibration moving range high limit valueset Module#1 Register
        public const UInt16 XHCMINLMT_M1			= 0x0231;	// 1 byte	Xaxis Hall Calibration moving range low limit valueset Module#1 Register
        public const UInt16 YHCMAXLMT_M1			= 0x0232;	// 1 byte	Yaxis Hall Calibration moving range high limit valueset Module#1 Register
        public const UInt16 YHCMINLMT_M1			= 0x0233;	// 1 byte	Yaxis Hall Calibration moving range low limit valueset Module#1 Register
                                                            
        public const UInt16 GGFADEUP				= 0x0238;	// 2 byte	GyroGainFadeUp direction Step set Register
        public const UInt16 GGFADEDOWN				= 0x023A;	// 2 byte	GyroGainFadeDown direction Step set Register
                                                            
        public const UInt16 GYRO_POLA_X_M1			= 0x0240;	// 1 byte	Xaxis GYRO polarity set Module#1 Register
        public const UInt16 GYRO_POLA_Y_M1			= 0x0241;	// 1 byte	Yaxis GYRO polarity set Module#1 Register
        public const UInt16 GYRO_ORIENT				= 0x0242;	// 1 byte	GYRO output Data directionset Register
        public const UInt16 GSCNUM					= 0x0243;	// 1 byte	Gyro Calibration Sample num set Register
        public const UInt16 GZEROMAXLMT				= 0x0244;	// 2 byte	Gyro origin high limit value set Register
        public const UInt16 GZEROMINLMT				= 0x0246;	// 2 byte	Gyro origin low limit value set Register
        public const UInt16 XGZERO					= 0x0248;	// 2 byte	Xaxis Gyro origin Offset set Register
        public const UInt16 YGZERO					= 0x024A;	// 2 byte	Yaxis Gyro origin Offset set Register
                                                            
        public const UInt16 XGG_M1					= 0x0254;	// 4 byte	Xaxis Gyro Gain coef set Module#1 Register
        public const UInt16 YGG_M1					= 0x0258;	// 4 byte	Yaxis Gyro Gain coef set Module#1 Register
                                                            
        public const UInt16 NCCTRL					= 0x026C;	// 1 byte	Noise Cancel control Register
        public const UInt16 NCSTEP					= 0x026D;	// 1 byte	Noise Cancel Step width set Register
        public const UInt16 NCINTERVAL				= 0x026E;	// 1 byte	Noise Cancel target position reset gap set Register
                                                            
        public const UInt16 XKP_M1					= 0x0270;	// 4 byte	Xaxis PID Filter P coef set Module#1 Register
        public const UInt16 XKI_M1					= 0x0274;	// 4 byte	Xaxis PID Filter I coef set Module#1 Register
        public const UInt16 XKD_M1					= 0x0278;	// 4 byte	Xaxis PID Filter D coef set Module#1 Register
        public const UInt16 X9B_M1					= 0x027C;	// 4 byte	Xaxis PID Filter Gain set Module#1 Register
        public const UInt16 XFLTAU_M1				= 0x0280;	// 4 byte	Xaxis Low Pass Filter AU coef set Module#1 Register
        public const UInt16 XFLTK_M1				= 0x0284;	// 4 byte	Xaxis Low Pass Filter K coef set Module#1 Register
        public const UInt16 YKP_M1					= 0x0288;	// 4 byte	Yaxis PID Filter P coef set Module#1 Register
        public const UInt16 YKI_M1					= 0x028C;	// 4 byte	Yaxis PID Filter I coef set Module#1 Register
        public const UInt16 YKD_M1					= 0x0290;	// 4 byte	Yaxis PID Filter D coef set Module#1 Register
        public const UInt16 Y9B_M1					= 0x0294;	// 4 byte	Yaxis PID Filter Gain set Module#1 Register
        public const UInt16 YFLTAU_M1				= 0x0298;	// 4 byte	Yaxis Low Pass Filter AU coef set Module#1 Register
        public const UInt16 YFLTK_M1				= 0x029C;	// 4 byte	Yaxis Low Pass Filter K coef set Module#1 Register
                                                            
        public const UInt16 OISDRIVEMODE			= 0x02D0;	// 4 byte	OIS act MODE Register
                                                            
        public const UInt16 AD2CTRL					= 0x02FE;	// 1 byte	AD2 control Register
                                                            
        public const UInt16 GYRO_COEF1_0			= 0x0300;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode0
        public const UInt16 GYRO_COEF2_0			= 0x0304;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode0
        public const UInt16 GYRO_COEF3_0			= 0x0308;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode0
        public const UInt16 GYRO_COEF4_0			= 0x030C;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode0
        public const UInt16 GYRO_COEF5_0			= 0x0310;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode0
        public const UInt16 GYRO_GGCOEF_0			= 0x0314;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode0
        public const UInt16 GYRO_GLCOEF_0			= 0x0318;	// 4 byte	Gyro Limit calibration set Register OIS Mode0
        public const UInt16 GYRO_PTCOEF_0			= 0x031C;	// 4 byte	PanTilt reset coef set Register OIS Mode0
        public const UInt16 GYRO_PTSTEP_0			= 0x0320;	// 4 byte	PanTilt reset Step set Register OIS Mode0
                                                            
        public const UInt16 GYRO_PTAREA2_0			= 0x0325;	// 1 byte	PanTilt reset Area2 set Register OIS Mode0
                                                            
        public const UInt16 GYRO_PTAREA3_0			= 0x0327;	// 1 byte	PanTilt reset Area3 set Register OIS Mode0
        public const UInt16 GYRO_SRL_0				= 0x0328;	// 4 byte	Gyro cal Slew Rate Limit set Register OIS Mode0
                                                            
        public const UInt16 GYRO_COEF1_1			= 0x0330;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode1
        public const UInt16 GYRO_COEF2_1			= 0x0334;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode1
        public const UInt16 GYRO_COEF3_1			= 0x0338;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode1
        public const UInt16 GYRO_COEF4_1			= 0x033C;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode1
        public const UInt16 GYRO_COEF5_1			= 0x0340;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode1
        public const UInt16 GYRO_GGCOEF_1			= 0x0344;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode1
        public const UInt16 GYRO_GLCOEF_1			= 0x0348;	// 4 byte	Gyro Limit calibration set Register OIS Mode1
        public const UInt16 GYRO_PTCOEF_1			= 0x034C;	// 4 byte	PanTilt reset coef set Register OIS Mode1
        public const UInt16 GYRO_PTSTEP_1			= 0x0350;	// 4 byte	PanTilt reset Step set Register OIS Mode1
                                                            
        public const UInt16 GYRO_PTAREA2_1			= 0x0355;	// 1 byte	PanTilt reset Area2 set Register OIS Mode1
                                                            
        public const UInt16 GYRO_PTAREA3_1			= 0x0357;	// 1 byte	PanTilt reset Area3 set Register OIS Mode1
        public const UInt16 GYRO_SRL_1				= 0x0358;	// 4 byte	Gyro Filter Slew Rate Limit set Register OIS Mode1
                                                            
        public const UInt16 GYRO_COEF1_2			= 0x0360;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode2
        public const UInt16 GYRO_COEF2_2			= 0x0364;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode2
        public const UInt16 GYRO_COEF3_2			= 0x0368;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode2
        public const UInt16 GYRO_COEF4_2			= 0x036C;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode2
        public const UInt16 GYRO_COEF5_2			= 0x0370;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode2
        public const UInt16 GYRO_GGCOEF_2			= 0x0374;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode2
        public const UInt16 GYRO_GLCOEF_2			= 0x0378;	// 4 byte	Gyro Limit calibration set Register OIS Mode2
        public const UInt16 GYRO_PTCOEF_2			= 0x037C;	// 4 byte	PanTilt reset coef set Register OIS Mode2
        public const UInt16 GYRO_PTSTEP_2			= 0x0380;	// 4 byte	PanTilt reset Step set Register OIS Mode2
                                                            
        public const UInt16 GYRO_PTAREA2_2			= 0x0385;	// 1 byte	PanTilt reset Area2 set Register OIS Mode2
                                                            
        public const UInt16 GYRO_PTAREA3_2			= 0x0387;   // 1 byte	PanTilt reset Area3 set Register OIS Mode2
        public const UInt16 GYRO_SRL_2				= 0x0388;	// 4 byte	Gyro Filter Slew Rate Limit set Register OIS Mode2
                                                            
        public const UInt16 GYRO_COEF1_3			= 0x0390;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode3
        public const UInt16 GYRO_COEF2_3			= 0x0394;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode3
        public const UInt16 GYRO_COEF3_3			= 0x0398;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode3
        public const UInt16 GYRO_COEF4_3			= 0x039C;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode3
        public const UInt16 GYRO_COEF5_3			= 0x03A0;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode3
        public const UInt16 GYRO_GGCOEF_3			= 0x03A4;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode3
        public const UInt16 GYRO_GLCOEF_3			= 0x03A8;	// 4 byte	Gyro Limit calibration set Register OIS Mode3
        public const UInt16 GYRO_PTCOEF_3			= 0x03AC;	// 4 byte	PanTilt reset coef set Register OIS Mode3
        public const UInt16 GYRO_PTSTEP_3			= 0x03B0;	// 4 byte	PanTilt reset Step set Register OIS Mode3
                                                            
        public const UInt16 GYRO_PTAREA2_3			= 0x03B5;	// 1 byte	PanTilt reset Area2 set Register OIS Mode3
                                                            
        public const UInt16 GYRO_PTAREA3_3			= 0x03B7;	// 1 byte	PanTilt reset Area3 set Register OIS Mode3
        public const UInt16 GYRO_SRL_3				= 0x03B8;	// 4 byte	Gyro Filter Slew Rate Limit set Register OIS Mode3
                                                            
        public const UInt16 GYRO_COEF1_4			= 0x03C0;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode4
        public const UInt16 GYRO_COEF2_4			= 0x03C4;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode4
        public const UInt16 GYRO_COEF3_4			= 0x03C8;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode4
        public const UInt16 GYRO_COEF4_4			= 0x03CC;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode4
        public const UInt16 GYRO_COEF5_4			= 0x03D0;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode4
        public const UInt16 GYRO_GGCOEF_4			= 0x03D4;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode4
        public const UInt16 GYRO_GLCOEF_4			= 0x03D8;	// 4 byte	Gyro Limit calibration set Register OIS Mode4
        public const UInt16 GYRO_PTCOEF_4			= 0x03DC;	// 4 byte	PanTilt reset coef set Register OIS Mode4
        public const UInt16 GYRO_PTSTEP_4			= 0x03E0;	// 4 byte	PanTilt reset Step set Register OIS Mode4
                                                            
        public const UInt16 GYRO_PTAREA2_4			= 0x03E5;	// 1 byte	PanTilt reset Area2 set Register OIS Mode4
                                                           
        public const UInt16 GYRO_PTAREA3_4			= 0x03E7;	// 1 byte	PanTilt reset Area3 set Register OIS Mode4
        public const UInt16 GYRO_SRL_4				= 0x03E8;	// 4 byte	Gyro Filter Slew Rate Limit set Register OIS Mode4
                                                            
        public const UInt16 HALLTEMP_COEF_OX1_M1	= 0x0478;	// 4 byte	Xaxis Hall Offset resetcoef 1 setModule#1 Register
        public const UInt16 HALLTEMP_COEF_OY1_M1	= 0x047C;	// 4 byte	Yaxis Hall Offset resetcoef 1 setModule#1 Register
        public const UInt16 HALLTEMP_COEF_OX2_M1	= 0x0480;	// 4 byte	Xaxis Hall Offset resetcoef 2 setModule#1 Register
        public const UInt16 HALLTEMP_COEF_OY2_M1	= 0x0484;	// 4 byte	Yaxis Hall Offset resetcoef 2 setModule#1 Register
                                                            
        public const UInt16 HALLTEMP_TEMPTH_X_M1	= 0x0490;	// 2 byte	Xaxis temperature coef set limit valuesetModule#1 Register
        public const UInt16 HALLTEMP_TEMPTH_Y_M1	= 0x0492;	// 2 byte	Yaxis temperature coef set limit valuesetModule#1 Register
        public const UInt16 HALLTEMP_COEF_OX1_M2	= 0x0494;	// 4 byte	Xaxis Hall Offset resetcoef 1 setModule#2 Register
        public const UInt16 HALLTEMP_COEF_OY1_M2	= 0x0498;	// 4 byte	Yaxis Hall Offset resetcoef 1 setModule#2 Register
        public const UInt16 HALLTEMP_COEF_OX2_M2	= 0x049C;	// 4 byte	Xaxis Hall Offset resetcoef 2 setModule#2 Register
        public const UInt16 HALLTEMP_COEF_OY2_M2	= 0x04A0;	// 4 byte	Yaxis Hall Offset resetcoef 2 setModule#2 Register
                                                            
        public const UInt16 HALLTEMP_TEMPTH_X_M2	= 0x04AC;	// 2 byte	Xaxis temperature coef set limit value setModule#2 Register
        public const UInt16 HALLTEMP_TEMPTH_Y_M2	= 0x04AE;	// 2 byte	Yaxis temperature coef set limit value setModule#2 Register
        public const UInt16 NTCHFLT1_XA0_M1			= 0x04B0;	// 4 byte	Xaxis NotchFilter1 A0 coef set Module#1 Register
        public const UInt16 NTCHFLT1_XA1_M1			= 0x04B4;	// 4 byte	Xaxis NotchFilter1 A1 coef set Module#1 Register
        public const UInt16 NTCHFLT1_XA2_M1			= 0x04B8;	// 4 byte	Xaxis NotchFilter1 A2 coef set Module#1 Register
        public const UInt16 NTCHFLT1_XB1_M1			= 0x04BC;	// 4 byte	Xaxis NotchFilter1 B1 coef set Module#1 Register
        public const UInt16 NTCHFLT1_XB2_M1			= 0x04C0;	// 4 byte	Xaxis NotchFilter1 B2 coef set Module#1 Register
        public const UInt16 NTCHFLT1_YA0_M1			= 0x04C4;	// 4 byte	Yaxis NotchFilter1 A0 coef set Module#1 Register
        public const UInt16 NTCHFLT1_YA1_M1			= 0x04C8;	// 4 byte	Yaxis NotchFilter1 A1 coef set Module#1 Register
        public const UInt16 NTCHFLT1_YA2_M1			= 0x04CC;	// 4 byte	Yaxis NotchFilter1 A2 coef set Module#1 Register
        public const UInt16 NTCHFLT1_YB1_M1			= 0x04D0;	// 4 byte	Yaxis NotchFilter1 B1 coef set Module#1 Register
        public const UInt16 NTCHFLT1_YB2_M1			= 0x04D4;	// 4 byte	Yaxis NotchFilter1 B2 coef set Module#1 Register
        public const UInt16 NTCHFLT2_XA0_M1			= 0x04D8;	// 4 byte	Xaxis NotchFilter2 A0 coef set Module#1 Register
        public const UInt16 NTCHFLT2_XA1_M1			= 0x04DC;	// 4 byte	Xaxis NotchFilter2 A1 coef set Module#1 Register
        public const UInt16 NTCHFLT2_XA2_M1			= 0x04E0;	// 4 byte	Xaxis NotchFilter2 A2 coef set Module#1 Register
        public const UInt16 NTCHFLT2_XB1_M1			= 0x04E4;	// 4 byte	Xaxis NotchFilter2 B1 coef set Module#1 Register
        public const UInt16 NTCHFLT2_XB2_M1			= 0x04E8;	// 4 byte	Xaxis NotchFilter2 B2 coef set Module#1 Register
        public const UInt16 NTCHFLT2_YA0_M1			= 0x04EC;	// 4 byte	Yaxis NotchFilter2 A0 coef set Module#1 Register
        public const UInt16 NTCHFLT2_YA1_M1			= 0x04F0;	// 4 byte	Yaxis NotchFilter2 A1 coef set Module#1 Register
        public const UInt16 NTCHFLT2_YA2_M1			= 0x04F4;	// 4 byte	Yaxis NotchFilter2 A2 coef set Module#1 Register
        public const UInt16 NTCHFLT2_YB1_M1			= 0x04F8;	// 4 byte	Yaxis NotchFilter2 B1 coef set Module#1 Register
        public const UInt16 NTCHFLT2_YB2_M1			= 0x04FC;	// 4 byte	Yaxis NotchFilter2 B2 coef set Module#1 Register
        public const UInt16 GYRO_COEF1_5			= 0x0500;	// 4 byte	Gyro Filter coef 1 set Register OIS Mode5
        public const UInt16 GYRO_COEF2_5			= 0x0504;	// 4 byte	Gyro Filter coef 2 set Register OIS Mode5
        public const UInt16 GYRO_COEF3_5			= 0x0508;	// 4 byte	Gyro Filter coef 3 set Register OIS Mode5
        public const UInt16 GYRO_COEF4_5			= 0x050C;	// 4 byte	Gyro Filter coef 4 set Register OIS Mode5
        public const UInt16 GYRO_COEF5_5			= 0x0510;	// 4 byte	Gyro Filter coef 5 set Register OIS Mode5
        public const UInt16 GYRO_GGCOEF_5			= 0x0514;	// 4 byte	Gyro Gain coef calibration set Register OIS Mode5
        public const UInt16 GYRO_GLCOEF_5			= 0x0518;	// 4 byte	Gyro Limit calibration set Register OIS Mode5
        public const UInt16 GYRO_PTCOEF_5			= 0x051C;	// 4 byte	PanTilt reset coef set Register OIS Mode5
        public const UInt16 GYRO_PTSTEP_5			= 0x0520;	// 4 byte	PanTilt reset Step set Register OIS Mode5
                                                            
        public const UInt16 GYRO_PTAREA2_5			= 0x0525;	// 1 byte	PanTilt reset Area2 set Register OIS Mode5
                                                            
        public const UInt16 GYRO_PTAREA3_5			= 0x0527;	// 1 byte	PanTilt reset Area3 set Register OIS Mode5
        public const UInt16 GYRO_SRL_5				= 0x0528;	// 4 byte	Gyro cal Slew Rate Limit set Register OIS Mode5
                //--------------------------------------------------------------------------------------------------------------------
        public const UInt16 HPOLA_M2				= 0x0530;	// 1 byte	Hall polarity set Module#2 Register
                                                            
        public const UInt16 HDRANGE_M2				= 0x0532;	// 2 byte	Hall Dynamic Range set Module#2 Register
        public const UInt16 XHMAXLMT_M2				= 0x0534;	// 2 byte	Xaxis Hall high limit value set Module#2 Register
        public const UInt16 XHMINLMT_M2				= 0x0536;	// 2 byte	Xaxis Hall low limit value set Module#2 Register
        public const UInt16 YHMAXLMT_M2				= 0x0538;	// 2 byte	Yaxis Hall high limit value set Module#2 Register
        public const UInt16 YHMINLMT_M2				= 0x053A;	// 2 byte	Yaxis Hall low limit value set Module#2 Register
        public const UInt16 XHGAIN_M2				= 0x053C;	// 1 byte	Xaxis Hall internal  Gain set Module#2 Register
        public const UInt16 YHGAIN_M2				= 0x053D;	// 1 byte	Yaxis Hall internal  Gain set Module#2 Register
        public const UInt16 XHOFS_M2				= 0x053E;	// 1 byte	Xaxis Hall Offset set Module#2 Register
        public const UInt16 YHOFS_M2				= 0x053F;	// 1 byte	Yaxis Hall Offset set Module#2 Register
        public const UInt16 XHG_M2					= 0x0540;	// 1 byte	Xaxis Hall Bias set Module#2 Register
        public const UInt16 YHG_M2					= 0x0541;	// 1 byte	Yaxis Hall Bias set Module#2 Register
        public const UInt16 XHMAX_M2				= 0x0542;	// 2 byte	Xaxis Hall max value set Module#2 Register
        public const UInt16 XHMIN_M2				= 0x0544;	// 2 byte	Xaxis Hall min value set Module#2 Register
        public const UInt16 YHMAX_M2				= 0x0546;	// 2 byte	Yaxis Hall max value set Module#2 Register
        public const UInt16 YHMIN_M2				= 0x0548;	// 2 byte	Yaxis Hall min value set Module#2 Register
        public const UInt16 XMECHACENTER_M2			= 0x054A;	// 2 byte	XaxisLens physical CenterpositionsetModule#2 Register
        public const UInt16 YMECHACENTER_M2			= 0x054C;	// 2 byte	YaxisLens physical CenterpositionsetModule#2 Register
        public const UInt16 XHCMAXLMT_M2			= 0x054E;	// 1 byte	Xaxis Hall Calibration set range high limit value set Module#2 Register
        public const UInt16 XHCMINLMT_M2			= 0x054F;	// 1 byte	Xaxis Hall Calibration set range low limit value set Module#2 Register
        public const UInt16 YHCMAXLMT_M2			= 0x0550;	// 1 byte	Yaxis Hall Calibration set range high limit value set Module#2 Register
        public const UInt16 YHCMINLMT_M2			= 0x0551;	// 1 byte	Yaxis Hall Calibration set range low limit value set Module#2 Register
        public const UInt16 GYRO_POLA_X_M2			= 0x0552;	// 1 byte	Xaxis GYRO polarity set Module#2 Register
        public const UInt16 GYRO_POLA_Y_M2			= 0x0553;	// 1 byte	Yaxis GYRO polarity set Module#2 Register
        public const UInt16 XGG_M2					= 0x0554;	// 4 byte	Xaxis Gyro Gain coef set Module#2 Register
        public const UInt16 YGG_M2					= 0x0558;	// 4 byte	Yaxis Gyro Gain coef set Module#2 Register
                                                            
        public const UInt16 XKP_M2					= 0x0560;	// 4 byte	Xaxis PID Filter P coef set Module#2 Register
        public const UInt16 XKI_M2					= 0x0564;	// 4 byte	Xaxis PID Filter I coef set Module#2 Register
        public const UInt16 XKD_M2					= 0x0568;	// 4 byte	Xaxis PID Filter D coef set Module#2 Register
        public const UInt16 X9B_M2					= 0x056C;	// 4 byte	Xaxis PID Filter Gain set Module#2 Register
        public const UInt16 XFLTAU_M2				= 0x0570;	// 4 byte	Xaxis Low Pass Filter AU coef set Module#2 Register
        public const UInt16 XFLTK_M2				= 0x0574;	// 4 byte	Xaxis Low Pass Filter K coef set Module#2 Register
        public const UInt16 YKP_M2					= 0x0578;	// 4 byte	Yaxis PID Filter P coef set Module#2 Register
        public const UInt16 YKI_M2					= 0x057C;	// 4 byte	Yaxis PID Filter I coef set Module#2 Register
        public const UInt16 YKD_M2					= 0x0580;	// 4 byte	Yaxis PID Filter D coef set Module#2 Register
        public const UInt16 Y9B_M2					= 0x0584;	// 4 byte	Yaxis PID Filter Gain set Module#2 Register
        public const UInt16 YFLTAU_M2				= 0x0588;	// 4 byte	Yaxis Low Pass Filter AU coef set Module#2 Register
        public const UInt16 YFLTK_M2				= 0x058C;	// 4 byte	Yaxis Low Pass Filter K coef set Module#2 Register
                                                            
        public const UInt16 NTCHFLT1_XA0_M2			= 0x05B0;	// 4 byte	Xaxis NotchFilter1 A0 coef set Module#2 Register
        public const UInt16 NTCHFLT1_XA1_M2			= 0x05B4;	// 4 byte	Xaxis NotchFilter1 A1 coef set Module#2 Register
        public const UInt16 NTCHFLT1_XA2_M2			= 0x05B8;	// 4 byte	Xaxis NotchFilter1 A2 coef set Module#2 Register
        public const UInt16 NTCHFLT1_XB1_M2			= 0x05BC;	// 4 byte	Xaxis NotchFilter1 B1 coef set Module#2 Register
        public const UInt16 NTCHFLT1_XB2_M2			= 0x05C0;	// 4 byte	Xaxis NotchFilter1 B2 coef set Module#2 Register
        public const UInt16 NTCHFLT1_YA0_M2			= 0x05C4;	// 4 byte	Yaxis NotchFilter1 A0 coef set Module#2 Register
        public const UInt16 NTCHFLT1_YA1_M2			= 0x05C8;	// 4 byte	Yaxis NotchFilter1 A1 coef set Module#2 Register
        public const UInt16 NTCHFLT1_YA2_M2			= 0x05CC;	// 4 byte	Yaxis NotchFilter1 A2 coef set Module#2 Register
        public const UInt16 NTCHFLT1_YB1_M2			= 0x05D0;	// 4 byte	Yaxis NotchFilter1 B1 coef set Module#2 Register
        public const UInt16 NTCHFLT1_YB2_M2			= 0x05D4;	// 4 byte	Yaxis NotchFilter1 B2 coef set Module#2 Register
        public const UInt16 NTCHFLT2_XA0_M2			= 0x05D8;	// 4 byte	Xaxis NotchFilter2 A0 coef set Module#2 Register
        public const UInt16 NTCHFLT2_XA1_M2			= 0x05DC;	// 4 byte	Xaxis NotchFilter2 A1 coef set Module#2 Register
        public const UInt16 NTCHFLT2_XA2_M2			= 0x05E0;	// 4 byte	Xaxis NotchFilter2 A2 coef set Module#2 Register
        public const UInt16 NTCHFLT2_XB1_M2			= 0x05E4;	// 4 byte	Xaxis NotchFilter2 B1 coef set Module#2 Register
        public const UInt16 NTCHFLT2_XB2_M2			= 0x05E8;	// 4 byte	Xaxis NotchFilter2 B2 coef set Module#2 Register
        public const UInt16 NTCHFLT2_YA0_M2			= 0x05EC;	// 4 byte	Yaxis NotchFilter2 A0 coef set Module#2 Register
        public const UInt16 NTCHFLT2_YA1_M2			= 0x05F0;	// 4 byte	Yaxis NotchFilter2 A1 coef set Module#2 Register
        public const UInt16 NTCHFLT2_YA2_M2			= 0x05F4;	// 4 byte	Yaxis NotchFilter2 A2 coef set Module#2 Register
        public const UInt16 NTCHFLT2_YB1_M2			= 0x05F8;	// 4 byte	Yaxis NotchFilter2 B1 coef set Module#2 Register
        public const UInt16 NTCHFLT2_YB2_M2         = 0x05FC;   // 4 byte	Yaxis NotchFilter2 B2 coef set Module#2 Register


        public const UInt16 OIS_AGING_GAP = 1000;		// center +- gap aging 2016.05.13 by ows
        public const UInt16 OIS_PORT = 2;			// Rumba S6 port select (1:Main_M1, 2:Sub_M2)
                                                     
        public const UInt16 RUMBA_PM_X_Freq		                = 0x32;		// 8bit Reg 0x00D1
        public const UInt16 RUMBA_PM_X_Num		                = 0x03;		// 8bit Reg 0x00D2
        public const UInt16 RUMBA_PM_X_SkipNum	                = 0x04;		// 8bit Reg 0x00D3
        public const UInt16 RUMBA_PM_X_Amp                      = 0x92;		// 16bit Reg 0x00D4
                                                     
        public const UInt16 RUMBA_PM_Y_Freq		                = 0x32;		// 8bit Reg 0x00D1
        public const UInt16 RUMBA_PM_Y_Num		                = 0x03;		// 8bit Reg 0x00D2
        public const UInt16 RUMBA_PM_Y_SkipNum	                = 0x04;		// 8bit Reg 0x00D3
        public const UInt16 RUMBA_PM_Y_Amp                      = 0x92;		// 16bit Reg 0x00D4

        public const UInt16 TIMEOUT_IDLE                        = 5000;		//	Idel Mode 진입		: 5000ms
        public const UInt16 TIMEOUT_HALLCAL                     = 100;			//	Hall Cal 진입		: 100ms
        public const UInt16 TIMEOUT_RUN                         = 100;			//	Hall Cal 진입		: 100ms
        public const UInt16 TIMEOUT_HALLCAL_COMPLETE            = 10000;		//	Hall Cal 완료		: 10000 ms
        public const UInt16 TIMEOUT_DATAWRITE                   = 150;			//	Regester 갱신		: 150 ms
        //public const UInt16 TIMEOUT_POLARITY_COMPLETE	= 150			//	Polarity Check 완료	: 150 ms
        public const UInt16 TIMEOUT_POLARITY_COMPLETE           = 1000;		//	Polarity Check 완료	: 1000 ms
        public const UInt16 TIMEOUT_AUTO_TEST_COMPLETE          = 3000;		//  auto test (sinewave, ringing) : 3000 ms
        public const UInt16 REGESTER_CHECK_INTERVAL             = 100;			//	Regester Check 주기	: 100us
    }

    public enum __CENTER_SELECT
    {
        Mecha = 0,
        Free
    }

    public enum __Flag
    {
        COMPLETE = 0,
        BUSY
    }

    public enum __SINCTRL
    {
        SINCTRL_XSINEN = 0x01,
        SINCTRL_YSINEN = 0x02,
    }

    public enum __OISSTS
    {
        OISSTS_INIT = 0,                //	초기상태
        OISSTS_IDLE_1,                  //	정지상태
        OISSTS_RUN_2,                       //	동작상태
        OISSTS_HALL_CALIBRATION,        //	Hall Calibration상태
        OISSTS_HALL_POLARITY,           //	Hall극성 확인 상태
        OISSTS_GYRO_CALIBRATION,        //	Gyro Calibration상태
        OISSTS_RESERVED_6,
        OISSTS_RESERVED_7,
        OISSTS_PWM_DUTY_FIXED,          //	PWM Duty고정 상태
        OISSTS_DFLS_UPDATE,             //	User Data 영역Update상태
        OISSTS_STNDBY,                  //	저소비전력 상태
        OISSTS_GYRO_COMM_CHECK,         // gyro 통신 이상 확인 상태
        OISSTS_RESERVED_12,
        OISSTS_ACT_GAIN_MEASURE,        //	Acautor gain 측정상태
        OISSTS_LOOP_GAIN_CTRL,          // loop gain 조정 상태
        OISSTS_RESERVED_15,
        OISSTS_GYRO_SELF_TEST,          // Gyroselftest 상태
        OISSTS_OISDATA_WRITE,           // ois data 영역 갱신 중 상태
        OISSTS_PIDPRAM_INIT,            // ois data 초기화 중 상태
        OISSTS_GYRO_WAKEUP_WAIT,        // gyro sensor 기동 대기 상태
        OISSTS_TIMEOUT_ERR = 0xFD,
        OISSTS_CAL_ERROR = 0xFE,
        OISSTS_IDLE_ERR = 0xFF
    }	// Reg 0x0001 161005 for rumba s6

    public enum __OISMODE
    {
        OISMODE_Still = 0,
        OISMODE_PanTilt,
        OISMODE_Fixed,                  // 고정 Mode
        OISMODE_SineWave,               // 정현파 Mode
        OISMODE_SquareWave,             // 구형파 Mode
        OISMODE_Centering,
        OISMODE_SineWave_LevelShift,    // 정현파 Level Shift
        OISMODE_SquareWave_LevelShift,  // 구형파 Level Shift
        OISMODE_0 = 16,
        OISMODE_1,
        OISMODE_2,
        OISMODE_3,
        OISMODE_4,
        OISMODE_5

    }// Reg 0x0002 161005 for rumba s6

    public enum __OISERR
    {
        OISERR_HCYERROR2 = 0x0800,      //	Hall Calibration Y축Dynamic Range 설정값 Error
        OISERR_HCXERROR2 = 0x0400,      //	Hall Calibration X축Dynamic Range 설정값 Error
        OISERR_CSERR = 0x0100,
        OISERR_GSLFERR = 0x0080,
        OISERR_GCOMERR = 0x0020,        //	Gyro sensor와의 SPI통신 Error
        OISERR_FLSERROIS = 0x0010,      //	OIS DATA SECTION 쓰기Error
        OISERR_HCYERROR1 = 0x0008,
        OISERR_HCXERROR1 = 0x0004,
        OISERR_GYZEROERR = 0x0002,      //	Gyro sensor의 Y축 출력값 Error
        OISERR_GXZEROERR = 0x0001,      //	Gyro sensor의 X축 출력값 Error
        OISERR_NO_ERROR = 0x0000        //	Gyro sensor의 이상없음
    }// Reg 0x0004-5 161005 for rumba s6

    public enum __HPCTRL
    {
        HPCTRL_ENABLE = 0x01,
        HPCTRL_COMPLETE = 0x00
    }   // Reg 0x0012 161006 for rumba s6

    public enum __OISSEL
    {
        OISSEL_OIS2 = 0x02,
        OISSEL_OIS1 = 0x01,
        OISSEL_NON = 0x00
    }// Reg 0x00BE 161006 for rumba s6

    public enum __HPOLA_M1
    {
        HPOLA_M1_POLAYERR = 0x08,           // Y축 Hall Polarity의Error 검출
        HPOLA_M1_POLAXERR = 0x04,           // X축 Hall Polarity의Error 검출
        HPOLA_M1_POLAY = 0x02,          // Y축의 극성 반전
        HPOLA_M1_POLAX = 0x01,          // X축의 극성 반전
        HPOLA_M1_NO_ERR = 0x00

    }// Reg 0x0200 161005 for rumba s6

    public enum __HPOLA_M2
    {
        HPOLA_M2_POLAYERR = 0x08,           // Y축 Hall Polarity의Error 검출
        HPOLA_M2_POLAXERR = 0x04,           // X축 Hall Polarity의Error 검출
        HPOLA_M2_POLAY = 0x02,          // Y축의 극성 반전
        HPOLA_M2_POLAX = 0x01,          // X축의 극성 반전
        HPOLA_M2_NO_ERR = 0x00
    }// Reg 0x0530 161005 for rumba s6
}
