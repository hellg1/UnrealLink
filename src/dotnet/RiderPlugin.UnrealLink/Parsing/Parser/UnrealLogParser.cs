using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.IDE.StackTrace;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Text;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Lexer;
using ReSharperPlugin.UnrealEditor.Parsing.Node;
using ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack;

namespace ReSharperPlugin.UnrealEditor.Parsing.Parser
{
    public partial class UnrealLogParser
    {
        private readonly StackTracePathResolverCache myCache;
        private readonly string[] myInvalidPathChars;
        private readonly ISolution mySolution;

        public UnrealLogParser(ISolution solution)
        {
            mySolution = solution;
            myCache = mySolution.GetComponent<StackTracePathResolverCache>();
            myInvalidPathChars =
                FileSystemDefinition.WindowsInvalidPathChars.Select(
                    c => c.ToString(CultureInfo.InvariantCulture)).Concat(" ").ToArray();
        }

        public CompositeNode Parse(string s)
        {
            var lexer = new UE4LogLexer(new ArrayBuffer(s));
            lexer.Start();
            var rootNode = new CompositeNode(new TextRange(0, lexer.Buffer.Length));
            while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED)
            {
                if (ParseScriptMsg(lexer, out var scriptMsgNode))
                {
                    rootNode.AppendNode(scriptMsgNode);
                    continue;
                }

                if (ParseIdentifier(lexer, out var classReferenceNode))
                {
                    rootNode.AppendNode(classReferenceNode);
                    continue;
                }

                if (ParseBlueprintPath(lexer, out var blueprintPathNode))
                {
                    rootNode.AppendNode(blueprintPathNode);
                    continue;
                }

                /*var pathState = lexer.SaveState();
                var pathNode = ParsePath(lexer);
                if (pathNode != null)
                {
                    rootNode.AppendNode(pathNode);
                    continue;
                }

                lexer.RestoreState(pathState);*/

                rootNode.AppendNode(new TextNode(new TextRange(lexer.TokenStart, lexer.TokenEnd), lexer.TokenText));
                lexer.Advance();
            }

            return rootNode;
        }

        private bool ParseBlueprintPath(UE4LogLexer lexer, out BlueprintPathNode blueprintPathNode)
        {
            var initialState = lexer.SaveState();
            var separator = "/";
            while (lexer.TokenText == separator)
            {
                lexer.Advance();
                while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED &&
                       lexer.TokenText != separator &&
                       (lexer.TokenType == UE4LogTokenTypes.IDENTIFIER ||
                        lexer.TokenType == UE4LogTokenTypes.COLON ||
                        lexer.TokenType == UE4LogTokenTypes.DOT))
                    lexer.Advance();
            }

            var textRange = new TextRange(initialState.BufferStart, lexer.TokenStart);

            if (textRange.Length < 2)
            {
                blueprintPathNode = null;
                return false;
            }

            var blueprintReference = new BlueprintReference(new FString(lexer.Buffer.GetText(textRange)));

            blueprintPathNode = new BlueprintPathNode(textRange, blueprintReference);
            return true;
        }

        private static void SkipWhiteSpace(UE4LogLexer lexer, string fullName = null)
        {
            while (lexer.TokenType == UE4LogTokenTypes.WHITESPACE)
            {
                if (fullName != null)
                    fullName += lexer.TokenText;
                lexer.Advance();
            }
        }

        private bool AdvanceIfStartsWith(UE4LogLexer lexer, string s)
        {
            var saveState = lexer.SaveState();
            var stringLexer = new UE4LogLexer(new ArrayBuffer(s));
            stringLexer.Start();
            while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED && stringLexer.TokenType != UE4LogTokenTypes.UNDEFINED)
            {
                if (lexer.TokenType == stringLexer.TokenType && lexer.TokenText == stringLexer.TokenText)
                {
                    lexer.Advance();
                    stringLexer.Advance();
                    continue;
                }

                lexer.RestoreState(saveState);
                return false;
            }

            return stringLexer.TokenType == UE4LogTokenTypes.UNDEFINED;
        }

        private bool ParseScriptMsg(UE4LogLexer lexer, out ScriptMsgNode scriptMsgNode)
        {
            /*
             * Script msg: ...
             */
            bool ParseScriptMsgHeader(out ScriptMsgHeaderNode resultNode)
            {
                var saveState = lexer.SaveState();
                if (!AdvanceIfStartsWith(lexer, "Script Msg: "))
                {
                    lexer.RestoreState(saveState);
                    resultNode = null;
                    return false;
                }

                while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED &&
                       lexer.TokenType != UE4LogTokenTypes.TAB)
                    //todo parse
                    lexer.Advance();

                resultNode = new ScriptMsgHeaderNode(new TextRange(saveState.BufferStart, lexer.TokenStart));
                return true;
            }

            ScriptStackFrameNode ParseScriptStackFrame()
            {
                lexer.Advance(); // TAB

                var outerState = lexer.SaveState();
                while (lexer.TokenType != UE4LogTokenTypes.DOT) lexer.Advance();

                var outerNodeTextRange = new TextRange(outerState.BufferStart, lexer.TokenStart);
                var outerName = lexer.Buffer.GetText(outerNodeTextRange);
                var outerNode = new ScriptStackFrameOuterNode(outerNodeTextRange, new FString(outerName));

                lexer.Advance(); // '.'

                ForceUpdate(lexer);

                var innerState = lexer.SaveState();
                while (lexer.TokenType != UE4LogTokenTypes.UNDEFINED && lexer.TokenType != UE4LogTokenTypes.EOL)
                    lexer.Advance();

                var innerNodeTextRange = new TextRange(innerState.BufferStart, lexer.TokenStart);
                var innerName = lexer.Buffer.GetText(innerNodeTextRange);
                var innerNode = new ScriptStackFrameInnerNode(innerNodeTextRange, new FString(innerName), outerNode);

                lexer.Advance(); // eol

                return new ScriptStackFrameNode(new TextRange(outerState.BufferStart, lexer.TokenStart),
                    outerNode,
                    innerNode);
            }

            IEnumerable<ScriptStackFrameNode> CollectStackFrames()
            {
                while (lexer.TokenType == UE4LogTokenTypes.TAB)
                {
                    lexer.Advance();
                    yield return ParseScriptStackFrame();
                }
            }

            var initialState = lexer.SaveState();

            if (!ParseScriptMsgHeader(out var scriptMsgHeaderNode))
            {
                scriptMsgNode = null;
                return false;
            }

            var beforeStackState = lexer.SaveState();

            var stackFrameNodes = CollectStackFrames().AsList();

            scriptMsgNode = new ScriptMsgNode(new TextRange(initialState.BufferStart, lexer.TokenStart))
            {
                HeaderNode = scriptMsgHeaderNode,
                StackNode = new ScriptStackNode(new TextRange(beforeStackState.BufferStart, lexer.TokenStart))
                {
                    FrameNodes = stackFrameNodes
                }
            };
            return true;
        }

        private static void ForceUpdate(UE4LogLexer lexer)
        {
            if (lexer.TokenType == UE4LogTokenTypes.IDENTIFIER || lexer.TokenType == UE4LogTokenTypes.WORD
            ) //TODO: this is awful bug of lexer, it doesnt go to next symbol on the fly, we need to use construction like this to force it to update itself
            {
            }
        }
    }
}