using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class ServiceStatus
    {
        public short ID { get; set; }
        public string Description { get; set; }
        public string Parameter { get; set; }
        public string Status { get; set; }
        public string CurrentTask { get; set; }
        public int? Progress { get; set; }
        public int? TaskCount { get;set; }
    }
}
