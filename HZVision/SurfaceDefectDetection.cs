using HalconDotNet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using WindowsFormsApp1;
using HslCommunication.ModBus;
using System.Runtime.InteropServices;
namespace HZVision
{
    public partial class SurfaceDefectDetection : Form
    {
        //Class1 C1 = new Class1();
        private ImageSaver OKimageSaver;
        private ImageSaver DPimageSaver;
        private ImageSaver HFimageSaver;
        private ImageSaver NGimageSaver;
        private ImageSaver DJimageSaver;
        private ImageSaver NG2imageSaver;
        private Mat currentImage;
        private HikvisionCamera hikCamera;
        private Logger detectionLogger;
        private string lastSavedImagePath = null;
        //private HWindowControl hWindowControlResult;
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private HTuple Sec1 = null;
        private HTuple Sec2 = null;
        private HTuple Sec = null;
        HTuple hv_DLModelHandle = new HTuple();
        HTuple hv_DLPreprocessParam = new HTuple();
        private ModbusTcpServer modbusServer;
        private System.Windows.Forms.Timer plcMonitorTimer; // 监控本地寄存器变化
        private short heartbeatValue = 0;
        private int heartbeatCounter = 0;
        private HObject BreakRoi = null;
        
        // 定义本地寄存器偏移地址
        private const string ADDR_HEARTBEAT = "10"; // 心跳信号 (PC定时写，PLC读)
        private const string ADDR_TRIGGER = "1";   // 触发信号 (PLC写1，PC检测并重置)
        private const string ADDR_RESULT = "11";    // 检测结果 (PC检测完写，PLC读)
        private const string ADDR_CamStatus = "12";    // 相机连接状态 (PC检测完写，PLC读,0正常1未连接)

        string iconFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HZico.ico");
        private System.Windows.Forms.Timer cameraRetryTimer; // 自动重连计时器
        private bool isUserDisconnected = false;
        private delegate void SafeUpdateUIDelegate(string message);
        private int saveImageCount = 50000;
        private bool isConnectedToCamera = false;
        private readonly string configFilePath;
        private readonly IniFile iniFile;
        private int regionthreshold=120, regionfilter1=9, regionfilter2=251,modbusPort=6000;
        private string ccdName="4";
        private bool AutoSave;
        private int sicisize=8;
        public SurfaceDefectDetection()
        {
            InitializeComponent();
            hikCamera = new HikvisionCamera();
            hikCamera.ImageGrabbed += OnCameraImageGrabbed;
            //this.Icon = new Icon(iconFilePath);
            detectionLogger = new Logger("DetectionLog.txt");
            configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (!File.Exists(configFilePath))
            {
                iniFile = new IniFile(configFilePath);
                CreateDefaultConfig();
            }
            else
            {
                iniFile = new IniFile(configFilePath);
            }
            try
            {
                int.TryParse(iniFile.Read("SaveImg", "NumSave", "40000"), out saveImageCount);
                //textImgNum.Text = iniFile.Read("SaveImg", "NumSave", "40000");
                bool.TryParse(iniFile.Read("SaveImg", "Auto", "true"), out AutoSave);
                //checkAutoSave.Checked = AutoSave;
                regionthreshold = int.Parse(iniFile.Read("Detection", "RegionThreshold", "120"));
                regionfilter1 = int.Parse(iniFile.Read("Detection", "RegionFilter1", "9"));
                regionfilter2 = int.Parse(iniFile.Read("Detection", "RegionFilter2", "251"));
                ccdName = iniFile.Read("Camera", "CCDName", "4");
                modbusPort= int.Parse(iniFile.Read("Communication", "Port", "6000"));
            }
            catch (Exception ex)
            {
                detectionLogger.Info("读取配置文件失败: " + ex.Message);
                SafeUpdateUI($"读取配置文件失败：{ex.Message}");
                CreateDefaultConfig();
                SafeUpdateUI($"配置文件已恢复默认值！");
            }
            detectionLogger.Info("================ 程序启动 ================");
            InitModbusServer();
            cameraRetryTimer = new System.Windows.Forms.Timer();
            cameraRetryTimer.Interval = 10000; // 10秒
            cameraRetryTimer.Tick += (s, e) => {
                if (!isUserDisconnected && !hikCamera.Connect(ccdName))
                {
                    Task.Run(() => TryConnectCamera()); // 在后台线程尝试，避免界面卡顿
                }
            };
            // 启动软件自动尝试连接
            isUserDisconnected = false;
            cameraRetryTimer.Start();
            Task.Run(() => TryConnectCamera());
            //this.Load += new EventHandler(temp_Load);
            //textImgNum.Text= saveImageCount.ToString();
            UpdateTime();
            buttReadyRev.Enabled = false;
            butStopRev.Enabled = false;
            //butSigCapture.Enabled = false;
            

        }

