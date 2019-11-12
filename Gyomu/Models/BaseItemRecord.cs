using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Serializable]
    public abstract class BaseItemRecord
    {
        public abstract object this[int key] { get; }
        public abstract object Key { get; }
        public abstract int[] Fields { get; }
        public abstract void Add(int key, object value);
    }
}
