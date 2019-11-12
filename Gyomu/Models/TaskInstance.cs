using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_instance")]
    public class TaskInstance
    {
        [Key]
        public long id { get; set; }
        public long task_data_id { get; set; }
        [Computed]
        public DateTime entry_date { get; set; }
        public string entry_author { get; set; }
        public string task_status { get; set; }
        public bool is_done { get; set; }
        public long? status_info_id { get; set; }
        public string parameter { get; set; }
        public string comment { get; set; }
    }
}
