using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace SurfaceDefectDetection.Auth
{
    public static class MachineCodeHelper
    {
        public static string GetMachineCode()
        {
            string cpuId = GetCpuId();
            string diskId = GetDiskId();

            return $"{cpuId}-{diskId}";
        }

        private static string GetCpuId()
        {
            try
            {
                using (var mc = new ManagementClass("Win32_Processor"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        return mo["ProcessorId"]?.ToString();
                    }
                }
            }
            catch { }
            return "UNKNOWN_CPU";
        }

        private static string GetDiskId()
        {
            try
            {
                using (var mc = new ManagementClass("Win32_LogicalDisk"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        if (mo["DeviceID"]?.ToString() == "C:")
                        {
                            return mo["VolumeSerialNumber"]?.ToString();
                        }
                    }
                }
            }
            catch { }
            return "UNKNOWN_DISK";
        }
    }
}
