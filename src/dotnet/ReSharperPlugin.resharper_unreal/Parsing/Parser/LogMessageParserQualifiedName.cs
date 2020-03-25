using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Lexer;
using ReSharperPlugin.UnrealEditor.Parsing.Node;

namespace ReSharperPlugin.UnrealEditor.Parsing.Parser
{
    public partial class UnrealLogParser
    {
        private bool ParseIdentifier(UE4LogLexer lexer, out IdentifierNode identifierNode)
        {
            identifierNode = null;
            while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED &&
                   lexer.TokenType == UE4LogTokenTypes.IDENTIFIER)
            {
                var textRange = new TextRange(lexer.TokenStart, lexer.TokenEnd);
                var name = lexer.Buffer.GetText(textRange);
                identifierNode = new IdentifierNode(textRange, name, identifierNode);
                lexer.Advance();
                if (!ParseQualifiedNameDelimiter(lexer))
                    return identifierNode != null;
            }

            return identifierNode != null;
        }

        private static bool ParseQualifiedNameDelimiter(UE4LogLexer lexer)
        {
            var saveState = lexer.SaveState();
            if (lexer.TokenType != UE4LogTokenTypes.COLON)
                return false;

            lexer.Advance();
            if (lexer.TokenType != UE4LogTokenTypes.COLON)
            {
                lexer.RestoreState(saveState);
                return false;
            }

            lexer.Advance();
            return true;
        }
    }
}