using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Service
{
    public class ServiceCommonType
    {
        public enum ServiceState
        {
            Initializing,
            Stopping,
            Stop,
            Running,
            Executing
        }
    }
}