        private void InitModbusServer()
        {
            try
            {
                modbusServer = new ModbusTcpServer();
                modbusServer.Port = modbusPort;
                modbusServer.DataFormat = HslCommunication.Core.DataFormat.CDAB;
                // 启动监听
                modbusServer.ServerStart();
                SafeUpdateUI("ModBusTCP Server已启动...");
                //detectionLogger.Info("连接到PLC...");
                // 初始化轮询计时器
                plcMonitorTimer = new System.Windows.Forms.Timer();
                plcMonitorTimer.Interval = 45; // 45ms 检查一次
                plcMonitorTimer.Tick += PlcMonitorTimer_Tick;
                plcMonitorTimer.Start();
            }
            catch (Exception ex)
            {
                SafeUpdateUI("ModBusTCP Server启动失败...");
                MessageBox.Show("Modbus Server 启动失败，请检查端口6000是否被占用或尝试以管理员身份运行。\n" + ex.Message);
            }
        }
        private void PlcMonitorTimer_Tick(object sender, EventArgs e)
        {
            if (modbusServer == null) return;
            heartbeatCounter++;
            if (heartbeatCounter >= 22) //
            {
                heartbeatCounter = 0;
                heartbeatValue = (short)(heartbeatValue == 0 ? 1 : 0);
                modbusServer.Write(ADDR_HEARTBEAT, heartbeatValue);
            }

            short triggerVal = modbusServer.ReadInt16(ADDR_TRIGGER).Content;
            labStatus.Text = $"接收: {triggerVal}";
            if (triggerVal == 1)
            {
                SafeUpdateUI($"接收到控制信号: {triggerVal}");
                modbusServer.Write(ADDR_TRIGGER, (short)0);

                // 执行检测
                this.BeginInvoke(new Action(() => {
                    detectionLogger.Info("收到 PLC Modbus 触发信号");
                    bool success = hikCamera.SoftTrigger();
                    if (!success)
                    {
                        SafeUpdateUI("相机软触发失败！");
                        detectionLogger.Info("相机软触发失败！");
                        if (!hikCamera.Connect("4"))
                        {
                            isConnectedToCamera = false;
                            TryConnectCamera();          // 尝试连接
                            cameraRetryTimer.Start();    // 开启自动重连监控
                            modbusServer.Write(ADDR_CamStatus, (short)1);
                            SafeUpdateUI("相机断开连接，准备重连...");
                        }
                    }
                }));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("6寸");
            comboBox1.Items.Add("8寸");
            comboBox1.Items.Add("12寸");
            //comboBox1.Items.Add(" C");
            comboBox1.SelectedIndex = 0; 
        }
        private void butConCam_Click(object sender, EventArgs e)
        {
            if (isConnectedToCamera)
            {
                isConnectedToCamera = false;
                isUserDisconnected = true;   // 设为主动断开
                //butStopRev_Click(this, EventArgs.Empty);
                butStopRev.Enabled = false;
                //butSigCapture.Enabled = false;
                cameraRetryTimer.Stop();     // 停止重连
                hikCamera.StopListening();
                hikCamera.Close();
                modbusServer.Write(ADDR_CamStatus, (short)0);
                butConCam.Text = "连接相机";
                butConCam.Enabled = true;
                butConCam.BackColor = Color.FromKnownColor(KnownColor.Control);
                modbusServer.Write(ADDR_CamStatus, (short)1);
                buttReadyRev.Enabled = false;
                SafeUpdateUI("主动断开连接");
            }
            else
            {
                //MessageBox.Show("连接相机失败！");
                isUserDisconnected = false;  // 重置标志位
                SafeUpdateUI("相机重连...");
                TryConnectCamera();          // 尝试连接
                cameraRetryTimer.Start();    // 开启自动重连监控
            }
        }
        private void TryConnectCamera()
        {
            if (isUserDisconnected) return; // 如果是用户主动断开，则不尝试重连
            var devices = HikvisionCamera.EnumDevices();

            if (devices.Count == 0)
            {
                SafeUpdateUI("未找到任何海康相机设备，正在重试...");
                modbusServer.Write(ADDR_CamStatus, (short)1);
                return;
            }

            if (hikCamera.Connect(ccdName))
            {
                isConnectedToCamera = true;
                hikCamera.StartListening();
                this.Invoke(new Action(() => {
                    butConCam.Text = "断开相机";
                    butConCam.Enabled = true;
                    butConCam.BackColor = Color.FromKnownColor(KnownColor.Control);
                    //buttReadyRev.Enabled = true;
                    butStopRev.Enabled = true;
                    buttReadyRev.Enabled = false;
                    //butSigCapture.Enabled = true;
                }));
                //hikCamera.SetTriggerMode(true);
                //hikCamera.SetTriggerSource(1);
                SafeUpdateUI("相机准备就绪，等待外部触发...");
                UpdateCameraParameters();
                modbusServer.Write(ADDR_CamStatus, (short)0);
                cameraRetryTimer.Stop(); // 成功连接后停止重连
                SafeUpdateUI($"相机已连接: {devices[0]}，等待检测...");
            }
            else
            {
                isConnectedToCamera = false;
                this.Invoke(new Action(() => {
                    //lblStatus.Text = "连接失败，10秒后重试...";
                    butConCam.Text = "正在尝试连接...";
                    modbusServer.Write(ADDR_CamStatus, (short)1);
                }));
                SafeUpdateUI("连接失败，10s后重试...");
            }
        }

        private void UpdateCameraParameters()
        {
            if (hikCamera == null) return;
            float frameRate = hikCamera.GetFrameRate();
            //lblFrameRate.Text = frameRate > 0 ? $"帧率: {frameRate:F2} fps" : "帧率: N/A"; 
            float exposureTime = hikCamera.GetExposureTime();
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    labExpouse.Text = exposureTime > 0 ? $"曝光: {(exposureTime / 1000.0):F2} ms" : "曝光: N/A";
                }));
            }

            //float capdelaytime = hikCamera.GetExposureTime();
        }

        private void buttReadyRev_Click(object sender, EventArgs e)
        {
            hikCamera.StartListening();
            buttReadyRev.Enabled = false;
            butStopRev.Enabled = true;
            SafeUpdateUI("相机准备就绪，等待外部触发...");
        }

        private void butStopRev_Click(object sender, EventArgs e)
        {
            hikCamera.StopListening();
            buttReadyRev.Enabled = true;
            butStopRev.Enabled = false;
            SafeUpdateUI("相机已停止接收。");
        }
        private void OnCameraImageGrabbed(Mat grabbedImage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnCameraImageGrabbed(grabbedImage)));
                return;
            }
            currentImage?.Dispose();
            currentImage = grabbedImage.Clone();
            pictureBox4.Image = BitmapConverter.ToBitmap(currentImage);
            btnTrigDetection.Enabled = true;
            //lblStatus.Text = $"已接收到新图像！ 时间: {DateTime.Now:HH:mm:ss}";
            SafeUpdateUI($"已接收到新图像！ 时间: {DateTime.Now:HH:mm:ss}");
            btnTrigDetection_Click(this, EventArgs.Empty);
        }

        private void btnTrigDetection_Click(object sender, EventArgs e)
        {
            if (currentImage == null || currentImage.Empty())
            {
                MessageBox.Show("没有可供检测的图像！"); return;
            }
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo parentDirInfo = Directory.GetParent(currentDir);
            DirectoryInfo debugDirInfo = Directory.GetParent(parentDirInfo.FullName);
            ResetDetectionUI();
            HObject resultContour = null;
            //int fragmentResult;
            //double detectscore;
            //double thresholdv;
            //double thresholdv = 0.8;
            int sicesizesign = 0;
            //double areatheshold = 2.4E6;
            //double scothreshold;
            //double.TryParse(txtthreshold.Text, out thresholdv);
            HTuple hv_ModelID = null;
            try
            {
                HTuple angle, fragmentResult;
                ProcessImage(currentImage,sicisize,out fragmentResult,out angle);

                if (modbusServer != null)
                {
                    //modbusServer.Write(ADDR_RESULT, (short)fragmentResult);
                    modbusServer.Write(ADDR_RESULT, (short)0);
                    SafeUpdateUI($"已发送检测结果：{fragmentResult}");
                }
                string resultStatusText = "";
                //lblResultArea.Text = $"检测结果: {fragmentResult}";
                //      lblResultInfo.Text = $"得分: {detectscore}";
                if (fragmentResult == 0)
                {
                    labDetectStatus.Text = "无缺口"; labDetectStatus.ForeColor = Color.Red;
                    resultStatusText = "无缺口";
                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)fragmentResult));
                    if (AutoSave)
                    {
                        Task.Run(() => NGimageSaver.Save(currentImage));
                    }
                   Task.Delay(1000).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));

                }
                if (fragmentResult == 1)
                {
                    labDetectStatus.Text = "OK：" + angle.D.ToString("F2"); labDetectStatus.ForeColor = Color.Green;
                    resultStatusText = "OK：" + angle.D.ToString("F2");
                }

                SafeUpdateUI("检测结果：" + resultStatusText);
                //  显示结果 
                HObject ho_displayImage = null;
                //try
                //{
                //    HOperatorSet.GenImage1(out ho_displayImage, "byte", currentImage.Width, currentImage.Height, currentImage.Data);
                //    var window = hSmartWindowResult.HalconWindow; 
                //    window.SetPart(0, 0, currentImage.Height - 1, currentImage.Width - 1);
                //    window.ClearWindow();
                //    window.DispObj(ho_displayImage);

                //    int a = BreakRoi.CountObj();
                //    bool b = BreakRoi.IsInitialized();
                //    if (BreakRoi != null && BreakRoi.IsInitialized() && BreakRoi.CountObj() > 0 )
                //    {
                //        if (resultStatusText == "OK")
                //        {
                //            window.SetColor("green");
                //        }
                //        else
                //        {
                //            window.SetColor("red");
                //        }
                            
                //        window.SetLineWidth(3);
                //        window.DispObj(BreakRoi);
                //    }
                //}
                //finally
                //{
                //    ho_displayImage?.Dispose();
                //    BreakRoi?.Dispose();
                //}
                HOperatorSet.CountSeconds(out Sec2);
                Sec = Sec2 - Sec1;
                double time = Sec.D;
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string imageName = string.IsNullOrEmpty(lastSavedImagePath) ? "N/A" : Path.GetFileName(lastSavedImagePath);
                string logMessage = $"{timestamp}\t{imageName}\t{fragmentResult}\t{sicesizesign}\t{time}";
                detectionLogger.Info(logMessage);
                //if (ioc == 1)
                //{ 
                //    //Task.Delay(30).ContinueWith(_ => io.IO_WritePin(ioCardSerialNumber, 1, 1));
                // //   io.IO_WritePin(ioCardSerialNumber, 1, 1);
                //}
            }
            catch (Exception ex)
            {
                SafeUpdateUI("检测过程中发生错误: " + ex.Message);
                detectionLogger.Info("检测过程中发生错误: " + ex.Message);
            }
            finally
            {
                resultContour?.Dispose();
            }
        }
        private void ResetDetectionUI()
        {
            if (hSmartWindowResult != null && hSmartWindowResult.HalconWindow != null)
            {
                hSmartWindowResult.HalconWindow.ClearWindow();
            }
            labDetectStatus.Text = "等待检测";
            labDetectStatus.ForeColor = Color.Black;
            //lblResultArea.Text = "缺陷面积: N/A";
        }
        //private void butSaveNum_Click(object sender, EventArgs e)
        //{
        //    int.TryParse(textImgNum.Text, out int imgnum);
        //    if (imgnum > 10000)
        //    {
        //        saveImageCount = imgnum;
        //        MessageBox.Show($"已设置保存图片数量为 {saveImageCount} 张。");
        //        SafeUpdateUI($"设置保存图像数量{saveImageCount}");
        //    }
        //    else
        //    {
        //        saveImageCount = 10000;
        //        MessageBox.Show("保存图片数量最小为10000。");
        //        SafeUpdateUI($"设置保存图像数量{saveImageCount}");
        //    }

        //    iniFile.Write("SaveImg", "NumSave", saveImageCount.ToString());
        // //   Task.Delay(10).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)1));
        //  //  Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
        //}
        private void ProcessImage(Mat srcImage,int sicisize,out HTuple hv_NotchExist, out HTuple angle)
        {
            HOperatorSet.CountSeconds(out Sec1);
            HObject ho_Image;
            HTuple hv_RegionCenterRow = 2528, hv_RegionCenterCol = 2545, hv_RegionRadius = 2469;
            HTuple hv_EdgeLow = 5, hv_EdgeHight = 10, hv_MinEdgeSize = 1000, hv_Iteration = 5;
            //HTuple angle = null;
            HTuple hv_BeginRow = 200, hv_BeginCol = 0, hv_EndRow = 4700, hv_EndCol = 4700;
            HTuple hv_ExtendPix = 5, hv_MinNotchDis = 20;
            HTuple hv_SiliSize = sicisize;
            HOperatorSet.GenImage1(out ho_Image, "byte", srcImage.Width, srcImage.Height, srcImage.Data);
            hv_NotchExist = 0;
            angle = 0;
            switch (sicisize)
            {
                case 6:
                case 8:
                    // 处理尺寸8/6的情况
                    notch_es_detect(ho_Image, hv_SiliSize, hv_RegionCenterRow, hv_RegionCenterCol,
                            hv_RegionRadius, hv_EdgeLow, hv_EdgeHight, hv_MinEdgeSize, hv_MinNotchDis, hv_Iteration,
                            out hv_NotchExist,out angle);
                    break;
                case 12:
                    // 处理尺寸12的情况
                    notch_twl_detect(ho_Image, hv_BeginRow, hv_BeginCol, hv_EndRow, hv_EndCol,
                            hv_ExtendPix, hv_MinNotchDis, out hv_NotchExist, out angle);
                    break;
            }
            //return ;
        }
        public void notch_es_detect(HObject ho_Img, HTuple hv_SiliSize, HTuple hv_RegionCenterRow,
            HTuple hv_RegionCenterCol, HTuple hv_RegionRadius, HTuple hv_EdgeLow, HTuple hv_EdgeHight,
            HTuple hv_MinEdgeSize, HTuple hv_MinNotchDis,HTuple hv_Iteration, out HTuple hv_NotchExist, out HTuple hv_Angle)
        {
            // Local iconic variables 
            HObject ho_Circle1 = null, ho_Circle2 = null, ho_DifCircle;
            HObject ho_ImageRing, ho_Edges, ho_SelectedContours, ho_SingleContour = null;
            HObject ho_Contour, ho_CenterCross, ho_FittedCircle, ho_NotchCross;

            // Local control variables 

            HTuple hv_NumContours = new HTuple(), hv_AllEdgeRow = new HTuple();
            HTuple hv_AllEdgeCol = new HTuple(), hv_EdgeIndex = new HTuple();
            HTuple hv_EdgeRow = new HTuple(), hv_EdgeCol = new HTuple();
            HTuple hv_CenterRow = new HTuple(), hv_CenterCol = new HTuple();
            HTuple hv_Radius = new HTuple(), hv_StartPhi = new HTuple();
            HTuple hv_EndPhi = new HTuple(), hv_PointOrder = new HTuple();
            HTuple hv_CorCenterRow = new HTuple(), hv_CorCenterCol = new HTuple();
            HTuple hv_Distance = new HTuple(), hv_Min = new HTuple();
            HTuple hv_Indices = new HTuple(), hv_notch_row = new HTuple();
            HTuple hv_notch_col = new HTuple(), hv_NotchAngle = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Circle1);
            HOperatorSet.GenEmptyObj(out ho_Circle2);
            HOperatorSet.GenEmptyObj(out ho_DifCircle);
            HOperatorSet.GenEmptyObj(out ho_ImageRing);
            HOperatorSet.GenEmptyObj(out ho_Edges);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours);
            HOperatorSet.GenEmptyObj(out ho_SingleContour);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HOperatorSet.GenEmptyObj(out ho_CenterCross);
            HOperatorSet.GenEmptyObj(out ho_FittedCircle);
            HOperatorSet.GenEmptyObj(out ho_NotchCross);
            hv_Angle = new HTuple();
            //============================================
            //�ļ���: notch_es_detect.hdvp
            //����: ȱ�Ǽ�⺯����
            //���룺ͼ�񡢹�Ƭ�ߴ硢����λ�á�����뾶����Ե�͡���Ե�ߡ���С��Ե���ȡ���������
            //������Ƕ�
            //============================================
            if ((int)(new HTuple(hv_SiliSize.TupleEqual(8))) != 0)
            {
                ho_Circle1.Dispose();
                HOperatorSet.GenCircle(out ho_Circle1, hv_RegionCenterRow, hv_RegionCenterCol,
                    2469);
                ho_Circle2.Dispose();
                HOperatorSet.GenCircle(out ho_Circle2, hv_RegionCenterRow, hv_RegionCenterCol,
                    2290);
            }
            else if ((int)(new HTuple(hv_SiliSize.TupleEqual(6))) != 0)
            {
                ho_Circle1.Dispose();
                HOperatorSet.GenCircle(out ho_Circle1, hv_RegionCenterRow, hv_RegionCenterCol,
                    1972);
                ho_Circle2.Dispose();
                HOperatorSet.GenCircle(out ho_Circle2, hv_RegionCenterRow, hv_RegionCenterCol,
                    1661);
            }

            ho_DifCircle.Dispose();
            HOperatorSet.Difference(ho_Circle1, ho_Circle2, out ho_DifCircle);
            ho_ImageRing.Dispose();
            HOperatorSet.ReduceDomain(ho_Img, ho_DifCircle, out ho_ImageRing);
            ho_Edges.Dispose();
            HOperatorSet.EdgesSubPix(ho_ImageRing, out ho_Edges, "canny", 3, hv_EdgeLow,
                hv_EdgeHight);

            ho_SelectedContours.Dispose();
            HOperatorSet.SelectContoursXld(ho_Edges, out ho_SelectedContours, "contour_length",
                hv_MinEdgeSize, 99999, -0.5, 0.5);
            hv_NumContours.Dispose();
            HOperatorSet.CountObj(ho_SelectedContours, out hv_NumContours);
            hv_AllEdgeRow.Dispose();
            hv_AllEdgeRow = new HTuple();
            hv_AllEdgeCol.Dispose();
            hv_AllEdgeCol = new HTuple();
            HTuple end_val22 = hv_NumContours;
            HTuple step_val22 = 1;
            for (hv_EdgeIndex = 1; hv_EdgeIndex.Continue(end_val22, step_val22); hv_EdgeIndex = hv_EdgeIndex.TupleAdd(step_val22))
            {
                ho_SingleContour.Dispose();
                HOperatorSet.SelectObj(ho_SelectedContours, out ho_SingleContour, hv_EdgeIndex);
                hv_EdgeRow.Dispose();
                hv_EdgeRow = new HTuple();
                hv_EdgeCol.Dispose();
                hv_EdgeCol = new HTuple();
                hv_EdgeRow.Dispose(); hv_EdgeCol.Dispose();
                HOperatorSet.GetContourXld(ho_SingleContour, out hv_EdgeRow, out hv_EdgeCol);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                            ExpTmpLocalVar_AllEdgeRow = hv_AllEdgeRow.TupleConcat(
                            hv_EdgeRow);
                        hv_AllEdgeRow.Dispose();
                        hv_AllEdgeRow = ExpTmpLocalVar_AllEdgeRow;
                    }
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                            ExpTmpLocalVar_AllEdgeCol = hv_AllEdgeCol.TupleConcat(
                            hv_EdgeCol);
                        hv_AllEdgeCol.Dispose();
                        hv_AllEdgeCol = ExpTmpLocalVar_AllEdgeCol;
                    }
                }
            }
            ho_Contour.Dispose();
            HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_AllEdgeRow, hv_AllEdgeCol);
            hv_CenterRow.Dispose(); hv_CenterCol.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
            HOperatorSet.FitCircleContourXld(ho_Contour, "geotukey", -1, 0, 0, 5, 2, out hv_CenterRow,
                out hv_CenterCol, out hv_Radius, out hv_StartPhi, out hv_EndPhi, out hv_PointOrder);
            hv_CorCenterRow.Dispose();
            hv_CorCenterRow = new HTuple(hv_CenterRow);
            hv_CorCenterCol.Dispose();
            hv_CorCenterCol = new HTuple(hv_CenterCol);

            var window = hSmartWindowResult.HalconWindow;
            int imgHeight = currentImage.Height;
            int imgWidth = currentImage.Width;

            // 获取窗口尺寸
            int windowWidth = hSmartWindowResult.Width;
            int windowHeight = hSmartWindowResult.Height;

            // 计算保持比例的显示区域
            double imgAspect = (double)imgWidth / imgHeight;
            double windowAspect = (double)windowWidth / windowHeight;

            int displayHeight, displayWidth;
            int startRow = 0, startCol = 0;

            if (imgAspect > windowAspect)
            {
                // 图像更宽，以宽度为基准
                displayWidth = imgWidth;
                displayHeight = (int)(imgWidth / windowAspect);
                startRow = (imgHeight - displayHeight) / 2;
            }
            else
            {
                // 图像更高，以高度为基准
                displayHeight = imgHeight;
                displayWidth = (int)(imgHeight * windowAspect);
                startCol = (imgWidth - displayWidth) / 2;
            }

            // 设置显示区域
            window.SetPart(startRow, startCol, startRow + displayHeight - 1, startCol + displayWidth - 1);



            //window.SetPart(0, 0, currentImage.Height - 1, currentImage.Width - 1);
            window.ClearWindow();
            window.DispObj(ho_Img);
            {
                window.SetColor("green");
            }
            {
                window.DispObj(ho_SelectedContours);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                // m_hWindowHandle.Dispose();
                HOperatorSet.GenCrossContourXld(out ho_CenterCross, hv_CenterRow, hv_CenterCol,
                    200, (new HTuple(45)).TupleRad());
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_FittedCircle.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_FittedCircle, hv_CenterRow, hv_CenterCol,
                    hv_Radius, 0, (new HTuple(360)).TupleRad(), "positive", 1);
            }



            //distance_pp (EdgeRow, EdgeCol, CenterRow, CenterCol, Distance)
            hv_Distance.Dispose();
            HOperatorSet.DistancePp(hv_AllEdgeRow, hv_AllEdgeCol, hv_CenterRow, hv_CenterCol,
                out hv_Distance);
            HTuple hv_Difdist, hv_AbsDifdist, hv_Less, hv_Sum;
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Difdist = hv_Distance - hv_Radius;
            }

            HOperatorSet.TupleAbs(hv_Difdist, out hv_AbsDifdist);

            HOperatorSet.TupleLessElem(hv_Difdist, -10, out hv_Less);

            HOperatorSet.TupleSum(hv_Less, out hv_Sum);
            if ((int)(new HTuple(hv_Sum.TupleGreater(hv_MinNotchDis))) != 0)
            {
                hv_NotchExist = 1;
            }
            else
            {
                hv_NotchExist = 0;
                hv_Angle = 0;
            }
            if(hv_NotchExist==1)
            {

                hv_Min.Dispose();
                HOperatorSet.TupleMin(hv_Distance, out hv_Min);
                hv_Indices.Dispose();
                HOperatorSet.TupleFind(hv_Distance, hv_Min, out hv_Indices);
                hv_notch_row.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_notch_row = hv_AllEdgeRow.TupleSelect(
                        hv_Indices);
                }
                hv_notch_col.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_notch_col = hv_AllEdgeCol.TupleSelect(
                        hv_Indices);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_NotchCross.Dispose();
                    HOperatorSet.GenCrossContourXld(out ho_NotchCross, hv_notch_row, hv_notch_col,
                        200, (new HTuple(45)).TupleRad());
                }
                hv_NotchAngle.Dispose();
                HOperatorSet.AngleLx(hv_CenterRow, hv_CenterCol, hv_notch_row, hv_notch_col,
                    out hv_NotchAngle);
                hv_Angle.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Angle = hv_NotchAngle.TupleDeg();
                }
            }
            else
            {
                ho_NotchCross = null;
            }
            window.SetColor("red");
            if(ho_NotchCross == null)
            {
                window.DispObj(ho_CenterCross);
                window.DispObj(ho_FittedCircle);
            }
            else
            {
                window.DispObj(ho_CenterCross);
                window.DispObj(ho_NotchCross);
            }

            ho_Circle1.Dispose();
            ho_Circle2.Dispose();
            ho_DifCircle.Dispose();
            ho_ImageRing.Dispose();
            ho_Edges.Dispose();
            ho_SelectedContours.Dispose();
            ho_SingleContour.Dispose();
            ho_Contour.Dispose();
            ho_CenterCross.Dispose();
            ho_FittedCircle.Dispose();
            ho_NotchCross.Dispose();

            hv_NumContours.Dispose();
            hv_AllEdgeRow.Dispose();
            hv_AllEdgeCol.Dispose();
            hv_EdgeIndex.Dispose();
            hv_EdgeRow.Dispose();
            hv_EdgeCol.Dispose();
            hv_CenterRow.Dispose();
            hv_CenterCol.Dispose();
            hv_Radius.Dispose();
            hv_StartPhi.Dispose();
            hv_EndPhi.Dispose();
            hv_PointOrder.Dispose();
            hv_CorCenterRow.Dispose();
            hv_CorCenterCol.Dispose();
            hv_Distance.Dispose();
            hv_Min.Dispose();
            hv_Indices.Dispose();
            hv_notch_row.Dispose();
            hv_notch_col.Dispose();
            hv_NotchAngle.Dispose();

            return;
        }
        public void notch_twl_detect(HObject ho_Img, HTuple hv_BeginRow, HTuple hv_BeginCol,
            HTuple hv_EndRow, HTuple hv_EndCol, HTuple hv_ExtendPix, HTuple hv_MinNotchDis,
            out HTuple hv_NotchExist, out HTuple hv_BigDeg)
        {
            // Local iconic variables 

            HObject ho_imgTwl = null, ho_Rectangle, ho_LimitImg;
            HObject ho_BoundRegion, ho_midContour, ho_RegionDilation;
            HObject ho_RegionErosion, ho_RegionDifference, ho_EdgeLimitImg;
            HObject ho_Edges, ho_BigCross, ho_BigFittedCircle, ho_BigNotchCross = null;

            // Local control variables 

            HTuple hv_RowEdge = new HTuple(), hv_ColEdge = new HTuple();
            HTuple hv_BigRow = new HTuple(), hv_BigCol = new HTuple();
            HTuple hv_BigRadius = new HTuple(), hv_StartPhi1 = new HTuple();
            HTuple hv_EndPhi1 = new HTuple(), hv_PointOrder1 = new HTuple();
            HTuple hv_BigNotchDistance = new HTuple(), hv_Difdist = new HTuple();
            HTuple hv_AbsDifdist = new HTuple(), hv_Less = new HTuple();
            HTuple hv_Sum = new HTuple(), hv_BigMin = new HTuple();
            HTuple hv_BigIndices = new HTuple(), hv_BigNotchRow = new HTuple();
            HTuple hv_BigNotchCol = new HTuple(), hv_BigAngle = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_imgTwl);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_LimitImg);
            HOperatorSet.GenEmptyObj(out ho_BoundRegion);
            HOperatorSet.GenEmptyObj(out ho_midContour);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_EdgeLimitImg);
            HOperatorSet.GenEmptyObj(out ho_Edges);
            HOperatorSet.GenEmptyObj(out ho_BigCross);
            HOperatorSet.GenEmptyObj(out ho_BigFittedCircle);
            HOperatorSet.GenEmptyObj(out ho_BigNotchCross);
            hv_NotchExist = new HTuple();
            hv_BigDeg = new HTuple();
            ho_imgTwl.Dispose();
            ho_imgTwl = new HObject(ho_Img);
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle, hv_BeginRow, hv_BeginCol, hv_EndRow,
                hv_EndCol);
            ho_LimitImg.Dispose();
            HOperatorSet.ReduceDomain(ho_imgTwl, ho_Rectangle, out ho_LimitImg);
            ho_BoundRegion.Dispose();
            HOperatorSet.Threshold(ho_LimitImg, out ho_BoundRegion, 0, 120);
            ho_midContour.Dispose();
            HOperatorSet.GenContourRegionXld(ho_BoundRegion, out ho_midContour, "border");
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_BoundRegion, out ho_RegionDilation, hv_ExtendPix);
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_BoundRegion, out ho_RegionErosion, hv_ExtendPix);
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_RegionDilation, ho_RegionErosion, out ho_RegionDifference
                );
            ho_EdgeLimitImg.Dispose();
            HOperatorSet.ReduceDomain(ho_imgTwl, ho_RegionDifference, out ho_EdgeLimitImg
                );
            ho_Edges.Dispose();
            HOperatorSet.EdgesSubPix(ho_EdgeLimitImg, out ho_Edges, "canny", 1, 20, 40);
            hv_RowEdge.Dispose(); hv_ColEdge.Dispose();
            HOperatorSet.GetContourXld(ho_Edges, out hv_RowEdge, out hv_ColEdge);
            hv_BigRow.Dispose(); hv_BigCol.Dispose(); hv_BigRadius.Dispose(); hv_StartPhi1.Dispose(); hv_EndPhi1.Dispose(); hv_PointOrder1.Dispose();
            HOperatorSet.FitCircleContourXld(ho_Edges, "geotukey", -1, 0, 0, 10, 2, out hv_BigRow,
                out hv_BigCol, out hv_BigRadius, out hv_StartPhi1, out hv_EndPhi1, out hv_PointOrder1);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_BigCross.Dispose();
                HOperatorSet.GenCrossContourXld(out ho_BigCross, hv_BigRow, hv_BigCol, 200, (new HTuple(45)).TupleRad()
                    );
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_BigFittedCircle.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_BigFittedCircle, hv_BigRow, hv_BigCol,
                    hv_BigRadius, 0, (new HTuple(360)).TupleRad(), "positive", 1);
            }
            hv_BigNotchDistance.Dispose();
            HOperatorSet.DistancePp(hv_RowEdge, hv_ColEdge, hv_BigRow, hv_BigCol, out hv_BigNotchDistance);
            //**ignore***
            // if (HDevWindowStack.IsOpen())
            // {
            //     HOperatorSet.DispObj(ho_imgTwl, HDevWindowStack.GetActive());
            // }
            var window = hSmartWindowResult.HalconWindow;
            int imgHeight = currentImage.Height;
            int imgWidth = currentImage.Width;

            // 获取窗口尺寸
            int windowWidth = hSmartWindowResult.Width;
            int windowHeight = hSmartWindowResult.Height;

            // 计算保持比例的显示区域
            double imgAspect = (double)imgWidth / imgHeight;
            double windowAspect = (double)windowWidth / windowHeight;

            int displayHeight, displayWidth;
            int startRow = 0, startCol = 0;

            if (imgAspect > windowAspect)
            {
                // 图像更宽，以宽度为基准
                displayWidth = imgWidth;
                displayHeight = (int)(imgWidth / windowAspect);
                startRow = (imgHeight - displayHeight) / 2;
            }
            else
            {
                // 图像更高，以高度为基准
                displayHeight = imgHeight;
                displayWidth = (int)(imgHeight * windowAspect);
                startCol = (imgWidth - displayWidth) / 2;
            }

            // 设置显示区域
            window.SetPart(startRow, startCol, startRow + displayHeight - 1, startCol + displayWidth - 1);
            window.ClearWindow();
            window.DispObj(ho_Img);
            window.SetColor("green");
            window.DispObj(ho_Edges);


                

            //BigCenterRow[Index] := BigRow
            //BigCenterCol[Index] := BigCol
            //gen_cross_contour_xld (Cross, BigCenterRow[Index], BigCenterCol[Index], 6, rad(45))
            //**ignore***
            //****Notch�����ж�*****
            //gen_region_contour_xld (Edges, EdgeRegion, 'margin')
            hv_Difdist.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_Difdist = hv_BigNotchDistance - hv_BigRadius;
            }
            hv_AbsDifdist.Dispose();
            HOperatorSet.TupleAbs(hv_Difdist, out hv_AbsDifdist);
            hv_Less.Dispose();
            HOperatorSet.TupleLessElem(hv_Difdist, -20, out hv_Less);
            hv_Sum.Dispose();
            HOperatorSet.TupleSum(hv_Less, out hv_Sum);
            if ((int)(new HTuple(hv_Sum.TupleGreater(hv_MinNotchDis))) != 0)
            {
                hv_NotchExist.Dispose();
                hv_NotchExist = 1;
            }
            else
            {
                hv_NotchExist.Dispose();
                hv_NotchExist = 0;
            }
            //****Notch�����ж�*****
            if ((int)(new HTuple(hv_NotchExist.TupleEqual(1))) != 0)
            {
                hv_BigMin.Dispose();
                HOperatorSet.TupleMin(hv_BigNotchDistance, out hv_BigMin);
                hv_BigIndices.Dispose();
                HOperatorSet.TupleFind(hv_BigNotchDistance, hv_BigMin, out hv_BigIndices);
                hv_BigNotchRow.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BigNotchRow = hv_RowEdge.TupleSelect(
                        hv_BigIndices);
                }
                hv_BigNotchCol.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BigNotchCol = hv_ColEdge.TupleSelect(
                        hv_BigIndices);
                }
                hv_BigAngle.Dispose();
                HOperatorSet.AngleLx(hv_BigRow, hv_BigCol, hv_BigNotchRow, hv_BigNotchCol,
                    out hv_BigAngle);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_BigNotchCross.Dispose();
                    HOperatorSet.GenCrossContourXld(out ho_BigNotchCross, hv_BigNotchRow, hv_BigNotchCol,
                        200, (new HTuple(45)).TupleRad());

                    window.SetColor("red");
                    window.DispObj(ho_BigNotchCross);
                    //window.DispObj(ho_BigCross);

                }
                hv_BigDeg.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BigDeg = hv_BigAngle.TupleDeg();
                }
                //disp_message (WindowHandle, '��Notch', 'window', 10, 10, 'red', 'true')
            }
            else
            {
                //disp_message (WindowHandle, '��Notch', 'window', 10, 10, 'red', 'true')
                hv_BigDeg.Dispose();
                hv_BigDeg = 0;
                //window.SetColor("green");
                //window.DispObj(ho_Edges);
            }

            ho_imgTwl.Dispose();
            ho_Rectangle.Dispose();
            ho_LimitImg.Dispose();
            ho_BoundRegion.Dispose();
            ho_midContour.Dispose();
            ho_RegionDilation.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionDifference.Dispose();
            ho_EdgeLimitImg.Dispose();
            ho_Edges.Dispose();
            ho_BigCross.Dispose();
            ho_BigFittedCircle.Dispose();
            ho_BigNotchCross.Dispose();

            hv_RowEdge.Dispose();
            hv_ColEdge.Dispose();
            hv_BigRow.Dispose();
            hv_BigCol.Dispose();
            hv_BigRadius.Dispose();
            hv_StartPhi1.Dispose();
            hv_EndPhi1.Dispose();
            hv_PointOrder1.Dispose();
            hv_BigNotchDistance.Dispose();
            hv_Difdist.Dispose();
            hv_AbsDifdist.Dispose();
            hv_Less.Dispose();
            hv_Sum.Dispose();
            hv_BigMin.Dispose();
            hv_BigIndices.Dispose();
            hv_BigNotchRow.Dispose();
            hv_BigNotchCol.Dispose();
            hv_BigAngle.Dispose();

            return;
        }



        private void SafeUpdateUI(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeUpdateUIDelegate(SafeUpdateUI), message);
                return;
            }

            // 更新日志文本框
            textBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            string[] lines = textBox1.Lines;
            if (lines.Length > 200)
            {
                int removeCount = lines.Length - 200;
                //textBox1.Lines = lines.Skip(removeCount).ToArray();
                textBox1.Clear();
            }
            textBox1.ScrollToCaret();
        }

        private void butOpenFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "图像文件|*.bmp;*.jpg;*.png" }) 
            { 
                if (ofd.ShowDialog() == DialogResult.OK) 
                { 
                    ResetDetectionUI(); 
                    currentImage?.Dispose();
                    currentImage = new Mat(ofd.FileName, ImreadModes.Grayscale);
                    pictureBox4.Image = BitmapConverter.ToBitmap(currentImage);
                    SafeUpdateUI($"文件已加载: {System.IO.Path.GetFileName(ofd.FileName)}");
                    btnTrigDetection.Enabled = true; 
                } 
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }
        private void UpdateTime() 
        {
            labTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void butSigCapture_Click(object sender, EventArgs e)
        {
            // Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
            hikCamera.SetTriggerSource(7);

            bool success = hikCamera.SoftTrigger();
             if (!success)
             {
                 SafeUpdateUI("相机软触发失败！");
                 detectionLogger.Info("相机软触发失败！");
             }
             else
             {
                 SafeUpdateUI("相机软触发成功！");
             }
            hikCamera.SetTriggerSource(0);

        }

        private void btn_Capture_Click(object sender, EventArgs e)
        {
            hikCamera.SetTriggerSource(7);

            bool success = hikCamera.SoftTrigger();
            if (!success)
            {
                SafeUpdateUI("相机软触发失败！");
                detectionLogger.Info("相机软触发失败！");
            }
            else
            {
                SafeUpdateUI("相机软触发成功！");
            }
            hikCamera.SetTriggerSource(0);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "8寸":
                    sicisize = 8;
                    break;
                case "6寸":
                    sicisize = 6;
                    break;
                case "12寸":
                    sicisize = 12;
                    break;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //butStopRev_Click(this, EventArgs.Empty);
            cameraRetryTimer.Stop();     // 停止重连
            hikCamera.StopListening();
            hikCamera.Close();
        }
        private void CreateDefaultConfig()
        {
            // Save设置
            iniFile.Write("SaveImg", "Auto", "ture");
            iniFile.Write("SaveImg", "NumSave", "50000");
            iniFile.Write("Detection", "RegionThreshold", "120");
            iniFile.Write("Detection", "RegionFilter1", "9");
            iniFile.Write("Detection", "RegionFilter2", "251");
            iniFile.Write("Camera", "CCDName", "4");
            iniFile.Write("Communication", "Port", "6000");
            SafeUpdateUI("已创建默认配置文件");
        }

        private void btnConfigSet_Click(object sender, EventArgs e)
        {
            using (var settingForm = new Setting(ccdName, modbusPort,regionthreshold, regionfilter1, regionfilter2, saveImageCount,AutoSave))
            {
                if (settingForm.ShowDialog() == DialogResult.OK)
                {
                    regionthreshold = settingForm.threshold;
                    regionfilter1 = settingForm.filtersize1;
                    regionfilter2 = settingForm.filtersize2;
                    saveImageCount = settingForm.saveNum;
                    ccdName = settingForm.ccdName;
                    modbusPort= settingForm.port;
                    AutoSave = settingForm.IsSaved;
                    iniFile.Write("Detection", "RegionThreshold", regionthreshold.ToString());
                    iniFile.Write("Detection", "RegionFilter1", regionfilter1.ToString());
                    iniFile.Write("Detection", "RegionFilter2", regionfilter2.ToString());
                    iniFile.Write("Camera", "CCDName", ccdName);
                    iniFile.Write("Communication", "Port", modbusPort.ToString());
                    iniFile.Write("SaveImg", "Auto", AutoSave ? "true" : "false");
                    SafeUpdateUI("已保存设置");
                    MessageBox.Show("参数已更新并保存！");
                }
            }
        }
    }
}
