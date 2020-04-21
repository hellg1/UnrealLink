using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Rider.Model;
using ReSharperPlugin.UnrealEditor.Parsing.Node;
using ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack;
using IdentifierNode = ReSharperPlugin.UnrealEditor.Parsing.Node.IdentifierNode;

namespace ReSharperPlugin.UnrealEditor.Parsing.Highlighting
{
    [SolutionComponent]
    public class UnrealLogHighlightingFactory
    {
        private int myHighlighterIdCounter;

        internal UnrealLogHighlighter CreateRiderHighlighter(UnrealLogHighlighterData highlighterData,
            int messageNumber)
        {
            var attributeId = highlighterData.AttributeId;
            var start = highlighterData.Range.StartOffset;
            var end = highlighterData.Range.EndOffset;
            return highlighterData.NavigationNode switch
            {
                IdentifierNode _ =>
                new UnrealLogIdentifierHighlighter(messageNumber, myHighlighterIdCounter++, attributeId, start, end),

                PathNode pathNode =>
                new UnrealLogPathHighlighter(pathNode.Path, messageNumber, myHighlighterIdCounter++, attributeId,
                    start, end),
                BlueprintPathNode blueprintPathNode =>
                new UnrealLogBlueprintLinkHighlighter(blueprintPathNode.PathName,
                    messageNumber, myHighlighterIdCounter++, attributeId, start, end),

                ScriptMsgHeaderNode _ =>
                new UnrealLogScriptMsgHeaderHighlighter(messageNumber, myHighlighterIdCounter++, attributeId, start,
                    end),

                ScriptStackFrameOuterNode scriptStackFrameOuterNode =>
                new UnrealLogStackFrameOuterHighlighter(scriptStackFrameOuterNode.Name,
                    messageNumber, myHighlighterIdCounter++, attributeId, start, end),
                ScriptStackFrameInnerNode scriptStackFrameInnerNode =>
                new UnrealLogStackFrameInnerHighlighter(scriptStackFrameInnerNode.OuterNode.Name,
                    scriptStackFrameInnerNode.Name,
                    messageNumber, myHighlighterIdCounter++, attributeId, start, end),

                null =>
                new UnrealLogDefaultHighlighter(messageNumber, myHighlighterIdCounter++, attributeId, start, end),
                _ => null
            };
        }
    }
}