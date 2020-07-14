using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    public interface IIniCacher
    {
        void ProcessProperty(FileSystemPath file, string section, string key, IniPropertyOperators op, IniCachedItem value);

        bool IsEmpty { get; }
    }
}