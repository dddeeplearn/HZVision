// Logger.cs

using System;
using System.IO;
using System.Text;

public class Logger : IDisposable
{
    private readonly StreamWriter logWriter;
    private readonly object lockObject = new object(); // 

    /// <param name="logFileName">日志文件名，例如 "DetectionLog.txt"。</param>
    public Logger(string logFileName = "DetectionLog.txt")
    {
        // 将日志文件放在程序运行目录下
        string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
        //string logFilePath = Path.Combine(@"E:\ImageSave", logFileName);

        try
        {
            logWriter = new StreamWriter(logFilePath, append: true, Encoding.UTF8)
            {
                AutoFlush = true
            };
        }
        catch (Exception ex)
        {
            // 输出错误
            Console.WriteLine($"无法创建或打开日志文件: {logFilePath}. 错误: {ex.Message}");
        }
    }

    /// <param name="message">要记录的信息。</param>
    public void Info(string message)
    {
        WriteEntry("INFO", message);
    }

    /// <param name="message">要记录的警告信息。</param>
    public void Warning(string message)
    {
        WriteEntry("WARN", message);
    }

    /// <param name="message">要记录的错误信息。</param>
    public void Error(string message)
    {
        WriteEntry("ERROR", message);
    }

    /// <param name="level">日志级别 (INFO, WARN, ERROR)。</param>
    /// <param name="message">日志内容。</param>
    private void WriteEntry(string level, string message)
    {
        if (logWriter == null) return;

        lock (lockObject)
        {
            if (level == "INFO")
            {
                logWriter.WriteLine(message);
            }
            else 
            {
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                logWriter.WriteLine(logLine);
            }
        }
    }

    public void Dispose()
    {
        logWriter?.Close();
        logWriter?.Dispose();
    }
}