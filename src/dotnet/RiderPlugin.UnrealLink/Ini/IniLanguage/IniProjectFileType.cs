using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    [ProjectFileTypeDefinition(Name)]
    public class IniProjectFileType : KnownProjectFileType
    {
        public new const string Name = "Ini";
        public new static IniProjectFileType Instance { get; private set; }

        private IniProjectFileType() : base(Name, Name, new[] {Ini_EXTENSION})
        {
        }
        
        protected IniProjectFileType([NotNull] string name) : base(name)
        {
        }

        protected IniProjectFileType([NotNull] string name, [NotNull] string presentableName) : base(name, presentableName)
        {
        }

        protected IniProjectFileType([NotNull] string name, [NotNull] string presentableName, [NotNull] IEnumerable<string> extensions) : base(name, presentableName, extensions)
        {
        }
        
        public const string Ini_EXTENSION = ".ini";
    }
}