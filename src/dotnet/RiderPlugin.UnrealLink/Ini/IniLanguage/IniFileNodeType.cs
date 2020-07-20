using System;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    public class IniFileNodeType : CompositeNodeType
    {
        public IniFileNodeType(string s, int index) : base(s, index)
        {
        }

        public static readonly IniFileNodeType Instance = new IniFileNodeType("IniFile", 0);
        
        public override CompositeElement Create()
        {
            return new IniFile();
        }
    }

    public class IniCompositeNodeType : CompositeNodeType
    {
        public IniCompositeNodeType(string s, int index) : base(s, index)
        {
        }
        
        public static readonly IniCompositeNodeType SECTION = new IniCompositeNodeType("Ini_SECTION", 0);
        public static readonly IniCompositeNodeType SECTION_HEADER = new IniCompositeNodeType("Ini_SECTION", 1);
        public static readonly IniCompositeNodeType PROPERTY = new IniCompositeNodeType("Ini_PROPERTY", 2);
        public static readonly IniCompositeNodeType VAL_OBJECT = new IniCompositeNodeType("Ini_VAL_OBJECT", 2);

        public override CompositeElement Create()
        {
            if (this == SECTION)
            {
                return new IniSection();
            }
            if (this == SECTION_HEADER)
            {
                return new IniSectionHeader();
            }
            if (this == PROPERTY)
            {
                return new IniProperty();
            }
            if (this == VAL_OBJECT)
            {
                return new IniValObject();
            }
            throw new InvalidOperationException();
        }
    }
}