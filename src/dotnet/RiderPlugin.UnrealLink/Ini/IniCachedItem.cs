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
        }

        // used if item is an object with multiple fields
        private Dictionary<string, IniCachedItem> values = new Dictionary<string, IniCachedItem>();
        // used if item is an elementary value (bool, string, etc)
        private string myValue = string.Empty;
        
        /// <summary>
        /// File where current value was acquired
        /// </summary>
        public FileSystemPath File { get; set; }
        
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

        /// <summary>
        /// Constructs string representation of value
        /// </summary>
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
            var clone = new IniCachedItem {File = File};

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