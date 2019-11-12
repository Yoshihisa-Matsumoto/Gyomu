using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_data_log")]
    public class TaskDataLog
    {
        [Key]
        public long id { get; set; }
        public long task_data_id { get; set; }
        [Computed]
        public DateTime log_time { get; set; }
        public string log { get; set; }
    }
}
