using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_instance_submit_information")]
    public class TaskSubmitInformation
    {
        [Key]
        public long id { get; set; }
        public long task_instance_id { get; set; }
        public string submit_to { get; set; }
    }
}
