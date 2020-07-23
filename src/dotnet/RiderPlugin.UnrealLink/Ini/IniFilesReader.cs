using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.DataFlow;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.SolutionStructure.SolutionConfigurations;
using JetBrains.ReSharper.Feature.Services.Cpp.ProjectModel.UE4;
using JetBrains.ReSharper.Feature.Services.Cpp.Util;
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
    public class IniFilesReader
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

        private List<FileSystemPath>[] orderedIniFiles = new List<FileSystemPath>[6];
        private HashSet<string> processedPlatforms = new HashSet<string>();
        
        private ILogger myLogger;
        private UnrealPluginDetector myPluginDetector;
        private CppUE4SolutionDetector mySolDetector;
        private ISolution mySolution;

        private IActiveConfigurationManager myActiveConfigurationManager;

        private IProject mainProject;
        private IProject engineProject;

        private CppUE4TargetPlatform curPlatform;
        private Lifetime myLifetime;

        public IniFilesReader(Lifetime lifetime, ISolution solution, ILogger logger, UnrealPluginDetector pluginDetector,
            CppUE4SolutionDetector solutionDetector, IActiveConfigurationManager activeConfigurationManager)
        {
            myLifetime = lifetime;
            
            myLogger = logger;
            myPluginDetector = pluginDetector;
            mySolution = solution;
            mySolDetector = solutionDetector;
            myActiveConfigurationManager = activeConfigurationManager;

            pluginDetector.InstallInfoProperty.PropertyChanged += Startup;

            myActiveConfigurationManager.ActiveConfigurationAndPlatform.Change.Advise_NoAcknowledgement(myLifetime, ConfigurationAndPlatformChange);

            if (!Enum.TryParse((myActiveConfigurationManager.ActiveConfigurationAndPlatform.GetValue() as
                SolutionConfigurationAndPlatform)?.Platform, out curPlatform))
            {
                curPlatform = CppUE4TargetPlatform.Unknown;
            }
        }

        /// <summary>
        /// Returns class default property value for target platform
        /// </summary>
        public string GetDefaultPropertyValue(CppUE4TargetPlatform targetPlatform, string category, string className, string property)
        {
            var categoryLower = category.ToLower();

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

        /// <summary>
        /// Returns class default property value for current configuration
        /// </summary>
        public string GetCurrentPropertyValue(string category, string className, string property)
        {
            return GetDefaultPropertyValue(curPlatform, category, className, property);
        }
        
        private void ConfigurationAndPlatformChange(PropertyChangedEventArgs<ISolutionConfigurationAndPlatform> e)
        {
            if (!Enum.TryParse((e.New as SolutionConfigurationAndPlatform)?.Platform, out curPlatform))
            {
                curPlatform = CppUE4TargetPlatform.Unknown;
            }
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

            ProcessConfigDirectories(mySolution.SolutionDirectory, mySolDetector.UE4SourcesPath);

            perCategoryCache.Add("__base", new ClassDefaultsCache(mainProject.Name));
            if (!orderedIniFiles[0].IsEmpty())
            {
                var visitor = new IniVisitor();
                visitor.AddCacher(perCategoryCache["__base"]);
            
                ParseIniFile(orderedIniFiles[0].First(), visitor);
            }
            
            for (int i = 1; i < 6; i++)
            {
                foreach (var file in orderedIniFiles[i])
                {
                    var filename = file.NameWithoutExtension.ToLower();
                    var platformAndCategory = ParsePlatformAndCategory(filename, i);

                    if (platformAndCategory.Second == null)
                    {
                        continue;
                    }
                    
                    var visitor = new IniVisitor();

                    if (!perCategoryCache.ContainsKey(platformAndCategory.Second))
                    {
                        perCategoryCache.Add(platformAndCategory.Second, perCategoryCache["__base"].Clone() as ClassDefaultsCache);
                    }
                    
                    perCategoryCache[platformAndCategory.Second].SetupPlatform(platformAndCategory.First);
                    visitor.AddCacher(perCategoryCache[platformAndCategory.Second]);
                    
                    ParseIniFile(file, visitor);
                }
            } 
        }

        private Pair<string, string> ParsePlatformAndCategory(string filename, int orderedIndx)
        {
            switch (orderedIndx)
            {
                case 1:
                    return new Pair<string, string>(IniCachedProperty.DefaultPlatform, filename.Substring(4));
                case 2:
                    foreach (var platform in processedPlatforms)
                    {
                        if (filename.StartsWith($"base{platform}"))
                        {
                            return new Pair<string, string>(platform, filename.Substring(4 + platform.Length));
                        }
                    }

                    return new Pair<string, string>(IniCachedProperty.DefaultPlatform, null);
                case 3:
                    return new Pair<string, string>(IniCachedProperty.DefaultPlatform, filename.Substring(7));
                case 4:
                case 5:
                    foreach (var platform in processedPlatforms)
                    {
                        if (filename.StartsWith(platform))
                        {
                            return new Pair<string, string>(platform, filename.Substring(platform.Length));
                        }
                    }
                    return new Pair<string, string>(IniCachedProperty.DefaultPlatform, null);
                default:
                    return new Pair<string, string>(IniCachedProperty.DefaultPlatform, null);
                    
            }
        }

        private void ProcessConfigDirectories(FileSystemPath projectDirectory, FileSystemPath engineDirectory)
        {
            for (int i = 0; i < 6; i++)
            {
                orderedIniFiles[i] = new List<FileSystemPath>();
            }
            
            var engineConfigDirectory = engineDirectory / "Config";

            if (engineConfigDirectory.ExistsDirectory)
            {
                SortFiles(engineConfigDirectory, new[] {true, true, false, false, false, false});

                ProcessPlatforms(engineConfigDirectory);
            }

            var enginePlatformsDirectory = engineDirectory / "Platforms";
            if (enginePlatformsDirectory.ExistsDirectory)
            {
                ProcessPlatforms(engineConfigDirectory, "Config");
            }

            var projectConfigDirectory = projectDirectory / "Config";
            if (projectConfigDirectory.ExistsDirectory)
            {
                SortFiles(projectConfigDirectory, new[] {false, false, false, true, false, false});

                var dirs = projectConfigDirectory.GetDirectoryEntries().Where(it => it.IsDirectory);
                foreach (var dir in dirs)
                {
                    var path = dir.GetAbsolutePath();
                    var platformName = path.Name.ToLower();

                    if (platformName == "Layouts" || platformName == "Localization" || !path.ExistsDirectory)
                    {
                        return;
                    }

                    processedPlatforms.Add(platformName);
                    
                    SortFiles(path, new[] {false, false, false, false, false, true}, platformName);
                }
            }
        }

        private void SortFiles(FileSystemPath path, bool[] checks, string platform = "")
        {
            var iniFiles = GetIniFiles(path);
            foreach (var file in iniFiles)
            {
                var filename = file.NameWithoutExtension.ToLower();
                if (checks[0] && filename == "base")
                {
                    orderedIniFiles[0].Add(file);
                }
                else if (checks[1] && filename.StartsWith("base"))
                {
                    orderedIniFiles[1].Add(file);
                }
                else if (checks[2] && filename.StartsWith($"base{platform}"))
                {
                    orderedIniFiles[2].Add(file);
                }
                else if (checks[3] && filename.StartsWith("default"))
                {
                    orderedIniFiles[3].Add(file);
                }
                else if (filename.StartsWith(platform))
                {
                    if (checks[4])
                    {
                        orderedIniFiles[4].Add(file);
                    }
                    else if (checks[5])
                    {
                        orderedIniFiles[5].Add(file);
                    }
                }
            }
        }
        
        private void ProcessPlatforms(FileSystemPath mainDirectory, string subfolder = "")
        {
            var dirs = mainDirectory.GetDirectoryEntries().Where(it => it.IsDirectory);
            foreach (var dir in dirs)
            {
                var path = dir.GetAbsolutePath();
                var platformName = path.Name.ToLower();
                var subfolderPath = path / subfolder;
            
                if (platformName == "Layouts" || platformName == "Localization" || !subfolderPath.ExistsDirectory)
                {
                    continue;
                }
                
                processedPlatforms.Add(platformName);

                SortFiles(subfolderPath, new[] {false, false, true, false, true, false}, platformName);
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