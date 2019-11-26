using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Threading
{
    public abstract class AbstractExecutionUnit : ExecutionUnit
    {
        #region ExecutionUnit Members

        public virtual StatusCode OnStart()
        {
            TaskStart?.Invoke(this, new EventArgs());
            return StatusCode.SUCCEED_STATUS;
        }

        public StatusCode OnExec()
        {
            Activate();
            StatusCode retVal = UnitExec();
            Deactivate();
            return retVal;
        }

        public virtual StatusCode OnEnd()
        {
            TaskEnd?.Invoke(this, new EventArgs());
            return StatusCode.SUCCEED_STATUS;
        }

        public virtual StatusCode OnError()
        {
            TaskError?.Invoke(this, new EventArgs());
            Deactivate();
            return StatusCode.SUCCEED_STATUS;
        }

        public bool IsRunning
        {
            get;private set;
        }

        #endregion

        protected void Activate()
        {
            IsRunning = true;
        }
        protected void Deactivate()
        {
            IsRunning = false;
        }

        public event EventHandler TaskEnd;
        public event EventHandler TaskStart;
        public event EventHandler TaskError;

        protected abstract StatusCode UnitExec();
    }
}
