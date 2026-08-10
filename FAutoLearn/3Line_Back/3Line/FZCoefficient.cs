using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using OpenCvSharp;
using static alglib;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using MathNetVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using MathNetMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Globalization;
using static FAutoLearn.FZMath;

namespace FAutoLearn
{
    public class FZMath
    {
        public double[,] mData = null;
        public double[,] mDataX = null;
        public double[,] mDataY = null;
        public int mInitNumX = 1;
        public int mInitNumY = 1;
        public int mInitNumDataSet = 1;
        public int mDataColLength = 0;
        public int mDerivedOrder = 0;
        public int mNumV = 0;
        public int mNthVset = 0;
        public int mMaxDimensionOfEquation = 0;
        public int[] mAttentionY = new int[4] { -1, -1, -1, -1 };
        public double mMinSensitivity = 0.00001;
        public double mErrThreshold = 0.001;
        public double[] mErrHistory = new double[100];
        public int[] uOn = null;
        public int[] uID = null;
        public double[,] u = null;
        public double vTan40 =   Math.Sin(40.0 / 180 * Math.PI);
        public double vSin40 =   Math.Sin(40.0 / 180 * Math.PI);//Math.Sin(39.9 / 180 * Math.PI);
        public double vSec40 = 1/Math.Cos(40.0 / 180 * Math.PI);//;1/Math.Cos(39.9 / 180 * Math.PI);
        public double vCos40 =   Math.Cos(40.0 / 180 * Math.PI);//Math.Cos(39.9 / 180 * Math.PI);

        public bool mbMasterCCS = false;
        public double[] mMasterCCS = new double[3] {0, 0, 0 };

        public void SetMasterCCS(double txMin, double tyMin, double tzMin)
        {
            mMasterCCS[0] = Math.PI * txMin / (60*180); //  Min to Radian
            mMasterCCS[1] = Math.PI * tyMin / (60*180); //  Min to Radian
            mMasterCCS[2] = Math.PI * tzMin / (60*180); //  Min to Radian
        }

        public void SetSideviewTheta(double rad)
        {
            vSin40 = Math.Sin(rad);
            vCos40 = Math.Cos(rad);
            vSec40 = 1 / Math.Sqrt(1 - vSin40 * vSin40);

        }

        public bool m_bNonStopProcessDone = false;

        public int mNumVnow = 0;
        public int[,] mExtNumVnow = new int[4, 2];
        public int[] uIDreadable = null;
        public int[,] m_uIndex = null;
        public int[,] m_LastUIndex = new int[4, 100];
        public int[,] m_LastVIndex = new int[4, 100];

        public string[] m_uConfig = null;
        public int[] m_u4C = new int[4];
        public int m_u4Cindex = 0;
        public double[,] matrixU = null;
        public double[] matrixY = null;
        //double[,] matrixInvU = null;
        public double[] matrixC = null;
        public double[,] matrixC_U = new double[4, 100];
        public double[,] matrixC_V = new double[4, 100];
        //double[] m_rank = null;
        public double m_OfsX = 0;
        public double m_OfsY = 0;
        public string mProcessMsg = "";
        public string[] mResultMsg = new string[4] { "", "", "", "" };
        public double[] mMarkDistNear = new double[4] { 9.3, 9.3, 9.3, 9.3 };
        public double[] mMarkDistFar = new double[4] { 12.2, 12.2, 12.2, 12.2 };
        public double[] mMarkDistDecenter = new double[4] { 0, 0, 0, 0 };
        public double[,] mLastGap = new double[2, 2];

        public int mCropCgap = 260; //  FVision 의 설정상태에 따라 업데이트 되어야 함.
        public int mCropABgap = 50; //  FVision 의 설정상태에 따라 업데이트 되어야 함.

        public class Plot3DData
        {
            public double[,] m3D_AxisXN = new double[2, 3];
            public double[,] m3D_AxisYN = new double[2, 3];
            public double[,] m3D_AxisXP = new double[2, 3];
            public double[,] m3D_AxisYP = new double[2, 3];
            public double[,] m3D_AxisZ = new double[2, 3];
            public double[,] m3D_SurfBNorg = new double[2, 3];
            public double[,] m3D_SurfBorg = new double[4, 3];
            public double[,] m3D_SurfBN = new double[2, 3];
            public double[,] m3D_SurfA = new double[4, 3];  //  Surface A
            public double[,] m3D_SurfB = new double[4, 3];  //  Surface B
            public double[,] m3D_SurfCorg = new double[4, 3];  //  Surface C ( Lens1G Top )
            public double[,] m3D_SurfDorg = new double[6, 3];  //  Surface D
            public double[,] m3D_SurfC = new double[4, 3];  //  Surface C ( Lens1G Top )
            public double[,] m3D_SurfD = new double[6, 3];  //  Surface D
            public double[,] m3D_SurfZ = new double[4, 3];  //  Surface Bottom

            public double[,] m2D_AxisXN = new double[2, 2];
            public double[,] m2D_AxisYN = new double[2, 2];
            public double[,] m2D_AxisXP = new double[2, 2];
            public double[,] m2D_AxisYP = new double[2, 2];
            public double[,] m2D_AxisZ = new double[2, 2];
            public double[,] m2D_SurfBN = new double[2, 2];
            public double[,] m2D_SurfA = new double[4, 2];  //  Surface A
            public double[,] m2D_SurfB = new double[4, 2];  //  Surface B
            public double[,] m2D_SurfCorg = new double[4, 2];  //  Surface C ( Lens1G Top )
            public double[,] m2D_SurfDorg = new double[6, 2];  //  Surface D ( Shaft )
            public double[,] m2D_SurfC = new double[4, 2];  //  Surface C ( Lens1G Top )
            public double[,] m2D_SurfD = new double[6, 2];  //  Surface D ( Shaft )
            public double[,] m2D_SurfZ = new double[4, 2];  //  Surface Bottom

            public double mViewCosPhi = 0;
            public double mViewSinPhi = 0;
            public double mViewCosTheta = 0;
            public double mViewSinTheta = 0;

            public Plot3DData()
            {
                m3D_AxisXP = new double[2, 3]{
                    { 0, 0, -3 },
                    { 3, 0, -3 }
                };
                m3D_AxisYP = new double[2, 3]{
                    { 0, 0, -3 },
                    { 0, 5, -3 }
                };
                m3D_AxisXN = new double[2, 3]{
                    { -3, 0, -3 },
                    {  0, 0, -3 }
                };
                m3D_AxisYN = new double[2, 3]{
                    { 0,-5, -3 },
                    { 0, 0, -3 }
                };
                m3D_AxisZ = new double[2, 3]{
                    { 0, 0, -3 },
                    { 0.0,0, 3 }
                };
                m3D_SurfBNorg = new double[2, 3]{
                    { 0,0, 4 },
                    { 0,0, 8 }
                };
                m3D_SurfBN = new double[2, 3]{
                    { 0,0, 4 },
                    { 0,0, 8 }
                };
                m3D_SurfA = new double[4, 3]{
                    { -3,  5, -3 },
                    {  3,  5, -3 },
                    {  3, -5, -3 },
                    { -3, -5, -3 },
                };
                m3D_SurfBorg = new double[4, 3]{
                    { -3,  5, 0.4 },
                    {  3,  5, 0.4 },
                    {  3, -5, 0.4 },
                    { -3, -5, 0.4 },
                };
                m3D_SurfC = new double[4, 3]{
                    { -50, -50, 30 },
                    { -50,  50, 30 },
                    { -30, -50, 30 },
                    { -30,  50, 30 },
                };

                m3D_SurfD = new double[6, 3]{
                    { 10, -50, -10 },
                    { 10,  50, -10 },
                    { 30, -50, -10 },
                    { 30,  50, -10 },
                    { 50, -50, -10 },
                    { 50,  50, -10 },
                };
                m3D_SurfZ = new double[4, 3]{
                    { -50, -50, -30 },
                    { -50,  50, -30 },
                    {  50, -50, -30 },
                    {  50,  50, -30 },
                };

                m3D_SurfCorg = new double[4, 3]{
                    { -50, -50, 30 },
                    { -50,  50, 30 },
                    { -30, -50, 30 },
                    { -30,  50, 30 },
                };

                m3D_SurfDorg = new double[6, 3]{
                    { 10, -50, -10 },
                    { 10,  50,  -10 },
                    { 30, -50, -10 },
                    { 30,  50,  -10 },
                    { 50, -50, -10 },
                    { 50,  50,  -10 },
                };
            }
        }
        public Plot3DData[] mPlotAB = new Plot3DData[2] { null, null };
        //public Plot3DData mPlotCD = new Plot3DData();

        double M_PI = Math.Asin(1) * 2;

        public class NormalRandom
        {
            private Random rand;
            private bool hasSpare;
            private double spare;

            public NormalRandom(int seed = 0)
            {
                rand = seed == 0 ? new Random() : new Random(seed);
                hasSpare = false;
            }

            // 평균 mean, 표준편차 stdDev
            public double Next(double mean = 0.0, double stdDev = 1.0)
            {
                if (hasSpare)
                {
                    hasSpare = false;
                    return spare * stdDev + mean;
                }

                double u, v, s;
                do
                {
                    u = rand.NextDouble() * 2.0 - 1.0;
                    v = rand.NextDouble() * 2.0 - 1.0;
                    s = u * u + v * v;
                } while (s >= 1.0 || s == 0.0);

                s = Math.Sqrt(-2.0 * Math.Log(s) / s);
                spare = v * s;
                hasSpare = true;
                return mean + stdDev * u * s;
            }
        }
        public NormalRandom NRgenerator = new NormalRandom();
        public struct Vector3D
        {
            public double X;
            public double Y;
            public double Z;
        }
        public struct Surface3D
        {
            //  N * ( X - P )  = 0
            public Vector3D N; //  Normal Vector
            public Vector3D P; //  Referecne Point
        }

        //public struct Line3D
        //{
        //    //  N * ( X - P )  = 0
        //    Vector3D V; //  Direction Vector
        //    Vector3D P; //  Referecne Point
        //}

        public class Point2D
        {
            public double X;
            public double Y;
            public Point2D(double x=0, double y=0)
            {
                X = x;
                Y = y;
            }
        }
        public struct Poly2nd
        {
            //  y = a(x-b)^2 + c
            public double a;
            public double b;
            public double c;
        }

        public struct WeightedPoint
        {
            public int X;
            public int Y;
            public double W;
        }
        public struct WeightedPoint2D
        {
            public double X;
            public double Y;
            public double W;
        }
        public struct CircleInfo
        {
            public double X;
            public double Y;
            public double R;
        }
        /// <summary>
        ///  다음은  master 측정 후 저장되어야 하고, 프로그램 실행 시 읽어들여야 한다.
        /// </summary>
        //public Point2D[,] mMasterLaserPts = new Point2D[2, 20];  //  Lens1G, ShaftTester 공통, 값은 서로 다름.
        //public Point2D[][,] mMasterLEDPts = new Point2D[2][,];    //  Lens1G, ShaftTester 공통, 값은 서로 다름.
        public Point2D[] mMasterShaftDirAngle = new Point2D[2];    //  Left 0. Right 1 ; Top X, Side Y, degree
        public double[,] mLaserAngleDirection = new double[2, 20];  //  0: Angle, 1: direction, 0 ~ 19 : Mark Index 
        public double[] mLaserFrontAngle = new double[20];      //  Lens1G, ShaftTester 공통, 값은 서로 다름.
        public double[] mLaserRearAngle = new double[20];       //  Only For ShaftTester
        public double[,] mAngleOfMasterU = new double[2, 2];     //  Only For ShaftTester
                                                                 //  Top/Side View ICS X 축으로부터 ICS 상의 Master Shaft Axis 로의 각도. 이상적으로는 0deg, CCW 방향이 +, Left 0, Right 1
                                                                 //  AngleOfMasterU[0, 0] : Left Shaft from Top
                                                                 //  AngleOfMasterU[0, 1] : Left Shaft from Side
                                                                 //  AngleOfMasterU[1, 0] : Right Shaft from Top
                                                                 //  AngleOfMasterU[1, 1] : Right Shaft from Side
        public double[] mMasterSurfANorm = new double[3]; //  Master 기준면 A 측정값에 따른 Normal Vector    Lens1G, ShaftTester 공통
        public double[] mMasterSurfBNorm = new double[3]; //  Master 기준면 B 측정값에 따른 Normal Vector    Only For Lens1G
        /// <summary>
        /// <summary>

        public string m_strMsg = "";

        public const int FOV_YH = 171;
        public const int FOV_XH = 390;  //  320 from FOVH = 640 -> 390 from FOVH = 780

        public double mOpticsAngleTop = 0;
        public double mOpticsAngleSide = 0;
        public Point2D mCenterOfImg = new Point2D(FOV_XH, FOV_YH);
        public Point2D mCOItop = new Point2D(FOV_XH, FOV_YH);
        public Point2D mCOIside = new Point2D(FOV_XH, FOV_YH);
        public double mC12 = 1;
        public double mC13 = 1;
        public double mScaleTop = 1;
        public double mScaleSide = 1;

        public double[] mScaleX = new double[3] { 0, 1, 0 };
        public double[] mScaleY = new double[3] { 0, 1, 0 };
        public double[] mScaleZ = new double[3] { 0, 1, 0 };
        public double[] mScaleTX = new double[3] { 0, 1, 0 };
        public double[] mScaleTY = new double[3] { 0, 1, 0 };
        public double[] mScaleTZ = new double[3] { 0, 1, 0 };
        public double mEastviewYPscale = 1.0;
        public double[] mXtoYbyView = new double[3];
        public double[] mXtoZbyView = new double[3];
        public double[] mXtoTXbyView = new double[3];
        public double[] mXtoTYbyView = new double[3];
        public double[] mXtoTZbyView = new double[3];
        public double[] mYtoXbyView = new double[3];
        public double[] mYtoZbyView = new double[3];
        public double[] mYtoTXbyView = new double[3];
        public double[] mYtoTYbyView = new double[3];
        public double[] mYtoTZbyView = new double[3];
        public double[] mZtoXbyView = new double[3];
        public double[] mZtoYbyView = new double[3];  
        public double[] mZtoTXbyView = new double[3];
        public double[] mZtoTYbyView = new double[3];
        public double[] mZtoTZbyView = new double[3];
        public double[] mTXtoTYbyView = new double[3];
        public double[] mTXtoTZbyView = new double[3];
        public double[] mTYtoTXbyView = new double[3];
        public double[] mTYtoTZbyView = new double[3];
        public double[] mTZtoTXbyView = new double[3];
        public double[] mTZtoTYbyView = new double[3];
        public double[] mXJtoX = new double[2];
        public double[] mYJtoY = new double[2];
        public double[] mZJtoZ = new double[2];
        public double[] mTZtoZbyView = new double[3];
        public FZMath()
        {
            //ResetLog();
            mPlotAB[0] = new Plot3DData();
            mPlotAB[1] = new Plot3DData();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mPlotAB[1].m3D_AxisXP[i, j] = mPlotAB[1].m3D_AxisXP[i, j] * 10;
                    mPlotAB[1].m3D_AxisXN[i, j] = mPlotAB[1].m3D_AxisXN[i, j] * 10;
                    mPlotAB[1].m3D_AxisYP[i, j] = mPlotAB[1].m3D_AxisYP[i, j] * 10;
                    mPlotAB[1].m3D_AxisYN[i, j] = mPlotAB[1].m3D_AxisYN[i, j] * 10;
                }
            }
        }
        //public int GetRealParameters(string rootDir)
        //{
        //    string sFilePath = rootDir + "\\DoNotTouch\\MarkGaps_Lens1GTester.txt";

        //    if (!File.Exists(sFilePath))
        //    {
        //        StreamWriter sw = new StreamWriter(sFilePath);
        //        sw.WriteLine("9.3\t12.0\t0.0");
        //        sw.WriteLine("9.3\t12.0\t0.0");
        //        sw.Close();
        //        return 2;
        //    }


        //    StreamReader sr = new StreamReader(sFilePath);
        //    string allLines = sr.ReadToEnd();
        //    string[] eachLine = allLines.Split("\n".ToCharArray());

        //    if (eachLine.Length > 3)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            string[] figures = eachLine[i].Split('\t');
        //            mMarkDistNear[i] = Convert.ToDouble(figures[0]);
        //            mMarkDistFar[i] = Convert.ToDouble(figures[1]);
        //            mMarkDistDecenter[i] = Convert.ToDouble(figures[2]);
        //        }
        //        return 4;
        //    }
        //    else if (eachLine.Length > 1)
        //    {
        //        int iChannel = 1;
        //        if (eachLine[1].Length > 4)
        //            iChannel = 2;

        //        for (int i = 0; i < iChannel; i++)
        //        {
        //            string[] figures = eachLine[i].Split('\t');
        //            mMarkDistNear[i] = Convert.ToDouble(figures[0]);
        //            mMarkDistFar[i] = Convert.ToDouble(figures[1]);
        //            mMarkDistDecenter[i] = Convert.ToDouble(figures[2]);
        //        }
        //        return iChannel;
        //    }
        //    else if (eachLine.Length > 0 && eachLine[0].Length > 4)
        //    {
        //        string[] figures = eachLine[0].Split('\t');
        //        mMarkDistNear[0] = Convert.ToDouble(figures[0]);
        //        mMarkDistFar[0] = Convert.ToDouble(figures[1]);
        //        mMarkDistDecenter[0] = Convert.ToDouble(figures[2]);
        //        return 1;
        //    }

        //    return 0;
        //}

        //public bool LoadDataFile()
        //{
        //    string sFilePath = "C:\\FZ4PTest\\DoNotTouch\\";
        //    OpenFileDialog openFile = new OpenFileDialog();
        //    openFile.DefaultExt = "txt";
        //    openFile.InitialDirectory = sFilePath;

        //    openFile.Filter = "txt(*.txt)|*.txt";
        //    if (openFile.ShowDialog() == DialogResult.OK)
        //    {
        //        string sFileName = openFile.FileName;
        //        //Thread threadReadDataFile = new Thread(() => ReadDataFile(sFileName));
        //        //threadReadDataFile.Start();
        //        ReadDataFile(sFileName);
        //    }
        //    return true;
        //}

        //public bool ReadDataFile(string filename)
        //{
        //    StreamReader sr = new StreamReader(filename);
        //    string bulkData = sr.ReadToEnd();
        //    sr.Close();
        //    string[] fullLines = bulkData.Split("\r".ToCharArray());

        //    int allLines = fullLines.Length;
        //    string[] aLine;
        //    int i = 0;
        //    int colLength = 0;
        //    if (fullLines[0].Length > 1)
        //    {
        //        aLine = fullLines[0].Split('\t');
        //        colLength = aLine.Length;
        //    }
        //    else
        //        return false;
        //    mInitNumDataSet = allLines;
        //    mDataColLength = colLength;

        //    mData = new double[colLength, mInitNumDataSet];

        //    for (i = 0; i < allLines; i++)
        //    {
        //        if (fullLines[i].Length > 1)
        //        {
        //            aLine = fullLines[i].Split('\t');
        //            if (aLine.Length < colLength) break;
        //            for (int j = 0; j < colLength; j++)
        //                mData[j, i] = Convert.ToDouble(aLine[j]);
        //        }
        //        else
        //            break;
        //    }
        //    mInitNumDataSet = i;
        //    //tbDataFile.Text = filename;
        //    //mProcessMsg += ">Number of Data : " + mInitNumDataSet.ToString() + "\r\n";
        //    //mProcessMsg += ">Number of Column : " + mDataColLength.ToString() + "\r\n";
        //    return true;
        //}

        //public bool ApplyCondition(bool IsAuto = false)
        //{
        //    mInitNumX = 4;
        //    mInitNumY = 3;
        //    mDerivedOrder = 5;
        //    mMaxDimensionOfEquation = 18;
        //    mMinSensitivity = 0.002;
        //    mErrThreshold = 0.0001;
        //    //mMinSensitivity = 0.001;
        //    //mErrThreshold = 0.00005;

        //    mDataX = new double[mInitNumX, mInitNumDataSet];
        //    mDataY = new double[mInitNumY, mInitNumDataSet];

        //    mNumV = mInitNumX;
        //    if (mDerivedOrder > 1)
        //    {
        //        mNumV += (mInitNumX + 1) * mInitNumX / 2;
        //    }
        //    if (mDerivedOrder > 2)
        //    {
        //        mNumV += (mInitNumX + 2) * (mInitNumX + 1) * mInitNumX / 6;
        //    }
        //    if (mDerivedOrder > 3)
        //    {
        //        mNumV += (mInitNumX + 3) * (mInitNumX + 2) * (mInitNumX + 1) * mInitNumX / 24;
        //    }
        //    if (mDerivedOrder > 4)
        //    {
        //        mNumV += (mInitNumX + 4) * (mInitNumX + 3) * (mInitNumX + 2) * (mInitNumX + 1) * mInitNumX / 120;
        //    }

        //    m_uConfig = new string[mNumV];
        //    u = new double[mNumV, mInitNumDataSet];
        //    uOn = new int[mNumV];
        //    uID = new int[mNumV];
        //    uIDreadable = new int[mNumV];
        //    m_uIndex = new int[100, mNumV];

        //    if (mDataColLength < (mInitNumX + mInitNumY))
        //    {
        //        mProcessMsg += "Data Column Length is less than num X + num Y.\r\n";
        //        return true;
        //    }
        //    for (int i = 0; i < mInitNumDataSet; i++)
        //    {
        //        for (int j = 0; j < mInitNumX; j++)
        //        {
        //            mDataX[j, i] = mData[j, i];
        //        }
        //        for (int j = 0; j < mInitNumY; j++)
        //        {
        //            mDataY[j, i] = mData[mInitNumX + j, i];
        //        }
        //    }
        //    return true;
        //}

        //public bool CalcStep1(int selectedIndex = -1)
        //{
        //    int last_u = 0;
        //    int i = 0;
        //    int iSelected = 0;

        //    if (selectedIndex < 0)
        //    {
        //        mAttentionY[iSelected++] = 0;
        //        mAttentionY[iSelected++] = 1;
        //    }
        //    for (i = 0; i < 4; i++)
        //    {
        //        if (i >= iSelected)
        //            mAttentionY[i] = -1;

        //        if (mAttentionY[i] > -1)
        //        {
        //            mProcessMsg += ">> mAttentionY[" + i.ToString() + "]=" + mAttentionY[i].ToString() + "\r\n";
        //        }
        //    }


        //    for (i = 0; i < mInitNumX; i++)
        //    {
        //        uOn[i] = 1;
        //        uID[i] = i;
        //        uIDreadable[i] = i + 1;
        //        for (int j = 0; j < mInitNumDataSet; j++)
        //        {
        //            u[i, j] = mDataX[i, j];
        //        }
        //    }
        //    last_u = i;
        //    int lcur_uID = 0;
        //    bool IsStop = false;
        //    if (mDerivedOrder > 1)
        //    {
        //        for (int a = 0; a < mInitNumX; a++)
        //        {
        //            for (int b = 0; b < mInitNumX; b++)
        //            {
        //                lcur_uID = (int)(Math.Pow(10, a + 1) + Math.Pow(10, b + 1));
        //                IsStop = false;
        //                for (int z = 0; z < last_u; z++)
        //                {
        //                    if (uID[z] == lcur_uID)
        //                    {
        //                        IsStop = true;
        //                        break;
        //                    }
        //                }
        //                if (IsStop)
        //                    continue;
        //                uOn[last_u] = 1;
        //                uID[last_u] = lcur_uID;
        //                uIDreadable[last_u] = (a + 1) + (b + 1) * 10;
        //                for (int j = 0; j < mInitNumDataSet; j++)
        //                {
        //                    u[last_u, j] = mDataX[a, j] * mDataX[b, j];
        //                }
        //                last_u++;
        //            }
        //        }
        //    }
        //    if (mDerivedOrder > 2)
        //    {
        //        for (int c = 0; c < mInitNumX; c++)
        //        {
        //            for (int b = 0; b < mInitNumX; b++)
        //            {
        //                for (int a = 0; a < mInitNumX; a++)
        //                {
        //                    lcur_uID = (int)(Math.Pow(10, a + 1) + Math.Pow(10, b + 1) + Math.Pow(10, c + 1));
        //                    IsStop = false;
        //                    for (int z = 0; z < last_u; z++)
        //                    {
        //                        if (uID[z] == lcur_uID)
        //                        {
        //                            IsStop = true;
        //                            break;
        //                        }
        //                    }
        //                    if (IsStop)
        //                        continue;
        //                    uOn[last_u] = 1;
        //                    uID[last_u] = lcur_uID;
        //                    uIDreadable[last_u] = (a + 1) + (b + 1) * 10 + (c + 1) * 100;
        //                    for (int j = 0; j < mInitNumDataSet; j++)
        //                    {
        //                        u[last_u, j] = mDataX[a, j] * mDataX[b, j] * mDataX[c, j];
        //                    }
        //                    last_u++;
        //                }
        //            }
        //        }
        //    }
        //    if (mDerivedOrder > 3)
        //    {
        //        for (int d = 0; d < mInitNumX; d++)
        //        {
        //            for (int c = 0; c < mInitNumX; c++)
        //            {
        //                for (int b = 0; b < mInitNumX; b++)
        //                {
        //                    for (int a = 0; a < mInitNumX; a++)
        //                    {
        //                        lcur_uID = (int)(Math.Pow(10, a + 1) + Math.Pow(10, b + 1) + Math.Pow(10, c + 1) + Math.Pow(10, d + 1));
        //                        IsStop = false;
        //                        for (int z = 0; z < last_u; z++)
        //                        {
        //                            if (uID[z] == lcur_uID)
        //                            {
        //                                IsStop = true;
        //                                break;
        //                            }
        //                        }
        //                        if (IsStop)
        //                            continue;
        //                        uOn[last_u] = 1;
        //                        uID[last_u] = lcur_uID;
        //                        uIDreadable[last_u] = (a + 1) + (b + 1) * 10 + (c + 1) * 100 + (d + 1) * 1000;
        //                        for (int j = 0; j < mInitNumDataSet; j++)
        //                        {
        //                            u[last_u, j] = mDataX[a, j] * mDataX[b, j] * mDataX[c, j] * mDataX[d, j];
        //                        }
        //                        last_u++;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (mDerivedOrder > 4)
        //    {
        //        for (int e = 0; e < mInitNumX; e++)
        //        {
        //            for (int d = 0; d < mInitNumX; d++)
        //            {
        //                for (int c = 0; c < mInitNumX; c++)
        //                {
        //                    for (int b = 0; b < mInitNumX; b++)
        //                    {
        //                        for (int a = 0; a < mInitNumX; a++)
        //                        {
        //                            lcur_uID = (int)(Math.Pow(10, a + 1) + Math.Pow(10, b + 1) + Math.Pow(10, c + 1) + Math.Pow(10, d + 1) + Math.Pow(10, e + 1));
        //                            IsStop = false;
        //                            for (int z = 0; z < last_u; z++)
        //                            {
        //                                if (uID[z] == lcur_uID)
        //                                {
        //                                    IsStop = true;
        //                                    break;
        //                                }
        //                            }
        //                            if (IsStop)
        //                                continue;
        //                            uOn[last_u] = 1;
        //                            uID[last_u] = lcur_uID;
        //                            uIDreadable[last_u] = (a + 1) + (b + 1) * 10 + (c + 1) * 100 + (d + 1) * 1000 + (e + 1) * 10000;
        //                            for (int j = 0; j < mInitNumDataSet; j++)
        //                            {
        //                                u[last_u, j] = mDataX[a, j] * mDataX[b, j] * mDataX[c, j] * mDataX[d, j] * mDataX[e, j];
        //                            }
        //                            last_u++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    // Check Derived Variables
        //    //string strID = "\r\n";
        //    //StreamWriter wr = new StreamWriter("DerivedVars.csv");
        //    //for (i = 0; i < last_u; i++)
        //    //{
        //    //    strID += i.ToString() + " ; " + uIDreadable[i].ToString() + "\t" + uID[i].ToString() + "\r\n";
        //    //    for (int j = 0; j < mInitNumDataSet; j++)
        //    //        wr.WriteLine(uID[i].ToString() + "," + u[i, j]);
        //    //}
        //    //wr.Close();
        //    return true;
        //}

        //public bool CalcStep2(ref int p, ref int nestedCount, int ySelectedIndex = 0)
        //{
        //    //  FInd First 2 indepnedent variables
        //    //SaveLog(">>CalcStep2()");
        //    nestedCount++;

        //    int attentionY = mAttentionY[ySelectedIndex];
        //    bool res = false;

        //    if (attentionY < 0) attentionY = 0;
        //    //SaveLog("\r\n nestedCount = " + nestedCount.ToString() + "\r\n AttentionY = " + attentionY.ToString() + "\r\n");
        //    matrixU = new double[2, 2];
        //    matrixY = new double[2];
        //    for (int i = 0; i < 2; i++)
        //    {
        //        for (int j = 0; j < 2; j++)
        //        {
        //            for (int z = 0; z < mInitNumDataSet; z++)
        //                matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //        }
        //    }
        //    int rankU = RankOfU(matrixU, 2);
        //    if (rankU == 2)
        //    {
        //        //SaveLog("Rank = 2 at " + p.ToString() + " at " + nestedCount.ToString() + "th\r\n");
        //        for (int i = 0; i < 2; i++)
        //        {
        //            for (int z = 0; z < mInitNumDataSet; z++)
        //                matrixY[i] += u[m_uIndex[p, i], z] * mDataY[attentionY, i];
        //        }
        //        return true;
        //    }
        //    else if (rankU == 0)
        //    {
        //        m_uIndex[p, 0] = m_uIndex[p, 1] + 1;
        //        m_uIndex[p, 1] = m_uIndex[p, 1] + 2;
        //        if (m_uIndex[p, 1] >= mNumV)
        //            return false;
        //        p++;
        //        res = CalcStep2(ref p, ref nestedCount, ySelectedIndex);
        //        //SaveLog(" Rank = 0 at " + p.ToString() + " at " + nestedCount.ToString() + "th\r\n");
        //        if (res)
        //            return true;
        //        else
        //            return false;
        //    }
        //    // In case of rank 1
        //    //SaveLog(" Rank = 1 at " + p.ToString() + " at " + nestedCount.ToString() + "th\r\n");
        //    while (true)
        //    {
        //        m_u4C[m_u4Cindex++] = m_uIndex[p, 0];
        //        m_u4C[m_u4Cindex++] = m_uIndex[p, 1];
        //        if (m_u4Cindex == 4)
        //        {
        //            //  Calc 4 cases to find First 2 independent Vars.
        //            matrixU = new double[2, 2];
        //            for (int i = 0; i < 2; i++)
        //            {
        //                for (int j = 0; j < 2; j++)
        //                {
        //                    for (int z = 0; z < mInitNumDataSet; z++)
        //                        matrixU[i, j] += u[m_u4C[0], z] * u[m_u4C[2], z];
        //                }
        //            }
        //            rankU = RankOfU(matrixU, 2);
        //            if (rankU == 2)
        //                return true;
        //            matrixU = new double[2, 2];
        //            for (int i = 0; i < 2; i++)
        //            {
        //                for (int j = 0; j < 2; j++)
        //                {
        //                    for (int z = 0; z < mInitNumDataSet; z++)
        //                        matrixU[i, j] += u[m_u4C[1], z] * u[m_u4C[3], z];
        //                }
        //            }
        //            rankU = RankOfU(matrixU, 2);
        //            if (rankU == 2)
        //                return true;
        //            matrixU = new double[2, 2];
        //            for (int i = 0; i < 2; i++)
        //            {
        //                for (int j = 0; j < 2; j++)
        //                {
        //                    for (int z = 0; z < mInitNumDataSet; z++)
        //                        matrixU[i, j] += u[m_u4C[1], z] * u[m_u4C[2], z];
        //                }
        //            }
        //            rankU = RankOfU(matrixU, 2);
        //            if (rankU == 2)
        //                return true;
        //            matrixU = new double[2, 2];
        //            for (int i = 0; i < 2; i++)
        //            {
        //                for (int j = 0; j < 2; j++)
        //                {
        //                    for (int z = 0; z < mInitNumDataSet; z++)
        //                        matrixU[i, j] += u[m_u4C[0], z] * u[m_u4C[3], z];
        //                }
        //            }
        //            rankU = RankOfU(matrixU, 2);
        //            if (rankU == 2)
        //                return true;
        //        }
        //        else
        //        {
        //            m_uIndex[p, 0] = m_uIndex[p, 1] + 1;
        //            m_uIndex[p, 1] = m_uIndex[p, 1] + 2;
        //            if (m_uIndex[p, 1] >= mNumV)
        //                return false;
        //            p++;
        //            res = CalcStep2(ref p, ref nestedCount, ySelectedIndex);
        //            if (res)
        //                return true;
        //        }
        //        if (m_uIndex[p, 1] >= mNumV)
        //            return false;
        //    }
        //    return false;
        //}

        //public bool CalcStep3()
        //{
        //    //  Add One more Vars to the Equation
        //    //SaveLog(">>CalcStep3()");
        //    int p = mNthVset;
        //    int rankU = 0;
        //    int lNumVnow = mNumVnow + 1;
        //    string strlog = "";

        //    if (lNumVnow >= mNumV)
        //    {
        //        //SaveLog("Already Number of Variables are full!");
        //        return false;
        //    }

        //    //if (lNumVnow > mMaxDimensionOfEquation)
        //    //{
        //    //    SaveLog("Already Number of Variables are full!");
        //    //    return false;
        //    //}
        //    if (lNumVnow > 1)
        //        m_uIndex[mNthVset, lNumVnow - 1] = m_uIndex[mNthVset, lNumVnow - 2];

        //    while (true)
        //    {
        //        //SaveLog("mNthVset=" + mNthVset.ToString() + " lNumVnow=" + lNumVnow.ToString() + "\r\n");
        //        //WriteTextSafe(strlog);
        //        while (true)
        //        {
        //            m_uIndex[mNthVset, lNumVnow - 1]++;
        //            //SaveLog("m_uIndex[mNthVset, lNumVnow - 1]=" + m_uIndex[mNthVset, lNumVnow - 1].ToString());
        //            if (m_uIndex[mNthVset, lNumVnow - 1] >= mNumV)
        //            {
        //                mNumVnow = lNumVnow - 1;
        //                for (int i = 0; i < mNumVnow; i++)
        //                    strlog += m_uIndex[mNthVset, i] + "\t";
        //                strlog += " \r\n";
        //                //SaveLog(strlog + "No more uIndex");
        //                mProcessMsg = "No more uIndex";
        //                return false;
        //            }
        //            if (uOn[m_uIndex[mNthVset, lNumVnow - 1]] > 0)
        //                break;
        //        }
        //        matrixU = new double[lNumVnow, lNumVnow];
        //        for (int i = 0; i < lNumVnow; i++)
        //        {
        //            for (int j = 0; j < lNumVnow; j++)
        //            {
        //                for (int z = 0; z < mInitNumDataSet; z++)
        //                    matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //            }
        //        }
        //        //SaveLog("Call RankOfU()");
        //        //WriteTextSafe(strlog );
        //        rankU = RankOfU(matrixU, lNumVnow);
        //        //SaveLog("RankOfU() finish");
        //        //strlog = "";
        //        //for (int i = 0; i < lNumVnow; i++)
        //        //    strlog += m_uIndex[mNthVset, i] + "\t";
        //        //strlog += " : rank is " + rankU.ToString() + " \r\n";
        //        //SaveLog(strlog);
        //        //WriteTextSafe(strlog );
        //        if (rankU == lNumVnow)
        //            break;
        //    }
        //    //SaveLog("New Var Added : " + m_uIndex[mNthVset, lNumVnow - 1].ToString());
        //    //MessageBox.Show("A_222");
        //    //strlog = "FInal lNumVnow=" + lNumVnow.ToString() + "\r\n"; ;
        //    //mProcessMsg = "New Rank is " + rankU.ToString() + "\r\n";

        //    if (rankU < lNumVnow)
        //        return false;

        //    mNumVnow = lNumVnow;
        //    return true;
        //}
        //public bool CalcStep3(double[] lSensitivity)
        //{
        //    //SaveLog(">>CalcStep3(lSensitivity)");
        //    //  Add One more Vars to the Equation
        //    int p = mNthVset;
        //    int rankU = 0;
        //    int lNumVnow = mNumVnow;
        //    //string strlog = "";
        //    int iDel = 0;
        //    double lminSens = 999999;

        //    for (int i = 0; i < mNumVnow; i++)
        //    {
        //        if (lminSens > lSensitivity[i])
        //        {
        //            iDel = i;
        //            lminSens = lSensitivity[i];
        //        }
        //    }
        //    for (int i = iDel; i < mNumVnow - 1; i++)
        //        m_uIndex[p, i] = m_uIndex[p, i + 1];

        //    m_uIndex[p, mNumVnow - 1] = m_uIndex[p, mNumVnow - 2];

        //    while (true)
        //    {
        //        m_uIndex[p, mNumVnow - 1]++;
        //        if (m_uIndex[p, mNumVnow - 1] >= mNumV)
        //            return false;

        //        matrixU = new double[lNumVnow, lNumVnow];
        //        for (int i = 0; i < lNumVnow; i++)
        //        {
        //            for (int j = 0; j < lNumVnow; j++)
        //            {
        //                for (int z = 0; z < mInitNumDataSet; z++)
        //                    matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //            }
        //        }
        //        rankU = RankOfU(matrixU, lNumVnow);

        //        if (rankU == lNumVnow)
        //            break;
        //    }
        //    return true;
        //}

        //public bool CalcStep4(int ySelectedIndex = 0)
        //{
        //    //SaveLog(">>CalcStep4()");
        //    int p = mNthVset;
        //    int attentionY = mAttentionY[ySelectedIndex];
        //    if (attentionY < 0) attentionY = 0;

        //    matrixY = new double[mNumVnow];
        //    matrixU = new double[mNumVnow, mNumVnow];

        //    //string strlog = "";
        //    //strlog += "MatrixY[] : \r\n";
        //    //StreamWriter wwr = new StreamWriter("matrixY.csv");
        //    for (int z = 0; z < mInitNumDataSet; z++)
        //    {
        //        for (int i = 0; i < mNumVnow; i++)
        //        {
        //            matrixY[i] += u[m_uIndex[p, i], z] * mDataY[attentionY, z];
        //            //wwr.Write(u[m_uIndex[p, i], z].ToString("E7") + "," + mDataY[attentionY, z].ToString("E7") + ",");
        //        }
        //        //wwr.Write("\r\n");
        //    }
        //    //wwr.Close();

        //    //MessageBox.Show("CCC");
        //    //for (int i = 0; i < mNumVnow; i++)
        //    //    strlog += matrixY[i].ToString("E7") + "\t";
        //    //strlog += "\r\nMatrixU[" + mNumVnow.ToString() + "] : \r\n";
        //    for (int i = 0; i < mNumVnow; i++)
        //    {
        //        for (int j = 0; j < mNumVnow; j++)
        //        {
        //            for (int z = 0; z < mInitNumDataSet; z++)
        //                matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //            //strlog += matrixU[i, j].ToString("E7") + "\t";
        //        }
        //        //strlog += "\r\n";
        //    }
        //    //MessageBox.Show("DDD");
        //    if (RankOfU(matrixU, mNumVnow) < mNumVnow)
        //    {
        //        //SaveLog("Lack of Rank");
        //        return false;
        //    }

        //    //MessageBox.Show("EEE");
        //    InverseU(ref matrixU, mNumVnow);
        //    //strlog += "\r\nInv matrixU[" + mNumVnow.ToString() + "] : \r\n";
        //    //for (int i = 0; i < mNumVnow; i++)
        //    //{
        //    //    for (int j = 0; j < mNumVnow; j++)
        //    //    {
        //    //        strlog += matrixU[i, j].ToString("E7") + "\t";
        //    //    }
        //    //    strlog += "\r\n";
        //    //}
        //    matrixC = new double[mNumVnow];
        //    MatrixCross(ref matrixU, ref matrixY, ref matrixC, mNumVnow);

        //    //strlog = "> Equation Coefs\r\n";
        //    //for (int i = 0; i < mNumVnow; i++)
        //    //{
        //    //    strlog += "\t" + m_uIndex[p, i].ToString() + "\t" + uIDreadable[m_uIndex[p, i]].ToString() + "\t" + matrixC[i].ToString("E4") + "\r\n";
        //    //}
        //    //SaveLog(strlog);
        //    return true;
        //}
        public int RankOfU(double[,] mbym, int dim)
        {
            int rank = 0;
            double[,] mat = new double[dim, dim];

            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    mat[i, j] = mbym[i, j]; // coefficients of our matrix

            for (int k = 0; k < dim; k++)
            {
                for (int i = k + 1; i < dim; i++)
                {
                    // Check if divider is equal to zero
                    if (mat[k, k] == 0)
                    {
                        // It is - pivot row
                        mat = RowPivot(mat, k);
                    }

                    double c = mat[i, k] / mat[k, k];

                    for (int k1 = 0; k1 < dim; k1++)
                    {
                        mat[i, k1] = mat[i, k1] - mat[k, k1] * c;
                    }
                }

                // Check if created row's elements sum is non-zero
                double sum = 0;

                for (int i = 0; i < dim; i++)
                {
                    sum += mat[k, i];
                }
                if (sum != 0) { rank++; } // Increase rank if sum of new row is non-zero.
            }
            //SaveLog(">>RankOfU() : " + rank.ToString());
            return rank;
        }
        private double[,] RowPivot(Double[,] matrix, int k)
        {
            // k - starting row to search for non-zero element
            for (int i = k + 1; i < matrix.GetLength(0); i++)
            {
                if (matrix[i, i] != 0)
                {
                    double[] x = new double[matrix.GetLength(1)];

                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        x[j] = matrix[k, j];
                        matrix[k, j] = matrix[i, j];
                        matrix[i, j] = x[j];
                    }
                    break;
                }
            }
            return matrix;
        }

        public void InverseU(ref double[,] invM, int dim)
        {
            //  Calculate Inverse Matrix of matrixU and save result to invM
            //double[,] copySrc = new double[dim, dim];

            //for (int i = 0; i < dim; ++i) // copy the values
            //    for (int j = 0; j < dim; ++j)
            //        copySrc[i, j] = invM[i, j];

            //double[,] result = MatrixInverse(copySrc, dim);
            //for (int i = 0; i < dim; ++i) // copy the values
            //    for (int j = 0; j < dim; ++j)
            //        invM[i, j] = result[i, j];

            int info;
            alglib.matinvreport rep;
            alglib.rmatrixinverse(ref invM, out info, out rep);
        }

        public static MathNet.Numerics.LinearAlgebra.Vector<double> SolveLeastSquares(double[,] A_raw, double[] b_raw)
        {
            var A = DenseMatrix.OfArray(A_raw); //Matrix<double>.Build.DenseOfArray(A_raw);
            var B = DenseVector.OfArray(b_raw);// MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(b_raw);

            // 1. SVD
            var svd = A.Svd();

            Matrix<double> U = svd.U;
            MathNet.Numerics.LinearAlgebra.Vector<double> S = svd.S;
            Matrix<double> VT = svd.VT;

            double tol = 1e-10;
            int estimatedRank = S.Count(s => s > tol);

            int m = A.RowCount;
            int n = A.ColumnCount;

            // Σ⁺ (수치적 rank 기반)
            var S_inv = Matrix<double>.Build.Dense(n, m, 0.0);
            for (int i = 0; i < estimatedRank; i++)
            {
                S_inv[i, i] = 1.0 / S[i];
            }

            // A⁺ = V * Σ⁺ * Uᵗ
            Matrix<double> A_pinv = VT.TransposeThisAndMultiply(S_inv).Multiply(U.Transpose());

            MathNet.Numerics.LinearAlgebra.Vector<double> X = A_pinv * B;

            // 잔차 계산
            MathNet.Numerics.LinearAlgebra.Vector<double> AX = A * X;
            MathNet.Numerics.LinearAlgebra.Vector<double> residual = B - AX;
            double residualNorm = residual.L2Norm();

            return X;
        }

        public double[] PseudoCircleOld(double[,] rawPoints, ref double[] normalVector)
        {
            int n = rawPoints.GetLength(0);
            var points = new MathNet.Numerics.LinearAlgebra.Vector<double>[n];

            for (int i = 0; i < n; i++)
            {
                points[i] = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] {
                rawPoints[i, 0],
                rawPoints[i, 1],
                rawPoints[i, 2]
                });
            }

            // 1. matrix 구성
            var mat = Matrix<double>.Build.Dense(n, 3);
            for (int i = 0; i < n; i++)
                mat.SetRow(i, points[i]);

            // 2. 중심점
            MathNet.Numerics.LinearAlgebra.Vector<double> centroid = mat.ColumnSums() / n;

            // 3. 중심화 (MapRows → 수동 대체)
            for (int i = 0; i < n; i++)
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> row = mat.Row(i) - centroid;
                mat.SetRow(i, row);
            }

            // 4. SVD → 평면 법선
            var svd = mat.Svd();
            MathNet.Numerics.LinearAlgebra.Vector<double> normal = svd.VT.Row(2); // 최소 특이값 방향

            normalVector[0] = normal[0];
            normalVector[1] = normal[1];
            normalVector[2] = normal[2];

            // 5. 평면 위 좌표계 (e1, e2) using Cross()
            MathNet.Numerics.LinearAlgebra.Vector<double> refVec = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 1 });
            MathNet.Numerics.LinearAlgebra.Vector<double> e1 = Cross(normal, refVec);
            if (e1.L2Norm() < 1e-6)
                e1 = Cross(normal, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] { 0, 1, 0 }));
            e1 = e1.Normalize(2);
            MathNet.Numerics.LinearAlgebra.Vector<double> e2 = Cross(normal, e1).Normalize(2);

            // 4. 평면 위에 점들 투영 (3D → 2D)
            var projected2D = new double[n, 2];
            for (int i = 0; i < n; i++)
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> d = points[i] - centroid;
                projected2D[i, 0] = d.DotProduct(e1);
                projected2D[i, 1] = d.DotProduct(e2);
            }

            // 5. 2D 원 피팅
            var A = Matrix<double>.Build.Dense(n, 3);
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(n);
            for (int i = 0; i < n; i++)
            {
                double x = projected2D[i, 0];
                double y = projected2D[i, 1];
                A[i, 0] = x;
                A[i, 1] = y;
                A[i, 2] = 1;
                b[i] = -(x * x + y * y);
            }

            var sol = A.Svd().Solve(b);
            double xc = -sol[0] / 2;
            double yc = -sol[1] / 2;
            double r = Math.Sqrt(xc * xc + yc * yc - sol[2]);

            MathNet.Numerics.LinearAlgebra.Vector<double> AX = A * sol;
            MathNet.Numerics.LinearAlgebra.Vector<double> residual = b - AX;
            double residualNorm = residual.L2Norm();

            // 6. 중심 3D 좌표 복원
            MathNet.Numerics.LinearAlgebra.Vector<double> center3D = centroid + xc * e1 + yc * e2;
            double[] res = new double[4] { center3D[0], center3D[1], center3D[2],r };
            return res;
        }

        public double[] PseudoCircle(double[,] rawPoints, ref double[] normalVector)
        {
            int n = rawPoints.GetLength(0);
            var points = new MathNet.Numerics.LinearAlgebra.Vector<double>[n];

            for (int i = 0; i < n; i++)
            {
                points[i] = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] {
                rawPoints[i, 0],
                rawPoints[i, 1],
                rawPoints[i, 2]
                });
            }

            // 1. matrix 구성
            var mat = Matrix<double>.Build.Dense(n, 3);
            for (int i = 0; i < n; i++)
                mat.SetRow(i, points[i]);

            // 2. 중심점
            MathNet.Numerics.LinearAlgebra.Vector<double> centroid = mat.ColumnSums() / n;

            // 3. 중심화 (MapRows → 수동 대체)
            for (int i = 0; i < n; i++)
            {
                MathNet.Numerics.LinearAlgebra.Vector<double> row = mat.Row(i) - centroid;
                mat.SetRow(i, row);
            }

            // 4. SVD → 평면 법선
            var svd = mat.Svd();
            MathNet.Numerics.LinearAlgebra.Vector<double> normal = svd.VT.Row(2); // 최소 특이값 방향

            normalVector[0] = normal[0];
            normalVector[1] = normal[1];
            normalVector[2] = normal[2];

            // 3. 원래 점들을 평면에 투영
            MathNet.Numerics.LinearAlgebra.Vector<double>[] projected = new MathNet.Numerics.LinearAlgebra.Vector<double>[n];
            for (int i = 0; i < n; i++)
            {
                var p = points[i];
                var v = p - centroid;
                var dist = v.DotProduct(normal);
                projected[i] = p - dist * normal;
            }

            // 4. 투영된 점들의 무게중심 (2D 원점)
            var projMat = Matrix<double>.Build.Dense(n, 3);
            for (int i = 0; i < n; i++) projMat.SetRow(i, projected[i]);
            var centerOnPlane = projMat.ColumnSums() / n;

            // 5. 투영점 중 가장 먼 두 점으로 X축 정의
            int i1 = 0, i2 = 1;
            double maxDist = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double d = (projected[i] - projected[j]).L2Norm();
                    if (d > maxDist) { maxDist = d; i1 = i; i2 = j; }
                }
            }
            MathNet.Numerics.LinearAlgebra.Vector<double> eX = (projected[i2] - projected[i1]).Normalize(2);
            MathNet.Numerics.LinearAlgebra.Vector<double> eY = Cross(normal, eX).Normalize(2); // 평면상 직교축

            // 6. 평면 2D 좌표로 변환
            var points2D = new double[n, 2];
            for (int i = 0; i < n; i++)
            {
                var d = projected[i] - centerOnPlane;
                points2D[i, 0] = d.DotProduct(eX);
                points2D[i, 1] = d.DotProduct(eY);
            }

            // 7. Pseudo Circle 피팅 (Ax + By + C = -(x² + y²))
            var A = Matrix<double>.Build.Dense(n, 3);
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(n);
            for (int i = 0; i < n; i++)
            {
                double x = points2D[i, 0];
                double y = points2D[i, 1];
                A[i, 0] = x;
                A[i, 1] = y;
                A[i, 2] = 1;
                b[i] = -(x * x + y * y);
            }

            var sol = A.Svd().Solve(b);
            double xc = -sol[0] / 2;
            double yc = -sol[1] / 2;
            double r = Math.Sqrt(xc * xc + yc * yc - sol[2]);

            MathNet.Numerics.LinearAlgebra.Vector<double> AX = A * sol;
            MathNet.Numerics.LinearAlgebra.Vector<double> residual = b - AX;
            double residualNorm = residual.L2Norm();

            // 8. 중심 복원: 평면 위 중심 + 3D 변환
            MathNet.Numerics.LinearAlgebra.Vector<double> circleCenter3D = centerOnPlane + xc * eX + yc * eY;

            double[] res = new double[4] { circleCenter3D[0], circleCenter3D[1], circleCenter3D[2], r };
            return res;
        }
        public static MathNetVector Cross(MathNetVector a, MathNetVector b)
        {
            return MathNetVector.Build.DenseOfArray(new double[]
            {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
            });
        }
        public static double[] Solve(Func<double[], double[]> A, Func<double[], double[]> AT, int m, int n, double[] b, int maxIter = 1000, double tol = 1e-10)
        {
            double[] x = new double[n]; // 초기 추정값 0
            double[] u = (double[])b.Clone();
            double beta = Norm(u);
            if (beta == 0) return x;

            for (int i = 0; i < m; i++) u[i] /= beta;

            double[] v = AT(u);
            double alpha = Norm(v);
            for (int i = 0; i < n; i++) v[i] /= alpha;

            double[] w = (double[])v.Clone();
            double phiBar = beta;
            double rhoBar = alpha;

            for (int iter = 0; iter < maxIter; iter++)
            {
                // u = A * v - alpha * u
                double[] Av = A(v);
                for (int i = 0; i < m; i++) u[i] = Av[i] - alpha * u[i];
                beta = Norm(u);
                if (beta == 0) break;
                for (int i = 0; i < m; i++) u[i] /= beta;

                // v = A^T * u - beta * v
                double[] Atu = AT(u);
                for (int i = 0; i < n; i++) v[i] = Atu[i] - beta * v[i];
                alpha = Norm(v);
                if (alpha == 0) break;
                for (int i = 0; i < n; i++) v[i] /= alpha;

                // Givens rotation
                double rho = Math.Sqrt(rhoBar * rhoBar + beta * beta);
                double c = rhoBar / rho;
                double s = beta / rho;
                double theta = s * alpha;
                rhoBar = -c * alpha;
                double phi = c * phiBar;
                phiBar = s * phiBar;

                // update solution
                for (int i = 0; i < n; i++) x[i] += (phi / rho) * w[i];

                // update direction vector w
                for (int i = 0; i < n; i++) w[i] = v[i] - (theta / rho) * w[i];

                if (phiBar * phiBar < tol * tol)
                    break;
            }

            return x;
        }

        private static double Norm(double[] vec)
        {
            double sum = 0.0;
            foreach (var val in vec)
                sum += val * val;
            return Math.Sqrt(sum);
        }

        public void MatrixCross(ref double[,] dimbydim, ref double[] dimby1, ref double[] res, int dim)
        {
            //  Calculate [ dim x dim ] x [ dim ]
            for (int i = 0; i < dim; i++)
            {
                res[i] = 0;
                for (int j = 0; j < dim; j++)
                {
                    res[i] += dimbydim[i, j] * dimby1[j];
                }
            }
        }
        public void MatrixCross(ref double[,] dimbydimL, ref double[,] dimbydimR, ref double[,] dimbydimRes, int dim)
        {
            //  Calculate [ dim x dim ] x [ dim ]
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    dimbydimRes[i, j] = 0;
                    for (int k = 0; k < dim; k++)
                        dimbydimRes[i, j] += dimbydimL[i, k] * dimbydimR[k, j];
                }
            }
        }
        public void MatrixCross(ref double[,] mbyn, ref double[,] nbym, ref double[,] mbym, int m, int n)
        {
            if (m > n) return;
            //  Calculate [ m x n ] x [ n x m ]
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    mbym[i, j] = 0;
                    for (int k = 0; k < n; k++)
                        mbym[i, j] += mbyn[i, k] * nbym[k, j];
                }
            }
        }
        public void MatrixCross(ref double[] justn, ref double[,] nbym, ref double[] justm, int n, int m)
        {
            //  Calculate [ n ] x [ n x m ] = [ m ]
            for (int j = 0; j < m; j++)
            {
                justm[j] = 0;
                for (int k = 0; k < n; k++)
                    justm[j] += justn[k] * nbym[k, j];
            }
        }
        public void MatrixCross(ref double[,] mbyn, ref double[,] nbyp, ref double[,] mbyp, int m, int n, int p)
        {
            if (m > n) return;
            //  Calculate [ m x n ] x [ n x m ]
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    mbyp[i, j] = 0;
                    for (int k = 0; k < n; k++)
                        mbyp[i, j] += mbyn[i, k] * nbyp[k, j];
                }
            }
        }
        public void MatrixCross(ref double[,] mbyn, ref double[] nby1, ref double[] mby1, int m, int n)
        {
            //  Calculate [ m x n ] x [ n x 1 ]
            for (int i = 0; i < m; i++)
            {
                mby1[i] = 0;
                for (int j = 0; j < n; j++)
                    mby1[i] += mbyn[i, j] * nby1[j];
            }
        }
        public double MatrixInnerProduct(ref double[] dimL, ref double[] dimR, int dim)
        {
            //  Calculate [ dim x dim ] x [ dim ]
            double res = 0;
            for (int i = 0; i < dim; i++)
            {
                res += dimL[i] * dimR[i];
            }
            return res;
        }

        public void MatrixTranspose(double[,] mbyn, ref double[,] nbym, int n, int m)
        {
            //  Calculate [ dim x dim ] x [ dim ]
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    nbym[j, i] = mbyn[i, j];
        }

        //public void GetPP(ref double[] pp, int numRow, int numData)
        //{
        //    double min = 0;
        //    double max = 0;

        //    int p = mNthVset;

        //    for (int i = 0; i < numRow; i++)
        //    {
        //        min = 999999999;
        //        max = -999999999;
        //        for (int j = 0; j < numData; j++)
        //        {
        //            if (u[m_uIndex[p, i], j] < min)
        //                min = u[m_uIndex[p, i], j];
        //            if (u[m_uIndex[p, i], j] > max)
        //                max = u[m_uIndex[p, i], j];
        //        }
        //        pp[i] = max - min;
        //    }
        //}

        //public void RunNonstopProcess(String rootDir)
        //{
        //    m_bNonStopProcessDone = false;
        //    ResetLog();
        //    int nPort = GetRealParameters(rootDir);
        //    //MessageBox.Show("mFZC Port = " + nPort.ToString());
        //    for (int i = 0; i < nPort; i++)
        //        NonStopProcess(i);
        //    m_bNonStopProcessDone = true;
        //}
        //public bool NonStopProcess(int iPort = 0, bool IsOptimize = true)
        //{
        //    //CalcStep1();
        //    //SaveLog(">DistParams[" + iPort + "] : " + mMarkDistNear[iPort].ToString("F3") + "\t" + mMarkDistFar[iPort].ToString("F3") + "\t" + mMarkDistDecenter[iPort].ToString("F3"));
        //    GenerateSourceData(mMarkDistNear[iPort], mMarkDistFar[iPort], mMarkDistDecenter[iPort]);
        //    int ySelectedIndex = 0;
        //    int p = 0;
        //    int nestedCount = 0;
        //    int itrCount = 0;
        //    bool res = true;

        //    if (IsOptimize)
        //        mProcessMsg = "";

        //    ApplyCondition(true);

        //    while (true)
        //    {
        //        p = 0;
        //        nestedCount = 0;
        //        m_uIndex[p, 0] = 0;
        //        m_uIndex[p, 1] = 1;
        //        for (int qq = 2; qq < mNumV; qq++)
        //        {
        //            m_uIndex[p, qq] = 0;
        //            m_uIndex[p, qq] = 0;
        //        }
        //        mNumVnow = 2;
        //        itrCount = 0;
        //        mNthVset = p;
        //        CalcStep1();
        //        if (mAttentionY[ySelectedIndex] > -1)
        //        {
        //            mProcessMsg += ">>For Y " + ySelectedIndex + "\r\n";
        //            //SaveLog(mProcessMsg);
        //            CalcStep2(ref p, ref nestedCount, ySelectedIndex);
        //            CalcStep3();
        //            CalcStep4(ySelectedIndex);

        //            res = true;

        //            while (true)
        //            {
        //                mErrHistory[itrCount] = CalcRssError(ySelectedIndex);
        //                mProcessMsg = "RSS Error \t" + mErrHistory[itrCount].ToString("E5") + " \titrCount = \t" + itrCount.ToString() + " \t mNumVnow \t" + mNumVnow + "\r\n";
        //                //SaveLog(mProcessMsg);

        //                if (IsOptimize && (mErrHistory[itrCount] < mErrThreshold))
        //                {
        //                    //SaveLog("Finish Good");
        //                    break;
        //                }

        //                if (++itrCount > 99)
        //                {
        //                    //SaveLog("Finish 0");
        //                    break;
        //                }

        //                if (mNumVnow < mMaxDimensionOfEquation)
        //                {
        //                    //if (itrCount < 2)
        //                    //    SaveLog("Call CalcStep3");
        //                    if (m_uIndex[mNthVset, mNumVnow] >= mNumV - 1)
        //                    {
        //                        //mProcessMsg = "No more uIndex\r\n";
        //                        break;
        //                    }

        //                    if (CalcStep3())    //  CalcStep3() : rank 가 맞는 변수를 추가하는 프로세스
        //                    {
        //                        //CalcStep4();
        //                        CalcStep4(ySelectedIndex);  //  CalcStep4() : 주어진 변수로부터 LMS 계수를 구하는 프로세스
        //                        mErrHistory[itrCount] = CalcRssError(ySelectedIndex);
        //                        //SaveLog("Old Err = " + mErrHistory[itrCount - 1].ToString("E5") + "\tNew Err = " + mErrHistory[itrCount].ToString("E5"));

        //                        if (mNumVnow > 2)
        //                        {
        //                            if (mErrHistory[itrCount] > mErrHistory[itrCount - 1])
        //                            {
        //                                // Error 가 너무 커진 경우 마지막으로 추가된 변수를 제거해야 한다.
        //                                //if (itrCount < 2)
        //                                //    SaveLog("Remove Lastly Added Vars");
        //                                RemLastAddedVars();
        //                                //CalcStep3();
        //                                CalcStep4(ySelectedIndex);
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                //if (itrCount < 2)
        //                                //    SaveLog("Call RemUnsensMinSensVars A");
        //                                RemUnsensMinSensVars(ySelectedIndex);
        //                            }
        //                        }
        //                    }
        //                }
        //                if (IsOptimize && (mNumVnow > 8) && mErrHistory[itrCount] > mErrHistory[itrCount - 1])
        //                {
        //                    //if (itrCount < 2)
        //                    //    SaveLog("Call RemUnsensMinSensVars B");
        //                    res = RemUnsensMinSensVars(ySelectedIndex);
        //                }
        //                else if (!IsOptimize && (mNumVnow == mMaxDimensionOfEquation))
        //                {
        //                    //SaveLog("Finish 1");
        //                    break;
        //                }

        //                if (mProcessMsg == "No more uIndex")
        //                {
        //                    //SaveLog("Finish 2");
        //                    break;
        //                }
        //                if (mNumVnow > mMaxDimensionOfEquation)
        //                {
        //                    //SaveLog("Finish 3");
        //                    break;
        //                }
        //            }
        //            if (mProcessMsg != "No more uIndex")
        //                RemUnsensMinSensVars(ySelectedIndex);
        //            //else
        //            //    p--;

        //            //mProcessMsg += ">Found Solution\r\n";

        //            string strlog = p.ToString() + "> Final Equation Coefs : Port " + iPort.ToString() + " Yindex " + ySelectedIndex.ToString() + " " + mNumVnow + " \r\n";
        //            double lLastErr = CalcRssError(ySelectedIndex);
        //            mProcessMsg += "RSS Error \t" + lLastErr.ToString("E5") + " \titrCount = \t" + itrCount.ToString() + " \t mNumVnow \t" + mNumVnow + "\r\n";
        //            for (int i = 0; i < mNumVnow; i++)
        //            {
        //                strlog += m_uIndex[p, i].ToString() + "\t" + uIDreadable[m_uIndex[p, i]].ToString() + "\t" + matrixC[i].ToString("E4") + "\t::";
        //                strlog += (uIDreadable[m_uIndex[p, i]] / 1000).ToString()
        //                          + ":" + ((uIDreadable[m_uIndex[p, i]] / 100) % 10).ToString()
        //                          + ":" + ((uIDreadable[m_uIndex[p, i]] / 10) % 10).ToString()
        //                          + ":" + (uIDreadable[m_uIndex[p, i]] % 10).ToString();
        //                strlog += "\r\n";
        //            }
        //            mProcessMsg += strlog;
        //            mResultMsg[iPort * 2 + ySelectedIndex] = mProcessMsg;
        //            SaveLog(mProcessMsg);
        //            mProcessMsg = "";
        //            if (ySelectedIndex == 0)
        //            {
        //                for (int i = 0; i < mNumVnow; i++)
        //                {
        //                    m_LastUIndex[iPort, i] = m_uIndex[p, i];
        //                    matrixC_U[iPort, i] = matrixC[i];
        //                }
        //                mExtNumVnow[iPort, 0] = mNumVnow;

        //            }
        //            else if (ySelectedIndex == 1)
        //            {
        //                for (int i = 0; i < mNumVnow; i++)
        //                {
        //                    m_LastVIndex[iPort, i] = m_uIndex[p, i];
        //                    matrixC_V[iPort, i] = matrixC[i];
        //                }
        //                mExtNumVnow[iPort, 1] = mNumVnow;
        //            }
        //            ySelectedIndex++;
        //        }
        //        else
        //            break;
        //    }

        //    return true;
        //}
        //public double CalcRssError(int ySelectedIndex = 0)
        //{
        //    double res = 0;
        //    double err = 0;
        //    double curValue = 0;
        //    int p = mNthVset;
        //    double ideal = 0;
        //    int attentionY = mAttentionY[ySelectedIndex];
        //    if (attentionY < 0) attentionY = 0;

        //    //StreamWriter wr;
        //    //string rssFile = "RssErr_" + attentionY.ToString() + ".csv";
        //    //wr = new StreamWriter(rssFile);
        //    for (int z = 0; z < mInitNumDataSet; z++)
        //    {
        //        curValue = 0;
        //        for (int j = 0; j < mNumVnow; j++)
        //        {
        //            curValue += matrixC[j] * u[m_uIndex[p, j], z];
        //        }
        //        err = curValue - mDataY[attentionY, z];
        //        //wr.WriteLine(curValue.ToString("E6") + "," + mDataY[attentionY, z].ToString("E6") + "," + err.ToString("E6"));
        //        res += (err * err);
        //        ideal += mDataY[attentionY, z] * mDataY[attentionY, z];
        //    }
        //    //wr.Close();

        //    res = Math.Sqrt(res / mInitNumDataSet) / Math.Sqrt(ideal / mInitNumDataSet);

        //    //SaveLog("RssError = " + res.ToString("F5"));
        //    return res;
        //}

        //public void CalcVarSensitivity(ref double[] lsens, bool IsShowLog = true)
        //{
        //    //SaveLog(">>CalcVarSensitivity()");
        //    double[] pp = new double[mNumVnow];
        //    GetPP(ref pp, mNumVnow, mInitNumDataSet);
        //    int p = mNthVset;

        //    for (int i = 0; i < mNumVnow; i++)
        //    {
        //        lsens[i] = matrixC[i] * pp[i];
        //    }
        //    if (IsShowLog)
        //    {
        //        string strlog = "\r\n>>CalcVarSensitivity()y\r\n";
        //        for (int i = 0; i < mNumVnow; i++)
        //        {
        //            strlog += m_uIndex[p, i].ToString() + "\t" + uIDreadable[m_uIndex[p, i]].ToString() + "\t" + lsens[i].ToString("E5") + "\r\n";
        //        }
        //        SaveLog(strlog);
        //    }
        //}

        //public void RemUnsensVars(int ySelectedIndex = 0)
        //{
        //    //SaveLog(">>RemUnsensVars ");

        //    int p = mNthVset;
        //    double[] lsens = new double[mNumVnow];

        //    //CalcVarSensitivity(ref lsens, false);
        //    int lNumVnow = mNumVnow;
        //    bool[] l_uOn = new bool[mNumVnow];
        //    int[] l_uIndex = new int[mNumVnow];
        //    for (int i = 0; i < lNumVnow; i++)
        //    {
        //        if (Math.Abs(matrixC[i]) < 1.0e-22)
        //            l_uOn[i] = false;
        //        else
        //            l_uOn[i] = true;
        //    }
        //    int k = 0;
        //    for (int i = 0; i < lNumVnow; i++)
        //    {
        //        if (l_uOn[i])
        //            l_uIndex[k++] = m_uIndex[p, i];
        //    }
        //    for (int i = 0; i < k; i++)
        //    {
        //        m_uIndex[p, i] = l_uIndex[i];
        //    }


        //    mNumVnow = k;
        //    matrixY = new double[mNumVnow];
        //    matrixC = new double[mNumVnow];
        //    matrixU = new double[mNumVnow, mNumVnow];

        //    for (int i = 0; i < mNumVnow; i++)
        //    {
        //        for (int j = 0; j < mNumVnow; j++)
        //        {
        //            for (int z = 0; z < mInitNumDataSet; z++)
        //                matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //        }
        //    }
        //    CalcStep4(ySelectedIndex);
        //}

        //public bool RemUnsensMinSensVars(int ySelectedIndex = 0)
        //{
        //    //SaveLog(">>RemUnsensMinSensVars()");
        //    int p = mNthVset;
        //    double[] lsens = new double[mNumVnow];

        //    CalcVarSensitivity(ref lsens, false);   // CalcVarSensitivity() : 각 변수별 감도를 구한다.

        //    //  감도가 일정값 이하인 변수를 제거한다.
        //    //  감도가 가장 낮은 변수를 제거한다.

        //    int lNumVnow = mNumVnow;
        //    bool[] l_uOn = new bool[mNumVnow];
        //    int[] l_uIndex = new int[mNumVnow];
        //    double lminSens = 999999;
        //    double lminSens2 = 999999;
        //    int lminSensIndex = -1;
        //    int[] lminSensIndex2 = new int[100];
        //    int count2nd = 0;
        //    for (int i = 0; i < lNumVnow; i++)
        //    {
        //        if (uOn[m_uIndex[p, i]] > 0)
        //            l_uOn[i] = true;
        //        if (i < 4)
        //        {
        //            if (matrixC[i] < 1.0e-10)
        //            {
        //                lminSens = Math.Abs(lsens[i]);
        //                lminSensIndex = i;
        //            }
        //        }
        //        else if (i < 14)
        //        {
        //            if (matrixC[i] < 1.0e-14)
        //            {
        //                lminSens = Math.Abs(lsens[i]);
        //                lminSensIndex = i;
        //            }
        //        }
        //        else if (i < 34)
        //        {
        //            if (matrixC[i] < 1.0e-17)
        //            {
        //                lminSens = Math.Abs(lsens[i]);
        //                lminSensIndex = i;
        //            }
        //        }
        //        //if (m_uIndex[p, i] == 3) continue;
        //        if ((Math.Abs(lsens[i]) < lminSens) && (lNumVnow > 5))
        //        {
        //            lminSens = Math.Abs(lsens[i]);
        //            lminSensIndex = i;
        //        }
        //        if (Math.Abs(lsens[i]) < mMinSensitivity)
        //        {
        //            lminSensIndex2[count2nd] = i;
        //            count2nd++;
        //        }
        //    }
        //    if (count2nd == 0 && lminSensIndex < 0)
        //    {
        //        return false;
        //    }
        //    //SaveLog(" B removed : " + lminSensIndex.ToString());

        //    string lstr = "count2nd= " + count2nd.ToString() + "\r\n";
        //    if (lminSensIndex >= 0 && lminSens < (mMinSensitivity * 5) && count2nd == 0)
        //    {
        //        lstr += "Removed lminSensIndex= " + lminSensIndex.ToString() + "\r\n";
        //        l_uOn[lminSensIndex] = false;
        //        uOn[m_uIndex[p, lminSensIndex]] = -1;
        //    }
        //    for (int i = 0; i < count2nd; i++)
        //    {
        //        //SaveLog("Removed Index: " + i.ToString());
        //        l_uOn[lminSensIndex2[i]] = false;
        //        uOn[m_uIndex[p, lminSensIndex2[i]]] = -1;
        //    }
        //    int k = 0;
        //    for (int i = 0; i < lNumVnow; i++)
        //    {
        //        //SaveLog(" l_uOn[" + i.ToString() +"]=" + l_uOn[i].ToString());
        //        if (l_uOn[i])
        //            l_uIndex[k++] = m_uIndex[p, i];
        //    }
        //    mNumVnow = k;
        //    lstr += "reduced mNumVnow= " + mNumVnow.ToString() + "\r\n";
        //    for (int i = 0; i < k; i++)
        //    {
        //        m_uIndex[p, i] = l_uIndex[i];
        //        lstr += m_uIndex[p, i].ToString() + "-";
        //    }
        //    //SaveLog(lstr);
        //    matrixY = new double[mNumVnow];
        //    //matrixC = new double[mNumVnow];
        //    matrixU = new double[mNumVnow, mNumVnow];
        //    bool res = true;
        //    res = CalcStep3();    //  변수 1개 추가
        //    if (!res)
        //        return res;
        //    res = CalcStep4(ySelectedIndex);
        //    if (!res)
        //        return res;

        //    return true;
        //}
        //public void RemLastAddedVars()
        //{
        //    uOn[m_uIndex[mNthVset, mNumVnow - 1]] = -1;

        //    mNumVnow--;
        //    int p = mNthVset;

        //    matrixY = new double[mNumVnow];
        //    matrixU = new double[mNumVnow, mNumVnow];

        //    for (int i = 0; i < mNumVnow; i++)
        //    {
        //        for (int j = 0; j < mNumVnow; j++)
        //        {
        //            for (int z = 0; z < mInitNumDataSet; z++)
        //                matrixU[i, j] += u[m_uIndex[p, i], z] * u[m_uIndex[p, j], z];
        //        }
        //    }
        //}

        //public void ResetLog()
        //{
        //    if (File.Exists("GMVLR_log.txt"))
        //        File.Delete("GMVLR_log.txt");
        //}
        //public void SaveLog(string lstr)
        //{
        //    StreamWriter wr = File.AppendText("GMVLR_log.txt");
        //    wr.WriteLine(lstr);
        //    wr.Close();
        //}
        static double[,] MatrixInverse3x3(double[,] m)
        {
            double[,] result = new double[3, 3];

            double sub1122_2112 = m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2];
            double sub1022_1220 = m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0];
            double sub1021_1120 = m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0];

            //double det = m[0,0] * (m[1,1] * m[2,2] - m[2,1] * m[1,2]) - m[0,1] * (m[1,0] * m[2,2] - m[1,2] * m[2,0]) + m[0,2] * (m[1,0] * m[2,1] - m[1,1] * m[2,0]);
            double det = m[0, 0] * sub1122_2112 - m[0, 1] * sub1022_1220 + m[0, 2] * sub1021_1120;
            if (det == 0)
                return result;

            result[0, 0] = sub1122_2112 / det ;//   (m[1,1] * m[2,2] - m[2,1] * m[1,2]) / det; 
            result[0,1] = (m[0,2] * m[2,1] - m[0,1] * m[2,2]) / det; 
            result[0,2] = (m[0,1] * m[1,2] - m[0,2] * m[1,1]) / det;

            result[1, 0] = - sub1022_1220 / det ; // (m[1,2] * m[2,0] - m[1,0] * m[2,2]) / det; 
            result[1,1] = (m[0,0] * m[2,2] - m[0,2] * m[2,0]) / det; 
            result[1,2] = (m[1,0] * m[0,2] - m[0,0] * m[1,2]) / det;

            result[2, 0] = sub1021_1120 / det ; //   (m[1,0] * m[2,1] - m[2,0] * m[1,1]) / det; 
            result[2,1] = (m[2,0] * m[0,1] - m[0,0] * m[2,1]) / det; 
            result[2,2] = (m[0,0] * m[1,1] - m[1,0] * m[0,1]) / det;
            return result;
        }
        static double[,] MatrixInverse(double[,] matrix, int dim)
        {
            int n = dim;
            double[,] result = new double[dim, dim];

            for (int i = 0; i < dim; ++i) // copy the values
                for (int j = 0; j < dim; ++j)
                    result[i, j] = matrix[i, j];

            int[] perm;
            int toggle;
            double[,] lum = MatrixDecompose(matrix, out perm, out toggle, dim);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }
                double[] x = HelperSolve(lum, b, dim); // 

                for (int j = 0; j < n; j++)
                    result[j, i] = x[j];
            }
            return result;
        }

        static double[,] MatrixDecompose(double[,] matrix, out int[] perm, out int toggle, int dim)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = dim;
            int cols = dim; // assume square
            int n = dim; // convenience

            double[,] result = new double[dim, dim];

            for (int i = 0; i < dim; ++i) // copy the values
                for (int j = 0; j < dim; ++j)
                    result[i, j] = matrix[i, j];

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; i++) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps.
                        // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            for (int j = 0; j < n - 1; j++) // each column
            {
                double colMax = result[j, j]; // find largest val in col
                colMax = colMax < 0 ? -colMax : colMax;
                int pRow = j;
                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i)
                {
                    if (Math.Abs(result[i, j]) > colMax)
                    {
                        colMax = result[i, j];
                        colMax = colMax < 0 ? -colMax : colMax;
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowTmp = new double[n];
                    for (int k = 0; k < n; k++)
                        rowTmp[k] = result[pRow, k];

                    for (int k = 0; k < n; k++)
                    {
                        result[pRow, k] = result[j, k];
                        result[j, k] = rowTmp[k];
                    }

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j, j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row, j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    //double[] rowPtr = result[goodRow];
                    //result[goodRow] = result[j];
                    //result[j] = rowPtr;

                    double[] rowTmp = new double[n];
                    for (int k = 0; k < n; k++)
                        rowTmp[k] = result[goodRow, k];

                    for (int k = 0; k < n; k++)
                    {
                        result[goodRow, k] = result[j, k];
                        result[j, k] = rowTmp[k];
                    }


                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i, j] /= result[j, j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i, k] -= result[i, j] * result[j, k];
                    }
                }
            } // main j column loop

            return result;
        } // MatrixDecompose

        static double[] HelperSolve(double[,] luMatrix, double[] b, int dim)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = dim;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; i++)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum;
            }
            x[n - 1] /= luMatrix[n - 1, n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum / luMatrix[i, i];
            }

            return x;
        }

        //private void GenerateSourceData(double lnear = 9, double lfar = 12, double ldecenter = 0)
        //{
        //    string srcFile = "";

        //    double L1_L2_InitDecenter = ldecenter;

        //    //  X drv
        //    //// Near Mark for 18.5deg ~ 29.5deg
        //    //double[] L1 = new double[3] { -1.21, 2.82, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { -1.66, 3.84, 12.0 }; //    Far 
        //    //double Initial_Theta = 135; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 18.5; //  0 는 0deg 로써 중심상태

        //    // Near Mark for 7.0deg ~ 19.0deg
        //    //double[] L1 = new double[3] { -0.35, 1.51, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { -0.48, 2.07, 12.0 }; //    Far 
        //    //double Initial_Theta = 135; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 7.0; //  0 는 0deg 로써 중심상태

        //    // Near Mark for 18.5deg ~ 29.5deg , Y 7.0 ~ 11.0
        //    //double[] L1 = new double[3] { 0.51, 3.60, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { 0.61, 4.90, 12.0 }; //    Far 
        //    //double Initial_Theta = 142.0; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 18.5; //  0 는 0deg 로써 중심상태


        //    //  Y drv
        //    // Near Mark for 9.3deg ~ 15.3deg
        //    //double[] L1 = new double[3] { 3.54, 0, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { 4.68, 0, 12.0 }; //    Far 
        //    //double Initial_Theta = 144.3; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 0; //  0 는 0deg 로써 중심상태

        //    //// Near Mark for X 3.0deg ~ 10.0deg
        //    //double[] L1 = new double[3] { 1.79, 0, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { 2.36, 0, 12.0 }; //    Far 
        //    //double Initial_Theta = 138.0; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 0; //  0 는 0deg 로써 중심상태

        //    ////Near Mark for X = -3.5deg ~3.5deg Y = -7deg ~ 7deg
        //    double[] L1 = new double[3] { 0, 0, 9.3 }; // 
        //    double[] L2 = new double[3] { 0, 0, 12.0 }; // 
        //    double Initial_Theta = 135; //  135 는 135deg 로써 중심상태
        //    double Initial_Psi = 0; //  0 는 0deg 로써 중심상태

        //    // Near Mark for X -3.0deg ~ -10.0deg
        //    //double[] L1 = new double[3] { -1.79, 0, 9.0 }; //    Near
        //    //double[] L2 = new double[3] { -2.36, 0, 12.0 }; //    Far 
        //    //double Initial_Theta = 144.0; //  135 는 135deg 로써 중심상태
        //    //double Initial_Psi = 0; //  0 는 0deg 로써 중심상태

        //    double[] Xtheta = new double[50000];
        //    double[] Xpsi = new double[50000];
        //    double[] Xheight = new double[50000];

        //    L1[2] = lnear;
        //    L2[2] = lfar;

        //    if (srcFile == "")
        //    {
        //        srcFile = "C:\\Lens1GTest\\DoNotTouch\\DefaultSrc_" + (lnear * 1000).ToString("F0") + "_" + (lfar * 1000).ToString("F0") + "_" + (L1_L2_InitDecenter * 1000).ToString("F0") + ".txt";
        //    }

        //    // center of prism surface

        //    int k = 0;
        //    double h, M_PI;
        //    double[] N0 = new double[3] { 0, 0, 0 };
        //    double[] N0res = new double[3] { 0, 0, 0 };
        //    double[] N0org = new double[3] { 0, 0, 0 };
        //    double[,] Rr = new double[3, 3];
        //    double[,] R = new double[3, 3];
        //    double[,] u = new double[4, 250000];
        //    double[,] u_tmp = new double[4, 250000];
        //    double[] uErr1 = new double[250000];
        //    double[] uErr2 = new double[250000];

        //    double[] Rc = new double[3] { 0, 0, 0 };

        //    double[] Xi = new double[3] { 1, 0, 0 };
        //    double[] Xo = new double[3];
        //    double[] Nr = new double[3];
        //    double[] N0_L1 = new double[3];
        //    double[] N0_L2 = new double[3];
        //    double[] Pr1 = new double[3];
        //    double[] Pr2 = new double[3];
        //    double[] X1 = new double[2];
        //    double[] X2 = new double[2];
        //    double[] X1org = new double[2];
        //    double[] X2org = new double[2];
        //    double Xshift = 0;
        //    double Yshift = 0;
        //    double theta = 0;
        //    double psi = 0;
        //    //double ct = 0;
        //    //double st = 0;
        //    //double cp = 0;
        //    //double sp = 0;
        //    double[,] Ry = new double[3, 3];
        //    double[,] Rz = new double[3, 3];
        //    double ii;

        //    double[] surfNi0 = new double[3] { 1, 0, 0 };
        //    double[] surfNr0 = new double[3] { 1, 0, 1 };   //  {0.707107, 0, 0.707107};
        //    double[] surfNo0 = new double[3] { 0, 0, 1 };

        //    double[] surfPi0 = new double[3] { 2.54, 0, 0 };    //  mm
        //    double[] surfPo0 = new double[3] { 0, 0, 2.54 };    //  mm
        //    double[] beamV = new double[3] { 1, 0, 0 };
        //    double rIndex = 1.52;
        //    //double[] markP0 = new double[3] { 0, 0, 8.5 };  //  Near Mark   L1
        //    //double[] markP1 = new double[3] { 0, 0, 11.0 }; //  Far Mark    L2

        //    double[] resMarkP = new double[3];
        //    double[] resMmirror1 = new double[3];
        //    double[] resMmirror2 = new double[3];

        //    double[] imgP_L1 = new double[3] { 10, 0.0, 0.0 };
        //    double[] imgP_L2 = new double[3] { 10, 0.0, 0.0 };
        //    double[] comp_imgP_L1 = new double[3];
        //    double[] comp_imgP_L2 = new double[3];
        //    //

        //    M_PI = Math.Asin(1) * 2;
        //    for (int hi = 0; hi < 1; hi++)
        //    {
        //        h = hi * 0.06;
        //        N0[0] = h / 1.41421356; N0[1] = 0; N0[2] = h / 1.41421356;
        //        // X1org , X2org
        //        //for (double xi = 0; xi <= 0; xi += 0.1)
        //        //{
        //        //    Xshift = xi;
        //        //    cp = 0;
        //        //    for (double yi = 0; yi <= 0; yi += 0.1)
        //        //    {
        //        for (double xi = -0.2; xi <= 0.2; xi += 0.08)
        //        {
        //            Xshift = xi;
        //            //cp = 0;
        //            for (double yi = -0.2; yi <= 0.2; yi += 0.08)
        //            {
        //                Yshift = yi;

        //                L1[0] = Xshift;
        //                L1[1] = -L1_L2_InitDecenter + Yshift;
        //                L2[1] = L1_L2_InitDecenter;

        //                theta = Initial_Theta / 180.0 * M_PI; //  기준 위치
        //                psi = Initial_Psi / 180.0 * M_PI;

        //                RotationR(theta, psi, ref R);

        //                MatrixCross(ref R, ref Xi, ref Nr, 3);//Nr = R * Xi;

        //                // reflection beam direction vector XoXi
        //                // Xo = 2 * (Xi'*Nr)*Nr - Xi; // ( Xo + Xi ) / 2 = (Xi' * Nr)*Nr
        //                double tmpXiNr = MatrixInnerProduct(ref Xi, ref Nr, 3);
        //                for (int w = 0; w < 3; w++)
        //                    Xo[w] = 2 * tmpXiNr * Nr[w] - Xi[w];

        //                ///////////////////////////////////////////////////////////////////
        //                ///////////////////////////////////////////////////////////////////
        //                //  Rotate N0 by R with Offset Rc
        //                // Surface Equation : N * ( P - P0 ) = 0 ; P0 = (0,0,0)
        //                theta = theta - 135.0 / 180 * M_PI; //  여기 135 는 절대 불변 회전축 Offset 에해 회전하면서 반사점의 이동을 계산

        //                RotationR(theta, psi, ref Rr);

        //                //N0 = Rr * (N0org - Rc)' + Rc';
        //                for (int w = 0; w < 3; w++)
        //                    N0res[w] = N0org[w] - Rc[w];

        //                MatrixCross(ref Rr, ref N0res, ref N0, 3);
        //                for (int w = 0; w < 3; w++)
        //                    N0[w] = N0[w] + Rc[w];

        //                ///////////////////////////////////////////////////////////////////
        //                ///////////////////////////////////////////////////////////////////

        //                // Cross Point
        //                //t1 = ((N0 - L1) * Nr) / (Xo' * Nr );
        //                for (int w = 0; w < 3; w++)
        //                    N0_L1[w] = N0[w] - L1[w];

        //                double tmpN0L1Nr = MatrixInnerProduct(ref N0_L1, ref Nr, 3);
        //                double tmpXoNr = MatrixInnerProduct(ref Xo, ref Nr, 3);
        //                double t1 = tmpN0L1Nr / tmpXoNr;


        //                for (int w = 0; w < 3; w++)
        //                    Pr1[w] = t1 * Xo[w] + L1[w];


        //                for (int w = 0; w < 3; w++)
        //                    N0_L2[w] = N0[w] - L2[w];


        //                double tmpN0L2Nr = MatrixInnerProduct(ref N0_L2, ref Nr, 3);
        //                double t2 = tmpN0L2Nr / tmpXoNr;

        //                for (int w = 0; w < 3; w++)
        //                    Pr2[w] = t2 * Xo[w] + L2[w];

        //                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                ////  여기서부터는 상기좌표로부터 다시 굴절율을 고려한 프리즘을 통과하여 마크평면과 교차하는 점의 좌표를 계산하는 과정

        //                ////  `굴절율이 적용됬을 때 L1 이 센서면으로 투영되는 점  좌표 계산
        //                imgDPtoMarkDP(rIndex, Pr1, L1, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L1, ref comp_imgP_L1);
        //                //  `굴절율이 적용됬을 때 Pr2 를 투영시키는 마크평면상의 점 좌표 계산
        //                imgDPtoMarkDP(rIndex, Pr2, L2, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L2, ref comp_imgP_L2);

        //                for (int w = 0; w < 3; w++)
        //                {
        //                    Pr1[w] = comp_imgP_L1[w];
        //                    Pr2[w] = comp_imgP_L2[w];
        //                }

        //                // 여기까지가 프리즘 굴절율을 1.717 로 한 상태에서 카메라에서 얻어지는 좌표(um)
        //                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                //  90.909 는 1pixel = 11um 에 따라서 mm 를 pixel  로 환산해준다.
        //                //  1pixel = 11um 일 때 1mm = 90.909091 pixel 이 된다.
        //                //  1pixel = 5.86um 일 때 1mm = 170.648464 pixel 이 된다.

        //                //  기준위치에서의 좌표값들
        //                //X1[0] = 90.909 * Pr1[1];
        //                //X1[1] = -90.909 * Pr1[2];  // Y -> imgX , Z -> -imgY
        //                //X2[0] = 90.909 * Pr2[1];
        //                //X2[1] = -90.909 * Pr2[2];  // Y -> imgX , Z -> -imgY
        //                X1[0] = 170.648464 * Pr1[1];
        //                X1[1] = -170.648464 * Pr1[2];  // Y -> imgX , Z -> -imgY
        //                X2[0] = 170.648464 * Pr2[1];
        //                X2[1] = -170.648464 * Pr2[2];  // Y -> imgX , Z -> -imgY
        //                X1org[0] = X1[0]; // 40도 일 때의 근마크의 영상좌표
        //                X1org[1] = X1[1]; // 40도 일 때의 근마크의 영상좌표
        //                X2org[0] = X2[0]; // 40도 일 때의 원마크의 영상좌표
        //                X2org[1] = X2[1]; // 40도 일 때의 원마크의 영상좌표


        //                for (double i = -15; i <= 15; i += 3)    //  Initial_Theta ~ Initial_Theta + 1.5deg
        //                {
        //                    for (double j = -30; j <= 30; j += 6)
        //                    {
        //                        //for (double j = -25; j <= 25; j += 10)
        //                        //{
        //                        //    for (double i = -25; i <= 25; i += 10)
        //                        //    {
        //                        ii = i;
        //                        //ii=0;
        //                        theta = (Initial_Theta + ii / 10) / 180.0 * M_PI;
        //                        psi = (Initial_Psi + j / 10) / 180.0 * M_PI;

        //                        Xtheta[k] = ii / 10;
        //                        Xpsi[k] = j / 10;
        //                        Xheight[k] = h;

        //                        RotationR(theta, psi, ref R);

        //                        // Source beam direction Vector
        //                        //Xi = [1 0 0]';

        //                        // Reflection Surface direction vector Nr
        //                        // Surface Equation : N * ( P - P0 ) = 0 ; P0 = (0,0,0)
        //                        //Nr = R * Xi;
        //                        MatrixCross(ref R, ref Xi, ref Nr, 3);

        //                        // reflection beam direction vector XoXi
        //                        // Xo = 2 * (Xi'*Nr)*Nr - Xi; // ( Xo + Xi ) / 2 = (Xi' * Nr)*Nr
        //                        tmpXiNr = MatrixInnerProduct(ref Xi, ref Nr, 3);
        //                        for (int w = 0; w < 3; w++)
        //                            Xo[w] = 2 * tmpXiNr * Nr[w] - Xi[w];

        //                        ///////////////////////////////////////////////////////////////////
        //                        ///////////////////////////////////////////////////////////////////
        //                        //  Rotate N0 by R with Offset Rc
        //                        theta = theta - 135.0 / 180 * M_PI; //  여기 135 는 절대 불변 회전축 Offset 에해 회전하면서 반사점의 이동을 계산

        //                        RotationR(theta, psi, ref Rr);

        //                        //N0 = Rr * (N0org - Rc)' + Rc';
        //                        for (int w = 0; w < 3; w++)
        //                            N0res[w] = N0org[w] - Rc[w];

        //                        MatrixCross(ref Rr, ref N0res, ref N0, 3);
        //                        for (int w = 0; w < 3; w++)
        //                            N0[w] = N0[w] + Rc[w];

        //                        ///////////////////////////////////////////////////////////////////
        //                        ///////////////////////////////////////////////////////////////////


        //                        // Cross Point
        //                        //t1 = ((N0 - L1) * Nr) / (Xo' * Nr );
        //                        for (int w = 0; w < 3; w++)
        //                            N0_L1[w] = N0[w] - L1[w];
        //                        tmpN0L1Nr = MatrixInnerProduct(ref N0_L1, ref Nr, 3);
        //                        tmpXoNr = MatrixInnerProduct(ref Xo, ref Nr, 3);
        //                        t1 = tmpN0L1Nr / tmpXoNr;


        //                        //Pr1 = t1 * Xo' + L1;
        //                        for (int w = 0; w < 3; w++)
        //                            Pr1[w] = t1 * Xo[w] + L1[w];

        //                        //t2 = ((N0 - L2) * Nr) / (Xo' * Nr );
        //                        for (int w = 0; w < 3; w++)
        //                            N0_L2[w] = N0[w] - L2[w];
        //                        tmpN0L2Nr = MatrixInnerProduct(ref N0_L2, ref Nr, 3);
        //                        t2 = tmpN0L2Nr / tmpXoNr;

        //                        //Pr2 = t2 * Xo' + L2;
        //                        for (int w = 0; w < 3; w++)
        //                            Pr2[w] = t2 * Xo[w] + L2[w];

        //                        // 여기까지가 프리즘 굴절율을 1.0 으로 한 상태에서 카메라에서 얻어지는 좌표(mm)
        //                        //  Pr2 : L2 로부터 굴절율 1.0 프리즘을 통과하여 sensor 면으로 투사된 점의 WCS 좌표
        //                        //  Pr1 : L1 로부터 굴절율 1.0 프리즘을 통과하여 sensor 면으로 투사된 점의 WCS 좌표

        //                        //  굴절율 0 일 때 Pr1 이 L1 에 매칭되는지 검증
        //                        //ImgPtoMarkP(1.0, Pr1, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L1, ref resMmirror1);
        //                        ////  굴절율 0 일 때 Pr2 이 L2 에 매칭되는지 검증
        //                        //ImgPtoMarkP(1.0, Pr2, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L2, ref resMmirror2);

        //                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                        ////  여기서부터는 상기좌표로부터 다시 굴절율을 고려한 프리즘을 통과하여 마크평면과 교차하는 점의 좌표를 계산하는 과정

        //                        ////  `굴절율이 적용됬을 때 L1 이 센서면으로 투영되는 점  좌표 계산
        //                        uErr1[k] = imgDPtoMarkDP(rIndex, Pr1, L1, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L1, ref comp_imgP_L1);
        //                        //  `굴절율이 적용됬을 때 Pr2 를 투영시키는 마크평면상의 점 좌표 계산
        //                        uErr2[k] = imgDPtoMarkDP(rIndex, Pr2, L2, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, L2, ref comp_imgP_L2);

        //                        for (int w = 0; w < 3; w++)
        //                        {
        //                            Pr1[w] = comp_imgP_L1[w];
        //                            Pr2[w] = comp_imgP_L2[w];
        //                        }

        //                        // 여기까지가 프리즘 굴절율을 1.717 로 한 상태에서 카메라에서 얻어지는 좌표(um)
        //                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                        //  90.909 는 1pixel = 11um 에 따라서 mm 를 pixel  로 환산해준다.
        //                        //  1pixel = 11um 일 때 1mm = 90.909091 pixel 이 된다.
        //                        //  1pixel = 5.86um 일 때 1mm = 170.648464 pixel 이 된다.

        //                        //X1[0] = 90.909091 * Pr1[1];    //  
        //                        //X1[1] = -90.909091 * Pr1[2];  // Y -> imgX , Z -> -imgY
        //                        //X2[0] = 90.909091 * Pr2[1];
        //                        //X2[1] = -90.909091 * Pr2[2];  // Y -> imgX , Z -> -imgY
        //                        X1[0] = 170.648464 * Pr1[1];    //  
        //                        X1[1] = -170.648464 * Pr1[2];  // Y -> imgX , Z -> -imgY
        //                        X2[0] = 170.648464 * Pr2[1];
        //                        X2[1] = -170.648464 * Pr2[2];  // Y -> imgX , Z -> -imgY

        //                        u_tmp[0, k] = X1[0];
        //                        u_tmp[1, k] = X1[1];
        //                        u_tmp[2, k] = X2[0];
        //                        u_tmp[3, k] = X2[1];

        //                        X1[0] = X1[0] - X1org[0];
        //                        X1[1] = X1[1] - X1org[1];
        //                        X2[0] = X2[0] - X2org[0];
        //                        X2[1] = X2[1] - X2org[1];

        //                        u[0, k] = X2[0] - X1[0];    //  X 방향 원마크이동량 - 근마크 이동량
        //                        u[1, k] = X2[1] - X1[1];    //  Y 방향 원마크이동량 - 근마크 이동량
        //                        u[2, k] = (X2org[0] - X1org[0]);// 40도 일 때의 원마크X좌표 빼기 근마크X좌표       
        //                        u[3, k] = (X2org[1] - X1org[1]);// 40도 일 때의 원마크Y좌표 빼기 근마크Y좌표 에서 표준거리를 뺀 값
        //                        k = k + 1;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    StreamWriter wr = new StreamWriter(srcFile);
        //    //wr.WriteLine("dX\tdy\tdX0\tdY0\ttheta\tpsi\tH\tNear X\tNear Y\tFar X\tFar Y");
        //    for (int line = 0; line < k; line++)
        //        wr.WriteLine(u[0, line].ToString("F9") + "\t" + u[1, line].ToString("F9") + "\t" + u[2, line].ToString("F9") + "\t" + u[3, line].ToString("F9") + "\t" + Xtheta[line].ToString("F9") + "\t" + Xpsi[line].ToString("F9") + "\t" + Xheight[line].ToString("F9") + "\t" + u_tmp[0, line].ToString("F9") + "\t" + u_tmp[1, line].ToString("F9") + "\t" + u_tmp[2, line].ToString("F9") + "\t" + u_tmp[3, line].ToString("F9"));// + "\t" + uErr1[line].ToString("E5") + "\t" + uErr2[line].ToString("E5"));

        //    wr.Close();

        //    ReadDataFile(srcFile);
        //    File.Delete(srcFile);
        //}

        public void RotationR(double theta_rad, double psi_rad, ref double[,] Rr)
        {
            double ct = 0;
            double st = 0;
            double cp = 0;
            double sp = 0;
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];

            ct = Math.Cos(theta_rad);   //  radian
            st = Math.Sin(theta_rad);   //  radian
            cp = Math.Cos(psi_rad);
            sp = Math.Sin(psi_rad);

            Ry[0, 0] = ct; Ry[0, 2] = st;
            Ry[1, 1] = 1;
            Ry[2, 0] = -st; Ry[2, 2] = ct;

            //Rz = [cp - sp 0; sp cp 0; 0 0 1];
            Rz[0, 0] = cp; Rz[0, 1] = -sp;
            Rz[1, 0] = sp; Rz[1, 1] = cp;
            Rz[2, 2] = 1;
            //R = Rz * Ry;
            MatrixCross(ref Rz, ref Ry, ref Rr, 3);
        }

        public void RotateArrayAlongYaxis(double theta_rad, Vector3D[] src, ref Vector3D[] dest)
        {
            double ct = 0;
            double st = 0;
            double cp = 0;
            double sp = 0;
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];
            double[,] Rr = new double[3, 3];

            ct = Math.Cos(theta_rad);   //  radian
            st = Math.Sin(theta_rad);   //  radian
            cp = Math.Cos(0);
            sp = Math.Sin(0);

            Ry[0, 0] = ct; Ry[0, 2] = st;
            Ry[1, 1] = 1;
            Ry[2, 0] = -st; Ry[2, 2] = ct;

            //Rz = [cp - sp 0; sp cp 0; 0 0 1];
            Rz[0, 0] = cp; Rz[0, 1] = -sp;
            Rz[1, 0] = sp; Rz[1, 1] = cp;
            Rz[2, 2] = 1;
            //R = Rz * Ry;
            MatrixCross(ref Rz, ref Ry, ref Rr, 3);
            int len = src.Length;
            double[] lp = new double[3];
            double[] lres = new double[3];
            for ( int i=0; i< len; i++)
            {
                lp[0] = src[i].X;
                lp[1] = src[i].Y;
                lp[2] = src[i].Z;
                MatrixCross(ref Rr, ref lp, ref lres, 3);
                dest[i].X = lres[0];
                dest[i].Y = lres[1];
                dest[i].Z = lres[2];
            }
        }
        public void RotateArrayAlongYaxis(double theta_rad, Vector3D src, ref Vector3D dest)
        {
            double ct = 0;
            double st = 0;
            double cp = 0;
            double sp = 0;
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];
            double[,] Rr = new double[3, 3];

            ct = Math.Cos(theta_rad);   //  radian
            st = Math.Sin(theta_rad);   //  radian
            cp = Math.Cos(0);
            sp = Math.Sin(0);

            Ry[0, 0] = ct; Ry[0, 2] = st;
            Ry[1, 1] = 1;
            Ry[2, 0] = -st; Ry[2, 2] = ct;

            //Rz = [cp - sp 0; sp cp 0; 0 0 1];
            Rz[0, 0] = cp; Rz[0, 1] = -sp;
            Rz[1, 0] = sp; Rz[1, 1] = cp;
            Rz[2, 2] = 1;
            //R = Rz * Ry;
            MatrixCross(ref Rz, ref Ry, ref Rr, 3);
            double[] lp = new double[3];
            double[] lres = new double[3];

            lp[0] = src.X;
            lp[1] = src.Y;
            lp[2] = src.Z;
            MatrixCross(ref Rr, ref lp, ref lres, 3);
            dest.X = lres[0];
            dest.Y = lres[1];
            dest.Z = lres[2];
        }

        public void RotationRAB(double theta_rad, double psi_rad, ref double[,] Rr)
        {
            double ct = 0;
            double st = 0;
            double cp = 0;
            double sp = 0;
            double[,] Rx = new double[3, 3];
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];

            ct = Math.Cos(theta_rad);   //  radian
            st = Math.Sin(theta_rad);   //  radian
            cp = Math.Cos(psi_rad);
            sp = Math.Sin(psi_rad);

            // Ry Y 축회전
            Ry[0, 0] = ct; Ry[0, 2] = st;
            Ry[1, 1] = 1;
            Ry[2, 0] = -st; Ry[2, 2] = ct;

            //Rx X 축 회전
            Rx[0, 0] = 1;
            Rx[1, 1] = cp; Rx[1, 2] = -sp;
            Rx[2, 1] = sp; Rx[2, 2] = cp;
            //R = Ry * Rx;

            ////Rz = [cp - sp 0; sp cp 0; 0 0 1]; -> Z 축 회전
            //Rz[0, 0] = cp; Rz[0, 1] = -sp;
            //Rz[1, 0] = sp; Rz[1, 1] = cp;
            //Rz[2, 2] = 1;
            ////R = Rz * Ry;
            MatrixCross(ref Ry, ref Rx, ref Rr, 3);
        }
        public void RotationXYZ(double phi_rad, double theta_rad, double psi_rad, ref double[,] Rr)
        {
            //  R = Rz Ry Rx
            double cz = 0;
            double sz = 0;
            double cy = 0;
            double sy = 0;
            double cx = 0;
            double sx = 0;
            double[,] Rx = new double[3, 3];
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];

            cz = Math.Cos(psi_rad);   //  radian
            sz = Math.Sin(psi_rad);   //  radian

            cy = Math.Cos(theta_rad);   //  radian
            sy = Math.Sin(theta_rad);   //  radian

            cx = Math.Cos(phi_rad);
            sx = Math.Sin(phi_rad);

            if (phi_rad == 0 && psi_rad==0)
            {
                // Y 축회전
                Rr[0, 0] = cy; Rr[0, 2] = sy;
                Rr[1, 1] = 1;
                Rr[2, 0] = -sy; Rr[2, 2] = cy;
            }
            else if (theta_rad == 0 && psi_rad == 0)
            {
                // X 축 회전
                Rr[0, 0] = 1;
                Rr[1, 1] = cx; Rr[1, 2] = -sx;
                Rr[2, 1] = sx; Rr[2, 2] = cx;
            }
            else if (phi_rad == 0 && theta_rad == 0)
            {
                // Z 축 회전
                Rr[0, 0] = cz; Rr[0, 1] = -sz;
                Rr[1, 0] = sz; Rr[1, 1] = cz;
                Rr[2, 2] = 1;
            }


            //R = Ry * Rx;

            ////Rz = [cp - sp 0; sp cp 0; 0 0 1]; -> Z 축 회전
            //R = Rz * Ry;
            //MatrixCross(ref Ry, ref Rz, ref Rr, 3);
            //MatrixCross(ref Rx, ref Rr, ref Rr, 3);
        }
        public void RotationEuler(double phi_rad, double theta_rad, double psi_rad, ref double[,] Rr)
        {
            //  R = Rz Ry Rx
            double cz = 0;
            double sz = 0;
            double cy = 0;
            double sy = 0;
            double cx = 0;
            double sx = 0;
            double[,] Rx = new double[3, 3];
            double[,] Ry = new double[3, 3];
            double[,] Rz = new double[3, 3];
            double[,] RyRx = new double[3, 3];

            cz = Math.Cos(psi_rad);   //  radian
            sz = Math.Sin(psi_rad);   //  radian

            cy = Math.Cos(theta_rad);   //  radian
            sy = Math.Sin(theta_rad);   //  radian

            cx = Math.Cos(phi_rad);
            sx = Math.Sin(phi_rad);

            // Y 축회전
            Ry[0, 0] = cy; Ry[0, 2] = sy;
            Ry[1, 1] = 1;
            Ry[2, 0] = -sy; Ry[2, 2] = cy;
            // X 축 회전
            Rx[0, 0] = 1;
            Rx[1, 1] = cx; Rx[1, 2] = -sx;
            Rx[2, 1] = sx; Rx[2, 2] = cx;
            // Z 축 회전
            Rz[0, 0] = cz; Rz[0, 1] = -sz;
            Rz[1, 0] = sz; Rz[1, 1] = cz;
            Rz[2, 2] = 1;

            //R = Ry * Rx;
            MatrixCross(ref Ry, ref Rx, ref RyRx, 3);
            //R = Rz * Ry * Rx;
            MatrixCross(ref Rz, ref RyRx, ref Rr, 3);
        }
        public void RefractionVector(double[] surfN, double[] beamV, double rIndex, ref double[] refractedV)
        {
            //  cos θi = N * Xi / ( | N | * | Xi | )
            //  NC  = cos θi | Xi | * N / | N |
            //  NC + NUI = Xi
            //  NUI = Xi – NC
            //  NUI = Xi - cos θi | Xi | *N / | N |
            //  NUO = -NUI * sin θo / sin θi = -NUI / n
            //  XO  = -NC + NUO

            double l_NV = 0;
            double cosT = 0;
            double l_Nsum = 0;
            double l_Vsum = 0;
            for (int i = 0; i < 3; i++)
            {
                l_NV += surfN[i] * beamV[i];
                l_Nsum += surfN[i] * surfN[i];
                l_Vsum += beamV[i] * beamV[i];
            }
            double absN = Math.Sqrt(l_Nsum);
            double absV = Math.Sqrt(l_Vsum);
            cosT = l_NV / (absN * absV);

            double[] Nc = new double[3];
            double[] Nui = new double[3];
            for (int i = 0; i < 3; i++)
            {
                Nc[i] = cosT * absV / absN * surfN[i];
                Nui[i] = beamV[i] - Nc[i];
                refractedV[i] = -Nc[i] - Nui[i] / rIndex;
            }
        }
        public void CPofBeamToSurfaceN(double[] surfN, double[] surfP, double[] beamV, double[] beamP, ref double[] resP)
        {
            //  Nin (P – PR) = 0
            //  PR = R[D / 2 0 0]’
            //  N0 = [1 0 0]
            //  P = PL1 + t N0
            //  Nin (PL1 + t N0 - PR ) = 0
            //  t Nin *N0 = Nin * (PR – PL1 )
            //  t = (Nin * (PR – PL1 ) ) / (Nin * N0)
            double l_surfN_beamV = 0;
            double t = 0;

            for (int i = 0; i < 3; i++)
                l_surfN_beamV += surfN[i] * beamV[i];

            for (int i = 0; i < 3; i++)
                t += surfN[i] * (surfP[i] - beamP[i]) / l_surfN_beamV;

            for (int i = 0; i < 3; i++)
                resP[i] = beamP[i] + t * beamV[i];

        }
        public void ReflectionVector(double[] surfN, double[] inV, ref double[] outV)
        {
            //  Nout = 2(Nin * Nr) * Nr – Nin
            double innerNinNr = 0;

            for (int i = 0; i < 3; i++)
                innerNinNr += inV[i] * surfN[i];

            for (int i = 0; i < 3; i++)
                outV[i] = innerNinNr * surfN[i] - inV[i];

        }
        public void ImgPtoMarkP(double rIndex, double[] imgP, double[] beamV, double[,] Rr, double[] surfNi0, double[] surfNr0, double[] surfNo0, ref double[] surfPi0, ref double[] surfPo0, double[] markP0, ref double[] resMarkP)
        {
            double[] Ndfr = new double[3];
            double[] Ndfr2 = new double[3];
            double[] Nout = new double[3];
            double[] surfPi = new double[3];
            double[] surfPo = new double[3];
            double[] surfNi = new double[3];
            double[] surfNr = new double[3];
            double[] surfNo = new double[3];
            double[] surfMark = new double[3] { 0, 0, 1 }; //    constant, Normal to Mark Plane
            double[] cpi = new double[3];   //  cross point on prism inlet
            double[] cpr = new double[3];   //  cross point on prism reflection surface
            double[] cpo = new double[3];   //  corss point on prism outlet
            double[] pZero = new double[3] { 0, 0, 0 }; //  constance, Center of rotation without Offset.


            //when center of rotation is [ 0 0 0 ]
            MatrixCross(ref Rr, ref surfNi0, ref surfNi, 3);    //  surfNi0 : prism inlet surface normal vector
            MatrixCross(ref Rr, ref surfNr0, ref surfNr, 3);    //  surfNi0 : prism reflection surface normal vector
            MatrixCross(ref Rr, ref surfNo0, ref surfNo, 3);    //  surfNi0 : prism outlet surface normal vector

            MatrixCross(ref Rr, ref surfPi0, ref surfPi, 3);    //  surfNi0 : A point on prism inlet surface
            MatrixCross(ref Rr, ref surfPo0, ref surfPo, 3);    //  surfNi0 : A point on prism outlet surface 

            /////////////////////////////////////////////////////////////////////
            //  단계별로 Octave 와 비교 검증 필요
            //  프리즘 인렛면과 빔간 교점
            //m_strMsg += "Sensor Point = [\t" + imgP[0].ToString("F4") + "\t" + imgP[1].ToString("F4") + "\t" + imgP[2].ToString("F4") + "\t] \r\n";
            CPofBeamToSurfaceN(surfNi, surfPi, beamV, imgP, ref cpi);
            //m_strMsg += "Sensor->Prism Crosspoint = [\t" + cpi[0].ToString("F4") + "\t" + cpi[1].ToString("F4") + "\t" + cpi[2].ToString("F4") + "\t] \r\n";

            RefractionVector(surfNi, beamV, rIndex, ref Ndfr);
            //m_strMsg += "Refracted Vector in Prism = [\t" + Ndfr[0].ToString("F4") + "\t" + Ndfr[1].ToString("F4") + "\t" + Ndfr[2].ToString("F4") + "\t] \r\n";
            //  프리즘 반사점
            CPofBeamToSurfaceN(surfNr, pZero, Ndfr, cpi, ref cpr);
            //m_strMsg += "Prism Reflection Point = [\t" + cpr[0].ToString("F4") + "\t" + cpr[1].ToString("F4") + "\t" + cpr[2].ToString("F4") + "\t] \r\n";
            //  프리즘 반사 후 빔 벡터
            ReflectionVector(surfNr, Ndfr, ref Ndfr2);
            //m_strMsg += "Prism Reflection Vector = [\t" + Ndfr2[0].ToString("F4") + "\t" + Ndfr2[1].ToString("F4") + "\t" + Ndfr2[2].ToString("F4") + "\t] \r\n";
            //  프리즘 반사 후 빔과 프리즘 아웃렛간 교점
            CPofBeamToSurfaceN(surfNo, surfPo, Ndfr2, cpr, ref cpo);
            //m_strMsg += "Prism to Mark Crosspoint = [\t" + cpo[0].ToString("F4") + "\t" + cpo[1].ToString("F4") + "\t" + cpo[2].ToString("F4") + "\t] \r\n";
            //  프리즘 탈출 후 빔 벡터
            RefractionVector(surfNo, Ndfr2, 1 / rIndex, ref Nout);
            //m_strMsg += "To Mark Vector = [\t" + Nout[0].ToString("F4") + "\t" + Nout[1].ToString("F4") + "\t" + Nout[2].ToString("F4") + "\t] \r\n";
            //  프리즘 탈출빔과 마크면간 교점
            CPofBeamToSurfaceN(surfMark, markP0, Nout, cpo, ref resMarkP);
            //m_strMsg += "Mark Plane Cross Point = [\t" + resMarkP[0].ToString("F4") + "\t" + resMarkP[1].ToString("F4") + "\t" + resMarkP[2].ToString("F4") + "\t] \r\n";

        }
        public double imgDPtoMarkDP(double rIndex, double[] imgP, double[] resMmirror, double[] beamV, double[,] Rr, double[] surfNi0, double[] surfNr0, double[] surfNo0, ref double[] surfPi0, ref double[] surfPo0, double[] markP0, ref double[] comp_imgP)
        {
            double[] imgPdy = new double[3];
            double[] imgPdz = new double[3];

            for (int i = 0; i < 3; i++)
            {
                imgPdy[i] = imgP[i];
                imgPdz[i] = imgP[i];
            }
            imgPdy[1] += 0.01; //  10um to Y dir
            imgPdz[2] += 0.01; //  10um to Z dir

            double[] resMorg = new double[3];
            double[] resMdx = new double[3];
            double[] resMdy = new double[3];

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  검증의 검증용 코드
            //  굴절율 1.0 가정 imgP 를 형성시키는 Mark 평면상의 점을 구한다.
            //  이 점은 굴절율 1.0 가정 시 imgP 에 투영된다.
            //ImgPtoMarkP( 1.0, imgP, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, markP0, ref resMmirror); //  imgP 를 
            //m_strMsg += "\r\n\r\n";
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            //  Sensor 면의 imgP 점이 굴절율을 가진 프리즘에 의해 markP0 면으로 전사된 점 resMorg 을 구한다.
            ImgPtoMarkP(rIndex, imgP, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, markP0, ref resMorg); //  
            //m_strMsg += "\r\n";
            //  Sensor 면의 imgP 점 기준 10um to Y dir 로 이동한 점이 굴절율을 가진 프리즘에 의해 markP0 면으로 전사된 점 resMdx 을 구한다.
            ImgPtoMarkP(rIndex, imgPdy, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, markP0, ref resMdx);
            //m_strMsg += "\r\n";
            //  Sensor 면의 imgP 점 기준 10um to Z dir 로 이동한 점이 굴절율을 가진 프리즘에 의해 markP0 면으로 전사된 점 resMdy 을 구한다.
            ImgPtoMarkP(rIndex, imgPdz, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, markP0, ref resMdy);
            //m_strMsg += "\r\n";

            double[] dMark = new double[2];
            double[] dXMark = new double[2];
            double[] dYMark = new double[2];
            double[,] toInvP = new double[2, 2];
            double[,] resInvP = new double[2, 2];


            dMark[0] = resMorg[0] - markP0[0];
            dMark[1] = resMorg[1] - markP0[1];

            dXMark[0] = resMdx[0] - resMorg[0]; //  Shift by imgP_y
            dXMark[1] = resMdx[1] - resMorg[1];

            dYMark[0] = resMdy[0] - resMorg[0]; //  Shift by imgP_z
            dYMark[1] = resMdy[1] - resMorg[1];

            toInvP[0, 0] = dXMark[0];
            toInvP[0, 1] = dYMark[0];
            toInvP[1, 0] = dXMark[1];
            toInvP[1, 1] = dYMark[1];

            resInvP = MatrixInverse(toInvP, 2);

            double kx = resInvP[0, 0] * (resMorg[0] - resMmirror[0]) + resInvP[0, 1] * (resMorg[1] - resMmirror[1]);
            double ky = resInvP[1, 0] * (resMorg[0] - resMmirror[0]) + resInvP[1, 1] * (resMorg[1] - resMmirror[1]);
            comp_imgP[0] = imgP[0];
            comp_imgP[1] = imgP[1] - 0.01 * kx;
            comp_imgP[2] = imgP[2] - 0.01 * ky;
            ImgPtoMarkP(rIndex, comp_imgP, beamV, Rr, surfNi0, surfNr0, surfNo0, ref surfPi0, ref surfPo0, markP0, ref resMorg);
            //m_strMsg += "\r\n";

            double errDist = Math.Sqrt((resMorg[0] - resMmirror[0]) * (resMorg[0] - resMmirror[0]) + (resMorg[1] - resMmirror[1]) * (resMorg[1] - resMmirror[1]));

            //m_strMsg += "ErrDist =\t" + errDist.ToString("E5") + "\r\n";
            return errDist;
        }

        /// <summary>
        /// Shaft Tester Use
        /// </summary>
        /// <param name="lpt"></param>
        /// <returns></returns>

        //public bool mcDirVector2DfromData(Point2D[] lpt, ref double[] lvector)
        //{
        //    double[,] mX = new double[lpt.Length, 2];
        //    double[,] mXT = new double[2, lpt.Length];
        //    double[,] mX22 = new double[2, 2];
        //    double[] mC = new double[lpt.Length];
        //    double[] mC21 = new double[2];
        //    double[] mRes = new double[2];

        //    //  ax + by = 1, mRes = { a, b }
        //    //  lvector = [ -b, a ] ; Direction Vector
        //    for (int i = 0; i < lpt.Length; i++)
        //    {
        //        mX[i, 0] = lpt[i].X;
        //        mX[i, 1] = lpt[i].Y;
        //        mC[i] = 1;
        //    }
        //    MatrixTranspose(mX, ref mXT, 2, lpt.Length);
        //    MatrixCross(ref mXT, ref mX, ref mX22, 2, lpt.Length);
        //    MatrixCross(ref mXT, ref mC, ref mC21, 2, lpt.Length);

        //    InverseU(ref mX22, 2);
        //    MatrixCross(ref mX22, ref mC21, ref mRes, 2);
        //    lvector[0] = -mRes[1];
        //    lvector[1] = -mRes[0];
        //    return true;
        //}
        //public bool mCalcPerpendicularity(double[] LeftV, double[] RightV, double refDistUm, ref double ldist, ref double langleDeg)
        //{
        //    //  ldist 는 거리로 환산한 직각도
        //    //  langleDeg 는 degree 로 환산한 직각도
        //    //  cos(theta) = A * B / (|A| * |B|)

        //    double innerProduct = LeftV[0] * RightV[0] + LeftV[1] * RightV[1] + LeftV[2] * RightV[2];
        //    double absLR = Math.Sqrt(LeftV[0] * LeftV[0] + LeftV[1] * LeftV[1] + LeftV[2] * LeftV[2]);
        //    absLR = absLR * Math.Sqrt(RightV[0] * RightV[0] + RightV[1] * RightV[1] + RightV[2] * RightV[2]);

        //    double theta = Math.Acos(innerProduct / absLR);
        //    langleDeg = theta * 180 / Math.PI;  //  degree
        //    ldist = Math.Abs(theta - Math.PI/2) * refDistUm;
        //    return true;
        //}
        //public double mCalcAnlgeBetweenVectors(double[] LeftV, double[] RightV)
        //{
        //    double innerProduct = LeftV[0] * RightV[0] + LeftV[1] * RightV[1] + LeftV[2] * RightV[2];
        //    double absLR = Math.Sqrt(LeftV[0] * LeftV[0] + LeftV[1] * LeftV[1] + LeftV[2] * LeftV[2]);
        //    absLR = absLR * Math.Sqrt(RightV[0] * RightV[0] + RightV[1] * RightV[1] + RightV[2] * RightV[2]);

        //    double theta = Math.Acos(innerProduct / absLR);
        //    double langleDeg = theta * 180 / Math.PI;  //  degree
        //    return langleDeg;
        //}
        //public bool mCalcStraightness(Vector3D[] shaftPts, ref double lDiameter, ref double[] ldirVtr, ref double[] ldist)
        //{
        //    //"Top View 만 활용
        //    //각 측정점의 X,Y,Z 좌표로부터  LMS 직선의 방정식을 구하고, LMS 직선의 방향벡터를 가지는 원기둥의 최소직경을 구해야 한다."

        //    //먼저 LMS 방향벡터를 구한다.
        //    mcDirVectorfrom3DPoints(shaftPts, ref ldirVtr);

        //    //string lstr1 = "3D to 2D for LeastCircle\r\n";
        //    //for (int i = 0; i < shaftPts.Length; i++)
        //    //    lstr1 += shaftPts[i].X.ToString("F3") + "\t" + shaftPts[i].Y.ToString("F3") + "\t" + shaftPts[i].Z.ToString("F3") + "\r\n";

        //    //lstr1 += "Dir Vector:\r\n" + ldirVtr[0].ToString("F5") + "\t" + ldirVtr[1].ToString("F5") + "\t" + ldirVtr[2].ToString("F5") + "\r\n";
        //    //MessageBox.Show(lstr1);


        //    //  LMS 방향벡터를 Z 축으로 하는 좌표계를 구한다.
        //    double[,] lBasis = new double[3, 3];
        //    mcBasisFromVector(ldirVtr, ref lBasis);
        //    //  WCS 좌표계에서 LMS 방향벡터를 Z 축으로 하는 좌표계로 회전시키는 회전변환행렬을 구한다.
        //    double[,] Rmatrix = new double[3, 3];
        //    mcRotationMatrix(lBasis, ref Rmatrix);
        //    //  측정점을 WCS 좌표계에서 LMS 방향벡터를 Z 축으로 하는 좌표계로 회전변환한 뒤 X Y 좌표만으로 뽑아낸다.
        //    Point2D[] xyPts = new Point2D[shaftPts.Length];
        //    mcTrasformXZYtoXY(Rmatrix, shaftPts, ref xyPts);

        //    //string lstr = "3D to 2D for LeastCircle\r\n";
        //    //for (int i = 0; i < xyPts.Length; i++)
        //    //    lstr += shaftPts[i].X.ToString("F3") + "\t" + shaftPts[i].Y.ToString("F3") + "\t" + shaftPts[i].Z.ToString("F3") + "\t" + xyPts[i].X.ToString("F3") + "\t" + xyPts[i].Y.ToString("F3") + "\r\n";
        //    //MessageBox.Show(lstr);
        //    //  2차원 좌표점들을 내포하는 최소원의 직경을 구한다.
        //    mcLeastCircle(xyPts, ref lDiameter, ref ldist);
        //    //MessageBox.Show("lDiameter = " + lDiameter.ToString("F1"));

        //    return true;
        //}

        //public bool mcDirVectorfrom3DPoints(Vector3D[] dataPts, ref double[] DirVector)
        //{
        //    //  측정점의 X, Y, Z 좌표로부터 LMS 직선의 방향벡터를 구한다.
        //    //  직선의 방향벡터를 (1,a,b) 로 가정하면
        //    double sum_xy = 0;
        //    double sum_xz = 0;
        //    double sum_xx = 0;
        //    double sum_x = 0;
        //    double sum_y = 0;
        //    double sum_z = 0;
        //    //double avgx = 0;
        //    //double avgy = 0;
        //    //double avgz = 0;

        //    //for (int i = 0; i < dataPts.Length; i++)
        //    //{
        //    //    avgx += dataPts[i].X;
        //    //    avgy += dataPts[i].Y;
        //    //    avgz += dataPts[i].Z;
        //    //}
        //    //avgx = avgx / dataPts.Length;
        //    //avgy = avgy / dataPts.Length;
        //    //avgz = avgz / dataPts.Length;
        //    double[,] XXT = new double[2, 2];
        //    for (int i = 0; i < dataPts.Length; i++)
        //    {
        //        sum_xy += dataPts[i].X * dataPts[i].Y;
        //        sum_xz += dataPts[i].X * dataPts[i].Z;
        //        sum_xx += dataPts[i].X * dataPts[i].X;
        //        sum_x += dataPts[i].X;
        //        sum_y += dataPts[i].Y;
        //        sum_z += dataPts[i].Z;
        //    }
        //    XXT[0, 0] = sum_xx;
        //    XXT[0, 1] = sum_x;
        //    XXT[1, 0] = sum_x;
        //    XXT[1, 1] = dataPts.Length;
        //    InverseU(ref XXT, 3);
        //    double[] Y = new double[2];
        //    double[] res = new double[2];
        //    Y[0] = sum_xy;
        //    Y[1] = sum_y;
        //    MatrixCross(ref XXT, ref Y, ref res, 2);
        //    DirVector[1] = res[0];

        //    Y[0] = sum_xz;
        //    Y[1] = sum_z;
        //    MatrixCross(ref XXT, ref Y, ref res, 2);
        //    DirVector[2] = res[0];
        //    DirVector[0] = 1;
        //    return true;
        //}

        //public bool mcBasisFromVector(double[] lvector, ref double[,] lbasis)
        //{
        //    //하나의 벡터를 받아서 해당 벡터를 Z 축이라고 가정하고, 오른손법칙에 따른 X 축, Y 축 의 벡터를 구해서[X Y Z]  로 정렬된 메트릭스를 반환한다.

        //    // lvector 를 unit vector 로 만든다.
        //    double absV = Math.Sqrt(lvector[0] * lvector[0] + lvector[1] * lvector[1] + lvector[2] * lvector[2]);
        //    lbasis[0, 0] = lvector[0] / absV;
        //    lbasis[1, 0] = lvector[1] / absV;
        //    lbasis[2, 0] = lvector[2] / absV;

        //    lbasis[0, 1] = -lbasis[1, 0];
        //    lbasis[1, 1] = lbasis[0, 0];
        //    lbasis[2, 1] = 0;

        //    absV = Math.Sqrt(lbasis[0, 1] * lbasis[0, 1] + lbasis[1, 1] * lbasis[1, 1]);
        //    lbasis[0, 1] = lbasis[0, 1] / absV;
        //    lbasis[1, 1] = lbasis[1, 1] / absV;
        //    lbasis[2, 1] = lbasis[2, 1] / absV;

        //    lbasis[0, 2] = -lvector[0] * lvector[2];
        //    lbasis[1, 2] = -lvector[1] * lvector[2];
        //    lbasis[2, 2] = lvector[0] * lvector[0] + lvector[1] * lvector[1];

        //    absV = Math.Sqrt(lbasis[0, 2] * lbasis[0, 2] + lbasis[1, 2] * lbasis[1, 2] + lbasis[2, 2] * lbasis[2, 2]);
        //    lbasis[0, 2] = lbasis[0, 2] / absV;
        //    lbasis[1, 2] = lbasis[1, 2] / absV;
        //    lbasis[2, 2] = lbasis[2, 2] / absV;
        //    return true;
        //}

        //public bool mcBasisFromPQ(double[] P, double[] Q, ref double[,] lbasis)
        //{
        //    //하나의 벡터를 받아서 해당 벡터를 Z 축이라고 가정하고, 오른손법칙에 따른 X 축, Y 축 의 벡터를 구해서[X Y Z]  로 정렬된 메트릭스를 반환한다.

        //    // l 를 unit  로 만든다.
        //    double absP = Math.Sqrt(P[0] * P[0] + P[1] * P[1] + P[2] * P[2]);
        //    lbasis[0, 0] = P[0] / absP;
        //    lbasis[1, 0] = P[1] / absP;
        //    lbasis[2, 0] = P[2] / absP;

        //    double absQ = Math.Sqrt(Q[0] * Q[0] + Q[1] * Q[1] + Q[2] * Q[2]);
        //    lbasis[0, 1] = Q[0] / absQ;
        //    lbasis[1, 1] = Q[1] / absQ;
        //    lbasis[2, 1] = Q[2] / absQ;

        //    lbasis[0, 2] = (P[1] * Q[2] - P[2] * Q[1])/(absP * absQ);
        //    lbasis[1, 2] = (P[2] * Q[0] - P[0] * Q[2])/(absP * absQ);
        //    lbasis[2, 2] = (P[0] * Q[1] - P[1] * Q[0])/(absP * absQ);
        //    return true;
        //}

        //public bool mcRotationMatrix(double[,] lbasis, ref double[,] Rmatrix)
        //{
        //    //각열을 하나의 벡터로 놓았을 때 절대값이 1 이고 각 벡터가 서로 수직인 3x3 matrix 를 Unit Matrix 로 변환시키는 회전행렬을 구한다.
        //    //  열간 직교성 확인해서 직교 아니면 return false
        //    double inner = 0;
        //    inner = lbasis[0, 0] * lbasis[0, 1] + lbasis[1, 0] * lbasis[1, 1] + lbasis[2, 0] * lbasis[2, 1];
        //    if (Math.Abs(inner) > 1.0e-15) return false;
        //    inner = lbasis[0, 1] * lbasis[0, 2] + lbasis[1, 1] * lbasis[1, 2] + lbasis[2, 1] * lbasis[2, 2];
        //    if (Math.Abs(inner) > 1.0e-15) return false;
        //    inner = lbasis[0, 2] * lbasis[0, 0] + lbasis[1, 2] * lbasis[1, 0] + lbasis[2, 2] * lbasis[2, 0];
        //    if (Math.Abs(inner) > 1.0e-15) return false;

        //    for (int i = 0; i < 3; i++)
        //        for (int j = 0; j < 3; j++)
        //            Rmatrix[i, j] = lbasis[i, j];

        //    //string lstr = "";
        //    //lstr = Rmatrix[0, 0].ToString("F5") + "\t" + Rmatrix[0, 1].ToString("F5") + "\t" + Rmatrix[0, 2].ToString("F5") + "\r\n";//             MessageBox.Show(
        //    //lstr += Rmatrix[1, 0].ToString("F5") + "\t" + Rmatrix[1, 1].ToString("F5") + "\t" + Rmatrix[1, 2].ToString("F5") + "\r\n";//             MessageBox.Show(
        //    //lstr += Rmatrix[2, 0].ToString("F5") + "\t" + Rmatrix[2, 1].ToString("F5") + "\t" + Rmatrix[2, 2].ToString("F5") + "\r\n";//             
        //    //MessageBox.Show("Rmatrix = \r\n" + lstr);
        //    InverseU(ref Rmatrix, 3);
        //    //lstr = Rmatrix[0, 0].ToString("F5") + "\t" +  Rmatrix[0, 1].ToString("F5") + "\t" + Rmatrix[0, 2].ToString("F5") + "\r\n";//             MessageBox.Show(
        //    //lstr += Rmatrix[1, 0].ToString("F5") + "\t" + Rmatrix[1, 1].ToString("F5") + "\t" + Rmatrix[1, 2].ToString("F5") + "\r\n";//             MessageBox.Show(
        //    //lstr += Rmatrix[2, 0].ToString("F5") + "\t" + Rmatrix[2, 1].ToString("F5") + "\t" + Rmatrix[2, 2].ToString("F5") + "\r\n";//             
        //    //MessageBox.Show("Rmatrix Inv = \r\n" + lstr);

        //    return true;
        //}
        //public bool mcTrasformXZYtoXY(double[,] Rmatrix, Vector3D[] dataPts, ref Point2D[] outPts)
        //{
        //    //3차원 좌표를 회전행렬로 회전시킨 뒤, (y, z) 좌표만 추출한다.
        //    double[] ldata = new double[3];
        //    double[] lres = new double[3];

        //    for (int count = 0; count < dataPts.Length; count++)
        //    {
        //        ldata[0] = dataPts[count].X;
        //        ldata[1] = dataPts[count].Y;
        //        ldata[2] = dataPts[count].Z;
        //        MatrixCross(ref Rmatrix, ref ldata, ref lres, 3);
        //        outPts[count].X = lres[1];
        //        outPts[count].Y = lres[2];
        //    }
        //    return true;
        //}
        public bool mcLeastCircle(Point2D[] inPts, ref double lDiameter, ref double[] lheight)
        {
            //"2차원 좌표들로부터 모든 점을 내포하는 최소원을 구한다.

            //1) 모든 측정점의 평균좌표 -> 초기 원의 중심(이하 Cavg ; = (dT, dS / cos(Q)) )
            double cx = 0;
            double cy = 0;
            double radius = 0;
            double radiusMax = 0;
            double rStep = 0;
            int indexA = -1;
            int indexB = -1;
            int indexC = -1;
            int outlier = 0;
            for (int i = 0; i < inPts.Length; i++)
            {
                cx += inPts[i].X;
                cy += inPts[i].Y;
            }
            cx = cx / inPts.Length;
            cy = cy / inPts.Length;

            //2) CAvg 에서 가장 먼 측정점 A까지의 거리 -> 초기 원의 반경
            //초기 중심부터 각 점까지의 거리를 구해서 최대거리를 반경으로 설정
            for (int i = 0; i < inPts.Length; i++)
            {
                radius = Math.Sqrt((inPts[i].X - cx) * (inPts[i].X - cx) + (inPts[i].Y - cy) * (inPts[i].Y - cy));
                if (radiusMax < radius)
                {
                    indexA = i;
                    radiusMax = radius;
                }
            }
            rStep = radiusMax / 10;
            radius = radiusMax;
            //3) Cavg 을 A 쪽으로 이동하면서 이동거리만큼 원의 반경을 감소시킴
            //중심에서 A 점까지의 벡터를 계산하고 중심을 어떤 Step 만큼 A 점쪽으로 이동, R 은 step 만큼 축소, 
            //점 2개 이상이 수정 R 값보다 큰 경우 Step 을 반으로 줄여서 반대로 이동 및 R 확대 하나의 점 이상이 수정 R 값보다 큰 경우 반대 알고리즘 반복
            //수정 R 값보다 작은 경우 Step 을 반으로 줄여서 정방향 이동 Step, 1개점만 밖으로 나간 경우 그점을 B,
            while (true)
            {
                cx = (inPts[indexA].X - cx) * (rStep / radius) + cx;
                cy = (inPts[indexA].Y - cy) * (rStep / radius) + cy;
                radius = radius - rStep;
                outlier = 0;
                for (int i = 0; i < inPts.Length; i++)
                {
                    if (i == indexA) continue;
                    double ldist = Math.Sqrt((inPts[i].X - cx) * (inPts[i].X - cx) + (inPts[i].Y - cy) * (inPts[i].Y - cy));
                    if (ldist > radius)
                    {
                        indexB = i;
                        radiusMax = radius;
                        outlier++;
                    }
                }
                if (outlier > 1)
                    rStep = -Math.Abs(rStep / 2);
                else if (outlier == 0)
                {
                    rStep = Math.Abs(rStep);
                    continue;
                }
                else
                    break;
            }
            //A-B 수직이등분선과 A-Cavg 간 교점(CAB) 계산 및 그때 R 값 계산
            double a1 = inPts[indexB].X - inPts[indexA].X;
            double b1 = inPts[indexB].Y - inPts[indexA].Y;
            double c1 = (inPts[indexB].X - inPts[indexA].X) * (inPts[indexB].X + inPts[indexA].X) / 2 + (inPts[indexB].Y - inPts[indexA].Y) * (inPts[indexB].Y + inPts[indexA].Y) / 2;

            double a2 = cy - inPts[indexA].Y;
            double b2 = -(cx - inPts[indexA].X);
            double c2 = inPts[indexA].X * (cy - inPts[indexA].Y) - inPts[indexA].Y * (cx - inPts[indexA].X);

            double[,] invA = new double[2, 2];
            double[] arrC = new double[2];
            double[] newC = new double[2];

            invA[0, 0] = a1;
            invA[0, 1] = b1;
            invA[1, 0] = a2;
            invA[1, 1] = b2;
            arrC[0] = c1;
            arrC[1] = c2;
            InverseU(ref invA, 2);
            MatrixCross(ref invA, ref arrC, ref newC, 2);

            cx = newC[0];
            cy = newC[1];
            radius = Math.Sqrt((cx - inPts[indexA].X) * (cx - inPts[indexA].X) + (cy - inPts[indexA].Y) * (cy - inPts[indexA].Y));
            radius = Math.Sqrt((cx - inPts[indexB].X) * (cx - inPts[indexB].X) + (cy - inPts[indexB].Y) * (cy - inPts[indexB].Y));

            //5) CAB 를 A - B 의 이등분선 방향으로 어떤 Step 만큼 이동시키면서 R 값도 CAB-A 간 거리로 축소
            //   점 2개 이상이 수정 R 값보다 큰 경우 Step 을 반으로 줄여서 반대로 이동 및 R 확대 하나의 점 이상이 수정 R 값보다 큰 경우 반대 알고리즘 반복
            //  수정 R 값보다 작은 경우 Step 을 반으로 줄여서 정방향 이동 Step, 1개점만 밖으로 나간 경우 그점을 D,
            double tx = (inPts[indexB].X + inPts[indexA].X) / 2;    //  tx,ty 로 접근하게 한다.
            double ty = (inPts[indexB].Y + inPts[indexA].Y) / 2;
            double stepX = (tx - cx) / 10;
            double stepY = (ty - cy) / 10;
            double signX = Math.Sign(stepX);
            double signY = Math.Sign(stepY);
            double prevR = radius;
            while (true)
            {
                cx = cx + stepX;
                cy = cy + stepY;
                radius = Math.Sqrt((cx - inPts[indexA].X) * (cx - inPts[indexA].X) + (cy - inPts[indexA].Y) * (cy - inPts[indexA].Y));
                if (prevR < radius && outlier == 0)
                    break;
                prevR = radius;
                outlier = 0;
                for (int i = 0; i < inPts.Length; i++)
                {
                    if (i == indexA || i == indexB) continue;
                    double ldist = Math.Sqrt((inPts[i].X - cx) * (inPts[i].X - cx) + (inPts[i].Y - cy) * (inPts[i].Y - cy));
                    if (ldist > radius)
                    {
                        indexC = i;
                        radiusMax = radius;
                        outlier++;
                    }
                }
                if (outlier > 1)
                {
                    stepX = -signX * Math.Abs(stepX / 2);
                    stepY = -signY * Math.Abs(stepY / 2);
                }
                else if (outlier == 0)
                {
                    stepX = signX * Math.Abs(stepX);
                    stepY = signY * Math.Abs(stepY);
                    continue;
                }
                else
                    break;
            }
            if (outlier == 1)
            {
                //6) A - B 이등분선과 B-D 이등분선의 교점 구하고 그때 반경 구한다.
                //   또는 A, B, C 를 지나는 원의 방정식을 바로 구한다."
                double[,] lcircle = new double[3, 3];
                double[] lc = new double[3];
                double[] lres = new double[3];
                lcircle[0, 0] = inPts[indexA].X;
                lcircle[0, 1] = inPts[indexA].Y;
                lcircle[0, 2] = 1;
                lcircle[1, 0] = inPts[indexB].X;
                lcircle[1, 1] = inPts[indexB].Y;
                lcircle[1, 2] = 1;
                lcircle[2, 0] = inPts[indexC].X;
                lcircle[2, 1] = inPts[indexC].Y;
                lcircle[2, 2] = 1;
                lc[0] = -(inPts[indexA].X * inPts[indexA].X + inPts[indexA].Y * inPts[indexA].Y);
                lc[1] = -(inPts[indexB].X * inPts[indexB].X + inPts[indexB].Y * inPts[indexB].Y);
                lc[2] = -(inPts[indexC].X * inPts[indexC].X + inPts[indexC].Y * inPts[indexC].Y);

                InverseU(ref lcircle, 3);
                MatrixCross(ref lcircle, ref lc, ref lres, 3);

                lDiameter = 2 * Math.Sqrt(lres[0] * lres[0] / 4 + lres[1] * lres[1] / 4 - lres[2]);
                lDiameter = lDiameter * 11.72;  //    pixel  to um

                cx = -lres[0] / 2;
                cy = -lres[0] / 2;
            }
            else
            {
                lDiameter = prevR * 11.72;
            }
            for (int i = 0; i < inPts.Length; i++)
            {
                lheight[i] = Math.Sqrt((inPts[i].X - cx) * (inPts[i].X - cx) + (inPts[i].X - cy) * (inPts[i].X - cy));
            }
            return true;
        }
        public void mcWeightedLMSCircle(WeightedPoint[] wp, int length, ref CircleInfo ci)
        {
            double[,] XXTinv = new double[3, 3];
            double[] XTA = new double[3];
            double[] A = new double[3];
            for (int i = 0; i < length; i++)
            {
                if (wp[i].W == 0) continue;
                XXTinv[0, 0] += wp[i].W * wp[i].X * wp[i].X;
                XXTinv[0, 1] += wp[i].W * wp[i].X * wp[i].Y;
                XXTinv[0, 2] += wp[i].W * wp[i].X;

                XXTinv[1, 0] += wp[i].W * wp[i].Y * wp[i].X;
                XXTinv[1, 1] += wp[i].W * wp[i].Y * wp[i].Y;
                XXTinv[1, 2] += wp[i].W * wp[i].Y;

                XXTinv[2, 0] += wp[i].W * wp[i].X;
                XXTinv[2, 1] += wp[i].W * wp[i].Y;
                XXTinv[2, 2] += wp[i].W;

                XTA[0] -= wp[i].W * wp[i].X * (wp[i].X * wp[i].X + wp[i].Y * wp[i].Y);
                XTA[1] -= wp[i].W * wp[i].Y * (wp[i].X * wp[i].X + wp[i].Y * wp[i].Y);
                XTA[2] -= wp[i].W * (wp[i].X * wp[i].X + wp[i].Y * wp[i].Y);
            }
            InverseU(ref XXTinv, 3);
            MatrixCross(ref XXTinv, ref XTA, ref A, 3);
            ci.X = -A[0] / 2;
            ci.Y = -A[1] / 2;
            ci.R = Math.Sqrt(-A[2] + ci.X * ci.X + ci.Y * ci.Y);
        }

        public void mcLMSProbabilityDensityFunction(Point2D[] wp, int length, ref Poly2nd p2nd)
        {
            //  Poly2nd.a, Poly2nd.b, Poly2nd.c =>
            //  log(y+2) = a( x - b )^2 + c
            double[,] XXTinv = new double[3, 3];
            double[] XTA = new double[3];
            double[] A = new double[3];
            double logY = 0;
            double XX = 0;
            for (int i = 0; i < length; i++)
            {
                XX            = wp[i].X * wp[i].X;
                double weight = wp[i].Y;

                XXTinv[0, 0] += weight * XX * XX;
                XXTinv[0, 1] += weight * XX * wp[i].X;
                XXTinv[0, 2] += weight * XX;

                XXTinv[1, 0] += weight * XX * wp[i].X;
                XXTinv[1, 1] += weight * XX;
                XXTinv[1, 2] += weight * wp[i].X;

                XXTinv[2, 0] += weight * XX;
                XXTinv[2, 1] += weight * wp[i].X;
                XXTinv[2, 2] += weight * 1;

                ////////////////////////////////////////////////////////////////
                ////  1. 확률밀도함수로 Fitting 하는 경우
                if (wp[i].Y > -2)
                    //logY = Math.Log(wp[i].Y + 3.1);
                    logY = Math.Log(wp[i].Y + 2);
                else
                    continue;

                logY = weight * logY * logY * logY;// * logY;// * logY;
                ////////////////////////////////////////////////////////////////

                //////////////////////////////////////////////////////////////
                //  2. 2차함수로 Fitting 하는 경우
                //if (wp[i].Y > -2)
                //    logY = wp[i].Y + 1;
                //else
                //    continue;
                //////////////////////////////////////////////////////////////

                //////////////////////////////////////////////////////////////
                //  4. 4차함수로 Fitting 하는 경우
                //if (wp[i].Y > -1)
                //    logY = Math.Sqrt(wp[i].Y + 3.5);    //  작은 값에 민감해진다.
                //else
                //    continue;
                //////////////////////////////////////////////////////////////

                //////////////////////////////////////////////////////////////
                //  4. n차함수로 Fitting 하는 경우
                //if (wp[i].Y > -1)
                //    //logY = Math.Pow((wp[i].Y + 4.0), 0.2);    // Simultion Data 로는 현재까지 중 거의 제일 좋은 수준
                //    logY = Math.Pow((wp[i].Y + 1.0), 0.4);    // 임시
                //else
                //    continue;
                //////////////////////////////////////////////////////////////

                XTA[0] += logY * XX;
                XTA[1] += logY * wp[i].X;
                XTA[2] += logY;
            }
            InverseU(ref XXTinv, 3);
            MatrixCross(ref XXTinv, ref XTA, ref A, 3);
            //  y = A[0] x^2 + A[1] + A[2];
            //  y = a( x - b )^2 + c
            p2nd.a = A[0];
            p2nd.b = -A[1] / (2 * A[0]);    //  Center of X
            p2nd.c = A[2] - A[1] * A[1] / (4 * A[0]);   //  Minimum Y
        }

        public void mcLMS2ndPoly(Point2D[] wp, int length, ref Poly2nd p2nd)
        {
            //  Poly2nd.a, Poly2nd.b, Poly2nd.c =>
            //  y = a( x - b )^2 + c
            double[,] XXTinv = new double[3, 3];
            double[] XTA = new double[3];
            double[] A = new double[3];
            double XX = 0;

            for (int i = 0; i < length; i++)
            {
                XX = wp[i].X * wp[i].X;
                XXTinv[0, 0] += XX * XX;
                XXTinv[0, 1] += XX * wp[i].X;
                XXTinv[0, 2] += XX;

                XXTinv[1, 0] += XX * wp[i].X;
                XXTinv[1, 1] += XX;
                XXTinv[1, 2] += wp[i].X;

                XXTinv[2, 0] += XX;
                XXTinv[2, 1] += wp[i].X;
                XXTinv[2, 2]++;

                XTA[0] += wp[i].Y * XX;
                XTA[1] += wp[i].Y * wp[i].X;
                XTA[2] += wp[i].Y;
            }
            InverseU(ref XXTinv, 3);
            MatrixCross(ref XXTinv, ref XTA, ref A, 3);
            //  y = A[0] x^2 + A[1] + A[2];
            //  y = a( x - b )^2 + c
            p2nd.a = A[0];
            p2nd.b = -A[1] / (2 * A[0]); //  Center of X
            p2nd.c = A[2] - A[1] * A[1] / (4 * A[0]);  //  Minimum Y
        }

        public void mcLMS2ndPoly(Point2D[] wp, int length, ref double[] A)
        {
            //  Poly2nd.a, Poly2nd.b, Poly2nd.c =>
            //  y = A[0] x^2 + A[1] + A[2];

            double[,] XXTinv = new double[3, 3];
            double[] XTA = new double[3];
            double XX = 0;

            for (int i = 0; i < length; i++)
            {
                XX = wp[i].X * wp[i].X;
                XXTinv[0, 0] += XX * XX;
                XXTinv[0, 1] += XX * wp[i].X;
                XXTinv[0, 2] += XX;

                XXTinv[1, 0] += XX * wp[i].X;
                XXTinv[1, 1] += XX;
                XXTinv[1, 2] += wp[i].X;

                XXTinv[2, 0] += XX;
                XXTinv[2, 1] += wp[i].X;
                XXTinv[2, 2]++;

                XTA[0] += wp[i].Y * XX;
                XTA[1] += wp[i].Y * wp[i].X;
                XTA[2] += wp[i].Y;
            }
            InverseU(ref XXTinv, 3);
            MatrixCross(ref XXTinv, ref XTA, ref A, 3);
            //  y = A[0] x^2 + A[1] + A[2];
        }

        public void mcLP2ndPoly(Point2D[] wp, int length, ref double[] A)
        {
            //  Simplyfied Linear Programming
            //  Poly2nd.a, Poly2nd.b, Poly2nd.c =>
            //  y = A[0] x^2 + A[1] + A[2];

            double[,] XXTinv = new double[3, 3];
            double[] XTA = new double[3];
            double XX = 0;

            for (int i = 0; i < length; i++)
            {
                XX = wp[i].X * wp[i].X;
                XXTinv[0, 0] += XX * XX;
                XXTinv[0, 1] += XX * wp[i].X;
                XXTinv[0, 2] += XX;

                XXTinv[1, 0] += XX * wp[i].X;
                XXTinv[1, 1] += XX;
                XXTinv[1, 2] += wp[i].X;

                XXTinv[2, 0] += XX;
                XXTinv[2, 1] += wp[i].X;
                XXTinv[2, 2]++;

                XTA[0] += wp[i].Y * XX;
                XTA[1] += wp[i].Y * wp[i].X;
                XTA[2] += wp[i].Y;
            }
            InverseU(ref XXTinv, 3);
            MatrixCross(ref XXTinv, ref XTA, ref A, 3);
            //  y = A[0] x^2 + A[1] + A[2];

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////  Minimize PP error using Cv2.MinAreaRect() => 사용불가. 적합하지 않음.

            ////////double A0org = A[0];
            ////////double A1org = A[1];
            ////////double A2org = A[2];

            ////////OpenCvSharp.Point[] lContour = new OpenCvSharp.Point[length];
            ////////double[] E = new double[length];
            ////////double[] Eopt = new double[length];
            ////////int[] Eindex = new int[length];
            ////////int[] Eindexopt = new int[length];

            ////////for (int i = 0; i < length; i++)
            ////////{
            ////////    lContour[i].X = (int)(1000*wp[i].X);
            ////////    E[i] = A0org * wp[i].X * wp[i].X + A1org * wp[i].X + A2org - wp[i].Y;
            ////////    lContour[i].Y = (int)(1000*E[i]);
            ////////    Eindex[i] = i;
            ////////}
            ////////RotatedRect rRect = Cv2.MinAreaRect(lContour);
            ////////double cx = rRect.Center.X;
            ////////double cy = rRect.Center.Y;
            ////////double ldegree = rRect.Angle;
            ////////if (Math.Abs(ldegree) > 45)
            ////////{
            ////////    if (ldegree > 0)
            ////////        ldegree = ldegree - 90;
            ////////    else
            ////////        ldegree = ldegree + 90;
            ////////}
            ////////double m = Math.Tan(ldegree/180*Math.PI);
            //////////  y - cy = m ( x - cx ) ; 
            //////////  y = m x - m cx + cy ;
            //////////  dA1 = m ;
            //////////  dA2 = cy - m cx ;

            ////////A[1] = A1org + m;
            //////////A[2] = A2org + (cy - m * cx)/1000;
            ////////for (int i = 0; i < length; i++)
            ////////{
            ////////    Eopt[i] = A0org * wp[i].X * wp[i].X + A[1] * wp[i].X + A[2] - wp[i].Y;
            ////////    Eindexopt[i] = i;
            ////////}
            ////////Array.Sort(E);
            ////////Array.Sort(Eopt, Eindexopt);
            ////////double Epp = E[length-1] - E[0];
            ////////double resEpp = Eopt[length - 1] - Eopt[0];
            ////////if ( Epp < resEpp)
            ////////{
            ////////    //  결과가 나쁘므로 원상복귀
            ////////    A[1] = A1org;
            ////////    A[2] = A2org;
            ////////}
            ////////A1org = A[1];
            ////////A2org = A[2];


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////  Minimize PP error 

            //double A0org = A[0];
            //double A1org = A[1];
            //double A2org = A[2];
            double[] _A = new double[3];
            _A[0] = A[0];
            _A[1] = A[1];
            _A[2] = A[2];

            //OpenCvSharp.Point[] lContour = new OpenCvSharp.Point[length];
            double[] E = new double[length];
            double[] Eorg = new double[length];
            int[] Eindex = new int[length];
            int[] Eindexopt = new int[length];
            double[] tmpE = new double[length];
            int[] tmpEindex = new int[length];
            int NoBetterOne = 0;
            double tmpEmax = 0;
            double tmpEmin = 0;
            double Emax = 0;
            double Emin = 0;
            double Epp = 0;
            double Epp0 = 0;
            double EppPre = 0;
            double EppOrg = 0;
            double dAstepPre = 0;
            int iEj1 = 0;
            int iEj2 = 0;
            double Ej1 = 0;
            double Ej2 = 0;
            double dA0 = 0;
            double dA1 = 0;
            


            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// 
            ///     A0
            ///     
            for (int i = 0; i < length; i++)
            {
                E[i] = _A[0] * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                Eindex[i] = i;
            }
            Array.Copy(E, Eorg, length);
            Array.Sort(E, Eindex);
            Emax = E[length - 1];
            Emin = E[0];
            Epp = Emax - Emin;
            Epp0 = Epp;
            EppOrg = Epp;

            NoBetterOne = 0;

            dA1 = (E[1] - E[0]) / (wp[Eindex[0]].X - wp[Eindex[1]].X);
            double dA1step = dA1 / 2;
            while (true)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  A0 loop
                //  A0 = A0 + dA0 에서 음의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
                if (Math.Abs(dA1) < 1.0e-8)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[0];  //  음의 최대오차점 인덱스
                    iEj2 = Eindex[1];  //  음의 최대 2번째 오차점 인덱스
                    Ej1 = _A[0] * wp[iEj1].X * wp[iEj1].X + (_A[1] + dA1) * wp[iEj1].X + _A[2] - wp[iEj1].Y; //  음의 최대오차
                    Ej2 = _A[0] * wp[iEj2].X * wp[iEj2].X + (_A[1] + dA1) * wp[iEj2].X + _A[2] - wp[iEj2].Y; //  음의 최대 2번쨰 오차
                    if (Ej1 > Emin && Ej2 > Emin)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = _A[0] * wp[i].X * wp[i].X + (_A[1] + dA1) * wp[i].X + _A[2] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA1 -= dA1step;
                            dA1step = dA1step / 2;
                        }
                        else
                        {
                            if (Math.Abs(Ej1 - Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[1] = _A[1] + dA1;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex = new int[11];
                                dA1step = dA1step / 5;
                                for (int j = -5; j < 6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = _A[0] * wp[i].X * wp[i].X + (_A[1] + dA1 + j * dA1step) * wp[i].X + _A[2] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j + 5] = tmpEmax - tmpEmin;
                                    EppjIndex[j + 5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[1] = _A[1] + dA1 + EppjIndex[0] * dA1step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA1 -= dA1step;
                        dA1step = dA1step / 2;
                    }
                    if (dA1step < 1.0e-10)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if (NoBetterOne == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }
            NoBetterOne = 0;
            //  A1 = A1 + dA1 에서 양의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
            dA1 = (E[length - 2] - E[length - 1]) / (wp[Eindex[length - 1]].X * wp[Eindex[length - 1]].X - wp[Eindex[length - 2]].X * wp[Eindex[length - 2]].X);
            dA1step = dA1 / 2;
            while (true)
            {
                if (Math.Abs(dA1) < 1.0e-8)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[length - 1];  //  양의 최대오차점 인덱스
                    iEj2 = Eindex[length - 2];  //  양의 최대 2번째 오차점 인덱스
                    Ej1 = _A[0] * wp[iEj1].X * wp[iEj1].X + (_A[1] + dA1) * wp[iEj1].X + _A[2] - wp[iEj1].Y; //  양의 최대오차
                    Ej2 = _A[0] * wp[iEj2].X * wp[iEj2].X + (_A[1] + dA1) * wp[iEj2].X + _A[2] - wp[iEj2].Y; //  양의 최대 2번쨰 오차
                    if (Ej1 < Emax && Ej2 < Emax)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = _A[0] * wp[i].X * wp[i].X + (_A[1] + dA1) * wp[i].X + _A[2] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA1 -= dA1step;
                            dA1step = dA1step / 2;
                        }
                        else
                        {
                            if (Math.Abs(Ej1 - Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[1] = _A[1] + dA1;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex = new int[11];
                                dA1step = dA1step / 5;
                                for (int j = -5; j < 6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = _A[0] * wp[i].X * wp[i].X + (_A[1] + dA1 + j * dA1step) * wp[i].X + _A[2] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j + 5] = tmpEmax - tmpEmin;
                                    EppjIndex[j + 5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[1] = _A[1] + dA1 + EppjIndex[0] * dA1step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA1 -= dA1step;
                        dA1step = dA1step / 2;
                    }
                    if (dA1step < 1.0e-10)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if (NoBetterOne == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// 
            ///     A1
            ///     
            NoBetterOne = 0;
            //  A0 = A0 + dA0 에서 음의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
            dA0 = (E[1] - E[0]) / (wp[Eindex[0]].X * wp[Eindex[0]].X - wp[Eindex[1]].X * wp[Eindex[1]].X);
            double dA0step = dA0 / 2;
            dAstepPre = dA0step;
            while (true)
            {
                if (Math.Abs(dA0) < 1.0e-11)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[0];  //  음의 최대오차점 인덱스
                    iEj2 = Eindex[1];  //  음의 최대 2번째 오차점 인덱스
                    Ej1 = (_A[0] + dA0) * wp[iEj1].X * wp[iEj1].X + _A[1] * wp[iEj1].X + _A[2] - wp[iEj1].Y; //  음의 최대오차
                    Ej2 = (_A[0] + dA0) * wp[iEj2].X * wp[iEj2].X + _A[1] * wp[iEj2].X + _A[2] - wp[iEj2].Y; //  음의 최대 2번쨰 오차
                    if (Ej1 > Emin && Ej2 > Emin)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = (_A[0] + dA0) * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA0 -= dA0step;
                            dAstepPre = -dA0step;
                            dA0step = dA0step / 2;
                            EppPre = Epp;
                        }
                        else
                        {
                            if (Math.Abs(Ej1-Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[0] = _A[0] + dA0;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex= new int[11];
                                dA0step = dA0step / 5;
                                for (int j=-5; j<6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = (_A[0] + dA0 + j*dA0step) * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j+5] = tmpEmax - tmpEmin;
                                    EppjIndex[j+5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[0] = _A[0] + dA0 + EppjIndex[0] * dA0step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA0 -= dA0step;
                        dA0step = dA0step / 2;
                    }

                    if (Math.Abs(dA0step)<1.0e-11)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if(NoBetterOne==0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }
            NoBetterOne = 0;
            //  A0 = A0 + dA0 에서 양의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
            dA0 = (E[length - 2] - E[length - 1]) / (wp[Eindex[length - 1]].X * wp[Eindex[length - 1]].X - wp[Eindex[length - 2]].X * wp[Eindex[length - 2]].X);
            dA0step = dA0 / 2;
            while(true)
            {
                if (Math.Abs(dA0) < 1.0e-11)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[length - 1];  //  양의 최대오차점 인덱스
                    iEj2 = Eindex[length - 2];  //  양의 최대 2번째 오차점 인덱스
                    Ej1 = (_A[0] + dA0) * wp[iEj1].X * wp[iEj1].X + _A[1] * wp[iEj1].X + _A[2] - wp[iEj1].Y; //  양의 최대오차
                    Ej2 = (_A[0] + dA0) * wp[iEj2].X * wp[iEj2].X + _A[1] * wp[iEj2].X + _A[2] - wp[iEj2].Y; //  양의 최대 2번쨰 오차
                    if (Ej1 < Emax && Ej2 < Emax)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = (_A[0] + dA0) * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA0 -= dA0step;
                            dA0step = dA0step / 2;
                        }
                        else
                        {
                            if (Math.Abs(Ej1 - Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[0] = _A[0] + dA0;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex = new int[11];
                                dA0step = dA0step / 5;
                                for (int j = -5; j < 6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = (_A[0] + dA0 + j * dA0step) * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j + 5] = tmpEmax - tmpEmin;
                                    EppjIndex[j + 5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[0] = _A[0] + dA0 + EppjIndex[0] * dA0step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA0 -= dA0step;
                        dA0step = dA0step / 2;
                    }
                    if (Math.Abs(dA0step) < 1.0e-11)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if (NoBetterOne == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X * wp[i].X + _A[1] * wp[i].X + _A[2] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }


            A[0] = _A[0];
            A[1] = _A[1];
        }

        public void mcLMS1stPoly(Point2D[] wp, int length, ref double a, ref double b, int istart = 0, int count = -1)
        {
            //  y = a x + b
            double[,] XXTinv = new double[2, 2];
            double[] XTA = new double[2];
            double[] A = new double[2];

            int iEnd = length;
            if (count > 0)
                iEnd = istart + count;

            for (int i = istart; i < iEnd; i++)
            {
                XXTinv[0, 0] += wp[i].X * wp[i].X;
                XXTinv[0, 1] += wp[i].X;

                XXTinv[1, 0] += wp[i].X;
                XXTinv[1, 1]++;

                XTA[0] += wp[i].Y * wp[i].X;
                XTA[1] += wp[i].Y;
            }
            InverseU(ref XXTinv, 2);
            MatrixCross(ref XXTinv, ref XTA, ref A, 2);
            a = A[0];
            b = A[1];
        }
        public void mcLP1stPoly(Point2D[] wp, int length, ref double a, ref double b, int istart = 0, int count = -1)
        {
            //  y = a x + b
            double[,] XXTinv = new double[2, 2];
            double[] XTA = new double[2];
            double[] A = new double[2];
            double[] _A = new double[2];

            int iEnd = length;
            if (count > 0)
                iEnd = istart + count;

            for (int i = istart; i < iEnd; i++)
            {
                XXTinv[0, 0] += wp[i].X * wp[i].X;
                XXTinv[0, 1] += wp[i].X;

                XXTinv[1, 0] += wp[i].X;
                XXTinv[1, 1]++;

                XTA[0] += wp[i].Y * wp[i].X;
                XTA[1] += wp[i].Y;
            }
            InverseU(ref XXTinv, 2);
            MatrixCross(ref XXTinv, ref XTA, ref A, 2);
            a = A[0];
            b = A[1];


            double A0org = A[0];
            double A1org = A[1];
            _A[0] = A[0];
            _A[1] = A[1];

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////  Minimize PP error 

            double[] E = new double[length];
            double[] Eorg = new double[length];
            int[] Eindex = new int[length];
            int[] Eindexopt = new int[length];
            double[] tmpE = new double[length];
            int[] tmpEindex = new int[length];
            int NoBetterOne = 0;
            double tmpEmax = 0;
            double tmpEmin = 0;
            double Emax = 0;
            double Emin = 0;
            double Epp = 0;
            double Epp0 = 0;
            double EppPre = 0;
            double EppOrg = 0;
            double dAstepPre = 0;
            int iEj1 = 0;
            int iEj2 = 0;
            double Ej1 = 0;
            double Ej2 = 0;
            double dA0 = 0;
            //double dA1 = 0;

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// 
            ///     A1
            ///     
            for (int i = 0; i < length; i++)
            {
                E[i] = _A[0] * wp[i].X + _A[1] - wp[i].Y;
                Eindex[i] = i;
            }
            Array.Copy(E, Eorg, length);
            Array.Sort(E, Eindex);
            Emax = E[length - 1];
            Emin = E[0];
            Epp = Emax - Emin;
            Epp0 = Epp;
            EppOrg = Epp;

            NoBetterOne = 0;
            
            dA0 = (E[1] - E[0]) / (wp[Eindex[0]].X - wp[Eindex[1]].X);
            double dA0step = dA0 / 2;
            while (true)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  A0 loop
                //  A0 = A0 + dA0 에서 음의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
                if (Math.Abs(dA0) < 1.0e-8)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[0];  //  음의 최대오차점 인덱스
                    iEj2 = Eindex[1];  //  음의 최대 2번째 오차점 인덱스
                    Ej1 = (_A[0] + dA0) * wp[iEj1].X + _A[1] - wp[iEj1].Y; //  음의 최대오차
                    Ej2 = (_A[0] + dA0) * wp[iEj2].X + _A[1] - wp[iEj2].Y; //  음의 최대 2번쨰 오차
                    if (Ej1 > Emin && Ej2 > Emin)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = (_A[0] + dA0) * wp[i].X + _A[1] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA0 -= dA0step;
                            dA0step = dA0step / 2;
                        }
                        else
                        {
                            if (Math.Abs(Ej1 - Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[0] = _A[0] + dA0;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex = new int[11];
                                dA0step = dA0step / 5;
                                for (int j = -5; j < 6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = (_A[0] + dA0 + j * dA0step) * wp[i].X + _A[1] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j + 5] = tmpEmax - tmpEmin;
                                    EppjIndex[j + 5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[0] = _A[0] + dA0 + EppjIndex[0] * dA0step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA0 -= dA0step;
                        dA0step = dA0step / 2;
                    }
                    if (dA0step < 1.0e-8)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if (NoBetterOne == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X + _A[1] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }
            NoBetterOne = 0;
            //  A1 = A1 + dA1 에서 양의 최대오차와 두번째 오차가 값이 같아지도록 하는 dA0 
            dA0 = (E[length - 2] - E[length - 1]) / (wp[Eindex[length - 1]].X  - wp[Eindex[length - 2]].X);
            dA0step = dA0 / 2;
            while (true)
            {
                if (Math.Abs(dA0) < 1.0e-8)
                {
                    NoBetterOne++;
                    break;
                }
                else
                {
                    iEj1 = Eindex[length - 1];  //  양의 최대오차점 인덱스
                    iEj2 = Eindex[length - 2];  //  양의 최대 2번째 오차점 인덱스
                    Ej1 = (_A[0] + dA0) * wp[iEj1].X + _A[1] - wp[iEj1].Y; //  양의 최대오차
                    Ej2 = (_A[0] + dA0) * wp[iEj2].X + _A[1] - wp[iEj2].Y; //  양의 최대 2번쨰 오차
                    if (Ej1 < Emax && Ej2 < Emax)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tmpE[i] = (_A[0] + dA0) * wp[i].X + _A[1] - wp[i].Y;
                            tmpEindex[i] = i;
                        }
                        Array.Sort(tmpE, tmpEindex);
                        tmpEmax = tmpE[length - 1];
                        tmpEmin = tmpE[0];

                        Epp = tmpEmax - tmpEmin;
                        if (Epp0 <= Epp)
                        {
                            dA0 -= dA0step;
                            dA0step = dA0step / 2;
                        }
                        else
                        {
                            if (Math.Abs(Ej1 - Ej2) < 1.0e-3)
                            {
                                Array.Copy(tmpE, E, length);
                                Array.Copy(tmpEindex, Eindex, length);
                                Emax = tmpEmax;
                                Emin = tmpEmin;
                                Epp0 = Epp;
                                _A[0] = _A[0] + dA0;
                                break;
                            }
                            else
                            {
                                //  결과가 좋은 경우 주변 Scan 필요
                                double[] Eppj = new double[11];
                                int[] EppjIndex = new int[11];
                                dA0step = dA0step / 5;
                                for (int j = -5; j < 6; j++)
                                {
                                    for (int i = 0; i < length; i++)
                                    {
                                        tmpE[i] = (_A[0] + dA0 + j * dA0step) * wp[i].X + _A[1] - wp[i].Y;
                                        tmpEindex[i] = i;
                                    }
                                    Array.Sort(tmpE, tmpEindex);
                                    tmpEmax = tmpE[length - 1];
                                    tmpEmin = tmpE[0];

                                    Eppj[j + 5] = tmpEmax - tmpEmin;
                                    EppjIndex[j + 5] = j;
                                }
                                Array.Sort(Eppj, EppjIndex);
                                Epp = Eppj[0];
                                _A[0] = _A[0] + dA0 + EppjIndex[0] * dA0step; //  더 정확히 구해봐야 결과 차이 미미
                                break;
                            }
                        }
                    }
                    else
                    {
                        dA0 -= dA0step;
                        dA0step = dA0step / 2;
                    }
                    if (dA0step < 1.0e-8)
                    {
                        NoBetterOne++;
                        break;
                    }
                }
            }
            if (NoBetterOne == 0)
            {
                for (int i = 0; i < length; i++)
                {
                    E[i] = _A[0] * wp[i].X + _A[1] - wp[i].Y;
                    Eindex[i] = i;
                }

                Array.Sort(E, Eindex);
                Emax = E[length - 1];
                Emin = E[0];
                Epp = Emax - Emin;
                Epp0 = Epp;
            }
            a = _A[0];
            b = _A[1];
        }

        public bool mcRefSurfDirVector(double oisxTilt, double oisyTilt, ref double[] DirVector)
        {
            //측정된 X 회전각 Φ, Y 회전각 θ 로부터 Refence 면의 방향벡터를 구한다.
            //  Radian 입력
            //이상적 상태는(1,0,0) 이며, 
            //회전각 반영 시 N = (1 / sqrt(1 + Φ * Φ + θ * θ), Φ, θ)"
            DirVector[0] = 1 / Math.Sqrt(1 + oisxTilt * oisxTilt + oisyTilt * oisyTilt);
            DirVector[1] = oisxTilt;
            DirVector[2] = -oisyTilt;
            return true;
        }
        public void mCalcParallelism(double[] Lshaft, double[] Rshaft, double refDistUm, ref double distUm, ref double resDeg)
        {
            double innerProduct = Lshaft[0] * Rshaft[0] + Lshaft[1] * Rshaft[1] + Lshaft[2] * Rshaft[2];
            double absLR = Math.Sqrt(Lshaft[0] * Lshaft[0] + Lshaft[1] * Lshaft[1] + Lshaft[2] * Lshaft[2]);
            absLR = absLR * Math.Sqrt(Rshaft[0] * Rshaft[0] + Rshaft[1] * Rshaft[1] + Rshaft[2] * Rshaft[2]);

            double theta = Math.Acos(innerProduct / absLR);
            distUm = theta * refDistUm;
            resDeg = theta * 180 / Math.PI;
        }

        /// <summary>
        /// 
        /// Lens1G Only Use  Lens1G Only Use     Lens1G Only Use     Lens1G Only Use     Lens1G Only Use
        /// 
        /// </summary>
        /// <param name="lpt"></param>
        /// <returns></returns>
        //public bool mMeasureLens1GLaserSample(ref Point[] lpt)
        //{
        //    //"레이져라인빔으로부터 각 Shaft 의 대표점 측정
        //    //Front View"
        //    return true;
        //}
        //public bool mMeasureLens1GLaserMaster(ref Point[] lpt)
        //{
        //    //"레이져라인빔으로부터 소정의 위치에서의 X좌표 측정
        //    //Front View"
        //    return true;
        //}
        //public bool mMeasureLens1GLEDSample(ref Point[] lpt)
        //{
        //    //"LED1 조명 On 시 Shaft 의 대표점 측정
        //    //LED2 조명 On 시 Shaft 의 대표점 측정"
        //    return true;
        //}
        //public bool mMeasureLEDMaster(ref Point[] lpt)
        //{
        //    //"LED1 조명 On 시 반사점 측정
        //    //LED2 조명 On 시 반사점 측정"
        //    return true;
        //}

        public bool mCalcParallelismCD(double[] surfN1, double[] surfN2, ref double lparal)
        {
            //C면 및 D 면의 방정식에 각각(X, Y) = (0, -5.5), (0, 5.5), (17, -5.5), (17, 5.5) 를 넣었을 때 Z 값의 차이(부호 포함)의 최대값과 최소값간의 차이값
            double[] unitN1 = new double[3];
            double[] unitN2 = new double[3];
            double[] lZ1 = new double[4];
            double[] lZ2 = new double[4];

            unitN1[1] = surfN1[1] / surfN1[0];
            unitN1[2] = surfN1[2] / surfN1[0];
            unitN2[1] = surfN2[1] / surfN2[0];
            unitN2[2] = surfN2[2] / surfN2[0];

            //  SurfN1 : x + unitN1[1] y + unitN1[2] z = 0;
            //  SurfN2 : x + unitN2[1] y + unitN2[2] z = 0;

            lZ1[0] = (-unitN1[1] * (-5500)) / unitN1[2];
            lZ1[1] = (-unitN1[1] * 5500) / unitN1[2];
            lZ1[2] = (-17000 - unitN1[1] * (-5500)) / unitN1[2];
            lZ1[3] = (-17000 - unitN1[1] * 5500) / unitN1[2];

            lZ2[0] = (-unitN2[1] * (-5500)) / unitN2[2];
            lZ2[1] = (-unitN2[1] * 5500) / unitN2[2];
            lZ2[2] = (-17000 - unitN2[1] * (-5500)) / unitN2[2];
            lZ2[3] = (-17000 - unitN2[1] * 5500) / unitN2[2];

            double minZ = 9999;
            double maxZ = -9999;
            for (int i = 0; i < 4; i++)
            {
                if (minZ > (lZ1[i] - lZ2[i]))
                    minZ = (lZ1[i] - lZ2[i]);
                if (maxZ < (lZ1[i] - lZ2[i]))
                    maxZ = (lZ1[i] - lZ2[i]);
            }
            lparal = maxZ - minZ;   //  unit : mm
            return true;
        }
        public bool mCalcParallelismAB(double[] surfN1, double[] surfN2, ref double lparal)
        {
            //A면 및 B면의 각각의 방향벡터로부터 중심이(0,0,0) 인 평면방정식을 가정하여, (X, Y, Z) = (x, -2, -3), (x, 2, -3), (x, -2, 3),(x, 2, 3) 의 4점을 넣었을 때 X 값의 차이(부호포함)의 최대값과 최소값간의 차이값
            double[] unitN1 = new double[3];
            double[] unitN2 = new double[3];
            double[] lZ1 = new double[4];
            double[] lZ2 = new double[4];

            unitN1[1] = surfN1[1] / surfN1[0];
            unitN1[2] = surfN1[2] / surfN1[0];
            unitN2[1] = surfN2[1] / surfN2[0];
            unitN2[2] = surfN2[2] / surfN2[0];

            //  SurfN1 : x + unitN1[1] y + unitN1[2] z = 0;
            //  SurfN2 : x + unitN2[1] y + unitN2[2] z = 0;
            lparal = 5000 * Math.Sqrt((unitN1[1] - unitN2[1]) * (unitN1[1] - unitN2[1]) + (unitN1[2] - unitN2[2]) * (unitN1[2] - unitN2[2]));

            return true;
        }

        public void SetLastGap(double x, double y, bool IsLeft)
        {
            int nLR = IsLeft ? 0 : 1;
            mLastGap[nLR, 0] = x;
            mLastGap[nLR, 1] = y;
        }

        public void Set3Dto2DConvertMatrix(int ID, double latitude, double longitude)
        {
            //  X_3d => ix = x cos longitude
            mPlotAB[ID].mViewCosPhi = Math.Cos(longitude / 180 * M_PI);     //  경도
            mPlotAB[ID].mViewSinPhi = Math.Sin(longitude / 180 * M_PI);
            mPlotAB[ID].mViewCosTheta = Math.Cos(latitude / 180 * M_PI);    //  위도
            mPlotAB[ID].mViewSinTheta = Math.Sin(latitude / 180 * M_PI);
        }

        public void Convert3Dto2D(int ID, double[,] p3D, ref double[,] p2D, int count)
        {
            //  Converting 
            if ( ID== 0 )
            {
                for (int i = 0; i < count; i++)
                {
                    p2D[i, 1] = p3D[i, 0] * mPlotAB[ID].mViewCosPhi - p3D[i, 1] * mPlotAB[ID].mViewSinPhi;
                    p2D[i, 0] = -(p3D[i, 0] * mPlotAB[ID].mViewSinPhi * mPlotAB[ID].mViewSinTheta + p3D[i, 1] * mPlotAB[ID].mViewCosPhi * mPlotAB[ID].mViewSinTheta + p3D[i, 2] * mPlotAB[ID].mViewCosTheta);
                }
            }else if ( ID == 1)
            {
                for (int i = 0; i < count; i++)
                {
                    p2D[i, 0] = p3D[i, 0] * mPlotAB[ID].mViewCosPhi - p3D[i, 1] * mPlotAB[ID].mViewSinPhi;
                    p2D[i, 1] = p3D[i, 0] * mPlotAB[ID].mViewSinPhi * mPlotAB[ID].mViewSinTheta + p3D[i, 1] * mPlotAB[ID].mViewCosPhi * mPlotAB[ID].mViewSinTheta + p3D[i, 2] * mPlotAB[ID].mViewCosTheta;
                }
            }
        }
        public void Convert3Dto2D(int ID, double[,,] p3D, ref double[,,] p2D, int length, int count)
        {
            //  Converting 
            for (int g = 0; g < length; g++)
                for (int i = 0; i < count; i++)
                {
                    p2D[g, i, 0] = p3D[g, i, 0] * mPlotAB[ID].mViewCosPhi - p3D[g, i, 1] * mPlotAB[ID].mViewSinPhi;
                    p2D[g, i, 1] = p3D[g, i, 0] * mPlotAB[ID].mViewSinPhi * mPlotAB[ID].mViewSinTheta + p3D[g, i, 1] * mPlotAB[ID].mViewCosPhi * mPlotAB[ID].mViewSinTheta + p3D[g, i, 2] * mPlotAB[ID].mViewCosTheta;
                }
        }
        public void ConvertAll3Dto2D(int ID = 0)
        {
            if (ID == 0)
            {
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisXP, ref mPlotAB[ID].m2D_AxisXP, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisYP, ref mPlotAB[ID].m2D_AxisYP, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisXN, ref mPlotAB[ID].m2D_AxisXN, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisYN, ref mPlotAB[ID].m2D_AxisYN, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisZ, ref mPlotAB[ID].m2D_AxisZ, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfBN, ref mPlotAB[ID].m2D_SurfBN, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfA, ref mPlotAB[ID].m2D_SurfA, 4);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfB, ref mPlotAB[ID].m2D_SurfB, 4);//  Converting 
            }
            else
            {
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisXP, ref mPlotAB[ID].m2D_AxisXP, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisYP, ref mPlotAB[ID].m2D_AxisYP, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisXN, ref mPlotAB[ID].m2D_AxisXN, 2);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_AxisYN, ref mPlotAB[ID].m2D_AxisYN, 2);//  Converting 

                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfC, ref mPlotAB[ID].m2D_SurfC, 4);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfD, ref mPlotAB[ID].m2D_SurfD, 6);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfCorg, ref mPlotAB[ID].m2D_SurfCorg, 4);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfDorg, ref mPlotAB[ID].m2D_SurfDorg, 6);//  Converting 
                Convert3Dto2D(ID, mPlotAB[ID].m3D_SurfZ, ref mPlotAB[ID].m2D_SurfZ, 4);//  Converting 
            }
        }
        public void GenerateRotationData(int ID, double curTheta = 0, double curPsi = 0)
        {
            double[,] Rr = new double[3, 3];
            double theta = 0;
            double psi = 0;

            theta = (curTheta + 135) / 180.0 * M_PI; //  기준 위치
            psi = curPsi / 180.0 * M_PI;

            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////
            //  Rotate N0 by R with Offset Rc
            // Surface Equation : N * ( P - P0 ) = 0 ; P0 = (0,0,0)
            theta = theta - 135.0 / 180 * M_PI; //  여기 135 는 절대 불변 회전축 Offset 에해 회전하면서 반사점의 이동을 계산

            RotationR(theta, psi, ref Rr);

            double[,] l3D_SurfB = new double[4, 3];
            double[,] l3D_resSurfB = new double[4, 3];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    l3D_SurfB[i, j] = mPlotAB[ID].m3D_SurfBorg[i, j];

            RotateRectangle(Rr, l3D_SurfB, ref l3D_resSurfB);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    mPlotAB[ID].m3D_SurfB[i, j] = l3D_resSurfB[i, j];

            RotateLine(Rr, mPlotAB[ID].m3D_SurfBNorg, ref mPlotAB[ID].m3D_SurfBN);
            return;
        }
        public void RotateLine(double[,] Rr, double[,] srcLine, ref double[,] resLine)
        {
            double[] surfPi0 = new double[3];
            double[] surfPi = new double[3];
            double[] pZero = new double[3] { 0, 0, 0 }; //  constance, Center of rotation without Offset.

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                    surfPi0[j] = srcLine[i, j]; //  j=0,1,2 => x,y,z 

                MatrixCross(ref Rr, ref surfPi0, ref surfPi, 3);    //  surfNi0 : A point on prism inlet surface
                for (int j = 0; j < 3; j++)
                    resLine[i, j] = surfPi[j];

            }
        }

        public void RotateRectangle(double[,] Rr, double[,] srcRect, ref double[,] resRect)
        {
            double[] surfPi0 = new double[3];
            double[] surfPi = new double[3];
            double[] pZero = new double[3] { 0, 0, 0 }; //  constance, Center of rotation without Offset.

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                    surfPi0[j] = srcRect[i, j];

                MatrixCross(ref Rr, ref surfPi0, ref surfPi, 3);    //  surfNi0 : A point on prism inlet surface
                for (int j = 0; j < 3; j++)
                    resRect[i, j] = surfPi[j];

            }
        }
        public void mCalcSurfacefrom3DPoints(Vector3D[] dataPts, ref Surface3D surInfo)
        {
            int len = dataPts.Length;

            double[] res = new double[3];
            double[] C = new double[len];
            double[,] lX = new double[len, 3];
            double[,] lXT = new double[3, len];
            double[,] lA = new double[3, len];
            double[,] XTX = new double[3, 3];
            double[,] invXTX = null;
            for (int i= 0; i < len; i++)
            {
                lX[i, 0] = dataPts[i].X;
                lX[i, 1] = dataPts[i].Y;
                lX[i, 2] = dataPts[i].Z;
                C[i] = 1;
            }
            MatrixTranspose(lX, ref lXT, 3, len);

            MatrixCross(ref lXT, ref lX, ref XTX, 3, len);

            invXTX = MatrixInverse(XTX, 3);

            MatrixCross(ref invXTX, ref lXT, ref lA, 3, 3, len);
            //  3bylen x lenby1 = 3by1
            MatrixCross(ref lA, ref C, ref res, 3, len);

            //  N * ( X - P ) = 0
            surInfo.N.X = res[0];
            surInfo.N.Y = res[1];
            surInfo.N.Z = res[2];
            surInfo.P.X = 0;
            surInfo.P.Y = 0;
            surInfo.P.Z = 1/ res[2];
        }
        public void PointsfromSingleSegment(int type, Vector3D[] p, out double[][] res)
        {
            int MinOrMax = type % 2;
            int len = p.Length;
            List<double[]> lres = new List<double[]>();
            double[] pt = new double[3]; 
            //double lx = 0;
            double ly = 0;

            if (type / 2 == 0)
            {
                //  Single Segment Along X min or max
                ly = p[0].Y;
                for (int i = 0; i < len; i++)
                {
                    if (MinOrMax == 0)
                    {
                        //  Find Max Point
                        if (pt[2] < p[i].Z)
                        {
                            pt[0] = p[i].X;
                            pt[1] = p[i].Y;
                            pt[2] = p[i].Z;
                        }
                    }
                    else
                    {
                        //  Find Min Point
                        if (pt[2] > p[i].Z)
                        {
                            pt[0] = p[i].X;
                            pt[1] = p[i].Y;
                            pt[2] = p[i].Z;
                        }
                    }
                    if (i < len - 1)
                    {
                        if (p[i + 1].Y == ly)
                        {
                            lres.Add(pt);
                            pt = new double[3];
                            ly = p[i + 1].Y;
                            if (MinOrMax == 0)
                                //  Find Max Point
                                pt[2] = -999999999;
                            else
                                //  Find Min Point
                                pt[2] = 999999999;
                        }
                    }
                }
                lres.Add(pt);
                res = lres.ToArray();
            }
            else if (type / 2 == 1)
            {
                //  Single Segment Along Y min or max
                ly = p[0].Y;
                for (int i = 0; i < len; i++)
                {
                    if (p[i].Y == ly)
                        break;

                    pt[0] = p[i].X;
                    pt[1] = p[i].Y;
                    pt[2] = p[i].Z;
                    lres.Add(pt);
                    pt = new double[3];
                }
                int lwidth = lres.Count;
                for (int i = lwidth; i < len; i++)
                {
                    if (MinOrMax == 0)
                    {
                        //  Find Max Point
                        if (lres[i % lwidth][2] < p[i].Z)
                        {
                            lres[i % lwidth][0] = p[i].X;
                            lres[i % lwidth][1] = p[i].Y;
                            lres[i % lwidth][2] = p[i].Z;
                        }
                    }
                    else
                    {
                        //  Find Min Point
                        if (lres[i % lwidth][2] > p[i].Z)
                        {
                            lres[i % lwidth][0] = p[i].X;
                            lres[i % lwidth][1] = p[i].Y;
                            lres[i % lwidth][2] = p[i].Z;
                        }
                    }
                }
                res = lres.ToArray();
            }
            else if (type / 2 == 2)
            {
                //  Single Segment Average Area
                for (int i = 0; i < len; i++)
                {
                    pt[0] += p[i].X;
                    pt[1] += p[i].Y;
                    pt[2] += p[i].Z;
                }
                pt[0] = pt[0] / len;
                pt[1] = pt[1] / len;
                pt[2] = pt[2] / len;
                lres.Add(pt);
                res = lres.ToArray();
            }
            else
                res = null;
        }
        public void PointsfromSingleSegment(int type, Vector3D[] p, out Vector3D[] res, int xPitch=0)
        {
            int MinOrMax = type % 2;
            int len = p.Length;
            List<Vector3D> lres = new List<Vector3D>();
            Vector3D pt = new Vector3D();
            //double lx = 0;
            double ly = 0;

            if (type / 2 == 0)
            {
                //  Single Segment Along X min or max
                //if (xPitch == 0)
                //{
                    ly = p[0].Y;
                    for (int i = 0; i < len; i++)
                    {
                        if (MinOrMax == 0)
                        {
                            //  Find Max Point
                            if (pt.Z < p[i].Z)
                            {
                                pt.X = p[i].X;
                                pt.Y = p[i].Y;
                                pt.Z = p[i].Z;
                            }
                        }
                        else
                        {
                            //  Find Min Point
                            if (pt.Z > p[i].Z)
                            {
                                pt.X = p[i].X;
                                pt.Y = p[i].Y;
                                pt.Z = p[i].Z;
                            }
                        }
                        if (i < len - 1)
                        {
                            if (p[i + 1].Y == ly)
                            {
                                lres.Add(pt);
                                pt = new Vector3D();
                                ly = p[i + 1].Y;
                                if (MinOrMax == 0)
                                    //  Find Max Point
                                    pt.Z = -999999999;
                                else
                                    //  Find Min Point
                                    pt.Z = 999999999;
                            }
                        }
                    }
                //}
                //else
                //{
                //    for (int i = 0; i < len; i++)
                //    {
                //        if (MinOrMax == 0)
                //        {
                //            //  Find Max Point
                //            if (pt.Z < p[i].Z)
                //            {
                //                pt.X = p[i].X;
                //                pt.Y = p[i].Y;
                //                pt.Z = p[i].Z;
                //            }
                //        }
                //        else
                //        {
                //            //  Find Min Point
                //            if (pt.Z > p[i].Z)
                //            {
                //                pt.X = p[i].X;
                //                pt.Y = p[i].Y;
                //                pt.Z = p[i].Z;
                //            }
                //        }
                //        if (i < len - 1)
                //        {
                //            if ((i % xPitch) == (xPitch - 1))
                //            {
                //                lres.Add(pt);
                //                pt = new Vector3D();
                //                lx = p[i + 1].X;
                //                if (MinOrMax == 0)
                //                    //  Find Max Point
                //                    pt.Z = -999999999;
                //                else
                //                    //  Find Min Point
                //                    pt.Z = 999999999;
                //            }
                //        }
                //    }
                //}
                lres.Add(pt);
                res = lres.ToArray();
            }
            else if (type / 2 == 1)
            {
                //  Single Segment Along Y min or max
                ly = p[0].Y;
                for (int i = 0; i < len; i++)
                {
                    if (p[i].Y == ly)
                        break;

                    pt.X = p[i].X;
                    pt.Y = p[i].Y;
                    pt.Z = p[i].Z;
                    lres.Add(pt);
                    pt = new Vector3D();
                }
                int lwidth = lres.Count;
                res = lres.ToArray();
                for (int i = lwidth; i < len; i++)
                {
                    if (MinOrMax == 0)
                    {
                        //  Find Max Point
                        if (res[i % lwidth].Z < p[i].Z)
                        {
                            res[i % lwidth].X = p[i].X;
                            res[i % lwidth].Y = p[i].Y;
                            res[i % lwidth].Z = p[i].Z;
                        }
                    }
                    else
                    {
                        //  Find Min Point
                        if (res[i % lwidth].Z > p[i].Z)
                        {
                            res[i % lwidth].X = p[i].X;
                            res[i % lwidth].Y = p[i].Y;
                            res[i % lwidth].Z = p[i].Z;
                        }
                    }
                }
                res = lres.ToArray();
            }
            else if (type / 2 == 2)
            {
                //  Single Segment Average Area
                for (int i = 0; i < len; i++)
                {
                    pt.X += p[i].X;
                    pt.Y += p[i].Y;
                    pt.Z += p[i].Z;
                }
                pt.X = pt.X / len;
                pt.Y = pt.Y / len;
                pt.Z = pt.Z / len;
                lres.Add(pt);
                res = lres.ToArray();
            }
            else
                res = null;
        }
        public void DistToPfromSurface(Surface3D surf, Vector3D[] p, ref double[] dist)
        {
            int len = p.Length;
            double absR = Math.Sqrt(surf.N.X * surf.N.X + surf.N.Y * surf.N.Y + surf.N.Z * surf.N.Z);
            for ( int i=0; i<len; i++)
                dist[i] = Math.Abs(surf.N.X * (p[i].X - surf.P.X) + surf.N.Y * (p[i].Y - surf.P.Y) + surf.N.Z * (p[i].Z - surf.P.Z))/ absR;
        }
        public void SignedDistToPfromSurface(Surface3D surf, Vector3D[] p, ref double[] dist)
        {
            int len = p.Length;
            double absR = Math.Sqrt(surf.N.X * surf.N.X + surf.N.Y * surf.N.Y + surf.N.Z * surf.N.Z);
            for (int i = 0; i < len; i++)
                dist[i] = (surf.N.X * (p[i].X - surf.P.X) + surf.N.Y * (p[i].Y - surf.P.Y) + surf.N.Z * (p[i].Z - surf.P.Z)) / absR;
        }
        public void DistToPfromSurface(Surface3D surf, double[][] p, ref double[] dist)
        {
            int len = p.Length;
            double absR = Math.Sqrt(surf.N.X * surf.N.X + surf.N.Y * surf.N.Y + surf.N.Z * surf.N.Z);
            for (int i = 0; i < len; i++)
                dist[i] = Math.Abs(surf.N.X * (p[i][0] - surf.P.X) + surf.N.Y * (p[i][1] - surf.P.Y) + surf.N.Z * (p[i][2] - surf.P.Z)) / absR;
        }
        public double DistToPfromSurface(Surface3D surf, Vector3D p)
        {
            double absR = Math.Sqrt(surf.N.X * surf.N.X + surf.N.Y * surf.N.Y + surf.N.Z * surf.N.Z);
            double dist = Math.Abs(surf.N.X * (p.X - surf.P.X) + surf.N.Y * (p.Y - surf.P.Y) + surf.N.Z * (p.Z - surf.P.Z)) / absR;
            return dist;
        }
        public double DistToPfromSurface(Surface3D surf, double[] p)
        {
            double absR = Math.Sqrt(surf.N.X * surf.N.X + surf.N.Y * surf.N.Y + surf.N.Z * surf.N.Z);
            double dist = Math.Abs(surf.N.X * (p[0] - surf.P.X) + surf.N.Y * (p[1] - surf.P.Y) + surf.N.Z * (p[2] - surf.P.Z)) / absR;
            return dist;
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  PIE CSH035 Project 전용
        /// </summary>
        /// <param name="Xidiffsrc"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xi0"></param>
        /// <param name="xW"></param>
        /// <param name="yi0"></param>
        /// <param name="yH"></param>
        /// <returns></returns>
        /// 
        //  width x height 의 Img Data 에서 xi ~ xf 사이에서 극대값 찾기
        //  xi0, xW, yi0, yH 주어지면, 
        //  xi0, xf0 = xi0 + xW 에서 LMSPDF 로 cx 구하고 xi1`, xf1 새로 설정
        //  반복해서 수렴시킨다.
        //  그런데 이때  yi0 yH 도 수렴중에 있는 경우가 있다.
        //          public void mcLMSProbabilityDensityFunction(Point2D[] wp, int length, ref Poly2nd p2nd)


        //int debugpeakCount = 0;
        public double[] mProfile = new double[11];
        public double[] mProfileRaw = new double[12];
        public double[] mProfileCoef = new double[11];
        //public double[][] mRatioXroughPeak = new double[50][];
        public double[] mRatio = new double[11];

        public int[] mIDrunning = new int[30];
        public void SetRatio(int m)
        {
            for (int i = 0; i < m; i++)
                //mRatio[i] = 9-Math.Abs(m / 2 - i);
                mRatio[i] = 1;
                //mRatio[i] = (20 - (m / 2 - i) * (m / 2 - i)) / 4.0;
                //mRatio[i] = (36 - Math.Pow(Math.Abs(m / 2 - i), 2.5)) / 4.0; //{ 2, 3, 4, 5, 4, 3, 2 };

        }
        

        public void CogOfShape(int id, ref byte[] src, int width, int height, ref double xia, ref double yia, double dia, ref double xDark, ref double xBright, ref double yDark, ref double yBright, int iIndex)
        {
            int length = width * height;
            double[] srcBk = new double[length];

            //  외곽부 평균치
            int avgOutN = 0;
            double avgOut = 0;
            int y_width = 0;
            for ( int y = 0; y< height; y++)
            {
                if (y > 2 && y < height - 3)
                    continue;
                y_width = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (x > 2 && x < width - 3)
                        continue;
                    avgOut += src[x + y_width];
                    avgOutN++;
                }
            }
            avgOut = avgOut / avgOutN;

            // 평균치 offset 제거
            for ( int i=0; i< length;i++ )
                srcBk[i] = src[i] - avgOut;

            //  중심에서 dia 내부에 대해서만 
            //  Dark 중심, Bright 중심
            double maxdist = dia * dia;
            double ydist2 = 0;
            double oldxia = xia;
            double oldyia = yia;

            for ( int itr = 0; itr<5; itr++ )
            {
                double ldarkV = 0;
                double ldarkX = 0;
                double ldarkY = 0;
                int ldarkN = 0;
                double lbrightV = 0;
                double lbrightX = 0;
                double lbrightY = 0;
                int lbrightN = 0;
                double lv = 0;
                double slope = 0;
                int threshold = 20;
                for (int y = 1; y < height-1; y++)
                {
                    ydist2 = (y - yia) * (y - yia);
                    y_width = y * width;
                    for (int x = 1; x < width-1; x++)
                    {
                        if ((x - xia) * (x - xia) + ydist2 > maxdist)
                            continue;
                        lv = srcBk[x + y_width] + srcBk[x+1 + y_width] + srcBk[x - 1 + y_width] + srcBk[x + y_width - width] + srcBk[x + y_width + width];  //  5pixel
                        //  기울기가 큰 경우 가중한다.
                        //slope = Math.Abs(srcBk[x + 1 + y_width] - srcBk[x - 1 + y_width] + srcBk[x + 2 + y_width] - srcBk[x - 2 + y_width]) + Math.Abs(srcBk[x + y_width + width] - srcBk[x + y_width - width] + srcBk[x + y_width + 2*width] - srcBk[x + y_width - 2*width]) ;
                        if (lv < -threshold)
                        {
                            lv = -lv - threshold;// Math.Sqrt(-lv - threshold + 0.1);
                            //lv = slope;// Math.Sqrt(-lv - threshold + 0.1);
                            ldarkV += lv;
                            ldarkX += lv * x;
                            ldarkY += lv * y;
                            ldarkN++;
                        }
                        else if (lv > threshold)
                        {
                            lv = lv - threshold;// Math.Sqrt(lv - threshold + 0.1);
                            //lv = slope;// Math.Sqrt(lv - threshold + 0.1);
                            lbrightV += lv;
                            lbrightX += lv * x;
                            lbrightY += lv * y;
                            lbrightN++;
                        }
                    }
                }
                xDark = ldarkX / ldarkV;
                yDark = ldarkY / ldarkV;
                xBright = lbrightX / lbrightV;
                yBright = lbrightY / lbrightV;

                xia = (xDark * ldarkN + xBright * lbrightN) / (ldarkN + lbrightN);
                yia = (yDark * ldarkN + yBright * lbrightN) / (ldarkN + lbrightN);
                if (oldxia == xia && oldyia == yia)
                    break;
                oldxia = xia;
                oldyia = yia;
            }
        }

        public double[][] mCircleMA_X = new double[5][];
        public double[][] mCircleMA_Y = new double[5][];
        public Point2d[] mCircleCOG = new Point2d[5];
        public void CogOfShape(int si, ref byte[] src, int width, int height, ref double xia, ref double yia, ref int xshift, ref int yshift, int iIndex)
        {
            int length = width * height;
            int[] xarray = new int[width];
            int[] yarray = new int[height];
            int[] xarrayCog = new int[width];
            int[] yarrayCog = new int[height];
            int y_width = 0;
            int yShift = 0;
            int xShift = 0;
            double sumx = 0;
            double sumy = 0;
            double sum = 0;
            int x = 0;
            int y = 0;

            int bottomX = 999999;
            int topX = -999999;
            int bottomV = 999999;
            int topV = -999999;

            try
            {
                int ymin = 3 - yShift;
                int ymax = height - 3 - yShift;
                int xmin = 3 - xShift;
                int xmax = width - 3 - xShift;

                for (y = ymin; y < ymax; y++)
                {
                    y_width = y * width;
                    for (x = 0; x < width; x++)
                        xarray[x] += src[x + y_width];
                }

                for (x = 0; x < width; x++)
                {
                    if (bottomX > xarray[x])
                        bottomX = xarray[x];
                    if (topX < xarray[x])
                        topX = xarray[x];
                }

                bottomX += (topX - bottomX) / 4;

                for (x = 0; x < width; x++)
                    xarray[x] -= bottomX;

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                //for (x = 0; x < width; x++)
                //    xarray[x] = (int)(xarray[x] / Math.Sqrt(Math.Abs(xarray[x])));
                //////////////////////////////////////////////////////////////////////////////////////////////////////

                for (y = 0; y < height; y++)
                {
                    y_width = y * width;
                    for (x = xmin; x < xmax; x++)
                        yarray[y] += src[x + y_width];
                }

                bottomV = 999999;
                topV = -999999;
                for (y = 0; y < height; y++)
                {
                    if (bottomV > yarray[y])
                        bottomV = yarray[y];
                    if (topV < yarray[y])
                        topV = yarray[y];
                }

                bottomV += (topV - bottomV) / 4;

                for (y = 0; y < height; y++)
                    yarray[y] -= bottomV;

            }
            catch (Exception e)
            {
                return;
            }
            //  Moving Average 생성 및 저장
            sumx = 0;
            sumy = 0;
            sum = 0;

            int xi = 0 ;
            int xf = 0;

            int yi = 0;
            int yf = 0;


            if (iIndex == 0 )
            {
                //  최초에 밝은 영역이 너무 작아서 마크가 오히려 주변보다 어두운 경우는 무게중심을 다르게 잡아줘야 한다.
                //  최초에 중앙부 영역의 평균밝기가 주변부 평균밝기보다 어두운 경우는 결과값을 반전시켜서 무게중심을 잡고, 반전시키지 않은 결과값을 기준으로 그 주변 영역의 패턴을 잡아야 한다.
                int outterX = 0;
                int innerX = 0;
                int outterY = 0;
                int innerY = 0;
                int width25 = width / 4;
                int width75 = (3 * width) / 4;
                int height25 = height / 4;
                int height75 = (3 * height) / 4;

                for (x = 0; x < width; x++)
                    if (x < width25 || x >= width75)
                        outterX += xarray[x];
                    else
                        innerX += xarray[x];

                int offset = 0;

                if (innerX < 0.8 * outterX) //  내측이 더 어두운 경우
                {
                    offset = (topX - bottomX) / 2 + 5;
                    for (x = 0; x < width; x++)
                    {
                        xarrayCog[x] = offset - xarray[x];
                    }
                }else if (innerX > 1.25 * outterX)
                    for (x = 0; x < width; x++)
                        xarrayCog[x] = xarray[x];
                else
                {
                    for (x = 0; x < width; x++)
                        xarrayCog[x] = xarray[x];

                    //  억지로 영역 중심에 무게중심이 잡히도록 조정
                    xarrayCog[width / 2 + 1] += topX - bottomX;
                    xarrayCog[width / 2] += topX - bottomX;
                    xarrayCog[width / 2 - 1] += topX - bottomX;
                }

                for (y = 0; y < height; y++)
                    yarrayCog[y] = yarray[y];

                //  다음은 초기 무게중심 결정하는 과정
                //  W - 12
                if (mCircleCOG[si] == null)
                    mCircleCOG[si] = new Point2d(width / 2.0, height / 2.0);
                long lbin = 0;

                ////////for (x = 0; x < width; x++)
                ////////{
                ////////    lbin = xarrayCog[x] * xarrayCog[x];
                ////////    sumx += lbin * x;
                ////////    sum += lbin;
                ////////}
                ////////mCircleCOG[si].X = sumx / sum;
                ////////sum = 0;
                ////////for (y = 0; y < height; y++)
                ////////{
                ////////    lbin = yarrayCog[y] * yarrayCog[y];
                ////////    sumy += lbin * y;
                ////////    sum +=  lbin;
                ////////}
                ////////mCircleCOG[si].Y = sumy / sum - 1;

                for ( int itrn = 0; itrn < 2; itrn ++)
                {
                    sumx = 0;
                    sum = 0;
                    int endloop = (int)(mCircleCOG[si].X + (width / 2 - 4));
                    for (x = (int)(mCircleCOG[si].X - (width / 2 - 4)); x <= endloop; x++)
                    {
                        if (x < 0 || x >= width) continue;
                        lbin = xarrayCog[x] * xarrayCog[x]; 
                        sumx += lbin * x;
                        sum += lbin;
                    }
                    mCircleCOG[si].X = sumx / sum;

                    sumy = 0;
                    sum = 0;
                    endloop = (int)(mCircleCOG[si].Y + (height / 2 - 4));
                    for (y = (int)(mCircleCOG[si].Y - (height / 2 - 4)); y <= endloop; y++)
                    {
                        if (y < 0 || y >= height) continue;
                        lbin = yarrayCog[y]* yarrayCog[y];
                        sumy += lbin * y;
                        sum += lbin;
                    }
                    //  1차 계산치 주변에서의 Bin 값에의해 COG 가 결정된다.
                    mCircleCOG[si].Y = sumy / sum;
                }
                //  무게중심 결정 완료


                //  무게중심 주변 수십pixel 을 기준 Profile (mCircleMA_ㅌ, mCircleMA_Y)로 삼고, 이후 이 기준 Profile대비하여 현재 Profile 이 얼마나 이동했는지를 비교하여 이동량을 결정한다.
                //  mCircleMA_X, mCircleMA_Y 를 모델을 만들때 저장하고 이후 측정시에는 모델에 저장된 정보와 비교만 하는 것으로 하면 반복성은 잘 나온다.
                //  모델과 차이가 크면 측정치의 산포가 커질 수 있다.
                //  기준 Profile 을 모델에 저장하도록 하면

                xi = (int)(mCircleCOG[si].X) - width / 2 + 7;
                xf = (int)(mCircleCOG[si].X) + width / 2 - 7;
                if (xi < 3)
                    xi = 3;
                if (xf > width - 4)
                    xf = width - 4;


                mCircleMA_X[si] = new double[xf - xi];

                for (x = xi; x < xf; x++)
                    if (x > 0 && x < width - 1)
                        mCircleMA_X[si][x - xi] = (xarray[x - 1] + xarray[x] + xarray[x + 1]);

                yi = (int)(mCircleCOG[si].Y) - height / 2 + 7;
                yf = (int)(mCircleCOG[si].Y) + height / 2 - 7;
                if (yi < 3)
                    yi = 3;
                if (yf > height - 4)
                    yf = height - 4;

                mCircleMA_Y[si] = new double[yf - yi];

                for (y = yi; y < yf; y++)
                    if (y > 0 && y < width - 1)
                        mCircleMA_Y[si][y - yi] = (yarray[y - 1] + yarray[y] + yarray[y + 1]) ;

                mCircleCOG[si].X -= xi;
                mCircleCOG[si].Y -= yi;
            }
            //  Moving Average 와 Convolution 계산


            int xPi = 0;
            int yPi = 0;

            // X Position
            double[,] m33 = new double[3, 3];
            double[,] invm = new double[3, 3];
            double[] m31 = new double[3];
            double[] res = new double[3];
            double x_2 = 0;
            int i = 0;
            int j = 0;

            ref double[] ma_X = ref mCircleMA_X[si];
            ref double[] ma_Y = ref mCircleMA_Y[si];
            int convLenX = ma_X.Length;
            int convLenY = ma_Y.Length;
            int imax = width - convLenX;
            int jmax = height - convLenY;

            int[] xIndex = new int[imax];
            double[] xconv = new double[imax];    //  11개면 충분
            double[] xconv2 = new double[imax];    //  11개면 충분

            int[] yIndex = new int[jmax];
            double[] yconv = new double[jmax];    //  11개면 충분
            double[] yconv2 = new double[jmax];    //  11개면 충분


            //  13개 위치에 대해서 Conv 값을 계산한다.

            for (i = 0; i < imax; i++)
            {
                xIndex[i] = i;
                for (j = 0; j < convLenX; j++)
                    //xconv[i] += xarray[i + j] * mCircleMA_X[si][j];
                    xconv[i] += xarray[i + j] * ma_X[j];

                xconv2[i] = xconv[i];

            }
            for (i = 0; i < jmax; i++)
            {
                yIndex[i] = i;
                for (j = 0; j < convLenY; j++)
                    //yconv[i] += yarray[i + j] * mCircleMA_Y[si][j];
                    yconv[i] += yarray[i + j] * ma_Y[j];

                yconv2[i] = yconv[i];
            }

            //Array.Copy(xconv, xconv2, imax);
            //Array.Copy(yconv, yconv2, jmax);
            Array.Sort(xconv2, xIndex);
            Array.Sort(yconv2, yIndex);

            xPi = xIndex[imax - 1];   //  Conv 값이 최대인 지점을 좌표 0 ~ 12 범위에서만 찾는다.
            yPi = yIndex[jmax - 1];

            for (i = 0; i < 7; i++)
            {
                // 극대값 주변 7개 데이터로 Peak 찾는다.
                x = xPi - 3 + i;
                if (x < 0 || x >= imax || xconv[x] == 0)
                    continue;
                x_2 = x * x;

                m33[0, 0] += x_2 * x_2;
                m33[0, 1] += x_2 * x;
                m33[0, 2] += x_2;

                //m33[1, 0] += x_2 * x;
                //m33[1, 1] += x_2;
                //m33[1, 2] += x;

                //m33[2, 0] += x_2;
                m33[2, 1] += x;
                m33[2, 2] += 1;

                m31[0] += x_2 * xconv[x];
                m31[1] += x * xconv[x];
                m31[2] += xconv[x];
            }
            m33[1, 0] = m33[0, 1];
            m33[1, 1] = m33[0, 2];
            m33[1, 2] = m33[2, 1];
            m33[2, 0] = m33[0, 2];

            //invm = MatrixInverse(m33, 3);
            invm = MatrixInverse3x3(m33);
            MatrixCross(ref invm, ref m31, ref res, 3);

            xia = -res[1] / (2 * res[0]);

            double asym1 = 0;
            double asym2 = 0;
            double xia2 = 0;

            //  한쪽으로 값이 치우쳐있을 것이므로 그에 대한 미세한 보상을 수행한다.
            if (xPi < (imax-3) && xPi > 2)
            {
                if (xconv[xPi - 3] < xconv[xPi + 3])
                {
                    if (xPi + 4 < imax)
                    {
                        asym1 = -xconv[xPi - 3] + xconv[xPi + 3];
                        asym2 = xconv[xPi - 3] - xconv[xPi + 4];

                        x = xPi + 4;
                    }
                    else
                    {
                        asym1 = -xconv[xPi - 3] + xconv[xPi + 3];
                        asym2 = asym1;

                        x = xPi + 3;
                    }
                    x_2 = x * x;


                }
                else //if (xconv[xPi - 3] > xconv[xPi + 3])
                {
                    if (xPi - 4 >= 0)
                    {
                        asym1 = xconv[xPi - 3] - xconv[xPi + 3];
                        asym2 = -xconv[xPi - 4] + xconv[xPi + 3];

                        x = xPi - 4;
                    }
                    else
                    {
                        asym1 = xconv[xPi - 3] - xconv[xPi + 3];
                        asym2 = asym1;

                        x = xPi - 3;
                    }
                    x_2 = x * x;
                }
                m33[0, 0] += x_2 * x_2;
                m33[0, 1] += x_2 * x;
                m33[0, 2] += x_2;

                //m33[1, 0] += x_2 * x;
                //m33[1, 1] += x_2;
                //m33[1, 2] += x;

                //m33[2, 0] += x_2;
                m33[2, 1] += x;
                m33[2, 2] += 1;

                m33[1, 0] = m33[0, 1];
                m33[1, 1] = m33[0, 2];
                m33[1, 2] = m33[2, 1];
                m33[2, 0] = m33[0, 2];

                m31[0] += x_2 * xconv[x];
                m31[1] += x * xconv[x];
                m31[2] += xconv[x];

                //invm = MatrixInverse(m33, 3);
                invm = MatrixInverse3x3(m33);
                MatrixCross(ref invm, ref m31, ref res, 3);

                //InverseU(ref m33, 3);
                //MatrixCross(ref m33, ref m31, ref res, 3);

                xia2 = -res[1] / (2 * res[0]);

                xia = (asym2 * xia + asym1 * xia2) / (asym1 + asym2);

            }


            // Y Position
            m33 = new double[3, 3];
            m31 = new double[3];
            res = new double[3];
            x_2 = 0;

            for (i = 0; i < 7; i++)
            {
                x = yPi - 3 + i;
                if (x < 0 || x >= jmax || yconv[x] == 0)
                    continue;
                x_2 = x * x;

                m33[0, 0] += x_2 * x_2;
                m33[0, 1] += x_2 * x;
                m33[0, 2] += x_2;

                //m33[1, 0] += x_2 * x;
                //m33[1, 1] += x_2;
                //m33[1, 2] += x;

                //m33[2, 0] += x_2;
                m33[2, 1] += x;

                m31[0] += x_2 * yconv[x];
                m31[1] += x * yconv[x];
                m31[2] += yconv[x];

                m33[2, 2] += 1;
            }
            m33[1, 0] = m33[0, 1];
            m33[1, 1] = m33[0, 2];
            m33[1, 2] = m33[2, 1];
            m33[2, 0] = m33[0, 2];

            //invm = MatrixInverse(m33, 3);
            invm = MatrixInverse3x3(m33);
            MatrixCross(ref invm, ref m31, ref res, 3);

            yia = -res[1] / (2 * res[0]);


            asym1 = 0;
            asym2 = 0;
            double yia2 = 0;
            //  한쪽으로 값이 치우쳐있을 것이므로 그에 대한 미세한 보상을 수행한다.
            if ( yPi < (jmax-3) && yPi > 2)
            {
                if (yconv[yPi - 3] < yconv[yPi + 3])
                {
                    if (yPi + 4 < jmax)
                    {
                        asym1 = -yconv[yPi - 3] + yconv[yPi + 3];
                        asym2 = yconv[yPi - 3] - yconv[yPi + 4];
                        x = yPi + 4;
                    }
                    else
                    {
                        asym1 = -yconv[yPi - 3] + yconv[yPi + 3];
                        asym2 = asym1;
                        x = yPi + 3;
                    }
                    x_2 = x * x;
                }
                else //if (xconv[xPi - 3] > xconv[xPi + 3])
                {
                    if (yPi - 4 >= 0)
                    {
                        asym1 = yconv[yPi - 3] - yconv[yPi + 3];
                        asym2 = -yconv[yPi - 4] + yconv[yPi + 3];

                        x = yPi - 4;
                    }
                    else
                    {
                        asym1 = yconv[yPi - 3] - yconv[yPi + 3];
                        asym2 = asym1;

                        x = yPi - 3;
                    }

                    x_2 = x * x;
                }
                m33[0, 0] += x_2 * x_2;
                m33[0, 1] += x_2 * x;
                m33[0, 2] += x_2;

                //m33[1, 0] += x_2 * x;
                //m33[1, 1] += x_2;
                //m33[1, 2] += x;

                //m33[2, 0] += x_2;
                m33[2, 1] += x;
                m33[2, 2] += 1;

                m33[1, 0] = m33[0, 1];
                m33[1, 1] = m33[0, 2];
                m33[1, 2] = m33[2, 1];
                m33[2, 0] = m33[0, 2];

                m31[0] += x_2 * yconv[x];
                m31[1] += x * yconv[x];
                m31[2] += yconv[x];

                //invm = MatrixInverse(m33, 3);
                invm = MatrixInverse3x3(m33);
                MatrixCross(ref invm, ref m31, ref res, 3);

                //InverseU(ref m33, 3);
                //MatrixCross(ref m33, ref m31, ref res, 3);

                yia2 = -res[1] / (2 * res[0]);

                yia = (asym2 * yia + asym1 * yia2) / (asym1 + asym2);
            }

            //if (si == 0)
            //{
            //    StreamWriter wr = new StreamWriter("y" + iIndex.ToString() + "_" + si.ToString() + ".csv");
            //    wr.WriteLine(xia.ToString() + "," + yia.ToString());
            //    for (y = 0; y < height; y++)
            //        wr.WriteLine(yarray[y].ToString());
            //    wr.Close();

            //}


            //  Y 에서 계산된 값이 7 보다 작으면 그만큼 X 추출배열을 앞으로 이동한다.
            //  Y 에서 계산된 값이 7 보다 크면 그만큼 X 추출배열을 뒤로 이동한다.
            //  X 에서 계산된 값이 7 보다 작으면 그만큼 Y 추출배열을 앞으로 이동한다.
            //  X 에서 계산된 값이 7 보다 크면 그만큼 Y 추출배열을 뒤로 이동한다.
            //xShift = (int)(imax / 2.0 - xia + 0.5);
            //yShift = (int)(jmax / 2.0 - yia + 0.5);
            //itrCnt++;

            //if ((Math.Abs(yShift) > 0 || Math.Abs(xShift) > 0) && itrCnt < 2)
            //    goto ShiftSampling;
            if ( iIndex > 0)
            {
                if (yia < -5)
                    yia = jmax-1;
                if (yia > jmax + 5)
                    yia = 1;

                //if (xia < 3.5)
                if (xia < 1.5)
                {
                    //xshift = (int)(xia - 4);    //  앞쪽에 치우친 경우 전체 데이터를 앞쪽에서 취해서 다시 계산할 수 있도록 ROI 를 앞으로 이동시킨다.
                    xshift = (int)(xia - 2);    //  앞쪽에 치우친 경우 전체 데이터를 앞쪽에서 취해서 다시 계산할 수 있도록 ROI 를 조금만 앞으로 이동시킨다.
                }
                else if (xia > imax - 4)    //  뒤쪽에 치우진 경우는 뒤쪽으로 많이 이동해봐야 한다.
                {
                    xshift = (int)(xia - (imax - 6));
                }
                //if (yia < 3.5)
                if (yia < 1.5)
                {
                    //yshift = (int)(yia - 4);    //  앞쪽에 치우친 경우 전체 데이터를 앞쪽에서 취해서 다시 계산할 수 있도록 ROI 를 앞으로 이동시킨다.
                    yshift = (int)(yia - 2);    //  앞쪽에 치우친 경우 전체 데이터를 앞쪽에서 취해서 다시 계산할 수 있도록 ROI 를 조금만 앞으로 이동시킨다.
                }
                else if (yia > jmax - 4)
                {
                    yshift = (int)(yia - (jmax - 6));
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            xia = mCircleCOG[si].X + xia;   //  모델 생성시에 최종적으로 무게중심으로 저장해줘야 하는 값은 바로 여기의 xia, yia 와 mCircleMA_X[], mCircleMA_X[] 이다.
            yia = mCircleCOG[si].Y + yia;   //  상기 내용을 모델생성시 반영해야 한다. 모델 생성 코드 수정 시 적용할 것.
        }

        public int[] mPeakOrder = new int[24];   //                mPeakOrder[i] = new int[24]; // 6 *  [ 0 - 1 - 2 - 3 : L - R - T - B ]

        public double[] ConvergePeakX2(int id, ref int[] Xidiffsrc, int width, int height, double xia, double yia, int xW, int yH, ref byte updown, ref int peaktype, int iIndex = 0)
        {
            //  원본 영상의 크기 width, height 로서 ROI 범위의 조각영상인 것을 전제로 한다.
            //  xi0 : 경계가 있을 것으로 예상되는 BOX 영역의 좌상단 X 좌표
            //  yi0 : 경계가 있을 것으로 예상되는 BOX 영역의 좌상단 Y 좌표
            //  xW  : BOX 영역의 폭
            //  yH  : BOX 영역의 높이
            //
            //  계산된 경계좌표를 중심으로 BOX 영역의 위치를 재설정하고 재설정된 BOX 영역에서 다시 경계좌표를 계산하기를 반복한다.
            //  반복하여, 직전 반복결과와 비교하여 결과의 변화가 0.001 이하이면 반복 종료, 최대반복은 5회까지 허용
            double res1st = 0;
            double res2nd = 0;

            int xi0 = (int)xia;
            int yi0 = (int)yia;

            int fxi0 = xi0;
            double oldf = fxi0;
            double sumXY = 0;
            double sumY = 0;
            double sumXY2 = 0;
            double sumY2 = 0;

            int kLength = (int)(width - xia) ;
            if (kLength > width / 2 && xia < width / 4)
                kLength = width / 2 + 1;

            int edgeDir = id % 4;

            if (edgeDir > 1) //  Y 축의 경우 2Line 에 대비
                kLength += 4;

            double[] roughPeak = new double[kLength + 6];
            double[] roughPeakBk = new double[kLength + 6];
            int[] peakIndex = new int[kLength + 6];
            double[] effPeak = new double[kLength];
            int[] effIndex = new int[kLength];
            double[] intgPeak = new double[kLength + 6];

            //double[] ratio = new double[xW];

            //double[] sumxx = new double[kLength];
            //double[] sumx = new double[kLength];

            //int debug = 0;
            int i = 0;
            if (xi0 < 0) xi0 = 0;
            if (yi0 < 0) yi0 = 0;
            int potentialType = 0;
            double ry = yia - yi0;
            int xi0_i = 0;
            int pIndex = 0;
            double peak = -99999;

            int npIndex = 0;
            double npeak = 99999;

            int incCnt = 0;
            int repeatCnt = 0;

            bool negPeak = false;
            int maxLength = 0;


            if (peaktype % 100 == 2)    
                negPeak = true;

            int inversion = (width + 1) / 2;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  반복성향상 전후 비교검토용
            //if ( edgeDir > 1) //   향상 전 기준치
            //    yH = yH / 2;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                        //  Iteration 하기 전에 절대 Peak 위치를 찾아서 거기서부터 출발해야 한다. 안그러면 Peak 가 초기 검색범위에 없어서 실패할 수 있다.
                while (repeatCnt < 6)
                {
                    for (i = 0; i < kLength; i++)
                    {
                        xi0_i = (int)(xi0 + i);
                        peakIndex[i] = xi0_i;
                        for (uint j = 0; j < yH; j++)
                        {
                            if (j + yi0 >= height - 1)
                                break;

                            roughPeak[i] += (1 - ry) * Xidiffsrc[xi0_i + (j + yi0) * width] + ry * Xidiffsrc[xi0_i + (j + yi0 + 1) * width];
                        }

                        if  ( xi0 < 5 && xi0_i >= inversion)    //  우측 경계
                            roughPeak[i] = -roughPeak[i];

                        else if (xi0> 5 && xi0_i < inversion)   //   좌측 경계
                            roughPeak[i] = -roughPeak[i];

                        roughPeakBk[i] = roughPeak[i];
                        if (edgeDir>1)
                        {
                            if (xi0 < 10)
                            {
                                if (i > 1 && i < kLength / 2 + 2)
                                {
                                    if (peak < roughPeakBk[i])
                                    {
                                        pIndex = i;
                                        peak = roughPeakBk[i];  //   최대값 및 최대값일 때 Index
                                    }
                                    if (npeak > roughPeakBk[i])  //peaktype == 3 이면 정방향 Peak 를 찾아야 한다.
                                    {
                                        npIndex = i;
                                        npeak = roughPeakBk[i];  //   최소값 및 최소값일 때 Index
                                    }
                                }
                            }
                            else
                            {
                                if (i > 1 && i < kLength / 2 + 4)
                                {
                                    if (peak < roughPeakBk[i])
                                    {
                                        pIndex = i;
                                        peak = roughPeakBk[i];  //   최대값 및 최대값일 때 Index
                                    }
                                    if (npeak > roughPeakBk[i])  //peaktype == 3 이면 정방향 Peak 를 찾아야 한다.
                                    {
                                        npIndex = i;
                                        npeak = roughPeakBk[i];  //   최소값 및 최소값일 때 Index
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (i > 1 && i < kLength - 2)
                            {
                                if (peak < roughPeakBk[i])
                                {
                                    pIndex = i;
                                    peak = roughPeakBk[i];
                                }
                                if (npeak > roughPeakBk[i])  //peaktype == 3 이면 정방향 Peak 를 찾아야 한다.
                                {
                                    npIndex = i;
                                    npeak = roughPeakBk[i];
                                }
                            }
                        }
                    }
                    //if (edgeDir < 2)
                    //{
                    //    if (((peak * peak < npeak * npeak && (peaktype % 100) != 3) || peaktype % 100 == 2) && npIndex > 6) //  peaktype % 100 == 2 이면 기울기가 음수 Peak 인 곳을 찾는다.
                    //    {
                    //        pIndex = npIndex;
                    //        peak = npeak;
                    //    }
                    //}

                    //if ( (( Math.Abs(peak) < Math.Abs(npeak) && (peaktype % 100) != 3 ) || peaktype % 100 == 2) && npIndex > 6) //  peaktype % 100 == 2 이면 기울기가 음수 Peak 인 곳을 찾는다.
                    //if ((( peak*peak < npeak*npeak && (peaktype % 100) != 3) || peaktype % 100 == 2) && npIndex > 6) //  peaktype % 100 == 2 이면 기울기가 음수 Peak 인 곳을 찾는다.
                    //{
                    //    pIndex = npIndex;
                    //    peak = npeak;
                    //}
                    if (pIndex > kLength - 4 && incCnt < 6)
                    {
                        kLength++;
                        incCnt++;
                    }
                    if (pIndex < 6)
                    {
                        if (xi0 == 0)
                            break;
                        xi0--;
                        repeatCnt++;
                        peak = -99999;
                        roughPeak = new double[kLength + 6];
                        roughPeakBk = new double[kLength + 6];
                        peakIndex = new int[kLength + 6];
                    }
                    else
                    {
                        break;
                    }
                }

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  가장 큰 값의 Index 가 양끝단 0,1,2 에 있거나 kLength-3, kLength-2, kLength-1 에 있는 경우는 범위를 변경시켜야 한다.

                //  2개의 봉우리가 있는 경우 6이상의 앞쪽 봉우리를 선택한다.
                //if (pIndex != npIndex && iIndex == 0)
                //double[] cproughPeakBk = new double[roughPeakBk.Length];
                //int[] cppeakIndex = new int[roughPeakBk.Length];
                //Array.Copy(roughPeakBk, 0, cproughPeakBk, 0, roughPeakBk.Length);
                //Array.Copy(peakIndex, 0, cppeakIndex, 0, roughPeakBk.Length);
                //Array.Sort(cproughPeakBk, cppeakIndex);
                //if ( iIndex == 0)
                //{
                //    if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 2]) > 6)    // 2번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cproughPeakBk[roughPeakBk.Length - 1] / cproughPeakBk[roughPeakBk.Length - 2] < 1.2 && cppeakIndex[roughPeakBk.Length - 2] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            //  그중에서도 2번쨰 peak 가 첫째 Peak 의 90% 이상이고 2번째 Peak 가 앞쪽이고 index 가 6 이상이면 2번째 Peak 를 써라
                //            if (cppeakIndex[roughPeakBk.Length - 2] > 7)
                //            {
                //                pIndex = cppeakIndex[roughPeakBk.Length - 2] - (int)xia;
                //                mPeakOrder[id] = 1; //  앞쪽 Peak 를 써라
                //            }
                //        }
                //    }else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 3]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cproughPeakBk[roughPeakBk.Length - 1] / cproughPeakBk[roughPeakBk.Length - 3] < 1.2 && cppeakIndex[roughPeakBk.Length - 3] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            //  그중에서도 3번쨰 peak 가 첫째 Peak 의 85% 이상이고 3번째 Peak 가 앞쪽이고 index 가 6 이상이면 3번째 Peak 를 써라
                //            if (cppeakIndex[roughPeakBk.Length - 3] > 7)
                //            {
                //                pIndex = cppeakIndex[roughPeakBk.Length - 3] - (int)xia;
                //                mPeakOrder[id] = 1; //  앞쪽 Peak 를 써라 
                //            }
                //        }
                //    }
                //}
                //else if (iIndex > 0 && mPeakOrder[id] == 1)
                //{
                //    if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 2]) > 6)    // 2번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if ( cppeakIndex[roughPeakBk.Length - 2] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 2] - (int)xia;
                //        }
                //    }
                //    else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 3]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if ( cppeakIndex[roughPeakBk.Length - 3] < cppeakIndex[roughPeakBk.Length - 1] )
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 3] - (int)xia;
                //        }
                //    }
                //    else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 4]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cppeakIndex[roughPeakBk.Length - 4] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 4] - (int)xia;
                //        }
                //    }
                //}
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                int effStart = peakIndex[3];
                int effEnd = peakIndex[peakIndex.Length - 4];
                i = peakIndex.Length - 4;
                while (effEnd == 0)
                {
                    effEnd = peakIndex[--i];
                    if (i == 10)
                        break;
                }

                //if (edgeDir > 1) //  Y 축의 경우 2Line 에 대비   //  Debugging 용
                //    pIndex = pIndex;

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///
                ///     바로 위까지해서 peak Index 를 구했다.
                ///     
                //  Peak 전후 9개배열만 활용해서 Rising Edge / Falling Edge / Rising Sharp Edge / Falling Sharp Edge 인지 구분한다.
                //  구분하여 peak 주변 9개 배열 값의 최소값이 0 이 되도록 Offset 처리한다. 이로써 Peak 의 중심좌표가 가장 정확하게 추출된다.
                int iFrom = (pIndex - 5 < 0 ? 0 : pIndex - 5);
                int iTo = (kLength - pIndex < 6 ? kLength - 1 : pIndex + 5);
                int effLength = iTo - iFrom + 1;
                effPeak = new double[effLength];
                effIndex = new int[effLength];

                //  roughPeak 에서 첫번째  +peak : 4~5번째, 
                //  roughPeak 에서 두번째  -peak : 11~12번째, 
                //  roughPeak 에서 세번째  +peak : 20~21째 -> Bottom 에 속함.

                potentialType = peaktype;

                // 최대 최소를 구할 때는 양끝단 3개 데이터를 버린 데이터셋에서 최대최소를 구한다.
                // 양단 3pxiel 이내에서 Peak 가 있는 경우는 불연속 발생으로 결과에 튀는 값 초래하므로 양단 3pixel 에서는 peak 를 찾을 필요 없음.
                //double minValue = effPeak[0];
                //double maxValue = effPeak[effPeak_Length_1];

                //  아래는 연산 범위내에 음수가 들어가지 않도록 offset 을 만들어준다
                //  이런 방식 말고, 연산범위내에 음수는 0 으로 처리하거나
                //  연산범위내에서 음수는  0.1 ~ 0.2 을 곱해서 비중이 아주 작도록 처리하는 방법이 있다.
                //double minValue = effPeak[1];   
                //double maxValue = effPeak[effPeak_Length_1-1];
                if (updown == 255)  //  방향성이 미정인 경우
                {
                    //if ((Math.Abs(minValue) > maxValue && peaktype == 0) || peaktype == 2)
                    if ( peaktype == 0 || peaktype == 2 )
                    {
                        // Peak 가 뭔가 이상해서 부호 반전 필요한 경우
                        potentialType = 2;
                        updown = 0;
                    }
                    else
                    {
                        potentialType = 3;
                        updown = 1;         //  정상적인 Peak 인 경우
                    }
                    peaktype = potentialType;
                }
                else
                {
                    if (updown == 0 || peaktype == 2)
                    {
                        // 부호 반전 필요.
                        potentialType = 2;
                    }
                    else
                    {
                        potentialType = 3;
                    }
                    peaktype = potentialType;
                }
                Array.Copy(roughPeakBk, iFrom, effPeak, 0, effLength);
                Array.Copy(peakIndex,   iFrom, effIndex, 0, effLength);
                Array.Sort(effPeak, effIndex);  //  더 빠른 함수로 변경 필요  231116
                fxi0 = effIndex[effIndex.Length - 1];
            }
            catch (Exception e)
            {
                //MessageBox.Show("ConvergePeakX() 1>> \r\n" + e.ToString());
                //return fxi0;
            }

            //  xW 의 자동조정은, xW 를 조절할 수 있는 분해능이 매우 낮아서 오히려 역효과 발생.
            //  상황에 따라 고정값 적용이 적합. 즉 Focusing 수준에 따라서 2가지 또는 3가지 값중 선택하는 방식은 가능할 것 같음.
            //  실험적으로 xW = 7 일때 반복성이 가장 좋은 것으로 나타남.

            int i0 = -3;// - (xW - 1) / 2;
            int ie = -i0 + 1;
            int icur = 0;
            sumXY = 0;
            sumY = 0;

            int newi = 0;
            double ratioXroughPeak = 0;
            maxLength = roughPeakBk.Length - 1;
            for (newi = i0; newi < ie; newi++)
            {
                icur = newi + fxi0 - (int)xi0;
                if (icur < 0) continue;
                if (icur >= maxLength) break;
                //ratioXroughPeak = ratio[newi - i0] * roughPeakBk[icur];
                ratioXroughPeak = roughPeakBk[icur];
                sumXY += ratioXroughPeak * (newi + fxi0);
                sumY  += ratioXroughPeak;
            }
            res1st = sumXY / (double)sumY;

            if (res1st - xi0 < 0 ) //  극히 비정상인 경우 두번째 Peak 를 활용한다.
                res1st = peakIndex[kLength - 2]; //  Peak 좌표

            double simpleRes = res1st;
            double oldres = res1st;
            double pY = 0;
            double err = 999;
            double err_1 = 999;

            uint itr = 0;
            double[] errMem = new double[10];
            int errMemCnt = 0;
            double roughpeak_icur = 0;
            double roughPeak_icur_1 = 0;

            try
            {
                for (itr = 0; itr < 25; itr++)
                {
                    int irx = (int)res1st;
                    double rx = res1st - irx;
                    sumXY = 0;
                    sumY = 0;
                    sumXY2 = 0;
                    sumY2 = 0;
                    int i_i0 = 0;
                    for (newi = i0; newi < ie; newi++)
                    {
                        icur = newi + irx - (int)xi0;
                        if (icur < 0) continue;
                        if (icur >= kLength) continue;

                        i_i0 = newi - i0;
                        roughpeak_icur = roughPeakBk[icur];
                        roughPeak_icur_1 = roughPeakBk[icur + 1];

                        if (roughpeak_icur < 0)
                            roughpeak_icur = roughpeak_icur / 5;
                        if (roughPeak_icur_1 < 0)
                            roughPeak_icur_1 = roughPeak_icur_1 / 5;

                        if (rx > 0)
                        {
                            if (newi + irx + 1 - (int)xi0 < kLength)
                                pY/*[i_i0]*/ = (1 - rx) * roughpeak_icur + rx * roughPeak_icur_1;
                            else
                                pY/*[i_i0]*/ = roughpeak_icur;   //  다음 값이 없으면 같은 값으로 가정.

                        }
                        else
                        {
                            pY = roughpeak_icur;
                        }

                        sumXY += (newi + res1st) * pY;
                        sumY += pY;
                    }

                    res1st = sumXY / (double)sumY;
                    if (sumY == 0)
                    {
                        res1st = fxi0;
                        break;
                    }
                    err = oldres - res1st;
                    err = err < 0 ? -err : err;
                    if (err < 0.0002)   //  0.0001 일때 반복성 더 나쁘다.
                        break;

                    if ( err > 5 * err_1 )
                    {
                        res1st = errMem[(errMemCnt - 1) % 6];
                        break;
                    }
                    res1st = res1st + (res1st - oldres) / 4;// 3.5 ; //  최대한 빨리 수렴하도록 접근속도를 변경함. 평균적으로 가장 빠르게 수렴시키는 최적값이 정확히 얼마인지는 알 수 없음.
                    err_1 = err;
                    oldres = res1st;
                    errMem[errMemCnt] = res1st;
                    errMemCnt = (errMemCnt + 1) % 6;
                }
                if(itr == 25)
                {
                    res1st = 0;
                    int emiCnt = 0;
                    for ( int emi = 0; emi < 6; emi++)
                    {
                        if (errMem[emi] == 0)
                            break;
                        res1st += errMem[emi];
                        emiCnt++;
                    }
                    res1st = res1st / emiCnt;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("ConvergePeakX() 2>>\r\n" + e.ToString());
                //return simpleRes;
            }
            if ( (xia < 3 && (res1st - xi0) > width/2) || res1st < 4 || (res1st - xi0) > width )
                res1st = fxi0;

            double[] res = new double[2];
            res[0] = res1st;

            if ( edgeDir >1)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  반복성향상 전후 비교검토용
                //  res2nd
                res2nd = (int)res1st + 7;
                if (edgeDir == 3 ) 
                    res2nd = (int)res1st - 7;

                simpleRes = res2nd;
                oldres = res2nd;

                pY = 0;
                err = 999;
                err_1 = 999;

                itr = 0;
                errMem = new double[10];
                errMemCnt = 0;
                roughpeak_icur = 0;
                roughPeak_icur_1 = 0;
                i0 = -3;
                ie = 4;
                try
                {
                    for (itr = 0; itr < 25; itr++)
                    {
                        int irx = (int)res2nd;
                        double rx = res2nd - irx;
                        sumXY = 0;
                        sumY = 0;
                        sumXY2 = 0;
                        sumY2 = 0;
                        int i_i0 = 0;
                        for (newi = i0; newi < ie; newi++)
                        {
                            icur = newi + irx - (int)xi0;
                            if (icur < 0) continue;
                            if (icur >= kLength) continue;

                            i_i0 = newi - i0;

                            //  2nd Edge 는 부호반전 필요
                            roughpeak_icur = -roughPeakBk[icur];
                            roughPeak_icur_1 = -roughPeakBk[icur + 1];

                            if (roughpeak_icur < 0)
                                roughpeak_icur = roughpeak_icur / 5;
                            if (roughPeak_icur_1 < 0)
                                roughPeak_icur_1 = roughPeak_icur_1 / 5;

                            if (rx > 0)
                            {
                                if (newi + irx + 1 - (int)xi0 < kLength)
                                    pY/*[i_i0]*/ = (1 - rx) * roughpeak_icur + rx * roughPeak_icur_1;
                                else
                                    pY/*[i_i0]*/ = roughpeak_icur;   //  다음 값이 없으면 같은 값으로 가정.

                            }
                            else
                            {
                                pY =  roughpeak_icur;
                            }

                            sumXY += (newi + res2nd) * pY;
                            sumY += pY;
                        }

                        res2nd = sumXY / (double)sumY;
                        if (sumY == 0)
                        {
                            res2nd = fxi0;
                            break;
                        }
                        err = oldres - res2nd;
                        err = err < 0 ? -err : err;
                        if (err < 0.0002)   //  0.0001 일때 반복성 더 나쁘다.
                            break;

                        if (err > 5 * err_1)
                        {
                            res2nd = errMem[(errMemCnt - 1) % 6];
                            break;
                        }
                        res2nd = res2nd + (res2nd - oldres) / 4;// 3.5 ; //  최대한 빨리 수렴하도록 접근속도를 변경함. 평균적으로 가장 빠르게 수렴시키는 최적값이 정확히 얼마인지는 알 수 없음.
                        err_1 = err;
                        oldres = res2nd;
                        errMem[errMemCnt] = res2nd;
                        errMemCnt = (errMemCnt + 1) % 6;
                    }
                    if (itr == 25)
                    {
                        res2nd = 0;
                        int emiCnt = 0;
                        for (int emi = 0; emi < 6; emi++)
                        {
                            if (errMem[emi] == 0)
                                break;
                            res2nd += errMem[emi];
                            emiCnt++;
                        }
                        res2nd = res2nd / emiCnt;
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("ConvergePeakX() 2>>\r\n" + e.ToString());
                    //return simpleRes;
                }
                if ((xia < 3 && (res2nd - xi0) > width / 2) || res2nd < 4 || (res2nd - xi0) > width)
                    res2nd = fxi0;
            }
            if (edgeDir > 1)
                res[1] = res2nd;    //  res1st

            return res;
        }
        public double[] ConvergePeakX3(int id, ref int[] Xidiffsrc, int width, int height, double xia, double yia, int xW, int yH, ref byte updown, ref int peaktype, int iIndex = 0)
        {
            //  원본 영상의 크기 width, height 로서 ROI 범위의 조각영상인 것을 전제로 한다.
            //  xi0 : 경계가 있을 것으로 예상되는 BOX 영역의 좌상단 X 좌표
            //  yi0 : 경계가 있을 것으로 예상되는 BOX 영역의 좌상단 Y 좌표
            //  xW  : BOX 영역의 폭
            //  yH  : BOX 영역의 높이
            //
            //  계산된 경계좌표를 중심으로 BOX 영역의 위치를 재설정하고 재설정된 BOX 영역에서 다시 경계좌표를 계산하기를 반복한다.
            //  반복하여, 직전 반복결과와 비교하여 결과의 변화가 0.001 이하이면 반복 종료, 최대반복은 5회까지 허용
            double res1st = 0;
            double res2nd = 0;
            double res3rd = 0;

            int xi0 = (int)xia;
            int yi0 = (int)yia;

            int fxi0 = xi0;
            double oldf = fxi0;
            double sumXY = 0;
            double sumY = 0;
            double sumXY2 = 0;
            double sumY2 = 0;

            int kLength = (int)(width - xia);
            if (kLength > width / 2 && xia < width / 4)
                kLength = width / 2 + 1;

            int edgeDir = id % 4;
            if (edgeDir > 1) //  Y 축의 경우 2Line 에 대비
                kLength += 4;

            double[] roughPeak = new double[kLength + 6];
            double[] roughPeakBk = new double[kLength + 6];
            int[] peakIndex = new int[kLength + 6];
            double[] effPeak = new double[kLength];
            int[] effIndex = new int[kLength];
            double[] intgPeak = new double[kLength + 6];

            //double[] ratio = new double[xW];

            //double[] sumxx = new double[kLength];
            //double[] sumx = new double[kLength];

            //int debug = 0;
            int i = 0;
            if (xi0 < 0) xi0 = 0;
            if (yi0 < 0) yi0 = 0;
            int potentialType = 0;
            double ry = yia - yi0;
            int xi0_i = 0;
            int pIndex = 0;
            double peak = -99999;

            int npIndex = 0;
            double npeak = 99999;

            int incCnt = 0;
            int repeatCnt = 0;

            bool negPeak = false;
            int maxLength = 0;


            if (peaktype % 100 == 2)
                negPeak = true;

            int inversion = (width + 1) / 2;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  반복성향상 전후 비교검토용
            //if ( edgeDir > 1) //   향상 전 기준치
            //    yH = yH / 2;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                //  Iteration 하기 전에 절대 Peak 위치를 찾아서 거기서부터 출발해야 한다. 안그러면 Peak 가 초기 검색범위에 없어서 실패할 수 있다.
                while (repeatCnt < 6)
                {
                    for (i = 0; i < kLength; i++)
                    {
                        xi0_i = (int)(xi0 + i);
                        peakIndex[i] = xi0_i;
                        for (uint j = 0; j < yH; j++)
                        {
                            if (j + yi0 >= height - 1)
                                break;

                            roughPeak[i] += (1 - ry) * Xidiffsrc[xi0_i + (j + yi0) * width] + ry * Xidiffsrc[xi0_i + (j + yi0 + 1) * width];
                        }

                        if (xi0 < 5 && xi0_i >= inversion)    //  우측 경계
                            roughPeak[i] = -roughPeak[i];

                        else if (xi0 > 5 && xi0_i < inversion)   //   좌측 경계
                            roughPeak[i] = -roughPeak[i];

                        roughPeakBk[i] = roughPeak[i];

                        if (edgeDir > 1)
                        {
                            if (npeak < -1500 && peak > 1500 && npIndex > pIndex)
                                continue;

                            if (i > 1 && i < kLength / 2 - 1)
                            {
                                if (peak < roughPeakBk[i])
                                {
                                    pIndex = i;
                                    peak = roughPeakBk[i];  //   최대값 및 최대값일 때 Index
                                }
                                if (npeak > roughPeakBk[i])  //peaktype == 3 이면 정방향 Peak 를 찾아야 한다.
                                {
                                    npIndex = i;
                                    npeak = roughPeakBk[i];  //   최소값 및 최소값일 때 Index
                                }
                            }
                        }
                        else
                        {
                            if (i > 1 && i < kLength - 2)
                            {
                                if (peak < roughPeakBk[i])
                                {
                                    pIndex = i;
                                    peak = roughPeakBk[i];
                                }
                                if (npeak > roughPeakBk[i])  //peaktype == 3 이면 정방향 Peak 를 찾아야 한다.
                                {
                                    npIndex = i;
                                    npeak = roughPeakBk[i];
                                }
                            }
                        }
                    }
                    //if ( (( Math.Abs(peak) < Math.Abs(npeak) && (peaktype % 100) != 3 ) || peaktype % 100 == 2) && npIndex > 6) //  peaktype % 100 == 2 이면 기울기가 음수 Peak 인 곳을 찾는다.
                    //if ((( peak*peak < npeak*npeak && (peaktype % 100) != 3) || peaktype % 100 == 2) && npIndex > 6) //  peaktype % 100 == 2 이면 기울기가 음수 Peak 인 곳을 찾는다.
                    //{
                    //    pIndex = npIndex;
                    //    peak = npeak;
                    //}
                    if (pIndex > kLength - 4 && incCnt < 6)
                    {
                        kLength++;
                        incCnt++;
                    }
                    if (pIndex < 6)
                    {
                        if (xi0 == 0)
                            break;
                        xi0--;
                        repeatCnt++;
                        peak = -99999;
                        roughPeak = new double[kLength + 6];
                        roughPeakBk = new double[kLength + 6];
                        peakIndex = new int[kLength + 6];
                    }
                    else
                    {
                        break;
                    }
                }

                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  가장 큰 값의 Index 가 양끝단 0,1,2 에 있거나 kLength-3, kLength-2, kLength-1 에 있는 경우는 범위를 변경시켜야 한다.

                //  2개의 봉우리가 있는 경우 6이상의 앞쪽 봉우리를 선택한다.
                //if (pIndex != npIndex && iIndex == 0)
                //double[] cproughPeakBk = new double[roughPeakBk.Length];
                //int[] cppeakIndex = new int[roughPeakBk.Length];
                //Array.Copy(roughPeakBk, 0, cproughPeakBk, 0, roughPeakBk.Length);
                //Array.Copy(peakIndex, 0, cppeakIndex, 0, roughPeakBk.Length);
                //Array.Sort(cproughPeakBk, cppeakIndex);
                //int pIndexOld = pIndex;
                //if (iIndex == 0)
                //{
                //    if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 2]) > 6)    // 2번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cproughPeakBk[roughPeakBk.Length - 1] / cproughPeakBk[roughPeakBk.Length - 2] < 1.2 && cppeakIndex[roughPeakBk.Length - 2] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            //  그중에서도 2번쨰 peak 가 첫째 Peak 의 90% 이상이고 2번째 Peak 가 앞쪽이고 index 가 6 이상이면 2번째 Peak 를 써라
                //            if (cppeakIndex[roughPeakBk.Length - 2] > 7)
                //            {
                //                pIndex = cppeakIndex[roughPeakBk.Length - 2] - (int)xia;
                //                mPeakOrder[id] = 1; //  앞쪽 Peak 를 써라
                //            }
                //        }
                //    }
                //    else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 3]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cproughPeakBk[roughPeakBk.Length - 1] / cproughPeakBk[roughPeakBk.Length - 3] < 1.2 && cppeakIndex[roughPeakBk.Length - 3] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            //  그중에서도 3번쨰 peak 가 첫째 Peak 의 85% 이상이고 3번째 Peak 가 앞쪽이고 index 가 6 이상이면 3번째 Peak 를 써라
                //            if (cppeakIndex[roughPeakBk.Length - 3] > 7)
                //            {
                //                pIndex = cppeakIndex[roughPeakBk.Length - 3] - (int)xia;
                //                mPeakOrder[id] = 1; //  앞쪽 Peak 를 써라 
                //            }
                //        }
                //    }
                //}
                //else if (iIndex > 0 && mPeakOrder[id] == 1)
                //{
                //    if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 2]) > 6)    // 2번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cppeakIndex[roughPeakBk.Length - 2] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 2] - (int)xia;
                //        }
                //    }
                //    else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 3]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cppeakIndex[roughPeakBk.Length - 3] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 3] - (int)xia;
                //        }
                //    }
                //    else if (Math.Abs(cppeakIndex[roughPeakBk.Length - 1] - cppeakIndex[roughPeakBk.Length - 4]) > 6)   // 3번째 peak 가 1번째 피크와 거리가 멀 때
                //    {
                //        if (cppeakIndex[roughPeakBk.Length - 4] < cppeakIndex[roughPeakBk.Length - 1])
                //        {
                //            pIndex = cppeakIndex[roughPeakBk.Length - 4] - (int)xia;
                //        }
                //    }
                //}
                //if (pIndex != pIndexOld)
                //    pIndex = pIndex;
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                int effStart = peakIndex[3];
                int effEnd = peakIndex[peakIndex.Length - 4];
                i = peakIndex.Length - 4;
                while (effEnd == 0)
                {
                    effEnd = peakIndex[--i];
                    if (i == 10)
                        break;
                }
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///
                ///     바로 위까지해서 peak Index 를 구했다.
                ///     
                //  Peak 전후 9개배열만 활용해서 Rising Edge / Falling Edge / Rising Sharp Edge / Falling Sharp Edge 인지 구분한다.
                //  구분하여 peak 주변 9개 배열 값의 최소값이 0 이 되도록 Offset 처리한다. 이로써 Peak 의 중심좌표가 가장 정확하게 추출된다.

                int iFrom = (pIndex - 5 < 0 ? 0 : pIndex - 5);
                int iTo = (kLength - pIndex < 6 ? kLength - 1 : pIndex + 5);

                if ( id%4>1)
                {
                    iFrom = (pIndex - 4 < 0 ? 0 : pIndex - 4);
                    iTo = (kLength - pIndex < 5 ? kLength - 1 : pIndex + 4);
                }

                int effLength = iTo - iFrom + 1;
                effPeak = new double[effLength];
                effIndex = new int[effLength];

                // 최대값의 절대값과 최소값의절대값 간 비율이 0.8 ~ 1.2 이고, Index 차이가 4 이하인 경우는 적분값으로 대체해서 계산한다.
                // 피크 +/-3 영역에 부호가 반대이고 절대값이 50% 이상인 점이있는 경우
                //  roughPeak 에서 첫번째  +peak : 4~5번째, 
                //  roughPeak 에서 두번째  -peak : 11~12번째, 
                //  roughPeak 에서 세번째  +peak : 20~21째 -> Bottom 에 속함.
                if (edgeDir < 3)
                {
                    Array.Copy(roughPeakBk, iFrom, effPeak, 0, effLength);
                    Array.Copy(peakIndex, iFrom, effIndex, 0, effLength);
                    Array.Sort(effPeak, effIndex);  //  더 빠른 함수로 변경 필요  231116
                    fxi0 = effIndex[effIndex.Length - 1];
                }
                else
                    fxi0 = pIndex + xi0;

                potentialType = peaktype;

                //  아래는 연산 범위내에 음수가 들어가지 않도록 offset 을 만들어준다
                //  이런 방식 말고, 연산범위내에 음수는 0 으로 처리하거나
                //  연산범위내에서 음수는  0.1 ~ 0.2 을 곱해서 비중이 아주 작도록 처리하는 방법이 있다.
                //double minValue = effPeak[1];   
                //double maxValue = effPeak[effPeak_Length_1-1];
                if (updown == 255)  //  방향성이 미정인 경우
                {
                    //if ((Math.Abs(minValue) > maxValue && peaktype == 0) || peaktype == 2)
                    if (peaktype == 0 || peaktype == 2)
                    {
                        // Peak 가 뭔가 이상해서 부호 반전 필요한 경우
                        potentialType = 2;
                        updown = 0;
                    }
                    else
                    {
                        potentialType = 3;
                        updown = 1;         //  정상적인 Peak 인 경우
                    }
                    peaktype = potentialType;
                }
                else
                {
                    if (updown == 0 || peaktype == 2)
                    {
                        // 부호 반전 필요.
                        potentialType = 2;
                    }
                    else
                    {
                        potentialType = 3;
                    }
                    peaktype = potentialType;
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show("ConvergePeakX() 1>> \r\n" + e.ToString());
                //return fxi0;
            }

            //  xW 의 자동조정은, xW 를 조절할 수 있는 분해능이 매우 낮아서 오히려 역효과 발생.
            //  상황에 따라 고정값 적용이 적합. 즉 Focusing 수준에 따라서 2가지 또는 3가지 값중 선택하는 방식은 가능할 것 같음.
            //  실험적으로 xW = 7 일때 반복성이 가장 좋은 것으로 나타남.

            int i0 = -3;// -(xW - 1) / 2;
            int ie = -i0 + 1;

            int icur = 0;
            sumXY = 0;
            sumY = 0;
            int newi = 0;
            double ratioXroughPeak = 0;
            maxLength = roughPeakBk.Length - 1;
            for (newi = i0; newi < ie; newi++)
            {
                icur = newi + fxi0 - (int)xi0;
                if (icur < 0) continue;
                if (icur >= maxLength) break;
                //ratioXroughPeak = ratio[newi - i0] * roughPeakBk[icur];
                ratioXroughPeak = roughPeakBk[icur];
                sumXY += ratioXroughPeak * (newi + fxi0);
                sumY += ratioXroughPeak;
            }
            res1st = sumXY / (double)sumY;

            if (res1st - xi0 < 0) //  극히 비정상인 경우 두번째 Peak 를 활용한다.
                res1st = peakIndex[kLength - 2]; //  Peak 좌표

            double oldres = res1st;
            double pY = 0;
            double err = 999;
            double err_1 = 999;

            uint itr = 0;
            double[] errMem = new double[10];
            int errMemCnt = 0;
            double roughpeak_icur = 0;
            double roughPeak_icur_1 = 0;

            try
            {
                for (itr = 0; itr < 25; itr++)
                {
                    int irx = (int)res1st;
                    double rx = res1st - irx;
                    sumXY = 0;
                    sumY = 0;
                    sumXY2 = 0;
                    sumY2 = 0;
                    int i_i0 = 0;
                    for (newi = i0; newi < ie; newi++)
                    {
                        icur = newi + irx - (int)xi0;
                        if (icur < 0) continue;
                        if (icur >= kLength) continue;

                        i_i0 = newi - i0;
                        roughpeak_icur = roughPeakBk[icur];
                        roughPeak_icur_1 = roughPeakBk[icur + 1];

                        if (roughpeak_icur < 0)
                            roughpeak_icur = roughpeak_icur / 5;
                        if (roughPeak_icur_1 < 0)
                            roughPeak_icur_1 = roughPeak_icur_1 / 5;

                        if (rx > 0)
                        {
                            if (newi + irx + 1 - (int)xi0 < kLength)
                                pY/*[i_i0]*/ = (1 - rx) * roughpeak_icur + rx * roughPeak_icur_1;
                            else
                                pY/*[i_i0]*/ = roughpeak_icur;   //  다음 값이 없으면 같은 값으로 가정.

                        }
                        else
                        {
                            pY = roughpeak_icur;
                        }

                        sumXY += (newi + res1st) * pY;
                        sumY += pY;
                    }

                    res1st = sumXY / (double)sumY;
                    if (sumY == 0)
                    {
                        res1st = fxi0;
                        break;
                    }
                    err = oldres - res1st;
                    err = err < 0 ? -err : err;
                    if (err < 0.0002)   //  0.0001 일때 반복성 더 나쁘다.
                        break;

                    if (err > 5 * err_1)
                    {
                        res1st = errMem[(errMemCnt - 1) % 6];
                        break;
                    }
                    res1st = res1st + (res1st - oldres) / 4;// 3.5 ; //  최대한 빨리 수렴하도록 접근속도를 변경함. 평균적으로 가장 빠르게 수렴시키는 최적값이 정확히 얼마인지는 알 수 없음.
                    err_1 = err;
                    oldres = res1st;
                    errMem[errMemCnt] = res1st;
                    errMemCnt = (errMemCnt + 1) % 6;
                }
                if (itr == 25)
                {
                    res1st = 0;
                    int emiCnt = 0;
                    for (int emi = 0; emi < 6; emi++)
                    {
                        if (errMem[emi] == 0)
                            break;
                        res1st += errMem[emi];
                        emiCnt++;
                    }
                    res1st = res1st / emiCnt;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("ConvergePeakX() 2>>\r\n" + e.ToString());
                //return simpleRes;
            }
            if ((xia < 3 && (res1st - xi0) > width / 2) || res1st < 4 || (res1st - xi0) > width)
                res1st = fxi0;

            double[] res = new double[3];
            res[0] = res1st;

            if ( edgeDir > 1)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  반복성향상 전후 비교검토용
                //  res2nd
                res2nd = res1st + 6;

                oldres = res2nd;

                pY = 0;
                err = 999;
                err_1 = 999;

                itr = 0;
                errMem = new double[10];
                errMemCnt = 0;
                roughpeak_icur = 0;
                roughPeak_icur_1 = 0;
                i0 = -3;
                ie = 4;
                try
                {
                    for (itr = 0; itr < 25; itr++)
                    {
                        int irx = (int)res2nd;
                        double rx = res2nd - irx;
                        sumXY = 0;
                        sumY = 0;
                        sumXY2 = 0;
                        sumY2 = 0;
                        int i_i0 = 0;
                        for (newi = i0; newi < ie; newi++)
                        {
                            icur = newi + irx - (int)xi0;
                            if (icur < 0) continue;
                            if (icur >= kLength) continue;

                            i_i0 = newi - i0;

                            //  2nd Edge 는 부호반전 필요
                            roughpeak_icur = -roughPeakBk[icur];
                            roughPeak_icur_1 = -roughPeakBk[icur + 1];

                            if (roughpeak_icur < 0)
                                roughpeak_icur = roughpeak_icur / 5;
                            if (roughPeak_icur_1 < 0)
                                roughPeak_icur_1 = roughPeak_icur_1 / 5;

                            if (rx > 0)
                            {
                                if (newi + irx + 1 - (int)xi0 < kLength)
                                    pY/*[i_i0]*/ = (1 - rx) * roughpeak_icur + rx * roughPeak_icur_1;
                                else
                                    pY/*[i_i0]*/ = roughpeak_icur;   //  다음 값이 없으면 같은 값으로 가정.

                            }
                            else
                            {
                                pY = roughpeak_icur;
                            }

                            sumXY += (newi + res2nd) * pY;
                            sumY += pY;
                        }

                        res2nd = sumXY / (double)sumY;
                        if (sumY == 0)
                        {
                            res2nd = fxi0;
                            break;
                        }
                        err = oldres - res2nd;
                        err = err < 0 ? -err : err;
                        if (err < 0.0002)   //  0.0001 일때 반복성 더 나쁘다.
                            break;

                        if (err > 5 * err_1)
                        {
                            res2nd = errMem[(errMemCnt - 1) % 6];
                            break;
                        }
                        res2nd = res2nd + (res2nd - oldres) / 4;// 3.5 ; //  최대한 빨리 수렴하도록 접근속도를 변경함. 평균적으로 가장 빠르게 수렴시키는 최적값이 정확히 얼마인지는 알 수 없음.
                        err_1 = err;
                        oldres = res2nd;
                        errMem[errMemCnt] = res2nd;
                        errMemCnt = (errMemCnt + 1) % 6;
                    }
                    if (itr == 25)
                    {
                        res2nd = 0;
                        int emiCnt = 0;
                        for (int emi = 0; emi < 6; emi++)
                        {
                            if (errMem[emi] == 0)
                                break;
                            res2nd += errMem[emi];
                            emiCnt++;
                        }
                        res2nd = res2nd / emiCnt;
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("ConvergePeakX() 2>>\r\n" + e.ToString());
                    //return simpleRes;
                }
                if ((xia < 3 && (res2nd - xi0) > width / 2) || res2nd < 4 || (res2nd - xi0) > width)
                    res2nd = fxi0;


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //  반복성향상 전후 비교검토용
                //  res3rd
                res3rd = res2nd + 6;

                oldres = res3rd;

                pY = 0;
                err = 999;
                err_1 = 999;

                itr = 0;
                errMem = new double[10];
                errMemCnt = 0;
                roughpeak_icur = 0;
                roughPeak_icur_1 = 0;
                i0 = -3;
                ie = 4;
                try
                {
                    for (itr = 0; itr < 25; itr++)
                    {
                        int irx = (int)res3rd;
                        double rx = res3rd - irx;
                        sumXY = 0;
                        sumY = 0;
                        sumXY2 = 0;
                        sumY2 = 0;
                        int i_i0 = 0;
                        for (newi = i0; newi < ie; newi++)
                        {
                            icur = newi + irx - (int)xi0;
                            if (icur < 0) continue;
                            if (icur >= kLength) continue;

                            i_i0 = newi - i0;

                            //  3rd Edge 는 부호반전 불필요
                            roughpeak_icur = roughPeakBk[icur];
                            roughPeak_icur_1 = roughPeakBk[icur + 1];

                            if (roughpeak_icur < 0)
                                roughpeak_icur = roughpeak_icur / 5;
                            if (roughPeak_icur_1 < 0)
                                roughPeak_icur_1 = roughPeak_icur_1 / 5;

                            if (rx > 0)
                            {
                                if (newi + irx + 1 - (int)xi0 < kLength)
                                    pY/*[i_i0]*/ = (1 - rx) * roughpeak_icur + rx * roughPeak_icur_1;
                                else
                                    pY/*[i_i0]*/ = roughpeak_icur;   //  다음 값이 없으면 같은 값으로 가정.

                            }
                            else
                            {
                                pY = roughpeak_icur;
                            }

                            sumXY += (newi + res3rd) * pY;
                            sumY += pY;
                        }

                        res3rd = sumXY / (double)sumY;
                        if (sumY == 0)
                        {
                            res3rd = fxi0;
                            break;
                        }
                        err = oldres - res3rd;
                        err = err < 0 ? -err : err;
                        if (err < 0.0002)   //  0.0001 일때 반복성 더 나쁘다.
                            break;

                        if (err > 5 * err_1)
                        {
                            res3rd = errMem[(errMemCnt - 1) % 6];
                            break;
                        }
                        res3rd = res3rd + (res3rd - oldres) / 4;// 3.5 ; //  최대한 빨리 수렴하도록 접근속도를 변경함. 평균적으로 가장 빠르게 수렴시키는 최적값이 정확히 얼마인지는 알 수 없음.
                        err_1 = err;
                        oldres = res3rd;
                        errMem[errMemCnt] = res3rd;
                        errMemCnt = (errMemCnt + 1) % 6;
                    }
                    if (itr == 25)
                    {
                        res3rd = 0;
                        int emiCnt = 0;
                        for (int emi = 0; emi < 6; emi++)
                        {
                            if (errMem[emi] == 0)
                                break;
                            res3rd += errMem[emi];
                            emiCnt++;
                        }
                        res3rd = res3rd / emiCnt;
                    }
                }
                catch (Exception e)
                {
                    //MessageBox.Show("ConvergePeakX() 2>>\r\n" + e.ToString());
                    //return simpleRes;
                }
                if ((xia < 3 && (res3rd - xi0) > width / 2) || res3rd < 4 || (res3rd - xi0) > width)
                    res3rd = fxi0;
            }
            if ( edgeDir > 1)
            {
                if (edgeDir == 2)   //   Y Top
                {
                    res[0] = res1st;
                    res[1] = res2nd;
                    res[2] = res3rd;
                }else if ( edgeDir == 3)    //  Y Bottom
                {
                    res[0] = res3rd;
                    res[1] = res2nd;
                    res[2] = res1st;
                }
            }

            return res;
        }

        public bool mCalcCrossAngle(Point2D pfrom, Point2D pTo, ref double langleRad)
        {
            //  ldist 는 거리로 환산한 직각도
            //  langleDeg 는 degree 로 환산한 직각도
            //  cos(theta) = A * B / (|A| * |B|)

            double outerProduct = pfrom.X * pTo.Y - pfrom.Y * pTo.X;
            try
            {
                double absLR = Math.Sqrt(pfrom.Y * pfrom.Y + pfrom.X * pfrom.X);
                absLR = absLR * Math.Sqrt(pTo.Y * pTo.Y + pTo.X * pTo.X);

                langleRad = Math.Asin(outerProduct / absLR);
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
        public bool mCalcCrossAngle(Point2D p1_i, Point2D p2_i, Point2D p1_f, Point2D p2_f, ref double langleRad)
        {
            //  1 -> 2 벡터가 시간에 따라 어떻게 회전했는지 계산.
            //  cos(theta) = A * B / (|A| * |B|)
            Point2D pfrom = new Point2D();
            Point2D pto   = new Point2D();

            pfrom.X = p2_i.X - p1_i.X;
            pfrom.Y = p2_i.Y - p1_i.Y;

            pto.X = p2_f.X - p1_f.X;
            pto.Y = p2_f.Y - p1_f.Y;
            mCalcCrossAngle(pfrom, pto, ref langleRad);
            return true;
        }

        public void GetC12C13fromP12P13(Point2D p1, Point2D p2, Point2D p3, ref double c12, ref double c13)
        {
            //  Side View 기준
            //
            //  본 함수는 Nominal 좌표값이 설정됬을 때 한번만 수행하면 된다.
            //  본 함수는 Nominal 좌표를 활용하는 것을 전제로 한다.
            //  원점을 P1 으로부터 P12 = P1->P2, P13=P1->P3 벡터의 합으로 나타낸다.
            //
            //  측정값을 활용하려면, 측정값의 좌표계를 Nominal 좌표계로 변환해서 입력해줘야 한다.
            //  Side View 기준이므로 Nominal 값을 활용할 때는 Y좌표값에 sin(40) 를 곱해줘야 한다.
            //if (p1 == null || p2 == null || p3 == null)
            //    return;

            double[,] m22= new double[2, 2];
            double[] m21 = new double[2];

            m22[0, 0] = p2.X - p1.X;
            m22[1, 0] = p3.X - p1.X;
            m22[0, 1] = (p2.Y - p1.Y) * vSin40;
            m22[1, 1] = (p3.Y - p1.Y) * vSin40;
            m21[0] = -p1.X;
            m21[1] = -p1.Y * vSin40;

            double[,] invm22 = MatrixInverse(m22, 2);
            InverseU(ref m22, 2);

            double[] k = new double[2];

            MatrixCross(ref m21, ref invm22, ref k, 2, 2);

            c12 = k[0];
            c13 = k[1];
        }

        public void OrgByC12nC13(Point2D ps1, Point2D ps2, Point2D ps3, Point2D pt1, double c12, double c13, ref Point2D orgPside, ref Point2D orgPtop)
        {
            //  ps1,  ps2,  ps3,  pt1 는 모두 광학계구조적 오차보정 및 스케일보정이 완료된 오른손좌표계 상태임을 전제로 한다.
            //
            //  P1, P2, P3 가 측정되었을 때 원점은 어디에 있는지 계산한다.
            //  원점의 좌표를 C12, C13 과 P1, P12, P13 으로부터 계산한다.
            //
            //  즉 영상 좌하단 꼭지점이 (0,0) 이고 영상 위쪽이 +Y, 영상 우측이 +X 이다.
            //
            //  SideView 에서 측정된 N,S,E 마크로부터 CIS 의 SideView 상의 좌표를 알아낸다.
            //  N - S 마크에 대한 CIS 의 상대적 좌표로부터 Top View 상의 CIS 좌표를 알아낸다.
            //

            //Point2D p12 = new Point2D();
            //Point2D p13 = new Point2D();

            //p12.X = ps2.X - ps1.X;
            //p13.X = ps3.X - ps1.X;
            //p12.Y = ps2.Y - ps1.Y;
            //p13.Y = ps3.Y - ps1.Y;

            //orgPside.X = ps1.X + c12 * p12.X + c13 * p13.X;
            //orgPside.Y = ps1.Y + c12 * p12.Y + c13 * p13.Y;

            //Point2D p1toOrgPside = new Point2D(orgPside.X - ps1.X, orgPside.Y - ps1.Y);

            //orgPtop.X = pt1.X + p1toOrgPside.X;
            //orgPtop.Y = pt1.Y + p1toOrgPside.Y / vSin40;

            orgPside.X = (ps1.X + ps2.X) / 2;
            orgPside.Y = (ps1.Y + ps2.Y) / 2;
            orgPtop.X = (ps1.X + ps2.X) / 2;
            orgPtop.Y = pt1.Y;

        }

        public Point2D[] PrelativetoCIS(Point2D[] p, Point2D orgP)
        {
            //  Point Vector relative to the center of Image Sensor
            //  center of Image Sensor ( orgP ) 가 주어지면, 제공된 벡터를 orgP 점에 대한 상대벡터로 변환한다.
            //  입력변수는 오른손좌표계, 화면 좌하단 모서리가 (0,0) 이고 위쪽이 +Y, 우측이 +X 인 좌표계 기준 좌표임을 전제한다.
            Point2D[] resp = new Point2D[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == null)
                {
                    resp[i] = null;
                    continue;
                }
                resp[i] = new Point2D(p[i].X - orgP.X, p[i].Y - orgP.Y);
            }

            return resp;
        }

        public Point2D[] RotateAlongZ(Point2D[] p, double psi)
        {
            // psi radian 만큼 Z 축에 대해서 좌표를 회전변환한다.
            //  반시계방향 회전각 Psi 
            double[,] R = new double[2, 2];
            double[] mP = new double[2];
            double[] rP = new double[2];
            Point2D[] resp = new Point2D[p.Length];

            R[0, 0] =   Math.Cos(psi);
            R[0, 1] = - Math.Sin(psi);
            //R[1, 0] = Math.Sin(psi);
            //R[1, 1] = Math.Cos(psi);
            R[1, 0] = - R[0, 1];
            R[1, 1] =   R[0, 0];

            for (int i = 0; i < p.Length; i++)
            {
                if ( p[i] == null)
                {
                    resp[i] = null;
                    continue;
                }
                mP[0] = p[i].X;
                mP[1] = p[i].Y;
                MatrixCross(ref R, ref mP, ref rP, 2);
                resp[i] = new Point2D(rP[0], rP[1]);
            }
            return resp;
        }

        //public Point2D TnPsifromTopView(Point2D COI, Point2D[] pOrg, Point2D[] pAfter, ref double psi, ref double[] err)
        //{
        //    //  pBefore : 회전 및 병진이 적용되기 전 Vector
        //    //  pAfter : 회전 및 병진이 적용된 후 Vector
        //    //  psi : 회전 각도 Radian
        //    //  평균 Translation Vector T 를 구하고, 평균 T 벡터와 개별 Ti 간의 차이의 절대값을  err 에 기록한다.

        //    //  mCalcCrossAngle(Point2D p1_i, Point2D p2_i, Point2D p1_f, Point2D p2_f, ref double langleRad, ref double langleDeg)
        //    //  p[0]->p[1] 벡터의 Before, After 로부터 회전각도를 구한다.

        //    Point2D resT = new Point2D();
        //    if (pAfter[0] == null || pAfter[1] == null)
        //        return resT;

        //    mCalcCrossAngle(pOrg[0], pOrg[1], pAfter[0], pAfter[1], ref psi);

        //    //Point2D[] pBefore = new Point2D[pOrg.Length];
        //    Point2D[] pBefore = new Point2D[2];
        //    int pBefore_Length = pBefore.Length;

        //    for ( int i=0; i< pBefore_Length; i++ )
        //        pBefore[i] = new Point2D(pOrg[i].X - COI.X, pOrg[i].Y - COI.Y);

        //    Point2D[] RotatedP = RotateAlongZ(pBefore, -psi);    //  회전 중심 정보가 없는 상황임.

        //    for (int i = 0; i < RotatedP.Length; i++)
        //    {
        //        RotatedP[i].X += COI.X;
        //        RotatedP[i].Y += COI.Y;
        //    }
        //    double[] tx = new double[pBefore_Length];
        //    double[] ty = new double[pBefore_Length];

        //    for ( int i=0; i< pBefore_Length; i++ )
        //    {
        //        tx[i] = pAfter[i].X - RotatedP[i].X;
        //        ty[i] = pAfter[i].Y - RotatedP[i].Y;
        //        resT.X += tx[i];
        //        resT.Y += ty[i];
        //    }
        //    resT.X = resT.X / pBefore_Length;
        //    resT.Y = resT.Y / pBefore_Length;

        //    for (int i = 0; i < pBefore_Length; i++)
        //    {
        //        err[i] = Math.Sqrt((resT.X - tx[i]) * (resT.X - tx[i]) + (resT.Y - ty[i]) * (resT.Y - ty[i]));
        //    }

        //    return resT;
        //}

        public Point2D TnPsifromTopView(Point2D tCOI, Point2D[] tpOrg, Point2D[] tpAfter, Point2D sCOI, Point2D[] spOrg, Point2D[] spAfter, ref double psi, ref double[] err)
        {
            //  pBefore : 회전 및 병진이 적용되기 전 Vector
            //  pAfter : 회전 및 병진이 적용된 후 Vector
            //  psi : 회전 각도 Radian
            //  평균 Translation Vector T 를 구하고, 평균 T 벡터와 개별 Ti 간의 차이의 절대값을  err 에 기록한다.

            //  mCalcCrossAngle(Point2D p1_i, Point2D p2_i, Point2D p1_f, Point2D p2_f, ref double langleRad, ref double langleDeg)


            Point2D resT = new Point2D();
            if (tpAfter[0] == null || tpAfter[1] == null || spAfter[0] == null || spAfter[2] == null || spAfter[3] == null)
                return resT;

            //  순수하게 North - South mark 를 연결하는 벡터의 TopView 에서의 각도변화를 측정
            
            mCalcCrossAngle(tpOrg[0], tpOrg[1], tpAfter[0], tpAfter[1], ref psi);

            //  Top View의 PSI 회전 전 좌표를 구한다.
            Point2D[] tpBefore = new Point2D[2];
            int tpBefore_Length = tpBefore.Length;
            Point2D[] spBefore = new Point2D[spOrg.Length];
            int spBefore_Length = spBefore.Length;

            for (int i = 0; i < tpBefore_Length; i++)
                tpBefore[i] = new Point2D(tpOrg[i].X - tCOI.X, -(tpOrg[i].Y - tCOI.Y));    //  Top View Mark 에 대해서 Center Of Image 에 대한 회전 전 좌표를 계산한다.

            for (int i = 0; i < spOrg.Length; i++)
            {
                if (spOrg[i] == null)
                    continue;
                spBefore[i] = new Point2D(spOrg[i].X - sCOI.X, -(spOrg[i].Y - sCOI.Y)/ vSin40);     //  Side View Mark 에 대해서 Center Of Image 에 대한 회전 전 좌표를 계산한다.
            }

            Point2D[] RotatedP = RotateAlongZ(tpBefore, -psi);    //  이상적 Top View Mark 를 psi 만큼 역회전 시킨다
            Point2D[] RotatedPS = RotateAlongZ(spBefore, -psi);    //  이상적 Side View Mark 를 psi 만큼 역회전 시킨다

            //  Top View 에서 이상적 좌표가 역회전한 영상좌표로서, tpOrg 와 비교된다.
            for (int i = 0; i < RotatedP.Length; i++)
            {
                RotatedP[i].X += tCOI.X;
                RotatedP[i].Y = tCOI.Y - RotatedP[i].Y;
            }

            //  Side View 에서 이상적 좌표가 역회전한 영상좌표로서, spOrg 와 비교된다.
            for (int i = 0; i < spBefore_Length; i++)
            {
                if (RotatedPS[i] == null)
                    continue;
                RotatedPS[i].X += sCOI.X;
                RotatedPS[i].Y = sCOI.Y - RotatedPS[i].Y * vSin40;
            }

            double[] tx = new double[4];
            double[] ty = new double[4];

            for (int i = 0; i < tpBefore_Length; i++)
            {
                
                tx[i] = tpAfter[i].X - RotatedP[i].X;   //  Top View 에서는 단순히 Z 회전만 적용된 좌표를 빼주면 Shift 만 남는다.
                ty[i] = tpAfter[i].Y - RotatedP[i].Y;   //  Top View 에서는 단순히 Z 회전만 적용된 좌표를 빼주면 Shift 만 남는다.
                resT.X += tx[i];
                resT.Y += ty[i];
            }
            //for (int i = 0; i < spBefore_Length; i++)
            for (int i = 0; i < 3; i++)
            {
                if (spAfter[i] == null)
                    continue;

                tx[i] = spAfter[i].X - RotatedPS[i].X;  //  Z 회전성분을 소거
                resT.X += tx[i];                    //  Side View 로부터의 X 변동량은 오차가 클 수 있으니 1/2 의 비중을 적용한다.
            }
            //  East Mark 에서 회전성분을 제거한 나머지 부분
            resT.X = resT.X / (tpBefore_Length + 2);  //  Side View 로부터의 X 변동량은 오차가 클 수 있으니 1/2 의 비중을 적용한다.
            resT.Y = resT.Y / tpBefore_Length;

            return resT;
        }
        public void CompnesateTopViewWithTXTY(Point2D tCOI, Point2D[] tpOrg, double TX, double TY, ref Point2D[] tpAfter)
        {
            Point2D[] tpBefore = new Point2D[2];
            for (int i = 0; i < 2; i++)
                tpBefore[i] = new Point2D(tpOrg[i].X - tCOI.X, -(tpOrg[i].Y - tCOI.Y));    //  Top View Mark 에 대해서 Center Of Image 에 대한 회전 전 좌표를 계산한다.

            double dx = 0;
            double dy = 0;
            for (int i = 0; i < 2; i++)
            {
                dx = tpBefore[i].X * (1 - Math.Cos(TY));
                dy = tpBefore[i].Y * (1 - Math.Cos(TX));

                tpAfter[i].X = tpBefore[i].X + dx + tCOI.X;
                tpAfter[i].Y = tCOI.Y - ( tpBefore[i].Y + dy );
            }
        }
        public void CalcScaleTopNSide(Point2D[] pTop, Point2D[] pSide, double normXdist, ref double scaleTop, ref double scaleSide)
        {
            //  normXdist must be provided as Pixel

            //  result is pixel / um
            //double idealScale = normXdist / (0.0055 / Global.LensMag);
            //scaleTop = idealScale / Math.Sqrt( (pTop[0].X - pTop[1].X) * (pTop[0].X - pTop[1].X) + (pTop[0].Y - pTop[1].Y) * (pTop[0].Y - pTop[1].Y));
            //scaleSide = idealScale / Math.Sqrt( (pSide[0].X - pSide[1].X) * (pSide[0].X - pSide[1].X) + (pSide[0].Y - pSide[1].Y) * (pSide[0].Y - pSide[1].Y));
            scaleTop = normXdist / (pTop[0].X - pTop[1].X);
            scaleSide = normXdist / (pSide[0].X - pSide[1].X);
        }
        //  아래 함수는 TopView 광학계 Side View 광학계 자체의 회전각도 오차 보정용 함수
        public void TopNSideViewRotationAnlge(Point2D[] pTopNorm, Point2D[] pSideNorm, Point2D[] pTop, Point2D[] pSide, ref double aT, ref double aS)
        {
            //  TX = TY = 0 인 평면위에 인식가능한 마크 M_north, M_south 를 만들어 Top View 및 Side View 에서 동시에 측정이 가능하도록 준비되어야 한다.
            //  별도로 설계된 Mask Plate 를 활용한다.
            //  North, South  Mark 의 Top View 좌표 및 Side View 좌표를 이용해 두 좌표계의 Angle 을 구한다.
            //  mCalcCrossAngle(Point2D pfrom, Point2D pTo, ref double langleRad) 를 활용한다.
            //  측정된 각도는 오직 XY 평면에서의 Z 축에 대한 회전으로 발생한 각도로 가정한다.
            //  Top View M_north, M_south 가 X 축에 늘어서도록 하는 각도 aT 를 구한다.
            //  Side View M_north, M_south 가 Y 축에 늘어서도록 하는 각도 aS 를 구한다.
            //  
            //  이후 모든 측정시 측정 직후 좌표를 aT , aS 만큼 역회전시킨다. 회전의 중심은 각각 Top View 의 중심, Side View 의 중심을 활용한다.

            //  수평 벡터는 실제로 존재하지 않으므로, 이상적인 norminalPoint 로부터의 각도를 계산한다.


            Point2D pFrom = new Point2D(0,0);
            Point2D pTo = new Point2D(0,0);

            pFrom.X = pTopNorm[1].X - pTopNorm[0].X;
            pFrom.Y = pTopNorm[1].Y - pTopNorm[0].Y;
            pTo.X = pTop[1].X - pTop[0].X;
            pTo.Y = pTop[1].Y - pTop[0].Y;
            if (pTo.X < 0)
            {
                pTo.X = -pTo.X;
                pTo.Y = -pTo.Y;
            }

            mCalcCrossAngle(pFrom, pTo, ref aT);

            pFrom.X = pSideNorm[1].X - pSideNorm[0].X;
            pFrom.Y = pSideNorm[1].Y - pSideNorm[0].Y;
            pTo.X = pSide[1].X - pSide[0].X;
            pTo.Y = pSide[1].Y - pSide[0].Y;
            if (pTo.X < 0)
            {
                pTo.X = -pTo.X;
                pTo.Y = -pTo.Y;
            }

            mCalcCrossAngle(pFrom, pTo, ref aS);

            //  Ideal 상태 대비하여 돌아가 있는 각도가 각각 aT, aS 이므로 보상하기위한 각도는 부호반전되어야 한다.
        }

        public Point2D[] CompensateTnPsi(Point2D topCOI, Point2D sideCOI, Point2D[] pT, Point2D[] pS, Point2D T, double Psi)
        {
            //  오른손좌표계, Top View, Side View 모두 각각의 COI ( Cener Of Image Sensor ) 를 원점으로 하여 변환 된 후의 좌표계에 대한 좌표값(스케일 보정완료)을 입력받는 것으로 가정한다.
            //  COI가 계속 변하므로 

            //  pT 는 Top View 에서 얻은 현재의 좌표값
            //  pS 는 Side View 에서 얻은 현재의 좌표값
            //  pN 은 Nominal 좌표값으로서, Top View 에서는 보이지 않고 Side View 에서만 보이는 마크에 대해서 처리할 때 활용한다.
            //  pN 은 Nominal 좌표값으로서, 단위는 Pixel 단위이어야 한다.
            //
            //  임의의 i 에 대하여, pT[i], pS[i], pN[i] 는 물리적으로 동일한 마크임을 전제로 한다.
            //  실제로는 pT[0] => pS[0], pT[1] => pS[2] 이다. 

            //  pS[0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  pS[1] : West , West  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  pS[2] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  pS[3] : East , East  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  pT[0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  pT[1] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.

            //  pT.Length < pS.Length == pN.Length 임을 전제로 한다.
            //  반환값은 T, Psi 가 소거된 (0 이었을 경우)상태에서의 Side View 에서의 각 마크의 좌표

            int pS_Length = pS.Length;
            Point2D[] pComp = new Point2D[pS_Length];   //  Side View 에서 T, Psi 가 소거된 좌표
            //double L = 0;
            int i = 0;

            pComp = new Point2D[pS_Length];   //  Side View 에서 T, Psi 가 소거된 좌표
            for (i = 0; i < pS_Length; i++)
            {
                if (pS[i] == null)
                {
                    pComp[i] = null;
                    continue;
                }
                pComp[i] = new Point2D(pS[i].X - T.X, pS[i].Y - T.Y * vSin40);
            }
            //Point2D[] sideCompTPsi = CompensatePsiToSideView(sideCOI, pComp, -Psi);
            Point2D[] sideCompTPsi = CompensatePsiToSideView(sideCOI, pComp, -Psi); //  이렇게 바꿔봄

            return sideCompTPsi;
        }

        public Point2D[] CompensatePsiToSideView(Point2D sideCOI, Point2D[] pImg, double angleRad)
        {
            //  COI 중심으로 회전시킨 좌표를 반환한다. COI 를 원점이 되게 이동한 다음 회전시키고 다시 COI 만큼 이동시킨다.
            //  Side View 인지 Top View 인지 알 필요 없다. 왜냐하면 Side View  로 얻은 좌표만 역회전시키므로.
            //  => 시뮬레이션 Data 로 재확인할 것.
            //  COI ( cener of image ) 정보와 angleRad 정보만 주어지면 View 에 관계없이 연산한다.
            //  pImg 는 영상 좌하단 모서리를 (0,0) 으로 하며, 영상위쪽으로 Y+, 영상오른쪽으로 X+ 를 따르는 좌표계임을 가정한다.
            //  영상 중심좌표 를 원점으로 하여 오른손좌표계 좌표로 변환한다.

            Point2D[] pICS = new Point2D[pImg.Length];

            //  ICs 중심기준으로 오른손좌표계 좌표변환
            for (int i = 0; i < pImg.Length; i++)
            {
                if (pImg[i] == null)
                {
                    pICS[i] = null;
                    continue;
                }
                pICS[i] = new Point2D(pImg[i].X - sideCOI.X, (pImg[i].Y - sideCOI.Y)/ vSin40);
            }

            Point2D[] resp = RotateAlongZ(pICS, angleRad);

            for (int i = 0; i < resp.Length; i++)
            {
                if (resp[i] == null)
                {
                    continue;
                }
                resp[i] = new Point2D(resp[i].X + sideCOI.X, resp[i].Y * vSin40 + sideCOI.Y);
            }

            return resp;
        }

        public Point2D[] CompensateViewRotation(Point2D COI, Point2D[] pImg, double angleRad)
        {
            //  COI 중심으로 회전시킨 좌표를 반환한다. COI 를 원점이 되게 이동한 다음 회전시키고 다시 COI 만큼 이동시킨다.
            //  Side View 인지 Top View 인지 알 필요 없다.
            //  COI ( cener of image ) 정보와 angleRad 정보만 주어지면 View 에 관계없이 연산한다.
            //  pImg 는 영상 좌하단 모서리를 (0,0) 으로 하며, 영상위쪽으로 Y+, 영상오른쪽으로 X+ 를 따르는 좌표계임을 가정한다.
            //  영상 중심좌표 를 원점으로 하여 오른손좌표계 좌표로 변환한다.

            Point2D[] pICS = new Point2D[pImg.Length];

            //  ICs 중심기준으로 오른손좌표계 좌표변환
            for ( int i=0; i< pImg.Length; i++)
            {
                if (pImg[i] == null)
                {
                    pICS[i] = null;
                    continue;
                }
                if (pImg[i].X == 0 && pImg[i].Y == 0)
                {
                    pICS[i] = null;
                    continue;
                }
                pICS[i] = new Point2D(pImg[i].X - COI.X, pImg[i].Y - COI.Y);
            }

            Point2D[] resp = RotateAlongZ(pICS, angleRad);

            for (int i = 0; i < resp.Length; i++)
            {
                if (resp[i] == null)
                {
                    continue;
                }
                if (resp[i].X == 0 && resp[i].Y == 0)
                {
                    resp[i] = null;
                    continue;
                }
                resp[i] = new Point2D(resp[i].X + COI.X, resp[i].Y + COI.Y);
            }

            return resp;
        }

        public Point2D[] mTop0 = null;
        public Point2D[] mSide0 = null;
        public double mPsi0 = 0;
        public double[,] M6DM = null;
        public double[,] M6DMwT = null;

        Point2D mOldT = new Point2D();


        public double mY1Y2_2Y3 = 0;
        public double m2Y1Y2_Y3 = 0;

        public void SetY1Y2_2Y3( double y1y2_2y3, double y1y2_y3_y1y2)
        {

            //  Y1 + Y2 + 2Y3
            mY1Y2_2Y3 = y1y2_2y3;
            m2Y1Y2_Y3 = y1y2_y3_y1y2;
        }
		
        int signTX = 1;
        int signTY  = 1;

        public void SetSignTXTY(bool negativeTX, bool negativeTY)
        {
            if (negativeTX)
                signTX = -1;
            else
                signTX = 1;

            if (negativeTY)
                signTY = -1;
            else
                signTY = 1;
        }

        public int mbCompY = 0;
        Point2D[] mSideStart = new Point2D[4];
        Point2D[] mTopStart = new Point2D[2];
        double mTX_0 = 0;
        double mTY_0 = 0;
        double mPixelToUm = 5.5 / 0.3;
        double mMinToRad = Math.PI / (60 * 180);
        bool mbGrabInitial = false;
        public void Extract6DMotion(int i, Point2D[] markT, Point2D[] markS,  ref Point2D T, ref double psi, ref double dZ, ref double TX, ref double TY, bool useCropGap = true)
        {
            //  marks 좌표는 기본적으로 오른손좌표계로 변환되어있음을 가정한다.
            Point2D[] pSideCompensateTnPsi = null;
            double[] err = new double[markT.Length];
            //double[,] invW = null;      //  M6DM
            //double[,] wT = null;        //  M6DMwT
            double[] ZTXTY = new double[3];

            //  markNorm 은 마크별 좌표값이 Nominal Value 인 배열, Nominal Value 는 Design Infomation 에서 User 가 입력한 값을 Pixel unit 로 변환한 값이어야 한다.
            //   아래 함수는 최초에 한번만 수행해주면 된다.
            //  CalcScaleTopNSide();            // 마스터 샘플로 Scale 보정되어있어야 한다.
            //  GetC12C13fromP12P13(pNom[0], pNom[1], pNom[2], ref c12, ref c13);
            //  TopNSideViewRotationAnlge()     // 마스터 샘플로 각 View 의 Rotation Angle 을 추출해놓아야 한다.
            //

            //  markS[][0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][1] : West , West  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][2] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markS[][3] : East , East  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markT[][0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
            //  markT[][1] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.

            //  다음은 본 함수 호출에 앞서서 1회 호출할 것
            //ZPhiThetaTransferMatrix(markNorm, out invW, out wT);
            //GetC12C13fromP12P13(markNorm[0], markNorm[2], markNorm[3], ref mC12, ref mC13);

            //  markT, markS 는 이미 오른손좌표계를 따르고 있어야 하고, 오른손좌표계에 의한 mCenterOfTop 와 mCenterOfSide 가 주어져야 한다.
            //  광학계의 구조적 각도오차를 보정하고 오른손좌표계, CIS ( Cener Of Image Sensor ) 로 변환 한다.
            //int debugCnt = 0;
            //if (i == 2)
            //    debugCnt = 2;


            //Point2D[] lpTop = CompensateViewRotation(mCenterOfImg, markT, mOpticsAngleTop);
            //Point2D[] lpSide = CompensateViewRotation(mCenterOfImg, markS, mOpticsAngleSide);
            if (markS[0].X == 0 || markS[2].X == 0 || markT[0].X == 0 || markT[1].X == 0) return;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  EastView Pixel Scale 보정
            markS[3].Y = (markS[3].Y - 285) * mEastviewYPscale + 285; //  285 은 Y = 0um 가 나오게하는 East View 의 Y 방향 중심위치(대략)
            // 실측치 : 285 ~ 286 PIXEL
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Point2D[] pTop = CompensateViewRotation(mCenterOfImg, markT, 0); 
            Point2D[] pSide = CompensateViewRotation(mCenterOfImg, markS, 0);

            if (useCropGap)
            {
                pTop[0].X -= ( 520 - mCropCgap ) / 2;    //  영상에서 잡힌 값에서 mCropCgap / 2 만큼 중심쪽으로 보내줘야 한다.
                pTop[1].X += ( 520 - mCropCgap ) / 2;    //  영상에서 잡힌 값에서 mCropCgap / 2 만큼 중심쪽으로 보내줘야 한다.
                //pTop[0].Y -= 30;    //  영상에서 잡힌 값에서 30 만큼 위쪽으로 보내줘야 한다.
                //pTop[1].Y -= 30;    //  영상에서 잡힌 값에서 30 만큼 위쪽으로 보내줘야 한다.
                pSide[0].Y += mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.
                pSide[2].Y += mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.
                pSide[3].Y -= mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.

            }

            Point2D[] pTopCopy = new Point2D[2];
            pTopCopy[0] = new Point2D(pTop[0].X, pTop[0].Y);
            pTopCopy[1] = new Point2D(pTop[1].X, pTop[1].Y);

            string sxtr = "";
            //  COI : Norminal Position of Center Of Image Sensor -> 궁극적으로는 실측된 각 마크 좌표와 Nominal 좌표로부터 COI 를 계산해서 그 값을 이용해야 한다.
            //Point2D COI = new Point2D(480, 169);    
            if (i == -1)
            {
                OrgByC12nC13(pSide[0], pSide[2], pSide[3], pTop[0], mC12, mC13, ref mCOIside, ref mCOItop);
                mTop0 = pTop;
                mSide0 = pSide;
                for (int s = 0; s < 4; s++)
                    mSideStart[s] = new Point2D();
                for (int s = 0; s < 2; s++)
                    mTopStart[s] = new Point2D();

                sxtr += "New mTop0\r\n";
            }
            else if (i == 0)
            {

                markT[0].X -= (520 - mCropCgap) / 2;    //  영상에서 잡힌 값에서 mCropCgap / 2 만큼 중심쪽으로 보내줘야 한다.
                markT[1].X += (520 - mCropCgap) / 2;    //  영상에서 잡힌 값에서 mCropCgap / 2 만큼 중심쪽으로 보내줘야 한다.
                //markT[0].Y -= 30;    //  영상에서 잡힌 값에서 30 만큼 위쪽으로 보내줘야 한다.
                //markT[1].Y -= 30;    //  영상에서 잡힌 값에서 30 만큼 위쪽으로 보내줘야 한다.
                markS[0].Y += mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.
                markS[2].Y += mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.
                markS[3].Y -= mCropABgap / 2;    //  영상에서 잡힌 값에서 mCropABgap / 2 만큼 중심쪽으로 보내줘야 한다.

                //qmCOIside, mCOItop 은 상수가 되어야 한다.  따라서 다음 코드는 불필요 250519
                //OrgByC12nC13(markS[0], markS[2], markS[3], markT[0], mC12, mC13, ref mCOIside, ref mCOItop);

                sxtr += "New mCOItop\r\n";
                //mTop0 = pTop;
                //mSide0 = pSide;
                //여기까지 초기화



                T = TnPsifromTopView(mCOItop, mTop0, pTop, mCOIside, mSide0, pSide, ref psi, ref err);   //  이 함수를 SideView, TopView 모두 이용하는 것으로 변경 필요   20230102
                mPsi0 = psi;
                pSideCompensateTnPsi = CompensateTnPsi(mCOItop, mCOIside, pTop, pSide, T, psi);
                CalcZPhiTheta(pSideCompensateTnPsi, mSide0, M6DM, M6DMwT, ref ZTXTY);

                //  수렴시키기 : TX 또는 TY 가 굉장히 클 때 ( 절대값 30min 이상 ) 의미가 있으나 그 이하에서는 영향 미미
                CompnesateTopViewWithTXTY(mCOItop, pTopCopy, ZTXTY[1], ZTXTY[2], ref pTop);
                T = TnPsifromTopView(mCOItop, mTop0, pTop, mCOIside, mSide0, pSide, ref psi, ref err);   //  이 함수를 SideView, TopView 모두 이용하는 것으로 변경 필요   20230102
                pSideCompensateTnPsi = CompensateTnPsi(mCOItop, mCOIside, pTop, pSide, T, psi);
                CalcZPhiTheta(pSideCompensateTnPsi, mSide0, M6DM, M6DMwT, ref ZTXTY);


                mSideStart = pSide;
                mTopStart = pTop;
            }
            else
            {
                //  mCOItop, mTop0, mCOIside, mSide0  값이   서로다른  Test 에서 유지되는지 확인할 것.
                T = TnPsifromTopView(mCOItop, mTop0, pTop, mCOIside, mSide0, pSide, ref psi, ref err);   //  이 함수를 SideView, TopView 모두 이용하는 것으로 변경 필요   20230102

                //  psi 보상은 조기회전상태에서 변화된 양 만큼만 보상해야 한다.
                //  따라서 psi - mPsi0 가 된다.
                pSideCompensateTnPsi = CompensateTnPsi(mCOItop, mCOIside, pTop, pSide, T, psi);
                CalcZPhiTheta(pSideCompensateTnPsi, mSide0, M6DM, M6DMwT, ref ZTXTY);

                //  수렴시키기 : TX 또는 TY 가 굉장히 클 때 ( 절대값 30min 이상 ) 의미가 있으나 그 이하에서는 영향 미미
                CompnesateTopViewWithTXTY(mCOItop, pTopCopy, ZTXTY[1], ZTXTY[2], ref pTop);
                T = TnPsifromTopView(mCOItop, mTop0, pTop, mCOIside, mSide0, pSide, ref psi, ref err);   //  이 함수를 SideView, TopView 모두 이용하는 것으로 변경 필요   20230102
                pSideCompensateTnPsi = CompensateTnPsi(mCOItop, mCOIside, pTop, pSide, T, psi);
                CalcZPhiTheta(pSideCompensateTnPsi, mSide0, M6DM, M6DMwT, ref ZTXTY);
            }

            T.X = -T.X;
            T.Y = -T.Y; // 모든 연산 뒤에 영상 위쪽으로 이동이 Y+ 되도록 조정함.

            dZ = -ZTXTY[0];// * vSec40;  //  입력이 Pixel unit 이면 결과도 Pixel unit, 입력이 mm unit 이면 결과도 mm unit, 위로 이동이 + 되도록 부호 조정함
            TX = ZTXTY[1];// * vSec40;  //  radian
            TY = -ZTXTY[2];// * vSec40;  //  radian

            //  CSH org 를 FOV 의 중심에 맞도록 offset 조정 241219
            T.Y -= 6;   //  Pixel

            psi = -psi;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  Debugging 용
            //string sstr = (T.X * mPixelToUm).ToString() + "," + (T.Y * mPixelToUm).ToString() +"," + (dZ * mPixelToUm).ToString() + "\r\n";
            //if (i % 16 == 1)
            //    File.AppendAllText("C:\\CSHTest\\DoNotTouch\\Admin\\CheckXCalScale_Before.csv", sstr);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //  이 다음단계에 Scale Factor 를 반영하는 방안
            //sxtr += mTop0[0].X.ToString("F3") + "," + markT[0].X.ToString("F3") + "," + pTop[0].X.ToString("F3") + ",";
            //sxtr += T.X.ToString("F4") + "," + (T.X * mPixelToUm).ToString("F3") + ",";
            T.X = T.X * T.X * mScaleX[0] * mPixelToUm + T.X * mScaleX[1] + mScaleX[2] / mPixelToUm;
            //sxtr += (T.X * mPixelToUm).ToString("F3") + ",";
            T.X -= T.Y * T.Y * mYtoXbyView[0] * mPixelToUm + T.Y * mYtoXbyView[1];// + mYtoXbyView[2] / mPixelToUm;
                                                                                  //sxtr += (T.X * mPixelToUm).ToString("F3") + ",";
                                                                                  /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                                  //  Debugging 용
                                                                                  //string sstr = (T.X * mPixelToUm).ToString() + ",";
                                                                                  //sstr += (T.X * mPixelToUm).ToString() + "," + (T.Y * mPixelToUm).ToString() + "\r\n";
                                                                                  //if ( i%16==1)
                                                                                  //    File.AppendAllText("C:\\CSHTest\\DoNotTouch\\Admin\\YtoXCoefVerification.csv", sstr);
                                                                                  /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //T.Y -= T.X * T.X * mXtoYbyView[0] * mPixelToUm + T.X * mXtoYbyView[1] + mXtoYbyView[2] / mPixelToUm;

            // offset X, Y, Z 추가 250325 -> mOffsetTZ, mOffsetTX, mOffsetTY 반영 맨 뒤로 이동
            //psi = psi * mScaleTZ[1] - mOffsetTZ;
            //TX = signTX * (TX - mOffsetTX)/ mMinToRad;  //  Convert Radian to min
            //TX = ( mScaleTX[0] * TX * TX + mScaleTX[1] * TX + mScaleTX[2]) * mMinToRad; // Convert min to radian
            //TY = signTY * (TY  * mScaleTY[1] - mOffsetTY);

            psi = psi * mScaleTZ[1];
            TX = TX / mMinToRad;  //  Convert Radian to min
            TX = (mScaleTX[0] * TX * TX + mScaleTX[1] * TX + mScaleTX[2]) * mMinToRad; // Convert min to radian
            TY = (TY * mScaleTY[1]);

            dZ = dZ * mPixelToUm * dZ * mScaleZ[0] + dZ * mScaleZ[1] + mScaleZ[2] / mPixelToUm;

            T.X -= dZ * dZ * mZtoXbyView[0] * mPixelToUm + dZ * mZtoXbyView[1];// + mZtoXbyView[2] / mPixelToUm;
            sxtr += (T.X * mPixelToUm).ToString("F3") + "," + (T.Y * mPixelToUm).ToString("F3") + "," + (dZ * mPixelToUm).ToString("F3") + "\r\n";
            T.Y -= dZ * dZ * mZtoYbyView[0] * mPixelToUm + dZ * mZtoYbyView[1];// + mZtoYbyView[2] / mPixelToUm;
            dZ -= T.Y * T.Y * mYtoZbyView[0] * mPixelToUm + T.Y * mYtoZbyView[1]// + mYtoZbyView[2] / mPixelToUm
                + T.X * T.X * mXtoZbyView[0] * mPixelToUm + T.X * mXtoZbyView[1];// + mXtoZbyView[2] / mPixelToUm;

            T.Y = T.Y * T.Y * mScaleY[0] * mPixelToUm + T.Y * mScaleY[1] + mScaleY[2] / mPixelToUm;// - 6;    //Pixel Unit
            T.Y -= T.X * T.X * mXtoYbyView[0] * mPixelToUm + T.X * mXtoYbyView[1];// + mXtoYbyView[2] / mPixelToUm;


            // sstr = (T.X * mPixelToUm).ToString() + "," + (TX / mMinToRad).ToString() + ",";

            TX -= mXtoTXbyView[1] * T.X / 187.5135 + mYtoTXbyView[1] * T.Y / 187.5135 + mZtoTXbyView[1] * dZ / 187.5135;
            TY -= mXtoTYbyView[1] * T.X / 187.5135 + mYtoTYbyView[1] * T.Y / 187.5135 + mZtoTYbyView[1] * dZ / 187.5135;
            psi -= mXtoTZbyView[1] * T.X / 187.5135 + mYtoTZbyView[1] * T.Y / 187.5135 + mZtoTZbyView[1] * dZ / 187.5135;

            //sstr += (TX / mMinToRad).ToString() + "\r\n";
            //if (i % 16 == 3)
            //    File.AppendAllText("C:\\CSHTest\\DoNotTouch\\Admin\\X_Vrfy.csv", sxtr);

            psi -= mTYtoTZbyView[1] * TY;
            TX -= mTYtoTXbyView[1] * TY;

            TY -= mTXtoTYbyView[1] * TX;
            psi -= mTXtoTZbyView[1] * TX;

            TX -= mTZtoTXbyView[1] * psi;
            TY -= mTZtoTYbyView[1] * psi;
            //  TX,TY,TZ 는 Radian
            //  X,Y,Z 는 pixel

            dZ -= mTZtoZbyView[1] * psi * 187.5135;    // TZtoZ 추가 250217

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  Debugging 용
            //sstr = (T.X * mPixelToUm).ToString() + "," + (T.Y * mPixelToUm).ToString() + "," + (dZ * mPixelToUm).ToString() + "\r\n";
            //if (i % 16 == 1)
            //    File.AppendAllText("C:\\CSHTest\\DoNotTouch\\Admin\\CheckXCalScale_After.csv", sstr);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (i == 0)
            {
                mTX_0 = TX;
                mTY_0 = TY;

                mOldT = T;
            }
            if ( mbMasterCCS)
            {
                TX = TX - mMasterCCS[0];
                TY = TY - mMasterCCS[1];
                psi = psi - mMasterCCS[2];
                T.X = T.X + mMasterCCS[2] * T.Y - mMasterCCS[1] * dZ;
                T.Y = T.Y - mMasterCCS[2] * T.X + mMasterCCS[0] * dZ;
                dZ = dZ - mMasterCCS[0] * T.Y+ mMasterCCS[1] * T.X;
            }

            T.X += mXJtoX[0] * T.X * T.Y * mPixelToUm  + mXJtoX[1] * T.X * dZ * mPixelToUm ;
            T.Y += mYJtoY[0] * T.Y * T.X * mPixelToUm + mYJtoY[1] * T.Y * dZ * mPixelToUm;
            dZ += mZJtoZ[0] * dZ * T.X * mPixelToUm + mZJtoZ[1] * dZ * T.Y * mPixelToUm;
            //psi = 1.008 * psi;    // 241216 // TZ Scale 사용하므로 제거

            // offset X, Y, Z 추가 250325
            T.X -= mOffsetX;
            T.Y -= mOffsetY;
            dZ-= mOffsetZ;
            
            TX = signTX * TX;
            TY = signTY * TY;

            TX -= (signTX > 0 ? mOffsetTX : -mOffsetTX);
            TY -= (signTY > 0 ? mOffsetTY : -mOffsetTY);
            psi -= mOffsetTZ;

            if (mbApplyEuler)
            {
                double[] before = new double[3] { T.X, T.Y, dZ };
                double[] after = new double[3];
                MatrixCross(ref mEulerMatrix, ref before, ref after, 3);
                T.X = after[0];
                T.Y = after[1];
                dZ = after[2];
            }
        }

        public void TransferByEulerMatrix(int count, ref double[] xArr, ref double[] yArr, ref double[] zArr)
        {
            for (int i=0; i<count; i++)
            {
                double[] before = new double[3] { xArr[i], yArr[i], zArr[i] };
                double[] after = new double[3];
                MatrixCross(ref mEulerMatrix, ref before, ref after, 3);
                xArr[i] = after[0];
                yArr[i] = after[1];
                zArr[i] = after[2];
            }
        }

        public double[,] mEulerMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        public bool mbApplyEuler = false;
        public void SetEulerMatrix(double[] eulerAngle)
        {
            RotationEuler(eulerAngle[0], eulerAngle[1], eulerAngle[2], ref mEulerMatrix);
        }
        public void ResetScales()
        {
            mScaleX = new double[3] { 0, 1, 0 };
            mScaleY = new double[3] { 0, 1, 0 };
            mScaleZ = new double[3] { 0, 1, 0 };

            mScaleTX = new double[3] { 0, 1, 0 };
            mScaleTY = new double[3] { 0, 1, 0 };
            mScaleTZ = new double[3] { 0, 1, 0 };
        }


        public void SetScales(double[] sX, double[] sY, double[] sZ, double[] sTX, double[] sTY, double[] sTZ, double sEY,
                              double[] sX2Y, double[] sX2Z, double[] sX2TX, double[] sX2TY, double[] sX2TZ,
                              double[] sY2X, double[] sY2Z, double[] sY2TX, double[] sY2TY, double[] sY2TZ,
                              double[] sZ2X, double[] sZ2Y, double[] sZ2TX, double[] sZ2TY, double[] sZ2TZ,
                              double[] sTX2TY, double[] sTX2TZ,
                              double[] sTY2TX, double[] sTY2TZ,
                              double[] sTZ2TX, double[] sTZ2TY,
                              double[] sXJ2X, double[] sYJ2Y, double[] sZJ2Z,
                              double[] sTZ2Z)

        {
            mEastviewYPscale = sEY;        

            for ( int i=0; i< 3; i++)
            {
                mScaleX[i] = sX[i];
                mScaleY[i] = sY[i];
                mScaleZ[i] = sZ[i];
                mScaleTX[i] = sTX[i];
                mScaleTY[i] = sTY[i];
                mScaleTZ[i] = sTZ[i];

                mXtoYbyView[i] = sX2Y[i];
                mXtoZbyView[i] = sX2Z[i];
                mXtoTXbyView[i] = sX2TX[i];
                mXtoTYbyView[i] = sX2TY[i];
                mXtoTZbyView[i] = sX2TZ[i];
                mYtoXbyView[i] = sY2X[i];
                mYtoZbyView[i] = sY2Z[i];
                mYtoTXbyView[i] = sY2TX[i];
                mYtoTYbyView[i] = sY2TY[i];
                mYtoTZbyView[i] = sY2TZ[i];
                mZtoXbyView[i] = sZ2X[i];
                mZtoYbyView[i] = sZ2Y[i];
                mZtoTXbyView[i] = sZ2TX[i];
                mZtoTYbyView[i] = sZ2TY[i];
                mZtoTZbyView[i] = sZ2TZ[i];
                mTXtoTYbyView[i] = sTX2TY[i];
                mTXtoTZbyView[i] = sTX2TZ[i];
                mTYtoTXbyView[i] = sTY2TX[i];
                mTYtoTZbyView[i] = sTY2TZ[i];
                mTZtoTXbyView[i] = sTZ2TX[i];
                mTZtoTYbyView[i] = sTZ2TY[i];
                mTZtoZbyView[i] = sTZ2Z[i];
            }
            for (int i = 0; i < 2; i++)
            {
                mXJtoX[i] = sXJ2X[i];
                mYJtoY[i] = sYJ2Y[i];
                mZJtoZ[i] = sZJ2Z[i];
            }

            //mScaleZ   = sZ;

            //mScaleTY  = sTY;
            //mScaleTZ = 1;// sY[1] / sX[1];

            //mZtoXbyView = sZ2X;
            //mZtoYbyView = sZ2Y;

            //mYtoXbyView = sY2X;
            //mYtoZbyView = new double[3];
            //if (sY2Z!=null)
            //{
            //    mYtoZbyView[0] = sY2Z[0];
            //    mYtoZbyView[1] = sY2Z[1];
            //    mYtoZbyView[2] = sY2Z[2];
            //}

            //mXtoYbyView = sX2Y;
            //mXtoZbyView = new double[3];
            //if (sX2Z!=null)
            //{
            //    mXtoZbyView[0]  = sX2Z[0];
            //    mXtoZbyView[1]  = sX2Z[1];
            //    mXtoZbyView[2]  = sX2Z[2];
            //}

            //mEastviewYPscale = sEY;
            //mXtoTXbyView = sX2TX;
        }

        // offset X, Y, Z 추가 250325
        double mOffsetX = 0;
        double mOffsetY = 0;
        double mOffsetZ = 0;

        double mOffsetTX = 0;
        double mOffsetTY = 0;
        double mOffsetTZ = 0;

        public void SetTXTYOffset(double x, double y, double z, double tX, double tY, double tZ)
        {
            // offset X, Y, Z 추가 250325

            // x, y, z : Pixel
            // tx, ty, tz : rad
            
            mOffsetX = x; 
            mOffsetY = y;
            mOffsetZ = z;
            mOffsetTX = tX;
            mOffsetTY = tY;
            mOffsetTZ = tZ;
        }

        public void GetTXTYOffset(ref double x, ref double y, ref double z, ref double tX, ref double tY, ref double tZ)
        {
            // offset X, Y, Z 추가 250325
            x = mOffsetX;
            y = mOffsetY;
            z = mOffsetZ;
            tX = mOffsetTX ;
            tY = mOffsetTY ;
            tZ = mOffsetTZ ;

        }

        Point2D[] mMarkNorm = null;
        public void ZPhiThetaTransferMatrix(Point2D[] markNorm, out double[,] trm, out double[,] wT)
        {
            //  markNorm 은 Nominal Value (명목좌표 또는 설계좌표) 이어야 한다. 절대값 자체가 중요하다.
            //  markNorm 은 Side View 에 보이는 마크 이어야 한다.

            //  markNorm[][0] : North, North Mark 가 2개 인 경우 이미 함수 호출 전에 평균취해서 1개로 축약해야한다.
            //  markNorm[][1] : West , West  Mark 가 2개 인 경우 이미 함수 호출 전에 평균취해서 1개로 축약해야한다.
            //  markNorm[][2] : South, South Mark 가 2개 인 경우 이미 함수 호출 전에 평균취해서 1개로 축약해야한다.
            //  markNorm[][3] : East , East  Mark 가 2개 인 경우 이미 함수 호출 전에 평균취해서 1개로 축약해야한다.

            mMarkNorm = new Point2D[markNorm.Length];

            double[,] w = new double[3, markNorm.Length];
            double[,] w33 = new double[3, 3];
            wT = null;
            int effLength = 0;
            double offsetNSmark = markNorm[0].Y;
            for (int i = 0; i < markNorm.Length; i++)
            {
                if (markNorm[i] == null)
                    continue;
                mMarkNorm[i] = new Point2D(markNorm[i].X, markNorm[i].Y);

                w[effLength, 0] = -vCos40;
                w[effLength, 1] = -vCos40 * markNorm[i].Y;
                w[effLength, 2] =  vCos40 * markNorm[i].X;

                effLength++;
            }
            if (effLength > 3)
            {
                wT = new double[effLength, 3];
                w33 = new double[3, 3];
                MatrixTranspose(w, ref wT, effLength, 3);
                MatrixCross(ref w, ref wT, ref w33, 3, effLength);
                trm = MatrixInverse(w33, 3);
            }
            else if (effLength == 3)
                trm = MatrixInverse(w, 3);
            else
                trm = null;
        }

        //public void ZPhiThetaTransferMatrix(Point2D[] markNorm, out double[,] trm, out double[,] wT)
        //{
        //    //  markNorm 은 Nominal Value (명목좌표 또는 설계좌표) 이어야 한다. 절대값 자체가 중요하다.
        //    //  markNorm 은 Side View 에 보이는 마크 이어야 한다.

        //    //  markNorm[][0] : North, North Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
        //    //  markNorm[][1] : West , West  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
        //    //  markNorm[][2] : South, South Mark 가 2개 인 경우 평균취해서 1개로 축약한다.
        //    //  markNorm[][3] : East , East  Mark 가 2개 인 경우 평균취해서 1개로 축약한다.

        //    double[,] w = new double[3, markNorm.Length];
        //    double[,] w33 = new double[3, 3];
        //    wT = null;
        //    int effLength = 0;
        //    double mm2Pixel = 1 / (0.0055 / Global.LensMag);

        //    for (int i = 0; i < markNorm.Length; i++)
        //    { 
        //        if (markNorm[i] == null)
        //            continue;

        //        w[0, effLength] = 1;
        //        w[1, effLength] = markNorm[i].Y * mm2Pixel;
        //        w[2, effLength] = -markNorm[i].X * mm2Pixel;
        //        effLength++;
        //    }
        //    if (effLength > 3)
        //    {
        //        wT = new double[effLength, 3];
        //        w33 = new double[3, 3];
        //        MatrixTranspose(w, ref wT, effLength, 3);
        //        MatrixCross(ref w, ref wT, ref w33, 3, effLength);
        //        trm = MatrixInverse(w33, 3);
        //    }
        //    else if (effLength == 3)
        //        trm = MatrixInverse(w, 3);
        //    else
        //        trm = null;
        //}

        public void CalcZPhiTheta(Point2D[] pSide, Point2D[] pSide0, double[,] invW, double[,] wT, ref double[] ZTXTY)
        {
            //  pSide, pSide0 모두  Side View 에서 측정된 Pixel 좌표값이어야 한다. ( 추후 Cos(40) 보정을 수행하고있다)
            //  입력값이 mm 단위면 mm. rad 로 결과 나오고
            //  입력값이 pixel 단위면, pixel, rad 로 결과 나온다.

            //  아래 방식으로 하면 East Mark 의 X 좌표값이 TY 값을 구하는데 영향을 크게 준다.\
            //  East Mark 의 X 좌표값의 영향을 줄이기 위해서는.. 
            //  East Mark 의 X 좌표는 측정값을 버리고 Nominal 값을 취하는 방법이 있다. -->Translation 이 적용이 안되게 되는 문제로 인해 다른 문제 발생.
            double[] dYi = new double[pSide.Length];
            int effLength = 0;
            int minLength = Math.Min(pSide.Length, pSide0.Length);
            for (int j = 0; j < minLength; j++)
            {
                if (pSide0[j] == null) continue;
                if (pSide[j] == null) continue;
                dYi[effLength] = pSide[j].Y - pSide0[j].Y;
                effLength++;
            }
            if (effLength < 3)
                return;

            if (wT != null)
            {
                double[] psuedoY = new double[3];
                //MatrixCross(ref dYi, ref wT, ref psuedoY, effLength, 3);
                //MatrixCross(ref psuedoY, ref invW, ref ZTXTY, 3, 3);
                MatrixCross(ref wT, ref dYi, ref psuedoY, effLength, 3);
                MatrixCross(ref invW, ref psuedoY, ref ZTXTY, 3, 3);
            }
            else
                MatrixCross(ref invW, ref dYi, ref ZTXTY, 3, 3);
        }
        public double SpatialDFT(double[] x, double[] timespace, out double[] amp, out double[] phase, int startIndex, int len)
        {
            amp = new double[len];
            phase = new double[len];
            Complex[] fx = new Complex[len];
            double[] w = new double[len];
            double[] intpX = new double[len];

            double[] forSort = new double[len];
            double[] uniformTS = new double[len];
            double[] sortX = new double[len];

            Array.Copy(timespace, startIndex, forSort, 0, len);
            Array.Copy(x, startIndex, sortX, 0, len);
            Array.Sort(forSort, sortX); //  더 빠른 함수로 변경 필요  231116
            double timespaceStep = (forSort[len - 1] - forSort[0]) / (len - 1);
            //MessageBox.Show(forSort[0].ToString("F3") + " " + forSort[len - 1].ToString("F3") + " " + len.ToString());

            //  lagrange interpolation 을 통해서 등시간격/등공간간격으로 데이터를 가공한 뒤 처리해야 한다.
            //  그렇지 않으면 Peak 가 뭉게지며, Noise 가 큰 경우 Noise에 가려 안보이게 될 수 있다.
            //  평균과 기울기를 소거해야 한다.
            double avgX = 0;
            for (int i = 0; i < len; i++)
            {
                w[i] = (i + 1) * 2 * Math.PI / len; // 주파수 -> 이 배열은 불변
                uniformTS[i] = forSort[0] + i * timespaceStep;

                avgX += x[i + startIndex];
            }
            double slope = (sortX[len - 1] - sortX[0]) / (forSort[len - 1] - forSort[0]);
            avgX = avgX / len - slope * timespaceStep * len / 2 + 0.17;

            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            //   아래 검증 완료
            for (int i = 0; i < len - 1; i++)
            {
                for (int p = startIndex; p < startIndex + len; p++)
                {
                    if (uniformTS[i] >= timespace[p] && uniformTS[i] < timespace[p + 1])
                    {
                        intpX[i] = x[p] - avgX - slope * (uniformTS[i] - uniformTS[0]) + (uniformTS[i] - timespace[p]) * (x[p + 1] - x[p]) / (timespace[p + 1] - timespace[p]);
                        break;
                    }
                    else if (uniformTS[i] < timespace[p] && uniformTS[i] > timespace[p + 1])
                    {
                        intpX[i] = x[p + 1] - avgX - slope * (uniformTS[i] - uniformTS[0]) + (uniformTS[i] - timespace[p + 1]) * (x[p] - x[p + 1]) / (timespace[p] - timespace[p + 1]);
                        break;
                    }
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                x[i + startIndex] = intpX[i];
                timespace[i + startIndex] = uniformTS[i];
            }
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////

            //for (int i = 0; i < len - 1; i++)
            //{
            //    x[i + startIndex] = x[i + startIndex] - 1.5*avgX - 2.2 - slope * (timespace[startIndex+i] - timespace[startIndex]);
            //}

            for (int i = 0; i < len; i++)
            {
                for (int k = 0; k < len - 1; k++)
                {
                    //Complex exp_iy = new Complex(0, -w[i] * timespaceStep * k);

                    //////////////////////////////////////////////////////////////////////////
                    //   아래 2줄은 검증 완료, timespace 가 반드시 등간격이어야 제대로된 결과가 나온다.
                    Complex exp_iy = new Complex(0, -w[i] * uniformTS[k]);
                    fx[i] += intpX[k] * Complex.Exp(exp_iy);
                    //////////////////////////////////////////////////////////////////////////

                    //////////////////////////////////////////////////////////////////////
                    ////   아래 2줄은 검증 필요 -> 이건 적용불가. 
                    //Complex exp_iy = new Complex(0, -w[i] * timespace[k]);
                    //fx[i] += x[k] * Complex.Exp(exp_iy);// * (timespace[k + 1] - timespace[k]);
                    //////////////////////////////////////////////////////////////////////
                }
            }
            for (int i = 0; i < len; i++)
            {
                amp[i] = fx[i].Magnitude;
                phase[i] = fx[i].Phase * 180 / Math.PI;
            }
            return 0.5;   //  결과가 Wavelength 에서 Peak 로 나타남
            //  333개가 1주기인 경우 peak 는 333
            //  467개가 1주기인 경우 peak 는 467
        }


        /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////
        ///// <summary>
        /////     New Formula to calculate X,Y,Z,TX,TY,TZ of the cnter of fiducial mark


        public double[] CalcRealXYTZfromProbes(double X1, double X2, double Y1, double Y2, double Lp2pAbs = 40, double RxAbs = 32, double RyAbs = 47)
        {
            //  X1 : X on center
            //  X2 : Lp2p from X1
            //  Y1 : Y on center
            //  Y2 : Lp2p from Y1
            //  Rx : Center to Probe X1
            //  Ry : Center to Probe Y1
            //
            //  아래는 현재 Rotation Stage 위의 Bracket 설계치수 -> 향후 40, 47, 47 로 Bracket 변경 예정
            //
            //  Lp2p    = 40 mm
            //  Rx      = -32 mm
            //  Ry      = 47 mm
            //
            double ofx = 0;//xAbs - 32;
            double ofy = 0;//RyAbs - 32;

            double t = (-(X2 - X1) + Y2 - Y1) / (2 * Lp2pAbs);
            double psi = Math.Atan(t);  //  Radian
            double Ax = -(RxAbs - ofx) * Math.Sin(psi) * t + -(RxAbs - ofx) * Math.Cos(psi);
            double Ay = (RyAbs-ofy) * Math.Sin(psi) * t + (RyAbs - ofy) * Math.Cos(psi);
            double[] XYTZ = new double[3];

            double tipR = 1.5;
            double delta = tipR * (1 / Math.Cos(psi) - 1);
            //  X probe 는 Stage 사각형의 좌측변(X = -32mm) 에 접촉하고 있고
            //  Y probe 는 Stage 사각형의 상측변(Y = 47mm) 에 접촉하고 있다
            X1 = X1 + delta;    //  좌측 변(X 좌표 음수) 에서 경계점은 Probe 측정점보다 우측으로 이동해있다. 
            X2 = X2 + delta;    //  좌측 변(X 좌표 음수) 에서 경계점은 Probe 측정점보다 우측으로 이동해있다. 
            Y1 = Y1 - delta;    //  상측 변(Y 좌표 양수) 에서 경계점은 Probe 측정점보다 하측으로 이동해있다. 
            Y2 = Y2 - delta;    //  상측 변(Y 좌표 양수) 에서 경계점은 Probe 측정점보다 하측으로 이동해있다. 

            XYTZ[0] = (Y1 * t + X1 - Ay * t - Ax) / (t * t + 1) + ofx;
            XYTZ[1] = (Y1 - t * X1 + Ax * t - Ay) / (t * t + 1) + ofy;
            XYTZ[2] = psi;
            return XYTZ;
        }

        public double mLFX0 = 0;  //  Head 별로 변경되어야 하는 값
        public double mLFY0 = 0;  //  Head 별로 변경되어야 하는 값
        public double mProbeTYL1 = 55;  //  Head 별로 변경되어야 하는 값
        public double mProbeTYL2 = 55;  //  Head 별로 변경되어야 하는 값
        public double mProbeTXL1 = 0;  //  Head 별로 변경되어야 하는 값
        public double mProbeTXL2 = 55;  //  Head 별로 변경되어야 하는 값
        public double mProbeXRx = 32;   //  Head 별로 변경되어야 하는 값  // 32
        public double mProbeYRy = 32;   //  Head 별로 변경되어야 하는 값  // 47

        public double[] CalcRealTXTYZfromProbes(double Z1, double Z2, double Z3, double X, double Y, double psi)
        {
            //  Z1 : probe Z1 @ - X
            //  Z2 : probe Z2 @ + X
            //  Z3 : probe Z3 @ + Y

            double[,] R = new double[3, 3];
            RotationZ(psi, ref R);
            double[] unitX = new double[3] { 1, 0, 0 };
            double[] unitRotatedX = new double[3];
            double[] unitY = new double[3] { 0, 1, 0 };
            double[] unitRotatedY = new double[3];
            MatrixCross(ref R, ref unitX, ref unitRotatedX, 3);
            MatrixCross(ref R, ref unitY, ref unitRotatedY, 3);

            double[] TXTYZ = new double[3];
            
            double Rp = 55; //  Distance(um) between Z Probes and center

            double c = Math.Cos(psi);
            double s = Math.Sin(psi);

            Point3d[] lZprobes = new Point3d[3];
            //lZprobes[0] = new Point3d(-55.66963123, -0.541726,              Z1);
            //lZprobes[1] = new Point3d(54.33036877,  -0.541726,              Z2);
            //lZprobes[2] = new Point3d(0,            Rp,    Z3);

            lZprobes[0] = new Point3d(-mProbeTYL1, mProbeTXL1, Z1); //  Stage 및 Head 에 의해 결정되는 값 - Probe 의 절대위치
            lZprobes[1] = new Point3d( mProbeTYL2, mProbeTXL1, Z2); //  Stage 및 Head 에 의해 결정되는 값 - Probe 의 절대위치
            lZprobes[2] = new Point3d(0, mProbeTXL2, Z3);            //  Stage 및 Head 에 의해 결정되는 값 - Probe 의 절대위치

            double[] ABCD = PlaneEquationFrom3Pts(lZprobes[0], lZprobes[1], lZprobes[2]);
            double angle = AngleBtwPlaneAndZ(ABCD);
            double tipR = 1.5;
            double delta = tipR * (1 / Math.Cos(angle) - 1);
            //  Stage 에 Tilt 가 발생하면 Fiducial 마크는 Probe 측정값보다 아래에 있게 된다.
            //  일단 영상 위쪽을 + 방향을 잡는다.

            Z1 = Z1 - delta;    //  Z1 probe 는 위에서 아래를 향하므로  
            Z2 = Z2 - delta;    //  Z2 probe 는 위에서 아래를 향하므로
            Z3 = Z3 + delta;    //  Z3 probe 는 아래에서 위를 향하므로


            double innerProductY = unitRotatedX[0] * ABCD[0] + unitRotatedX[1] * ABCD[1] + unitRotatedX[2] * ABCD[2];
            TXTYZ[1] = Math.Asin(innerProductY);
            double innerProductX = unitRotatedY[0] * ABCD[0] + unitRotatedY[1] * ABCD[1] + unitRotatedY[2] * ABCD[2];
            TXTYZ[0] = Math.Asin(innerProductX);
            TXTYZ[2] = -(ABCD[0] * (X + mLFX0) + +ABCD[1] * (Y + mLFY0) + ABCD[3]) / ABCD[2];
            //TXTYZ[2] = -(ABCD[0] * X + ABCD[1] * Y + ABCD[3]) / ABCD[2];

            return TXTYZ;
        }
        public void RotationZ(double psi_rad /* Z rotation */, ref double[,] Rr)
        {
            double cp = 0;
            double sp = 0;
            double[,] Rz = new double[3, 3];

            cp = Math.Cos(psi_rad);
            sp = Math.Sin(psi_rad);

            ////Rz = [cp - sp 0; sp cp 0; 0 0 1];
            Rr[0, 0] = cp; Rr[0, 1] = -sp;
            Rr[1, 0] = sp; Rr[1, 1] = cp;
            Rr[2, 2] = 1;
        }
        public void RotationZ2x2(double psi_rad /* Z rotation */, ref double[,] Rr)
        {
            double cp = 0;
            double sp = 0;

            cp = Math.Cos(psi_rad);
            sp = Math.Sin(psi_rad);

            ////Rz = [cp - sp 0; sp cp 0; 0 0 1];
            Rr[0, 0] = cp; Rr[0, 1] = -sp;
            Rr[1, 0] = sp; Rr[1, 1] = cp;
        }
        public double[] PlaneEquationFrom3Pts(Point3d p1, Point3d p2, Point3d p3)
        {
            //  A = p2 – p1, B = p3 – p1
            //  (X-p1) * ( A X B ) = 0
            // ABCD[0] x + ABCD[1] y + ABCD[2] z + ABCD[3] = 0

            double[] ABCD = new double[4];
            double[] A = new double[3];
            double[] B = new double[3];
            double[] AB = new double[3];
            A[0] = p2.X - p1.X;
            A[1] = p2.Y - p1.Y;
            A[2] = p2.Z - p1.Z;
            B[0] = p3.X - p1.X;
            B[1] = p3.Y - p1.Y;
            B[2] = p3.Z - p1.Z;
            AB[0] = A[1] * B[2] - A[2] * B[1];
            AB[1] = A[2] * B[0] - A[0] * B[2];
            AB[2] = A[0] * B[1] - A[1] * B[0];
            double L = Math.Sqrt(AB[0] * AB[0] + AB[1] * AB[1] + AB[2] * AB[2]);

            ABCD[0] = AB[0] / L;
            ABCD[1] = AB[1] / L;
            ABCD[2] = AB[2] / L;
            ABCD[3] = (-AB[0] * p1.X - AB[1] * p1.Y - AB[2] * p1.Z) / L;
            return ABCD;
        }
        public double AngleBtwPlaneAndZ(double[] A)
        {
            // 평면이 Z 축과 이루는 각도
            double L = Math.Sqrt(A[0] * A[0] + A[1] * A[1] + A[2] * A[2]);
            double res = Math.Acos(A[2] / L);

            return res; // radian;
        }
        public  double[] TransferPhiThetaPsifromAcstoBcs(double[] lPhiThetaPsiArcmin)
        {
            //  Refer to ChatGPT

            // 사용자 입력 회전각 (A 좌표계 기준)
            double a_rad = lPhiThetaPsiArcmin[0] * mMinToRad;  // X축 회전
            double b_rad = lPhiThetaPsiArcmin[1] * mMinToRad; // Y축 회전
            double c_rad = lPhiThetaPsiArcmin[2] * mMinToRad; // Z축 회전


            // A 기준 회전 행렬 (Z * Y * X 순서)
            Matrix4x4 Rx = Matrix4x4.CreateRotationX((float)a_rad);
            Matrix4x4 Ry = Matrix4x4.CreateRotationY((float)b_rad);
            Matrix4x4 Rz = Matrix4x4.CreateRotationZ((float)c_rad);
            Matrix4x4 R_A = Rz * Ry * Rx; // 순서: X → Y → Z

            // [3] A → B 좌표계 회전 (X축 기준 -45도)
            Matrix4x4 R_AB = Matrix4x4.CreateRotationX((float)(- Math.PI / 4));

            // [4] B 기준 회전행렬 = R_AB^T * R_A
            Matrix4x4 R_AB_T = Matrix4x4.Transpose(R_AB);
            Matrix4x4 R_B = R_AB_T * R_A * R_AB;

            // [5] 회전행렬 → Quaternion → 3x3 행렬 for alglib
            Quaternion q = Quaternion.CreateFromRotationMatrix(R_B);
            double[] eulerAngle = QuaternionToEulerAngles(q);

            // [7] 라디안 → Arcmin 단위 변환
            double[] res = new double[3];
            res[0] = eulerAngle[0] / mMinToRad;
            res[1] = eulerAngle[1] / mMinToRad;
            res[2] = eulerAngle[2] / mMinToRad;

            return res;
        }
        public double[] QuaternionToEulerAngles(Quaternion q)
        {
            //  Refer to https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            double[] angles = new double[3];

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles[0] = Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            sinp = sinp > 1 ? 1 : sinp;
            sinp = sinp < -1 ? -1 : sinp;
            angles[1] = Math.Asin(sinp); // Y 회전 (Pitch)

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles[2] = Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public Matrix4x4 RotationYforBcs = new Matrix4x4();
        public void InitializeRotationXforBcs()
        {
            //  자화에서의 Y 축은 CSHead 에서는 X 축에 해당함
            //  자화에서의 Z 축은 CSHead 에서의 좌표계를 X 축으로 -45 deg 회전한 Z 축에 대해 부호가 반대임.

            //  WCS 를 X 축으로 -45도 회전시킨 좌표계
            ////float angleDegrees = -45.0f;
            ////float angleRadians = angleDegrees * (float)Math.PI / 180f;

            ////// 회전행렬: X축 기준 회전
            ////Matrix4x4 rotationY = Matrix4x4.CreateRotationY(angleRadians);

            //  좌표계를 X 축방향으로 -45도 돌리려면, 아래 식에서는 +45deg 를 넣어줘야 한다.
            //  즉 모든 좌표를 Y 축방향으로 +45deg 돌려주는 Matrix 를 준비한다.
            RotationYforBcs = Matrix4x4.CreateRotationX((float)Math.PI/4); 
        }


        public Vector3[] ConvertPtsOnAcsToPtsOnBcs(Vector3[] PtinAcs)
        {
            int len = PtinAcs.Length;
            Vector3[] res = new Vector3[len];

            // 점 P를 4D 벡터로 확장 (동차 좌표계 형태)
            for ( int i=0; i<len; i++)
                res[i] = Vector3.Transform(PtinAcs[i], RotationYforBcs);

            return res;
        }
        public Vector3 ConvertPtsOnAcsToPtsOnBcs(Vector3 PtinAcs)
        {
            return Vector3.Transform(PtinAcs, RotationYforBcs);
        }

        public void TransferPhiThetaPsifromAcstoBcs(ref double[] lPhiThetaPsiArcmin)
        {
            double a = 0.707106781 * (lPhiThetaPsiArcmin[0] + lPhiThetaPsiArcmin[2]);   
            double b = lPhiThetaPsiArcmin[1];
            double c = 0.707106781 * (lPhiThetaPsiArcmin[2] - lPhiThetaPsiArcmin[0]);

            lPhiThetaPsiArcmin[0] = a;
            lPhiThetaPsiArcmin[1] = b;
            lPhiThetaPsiArcmin[2] = c;
        }

        public void CoordinateSystemTransformationVerification()
        {
            //  Create a few Points in WCS
            Vector3[] splPts = new Vector3[6];
            splPts[0] = new Vector3(1000, 0, 0);
            splPts[1] = new Vector3(-1000, 0, 0);
            splPts[2] = new Vector3(0, 2000, 0);
            splPts[3] = new Vector3(0, 1000, 0);
            splPts[4] = new Vector3(0, -1000, 0);
            splPts[5] = new Vector3(2000, 0, 0);

            //  Create a few angle combination  ( Arcmin )
            //  회전하기 전의 A 좌표계에서의 좌표
            double[][] PhiThetaPsi = new double[16][];
            PhiThetaPsi[0] = new double[3] { 0, 0, 0 };
            PhiThetaPsi[1] = new double[3] { -30, 0, 0 };
            PhiThetaPsi[2] = new double[3] { 30, 0, 0 };
            PhiThetaPsi[3] = new double[3] { 0, 30, 0 };
            PhiThetaPsi[4] = new double[3] { 0, -30, 0 };
            PhiThetaPsi[5] = new double[3] { 0, 0, -50 };
            PhiThetaPsi[6] = new double[3] { 0, 0, 50 };
            PhiThetaPsi[7] = new double[3] { 20, 60, 0 };
            PhiThetaPsi[8] = new double[3] { 20, -60, 0 };
            PhiThetaPsi[9] = new double[3] { -20, -60, 0 };
            PhiThetaPsi[10] = new double[3] { 0, 30, -80 };
            PhiThetaPsi[11] = new double[3] { 0, 30, 80 };
            PhiThetaPsi[12] = new double[3] { 0, -30, 80 };
            PhiThetaPsi[13] = new double[3] { -20, 0, 90 };
            PhiThetaPsi[14] = new double[3] { 20, 0, 90 };
            PhiThetaPsi[15] = new double[3] { 20, 0, -90 };

            //  각 점을, X 축이 -45deg 회전한 좌표계에서의 좌표로 변환
            //  회전하기 전의 A 좌표계에서의 좌표를 B 좌표계에서의 좌표로 변환 저장한다.
            Vector3[] splPtsOnBcs = ConvertPtsOnAcsToPtsOnBcs(splPts);

            Vector3[] rotatedPts = new Vector3[splPts.Length];
            List<Vector3[]> resPtOnAcs = new List<Vector3[]>();
            List<Vector3[]> rotPtsOnBcs = new List<Vector3[]>();
            List<Vector3[]> rotPtsOnBcs2 = new List<Vector3[]>();

            for ( int i=0; i<16; i++)
            {
                //  먼저 방법 1
                //  A 좌표계에서 회전한 좌표 만들기
                //  여기까지는 단순한 각도를 통해서 값이 맞는지 확인할 수 있다.
                rotatedPts = RotatePtsByEuler(splPts, PhiThetaPsi[i]);
                //  결과를 저장
                resPtOnAcs.Add(rotatedPts);

                //  회전한 후의 A 좌표계에서의 좌표를 모두 B 좌표계에서의 좌표로 변환 저장한다.
                //  여기까지는 단순한 각도를 통해서 값이 맞는지 확인할 수 있다.
                Vector3[] rotPtOnBcs = ConvertPtsOnAcsToPtsOnBcs(rotatedPts);
                rotPtsOnBcs.Add(rotPtOnBcs);

                //   회전각도를 Bcs 에 대한 벡터로 변환
                double[] PhiThetaPsiOnBcs = TransferPhiThetaPsifromAcstoBcs(PhiThetaPsi[i]);

                //  Bcs 에서 splPtsOnBcs 를 PhiThetaPsiOnBcs 로 회전하여 좌표를 구한다
                Vector3[] rotPtOnBcs2 = RotatePtsByEuler(splPtsOnBcs, PhiThetaPsiOnBcs);
                rotPtsOnBcs2.Add(rotPtOnBcs2);

                //  rotPtsOnBcs 의 모든 요소는 rotPtsOnBcs2 의 모든 요소와 같아야 한다.
                //  같은지 비교
                //  rotPtOnBcs 는 A 좌표계에서 회전시킨 뒤 B 좌표계로 변환한 좌표
                //  rotPtOnBcs2 는 A 좌표계에서 B 좌표계로 변환한 뒤 B 좌표계에서 (변환된 회전각에 따라)회전시킨 좌표
                double dist = 0;
                for (int j = 0; j < splPts.Length; j++)
                    dist = Vector3.Distance(rotPtOnBcs[j], rotPtOnBcs2[j]);
            }
        }

        public Vector3[] RotatePtsByEuler(Vector3[] lPts, double[] eulerAnglesArcmin)
        {
            // 사용자 입력 회전각 (A 좌표계 기준)
            double a_rad = eulerAnglesArcmin[0] * mMinToRad;  // X축 회전
            double b_rad = eulerAnglesArcmin[1] * mMinToRad; // Y축 회전
            double c_rad = eulerAnglesArcmin[2] * mMinToRad; // Z축 회전


            // A 기준 회전 행렬 (Z * Y * X 순서)
            Matrix4x4 Rx = Matrix4x4.CreateRotationX((float)a_rad);
            Matrix4x4 Ry = Matrix4x4.CreateRotationY((float)b_rad);
            Matrix4x4 Rz = Matrix4x4.CreateRotationZ((float)c_rad);
            Matrix4x4 R = Rz * Ry * Rx; // 순서: X → Y → Z
            int len = lPts.Length;
            Vector3[] res = new Vector3[len];
            for ( int i=0; i< len; i++ )
            {
                res[i] = Vector3.Transform(lPts[i], R);

            }
            return res;
        }
        
        public double NormdistRand( double mean, double stdev)
        {
            double res = NRgenerator.Next(mean, stdev);
            return res;
        }

        public double mPrismZeroTX = 0;
        public double mPrismZeroTY = 0;
        public double mPrismZeroTZ = 0;

        public void SetPrismZeroTXTZ(double tx0, double ty0, double tz0)
        {
            mPrismZeroTX = tx0; mPrismZeroTY = ty0;  mPrismZeroTZ = tz0;
        }

        public double[] ConvertTXTYTZofCSHtoPrism(double tx, double ty, double tz, bool bAcrmin = false, bool bProbe = false)
        {
            double[] prismTXTYTZ = new double[3];

            if (!bProbe)
            {
                prismTXTYTZ[2] = (-ty + tz) / 1.414213562 - mPrismZeroTZ; 
                prismTXTYTZ[0] = (ty + tz) / 1.414213562 - mPrismZeroTX;

                if (!bAcrmin)
                    prismTXTYTZ[1] = tx + 1.4 * ((prismTXTYTZ[0] / mMinToRad) * (prismTXTYTZ[0] / mMinToRad) / 10000 - (prismTXTYTZ[2] / mMinToRad) * (prismTXTYTZ[2] / mMinToRad) / 10000) - mPrismZeroTY / mMinToRad;
                else
                    prismTXTYTZ[1] = tx + 1.4 * ((prismTXTYTZ[0] * prismTXTYTZ[0]) / 10000 - (prismTXTYTZ[2] * prismTXTYTZ[2]) / 10000) - mPrismZeroTY;
            }
            else
            {
                prismTXTYTZ[2] = (-ty + tz) / 1.414213562;
                prismTXTYTZ[0] = (ty + tz) / 1.414213562;

                if (!bAcrmin)
                    prismTXTYTZ[1] = tx + 1.4 * ((prismTXTYTZ[0] / mMinToRad) * (prismTXTYTZ[0] / mMinToRad) / 10000 - (prismTXTYTZ[2] / mMinToRad) * (prismTXTYTZ[2] / mMinToRad) / 10000);
                else
                    prismTXTYTZ[1] = tx + 1.4 * ((prismTXTYTZ[0] * prismTXTYTZ[0]) / 10000 - (prismTXTYTZ[2] * prismTXTYTZ[2]) / 10000);
            }

            return prismTXTYTZ;
        }

        //public static int[] SortRadix(this int[] inputArray)
        //{
        //    const int bitsPerDigit = 8;
        //    const uint numberOfBins = 1 << bitsPerDigit;
        //    uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
        //    int d;
        //    var outputArray = new int[inputArray.Length];

        //    int[][] startOfBin = new int[numberOfDigits][];
        //    for (int i = 0; i < numberOfDigits; i++)
        //        startOfBin[i] = new int[numberOfBins];
        //    bool outputArrayHasResult = false;

        //    const uint bitMask = numberOfBins - 1;
        //    const uint PowerOfTwoRadix = 4;
        //    const uint halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
        //    int shiftRightAmount = 0;

        //    uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

        //    for (d = 0; d < numberOfDigits; d++)
        //    {
        //        startOfBin[d][0] = 0;
        //        for (uint i = 1; i < numberOfBins; i++)
        //            startOfBin[d][i] = startOfBin[d][i - 1] + (int)count[d][i - 1];
        //    }

        //    d = 0;
        //    while (d < numberOfDigits)
        //    {
        //        int[] startOfBinLoc = startOfBin[d];

        //        if (d != 3)
        //            for (uint current = 0; current < inputArray.Length; current++)
        //                outputArray[startOfBinLoc[((uint)inputArray[current] >> shiftRightAmount) & bitMask]++] = inputArray[current];
        //        else
        //            for (uint current = 0; current < inputArray.Length; current++)
        //                outputArray[startOfBinLoc[((uint)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];

        //        shiftRightAmount += bitsPerDigit;
        //        outputArrayHasResult = !outputArrayHasResult;
        //        d++;

        //        int[] tmp = inputArray;       // swap input and output arrays
        //        inputArray = outputArray;
        //        outputArray = tmp;
        //    }
        //    return outputArrayHasResult ? outputArray : inputArray;
        //}
        //[StructLayout(LayoutKind.Explicit)]
        //internal struct Int32ByteUnion
        //{
        //    [FieldOffset(0)]
        //    public byte byte0;
        //    [FieldOffset(1)]
        //    public byte byte1;
        //    [FieldOffset(2)]
        //    public byte byte2;
        //    [FieldOffset(3)]
        //    public byte byte3;

        //    [FieldOffset(0)]
        //    public Int32 integer;
        //}
        //public static uint[][] HistogramByteComponents(int[] inArray, Int32 l, Int32 r)
        //{
        //    const int numberOfBins = 256;
        //    const int numberOfDigits = sizeof(ulong);
        //    uint[][] count = new uint[numberOfDigits][];
        //    for (int i = 0; i < numberOfDigits; i++)
        //        count[i] = new uint[numberOfBins];

        //    var union = new Int32ByteUnion();
        //    for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
        //    {
        //        union.integer = inArray[current];
        //        count[0][union.byte0]++;
        //        count[1][union.byte1]++;
        //        count[2][union.byte2]++;
        //        count[3][((uint)inArray[current] >> 24) ^ 128]++;
        //    }
        //    return count;
        //}


        /// <summary>
        /// 주어진 CSV 파일 경로들에서 X,Y,Z 좌표 구간을 읽어,
        /// 각 파일에 대해 회전 중심, 반지름, 회전축 단위벡터를 계산하여 반환합니다.
        /// </summary>
        /// <param name="Prism45filePaths">
        /// 처리할 CSV 파일 경로 배열. 각 파일은 헤더 1줄을 포함하고,
        /// 최소 4개 이상의 열을 가져야 하며 1~3열이 각각 X, Y, Z 좌표여야 합니다(0열은 인덱스).
        /// </param>
        /// <param name="startIndex">
        /// 읽기 시작 행의 0-기반 인덱스.
        /// </param>
        /// <param name="count">
        /// 읽을 행(row) 수.
        /// </param>
        /// <returns>
        /// 파일 순서대로 계산된 <see cref="RotationResult"/> 목록.
        /// 각 <see cref="RotationResult"/>에는 회전 중심 좌표, 반지름, 회전축 단위벡터가 포함됩니다.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// 지정한 경로의 CSV 파일이 존재하지 않는 경우.
        /// </exception>
        /// <exception cref="System.IO.InvalidDataException">
        /// CSV의 행/열 수가 요구 조건에 미달하는 경우(행: <c>startIndex + count</c> 미만, 열: 4 미만).
        /// </exception>
        public List<RotationResult> FindCenterRadiusNormal(string[] Prism45filePaths, int startIndex = 0, int count = 601)
        {
            List<RotationResult> rotationResults = new List<RotationResult>();
            
            foreach (var path in Prism45filePaths)
            {
                // CSV 파일에서 데이터 불러오기
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"파일이 존재하지 않습니다: {path}");
                }

                var table = ReadCsvAsDoubleMatrix(path, hasHeader: true);

                if (table.RowCount <= startIndex + count - 1 || table.ColumnCount < 4)
                {
                    throw new InvalidDataException($"파일 {path}의 데이터 크기가 부족합니다. Row={table.RowCount}, Col={table.ColumnCount}");
                }

                // x,y,z 데이터 추출
                var xyzPoints = table.SubMatrix(startIndex, count, 1, 3);

                // 원 중심, 반지름, 축 방향벡터 계산
                rotationResults.Add(FindCenterRadiusNormal(xyzPoints));
            }

            Console.WriteLine("Done.");
            return rotationResults;
        }

        /// <summary>
        /// 주어진 3차원 좌표 리스트(prism45TestData)로부터 회전 중심, 반지름, 회전축 단위벡터를 계산합니다.
        /// </summary>
        /// <param name="prism45TestData">
        /// X, Y, Z 좌표를 담은 리스트.
        /// 리스트의 각 항목은 최소 길이가 3 이상인 배열이며,
        /// double[0] = X, double[1] = Y, double[2] = Z 값을 사용합니다.
        /// </param>
        /// <returns>
        /// <see cref="RotationResult"/>:
        /// 회전 중심 좌표, 반지름, 회전축 단위벡터
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 입력 데이터가 null 인 경우
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 입력 데이터가 비어 있거나, 요소 중 배열 길이가 3 미만인 경우
        /// </exception>
        public RotationResult FindCenterRadiusNormal(List<double[]> prism45TestData)
        {
            if (prism45TestData is null)
                throw new ArgumentNullException(nameof(prism45TestData), "입력 데이터가 null입니다.");

            if (prism45TestData.Count == 0)
                throw new ArgumentException("입력 데이터가 비어 있습니다.", nameof(prism45TestData));

            if (prism45TestData.Any(p => p is null || p.Length < 3))
                throw new ArgumentException("각 데이터는 최소 3개의 값(X, Y, Z)을 포함해야 합니다.", nameof(prism45TestData));

            int n = prism45TestData.Count;

            // x,y,z 데이터 추출
            var xyzPoints = DenseMatrix.Create(n, 3, (row, col) => prism45TestData[row][col]);
            
            // 원 중심, 반지름, 축 방향벡터 계산
            return FindCenterRadiusNormal(xyzPoints);
        }


        /// <summary>
        /// N×3 형태의 3차원 좌표 집합(x, y, z)에서
        /// 회전축의 단위벡터, 회전 중심, 반지름을 추정합니다.
        /// </summary>
        /// <param name="xyzPoints">
        /// 크기가 N×3인 좌표 행렬(열 0,1,2는 각각 X, Y, Z).
        /// </param>
        /// <returns>
        /// <see cref="RotationResult"/>: 회전 중심(3D), 반지름, 회전축 단위벡터.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="xyzPoints"/> 가 null 인 경우.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="xyzPoints"/> 의 열 개수가 3이 아니거나, 행 개수가 3 미만인 경우.
        /// </exception>
        public RotationResult FindCenterRadiusNormal(MathNetMatrix xyzPoints)
        {
            if (xyzPoints is null)
                throw new ArgumentNullException(nameof(xyzPoints));

            if (xyzPoints.ColumnCount != 3)
                throw new ArgumentException("xyzPoints는 N×3 행렬이어야 합니다. (열 개수=3)", nameof(xyzPoints));

            if (xyzPoints.RowCount < 3)
                throw new ArgumentException("원/축을 추정하려면 최소 3개의 점이 필요합니다. (행 개수≥3)", nameof(xyzPoints));

            // 회전 축 법선벡터 nVector
            var nVector = FindNormalOnPlane(xyzPoints);

            // 회전 중심 및 반지름 
            (var center3D, var radius) = FindCircleOnPlane(xyzPoints);

            var rotationResult = new RotationResult(center3D, radius, nVector);
            Console.WriteLine(rotationResult.ToString());

            return rotationResult;
        }

        /// <summary>
        /// N×3 형태의 3차원 좌표 집합(x, y, z)으로부터
        /// 평면의 법선 벡터 추정합니다.
        /// </summary>
        /// <param name="xyzPoints">
        /// 크기가 N×3인 좌표 행렬(열 0,1,2는 각각 X, Y, Z).
        /// </param>
        /// <returns>
        /// 추정한 평면의 법선 벡터
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="xyzPoints"/> 가 null 인 경우.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="xyzPoints"/> 의 열 개수가 3이 아니거나, 행 개수가 3 미만인 경우.
        /// </exception>
        public MathNetVector FindNormalOnPlane(MathNetMatrix xyzPoints)
        {
            if (xyzPoints is null)
                throw new ArgumentNullException(nameof(xyzPoints));

            if (xyzPoints.ColumnCount != 3)
                throw new ArgumentException("xyzPoints는 N×3 행렬이어야 합니다. (열 개수=3)", nameof(xyzPoints));

            if (xyzPoints.RowCount < 3)
                throw new ArgumentException("회전축을 추정하려면 최소 3개의 점이 필요합니다. (행 개수≥3)", nameof(xyzPoints));

            var points = xyzPoints.Clone();
            var orgx = points.Column(0);
            var orgy = points.Column(1);
            var orgz = points.Column(2);

            int n = points.RowCount;
            var ones = DenseVector.Create(n, 1.0);
            var M = DenseMatrix.OfColumnVectors(orgx, orgy, orgz, ones);

            // SVD → pseudo-plane 법선
            var svdM = M.Svd(true);
            var V_M = svdM.VT.Transpose();
            var myplane = V_M.Column(V_M.ColumnCount - 1); // [a,b,c,d]^T
            var normalVector = myplane.SubVector(0, 3);

            // 단위화된 회전축 벡터
            return normalVector / normalVector.L2Norm();
        }

        /// <summary>
        /// N×3 형태의 3차원 좌표 집합(x, y, z)으로부터
        /// 회전 중심, 반지름을 추정합니다.
        /// </summary>
        /// <param name="xyzPoints">
        /// 크기가 N×3인 좌표 행렬(열 0,1,2는 각각 X, Y, Z).
        /// </param>
        /// <returns>
        /// 추정한 회전 중심 좌표(Center), 반지름 (Radius)
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="xyzPoints"/> 가 null 인 경우.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="xyzPoints"/> 의 열 개수가 3이 아니거나, 행 개수가 3 미만인 경우.
        /// </exception>
        public (MathNetVector Center3D, double Radius) FindCircleOnPlane(MathNetMatrix xyzPoints)
        {
            if (xyzPoints is null)
                throw new ArgumentNullException(nameof(xyzPoints));

            if (xyzPoints.ColumnCount != 3)
                throw new ArgumentException("xyzPoints는 N×3 행렬이어야 합니다. (열 개수=3)", nameof(xyzPoints));

            if (xyzPoints.RowCount < 3)
                throw new ArgumentException("원을 추정하려면 최소 3개의 점이 필요합니다. (행 개수≥3)", nameof(xyzPoints));

            var points = xyzPoints.Clone();
            int n = points.RowCount;

            // 1. 중심화
            var centroid3D = MeanRow(points);
            var centered = CenterRows(points, centroid3D);

            // 2. 평면 법선
            var svdC = centered.Svd(true);
            var V_C = svdC.VT.Transpose();
            var normal = V_C.Column(V_C.ColumnCount - 1);
            var normalUnit = normal / normal.L2Norm();

            // 3. 평면에 정사영
            var projected = DenseMatrix.Create(n, 3, 0.0);
            for (int i = 0; i < n; i++)
            {
                var p = points.Row(i).ToVector();
                var v = p - centroid3D;
                double dist = v.DotProduct(normalUnit);
                var proj = p - dist * normalUnit;
                projected.SetRow(i, proj);
            }

            // 4. 평면 위 평균점
            var centerOnPlane = MeanRow(projected);

            // 5. 좌표계 정의 (eX, eY)
            int i1 = 0, i2 = 1;
            double maxDist = -1.0;
            for (int i = 0; i < n; i++)
            {
                var pi = projected.Row(i).ToVector();
                for (int j = i + 1; j < n; j++)
                {
                    var pj = projected.Row(j).ToVector();
                    double d = (pi - pj).L2Norm();
                    if (d > maxDist)
                    {
                        maxDist = d; i1 = i; i2 = j;
                    }
                }
            }
            var eX = (projected.Row(i2).ToVector() - projected.Row(i1).ToVector()).Normalize(2);
            var eY = Cross(normalUnit, eX).Normalize(2);

            // 6. 2D 변환
            var projected2D = DenseMatrix.Create(n, 2, 0.0);
            for (int i = 0; i < n; i++)
            {
                var d = projected.Row(i).ToVector() - centerOnPlane;
                projected2D[i, 0] = d.DotProduct(eX);
                projected2D[i, 1] = d.DotProduct(eY);
            }

            // 7. 원 피팅 (Ax = b 해법)
            var x = projected2D.Column(0);
            var y = projected2D.Column(1);
            var A = DenseMatrix.OfColumns(n, 3, new[] { x, y, DenseVector.Create(n, 1.0) });
            var b = DenseVector.Create(n, i => -(x[i] * x[i] + y[i] * y[i]));
            var coeff = A.QR().Solve(b);

            double xc = -coeff[0] / 2.0;
            double yc = -coeff[1] / 2.0;
            double radius = Math.Sqrt(xc * xc + yc * yc - coeff[2]);

            // 8. 3D 원 중심 복원
            var center3D = centerOnPlane + xc * eX + yc * eY;

            return (center3D, radius);
        }

        /// <summary>
        /// 회전 분석 결과를 담는 구조체.
        /// 회전 중심 좌표(<see cref="Center"/>), 반지름(<see cref="Radius"/>),회전축 단위벡터(<see cref="AxisVector"/>) 
        /// </summary>
        public readonly struct RotationResult
        {
            public MathNetVector Center { get; }   // CenterX, Y, Z 대신
            public double Radius { get; }           // 반지름

            public MathNetVector AxisVector { get; }     // AxisX, Y, Z 대신

            public RotationResult(MathNetVector center, double radius, MathNetVector axisVector)
            {
                Center = center;
                Radius = radius;
                AxisVector = axisVector;
            }

            public override string ToString()
            {
                return $"Center: ({Center[0]:F2}, {Center[1]:F2}, {Center[2]:F2})\t" +
                       $"Radius: {Radius:F2} um\n" +
                       $"Rotation Axis : ({AxisVector[0]:F4}, {AxisVector[1]:F4}, {AxisVector[2]:F4})\n";
            }
        }

        public Matrix<double> ReadCsvAsDoubleMatrix(string path, bool hasHeader)
        {
            var rows = new List<double[]>();
            using (var sr = new StreamReader(path))
            {
                string line;
                bool first = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (first && hasHeader)
                    {
                        first = false;
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var tokens = line.Split(',')
                                        .Select(s => s.Trim())
                                        .ToArray();

                    var vals = new double[tokens.Length];
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        if (!double.TryParse(tokens[i], NumberStyles.Any, CultureInfo.InvariantCulture, out vals[i]))
                        {
                            vals[i] = double.NaN;
                        }
                    }

                    rows.Add(vals);
                }
            }

            int m = rows.Count;
            if (m == 0) return DenseMatrix.Create(0, 0, 0.0);
            int n = rows.Max(r => r.Length);

            var mat = DenseMatrix.Create(m, n, 0.0);
            for (int i = 0; i < m; i++)
            {
                var r = rows[i];
                for (int j = 0; j < r.Length; j++)
                    mat[i, j] = r[j];
            }
            return mat;
        }

        public MathNetVector MeanRow(Matrix<double> M)
        {
            int m = M.RowCount;
            int n = M.ColumnCount;
            var mean = DenseVector.Create(n, 0.0);
            for (int j = 0; j < n; j++)
            {
                double s = 0.0;

                for (int i = 0; i < m; i++)
                {
                    s += M[i, j];
                }

                mean[j] = s / m;
            }
            return mean;
        }

        public Matrix<double> CenterRows(Matrix<double> M, MathNetVector mean)
        {
            var C = M.Clone();
            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                    C[i, j] -= mean[j];
            }
            return C;
        }
    }

    static class VectorExt
    {
        public static MathNetVector ToVector(this MathNetVector v) => v.Clone();

    }

}

