using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("status_handler")]
    public class StatusHandler
    {
        [Key]
        public int id { get; set; }
        public short application_id { get; set; }
        public string region { get; set; }
        public short status_type { get; set; }
        public string recipient_address { get; set; }
        public string recipient_type { get; set; }
    }
}
