using System;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.Text;
using JetBrains.Util;
using RiderPlugin.UnrealLink.Ini.IniLanguage;

%%
%{
TokenNodeType currentTokenType;
bool isMultiline;
int stateBeforeString;
%}

%unicode

%init{
isMultiline = false;
currentTokenType = null;
stateBeforeString = -1;
%init}

%namespace RiderPlugin.UnrealLink.Ini.IniLanguage
%class IniLexerGenerated
%implements IIncrementalLexer

%function _locateToken
%public
%type TokenNodeType
%ignorecase

%eofval{
currentTokenType = null; return currentTokenType;
%eofval}

ALPHA=[A-Za-z]
DIGIT=[0-9]

NEWLINE_CHARS=\u000D\u000A
NEWLINE=(({NEWLINE_CHARS})|\u000D|\u000A)

WHITESPACE_CHARS=\u0020\u0009\u000B\u000C
WHITESPACE=\u0020|\u0009|\u000B|\u000C

WS_CHARS={NEWLINE_CHARS}{WHITESPACE_CHARS}

L_SBRACKET=\u005B
R_SBRACKET=\u005D
SBRACKETS=({L_SBRACKET}|{R_SBRACKET})
SBRACKETS_CHARS={L_SBRACKET}{R_SBRACKET}

L_BRACKET=\u0028
R_BRACKET=\u0029
BRACKETS=({L_BRACKET}|{R_BRACKET})
BRACKETS_CHARS={L_BRACKET}{R_BRACKET}

EQ=\u003D

PLUS=\u002B
DOT=\u002E
EXCL_MK=\u0021
MINUS=\u002D

OPERATORS={PLUS}{DOT}{EXCL_MK}{MINUS}

COMMA=\u002C
COLON=\u003A
SEMICOLON=\u003B

QUOTE_MK=\u0022

BACKSLASH=\u005C

NAME=({ALPHA}+{DIGIT}*)
PLATFORM=([^{WS_CHARS}{EQ}{SBRACKETS_CHARS}{SEMICOLON}{COLON}{OPERATORS}][^{NEWLINE_CHARS}{EQ}{SBRACKETS_CHARS}{SEMICOLON}{COLON}]*{COLON})

SECTION_NAME=([^{WS_CHARS}{SBRACKETS_CHARS}]+)
PROP_KEY=([^{WS_CHARS}{EQ}{COLON}{SBRACKETS_CHARS}{OPERATORS}][^{WS_CHARS}{EQ}{COLON}{SBRACKETS_CHARS}]*)
PROP_VAL=([^{WS_CHARS}{BRACKETS_CHARS}{EQ}{QUOTE_MK}][^{WS_CHARS}]*)
OBJECT_VAL=([^{WS_CHARS}{COMMA}{BRACKETS_CHARS}{EQ}{QUOTE_MK}][^{WS_CHARS}{COMMA}{EQ}{BRACKETS_CHARS}]*)

COMMENT=({SEMICOLON}[^{NEWLINE_CHARS}]*)

STR_LITERAL=([^{QUOTE_MK}{NEWLINE_CHARS}]+)

%state YY_IN_VALUE, YY_IN_OBJECT_KEY, YY_IN_OBJECT_VAL, YY_IN_SECTION_HEADER, YY_IN_STRING

%%
<YYINITIAL> {COMMENT} { return GetToken(IniTokenType.COMMENT); }
<YYINITIAL, YY_IN_VALUE, YY_IN_OBJECT_VAL, YY_IN_OBJECT_KEY, YY_IN_SECTION_HEADER> {BACKSLASH} { isMultiline = true; return GetToken(IniTokenType.LINESPLITTER); }

<YYINITIAL> {L_SBRACKET} { yybegin(YY_IN_SECTION_HEADER); return GetToken(IniTokenType.L_SBRACKET); }
<YY_IN_SECTION_HEADER> {SECTION_NAME} { return GetToken(IniTokenType.SECTION_NAME); }
<YY_IN_SECTION_HEADER> {R_SBRACKET} { yybegin(YYINITIAL); return GetToken(IniTokenType.R_SBRACKET); }

<YYINITIAL, YY_IN_VALUE, YY_IN_OBJECT_KEY, YY_IN_OBJECT_VAL, YY_IN_SECTION_HEADER, YY_IN_STRING> {NEWLINE} { return GetToken(IniTokenType.NEWLINE); }

<YYINITIAL> {PLUS} { return GetToken(IniTokenType.ADD_WITH_CHECK); }
<YYINITIAL> {DOT} { return GetToken(IniTokenType.ADD); }
<YYINITIAL> {EXCL_MK} { return GetToken(IniTokenType.RM_PROP); }
<YYINITIAL> {MINUS} { return GetToken(IniTokenType.RM_LN); }

<YYINITIAL> {PLATFORM} { return GetToken(IniTokenType.PLATFORM); }
<YYINITIAL> {PROP_KEY} { yybegin(YY_IN_VALUE); return GetToken(IniTokenType.PROP_KEY); }
<YY_IN_VALUE, YY_IN_OBJECT_VAL> {EQ} { return GetToken(IniTokenType.EQ); }
<YY_IN_VALUE, YY_IN_OBJECT_VAL> {L_BRACKET} { yybegin(YY_IN_OBJECT_KEY); return GetToken(IniTokenType.L_BRACKET); }
<YY_IN_VALUE> {PROP_VAL} { return GetToken(IniTokenType.PROP_VAL); }
<YY_IN_OBJECT_KEY> {PROP_KEY} { yybegin(YY_IN_OBJECT_VAL); return GetToken(IniTokenType.OBJECT_KEY); }
<YY_IN_OBJECT_VAL> {OBJECT_VAL} { return GetToken(IniTokenType.OBJECT_VAL); }

<YY_IN_OBJECT_VAL> {COMMA} { yybegin(YY_IN_OBJECT_KEY); return GetToken(IniTokenType.COMMA); }
<YY_IN_OBJECT_VAL> {R_BRACKET} { return GetToken(IniTokenType.R_BRACKET); }

<YY_IN_OBJECT_VAL, YY_IN_VALUE> {QUOTE_MK} { return GetToken(IniTokenType.QUOTE_MK); }
<YY_IN_STRING> {QUOTE_MK} { return GetToken(IniTokenType.QUOTE_MK); }
<YY_IN_STRING> {STR_LITERAL} { return GetToken(IniTokenType.STR_LITERAL); }

<YYINITIAL, YY_IN_VALUE, YY_IN_OBJECT_VAL, YY_IN_OBJECT_KEY, YY_IN_SECTION_HEADER> {WHITESPACE} { return currentTokenType = IniTokenType.WHITESPACE; }

<YYINITIAL, YY_IN_VALUE, YY_IN_OBJECT_VAL, YY_IN_OBJECT_KEY, YY_IN_SECTION_HEADER, YY_IN_STRING> . { return currentTokenType = IniTokenType.BAD_CHAR; }