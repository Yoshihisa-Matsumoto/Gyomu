using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_market_holiday")]
    public partial class MarketHoliday:BaseDapperFastCrud<MarketHoliday>
    {
        [Key]
        public virtual string market { get; set; }
        public virtual short year { get; set; }
        [Key]
        public virtual string holiday { get; set; }
    }
}
