using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;

namespace Gyomu.Access
{
    public class MarketDateAccess
    {
        public enum SupportMarket
        {
            [Description("JP")]
            Japan
        }
        private string Market { get; set; }
        List<DateTime> holidays = null;

        static Dictionary<string, DateTime> dictMarketLastReferenceTime = new Dictionary<string, DateTime>();
        static Dictionary<string, List<DateTime>> dictMarketHolidays = new Dictionary<string, List<DateTime>>();

        public MarketDateAccess(SupportMarket market)
        {
            Market = EnumAccess.GetEnumValueDescription(market);
            init();
        }
        private void init()
        {
            lock (dictMarketHolidays)
            {
                lock (dictMarketLastReferenceTime)
                {
                    if (dictMarketLastReferenceTime.ContainsKey(Market)
                        && dictMarketHolidays.ContainsKey(Market)
                        && DateTime.UtcNow.Subtract(dictMarketLastReferenceTime[Market]).TotalMinutes < 15)
                    {
                        holidays = dictMarketHolidays[Market];
                        return;
                    }
                    try
                    {
                        using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
                        {
                            holidays = Common.GyomuDataAccess.ListHoliday(Market);

                            if (dictMarketHolidays.ContainsKey(Market) == false)
                                dictMarketHolidays.Add(Market, holidays);
                            else
                                dictMarketHolidays[Market] = holidays;
                            if (dictMarketLastReferenceTime.ContainsKey(Market) == false)
                                dictMarketLastReferenceTime.Add(Market, DateTime.UtcNow);
                            else
                                dictMarketLastReferenceTime[Market] = DateTime.UtcNow;

                        }
                    }
                    catch (Exception ex)
                    {
                        if (dictMarketHolidays.ContainsKey(Market))
                            holidays = dictMarketHolidays[Market];
                        else
                            throw ex;
                    }
                }
            }
        }

