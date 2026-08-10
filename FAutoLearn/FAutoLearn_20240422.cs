using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using OpenCvSharp;
using System.IO;
using MOTSimulator;
using static alglib;

namespace FAutoLearn
{
    public partial class FAutoLearn : Form
    {

        public string mOrgFile = "";
        string RootPath = "C:\\CSHTest\\DoNotTouch\\";

        public FZMath mFZM = new FZMath();
        public MOTSimDlg motSimDlg = new MOTSimDlg();

        //public Mat mSourceImgFile = null;
        public Mat[] mSourceImg = new Mat[30];
        public Mat mCustomImg = null;
        public Mat mCustomImg2 = null;
        //Mat mProcessImg = null;
        public Mat mOverlayedImg = null;
        public Mat mSchematicImg = null;
        public Mat mSchematicOverlayedImg = null;
        public int mLastImg = 0;
        public int mProcSequence = 0;
        public Rect[][] mDetectedRect = new Rect[20][];
        public Rect[][] mDetectedLearnRect = new Rect[200][];
        public int[] mDetectedModel = new int[200];
        public int[][] mDetectedLearned = new int[200][];
        public int mTrainingLength = 0;
        public string[] mTargetFiles;
        public bool mHaveOpenBaseModel = false;
        public bool mHaveSaveBaseModel = false;
        public bool mIsDebug = false;
        //public int nSizeX = 0;
        //public int nSizeY = 0;
        public int m_nCam = 0;
        public string mMatroxMsg = "";
        public double mm_FakeMark = 0.38;        //(X + InMidTopX,       Y + InMidTopY )에서 Thresh 이하

        public string mLastSchematicPicture = "";
        public bool mbSchematicPicture = false;
        public bool mbGetHistogram = false;

        public double vSin40 = Math.Sin(40.0 / 180 * Math.PI);
        public double vCos40 = Math.Cos(40.0 / 180 * Math.PI);
        public double vTan40 = Math.Tan(40.0 / 180 * Math.PI);

        const double LensMag = 0.35;

        public void SetSideviewTheta(double rad)
        {
            vSin40 = Math.Sin(rad);
            vCos40 = Math.Cos(rad);
            vTan40 = Math.Tan(rad);
            mFZM.SetSideviewTheta(rad);
            motSimDlg.SetSideviewTheta(rad);
        }

        public CSTracker mCst = new CSTracker();
        public CSTracker mCstIgnore = new CSTracker();
        public List<CSMarker> mCsmSide = new List<CSMarker>();
        public List<CSMarker> mCsmTop = new List<CSMarker>();
        public List<TextBox[]> mMarkerPosInputS = new List<TextBox[]>();
        public List<TextBox[]> mMarkerPosInputT = new List<TextBox[]>();
        public int mCurCsmIndex = 0;
        public FZMath.Point2D[] mMarkNorm = new FZMath.Point2D[4];   //  [0] : North, [1] : West ,  [2] : South, [3] : East //  Pixel 단위로 입력되어야함
        public const int FOV_Y = 342;
        public const int FOV_X = 760;

        public bool mbConfirmed = false;
        public bool mFastMode = false;
        public int mCandidateIndex = 0;   //  모델을 하나 시도할 때마다 증가한다.

        public bool mCheckCompatibility = false;
        public struct sSearchModel
        {
            public int mScale;
            public int width;
            public int height;
            public int conv;
            public int planeShift;
            public double planeHeight;
            public int[] img;
            //public int[] diffimg;
        }

        public class sMarkResult
        {
            public int Azimuth = -1;
            public OpenCvSharp.Point2d pos = new OpenCvSharp.Point2d();
            public System.Drawing.Size mSize = new System.Drawing.Size();
            public double mMTF = 0;
            public sMarkResult(int azimuth = -1, double x = 0, double y = 0)
            {
                Azimuth = azimuth;
                pos.X = x;
                pos.Y = y;
            }
        }

        public class sFiducialMark
        {
            public int ID = 0;
            public int Azimuth = 12;    // 미정인 상태
            public int xPosType = 0;
            public int yPosType = 0;
            public OpenCvSharp.Size modelSize = new OpenCvSharp.Size();
            public Rect searchRoi = new Rect();   //  Search ROI
            public int conv = 0;
            public int planeShift = 0;
            public double planeHeight = 0;// 현재 사용 안함
            public int MScale = 4;  //  Model Scale
            public Rect exArea;
            public int[] img = null;    //  model image data
            public sFiducialInfo fidInfo = new sFiducialInfo()
            {
                signX = 1, signY = 1
            };
        }
        public struct sFiducialInfo
        {
            public string schematicFile;    //  마크가 생성된 Schmatic Picture File 의 Full Path Name
            public int Azimuth;    //   마크의 Azimuth : 0,1,2,3,4, ... , 11
            public int markID;  //   없을 때 0
            public double X;   //  마크 중심의 실제 좌표 X
            public double Y;   //  마크 중심의 실제 좌표 Y
            public int signX;   //  마크 중심의 실제 좌표 X
            public int signY;   //  마크 중심의 실제 좌표 Y
            //public System.Drawing.Point markPos = new System.Drawing.Point(0, 0);    //  마크의 좌상 위치
            //public System.Drawing.Point TBXpos = new System.Drawing.Point(0, 0);     //  도면정보상의 X 좌표가 들어있는 TextBox 의 좌표
            //public System.Drawing.Point TBYpos = new System.Drawing.Point(0, 0);     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 좌표
            public int markPosX;    //  마크의 좌상 X 위치 ( Tracker )
            public int markPosY;     //  마크의 좌상 Y 위치 ( Tracker )
            public int TBXposX;     //  도면정보상의 X 좌표가 들어있는 TextBox 의 X좌표
            public int TBXposY;    //  도면정보상의 X 좌표가 들어있는 TextBox 의 Y좌표
            public int TBYposX;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 X좌표
            public int TBYposY;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 Y좌표
        }
        //public class sFiducialInfo
        //{
        //    public string schematicFile = "";    //  마크가 생성된 Schmatic Picture File 의 Full Path Name
        //    public int Azimuth = 0;    //   마크의 Azimuth : 0,1,2,3,4, ... , 11
        //    public int markID = 0;  //   없을 때 0
        //    public double X = 0;   //  마크 중심의 실제 좌표 X
        //    public double Y = 0;   //  마크 중심의 실제 좌표 Y
        //    public int signX = 1;   //  마크 중심의 실제 좌표 X
        //    public int signY = 1;   //  마크 중심의 실제 좌표 Y
        //    //public System.Drawing.Point markPos = new System.Drawing.Point(0, 0);    //  마크의 좌상 위치
        //    //public System.Drawing.Point TBXpos = new System.Drawing.Point(0, 0);     //  도면정보상의 X 좌표가 들어있는 TextBox 의 좌표
        //    //public System.Drawing.Point TBYpos = new System.Drawing.Point(0, 0);     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 좌표
        //    public int markPosX = 0;    //  마크의 좌상 X 위치 ( Tracker )
        //    public int markPosY = 0;     //  마크의 좌상 Y 위치 ( Tracker )
        //    public int TBXposX = 0;     //  도면정보상의 X 좌표가 들어있는 TextBox 의 X좌표
        //    public int TBXposY = 0;    //  도면정보상의 X 좌표가 들어있는 TextBox 의 Y좌표
        //    public int TBYposX = 0;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 X좌표
        //    public int TBYposY = 0;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 Y좌표
        //}

        public struct DetectedMark
        {
            public string imgFile;
            public Rect rect;
            public int absIndex;   //  datagridview1 에서 row index 와 같다.
            public int Azimuth;    //  마크의 방위
            public double x;    //  마크의 정밀 위치
            public double y;    //  마크의 정밀 위치
            public bool IsGood;

            public double conv;
            public double diff;     // Diff
            public double Xdiff;    // XDiff  
            public double Ydiff;    // YDiff  
            public double IO;       // I-O    
            public double LR;       // L-R    
            public double TB;       // T-B   
            public double CLR;      // C-LR   
            public double CTB;      // C-TB 
            public double std;      // σ 
            public double IFstd;    // IσFσ
            public byte[] img;
        }
        public class sPeakType
        {
            public int[] type = new int[4];
        }
        public sPeakType[] mPeakType = new sPeakType[12];


        public byte[][] mQuadImg = new byte[400][];    //  Max 400 ea quadImg Buffer, absIndex 로 참조된다.
        public int[] mGIndex = new int[400];         //  하나하나가 지정된 absIndex 를 값으로 가진다.
        public int[] mNGIndex = new int[400];        //  하나하나가 지정된 absIndex 를 값으로 가진다.
        public int mNGCount = 0;
        public DetectedMark[] mDetectedmark = null;
        public int mMarkCount = 0;                      //  mDetectedmark 가 초기화된 이후 찾은 마크 개수
        public int mMarkFocused = -1;

        public int mBaseModelCnt = 0;

        public byte[][] q_Value = new byte[30][];   //  축소이미지

        public sSearchModel[] mSearchModel = new sSearchModel[10];
        public sSearchModel[] mNewSearchModel = new sSearchModel[10];
        public int mSearchModelIndex = 0;
        public int mSelectedSubIndex = 0;

        public System.Drawing.Image mBtnP;
        public System.Drawing.Image mBtnN;

        public List<sFiducialInfo> mFidInfo = new List<sFiducialInfo>();
        public List<sFiducialMark> mFidMarkSide = new List<sFiducialMark>();
        public List<sFiducialMark> mFidMarkTop = new List<sFiducialMark>();
        public sFiducialMark[] mFidMarkSideBk = null;
        public sFiducialMark[] mFidMarkTopBk = null;
        public List<sFiducialMark> mFidMarkSideF = null;// new List<sFiducialMark>();
        public List<sFiducialMark> mFidMarkTopF = null;// new List<sFiducialMark>();

        public bool IsSeardchROIUpdate = false;
        public bool IsModelSizeUpdate = false;
        public int mBreakIndex = -1;

        public FAutoLearn()
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            string newDir = RootPath + "RawData";
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAutoLearn));
            mBtnP = ((System.Drawing.Image)(resources.GetObject("button9.BackgroundImage")));
            mBtnN = ((System.Drawing.Image)(resources.GetObject("button9.BackgroundImage")));

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            newDir = RootPath + "Training";
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }


            InitializeComponent();
            //textBox1.Text = "0.0";
            for (int i = 0; i < 20; i++)
                mDetectedRect[i] = new Rect[1000];

            //for (int i = 0; i < mBaseModel.Length; i++)
            //{
            //    mBaseModel[i].slave = new byte[100];
            //    mBaseModel[i].slaveCnt = 0;
            //}
            //for (int i = 0; i < mLearnedModel.Length; i++)
            //{
            //    mLearnedModel[i].slave = new byte[100];
            //    mLearnedModel[i].shiftX = new byte[100];
            //    mLearnedModel[i].shiftY = new byte[100];
            //    mLearnedModel[i].slaveCnt = 0;
            //}
            for (int i = 0; i < 20; i++)
                mSourceImg[i] = new Mat();

            //for (int i = 0; i < mCsmSide.Count; i++)
            //{
            //    mCsmSide[i] = new CSMarker();
            //    mCsmSide[i].Create();
            //}

            //for (int i = 0; i < mCsmTop.Count; i++)
            //{
            //    mCsmTop[i] = new CSMarker();
            //    mCsmTop[i].Create();
            //}

            for (int i = 0; i < 400; i++)
            {
                mGIndex[i] = -1;
                mNGIndex[i] = -1;
            }

            mCst.ResizePinSize = 6;
            mCst.Create();
            mCstIgnore.ResizePinSize = 6;
            mCstIgnore.Create(1);

            for (int i = 0; i < mPeakType.Length; i++)
                mPeakType[i] = new sPeakType();
        }

        private void InitBtns()
        {
            System.Drawing.Point[] ptsUU =  {
                                new System.Drawing.Point(25,  0),
                                new System.Drawing.Point(50,  13),
                                new System.Drawing.Point(50,  21),
                                new System.Drawing.Point(0,  21),
                                new System.Drawing.Point(0,  13),
                                new System.Drawing.Point(25, 0),
                          };
            // Make the GraphicsPath.
            GraphicsPath polygon_path = new GraphicsPath(FillMode.Winding);
            polygon_path.AddPolygon(ptsUU);
            Region polygon_region = new Region(polygon_path);
            btnUU.Region = polygon_region;
            btnUU.SetBounds(
                btnUU.Location.X,
                btnUU.Location.Y, ptsUU[2].X + 4, ptsUU[3].Y + 4);

            btnDU.Region = polygon_region;
            btnDU.SetBounds(
                btnDU.Location.X,
                btnDU.Location.Y, ptsUU[2].X + 4, ptsUU[3].Y + 4);

            System.Drawing.Point[] ptsUD =  {
                                new System.Drawing.Point(0, 0),
                                new System.Drawing.Point(50, 0),
                                new System.Drawing.Point(50, 8),
                                new System.Drawing.Point(25, 21),
                                new System.Drawing.Point(0, 8),
                                new System.Drawing.Point(0, 0),
                          };
            // Make the GraphicsPath.
            polygon_path = new GraphicsPath(FillMode.Winding);
            polygon_path.AddPolygon(ptsUD);
            polygon_region = new Region(polygon_path);
            btnUD.Region = polygon_region;
            btnUD.SetBounds(
                btnUD.Location.X,
                btnUD.Location.Y, ptsUD[2].X + 4, ptsUD[3].Y + 4);

            btnDD.Region = polygon_region;
            btnDD.SetBounds(
                btnDD.Location.X,
                btnDD.Location.Y, ptsUD[2].X + 4, ptsUD[3].Y + 4);

            //////////////////////////////////////////////////////////////////

            System.Drawing.Point[] ptsLL =  {
                                new System.Drawing.Point(0,  25),
                                new System.Drawing.Point(13,  0),
                                new System.Drawing.Point(21,  0),
                                new System.Drawing.Point(21,  50),
                                new System.Drawing.Point(13,  50),
                                new System.Drawing.Point(0, 25),
                          };
            // Make the GraphicsPath.
            polygon_path = new GraphicsPath(FillMode.Winding);
            polygon_path.AddPolygon(ptsLL);
            polygon_region = new Region(polygon_path);
            btnLL.Region = polygon_region;
            btnLL.SetBounds(
                btnLL.Location.X,
                btnLL.Location.Y, ptsLL[2].X + 4, ptsLL[3].Y + 4);

            btnRL.Region = polygon_region;
            btnRL.SetBounds(
                btnRL.Location.X,
                btnRL.Location.Y, ptsLL[2].X + 4, ptsLL[3].Y + 4);

            System.Drawing.Point[] ptsRR =  {
                                new System.Drawing.Point(0,  0),
                                new System.Drawing.Point(9, 0),
                                new System.Drawing.Point(21, 25),
                                new System.Drawing.Point(9, 50),
                                new System.Drawing.Point(0, 50),
                                new System.Drawing.Point(0, 0),
                          };
            // Make the GraphicsPath.
            polygon_path = new GraphicsPath(FillMode.Winding);
            polygon_path.AddPolygon(ptsRR);
            polygon_region = new Region(polygon_path);
            btnLR.Region = polygon_region;
            btnLR.SetBounds(
                btnLR.Location.X,
                btnLR.Location.Y, ptsRR[2].X + 4, ptsRR[3].Y + 4);

            btnRR.Region = polygon_region;
            btnRR.SetBounds(
                btnRR.Location.X,
                btnRR.Location.Y, ptsRR[2].X + 4, ptsRR[3].Y + 4);


            button12.Text = '\u23F6'.ToString();
            button13.Text = '\u23F4'.ToString();
            button14.Text = '\u23F5'.ToString();
            button15.Text = '\u23F7'.ToString();
        }
        private void InitialzeDataSet(int maxNumMark)
        {
            mDetectedmark = new DetectedMark[5 * maxNumMark];
            mMarkCount = 0;
            for (int i = 0; i < 400; i++)
            {
                mGIndex[i] = -1;
                mNGIndex[i] = -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";//  Select Multiple Image File Names and Show on the Listbox
            listBox1.Items.Clear();

            //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\RawData"));
            //string sFilePath = folder;
            string sFilePath = Path.GetFullPath("C:\\CSHTest\\Result\\RawData");
            if (!Directory.Exists(sFilePath))
                Directory.CreateDirectory(sFilePath);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.InitialDirectory = sFilePath;
            openFileDialog1.FilterIndex = 2;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //Image myImage = Image.FromFile(filename);
            mTargetFiles = new string[openFileDialog1.FileNames.Length];
            //int[] forder = new int[4] { 0, 3, 1, 2 };
            for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
            {
                mTargetFiles[i] = openFileDialog1.FileNames[i];
                listBox1.Items.Add(mTargetFiles[i]);
            }
            InitialzeDataSet(20 * openFileDialog1.FileNames.Length);

            listBox1.SetSelected(0, true);
            mOrgFile = listBox1.Items[listBox1.SelectedIndex].ToString();

            //Mat tmpImg = new Mat();
            //if (mSourceImg[0] == null)
            //    tmpImg = new Mat(mOrgFile);
            //else
            //    tmpImg = Cv2.ImRead(mOrgFile);
            //Cv2.CvtColor(tmpImg, mSourceImg[0], ColorConversionCodes.BGR2GRAY);

            mSourceImg[0] = new Mat(mOrgFile, ImreadModes.Grayscale);
            mOverlayedImg = new Mat();
            Cv2.CvtColor(mSourceImg[0], mOverlayedImg, ColorConversionCodes.GRAY2RGB);

            DrawOpticalInfo();

            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);

            pictureBox2.Image = myImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        public void DrawOpticalInfo()
        {
            //  Draw Top View Center
            int lWidth = mOverlayedImg.Width;
            int lHeight = mOverlayedImg.Height;

            double tgtOffset = mOpticsTgtOffset / (0.0055 / LensMag) * vSin40;
            //  Draw Center of Top View -> 영상의 유효영역을 벗어나므로 그리지 않는다.

            //  Draw Side View center                                                         
            Cv2.Line(mOverlayedImg, lWidth / 2 - 20, lHeight / 2, lWidth / 2 + 20, lHeight / 2, Scalar.Aqua, 1, LineTypes.Link4);
            Cv2.Line(mOverlayedImg, lWidth / 2, lHeight / 2 - 10, lWidth / 2, lHeight / 2 + 10, Scalar.Aqua, 1, LineTypes.Link4);

            //  Draw Center of Top View    
            Cv2.Line(mOverlayedImg, lWidth / 2 - 20, (int)(lHeight / 2 - tgtOffset), lWidth / 2 + 20, (int)(lHeight / 2 - tgtOffset), Scalar.Red, 1, LineTypes.Link4);
            Cv2.Line(mOverlayedImg, lWidth / 2, (int)(lHeight / 2 - 10 - tgtOffset), lWidth / 2, (int)(lHeight / 2 + 10 - tgtOffset), Scalar.Red, 1, LineTypes.Link4);
            //Cv2.Circle(mOverlayedImg, new OpenCvSharp.Point(lWidth / 2, lHeight / 2 - tgtOffset), 4, Scalar.Red, 1);

            //  Draw Center of all fiducial marks
            //  mOverlayedImg 에 표시한다.
            //  Draw center of the Focus Translator Window
            double fWndOffset = (mOpticsFWOffset / (0.0055 / LensMag));
            double wy = lHeight / 2 + fWndOffset;
            Cv2.Line(mOverlayedImg, lWidth / 2 - lWidth / 6, (int)(wy + 0.4999), lWidth / 2 + lWidth / 6, (int)(wy + 0.4999), Scalar.LimeGreen, 1, LineTypes.Link4);

            //double x = 0;
            //double y = 0;
            double rx = 0;
            double ry = 0;
            bool[] sideAzimuthUpper = new bool[8];
            bool[] sideAzimuthLeft = new bool[8];
            System.Drawing.Point[] markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
            int markCount = 0;

            int cx = mOverlayedImg.Width / 2;
            int cy = mOverlayedImg.Height / 2;

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                bool yUpper = true;
                if (mFidMarkSide[i].fidInfo.TBXposY > pictureBox6.Height / 2)
                    yUpper = false;

                bool IsLeft = true;
                if (mFidMarkSide[i].fidInfo.TBYposX > pictureBox6.Width / 2)    //   Y 좌표 넣는 Box 가 우측에 있이면 우측
                    IsLeft = false;

                sideAzimuthUpper[mFidMarkSide[i].Azimuth] = yUpper;
                sideAzimuthLeft[mFidMarkSide[i].Azimuth] = IsLeft;
                TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, yUpper, true, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);

                Cv2.Line(mOverlayedImg, cx + (int)rx - 10, cy + (int)markPos[markCount].Y, cx + (int)rx + 10, cy + (int)markPos[markCount].Y, Scalar.OrangeRed, 1, LineTypes.Link4);
                Cv2.Line(mOverlayedImg, cx + (int)rx, cy + (int)(markPos[markCount].Y - 10), cx + (int)rx, cy + (int)(markPos[markCount].Y + 10), Scalar.OrangeRed, 1, LineTypes.Link4);

                markCount++;
            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                //  Top Azimuth = 0,1,2,3  :: corresponds to Side 0,1,4,5

                //  mFidMarkTop[i].Azimuth
                bool yUpper = true;
                bool IsLeft = true;
                if (mFidMarkTop[i].Azimuth < 2)
                {
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth];  //  이게 뭐하는 변수인가 ?
                    IsLeft = sideAzimuthLeft[mFidMarkTop[i].Azimuth];
                }
                else
                {
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth + 2];
                    IsLeft = sideAzimuthLeft[mFidMarkTop[i].Azimuth + 2];
                }

                TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, yUpper, false, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);

                Cv2.Line(mOverlayedImg, cx + (int)markPos[markCount].X - 10, cy + (int)markPos[markCount].Y, cx + (int)markPos[markCount].X + 10, cy + (int)markPos[markCount].Y, Scalar.SkyBlue, 1, LineTypes.Link4);
                Cv2.Line(mOverlayedImg, cx + (int)markPos[markCount].X, cy + (int)markPos[markCount].Y - 10, cx + (int)markPos[markCount].X, cy + (int)markPos[markCount].Y + 10, Scalar.SkyBlue, 1, LineTypes.Link4);

                markCount++;
            }
            //if (markCount>0)
            //{
            //    StreamWriter wr = new StreamWriter("markPosOnPanel.csv");
            //    for (int i = 0; i < markPos.Length; i++)
            //        wr.WriteLine(markPos[i].X.ToString() + "," + markPos[i].Y.ToString());
            //    wr.Close();
            //}
            SaveMarkPosOnPanel();
        }

        public System.Drawing.Point[] mMarkPosOnPanel = null;
        public long[] mMarkConvOnPanel = null;

        public void GetDefaultMarkPosOnPanel(out System.Drawing.Point[] markPos)
        {
            markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];

            //int lWidth = mOverlayedImg.Width;
            //int lHeight = mOverlayedImg.Height;

            double fWndOffset = (mOpticsFWOffset / (0.0055 / LensMag)); //  mOpticsFWOffset 에 -365 가 저장되어있어야 한다.
            //double wy = lHeight / 2 + fWndOffset;

            //double x = 0;
            //double y = 0;
            double rx = 0;
            double ry = 0;

            bool[] sideAzimuthUpper = new bool[8];  //  마크가 영상상반부에 위치하는지 하반부에 위치하는지 저장했다가 좌표계산 시 활용
            bool[] sideAzimuthLeft = new bool[8];   //  마크가 영상좌반부에 위치하는지 우반부에 위치하는지 저장했다가 좌표계산 시 활용
            int markCount = 0;

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                bool yUpper = true;
                if (mFidMarkSide[i].fidInfo.TBXposY > pictureBox6.Height / 2)
                    yUpper = false;

                bool IsLeft = true;
                if (mFidMarkSide[i].fidInfo.TBYposX > pictureBox6.Width / 2)    //   Y 좌표 넣는 Box 가 우측에 있이면 우측
                    IsLeft = false;

                sideAzimuthUpper[mFidMarkSide[i].Azimuth] = yUpper;
                sideAzimuthLeft[mFidMarkSide[i].Azimuth] = IsLeft;
                TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, yUpper, true, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markCount++;
            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                //  Top Azimuth = 0,1,2,3  :: corresponds to Side 0,1,4,5

                //  mFidMarkTop[i].Azimuth
                bool yUpper = true;
                bool IsLeft = true;
                if (mFidMarkTop[i].Azimuth < 2)
                {
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth];
                    IsLeft = sideAzimuthLeft [mFidMarkTop[i].Azimuth];
                }                             
                else                          
                {                             
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth + 2];
                    IsLeft = sideAzimuthLeft [mFidMarkTop[i].Azimuth + 2];
                }

                TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, yUpper, false, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markCount++;

            }
        }
        public void GetMarkPosOnPanel(out System.Drawing.Point[] markPos)
        {
            markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
            if (mCandidateIndex < 1)
                return;

            
            //int lWidth = mOverlayedImg.Width;
            //int lHeight = mOverlayedImg.Height;

            double fWndOffset = (mOpticsFWOffset / (0.0055 / LensMag));
            //double wy = lHeight / 2 + fWndOffset;

            //double x = 0;
            //double y = 0;
            double rx = 0;
            double ry = 0;

            bool[] sideAzimuthUpper = new bool[8];  //  마크가 영상상반부에 위치하는지 하반부에 위치하는지 저장했다가 좌표계산 시 활용
            bool[] sideAzimuthLeft = new bool[8];   //  마크가 영상좌반부에 위치하는지 우반부에 위치하는지 저장했다가 좌표계산 시 활용
            int markCount = 0;

            for (int i = 0; i < mFMSideCandidate[mCandidateIndex - 1].Count; i++)
            {
                bool yUpper = true;
                if (mFMSideCandidate[mCandidateIndex - 1][i].fidInfo.TBXposY > pictureBox6.Height / 2)
                    yUpper = false;

                bool IsLeft = true;
                if (mFMSideCandidate[mCandidateIndex - 1][i].fidInfo.TBYposX > pictureBox6.Width / 2)    //   Y 좌표 넣는 Box 가 우측에 있이면 우측
                    IsLeft = false;

                sideAzimuthUpper[mFMSideCandidate[mCandidateIndex - 1][i].Azimuth] = yUpper;
                sideAzimuthLeft[mFMSideCandidate[mCandidateIndex - 1][i].Azimuth] = IsLeft;
                TransferAbsToPicturePos(mFMSideCandidate[mCandidateIndex - 1][i].fidInfo.X, mFMSideCandidate[mCandidateIndex - 1][i].fidInfo.Y, ref rx, ref ry, yUpper, true, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markCount++;
            }

            for (int i = 0; i < mFMTopCandidate[mCandidateIndex - 1].Count; i++)
            {
                //  Top Azimuth = 0,1,2,3  :: corresponds to Side 0,1,4,5

                //  mFidMarkTop[i].Azimuth
                bool yUpper = true;
                bool IsLeft = true;
                if (mFMTopCandidate[mCandidateIndex - 1][i].Azimuth < 2)
                {
                    yUpper = sideAzimuthUpper[mFMTopCandidate[mCandidateIndex - 1][i].Azimuth];
                    IsLeft = sideAzimuthLeft[mFMTopCandidate[mCandidateIndex - 1][i].Azimuth];
                }
                else
                {
                    yUpper = sideAzimuthUpper[mFMTopCandidate[mCandidateIndex - 1][i].Azimuth + 2];
                    IsLeft = sideAzimuthLeft[mFMTopCandidate[mCandidateIndex - 1][i].Azimuth + 2];
                }

                TransferAbsToPicturePos(mFMTopCandidate[mCandidateIndex - 1][i].fidInfo.X, mFMTopCandidate[mCandidateIndex - 1][i].fidInfo.Y, ref rx, ref ry, yUpper, false, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markCount++;

            }
        }

        public void SaveMarkPosOnPanel()
        {
            //int lWidth = mOverlayedImg.Width;
            //int lHeight = mOverlayedImg.Height;

            double fWndOffset = (mOpticsFWOffset / (0.0055 / LensMag));
            //double wy = lHeight / 2 + fWndOffset;

            //double x = 0;
            //double y = 0;
            double rx = 0;
            double ry = 0;

            bool[] sideAzimuthUpper = new bool[8];  //  마크가 영상상반부에 위치하는지 하반부에 위치하는지 저장했다가 좌표계산 시 활용
            bool[] sideAzimuthLeft = new bool[8];   //  마크가 영상좌반부에 위치하는지 우반부에 위치하는지 저장했다가 좌표계산 시 활용
            System.Drawing.Point[] markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
            long[] markConv = new long[mFidMarkSide.Count + mFidMarkTop.Count];
            int markCount = 0;

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                bool yUpper = true;
                if (mFidMarkSide[i].fidInfo.TBXposY > pictureBox6.Height / 2)
                    yUpper = false;

                bool IsLeft = true;
                if (mFidMarkSide[i].fidInfo.TBYposX > pictureBox6.Width / 2)    //   Y 좌표 넣는 Box 가 우측에 있이면 우측
                    IsLeft = false;

                sideAzimuthUpper[mFidMarkSide[i].Azimuth] = yUpper;
                sideAzimuthLeft[mFidMarkSide[i].Azimuth] = IsLeft;
                TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, yUpper, true, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markConv[markCount] = mFidMarkSide[i].conv;
                markCount++;
            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                //  Top Azimuth = 0,1,2,3  :: corresponds to Side 0,1,4,5

                //  mFidMarkTop[i].Azimuth
                bool yUpper = true;
                bool IsLeft = true;
                if (mFidMarkTop[i].Azimuth < 2)
                {
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth];
                    IsLeft = sideAzimuthLeft[mFidMarkTop[i].Azimuth];
                }
                else
                {
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth + 2];
                    IsLeft = sideAzimuthLeft[mFidMarkTop[i].Azimuth + 2];
                }

                TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, yUpper, false, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry + 0.4999);
                markConv[markCount] = mFidMarkSide[i].conv;
                markCount++;

            }
            if (markCount > 0)
            {
                mMarkPosOnPanel = new System.Drawing.Point[markCount];
                mMarkConvOnPanel = new long[markCount];

                StreamWriter wr = new StreamWriter(RootPath + "markPosOnPanel.csv");
                for (int i = 0; i < markPos.Length; i++)
                {
                    mMarkPosOnPanel[i].X = markPos[i].X;
                    mMarkPosOnPanel[i].Y = markPos[i].Y;
                    wr.WriteLine(markPos[i].X.ToString() + "," + markPos[i].Y.ToString());
                }
                wr.Close();
            }
        }
        public string GetFileNameOfMarkPosOnPanel()
        {
            return RootPath + "markPosOnPanel.csv";
        }

        public void TransferAbsToPicturePos(double x, double y, ref double rx, ref double ry, bool yUpper = true, bool IsSideView = true, bool IsLeft = true)
        {
            if (IsLeft)
                rx = -x / (0.0055 / LensMag);
            else
                rx = +x / (0.0055 / LensMag);

            //  mOpticsTgtOffset, mOpticsTopViewOffset 는 모델에 따라 달라지는 값인가?

            double S = mFidMarkSide[0].planeShift / 1000.0; //  -365 가 저장되어있어야 한다.
            double planeHeight = mFidMarkSide[0].planeHeight / 1000.0;
            double Oh = 1.95 + planeHeight / vTan40;

            if (IsSideView)
            {
                if (yUpper)
                {
                    //ry = (  (-mOpticsTgtOffset - y) * vSin40 - motSimDlg.FOV_YSHIFT * vSin40);
                    ry = ( - motSimDlg.FOV_YSHIFT - (y + S) * vSin40);
                }
                else
                {
                    //ry = ((-mOpticsTgtOffset + y) * vSin40 - motSimDlg.FOV_YSHIFT * vSin40);
                    ry = ( - motSimDlg.FOV_YSHIFT + (y - S) * vSin40);
                }

                ry = ry / (0.0055 / LensMag);
            }
            else
            {
                if (yUpper)
                {
                    //ry = (motSimDlg.M + mOpticsTopViewOffset - mOpticsTgtOffset - y);
                    ry = - motSimDlg.FOV_YSHIFT + motSimDlg.M + Oh - y - S;
                }
                else
                {
                    //ry = (motSimDlg.M + mOpticsTopViewOffset - mOpticsTgtOffset + y);  // 실제로는 발생할 수 없는 Case
                    ry = - motSimDlg.FOV_YSHIFT + motSimDlg.M + Oh + y - S;
                }

                ry = ry / (0.0055 / LensMag);
            }
        }

        //  Filtering Image
        public static byte[,] applyFilterMask(byte[,] imageArray, int filterSize, int[,] filterMask, uint width, uint height)
        {
            byte[,] masked2DImage = new byte[height, width];
            for (int row = (filterSize / 2); row < height - (filterSize / 2); row++)
            {
                for (int col = (filterSize / 2); col < width - (filterSize / 2); col++)
                {
                    int filterResult = 0;

                    // these nested loops are to apply (generic) filter mask
                    for (int filterRow = 0; filterRow < filterMask.GetLength(0); filterRow++)
                    {
                        for (int filterColumn = 0; filterColumn < filterMask.GetLength(1); filterColumn++)
                        {
                            filterResult += imageArray[row - (filterSize / 2) + filterRow, col - (filterSize / 2) + filterColumn] * filterMask[filterRow, filterColumn];
                        }
                    }

                    filterResult = sumOfFilterMask(filterMask) == 0 ? filterResult : (filterResult / sumOfFilterMask(filterMask));
                    filterResult = filterResult > 255 ? 255 : (filterResult < 0 ? 0 : filterResult);
                    masked2DImage[row, col] = Convert.ToByte(filterResult);
                }
            }

            return masked2DImage;
        }
        public static int sumOfFilterMask(int[,] filterMask)
        {
            int res = 0;
            int ie = filterMask.GetLength(0);
            int je = filterMask.GetLength(1);
            for (int i = 0; i < ie; i++)
                for (int j = 0; j < je; j++)
                    res += filterMask[i, j];
            return res;
        }
        public static Bitmap Create24bpp(Image image, System.Drawing.Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(bmp))
                gr.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height));

            return bmp;
        }

        public void ToGrayScale(Mat src, Mat dest)
        {
            //Mat grayImg = new Mat();

            Cv2.CvtColor(src, dest, ColorConversionCodes.BGR2GRAY);
            //Cv2.BitwiseNot(grayImg, dest);
        }

        public void ToJustGrayScale(Mat src, Mat dest)
        {
            Cv2.CvtColor(src, dest, ColorConversionCodes.BGR2GRAY);
        }

        public static Bitmap ToGrayscale(Bitmap bmp, int nColor = 0)
        {
            var result = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format8bppIndexed);

            BitmapData data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // Copy the bytes from the image into a byte array
            byte[] bytes = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            var rgb = (byte)0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    switch (nColor)
                    {
                        case 0:
                            rgb = (byte)(255 - (byte)((c.R + c.G + c.B) / 3));
                            break;
                        case 1:
                            rgb = (byte)(c.R);
                            break;
                        case 2:
                            rgb = (byte)(c.G);
                            break;
                        case 3:
                            rgb = (byte)(c.B);
                            break;
                    }
                    //var rgb = (byte)(c.R);
                    //var rgb = (byte)(c.G);
                    //var rgb = (byte)(c.B);

                    bytes[x + data.Stride * y] = rgb;
                    //bytes[x + bmp.Width * y] = rgb;
                }
            }

            // Copy the bytes from the byte array into the image
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            result.UnlockBits(data);

            return result;
        }

        public void GetHistogramData(Mat src, ref float[] lHisto)
        {
            //int[] lHisto = new int[256];
            int[] hist_size = new int[1] { 256 };
            Rangef[] ranges = { new Rangef(0, 256), }; // min/max
            Mat hist = new Mat();

            Cv2.CalcHist(new Mat[] { src }, new int[] { 0 }, null, hist, 1, hist_size, ranges);

            hist.GetArray(out lHisto);
            //for (int i = 0; i < 256; i++)
            //{
            //    lHisto[i] = (int)(hist.Get<float>(i));
            //}
        }

        //public int SaveModelData(Mat src)
        //{
        //    double min = 0;
        //    double max = 0;
        //    byte bsrc = 0;

        //    src.SaveImage("BaseModel_" + mBaseModelCnt.ToString() + ".png");

        //    int[] lHisto = new int[256];
        //    int[] hist_size = new int[1] { 256 };
        //    Rangef[] ranges = { new Rangef(0, 256), }; // min/max
        //    Mat hist = new Mat();

        //    Cv2.CalcHist(new Mat[] { src }, new int[] { 0 }, null, hist, 1, hist_size, ranges);

        //    int totalpixel = src.Width * src.Height;
        //    double lowSum = 0;
        //    int k = 0;

        //    int IgnoreUnder = -1;
        //    int NegativeUnder = -1;
        //    int SaturateOver = 255;
        //    double lfillfactor = 0;
        //    for (int i = 0; i < 256; i++)
        //    {
        //        lHisto[i] = (int)(hist.Get<float>(i));
        //        lowSum += lHisto[i];
        //        if (i == 95)
        //            lfillfactor = 1 - lowSum / totalpixel;

        //        if ( i> 4 && i<255 && mBaseModel[mBaseModelCnt].IgnoreUnder == 0)
        //        {
        //            if (lHisto[i - 4] > lHisto[i - 2] && lHisto[i - 2] < lHisto[i] && lHisto[i - 3] > lHisto[i - 2] && lHisto[i - 2] < lHisto[i - 1])
        //                mBaseModel[mBaseModelCnt].IgnoreUnder = i - 1;
        //        }

        //        if (lowSum / totalpixel > 0.3 && IgnoreUnder < 0)
        //            IgnoreUnder = i - 1;

        //        if (lowSum / totalpixel > 0.75 && NegativeUnder < 0)
        //            NegativeUnder = i - 1;

        //        if (lowSum / totalpixel > 0.98 && SaturateOver ==255)
        //        {
        //            SaturateOver = i - 1;
        //            break;
        //        }
        //    }
        //    mBaseModel[mBaseModelCnt].IgnoreUnder = IgnoreUnder;
        //    mBaseModel[mBaseModelCnt].NegativeUnder = NegativeUnder;   //  이 값 이하는 Convolution 시 음수처리
        //    mBaseModel[mBaseModelCnt].SaturateOver = SaturateOver;    //  이 값 이상은 Convolution 시 (i-1) 로 처리
        //    mBaseModel[mBaseModelCnt].fillfactor = lfillfactor;

        //    mBaseModel[mBaseModelCnt].img = new byte[src.Width * src.Height];

        //    src.GetArray(out mBaseModel[mBaseModelCnt].img );
        //    Moments mmts = src.Moments();
        //    mBaseModel[mBaseModelCnt].nu02 = mmts.Nu02;
        //    mBaseModel[mBaseModelCnt].nu20 = mmts.Nu20;
        //    mBaseModel[mBaseModelCnt].nu11 = mmts.Nu11;
        //    mBaseModel[mBaseModelCnt].eccent = ((mmts.Nu02 - mmts.Nu20) * (mmts.Nu02 - mmts.Nu20) - 4 * mmts.Nu11 * mmts.Nu11) / ((mmts.Nu02 + mmts.Nu20) * (mmts.Nu02 + mmts.Nu20));
        //    mBaseModel[mBaseModelCnt].theta = 0.5 * Math.Atan(2 * mmts.Nu11 / (mmts.Nu02 - mmts.Nu20));
        //    double sum = 0;
        //    double mmdist = 0;
        //    int shiftx = 0, shifty = 0;
        //    CalcConvolutionModel2Model(mBaseModelCnt, mBaseModelCnt, ref sum, ref mmdist, ref shiftx, ref shifty);
        //    mBaseModel[mBaseModelCnt].sConv = sum;
        //    mBaseModel[mBaseModelCnt].sDist = mmdist;

        //    //////////////////////////////////////////////////////////////
        //    //  4단계로 픽셀을 구분했을 때 보이는 그림 확인해보기.
        //    //Mat verifyImg = new Mat();
        //    //src.CopyTo(verifyImg);

        //    //byte[] data = new byte[src.Width * src.Height];
        //    //for (int i = 0; i < src.Width; i++)
        //    //{
        //    //    for (int j = 0; j < src.Height; j++)
        //    //    {
        //    //        if (mBaseModel[mBaseModelCnt].img[i + j * src.Width] < mBaseModel[mBaseModelCnt].IgnoreUnder)
        //    //            data[i + j * src.Width] = 0;
        //    //        else if (mBaseModel[mBaseModelCnt].img[i + j * src.Width] < mBaseModel[mBaseModelCnt].NegativeUnder)
        //    //            data[i + j * src.Width] = 85;
        //    //        else if (mBaseModel[mBaseModelCnt].img[i + j * src.Width] < mBaseModel[mBaseModelCnt].SaturateOver)
        //    //            data[i + j * src.Width] = 170;
        //    //        else
        //    //            data[i + j * src.Width] = 255;
        //    //    }
        //    //}
        //    //verifyImg.SetArray(data);
        //    //Mat tmpImg = new Mat();
        //    //Cv2.Resize(verifyImg, tmpImg, new OpenCvSharp.Size(verifyImg.Width * 10, verifyImg.Height * 10), 0, 0, InterpolationFlags.Area);

        //    //tmpImg.SaveImage("BaseModel_" + mBaseModelCnt.ToString() + ".png");
        //    //////////////////////////////////////////////////////////////

        //    return mBaseModelCnt;
        //}

        //public void SaveLearnedModelData(int lmIndex )
        //{
        //    int[] lHisto = new int[256];
        //    int[] hist_size = new int[1] { 256 };
        //    Rangef[] ranges = { new Rangef(0, 256), }; // min/max

        //    //  Histogram of
        //    mLearnedModel[lmIndex].Height = mBaseModel[0].Height;
        //    mLearnedModel[lmIndex].Width = mBaseModel[0].Width;
        //    int totalpixel = mLearnedModel[lmIndex].img.Length;
        //    double lfillfactor = 0;

        //    for (int i = 0; i < totalpixel; i++)
        //        lHisto[mLearnedModel[lmIndex].img[i]]++;

        //    double lowSum = 0;
        //    int k = 0;
        //    int IgnoreUnder = -1;
        //    int NegativeUnder = -1;
        //    int SaturateOver = 255;
        //    for (int i = 0; i < 256; i++)
        //    {
        //        lowSum += lHisto[i];
        //        if (i == 95)
        //            lfillfactor = 1 - lowSum / ( (mLearnedModel[lmIndex].Height / 2) * (mLearnedModel[lmIndex].Width / 2) );

        //        if (i > 4 && i < 255 && mLearnedModel[lmIndex].IgnoreUnder == 0)
        //        {
        //            if (lHisto[i - 4] > lHisto[i - 2] && lHisto[i - 2] < lHisto[i] && lHisto[i - 3] > lHisto[i - 2] && lHisto[i - 2] < lHisto[i - 1])
        //                mLearnedModel[lmIndex].IgnoreUnder = i - 1;
        //        }
        //        if (lowSum / totalpixel > 0.3 && IgnoreUnder < 0)
        //            IgnoreUnder = i - 1;

        //        if (lowSum / totalpixel > 0.75 && NegativeUnder < 0)
        //            NegativeUnder = i - 1;

        //        if (lowSum / totalpixel > 0.99 && SaturateOver == 255)
        //        {
        //            SaturateOver = i - 1;
        //            break;
        //        }

        //    }
        //    mLearnedModel[lmIndex].fillfactor = lfillfactor;
        //    mLearnedModel[lmIndex].IgnoreUnder = IgnoreUnder;
        //    mLearnedModel[lmIndex].NegativeUnder = NegativeUnder;   //  이 값 이하는 Convolution 시 음수처리
        //    mLearnedModel[lmIndex].SaturateOver = SaturateOver;    //  이 값 이상은 Convolution 시 (i-1) 로 처리
        //    Mat src = new Mat(mLearnedModel[lmIndex].Height/2, mLearnedModel[lmIndex].Width/2, MatType.CV_8UC1, mLearnedModel[lmIndex].img);

        //    Moments mmts = src.Moments();
        //    mLearnedModel[lmIndex].nu02 = mmts.Nu02;
        //    mLearnedModel[lmIndex].nu20 = mmts.Nu20;
        //    mLearnedModel[lmIndex].nu11 = mmts.Nu11;
        //    mLearnedModel[lmIndex].eccent = ((mmts.Nu02 - mmts.Nu20) * (mmts.Nu02 - mmts.Nu20) - 4 * mmts.Nu11 * mmts.Nu11) / ((mmts.Nu02 + mmts.Nu20) * (mmts.Nu02 + mmts.Nu20)); 
        //    //mLearnedModel[lmIndex].fillfactor = Math.Sqrt(0.5 * (mmts.Nu02 + mmts.Nu20) - Math.Sqrt(4 * mmts.Nu11 * mmts.Nu11 - (mmts.Nu02 - mmts.Nu20) * (mmts.Nu02 - mmts.Nu20)));
        //    mLearnedModel[lmIndex].theta = 0.5 * Math.Atan(2 * mmts.Nu11 / (mmts.Nu02 - mmts.Nu20));
        //    double sConv = 0;
        //    double sDist = 0;
        //    int shiftx = 0, shifty = 0;

        //    CalcConvolutionModel2Model(lmIndex, lmIndex, ref sConv, ref sDist, ref shiftx, ref shifty);
        //    mLearnedModel[lmIndex].sConv = sConv;
        //    mLearnedModel[lmIndex].sDist = sDist;

        //    ////////////////////////////////////////////////////////////
        //    //  4단계로 픽셀을 구분했을 때 보이는 그림 확인해보기.
        //    //Mat verifyImg = new Mat();
        //    //src.CopyTo(verifyImg);

        //    //byte[] data = new byte[src.Width * src.Height];
        //    //for (int i = 0; i < src.Width; i++)
        //    //{
        //    //    for (int j = 0; j < src.Height; j++)
        //    //    {
        //    //        if (mLearnedModel[mLearnedModelCnt].img[i + j * src.Width] < mLearnedModel[mLearnedModelCnt].IgnoreUnder)
        //    //            data[i + j * src.Width] = 0;
        //    //        else if (mLearnedModel[mLearnedModelCnt].img[i + j * src.Width] < mLearnedModel[mLearnedModelCnt].NegativeUnder)
        //    //            data[i + j * src.Width] = 85;
        //    //        else if (mLearnedModel[mLearnedModelCnt].img[i + j * src.Width] < mLearnedModel[mLearnedModelCnt].SaturateOver)
        //    //            data[i + j * src.Width] = 170;
        //    //        else
        //    //            data[i + j * src.Width] = 255;
        //    //    }
        //    //}
        //    //verifyImg.SetArray(data);
        //    //Mat tmpImg = new Mat();
        //    //Cv2.Resize(verifyImg, tmpImg, new OpenCvSharp.Size(verifyImg.Width, verifyImg.Height), 0, 0, InterpolationFlags.Area);
        //    //tmpImg.SetArray(mLearnedModel[lmIndex].img);
        //    string  strLearnedFile = "LearnedModel_" + lmIndex.ToString() + ".png";
        //    src.SaveImage(strLearnedFile);

        //    mLearnedModelCnt++;
        //}

        //void CalcConvolutionSpl2LearnedModel(Mat Spl, int modelIndex, ref double sum, ref double mmdist)
        //{
        //    //Mat model = new Mat();
        //    //Mat conv = new Mat();

        //    //src.CopyTo(model);
        //    //model.SetArray(mBaseModel[modelIndex].img);
        //    byte[] srcData;
        //    Spl.GetArray(out srcData);
        //    int data = 0;
        //    int model = 0;

        //    for (int i = 0; i < Spl.Width; i++)
        //    {
        //        for (int j = 0; j < Spl.Height; j++)
        //        {
        //            data = srcData[i + j * Spl.Width];
        //            model = mLearnedModel[modelIndex].img[i + j * Spl.Width];
        //            if (data > mLearnedModel[modelIndex].SaturateOver)
        //                data = mLearnedModel[modelIndex].SaturateOver;
        //            if (data > mLearnedModel[modelIndex].SaturateOver)
        //                model = mLearnedModel[modelIndex].SaturateOver;

        //            if (model < mLearnedModel[modelIndex].IgnoreUnder && mIgnoreDark )
        //                continue;
        //            if (i + j < 5 || i + j > Spl.Width + Spl.Height - 7)    //  좌상, 우하 모서리 무시
        //                continue;
        //            if ((Spl.Width - i + j) < 5 || (Spl.Width - i + j) > Spl.Width + Spl.Height - 7)    //  우상, 좌하 모서리 무시
        //                continue;
        //            sum += (data - mLearnedModel[modelIndex].NegativeUnder) * (model - mLearnedModel[modelIndex].NegativeUnder);
        //        }
        //    }
        //    sum = sum / (Spl.Width * Spl.Height);   //  Normalize
        //    //Moments mmts = Spl.Moments();
        //    //double eccent = ((mmts.Nu02 - mmts.Nu20) * (mmts.Nu02 - mmts.Nu20) - 4 * mmts.Nu11 * mmts.Nu11) / ((mmts.Nu02 + mmts.Nu20) * (mmts.Nu02 + mmts.Nu20));
        //    //double theta = 0.5 * Math.Atan(2 * mmts.Nu11 / (mmts.Nu02 - mmts.Nu20));

        //    //double dtheta = theta - mLearnedModel[modelIndex].theta;
        //    //if (dtheta > 0.25 * 3.1415927)
        //    //    dtheta = (0.5 * 3.1415927 - dtheta) / (0.25 * 3.1415927);

        //    //mmdist = Math.Sqrt((eccent - mLearnedModel[modelIndex].eccent) * (eccent - mLearnedModel[modelIndex].eccent) + dtheta * dtheta);
        //}
        void CalcConvolutionSpl2Model(Mat Spl, int modelIndex, ref double sum, ref double mmdist)
        {
        }
        //void CalcConvolutionModel2Model(int m1, int m2, ref double sum, ref double mmdist, ref int shiftX, ref int shiftY)
        //{
        //    int data = 0;
        //    int model = 0;
        //    int lwidth = 7;// mBaseModel[m1].Width / 2;
        //    int lheight = 7;// mBaseModel[m1].Height / 2;
        //    int i = 0;
        //    int i2 = 0;
        //    double trysum = 0;
        //    double maxsum = 0;
        //    int maxsx=0, maxsy = 0;

        //    for ( int sx = -3; sx<4; sx++ )
        //    {
        //        for (int sy = -3; sy < 4; sy++)
        //        {
        //            maxsum = 0;
        //            trysum = 0;
        //            for (int xi = 0; xi < lwidth; xi++)
        //            {
        //                for (int yj = 0; yj < lheight; yj++)
        //                {
        //                    if (xi + sx >= lwidth || xi + sx < 0) continue;
        //                    if (yj + sy >= lheight || yj + sy < 0) continue;
        //                    i = xi + yj * lwidth;
        //                    i2 = xi + sx + (yj + sy) * lwidth;
        //                    data = mBaseModel[m1].img[i];
        //                    model = mBaseModel[m2].img[i2];
        //                    if ((model < mBaseModel[m2].IgnoreUnder || model < mBaseModel[m2].IgnoreUnder) && mIgnoreDark)
        //                        continue;

        //                    if (data < mBaseModel[m1].NegativeUnder && model < mBaseModel[m2].NegativeUnder)
        //                        trysum += data * model;
        //                    else if (data < mBaseModel[m1].NegativeUnder ^ model < mBaseModel[m2].NegativeUnder)
        //                        trysum -= data * model;
        //                    else
        //                    {
        //                        if (data > mBaseModel[m1].SaturateOver)
        //                            data = mBaseModel[m1].SaturateOver;
        //                        if (model > mBaseModel[m2].SaturateOver)
        //                            model = mBaseModel[m2].SaturateOver;
        //                        if (model < mBaseModel[m1].IgnoreUnder && mIgnoreDark)
        //                            continue;

        //                        trysum += data * model;
        //                    }
        //                }
        //            }
        //            if (trysum > maxsum)
        //            {
        //                maxsum = trysum;
        //                maxsx = sx;
        //                maxsy = sy;
        //            }
        //        }
        //    }
        //    sum = maxsum / mBaseModel[m1].img.Length;   //  Normalize
        //    shiftX = maxsx;
        //    shiftY = maxsy;
        //    mmdist = mBaseModel[m1].fillfactor - mBaseModel[m2].fillfactor;
        //    //double dtheta = mBaseModel[m1].theta - mBaseModel[m2].theta;
        //    //if (dtheta > 0.25 * 3.1415927)
        //    //    dtheta = (0.5 * 3.1415927 - dtheta) / (0.25 * 3.1415927);

        //    //mmdist = Math.Sqrt((mBaseModel[m1].eccent - mBaseModel[m2].eccent) * (mBaseModel[m1].eccent - mBaseModel[m2].eccent) + dtheta * dtheta);
        //    //mmdist = Math.Sqrt((mBaseModel[m1].eccent - mBaseModel[m2].eccent) * (mBaseModel[m1].eccent - mBaseModel[m2].eccent) + (mBaseModel[m1].fillfactor - mBaseModel[m2].fillfactor) * (mBaseModel[m1].fillfactor - mBaseModel[m2].fillfactor) + dtheta * dtheta);
        //}

        public void SegmentedBinStretching(Mat src, ref Mat dest, int[] xArray, int[] yArray, bool IsAuto = true)
        {
            byte[] bytes;
            src.GetArray(out bytes);
            //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            //byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            int[] rx = new int[yArray.Length];
            int[] ry = new int[yArray.Length];
            int stepX = src.Width / (xArray.Length - 1);
            int stepY = src.Height / (yArray.Length - 1);
            int[] lHisto = new int[256];
            int[] hist_size = new int[1] { 256 };
            Rangef[] ranges = { new Rangef(0, 256), }; // min/max
            Mat hist = new Mat();

            Cv2.CalcHist(new Mat[] { src }, new int[] { 0 }, null, hist, 1, hist_size, ranges);

            int totalpixel = src.Width * src.Height;
            double lowSum = 0;
            //int k = 0;
            byte limitMin = 0;
            byte limitMax = 0;
            for (int i = 0; i < 256; i++)
            {
                lHisto[i] = (int)(hist.Get<float>(i));
                lowSum += lHisto[i];
                if (lowSum / totalpixel > 0.12 && limitMin == 0)
                    limitMin = (byte)(i - 1);

                if (i > 2)
                {
                    if (lHisto[i - 3] + lHisto[i - 2] > lHisto[i - 1] + lHisto[i])
                    {
                        if (limitMax == 0) limitMax = (byte)(i + 7);
                    }
                }
            }


            if (IsAuto)
            {
                for (int i = 0; i < xArray.Length - 1; i++)
                    rx[i] = stepX * i;
                for (int i = 0; i < yArray.Length - 1; i++)
                    ry[i] = stepY * i;

                rx[xArray.Length - 1] = src.Width;
                ry[yArray.Length - 1] = src.Height;
            }
            else
            {
                rx = xArray;
                ry = yArray;
            }

            byte min = 0;
            byte max = 0;

            for (int i = 0; i < rx.Length - 1; i++)
            {
                for (int j = 0; j < ry.Length - 1; j++)
                {
                    min = 255;
                    max = 0;
                    for (int y = ry[j]; y < ry[j + 1]; y++)
                    {
                        for (int x = rx[i]; x < rx[i + 1]; x++)
                        {
                            byte c = bytes[x + src.Width * y];
                            if (min > c)
                                min = c;
                            if (max < c)
                                max = c;
                        }
                    }
                    if (min < limitMin)
                        min = limitMin;

                    if (max < limitMax)
                        max = limitMax;

                    for (int y = ry[j]; y < ry[j + 1]; y++)
                    {
                        for (int x = rx[i]; x < rx[i + 1]; x++)
                        {
                            byte c = bytes[x + src.Width * y];
                            if (c <= min + 1)
                                bytes[x + src.Width * y] = 0;
                            else if (c >= max - 1)
                                bytes[x + src.Width * y] = 255;
                            else
                                bytes[x + src.Width * y] = (byte)((255 * (c - min)) / (max - min - 2));
                        }
                    }
                }
            }

            // Copy the bytes from the byte array into the image
            //Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            //image.UnlockBits(data);
            //image.Save("tmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //dest = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
            src.CopyTo(dest);
            dest.SetArray(bytes);
        }

        public void RemoveBlob(Mat src, Mat dest)
        {
            //byte[] bytes;
            //src.GetArray(out bytes);

            ////Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            ////BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            ////byte[] bytes = new byte[data.Height * data.Stride];
            ////Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            //var bin = (byte)0;

            //// 가로 10 x 세로 20 영역에서 90% 이상이 128 이상인 경우 
            ////  해당 영역

            //for (int y = 0; y < src.Height; y++)
            //{
            //    for (int x = 0; x < src.Width; x++)
            //    {
            //        bin = bytes[x + src.Width * y];
            //    }
            //}

            // Copy the bytes from the byte array into the image
            //Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            //image.UnlockBits(data);
            //image.Save("tmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            ////dest = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
            //dest = new Mat("tmp.bmp");
        }
        public void BlobMatching(Mat src, Mat dest, Mat target)
        {
            var binaryImage = new Mat(src.Size(), MatType.CV_8UC1);


        }
        public void RotateImage(Mat src, Mat dest, double angleDeg)
        {
            //double angle = 0;
            //if (textBox1.Text.Length > 0)
            //    angle = Convert.ToDouble(textBox1.Text);

            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(src.Width / 2, src.Height / 2), angleDeg, 1.0);
            Cv2.WarpAffine(src, dest, matrix, new OpenCvSharp.Size(src.Width, src.Height));
        }

        public int[] mStrokeRefX = null;
        public int[] mStrokeRefY = null;

        public double[] mSpaceRatioX = new double[1000];
        public double[] mSpaceRatioY = new double[1000];
        public double[] mSpaceAvgX = new double[1000];
        public double[] mSpaceAvgY = new double[1000];
        public double[] mFillFactorAvg = new double[1000];
        public double[] mFillFactorStdev = new double[1000];


        //public int SearchTrainingModel(Mat src, bool NoThreading = true)
        //{
        //    int TrainingLength = 0;
        //    if (mTrainingModel == null)
        //    {
        //        TrainingLength = LoadTrainingModel();
        //        mTrainingLength = TrainingLength;
        //        if (TrainingLength < 1) return 0;
        //    }
        //    else
        //    {
        //        TrainingLength = mTrainingLength;
        //        if (TrainingLength < 1) return 0;
        //    }

        //    mOverlayedImg = new Mat();
        //    Cv2.CvtColor(src, mOverlayedImg, ColorConversionCodes.GRAY2RGB);

        //    //  Scan to make partial image to compare training modelㄹ
        //    int xStep = 2;
        //    int yStep = 2;
        //    int xMax = 0;
        //    int xMin = 0;
        //    Mat ImgGray = new Mat();
        //    Mat Img742 = new Mat();
        //    Mat Img742Dest = new Mat();
        //    double lFillFactor = 0;

        //    int[] csumCnt = new int[TrainingLength];
        //    int[] csumEffCnt = new int[TrainingLength];
        //    double[] csum2 = new double[TrainingLength];
        //    double[] csumSum = new double[TrainingLength];
        //    double[] convSum = new double[TrainingLength];
        //    double[][] csumRanking = new double[TrainingLength][];
        //    double[] csumStdev = new double[TrainingLength];
        //    double[] csumAvg = new double[TrainingLength];
        //    double[] Threshold = new double[TrainingLength];

        //    for (int i = 0; i < TrainingLength; i++)
        //        Threshold[i] = 1.0;

        //    for (int mod = 0; mod < TrainingLength; mod++)
        //        csumRanking[mod] = new double[50];


        //    TrainingLength = 6;

        //    if (NoThreading)
        //    {
        //        bool skipNext = false;
        //        xStep = 2;
        //        yStep = 2;
        //        skipNext = false;
        //        for (int mod = 0; mod < TrainingLength; mod++)
        //        {
        //            mDetectedModel[mod] = 0;
        //            for (int i = 1; i < mStrokeRefX.Length - 2; i++)
        //            {
        //                if (mStrokeRefX[i] < src.Width / 10) continue;
        //                if (mStrokeRefX[i] > src.Width - 100) break;
        //                if (skipNext)
        //                {
        //                    skipNext = false;
        //                    continue;
        //                }
        //                if (mStrokeRefX[i + 1] - mStrokeRefX[i] < 40)
        //                {
        //                    xMin = (mStrokeRefX[i + 1] + mStrokeRefX[i]) / 2 - 10;
        //                    xMax = xMin + 20;
        //                    skipNext = true;
        //                }
        //                else
        //                {
        //                    xMin = mStrokeRefX[i] - 10;
        //                    xMax = xMin + 20;
        //                }

        //                for (int xi = xMin; xi < xMax; xi += xStep)
        //                {
        //                    for (int yi = mStrokeRefY[1] + 10; yi < (src.Height - 200); yi += yStep)
        //                    {

        //                        if (yi > src.Height - 200) break;
        //                        Rect roi = new Rect(xi, yi, mTrainingModel[0].width, mTrainingModel[0].height);
        //                        Mat subImg = src.SubMat(roi);
        //                        Cv2.Resize(subImg, Img742Dest, new OpenCvSharp.Size(FWIDTH, FHEIGHT), 0, 0, InterpolationFlags.Area);
        //                        BinStretching(Img742Dest, ref Img742, ref lFillFactor);

        //                        csumCnt[mod]++;
        //                        if (lFillFactor < 0.7 * mTrainingModel[mod].fillfactor || lFillFactor > 1.5 * mTrainingModel[mod].fillfactor)
        //                        {
        //                            yi += mTrainingModel[mod].height / 2;
        //                            continue;
        //                        }

        //                        EvaluateConv(Img742, ref mTrainingModel[mod].data, ref convSum[mod], mod);
        //                        double convR = convSum[mod] / mTrainingModel[mod].convSum;

        //                        if (convR > Threshold[mod])
        //                        {
        //                            //mOverlayedImg.Rectangle(roi, Scalar.Cyan, 2);
        //                            yi += mTrainingModel[mod].height - yStep;
        //                            if (mDetectedModel[mod] < 100)
        //                                mDetectedRect[mod][mDetectedModel[mod]++] = roi;
        //                        }
        //                        else if (convR < (Threshold[mod] / 2) && convR > 0)
        //                            yi += mTrainingModel[mod].height / 2;
        //                        else if (convR < 0)
        //                            yi += mTrainingModel[mod].height - yStep;

        //                        if (convR > csumRanking[mod][0])
        //                        {
        //                            csumRanking[mod][0] = convR;
        //                            Array.Sort(csumRanking[mod]);
        //                        }
        //                        csum2[mod] += convR * convR;
        //                        csumSum[mod] += convR;
        //                        csumEffCnt[mod]++;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Thread[] threadList = new Thread[TrainingLength];
        //        for ( int i=0; i< TrainingLength; i++)
        //        {
        //            mIsRunModel[i] = false;
        //            threadList[i] = new Thread(() => {
        //                SearchSingleModel(src, i, ref  csumCnt[i], ref  csumEffCnt[i], ref  csum2[i], ref  csumSum[i], ref  convSum[i], ref csumRanking[i], ref csumStdev[i], ref csumAvg[i], ref Threshold[i]);
        //                });
        //            threadList[i].Start();
        //        }
        //        Thread.Sleep(500);
        //        int nFinish = 0;
        //        while(true)
        //        {
        //            nFinish = 0;
        //            for (int i = 0; i < TrainingLength; i++)
        //            {
        //                if (!mIsRunModel[i]) nFinish++;
        //            }
        //            if (nFinish == TrainingLength)
        //                break;
        //            Thread.Sleep(50);
        //        }
        //    }

        //    int res = 0;
        //    for (int i = 0; i < TrainingLength; i++)
        //    {
        //        for ( int j=0; j< mDetectedModel[i]; j++)
        //        {
        //            if ( i< (TrainingLength-1) )
        //            {
        //                for (int k = i + 1; k < TrainingLength; k++)
        //                {
        //                    for ( int l=0; l< mDetectedModel[k]; l++ )
        //                    {
        //                        if (mDetectedRect[i][j].X == 0) continue;
        //                        if ( Math.Abs(mDetectedRect[i][j].X - mDetectedRect[k][l].X)<4 && Math.Abs(mDetectedRect[i][j].Y - mDetectedRect[k][l].Y) < 8)
        //                        {
        //                            mDetectedRect[k][l].X = 0;
        //                            mDetectedRect[k][l].Y = 0;
        //                        }
        //                    }
        //                }
        //            }
        //            if (mDetectedRect[i][j].X>0)
        //            {
        //                mOverlayedImg.Rectangle(mDetectedRect[i][j], Scalar.Cyan, 2);
        //                res++;
        //            }
        //        }
        //    }


        //    for ( int mod = 0; mod < TrainingLength; mod++)
        //    {
        //        csumAvg[mod] = (csumSum[mod] / csumCnt[mod]);
        //        csumStdev[mod] = Math.Sqrt(csum2[mod] / csumCnt[mod] - csumAvg[mod] * csumAvg[mod]);
        //    }

        //    StreamWriter wr = new StreamWriter("EvalResult.csv");
        //    string llstr = "";
        //    for (int mod = 0; mod < TrainingLength; mod++)
        //        llstr += csumAvg[mod].ToString("F3") + ",";

        //    wr.WriteLine("ConvSum Avg," + llstr);
        //    llstr = "";
        //    for (int mod = 0; mod < TrainingLength; mod++)
        //        llstr += csumStdev[mod].ToString("F3") + ",";

        //    wr.WriteLine("ConvSum Stdev," + llstr);
        //    for ( int i=0; i<50; i++)
        //    {
        //        llstr = "";
        //        for (int mod = 0; mod < TrainingLength; mod++)
        //            llstr += csumRanking[mod][49-i].ToString("F3") + ",";
        //        wr.WriteLine("Rank " + (50-i).ToString() + "," + llstr);
        //    }
        //    wr.Close();
        //    return res;
        //}

        //int[] csumCnt = new int[TrainingLength];
        //int[] csumEffCnt = new int[TrainingLength];
        //double[] csum2 = new double[TrainingLength];
        //double[] csumSum = new double[TrainingLength];
        //double[] eVal = new double[TrainingLength];
        //double[] convSum = new double[TrainingLength];
        //double[][] csumRanking = new double[TrainingLength][];
        //double[] csumStdev = new double[TrainingLength];
        //double[] csumAvg = new double[TrainingLength];
        //double[] Threshold = new double[TrainingLength];

        //public void SearchSingleModel(Mat orgSrc, int mod, ref int csumCnt, ref int csumEffCnt, ref double csum2, ref double csumSum, ref double convSum, ref double[] csumRanking, ref double csumStdev, ref double csumAvg, ref double Threshold)
        //{
        //    mIsRunModel[mod] = true;
        //    mDetectedModel[mod] = 0;

        //    bool skipNext = false;
        //    int xStep = 2;
        //    int yStep = 2;
        //    int xMax = 0;
        //    int xMin = 0;
        //    int yMax = 0;

        //    Mat src = new Mat();
        //    orgSrc.CopyTo(src);

        //    Mat ImgGray = new Mat();
        //    Mat Img742 = new Mat();
        //    Mat Img742Dest = new Mat();
        //    double lFillFactor = 0;

        //    skipNext = false;
        //    for (int i = 1; i < mStrokeRefX.Length - 2; i++)
        //    {
        //        if (mStrokeRefX[i] < src.Width / 10) continue;
        //        if (mStrokeRefX[i] > src.Width - 100) break;
        //        if (skipNext)
        //        {
        //            skipNext = false;
        //            continue;
        //        }
        //        if (mStrokeRefX[i + 1] - mStrokeRefX[i] < 40)
        //        {
        //            xMin = (mStrokeRefX[i + 1] + mStrokeRefX[i]) / 2 - 10;
        //            xMax = xMin + 20;
        //            skipNext = true;
        //        }
        //        else
        //        {
        //            xMin = mStrokeRefX[i] - 10;
        //            xMax = xMin + 20;
        //        }

        //        for (int xi = xMin; xi < xMax; xi += xStep)
        //        {
        //            for (int yi = mStrokeRefY[1] + 10; yi < (src.Height - 200); yi += yStep)
        //            {

        //                if (yi > src.Height - 200) break;
        //                Rect roi = new Rect(xi, yi, mTrainingModel[0].width, mTrainingModel[0].height);
        //                Mat subImg = src.SubMat(roi);
        //                Cv2.Resize(subImg, Img742Dest, new OpenCvSharp.Size(FWIDTH, FHEIGHT), 0, 0, InterpolationFlags.Area);
        //                BinStretching(Img742Dest, ref Img742, ref lFillFactor);

        //                csumCnt++;
        //                if (lFillFactor < 0.7 * mTrainingModel[mod].fillfactor || lFillFactor > 1.5 * mTrainingModel[mod].fillfactor)
        //                {
        //                    yi += mTrainingModel[mod].height / 2;
        //                    continue;
        //                }

        //                EvaluateConv(Img742, ref mTrainingModel[mod].data, ref convSum, mod);
        //                double convR = convSum / mTrainingModel[mod].convSum;

        //                if (convR > Threshold)
        //                {
        //                    //mOverlayedImg.Rectangle(roi, Scalar.Cyan, 2);
        //                    yi += mTrainingModel[mod].height - yStep;
        //                    if (mDetectedModel[mod] < 100)
        //                        mDetectedRect[mod][mDetectedModel[mod]++] = roi;
        //                }
        //                else if (convR < (Threshold / 2) && convR > 0)
        //                    yi += mTrainingModel[mod].height / 2;
        //                else if (convR < 0)
        //                    yi += mTrainingModel[mod].height - yStep;

        //                if (convR > csumRanking[0])
        //                {
        //                    csumRanking[0] = convR;
        //                    Array.Sort(csumRanking);
        //                }
        //                csum2 += convR * convR;
        //                csumSum += convR;
        //                csumEffCnt++;
        //            }
        //        }
        //    }
        //    mIsRunModel[mod] = false;
        //}

        public void UseTemplate()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            string filename = "";
            string path = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                path = openFileDialog1.SafeFileName;
                if (filename == "") return;
            }
            else
                return;

            Mat temp = new Mat(filename);
            Mat grayImg = new Mat();

            Cv2.CvtColor(temp, grayImg, ColorConversionCodes.BGR2GRAY);
            Cv2.BitwiseNot(grayImg, temp);

            using (Mat result = new Mat())
            {
                // 템플릿 매칭
                Cv2.MatchTemplate(mSourceImg[0], temp, result, TemplateMatchModes.CCoeffNormed);

                // 매칭 범위 지정
                Cv2.Threshold(result, result, 0.1, 1.0, ThresholdTypes.Tozero);

                while (true)
                {
                    // 이미지 매칭 범위
                    OpenCvSharp.Point minloc, maxloc;
                    double minval, maxval;
                    Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                    var threshold = 0.4;
                    if (maxval >= threshold)
                    {
                        // 검색된 부분 빨간 테두리
                        Rect rect = new Rect(maxloc.X, maxloc.Y, temp.Width, temp.Height);
                        Cv2.Rectangle(mSourceImg[0], rect, new OpenCvSharp.Scalar(255, 255, 255), 2);

                        // 
                        Rect outRect;
                        Cv2.FloodFill(result, maxloc, new OpenCvSharp.Scalar(0), out outRect, new OpenCvSharp.Scalar(0.1), new OpenCvSharp.Scalar(1.0), FloodFillFlags.Link4);

                    }
                    else
                    {
                        break;
                    }
                }

                Cv2.ImShow("Found_show", mSourceImg[0]);
            }
        }
        public string[] mModelImg = null;

        public int FWIDTH = 10;
        public int FHEIGHT = 60;
        public void MakeTrainingFIle(Mat src, string filename)
        {
            //  get Size Info.
            int lwidth = src.Width;
            int lheight = src.Height;
            double lFillFactor = 0;

            //  Convert to Gray Scale
            Mat grayImg = new Mat();
            Mat destImg = new Mat();
            Mat Img742 = new Mat();
            Mat Img742Dest = new Mat();
            Mat tmpImg = new Mat();

            // Resize to 7x42 -> Gray -> Inverse -> BinStretch
            Cv2.Resize(src, destImg, new OpenCvSharp.Size(FWIDTH + 2, FHEIGHT + 2), 0, 0, InterpolationFlags.Area);
            Cv2.CvtColor(destImg, grayImg, ColorConversionCodes.BGR2GRAY);
            Cv2.BitwiseNot(grayImg, Img742Dest);
            BinStretching(Img742Dest, ref Img742, ref lFillFactor);
        }
        public string mTrainingFilePath = "";

        //public int LoadTrainingModel()
        //{
        //    var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));
        //    mTrainingFilePath = folder + "\\Training.csv";

        //    if ( !File.Exists(mTrainingFilePath))
        //    {
        //        MessageBox.Show("Could not find Model.");
        //        return 0;
        //    }
        //    StreamReader sr = new StreamReader(mTrainingFilePath);
        //    string fullstr = sr.ReadToEnd();
        //    sr.Close();

        //    string[] lModels = fullstr.Split("EOM".ToCharArray());
        //    int num = 0;
        //    for (int i = 0; i < lModels.Length; i++)
        //    {
        //        if (lModels[i].Length < 100)
        //            continue;
        //        num++;
        //    }

        //    mTrainingModel = new sTrainingModel[4*num];
        //    int k = 0;
        //    int q = 0;
        //    for ( int i=0; i< lModels.Length; i++)
        //    {
        //        if (lModels[i].Length < 100)
        //            continue;
        //        mTrainingModel[k].Init(FWIDTH,FHEIGHT);
        //        string[] lines = lModels[i].Split("\r\n".ToCharArray());
        //        for ( int j=0; j<lines.Length; j++)
        //        {
        //            if (lines[j].Length < 20) continue;
        //            string[] columes = lines[j].Split(',');
        //            int chksum = 0;
        //            if ( !int.TryParse(columes[0], out chksum))
        //            {
        //                mTrainingModel[k].width     = Convert.ToInt32(columes[1]);
        //                mTrainingModel[k].height    = Convert.ToInt32(columes[2]);
        //                mTrainingModel[k].fillfactor      = Convert.ToDouble(columes[3]);
        //                mTrainingModel[k].convSum   = Convert.ToDouble(columes[4]);
        //                mTrainingModel[k].type      = Convert.ToInt32(columes[5]);
        //                q = 0;
        //            }else
        //            {
        //                for ( int m=0; m< FWIDTH; m++)
        //                {
        //                    mTrainingModel[k].data[q,m] = Convert.ToInt32(columes[m]);
        //                }
        //                q++;
        //            }
        //        }
        //        k++;
        //    }
        //    int modelCount = k;
        //    for ( int i=0; i< k;i++)
        //    {
        //        if (mTrainingModel[i].type == 1 )    //  좌우반전 모델 추가 필요
        //        {
        //            mTrainingModel[modelCount].Init(FWIDTH, FHEIGHT);
        //            mTrainingModel[i].CopyTo(ref mTrainingModel[modelCount]);
        //            for (int xi = 0; xi < FWIDTH; xi++)
        //            {
        //                for (int yi = 0; yi < FHEIGHT; yi++)
        //                    mTrainingModel[modelCount].data[yi, xi] = mTrainingModel[i].data[yi, FWIDTH-xi-1];
        //            }
        //            mTrainingModel[modelCount].type = 0; //  파생모델
        //            modelCount++;
        //        }
        //        if (mTrainingModel[i].type == 2)    //  상하반전 모델 추가 필요, 상하 반전한 모델의 좌우반전모델 필요
        //        {
        //            //  원본의 좌우반전
        //            mTrainingModel[modelCount].Init(FWIDTH, FHEIGHT);
        //            mTrainingModel[i].CopyTo(ref mTrainingModel[modelCount]); ;
        //            for (int xi = 0; xi < FWIDTH; xi++)
        //            {
        //                for (int yi = 0; yi < FHEIGHT; yi++)
        //                    mTrainingModel[modelCount].data[yi, xi] = mTrainingModel[i].data[yi, FWIDTH - xi - 1];
        //            }
        //            mTrainingModel[modelCount].type = 0;
        //            modelCount++;
        //            // 원본의 상하반전
        //            mTrainingModel[modelCount].Init(FWIDTH, FHEIGHT);
        //            mTrainingModel[i].CopyTo(ref mTrainingModel[modelCount]); ;
        //            for (int xi = 0; xi < FWIDTH; xi++)
        //            {
        //                for (int yi = 0; yi < FHEIGHT; yi++)
        //                    mTrainingModel[modelCount].data[yi, xi] = mTrainingModel[i].data[ FHEIGHT - yi-1, xi];
        //            }
        //            mTrainingModel[modelCount].type = 0;
        //            modelCount++;
        //            //  원본모델의 원점대칭
        //            mTrainingModel[modelCount].Init(FWIDTH, FHEIGHT);
        //            mTrainingModel[i].CopyTo(ref mTrainingModel[modelCount]); ;
        //            for (int xi = 0; xi < FWIDTH; xi++)
        //            {
        //                for (int yi = 0; yi < FHEIGHT; yi++)
        //                    mTrainingModel[modelCount].data[yi, xi] = mTrainingModel[i].data[FHEIGHT - yi - 1, FWIDTH - xi - 1];
        //            }
        //            mTrainingModel[modelCount].type = 0;
        //            modelCount++;
        //        }
        //    }
        //    return modelCount;
        //}
        //public void EvaluateConv(Mat src, ref int[,] trainedModel, ref double convSum, int findex = 0)
        //{
        //    byte[] bytesSrc;
        //    int[,] filtered = new int[src.Width ,src.Height ];
        //    int[,] convImg = new int[src.Width , src.Height ];

        //    src.GetArray(out bytesSrc);


        //    //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
        //    //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        //    //byte[] bytesSrc2 = new byte[data.Height * data.Stride];
        //    //Marshal.Copy(data.Scan0, bytesSrc2, 0, bytesSrc.Length);
        //    Scalar res = src.Mean();
        //    double avgBin = (res.Val0 + 64 )/2;

        //    //for (int i = 0; i < src.Width ; i++)
        //    //{
        //    //    for (int j = 0; j < src.Height ; j++)
        //    //    {
        //    //        avgBin += bytesSrc[i + src.Width * j];
        //    //    }
        //    //}
        //    //avgBin = (avgBin / (src.Width  * src.Height ) + 64)/2;

        //    int c = 0;
        //    int[] dataSum = new int[src.Height];
        //    int[] trainSum = new int[src.Height];
        //    int[] dataXtrain = new int[src.Height];
        //    double bIsEff = 0.2;
        //    convSum = 0;

        //    for (int j = 0; j < src.Height ; j++)
        //    {
        //        bIsEff = 0.2;
        //        for (int i = 0; i < src.Width ; i++)
        //        {

        //            c = (bytesSrc[i + src.Width * j] - (int)avgBin) / 10;
        //            c = (c > 16 ? 16 : c);
        //            filtered[i, j] = c;
        //            if (trainedModel[j, i] == 5 && c < 0)
        //            {
        //                bIsEff = 0;
        //                convImg[i, j] = trainedModel[j, i] * c / 5;
        //                convSum += convImg[i, j];
        //                continue;
        //            }
        //            dataSum[j] += c;
        //            trainSum[j] += trainedModel[j, i];
        //            convImg[i, j] = trainedModel[j, i] * c;
        //            convSum += convImg[i, j];
        //        }
        //        dataXtrain[j] = (int)(dataSum[j] * trainSum[j] * bIsEff);
        //        convSum += dataXtrain[j];
        //        if (j > (src.Height / 2) && convSum < 0)
        //            break;
        //    }
        //    //  dest is trained Image
        //    if (!mThreading)
        //    {
        //        if (convSum > mTrainingModel[findex].convSum)
        //        {
        //            StreamWriter wr = new StreamWriter("EvalConv_" + findex.ToString() + "_" + mDetectedModel[findex].ToString() + ".csv");
        //            string lstr = "Src,Filtered,Trained,Conv," + convSum.ToString("F0") + "\r\n";
        //            wr.WriteLine(lstr);
        //            for (int j = 0; j < src.Height; j++)
        //            {
        //                lstr = "";
        //                for (int i = 0; i < src.Width; i++)
        //                {
        //                    lstr += bytesSrc[i + src.Width * j].ToString() + ",";
        //                }
        //                lstr += ",,";
        //                for (int i = 0; i < src.Width; i++)
        //                {
        //                    lstr += filtered[i, j].ToString() + ",";
        //                }
        //                lstr += "," + dataSum[j].ToString() + ",,,";
        //                for (int i = 0; i < src.Width; i++)
        //                {
        //                    lstr += trainedModel[j, i].ToString() + ",";
        //                }
        //                lstr += "," + trainSum[j].ToString() + ",,,";
        //                for (int i = 0; i < src.Width; i++)
        //                {
        //                    lstr += convImg[i, j].ToString() + ",";
        //                }
        //                lstr += "," + dataXtrain[j].ToString() + ",";
        //                wr.WriteLine(lstr);
        //            }
        //            wr.Close();
        //        }
        //    }
        //}
        public void ApplyFilter(Mat src, ref int[,] trained, double[,] filter, ref double avgBin, ref double convSum, int findex = 0)
        {
            byte[] bytesSrc;

            src.GetArray(out bytesSrc);
            int[,] verifyImg = new int[src.Width - 2, src.Height - 2];

            //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            //byte[] bytesSrc2 = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytesSrc2, 0, bytesSrc.Length);
            avgBin = 0;
            convSum = 0;
            double filteredVal = 0;
            int verifyVal = 0;
            int pix = 0;
            for (int i = 0; i < src.Width - 2; i++)
            {
                for (int j = 0; j < src.Height - 2; j++)
                {
                    trained[i, j] = 0;
                    filteredVal = 1;
                    verifyVal = 0;
                    for (int di = 0; di < 3; di++)
                    {
                        for (int dj = 0; dj < 3; dj++)
                        {
                            pix = bytesSrc[(i + di) + src.Width * (j + dj)];

                            verifyVal += bytesSrc[(i + di) + src.Width * (j + dj)];

                            if (filter[dj, di] > 0)
                                filteredVal *= Math.Pow((pix < 2 ? 1 : pix), filter[dj, di]);
                            //  filter[dj, di] 가 맞음. filter[di, dj] 는 틀림
                        }
                    }
                    trained[i, j] = (int)filteredVal;
                    verifyImg[i, j] = verifyVal;
                    avgBin += filteredVal;
                }
            }
            // dest is Filter Processed Image
            avgBin = avgBin / ((src.Width - 2) * (src.Height - 2));
            //int c = 0;
            for (int i = 0; i < src.Width - 2; i++)
            {
                for (int j = 0; j < src.Height - 2; j++)
                {
                    trained[i, j] = (trained[i, j] - (int)avgBin + 1) / 100;
                    convSum += trained[i, j] * verifyImg[i, j];
                }
            }
            //  dest is trained Image

            StreamWriter wr = new StreamWriter(RootPath + "train_" + findex.ToString() + ".csv");
            string lstr = "";
            for (int j = 0; j < src.Height; j++)
            {
                lstr = "";
                for (int i = 0; i < src.Width; i++)
                {
                    lstr += bytesSrc[i + src.Width * j].ToString() + ",";
                }
                wr.WriteLine(lstr);
            }
            wr.WriteLine("");
            wr.WriteLine("");
            for (int j = 0; j < src.Height - 2; j++)
            {
                lstr = "";
                for (int i = 0; i < src.Width - 2; i++)
                {
                    lstr += trained[i, j].ToString() + ",";
                }
                wr.WriteLine(lstr);
            }
            wr.Close();
        }
        //public void BinStretchingBad(Mat src, ref Mat dest, ref double fillfactor)
        //{
        //    double min = 0;
        //    double max = 0;

        //    src.MinMaxIdx(out min, out max);
        //    double alpha = 257 / (max - min);
        //    src.ConvertTo(dest, -1, alpha); //  Contrast Enhacement.

        //    //Scalar res = src.Mean();
        //    //double avg = res.Val0;
        //    Mat tmpImg = new Mat();
        //    Cv2.Threshold(dest, tmpImg, 96, 255, ThresholdTypes.Binary);  //  96 이상은 255, 미만은 0 처리
        //    fillfactor = tmpImg.CountNonZero();
        //    fillfactor = fillfactor / (src.Height * src.Width);
        //}

        public void BinStretching(Mat src, ref Mat dest, ref double fillfactor)
        {
            byte[] bytes;
            src.GetArray(out bytes);
            int[] lHisto = new int[256];

            int max = 0;
            int min = 255;
            //double avg = 0;
            int pos = 0;
            for (int i = 0; i < src.Width; i += 2)
            {
                for (int j = 0; j < src.Height; j += 2)
                {
                    pos = i + src.Width * j;
                    lHisto[bytes[pos]]++;
                    max = max < bytes[pos] ? bytes[pos] : max;
                    min = min > bytes[pos] ? bytes[pos] : min;
                }
            }
            if (max < 32 || (max - min) < 16)
            {
                fillfactor = 0;
                src.CopyTo(dest);
                return;
            }
            for (int j = 96; j < 256; j++)    //  Average 이상인 Pixel 의 개수
                fillfactor += lHisto[j];

            int max_min = max - min - 1;
            fillfactor = fillfactor / (src.Height * src.Width);
            byte c = 0;
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    pos = i + src.Width * j;
                    c = bytes[pos];
                    c = (c == max ? (byte)(c - 1) : c);
                    bytes[pos] = (byte)(1 + (254 * (c - min)) / max_min);
                }
            }
            src.CopyTo(dest);
            dest.SetArray(bytes);
        }
        public bool mThreading = false;
        public int[] mSearchYfrom = new int[4] { 0, 15, 30, 50 };
        public int[] mSearchYto = new int[4] { 14, 29, 49, 70 };
        public int mUserSubConv = 2;
        public bool mDetectEveryMark = false;
        public void SetSearchYRegion()
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            mDetectEveryMark = cbDetectEveryMark.Checked;
            ClearDataGridView1();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            SetSearchYRegion();

            BackupFMI();
            DetectInSelectedFile();
            RecoverFromBackupFMI();

        }

        sMarkResult[] mMarkMargin = null;

        public int mCalcEffBand = 7;
        public void DetectInSelectedFile(int findex = 0)
        {
            //  Find Current Model in the file focsed in the list box
            //  Show Box detected on the image
            //  Calculate all the values for the dataggridview1, datagridview2, datagridview3
            //  After Processing, File Focus in the list box is automatically moves to the next file

            Mat src = new Mat();
            Mat ImgDest = new Mat();

            mSourceImg[0].CopyTo(src);

            sFiducialMark lfMark = null;

            
            if (mCurrentScell.X >= 0)
            {
                lfMark = mFidMarkSide[mCurrentScell.Y];
                mSearchModelIndex = mCurrentScell.Y;
            }
            else if (mCurrentTcell.X >= 0)
            {
                lfMark = mFidMarkTop[mCurrentTcell.Y];
                mSearchModelIndex = mCurrentTcell.Y + 8;
            }

            if (lfMark.modelSize.Width == 0)
                return;

            byte[] srcData = null;


            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////
            //  여기서 마크 찾기를 수행한다.
            Cv2.Resize(src, ImgDest, new OpenCvSharp.Size(src.Width / mModelScale, src.Height / mModelScale), 1.0/ mModelScale, 1.0/ mModelScale, InterpolationFlags.Area);  //  1/mModelScale 축소
            //ImgDest.SaveImage("ForAnalysis.bmp");
            ImgDest.GetArray(out q_Value[0]);   //  ImgDest : 1/mModelScale Compressed Image

            //Bitmap image = null;
            //image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(ImgDest);
            //pictureBox4.Image = image;
            //pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;

            double[] sPitch = new double[10];
            double[] sYaw = new double[10];
            double[] cx = new double[12];
            double[] cy = new double[12];
            int[] lazimuth = new int[12];
            long nFound = 0;
            sMarkResult[] smr = new sMarkResult[12];
            sMarkResult[] smr_T = new sMarkResult[12];
            sMarkResult[] smr_B = new sMarkResult[12];

            for (int i = 0; i < 12; i++)
            {
                smr[i] = new sMarkResult();
                smr_T[i] = new sMarkResult();
                smr_B[i] = new sMarkResult();
            }

            OpenCvSharp.Point[] fMarkPos = null;

            //if (!mDetectEveryMark)
            //{
            //    if (mCurrentScell.X >= 0)
            //        fMarkPos = FineCOG(findex, ref smr, ref nFound, false, 0, mCurrentScell.Y);
            //    else if (mCurrentTcell.X >= 0)
            //        fMarkPos = FineCOG(findex, ref smr, ref nFound, false, 0, (mCurrentTcell.Y + 8));
            //}else
            //{
            //    fMarkPos = FineCOG(findex, ref smr, ref nFound, false, 0, -1);
            //}
            bool IsFirst = false;
            if (findex == 0)
                IsFirst = true;

            if (!mDetectEveryMark)
            {
                if (mCurrentScell.X >= 0)
                    fMarkPos = FineCOG(IsFirst, findex, ref smr, ref smr_T, ref smr_B, ref nFound, false, 0, mCurrentScell.Y);
                else if (mCurrentTcell.X >= 0)
                    fMarkPos = FineCOG(IsFirst, findex, ref smr, ref smr_T, ref smr_B, ref nFound, false, 0, (mCurrentTcell.Y + 8));
            }
            else
            {
                fMarkPos = FineCOG(IsFirst, findex, ref smr, ref smr_T, ref smr_B, ref nFound, false, 0, -1);
                if (mCheckCompatibility)
                    mMarkMargin = smr;
            }

            if (nFound== 0)
            {
                //richTextBox1.Text += "Fail to find mark.\r\n";
                return;
            }
            string lstr = nFound.ToString() + " Found\r\n";

            for (int i = 0; i < nFound; i++)
                lstr += smr[i].pos.X.ToString("F4") + "\t" + smr[i].pos.Y.ToString("F4") + "\r\n";

            richTextBox1.Text += lstr + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////
            //  다음은 일단 마크를 찾았다 치고, 찾은 위치 xi, yi 에 대해서 처리한다.
            //  원본 이미지는 src 로서 스케일 1:1 
            //int xi = 0;
            //int yi = 0;
            //byte[] srcData = null;

            for (int i = 0; i < nFound; i++)
            {
                if (mDetectEveryMark)
                {
                    if (i < mFidMarkSide.Count)
                        lfMark = mFidMarkSide[i];
                    else
                        lfMark = mFidMarkTop[i - mFidMarkSide.Count];
                }

                int lwidth = lfMark.modelSize.Width;
                int lheight = lfMark.modelSize.Height;

                if (cx[i] < 0 || cy[i] < 0)
                    continue;
                if (double.IsNaN(cx[i]) || double.IsNaN(cy[i]))
                    continue;

                Rect roi = new Rect(fMarkPos[i].X, fMarkPos[i].Y, lwidth, lheight);
                Mat subImg = ImgDest.SubMat(roi);
                //Cv2.ImShow(i.ToString(), subImg);


                mDetectedRect[mSearchModelIndex][mMarkCount].X = fMarkPos[i].X * mModelScale;
                mDetectedRect[mSearchModelIndex][mMarkCount].Y = fMarkPos[i].Y * mModelScale;
                mDetectedRect[mSearchModelIndex][mMarkCount].Width  = mModelScale * lwidth;
                mDetectedRect[mSearchModelIndex][mMarkCount].Height = mModelScale * lheight;

                //  4개의 마크를 찾은 뒤 정보를 srcData 라고 하면, 
                double res = 0;
                double subconv = 0;
                mDetectedmark[mMarkCount].x = fMarkPos[i].X;
                mDetectedmark[mMarkCount].y = fMarkPos[i].Y;
                mDetectedmark[mMarkCount].Azimuth = lazimuth[i];

                mDetectedmark[mMarkCount].conv = mPrevConv[0][i];// CalcConv(ref lfMark, fMarkPos[i].X, fMarkPos[i].Y, 0, ref subconv);
                mDetectedmark[mMarkCount].img = new byte[lwidth * lheight];

                subImg.GetArray(out srcData);
                Array.Copy(srcData, mDetectedmark[mMarkCount].img, srcData.Length);

                mCalcDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].diff = res;

                mCalcXDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].Xdiff = res;

                mCalcYDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].Ydiff = res;

                mCalcInOutAvg(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].IO = res;

                mCalcLeftRightAvg(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].LR = res;

                mCalcTopBottomAvg(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].TB = res;

                mCalcCenterToLeftRightAvg(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].CLR = res;

                mCalcCenterToTopBottomAvg(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].CTB = res;

                mCalcFullStdev(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].std = res;

                mCalcInFullStdev(srcData, ref lfMark, ref res);
                mDetectedmark[mMarkCount].IFstd = res;

                mDetectedmark[mMarkCount].rect = mDetectedRect[mSearchModelIndex][mMarkCount];
                mDetectedmark[mMarkCount].IsGood = true;
                mDetectedmark[mMarkCount].imgFile = mOrgFile;
                mDetectedmark[mMarkCount].absIndex = mMarkCount;

                mQuadImg[mMarkCount] = new byte[lwidth * lheight];
                subImg.GetArray(out mQuadImg[mMarkCount]);

                mGIndex[mMarkCount] = mMarkCount;

                mMarkCount++;
            }

            DrawMarkDetected(fMarkPos, (int)nFound, mDetectEveryMark);

            UpdateDataGridView1();
            UpdateDataGridView2();
        }

        public void DrawSearchROI(OpenCvSharp.Rect rcRoi)
        {
            Mat tmpImg = new Mat();
            if (mOverlayedImg == null)
            {
                mOverlayedImg = new Mat();
                Cv2.CvtColor(mSourceImg[0], mOverlayedImg, ColorConversionCodes.GRAY2RGB);
            }

            mOverlayedImg.Rectangle(rcRoi, Scalar.Pink, 1);
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);
            pictureBox2.Image = myImage;
        }
        public void DrawMarkDetected(OpenCvSharp.Point[] fMarkPos, int lMarkCount, bool detectEveryMark = true)
        {
            Mat tmpImg = new Mat();
            if (mOverlayedImg == null)
                mOverlayedImg = new Mat();

            Cv2.CvtColor(mSourceImg[0], mOverlayedImg, ColorConversionCodes.GRAY2RGB);
            sFiducialMark lfMark = null;

            if (lMarkCount == 1)
            {
                if (mCurrentScell.X >= 0)
                {
                    lfMark = mFidMarkSide[mCurrentScell.Y];
                }
                else if (mCurrentTcell.X >= 0)
                {
                    lfMark = mFidMarkTop[mCurrentTcell.Y];
                }
            }

            for (int i = 0; i < lMarkCount; i++)
            {
                if (detectEveryMark)
                {
                    if (i < mFidMarkSide.Count)
                        lfMark = mFidMarkSide[i];
                    else
                        lfMark = mFidMarkTop[i - mFidMarkSide.Count];
                }

                int lwidth = lfMark.modelSize.Width;
                int lheight = lfMark.modelSize.Height;

                OpenCvSharp.Rect lrc = new OpenCvSharp.Rect();
                lrc.X = fMarkPos[i].X * mModelScale;
                lrc.Y = fMarkPos[i].Y * mModelScale;
                lrc.Width = mModelScale * lwidth;
                lrc.Height = mModelScale * lheight;

                if (i == mMarkFocused)
                    mOverlayedImg.Rectangle(lrc, Scalar.Cyan, 2);
                else
                    mOverlayedImg.Rectangle(lrc, Scalar.Cyan, 1);
            }
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);
            pictureBox2.Image = myImage;
        }

        public void ItrTest()
        {
            Mat ImgResize = new Mat();
            Mat ImgStrech = new Mat();
            Rect roi = new Rect(100, 100, 300, 100);

            Mat subImg = mSourceImg[0].SubMat(roi);
            Cv2.Resize(subImg, ImgResize, new OpenCvSharp.Size(FWIDTH, FHEIGHT), 0, 0, InterpolationFlags.Area);
            double lFillFactor = 0;
            BinStretching(ImgResize, ref ImgStrech, ref lFillFactor);

            //ImgStrech.ConvertTo(DestroyHandle, -1, 255/(MaximizeBox-MinimizeBox))
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //private void pictureBox3_MouseEnter(object sender, EventArgs e)
        //{

        //}

        //private void pictureBox3_MouseLeave(object sender, EventArgs e)
        //{

        //}
        public Rect mSelectRect;
        public bool mEffectiveRect = false;
        public bool mFirstRect = false;

        //private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        //{
        //    mCst.TrackRubberBand(pictureBox3, e);

        //    Rect lRect = new Rect(mCst.X, mCst.Y, (e.X - mCst.X), (e.Y - mCst.Y));
        //    if (mSourceImg != null && mCst.IsTracking> 0)
        //    {
        //        double xstart = (mCst.X+1) / (double)pictureBox3.Size.Width * mSourceImg.Width;
        //        double ystart = (mCst.Y+1) / (double)pictureBox3.Size.Height * mSourceImg.Height;

        //        if (mCst.IsTracking == 1)
        //        {
        //            double lwidth = (e.X - mCst.X) / (double)pictureBox3.Size.Width * mSourceImg.Width;
        //            double lheight = (e.Y - mCst.Y) / (double)pictureBox3.Size.Height * mSourceImg.Height;
        //            mSelectRect = new Rect((int)xstart, (int)ystart, (int)lwidth, (int)lheight);
        //            mFirstRect = true;
        //            //label6.Text = mSelectRect.ToString();
        //        }
        //        else if ( mCst.IsTracking == 2)
        //        {
        //            mSelectRect.X = (int)xstart;
        //            mSelectRect.Y = (int)ystart;
        //            mFirstRect = true;
        //            //label6.Text = mSelectRect.ToString();
        //        }
        //    }
        //}
        public bool mIgnoreDark = false;

        //public void CreateBaseModel()
        //{
        //    Mat subImg = new Mat();
        //    Mat resizeImg = new Mat();
        //    Mat anyImg = new Mat();
        //    byte[] tmp = new byte[7 * 7];
        //    Mat tmpImg = new Mat(7, 7, MatType.CV_8UC1, tmp);
        //    double lFillFactor = 0;
        //    Rect lSelectRect = new Rect();

        //    lSelectRect.Width = mSelectRect.Width;
        //    lSelectRect.Height = mSelectRect.Height;

        //    double sConv = 0;
        //    double mmdist = 0;
        //    double convR = 0;
        //    double convD = 0;
        //    double convRold = 0;
        //    int maxi = 0;
        //    int maxj = 0;

        //    if (!mFirstRect)
        //    {
        //        for (int i = -7; i < 8; i++)
        //        {
        //            for (int j = -7; j < 8; j++)
        //            {
        //                lSelectRect.X = mSelectRect.X + i;
        //                lSelectRect.Y = mSelectRect.Y + j;

        //                subImg = mSourceImg.SubMat(lSelectRect);

        //                Cv2.Resize(subImg, resizeImg, new OpenCvSharp.Size(28, 56), 0, 0, InterpolationFlags.Area);
        //                MaxAvgPooling(resizeImg, tmpImg);

        //                sConv = 0;
        //                mmdist = 0;
        //                CalcConvolutionSpl2Model(tmpImg, 0, ref sConv, ref mmdist);
        //                convR = sConv / mSearchModel[0].conv;

        //                if (convRold < convR)
        //                {
        //                    convRold = convR;
        //                    maxi = i;
        //                    maxj = j;
        //                }
        //            }
        //        }
        //    }
        //    if (convRold < 0.3)   //  비슷한 영역이 있다고 할 수 없는 경우
        //    {
        //        maxi = 0;
        //        maxj = 0;
        //    }
        //    lSelectRect.X = mSelectRect.X + maxi;
        //    lSelectRect.Y = mSelectRect.Y + maxj;
        //    mSelectRect.X = lSelectRect.X;
        //    mSelectRect.Y = lSelectRect.Y;

        //    subImg = mSourceImg.SubMat(lSelectRect);

        //    Cv2.Resize(subImg, resizeImg, new OpenCvSharp.Size(28, 56), 0, 0, InterpolationFlags.Area);

        //    //Cv2.Resize(resizeImg, anyImg, new OpenCvSharp.Size(140, 280), 0, 0, InterpolationFlags.Area);
        //    //Cv2.ImShow("First", anyImg);
        //    //MessageBox.Show("First");

        //    MaxAvgPooling(resizeImg, tmpImg);
        //    //Cv2.Resize(tmpImg, anyImg, new OpenCvSharp.Size(140, 140), 0, 0, InterpolationFlags.Area);
        //    //Cv2.ImShow("MaxAvgPooling", anyImg);
        //    //MessageBox.Show("MaxAvgPooling");
        //    //BinStretching(resizeImg, ref subImg, ref lFillFactor);
        //    // 초기에 설정한 모델과 차이가 가장 적은 위치에서 모델의 새로 잡아야 한다.

        //    //Cv2.Resize(resizeImg, tmpImg, new OpenCvSharp.Size(FWIDTH * 5, FHEIGHT * 5), 0, 0, InterpolationFlags.Area);
        //    //Cv2.ImShow("Tmp", tmpImg);

        //    mSearchModel[mBaseModelCnt].width = mSelectRect.Width;
        //    mSearchModel[mBaseModelCnt].height = mSelectRect.Height;
        //    //SaveModelData(tmpImg);  //  SaveModelData() 안에서 Subject 기준으로 파일로 저장하고, 추가하고, 나중에 파일을 읽어들일 수 있도록 한다.
        //                            //  읽어들일 때는 Subject 기준으로 하나의 파일을 읽어들이면 된다.
        //                            //  나중에 새로운 영상파일을 로드한 뒤 기존 Subject base Mode 을 선택하면 거기에 mBaseModel 을 추가하도록 한다.

        //    mBaseModelCnt++;
        //}
        public void MaxAvgPooling(Mat src, Mat dest)
        {
            byte[] p = new byte[28 * 56];
            byte[] q14 = new byte[14];  //  Intermediate Buffer
            byte[] q7 = new byte[7 * 56];  //  Final 1
            //byte[] q756 = new byte[56];  //  Intermediate Buffer
            byte[] q728 = new byte[28];  //  Intermediate Buffer
            byte[] q714 = new byte[14];  //  Intermediate Buffer
            byte[] q77 = new byte[7 * 7];   //  Final 2
            byte[] pH = new byte[28];
            byte[] pV = new byte[56];
            src.GetArray(out p);
            //  Max-Avg Pooling along X direction
            for (int y = 0; y < src.Height; y++)
            {
                Array.Copy(p, y * src.Width, pH, 0, src.Width);
                for (int i = 0; i < src.Width - 1; i += 2)
                {
                    int localMax = 0;
                    int istart = (i > 3 ? (i - 4) : 0);
                    int iend = ((i + 4) < src.Width ? (i + 4) : src.Width);
                    for (int ii = istart; ii < iend; ii++)
                        localMax = (p[ii + y * src.Width] > localMax ? p[ii + y * src.Width] : localMax);

                    if (localMax == Math.Max(p[i + y * src.Width], p[i + 1 + y * src.Width]))
                        q14[i / 2] = (byte)localMax;
                    else
                        q14[i / 2] = (byte)((p[i + y * src.Width] + p[i + 1 + y * src.Width]) / 2);
                }
                for (int i = 0; i < 13; i += 2)
                {
                    int localMax = 0;
                    int istart = (i > 2 ? (i - 3) : 0);
                    int iend = ((i + 3) < 13 ? (i + 3) : 13);
                    for (int ii = istart; ii <= iend; ii++)
                        localMax = (q14[ii] > localMax ? q14[ii] : localMax);

                    if (localMax == Math.Max(q14[i], q14[i + 1]))
                        q7[i / 2 + y * 7] = (byte)localMax;
                    else
                        q7[i / 2 + y * 7] = (byte)((q14[i] + q14[i + 1]) / 2);
                }
            }
            //  Max-Avg Pooling along Y direction
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < src.Height; y++)
                    pV[y] = q7[x + y * 7];

                for (int y = 0; y < src.Height - 1; y += 2)
                {
                    int localMax = 0;
                    int istart = (y > 3 ? (y - 4) : 0);
                    int iend = ((y + 4) < src.Height ? (y + 4) : src.Height);
                    for (int yi = istart; yi < iend; yi++)
                        localMax = (q7[x + yi * 7] > localMax ? q7[x + yi * 7] : localMax);

                    if (localMax == Math.Max(q7[x + y * 7], q7[x + (y + 1) * 7]))
                        q728[(y / 2)] = (byte)localMax;
                    else
                        q728[(y / 2)] = (byte)((q7[x + y * 7] + q7[x + (y + 1) * 7]) / 2);
                }
                for (int y = 0; y < 27; y += 2)
                {
                    int localMax = 0;
                    int istart = (y > 2 ? (y - 3) : 0);
                    int iend = ((y + 3) < 27 ? (y + 3) : 27);
                    for (int yi = istart; yi <= iend; yi++)
                        localMax = (q728[yi] > localMax ? q728[yi] : localMax);

                    if (localMax == Math.Max(q728[y], q728[(y + 1)]))
                        q714[(y / 2)] = (byte)localMax;
                    else
                        q714[(y / 2)] = (byte)((q728[y] + q728[(y + 1)]) / 2);
                }
                for (int y = 0; y < 13; y += 2)
                {
                    int localMax = 0;
                    int istart = (y > 2 ? (y - 3) : 0);
                    int iend = ((y + 3) < 13 ? (y + 3) : 13);
                    for (int yi = istart; yi <= iend; yi++)
                        localMax = (q714[yi] > localMax ? q714[yi] : localMax);

                    if (localMax == Math.Max(q714[y], q714[(y + 1)]))
                        q77[x + (y / 2) * 7] = (byte)localMax;
                    else
                        q77[x + (y / 2) * 7] = (byte)((q714[y] + q714[(y + 1)]) / 2);
                }
            }
            byte min = 255;
            byte max = 0;
            for (int i = 0; i < 49; i++)
            {
                if (q77[i] < min)
                    min = q77[i];
                if (q77[i] > max)
                    max = q77[i];
            }
            for (int i = 0; i < 49; i++)
                q77[i] = (byte)((q77[i] - min) * 255.0 / (max - min));

            dest.SetArray(q77);
        }

        //private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        //{
        //    mCst.StartPoint(pictureBox3, e);
        //}

        //private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        //{
        //    mCst.EndPoint(pictureBox3, e);
        //}

        //private void pictureBox3_Paint(object sender, PaintEventArgs e)
        //{
        //    mCst.DrawRubberBand(pictureBox3, e);
        //}

        //private void pictureBox3_Click(object sender, EventArgs e)
        //{

        //}

        //private void pictureBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        //{



        //}

        //private void pictureBox3_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        Cv2.ImShow("A", mOverlayedImg);
        //        if (mFirstRect)
        //        {
        //            if ( mSelectRect.Width > 5 && mSelectRect.Height > 5 && mSelectRect.X > 0 && mSelectRect.Y > 0)
        //            {
        //                mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
        //                //CreateBaseModel();
        //                mFirstRect = false;
        //            }else if (mSelectRect.Width > 5 && mSelectRect.Height > 5 && mSelectRect.X == 0 && mSelectRect.Y == 0)
        //            {
        //                mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
        //                mFirstRect = false;
        //            }
        //        }
        //        int xstart = (int)((e.X + 1) / (double)pictureBox3.Size.Width * mSourceImg.Width);
        //        int ystart = (int)((e.Y + 1) / (double)pictureBox3.Size.Height * mSourceImg.Height);

        //        mSelectRect.X = xstart - mSelectRect.Width / 2;
        //        mSelectRect.Y = ystart - mSelectRect.Height / 2;

        //        //CreateBaseModel();
        //        mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
        //        Cv2.ImShow("A", mOverlayedImg);

        //        Bitmap image = null;
        //        image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);

        //        pictureBox3.Image = image;
        //        pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;

        //    }
        //}

        //private void button2_Click_1(object sender, EventArgs e)
        //{
        //    double sum = 0;
        //    double mmdist = 0;
        //    double sumThresh = 0.7;
        //    double distThresh = 0.7;
        //    bool[] used = new bool[mBaseModelCnt];
        //    int shiftX = 0, shiftY = 0;
        //    for (int i = 0; i < mBaseModelCnt; i++)
        //        used[i] = false;

        //    //  각 base model 을 중심으로 slave 를 구한다.
        //    for ( int i=0; i< mBaseModelCnt-1 ; i++)
        //    {
        //        for ( int j= i+1; j< mBaseModelCnt; j++)
        //        {
        //            sum = 0;
        //            mmdist = 0;
        //            CalcConvolutionModel2Model(i, j, ref sum, ref mmdist, ref shiftX, ref shiftY);
        //            if (sumThresh * mBaseModel[i].sConv < sum && mmdist < distThresh )
        //                mBaseModel[i].slave[mBaseModel[i].slaveCnt++] = (byte)j;

        //            if (sumThresh * mBaseModel[j].sConv < sum && mmdist < distThresh )
        //                mBaseModel[j].slave[mBaseModel[j].slaveCnt++] = (byte)i;
        //        }
        //    }

        //    //  1. slave 가 가장 많은 basemodel 을 골라 자신과 slave 들의 평균영상을 만든다.
        //    //  2. SaveLearnedModelData() 처리한다.
        //    //  3. 해당 base model 내의 slave 들과 Convolution  값을 모두 구해서 80% 가 Pass 되는 임계값을 sConv 에 저장한다.
        //    //  4. 해당 basemodel 에 포함되지 않은 모델중에서 slave 가 가장 많은 base 모델을 고른다.
        //    //  5. 1 ~ 4 처리한다.
        //    //  6. 1 ~ 5 를 반복한다.
        //    //  7. Slave 가 1개 이하인 경우 종료
        //    //  8. SaveLearnedModelToFile( ) 처리한다. Subject 를 사용자가 입력해서 저장한다.

        //    int[] max = new int[50];
        //    int[] max_i = new int[50] ;
        //    int k = 0;
        //    while(true)
        //    {
        //        for (int i = 0; i < mBaseModelCnt; i++)
        //        {
        //            if (max[k] < mBaseModel[i].slaveCnt && used[i] == false)
        //            {
        //                max[k] = mBaseModel[i].slaveCnt;
        //                max_i[k] = i;
        //            }
        //        }
        //        if (max[k] < 2)
        //            break;
        //        mLearnedModel[k].img = new byte[mBaseModel[0].img.Length];
        //        mLearnedModel[k].Width = mBaseModel[0].Width;
        //        mLearnedModel[k].Height = mBaseModel[0].Height;
        //        mLearnedModel[k].index = max_i[k];
        //        Array.Copy(mBaseModel[ max_i[k] ].img, mLearnedModel[k].img, mBaseModel[0].img.Length);

        //        int[] avg = new int[mBaseModel[0].img.Length];
        //        used[max_i[k]] = true;
        //        for (int j = 0; j < mBaseModel[0].img.Length; j++)
        //            avg[j] += mBaseModel[max_i[k]].img[j];


        //        for (int i = 0; i < mBaseModel[ max_i[k] ].slaveCnt; i++)
        //        {
        //            used[ mBaseModel[ max_i[k] ].slave[i] ] = true;
        //            for (int j = 0; j < mBaseModel[0].img.Length; j++)
        //                avg[j] += mBaseModel[ mBaseModel[max_i[k]].slave[i] ].img[j];
        //        }
        //        for (int j = 0; j < mBaseModel[0].img.Length; j++)
        //            mLearnedModel[k].img[j] = (byte)(avg[j]/ (mBaseModel[max_i[k]].slaveCnt+1));    //  Master + Slave Count

        //        double lmax = 0;
        //        double lmin = 255;
        //        for (int j = 0; j < mBaseModel[0].img.Length; j++)
        //        {
        //            if (lmax < mLearnedModel[k].img[j])
        //                lmax = mLearnedModel[k].img[j];
        //            if (lmin > mLearnedModel[k].img[j])
        //                lmin = mLearnedModel[k].img[j];
        //        }
        //        for (int j = 0; j < mBaseModel[0].img.Length; j++)
        //            mLearnedModel[k].img[j] = (byte)((mLearnedModel[k].img[j] - lmin) * 255.0 / (lmax - lmin));

        //        SaveLearnedModelData(k);
        //        k++;
        //    }
        //    mLearnedModelCnt = k;
        //}

        public string mCurrentSubject = "";
        public string mCurrentBaseSubject = "";

        //private void button5_Click_1(object sender, EventArgs e)
        //{
        //    var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

        //    string sFilePath = folder;

        //    SaveFileDialog saveFile = new SaveFileDialog();
        //    saveFile.DefaultExt = "slm";
        //    saveFile.InitialDirectory = sFilePath;

        //    saveFile.Title = "Save as SLM file";
        //    saveFile.Filter = "SLM(*.slm)|*.slm";
        //    string sFileName = "";
        //    if (saveFile.ShowDialog() == DialogResult.OK)
        //        sFileName = saveFile.FileName;
        //    else
        //        return;

        //    mCurrentSubject = sFileName;
        //    BinaryWriter bw = new BinaryWriter(File.Open(sFileName, FileMode.Create));
        //    for (int i = 0; i < mLearnedModelCnt; i++)
        //    {
        //        bw.Write( mLearnedModel[i] .Width           );
        //        bw.Write( mLearnedModel[i] .Height          );
        //        bw.Write( mLearnedModel[i] .index           );
        //        bw.Write( mLearnedModel[i] .IgnoreUnder     );
        //        bw.Write( mLearnedModel[i] .NegativeUnder   );
        //        bw.Write( mLearnedModel[i] .SaturateOver    );
        //        bw.Write( mLearnedModel[i] .sConv           );
        //        bw.Write( mLearnedModel[i] .sDist           );
        //        bw.Write( mLearnedModel[i] .nu02            );
        //        bw.Write( mLearnedModel[i] .nu20            );
        //        bw.Write( mLearnedModel[i] .nu11            );
        //        bw.Write( mLearnedModel[i] .eccent          );
        //        bw.Write( mLearnedModel[i] .fillfactor          );
        //        bw.Write( mLearnedModel[i] .theta           );
        //        bw.Write( mLearnedModel[i] .slave           );
        //        bw.Write( mLearnedModel[i] .shiftX           );
        //        bw.Write( mLearnedModel[i] .shiftY           );
        //        bw.Write( mLearnedModel[i] .slaveCnt        );
        //        bw.Write( mLearnedModel[i] .img             );
        //    }
        //    bw.Close();
        //}

        //private void button6_Click_2(object sender, EventArgs e)
        //{
        //    if (mCurrentSubject.Length > 10)
        //    {
        //        BinaryWriter bw = new BinaryWriter(File.Open(mCurrentSubject, FileMode.Append));
        //        for (int i = 0; i < mLearnedModelCnt; i++)
        //        {
        //            bw.Write(mLearnedModel[i].Width);
        //            bw.Write(mLearnedModel[i].Height);
        //            bw.Write(mLearnedModel[i].index);
        //            bw.Write(mLearnedModel[i].IgnoreUnder);
        //            bw.Write(mLearnedModel[i].NegativeUnder);
        //            bw.Write(mLearnedModel[i].SaturateOver);
        //            bw.Write(mLearnedModel[i].sConv);
        //            bw.Write(mLearnedModel[i].sDist);
        //            bw.Write(mLearnedModel[i].nu02);
        //            bw.Write(mLearnedModel[i].nu20);
        //            bw.Write(mLearnedModel[i].nu11);
        //            bw.Write(mLearnedModel[i].eccent);
        //            bw.Write(mLearnedModel[i].fillfactor);
        //            bw.Write(mLearnedModel[i].theta);
        //            bw.Write(mLearnedModel[i].slave);
        //            bw.Write(mLearnedModel[i].slaveCnt);
        //            bw.Write(mLearnedModel[i].img);
        //        }
        //        bw.Close();
        //    }
        //}

        //private void button3_Click_2(object sender, EventArgs e)
        //{
        //    OpenLearnedModel();

        //    //richTextBox3.Text += "Model Category \t" + mLearnedModelCnt.ToString() + "\r\n";
        //}
        public int OpenBaseModel(ref int[] basewidth, ref int[] baseheight, string fileName = "")
        {
            //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));
            //string sFilePath = folder;

            string sFilePath = RootPath + "Training";
            if (!Directory.Exists(sFilePath))
                Directory.CreateDirectory(sFilePath);

            string sFileName = "";

            if (fileName == "")
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.DefaultExt = "csv";
                openFile.InitialDirectory = sFilePath;
                openFile.Filter = "CSV(*.csv)|*.csv";
                if (openFile.ShowDialog() == DialogResult.OK)
                    sFileName = openFile.FileName;
                else
                    return 0;
            }
            else
                sFileName = fileName;

            if (!File.Exists(sFileName)) return 0;
            string all = "";
            try
            {
                StreamReader wr = new StreamReader(sFileName);
                all = wr.ReadToEnd();
                wr.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to access file : " + sFileName);
                return 0;
            }

            string[] allModel = all.Split("[HEAD]".ToCharArray());
            int modelNo = 0;
            for (int i = 0; i < allModel.Length; i++)
            {
                if (allModel[i].Length < 5) continue;
                string[] allLines = allModel[i].Split("\n".ToCharArray());
                string[] lhead = allLines[0].Split(',');
                mSearchModel[modelNo].width = Convert.ToInt16(lhead[1]);
                mSearchModel[modelNo].height = Convert.ToInt16(lhead[2]);
                mSearchModel[modelNo].conv = Convert.ToInt16(lhead[3]);
                mSearchModel[modelNo].planeShift = Convert.ToInt16(lhead[4]);
                if (lhead.Length > 5)
                    if (lhead[5].Length > 1)
                        mSearchModel[modelNo].planeHeight = Convert.ToDouble(lhead[5]);  //  이 항목이 없을수도 있으므로.

                mSearchModel[modelNo].img = new int[mSearchModel[modelNo].width * mSearchModel[modelNo].height];
                //mSearchModel[modelNo].diffimg = new int[(mSearchModel[modelNo].width - 2) * (mSearchModel[modelNo].height - 2)];
                if (allLines.Length <= mSearchModel[modelNo].height)
                    return modelNo;

                int cnt = 0;
                for (int j = 1; j <= mSearchModel[modelNo].height; j++)
                {
                    string[] ldata = allLines[j].Split(',');
                    if (ldata.Length < mSearchModel[modelNo].width)
                        return modelNo;

                    for (int k = 0; k < mSearchModel[modelNo].width; k++)
                        mSearchModel[modelNo].img[cnt++] = Convert.ToInt16(ldata[k]);
                }

                //for (int yi = 0; yi < mSearchModel[modelNo].height - 2; yi++)
                //    for (int xi = 0; xi < mSearchModel[modelNo].width - 2; xi++)
                //    {
                //        if (xi < 4 || xi > mSearchModel[modelNo].width - 8)
                //            mSearchModel[modelNo].diffimg[xi + yi * (mSearchModel[modelNo].width - 2)] = mSearchModel[modelNo].img[xi + yi * mSearchModel[modelNo].width] - mSearchModel[modelNo].img[xi + 2 + yi * mSearchModel[modelNo].width];
                //        else if (yi < 4 || yi > mSearchModel[modelNo].height - 7)
                //            mSearchModel[modelNo].diffimg[xi + yi * (mSearchModel[modelNo].width - 2)] = mSearchModel[modelNo].img[xi + yi * mSearchModel[modelNo].width] - mSearchModel[modelNo].img[xi + (yi + 2) * mSearchModel[modelNo].width];
                //    }

                basewidth[modelNo] = mSearchModel[modelNo].width;
                baseheight[modelNo] = mSearchModel[modelNo].height;
                modelNo++;
            }

            mCurrentBaseModelFile = sFileName;

            return modelNo;
        }

        public int[] mFoundInFile = new int[200];
        public int mSearchRectCount = 0;

        private void button10_Click(object sender, EventArgs e)
        {
            string sFileName = "";
            try
            {
                var folder = Path.GetFullPath(RootPath + "Training");

                string sFilePath = folder;


                SaveFileDialog saveFile = new SaveFileDialog();
                //saveFile.DefaultExt = "csv";
                //saveFile.InitialDirectory = sFilePath;
                //saveFile.Filter = "CSV(*.csv)|*.csv";
                //if (saveFile.ShowDialog() == DialogResult.OK)
                //    sFileName = saveFile.FileName;
                //else
                //    return;

                //SaveModelFile(sFileName);
                saveFile.DefaultExt = "xml";
                saveFile.InitialDirectory = sFilePath;
                saveFile.Filter = "XML(*.xml)|*.xml";
                if (saveFile.ShowDialog() == DialogResult.OK)
                    sFileName = saveFile.FileName;
                else
                    return;

                SaveFiducialMark(sFileName);
            }
            catch (IOException)
            {
                MessageBox.Show("Try Again after closing the file : \r\n" + sFileName);
                return;
            }
        }
        public string mCurrentBaseModelFile = "";
        public void SaveModelFile(string sFileName = "")
        {
            if (sFileName == "")
            {
                StreamWriter wr;
                wr = new StreamWriter(mCurrentBaseModelFile);
                for (int mi = 0; mi < 10; mi++)
                {
                    if (mSearchModel[mi].width < 1) continue;
                    string lstr = "[HEAD]," + mSearchModel[mi].width.ToString()
                                    + "," + mSearchModel[mi].height.ToString()
                                    + "," + mSearchModel[mi].conv.ToString()
                                    + "," + mSearchModel[mi].planeShift.ToString()
                                    + "," + mSearchModel[mi].planeHeight.ToString("F2");
                    wr.WriteLine(lstr);
                    for (int j = 0; j < mSearchModel[mi].height; j++)
                    {
                        lstr = "";
                        for (int i = 0; i < mSearchModel[mi].width; i++)
                            lstr += mSearchModel[mi].img[i + j * mSearchModel[mi].width].ToString() + ",";
                        wr.WriteLine(lstr);
                    }
                }
                wr.Close();
            }
            else
            {
                StreamWriter wr;
                wr = new StreamWriter(sFileName);
                for (int mi = 0; mi < 10; mi++)
                {
                    if (mSearchModel[mi].width < 1) continue;
                    string lstr = "[HEAD]," + mSearchModel[mi].width.ToString()
                                    + "," + mSearchModel[mi].height.ToString()
                                    + "," + mSearchModel[mi].conv.ToString()
                                    + "," + mSearchModel[mi].planeShift.ToString()
                                    + "," + mSearchModel[mi].planeHeight.ToString("F2");
                    wr.WriteLine(lstr);
                    for (int j = 0; j < mSearchModel[mi].height; j++)
                    {
                        lstr = "";
                        for (int i = 0; i < mSearchModel[mi].width; i++)
                            lstr += mSearchModel[mi].img[i + j * mSearchModel[mi].width].ToString() + ",";
                        wr.WriteLine(lstr);
                    }
                }
                wr.Close();
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            btnMouseHover(sender, e);

            //double kkk = 1.999;
            //MessageBox.Show(((int)(kkk * 10)).ToString());

            int[] basewidth = new int[10];
            int[] baseheight = new int[10];
            int res = OpenBaseModel(ref basewidth, ref baseheight);
            if (res == 0) return;
            string lstr = "";

            for (int i = 0; i < res; i++)
            {
                lstr = "M_" + i.ToString() + " " + mSearchModel[i].width.ToString() + " " + mSearchModel[i].height.ToString() + " " + mSearchModel[i].conv.ToString() + " " + mSearchModel[i].planeShift.ToString() + " " + mSearchModel[i].planeHeight.ToString("F2");
            }

            mHaveOpenBaseModel = true;

            btnMouseEnter(sender, e);
        }

        private void cbIgnoreDarkArea_CheckedChanged(object sender, EventArgs e)
        {

        }
        private bool panel1Shown = false;
        private System.Drawing.Size pictureBox5Size = new System.Drawing.Size();
        public bool mFAutoLearnLoaded = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitBtns();
            InitdgvDesignNModelSide();
            InitdgvDesignNModelTop();
            InitDataGridView();
            InitDataGridView2();
            InitDataGridView3();
            LoadOpticsConfig();

            panel1.Location = new System.Drawing.Point(8, 312);
            tbOpticsTopViewOffset.Text = mOpticsTopViewOffset.ToString("F3");
            tbOpticsTgtOffset.Text = mOpticsTgtOffset.ToString("F3");
            tbFocusWindowOffset.Text = mOpticsFWOffset.ToString("F3");
            tbDefaultROIWidth.Text = "2000";
            tbDefaultROIHeight.Text = "2000";
            lbModelScale.SelectedIndex = 0;
            panel1.Show();
            panel1Shown = true;

            pictureBox5Size.Width = pictureBox5.Size.Width;
            pictureBox5Size.Height = pictureBox5.Size.Height;

            if (mLastFMIisLoaded)
                ShowMarkDGV();

            mFAutoLearnLoaded = true;
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
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(8, 8, 8);
            this.dataGridView1.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
            this.dataGridView1.ScrollBars = ScrollBars.Vertical;


            // Column
            this.dataGridView1.Columns[0].Name = "No.";
            this.dataGridView1.Columns[1].Name = "X";
            this.dataGridView1.Columns[2].Name = "Y";
            this.dataGridView1.Columns[3].Name = "Conv";
            this.dataGridView1.Columns[4].Name = "G/NG";
            this.dataGridView1.Columns[0].Width = 35;
            this.dataGridView1.Columns[1].Width = 50;
            this.dataGridView1.Columns[2].Width = 40;
            this.dataGridView1.Columns[3].Width = 50;
            this.dataGridView1.Columns[4].Width = 44;

            for (i = 0; i < 5; i++)
            {
                this.dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                this.dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //this.dataGridView1.Columns[i].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
                //this.dataGridView1.Columns[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(218, 218, 218);
            }

            this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            for (i = 0; i < 1000; i++)
            {
                this.dataGridView1.Rows.Add(i.ToString(), "0", "0", "0", "-");
                if (colTitle != "")
                {
                    colTitle = "";
                    bColorChange = !bColorChange;
                }

                for (int j = 0; j < 5; j++)
                {
                    if (bColorChange)
                    {
                        this.dataGridView1[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(16, 8, 18);
                        this.dataGridView1[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(218, 208, 218);
                    }
                    else
                    {
                        this.dataGridView1[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                        this.dataGridView1[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
                    }
                }
                effRowNum++;
            }

            this.dataGridView1.Rows.Add("", "", "", "", "");
            //m__G.FlowTracker("InitDataGridView effRowNum:" + effRowNum.ToString());

            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 22;

            for (i = 0; i < effRowNum; i++)
            {
                this.dataGridView1.Rows[i].Height = 16;
                this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
                this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
                this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
                this.dataGridView1[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                this.dataGridView1[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }
            this.dataGridView1.Rows[i].Height = 16;
            this.dataGridView1.Rows[i].Resizable = DataGridViewTriState.False;
            this.dataGridView1.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9);
            this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);

            for (int colum = 3; colum < this.dataGridView1.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
                {
                    this.dataGridView1[colum, row].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                    this.dataGridView1.ReadOnly = true;
                }
            }
            this.dataGridView1.ReadOnly = true;
        }
        private void InitDataGridView2()
        {
            int i = 0;

            dataGridView2.ColumnCount = 14;
            dataGridView2.Font = new Font("Calibri", 9, FontStyle.Regular);
            for (i = 0; i < dataGridView2.ColumnCount; i++)
            {
                dataGridView2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.BackgroundColor = System.Drawing.Color.FromArgb(8, 8, 24);
            dataGridView2.ForeColor = System.Drawing.Color.FromArgb(208, 208, 224);
            dataGridView2.ScrollBars = ScrollBars.Vertical;


            // Column
            dataGridView2.Columns[0].Name = "No.";
            dataGridView2.Columns[1].Name = "X";
            dataGridView2.Columns[2].Name = "Y";
            dataGridView2.Columns[3].Name = "Conv";            //  General Conv Value
            dataGridView2.Columns[4].Name = "Diff";        //  Diogonal Differential Conv Value
            dataGridView2.Columns[5].Name = "XDiff";       //  Horizontal Differential Conv Value
            dataGridView2.Columns[6].Name = "YDiff";       //  Vertical Differential Conv Value
            dataGridView2.Columns[7].Name = "I-O";        //  (Inner Avg - Outer Avg ) / Full Avg
            dataGridView2.Columns[8].Name = "L-R";        //  (Left Avg - Right Avg ) / Full Avg
            dataGridView2.Columns[9].Name = "T-B";        //  (Top Avg - Bottom Avg ) / Full Avg
            dataGridView2.Columns[10].Name = "C-LR";      //  (Central Area Avg - Left & Right Avg ) / Full Avg
            dataGridView2.Columns[11].Name = "C-TB";      //  (Central Area Avg - Top & Bottom Avg ) / Full Avg
            dataGridView2.Columns[12].Name = "σ";          //  Full Standard Deviation
            dataGridView2.Columns[13].Name = "IFσ";   //  Inner Area Standard Deviation / Full Standard Deviation 
            //this.dataGridView2.Columns[14].Name = "OFσ";   //  Outer Area Standard Deviation / Full Standard Deviation 
            for (i = 0; i < 14; i++)
            {
                dataGridView2.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Regular);
                dataGridView2.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            this.dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);

            dataGridView2.Columns[0].Width = 35;
            dataGridView2.Columns[1].Width = 50;
            dataGridView2.Columns[2].Width = 40;
            for (i = 3; i < 14; i++)
                dataGridView2.Columns[i].Width = 40;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            //m__G.FlowTracker(m__G.mTestItem[i, 0] + ":" + m__G.mTestItem[i, 1] + ":" + m__G.mTestItem[i, 2] + ":" + m__G.mTestItem[i, 3] + ":" + m__G.mTestItem[i, 4]);
            dataGridView2.Rows.Add("Avg", "-", "-", "0");  //  당해 열의 모든 행의 평균값
            dataGridView2.Rows.Add("Std", "-", "-", "0");  //  당해 열의 모든 행의 표준편차
            dataGridView2.Rows.Add("Dist", "-", "-", "0"); //  당해 열에 대해서 (양품평균 - 불량평균) / (양품표준편차 + 불량 표준편차)

            for (i = 0; i < 3; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    this.dataGridView2[j, i].Style.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
                    this.dataGridView2[j, i].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 240);
                }
            }
            for (i = 0; i < 30; i++)
            {
                effRowNum = i + 3;
                dataGridView2.Rows.Add(i.ToString(), "0", "0", "0", "-");
                if (colTitle != "")
                {
                    colTitle = "";
                    bColorChange = !bColorChange;
                }

                for (int j = 0; j < 14; j++)
                {
                    if (bColorChange)
                    {
                        this.dataGridView2[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                        this.dataGridView2[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
                        //dataGridView2[0, effRowNum].Style.BackColor = Color.Lavender;
                        //dataGridView2[1, effRowNum].Style.BackColor = Color.Lavender;
                    }
                    else
                    {
                        this.dataGridView2[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 24);
                        this.dataGridView2[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 240);
                        //dataGridView2[0, effRowNum].Style.BackColor = Color.White;
                        //dataGridView2[1, effRowNum].Style.BackColor = Color.White;
                    }
                }
            }

            dataGridView2.Rows.Add("", "", "", "", "");
            //m__G.FlowTracker("InitDataGridView effRowNum:" + effRowNum.ToString());

            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ColumnHeadersHeight = 22;

            for (i = 0; i < 33; i++)
            {
                dataGridView2.Rows[i].Height = 16;
                dataGridView2.Rows[i].Resizable = DataGridViewTriState.False;
                dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                //this.dataGridView2[1, i].Style.Font = new Font("Calibri", 8, FontStyle.Bold);
                //this.dataGridView2[2, i].Style.Font = new Font("Calibri", 8, FontStyle.Bold);
                //this.dataGridView2[3, i].Style.Font = new Font("Calibri", 8, FontStyle.Bold);
                //this.dataGridView2[4, i].Style.Font = new Font("Calibri", 8, FontStyle.Italic);
            }

            for (int colum = 3; colum < dataGridView2.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < dataGridView2.Rows.Count; row++)
                {
                    dataGridView2[colum, row].Style.BackColor = System.Drawing.Color.FromArgb(16, 16, 48);
                    dataGridView2.ReadOnly = true;
                }
            }
            dataGridView2.ReadOnly = true;
        }
        private void InitDataGridView3()
        {
            int i = 0;

            dataGridView3.ColumnCount = 14;
            dataGridView3.Font = new Font("Calibri", 9, FontStyle.Regular);
            for (i = 0; i < dataGridView3.ColumnCount; i++)
            {
                dataGridView3.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.BackgroundColor = System.Drawing.Color.FromArgb(24, 8, 8);
            dataGridView3.ForeColor = System.Drawing.Color.FromArgb(224, 208, 208);
            dataGridView3.ScrollBars = ScrollBars.Vertical;


            // Column
            // Column
            dataGridView3.Columns[0].Name = "No.";
            dataGridView3.Columns[1].Name = "X";
            dataGridView3.Columns[2].Name = "Y";
            dataGridView3.Columns[3].Name = "Conv";            //  General Conv Value
            dataGridView3.Columns[4].Name = "Diff";        //  Diogonal Differential Conv Value
            dataGridView3.Columns[5].Name = "XDiff";       //  Horizontal Differential Conv Value
            dataGridView3.Columns[6].Name = "YDiff";       //  Vertical Differential Conv Value
            dataGridView3.Columns[7].Name = "I-O";        //  (Inner Avg - Outer Avg ) / Full Avg
            dataGridView3.Columns[8].Name = "L-R";        //  (Left Avg - Right Avg ) / Full Avg
            dataGridView3.Columns[9].Name = "T-B";        //  (Top Avg - Bottom Avg ) / Full Avg
            dataGridView3.Columns[10].Name = "C-LR";      //  (Central Area Avg - Left & Right Avg ) / Full Avg
            dataGridView3.Columns[11].Name = "C-TB";      //  (Central Area Avg - Top & Bottom Avg ) / Full Avg
            dataGridView3.Columns[12].Name = "σ";          //  Full Standard Deviation
            dataGridView3.Columns[13].Name = "IσFσ";   //  Inner Area Standard Deviation / Full Standard Deviation 
            //this.dataGridView3.Columns[14].Name = "OσFσ";   //  Outer Area Standard Deviation / Full Standard Deviation 
            for (i = 0; i < 14; i++)
            {
                dataGridView3.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Regular);
                dataGridView3.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            this.dataGridView3.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.dataGridView3.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);

            dataGridView3.Columns[0].Width = 35;
            dataGridView3.Columns[1].Width = 50;
            dataGridView3.Columns[2].Width = 40;
            for (i = 3; i < 14; i++)
                dataGridView3.Columns[i].Width = 40;

            // Row
            int effRowNum = 0;
            bool bColorChange = true;
            string colTitle = "";

            dataGridView3.Rows.Add("Avg", "-", "-", "0");  //  당해 열의 모든 행의 평균값
            dataGridView3.Rows.Add("Std", "-", "-", "0");  //  당해 열의 모든 행의 표준편차
            dataGridView3.Rows.Add("Dist", "-", "-", "0"); //  당해 열에 대해서 (양품평균 - 불량평균) / (양품표준편차 + 불량 표준편차)

            for (i = 0; i < 3; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    this.dataGridView3[j, i].Style.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
                    this.dataGridView3[j, i].Style.ForeColor = System.Drawing.Color.FromArgb(240, 208, 208);
                }
            }

            for (i = 0; i < 30; i++)
            {
                effRowNum = i + 3;
                dataGridView3.Rows.Add(i.ToString(), "0", "0", "0", "-");
                if (colTitle != "")
                {
                    colTitle = "";
                    bColorChange = !bColorChange;
                }

                for (int j = 0; j < 14; j++)
                {
                    if (bColorChange)
                    {
                        this.dataGridView3[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                        this.dataGridView3[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
                        //dataGridView3[0, effRowNum].Style.BackColor = Color.Lavender;
                        //dataGridView3[1, effRowNum].Style.BackColor = Color.Lavender;
                    }
                    else
                    {
                        this.dataGridView3[j, effRowNum].Style.BackColor = System.Drawing.Color.FromArgb(24, 8, 8);
                        this.dataGridView3[j, effRowNum].Style.ForeColor = System.Drawing.Color.FromArgb(240, 208, 208);
                        //dataGridView3[0, effRowNum].Style.BackColor = Color.White;
                        //dataGridView3[1, effRowNum].Style.BackColor = Color.White;
                    }
                }
            }

            dataGridView3.Rows.Add("", "", "", "", "");
            //m__G.FlowTracker("InitDataGridView effRowNum:" + effRowNum.ToString());

            dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView3.EnableHeadersVisualStyles = false;
            dataGridView3.ColumnHeadersHeight = 22;

            for (i = 0; i < 33; i++)
            {
                dataGridView3.Rows[i].Height = 16;
                dataGridView3.Rows[i].Resizable = DataGridViewTriState.False;
                dataGridView3.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                //this.dataGridView3[1, i].Style.Font = new Font("Calibri", 8, FontStyle.Bold);
                //this.dataGridView3[2, i].Style.Font = new Font("Calibri", 8, FontStyle.Bold);
                //this.dataGridView3[4, i].Style.Font = new Font("Calibri", 8, FontStyle.Italic);
            }
            for (int colum = 3; colum < dataGridView3.ColumnCount - 1; colum++)
            {
                for (int row = 0; row < dataGridView3.Rows.Count; row++)
                {
                    dataGridView3[colum, row].Style.BackColor = System.Drawing.Color.FromArgb(40, 16, 16);
                    dataGridView3.ReadOnly = true;
                }
            }
            dataGridView3.ReadOnly = true;
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    if (tbSubConv.Text.Length > 0)
        //        mUserSubConv = Convert.ToInt16(tbSubConv.Text);
        //    else
        //        mUserSubConv = 2;

        //    //  기존  Model 에 Weight 를 반영하여 새로운 Model 을 만든다.
        //    //  W x 기존 모델 + SUM ( DataGridView2 소속으로 지정된 Detected 데이터 ; G Detected Data )
        //    //  상기 2차원 데이터를 Historam 기준으로 2%, 98% Saturation 처리하여  -20 ~ 20 으로 Normalize
        //    int lwidth = mSearchModel[mSearchModelIndex].width;
        //    int lheight = mSearchModel[mSearchModelIndex].height;
        //    int bin1pro = (int)(lwidth * lheight * 0.01 + 0.5);
        //    int[] lAvg = new int[lwidth * lheight];
        //    int[] lSort = new int[lwidth * lheight];


        //    int lmin = 0;
        //    int lmax = 0;
        //    int lavg = 0;
        //    double lconv = 99999;
        //    double lsum = 0;
        //    long lcount = 0;
        //    int oldweight = 0;

        //    for (int i = 0; i < mMarkCount; i++)
        //    {
        //        if (!mDetectedmark[i].IsGood)
        //            continue;
        //        if (lconv > mDetectedmark[i].conv)
        //            lconv = mDetectedmark[i].conv;  //  최소값을 찾아서 0.9 배 해서 모델에 저장한다.

        //    }

        //    for (int i = 0; i < mMarkCount; i++)
        //    {
        //        if (!mDetectedmark[i].IsGood)
        //            continue;

        //        for (int x = 0; x < lwidth; x++)
        //            for (int y = 0; y < lheight; y++)
        //            {
        //                lAvg[x + y * lwidth] += mQuadImg[mDetectedmark[i].absIndex][x + y * lwidth];
        //            }
        //    }
        //    if (oldweight > 0)
        //    {
        //        for (int x = 0; x < lwidth; x++)
        //            for (int y = 0; y < lheight; y++)
        //            {
        //                lAvg[x + y * lwidth] += mSearchModel[mSearchModelIndex].img[x + y * lwidth] * oldweight;
        //            }
        //    }

        //    for (int x = 0; x < lwidth; x++)
        //        for (int y = 0; y < lheight; y++)
        //        {
        //            lAvg[x + y * lwidth] = lAvg[x + y * lwidth] / (mMarkCount + oldweight);
        //            lsum += lAvg[x + y * lwidth];
        //            lcount++;
        //        }

        //    Array.Copy(lAvg, lSort, lAvg.Length);
        //    Array.Sort(lSort);
        //    lmin = lSort[bin1pro];
        //    lmax = lSort[lwidth * lheight - 1 - bin1pro];
        //    //lavg = (int)(lsum / lcount);
        //    lavg = lSort[(lwidth * lheight) / 2] - 1;
        //    if (lavg < 10)
        //        lavg = 10;

        //    for (int itr = 0; itr < 2; itr++)
        //    {
        //        lsum = 0;
        //        lcount = 0;
        //        for (int x = 0; x < lwidth; x++)
        //            for (int y = 0; y < lheight; y++)
        //            {
        //                if (lAvg[x + y * lwidth] > lavg)
        //                    lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lmax - lavg);
        //                else
        //                    lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lavg - lmin);

        //                if (lSort[x + y * lwidth] < -20)
        //                    lSort[x + y * lwidth] = -20;

        //                if (lSort[x + y * lwidth] > 20)
        //                    lSort[x + y * lwidth] = 20;

        //                lsum += lSort[x + y * lwidth];
        //                lcount++;
        //            }
        //        double tmpavg = lsum / lcount;
        //        if (Math.Abs(tmpavg) < 0.7)
        //            break;
        //        lavg = (int)(lavg + tmpavg / 2);
        //    }



        //    mNewSearchModel[mSearchModelIndex].width = lwidth;
        //    mNewSearchModel[mSearchModelIndex].height = lheight;
        //    mNewSearchModel[mSearchModelIndex].conv = (int)(lconv * 0.9);
        //    mNewSearchModel[mSearchModelIndex].img = new int[lwidth * lheight];
        //    for (int i = 0; i < lwidth * lheight; i++)
        //        mNewSearchModel[mSearchModelIndex].img[i] = lSort[i];


        //    byte[] tmpBytes = new byte[lwidth * lheight];
        //    //for (int i = 0; i < tmpBytes.Length; i++)
        //    //    tmpBytes[i] = (byte)(6 * mNewSearchModel[mSearchModelIndex].img[i] + 120);
        //    Mat tmpImg = new Mat();
        //    Mat tmp10x = new Mat();
        //    //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
        //    //Cv2.ImShow("Avg Img", tmp10x);

        //    OptimizeModel();
        //    //Task taskOptimizeModel = Task.Run(() => { OptimizeModel(); });
        //    //// Task가 끝날 때까지 대기
        //    //taskOptimizeModel.Wait();

        //    //Thread Thread_OptimizeModel = new Thread(() => OptimizeModel());
        //    //Thread_OptimizeModel.Start();

        //    ///////////////////////////////////////////////////////////////////////
        //    /////   Show Mode Image on picturebox3
        //    tmpBytes = new byte[lwidth * lheight];
        //    for (int i = 0; i < tmpBytes.Length; i++)
        //        tmpBytes[i] = (byte)(6 * mNewSearchModel[mSearchModelIndex].img[i] + 120);

        //    tmpImg = new Mat(lheight, lwidth, MatType.CV_8U, tmpBytes);
        //    Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
        //    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmp10x);

        //    pictureBox4.Image = myImage;
        //    pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
        //    ///////////////////////////////////////////////////////////////////////
        //}
        private void button4_Click(object sender, EventArgs e)
        {
            mDetectEveryMark = true;
            ClearDataGridView1();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            SetSearchYRegion();

            BackupFMI();
            mCheckCompatibility = true;
            DetectInSelectedFile();
            mCheckCompatibility = false;
            RecoverFromBackupFMI();

            int focusedModel = 0;
            for (int i = 0; i < mMarkMargin.Length; i++)
            {
                if (mMarkMargin[i].pos.X == 0)
                    return;

                int xL = (int)mMarkMargin[i].pos.X;
                int xR = (int)(100 * (mMarkMargin[i].pos.X - (int)mMarkMargin[i].pos.X));
                int yT = (int)mMarkMargin[i].pos.Y;
                int yB = (int)(100 * (mMarkMargin[i].pos.Y - (int)mMarkMargin[i].pos.Y));
                int dX = (xL - xR) / 2;
                int dY = (yT - yB) / 2;
                if ( i<3)
                {
                    mCurrentTcell.X = 0;
                    mCurrentTcell.X = -1;

                    mCurrentScell.X = 4;
                    mCurrentScell.Y = i;
                    focusedModel = i;
                }
                else
                {
                    mCurrentScell.X = 0;
                    mCurrentScell.Y = -1;

                    mCurrentTcell.X = 4;
                    mCurrentTcell.Y = i-3;
                    focusedModel = i + 1;
                }
                if (mModelROI[focusedModel].X == 0)
                    return;

                mModelROI[focusedModel].X += dX;
                mModelROI[focusedModel].Y += dY;
                mSelectRect = mModelROI[focusedModel];

                Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                Rect roi2 = new Rect((int)mSelectRect.X+1, (int)mSelectRect.Y+1, mSelectRect.Width, mSelectRect.Height);

                //	영상클립

                mCustomImg = mSourceImg[0].SubMat(roi);
                mCustomImg2 = mSourceImg[0].SubMat(roi2);
                //	보여주기
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                pictureBox5.Image = myImage;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateMousePosition();

                CreateModel();
                SaveModel();
            }
        }

        public void OptimizeModel()
        {
            double[] SMixup = new double[100];
            double[] SConv1 = new double[100];
            double[] SConv2 = new double[100];
            uint lwidth = (uint)mNewSearchModel[mSearchModelIndex].width;
            uint lheight = (uint)mNewSearchModel[mSearchModelIndex].height;
            //double subConv = 0;
            //double Conv = 0;
            double[] eachConv = new double[mMarkCount *5];
            double[] eachSubConv = new double[mMarkCount *5];
            double passConv = 0;
            double failConv = 0;
            double passSubConv = 0;
            double failSubConv = 0;
            int passCount = 0;
            int failCount = 0;
            int[] Dimg = new int[lwidth * lheight];
            byte[] tmpBytes = new byte[lwidth * lheight];

            int itr = 1;
            int oldMarkCount = mMarkCount;
            Mat tmpImg = new Mat((int)lheight, (int)lwidth, MatType.CV_8U, tmpBytes);


            sFiducialMark lfMark = null;
            if (mSearchModelIndex < 8)
            {
                lfMark = mFidMarkSide[mSearchModelIndex];
            }
            else
            {
                lfMark = mFidMarkTop[mSearchModelIndex - 8];
            }

            //  Generate Fail Image for each mDetectedmark[Di].img in case of mDetectedmark[Di].IsGood == true
            for (int Di = 0; Di < oldMarkCount; Di++)
            {
                if (!mDetectedmark[Di].IsGood)
                    continue;

                // Min Bin, Max Bin , H 3 avg, V 3 avg
                int minBin = 255;
                int maxBin = 0;
                for (int ni = 0; ni < lwidth * lheight; ni++)
                {
                    if (minBin > mDetectedmark[Di].img[ni])
                        minBin = mDetectedmark[Di].img[ni];
                    if (maxBin < mDetectedmark[Di].img[ni])
                        maxBin = mDetectedmark[Di].img[ni];
                }
                uint x0 = lwidth / 2 - 1;
                int[] vAvg = new int[lheight];
                for (uint yi = 0; yi < lheight; yi++)
                    for (uint xi = x0; xi < x0 + 3; xi++)
                        vAvg[yi] += mDetectedmark[Di].img[xi + yi * lwidth];

                uint y0 = lheight / 2 - 1;
                int[] hAvg = new int[lwidth];
                for (uint xi = 0; xi < lwidth; xi++)
                    for (uint yi = y0; yi < y0 + 3; yi++)
                        hAvg[xi] += mDetectedmark[Di].img[xi + yi * lwidth];

                //  Bin 1/3 축소 영샹
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                uint lArea = lwidth * lheight;
                for (uint ni = 0; ni < lArea; ni++)
                    mDetectedmark[mMarkCount].img[ni] = (byte)(0.333 * (mDetectedmark[Di].img[ni] - minBin) + minBin);

                //byte[] tmpb = new byte[lwidth * lheight];
                //Array.Copy(mDetectedmark[mMarkCount].img, tmpb, lwidth * lheight);
                //tmpImg.SetArray(tmpb);
                //Mat tmp10x = new Mat();
                //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth*10, lheight*10), 10, 10, InterpolationFlags.Area);
                //string title = "x1/3 Imge " + Di.ToString();
                //Cv2.ImShow(title, tmp10x);
                mMarkCount++;

                //  Bin 3배 확대영상
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                for (uint ni = 0; ni < lArea; ni++)
                {
                    int p = 3 * (mDetectedmark[Di].img[ni] - minBin) + minBin;
                    mDetectedmark[mMarkCount].img[ni] = (byte)(p > 255 ? 255 : p);
                }
                //tmpImg.SetArray(mDetectedmark[mMarkCount].img);
                //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
                //title = "x3 Imge " + Di.ToString();
                //Cv2.ImShow(title, tmp10x);
                mMarkCount++;

                //  중앙 횡 3줄 평균을 상하로 확장한 영상
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                for (uint yi = 0; yi < lheight; yi++)
                    for (uint xi = 0; xi < lwidth; xi++)
                        mDetectedmark[mMarkCount].img[xi + yi * lwidth] = (byte)(hAvg[xi] / 3);

                //tmpImg.SetArray(mDetectedmark[mMarkCount].img);
                //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
                //title = "Vetically Expanded Imge " + Di.ToString();
                //Cv2.ImShow(title, tmp10x);
                mMarkCount++;

                //  중앙 종 3열 평균을 좌우로 확장한 영상
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                for (uint yi = 0; yi < lheight; yi++)
                    for (uint xi = 0; xi < lwidth; xi++)
                        mDetectedmark[mMarkCount].img[xi + yi * lwidth] = (byte)(vAvg[yi] / 3);

                //tmpImg.SetArray(mDetectedmark[mMarkCount].img);
                //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
                //title = "Horizontally Expanded Imge " + Di.ToString();
                //Cv2.ImShow(title, tmp10x);
                mMarkCount++;
            }


            while (itr < 6)
            {
                passConv = 0;
                failConv = 0;
                passSubConv = 0;
                failSubConv = 0;
                passCount = 0;
                failCount = 0;
                for (int Di = 0; Di < mMarkCount; Di++)
                {
                    // mNewSearchModel[mSearchModelIndex].img  와  mDetectedmark[Di].img 간 비교한다.
                    eachConv[Di] = CalcConv(ref lfMark, mNewSearchModel[mSearchModelIndex].img, lwidth, lheight, mUserSubConv, ref eachSubConv[Di], Di);
                    if (mDetectedmark[Di].IsGood)
                    {
                        passConv += eachConv[Di];
                        passSubConv += eachSubConv[Di];
                        passCount++;
                    }
                    else
                    {
                        failConv += eachConv[Di];
                        failSubConv += eachSubConv[Di];
                        failCount++;
                    }
                }
                SConv1[itr] = Math.Abs(passConv / passCount - failConv / failCount);    //  Conv 값에 대해서 Pass 품의 평균값과 Fail 품의 평균값의 차이를 저장
                SConv2[itr] = Math.Abs(passSubConv / passCount - failSubConv / failCount);  //  SubConv 값에 대해서  Pass 품의 평균값과 Fail 품의 평균값의 차이를 저장
                itr++;

                for (int xi = 0; xi < lwidth; xi++)
                    for (int yi = 0; yi < lheight; yi++)
                    {
                        ////if (Math.Abs(mNewSearchModel[mSearchModelIndex].img[xi + yi * lwidth]) == 19)
                        ////{
                        ////    Dimg[xi + yi * lwidth] = 0;
                        ////    continue;
                        ////}

                        //  mNewSearchModel[mSearchModelIndex].img 의 어느 한 Pixel 의 값을 증가시켰을 때 
                        //  Pass 품의 Conv 값 평균치와 Fail품의 Conv 값 평균치 차기가 커지면 
                        //  delta = 1
                        //  Pass 품의 Conv 값 평균치와 Fail품의 Conv 값 평균치 차기가 줄어들면
                        //  delta = -1
                        //  Pass 품의 Conv 값 평균치와 Fail품의 SubConv 값 평균치 차기가 커지면 
                        //  delta += 1
                        //  Pass 품의 Conv 값 평균치와 Fail품의 SubConv 값 평균치 차기가 줄어들면
                        //  delta += -1
                        //  한 뒤 Dimg[] 에 저장.
                        //  

                        mNewSearchModel[mSearchModelIndex].img[xi + yi * lwidth] += 1;
                        passConv = 0;
                        failConv = 0;
                        passSubConv = 0;
                        failSubConv = 0;
                        passCount = 0;
                        failCount = 0;
                        for (int Di = 0; Di < mMarkCount; Di++)
                        {
                            eachConv[Di] = CalcConv(ref lfMark, mNewSearchModel[mSearchModelIndex].img, lwidth, lheight, mUserSubConv, ref eachSubConv[Di], Di);
                            if (mDetectedmark[Di].IsGood)
                            {
                                passConv += eachConv[Di];
                                passSubConv += eachSubConv[Di];
                                passCount++;
                            }
                            else
                            {
                                failConv += eachConv[Di];
                                failSubConv += eachSubConv[Di];
                                failCount++;
                            }
                        }
                        SConv1[itr] = Math.Abs(passConv / passCount - failConv / failCount);
                        SConv2[itr] = Math.Abs(passSubConv / passCount - failSubConv / failCount);

                        int delta = (SConv1[itr] - SConv1[itr - 1] > 0 ? 1 : -1);
                        delta += (SConv2[itr] - SConv2[itr - 1] > 0 ? 1 : -1);
                        Dimg[xi + yi * lwidth] = delta;
                        mNewSearchModel[mSearchModelIndex].img[xi + yi * lwidth] -= 1;
                    }
                //  모든 Pixel 에 대해서 Dimg[] 가 업데이트가 완료되면 Dimg 를 mNewSearchModel[mSearchModelIndex].img 에 더해준다.
                //  더해줄 때 Saturation 처리.
                for (int pi = 0; pi < lwidth * lheight; pi++)
                {
                    mNewSearchModel[mSearchModelIndex].img[pi] += Dimg[pi];
                    if (mNewSearchModel[mSearchModelIndex].img[pi] >= 20)
                        mNewSearchModel[mSearchModelIndex].img[pi] = 19;
                    if (mNewSearchModel[mSearchModelIndex].img[pi] <= -20)
                        mNewSearchModel[mSearchModelIndex].img[pi] = -19;
                }

                Thread.Sleep(50);
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        richTextBox1.Text += itr.ToString() + "\t" + SConv1[itr].ToString("F1") + "\t" + SConv2[itr].ToString("F1") + "\r\n";
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                    });
                }
            }
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    SetNewModelParams();
                });
            }
            else
            {
                SetNewModelParams();
            }
        }

        public void SetNewModelParams()
        {
            //  최종 모델파일을 적용하여 List 박스에 있는 파일에서 모두 Detection
            //  DataGridView1 내용 Update
            //  DataGridView1 의 임의의 Row 를 선택하면 해당 Img 파일을 열어서 Detection Box 표시함.
            //  DataGridView1 의 Row 를 한줄씩 내려감에 따라 해당 Img 파일을 열어서 Detection Box 표시함.
            sFiducialMark lfMark = null;
            if (mSearchModelIndex < 8)
            {
                lfMark = mFidMarkSide[mSearchModelIndex];
            }
            else
            {
                lfMark = mFidMarkTop[mSearchModelIndex - 8];
            }

            sFiducialMark lOldModel = new sFiducialMark();
            if (lfMark.img != null)
            {
                lOldModel.img = new int[lfMark.modelSize.Width * lfMark.modelSize.Height];
                lOldModel = lfMark;
            }
            else
                return;

            mNewSearchModel[mSearchModelIndex].planeShift = 0;

            //  Initialize New Model Parameters
            if (mNewSearchModel[mSearchModelIndex].img != null)
            {
                lfMark.img = mNewSearchModel[mSearchModelIndex].img;
                lfMark.conv = mNewSearchModel[mSearchModelIndex].conv;
                lfMark.planeShift = mNewSearchModel[mSearchModelIndex].planeShift;
                lfMark.planeHeight = mNewSearchModel[mSearchModelIndex].planeHeight;
            }

            //dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            byte[] srcData = null;
            double res = 0;
            //double subconv = 0;

            Mat src = new Mat();
            Mat ImgDest = new Mat();

            for (int i = 0; i < mMarkCount; i++)
            {
                //if (!mDetectedmark[i].IsGood) continue;

                srcData = new byte[mDetectedmark[i].img.Length];
                Array.Copy(mDetectedmark[i].img, srcData, mDetectedmark[i].img.Length);

                mDetectedmark[i].conv = mCalcConv(srcData, ref lfMark);

                mCalcDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[i].diff = res;

                mCalcXDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[i].Xdiff = res;

                mCalcYDiffConv(srcData, ref lfMark, ref res);
                mDetectedmark[i].Ydiff = res;

                mCalcInOutAvg(srcData, ref lfMark, ref res);
                mDetectedmark[i].IO = res;

                mCalcLeftRightAvg(srcData, ref lfMark, ref res);
                mDetectedmark[i].LR = res;

                mCalcTopBottomAvg(srcData, ref lfMark, ref res);
                mDetectedmark[i].TB = res;

                mCalcCenterToLeftRightAvg(srcData, ref lfMark, ref res);
                mDetectedmark[i].CLR = res;

                mCalcCenterToTopBottomAvg(srcData, ref lfMark, ref res);
                mDetectedmark[i].CTB = res;

                mCalcFullStdev(srcData, ref lfMark, ref res);
                mDetectedmark[i].std = res;

                mCalcInFullStdev(srcData, ref lfMark, ref res);
                mDetectedmark[i].IFstd = res;
            }
            UpdateDataGridView1();
            UpdateDataGridView2();
            UpdateDataGridView3();

            double avgConv = 0;
            double minConv = 99999999;
            double avgSubConv = 0;
            double lSubConv = 0;
            double minSubConv = 99999999;
            double NGavgConv = 0;
            double NGmaxConv = 99999999;
            double NGavgSubConv = 0;
            double NGlSubConv = 0;
            double NGmaxSubConv = 99999999;
            int effCount = 0;
            int NGerrCount = 0;
            for (int i = 0; i < mMarkCount; i++)
            {
                if (!mDetectedmark[i].IsGood)
                {
                    NGerrCount++;
                    NGavgConv += mDetectedmark[i].conv;
                    if (NGmaxConv < mDetectedmark[i].conv)
                        NGmaxConv = mDetectedmark[i].conv;

                    switch (mNewSearchModel[mSearchModelIndex].planeShift % 100)
                    {
                        case 1:
                            NGlSubConv = mDetectedmark[i].diff;
                            break;
                        case 2:
                            NGlSubConv = mDetectedmark[i].Xdiff;
                            break;
                        case 3:
                            NGlSubConv = mDetectedmark[i].Ydiff;
                            break;
                        case 4:
                            NGlSubConv = mDetectedmark[i].IO;
                            break;
                        case 5:
                            NGlSubConv = mDetectedmark[i].LR;
                            break;
                        case 6:
                            NGlSubConv = mDetectedmark[i].TB;
                            break;
                        case 7:
                            NGlSubConv = mDetectedmark[i].CLR;
                            break;
                        case 8:
                            NGlSubConv = mDetectedmark[i].CTB;
                            break;
                        case 9:
                            NGlSubConv = mDetectedmark[i].std;
                            break;
                        case 10:
                            NGlSubConv = mDetectedmark[i].IFstd;
                            break;
                        case 11:
                            break;
                        case 12:
                            break;
                        default:
                            break;
                    }
                    NGavgSubConv += NGlSubConv;
                    if (NGmaxSubConv < NGlSubConv)
                        NGmaxSubConv = NGlSubConv;
                }
                else
                {
                    effCount++;
                    avgConv += mDetectedmark[i].conv;
                    if (minConv > mDetectedmark[i].conv)
                        minConv = mDetectedmark[i].conv;

                    switch (mNewSearchModel[mSearchModelIndex].planeShift % 100)
                    {
                        case 1:
                            lSubConv = mDetectedmark[i].diff;
                            break;
                        case 2:
                            lSubConv = mDetectedmark[i].Xdiff;
                            break;
                        case 3:
                            lSubConv = mDetectedmark[i].Ydiff;
                            break;
                        case 4:
                            lSubConv = mDetectedmark[i].IO;
                            break;
                        case 5:
                            lSubConv = mDetectedmark[i].LR;
                            break;
                        case 6:
                            lSubConv = mDetectedmark[i].TB;
                            break;
                        case 7:
                            lSubConv = mDetectedmark[i].CLR;
                            break;
                        case 8:
                            lSubConv = mDetectedmark[i].CTB;
                            break;
                        case 9:
                            lSubConv = mDetectedmark[i].std;
                            break;
                        case 10:
                            lSubConv = mDetectedmark[i].IFstd;
                            break;
                        case 11:
                            break;
                        case 12:
                            break;
                        default:
                            break;
                    }
                    avgSubConv += lSubConv;
                    if (minSubConv > lSubConv)
                        minSubConv = lSubConv;
                }

            }
            avgConv = avgConv / effCount;
            avgSubConv = avgSubConv / effCount;

            NGavgConv = NGavgConv / NGerrCount;
            NGavgSubConv = NGavgSubConv / NGerrCount;

            mNewSearchModel[mSearchModelIndex].conv = (int)(minConv - avgConv / 5);
            mNewSearchModel[mSearchModelIndex].planeHeight = minSubConv - avgSubConv / 5;

            lfMark = lOldModel;
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            //  PrevModel.txt 파일을 열어 들어있는 파일명을 읽어들이고 그 파일명의 파일을 모델파일로 읽어들인다.
        }

        // Diff
        // XDiff  
        // YDiff  
        // I-O    
        // L-R    
        // T-B   
        // C-LR   
        // C-TB 
        // σ 
        // IσFσ

        void CalcDiffConv(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;

            for (int i = 1; i < Spl.Width - 1; i++)
            {
                for (int j = 1; j < Spl.Height - 1; j++)
                {
                    data = srcData[i - 1 + j * Spl.Width] + srcData[i + (j - 1) * Spl.Width];
                    data = data - srcData[i + 1 + j * Spl.Width] - srcData[i + (j + 1) * Spl.Width];
                    model = lfmark.img[i - 1 + j * Spl.Width] + lfmark.img[i + (j - 1) * Spl.Width];
                    model = model - lfmark.img[i + 1 + j * Spl.Width] - lfmark.img[i + (j + 1) * Spl.Width];

                    res += data * model;
                    avgSrc += srcData[i + j * Spl.Width];
                    //avgModel += mSearchModel[modelIndex].img[i + j * Spl.Width];
                }
            }
            avgSrc = avgSrc / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of Src
            //avgModel = avgModel / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of Model

            res = res / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of SrcDiff * ModelDiff
                                                                //            res = res / (avgSrc * avgModel);    //   normalize
            res = res / avgSrc;    //   normalize
        }

        void CalcXDiffConv(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;

            for (int i = 1; i < Spl.Width - 1; i++)
            {
                for (int j = 1; j < Spl.Height - 1; j++)
                {
                    data = srcData[i - 1 + j * Spl.Width] + srcData[i - 1 + (j - 1) * Spl.Width] + srcData[i - 1 + (j + 1) * Spl.Width];
                    data = data - srcData[i + 1 + j * Spl.Width] - srcData[i + 1 + (j - 1) * Spl.Width] - srcData[i + 1 + (j + 1) * Spl.Width];
                    model = lfmark.img[i - 1 + j * Spl.Width] + lfmark.img[i - 1 + (j - 1) * Spl.Width] + lfmark.img[i - 1 + (j + 1) * Spl.Width];
                    model = model - lfmark.img[i + 1 + j * Spl.Width] - lfmark.img[i + 1 + (j - 1) * Spl.Width] - lfmark.img[i + 1 + (j + 1) * Spl.Width];

                    res += data * model;
                    avgSrc += srcData[i + j * Spl.Width];
                    //avgModel += mSearchModel[modelIndex].img[i - 1 + j * Spl.Width];
                }
            }
            avgSrc = avgSrc / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of Src
            //avgModel = avgModel / (Spl.Width - 2) * (Spl.Height - 2);   //  Avg of Model

            res = res / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of SrcDiff * ModelDiff
            //res = res / (avgSrc * avgModel);    //   normalize
            res = res / avgSrc;    //   normalize
        }

        void CalcYDiffConv(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;

            for (int i = 1; i < Spl.Width - 1; i++)
            {
                for (int j = 1; j < Spl.Height - 1; j++)
                {
                    data = srcData[i - 1 + (j - 1) * Spl.Width] + srcData[i + (j - 1) * Spl.Width] + srcData[i + 1 + (j - 1) * Spl.Width];
                    data = data - srcData[i - 1 + (j + 1) * Spl.Width] - srcData[i + (j + 1) * Spl.Width] - srcData[i + 1 + (j + 1) * Spl.Width];
                    model = lfmark.img[i - 1 + (j - 1) * Spl.Width] + lfmark.img[i + (j - 1) * Spl.Width] + lfmark.img[i + 1 + (j - 1) * Spl.Width];
                    model = model - lfmark.img[i - 1 + (j + 1) * Spl.Width] - lfmark.img[i + (j + 1) * Spl.Width] - lfmark.img[i + 1 + (j + 1) * Spl.Width];

                    res += data * model;
                    avgSrc += srcData[i + j * Spl.Width];
                    //avgModel += mSearchModel[modelIndex].img[i - 1 + j * Spl.Width];
                }
            }
            avgSrc = avgSrc / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of Src
            //avgModel = avgModel / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of Model

            res = res / ((Spl.Width - 2) * (Spl.Height - 2));   //  Avg of SrcDiff * ModelDiff
            //res = res / (avgSrc * avgModel);    //   normalize
            res = res / avgSrc;    //   normalize  
        }

        void CalcInOutAvg(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int icount = 0;
            int ocount = 0;
            double avgSrc = 0;
            double isrc = 0;
            double osrc = 0;

            for (int i = 0; i < Spl.Width; i++)
                for (int j = 0; j < Spl.Height; j++)
                    avgSrc += srcData[i + j * Spl.Width];

            avgSrc = avgSrc / (Spl.Width * Spl.Height);   //  Avg of Src

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    if (i < 2 || i > Spl.Width - 3 || j < 2 || j > Spl.Height - 3)
                    {
                        isrc += srcData[i + j * Spl.Width];
                        icount++;
                    }
                    else
                    {
                        osrc += srcData[i + j * Spl.Width];
                        ocount++;
                    }
                }
            }
            isrc = isrc / icount;
            osrc = osrc / ocount;

            res = (isrc - osrc) / avgSrc;
        }

        void CalcLeftRightAvg(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int Lcount = 0;
            int Rcount = 0;
            double avgSrc = 0;
            double Lsrc = 0;
            double Rsrc = 0;

            for (int i = 0; i < Spl.Width; i++)
                for (int j = 0; j < Spl.Height; j++)
                    avgSrc += srcData[i + j * Spl.Width];

            avgSrc = avgSrc / (Spl.Width * Spl.Height);   //  Avg of Src

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    if (i < Spl.Width / 2)
                    {
                        Lsrc += srcData[i + j * Spl.Width];
                        Lcount++;
                    }
                    else
                    {
                        Rsrc += srcData[i + j * Spl.Width];
                        Rcount++;
                    }
                }
            }
            Lsrc = Lsrc / Lcount;
            Rsrc = Rsrc / Rcount;

            res = (Lsrc - Rsrc) / avgSrc;
        }
        void CalcTopBottomAvg(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int Tcount = 0;
            int Bcount = 0;
            double avgSrc = 0;
            double Tsrc = 0;
            double Bsrc = 0;

            for (int i = 0; i < Spl.Width; i++)
                for (int j = 0; j < Spl.Height; j++)
                    avgSrc += srcData[i + j * Spl.Width];

            avgSrc = avgSrc / (Spl.Width * Spl.Height);   //  Avg of Src

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    if (j < Spl.Height / 2)
                    {
                        Tsrc += srcData[i + j * Spl.Width];
                        Tcount++;
                    }
                    else
                    {
                        Bsrc += srcData[i + j * Spl.Width];
                        Bcount++;
                    }
                }
            }
            Tsrc = Tsrc / Tcount;
            Bsrc = Bsrc / Bcount;

            res = (Tsrc - Bsrc) / avgSrc;
        }

        void CalcCenterToLeftRightAvg(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int LRcount = 0;
            int Ccount = 0;
            double avgSrc = 0;
            double LRsrc = 0;
            double Csrc = 0;

            for (int i = 0; i < Spl.Width; i++)
                for (int j = 0; j < Spl.Height; j++)
                    avgSrc += srcData[i + j * Spl.Width];

            avgSrc = avgSrc / (Spl.Width * Spl.Height);   //  Avg of Src

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    if (i < 2 || i > Spl.Width - 3)
                    {
                        LRsrc += srcData[i + j * Spl.Width];
                        LRcount++;
                    }
                    else if (i > 2 && i < Spl.Width - 3)
                    {
                        Csrc += srcData[i + j * Spl.Width];
                        Ccount++;
                    }
                }
            }
            LRsrc = LRsrc / LRcount;
            Csrc = Csrc / Ccount;

            res = (LRsrc - Csrc) / avgSrc;
        }

        void CalcCenterToTopBottomAvg(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int TBcount = 0;
            int Ccount = 0;
            double avgSrc = 0;
            double TBsrc = 0;
            double Csrc = 0;

            for (int i = 0; i < Spl.Width; i++)
                for (int j = 0; j < Spl.Height; j++)
                    avgSrc += srcData[i + j * Spl.Width];

            avgSrc = avgSrc / (Spl.Width * Spl.Height);   //  Avg of Src

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    if (j < 2 || j > Spl.Height - 3)
                    {
                        TBsrc += srcData[i + j * Spl.Width];
                        TBcount++;
                    }
                    else if (j > 2 && j < Spl.Height - 3)
                    {
                        Csrc += srcData[i + j * Spl.Width];
                        Ccount++;
                    }
                }
            }
            TBsrc = TBsrc / TBcount;
            Csrc = Csrc / Ccount;

            res = (TBsrc - Csrc) / avgSrc;
        }

        void CalcFullStdev(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int data = 0;
            int count = 0;
            double sum = 0;
            double ssum = 0;

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    data = srcData[i + j * Spl.Width];
                    sum += data;
                    ssum += data * data;
                    count++;
                }
            }
            res = Math.Sqrt(ssum / count - sum / count * sum / count) / (sum / count);
        }
        void CalcInFullStdev(Mat Spl, ref sFiducialMark lfmark, ref double res)
        {
            byte[] srcData;
            Spl.GetArray(out srcData);
            int data = 0;
            int i_count = 0;
            double i_sum = 0;
            double i_ssum = 0;
            double i_avg = 0;
            int f_count = 0;
            double f_sum = 0;
            double f_ssum = 0;
            double f_avg = 0;

            for (int i = 0; i < Spl.Width; i++)
            {
                for (int j = 0; j < Spl.Height; j++)
                {
                    data = srcData[i + j * Spl.Width];
                    if (i > 3 && i < Spl.Width - 4 && j > 2 && j < Spl.Height - 3)
                    {
                        i_sum += data;
                        i_ssum += data * data;
                        i_count++;
                    }
                    f_sum += data;
                    f_ssum += data * data;
                    f_count++;
                }
            }
            f_avg = f_sum / f_count;
            i_avg = i_sum / i_count;

            double i_std = Math.Sqrt(i_ssum / i_count - i_avg * i_avg);
            double f_std = Math.Sqrt(f_ssum / f_count - f_avg * f_avg);
            res = (i_std - f_std) / ((f_sum + i_sum) / (i_count + f_count));
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //void mCalcDiffConv(byte[] srcData, int modelIndex, ref double res)
        void mCalcDiffConv(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;
            int lwidth  = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 1; i < lwidth - 1; i++)
            {
                for (int j = 1; j < lheight - 1; j++)
                {
                    data = srcData[i - 1 + j * lwidth] + srcData[i + (j - 1) * lwidth];
                    data = data - srcData[i + 1 + j * lwidth] - srcData[i + (j + 1) * lwidth];
                    model = lfmark.img[i - 1 + j * lwidth] + lfmark.img[i + (j - 1) * lwidth];
                    model = model - lfmark.img[i + 1 + j * lwidth] - lfmark.img[i + (j + 1) * lwidth];

                    res += data * model;
                    avgSrc += srcData[i + j * lwidth];
                }
            }
            avgSrc = avgSrc / ((lwidth - 2) * (lheight - 2));   //  Avg of Src

            res = res / ((lwidth - 2) * (lheight - 2));   //  Avg of SrcDiff * ModelDiff
                                                          //            res = res / (avgSrc * avgModel);    //   normalize
            res = res / avgSrc;    //   normalize
        }

        void mCalcXDiffConv(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;
            int lwidth  = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 1; i < lwidth - 1; i++)
            {
                for (int j = 1; j < lheight - 1; j++)
                {
                    data = srcData[i - 1 + j * lwidth] + srcData[i - 1 + (j - 1) * lwidth] + srcData[i - 1 + (j + 1) * lwidth];
                    data = data - srcData[i + 1 + j * lwidth] - srcData[i + 1 + (j - 1) * lwidth] - srcData[i + 1 + (j + 1) * lwidth];
                    model = lfmark.img[i - 1 + j * lwidth] + lfmark.img[i - 1 + (j - 1) * lwidth] + lfmark.img[i - 1 + (j + 1) * lwidth];
                    model = model - lfmark.img[i + 1 + j * lwidth] - lfmark.img[i + 1 + (j - 1) * lwidth] - lfmark.img[i + 1 + (j + 1) * lwidth];

                    res += data * model;
                    avgSrc += srcData[i + j * lwidth];
                }
            }
            avgSrc = avgSrc / ((lwidth - 2) * (lheight - 2));   //  Avg of Src

            res = res / ((lwidth - 2) * (lheight - 2));   //  Avg of SrcDiff * ModelDiff
            res = res / avgSrc;    //   normalize
        }

        void mCalcYDiffConv(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int data = 0;
            int model = 0;
            double avgSrc = 0;
            //double avgModel = 0;
            int lwidth  = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 1; i < lwidth - 1; i++)
            {
                for (int j = 1; j < lheight - 1; j++)
                {
                    data = srcData[i - 1 + (j - 1) * lwidth] + srcData[i + (j - 1) * lwidth] + srcData[i + 1 + (j - 1) * lwidth];
                    data = data - srcData[i - 1 + (j + 1) * lwidth] - srcData[i + (j + 1) * lwidth] - srcData[i + 1 + (j + 1) * lwidth];
                    model = lfmark.img[i - 1 + (j - 1) * lwidth] + lfmark.img[i + (j - 1) * lwidth] + lfmark.img[i + 1 + (j - 1) * lwidth];
                    model = model - lfmark.img[i - 1 + (j + 1) * lwidth] - lfmark.img[i + (j + 1) * lwidth] - lfmark.img[i + 1 + (j + 1) * lwidth];

                    res += data * model;
                    avgSrc += srcData[i + j * lwidth];
                    //avgModel += mSearchModel[modelIndex].img[i - 1 + j * lwidth];
                }
            }
            avgSrc = avgSrc / ((lwidth - 2) * (lheight - 2));   //  Avg of Src
            //avgModel = avgModel / ((lwidth - 2) * (lheight - 2));   //  Avg of Model

            res = res / ((lwidth - 2) * (lheight - 2));   //  Avg of SrcDiff * ModelDiff
            //res = res / (avgSrc * avgModel);    //   normalize
            res = res / avgSrc;    //   normalize  
        }

        void mCalcInOutAvg(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int icount = 0;
            int ocount = 0;
            double avgSrc = 0;
            double isrc = 0;
            double osrc = 0;
            int lwidth  = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 0; i < lwidth; i++)
                for (int j = 0; j < lheight; j++)
                    avgSrc += srcData[i + j * lwidth];

            avgSrc = avgSrc / (lwidth * lheight);   //  Avg of Src

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    if (i < 2 || i > lwidth - 3 || j < 2 || j > lheight - 3)
                    {
                        isrc += srcData[i + j * lwidth];
                        icount++;
                    }
                    else
                    {
                        osrc += srcData[i + j * lwidth];
                        ocount++;
                    }
                }
            }
            isrc = isrc / icount;
            osrc = osrc / ocount;

            res = (isrc - osrc) / avgSrc;
        }

        void mCalcLeftRightAvg(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int Lcount = 0;
            int Rcount = 0;
            double avgSrc = 0;
            double Lsrc = 0;
            double Rsrc = 0;
            int lwidth  = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;


            for (int i = 0; i < lwidth; i++)
                for (int j = 0; j < lheight; j++)
                    avgSrc += srcData[i + j * lwidth];

            avgSrc = avgSrc / (lwidth * lheight);   //  Avg of Src

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    if (i < lwidth / 2)
                    {
                        Lsrc += srcData[i + j * lwidth];
                        Lcount++;
                    }
                    else
                    {
                        Rsrc += srcData[i + j * lwidth];
                        Rcount++;
                    }
                }
            }
            Lsrc = Lsrc / Lcount;
            Rsrc = Rsrc / Rcount;

            res = (Lsrc - Rsrc) / avgSrc;
        }
        void mCalcTopBottomAvg(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int Tcount = 0;
            int Bcount = 0;
            double avgSrc = 0;
            double Tsrc = 0;
            double Bsrc = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 0; i < lwidth; i++)
                for (int j = 0; j < lheight; j++)
                    avgSrc += srcData[i + j * lwidth];

            avgSrc = avgSrc / (lwidth * lheight);   //  Avg of Src

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    if (j < lheight / 2)
                    {
                        Tsrc += srcData[i + j * lwidth];
                        Tcount++;
                    }
                    else
                    {
                        Bsrc += srcData[i + j * lwidth];
                        Bcount++;
                    }
                }
            }
            Tsrc = Tsrc / Tcount;
            Bsrc = Bsrc / Bcount;

            res = (Tsrc - Bsrc) / avgSrc;
        }

        void mCalcCenterToLeftRightAvg(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int LRcount = 0;
            int Ccount = 0;
            double avgSrc = 0;
            double LRsrc = 0;
            double Csrc = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 0; i < lwidth; i++)
                for (int j = 0; j < lheight; j++)
                    avgSrc += srcData[i + j * lwidth];

            avgSrc = avgSrc / (lwidth * lheight);   //  Avg of Src

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    if (i < 2 || i > lwidth - 3)
                    {
                        LRsrc += srcData[i + j * lwidth];
                        LRcount++;
                    }
                    else if (i > 2 && i < lwidth - 3)
                    {
                        Csrc += srcData[i + j * lwidth];
                        Ccount++;
                    }
                }
            }
            LRsrc = LRsrc / LRcount;
            Csrc = Csrc / Ccount;

            res = (LRsrc - Csrc) / avgSrc;
        }

        void mCalcCenterToTopBottomAvg(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int TBcount = 0;
            int Ccount = 0;
            double avgSrc = 0;
            double TBsrc = 0;
            double Csrc = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;


            for (int i = 0; i < lwidth; i++)
                for (int j = 0; j < lheight; j++)
                    avgSrc += srcData[i + j * lwidth];

            avgSrc = avgSrc / (lwidth * lheight);   //  Avg of Src

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    if (j < 2 || j > lheight - 3)
                    {
                        TBsrc += srcData[i + j * lwidth];
                        TBcount++;
                    }
                    else if (j > 2 && j < lheight - 3)
                    {
                        Csrc += srcData[i + j * lwidth];
                        Ccount++;
                    }
                }
            }
            TBsrc = TBsrc / TBcount;
            Csrc = Csrc / Ccount;

            res = (TBsrc - Csrc) / avgSrc;
        }

        void mCalcFullStdev(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int data = 0;
            int count = 0;
            double sum = 0;
            double ssum = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    data = srcData[i + j * lwidth];
                    sum += data;
                    ssum += data * data;
                    count++;
                }
            }
            res = Math.Sqrt(ssum / count - sum / count * sum / count) / (sum / count);
        }
        void mCalcInFullStdev(byte[] srcData, ref sFiducialMark lfmark, ref double res)
        {
            int data = 0;
            int i_count = 0;
            double i_sum = 0;
            double i_ssum = 0;
            double i_avg = 0;
            int f_count = 0;
            double f_sum = 0;
            double f_ssum = 0;
            double f_avg = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;

            for (int i = 0; i < lwidth; i++)
            {
                for (int j = 0; j < lheight; j++)
                {
                    data = srcData[i + j * lwidth];
                    if (i > 3 && i < lwidth - 4 && j > 2 && j < lheight - 3)
                    {
                        i_sum += data;
                        i_ssum += data * data;
                        i_count++;
                    }
                    f_sum += data;
                    f_ssum += data * data;
                    f_count++;
                }
            }
            f_avg = f_sum / f_count;
            i_avg = i_sum / i_count;

            double i_std = Math.Sqrt(i_ssum / i_count - i_avg * i_avg);
            double f_std = Math.Sqrt(f_ssum / f_count - f_avg * f_avg);
            res = (i_std - f_std) / ((f_sum + i_sum) / (i_count + f_count));
        }

        ////////////////////////////////////////////////
        ////////////////////////////////////////////////
        ////////////////////////////////////////////////

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            mOrgFile = listBox1.Items[listBox1.SelectedIndex].ToString();


            //if (mSourceImg[0] == null)
            //    mSourceImgFile = new Mat(mOrgFile);
            //else
            //    mSourceImgFile = Cv2.ImRead(mOrgFile);

            //Cv2.CvtColor(mSourceImgFile, mSourceImg[0], ColorConversionCodes.BGR2GRAY);

            mSourceImg[0] = new Mat(mOrgFile, ImreadModes.Grayscale);
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mSourceImg[0]);
            mOverlayedImg = null;

            //   무조건 1:1 으로 보여져야 모델생성을 제대로 할 수 있다.
            pictureBox2.Size = new System.Drawing.Size(mSourceImg[0].Width, mSourceImg[0].Height);
            pictureBox2.Image = myImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //  G / NG 상태를 바꾸면서 그때그때 Data 를 G, NG datagridview 및 data 소속을 바꾼다.
            if (mDetectedmark == null)
                return;

            if (e.ColumnIndex == 4)
            {

                if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "G")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "NG";
                    MoveToNGdata(e.RowIndex);
                }
                else
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "G";
                    MoveToGdata(e.RowIndex);
                }
            }
        }
        private void MoveToNGdata(int rawIndex)
        {
            mDetectedmark[rawIndex].IsGood = false;
            int i = 0;
            bool IsFound = false;

            for (i = 0; i < mMarkCount; i++)
            {
                if (mNGIndex[i] == rawIndex)
                    return;
            }
            for (i = 0; i < mMarkCount; i++)
            {
                if (mNGIndex[i] < 0)
                {
                    mNGIndex[i] = rawIndex;
                    break;
                }
            }
            for (i = 0; i < mMarkCount; i++)
            {
                if (mGIndex[i] == rawIndex)
                {
                    IsFound = true;
                    break;
                }
            }
            if (IsFound)
                for (; i < mMarkCount; i++)
                    mGIndex[i] = mGIndex[i + 1];

            //  Update datagridview2, datagridview3 according to mGIndex[], mNGIndex[]
            UpdateDataGridView2();
            UpdateDataGridView3();
            UpdateDist();
        }

        private void MoveToGdata(int rawIndex)
        {
            mDetectedmark[rawIndex].IsGood = true;
            int i = 0;
            bool IsFound = false;

            for (i = 0; i < mMarkCount; i++)
            {
                if (mGIndex[i] == rawIndex)
                    return;
            }
            for (i = 0; i < mMarkCount; i++)
            {
                if (mGIndex[i] < 0)
                {
                    mGIndex[i] = rawIndex;
                    break;
                }
            }
            for (i = 0; i < mMarkCount; i++)
            {
                if (mNGIndex[i] == rawIndex)
                {
                    IsFound = true;
                    break;

                }
            }
            if (IsFound)
                for (; i < mMarkCount; i++)
                    mNGIndex[i] = mNGIndex[i + 1];

            //  Update datagridview2, datagridview3 according to mGIndex[], mNGIndex[]
            UpdateDataGridView2();
            UpdateDataGridView3();
            UpdateDist();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 해당 마크가 소속된 Image 를 화면에 보여주고 해당 마크에 굵은 박스를 표시한다.
            if (e.RowIndex < 0)
                return;
            ShowFocusedMark(e.RowIndex);
        }

        public void ShowFocusedMark(int focused)
        {
            if (mDetectedmark == null)
                return;
            mMarkFocused = focused;
            if (mMarkFocused > mMarkCount)
                return;

            if (mDetectedmark[mMarkFocused].imgFile == null)
                return;

            Bitmap myImage;
            if (mDetectedmark[mMarkFocused].imgFile != mOrgFile)
            {
                mOrgFile = mDetectedmark[mMarkFocused].imgFile;
                mSourceImg[0] = new Mat(mOrgFile);

                Mat src = new Mat();

                ToGrayScale(mSourceImg[0], src);
                src.CopyTo(mSourceImg[0]);
            }


            Cv2.CvtColor(mSourceImg[0], mOverlayedImg, ColorConversionCodes.GRAY2RGB);

            //Cv2.PutText(mSchematicOverlayedImg, "N", new OpenCvSharp.Point(x - 20, y / 2 - 8), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(32, 32, 255), 2);
            Cv2.PutText(mOverlayedImg, mDetectedmark[mMarkFocused].imgFile, new OpenCvSharp.Point(1,10), HersheyFonts.HersheyPlain, 0.7, Scalar.FromRgb(255, 32, 32), 1);
            for (int i = 0; i < mMarkCount; i++)
            {
                if (mDetectedmark[i].imgFile != mOrgFile)
                    continue;

                if (i == mMarkFocused)
                    mOverlayedImg.Rectangle(mDetectedRect[mSearchModelIndex][i], Scalar.Cyan, 2);
                else
                    mOverlayedImg.Rectangle(mDetectedRect[mSearchModelIndex][i], Scalar.Cyan, 1);
            }
            myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);
            pictureBox2.Image = myImage;

        }

        private void UpdateDataGridView1()
        {
            for (int i = 0; i < mMarkCount; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i.ToString();
                dataGridView1.Rows[i].Cells[1].Value = mDetectedmark[i].rect.X.ToString("F0");
                dataGridView1.Rows[i].Cells[2].Value = mDetectedmark[i].rect.Y.ToString("F0");
                dataGridView1.Rows[i].Cells[3].Value = mDetectedmark[i].conv.ToString("F0");
                dataGridView1.Rows[i].Cells[4].Value = "G";
            }
            //this.dataGridView1.Columns[0].Name = "Spl No.";
            //this.dataGridView1.Columns[1].Name = "X";
            //this.dataGridView1.Columns[2].Name = "Y";
            //this.dataGridView1.Columns[3].Name = "Conv";
            //this.dataGridView1.Columns[4].Name = "G / NG";
        }

        private void ClearDataGridView1()
        {
            for (int i = 0; i < 1000; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i.ToString();
                dataGridView1.Rows[i].Cells[1].Value = "0";
                dataGridView1.Rows[i].Cells[2].Value = "0";
                dataGridView1.Rows[i].Cells[3].Value = "0";
                dataGridView1.Rows[i].Cells[4].Value = "-";
            }
            mMarkCount = 0;
            //this.dataGridView1.Columns[0].Name = "Spl No.";
            //this.dataGridView1.Columns[1].Name = "X";
            //this.dataGridView1.Columns[2].Name = "Y";
            //this.dataGridView1.Columns[3].Name = "Conv";
            //this.dataGridView1.Columns[4].Name = "G / NG";
        }

        private void UpdateDataGridView2()
        {
            dataGridView2.Rows.Clear();
            double avg_conv = 0;
            double avg_diff = 0;
            double avg_Xdiff = 0;
            double avg_Ydiff = 0;
            double avg_IO = 0;
            double avg_LR = 0;
            double avg_TB = 0;
            double avg_CLR = 0;
            double avg_CTB = 0;
            double avg_std = 0;
            double avg_IFstd = 0;

            double std_conv = 0;
            double std_diff = 0;
            double std_Xdiff = 0;
            double std_Ydiff = 0;
            double std_IO = 0;
            double std_LR = 0;
            double std_TB = 0;
            double std_CLR = 0;
            double std_CTB = 0;
            double std_std = 0;
            double std_IFstd = 0;

            int effCount = 0;

            for (int i = 0; i < mMarkCount; i++)
            {
                if (mGIndex[i] < 0)
                    break;
                avg_conv += mDetectedmark[mGIndex[i]].conv;
                avg_diff += mDetectedmark[mGIndex[i]].diff;
                avg_Xdiff += mDetectedmark[mGIndex[i]].Xdiff;
                avg_Ydiff += mDetectedmark[mGIndex[i]].Ydiff;
                avg_IO += mDetectedmark[mGIndex[i]].IO;
                avg_LR += mDetectedmark[mGIndex[i]].LR;
                avg_TB += mDetectedmark[mGIndex[i]].TB;
                avg_CLR += mDetectedmark[mGIndex[i]].CLR;
                avg_CTB += mDetectedmark[mGIndex[i]].CTB;
                avg_std += mDetectedmark[mGIndex[i]].std;
                avg_IFstd += mDetectedmark[mGIndex[i]].IFstd;

                std_conv += (mDetectedmark[mGIndex[i]].conv * mDetectedmark[mGIndex[i]].conv);
                std_diff += (mDetectedmark[mGIndex[i]].diff * mDetectedmark[mGIndex[i]].diff);
                std_Xdiff += (mDetectedmark[mGIndex[i]].Xdiff * mDetectedmark[mGIndex[i]].Xdiff);
                std_Ydiff += (mDetectedmark[mGIndex[i]].Ydiff * mDetectedmark[mGIndex[i]].Ydiff);
                std_IO += (mDetectedmark[mGIndex[i]].IO * mDetectedmark[mGIndex[i]].IO);
                std_LR += (mDetectedmark[mGIndex[i]].LR * mDetectedmark[mGIndex[i]].LR);
                std_TB += (mDetectedmark[mGIndex[i]].TB * mDetectedmark[mGIndex[i]].TB);
                std_CLR += (mDetectedmark[mGIndex[i]].CLR * mDetectedmark[mGIndex[i]].CLR);
                std_CTB += (mDetectedmark[mGIndex[i]].CTB * mDetectedmark[mGIndex[i]].CTB);
                std_std += (mDetectedmark[mGIndex[i]].std * mDetectedmark[mGIndex[i]].std);
                std_IFstd += (mDetectedmark[mGIndex[i]].IFstd * mDetectedmark[mGIndex[i]].IFstd);

                effCount++;
            }
            avg_conv = avg_conv / effCount;
            avg_diff = avg_diff / effCount;
            avg_Xdiff = avg_Xdiff / effCount;
            avg_Ydiff = avg_Ydiff / effCount;
            avg_IO = avg_IO / effCount;
            avg_LR = avg_LR / effCount;
            avg_TB = avg_TB / effCount;
            avg_CLR = avg_CLR / effCount;
            avg_CTB = avg_CTB / effCount;
            avg_std = avg_std / effCount;
            avg_IFstd = avg_IFstd / effCount;
            if (effCount > 0)
            {
                std_conv = Math.Sqrt(std_conv / effCount - avg_conv * avg_conv);
                std_diff = Math.Sqrt(std_diff / effCount - avg_diff * avg_diff);
                std_Xdiff = Math.Sqrt(std_Xdiff / effCount - avg_Xdiff * avg_Xdiff);
                std_Ydiff = Math.Sqrt(std_Ydiff / effCount - avg_Ydiff * avg_Ydiff);
                std_IO = Math.Sqrt(std_IO / effCount - avg_IO * avg_IO);
                std_LR = Math.Sqrt(std_LR / effCount - avg_LR * avg_LR);
                std_TB = Math.Sqrt(std_TB / effCount - avg_TB * avg_TB);
                std_CLR = Math.Sqrt(std_CLR / effCount - avg_CLR * avg_CLR);
                std_CTB = Math.Sqrt(std_CTB / effCount - avg_CTB * avg_CTB);
                std_std = Math.Sqrt(std_std / effCount - avg_std * avg_std);
                std_IFstd = Math.Sqrt(std_IFstd / effCount - avg_IFstd * avg_IFstd);
            }

            dataGridView2.Rows.Add("Avg", "-", "-",
                                    avg_conv.ToString("F2"),
                                    avg_diff.ToString("F2"),
                                    avg_Xdiff.ToString("F2"),
                                    avg_Ydiff.ToString("F2"),
                                    avg_IO.ToString("F2"),
                                    avg_LR.ToString("F2"),
                                    avg_TB.ToString("F2"),
                                    avg_CLR.ToString("F2"),
                                    avg_CTB.ToString("F2"),
                                    avg_std.ToString("F2"),
                                    avg_IFstd.ToString("F2")
                                    );
            dataGridView2.Rows.Add("Std", "-", "-",
                                    std_conv.ToString("F2"),
                                    std_diff.ToString("F2"),
                                    std_Xdiff.ToString("F2"),
                                    std_Ydiff.ToString("F2"),
                                    std_IO.ToString("F2"),
                                    std_LR.ToString("F2"),
                                    std_TB.ToString("F2"),
                                    std_CLR.ToString("F2"),
                                    std_CTB.ToString("F2"),
                                    std_std.ToString("F2"),
                                    std_IFstd.ToString("F2")
                                    );
            dataGridView2.Rows.Add("Dist", "-", "-", "-",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0"
                                    );
            for (int i = 0; i < 3; i++)
            {
                dataGridView2.Rows[i].Height = 16;
                dataGridView2.Rows[i].Resizable = DataGridViewTriState.False;
                dataGridView2.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                dataGridView2.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(208, 208, 240);
                dataGridView2.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            }
            for (int i = 0; i < mMarkCount; i++)
            {
                if (mGIndex[i] < 0)
                    break;
                dataGridView2.Rows.Add(i.ToString(), mDetectedmark[mGIndex[i]].rect.X.ToString("F0"), mDetectedmark[mGIndex[i]].rect.Y.ToString("F0"),
                                        mDetectedmark[mGIndex[i]].conv.ToString("F0"),
                                        mDetectedmark[mGIndex[i]].diff.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].Xdiff.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].Ydiff.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].IO.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].LR.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].TB.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].CLR.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].CTB.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].std.ToString("F2"),
                                        mDetectedmark[mGIndex[i]].IFstd.ToString("F2")
                                       );

                dataGridView2.Rows[i + 3].Height = 16;
                dataGridView2.Rows[i + 3].Resizable = DataGridViewTriState.False;
                dataGridView2.Rows[i + 3].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                dataGridView2.Rows[i + 3].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                dataGridView2.Rows[i + 3].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);

            }

            for (int colum = 3; colum < this.dataGridView2.ColumnCount; colum++)
            {
                for (int row = 3; row < this.dataGridView2.Rows.Count; row++)
                {
                    this.dataGridView2[colum, row].Style.BackColor = System.Drawing.Color.FromArgb(8, 8, 24);
                    this.dataGridView2[colum, row].Style.ForeColor = System.Drawing.Color.FromArgb(208, 208, 240);
                    this.dataGridView2.ReadOnly = true;
                }
            }
        }

        private void UpdateDataGridView3()
        {
            dataGridView3.Rows.Clear();
            double avg_conv = 0;
            double avg_diff = 0;
            double avg_Xdiff = 0;
            double avg_Ydiff = 0;
            double avg_IO = 0;
            double avg_LR = 0;
            double avg_TB = 0;
            double avg_CLR = 0;
            double avg_CTB = 0;
            double avg_std = 0;
            double avg_IFstd = 0;

            double std_conv = 0;
            double std_diff = 0;
            double std_Xdiff = 0;
            double std_Ydiff = 0;
            double std_IO = 0;
            double std_LR = 0;
            double std_TB = 0;
            double std_CLR = 0;
            double std_CTB = 0;
            double std_std = 0;
            double std_IFstd = 0;

            int effCount = 0;

            for (int i = 0; i < mMarkCount; i++)
            {
                if (mNGIndex[i] < 0)
                    break;
                avg_conv += mDetectedmark[mNGIndex[i]].conv;
                avg_diff += mDetectedmark[mNGIndex[i]].diff;
                avg_Xdiff += mDetectedmark[mNGIndex[i]].Xdiff;
                avg_Ydiff += mDetectedmark[mNGIndex[i]].Ydiff;
                avg_IO += mDetectedmark[mNGIndex[i]].IO;
                avg_LR += mDetectedmark[mNGIndex[i]].LR;
                avg_TB += mDetectedmark[mNGIndex[i]].TB;
                avg_CLR += mDetectedmark[mNGIndex[i]].CLR;
                avg_CTB += mDetectedmark[mNGIndex[i]].CTB;
                avg_std += mDetectedmark[mNGIndex[i]].std;
                avg_IFstd += mDetectedmark[mNGIndex[i]].IFstd;

                std_conv += (mDetectedmark[mNGIndex[i]].conv * mDetectedmark[mNGIndex[i]].conv);
                std_diff += (mDetectedmark[mNGIndex[i]].diff * mDetectedmark[mNGIndex[i]].diff);
                std_Xdiff += (mDetectedmark[mNGIndex[i]].Xdiff * mDetectedmark[mNGIndex[i]].Xdiff);
                std_Ydiff += (mDetectedmark[mNGIndex[i]].Ydiff * mDetectedmark[mNGIndex[i]].Ydiff);
                std_IO += (mDetectedmark[mNGIndex[i]].IO * mDetectedmark[mNGIndex[i]].IO);
                std_LR += (mDetectedmark[mNGIndex[i]].LR * mDetectedmark[mNGIndex[i]].LR);
                std_TB += (mDetectedmark[mNGIndex[i]].TB * mDetectedmark[mNGIndex[i]].TB);
                std_CLR += (mDetectedmark[mNGIndex[i]].CLR * mDetectedmark[mNGIndex[i]].CLR);
                std_CTB += (mDetectedmark[mNGIndex[i]].CTB * mDetectedmark[mNGIndex[i]].CTB);
                std_std += (mDetectedmark[mNGIndex[i]].std * mDetectedmark[mNGIndex[i]].std);
                std_IFstd += (mDetectedmark[mNGIndex[i]].IFstd * mDetectedmark[mNGIndex[i]].IFstd);

                effCount++;
            }
            avg_conv = avg_conv / effCount;
            avg_diff = avg_diff / effCount;
            avg_Xdiff = avg_Xdiff / effCount;
            avg_Ydiff = avg_Ydiff / effCount;
            avg_IO = avg_IO / effCount;
            avg_LR = avg_LR / effCount;
            avg_TB = avg_TB / effCount;
            avg_CLR = avg_CLR / effCount;
            avg_CTB = avg_CTB / effCount;
            avg_std = avg_std / effCount;
            avg_IFstd = avg_IFstd / effCount;
            if (effCount > 0)
            {
                std_conv = Math.Sqrt(std_conv / effCount - avg_conv * avg_conv);
                std_diff = Math.Sqrt(std_diff / effCount - avg_diff * avg_diff);
                std_Xdiff = Math.Sqrt(std_Xdiff / effCount - avg_Xdiff * avg_Xdiff);
                std_Ydiff = Math.Sqrt(std_Ydiff / effCount - avg_Ydiff * avg_Ydiff);
                std_IO = Math.Sqrt(std_IO / effCount - avg_IO * avg_IO);
                std_LR = Math.Sqrt(std_LR / effCount - avg_LR * avg_LR);
                std_TB = Math.Sqrt(std_TB / effCount - avg_TB * avg_TB);
                std_CLR = Math.Sqrt(std_CLR / effCount - avg_CLR * avg_CLR);
                std_CTB = Math.Sqrt(std_CTB / effCount - avg_CTB * avg_CTB);
                std_std = Math.Sqrt(std_std / effCount - avg_std * avg_std);
                std_IFstd = Math.Sqrt(std_IFstd / effCount - avg_IFstd * avg_IFstd);
            }

            dataGridView3.Rows.Add("Avg", "-", "-",
                                    avg_conv.ToString("F0"),
                                    avg_diff.ToString("F2"),
                                    avg_Xdiff.ToString("F2"),
                                    avg_Ydiff.ToString("F2"),
                                    avg_IO.ToString("F2"),
                                    avg_LR.ToString("F2"),
                                    avg_TB.ToString("F2"),
                                    avg_CLR.ToString("F2"),
                                    avg_CTB.ToString("F2"),
                                    avg_std.ToString("F2"),
                                    avg_IFstd.ToString("F2")
                                    );
            dataGridView3.Rows.Add("Std", "-", "-",
                                    std_conv.ToString("F0"),
                                    std_diff.ToString("F2"),
                                    std_Xdiff.ToString("F2"),
                                    std_Ydiff.ToString("F2"),
                                    std_IO.ToString("F2"),
                                    std_LR.ToString("F2"),
                                    std_TB.ToString("F2"),
                                    std_CLR.ToString("F2"),
                                    std_CTB.ToString("F2"),
                                    std_std.ToString("F2"),
                                    std_IFstd.ToString("F2")
                                    );
            dataGridView3.Rows.Add("Dist", "-", "-", "-",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0",
                                    "0"
                                    );
            for (int i = 0; i < 3; i++)
            {
                dataGridView3.Rows[i].Height = 16;
                dataGridView3.Rows[i].Resizable = DataGridViewTriState.False;
                dataGridView3.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                dataGridView3.Rows[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(240, 208, 208);
                dataGridView3.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(28, 28, 28);
            }
            mNGCount = 0;
            for (int i = 0; i < mMarkCount; i++)
            {
                if (mNGIndex[i] < 0)
                    break;
                mNGCount++;
                dataGridView3.Rows.Add(i.ToString(), mDetectedmark[mNGIndex[i]].rect.X.ToString("F0"), mDetectedmark[mNGIndex[i]].rect.Y.ToString("F0"),
                                        mDetectedmark[mNGIndex[i]].conv.ToString("F0"),
                                        mDetectedmark[mNGIndex[i]].diff.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].Xdiff.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].Ydiff.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].IO.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].LR.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].TB.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].CLR.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].CTB.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].std.ToString("F2"),
                                        mDetectedmark[mNGIndex[i]].IFstd.ToString("F2")
                                       );
                dataGridView3.Rows[i + 3].Height = 16;
                dataGridView3.Rows[i + 3].Resizable = DataGridViewTriState.False;
                dataGridView3.Rows[i + 3].DefaultCellStyle.Font = new Font("Calibri", 8, FontStyle.Regular);
                dataGridView3.Rows[i + 3].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(8, 8, 8);
                dataGridView3.Rows[i + 3].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(208, 208, 208);
            }
            for (int colum = 3; colum < this.dataGridView3.ColumnCount; colum++)
            {
                for (int row = 3; row < this.dataGridView3.Rows.Count; row++)
                {
                    this.dataGridView3[colum, row].Style.BackColor = System.Drawing.Color.FromArgb(24, 8, 8);
                    this.dataGridView3[colum, row].Style.ForeColor = System.Drawing.Color.FromArgb(240, 208, 208);
                    this.dataGridView3.ReadOnly = true;
                }
            }
        }

        private void UpdateDist()
        {
            double Gavg = 0;
            double NGavg = 0;

            double Gstd = 0;
            double NGstd = 0;
            double dist = 0;
            double distmax = 0;
            int maxIndex = 0;
            for (int i = 3; i < 14; i++)
            {
                Gavg = Convert.ToDouble(dataGridView2.Rows[0].Cells[i].Value.ToString());
                Gstd = Convert.ToDouble(dataGridView2.Rows[1].Cells[i].Value.ToString());

                NGavg = Convert.ToDouble(dataGridView3.Rows[0].Cells[i].Value.ToString());
                NGstd = Convert.ToDouble(dataGridView3.Rows[1].Cells[i].Value.ToString());

                dist = 2 * Math.Abs((Gavg - NGavg) / (Gstd + NGstd));
                dataGridView2.Rows[2].Cells[i].Value = dist.ToString("F2");
                dataGridView3.Rows[2].Cells[i].Value = dist.ToString("F2");

                if (distmax < dist)
                {
                    maxIndex = i;
                    distmax = dist;
                }
            }
            UpdateSelectSub(maxIndex);
        }

        byte[] mUpDown = new byte[12];

        int[] LUTBgNoise = new int[1000];
        int[] LUTBgNoiseY = new int[1000];

        public void SetBackgroundNoise(int[] bgNoise)
        {
            LUTBgNoise = new int[bgNoise.Length];
            Array.Copy(bgNoise, LUTBgNoise, bgNoise.Length);
        }
        public void SetBackgroundNoise(int[] bgNoise, int[] bgNoiseY)
        {
            LUTBgNoise = new int[bgNoise.Length];
            Array.Copy(bgNoise, LUTBgNoise, bgNoise.Length);

            LUTBgNoiseY = new int[bgNoiseY.Length];
            Array.Copy(bgNoiseY, LUTBgNoiseY, bgNoiseY.Length);
        }


        bool[] mDoneBuf = new bool[19*19];
        //System.Drawing.Point[] mDonePos = new System.Drawing.Point[19 * 19];
        System.Drawing.Point[][] mPrevPos = new System.Drawing.Point[30][];
        long[][] mPrevConv = new long[30][];
        public int[] mIndexInThread = new int[30];

        public OpenCvSharp.Point[] FineCOG(bool IsFirst, int iIndex, ref sMarkResult[] smr, ref sMarkResult[] smr_T, ref sMarkResult[] smr_B, ref long Nfound, bool IsDebug = false, int iBuf = 0, int whichModel = -1)
        {
                //  q_Value[iBuf] 에서 Object 를 찾은 뒤
                //  mSourceImg[iBuf] 에서 정밀좌표를 추출한다.
            if (IsFirst)
            {
                for ( int cntBuf = 0; cntBuf < mPrevPos.Length; cntBuf ++ )
                {
                    mPrevPos[cntBuf] = new System.Drawing.Point[12];
                    mPrevConv[cntBuf] = new long[12];
                    int iB = 0;
                    for (; iB < mMarkPosOnPanel.Length; iB++)
                    {
                        mPrevPos[cntBuf][iB] = new System.Drawing.Point(mMarkPosOnPanel[iB].X/ mModelScale, mMarkPosOnPanel[iB].Y/ mModelScale);
                        mPrevConv[cntBuf][iB] = mMarkConvOnPanel[iB];
                    }
                    for (; iB < 12; iB++)
                    {
                        mPrevPos[cntBuf][iB] = new System.Drawing.Point(-1, -1);
                        mPrevConv[cntBuf][iB] = 0;
                    }
                }
            }
            mIndexInThread[iBuf] = iIndex;
            float[][] lHisto = new float[5][];
            int numPixHisto = 0;


            int cntFound = 0;
            OpenCvSharp.Point[] posFoundS = new OpenCvSharp.Point[12];
            long[] convFoundS = new long[12];
            int[] shiftX = new int[3] { -1, 0, 1 };
            int[] shiftY = new int[4] { 1, 2, 3, 4 };

            int modelIndex = 0;
            int modelIndex_s = 0;   //  Model Start 번호
            int modelIndex_e = mFidMarkSide.Count + mFidMarkTop.Count;  //  Model End 번호
            int modelScaleFactor = 4;
            int mWidth = 0;
            int mHeight = 0;
            int mSourceImg_Height = mSourceImg[iBuf].Height;
            int mSourceImg_Width = mSourceImg[iBuf].Width;

            if (iIndex == 1)
                cntFound = 0;

            sFiducialMark lfMark = null;
            bool IsSide = true;
            double subconv = 0;

            mIsDebug = IsDebug;

            if (whichModel >= 0)
            {
                //  특정 모델이 지정된 경우
                //   특정 모델이 지정되지 않은 경우는 whichModel = -1 로써 깔끔하게 정리됨.
                modelIndex_s = whichModel;
                modelIndex_e = whichModel + 1;
                if (modelIndex_s > 7)
                {
                    modelIndex_s = modelIndex_s - 8 + mFidMarkSide.Count;
                    modelIndex_e = modelIndex_s + 1;
                }
            }

            int gotoLoopCount = 0;

            if (iIndex == 0)
            {
                //  0번 영상일때만 mPeakType 을 초기화한다. 따라서 0번 영상에 의해 모든 Thread 에서 처리할 mPeakType 이 정해진다.
                //  0번에서 마크 찾을 때 활용한 모델과 N 번에서 마크 찾을 때 활용한 모델이 다르더라도 동일한 방식으로 처리하게 된다.
                for (int i = 0; i < mPeakType.Length; i++)
                    mPeakType[i] = new sPeakType();

                //mFZM.ClearRatioXroughPeak();
                mFZM.SetRatio(mCalcEffBand);
            }
            //if (iIndex == 76)
            //    subconv = 0;

            callCount = 0;
            int doneLength = 37 * 37; //    편측 825um    //  29 * 29;  // 편측 731um   //  23*23; //  편측 589um
            long[] lDoneConv = new long[doneLength];
            int[] lDoneBuf = new int[doneLength];
            int[] lDoneIndex = new int[doneLength];
            int lmCalcEffBand = mCalcEffBand;

            long[] localConv = new long[12];
            long[] localConvCopy = new long[12];
            double[] localSubConv = new double[12];
            int lLastTrial = 0;
            int si = 0;

            ref long[] prevConv = ref mPrevConv[iBuf];

            for (modelIndex = modelIndex_s; modelIndex < modelIndex_e; modelIndex++)// foreach( sFiducialMark lfMark in mFidMarkSide)
            {
                bool FoundIt = false;
                int threshT = 0 ;
                int thresh0 = 0 ;
                int thresh1 = 0 ;
                int thresh2 = 0 ;
                int thresh3 = 0 ;
                ref System.Drawing.Point prevPos = ref mPrevPos[iBuf][si];

                if ( IsFirst )
                //if (false)
                {
                    while (!FoundIt)
                    {
                        if (mCandidateIndex == 0)
                        {
                            //  첫번째 시도
                            if (modelIndex < mFidMarkSide.Count)
                            {
                                //lfMark = mFidMarkSide[modelIndex];
                                //if (mFMSideCandidate.Count==3 && )
                                //    lfMark = mFMSideCandidate[2][modelIndex];
                                //else
                                    lfMark = mFidMarkSide[modelIndex];

                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFidMarkTop[modelIndex - mFidMarkSide.Count];
                                IsSide = false;
                            }
                        }
                        else
                        {
                            //  두번째 이후 시도
                            if (mFMSideCandidate.Count == 0 || mCandidateIndex > mFMSideCandidate.Count)
                                break;


                            if (modelIndex < mFMSideCandidate[mCandidateIndex - 1].Count)
                            {
                                //  Side Model 의 경우

                                if (mFMSideCandidate.Count == 3 && mCandidateIndex == 2) // FCF //  모델가지수가 3일때는 4가지가 시험되고 있는데 이 경우, 현재 모델이 CFC 이면 S 마크는 0번의 S 마크를 적용한다.
                                {
                                    //  FCF 0 - org - 0
                                    if (modelIndex!=2)
                                        lfMark = mFMSideCandidate[0][modelIndex]; //  0번 ( Std 모델 ) 의 0,1 을 사용한다.
                                    else
                                        lfMark = mFidMarkSide[2];   //  SMT Model 의 2를 사용
                                }
                                else if (mFMSideCandidate.Count == 3 && mCandidateIndex == 3) // CFC
                                {
                                    //  CFC org - 0 - org
                                    if (modelIndex == 2)
                                        lfMark = mFMSideCandidate[0][2]; //  0번 ( Std 모델 ) 의 2 를 사용한다.
                                    else
                                        lfMark = mFidMarkSide[modelIndex];  //  SMT Model 의 0, 1 를 사용
                                }else
                                    lfMark = mFMSideCandidate[mCandidateIndex - 1][modelIndex];

                                IsSide = true;
                            }
                            else
                            {
                                //  Top Model 의 경우
                                if (mFMSideCandidate.Count == 3 && mCandidateIndex == 2) // FCF
                                    //  0-0
                                    lfMark = mFMTopCandidate[0][modelIndex - mFMSideCandidate[0].Count];    //  Fiducial


                                else if (mFMSideCandidate.Count == 3 && mCandidateIndex == 3) // CFC
                                    //  org - org
                                    lfMark = mFidMarkTop[modelIndex - mFMSideCandidate[0].Count];   //  Component
                                else
                                    lfMark = mFMTopCandidate[mCandidateIndex - 1][modelIndex - mFMSideCandidate[0].Count];


                                IsSide = false;
                            }
                        }
                        if (lfMark == null)
                            break;

                        int i = 0;
                        int j = 0;
                        long lconv = 0;
                        long maxConv = 0;
                        bool bCand = false;
                        bool bFinishViewRegion = false;

                        threshT = lfMark.conv;
                        thresh0 = (int)(threshT * 0.4);
                        thresh1 = (int)(threshT * 0.6);
                        thresh2 = (int)(threshT * 0.9);
                        thresh3 = (int)(threshT * 0.7);

                        modelScaleFactor = lfMark.MScale;

                        bool bSaved = false;
                        //int prev_i = 0;

                        //int xSkipWidth = 55;
                        //xSkipWidth = lfMark.modelSize.Width;
                        List<System.Drawing.Point> pSkips = new List<System.Drawing.Point>();
                        //  모델 하나를 검색하면 다음모델로 넘어가야 한다.
                        //  모델크기가 0 이면 continue
                        //  결과를 모델 lfMark 에 저장할 수있는가?
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        /// Side View
                        j = (lfMark.searchRoi.Top / modelScaleFactor);

                        ////  빠르게 찾기 위해 검색영역을 축소
                        //if (modelIndex > modelIndex_s && modelIndex < modelIndex_s + 3)
                        //    j = posFoundS[0].Y + (mMarkPosOnPanel[cntFound].Y - mMarkPosOnPanel[0].Y) / modelScaleFactor - 4;
                        //else if (cntFound == 3)
                        //    j = posFoundS[0].Y + (mMarkPosOnPanel[cntFound].Y - mMarkPosOnPanel[0].Y) / modelScaleFactor - 13;  //  Z 변위가 최대로 내려갔을 때 -750um 까지 측정가능
                        //else if (cntFound == 4)
                        //    j = posFoundS[cntFound - 1].Y - 2;

                        int j_max = (lfMark.searchRoi.Bottom / modelScaleFactor);
                        int i_max = (lfMark.searchRoi.Right / modelScaleFactor);

                        mWidth  = (lfMark.modelSize.Width);
                        mHeight = (lfMark.modelSize.Height);

                        int i_start = (lfMark.searchRoi.Left / modelScaleFactor);
                        ////  빠르게 찾기 위해 검색영역을 축소
                        //if (cntFound == 1)
                        //{
                        //    i_start = posFoundS[0].X + (mMarkPosOnPanel[cntFound].X - mMarkPosOnPanel[0].X) / modelScaleFactor - 1;
                        //    i_max = i_start + 4;
                        //}
                        //else if (cntFound > 1)
                        //{
                        //    i_start = posFoundS[0].X + (mMarkPosOnPanel[cntFound].X - mMarkPosOnPanel[0].X) / modelScaleFactor - 4;
                        //    i_max = i_start + 8;
                        //}

                        while (j <= j_max)
                        {
                            //i = (lfMark.searchRoi.Left / modelScaleFactor);
                            i = i_start;

                            while (i < i_max)
                            {
                                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                //  Skip Logic 은 exArea 적용할 때는 사용 불가. exArea 적용할 때는 conv 값이 값자기 크게 변함.
                                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                if ( i==26 && j == 12)
                                    threshT = lfMark.conv;

                                lconv = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, i, j, iBuf, ref subconv, ref lfMark.exArea);
                                //prev_i = i;
                                i++;
                                //if (lconv<0)
                                //{
                                //    i++;
                                //    continue;
                                //}
                                if (j > 12 && i > 205)
                                    thresh1 = (int)(threshT * 0.6);


                                if (lconv > threshT)
                                    bCand = true;

                                if (lconv < thresh3 && bCand == true)
                                {
                                    //  이 로직은 Conv 값의 Local Max 를 검출성공으로 판단하는 로직
                                    bCand = false;
                                    if (bSaved)
                                    {
                                        bSaved = false;
                                        maxConv = 0;

                                        bFinishViewRegion = true;
                                        FoundIt = true;

                                        cntFound++;
                                        break;
                                    }
                                }
                                if (bCand)
                                {
                                    //////////////////////////////////////////////////////////////////////////////
                                    //////////////////////////////////////////////////////////////////////////////
                                    //bool IsCorrect = true;

                                    //if (lfMark.sub == 0)
                                    //    IsCorrect = true;

                                    //else if (lfMark.sub > 100)
                                    //{
                                    //    if (subconv < lfMark.subconv)
                                    //        IsCorrect = true;
                                    //}
                                    //else
                                    //{
                                    //    if (subconv > lfMark.subconv)
                                    //        IsCorrect = true;
                                    //}

                                    //////////////////////////////////////////////////////////////////////////////
                                    //////////////////////////////////////////////////////////////////////////////

                                    //if (maxConv < lconv && (IsCorrect == true))
                                    if (maxConv < lconv )
                                    {
                                        bSaved = true;
                                        maxConv = lconv;
                                        if (IsSide)
                                            smr[si].Azimuth = lfMark.Azimuth;
                                        else
                                            smr[si].Azimuth = lfMark.Azimuth + 8;
                                        smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                                        posFoundS[si].X = (int)i - 1;
                                        posFoundS[si].Y = (int)j;
                                        convFoundS[si] = lconv;
                                    }
                                }
                            }
                            j++;
                            if (bFinishViewRegion)
                                break;
                        }
                        //mCandidateIndex++;
                        break;
                    }
                }else
                {
                    //  현재는 1번째 이후 영상에 대해서도 일단 모든 모델로 다 찾기를 시도해서 걸리는 모델을 활용해서 좌표를 추출한다.
                    while (true)
                    {
                        if (mCandidateIndex == 0)
                        {
                            //  첫번째 시도
                            if (modelIndex < mFidMarkSide.Count)
                            {
                                //if (mFMSideCandidate.Count == 3)
                                //    lfMark = mFMSideCandidate[2][modelIndex];
                                //else
                                    lfMark = mFidMarkSide[modelIndex];

                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFidMarkTop[modelIndex - mFidMarkSide.Count];
                                IsSide = false;
                            }
                        }
                        else
                        {
                            //  두번째 이후 시도
                            if (mFMSideCandidate.Count == 0 || mCandidateIndex > mFMSideCandidate.Count)
                                break;

                            if (modelIndex < mFMSideCandidate[mCandidateIndex - 1].Count)
                            {
                                //  Side Model 의 경우

                                if (mFMSideCandidate.Count == 3 && mCandidateIndex == 2) // FCF //  모델가지수가 3일때는 4가지가 시험되고 있는데 이 경우, 현재 모델이 CFC 이면 S 마크는 0번의 S 마크를 적용한다.
                                {
                                    //  0: CCC  1: FFF  2: FCF  3: CFC,     CFC 일 때 S 마크모델로서 CCC 일때의 S 마크모델을 적용함.
                                    if (modelIndex != 2)
                                        lfMark = mFMSideCandidate[0][modelIndex]; //  0번 ( Std 모델 ) 의 0,1 을 사용한다.
                                    else
                                        lfMark = mFidMarkSide[2];   //  SMT Model 의 2를 사용
                                }
                                else if (mFMSideCandidate.Count == 3 && mCandidateIndex == 3) // CFC
                                {
                                    if (modelIndex == 2)
                                        lfMark = mFMSideCandidate[0][2]; //  0번 ( Std 모델 ) 의 2 를 사용한다.
                                    else
                                        lfMark = mFidMarkSide[modelIndex];  //  SMT Model 의 0, 1 를 사용
                                }
                                else
                                    lfMark = mFMSideCandidate[mCandidateIndex - 1][modelIndex];

                                IsSide = true;
                            }
                            else
                            {
                                //  Top Model 의 경우
                                if (mFMSideCandidate.Count == 3 && mCandidateIndex == 2) // FCF
                                    lfMark = mFMTopCandidate[0][modelIndex - mFMSideCandidate[0].Count];
                                else if (mFMSideCandidate.Count == 3 && mCandidateIndex == 3) // CFC
                                    lfMark = mFidMarkTop[modelIndex - mFMSideCandidate[0].Count];
                                else
                                    lfMark = mFMTopCandidate[mCandidateIndex - 1][modelIndex - mFMSideCandidate[0].Count];

                                IsSide = false;
                            }
                        }
                        if (lfMark == null)
                            break;

                        threshT = lfMark.conv;
                        if (prevConv[si] == 0)
                            prevConv[si] = lfMark.conv;

                        lDoneConv = new long[doneLength];
                        lDoneBuf = new int[doneLength];
                        lDoneIndex = new int[doneLength];
                        System.Drawing.Point[] lDonePos = new System.Drawing.Point[doneLength];
                        int absIndex = 0;

                        modelScaleFactor = lfMark.MScale;
                        mWidth = (lfMark.modelSize.Width);
                        mHeight = (lfMark.modelSize.Height);

                        //  First Trial
                        //if (iIndex == 28 && si == 0)
                        //    lLastTrial = 0;

                        lLastTrial = 0;
                        int peakIndex = -1;
                        long peakConv = -9999;
                        for (int j = -1; j < 2; j++)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if ( peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;
                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);

                        ///////////////////////////////////////////////////////////////////////////
                        //  Second Trial
                        for (int j = -2; j < 3; j++)
                        {
                            for (int i = -2 + j % 2; i < 3; i+=2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;

                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;

                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;

                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.96)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //   현재 작은값이 앞으로 가 있으므로 앞쪽은 0. 뒤쪽에서부터  의미있는 값이 들어있으므로 일단 들어있는
                        //   값들을 앞으로 다시 옮겨놓아야 마지막데이터 다음부터 다시 새로운 데이터 저장이 가능하다.
                        //   앞으로 옮겨 놓은 값들이 그 값 자체가 중요하지 않다면 값을 버리면 되고 중요하면 값의 순서를 유지해줘야 함.
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);

                        ///////////////////////////////////////////////////////////////////////////
                        //  Third Trial
                        for (int j = -3; j < 4; j++)
                        {
                            for (int i = -3 + j % 2; i < 4; i+=2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;

                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  4th Trial
                        for (int j = -5; j < 6; j++)
                        {
                            for (int i = -5 + j % 2; i < 6; i+=2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  5th Trial
                        lLastTrial = 5;
                        for (int j = -7; j < 8; j++)
                        {
                            for (int i = -7 + j % 2; i < 8; i+=2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  6th Trial
                        for (int j = -9; j < 10; j++)
                        {
                            for (int i = -9 + j%2; i < 10; i+=2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);                            ///////////////////////////////////////////////////////////////////////////
                        //  7th Trial
                        for (int j = -11; j < 12; j++)
                        {
                            for (int i = -11 + j % 2; i < 12; i+=2)
                            {
                                if (i + j < -21 || i + j > 21)
                                    continue;
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);                            ///////////////////////////////////////////////////////////////////////////
                        //  Last Trial
                        for (int j = -18; j < 19; j++)
                        {
                            for (int i = -18 + j % 2; i < 18; i+=2)
                            {
                                if (i + j < -24 || i + j > 24)
                                    continue;
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT*0.9)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //mCandidateIndex++;
                        break;
                    }
                }


                if (lfMark == null)
                {
                    si++;
                    continue;
                }

                if (!FoundIt )
                {
                    si++;
                    continue;
                }

                ////////////////////////////////
                ////////////////////////////////
                ////////////////////////////////
                /////   다음은 임시코드, 삭제 필요
                //long tmpConv = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, posFoundS[si].X, posFoundS[si].Y, iBuf, ref subconv, ref lfMark.exArea);
                ////////////////////////////////
                ////////////////////////////////
                ////////////////////////////////

                //if ((iIndex == 1816 && si == 4) || (iIndex == 1817 && si == 4))
                //    si = si;

                if (convFoundS[si] < threshT * 1.3)
                {
                    localConv = new long[12];
                    localConvCopy = new long[12];
                    localSubConv = new double[12];
                    long[] lconvIndex = new long[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                    if (posFoundS[si].Y > 0 && IsFirst)
                    {
                        localConv[0] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[0]);
                        localConv[1] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 1, iBuf, ref localSubConv[1]);
                        localConv[2] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[2]);

                        localConv[3] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[3]);
                        localConv[4] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 2, iBuf, ref localSubConv[4]);
                        localConv[5] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[5]);

                        localConv[6] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 3, iBuf, ref localSubConv[6]);
                        localConv[7] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 3, iBuf, ref localSubConv[7]);
                        localConv[8] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 3, iBuf, ref localSubConv[8]);
                        if (posFoundS[si].Y + 4 + lfMark.modelSize.Height < mSourceImg_Height / lfMark.MScale)
                        {
                            localConv[9] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 4, iBuf, ref localSubConv[9]);
                            localConv[10] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 4, iBuf, ref localSubConv[10]);
                            localConv[11] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 4, iBuf, ref localSubConv[11]);
                        }
                        //Array.Copy(localConv, localConvCopy, 12);
                        Array.Sort(localConv, lconvIndex);
                        //Array.Sort(localConvCopy, localSubConv);
                        if (localConv[11] > convFoundS[si])
                        {
                            convFoundS[si] = localConv[11];
                            posFoundS[si].X += shiftX[lconvIndex[11] % 3];
                            posFoundS[si].Y += shiftY[lconvIndex[11] / 3];
                        }
                    }
                    else if (posFoundS[si].Y > 0)
                    {
                        localConv[0] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y, iBuf, ref localSubConv[0]);

                        localConv[2] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y, iBuf, ref localSubConv[2]);

                        localConv[3] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[0]);
                        localConv[4] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 1, iBuf, ref localSubConv[1]);
                        localConv[5] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[2]);

                        localConv[6] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y - 1, iBuf, ref localSubConv[0]);
                        localConv[7] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y - 1, iBuf, ref localSubConv[1]);
                        localConv[8] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y - 1, iBuf, ref localSubConv[2]);

                        //localConv[9] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y - 2, iBuf, ref localSubConv[0]);
                        //localConv[10] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y - 2, iBuf, ref localSubConv[1]);
                        //localConv[11] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y - 2, iBuf, ref localSubConv[2]);

                        //Array.Copy(localConv, localConvCopy, 12);
                        Array.Sort(localConv, lconvIndex);
                        //Array.Sort(localConvCopy, localSubConv);
                        if (localConv[11] > convFoundS[si])
                        {
                            shiftX = new int[3] { -1, 0, 1 };
                            shiftY = new int[4] { 0, 1, -1, -2 };

                            convFoundS[si] = localConv[11];
                            posFoundS[si].X += shiftX[lconvIndex[11] % 3];
                            posFoundS[si].Y += shiftY[lconvIndex[11] / 3];
                        }
                    }
                }
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////
                ////////  SMT Component 일 때만 적용한다.
                //////long[] upperConv = new long[12];
                //////int[] upOrder = new int[12] { 0, 1, 2, 3, 4, 5, 6,7,8,9,10,11 };

                //////if (posFoundS[si].Y>11 && modelIndex != 2 )
                //////{
                //////    int upshift = 0;
                //////    upperConv[upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 16, iBuf, ref localSubConv[0 ]);
                //////    if (upperConv[upshift] < threshT)
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 15, iBuf, ref localSubConv[1 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 14, iBuf, ref localSubConv[2 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 13, iBuf, ref localSubConv[3 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 12, iBuf, ref localSubConv[4 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 11,  iBuf, ref localSubConv[5 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 10,  iBuf, ref localSubConv[6 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 9,  iBuf, ref localSubConv[7 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 8,  iBuf, ref localSubConv[8 ]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 7,  iBuf, ref localSubConv[9 ]);
                //////    if (upperConv[upshift] < threshT)                             
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 6,  iBuf, ref localSubConv[10]);
                //////    if (upperConv[upshift] < threshT)                               
                //////        upperConv[++upshift] = CalcConv(ref lfMark, posFoundS[si].X , posFoundS[si].Y - 5, iBuf, ref localSubConv[11]);

                //////    if (upperConv[upshift] >= threshT)
                //////    {
                //////        convFoundS[si] = upperConv[upshift];
                //////        posFoundS[si].Y = posFoundS[si].Y - 16 + upOrder[upshift];
                //////    }
                //////}
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                prevPos.X = posFoundS[si].X;
                prevPos.Y = posFoundS[si].Y;
                prevConv[si] = convFoundS[si];

                if (iIndex == 0)
                {
                    if (modelIndex == 1)
                        FoundIt = true;

                    //  모든 mPrevPos 에 값을 넣어준다.
                    for (int cntBuf = 0; cntBuf < mPrevPos.Length; cntBuf++)
                    {
                        mPrevPos [cntBuf][si].X = posFoundS  [si].X;
                        mPrevPos [cntBuf][si].Y = posFoundS  [si].Y;
                        mPrevConv[cntBuf][si]   = convFoundS [si];
                    }

                    ////  현재 검색된 영역을 기준으로 모델을 업데이트한다.
                    //Rect lsubRoi = new Rect(posFoundS[si].X, posFoundS[si].Y, mWidth, mHeight);
                    //byte[] lsrcData = new byte[lfMark.img.Length];
                    //int quaterWidth = mSourceImg[iBuf].Width / lfMark.MScale;// lfmark.MScale;

                    //for (int yy = 0; yy < mHeight; yy++)
                    //    for (int xx = 0; xx < mWidth; xx++)
                    //        lsrcData[xx + yy * mWidth] = q_Value[iBuf][xx + posFoundS[si].X + (yy + posFoundS[si].Y) * quaterWidth];

                    //CreateModelFromSubImg(lsrcData, mWidth, mHeight, lfMark.MScale, ref lfMark, false);

                    //prevConv[si] = lfMark.conv;

                    //if (mCandidateIndex > 0 && mFMSideCandidate.Count > mCandidateIndex - 1)
                    //{
                    //    //  여기는 추가 검증 필요
                    //    if (modelIndex < mFMSideCandidate[mCandidateIndex - 1].Count)
                    //        mFidMarkSide[modelIndex] = lfMark;
                    //    else
                    //        mFidMarkTop[modelIndex - mFidMarkSide.Count] = lfMark;
                    //}
                }
                smr[si].mMTF = convFoundS[si];
                //  각 마크의 COG 를 구한다.
                if (posFoundS[si].X == 0)
                {
                    si++;
                    continue;
                }

                //Rect rc = new Rect();

                int xhead = 1;
                int yhead = 1;

                int xtail = 5;
                int ytail = 5;
                if (lfMark.xPosType == 3 || lfMark.yPosType == 3)
                {
                    xhead = 4;
                    yhead = 4;
                    xtail = 6;
                    ytail = 6;
                }

                HeadExtension:

                int subLeft = posFoundS[si].X * modelScaleFactor - xhead;
                int subTop = posFoundS[si].Y * modelScaleFactor - yhead;

                if (subLeft < 0)
                    subLeft = 0;
                if (subTop < 0)
                    subTop = 0;


                TailExtension:

                gotoLoopCount++;

                bool EOY = false;
                Rect subRoi = new Rect(subLeft, subTop, (int)(mWidth * modelScaleFactor + xtail), (int)(mHeight * modelScaleFactor + ytail));
                if (subRoi.Bottom >= mSourceImg_Height)
                {
                    int deltaY = subRoi.Bottom - mSourceImg_Height;
                    if (deltaY > 1 && subRoi.Y > 0)
                    {
                        subRoi.Height -= (deltaY - 1);
                        subRoi.Y -= 1;
                        EOY = true;
                    }
                    else
                    {
                        subRoi.Height -= deltaY;
                    }
                }
                if ( subRoi.Right >= mSourceImg_Width)
                {
                    subRoi.Left--;
                    subRoi.Width = mSourceImg_Width - subRoi.Left;
                }
                Mat tgtImg = null;
                byte[] tgtBuf = null;
                try
                {
                    tgtImg = mSourceImg[iBuf].SubMat(subRoi);
                    tgtImg.GetArray(out tgtBuf);
                }
                catch(Exception e)
                {
                    MessageBox.Show("Goto Loop Count = " + gotoLoopCount.ToString() + "\r\nxHead = " + xhead.ToString() + "\r\nyHead = " + yhead.ToString() + "\r\nxTail = " + xtail.ToString() + "\r\nyTail = " + ytail.ToString());
                }

                //////if (si == 2)
                //////{
                //////    tgtImg.SaveImage("img" + iIndex.ToString() + "_" + si.ToString() + "_" + convFoundS[si].ToString() + ".bmp");
                //////    StreamWriter wr = new StreamWriter("rawbin" + iIndex.ToString() + "_" + si.ToString() + ".csv");
                //////    string rawStr = "";
                //////    for (int y = 0; y < subRoi.Height; y++)
                //////    {
                //////        rawStr = "";
                //////        for (int x = 0; x < subRoi.Width; x++)
                //////        {
                //////            rawStr += tgtBuf[x + y * subRoi.Width].ToString() + ",";
                //////        }
                //////        wr.WriteLine(rawStr);
                //////    }
                //////    wr.Close();
                //////}
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////   다음 지우지 말 것
                //if ( iIndex < 2)
                //{
                //    StreamWriter wr = new StreamWriter("RawBin" + iIndex.ToString() + "_" + si.ToString() + ".csv");
                //    string rawStr = "";
                //    for (int y = 0; y < subRoi.Height; y++)
                //    {
                //        rawStr = "";
                //        for (int x = 0; x < subRoi.Width; x++)
                //        {
                //            rawStr += tgtBuf[x + y * subRoi.Width].ToString() + ",";
                //        }
                //        wr.WriteLine(rawStr);
                //    }
                //    wr.Close();
                //}
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if ( iIndex == 0 && mbGetHistogram)
                {
                    if ( si < 5)
                    {
                        numPixHisto += subRoi.Width * subRoi.Height;
                        GetHistogramData(tgtImg, ref lHisto[si]);
                    }
                    //if ( si == 4)
                    //{
                    //    DateTime now = DateTime.Now;
                    //    string filename = "C:\\CSHTest\\Result\\RawData\\Image\\HistMark" + mCandidateIndex.ToString() + "_" + now.ToString("MMddhhmmss") + ".csv";

                    //    StreamWriter sr = new StreamWriter(filename);
                    //    for( int j=0; j<255; j++)
                    //    {
                    //        sr.WriteLine(lHisto[0][j].ToString("F0") + "," + lHisto[1][j].ToString("F0") + "," + lHisto[2][j].ToString("F0") + "," + lHisto[3][j].ToString("F0") + "," + lHisto[4][j].ToString("F0"));
                    //    }
                    //    sr.Close();
                    //}
                }

                int tHeight = subRoi.Height;
                int tWidth = subRoi.Width;

                int tWidth_1 = tWidth - 1;
                int tHeight_1 = tHeight - 1;
                int tWidth_2 = tWidth - 2;
                int tHeight_2 = tHeight - 2;
                int tWidthOver2 = tWidth / 2;
                int tHeightOver2 = tHeight / 2;

                int[] xDiffimg = new int[subRoi.Width * subRoi.Height];
                int[] yDiffimg = new int[tWidth * tHeight_1];
                int[] diffSign = new int[2] { 1, -1 };

                if (lfMark.xPosType != 3)
                {


                    if (lfMark.exArea.Width > 0)
                    {
                        Rect exRc = lfMark.exArea;
                        exRc.X *= 3;
                        exRc.Y *= 3;
                        exRc.Width *= 3 + 1;
                        exRc.Height *= 3 + 1;
                        int y_tWidth = 0;
                        int y_tWidth_1 = 0;
                        int tmpdiff = 0;
                        int tmp2diff = 0;

                        for (int y = 0; y < tHeight; y++)
                        {
                            y_tWidth = y * tWidth;
                            y_tWidth_1 = y * tWidth_1;

                            for (int x = 1; x < tWidth_1; x++)
                            {
                                int curSign = x < tWidthOver2 ? 1 : -1;
                                tmpdiff = 2 * curSign * ((int)tgtBuf[x + 1 + y_tWidth] - tgtBuf[x - 1 + y_tWidth]);

                                if (x > 1 && x < tWidth_2)
                                    tmp2diff = curSign * ((int)tgtBuf[x + 2 + y_tWidth] - tgtBuf[x - 2 + y_tWidth]);

                                if (exRc.Contains(x, y))
                                {
                                    if (Math.Abs(tmpdiff) > 14 || Math.Abs(tmp2diff) > 7)
                                        continue;
                                }

                                xDiffimg[x + y_tWidth_1] = tmpdiff;// - LUTBgNoise[subLeft + x + 1] + LUTBgNoise[subLeft + x - 1]);
                                if (x > 1 && x < tWidth_2)
                                    xDiffimg[x + y_tWidth_1] += tmp2diff;// - LUTBgNoise[subLeft + x + 2] + LUTBgNoise[subLeft + x - 2])/2;
                            }
                        }
                        int x_tHeight_1 = 0;
                        for (int x = 0; x < tWidth; x++)
                        {
                            x_tHeight_1 = x * tHeight_1;
                            for (int y = 1; y < tHeight_1; y++)
                            {
                                int curSign = y < tHeightOver2 ? 1 : -1;    //  경계를 무조건 밝게 표시하게 해준다.
                                tmpdiff = 2 * curSign * ((int)tgtBuf[x + (y + 1) * tWidth] - tgtBuf[x + (y - 1) * tWidth]);

                                if (y > 1 && y < tHeight_2)
                                    tmp2diff = curSign * ((int)tgtBuf[x + (y + 2) * tWidth] - tgtBuf[x + (y - 2) * tWidth]);

                                if (exRc.Contains(x, y))
                                {
                                    if (Math.Abs(tmpdiff) > 14 || Math.Abs(tmp2diff) > 7)
                                        continue;
                                }

                                yDiffimg[y + x_tHeight_1] = tmpdiff;// - LUTBgNoiseY[subTop + y + 1] + LUTBgNoiseY[subTop + y - 1];
                                if (y > 1 && y < tHeight_2)
                                    yDiffimg[y + x_tHeight_1] += tmp2diff;// - LUTBgNoiseY[subTop + y + 2] + LUTBgNoiseY[subTop + y - 2])/2;
                            }
                        }

                        //  Diff 영상 저장용. 지우지 말것.
                        //if (si == 0)
                        //{
                        //    byte[] tmpByte = new byte[yDiffimg.Length];
                        //    for (int pi = 0; pi < yDiffimg.Length; pi++)
                        //    {
                        //        tmpByte[pi] = (byte)((128 + yDiffimg[pi]));
                        //    }
                        //    Mat tmpImg = new Mat(tWidth, tHeight_1, MatType.CV_8U, tmpByte);
                        //    tmpImg.SaveImage("Ydiff.bmp");

                        //    tmpByte = new byte[xDiffimg.Length];
                        //    for (int pi = 0; pi < xDiffimg.Length; pi++)
                        //    {
                        //        tmpByte[pi] = (byte)((128 + xDiffimg[pi]));
                        //    }
                        //    tmpImg = new Mat(tHeight, tWidth_1, MatType.CV_8U, tmpByte);
                        //    tmpImg.SaveImage("Xdiff.bmp");
                        //}
                    }
                    else
                    {
                        int y_tWidth = 0;
                        int y_tWidth_1 = 0;

                        for (int y = 0; y < tHeight; y++)
                        {
                            y_tWidth = y * tWidth;

                            y_tWidth_1 = y * tWidth_1;
                            for (int x = 1; x < tWidth_1; x++)
                            {
                                int curSign = x < tWidthOver2 ? 1 : -1;

                                xDiffimg[x + y_tWidth_1] = 2 * curSign * ((int)tgtBuf[x + 1 + y_tWidth] - tgtBuf[x - 1 + y_tWidth]);// - LUTBgNoise[subLeft + x + 1] + LUTBgNoise[subLeft + x - 1]);

                                if (x > 1 && x < tWidth_2)
                                    xDiffimg[x + y_tWidth_1] += curSign * ((int)tgtBuf[x + 2 + y_tWidth] - tgtBuf[x - 2 + y_tWidth]);// - LUTBgNoise[subLeft + x + 2] + LUTBgNoise[subLeft + x - 2])/2;
                            }
                        }
                        int x_tHeight_1 = 0;
                        for (int x = 0; x < tWidth; x++)
                        {
                            x_tHeight_1 = x * tHeight_1;
                            for (int y = 1; y < tHeight_1; y++)
                            {
                                int curSign = y < tHeightOver2 ? 1 : -1;    //  경계를 무조건 밝게 표시하게 해준다.

                                yDiffimg[y + x_tHeight_1] = 2 * curSign * ((int)tgtBuf[x + (y + 1) * tWidth] - tgtBuf[x + (y - 1) * tWidth]);// - LUTBgNoiseY[subTop + y + 1] + LUTBgNoiseY[subTop + y - 1];

                                if (y > 1 && y < tHeight_2)
                                    yDiffimg[y + x_tHeight_1] += curSign * ((int)tgtBuf[x + (y + 2) * tWidth] - tgtBuf[x + (y - 2) * tWidth]);// - LUTBgNoiseY[subTop + y + 2] + LUTBgNoiseY[subTop + y - 2])/2;
                            }
                        }


                        //Diff 영상 저장용.지우지 말것.
                        //byte[] tmpByte = new byte[yDiffimg.Length];
                        //for (int pi = 0; pi < yDiffimg.Length; pi++)
                        //{
                        //    tmpByte[pi] = (byte)((128 + yDiffimg[pi]/2));
                        //}
                        //Mat tmpImg = new Mat(tWidth, tHeight_1, MatType.CV_8U, tmpByte);
                        //tmpImg.SaveImage("Ydiff"+si.ToString() + ".bmp");

                        //tmpByte = new byte[xDiffimg.Length];
                        //for (int pi = 0; pi < xDiffimg.Length; pi++)
                        //{
                        //    tmpByte[pi] = (byte)((128 + xDiffimg[pi]/2));
                        //}
                        //tmpImg = new Mat(tHeight, tWidth_1, MatType.CV_8U, tmpByte);
                        //tmpImg.SaveImage("Xdiff"+si.ToString() + ".bmp");
                    }
                }

                int debugValue = 0;
                double xL = 0;
                double xR = 0;
                double yT = 0;
                double yB = tHeight;
                double yT2 = 0;
                double yB2 = tHeight;
                byte lupdown = 0;
                byte resUpDown = 0;
                if (iIndex == 0)
                    mUpDown[smr[si].Azimuth] = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                //  smr[si].Azimuth = 0,4,6 => Side View
                //  si = 0,1,2 => side view
                double gapX = 0;
                double gapY = 0;

                int dgbIndex = 0;


                try
                {

                    if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                    {

                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        xL = mFZM.ConvergePeakX( 4 * si , ref xDiffimg, tWidth - 1, tHeight, 2, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 1;
                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        xR = mFZM.ConvergePeakX( 4 * si + 1 , ref xDiffimg, tWidth - 1, tHeight, xL + tWidth / 4, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 2;

                        gapX = (int)xL + ((int)((tWidth - 1) - xR)) / 100.0;
                        if ((int)((tWidth - 1) - xR) < 6)
                        {
                            xtail += 2;
                            goto TailExtension;
                        }
                        if((int)xL < 6 && xhead < 2)
                        {
                            xhead += 6 - (int)xL;
                            goto HeadExtension;
                        }
                        if (xhead == 0)
                            xhead++;

                        smr[si].pos.X = (xL + xR) / 2;
                    }
                    else if (lfMark.xPosType == 1)    //  Left Harf 
                    {
                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        xL = mFZM.ConvergePeakX(4 * si, ref xDiffimg, tWidth - 1, tHeight, 2, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 3;
                        xR = tWidth - modelScaleFactor;
                        smr[si].pos.X = xL;
                    }
                    else if (lfMark.xPosType == 2)     //  Right Half
                    {
                        xL = modelScaleFactor;
                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        xR = mFZM.ConvergePeakX(4 * si + 1, ref xDiffimg, tWidth - 1, tHeight, 0.5 * tWidth, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 4;
                        //SetCurUpDown(1, lupdown, ref resUpDown);
                        smr[si].pos.X = xR;
                    }
                    else if (lfMark.xPosType == 3)     //  Shape
                    {
                        //  tgtBuf 를 이용해서 중심좌표를 바로 구한다.
                        //  xL 은 Dark Area 중심, xR 은 Bright Area 중심
                        //  yT 은 Dark Area 중심, yB 은 Bright Area 중심
                        //  19 -> 21 for 530 um
                        double xia = tWidth / 2.0;
                        double yia = tHeight / 2.0;

                        //mFZM.CogOfShape(4 * si + 1, ref tgtBuf, tWidth, tHeight, ref xia, ref yia, 19, ref xL, ref xR, ref yT, ref yB, iIndex);
                        int xshift = 0;
                        int yshift = 0;

                        if (mBreakIndex == iIndex)
                            dgbIndex = 0;

                        if (tgtBuf != null)
                        {
                            mFZM.CogOfShape(si, ref tgtBuf, tWidth, tHeight, ref xia, ref yia, ref xshift, ref yshift, iIndex);
                        }
                        else
                            ;

                        dgbIndex = 4;
                        bool bRangeRetry = false;
                        //SetCurUpDown(1, lupdown, ref resUpDown);
                        if (xshift < 0 && xhead < 8)
                        {
                            xhead = xhead - xshift;
                            bRangeRetry = true;
                        }
                        else if (xshift > 0 && xhead > 0)
                        {
                            xhead = xhead - xshift;
                            bRangeRetry = true;
                        }
                        if (yshift < 0 && yhead < 8)
                        {
                            yhead = yhead - yshift;
                            bRangeRetry = true;
                        }
                        else if (yshift > 0 && yhead > 0)
                        {
                            yhead = yhead - yshift;
                            bRangeRetry = true;
                        }

                        if (bRangeRetry)
                        {
                            //mGotoLoopCount+= yshift;
                            //mAccuShiftX += xshift;
                            //mAccuShiftY += yshift;
                            goto HeadExtension;
                        }

                        smr[si].pos.X = xia;
                        smr[si].pos.Y = yia;
                        xL = xia;
                        xR = xia;
                        yT = yia;
                        yB = yia;
                    }
                    else      // None
                    {
                        xL = modelScaleFactor;
                        smr[si].pos.X = -1;
                        xR = tWidth - modelScaleFactor;
                    }

                    if (iIndex == 0)
                        mUpDown[smr[si].Azimuth] = 255;
                    else
                        lupdown = mUpDown[smr[si].Azimuth];

                    if (lfMark.yPosType == 0)    //  Top Harf & Bottom Half
                    {
                        lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                        yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xL-2, mCalcEffBand, (int)(xR - xL+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);
                        dgbIndex = 5;

                        lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                        yB = mFZM.ConvergePeakX(4 * si + 3, ref yDiffimg, tHeight - 1, tWidth, tHeight/2-1, xL-2, mCalcEffBand, (int)(xR - xL+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[3], iIndex);
                        dgbIndex = 6;

                        gapY = (int)yT + ((int)((tHeight - 1) - yB)) / 100.0;
                        if ((int)((tHeight - 1) - yB) < 6 && !EOY)
                        {
                            ytail += 2;
                            goto TailExtension;
                        }
                        if ((int)yT < 6 && yhead < 2 && !EOY)
                        {
                            yhead += 6 - (int)yT;
    
                            goto HeadExtension;
                        }
                        smr[si].pos.Y = (yT + yB) / 2;
                    }
                    else if (lfMark.yPosType == 1)    //  Top Harf 
                    {
                        lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);

                        if ( si == 2)
                        {
                            int xLmax = 3;
                            int xRmax = (int)(xR + (xL - 3));
                            yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xLmax, mCalcEffBand, xRmax - xLmax, ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);
                        }
                        else
                            yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xL, mCalcEffBand, (int)(xR - xL), ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);

                        dgbIndex = 7;
                        //SetCurUpDown(2, lupdown, ref resUpDown);
                        yB = subRoi.Height - modelScaleFactor;
                        smr[si].pos.Y = yT;
                    }
                    else if (lfMark.yPosType == 2)     //  Bottom Half
                    {
                        lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                        yB = mFZM.ConvergePeakX(4 * si + 3, ref yDiffimg, tHeight - 1, tWidth, 0.5 * tHeight, xL, mCalcEffBand, (int)(xR - xL), ref lupdown, ref mPeakType[smr[si].Azimuth].type[3], iIndex);
                        dgbIndex = 8;
                        //SetCurUpDown(3, lupdown, ref resUpDown);
                        yT = modelScaleFactor;
                        smr[si].pos.Y = yB;
                    }
                    else if (lfMark.yPosType == 3)     //  Shape
                    {
                        ////  tgtBuf 를 이용해서 중심좌표를 바로 구한다.
                        ////  xL 은 Dark Area 중심, xR 은 Bright Area 중심
                        ////  yT 은 Dark Area 중심, yB 은 Bright Area 중심
                        ////  18
                        //mFZM.CogOfShape(4 * si + 1, ref tgtBuf, tWidth, tHeight, (xL + xR)/2, (yT + yB)/2 , 21, ref xL, ref xR, ref yT, ref yB, iIndex);
                        //dgbIndex = 4;
                        ////SetCurUpDown(1, lupdown, ref resUpDown);
                        //smr[si].pos.Y = (yT + yB) / 2;
                    }
                    else      // None
                    {
                        yT = modelScaleFactor;
                        smr[si].pos.Y = -1;
                        yB = tHeight - modelScaleFactor;
                    }

                    if (iIndex == 0)
                        mUpDown[smr[si].Azimuth] = 255;
                    else
                        lupdown = mUpDown[smr[si].Azimuth];

                    if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                    {
                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        xL = mFZM.ConvergePeakX(4 * si, ref xDiffimg, tWidth - 1, tHeight, 2, yT-2, mCalcEffBand, (int)(yB - yT+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 9;
                        SetCurUpDown(0, lupdown, ref resUpDown);                                                                                 

                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        //xR = mFZM.ConvergePeakX(ref xDiffimg, tWidth - 1, tHeight, 0.5 * tWidth, yT+1, mCalcEffBand, (int)(yB - yT - 2), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1]);
                        xR = mFZM.ConvergePeakX(4 * si + 1 , ref xDiffimg, tWidth - 1, tHeight, xL + tWidth / 4, yT-2, mCalcEffBand, (int)(yB - yT+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 10;
                        if (si == 4 && (iIndex == 2946 || iIndex == 2947 || iIndex == 2948))
                            gapX += 0.0001;

                        SetCurUpDown(1, lupdown, ref mUpDown[smr[si].Azimuth]);                                                                  
                        smr[si].pos.X = (xL + xR) / 2;
                    }
                    else if (lfMark.xPosType == 1)    //  Left Harf                                                                                
                    {
                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        xL = mFZM.ConvergePeakX(4 * si, ref xDiffimg, tWidth - 1, tHeight, 2, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 11;
                        //SetCurUpDown(0, lupdown, ref resUpDown);                                                                                 
                        xR = tWidth - modelScaleFactor;
                        smr[si].pos.X = xL;
                    }
                    else if (lfMark.xPosType == 2)     //  Right Half                                                                              
                    {
                        xL = 0;
                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        xR = mFZM.ConvergePeakX(4 * si + 1, ref xDiffimg, tWidth - 1, tHeight, 0.5 * tWidth, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 12;
                        //SetCurUpDown(1, lupdown, ref resUpDown);
                        smr[si].pos.X = xR;
                    }
                    else if (lfMark.xPosType == 3)     //  Shape
                    {
                        ////  tgtBuf 를 이용해서 중심좌표를 바로 구한다.
                        ////  xL 은 Dark Area 중심, xR 은 Bright Area 중심
                        ////  yT 은 Dark Area 중심, yB 은 Bright Area 중심
                        ////  19 -> 21 for 530 um
                        //mFZM.CogOfShape(4 * si + 1, ref tgtBuf, tWidth, tHeight, (xL + xR) / 2, (yT + yB) / 2, 21, ref xL, ref xR, ref yT, ref yB, iIndex);
                        //dgbIndex = 4;
                        ////SetCurUpDown(1, lupdown, ref resUpDown);
                        //smr[si].pos.X = (xL + xR) / 2;
                    }
                    else      // None
                    {
                        xL = modelScaleFactor;
                        smr[si].pos.X = -1;
                        xR = tWidth - modelScaleFactor;
                    }

                    ////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////
                    if (iIndex == 0)
                        mUpDown[smr[si].Azimuth] = 255;
                    else
                        lupdown = mUpDown[smr[si].Azimuth];

                    if (si == 1)
                        lupdown = mUpDown[smr[si].Azimuth];

                    if (lfMark.yPosType == 0)    //  Top Harf & Bottom Half
                    {
                        lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                        yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xL-2, mCalcEffBand, (int)(xR - xL+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);
                        //yT2 = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, yT, xL - 1, mCalcEffBand, (int)(xR - xL + 2), ref lupdown, 2, tWidth/2, iIndex);
                        SetCurUpDown(2, lupdown, ref resUpDown);

                        lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                        yB = mFZM.ConvergePeakX(4 * si + 3, ref yDiffimg, tHeight - 1, tWidth, tHeight / 2 - 1, xL-2, mCalcEffBand, (int)(xR - xL+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[3], iIndex);
                        //yB2 = mFZM.ConvergePeakX(4 * si + 3, ref yDiffimg, tHeight - 1, tWidth, tHeight / 2 - 1, xL - 1, mCalcEffBand, (int)(xR - xL + 2), ref lupdown, 0, (int)(tWidth - (tWidth-yB)), iIndex );
                        SetCurUpDown(3, lupdown, ref resUpDown);

                        smr[si].pos.Y = (yT + yB) / 2;
                        smr_T[si].pos.Y = yT + subTop;// + compT;
                        smr_B[si].pos.Y = yB + subTop;// + compB;
                    }
                    else if (lfMark.yPosType == 1)    //  Top Harf 
                    {
                        lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                        if (si == 2)
                        {
                            int xLmax = 3;
                            int xRmax = (int)(xR + (xL - 3));
                            yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xLmax, mCalcEffBand, xRmax - xLmax, ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);
                        }
                        else
                            yT = mFZM.ConvergePeakX(4 * si + 2, ref yDiffimg, tHeight - 1, tWidth, 2, xL, mCalcEffBand, (int)(xR - xL), ref lupdown, ref mPeakType[smr[si].Azimuth].type[2], iIndex);
                        dgbIndex = 15;
                        SetCurUpDown(2, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si + 2);
                        yB = tHeight - modelScaleFactor;
                        smr[si].pos.Y = yT;
                    }
                    else if (lfMark.yPosType == 2)     //  Bottom Half
                    {
                        lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                        yB = mFZM.ConvergePeakX(4 * si + 3, ref yDiffimg, tHeight - 1, tWidth, 0.5 * tHeight, xL, mCalcEffBand, (int)(xR - xL), ref lupdown, ref mPeakType[smr[si].Azimuth].type[3], iIndex);
                        dgbIndex = 16;
                        SetCurUpDown(3, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si + 3);
                        yT = modelScaleFactor;
                        smr[si].pos.Y = yB;
                    }
                    else if (lfMark.yPosType == 3)     //  Shape
                    {
                        ////  tgtBuf 를 이용해서 중심좌표를 바로 구한다.
                        ////  xL 은 Dark Area 중심, xR 은 Bright Area 중심
                        ////  yT 은 Dark Area 중심, yB 은 Bright Area 중심
                        ////  18
                        //mFZM.CogOfShape(4 * si + 1, ref tgtBuf, tWidth, tHeight, (xL + xR) / 2, (yT + yB) / 2, 21, ref xL, ref xR, ref yT, ref yB, iIndex);
                        //dgbIndex = 4;
                        ////SetCurUpDown(1, lupdown, ref resUpDown);
                        //smr[si].pos.Y = (yT + yB) / 2;
                    }
                    else      // None
                    {
                        yT = modelScaleFactor;
                        smr[si].pos.Y = -1;
                        yB = tHeight - modelScaleFactor;
                    }
                    if (iIndex == 0)
                        lupdown = 255;
                    else
                        lupdown = mUpDown[smr[si].Azimuth];

                    if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                    {

                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        //                                  영상 폭, 영상 높이, 시작 X 좌표, 시작 Y 좌표, band, 처리할 높이, 방향성, 처리 Type )
                        xL = mFZM.ConvergePeakX(4 * si , ref xDiffimg, tWidth - 1, tHeight, 2, yT-2, mCalcEffBand, (int)(yB - yT+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 17;
                        //double xL1 = mFZM.ConvergePeakX(ref xDiffimg, tWidth - 1, tHeight, 0, yT + 1, mCalcEffBand, (int)(yB - yT - 1)/2-2, ref lupdown, ref mPeakType[smr[si].Azimuth].type[0]);
                        //double xL2 = mFZM.ConvergePeakX(ref xDiffimg, tWidth - 1, tHeight, 0, yT + 2, mCalcEffBand, (int)(yB - yT - 1)/2-4, ref lupdown, ref mPeakType[smr[si].Azimuth].type[0]);
                        //xL = (xL1 + xL2) / 2;
                        SetCurUpDown(0, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si);

                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        xR = mFZM.ConvergePeakX(4 * si + 1, ref xDiffimg, tWidth - 1, tHeight, xL + tWidth / 4, yT-2, mCalcEffBand, (int)(yB - yT+4), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 18;
                        //double xR1 = mFZM.ConvergePeakX(ref xDiffimg, tWidth - 1, tHeight, 0.55 * tWidth, yT + 1, mCalcEffBand, (int)(yB - yT - 1)-2, ref lupdown, ref mPeakType[smr[si].Azimuth].type[1]);
                        //double xR2 = mFZM.ConvergePeakX(ref xDiffimg, tWidth - 1, tHeight, 0.55 * tWidth, yT + 2, mCalcEffBand, (int)(yB - yT - 1)-4, ref lupdown, ref mPeakType[smr[si].Azimuth].type[1]);
                        //xR = (xR1 + xR2) / 2;
                        SetCurUpDown(1, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si + 1);

                    }
                    else if (lfMark.xPosType == 1)    //  Left Harf 
                    {
                        if (smr[si].Azimuth == 10)
                            debugValue = 1;
                        lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                        xL = mFZM.ConvergePeakX(4 * si , ref xDiffimg, tWidth - 1, tHeight, 2, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[0], iIndex);
                        dgbIndex = 19;
                        SetCurUpDown(0, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si );
                        xR = tWidth - 1;
                        smr[si].pos.X = xL;
                    }
                    else if (lfMark.xPosType == 2)     //  Right Half
                    {
                        xL = 0;
                        lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                        xR = mFZM.ConvergePeakX(4 * si + 1, ref xDiffimg, tWidth - 1, tHeight, 0.5 * tWidth, yT, mCalcEffBand, (int)(yB - yT), ref lupdown, ref mPeakType[smr[si].Azimuth].type[1], iIndex);
                        dgbIndex = 20;
                        SetCurUpDown(1, lupdown, ref resUpDown);
                        //if (iIndex == 0)
                            //mFZM.SetRatioXroughPeak(4 * si + 1);
                        smr[si].pos.X = xR;
                    }
                    else if (lfMark.xPosType == 3)     //  Shape
                    {
                        ////  tgtBuf 를 이용해서 중심좌표를 바로 구한다.
                        ////  xL 은 Dark Area 중심, xR 은 Bright Area 중심
                        ////  yT 은 Dark Area 중심, yB 은 Bright Area 중심
                        ////  18
                        //mFZM.CogOfShape(4 * si + 1, ref tgtBuf, tWidth, tHeight, (xL + xR) / 2, (yT + yB) / 2, 21, ref xL, ref xR, ref yT, ref yB, iIndex);
                        //dgbIndex = 4;
                        ////SetCurUpDown(1, lupdown, ref resUpDown);
                        //smr[si].pos.X = (xL + xR) / 2;
                    }
                    else      // None
                    {
                        xL = 0;
                        smr[si].pos.X = -1;
                        xR = tWidth - 1;
                    }
                    smr[si].pos.X = (xL + xR) / 2;
                    smr_T[si].pos.X = (xL + xR) / 2 + subLeft;
                    smr_B[si].pos.X = (xL + xR) / 2 + subLeft;

                    mUpDown[smr[si].Azimuth] = resUpDown;
                    if (smr[si].pos.X > 0 && smr[si].pos.Y > 0)
                    {
                        smr[si].pos.X += subLeft;    //  중심좌표의 절대좌표 X
                        smr[si].pos.Y += subTop;     //  중심좌표의 절대좌표 Y
                        //if (iIndex == 0)
                        //{
                        //    if (modelIndex == 1)
                        //        FoundIt = true;

                        //    //  모든 mPrevPos 에 값을 넣어준다.
                        //    for (int cntBuf = 0; cntBuf < mPrevPos.Length; cntBuf++)
                        //    {
                        //        mPrevPos[cntBuf][si].X = (int)(smr[si].pos.X / modelScaleFactor - mWidth / 2);
                        //        mPrevPos[cntBuf][si].Y = (int)(smr[si].pos.Y / modelScaleFactor - mHeight / 2);
                        //    }
                        //}
                        //else
                        //{
                        //    prevPos.X = (int)(smr[si].pos.X / modelScaleFactor - mWidth / 2);
                        //    prevPos.Y = (int)(smr[si].pos.Y / modelScaleFactor - mHeight / 2);
                        //}
                    }
                }
                catch(Exception e)
                {
                    //  mPeakType[smr[si].Azimuth].type[2]
                    MessageBox.Show("FineCOG(" + iIndex.ToString() + ") dgbIndex = " + dgbIndex.ToString() + "\r\n" + smr.Length.ToString() + "\r\nsmr[si].Azimuth = " + smr[si].Azimuth.ToString() + "\r\n" + "\r\nmFZM = " + mFZM.ToString()
                        + "\r\nmPeakType[smr[si].Azimuth].type[3] = " + mPeakType[smr[si].Azimuth].type[3].ToString()
                        + "\r\n\r\n\r\n" + e.ToString());
                }
                ////////////////////////////////////////////////////////////////////////////
                /////   임시코드 -> 모델 설정이 잘 됬는지 확인용 코드
                ////////////////////////////////////////////////////////////////////////////
                if (mCheckCompatibility)
                {
                    smr[si].pos.X = gapX;    //  X 방향으로 Gap 이 충분한지 확인 용
                    smr[si].pos.Y = gapY;    //  Y 방향으로 Gap 이 충분한지 확인 용
                }
                ////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////
                
                si++;
            }

            //if ( iIndex == 0 && mbGetHistogram)
            //{
            //    if (Directory.Exists("D:\\CSH035\\NoiseTest\\"))
            //    {
            //        float[] sumHisto = new float[256];
            //        for (int i = 0; i < 3; i++)
            //        {
            //            for (int h = 0; h < 256; h++)
            //                sumHisto[h] += lHisto[i][h];
            //        }
            //        try
            //        {
            //            StreamWriter lsw = new StreamWriter("D:\\CSH035\\NoiseTest\\Histo_" + numPixHisto.ToString() + ".csv");
            //            for (int h = 0; h < 256; h++)
            //                lsw.WriteLine(sumHisto[h].ToString());
            //            lsw.Close();

            //            mSourceImg[iBuf].SaveImage("D:\\CSH035\\NoiseTest\\HistoImg.bmp");
            //        }
            //        catch(Exception e)
            //        {
            //            ;
            //        }
            //    }else
            //    {
            //        float[] sumHisto = new float[256];
            //        for (int i = 0; i < 4; i++)
            //        {
            //            for (int h = 0; h < 256; h++)
            //                sumHisto[h] += lHisto[i][h];
            //        }
            //        try
            //        {
            //            StreamWriter lsw = new StreamWriter("Histo_" + numPixHisto.ToString() + ".csv");
            //            for (int h = 0; h < 256; h++)
            //                lsw.WriteLine(sumHisto[h].ToString() + "," + lHisto[0][h] + "," + lHisto[1][h] + "," + lHisto[2][h] + "," + lHisto[3][h] + "," + lHisto[4][h]);
            //            lsw.Close();

            //            mSourceImg[iBuf].SaveImage("HistoImg.bmp");
            //        }
            //        catch (Exception e)
            //        {
            //            ;
            //        }
            //    }
            //}
            
            //  앞에서 이미 처리해주고 있음.
            //if ( IsFirst )
            //{
            //    for ( int bi = 0; bi< mPrevPos.Length; bi++)
            //        for ( int si=0; si<cntFound; si++)
            //        {
            //            mPrevPos[bi][si].X = posFoundS[si].X;
            //            mPrevPos[bi][si].Y = posFoundS[si].Y;
            //            mPrevConv[bi][si] = convFoundS[si];
            //        }
            //}

            Nfound = cntFound;
            return posFoundS;
        }

        public int mGotoLoopCount = 0;
        public int mAccuShiftX = 0;
        public int mAccuShiftY = 0;
        public OpenCvSharp.Point[] FineMTF(bool IsFirst, int iIndex, ref sMarkResult[] smr, ref long Nfound, bool IsDebug = false, int iBuf = 0, int whichModel = -1)
        {
            if (IsFirst)
            {
                for (int cntBuf = 0; cntBuf < mPrevPos.Length; cntBuf++)
                {
                    mPrevPos[cntBuf] = new System.Drawing.Point[12];
                    mPrevConv[cntBuf] = new long[12];
                    for (int iB = 0; iB < 12; iB++)
                        mPrevPos[cntBuf][iB] = new System.Drawing.Point(-1, -1);
                }
            }

            int cntFound = 0;
            OpenCvSharp.Point[] posFoundS = new OpenCvSharp.Point[12];
            long[] convFoundS = new long[12];
            int[] shiftX = new int[3] { -1, 0, 1 };
            int[] shiftY = new int[4] { 1, 2, 3, 4 };

            int modelIndex = 0;
            int modelIndex_s = 0;   //  Model Start 번호
            int modelIndex_e = mFidMarkSide.Count + mFidMarkTop.Count;  //  Model End 번호
            int modelScaleFactor = 4;
            int mSourceImg_Height = mSourceImg[iBuf].Height;
            int mSourceImg_Width = mSourceImg[iBuf].Width;

            int mWidth = 0;
            int mHeight = 0;


            int doneLength = 29 * 29;   //  23*23
            long[] lDoneConv = new long[doneLength];
            int[] lDoneBuf = new int[doneLength];
            int[] lDoneIndex = new int[doneLength];

            sFiducialMark lfMark = null;
            bool IsSide = true;
            double subconv = 0;

            mIsDebug = IsDebug;

            if (whichModel >= 0)
            {
                //  특정 모델이 지정된 경우
                //   특정 모델이 지정되지 않은 경우는 whichModel = -1 로써 깔끔하게 정리됨.
                modelIndex_s = whichModel;
                modelIndex_e = whichModel + 1;
                if (modelIndex_s > 7)
                {
                    modelIndex_s = modelIndex_s - 8 + mFidMarkSide.Count;
                    modelIndex_e = modelIndex_s + 1;
                }
            }

            if (iIndex == 0)
            {
                //  0번 영상일때만 mPeakType 을 초기화한다. 따라서 0번 영상에 의해 모든 Thread 에서 처리할 mPeakType 이 정해진다.
                //  0번에서 마크 찾을 때 활용한 모델과 N 번에서 마크 찾을 때 활용한 모델이 다르더라도 동일한 방식으로 처리하게 된다.
                for (int i = 0; i < mPeakType.Length; i++)
                    mPeakType[i] = new sPeakType();
            }

            callCount = 0;
            ref long[] prevConv = ref mPrevConv[iBuf];
            int lLastTrial = 0;
            int si = 0;

            for (modelIndex = modelIndex_s; modelIndex < modelIndex_e; modelIndex++)// foreach( sFiducialMark lfMark in mFidMarkSide)
            {
                bool FoundIt = false;
                int threshT = 0;
                ref System.Drawing.Point prevPos = ref mPrevPos[iBuf][si];
                if (IsFirst)
                {
                    while (!FoundIt)
                    {
                        if (mCandidateIndex == 0)
                        {
                            //  첫번째 시도
                            if (modelIndex < mFidMarkSide.Count)
                            {
                                lfMark = mFidMarkSide[modelIndex];
                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFidMarkTop[modelIndex - mFidMarkSide.Count];
                                IsSide = false;
                            }
                        }
                        else
                        {
                            //  두번째 이후 시도
                            if (mFMSideCandidate.Count == 0 || mCandidateIndex > mFMSideCandidate.Count)
                                break;

                            if (modelIndex < mFMSideCandidate[mCandidateIndex - 1].Count)
                            {
                                lfMark = mFMSideCandidate[mCandidateIndex - 1][modelIndex];
                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFMTopCandidate[mCandidateIndex - 1][modelIndex - mFMSideCandidate[mCandidateIndex - 1].Count];
                                IsSide = false;
                            }
                        }
                        if (lfMark == null)
                            break;

                        modelScaleFactor = lfMark.MScale;

                        int i = 0;
                        int j = 0;
                        long lconv = 0;
                        long maxConv = 0;
                        bool bCand = false;
                        bool bFinishViewRegion = false;

                        threshT = lfMark.conv;
                        int thresh0 = (int)(threshT * 0.4);
                        int thresh1 = (int)(threshT * 0.6);
                        int thresh2 = (int)(threshT * 0.9);
                        int thresh3 = (int)(threshT * 0.7);

                        modelScaleFactor = lfMark.MScale;

                        bool bSaved = false;
                        //int prev_i = 0;

                        //int xSkipWidth = 55;
                        //xSkipWidth = lfMark.modelSize.Width;
                        List<System.Drawing.Point> pSkips = new List<System.Drawing.Point>();
                        //  모델 하나를 검색하면 다음모델로 넘어가야 한다.
                        //  모델크기가 0 이면 continue
                        //  결과를 모델 lfMark 에 저장할 수있는가?
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        /// Side View
                        j = lfMark.searchRoi.Top / modelScaleFactor;
                        int j_max = lfMark.searchRoi.Bottom / modelScaleFactor;
                        int i_max = lfMark.searchRoi.Right / modelScaleFactor;
                        while (j <= j_max)
                        {
                            i = lfMark.searchRoi.Left / modelScaleFactor;

                            while (i < i_max)
                            {
                                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                //  Skip Logic 은 exArea 적용할 때는 사용 불가. exArea 적용할 때는 conv 값이 값자기 크게 변함.
                                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

                                lconv = CalcConv(lfMark.modelSize.Width, lfMark.modelSize.Height, lfMark.MScale, ref lfMark.img, i, j, iBuf, ref subconv, ref lfMark.exArea);
                                //prev_i = i;
                                i++;

                                if (lconv > threshT)
                                    bCand = true;

                                if (lconv < thresh3 && bCand == true)
                                {
                                    //  이 로직은 Conv 값의 Local Max 를 검출성공으로 판단하는 로직
                                    bCand = false;
                                    if (bSaved)
                                    {
                                        bSaved = false;
                                        maxConv = 0;

                                        bFinishViewRegion = true;
                                        FoundIt = true;

                                        cntFound++;
                                        break;
                                    }
                                }
                                if (bCand)
                                {
                                    //////////////////////////////////////////////////////////////////////////////
                                    //////////////////////////////////////////////////////////////////////////////
                                    //bool IsCorrect = false;
                                    //if (lfMark.sub == 0)
                                    //    IsCorrect = true;

                                    //else if (lfMark.sub > 100)
                                    //{
                                    //    if (subconv < lfMark.subconv)
                                    //        IsCorrect = true;
                                    //}
                                    //else
                                    //{
                                    //    if (subconv > lfMark.subconv)
                                    //        IsCorrect = true;
                                    //}
                                    //////////////////////////////////////////////////////////////////////////////
                                    //////////////////////////////////////////////////////////////////////////////

                                    //if (maxConv < lconv && (IsCorrect == true))
                                    if (maxConv < lconv )
                                    {
                                        bSaved = true;
                                        maxConv = lconv;
                                        if (IsSide)
                                            smr[si].Azimuth = lfMark.Azimuth;
                                        else
                                            smr[si].Azimuth = lfMark.Azimuth + 8;
                                        smr[si].mSize = new System.Drawing.Size(lfMark.modelSize.Width, lfMark.modelSize.Height);
                                        posFoundS[si].X = i - 1;
                                        posFoundS[si].Y = j;
                                        convFoundS[si] = lconv;
                                    }
                                }
                            }
                            j++;
                            if (bFinishViewRegion)
                                break;
                        }
                        //mCandidateIndex++;
                        break;
                    }
                }
                else
                {
                    //  현재는 1번째 이후 영상에 대해서도 일단 모든 모델로 다 찾기를 시도해서 걸리는 모델을 활용해서 좌표를 추출한다.
                    while (true)
                    {
                        if (mCandidateIndex == 0)
                        {
                            //  첫번째 시도
                            if (modelIndex < mFidMarkSide.Count)
                            {
                                lfMark = mFidMarkSide[modelIndex];
                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFidMarkTop[modelIndex - mFidMarkSide.Count];
                                IsSide = false;
                            }
                        }
                        else
                        {
                            //  두번째 이후 시도
                            if (mFMSideCandidate.Count == 0 || mCandidateIndex > mFMSideCandidate.Count)
                                break;

                            if (modelIndex < mFMSideCandidate[mCandidateIndex - 1].Count)
                            {
                                lfMark = mFMSideCandidate[mCandidateIndex - 1][modelIndex];
                                IsSide = true;
                            }
                            else
                            {
                                lfMark = mFMTopCandidate[mCandidateIndex - 1][modelIndex - mFMSideCandidate[mCandidateIndex - 1].Count];
                                IsSide = false;
                            }
                        }
                        if (lfMark == null)
                            break;

                        threshT = lfMark.conv;
                        lDoneConv = new long[doneLength];
                        lDoneBuf = new int[doneLength];
                        lDoneIndex = new int[doneLength];
                        System.Drawing.Point[] lDonePos = new System.Drawing.Point[doneLength];
                        int absIndex = 0;

                        modelScaleFactor = lfMark.MScale;
                        mWidth = (lfMark.modelSize.Width);
                        mHeight = (lfMark.modelSize.Height);

                        //  First Trial
                        //if (iIndex == 28 && si == 0)
                        //    lLastTrial = 0;

                        lLastTrial = 0;
                        int peakIndex = -1;
                        long peakConv = -9999;
                        for (int j = -1; j < 2; j++)
                        {
                            for (int i = -1; i < 2; i++)
                            {
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;
                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);

                        ///////////////////////////////////////////////////////////////////////////
                        //  Second Trial
                        for (int j = -2; j < 3; j++)
                        {
                            for (int i = -2; i < 3; i += 2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;

                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;

                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;

                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.96)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //   현재 작은값이 앞으로 가 있으므로 앞쪽은 0. 뒤쪽에서부터  의미있는 값이 들어있으므로 일단 들어있는
                        //   값들을 앞으로 다시 옮겨놓아야 마지막데이터 다음부터 다시 새로운 데이터 저장이 가능하다.
                        //   앞으로 옮겨 놓은 값들이 그 값 자체가 중요하지 않다면 값을 버리면 되고 중요하면 값의 순서를 유지해줘야 함.
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);

                        ///////////////////////////////////////////////////////////////////////////
                        //  Third Trial
                        for (int j = -3; j < 4; j++)
                        {
                            for (int i = -3; i < 4; i += 2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;

                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  4th Trial
                        for (int j = -5; j < 6; j++)
                        {
                            for (int i = -5; i < 6; i += 2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  5th Trial
                        lLastTrial = 5;
                        for (int j = -7; j < 8; j++)
                        {
                            for (int i = -7; i < 8; i += 2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);
                        ///////////////////////////////////////////////////////////////////////////
                        //  6th Trial
                        for (int j = -9; j < 10; j++)
                        {
                            for (int i = -9; i < 10; i += 2)
                            {
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);                            ///////////////////////////////////////////////////////////////////////////
                        //  7th Trial
                        for (int j = -11; j < 12; j++)
                        {
                            for (int i = -11; i < 12; i += 2)
                            {
                                if (i + j < -19 || i + j > 19)
                                    continue;
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //Array.Reverse(lDoneConv);
                        //Array.Reverse(lDoneIndex);                            ///////////////////////////////////////////////////////////////////////////
                        //  Last Trial
                        for (int j = -14; j < 15; j++)
                        {
                            for (int i = -14; i < 15; i += 2)
                            {
                                if (i + j < -20 || i + j > 20)
                                    continue;
                                if (lDoneBuf[18 + i + (18 + j) * 37] > 0)
                                    continue;
                                lDoneConv[absIndex] = CalcConv(mWidth, mHeight, modelScaleFactor, ref lfMark.img, (i + prevPos.X), (j + prevPos.Y), iBuf, ref subconv, ref lfMark.exArea);
                                lDoneBuf[18 + i + (18 + j) * 37] = 1;
                                lDonePos[absIndex].X = i + prevPos.X;
                                lDonePos[absIndex].Y = j + prevPos.Y;
                                lDoneIndex[absIndex] = absIndex;
                                //absIndex++;
                                //if (prevConv[si] < lDoneConv[absIndex - 1])
                                //    break;

                                if (peakConv < lDoneConv[absIndex])
                                {
                                    peakConv = lDoneConv[absIndex];
                                    peakIndex = absIndex;
                                }
                                absIndex++;
                                if (prevConv[si] < peakConv)
                                    break;
                            }
                            //if (prevConv[si] < lDoneConv[absIndex - 1])
                            //    break;
                            if (prevConv[si] < peakConv)
                                break;
                        }
                        //Array.Sort(lDoneConv, lDoneIndex);
                        //if (lDoneConv[doneLength - 1] > threshT)//> prevConv[cntFound] * 0.95)
                        if (peakConv > threshT * 0.9)
                        {
                            if (IsSide)
                                smr[si].Azimuth = lfMark.Azimuth;
                            else
                                smr[si].Azimuth = lfMark.Azimuth + 8;

                            smr[si].mSize = new System.Drawing.Size((int)mWidth, (int)mHeight);
                            //posFoundS[si].X = lDonePos[lDoneIndex[doneLength - 1]].X;
                            //posFoundS[si].Y = lDonePos[lDoneIndex[doneLength - 1]].Y;
                            //convFoundS[si] = lDoneConv[doneLength - 1];
                            posFoundS[si].X = lDonePos[peakIndex].X;
                            posFoundS[si].Y = lDonePos[peakIndex].Y;
                            convFoundS[si] = peakConv;

                            FoundIt = true;
                            cntFound++;
                            break;
                        }
                        //mCandidateIndex++;
                        break;
                    }
                }


                if (lfMark == null)
                {
                    si++;
                    continue;
                }

                if (cntFound == 0)
                {
                    si++;
                    continue;
                }

                //si = cntFound - 1;
                long[] localConv = new long[12];
                long[] localConvCopy = new long[12];
                double[] localSubConv = new double[12];
                long[] lconvIndex = new long[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                if (posFoundS[si].Y > 0 && IsFirst)
                {
                    localConv[0] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[0]);
                    localConv[1] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 1, iBuf, ref localSubConv[1]);
                    localConv[2] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[2]);

                    localConv[3] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[3]);
                    localConv[4] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 2, iBuf, ref localSubConv[4]);
                    localConv[5] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[5]);

                    localConv[6] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 3, iBuf, ref localSubConv[6]);
                    localConv[7] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 3, iBuf, ref localSubConv[7]);
                    localConv[8] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 3, iBuf, ref localSubConv[8]);
                    if (posFoundS[si].Y + 4 + lfMark.modelSize.Height < mSourceImg_Height / lfMark.MScale)
                    {
                        localConv[9] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 4, iBuf, ref localSubConv[9]);
                        localConv[10] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 4, iBuf, ref localSubConv[10]);
                        localConv[11] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 4, iBuf, ref localSubConv[11]);
                    }
                    Array.Copy(localConv, localConvCopy, 12);
                    Array.Sort(localConv, lconvIndex);
                    Array.Sort(localConvCopy, localSubConv);
                    if (localConv[11] > convFoundS[si])
                    {
                        convFoundS[si] = localConv[11];
                        posFoundS[si].X += shiftX[lconvIndex[11] % 3];
                        posFoundS[si].Y += shiftY[lconvIndex[11] / 3];
                    }
                }
                else if (posFoundS[si].Y > 0)
                {
                    localConv[0] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y, iBuf, ref localSubConv[0]);

                    localConv[2] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y, iBuf, ref localSubConv[2]);

                    localConv[3] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[0]);
                    localConv[4] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 1, iBuf, ref localSubConv[1]);
                    localConv[5] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[2]);
                    Array.Copy(localConv, localConvCopy, 12);
                    Array.Sort(localConv, lconvIndex);
                    Array.Sort(localConvCopy, localSubConv);
                    if (localConv[11] > convFoundS[si])
                    {
                        convFoundS[si] = localConv[11];
                        posFoundS[si].X += shiftX[lconvIndex[11] % 3];
                        posFoundS[si].Y += shiftY[lconvIndex[11] / 3] - 1;
                    }
                }
                prevPos.X = posFoundS[si].X;
                prevPos.Y = posFoundS[si].Y;
                prevConv[si] = convFoundS[si];

                if (iIndex == 0)
                {
                    if (modelIndex == 1)
                        FoundIt = true;

                    //  iIndex = 0 일 때는 모든 iBuf 에 대해서 mPrevPos 에 값을 넣어준다.
                    //  iIndex > 0 이면 현재 iBuf 에만 값을 넣어주면 되며 바로 위에서 넣어줬다.
                    for (int cntBuf = 0; cntBuf < mPrevPos.Length; cntBuf++)
                    {
                        mPrevPos[cntBuf][si].X = posFoundS[si].X;
                        mPrevPos[cntBuf][si].Y = posFoundS[si].Y;
                        mPrevConv[cntBuf][si] = convFoundS[si];
                    }
                }

                //  각 마크의 COG 를 구한다.
                if (posFoundS[si].X == 0) continue;

                //Rect rc = new Rect();
                int subLeft = posFoundS[si].X * modelScaleFactor - 2;
                int subTop = posFoundS[si].Y * modelScaleFactor - 2;

                if (subLeft < 0)
                    subLeft = 0;
                if (subTop < 0)
                    subTop = 0;

                Rect subRoi = new Rect(subLeft, subTop, (lfMark.modelSize.Width + 1) * modelScaleFactor + 2, (lfMark.modelSize.Height + 1) * modelScaleFactor + 2);
                if (subRoi.Bottom > mSourceImg_Height)
                {
                    int deltaY = subRoi.Bottom - mSourceImg_Height;
                    if (deltaY > 1)
                    {
                        subRoi.Height -= (deltaY - 1);
                        subRoi.Y -= 1;
                    }
                    else
                    {
                        subRoi.Height -= deltaY;
                    }
                }
                subRoi.Left = subRoi.Left - 5;
                subRoi.Width = subRoi.Width + 10;

                Mat tgtImg = mSourceImg[iBuf].SubMat(subRoi);
                //if ( si==4 )
                //{
                //    tgtImg.SaveImage("makr4.bmp");
                //}
                byte[] tgtBuf = null;
                tgtImg.GetArray(out tgtBuf);

                smr[si].mMTF = GetMTF(tgtBuf, subRoi.Width, subRoi.Height);
                smr[si].pos.X = subLeft + subRoi.Width / 2;    //  중심좌표
                smr[si].pos.Y = subTop + subRoi.Height / 2;     //  중심좌표

                si++;

            }

            Nfound = cntFound;
            return posFoundS;
        }

        public byte GetCurUpDown(int iq , byte updown)
        {
            if (updown == 255)
                return 255;

            switch(iq)
            {
                case 0: return (byte)(updown % 2);
                    break;
                case 1: return (byte)((updown >> 1) % 2);
                    break;
                case 2: return (byte)((updown >> 2) % 2);
                    break;
                case 3: return (byte)((updown >> 3) % 2);
                    break;
                default: break;
            }
            return 0;
        }

        public double GetMTF(byte[] src, int width, int height)
        {
            double mtf = 0;
            int min = 255;
            int max = 0;
            int slope = 0;
            int maxslope = 0;
            double sumMaxSlope = 0;

            for (int j = 1; j < height-1; j++)
            {
                for (int i = 0; i < width - 2; i++)
                {
                    if (src[i + j * width] > max)
                        max = src[i + j * width];
                    if (src[i + j * width] < min)
                        min = src[i + j * width];

                    slope = Math.Abs(2 * src[i + (j - 1) * width] - src[i + 1 + (j - 1) * width] - src[i + 2 + (j - 1) * width]);
                    slope += Math.Abs(2 * src[i + j * width] - src[i + 1 + j * width] - src[i + 2 + j * width]);
                    slope += Math.Abs(2 * src[i + (j + 1) * width] - src[i + 1 + (j + 1) * width] - src[i + 2 + (j + 1) * width]);
                    if (slope > maxslope)
                        maxslope = slope;
                }
                sumMaxSlope += maxslope;
            }
            mtf = 2*sumMaxSlope/height;
            //byte[] copySrc = new byte[src.Length] ;
            //Array.Copy(src, copySrc, src.Length);

            //Array.Sort(copySrc);
            //double min = copySrc[0] + copySrc[1] + copySrc[2] + copySrc[3];
            //double max = copySrc[src.Length-1] + copySrc[src.Length-2] + copySrc[src.Length-3] + copySrc[src.Length-4];
            //mtf = (max - min) / (max + min);

            return mtf;
        }
        public void SetCurUpDown(int iq, byte srcUpdown, ref byte resUpdown)
        {
            if (srcUpdown == 255)
                resUpdown = 255;

            switch (iq)
            {
                case 0:
                    resUpdown = (byte)(resUpdown | srcUpdown);
                    break;
                case 1:
                    resUpdown = (byte)(resUpdown | (srcUpdown<<1));
                    break;
                case 2:
                    resUpdown = (byte)(resUpdown | (srcUpdown<<2));
                    break;
                case 3:
                    resUpdown = (byte)(resUpdown | (srcUpdown<<3));
                    break;
            }
        }
        public void SwapPoint(ref OpenCvSharp.Point pA, ref OpenCvSharp.Point pB)
        {
            OpenCvSharp.Point tmp = new OpenCvSharp.Point();
            tmp.X = pA.X;
            tmp.Y = pA.Y;
            pA.X = pB.X;
            pA.Y = pB.Y;
            pB.X = tmp.X;
            pB.Y = tmp.Y;
        }

        public long CalcConv(ref sFiducialMark lfmark, int[] mimg, uint lwidth, uint lheight, int sub, ref double subconv, int iDetectedMark)
        {
            return 0;


            //  현재 사용하지 않는 함수
            int minBin = 99;
            int maxBin = 0;
            int[] srcData = new int[lwidth * lheight];
            int[] sorted = new int[lwidth * lheight];

            for (uint j = 0; j < lheight; j++)
                for (uint i = 0; i < lwidth; i++)
                    sorted[i + j * lwidth] = mimg[ i + ( j) * lwidth];

            Array.Sort(sorted); //  더 빠른 함수로 변경 필요  231116  ??

            //int bin2pro = (int)(0.98 * lwidth * lheight);
            int bin5pro = (int)(0.96 * lwidth * lheight);
            int bin80pro = (int)(lwidth * lheight / 5);
            int bin95pro = (int)(lwidth * lheight / 20);

            minBin = sorted[bin80pro];
            maxBin = sorted[bin5pro];

            minBin = (maxBin + minBin) / 2 + (maxBin - minBin) / 40;

            if (maxBin - minBin < 5)
                return 0;

            long res = 0;
            long res2 = 0;
            uint lwidth_j = 0;

            for (uint j = 0; j < lheight; j++)
            {
                lwidth_j = lwidth * j;
                for (uint i = 0; i < lwidth; i++)
                {
                    srcData[i + j * lwidth] = mimg[i + (j) * lwidth];

                    if (mimg[i + (j) * lwidth] < minBin - 1)
                        res -= (lfmark.img[i + lwidth_j] % 100 - 20);  //  * -1
                    else if (mimg[i + (j) * lwidth] > minBin + 2)
                        res += (lfmark.img[i + lwidth_j] % 100 - 20);  //  * +1

                    if (mimg[i + (j) * lwidth] < minBin - 1)
                        res2 -= (lfmark.img[i + lwidth_j] / 100 - 20);  //  * -1
                    else if (mimg[i + (j) * lwidth] > minBin + 2)
                        res2 += (lfmark.img[i + lwidth_j] / 100 - 20);  //  * +1

                    //q_cur = q_ValueImg[x + i + yj];
                    //modValue = lfmark_img[i + lwidth_j];

                    ////  부호가 다르면 2배로 빼주는 방식을 적용한다.
                    //if (q_cur < minBin && modValue < 0)
                    //    res -= modValue;  //  * -1
                    //else if (q_cur < minBin && modValue > 0)
                    //    res -= 4 * modValue;  //  * -1
                    //else if (q_cur > minBin && modValue > 0)
                    //    res += modValue;  //  * -1
                    //else if (q_cur > minBin && modValue < 0)
                    //    res += 4 * modValue;  //  * -1

                }
            }
            subconv = 0;

            switch (sub % 100)
            {
                case 1:
                    mCalcDiffConv(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].diff = subconv;
                    break;
                case 2:
                    mCalcXDiffConv(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].Xdiff = subconv;
                    break;
                case 3:
                    mCalcYDiffConv(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].Ydiff = subconv;
                    break;
                case 4:
                    mCalcInOutAvg(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].IO = subconv;
                    break;
                case 5:
                    mCalcLeftRightAvg(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].LR = subconv;
                    break;
                case 6:
                    mCalcTopBottomAvg(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].TB = subconv;
                    break;
                case 7:
                    mCalcCenterToLeftRightAvg(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].CLR = subconv;
                    break;
                case 8:
                    mCalcCenterToTopBottomAvg(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].CTB = subconv;
                    break;
                case 9:
                    mCalcFullStdev(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].std = subconv;
                    break;
                case 10:
                    mCalcInFullStdev(mDetectedmark[iDetectedMark].img, ref lfmark, ref subconv);
                    mDetectedmark[iDetectedMark].IFstd = subconv;
                    break;
                default:
                    break;
            }
            res = res > res2 ? res : res2;
            return res;
        }
        //public long CalcConv(int model, int x, int y, int iBuf, ref double subconv, bool blast = false)
        public long CalcConv(ref sFiducialMark lfmark, int x, int y, int iBuf, ref double subconv, bool blast = false)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            long res = 0;
            long res2 = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;
            int bufLength = lwidth * lheight;

            //List<byte> brSorted = new List<byte>();
            //byte[] sorted = null;
            int[] sorted = new int[256];
            Rect exArea = lfmark.exArea;
            int i = 0;


            try
            {
                int quaterWidth = mSourceImg[iBuf].Width / lfmark.MScale;
                int minBin = 99;
                int maxBin = 0;
                //int ip = 0;
                int yj = 0;
                ref byte[] q_ValueImg = ref q_Value[iBuf];
                int fullCnt = 0;

                if (exArea.Width>0)
                {
                    for (int j = 0; j < lheight; j++)
                    {
                        yj = (y + j) * quaterWidth;
                        for ( i = 0; i < lwidth; i++)
                        {
                            if (exArea.Contains(new OpenCvSharp.Point(i, j)))
                                continue;

                            //brSorted.Add(q_ValueImg[x + i + yj]);
                            sorted[q_ValueImg[x + i + yj]]++;
                            fullCnt++;
                        }
                    }
                }
                else
                {
                    //sorted = new byte[((lheight + 1) / 2) * ((lwidth + 1) / 2)];
                    int k = 0;
                    fullCnt = lheight* lwidth;
                    for (int j = 0; j < lheight; j++)
                    {
                        yj = (y + j) * quaterWidth;
                        for ( i = 0; i < lwidth; i++)
                        {
                            //sorted[k++] = q_ValueImg[x + i + yj];
                            sorted[q_ValueImg[x + i + yj]]++;
                            //fullCnt++;
                        }
                    }
                }

                //Array.Sort(sorted); //  더 빠른 함수로 변경 필요  231116  !!  18*12 = 216 >> 54, 18*18 = 324 >> 81
                int accCnt = 0;
                int bin5pro = (int)(0.96 * fullCnt);
                int bin50pro = (int)(fullCnt / 2);  //  bin80pro = (int)(fullCnt / 5);
                for (i = 0; i < 256; i++)
                {
                    accCnt += sorted[i];
                    if (accCnt > bin50pro)// if (accCnt > bin80pro)
                        break;
                }
                minBin = i - 1;//                minBin = i ;

                i++;
                for (; i < 256; i++)
                {
                    accCnt += sorted[i];
                    if (accCnt > bin5pro)
                        break;
                }
                maxBin = i;

                if (maxBin - minBin < 5)
                    return -5000;


                int q_cur = 0;
                int j_half = lheight / 2;
                int j_half2 = (int)(lheight * 0.7);
                int lwidth_j = 0;
                ref int[] lfmark_img = ref lfmark.img;
                int modValue = 0;
                int modValue2 = 0;

                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    lwidth_j = lwidth * j;

                    for ( i = 0; i < lwidth; i++)
                    {
                        q_cur = q_ValueImg[x + i + yj];
                        modValue = (lfmark_img[i + lwidth_j] % 100 - 20);
                        modValue2 = (lfmark_img[i + lwidth_j] / 100 - 20);

                        //  부호가 다르면 2배로 빼주는 방식을 적용한다.
                        if (q_cur < minBin && modValue < 0)
                            res -= modValue;  //  * -1
                        else if (q_cur < minBin && modValue > 0)
                            res -= modValue;  //  * -1
                        else if (q_cur > minBin && modValue > 0)
                            res += modValue;  //  * -1
                        else if (q_cur > minBin && modValue < 0)
                            res += modValue;  //  * -1

                        if (q_cur < minBin && modValue2 < 0)
                            res2 -= modValue2;  //  * -1
                        else if (q_cur < minBin && modValue2 > 0)
                            res2 -= modValue2;  //  * -1
                        else if (q_cur > minBin && modValue2 > 0)
                            res2 += modValue2;  //  * -1
                        else if (q_cur > minBin && modValue2 < 0)
                            res2 += modValue2;  //  * -1

                        //if (q_cur < minBinMinus1)
                        //    res -= img[i + lwidth_j];  //  * -1
                        //else if (q_cur > minBinPlus2)
                        //    res += img[i + lwidth_j];  //  * +1

                        //q_cur = q_ValueImg[x + i + yj];

                        ////res += (q_cur < minBinMinus1 ? -lfmark.img[i + lwidth_j] : (q_cur > minBinPlus2 ? lfmark.img[i + lwidth_j] : 0));

                        //if (q_cur < minBin)
                        //    res -= lfmark_img[i + lwidth_j];  //  * -1
                        //else if (q_cur > minBin)
                        //    res += lfmark_img[i + lwidth_j];  //  * +1
                    }
                    if (j == j_half)
                    {
                        if (res < 0)
                            return 2 * res;
                    }
                    else if (j == j_half2)
                    {
                        if (res < 0)
                            return (int)(1.4 * res);
                    }
                }
            }
            catch(Exception e)
            {
                ;
            }


            //subconv = 0;
            //if (res > lfmark.conv)
            //{
            //    switch (lfmark.sub % 100)
            //    {
            //        case 1:
            //            mCalcDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 2:
            //            mCalcXDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 3:
            //            mCalcYDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 4:
            //            mCalcInOutAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 5:
            //            mCalcLeftRightAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 6:
            //            mCalcTopBottomAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 7:
            //            mCalcCenterToLeftRightAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 8:
            //            mCalcCenterToTopBottomAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 9:
            //            mCalcFullStdev(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 10:
            //            mCalcInFullStdev(srcData, ref lfmark, ref subconv);
            //            break;
            //        default:
            //            break;
            //    }
            //}
            res = res > res2 ? res : res2;
            return res;
        }
        public long callCount = 0;

        public long CalcConv(int lwidth, int lheight, int modelScale, ref int[] img, int x, int y, int iBuf, ref double subconv, ref Rect exArea)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            long res = 0;
            long res2 = 0;
            //int lwidth = modelWidth;// lfmark.modelSize.Width;
            //int lheight = modelHeight;// lfmark.modelSize.Height;
            int bufLength = lwidth * lheight;
            //List<byte> brSorted = new List<byte>();
            //byte[] sorted = null;
            int[] sorted = new int[256];
            callCount++;
            int i = 0;
            //int[] curImg = new int[img.Length];

            try
            {
                int quaterWidth = mSourceImg[iBuf].Width / modelScale;// lfmark.MScale;
                int quaterHeight = mSourceImg[iBuf].Height / modelScale;// lfmark.MScale;

                if (quaterHeight < y + lheight)
                    return 0;

                int minBin = 99;
                int maxBin = 0;
                //int ip = 0;
                int yj = 0;
                int lwidth_j = 0;
                ref byte[] q_ValueImg = ref q_Value[iBuf];
                int fullCnt = 0;

                if (exArea.Width > 0)
                {
                    for (int j = 0; j < lheight; j++)
                    {
                        yj = (y + j) * quaterWidth;
                        for ( i = 0; i < lwidth; i++)
                        {
                            if (exArea.Contains(new OpenCvSharp.Point(i, j)))
                                continue;

                            //brSorted.Add(q_ValueImg[x + i + yj]);
                            sorted[q_ValueImg[x + i + yj]]++;
                            fullCnt++;
                        }
                    }
                    //sorted = brSorted.ToArray();
                }
                else
                {
                    //sorted = new byte[((lheight+1)/2) * ((lwidth+1)/2)];
                    int qq = 0;
                    fullCnt = lheight * lwidth;

                    for (int j = 0; j < lheight; j++ )
                    {
                        yj = (y + j) * quaterWidth;
                        for (i = 0; i < lwidth; i ++)
                        {
                            //sorted[k++] = q_ValueImg[x + i + yj];
                            sorted[q_ValueImg[x + i + yj]]++;
                            //fullCnt++;
                        }
                    }
                }
                //Array.Sort(sorted); //  더 빠른 함수로 변경 필요  231116  !!  18*12 = 216 >> 54, 18*18 = 324 >> 81
                int accCnt = 0;
                int bin5pro = (int)(0.96 * fullCnt);
                int bin50pro = (int)(fullCnt / 2);  //  bin80pro = (int)(fullCnt / 5);
                for (i = 0; i < 256; i++)
                {
                    accCnt += sorted[i];
                    if (accCnt > bin50pro)// if (accCnt > bin80pro)
                        break;
                }
                minBin = i-1;//                minBin = i ;

                i++;
                for (; i < 256; i++)
                {
                    accCnt += sorted[i];
                    if (accCnt > bin5pro)
                        break;
                }
                maxBin = i;

                if (maxBin - minBin < 5)
                    return -5000;

                //int minBinMinus1 = minBin ;
                //int minBinPlus2 = minBin ;//+2
                int q_cur = 0;
                int j_half = lheight / 2;
                int j_half2 = (int)(lheight * 0.7);
                int modValue = 0;
                int modValue2 = 0;

                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    lwidth_j = lwidth * j;

                    for ( i = 0; i < lwidth; i++)
                    {
                        //q_cur = q_Value[iBuf][x + i + yj];
                        q_cur = q_ValueImg[x + i + yj];
                        //curImg[i + lwidth_j] = q_cur;

                        modValue = img[i + lwidth_j] % 100 - 20;
                        modValue2 = img[i + lwidth_j] / 100 - 20;
                        //res += (q_cur < minBinMinus1 ? -img[i + lwidth_j] : (q_cur > minBinPlus2 ? img[i + lwidth_j] : 0));

                        //  부호가 다르면 2배로 빼주는 방식을 적용한다.
                        if (q_cur < minBin && modValue < 0)
                            res -= modValue;  //  * -1
                        else if (q_cur < minBin && modValue > 0)
                            res -= modValue;  //  * -1
                        else if (q_cur > minBin && modValue > 0)
                            res += modValue;  //  * -1
                        else if (q_cur > minBin && modValue < 0)
                            res += modValue;  //  * -1

                        if (q_cur < minBin && modValue2 < 0)
                            res2 -= modValue2;  //  * -1
                        else if (q_cur < minBin && modValue2 > 0)
                            res2 -= modValue2;  //  * -1
                        else if (q_cur > minBin && modValue2 > 0)
                            res2 += modValue2;  //  * -1
                        else if (q_cur > minBin && modValue2 < 0)
                            res2 += modValue2;  //  * -1

                        //if (q_cur < minBinMinus1)
                        //    res -= img[i + lwidth_j];  //  * -1
                        //else if (q_cur > minBinPlus2)
                        //    res += img[i + lwidth_j];  //  * +1
                    }
                    if (j == j_half)
                    {
                        if (res < 0)
                            return 2 * res;
                    }
                    else if (j == j_half2)
                    {
                        if (res < 500)
                            return (int)(1.4 * res);
                    }
                }
            }
            catch (Exception e)
            {
                ;
            }
            subconv = 0;
            //if (res > lfmark.conv)
            //{
            //    switch (lfmark.sub % 100)
            //    {
            //        case 1:
            //            mCalcDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 2:
            //            mCalcXDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 3:
            //            mCalcYDiffConv(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 4:
            //            mCalcInOutAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 5:
            //            mCalcLeftRightAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 6:
            //            mCalcTopBottomAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 7:
            //            mCalcCenterToLeftRightAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 8:
            //            mCalcCenterToTopBottomAvg(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 9:
            //            mCalcFullStdev(srcData, ref lfmark, ref subconv);
            //            break;
            //        case 10:
            //            mCalcInFullStdev(srcData, ref lfmark, ref subconv);
            //            break;
            //        default:
            //            break;
            //    }
            //}
            res = res > res2 ? res : res2;
            return res;
        }
        public long mCalcConv(byte[] srcData, ref sFiducialMark lfmark)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;
            long res = 0;
            long res2 = 0;
            int bufLength = lwidth * lheight;

            int minBin = 99;
            int maxBin = 0;
            List<int> lbrSort = new List<int>();
            Rect ignoreRc = lfmark.exArea;
            int[] sorted = new int[256];
            //int[] curImg = new int[lfmark.img.Length];
            ref int[] lfmark_img = ref lfmark.img;


            for (int x = 0; x < lwidth; x++)
                for (int y = 0; y < lheight; y++)
                {
                    if (ignoreRc.Contains(new OpenCvSharp.Point(x, y)))
                        continue;

                    sorted[srcData[x + y * lwidth]]++;

                }

            int accCnt = 0;
            int bin5pro = (int)(0.96 * bufLength);
            int bin50pro = (int)(bufLength / 2);  //  bin80pro = (int)(fullCnt / 5);
            int i = 0;

            for (i = 0; i < 256; i++)
            {
                accCnt += sorted[i];
                if (accCnt > bin50pro)// if (accCnt > bin80pro)
                    break;
            }
            minBin = i - 1;//                minBin = i ;

            i++;
            for (; i < 256; i++)
            {
                accCnt += sorted[i];
                if (accCnt > bin5pro)
                    break;
            }
            maxBin = i;

            ////  모델 만들때는 Skip 할 필요 없음.
            ////  Skip When Low Contrast 
            //if (maxBin - minBin < 10)
            //    return -1;

            int minBinMinus1 = minBin - 1;
            int minBinPlus2 = minBin + 1;
            int q_cur = 0;
            int lwidth_j = 0;
            //int j_half = lheight / 2;
            //int j_half2 = (int)(lheight * 0.7);

            int modValue = 0;
            int modValue2 = 0;

            for (int j = 0; j < lheight; j++)
            {
                lwidth_j = lwidth * j;

                for (i = 0; i < lwidth; i++)
                {
                    q_cur = srcData[i + j* lwidth];

                    modValue = (lfmark_img[i + lwidth_j] % 100 - 20);
                    modValue2 = (lfmark_img[i + lwidth_j] / 100 - 20);

                    //  부호가 다르면 2배로 빼주는 방식을 적용한다.
                    if (q_cur < minBinMinus1 && modValue < 0)
                        res -= modValue;  //  * -1
                    else if (q_cur < minBinMinus1 && modValue > 0)
                        res -= modValue;  //  * -1
                    else if (q_cur > minBinPlus2 && modValue > 0)
                        res += modValue;  //  * -1
                    else if (q_cur > minBinPlus2 && modValue < 0)
                        res += modValue;  //  * -1

                    if (q_cur < minBinMinus1 && modValue2 < 0)
                        res2 -= modValue2;  //  * -1
                    else if (q_cur < minBinMinus1 && modValue2 > 0)
                        res2 -= modValue2;  //  * -1
                    else if (q_cur > minBinPlus2 && modValue2 > 0)
                        res2 += modValue2;  //  * -1
                    else if (q_cur > minBinPlus2 && modValue2 < 0)
                        res2 += modValue2;  //  * -1

                    //if (q_cur < minBinMinus1)
                    //    res -= lfmark.img[i + lwidth_j] % 100 - 20;  //  * -1
                    //else if (q_cur > minBinPlus2)
                    //    res += lfmark.img[i + lwidth_j] % 100 - 20;  //  * +1

                    //if (q_cur < minBinMinus1)
                    //    res2 -= lfmark.img[i + lwidth_j] / 100 - 20;  //  * -1
                    //else if (q_cur > minBinPlus2)
                    //    res2 += lfmark.img[i + lwidth_j] / 100 - 20;  //  * +1
                }
            }
            res = res > res2 ? res : res2;
            return res;
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (mOverlayedImg == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                //Cv2.ImShow("A", mOverlayedImg);
                if (mFirstRect)
                {
                    if (mSelectRect.Width > 5 && mSelectRect.Height > 5 && mSelectRect.X > 0 && mSelectRect.Y > 0)
                    {
                        mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
                        //CreateBaseModel();
                        mFirstRect = false;
                    }
                    else if (mSelectRect.Width > 5 && mSelectRect.Height > 5 && mSelectRect.X == 0 && mSelectRect.Y == 0)
                    {
                        mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
                        mFirstRect = false;
                    }
                }
                int xstart = (int)((e.X + 1) / (double)pictureBox2.Size.Width * mSourceImg[0].Width);
                int ystart = (int)((e.Y + 1) / (double)pictureBox2.Size.Height * mSourceImg[0].Height);

                mSelectRect.X = xstart - mSelectRect.Width / 2;
                mSelectRect.Y = ystart - mSelectRect.Height / 2;

                //CreateBaseModel();
                mOverlayedImg.Rectangle(mSelectRect, Scalar.Cyan, 2);
                //Cv2.ImShow("A", mOverlayedImg);

                Bitmap image = null;
                image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mOverlayedImg);

                pictureBox2.Image = image;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            }
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            mCst.StartPoint(pictureBox2, e);
            mSearchIgnore = new Rect();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            mCst.TrackRubberBand(pictureBox2, e);

            Rect lRect = new Rect(mCst.X, mCst.Y, (e.X - mCst.X), (e.Y - mCst.Y));
            if (mSourceImg[0] != null && mCst.IsTracking > 0)
            {
                Rectangle lRectangle = mCst.GetSelectRect();
                double xstart = (lRectangle.X) / (double)pictureBox2.Size.Width * mSourceImg[0].Width;
                double ystart = (lRectangle.Y) / (double)pictureBox2.Size.Height * mSourceImg[0].Height;

                if (mCst.IsTracking > 0 && mCst.IsTracking < 10)
                {
                    ///////////////////////////////////////////////////
                    //  Tracker 를 새로 만들거나 크기를 바꾸는 경우
                    double lwidth = lRectangle.Width / (double)pictureBox2.Size.Width * mSourceImg[0].Width;
                    double lheight = lRectangle.Height / (double)pictureBox2.Size.Height * mSourceImg[0].Height;
                    mSelectRect = new Rect((int)xstart, (int)ystart, (int)lwidth, (int)lheight);

                    mFirstRect = true;
                }
                else if (mCst.IsTracking == 10)
                {
                    ///////////////////////////////////////////////////
                    //  이미 있는 tracker 를 이동하는 경우

                    mSelectRect.X = (int)xstart;
                    mSelectRect.Y = (int)ystart;
                    mFirstRect = true;
                    //label6.Text = mSelectRect.ToString();
                }
                //	보여지는 영상은 mSourceImg 에 들어있다.
                //	영상 클립박스

                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    if (mCurrentScell.X == 4 || mCurrentTcell.X == 4)
                    {
                        //  Model 지정 작업중인 경우
                        Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                        //	영상클립
                        mCustomImg = mSourceImg[0].SubMat(roi);
                        Rect roi2 = new Rect((int)mSelectRect.X + 1, (int)mSelectRect.Y + 1, mSelectRect.Width, mSelectRect.Height);
                        mCustomImg2 = mSourceImg[0].SubMat(roi2);

                        //	보여주기
                        Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                        pictureBox5.Image = myImage;
                        pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;

                        //if (mCurrentScell.X > 0)
                        //    dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = mSelectRect.Width.ToString() + "x" + mSelectRect.Height.ToString();
                        //if (mCurrentTcell.X > 0)
                        //    dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[mCurrentTcell.X].Value = mSelectRect.Width.ToString() + "x" + mSelectRect.Height.ToString();
                    }
                    //else if (mCurrentScell.X == 5 || mCurrentTcell.X == 5)
                    //{
                    //    //  ROI 지정작업중인 경우
                    //    if (mCurrentScell.X > 0)
                    //        dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = mSelectRect.X.ToString() + "," + mSelectRect.Y.ToString() + "," + mSelectRect.Width.ToString() + "," + mSelectRect.Height.ToString();
                    //    if (mCurrentTcell.X > 0)
                    //        dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[mCurrentTcell.X].Value  = mSelectRect.X.ToString() + "," + mSelectRect.Y.ToString() + "," + mSelectRect.Width.ToString() + "," + mSelectRect.Height.ToString();
                    //}
                }
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            mCst.EndPoint(pictureBox2, e);
            UpdateMousePosition();

            RedrawROIImg();

        }

        public void RedrawROIImg()
        {
            if (mCustomImg == null)
                return;

            double pW = pictureBox5Size.Width;
            double pH = pictureBox5Size.Height;


            double cW = mCustomImg.Width;
            double cH = mCustomImg.Height;

            if (pH / pW > cH / cW)   //  높이가 여유가 있는 경우 높이를 조정한다.
                pH = ((pW / cW) * cH);
            else
                pW = ((pH / cH) * cW);

            pictureBox5.Size = new System.Drawing.Size((int)pW, (int)pH);
        }

        private void UpdateMousePosition()
        {
            
            if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
            {
                if (mCurrentScell.X == 4 || mCurrentTcell.X == 4)
                {
                    //  default SearchROI : 정확히는 Search Start Point 가 될 영역이므로 모델의 크기를 고려할 필요는 없다.
                    int lwidth = int.Parse(tbDefaultROIWidth.Text);
                    int lheight = int.Parse(tbDefaultROIHeight.Text);
                    int left = mSelectRect.X - (int)(lwidth / 31.429) - 5;
                    int Rwidth = (int)(lwidth / 15.714);
                    if (left < 0)
                        left = 0;

                    if (mCurrentScell.X > 0)
                    {
                        dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = (mSelectRect.Width/mModelScale).ToString() + "," + (mSelectRect.Height/ mModelScale).ToString();
                        mFidMarkSide[mCurrentScell.Y].modelSize = new OpenCvSharp.Size(mSelectRect.Width/ mModelScale, mSelectRect.Height/ mModelScale);

                        int top = mSelectRect.Y - (int)(lheight / 48.894);
                        int Rheight = (int)(lheight /24.447) + 12;
                        if (top < 0)
                            top = 0;

                        Rect sRoi = new Rect(left, top, Rwidth, Rheight);
                        mFidMarkSide[mCurrentScell.Y].searchRoi = sRoi;
                        dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[5].Value = sRoi.X.ToString() + "," + sRoi.Y.ToString() + "," + sRoi.Width.ToString() + "," + sRoi.Height.ToString();
                    }
                    if (mCurrentTcell.X > 0)
                    {
                        dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[mCurrentTcell.X].Value = (mSelectRect.Width/ mModelScale).ToString() + "," + (mSelectRect.Height/ mModelScale).ToString();
                        mFidMarkTop[mCurrentTcell.Y].modelSize = new OpenCvSharp.Size(mSelectRect.Width/ mModelScale, mSelectRect.Height/ mModelScale);

                        int top = mSelectRect.Y - (int)(lheight / 31.429);
                        int Rheight = (int)(lheight / 15.714);
                        if (top < 0)
                            top = 0;

                        Rect sRoi = new Rect(left, top, Rwidth, Rheight);
                        mFidMarkTop[mCurrentTcell.Y].searchRoi = sRoi;
                        dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[5].Value = sRoi.X.ToString() + "," + sRoi.Y.ToString() + "," + sRoi.Width.ToString() + "," + sRoi.Height.ToString();
                    }
                }
                else if (mCurrentScell.X == 5 || mCurrentTcell.X == 5)
                {

                    //ROI 지정
                    if (mCurrentScell.X > 0)
                    {
                        dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = mSelectRect.X.ToString() + "," + mSelectRect.Y.ToString() + "," + mSelectRect.Width.ToString() + "," + mSelectRect.Height.ToString();
                        mFidMarkSide[mCurrentScell.Y].searchRoi = mSelectRect;
                    }
                    if (mCurrentTcell.X > 0)
                    {
                        dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[mCurrentTcell.X].Value = mSelectRect.X.ToString() + "," + mSelectRect.Y.ToString() + "," + mSelectRect.Width.ToString() + "," + mSelectRect.Height.ToString();
                        mFidMarkTop[mCurrentTcell.Y].searchRoi = mSelectRect;
                    }
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            mCst.DrawRubberBand(pictureBox2, e);
        }

        private void dataGridView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex > 3)
                UpdateSelectSub(e.ColumnIndex);
        }

        public void UpdateSelectSub(int ColumnIndex)
        {
            string[] lstr =
            {
                "",
                "Diff",
                "XDiff",
                "YDiff",
                "I-O",
                "L-R",
                "T-B",
                "C-LR",
                "C-TB",
                "σ",
                "IFσ",
            };

            if (ColumnIndex > 3)
            {
                if (mSelectedSubIndex > 0)
                    dataGridView2.Columns[mSelectedSubIndex + 3].HeaderCell.Style.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);

                mSelectedSubIndex = (ColumnIndex - 3);
                dataGridView2.Columns[ColumnIndex].HeaderCell.Style.BackColor = Color.Red;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            sSearchModel[] lOldModel = new sSearchModel[10];
            for (int i = 0; i < 10; i++)
            {
                if (mSearchModel[i].img != null)
                {
                    lOldModel[i].img = new int[mSearchModel[i].width * mSearchModel[i].height];
                    lOldModel[i] = mSearchModel[i];
                }
                else
                    break;
            }

            for (int i = 0; i < 10; i++)
            {
                if (mNewSearchModel[i].img != null)
                {
                    mSearchModel[i].img = mNewSearchModel[i].img;
                    mSearchModel[i].conv = mNewSearchModel[i].conv;
                    mSearchModel[i].planeShift = mNewSearchModel[i].planeShift;
                    mSearchModel[i].planeHeight = mNewSearchModel[i].planeHeight;
                }
            }

            //dataGridView1.Rows.Clear();

            //DetectInSelectedFile(mSourceImgFile);
            DetectInAllFiles();

            for (int i = 0; i < 10; i++)
            {
                if (mNewSearchModel[i].img != null)
                {
                    mSearchModel[i] = lOldModel[i];
                }
                else
                    continue;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mDetectEveryMark = cbDetectEveryMark.Checked;
            InitialzeDataSet(mTargetFiles.Length * 20);
            SetSearchYRegion();
            BackupFMI();
            DetectInAllFiles();
            RecoverFromBackupFMI();
        }

        public void DetectInAllFiles()
        {
            ClearDataGridView1();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            int i = 0;
            while (i < listBox1.Items.Count)
            {
                mOrgFile = listBox1.Items[i].ToString();

                if (mSourceImg == null)
                    continue;
                else
                    mSourceImg[0] = new Mat(mOrgFile, ImreadModes.Grayscale);

                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mSourceImg[0]);

                pictureBox2.Image = myImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

                DetectInSelectedFile(i);
                i++;
            }
        }

        Rect[] mModelROI = new Rect[6];
        private void button12_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, -1);
                mSelectRect.Y--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    Rect roi2 = new Rect((int)mSelectRect.X + 1, (int)mSelectRect.Y + 1, mSelectRect.Width, mSelectRect.Height);
                    mCustomImg2 = mSourceImg[0].SubMat(roi2);
                    //	보여주기
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, -1, 0, 0);
                ResizeROIrect(delta);
            }
        }

        public void ResizeROIrect(Rect delta)
        {
            sFiducialMark lfMark = mCurFocusedMark;

            if (lfMark == null)
                return;

            int lWidth = lfMark.modelSize.Width;
            int lHeight = lfMark.modelSize.Height;

            if (lWidth == 0)
                return;

            byte[] tmpBytes = new byte[lWidth * lHeight];

            if (lfMark.img == null)
                return;

            if (lfMark.img.Length < lWidth * lHeight)
                return;

            OpenCvSharp.Rect roi = new Rect();
            roi.X       = lfMark.searchRoi.X        + delta.X;
            roi.Y       = lfMark.searchRoi.Y        + delta.Y;
            roi.Width   = lfMark.searchRoi.Width    + delta.Width + lfMark.modelSize.Width * lfMark.MScale;
            roi.Height  = lfMark.searchRoi.Height   + delta.Height + lfMark.modelSize.Height * lfMark.MScale;
            
            lfMark.searchRoi.X       += delta.X       ;
            lfMark.searchRoi.Y       += delta.Y       ;
            lfMark.searchRoi.Width   += delta.Width   ;
            lfMark.searchRoi.Height  += delta.Height  ;
            
            mOverlayedImg = new Mat();
            Cv2.CvtColor(mSourceImg[0], mOverlayedImg, ColorConversionCodes.GRAY2RGB);

            DrawSearchROI(roi);

            if (mCurrentScell.X > 0)
            {
                dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = roi.X.ToString() + "," + roi.Y.ToString() + "," + roi.Width.ToString() + "," + roi.Height.ToString();
            }
            if (mCurrentTcell.X > 0)
            {
                dgvDesignNModelTop.Rows[mCurrentTcell.Y].Cells[mCurrentTcell.X].Value = roi.X.ToString() + "," + roi.Y.ToString() + "," + roi.Width.ToString() + "," + roi.Height.ToString();
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, -1, 0);
                mSelectRect.X--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    Rect roi2 = new Rect((int)mSelectRect.X + 1, (int)mSelectRect.Y + 1, mSelectRect.Width, mSelectRect.Height);
                    mCustomImg2 = mSourceImg[0].SubMat(roi2);
                    //	보여주기
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(-1, 0, 0, 0);
                ResizeROIrect(delta);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 1, 0);
                mSelectRect.X++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    Rect roi2 = new Rect((int)mSelectRect.X + 1, (int)mSelectRect.Y + 1, mSelectRect.Width, mSelectRect.Height);
                    mCustomImg2 = mSourceImg[0].SubMat(roi2);
                    //	보여주기
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }else if (IsSeardchROIUpdate)
                {
                    Rect delta = new Rect(1, 0, 0, 0);
            ResizeROIrect(delta);
        }
    }

private void button15_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, +1);
                mSelectRect.Y++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    Rect roi2 = new Rect((int)mSelectRect.X + 1, (int)mSelectRect.Y + 1, mSelectRect.Width, mSelectRect.Height);
                    mCustomImg2 = mSourceImg[0].SubMat(roi2);
                    //	보여주기
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 1, 0, 0);
                ResizeROIrect(delta);
            }
        }
        public int mModelScale = 4;

        List<OpenCvSharp.Size> mSizeFidMarkTop = new List<OpenCvSharp.Size>();
        List<OpenCvSharp.Size> mSizeFidMarkSide = new List<OpenCvSharp.Size>();
        private void button5_Click(object sender, EventArgs e)
        {
            CreateModel();
        }

        public void CreateModel()
        {
            if (mSelectRect.Width == 0)
            {
                MessageBox.Show("Try after making ROI");
                return;
            }
            if (mCurrentScell.X != 4 && mCurrentTcell.X != 4)
                return;

            mModelScale = 8 - lbModelScale.SelectedIndex;

            double lmodelScale = mModelScale;
            byte[] srcData = null;
            Mat qSubImg = new Mat();
            Cv2.Resize(mCustomImg, qSubImg, new OpenCvSharp.Size(mCustomImg.Width / mModelScale, mCustomImg.Height / mModelScale), 1.0 / mModelScale, 1.0 / mModelScale, InterpolationFlags.Area);  //  1/mModelScale 축소
            qSubImg.GetArray(out srcData);

            //Cv2.ImShow("org", mCustomImg);

            byte[] srcData2 = null;
            Mat qSubImg2 = new Mat();
            Cv2.Resize(mCustomImg2, qSubImg2, new OpenCvSharp.Size(mCustomImg.Width / mModelScale, mCustomImg.Height / mModelScale), 1.0 / mModelScale, 1.0 / mModelScale, InterpolationFlags.Area);  //  1/mModelScale 축소
            qSubImg2.GetArray(out srcData2);

            //Cv2.ImShow("shift", mCustomImg2);

            int modelNo = 0;

            if (mCurrentScell.Y >= 0)
                modelNo = mCurrentScell.Y;

            else if (mCurrentTcell.Y >= 0)
                modelNo = mCurrentTcell.Y + 8;

            int lwidth = qSubImg.Width;
            int lheight = qSubImg.Height;

            mSearchModelIndex = modelNo;

            sFiducialMark lfMark = null;

            mSizeFidMarkTop.Clear();
            mSizeFidMarkSide.Clear();
            mSizeFidMarkTop = new List<OpenCvSharp.Size>();
            mSizeFidMarkSide = new List<OpenCvSharp.Size>();
            foreach (sFiducialMark sfm in mFidMarkSide)
            {
                OpenCvSharp.Size lsize = new OpenCvSharp.Size();
                lsize.Width = sfm.modelSize.Width;
                lsize.Height = sfm.modelSize.Height;
                mSizeFidMarkSide.Add(lsize);
            }
            foreach (sFiducialMark sfm in mFidMarkTop)
            {
                OpenCvSharp.Size lsize = new OpenCvSharp.Size();
                lsize.Width = sfm.modelSize.Width;
                lsize.Height = sfm.modelSize.Height;
                mSizeFidMarkTop.Add(lsize);
            }

            if (mSearchModelIndex < 8)
            {
                lfMark = mFidMarkSide[modelNo];
                mModelROI[modelNo] = mSelectRect;
            }
            else
            {
                lfMark = mFidMarkTop[modelNo - 8];
                mModelROI[modelNo - 4] = mSelectRect;
            }

            //  Create Model Data
            //  Edge 주변만 살리고 나머지 축이는 방식 검토 필요.
            //  다음 반드시 Pair 로 사용해야 함.   231009
            CreateModelFromSubImg(srcData, lwidth, lheight, lmodelScale, ref lfMark, true, false);
            CreateModelFromSubImg(srcData2, lwidth, lheight, lmodelScale, ref lfMark, true, true);

            richTextBox1.Text += "Custom Model conv = " + lfMark.conv.ToString("F0") + "\r\n";
            richTextBox1.Text += "Model Size = " + lfMark.modelSize.Width.ToString() + "," + lfMark.modelSize.Height.ToString() + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();

            //StreamWriter wr = new StreamWriter("VerifyCustom.csv");
            //string lstr = "";
            //for (int y = 0; y < lheight; y++)
            //{
            //    lstr = "";
            //    for (int x = 0; x < lwidth; x++)
            //        lstr += lSort[x + y * lwidth].ToString("F0") + ",";
            //    wr.WriteLine(lstr);
            //}
            //wr.Close();

            byte[] tmpBytes = new byte[lwidth * lheight];
            for (int i = 0; i < lwidth * lheight; i++)
                tmpBytes[i] = (byte)(6 * (lfMark.img[i]%100));

            Mat tmpImg = new Mat(lheight, lwidth, MatType.CV_8U, tmpBytes);
            if (lheight < 128)
            {
                Mat BigImg = new Mat();
                double scale = 128.0 / lheight;
                Cv2.Resize(tmpImg, BigImg, new OpenCvSharp.Size((int)(tmpImg.Width * scale), (int)(tmpImg.Height * scale)), scale, scale, InterpolationFlags.Area);
                BigImg.CopyTo(tmpImg);
            }
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);
            pictureBox4.Image = myImage;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;

            for (int i = 0; i < lwidth * lheight; i++)
                tmpBytes[i] = (byte)(6 * (lfMark.img[i]/100));
            
            tmpImg = new Mat(lheight, lwidth, MatType.CV_8U, tmpBytes);
            if (lheight < 128)
            {
                Mat BigImg = new Mat();
                double scale = 128.0 / lheight;
                Cv2.Resize(tmpImg, BigImg, new OpenCvSharp.Size((int)(tmpImg.Width * scale), (int)(tmpImg.Height * scale)), scale, scale, InterpolationFlags.Area);
                BigImg.CopyTo(tmpImg);
            }
            Bitmap myImage2 = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);
            pictureBox1.Image = myImage2;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }
        public void CreateModelFromSubImg(byte[] srcData, int lwidth, int lheight, double lmodelScale, ref sFiducialMark lfMark, bool applyIgnoreRC, bool IsShift = false)
        {
            lfMark.modelSize.Width = lwidth;
            lfMark.modelSize.Height = lheight;
            lfMark.conv = 0;
            lfMark.planeShift = 0;
            lfMark.MScale = (int)lmodelScale;
            if (!IsShift)
                lfMark.img = new int[lwidth * lheight];

            int[] lAvg = new int[lwidth * lheight];
            int[] lSort = new int[lwidth * lheight];
            List<int> lbrSort = new List<int>();
            Rect ignoreRc = new Rect();
            if (applyIgnoreRC)
            {
                ignoreRc = new Rect((int)(mSearchIgnore.X / lmodelScale + 0.5), (int)(mSearchIgnore.Y / lmodelScale + 0.5), (int)(mSearchIgnore.Width / lmodelScale + 0.5), (int)(mSearchIgnore.Height / lmodelScale + 0.5));
                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        lAvg[x + y * lwidth] = srcData[x + y * lwidth];

                        if (ignoreRc.Contains(new OpenCvSharp.Point(x, y)))
                            continue;

                        lbrSort.Add(lAvg[x + y * lwidth]);
                    }
            }
            else
            {
                ignoreRc = lfMark.exArea;
                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        lAvg[x + y * lwidth] = srcData[x + y * lwidth];

                        if (ignoreRc.Contains(new OpenCvSharp.Point(x, y)))
                            continue;

                        lbrSort.Add(lAvg[x + y * lwidth]);
                    }
            }
            lfMark.exArea = ignoreRc;

            int[] brSort = lbrSort.ToArray();
            Array.Sort(brSort);
            int bin85pro = brSort.Length / 8;
            int bin50pro = brSort.Length / 2;
            //int bin4pro = (int)(brSort.Length * 0.96);
            int bin2pro = (int)(brSort.Length * 0.98);

            int lmin = brSort[bin85pro];
            int lmax = brSort[bin2pro];
            int lavg = brSort[bin50pro] - 1; //(lmin + lmax) / 2 - 2;

            if (lavg < lmin + 2)
                lavg = lmin + 2;

            lmax = brSort[bin2pro]; 

            for (int x = 0; x < lwidth; x++)
                for (int y = 0; y < lheight; y++)
                {
                    if (ignoreRc.Contains(new OpenCvSharp.Point(x, y)))
                    {
                        lSort[x + y * lwidth] = 0;
                    }
                    else
                    {
                        if (lAvg[x + y * lwidth] > lavg)
                            lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lmax - lavg);
                        else
                            lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lavg - lmin);

                        if (lSort[x + y * lwidth] < -20)
                            lSort[x + y * lwidth] = -20;

                        if (lSort[x + y * lwidth] > 20)
                            lSort[x + y * lwidth] = 20;
                    }
                }
            if ( !IsShift )
            {
                for (int i = 0; i < lwidth * lheight; i++)
                    lfMark.img[i] = lSort[i] + 20;      //  -20 ~ 20
            }
            else
            {
                for (int i = 0; i < lwidth * lheight; i++)
                    lfMark.img[i] += (lSort[i]+20)*100;   //  -2000 ~ 2000
            }

            //  Calculate Self Conv
            //if (applyIgnoreRC)
            lfMark.conv = (int)(0.70 * (int)mCalcConv(srcData, ref lfMark));
            //else
            //    lfMark.conv = (int)(0.9 * (int)mCalcConv(srcData, ref lfMark));
        }
        private void button8_Click(object sender, EventArgs e)
        {

            SaveModel();
        }

        public void SaveModel()
        {
            if (mLastFMIfile == "")
            {
                var folder = Path.GetFullPath(RootPath + "Training");

                string sFilePath = folder;

                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.DefaultExt = "xml";
                saveFile.InitialDirectory = sFilePath;
                saveFile.Filter = "XML(*.xml)|*.xml";
                if (saveFile.ShowDialog() == DialogResult.OK)
                    sFilePath = saveFile.FileName;
                else
                    return;

                mLastFMIfile = sFilePath;
                label6.Text = mLastFMIfile;
            }
            int i = 0;

            //  mLastFMIfile

            List<sFiducialMark> TopBk = mFidMarkTop.ToList();
            List<sFiducialMark> SideBk = mFidMarkSide.ToList();

            for (i = 0; i < mFidMarkSide.Count; i++)
                mSizeFidMarkSide[i] = new OpenCvSharp.Size(mFidMarkSide[i].modelSize.Width, mFidMarkSide[i].modelSize.Height);

            for (i = 0; i < mFidMarkTop.Count; i++)
                mSizeFidMarkTop[i] = new OpenCvSharp.Size(mFidMarkTop[i].modelSize.Width, mFidMarkTop[i].modelSize.Height);

            //  여기서 modelSize 가 제멋대로 0 으로 변경됨.
            if (mCurrentScell.X >= 0 && mCurrentScell.Y >= 0)
            {
                i = mCurrentScell.Y;
                dgvDesignNModelSide.Rows[i].Cells[6].Value = mFidMarkSide[i].conv.ToString();
            }
            else if ( mCurrentTcell.X >= 0 && mCurrentTcell.Y >= 0)
            {
                i = mCurrentTcell.Y;
                dgvDesignNModelTop.Rows[i].Cells[6].Value = mFidMarkTop[i].conv.ToString();
            }

            //  그래서 modelSize 를 백업했다가 다시 저장.
            for (i = 0; i < mFidMarkSide.Count; i++)
            {
                mFidMarkSide[i].modelSize.Width = mSizeFidMarkSide[i].Width;
                mFidMarkSide[i].modelSize.Height = mSizeFidMarkSide[i].Height;
            }

            for (i = 0; i < mFidMarkTop.Count; i++)
            {
                mFidMarkTop[i].modelSize.Width = mSizeFidMarkTop[i].Width;
                mFidMarkTop[i].modelSize.Height = mSizeFidMarkTop[i].Height;
            }


            SaveFiducialMark(mLastFMIfile);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            mSearchModelIndex = 0;
            int len = mDetectedmark.Length;
            InitialzeDataSet(len);
        }

        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                richTextBox1.Text = "";

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 1)
                ShowFocusedMark(dataGridView1.CurrentCell.RowIndex);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (tbTargetConv.Text.Length > 0)
            {
                //string lstr = "";
                int i = 0;
                if (mCurrentScell.X >= 0)
                {
                    i = mCurrentScell.Y;
                    OpenCvSharp.Size lmSize = new OpenCvSharp.Size(mFidMarkSide[i].modelSize.Width, mFidMarkSide[i].modelSize.Height);
                    if (tbTargetConv.Text.Length > 0)
                        mFidMarkSide[i].conv = Convert.ToInt16(tbTargetConv.Text);
                    dgvDesignNModelSide.Rows[i].Cells[6].Value = tbTargetConv.Text;
                    //dgvDesignNModelSide.Rows[i].Cells[7].Value = tbSubConv.Text;
                    //dgvDesignNModelSide.Rows[i].Cells[8].Value = tbTargetSubConv.Text;

                    //  값이 변했는지 안변했는지 확인하기 위한 목적
                    mFidMarkSide[i].modelSize = lmSize;
                    lmSize = mFidMarkSide[i].modelSize;
                    //dgvDesignNModelSide.Rows[i].Cells[5].Value = sRoi.X.ToString() + "," + sRoi.Y.ToString() + "," + sRoi.Width.ToString() + "," + sRoi.Height.ToString();
                }
                else if ((mCurrentTcell.X >= 0))
                {
                    i = mCurrentTcell.Y;
                    OpenCvSharp.Size lmSize = new OpenCvSharp.Size(mFidMarkTop[i].modelSize.Width, mFidMarkTop[i].modelSize.Height);
                    
                    if (tbTargetConv.Text.Length > 0)
                        mFidMarkTop[i].conv = Convert.ToInt16(tbTargetConv.Text);

                    dgvDesignNModelTop.Rows[i].Cells[6].Value = tbTargetConv.Text;
                    //dgvDesignNModelTop.Rows[i].Cells[7].Value = tbSubConv.Text;
                    //dgvDesignNModelTop.Rows[i].Cells[8].Value = tbTargetSubConv.Text;

                    //  값이 변했는지 안변했는지 확인하기 위한 목적
                    mFidMarkTop[i].modelSize = lmSize;
                    lmSize = mFidMarkTop[i].modelSize;
                    //dgvDesignNModelTop.Rows[i].Cells[5].Value = sRoi.X.ToString() + "," + sRoi.Y.ToString() + "," + sRoi.Width.ToString() + "," + sRoi.Height.ToString();
                }

                //SaveModelFile();
                //SaveFiducialMark();
            }
        }

        private void btnMouseEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 911)
            {
                lbtn.BackgroundImage = Properties.Resources.AddP;
            }
            else if (lbtn.TabIndex == 912)
            {
                lbtn.BackgroundImage = Properties.Resources.SubP;
            }
            else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
                lbtn.BackgroundImage = Properties.Resources.BtnCP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;
        }
        private void btnMouseEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 911)
            {
                lbtn.BackgroundImage = Properties.Resources.AddP;
            }
            else if (lbtn.TabIndex == 912)
            {
                lbtn.BackgroundImage = Properties.Resources.SubP;
            }
            else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
                lbtn.BackgroundImage = Properties.Resources.BtnCP;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKP;

        }
        private void btnMouseHover(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 911)
            {
                lbtn.BackgroundImage = Properties.Resources.AddN;
            }
            else if (lbtn.TabIndex == 912)
            {
                lbtn.BackgroundImage = Properties.Resources.SubN;
            }
            else if ((lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
                lbtn.BackgroundImage = Properties.Resources.BtnCN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        private void btnMouseHover(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 911)
            {
                lbtn.BackgroundImage = Properties.Resources.AddN;
            }
            else if (lbtn.TabIndex == 912)
            {
                lbtn.BackgroundImage = Properties.Resources.SubN;
            }
            else if ( (lbtn.TabIndex >= 188 && lbtn.TabIndex <= 191) || lbtn.Text.Contains("Finish"))
                lbtn.BackgroundImage = Properties.Resources.BtnCN;
            else
                lbtn.BackgroundImage = Properties.Resources.BtnKN;

        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void UpdatedgvDesignNModelSide()
        {
            for (int i = 0; i < mMarkCount; i++)
            {
                dgvDesignNModelSide.Rows[i].Cells[0].Value = i.ToString();
                dgvDesignNModelSide.Rows[i].Cells[1].Value = mDetectedmark[i].rect.X.ToString("F0");
                dgvDesignNModelSide.Rows[i].Cells[2].Value = mDetectedmark[i].rect.Y.ToString("F0");
                dgvDesignNModelSide.Rows[i].Cells[3].Value = mDetectedmark[i].conv.ToString("F0");
                dgvDesignNModelSide.Rows[i].Cells[4].Value = "G";
            }
        }

        private void InitdgvDesignNModelSide()
        {
            int i = 0;

            dgvDesignNModelSide.AllowUserToAddRows = true;
            dgvDesignNModelSide.ColumnHeadersHeight = 22;
            dgvDesignNModelSide.DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
            dgvDesignNModelSide.RowsDefaultCellStyle.Font = new Font("Calibri", 9);
            dgvDesignNModelSide.AllowUserToResizeRows = false;
            dgvDesignNModelSide.AllowUserToResizeColumns = false;
            dgvDesignNModelSide.RowHeadersVisible = false;
            dgvDesignNModelSide.Font = new Font("Calibri", 10, FontStyle.Bold);
            dgvDesignNModelSide.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            dgvDesignNModelSide.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);

            //dgvDesignNModelSide.BackgroundColor = System.Drawing.Color.FromArgb(8, 8, 8);
            dgvDesignNModelSide.BackgroundColor = System.Drawing.Color.FromArgb(96, 96, 100);
            dgvDesignNModelSide.ForeColor = System.Drawing.Color.FromArgb(208, 208, 212);
            dgvDesignNModelSide.ScrollBars = ScrollBars.Vertical;
            dgvDesignNModelSide.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDesignNModelSide.EnableHeadersVisualStyles = false;
            dgvDesignNModelSide.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(newComboEventHanlderSide);

            // Column
            dgvDesignNModelSide.Columns.Add("No.", "No.");
            dgvDesignNModelSide.Columns[0].Width = 30;

            //dgvDesignNModelSide.Columns[1].Name = "Azimuth";
            DataGridViewComboBoxColumn cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "Azimuth";
            cbc.Width = 70;
            cbc.Name = "Azimuth";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelSide.Columns.Add(cbc);

            //dgvDesignNModelSide.Columns[2].Name = "X pos Type";
            cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "X pos Type";
            cbc.Width = 180;
            cbc.Name = "X pos Type";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelSide.Columns.Add(cbc);

            //dgvDesignNModelSide.Columns[3].Name = "Y pos Type";
            cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "Y pos Typ";
            cbc.Width = 180;
            cbc.Name = "Y pos Typ";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelSide.Columns.Add(cbc);

            dgvDesignNModelSide.Columns.Add("Model Size", "Model Size");
            dgvDesignNModelSide.Columns.Add("Search ROI", "Search ROI");
            dgvDesignNModelSide.Columns.Add("Conv"      , "Conv"      );
            dgvDesignNModelSide.Columns.Add("Shift"     , "Shift"       );
            dgvDesignNModelSide.Columns.Add("Height"   , "Height"   );

            dgvDesignNModelSide.Columns[4].Width = 90;
            dgvDesignNModelSide.Columns[5].Width = 110;
            dgvDesignNModelSide.Columns[6].Width = 50;
            dgvDesignNModelSide.Columns[7].Width = 50;
            dgvDesignNModelSide.Columns[8].Width = 70;

            for (i = 0; i < this.dgvDesignNModelTop.ColumnCount; i++)
            {
                dgvDesignNModelSide.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvDesignNModelSide.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                dgvDesignNModelSide.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            //dgvDesignNModelSide.ReadOnly = true;
        }
        private void CleardgvDesignNModelSide()
        {
            //for (int i = 0; i < 8; i++)
            //{
            //    dgvDesignNModelSide.Rows[i].Cells[0].Value = i.ToString();
            //    dgvDesignNModelSide.Rows[i].Cells[1].Value = "0";
            //    dgvDesignNModelSide.Rows[i].Cells[2].Value = "0";
            //    dgvDesignNModelSide.Rows[i].Cells[3].Value = "0";
            //    dgvDesignNModelSide.Rows[i].Cells[4].Value = "-";
            //    dgvDesignNModelSide.Rows[i].Cells[5].Value = "-";
            //    dgvDesignNModelSide.Rows[i].Cells[6].Value = "-";
            //    dgvDesignNModelSide.Rows[i].Cells[7].Value = "-";
            //    dgvDesignNModelSide.Rows[i].Cells[8].Value = "-";
            //}
            //mMarkCount = 0;
        }
        private void UpdatedgvDesignNModelTop()
        {
            //for (int i = 0; i < mMarkCount; i++)
            //{
            //    dgvDesignNModelTop.Rows[i].Cells[0].Value = i.ToString();
            //    dgvDesignNModelTop.Rows[i].Cells[1].Value = mDetectedmark[i].rect.X.ToString("F0");
            //    dgvDesignNModelTop.Rows[i].Cells[2].Value = mDetectedmark[i].rect.Y.ToString("F0");
            //    dgvDesignNModelTop.Rows[i].Cells[3].Value = mDetectedmark[i].conv.ToString("F0");
            //    dgvDesignNModelTop.Rows[i].Cells[4].Value = "G";
            //}
        }

        private void InitdgvDesignNModelTop()
        {
            int i = 0;
            dgvDesignNModelTop.AllowUserToAddRows = true;
            dgvDesignNModelTop.ColumnHeadersHeight = 22;
            dgvDesignNModelTop.DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
            dgvDesignNModelTop.RowsDefaultCellStyle.Font = new Font("Calibri", 9);
            dgvDesignNModelTop.AllowUserToResizeRows = false;
            dgvDesignNModelTop.AllowUserToResizeColumns = false;
            dgvDesignNModelTop.RowHeadersVisible = false;
            dgvDesignNModelTop.Font = new Font("Calibri", 10, FontStyle.Bold);
            dgvDesignNModelTop.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            dgvDesignNModelTop.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            //dgvDesignNModelTop.BackgroundColor = System.Drawing.Color.FromArgb(8, 8, 8);
            dgvDesignNModelTop.BackgroundColor = System.Drawing.Color.FromArgb(96, 96, 100);
            dgvDesignNModelTop.ForeColor = System.Drawing.Color.FromArgb(208, 208, 212);
            dgvDesignNModelTop.ScrollBars = ScrollBars.Vertical;
            dgvDesignNModelTop.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvDesignNModelTop.EnableHeadersVisualStyles = false;
            dgvDesignNModelTop.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(newComboEventHanlderTop);



            // Column
            dgvDesignNModelTop.Columns.Add("No.", "No.");
            dgvDesignNModelTop.Columns[0].Width = 30;

            //dgvDesignNModelTop.Columns[1].Name = "Azimuth";
            DataGridViewComboBoxColumn cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "Azimuth";
            cbc.Width = 70;
            cbc.Name = "Azimuth";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelTop.Columns.Add(cbc);

            //dgvDesignNModelTop.Columns[2].Name = "X pos Type";
            cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "X pos Type";
            cbc.Width = 180;
            cbc.Name = "X pos Type";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelTop.Columns.Add(cbc);

            //dgvDesignNModelTop.Columns[3].Name = "Y pos Type";
            cbc = new DataGridViewComboBoxColumn();
            cbc.HeaderText = "Y pos Typ";
            cbc.Width = 180;
            cbc.Name = "Y pos Typ";
            cbc.DefaultCellStyle.BackColor = Color.White;
            cbc.DefaultCellStyle.ForeColor = Color.Black;
            cbc.FlatStyle = FlatStyle.Flat;
            dgvDesignNModelTop.Columns.Add(cbc);

            dgvDesignNModelTop.Columns.Add("Model Size", "Model Size");
            dgvDesignNModelTop.Columns.Add("Search ROI", "Search ROI");
            dgvDesignNModelTop.Columns.Add("Conv"      , "Conv"      );
            dgvDesignNModelTop.Columns.Add("Shift"       , "Shift");
            dgvDesignNModelTop.Columns.Add("Height"   , "Height");
                           
            dgvDesignNModelTop.Columns[4].Width = 90;
            dgvDesignNModelTop.Columns[5].Width = 110;
            dgvDesignNModelTop.Columns[6].Width = 50;
            dgvDesignNModelTop.Columns[7].Width = 50;
            dgvDesignNModelTop.Columns[8].Width = 70;

            for (i = 0; i < this.dgvDesignNModelTop.ColumnCount; i++)
            {
                dgvDesignNModelTop.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvDesignNModelTop.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                dgvDesignNModelTop.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //this.dgvDesignNModel.Columns[i].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
                //this.dgvDesignNModel.Columns[i].DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(218, 218, 218);
            }

            //dgvDesignNModelTop.ReadOnly = true;
        }

        private void CleardgvDesignNModelTop()
        {
            for (int i = 0; i < 4; i++)
            {
                dgvDesignNModelTop.Rows[i].Cells[0].Value = i.ToString();
                dgvDesignNModelTop.Rows[i].Cells[1].Value = "0";
                dgvDesignNModelTop.Rows[i].Cells[2].Value = "0";
                dgvDesignNModelTop.Rows[i].Cells[3].Value = "0";
                dgvDesignNModelTop.Rows[i].Cells[4].Value = "-";
                dgvDesignNModelTop.Rows[i].Cells[5].Value = "-";
                dgvDesignNModelTop.Rows[i].Cells[6].Value = "-";
                dgvDesignNModelTop.Rows[i].Cells[7].Value = "-";
                dgvDesignNModelTop.Rows[i].Cells[8].Value = "-";
            }
            mMarkCount = 0;
        }

        public System.Drawing.Point mCurrentScell = new System.Drawing.Point();
        public System.Drawing.Point mCurrentTcell = new System.Drawing.Point();
        public List<System.Drawing.Point> mCurrentSList = new List<System.Drawing.Point>();
        public List<System.Drawing.Point> mCurrentTList = new List<System.Drawing.Point>();
        private void dgvDesignNModel_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //  Cell 을 클릭하면 해줘야 할 일
            mCurrentScell = new System.Drawing.Point(e.ColumnIndex, e.RowIndex);
            mCurrentTcell = new System.Drawing.Point(-1, -1);

            if (e.ColumnIndex == 4)
            {
                IsSeardchROIUpdate = false;
                IsModelSizeUpdate = true;
            }
            else if (e.ColumnIndex == 5)
            {
                IsSeardchROIUpdate = true;
                IsModelSizeUpdate = false;
            }
            DesignModelSideChanged(sender, 0);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            panel1.Location = new System.Drawing.Point(8, 312);
            panel1.Show();
            panel1Shown = true;

        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (!panel1Shown)
                return;

            ApplyFiducialMarkInfo(false);
            if (File.Exists(RootPath + "LastDesignInfo.png"))
            {
                Mat tmpImg = new Mat(RootPath + "LastDesignInfo.png", ImreadModes.Grayscale);
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);

                pictureBox3.Image = myImage;
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            }
            panel1.Hide();
            panel1Shown = false;

        }

        private void button20_Click(object sender, EventArgs e)
        {
            mbConfirmed = true;
            motSimDlg.Hide();
            this.Hide();
        }

        private void pictureBox6_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox6_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (File.Exists(RootPath + "LastDesignInfo.png"))
            {
                Mat tmpImg = new Mat(RootPath + "LastDesignInfo.png", ImreadModes.Grayscale);
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);

                pictureBox3.Image = myImage;
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            }
            panel1.Hide();
            panel1Shown = false;
        }

        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            bool res = false;
            mCurCsmIndex = -1;
            for ( int i=0; i< mCsmSide.Count; i++ )
            {
                if (mCsmSide[i] == null)
                    continue;
                res = mCsmSide[i].StartPoint(pictureBox6, e);
                if (res)
                {
                    mCurCsmIndex = i;
                    pictureBox6.Invalidate();
                    return;
                }
            }
            for (int i = 0; i < mCsmTop.Count; i++)
            {
                if (mCsmTop[i] == null)
                    continue;
                res = mCsmTop[i].StartPoint(pictureBox6, e);
                if (res)
                {
                    mCurCsmIndex = i + 8;
                    pictureBox6.Invalidate();
                    return;
                }
            }
            pictureBox6.Invalidate();
        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {

        }

        private void pictureBox6_MouseMove(object sender, MouseEventArgs e)
        {
            
            for( int i=0; i< mCsmSide.Count; i++)
            {
                if (mCsmSide[i] == null)
                    continue;

                if ( mCsmSide[i].TrackRubberBand(pictureBox6, e) )
                {
                    //rtbLog.Text += "S-Mark " + i.ToString() + "moving\r\n";
                    //rtbLog.SelectionStart = rtbLog.Text.Length;
                    //rtbLog.ScrollToCaret();
                    return;
                }
            }
            for (int i = 0; i < mCsmTop.Count; i++)
            {
                if (mCsmTop[i] == null)
                    continue;
                if ( mCsmTop[i].TrackRubberBand(pictureBox6, e))
                {
                    //rtbLog.Text += "T-Mark " + i.ToString() + "moving\r\n";
                    //rtbLog.SelectionStart = rtbLog.Text.Length;
                    //rtbLog.ScrollToCaret();
                    return;
                }
            }
        }

        private void pictureBox6_MouseUp(object sender, MouseEventArgs e)
        {
            if (mCurCsmIndex < 0 )
                return;


            if (mCurCsmIndex < 8)
            {
                if (mCsmSide.Count < 1)
                    return;
                if (mCsmSide[mCurCsmIndex] != null)
                {
                    bool res = mCsmSide[mCurCsmIndex].EndPoint(pictureBox6, e);
                    if (res)
                    {
                        int signOfY = 1;
                        int signOfX = 1;
                        int xt = (mCsmSide[mCurCsmIndex].X + pictureBox6.Width / 2) / 2 - 26;
                        int yt = (mCsmSide[mCurCsmIndex].Y - 20);
                        if (mCsmSide[mCurCsmIndex].Y > pictureBox6.Height / 2)
                        {
                            yt = mCsmSide[mCurCsmIndex].Y + 15;
                            signOfY = -1;
                        }

                        mMarkerPosInputS[mCurCsmIndex][0].Location = new System.Drawing.Point(xt,yt);
                        int xs = (mCsmSide[mCurCsmIndex].X + 15);
                        
                        if (mCsmSide[mCurCsmIndex].X < pictureBox6.Width / 2)
                        {
                            xs = (mCsmSide[mCurCsmIndex].X - 55);
                            signOfX = -1;
                        }

                        int ys = (mCsmSide[mCurCsmIndex].Y + pictureBox6.Height / 2) / 2 - 8;
                        mMarkerPosInputS[mCurCsmIndex][1].Location = new System.Drawing.Point(xs, ys);
                        mMarkerPosInputS[mCurCsmIndex][0].Show();
                        mMarkerPosInputS[mCurCsmIndex][1].Show();
                        //mFidMarkSide[mCurCsmIndex].fidInfo.TBXpos = mMarkerPosInputS[mCurCsmIndex][0].Location;
                        //mFidMarkSide[mCurCsmIndex].fidInfo.TBYpos = mMarkerPosInputS[mCurCsmIndex][1].Location;
                        //mFidMarkSide[mCurCsmIndex].fidInfo.markPos = new System.Drawing.Point(mCsmSide[mCurCsmIndex].X, mCsmSide[mCurCsmIndex].Y);
                        mFidMarkSide[mCurCsmIndex].fidInfo.TBXposX = mMarkerPosInputS[mCurCsmIndex][0].Location.X;
                        mFidMarkSide[mCurCsmIndex].fidInfo.TBXposY = mMarkerPosInputS[mCurCsmIndex][0].Location.Y;
                        mFidMarkSide[mCurCsmIndex].fidInfo.TBYposX = mMarkerPosInputS[mCurCsmIndex][1].Location.X;
                        mFidMarkSide[mCurCsmIndex].fidInfo.TBYposY = mMarkerPosInputS[mCurCsmIndex][1].Location.Y;
                        mFidMarkSide[mCurCsmIndex].fidInfo.markPosX = mCsmSide[mCurCsmIndex].X;
                        mFidMarkSide[mCurCsmIndex].fidInfo.markPosY = mCsmSide[mCurCsmIndex].Y;
                        mFidMarkSide[mCurCsmIndex].fidInfo.signX = signOfX;
                        mFidMarkSide[mCurCsmIndex].fidInfo.signY = signOfY;
                    }
                    else
                    {
                        mMarkerPosInputS[mCurCsmIndex][0].Hide();
                        mMarkerPosInputS[mCurCsmIndex][1].Hide();
                    }
                }
                //rtbLog.Text += "S-Mark " + mCurCsmIndex.ToString() + "Stop\r\n";
                //rtbLog.SelectionStart = rtbLog.Text.Length;
                //rtbLog.ScrollToCaret();
                //  mFidMark 에 반영해야 한다.
            }
            else
            {
                if (mCsmTop.Count < 1)
                    return;
                if (mCsmTop[mCurCsmIndex - 8] != null)
                {
                    bool res = mCsmTop[mCurCsmIndex - 8].EndPoint(pictureBox6, e);
                    int signOfY = 1;
                    int signOfX = 1;
                    if (res)
                    {
                        int xt = (mCsmTop[mCurCsmIndex - 8].X + pictureBox6.Width / 2) / 2 - 26;
                        int yt = (mCsmTop[mCurCsmIndex - 8].Y - 20);
                        if (mCsmTop[mCurCsmIndex - 8].Y > pictureBox6.Height / 2)
                        {
                            signOfY = -1;
                            yt = mCsmTop[mCurCsmIndex - 8].Y + 15;
                        }
                        mMarkerPosInputT[mCurCsmIndex - 8][0].Location = new System.Drawing.Point(xt, yt);
                        int xs = (mCsmTop[mCurCsmIndex - 8].X + 15);
                        if (mCsmTop[mCurCsmIndex - 8].X < pictureBox6.Width / 2)
                        {
                            signOfX = -1;
                            xs = (mCsmTop[mCurCsmIndex - 8].X - 55);
                        }
                        int ys = (mCsmTop[mCurCsmIndex - 8].Y + pictureBox6.Height / 2) / 2 - 8;
                        mMarkerPosInputT[mCurCsmIndex - 8][1].Location = new System.Drawing.Point(xs, ys);
                        //mMarkerPosInputT[mCurCsmIndex - 8][0].ReadOnly = true;
                        //mMarkerPosInputT[mCurCsmIndex - 8][1].ReadOnly = true;
                        //mMarkerPosInputT[mCurCsmIndex - 8][0].BackColor = Color.FromArgb(64,64,64);
                        //mMarkerPosInputT[mCurCsmIndex - 8][1].BackColor = Color.FromArgb(64, 64, 64);
                        mMarkerPosInputT[mCurCsmIndex - 8][0].Show();
                        mMarkerPosInputT[mCurCsmIndex - 8][1].Show();
                        //mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBXpos = mMarkerPosInputT[mCurCsmIndex - 8][0].Location;
                        //mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBYpos = mMarkerPosInputT[mCurCsmIndex - 8][1].Location;
                        //mFidMarkTop[mCurCsmIndex - 8].fidInfo.markPos = new System.Drawing.Point(mCsmTop[mCurCsmIndex - 8].X, mCsmTop[mCurCsmIndex - 8].Y);
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBXposX = mMarkerPosInputT[mCurCsmIndex - 8][0].Location.X;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBXposY = mMarkerPosInputT[mCurCsmIndex - 8][0].Location.Y;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBYposX = mMarkerPosInputT[mCurCsmIndex - 8][1].Location.X;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.TBYposY = mMarkerPosInputT[mCurCsmIndex - 8][1].Location.Y;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.markPosX = mCsmTop[mCurCsmIndex - 8].X;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.markPosY = mCsmTop[mCurCsmIndex - 8].Y;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.signX = signOfX;
                        mFidMarkTop[mCurCsmIndex - 8].fidInfo.signY = signOfY;
                    }
                    else
                    {
                        mMarkerPosInputT[mCurCsmIndex - 8][0].Hide();
                        mMarkerPosInputT[mCurCsmIndex - 8][1].Hide();
                    }
                }
                //  mFidMark 에 반영해야 한다.
                //rtbLog.Text += "T-Mark " + (mCurCsmIndex-8).ToString() + "Stop\r\n";
                //rtbLog.SelectionStart = rtbLog.Text.Length;
                //rtbLog.ScrollToCaret();
            }
            pictureBox6.Invalidate();
        }

        private void pictureBox6_Paint(object sender, PaintEventArgs e)
        {
            //if (mCurCsmIndex < 0)
            //    return;

            //if (mCsmSide[mCurCsmIndex] == null)
            //    return;

            for ( int i=0; i< mCsmSide.Count; i++)
            {
                if (mCsmSide[i] == null)
                    continue;
                mCsmSide[i].DrawCOGmarker(pictureBox6, e);
            }

            for ( int i=0; i< mCsmTop.Count; i++)
            {
                if (mCsmTop[i] == null)
                    continue;
                mCsmTop[i].DrawCOGmarker(pictureBox6, e);
            }
        }
        public void btnMouseClick(object sender, EventArgs e)
        {
            // lbtn.BackgroundImage 
            Button lbtn = (Button)sender;
            string lstr = lbtn.Text;
            //if (lstr.Contains("Open") || lstr.Contains("Save") || lstr.Contains("Setting") || lstr.Contains("Test"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
            //else if (lstr.Contains("Adm") || lstr.Contains("Ope") || lstr.Contains("Ope") || lstr.Contains("Mea"))
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
            //else if (lstr.Length > 0)
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
            //else if (lbtn.TabIndex / 10 == 90)
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
            //else if (lbtn.TabIndex / 10 == 91)
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
            //else if (lbtn.TabIndex / 10 == 92)
            //    lbtn.BackgroundImage = Properties.Resources.BtnKN;
        }

        public string[] mFidAzimuthSide = new string[8]
        {
            "North 1", "North 2", "West 1", "West 2", "South 1", "South 2", "East 1", "East 2"
        };

        public string[] mFidAzimuthTop = new string[4]
        {
            "North 1", "North 2", "South 1", "South 2"
        };

        public string[] mYPosType = new string[5]
        {
            "H edge in Upper/Lower Half", "H edge in Upper Half", "H edge in Lower Half", "Shape", "None"
        };

        public string[] mXPosType = new string[5]
        {
            "V edge in Left/Right Half", "V edge in Left Half", "V edge in Right Half", "Shape", "None"
        };

        public bool[] mFidSideCreated = new bool[8] { false, false, false, false, false, false, false, false };
        public bool[] mFidTopCreated = new bool[4] { false, false, false, false };

        private void btnAddMarkerSide_Click(object sender, EventArgs e)
        {
            btnMouseClick(sender, e);
            //  Create GAObject instance
            if (!mbSchematicPicture)
            {
                MessageBox.Show("Schematic Picture must be preloaded.");
                return;
            }
            AddMarkerSide();
        }
        public void AddMarkerSide()
        {
            if (dgvDesignNModelSide.Rows.Count == 9)
                return;

            Color[] inpusBGColor = new Color[12]
            {
                Color.FromArgb(36,68,145),
                Color.FromArgb(20,52,145),

                Color.FromArgb(8,125,8),
                Color.FromArgb(0,98,0),

                Color.FromArgb(125, 46, 46),
                Color.FromArgb(110, 0, 0),

                Color.FromArgb(125, 8, 125),
                Color.FromArgb(98, 0, 88),

                Color.FromArgb(0,70,125),
                Color.FromArgb(0,38,102),

                Color.FromArgb(0, 0,0),
                Color.FromArgb(0, 0,0)
            };
            Color[] inpusFGColor = new Color[12]
            {
                Color.FromArgb(64,148,255),
                Color.FromArgb(48,120,255),

                Color.FromArgb(40,255,40),
                Color.FromArgb(16,228,16),

                Color.FromArgb(255, 80, 80),
                Color.FromArgb(240, 40, 40),

                Color.FromArgb(255, 48, 255),
                Color.FromArgb(228, 30, 228),

                Color.FromArgb(0,190,255),
                Color.FromArgb(0,160,232),

                Color.FromArgb(0, 0,0),
                Color.FromArgb(0, 0,0)
            };
            sFiducialMark lFmark = new sFiducialMark();
            mFidMarkSide.Add(lFmark);
            lFmark.ID = mFidMarkSide.Count + 1;
            lFmark.fidInfo.schematicFile = mLastSchematicFile;
            //sFiducialInfo lFinfo = new sFiducialInfo();   //  Azimuth 선택 시 필요한 구문
            //mFidInfo.Add(lFinfo);
            //lFmark.fidInfo = lFinfo;

            DataGridViewRow row = new DataGridViewRow();//dgvObject.Rows[dgvObject.Rows.Count - 1];
            row.CreateCells(dgvDesignNModelSide);
            row.Height = 20;
            row.DefaultCellStyle.BackColor = Color.FromArgb(8, 8, 8);
            row.DefaultCellStyle.ForeColor = Color.FromArgb(228, 228, 228);
            row.Cells[0].Value = dgvDesignNModelSide.RowCount.ToString();
            row.Cells[4].ReadOnly = true;
            row.Cells[5].ReadOnly = true;

            DataGridViewComboBoxCell cbCell = (row.Cells[1] as DataGridViewComboBoxCell);   //  Azimuth

            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mFidAzimuthSide.Length; i++)
                if (!mFidSideCreated[i])
                {
                    cbCell.Items.Add(mFidAzimuthSide[i]);
                    //mFidSideCreated[i] = true;    //   최초에 생성할때는 모두 다 포함시킨다. 하나를 선택하면 나머지 Row 에서는 배제되어야 한다.
                }
            cbCell.Items.Add("-");
            cbCell.Value = "-";
            lFmark.Azimuth = ConvertToAzimuth(cbCell.Value.ToString(), true);

            cbCell = (row.Cells[2] as DataGridViewComboBoxCell);   //  X pos Type
            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mXPosType.Length; i++)
                cbCell.Items.Add(mXPosType[i]);

            cbCell.Value = cbCell.Items[0];
            lFmark.xPosType = ConvertToPosType(cbCell.Value.ToString(), true);

            cbCell = (row.Cells[3] as DataGridViewComboBoxCell);   //  Y pos Type
            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mYPosType.Length; i++)
                cbCell.Items.Add(mYPosType[i]);
            cbCell.Value = cbCell.Items[0];
            lFmark.yPosType = ConvertToPosType(cbCell.Value.ToString(), false);

            dgvDesignNModelSide.Rows.Add(row);
            dgvDesignNModelSide.CurrentCell = dgvDesignNModelSide.Rows[dgvDesignNModelSide.Rows.Count - 2].Cells[0];

            if (mFidMarkSideF != null )
            {
                if (mFidMarkSideF.Count == 0)
                    return;

                int FMSindex = dgvDesignNModelSide.Rows.Count - 2;
                if (dgvDesignNModelSide.Rows.Count > FMSindex)
                {
                    cbCell = (dgvDesignNModelSide.Rows[FMSindex].Cells[1] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkSideF[FMSindex].Azimuth];

                    cbCell = (dgvDesignNModelSide.Rows[FMSindex].Cells[2] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkSideF[FMSindex].xPosType];

                    cbCell = (dgvDesignNModelSide.Rows[FMSindex].Cells[3] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkSideF[FMSindex].yPosType];

                    dgvDesignNModelSide.Rows[FMSindex].Cells[4].Value = mFidMarkSideF[FMSindex].modelSize.Width.ToString() + "," + mFidMarkSideF[FMSindex].modelSize.Height.ToString();
                    dgvDesignNModelSide.Rows[FMSindex].Cells[5].Value = mFidMarkSideF[FMSindex].searchRoi.X.ToString() + "," + mFidMarkSideF[FMSindex].searchRoi.Y.ToString() + "," + mFidMarkSideF[FMSindex].searchRoi.Width.ToString() + "," + mFidMarkSideF[FMSindex].searchRoi.Height.ToString();
                    dgvDesignNModelSide.Rows[FMSindex].Cells[6].Value = mFidMarkSideF[FMSindex].conv.ToString();
                    dgvDesignNModelSide.Rows[FMSindex].Cells[7].Value = mFidMarkSideF[FMSindex].planeShift.ToString();
                    dgvDesignNModelSide.Rows[FMSindex].Cells[8].Value = mFidMarkSideF[FMSindex].planeHeight.ToString();

                    if (mCsmSide.Count > FMSindex)
                    {
                        mCsmSide[FMSindex].Destroy();
                        mCsmSide.RemoveAt(FMSindex);
                        mMarkerPosInputS.RemoveAt(FMSindex);
                        pictureBox6.Controls.Remove(mMarkerPosInputS[FMSindex][0]);
                        pictureBox6.Controls.Remove(mMarkerPosInputS[FMSindex][1]);
                    }

                    CSMarker lcsMarkerS = new CSMarker();
                    lcsMarkerS.SetEffectveRect(mMarkerEffectiveRc.X, mMarkerEffectiveRc.Y, mMarkerEffectiveRc.Width, mMarkerEffectiveRc.Height);
                    lcsMarkerS.Create(mFidMarkSideF[FMSindex].Azimuth, mFidMarkSideF[FMSindex].fidInfo.markPosX, mFidMarkSideF[FMSindex].fidInfo.markPosY);  //  이것 만으로는 그림은 안그린다.
                    mCsmSide.Add(lcsMarkerS);

                    TextBox[] ltb = new TextBox[2];
                    for (int k = 0; k < 2; k++)
                    {
                        ltb[k] = new TextBox();
                        ltb[k].BorderStyle = BorderStyle.None;
                        ltb[k].ForeColor = inpusFGColor[lcsMarkerS.Type + k];// Color.FromArgb(228, 228, 228);
                        ltb[k].BackColor = inpusBGColor[lcsMarkerS.Type + k];
                        ltb[k].Font = new Font("Calibri", 9, FontStyle.Bold);
                        ltb[k].TextAlign = HorizontalAlignment.Center;
                        ltb[k].Size = new System.Drawing.Size(52, 20);
                        ltb[k].Hide();
                    }
                    ltb[0].Text = mFidMarkSideF[FMSindex].fidInfo.X.ToString();
                    ltb[1].Text = mFidMarkSideF[FMSindex].fidInfo.Y.ToString();
                    mMarkerPosInputS.Add(ltb);
                    int mkrIndex = mCsmSide.Count - 1;

                    pictureBox6.Controls.Add(mMarkerPosInputS[mkrIndex][0]);
                    pictureBox6.Controls.Add(mMarkerPosInputS[mkrIndex][1]);

                    bool res = mCsmSide[mkrIndex].EndPoint(pictureBox6);
                    if (res)
                    {
                        int xt = (mCsmSide[mkrIndex].X + pictureBox6.Width / 2) / 2 - 26;
                        int yt = (mCsmSide[mkrIndex].Y - 20);
                        if (mCsmSide[mkrIndex].Y > pictureBox6.Height / 2)
                            yt = mCsmSide[mkrIndex].Y + 12;
                        mMarkerPosInputS[mkrIndex][0].Location = new System.Drawing.Point(xt, yt);
                        int xs = (mCsmSide[mkrIndex].X + 12);
                        if (mCsmSide[mkrIndex].X < pictureBox6.Width / 2)
                            xs = (mCsmSide[mkrIndex].X - 55);
                        int ys = (mCsmSide[mkrIndex].Y + pictureBox6.Height / 2) / 2 - 8;
                        mMarkerPosInputS[mkrIndex][1].Location = new System.Drawing.Point(xs, ys);

                        mMarkerPosInputS[mkrIndex][0].Show();
                        mMarkerPosInputS[mkrIndex][1].Show();
                    }
                    mFidMarkSide[FMSindex] = mFidMarkSideF[FMSindex];
                }
            }
            //  마커를 생성하지 않는다.
            //  추후 Azimuth Type 을 선택하면 그 때 마커를 Azimuth 에 따라 생성한다.
            //  Azimuth 를 변경하는 경우 해당 마커의 type 을 변경해야 한다.
            //  이를 위해 lFmark 는 Mark ID 를 가지고 있도록 한다.
        }

        public void BackupFMI()
        {
            mFidMarkSideBk = new sFiducialMark[mFidMarkSide.Count];
            mFidMarkTopBk = new sFiducialMark[mFidMarkTop.Count];

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(sFiducialMark[]));
            MemoryStream lstream = new MemoryStream();

            sFiducialMark[] lfmk = mFidMarkSide.ToArray();
            writer.Serialize(lstream, lfmk);
            lstream.Position = 0;
            mFidMarkSideBk = (sFiducialMark[])writer.Deserialize(lstream);

            lstream = new MemoryStream();
            lfmk = mFidMarkTop.ToArray();
            writer.Serialize(lstream, lfmk);
            lstream.Position = 0;
            mFidMarkTopBk = (sFiducialMark[])writer.Deserialize(lstream);
        }
        public void RecoverFromBackupFMI()
        {
            if (mFidMarkSideBk == null)
                return;
            int count = 0;
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(sFiducialMark[]));
            while (count == 0)
            {
                MemoryStream lstream = new MemoryStream();

                writer.Serialize(lstream, mFidMarkSideBk);
                lstream.Position = 0;
                sFiducialMark[] lfmk = (sFiducialMark[])writer.Deserialize(lstream);
                mFidMarkSide.Clear();
                foreach (sFiducialMark fmk in lfmk)
                    mFidMarkSide.Add(fmk);

                count = mFidMarkSide.Count;
            }
            count = 0;
            while (count == 0)
            {
                MemoryStream lstream = new MemoryStream();
                writer = new System.Xml.Serialization.XmlSerializer(typeof(sFiducialMark[]));
                writer.Serialize(lstream, mFidMarkTopBk);
                lstream.Position = 0;
                sFiducialMark[] lfmk = (sFiducialMark[])writer.Deserialize(lstream);
                mFidMarkTop.Clear();
                foreach (sFiducialMark fmk in lfmk)
                    mFidMarkTop.Add(fmk);
                count = mFidMarkSide.Count;
            }
        }
        public int AddMarkerTop(int type)
        {
            //  Top Azimuth = 0,1,2,3  :: corresponds to Side 0,1,4,5
            int myType = type;
            if (type > 1)
                myType = type - 2;

            if (mFidMarkTop.Count > 0)
            {
                for (int i = 0; i < mFidMarkTop.Count; i++)
                    if (mFidMarkTop[i].Azimuth == myType)
                        return -1;
            }
            sFiducialMark lFmark = new sFiducialMark();
            mFidMarkTop.Add(lFmark);
            lFmark.ID = mFidMarkTop.Count + 101;

            //sFiducialInfo lFinfo = new sFiducialInfo();   //  Azimuth 선택 시 필요한 구문
            //mFidInfo.Add(lFinfo);
            //lFmark.fidInfo = lFinfo;

            DataGridViewRow row = new DataGridViewRow();//dgvObject.Rows[dgvObject.Rows.Count - 1];
            row.CreateCells(dgvDesignNModelTop);
            row.Height = 20;
            row.DefaultCellStyle.BackColor = Color.FromArgb(8, 8, 8);
            row.DefaultCellStyle.ForeColor = Color.FromArgb(228, 228, 228);
            row.Cells[0].Value = dgvDesignNModelTop.RowCount.ToString();
            row.Cells[4].ReadOnly = true;
            row.Cells[5].ReadOnly = true;

            DataGridViewComboBoxCell cbCell = (row.Cells[1] as DataGridViewComboBoxCell);   //  Azimuth

            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mFidAzimuthTop.Length; i++)
                if (!mFidTopCreated[i])
                {
                    cbCell.Items.Add(mFidAzimuthTop[i]);
                    //mFidTopCreated[i] = true;
                }
            cbCell.Value = cbCell.Items[myType];

            lFmark.Azimuth = myType;// 0,1,2,3 만 가능해야 한다.

            cbCell = (row.Cells[2] as DataGridViewComboBoxCell);   //  X pos Type
            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mXPosType.Length; i++)
                cbCell.Items.Add(mXPosType[i]);

            cbCell.Value = cbCell.Items[0];
            lFmark.xPosType = ConvertToPosType(cbCell.Value.ToString(), true);

            cbCell = (row.Cells[3] as DataGridViewComboBoxCell);   //  Y pos Type
            cbCell.FlatStyle = FlatStyle.Flat;
            cbCell.Style.BackColor = Color.FromArgb(8, 8, 8);
            cbCell.Style.ForeColor = Color.FromArgb(228, 228, 228);
            for (int i = 0; i < mYPosType.Length; i++)
                cbCell.Items.Add(mYPosType[i]);
            cbCell.Value = cbCell.Items[0];
            lFmark.yPosType = ConvertToPosType(cbCell.Value.ToString(), false);

            dgvDesignNModelTop.Rows.Add(row);
            dgvDesignNModelTop.CurrentCell = dgvDesignNModelTop.Rows[dgvDesignNModelTop.Rows.Count - 2].Cells[0];


            if (mFidMarkTopF != null)
            {
                if (mFidMarkTopF.Count == 0)
                    return myType;

                int FMSindex = dgvDesignNModelTop.Rows.Count - 2;
                if (dgvDesignNModelTop.Rows.Count > FMSindex)
                {
                    cbCell = (dgvDesignNModelTop.Rows[FMSindex].Cells[1] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkTopF[FMSindex].Azimuth];

                    cbCell = (dgvDesignNModelTop.Rows[FMSindex].Cells[2] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkTopF[FMSindex].xPosType];

                    cbCell = (dgvDesignNModelTop.Rows[FMSindex].Cells[3] as DataGridViewComboBoxCell);   //  Azimuth
                    cbCell.Value = cbCell.Items[mFidMarkTopF[FMSindex].yPosType];

                    dgvDesignNModelTop.Rows[FMSindex].Cells[4].Value = mFidMarkTopF[FMSindex].modelSize.Width.ToString() + "," + mFidMarkTopF[FMSindex].modelSize.Height.ToString();
                    dgvDesignNModelTop.Rows[FMSindex].Cells[5].Value = mFidMarkTopF[FMSindex].searchRoi.X.ToString() + "," + mFidMarkTopF[FMSindex].searchRoi.Y.ToString() + "," + mFidMarkTopF[FMSindex].searchRoi.Width.ToString() + "," + mFidMarkTopF[FMSindex].searchRoi.Height.ToString();
                    dgvDesignNModelTop.Rows[FMSindex].Cells[6].Value = mFidMarkTopF[FMSindex].conv.ToString();
                    dgvDesignNModelTop.Rows[FMSindex].Cells[7].Value = mFidMarkTopF[FMSindex].planeShift.ToString();
                    dgvDesignNModelTop.Rows[FMSindex].Cells[8].Value = mFidMarkTopF[FMSindex].planeHeight.ToString();

                    if (mCsmTop.Count > FMSindex)
                    {
                        mCsmTop[FMSindex].Destroy();
                        mCsmTop.RemoveAt(FMSindex);
                        mMarkerPosInputT.RemoveAt(FMSindex);
                        pictureBox6.Controls.Remove(mMarkerPosInputT[FMSindex][0]);
                        pictureBox6.Controls.Remove(mMarkerPosInputT[FMSindex][1]);
                    }

                    CSMarker lcsMarkerS = new CSMarker();
                    lcsMarkerS.SetEffectveRect(mMarkerEffectiveRc.X, mMarkerEffectiveRc.Y, mMarkerEffectiveRc.Width, mMarkerEffectiveRc.Height);
                    lcsMarkerS.Create(mFidMarkTopF[FMSindex].Azimuth + 8, mFidMarkTopF[FMSindex].fidInfo.markPosX, mFidMarkTopF[FMSindex].fidInfo.markPosY);  //  이것 만으로는 그림은 안그린다.
                    mCsmTop.Add(lcsMarkerS);

                    TextBox[] ltb = new TextBox[2];
                    for (int k = 0; k < 2; k++)
                    {
                        ltb[k] = new TextBox();
                        ltb[k].BorderStyle = BorderStyle.None;
                        ltb[k].ForeColor = lcsMarkerS.SelectRectPen.Color;//Color.FromArgb(228,228,228);
                        ltb[k].BackColor = Color.FromArgb(32, 32, 32);
                        ltb[k].ReadOnly = true;
                        ltb[k].Font = new Font("Calibri", 9, FontStyle.Bold);
                        ltb[k].TextAlign = HorizontalAlignment.Center;
                        ltb[k].Size = new System.Drawing.Size(52, 20);
                        ltb[k].Hide();
                    }
                    ltb[0].Text = mFidMarkTopF[FMSindex].fidInfo.X.ToString();
                    ltb[1].Text = mFidMarkTopF[FMSindex].fidInfo.Y.ToString();
                    mMarkerPosInputT.Add(ltb);
                    int mkrIndex = mCsmTop.Count - 1;
                    pictureBox6.Controls.Add(mMarkerPosInputT[mkrIndex][0]);
                    pictureBox6.Controls.Add(mMarkerPosInputT[mkrIndex][1]);

                    bool res = mCsmTop[mkrIndex].EndPoint(pictureBox6);
                    if (res)
                    {
                        int xt = (mCsmTop[mkrIndex].X + pictureBox6.Width / 2) / 2 - 26;
                        int yt = (mCsmTop[mkrIndex].Y - 20);
                        if (mCsmTop[mkrIndex].Y > pictureBox6.Height / 2)
                            yt = mCsmTop[mkrIndex].Y + 12;
                        mMarkerPosInputT[mkrIndex][0].Location = new System.Drawing.Point(xt, yt);
                        int xs = (mCsmTop[mkrIndex].X + 12);
                        if (mCsmTop[mkrIndex].X < pictureBox6.Width / 2)
                            xs = (mCsmTop[mkrIndex].X - 55);
                        int ys = (mCsmTop[mkrIndex].Y + pictureBox6.Height / 2) / 2 - 8;
                        mMarkerPosInputT[mkrIndex][1].Location = new System.Drawing.Point(xs, ys);

                        mMarkerPosInputT[mkrIndex][0].Show();
                        mMarkerPosInputT[mkrIndex][1].Show();
                    }

                    mFidMarkTop[FMSindex] = mFidMarkTopF[FMSindex];
                }
            }
            return myType;
        }

        public int ConvertToAzimuth(string lstr, bool IsSide)
        {
            if (IsSide)
            {
                for (int i = 0; i < mFidAzimuthSide.Length; i++)
                    if (lstr == mFidAzimuthSide[i])
                        return i;
            }
            else
            {
                for (int i = 0; i < mFidAzimuthTop.Length; i++)
                    if (lstr == mFidAzimuthTop[i])
                        return i;
            }
            return -1;
        }
        public int ConvertToPosType(string lstr, bool IsX)
        {
            if (IsX)
            {
                for (int i = 0; i < mXPosType.Length; i++)
                    if (lstr == mXPosType[i])
                        return i;
            }
            else
            {
                for (int i = 0; i < mYPosType.Length; i++)
                    if (lstr == mYPosType[i])
                        return i;
            }
            return 0;
        }

        private void btnDelObject_Click(object sender, EventArgs e)
        {
            btnMouseClick(sender, e);


            RemoveFidMark();
        }

        public void RemoveFidMark()
        {
            int index = dgvDesignNModelSide.CurrentRow.Index;

            if (mCsmSide.Count <= index)
            {
                if (dgvDesignNModelSide.Rows.Count > index + 1)
                    dgvDesignNModelSide.Rows.RemoveAt(index);

                return;
            }

            if (mCsmSide[index] == null)
            {
                try
                {
                    dgvDesignNModelSide.Rows.RemoveAt(index);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                for (int i = 0; i < mCsmTop.Count; i++)
                {
                    if (mCsmTop[i] == null)
                        continue;

                    if (mCsmSide[index].Type < 2)
                    {
                        if (mCsmTop[i].Type == mCsmSide[index].Type + 8)  //  0,1 -> 8,9
                        {
                            mCsmTop[i].Destroy();
                            mCsmTop.RemoveAt(i);
                            dgvDesignNModelTop.Rows.RemoveAt(i);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][0]);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][1]);
                            mMarkerPosInputT.RemoveAt(i);
                        }
                    }
                    else if (mCsmSide[index].Type == 4 || mCsmSide[index].Type == 5)
                    {
                        if (mCsmTop[i].Type == mCsmSide[index].Type + 6)  //  4,5 -> 10, 11
                        {
                            mCsmTop[i].Destroy();
                            mCsmTop.RemoveAt(i);
                            dgvDesignNModelTop.Rows.RemoveAt(i);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][0]);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][1]);
                            mMarkerPosInputT.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < mFidMarkTop.Count; i++)
                {

                    if (mCsmSide[index].Type < 2)
                    {
                        if (mFidMarkTop[i].Azimuth == mCsmSide[index].Type)
                        {
                            mFidMarkTop.RemoveAt(i);
                            if (mFidMarkTopF.Count > i)
                                mFidMarkTopF.RemoveAt(i);

                            break;
                        }
                    }
                    else if (mCsmSide[index].Type == 4 || mCsmSide[index].Type == 5)
                    {
                        //  mCsmSide[index].Type == 4, 5 -> mFidMarkTop[i].Azimuth==2, 3
                        if (mFidMarkTop[i].Azimuth == mCsmSide[index].Type - 2)
                        {
                            mFidMarkTop.RemoveAt(i);
                            if (mFidMarkTopF.Count > i)
                                mFidMarkTopF.RemoveAt(i);
                            break;
                        }
                    }
                }

                for (int i = 0; i < mFidMarkSide.Count; i++)
                    if (mFidMarkSide[i] != null)
                    {
                        if (mFidMarkSide[i].Azimuth == mCsmSide[index].Type)
                        {
                            mFidMarkSide.RemoveAt(i);
                            if (mFidMarkSideF.Count > i)
                                mFidMarkSideF.RemoveAt(i);
                            break;
                        }
                    }
                mCsmSide[index].Destroy();
                mCsmSide.RemoveAt(index);
                pictureBox6.Controls.Remove(mMarkerPosInputS[index][0]);
                pictureBox6.Controls.Remove(mMarkerPosInputS[index][1]);
                mMarkerPosInputS.RemoveAt(index);
                dgvDesignNModelSide.Rows.RemoveAt(index);
            }

            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mSchematicOverlayedImg);
            pictureBox6.Image = myImage;
            pictureBox6.Invalidate();
        }

        public double mOpticsTgtOffset = 0.0;     //  더미렌즈 중심이 Side View 중심으로부터 이격되어있는 경우 +Y 방향으로 떨어진 거리
        public double mOpticsTopViewOffset = 1.95;  //  Center of Side View 에 해당하는 Target 의 Point 가 Top View 영상의 중심에서 얼마나 떨어져있는가 ( Top View 기준 )
        public double mOpticsFWOffset = 0.0;        //  Side View Center 에서 Focus Window 경계까지 거리로서 영상 상단쪽이 +
        public double mOpticsPlaneHeight = 0.0;        //  Side View Center 에서 Focus Window 경계까지 거리로서 영상 상단쪽이 +

        private void button19_Click(object sender, EventArgs e)
        {
            btnMouseHover(sender, e);
            double tgtOffset = 0;
            double topviewOffset = 0;
            double focusWindowOffset = 0;
            if ( tbOpticsTgtOffset.Text.Length > 0 )
            {
                try
                {
                    tgtOffset = Double.Parse(tbOpticsTgtOffset.Text);
                }
                catch(Exception err)
                {
                    MessageBox.Show("Offset value is not correct.");
                    return;
                }
            }
            if (tbOpticsTopViewOffset.Text.Length > 0)
            {
                try
                {
                    topviewOffset = Double.Parse(tbOpticsTopViewOffset.Text);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Offset value is not correct.");
                    return;
                }
            }
            if (tbFocusWindowOffset.Text.Length > 0)
            {
                try
                {
                    focusWindowOffset = Double.Parse(tbFocusWindowOffset.Text);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Offset value is not correct.");
                    return;
                }
            }

            //  tbOpticsTgtOffset: Center of Side View -> Center of Dummy Lens 거리
            //  tbOpticsTopViewOffset : Center of Side View -> Center of Top View 거리
            //  tbFocusWindowOffset : Focus Translator Window 의 Side View 광축에 대한 Offset, + 방향이 좌측

            mOpticsTgtOffset        = motSimDlg.W;// tgtOffset;
            mOpticsTopViewOffset    = motSimDlg.Oh;// topviewOffset;
            mOpticsFWOffset         = motSimDlg.S;// focusWindowOffset;

            StreamWriter wr = new StreamWriter(RootPath + "OpticsConfig.txt");
            wr.WriteLine(mOpticsTgtOffset.ToString("F3"));
            wr.WriteLine(mOpticsTopViewOffset.ToString("F3"));
            wr.WriteLine(mOpticsFWOffset.ToString("F3"));
            wr.Close();

            SaveFiducialMark(mLastFMIfile);    //  
        }
        public void LoadOpticsConfig()
        {
            if (!File.Exists(RootPath + "OpticsConfig.txt"))
                return;

            StreamReader wr = new StreamReader(RootPath + "OpticsConfig.txt");
            string lstr = wr.ReadToEnd();
            wr.Close();

            string[] allLine = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            mOpticsTgtOffset = double.Parse(allLine[0]);
            mOpticsTopViewOffset = double.Parse(allLine[1]);
            if (allLine.Length>2)
                mOpticsFWOffset = double.Parse(allLine[2]);
        }

        public System.Drawing.Point[] mInitialMarker = new System.Drawing.Point[]
        {
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
            new System.Drawing.Point{ X=0,Y=0 },  new System.Drawing.Point{ X=0,Y=0},
        };
        public void SetInitialMarkerPosition(int w, int h)
        {
            //  North
            mInitialMarker[0] = new System.Drawing.Point { X = w-22, Y = h/2 + 5 };
            mInitialMarker[1] = new System.Drawing.Point { X = w-22, Y = h/2 + 25 };

            //  West
            mInitialMarker[2] = new System.Drawing.Point { X = w / 2 + 18, Y = 11 };
            mInitialMarker[3] = new System.Drawing.Point { X = w / 2 + 38, Y = 11 };

            //  South
            mInitialMarker[4] = new System.Drawing.Point { X = 13, Y = h/2 + 5  };
            mInitialMarker[5] = new System.Drawing.Point { X = 13, Y = h / 2 + 25 };

            //  East
            mInitialMarker[6] = new System.Drawing.Point { X = w / 2 + 18, Y = h - 21 };
            mInitialMarker[7] = new System.Drawing.Point { X = w / 2 + 38, Y = h - 21 };

            //  Top View
            mInitialMarker[8] = new System.Drawing.Point { X = w - 22, Y = 3 * h / 4 - 2};
            mInitialMarker[9] = new System.Drawing.Point { X = w - 22, Y = 3 * h / 4 + 18};

            mInitialMarker[10] = new System.Drawing.Point { X = 13, Y = 3 * h / 4 - 2};
            mInitialMarker[11] = new System.Drawing.Point { X = 13, Y = 3 * h / 4 + 18};
        }
        private void dgvDesignNModelSide_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            sFiducialMark lfMark = mFidMarkSide[dgvDesignNModelSide.CurrentRow.Index];
            DataGridViewComboBoxCell cbCell = (dgvDesignNModelSide.Rows[dgvDesignNModelSide.CurrentRow.Index].Cells[1] as DataGridViewComboBoxCell);
            //  Cell의 모든 값을 mFidInfo, mFidMark 에 Update 해준다
            if ( e.ColumnIndex < 4)
            {
                sFiducialInfo lfInfo = lfMark.fidInfo;
                lfMark.Azimuth = ConvertToAzimuth(cbCell.Value.ToString(), true);
                cbCell = (dgvDesignNModelSide.Rows[dgvDesignNModelSide.CurrentRow.Index].Cells[2] as DataGridViewComboBoxCell);
                lfMark.xPosType = ConvertToPosType(cbCell.Value.ToString(), true);
                cbCell = (dgvDesignNModelSide.Rows[dgvDesignNModelSide.CurrentRow.Index].Cells[3] as DataGridViewComboBoxCell);
                lfMark.yPosType = ConvertToPosType(cbCell.Value.ToString(), false);
            }
            //  Model Size Change Update
            else if (e.ColumnIndex == 4)
            {
                lfMark.modelSize = new OpenCvSharp.Size( mSearchModel[mSearchModelIndex].width, mSearchModel[mSearchModelIndex].height);
            }

            //  SearchROI Change Update
            //else if (e.ColumnIndex == 5)
            //{
            //    Rectangle lrc = mCst.GetSelectRect();
            //    lfMark.searchRoi = new Rect(lrc.X, lrc.Y, lrc.Width, lrc.Height);
            //}

            //  Conv value Change Update

            //  Sub value Change Update

            //  Subconv Value Change Update

            //  Maker 정보변동은 MouseMove Event 에 따라 ( Marker 의 움직임에 따라 ) 마커내부의 변동되는변수를 해당하는 mFidMark 에 반영해야 한다.
        }

        private void dgvDesignNModelTop_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //  Cell의 모든 값을 mFidInfo, mFidMark 에 Update 해준다
            //  Cell의 모든 값을 mFidInfo, mFidMark 에 Update 해준다
            sFiducialMark lfMark = mFidMarkTop[dgvDesignNModelTop.CurrentRow.Index];
            sFiducialInfo lfInfo = lfMark.fidInfo;
            DataGridViewComboBoxCell cbCell = (dgvDesignNModelTop.Rows[dgvDesignNModelTop.CurrentRow.Index].Cells[1] as DataGridViewComboBoxCell);

            lfMark.Azimuth = ConvertToAzimuth(cbCell.Value.ToString(), false);
            cbCell = (dgvDesignNModelTop.Rows[dgvDesignNModelTop.CurrentRow.Index].Cells[2] as DataGridViewComboBoxCell);
            lfMark.xPosType = ConvertToPosType(cbCell.Value.ToString(), true);
            cbCell = (dgvDesignNModelTop.Rows[dgvDesignNModelTop.CurrentRow.Index].Cells[3] as DataGridViewComboBoxCell);
            lfMark.yPosType = ConvertToPosType(cbCell.Value.ToString(), false);

            //  Azimuth Type Change

            //  Model Size Change Update
            if (IsModelSizeUpdate)
            {
                lfMark.modelSize = new OpenCvSharp.Size(mSearchModel[mSearchModelIndex].width, mSearchModel[mSearchModelIndex].height);
            }
            //  SearchROI Change Update
            //if (IsSeardchROIUpdate)
            //{
            //    Rectangle lrc = mCst.GetSelectRect();
            //    lfMark.searchRoi = new Rect(lrc.X, lrc.Y, lrc.Width, lrc.Height);
            //}

            //  Conv value Change Update

            //  Sub value Change Update

            //  Subconv Value Change Update

            //  Maker 정보변동은 MouseMove Event 에 따라 ( Marker 의 움직임에 따라 ) 마커내부의 변동되는변수를 해당하는 mFidMark 에 반영해야 한다.        }
        }


        private void newComboEventHanlderTop(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                DataGridView ldgv = sender as DataGridView;
                if (ldgv.CurrentCell.ColumnIndex != 1)
                    return;

                ComboBox cb = e.Control as ComboBox;
                if (cb != null)
                {
                    cb.SelectedIndexChanged -= new EventHandler(ChangeAzimuthTop);
                    cb.SelectedIndexChanged += new EventHandler(ChangeAzimuthTop);
                }
            }
            catch
            {
                ;
            }
        }
        private void ChangeAzimuthTop(object sender, EventArgs e)
        {
            //   Marker 굵게 다시그려주기만 하면 됨.
            pictureBox6.Invalidate();
        }
        private void newComboEventHanlderSide(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                DataGridView ldgv = sender as DataGridView;
                if (ldgv.CurrentCell.ColumnIndex != 1)
                    return;

                ComboBox cb = e.Control as ComboBox;
                if (cb != null)
                {
                    cb.SelectedIndexChanged -= new EventHandler(ChangeAzimuthSide);
                    cb.SelectedIndexChanged += new EventHandler(ChangeAzimuthSide);
                }
            }
            catch
            {
                ;
            }
        }
        private void ChangeAzimuthSide(object sender, EventArgs e)
        {
            //   Marker 생성 및  Text Box 생성
            //   Marker 가 없으면 생성 및  Text Box 생성
            int lCurCsmIndex = dgvDesignNModelSide.CurrentRow.Index;
            mCurCsmIndex = lCurCsmIndex;

            //string lstr = "";
            //lstr += "Current Row = " + lCurCsmIndex.ToString() + "\r\n";

            sFiducialMark lfMark = mFidMarkSide[dgvDesignNModelSide.CurrentRow.Index];

            ComboBox cb = (ComboBox)sender;
            lfMark.Azimuth = ConvertToAzimuth(cb.Text, true);
            if (lfMark.Azimuth < 0)
                return;

            Color[] inpusBGColor = new Color[12]
            {
                Color.FromArgb(46,78,155),
                Color.FromArgb(30,62,155),

                Color.FromArgb(8,145,8),
                Color.FromArgb(0,118,0),

                Color.FromArgb(145, 46, 46),
                Color.FromArgb(130, 0, 0),

                Color.FromArgb(145, 8, 145),
                Color.FromArgb(118, 0, 118),

                Color.FromArgb(0,80,145),
                Color.FromArgb(0,48,122),

                Color.FromArgb(0, 0,0),
                Color.FromArgb(0, 0,0)
            };
            Color[] inpusFGColor = new Color[12]
            {
                Color.FromArgb(64,148,255),
                Color.FromArgb(48,120,255),

                Color.FromArgb(40,255,40),
                Color.FromArgb(16,228,16),

                Color.FromArgb(255, 80, 80),
                Color.FromArgb(240, 40, 40),

                Color.FromArgb(255, 48, 255),
                Color.FromArgb(228, 30, 228),

                Color.FromArgb(0,190,255),
                Color.FromArgb(0,160,232),

                Color.FromArgb(0, 0,0),
                Color.FromArgb(0, 0,0)
            };
            if (mCsmSide.Count > lCurCsmIndex)
            {
                if (mCsmSide[lCurCsmIndex] != null)
                {
                    mCsmSide[lCurCsmIndex].Destroy();
                    mCsmSide.RemoveAt(lCurCsmIndex);
                    pictureBox6.Controls.Remove(mMarkerPosInputS[lCurCsmIndex][0]);
                    pictureBox6.Controls.Remove(mMarkerPosInputS[lCurCsmIndex][1]);
                    mMarkerPosInputS.RemoveAt(lCurCsmIndex);
                }
            }

            //lstr += "Side Azimuth = " + lfMark.Azimuth.ToString() + "\r\n";
            CSMarker lcsMarkerS = new CSMarker();
            lcsMarkerS.SetEffectveRect(mMarkerEffectiveRc.X, mMarkerEffectiveRc.Y, mMarkerEffectiveRc.Width, mMarkerEffectiveRc.Height);
            lcsMarkerS.Create(lfMark.Azimuth, mInitialMarker[lfMark.Azimuth].X, mInitialMarker[lfMark.Azimuth].Y);  //  이것 만으로는 그림은 안그린다.
            mCsmSide.Add(lcsMarkerS);

            TextBox[] ltb = new TextBox[2];
            for (int k = 0; k < 2; k++)
            {
                ltb[k] = new TextBox();
                ltb[k].BorderStyle = BorderStyle.None;
                ltb[k].ForeColor = inpusFGColor[lcsMarkerS.Type + k];// Color.FromArgb(228, 228, 228);
                ltb[k].BackColor = inpusBGColor[lcsMarkerS.Type + k];
                ltb[k].Font = new Font("Calibri", 9, FontStyle.Bold);
                ltb[k].TextAlign = HorizontalAlignment.Center;
                ltb[k].Size = new System.Drawing.Size(52, 20);
                ltb[k].Hide();
            }
            mMarkerPosInputS.Add(ltb);
            pictureBox6.Controls.Add(mMarkerPosInputS[mMarkerPosInputS.Count - 1][0]);
            pictureBox6.Controls.Add(mMarkerPosInputS[mMarkerPosInputS.Count - 1][1]);

            //  North1/2 또는 South1/2 인 경우 dgvDesignNModelTop 에 Row 생성, 이미 있으면 생성하지 않는다.
            if (lfMark.Azimuth < 2 || lfMark.Azimuth == 4 || lfMark.Azimuth == 5)
            {
                int lazimuth = AddMarkerTop(lfMark.Azimuth);//  0,1,2,3 이 반환된다.
                if (lazimuth >= 0)
                {
                    if (mCsmTop.Count > lazimuth)
                    {
                        for ( int ai = 0; ai < mCsmTop.Count; ai ++)
                        {
                            if (mCsmTop[ai] == null)
                                continue;
                            if ( mCsmTop[ai].Type == lazimuth + 8)  //  이미 마커가 생성되어 있는 경우
                            {
                                mCsmTop[ai].Destroy();
                                mCsmTop.RemoveAt(ai);
                                pictureBox6.Controls.Remove(mMarkerPosInputT[ai][0]);
                                pictureBox6.Controls.Remove(mMarkerPosInputT[ai][1]);
                                mMarkerPosInputT.RemoveAt(ai);
                            }
                        }
                    }
                    CSMarker lcsMarker = new CSMarker();
                    //lstr+= "Top Azimuth = " + lfMark.Azimuth.ToString() + "\r\n";
                    lcsMarker.Create(lazimuth + 8, mInitialMarker[lazimuth + 8].X, mInitialMarker[lazimuth + 8].Y);
                    mCsmTop.Add(lcsMarker);

                    ltb = new TextBox[2];
                    for ( int k=0; k<2; k++)
                    {
                        ltb[k] = new TextBox();
                        ltb[k].BorderStyle = BorderStyle.None;
                        ltb[k].ForeColor = lcsMarker.SelectRectPen.Color;//Color.FromArgb(228,228,228);
                        ltb[k].BackColor = Color.FromArgb(32, 32, 32);
                        ltb[k].ReadOnly = true;
                        ltb[k].Font = new Font("Calibri", 9, FontStyle.Bold);
                        ltb[k].TextAlign = HorizontalAlignment.Center;
                        ltb[k].Size = new System.Drawing.Size(52, 20);
                        ltb[k].Hide();
                    }
                    mMarkerPosInputT.Add(ltb);
                    pictureBox6.Controls.Add(mMarkerPosInputT[mMarkerPosInputT.Count - 1][0]);
                    pictureBox6.Controls.Add(mMarkerPosInputT[mMarkerPosInputT.Count - 1][1]);
                }
            }
            // 전체를 다시그린다.

            //rtbLog.Text += lstr;
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mSchematicOverlayedImg);
            pictureBox6.Image = myImage;

            pictureBox6.Invalidate();
        }
        public Rect mMarkerEffectiveRc = new Rect();
        public string mLastSchematicFile = "";
        private void button6_Click(object sender, EventArgs e)
        {
            btnMouseHover(sender, e);
            OpenSchematicFile();
        }

        public void OpenSchematicFile()
        {
            //  모델을 만들때 또는 모델의 좌표정보를 업데이트할 때 Schematic File 의 이름을 새롭게 변경한 뒤 열어야 한다.

            //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Schematic"));
            var folder = Path.GetFullPath("C:\\CSHTest\\DoNotTouch\\Schematic");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string sFilePath = folder;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Schematic File for Marks";
            openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            openFileDialog1.Multiselect = false;
            openFileDialog1.InitialDirectory = sFilePath;
            openFileDialog1.FilterIndex = 2;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //Image myImage = Image.FromFile(filename);
            mLastSchematicFile = openFileDialog1.FileName;  //  mLastSchematicFile 에 고유의 이름을 가지게할 필요가 있다.

            foreach (sFiducialMark mark in mFidMarkSideF)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkTopF)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkSide)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkTop)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            ShowSchematics();
        }

        public void ExternalUpdateSchmatics(string schematicFile)
        {
            mLastSchematicFile = schematicFile;

            foreach(sFiducialMark mark in mFidMarkSideF)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkTopF)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkSide)
                mark.fidInfo.schematicFile = mLastSchematicFile;

            foreach (sFiducialMark mark in mFidMarkTop)
                mark.fidInfo.schematicFile = mLastSchematicFile;

        }
        private void ShowSchematics()
        {
            if (!File.Exists(mLastSchematicFile))
                return;

            //Mat tmpImg = Cv2.ImRead(mLastSchematicFile);
            Mat tmpGray = new Mat(mLastSchematicFile, ImreadModes.Grayscale);

            //Mat tmpGray = new Mat();

            mSchematicOverlayedImg = new Mat();
            mSchematicImg = new Mat(tmpGray.Height + 100, tmpGray.Width + 100, MatType.CV_8UC1, Scalar.Black);

            //try
            //{
            //    Cv2.CvtColor(tmpImg, tmpGray, ColorConversionCodes.BGR2GRAY);
            //}
            //catch (Exception le)
            //{
            //    tmpImg.CopyTo(tmpGray);
            //}
            mMarkerEffectiveRc = new Rect(50, 50, tmpGray.Width, tmpGray.Height);

            Mat lSchmatic = mSchematicImg.SubMat(mMarkerEffectiveRc);
            tmpGray.CopyTo(lSchmatic);

            Cv2.CvtColor(mSchematicImg, mSchematicOverlayedImg, ColorConversionCodes.GRAY2RGB);

            int x = mSchematicOverlayedImg.Width;
            int y = mSchematicOverlayedImg.Height;


            Cv2.PutText(mSchematicOverlayedImg, "N", new OpenCvSharp.Point(x - 20, y / 2 - 8), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(32, 32, 255), 2);
            Cv2.PutText(mSchematicOverlayedImg, "W", new OpenCvSharp.Point(x / 2 - 8, 19), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(0, 255, 0), 2);
            Cv2.PutText(mSchematicOverlayedImg, "S", new OpenCvSharp.Point(8, y / 2 - 8), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(255, 32, 32), 2);
            Cv2.PutText(mSchematicOverlayedImg, "E", new OpenCvSharp.Point(x / 2 - 8, y - 7), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(255, 0, 255), 2);
            Cv2.PutText(mSchematicOverlayedImg, "tN", new OpenCvSharp.Point(x - 30, 3 * y / 4 - 14), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(32, 32, 255), 2);
            Cv2.PutText(mSchematicOverlayedImg, "tS", new OpenCvSharp.Point(8, 3 * y / 4 - 14), HersheyFonts.HersheyPlain, 1.2, Scalar.FromRgb(255, 32, 32), 2);
            Cv2.Line(mSchematicOverlayedImg, 50, y / 2, x - 50, y / 2, Scalar.White, 1, LineTypes.Link4);
            Cv2.Line(mSchematicOverlayedImg, x / 2, 50, x / 2, y - 50, Scalar.White, 1, LineTypes.Link4);

            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mSchematicOverlayedImg);

            pictureBox6.Image = myImage;
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            mbSchematicPicture = true;

            mMarkerEffectiveRc.X = (int)(mMarkerEffectiveRc.X * (double)pictureBox6.Width / (double)mSchematicImg.Width);
            mMarkerEffectiveRc.Y = (int)(mMarkerEffectiveRc.Y * (double)pictureBox6.Height / (double)mSchematicImg.Height);
            mMarkerEffectiveRc.Width = (int)(mMarkerEffectiveRc.Width * (double)pictureBox6.Width / (double)mSchematicImg.Width);
            mMarkerEffectiveRc.Height = (int)(mMarkerEffectiveRc.Height * (double)pictureBox6.Height / (double)mSchematicImg.Height);

            SetInitialMarkerPosition(pictureBox6.Width, pictureBox6.Height);
        }
        private void button21_Click(object sender, EventArgs e)
        {
            btnMouseHover(sender, e);
            ApplyFiducialMarkInfo(true);
        }

        public void ApplyFiducialMarkInfo(bool defaultRoi = true)
        {
            //  모든 TextBox 의 데이터를 각 변수에 저장한다.
            int countErr = 0;
            string lstr = "";
            string[] laz = new string[6] { "N", "W", "S", "E", "Nt", "St" };

            PointF pNorth = new PointF(0, 0);
            PointF pSouth = new PointF(0, 0);
            PointF pEast = new PointF(0, 0);
            PointF pSize = new PointF((float)0.45, (float)0.45);

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                if (mMarkerPosInputS[i][0].Text.Length > 0)
                    mFidMarkSide[i].fidInfo.X = double.Parse(mMarkerPosInputS[i][0].Text);  //  Side 0,1 -> Top 8(0),9(1),  Side 4,5 -> Top 10(2),11(3)
                else
                    countErr++;
                if (mMarkerPosInputS[i][1].Text.Length > 0)
                    mFidMarkSide[i].fidInfo.Y = double.Parse(mMarkerPosInputS[i][1].Text);
                else
                    countErr++;
                lstr += laz[mFidMarkSide[i].Azimuth / 2] + "\tx=" + mFidMarkSide[i].fidInfo.X.ToString("F3") + "\ty=" + mFidMarkSide[i].fidInfo.Y.ToString("F3") + "\r\n";

                if (mFidMarkSide[i].Azimuth / 2 == 0)
                {
                    pNorth.X = (float)mFidMarkSide[i].fidInfo.X;
                    pNorth.Y = (float)mFidMarkSide[i].fidInfo.Y;
                }else if (mFidMarkSide[i].Azimuth / 2 == 2)
                {
                    pSouth.X = (float)mFidMarkSide[i].fidInfo.X;
                    pSouth.Y = (float)mFidMarkSide[i].fidInfo.Y;
                }else if (mFidMarkSide[i].Azimuth / 2 == 3)
                {
                    pEast.X = (float)mFidMarkSide[i].fidInfo.X;
                    pEast.Y = (float)mFidMarkSide[i].fidInfo.Y;
                }
            }
            motSimDlg.ExternalSetData(pNorth, pSize, pSouth, pSize, pEast, pSize);

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                for (int j = 0; j < mFidMarkSide.Count; j++)
                {
                    if ((mFidMarkTop[i].Azimuth == 0 && mFidMarkSide[j].Azimuth == 0) || (mFidMarkTop[i].Azimuth == 1 && mFidMarkSide[j].Azimuth == 1))
                    {
                        //  North Mark 인 경우

                        if (mMarkerPosInputS[j][0].Text.Length > 0)
                            mFidMarkTop[i].fidInfo.X = double.Parse(mMarkerPosInputS[j][0].Text);   //  TOp 0,1,2,3 => Side 0,1,4,5
                        else
                            countErr++;

                        if (mMarkerPosInputS[j][1].Text.Length > 0)
                            mFidMarkTop[i].fidInfo.Y = double.Parse(mMarkerPosInputS[j][1].Text);
                        else
                            countErr++;

                        mMarkerPosInputT[i][0].Text = mMarkerPosInputS[j][0].Text;
                        mMarkerPosInputT[i][1].Text = mMarkerPosInputS[j][1].Text;
                        lstr += mFidMarkSide[j].Azimuth.ToString() + "-" + mFidMarkTop[i].Azimuth.ToString() + "\tx=" + mFidMarkTop[i].fidInfo.X.ToString("F3") + "\ty=" + mFidMarkTop[i].fidInfo.Y.ToString("F3") + "\r\n";
                    }
                    else if ((mFidMarkTop[i].Azimuth == 2 && mFidMarkSide[j].Azimuth == 4) || (mFidMarkTop[i].Azimuth == 3 && mFidMarkSide[j].Azimuth == 5))
                    {
                        //  South Mark 인 경우
                        //   Azimuth 를 확인하고 값을 할당해줘야 한다. 수정 필요
                        if (mMarkerPosInputS[j][0].Text.Length > 0)
                            mFidMarkTop[i].fidInfo.X = double.Parse(mMarkerPosInputS[j][0].Text);   //  TOp 0,1,2,3 => Side 0,1,4,5
                        else
                            countErr++;

                        if (mMarkerPosInputS[j][1].Text.Length > 0)
                            mFidMarkTop[i].fidInfo.Y = double.Parse(mMarkerPosInputS[j][1].Text);
                        else
                            countErr++;

                        mMarkerPosInputT[i][0].Text = mMarkerPosInputS[j][0].Text;
                        mMarkerPosInputT[i][1].Text = mMarkerPosInputS[j][1].Text;
                        lstr += mFidMarkSide[j].Azimuth.ToString() + "-" + mFidMarkTop[i].Azimuth.ToString() + "\tx=" + mFidMarkTop[i].fidInfo.X.ToString("F3") + "\ty=" + mFidMarkTop[i].fidInfo.Y.ToString("F3") + "\r\n";
                    }
                }
            }

            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////
            //
            //  Search ROI 영역 자동설정 : 마크의 영상좌표 확보
            
            System.Drawing.Point[] markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
            for ( int i=0; i< (mFidMarkSide.Count + mFidMarkTop.Count) ; i++)
            {
                markPos[i] = new System.Drawing.Point();
            }

            GetDefaultMarkPosOnPanel(out markPos);
            //  마크의 상대좌표계 영상좌표 변환
            for (int i = 0; i < (mFidMarkSide.Count + mFidMarkTop.Count); i++)
            {
                markPos[i].X += FOV_X / 2;
                markPos[i].Y += FOV_Y / 2;
            }
            SetMarkSearchROI(markPos, defaultRoi);

            //  Search ROI 영역 자동설정
            //
            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////

            rtbLog.Text += lstr;
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();

            if (countErr > 0)
            {
                rtbLog.Text += "\r\nEmpty position data!\r\n";
            }

            MakeDesignInfoToImage();
        }

        public void SetMarkSearchROI(System.Drawing.Point[] markPos, bool defaultRoi = true)
        {
            double um2pixel = 5.5 / LensMag;

            int mscale = mFidMarkSide[0].MScale;
            int roiWidth = 2000;
            int roiHeight = 2000;

            if (tbDefaultROIHeight.Text != "")
                roiHeight = int.Parse(tbDefaultROIHeight.Text);

            if (tbDefaultROIWidth.Text != "")
                roiWidth = int.Parse(tbDefaultROIWidth.Text);

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                if (defaultRoi)
                {
                    mFidMarkSide[i].searchRoi.X = markPos[i].X - (int)(7 * mscale + roiWidth / (2 * um2pixel));    //  7*3 + 1000/15.71
                    mFidMarkSide[i].searchRoi.Y = markPos[i].Y - (int)(5 * mscale + vSin40 * roiHeight / (2 * um2pixel));    //  5*3 + (1000/15.71)*sin40
                    mFidMarkSide[i].searchRoi.Width = (int)(roiWidth / (um2pixel));    //  7*3 + 1000/15.71
                    mFidMarkSide[i].searchRoi.Height = (int)(vSin40 * roiHeight / (um2pixel)) + 3;    //  5*3 + (1000/15.71)*sin40
                    if (mFidMarkSide[i].searchRoi.Y < 0)
                    {
                        mFidMarkSide[i].searchRoi.Height += mFidMarkSide[i].searchRoi.Y;    //  5*3 + (1000/15.71)*sin40
                        mFidMarkSide[i].searchRoi.Y = 0;
                    }
                    if (mFidMarkSide[i].searchRoi.X < 0)
                    {
                        mFidMarkSide[i].searchRoi.Width += mFidMarkSide[i].searchRoi.X;
                        mFidMarkSide[i].searchRoi.X = 0;
                    }
                }
                dgvDesignNModelSide.Rows[i].Cells[5].Value = mFidMarkSide[i].searchRoi.X.ToString() + "," + mFidMarkSide[i].searchRoi.Y.ToString() + "," + mFidMarkSide[i].searchRoi.Width.ToString() + "," + mFidMarkSide[i].searchRoi.Height.ToString();

            }
            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                if (defaultRoi)
                {
                    mFidMarkTop[i].searchRoi.X = markPos[mFidMarkSide.Count + i].X - (int)(7 * mscale + roiWidth / (2 * um2pixel));    //  7*3 + 1000/15.71
                    mFidMarkTop[i].searchRoi.Y = markPos[mFidMarkSide.Count + i].Y - (int)(7 * mscale + roiHeight / (2 * um2pixel));    //  7*3 + 1000/15.71
                    mFidMarkTop[i].searchRoi.Width = (int)(roiWidth / um2pixel);    //  7*3 + 1000/15.71
                    mFidMarkTop[i].searchRoi.Height = (int)(roiHeight / um2pixel) + 3;    //  5*3 + (1000/15.71)*sin40
                                                                                          //if (mFidMarkTop[i].searchRoi.Y < 0)
                                                                                          //{
                                                                                          //    mFidMarkTop[i].searchRoi.Height += mFidMarkTop[i].searchRoi.Y;    //  5*3 + (1000/15.71)*sin40
                                                                                          //    mFidMarkTop[i].searchRoi.Y = 0;
                                                                                          //}
                    if (mFidMarkTop[i].searchRoi.X < 0)
                    {
                        mFidMarkTop[i].searchRoi.Width += mFidMarkTop[i].searchRoi.X;    //  5*3 + (1000/15.71)*sin40
                        mFidMarkTop[i].searchRoi.X = 0;
                    }
                }
                dgvDesignNModelTop.Rows[i].Cells[5].Value = mFidMarkTop[i].searchRoi.X.ToString() + "," + mFidMarkTop[i].searchRoi.Y.ToString() + "," + mFidMarkTop[i].searchRoi.Width.ToString() + "," + mFidMarkTop[i].searchRoi.Height.ToString();
            }
        }


        private void dgvDesignNModelTop_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            mCurrentTcell = new System.Drawing.Point(e.ColumnIndex, e.RowIndex);
            mCurrentScell = new System.Drawing.Point(-1, -1);

            if (e.ColumnIndex == 4)
            {
                IsSeardchROIUpdate = false;
                IsModelSizeUpdate = true;
            }
            else if (e.ColumnIndex == 5)
            {
                IsSeardchROIUpdate = true;
                IsModelSizeUpdate = false;
            }
            DesignModelSideChanged(sender, 1);
        }

        private void MakeDesignInfoToImage()
        {
            try
            {
                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap(pictureBox6.Width, pictureBox6.Height, PixelFormat.Format32bppArgb);
                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);
                //Creating a Rectangle object which will
                //capture our Current Screen
                Rectangle captureRectangle = pictureBox6.Bounds;// Screen.AllScreens[0].Bounds;
                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(panel1.Location.X, panel1.Location.Y+25, 0, 0, captureRectangle.Size);
                //Saving the Image File (I am here Saving it in My E drive).
                captureBitmap.Save(RootPath + "LastDesignInfo.png", ImageFormat.Png);
                //Displaying the Successfull Result
                //MessageBox.Show("Screen Captured");
            }
            catch (Exception e)
            {
                //MessageBox.Show(ex.Message);
                ;
            }
        }

        private void pictureBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            panel1.Location = new System.Drawing.Point(8, 312);
            panel1.Show();
            panel1Shown = true;

        }

        private void btnCustomDown(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 917 || lbtn.TabIndex == 918)
            {
                lbtn.BackgroundImage = Properties.Resources.tVU;
            }
            else if (lbtn.TabIndex == 919 || lbtn.TabIndex == 920)
            {
                lbtn.BackgroundImage = Properties.Resources.tVD;
            }
            else if (lbtn.TabIndex == 913 || lbtn.TabIndex == 915)
            {
                lbtn.BackgroundImage = Properties.Resources.tHL;
            }
            else if (lbtn.TabIndex == 914 || lbtn.TabIndex == 916)
            {
                lbtn.BackgroundImage = Properties.Resources.tHR;
            }
        }

        private void btnCustomDown(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 917 || lbtn.TabIndex == 918)
            {
                lbtn.BackgroundImage = Properties.Resources.tVU;
            }
            else if (lbtn.TabIndex == 919 || lbtn.TabIndex == 920)
            {
                lbtn.BackgroundImage = Properties.Resources.tVD;
            }
            else if (lbtn.TabIndex == 913 || lbtn.TabIndex == 915)
            {
                lbtn.BackgroundImage = Properties.Resources.tHL;
            }
            else if (lbtn.TabIndex == 914 || lbtn.TabIndex == 916)
            {
                lbtn.BackgroundImage = Properties.Resources.tHR;
            }
        }

        private void btnCustomEnter(object sender, EventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 917 || lbtn.TabIndex == 918)
            {
                lbtn.BackgroundImage = Properties.Resources.tVUi;
            }
            else if (lbtn.TabIndex == 919 || lbtn.TabIndex == 920)
            {
                lbtn.BackgroundImage = Properties.Resources.tVDi;
            }
            else if (lbtn.TabIndex == 913 || lbtn.TabIndex == 915)
            {
                lbtn.BackgroundImage = Properties.Resources.tHLi;
            }
            else if (lbtn.TabIndex == 914 || lbtn.TabIndex == 916)
            {
                lbtn.BackgroundImage = Properties.Resources.tHRi;
            }
        }

        private void btnCustomEnter(object sender, MouseEventArgs e)
        {
            Button lbtn = (Button)sender;
            if (lbtn.TabIndex == 917 || lbtn.TabIndex == 918)
            {
                lbtn.BackgroundImage = Properties.Resources.tVUi;
            }
            else if (lbtn.TabIndex == 919 || lbtn.TabIndex == 920)
            {
                lbtn.BackgroundImage = Properties.Resources.tVDi;
            }
            else if (lbtn.TabIndex == 913 || lbtn.TabIndex == 915)
            {
                lbtn.BackgroundImage = Properties.Resources.tHLi;
            }
            else if (lbtn.TabIndex == 914 || lbtn.TabIndex == 916)
            {
                lbtn.BackgroundImage = Properties.Resources.tHRi;
            }
        }

        private void btnUU_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, -1, 0, 1);
                mSelectRect.Y--;
                mSelectRect.Height++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, -1, 0, 1);
                ResizeROIrect(delta);
            }
        }

        private void btnUD_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, +1, 0, -1);
                mSelectRect.Y++;
                mSelectRect.Height--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 1, 0, -1);
                ResizeROIrect(delta);
            }
        }

        private void btnDU_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, 0, 0, -1);
                mSelectRect.Height--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 0, 0, -1);
                ResizeROIrect(delta);
            }
        }

        private void btnDD_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, 0, 0, 1);
                mSelectRect.Height++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 0, 0, 1);
                ResizeROIrect(delta);
            }
        }

        private void btnLL_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, -1, 0, 1, 0);
                mSelectRect.X--;
                mSelectRect.Width++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(-1, 0, 1, 0);
                ResizeROIrect(delta);
            }
        }

        private void btnLR_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 1, 0, -1, 0);
                mSelectRect.X++;
                mSelectRect.Width--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(1, 0, -1, 0);
                ResizeROIrect(delta);
            }
        }

        private void btnRL_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, 0, -1, 0);
                mSelectRect.Width--;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 0, -1, 0);
                ResizeROIrect(delta);
            }
        }

        private void btnRR_Click(object sender, EventArgs e)
        {
            if (IsModelSizeUpdate)
            {
                mCst.TrackRubberBand(pictureBox2, 0, 0, 1, 0);
                mSelectRect.Width++;
                if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
                {
                    Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                    //	영상클립

                    mCustomImg = mSourceImg[0].SubMat(roi);
                    //	보여주기
                    RedrawROIImg();
                    Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                    pictureBox5.Image = myImage;
                    pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    UpdateMousePosition();
                }
            }
            else if (IsSeardchROIUpdate)
            {
                Rect delta = new Rect(0, 0, 1, 0);
                ResizeROIrect(delta);
            }
        }
        public string mLastFMIfile = "";

        public void SaveFiducialMark(string XMLfilename = "")
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(sFiducialMark[]));

            string path = null;

            if (XMLfilename !="")
                path = XMLfilename;    //  
            else
                path = mLastFMIfile;    //  

            DataGridViewComboBoxCell cbCell = null;

            //  Cell의 모든 값을 mFidInfo, mFidMark 에 Update 해준다
            for (int i=0; i< mFidMarkSide.Count; i++)
            {
                cbCell = (dgvDesignNModelSide.Rows[i].Cells[2] as DataGridViewComboBoxCell);
                mFidMarkSide[i].xPosType = ConvertToPosType(cbCell.Value.ToString(), true);
                cbCell = (dgvDesignNModelSide.Rows[i].Cells[3] as DataGridViewComboBoxCell);
                mFidMarkSide[i].yPosType = ConvertToPosType(cbCell.Value.ToString(), false);

                if ( mFidMarkSide[i].img == null )
                    continue;

                if (mFidMarkSide[i].img.Length != mFidMarkSide[i].modelSize.Width * mFidMarkSide[i].modelSize.Height)
                {
                    mFidMarkSide[i].modelSize.Width = mSizeFidMarkSide[i].Width;
                    mFidMarkSide[i].modelSize.Height = mSizeFidMarkSide[i].Height;
                }
            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                cbCell = (dgvDesignNModelTop.Rows[i].Cells[2] as DataGridViewComboBoxCell);
                mFidMarkTop[i].xPosType = ConvertToPosType(cbCell.Value.ToString(), true);
                cbCell = (dgvDesignNModelTop.Rows[i].Cells[3] as DataGridViewComboBoxCell);
                mFidMarkTop[i].yPosType = ConvertToPosType(cbCell.Value.ToString(), false);

                if (mFidMarkTop[i].img == null)
                    continue;

                if (mFidMarkTop[i].img.Length != mFidMarkTop[i].modelSize.Width * mFidMarkTop[i].modelSize.Height)
                {
                    mFidMarkTop[i].modelSize.Width = mSizeFidMarkTop[i].Width;
                    mFidMarkTop[i].modelSize.Height = mSizeFidMarkTop[i].Height;
                }
            }

            List<sFiducialMark> TopBk = mFidMarkTop.ToList();
            List<sFiducialMark> SideBk = mFidMarkSide.ToList();

            System.IO.FileStream file = System.IO.File.Create(path);
            List<sFiducialMark> allList = new List<sFiducialMark>();
            allList.AddRange(mFidMarkSide);
            allList.AddRange(mFidMarkTop);

            sFiducialMark[] lsFM = allList.ToArray();
            writer.Serialize(file, lsFM);
            file.Close();
            //MessageBox.Show("Saved");
            if (XMLfilename != "")
                mLastFMIfile = XMLfilename;

            StreamWriter wr = new StreamWriter(RootPath + "LastFMIfile.txt");
            wr.WriteLine(mLastFMIfile);
            wr.Close();
            //MessageBox.Show("SaveFiducialMark() " + mLastFMIfile);
        }

        public void SetMarkNorm(sFiducialMark[] lFidMarkSide)
        {
            FZMath.Point2D[] tmp = new FZMath.Point2D[8];
            foreach (sFiducialMark fm in lFidMarkSide)
            {
                if (fm != null)
                {
                    tmp[fm.Azimuth] = new FZMath.Point2D(fm.fidInfo.X, fm.fidInfo.Y);

                    tmp[fm.Azimuth].X = fm.fidInfo.signX * tmp[fm.Azimuth].X;
                    tmp[fm.Azimuth].Y = fm.fidInfo.signY * tmp[fm.Azimuth].Y;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                double mm2pixel = 1 / (0.0055 / LensMag);
                if (tmp[2 * i] != null && tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * (tmp[2 * i].X + tmp[2 * i + 1].X) / 2, mm2pixel * (tmp[2 * i].Y + tmp[2 * i + 1].Y) / 2);
                else if (tmp[2 * i] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i].X, mm2pixel * tmp[2 * i].Y);
                else if (tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i + 1].X, mm2pixel * tmp[2 * i + 1].Y);
                else
                    mMarkNorm[i] = null;

                //if (i == 2)
                //    mMarkNorm[i].X = - mMarkNorm[i].X;
                //if (i == 3)
                //    mMarkNorm[i].Y = - mMarkNorm[i].Y;
            }
        }
        public void SetDefaultMarkNorm()
        {
            sFiducialMark[] lFidMarkSide = mFidMarkSideF.ToArray();
            FZMath.Point2D[] tmp = new FZMath.Point2D[8];
            foreach (sFiducialMark fm in lFidMarkSide)
            {
                if (fm != null)
                {
                    tmp[fm.Azimuth] = new FZMath.Point2D(fm.fidInfo.X, fm.fidInfo.Y);

                    tmp[fm.Azimuth].X = fm.fidInfo.signX * tmp[fm.Azimuth].X;
                    tmp[fm.Azimuth].Y = fm.fidInfo.signY * tmp[fm.Azimuth].Y;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                double mm2pixel = 1 / (0.0055 / LensMag);
                if (tmp[2 * i] != null && tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * (tmp[2 * i].X + tmp[2 * i + 1].X) / 2, mm2pixel * (tmp[2 * i].Y + tmp[2 * i + 1].Y) / 2);
                else if (tmp[2 * i] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i].X, mm2pixel * tmp[2 * i].Y);
                else if (tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i + 1].X, mm2pixel * tmp[2 * i + 1].Y);
                else
                    mMarkNorm[i] = null;

                //if (i == 2)
                //    mMarkNorm[i].X = - mMarkNorm[i].X;
                //if (i == 3)
                //    mMarkNorm[i].Y = - mMarkNorm[i].Y;
            }
        }
        public void SetMarkNorm()
        {

            sFiducialMark[] lFidMarkSide = null;
            
            if (mCandidateIndex>0)
                lFidMarkSide = mFMSideCandidate[mCandidateIndex - 1].ToArray();
            else
                lFidMarkSide = mFidMarkSide.ToArray();

            FZMath.Point2D[] tmp = new FZMath.Point2D[8];
            foreach (sFiducialMark fm in lFidMarkSide)
            {
                if (fm != null)
                {
                    tmp[fm.Azimuth] = new FZMath.Point2D(fm.fidInfo.X, fm.fidInfo.Y);

                    tmp[fm.Azimuth].X = fm.fidInfo.signX * tmp[fm.Azimuth].X;
                    tmp[fm.Azimuth].Y = fm.fidInfo.signY * tmp[fm.Azimuth].Y;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                double mm2pixel = 1 / (0.0055 / LensMag);
                if (tmp[2 * i] != null && tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * (tmp[2 * i].X + tmp[2 * i + 1].X) / 2, mm2pixel * (tmp[2 * i].Y + tmp[2 * i + 1].Y) / 2);
                else if (tmp[2 * i] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i].X, mm2pixel * tmp[2 * i].Y);
                else if (tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(mm2pixel * tmp[2 * i + 1].X, mm2pixel * tmp[2 * i + 1].Y);
                else
                    mMarkNorm[i] = null;

                //if (i == 2)
                //    mMarkNorm[i].X = - mMarkNorm[i].X;
                //if (i == 3)
                //    mMarkNorm[i].Y = - mMarkNorm[i].Y;
            }
        }

        public sFiducialMark[] LoadFiducialMark(string XMLfilename)
        {
            // Now we can read the serialized book ...  
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(sFiducialMark[]));

            System.IO.StreamReader file = new System.IO.StreamReader(XMLfilename);
            sFiducialMark[] res = (sFiducialMark[])reader.Deserialize(file);
            file.Close();
            return res;
        }

        public bool mLastFMIisLoaded = false;

        public string GetLastFMI()
        {
            try
            {
                StreamReader rd = new StreamReader(RootPath + "LastFMIfile.txt");
                mLastFMIfile = rd.ReadLine();
                rd.Close();
            }catch(Exception e)
            {
                return "";
            }

            if (!File.Exists(mLastFMIfile))
                return "";

            return mLastFMIfile;
        }
        public bool LoadLastFMI()
        {
            if (!File.Exists(RootPath + "LastFMIfile.txt"))
                return false;

            StreamReader rd = new StreamReader(RootPath + "LastFMIfile.txt");
            string lstr = rd.ReadLine();
            rd.Close();

            mLastFMIfile = lstr;
            if (!File.Exists(mLastFMIfile))
                return false;

            sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            if (lfm.Length < 1)
                return false;

            mFidMarkSide.Clear();
            mFidMarkTop.Clear();
            //mFidMarkSide = new List<sFiducialMark>();
            //mFidMarkTop = new List<sFiducialMark>();

            if (mFidMarkSideF == null)
                mFidMarkSideF = new List<sFiducialMark>();
            else
                mFidMarkSideF.Clear();
            if (mFidMarkTopF == null)
                mFidMarkTopF = new List<sFiducialMark>();
            else
                mFidMarkTopF.Clear();

            //mFidMarkSideF = new List<sFiducialMark>();
            //mFidMarkTopF = new List<sFiducialMark>();
            for (int i = 0; i < lfm.Length; i++)
            {
                if (lfm[i].ID < 100)
                    mFidMarkSideF.Add(lfm[i]);
                else
                    mFidMarkTopF.Add(lfm[i]);
            }

            SetMarkNorm(mFidMarkSideF.ToArray());
            mLastFMIisLoaded = true;
            return true;
        }

        string[] mFMICanditates = null;
        public void SetFMICandidates(string[] flist)
        {
            if (flist == null)
                return;

            mFMICanditates = new string[flist.Length];
            for( int i=0; i< flist.Length; i++)
            {
                mFMICanditates[i] = flist[i];
            }
        }

        public int GetNumFMICandidate()
        {
            return mFMICanditates.Length;
        }
        private void btnLoadFMI_Click(object sender, EventArgs e)
        {
            var folder = Path.GetFullPath(RootPath + "Training");

            string sFilePath = folder;

            string sFileName = "";

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "xml";
            openFile.InitialDirectory = sFilePath;
            openFile.Filter = "XML(*.xml)|*.xml";
            if (openFile.ShowDialog() == DialogResult.OK)
                sFileName = openFile.FileName;
            else
                return ;

            mCurrentScell.X = 0;
            mCurrentScell.Y = 0;

            mLastFMIfile = sFileName;

            mModelScale = LoadFMIFile(mLastFMIfile);
            label6.Text = mLastFMIfile;

            //sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            //if (lfm.Length < 1)
            //    return;

            //label6.Text = mLastFMIfile;

            //StreamWriter wr = new StreamWriter(RootPath + "LastFMIfile.txt");
            //wr.WriteLine(mLastFMIfile);
            //wr.Close();

            //mFidMarkSide = new List<sFiducialMark>();
            //mFidMarkTop = new List<sFiducialMark>();

            //mFidMarkSideF = new List<sFiducialMark>();
            //mFidMarkTopF = new List<sFiducialMark>();
            //for (int i = 0; i < lfm.Length; i++)
            //{
            //    if (lfm[i].ID < 100)
            //        mFidMarkSideF.Add(lfm[i]);
            //    else
            //        mFidMarkTopF.Add(lfm[i]);
            //}

            //ShowMarkDGV();
        }
        public int ExternalLoadFMIFile(string sFile)
        {
            //  일단 최초에 로딩 된 뒤부터 유효하게 동작해야 한다.
            if (!mFAutoLearnLoaded)
                return 0;

            if (!mLastFMIisLoaded)
                return 0;

            mLastFMIfile = sFile;
            mModelScale = LoadFMIFile(mLastFMIfile);
            return mModelScale;
        }

        List<List<sFiducialMark>> mFMSideCandidate = new List<List<sFiducialMark>>();
        List<List<sFiducialMark>> mFMTopCandidate = new List<List<sFiducialMark>>();
        public void LoadFMICandidate()
        {
            if (mFMICanditates == null)
                return;

            mFMSideCandidate = new List<List<sFiducialMark>>();
            mFMTopCandidate = new List<List<sFiducialMark>>();
            bool isFirst = true;
            foreach ( string fname in mFMICanditates)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                if (!File.Exists(fname))
                    continue;
                
                sFiducialMark[] lfm = LoadFiducialMark(fname);
                if (lfm.Length < 1)
                    continue;

                List<sFiducialMark>  lFidMarkSide = new List<sFiducialMark>();
                List<sFiducialMark>  lFidMarkTop = new List<sFiducialMark>();

                for (int i = 0; i < lfm.Length; i++)
                {
                    if (lfm[i].ID < 100)
                        lFidMarkSide.Add(lfm[i]);
                    else
                        lFidMarkTop.Add(lfm[i]);
                }
                mFMSideCandidate.Add(lFidMarkSide);
                mFMTopCandidate.Add(lFidMarkTop);
            }
        }

        public int LoadFMIFile(string filename)
        {
            //MessageBox.Show("LoadFMIFile() 1 : " + mLastFMIfile);
            sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            if (lfm.Length < 1)
                return 0;


            StreamWriter wr = new StreamWriter(RootPath + "LastFMIfile.txt");
            //MessageBox.Show("LoadFMIFile() 2 : " + mLastFMIfile);
            wr.WriteLine(mLastFMIfile);
            wr.Close();

            mFidMarkSide = new List<sFiducialMark>();
            mFidMarkTop = new List<sFiducialMark>();

            mFidMarkSideF = new List<sFiducialMark>();
            mFidMarkTopF = new List<sFiducialMark>();
            for (int i = 0; i < lfm.Length; i++)
            {
                if (lfm[i].ID < 100)
                    mFidMarkSideF.Add(lfm[i]);
                else
                    mFidMarkTopF.Add(lfm[i]);
            }

            ShowMarkDGV();
            return mFidMarkSideF[0].MScale;
        }
        public void ShowMarkDGV()
        {
            //  Show Mark Information to the Data Grid View
            if (mFidMarkSideF.Count < 3 || mFidMarkTopF.Count < 2)
                return;

            mLastSchematicFile = mFidMarkSideF[0].fidInfo.schematicFile;
            lbModelScale.SelectedIndex = 8 - mFidMarkSideF[0].MScale;
            ShowSchematics();

            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////
            /////   Clear Previous Instances
            int delCount = mMarkerPosInputS.Count;
            for (int i = 0; i < delCount; i++)
            {
                pictureBox6.Controls.Remove(mMarkerPosInputS[i][0]);
                pictureBox6.Controls.Remove(mMarkerPosInputS[i][1]);
            }
            mMarkerPosInputS.RemoveRange(0, delCount);

            delCount = mMarkerPosInputT.Count;
            for (int i = 0; i < delCount; i++)
            {
                pictureBox6.Controls.Remove(mMarkerPosInputT[i][0]);
                pictureBox6.Controls.Remove(mMarkerPosInputT[i][1]);
            }
            mMarkerPosInputT.RemoveRange(0, delCount);

            dgvDesignNModelSide.Rows.Clear();
            dgvDesignNModelTop.Rows.Clear();

            if (mCsmSide != null)
                mCsmSide.Clear();
            else
                mCsmSide = new List<CSMarker>();

            if (mCsmTop != null)
                mCsmTop.Clear();
            else
                mCsmTop = new List<CSMarker>();

            if (mMarkerPosInputS != null)
                mMarkerPosInputS.Clear();
            else
                mMarkerPosInputS = new List<TextBox[]>();

            if (mMarkerPosInputT != null)
                mMarkerPosInputT.Clear();
            else
                mMarkerPosInputT = new List<TextBox[]>();

            /////   End of Clear Previous Instances
            ///////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////

            for (int i = 0; i < mFidMarkSideF.Count; i++)
            {
                AddMarkerSide();
            }
            for (int i = 0; i < mFidMarkTopF.Count; i++)
            {
                int azimuthParam = mFidMarkTopF[i].Azimuth;
                if (azimuthParam > 1)
                    azimuthParam += 2;

                AddMarkerTop(azimuthParam);
            }

            double value1 = (mFidMarkSide[0].fidInfo.Y + mFidMarkSide[1].fidInfo.Y + 2 * mFidMarkSide[2].fidInfo.Y) / (0.0055 / LensMag);
            double value2 = (2 * (mFidMarkSide[0].fidInfo.Y + mFidMarkSide[1].fidInfo.Y) - mFidMarkSide[2].fidInfo.Y) / (0.0055 / LensMag);
            mFZM.SetY1Y2_2Y3(value1, value2);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    label6.Text = mLastFMIfile;
                });
            }
            else
                label6.Text = mLastFMIfile;

            //  dgvDesignNModelSide update 해주기
            //  dgvDesignNModelTop update 해주기
            //  pictureBox6 에 그림 업데이트 해주기

            //  mFidMarkSideF 에 따라서 dgvDesignNModelSide 에 Add btn 클릭한 성황으로 진행시키고, Cell Value Change 도 Cell 순서대로 진행한다.
            //  marker position 에 mFidMarkSide 의 정보를 Update 해준 뒤 MouseUp 에서 수행하는 함수를 호출해준다.
            //  dgvDesignNModelSide Cell Value Change 상황에 따라 mFidMarkTop 가 생성됬을 것이므로, 
            //  mFidMarkTop 순서대로 mFidMarkTopF 의 정보를 Update 해주고, MouseUp 에서 수행하는 함수를 호출해준다.
        }
        private void button22_Click(object sender, EventArgs e)
        {
            var folder = Path.GetFullPath(RootPath + "Training");

            string sFilePath = folder;

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "xml";
            saveFile.InitialDirectory = sFilePath;
            saveFile.Filter = "XML(*.xml)|*.xml";
            if (saveFile.ShowDialog() == DialogResult.OK)
                sFilePath = saveFile.FileName;
            else
                return;


            mLastFMIfile = sFilePath;
            SaveFiducialMark(mLastFMIfile);    //  
            label6.Text = mLastFMIfile;

            SaveMarkPosOnPanel();
        }
        private void dgvDesignNModel_SelectionChanged(object sender, EventArgs e)
        {
            DesignModelSideChanged(sender, 0);
        }

        public sFiducialMark mCurFocusedMark = null;
        public void DesignModelSideChanged(object sender, int which)
        {
            //  DesignModel 선택이 바뀌면 해줘야 할 일
            //  해당 마크를 두껍게 표시해준다
            sFiducialMark lfMark = null;
            if ( which == 0 && mFidMarkSide.Count > 0)
            {
                mSearchModelIndex = dgvDesignNModelSide.CurrentCell.RowIndex;
                if (mSearchModelIndex >= mFidMarkSide.Count)
                {
                    if (mSearchModelIndex > 0)
                        mSearchModelIndex--;
                    else
                        return;
                }
                lfMark = mFidMarkSide[mSearchModelIndex];
            }
            else if ( mFidMarkTop.Count > 0)
            {
                mSearchModelIndex = dgvDesignNModelTop.CurrentCell.RowIndex;
                if (mSearchModelIndex >= mFidMarkTop.Count)
                {
                    if (mSearchModelIndex > 0)
                        mSearchModelIndex--;
                    else
                        return;
                }
                lfMark = mFidMarkTop[mSearchModelIndex];

                mSearchModelIndex += 8; //  Top 인 경우는 8 을 더해준다.
            }
            if (lfMark == null)
                return;

            int lWidth = lfMark.modelSize.Width;
            int lHeight = lfMark.modelSize.Height;

            if (lWidth == 0)
                return;

            byte[] tmpBytes = new byte[lWidth * lHeight];

            if (lfMark.img == null)
                return;

            if (lfMark.img.Length < lWidth * lHeight)
                return;

            int[] tmp2 = new int[lWidth * lHeight];
            Array.Copy(lfMark.img, tmp2, lWidth * lHeight);
            Array.Sort(tmp2);
            if (tmp2[0] < 0)
            {
                for (int i = 0; i < tmpBytes.Length; i++)
                    tmpBytes[i] = (byte)(6 * (lfMark.img[i] % 100) + 120);
            }else
            {
                for (int i = 0; i < tmpBytes.Length; i++)
                    tmpBytes[i] = (byte)(6 * (lfMark.img[i] % 100));
            }

            Mat tmpImg = new Mat(lHeight, lWidth, MatType.CV_8U, tmpBytes);
            if (lHeight < 128)
            {
                Mat BigImg = new Mat();
                double scale = 128.0 / lHeight;
                Cv2.Resize(tmpImg, BigImg, new OpenCvSharp.Size((int)(tmpImg.Width * scale), (int)(tmpImg.Height * scale)), scale, scale, InterpolationFlags.Area);
                BigImg.CopyTo(tmpImg);
            }
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);

            pictureBox1.Image = myImage;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            if (IsSeardchROIUpdate)
            {
                mCurFocusedMark = lfMark;
                OpenCvSharp.Rect roi = new Rect();
                roi.X = lfMark.searchRoi.X;
                roi.Y = lfMark.searchRoi.Y;
                //roi.Width = lfMark.searchRoi.Width + lfMark.modelSize.Width * lfMark.MScale;
                //roi.Height = lfMark.searchRoi.Height + lfMark.modelSize.Height * lfMark.MScale;
                roi.Width = lfMark.searchRoi.Width ;
                roi.Height = lfMark.searchRoi.Height ;
                DrawSearchROI(roi);
            }
        }
        private void dgvDesignNModelTop_SelectionChanged(object sender, EventArgs e)
        {
            DesignModelSideChanged(sender, 1);
        }

        public void WaitMOTSimConfirmed()
        {

            while(true)
            {
                Thread.Sleep(50);
                if (motSimDlg.mbConfirmed)
                    break;
            }
            motSimDlg.mbConfirmed = false;

            mOpticsTgtOffset        = motSimDlg.W;
            mOpticsTopViewOffset    = motSimDlg.Oh;
            mOpticsFWOffset         = motSimDlg.S * 1000;
            mOpticsPlaneHeight      = motSimDlg.planeHeight * 1000;

            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                if (mFidMarkSide[i].img == null)
                    continue;
                if (mFidMarkSide[i].img.Length == mFidMarkSide[i].modelSize.Width * mFidMarkSide[i].modelSize.Height)
                {
                    mFidMarkSide[i].planeShift = (int)mOpticsFWOffset;
                    mFidMarkSide[i].planeHeight = mOpticsPlaneHeight;
                }
            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                if (mFidMarkTop[i].img == null)
                    continue;

                if (mFidMarkTop[i].img.Length == mFidMarkTop[i].modelSize.Width * mFidMarkTop[i].modelSize.Height)
                {
                    mFidMarkTop[i].planeShift = (int)mOpticsFWOffset;
                    mFidMarkTop[i].planeHeight = mOpticsPlaneHeight;
                }
            }

            for (int i = 0; i < dgvDesignNModelSide.Rows.Count; i++)
            {
                dgvDesignNModelSide.Rows[i].Cells[7].Value = mOpticsFWOffset.ToString("0");
                dgvDesignNModelSide.Rows[i].Cells[8].Value = mOpticsPlaneHeight.ToString("0");
            }
            for (int i = 0; i < dgvDesignNModelTop.Rows.Count; i++)
            {
                dgvDesignNModelTop.Rows[i].Cells[7].Value = mOpticsFWOffset.ToString("0");
                dgvDesignNModelTop.Rows[i].Cells[8].Value = mOpticsPlaneHeight.ToString("0");
            }

            if (tbOpticsTgtOffset.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbOpticsTgtOffset.Text = mOpticsTgtOffset.ToString("F3");
                });
            else
                tbOpticsTgtOffset.Text      = mOpticsTgtOffset.ToString("F3");

            if (tbOpticsTopViewOffset.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbOpticsTopViewOffset.Text = mOpticsTopViewOffset.ToString("F3");
                });
            else
                tbOpticsTopViewOffset.Text = mOpticsTopViewOffset.ToString("F3");


            if (tbFocusWindowOffset.InvokeRequired)
                BeginInvoke((MethodInvoker)delegate
                {
                    tbFocusWindowOffset.Text = mOpticsFWOffset.ToString("F3");
                });
            else
                tbFocusWindowOffset.Text = mOpticsFWOffset.ToString("F3");
        }
        private void button9_Click_1(object sender, EventArgs e)
        {
            motSimDlg.mbConfirmed = false;
            motSimDlg.Show();
            //motSimDlg.ExternalSetData(1.79, 1.34, -2.3, -2.75);
            motSimDlg.BringToFront();

            Thread ThreadWaitMOTSimConfirmed = new Thread(() => WaitMOTSimConfirmed());
            ThreadWaitMOTSimConfirmed.Start();

            return;

            FZMath.Point2D[] pSide = new FZMath.Point2D[4];
            pSide[0] = new FZMath.Point2D(3,2.1);
            pSide[1] = new FZMath.Point2D(-3,2.1);
            pSide[2] = null;
            pSide[3] = new FZMath.Point2D(0,-1.9);
            FZMath.Point2D[] pSide1 = new FZMath.Point2D[4];
            pSide1[0] = new FZMath.Point2D(3, 2.05);
            pSide1[1] = new FZMath.Point2D(-3, 1.99);
            pSide1[2] = null;
            pSide1[3] = new FZMath.Point2D(0, -2.02);

            double[,] trM = null;
            double[,] wT = null;
            double[] ZTXTY = new double[3];
            double sec40 = 1 / Math.Sqrt( 1 - vSin40 * vSin40 );


            mFZM.ZPhiThetaTransferMatrix(pSide, out trM, out wT);   //  마크들의 명목좌표를 이용해 변환행렬을 미리 구해놓는다.

            //  입력값이 mm 단위면 mm. rad 로 결과 나오고
            //  입력값이 pixel 단위면, pixel, rad 로 결과 나온다.
            mFZM.CalcZPhiTheta(pSide1, pSide, trM, wT, ref ZTXTY);  //  pSide 는 초기상태, pSide1 는 현재 상태, Z 변위 및 tilt X, tilt Y 를 구한다.

            double dZ = ZTXTY[0] * sec40;   //  Pixel 입력시 pixel, mm 입력 시 mm
            double TX = ZTXTY[1] * sec40;   //  radian
            double TY = ZTXTY[2] * sec40;   //  radian
            //  여기까지 검증 완료


            double c12 = 0;
            double c13 = 0;
            //  GetC12C13fromP12P13 의 입력으로는 Nominal Value 를 적용해야 한다
            FZMath.Point2D[] pNom = new FZMath.Point2D[3];
            pNom[0] = new FZMath.Point2D(3, 2);
            pNom[1] = new FZMath.Point2D(-3, 2);
            pNom[2] = null;
            pNom[3] = new FZMath.Point2D(0, -2);

            mFZM.GetC12C13fromP12P13(pNom[0], pNom[1], pNom[3], ref c12, ref c13);
            //  여기까지 검증 완료

            pSide[0] = new FZMath.Point2D(3.2, 2.1 * vSin40);
            pSide[1] = new FZMath.Point2D(-2.8, 2.1 * vSin40);
            pSide[2] = null;
            pSide[3] = new FZMath.Point2D(0.2, -1.9 * vSin40);

            FZMath.Point2D pTop = new FZMath.Point2D();
            pTop = new FZMath.Point2D(3.2, 2.1);

            FZMath.Point2D orgSide = new FZMath.Point2D();
            FZMath.Point2D orgTop = new FZMath.Point2D();

            //  orgSide.X = 0.2, orgSide.Y = 0.1 s40
            //  orgTop.X = 0.2, orgTop.Y = 0.1
            mFZM.OrgByC12nC13(pSide[0], pSide[1], pSide[3], pTop, c12, c13, ref orgSide, ref orgTop);
            //  여기까지 검증 완료

            //  markS[][0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][1] : West , West  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][2] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][3] : East , East  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markT[][0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markT[][1] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.

        }

        public void Prepare6DMotion(int nSizeX=0, int nSizeY=0 )
        {
            if (mMarkNorm == null)
                return;
            if (mMarkNorm[2] == null || mMarkNorm[3] == null)
                return;

            mFZM.ZPhiThetaTransferMatrix(mMarkNorm, out mFZM.M6DM, out mFZM.M6DMwT);
            mFZM.GetC12C13fromP12P13(mMarkNorm[0], mMarkNorm[2], mMarkNorm[3], ref mFZM.mC12, ref mFZM.mC13);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            //mFZM.SaveTXTYZeroOffset(0, 0, 0, 0, 0, 0, 0);
            double txo = 0;
            double tyo = 0;
            double tzo = 0;
            mFZM.vSin40 = Math.Sin(40.0 / 180 * Math.PI);
            mFZM.vSec40 = 1 / Math.Cos(40.0 / 180 * Math.PI);
            mFZM.vCos40 = Math.Cos(40.0 / 180 * Math.PI);

            mFZM.GetTXTYOffset(ref txo, ref tyo, ref tzo);
            mFZM.SetTXTYOffset(0, 0, 0);
            mFZM.ResetScales();

            double psi = 0;
            double dZ = 0;
            double TX = 0;
            double TY = 0;
            int i = 0;

            FZMath.Point2D[][] markT = new FZMath.Point2D[20][];
            FZMath.Point2D[][] markS = new FZMath.Point2D[20][];
            FZMath.Point2D resT = new FZMath.Point2D();

            ////  Norminal 위치 : 물리적 위치이므로 Top View 좌표계 활용, 
            ////  Norminal 위치는 CIS 를 (0,0) 으로 하여 um 단위를 Pixel 단위로  변환해줘야 한다.

            //mMarkNorm[0] = new FZMath.Point2D(+300, +155.5723827);  //  pixel 단위로 입략되어야 함
            //mMarkNorm[1] = null;
            //mMarkNorm[2] = new FZMath.Point2D(-300, +155.5723827);  //  pixel 단위
            //mMarkNorm[3] = new FZMath.Point2D(+0, -155.5723827);  //  pixel 단위

            //  아래와 같은 값을 넣는게 맞는지( 0,0 을 중심으로 값의 정의했음) 확인 필요 -> 확인 완료. 맞음.  240103
            mMarkNorm[0] = new FZMath.Point2D(+300, +103.7149218);  //  pixel 단위로 입략되어야 함
            mMarkNorm[1] = null;
            mMarkNorm[2] = new FZMath.Point2D(-300, +103.7149218);  //  pixel 단위
            mMarkNorm[3] = new FZMath.Point2D(-100, -207.4298436);  //  pixel 단위

            for (i = 0; i < 20; i++)
            {
                markT[i] = new FZMath.Point2D[2];
                markS[i] = new FZMath.Point2D[4];
            }


            markT[0][0] = new FZMath.Point2D(); //  North
            markT[0][1] = new FZMath.Point2D(); //  South

            markT[1][0] = new FZMath.Point2D(); //  North
            markT[1][1] = new FZMath.Point2D(); //  South

            markS[0][0] = new FZMath.Point2D(); //  North
            markS[0][1] = null; //  West
            markS[0][2] = new FZMath.Point2D(); //  South
            markS[0][3] = new FZMath.Point2D(); //  East

            markS[1][0] = new FZMath.Point2D(); //  North
            markS[1][1] = null; //  West
            markS[1][2] = new FZMath.Point2D(); //  South
            markS[1][3] = new FZMath.Point2D(); //  East

            //  제자리
            markS[0][0] = new FZMath.Point2D(780, 80);    //  North
            markS[0][1] = null;                            //  West
            markS[0][2] = new FZMath.Point2D(180, 80);   //  South
            markS[0][3] = new FZMath.Point2D(380, 280);     //  East    왼쪽으로 Shift 된 상태

            markT[0][0] = new FZMath.Point2D(780, 280);    //  North
            markT[0][1] = new FZMath.Point2D(180, 280);     //  East

            markS[1][0] = new FZMath.Point2D(780, 80);    //  North
            markS[1][1] = null;                            //  West
            markS[1][2] = new FZMath.Point2D(180, 80);   //  South
            markS[1][3] = new FZMath.Point2D(380, 280);     //  East


            markT[1][0] = new FZMath.Point2D(780, 280);    //  North
            markT[1][1] = new FZMath.Point2D(180, 280);     //  East

            //  X 이동 0.7
            markS[2][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 - 100);    //  North
            markS[2][1] = null;                            //  West
            markS[2][2] = new FZMath.Point2D(480 - 300 + 0.7, 180 - 100);   //  South
            markS[2][3] = new FZMath.Point2D(480 - 100 + 0.7, 180 + 100);     //  East

            markT[2][0] = new FZMath.Point2D(480 + 300 + 0.7, 280);    //  North
            markT[2][1] = new FZMath.Point2D(480 - 300 + 0.7, 280);     //  East

            //  Y 이동 0.7
            markS[3][0] = new FZMath.Point2D(480 + 300, 180 - 100 + 0.44995);    //  North
            markS[3][1] = null;                            //  West
            markS[3][2] = new FZMath.Point2D(480 - 300, 180 - 100 + 0.44995);   //  South
            markS[3][3] = new FZMath.Point2D(480 - 100, 180 + 100 + 0.44995);     //  East

            markT[3][0] = new FZMath.Point2D(480 + 300, 280 + 0.7);    //  North
            markT[3][1] = new FZMath.Point2D(480 - 300, 280 + 0.7);     //  East

            //  Z 이동 0.7
            markS[4][0] = new FZMath.Point2D(480 + 300, 180 - 100 + 0.53623111);    //  North
            markS[4][1] = null;                            //  West
            markS[4][2] = new FZMath.Point2D(480 - 300, 180 - 100 + 0.53623111);   //  South
            markS[4][3] = new FZMath.Point2D(480 - 100, 180 + 100 + 0.53623111);     //  East

            markT[4][0] = new FZMath.Point2D(480 + 300, 280);    //  North
            markT[4][1] = new FZMath.Point2D(480 - 300, 280);     //  East

            //  TZ 회전 0.003rad
            markS[5][0] = new FZMath.Point2D(779.6875057, 79.42179202);    //  North
            markS[5][1] = null;                            //  West
            markS[5][2] = new FZMath.Point2D(179.6902057, 80.57880798);   //  South
            markS[5][3] = new FZMath.Point2D(380.6227386, 280.192236);     //  East

            markT[5][0] = new FZMath.Point2D(779.6875057, 279.1004681);    //  North
            markT[5][1] = new FZMath.Point2D(179.6902057, 280.9004654);     //  East

            //  TX 회전 0.003rad
            markS[6][0] = new FZMath.Point2D(780, 79.76194964);    //  North
            markS[6][1] = null;                            //  West
            markS[6][2] = new FZMath.Point2D(180, 79.76194964);   //  South
            markS[6][3] = new FZMath.Point2D(380, 280.4761007);     //  East

            markT[6][0] = new FZMath.Point2D(780, 280.0004667);    //  North
            markT[6][1] = new FZMath.Point2D(180, 280.0004667);     //  East

            //  TY 회전 0.003
            markS[7][0] = new FZMath.Point2D(779.99865, 79.31056103);    //  North
            markS[7][1] = null;                            //  West
            markS[7][2] = new FZMath.Point2D(180.00135, 80.68943896);   //  South
            markS[7][3] = new FZMath.Point2D(380.00045, 280.229813);     //  East

            markT[7][0] = new FZMath.Point2D(779.99865, 280);    //  North
            markT[7][1] = new FZMath.Point2D(180.00135, 280);     //  East

            //  X 0.7 + Y -0.7 + Z 0.7
            markS[8][0] = new FZMath.Point2D(780.7, 79.91372021);    //  North
            markS[8][1] = null;                            //  West
            markS[8][2] = new FZMath.Point2D(180.7, 79.91372021);   //  South
            markS[8][3] = new FZMath.Point2D(380.7, 279.9137202);     //  East

            markT[8][0] = new FZMath.Point2D(780.7, 280.7);    //  North
            markT[8][1] = new FZMath.Point2D(180.7, 280.7);     //  East

            //  TZ 0.003rad + X 0.7 + Y -0.7 + Z 0.7
            markS[9][0] = new FZMath.Point2D(780.3875057, 79.33551223);    //  North
            markS[9][1] = null;                            //  West
            markS[9][2] = new FZMath.Point2D(180.3902057, 80.49252819);   //  South
            markS[9][3] = new FZMath.Point2D(381.3227386, 280.1059562);     //  East

            markT[9][0] = new FZMath.Point2D(780.3875057, 279.8004681);    //  North
            markT[9][1] = new FZMath.Point2D(180.3902057, 281.6004654);     //  East


            /////////////////////////////////////////////////////////////////////////////////////////
            //  TZ 0.003 + TX 0.003
            markS[10][0] = new FZMath.Point2D(779.6875057, 79.18167702);    //  North
            markS[10][1] = null;                            //  West
            markS[10][2] = new FZMath.Point2D(179.6902057, 80.3428244);   //  South
            markS[10][3] = new FZMath.Point2D(380.6227386, 280.6690232);     //  East

            markT[10][0] = new FZMath.Point2D(779.6875057, 279.1009388);    //  North
            markT[10][1] = new FZMath.Point2D(179.6902057, 280.900928);     //  East

            //  TZ 0.003 + TY 0.003
            markS[11][0] = new FZMath.Point2D(779.6861571, 78.7330712);    //  North
            markS[11][1] = null;                            //  West
            markS[11][2] = new FZMath.Point2D(179.6915571, 81.26895889);   //  South
            markS[11][3] = new FZMath.Point2D(380.6231858, 280.4206179);     //  East

            markT[11][0] = new FZMath.Point2D(779.6861571, 279.1004681);    //  North
            markT[11][1] = new FZMath.Point2D(179.6915571, 280.9004654);     //  East

            //  여기까지 값들은 맞음.
            /////////////////////////////////////////////////////////////////////////////////////////
            //  X + Y + Z + TX(0.003) + TY(-0.003) //  이걸 해봐야 한다.
            markS[12][0] = new FZMath.Point2D(780.69865, 79.22458125);    //  North
            markS[12][1] = null;                            //  West
            markS[12][2] = new FZMath.Point2D(180.70135, 80.60345918);   //  South
            markS[12][3] = new FZMath.Point2D(380.70045, 280.1429332);     //  East

            markT[12][0] = new FZMath.Point2D(780.69865, 280.7004667);    //  North
            markT[12][1] = new FZMath.Point2D(180.70135, 280.7004667);     //  East


            //  X + Y + Z + TZ(-0.003) + TY(0.003) //  이걸 해봐야 한다.
            markS[13][0] = new FZMath.Point2D(780.3861571, 78.64679142);    //  North
            markS[13][1] = null;                            //  West
            markS[13][2] = new FZMath.Point2D(180.3915571, 81.18267911);   //  South
            markS[13][3] = new FZMath.Point2D(381.3231858, 280.3343381);     //  East

            markT[13][0] = new FZMath.Point2D(780.3861571, 279.8004681);    //  North
            markT[13][1] = new FZMath.Point2D(180.3915571, 281.6004654);     //  East

            /////////////////////////////////////////////////////////////////////////////////////////
            /////   여기부터 값을 만들어줘야 한다.
            //  X(0.7) + Y(0.7) + Z(0.7) + TX(-0.011) + TY(0.003) + TZ(0.001)
            markS[14][0] = new FZMath.Point2D(780.5947856, 78.15929952);    //  North
            markS[14][1] = null;                            //  West
            markS[14][2] = new FZMath.Point2D(180.5977856, 79.92888171);   //  South
            markS[14][3] = new FZMath.Point2D(380.9079289, 281.9479098);     //  East

            markT[14][0] = new FZMath.Point2D(780.5947856, 280.4063447);    //  North
            markT[14][1] = new FZMath.Point2D(180.5977856, 281.0063083);     //  East


            //  X + Y + Z + TX(0.003) + TY(-0.012) + TZ(-0.002)
            markS[15][0] = new FZMath.Point2D(780.4703855, 77.0124302);    //  North
            markS[15][1] = null;                            //  West
            markS[15][2] = new FZMath.Point2D(180.5147849, 83.29639018);   //  South
            markS[15][3] = new FZMath.Point2D(381.1222294, 280.4796669);     //  East

            markT[15][0] = new FZMath.Point2D(780.4703855, 280.1006772);    //  North
            markT[15][1] = new FZMath.Point2D(180.5147849, 281.300671);     //  East

            //  TY 회전
            //markS[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 + 100 + 0.689438965);    //  North
            //markS[7][1] = null;                            //  West
            //markS[7][2] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 + 100 - 0.689438965);   //  South
            //markS[7][3] = new FZMath.Point2D(480 + 0, 180 - 100);     //  East

            //markT[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 - 90);    //  North
            //markT[7][1] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 - 90);     //  East



            Prepare6DMotion();
            mFZM.mbCompY = 1;
            i = 0;
            mFZM.Extract6DMotion(-1, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 1; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 2; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 3; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 4; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 5;
            psi = 0; dZ = 0; TX = 0; TY = 0;
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 6; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 7; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 8; //   dZ 결과값이 반으로 나타남 -> 입력값을 다시 확인하고 단계별로 따라가면 오류 원인 찾을 수 있음.
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 9; //   dZ 결과값이 반으로 나타남
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 10; //  정상
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 11; //  정상
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 12; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 13; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 14; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            i = 15; 
            mFZM.Extract6DMotion(i, markT[i], markS[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.

            FZMath.Point2D[][] pSide = null;
            FZMath.Point2D[][] pTop = null;
            double xi = 0;  //  mm
            double xf = 0.01;   //  mm
            double yi = -0.3;  //  mm
            double yf = 0.3;   //  mm
            double txi = -0.0001;  //  rad
            double txf = 0.0001;   //  rad
            double tyi = -0.001;  //  rad
            double tyf = 0.001;   //  rad
            double ftx = 4;
            double fty = 2;

            GenerateSimData(1000, out pSide, out pTop, xi, xf, yi, yf, txi, txf, tyi, tyf, ftx, fty);
            Prepare6DMotion();
            double umScale = 5.5 / LensMag;
            double minScale = 60*180/Math.PI;
            string lstr = "";
            for ( i=0; i<1000; i++)
            {
                mFZM.Extract6DMotion(i, pTop[i], pSide[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
                lstr += pSide[i][0].X.ToString("F4") + "," + pSide[i][0].Y.ToString("F4") + "," + pSide[i][2].X.ToString("F4") + "," + pSide[i][2].Y.ToString("F4") + "," + pSide[i][3].X.ToString("F4") + "," + pSide[i][3].Y.ToString("F4") + ",";
                lstr += pTop[i][0].X.ToString("F4") + "," + pTop[i][0].Y.ToString("F4") + "," + pTop[i][1].X.ToString("F4") + "," + pTop[i][1].Y.ToString("F4") + ",,";
                lstr += (umScale * resT.X).ToString("F4") + "," + (umScale * resT.Y).ToString("F4") + "," + (umScale * dZ).ToString("F4") + "," + (minScale * TX).ToString("F4") + "," + (minScale * TY).ToString("F4") + "," + (minScale * psi).ToString("F4") + "\r\n";
            }
            StreamWriter wr = new StreamWriter("SimYTX.csv");
            wr.Write(lstr);
            wr.Close();

            pSide = null;
            pTop = null;
            xi = -0.2;  //  mm
            xf = 0.2;   //  mm
            yi = -0.3;  //  mm
            yf = 0.3;   //  mm
            txi = 0;  //  rad
            txf = 0;   //  rad
            tyi = 0;  //  rad
            tyf = 0;   //  rad
            ftx = 1;
            fty = 1;

            GenerateSimData(1000, out pSide, out pTop, xi, xf, yi, yf, txi, txf, tyi, tyf, ftx, fty);
            Prepare6DMotion();
            lstr = "";
            for (i = 0; i < 1000; i++)
            {
                mFZM.Extract6DMotion(i, pTop[i], pSide[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
                lstr += pSide[i][0].X.ToString("F4") + "," + pSide[i][0].Y.ToString("F4") + "," + pSide[i][2].X.ToString("F4") + "," + pSide[i][2].Y.ToString("F4") + "," + pSide[i][3].X.ToString("F4") + "," + pSide[i][3].Y.ToString("F4") + ",";
                lstr += pTop[i][0].X.ToString("F4") + "," + pTop[i][0].Y.ToString("F4") + "," + pTop[i][1].X.ToString("F4") + "," + pTop[i][1].Y.ToString("F4") + ",,";
                lstr += (umScale * resT.X).ToString("F4") + "," + (umScale * resT.Y).ToString("F4") + "," + (umScale * dZ).ToString("F4") + "," + (minScale * TX).ToString("F4") + "," + (minScale * TY).ToString("F4") + "," + (minScale * psi).ToString("F4") + "\r\n";
            }
            wr = new StreamWriter("SimXY.csv");
            wr.Write(lstr);
            wr.Close();

            pSide = null;
            pTop = null;
            xi = 0;  //  mm
            xf = 0;   //  mm
            yi = 0;  //  mm
            yf = 0;   //  mm
            txi = -0.002;  //  rad
            txf = 0.002;   //  rad
            tyi = -0.003;  //  rad
            tyf = 0.003;   //  rad
            ftx = 4;
            fty = 2;

            GenerateSimData(1000, out pSide, out pTop, xi, xf, yi, yf, txi, txf, tyi, tyf, ftx, fty);
            Prepare6DMotion();
            lstr = "";
            for (i = 0; i < 1000; i++)
            {
                mFZM.Extract6DMotion(i, pTop[i], pSide[i], ref resT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
                lstr += pSide[i][0].X.ToString("F4") + "," + pSide[i][0].Y.ToString("F4") + "," + pSide[i][2].X.ToString("F4") + "," + pSide[i][2].Y.ToString("F4") + "," + pSide[i][3].X.ToString("F4") + "," + pSide[i][3].Y.ToString("F4") + ",";
                lstr += pTop[i][0].X.ToString("F4") + "," + pTop[i][0].Y.ToString("F4") + "," + pTop[i][1].X.ToString("F4") + "," + pTop[i][1].Y.ToString("F4") + ",,";
                lstr += (umScale * resT.X).ToString("F4") + "," + (umScale * resT.Y).ToString("F4") + "," + (umScale * dZ).ToString("F4") + "," + (minScale * TX).ToString("F4") + "," + (minScale * TY).ToString("F4") + "," + (minScale * psi).ToString("F4") + "\r\n";
            }
            wr = new StreamWriter("SimTXTY.csv");
            wr.Write(lstr);
            wr.Close();
            //OpenFileDialog openFile = new OpenFileDialog();
            //openFile.DefaultExt = "csv";
            //openFile.Filter = "CSV(*.csv)|*.csv";
            //string sFileName = "";
            //if (openFile.ShowDialog() == DialogResult.OK)
            //    sFileName = openFile.FileName;
            //else
            //    return;

            //if (!File.Exists(sFileName))
            //    return;

            //string sFileNameCopy = "temp.csv";
            //File.Copy(sFileName, sFileNameCopy);
            //StreamReader sr = new StreamReader(sFileNameCopy);
            //string allstuff = sr.ReadToEnd();
            //sr.Close();
            //File.Delete(sFileNameCopy);

            //string[] eachLines = allstuff.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //markS = new FZMath.Point2D[eachLines.Length][];
            //markT = new FZMath.Point2D[eachLines.Length][];

            //int effCnt = 0;
            //for ( int il = 6; il < eachLines.Length; il++ )
            //{
            //    if (eachLines[il].Length < 8)
            //        continue;
            //    string[] posData = eachLines[il].Split(',');
            //    double[] lvalue = new double[24];
            //    for (int iq = 0; iq < 24; iq++)
            //        lvalue[iq] = double.Parse(posData[iq + 8]);

            //    if (lvalue[0] == 0 && lvalue[1] == 0)
            //        break;

            //    markS[effCnt] = new FZMath.Point2D[4];
            //    markT[effCnt] = new FZMath.Point2D[2];
            //    markS[effCnt][0] = new FZMath.Point2D(lvalue[0], lvalue[1]);
            //    markS[effCnt][1] = null;
            //    markS[effCnt][2] = new FZMath.Point2D(lvalue[8], lvalue[9]);
            //    markS[effCnt][3] = new FZMath.Point2D((lvalue[12]+lvalue[14])/2, (lvalue[13]+ lvalue[15])/2);

            //    markT[effCnt][0] = new FZMath.Point2D(lvalue[16], lvalue[17]);
            //    markT[effCnt][1] = new FZMath.Point2D(lvalue[20], lvalue[21]);
            //    effCnt++;
            //}

            //Prepare6DMotion();
            ////  Extract6DMotion_(int i, Point2D[] markT, Point2D[] markS, Point2D[] markNorm, ref Point2D T, ref double psi, ref double dZ, ref double TX, ref double TY)
            //mFZM.Extract6DMotion_(0, markT[0], markS[0], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            //string resStr = "";
            ////int debug = 0;
            //for ( i=1; i< effCnt; i++)
            //{
            //    mFZM.Extract6DMotion_(i, markT[i], markS[i], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
                //resStr += (refT.X * (5.5 / LensMag)).ToString("F1") + "\t" + (refT.Y * (5.5 / LensMag)).ToString("F1") + "\t" + (dZ * (5.5 / LensMag)).ToString("F1") + "\t"
            //             + (TX * 180 * 60 / Math.PI).ToString("F1") + "\t" + (TY * 180 * 60 / Math.PI).ToString("F1") + "\t" + (psi * 180 * 60 / Math.PI).ToString("F1") + "\r\n";

            //    continue;
            //}
            //rtbLog.Text = resStr;
            mFZM.SetTXTYOffset(txo, tyo, tzo);

            return;
        }

        public void GenerateSimData(int len, out FZMath.Point2D[][] pSide, out FZMath.Point2D[][] pTop, double xi, double xf, double yi, double yf, double txi, double txf, double tyi, double tyf , double ftx, double fty)
        {
            FZMath.Point2D[] markSpos = new FZMath.Point2D[4];
            FZMath.Point2D[] markTpos = new FZMath.Point2D[2];
            double rx = 0;double ry = 0;


            for (int i = 0; i < mFidMarkSide.Count; i++)
            {
                if (i == 0)
                {
                    TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, true, true, false);
                    markSpos[0] = new FZMath.Point2D();
                    markSpos[0].X = (int)(rx + 0.4999) + 320;
                    markSpos[0].Y = (int)(ry + 0.4999) + 171;
                }
                else if (i == 1)
                {
                    TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, true, true, true);
                    markSpos[2] = new FZMath.Point2D();
                    markSpos[2].X = (int)(rx + 0.4999) + 320;
                    markSpos[2].Y = (int)(ry + 0.4999) + 171;
                }
                else if (i == 2)
                {
                    TransferAbsToPicturePos(mFidMarkSide[i].fidInfo.X, mFidMarkSide[i].fidInfo.Y, ref rx, ref ry, false, true, true);
                    markSpos[3] = new FZMath.Point2D();
                    markSpos[3].X = (int)(rx + 0.4999) + 320;
                    markSpos[3].Y = (int)(ry + 0.4999) + 171;
                }
                else
                    continue;

            }

            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                markTpos[i] = new FZMath.Point2D();
                if (i == 0)
                    TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, true, false, false);
                else if (i == 1)
                    TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, true, false, true);

                markTpos[i].X = (int)(rx + 0.4999) + 320;
                markTpos[i].Y = (int)(ry + 0.4999) + 165;   //  Top View 기준 Mark Center 위치 고려
            }


            pSide = new FZMath.Point2D[len][];
            pTop = new FZMath.Point2D[len][];

            for ( int i=0; i<len; i++)
            {
                pSide[i] = new FZMath.Point2D[4];
                pSide[i][0] = new FZMath.Point2D();
                pSide[i][2] = new FZMath.Point2D();
                pSide[i][3] = new FZMath.Point2D();
                pTop[i] = new FZMath.Point2D[2];
                pTop[i][0] = new FZMath.Point2D();
                pTop[i][1] = new FZMath.Point2D();

            }
            double x = xi;
            double y = yi;
            double z = -0.3;
            double dz = 0.6/len;
            double tx = txi;
            double ty = tyi;
            double Atx = (txf - txi);
            double Aty = (tyf - tyi);
            double dx = (xf - xi) / (len-1);
            double dy = (yf - yi) / (len-1);

            double tz = 0;
            double sin40 = Math.Sin(40.0 / 180 * Math.PI);
            double cos40 = Math.Cos(40.0 / 180 * Math.PI);

            for (int i = 0; i < len; i++)
            {
                tx = txi + Atx/2 + Atx * Math.Sin(ftx * Math.PI * 2 * i / len);
                ty = tyi + Aty/2 + Aty * Math.Sin(fty * Math.PI * 2 * i / len);
                tz = 0.001 * Math.Cos(5 * Math.PI * 2 * i / len);

                pSide[i][0].X = markSpos[0].X + (x - 1.565 * tz) / (0.0055/ LensMag) ;//  X - 1.565 TZ;
                pSide[i][2].X = markSpos[2].X + (x - 1.565 * tz) / (0.0055/ LensMag) ;//  X - 1.565 TZ;
                pSide[i][3].X = markSpos[3].X + (x + 2.525 * tz) / (0.0055/ LensMag) ;//  X - 1.565 TZ;

                pTop[i][0].X = markTpos[0].X + (x - 1.565 * tz) / (0.0055 / LensMag);//  X - 1.565 TZ;
                pTop[i][1].X = markTpos[1].X + (x - 1.565 * tz) / (0.0055 / LensMag);//  X - 1.565 TZ;

                pSide[i][0].Y = markSpos[0].Y + ( - z * cos40 + y * sin40 - 1.565 * tx * cos40 + 2.625 * ty * cos40 - 2.625 * tz * sin40) / (0.0055 / LensMag);//  X - 1.565 TZ;
                pSide[i][2].Y = markSpos[2].Y + ( - z * cos40 + y * sin40 - 1.565 * tx * cos40 - 2.625 * ty * cos40 + 2.625 * tz * sin40) / (0.0055 / LensMag);//  X - 1.565 TZ;
                pSide[i][3].Y = markSpos[3].Y + ( - z * cos40 + y * sin40 + 2.525 * tx * cos40) / (0.0055 / LensMag);//  X - 1.565 TZ;

                pTop[i][0].Y = markTpos[0].Y + (y - 2.625 * tz) / (0.0055 / LensMag);//  X - 1.565 TZ;
                pTop[i][1].Y = markTpos[1].Y + (y + 2.625 * tz) / (0.0055 / LensMag);//  X - 1.565 TZ;

                x += dx;
                y += dy;
                z += dz;
            }
        }

        //private void button24_Click(object sender, EventArgs e)
        //{
        //    //  CalcScaleTopNSide(Point2D[] pTop, Point2D[] pSide, double normLength, ref double scaleTop, ref double scaleSide)

        //    //FZMath mFZM = new FZMath();

        //    FZMath.Point2D[] pTopNorm = new FZMath.Point2D[2];
        //    FZMath.Point2D[] pSideNorm = new FZMath.Point2D[2];
        //    FZMath.Point2D[] pTop = new FZMath.Point2D[2];
        //    FZMath.Point2D[] pSide = new FZMath.Point2D[2];

        //    double normLength = 445*(0.0055/LensMag);
        //    double scaleTop = 1;
        //    double scaleSide = 1;

        //    pTopNorm[0] = new FZMath.Point2D(222.0, 12);
        //    pTopNorm[1] = new FZMath.Point2D(-223.0, 10);

        //    pSideNorm[0] = new FZMath.Point2D(222.0, 12);
        //    pSideNorm[1] = new FZMath.Point2D(-223.0, 10);

        //    pTop[0] = new FZMath.Point2D(222.0, 12.2);
        //    pTop[1] = new FZMath.Point2D(-223.2, 10);

        //    pSide[0] = new FZMath.Point2D(222.0, 11.9);
        //    pSide[1] = new FZMath.Point2D(-222.9, 10);

        //    mFZM.CalcScaleTopNSide(pTop, pSide, normLength, ref scaleTop, ref scaleSide);
        //    mFZM.mScaleTop = scaleTop;
        //    mFZM.mScaleSide = scaleSide;
        //    //  여기까지 검증 완료 mScaleTop, mScaleSide 적용함수는 별도로없다. 나중에 좌표추출부에서 직접 참조하면 된다.

        //    FZMath.Point2D COI = new FZMath.Point2D(0, 0);

        //    double aT = 1;
        //    double aS = 1;
        //    mFZM.TopNSideViewRotationAnlge(pTopNorm, pSideNorm, pTop, pSide, ref aT, ref aS);
        //    //  여기까지 검증 완료

        //    mFZM.mOpticsAngleTop = -aT;
        //    mFZM.mOpticsAngleSide = -aS;

        //    FZMath.Point2D[] pTopVAC = mFZM.CompensateViewRotation(COI, pTop, mFZM.mOpticsAngleTop);
        //    FZMath.Point2D[] pSideVAC = mFZM.CompensateViewRotation(COI, pSide, mFZM.mOpticsAngleSide);
        //    //  실제로 SideView Center 는 설계치 기준 및 영상크기 960x360 기준으로 ( 480.000, 180.000 ) : 영상 좌하단모서리가 0,0 임을 전제
        //    //  실제로 TopView Center 는 설계치 기준 및 영상크기 960x360 기준으로 ( 480.000, 104.591 ) : 영상 좌하단모서리가 0,0 임을 전제
        //    //  여기까지 검증 완료

        //    //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));
        //    //string sFilePath = folder;
        //    string sFilePath = RootPath + "Training" ;
        //    if (!Directory.Exists(sFilePath))
        //        Directory.CreateDirectory(sFilePath);

        //    string sFileName = "";

        //    OpenFileDialog openFile = new OpenFileDialog();
        //    openFile.DefaultExt = "csv";
        //    openFile.InitialDirectory = sFilePath;
        //    openFile.Filter = "CSV(*.csv)|*.csv";
        //    openFile.Multiselect = true;
        //    if (openFile.ShowDialog() == DialogResult.OK)
        //    {
        //        mTargetFiles = new string[openFile.FileNames.Length];
        //        for (int i = 0; i < openFile.FileNames.Length; i++)
        //            mTargetFiles[i] = openFile.FileNames[i];
        //    }
        //    else
        //        return;

        //    int xW = mCalcEffBand;    //   이 폭은 Defocusing 수준에 따라 6 ~ 10범위에서 자동적으로 설정되도록 함이 적절하며, 관련 알고리즘 필요
        //    int lpeaktype = 0;

        //    for ( int fi = 0; fi < openFile.FileNames.Length; fi++ )
        //    {
        //        int width = 0;
        //        int height = 0;
        //        double xi0 = 2;
        //        double yi0 = 3;
        //        int yH = 18;
        //        List<int> DiffSrc = new List<int>();

        //        sFileName = mTargetFiles[fi];

        //        StreamReader rd = new StreamReader(sFileName);
        //        string lstr = rd.ReadToEnd();
        //        rd.Close();

        //        string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //        for (int i = 0; i < allLines.Length; i++)
        //        {
        //            string[] eachLine = allLines[i].Split(',');
        //            if (eachLine.Length < 2)
        //                break;
        //            height++;
        //            width = eachLine.Length;
        //            for (int j = 0; j < eachLine.Length; j++)
        //            {
        //                int val = int.Parse(eachLine[j]);
        //                DiffSrc.Add(val);
        //            }
        //        }
        //        int[] XiDiffsrc = DiffSrc.ToArray();

        //        //  시작점 설정
        //        xi0 = width / 2 - xW / 2 - 2;

        //        byte lupdown = 255;
        //        double edgeX = mFZM.ConvergePeakX(ref XiDiffsrc, width, height, (int)xi0, (int)yi0, xW, yH, ref lupdown, ref lpeaktype);
        //        //  여기까지 검증 완료

        //        double edgeY = 0;
        //        //int[] yiDiffsrc = new int[XiDiffsrc.Length];
        //        //for (int i = 0; i < width; i++)
        //        //    for (int j = 0; j < height; j++)
        //        //        yiDiffsrc[j + i * height] = XiDiffsrc[i + j * width];

        //        //edgeY = mFZM.ConvergePeakY(ref yiDiffsrc, height, width, yi0, xi0, yH, xW);
        //        ////  여기까지 검증 완료


        //        int lastIndex = sFileName.LastIndexOf("\\");
        //        rtbLog.Text += sFileName.Substring(lastIndex+1) + "\t" + edgeX.ToString("F4") + "\t" + edgeY.ToString("F4") + "\r\n";
        //    }

        //}

        private void button25_Click(object sender, EventArgs e)
        {
            var folder =  "C:\\CSHTest\\Result\\RawData\\Image";

            int clipOffset = 60;
            if ( tbClipOffset.Text.Length > 0)
                clipOffset = int.Parse(tbClipOffset.Text);

            string sFilePath = folder;

            //string sFileName = "";

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "bmp";
            openFile.InitialDirectory = sFilePath;
            openFile.Filter = "bmp(*.bmp)|*.bmp";
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                mTargetFiles = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                    mTargetFiles[i] = openFile.FileNames[i];
            }
            else
                return;

            Mat fullImg = new Mat();
            Mat Img960x360 = new Mat();
            for (int i=0; i< mTargetFiles.Length; i++)
            {
                fullImg = new Mat(mTargetFiles[i], ImreadModes.Grayscale);
                Rect rc = new Rect(clipOffset, 0, FOV_X, FOV_Y);
                Img960x360 = fullImg.SubMat(rc);

                int extIndex = mTargetFiles[i].LastIndexOf('.');
                string newFile = mTargetFiles[i].Substring(0, extIndex) + "_" + FOV_X.ToString() + ".bmp";

                Img960x360.SaveImage(newFile);
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            var folder = Path.GetFullPath(RootPath + "Training");

            string sFilePath = folder;

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "xml";
            saveFile.InitialDirectory = sFilePath;
            saveFile.Filter = "XML(*.xml)|*.xml";
            if (saveFile.ShowDialog() == DialogResult.OK)
                sFilePath = saveFile.FileName;
            else
                return;

            SaveFiducialMark(sFilePath);

            mLastFMIfile = sFilePath;
            label6.Text = mLastFMIfile;            //  mLastFMIfile
            //SaveFiducialMark(mLastFMIfile);
        }

        public Rect mIgnoreRect = new Rect();
        public Rect mSearchIgnore = new Rect();
        public bool mFirstIgnoreRect = false;

        private void pictureBox5_MouseClick(object sender, MouseEventArgs e)
        {
            if (mCustomImg == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                //Cv2.ImShow("A", mOverlayedImg);
                if (mFirstIgnoreRect)
                {
                    if (mIgnoreRect.Width > 5 && mIgnoreRect.Height > 5 && mIgnoreRect.X > 0 && mIgnoreRect.Y > 0)
                    {
                        mCustomImg.Rectangle(mIgnoreRect, Scalar.Cyan, 2);
                        //CreateBaseModel();
                        mFirstIgnoreRect = false;
                    }
                    else if (mIgnoreRect.Width > 5 && mIgnoreRect.Height > 5 && mIgnoreRect.X == 0 && mIgnoreRect.Y == 0)
                    {
                        mCustomImg.Rectangle(mIgnoreRect, Scalar.Cyan, 2);
                        mFirstIgnoreRect = false;
                    }
                }
                int xstart = (int)((e.X + 1) / (double)pictureBox5.Size.Width * mSourceImg[0].Width);
                int ystart = (int)((e.Y + 1) / (double)pictureBox5.Size.Height * mSourceImg[0].Height);

                mIgnoreRect.X = xstart - mIgnoreRect.Width / 2;
                mIgnoreRect.Y = ystart - mIgnoreRect.Height / 2;

                //CreateBaseModel();
                mCustomImg.Rectangle(mIgnoreRect, Scalar.Cyan, 2);
                //Cv2.ImShow("A", mOverlayedImg);

                Bitmap image = null;
                image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);

                pictureBox5.Image = image;
                pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;

            }
        }

        private void pictureBox5_MouseDown(object sender, MouseEventArgs e)
        {
            if (!cbIgnoreArea.Checked) return;
            mCstIgnore.StartPoint(pictureBox5, e);
        }

        private void pictureBox5_MouseMove(object sender, MouseEventArgs e)
        {
            if (!cbIgnoreArea.Checked) return;
            mCstIgnore.TrackRubberBand(pictureBox5, e);

            Rect lRect = new Rect(mCstIgnore.X, mCstIgnore.Y, (e.X - mCstIgnore.X), (e.Y - mCstIgnore.Y));
            if (mCustomImg != null && mCstIgnore.IsTracking > 0)
            {
                Rectangle lRectangle = mCstIgnore.GetSelectRect();
                double xstart = (lRectangle.X + 1) / (double)pictureBox5.Size.Width * mCustomImg.Width;
                double ystart = (lRectangle.Y + 1) / (double)pictureBox5.Size.Height * mCustomImg.Height;

                if (mCstIgnore.IsTracking > 0 && mCstIgnore.IsTracking < 10)
                {
                    ///////////////////////////////////////////////////
                    //  Tracker 를 새로 만들거나 크기를 바꾸는 경우
                    double lwidth = lRectangle.Width / (double)pictureBox5.Size.Width * mCustomImg.Width;
                    double lheight = lRectangle.Height / (double)pictureBox5.Size.Height * mCustomImg.Height;
                    mIgnoreRect = new Rect((int)xstart, (int)ystart, (int)lwidth, (int)lheight);

                    mFirstIgnoreRect = true;
                }
                else if (mCstIgnore.IsTracking == 10)
                {
                    ///////////////////////////////////////////////////
                    //  이미 있는 tracker 를 이동하는 경우

                    mIgnoreRect.X = (int)xstart;
                    mIgnoreRect.Y = (int)ystart;
                    mFirstIgnoreRect = true;
                    //label6.Text = mSelectRect.ToString();
                }
                //	보여지는 영상은 mSourceImg 에 들어있다.
                //	영상 클립박스

                if (mIgnoreRect.Width > 0 && mIgnoreRect.Height > 0)
                {
                    if (mCurrentScell.X == 4 || mCurrentTcell.X == 4)
                    {
                        Rect roi = new Rect((int)mIgnoreRect.X, (int)mIgnoreRect.Y, mIgnoreRect.Width, mIgnoreRect.Height);
                        //   해당 영역을 Search Model 에서 "무시영역"으로 설정한다.
                        //   영역 크기 및 위치가 제대로 설정되는지 검증 필요
                    }
                }
            }
        }

        private void pictureBox5_MouseUp(object sender, MouseEventArgs e)
        {
            if (!cbIgnoreArea.Checked) return;
            mCstIgnore.EndPoint(pictureBox5, e);
            mSearchIgnore = mIgnoreRect;
            richTextBox1.Text += "IgnoreRect : " + mSearchIgnore.X.ToString() + "," + mSearchIgnore.Y.ToString() + "," + mSearchIgnore.Width.ToString() + "," + mSearchIgnore.Height.ToString() + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            //UpdateMousePosition();
            //  최종 mIgnoreRect 를 SearchModel 에 반영한다.
        }

        private void pictureBox5_Paint(object sender, PaintEventArgs e)
        {
            if (!cbIgnoreArea.Checked) return;
            mCstIgnore.DrawRubberBand(pictureBox5, e);
        }

        private void lbModelScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            mModelScale = 8 - lbModelScale.SelectedIndex;

            richTextBox1.Text += "Model Scale changed to " + mModelScale.ToString() + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void dgvDesignNModelSide_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            mCurrentScell = new System.Drawing.Point(e.ColumnIndex, e.RowIndex);
            mCurrentTcell = new System.Drawing.Point(-1, -1);

            if (e.ColumnIndex == 4)
            {
                IsSeardchROIUpdate = false;
                IsModelSizeUpdate = true;
            }
            else if (e.ColumnIndex == 5)
            {
                IsSeardchROIUpdate = true;
                IsModelSizeUpdate = false;
            }else
            {
                IsSeardchROIUpdate = false;
                IsModelSizeUpdate = false;

            }

            DesignModelSideChanged(sender, 0);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            btnMouseClick(sender, e);

            while (dgvDesignNModelSide.Rows.Count > 1)
            {
                RemoveFidMark();
            }
            mLastFMIfile = "";
            label6.Text = mLastFMIfile;

            OpenSchematicFile();
        }

        long[][] mDoneConv = new long[30][];
        int[][] mDonePosX = new int[30][];
        int[][] mDonePosY = new int[30][];
        int[][] mDoneIndex = new int[30][];

        //private void calcConvAlongX(int lwidth, int lheight, int modelScale, ref int[] img, int x, int y, int iBuf, ref double subconv, ref Rect exArea)
        //{
        //    for (int i = 0; i < 23; i++)
        //    {
        //        if (iBuf + i < 4 || iBuf + i > 41)
        //            continue;
        //        if (iBuf + (22-i) < 4 || (22-iBuf) + i > 41)
        //            continue;

        //        mDoneConv[iBuf][i] = CalcConv(lwidth, lheight, modelScale, ref img, x + i, y, iBuf, ref subconv, ref exArea);
        //        mDonePosX [iBuf][i] = x + i;
        //        mDonePosY [iBuf][i] = y;
        //        mDoneIndex[iBuf][i] = i + iBuf * 23;
        //    }
        //}
        public OpenCvSharp.Point2d[][] mYLUT = null;
        public double mEastMarkYscale = 1;

        public bool GetYLUT(string camID, int currentFovY)
        {

            //MessageBox.Show("ZLUTfile = " + filename);    //  파일명 제대로 들어오는지 디버깅
            string filename = RootPath + "YLUT" + camID + ".csv";
            if (!File.Exists(filename))
                return false;

            mYLUT = new OpenCvSharp.Point2d[3][];

            List<OpenCvSharp.Point2d> lp = new List<OpenCvSharp.Point2d>();
            StreamReader sr = new StreamReader(filename);
            string allstr = sr.ReadToEnd();
            sr.Close();
            string[] eachLine = allstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int len = eachLine.Length;
            mYLUT[0] = new OpenCvSharp.Point2d[len-2];
            mYLUT[1] = new OpenCvSharp.Point2d[len-2];
            mYLUT[2] = new OpenCvSharp.Point2d[len-2];
            string[] strElements = eachLine[0].Split(',');
            strElements = eachLine[0].Split(',');
            int Ystart = int.Parse(strElements[1]);
            strElements = eachLine[1].Split(',');
            double Nscale = double.Parse(strElements[1]);
            double Sscale = double.Parse(strElements[3]);
            double Escale = double.Parse(strElements[5]);
            mEastMarkYscale = (Nscale + Sscale) / (2 * Escale);

            int yshift = currentFovY - Ystart;
            for ( int i=2; i< len; i++)
            {
                strElements = eachLine[i].Split(',');
                double x1 = double.Parse(strElements[0]);
                double y1 = double.Parse(strElements[1]);
                double x2 = double.Parse(strElements[2]);
                double y2 = double.Parse(strElements[3]);
                double x3 = double.Parse(strElements[4]);
                double y3 = double.Parse(strElements[5]);
                mYLUT[0][i - 2] = new OpenCvSharp.Point2d(x1 - yshift, y1 );
                mYLUT[1][i - 2] = new OpenCvSharp.Point2d(x2 - yshift, y2 );
                mYLUT[2][i - 2] = new OpenCvSharp.Point2d(x3 - yshift, y3 );
            }

            return true;

        }

        public void ApplyFOVYToYLUT(int yshift)
        {

        }
        public void ApplyYLUT(ref double y1, ref double y2, ref double y3)
        {
            //return;

            if (mYLUT == null)
                return ;

            int len = mYLUT[0].Length;
            for (int i = 0; i < len - 1; i++)
            {
                //  y1
                if ((mYLUT[0][i].X - y1) * (mYLUT[0][i + 1].X - y1) < 0)
                {
                    double delta = mYLUT[0][i].Y + (mYLUT[0][i + 1].Y - mYLUT[0][i].Y) * (y1 - mYLUT[0][i].X) / (mYLUT[0][i + 1].X - mYLUT[0][i].X);
                    y1 = y1 + delta;
                    break;
                }
                else if (y1 < mYLUT[0][0].X)
                {
                    y1 = y1 + mYLUT[0][0].Y + (mYLUT[0][1].Y - mYLUT[0][0].Y) * (y1 - mYLUT[0][0].X) / (mYLUT[0][1].X - mYLUT[0][0].X);
                    break;
                }
                else if (y1 > mYLUT[0][len - 1].X)
                {
                    y1 = y1 + mYLUT[0][len - 2].Y + (mYLUT[0][len - 1].Y - mYLUT[0][len - 2].Y) * (y2 - mYLUT[0][len - 2].X) / (mYLUT[0][len - 1].X - mYLUT[0][len - 2].X);
                    break;
                }
            }

            for (int i = 0; i < len - 1; i++)
            {
                //  y2
                if ((mYLUT[1][i].X - y2) * (mYLUT[1][i + 1].X - y2) < 0)
                {
                    double delta = mYLUT[1][i].Y + (mYLUT[1][i + 1].Y - mYLUT[1][i].Y) * (y2 - mYLUT[1][i].X) / (mYLUT[1][i + 1].X - mYLUT[1][i].X);
                    y2 = y2 + delta;
                    break;
                }
                else if (y2 < mYLUT[1][0].X)
                {
                    y2 = y2 + mYLUT[1][0].Y + (mYLUT[1][1].Y - mYLUT[1][0].Y) * (y1 - mYLUT[1][0].X) / (mYLUT[1][1].X - mYLUT[1][0].X);
                    break;
                }
                else if (y2 > mYLUT[1][len - 1].X)
                {
                    y2 = y2 + mYLUT[1][len - 2].Y + (mYLUT[1][len - 1].Y - mYLUT[1][len - 2].Y) * (y2 - mYLUT[1][len - 2].X) / (mYLUT[1][len - 1].X - mYLUT[1][len - 2].X);
                    break;
                }
            }

            for (int i = 0; i < len - 1; i++)
            {
                //  y3
                if ((mYLUT[2][i].X - y3) * (mYLUT[2][i + 1].X - y3) < 0)
                {
                    double delta = mYLUT[2][i].Y + (mYLUT[2][i + 1].Y - mYLUT[2][i].Y) * (y3 - mYLUT[2][i].X) / (mYLUT[2][i + 1].X - mYLUT[2][i].X);
                    y3 = y3 + delta;
                    break;
                }
                else if (y3 < mYLUT[2][0].X)
                {
                    y3 = y3 + mYLUT[2][0].Y + (mYLUT[2][1].Y - mYLUT[2][0].Y) * (y1 - mYLUT[2][0].X) / (mYLUT[2][1].X - mYLUT[2][0].X);
                    break;
                }
                else if (y3 > mYLUT[2][len - 1].X)
                {
                    y3 = y3 + mYLUT[2][len - 2].Y + (mYLUT[2][len - 1].Y - mYLUT[2][len - 2].Y) * (y2 - mYLUT[2][len - 2].X) / (mYLUT[2][len - 1].X - mYLUT[2][len - 2].X);
                    break;
                }
            }
            y3 = y3 * mEastMarkYscale;
        }
    }
}