using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public class Base64Encode
    {
        public static String Encode(string strPlain, Encoding enc = null)
        {
            if (enc == null)
                enc = Encoding.UTF8;
            return Convert.ToBase64String(enc.GetBytes(strPlain));
        }
        public static String Decode(string strEncode, Encoding enc = null)
        {
            if (enc == null)
                enc = Encoding.UTF8;
            return enc.GetString(Convert.FromBase64String(strEncode));
        }
    }
}
