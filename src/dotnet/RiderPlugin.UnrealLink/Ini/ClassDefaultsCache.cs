using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    public class ClassDefaultsCache : IIniCacheBuilder, ICloneable
    {
        public ClassDefaultsCache(string projectName)
        {
            sectionPrefix = "/Script/" + projectName;
        }

        private readonly string sectionPrefix;

        private string curPlatform = IniCachedProperty.DefaultPlatform;
        
        private Dictionary<string, IniCachedSection> sections = new Dictionary<string,IniCachedSection>();
        
        public void ProcessProperty(FileSystemPath file, string section, string key, IniPropertyOperators op, IniCachedItem value)
        {
            if (!section.StartsWith(sectionPrefix))
            {
                return;
            }

            var className = section.Substring(sectionPrefix.Length + 1);
            
            if (sections.ContainsKey(className))
            {
                sections[className].ModifyProperty(key, op, value, curPlatform);
            }
            else
            {
                var cachedSection = new IniCachedSection();
                cachedSection.ModifyProperty(key, op, value, curPlatform);
                sections.Add(className, cachedSection);
            }
        }

        public void SetupPlatform(string platform)
        {
            curPlatform = platform;
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

        public string GetClassDefaultValue(string className, string propertyName, string platform = IniCachedProperty.DefaultPlatform)
        {
            if (!sections.ContainsKey(className))
            {
                return null;
            }

            var vals = sections[className].GetProperty(propertyName).GetValues(platform);
            if (vals.IsEmpty())
            {
                return null;
            }

            return vals.Last().Value;
        }
        
        public bool IsEmpty => sections.IsEmpty();

        public object Clone()
        {
            var copy = new ClassDefaultsCache(sectionPrefix.Substring(8));

            foreach (var item in sections)
            {
                copy.sections.Add(item.Key, item.Value.Clone() as IniCachedSection);
            }

            return copy;
        }
    }
}