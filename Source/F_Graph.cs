using System;
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
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Media;


//temp memo 170113 khkim
namespace CSH030Ex
{

    public class FGraph // : Form
    {
        private Global m__G = null;
        public F_Main MyOwner = null;
        public AFDrvIC mDriverIC = new AFDrvIC();

        delegate void UpdateUIDelegate();
        //double[] debugTmp = new double[320000];

        //-------------------------------------------------------------------------------------------------
        enum AI { CH1 = 0, CH2, CH3, CH4, CH5, CH6, CH7, CH8, CH9, CH10, CH11, CH12, CH13 };
        enum AO { CH1 = 0, CH2 };
        enum Port { P_LEFT = 0, P_RIGHT };

        const int CHANNEL_COUNT_MAX = 8;

        public const int LGMON_AMP_N_PHASE = 0x0C08;
        public const int LGCTRL = 0x0C00;
        public const int LG_FREQUENCY = 0x0C01;
        public const int LG_AMPLITUDE = 0x0C02;
        public const int LG_SKIPNUM = 0x0C03;
        public const int LG_TRYNUM = 0x0C04;
        public const byte LG_CTRL_MASK_ENABLE = 0x01;
        public const byte LOOPGAIN_X_AXIS = 0x01;
        public const byte LOOPGAIN_Y_AXIS = 0x11;
        public const int PID_CF_16_667_KHZ = 16667;
        public const int GAIN_PLUS = 1;
        public const int GAIN_MINUS = 0;
        public const int TEMP_INIT_VALUE = 1234;
        //public const int PHASE_MARGIN_NG_THRESHOLD = 50;
        public const int PHASE_MARGIN_NG_THRESHOLD = 30;
        public double[,] mGain10Hz = new double[4, 2];
        public long[] GrabT = new long[20000];
        public double mFPS = 0;
        public long mTimerFrequency = 0;
        public int mL12_FrameCount = 0;
        public int mAF_FrameCount = 0;
        public int mZoom_FrameCount = 0;

        // SZ3751 Dedicated
        public int REG_AFZM_CRTL = 0x0200;
        public int REG_AFZM_MODE = 0x0202;
        public int REG_AF_TARGET = 0x0204;
        public int REG_ZM_TARGET = 0x0206;
        public int REG_AF_EPA_CTRL = 0x020E;
        public int REG_ZM_EPA_CTRL = 0x020F;

        public int REG_AF_EPA_MAX = 0x0210;
        public int REG_AF_EPA_MIN = 0x0214;
        public int REG_ZM_EPA_MAX = 0x0212;
        public int REG_ZM_EPA_MIN = 0x0216;

        public int REG_NVM_CTRL = 0x0300;
        public int REG_LOG_SWITCH = 0x0B00;
        public int REG_AF_RAW_POSITION = 0x0B10;
        public int REG_ZM_RAW_POSITION = 0x0B12;
        enum DrvChannel
        {
            AF = 0,
            Zoom
        }

        public string mMsg = "";


        public struct stLoopGainResult
        {   //double *pPm, byte *pFreq, byte *pNGcode, double *pMaxDiffGain
            public bool status;
            public double phaseMargin;
            public double pmFreq;
            public double phaseMargin2nd;
            public double pmFreq2nd;
            public double maxDiffGain;
        }
        public stLoopGainResult[,] mloopGainResult_t = new stLoopGainResult[4, 20];
        //##############################################################################################
        //##########################   Analog Input/Output Buffer   ####################################
        //##############################################################################################

        // 입력 버퍼
        //public double[,] m_Hall_AF = new double[4, 17000];    // Ch 3
        //public double[,] m_Hall_ZM = new double[4, 17000];    // Ch 1
        //public double[,] m_HallAbs_AF = new double[4, 17000];    // Ch 3
        //public double[,] m_HallAbs_ZM = new double[4, 17000];    // Ch 1
        //public int[,] m_Hall_L2_Read = new int[2, 17000];    // Ch 3
        //public int[,] m_Hall_L3_Read = new int[2, 17000];    // Ch 3
        //public double[,] m_Cur_AF_Read = new double[2, 17000];    // Ch 3
        //public double[,] m_Cur_ZM_Read = new double[2, 17000];    // Ch 3

        public double[,] m_SensCurrent = new double[4, 17000];    // Ch 4
        //public double[,] m_AFStepHall = new double[4, 1000];    // Ch 4
        //public double[,] m_ZMStepHall = new double[4, 1000];    // Ch 4

        //// 출력 Waveform 저장 버퍼
        //public double[] m_bufWaveform_PreAF = new double[17000];
        //public double[] m_bufWaveform_PostAF = new double[17000];

        //public double[] m_bufWaveform_PreCLX = new double[17000];
        //public double[] m_bufWaveform_PostCLX = new double[17000];

        //public double[] m_bufWaveform_PreCLY = new double[17000];
        //public double[] m_bufWaveform_PostCLY = new double[17000];

        public int[] m_Waveform_AF = new int[17000];
        //public int[] m_Waveform_AFStep = new int[1000];

        public int[] m_Waveform_ZM = new int[17000];
        //public int[] m_Waveform_ZMStep = new int[1000];

        public float[,] mCrosstalkCoef = new float[4, 8];

        public Int32 daqDOstate;
        public Int32 daqDIstate;

        public int chk_state;
        //-------------------------------------------------------------------------------------------------
        // 아날로그 출력 버퍼
        private double[] m_dataScaled_AO = new double[2];

        // Graph
        //SimpleGraph[] m_simpleGraph = new SimpleGraph[CHANNEL_COUNT_MAX];
        //TimeUnit m_timeUnit;

        // 타이머 종료 이벤트
        public AutoResetEvent[] AnalogOut_DoneEvent = new AutoResetEvent[2] { null, null };//    new AutoResetEvent(false);
        public AutoResetEvent[] AnalogIn_DoneEvent = new AutoResetEvent[2] { null, null };//    new AutoResetEvent(false);

        // 타이머 생성
        public readonly MicroLibrary.MicroTimer[] microTimer_AO = new MicroLibrary.MicroTimer[2] { null, null };
        public readonly MicroLibrary.MicroTimer[] microTimer_AI = new MicroLibrary.MicroTimer[2] { null, null };

