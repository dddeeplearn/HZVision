// HikvisionCamera.cs (Modified for External Trigger Mode)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MvCamCtrl.NET;
using OpenCvSharp;

public class CameraDeviceInfo
{
    public string Name { get; set; } // 相机的可读名称 (用户ID或序列号)
    public MyCamera.MV_CC_DEVICE_INFO DeviceInfo { get; set; } // 相机的完整信息结构体
}
public class HikvisionCamera : IDisposable
{
    private MyCamera camera;
    private bool isGrabbing = false;

    // 定义一个委托实例，必须保持其生命周期，否则会被GC回收导致回调失败
    private MyCamera.cbOutputExdelegate imageCallbackDelegate;

    public event Action<Mat> ImageGrabbed;

    public HikvisionCamera()
    {
        camera = new MyCamera();
        // 初始化委托
        imageCallbackDelegate = new MyCamera.cbOutputExdelegate(ImageCallback);
    }

    // ... EnumDevices() 方法保持不变 ...
    public static List<CameraDeviceInfo> EnumDevices()
    {
        var deviceList = new List<CameraDeviceInfo>();
        MyCamera.MV_CC_DEVICE_INFO_LIST stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDeviceList);
        if (nRet != MyCamera.MV_OK) { return deviceList; }
        for (int i = 0; i < stDeviceList.nDeviceNum; i++)
        {
            MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
            string cameraName = "";
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {

                var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO_EX));
                string strUserDefinedName = System.Text.Encoding.UTF8.GetString(gigeInfo.chUserDefinedName).TrimEnd('\0');
                string strSerialNumber = gigeInfo.chSerialNumber.TrimEnd('\0');
                cameraName = !string.IsNullOrEmpty(strUserDefinedName) ? strUserDefinedName : strSerialNumber;
                //deviceList.Add($"GigE: {(string.IsNullOrEmpty(strUserDefinedName) ? strSerialNumber : strUserDefinedName)}");

            }
            else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));
                string strUserDefinedName = System.Text.Encoding.UTF8.GetString(usbInfo.chUserDefinedName).TrimEnd('\0');
                string strSerialNumber = usbInfo.chSerialNumber.TrimEnd('\0');
                //deviceList.Add($"USB: {(string.IsNullOrEmpty(strUserDefinedName) ? strSerialNumber : strUserDefinedName)}");
                cameraName = !string.IsNullOrEmpty(strUserDefinedName) ? strUserDefinedName : strSerialNumber;
            }
            if (!string.IsNullOrEmpty(cameraName))
            {
                deviceList.Add(new CameraDeviceInfo { Name = cameraName, DeviceInfo = device });
            }
        }
        return deviceList;
    }

    public bool Connect(string cameraName)
    {
        // 1. 枚举所有设备
        List<CameraDeviceInfo> allDevices = EnumDevices();
        if (allDevices.Count == 0)
        {
            Console.WriteLine("未找到任何相机设备。");
            return false;
        }

        // 2. 根据名称查找目标设备
        CameraDeviceInfo targetDevice = allDevices.FirstOrDefault(d => d.Name == cameraName);

        if (targetDevice == null)
        {
            Console.WriteLine($"未找到名称为 '{cameraName}' 的相机。");
            // 可选：打印出所有找到的相机名称，方便调试
            Console.WriteLine("找到的相机列表: " + string.Join(", ", allDevices.Select(d => d.Name)));
            return false;
        }

        // 3. 使用找到的设备信息结构体来创建句柄并连接
        var deviceToConnect = targetDevice.DeviceInfo;
        int nRet = camera.MV_CC_CreateDevice_NET(ref deviceToConnect);
        if (nRet != MyCamera.MV_OK) return false;

        nRet = camera.MV_CC_OpenDevice_NET();
        if (nRet != MyCamera.MV_OK) return false;

        return true;
    }
    /// <summary>
    /// 开始监听相机（注册回调并开始抓图）。
    /// 在触发模式下，这会使相机进入等待触发的状态。
    /// </summary>
    public void StartListening()
    {
        if (isGrabbing) return;

        // 注册回调函数
        int nRet = camera.MV_CC_RegisterImageCallBackEx_NET(imageCallbackDelegate, IntPtr.Zero);
        if (nRet != MyCamera.MV_OK)
        {
            throw new Exception("注册图像回调失败!");
        }

        // 开始抓图。在触发模式下，相机不会立即出图，而是等待触发信号。
        nRet = camera.MV_CC_StartGrabbing_NET();
        if (nRet != MyCamera.MV_OK)
        {
            // 如果开始失败，需要注销回调
            camera.MV_CC_RegisterImageCallBackEx_NET(null, IntPtr.Zero);
            throw new Exception("启动相机采集失败 (触发模式)!");
        }

        isGrabbing = true;
    }

    /// <summary>
    /// 当相机被触发并捕获到图像时，SDK会调用此方法。
    /// </summary>
    private void ImageCallback(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
    {
        if (pData != IntPtr.Zero && IsMonoData(pFrameInfo.enPixelType))
        {
            Mat grabbedMat = Mat.FromPixelData((int)pFrameInfo.nHeight, (int)pFrameInfo.nWidth, MatType.CV_8UC1, pData);
            // 触发事件，将克隆后的图像传递出去
            ImageGrabbed?.Invoke(grabbedMat.Clone());
        }
    }

    /// <summary>
    /// 停止监听相机（停止抓图并注销回调）。
    /// </summary>
    public void StopListening()
    {
        if (!isGrabbing) return;

        int nRet = camera.MV_CC_StopGrabbing_NET();
        if (nRet != MyCamera.MV_OK)
        {
            // 即使停止失败，也应尝试注销回调
        }

        // 注销回调
        camera.MV_CC_RegisterImageCallBackEx_NET(null, IntPtr.Zero);
        isGrabbing = false;
    }

    // ... (GetFrameRate, GetExposureTime, IsMonoData, Close, Dispose 等方法保持不变) ...
    public float GetFrameRate()
    {
        if (camera == null) return -1;
        MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
        int nRet = camera.MV_CC_GetFloatValue_NET("AcquisitionFrameRate", ref stParam);
        if (MyCamera.MV_OK == nRet) { return stParam.fCurValue; }
        return -1;
    }
    public float GetExposureTime()
    {
        if (camera == null) return -1;
        MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
        int nRet = camera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
        if (MyCamera.MV_OK == nRet) { return stParam.fCurValue; }
        return -1;
    }
    public float GetDelayTime()
    {
        if (camera == null) return -1;
        MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
        int nRet = camera.MV_CC_GetFloatValue_NET("TriggerDelay", ref stParam);
        if (MyCamera.MV_OK == nRet) { return stParam.fCurValue; }
        return -1;
    }
    private bool IsMonoData(MyCamera.MvGvspPixelType enPixelType)
    {
        switch (enPixelType)
        {
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                return true;
            default:
                return false;
        }
    }
    public bool SetTriggerDelay(float delayInMicroseconds)
    {
        if (camera == null) return false;

        // 使用通用接口 MV_CC_SetFloatValue_NET 来设置浮点型参数
        int nRet = camera.MV_CC_SetFloatValue_NET("TriggerDelay", delayInMicroseconds);

        return nRet == MyCamera.MV_OK;
    }
    public void Close()
    {
        if (isGrabbing) StopListening();
        if (camera != null)
        {
            camera.MV_CC_CloseDevice_NET();
            camera.MV_CC_DestroyDevice_NET();
        }
    }


    public bool SoftTrigger()
    {
        // 发送软触发命令 (对应海康 SDK 指令)
        int nRet = camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
        return nRet == MyCamera.MV_OK;
    }

    // 设置触发模式的方法
    public void SetTriggerMode(bool on)
    {
        camera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)(on ? 1 : 0));
    }

    public void SetTriggerSource(uint source)
    {
        // 0: Line0, 7: Software
        camera.MV_CC_SetEnumValue_NET("TriggerSource", source);
    }


    public void Dispose() { Close(); }
}