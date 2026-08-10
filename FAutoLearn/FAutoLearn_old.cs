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

namespace FAutoLearn
{
    public partial class FAutoLearn : Form
    {

        public string mOrgFile = "";

        public FZMath mFZM = new FZMath();

        //public Mat mSourceImgFile = null;
        public Mat[] mSourceImg = new Mat[16];
        public Mat mCustomImg = null;
        //Mat mProcessImg = null;
        public Mat mOverlayedImg = null;
        public Mat mSchematicImg = null;
        public Mat mSchematicOverlayedImg = null;
        public int mLastImg = 0;
        public int mProcSequence = 0;
        public Rect[][] mDetectedRect = new Rect[16][];
        public Rect[][] mDetectedLearnRect = new Rect[200][];
        public int[] mDetectedModel = new int[200];
        public int[][] mDetectedLearned = new int[200][];
        public int mTrainingLength = 0;
        //public bool[] mIsRunModel = new bool[200];
        //public bool[] mIsRunSingleFile = new bool[16];
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

        public double vSin40 = Math.Sin(40.0 / 180 * Math.PI);


        public CSTracker mCst = new CSTracker();
        public CSTracker mCstIgnore = new CSTracker();
        public List<CSMarker> mCsmSide = new List<CSMarker>();
        public List<CSMarker> mCsmTop = new List<CSMarker>();
        public List<TextBox[]> mMarkerPosInputS = new List<TextBox[]>();
        public List<TextBox[]> mMarkerPosInputT = new List<TextBox[]>();
        public int mCurCsmIndex = 0;
        public FZMath.Point2D[] mMarkNorm = new FZMath.Point2D[4];   //  [0] : North, [1] : West ,  [2] : South, [3] : East
        public const int FOV_Y = 342;
        public const int FOV_X = 840;

        public struct sSearchModel
        {
            public int mScale;
            public int width;
            public int height;
            public int conv;
            public int sub;
            public double subconv;
            public int[] img;
            //public int[] diffimg;
        }

        public class sMarkResult
        {
            public int Azimuth = -1;
            public OpenCvSharp.Point2d pos = new OpenCvSharp.Point2d();
            public System.Drawing.Size mSize = new System.Drawing.Size();
            public sMarkResult(int azimuth=-1, double x=0, double y=0)
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
            public int sub = 2;
            public double subconv = 0;
            public int MScale = 4;  //  Model Scale
            public int[] img = null;    //  model image data
            public sFiducialInfo fidInfo = new sFiducialInfo();
        }
        public class sFiducialInfo
        {
            public string schematicFile = "";    //  마크가 생성된 Schmatic Picture File 의 Full Path Name
            public int Azimuth = 0;    //   마크의 Azimuth : 0,1,2,3,4, ... , 11
            public int markID = 0;  //   없을 때 0
            public double X = 0;   //  마크 중심의 실제 좌표 X
            public double Y = 0;   //  마크 중심의 실제 좌표 Y
            public int signX = 1;   //  마크 중심의 실제 좌표 X
            public int signY = 1;   //  마크 중심의 실제 좌표 Y
            //public System.Drawing.Point markPos = new System.Drawing.Point(0, 0);    //  마크의 좌상 위치
            //public System.Drawing.Point TBXpos = new System.Drawing.Point(0, 0);     //  도면정보상의 X 좌표가 들어있는 TextBox 의 좌표
            //public System.Drawing.Point TBYpos = new System.Drawing.Point(0, 0);     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 좌표
            public int markPosX = 0;    //  마크의 좌상 X 위치 ( Tracker )
            public int markPosY = 0;     //  마크의 좌상 Y 위치 ( Tracker )
            public int TBXposX = 0;     //  도면정보상의 X 좌표가 들어있는 TextBox 의 X좌표
            public int TBXposY = 0;    //  도면정보상의 X 좌표가 들어있는 TextBox 의 Y좌표
            public int TBYposX = 0;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 X좌표
            public int TBYposY = 0;     //  도면정보상의 Y 좌표가 들어있는 TextBox 의 Y좌표
        }

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

        public byte[][] mQuadImg = new byte[400][];    //  Max 400 ea quadImg Buffer, absIndex 로 참조된다.
        public int[] mGIndex = new int[400];         //  하나하나가 지정된 absIndex 를 값으로 가진다.
        public int[] mNGIndex = new int[400];        //  하나하나가 지정된 absIndex 를 값으로 가진다.
        public int mNGCount = 0;
        public DetectedMark[] mDetectedmark = null;
        public int mMarkCount = 0;                      //  mDetectedmark 가 초기화된 이후 찾은 마크 개수
        public int mMarkFocused = -1;

        public int mBaseModelCnt = 0;

        public byte[][] q_Value = new byte[16][];   //  축소이미지

        public sSearchModel[] mSearchModel = new sSearchModel[10];
        public sSearchModel[] mNewSearchModel = new sSearchModel[10];
        public int mSearchModelIndex = 0;
        public int mSelectedSubIndex = 0;

        public System.Drawing.Image mBtnP;
        public System.Drawing.Image mBtnN;

        public List<sFiducialInfo> mFidInfo = new List<sFiducialInfo>();
        public List<sFiducialMark> mFidMarkSide = new List<sFiducialMark>();
        public List<sFiducialMark> mFidMarkTop = new List<sFiducialMark>();
        public List<sFiducialMark> mFidMarkSideF = null;// new List<sFiducialMark>();
        public List<sFiducialMark> mFidMarkTopF = null;// new List<sFiducialMark>();

        public bool IsSeardchROIUpdate = false;
        public bool IsModelSizeUpdate = false;

