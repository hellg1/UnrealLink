using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Psi.Cpp.UE4;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using JetBrains.Util.DataStructures;

namespace RiderPlugin.UnrealLink
{
    [SolutionComponent]
    public class UnrealLinkResolver
    {
        private static readonly CompactMap<char, char> PairSymbol = new CompactMap<char, char>();
        private readonly ILogger myLogger;
        private readonly Lazy<List<FileSystemPath>> myPossiblePaths;
        private readonly ISolution mySolution;
        private readonly Lazy<FileSystemPath> myUe4SourcesPath;
        private readonly ICppUE4SolutionDetector myUnrealEngineSolutionDetector;

        static UnrealLinkResolver()
        {
            PairSymbol.Add(')', '(');
            PairSymbol.Add(']', '[');
            PairSymbol.Add('\'', '\'');
            PairSymbol.Add('"', '"');
        }

        public UnrealLinkResolver(ISolution solution, ILogger logger,
            ICppUE4SolutionDetector unrealEngineSolutionDetector)
        {
            mySolution = solution;
            myLogger = logger;
            myUnrealEngineSolutionDetector = unrealEngineSolutionDetector;
            var solutionDirectory = mySolution.SolutionDirectory;
            myUe4SourcesPath = new Lazy<FileSystemPath>(() =>
            {
                using (ReadLockCookie.Create())
                {
                    return unrealEngineSolutionDetector.UE4SourcesPath;
                }
            });

            myPossiblePaths = new Lazy<List<FileSystemPath>>(() =>
                new List<FileSystemPath>
                {
                    myUe4SourcesPath.Value,
                    myUe4SourcesPath.Value.Parent,
                    myUe4SourcesPath.Value / "Content",
                    myUe4SourcesPath.Value / "Content" / "Editor",
                    myUe4SourcesPath.Value / "Content" / "Editor" / "Slate", // FSlateStyleSet::ContentRootDir
                    myUe4SourcesPath.Value / "Plugins",

                    solutionDirectory,
                    solutionDirectory / "Content",
                    solutionDirectory / "Plugins"
                });
        }

        [CanBeNull]
        private FileSystemPath ConvertToAbsolutePath(FileSystemPath path)
        {
            if (path.IsAbsolute) return path;

            return myPossiblePaths
                .Value.SelectNotNull(possibleDir =>
                {
                    var relativePath = path.AsRelative();
                    if (relativePath == null || relativePath.IsEmpty) return null;

                    var candidate = possibleDir / relativePath;
                    return candidate.Exists == FileSystemPath.Existence.Missing ? null : candidate;
                })
                .FirstOrDefault(null);
        }

        [CanBeNull]
        private ILinkResponse TryParseFullPath([NotNull] string input, [NotNull] StringRange range)
        {
            try
            {
                var path = ConvertToAbsolutePath(FileSystemPath.Parse(input));
                if (path == null) return null;

                if (path.ExtensionNoDot == "umap")
                    //todo
                    return null;

                if (path.ExtensionNoDot == "uasset")
                    return new LinkResponseBlueprint(new FString(path.ToUri().AbsolutePath), range);

                return new LinkResponseFilePath(new FString(path.ToUri().AbsolutePath), range);
            }
            catch (InvalidPathException e)
            {
                myLogger.Warn(e);
            }
            catch (Exception e)
            {
                myLogger.Error(e, "occured while trying parse full path");
            }

            return null;
        }

        [CanBeNull]
        private ILinkResponse TryParseFullName([NotNull] string s, [NotNull] StringRange range,
            IRdCall<FString, bool> isBlueprintPathName)
        {
            var path = new FString(s);
            return isBlueprintPathName.Sync(path)
                ? new LinkResponseBlueprint(path, range)
                : null;
        }

        [NotNull]
        internal ILinkResponse ResolveLink(LinkRequest @struct, IRdCall<FString, bool> isBlueprintPathName)
        {
            // ReSharper disable once LocalFunctionCanBeMadeStatic
            string SqueezeBorders(string s, out int l, out int r)
            {
                l = 0;
                r = s.Length;
                if (s.EndsWith(".")) --r;

                if (PairSymbol.TryGetValue(s[r - 1], out var value))
                {
                    l = s.AsSpan(l, r - l - 1).LastIndexOf(value) + 1;
                    --r;
                }

                return s.Substring(l, r - l);
            }

            var link = @struct.Data.Data;
            var squeezed = SqueezeBorders(link, out var left, out var right);
            var range = new StringRange(left, right);

            var fullPath = TryParseFullPath(squeezed, range);
            if (fullPath != null) return fullPath;

            var fullName = TryParseFullName(squeezed, range, isBlueprintPathName);
            if (fullName != null) return fullName;


            return new LinkResponseUnresolved();
        }
    }
}