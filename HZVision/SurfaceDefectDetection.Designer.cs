namespace HZVision
{
    partial class SurfaceDefectDetection
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SurfaceDefectDetection));
            this.labDetectStatus = new System.Windows.Forms.Label();
            this.labResult = new System.Windows.Forms.Label();
            this.btnTrigDetection = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.labTime = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labStatus = new System.Windows.Forms.Label();
            this.labExpouse = new System.Windows.Forms.Label();
            this.butStopRev = new System.Windows.Forms.Button();
            this.buttReadyRev = new System.Windows.Forms.Button();
            this.butConCam = new System.Windows.Forms.Button();
            this.butOpenFile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnConfigSet = new System.Windows.Forms.Button();
            this.hSmartWindowResult = new HalconDotNet.HSmartWindowControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btn_Capture = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labDetectStatus
            // 
            this.labDetectStatus.AutoSize = true;
            this.labDetectStatus.Font = new System.Drawing.Font("黑体", 20F, System.Drawing.FontStyle.Bold);
            this.labDetectStatus.Location = new System.Drawing.Point(1144, 523);
            this.labDetectStatus.Name = "labDetectStatus";
            this.labDetectStatus.Size = new System.Drawing.Size(181, 40);
            this.labDetectStatus.TabIndex = 14;
            this.labDetectStatus.Text = "等待检测";
            this.labDetectStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labResult
            // 
            this.labResult.AutoSize = true;
            this.labResult.Font = new System.Drawing.Font("黑体", 20F);
            this.labResult.Location = new System.Drawing.Point(930, 522);
            this.labResult.Name = "labResult";
            this.labResult.Size = new System.Drawing.Size(217, 40);
            this.labResult.TabIndex = 13;
            this.labResult.Text = "检测结果：";
            this.labResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTrigDetection
            // 
            this.btnTrigDetection.AutoSize = true;
            this.btnTrigDetection.Font = new System.Drawing.Font("黑体", 20F, System.Drawing.FontStyle.Bold);
            this.btnTrigDetection.Location = new System.Drawing.Point(284, 508);
            this.btnTrigDetection.Name = "btnTrigDetection";
            this.btnTrigDetection.Size = new System.Drawing.Size(614, 68);
            this.btnTrigDetection.TabIndex = 12;
            this.btnTrigDetection.Text = "触发检测";
            this.btnTrigDetection.UseVisualStyleBackColor = true;
            this.btnTrigDetection.Click += new System.EventHandler(this.btnTrigDetection_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(284, 587);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(1219, 173);
            this.textBox1.TabIndex = 11;
            this.textBox1.Text = "日志信息：";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox4.Location = new System.Drawing.Point(284, 10);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(614, 481);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 9;
            this.pictureBox4.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.labTime);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.butStopRev);
            this.groupBox1.Controls.Add(this.buttReadyRev);
            this.groupBox1.Controls.Add(this.butConCam);
            this.groupBox1.Controls.Add(this.butOpenFile);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(7, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(251, 755);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "控制";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 394);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "硅片尺寸选择";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(139, 390);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(87, 26);
            this.comboBox1.TabIndex = 8;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // labTime
            // 
            this.labTime.AutoSize = true;
            this.labTime.Font = new System.Drawing.Font("宋体", 10F);
            this.labTime.Location = new System.Drawing.Point(7, 731);
            this.labTime.Name = "labTime";
            this.labTime.Size = new System.Drawing.Size(79, 20);
            this.labTime.TabIndex = 7;
            this.labTime.Text = "labTime";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labStatus);
            this.groupBox3.Controls.Add(this.labExpouse);
            this.groupBox3.Location = new System.Drawing.Point(15, 462);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(220, 96);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            // 
            // labStatus
            // 
            this.labStatus.AutoSize = true;
            this.labStatus.Font = new System.Drawing.Font("黑体", 9F);
            this.labStatus.Location = new System.Drawing.Point(6, 24);
            this.labStatus.Name = "labStatus";
            this.labStatus.Size = new System.Drawing.Size(89, 18);
            this.labStatus.TabIndex = 4;
            this.labStatus.Text = "状态：N/A";
            // 
            // labExpouse
            // 
            this.labExpouse.AutoSize = true;
            this.labExpouse.Font = new System.Drawing.Font("黑体", 9F);
            this.labExpouse.Location = new System.Drawing.Point(6, 58);
            this.labExpouse.Name = "labExpouse";
            this.labExpouse.Size = new System.Drawing.Size(89, 18);
            this.labExpouse.TabIndex = 5;
            this.labExpouse.Text = "曝光：N/A";
            // 
            // butStopRev
            // 
            this.butStopRev.AutoSize = true;
            this.butStopRev.Font = new System.Drawing.Font("宋体", 9F);
            this.butStopRev.Location = new System.Drawing.Point(24, 207);
            this.butStopRev.Name = "butStopRev";
            this.butStopRev.Size = new System.Drawing.Size(202, 45);
            this.butStopRev.TabIndex = 3;
            this.butStopRev.Text = "停止接收";
            this.butStopRev.UseVisualStyleBackColor = true;
            this.butStopRev.Click += new System.EventHandler(this.butStopRev_Click);
            // 
            // buttReadyRev
            // 
            this.buttReadyRev.AutoSize = true;
            this.buttReadyRev.Font = new System.Drawing.Font("宋体", 9F);
            this.buttReadyRev.Location = new System.Drawing.Point(24, 154);
            this.buttReadyRev.Name = "buttReadyRev";
            this.buttReadyRev.Size = new System.Drawing.Size(202, 45);
            this.buttReadyRev.TabIndex = 2;
            this.buttReadyRev.Text = "准备接收";
            this.buttReadyRev.UseVisualStyleBackColor = true;
            this.buttReadyRev.Click += new System.EventHandler(this.buttReadyRev_Click);
            // 
            // butConCam
            // 
            this.butConCam.AutoSize = true;
            this.butConCam.Font = new System.Drawing.Font("宋体", 9F);
            this.butConCam.Location = new System.Drawing.Point(24, 100);
            this.butConCam.Name = "butConCam";
            this.butConCam.Size = new System.Drawing.Size(202, 45);
            this.butConCam.TabIndex = 1;
            this.butConCam.Text = "连接相机";
            this.butConCam.UseVisualStyleBackColor = true;
            this.butConCam.Click += new System.EventHandler(this.butConCam_Click);
            // 
            // butOpenFile
            // 
            this.butOpenFile.AutoSize = true;
            this.butOpenFile.Font = new System.Drawing.Font("宋体", 9F);
            this.butOpenFile.Location = new System.Drawing.Point(24, 48);
            this.butOpenFile.Name = "butOpenFile";
            this.butOpenFile.Size = new System.Drawing.Size(202, 45);
            this.butOpenFile.TabIndex = 0;
            this.butOpenFile.Text = "打开文件";
            this.butOpenFile.UseVisualStyleBackColor = true;
            this.butOpenFile.Click += new System.EventHandler(this.butOpenFile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_Capture);
            this.groupBox2.Controls.Add(this.btnConfigSet);
            this.groupBox2.Location = new System.Drawing.Point(15, 30);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(220, 337);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // btnConfigSet
            // 
            this.btnConfigSet.Location = new System.Drawing.Point(9, 228);
            this.btnConfigSet.Name = "btnConfigSet";
            this.btnConfigSet.Size = new System.Drawing.Size(202, 40);
            this.btnConfigSet.TabIndex = 8;
            this.btnConfigSet.Text = "参数设置";
            this.btnConfigSet.UseVisualStyleBackColor = true;
            this.btnConfigSet.Click += new System.EventHandler(this.btnConfigSet_Click);
            // 
            // hSmartWindowResult
            // 
            this.hSmartWindowResult.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.hSmartWindowResult.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.hSmartWindowResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hSmartWindowResult.HDoubleClickToFitContent = true;
            this.hSmartWindowResult.HDrawingObjectsModifier = HalconDotNet.HSmartWindowControl.DrawingObjectsModifier.None;
            this.hSmartWindowResult.HImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hSmartWindowResult.HKeepAspectRatio = true;
            this.hSmartWindowResult.HMoveContent = true;
            this.hSmartWindowResult.HZoomContent = HalconDotNet.HSmartWindowControl.ZoomContent.WheelForwardZoomsIn;
            this.hSmartWindowResult.Location = new System.Drawing.Point(915, 11);
            this.hSmartWindowResult.Margin = new System.Windows.Forms.Padding(0);
            this.hSmartWindowResult.Name = "hSmartWindowResult";
            this.hSmartWindowResult.Size = new System.Drawing.Size(614, 481);
            this.hSmartWindowResult.TabIndex = 15;
            this.hSmartWindowResult.WindowSize = new System.Drawing.Size(614, 481);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btn_Capture
            // 
            this.btn_Capture.AutoSize = true;
            this.btn_Capture.Font = new System.Drawing.Font("宋体", 9F);
            this.btn_Capture.Location = new System.Drawing.Point(12, 276);
            this.btn_Capture.Name = "btn_Capture";
            this.btn_Capture.Size = new System.Drawing.Size(202, 45);
            this.btn_Capture.TabIndex = 10;
            this.btn_Capture.Text = "单次触发";
            this.btn_Capture.UseVisualStyleBackColor = true;
            this.btn_Capture.Click += new System.EventHandler(this.btn_Capture_Click);
            // 
            // SurfaceDefectDetection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1546, 765);
            this.Controls.Add(this.hSmartWindowResult);
            this.Controls.Add(this.labDetectStatus);
            this.Controls.Add(this.labResult);
            this.Controls.Add(this.btnTrigDetection);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SurfaceDefectDetection";
            this.ShowIcon = false;
            this.Text = "缺口检测";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labDetectStatus;
        private System.Windows.Forms.Label labResult;
        private System.Windows.Forms.Button btnTrigDetection;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labTime;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.Label labExpouse;
        private System.Windows.Forms.Button butStopRev;
        private System.Windows.Forms.Button buttReadyRev;
        private System.Windows.Forms.Button butConCam;
        private System.Windows.Forms.Button butOpenFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private HalconDotNet.HSmartWindowControl hSmartWindowResult;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnConfigSet;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Capture;
    }
}

