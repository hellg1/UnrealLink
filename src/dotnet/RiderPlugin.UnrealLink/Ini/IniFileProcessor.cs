using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.Text;
using JetBrains.Util;
using RiderPlugin.UnrealLink.Ini.IniLanguage;
using RiderPlugin.UnrealLink.PluginInstaller;


namespace RiderPlugin.UnrealLink.Ini
{
    
    /// <summary>
    /// Class for processing ini files from project directory
    /// TODO: make API for accessing cached data
    /// </summary>
    [SolutionComponent]
    public class IniFileProcessor
    {
        private static HashSet<string> platformNames = new HashSet<string>
        {
            IniCachedProperty.DefaultPlatform, "android", "hololens", "ios", "linux", "linuxaarch64", "lumin", "mac", "tvos", "unix", "windows"
        };

        private Dictionary<string, ClassDefaultsCache> perCategoryCache = new Dictionary<string, ClassDefaultsCache>();
        
        private ILogger myLogger;
        private UnrealPluginDetector myPluginDetector;
        private ISolution mySolution;

        private IProject mainProject;
        private IProject engineProject;

        public IniFileProcessor(ISolution solution, ILogger logger, UnrealPluginDetector pluginDetector)
        {
            myLogger = logger;
            myPluginDetector = pluginDetector;
            mySolution = solution;

            pluginDetector.InstallInfoProperty.PropertyChanged += Startup;
        }

        private void Startup(object sender, PropertyChangedEventArgs e)
        {
            if (myPluginDetector.UnrealVersion == new Version(0, 0, 0))
            {
                myLogger.LogMessage(LoggingLevel.INFO, "UE4 was not found");
                return;
            }

            engineProject = mySolution.GetProjectsByName("UE4").FirstNotNull();
            mainProject = mySolution.GetProjectsByName(mySolution.Name).FirstNotNull();
            
            if (engineProject == null)
            {
                myLogger.LogMessage(LoggingLevel.WARN,  "UE4 project is not found");
                return;
            }

            var projectConfigDirectory = mySolution.SolutionDirectory.AddSuffix("/Config");
            if (projectConfigDirectory.Exists != FileSystemPath.Existence.Directory)
            {
                myLogger.LogMessage(LoggingLevel.WARN, "Config directory is not found");
            }

            foreach (var platform in platformNames)
            {
                ProcessPlatform(projectConfigDirectory, platform);
            }

            // var classDefaults = perCategoryCache["game"].GetClassDefaults("MyGenerator");
            // var classProperty = perCategoryCache["game"].GetClassProperty("MyGenerator", "wallProbability");
            // var classDefaultValue = perCategoryCache["game"].GetClassDefaultValue("MyGenerator", "floorProbability","android");
        }

        private void ProcessPlatform(FileSystemPath projectConfigDirectory, string platformName)
        {
            var platformDirectory = projectConfigDirectory.Clone();
            if (platformName != IniCachedProperty.DefaultPlatform)
            {
                platformDirectory = projectConfigDirectory.AddSuffix($"/{platformName}");
                
                if (!platformDirectory.ExistsDirectory)
                {
                    return;
                }
                
                myLogger.LogMessage(LoggingLevel.INFO, $"Platform {platformName} detected in config directory");
            }

            var filesToProcess = GetIniFiles(platformDirectory)
                .Where(item =>
                {
                    var filename = item.NameWithoutExtension.ToLower();
                    return filename.StartsWith(platformName);
                });

            foreach (var file in filesToProcess)
            {
                var filename = file.NameWithoutExtension.ToLower();
                var category = filename.Substring(7);
                
                var visitor = new IniVisitor();

                if (!perCategoryCache.ContainsKey(category))
                {
                    perCategoryCache.Add(category, new ClassDefaultsCache(mainProject.Name));
                }

                var cache = perCategoryCache[category];
                cache.SetupPlatform(platformName);
                visitor.AddCacher(cache);
                
                ParseIniFile(file, visitor);
            }
        }

        private void ParseIniFile(FileSystemPath path, IniVisitor visitor)
        {
            var buffer = new StringBuffer(File.ReadAllText(path.FullPath));

            var langService = IniLanguage.IniLanguage.Instance.LanguageService();
            
            var lexer = langService.GetPrimaryLexerFactory().CreateLexer(buffer);
            var parser = new IniParser(lexer);
            var file = parser.ParseFile();

            // var str = parser.DumpPsi(file);
            visitor.VisitFile(file, path);
        }
        
        private IEnumerable<FileSystemPath> GetIniFiles(FileSystemPath directory)
        {
            var entries = directory.GetDirectoryEntries();

            var filteredEntries = entries
                .Select(it => it.GetAbsolutePath())
                .Where(entry => entry.ExistsFile && entry.ExtensionNoDot == "ini");

            return filteredEntries;
        }
    }
}