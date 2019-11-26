using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;
using Dapper;
using System.Linq;
using Dapper.FastCrud;

namespace Gyomu.Common
{
    public partial class GyomuDataAccess:BaseDapperAccess
    {
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
            TaskSubmitInformation,
            Service,
            ServiceType,
            TaskSchedulerConfig
        }
        private enum TaskDataQuery
        {
            SelectByApplicationIDTaskIDStartDateEndDate,
            SelectByApplicationIDTaskIDStatusIDUserList,
            SelectOpenTaskByApplicationIDDateRange,
            SelectOpenTaskByApplicationIDEntryAuthorDateRange,
        }
        //private enum TaskInstanceQuery
        //{
        //    SelectByTaskDataID
        //}
        //private static string SELECT { get { return "SELECT"; } }
        //private static string INSERT { get { return "INSERT"; } }
        //private static string UPDATE { get { return "UPDATE"; } }
        //private static string DELETE { get { return "DELETE"; } }
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
                            //{SELECT,"SELECT * from gyomu_param_master WHERE item_key=@key" },
                            //{DELETE,"DELETE from gyomu_param_master WHERE item_key=@key AND item_fromdate=''" },
                            //{UPDATE,"UPDATE gyomu_param_master SET item_value=@value WHERE item_key=@key AND item_fromdate=''" },
                            //{INSERT,"INSERT INTO gyomu_param_master (item_key,item_value,item_fromdate) VALUES(@key,@value,'')" },
                            {LOCK,"SELECT * from gyomu_param_master WHERE item_key=@key FOR UPDATE" }
                        }
                    },
                    //{Table.StatusHandler,
                    //    new Dictionary<string, string>()
                    //    {
                    //         {"SelectByApplication","SELECT * from gyomu_status_handler WHERE application_id=@application_id" }

                    //    }
                    //},
                    //{Table.StatusInfo,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {"SelectByApplication","SELECT * from gyomu_status_info WHERE application_id=@application_id" }
                    //    }
                    //},
                    //{Table.MarketHoliday,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {SELECT,"SELECT holiday from gyomu_market_holiday WHERE market=@market" },
                    //        {DELETE,"DELETE from gyomu_market_holiday WHERE market=@market AND year=@year" },
                    //        {INSERT,"INSERT INTO gyomu_market_holiday VALUES(@market,@year,@holiday)" }
                    //    }
                    //},
                    //{Table.MilestoneDaily,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {"SelectByTargetDate","SELECT * from gyomu_milestone_daily WHERE target_date=@target_date" },
                    //        {INSERT,"INSERT INTO gyomu_milestone_daily (target_date,milestone_id) VALUES(@target_date,@milestone_id)" },
                    //        {DELETE,"DELETE FROM gyomu_milestone_daily WHERE target_date=@target_date AND milestone_id=@milestone_id" },
                    //        {SELECT,"SELECT * from gyomu_milestone_daily WHERE target_date=@target_date AND milestone_id=@milestone_id" },
                    //    }
                    //},
                    //{Table.VariableParameter,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {SELECT,"SELECT * from gyomu_variable_parameter" }
                    //    }
                    //},
                    //{Table.TaskInfo,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {INSERT,"INSERT INTO gyomu_task_info_cdtbl (application_id, task_id, description, assembly_name, class_name, restartable) VALUES (@application_id,@task_id,@description,@assembly_name,@class_name,@restartable)" },
                    //        {SELECT,"SELECT * from gyomu_task_info_cdtbl WHERE application_id=@application_id AND task_id=@task_id" },
                    //        {"SelectAll","SELECT * from gyomu_task_info_cdtbl" },
                    //        {DELETE,"DELETE from gyomu_task_info_cdtbl WHERE application_id=@application_id AND task_id=@task_id" },
                    //        {"SelectByApplication","SELECT * from gyomu_task_info_cdtbl WHERE application_id=@application_id" },
                    //        {"SelectByAssembly","SELECT * from gyomu_task_info_cdtbl WHERE assembly_name=@assembly_name" }
                    //    }

                    //},
                    //{Table.TaskAccessList,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {SELECT,"SELECT * from gyomu_task_info_access_list WHERE application_id=@application_id AND task_info_id=@task_id" }
                    //    }
                    //},
                    {Table.TaskData,
                        new Dictionary<string, string>()
                        {
 //                           {SELECT,"SELECT * from gyomu_task_data WHERE application_id=@application_id AND task_id=@task_id" },
                            {LOCK,"SELECT gyomu_task_data.* FROM gyomu_task_data WHERE id=@id FOR UPDATE" },
                            //{enumDescription(TaskDataQuery.SelectByApplicationIDTaskIDStartDateEndDate),
                            //    "SELECT gyomu_task_data.* FROM gyomu_task_data WHERE application_id=@application_id and task_info_id=@task_id AND entry_date >=@startDate and entry_date<@endDate" },
                            {enumDescription(TaskDataQuery.SelectByApplicationIDTaskIDStatusIDUserList),
                                "SELECT td.* FROM  gyomu_task_data td WITH(NOLOCK) INNER JOIN dbo.gyomu_task_data_status tds WITH(NOLOCK) ON td.id=tds.task_data_id INNER JOIN dbo.gyomu_task_instance_submit_information tis WITH(NOLOCK) ON tds.latest_task_instance_id=tis.task_instance_id WHERE application_id=@application_id AND task_info_id=@task_info_id AND tds.task_status=@status AND submit_to IN @userList) ORDER BY tds.latest_task_instance_id"},
                         }
                    },
                    //{Table.TaskInstance,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {enumDescription(TaskInstanceQuery.SelectByTaskDataID),
                    //            "SELECT * FROM gyomu_task_instance WHERE task_data_id=@task_data_id ORDER BY entry_date DESC" },
                    //    }
                    //},
                    //{Table.TaskDataLog,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {SELECT,"SELECT * FROM gyomu_task_data_log WHERE task_data_id=@task_data_id " }
                    //    }
                    //},
                    //{Table.TaskSubmitInformation,
                    //    new Dictionary<string, string>()
                    //    {
                    //        {SELECT,"SELECT * FROM gyomu_task_instance_submit_information WHERE task_instance_id=@task_instance_id" }
                    //    }
                    //}
                }
            },
            {SettingItem.DBType.MSSQL,
                new Dictionary<Table,Dictionary<string, string>>()
                {
                    {Table.ParameterMaster,
                        new Dictionary<string, string>()
                        {
                             {LOCK,"SELECT * from gyomu_param_master WITH(UPDLOCK) WHERE item_key=@key" }
                        }
                    },
                    {Table.TaskData,
                        new Dictionary<string, string>()
                        {
                            {LOCK,"SELECT task_data.* FROM gyomu_task_data WITH(UPDLOCK) WHERE id=@id" },
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
        //private static Dictionary<SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<Models.ApplicationInfo>> applicationInfoMapping = new Dictionary<SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<Models.ApplicationInfo>>();
        //private static Dictionary<SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<Models.StatusHandler>> statusHandlerMapping = new Dictionary<SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<Models.StatusHandler>>();

        //private static Dapper.FastCrud.Mappings.EntityMapping<Models.ApplicationInfo> ApplicationMapper
        //{
        //    get
        //    {
        //        return applicationInfoMapping[DBConnectionFactory.SQLDB];
        //    }
        //}
        //private static Dapper.FastCrud.Mappings.EntityMapping<T> getMapping<T>()
        //{
        //    Type type = typeof(T);
        //    if (type ==typeof(Models.ApplicationInfo))
        //    {
        //        return applicationInfoMapping[DBConnectionFactory.SQLDB];
        //    }
        //}
        #region gyomu_apps_info_cdtbl
        internal static void InsertApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                
                conn.Insert<Models.ApplicationInfo>(applicationInfo,statement => statement.WithEntityMappingOverride(Models.ApplicationInfo.GetMapping()));
                //conn.Execute(PerTableTypeSQLQuery[Table.ApplicationInfo][INSERT], 
                //    new { id= applicationInfo.application_id,
                //        description= applicationInfo.description,
                //        mail_from_address = applicationInfo.mail_from_address,
                //        mail_from_name= applicationInfo.mail_from_name
                //    });
            }
        }
        internal static void DeleteApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                
                conn.Delete<Models.ApplicationInfo>(applicationInfo, statement => statement.WithEntityMappingOverride(Models.ApplicationInfo.GetMapping()));
                //conn.Execute(PerTableTypeSQLQuery[Table.ApplicationInfo][DELETE],
                //    new
                //    {
                //        id = application_id
                //    });
            }
        }
        internal static void UpdateApplicationInfo(Models.ApplicationInfo applicationInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.ApplicationInfo>(applicationInfo, statement => statement.WithEntityMappingOverride(Models.ApplicationInfo.GetMapping()));
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
        internal static Models.ApplicationInfo SelectApplicationInfo(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.ApplicationInfo>(new Models.ApplicationInfo() { application_id = application_id }, statement => statement.WithEntityMappingOverride(Models.ApplicationInfo.GetMapping()));
            }
        }
        #endregion

        #region gyomu_status_handler
        internal static int InsertStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.StatusHandler>(statusHandler, statement => statement.WithEntityMappingOverride(Models.StatusHandler.GetMapping()));
                return statusHandler.id;
            }
        }
        internal static void DeleteStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.StatusHandler>(statusHandler, statement => statement.WithEntityMappingOverride(Models.StatusHandler.GetMapping()));
            }
        }
        internal static void UpdateStatusHandler(Models.StatusHandler statusHandler)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.StatusHandler>(statusHandler, statement => statement.WithEntityMappingOverride(Models.StatusHandler.GetMapping()));
            }
        }
        internal static Models.StatusHandler SelectStatusHandler(int id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.StatusHandler>(new Models.StatusHandler() { id = id }, statement => statement.WithEntityMappingOverride(Models.StatusHandler.GetMapping()));
            }
        }
        internal static List<Models.StatusHandler> SelectStatusHandlers(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.StatusHandler>(statement =>
                statement.WithEntityMappingOverride(Models.StatusHandler.GetMapping()).Where
                ($"{nameof(Models.StatusHandler.application_id):C}=@ApplicationID")
                .WithParameters(new { ApplicationID = application_id })).ToList();
                //return conn.Query<Models.StatusHandler>(getQuery(Table.StatusHandler, "SelectByApplication"),
                //    new { application_id }
                //    ).ToList();
            }
        }
        #endregion
        #region gyomu_param_master
        internal static void InsertParameter(string key, string strValue)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.ParameterMaster>(new Models.ParameterMaster() { item_key = key, item_value = strValue, item_fromdate = "" }
                , statement => statement.WithEntityMappingOverride(Models.ParameterMaster.GetMapping()));
                //                conn.Execute(getQuery(Table.ParameterMaster, INSERT), new { key, value = strValue });
            }
        }
        internal static void DeleteParameter(string key)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.ParameterMaster>(new Models.ParameterMaster { item_key=key,item_fromdate=""}
                                , statement => statement.WithEntityMappingOverride(Models.ParameterMaster.GetMapping()));
                //conn.Execute(getQuery(Table.ParameterMaster, DELETE), new { key });
            }
        }
        internal static void UpdateParameter(string key, string strValue)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.ParameterMaster>(new Models.ParameterMaster { item_key=key,item_fromdate="",item_value=strValue}
                                , statement => statement.WithEntityMappingOverride(Models.ParameterMaster.GetMapping()));
                //conn.Execute(getQuery(Table.ParameterMaster, UPDATE), new { value = strValue, key });
            }
        }
        internal static Models.ParameterMaster LoadParameter(string key)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                Models.ParameterMaster[] parameters = conn.Find<Models.ParameterMaster>(statement =>
                statement.WithEntityMappingOverride(Models.ParameterMaster.GetMapping()).Where
                ($"{nameof(Models.ParameterMaster.item_key):C}=@Key")
                .WithParameters(new { Key = key })).ToArray();
                //Models.ParameterMaster[] parameters = conn.Query<Models.ParameterMaster>(getQuery(Table.ParameterMaster, SELECT), new { key }).ToArray();
                return parameters?.Length > 0 ? parameters[0] : null;
            }
        }
        internal static T LockParameterProcess<T>(string key, Func<T> method)
        {

            T result = default;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
                {
                    conn.Query<Models.ParameterMaster>(getQuery(Table.ParameterMaster, LOCK), new { key });



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
        #region gyomu_status_info
        internal static long InsertStatusInfo(Models.StatusInfo statusInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.StatusInfo>(statusInfo, statement => statement.WithEntityMappingOverride(Models.StatusInfo.GetMapping()));

                long id = statusInfo.id;
                return id;
            }
        }
        internal static void DeleteStatusInfo(Models.StatusInfo statusInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.StatusInfo>(statusInfo, statement => statement.WithEntityMappingOverride(Models.StatusInfo.GetMapping()));
            }
        }
        internal static Models.StatusInfo SelectStatusInfo(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.StatusInfo>(new Models.StatusInfo { id=id}
                    , statement => statement.WithEntityMappingOverride(Models.StatusInfo.GetMapping()));
            }
        }
        internal static List<Models.StatusInfo> SelectStatusInfosByApplicationID(short application_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.StatusInfo>(statement => statement
                .WithEntityMappingOverride(Models.StatusInfo.GetMapping())
                .Where($"{nameof(Models.StatusInfo.application_id):C}=@ApplicationID")
                .WithParameters(new { ApplicationID = application_id })).ToList();
                
                //return conn.Query<Models.StatusInfo>(getQuery(Table.StatusInfo, "SelectByApplication"),
                //    new { application_id }
                //    ).ToList();
            }
        }
        #endregion
        #region gyomu_market_holiday
        internal static void InsertHoliday(string market, short year, DateTime holiday)
        {
            if (holiday.Year != year)
                throw new InvalidOperationException("Holiday and Year is different year");
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.MarketHoliday>(new Models.MarketHoliday { market = market, year = year, holiday = holiday.ToString("yyyyMMdd") }
                , statement => statement.WithEntityMappingOverride(Models.MarketHoliday.GetMapping()));
                //conn.Execute(getQuery(Table.MarketHoliday, INSERT), new { market, year, holiday = holiday.ToString("yyyyMMdd") });
            }
        }
        internal static void InsertHoliday(string market, short year, List<DateTime> holidays)
        {
            foreach (DateTime holiday in holidays)
                if (holiday.Year != year)
                    throw new InvalidOperationException("Holiday and Year is different year");
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                foreach (DateTime holiday in holidays)
                    conn.Insert<Models.MarketHoliday>(new Models.MarketHoliday { market = market, year = year, holiday = holiday.ToString("yyyyMMdd") }
                , statement => statement.WithEntityMappingOverride(Models.MarketHoliday.GetMapping()));
                // conn.Execute(getQuery(Table.MarketHoliday, INSERT), new { market, year, holiday = holiday.ToString("yyyyMMdd") });
            }
        }
        internal static void DeleteHoliday(string market, short year)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.BulkDelete<Models.MarketHoliday>(
                    statement => statement.WithEntityMappingOverride(Models.MarketHoliday.GetMapping())
                    .Where($"{nameof(Models.MarketHoliday.market):C}=@Market")
                    .Where($"{nameof(Models.MarketHoliday.year):C}=@Year")
                    .WithParameters(new { Market = market, Year = year }));
