using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini
{
    public interface IIniCacheBuilder
    {
        /// <summary>
        /// Processes property sent by IniVisitor
        /// </summary>
        /// <param name="file">Current file's path</param>
        /// <param name="section">Current section</param>
        /// <param name="key">Current property</param>
        /// <param name="op">Property operation</param>
        /// <param name="value">Property's value</param>
        void ProcessProperty(FileSystemPath file, string section, string key, IniPropertyOperators op, IniCachedItem value);

        /// <summary>
        /// Setup which platform we will process
        /// </summary>
        void SetupPlatform(string platform);
        
        bool IsEmpty { get; }
    }
}