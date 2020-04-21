using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node
{
    public class BlueprintPathNode : StackTraceNode
    {
        public BlueprintPathNode(TextRange range, [NotNull] BlueprintReference pathName) : base(range)
        {
            PathName = pathName;
        }

        [NotNull] public BlueprintReference PathName { get; }

        public override void Accept(StackTraceVisitor visitor)
        {
            ((UnrealLogVisitor) visitor).VisitBlueprintPathNode(this);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{BlueprintPath: ");
            writer.Write("}");
        }
    }
}