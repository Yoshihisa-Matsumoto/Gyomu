using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class MarketHoliday
    {
        public string market { get; set; }
        public short year { get; set; }
        public string holiday { get; set; }
    }
}
