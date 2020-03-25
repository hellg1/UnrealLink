using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Util;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptStackFrameNode : StackTraceNode
    {
        public ScriptStackFrameNode(TextRange range,
            [NotNull] ScriptStackFrameOuterNode outerNode,
            [NotNull] ScriptStackFrameInnerNode innerNode) : base(range)
        {
            InnerNode = innerNode;
            OuterNode = outerNode;
        }

        [NotNull] internal ScriptStackFrameOuterNode OuterNode { get; }
        [NotNull] internal ScriptStackFrameInnerNode InnerNode { get; }

        public override void Accept(StackTraceVisitor visitor)
        {
            OuterNode.Accept(visitor);
            InnerNode.Accept(visitor);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{StackFrame:");
            writer.Write("}");
        }
    }
}