        public FAutoLearn()
        {
            string newDir = "..\\RawData";
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAutoLearn));
            mBtnP = ((System.Drawing.Image)(resources.GetObject("button9.BackgroundImage")));
            mBtnN = ((System.Drawing.Image)(resources.GetObject("button9.BackgroundImage")));

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            newDir = "..\\Training";
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }


            InitializeComponent();
            //textBox1.Text = "0.0";
            for (int i = 0; i < 16; i++)
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
            for ( int i=0; i<16; i++)
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

            //for (int i = 0; i < 12; i++)
            //    p_Value[i] = new byte[960* FOV_Y];

            mCst.ResizePinSize = 6;
            mCst.Create();
            mCstIgnore.ResizePinSize = 6;
            mCstIgnore.Create(1);
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

            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\RawData"));

            string sFilePath = folder;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.InitialDirectory = "C:\\TZAFTest\\Result";
            openFileDialog1.FilterIndex = 2;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //Image myImage = Image.FromFile(filename);
            mTargetFiles = new string[openFileDialog1.FileNames.Length];
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

            //int tvOffset = (int)(mOpticsTopViewOffset / (0.0055 / 0.35) + 0.4999);
            double tvOffset = (mOpticsTopViewOffset / (0.0055 / 0.35));
            double tgtOffset = mOpticsTgtOffset / (0.0055 / 0.35) * vSin40;
            //  Draw Center of Top View -> 영상의 유효영역을 벗어나므로 그리지 않는다.
            //Cv2.Line(mOverlayedImg, lWidth / 2 - 20, lHeight / 2+ tvOffset,        lWidth / 2 + 20,  lHeight / 2+ tvOffset, Scalar.Magenta, 1, LineTypes.Link4);
            //Cv2.Line(mOverlayedImg, lWidth / 2,      lHeight / 2 - 10+ tvOffset,   lWidth / 2,       lHeight / 2 + 10+ tvOffset, Scalar.Magenta, 1, LineTypes.Link4);
            //  Draw Side View center                                                         
            Cv2.Line(mOverlayedImg, lWidth / 2 - 20, lHeight / 2,           lWidth / 2 + 20,  lHeight / 2, Scalar.Aqua, 1, LineTypes.Link4);
            Cv2.Line(mOverlayedImg, lWidth / 2,      lHeight / 2 - 10,      lWidth / 2,       lHeight / 2 + 10, Scalar.Aqua, 1, LineTypes.Link4);
            //  Draw Center of Image Sensor    
            Cv2.Line(mOverlayedImg, lWidth / 2 - 20, (int)(lHeight / 2 - tgtOffset), lWidth / 2 + 20, (int)(lHeight / 2 - tgtOffset), Scalar.Red, 1, LineTypes.Link4);
            Cv2.Line(mOverlayedImg, lWidth / 2, (int)(lHeight / 2 - 10 - tgtOffset), lWidth / 2, (int)(lHeight / 2 + 10 - tgtOffset), Scalar.Red, 1, LineTypes.Link4);
            //Cv2.Circle(mOverlayedImg, new OpenCvSharp.Point(lWidth / 2, lHeight / 2 - tgtOffset), 4, Scalar.Red, 1);

            //  Draw Center of all fiducial marks
            //  mOverlayedImg 에 표시한다.
            double fWndOffset = (mOpticsFWOffset / (0.0055 / 0.35))/ vSin40;
            double wy = lHeight / 2 + fWndOffset;
            Cv2.Line(mOverlayedImg, lWidth / 2 - lWidth/6, (int)(wy+0.4999), lWidth / 2 + lWidth / 6, (int)(wy+0.4999), Scalar.LimeGreen, 1, LineTypes.Link4);

            double x = 0;
            double y = 0;
            double rx = 0;
            double ry = 0;
            bool[] sideAzimuthUpper = new bool[8];
            bool[] sideAzimuthLeft = new bool[8];
            System.Drawing.Point[] markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
            int markCount = 0;

            int cx = mOverlayedImg.Width / 2;
            int cy = mOverlayedImg.Height / 2;
            for ( int i=0; i< mFidMarkSide.Count; i++)
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
                markPos[markCount].Y = (int)(ry - tgtOffset + 0.4999);
                markCount++;

                Cv2.Line(mOverlayedImg, cx + (int)rx - 10   , cy + (int)(ry - tgtOffset +0.4999)      , cx + (int)rx + 10 , cy + (int)(ry - tgtOffset+0.4999), Scalar.OrangeRed, 1, LineTypes.Link4);
                Cv2.Line(mOverlayedImg, cx + (int)rx        , cy + (int)(ry - 10 - tgtOffset +0.4999) , cx + (int)rx      , cy + (int)(ry + 10 - tgtOffset+0.4999), Scalar.OrangeRed, 1, LineTypes.Link4);
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
                    yUpper = sideAzimuthUpper[mFidMarkTop[i].Azimuth+2];
                    IsLeft = sideAzimuthLeft[mFidMarkTop[i].Azimuth+2];
                }

                TransferAbsToPicturePos(mFidMarkTop[i].fidInfo.X, mFidMarkTop[i].fidInfo.Y, ref rx, ref ry, yUpper, false, IsLeft);

                markPos[markCount].X = (int)(rx + 0.4999);
                markPos[markCount].Y = (int)(ry - tgtOffset + 0.4999);
                markCount++;

                Cv2.Line(mOverlayedImg, cx + (int)rx - 10    , cy + (int)ry       , cx + (int)rx + 10  , cy + (int)ry , Scalar.FromRgb(64, 180, 255), 1, LineTypes.Link4);
                Cv2.Line(mOverlayedImg, cx + (int)rx         , cy + (int)ry - 10  , cx + (int)rx       , cy + (int)ry + 10 , Scalar.FromRgb(64, 180, 255), 1, LineTypes.Link4);
            }
            //if (markCount>0)
            //{
            //    StreamWriter wr = new StreamWriter("markPosOnPanel.csv");
            //    for (int i = 0; i < markPos.Length; i++)
            //        wr.WriteLine(markPos[i].X.ToString() + "," + markPos[i].Y.ToString());
            //    wr.Close();
            //}
        }
        public void SaveMarkPosOnPanel()
        {
            double tvOffset = (mOpticsTopViewOffset / (0.0055 / 0.35));
            double tgtOffset = mOpticsTgtOffset / (0.0055 / 0.35) * vSin40;

            //int lWidth = mOverlayedImg.Width;
            //int lHeight = mOverlayedImg.Height;

            double fWndOffset = (mOpticsFWOffset / (0.0055 / 0.35)) / vSin40;
            //double wy = lHeight / 2 + fWndOffset;

            double x = 0;
            double y = 0;
            double rx = 0;
            double ry = 0;

            bool[] sideAzimuthUpper = new bool[8];
            bool[] sideAzimuthLeft = new bool[8];
            System.Drawing.Point[] markPos = new System.Drawing.Point[mFidMarkSide.Count + mFidMarkTop.Count];
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
                markPos[markCount].Y = (int)(ry - tgtOffset + 0.4999);
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
                markPos[markCount].Y = (int)(ry - tgtOffset + 0.4999);
                markCount++;

            }
            if (markCount > 0)
            {
                StreamWriter wr = new StreamWriter("markPosOnPanel.csv");
                for (int i = 0; i < markPos.Length; i++)
                    wr.WriteLine(markPos[i].X.ToString() + "," + markPos[i].Y.ToString());
                wr.Close();
            }
        }
        public string GetFileNameOfMarkPosOnPanel()
        {
            return "markPosOnPanel.csv";
        }

        //public void TransferAbsToPicturePos(double x, double y, ref double rx, ref double ry, bool yUpper = true, bool IsSideView = true, bool IsLeft = true)
        //{
        //    int lWidth = mOverlayedImg.Width;
        //    int lHeight = mOverlayedImg.Height;
        //    double tvOffset = (mOpticsTopViewOffset / (0.0055 / 0.35));

        //    if (IsLeft)
        //        rx = lWidth / 2 - x / (0.0055 / 0.35);
        //    else
        //        rx = lWidth / 2 + x / (0.0055 / 0.35);

        //    if (IsSideView)
        //    {
        //        if (yUpper)
        //            ry = lHeight / 2 - (y / (0.0055 / 0.35)) * vSin40;
        //        else
        //            ry = lHeight / 2 + (y / (0.0055 / 0.35)) * vSin40;
        //    }
        //    else
        //    {
        //        if (yUpper)
        //            ry = lHeight / 2 + tvOffset - (y / (0.0055 / 0.35));
        //        else
        //            ry = lHeight / 2 + tvOffset + (y / (0.0055 / 0.35));  // 실제로는 발생할 수 없는 Case
        //    }
        //}

        public void TransferAbsToPicturePos(double x, double y, ref double rx, ref double ry, bool yUpper = true, bool IsSideView = true, bool IsLeft = true)
        {
            //int lWidth = mOverlayedImg.Width;
            //int lHeight = mOverlayedImg.Height;
            double tvOffset = (mOpticsTopViewOffset / (0.0055 / 0.35));

            if ( IsLeft )
                rx = - x / (0.0055 / 0.35);
            else
                rx = + x / (0.0055 / 0.35);

            if (IsSideView)
            {
                if (yUpper)
                    ry = - (y / (0.0055 / 0.35)) * vSin40;
                else
                    ry = + (y / (0.0055 / 0.35)) * vSin40;
            }
            else
            {
                if (yUpper)
                    ry = + tvOffset - (y / (0.0055 / 0.35));
                else
                    ry = + tvOffset + (y / (0.0055 / 0.35));  // 실제로는 발생할 수 없는 Case
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
            int k = 0;
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
        public void RotateImage(Mat src, Mat dest)
        {
            //double angle = 0;
            //if (textBox1.Text.Length > 0)
            //    angle = Convert.ToDouble(textBox1.Text);

            //Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(src.Width / 2, src.Height / 2), angle, 1.0);
            //Cv2.WarpAffine(src, dest, matrix, new OpenCvSharp.Size(src.Width, src.Height));
        }

        public int[] mStrokeRefX = null;
        public int[] mStrokeRefY = null;

        public double[] mSpaceRatioX = new double[1000];
        public double[] mSpaceRatioY = new double[1000];
        public double[] mSpaceAvgX = new double[1000];
        public double[] mSpaceAvgY = new double[1000];
        public double[] mFillFactorAvg = new double[1000];
        public double[] mFillFactorStdev = new double[1000];

        public void ProfileAnalysis(Mat src, ref double[] xArray, ref double[] yArray)
        {
            //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            byte[] bytes;
            src.GetArray(out bytes);

            int imageWidth = src.Width;
            int imageHeight = src.Height;

            int[] lStrokeRefX = new int[100];
            int[] lStrokeRefY = new int[100];

            //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            //byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            byte min = 0;
            byte max = 0;
            int qq = 0;

            int blankH = 2 * imageHeight / 25;
            int blankW = imageWidth / 15;

            for (int i = 0; i < imageWidth; i++)
                xArray[i] = 0;

            for (int j = 0; j < imageHeight; j++)
                yArray[j] = 0;

            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    xArray[i] += bytes[i + imageWidth * j] * bytes[i + imageWidth * j] * bytes[i + imageWidth * j];   // 종으로 같은 열을 다 더한 것
                    yArray[j] += bytes[i + imageWidth * j] * bytes[i + imageWidth * j] * bytes[i + imageWidth * j];   // 횡으로 같은 줄을 다 더한 것
                }
            }
            //Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            //image.UnlockBits(data);

            double peak = 0;
            int numPeak = 0;
            double[] maXarray = new double[imageWidth - 4];
            double[] maYarray = new double[imageHeight - 4];

            for (int i = 0; i < imageWidth - 4; i++)
            {
                xArray[i] = (xArray[i] + xArray[i + 1] + xArray[i + 2] + xArray[i + 3] + xArray[i + 4]);
                if (peak < xArray[i] && (i > 2 * blankW && i < (imageWidth - 2 * blankW)))
                {
                    if (peak < xArray[i])
                        peak = xArray[i];
                }
            }
            for (int i = 0; i < imageWidth - 4; i++)
                xArray[i] = (40 * (xArray[i] + xArray[i + 1])) / peak;

            for (int i = 0; i < imageWidth - 2 * blankW - 4; i++)
            {
                for (int j = 0; j < 2 * blankW; j++)
                    maXarray[blankW + i] += xArray[i + j];
                maXarray[blankW + i] = maXarray[blankW + i] / (2 * blankW);
            }
            for (int i = 0; i < blankW; i++)
            {
                maXarray[i] = maXarray[blankW];
                maXarray[maXarray.Length - blankW - 4 + i] = maXarray[maXarray.Length - blankW - 4];
            }

            peak = 0;
            for (int j = 0; j < imageHeight - 4; j++)
            {
                yArray[j] = (yArray[j] + yArray[j + 1] + yArray[j + 2] + yArray[j + 3] + yArray[j + 4]);
                if (peak < yArray[j] && (j > 5 * blankH && j < (imageHeight - 5 * blankH)))
                {
                    if (peak < yArray[j])
                        peak = yArray[j];
                }
            }
            for (int j = 0; j < imageHeight - 4; j++)
                yArray[j] = (40 * (yArray[j] + yArray[j + 1])) / peak;

            for (int i = 0; i < imageHeight - 2 * blankH - 4; i++)
            {
                for (int j = 0; j < 2 * blankH; j++)
                    maYarray[blankH + i] += yArray[i + j];
                maYarray[blankH + i] = maYarray[blankH + i] / (2 * blankH);
            }
            for (int i = 0; i < blankH; i++)
            {
                maYarray[i] = maYarray[blankH];
                maYarray[maYarray.Length - blankH + i] = maYarray[maYarray.Length - blankH - 4];
            }

            bool IsSpace = false;
            bool IsSpace2 = false;
            int endX = 0;
            int beginX = imageWidth;
            double lSpaceWidth = 0;
            double lFillWidth = 0;
            double lSpaceSum = 0;
            double lTotalWidth = 0;
            int q = 0;

            for (int i = 0; i < imageWidth - 4; i++)
            {
                if (xArray[i] < maXarray[i] - 6)
                {
                    if (!IsSpace2)
                    {
                        beginX = i;
                        IsSpace2 = true;
                    }
                }
                else
                {
                    if (IsSpace2)
                    {
                        endX = i;
                        IsSpace2 = false;
                        if (endX > beginX)
                        {
                            lStrokeRefX[q] = (beginX + endX) / 2;
                            if (endX - beginX < imageWidth / 15)
                                q++;
                        }
                    }
                }
                if (xArray[i] < maXarray[i])
                {
                    if (!IsSpace && lFillWidth > imageWidth / 15)
                    {
                        lTotalWidth -= lFillWidth;
                    }
                    IsSpace = true;
                    lSpaceWidth++;
                    lTotalWidth++;

                    lFillWidth = 0;
                }
                else
                {
                    if (IsSpace)
                    {
                        IsSpace = false;
                        if (lSpaceWidth > imageWidth / 15)
                        {
                            lTotalWidth -= lSpaceWidth;
                        }
                        else
                        {
                            lSpaceSum += lSpaceWidth;
                        }
                        lSpaceWidth = 0;
                    }
                    else
                    {
                        lTotalWidth++;
                        lFillWidth++;
                    }
                }
            }
            mStrokeRefX = new int[q];
            Array.Copy(lStrokeRefX, mStrokeRefX, q);

            double qy = lSpaceSum / q;

            double ySpaceRatio = lSpaceSum / lTotalWidth;

            IsSpace = false;
            IsSpace2 = false;
            lSpaceWidth = 0;
            lSpaceSum = 0;
            lTotalWidth = 0;
            q = 0;
            beginX = 0;
            endX = 0;
            lFillWidth = 0;
            for (int i = 0; i < imageHeight - 4; i++)
            {
                if (yArray[i] < maYarray[i] - 6)
                {
                    if (!IsSpace2)
                    {
                        beginX = i;
                        IsSpace2 = true;
                    }
                }
                else
                {
                    if (IsSpace2)
                    {
                        endX = i;
                        IsSpace2 = false;
                        if (endX > beginX)
                        {
                            lStrokeRefY[q] = (beginX + endX) / 2;
                            if (endX - beginX < imageHeight / 15)
                                q++;
                        }
                    }
                }
                if (yArray[i] < maYarray[i])
                {
                    if (!IsSpace && lSpaceWidth > imageHeight / 23)
                    {
                        lTotalWidth -= lFillWidth;
                    }
                    IsSpace = true;
                    lSpaceWidth++;
                    lTotalWidth++;

                    lFillWidth = 0;
                }
                else
                {
                    if (IsSpace)
                    {
                        IsSpace = false;
                        if (lSpaceWidth > imageHeight / 23)
                        {
                            lTotalWidth -= lSpaceWidth;
                        }
                        else
                        {
                            lSpaceSum += lSpaceWidth;
                        }
                        lSpaceWidth = 0;
                    }
                    else
                    {
                        lTotalWidth++;
                        lFillWidth++;
                    }
                }
            }
            mStrokeRefY = new int[q];
            Array.Copy(lStrokeRefY, mStrokeRefY, q);

            double qx = lSpaceSum / q;

            double xSpaceRatio = lSpaceSum / lTotalWidth;

            //richTextBox3.Text = "X Space Ratio \t" + xSpaceRatio.ToString("F3") + "\r\n" + "Y Space Ratio \t" + ySpaceRatio.ToString("F3") + "\r\n";
            //richTextBox3.Text += "X Space Avg \t" + qx.ToString("F1") + "\r\n" + "Y Space Avg \t" + qy.ToString("F1") + "\r\n";

            mSpaceRatioX[0] = xSpaceRatio;
            mSpaceRatioY[0] = ySpaceRatio;
            mSpaceAvgX[0] = qx;
            mSpaceAvgY[0] = qy;

            StreamWriter wr = new StreamWriter("ProfileAna.csv");
            for (int i = 0; i < Math.Max(imageWidth, imageHeight); i++)
            {
                if (i < imageWidth)
                    wr.WriteLine(xArray[i].ToString() + "," + yArray[i].ToString());
                else
                    wr.WriteLine("," + yArray[i].ToString());
            }
            wr.Close();
        }

        public void StrokeAnalysis(Mat src)
        {
            byte[] bytes;
            src.GetArray(out bytes);

            //Bitmap image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
            //BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            //byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            //image.UnlockBits(data);

            double[,] fillFactor = null;

            if (mStrokeRefX == null) return;
            fillFactor = new double[mStrokeRefX.Length, mStrokeRefY.Length];

            //  Calculate Fill Factor for each Cahractor
            double avgFillFactor = 0;
            int effectiveCount = 0;
            for (int i = 0; i < mStrokeRefX.Length - 1; i++)
            {
                for (int j = 0; j < mStrokeRefY.Length - 1; j++)
                {
                    for (int xi = mStrokeRefX[i]; xi < mStrokeRefX[i + 1]; xi++)
                    {
                        for (int yi = mStrokeRefY[j]; yi < mStrokeRefY[j + 1]; yi++)
                        {
                            if (bytes[xi + src.Width * yi] > 128)
                                fillFactor[i, j]++;
                        }
                    }
                    fillFactor[i, j] = fillFactor[i, j] / ((mStrokeRefX[i + 1] - mStrokeRefX[i]) * (mStrokeRefY[j + 1] - mStrokeRefY[j]));
                    if (fillFactor[i, j] > 0.000005)
                    {
                        avgFillFactor += fillFactor[i, j];
                        effectiveCount++;
                    }
                }
            }
            avgFillFactor = avgFillFactor / effectiveCount;

            //  Calculate Standard Deviation of FillFactor array
            double stdevFillFactor = 0;
            for (int i = 0; i < mStrokeRefX.Length - 1; i++)
            {
                for (int j = 0; j < mStrokeRefY.Length - 1; j++)
                {
                    if (fillFactor[i, j] > 0.000005)
                        stdevFillFactor += (fillFactor[i, j] - avgFillFactor) * (fillFactor[i, j] - avgFillFactor);
                }
            }
            stdevFillFactor = Math.Sqrt(stdevFillFactor / effectiveCount);

            //richTextBox3.Text += "Avg of FillFactor \t" + avgFillFactor.ToString("F5") + "\r\n";
            //richTextBox3.Text += "Stdev of FillFactor \t" + stdevFillFactor.ToString("F5") + "\r\n";

            mFillFactorAvg[0] = avgFillFactor;
            mFillFactorStdev[0] = stdevFillFactor;
            //StreamWriter wr = new StreamWriter("StrokeAna.csv");
            //string lstr = "";
            //wr.WriteLine("Stdev Fill Factor," + stdevFillFactor.ToString("F5"));
            //for (int i = 0; i < mStrokeRefX.Length - 1; i++)
            //{
            //    lstr = "";
            //    for (int j = 0; j < mStrokeRefY.Length - 1; j++)
            //    {
            //        if (fillFactor[i, j] > 0.000005)
            //            lstr += fillFactor[i, j].ToString("F5") + ",";
            //        else
            //            lstr += ",";
            //    }
            //    wr.WriteLine(lstr);
            //}
            //wr.Close(); 
        }

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
            int c = 0;
            for (int i = 0; i < src.Width - 2; i++)
            {
                for (int j = 0; j < src.Height - 2; j++)
                {
                    trained[i, j] = (trained[i, j] - (int)avgBin + 1) / 100;
                    convSum += trained[i, j] * verifyImg[i, j];
                }
            }
            //  dest is trained Image

            StreamWriter wr = new StreamWriter("train_" + findex.ToString() + ".csv");
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
            double avg = 0;
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
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            SetSearchYRegion();
            DetectInSelectedFile();
        }

        public int mCalcEffBand = 7;
        public void DetectInSelectedFile()
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
            //src.GetArray(out p_Value[0]);     //  src : Full Size Image
            Cv2.Resize(src, ImgDest, new OpenCvSharp.Size(src.Width / mModelScale, src.Height / mModelScale), 1.0/ mModelScale, 1.0/ mModelScale, InterpolationFlags.Linear);  //  1/mModelScale 축소
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

            for (int i = 0; i < 12; i++)
                smr[i] = new sMarkResult();

            OpenCvSharp.Point[] fMarkPos = null;
            
            if (!mDetectEveryMark)
            {
                if (mCurrentScell.X >= 0)
                    fMarkPos = FineCOG(0, ref smr, ref nFound, false, 0, mCurrentScell.Y);
                else if (mCurrentTcell.X >= 0)
                    fMarkPos = FineCOG(0, ref smr, ref nFound, false, 0, (mCurrentTcell.Y + 8));
            }else
            {
                fMarkPos = FineCOG(0, ref smr, ref nFound, false, 0, -1);
            }

            if (nFound== 0)
            {
                //richTextBox1.Text += "Fail to find mark.\r\n";
                return;
            }
            string lstr = nFound.ToString() + " Found\t";

            for (int i = 0; i < nFound; i++)
                lstr += smr[i].pos.X.ToString("F4") + "\t" + smr[i].pos.Y.ToString("F4") + "\t";

            richTextBox1.Text += lstr + "\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////
            //  다음은 일단 마크를 찾았다 치고, 찾은 위치 xi, yi 에 대해서 처리한다.
            //  원본 이미지는 src 로서 스케일 1:1 
            int xi = 0;
            int yi = 0;
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

                mDetectedmark[mMarkCount].conv = CalcConv(ref lfMark, fMarkPos[i].X, fMarkPos[i].Y, 0, ref subconv);
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
            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

            string sFilePath = folder;

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
                mSearchModel[modelNo].sub = Convert.ToInt16(lhead[4]);
                if (lhead.Length > 5)
                    if (lhead[5].Length > 1)
                        mSearchModel[modelNo].subconv = Convert.ToDouble(lhead[5]);  //  이 항목이 없을수도 있으므로.

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
                var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

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
                                    + "," + mSearchModel[mi].sub.ToString()
                                    + "," + mSearchModel[mi].subconv.ToString("F2");
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
                                    + "," + mSearchModel[mi].sub.ToString()
                                    + "," + mSearchModel[mi].subconv.ToString("F2");
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

            double kkk = 1.999;
            //MessageBox.Show(((int)(kkk * 10)).ToString());

            int[] basewidth = new int[10];
            int[] baseheight = new int[10];
            int res = OpenBaseModel(ref basewidth, ref baseheight);
            if (res == 0) return;
            string lstr = "";

            for (int i = 0; i < res; i++)
            {
                lstr = "M_" + i.ToString() + " " + mSearchModel[i].width.ToString() + " " + mSearchModel[i].height.ToString() + " " + mSearchModel[i].conv.ToString() + " " + mSearchModel[i].sub.ToString() + " " + mSearchModel[i].subconv.ToString("F2");
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
            tbDefaultROIWidth.Text = "1400";
            tbDefaultROIHeight.Text = "1400";
            lbModelScale.SelectedIndex = 0;
            tbTargetSubConv.Text = "0";
            tbSubConv.Text = "0";
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

        private void button4_Click(object sender, EventArgs e)
        {
            if (tbSubConv.Text.Length > 0)
                mUserSubConv = Convert.ToInt16(tbSubConv.Text);
            else
                mUserSubConv = 2;

            //  기존  Model 에 Weight 를 반영하여 새로운 Model 을 만든다.
            //  W x 기존 모델 + SUM ( DataGridView2 소속으로 지정된 Detected 데이터 ; G Detected Data )
            //  상기 2차원 데이터를 Historam 기준으로 2%, 98% Saturation 처리하여  -20 ~ 20 으로 Normalize
            int lwidth = mSearchModel[mSearchModelIndex].width;
            int lheight = mSearchModel[mSearchModelIndex].height;
            int bin1pro = (int)(lwidth * lheight * 0.01 + 0.5);
            int[] lAvg = new int[lwidth * lheight];
            int[] lSort = new int[lwidth * lheight];


            int lmin = 0;
            int lmax = 0;
            int lavg = 0;
            double lconv = 99999;
            double lsum = 0;
            long lcount = 0;
            int oldweight = 0;

            for (int i = 0; i < mMarkCount; i++)
            {
                if (!mDetectedmark[i].IsGood)
                    continue;
                if (lconv > mDetectedmark[i].conv)
                    lconv = mDetectedmark[i].conv;  //  최소값을 찾아서 0.9 배 해서 모델에 저장한다.

            }

            for (int i = 0; i < mMarkCount; i++)
            {
                if (!mDetectedmark[i].IsGood)
                    continue;

                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        lAvg[x + y * lwidth] += mQuadImg[mDetectedmark[i].absIndex][x + y * lwidth];
                    }
            }
            if (oldweight > 0)
            {
                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        lAvg[x + y * lwidth] += mSearchModel[mSearchModelIndex].img[x + y * lwidth] * oldweight;
                    }
            }

            for (int x = 0; x < lwidth; x++)
                for (int y = 0; y < lheight; y++)
                {
                    lAvg[x + y * lwidth] = lAvg[x + y * lwidth] / (mMarkCount + oldweight);
                    lsum += lAvg[x + y * lwidth];
                    lcount++;
                }

            Array.Copy(lAvg, lSort, lAvg.Length);
            Array.Sort(lSort);
            lmin = lSort[bin1pro];
            lmax = lSort[lwidth * lheight - 1 - bin1pro];
            //lavg = (int)(lsum / lcount);
            lavg = lSort[(lwidth * lheight) / 2] - 1;
            if (lavg < 10)
                lavg = 10;

            for (int itr = 0; itr < 2; itr++)
            {
                lsum = 0;
                lcount = 0;
                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        if (lAvg[x + y * lwidth] > lavg)
                            lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lmax - lavg);
                        else
                            lSort[x + y * lwidth] = (19 * (lAvg[x + y * lwidth] - lavg)) / (lavg - lmin);

                        if (lSort[x + y * lwidth] < -20)
                            lSort[x + y * lwidth] = -20;

                        if (lSort[x + y * lwidth] > 20)
                            lSort[x + y * lwidth] = 20;

                        lsum += lSort[x + y * lwidth];
                        lcount++;
                    }
                double tmpavg = lsum / lcount;
                if (Math.Abs(tmpavg) < 0.7)
                    break;
                lavg = (int)(lavg + tmpavg / 2);
            }



            mNewSearchModel[mSearchModelIndex].width = lwidth;
            mNewSearchModel[mSearchModelIndex].height = lheight;
            mNewSearchModel[mSearchModelIndex].conv = (int)(lconv * 0.9);
            mNewSearchModel[mSearchModelIndex].img = new int[lwidth * lheight];
            for (int i = 0; i < lwidth * lheight; i++)
                mNewSearchModel[mSearchModelIndex].img[i] = lSort[i];


            byte[] tmpBytes = new byte[lwidth * lheight];
            //for (int i = 0; i < tmpBytes.Length; i++)
            //    tmpBytes[i] = (byte)(6 * mNewSearchModel[mSearchModelIndex].img[i] + 120);
            Mat tmpImg = new Mat();
            Mat tmp10x = new Mat();
            //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
            //Cv2.ImShow("Avg Img", tmp10x);

            OptimizeModel();
            //Task taskOptimizeModel = Task.Run(() => { OptimizeModel(); });
            //// Task가 끝날 때까지 대기
            //taskOptimizeModel.Wait();

            //Thread Thread_OptimizeModel = new Thread(() => OptimizeModel());
            //Thread_OptimizeModel.Start();

            ///////////////////////////////////////////////////////////////////////
            /////   Show Mode Image on picturebox3
            tmpBytes = new byte[lwidth * lheight];
            for (int i = 0; i < tmpBytes.Length; i++)
                tmpBytes[i] = (byte)(6 * mNewSearchModel[mSearchModelIndex].img[i] + 120);

            tmpImg = new Mat(lheight, lwidth, MatType.CV_8U, tmpBytes);
            Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
            Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmp10x);

            pictureBox4.Image = myImage;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            ///////////////////////////////////////////////////////////////////////
        }

        public void OptimizeModel()
        {
            double[] SMixup = new double[100];
            double[] SConv1 = new double[100];
            double[] SConv2 = new double[100];
            int lwidth = mNewSearchModel[mSearchModelIndex].width;
            int lheight = mNewSearchModel[mSearchModelIndex].height;
            double subConv = 0;
            double Conv = 0;
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
            Mat tmpImg = new Mat(lheight, lwidth, MatType.CV_8U, tmpBytes);


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
                int x0 = lwidth / 2 - 1;
                int[] vAvg = new int[lheight];
                for (int yi = 0; yi < lheight; yi++)
                    for (int xi = x0; xi < x0 + 3; xi++)
                        vAvg[yi] += mDetectedmark[Di].img[xi + yi * lwidth];

                int y0 = lheight / 2 - 1;
                int[] hAvg = new int[lwidth];
                for (int xi = 0; xi < lwidth; xi++)
                    for (int yi = y0; yi < y0 + 3; yi++)
                        hAvg[xi] += mDetectedmark[Di].img[xi + yi * lwidth];

                //  Bin 1/3 축소 영샹
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                for (int ni = 0; ni < lwidth * lheight; ni++)
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
                for (int ni = 0; ni < lwidth * lheight; ni++)
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
                for (int yi = 0; yi < lheight; yi++)
                    for (int xi = 0; xi < lwidth; xi++)
                        mDetectedmark[mMarkCount].img[xi + yi * lwidth] = (byte)(hAvg[xi] / 3);

                //tmpImg.SetArray(mDetectedmark[mMarkCount].img);
                //Cv2.Resize(tmpImg, tmp10x, new OpenCvSharp.Size(lwidth * 10, lheight * 10), 10, 10, InterpolationFlags.Area);
                //title = "Vetically Expanded Imge " + Di.ToString();
                //Cv2.ImShow(title, tmp10x);
                mMarkCount++;

                //  중앙 종 3열 평균을 좌우로 확장한 영상
                mDetectedmark[mMarkCount].img = new byte[lfMark.modelSize.Width * lfMark.modelSize.Height];
                for (int yi = 0; yi < lheight; yi++)
                    for (int xi = 0; xi < lwidth; xi++)
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
            if (tbSubConv.Text.Length > 0)
                mNewSearchModel[mSearchModelIndex].sub = Convert.ToInt16(tbSubConv.Text);
            else
                mNewSearchModel[mSearchModelIndex].sub = 2;

            //  Initialize New Model Parameters
            if (mNewSearchModel[mSearchModelIndex].img != null)
            {
                lfMark.img = mNewSearchModel[mSearchModelIndex].img;
                lfMark.conv = mNewSearchModel[mSearchModelIndex].conv;
                lfMark.sub = mNewSearchModel[mSearchModelIndex].sub;
                lfMark.subconv = mNewSearchModel[mSearchModelIndex].subconv;
            }

            //dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            byte[] srcData = null;
            double res = 0;
            double subconv = 0;

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

                    switch (mNewSearchModel[mSearchModelIndex].sub % 100)
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

                    switch (mNewSearchModel[mSearchModelIndex].sub % 100)
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
            mNewSearchModel[mSearchModelIndex].subconv = minSubConv - avgSubConv / 5;

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
            double avgModel = 0;

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
            double avgModel = 0;

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
            double avgModel = 0;

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
            double avgModel = 0;
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
            double avgModel = 0;
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
            double avgModel = 0;
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

        byte[] LUTBgNoise = new byte[1000];

        public void SetBackgroundNoise(byte[] bgNoise)
        {
            LUTBgNoise = new byte[bgNoise.Length];
            Array.Copy(bgNoise, LUTBgNoise, bgNoise.Length);
        }

        public OpenCvSharp.Point[] FineCOG(int iIndex, ref sMarkResult[] smr, ref long Nfound, bool IsDebug = false, int iBuf = 0, int whichModel = -1 )
        {
            int cntFound = 0;
            OpenCvSharp.Point[] posFoundS = new OpenCvSharp.Point[12];
            long[] convFoundS = new long[12];
            int[] shiftX = new int[3] { -1, 0, 1 };
            int[] shiftY = new int[2] { 1, 2 };

            int modelIndex = 0;
            int modelIndex_s = 0;   //  Model Start 번호
            int modelIndex_e = mFidMarkSide.Count + mFidMarkTop.Count;  //  Model End 번호
            int modelScaleFactor = 4;

            sFiducialMark lfMark = null;
            bool IsSide = true;

            mIsDebug = IsDebug;

            if (whichModel>= 0) 
            {
                //  특정 모델이 지정된 경우
                //   특정 모델이 지정되지 않은 경우는 whichModel = -1 로써 깔끔하게 정리됨.
                modelIndex_s = whichModel;
                modelIndex_e = whichModel + 1;
                if (modelIndex_s>7)
                {
                    modelIndex_s = modelIndex_s - 8 + mFidMarkSide.Count;
                    modelIndex_e = modelIndex_s + 1;
                }
            }
            for (modelIndex = modelIndex_s; modelIndex< modelIndex_e; modelIndex++)// foreach( sFiducialMark lfMark in mFidMarkSide)
            {
                int si = 0;

                bool FoundIt = false;
                int FoundItCount = 0;   //  모델을 하나 시도할 때마다 증가한다.
                while(!FoundIt)
                {
                    if (FoundItCount==0)
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
                        if (mFMSideCandidate.Count == 0  || FoundItCount > mFMSideCandidate.Count )
                            break;

                        if (modelIndex < mFMSideCandidate[FoundItCount - 1].Count)
                        {
                            if (mFMSideCandidate[FoundItCount - 1].Count < modelIndex + 1)
                                break;
                            lfMark = mFMSideCandidate[FoundItCount - 1][modelIndex];
                            //  이것을 mFidMarkSide 에 Replace 해줘야 한다.
                            //  ...
                            //mFidMarkSide.RemoveAt(modelIndex);
                            //mFidMarkSide.Insert(modelIndex, lfMark);
                            IsSide = true;
                        }
                        else
                        {
                            if (mFMTopCandidate[FoundItCount - 1].Count < modelIndex - mFidMarkSide.Count + 1)
                                break;
                            lfMark = mFMTopCandidate[FoundItCount - 1][modelIndex - mFidMarkSide.Count];
                            //  이것을 mFidMarkSide 에 Replace 해줘야 한다.
                            //  ...
                            //mFidMarkTop.RemoveAt(modelIndex - mFidMarkSide.Count);
                            //mFidMarkTop.Insert(modelIndex - mFidMarkSide.Count, lfMark);
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

                    int threshT = lfMark.conv;
                    int thresh0 = (int)(threshT * 0.4);
                    int thresh1 = (int)(threshT * 0.6);
                    int thresh2 = (int)(threshT * 0.9);
                    int thresh3 = (int)(threshT * 0.7);

                    modelScaleFactor = lfMark.MScale;

                    double subconv = 0;
                    bool bSaved = false;
                    int prev_i = 0;

                    int xSkipWidth = 55;
                    xSkipWidth = lfMark.modelSize.Width;
                    List<System.Drawing.Point> pSkips = new List<System.Drawing.Point>();
                    //  모델 하나를 검색하면 다음모델로 넘어가야 한다.
                    //  모델크기가 0 이면 continue
                    //  결과를 모델 lfMark 에 저장할 수있는가?
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// Side View
                    j = lfMark.searchRoi.Top / modelScaleFactor;
                    int j_max = lfMark.searchRoi.Bottom / modelScaleFactor;
                    while (j <= j_max)
                    {
                        i = lfMark.searchRoi.Left / modelScaleFactor;
                        int i_max = lfMark.searchRoi.Right / modelScaleFactor;

                        while (i < i_max)
                        {
                            bool IsContinue = false;
                            foreach (System.Drawing.Point pSkip in pSkips)
                            {
                                if (Math.Abs(pSkip.Y - j) < 2 && Math.Abs(pSkip.X - i) < 4)
                                {
                                    IsContinue = true;
                                    break;
                                }
                            }
                            if (IsContinue)
                            {
                                i += 2;
                                continue;
                            }

                            //lconv = CalcConv(ref lfMark, i, j, iBuf, ref subconv);
                            lconv = CalcConv(lfMark.modelSize.Width, lfMark.modelSize.Height, lfMark.MScale, ref lfMark.img, i, j, iBuf, ref subconv);
                            prev_i = i;
                            if (lconv < -thresh0)
                            {
                                System.Drawing.Point llp = new System.Drawing.Point(i, j);
                                pSkips.Add(llp);
                                i += 5;
                            }
                            else if (lconv < 0)
                                i += 3;
                            else if (lconv < thresh0)
                                i += 2;
                            else
                                i++;

                            if (lconv > threshT)
                                bCand = true;

                            if (lconv < thresh3 && bCand == true)
                            {
                                bCand = false;
                                if (bSaved)
                                {
                                    bSaved = false;
                                    maxConv = 0;
                                    cntFound++;

                                    bFinishViewRegion = true;
                                    FoundIt = true;
                                    //if (iIndex == 0)
                                    //{
                                    //    //   첫번째 영상에서 검출에 성공한 Model 을 확정한다.
                                    //    if (modelIndex < mFMSideCandidate[FoundItCount - 1].Count)
                                    //    {
                                    //        mFidMarkSide.RemoveAt(modelIndex);
                                    //        mFidMarkSide.Insert(modelIndex, lfMark);
                                    //    }
                                    //    else
                                    //    {
                                    //        mFidMarkTop.RemoveAt(modelIndex - mFidMarkSide.Count);
                                    //        mFidMarkTop.Insert(modelIndex - mFidMarkSide.Count, lfMark);
                                    //    }
                                    //}
                                    break;
                                }
                            }
                            if (bCand)
                            {
                                //////////////////////////////////////////////////////////////////////////////
                                //////////////////////////////////////////////////////////////////////////////
                                bool IsCorrect = false;
                                if (lfMark.sub == 0)
                                    IsCorrect = true;

                                else if (lfMark.sub > 100)
                                {
                                    if (subconv < lfMark.subconv)
                                        IsCorrect = true;
                                }
                                else
                                {
                                    if (subconv > lfMark.subconv)
                                        IsCorrect = true;
                                }
                                //////////////////////////////////////////////////////////////////////////////
                                //////////////////////////////////////////////////////////////////////////////

                                if (maxConv < lconv && (IsCorrect == true))
                                {
                                    bSaved = true;
                                    maxConv = lconv;
                                    if (IsSide)
                                        smr[cntFound].Azimuth = lfMark.Azimuth;
                                    else
                                        smr[cntFound].Azimuth = lfMark.Azimuth + 8;
                                    smr[cntFound].mSize = new System.Drawing.Size(lfMark.modelSize.Width, lfMark.modelSize.Height);
                                    posFoundS[cntFound].X = i - 1;
                                    posFoundS[cntFound].Y = j;
                                    convFoundS[cntFound] = lconv;
                                }
                            }
                        }
                        j++;
                        if (bFinishViewRegion)
                            break;
                    }
                    FoundItCount++;
                }
                if (lfMark == null)
                    continue;

                if (cntFound == 0)
                    continue;

                si = cntFound-1;
                long[] localConv = new long[6];
                long[] localConvCopy = new long[6];
                double[] localSubConv = new double[6];
                long[] lconvIndex = new long[6] { 0, 1, 2, 3, 4, 5 };
                if (posFoundS[si].Y > 0)
                {
                    localConv[0] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[0]);
                    localConv[1] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 1, iBuf, ref localSubConv[1]);
                    localConv[2] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 1, iBuf, ref localSubConv[2]);

                    localConv[3] = CalcConv(ref lfMark, posFoundS[si].X - 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[3]);
                    localConv[4] = CalcConv(ref lfMark, posFoundS[si].X, posFoundS[si].Y + 2, iBuf, ref localSubConv[4]);
                    localConv[5] = CalcConv(ref lfMark, posFoundS[si].X + 1, posFoundS[si].Y + 2, iBuf, ref localSubConv[5]);
                }
                Array.Copy(localConv, localConvCopy, 6);
                Array.Sort(localConv, lconvIndex);
                Array.Sort(localConvCopy, localSubConv);
                if (localConv[5] > convFoundS[si])
                {
                    convFoundS[si] = localConv[5];
                    posFoundS[si].X += shiftX[lconvIndex[5] % 3];
                    posFoundS[si].Y += shiftY[lconvIndex[5] / 3];
                }

                //  각 마크의 COG 를 구한다.
                if (posFoundS[si].X == 0) continue;

                Rect rc = new Rect();
                int subLeft = posFoundS[si].X * modelScaleFactor - 2;
                int subTop = posFoundS[si].Y * modelScaleFactor - 2;

                if (subLeft < 0)
                    subLeft = 0;
                if (subTop < 0)
                    subTop = 0;

                Rect subRoi = new Rect(subLeft, subTop, (lfMark.modelSize.Width+1) * modelScaleFactor + 2, (lfMark.modelSize.Height+1) * modelScaleFactor + 2);
                if (subRoi.Bottom > mSourceImg[iBuf].Height)
                {
                    int deltaY = subRoi.Bottom - mSourceImg[iBuf].Height;
                    if (deltaY>1)
                    {
                        subRoi.Height -= (deltaY-1);
                        subRoi.Y -= 1;
                    }
                    else
                    {
                        subRoi.Height -= deltaY;
                    }
                }
                Mat tgtImg = mSourceImg[iBuf].SubMat(subRoi);

                //Mat tgt4 = new Mat(tgtImg.Height * 8, tgtImg.Width * 8, MatType.CV_8UC1);
                //Cv2.Resize(tgtImg, tgt4, new OpenCvSharp.Size(tgtImg.Height * 8, tgtImg.Width * 8), 8, 8);
                //Cv2.ImShow("Org", tgt4);

                //if (smr[si].Azimuth == 0)
                //{
                //    Mat tgt4 = new Mat(tgtImg.Height * 4, tgtImg.Width * 4, MatType.CV_8UC1);
                //    Cv2.Resize(tgtImg, tgt4, new OpenCvSharp.Size(tgtImg.Height * 4, tgtImg.Width * 4), 4, 4);
                //    Cv2.ImShow("1", tgt4);
                //}
                //cx[cntFound] = posFoundS[cntFound].X;
                //cy[cntFound] = posFoundS[cntFound].Y;
                //cntFound++;
                //Nfound = cntFound;
                //return true;

                byte[] tgtBuf = null;
                tgtImg.GetArray(out tgtBuf);
                int[] xDiffimg = new int[tgtBuf.Length];

                //StreamWriter wr = new StreamWriter("C:\\TZAFTest\\Result\\Diff\\DiffX" + ".csv");
                //String strTmp = "";
                int[] diffSign = new int[2]{ 1,1};
                double LLBr = 0;
                double LRBr = 0;
                double RLBr = 0;
                double RRBr = 0;

                bool bUseFull4pixel = cbFull4pixel.Checked;

                for (int y = 0; y < tgtImg.Height; y++)
                {
                    //strTmp = "";
                    LLBr = 0;
                    LRBr = 0;
                    RLBr = 0;
                    RRBr = 0;
                    List<int> monitorBin   = new List<int>();
                    List<int> monitorSlope = new List<int>();
                    List<int> monitorIndex = new List<int>();
                    int pBin = 0;
                    int kk = 0;
                    for (int x = 2; x < tgtImg.Width/5; x++)
                    {
                        pBin =  tgtBuf[x + y * tgtImg.Width];
                        LLBr += pBin;

                        //monitorBin.Add(pBin);
                        //pBin = Math.Abs(tgtBuf[x + 2 + y * tgtImg.Width] + tgtBuf[x + 1 + y * tgtImg.Width] - tgtBuf[x - 1 + y * tgtImg.Width] - tgtBuf[x - 2 + y * tgtImg.Width]);
                        //monitorSlope.Add(pBin);
                        //monitorIndex.Add(kk);
                        //kk++;
                    }
                    LLBr = LLBr / (tgtImg.Width / 5 - 2);

                    for (int x = tgtImg.Width / 5; x < (2 * tgtImg.Width) / 5 - 2; x++)
                    {
                        pBin =  tgtBuf[x + y * tgtImg.Width];
                        LRBr += pBin;

                        //monitorBin.Add(pBin);
                        //pBin = Math.Abs(tgtBuf[x + 2 + y * tgtImg.Width] + tgtBuf[x + 1 + y * tgtImg.Width] - tgtBuf[x - 1 + y * tgtImg.Width] - tgtBuf[x - 2 + y * tgtImg.Width]);
                        //monitorSlope.Add(pBin);
                        //monitorIndex.Add(kk);
                        //kk++;
                    }
                    LRBr = LRBr / ((2 * tgtImg.Width) / 5 - tgtImg.Width / 5 - 2);

                    //int[] sortedSlope = monitorSlope.ToArray();
                    //int[] sortedIndex = monitorIndex.ToArray();
                    //Array.Sort(sortedSlope, sortedIndex);
                    //double leftSum = 0;
                    //double rightSum = 0;
                    //int accCnt = 0;
                    //for (int x = sortedIndex[sortedIndex.Length - 1] - 1; x >= 0; x--)
                    //{
                    //    leftSum += monitorBin[x];
                    //    accCnt++;
                    //    if (accCnt > 3)
                    //        break;
                    //}
                    //leftSum = leftSum / accCnt;
                    //accCnt = 0;
                    //for (int x = sortedIndex[sortedIndex.Length - 1] + 1; x < sortedIndex.Length; x++)
                    //{
                    //    rightSum += monitorBin[x];
                    //    accCnt++;
                    //    if (accCnt > 3)
                    //        break;
                    //}
                    //rightSum = rightSum / accCnt;



                    //if (leftSum > rightSum)
                    //    diffSign[0] = -1;
                    //else
                    //    diffSign[0] = 1;

                    if (LLBr > LRBr)
                        diffSign[0] = -1;
                    else
                        diffSign[0] = 1;

                    monitorBin.Clear();
                    monitorSlope.Clear();
                    monitorIndex.Clear();
                    kk = 0;
                    for (int x = (3 * tgtImg.Width) / 5 + 3; x < (4 * tgtImg.Width) / 5; x++)
                    {
                        pBin = tgtBuf[x + y * tgtImg.Width];
                        RLBr += pBin;

                        //monitorBin.Add(pBin);
                        //pBin = tgtBuf[x + 2 + y * tgtImg.Width] + tgtBuf[x + 1 + y * tgtImg.Width] - tgtBuf[x - 1 + y * tgtImg.Width] - tgtBuf[x - 2 + y * tgtImg.Width];
                        //monitorSlope.Add(pBin);
                        //monitorIndex.Add(kk);
                        //kk ++;
                    }
                    RLBr = RLBr / ((4 * tgtImg.Width) / 5 - (3 * tgtImg.Width) / 5 - 3);

                    for (int x = (4 * tgtImg.Width) / 5; x < tgtImg.Width - 3; x++)
                    {
                        pBin = tgtBuf[x + y * tgtImg.Width];
                        RRBr += pBin;

                        //monitorBin.Add(pBin);
                        //pBin = tgtBuf[x + 2 + y * tgtImg.Width] + tgtBuf[x + 1 + y * tgtImg.Width] - tgtBuf[x - 1 + y * tgtImg.Width] - tgtBuf[x - 2 + y * tgtImg.Width];
                        //monitorSlope.Add(pBin);
                        //monitorIndex.Add(kk);
                        //kk ++;
                    }
                    RRBr = RRBr / (tgtImg.Width - 3 - (4 * tgtImg.Width) / 5);

                    //sortedSlope = monitorSlope.ToArray();
                    //sortedIndex = monitorIndex.ToArray();
                    //Array.Sort(sortedSlope, sortedIndex);
                    //leftSum = 0;
                    //rightSum = 0;
                    //accCnt = 0;
                    //for (int x = sortedIndex[sortedIndex.Length-1]-1; x >= 0 ; x--)
                    //{
                    //    leftSum += monitorBin[x];
                    //    accCnt++;
                    //    if (accCnt > 3)
                    //        break;
                    //}
                    //leftSum = leftSum / accCnt;
                    //accCnt = 0;
                    //for (int x = sortedIndex[sortedIndex.Length-1]+1; x < sortedIndex.Length; x++)
                    //{
                    //    rightSum += monitorBin[x];
                    //    accCnt++;
                    //    if (accCnt > 3)
                    //        break;
                    //}
                    //rightSum = rightSum / accCnt;

                    //if (leftSum > rightSum)
                    //    diffSign[1] = -1;
                    //else
                    //    diffSign[1] = 1;


                    if (RLBr > RRBr)
                        diffSign[1] = -1;
                    else
                        diffSign[1] = 1;

                    for (int x = 1; x < tgtImg.Width - 1; x++)
                    {
                        int curSign = x < tgtImg.Width / 2 ? diffSign[0] : diffSign[1];
                        xDiffimg[x + y * (tgtImg.Width - 1)] = curSign * (tgtBuf[x + 1 + y * tgtImg.Width] - tgtBuf[x - 1 + y * tgtImg.Width] - LUTBgNoise[subLeft + x + 1] + LUTBgNoise[subLeft + x - 1]);
                        if ( x > 1 && x < tgtImg.Width - 2)
                        {
                            xDiffimg[x + y * (tgtImg.Width - 1)] += curSign * (tgtBuf[x + 2 + y * tgtImg.Width] - tgtBuf[x - 2 + y * tgtImg.Width] - LUTBgNoise[subLeft + x + 2] + LUTBgNoise[ subLeft + x - 2] ) ;
                        }
                    }
                }
                //if (smr[si].Azimuth == 4)
                //{
                //    byte[] tmpXDiff = new byte[xDiffimg.Length];

                //    for (int qqq = 0; qqq < xDiffimg.Length; qqq++)
                //        tmpXDiff[qqq] = (byte)((128 + xDiffimg[qqq]));

                //    Mat lsrc = new Mat(tgtImg.Height, tgtImg.Width - 1, MatType.CV_8UC1, tmpXDiff);

                //    Mat ltgt4 = new Mat(lsrc.Height * 8, lsrc.Width * 8, MatType.CV_8UC1);
                //    Cv2.Resize(lsrc, ltgt4, new OpenCvSharp.Size(tgtImg.Height * 8, tgtImg.Width * 8), 8, 8);
                //    Cv2.ImShow("Diff", ltgt4);
                //}
                //wr.Close();
                int[] yDiffimg = new int[tgtImg.Width * (tgtImg.Height - 1)];
                byte[] yBDiffimg = new byte[tgtImg.Width * (tgtImg.Height - 1)];

                //wr = new StreamWriter("C:\\TZAFTest\\Result\\Diff\\DiffY_tr" + ".csv");
                for (int x = 0; x < tgtImg.Width; x++)
                {
                    //strTmp = "";
                    for (int y = 1; y < tgtImg.Height - 1; y++)
                    {
                        //if (x > 1 && y > 1 && (x - 2) / modelScaleFactor < lfMark.modelSize.Width && (y - 1) / modelScaleFactor < lfMark.modelSize.Height)
                        //{
                        //    yDiffimg[y + x * (tgtImg.Height - 1)] = tgtBuf[x + (y + 1) * tgtImg.Width] - tgtBuf[x + (y - 1) * tgtImg.Width];
                        //}
                        //else
                        //    yDiffimg[y + x * (tgtImg.Height - 1)] = tgtBuf[x + (y + 1) * tgtImg.Width] - tgtBuf[x + (y-1) * tgtImg.Width];
                        yDiffimg[y + x * (tgtImg.Height - 1)] = tgtBuf[x + (y + 1) * tgtImg.Width] - tgtBuf[x + (y - 1) * tgtImg.Width];
                        if ( y>1 && y< tgtImg.Height - 2)
                        {
                            //if (bUseFull4pixel)
                            //    yDiffimg[y + x * (tgtImg.Height - 1)] += (tgtBuf[x + (y + 2) * tgtImg.Width] - tgtBuf[x + (y - 2) * tgtImg.Width]);
                            //else
                                yDiffimg[y + x * (tgtImg.Height - 1)] += (tgtBuf[x + (y + 2) * tgtImg.Width] - tgtBuf[x + (y - 2) * tgtImg.Width]);
                        }
                        //strTmp += yDiffimg[y + x * (tgtImg.Height - 1)].ToString() + ",";
                    }
                    //wr.WriteLine(strTmp);
                }
                int debugValue = 0;
                //wr.Close();

                double xL = 0;
                double xR = 0;
                double yT = 0;
                double yB = tgtImg.Height;
                byte lupdown = 0;
                byte resUpDown = 0;
                if (iIndex == 0)
                    mUpDown[smr[si].Azimuth] = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                {
                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT, mCalcEffBand, (int)(yB - yT), ref lupdown);
                    //SetCurUpDown(0, lupdown, ref resUpDown);

                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT, mCalcEffBand, (int)(yB - yT), ref lupdown);
                    //SetCurUpDown(1, lupdown, ref resUpDown);
                    smr[si].pos.X = (xL + xR) / 2;
                }
                else if (lfMark.xPosType == 1)    //  Left Harf 
                {
                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT, mCalcEffBand, (int)(yB - yT), ref lupdown);
                    //SetCurUpDown(0, lupdown, ref resUpDown);
                    xR = tgtImg.Width - 1;
                    smr[si].pos.X = xL;
                }
                else if (lfMark.xPosType == 2)     //  Right Half
                {
                    xL = 0;
                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT, mCalcEffBand, (int)(yB - yT), ref lupdown);
                    //SetCurUpDown(1, lupdown, ref resUpDown);
                    smr[si].pos.X = xR;
                }
                else      // None
                {
                    xL = 0;
                    smr[si].pos.X = -1;
                    xR = tgtImg.Width - 1;
                }

                if (iIndex == 0)
                    mUpDown[smr[si].Azimuth] = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                if (lfMark.yPosType == 0)    //  Top Harf & Bottom Half
                {
                    lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                    yT = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0,                       xL, mCalcEffBand, (int)(xR - xL), ref lupdown);
                    //SetCurUpDown(2, lupdown, ref resUpDown);

                    lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                    yB = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0.55 * tgtImg.Height ,   xL, mCalcEffBand, (int)(xR - xL), ref lupdown);
                    //SetCurUpDown(3, lupdown, ref resUpDown);
                    smr[si].pos.Y = (yT + yB) / 2;
                }
                else if (lfMark.yPosType == 1)    //  Top Harf 
                {
                    lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                    yT = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0,                       xL, mCalcEffBand, (int)(xR - xL), ref lupdown);
                    //SetCurUpDown(2, lupdown, ref resUpDown);
                    yB = tgtImg.Height - 1;
                    smr[si].pos.Y = yT;
                }
                else if (lfMark.yPosType == 2)     //  Bottom Half
                {
                    lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                    yB = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0.55 * tgtImg.Height,    xL, mCalcEffBand, (int)(xR - xL), ref lupdown);
                    //SetCurUpDown(3, lupdown, ref resUpDown);
                    yT = 0;
                    smr[si].pos.Y = yB;
                }
                else      // None
                {
                    yT = 0;
                    smr[si].pos.Y = -1;
                    yB = tgtImg.Height - 1;
                }

                if (iIndex == 0)
                    mUpDown[smr[si].Azimuth] = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                {
                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT, mCalcEffBand, (int)(yB - yT ), ref lupdown);
                    //SetCurUpDown(0, lupdown, ref resUpDown);                                                                                 
                                                                                                                                               
                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);                                                                       
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT, mCalcEffBand, (int)(yB - yT ), ref lupdown);
                    //SetCurUpDown(1, lupdown, ref mUpDown[smr[si].Azimuth]);                                                                  
                    smr[si].pos.X = (xL + xR) / 2;                                                                                             
                }                                                                                                                              
                else if (lfMark.xPosType == 1)    //  Left Harf                                                                                
                {                                                                                                                              
                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);                                                                       
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT, mCalcEffBand, (int)(yB - yT ), ref lupdown);
                    //SetCurUpDown(0, lupdown, ref resUpDown);                                                                                 
                    xR = tgtImg.Width - 1;                                                                                                     
                    smr[si].pos.X = xL;                                                                                                        
                }                                                                                                                              
                else if (lfMark.xPosType == 2)     //  Right Half                                                                              
                {                                                                                                                              
                    xL = 0;                                                                                                                    
                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);                                                                       
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT, mCalcEffBand, (int)(yB - yT ), ref lupdown);
                    //SetCurUpDown(1, lupdown, ref resUpDown);
                    smr[si].pos.X = xR;
                }
                else      // None
                {
                    xL = 0;
                    smr[si].pos.X = -1;
                    xR = tgtImg.Width - 1;
                }

                ////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////
                ///                if (iIndex == 0)
                if (iIndex == 0)
                    mUpDown[smr[si].Azimuth] = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                if (lfMark.yPosType == 0)    //  Top Harf & Bottom Half
                {
                    lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                    yT = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0,                       xL, mCalcEffBand, (int)(xR - xL - 1), ref lupdown);
                    SetCurUpDown(2, lupdown, ref resUpDown);

                    lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                    yB = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0.55 * tgtImg.Height,    xL, mCalcEffBand, (int)(xR - xL - 1), ref lupdown);
                    SetCurUpDown(3, lupdown, ref resUpDown);

                    //if (smr[si].Azimuth==0)
                    //{
                    //    StreamWriter wwr = new StreamWriter("ydiff.csv");
                    //    string lstr = "";
                    //    for (int x = 0; x < tgtImg.Width; x++)
                    //    {
                    //        lstr = "";
                    //        for (int y = 1; y < tgtImg.Height - 1; y++)
                    //        {
                    //            lstr += yDiffimg[y + x * (tgtImg.Height - 1)].ToString() + ",";
                    //        }
                    //        wwr.WriteLine(lstr);
                    //    }
                    //    wwr.Close();
                    //}
                    smr[si].pos.Y = (yT + yB) / 2;
                }
                else if (lfMark.yPosType == 1)    //  Top Harf 
                {
                    lupdown = GetCurUpDown(2, mUpDown[smr[si].Azimuth]);
                    yT = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0,                       xL, mCalcEffBand, (int)(xR - xL - 1), ref lupdown);
                    SetCurUpDown(2, lupdown, ref resUpDown);
                    yB = tgtImg.Height - 1;
                    smr[si].pos.Y = yT;
                }
                else if (lfMark.yPosType == 2)     //  Bottom Half
                {
                    lupdown = GetCurUpDown(3, mUpDown[smr[si].Azimuth]);
                    yB = mFZM.ConvergePeakX(ref yDiffimg, tgtImg.Height - 1, tgtImg.Width, 0.55 * tgtImg.Height,    xL, mCalcEffBand, (int)(xR - xL - 1), ref lupdown);
                    SetCurUpDown(3, lupdown, ref resUpDown);
                    yT = 0;
                    smr[si].pos.Y = yB;
                }
                else      // None
                {
                    yT = 0;
                    smr[si].pos.Y = -1;
                    yB = tgtImg.Height - 1;
                }
                if (iIndex == 0)
                    lupdown = 255;
                else
                    lupdown = mUpDown[smr[si].Azimuth];

                if (lfMark.xPosType == 0)    //  Left Harf & Right Half
                {
                    if (smr[si].Azimuth == 4)
                        debugValue = 1;

                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT , mCalcEffBand, (int)(yB - yT - 1), ref lupdown);
                    SetCurUpDown(0, lupdown, ref resUpDown);

                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT  , mCalcEffBand, (int)(yB - yT - 1), ref lupdown);
                    SetCurUpDown(1, lupdown, ref resUpDown);
                    smr[si].pos.X = (xL + xR) / 2;
                }
                else if (lfMark.xPosType == 1)    //  Left Harf 
                {
                    lupdown = GetCurUpDown(0, mUpDown[smr[si].Azimuth]);
                    xL = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0,                       yT , mCalcEffBand, (int)(yB - yT - 1), ref lupdown);
                    SetCurUpDown(0, lupdown, ref resUpDown);
                    xR = tgtImg.Width - 1;
                    smr[si].pos.X = xL;
                }
                else if (lfMark.xPosType == 2)     //  Right Half
                {
                    xL = 0;
                    lupdown = GetCurUpDown(1, mUpDown[smr[si].Azimuth]);
                    xR = mFZM.ConvergePeakX(ref xDiffimg, tgtImg.Width - 1, tgtImg.Height, 0.55 * tgtImg.Width,      yT  , mCalcEffBand, (int)(yB - yT - 1), ref lupdown);
                    SetCurUpDown(1, lupdown, ref resUpDown);
                    smr[si].pos.X = xR;
                }
                else      // None
                {
                    xL = 0;
                    smr[si].pos.X = -1;
                    xR = tgtImg.Width - 1;
                }
                mUpDown[smr[si].Azimuth] = resUpDown;
                if (smr[si].pos.X>0 && smr[si].pos.Y > 0)
                {
                    smr[si].pos.X += subLeft;    //  중심좌표
                    smr[si].pos.Y += subTop;     //  중심좌표
                }
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
            }
            return 0;
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

        public long CalcConv(ref sFiducialMark lfmark, int[] mimg, int lwidth, int lheight, int sub, ref double subconv, int iDetectedMark)
        {
            int minBin = 99;
            int maxBin = 0;
            int[] srcData = new int[lwidth * lheight];
            int[] sorted = new int[lwidth * lheight];

            for (int j = 0; j < lheight; j++)
                for (int i = 0; i < lwidth; i++)
                    sorted[i + j * lwidth] = mimg[ i + ( j) * lwidth];

            Array.Sort(sorted);

            int bin2pro = (int)(0.98 * lwidth * lheight);
            int bin5pro = (int)(0.95 * lwidth * lheight);
            int bin80pro = (int)(lwidth * lheight / 5);
            int bin95pro = (int)(lwidth * lheight / 20);

            minBin = sorted[bin80pro];
            maxBin = sorted[bin5pro];

            minBin = (maxBin + minBin) / 2;

            if (maxBin - minBin < 15)
                return 0;

            long res = 0;

            for (int j = 0; j < lheight; j++)
                for (int i = 0; i < lwidth; i++)
                {
                    srcData[i + j * lwidth] = mimg[ i + ( j) * lwidth];

                    if (mimg[ i + ( j) * lwidth] < minBin - 1)
                        res -= lfmark.img[i + lwidth * j];  //  * -1
                    else if (mimg[ i + ( j) * lwidth] > minBin + 2)
                        res += lfmark.img[i + lwidth * j];  //  * +1
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
            return res;
        }
        //public long CalcConv(int model, int x, int y, int iBuf, ref double subconv, bool blast = false)
        public long CalcConv(ref sFiducialMark lfmark, int x, int y, int iBuf, ref double subconv, bool blast = false)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            long res = 0;
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;
            int bufLength = lwidth * lheight;
            byte[] srcData = new byte[bufLength];
            byte[] sorted = new byte[bufLength];

            try
            {
                int quaterWidth = mSourceImg[iBuf].Width / lfmark.MScale;
                int minBin = 99;
                int maxBin = 0;
                int ip = 0;
                int yj = 0;

                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    for (int i = 0; i < lwidth; i++)
                    {
                        ip = i + j * lwidth;
                        sorted[ip] = q_Value[iBuf][x + i + yj];
                        srcData[ip] = sorted[ip];
                    }
                }

                //Array.Copy(sorted, srcData, bufLength);
                Array.Sort(sorted);

                //int bin2pro = (int)(0.98 * bufLength);
                int bin5pro = (int)(0.95 * bufLength);
                int bin80pro = (int)(bufLength / 5);
                //int bin95pro = (int)(bufLength / 20);

                minBin = sorted[bin80pro];
                maxBin = sorted[bin5pro];

                minBin = (maxBin + minBin) / 2;

                if (maxBin - minBin < 15)
                    return -5000;


                int minBinMinus1 = minBin - 1;
                int minBinPlus2 = minBin + 2;
                int q_cur = 0;
                int j_half = lheight / 2;
                int j_half2 = (int)(lheight * 0.7);
                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    for (int i = 0; i < lwidth; i++)
                    {
                        q_cur = q_Value[iBuf][x + i + yj];

                        if (q_cur < minBinMinus1)
                            res -= lfmark.img[i + lwidth * j];  //  * -1
                        else if (q_cur > minBinPlus2)
                            res += lfmark.img[i + lwidth * j];  //  * +1
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


            subconv = 0;
            if (res > lfmark.conv)
            {
                switch (lfmark.sub % 100)
                {
                    case 1:
                        mCalcDiffConv(srcData, ref lfmark, ref subconv);
                        break;
                    case 2:
                        mCalcXDiffConv(srcData, ref lfmark, ref subconv);
                        break;
                    case 3:
                        mCalcYDiffConv(srcData, ref lfmark, ref subconv);
                        break;
                    case 4:
                        mCalcInOutAvg(srcData, ref lfmark, ref subconv);
                        break;
                    case 5:
                        mCalcLeftRightAvg(srcData, ref lfmark, ref subconv);
                        break;
                    case 6:
                        mCalcTopBottomAvg(srcData, ref lfmark, ref subconv);
                        break;
                    case 7:
                        mCalcCenterToLeftRightAvg(srcData, ref lfmark, ref subconv);
                        break;
                    case 8:
                        mCalcCenterToTopBottomAvg(srcData, ref lfmark, ref subconv);
                        break;
                    case 9:
                        mCalcFullStdev(srcData, ref lfmark, ref subconv);
                        break;
                    case 10:
                        mCalcInFullStdev(srcData, ref lfmark, ref subconv);
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        public long CalcConv(int lwidth, int lheight, int modelScale, ref int[] img, int x, int y, int iBuf, ref double subconv, bool blast = false)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            long res = 0;
            //int lwidth = modelWidth;// lfmark.modelSize.Width;
            //int lheight = modelHeight;// lfmark.modelSize.Height;
            int bufLength = lwidth * lheight;
            byte[] srcData = new byte[bufLength];
            byte[] sorted = new byte[bufLength];

            try
            {
                int quaterWidth = mSourceImg[iBuf].Width / modelScale;// lfmark.MScale;
                int minBin = 99;
                int maxBin = 0;
                int ip = 0;
                int yj = 0;

                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    for (int i = 0; i < lwidth; i++)
                    {
                        ip = i + j * lwidth;
                        sorted[ip] = q_Value[iBuf][x + i + yj];
                        //srcData[ip] = sorted[ip];
                    }
                }

                //Array.Copy(sorted, srcData, bufLength);
                Array.Sort(sorted);

                int bin5pro = (int)(0.95 * bufLength);
                int bin80pro = (int)(bufLength / 5);

                minBin = sorted[bin80pro];
                maxBin = sorted[bin5pro];

                minBin = (maxBin + minBin) / 2;

                //  Skip When Low Contrast 
                if (maxBin - minBin < 10)
                    return -1;


                int minBinMinus1 = minBin - 1;
                int minBinPlus2 = minBin + 2;
                int q_cur = 0;
                int j_half = lheight / 2;
                int j_half2 = (int)(lheight * 0.7);
                for (int j = 0; j < lheight; j++)
                {
                    yj = (y + j) * quaterWidth;
                    for (int i = 0; i < lwidth; i++)
                    {
                        q_cur = q_Value[iBuf][x + i + yj];

                        if (q_cur < minBinMinus1)
                            res -= img[i + lwidth * j];  //  * -1
                        else if (q_cur > minBinPlus2)
                            res += img[i + lwidth * j];  //  * +1
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
            return res;
        }
        public long mCalcConv(byte[] srcData, ref sFiducialMark lfmark)
        {
            //  Preprocess for q_Value to have -1,0,1 only
            int lwidth = lfmark.modelSize.Width;
            int lheight = lfmark.modelSize.Height;
            long res = 0;
            int bufLength = lwidth * lheight;

            int minBin = 99;
            int maxBin = 0;
            byte[] sorted = new byte[srcData.Length];
            Array.Copy(srcData, sorted, bufLength);

            Array.Sort(sorted);

            int bin5pro = (int)(0.95 * bufLength);
            int bin80pro = (int)(bufLength / 5);

            minBin = sorted[bin80pro];
            maxBin = sorted[bin5pro];

            minBin = (maxBin + minBin) / 2;

            ////  모델 만들때는 Skip 할 필요 없음.
            ////  Skip When Low Contrast 
            //if (maxBin - minBin < 10)
            //    return -1;

            int minBinMinus1 = minBin - 1;
            int minBinPlus2 = minBin + 2;
            int q_cur = 0;
            //int j_half = lheight / 2;
            //int j_half2 = (int)(lheight * 0.7);
            for (int j = 0; j < lheight; j++)
            {
                for (int i = 0; i < lwidth; i++)
                {
                    q_cur = srcData[i + j* lwidth];

                    if (q_cur < minBinMinus1)
                        res -= lfmark.img[i + lwidth * j];  //  * -1
                    else if (q_cur > minBinPlus2)
                        res += lfmark.img[i + lwidth * j];  //  * +1
                }

                ////  모델 만들때는 시간단축 무의미
                //if (j == j_half)
                //{
                //    if (res < 0)
                //        return 2 * res;
                //}
                //else if (j == j_half2)
                //{
                //    if (res < 0)
                //        return (int)(1.4 * res);
                //}
            }
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
                double xstart = (lRectangle.X + 1) / (double)pictureBox2.Size.Width * mSourceImg[0].Width;
                double ystart = (lRectangle.Y + 1) / (double)pictureBox2.Size.Height * mSourceImg[0].Height;

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
                    int left = mSelectRect.X - (int)(lwidth / 31.429);
                    int Rwidth = (int)(lwidth / 15.714);
                    if (left < 0)
                        left = 0;

                    if (mCurrentScell.X > 0)
                    {
                        dgvDesignNModelSide.Rows[mCurrentScell.Y].Cells[mCurrentScell.X].Value = (mSelectRect.Width/mModelScale).ToString() + "," + (mSelectRect.Height/ mModelScale).ToString();
                        mFidMarkSide[mCurrentScell.Y].modelSize = new OpenCvSharp.Size(mSelectRect.Width/ mModelScale, mSelectRect.Height/ mModelScale);

                        int top = mSelectRect.Y - (int)(lheight / 48.894);
                        int Rheight = (int)(lheight /24.447);
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
                    //  ROI 지정
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
                    mSearchModel[i].sub = mNewSearchModel[i].sub;
                    mSearchModel[i].subconv = mNewSearchModel[i].subconv;
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
            DetectInAllFiles();
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

                DetectInSelectedFile();
                i++;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            mCst.TrackRubberBand(pictureBox2, 0, -1);
            mSelectRect.Y--;
            if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
            {
                Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                //	영상클립

                mCustomImg = mSourceImg[0].SubMat(roi);
                //	보여주기
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                pictureBox5.Image = myImage;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateMousePosition();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            mCst.TrackRubberBand(pictureBox2, -1, 0);
            mSelectRect.X--;
            if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
            {
                Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                //	영상클립

                mCustomImg = mSourceImg[0].SubMat(roi);
                //	보여주기
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                pictureBox5.Image = myImage;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateMousePosition();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            mCst.TrackRubberBand(pictureBox2, 1, 0);
            mSelectRect.X++;
            if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
            {
                Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                //	영상클립

                mCustomImg = mSourceImg[0].SubMat(roi);
                //	보여주기
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                pictureBox5.Image = myImage;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateMousePosition();
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            mCst.TrackRubberBand(pictureBox2, 0, +1);
            mSelectRect.Y++;
            if (mSelectRect.Width > 0 && mSelectRect.Height > 0)
            {
                Rect roi = new Rect((int)mSelectRect.X, (int)mSelectRect.Y, mSelectRect.Width, mSelectRect.Height);
                //	영상클립

                mCustomImg = mSourceImg[0].SubMat(roi);
                //	보여주기
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mCustomImg);
                pictureBox5.Image = myImage;
                pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                UpdateMousePosition();
            }
        }
        public int mModelScale = 4;

        List<OpenCvSharp.Size> mSizeFidMarkTop = new List<OpenCvSharp.Size>();
        List<OpenCvSharp.Size> mSizeFidMarkSide = new List<OpenCvSharp.Size>();
        private void button5_Click(object sender, EventArgs e)
        {
            if (mSelectRect.Width == 0)
            {
                MessageBox.Show("Try after making ROI");
                return;
            }
            if (mCurrentScell.X != 4 && mCurrentTcell.X != 4)
                return;

            mModelScale = 4 - lbModelScale.SelectedIndex;

            double lmodelScale = mModelScale;
            byte[] srcData = null;
            Mat qSubImg = new Mat();
            Cv2.Resize(mCustomImg, qSubImg, new OpenCvSharp.Size(mCustomImg.Width / mModelScale, mCustomImg.Height / mModelScale), 1.0/ mModelScale, 1.0 / mModelScale, InterpolationFlags.Area);  //  1/mModelScale 축소
            qSubImg.GetArray(out srcData);

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
            }
            else
            {
                lfMark = mFidMarkTop[modelNo-8];
            }
            //  Create Model Data
            //  Edge 주변만 살리고 나머지 축이는 방식 검토 필요.
            CreateModelFromSubImg(srcData, lwidth, lheight, lmodelScale, ref lfMark, true);


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
                tmpBytes[i] = (byte)(6 * lfMark.img[i] + 120);

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
        }
        public void CreateModelFromSubImg(byte[] srcData, int lwidth, int lheight, double lmodelScale, ref sFiducialMark lfMark, bool applyIgnoreRC)
        {
            lfMark.modelSize.Width = lwidth;
            lfMark.modelSize.Height = lheight;
            lfMark.conv = 0;
            lfMark.sub = 0;
            lfMark.MScale = (int)lmodelScale;
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
                for (int x = 0; x < lwidth; x++)
                    for (int y = 0; y < lheight; y++)
                    {
                        lAvg[x + y * lwidth] = srcData[x + y * lwidth];
                        lbrSort.Add(lAvg[x + y * lwidth]);
                    }
            }

            int[] brSort = lbrSort.ToArray();
            Array.Sort(brSort);
            int bin85pro = brSort.Length / 7;
            int bin5pro = (int)(brSort.Length * 0.95);
            int bin2pro = (int)(brSort.Length * 0.98);

            int lmin = brSort[bin85pro];
            int lmax = brSort[bin5pro];
            int lavg = (lmin + lmax) / 2 - 2;

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
            for (int i = 0; i < lwidth * lheight; i++)
                lfMark.img[i] = lSort[i];

            //  Calculate Self Conv
            lfMark.conv = (int)(0.8 * (int)mCalcConv(srcData, ref lfMark));
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (mLastFMIfile == "")
            {
                var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

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

            for ( i = 0; i < mFidMarkSide.Count; i++)
                mSizeFidMarkSide[i]  = new OpenCvSharp.Size(mFidMarkSide[i].modelSize.Width, mFidMarkSide[i].modelSize.Height) ;

            for ( i = 0; i < mFidMarkTop.Count; i++)
                mSizeFidMarkTop[i] = new OpenCvSharp.Size(mFidMarkTop[i].modelSize.Width, mFidMarkTop[i].modelSize.Height);
            
            //  여기서 modelSize 가 제멋대로 0 으로 변경됨.
            if (mCurrentScell.X >= 0)
            {
                i = mCurrentScell.Y;
                dgvDesignNModelSide.Rows[i].Cells[6].Value = mFidMarkSide[i].conv.ToString();
            }
            else if ((mCurrentTcell.X >= 0))
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
            if (tbTargetConv.Text.Length > 0 || tbTargetSubConv.Text.Length > 0)
            {
                string lstr = "";
                int i = 0;
                if (mCurrentScell.X >= 0)
                {
                    i = mCurrentScell.Y;
                    OpenCvSharp.Size lmSize = new OpenCvSharp.Size(mFidMarkSide[i].modelSize.Width, mFidMarkSide[i].modelSize.Height);
                    if (tbTargetConv.Text.Length > 0)
                        mFidMarkSide[i].conv = Convert.ToInt16(tbTargetConv.Text);
                    if (tbSubConv.Text.Length > 0)
                        mFidMarkSide[i].sub = Convert.ToInt16(tbSubConv.Text);
                    if (tbTargetSubConv.Text.Length > 0)
                        mFidMarkSide[i].subconv = Convert.ToDouble(tbTargetSubConv.Text);
                    dgvDesignNModelSide.Rows[i].Cells[6].Value = tbTargetConv.Text;
                    dgvDesignNModelSide.Rows[i].Cells[7].Value = tbSubConv.Text;
                    dgvDesignNModelSide.Rows[i].Cells[8].Value = tbTargetSubConv.Text;

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
                    if (tbSubConv.Text.Length > 0)
                        mFidMarkTop[i].sub = Convert.ToInt16(tbSubConv.Text);
                    if (tbTargetSubConv.Text.Length > 0)
                        mFidMarkTop[i].subconv = Convert.ToDouble(tbTargetSubConv.Text);

                    dgvDesignNModelTop.Rows[i].Cells[6].Value = tbTargetConv.Text;
                    dgvDesignNModelTop.Rows[i].Cells[7].Value = tbSubConv.Text;
                    dgvDesignNModelTop.Rows[i].Cells[8].Value = tbTargetSubConv.Text;

                    //  값이 변했는지 안변했는지 확인하기 위한 목적
                    mFidMarkTop[i].modelSize = lmSize;
                    lmSize = mFidMarkTop[i].modelSize;
                    //dgvDesignNModelTop.Rows[i].Cells[5].Value = sRoi.X.ToString() + "," + sRoi.Y.ToString() + "," + sRoi.Width.ToString() + "," + sRoi.Height.ToString();
                }

                //SaveModelFile();
                SaveFiducialMark();
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
            dgvDesignNModelSide.Columns.Add("Sub"       , "Sub"       );
            dgvDesignNModelSide.Columns.Add("Subconv"   , "Subconv"   );

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
            dgvDesignNModelTop.Columns.Add("Sub"       , "Sub"       );
            dgvDesignNModelTop.Columns.Add("Subconv"   , "Subconv"   );
                           
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

            ApplyFiducialMarkInfo();
            if (File.Exists("LastDesignInfo.png"))
            {
                Mat tmpImg = new Mat("LastDesignInfo.png", ImreadModes.Grayscale);
                Bitmap myImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmpImg);

                pictureBox3.Image = myImage;
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            }
            panel1.Hide();
            panel1Shown = false;

        }

        private void button20_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void pictureBox6_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox6_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (File.Exists("LastDesignInfo.png"))
            {
                Mat tmpImg = new Mat("LastDesignInfo.png", ImreadModes.Grayscale);
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

        public string[] mYPosType = new string[4]
        {
            "H edge in Upper/Lower Half", "H edge in Upper Half", "H edge in Lower Half", "None"
        };

        public string[] mXPosType = new string[4]
        {
            "V edge in Left/Right Half", "V edge in Left Half", "V edge in Right Half", "None"
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
                    dgvDesignNModelSide.Rows[FMSindex].Cells[7].Value = mFidMarkSideF[FMSindex].sub.ToString();
                    dgvDesignNModelSide.Rows[FMSindex].Cells[8].Value = mFidMarkSideF[FMSindex].subconv.ToString();

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
                    dgvDesignNModelTop.Rows[FMSindex].Cells[7].Value = mFidMarkTopF[FMSindex].sub.ToString();
                    dgvDesignNModelTop.Rows[FMSindex].Cells[8].Value = mFidMarkTopF[FMSindex].subconv.ToString();

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
            int index = dgvDesignNModelSide.CurrentRow.Index;

            if (mCsmSide.Count <= index)
            {
                if (dgvDesignNModelSide.Rows.Count > index+1)
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
                        if (mCsmTop[i].Type == mCsmSide[index].Type+8)  //  0,1 -> 8,9
                        {
                            mCsmTop[i].Destroy();
                            mCsmTop.RemoveAt(i);
                            dgvDesignNModelTop.Rows.RemoveAt(i);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][0]);
                            pictureBox6.Controls.Remove(mMarkerPosInputT[i][1]);
                            mMarkerPosInputT.RemoveAt(i);
                        }
                    }
                    else if (mCsmSide[index].Type==4 || mCsmSide[index].Type==5)
                    {
                        if (mCsmTop[i].Type == mCsmSide[index].Type+6)  //  4,5 -> 10, 11
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
                            if (mFidMarkTopF.Count >i)
                                mFidMarkTopF.RemoveAt(i);

                            break;
                        }
                    }
                    else if (mCsmSide[index].Type==4 || mCsmSide[index].Type==5)
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

        public double mOpticsTgtOffset = 0.284;     //  on Image Plane ( == Real World )
        public double mOpticsTopViewOffset = 3.5065; //  Center of Side View 에 해당하는 Target 의 Point 가 Top View 영상의 중심에서 얼마나 떨어져있는가 ( Top View 기준 )
        public double mOpticsFWOffset = 0.164;

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
                catch(Exception exp)
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
                catch (Exception exp)
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
                catch (Exception exp)
                {
                    MessageBox.Show("Offset value is not correct.");
                    return;
                }
            }
            mOpticsTgtOffset = tgtOffset;
            mOpticsTopViewOffset = topviewOffset;
            mOpticsFWOffset = focusWindowOffset;

            StreamWriter wr = new StreamWriter("OpticsConfig.txt");
            wr.WriteLine(mOpticsTgtOffset.ToString("F3"));
            wr.WriteLine(mOpticsTopViewOffset.ToString("F3"));
            wr.WriteLine(mOpticsFWOffset.ToString("F3"));
            wr.Close();
        }
        public void LoadOpticsConfig()
        {
            if (!File.Exists("OpticsConfig.txt"))
                return;

            StreamReader wr = new StreamReader("OpticsConfig.txt");
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

            string lstr = "";
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
                    mMarkerPosInputS.RemoveAt(lCurCsmIndex);
                    pictureBox6.Controls.Remove(mMarkerPosInputS[lCurCsmIndex][0]);
                    pictureBox6.Controls.Remove(mMarkerPosInputS[lCurCsmIndex][1]);
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
            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Schematic"));

            string sFilePath = folder;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            openFileDialog1.Multiselect = false;
            openFileDialog1.InitialDirectory = sFilePath;
            openFileDialog1.FilterIndex = 2;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //Image myImage = Image.FromFile(filename);
            mLastSchematicFile = openFileDialog1.FileName;

            ShowSchematics();
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
            ApplyFiducialMarkInfo();

        }

        public void ApplyFiducialMarkInfo()
        {
            //  모든 TextBox 의 데이터를 각 변수에 저장한다.
            int countErr = 0;
            string lstr = "";
            string[] laz = new string[6] { "N", "W", "S", "E", "Nt", "St" };

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
            }
            for (int i = 0; i < mFidMarkTop.Count; i++)
            {
                for (int j = 0; j < mFidMarkSide.Count; j++)
                {
                    if ((mFidMarkTop[i].Azimuth == 0 && mFidMarkSide[j].Azimuth == 0) || (mFidMarkTop[i].Azimuth == 1 && mFidMarkSide[j].Azimuth == 1))
                    {

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
            rtbLog.Text += lstr;
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();

            if (countErr > 0)
            {
                rtbLog.Text += "\r\nEmpty position data!\r\n";
            }

            MakeDesignInfoToImage();
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
                captureBitmap.Save("LastDesignInfo.png", ImageFormat.Png);
                //Displaying the Successfull Result
                //MessageBox.Show("Screen Captured");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
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

        private void btnUD_Click(object sender, EventArgs e)
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

        private void btnDU_Click(object sender, EventArgs e)
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

        private void btnDD_Click(object sender, EventArgs e)
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

        private void btnLL_Click(object sender, EventArgs e)
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

        private void btnLR_Click(object sender, EventArgs e)
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

        private void btnRL_Click(object sender, EventArgs e)
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

        private void btnRR_Click(object sender, EventArgs e)
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

            if (XMLfilename != "")
                mLastFMIfile = XMLfilename;

            StreamWriter wr = new StreamWriter("LastFMIfile.txt");
            wr.WriteLine(mLastFMIfile);
            wr.Close();
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
                if (tmp[2 * i] != null && tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D((tmp[2 * i].X + tmp[2 * i + 1].X) / 2, (tmp[2 * i].Y + tmp[2 * i + 1].Y) / 2);
                else if (tmp[2 * i] != null)
                    mMarkNorm[i] = new FZMath.Point2D(tmp[2 * i].X, tmp[2 * i].Y);
                else if (tmp[2 * i + 1] != null)
                    mMarkNorm[i] = new FZMath.Point2D(tmp[2 * i + 1].X, tmp[2 * i + 1].Y);
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
        public void LoadLastFMI()
        {
            if (!File.Exists("LastFMIfile.txt"))
                return;

            StreamReader rd = new StreamReader("LastFMIfile.txt");
            string lstr = rd.ReadLine();
            rd.Close();

            mLastFMIfile = lstr;
            if (!File.Exists(mLastFMIfile))
                return;

            sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            if (lfm.Length < 1)
                return;

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
        private void btnLoadFMI_Click(object sender, EventArgs e)
        {
            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

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

            LoadFMIFile(mLastFMIfile);
            //sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            //if (lfm.Length < 1)
            //    return;

            //label6.Text = mLastFMIfile;

            //StreamWriter wr = new StreamWriter("LastFMIfile.txt");
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
        public void ExternalLoadFMIFile(string sFile)
        {
            //  일단 최초에 로딩 된 뒤부터 유효하게 동작해야 한다.
            if (!mLastFMIisLoaded)
                return;

            mLastFMIfile = sFile;
            LoadFMIFile(mLastFMIfile);
        }
        List<List<sFiducialMark>> mFMSideCandidate = new List<List<sFiducialMark>>();
        List<List<sFiducialMark>> mFMTopCandidate = new List<List<sFiducialMark>>();
        public void LoadFMICandidate()
        {
            mFMSideCandidate = new List<List<sFiducialMark>>();
            mFMTopCandidate = new List<List<sFiducialMark>>();
            foreach ( string fname in mFMICanditates)
            {
                //if (fname == mLastFMIfile)
                //    continue;

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

        public void LoadFMIFile(string filename)
        {
            sFiducialMark[] lfm = LoadFiducialMark(mLastFMIfile);
            if (lfm.Length < 1)
                return;


            StreamWriter wr = new StreamWriter("LastFMIfile.txt");
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
        }
        public void ShowMarkDGV()
        {
            if (mFidMarkSideF.Count == 0 || mFidMarkTopF.Count == 0)
                return;

            mLastSchematicFile = mFidMarkSideF[0].fidInfo.schematicFile;
            lbModelScale.SelectedIndex = 4 - mFidMarkSideF[0].MScale;
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
            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

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
            label6.Text = mLastFMIfile;

            SaveMarkPosOnPanel();
        }
        private void dgvDesignNModel_SelectionChanged(object sender, EventArgs e)
        {
            DesignModelSideChanged(sender, 0);
        }
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

            for (int i = 0; i < tmpBytes.Length; i++)
                tmpBytes[i] = (byte)(6 * lfMark.img[i] + 120);

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

        }
        private void dgvDesignNModelTop_SelectionChanged(object sender, EventArgs e)
        {
            DesignModelSideChanged(sender, 1);
        }

        private void button9_Click_1(object sender, EventArgs e)
        {

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
            double sec40 = 1 / Math.Cos(40.0 / 180 * Math.PI);


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

            mFZM.ZPhiThetaTransferMatrix(mMarkNorm, out mFZM.M6DM, out mFZM.M6DMwT);
            mFZM.GetC12C13fromP12P13(mMarkNorm[0], mMarkNorm[2], mMarkNorm[3], ref mFZM.mC12, ref mFZM.mC13);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            double psi = 0;
            double dZ = 0;
            double TX = 0;
            double TY = 0;
            int i = 0;

            FZMath.Point2D[][] markT = new FZMath.Point2D[20][];
            FZMath.Point2D[][] markS = new FZMath.Point2D[20][];
            FZMath.Point2D refT = new FZMath.Point2D();

            //  Norminal 위치 : 물리적 위치이므로 Top View 좌표계 활용, 
            //  Norminal 위치는 CIS 를 (0,0) 으로 하여 um 단위를 Pixel 단위로  변환해줘야 한다.

            //mMarkNorm[0] = new FZMath.Point2D( + 300, + 155.5723827);
            //mMarkNorm[1] = null;
            //mMarkNorm[2] = new FZMath.Point2D( - 300, + 155.5723827);
            //mMarkNorm[3] = new FZMath.Point2D( + 0,   - 155.5723827);

            //for ( i=0; i<20; i++)
            //{
            //    markT[i] = new FZMath.Point2D[2];
            //    markS[i] = new FZMath.Point2D[4];
            //}


            //markT[0][0] = new FZMath.Point2D(); //  North
            //markT[0][1] = new FZMath.Point2D(); //  South

            //markT[1][0] = new FZMath.Point2D(); //  North
            //markT[1][1] = new FZMath.Point2D(); //  South

            //markS[0][0] = new FZMath.Point2D(); //  North
            //markS[0][1] = null; //  West
            //markS[0][2] = new FZMath.Point2D(); //  South
            //markS[0][3] = new FZMath.Point2D(); //  East

            //markS[1][0] = new FZMath.Point2D(); //  North
            //markS[1][1] = null; //  West
            //markS[1][2] = new FZMath.Point2D(); //  South
            //markS[1][3] = new FZMath.Point2D(); //  East

            ////  제자리
            //markS[0][0] = new FZMath.Point2D(480 + 300, 180 + 100);    //  North
            //markS[0][1] = null;                            //  West
            //markS[0][2] = new FZMath.Point2D(480 -300, 180 + 100);   //  South
            //markS[0][3] = new FZMath.Point2D(480 + 0, 180-100);     //  East

            //markS[1][0] = new FZMath.Point2D(480 + 300, 180 + 100);    //  North
            //markS[1][1] = null;                            //  West
            //markS[1][2] = new FZMath.Point2D(480 - 300, 180 + 100);   //  South
            //markS[1][3] = new FZMath.Point2D(480 + 0, 180 -100);     //  East

            //markT[0][0] = new FZMath.Point2D(480 + 300 , 180 - 90);    //  North
            //markT[0][1] = new FZMath.Point2D(480 - 300 , 180 - 90);     //  East

            //markT[1][0] = new FZMath.Point2D(480 + 300, 180 - 90);    //  North
            //markT[1][1] = new FZMath.Point2D(480 - 300 , 180 - 90);     //  East

            ////  X 이동 0.7
            //markS[2][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 + 100);    //  North
            //markS[2][1] = null;                            //  West
            //markS[2][2] = new FZMath.Point2D(480 - 300 + 0.7, 180 + 100);   //  South
            //markS[2][3] = new FZMath.Point2D(480 + 0 + 0.7, 180 - 100);     //  East

            //markT[2][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 - 90);    //  North
            //markT[2][1] = new FZMath.Point2D(480 - 300 + 0.7, 180 - 90);     //  East

            ////  Y 이동 0.7
            //markS[3][0] = new FZMath.Point2D(480 + 300, 180 + 100 + 0.44995);    //  North
            //markS[3][1] = null;                            //  West
            //markS[3][2] = new FZMath.Point2D(480 - 300, 180 + 100 + 0.44995);   //  South
            //markS[3][3] = new FZMath.Point2D(480 + 0, 180 - 100 + 0.44995);     //  East

            //markT[3][0] = new FZMath.Point2D(480 + 300, 180 - 90 + 0.7);    //  North
            //markT[3][1] = new FZMath.Point2D(480 - 300, 180 - 90 + 0.7);     //  East

            ////  Z 이동 0.7
            //markS[4][0] = new FZMath.Point2D(480 + 300, 180 + 100 + 0.53623111);    //  North
            //markS[4][1] = null;                            //  West
            //markS[4][2] = new FZMath.Point2D(480 - 300, 180 + 100 + 0.53623111);   //  South
            //markS[4][3] = new FZMath.Point2D(480 + 0, 180 - 100 + 0.53623111);     //  East

            //markT[4][0] = new FZMath.Point2D(480 + 300, 180 - 90);    //  North
            //markT[4][1] = new FZMath.Point2D(480 - 300, 180 - 90);     //  East

            ////  TZ 회전 0.003rad
            //markS[5][0] = new FZMath.Point2D(480 + 300 - 0.468066447, 180 + 100 + 0.578057981);    //  North
            //markS[5][1] = null;                            //  West
            //markS[5][2] = new FZMath.Point2D(480 - 300 - 0.465366449, 180 + 100 - 0.578957981);   //  South
            //markS[5][3] = new FZMath.Point2D(480 + 0.466716448, 180 - 100 + 0.00045 );     //  East

            //markT[5][0] = new FZMath.Point2D(480 + 300 - 0.468066447, 180 - 90 + 0.899298575);    //  North
            //markT[5][1] = new FZMath.Point2D(480 - 300 - 0.465366449, 180 - 90 - 0.900698725);     //  East

            ////  TX 회전 -0.003rad
            //markS[6][0] = new FZMath.Point2D(480 + 300, 180 + 100 + 20 * 0.357525542);    //  North
            //markS[6][1] = null;                            //  West
            //markS[6][2] = new FZMath.Point2D(480 - 300, 180 + 100 + 20 * 0.357525542);   //  South
            //markS[6][3] = new FZMath.Point2D(480 + 0, 180 - 100 - 20 * 0.357525542);     //  East

            //markT[6][0] = new FZMath.Point2D(480 + 300, 180 - 90 - 20 * 0.000700075);    //  North
            //markT[6][1] = new FZMath.Point2D(480 - 300, 180 - 90 - 20 * 0.000700075);     //  East

            ////  TY 회전
            //markS[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 + 100 + 0.689438965);    //  North
            //markS[7][1] = null;                            //  West
            //markS[7][2] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 + 100 - 0.689438965);   //  South
            //markS[7][3] = new FZMath.Point2D(480 + 0, 180 - 100);     //  East

            //markT[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 - 90);    //  North
            //markT[7][1] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 - 90);     //  East

            ////  X 0.7 + Y 0.7 + Z 0.7
            //markS[8][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 + 100 + 0.986182437);    //  North
            //markS[8][1] = null;                            //  West
            //markS[8][2] = new FZMath.Point2D(480 - 300 + 0.7, 180 + 100 + 0.986182437);   //  South
            //markS[8][3] = new FZMath.Point2D(480 + 0 + 0.7, 180 - 100 + 0.986182437);     //  East

            //markT[8][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 - 90 + 0.7);    //  North
            //markT[8][1] = new FZMath.Point2D(480 - 300 + 0.7, 180 - 90 + 0.7);     //  East

            ////  X 0.7 + Y 0.7 + Z 0.7 + TZ 0.003rad
            //markS[9][0] = new FZMath.Point2D(480 + 300 + 0.231933553, 180 + 100 + 1.564240418);    //  North
            //markS[9][1] = null;                            //  West
            //markS[9][2] = new FZMath.Point2D(480 - 300 + 0.234633551, 180 + 100 + 0.407224456);   //  South
            //markS[9][3] = new FZMath.Point2D(480 + 0 + 1.166716448, 180 - 100 + 0.985732437);     //  East

            //markT[9][0] = new FZMath.Point2D(480 + 300 + 0.231933553, 180 - 90 + 1.599298575);    //  North
            //markT[9][1] = new FZMath.Point2D(480 - 300 + 0.234633551, 180 - 90 - 0.200698725);     //  East

            ///////////////////////////////////////////////////////////////////////////////////////////
            ////  X + Y + Z + TX
            //markS[10][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 + 100 + 1.343707978);    //  North
            //markS[10][1] = null;                                        //  West
            //markS[10][2] = new FZMath.Point2D(480 - 300 + 0.7, 180 + 100 + 1.343707978);   //  South
            //markS[10][3] = new FZMath.Point2D(480 + 0   + 0.7, 180 - 100 + 0.628656895);     //  East

            //markT[10][0] = new FZMath.Point2D(480 + 300 + 0.7, 180 - 90 + 0.7 );    //  North
            //markT[10][1] = new FZMath.Point2D(480 - 300 + 0.7, 180 - 90 + 0.7 );     //  East

            ////  X + Y + Z + TY
            //markS[11][0] = new FZMath.Point2D(480 + 300 + 0.698650001, 180 + 100 + 1.675621402);    //  North
            //markS[11][1] = null;                            //  West
            //markS[11][2] = new FZMath.Point2D(480 - 300 + 0.701349999, 180 + 100 + 0.296743472);   //  South
            //markS[11][3] = new FZMath.Point2D(480 + 0   + 0.7, 180 - 100 + 0.986182437);     //  East

            //markT[11][0] = new FZMath.Point2D(480 + 300 + 0.6986500017, 180 - 90 + 0.7);    //  North
            //markT[11][1] = new FZMath.Point2D(480 - 300 + 0.701349999, 180 - 90 + 0.7);     //  East

            ///////////////////////////////////////////////////////////////////////////////////////////
            ////  X + Y + Z + TX + TY
            //markS[12][0] = new FZMath.Point2D(480 + 300 + 0.698650001, 180 + 100 + 2.03314643);    //  North
            //markS[12][1] = null;                            //  West
            //markS[12][2] = new FZMath.Point2D(480 - 300 + 0.701349999, 180 + 100 + 0.654269014);   //  South
            //markS[12][3] = new FZMath.Point2D(480 + 0.7, 180 - 100 + 0.628656895);     //  East

            //markT[12][0] = new FZMath.Point2D(480 + 300 + 0.698650001, 180 - 90 + 0.699299925);    //  North
            //markT[12][1] = new FZMath.Point2D(480 - 300 + 0.701349999, 180 - 90 + 0.699299925);     //  East

            ////  X + Y + Z + TZ + TY
            //markS[13][0] = new FZMath.Point2D(480 + 300 + 0.230583554, 180 + 100 + 2.253679383);    //  North
            //markS[13][1] = null;                            //  West
            //markS[13][2] = new FZMath.Point2D(480 - 300 + 0.23598355, 180 + 100 - 0.282214508);   //  South
            //markS[13][3] = new FZMath.Point2D(480 + 0 + 1.166716448, 180 - 100 + 0.985732437);     //  East

            //markT[13][0] = new FZMath.Point2D(480 + 300 + 0.230583554, 180 - 90 + 1.599298575);    //  North
            //markT[13][1] = new FZMath.Point2D(480 - 300 + 0.23598355, 180 - 90 - 0.200698726);     //  East

            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////   여기부터 값을 만들어줘야 한다.
            ////  X + Y + Z + TX + TY + TZ
            //markS[14][0] = new FZMath.Point2D(480 + 300 + 0.230583554, 180 + 100 + 2.611204924);    //  North
            //markS[14][1] = null;                            //  West
            //markS[14][2] = new FZMath.Point2D(480 - 300 + 0.23598355, 180 + 100 + 0.075311033);   //  South
            //markS[14][3] = new FZMath.Point2D(480 + 1.166716448, 180 - 100 + 0.628206896);     //  East

            //markT[14][0] = new FZMath.Point2D(480 + 300 + 0.230583554, 180 - 90 + 1.5985985);    //  North
            //markT[14][1] = new FZMath.Point2D(480 - 300 + 0.23598355, 180 - 90 - 0.2013988);     //  East

            ////  TX 회전 -0.06rad + TY 회전 0.003rad
            //markS[15][0] = new FZMath.Point2D(480 + 300 - 2*0.001349999, 180 + 100 + 20 * 0.357525542 + 2*0.689438965);    //  North
            //markS[15][1] = null;                            //  West
            //markS[15][2] = new FZMath.Point2D(480 - 300 + 2*0.001349999, 180 + 100 + 20 * 0.357525542 - 2*0.689438965);   //  South
            //markS[15][3] = new FZMath.Point2D(480 + 0, 180 - 100 - 20 * 0.357525542);     //  East

            //markT[15][0] = new FZMath.Point2D(480 + 300 - 2*0.001349999, 180 - 90 - 20 * 0.000700075);    //  North
            //markT[15][1] = new FZMath.Point2D(480 - 300 + 2*0.001349999, 180 - 90 - 20 * 0.000700075);     //  East

            ////  TY 회전
            ////markS[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 + 100 + 0.689438965);    //  North
            ////markS[7][1] = null;                            //  West
            ////markS[7][2] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 + 100 - 0.689438965);   //  South
            ////markS[7][3] = new FZMath.Point2D(480 + 0, 180 - 100);     //  East

            ////markT[7][0] = new FZMath.Point2D(480 + 300 - 0.001349999, 180 - 90);    //  North
            ////markT[7][1] = new FZMath.Point2D(480 - 300 + 0.001349999, 180 - 90);     //  East



            //Prepare6DMotion();
            //i = 0;
            //mFZM.Extract6DMotion(i, markT[i], markS[i], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            //i = 15;
            //mFZM.Extract6DMotion(i, markT[i], markS[i], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.


            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "csv";
            openFile.Filter = "CSV(*.csv)|*.csv";
            string sFileName = "";
            if (openFile.ShowDialog() == DialogResult.OK)
                sFileName = openFile.FileName;
            else
                return;

            if (!File.Exists(sFileName))
                return;

            string sFileNameCopy = "temp.csv";
            File.Copy(sFileName, sFileNameCopy);
            StreamReader sr = new StreamReader(sFileNameCopy);
            string allstuff = sr.ReadToEnd();
            sr.Close();
            File.Delete(sFileNameCopy);

            string[] eachLines = allstuff.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            markS = new FZMath.Point2D[eachLines.Length][];
            markT = new FZMath.Point2D[eachLines.Length][];

            int effCnt = 0;
            for ( int il = 6; il < eachLines.Length; il++ )
            {
                if (eachLines[il].Length < 8)
                    continue;
                string[] posData = eachLines[il].Split(',');
                double[] lvalue = new double[24];
                for (int iq = 0; iq < 24; iq++)
                    lvalue[iq] = double.Parse(posData[iq + 8]);

                if (lvalue[0] == 0 && lvalue[1] == 0)
                    break;

                markS[effCnt] = new FZMath.Point2D[4];
                markT[effCnt] = new FZMath.Point2D[2];
                markS[effCnt][0] = new FZMath.Point2D(lvalue[0], lvalue[1]);
                markS[effCnt][1] = null;
                markS[effCnt][2] = new FZMath.Point2D(lvalue[8], lvalue[9]);
                markS[effCnt][3] = new FZMath.Point2D((lvalue[12]+lvalue[14])/2, (lvalue[13]+ lvalue[15])/2);

                markT[effCnt][0] = new FZMath.Point2D(lvalue[16], lvalue[17]);
                markT[effCnt][1] = new FZMath.Point2D(lvalue[20], lvalue[21]);
                effCnt++;
            }

            Prepare6DMotion();
            //  Extract6DMotion(int i, Point2D[] markT, Point2D[] markS, Point2D[] markNorm, ref Point2D T, ref double psi, ref double dZ, ref double TX, ref double TY)
            mFZM.Extract6DMotion(0, markT[0], markS[0], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
            string resStr = "";
            int debug = 0;
            for ( i=1; i< effCnt; i++)
            {
                mFZM.Extract6DMotion(i, markT[i], markS[i], ref refT, ref psi, ref dZ, ref TX, ref TY); //  여긴 검증하는 곳.
                resStr += (refT.X * (5.5 / 0.35)).ToString("F1") + "\t" + (refT.Y * (5.5 / 0.35)).ToString("F1") + "\t" + (dZ * (5.5 / 0.35)).ToString("F1") + "\t"
                         + (TX * 180 * 60 / Math.PI).ToString("F1") + "\t" + (TY * 180 * 60 / Math.PI).ToString("F1") + "\t" + (psi * 180 * 60 / Math.PI).ToString("F1") + "\r\n";

                continue;
            }
            rtbLog.Text = resStr;

            return;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            //  CalcScaleTopNSide(Point2D[] pTop, Point2D[] pSide, double normLength, ref double scaleTop, ref double scaleSide)

            //FZMath mFZM = new FZMath();

            FZMath.Point2D[] pTopNorm = new FZMath.Point2D[2];
            FZMath.Point2D[] pSideNorm = new FZMath.Point2D[2];
            FZMath.Point2D[] pTop = new FZMath.Point2D[2];
            FZMath.Point2D[] pSide = new FZMath.Point2D[2];

            double normLength = 445*(0.0055/0.35);
            double scaleTop = 1;
            double scaleSide = 1;

            pTopNorm[0] = new FZMath.Point2D(222.0, 12);
            pTopNorm[1] = new FZMath.Point2D(-223.0, 10);

            pSideNorm[0] = new FZMath.Point2D(222.0, 12);
            pSideNorm[1] = new FZMath.Point2D(-223.0, 10);

            pTop[0] = new FZMath.Point2D(222.0, 12.2);
            pTop[1] = new FZMath.Point2D(-223.2, 10);

            pSide[0] = new FZMath.Point2D(222.0, 11.9);
            pSide[1] = new FZMath.Point2D(-222.9, 10);

            mFZM.CalcScaleTopNSide(pTop, pSide, normLength, ref scaleTop, ref scaleSide);
            mFZM.mScaleTop = scaleTop;
            mFZM.mScaleSide = scaleSide;
            //  여기까지 검증 완료 mScaleTop, mScaleSide 적용함수는 별도로없다. 나중에 좌표추출부에서 직접 참조하면 된다.

            FZMath.Point2D COI = new FZMath.Point2D(0, 0);

            double aT = 1;
            double aS = 1;
            mFZM.TopNSideViewRotationAnlge(pTopNorm, pSideNorm, pTop, pSide, ref aT, ref aS);
            //  여기까지 검증 완료

            mFZM.mOpticsAngleTop = -aT;
            mFZM.mOpticsAngleSide = -aS;

            FZMath.Point2D[] pTopVAC = mFZM.CompensateViewRotation(COI, pTop, mFZM.mOpticsAngleTop);
            FZMath.Point2D[] pSideVAC = mFZM.CompensateViewRotation(COI, pSide, mFZM.mOpticsAngleSide);
            //  실제로 SideView Center 는 설계치 기준 및 영상크기 960x360 기준으로 ( 480.000, 180.000 ) : 영상 좌하단모서리가 0,0 임을 전제
            //  실제로 TopView Center 는 설계치 기준 및 영상크기 960x360 기준으로 ( 480.000, 104.591 ) : 영상 좌하단모서리가 0,0 임을 전제
            //  여기까지 검증 완료

            var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));

            string sFilePath = folder;

            string sFileName = "";

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "csv";
            openFile.InitialDirectory = sFilePath;
            openFile.Filter = "CSV(*.csv)|*.csv";
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                mTargetFiles = new string[openFile.FileNames.Length];
                for (int i = 0; i < openFile.FileNames.Length; i++)
                    mTargetFiles[i] = openFile.FileNames[i];
            }
            else
                return;

            int xW = mCalcEffBand;    //   이 폭은 Defocusing 수준에 따라 6 ~ 10범위에서 자동적으로 설정되도록 함이 적절하며, 관련 알고리즘 필요

            for ( int fi = 0; fi < openFile.FileNames.Length; fi++ )
            {
                int width = 0;
                int height = 0;
                double xi0 = 2;
                double yi0 = 3;
                int yH = 18;
                List<int> DiffSrc = new List<int>();

                sFileName = mTargetFiles[fi];

                StreamReader rd = new StreamReader(sFileName);
                string lstr = rd.ReadToEnd();
                rd.Close();

                string[] allLines = lstr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < allLines.Length; i++)
                {
                    string[] eachLine = allLines[i].Split(',');
                    if (eachLine.Length < 2)
                        break;
                    height++;
                    width = eachLine.Length;
                    for (int j = 0; j < eachLine.Length; j++)
                    {
                        int val = int.Parse(eachLine[j]);
                        DiffSrc.Add(val);
                    }
                }
                int[] XiDiffsrc = DiffSrc.ToArray();

                //  시작점 설정
                xi0 = width / 2 - xW / 2 - 2;

                byte lupdown = 255;
                double edgeX = mFZM.ConvergePeakX(ref XiDiffsrc, width, height, (int)xi0, (int)yi0, xW, yH, ref lupdown);
                //  여기까지 검증 완료

                double edgeY = 0;
                //int[] yiDiffsrc = new int[XiDiffsrc.Length];
                //for (int i = 0; i < width; i++)
                //    for (int j = 0; j < height; j++)
                //        yiDiffsrc[j + i * height] = XiDiffsrc[i + j * width];

                //edgeY = mFZM.ConvergePeakY(ref yiDiffsrc, height, width, yi0, xi0, yH, xW);
                ////  여기까지 검증 완료


                int lastIndex = sFileName.LastIndexOf("\\");
                rtbLog.Text += sFileName.Substring(lastIndex+1) + "\t" + edgeX.ToString("F4") + "\t" + edgeY.ToString("F4") + "\r\n";
            }

        }

        private void button25_Click(object sender, EventArgs e)
        {
            var folder =  "C:\\TZAFTest\\Result\\RawData\\Image";

            int clipOffset = 60;
            if ( tbClipOffset.Text.Length > 0)
                clipOffset = int.Parse(tbClipOffset.Text);

            string sFilePath = folder;

            string sFileName = "";

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
            //  mLastFMIfile
            SaveFiducialMark(mLastFMIfile);
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
            mModelScale = 4 - lbModelScale.SelectedIndex;

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
            }
            DesignModelSideChanged(sender, 0);

        }
    }
}