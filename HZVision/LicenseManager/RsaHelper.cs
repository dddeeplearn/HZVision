using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SurfaceDefectDetection.Auth
{
    public static class RsaHelper
    {
        // 公钥
        private const string PublicKey = @"
        <RSAKeyValue><Modulus>
        yaWkDI1FahCH+uG6lIMQCUb27KPu6XvRAK/dRwx+QPVu6KPhIr8HKtXqlDgy8DGNXxnMRBxuMmhwrg
        KATasXvz6RjLYc+djRbevPg+chHEh5dUeU9wwRtoB7zd8PbVk5asDjs/87Ok1keCz+9AxM23lyZGh1
        rpS5tscNS3iJZoiCqqMbCCjPPH3V9MCwG1gQOtkiNOLne9lso1AI6Jp9XkD8y/dwrBYrVAZuYgm/eW
        mMDTNUn08eQjwRFaKPqkJQrzlEl5N+4ZJqffvzN2IZkf97ATIIkOYUqe8HgSmyudJsS0wka6n7HaHH
        cAc9p+MILYl68B5CUOeoPKErIQ7e5Q==
        </Modulus>
        <Exponent>AQAB</Exponent>
        </RSAKeyValue>";

        public static bool Verify(string data, string signature)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    rsa.FromXmlString(PublicKey);

                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signBytes = Convert.FromBase64String(signature);

                    return rsa.VerifyData(
                        dataBytes,
                        signBytes,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
