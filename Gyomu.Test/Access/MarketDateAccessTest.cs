using Gyomu.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Access
{
    public class MarketDateAccessTest
    {
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void BusinessDayTest(SettingItem.DBType db)
        {
            Common.DBConnectionFactoryTest.LockProcess(db, new Action[] {
                Common.GyomuDataAccessTest.deleteMarketHoliday,
                Common.GyomuDataAccessTest.insertMarketHoliday,
                businessDayTest,
                Common.GyomuDataAccessTest.deleteMarketHoliday
                 }
            );

        }
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void MonthTest(SettingItem.DBType db)
        {
            Common.DBConnectionFactoryTest.LockProcess(db, new Action[] {
                Common.GyomuDataAccessTest.deleteMarketHoliday,
                Common.GyomuDataAccessTest.insertMarketHoliday,
                businessDayTest,
                Common.GyomuDataAccessTest.deleteMarketHoliday
                 }
            );

        }
        private static void businessDayTest()
        {
            Gyomu.Access.MarketDateAccess marketAccess = new Gyomu.Access.MarketDateAccess(Gyomu.Access.MarketDateAccess.SupportMarket.Japan);

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 4, 27),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 1), -1)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 4),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 7), -1)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 7), -2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 1),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 4, 27), 1)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 7),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 4), 1)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 7),
                marketAccess.GetBusinessDay(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2), 2)
                );



        }
        private static void monthDayTest()
        {
            Gyomu.Access.MarketDateAccess marketAccess = new Gyomu.Access.MarketDateAccess(Gyomu.Access.MarketDateAccess.SupportMarket.Japan);

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2),
                marketAccess.GetBusinessDayOfBeginningMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 30),
                marketAccess.GetBusinessDayOfEndMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 6, 4),
                marketAccess.GetBusinessDayOfNextBeginningMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 6, 4),
                marketAccess.GetBusinessDayOfNextBeginningMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 1), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 6, 4),
                marketAccess.GetBusinessDayOfNextBeginningMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 31), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 30),
                marketAccess.GetBusinessDayOfNextEndMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 2), 2)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 6, 28),
                marketAccess.GetBusinessDayOfNextEndMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 31), 2)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 6, 28),
                marketAccess.GetBusinessDayOfNextEndMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 30), 2)
                );

            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 4, 3),
                marketAccess.GetBusinessDayOfPreviousBeginningMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 30), 2)
                );
            Assert.Equal(new DateTime(Common.GyomuDataAccessTest.TestYear, 4, 27),
                marketAccess.GetBusinessDayOfPreviousEndMonthWithOffset(
                    new DateTime(Common.GyomuDataAccessTest.TestYear, 5, 30), 2)
                );
        }
    }
}
