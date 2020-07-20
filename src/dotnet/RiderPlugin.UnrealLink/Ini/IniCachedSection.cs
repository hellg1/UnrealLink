using System.Collections.Generic;

namespace RiderPlugin.UnrealLink.Ini
{
    /// <summary>
    /// Represents ini file section
    /// </summary>
    public class IniCachedSection
    {
        private Dictionary<string, IniCachedProperty> properties = new Dictionary<string, IniCachedProperty>();

        public void ModifyProperty(string key, IniPropertyOperators op, IniCachedItem newValue, string platform)
        {
            if (properties.ContainsKey(key))
            {
                properties[key].ModifyValue(newValue, op, platform);
            }
            else
            {
                var prop = new IniCachedProperty(key);
                prop.ModifyValue(newValue, op, platform);
                properties.Add(key, prop);
            }
        }

        public IniCachedProperty GetProperty(string propertyName)
        {
            if (properties.ContainsKey(propertyName))
            {
                return properties[propertyName];
            }
            
            return null;
        }
     }
}