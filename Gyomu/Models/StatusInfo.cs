using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("status_info")]
    public class StatusInfo
    {
        [Key]
        public long id { get; set; }
        public short application_id { get; set; }
        [Computed]
        public DateTime entry_date { get; set; }
        public string entry_author { get; set; }
        public short status_type { get; set; }
        public short error_id { get; set; }
        public int instance_id { get; set; }
        public string hostname { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public string developer_info { get; set; }

    }
}
