using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Task
{
    public abstract class AbstractDailyAsyncTask:AbstractSimpleAsyncTask
    {
        protected abstract Access.MarketDateAccess MarketAccess { get; }
        protected VariableTranslator Translator { get; private set; }
        protected override StatusCode OnExec(string parameter)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            DateTime targetDate = DateTime.Today;
            if (string.IsNullOrEmpty(parameter) == false)
            {
                if (DateTime.TryParseExact(parameter, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime tmpDate))
                    targetDate = tmpDate;
            }
            Translator = new VariableTranslator(MarketAccess);
            return OnExec(targetDate);
        }
        protected abstract StatusCode OnExec(DateTime targetDate);

    }
}
