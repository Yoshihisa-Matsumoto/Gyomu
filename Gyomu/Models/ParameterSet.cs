using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class ParameterSet
    {
        public List<ParameterItem> Items { get; set; }
    }
    public class ParameterItem
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public bool AcceptMultiple { get; set; }
        public List<string> Values { get; set; }
        public string GetValue()
        {
            if (Values == null || Values.Count == 0)
                return null;
            return Values[0];
        }
    }
}