//                conn.Execute(getQuery(Table.MarketHoliday, DELETE), new { market, year });
            }
        }
        internal static List<DateTime> ListHoliday(string market)
        {
            List<DateTime> holidayList = new List<DateTime>();
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                IEnumerable<string> holidays = conn.Find<Models.MarketHoliday>(
                    statement => statement.WithEntityMappingOverride(Models.MarketHoliday.GetMapping())
                    .Where($"{nameof(Models.MarketHoliday.market):C}=@Market")
                    .WithParameters(new { Market = market }))
                    .Select(t => t.holiday);
                foreach (string holidayYYYYMMDD in holidays)
                    holidayList.Add(DateTime.ParseExact(holidayYYYYMMDD, "yyyyMMdd", null));

            }
            return holidayList;
        }
        #endregion
        #region gyomu_milestone_daily
        internal static void InsertMilestoneDaily(DateTime targetDate, string milestoneId, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.MilestoneDaily>(new Models.MilestoneDaily { target_date=targetDateYYYYMMDD,milestone_id=milestoneId}
                , statement => statement.WithEntityMappingOverride(Models.MilestoneDaily.GetMapping()));
                //conn.Execute(getQuery(Table.MilestoneDaily, INSERT), new { target_date = targetDateYYYYMMDD, milestone_id = milestoneId });
            }
        }
        internal static void DeleteMilestoneDaily(DateTime targetDate, string milestoneId, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.MilestoneDaily>(new Models.MilestoneDaily { target_date = targetDateYYYYMMDD, milestone_id = milestoneId }
                , statement => statement.WithEntityMappingOverride(Models.MilestoneDaily.GetMapping()));
                //conn.Execute(getQuery(Table.MilestoneDaily, DELETE), new { target_date = targetDateYYYYMMDD, milestone_id = milestoneId });
            }
        }
        internal static List<Models.MilestoneDaily> SelectMilestoneDaily(DateTime targetDate, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.MilestoneDaily>(
                    statement => statement.WithEntityMappingOverride(Models.MilestoneDaily.GetMapping())
                    .Where($"{nameof(Models.MilestoneDaily.target_date):C}=@TargetDate")
                    .WithParameters(new { TargetDate = targetDateYYYYMMDD })
                    ).ToList();
                //return conn.Query<Models.MilestoneDaily>(getQuery(Table.MilestoneDaily, "SelectByTargetDate"), new { target_date = targetDateYYYYMMDD }).ToList();
            }
        }
        internal static Models.MilestoneDaily SelectMilestoneDaily(DateTime targetDate, string milestoneId, bool isMonthly = false)
        {
            string targetDateYYYYMMDD = targetDate.ToString("yyyyMMdd");
            if (isMonthly)
                targetDateYYYYMMDD = targetDate.ToString("yyyyMM") + "**";
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.MilestoneDaily>(new Models.MilestoneDaily { target_date = targetDateYYYYMMDD, milestone_id = milestoneId }
                , statement => statement.WithEntityMappingOverride(Models.MilestoneDaily.GetMapping())
                );
                //return conn.QueryFirstOrDefault<Models.MilestoneDaily>(getQuery(Table.MilestoneDaily, SELECT), new { target_date = targetDateYYYYMMDD, milestone_id = milestoneId });
            }
        }
        #endregion
        #region gyomu_variable_parameter
        internal static List<Models.VariableParameter> GetVariableParameters()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.VariableParameter>(statement => statement.WithEntityMappingOverride(Models.VariableParameter.GetMapping())).ToList();
                //return conn.Query<Models.VariableParameter>(getQuery(Table.VariableParameter, SELECT)).ToList();
            }
        }
        #endregion

        #region gyomu_task_info_cdtbl
        public static void InsertTaskInfo(Models.TaskInfo taskInfo)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskInfo>(taskInfo
                    , statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping()));
