using Dapper.FastCrud;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class BaseDapperFastCrud<T>
    {
        private static Dictionary<Common.SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<T>> InfoMapping = new Dictionary<Common.SettingItem.DBType, Dapper.FastCrud.Mappings.EntityMapping<T>>();
        static BaseDapperFastCrud()
        {
            InfoMapping.Add(Common.SettingItem.DBType.MSSQL,
                OrmConfiguration.GetDefaultEntityMapping<T>()
                    .Clone().SetDialect(SqlDialect.MsSql));
            InfoMapping.Add(Common.SettingItem.DBType.POSTGRESQL,
                OrmConfiguration.GetDefaultEntityMapping<T>()
                    .Clone().SetDialect(SqlDialect.PostgreSql));
        }
            
        internal static Dapper.FastCrud.Mappings.EntityMapping<T> GetMapping()
        {
            return InfoMapping[Common.DBConnectionFactory.SQLDB];
        }
    }
}
