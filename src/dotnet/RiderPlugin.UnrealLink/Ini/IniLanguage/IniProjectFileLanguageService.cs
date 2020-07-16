using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.UI.Icons;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    [ProjectFileType(typeof(IniProjectFileType))]
    public class IniProjectFileLanguageService : ProjectFileLanguageService
    {
        public IniProjectFileLanguageService(ProjectFileType projectFileType) : base(projectFileType)
        {
        }

        public override IPsiSourceFileProperties GetPsiProperties(IProjectFile projectFile, IPsiSourceFile sourceFile,
            IsCompileService isCompileService)
        {
            this.AssertProjectFileType(projectFile.LanguageType);
            return new DefaultPsiProjectFileProperties(projectFile, sourceFile);
        }
        
        public override PsiLanguageType GetPsiLanguageType(IProjectFile projectFile)
        {
            return GetPsiLanguageType(projectFile.LanguageType);
        }
        
        public override PsiLanguageType GetPsiLanguageType(IPsiSourceFile sourceFile)
        {
            var projectFile = sourceFile.ToProjectFile();
            return projectFile != null ? GetPsiLanguageType(projectFile) : GetPsiLanguageType(sourceFile.LanguageType);
        }
        
        public override PsiLanguageType GetPsiLanguageType(ProjectFileType languageType)
        {
            return IniLanguage.Instance;
        }
        
        public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer,
            IPsiSourceFile sourceFile = null)
        {
            return new IniLanguageService.IniLexerFactory();
        }

        protected override PsiLanguageType PsiLanguageType => IniLanguage.Instance;
        public override IconId Icon => null;
    }
}