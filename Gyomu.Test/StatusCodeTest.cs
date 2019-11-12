using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using Xunit;
using Gyomu.Test.Common;

namespace Gyomu.Test
{
    public class StatusCodeTest
    {
        private const short testApplicationId = 32650;
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void RegisterStatusCodeTest(Gyomu.Common.SettingItem.DBType db)
        {
            DBConnectionFactoryTest.LockProcess(db, new Action[] {
                GyomuDataAccessTest.deleteApplicationInfo, GyomuDataAccessTest.insertApplicationInfo,
                GyomuDataAccessTest.deleteStatusHandler,GyomuDataAccessTest.insertStatusHandler,
                registerStatusCode,
                GyomuDataAccessTest.deleteStatusInfo,
                GyomuDataAccessTest.deleteStatusHandler, GyomuDataAccessTest.deleteApplicationInfo }
            );
        }
        private static void registerStatusCode()
        {
            Gyomu.Common.Configurator config = Gyomu.Common.BaseConfigurator.GetInstance();
            config.ApplicationID = Common.GyomuDataAccessTest.testApplicationId;
            StatusCode retVal = Gyomu.StatusCode.Debug("Debug Test", config);
            Console.WriteLine(retVal.StatusID);
        }
    }
}
