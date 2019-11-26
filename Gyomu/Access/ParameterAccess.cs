using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Transactions;
using System.Data;
using System.Linq;

namespace Gyomu.Access
{
    public class ParameterAccess
    {
        private static string AES_KEY = null;
        private static string ADMIN_AES_KEY = null;
        private static string getAESKey()
        {
            if (AES_KEY != null)
                return AES_KEY;
            else
            {
                string val = GetBase64EncodedStringValue("gyomu_aes_key");
                if (string.IsNullOrEmpty(val))
                    throw new Exception("AES Key Not Setup. Please ask Developer");
                AES_KEY = val;
                return AES_KEY;
            }
        }
        private static string getAdminAESKey(Common.User u)
        {
            if (ADMIN_AES_KEY != null)
                return ADMIN_AES_KEY;
            else
            {
                string val = GetUserEncryptedStringValue("gyomu_admin_aes_key", u);
                if (string.IsNullOrEmpty(val))
                    throw new Exception("Admin AES Key Not Setup for you. Please setup");
                ADMIN_AES_KEY = val;
                return ADMIN_AES_KEY;
            }
        }
        internal const string USER_ROOTKEY = "rootKey$";
        internal static bool KeyExist(string key, List<Models.ParameterMaster> lstParam)
        {
            return loadParameter(key, lstParam) != null;
        }


        private static string loadParameter(string key,List<Models.ParameterMaster> lstParam)
        {
            return Common.GyomuDataAccess.LoadParameter(key)?.item_value;
        }

        internal static string GetKey(string key, Common.User u)
        {
            if (u != null && key.Equals(USER_ROOTKEY))
                key = System.Environment.MachineName + "_" + u.UserID + "_" + key;
            else if (u != null)
                key = u.UserID + "_" + key;
            return key;
        }
        private static readonly object lockObject = new object();
        private static T getValue<T>(string key, Common.User u = null, DateTime? targetDate = null)
        {
            List<Models.ParameterMaster> lstParam = new List<Models.ParameterMaster>();
            lock (lockObject)
            {
                key = GetKey(key, u);

                string value = null;
                int iErr = 0;
                while (iErr < 3)
                {
                    try
                    {
                        value = loadParameter(key, lstParam);

                        break;
                    }
                    catch (Exception ex)
                    {
                        iErr++;
                        if (iErr >= 3)
                            throw ex;
                    }
                }
                if (targetDate.HasValue)
                {
                    string strDate = targetDate.Value.ToString("yyyyMMdd");
                    value = lstParam.Where(p => p.item_value.Equals("")).FirstOrDefault()?.item_value;
                    foreach (Models.ParameterMaster r in lstParam.OrderBy(r => r.item_fromdate))
                    {
                        if (string.IsNullOrEmpty(r.item_fromdate))
                            continue;
                        else if (string.IsNullOrEmpty(r.item_fromdate.Trim()))
                            value = r.item_value;

                        else if (strDate.CompareTo(r.item_fromdate) == 0)
                        {
                            value = r.item_value;
                            break;
                        }
                        else if (strDate.CompareTo(r.item_fromdate) > 0)
                            value = r.item_value;
                        else
                            break;
                    }

                }
                if (value == null)
                    return default(T);
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
            }

        }
        private static void setValue<T>(string key, T i, Common.User u = null)
        {
            List<Models.ParameterMaster> lstParam = new List<Models.ParameterMaster>();
            key = GetKey(key, u);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {

                if (KeyExist(key, lstParam))
                {
                    if (i == null)
                        Common.GyomuDataAccess.DeleteParameter(key);
                    else
                    {
                        string strVal = i.ToString();
                        Common.GyomuDataAccess.UpdateParameter(key, strVal);
                    }
                }
                else
                {
                    if (i != null)
                    {
                        string strVal = i.ToString();
                        Common.GyomuDataAccess.InsertParameter(key, strVal);
                    }
                }

                scope.Complete();
            }
        }

