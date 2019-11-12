using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class MilestoneDaily
    {
        public string target_date { get; set; }
        public string milestone_id { get; set; }
        [Computed]
        public DateTime update_time { get; set; }
    }
}
