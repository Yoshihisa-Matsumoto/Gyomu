using Gyomu.Common;
using Gyomu.Test.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Access
{
    public class MilestoneAccessTest
    {
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void InsertMilestoneDailyTest(SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                GyomuDataAccessTest.deleteMilestoneDaily,
                milestoneRegisterTest,
                GyomuDataAccessTest.deleteMilestoneDaily
                 }
            );

        }
        private static void milestoneRegisterTest()
        {
            string[] milestones = new string[] { "Test Milestone", "Test Milestone2" };
            Gyomu.Access.MilestoneAccess.Register(milestones[0], Common.GyomuDataAccessTest.targetMilestoneDate);
            Assert.True(Gyomu.Access.MilestoneAccess.Exists(milestones[0], Common.GyomuDataAccessTest.targetMilestoneDate));
            string milestone = "Test Month Milestone";
            Gyomu.Access.MilestoneAccess.Register(milestone, Common.GyomuDataAccessTest.targetMilestoneDate,true);
            Assert.True(Gyomu.Access.MilestoneAccess.Exists(milestone, Common.GyomuDataAccessTest.targetMilestoneDate, true));
            Assert.True(Gyomu.Access.MilestoneAccess.Wait(milestones[0], Common.GyomuDataAccessTest.targetMilestoneDate, 1));

            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(3000);
                Gyomu.Access.MilestoneAccess.Register(milestones[1], Common.GyomuDataAccessTest.targetMilestoneDate);
            });
            Assert.True(Gyomu.Access.MilestoneAccess.Wait(milestones[1], Common.GyomuDataAccessTest.targetMilestoneDate, 1));

        }
    }
}
