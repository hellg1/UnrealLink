using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    /// <summary>
    /// Represents property's value
    /// </summary>
    public class IniCachedItem : ICloneable
    {
        public IniCachedItem()
        {
            Value = string.Empty;
        }

        public FileSystemPath LastFile { get; set; }
        private Dictionary<string, IniCachedItem> values = new Dictionary<string, IniCachedItem>();
        private string myValue;
        
        /// <summary>
        /// Does value contains multiple fields (i.e. object)
        /// </summary>
        public bool IsObject { get; private set; }

        public string Value
        {
            get => ConstructValue().ToString();
            set => myValue = value;
        }

        /// <summary>
        /// Adds field to value
        /// </summary>
        public void AddValue(string key, IniCachedItem value)
        {
            IsObject = true;
            values.Add(key, value);
        }

        private StringBuilder ConstructValue()
        {
            if (!IsObject)
            {
                return new StringBuilder(myValue);
            }
            
            var res = new StringBuilder();
            res.Append("(");
            string comma = "";
            
            foreach (var item in values)
            {
                res.Append(comma);
                res.Append(item.Key);
                res.Append("=");
                res.Append(item.Value.ConstructValue());
                comma = ",";
            }
            
            res.Append(")");
            
            return res;
        }

        public object Clone()
        {
            var clone = new IniCachedItem {LastFile = LastFile};

            if (IsObject)
            {
                foreach (var it in values)
                {
                    clone.AddValue(it.Key, it.Value.Clone() as IniCachedItem);
                }
            }
            else
            {
                clone.Value = myValue;
            }

            return clone;
        }
    }
}