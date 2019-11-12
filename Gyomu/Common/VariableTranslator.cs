using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Gyomu.Common
{
    public class VariableTranslator
    {
        public VariableTranslator() {  }
        public VariableTranslator(Access.MarketDateAccess marketAccess) { MarketAccess = marketAccess; }
        public string Parse(string input_string, DateTime targetDate)
        {
            int iStart = input_string.IndexOf("{%");
            int iEnd = input_string.IndexOf("%}");
            if (iStart != -1 && iEnd != -1 && iEnd > iStart)
            {
                string prefix = input_string.Substring(0, iStart);
                string keyword = input_string.Substring(iStart + 2, (iEnd - iStart - 2));
                string suffix = input_string.Substring(iEnd + 2);

                input_string = prefix + Translate(keyword, targetDate) + suffix;
                return Parse(input_string, targetDate);
            }
            else
                return input_string;
        }
        List<Models.VariableParameter> Variables = GyomuDataAccess.GetVariableParameters();
        private Access.MarketDateAccess MarketAccess { get; set; }
        private enum VariableType
        {
            Date,
            ParamMaster,
        };

        private string Translate(string keyword, DateTime targetDate)
        {
            string[] parts = keyword.Split('$');
            int iFactor = 1;
            StringBuilder strBuf = new StringBuilder();
            DateTime? varDate = null;
            VariableType vt = VariableType.Date;
            foreach (string p in parts)
            {

                if (Int32.TryParse(p, out int iTmp))
                    iFactor = iTmp;
                
                else
                {
                    try
                    {
                        Access.MarketDateAccess.SupportMarket market = Access.EnumAccess.Parse<Access.MarketDateAccess.SupportMarket>(p);
                        MarketAccess = new Access.MarketDateAccess(market);
                        continue;
                    }
                    catch (Exception) { }

                    Models.VariableParameter varParameter = Variables.Where(v => v.variable_key.Equals(p)).FirstOrDefault();
                    if (varParameter != null)
                    {
                        switch (varParameter.variable_key)
                        {
                            case "TODAY":
                                varDate = targetDate;
                                break;
                            case "BBOM":
                                varDate = MarketAccess.GetBusinessDayOfBeginningMonthWithOffset(targetDate, iFactor);
                                break;
                            case "NEXTBBOM":
                                varDate = MarketAccess.GetBusinessDayOfNextBeginningMonthWithOffset(targetDate, iFactor);
                                break;
                            case "BOM":
                                DateTime dtBOM = new DateTime(targetDate.Year, targetDate.Month, 1);
                                varDate = dtBOM.AddDays(iFactor - 1);
                                break;
                            case "BEOM":
                                varDate = MarketAccess.GetBusinessDayOfEndMonthWithOffset(targetDate, iFactor);
                                break;
                            case "NEXTBEOM":
                                varDate = MarketAccess.GetBusinessDayOfNextEndMonthWithOffset(targetDate, iFactor);
                                break;
                            case "PREVBEOM":
                                varDate = MarketAccess.GetBusinessDayOfPreviousEndMonthWithOffset(targetDate, iFactor);
                                break;
                            case "EOM":
                                DateTime dtEOM = new DateTime(targetDate.Year + (targetDate.Month == 12 ? 1 : 0), targetDate.Month + (targetDate.Month == 12 ? -11 : 1), 1);
                                varDate = dtEOM.AddDays(-iFactor);
                                break;
                            case "NEXTBUS":
                                varDate = MarketAccess.GetBusinessDay(targetDate, iFactor);
                                break;
                            case "NEXTDAY":
                                varDate = targetDate.AddDays(iFactor);
                                break;
                            case "PREVBUS":
                                varDate = MarketAccess.GetBusinessDay(targetDate, -iFactor);
                                break;
                            case "PREVDAY":
                                varDate = targetDate.AddDays(-iFactor);
                                break;
                            case "PARAMMASTER":
                                vt = VariableType.ParamMaster;
                                break;
                            case "EOY":
                                DateTime dtEOY = new DateTime(targetDate.Year + 1, 1, 1);
                                varDate = dtEOY.AddDays(-iFactor);
                                break;
                            case "BEOY":
                                varDate = MarketAccess.GetBusinessDayOfEndOfYear(targetDate, iFactor);
                                break;
                            case "BBOY":
                                varDate = MarketAccess.GetBusinessDayOfBeginningOfYear(targetDate, iFactor);
                                break;
                            case "BOY":
                                DateTime dtBOY = new DateTime(targetDate.Year, 1, 1);
                                varDate = dtBOY.AddDays(iFactor - 1);
                                break;
                            
                        }
                    }
                    else
                    {
                        switch (vt)
                        {
                            case VariableType.Date:
                                if (varDate != null)
                                    strBuf.Append(varDate.Value.ToString(p));
                                else
                                    strBuf.Append(p);
                                break;
                            case VariableType.ParamMaster:
                                strBuf.Append(Access.ParameterAccess.GetStringValue(p));
                                break;
                        }

                    }
                }



            }
            return strBuf.ToString();
        }

        public DateTime? ParseDate(string keyword, DateTime targetDate)
        {
            string[] parts = keyword.Split('$');
            int iFactor = 1;
            StringBuilder strBuf = new StringBuilder();
            DateTime? varDate = null;
            foreach (string p in parts)
            {

                if (Int32.TryParse(p, out int iTmp))
                    iFactor = iTmp;
                else
                {
                    Models.VariableParameter varParameter = Variables.Where(v => v.variable_key.Equals(p)).FirstOrDefault();
                    if (varParameter != null)
                    {
                        switch (varParameter.variable_key)
                        {
                            case "TODAY":
                                return targetDate;
                            case "BBOM":
                                return MarketAccess.GetBusinessDayOfBeginningMonthWithOffset(targetDate, iFactor);
                            case "BOM":
                                DateTime dtBOM = new DateTime(targetDate.Year, targetDate.Month, 1);
                                return dtBOM.AddDays(iFactor - 1);

                            case "BEOM":
                                return MarketAccess.GetBusinessDayOfEndMonthWithOffset(targetDate, iFactor); 

                            case "PREVBEOM":
                                return MarketAccess.GetBusinessDayOfPreviousEndMonthWithOffset(targetDate, iFactor);

                            case "EOM":
                                DateTime dtEOM = new DateTime(targetDate.Year + (targetDate.Month == 12 ? 1 : 0), targetDate.Month + (targetDate.Month == 12 ? -11 : 1), 1);
                                return dtEOM.AddDays(-iFactor);

                            case "NEXTBUS":
                                return MarketAccess.GetBusinessDay(targetDate, iFactor);

                            case "NEXTDAY":
                                return targetDate.AddDays(iFactor);

                            case "PREVBUS":
                                return MarketAccess.GetBusinessDay(targetDate, -iFactor);

                            case "PREVDAY":
                                return targetDate.AddDays(-iFactor);
                            case "EOY":
                                DateTime dtEOY = new DateTime(targetDate.Year + 1, 1, 1);
                                varDate = dtEOY.AddDays(-iFactor);
                                break;
                            case "BEOY":
                                return MarketAccess.GetBusinessDayOfEndOfYear(targetDate, iFactor);
                            case "BBOY":
                                return MarketAccess.GetBusinessDayOfBeginningOfYear(targetDate, iFactor);
                            case "BOY":
                                DateTime dtBOY = new DateTime(targetDate.Year, 1, 1);
                                varDate = dtBOY.AddDays(iFactor - 1);
                                break;
                            
                            default:
                                return null;
                        }
                    }

                }



            }
            return null;
        }
    }
}
