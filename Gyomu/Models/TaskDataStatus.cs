using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_data_status")]
    public class TaskDataStatus
    {
        [ExplicitKey]
        public long task_data_id { get; set; }
        public string task_status { get; set; }
        public DateTime latest_update_date { get; set; }
        public long latest_task_instance_id { get; set; }
    }
}