        public int MarketOffset(DateTime targetDate, DateTime offsetDate)
        {
            if (targetDate.Equals(offsetDate))
                return 0;
            int iMove = 1;
            if (targetDate < offsetDate)
            {
                iMove = -1;
            }
            DateTime tmpDate = offsetDate;
            int iOffset = 0;
            while (tmpDate.Equals(targetDate) == false)
            {
                tmpDate = GetBusinessDay(tmpDate, iMove);
                iOffset += iMove;
            }
            return iOffset;
        }
        public DateTime GetBusinessDay(DateTime targetDate, int days)
        {
            if (days == 0)
                throw new InvalidOperationException("days need to be non-zero");

            if (days > 0)
                return getNextBusinessDay(targetDate, days);
            else
                return getPreviousBusinessDay(targetDate, Math.Abs(days));
        }
        private DateTime getNextBusinessDay(DateTime targetDate, int days)
        {
            if (days <= 0)
                throw new InvalidOperationException("Negative value not allowed");
            DateTime busDay = targetDate;
            while (days > 0)
            {
                busDay = busDay.AddDays(1);
                if (_isBusinessDay(busDay))
                    days--;
            }
            return busDay;
        }
        private DateTime getPreviousBusinessDay(DateTime targetDate, int days)
        {
            if (days <= 0)
                throw new InvalidOperationException("Negative value not allowed");
            DateTime busDay = targetDate;
            while (days > 0)
            {
                busDay = busDay.AddDays(-1);
                if (_isBusinessDay(busDay))
                    days--;
            }
            return busDay;
        }
        public DateTime GetBusinessDayOfBeginningMonthWithOffset(DateTime targetDate, int offsetDays=1)
        {
            DateTime dtBBOM = new DateTime(targetDate.Year, targetDate.Month, 1);
            if (IsBusinessDay(dtBBOM))
                if (offsetDays > 1)
                    return GetBusinessDay(dtBBOM, offsetDays - 1);
                else
                    return dtBBOM;

           return GetBusinessDay(dtBBOM, offsetDays);
        }
        public DateTime GetBusinessDayOfNextBeginningMonthWithOffset(DateTime targetDate, int offsetDays = 1)
        {
            DateTime dtNextBBOM = new DateTime(targetDate.Year + (targetDate.Month == 12 ? 1 : 0), targetDate.Month + (targetDate.Month == 12 ? -11 : 1), 1);
            DateTime dtTmpNextBBOM = dtNextBBOM;
            if (IsBusinessDay(dtNextBBOM))
            {
                if (offsetDays > 1)
                    dtTmpNextBBOM = GetBusinessDay(dtNextBBOM, offsetDays - 1);
            }
            else
                dtTmpNextBBOM = GetBusinessDay(dtNextBBOM, offsetDays);
            if (dtTmpNextBBOM.Equals(targetDate) || targetDate > dtTmpNextBBOM)
            {
                dtNextBBOM = dtNextBBOM.AddMonths(1);
                if (IsBusinessDay(dtNextBBOM))
                {
                    if (offsetDays > 1)
                        dtTmpNextBBOM = GetBusinessDay(dtNextBBOM, offsetDays - 1);
                }
                else
                    dtTmpNextBBOM = GetBusinessDay(dtNextBBOM, offsetDays);
            }
            return dtTmpNextBBOM;
        }
        public DateTime GetBusinessDayOfPreviousBeginningMonthWithOffset(DateTime targetDate, int offsetDays = 1)
        {
            DateTime dtPrevBBOM = new DateTime(targetDate.Year + (targetDate.Month == 1 ? -1 : 0), targetDate.Month + (targetDate.Month == 1 ? 11 : 1), 1);
            DateTime dtTmpPrevBBOM = dtPrevBBOM;
            if (IsBusinessDay(dtPrevBBOM))
            {
                if (offsetDays > 1)
                    dtTmpPrevBBOM = GetBusinessDay(dtPrevBBOM, offsetDays - 1);
            }
            else
                dtTmpPrevBBOM = GetBusinessDay(dtPrevBBOM, offsetDays);
            if (dtTmpPrevBBOM.Equals(targetDate) || targetDate > dtTmpPrevBBOM)
            {
                dtPrevBBOM = dtPrevBBOM.AddMonths(1);
                if (IsBusinessDay(dtPrevBBOM))
                {
                    if (offsetDays > 1)
                        dtTmpPrevBBOM = GetBusinessDay(dtPrevBBOM, offsetDays - 1);
                }
                else
                    dtTmpPrevBBOM = GetBusinessDay(dtPrevBBOM, offsetDays);
            }
            return dtTmpPrevBBOM;
        }
        public DateTime GetBusinessDayOfEndMonthWithOffset(DateTime targetDate,int offsetDays)
        {
            DateTime dtEBOM = new DateTime(targetDate.Year + (targetDate.Month == 12 ? 1 : 0), targetDate.Month + (targetDate.Month == 12 ? -11 : 1), 1);
            return GetBusinessDay(dtEBOM, -offsetDays);
        }
        public DateTime GetBusinessDayOfNextEndMonthWithOffset(DateTime targetDate,int offsetDays)
        {
            DateTime dtNextEBOM = new DateTime(targetDate.Year + (targetDate.Month == 12 ? 1 : 0), targetDate.Month + (targetDate.Month == 12 ? -11 : 1), 1);
            DateTime dtTmpNextEBOM = GetBusinessDay(dtNextEBOM, -offsetDays);
            if (targetDate.Equals(dtTmpNextEBOM) || targetDate < dtTmpNextEBOM)
            {
                dtNextEBOM = dtNextEBOM.AddMonths(1);
                dtTmpNextEBOM = GetBusinessDay(dtNextEBOM, -offsetDays);
            }
            return dtTmpNextEBOM;
        }
        public DateTime GetBusinessDayOfPreviousEndMonthWithOffset(DateTime targetDate,int offsetDays)
        {
            DateTime dtPREEBOM = new DateTime(targetDate.Year, targetDate.Month, 1);
            return GetBusinessDay(dtPREEBOM, -offsetDays);
        }
        public DateTime GetBusinessDayOfBeginningOfYear(DateTime targetDate, int offsetDays)
        {
            DateTime dtBBOY = new DateTime(targetDate.Year, 1, 1);
            if (IsBusinessDay(dtBBOY))
                return GetBusinessDay(dtBBOY, offsetDays - 1);

            else
                return GetBusinessDay(dtBBOY, offsetDays);
        }
        public DateTime GetBusinessDayOfEndOfYear(DateTime targetDate, int offsetDays)
        {
            DateTime dtBEOY = new DateTime(targetDate.Year + 1, 1, 1);
            return GetBusinessDay(dtBEOY, -offsetDays);
        }
        public bool IsBusinessDay(DateTime targetDate)
        {
            return _isBusinessDay(targetDate);
        }
        
        private bool _isBusinessDay(DateTime targetDate)
        {
            switch (targetDate.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return false;
                default:
                    int cnt = holidays.Count(h => h.Equals(targetDate));
                    if (cnt > 0)
                        return false;
                    return true;
            }
        }
    }
}
