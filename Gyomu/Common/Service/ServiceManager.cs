using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Gyomu.Common.Service
{
    public class ServiceManager
    {
        Dictionary<IService, Task> dictThreadService = new Dictionary<IService, Task>();
        private static List<IService> lstService = new List<IService>();

        public Configurator Config { get; private set; }
        public short ApplicationID { get; private set; }
        public ServiceManager(Configurator config,short applicationId) { Config = config;ApplicationID = applicationId; }
        private ServiceManager() { }

        private bool CanInitialize
        {
            get
            {
                if (lstService.Count == 0)
                    return true;
                else
                    return false;
            }
        }
        public StatusCode Init(string param_key)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            lock (this)
            {
                if (CanInitialize == false)
                    return retVal;

                string strValue = Access.ParameterAccess.GetStringValue(param_key);
                StatusCode.Debug("KEY:" + param_key + " VAL:" + strValue, Config);
                List<short> lstServerToActivate = new List<short>();
                if (string.IsNullOrEmpty(strValue) == false)
                    lstServerToActivate = Newtonsoft.Json.JsonConvert.DeserializeObject<List<short>>(strValue);

                List<Models.ServiceType> lstType = GyomuDataAccess.GetAllServiceType();
                List<Models.Service> lstServerService = GyomuDataAccess.GetAllService();

                retVal = internalInit(lstServerService, lstType);
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            return retVal;
        }
        public StatusCode Init(short[] serviceIds)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            lock (this)
            {
                if (CanInitialize == false)
                    return retVal;

                //Data.CommonData commonData = new Data.CommonData();
                //commonData.LoadServer(serviceIds);
                List<Models.ServiceType> lstType = GyomuDataAccess.GetAllServiceType();
                List<Models.Service> lstService = GyomuDataAccess.GetAllService().Where(s => serviceIds.Contains(s.id)).ToList();

                retVal = internalInit(lstService, lstType);
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            return retVal;
        }

        private StatusCode internalInit(List<Models.Service> lstService, List<Models.ServiceType> lstType)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            //foreach (Data.CommonData.server_service_cdtblRow r in commonData.server_service_cdtbl.Rows)
            foreach (Models.Service service in lstService)
            {
                try
                {
                    Type found_type = null;
                    Assembly found_assembly = null;
                    Models.ServiceType serviceType = lstType.Where(st => st.id == service.service_type_id).FirstOrDefault();
                    string assembly_name = serviceType.assembly_name;
                    string full_name = serviceType.class_name;
                    if (string.IsNullOrEmpty(assembly_name) == false)
                    {
                        try
                        {
                            found_assembly = AppDomain.CurrentDomain.Load(assembly_name);
                        }
                        catch (Exception) { }
                    }
                    if (found_assembly == null)
                    {
                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            found_assembly = assembly;
                            found_type = assembly.GetType(full_name);
                            if (found_type != null)
                                break;
                        }
                    }

                    IService svc = (IService)found_assembly.CreateInstance(full_name);
                    svc.Init(Config, Access.JsonAccess.Deserialize<Models.ParameterSet>(service.parameter), service);
                    ServiceManager.lstService.Add(svc);


                }
                catch (Exception ex)
                {
                    retVal = new CommonStatusCode(CommonStatusCode.SERVICE_LOAD_ERROR, new object[] { service.id }, ex, Config,ApplicationID);
                }
            }

            return retVal;
        }
        public List<long> GetTaskList(short id)
        {
            lock (this)
            {
                IService svc = lstService.Find(s => s.ID == id);
                if (svc != null)
                {
                    return svc.TaskList;
                }
                else
                    return null;
            }
        }
        public StatusCode Start(short id)
        {
            lock (this)
            {
                IService svc = lstService.Find(s => s.ID == id);
                if (svc != null)
                {
                    if (dictThreadService.ContainsKey(svc) == false)
                    {
                        Task t = Task<StatusCode>.Factory.StartNew(svc.Run);
                        dictThreadService.Add(svc, t);
                    }
                    else
                    {
                        Task t = dictThreadService[svc];
                        if (t.Status != TaskStatus.Running)
                        {
                            dictThreadService.Remove(svc);
                            t = Task<StatusCode>.Factory.StartNew(svc.Run);
                            dictThreadService.Add(svc, t);
                        }
                    }
                }
                else
                {
                    return new CommonStatusCode(CommonStatusCode.SERVICE_NOT_FOUND, new object[] { id }, Config,ApplicationID);
                }
            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode StartAll()
        {
            lock (this)
            {
                foreach (IService svc in lstService)
                {
                    bool canStart = true;
                    switch (svc.STATE)
                    {
                        case ServiceCommonType.ServiceState.Stop:
                            break;
                        default:
                            canStart = false;
                            break;
                    }

                    if (canStart)
                    {
                        Task t = Task<StatusCode>.Factory.StartNew(svc.Run);
                        dictThreadService.Add(svc, t);
                    }
                }
            }
            return StatusCode.SUCCEED_STATUS;

        }
        public StatusCode Stop(short id)
        {
            lock (this)
            {
                IService svc = lstService.Find(s => s.ID == id);
                if (svc != null)
                {
                    switch (svc.STATE)
                    {
                        case ServiceCommonType.ServiceState.Executing:
                        case ServiceCommonType.ServiceState.Running:
                            svc.Stop();
                            dictThreadService.Remove(svc);
                            break;
                    }

                }
                else
                {
                    return new CommonStatusCode(CommonStatusCode.SERVICE_NOT_FOUND, new object[] { id }, Config,ApplicationID);
                }
            }
            return StatusCode.SUCCEED_STATUS;
        }

        public StatusCode StopAll()
        {
            lock (this)
            {
                foreach (IService svc in lstService)
                {
                    bool canStop = true;
                    switch (svc.STATE)
                    {
                        case ServiceCommonType.ServiceState.Executing:
                        case ServiceCommonType.ServiceState.Running:
                            break;
                        default:
                            canStop = false;
                            break;
                    }

                    if (canStop)
                    {
                        svc.Stop();
                    }
                }
            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode IssueCommand(short id, string parameter)
        {
            lock (this)
            {
                IService svc = lstService.Find(s => s.ID == id);
                if (svc != null)
                {
                    return svc.IssueCommand(parameter);
                }
            }
            return StatusCode.SUCCEED_STATUS;
        }
        public List<Models.ServiceStatus> CurrentStatus
        {
            get
            {
                List<Models.ServiceStatus> statusSet = new List<Models.ServiceStatus>();
                foreach (IService svc in lstService)
                {
                    Models.ServiceStatus status = new Models.ServiceStatus();
                    status.ID = svc.ID;
                    status.Description = svc.Description;
                    status.Parameter = svc.Parameter;
                    status.Status = svc.STATE.ToString();
                    status.CurrentTask = svc.CurrentTask;
                    if (svc.PercentProgress == null)
                        status.Progress = null;
                    else
                    {
                        status.Progress = svc.PercentProgress.Value;
                    }
                    if (svc.TaskCount == null)
                        status.TaskCount = null;
                    else
                    {
                        status.TaskCount = svc.TaskCount.Value;
                    }
                    statusSet.Add(status);
                }
                return statusSet;
            }
        }

        public StatusCode InitializeAgain(short[] serviceIds)
        {
            lock (this)
            {
                if (lstService.Count(s => s.STATE != ServiceCommonType.ServiceState.Stop) > 0)
                    return StatusCode.SUCCEED_STATUS;
                lstService.Clear();
                dictThreadService.Clear();
                return Init(serviceIds);
            }
        }
        public void Uninitialize()
        {
            while (true)
            {
                StopAll();
                if (lstService.Count(s => s.STATE != ServiceCommonType.ServiceState.Stop) == 0)
                    break;
                System.Threading.Thread.Sleep(1000);
            }
            lstService.Clear();
        }

        public bool IsCompleted(short id, string parameter)
        {
            lock (this)
            {
                IService svc = lstService.Find(s => s.ID == id);
                if (svc != null)
                {

                    return svc.IsCompleted(parameter);
                }
                else
                {
                    new CommonStatusCode(CommonStatusCode.SERVICE_NOT_FOUND, new object[] { id }, Config,ApplicationID);
                }
            }
            return false;
        }
        public void CheckServiceThreadState()
        {
            foreach (IService svc in dictThreadService.Keys)
            {
                Task t = dictThreadService[svc];
                if (t.Exception != null)
                {
                    StatusCode.Debug(svc.Description + " has Error \n" + t.Exception.ToString(), Config);
                }
            }
        }
    }
}
