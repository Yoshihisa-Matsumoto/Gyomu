using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Access
{
    public class DictionaryAccess
    {
        public static StatusCode ReconcileData<T, EType>(System.Collections.IDictionary dictA, System.Collections.IDictionary dictB, Models.ReconcileResult<T, EType> result, int[] customFields = null)
                    where T : Models.BaseItemRecord, new()
                    where EType : struct
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            retVal = reconcileDataA<T, EType>(dictA, dictB, result, null, customFields);
            if (retVal.IsSucceeded == false)
                return retVal;
            retVal = reconcileDataB<T, EType>(dictA, dictB, result, null);
            if (retVal.IsSucceeded == false)
                return retVal;
            return retVal;
        }
        private static StatusCode reconcileDataA<T, EType>(System.Collections.IDictionary dictA, System.Collections.IDictionary dictB, Models.ReconcileResult<T, EType> result, string strPrefix, int[] customFields)
                    where T : Models.BaseItemRecord, new()
                    where EType : struct
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            string strKey = null;
            foreach (object key in dictA.Keys)
            {
                string strItem = null;
                if (key.GetType().IsEnum)
                    strItem = EnumAccess.GetEnumValueDescription(key);
                else
                    strItem = key.ToString();
                strKey = strPrefix;
                object valA = dictA[key];
                if (strKey == null)
                    strKey = strItem;
                else
                    strKey += ":" + strItem;

                if (dictB.Contains(key) == false)
                {
                    T item = new T();
                    result.Add(strKey, default(EType), item, null, null);
                    continue;
                }
                object valB = dictB[key];
                if (valA is System.Collections.IDictionary colA)
                {
                    System.Collections.IDictionary colB = (System.Collections.IDictionary)valB;

                    retVal = reconcileDataA<T, EType>(colA, colB, result, strKey, customFields);
                    if (retVal.IsSucceeded == false)
                        return retVal;
                }
                else if (valA is T tA)
                {
                    T tB = (T)valB;
                    if (customFields != null)
                    {
                        foreach (int fld in customFields)
                        {
                            if (tA[fld].Equals(tB[fld]) == false)
                            {

                                result.Add(strKey, (EType)((object)fld), tA, tB, null);
                            }
                        }
                    }
                    else
                    {
                        foreach (int fld in tA.Fields)
                        {
                            if (tA[fld].Equals(tB[fld]) == false)
                            {

                                result.Add(strKey, (EType)((object)fld), tA, tB, null);
                            }
                        }
                    }
                }
                else
                {
                    //Throw Exception

                }

            }

            return retVal;
        }
        private static StatusCode reconcileDataB<T, EType>(System.Collections.IDictionary dictA, System.Collections.IDictionary dictB, Models.ReconcileResult<T, EType> result, string strPrefix)
                    where T : Models.BaseItemRecord, new()
                    where EType : struct
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            string strKey = null;
            foreach (object key in dictB.Keys)
            {
                string strItem = null;
                if (key.GetType().IsEnum)
                    strItem = EnumAccess.GetEnumValueDescription(key);
                else
                    strItem = key.ToString();
                strKey = strPrefix;
                object valB = dictB[key];
                if (strKey == null)
                    strKey = strItem;
                else
                    strKey += ":" + strItem;

                if (dictA.Contains(key) == false)
                {
                    T item = new T();
                    result.Add(strKey, default(EType), null, item, null);
                    continue;
                }
                object valA = dictA[key];
                if (valB is System.Collections.IDictionary)
                {
                    System.Collections.IDictionary colA = (System.Collections.IDictionary)valA;
                    System.Collections.IDictionary colB = (System.Collections.IDictionary)valB;

                    retVal = reconcileDataB<T, EType>(colA, colB, result, strKey);
                    if (retVal.IsSucceeded == false)
                        return retVal;
                }

            }

            return retVal;
        }
        public static void BuildDictionary<K, R>(K key, R record, int[] fields, ref Dictionary<K, R> dictData)
            where R : Models.BaseItemRecord, new()
        {

            if (dictData.ContainsKey(key) == false)
            {
                if (fields == null)
                {
                    dictData.Add(key, record);
                    return;
                }
                else
                    dictData.Add(key, new R());
            }
            R baseData = dictData[key];
            if (fields == null)
                fields = record.Fields;
            foreach (int iField in fields)
            {
                baseData.Add(iField, record[iField]);
            }

        }
        public static void BuildDictionary<K1, K2, R>(K1 key1, K2 key2, R record, int[] fields, ref Dictionary<K1, Dictionary<K2, R>> dictData)
               where R : Models.BaseItemRecord, new()
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, R>());
            Dictionary<K2, R> dictChild = dictData[key1];
            BuildDictionary<K2, R>(key2, record, fields, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, R>(K1 key1, K2 key2, K3 key3, R record, int[] fields, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, R>>> dictData)
               where R : Models.BaseItemRecord, new()
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, R>>());
            Dictionary<K2, Dictionary<K3, R>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, R>(key2, key3, record, fields, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, K4, R>(K1 key1, K2 key2, K3 key3, K4 key4, R record, int[] fields, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, R>>>> dictData)
               where R : Models.BaseItemRecord, new()
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, Dictionary<K4, R>>>());
            Dictionary<K2, Dictionary<K3, Dictionary<K4, R>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, K4, R>(key2, key3, key4, record, fields, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, K4, K5, R>(K1 key1, K2 key2, K3 key3, K4 key4, K5 key5, R record, int[] fields, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, R>>>>> dictData)
               where R : Models.BaseItemRecord, new()
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, R>>>>());
            Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, R>>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, K4, K5, R>(key2, key3, key4, key5, record, fields, ref dictChild);
        }
        public static void BuildDictionary<K, R>(K key, R record, ref Dictionary<K, List<R>> dictData)
        {
            if (dictData.ContainsKey(key) == false)
                dictData.Add(key, new List<R>() { record });
            else
                dictData[key].Add(record);
        }
        public static void BuildDictionary<K1, K2, R>(K1 key1, K2 key2, R record, ref Dictionary<K1, Dictionary<K2, List<R>>> dictData)
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, List<R>>());
            Dictionary<K2, List<R>> dictChild = dictData[key1];
            BuildDictionary<K2, R>(key2, record, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, R>(K1 key1, K2 key2, K3 key3, R record, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, List<R>>>> dictData)
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, List<R>>>());
            Dictionary<K2, Dictionary<K3, List<R>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, R>(key2, key3, record, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, K4, R>(K1 key1, K2 key2, K3 key3, K4 key4, R record, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, List<R>>>>> dictData)
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, Dictionary<K4, List<R>>>>());
            Dictionary<K2, Dictionary<K3, Dictionary<K4, List<R>>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, K4, R>(key2, key3, key4, record, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, K4, K5, R>(K1 key1, K2 key2, K3 key3, K4 key4, K5 key5, R record, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, List<R>>>>>> dictData)
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, List<R>>>>>());
            Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, List<R>>>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, K4, K5, R>(key2, key3, key4, key5, record, ref dictChild);
        }
        public static void BuildDictionary<K1, K2, K3, K4, K5,K6, R>(K1 key1, K2 key2, K3 key3, K4 key4, K5 key5,K6 key6, R record, ref Dictionary<K1, Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5,Dictionary<K6, List<R>>>>>>> dictData)
        {
            if (dictData.ContainsKey(key1) == false)
                dictData.Add(key1, new Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, Dictionary<K6,List<R>>>>>>());
            Dictionary<K2, Dictionary<K3, Dictionary<K4, Dictionary<K5, Dictionary<K6,List<R>>>>>> dictChild = dictData[key1];
            BuildDictionary<K2, K3, K4, K5,K6, R>(key2, key3, key4, key5,key6, record, ref dictChild);
        }
    }
}
