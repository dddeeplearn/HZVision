using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceDefectDetection.Auth
{
    public class LicenseModel
    {
        public string MachineCode { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Signature { get; set; }
    }
}
