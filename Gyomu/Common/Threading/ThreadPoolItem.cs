using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Gyomu.Common.Threading
{
    internal class ThreadPoolItem
    {
        private ThreadPoolManager Manager { get;  set; }
        private ThreadPoolItem()
        {
        }

        private int ItemId;
        private object[] arg = new object[1];

        internal ThreadPoolItem(ThreadPoolManager mgr, int id)
        {
            Manager = mgr;
            ItemId = id;
            arg[0] = id;
        }
        internal event EventHandler<ThreadPoolItemEventArg> UnitStarted;
        internal event EventHandler<ThreadPoolItemEventArg> UnitFinished;

        System.Threading.AutoResetEvent auto_event = new AutoResetEvent(false);
        private bool isReadyForEvent = false;
        internal void Trigger()
        {
            while (isReadyForEvent == false)
                System.Threading.Thread.Sleep(0);
            auto_event.Set();
        }

        private void ErrorProc(StatusCode retVal)
        {
            StatusCode retVal2 = StatusCode.SUCCEED_STATUS;
            try
            {
                retVal2 = ExecutionUnit.OnError();

            }
            catch (Exception ex)
            {
                retVal2 =
                new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_ERROR, ex,Manager.Config, Manager.ApplicationId);

            }
            try
            {
                Manager.RegisterError(retVal);
            }
            catch (Exception ex)
            {
                new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_MANAGER_ERROR, ex,Manager.Config, Manager.ApplicationId);
            }

            if (retVal2 != StatusCode.SUCCEED_STATUS)
            {
                try
                {
                    Manager.RegisterError(retVal2);
                }
                catch (Exception ex)
                {
                    new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_MANAGER_ERROR, ex,Manager.Config, Manager.ApplicationId);
                }
            }
        }
        internal void ExecThreadItem()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            PoolItemStatus current_status = Status;
            while (current_status == PoolItemStatus.Active || current_status == PoolItemStatus.UnitAttached)
            {
                //System.Threading.Thread.Sleep(0);
                isReadyForEvent = true;
                auto_event.WaitOne();
                isReadyForEvent = false;
                if (ExecutionUnit != null)
                {
                    retVal = StatusCode.SUCCEED_STATUS;
                    ThreadPoolItemEventArg eventArg = new ThreadPoolItemEventArg(ExecutionUnit);

                    Status = PoolItemStatus.Running;
                    //new CommonStatusCode(CommonStatusCode.ITEM_STARTED, arg, Manager.Config, Manager.ApplicationId);
                    if (UnitStarted != null)
                        UnitStarted(this, eventArg);
                    try
                    {
                        retVal = ExecutionUnit.OnStart();
                    }
                    catch (Exception ex)
                    {
                        retVal =
                            new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_ERROR, ex,Manager.Config, Manager.ApplicationId);
                    }

                    if (retVal != StatusCode.SUCCEED_STATUS)
                    {
                        ErrorProc(retVal);

                    }
                    else
                    {
                        try
                        {
                            retVal = ExecutionUnit.OnExec();
                        }
                        catch (Exception ex)
                        {
                            retVal =
                            new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_ERROR, ex,Manager.Config, Manager.ApplicationId);
                        }
                        if (retVal != StatusCode.SUCCEED_STATUS)
                        {
                            ErrorProc(retVal);
                        }
                        else
                        {
                            try
                            {
                                retVal = ExecutionUnit.OnEnd();
                            }
                            catch (Exception ex)
                            {
                                retVal =
                                new CommonStatusCode(CommonStatusCode.UNKNOWN_THREAD_ERROR, ex,Manager.Config, Manager.ApplicationId);
                            }
                            if (retVal != StatusCode.SUCCEED_STATUS)
                            {
                                ErrorProc(retVal);
                            }
                        }
                    }
                    //new CommonStatusCode(CommonStatusCode.ITEM_FINISHED, arg, Manager.Config, Manager.ApplicationId);
                    ExecutionUnit = null;
                    Status = PoolItemStatus.Active;

                    if (UnitFinished != null)
                        UnitFinished(this, eventArg);


                }
                else
                {
                    //object[] args= new object[2];
                    //args[0] = ItemId;
                    //args[1] = current_status;
                    //new CommonStatusCode(CommonStatusCode.ITEM_TRIGGERED_WITH_INVALID_STATUS, args, Manager.Config, Manager.ApplicationId);
                }
                current_status = Status;
                System.Threading.Thread.Sleep(0);
            }
        }

        internal enum PoolItemStatus
        {
            Active,
            UnitAttached,
            Running,
            Stopping,
            Stop
        };

        private PoolItemStatus _status = PoolItemStatus.Active;
        internal bool IsAvailable
        {
            get { return Status == PoolItemStatus.Active; }
        }
        internal PoolItemStatus Status
        {
            get
            {
                lock (this)
                {
                    return _status;
                }
            }
            set
            {
                lock (this)
                {
                    _status = value;
                }
            }
        }

        private ExecutionUnit _unit = null;
        internal ExecutionUnit ExecutionUnit
        {
            get
            {
                lock (this)
                {
                    return _unit;
                }
            }
            set
            {
                lock (this)
                {
                    if (value != null)
                    {
                        Status = PoolItemStatus.UnitAttached;
                        //new CommonStatusCode(CommonStatusCode.ITEM_ATTACHED, arg, Manager.Config, Manager.ApplicationId);
                    }
                    _unit = value;

                }
            }
        }

        internal void Deactivate()
        {
            Status = PoolItemStatus.Stopping;
        }
    }
}
