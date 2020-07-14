using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;
using NuGet;

namespace RiderPlugin.UnrealLink.Ini
{
    public class ClassDefaultsCache : IIniCacher
    {
        public ClassDefaultsCache(string projectName)
        {
            sectionPrefix = "/Script/" + projectName;
        }

        private string sectionPrefix;

        private Dictionary<string, IniCachedSection> sections = new Dictionary<string,IniCachedSection>();
        
        public void ProcessProperty(FileSystemPath file, string section, string key, IniPropertyOperators op, IniCachedItem value)
        {
            if (!section.StartsWith(sectionPrefix))
            {
                return;
            }

            if (sections.ContainsKey(section))
            {
                sections[section].ModifyProperty(key, op, value);
            }
            else
            {
                var cachedSection = new IniCachedSection();
                cachedSection.ModifyProperty(key, op, value);
                sections.Add(section, cachedSection);
            }
        }

        public IniCachedSection GetClassDefaults(string className)
        {
            if (sections.ContainsKey(className))
            {
                return sections[className];
            }

            return null;
        }

        public IniCachedProperty GetClassProperty(string className, string propertyName)
        {
            if (!sections.ContainsKey(className))
            {
                return null;
            }

            var classSection = sections[className];
            return classSection.GetProperty(propertyName);
        }

        public string GetClassDefaultValue(string className, string propertyName)
        {
            if (!sections.ContainsKey(className))
            {
                return null;
            }

            var vals = sections[className].GetProperty(propertyName).GetValues();
            if (vals.IsEmpty())
            {
                return null;
            }

            if (vals.Last().IsObject)
            {
                return null;
            }

            return vals.Last().Value;
        }
        
        public bool IsEmpty => sections.IsEmpty();
    }
}