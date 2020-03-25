using System.Collections;
using JetBrains.Util;
using JetBrains.Text;
using ReSharperPlugin.UnrealEditor.Parsing.Lexer;

%%

%unicode

%init{
   currTokenType = UE4LogTokenTypes.UNDEFINED;
%init}

%{
  protected UE4LogTokenTypes currTokenType;
    
  protected UE4LogTokenTypes makeToken (UE4LogTokenTypes type) 
  {
    return currTokenType = type;
  }

  public struct State
  {
    public UE4LogTokenTypes TokenType;
    public int BufferIndex;
    public int BufferStart;
    public int BufferEnd;
    public int LexicalState;
  }
  
  public void Start ()
  {
    Start (0, yy_buffer.Length, YYINITIAL);
  }

  public void Start (int startOffset, int endOffset, uint state)
  {
    yy_buffer_index = startOffset;
    yy_buffer_start = startOffset;
    yy_buffer_end = startOffset;
    yy_eof_pos = endOffset;
    yy_lexical_state = (int)state;
    currTokenType = UE4LogTokenTypes.UNDEFINED;
  }

  public void Advance ()
  {
    locateToken();
    currTokenType = UE4LogTokenTypes.UNDEFINED;
  }

  public uint LexerState 
  { 
    get
    {
      return (uint)yy_lexical_state;
    }
  }

  public State SaveState ()
  {
    State state;
    state.TokenType    = currTokenType;
    state.BufferIndex  = yy_buffer_index;
    state.BufferStart  = yy_buffer_start;
    state.BufferEnd    = yy_buffer_end;
    state.LexicalState = yy_lexical_state;
    return state;
  }

  public void RestoreState (State pos)
  {
    currTokenType    = pos.TokenType;
    yy_buffer_index  = pos.BufferIndex;
    yy_buffer_start  = pos.BufferStart;
    yy_buffer_end    = pos.BufferEnd;
    yy_lexical_state = pos.LexicalState;
  }

  public UE4LogTokenTypes TokenType 
  { 
    get 
    {
      locateToken(); 
      return currTokenType;
    } 
  }

  public int TokenStart 
  { 
    get
    {
      locateToken(); 
      return yy_buffer_start;
    }
  }

  public int TokenEnd 
  { 
    get
    {
      locateToken(); 
      return yy_buffer_end;
    }
  }

  public string TokenText
  {
    get { return yytext(); }
  }
  
  public int LexemIndent { get {return 7;} }
  public IBuffer Buffer { get { return yy_buffer; } }

  protected virtual void locateToken()
  {
    if (currTokenType != UE4LogTokenTypes.UNDEFINED) return;
    currTokenType = _locateToken();
  } 
  
  public static short NStates { get { return (short)yy_state_dtrans.Length; } } 
  
  #region Protected access to lexer internals
  protected int BufferIndex { get {return yy_buffer_index;} set {yy_buffer_index=value;} }
  protected int BufferStart { get {return yy_buffer_start;} set {yy_buffer_start=value;} }
  protected int BufferEnd { get {return yy_buffer_end;} set {yy_buffer_end=value;} }
  protected int EOFPos { get {return yy_eof_pos;} set {yy_eof_pos=value;} }
  protected int LexicalState { get {return yy_lexical_state;} set {yy_lexical_state=value;} }
  #endregion
  
%}

%namespace ReSharperPlugin.UnrealEditor.Parsing.Lexer
%class UE4LogLexer
%public
%function _locateToken
%type UE4LogTokenTypes
%eofval{ 
  currTokenType = UE4LogTokenTypes.UNDEFINED; return currTokenType;
%eofval}

%include Lib/Unicode.lex

CARRIAGE_RETURN_CHAR=\u000D
LINE_FEED_CHAR=\u000A
NEW_LINE_PAIR={CARRIAGE_RETURN_CHAR}{LINE_FEED_CHAR}
NEW_LINE_CHAR=({CARRIAGE_RETURN_CHAR}|{LINE_FEED_CHAR}|(\u0085)|(\u2028)|(\u2029))
TAB_CHAR=\u0009
WHITE_SPACE_CHAR=({UNICODE_ZS}|(\u000B)|(\u000C)|(\u200B)|(\uFEFF))

WHITE_SPACE=({WHITE_SPACE_CHAR}+)

HEX_DIGIT=(0|1|2|3|4|5|6|7|8|9|a|b|c|d|e|f|A|B|C|D|E|F)

COLON=\u003A
COMMA=\u002C
LPARENTH=\u0028
RPARENTH=\u0029
LBRACKET=\u005B
RBRACKET=\u005D
LANGLE=\u003C
RANGLE=\u003E
DOT=\u002E
AMPERSAND=\u0026
ASTERISK=\u002A
EXCLAMATION=\u0021
EQUALS=\u003D
LBRACE=\u007B
RBRACE=\u007D
QUOTATION=\u0022
MINUS=\u002D

GRAVE_ACCENT=\u0060
UNDERSCORE=\u005F

