using System.Collections.Generic;
using System.Text;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    public class IniCachedItem
    {
        public IniCachedItem()
        {
            Value = string.Empty;
        }

        public FileSystemPath LastFile { get; set; }
        private Dictionary<string, IniCachedItem> values = new Dictionary<string, IniCachedItem>();
        private string myValue;
        
        public bool IsObject { get; set; }

        public string Value
        {
            get => ConstructValue().ToString();
            set => myValue = value;
        }

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
            
            foreach (var item in values)
            {
                res.Append(item.Key);
                res.Append("=");
                res.Append(item.Value.ConstructValue());
                res.Append(",");
            }

            res.Append(")");
            
            return res;
        }
    }
}