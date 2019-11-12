using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_data")]
    public class TaskData
    {
        [Key]
        public long id { get; set; }
        public short application_id { get; set; }
        public short task_info_id { get; set; }
        [Computed]
        public DateTime entry_date { get; set; }
        public string entry_author { get; set; }
        public long? parent_task_data_id { get; set; }
        public string parameter { get; set; }
    }
}
