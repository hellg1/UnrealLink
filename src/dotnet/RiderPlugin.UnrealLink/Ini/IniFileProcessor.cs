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
    [SolutionComponent]
    public class IniFileProcessor
    {
        private static HashSet<string> platformNames = new HashSet<string> { "android", "hololens", "ios", "linux", "linuxaarch64", "lumin", "mac", "tvos", "unix", "windows" };

        private Dictionary<string, ClassDefaultsCache> perCategoryCache = new Dictionary<string, ClassDefaultsCache>();
        
        private ILogger myLogger;
        private UnrealPluginDetector myPluginDetector;
        private ISolution mySolution;
    
        public IniFileProcessor(ISolution solution, ILogger logger, UnrealPluginDetector pluginDetector)
        {
            myLogger = logger;
            myPluginDetector = pluginDetector;
            mySolution = solution;
            
            myLogger.Info("test message");

            pluginDetector.InstallInfoProperty.PropertyChanged += CheckForUE;
        }

        private void CheckForUE(object sender, PropertyChangedEventArgs e)
        {
            if (myPluginDetector.UnrealVersion == new Version(0, 0, 0))
            {
                myLogger.LogMessage(LoggingLevel.INFO, "UE4 was not found");
                return;
            }
            
            myLogger.LogMessage(LoggingLevel.INFO, $"UE4 vers: {myPluginDetector.UnrealVersion}");
            var engineProject = mySolution.GetProjectsByName("UE4").FirstNotNull();
            var mainProject = mySolution.GetProjectsByName(mySolution.Name).FirstNotNull();
            if (engineProject == null)
            {
                myLogger.LogMessage(LoggingLevel.WARN,  "UE4 project is not found");
                return;
            }

            myLogger.LogMessage(LoggingLevel.INFO, engineProject.ProjectFileLocation.Directory.ToString());
            
            var projectConfigDirectory = mySolution.SolutionDirectory.AddSuffix("/Config");
            if (projectConfigDirectory.Exists != FileSystemPath.Existence.Directory)
            {
                myLogger.LogMessage(LoggingLevel.WARN, "Config directory is not found");
            }

            var iniFiles = GetIniFiles(projectConfigDirectory);
            
            var defaultFiles = new List<FileSystemPath>();
            var projectFiles = GetIniFiles(projectConfigDirectory);
            
            foreach (var file in projectFiles)
            {
                var iniFile = file.GetAbsolutePath();
                var filename = iniFile.NameWithoutExtension.ToLower();
                if (filename.StartsWith("default"))
                {
                    defaultFiles.Add(iniFile);
                } 
            }

            foreach (var file in defaultFiles)
            {
                var filename = file.NameWithoutExtension.ToLower();
                var category = filename.Substring(7);
                
                var visitor = new IniVisitor();
                var cache = new ClassDefaultsCache(mainProject.Name);
                visitor.AddCacher(cache);
                
                ParseIniFile(file, visitor);

                if (!cache.IsEmpty)
                {
                    perCategoryCache.Add(category, cache);
                }
            }
        }

        private List<FileSystemPath>[] GetOrderedFiles(FileSystemPath projectConfigDirectory, FileSystemPath engineConfigDirectory)
        {
            var orderedFiles = new List<FileSystemPath>[6];
            for (int i = 0; i < 6; i++)
            {
                orderedFiles[i] = new List<FileSystemPath>();
            }

            var projectFiles = GetIniFiles(projectConfigDirectory);
            
            foreach (var file in projectFiles)
            {
                var iniFile = file.GetAbsolutePath();
                var filename = iniFile.NameWithoutExtension.ToLower();
                if (filename.StartsWith("default"))
                {
                    orderedFiles[4].Add(iniFile);
                } 
            }

            var engineFiles = GetIniFiles(engineConfigDirectory);

            foreach (var file in projectFiles)
            {
                var iniFile = file.GetAbsolutePath();
                var filename = iniFile.NameWithoutExtension.ToLower();
                if (filename == "base")
                {
                    orderedFiles[0].Add(iniFile);
                }
                else if (filename.StartsWith("base"))
                {
                    orderedFiles[1].Add(iniFile);                    
                }
            }

            return orderedFiles;
        }

        private void ParseIniFile(FileSystemPath path, IniVisitor visitor)
        {
            var buffer = new StringBuffer(File.ReadAllText(path.FullPath));

            var langService = IniLanguage.IniLanguage.Instance.LanguageService();
            
            var lexer = langService.GetPrimaryLexerFactory().CreateLexer(buffer);
            var parser = new IniParser(lexer);
            var file = parser.ParseFile();

            var str = parser.DumpPsi(file);

            
            visitor.VisitFile(file, path);
        }
        
        private IEnumerable<DirectoryEntryData> GetIniFiles(FileSystemPath directory)
        {
            var entries = directory.GetDirectoryEntries();

            var filteredEntries = entries.Where(entry => entry.IsFile && entry.GetAbsolutePath().ExtensionNoDot == "ini");
            
            return filteredEntries;
        }
    }
}