        public static bool GetBoolValue(string key, Common.User u = null, DateTime? targetDate = null) { return getValue<bool>(key, u, targetDate); }
        public static int GetIntValue(string key, Common.User u = null, DateTime? targetDate = null) { return getValue<int>(key, u, targetDate); }
        public static decimal GetDecimalValue(string key, Common.User u = null, DateTime? targetDate = null) { return getValue<decimal>(key, u, targetDate); }
        public static string GetStringValue(string key, Common.User u = null, DateTime? targetDate = null) { return getValue<string>(key, u, targetDate); }
        public static List<string> GetStringListValue(string key, Common.User u = null, DateTime? targetDate = null)
        {
            string strVal = getValue<string>(key, u, targetDate);
            return strVal == null ? null : JsonAccess.Deserialize<List<string>>(strVal);
        }
        public static List<T> GetListValue<T>(string key, Common.User u = null, DateTime? targetDate = null)
        {
            string strVal = getValue<string>(key, u, targetDate);
            return strVal == null ? null : JsonAccess.Deserialize<List<T>>(strVal);
        }
        public static Dictionary<string, string> GetStringDictionaryValue(string key, Common.User u = null, DateTime? targetDate = null)
        {
            string strVal = getValue<string>(key, u, targetDate);
            return strVal == null ? null : JsonAccess.Deserialize<Dictionary<string, string>>(strVal);

        }
        public static Dictionary<string, T> GetDictionaryValue<T>(string key, Common.User u = null, DateTime? targetDate = null)
        {
            string strVal = getValue<string>(key, u, targetDate);
            return strVal == null ? null : JsonAccess.Deserialize<Dictionary<string, T>>(strVal);
        }
        public static string GetUserEncryptedStringValue(string key, Common.User u, DateTime? targetDate = null)
        {

            string encrypted = GetStringValue(key, u, targetDate);
            if (string.IsNullOrEmpty(encrypted))
                return null;
            string root_key = GetStringValue(GetKey(USER_ROOTKEY, u));
            string user_key = Common.UserEncryption.Decrypt(root_key);
            return Common.AESEncryption.AESDecrypt(encrypted, user_key);
        }
        public static string GetBase64EncodedStringValue(string key)
        {
            string encoded = GetStringValue(key);
            if (string.IsNullOrEmpty(encoded))
                return null;
            return Common.Base64Encode.Decode(encoded);
        }
        public static string GetAESEncryptStringValue(string key)
        {
            string aes_key = getAESKey();
            string encrypted = GetStringValue(key);
            if (string.IsNullOrEmpty(encrypted))
                return null;
            return Common.AESEncryption.AESDecrypt(encrypted, aes_key);
        }
        public static string GetAdminAESEncryptStringValue(string key, Common.User u)
        {
            string aes_key = getAdminAESKey(u);
            string encrypted = GetStringValue(key);
            if (string.IsNullOrEmpty(encrypted))
                return null;
            return Common.AESEncryption.AESDecrypt(encrypted, aes_key);
        }
        public static T GetXMLValue<T>(string key, Common.User u = null, DateTime? targetDate = null)
        {
            string val = getValue<string>(key, u, targetDate);
            if (val == null)
                return default(T);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (System.IO.StringReader reader = new System.IO.StringReader(val))
            {
                try
                {
                    return (T)serializer.Deserialize(reader);
                }
                catch (Exception) { return default(T); }
            }
        }

        public static void SetBoolValue(string key, bool v, Common.User u = null) { setValue<bool>(key, v, u); }
        public static void SetIntValue(string key, int v, Common.User u = null) { setValue<int>(key, v, u); }
        public static void SetDecimalValue(string key, decimal v, Common.User u = null) { setValue<decimal>(key, v, u); }
        public static void SetStringValue(string key, string v, Common.User u = null) { setValue<string>(key, v, u); }
        public static void SetStringListValue(string key, List<string> v, Common.User u = null)
        {
            string strVal = Newtonsoft.Json.JsonConvert.SerializeObject(v);
            setValue<string>(key, strVal, u);
        }
        public static void SetStringValueWithUserEncryption(string key, string v, Common.User u)
        {
            string root_key = GetStringValue(GetKey(USER_ROOTKEY, u));
            string user_key = Common.UserEncryption.Decrypt(root_key);
            string crypted = Common.AESEncryption.AESEncrypt(v, user_key);
            setValue<string>(key, crypted, u);
        }
        public static void SetRootKeyWithUserEncryption(string key, string v, Common.User u)
        {
            string crypted = Common.UserEncryption.Encrypt(v);
            setValue<string>(key, crypted, u);
        }
        public static void SetStringValueWithBase64Encode(string key, string v)
        {
            string encoded = Common.Base64Encode.Encode(v);
            setValue<string>(key, encoded);
        }
        public static void SetStringValueWithAESEncryption(string key, string v)
        {
            string aes_key = getAESKey();
            string crypted = Common.AESEncryption.AESEncrypt(v, aes_key);
            setValue<string>(key, crypted);
        }
        public static void SetStringValueWithAdminAESEncryption(string key, string v, Common.User u)
        {
            string aes_key = getAdminAESKey(u);
            string crypted = Common.AESEncryption.AESEncrypt(v, aes_key);
            setValue<string>(key, crypted);
        }
        public static void SetXMLValue<T>(string key, T v)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (System.IO.StringWriter writer = new System.IO.StringWriter())
            {
                serializer.Serialize(writer, v);
                setValue<string>(key, writer.ToString());
            }
        }

        public static T LockAndProcess<T>(string key, Func<T> method)
        {
            return Common.GyomuDataAccess.LockParameterProcess<T>(key, method);
        }
    }
}
