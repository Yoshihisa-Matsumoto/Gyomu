using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace Gyomu.Common
{
    public class UserEncryption
    {
        private static readonly Encoding enc = Encoding.UTF8;
        [SupportedOSPlatform("windows")]
        public static String Encrypt(String original)
        {
            byte[] bytes = System.Security.Cryptography.ProtectedData.Protect(enc.GetBytes(original), null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(bytes);

        }
        [SupportedOSPlatform("windows")]
        public static String Decrypt(String crypt)
        {
            byte[] bytes = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(crypt), null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return enc.GetString(bytes);

        }
    }
}
