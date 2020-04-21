using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Util;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptMsgNode : StackTraceNode
    {
        public ScriptMsgNode(TextRange range) : base(range)
        {
        }

        [CanBeNull] public ScriptMsgHeaderNode HeaderNode { get; set; }
        [CanBeNull] public ScriptStackNode StackNode { get; set; }

        public override void Accept(StackTraceVisitor visitor)
        {
            HeaderNode?.Accept(visitor);
            StackNode?.Accept(visitor);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{ScriptMsg: ");
            writer.WriteLine();
            HeaderNode?.Dump(writer);
            writer.WriteLine();
            StackNode?.Dump(writer);
            writer.WriteLine();
            writer.Write("}");
        }
    }
}