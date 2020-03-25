namespace ReSharperPlugin.UnrealEditor.Parsing.Parser
{
    public partial class UnrealLogParser
    {
//        [CanBeNull]
//        private PathNode ParsePath(UE4LogLexer myLexer)
//        {
//            var initialState = myLexer.SaveState();
//            var fullName = new StringBuilder();
//            var localName = new StringBuilder();
//            var localNameContainsDot = false;
//            var isFileLink = false;
//            if (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER
//                || myLexer.TokenType == UE4LogTokenTypes.WORD
//                || myLexer.TokenType == UE4LogTokenTypes.DOT
//                || myLexer.TokenType == UE4LogTokenTypes.PATH_SEPARATOR)
//            {
//                var initialPrefix = myLexer.TokenText;
//                fullName.Append(initialPrefix);
//                myLexer.Advance();
//
//                if (initialPrefix.Equals("file", StringComparison.OrdinalIgnoreCase) &&
//                    myLexer.TokenType == UE4LogTokenTypes.PATH_SEPARATOR)
//                    isFileLink = true;
//
//                while (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER || myLexer.TokenType == UE4LogTokenTypes.WORD ||
//                       myLexer.TokenType == UE4LogTokenTypes.DOT)
//                {
//                    InterruptableActivityCookie.CheckAndThrow();
//                    fullName.Append(myLexer.TokenText);
//                    myLexer.Advance();
//                }
//
//                if (myLexer.TokenType == UE4LogTokenTypes.PATH_SEPARATOR)
//                    while (myLexer.TokenType != UE4LogTokenTypes.UNDEFINED)
//                    {
//                        InterruptableActivityCookie.CheckAndThrow();
//                        if (myLexer.TokenText.Contains('\n'))
//                            //skip next lines, see https://youtrack.jetbrains.com/issue/RSRP-4464
//                        {
//                            myLexer.Advance();
//                            continue;
//                        }
//
//                        var localNamePart = myLexer.TokenText;
//                        fullName.Append(localNamePart);
//                        if (myLexer.TokenType == UE4LogTokenTypes.PATH_SEPARATOR)
//                        {
//                            localName.Clear();
//                            localNameContainsDot = false;
//                        }
//                        else
//                        {
//                            localName.Append(localNamePart);
//                            localNameContainsDot |= localNamePart.Contains('.');
//                        }
//
//                        myLexer.Advance();
//
//                        if (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER ||
//                            myLexer.TokenType == UE4LogTokenTypes.WORD
//                        ) //TODO: this is awful bug of lexer, it doesnt go to next symbol on the fly, we need to use construction like this to force it to update itself
//                        {
//                        }
//
//                        if (myLexer.TokenText == "#" && isFileLink
//                                                     && (
//                                                         localNameContainsDot
//                                                         || fullName[fullName.Length - 1] == '\\'
//                                                         || fullName[fullName.Length - 1] == '/')
//                        )
//                        {
//                            var stateIsCommand = myLexer.SaveState();
//                            myLexer.Advance();
//                            if (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER)
//                            {
//                                fullName.Append("#").Append(myLexer.TokenText);
//                                myLexer.Advance();
//                            }
//                            else
//                            {
//                                myLexer.RestoreState(stateIsCommand);
//                            }
//
//                            var range = new TextRange(initialState.BufferStart, myLexer.TokenStart);
//                            var node = PathNode.CreatePathNode(range, fullName, localName, true);
//                            StackTracePathResolver.ResolvePath(node, myCache, mySolution);
//
//                            return node;
//                        }
//
//                        if (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER ||
//                            myLexer.TokenType == UE4LogTokenTypes.WORD)
//                            continue;
//
//                        if (myLexer.TokenType > (UE4LogTokenTypes) 4) //it is a symbol
//                        {
//                            var text = myLexer.TokenText;
//                            if (myInvalidPathChars.Any(invalidPathChar => text.Contains(invalidPathChar)))
//                            {
//                                var range = new TextRange(initialState.BufferStart, myLexer.TokenStart);
//                                var node = PathNode.CreatePathNode(range, fullName, localName, isFileLink);
//                                StackTracePathResolver.ResolvePath(node, myCache, mySolution);
//                                myLexer.Advance();
//
//                                return node;
//                            }
//                        }
//
//                        if (myLexer.TokenType == UE4LogTokenTypes.IDENTIFIER)
//                        {
//                            var beforeIdentState = myLexer.SaveState();
//                            var name = myLexer.TokenText;
//                            var startInd = myLexer.TokenStart;
//
//                            myLexer.Advance();
//
//                            /*var identifierNode = ParseIdentifier(name, startInd);
//                            if (identifierNode?.Qualifier != null)
//                            {
//                                myLexer.RestoreState(beforeIdentState);
//                                var range = new TextRange(initialState.BufferStart, myLexer.TokenStart);
//
//                                var node = PathNode.CreatePathNode(range, fullName, localName, isFileLink);
//                                StackTracePathResolver.ResolvePath(node, myCache, mySolution);
//
//                                return node;
//                            }*/
//                            if (ParseIdentifier(myLexer, out _))
//                            {
//                                myLexer.RestoreState(beforeIdentState);
//                                var range = new TextRange(initialState.BufferStart, myLexer.TokenStart);
//
//                                var node = PathNode.CreatePathNode(range, fullName, localName, isFileLink);
//                                StackTracePathResolver.ResolvePath(node, myCache, mySolution);
//
//                                return node;
//                            }
//                        }
//
//
//                        if (myLexer.TokenType == UE4LogTokenTypes.PATH_SEPARATOR) continue;
//
//                        if (myLexer.TokenType == UE4LogTokenTypes.UNDEFINED ||
//                            myLexer.TokenType == UE4LogTokenTypes.COLON ||
//                            myLexer.TokenType == UE4LogTokenTypes.WHITESPACE &&
//                            localNameContainsDot &&
//                            localName.Length > 0 &&
//                            localName[localName.Length - 1] !=
//                            '.' && //local name contains dot, and dot is not a last symbol
//                            myLexer.TokenText.Contains("\n"))
//                        {
//                            var range = new TextRange(initialState.BufferStart, myLexer.TokenStart);
//
//                            var node = PathNode.CreatePathNode(range, fullName, localName, isFileLink);
//                            // if (!PreprocessUnityLogPath(node))
//                            // ParseCoordinates(node);
//                            StackTracePathResolver.ResolvePath(node, myCache, mySolution);
//                            myLexer.Advance();
//                            return node;
//                        }
//                    }
//            }
//
//            myLexer.Advance();
//            return null;
//        }
    }
}