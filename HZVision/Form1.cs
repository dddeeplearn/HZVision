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
using WindowsFormsApp1;
using HslCommunication.ModBus;
namespace HZVision
{
    public partial class Form1 : Form
    {
        Class1 C1 = new Class1();
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
        public Form1()
        {
            InitializeComponent();
            hikCamera = new HikvisionCamera();
            hikCamera.ImageGrabbed += OnCameraImageGrabbed;
            this.Icon = new Icon(iconFilePath);
            detectionLogger = new Logger("DetectionLog.txt");
            detectionLogger.Info("================ 程序启动 ================");
            InitModbusServer();
            cameraRetryTimer = new System.Windows.Forms.Timer();
            cameraRetryTimer.Interval = 10000; // 10秒
            cameraRetryTimer.Tick += (s, e) => {
                if (!isUserDisconnected && !hikCamera.Connect("4"))
                {
                    Task.Run(() => TryConnectCamera()); // 在后台线程尝试，避免界面卡顿
                }
            };
            // 启动软件自动尝试连接
            isUserDisconnected = false;
            cameraRetryTimer.Start();
            Task.Run(() => TryConnectCamera());
            this.Load += new EventHandler(temp_Load);
        }

        private void InitModbusServer()
        {
            try
            {
                modbusServer = new ModbusTcpServer();
                
                modbusServer.Port = 6000;
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
                    }
                }));
            }
        }

        private void temp_Load(object sender, EventArgs e)
        {
            // 预加载模型
            try
            {
                //     HTuple Gpus = new HTuple();

                //     HOperatorSet.ReadDlClassifier(baseDirectory + "model_Q.hdl", out hv_DLModelHandle);
                HOperatorSet.ReadDlModel(baseDirectory + "model_Q.hdl", out hv_DLModelHandle);
                HOperatorSet.ReadDict(baseDirectory + "model_Q_dl_preprocess_params.hdict", new HTuple(), new HTuple(), out hv_DLPreprocessParam);
                //    HOperatorSet.GetSystem("cuda_devices",out Gpus);

                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "batch_size", 1);
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "runtime", "gpu");
                //      HOperatorSet.SetDlClassifierParam(hv_DLModelHandle, "runtime", "cpu");
                //      HOperatorSet.SetDlClassifierParam(hv_DLModelHandle, "batch_size", 1);
                //    HOperatorSet.ReadShapeModel(baseDirectory + "//Matching 01.shm", out modelID_182);
                //     HOperatorSet.ReadShapeModel(baseDirectory+"//Matching210.shm", out modelID_210);
            }
            catch (Exception ex)
            {
                SafeUpdateUI($"加载模型失败：{ex.Message}");
                MessageBox.Show($"加载模型失败: {ex.Message}");
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void butConCam_Click(object sender, EventArgs e)
        {
            if (isConnectedToCamera)
            {
                isConnectedToCamera = false;
                isUserDisconnected = true;   // 设为主动断开
                butStopRev_Click(this, EventArgs.Empty);
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
                TryConnectCamera();          // 尝试连接
                cameraRetryTimer.Start();    // 开启自动重连监控
                SafeUpdateUI("相机重连...");
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
            if (hikCamera.Connect("4"))
            {
                isConnectedToCamera = true;
                this.Invoke(new Action(() => {
                    butConCam.Text = "断开相机";
                    butConCam.Enabled = true;
                    butConCam.BackColor = Color.FromKnownColor(KnownColor.Control);
                    buttReadyRev.Enabled = true;
                }));
                hikCamera.SetTriggerMode(true);
                hikCamera.SetTriggerSource(1);
                buttReadyRev_Click(this, EventArgs.Empty);
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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    buttReadyRev.Enabled = false;
                    butStopRev.Enabled = true;
                    SafeUpdateUI("相机准备就绪，等待外部触发...");
                }));
            }
        }

        private void butStopRev_Click(object sender, EventArgs e)
        {
            hikCamera.StopListening();
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    buttReadyRev.Enabled = true;
                    butStopRev.Enabled = false;
                    SafeUpdateUI("相机已停止接收。");
                }));
            }
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
            string imgSavepath = debugDirInfo + "\\ImageSave\\";
            string savePath1 = imgSavepath + "/OKImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string savePath2 = imgSavepath + "/NGImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string savePath3 = imgSavepath + "/HFImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string savePath4 = imgSavepath + "/DPImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string savePath5 = imgSavepath + "/DJImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string savePath6 = imgSavepath + "/NG2ImageSave/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            if (!System.IO.Directory.Exists(@savePath1))
            {
                System.IO.Directory.CreateDirectory(@savePath1);//不存在就创建文件夹
            }
            if (!System.IO.Directory.Exists(@savePath2))
            {
                System.IO.Directory.CreateDirectory(@savePath2);//不存在就创建文件夹
            }
            if (!System.IO.Directory.Exists(@savePath3))
            {
                System.IO.Directory.CreateDirectory(@savePath3);//不存在就创建文件夹
            }
            if (!System.IO.Directory.Exists(@savePath4))
            {
                System.IO.Directory.CreateDirectory(@savePath4);//不存在就创建文件夹
            }
            if (!System.IO.Directory.Exists(@savePath5))
            {
                System.IO.Directory.CreateDirectory(@savePath5);//不存在就创建文件夹
            }
            if (!System.IO.Directory.Exists(@savePath6))
            {
                System.IO.Directory.CreateDirectory(@savePath6);//不存在就创建文件夹
            }
            OKimageSaver = new ImageSaver(savePath1, saveImageCount);
            NGimageSaver = new ImageSaver(savePath2, saveImageCount);
            HFimageSaver = new ImageSaver(savePath3, saveImageCount);
            DPimageSaver = new ImageSaver(savePath4, saveImageCount);
            DJimageSaver = new ImageSaver(savePath5, saveImageCount);
            NG2imageSaver = new ImageSaver(savePath6, saveImageCount);
            ResetDetectionUI();

            HObject resultContour = null;
            int fragmentResult;
            double detectscore;
            //double thresholdv;
            //double thresholdv = 0.8;
            int sicesizesign = 0;
            double areatheshold = 2.4E6;
            double scothreshold;
            //double.TryParse(txtthreshold.Text, out thresholdv);
            HTuple hv_ModelID = null;
            try
            {
                fragmentResult = ProcessImage(currentImage);

                if (modbusServer != null)
                {
                    modbusServer.Write(ADDR_RESULT, (short)fragmentResult);
                }
                string resultStatusText = "";
                //lblResultArea.Text = $"检测结果: {fragmentResult}";
                //      lblResultInfo.Text = $"得分: {detectscore}";
                if (fragmentResult == 1)
                {
                    labDetectStatus.Text = "NG"; labDetectStatus.ForeColor = Color.Red;
                    resultStatusText = "NG";
                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => NGimageSaver.Save(currentImage));
                    }
                }
                if (fragmentResult == 2)
                {
                    labDetectStatus.Text = "DP"; labDetectStatus.ForeColor = Color.Blue;
                    resultStatusText = "DP";
                    //if (ioc == 1)
                    //{
                    //    OperateResult operate = inovanceTcp.Write(writeAddress, (short)1);
                    //    Task.Delay(40).ContinueWith(_ => inovanceTcp.Write(writeAddress, (short)0));

                    //}

                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => DPimageSaver.Save(currentImage));
                    }
                }
                if (fragmentResult == 0)
                {
                    labDetectStatus.Text = "OK"; labDetectStatus.ForeColor = Color.Green;
                    resultStatusText = "OK";
                    //    if (ioc == 1) { io.IO_WritePin(ioCardSerialNumber, 1, 1); }
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => OKimageSaver.Save(currentImage));
                    }
                }
                if (fragmentResult == 3)
                {
                    labDetectStatus.Text = "Half"; labDetectStatus.ForeColor = Color.Green;
                    resultStatusText = "Half";
                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => HFimageSaver.Save(currentImage));
                    }
                }
                if (fragmentResult == 4)
                {
                    labDetectStatus.Text = "DJ"; labDetectStatus.ForeColor = Color.Green;
                    resultStatusText = "DJ";
                    //    if (ioc == 1) { io.IO_WritePin(ioCardSerialNumber, 1, 1); }
                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => DJimageSaver.Save(currentImage));
                    }
                }
                if (fragmentResult == 5)
                {
                    labDetectStatus.Text = "NG2"; labDetectStatus.ForeColor = Color.Red;
                    resultStatusText = "NG2";
                    Task.Delay(100).ContinueWith(_ => modbusServer.Write(ADDR_RESULT, (short)0));
                    if (checkAutoSave.Checked)
                    {
                        Task.Run(() => NG2imageSaver.Save(currentImage));
                    }
                }
                SafeUpdateUI("检测结果：" + resultStatusText);
                //  显示结果 
                HObject ho_displayImage = null;
                try
                {
                    HOperatorSet.GenImage1(out ho_displayImage, "byte", currentImage.Width, currentImage.Height, currentImage.Data);
                    var window = hSmartWindowResult.HalconWindow; 
                    window.SetPart(0, 0, currentImage.Height - 1, currentImage.Width - 1);
                    window.ClearWindow();
                    window.DispObj(ho_displayImage);

                    if (resultContour != null && resultContour.IsInitialized() && resultContour.CountObj() > 0)
                    {
                        window.SetColor("green");
                        window.SetLineWidth(3);
                        window.DispObj(resultContour);
                    }
                }
                finally
                {
                    ho_displayImage?.Dispose();
                }
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
        private void butSaveNum_Click(object sender, EventArgs e)
        {
            int.TryParse(textImgNum.Text, out int imgnum);
            if (imgnum > 10000)
            {
                saveImageCount = imgnum;
                MessageBox.Show($"已设置保存图片数量为 {saveImageCount} 张。");
                SafeUpdateUI($"设置保存图像数量{saveImageCount}");
            }
            else
            {
                saveImageCount = 10000;
                MessageBox.Show("保存图片数量最小为10000。");
                SafeUpdateUI($"设置保存图像数量{saveImageCount}");
            }
        }
        private int ProcessImage(Mat srcImage)

        {
            // 声明
            HObject ho_Image = null, ho_Image20250618181212823 = null, ho_diff = null, ho_ImageMedian = null, Rectangle = null;
            HObject ho_ImagePro = null, ho_ModelRegion = null, ho_TemplateImage = null, ho_ModelContours = null;
            HObject ho_TransContours = null, ho_MatchContour = null, ho_MatchedRegion = null, reg = null;
            HObject ho_CroppedImage = null, ho_BrightRegions = null, ho_ConnectedRegions = null;
            HObject ho_SelectedRegions = null, ho_Rectangle = null, ho_Cross1 = null, ROI_0 = null;
            HObject ho_Cross2 = null, ho_Cross3 = null, ho_Cross4 = null, ImageReduced = null, Region1 = null;
            //HTuple hv_ModelID = null, hv_ModelRegionArea = null, hv_RefRow = null, hv_RefColumn = null;
            HTuple hv_ModelRegionArea = null, hv_RefRow = null, hv_RefColumn = null;
            HTuple hv_HomMat2D = null, hv_MatchResultID = null, hv_NumMatchResult = null, hv_Row = null;
            HTuple hv_Area1 = null, hv_Column1 = null, hv_Row1 = null;
            HTuple hv_Column = null, hv_Angle = null, hv_ScaleRow = null, hv_ScaleColumn = null;
            HTuple hv_Score = null, hv_Phi = null, hv_Length1 = null, hv_Length2 = null, Area1 = null, Row1 = null, Column1 = null;
            HTuple hv_Cos = null, hv_Sin = null, hv_LT_X = null, hv_LT_Y = null, hv_RT_X = null;
            HTuple hv_RT_Y = null, hv_RB_X = null, hv_RB_Y = null, hv_LD_X = null, hv_LD_Y = null;
            HTuple hv_DistanceMin1 = null, hv_DistanceMax1 = null, hv_DistanceMin2 = null, hv_DistanceMax2 = null;
            HTuple hv_DistanceMin3 = null, hv_DistanceMax3 = null, hv_DistanceMin4 = null, hv_DistanceMax4 = null;
            HTuple hv_Distances = null, hv_BooleanResult = null, hv_MaxDistanceMin = null;
            HObject ho__TmpRegion = null;
            HObject ho_UnionContours = null, ho_ClosedRegion = null, ho_OpenedRegion = null, ho_FinalContours = null, EMP = null, GAU = null;
            HObject ho_Region = null;
            HTuple hv_Index = new HTuple();
            HObject ho_RegionClosing = null, ho_RegionOpening = null;
            HTuple hv_Area = new HTuple();
            int hv_fragment = 0;
            double finalScore = 0.0;
            int sicesize = 210;
            double areaValue = 0;
            HOperatorSet.GenEmptyObj(out ROI_0);
            HOperatorSet.GenEmptyObj(out ImageReduced);
            HOperatorSet.GenEmptyObj(out Region1);
            HOperatorSet.GenEmptyObj(out ho__TmpRegion);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Image20250618181212823);
            HOperatorSet.GenEmptyObj(out ho_ImageMedian);
            HOperatorSet.GenEmptyObj(out ho_ImagePro);
            HOperatorSet.GenEmptyObj(out ho_ModelRegion);
            HOperatorSet.GenEmptyObj(out ho_TemplateImage);
            HOperatorSet.GenEmptyObj(out ho_ModelContours);
            HOperatorSet.GenEmptyObj(out ho_TransContours);
            HOperatorSet.GenEmptyObj(out ho_MatchContour);
            HOperatorSet.GenEmptyObj(out ho_MatchedRegion);
            HOperatorSet.GenEmptyObj(out ho_CroppedImage);
            HOperatorSet.GenEmptyObj(out ho_BrightRegions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Cross1);
            HOperatorSet.GenEmptyObj(out ho_Cross2);
            HOperatorSet.GenEmptyObj(out ho_Cross3);
            HOperatorSet.GenEmptyObj(out ho_Cross4);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ClosedRegion);
            HOperatorSet.GenEmptyObj(out ho_OpenedRegion);
            HOperatorSet.GenEmptyObj(out ho_FinalContours);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_diff);
            HOperatorSet.GenEmptyObj(out Rectangle);
            HOperatorSet.GenEmptyObj(out reg);
            HOperatorSet.GenEmptyObj(out GAU);
            HOperatorSet.GenEmptyObj(out EMP);
            HTuple hv_ClassNames = new HTuple(), hv_ClassIDs = new HTuple();
            HTuple hv_DLSampleBatch = new HTuple(), hv_DLResultBatch = new HTuple(), hv_num = new HTuple();

            HOperatorSet.CountSeconds(out Sec1);
            HOperatorSet.GenImage1(out ho_Image, "byte", srcImage.Width, srcImage.Height, srcImage.Data);

            //int.TryParse(txtClosingKernel.Text, out sicesize);

            try
            {
                C1.gen_dl_samples_from_images(ho_Image, out hv_DLSampleBatch);
                C1.preprocess_dl_samples(hv_DLSampleBatch, hv_DLPreprocessParam);
                //      HOperatorSet.ApplyDlClassifier(ho_Image, hv_DLModelHandle, out hv_DLResultBatch);
                HOperatorSet.ApplyDlModel(hv_DLModelHandle, hv_DLSampleBatch, new HTuple(),
                                out hv_DLResultBatch);
                //             HOperatorSet.GetDictTuple(hv_DLResultBatch, "classification_class_ids", out hv_ClassIDs);
                HOperatorSet.GetDictTuple(hv_DLResultBatch, "classification_class_names", out hv_ClassIDs);


                HOperatorSet.TupleLength(hv_ClassIDs, out hv_num);
                if (hv_num > 0)
                {
                    hv_ClassNames = hv_ClassIDs.TupleSelect(0);
                    if (hv_ClassNames == "OK")
                    {

                        hv_fragment = 0;
                    }
                    if (hv_ClassNames == "NG")
                    {
                        hv_fragment = 1;


                    }
                    if (hv_ClassNames == "DP")
                    {
                        hv_fragment = 2;

                    }
                    if (hv_ClassNames == "Half")
                    {

                        hv_fragment = 3;
                    }
                    if (hv_ClassNames == "DJ")
                    {

                        hv_fragment = 4;
                    }
                    if (hv_ClassNames == "NG2")
                    {

                        hv_fragment = 5;
                    }
                }
                return (hv_fragment);

            }

            finally
            {
                ho_Image?.Dispose();
                ho_Image20250618181212823?.Dispose();
                ho_ImageMedian?.Dispose();
                ho_ImagePro?.Dispose();
                ho_ModelRegion?.Dispose();
                ho_TemplateImage?.Dispose();
                ho_ModelContours?.Dispose();
                ho_TransContours?.Dispose();
                ho_MatchedRegion?.Dispose();
                ho_CroppedImage?.Dispose();
                ho_BrightRegions?.Dispose();
                ho_ConnectedRegions?.Dispose();
                ho_SelectedRegions?.Dispose();
                ho_Rectangle?.Dispose();
                ho_Cross1?.Dispose();
                ho_Cross2?.Dispose();
                ho_Cross3?.Dispose();
                ho_Cross4?.Dispose();

                //hv_ModelID?.Dispose();
                hv_ModelRegionArea?.Dispose();
                hv_RefRow?.Dispose();
                hv_RefColumn?.Dispose();
                hv_HomMat2D?.Dispose();
                hv_MatchResultID?.Dispose();
                hv_NumMatchResult?.Dispose();
                hv_Row?.Dispose();
                hv_Column?.Dispose();
                hv_Angle?.Dispose();
                hv_ScaleRow?.Dispose();
                hv_ScaleColumn?.Dispose();
                hv_Score?.Dispose();
                hv_Phi?.Dispose();
                hv_Length1?.Dispose();
                hv_Length2?.Dispose();
                hv_Cos?.Dispose();
                hv_Sin?.Dispose();
                hv_LT_X?.Dispose();
                hv_LT_Y?.Dispose();
                hv_RT_X?.Dispose();
                hv_RT_Y?.Dispose();
                hv_RB_X?.Dispose();
                hv_RB_Y?.Dispose();
                hv_LD_X?.Dispose();
                hv_LD_Y?.Dispose();
                hv_DistanceMin1?.Dispose();
                hv_DistanceMax1?.Dispose();
                hv_DistanceMin2?.Dispose();
                hv_DistanceMax2?.Dispose();
                hv_DistanceMin3?.Dispose();
                hv_DistanceMax3?.Dispose();
                hv_DistanceMin4?.Dispose();
                hv_DistanceMax4?.Dispose();
                hv_Distances?.Dispose();
                hv_BooleanResult?.Dispose();
                hv_MaxDistanceMin?.Dispose();
            }

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
    }
}
