using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FAutoLearn
{
    public class CSTracker
    {
        /// <summary>
        /// 圆的直径
        /// </summary>
        public int ResizePinSize = 4;

        //半径
        int HalfRPR
        {
            get { return ResizePinSize / 2; }

        }
        private Pen SelectRectPen, ResizePinPen;
        private Brush PinBrush;
        private float[] SelectRectDashPattern;
        //public static Rectangle SelectRect;
        //private static Rectangle ResizePin1, ResizePin2, ResizePin3, ResizePin4, ResizePin5, ResizePin6, ResizePin7, ResizePin8;
        //private static Rectangle rect;
        //private static int ActivationPinPosition;
        //private static int SelectX0, SelectY0, SelectX1, SelectY1;

        public  Rectangle SelectRect;
        private Rectangle ResizePin1, ResizePin2, ResizePin3, ResizePin4, ResizePin5, ResizePin6, ResizePin7, ResizePin8;
        private Rectangle rect;
        private int ActivationPinPosition;
        private int SelectX0, SelectY0, SelectX1, SelectY1;
        private int mytype = 0;

        //8个角+点空白处+点中间
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

        #region HideResizePin//隐藏ResizePin
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

        #region ShowResizePin//显示ResizePin
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

        #region FillResizePins // 填充ResizePinRect颜色
        void FillResizePins(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.FillEllipse(PinBrush, ResizePin1);
            e.Graphics.FillEllipse(PinBrush, ResizePin2);
            e.Graphics.FillEllipse(PinBrush, ResizePin3);
            e.Graphics.FillEllipse(PinBrush, ResizePin4);
            e.Graphics.FillEllipse(PinBrush, ResizePin5);
            e.Graphics.FillEllipse(PinBrush, ResizePin6);
            e.Graphics.FillEllipse(PinBrush, ResizePin7);
            e.Graphics.FillEllipse(PinBrush, ResizePin8);
        }
        #endregion

        #region DrawResizePin//绘制ResizePin
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

        #region InELP//是否在ResizePin圆内
        bool InELP(MouseEventArgs e, Point ElpCenter)
        {
            int elpX = ElpCenter.X;
            int elpY = ElpCenter.Y;
            return !((elpX - e.X) * (elpX - e.X) + (elpY - e.Y) * (elpY - e.Y) >= HalfRPR * HalfRPR);
        }
        #endregion

        #region HitTest//测试哪个ResizePin被选中
        private int HitTest(MouseEventArgs e)
        {
            if (InELP(e, new Point(ResizePin1.X + HalfRPR, ResizePin1.Y + HalfRPR)))
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
            else if (e.X >= SelectRect.X + ResizePinSize && e.X <= SelectRect.X + SelectRect.Width - ResizePinSize
                        && e.Y >= SelectRect.Y + ResizePinSize && e.Y <= SelectRect.Y + SelectRect.Height - ResizePinSize)
                return hitMiddle;
            else
                return hitNothing;
        }
        #endregion

        #region Create
        public void Create(int colorIndex = 0)
        {
            mytype = colorIndex;
            if (colorIndex==0)
            {
                ResizePinPen = new Pen(Color.Red, 1);
                SelectRectPen = new Pen(Color.OrangeRed, 1.0f);
                PinBrush = Brushes.OrangeRed;
            }
            else if (colorIndex == 1)
            {
                ResizePinPen = new Pen(Color.Navy, 1);
                SelectRectPen = new Pen(Color.Blue, 1.0f);
                PinBrush = Brushes.Blue;
            }
            else if (colorIndex == 2)
            {
                ResizePinPen = new Pen(Color.DodgerBlue, 1);
                SelectRectPen = new Pen(Color.LightBlue, 1.0f);
                PinBrush = Brushes.LightBlue;
            }
            else
            {
                ResizePinPen = new Pen(Color.Orange, 1);
                SelectRectPen = new Pen(Color.Yellow, 1.0f);
                PinBrush = Brushes.Yellow;
            }
            SelectRectDashPattern = new float[] { 3, 2, 1 };

            //{
            //    DashPattern = SelectRectDashPattern,
            //};
            SelectRect = new Rectangle();
            ResizePin1 = new Rectangle();
            ResizePin2 = new Rectangle();
            ResizePin3 = new Rectangle();
            ResizePin4 = new Rectangle();
            ResizePin5 = new Rectangle();
            ResizePin6 = new Rectangle();
            ResizePin7 = new Rectangle();
            ResizePin8 = new Rectangle();
            rect = new Rectangle();
            ActivationPinPosition = hitNothing;
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
                switch (HitTest(e))
                {
                    case hitTopLeft:
                        ActivationPinPosition = hitTopLeft;
                        SelectX1 = SelectRect.X + SelectRect.Width;
                        SelectY1 = SelectRect.Y + SelectRect.Height;
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
                        SelectRect.Width = 0;
                        SelectRect.Height = 0;
                        SelectX0 = e.X;
                        SelectY0 = e.Y;
                        HideResizePin();
                        return false;
                        break;
                }
            }
            return true;
        }
        #endregion

        public int IsTracking = 0;
        #region TrackRubberBand 
        public void TrackRubberBand(PictureBox p, MouseEventArgs e, bool Iskeep = false)
        {
            switch (HitTest(e))
            {
                case hitTopLeft:
                    p.Cursor = Cursors.SizeNWSE;
                    break;
                case hitTop:
                    p.Cursor = Cursors.SizeNS;
                    break;
                case hitTopRight:
                    p.Cursor = Cursors.SizeNESW;
                    break;
                case hitRight:
                    p.Cursor = Cursors.SizeWE;
                    break;
                case hitBottomRight:
                    p.Cursor = Cursors.SizeNWSE;
                    break;
                case hitBottom:
                    p.Cursor = Cursors.SizeNS;
                    break;
                case hitBottomLeft:
                    p.Cursor = Cursors.SizeNESW;
                    break;
                case hitLeft:
                    p.Cursor = Cursors.SizeWE;
                    break;
                case hitMiddle:
                    p.Cursor = Cursors.SizeAll;
                    break;
                case hitNothing:
                    p.Cursor = Cursors.Default;
                    break;
            }
            if (e.Button == MouseButtons.Left)
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
        public void TrackRubberBand(PictureBox p, int dx, int dy, int dw, int dh, bool Iskeep = false)
        {
            p.Cursor = Cursors.SizeAll;

            SelectRect.X += dx;
            SelectRect.Y += dy;
            SelectRect.Width += dw;
            SelectRect.Height += dh;

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
        public void EndPoint(PictureBox frm, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsTracking = 0;
                frm.Invalidate(null, true);
                SelectX0 = SelectRect.X;
                SelectY0 = SelectRect.Y;
                if (HitTest(e) != hitNothing && X > ResizePinSize && Y > ResizePinSize)
                    ShowResizePin();
            }
        }
        #endregion

        #region DrawRubberBand //
        public void DrawRubberBand(PictureBox frm, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(SelectRectPen, SelectRect);
            if ( mytype == 1)
            {
                e.Graphics.DrawLine(SelectRectPen, new Point((SelectRect.Left + SelectRect.Right) / 2, SelectRect.Top), new Point(SelectRect.Right, (SelectRect.Top + SelectRect.Bottom) / 2));
                e.Graphics.DrawLine(SelectRectPen, new Point(SelectRect.Left, SelectRect.Top), new Point(SelectRect.Right, SelectRect.Bottom));
                e.Graphics.DrawLine(SelectRectPen, new Point(SelectRect.Left, (SelectRect.Top + SelectRect.Bottom) / 2), new Point((SelectRect.Left + SelectRect.Right) / 2, SelectRect.Bottom));
            }
            FillResizePins(e);
            DrawResizePin(e.Graphics);
        }
        #endregion

        #region GenerateRectangle 
        private void GenerateRectangle(ref Rectangle TempRectangle, int X0, int Y0, int X1, int Y1)
        {
            if (X0 < X1)
            {
                TempRectangle.X = X0;
                TempRectangle.Width = X1 - X0;
            }
            else
            {
                TempRectangle.X = X1;
                TempRectangle.Width = X0 - X1;
            }
            if (Y0 < Y1)
            {
                TempRectangle.Y = Y0;
                TempRectangle.Height = Y1 - Y0;
            }
            else
            {
                TempRectangle.Y = Y1;
                TempRectangle.Height = Y0 - Y1;
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
            rect.X = SelectRect.X - 8;
            rect.Y = SelectRect.Y - 8;
            rect.Width = SelectRect.Width + 16;
            rect.Height = SelectRect.Height + 16;

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
