using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Util;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptStackNode : /*LogMessageNode*/ StackTraceNode
    {
        public ScriptStackNode(TextRange range) : base(range)
        {
        }

        [CanBeNull] public ScriptStackHeaderNode HeaderNode { get; set; }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<ScriptStackFrameNode> FrameNodes { get; set; } = new List<ScriptStackFrameNode>();

        public override void Accept(StackTraceVisitor visitor)
        {
            HeaderNode?.Accept(visitor);
            foreach (var scriptStackFrameNode in FrameNodes) scriptStackFrameNode.Accept(visitor);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{ScriptStack: ");
            HeaderNode?.Dump(writer);
            writer.WriteLine();
            foreach (var frameNode in FrameNodes)
            {
                frameNode.Dump(writer);
                writer.WriteLine();
            }

            writer.Write("}");
        }
    }
}