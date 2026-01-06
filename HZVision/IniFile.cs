using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HZVision
{
    public class IniFile
    {
        private readonly string filePath;
        private readonly Encoding encoding;
        private readonly object fileLock = new object();

        // 存储INI数据
        private Dictionary<string, Dictionary<string, string>> iniData =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 初始化INI文件操作类
        /// </summary>
        public IniFile(string path, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("INI文件路径不能为空", nameof(path));

            filePath = path;
            this.encoding = encoding ?? Encoding.UTF8;

            EnsureDirectoryExists();
            Load();
        }

        #region 文件加载和保存

        /// <summary>
        /// 加载INI文件
        /// </summary>
        public void Load()
        {
            lock (fileLock)
            {
                iniData.Clear();

                if (!File.Exists(filePath))
                {
                    // 文件不存在，创建空数据结构
                    EnsureFileExists();
                    return;
                }

                string currentSection = string.Empty;

                foreach (string line in File.ReadLines(filePath, encoding))
                {
                    string trimmedLine = line.Trim();

                    // 跳过空行和注释
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                        continue;

                    // 处理节
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                        if (!iniData.ContainsKey(currentSection))
                        {
                            iniData[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }
                        continue;
                    }

                    // 处理键值对
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0 && !string.IsNullOrEmpty(currentSection))
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();

                        // 处理值中的注释
                        int commentIndex = value.IndexOf(';');
                        if (commentIndex >= 0)
                        {
                            value = value.Substring(0, commentIndex).Trim();
                        }

                        iniData[currentSection][key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// 保存INI文件
        /// </summary>
        public void Save()
        {
            lock (fileLock)
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
                {
                    bool isFirstSection = true;

                    foreach (var section in iniData)
                    {
                        // 节之间添加空行（第一个节前不添加）
                        if (!isFirstSection)
                        {
                            writer.WriteLine();
                        }
                        isFirstSection = false;

                        // 写入节名
                        writer.WriteLine($"[{section.Key}]");

                        // 写入键值对
                        foreach (var kvp in section.Value)
                        {
                            writer.WriteLine($"{kvp.Key}={kvp.Value}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保存到新文件
        /// </summary>
        public void SaveAs(string newPath)
        {
            lock (fileLock)
            {
                string originalPath = filePath;

                // 暂时改变路径
                var tempIniFile = new IniFile(newPath, encoding);
                tempIniFile.iniData = iniData;
                tempIniFile.Save();
            }
        }

        #endregion

        #region 基本读写操作

        /// <summary>
        /// 读取字符串值
        /// </summary>
        public string Read(string section, string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                return defaultValue;

            lock (fileLock)
            {
                if (iniData.TryGetValue(section, out var sectionData) &&
                    sectionData.TryGetValue(key, out string value))
                {
                    return value;
                }

                return defaultValue;
            }
        }

        /// <summary>
        /// 写入字符串值
        /// </summary>
        public void Write(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
                throw new ArgumentException("节名不能为空", nameof(section));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("键名不能为空", nameof(key));

            lock (fileLock)
            {
                if (!iniData.ContainsKey(section))
                {
                    iniData[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                iniData[section][key] = value ?? string.Empty;
            }
            SaveToFile();
        }


        #endregion

        #region 高级操作

        /// <summary>
        /// 获取所有节名
        /// </summary>
        public List<string> GetSectionNames()
        {
            lock (fileLock)
            {
                return new List<string>(iniData.Keys);
            }
        }

        /// <summary>
        /// 获取节中的所有键值对
        /// </summary>
        public Dictionary<string, string> GetSection(string section)
        {
            lock (fileLock)
            {
                if (iniData.TryGetValue(section, out var sectionData))
                {
                    return new Dictionary<string, string>(sectionData);
                }

                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 获取节中的所有键名
        /// </summary>
        public List<string> GetKeys(string section)
        {
            lock (fileLock)
            {
                if (iniData.TryGetValue(section, out var sectionData))
                {
                    return new List<string>(sectionData.Keys);
                }

                return new List<string>();
            }
        }

        /// <summary>
        /// 删除键
        /// </summary>
        public bool DeleteKey(string section, string key)
        {
            lock (fileLock)
            {
                if (iniData.TryGetValue(section, out var sectionData))
                {
                    return sectionData.Remove(key);
                }

                return false;
            }
        }

        /// <summary>
        /// 删除整个节
        /// </summary>
        public bool DeleteSection(string section)
        {
            lock (fileLock)
            {
                return iniData.Remove(section);
            }
        }

        /// <summary>
        /// 检查节是否存在
        /// </summary>
        public bool SectionExists(string section)
        {
            lock (fileLock)
            {
                return iniData.ContainsKey(section);
            }
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool KeyExists(string section, string key)
        {
            lock (fileLock)
            {
                return iniData.TryGetValue(section, out var sectionData) &&
                       sectionData.ContainsKey(key);
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            lock (fileLock)
            {
                iniData.Clear();
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 确保文件目录存在
        /// </summary>
        private void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 确保文件存在
        /// </summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty, encoding);
            }
        }
        private void SaveToFile()
        {
            lock (fileLock)
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    bool isFirstSection = true;

                    foreach (var section in iniData)
                    {
                        // 节之间添加空行
                        if (!isFirstSection)
                        {
                            writer.WriteLine();
                        }
                        isFirstSection = false;

                        // 写入节名
                        writer.WriteLine($"[{section.Key}]");

                        // 写入键值对
                        foreach (var kvp in section.Value)
                        {
                            writer.WriteLine($"{kvp.Key}={kvp.Value}");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
