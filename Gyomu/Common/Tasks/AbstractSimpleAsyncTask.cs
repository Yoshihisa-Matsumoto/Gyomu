using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Tasks
{
    public abstract class AbstractSimpleAsyncTask:AbstractSimpleTask
    {
        private protected override bool IsAsynchrnous { get { return true; } }
        protected override StatusCode OnCancel(string parameter, string comment)
        {
            AsyncThread.Abort();
            return base.OnCancel(parameter, comment);
        }
    }
}
