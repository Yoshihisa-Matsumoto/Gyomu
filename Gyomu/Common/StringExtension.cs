using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public static class StringExtension
    {
        public static string Right(this string me, int len)
        {
            if (len < 0)
                throw new ArgumentException("Length must be positive");
            if (me == null)
                return null;
            if (me.Length <= len)
                return me;
            return me.Substring(me.Length - len, len);
        }
        public static string Mid(this string me, int start, int len)
        {
            if (start < 0 || len < 0)
                throw new ArgumentException("Start & Length must be positive");
            if (me == null || me.Length < start)
                return me;
            if (me.Length < (start + len))
                return me.Substring(start);
            return me.Substring(start, len);
        }
        public static string Mid(this string me, int start)
        {
            return Mid(me, start, me.Length);
        }
        public static bool IsASCII(this string buf)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                int iChar = (int)buf[i];
                if (iChar > 0x7F)
                    return false;
            }
            return true;
        }
    }
}
