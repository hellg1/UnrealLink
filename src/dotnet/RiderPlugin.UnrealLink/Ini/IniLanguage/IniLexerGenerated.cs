using JetBrains.ReSharper.Psi.Cpp.Parsing;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;

namespace RiderPlugin.UnrealLink.Ini.IniLanguage
{
    public partial class IniLexerGenerated
    {
        public void Start()
        {
            Start(0, yy_buffer.Length, YYINITIAL);
        }

        public void Advance()
        {
            currentTokenType = null;
            LocateToken();
        }

        private void LocateToken()
        {
            if (currentTokenType == null)
            {
                currentTokenType = _locateToken();
            }
        }

        public TokenNodeType GetToken(TokenNodeType tokenNodeType)
        {
            if (isMultiline)
            {
                isMultiline = false;
                if (tokenNodeType == IniTokenType.NEWLINE)
                {
                    return currentTokenType = tokenNodeType;
                }

                return currentTokenType = IniTokenType.BAD_CHAR;
            }
            
            if (tokenNodeType == IniTokenType.NEWLINE)
            {
                yybegin(YYINITIAL);
            }

            if (tokenNodeType == IniTokenType.QUOTE_MK)
            {
                if (yy_lexical_state == YY_IN_STRING)
                {
                    yybegin(stateBeforeString);
                }
                else
                {
                    stateBeforeString = yy_lexical_state;
                    yybegin(YY_IN_STRING);
                }
            }

            return currentTokenType = tokenNodeType;
        }
        
        public object CurrentPosition 
        {
            get
            {
                TokenPosition tokenPosition;
                tokenPosition.CurrTokenType = currentTokenType;
                tokenPosition.YyBufferIndex = yy_buffer_index;
                tokenPosition.YyBufferStart = yy_buffer_start;
                tokenPosition.YyBufferEnd = yy_buffer_end;
                tokenPosition.YyLexicalState = yy_lexical_state;
                return tokenPosition;
            }
            set
            {
                var tokenPosition = (TokenPosition) value;
                currentTokenType = tokenPosition.CurrTokenType;
                yy_buffer_index = tokenPosition.YyBufferIndex;
                yy_buffer_start = tokenPosition.YyBufferStart;
                yy_buffer_end = tokenPosition.YyBufferEnd;
                yy_lexical_state = tokenPosition.YyLexicalState;
            } 
        }

        public TokenNodeType TokenType
        {
            get
            {
                LocateToken();
                return currentTokenType;
            }
        }

        public int TokenStart
        {
            get
            {
                LocateToken();
                return yy_buffer_start;
            }
        }

        public int TokenEnd
        {
            get
            {
                LocateToken();
                return yy_buffer_end;
            }
        }

        public IBuffer Buffer => yy_buffer;
        public uint LexerStateEx => (uint) yy_lexical_state;

        public int EOFPos => yy_eof_pos;
        public int LexemIndent => 0;
        
        public void Start(int startOffset, int endOffset, uint state)
        {
            yy_buffer_index = startOffset;
            yy_buffer_start = startOffset;
            yy_buffer_end = startOffset;

            yy_eof_pos = endOffset;

            yy_lexical_state = (int) state;
            
            currentTokenType = null;
            isMultiline = false;
        }
    }
}