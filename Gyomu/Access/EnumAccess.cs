using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gyomu.Access
{
    public class EnumAccess
    {
        public static List<string> GetEnumValues(Type typeOfEnum)
        {
            List<string> lstData = new List<string>();
            foreach (object val in Enum.GetValues(typeOfEnum))
                lstData.Add(GetEnumValueDescription(val));
            return lstData;
        }
        public static T Parse<T>(string strVal) where T : struct
        {
            Dictionary<string, string> mapKey = getEnumMap(typeof(T));
            if (mapKey.ContainsKey(strVal))
                strVal = mapKey[strVal];
            return (T)Enum.Parse(typeof(T), strVal, false);
        }
        ///<summary>
        /// Return Value from "Description" attribute if exist.
        /// Otherwise return Default "ToString"
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static string GetEnumValueDescription(object value)
        {
            string result = string.Empty;
            if (value != null)
            {
                result = value.ToString();
                System.Type type = value.GetType();
                try
                {
                    result = Enum.GetName(type, value);
                    System.Reflection.FieldInfo fieldInfo = type.GetField(result);
                    object[] attributeArray = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    DescriptionAttribute attribute = null;
                    if (attributeArray.Length > 0)
                    {
                        attribute = (DescriptionAttribute)attributeArray[0];
                    }
                    if (attribute != null)
                        result = attribute.Description;
                }
                catch (ArgumentNullException)
                {
                    result = string.Empty;
                }
                catch (ArgumentException)
                {
                    result = value.ToString();
                }
            }
            return result;
        }
        private static Dictionary<string, string> getEnumMap(Type typeOfEnum)
        {
            Dictionary<string, string> dictMap = new Dictionary<string, string>();
            foreach (object val in Enum.GetValues(typeOfEnum))
            {
                System.Type type = val.GetType();
                string result;
                try
                {
                    result = Enum.GetName(type, val); string key = result;
                    System.Reflection.FieldInfo fieldInfo = type.GetField(result);
                    object[] attributeArray = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    DescriptionAttribute attribute = null;
                    if (attributeArray.Length > 0)
                    {
                        attribute = (DescriptionAttribute)attributeArray[0];
                    }
                    if (attribute != null)
                        result = attribute.Description;
                    if (key != result)
                    {
                        dictMap.Add(result, key);
                    }
                }
                catch (Exception) { }
            }
            return dictMap;
        }
    }
}
