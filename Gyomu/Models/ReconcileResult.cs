using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Gyomu.Models
{
    public class ReconcileResult<T, EType>
        where T : BaseItemRecord
        where EType : struct
    {
        string Item1ID { get; set; }
        string Item2ID { get; set; }
        public ReconcileResult(string item1, string item2)
        {
            Item1ID = item1;
            Item2ID = item2;
            if (typeof(EType).IsEnum == false)
                throw new Exception("Invalid Generic type of EType. must be enum");
        }
        public Dictionary<string, ReconcileItem<T>> dictKeyResult = new Dictionary<string, ReconcileItem<T>>();
        public void AddCountMismatch(string key, EType field, int cnt1, int cnt2)
        {
            ReconcileItem<T> item = null;
            lock (dictKeyResult)
            {
                if (dictKeyResult.ContainsKey(key) == false)
                    dictKeyResult.Add(key, new ReconcileItem<T>(default(T), default(T), key));
            }
            item = dictKeyResult[key];
            item.Add(Access.EnumAccess.GetEnumValueDescription(field), cnt1, cnt2);
        }
        public void Add(string key, EType field, T r1, T r2, T[] duplicatedRows)
        {
            ReconcileItem<T> item = null;
            string strField = Access.EnumAccess.GetEnumValueDescription(field);
            lock (dictKeyResult)
            {
                if (dictKeyResult.ContainsKey(key) == false)
                    dictKeyResult.Add(key, new ReconcileItem<T>(r1, r2, key));
            }
            item = dictKeyResult[key];
            if (r1 != null && r2 != null /*&& field != null*/)
            {
                lock (item)
                {
                    item.Add(strField, r1[Convert.ToInt32(field)], r2[Convert.ToInt32(field)]);
                }
            }
            else if (duplicatedRows != null)
            {
                if (duplicatedRows.Contains(r1))
                {
                    lock (item)
                    {
                        item.Add(strField, null, r2.Key);
                    }

                }
                else if (duplicatedRows.Contains(r2))
                    lock (item)
                        item.Add(strField, r1.Key, null);
            }
        }
        public bool Matched { get { return dictKeyResult.Count == 0; } }

        public void Export(string filename)
        {
            using (System.IO.StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("KEY,Item,\"" + Item1ID + "\",\"" + Item2ID + "\"");
                foreach (string key in dictKeyResult.Keys)
                {
                    ReconcileItem<T> i = dictKeyResult[key];
                    writer.WriteLine(i.CSVLine);
                    writer.Flush();
                }
            }
        }
    }
    public class ReconcileItem<T>
        where T : BaseItemRecord
    {
        public string KEY { get; set; }
        public T R1 { get; set; }
        public T R2 { get; set; }
        Dictionary<string, List<object>> dictColumnValues = new Dictionary<string, List<object>>();

        public Dictionary<string, List<object>> COLUMN_VALUES { get { return dictColumnValues; } }

        internal ReconcileItem(T r1, T r2, string key)
        {
            R1 = r1; R2 = r2; KEY = key;
        }
        internal void Add(string field, object v1, object v2)
        {
            if (field != null)
            {
                List<object> lstVal = new List<object>
                {
                    v1,
                    v2
                };
                dictColumnValues.Add(field, lstVal);
            }
            else
            {
                field = "#Duplicated#";
                List<object> lstVal = new List<object>();
                if (v1 == null)
                {
                    //Row1 is duplicated
                    lstVal.Add("Row1 Duplicated");
                    lstVal.Add("");
                }
                else
                {
                    lstVal.Add("");
                    lstVal.Add("Row2 Duplicated");
                }
                dictColumnValues.Add(field, lstVal);
            }
        }
        internal string CSVLine
        {
            get
            {
                StringBuilder strBuf = new StringBuilder();
                if (dictColumnValues.Count == 0)
                {
                    strBuf.Append("\"" + KEY + "\"");
                    strBuf.Append(",");
                    strBuf.Append("Missing");
                    strBuf.Append(",");
                    if (R1 == null)
                    {
                        strBuf.Append("Missing in Table1");
                        strBuf.Append(",");
                        strBuf.Append("");
                    }
                    else if (R2 == null)
                    {

                        strBuf.Append("");
                        strBuf.Append(",");
                        strBuf.Append("Missing in Table2");
                    }
                }
                else
                {
                    int i = 0;
                    foreach (string field in dictColumnValues.Keys)
                    {
                        if (i != 0)
                            strBuf.AppendLine();
                        i++;
                        strBuf.Append("\"" + KEY + "\"");
                        strBuf.Append(",");
                        strBuf.Append(field);
                        foreach (object o in dictColumnValues[field])
                        {
                            strBuf.Append(",");
                            strBuf.Append(o.ToString());
                        }
                        //strBuf.AppendLine();
                    }
                }
                return strBuf.ToString();
            }
        }
    }
}
