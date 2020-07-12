using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    public class IniFile : FileElementBase
    {
        public override NodeType NodeType => IniFileNodeType.Instance;
        public override PsiLanguageType Language => Ini.IniLanguage.IniLanguage.Instance;
    }

    public class IniSection : CompositeElement
    {
        public override NodeType NodeType => IniCompositeNodeType.SECTION;
        public override PsiLanguageType Language => Ini.IniLanguage.IniLanguage.Instance;
    }

    public class IniSectionHeader : CompositeElement
    {
        public override NodeType NodeType => IniCompositeNodeType.SECTION_HEADER;
        public override PsiLanguageType Language => Ini.IniLanguage.IniLanguage.Instance; 
    }
    
    public class IniProperty : CompositeElement
    {
        public override NodeType NodeType => IniCompositeNodeType.PROPERTY;
        public override PsiLanguageType Language => Ini.IniLanguage.IniLanguage.Instance; 
    }

    public class IniValObject : CompositeElement
    {
        public override NodeType NodeType => IniCompositeNodeType.VAL_OBJECT;
        public override PsiLanguageType Language => Ini.IniLanguage.IniLanguage.Instance; 
    }
}