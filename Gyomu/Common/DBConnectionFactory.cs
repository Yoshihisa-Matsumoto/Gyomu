using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace Gyomu.Common
{

    public class DBConnectionFactory
    {
        //private static string _sqldb = null;
        internal static Common.SettingItem.DBType SQLDB
        {
            get
            {
                string sqldb = null;
                if (sqldb == null)
                {
                    sqldb = System.Environment.GetEnvironmentVariable(Common.SettingItem.GYOMU_COMMON_MAINDB_TYPE);
                }
                if(string.IsNullOrEmpty(sqldb))
                    throw new InvalidOperationException("Environment Variable GYOMU_COMMON_MAINDB_TYPE not set");
                try
                {
                    SettingItem.DBType dbType = Access.EnumAccess.Parse<SettingItem.DBType>(sqldb);
                    return dbType;
                }
                catch (Exception)
                {
                    return SettingItem.DBType.Other;
                }
            }
        }

        internal static IDbConnection GetGyomuConnection()
        {
            switch (SQLDB)
            {
                case Common.SettingItem.DBType.POSTGRESQL:
                    return new Npgsql.NpgsqlConnection(System.Environment.GetEnvironmentVariable(Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION));
                case Common.SettingItem.DBType.MSSQL:
                    return new System.Data.SqlClient.SqlConnection(System.Environment.GetEnvironmentVariable(Common.SettingItem.GYOMU_COMMON_MAINDB_CONNECTION));
                default:
                    throw new InvalidOperationException("Environment Variable GYOMU_COMMON_MAINDB_CONNECTION not have proper setting");
            }
        }
    }
}
