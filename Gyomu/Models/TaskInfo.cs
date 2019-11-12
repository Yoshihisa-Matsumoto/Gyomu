using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("task_info_cdtbl")]
    public class TaskInfo
    {
        public short application_id { get; set; }
        public short task_id { get; set; }
        public string description { get; set; }
        public string assembly_name { get; set; }
        public string class_name { get; set; }
        public bool restartable { get; set; }


    }
}
