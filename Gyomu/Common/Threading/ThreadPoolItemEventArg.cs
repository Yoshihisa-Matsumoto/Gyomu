using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Threading
{
    internal class ThreadPoolItemEventArg : EventArgs
    {
        internal ThreadPoolItemEventArg(ExecutionUnit unit)
        {
            Unit = unit;
        }

        internal ExecutionUnit Unit { get; private set; }
    }
}
