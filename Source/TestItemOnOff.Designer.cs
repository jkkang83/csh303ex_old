namespace CSH030Ex
{
    partial class TestItemOnOff
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.GroupName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TestItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TestItemState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TestItemSaveBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GroupName,
            this.TestItemName,
            this.TestItemState});
            this.dataGridView1.Location = new System.Drawing.Point(0, 29);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(474, 897);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            // 
            // GroupName
            // 
            this.GroupName.HeaderText = "Group";
            this.GroupName.Name = "GroupName";
            this.GroupName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // TestItemName
            // 
            this.TestItemName.HeaderText = "Test Item";
            this.TestItemName.Name = "TestItemName";
            this.TestItemName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // TestItemState
            // 
            this.TestItemState.HeaderText = "Enable";
            this.TestItemState.Name = "TestItemState";
            this.TestItemState.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // TestItemSaveBtn
            // 
            this.TestItemSaveBtn.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.TestItemSaveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TestItemSaveBtn.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestItemSaveBtn.Location = new System.Drawing.Point(0, -2);
            this.TestItemSaveBtn.Name = "TestItemSaveBtn";
            this.TestItemSaveBtn.Size = new System.Drawing.Size(474, 28);
            this.TestItemSaveBtn.TabIndex = 1;
            this.TestItemSaveBtn.Text = "Save Test Item Setting";
            this.TestItemSaveBtn.UseVisualStyleBackColor = false;
            this.TestItemSaveBtn.Click += new System.EventHandler(this.TestItemSaveBtn_Click);
            // 
            // TestItemOnOff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(474, 924);
            this.ControlBox = false;
            this.Controls.Add(this.TestItemSaveBtn);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TestItemOnOff";
            this.Text = "Test Item Selection";
            this.Load += new System.EventHandler(this.TestItemOnOff_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button TestItemSaveBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GroupName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TestItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TestItemState;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}