using System.Collections.Generic;
using System.Text;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;
using JetBrains.Util;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    public class IniTokenType : TokenNodeType
    {
        public IniTokenType(string s, int index) : base(s, index)
        {
        }

        public static IniTokenType L_SBRACKET = new IniTokenType("L_SBRACKET", 0);
        public static IniTokenType R_SBRACKET = new IniTokenType("R_SBRACKET", 1);

        public static IniTokenType ADD_WITH_CHECK = new IniTokenType("ADD_WITH_CHECK", 2);
        public static IniTokenType ADD = new IniTokenType("ADD", 3);
        public static IniTokenType RM_PROP = new IniTokenType("RM_PROP", 4);
        public static IniTokenType RM_LN = new IniTokenType("RM_LN", 5); 
        
        public static IniTokenType EQ = new IniTokenType("EQ", 6);
        
        public static IniTokenType COMMA = new IniTokenType("COMMA", 7);
        public static IniTokenType COLON = new IniTokenType("COLON", 8);
        
        public static IniTokenType L_BRACKET = new IniTokenType("L_BRACKET", 9);
        public static IniTokenType R_BRACKET = new IniTokenType("R_BRACKET", 10);
        
        public static IniTokenType LINESPLITTER = new IniTokenType("LINESPLITTER", 11);
        
        public static IniTokenType SECTION_NAME = new IniTokenType("SECTION_NAME", 12);
        public static IniTokenType PLATFORM = new IniTokenType("PLATFORM", 13); 
        public static IniTokenType PROP_KEY = new IniTokenType("PROP_KEY", 14);
        public static IniTokenType PROP_VAL = new IniTokenType("PROP_VAL", 15);
        public static IniTokenType OBJECT_KEY = new IniTokenType("OBJECT_KEY", 16);
        public static IniTokenType OBJECT_VAL = new IniTokenType("OBJECT_VAL", 17);
        
        public static IniTokenType COMMENT = new IniTokenType("COMMENT", 18);
        
        public static IniTokenType NEWLINE = new IniTokenType("NEWLINE", 19);
        public static IniTokenType WHITESPACE = new IniTokenType("WHITESPACE", 20);
        
        public static IniTokenType QUOTE_MK = new IniTokenType("QUOTE_MK", 21);
        public static IniTokenType STR_LITERAL = new IniTokenType("STR_LITERAL", 22);
        
        public static IniTokenType BAD_CHAR = new IniTokenType("BAD_CHAR", 23);

        public static HashSet<TokenNodeType> Whitespaces = new HashSet<TokenNodeType> {WHITESPACE, NEWLINE, COMMENT, LINESPLITTER};
        public static HashSet<TokenNodeType> PropOperators = new HashSet<TokenNodeType> {ADD_WITH_CHECK, ADD, RM_PROP, RM_LN};
        
        public override LeafElementBase Create(IBuffer buffer, TreeOffset startOffset, TreeOffset endOffset)
        {
            return new IniToken(buffer.GetText(new TextRange(startOffset.Offset, endOffset.Offset)), this);
        }

        public override bool IsWhitespace => this == WHITESPACE;
        public override bool IsComment => this == COMMENT;
        public override bool IsStringLiteral => false;
        public override bool IsConstantLiteral => false;
        public override bool IsIdentifier => this == PROP_KEY;
        public override bool IsKeyword => this == PLATFORM;
        public override string TokenRepresentation { get; }
        
        public class IniToken : LeafElementBase, ITokenNode
        {
            private readonly string myText;
            private IniTokenType myType;

            public IniToken(string text, IniTokenType type)
            {
                myText = text;
                myType = type;
            }

            public override int GetTextLength()
            {
                return myText.Length;
            }

            public override StringBuilder GetText(StringBuilder to)
            {
                to.Append(GetText());
                return to;
            }

            public override IBuffer GetTextAsBuffer()
            {
                return new StringBuffer(GetText());
            }

            public override string GetText()
            {
                return myText;
            }

            public override string ToString()
            {
                return myType.ToString();
            }

            public override NodeType NodeType => myType;
            public override PsiLanguageType Language => IniLanguage.Instance;
            public TokenNodeType GetTokenType()
            {
                return myType;
            }
        }
    }
}