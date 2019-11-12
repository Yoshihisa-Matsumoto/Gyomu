using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_info_access_list")]
    public class TaskAccessList
    {
        [Key]
        public long id { get; set; }
        public short application_id { get; set; }
        public short task_info_id { get; set; }
        public string account_name { get; set; }
        public bool can_access { get; set; }
        public bool forbidden { get; set; }
    }
}
