using System.IO;
using System.Text;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    public class IniParser : IParser
    {
        private readonly ILexer myLexer;
        
        public IniParser(ILexer lexer)
        {
            myLexer = lexer;
        }
        
        public IFile ParseFile()
        {
            using (var def = Lifetime.Define())
            {
                var builder = new PsiBuilder(myLexer, IniFileNodeType.Instance, new TokenFactory(), def.Lifetime);
                var fileMark = builder.Mark();

                SkipEmptyLines(builder);
                
                while (!builder.Eof())
                {
                    if (builder.GetTokenType() == IniTokenType.L_SBRACKET)
                    {
                        ParseSection(builder);
                    }
                    else
                    {
                        builder.AdvanceLexer();
                    }
                }
                
                builder.Done(fileMark, IniFileNodeType.Instance, null);

                var file = (IFile) builder.BuildTree();
                var dump = DumpPsi(file);

                return file;
            }
        }

        // for debug purposes
        public string DumpPsi(IFile file)
        {
            var stringBuilder = new StringBuilder();
            DebugUtil.DumpPsi(new StringWriter(stringBuilder), file);
            return stringBuilder.ToString();
        }

        private void ParseSection(PsiBuilder builder)
        {
            if (builder.Eof() || builder.GetTokenType() != IniTokenType.L_SBRACKET)
            {
                return;
            }
            
            var sectionStart = builder.Mark();
            
            ParseSectionHeader(builder);
            SkipEmptyLines(builder);
            ParseSectionContent(builder);
            
            builder.Done(sectionStart, IniCompositeNodeType.SECTION, null);
        }

        private void ParseSectionHeader(PsiBuilder builder)
        {
            var sectionHeaderStart = builder.Mark();
            builder.AdvanceLexer();
            while (!builder.Eof() && builder.GetTokenType() != IniTokenType.R_SBRACKET && builder.GetTokenType() != IniTokenType.NEWLINE)
            {
                builder.AdvanceLexer();
            }

            if (builder.Eof() || builder.GetTokenType() == IniTokenType.NEWLINE)
            {
                builder.Error("Missing ']'");
            }
            else
            {
                builder.AdvanceLexer();
            }

            builder.Done(sectionHeaderStart, IniCompositeNodeType.SECTION_HEADER, null);
        }

        private void ParseSectionContent(PsiBuilder builder)
        {
            while (!builder.Eof() && builder.GetTokenType() != IniTokenType.L_SBRACKET)
            {
                if (builder.GetTokenType() == IniTokenType.PLATFORM || builder.GetTokenType() == IniTokenType.PROP_KEY || IniTokenType.PropOperators.Contains(builder.GetTokenType()))
                {
                    ParseProperty(builder);
                }
                else
                {
                    builder.AdvanceLexer();
                }
            }
        }

        private void ParseProperty(PsiBuilder builder)
        {
            var propertyStart = builder.Mark();

            if (IniTokenType.PropOperators.Contains(builder.GetTokenType()))
            {
                builder.AdvanceLexer();
            }
            
            SkipWhitespaces(builder);
            if (builder.Eof() || builder.GetTokenType() == IniTokenType.NEWLINE)
            {
                builder.Done(propertyStart, IniCompositeNodeType.PROPERTY, null);
                return;
            }

            // some properties in engine have prefixes like "Windows:bIsEnabled=true" from DataDrivenPlatforms.ini
            if (builder.GetTokenType() == IniTokenType.PLATFORM)
            {
                builder.AdvanceLexer();
            }

            SkipWhitespaces(builder);
            
            if (builder.Eof() || builder.GetTokenType() == IniTokenType.NEWLINE)
            {
                builder.Done(propertyStart, IniCompositeNodeType.PROPERTY, null);
                return;
            }

            if (builder.GetTokenType() == IniTokenType.PROP_KEY || builder.GetTokenType() == IniTokenType.OBJECT_KEY)
            {
                builder.AdvanceLexer();
            }
            
            if (!SkipLinesplitter(builder))
            {
                builder.Done(propertyStart, IniCompositeNodeType.PROPERTY, null);
                return;
            }

            if (builder.GetTokenType() == IniTokenType.EQ)
            {
                builder.AdvanceLexer();
            }

            if (!SkipLinesplitter(builder))
            {
                builder.Done(propertyStart, IniCompositeNodeType.PROPERTY, null);
                return;
            }

            ParsePropertyValue(builder);

            builder.Done(propertyStart, IniCompositeNodeType.PROPERTY, null);
        }

        private void ParsePropertyValue(PsiBuilder builder)
        {
            if (builder.GetTokenType() == IniTokenType.L_BRACKET)
            {
                var objectMark = builder.Mark();

                while (!builder.Eof() && (builder.GetTokenType() == IniTokenType.L_BRACKET || builder.GetTokenType() == IniTokenType.COMMA))
                {
                    builder.AdvanceLexer();

                    if (!SkipLinesplitter(builder))
                    {
                        builder.Error("Expected ')'");
                        break;
                    }
                    
                    ParseProperty(builder);

                    if (!SkipLinesplitter(builder))
                    {
                        builder.Error("Expected ')'");
                        break;
                    }

                    if (builder.GetTokenType() == IniTokenType.R_BRACKET)
                    {
                        break;
                    }

                    while (!builder.Eof() && builder.GetTokenType() == IniTokenType.L_BRACKET)
                    {
                        builder.AdvanceLexer();
                        SkipLinesplitter(builder);
                    }
                }

                if (builder.Eof() || builder.GetTokenType() != IniTokenType.R_BRACKET)
                {
                    builder.Error("Expected ')'");
                }
                else
                {
                    builder.AdvanceLexer();
                }

                builder.Done(objectMark, IniCompositeNodeType.VAL_OBJECT, null);
            }
            else if (builder.GetTokenType() == IniTokenType.PROP_VAL || builder.GetTokenType() == IniTokenType.OBJECT_VAL)
            {
                builder.AdvanceLexer();
            } else if (builder.GetTokenType() == IniTokenType.QUOTE_MK)
            {
                ParseStringLiteral(builder);
            }

            SkipLinesplitter(builder);
        }

        private void ParseStringLiteral(PsiBuilder builder)
        {
            if (builder.Eof() || builder.GetTokenType() != IniTokenType.QUOTE_MK)
            {
                return;
            }

            builder.AdvanceLexer();
            if (!builder.Eof() && builder.GetTokenType() == IniTokenType.STR_LITERAL)
            {
                builder.AdvanceLexer();
            }

            if (builder.Eof() || builder.GetTokenType() != IniTokenType.QUOTE_MK)
            {
                builder.Error("Expected '\"'");
                return;
            }

            builder.AdvanceLexer();
        }
        
        /// <summary>
        /// Skip empty lines (backslashes also would be skipped)
        /// </summary>
        private void SkipEmptyLines(PsiBuilder builder)
        {
            while (!builder.Eof() && IniTokenType.Whitespaces.Contains(builder.GetTokenType()))
            {
                builder.AdvanceLexer();
            }
        } 
        
        private void SkipWhitespaces(PsiBuilder builder)
        {
            while (!builder.Eof() && builder.GetTokenType() == IniTokenType.WHITESPACE)
            {
                builder.AdvanceLexer();
            }
        }
        
        /// <summary>
        /// Skips backslashes and whitespaces
        /// </summary>
        /// <returns>False if eof or newline appears after backslash</returns>
        private bool SkipLinesplitter(PsiBuilder builder)
        {
            SkipWhitespaces(builder);
            
            while (!builder.Eof() && builder.GetTokenType() == IniTokenType.LINESPLITTER)
            {
                builder.AdvanceLexer();
                if (builder.Eof())
                {
                    return false;
                }

                if (builder.GetTokenType() == IniTokenType.NEWLINE)
                {
                    builder.AdvanceLexer();
                }
            }

            SkipWhitespaces(builder);
            
            return !builder.Eof() && builder.GetTokenType() != IniTokenType.NEWLINE;
        }
    }

    public class TokenFactory : IPsiBuilderTokenFactory
    {
        public LeafElementBase CreateToken(TokenNodeType tokenNodeType, IBuffer buffer, int startOffset, int endOffset)
        {
            return tokenNodeType.Create(buffer, new TreeOffset(startOffset), new TreeOffset(endOffset));
        }
    }
}