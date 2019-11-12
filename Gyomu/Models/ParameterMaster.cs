using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Contrib.Extensions;

namespace Gyomu.Models
{
    [Table("param_master")]
    public class ParameterMaster
    {
        public string item_key { get; set; }
        public string item_value { get; set; }
        public string item_fromdate { get; set; }
    }
}
