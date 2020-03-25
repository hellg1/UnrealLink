using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Lifetimes;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes;
using ReSharperPlugin.UnrealEditor.Parsing.Highlighting;
using ReSharperPlugin.UnrealEditor.Parsing.Node;
using ReSharperPlugin.UnrealEditor.Parsing.Node.ScriptStack;
using ReSharperPlugin.UnrealEditor.Parsing.Resolver;
using IdentifierNode = ReSharperPlugin.UnrealEditor.Parsing.Node.IdentifierNode;

namespace ReSharperPlugin.UnrealEditor.Parsing.Visitor
{
    public class UnrealLogHighlightVisitor : UnrealLogVisitor
    {
        public UnrealLogHighlightVisitor(StackTraceOptions options, Lifetime lifetime, RdEditorModel unrealModel,
            IdentifierResolver identifierResolver) :
            base(options)
        {
            MyLifetime = lifetime;
            UnrealModel = unrealModel;
            IdentifierResolver = identifierResolver;
        }

        public Lifetime MyLifetime { get; }
        public RdEditorModel UnrealModel { get; }
        public IIdentifierResolver IdentifierResolver { get; }

        [NotNull]
        [ItemNotNull]
        public List<UnrealLogHighlighterData> CollectedHighlighters { get; } = new List<UnrealLogHighlighterData>();

        public override void VisitIdentifierNode(IdentifierNode identifierNode)
        {
            identifierNode.Qualifier?.Accept(this);

            var attribute = IdentifierResolver.ResolveAttributeId(identifierNode);
            if (attribute != null)
            {
                var highlighterData = new UnrealLogHighlighterData(identifierNode.Range, attribute, identifierNode);
                CollectedHighlighters.Add(highlighterData);
            }
        }

        public override void VisitScriptStackHeaderNode(ScriptStackHeaderNode scriptStackHeaderNode)
        {
            var highlighterData = new UnrealLogHighlighterData(scriptStackHeaderNode.Range,
                UnrealLogHighlightingAttributeIds.ERROR_MESSAGE,
                scriptStackHeaderNode);
            CollectedHighlighters.Add(highlighterData);
        }

        public override void VisitBlueprintPathNode(BlueprintPathNode blueprintPathNode)
        {
            var isBlueprintPathName = UnrealModel.IsBlueprintPathName.Sync(blueprintPathNode.PathName.PathName);
            //todo async?
            if (isBlueprintPathName)
            {
                var highlighterData = new UnrealLogHighlighterData(blueprintPathNode.Range,
                    UnrealLogHighlightingAttributeIds.BLUEPRINT,
                    blueprintPathNode
                );
                CollectedHighlighters.Add(highlighterData);
            }
        }

        public override void VisitScriptStackFrameOuterNode(ScriptStackFrameOuterNode scriptStackFrameOuterNode)
        {
            var highlighterData = new UnrealLogHighlighterData(scriptStackFrameOuterNode.Range,
                UnrealLogHighlightingAttributeIds.SCRIPT_STACK_FRAME_OUTER,
                scriptStackFrameOuterNode
            );
            CollectedHighlighters.Add(highlighterData);
        }

        public override void VisitScriptStackFrameInnerNode(ScriptStackFrameInnerNode scriptStackFrameInnerNode)
        {
            var highlighterData = new UnrealLogHighlighterData(scriptStackFrameInnerNode.Range,
                UnrealLogHighlightingAttributeIds.SCRIPT_STACK_FRAME_INNER,
                scriptStackFrameInnerNode
            );
            CollectedHighlighters.Add(highlighterData);
        }

        public override void VisitScriptMsgHeaderNode(ScriptMsgHeaderNode scriptMsgHeaderNode)
        {
            var highlighterData = new UnrealLogHighlighterData(scriptMsgHeaderNode.Range,
                UnrealLogHighlightingAttributeIds.ERROR_MESSAGE,
                scriptMsgHeaderNode);
            CollectedHighlighters.Add(highlighterData);
        }

        public override void VisitResolvedPath(PathNode pathNode)
        {
            var highlighterData = new UnrealLogHighlighterData(pathNode.Range,
                UnrealLogHighlightingAttributeIds.PATH,
                pathNode
            );
            CollectedHighlighters.Add(highlighterData);
        }

        public override void VisitText(TextNode node)
        {
            throw new NotImplementedException();
        }

        public override void VisitParameter(ParameterNode node)
        {
            throw new NotImplementedException();
        }

        public override void VisitResolvedNode(
            JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace.Nodes.IdentifierNode node)
        {
            throw new NotImplementedException();
        }
    }
}