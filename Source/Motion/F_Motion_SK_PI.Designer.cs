using System.Drawing;
using System.Windows.Forms;

namespace MotorizedStage_SK_PI
{
    partial class F_Motion_SK_PI : Form
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Motion_SK_PI));
            this.Label1 = new System.Windows.Forms.Label();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.SetGrp = new System.Windows.Forms.GroupBox();
            this.IsEditChk = new System.Windows.Forms.CheckBox();
            this.ResetBtn = new System.Windows.Forms.Button();
            this.SetHomeGrp = new System.Windows.Forms.GroupBox();
            this.homeFileTxt = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TZHomeTxt = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TYHomeTxt = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TXHomeTxt = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ZHomeTxt = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.YHomeTxt = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.XHomeTxt = new System.Windows.Forms.TextBox();
            this.label67 = new System.Windows.Forms.Label();
            this.CurPosToHomeBtn = new System.Windows.Forms.Button();
            this.SetBtn = new System.Windows.Forms.Button();
            this.CoordinateGrp = new System.Windows.Forms.GroupBox();
            this.label74 = new System.Windows.Forms.Label();
            this.TZCSTxt = new System.Windows.Forms.TextBox();
            this.label75 = new System.Windows.Forms.Label();
            this.TYCSTxt = new System.Windows.Forms.TextBox();
            this.label76 = new System.Windows.Forms.Label();
            this.TXCSTxt = new System.Windows.Forms.TextBox();
            this.label77 = new System.Windows.Forms.Label();
            this.ZCSTxt = new System.Windows.Forms.TextBox();
            this.label78 = new System.Windows.Forms.Label();
            this.YCSTxt = new System.Windows.Forms.TextBox();
            this.label79 = new System.Windows.Forms.Label();
            this.XCSTxt = new System.Windows.Forms.TextBox();
            this.PivotGrp = new System.Windows.Forms.GroupBox();
            this.label71 = new System.Windows.Forms.Label();
            this.label72 = new System.Windows.Forms.Label();
            this.label73 = new System.Windows.Forms.Label();
            this.ZPivotTxt = new System.Windows.Forms.TextBox();
            this.YPivotTxt = new System.Windows.Forms.TextBox();
            this.XPivotTxt = new System.Windows.Forms.TextBox();
            this.SpeedLevelGrp = new System.Windows.Forms.GroupBox();
            this.NormalSRdo = new System.Windows.Forms.RadioButton();
            this.SlowSRdo = new System.Windows.Forms.RadioButton();
            this.FastSRdo = new System.Windows.Forms.RadioButton();
            this.label70 = new System.Windows.Forms.Label();
            this.TZSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.label69 = new System.Windows.Forms.Label();
            this.TYSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.TXSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.ZSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.YSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.XSpeedLevelTxt = new System.Windows.Forms.TextBox();
            this.XGrp = new System.Windows.Forms.GroupBox();
            this.XIsReadyLbl = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.XIsHomeLbl = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.XIsNegLimitTxt = new System.Windows.Forms.Label();
            this.XIsPosLimitLbl = new System.Windows.Forms.Label();
            this.XIsBusyLbl = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.XSpeedTxt = new System.Windows.Forms.TextBox();
            this.XAlarmLbl = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.XCurPosTxt = new System.Windows.Forms.TextBox();
            this.HomeGrp = new System.Windows.Forms.GroupBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.StopBtn = new System.Windows.Forms.Button();
            this.VisionHomeBtn = new System.Windows.Forms.Button();
            this.MechOriginBtn = new System.Windows.Forms.Button();
            this.ComPortCmb = new System.Windows.Forms.ComboBox();
            this.IsCunnectedLbl = new System.Windows.Forms.Label();
            this.MoveGrp = new System.Windows.Forms.GroupBox();
            this.label32 = new System.Windows.Forms.Label();
            this.NormalMRdo = new System.Windows.Forms.RadioButton();
            this.AxisGrp = new System.Windows.Forms.GroupBox();
            this.AllAxesRdo = new System.Windows.Forms.RadioButton();
            this.YAxisRdo = new System.Windows.Forms.RadioButton();
            this.TZAxisRdo = new System.Windows.Forms.RadioButton();
            this.TYAxisRdo = new System.Windows.Forms.RadioButton();
            this.TXAxisRdo = new System.Windows.Forms.RadioButton();
            this.ZAxisRdo = new System.Windows.Forms.RadioButton();
            this.XAxisRdo = new System.Windows.Forms.RadioButton();
            this.SlowMRdo = new System.Windows.Forms.RadioButton();
            this.FastMRdo = new System.Windows.Forms.RadioButton();
            this.MoveTypeCbo = new System.Windows.Forms.ComboBox();
            this.AxisMovPnl = new System.Windows.Forms.Panel();
            this.label27 = new System.Windows.Forms.Label();
            this.StopAxisBtn = new System.Windows.Forms.Button();
            this.MoveAxisBtn = new System.Windows.Forms.Button();
            this.JogMinusBtn = new System.Windows.Forms.Button();
            this.JogPlusBtn = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.targetPosTxt = new System.Windows.Forms.TextBox();
            this.AllAxesMovPnl = new System.Windows.Forms.Panel();
            this.label13 = new System.Windows.Forms.Label();
            this.TZtargetPosTxt = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.TYtargetPosTxt = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.TXtargetPosTxt = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.ZtargetPosTxt = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.YtargetPosTxt = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.XtargetPosTxt = new System.Windows.Forms.TextBox();
            this.Stop6DBtn = new System.Windows.Forms.Button();
            this.Move6DBtn = new System.Windows.Forms.Button();
            this.LogText = new System.Windows.Forms.RichTextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.YGrp = new System.Windows.Forms.GroupBox();
            this.YIsReadyLbl = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.YIsHomeLbl = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.YIsNegLimitTxt = new System.Windows.Forms.Label();
            this.YIsPosLimitLbl = new System.Windows.Forms.Label();
            this.YIsBusyLbl = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.YSpeedTxt = new System.Windows.Forms.TextBox();
            this.YAlarmLbl = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.YCurPosTxt = new System.Windows.Forms.TextBox();
            this.ZGrp = new System.Windows.Forms.GroupBox();
            this.ZIsReadyLbl = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.ZIsHomeLbl = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.ZIsNegLimitTxt = new System.Windows.Forms.Label();
            this.ZIsPosLimitLbl = new System.Windows.Forms.Label();
            this.ZIsBusyLbl = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.ZSpeedTxt = new System.Windows.Forms.TextBox();
            this.ZAlarmLbl = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.ZCurPosTxt = new System.Windows.Forms.TextBox();
            this.TXGrp = new System.Windows.Forms.GroupBox();
            this.TXIsReadyLbl = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.TXIsHomeLbl = new System.Windows.Forms.Label();
            this.label58 = new System.Windows.Forms.Label();
            this.TXIsNegLimitTxt = new System.Windows.Forms.Label();
            this.TXIsPosLimitLbl = new System.Windows.Forms.Label();
            this.TXIsBusyLbl = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.TXSpeedTxt = new System.Windows.Forms.TextBox();
            this.TXAlarmLbl = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.label80 = new System.Windows.Forms.Label();
            this.TXCurPosTxt = new System.Windows.Forms.TextBox();
            this.TYGrp = new System.Windows.Forms.GroupBox();
            this.TYIsReadyLbl = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.TYIsHomeLbl = new System.Windows.Forms.Label();
            this.label84 = new System.Windows.Forms.Label();
            this.TYIsNegLimitTxt = new System.Windows.Forms.Label();
            this.TYIsPosLimitLbl = new System.Windows.Forms.Label();
            this.TYIsBusyLbl = new System.Windows.Forms.Label();
            this.label88 = new System.Windows.Forms.Label();
            this.label89 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.TYSpeedTxt = new System.Windows.Forms.TextBox();
            this.TYAlarmLbl = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.label93 = new System.Windows.Forms.Label();
            this.TYCurPosTxt = new System.Windows.Forms.TextBox();
            this.TZGrp = new System.Windows.Forms.GroupBox();
            this.TZIsReadyLbl = new System.Windows.Forms.Label();
            this.label95 = new System.Windows.Forms.Label();
            this.TZIsHomeLbl = new System.Windows.Forms.Label();
            this.label97 = new System.Windows.Forms.Label();
            this.TZIsNegLimitTxt = new System.Windows.Forms.Label();
            this.TZIsPosLimitLbl = new System.Windows.Forms.Label();
            this.TZIsBusyLbl = new System.Windows.Forms.Label();
            this.label101 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.label103 = new System.Windows.Forms.Label();
            this.TZSpeedTxt = new System.Windows.Forms.TextBox();
            this.TZAlarmLbl = new System.Windows.Forms.Label();
            this.label105 = new System.Windows.Forms.Label();
            this.label106 = new System.Windows.Forms.Label();
            this.TZCurPosTxt = new System.Windows.Forms.TextBox();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.LogClearBtn = new System.Windows.Forms.Button();
            this.ReferenceHexapodBtn = new System.Windows.Forms.Button();
            this.ReferencXYZBtn = new System.Windows.Forms.Button();
            this.label30 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.btnXYZTest = new System.Windows.Forms.Button();
            this.SetGrp.SuspendLayout();
            this.SetHomeGrp.SuspendLayout();
            this.CoordinateGrp.SuspendLayout();
            this.PivotGrp.SuspendLayout();
            this.SpeedLevelGrp.SuspendLayout();
            this.XGrp.SuspendLayout();
            this.HomeGrp.SuspendLayout();
            this.MoveGrp.SuspendLayout();
            this.AxisGrp.SuspendLayout();
            this.AxisMovPnl.SuspendLayout();
            this.AllAxesMovPnl.SuspendLayout();
            this.YGrp.SuspendLayout();
            this.ZGrp.SuspendLayout();
            this.TXGrp.SuspendLayout();
            this.TYGrp.SuspendLayout();
            this.TZGrp.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold);
            this.Label1.Location = new System.Drawing.Point(37, 13);
            this.Label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(86, 18);
            this.Label1.TabIndex = 5;
            this.Label1.Text = "COM Ports";
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ConnectBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ConnectBtn.BackgroundImage")));
            this.ConnectBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ConnectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConnectBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.ConnectBtn.ForeColor = System.Drawing.Color.White;
            this.ConnectBtn.Location = new System.Drawing.Point(179, 15);
            this.ConnectBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(123, 38);
            this.ConnectBtn.TabIndex = 20;
            this.ConnectBtn.Text = "Connect";
            this.ConnectBtn.UseVisualStyleBackColor = false;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectPortBtn_Click);
            // 
            // SetGrp
            // 
            this.SetGrp.Controls.Add(this.IsEditChk);
            this.SetGrp.Controls.Add(this.ResetBtn);
            this.SetGrp.Controls.Add(this.SetHomeGrp);
            this.SetGrp.Controls.Add(this.SetBtn);
            this.SetGrp.Controls.Add(this.CoordinateGrp);
            this.SetGrp.Controls.Add(this.PivotGrp);
            this.SetGrp.Controls.Add(this.SpeedLevelGrp);
            this.SetGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.SetGrp.Location = new System.Drawing.Point(371, 12);
            this.SetGrp.Name = "SetGrp";
            this.SetGrp.Size = new System.Drawing.Size(367, 420);
            this.SetGrp.TabIndex = 33;
            this.SetGrp.TabStop = false;
            this.SetGrp.Text = "SET";
            // 
            // IsEditChk
            // 
            this.IsEditChk.AutoSize = true;
            this.IsEditChk.Location = new System.Drawing.Point(152, 19);
            this.IsEditChk.Name = "IsEditChk";
            this.IsEditChk.Size = new System.Drawing.Size(50, 20);
            this.IsEditChk.TabIndex = 332;
            this.IsEditChk.Text = "Edit";
            this.IsEditChk.UseVisualStyleBackColor = true;
            this.IsEditChk.CheckedChanged += new System.EventHandler(this.IsEditChk_CheckedChanged);
            // 
            // ResetBtn
            // 
            this.ResetBtn.BackColor = System.Drawing.SystemColors.Control;
            this.ResetBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ResetBtn.BackgroundImage")));
            this.ResetBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ResetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.ResetBtn.ForeColor = System.Drawing.Color.White;
            this.ResetBtn.Location = new System.Drawing.Point(288, 14);
            this.ResetBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ResetBtn.Name = "ResetBtn";
            this.ResetBtn.Size = new System.Drawing.Size(71, 30);
            this.ResetBtn.TabIndex = 331;
            this.ResetBtn.Text = "Reset";
            this.ResetBtn.UseVisualStyleBackColor = false;
            this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
            // 
            // SetHomeGrp
            // 
            this.SetHomeGrp.Controls.Add(this.homeFileTxt);
            this.SetHomeGrp.Controls.Add(this.label2);
            this.SetHomeGrp.Controls.Add(this.TZHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label4);
            this.SetHomeGrp.Controls.Add(this.TYHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label5);
            this.SetHomeGrp.Controls.Add(this.TXHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label8);
            this.SetHomeGrp.Controls.Add(this.ZHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label9);
            this.SetHomeGrp.Controls.Add(this.YHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label11);
            this.SetHomeGrp.Controls.Add(this.XHomeTxt);
            this.SetHomeGrp.Controls.Add(this.label67);
            this.SetHomeGrp.Controls.Add(this.CurPosToHomeBtn);
            this.SetHomeGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetHomeGrp.Location = new System.Drawing.Point(5, 281);
            this.SetHomeGrp.Name = "SetHomeGrp";
            this.SetHomeGrp.Size = new System.Drawing.Size(352, 127);
            this.SetHomeGrp.TabIndex = 315;
            this.SetHomeGrp.TabStop = false;
            this.SetHomeGrp.Text = "Home";
            // 
            // homeFileTxt
            // 
            this.homeFileTxt.Location = new System.Drawing.Point(140, 97);
            this.homeFileTxt.Name = "homeFileTxt";
            this.homeFileTxt.ReadOnly = true;
            this.homeFileTxt.Size = new System.Drawing.Size(199, 22);
            this.homeFileTxt.TabIndex = 343;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(118, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 16);
            this.label2.TabIndex = 342;
            this.label2.Text = "TZ";
            // 
            // TZHomeTxt
            // 
            this.TZHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TZHomeTxt.Location = new System.Drawing.Point(147, 69);
            this.TZHomeTxt.Name = "TZHomeTxt";
            this.TZHomeTxt.ReadOnly = true;
            this.TZHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.TZHomeTxt.TabIndex = 341;
            this.TZHomeTxt.Text = "100";
            this.TZHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(118, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 16);
            this.label4.TabIndex = 340;
            this.label4.Text = "TY";
            // 
            // TYHomeTxt
            // 
            this.TYHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TYHomeTxt.Location = new System.Drawing.Point(147, 45);
            this.TYHomeTxt.Name = "TYHomeTxt";
            this.TYHomeTxt.ReadOnly = true;
            this.TYHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.TYHomeTxt.TabIndex = 339;
            this.TYHomeTxt.Text = "100";
            this.TYHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(117, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 338;
            this.label5.Text = "TX";
            // 
            // TXHomeTxt
            // 
            this.TXHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TXHomeTxt.Location = new System.Drawing.Point(147, 21);
            this.TXHomeTxt.Name = "TXHomeTxt";
            this.TXHomeTxt.ReadOnly = true;
            this.TXHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.TXHomeTxt.TabIndex = 337;
            this.TXHomeTxt.Text = "100";
            this.TXHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(13, 72);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 16);
            this.label8.TabIndex = 336;
            this.label8.Text = "Z";
            // 
            // ZHomeTxt
            // 
            this.ZHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZHomeTxt.Location = new System.Drawing.Point(34, 69);
            this.ZHomeTxt.Name = "ZHomeTxt";
            this.ZHomeTxt.ReadOnly = true;
            this.ZHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.ZHomeTxt.TabIndex = 335;
            this.ZHomeTxt.Text = "100";
            this.ZHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(13, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(15, 16);
            this.label9.TabIndex = 334;
            this.label9.Text = "Y";
            // 
            // YHomeTxt
            // 
            this.YHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YHomeTxt.Location = new System.Drawing.Point(34, 45);
            this.YHomeTxt.Name = "YHomeTxt";
            this.YHomeTxt.ReadOnly = true;
            this.YHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.YHomeTxt.TabIndex = 333;
            this.YHomeTxt.Text = "100";
            this.YHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(12, 24);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(16, 16);
            this.label11.TabIndex = 332;
            this.label11.Text = "X";
            // 
            // XHomeTxt
            // 
            this.XHomeTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XHomeTxt.Location = new System.Drawing.Point(34, 21);
            this.XHomeTxt.Name = "XHomeTxt";
            this.XHomeTxt.ReadOnly = true;
            this.XHomeTxt.Size = new System.Drawing.Size(67, 22);
            this.XHomeTxt.TabIndex = 331;
            this.XHomeTxt.Text = "100";
            this.XHomeTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.Font = new System.Drawing.Font("Arial", 9F);
            this.label67.Location = new System.Drawing.Point(13, 101);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(121, 15);
            this.label67.TabIndex = 329;
            this.label67.Text = "Logic Origin FilePath";
            // 
            // CurPosToHomeBtn
            // 
            this.CurPosToHomeBtn.BackColor = System.Drawing.SystemColors.Control;
            this.CurPosToHomeBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CurPosToHomeBtn.BackgroundImage")));
            this.CurPosToHomeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CurPosToHomeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CurPosToHomeBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.CurPosToHomeBtn.ForeColor = System.Drawing.Color.White;
            this.CurPosToHomeBtn.Location = new System.Drawing.Point(222, 21);
            this.CurPosToHomeBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.CurPosToHomeBtn.Name = "CurPosToHomeBtn";
            this.CurPosToHomeBtn.Size = new System.Drawing.Size(117, 70);
            this.CurPosToHomeBtn.TabIndex = 328;
            this.CurPosToHomeBtn.Text = "Current Position \r\nTo Home";
            this.CurPosToHomeBtn.UseVisualStyleBackColor = false;
            this.CurPosToHomeBtn.Click += new System.EventHandler(this.CurPosToHomeBtn_Click);
            // 
            // SetBtn
            // 
            this.SetBtn.BackColor = System.Drawing.SystemColors.Control;
            this.SetBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetBtn.BackgroundImage")));
            this.SetBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SetBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.SetBtn.ForeColor = System.Drawing.Color.White;
            this.SetBtn.Location = new System.Drawing.Point(209, 14);
            this.SetBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.SetBtn.Name = "SetBtn";
            this.SetBtn.Size = new System.Drawing.Size(71, 31);
            this.SetBtn.TabIndex = 330;
            this.SetBtn.Text = "Set";
            this.SetBtn.UseVisualStyleBackColor = false;
            this.SetBtn.Click += new System.EventHandler(this.SetBtn_Click);
            // 
            // CoordinateGrp
            // 
            this.CoordinateGrp.Controls.Add(this.label74);
            this.CoordinateGrp.Controls.Add(this.TZCSTxt);
            this.CoordinateGrp.Controls.Add(this.label75);
            this.CoordinateGrp.Controls.Add(this.TYCSTxt);
            this.CoordinateGrp.Controls.Add(this.label76);
            this.CoordinateGrp.Controls.Add(this.TXCSTxt);
            this.CoordinateGrp.Controls.Add(this.label77);
            this.CoordinateGrp.Controls.Add(this.ZCSTxt);
            this.CoordinateGrp.Controls.Add(this.label78);
            this.CoordinateGrp.Controls.Add(this.YCSTxt);
            this.CoordinateGrp.Controls.Add(this.label79);
            this.CoordinateGrp.Controls.Add(this.XCSTxt);
            this.CoordinateGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoordinateGrp.Location = new System.Drawing.Point(6, 48);
            this.CoordinateGrp.Name = "CoordinateGrp";
            this.CoordinateGrp.Size = new System.Drawing.Size(229, 101);
            this.CoordinateGrp.TabIndex = 314;
            this.CoordinateGrp.TabStop = false;
            this.CoordinateGrp.Text = "Coordinate System";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label74.Location = new System.Drawing.Point(119, 72);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(22, 16);
            this.label74.TabIndex = 320;
            this.label74.Text = "TZ";
            // 
            // TZCSTxt
            // 
            this.TZCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TZCSTxt.Location = new System.Drawing.Point(148, 69);
            this.TZCSTxt.Name = "TZCSTxt";
            this.TZCSTxt.ReadOnly = true;
            this.TZCSTxt.Size = new System.Drawing.Size(67, 22);
            this.TZCSTxt.TabIndex = 319;
            this.TZCSTxt.Text = "100";
            this.TZCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label75.Location = new System.Drawing.Point(119, 48);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(23, 16);
            this.label75.TabIndex = 318;
            this.label75.Text = "TY";
            // 
            // TYCSTxt
            // 
            this.TYCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TYCSTxt.Location = new System.Drawing.Point(148, 45);
            this.TYCSTxt.Name = "TYCSTxt";
            this.TYCSTxt.ReadOnly = true;
            this.TYCSTxt.Size = new System.Drawing.Size(67, 22);
            this.TYCSTxt.TabIndex = 317;
            this.TYCSTxt.Text = "100";
            this.TYCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label76.Location = new System.Drawing.Point(118, 24);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(24, 16);
            this.label76.TabIndex = 316;
            this.label76.Text = "TX";
            // 
            // TXCSTxt
            // 
            this.TXCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TXCSTxt.Location = new System.Drawing.Point(148, 21);
            this.TXCSTxt.Name = "TXCSTxt";
            this.TXCSTxt.ReadOnly = true;
            this.TXCSTxt.Size = new System.Drawing.Size(67, 22);
            this.TXCSTxt.TabIndex = 315;
            this.TXCSTxt.Text = "100";
            this.TXCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label77.Location = new System.Drawing.Point(14, 72);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(14, 16);
            this.label77.TabIndex = 314;
            this.label77.Text = "Z";
            // 
            // ZCSTxt
            // 
            this.ZCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZCSTxt.Location = new System.Drawing.Point(35, 69);
            this.ZCSTxt.Name = "ZCSTxt";
            this.ZCSTxt.ReadOnly = true;
            this.ZCSTxt.Size = new System.Drawing.Size(67, 22);
            this.ZCSTxt.TabIndex = 313;
            this.ZCSTxt.Text = "100";
            this.ZCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label78.Location = new System.Drawing.Point(14, 48);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(15, 16);
            this.label78.TabIndex = 312;
            this.label78.Text = "Y";
            // 
            // YCSTxt
            // 
            this.YCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YCSTxt.Location = new System.Drawing.Point(35, 45);
            this.YCSTxt.Name = "YCSTxt";
            this.YCSTxt.ReadOnly = true;
            this.YCSTxt.Size = new System.Drawing.Size(67, 22);
            this.YCSTxt.TabIndex = 311;
            this.YCSTxt.Text = "100";
            this.YCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label79.Location = new System.Drawing.Point(13, 24);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(16, 16);
            this.label79.TabIndex = 310;
            this.label79.Text = "X";
            // 
            // XCSTxt
            // 
            this.XCSTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XCSTxt.Location = new System.Drawing.Point(35, 21);
            this.XCSTxt.Name = "XCSTxt";
            this.XCSTxt.ReadOnly = true;
            this.XCSTxt.Size = new System.Drawing.Size(67, 22);
            this.XCSTxt.TabIndex = 42;
            this.XCSTxt.Text = "100";
            this.XCSTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PivotGrp
            // 
            this.PivotGrp.Controls.Add(this.label71);
            this.PivotGrp.Controls.Add(this.label72);
            this.PivotGrp.Controls.Add(this.label73);
            this.PivotGrp.Controls.Add(this.ZPivotTxt);
            this.PivotGrp.Controls.Add(this.YPivotTxt);
            this.PivotGrp.Controls.Add(this.XPivotTxt);
            this.PivotGrp.Location = new System.Drawing.Point(241, 50);
            this.PivotGrp.Name = "PivotGrp";
            this.PivotGrp.Size = new System.Drawing.Size(116, 99);
            this.PivotGrp.TabIndex = 313;
            this.PivotGrp.TabStop = false;
            this.PivotGrp.Text = "Pivot";
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label71.Location = new System.Drawing.Point(14, 71);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(14, 16);
            this.label71.TabIndex = 322;
            this.label71.Text = "Z";
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label72.Location = new System.Drawing.Point(14, 47);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(15, 16);
            this.label72.TabIndex = 321;
            this.label72.Text = "Y";
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label73.Location = new System.Drawing.Point(13, 23);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(16, 16);
            this.label73.TabIndex = 320;
            this.label73.Text = "X";
            // 
            // ZPivotTxt
            // 
            this.ZPivotTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZPivotTxt.Location = new System.Drawing.Point(36, 69);
            this.ZPivotTxt.Name = "ZPivotTxt";
            this.ZPivotTxt.ReadOnly = true;
            this.ZPivotTxt.Size = new System.Drawing.Size(67, 22);
            this.ZPivotTxt.TabIndex = 319;
            this.ZPivotTxt.Text = "100";
            this.ZPivotTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // YPivotTxt
            // 
            this.YPivotTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YPivotTxt.Location = new System.Drawing.Point(36, 45);
            this.YPivotTxt.Name = "YPivotTxt";
            this.YPivotTxt.ReadOnly = true;
            this.YPivotTxt.Size = new System.Drawing.Size(67, 22);
            this.YPivotTxt.TabIndex = 318;
            this.YPivotTxt.Text = "100";
            this.YPivotTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // XPivotTxt
            // 
            this.XPivotTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XPivotTxt.Location = new System.Drawing.Point(36, 21);
            this.XPivotTxt.Name = "XPivotTxt";
            this.XPivotTxt.ReadOnly = true;
            this.XPivotTxt.Size = new System.Drawing.Size(67, 22);
            this.XPivotTxt.TabIndex = 317;
            this.XPivotTxt.Text = "100";
            this.XPivotTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SpeedLevelGrp
            // 
            this.SpeedLevelGrp.Controls.Add(this.NormalSRdo);
            this.SpeedLevelGrp.Controls.Add(this.SlowSRdo);
            this.SpeedLevelGrp.Controls.Add(this.FastSRdo);
            this.SpeedLevelGrp.Controls.Add(this.label70);
            this.SpeedLevelGrp.Controls.Add(this.TZSpeedLevelTxt);
            this.SpeedLevelGrp.Controls.Add(this.label69);
            this.SpeedLevelGrp.Controls.Add(this.TYSpeedLevelTxt);
            this.SpeedLevelGrp.Controls.Add(this.label47);
            this.SpeedLevelGrp.Controls.Add(this.TXSpeedLevelTxt);
            this.SpeedLevelGrp.Controls.Add(this.label46);
            this.SpeedLevelGrp.Controls.Add(this.ZSpeedLevelTxt);
            this.SpeedLevelGrp.Controls.Add(this.label45);
            this.SpeedLevelGrp.Controls.Add(this.YSpeedLevelTxt);
            this.SpeedLevelGrp.Controls.Add(this.label44);
            this.SpeedLevelGrp.Controls.Add(this.XSpeedLevelTxt);
            this.SpeedLevelGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpeedLevelGrp.Location = new System.Drawing.Point(6, 157);
            this.SpeedLevelGrp.Name = "SpeedLevelGrp";
            this.SpeedLevelGrp.Size = new System.Drawing.Size(353, 105);
            this.SpeedLevelGrp.TabIndex = 63;
            this.SpeedLevelGrp.TabStop = false;
            this.SpeedLevelGrp.Text = "Speed Level";
            // 
            // NormalSRdo
            // 
            this.NormalSRdo.AutoSize = true;
            this.NormalSRdo.Checked = true;
            this.NormalSRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.NormalSRdo.Location = new System.Drawing.Point(146, 17);
            this.NormalSRdo.Name = "NormalSRdo";
            this.NormalSRdo.Size = new System.Drawing.Size(62, 20);
            this.NormalSRdo.TabIndex = 324;
            this.NormalSRdo.TabStop = true;
            this.NormalSRdo.Text = "Nomal";
            this.NormalSRdo.UseVisualStyleBackColor = true;
            this.NormalSRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelSRdo_CheckedChanged);
            // 
            // SlowSRdo
            // 
            this.SlowSRdo.AutoSize = true;
            this.SlowSRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.SlowSRdo.Location = new System.Drawing.Point(78, 17);
            this.SlowSRdo.Name = "SlowSRdo";
            this.SlowSRdo.Size = new System.Drawing.Size(53, 20);
            this.SlowSRdo.TabIndex = 322;
            this.SlowSRdo.Text = "Slow";
            this.SlowSRdo.UseVisualStyleBackColor = true;
            this.SlowSRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelSRdo_CheckedChanged);
            // 
            // FastSRdo
            // 
            this.FastSRdo.AutoSize = true;
            this.FastSRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.FastSRdo.Location = new System.Drawing.Point(223, 17);
            this.FastSRdo.Name = "FastSRdo";
            this.FastSRdo.Size = new System.Drawing.Size(51, 20);
            this.FastSRdo.TabIndex = 323;
            this.FastSRdo.Text = "Fast";
            this.FastSRdo.UseVisualStyleBackColor = true;
            this.FastSRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelSRdo_CheckedChanged);
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label70.Location = new System.Drawing.Point(235, 75);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(22, 16);
            this.label70.TabIndex = 320;
            this.label70.Text = "TZ";
            // 
            // TZSpeedLevelTxt
            // 
            this.TZSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TZSpeedLevelTxt.Location = new System.Drawing.Point(263, 72);
            this.TZSpeedLevelTxt.Name = "TZSpeedLevelTxt";
            this.TZSpeedLevelTxt.ReadOnly = true;
            this.TZSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.TZSpeedLevelTxt.TabIndex = 319;
            this.TZSpeedLevelTxt.Text = "100";
            this.TZSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label69.Location = new System.Drawing.Point(129, 75);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(23, 16);
            this.label69.TabIndex = 318;
            this.label69.Text = "TY";
            // 
            // TYSpeedLevelTxt
            // 
            this.TYSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TYSpeedLevelTxt.Location = new System.Drawing.Point(158, 72);
            this.TYSpeedLevelTxt.Name = "TYSpeedLevelTxt";
            this.TYSpeedLevelTxt.ReadOnly = true;
            this.TYSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.TYSpeedLevelTxt.TabIndex = 317;
            this.TYSpeedLevelTxt.Text = "100";
            this.TYSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label47.Location = new System.Drawing.Point(22, 75);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(24, 16);
            this.label47.TabIndex = 316;
            this.label47.Text = "TX";
            // 
            // TXSpeedLevelTxt
            // 
            this.TXSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TXSpeedLevelTxt.Location = new System.Drawing.Point(52, 72);
            this.TXSpeedLevelTxt.Name = "TXSpeedLevelTxt";
            this.TXSpeedLevelTxt.ReadOnly = true;
            this.TXSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.TXSpeedLevelTxt.TabIndex = 315;
            this.TXSpeedLevelTxt.Text = "100";
            this.TXSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label46.Location = new System.Drawing.Point(239, 47);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(14, 16);
            this.label46.TabIndex = 314;
            this.label46.Text = "Z";
            // 
            // ZSpeedLevelTxt
            // 
            this.ZSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZSpeedLevelTxt.Location = new System.Drawing.Point(263, 44);
            this.ZSpeedLevelTxt.Name = "ZSpeedLevelTxt";
            this.ZSpeedLevelTxt.ReadOnly = true;
            this.ZSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.ZSpeedLevelTxt.TabIndex = 313;
            this.ZSpeedLevelTxt.Text = "100";
            this.ZSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label45.Location = new System.Drawing.Point(133, 47);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(15, 16);
            this.label45.TabIndex = 312;
            this.label45.Text = "Y";
            // 
            // YSpeedLevelTxt
            // 
            this.YSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YSpeedLevelTxt.Location = new System.Drawing.Point(158, 44);
            this.YSpeedLevelTxt.Name = "YSpeedLevelTxt";
            this.YSpeedLevelTxt.ReadOnly = true;
            this.YSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.YSpeedLevelTxt.TabIndex = 311;
            this.YSpeedLevelTxt.Text = "100";
            this.YSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label44.Location = new System.Drawing.Point(26, 47);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(16, 16);
            this.label44.TabIndex = 310;
            this.label44.Text = "X";
            // 
            // XSpeedLevelTxt
            // 
            this.XSpeedLevelTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XSpeedLevelTxt.Location = new System.Drawing.Point(52, 44);
            this.XSpeedLevelTxt.Name = "XSpeedLevelTxt";
            this.XSpeedLevelTxt.ReadOnly = true;
            this.XSpeedLevelTxt.Size = new System.Drawing.Size(67, 22);
            this.XSpeedLevelTxt.TabIndex = 42;
            this.XSpeedLevelTxt.Text = "100";
            this.XSpeedLevelTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // XGrp
            // 
            this.XGrp.Controls.Add(this.XIsReadyLbl);
            this.XGrp.Controls.Add(this.label19);
            this.XGrp.Controls.Add(this.XIsHomeLbl);
            this.XGrp.Controls.Add(this.label16);
            this.XGrp.Controls.Add(this.XIsNegLimitTxt);
            this.XGrp.Controls.Add(this.XIsPosLimitLbl);
            this.XGrp.Controls.Add(this.XIsBusyLbl);
            this.XGrp.Controls.Add(this.label49);
            this.XGrp.Controls.Add(this.label6);
            this.XGrp.Controls.Add(this.label10);
            this.XGrp.Controls.Add(this.XSpeedTxt);
            this.XGrp.Controls.Add(this.XAlarmLbl);
            this.XGrp.Controls.Add(this.label7);
            this.XGrp.Controls.Add(this.label3);
            this.XGrp.Controls.Add(this.XCurPosTxt);
            this.XGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.XGrp.Location = new System.Drawing.Point(16, 440);
            this.XGrp.Name = "XGrp";
            this.XGrp.Size = new System.Drawing.Size(175, 149);
            this.XGrp.TabIndex = 40;
            this.XGrp.TabStop = false;
            this.XGrp.Text = "X";
            // 
            // XIsReadyLbl
            // 
            this.XIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.XIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.XIsReadyLbl.Name = "XIsReadyLbl";
            this.XIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.XIsReadyLbl.TabIndex = 86;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(88, 119);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(47, 16);
            this.label19.TabIndex = 85;
            this.label19.Text = "Ready";
            // 
            // XIsHomeLbl
            // 
            this.XIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.XIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.XIsHomeLbl.Name = "XIsHomeLbl";
            this.XIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.XIsHomeLbl.TabIndex = 84;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(88, 93);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(44, 16);
            this.label16.TabIndex = 83;
            this.label16.Text = "Home";
            // 
            // XIsNegLimitTxt
            // 
            this.XIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.XIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.XIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.XIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.XIsNegLimitTxt.Name = "XIsNegLimitTxt";
            this.XIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.XIsNegLimitTxt.TabIndex = 82;
            this.XIsNegLimitTxt.Text = "-";
            this.XIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // XIsPosLimitLbl
            // 
            this.XIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.XIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.XIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.XIsPosLimitLbl.Name = "XIsPosLimitLbl";
            this.XIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.XIsPosLimitLbl.TabIndex = 81;
            this.XIsPosLimitLbl.Text = "+";
            this.XIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // XIsBusyLbl
            // 
            this.XIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.XIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.XIsBusyLbl.Name = "XIsBusyLbl";
            this.XIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.XIsBusyLbl.TabIndex = 80;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(126, 14);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(39, 16);
            this.label49.TabIndex = 75;
            this.label49.Text = "Limit";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 16);
            this.label6.TabIndex = 74;
            this.label6.Text = "Busy";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 63);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 16);
            this.label10.TabIndex = 70;
            this.label10.Text = "Speed";
            // 
            // XSpeedTxt
            // 
            this.XSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.XSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.XSpeedTxt.Name = "XSpeedTxt";
            this.XSpeedTxt.ReadOnly = true;
            this.XSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.XSpeedTxt.TabIndex = 69;
            this.XSpeedTxt.Text = "100";
            this.XSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // XAlarmLbl
            // 
            this.XAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.XAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.XAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.XAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.XAlarmLbl.Name = "XAlarmLbl";
            this.XAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.XAlarmLbl.TabIndex = 65;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 16);
            this.label7.TabIndex = 56;
            this.label7.Text = "Positin";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 16);
            this.label3.TabIndex = 48;
            this.label3.Text = "Alarm";
            // 
            // XCurPosTxt
            // 
            this.XCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.XCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.XCurPosTxt.Name = "XCurPosTxt";
            this.XCurPosTxt.ReadOnly = true;
            this.XCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.XCurPosTxt.TabIndex = 45;
            this.XCurPosTxt.Text = "100";
            this.XCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // HomeGrp
            // 
            this.HomeGrp.Controls.Add(this.label35);
            this.HomeGrp.Controls.Add(this.label34);
            this.HomeGrp.Controls.Add(this.StopBtn);
            this.HomeGrp.Controls.Add(this.VisionHomeBtn);
            this.HomeGrp.Controls.Add(this.MechOriginBtn);
            this.HomeGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.HomeGrp.Location = new System.Drawing.Point(12, 255);
            this.HomeGrp.Name = "HomeGrp";
            this.HomeGrp.Size = new System.Drawing.Size(314, 101);
            this.HomeGrp.TabIndex = 60;
            this.HomeGrp.TabStop = false;
            this.HomeGrp.Text = "Home";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Arial", 9F);
            this.label35.Location = new System.Drawing.Point(114, 83);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(180, 15);
            this.label35.TabIndex = 332;
            this.label35.Text = "비전에서 저장한 home으로 이동";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Arial", 9F);
            this.label34.Location = new System.Drawing.Point(17, 21);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(109, 15);
            this.label34.TabIndex = 331;
            this.label34.Text = "모터 원점으로 이동";
            // 
            // StopBtn
            // 
            this.StopBtn.BackColor = System.Drawing.SystemColors.Control;
            this.StopBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("StopBtn.BackgroundImage")));
            this.StopBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.StopBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopBtn.ForeColor = System.Drawing.Color.White;
            this.StopBtn.Location = new System.Drawing.Point(214, 23);
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.Size = new System.Drawing.Size(85, 63);
            this.StopBtn.TabIndex = 47;
            this.StopBtn.Text = "Stop";
            this.StopBtn.UseVisualStyleBackColor = false;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
            // 
            // VisionHomeBtn
            // 
            this.VisionHomeBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("VisionHomeBtn.BackgroundImage")));
            this.VisionHomeBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.VisionHomeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.VisionHomeBtn.ForeColor = System.Drawing.Color.White;
            this.VisionHomeBtn.Location = new System.Drawing.Point(115, 23);
            this.VisionHomeBtn.Name = "VisionHomeBtn";
            this.VisionHomeBtn.Size = new System.Drawing.Size(85, 63);
            this.VisionHomeBtn.TabIndex = 44;
            this.VisionHomeBtn.Text = "Vision Home";
            this.VisionHomeBtn.UseVisualStyleBackColor = true;
            this.VisionHomeBtn.Click += new System.EventHandler(this.VisionHomeBtn_Click);
            // 
            // MechOriginBtn
            // 
            this.MechOriginBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MechOriginBtn.BackgroundImage")));
            this.MechOriginBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MechOriginBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MechOriginBtn.ForeColor = System.Drawing.Color.White;
            this.MechOriginBtn.Location = new System.Drawing.Point(16, 23);
            this.MechOriginBtn.Name = "MechOriginBtn";
            this.MechOriginBtn.Size = new System.Drawing.Size(85, 63);
            this.MechOriginBtn.TabIndex = 43;
            this.MechOriginBtn.Text = "Mecha Home";
            this.MechOriginBtn.UseVisualStyleBackColor = true;
            this.MechOriginBtn.Click += new System.EventHandler(this.MechOriginBtn_Click);
            // 
            // ComPortCmb
            // 
            this.ComPortCmb.Font = new System.Drawing.Font("Arial", 9F);
            this.ComPortCmb.FormattingEnabled = true;
            this.ComPortCmb.Location = new System.Drawing.Point(37, 30);
            this.ComPortCmb.Name = "ComPortCmb";
            this.ComPortCmb.Size = new System.Drawing.Size(134, 23);
            this.ComPortCmb.TabIndex = 279;
            this.ComPortCmb.DropDown += new System.EventHandler(this.ComPortCmb_DropDown);
            // 
            // IsCunnectedLbl
            // 
            this.IsCunnectedLbl.BackColor = System.Drawing.Color.Silver;
            this.IsCunnectedLbl.Font = new System.Drawing.Font("Arial", 9F);
            this.IsCunnectedLbl.Location = new System.Drawing.Point(310, 16);
            this.IsCunnectedLbl.Name = "IsCunnectedLbl";
            this.IsCunnectedLbl.Size = new System.Drawing.Size(45, 37);
            this.IsCunnectedLbl.TabIndex = 301;
            // 
            // MoveGrp
            // 
            this.MoveGrp.Controls.Add(this.label32);
            this.MoveGrp.Controls.Add(this.NormalMRdo);
            this.MoveGrp.Controls.Add(this.AxisGrp);
            this.MoveGrp.Controls.Add(this.SlowMRdo);
            this.MoveGrp.Controls.Add(this.FastMRdo);
            this.MoveGrp.Controls.Add(this.HomeGrp);
            this.MoveGrp.Controls.Add(this.MoveTypeCbo);
            this.MoveGrp.Controls.Add(this.AxisMovPnl);
            this.MoveGrp.Controls.Add(this.AllAxesMovPnl);
            this.MoveGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.MoveGrp.Location = new System.Drawing.Point(16, 62);
            this.MoveGrp.Name = "MoveGrp";
            this.MoveGrp.Size = new System.Drawing.Size(339, 370);
            this.MoveGrp.TabIndex = 302;
            this.MoveGrp.TabStop = false;
            this.MoveGrp.Text = "Move";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Arial", 9F);
            this.label32.Location = new System.Drawing.Point(35, 352);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(0, 15);
            this.label32.TabIndex = 331;
            // 
            // NormalMRdo
            // 
            this.NormalMRdo.AutoSize = true;
            this.NormalMRdo.Checked = true;
            this.NormalMRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.NormalMRdo.Location = new System.Drawing.Point(199, 79);
            this.NormalMRdo.Name = "NormalMRdo";
            this.NormalMRdo.Size = new System.Drawing.Size(62, 20);
            this.NormalMRdo.TabIndex = 343;
            this.NormalMRdo.TabStop = true;
            this.NormalMRdo.Text = "Nomal";
            this.NormalMRdo.UseVisualStyleBackColor = true;
            this.NormalMRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelMRdo_CheckedChanged);
            // 
            // AxisGrp
            // 
            this.AxisGrp.Controls.Add(this.AllAxesRdo);
            this.AxisGrp.Controls.Add(this.YAxisRdo);
            this.AxisGrp.Controls.Add(this.TZAxisRdo);
            this.AxisGrp.Controls.Add(this.TYAxisRdo);
            this.AxisGrp.Controls.Add(this.TXAxisRdo);
            this.AxisGrp.Controls.Add(this.ZAxisRdo);
            this.AxisGrp.Controls.Add(this.XAxisRdo);
            this.AxisGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AxisGrp.Location = new System.Drawing.Point(12, 21);
            this.AxisGrp.Name = "AxisGrp";
            this.AxisGrp.Size = new System.Drawing.Size(314, 41);
            this.AxisGrp.TabIndex = 23;
            this.AxisGrp.TabStop = false;
            this.AxisGrp.Text = "Axis";
            // 
            // AllAxesRdo
            // 
            this.AllAxesRdo.AutoSize = true;
            this.AllAxesRdo.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.AllAxesRdo.Location = new System.Drawing.Point(269, 15);
            this.AllAxesRdo.Name = "AllAxesRdo";
            this.AllAxesRdo.Size = new System.Drawing.Size(42, 20);
            this.AllAxesRdo.TabIndex = 27;
            this.AllAxesRdo.TabStop = true;
            this.AllAxesRdo.Text = "All";
            this.AllAxesRdo.UseVisualStyleBackColor = true;
            this.AllAxesRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // YAxisRdo
            // 
            this.YAxisRdo.AutoSize = true;
            this.YAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.YAxisRdo.Location = new System.Drawing.Point(54, 15);
            this.YAxisRdo.Name = "YAxisRdo";
            this.YAxisRdo.Size = new System.Drawing.Size(34, 20);
            this.YAxisRdo.TabIndex = 26;
            this.YAxisRdo.TabStop = true;
            this.YAxisRdo.Text = "Y";
            this.YAxisRdo.UseVisualStyleBackColor = true;
            this.YAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // TZAxisRdo
            // 
            this.TZAxisRdo.AutoSize = true;
            this.TZAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TZAxisRdo.Location = new System.Drawing.Point(224, 15);
            this.TZAxisRdo.Name = "TZAxisRdo";
            this.TZAxisRdo.Size = new System.Drawing.Size(39, 20);
            this.TZAxisRdo.TabIndex = 25;
            this.TZAxisRdo.TabStop = true;
            this.TZAxisRdo.Text = "TZ";
            this.TZAxisRdo.UseVisualStyleBackColor = true;
            this.TZAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // TYAxisRdo
            // 
            this.TYAxisRdo.AutoSize = true;
            this.TYAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TYAxisRdo.Location = new System.Drawing.Point(177, 15);
            this.TYAxisRdo.Name = "TYAxisRdo";
            this.TYAxisRdo.Size = new System.Drawing.Size(41, 20);
            this.TYAxisRdo.TabIndex = 24;
            this.TYAxisRdo.TabStop = true;
            this.TYAxisRdo.Text = "TY";
            this.TYAxisRdo.UseVisualStyleBackColor = true;
            this.TYAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // TXAxisRdo
            // 
            this.TXAxisRdo.AutoSize = true;
            this.TXAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TXAxisRdo.Location = new System.Drawing.Point(132, 15);
            this.TXAxisRdo.Name = "TXAxisRdo";
            this.TXAxisRdo.Size = new System.Drawing.Size(39, 20);
            this.TXAxisRdo.TabIndex = 23;
            this.TXAxisRdo.TabStop = true;
            this.TXAxisRdo.Text = "TX";
            this.TXAxisRdo.UseVisualStyleBackColor = true;
            this.TXAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // ZAxisRdo
            // 
            this.ZAxisRdo.AutoSize = true;
            this.ZAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.ZAxisRdo.Location = new System.Drawing.Point(94, 15);
            this.ZAxisRdo.Name = "ZAxisRdo";
            this.ZAxisRdo.Size = new System.Drawing.Size(32, 20);
            this.ZAxisRdo.TabIndex = 22;
            this.ZAxisRdo.TabStop = true;
            this.ZAxisRdo.Text = "Z";
            this.ZAxisRdo.UseVisualStyleBackColor = true;
            this.ZAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // XAxisRdo
            // 
            this.XAxisRdo.AutoSize = true;
            this.XAxisRdo.Checked = true;
            this.XAxisRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.XAxisRdo.Location = new System.Drawing.Point(16, 15);
            this.XAxisRdo.Name = "XAxisRdo";
            this.XAxisRdo.Size = new System.Drawing.Size(32, 20);
            this.XAxisRdo.TabIndex = 21;
            this.XAxisRdo.TabStop = true;
            this.XAxisRdo.Text = "X";
            this.XAxisRdo.UseVisualStyleBackColor = true;
            this.XAxisRdo.CheckedChanged += new System.EventHandler(this.AxisRdo_CheckedChanged);
            // 
            // SlowMRdo
            // 
            this.SlowMRdo.AutoSize = true;
            this.SlowMRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.SlowMRdo.Location = new System.Drawing.Point(141, 79);
            this.SlowMRdo.Name = "SlowMRdo";
            this.SlowMRdo.Size = new System.Drawing.Size(53, 20);
            this.SlowMRdo.TabIndex = 341;
            this.SlowMRdo.Text = "Slow";
            this.SlowMRdo.UseVisualStyleBackColor = true;
            this.SlowMRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelMRdo_CheckedChanged);
            // 
            // FastMRdo
            // 
            this.FastMRdo.AutoSize = true;
            this.FastMRdo.Font = new System.Drawing.Font("Arial", 9.75F);
            this.FastMRdo.Location = new System.Drawing.Point(267, 79);
            this.FastMRdo.Name = "FastMRdo";
            this.FastMRdo.Size = new System.Drawing.Size(51, 20);
            this.FastMRdo.TabIndex = 342;
            this.FastMRdo.Text = "Fast";
            this.FastMRdo.UseVisualStyleBackColor = true;
            this.FastMRdo.CheckedChanged += new System.EventHandler(this.SpeedLevelMRdo_CheckedChanged);
            // 
            // MoveTypeCbo
            // 
            this.MoveTypeCbo.FormattingEnabled = true;
            this.MoveTypeCbo.Location = new System.Drawing.Point(21, 77);
            this.MoveTypeCbo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MoveTypeCbo.Name = "MoveTypeCbo";
            this.MoveTypeCbo.Size = new System.Drawing.Size(110, 24);
            this.MoveTypeCbo.TabIndex = 49;
            // 
            // AxisMovPnl
            // 
            this.AxisMovPnl.Controls.Add(this.label27);
            this.AxisMovPnl.Controls.Add(this.StopAxisBtn);
            this.AxisMovPnl.Controls.Add(this.MoveAxisBtn);
            this.AxisMovPnl.Controls.Add(this.JogMinusBtn);
            this.AxisMovPnl.Controls.Add(this.JogPlusBtn);
            this.AxisMovPnl.Controls.Add(this.label12);
            this.AxisMovPnl.Controls.Add(this.targetPosTxt);
            this.AxisMovPnl.Location = new System.Drawing.Point(19, 107);
            this.AxisMovPnl.Name = "AxisMovPnl";
            this.AxisMovPnl.Size = new System.Drawing.Size(314, 142);
            this.AxisMovPnl.TabIndex = 346;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label27.Location = new System.Drawing.Point(16, 78);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(30, 16);
            this.label27.TabIndex = 345;
            this.label27.Text = "Jog";
            // 
            // StopAxisBtn
            // 
            this.StopAxisBtn.BackColor = System.Drawing.Color.Red;
            this.StopAxisBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("StopAxisBtn.BackgroundImage")));
            this.StopAxisBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.StopAxisBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopAxisBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopAxisBtn.ForeColor = System.Drawing.Color.White;
            this.StopAxisBtn.Location = new System.Drawing.Point(226, 7);
            this.StopAxisBtn.Name = "StopAxisBtn";
            this.StopAxisBtn.Size = new System.Drawing.Size(75, 58);
            this.StopAxisBtn.TabIndex = 344;
            this.StopAxisBtn.Text = "Stop";
            this.StopAxisBtn.UseVisualStyleBackColor = false;
            this.StopAxisBtn.Click += new System.EventHandler(this.StopAxisBtn_Click);
            // 
            // MoveAxisBtn
            // 
            this.MoveAxisBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MoveAxisBtn.BackgroundImage")));
            this.MoveAxisBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MoveAxisBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveAxisBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MoveAxisBtn.ForeColor = System.Drawing.Color.White;
            this.MoveAxisBtn.Location = new System.Drawing.Point(145, 7);
            this.MoveAxisBtn.Name = "MoveAxisBtn";
            this.MoveAxisBtn.Size = new System.Drawing.Size(75, 58);
            this.MoveAxisBtn.TabIndex = 343;
            this.MoveAxisBtn.Text = "Move";
            this.MoveAxisBtn.UseVisualStyleBackColor = true;
            this.MoveAxisBtn.Click += new System.EventHandler(this.MoveAxisBtn_Click);
            // 
            // JogMinusBtn
            // 
            this.JogMinusBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("JogMinusBtn.BackgroundImage")));
            this.JogMinusBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.JogMinusBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.JogMinusBtn.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JogMinusBtn.ForeColor = System.Drawing.Color.White;
            this.JogMinusBtn.Location = new System.Drawing.Point(143, 76);
            this.JogMinusBtn.Name = "JogMinusBtn";
            this.JogMinusBtn.Size = new System.Drawing.Size(75, 58);
            this.JogMinusBtn.TabIndex = 342;
            this.JogMinusBtn.Text = "<<";
            this.JogMinusBtn.UseVisualStyleBackColor = true;
            this.JogMinusBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.JogMinusBtn_MouseDown);
            this.JogMinusBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.JogBtn_MouseUp);
            // 
            // JogPlusBtn
            // 
            this.JogPlusBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("JogPlusBtn.BackgroundImage")));
            this.JogPlusBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.JogPlusBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.JogPlusBtn.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JogPlusBtn.ForeColor = System.Drawing.Color.White;
            this.JogPlusBtn.Location = new System.Drawing.Point(224, 76);
            this.JogPlusBtn.Name = "JogPlusBtn";
            this.JogPlusBtn.Size = new System.Drawing.Size(75, 58);
            this.JogPlusBtn.TabIndex = 341;
            this.JogPlusBtn.Text = ">>";
            this.JogPlusBtn.UseVisualStyleBackColor = true;
            this.JogPlusBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.JogPlusBtn_MouseDown);
            this.JogPlusBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.JogBtn_MouseUp);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(104, 30);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(27, 16);
            this.label12.TabIndex = 336;
            this.label12.Text = "um";
            // 
            // targetPosTxt
            // 
            this.targetPosTxt.Location = new System.Drawing.Point(19, 23);
            this.targetPosTxt.Name = "targetPosTxt";
            this.targetPosTxt.Size = new System.Drawing.Size(81, 22);
            this.targetPosTxt.TabIndex = 335;
            this.targetPosTxt.Text = "0";
            // 
            // AllAxesMovPnl
            // 
            this.AllAxesMovPnl.Controls.Add(this.label13);
            this.AllAxesMovPnl.Controls.Add(this.TZtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.label15);
            this.AllAxesMovPnl.Controls.Add(this.TYtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.label18);
            this.AllAxesMovPnl.Controls.Add(this.TXtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.label20);
            this.AllAxesMovPnl.Controls.Add(this.ZtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.label22);
            this.AllAxesMovPnl.Controls.Add(this.YtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.label23);
            this.AllAxesMovPnl.Controls.Add(this.XtargetPosTxt);
            this.AllAxesMovPnl.Controls.Add(this.Stop6DBtn);
            this.AllAxesMovPnl.Controls.Add(this.Move6DBtn);
            this.AllAxesMovPnl.Location = new System.Drawing.Point(19, 107);
            this.AllAxesMovPnl.Name = "AllAxesMovPnl";
            this.AllAxesMovPnl.Size = new System.Drawing.Size(314, 137);
            this.AllAxesMovPnl.TabIndex = 308;
            this.AllAxesMovPnl.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(115, 98);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(22, 16);
            this.label13.TabIndex = 332;
            this.label13.Text = "TZ";
            // 
            // TZtargetPosTxt
            // 
            this.TZtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TZtargetPosTxt.Location = new System.Drawing.Point(144, 95);
            this.TZtargetPosTxt.Name = "TZtargetPosTxt";
            this.TZtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TZtargetPosTxt.TabIndex = 331;
            this.TZtargetPosTxt.Text = "100";
            this.TZtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(115, 60);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(23, 16);
            this.label15.TabIndex = 330;
            this.label15.Text = "TY";
            // 
            // TYtargetPosTxt
            // 
            this.TYtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TYtargetPosTxt.Location = new System.Drawing.Point(144, 57);
            this.TYtargetPosTxt.Name = "TYtargetPosTxt";
            this.TYtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TYtargetPosTxt.TabIndex = 329;
            this.TYtargetPosTxt.Text = "100";
            this.TYtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(114, 22);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(24, 16);
            this.label18.TabIndex = 328;
            this.label18.Text = "TX";
            // 
            // TXtargetPosTxt
            // 
            this.TXtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TXtargetPosTxt.Location = new System.Drawing.Point(144, 19);
            this.TXtargetPosTxt.Name = "TXtargetPosTxt";
            this.TXtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TXtargetPosTxt.TabIndex = 327;
            this.TXtargetPosTxt.Text = "100";
            this.TXtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(10, 98);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(14, 16);
            this.label20.TabIndex = 326;
            this.label20.Text = "Z";
            // 
            // ZtargetPosTxt
            // 
            this.ZtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZtargetPosTxt.Location = new System.Drawing.Point(31, 95);
            this.ZtargetPosTxt.Name = "ZtargetPosTxt";
            this.ZtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.ZtargetPosTxt.TabIndex = 325;
            this.ZtargetPosTxt.Text = "100";
            this.ZtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(10, 60);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(15, 16);
            this.label22.TabIndex = 324;
            this.label22.Text = "Y";
            // 
            // YtargetPosTxt
            // 
            this.YtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YtargetPosTxt.Location = new System.Drawing.Point(31, 57);
            this.YtargetPosTxt.Name = "YtargetPosTxt";
            this.YtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.YtargetPosTxt.TabIndex = 323;
            this.YtargetPosTxt.Text = "100";
            this.YtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(9, 22);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(16, 16);
            this.label23.TabIndex = 322;
            this.label23.Text = "X";
            // 
            // XtargetPosTxt
            // 
            this.XtargetPosTxt.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XtargetPosTxt.Location = new System.Drawing.Point(31, 19);
            this.XtargetPosTxt.Name = "XtargetPosTxt";
            this.XtargetPosTxt.Size = new System.Drawing.Size(67, 22);
            this.XtargetPosTxt.TabIndex = 321;
            this.XtargetPosTxt.Text = "100";
            this.XtargetPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Stop6DBtn
            // 
            this.Stop6DBtn.BackColor = System.Drawing.Color.Red;
            this.Stop6DBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Stop6DBtn.BackgroundImage")));
            this.Stop6DBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Stop6DBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Stop6DBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Stop6DBtn.ForeColor = System.Drawing.Color.White;
            this.Stop6DBtn.Location = new System.Drawing.Point(222, 71);
            this.Stop6DBtn.Name = "Stop6DBtn";
            this.Stop6DBtn.Size = new System.Drawing.Size(75, 58);
            this.Stop6DBtn.TabIndex = 48;
            this.Stop6DBtn.Text = "Stop";
            this.Stop6DBtn.UseVisualStyleBackColor = false;
            this.Stop6DBtn.Click += new System.EventHandler(this.Stop6DBtn_Click);
            // 
            // Move6DBtn
            // 
            this.Move6DBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Move6DBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Move6DBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Move6DBtn.ForeColor = System.Drawing.Color.White;
            this.Move6DBtn.Location = new System.Drawing.Point(222, 7);
            this.Move6DBtn.Name = "Move6DBtn";
            this.Move6DBtn.Size = new System.Drawing.Size(75, 58);
            this.Move6DBtn.TabIndex = 47;
            this.Move6DBtn.Text = "Move";
            this.Move6DBtn.UseVisualStyleBackColor = true;
            this.Move6DBtn.Click += new System.EventHandler(this.Move6DBtn_Click);
            // 
            // LogText
            // 
            this.LogText.BackColor = System.Drawing.Color.Black;
            this.LogText.ForeColor = System.Drawing.Color.White;
            this.LogText.Location = new System.Drawing.Point(744, 62);
            this.LogText.Name = "LogText";
            this.LogText.ReadOnly = true;
            this.LogText.Size = new System.Drawing.Size(352, 248);
            this.LogText.TabIndex = 306;
            this.LogText.Text = "";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label21.Location = new System.Drawing.Point(750, 26);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(31, 16);
            this.label21.TabIndex = 307;
            this.label21.Text = "Log";
            // 
            // YGrp
            // 
            this.YGrp.Controls.Add(this.YIsReadyLbl);
            this.YGrp.Controls.Add(this.label14);
            this.YGrp.Controls.Add(this.YIsHomeLbl);
            this.YGrp.Controls.Add(this.label17);
            this.YGrp.Controls.Add(this.YIsNegLimitTxt);
            this.YGrp.Controls.Add(this.YIsPosLimitLbl);
            this.YGrp.Controls.Add(this.YIsBusyLbl);
            this.YGrp.Controls.Add(this.label24);
            this.YGrp.Controls.Add(this.label25);
            this.YGrp.Controls.Add(this.label26);
            this.YGrp.Controls.Add(this.YSpeedTxt);
            this.YGrp.Controls.Add(this.YAlarmLbl);
            this.YGrp.Controls.Add(this.label28);
            this.YGrp.Controls.Add(this.label29);
            this.YGrp.Controls.Add(this.YCurPosTxt);
            this.YGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.YGrp.Location = new System.Drawing.Point(197, 440);
            this.YGrp.Name = "YGrp";
            this.YGrp.Size = new System.Drawing.Size(175, 149);
            this.YGrp.TabIndex = 87;
            this.YGrp.TabStop = false;
            this.YGrp.Text = "Y";
            // 
            // YIsReadyLbl
            // 
            this.YIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.YIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.YIsReadyLbl.Name = "YIsReadyLbl";
            this.YIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.YIsReadyLbl.TabIndex = 86;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(88, 119);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(47, 16);
            this.label14.TabIndex = 85;
            this.label14.Text = "Ready";
            // 
            // YIsHomeLbl
            // 
            this.YIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.YIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.YIsHomeLbl.Name = "YIsHomeLbl";
            this.YIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.YIsHomeLbl.TabIndex = 84;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(88, 93);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(44, 16);
            this.label17.TabIndex = 83;
            this.label17.Text = "Home";
            // 
            // YIsNegLimitTxt
            // 
            this.YIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.YIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.YIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.YIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.YIsNegLimitTxt.Name = "YIsNegLimitTxt";
            this.YIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.YIsNegLimitTxt.TabIndex = 82;
            this.YIsNegLimitTxt.Text = "-";
            this.YIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // YIsPosLimitLbl
            // 
            this.YIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.YIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.YIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.YIsPosLimitLbl.Name = "YIsPosLimitLbl";
            this.YIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.YIsPosLimitLbl.TabIndex = 81;
            this.YIsPosLimitLbl.Text = "+";
            this.YIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // YIsBusyLbl
            // 
            this.YIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.YIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.YIsBusyLbl.Name = "YIsBusyLbl";
            this.YIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.YIsBusyLbl.TabIndex = 80;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(126, 14);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(39, 16);
            this.label24.TabIndex = 75;
            this.label24.Text = "Limit";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(10, 119);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(37, 16);
            this.label25.TabIndex = 74;
            this.label25.Text = "Busy";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(10, 63);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(48, 16);
            this.label26.TabIndex = 70;
            this.label26.Text = "Speed";
            // 
            // YSpeedTxt
            // 
            this.YSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.YSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.YSpeedTxt.Name = "YSpeedTxt";
            this.YSpeedTxt.ReadOnly = true;
            this.YSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.YSpeedTxt.TabIndex = 69;
            this.YSpeedTxt.Text = "100";
            this.YSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // YAlarmLbl
            // 
            this.YAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.YAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.YAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.YAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.YAlarmLbl.Name = "YAlarmLbl";
            this.YAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.YAlarmLbl.TabIndex = 65;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(10, 35);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(50, 16);
            this.label28.TabIndex = 56;
            this.label28.Text = "Positin";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(10, 93);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(45, 16);
            this.label29.TabIndex = 48;
            this.label29.Text = "Alarm";
            // 
            // YCurPosTxt
            // 
            this.YCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.YCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.YCurPosTxt.Name = "YCurPosTxt";
            this.YCurPosTxt.ReadOnly = true;
            this.YCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.YCurPosTxt.TabIndex = 45;
            this.YCurPosTxt.Text = "100";
            this.YCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ZGrp
            // 
            this.ZGrp.Controls.Add(this.ZIsReadyLbl);
            this.ZGrp.Controls.Add(this.label31);
            this.ZGrp.Controls.Add(this.ZIsHomeLbl);
            this.ZGrp.Controls.Add(this.label33);
            this.ZGrp.Controls.Add(this.ZIsNegLimitTxt);
            this.ZGrp.Controls.Add(this.ZIsPosLimitLbl);
            this.ZGrp.Controls.Add(this.ZIsBusyLbl);
            this.ZGrp.Controls.Add(this.label37);
            this.ZGrp.Controls.Add(this.label50);
            this.ZGrp.Controls.Add(this.label51);
            this.ZGrp.Controls.Add(this.ZSpeedTxt);
            this.ZGrp.Controls.Add(this.ZAlarmLbl);
            this.ZGrp.Controls.Add(this.label53);
            this.ZGrp.Controls.Add(this.label54);
            this.ZGrp.Controls.Add(this.ZCurPosTxt);
            this.ZGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.ZGrp.Location = new System.Drawing.Point(378, 440);
            this.ZGrp.Name = "ZGrp";
            this.ZGrp.Size = new System.Drawing.Size(175, 149);
            this.ZGrp.TabIndex = 88;
            this.ZGrp.TabStop = false;
            this.ZGrp.Text = "Z";
            // 
            // ZIsReadyLbl
            // 
            this.ZIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.ZIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.ZIsReadyLbl.Name = "ZIsReadyLbl";
            this.ZIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.ZIsReadyLbl.TabIndex = 86;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(88, 119);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(47, 16);
            this.label31.TabIndex = 85;
            this.label31.Text = "Ready";
            // 
            // ZIsHomeLbl
            // 
            this.ZIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.ZIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.ZIsHomeLbl.Name = "ZIsHomeLbl";
            this.ZIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.ZIsHomeLbl.TabIndex = 84;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(88, 93);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(44, 16);
            this.label33.TabIndex = 83;
            this.label33.Text = "Home";
            // 
            // ZIsNegLimitTxt
            // 
            this.ZIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.ZIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ZIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.ZIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.ZIsNegLimitTxt.Name = "ZIsNegLimitTxt";
            this.ZIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.ZIsNegLimitTxt.TabIndex = 82;
            this.ZIsNegLimitTxt.Text = "-";
            this.ZIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ZIsPosLimitLbl
            // 
            this.ZIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.ZIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.ZIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.ZIsPosLimitLbl.Name = "ZIsPosLimitLbl";
            this.ZIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.ZIsPosLimitLbl.TabIndex = 81;
            this.ZIsPosLimitLbl.Text = "+";
            this.ZIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ZIsBusyLbl
            // 
            this.ZIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.ZIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.ZIsBusyLbl.Name = "ZIsBusyLbl";
            this.ZIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.ZIsBusyLbl.TabIndex = 80;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(126, 14);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(39, 16);
            this.label37.TabIndex = 75;
            this.label37.Text = "Limit";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(10, 119);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(37, 16);
            this.label50.TabIndex = 74;
            this.label50.Text = "Busy";
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(10, 63);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(48, 16);
            this.label51.TabIndex = 70;
            this.label51.Text = "Speed";
            // 
            // ZSpeedTxt
            // 
            this.ZSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.ZSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.ZSpeedTxt.Name = "ZSpeedTxt";
            this.ZSpeedTxt.ReadOnly = true;
            this.ZSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.ZSpeedTxt.TabIndex = 69;
            this.ZSpeedTxt.Text = "100";
            this.ZSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ZAlarmLbl
            // 
            this.ZAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.ZAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ZAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ZAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.ZAlarmLbl.Name = "ZAlarmLbl";
            this.ZAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.ZAlarmLbl.TabIndex = 65;
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(10, 35);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(50, 16);
            this.label53.TabIndex = 56;
            this.label53.Text = "Positin";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(10, 93);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(45, 16);
            this.label54.TabIndex = 48;
            this.label54.Text = "Alarm";
            // 
            // ZCurPosTxt
            // 
            this.ZCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.ZCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.ZCurPosTxt.Name = "ZCurPosTxt";
            this.ZCurPosTxt.ReadOnly = true;
            this.ZCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.ZCurPosTxt.TabIndex = 45;
            this.ZCurPosTxt.Text = "100";
            this.ZCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TXGrp
            // 
            this.TXGrp.Controls.Add(this.TXIsReadyLbl);
            this.TXGrp.Controls.Add(this.label56);
            this.TXGrp.Controls.Add(this.TXIsHomeLbl);
            this.TXGrp.Controls.Add(this.label58);
            this.TXGrp.Controls.Add(this.TXIsNegLimitTxt);
            this.TXGrp.Controls.Add(this.TXIsPosLimitLbl);
            this.TXGrp.Controls.Add(this.TXIsBusyLbl);
            this.TXGrp.Controls.Add(this.label62);
            this.TXGrp.Controls.Add(this.label63);
            this.TXGrp.Controls.Add(this.label64);
            this.TXGrp.Controls.Add(this.TXSpeedTxt);
            this.TXGrp.Controls.Add(this.TXAlarmLbl);
            this.TXGrp.Controls.Add(this.label66);
            this.TXGrp.Controls.Add(this.label80);
            this.TXGrp.Controls.Add(this.TXCurPosTxt);
            this.TXGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.TXGrp.Location = new System.Drawing.Point(559, 440);
            this.TXGrp.Name = "TXGrp";
            this.TXGrp.Size = new System.Drawing.Size(175, 149);
            this.TXGrp.TabIndex = 89;
            this.TXGrp.TabStop = false;
            this.TXGrp.Text = "TX";
            // 
            // TXIsReadyLbl
            // 
            this.TXIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TXIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.TXIsReadyLbl.Name = "TXIsReadyLbl";
            this.TXIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.TXIsReadyLbl.TabIndex = 86;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(88, 119);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(47, 16);
            this.label56.TabIndex = 85;
            this.label56.Text = "Ready";
            // 
            // TXIsHomeLbl
            // 
            this.TXIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.TXIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.TXIsHomeLbl.Name = "TXIsHomeLbl";
            this.TXIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.TXIsHomeLbl.TabIndex = 84;
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Location = new System.Drawing.Point(88, 93);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(44, 16);
            this.label58.TabIndex = 83;
            this.label58.Text = "Home";
            // 
            // TXIsNegLimitTxt
            // 
            this.TXIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.TXIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.TXIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TXIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.TXIsNegLimitTxt.Name = "TXIsNegLimitTxt";
            this.TXIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.TXIsNegLimitTxt.TabIndex = 82;
            this.TXIsNegLimitTxt.Text = "-";
            this.TXIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TXIsPosLimitLbl
            // 
            this.TXIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.TXIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TXIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.TXIsPosLimitLbl.Name = "TXIsPosLimitLbl";
            this.TXIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.TXIsPosLimitLbl.TabIndex = 81;
            this.TXIsPosLimitLbl.Text = "+";
            this.TXIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TXIsBusyLbl
            // 
            this.TXIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TXIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.TXIsBusyLbl.Name = "TXIsBusyLbl";
            this.TXIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.TXIsBusyLbl.TabIndex = 80;
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Location = new System.Drawing.Point(126, 14);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(39, 16);
            this.label62.TabIndex = 75;
            this.label62.Text = "Limit";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Location = new System.Drawing.Point(10, 119);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(37, 16);
            this.label63.TabIndex = 74;
            this.label63.Text = "Busy";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Location = new System.Drawing.Point(10, 63);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(48, 16);
            this.label64.TabIndex = 70;
            this.label64.Text = "Speed";
            // 
            // TXSpeedTxt
            // 
            this.TXSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TXSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.TXSpeedTxt.Name = "TXSpeedTxt";
            this.TXSpeedTxt.ReadOnly = true;
            this.TXSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.TXSpeedTxt.TabIndex = 69;
            this.TXSpeedTxt.Text = "100";
            this.TXSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TXAlarmLbl
            // 
            this.TXAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.TXAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TXAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TXAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.TXAlarmLbl.Name = "TXAlarmLbl";
            this.TXAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.TXAlarmLbl.TabIndex = 65;
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Location = new System.Drawing.Point(10, 35);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(50, 16);
            this.label66.TabIndex = 56;
            this.label66.Text = "Positin";
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.Location = new System.Drawing.Point(10, 93);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(45, 16);
            this.label80.TabIndex = 48;
            this.label80.Text = "Alarm";
            // 
            // TXCurPosTxt
            // 
            this.TXCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TXCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.TXCurPosTxt.Name = "TXCurPosTxt";
            this.TXCurPosTxt.ReadOnly = true;
            this.TXCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TXCurPosTxt.TabIndex = 45;
            this.TXCurPosTxt.Text = "100";
            this.TXCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TYGrp
            // 
            this.TYGrp.Controls.Add(this.TYIsReadyLbl);
            this.TYGrp.Controls.Add(this.label82);
            this.TYGrp.Controls.Add(this.TYIsHomeLbl);
            this.TYGrp.Controls.Add(this.label84);
            this.TYGrp.Controls.Add(this.TYIsNegLimitTxt);
            this.TYGrp.Controls.Add(this.TYIsPosLimitLbl);
            this.TYGrp.Controls.Add(this.TYIsBusyLbl);
            this.TYGrp.Controls.Add(this.label88);
            this.TYGrp.Controls.Add(this.label89);
            this.TYGrp.Controls.Add(this.label90);
            this.TYGrp.Controls.Add(this.TYSpeedTxt);
            this.TYGrp.Controls.Add(this.TYAlarmLbl);
            this.TYGrp.Controls.Add(this.label92);
            this.TYGrp.Controls.Add(this.label93);
            this.TYGrp.Controls.Add(this.TYCurPosTxt);
            this.TYGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.TYGrp.Location = new System.Drawing.Point(740, 440);
            this.TYGrp.Name = "TYGrp";
            this.TYGrp.Size = new System.Drawing.Size(175, 149);
            this.TYGrp.TabIndex = 90;
            this.TYGrp.TabStop = false;
            this.TYGrp.Text = "TY";
            // 
            // TYIsReadyLbl
            // 
            this.TYIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TYIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.TYIsReadyLbl.Name = "TYIsReadyLbl";
            this.TYIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.TYIsReadyLbl.TabIndex = 86;
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Location = new System.Drawing.Point(88, 119);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(47, 16);
            this.label82.TabIndex = 85;
            this.label82.Text = "Ready";
            // 
            // TYIsHomeLbl
            // 
            this.TYIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.TYIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.TYIsHomeLbl.Name = "TYIsHomeLbl";
            this.TYIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.TYIsHomeLbl.TabIndex = 84;
            // 
            // label84
            // 
            this.label84.AutoSize = true;
            this.label84.Location = new System.Drawing.Point(88, 93);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(44, 16);
            this.label84.TabIndex = 83;
            this.label84.Text = "Home";
            // 
            // TYIsNegLimitTxt
            // 
            this.TYIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.TYIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.TYIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TYIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.TYIsNegLimitTxt.Name = "TYIsNegLimitTxt";
            this.TYIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.TYIsNegLimitTxt.TabIndex = 82;
            this.TYIsNegLimitTxt.Text = "-";
            this.TYIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TYIsPosLimitLbl
            // 
            this.TYIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.TYIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TYIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.TYIsPosLimitLbl.Name = "TYIsPosLimitLbl";
            this.TYIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.TYIsPosLimitLbl.TabIndex = 81;
            this.TYIsPosLimitLbl.Text = "+";
            this.TYIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TYIsBusyLbl
            // 
            this.TYIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TYIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.TYIsBusyLbl.Name = "TYIsBusyLbl";
            this.TYIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.TYIsBusyLbl.TabIndex = 80;
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Location = new System.Drawing.Point(126, 14);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(39, 16);
            this.label88.TabIndex = 75;
            this.label88.Text = "Limit";
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Location = new System.Drawing.Point(10, 119);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(37, 16);
            this.label89.TabIndex = 74;
            this.label89.Text = "Busy";
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Location = new System.Drawing.Point(10, 63);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(48, 16);
            this.label90.TabIndex = 70;
            this.label90.Text = "Speed";
            // 
            // TYSpeedTxt
            // 
            this.TYSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TYSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.TYSpeedTxt.Name = "TYSpeedTxt";
            this.TYSpeedTxt.ReadOnly = true;
            this.TYSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.TYSpeedTxt.TabIndex = 69;
            this.TYSpeedTxt.Text = "100";
            this.TYSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TYAlarmLbl
            // 
            this.TYAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.TYAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TYAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TYAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.TYAlarmLbl.Name = "TYAlarmLbl";
            this.TYAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.TYAlarmLbl.TabIndex = 65;
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Location = new System.Drawing.Point(10, 35);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(50, 16);
            this.label92.TabIndex = 56;
            this.label92.Text = "Positin";
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Location = new System.Drawing.Point(10, 93);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(45, 16);
            this.label93.TabIndex = 48;
            this.label93.Text = "Alarm";
            // 
            // TYCurPosTxt
            // 
            this.TYCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TYCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.TYCurPosTxt.Name = "TYCurPosTxt";
            this.TYCurPosTxt.ReadOnly = true;
            this.TYCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TYCurPosTxt.TabIndex = 45;
            this.TYCurPosTxt.Text = "100";
            this.TYCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TZGrp
            // 
            this.TZGrp.Controls.Add(this.TZIsReadyLbl);
            this.TZGrp.Controls.Add(this.label95);
            this.TZGrp.Controls.Add(this.TZIsHomeLbl);
            this.TZGrp.Controls.Add(this.label97);
            this.TZGrp.Controls.Add(this.TZIsNegLimitTxt);
            this.TZGrp.Controls.Add(this.TZIsPosLimitLbl);
            this.TZGrp.Controls.Add(this.TZIsBusyLbl);
            this.TZGrp.Controls.Add(this.label101);
            this.TZGrp.Controls.Add(this.label102);
            this.TZGrp.Controls.Add(this.label103);
            this.TZGrp.Controls.Add(this.TZSpeedTxt);
            this.TZGrp.Controls.Add(this.TZAlarmLbl);
            this.TZGrp.Controls.Add(this.label105);
            this.TZGrp.Controls.Add(this.label106);
            this.TZGrp.Controls.Add(this.TZCurPosTxt);
            this.TZGrp.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.TZGrp.Location = new System.Drawing.Point(921, 440);
            this.TZGrp.Name = "TZGrp";
            this.TZGrp.Size = new System.Drawing.Size(175, 149);
            this.TZGrp.TabIndex = 91;
            this.TZGrp.TabStop = false;
            this.TZGrp.Text = "TZ";
            // 
            // TZIsReadyLbl
            // 
            this.TZIsReadyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TZIsReadyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZIsReadyLbl.Location = new System.Drawing.Point(135, 117);
            this.TZIsReadyLbl.Name = "TZIsReadyLbl";
            this.TZIsReadyLbl.Size = new System.Drawing.Size(21, 21);
            this.TZIsReadyLbl.TabIndex = 86;
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Location = new System.Drawing.Point(88, 119);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(47, 16);
            this.label95.TabIndex = 85;
            this.label95.Text = "Ready";
            // 
            // TZIsHomeLbl
            // 
            this.TZIsHomeLbl.BackColor = System.Drawing.Color.LightGray;
            this.TZIsHomeLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZIsHomeLbl.Location = new System.Drawing.Point(135, 91);
            this.TZIsHomeLbl.Name = "TZIsHomeLbl";
            this.TZIsHomeLbl.Size = new System.Drawing.Size(21, 21);
            this.TZIsHomeLbl.TabIndex = 84;
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Location = new System.Drawing.Point(88, 93);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(44, 16);
            this.label97.TabIndex = 83;
            this.label97.Text = "Home";
            // 
            // TZIsNegLimitTxt
            // 
            this.TZIsNegLimitTxt.BackColor = System.Drawing.Color.LightGray;
            this.TZIsNegLimitTxt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZIsNegLimitTxt.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.TZIsNegLimitTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TZIsNegLimitTxt.Location = new System.Drawing.Point(135, 62);
            this.TZIsNegLimitTxt.Name = "TZIsNegLimitTxt";
            this.TZIsNegLimitTxt.Size = new System.Drawing.Size(21, 21);
            this.TZIsNegLimitTxt.TabIndex = 82;
            this.TZIsNegLimitTxt.Text = "-";
            this.TZIsNegLimitTxt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TZIsPosLimitLbl
            // 
            this.TZIsPosLimitLbl.BackColor = System.Drawing.Color.LightGray;
            this.TZIsPosLimitLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZIsPosLimitLbl.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TZIsPosLimitLbl.Location = new System.Drawing.Point(135, 34);
            this.TZIsPosLimitLbl.Name = "TZIsPosLimitLbl";
            this.TZIsPosLimitLbl.Size = new System.Drawing.Size(21, 21);
            this.TZIsPosLimitLbl.TabIndex = 81;
            this.TZIsPosLimitLbl.Text = "+";
            this.TZIsPosLimitLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TZIsBusyLbl
            // 
            this.TZIsBusyLbl.BackColor = System.Drawing.Color.LightGray;
            this.TZIsBusyLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZIsBusyLbl.Location = new System.Drawing.Point(61, 117);
            this.TZIsBusyLbl.Name = "TZIsBusyLbl";
            this.TZIsBusyLbl.Size = new System.Drawing.Size(21, 21);
            this.TZIsBusyLbl.TabIndex = 80;
            // 
            // label101
            // 
            this.label101.AutoSize = true;
            this.label101.Location = new System.Drawing.Point(126, 14);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(39, 16);
            this.label101.TabIndex = 75;
            this.label101.Text = "Limit";
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Location = new System.Drawing.Point(10, 119);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(37, 16);
            this.label102.TabIndex = 74;
            this.label102.Text = "Busy";
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.Location = new System.Drawing.Point(10, 63);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(48, 16);
            this.label103.TabIndex = 70;
            this.label103.Text = "Speed";
            // 
            // TZSpeedTxt
            // 
            this.TZSpeedTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TZSpeedTxt.Location = new System.Drawing.Point(61, 60);
            this.TZSpeedTxt.Name = "TZSpeedTxt";
            this.TZSpeedTxt.ReadOnly = true;
            this.TZSpeedTxt.Size = new System.Drawing.Size(67, 22);
            this.TZSpeedTxt.TabIndex = 69;
            this.TZSpeedTxt.Text = "100";
            this.TZSpeedTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TZAlarmLbl
            // 
            this.TZAlarmLbl.BackColor = System.Drawing.Color.LightGray;
            this.TZAlarmLbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.TZAlarmLbl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.TZAlarmLbl.Location = new System.Drawing.Point(61, 91);
            this.TZAlarmLbl.Name = "TZAlarmLbl";
            this.TZAlarmLbl.Size = new System.Drawing.Size(21, 21);
            this.TZAlarmLbl.TabIndex = 65;
            // 
            // label105
            // 
            this.label105.AutoSize = true;
            this.label105.Location = new System.Drawing.Point(10, 35);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(50, 16);
            this.label105.TabIndex = 56;
            this.label105.Text = "Positin";
            // 
            // label106
            // 
            this.label106.AutoSize = true;
            this.label106.Location = new System.Drawing.Point(10, 93);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(45, 16);
            this.label106.TabIndex = 48;
            this.label106.Text = "Alarm";
            // 
            // TZCurPosTxt
            // 
            this.TZCurPosTxt.Font = new System.Drawing.Font("Arial", 9.75F);
            this.TZCurPosTxt.Location = new System.Drawing.Point(61, 32);
            this.TZCurPosTxt.Name = "TZCurPosTxt";
            this.TZCurPosTxt.ReadOnly = true;
            this.TZCurPosTxt.Size = new System.Drawing.Size(67, 22);
            this.TZCurPosTxt.TabIndex = 45;
            this.TZCurPosTxt.Text = "100";
            this.TZCurPosTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.CloseBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CloseBtn.BackgroundImage")));
            this.CloseBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.CloseBtn.ForeColor = System.Drawing.Color.White;
            this.CloseBtn.Location = new System.Drawing.Point(982, 16);
            this.CloseBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(114, 38);
            this.CloseBtn.TabIndex = 308;
            this.CloseBtn.Text = "Close";
            this.CloseBtn.UseVisualStyleBackColor = false;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // LogClearBtn
            // 
            this.LogClearBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.LogClearBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LogClearBtn.BackgroundImage")));
            this.LogClearBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.LogClearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LogClearBtn.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.LogClearBtn.ForeColor = System.Drawing.Color.White;
            this.LogClearBtn.Location = new System.Drawing.Point(901, 16);
            this.LogClearBtn.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.LogClearBtn.Name = "LogClearBtn";
            this.LogClearBtn.Size = new System.Drawing.Size(71, 38);
            this.LogClearBtn.TabIndex = 309;
            this.LogClearBtn.Text = "Clear";
            this.LogClearBtn.UseVisualStyleBackColor = false;
            this.LogClearBtn.Click += new System.EventHandler(this.LogClearBtn_Click);
            // 
            // ReferenceHexapodBtn
            // 
            this.ReferenceHexapodBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ReferenceHexapodBtn.BackgroundImage")));
            this.ReferenceHexapodBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ReferenceHexapodBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReferenceHexapodBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.ReferenceHexapodBtn.ForeColor = System.Drawing.Color.White;
            this.ReferenceHexapodBtn.Location = new System.Drawing.Point(844, 341);
            this.ReferenceHexapodBtn.Name = "ReferenceHexapodBtn";
            this.ReferenceHexapodBtn.Size = new System.Drawing.Size(85, 63);
            this.ReferenceHexapodBtn.TabIndex = 310;
            this.ReferenceHexapodBtn.Text = "Reference Hexapod";
            this.ReferenceHexapodBtn.UseVisualStyleBackColor = true;
            this.ReferenceHexapodBtn.Click += new System.EventHandler(this.ReferenceHexapodBtn_Click);
            // 
            // ReferencXYZBtn
            // 
            this.ReferencXYZBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ReferencXYZBtn.BackgroundImage")));
            this.ReferencXYZBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ReferencXYZBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReferencXYZBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.ReferencXYZBtn.ForeColor = System.Drawing.Color.White;
            this.ReferencXYZBtn.Location = new System.Drawing.Point(753, 340);
            this.ReferencXYZBtn.Name = "ReferencXYZBtn";
            this.ReferencXYZBtn.Size = new System.Drawing.Size(85, 63);
            this.ReferencXYZBtn.TabIndex = 311;
            this.ReferencXYZBtn.Text = "Reference XYZ Stage";
            this.ReferencXYZBtn.UseVisualStyleBackColor = true;
            this.ReferencXYZBtn.Click += new System.EventHandler(this.ReferencXYZBtn_Click);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Arial", 9F);
            this.label30.Location = new System.Drawing.Point(750, 323);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(241, 15);
            this.label30.TabIndex = 330;
            this.label30.Text = "Referencing(원점 찾아 이동 후 위치 초기화)\r\n";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("Arial", 9F);
            this.label36.Location = new System.Drawing.Point(750, 414);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(222, 15);
            this.label36.TabIndex = 331;
            this.label36.Text = "헥사포드의 x,y,z 실시간 위치도 필요한가.";
            // 
            // btnXYZTest
            // 
            this.btnXYZTest.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnXYZTest.BackgroundImage")));
            this.btnXYZTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnXYZTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnXYZTest.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnXYZTest.ForeColor = System.Drawing.Color.White;
            this.btnXYZTest.Location = new System.Drawing.Point(1001, 342);
            this.btnXYZTest.Name = "btnXYZTest";
            this.btnXYZTest.Size = new System.Drawing.Size(85, 63);
            this.btnXYZTest.TabIndex = 332;
            this.btnXYZTest.Text = "XYZ Test";
            this.btnXYZTest.UseVisualStyleBackColor = true;
            this.btnXYZTest.Click += new System.EventHandler(this.btnXYZTest_Click);
            // 
            // F_Motion_SK_PI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1110, 604);
            this.ControlBox = false;
            this.Controls.Add(this.btnXYZTest);
            this.Controls.Add(this.label36);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.ReferencXYZBtn);
            this.Controls.Add(this.ReferenceHexapodBtn);
            this.Controls.Add(this.LogClearBtn);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.TZGrp);
            this.Controls.Add(this.TYGrp);
            this.Controls.Add(this.TXGrp);
            this.Controls.Add(this.ZGrp);
            this.Controls.Add(this.YGrp);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.LogText);
            this.Controls.Add(this.IsCunnectedLbl);
            this.Controls.Add(this.ComPortCmb);
            this.Controls.Add(this.XGrp);
            this.Controls.Add(this.SetGrp);
            this.Controls.Add(this.ConnectBtn);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.MoveGrp);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "F_Motion_SK_PI";
            this.Text = "Motion";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_Motion_SK_PI_FormClosing);
            this.Load += new System.EventHandler(this.F_Motion_SK_PI_Load);
            this.SetGrp.ResumeLayout(false);
            this.SetGrp.PerformLayout();
            this.SetHomeGrp.ResumeLayout(false);
            this.SetHomeGrp.PerformLayout();
            this.CoordinateGrp.ResumeLayout(false);
            this.CoordinateGrp.PerformLayout();
            this.PivotGrp.ResumeLayout(false);
            this.PivotGrp.PerformLayout();
            this.SpeedLevelGrp.ResumeLayout(false);
            this.SpeedLevelGrp.PerformLayout();
            this.XGrp.ResumeLayout(false);
            this.XGrp.PerformLayout();
            this.HomeGrp.ResumeLayout(false);
            this.HomeGrp.PerformLayout();
            this.MoveGrp.ResumeLayout(false);
            this.MoveGrp.PerformLayout();
            this.AxisGrp.ResumeLayout(false);
            this.AxisGrp.PerformLayout();
            this.AxisMovPnl.ResumeLayout(false);
            this.AxisMovPnl.PerformLayout();
            this.AllAxesMovPnl.ResumeLayout(false);
            this.AllAxesMovPnl.PerformLayout();
            this.YGrp.ResumeLayout(false);
            this.YGrp.PerformLayout();
            this.ZGrp.ResumeLayout(false);
            this.ZGrp.PerformLayout();
            this.TXGrp.ResumeLayout(false);
            this.TXGrp.PerformLayout();
            this.TYGrp.ResumeLayout(false);
            this.TYGrp.PerformLayout();
            this.TZGrp.ResumeLayout(false);
            this.TZGrp.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.GroupBox SetGrp;
        private System.Windows.Forms.GroupBox XGrp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox XCurPosTxt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox HomeGrp;
        private System.Windows.Forms.Button MechOriginBtn;
        private System.Windows.Forms.Button VisionHomeBtn;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox XSpeedTxt;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.Label label6;
        private ComboBox ComPortCmb;
        private Label IsCunnectedLbl;
        private GroupBox SpeedLevelGrp;
        private TextBox XSpeedLevelTxt;
        private Label label44;
        private Label label47;
        private TextBox TXSpeedLevelTxt;
        private Label label46;
        private TextBox ZSpeedLevelTxt;
        private Label label45;
        private TextBox YSpeedLevelTxt;
        private Label label70;
        private TextBox TZSpeedLevelTxt;
        private Label label69;
        private TextBox TYSpeedLevelTxt;
        private GroupBox PivotGrp;
        private RadioButton NormalSRdo;
        private RadioButton SlowSRdo;
        private RadioButton FastSRdo;
        private Label label71;
        private Label label72;
        private Label label73;
        private TextBox ZPivotTxt;
        private TextBox YPivotTxt;
        private TextBox XPivotTxt;
        private GroupBox CoordinateGrp;
        private Label label74;
        private TextBox TZCSTxt;
        private Label label75;
        private TextBox TYCSTxt;
        private Label label76;
        private TextBox TXCSTxt;
        private Label label77;
        private TextBox ZCSTxt;
        private Label label78;
        private TextBox YCSTxt;
        private Label label79;
        private TextBox XCSTxt;
        internal Button ResetBtn;
        internal Button SetBtn;
        private Label XIsBusyLbl;
        private Label XAlarmLbl;
        private GroupBox MoveGrp;
        private GroupBox AxisGrp;
        private RadioButton AllAxesRdo;
        private RadioButton YAxisRdo;
        private RadioButton TZAxisRdo;
        private RadioButton TYAxisRdo;
        private RadioButton TXAxisRdo;
        private RadioButton ZAxisRdo;
        private RadioButton XAxisRdo;
        private Button StopBtn;
        private GroupBox SetHomeGrp;
        internal Button CurPosToHomeBtn;
        private Label label67;
        private RichTextBox LogText;
        private Label XIsPosLimitLbl;
        private Label XIsNegLimitTxt;
        private Label label2;
        private TextBox TZHomeTxt;
        private Label label4;
        private TextBox TYHomeTxt;
        private Label label5;
        private TextBox TXHomeTxt;
        private Label label8;
        private TextBox ZHomeTxt;
        private Label label9;
        private TextBox YHomeTxt;
        private Label label11;
        private TextBox XHomeTxt;
        private Label XIsHomeLbl;
        private Label label16;
        private Label XIsReadyLbl;
        private Label label19;
        private Label label21;
        private GroupBox YGrp;
        private Label YIsReadyLbl;
        private Label label14;
        private Label YIsHomeLbl;
        private Label label17;
        private Label YIsNegLimitTxt;
        private Label YIsPosLimitLbl;
        private Label YIsBusyLbl;
        private Label label24;
        private Label label25;
        private Label label26;
        private TextBox YSpeedTxt;
        private Label YAlarmLbl;
        private Label label28;
        private Label label29;
        private TextBox YCurPosTxt;
        private GroupBox ZGrp;
        private Label ZIsReadyLbl;
        private Label label31;
        private Label ZIsHomeLbl;
        private Label label33;
        private Label ZIsNegLimitTxt;
        private Label ZIsPosLimitLbl;
        private Label ZIsBusyLbl;
        private Label label37;
        private Label label50;
        private Label label51;
        private TextBox ZSpeedTxt;
        private Label ZAlarmLbl;
        private Label label53;
        private Label label54;
        private TextBox ZCurPosTxt;
        private GroupBox TXGrp;
        private Label TXIsReadyLbl;
        private Label label56;
        private Label TXIsHomeLbl;
        private Label label58;
        private Label TXIsNegLimitTxt;
        private Label TXIsPosLimitLbl;
        private Label TXIsBusyLbl;
        private Label label62;
        private Label label63;
        private Label label64;
        private TextBox TXSpeedTxt;
        private Label TXAlarmLbl;
        private Label label66;
        private Label label80;
        private TextBox TXCurPosTxt;
        private GroupBox TYGrp;
        private Label TYIsReadyLbl;
        private Label label82;
        private Label TYIsHomeLbl;
        private Label label84;
        private Label TYIsNegLimitTxt;
        private Label TYIsPosLimitLbl;
        private Label TYIsBusyLbl;
        private Label label88;
        private Label label89;
        private Label label90;
        private TextBox TYSpeedTxt;
        private Label TYAlarmLbl;
        private Label label92;
        private Label label93;
        private TextBox TYCurPosTxt;
        private GroupBox TZGrp;
        private Label TZIsReadyLbl;
        private Label label95;
        private Label TZIsHomeLbl;
        private Label label97;
        private Label TZIsNegLimitTxt;
        private Label TZIsPosLimitLbl;
        private Label TZIsBusyLbl;
        private Label label101;
        private Label label102;
        private Label label103;
        private TextBox TZSpeedTxt;
        private Label TZAlarmLbl;
        private Label label105;
        private Label label106;
        private TextBox TZCurPosTxt;
        private CheckBox IsEditChk;
        private Panel AllAxesMovPnl;
        private ComboBox MoveTypeCbo;
        private Button Stop6DBtn;
        private Button Move6DBtn;
        private Panel AxisMovPnl;
        private RadioButton NormalMRdo;
        private RadioButton SlowMRdo;
        private RadioButton FastMRdo;
        private Label label12;
        private TextBox targetPosTxt;
        private Label label13;
        private TextBox TZtargetPosTxt;
        private Label label15;
        private TextBox TYtargetPosTxt;
        private Label label18;
        private TextBox TXtargetPosTxt;
        private Label label20;
        private TextBox ZtargetPosTxt;
        private Label label22;
        private TextBox YtargetPosTxt;
        private Label label23;
        private TextBox XtargetPosTxt;
        private Button StopAxisBtn;
        private Button MoveAxisBtn;
        private Button JogMinusBtn;
        private Button JogPlusBtn;
        private Label label27;
        internal Button CloseBtn;
        internal Button LogClearBtn;
        private TextBox homeFileTxt;
        private Label label32;
        private Button ReferenceHexapodBtn;
        private Button ReferencXYZBtn;
        private Label label30;
        private Label label34;
        private Label label35;
        private Label label36;
        private Button btnXYZTest;
    }
}

