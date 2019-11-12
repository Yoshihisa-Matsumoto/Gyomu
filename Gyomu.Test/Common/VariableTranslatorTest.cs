using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Gyomu.Test.Common
{
    public class VariableTranslatorTest
    {
        [Theory]
        [InlineData(Gyomu.Common.SettingItem.DBType.MSSQL)]
        [InlineData(Gyomu.Common.SettingItem.DBType.POSTGRESQL)]
        public void ParseTest(Gyomu.Common.SettingItem.DBType db)
        {
            Common.DBConnectionFactoryTest.LockProcess(db, new Action[] {
                Common.GyomuDataAccessTest.deleteMarketHoliday,
                Common.GyomuDataAccessTest.insertMarketHoliday,
                parse,
                Common.GyomuDataAccessTest.deleteMarketHoliday
                 }
            );
        }
        private static void parse()
        {
            Gyomu.Common.VariableTranslator translator = new Gyomu.Common.VariableTranslator();
            DateTime targetDate = new DateTime(GyomuDataAccessTest.TestYear,5,2);

            string parameter = "{%JP$TODAY$yyyyMMdd%}";
            Assert.Equal(targetDate.ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$NEXTBUS$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 7).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$PREVBUS$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 4, 27).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BBOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 2).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BBOY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 1, 4).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 2).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BOY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 1, 2).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BEOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 30).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$BEOY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 12, 28).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));


            parameter = "{%JP$2$EOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 30).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));


            parameter = "{%JP$2$EOY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 12, 30).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));


            parameter = "{%JP$2$NEXTBBOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 6, 4).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$NEXTBUS$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 7).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$NEXTDAY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 5, 4).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$NEXTBEOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 6, 28).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$PREVBUS$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 4, 27).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$PREVDAY$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 4, 30).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

            parameter = "{%JP$2$PREVBEOM$yyyyMMdd%}";
            Assert.Equal(new DateTime(GyomuDataAccessTest.TestYear, 4, 26).ToString("yyyyMMdd"), translator.Parse(parameter, targetDate));

        }
    }
}
