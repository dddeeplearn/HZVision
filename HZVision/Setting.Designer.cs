namespace HZVision
{
    partial class Setting
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
            this.textBox_threshold = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_filtersize1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_filtersize2 = new System.Windows.Forms.TextBox();
            this.textImgNum = new System.Windows.Forms.TextBox();
            this.checkAutoSave = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.butSaveNum = new System.Windows.Forms.Button();
            this.btnConcel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_threshold
            // 
            this.textBox_threshold.Location = new System.Drawing.Point(207, 50);
            this.textBox_threshold.Name = "textBox_threshold";
            this.textBox_threshold.Size = new System.Drawing.Size(100, 28);
            this.textBox_threshold.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 9F);
            this.label1.Location = new System.Drawing.Point(67, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "阈值";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("黑体", 9F);
            this.label2.Location = new System.Drawing.Point(67, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "滤波尺寸1";
            // 
            // textBox_filtersize1
            // 
            this.textBox_filtersize1.Location = new System.Drawing.Point(207, 101);
            this.textBox_filtersize1.Name = "textBox_filtersize1";
            this.textBox_filtersize1.Size = new System.Drawing.Size(100, 28);
            this.textBox_filtersize1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("黑体", 9F);
            this.label3.Location = new System.Drawing.Point(67, 159);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "滤波尺寸2";
            // 
            // textBox_filtersize2
            // 
            this.textBox_filtersize2.Location = new System.Drawing.Point(207, 156);
            this.textBox_filtersize2.Name = "textBox_filtersize2";
            this.textBox_filtersize2.Size = new System.Drawing.Size(100, 28);
            this.textBox_filtersize2.TabIndex = 4;
            // 
            // textImgNum
            // 
            this.textImgNum.Location = new System.Drawing.Point(207, 206);
            this.textImgNum.Name = "textImgNum";
            this.textImgNum.Size = new System.Drawing.Size(100, 28);
            this.textImgNum.TabIndex = 10;
            // 
            // checkAutoSave
            // 
            this.checkAutoSave.AutoSize = true;
            this.checkAutoSave.Checked = true;
            this.checkAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAutoSave.Font = new System.Drawing.Font("黑体", 9F);
            this.checkAutoSave.Location = new System.Drawing.Point(333, 208);
            this.checkAutoSave.Name = "checkAutoSave";
            this.checkAutoSave.Size = new System.Drawing.Size(142, 22);
            this.checkAutoSave.TabIndex = 8;
            this.checkAutoSave.Text = "自动保存图像";
            this.checkAutoSave.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("黑体", 9F);
            this.label5.Location = new System.Drawing.Point(67, 209);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 18);
            this.label5.TabIndex = 9;
            this.label5.Text = "保存图像数量：";
            // 
            // butSaveNum
            // 
            this.butSaveNum.Location = new System.Drawing.Point(275, 282);
            this.butSaveNum.Name = "butSaveNum";
            this.butSaveNum.Size = new System.Drawing.Size(112, 40);
            this.butSaveNum.TabIndex = 11;
            this.butSaveNum.Text = "保存设置";
            this.butSaveNum.UseVisualStyleBackColor = true;
            this.butSaveNum.Click += new System.EventHandler(this.butSaveNum_Click);
            // 
            // btnConcel
            // 
            this.btnConcel.Location = new System.Drawing.Point(393, 282);
            this.btnConcel.Name = "btnConcel";
            this.btnConcel.Size = new System.Drawing.Size(112, 40);
            this.btnConcel.TabIndex = 12;
            this.btnConcel.Text = "取消";
            this.btnConcel.UseVisualStyleBackColor = true;
            this.btnConcel.Click += new System.EventHandler(this.btnConcel_Click);
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 334);
            this.Controls.Add(this.btnConcel);
            this.Controls.Add(this.butSaveNum);
            this.Controls.Add(this.textImgNum);
            this.Controls.Add(this.checkAutoSave);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_filtersize2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_filtersize1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_threshold);
            this.MaximizeBox = false;
            this.Name = "Setting";
            this.Text = "设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_threshold;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_filtersize1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_filtersize2;
        private System.Windows.Forms.TextBox textImgNum;
        private System.Windows.Forms.CheckBox checkAutoSave;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button butSaveNum;
        private System.Windows.Forms.Button btnConcel;
    }
}