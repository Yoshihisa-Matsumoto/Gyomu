using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Service
{
    public interface IService
    {
        Common.Configurator Config { get; }
        
        short ID { get; }
        string Description { get; }
        string Parameter { get; }

        ServiceCommonType.ServiceState STATE { get; }

        void Init(Common.Configurator config, Models.ParameterSet parameter, Models.Service service);

        void Stop();
        StatusCode Run();

        int? PercentProgress { get; }

        string CurrentTask { get; }
        int? TaskCount { get; }
        List<long> TaskList { get; }
        bool IsCompleted(string parameter);
        StatusCode IssueCommand(string parameter);
    }
}
