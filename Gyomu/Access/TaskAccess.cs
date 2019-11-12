using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Gyomu.Access
{
    public class TaskAccess
    {
        public static Common.Task.AbstractBaseTask CreateNewTask(string assembly_name, string full_name, Common.Configurator config)
        {
            return CreateInstance(assembly_name, full_name, config);
        }
        public static Common.Task.AbstractBaseTask CreateNewTask(string assembly_name, string full_name)
        {
            return CreateInstance(assembly_name, full_name, Common.BaseConfigurator.GetInstance());
        }
        public static Common.Task.AbstractBaseTask CreateNewTask(short application_id, short task_id)
        {
            lock (lockObj)
            {
                Models.TaskInfo taskInfo =  Common.GyomuDataAccess.SelectTaskInfo(application_id, task_id);
                return CreateNewTask(taskInfo, Common.BaseConfigurator.GetInstance());
            }

        }

        public static Common.Task.AbstractBaseTask OpenTask(string assembly_name, string full_name, Common.Configurator config, long task_data_id)
        {
            Common.Task.AbstractBaseTask task = CreateInstance(assembly_name, full_name, config);
            if (task != null)
            {
                task.TaskDataID = task_data_id;
            }

            return task;
        }

        public static T CreateNewTask<T>(Common.Configurator config) where T : Common.Task.AbstractBaseTask
        {
            return (T)createInstance(typeof(T), config);
        }
        public static T OpenTask<T>(Common.Configurator config, long task_data_id) where T : Common.Task.AbstractBaseTask
        {

            Common.Task.AbstractBaseTask task = createInstance(typeof(T), config);
            if (task != null)
            {
                task.TaskDataID = task_data_id;
            }
            return (T)task;
        }

        public static List<Common.Task.AbstractBaseTask> ListupDelegatedTasks(short application_id, short task_id, Common.Configurator config)
        {
            List<Models.TaskData> taskDataList = Common.GyomuDataAccess.SelectTaskDataByApplicationIDTaskIDStatusUserList
                (application_id, task_id, Common.Task.AbstractBaseTask.STATUS_DELEGATE,
                new List<string>() { config.User.UserID });
            List<Common.Task.AbstractBaseTask> lstTask = new List<Common.Task.AbstractBaseTask>();
            Models.TaskInfo taskInfo = Common.GyomuDataAccess.SelectTaskInfo(application_id, task_id);

            foreach (Models.TaskData taskData in taskDataList)
            {
                Common.Task.AbstractBaseTask task = OpenTask(taskInfo.assembly_name, taskInfo.class_name, config ?? Common.BaseConfigurator.GetInstance(), taskData.id);
                lstTask.Add(task);
            }
            return lstTask;
        }

        public static bool TaskSucceeded(long task_data_id)
        {
            Models.TaskDataStatus taskStatus= Common.GyomuDataAccess.SelectTaskStatus(new Models.TaskData() { id = task_data_id });
            return taskStatus.task_status.Equals(Common.Task.AbstractBaseTask.STATUS_COMPLETE);

        }

        internal static object lockObj = new object();

        #region internal method
        internal static Common.Task.AbstractBaseTask CreateNewTask(Models.TaskInfo taskInfo, Common.Configurator config)
        {
            return CreateInstance(taskInfo.assembly_name, taskInfo.class_name, config);
        }
        #endregion

        #region private Method
        private static Common.Task.AbstractBaseTask createInstance(Type task_type, Common.Configurator config)
        {
            Common.Task.AbstractBaseTask abstractTask = null;
            try
            {
                Assembly found_assembly = task_type.Assembly;

                abstractTask = (Common.Task.AbstractBaseTask)found_assembly.CreateInstance(task_type.FullName);
            }
            catch (Exception)
            {
                return null;
            }
            if (abstractTask != null)
                abstractTask.Config = config;
            return abstractTask;
        }

        private static Common.Task.AbstractBaseTask CreateInstance(string assembly_name, string full_name, Common.Configurator config)
        {
            Common.Task.AbstractBaseTask abstractTask = null;
            try
            {
                Type found_type = null;
                Assembly found_assembly = null;
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
                abstractTask = (Common.Task.AbstractBaseTask)found_assembly.CreateInstance(full_name);
            }
            catch (Exception)
            {
                return null;
            }
            if (abstractTask != null)
                abstractTask.Config = config;
            return abstractTask;
        }
        #endregion

        #region Data Access
        public static bool IsCompleted(long task_data_id, out DateTime updateTime)
        {
            Models.TaskDataStatus taskStatus = Common.GyomuDataAccess.SelectTaskStatus(new Models.TaskData() { id = task_data_id });
            updateTime = taskStatus.latest_update_date;
            switch (taskStatus.task_status)
            {
                case Common.Task.AbstractBaseTask.STATUS_COMPLETE:
                case Common.Task.AbstractBaseTask.STATUS_FAIL:
                    return true;
                default:
                    return false;
            }
        }        

         #endregion
    }
}
