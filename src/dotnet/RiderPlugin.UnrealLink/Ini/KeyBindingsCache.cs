using System;
using JetBrains.Util;
using RiderPlugin.UnrealLink.Ini.IniLanguage;

namespace RiderPlugin.UnrealLink.Ini
{
    /// <summary>
    /// Class for storing ActionMappings and AxisMappings (i.e. key bindings)
    /// </summary>
    public class KeyBindingsCache : IIniCacher
    {
        public KeyBindingsCache()
        {
            actionMappings = new IniCachedProperty("ActionMappings");
            axisMappings = new IniCachedProperty("AxisMappings");
        }

        private IniCachedProperty actionMappings;
        private IniCachedProperty axisMappings;

        public void ProcessProperty(FileSystemPath file, string section, string key, IniPropertyOperators op, IniCachedItem value)
        {
            if (!file.NameWithoutExtension.EndsWith("Input") || section != "/Script/Engine.InputSettings")
            {
                return;
            }
            
            switch (key)
            {
                case "ActionMappings":
                    actionMappings.ModifyValue(value, op);
                    break;
                case "AxisMappings":
                    axisMappings.ModifyValue(value, op);
                    break;
            }
        }

        public bool IsEmpty => actionMappings.IsEmpty && axisMappings.IsEmpty;
    }
}