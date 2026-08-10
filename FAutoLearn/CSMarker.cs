using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FAutoLearn
{
    public class CSMarker
    {
        public int ResizePinSize = 2;

        int HalfRPR
        {
            get { return ResizePinSize / 2; }

        }
        public static Rectangle EffectiveRect = new Rectangle();
        private static int countCreated = 0;
        public int ID;
        public int Type=-1;
        private float[] SelectRectDashPattern;
        //private static Pen SelectRectPen, ResizePinPen;
        //public static Rectangle SelectRect;
        //private static Rectangle ResizePin1, ResizePin2, ResizePin3, ResizePin4, ResizePin5, ResizePin6, ResizePin7, ResizePin8;
        //private static Rectangle rect;
        //private static int ActivationPinPosition;
        //private static int SelectX0, SelectY0, SelectX1, SelectY1;

        public Pen SelectRectPen, ResizePinPen;
        public Rectangle SelectRect;
        private Rectangle ResizePin1, ResizePin2, ResizePin3, ResizePin4, ResizePin5, ResizePin6, ResizePin7, ResizePin8;
        private Rectangle rect;
        private int ActivationPinPosition;
        private int SelectX0, SelectY0, SelectX1, SelectY1;

        private const int hitNothing = -1;
        private const int hitTopLeft = 0;
        private const int hitTopRight = 1;
        private const int hitBottomRight = 2;
        private const int hitBottomLeft = 3;
        private const int hitTop = 4;
        private const int hitRight = 5;
        private const int hitBottom = 6;
        private const int hitLeft = 7;
        private const int hitMiddle = 8;

        #region HideResizePin
        private void HideResizePin()
        {
            ResizePin1.Width = 0; ResizePin1.Height = 0;
            ResizePin2.Width = 0; ResizePin2.Height = 0;
            ResizePin3.Width = 0; ResizePin3.Height = 0;
            ResizePin4.Width = 0; ResizePin4.Height = 0;
            ResizePin5.Width = 0; ResizePin5.Height = 0;
            ResizePin6.Width = 0; ResizePin6.Height = 0;
            ResizePin7.Width = 0; ResizePin7.Height = 0;
            ResizePin8.Width = 0; ResizePin8.Height = 0;
        }
        #endregion

        public void SetEffectveRect(int x, int y, int w, int h)
        {
            EffectiveRect.X = x;
            EffectiveRect.Y = y;
            EffectiveRect.Width = w;
            EffectiveRect.Height = h;
        }

        #region SetResizePinVal 实时显示ResizePin
        void SetResizePinVal()
        {
            ResizePin1.X = X - HalfRPR;
            ResizePin1.Y = Y - HalfRPR;
            ResizePin2.X = X + Width / 2 - HalfRPR;
            ResizePin2.Y = Y - HalfRPR;
            ResizePin3.X = X + Width - HalfRPR;
            ResizePin3.Y = Y - HalfRPR;
            ResizePin4.X = X + Width - HalfRPR;
            ResizePin4.Y = Y + Height / 2 - HalfRPR;
            ResizePin5.X = X + Width - HalfRPR;
            ResizePin5.Y = Y + Height - HalfRPR;
            ResizePin6.X = X + Width / 2 - HalfRPR;
            ResizePin6.Y = Y + Height - HalfRPR;
            ResizePin7.X = X - HalfRPR;
            ResizePin7.Y = Y + Height - HalfRPR;
            ResizePin8.X = X - HalfRPR;
            ResizePin8.Y = Y + Height / 2 - HalfRPR;
        }
        #endregion

        #region ShowResizePin
        private void ShowResizePin()
        {
            ResizePin1.X = SelectRect.X - HalfRPR;
            ResizePin1.Y = SelectRect.Y - HalfRPR;
            ResizePin2.X = SelectRect.X + SelectRect.Width / 2 - HalfRPR;
            ResizePin2.Y = SelectRect.Y - HalfRPR;
            ResizePin3.X = SelectRect.X + SelectRect.Width - HalfRPR;
            ResizePin3.Y = SelectRect.Y - HalfRPR;
            ResizePin4.X = SelectRect.X + SelectRect.Width - HalfRPR;
            ResizePin4.Y = SelectRect.Y + SelectRect.Height / 2 - HalfRPR;
            ResizePin5.X = SelectRect.X + SelectRect.Width - HalfRPR;
            ResizePin5.Y = SelectRect.Y + SelectRect.Height - HalfRPR;
            ResizePin6.X = SelectRect.X + SelectRect.Width / 2 - HalfRPR;
            ResizePin6.Y = SelectRect.Y + SelectRect.Height - HalfRPR;
            ResizePin7.X = SelectRect.X - HalfRPR;
            ResizePin7.Y = SelectRect.Y + SelectRect.Height - HalfRPR;
            ResizePin8.X = SelectRect.X - HalfRPR;
            ResizePin8.Y = SelectRect.Y + SelectRect.Height / 2 - HalfRPR;
            ResizePin1.Width = ResizePinSize; ResizePin1.Height = ResizePinSize;
            ResizePin2.Width = ResizePinSize; ResizePin2.Height = ResizePinSize;
            ResizePin3.Width = ResizePinSize; ResizePin3.Height = ResizePinSize;
            ResizePin4.Width = ResizePinSize; ResizePin4.Height = ResizePinSize;
            ResizePin5.Width = ResizePinSize; ResizePin5.Height = ResizePinSize;
            ResizePin6.Width = ResizePinSize; ResizePin6.Height = ResizePinSize;
            ResizePin7.Width = ResizePinSize; ResizePin7.Height = ResizePinSize;
            ResizePin8.Width = ResizePinSize; ResizePin8.Height = ResizePinSize;

        }
        #endregion

        #region FillResizePins 
        void FillResizePins(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.FillEllipse(Brushes.Red, ResizePin1);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin2);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin3);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin4);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin5);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin6);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin7);
            e.Graphics.FillEllipse(Brushes.Red, ResizePin8);
        }
        #endregion

        #region DrawResizePin
        void DrawResizePin(Graphics g)
        {
            g.DrawEllipse(ResizePinPen, ResizePin1);
            g.DrawEllipse(ResizePinPen, ResizePin2);
            g.DrawEllipse(ResizePinPen, ResizePin3);
            g.DrawEllipse(ResizePinPen, ResizePin4);
            g.DrawEllipse(ResizePinPen, ResizePin5);
            g.DrawEllipse(ResizePinPen, ResizePin6);
            g.DrawEllipse(ResizePinPen, ResizePin7);
            g.DrawEllipse(ResizePinPen, ResizePin8);
        }
        #endregion

        #region InELP
        bool InELP(MouseEventArgs e, Point ElpCenter)
        {
            int elpX = ElpCenter.X;
            int elpY = ElpCenter.Y;
            return !((elpX - e.X) * (elpX - e.X) + (elpY - e.Y) * (elpY - e.Y) >= HalfRPR * HalfRPR);
        }
        #endregion

        #region HitTest
        private int HitTest(MouseEventArgs e)
        {
            if (e.X >= SelectRect.X - 3 && e.X <= SelectRect.X + SelectRect.Width + 3
                        && e.Y >= SelectRect.Y - 3 && e.Y <= SelectRect.Y + SelectRect.Height + 3)
                return hitMiddle;   //  Middle 만 유효하게 처리하자.
            else if (InELP(e, new Point(ResizePin1.X + HalfRPR, ResizePin1.Y + HalfRPR)))
                return hitTopLeft;
            else if (InELP(e, new Point(ResizePin2.X + HalfRPR, ResizePin2.Y + HalfRPR)))
                return hitTop;
            else if (InELP(e, new Point(ResizePin3.X + HalfRPR, ResizePin3.Y + HalfRPR)))
                return hitTopRight;
            else if (InELP(e, new Point(ResizePin4.X + HalfRPR, ResizePin4.Y + HalfRPR)))
                return hitRight;
            else if (InELP(e, new Point(ResizePin5.X + HalfRPR, ResizePin5.Y + HalfRPR)))
                return hitBottomRight;
            else if (InELP(e, new Point(ResizePin6.X + HalfRPR, ResizePin6.Y + HalfRPR)))
                return hitBottom;
            else if (InELP(e, new Point(ResizePin7.X + HalfRPR, ResizePin7.Y + HalfRPR)))
                return hitBottomLeft;
            else if (InELP(e, new Point(ResizePin8.X + HalfRPR, ResizePin8.Y + HalfRPR)))
                return hitLeft;
            //else if (e.X >= SelectRect.X + ResizePinSize && e.X <= SelectRect.X + SelectRect.Width - ResizePinSize
            //            && e.Y >= SelectRect.Y + ResizePinSize && e.Y <= SelectRect.Y + SelectRect.Height - ResizePinSize)
            else
                return hitNothing;
        }
        #endregion

        #region Create
        Pen[] penColor = new Pen[13] {
            new Pen(Color.FromArgb(128, 64,96,255),2),
            new Pen(Color.FromArgb(128, 48,80,255),2),

            new Pen(Color.FromArgb(128, 16,255,16),2),
            new Pen(Color.FromArgb(128, 0,228,0),2),

            new Pen(Color.FromArgb(128, 255,64,64),2),
            new Pen(Color.FromArgb(128, 240,0,0),2),

            new Pen(Color.FromArgb(128, 255,16,255),2),
            new Pen(Color.FromArgb(128, 228,0,228),2),

            new Pen(Color.FromArgb(128, 0,160,255),2),
            new Pen(Color.FromArgb(128, 0,128,232),2),

            new Pen(Color.FromArgb(128, 255,160,0),2),
            new Pen(Color.FromArgb(128, 255,132,0),2),

            new Pen(Color.FromArgb(128, 255,255,255),2),
        };

        //public void Create(int type=0)
        //{
        //    Type = type;
        //    ResizePinPen = penColor[type];
        //    SelectRectDashPattern = new float[] { 3, 2, 1 };

        //    SelectRectPen = penColor[type];
        //    //{
        //    //    DashPattern = SelectRectDashPattern,
        //    //};
        //    SelectRect = new Rectangle();
        //    ResizePin1 = new Rectangle();
        //    ResizePin2 = new Rectangle();
        //    ResizePin3 = new Rectangle();
        //    ResizePin4 = new Rectangle();
        //    ResizePin5 = new Rectangle();
        //    ResizePin6 = new Rectangle();
        //    ResizePin7 = new Rectangle();
        //    ResizePin8 = new Rectangle();
        //    rect = new Rectangle();
        //    ActivationPinPosition = hitNothing;

        //    countCreated++;
        //    ID = countCreated;
        //}
        public void Create(int type, int x, int y)
        {
            Type = type;
            ResizePinPen = penColor[type];
            SelectRectDashPattern = new float[] { 3, 2, 1 };

            SelectRectPen = penColor[type];// new Pen(Color.OrangeRed, 1.0f);
            SelectRectPen.Width = 2;
            //{
            //    DashPattern = SelectRectDashPattern,
            //};
            SelectRect = new Rectangle(x, y,13,13);
            rect = new Rectangle();
            InvalidateRectangle();
            ResizePin1 = new Rectangle();
            ResizePin2 = new Rectangle();
            ResizePin3 = new Rectangle();
            ResizePin4 = new Rectangle();
            ResizePin5 = new Rectangle();
            ResizePin6 = new Rectangle();
            ResizePin7 = new Rectangle();
            ResizePin8 = new Rectangle();
            ActivationPinPosition = hitNothing;

            countCreated++;
            ID = countCreated;
        }
        #endregion

        //待在box里面别出去
        void StayInBoxR(PictureBox R)
        {
            if (SelectRect.X < 0)
                SelectRect.X = 0;
            if (SelectRect.Y < 0)
                SelectRect.Y = 0;
            if (SelectRect.X + SelectRect.Width > R.Width)
                SelectRect.X = R.Width - SelectRect.Width;
            if (SelectRect.Y + SelectRect.Height > R.Height)
                SelectRect.Y = R.Height - SelectRect.Height;
        }

        #region Destroy//释放资源
        public void Destroy()
        {
            SelectRectPen.Dispose();
            ResizePinPen.Dispose();
        }
        #endregion

        #region StartPoint//
        public bool StartPoint(PictureBox p, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsTracking = 1;
                p.Invalidate(null, true);
                if (Type == 4)
                    IsTracking = 1;
                switch (HitTest(e))
                {
                    case hitTopLeft:
                        ActivationPinPosition = hitTopLeft;
                        SelectX1 = SelectRect.X + SelectRect.Width;
                        SelectY1 = SelectRect.Y + SelectRect.Height;
                        GenerateRectangle(ref SelectRect, SelectX1, SelectY1, 13, 13);
                        IsTracking = 2;
                        break;
                    case hitTop:
                        ActivationPinPosition = hitTop;
                        SelectX1 = SelectRect.Width + SelectRect.X;
                        SelectY1 = SelectRect.Height + SelectY0;
                        IsTracking = 3;
                        break;
                    case hitTopRight:
                        ActivationPinPosition = hitTopRight;
                        SelectY1 = SelectRect.Y + SelectRect.Height;
                        IsTracking = 4;
                        break;
                    case hitRight:
                        ActivationPinPosition = hitRight;
                        SelectY1 = SelectRect.Y + SelectRect.Height;
                        IsTracking = 5;
                        break;
                    case hitBottomRight:
                        ActivationPinPosition = hitBottomRight;
                        IsTracking = 6;
                        break;
                    case hitBottom:
                        ActivationPinPosition = hitBottom;
                        SelectX1 = SelectRect.X + SelectRect.Width;
                        IsTracking = 7;
                        break;
                    case hitBottomLeft:
                        ActivationPinPosition = hitBottomLeft;
                        SelectX1 = SelectRect.X + SelectRect.Width;
                        IsTracking = 8;
                        break;
                    case hitLeft:
                        ActivationPinPosition = hitLeft;
                        SelectX1 = SelectRect.X + SelectRect.Width;
                        SelectY1 = SelectRect.Y + SelectRect.Height;
                        IsTracking = 9;
                        break;
                    case hitMiddle:
                        ActivationPinPosition = hitMiddle;
                        SelectX0 = e.X - SelectRect.X;
                        SelectY0 = e.Y - SelectRect.Y;
                        SelectX1 = SelectRect.X + SelectRect.Width - e.X;
                        SelectY1 = SelectRect.Y + SelectRect.Height - e.Y;
                        IsTracking = 10;
                        break;
                    case hitNothing:
                        ActivationPinPosition = hitNothing;
                        //SelectRect.Width = 0;
                        //SelectRect.Height = 0;
                        SelectX0 = e.X;
                        SelectY0 = e.Y;
                        HideResizePin();
                        IsTracking = -1;
                        return false;
                        break;
                }
            }
            return true;
        }
        #endregion

        public int IsTracking = 0;
        #region TrackRubberBand 
        public bool TrackRubberBand(PictureBox p, MouseEventArgs e, bool Iskeep = false)
        {
            bool res = false;
            switch (HitTest(e))
            {
                //case hitTopLeft:
                //    p.Cursor = Cursors.SizeNWSE;
                //    break;
                //case hitTop:
                //    p.Cursor = Cursors.SizeNS;
                //    break;
                //case hitTopRight:
                //    p.Cursor = Cursors.SizeNESW;
                //    break;
                //case hitRight:
                //    p.Cursor = Cursors.SizeWE;
                //    break;
                //case hitBottomRight:
                //    p.Cursor = Cursors.SizeNWSE;
                //    break;
                //case hitBottom:
                //    p.Cursor = Cursors.SizeNS;
                //    break;
                //case hitBottomLeft:
                //    p.Cursor = Cursors.SizeNESW;
                //    break;
                //case hitLeft:
                //    p.Cursor = Cursors.SizeWE;
                //    break;
                case hitMiddle:
                    p.Cursor = Cursors.SizeAll;
                    res = true;
                    break;
                case hitNothing:
                    p.Cursor = Cursors.Default;
                    IsTracking = -1;
                    res = false;
                    break;
            }
            if (e.Button == MouseButtons.Left && IsTracking >= 0)
            {
                int TLX = e.X;
                int TLY = e.Y;

                if (e.X < 0)
                    TLX = 0;
                else if (e.X > p.Width)
                    TLX = p.Width;

                if (e.Y < 0)
                    TLY = 0;
                else if (e.Y > p.Height)
                    TLY = p.Height;
                if (!Iskeep)
                {
                    p.Invalidate(InvalidateRectangle(), false);
                }

                switch (ActivationPinPosition)
                {
                    case hitTopLeft:
                        GenerateRectangle(ref SelectRect, TLX, TLY, SelectX1, SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitTop:
                        GenerateRectangle(ref SelectRect, SelectX0, TLY, SelectX1, SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitTopRight:
                        GenerateRectangle(ref SelectRect, SelectX0, TLY, TLX, SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitRight:
                        GenerateRectangle(ref SelectRect, SelectX0, SelectY0, TLX, SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitBottomRight:
                        GenerateRectangle(ref SelectRect, SelectX0, SelectY0, TLX, TLY);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitBottom:
                        GenerateRectangle(ref SelectRect, SelectX0, SelectY0, SelectX1, TLY);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitBottomLeft:
                        GenerateRectangle(ref SelectRect, TLX, SelectY0, SelectX1, TLY);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitLeft:
                        GenerateRectangle(ref SelectRect, TLX, SelectY0, SelectX1, SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitMiddle:
                        GenerateRectangle(ref SelectRect, e.X - SelectX0, e.Y - SelectY0, e.X + SelectX1, e.Y + SelectY1);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                    case hitNothing:
                        GenerateRectangle(ref SelectRect, SelectX0, SelectY0, TLX, TLY);
                        StayInBoxR(p);
                        SetResizePinVal();
                        break;
                }
            }
            return res;
        }

        public void ForcePosition(PictureBox p, int x, int y)
        {
            SelectRect.X = x;
            SelectRect.Y = y;

            p.Invalidate(InvalidateRectangle(), false);

            GenerateRectangle(ref SelectRect, SelectRect.X, SelectRect.Y, SelectRect.X + SelectRect.Width, SelectRect.Y + SelectRect.Height);
            StayInBoxR(p);
            SetResizePinVal();
        }

        public void TrackRubberBand(PictureBox p, int dx, int dy, bool Iskeep = false)
        {
            p.Cursor = Cursors.SizeAll;

            SelectRect.X += dx;
            SelectRect.Y += dy;

            if (!Iskeep)
            {
                p.Invalidate(InvalidateRectangle(), false);
            }

            GenerateRectangle(ref SelectRect, SelectRect.X, SelectRect.Y, SelectRect.X + SelectRect.Width, SelectRect.Y + SelectRect.Height);
            StayInBoxR(p);
            SetResizePinVal();
        }
        #endregion

        #region EndPoint
        public bool EndPoint(PictureBox frm, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsTracking = 0;
                frm.Invalidate(null, true);
                SelectX0 = SelectRect.X;
                SelectY0 = SelectRect.Y;
                if (EffectiveRect.X < SelectRect.X && SelectRect.X < EffectiveRect.X + EffectiveRect.Width - 10)
                    if (EffectiveRect.Y < SelectRect.Y && SelectRect.Y < EffectiveRect.Y + EffectiveRect.Height - 10)
                        return true;
                //if (HitTest(e) != hitNothing && X > ResizePinSize && Y > ResizePinSize)
                //    ShowResizePin();
            }
            return false;
        }
        public bool EndPoint(PictureBox frm)
        {
            IsTracking = 0;
            frm.Invalidate(null, true);
            SelectX0 = SelectRect.X;
            SelectY0 = SelectRect.Y;
            if (EffectiveRect.X < SelectRect.X && SelectRect.X < EffectiveRect.X + EffectiveRect.Width - 10)
                if (EffectiveRect.Y < SelectRect.Y && SelectRect.Y < EffectiveRect.Y + EffectiveRect.Height - 10)
                    return true;
            //if (HitTest(e) != hitNothing && X > ResizePinSize && Y > ResizePinSize)
            //    ShowResizePin();
            return false;
        }
        #endregion

        #region DrawRubberBand //
        public void  DrawCOGmarker(PictureBox frm, PaintEventArgs e)
        {
            if (SelectRect.Width == 0 )
                return;

            Point p1 = new Point(SelectRect.X + SelectRect.Width / 2 + 1, SelectRect.Y - 2);
            Point p2 = new Point(SelectRect.X + SelectRect.Width / 2 + 1, SelectRect.Y + SelectRect.Height + 2);

            e.Graphics.DrawEllipse(SelectRectPen, SelectRect);
            e.Graphics.DrawLine(SelectRectPen, p1, p2);
            p1 = new Point(SelectRect.X - 2,                    SelectRect.Y + SelectRect.Height / 2 + 1);
            p2 = new Point(SelectRect.X + SelectRect.Width + 2, SelectRect.Y + SelectRect.Height / 2 + 1);
            e.Graphics.DrawLine(SelectRectPen, p1, p2);

            //if (IsTracking == 0)
            //{
            if (EffectiveRect.X < SelectRect.X && SelectRect.X < EffectiveRect.X + EffectiveRect.Width - 13)
                if (EffectiveRect.Y < SelectRect.Y && SelectRect.Y < EffectiveRect.Y + EffectiveRect.Height - 13)
                {
                    p1 = new Point(SelectRect.X + SelectRect.Width / 2 + 1, SelectRect.Y + SelectRect.Height / 2);

                    p2 = new Point(SelectRect.X + SelectRect.Width / 2 + 1, EffectiveRect.Y + EffectiveRect.Height / 2);

                    e.Graphics.DrawLine(SelectRectPen, p1, p2);
                    p2 = new Point(EffectiveRect.X + EffectiveRect.Width / 2 + 2, SelectRect.Y + SelectRect.Height / 2);
                    e.Graphics.DrawLine(SelectRectPen, p1, p2);
                }
            //}
        }

        //public void DrawRubberBand(PictureBox frm, PaintEventArgs e)
        //{
        //    e.Graphics.DrawRectangle(SelectRectPen, SelectRect);
        //    //FillResizePins(e);
        //    //DrawResizePin(e.Graphics);
        //}
        #endregion

        #region GenerateRectangle 
        private void GenerateRectangle(ref Rectangle TempRectangle, int X0, int Y0, int X1, int Y1)
        {
            if (X0 < X1)
            {
                TempRectangle.X = X0;
                TempRectangle.Width = 13;// X1 - X0;
            }
            else
            {
                TempRectangle.X = X1;
                TempRectangle.Width = 13;// X0 - X1;
            }
            if (Y0 < Y1)
            {
                TempRectangle.Y = Y0;
                TempRectangle.Height = 13;// Y1 - Y0;
            }
            else
            {
                TempRectangle.Y = Y1;
                TempRectangle.Height = 13;// Y0 - Y1;
            }
        }
        #endregion

        #region RubberBandAttribute //RubberBand
        public int X
        {
            get
            {
                return SelectRect.X;
            }
        }
        public int Y
        {
            get
            {
                return SelectRect.Y;
            }
        }
        public int Width
        {
            get
            {
                return SelectRect.Width;
            }

        }
        public int Height
        {
            get
            {
                return SelectRect.Height;
            }
        }

        public Pen RectView { get => RectView; set => RectView = value; }
        #endregion

        #region InvalidateRectangle 
        public Rectangle InvalidateRectangle()
        {
            rect.X = SelectRect.X - 3;
            rect.Y = SelectRect.Y - 3;
            rect.Width = SelectRect.Width + 6;
            rect.Height = SelectRect.Height + 6;

            return rect;
        }
        #endregion

        /// <summary>
        /// set ccs
        /// </summary>
        /// <param name="ex">x坐标</param>
        /// <param name="ey">y坐标</param>
        /// <param name="ew">宽</param>
        /// <param name="eh">高</param>
        public void SetRect(int ex, int ey, int ew, int eh)
        {
            GenerateRectangle(ref SelectRect, ex, ey, ex + ew, ey + eh);
        }

        //隐藏起来
        public void HideCCS()
        {
            HideResizePin();
            SetRect(0, 0, 0, 0);
        }
        public Rectangle GetSelectRect()
        {
            return SelectRect;
        }
    }
}