        // 타이머 카운트 및 출력 타입
        public int[] m_nCount_AI = new int[2];
        public int[] m_nCount_AO = new int[2];
        public int[] m_nMaxCount_AI = new int[2];
        public int[] m_nMaxCount_AO = new int[2];
        public int[] m_nAOType = new int[2];
        // 입출력 타입 
        public enum AO_TYPE { _NULL = 0, _AF, _STEPRESPONSE, _FRAX, _FRAY, _FRAINIT, _Reserved, _RumbaS6X, _RumbaS6Y, _S6XStep, _S6YStep, _DW9841_L2, _DW9841_L3, _DW9841_L2L3, _DW9841_L2Step, _DW9841_L3Step, _SEM_AF, _SEM_Zoom, _SEM_AFZoom, _SEM_AFStep, _SEM_ZoomStep };
        //public enum AO_TYPE { _NULL = 0, _AF, _STEPRESPONSE, _FRAX, _FRAY, _FRAINIT, _Reserved, _PreAF, _PostAF, _CLAF, _CLAFCAL, _CLAFStep, _CLX, _CLXStep, _CLY, _CLYStep, _CLXY, _CLXYCAL, _CLXYStep, _CLXYROI };

        // 출력 종료 Flag
        public bool m_bProcess_WaveformAFOut_OK = true;
        public bool[] m_bProcess_WaveformL2L3Out_OK = new bool[2] { true, true };
        public bool[] m_bProcess_WaveformCLXYCalOut_OK = new bool[4];
        public bool[] m_bProcess_WfmAFZoomStepOut_OK = new bool[4] { true, true, true, true };
        public bool m_bProcess_WaveformXStepOut_OK = true;
        public bool m_bProcess_WaveformYStepOut_OK = true;
        public bool m_bProcess_WaveformOISOut_OK = true;
        public bool[] m_bProcess_PhaseMargin_OK = new bool[4];
        public bool[] m_bProcess_FRAAging_OK = new bool[4];
        public bool[] m_bProcess_CrossTalk_OK = new bool[4];
        public bool[] m_bProcess_Aging_OK = new bool[4];
        public bool[] m_bProcess_FluxCal_OK = new bool[4];
        public bool[] m_bProcess_MarginCal_OK = new bool[4];
        public bool[] m_bProcess_RunUserScript_OK = new bool[4];

        // 시간 측정 변수
        //public long m_dStartTime;

        //public long[] m_CLXYOutTime = new long[10000];

        //public long[,] m_OutTimeAF = new long[2, 10000];
        //public long[,] m_AFStepOutTime = new long[2, 10000];

        //public long[,] m_OutTimeZM = new long[2, 10000];
        //public long[,] m_ZoomStepOutTime = new long[2, 10000];

        //public long m_CLAFPeakTime;
        public long[] m_AFPeakTime = new long[2];
        public long[] m_ZMPeakTime = new long[2];

        public long m_dEndTime;
        //double m_dElapsedTime;


        //----------------------------------------------------------- khkim 변수 start ---------------------------------------
        public bool State_L_Lens_Release = false;               // 왼쪽 더미렌즈 
        public bool State_L_Lens_Picking = false;
        public bool State_R_Lens_Release = false;
        public bool State_R_Lens_Picking = false;
        public bool State_Picker_Down = false;
        public bool State_Actuator_Connect = false;
        public bool State_Loading = false;
        public bool State_Unloading = false;
        public bool State_Rotation = false;

        public bool Sensor_Init_Stop = false;
        public bool Sensor_End_Stop = false;
        public bool Sensor_Actuator_Connect = false;
        public bool Sensor_Picker_Down = false;
        public bool Sensor_Loading = false;
        public bool Sensor_Unloading = false;

        public int mSeqIndex = 0;
        public int mSeqStartIndex = 0;
        public int mSeqFinishIndex = 0;
        public int mbDir = 1;
        public bool mSeqStopped = false;
        public bool mSeqStop = false;

        public bool startSW_push = false;
        public bool flag_startSW_push = false;
        public bool Stage_Initialize_done = false;
        public bool[] mPortIdle = new bool[2] { true, true };

        public int r_index = 1;                 // (무한)반복횟수 변수 선언
        public bool infinity_test_flag = false; // 무한반복 테스트
        public bool testing_done = false;       // 테스트 완료 변수

        //public int senario_cnt = 43;            // 총 시나리오 수 
        public int senario_cnt = 25;            // 총 시나리오 수 
        public bool closing_flag = false;       // 테스트 완료 변수
        public bool initializing_stop = false;       // 테스트 완료 변수

        public byte[] mXYPosBkData = new byte[4];

        public bool BeforeSafetySensorOnoff = true;  //hch
        public bool SafetySensorOnOff = true; //hch
        public byte m_crlDftFlag = 0;

        public int[] mNGCode_EPA = new int[4];
        public int[] mNGCode_LinearityComp = new int[4];

        public double[] rEpaBefore = new double[4] { 0.0, 0.0, 0.0, 0.0 };
        public double[] lEpaBefore = new double[4] { 0.0, 0.0, 0.0, 0.0 };

        public double[] rEpaAfter = new double[4] { 0.0, 0.0, 0.0, 0.0 };
        public double[] lEpaAfter = new double[4] { 0.0, 0.0, 0.0, 0.0 };

        public double[] rEpaInitial = new double[4] { 0.0, 0.0, 0.0, 0.0 };
        public double[] lEpaInitial = new double[4] { 0.0, 0.0, 0.0, 0.0 };

        [StructLayout(LayoutKind.Sequential)]
        public struct SemcoDataInfo
        {
            [MarshalAs(UnmanagedType.R8)]
            public double x0;
            [MarshalAs(UnmanagedType.R8)]
            public double x1;
            [MarshalAs(UnmanagedType.R8)]
            public double x2;
            [MarshalAs(UnmanagedType.R8)]
            public double x3;
            [MarshalAs(UnmanagedType.R8)]
            public double x4;
            [MarshalAs(UnmanagedType.R8)]
            public double x5;
        }

        public SemcoDataInfo SemcoDI;


