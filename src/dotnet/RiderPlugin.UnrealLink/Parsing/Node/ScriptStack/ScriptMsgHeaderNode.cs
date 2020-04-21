using System.IO;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptMsgHeaderNode : StackTraceNode
    {
        public ScriptMsgHeaderNode(TextRange range) : base(range)
        {
        }

        public override void Accept(StackTraceVisitor visitor)
        {
            ((UnrealLogVisitor) visitor).VisitScriptMsgHeaderNode(this);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{ScriptMsgHeader: ");
            writer.Write("}");
        }
    }
}