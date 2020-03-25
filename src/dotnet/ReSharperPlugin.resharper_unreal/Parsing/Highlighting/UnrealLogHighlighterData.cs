using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Util;

namespace ReSharperPlugin.UnrealEditor.Parsing.Highlighting
{
    public class UnrealLogHighlighterData
    {
        public UnrealLogHighlighterData(TextRange range, [NotNull] string attributeId,
            [CanBeNull] StackTraceNode navigationNode)
        {
            AttributeId = attributeId;
            NavigationNode = navigationNode;
            Range = range;
        }

        [NotNull] public string AttributeId { get; }

        [CanBeNull] public StackTraceNode NavigationNode { get; set; }

        public TextRange Range { get; }
    }
}