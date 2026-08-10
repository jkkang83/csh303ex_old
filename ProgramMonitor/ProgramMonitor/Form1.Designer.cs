
namespace ProgramMonitor
{
    partial class Form1
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
            this.LogNetwork = new System.Windows.Forms.TextBox();
            this.Restart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LogNetwork
            // 
            this.LogNetwork.BackColor = System.Drawing.Color.Black;
            this.LogNetwork.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogNetwork.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogNetwork.ForeColor = System.Drawing.Color.LemonChiffon;
            this.LogNetwork.Location = new System.Drawing.Point(0, 58);
            this.LogNetwork.Multiline = true;
            this.LogNetwork.Name = "LogNetwork";
            this.LogNetwork.ReadOnly = true;
            this.LogNetwork.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogNetwork.Size = new System.Drawing.Size(469, 319);
            this.LogNetwork.TabIndex = 259;
            this.LogNetwork.Tag = "S";
            // 
            // Restart
            // 
            this.Restart.Location = new System.Drawing.Point(0, 0);
            this.Restart.Name = "Restart";
            this.Restart.Size = new System.Drawing.Size(261, 52);
            this.Restart.TabIndex = 260;
            this.Restart.Text = "Restart";
            this.Restart.UseVisualStyleBackColor = true;
            this.Restart.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 380);
            this.Controls.Add(this.Restart);
            this.Controls.Add(this.LogNetwork);
            this.Name = "Form1";
            this.Text = "CSH Restater";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LogNetwork;
        private System.Windows.Forms.Button Restart;
    }
}

