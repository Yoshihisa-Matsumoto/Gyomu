using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Gyomu.Common.Threading
{
    public class ThreadPoolManager : IDisposable
    {
        public int MaxThreadCount { get; private set; }

        internal short ApplicationId { get; private set; }
        internal int InstanceId { get; private set; }
        internal Configurator Config { get; private set; }
        public ThreadPoolManager(int numMaxThread, short application_id, Configurator config)
        {
            MaxThreadCount = numMaxThread;
            ApplicationId = application_id;
            Config = config;
        }
        private readonly List<StatusCode> lstError = new List<StatusCode>();
        public List<StatusCode> ErrorList
        {
            get
            {
                lock (lstError)
                {
                    List<StatusCode> retVal = new List<StatusCode>();
                    foreach (StatusCode code in lstError)
                    {
                        retVal.Add(code);
                    }
                    return retVal;
                }
            }
        }

        private Queue<ExecutionUnit> ExecutionUnitQueue = new Queue<ExecutionUnit>();

        private readonly List<ThreadPoolItem> lstPoolItem = new List<ThreadPoolItem>();
        internal List<ThreadPoolItem> PoolItemList
        {
            get
            {
                lock (this)
                {
                    return lstPoolItem;
                }
            }
        }

        private readonly List<Thread> lstThread = new List<Thread>();

        private bool isTransitioning = false;

        private StatusCode clearExisting()
        {
            lock (this)
            {
                if (ExecutionUnitQueue.Count > 0 || numRunningItem > 0)
                {
                    object[] args = new object[1];
                    return
                        new CommonStatusCode(CommonStatusCode.POOL_MGR_START_WITHOUT_FINISH, args,Config, ApplicationId);
                }
                lstError.Clear();
                foreach (ThreadPoolItem item in PoolItemList)
                {
                    item.Status = ThreadPoolItem.PoolItemStatus.Stop;
                    item.Trigger();
                }
                PoolItemList.Clear();

                UnitAdded = null;


            }
            return StatusCode.SUCCEED_STATUS;
        }
        public void ClearError()
        {
            lock (this)
            {
                lstError.Clear();
            }
        }

        internal void RegisterError(StatusCode status)
        {
            lstError.Add(status);
            ErrorInserted?.Invoke(this, new EventArgs());
        }

        private event EventHandler ErrorInserted;

        internal event EventHandler<ThreadPoolItemEventArg> UnitAdded;

        public bool Started { get; private set; }

        private Thread TriggerThread = null;
        private Queue<ThreadPoolItem> queueInvokeRequest = new Queue<ThreadPoolItem>();
        private bool isTriggerThreadFinish = false;
        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private void invokeThread()
        {
            while (isTriggerThreadFinish == false)
            {
                resetEvent.WaitOne();
                while (queueInvokeRequest.Count > 0)
                {
                    ThreadPoolItem targetItem = null;
                    lock (queueInvokeRequest)
                    {
                        targetItem = queueInvokeRequest.Dequeue();
                    }
                    if (targetItem != null)
                        targetItem.Trigger();

                }
            }
        }

        public bool Start()
        {
            lock (this)
            {
                if (Started == false)
                {
                    clearExisting();
                    UnitAdded += new EventHandler<ThreadPoolItemEventArg>(threadPoolManager_UnitAdded);
                    for (int i = 0; i < MaxThreadCount; i++)
                    {
                        ThreadPoolItem item = new ThreadPoolItem(this, i);
                        item.UnitStarted += new EventHandler<ThreadPoolItemEventArg>(item_UnitStarted);
                        item.UnitFinished += new EventHandler<ThreadPoolItemEventArg>(item_UnitFinished);
                        PoolItemList.Add(item);
                        Thread newThread = new Thread(new ThreadStart(item.ExecThreadItem));
                        lstThread.Add(newThread);
                        newThread.Start();

                        item.Status = ThreadPoolItem.PoolItemStatus.Active;
                    }
                    TriggerThread = new Thread(new ThreadStart(this.invokeThread));
                    isTriggerThreadFinish = false;
                    TriggerThread.Start();

                    Started = true;
                    return true;
                }
            }

            return false;
        }

        void threadPoolManager_UnitAdded(object sender, ThreadPoolItemEventArg e)
        {
            assignExecutionUnit();
        }

        void item_UnitFinished(object sender, ThreadPoolItemEventArg e)
        {
            lock (this)
            {
                isTransitioning = true;
                lock (dictItemUnit)
                {
                    dictItemUnit.Remove((ThreadPoolItem)sender);
                    numRunningItem = dictItemUnit.Count;
                }
            }
            assignExecutionUnit();
        }

        void item_UnitStarted(object sender, ThreadPoolItemEventArg e)
        {
            lock (this)
            {
                isTransitioning = false;
                lock (dictItemUnit)
                {
                    dictItemUnit.Add((ThreadPoolItem)sender, e.Unit);
                    numRunningItem = dictItemUnit.Count;
                }
            }
        }

        Dictionary<ThreadPoolItem, ExecutionUnit> dictItemUnit = new Dictionary<ThreadPoolItem, ExecutionUnit>();
        private int numRunningItem = 0;

        public bool IsAvailable { get { return AvailablePoolItem == null ? false : true; } }

        private ThreadPoolItem AvailablePoolItem
        {
            get
            {
                if (numRunningItem == MaxThreadCount)
                    return null;

                foreach (ThreadPoolItem item in PoolItemList)
                {
                    lock (item)
                    {
                        if (item.Status == ThreadPoolItem.PoolItemStatus.Active)
                            return item;
                    }
                }
                return null;
            }
        }

        //Should be Synchronized
        public StatusCode AddExecutionUnit(ExecutionUnit unit)
        {
            lock (ExecutionUnitQueue)
            {
                ExecutionUnitQueue.Enqueue(unit);
            }
            UnitAdded?.Invoke(this, new ThreadPoolItemEventArg(unit));
            return StatusCode.SUCCEED_STATUS;
        }

        //Should be Synchronized
        public StatusCode AddExecutionUnitIfAvailable(ExecutionUnit unit, bool ReturnIfErrorFound)
        {
            object[] args = new object[1];
            if (ReturnIfErrorFound && ErrorList.Count > 0)
            {
                args[0] = ErrorList.Count;
                return
                    new CommonStatusCode(CommonStatusCode.QUEUE_ERROR_FOUND, args, Config,ApplicationId);
            }
            ThreadPoolItem nextItem = null;
            while ((nextItem = AvailablePoolItem) == null)
            {
                System.Threading.Thread.Sleep(0);

            }
            if (ReturnIfErrorFound && ErrorList.Count > 0)
            {
                args[0] = ErrorList.Count;
                return
                    new CommonStatusCode(CommonStatusCode.QUEUE_ERROR_FOUND, args, Config,ApplicationId);
            }
            StatusCode retVal = AddExecutionUnit(unit);

            return retVal;
        }

        private void assignExecutionUnit()
        {
            ThreadPoolItem pool_item = null;
            lock (this)
            {
                if (ExecutionUnitQueue.Count > 0)
                {
                    pool_item = AvailablePoolItem;
                    if (pool_item != null)
                    {
                        lock (pool_item)
                        {
                            if (pool_item.ExecutionUnit == null && pool_item.IsAvailable)
                            {
                                lock (ExecutionUnitQueue)
                                {
                                    ExecutionUnit targetUnit = ExecutionUnitQueue.Dequeue();
                                    if (targetUnit != null)
                                        pool_item.ExecutionUnit = targetUnit;
                                }
                            }
                        }
                    }
                }
            }
            if (pool_item != null && pool_item.Status == ThreadPoolItem.PoolItemStatus.UnitAttached)
            {
                lock (queueInvokeRequest)
                    queueInvokeRequest.Enqueue(pool_item);
                resetEvent.Set();
            }
            else
            {
                isTransitioning = false;
            }
        }

        //Should be Synchronized
        public StatusCode WaitForAllFinish()
        {
            while (isTransitioning || ExecutionUnitQueue.Count > 0)
            {
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(0);
            }
            System.Threading.Thread.Sleep(100);
            while (isTransitioning || numRunningItem > 0)
            {
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(0);
            }
            if (isTransitioning || ExecutionUnitQueue.Count > 0)
                return WaitForAllFinish();
            return StatusCode.SUCCEED_STATUS;
        }


        public StatusCode Stop()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            if (Started)
            {
                retVal = WaitForAllFinish();
                if (retVal != StatusCode.SUCCEED_STATUS)
                    return retVal;

                clearExisting();
                foreach (Thread th in lstThread)
                {
                    if (th.IsAlive)
                        th.Abort();
                }
                TriggerThread.Abort();
                isTriggerThreadFinish = true;
                Started = false;
            }
            return retVal;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
