using System.IO;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptStackFrameOuterNode : StackTraceNode
    {
        public ScriptStackFrameOuterNode(TextRange range, FString name) : base(range)
        {
            Name = name;
        }

        public FString Name { get; }

        public override void Accept(StackTraceVisitor visitor)
        {
            ((UnrealLogVisitor) visitor).VisitScriptStackFrameOuterNode(this);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{ScriptStackFrameOuterNode: ");
            writer.Write("}");
        }
    }
}