//                conn.Execute(getQuery(Table.TaskInfo, INSERT), new { application_id = taskInfo.application_id, task_id = taskInfo.task_id, description = taskInfo.description, assembly_name = taskInfo.assembly_name, class_name = taskInfo.class_name, restartable = taskInfo.restartable });
            }
        }
        internal static void InsertTaskInfo(short applicationId, short taskId, string description, string assembyName, string className, bool canRestart)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskInfo>(new Models.TaskInfo { application_id=applicationId,task_id=taskId,description=description,assembly_name=assembyName,class_name=className,restartable=canRestart}
                    , statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping()));
                //conn.Execute(getQuery(Table.TaskInfo, INSERT), new { application_id = applicationId, task_id = taskId, description = description, assembly_name = assembyName, class_name = className, restartable = canRestart });
            }
        }
        internal static void DeleteTaskInfo(short applicationId, short taskId)
        {

            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskInfo>(new Models.TaskInfo { application_id=applicationId,task_id=taskId}
                , statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping()));
                //conn.Execute(getQuery(Table.TaskInfo, DELETE), new { application_id = applicationId, task_id = taskId });
            }
        }
        internal static Models.TaskInfo SelectTaskInfo(short applicationId, short taskId)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskInfo>(new Models.TaskInfo { application_id = applicationId, task_id = taskId }
                , statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping()));
                //return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, SELECT), new { application_id = applicationId, task_id = taskId }).FirstOrDefault();
            }
        }
        internal static List<Models.TaskInfo> SelectAllTaskInfo()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskInfo>(statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping())).ToList();
                //return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectAll")).ToList();
            }
        }
        internal static List<Models.TaskInfo> SelectTaskInfoByApplication(short applicationId)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskInfo>(
                    statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping())
                    .Where($"{nameof(Models.TaskInfo.application_id):C}=@ApplicationID")
                    .WithParameters(new { ApplicationID = applicationId })
                    ).ToList();
                //return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectByApplication"), new { application_id = applicationId }).ToList();
            }
        }
        public static List<Models.TaskInfo> SelectTaskInfoByAssembly(string assemblyName)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskInfo>(
                    statement => statement.WithEntityMappingOverride(Models.TaskInfo.GetMapping())
                    .Where($"{nameof(Models.TaskInfo.assembly_name):C}=@AssemblyName")
                    .WithParameters(new { AssemblyName = assemblyName })
                    ).ToList();
                //return conn.Query<Models.TaskInfo>(getQuery(Table.TaskInfo, "SelectByAssembly"), new { assembly_name = assemblyName }).ToList();
            }
        }
        #endregion

        #region gyomu_task_info_access_list
        internal static void InsertTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskAccessList>(taskAccess
                , statement => statement.WithEntityMappingOverride(Models.TaskAccessList.GetMapping()));
            }
        }
        internal static void DeleteTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskAccessList>(taskAccess
                , statement => statement.WithEntityMappingOverride(Models.TaskAccessList.GetMapping()));
            }
        }
        internal static void UpdateTaskAccessList(Models.TaskAccessList taskAccess)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.TaskAccessList>(taskAccess
                , statement => statement.WithEntityMappingOverride(Models.TaskAccessList.GetMapping()));
            }
        }
        internal static Models.TaskAccessList SelectTaskAccessList(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskAccessList>(
                    statement => statement.WithEntityMappingOverride(Models.TaskAccessList.GetMapping())
                    .Where($"{nameof(Models.TaskAccessList.id):C}=@ID")
                    .WithParameters(new { ID = id })
                    ).FirstOrDefault();
            }
        }
        internal static List<Models.TaskAccessList> SelectTaskAccessLists(short application_id, short task_id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskAccessList>(
                    statement => statement.WithEntityMappingOverride(Models.TaskAccessList.GetMapping())
                    .Where($"{nameof(Models.TaskAccessList.application_id):C}=@ApplicationID")
                    .Where($"{nameof(Models.TaskAccessList.task_info_id):C}=@TaskID")
                    .WithParameters(new { ApplicationID = application_id, TaskID = task_id })
                    ).ToList();
                //return conn.Query<Models.TaskAccessList>(getQuery(Table.TaskAccessList, SELECT),
                //    new { application_id = application_id, task_id = task_id }
                //    ).ToList();
            }
        }
        #endregion

        #region gyomu_task_data
        internal static void InsertTaskData(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskData>(taskData
                , statement => statement.WithEntityMappingOverride(Models.TaskData.GetMapping()));
            }
        }
        internal static void CreateNewTask(
            Models.TaskInfo taskInformation, Configurator Config,
            string parameter, string comment,
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
                    parent_task_data_id = parentTask == null ? (long?)null : parentTask.id
                };
                InsertTaskData(taskData);
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
                InsertTaskInstance(taskInstance);
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
                conn.Delete<Models.TaskData>(taskData
                , statement => statement.WithEntityMappingOverride(Models.TaskData.GetMapping()));
            }
        }

        internal static Models.TaskData SelectTaskData(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskData>(new Models.TaskData { id=id}
                , statement => statement.WithEntityMappingOverride(Models.TaskData.GetMapping()));
            }
        }
        internal static List<Models.TaskData> SelectTaskDataByApplicationIDTaskIDDateRange(short applicationId, short task_id, DateTime startDate, DateTime endDate)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskData>(
                    statement => statement.WithEntityMappingOverride(Models.TaskData.GetMapping())
                    .Where($@"{nameof(Models.TaskData.application_id):C}=@ApplicationID AND {nameof(Models.TaskData.task_info_id):C}=@TaskID 
                    AND { nameof(Models.TaskData.entry_date):C} >= @StartDate AND { nameof(Models.TaskData.entry_date):C}  < @EndDate")
                    .WithParameters(new { ApplicationID = applicationId, TaskID=task_id, StartDate=startDate,EndDate=endDate })
                    ).ToList();
                //return conn.Query<Models.TaskData>(getQueryByEnum(Table.TaskData, TaskDataQuery.SelectByApplicationIDTaskIDStartDateEndDate),
                //    new
                //    {
                //        application_id = applicationId,
                //        task_id = task_id,
                //        startDate = startDate,
                //        endDate = endDate
                //    }
                //     ).ToList();
            }
        }
        internal static List<Models.TaskData> SelectTaskDataByApplicationIDTaskIDStatusUserList(short applicationId, short taskId, string status, List<string> userList)
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
        #region gyomu_task_instance
        internal static void InsertTaskInstance(Models.TaskInstance taskInstance)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert(taskInstance
                    , statement => statement.WithEntityMappingOverride(Models.TaskInstance.GetMapping()));
            }
        }
        internal static void CreateNewTaskInstance(Models.TaskData taskData, Configurator Config, string Status, string parameter, string comment, StatusCode status, List<WindowsUser> submitTo, out Models.TaskInstance taskInstance)
        {
            bool isDone = false;
            switch (Status)
            {
                case Common.Tasks.AbstractBaseTask.STATUS_COMPLETE:
                case Common.Tasks.AbstractBaseTask.STATUS_FAIL:
                case Common.Tasks.AbstractBaseTask.STATUS_CANCEL:
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
                InsertTaskInstance(taskInstance);
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
                    foreach (WindowsUser winUser in submitTo)
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
                conn.Delete(taskInstance
                   , statement => statement.WithEntityMappingOverride(Models.TaskInstance.GetMapping()));

            }
        }
        internal static Models.TaskInstance SelectTaskInstance(long id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {

                return conn.Get<Models.TaskInstance>(new Models.TaskInstance { id=id}
                 , statement => statement.WithEntityMappingOverride(Models.TaskInstance.GetMapping()));

            }
        }
        internal static List<Models.TaskInstance> SelectTaskInstanceByTaskData(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskInstance>(
                    statement => statement.WithEntityMappingOverride(Models.TaskInstance.GetMapping())
                    .Where($@"{nameof(Models.TaskInstance.task_data_id):C}=@TaskDataID")
                    .OrderBy($"{nameof(Models.TaskInstance.entry_date):C} DESC")
                    .WithParameters(new { TaskDataID = taskData.id})
                    ).ToList();
                //return conn.Query<Models.TaskInstance>(getQueryByEnum(Table.TaskInstance, TaskInstanceQuery.SelectByTaskDataID),
                //    new { task_data_id = taskData.id }
                //     ).ToList();
            }
        }
        #endregion
        #region gyomu_task_instance_submit_information
        internal static void InsertTaskSubmitInformation(Models.TaskSubmitInformation taskSubmit)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskSubmitInformation>(taskSubmit
                     , statement => statement.WithEntityMappingOverride(Models.TaskSubmitInformation.GetMapping()));

            }
        }

        internal static List<Models.TaskSubmitInformation> SelectTaskSubmitInformation(Models.TaskInstance taskInstance)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskSubmitInformation>(
                    statement => statement.WithEntityMappingOverride(Models.TaskSubmitInformation.GetMapping())
                    .Where($"{nameof(Models.TaskSubmitInformation.task_instance_id):C}=@TaskInstanceID")
                    .WithParameters(new { TaskInstanceID = taskInstance.id })
                    ).ToList();
                //return conn.Query<Models.TaskSubmitInformation>(getQueryByEnum(Table.TaskSubmitInformation, SELECT),
                //   new { task_instance_id = taskInstance.id }
                 //   ).ToList();
            }
        }
        internal static void DeleteTaskSubmitInformation(Models.TaskSubmitInformation taskSubmit)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskSubmitInformation>(taskSubmit
                    , statement => statement.WithEntityMappingOverride(Models.TaskSubmitInformation.GetMapping()));

            }
        }
        #endregion
        #region gyomu_task_data_status
        internal static void InsertTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskDataStatus>(taskStatus
                , statement => statement.WithEntityMappingOverride(Models.TaskDataStatus.GetMapping()));

            }
        }
        internal static void UpdateTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.TaskDataStatus>(taskStatus
                , statement => statement.WithEntityMappingOverride(Models.TaskDataStatus.GetMapping()));

            }
        }
        internal static Models.TaskDataStatus SelectTaskStatus(Models.TaskData taskData)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskDataStatus>(new Models.TaskDataStatus { task_data_id = taskData.id }
                 , statement => statement.WithEntityMappingOverride(Models.TaskDataStatus.GetMapping()));

            }
        }
        internal static void DeleteTaskStatus(Models.TaskDataStatus taskStatus)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskDataStatus>(taskStatus
                , statement => statement.WithEntityMappingOverride(Models.TaskDataStatus.GetMapping()));

            }
        }
        #endregion
        #region gyomu_task_data_log
        internal static void InsertTaskDataLog(Models.TaskDataLog taskLog)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskDataLog>(taskLog
                , statement => statement.WithEntityMappingOverride(Models.TaskDataLog.GetMapping()));


            }
        }
        internal static void DeleteTaskDataLog(Models.TaskDataLog taskLog)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskDataLog>(taskLog
                , statement => statement.WithEntityMappingOverride(Models.TaskDataLog.GetMapping()));

            }
        }
        internal static List<Models.TaskDataLog> SelectTaskLogs(long taskDataID)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.TaskDataLog>(
                   statement => statement.WithEntityMappingOverride(Models.TaskDataLog.GetMapping())
                   .Where($"{nameof(Models.TaskDataLog.task_data_id):C}=@TaskDataID")
                   .WithParameters(new { TaskDataID = taskDataID })
                   ).ToList();
                //return conn.Query<Models.TaskDataLog>(getQuery(Table.TaskDataLog, SELECT),
                //    new { task_data_id = taskDataID }
                //    ).ToList();
            }
        }
        #endregion
        #region gyomu_service_type
        internal static void InsertServiceType(Models.ServiceType serviceType)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {

                conn.Insert<Models.ServiceType>(serviceType, statement => statement.WithEntityMappingOverride(Models.ServiceType.GetMapping()));
                
            }
        }
        internal static void DeleteServiceType(Models.ServiceType serviceType)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {

                conn.Delete<Models.ServiceType>(serviceType, statement => statement.WithEntityMappingOverride(Models.ServiceType.GetMapping()));
                
            }
        }
        internal static void UpdateServiceType(Models.ServiceType serviceType)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.ServiceType>(serviceType, statement => statement.WithEntityMappingOverride(Models.ServiceType.GetMapping()));
            }
        }
        internal static Models.ServiceType SelectServiceType(short serviceTypeID)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.ServiceType>(new Models.ServiceType() { id = serviceTypeID }, statement => statement.WithEntityMappingOverride(Models.ServiceType.GetMapping()));
            }
        }
        internal static List<Models.ServiceType> GetAllServiceType()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.ServiceType>(statement => statement.WithEntityMappingOverride(Models.ServiceType.GetMapping())).ToList();
                
            }
        }
        #endregion
        #region gyomu_service
        internal static void InsertService(Models.Service service)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {

                conn.Insert<Models.Service>(service, statement => statement.WithEntityMappingOverride(Models.Service.GetMapping()));

            }
        }
        internal static void DeleteService(Models.Service service)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {

                conn.Delete<Models.Service>(service, statement => statement.WithEntityMappingOverride(Models.Service.GetMapping()));

            }
        }
        internal static void UpdateService(Models.Service service)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.Service>(service, statement => statement.WithEntityMappingOverride(Models.Service.GetMapping()));
            }
        }
        internal static Models.Service SelectService(short serviceID)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.Service>(new Models.Service() { id = serviceID }, statement => statement.WithEntityMappingOverride(Models.Service.GetMapping()));
            }
        }
        internal static List<Models.Service> GetAllService()
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Find<Models.Service>(statement => statement.WithEntityMappingOverride(Models.Service.GetMapping())).ToList();

            }
        }
        #endregion
        #region gyomu_task_scheduler_config
        internal static void InsertTaskSchedulerConfig(Models.TaskSchedulerConfig taskSchedulerConfig)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Insert<Models.TaskSchedulerConfig>(taskSchedulerConfig, statement => statement.WithEntityMappingOverride(Models.TaskSchedulerConfig.GetMapping()));
            }
        }
        internal static void DeleteTaskSchedulerConfig(Models.TaskSchedulerConfig taskSchedulerConfig)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Delete<Models.TaskSchedulerConfig>(taskSchedulerConfig, statement => statement.WithEntityMappingOverride(Models.TaskSchedulerConfig.GetMapping()));
            }
        }
        internal static void UpdateTaskSchedulerConfig(Models.TaskSchedulerConfig taskSchedulerConfig)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                conn.Update<Models.TaskSchedulerConfig>(taskSchedulerConfig, statement => statement.WithEntityMappingOverride(Models.TaskSchedulerConfig.GetMapping()));
            }
        }
        internal static Models.TaskSchedulerConfig SelectTaskSchedulerConfig(int id)
        {
            using (IDbConnection conn = Common.DBConnectionFactory.GetGyomuConnection())
            {
                return conn.Get<Models.TaskSchedulerConfig>(new Models.TaskSchedulerConfig() { id = id }, statement => statement.WithEntityMappingOverride(Models.TaskSchedulerConfig.GetMapping()));
            }
        }
        
        #endregion
    }
}