VOLUME_SEPARATOR=\u003A
DIRECTORY_SEPARATOR1=\u002F
DIRECTORY_SEPARATOR2=\u005C
PATH_SEPARATOR=({VOLUME_SEPARATOR}{DIRECTORY_SEPARATOR1}|{VOLUME_SEPARATOR}{DIRECTORY_SEPARATOR2}|{DIRECTORY_SEPARATOR1}|{DIRECTORY_SEPARATOR2})

DECIMAL_DIGIT_CHARACTER={UNICODE_ND}

LETTER_CHARACTER=({UNICODE_LL}|{UNICODE_LM}|{UNICODE_LO}|{UNICODE_LT}|{UNICODE_LU}|{UNICODE_NL})

IDENTIFIER_START_CHARACTER=({LETTER_CHARACTER}|{UNDERSCORE})
IDENTIFIER_PART_CHARACTER=({IDENTIFIER_START_CHARACTER}|{DECIMAL_DIGIT_CHARACTER}|{GRAVE_ACCENT})
IDENTIFIER=({IDENTIFIER_START_CHARACTER}{IDENTIFIER_PART_CHARACTER}*)

ZERO=\u0030
HEX_X=(\u0078|\u0058)

FLOAT_NUMBER=({DECIMAL_DIGIT_CHARACTER}+{DOT}{DECIMAL_DIGIT_CHARACTER}+)
DEC_NUMBER=({DECIMAL_DIGIT_CHARACTER}+)
HEX_NUMBER=({ZERO}{HEX_X}{HEX_DIGIT}+)

%%

<YYINITIAL> {WHITE_SPACE} { currTokenType = makeToken(UE4LogTokenTypes.WHITESPACE); return currTokenType; }
<YYINITIAL> {PATH_SEPARATOR} { currTokenType = makeToken(UE4LogTokenTypes.PATH_SEPARATOR); return currTokenType; }
<YYINITIAL> {TAB_CHAR} { currTokenType = makeToken(UE4LogTokenTypes.TAB); return currTokenType; }
<YYINITIAL> {NEW_LINE_CHAR} { currTokenType = makeToken(UE4LogTokenTypes.EOL); return currTokenType; }

<YYINITIAL> {COLON} { currTokenType = makeToken(UE4LogTokenTypes.COLON); return currTokenType; }
<YYINITIAL> {COMMA} { currTokenType = makeToken(UE4LogTokenTypes.COMMA); return currTokenType; }
<YYINITIAL> {LPARENTH} { currTokenType = makeToken(UE4LogTokenTypes.LPARENTH); return currTokenType; }
<YYINITIAL> {RPARENTH} { currTokenType = makeToken(UE4LogTokenTypes.RPARENTH); return currTokenType; }
<YYINITIAL> {LBRACKET} { currTokenType = makeToken(UE4LogTokenTypes.LBRACKET); return currTokenType; }
<YYINITIAL> {RBRACKET} { currTokenType = makeToken(UE4LogTokenTypes.RBRACKET); return currTokenType; }
<YYINITIAL> {LANGLE} { currTokenType = makeToken(UE4LogTokenTypes.LANGLE); return currTokenType; }
<YYINITIAL> {RANGLE} { currTokenType = makeToken(UE4LogTokenTypes.RANGLE); return currTokenType; }
<YYINITIAL> {DOT} { currTokenType = makeToken(UE4LogTokenTypes.DOT); return currTokenType; }
<YYINITIAL> {AMPERSAND} { currTokenType = makeToken(UE4LogTokenTypes.AMPERSAND); return currTokenType; }
<YYINITIAL> {ASTERISK} { currTokenType = makeToken(UE4LogTokenTypes.ASTERISK); return currTokenType; }
<YYINITIAL> {EXCLAMATION} { currTokenType = makeToken(UE4LogTokenTypes.EXCLAMATION); return currTokenType; }
<YYINITIAL> {EQUALS} { currTokenType = makeToken(UE4LogTokenTypes.EQUALS); return currTokenType; }
<YYINITIAL> {LBRACE} { currTokenType = makeToken(UE4LogTokenTypes.LBRACE); return currTokenType; }
<YYINITIAL> {RBRACE} { currTokenType = makeToken(UE4LogTokenTypes.RBRACE); return currTokenType; }
<YYINITIAL> {QUOTATION} { currTokenType = makeToken(UE4LogTokenTypes.QUOTATION); return currTokenType; }
<YYINITIAL> {MINUS} { currTokenType = makeToken(UE4LogTokenTypes.MINUS); return currTokenType; }

<YYINITIAL> {FLOAT_NUMBER} { currTokenType = makeToken(UE4LogTokenTypes.NUMBER); return currTokenType; }
<YYINITIAL> {HEX_NUMBER} { currTokenType = makeToken(UE4LogTokenTypes.NUMBER); return currTokenType; }
<YYINITIAL> {DEC_NUMBER} { currTokenType = makeToken(UE4LogTokenTypes.NUMBER); return currTokenType; }

<YYINITIAL> {IDENTIFIER} { currTokenType = makeToken(UE4LogTokenTypes.IDENTIFIER); return currTokenType; }

<YYINITIAL> . { currTokenType = makeToken(UE4LogTokenTypes.WORD); return currTokenType; }
