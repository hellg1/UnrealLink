using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    /// <summary>
    /// Represents property in ini file
    /// </summary>
    public class IniCachedProperty
    {
        public IniCachedProperty(string key)
        {
        }

        public const string DefaultPlatform = "default";
        
        //private List<IniCachedItem> generalValues = new List<IniCachedItem>();
        private Dictionary<string, List<IniCachedItem>> perPlatformValues = new Dictionary<string, List<IniCachedItem>> { { DefaultPlatform, new List<IniCachedItem>()} };

        public void ModifyValue(IniCachedItem newValue, IniPropertyOperators op, string platform = DefaultPlatform)
        {
            if (!perPlatformValues.ContainsKey(platform))
            {
                perPlatformValues.Add(platform, perPlatformValues[DefaultPlatform]
                    .Select(item => item.Clone() as IniCachedItem).ToList());
            }

            switch (op)
            {
                case IniPropertyOperators.New:
                {
                    perPlatformValues[platform] = new List<IniCachedItem> { newValue };
                    break;
                }
                case IniPropertyOperators.Add:
                {
                    perPlatformValues[platform].Add(newValue);
                    break;
                }
                case IniPropertyOperators.AddWithCheck:
                {
                    if (!perPlatformValues[platform].Contains(newValue)) {
                        perPlatformValues[platform].Add(newValue);
                    }
                    break;
                }
                case IniPropertyOperators.RemoveLn:
                {
                    perPlatformValues[platform].RemoveAll(it => it == newValue);
                    break;
                }
                case IniPropertyOperators.RemoveProperty:
                {
                    perPlatformValues[platform] = new List<IniCachedItem>();
                    break;
                }
            }
        }

        public IniCachedItem[] GetValues(string platform = DefaultPlatform)
        {
            if (!perPlatformValues.ContainsKey(platform))
            {
                platform = DefaultPlatform;
            }
            
            if (perPlatformValues.ContainsKey(platform))
            {
                return perPlatformValues[platform].ToArray();
            }

            return null;
        }

        public bool IsEmpty
        {
            get
            {
                var res = true;
                foreach (var platform in perPlatformValues)
                {
                    res &= platform.Value.IsEmpty();
                }

                return res;
            }
        }
    }

    public enum IniPropertyOperators
    {
        New,
        Add,
        AddWithCheck,
        RemoveLn,
        RemoveProperty
    }
}