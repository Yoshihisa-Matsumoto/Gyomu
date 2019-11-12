using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Access
{
    public class XMLAccess
    {
        public static T GetXMLValue<T>(string xmlData)
        {
            string val = xmlData;
            if (val == null)
                return default(T);
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                using (System.IO.StringReader reader = new System.IO.StringReader(val))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception )
            {
                return default(T);
            }
        }

        public static string GetXMLString<T>(T v)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                using (System.IO.StringWriter writer = new System.IO.StringWriter())
                {
                    serializer.Serialize(writer, v);
                    return writer.ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
