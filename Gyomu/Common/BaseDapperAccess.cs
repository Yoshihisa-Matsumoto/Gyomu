using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gyomu.Common
{
    public class BaseDapperAccess
    {
        public static void RegisterCustomMappingType(Type t)
        {
            Dapper.SqlMapper.SetTypeMap(
    t,
    new CustomPropertyTypeMap(
        t,
        (type, columnName) =>
            type.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .Any(attr => attr.Name == columnName))));
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName)
        {
            Name = columnName;
        }
        public string Name { get; set; }
    }
}
