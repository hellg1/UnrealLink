using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing.Node
{
    public class IdentifierNode : StackTraceNode
    {
        public IdentifierNode(TextRange range, string name, IdentifierNode qualifier = null) : base(range)
        {
            Name = name;
            Qualifier = qualifier;
        }

        public string Name { get; }
        public IdentifierNode Qualifier { get; }

        [CanBeNull] public CppParserSymbolDeclaredElement CppDeclaredElement { get; set; }
        [CanBeNull] public IClrDeclaredElement CsDeclaredElement { get; set; }

        public override void Accept(StackTraceVisitor visitor)
        {
            ((UnrealLogVisitor) visitor).VisitIdentifierNode(this);
        }

        public override void Dump(TextWriter writer)
        {
            writer.Write("{IdentifierNode: ");
            Qualifier?.Dump(writer);
            writer.Write("}");
        }
    }
}