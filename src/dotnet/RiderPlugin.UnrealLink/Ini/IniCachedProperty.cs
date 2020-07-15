using System.Collections.Generic;
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
            Key = key;
        }
        
        /// <summary>
        /// Property's name
        /// </summary>
        public string Key { get; }

        private List<IniCachedItem> values = new List<IniCachedItem>();

        public void ModifyValue(IniCachedItem newValue, IniPropertyOperators op)
        {
            switch (op)
            {
                case IniPropertyOperators.New:
                {
                    values = new List<IniCachedItem> { newValue };
                    break;
                }
                case IniPropertyOperators.Add:
                {
                    values.Add(newValue);
                    break;
                }
                case IniPropertyOperators.AddWithCheck:
                {
                    if (!values.Contains(newValue)) {
                        values.Add(newValue);
                    }
                    break;
                }
                case IniPropertyOperators.RemoveLn:
                {
                    values.RemoveAll(it => it == newValue);
                    break;
                }
                case IniPropertyOperators.RemoveProperty:
                {
                    values = new List<IniCachedItem>();
                    break;
                }
            }
        }

        public IniCachedItem[] GetValues()
        {
            if (IsEmpty)
            {
                return null;
            }

            return values.ToArray();
        }

        public bool IsEmpty => values.IsEmpty();
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