using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.OleDb;

namespace Gyomu.Access
{
    public class DataSetAccess
    {

        public static void Export<T>(T dt, string filename)
                        where T : DataTable
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, System.IO.FileAccess.Write, FileShare.ReadWrite))
            {
                dt.WriteXml(fs);
            }
        }
        public static MemoryStream Export<T>(T dt)
            where T : DataTable
        {
            MemoryStream ms = new MemoryStream();
            dt.WriteXml(ms);
            return ms;
        }
        public static void Import<T>(ref T dt, string filename)
                        where T : DataTable
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                dt.ReadXml(fs);
            }
        }
        public static void Import<T>(ref T dt, MemoryStream ms)
            where T : DataTable
        {
            ms.Position = 0;
            dt.ReadXml(ms);
        }
        public static void DumpDataSet(DataTable dt, string filename, List<string> lstSkipColumn = null)
        {
            List<string> lstColumnName = new List<string>();

            using (StreamWriter writer = new StreamWriter(filename))
            {
                StringBuilder strBuf = new StringBuilder();
                foreach (DataColumn c in dt.Columns)
                {
                    if (lstSkipColumn != null && lstSkipColumn.Contains(c.ColumnName))
                        continue;

                    strBuf.Append("\"" + c.ColumnName + "\"");
                    strBuf.Append(",");
                }
                string strLine = strBuf.ToString().Substring(0, strBuf.Length - 1);
                writer.WriteLine(strLine);

                foreach (DataRow r in dt.Rows)
                {
                    strBuf.Clear();
                    foreach (DataColumn c in dt.Columns)
                    {
                        if (lstSkipColumn != null && lstSkipColumn.Contains(c.ColumnName))
                            continue;
                        strBuf.Append(dumpDatavalue(r[c], c));
                        strBuf.Append(",");
                    }
                    strLine = strBuf.ToString().Substring(0, strBuf.Length - 1);
                    writer.WriteLine(strLine);
                    writer.Flush();
                }
                writer.Flush();
            }
        }

        private static string dumpDatavalue(object obj, DataColumn c)
        {
            if (Convert.IsDBNull(obj))
                return "\"" + "[NULL]" + "\"";

            if (c.DataType.Equals(typeof(DateTime)))
            {
                DateTime dt = (DateTime)obj;
                if (dt == dt.Date)
                    return "\"" + dt.ToString("yyyy/MM/dd") + "\"";
                else
                    return "\"" + dt.ToString("yyyy/MM/dd HH:mm:ss") + "\"";
            }
            else
                return "\"" + obj.ToString() + "\"";
        }
        //You need to install Microsoft Access Database Engine 2010 Redistributable (AccessDatabaseEngine_X64.exe)
        public static DataTable GetDataTableFromCSV(String strFilePath, Boolean isInHeader = true, string schema_ini_file = null)
        {
            DataTable dt = new DataTable();
            String strInHeader = isInHeader ? "YES" : "NO";
            String strCon = "Provider=Microsoft.ACE.OLEDB.12.0;"

                                + "Data Source=" + Path.GetDirectoryName(strFilePath) + "\\; "
                                + "Extended Properties=\"Text;HDR=" + strInHeader + ";FMT=Delimited\"";
            string strSchemeFile = null;
            if (schema_ini_file != null)
            {
                strSchemeFile = new FileInfo(strFilePath).Directory.FullName + @"\Schema.ini";
                System.IO.File.Copy(schema_ini_file, strSchemeFile, true);
            }
            OleDbConnection con = new OleDbConnection(strCon);
            String strCmd = "SELECT * FROM [" + Path.GetFileName(strFilePath) + "]";

            OleDbCommand cmd = new OleDbCommand(strCmd, con);
            OleDbDataAdapter adp = new OleDbDataAdapter(cmd);
            adp.Fill(dt);
            if (schema_ini_file != null)
                System.IO.File.Delete(strSchemeFile);
            return dt;
        }
        public static ReconcileResult QuickReconcile(DataTable tbl1, DataTable tbl2, string table1_header, string table2_header, List<string> lstKey, List<string> lstIgnore, List<string> lstAmbiguousSortKey)
        {
            ReconcileResult result = new ReconcileResult(table1_header, table2_header);

            if (tbl1.Rows.Count != tbl2.Rows.Count)
            {
                result.Add("Data Count is differnt Tbl1:" + tbl1.Rows.Count.ToString() + " tbl2:" + tbl2.Rows.Count.ToString(), null, null, null, null);
            }
            else
            {
                string strSortKey = GetSortString(lstKey);
                DataView dv1 = new DataView(tbl1, null, strSortKey, DataViewRowState.CurrentRows);
                DataView dv2 = new DataView(tbl2, null, strSortKey, DataViewRowState.CurrentRows);

                for (int i = 0; i < dv1.Count; i++)
                {
                    DataRow r1 = dv1[i].Row;
                    DataRow r2 = dv2[i].Row;
                    string key = GetFilterString(r1, lstKey);
                    foreach (DataColumn c in tbl1.Columns)
                    {
                        if (lstIgnore.Contains(c.ColumnName))
                            continue;

                        object obj1 = r1[c.ColumnName];
                        object obj2 = r2[c.ColumnName];


                        if (r1[c.ColumnName] != r2[c.ColumnName])
                        {
                            if (Convert.IsDBNull(obj1) == false && Convert.IsDBNull(obj2) == false)
                            {
                                if (c.DataType == typeof(decimal))
                                {
                                    decimal dec1 = Convert.ToDecimal(obj1);
                                    decimal dec2 = Convert.ToDecimal(obj2);
                                    if (dec1 == dec2)
                                        continue;
                                }
                                else
                                {
                                    if (obj1.ToString().Trim().Equals(obj2.ToString().Trim()))
                                        continue;
                                }
                            }
                            result.Add(key, c, r1, r2, null);
                        }
                    }
                }
            }

            return result;
        }
        public static ReconcileResult CustomReconcileCSV(string csvFilename1, string csvFilename2, bool headerExist1, bool headerExist2, string table1_header, string table2_header, Dictionary<string, string> dictKey, Dictionary<string, string> dictCompareColumns, Dictionary<string, string> dictAmbigousSortKey, BaseReconcile reconcileMethod = null, string schema_ini_file = null)
        {

            DataTable tbl1 = null;
            DataTable tbl2 = null;
            Task t1 = Task.Factory.StartNew(() =>
            {
                tbl1 = GetDataTableFromCSV(csvFilename1, headerExist1, schema_ini_file);
            });

            Task t2 = Task.Factory.StartNew(() =>
            {
                tbl2 = GetDataTableFromCSV(csvFilename2, headerExist2, schema_ini_file);
            });
            Task.WaitAll(new Task[] { t1, t2 });
            ReconcileResult result = CustomReconcile(tbl1, tbl2, table1_header, table2_header, dictKey, dictCompareColumns, dictAmbigousSortKey, reconcileMethod);
            tbl1.Clear(); tbl1 = null;
            tbl2.Clear(); tbl2 = null;
            return result;
        }
        /*
         * This reconciliation is done between different schema table
         */
        public static ReconcileResult CustomReconcile(DataTable tbl1, DataTable tbl2, string table1_header, string table2_header, Dictionary<string, string> dictKey, Dictionary<string, string> dictCompareColumns, Dictionary<string, string> dictAmbigousSortKey, BaseReconcile reconcileMethod = null)
        {
            ReconcileResult result = new ReconcileResult(table1_header, table2_header);
            if (reconcileMethod == null)
                reconcileMethod = new BaseReconcile();
            Dictionary<string, string> dictDuplicatesKey = new Dictionary<string, string>(Math.Max(tbl1.Rows.Count, tbl2.Rows.Count));
            string strSortKeyTbl1 = GetSortString(dictKey.Keys.ToList());
            List<string> lstKey2 = new List<string>();
            foreach (string k in dictKey.Keys)
                lstKey2.Add(dictKey[k]);
            string strSortKeyTbl2 = GetSortString(lstKey2);
            tbl1.DefaultView.Sort = strSortKeyTbl1;
            tbl2.DefaultView.Sort = strSortKeyTbl2;

            foreach (DataRow r1 in tbl1.Rows)
            {
                string tbl1_key = GetFilterString(r1, dictKey.Keys.ToList());
                if (dictDuplicatesKey.ContainsKey(tbl1_key))
                    continue;
                string tbl2_key = GetCounterpartFilterString(r1, dictKey, true);
                DataRow[] rows = tbl2.Select(tbl2_key);

                if (rows == null || rows.Length == 0)
                {
                    result.Add(tbl1_key, null, r1, null, null);
                }
                else
                {
                    if (rows.Length > 1)
                    {
                        lock (dictDuplicatesKey)
                        {
                            if (dictDuplicatesKey.ContainsKey(tbl1_key) == false)
                                dictDuplicatesKey.Add(tbl1_key, tbl2_key);
                        }

                    }
                    else
                    {
                        DataRow r2 = rows[0];
                        reconcileMethod.Reconcile(result, r1, r2, tbl1_key, table1_header, table2_header, dictKey, dictCompareColumns);
                    }
                }
            }
            int i = 0;
            foreach (DataRow r2 in tbl2.Rows)
            {
                i++;
                string tbl1_key = GetCounterpartFilterString(r2, dictKey, false);

                if (dictDuplicatesKey.ContainsKey(tbl1_key))
                    continue;
                string tbl2_key = GetFilterString(r2, dictKey.Values.ToList());
                DataRow[] rows = tbl1.Select(tbl1_key);
                if (rows == null || rows.Length == 0)
                {
                    result.Add(tbl1_key, null, null, r2, null);
                }
                else if (rows.Length > 1)
                {
                    lock (dictDuplicatesKey)
                    {
                        if (dictDuplicatesKey.ContainsKey(tbl1_key) == false)
                            dictDuplicatesKey.Add(tbl1_key, tbl2_key);
                    }
                }
            }

            foreach (string key in dictDuplicatesKey.Keys)
            {
                Console.WriteLine("Duplicate in " + key);
                reconcileCustomDuplicates(key, dictDuplicatesKey[key], result, dictCompareColumns, dictAmbigousSortKey, tbl1, tbl2, reconcileMethod);
            }

            return result;
        }
        private static void reconcileCustomDuplicates(string tbl1_key, string tbl2_key, ReconcileResult result, Dictionary<string, string> dictCompareColumns, Dictionary<string, string> dictAmbigousSortKey, DataTable tbl1, DataTable tbl2, BaseReconcile reconcileMethod)
        {
            StringBuilder strBufTbl1 = new StringBuilder();
            StringBuilder strBufTbl2 = new StringBuilder();
            foreach (string i in dictAmbigousSortKey.Keys)
            {
                if (strBufTbl1.Length > 0)
                {
                    strBufTbl1.Append(" , ");
                    strBufTbl2.Append(" , ");
                }
                strBufTbl1.Append(i);
                strBufTbl2.Append(dictAmbigousSortKey[i]);
            }
            DataView dv1 = new DataView(tbl1, tbl1_key, strBufTbl1.ToString(), DataViewRowState.CurrentRows);
            DataView dv2 = new DataView(tbl2, tbl2_key, strBufTbl2.ToString(), DataViewRowState.CurrentRows);
            if (dv1.Count != dv2.Count)
            {
                Console.WriteLine("Count mismatch in" + tbl1_key);
                Console.WriteLine("Table1:" + dv1.Count + "\tTable2:" + dv2.Count);
                result.AddCountMismatch(tbl1_key, new DataColumn("Different Count"), dv1.Count, dv2.Count);
            }
            else
            {
                for (int i = 0; i < dv1.Count; i++)
                {
                    DataRow r1 = dv1[i].Row;
                    DataRow r2 = dv2[i].Row;

                    foreach (string tbl1_column_name in dictCompareColumns.Keys)
                    {
                        reconcileMethod.simpleMatching(result, r1, r2, tbl1_column_name, dictCompareColumns[tbl1_column_name], tbl1_key);
                        
                    }
                }
            }

        }
        public static ReconcileResult Reconcile(DataTable tbl1, DataTable tbl2, string table1_header, string table2_header, List<string> lstKey, List<string> lstIgnore, List<string> lstAmbiguousSortKey)
        {
            ReconcileResult result = new ReconcileResult(table1_header, table2_header);
            List<string> lstDuplicatesKey = new List<string>();
            string strSortKey = GetSortString(lstKey);
            tbl1.DefaultView.Sort = strSortKey;
            tbl2.DefaultView.Sort = strSortKey;

            foreach (DataRow r1 in tbl1.Rows)
            {
                string key = GetFilterString(r1, lstKey);
                DataRow[] rows = tbl2.Select(key);

                if (rows == null || rows.Length == 0)
                {
                    result.Add(key, null, r1, null, null);
                }
                else
                {
                    if (rows.Length > 1)
                    {
                        lock (lstDuplicatesKey)
                        {
                            if (lstDuplicatesKey.Contains(key) == false)
                                lstDuplicatesKey.Add(key);
                        }

                    }
                    else
                    {
                        DataRow r2 = rows[0];
                        foreach (DataColumn c in tbl1.Columns)
                        {
                            if (lstIgnore.Contains(c.ColumnName))
                                continue;

                            object obj1 = r1[c.ColumnName];
                            object obj2 = r2[c.ColumnName];


                            if (r1[c.ColumnName] != r2[c.ColumnName])
                            {
                                if (Convert.IsDBNull(obj1) == false && Convert.IsDBNull(obj2) == false)
                                {
                                    //if (c.DataType == typeof(string))
                                    {
                                        if (obj1.ToString().Trim().Equals(obj2.ToString().Trim()))
                                            continue;
                                    }
                                }
                                result.Add(key, c, r1, r2, null);
                            }
                        }
                    }
                }
            }

            foreach (DataRow r2 in tbl2.Rows)
            {
                string key = GetFilterString(r2, lstKey);
                DataRow[] rows = tbl1.Select(key);
                if (rows == null || rows.Length == 0)
                {
                    result.Add(key, null, null, r2, null);
                }
                else if (rows.Length > 1)
                {
                    lock (lstDuplicatesKey)
                    {
                        if (lstDuplicatesKey.Contains(key) == false)
                            lstDuplicatesKey.Add(key);
                    }
                }
            }

            foreach (string key in lstDuplicatesKey)
            {
                reconcileDuplicates(key, result, lstIgnore, lstAmbiguousSortKey, tbl1, tbl2);
            }

            return result;
        }
        private static void reconcileDuplicates(string key, ReconcileResult result, List<string> lstIgnore, List<string> lstAmbiguousSortKey, DataTable tbl1, DataTable tbl2)
        {
            StringBuilder strBuf = new StringBuilder();
            foreach (string i in lstAmbiguousSortKey)
            {
                if (strBuf.Length > 0)
                    strBuf.Append(" , ");
                strBuf.Append(i);
            }
            DataView dv1 = new DataView(tbl1, key, strBuf.ToString(), DataViewRowState.CurrentRows);
            DataView dv2 = new DataView(tbl2, key, strBuf.ToString(), DataViewRowState.CurrentRows);
            if (dv1.Count != dv2.Count)
            {
                Console.WriteLine("Count mismatch in" + key);
                Console.WriteLine("Table1:" + dv1.Count + "\tTable2:" + dv2.Count);
                result.AddCountMismatch(key, new DataColumn("Different Count"), dv1.Count, dv2.Count);
            }
            else
            {
                for (int i = 0; i < dv1.Count; i++)
                {
                    DataRow r1 = dv1[i].Row;
                    DataRow r2 = dv2[i].Row;
                    foreach (DataColumn c in tbl1.Columns)
                    {
                        if (lstIgnore.Contains(c.ColumnName))
                            continue;

                        object obj1 = r1[c.ColumnName];
                        object obj2 = r2[c.ColumnName];


                        if (r1[c.ColumnName] != r2[c.ColumnName])
                        {
                            if (Convert.IsDBNull(obj1) == false && Convert.IsDBNull(obj2) == false)
                            {
                                //if (c.DataType == typeof(string))
                                {
                                    if (obj1.ToString().Trim().Equals(obj2.ToString().Trim()))
                                        continue;
                                }
                            }
                            string subKey = GetFilterString(r1, lstAmbiguousSortKey);
                            //Console.WriteLine(key + ":" + subKey);
                            result.Add(key + ":" + subKey, c, r1, r2, null);
                        }
                    }
                }
            }

        }
        private static string GetSortString(List<string> lstKey)
        {
            StringBuilder strBuf = new StringBuilder();
            foreach (string c in lstKey)
            {
                if (strBuf.Length > 0)
                    strBuf.Append(",");
                strBuf.Append(c);
            }
            return strBuf.ToString();
        }
        private static string GetCounterpartFilterString(DataRow r, Dictionary<string, string> dictKey, bool isDataRowTbl1)
        {
            StringBuilder strBuf = new StringBuilder();
            foreach (string tbl1_col_name in dictKey.Keys)
            {
                string source_column = isDataRowTbl1 ? tbl1_col_name : dictKey[tbl1_col_name];
                string dest_column = isDataRowTbl1 ? dictKey[tbl1_col_name] : tbl1_col_name;
                object objTmp = r[source_column];
                if (strBuf.Length > 0)
                {
                    strBuf.Append(" AND ");
                }
                strBuf.Append(dest_column);
                if (Convert.IsDBNull(objTmp))
                {
                    strBuf.Append(" IS NULL");
                }
                else
                {
                    strBuf.Append(" = ");
                    if (r.Table.Columns[source_column].DataType == typeof(DateTime))
                    {
                        strBuf.Append("#");
                        DateTime dt = (DateTime)objTmp;
                        strBuf.Append(dt.ToString());
                        strBuf.Append("#");
                    }
                    else if (r.Table.Columns[source_column].DataType == typeof(string))
                    {
                        strBuf.Append("'");
                        strBuf.Append(objTmp.ToString());
                        strBuf.Append("'");
                    }
                    else
                    {
                        strBuf.Append(objTmp.ToString());
                    }
                }
            }
            return strBuf.ToString();
        }
        private static string GetFilterString(DataRow r, List<string> lstKey)
        {
            StringBuilder strBuf = new StringBuilder();
            foreach (string c in lstKey)
            {
                object objTmp = r[c];
                if (strBuf.Length > 0)
                {
                    strBuf.Append(" AND ");
                }
                strBuf.Append(c);
                if (Convert.IsDBNull(objTmp))
                {
                    strBuf.Append(" IS NULL");
                }
                else
                {
                    strBuf.Append(" = ");
                    if (r.Table.Columns[c].DataType == typeof(DateTime))
                    {
                        strBuf.Append("#");
                        DateTime dt = (DateTime)objTmp;
                        strBuf.Append(dt.ToString());
                        strBuf.Append("#");
                    }
                    else if (r.Table.Columns[c].DataType == typeof(string))
                    {
                        strBuf.Append("'");
                        strBuf.Append(objTmp.ToString());
                        strBuf.Append("'");
                    }
                    else
                    {
                        strBuf.Append(objTmp.ToString());
                    }
                }
            }
            return strBuf.ToString();
        }
        public static bool SimpleReconcileSortedCSV(string filename1, string filename2)
        {
            FileInfo fi1 = new FileInfo(filename1);
            FileInfo fi2 = new FileInfo(filename2);
            bool isMatch = fi1.Length == fi2.Length;
            int THRESHOLD = 5;
            using (StreamReader reader1 = new StreamReader(filename1))
            {
                using (StreamReader reader2 = new StreamReader(filename2))
                {
                    string strLine1, strLine2;
                    int cnt = 0;
                    int line = 0;
                    while ((strLine1 = reader1.ReadLine()) != null)
                    {
                        line++;
                        strLine2 = reader2.ReadLine();
                        if (strLine2 != null)
                        {
                            if (strLine1.Equals(strLine2) == false)
                            {
                                isMatch = false;
                                if (cnt < THRESHOLD)
                                {
                                    Console.WriteLine("Line " + line.ToString() + " is different");
                                    Console.WriteLine(strLine1);
                                    Console.WriteLine(strLine2);
                                    Console.WriteLine();
                                }
                                cnt++;
                            }
                        }
                        else
                        {
                            isMatch = false;
                            if (cnt < THRESHOLD)
                            {
                                Console.WriteLine("Line " + line + " not exist on file2");
                            }
                            cnt++;
                        }
                    }
                    while ((strLine2 = reader2.ReadLine()) != null)
                    {
                        line++;
                        isMatch = false;
                        if (cnt < THRESHOLD)
                        {
                            Console.WriteLine("Line " + line + " not exist on file1");
                        }
                        cnt++;

                    }

                    Console.WriteLine("Total Error: " + cnt);
                }
            }

            return isMatch;
        }

    }
    public class BaseReconcile
    {
        public virtual void Reconcile(ReconcileResult result, DataRow r1, DataRow r2, string key, string table1_header, string table2_header, Dictionary<string, string> dictKey, Dictionary<string, string> dictCompareColumns)
        {
            foreach (string tbl1_column_name in dictCompareColumns.Keys)
            {

                simpleMatching(result, r1, r2, tbl1_column_name, dictCompareColumns[tbl1_column_name], key);
            }
        }
        internal void simpleMatching(ReconcileResult result, DataRow r1, DataRow r2, string col1, string col2, string key)
        {
            object obj1 = r1[col1];
            object obj2 = r2[col2];


            if (obj1 != obj2)
            {
                if (Convert.IsDBNull(obj1) == false && Convert.IsDBNull(obj2) == false)
                {
                    //if (c.DataType == typeof(string))
                    {
                        if (obj1.ToString().Trim().Equals(obj2.ToString().Trim()))
                            return;
                    }
                }
                result.Add(key, r1.Table.Columns[col1], r1, r2, null);
            }
        }
    }
    public class ReconcileResult
    {
        string Table1ID { get; set; }
        string Table2ID { get; set; }
        internal ReconcileResult(string tbl1, string tbl2)
        {
            Table1ID = tbl1;
            Table2ID = tbl2;
        }
        public Dictionary<string, ReconcileItem> dictKeyResult = new Dictionary<string, ReconcileItem>();
        internal void AddCountMismatch(string key, DataColumn c, int cnt1, int cnt2)
        {
            ReconcileItem item = null;
            lock (dictKeyResult)
            {
                if (dictKeyResult.ContainsKey(key) == false)
                    dictKeyResult.Add(key, new ReconcileItem(null, null, key));
            }
            item = dictKeyResult[key];
            item.Add(c, cnt1, cnt2);
        }
        internal void Add(string key, DataColumn c, DataRow r1, DataRow r2, DataRow[] duplicatedRows)
        {
            ReconcileItem item = null;
            lock (dictKeyResult)
            {
                if (dictKeyResult.ContainsKey(key) == false)
                    dictKeyResult.Add(key, new ReconcileItem(r1, r2, key));
            }
            item = dictKeyResult[key];
            if (r1 != null && r2 != null && c != null)
            {
                lock (item)
                {
                    item.Add(c, r1[c.ColumnName], r2[c.ColumnName]);
                }
            }
            else if (duplicatedRows != null)
            {
                if (duplicatedRows.Contains(r1))
                {
                    lock (item)
                    {
                        item.Add(c, null, r2[0]);
                    }

                }
                else if (duplicatedRows.Contains(r2))
                    lock (item)
                        item.Add(c, r1[0], null);
            }
        }
        public bool Matched { get { return dictKeyResult.Count == 0; } }

        public void Export(string filename)
        {
            using (System.IO.StreamWriter writer = new StreamWriter(filename, false, Encoding.GetEncoding("Shift_JIS")))
            {
                writer.WriteLine("KEY,Item,\"" + Table1ID + "\",\"" + Table2ID + "\"");
                foreach (string key in dictKeyResult.Keys)
                {
                    ReconcileItem i = dictKeyResult[key];
                    writer.WriteLine(i.CSVLine);
                    writer.Flush();
                }
            }
        }
    }
    public class ReconcileItem
    {
        public string KEY { get; set; }
        public DataRow R1 { get; set; }
        public DataRow R2 { get; set; }
        Dictionary<DataColumn, List<object>> dictColumnValues = new Dictionary<DataColumn, List<object>>();

        public Dictionary<DataColumn, List<object>> COLUMN_VALUES { get { return dictColumnValues; } }

        internal ReconcileItem(DataRow r1, DataRow r2, string key)
        {
            R1 = r1; R2 = r2; KEY = key;
        }
        internal void Add(DataColumn c, object v1, object v2)
        {
            if (c != null)
            {
                List<object> lstVal = new List<object>();
                lstVal.Add(v1);
                lstVal.Add(v2);
                dictColumnValues.Add(c, lstVal);
            }
            else
            {
                c = new DataColumn("#Duplicated#");
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
                dictColumnValues.Add(c, lstVal);
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
                    foreach (DataColumn c in dictColumnValues.Keys)
                    {
                        strBuf.Append("\"" + KEY + "\"");
                        strBuf.Append(",");
                        strBuf.Append(c.ColumnName);
                        foreach (object o in dictColumnValues[c])
                        {
                            strBuf.Append(",");
                            strBuf.Append(o.ToString());
                        }
                        strBuf.AppendLine();
                    }
                }
                return strBuf.ToString();
            }
        }
    }
}
