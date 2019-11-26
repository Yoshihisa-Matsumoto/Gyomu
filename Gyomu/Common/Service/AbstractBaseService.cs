using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Service
{
    public abstract class AbstractBaseService : IService
    {
        public Common.Configurator Config { get; protected set; }

        public short ID { get; protected set; }
        public string Description { get; protected set; }

        public virtual string Parameter { get { return null; } }
        public virtual string CurrentTask { get { return null; } }
        public ServiceCommonType.ServiceState STATE { get; protected set; }
        private Models.Service service = null;
        private Models.ParameterSet parameterSet = null;
        public void Init(Common.Configurator config, Models.ParameterSet parameter, Models.Service service)
        {
            Config = config;
            Description = service.description;
            ID = service.id;
            this.service = service;
            parameterSet = parameter;
            STATE = ServiceCommonType.ServiceState.Initializing;
            init(parameter, service);
            STATE = ServiceCommonType.ServiceState.Stop;
        }

        protected abstract void init(Models.ParameterSet parameter, Models.Service service);

        public virtual void Stop() { CanExecute = false; }

        protected bool CanExecute = true;

        public int? PercentProgress { get; protected set; }
        public virtual int? TaskCount { get; protected set; }
        public virtual List<long> TaskList { get; }
        protected int MAX_FAILURE { get; set; }

        public StatusCode Run()
        {

            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            if (STATE != ServiceCommonType.ServiceState.Stop)
                return retVal;
            init(parameterSet, service);
            STATE = ServiceCommonType.ServiceState.Running;
            CanExecute = true;
            while (CanExecute)
            {
                retVal = _internalExecute();
                if (CanExecute == false || retVal.IsSucceeded == false)
                {
                    PercentProgress = null;
                    break;
                }
                System.Threading.Thread.Sleep(3000);
                PercentProgress = null;
            }

            STATE = ServiceCommonType.ServiceState.Stop;
            return retVal;
        }
        protected abstract void _uninit();
        protected abstract StatusCode _internalExecute();

        public abstract bool IsCompleted(string parameter);

        protected static object objLock = new object();
        public virtual StatusCode IssueCommand(string parameter) { return StatusCode.SUCCEED_STATUS; }
    }
}
