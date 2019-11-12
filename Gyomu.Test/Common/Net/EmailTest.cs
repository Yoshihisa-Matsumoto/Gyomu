using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Gyomu.Test.Common;

namespace Gyomu.Test.Common.Net
{
    public class EmailTest
    {
        
        //[Theory]
        //[InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        //[InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void EmailSendTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                GyomuDataAccessTest.deleteApplicationInfo, GyomuDataAccessTest.insertApplicationInfo,
                GyomuDataAccessTest.deleteStatusHandler,GyomuDataAccessTest.insertStatusHandler,
                emailSend,
                //GyomuDataAccessTest.deleteStatusInfo,
                GyomuDataAccessTest.deleteStatusHandler, GyomuDataAccessTest.deleteApplicationInfo }
            );
        }
        private void emailSend()
        {
            Gyomu.Common.Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            Gyomu.Common.Net.Email email = new Gyomu.Common.Net.Email(Common.GyomuDataAccessTest.testApplicationId, config);
            Gyomu.Common.WindowsUser windowsUser = Gyomu.Common.WindowsUser.CurrentWindowsUser;
            Gyomu.StatusCode statusCode= email.Send(windowsUser.MailAddress, windowsUser.DisplayName, new string[] { windowsUser.MailAddress }, new string[] { windowsUser.MailAddress }, "Test Subject", "Test Body", null);
            Assert.True(statusCode.IsSucceeded);
        }
    }
}
