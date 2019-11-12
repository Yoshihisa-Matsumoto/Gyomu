using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Gyomu.Common;
using System.Linq;
using Gyomu.Models;

namespace Gyomu.Test.Common
{
    public class GyomuDataAccessTest
    {
        internal const short testApplicationId = 32650;
        internal const short testTaskID1 = 1;
        internal const short testTaskID2 = 2;
        #region ApplicationInfo
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertApplicationInfoTest(SettingItem.DBType db)

        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteApplicationInfo, insertApplicationInfo,
                updateApplicationInfo, deleteApplicationInfo }
            );
        }
        internal static void insertApplicationInfo()
        {
            Gyomu.Common.WindowsUser currentUser = Gyomu.Common.WindowsUser.CurrentWindowsUser;
            Models.ApplicationInfo insertItem = new Models.ApplicationInfo() { application_id = testApplicationId, description = "Test Application", mail_from_address = currentUser.MailAddress, mail_from_name = currentUser.DisplayName };
            Gyomu.Common.GyomuDataAccess.InsertApplicationInfo(
               insertItem);
            Models.ApplicationInfo selectItem = selectApplicationInfo();
            Assert.Equal(insertItem.application_id, selectItem.application_id);
            Assert.Equal(insertItem.description, selectItem.description);
            Assert.Equal(insertItem.mail_from_address, selectItem.mail_from_address);
            Assert.Equal(insertItem.mail_from_name, selectItem.mail_from_name);
        }
        internal static void deleteApplicationInfo()
        {
            Gyomu.Common.GyomuDataAccess.DeleteApplicationInfo(
                new Models.ApplicationInfo() { application_id = testApplicationId });
            Assert.Null(selectApplicationInfo());
        }
        internal static Models.ApplicationInfo selectApplicationInfo()
        {
            return Gyomu.Common.GyomuDataAccess.SelectApplicationInfo(testApplicationId);
        }
        internal static void updateApplicationInfo()
        {
            Models.ApplicationInfo applicationInfo= selectApplicationInfo();
            string originalDescription = applicationInfo.description;
            string updatedDescription = "Test App-Update";

            applicationInfo.description = updatedDescription;
            Gyomu.Common.GyomuDataAccess.UpdateApplicationInfo(applicationInfo);
            Models.ApplicationInfo updatedInfo = Gyomu.Common.GyomuDataAccess.SelectApplicationInfo(testApplicationId);
            Assert.Equal(updatedDescription, updatedInfo.description);

            applicationInfo.description = originalDescription;
            Gyomu.Common.GyomuDataAccess.UpdateApplicationInfo(applicationInfo);
            updatedInfo = Gyomu.Common.GyomuDataAccess.SelectApplicationInfo(testApplicationId);
            Assert.Equal(originalDescription, updatedInfo.description);
        }
        #endregion
        #region StatusHandler
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertStatusHandlerTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteApplicationInfo, insertApplicationInfo,
                deleteStatusHandler,insertStatusHandler,
                deleteStatusHandler, deleteApplicationInfo }
            );
            
        }

        internal static void insertStatusHandler()
        {
            Gyomu.Common.WindowsUser currentUser = Gyomu.Common.WindowsUser.CurrentWindowsUser;
            Models.StatusHandler insertItem = new Models.StatusHandler() {
                application_id =testApplicationId,recipient_address=currentUser.MailAddress,
                recipient_type="TO",status_type=StatusCode.ERROR_DEVEL
            };
            int insertedId=Gyomu.Common.GyomuDataAccess.InsertStatusHandler(
               insertItem);
            Models.StatusHandler selectItem = selectStatusHandler(insertedId);
            Assert.Equal(insertedId, selectItem.id);
            Assert.Equal(insertItem.application_id, selectItem.application_id);
            Assert.Equal(insertItem.recipient_address, selectItem.recipient_address);
            Assert.Equal(insertItem.recipient_type, selectItem.recipient_type);
            Assert.Equal(insertItem.region, selectItem.region);
            Assert.Equal(insertItem.status_type, selectItem.status_type);
        }

        internal static Models.StatusHandler selectStatusHandler(int id)
        {
            return GyomuDataAccess.SelectStatusHandler(id);
        }
        internal static void deleteStatusHandler()
        {
            foreach(Models.StatusHandler handler in GyomuDataAccess.SelectStatusHandlers(testApplicationId))
            {
                GyomuDataAccess.DeleteStatusHandler(handler);
            }
            Assert.Empty(GyomuDataAccess.SelectStatusHandlers(testApplicationId));
        }
        #endregion
        #region StatusInfo
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertStatusInfoTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteApplicationInfo, insertApplicationInfo,
                deleteStatusHandler,insertStatusHandler,
                insertStatusInfo,
                deleteStatusInfo,
                deleteStatusHandler, deleteApplicationInfo }
            );
        }
        internal static void insertStatusInfo()
        {
            Gyomu.Common.WindowsUser currentUser = Gyomu.Common.WindowsUser.CurrentWindowsUser;
            Models.StatusInfo insertItem = new Models.StatusInfo()
            {
                application_id = testApplicationId,
                instance_id=1234,
                hostname=System.Environment.MachineName,
                description="Test Description",
                developer_info="Test Developer Info",
                entry_author=currentUser.UserID,
                error_id=12345,
                status_type= StatusCode.ERROR_DEVEL,
                summary="Test Summary"
            };
            long insertedId = Gyomu.Common.GyomuDataAccess.InsertStatusInfo(
               insertItem);
            Models.StatusInfo selectItem = selectStatusInfo(insertedId);
            Assert.Equal(insertedId, selectItem.id);
            Assert.Equal(insertItem.application_id, selectItem.application_id);
            Assert.Equal(insertItem.description, selectItem.description);
            Assert.Equal(insertItem.developer_info, selectItem.developer_info);
            Assert.Equal(insertItem.entry_author, selectItem.entry_author);

        }

        internal static Models.StatusInfo selectStatusInfo(long id)
        {
            return GyomuDataAccess.SelectStatusInfo(id);
        }
        internal static void deleteStatusInfo()
        {
            foreach (Models.StatusInfo info in GyomuDataAccess.SelectStatusInfosByApplicationID(testApplicationId))
            {
                GyomuDataAccess.DeleteStatusInfo(info);
            }
            Assert.Empty(GyomuDataAccess.SelectStatusInfosByApplicationID(testApplicationId));
        }
        #endregion
        #region MarketHoliday
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertMarketHolidayTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                insertMarketHoliday,deleteMarketHoliday
                 }
            );

        }
        internal const short TestYear = 1984; 
        internal static void insertMarketHoliday()
        {
            List<DateTime> holidaysToAdd = new List<DateTime>()
            {
                new DateTime(TestYear,1,1),
                new DateTime(TestYear,1,2),
                new DateTime(TestYear,1,15),
                new DateTime(TestYear,1,16),
                new DateTime(TestYear,2,11),
                new DateTime(TestYear,3,20),
                new DateTime(TestYear,4,29),
                new DateTime(TestYear,4,30),
                new DateTime(TestYear,5,3),
                new DateTime(TestYear,5,5),
                new DateTime(TestYear,9,15),
                new DateTime(TestYear,9,23),
                new DateTime(TestYear,9,24),
                new DateTime(TestYear,10,10),
                new DateTime(TestYear,11,03),
                new DateTime(TestYear,11,23)
            };
            GyomuDataAccess.InsertHoliday("JP", TestYear, holidaysToAdd);
            List<DateTime> insertedData=GyomuDataAccess.ListHoliday("JP").Where(d=>d.Year==TestYear).ToList();
            Assert.Equal(holidaysToAdd.Count, insertedData.Count);
            foreach (DateTime holiday in holidaysToAdd)
                Assert.Contains(holiday, insertedData);
        }
        internal static void deleteMarketHoliday()
        {
            GyomuDataAccess.DeleteHoliday("JP", TestYear);
            Assert.Empty(GyomuDataAccess.ListHoliday("JP").Where(d => d.Year == TestYear));
        }
        #endregion
        #region MilestoneDaily
        internal static DateTime targetMilestoneDate = new DateTime(TestYear, 5, 30);

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertMilestoneDailyTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteMilestoneDaily,
                insertMilestoneDaily,
                //deleteMilestoneDaily
                 }
            );

        }
        internal static void insertMilestoneDaily()
        {
            string[] milestones = new string[] { "Test Milestone", "Test Milestone2" };
            Gyomu.Common.GyomuDataAccess.InsertMilestoneDaily(targetMilestoneDate, milestones[0]);
            Gyomu.Common.GyomuDataAccess.InsertMilestoneDaily(targetMilestoneDate, milestones[1]);

            List<Models.MilestoneDaily> milestoneDailies = GyomuDataAccess.SelectMilestoneDaily(targetMilestoneDate);
            foreach(Models.MilestoneDaily daily in milestoneDailies)
            {
                Assert.Contains<string>(daily.milestone_id,milestones);
            }
            Models.MilestoneDaily milestoneDaily = GyomuDataAccess.SelectMilestoneDaily(targetMilestoneDate, milestones[0]);
            Assert.NotNull(milestoneDaily);

            string milestone = "Test Month Milestone";
            GyomuDataAccess.InsertMilestoneDaily(targetMilestoneDate, milestone,true);
            Assert.NotNull(GyomuDataAccess.SelectMilestoneDaily(targetMilestoneDate, milestone, true));
        }
        internal static void deleteMilestoneDaily()
        {
            string[] milestones = new string[] { "Test Milestone", "Test Milestone2" };
            
            GyomuDataAccess.DeleteMilestoneDaily(targetMilestoneDate, milestones[0]);
            GyomuDataAccess.DeleteMilestoneDaily(targetMilestoneDate, milestones[1]);
            Assert.Empty(GyomuDataAccess.SelectMilestoneDaily(targetMilestoneDate));

            string milestone = "Test Month Milestone";
            GyomuDataAccess.DeleteMilestoneDaily(targetMilestoneDate, milestone,true);
            Assert.Null(GyomuDataAccess.SelectMilestoneDaily(targetMilestoneDate, milestone, true));
        }

        #endregion
        #region VariableParameter
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void FetchVariableParameterTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                fetchVariableParameter
                 }
            );

        }
        private static void fetchVariableParameter()
        {
            List<Models.VariableParameter> parameters = GyomuDataAccess.GetVariableParameters();
            Models.VariableParameter param = parameters.Where(p => p.variable_key.Equals("TODAY")).FirstOrDefault();
            Assert.NotNull(param);
        }
        #endregion
        #region TaskInfo

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskInfoTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskInfo,
                insertTaskInfo,
                deleteTaskInfo
                 }
            );

        }
        internal static void insertTaskInfo()
        {
            Gyomu.Common.GyomuDataAccess.InsertTaskInfo(testApplicationId, testTaskID1, "Test Task1", "Gyomu.Test", "Gyomu.Test.Common.TestTask1", true);
            Gyomu.Common.GyomuDataAccess.InsertTaskInfo(testApplicationId, testTaskID2, "Test Task2", "Gyomu.Test", "Gyomu.Test.Common.TestTask2", true);

            Models.TaskInfo task1 = GyomuDataAccess.SelectTaskInfo(testApplicationId, testTaskID1);
            Models.TaskInfo task2 = GyomuDataAccess.SelectTaskInfo(testApplicationId, testTaskID2);
            List<Models.TaskInfo> tasks = GyomuDataAccess.SelectTaskInfoByApplication(testApplicationId);
            Assert.Equal(task1, tasks.Where(t => t.application_id == testApplicationId && t.task_id == testTaskID1).FirstOrDefault(), new TaskComparer());
            Assert.Equal(task2, tasks.Where(t => t.application_id == testApplicationId && t.task_id == testTaskID2).FirstOrDefault(), new TaskComparer());


            tasks = GyomuDataAccess.SelectAllTaskInfo();
            Assert.Equal(task1, tasks.Where(t => t.application_id == testApplicationId && t.task_id == testTaskID1).FirstOrDefault(), new TaskComparer());
            Assert.Equal(task2, tasks.Where(t => t.application_id == testApplicationId && t.task_id == testTaskID2).FirstOrDefault(), new TaskComparer());
        }
        internal static void deleteTaskInfo()
        {


            GyomuDataAccess.DeleteTaskInfo(testApplicationId, testTaskID1);
            GyomuDataAccess.DeleteTaskInfo(testApplicationId, testTaskID2);

            Assert.Empty(GyomuDataAccess.SelectTaskInfoByApplication(testApplicationId));

            
        }

        #endregion
        #region TaskAccessList

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskAccessList(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskAccessList,
                insertTaskAccessList,
                deleteTaskAccessList
                 }
            );

        }
        internal static void insertTaskAccessList()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Models.TaskAccessList access1 = new Models.TaskAccessList()
            {
                application_id = testApplicationId,
                task_info_id = testTaskID1,
                account_name = config.Username,
                can_access = true,
                forbidden = false
            };
            Models.TaskAccessList access2 = new Models.TaskAccessList()
            {
                application_id = testApplicationId,
                task_info_id = testTaskID1,
                account_name = config.Username+"TEST",
                can_access = true,
                forbidden = false
            };
            GyomuDataAccess.InsertTaskAccessList(access1);
            GyomuDataAccess.InsertTaskAccessList(access2);
            Models.TaskAccessList accessout=GyomuDataAccess.SelectTaskAccessList(access1.id);
            List<Models.TaskAccessList> accessLists= GyomuDataAccess.SelectTaskAccessLists(testApplicationId,1);
            Assert.Equal(access1, accessout, new TaskComparer());
            Assert.Equal(access1, accessLists.Where(t => t.id== access1.id).FirstOrDefault(), new TaskComparer());
            Assert.Equal(access2, accessLists.Where(t => t.id == access2.id).FirstOrDefault(), new TaskComparer());
        }
        internal static void deleteTaskAccessList()
        {
            List<Models.TaskAccessList> accessLists = GyomuDataAccess.SelectTaskAccessLists(testApplicationId,1);
            foreach (TaskAccessList access in accessLists)
                GyomuDataAccess.DeleteTaskAccessList(access);

            Assert.Empty(GyomuDataAccess.SelectTaskAccessLists(testApplicationId,1));


        }

        #endregion
        #region TaskData

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskData(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskData,
                insertTaskData,
                deleteTaskData
                 }
            );

        }
        internal static void insertTaskData()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Models.TaskData task1 = new Models.TaskData()
            {
                application_id = testApplicationId,
                task_info_id = testTaskID1,
                entry_author=config.Username,
                parameter="Test1",
                
            };
            Models.TaskData task2 = new Models.TaskData()
            {
                application_id = testApplicationId,
                task_info_id = testTaskID1,
                entry_author = config.Username,
                parameter = "Test1",
            };
            GyomuDataAccess.InsertTaskData(ref task1);

            GyomuDataAccess.InsertTaskData(ref task2);

            TaskData outData = GyomuDataAccess.SelectTaskData(task1.id);
            Assert.Equal(task1, outData, new TaskComparer());
            outData = GyomuDataAccess.SelectTaskData(task2.id);
            Assert.Equal(task2, outData, new TaskComparer());
            
        }

        internal static void deleteTaskData()
        {
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = DateTime.UtcNow;
            List<Models.TaskData> taskDataList = GyomuDataAccess.SelectTaskDataByApplicationIDTaskIDDateRange(
                testApplicationId, testTaskID1, startDate, endDate);
            foreach (TaskData taskData in taskDataList)
                GyomuDataAccess.DeleteTaskData(taskData);

            Assert.Empty(GyomuDataAccess.SelectTaskDataByApplicationIDTaskIDDateRange(
                testApplicationId, testTaskID1, startDate, endDate));

            taskDataList = GyomuDataAccess.SelectTaskDataByApplicationIDTaskIDDateRange(
                testApplicationId, testTaskID2, startDate, endDate);
            foreach (TaskData taskData in taskDataList)
                GyomuDataAccess.DeleteTaskData(taskData);

            Assert.Empty(GyomuDataAccess.SelectTaskDataByApplicationIDTaskIDDateRange(
                testApplicationId, testTaskID2, startDate, endDate));


        }

        #endregion
        #region TaskInstance

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskInstance(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskInstance,
                insertTaskInstance,
                deleteTaskInstance
                 }
            );

        }
        internal const long testTaskDataID = long.MaxValue;
        internal static void insertTaskInstance()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Models.TaskInstance task1 = new Models.TaskInstance()
            {
                task_data_id= testTaskDataID,
                task_status="EXEC",
                comment="NON",
                entry_author=config.Username,
                is_done=false,
                parameter="PARAM",
                

            };
            Models.TaskInstance task2 = new Models.TaskInstance()
            {
                task_data_id = testTaskDataID,
                task_status = "COMP",
                comment = "asd",
                entry_author = config.Username,
                is_done = true,
                parameter = "PARAM2",


            };
            GyomuDataAccess.InsertTaskInstance(ref task1);

            GyomuDataAccess.InsertTaskInstance(ref task2);

            TaskInstance outInstance = GyomuDataAccess.SelectTaskInstance(task1.id);
            Assert.Equal(task1, outInstance, new TaskComparer());
            outInstance = GyomuDataAccess.SelectTaskInstance(task2.id);
            Assert.Equal(task2, outInstance, new TaskComparer());

        }

        internal static void deleteTaskInstance()
        {
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = DateTime.UtcNow;
            TaskData taskData = new TaskData() { id = testTaskDataID };
            List<Models.TaskInstance> taskInstanceList = GyomuDataAccess.SelectTaskInstanceByTaskData(taskData);
            foreach (TaskInstance taskInstance in taskInstanceList)
                GyomuDataAccess.DeleteTaskInstance(taskInstance);

            Assert.Empty(GyomuDataAccess.SelectTaskInstanceByTaskData(taskData));

        }

        #endregion
        #region TaskDataStatus

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskDataStatus(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskDataStatus,
                insertTaskDataStatus,
                deleteTaskDataStatus
                 }
            );

        }
        internal static void insertTaskDataStatus()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Models.TaskDataStatus task1 = new Models.TaskDataStatus()
            {
                task_data_id = testTaskDataID,
                task_status = "EXEC",
                latest_task_instance_id=1,
                latest_update_date=DateTime.UtcNow
            };
            
            GyomuDataAccess.InsertTaskStatus(task1);
            TaskData taskData = new TaskData() { id = testTaskDataID };
            TaskDataStatus outStatus = GyomuDataAccess.SelectTaskStatus(taskData);
            Assert.Equal(task1, outStatus, new TaskComparer());

        }

        internal static void deleteTaskDataStatus()
        {
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = DateTime.UtcNow;
            TaskData taskData = new TaskData() { id = testTaskDataID };
            Models.TaskDataStatus taskStatus = GyomuDataAccess.SelectTaskStatus(taskData);
            if(taskStatus!=null)
                GyomuDataAccess.DeleteTaskStatus(taskStatus);

            Assert.Null(GyomuDataAccess.SelectTaskStatus(taskData));

        }

        #endregion
        #region Create task

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void CreateTask(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskDataStatus,
                deleteTaskInstance,
                deleteTaskData,
                createTask,
                deleteTaskData,
                deleteTaskInstance,
                deleteTaskDataStatus
                 }
            );

        }
        internal static void createTask()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            GyomuDataAccess.CreateNewTask(
                new TaskInfo()
                {
                    application_id = testApplicationId,
                    task_id = testTaskID1,
                }, config, "Parameter1", "Comment1",null, out Models.TaskData taskData, out TaskInstance taskInstance,
                out TaskDataStatus taskStatus);

            TaskData outData = GyomuDataAccess.SelectTaskData(taskData.id);
            TaskInstance outInstance = GyomuDataAccess.SelectTaskInstance(taskInstance.id);
            TaskDataStatus outStatus = GyomuDataAccess.SelectTaskStatus(outData);
            Assert.Equal(taskData, outData, new TaskComparer());
            Assert.Equal(taskInstance, outInstance, new TaskComparer());
            Assert.Equal(taskStatus, outStatus, new TaskComparer());

            GyomuDataAccess.DeleteTaskData(taskData);
            GyomuDataAccess.DeleteTaskInstance(taskInstance);
            GyomuDataAccess.DeleteTaskStatus(taskStatus);
            Assert.Null(GyomuDataAccess.SelectTaskData(taskData.id));
            Assert.Null(GyomuDataAccess.SelectTaskInstance(taskInstance.id));
            Assert.Null(GyomuDataAccess.SelectTaskStatus(taskData));
        }


        #endregion
        #region TaskDataLog

        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertTaskDataLog(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                deleteTaskDataLog,
                insertTaskDataLog,
                deleteTaskDataLog
                 }
            );

        }
        internal static void insertTaskDataLog()
        {
            Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Models.TaskDataLog log1 = new Models.TaskDataLog()
            {
                task_data_id = testTaskDataID,
                log="TEST LOG"
            };

            GyomuDataAccess.InsertTaskDataLog(ref log1);
            Assert.Equal("TEST LOG",log1.log);


        }

        internal static void deleteTaskDataLog()
        {            
            List<Models.TaskDataLog> taskLogList= GyomuDataAccess.SelectTaskLogs(testTaskDataID);
            foreach (TaskDataLog log in taskLogList)
                GyomuDataAccess.DeleteTaskDataLog(log);

            Assert.Empty(GyomuDataAccess.SelectTaskLogs(testTaskDataID));

        }

        #endregion
    }
    public class TestTask1
    {

    }
    public class TaskComparer : IEqualityComparer<Models.TaskInfo>, IEqualityComparer<Models.TaskAccessList>, IEqualityComparer<TaskData>,IEqualityComparer<TaskInstance>,IEqualityComparer<TaskDataStatus>
    {
        public bool Equals(TaskInfo x, TaskInfo y)
        {
            return x.application_id == y.application_id
                && x.task_id == y.task_id
                && (string.IsNullOrEmpty(x.description) == string.IsNullOrEmpty(y.description) && x.description.Equals(y.description))
                && (string.IsNullOrEmpty(x.assembly_name) == string.IsNullOrEmpty(y.assembly_name) && x.assembly_name.Equals(y.assembly_name))
                && (string.IsNullOrEmpty(x.class_name) == string.IsNullOrEmpty(y.class_name) && x.class_name.Equals(y.class_name))
                && x.restartable == y.restartable;
        }

        public bool Equals(TaskAccessList x, TaskAccessList y)
        {
            return x.application_id == y.application_id
                && x.task_info_id == y.task_info_id
                && x.account_name.Equals(y.account_name)
                && x.can_access == y.can_access
                && x.forbidden == y.forbidden;
        }

        public bool Equals(TaskData x, TaskData y)
        {
            return x.application_id == y.application_id
                && x.task_info_id == y.task_info_id
                && (string.IsNullOrEmpty(x.parameter) == string.IsNullOrEmpty(y.parameter) && x.parameter.Equals(y.parameter))
                && x.id == y.id
              //  && x.entry_date.Equals(y.entry_date)
                && x.entry_author.Equals(y.entry_author)
                && (x.parent_task_data_id ?? -1) == (y.parent_task_data_id ?? -1);
        }

        public bool Equals(TaskInstance x, TaskInstance y)
        {
            return x.id == y.id
                && x.task_data_id == y.task_data_id
             //   && x.entry_date.Equals(y.entry_date)
                && x.entry_author.Equals(y.entry_author)
                && x.task_status.Equals(y.task_status)
                && x.is_done == y.is_done
                && (string.IsNullOrEmpty(x.parameter) == string.IsNullOrEmpty(y.parameter) && x.parameter.Equals(y.parameter))
                && (string.IsNullOrEmpty(x.comment) == string.IsNullOrEmpty(y.comment) && x.comment.Equals(y.comment));
        }

        public bool Equals(TaskDataStatus x, TaskDataStatus y)
        {
            return x.task_data_id == y.task_data_id
                && x.latest_task_instance_id == y.latest_task_instance_id
                && x.task_status.Equals(y.task_status)
                && x.latest_update_date.Equals(y.latest_update_date);
        }

        public int GetHashCode(TaskInfo obj)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(TaskAccessList obj)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(TaskData obj)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(TaskInstance obj)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(TaskDataStatus obj)
        {
            throw new NotImplementedException();
        }
    }
}
