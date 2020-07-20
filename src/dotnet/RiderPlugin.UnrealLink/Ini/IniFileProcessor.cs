using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Cpp.ProjectModel.UE4;
using JetBrains.ReSharper.Psi;
using JetBrains.Text;
using JetBrains.Util;
using RiderPlugin.UnrealLink.Ini.IniLanguage;
using RiderPlugin.UnrealLink.PluginInstaller;

namespace RiderPlugin.UnrealLink.Ini
{
    /// <summary>
    /// Class for processing ini files from project directory
    /// </summary>
    [SolutionComponent]
    public class IniFileProcessor
    {
        private static Dictionary<CppUE4TargetPlatform, string> platformNames = new Dictionary<CppUE4TargetPlatform, string>
        {
            { CppUE4TargetPlatform.Unknown, IniCachedProperty.DefaultPlatform },
            { CppUE4TargetPlatform.Android, "android"},
            { CppUE4TargetPlatform.Linux, "linux" },
            { CppUE4TargetPlatform.Lumin, "lumin" },
            { CppUE4TargetPlatform.Mac, "mac" },
            { CppUE4TargetPlatform.Quail, "quail" },
            { CppUE4TargetPlatform.Switch, "switch" },
            { CppUE4TargetPlatform.Win32, "windows" },
            { CppUE4TargetPlatform.Win64, "windows" },
            { CppUE4TargetPlatform.PS4, "ps4" },
            { CppUE4TargetPlatform.XboxOne, "xboxone" },
            { CppUE4TargetPlatform.IOS, "ios" },
            { CppUE4TargetPlatform.HTML5, "html5"},
            { CppUE4TargetPlatform.TVOS, "tvos" },
        };

        private Dictionary<string, ClassDefaultsCache> perCategoryCache = new Dictionary<string, ClassDefaultsCache>();

        private HashSet<string> processedPlatforms = new HashSet<string>();
        
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

        public string GetDefaultPropertyValue(CppUE4TargetPlatform targetPlatform, string category, string className, string property)
        {
            var categoryLower = category.ToLower();
            
            if (perCategoryCache == null)
            {
                return null;
            }

            if (!perCategoryCache.ContainsKey(categoryLower))
            {
                return null;
            }
            
            var cache = perCategoryCache[categoryLower];
            
            var platform = platformNames.ContainsKey(targetPlatform)
                ? platformNames[targetPlatform]
                : IniCachedProperty.DefaultPlatform;

            return cache.GetClassProperty(className, property)?.GetValues(platform).Last().Value;
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
                myLogger.LogMessage(LoggingLevel.WARN, "Config directory was not found");
                return;
            }

            var dirs = projectConfigDirectory.GetDirectoryEntries().Where(it => it.IsDirectory);

            ProcessPlatform(projectConfigDirectory, projectConfigDirectory);
            
            foreach (var dir in dirs)
            {
                ProcessPlatform(dir.GetAbsolutePath(), projectConfigDirectory);
            }
        }

        private void ProcessPlatform(FileSystemPath path, FileSystemPath projectConfigDirectory)
        {
            var platformName = path == projectConfigDirectory ? IniCachedProperty.DefaultPlatform : path.Name.ToLower();
            
            if (processedPlatforms.Contains(platformName) || platformName == "Layouts" || platformName == "Localization")
            {
                return;
            }

            processedPlatforms.Add(platformName);

            var filesToProcess = GetIniFiles(path)
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