        [DllImport("Semco_DLL_CSharp.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static int Semco_CalcCompensation_AF(double[] lpHallInput, double[] lpImageInput, int nInputNum, ref SemcoDataInfo lpDataOutput);
        [DllImport("Semco_DLL_CSharp.dll", CallingConvention = CallingConvention.Cdecl)]
        extern public static int Semco_CalcCompensation_Zoom(double[] lpHallInput, double[] lpImageInput, int nInputNum, ref SemcoDataInfo lpDataOutput);

        //----------------------------------------------------------- khkim 변수 end ---------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public FGraph()
        {
            //InitializeComponent();

            // Initial Analog INPUT/OUTPUT
            //MessageBox.Show("Camera Off->On");

            microTimer_AO[0] = new MicroLibrary.MicroTimer();
            microTimer_AO[0].MicroTimerElapsed += new MicroLibrary.MicroTimer.MicroTimerElapsedEventHandler(OnTimedEvent_AO_L);

            microTimer_AI[0] = new MicroLibrary.MicroTimer();
            microTimer_AI[0].MicroTimerElapsed += new MicroLibrary.MicroTimer.MicroTimerElapsedEventHandler(OnTimedEvent_AI_L);

            AnalogOut_DoneEvent[0] = new AutoResetEvent(false);
            AnalogIn_DoneEvent[0] = new AutoResetEvent(false);

        }

        public void InitFGraph()
        {
            //  카메라 전원을 제어하는 경우

            //CamPower_Release();
            //Thread.Sleep(300);

            //CamPower_Connect();
            //Thread.Sleep(2000);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        ~FGraph()
        {
            // Stop the timer, wait for up to 1 sec for current event to finish,
            //  if it does not finish within this time abort the timer thread

            //MessageBox.Show("프로그램을 종료합니다. 잠시만 기다려주세요");
            safe_closing();     //khkim 장비 안전종료 - 피커 올리고, 로테이션 원위치, 

            for (int i = 0; i < 1; i++)
            {
                if (!microTimer_AO[i].StopAndWait(1000))
                {
                    microTimer_AO[i].Abort();
                }

                if (!microTimer_AI[i].StopAndWait(1000))
                {
                    microTimer_AI[i].Abort();
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void StartMonitor()
        {
            m__G = Global.GetInstance();

            Thread Thread_MinotorDIO = new Thread(() => MonitorDIO());
            Thread_MinotorDIO.Start();
            mDriverIC.MyOwner = this;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void ClearWaveformBuffer(int port)
        {
            for (int i = 0; i < 17000; i++)
            {
                m_SensCurrent[2 * port, i] = 0;
                m_SensCurrent[2 * port + 1, i] = 0;
            }
            //for(int i=0; i<MAX_POINT_BUF; i++)
            //{
            //    //m_bufWaveform_AF[i]       = 2.5;      
            //    //m_bufWaveform_OISX[i] =2.5;
            //    //m_bufWaveform_OISY[i] = 2.5;

            //    //m_bufWaveform_FRAX[i]     = 2.5;    
            //    //m_bufWaveform_FRAY[i]     = 2.5;
            //    //m_bufWaveform_STEPRESPONSE[i] = 2.5;
            //}

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------   DO   ----------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Init_InstantDO()
        {
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DO_Write(int port, byte value)
        {
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void DO_Read(int port, ref byte data)
        {
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------- Analog Input : Advantek AI  ----------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Init_InstantAI()
        {
            //The default device of project is demo device, users can choose other devices according to their needs. 
            // 입력 타이머 셋팅
            microTimer_AI[0].Interval = 2000;    //  1kHz
            microTimer_AI[0].IgnoreEventIfLateBy = 200;
            //microTimer_AI[1].Interval = 2000;    //  1kHz
            //microTimer_AI[1].IgnoreEventIfLateBy = 200;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnTimedEvent_AI_L(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        {
            OnTimedEvent_AI(0);
        }
        private void OnTimedEvent_AI_R(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        {
            //OnTimedEvent_AI(1);
        }
        private void OnTimedEvent_AO_L(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        {

            OnTimedEvent_AO(0);
        }
        private void OnTimedEvent_AO_R(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        {
            //OnTimedEvent_AO(1);
        }

        private void OnTimedEvent_AI(int port)
        {
            if (m_nCount_AI[port] >= m_nMaxCount_AI[port])
            {
                microTimer_AI[port].Enabled = false;
                AnalogIn_DoneEvent[port].Set();
                return;
            }

            long CurTime = 0;
            SupremeTimer.QueryPerformanceCounter(ref CurTime);
            switch (m_nAOType[port])
            {
                case (int)(AO_TYPE._RumbaS6X):
                case (int)(AO_TYPE._DW9841_L2L3):
                case (int)(AO_TYPE._DW9841_L2):
                case (int)(AO_TYPE._SEM_AFZoom):
                case (int)(AO_TYPE._SEM_AF):
                case (int)(AO_TYPE._RumbaS6Y):
                case (int)(AO_TYPE._DW9841_L3):
                case (int)(AO_TYPE._SEM_Zoom):
                default:
                    break;
            }
            int ch = port * 2;

            if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
            {
                m_SensCurrent[ch, m_nCount_AI[port]] = mDriverIC.GetCurrent(ch);   //Current Sensing,  unit : mA
                if (m_nCount_AI[port] > 0)
                    m_SensCurrent[ch, m_nCount_AI[port]] = m_SensCurrent[ch, m_nCount_AI[port]] > 300 ? m_SensCurrent[ch, m_nCount_AI[port] - 1] : m_SensCurrent[ch, m_nCount_AI[port]];
            }
            if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch+1])
            {
                m_SensCurrent[ch + 1, m_nCount_AI[port]] = mDriverIC.GetCurrent(ch + 1);   //Current Sensing,  unit : mA
                if (m_nCount_AI[port] > 0)
                    m_SensCurrent[ch + 1, m_nCount_AI[port]] = m_SensCurrent[ch + 1, m_nCount_AI[port]] > 300 ? m_SensCurrent[ch + 1, m_nCount_AI[port] - 1] : m_SensCurrent[ch + 1, m_nCount_AI[port]];
            }

            m_nCount_AI[port]++;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------- Analog Output : Advantek AO  ----------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void Init_InstantAO()
        {
            microTimer_AO[0].Interval = 10000;    //  1kHz
            microTimer_AO[0].IgnoreEventIfLateBy = 2000;
            //microTimer_AO[1].Interval = 10000;    //  1kHz
            //microTimer_AO[1].IgnoreEventIfLateBy = 2000;
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------
        //###################   아날로그 출력   타이머 #######################################
        //private void OnTimedEvent_AO(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        private void OnTimedEvent_AO(int port)
        {
            int ch = port * 2;
            byte[] wbuffer = new byte[2];
            byte[] rbuf2 = new byte[2];
            byte[] rbuf4 = new byte[4];
            byte[] wbuf2 = new byte[2];
            byte[] wbuf = new byte[1];
            byte[] rbuffer = new byte[4];
            byte[] r2buffer = new byte[4];
            byte[] rbuffer_c = new byte[2];
            byte[] r2buffer_c = new byte[2];
            int[] iHallPosX = new int[2];
            int[] iHallPosY = new int[2];
            //UInt32 ldataL2;
            long CurTime = 0;
            long CurTime2 = 0;
            //double lwait = 0;
            double res = 0;

            //if (m_nCount_AO[port] == 0)
            //    SupremeTimer.QueryPerformanceCounter(ref m_dStartTime);
            SupremeTimer.QueryPerformanceCounter(ref CurTime);

            if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
            {
                if (m_nCount_AO[port] > 0)
                {
                    m_SensCurrent[ch, m_nCount_AO[port]] = mDriverIC.GetCurrent(ch);   //Current Sensing,  unit : mA
                    //if (m_SensCurrent[ch, m_nCount_AO[port]]<0.15)
                    //    m_SensCurrent[ch, m_nCount_AO[port]] = mDriverIC.GetCurrent(ch);   //Current Sensing,  unit : mA

                    //if (m_nCount_AO[port] == 2)
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025B, 0x01);   //  Free Zoom
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025B, 0x01);   //  Free Zoom
                    //}

                    //if (m_nCount_AO[port] == (m_nMaxCount_AO[port] / 2 - 1))
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025B, 0x00);   //  Active Zoom
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025B, 0x00);   //  Active Zoom
                    //}

                    //if (m_nCount_AO[port] == (m_nMaxCount_AO[port] / 2 + 2))
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025A, 0x00);   //  Free AF
                    //    mDriverIC.WriteToSEM_1BYTE(ch, 0x025A, 0x00);   //  Free AF
                    //}

                    if (m_nCount_AI[port] > 0)
                        m_SensCurrent[ch, m_nCount_AO[port]] = m_SensCurrent[ch, m_nCount_AO[port]] > 300 ? m_SensCurrent[ch, m_nCount_AO[port] - 1] : m_SensCurrent[ch, m_nCount_AO[port]];

                    for (int i = 0; i < 50000; i++)   //  ~ 220us delay
                        res = Math.Abs(Math.Sin(i));

                }
            }

            if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch+1])
            {
                if (m_nCount_AO[port] > 0)
                {
                    m_SensCurrent[ch + 1, m_nCount_AO[port]] = mDriverIC.GetCurrent(ch + 1);   //Current Sensing,  unit : mA
                    //if (m_SensCurrent[ch + 1, m_nCount_AO[port]] < 0.15)
                    //    m_SensCurrent[ch + 1, m_nCount_AO[port]] = mDriverIC.GetCurrent(ch + 1);   //Current Sensing,  unit : mA

                    //if (m_nCount_AO[port] == 2)
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025B, 0x01);   //  Free Zoom
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025B, 0x01);   //  Free Zoom
                    //}

                    //if (m_nCount_AO[port] == (m_nMaxCount_AO[port] / 2 - 1))
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025B, 0x00);   //  Active Zoom
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025B, 0x00);   //  Active Zoom
                    //}

                    //if (m_nCount_AO[port] == (m_nMaxCount_AO[port] / 2 + 2))
                    //{
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025A, 0x00);   //  Free AF
                    //    mDriverIC.WriteToSEM_1BYTE(ch + 1, 0x025A, 0x00);   //  Free AF
                    //}

                    if (m_nCount_AI[port] > 0)
                        m_SensCurrent[ch + 1, m_nCount_AO[port]] = m_SensCurrent[ch + 1, m_nCount_AO[port]] > 300 ? m_SensCurrent[ch + 1, m_nCount_AO[port] - 1] : m_SensCurrent[ch + 1, m_nCount_AO[port]];

                    for (int i = 0; i < 50000; i++)   //  ~ 440us delay
                        res = Math.Abs(Math.Sin(i));

                    //ldataL2 = 0;
                    //ldataL2 = mDriverIC.GetAFZMLensPos_SEM(ch + 1);

                    //m__G.fGraph.m_Hall_AF[ch + 1, m_nCount_AO[port] - 1] = (ushort)(ldataL2 >> 16);    //  상
                    //m__G.fGraph.m_Hall_ZM[ch + 1, m_nCount_AO[port] - 1] = (ushort)(ldataL2);
                    //m_Hall_AF[ch + 1, m_nCount_AO[port] - 1] = mDriverIC.GetAFLensPos_SEM(ch + 1);
                    //m_Hall_ZM[ch + 1, m_nCount_AO[port] - 1] = mDriverIC.GetZoomLensPos_SEM(ch + 1);
                    //m_HallAbs_AF[ch + 1, m_nCount_AO[port] - 1] = mDriverIC.ReadFromSEM_4BYTE(ch + 1, 0x120C);
                    //m_HallAbs_ZM[ch + 1, m_nCount_AO[port] - 1] = mDriverIC.ReadFromSEM_4BYTE(ch + 1, 0x1210);
                }
            }

            if (m_nCount_AO[port] >= m_nMaxCount_AO[port])
            {
                m__G.oCam[0].GrabB_User(m_nCount_AO[port] - 1);
                mDriverIC.WriteToSEM_1BYTE(0, 0x025A, 0x00);   //  Active AF
                mDriverIC.WriteToSEM_1BYTE(0, 0x025B, 0x00);   //  Active AF
                if (m__G.mCamCount > 1)
                {
                    m__G.oCam[1].GrabB_User(m_nCount_AO[port] - 1);
                    mDriverIC.WriteToSEM_1BYTE(1, 0x025A, 0x00);   //  Active AF
                    mDriverIC.WriteToSEM_1BYTE(1, 0x025B, 0x00);   //  Active AF
                }


                SupremeTimer.QueryPerformanceCounter(ref CurTime2);
                mFPS = 1010 + ((CurTime2 - CurTime) % 90) / 10.0;
                microTimer_AO[port].Enabled = false;
                AnalogOut_DoneEvent[port].Set();
                return;
            }

            switch (m_nAOType[port])
            {
                case (int)AO_TYPE._SEM_AFStep:

                    //if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
                    //{
                    //    mDriverIC.AFLensMove_SEM(ch, (UInt16)m_Waveform_AFStep[m_nCount_AO[port]]);
                    //    m__G.fManage.AddOperatorLog(ch, "AF step to : " + m_Waveform_AFStep[m_nCount_AO[port]].ToString());
                    //}
                    //if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch+1])
                    //{
                    //    mDriverIC.AFLensMove_SEM(ch + 1, (UInt16)m_Waveform_AFStep[m_nCount_AO[port]]);
                    //    m__G.fManage.AddOperatorLog(ch + 1, "AF step to : " + m_Waveform_AFStep[m_nCount_AO[port]].ToString());
                    //}
                    //SupremeTimer.QueryPerformanceCounter(ref CurTime);
                    //m_AFStepOutTime[port, m_nCount_AO[port]] = CurTime;
                    break;

                case (int)AO_TYPE._SEM_ZoomStep:
                    //if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
                    //{
                    //    mDriverIC.ZoomLensMove_SEM(ch, (UInt16)m_Waveform_ZMStep[m_nCount_AO[port]]);
                    //    m__G.fManage.AddOperatorLog(ch, "Zoom step to : " + m_Waveform_ZMStep[m_nCount_AO[port]].ToString());
                    //}
                    //if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch+1])
                    //{
                    //    mDriverIC.ZoomLensMove_SEM(ch + 1, (UInt16)m_Waveform_ZMStep[m_nCount_AO[port]]);
                    //    m__G.fManage.AddOperatorLog(ch + 1, "Zoom step to : " + m_Waveform_ZMStep[m_nCount_AO[port]].ToString());
                    //}
                    //SupremeTimer.QueryPerformanceCounter(ref CurTime);
                    //m_ZoomStepOutTime[port, m_nCount_AO[port]] = CurTime;
                    break;

                case (int)AO_TYPE._SEM_AFZoom:
                    //if (m_nCount_AO[port] > 0)
                    //{
                    //    //  1msec wait for Settling LED Power
                    //    m__G.oCam[0].GrabB_User(m_nCount_AO[port] - 1);

                    //    if (m__G.mCamCount > 1)
                    //        m__G.oCam[1].GrabB_User(m_nCount_AO[port] - 1);
                    //    //  Turn Off LED for Heat Dissipation
                    //}
                    //if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
                    //{
                       
                    //    if(!mDriverIC.AFLensMove_SEM(ch, (UInt16)m_Waveform_AF[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch, "AF LensMove I2C Fail", false);
                    //    }

                    //    if(!mDriverIC.ZoomLensMove_SEM(ch, (UInt16)m_Waveform_ZM[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch, "ZM LensMove I2C Fail", false);
                    //    }
                    //    //m__G.fManage.AddOperatorLog(ch, "Target : " + m_Waveform_AF[m_nCount_AO[port]].ToString("X4"));
                    //}
                    //if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch+1])
                    //{
                    //    if (!mDriverIC.AFLensMove_SEM(ch + 1, (UInt16)m_Waveform_AF[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch + 1, "AF LensMove I2C Fail", false);
                    //    }
                     
                    //    if(!mDriverIC.ZoomLensMove_SEM(ch + 1, (UInt16)m_Waveform_ZM[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch + 1, "ZM LensMove I2C Fail", false);
                    //    }
                    //    //m__G.fManage.AddOperatorLog(ch + 1, "Target : " + m_Waveform_AF[m_nCount_AO[port]].ToString("X4"));
                    //}
                    ////mDriverIC.SetLEDpowers((int)(m__G.sRecipe.iLEDcurrentLL * 500), (int)(m__G.sRecipe.iLEDcurrentLR * 500));

                    //SupremeTimer.QueryPerformanceCounter(ref CurTime);
                    //m_OutTimeAF[port, m_nCount_AO[port]] = CurTime;
                    //if (m_nCount_AO[port] > (m_nMaxCount_AO[port] / 2))
                    //    m_AFPeakTime[port] = m_OutTimeAF[port, m_nCount_AO[port] - 1] + m__G.TimerFrequency / 500;

                    break;

                case (int)AO_TYPE._SEM_AF:
                    //if (m_nCount_AO[port] > 0)
                    //{
                    //    //  1msec wait for Settling LED Power
                    //    m__G.oCam[0].GrabB_User(m_nCount_AO[port] - 1);

                    //    if (m__G.mCamCount > 1)
                    //        m__G.oCam[1].GrabB_User(m_nCount_AO[port] - 1);
                    //    //  Turn Off LED for Heat Dissipation
                    //}
                    //if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
                    //{
                    //    if (!mDriverIC.AFLensMove_SEM(ch, (UInt16)m_Waveform_AF[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch, "AF LensMove I2C Fail", false);
                    //    }
                    //}
                    //if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch + 1])
                    //{
                    //    if (!mDriverIC.AFLensMove_SEM(ch + 1, (UInt16)m_Waveform_AF[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch + 1, "AF LensMove I2C Fail", false);
                    //    }
                    //}

                    //SupremeTimer.QueryPerformanceCounter(ref CurTime);
                    //m_OutTimeAF[port, m_nCount_AO[port]] = CurTime;
                    //if (m_nCount_AO[port] > (m_nMaxCount_AO[port] / 2))
                    //    m_AFPeakTime[port] = m_OutTimeAF[port, m_nCount_AO[port] - 1] + m__G.TimerFrequency / 500;

                    break;

                case (int)AO_TYPE._SEM_Zoom:
                    //if (m_nCount_AO[port] > 0)
                    //{
                    //    //  1msec wait for Settling LED Power
                    //    m__G.oCam[0].GrabB_User(m_nCount_AO[port] - 1);

                    //    if (m__G.mCamCount > 1)
                    //        m__G.oCam[1].GrabB_User(m_nCount_AO[port] - 1);
                    //    //  Turn Off LED for Heat Dissipation
                    //}
                    //if (m__G.m_ChannelOn[ch] && m__G.m_ChannelEff[ch])
                    //{
                    //    if (!mDriverIC.ZoomLensMove_SEM(ch, (UInt16)m_Waveform_ZM[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch, "ZM LensMove I2C Fail", false);
                    //    }
                    //}
                    //if (m__G.m_ChannelOn[ch + 1] && m__G.m_ChannelEff[ch + 1])
                    //{
                    //    if (!mDriverIC.ZoomLensMove_SEM(ch + 1, (UInt16)m_Waveform_ZM[m_nCount_AO[port]]))
                    //    {
                    //        m__G.fManage.AddOperatorLog(ch + 1, "ZM LensMove I2C Fail", false);
                    //    }
                    //}

                    //SupremeTimer.QueryPerformanceCounter(ref CurTime);
                    //m_OutTimeZM[port, m_nCount_AO[port]] = CurTime;
                    //if (m_nCount_AO[port] > (m_nMaxCount_AO[port] / 2))
                    //    m_ZMPeakTime[port] = m_OutTimeZM[port, m_nCount_AO[port] - 1] + m__G.TimerFrequency / 500;

                    break;
            }
            m_nCount_AO[port]++;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetBU252CrossAxisOffet(int port)
        {
            //  Set Y Axis Default Offset for X driving Test
            byte[] wbuf = new byte[1] { 0x00 };         //  중심
            int ch = port * 2;
            if (m__G.m_ChannelOn[ch])
            {
                mDriverIC.WriteToBU242(ch, 0x6026, wbuf);
                mDriverIC.WriteToBU242(ch, 0x6027, wbuf);
                wbuf[0] = 0x03;
                mDriverIC.WriteToBU242(ch, ConstBU242.CONTROL, wbuf);                // Manual Mode
            }
            if (m__G.m_ChannelOn[ch + 1])
            {
                mDriverIC.WriteToBU242(ch + 1, 0x6026, wbuf);
                mDriverIC.WriteToBU242(ch + 1, 0x6027, wbuf);
                wbuf[0] = 0x03;
                mDriverIC.WriteToBU242(ch, ConstBU242.CONTROL, wbuf);                // Manual Mode
            }
        }



        public int MakeAFWaveform(int pos_min, int pos_max, int lstep = 30)
        {
            int i = 0;
            int k = 0;
            int iDrvStep = lstep;

            m_Waveform_AF[k++] = pos_min;    //  for AK7371
            m_Waveform_AF[k++] = pos_min;    //  for AK7371

            if (iDrvStep < 1)
                iDrvStep = 30; //  default step

            do
            {
                m_Waveform_AF[k++] = pos_min;
                pos_min += iDrvStep;
            } while (pos_min < pos_max);

            m_Waveform_AF[k++] = pos_max;
            m_Waveform_AF[k] = pos_max;

            for (i = 1; i <= k - 1; i++)
                m_Waveform_AF[k + i] = m_Waveform_AF[k - i - 1];

            m_Waveform_AF[k + i] = 2048;  //  Back to neutral Point

            return k + i - 2;
        }
        public int MakeZMWaveform(int pos_min, int pos_max, int lstep = 50)
        {
            int i = 0;
            int k = 0;
            int iDrvStep = lstep;

            m_Waveform_ZM[k++] = pos_min;    //  for AK7371
            m_Waveform_ZM[k++] = pos_min;    //  for AK7371

            if (iDrvStep < 20)
                iDrvStep = 30; //  default step

            do
            {
                m_Waveform_ZM[k++] = pos_min;
                pos_min += iDrvStep;
            } while (pos_min < pos_max);

            m_Waveform_ZM[k++] = pos_max;
            m_Waveform_ZM[k] = pos_max;

            for (i = 1; i <= k - 1; i++)
                m_Waveform_ZM[k + i] = m_Waveform_ZM[k - i - 1];

            m_Waveform_ZM[k + i] = 2048;  //  Back to neutral Point

            return k + i - 2;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------

        public void AO_Write(int ch, double value)
        {
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void AI_Read(int ch, ref double value)
        {
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
        public void Drive_LED(int ch, double volt)
        {
            //MessageBox.Show(ch.ToString() + "\t" + volt.ToString());
            
            mDriverIC.SetLEDpower(ch+1, (int)(volt * 500));
            if ( m__G != null )
            m__G.fManage.AddOperatorLog(ch, "Drive_LED = " + volt.ToString("F2"));
        }

        bool bDriveLED = false;
        public void Drive_LEDs(double voltR, double voltL, bool IsShow = false)
        {
            //MessageBox.Show(ch.ToString() + "\t" + volt.ToString());
            while (bDriveLED)
                Thread.Sleep(1);
            bDriveLED = true;
            mDriverIC.SetLEDpower(1, (int)(voltR * 500));
            mDriverIC.SetLEDpower(2, (int)(voltL * 500));
            if (IsShow)
                m__G.fManage.AddOperatorLog(0, "L-LED = " + voltL.ToString("F2") + "\tR-LED = " + voltR.ToString("F2"));
            bDriveLED = false;
        }



        public struct __HallSensor_Calibration_Parameter_DW9841
        {
            volatile public UInt16 HALL_BIAS_X;
            volatile public UInt16 HALL_AMP_OFS_X;
            volatile public Int16 HALL_DIG_GAIN_X;
            volatile public Int16 HALL_DIG_OFS_X;
            volatile public Int16 HALL_CAL_MAX_X;
            volatile public Int16 HALL_CAL_MIN_X;
            volatile public Int16 HALL_CAL_RANGE_X;
            volatile public UInt16 HALL_BIAS_Y;
            volatile public UInt16 HALL_AMP_OFS_Y;
            volatile public Int16 HALL_DIG_GAIN_Y;
            volatile public Int16 HALL_DIG_OFS_Y;
            volatile public Int16 HALL_CAL_MAX_Y;
            volatile public Int16 HALL_CAL_MIN_Y;
            volatile public Int16 HALL_CAL_RANGE_Y;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------  CLAF Test 출력 파형  -----------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------


        //------------------------------------------------------------------------------------------------------------------------------------------------
        //******************************************* 장비 구동_khkim **************************************************
        //------------------------------------------------------------------------------------------------------------------------------------------------

        public void safe_closing()
        {
            closing_flag = true;
            IsRun = false;          // monitorDIO - while loop exit
            Thread.Sleep(100);
        }

        public void socket_IN(int port)    //khkim for auto single
        {
            //Drive_LED(port, m__G.sRecipe.iLEDcurrentLL);
            m__G.fGraph.mDriverIC.LoadSocket(port);

        }
        public void socket_OUT(int port)  //khkim for auto single
        {
            //Drive_LED(port, 0);
            m__G.fGraph.mDriverIC.UnloadSocket(port);
        }

        //public void LoadTestUnload(int port) //khkim for auto single
        //{
        //    try
        //    {
        //        mPortIdle[port] = false;
        //        //  여기서 바코드 리딩 함수 추가 필요
        //        //  ...
        //        //  ...
        //        bool bIn = true;
        //        int ch = port * 2;

        //        int lunsafeCount = mDriverIC.m_nUnsafe;
        //        Thread.Sleep(100);
        //        m__G.fManage.ClearGraph(port);
        //        m__G.fManage.AddOperatorLog(ch, "Unsafe : " + mDriverIC.m_nUnsafe.ToString(), true);

        //        if (!F_Main.MotionUsed)
        //        {
        //            if (lunsafeCount != mDriverIC.m_nUnsafe)
        //            {
        //                mPortIdle[port] = true;
        //                m__G.fManage.AddOperatorLog(ch, "Safe Sensor Detected. Push Start Button Again..", true);

        //                return;
        //            }

        //        }

        //        socket_IN(port);

        //        for (int i = 0; i < 4; i++)
        //        {
        //            Thread.Sleep(50);
        //            if (!F_Main.MotionUsed)
        //            {
        //                if (lunsafeCount != mDriverIC.m_nUnsafe)
        //                {
        //                    socket_OUT(port);
        //                    mPortIdle[port] = true;
        //                    m__G.fManage.AddOperatorLog(ch, "Safe Sensor Detected. Push Start Button Again.", true);
        //                    return;

        //                }
        //            }

        //        }
        //        //m__G.fManage.AddOperatorLog(ch, "m_nUnsafe = " + mDriverIC.m_nUnsafe);
        //        Thread.Sleep(1500);

        //        //  다음은 풀어서 시험해봐야 함. 20181122
        //        m__G.fManage.mTestFinishState[ch] = 0;
        //        m__G.fManage.mTestFinishState[ch + 1] = 0;

        //        m__G.fManage.OperatorStartTest(port);

        //        Thread.Sleep(1000);

        //        while (!m__G.fManage.mProcessMonitorEscaped[port])
        //        {
        //            Thread.Sleep(200);
        //        }
        //        //m__G.fManage.AddOperatorLog(ch, "Then " + m__G.fManage.mTestFinishState[ch].ToString() + ":" + m__G.fManage.mTestFinishState[ch + 1].ToString());
        //        //m__G.fManage.AddOperatorLog(ch, "Test Finish(" + port.ToString() + ")");
        //        // 다음은 약간 유의미       20181122
        //        bIn = true;
        //        while (bIn)
        //        {
        //            //m__G.wrSytemLog("Test Finish Detected : port " + port.ToString());
        //            socket_OUT(port);
        //            m__G.wrSytemLog("socket_OUT( " + port.ToString() + ")");
        //            bIn = false;
        //            for (int i = 0; i < 5; i++)
        //            {
        //                if (mDriverIC.m_bUnsafe)
        //                {
        //                    DateTime lnow = DateTime.Now;
        //                    m__G.wrSytemLog("Safe Sensor Detected : " + lnow.ToString("yyyy/MM/dd/HH/mm/ss"));
        //                    m__G.fManage.AddOperatorLog(ch, "Safe Sensor Detected", true);
        //                    //Thread.Sleep(1);
        //                    socket_IN(port);
        //                    bIn = true;
        //                    Thread.Sleep(100);
        //                    while (true)
        //                    {
        //                        if (!mDriverIC.m_bUnsafe)
        //                            break;
        //                        Thread.Sleep(100);
        //                    }
        //                }
        //                Thread.Sleep(100);
        //            }
        //            m__G.wrSytemLog("Unsafe(" + port.ToString() + ") ? " + bIn.ToString());
        //        }
        //        mPortIdle[port] = true;
        //        m__G.mIdleTime[port] = DateTime.Now;

        //        if (F_Main.iscommand && F_Main.MotionUsed)
        //        {
        //            F_Main.isTesting = false;
        //            F_Main.iscommand = false;
                   
        //            if (F_Main.isFirstPosition)
        //            {
        //                if (m__G.fManage.mInfoBtn[ch].Text == "PASS" || m__G.fManage.mInfoBtn[ch].Text == "Pass")
        //                {
        //                    m__G.fManage.ClientMain("ReadyTest");
        //                    F_Main.isFirstPosition = false;
        //                }
        //                else
        //                {
        //                    if (m__G.m_FirstFailMotionStop)
        //                        m__G.fManage.ClientMain("FirstFail");
        //                    else
        //                        m__G.fManage.ClientMain("ReadyTest");
        //                    F_Main.isFirstPosition = false;
        //                }
        //            }
        //            else
        //            {
        //                m__G.fManage.ClientMain("ReadyTest");
        //            }
                    
        //        }
        //        if (F_Main.MachineType == (int)MachineType.Handler)
        //        {
        //            Thread.Sleep(1000);
        //            string tmpstr = string.Empty;
        //            if (m__G.fManage.mInfoBtn[ch].Text == "PASS" || m__G.fManage.mInfoBtn[ch].Text == "Pass")
        //                tmpstr = "1,PASS,";
        //            else
        //                tmpstr = "1," + m__G.fManage.mInfoBtn[ch].Text + ",";

        //            //if (m__G.fManage.mInfoBtn[ch + 1].Text == "PASS" || m__G.fManage.mInfoBtn[ch + 1].Text == "Pass")
        //            //    tmpstr = tmpstr + "2,PASS";
        //            //else
        //            //    tmpstr = tmpstr + "2," + m__G.fManage.mInfoBtn[ch + 1].Text;

        //            m__G.fManage.ClientMain(tmpstr + "\r\n");


        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }


        //}

        public int GetPortCount()
        {
            if (m__G.mChannelCount < mDriverIC.m_PortCount)
                return m__G.mChannelCount;
            else
                return mDriverIC.m_PortCount;
        }

        public bool IsRun = true;
        private void MonitorDIO()
        {
            //bool IsRun = true;
            byte[] DIdata = new byte[3];
            while (true)
            {
                Thread.Sleep(10000);

                if (m__G.mDoingStatus == "IDLE")
                {
                    m__G.mIDLEcount++;
                    //  5초 이상 IDLE 된 경우만 LED 를 점등해준다.
                    if (m__G.mIDLEcount > 3)
                    {
                        Drive_LEDs(0.5, 0.5);
                        Thread.Sleep(1);

                        // 점등 와중에IDLE 벗어나는 경우도 있으므로 아래 처리
                        if (m__G.mIDLEcount > 3)
                        {
                            if (m__G.mDoingStatus == "IDLE")
                            {
                                Drive_LEDs(0, 0);
                                m__G.mIDLEcount=0;
                            }
                        }
                    }
                }
                else
                    m__G.mIDLEcount = 0;
            }
        }

        private void SafetySensorCheck() //hch
        {

            if (BeforeSafetySensorOnoff)
            {
                if (!SafetySensorOnOff)
                    if (!F_Main.isTesting)
                        m__G.fManage.ClientMain("SafetySensorOff");



            }
            else
            {
                if (SafetySensorOnOff)
                    if (!F_Main.isTesting)
                    {
                        m__G.fManage.ClientMain("SafetySensorOn");
                        Thread.Sleep(2000);
                    }


            }
            //    
        }

        //public bool RunTestOnCommand() //hch
        //{
        //    F_Main.iscommand = true;

        //    return RunTest();
        //}
        //public bool RunTest()
        //{
        //    try
        //    {
        //        bool res = false;

        //        if (mPortIdle[(int)(Port.P_LEFT)])
        //        {
        //            //  안전센서 모니터링에서 세팅한 값 확인
        //            //  안전센서 모니터링 Thread 에서 값을 true-false 로 50msec 단위로 점검한다.
        //            if (m__G.mChannelCount > 1)
        //            {
        //                m__G.fGraph.mDriverIC.SetFailLED(0, false);
        //                m__G.fGraph.mDriverIC.SetFailLED(1, false);
        //            }

        //            Thread ThreadRunSequence = new Thread(() => LoadTestUnload((int)(Port.P_LEFT)));
        //            ThreadRunSequence.Start();
        //            res = true;
        //            Thread.Sleep(200);
        //        }

        //        //    if (m__G.mCamCount < 2) return res;

        //        //if (mPortIdle[(int)(Port.P_RIGHT)])
        //        //{
        //        //    //  안전센서 모니터링에서 세팅한 값 확인
        //        //    //  안전센서 모니터링 Thread 에서 값을 true-false 로 50msec 단위로 점검한다.
        //        //    m__G.fGraph.mDriverIC.SetFailLED(2, false);
        //        //    m__G.fGraph.mDriverIC.SetFailLED(3, false);
        //        //    Thread ThreadRunSequence = new Thread(() => LoadTestUnload((int)(Port.P_RIGHT)));
        //        //    ThreadRunSequence.Start();
        //        //    res = true;
        //        //    Thread.Sleep(200);
        //        //}
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        return false;
        //    }

        //}
        //public bool IsIdle()
        //{
        //    bool res = false;

        //    //if (mPortIdle[(int)(Port.P_RIGHT)])
        //    if (mPortIdle[(int)(Port.P_LEFT)])
        //        res = true;

        //    return res;
        //}
        //public bool IsIdle(int port)
        //{
        //    return mPortIdle[port];
        //}
        //******************************************* 장비 구동_khkim **************************************************
        //--------------------------------------------------------------------------------------------------------------
        //public void ReadHall(ref double hX, ref double hY)
        //{

        //    //hX = -m_dataScaled_AI[(int)AI.CH7];
        //    //hY = m_dataScaled_AI[(int)AI.CH6];
        //}

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //  FRA by Driver IC Internal Function
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////DW9841


        public int DW9841_FW_SIZE = 20480;
        public int DOWNLOAD_FW_LEN = 0;
        public UInt16 I2C_BULK_SIZE = 16;
        public int REF_STROKE = 130;
        public UInt16 MTP_START_ADDRESS = 0x8000;



        public void MemoryCopy(byte[] Value, byte[] Data, UInt16 Length, int iStartIndex)
        {
            int i = 0;
            for (i = 0; i < Length; i++) Value[i] = Data[iStartIndex + i];
        }

        public void MemoryCopy2(byte[] Value, byte[] Data, UInt16 Length, int iStartIndex)
        {
            int i = 0;
            for (i = 0; i < Length; i++) Value[iStartIndex + i] = Data[i];
        }

        public int GetDirection(UInt16 baseTarget, UInt16 firstTarget, double basePosition, double firstPosition)
        {
            int direction = 0;
            if (baseTarget < firstPosition)
            {
                if (basePosition < firstPosition) direction = 1;
                else direction = -1;
            }
            else
            {
                if (basePosition < firstPosition) direction = 1;
                else direction = -1;
            }
            return direction;
        }
        public bool[] m_bEPAdone = { false, false };
        public UInt16[] mZoomMacroFinal = new UInt16[2];



    }
}