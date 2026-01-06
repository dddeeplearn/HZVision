namespace HZVision
{
    partial class Authorization
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTip = new System.Windows.Forms.Label();
            this.txtMachineCode = new System.Windows.Forms.TextBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new System.Drawing.Point(25, 23);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(360, 30);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "软件未授权";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTip
            // 
            this.lblTip.Location = new System.Drawing.Point(25, 63);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(360, 20);
            this.lblTip.TabIndex = 5;
            this.lblTip.Text = "请将下面的机器码发送给软件供应商获取授权文件";
            // 
            // txtMachineCode
            // 
            this.txtMachineCode.Location = new System.Drawing.Point(25, 93);
            this.txtMachineCode.Multiline = true;
            this.txtMachineCode.Name = "txtMachineCode";
            this.txtMachineCode.ReadOnly = true;
            this.txtMachineCode.Size = new System.Drawing.Size(360, 80);
            this.txtMachineCode.TabIndex = 6;
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(145, 188);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(120, 30);
            this.btnCopy.TabIndex = 7;
            this.btnCopy.Text = "复制机器码";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click_1);
            // 
            // Authorization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 251);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.txtMachineCode);
            this.Controls.Add(this.btnCopy);
            this.Name = "Authorization";
            this.Text = "软件授权提示";
            this.Load += new System.EventHandler(this.Authorization_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.TextBox txtMachineCode;
        private System.Windows.Forms.Button btnCopy;
    }
}