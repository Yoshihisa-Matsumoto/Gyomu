using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class FileFilterInfo
    {
        public enum FilterType
        {
            FileName,
            CreateTimeUtc, //All UTC
            LastAccessTimeUtc, //All UTC 
            LastWriteTimeUtc //All UTC 
        }

        internal FilterType Kind { get; private set; }

        public enum CompareType
        {
            Equal,
            Larger,
            Less,
            LargerEqual,
            LessEqual
        }

        public CompareType Operator { get; private set; }

        public object FilterData { get; private set; }

        public FileFilterInfo(FilterType type, CompareType comparer, object filter)
        {
            Kind = type;
            Operator = comparer;
            FilterData = filter;

            init();
        }
        private string strFileNameComparer = null;
        internal string NameFilter { get { return strFileNameComparer; } }
        private DateTime targetDate;
        internal DateTime TargetDate { get { return targetDate; } }

        private void init()
        {
            if (Kind == FilterType.FileName)
            {
                strFileNameComparer = (string)FilterData;
            }
            else
            {
                if (FilterData is DateTime)
                    targetDate = (DateTime)FilterData;
                else if (FilterData is string strDate)
                {
                    if (DateTime.TryParseExact(strDate, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                           System.Globalization.DateTimeStyles.None, out targetDate) == false)
                    {
                        if (DateTime.TryParse(strDate, out targetDate) == false)
                        {
                            throw new InvalidOperationException("Date Parameter is invalid: " + strDate);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Date Parameter is invalid: " + FilterData.ToString());
                }
            }
        }
    }
}
