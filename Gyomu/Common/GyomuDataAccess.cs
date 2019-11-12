using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;
using Dapper;
using System.Linq;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using System.Reflection;

namespace Gyomu.Common
{
    internal class GyomuDataAccess:BaseDapperAccess
    {
        static GyomuDataAccess()
        {

        }
        private enum Table
        {
            ApplicationInfo,
            ParameterMaster,
            StatusHandler,
            StatusInfo,
            MarketHoliday,
            MilestoneDaily,
            VariableParameter,
            TaskInfo,
            TaskAccessList,
            TaskData,
            TaskInstance,
            TaskDataStatus,
            TaskDataLog,
            TaskSubmitInformation
        }
        private enum TaskDataQuery
        {
            SelectByApplicationIDTaskIDStartDateEndDate,
            SelectByApplicationIDTaskIDStatusIDUserList,
            SelectOpenTaskByApplicationIDDateRange,
            SelectOpenTaskByApplicationIDEntryAuthorDateRange,
        }
        private enum TaskInstanceQuery
        {
            SelectByTaskDataID
        }
        private static string SELECT { get { return "SELECT"; } }
        private static string INSERT { get { return "INSERT"; } }
        private static string UPDATE { get { return "UPDATE"; } }
        private static string DELETE { get { return "DELETE"; } }
        private static string LOCK { get { return "LOCK"; } }
        private static Dictionary<Common.SettingItem.DBType, Dictionary<Table, Dictionary<string, string>>> 
            PerDBTableTypeSQLQuery = new Dictionary<Common.SettingItem.DBType, Dictionary<Table, Dictionary<string, string>>>()
        {
            {SettingItem.DBType.POSTGRESQL,
                new Dictionary<Table,Dictionary<string, string>>()
                {
                    {Table.ParameterMaster,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * from param_master WHERE item_key=@key" },
                            {DELETE,"DELETE from param_master WHERE item_key=@key AND item_fromdate=''" },
                            {UPDATE,"UPDATE param_master SET item_value=@value WHERE item_key=@key AND item_fromdate=''" },
                            {INSERT,"INSERT INTO param_master (item_key,item_value,item_fromdate) VALUES(@key,@value,'')" },
                            {LOCK,"SELECT * from param_master WHERE item_key=@key FOR UPDATE" }
                        }
                    },
                    {Table.StatusHandler,
                        new Dictionary<string, string>()
                        {
                             {"SelectByApplication","SELECT * from status_handler WHERE application_id=@application_id" }

                        }
                    },
                    {Table.StatusInfo,
                        new Dictionary<string, string>()
                        {
                            {"SelectByApplication","SELECT * from status_info WHERE application_id=@application_id" }
                        }
                    },
                    {Table.MarketHoliday,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT holiday from market_holiday WHERE market=@market" },
                            {DELETE,"DELETE from market_holiday WHERE market=@market AND year=@year" },
                            {INSERT,"INSERT INTO market_holiday VALUES(@market,@year,@holiday)" }
                        }
                    },
                    {Table.MilestoneDaily,
                        new Dictionary<string, string>()
                        {
                            {"SelectByTargetDate","SELECT * from milestone_daily WHERE target_date=@target_date" },
                            {INSERT,"INSERT INTO milestone_daily (target_date,milestone_id) VALUES(@target_date,@milestone_id)" },
                            {DELETE,"DELETE FROM milestone_daily WHERE target_date=@target_date AND milestone_id=@milestone_id" },
                            {SELECT,"SELECT * from milestone_daily WHERE target_date=@target_date AND milestone_id=@milestone_id" },
                        }
                    },
                    {Table.VariableParameter,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * from variable_parameter" }
                        }
                    },
                    {Table.TaskInfo,
                        new Dictionary<string, string>()
                        {
                            {INSERT,"INSERT INTO task_info_cdtbl (application_id, task_id, description, assembly_name, class_name, restartable) VALUES (@application_id,@task_id,@description,@assembly_name,@class_name,@restartable)" },
                            {SELECT,"SELECT * from task_info_cdtbl WHERE application_id=@application_id AND task_id=@task_id" },
                            {"SelectAll","SELECT * from task_info_cdtbl" },
                            {DELETE,"DELETE from task_info_cdtbl WHERE application_id=@application_id AND task_id=@task_id" },
                            {"SelectByApplication","SELECT * from task_info_cdtbl WHERE application_id=@application_id" },
                            {"SelectByAssembly","SELECT * from task_info_cdtbl WHERE assembly_name=@assembly_name" }
                        }

                    },
                    {Table.TaskAccessList,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * from task_info_access_list WHERE application_id=@application_id AND task_info_id=@task_id" }
                        }
                    },
                    {Table.TaskData,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * from task_data WHERE application_id=@application_id AND task_id=@task_id" },
                            {LOCK,"SELECT task_data.* FROM task_data WHERE id=@id FOR UPDATE" },
                            {enumDescription(TaskDataQuery.SelectByApplicationIDTaskIDStartDateEndDate),
                                "SELECT task_data.* FROM task_data WHERE application_id=@application_id and task_info_id=@task_id AND entry_date >=@startDate and entry_date<@endDate" },
                            {enumDescription(TaskDataQuery.SelectByApplicationIDTaskIDStatusIDUserList),
                                "SELECT td.* FROM  task_data td WITH(NOLOCK) INNER JOIN dbo.task_data_status tds WITH(NOLOCK) ON td.id=tds.task_data_id INNER JOIN dbo.task_instance_submit_information tis WITH(NOLOCK) ON tds.latest_task_instance_id=tis.task_instance_id WHERE application_id=@application_id AND task_info_id=@task_info_id AND tds.task_status=@status AND submit_to IN @userList) ORDER BY tds.latest_task_instance_id"},
                            //{enumDescription(TaskDataQuery.SelectOpenTaskByApplicationIDDateRange),
                            //    "SELECT  task_data.* FROM task_data INNER JOIN task_data_status ON task_data.id = task_data_status.task_data_id WHERE task_data.application_id=@application_id AND task_status_id NOT IN ('COMPLETE','FAIL','NOTEXEC') AND (task_data.entry_date >= @startDate AND task_data_status.latest_update_date <= @endDate) ORDER BY task_data.entry_date DESC" },
                            //{enumDescription(TaskDataQuery.SelectOpenTaskByApplicationIDEntryAuthorDateRange),
                            //    "SELECT task_data.* FROM task_data INNER JOIN task_data_status ON task_data.id = task_data_status.task_data_id WHERE task_data.application_id=@application_id AND  (task_data.entry_author = @entry_author) AND task_status_id NOT IN ('COMPLETE','FAIL','NOTEXEC') AND (task_data.entry_date >= @startDate AND task_data_status.latest_update_date <= @endDate) ORDER BY dbo.task_data.entry_date DESC" },
                        }
                    },
                    {Table.TaskInstance,
                        new Dictionary<string, string>()
                        {
                            {enumDescription(TaskInstanceQuery.SelectByTaskDataID),
                                "SELECT * FROM task_instance WHERE task_data_id=@task_data_id ORDER BY entry_date DESC" },
                        }
                    },
                    {Table.TaskDataLog,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * FROM task_data_log WHERE task_data_id=@task_data_id " }
                        }
                    },
                    {Table.TaskSubmitInformation,
                        new Dictionary<string, string>()
                        {
                            {SELECT,"SELECT * FROM task_instance_submit_information WHERE task_instance_id=@task_instance_id" }
                        }
                    }
                }
            },
            {SettingItem.DBType.MSSQL,
                new Dictionary<Table,Dictionary<string, string>>()
                {
                    {Table.ParameterMaster,
                        new Dictionary<string, string>()
                        {
                             {LOCK,"SELECT * from param_master WITH(UPDLOCK) WHERE item_key=@key" }
                        }
                    },
                    {Table.TaskData,
                        new Dictionary<string, string>()
                        {
                            {LOCK,"SELECT task_data.* FROM task_data WITH(UPDLOCK) WHERE id=@id" },
                        }
                    }
                }
            }
        };
        //private static Dictionary<Table, Dictionary<string, string>> PerTableTypeSQLQuery = PerDBTableTypeSQLQuery[DBConnectionFactory.SQLDB];
        private static string getQuery(Table tbl, string key)
        {
            Dictionary<Table, Dictionary<string, string>> PerTableTypeSQLQuery = PerDBTableTypeSQLQuery.ContainsKey(DBConnectionFactory.SQLDB) ? PerDBTableTypeSQLQuery[DBConnectionFactory.SQLDB] : PerDBTableTypeSQLQuery[SettingItem.DBType.POSTGRESQL];
            Dictionary<string, string> dictKeyQuery = PerTableTypeSQLQuery.ContainsKey(tbl) ? PerTableTypeSQLQuery[tbl] : PerDBTableTypeSQLQuery[SettingItem.DBType.POSTGRESQL][tbl];
            string query = dictKeyQuery.ContainsKey(key) ? dictKeyQuery[key] : PerDBTableTypeSQLQuery[SettingItem.DBType.POSTGRESQL][tbl][key];
            return query;
        }
        private static string enumDescription(object enumValue) { return Access.EnumAccess.GetEnumValueDescription(enumValue); }
        private static string getQueryByEnum(Table tbl, object enumValue)
        {
            if (enumValue.GetType().IsEnum == false)
                throw new InvalidOperationException("argument is not enum");
            return getQuery(tbl, enumDescription(enumValue));
        
        }
        #region param_master
        public static void InsertParameter(string key,string strValue)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.ParameterMaster,INSERT), new { key, value = strValue });
            }
        }
        public static void DeleteParameter(string key)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.ParameterMaster,DELETE) , new { key });
            }
        }
        public static void UpdateParameter(string key, string strValue)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.ParameterMaster,UPDATE), new { value = strValue, key });
            }
        }
        public static Models.ParameterMaster LoadParameter(string key)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                Models.ParameterMaster[] parameters = conn.Query<Models.ParameterMaster>(getQuery(Table.ParameterMaster,SELECT), new { key }).ToArray();
                return parameters?.Length > 0 ? parameters[0]:null;
            }
        }
        public static T LockParameterProcess<T>(string key, Func<T> method)
        {

            T result = default(T);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
                {
                    conn.Query<Models.ParameterMaster>(getQuery(Table.ParameterMaster,LOCK), new { key });



                    //Call Function
                    using (TransactionScope scope2 = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        result = method();
                    }
                }
            }
            return result;
        }
        #endregion
        #region apps_info_cdtbl
        public static void InsertApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.ApplicationInfo>(applicationInfo);
                //conn.Execute(PerTableTypeSQLQuery[Table.ApplicationInfo][INSERT], 
                //    new { id= applicationInfo.application_id,
                //        description= applicationInfo.description,
                //        mail_from_address = applicationInfo.mail_from_address,
                //        mail_from_name= applicationInfo.mail_from_name
                //    });
            }
        }
        public static void DeleteApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.ApplicationInfo>(applicationInfo);
                //conn.Execute(PerTableTypeSQLQuery[Table.ApplicationInfo][DELETE],
                //    new
                //    {
                //        id = application_id
                //    });
            }
        }
        public static void UpdateApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.ApplicationInfo>(applicationInfo);
                //conn.Execute(PerTableTypeSQLQuery[Table.ApplicationInfo][UPDATE],
                //    new
                //    {
                //        id= applicationInfo.application_id,
                //        description = applicationInfo.description,
                //        mail_from_address = applicationInfo.mail_from_address,
                //        mail_from_name = applicationInfo.mail_from_name
                //    });
            }
        }
        public static Models.ApplicationInfo SelectApplicationInfo(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.ApplicationInfo>(application_id);
            }
        }
        #endregion
        #region status_handler
        public static int InsertStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id=conn.Insert<Models.StatusHandler>(statusHandler);
                statusHandler.id = Convert.ToInt32(id);
                return statusHandler.id;    
            }
        }
        public static void DeleteStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.StatusHandler>(statusHandler);
            }
        }
        public static void UpdateStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.StatusHandler>(statusHandler);
            }
        }
        public static Models.StatusHandler SelectStatusHandler(int id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.StatusHandler>(id);
            }
        }
        public static List<Models.StatusHandler> SelectStatusHandlers(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.StatusHandler>(getQuery(Table.StatusHandler,"SelectByApplication"),
                    new { application_id }
                    ).ToList();
            }
        }
        #endregion
        #region status_info
        public static long InsertStatusInfo(Models.StatusInfo statusInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id = conn.Insert<Models.StatusInfo>(statusInfo);
                statusInfo.id = id;
                return statusInfo.id;
            }
        }
        internal static void DeleteStatusInfo(Models.StatusInfo statusInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.StatusInfo>(statusInfo);
            }
        }
        public static Models.StatusInfo SelectStatusInfo(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.StatusInfo>(id);
            }
        }
        public static List<Models.StatusInfo> SelectStatusInfosByApplicationID(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.StatusInfo>(getQuery(Table.StatusInfo, "SelectByApplication"),
                    new { application_id }
                    ).ToList();
            }
        }
        #endregion
        #region market_holiday
        public static void InsertHoliday(string market, short year,DateTime holiday)
        {
            if (holiday.Year != year)
                throw new InvalidOperationException("Holiday and Year is different year");
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.MarketHoliday, INSERT), new { market, year, holiday=holiday.ToString("yyyyMMdd")});
            }
        }
        public static void InsertHoliday(string market, short year, List<DateTime> holidays)
        {
            foreach (DateTime holiday in holidays)
                if (holiday.Year != year)
                    throw new InvalidOperationException("Holiday and Year is different year");
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                foreach(DateTime holiday in holidays)
                    conn.Execute(getQuery(Table.MarketHoliday, INSERT), new { market, year, holiday = holiday.ToString("yyyyMMdd") });
            }
        }
        public static void DeleteHoliday(string market,short year)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.MarketHoliday, DELETE), new { market, year });
            }
        }
        public static List<DateTime> ListHoliday(string market)
        {
            List<DateTime> holidays = new List<DateTime>();
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                foreach (string holidayYYYYMMDD in conn.Query<string>(getQuery(Table.MarketHoliday, SELECT), new { market }))
                    holidays.Add(DateTime.ParseExact(holidayYYYYMMDD, "yyyyMMdd", null));

            }
            return holidays;
        }
        #endregion
        #region milestone_daily
        public static void InsertMilestoneDaily(DateTime targetDate, string milestoneId, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.MilestoneDaily, INSERT), new { target_date = targetDateYYYYMMDD, milestone_id=milestoneId });
            }
        }
        public static void DeleteMilestoneDaily(DateTime targetDate, string milestoneId, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.MilestoneDaily, DELETE), new { target_date = targetDateYYYYMMDD, milestone_id = milestoneId });
            }
        }
        public static List<Models.MilestoneDaily> SelectMilestoneDaily(DateTime targetDate, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.MilestoneDaily>(getQuery(Table.MilestoneDaily, "SelectByTargetDate"), new { target_date = targetDateYYYYMMDD }).ToList();
            }
        }
        public static Models.MilestoneDaily SelectMilestoneDaily(DateTime targetDate, string milestoneId,bool isMonthly=false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.QueryFirstOrDefault<Models.MilestoneDaily>(getQuery(Table.MilestoneDaily, SELECT), new { target_date = targetDateYYYYMMDD, milestone_id = milestoneId });
            }
        }
        #endregion
        #region variable_parameter
        public static List<Models.VariableParameter> GetVariableParameters()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.VariableParameter>(getQuery(Table.VariableParameter, SELECT)).ToList();
            }
        }
        #endregion
        #region task_info_cdtbl
        public static void InsertTaskInfo(short  applicationId, short taskId, string description, string assembyName,string className,bool canRestart)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.TaskInfo, INSERT), new { application_id=applicationId, task_id=taskId, description=description, assembly_name =assembyName, class_name=className, restartable=canRestart });
            }
        }
        public static void DeleteTaskInfo(short applicationId, short taskId)
        {

            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Execute(getQuery(Table.TaskInfo, DELETE), new { application_id = applicationId, task_id = taskId });
            }
        }
        public static Models.TaskInfo SelectTaskInfo(short applicationId, short taskId)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, SELECT), new { application_id = applicationId, task_id = taskId }).FirstOrDefault();
            }
        }
        public static List<Models.TaskInfo> SelectAllTaskInfo()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectAll")).ToList();
            }
        }
        public static List<Models.TaskInfo> SelectTaskInfoByApplication(short applicationId)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectByApplication"), new { application_id = applicationId }).ToList();
            }
        }
        public static List<Models.TaskInfo> SelectTaskInfoByAssembly(string assemblyName)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectByAssembly"), new { assembly_name = assemblyName }).ToList();
            }
        }
        #endregion
        #region task_info_access_list
        public static void InsertTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id = conn.Insert<Models.TaskAccessList>(taskAccess);
                taskAccess.id = id;
            }
        }
        public static void DeleteTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskAccessList>(taskAccess);
            }
        }
        public static void UpdateTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.TaskAccessList>(taskAccess);
            }
        }
        public static Models.TaskAccessList SelectTaskAccessList(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskAccessList>(id);
            }
        }
        public static List<Models.TaskAccessList> SelectTaskAccessLists(short application_id,short task_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskAccessList>(getQuery(Table.TaskAccessList, SELECT),
                    new { application_id =application_id,task_id=task_id}
                    ).ToList();
            }
        }
        #endregion
        #region task_data
        internal static void InsertTaskData(ref Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id = conn.Insert<Models.TaskData>(taskData);
                taskData = conn.Get<Models.TaskData>(id);
            }
        }
        internal static void CreateNewTask(
            Models.TaskInfo taskInformation,Configurator Config,
            string parameter,string comment,
            Models.TaskData parentTask,
            out Models.TaskData taskData,
            out Models.TaskInstance taskInstance,
            out Models.TaskDataStatus taskStatus)
        {
            using (System.Transactions.TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                taskData = new Models.TaskData()
                {
                    application_id = taskInformation.application_id,
                    task_info_id = taskInformation.task_id,
                    entry_author = Config.Username,
                    parameter = parameter,
                    parent_task_data_id = parentTask==null?(long?)null:parentTask.id
                };
                InsertTaskData(ref taskData);
                taskInstance = new Models.TaskInstance()
                {
                    task_data_id = taskData.id,
                    entry_date = taskData.entry_date,
                    entry_author = taskData.entry_author,
                    task_status = "INIT",
                    is_done = false,
                    parameter = parameter,
                    comment = comment
                };
                InsertTaskInstance(ref taskInstance);
                taskStatus = new Models.TaskDataStatus()
                {
                    task_data_id = taskData.id,
                    latest_task_instance_id = taskInstance.id,
                    latest_update_date = taskInstance.entry_date,
                    task_status = taskInstance.task_status
                };
                InsertTaskStatus(taskStatus);
                scope.Complete();
            }
        }
        internal static Models.TaskData LockTaskData(long taskDataID)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.QueryFirst<Models.TaskData>(getQuery(Table.TaskData, LOCK),
                    new { id = taskDataID });
            }
        }
        internal static void DeleteTaskData(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskData>(taskData);
            }
        }
        
        public static Models.TaskData SelectTaskData(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskData>(id);
            }
        }
        public static List<Models.TaskData> SelectTaskDataByApplicationIDTaskIDDateRange(short applicationId, short task_id, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskData>(getQueryByEnum(Table.TaskData,TaskDataQuery.SelectByApplicationIDTaskIDStartDateEndDate),
                    new { application_id = applicationId,task_id=task_id,
                        startDate=startDate, endDate=endDate
                    }
                     ).ToList();
            }
        }
        public static List<Models.TaskData> SelectTaskDataByApplicationIDTaskIDStatusUserList(short applicationId,short taskId,string status,List<string> userList)
        {
            using (IDbConnection conn = DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskData>(getQueryByEnum(Table.TaskData, TaskDataQuery.SelectByApplicationIDTaskIDStatusIDUserList),
                    new
                    {
                        application_id = applicationId,
                        task_info_id = taskId,
                        status = status,
                        userList = userList.ToArray()
                    }).ToList();
            }
        }

        #endregion
        #region task_instance
        internal static void InsertTaskInstance(ref Models.TaskInstance taskInstance)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id = conn.Insert(taskInstance);
                taskInstance.id = id;
                taskInstance = conn.Get<Models.TaskInstance>(id);
            }
        }
        internal static void CreateNewTaskInstance(Models.TaskData taskData,Configurator Config,string Status,string parameter, string comment,StatusCode status,List<WindowsUser> submitTo, out Models.TaskInstance taskInstance)
        {
            bool isDone = false;
            switch (Status)
            {
                case Common.Task.AbstractBaseTask.STATUS_COMPLETE:
                case Common.Task.AbstractBaseTask.STATUS_FAIL:
                case Common.Task.AbstractBaseTask.STATUS_CANCEL:
                    isDone = true;
                    break;
            }
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                taskInstance = new Models.TaskInstance()
                {
                    task_data_id = taskData.id,
                    entry_date = taskData.entry_date,
                    entry_author = taskData.entry_author,
                    task_status = Status,
                    is_done = isDone,
                    parameter = parameter,
                    comment = comment,
                    status_info_id = status == null ? (long?)null : status.StatusID
                };
                InsertTaskInstance(ref taskInstance);
                Models.TaskDataStatus taskStatus = SelectTaskStatus(taskData);
                Models.TaskDataStatus statusRecord = new Models.TaskDataStatus()
                {
                    task_data_id = taskData.id,
                    latest_task_instance_id = taskInstance.id,
                    latest_update_date = taskInstance.entry_date,
                    task_status = taskInstance.task_status
                };
                if (taskStatus == null)
                {
                    
                    InsertTaskStatus(statusRecord);
                }
                else
                {
                    UpdateTaskStatus(statusRecord);
                }

                if (submitTo != null)
                {
                    foreach(WindowsUser winUser in submitTo)
                    {
                        if (winUser.IsValid)
                            InsertTaskSubmitInformation(new Models.TaskSubmitInformation() { task_instance_id = taskInstance.id, submit_to = winUser.UserID });
                    }
                    
                }
                scope.Complete();
            }
        }
        internal static void DeleteTaskInstance(Models.TaskInstance taskInstance)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete(taskInstance);
            }
        }
        public static Models.TaskInstance SelectTaskInstance(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskInstance>(id);
            }
        }
        public static List<Models.TaskInstance> SelectTaskInstanceByTaskData(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
               return conn.Query<Models.TaskInstance>(getQueryByEnum(Table.TaskInstance,TaskInstanceQuery.SelectByTaskDataID),
                   new { task_data_id = taskData.id }
                    ).ToList();
            }
        }
        #endregion
        #region task_data_status
        internal static void InsertTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskDataStatus>(taskStatus);
                taskStatus = conn.Get<Models.TaskDataStatus>(taskStatus.task_data_id);
            }
        }
        internal static void UpdateTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.TaskDataStatus>(taskStatus);
            }
        }
        public static Models.TaskDataStatus SelectTaskStatus(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskDataStatus>(taskData.id);
            }
        }
        internal static void DeleteTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskDataStatus>(taskStatus);
            }
        }
        #endregion
        #region task_data_log
        internal static void InsertTaskDataLog(ref Models.TaskDataLog taskLog)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                long id = conn.Insert<Models.TaskDataLog>(taskLog);
                taskLog = conn.Get<Models.TaskDataLog>(id);
            }
        }
        internal static void DeleteTaskDataLog(Models.TaskDataLog taskLog)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskDataLog>(taskLog);
            }
        }
        internal static List<Models.TaskDataLog> SelectTaskLogs(long taskDataID)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskDataLog>(getQuery(Table.TaskDataLog, SELECT),
                    new { task_data_id = taskDataID }
                    ).ToList();
            }
        }
        #endregion
        #region task_instance_submit_information
        internal static Models.TaskSubmitInformation InsertTaskSubmitInformation(Models.TaskSubmitInformation taskSubmit)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskSubmitInformation>(taskSubmit);
                return conn.Get<Models.TaskSubmitInformation>(taskSubmit.id);
            }
        }
        
        public static List<Models.TaskSubmitInformation> SelectTaskSubmitInformation(Models.TaskInstance taskInstance)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Query<Models.TaskSubmitInformation>(getQueryByEnum(Table.TaskSubmitInformation, SELECT),
                   new { task_instance_id = taskInstance.id }
                    ).ToList();
            }
        }
        internal static void DeleteTaskSubmitInformation(Models.TaskSubmitInformation taskSubmit)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskSubmitInformation>(taskSubmit);
            }
        }
        #endregion
    }
}
