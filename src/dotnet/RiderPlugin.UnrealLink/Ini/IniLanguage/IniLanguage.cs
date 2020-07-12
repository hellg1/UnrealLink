using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    [LanguageDefinition(Name)]
    public class IniLanguage : KnownLanguage
    {
        public new const string Name = "Ini";

        public new static IniLanguage Instance { get; private set; }
        
        private IniLanguage() : base(Name, Name)
        {
        }
        
        protected IniLanguage([NotNull] string name) : base(name)
        {
        }

        protected IniLanguage([NotNull] string name, [NotNull] string presentableName) : base(name, presentableName)
        {
        }
    }
}