using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Threading
{
    public interface ExecutionUnit
    {
        StatusCode OnStart();
        StatusCode OnExec();
        StatusCode OnEnd();
        StatusCode OnError();

        bool IsRunning { get; }
    }
}
