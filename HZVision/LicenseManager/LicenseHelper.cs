using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace SurfaceDefectDetection.Auth
{
    public static class LicenseHelper
    {
        private static string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string LicensePath =
            currentDir+"license.json";

        public static bool CheckLicense()
        {
            try
            {
                if (!File.Exists(LicensePath))
                    return false;

                string json = File.ReadAllText(LicensePath);
                var license = JsonConvert.DeserializeObject<LicenseModel>(json);

                if (license == null)
                    return false;

                // 1. 校验机器码
                string localMachineCode = MachineCodeHelper.GetMachineCode();
                if (!string.Equals(localMachineCode, license.MachineCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                // 2. 校验有效期
                if (DateTime.Now > license.ExpireTime)
                    return false;

                // 3. 校验签名
                string signData = $"{license.MachineCode}|{license.ExpireTime:yyyy-MM-dd}";
                if (!RsaHelper.Verify(signData, license.Signature))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
