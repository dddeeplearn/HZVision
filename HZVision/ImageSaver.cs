// ImageSaver.cs

using System;
using System.IO;
using System.Linq;
using OpenCvSharp;

public class ImageSaver
{
    private readonly string saveDirectory;
    private readonly int maxFilesToKeep;
    private int currentFileCounter = 0;
    public ImageSaver(string directory = "ImageHistory", int maxFiles = 100)
    {
        this.saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);
        this.maxFilesToKeep = maxFiles;

        if (!Directory.Exists(this.saveDirectory))
        {
            Directory.CreateDirectory(this.saveDirectory);
        }
    }

    public string Save(Mat imageToSave)
    {
        if (imageToSave == null || imageToSave.Empty())
        {
            return null;
        }
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string fileName = $"Image_{timestamp}.jpg";
        string filePath = Path.Combine(saveDirectory, fileName);
        try
        {
            using (var imageCopy = imageToSave.Clone())
            {
                Cv2.ImWrite(filePath, imageCopy);
            }
            currentFileCounter++;
        }
        catch (Exception)
        {
            return null; 
        }
        if (maxFilesToKeep > 0 && currentFileCounter >= maxFilesToKeep)
        {
            CleanOldFiles();
            currentFileCounter = 0;
        }
        return filePath;
    }


    /// 清理保存目录下的所有文件。
    private void CleanOldFiles()
    {
        try
        {
            var directoryInfo = new DirectoryInfo(saveDirectory);
            // 获取所有.bmp文件
            var files = directoryInfo.GetFiles("*.bmp");
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    
                }
            }
        }
        catch (Exception)
        {
            
        }
    }
}