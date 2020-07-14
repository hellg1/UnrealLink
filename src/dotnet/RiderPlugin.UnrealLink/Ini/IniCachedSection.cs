using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    public class IniCachedSection
    {
        private Dictionary<string, IniCachedProperty> properties = new Dictionary<string, IniCachedProperty>();

        public void ModifyProperty(string key, IniPropertyOperators op, IniCachedItem newValue)
        {
            if (properties.ContainsKey(key))
            {
                properties[key].ModifyValue(newValue, op);
            }
            else
            {
                var prop = new IniCachedProperty(key);
                prop.ModifyValue(newValue, op);
                properties.Add(key, prop);
            }
        }

        public IniCachedProperty GetProperty(string property)
        {
            if (properties.ContainsKey(property))
            {
                return properties[property];
            }
            
            return null;
        }
     }
}