using System.IO;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack
{
    public class ScriptStackFrameInnerNode : StackTraceNode
    {
        public ScriptStackFrameInnerNode(TextRange range, FString name, ScriptStackFrameOuterNode outerNode) :
            base(range)
        {
            Name = name;
            OuterNode = outerNode;
        }

        public FString Name { get; }
        public ScriptStackFrameOuterNode OuterNode { get; }

        public override void Accept(StackTraceVisitor visitor)
        {
            ((UnrealLogVisitor) visitor).VisitScriptStackFrameInnerNode(this);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{ScriptStackFrameInnerNode: ");
            writer.Write("}");
        }
    }
}