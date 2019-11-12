using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace Gyomu.Test.Common
{
    public class DBConnectionFactoryTest
    {
        [Fact]
        public void SQLDBSettingExistTest()
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, Gyomu.Access.EnumAccess.GetEnumValueDescription(Gyomu.Common.SettingItem.DBType.POSTGRESQL));
                Gyomu.Common.SettingItem.DBType SqlDb = Gyomu.Common.DBConnectionFactory.SQLDB;
                Assert.Equal(Gyomu.Common.SettingItem.DBType.POSTGRESQL,SqlDb);
            }
        }
        [Fact]
        public void SQLDBSettingNonExistTest()
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, "");
                Assert.Throws<InvalidOperationException>(() =>
                {
                    Gyomu.Common.SettingItem.DBType SqlDb = Gyomu.Common.DBConnectionFactory.SQLDB;
                });
            }
        }
        [Fact(DisplayName = "Postgre Connection Test. Must exist on local as default")]
        public void GetPostgreSqlConnectionTest()
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, Gyomu.Access.EnumAccess.GetEnumValueDescription(Gyomu.Common.SettingItem.DBType.POSTGRESQL));
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION, "Server=localhost;Port=5432;User ID=postgres;Database=common;Password=password;Enlist=true");
                using (System.Data.IDbConnection conn = Gyomu.Common.DBConnectionFactory.GetGyomuConnection())
                {
                    Assert.NotNull(conn);
                    conn.Open();
                    Assert.Equal(System.Data.ConnectionState.Open, conn.State);
                }
            }
        }
        internal static object lockObject = new object();
        [Fact(DisplayName = "MSSQL Connection Test. Must exist on local as default")]
        public void GetMSSQLConnectionTest()
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, Gyomu.Access.EnumAccess.GetEnumValueDescription(Gyomu.Common.SettingItem.DBType.MSSQL));
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION, "Server=localhost\\SQLEXPRESS;Database=common;Integrated Security=True;");
                using (System.Data.IDbConnection conn = Gyomu.Common.DBConnectionFactory.GetGyomuConnection())
                {
                    Assert.NotNull(conn);
                    conn.Open();
                    Assert.Equal(System.Data.ConnectionState.Open, conn.State);
                }
            }
        }
        [Fact]
        public void GetInvalidConnectionTest()
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, "Invalid");
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION, "Server=localhost\\SQLEXPRESS;Database=common;Integrated Security=True;");

                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (System.Data.IDbConnection conn = Gyomu.Common.DBConnectionFactory.GetGyomuConnection())
                    { }
                });


            }
        }
        internal static void LockProcess(Gyomu.Common.SettingItem.DBType db, Action method)
        {
            Action[] methods = new Action[] { method };
            LockProcess(db, methods);
        }
        internal static void LockProcess(Gyomu.Common.SettingItem.DBType db,Action[] methods)
        {
            Gyomu.Common.SettingItem.DBType[] supported_database = new Gyomu.Common.SettingItem.DBType[] { Gyomu.Common.SettingItem.DBType.POSTGRESQL, Gyomu.Common.SettingItem.DBType.MSSQL };

            if (supported_database.Contains(db) == false)
                return;

            switch (db)
            {
                case Gyomu.Common.SettingItem.DBType.MSSQL:
                    LockForMSSQLAndProcess(methods);
                    break;
                case Gyomu.Common.SettingItem.DBType.POSTGRESQL:
                    LockForPostgresAndProcess(methods);
                    break;
            }
        }
        internal static void LockForPostgresAndProcess(Action[] methods)
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, Gyomu.Access.EnumAccess.GetEnumValueDescription(Gyomu.Common.SettingItem.DBType.POSTGRESQL));
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION, "Server=localhost;Port=5432;User ID=postgres;Database=common;Password=password;Enlist=true");
                using (System.Data.IDbConnection conn = Gyomu.Common.DBConnectionFactory.GetGyomuConnection())
                {
                    foreach(Action method in methods)
                        method();
                }
            }
        }
        internal static void LockForMSSQLAndProcess(Action[] methods)
        {
            lock (lockObject)
            {
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE, Gyomu.Access.EnumAccess.GetEnumValueDescription(Gyomu.Common.SettingItem.DBType.MSSQL));
                System.Environment.SetEnvironmentVariable(Gyomu.Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION, "Server=localhost\\SQLEXPRESS;Database=common;Integrated Security=True;"); using (System.Data.IDbConnection conn = Gyomu.Common.DBConnectionFactory.GetGyomuConnection())
                {
                    foreach (Action method in methods)
                        method();
                }
            }
        }
    }
}
