using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.Metadata.Reader.API;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Rider.Model;
using JetBrains.Unreal.Lib;
using JetBrains.Util.Special;
using ReSharperPlugin.UnrealEditor.Parsing;
using ReSharperPlugin.UnrealEditor.Parsing.Highlighting;
using ReSharperPlugin.UnrealEditor.Parsing.Node;
using ReSharperPlugin.UnrealEditor.Parsing.Parser;
using ReSharperPlugin.UnrealEditor.Parsing.Resolver;

namespace ReSharperPlugin.UnrealEditor
{
    [SolutionComponent]
    public class UnrealToolWindowHost
    {
        private readonly UnrealLogHighlightingFactory myHighlightingFactory;
        private readonly IdentifierResolver myIdentifierResolver;
        private readonly Lifetime myLifetime;
        private readonly ISolution mySolution;
        private readonly StackTraceOptions myStackTraceOptions;
        private readonly UnrealHost myUnrealHost;
        private readonly List<UnrealTabModel> myUnrealTabs = new List<UnrealTabModel>();
        private readonly IShellLocks myLocks;

        public UnrealToolWindowHost(Lifetime lifetime, ISolution solution, UnrealHost unrealHost,
            IdentifierResolver identifierResolver, StackTraceOptions stackTraceOptions,
            UnrealLogHighlightingFactory highlightingFactory)
        {
            myLifetime = lifetime;
            mySolution = solution;
            myLocks = solution.Locks;
            myUnrealHost = unrealHost;
            myIdentifierResolver = identifierResolver;
            myStackTraceOptions = stackTraceOptions;
            myHighlightingFactory = highlightingFactory;
        }

        public UnrealTabModel AddTab(string projectName, RdEditorModel unrealModel)
        {
            var unrealTabModel = new UnrealTabModel(this, myHighlightingFactory);
            var unrealPane = CreateUnrealPane(projectName, unrealModel, unrealTabModel);
            unrealTabModel.UnrealPane = unrealPane;

            myUnrealTabs.Add(unrealTabModel);

            myUnrealHost.PerformModelAction(riderModel => { riderModel.ToolWindowModel.Tabs.Add(unrealPane); });

            return unrealTabModel;
        }

        private UnrealPane CreateUnrealPane(string projectName, RdEditorModel unrealModel,
            UnrealTabModel unrealTabModel)
        {
            return new UnrealPane(projectName).With(pane =>
            {
                pane.NavigateIdentifier.Advise(myLifetime,
                    highlighter => { NavigateIdentifier(unrealTabModel, highlighter); });
                pane.NavigateBlueprint.Advise(myLifetime,
                    blueprint => OnOpenedBlueprint(unrealModel, blueprint));
            });
        }

        public void Highlight(UnrealLogEvent message, UnrealTabModel unrealTabModel, RdEditorModel unrealModel)
        {
            var parser = new UnrealLogParser(mySolution);
            var unrealLogUpdateManager = new UnrealLogUpdateManager(
                parser,
                myStackTraceOptions,
                myIdentifierResolver,
                myLifetime, mySolution.Locks);

            if (!myLifetime.IsAlive)
                return;
            myLocks.ExecuteOrQueueReadLock(myLifetime, "Unreal log highlighter",
                () => unrealLogUpdateManager.Start(message, unrealTabModel, unrealModel));
        }

        public void NavigateIdentifier(UnrealTabModel tabModel, UnrealLogIdentifierHighlighter highlighter)
        {
            tabModel.NavigateIdentifier(highlighter);
        }

        private void OnOpenedBlueprint(RdEditorModel unrealModel, BlueprintReference blueprintReference)
        {
            unrealModel.OpenBlueprint(blueprintReference);
        }
    }

    public class UnrealTabModel
    {
        private readonly ConcurrentDictionary<UnrealLogIdentifierHighlighter, IDeclaredElement> myDeclaredElements =
            new ConcurrentDictionary<UnrealLogIdentifierHighlighter, IDeclaredElement>();

        private readonly UnrealLogHighlightingFactory myHighlightingFactory;

        private readonly UnrealToolWindowHost myToolWindowHost;

        public UnrealTabModel(UnrealToolWindowHost toolWindowHost,
            UnrealLogHighlightingFactory highlightingFactory)
        {
            myToolWindowHost = toolWindowHost;
            myHighlightingFactory = highlightingFactory;
        }

        public UnrealPane UnrealPane { get; set; }

        public void NavigateIdentifier(UnrealLogIdentifierHighlighter highlighter)
        {
            var declaredElement = myDeclaredElements[highlighter];
            using (ReadLockCookie.Create())
            {
                using (CompilationContextCookie.GetOrCreate(UniversalModuleReferenceContext.Instance))
                {
                    declaredElement.Navigate(true);
                }
            }
        }

        internal UnrealLogHighlighter ProcessHighlighter(UnrealLogHighlighterData data, int messageNumber)
        {
            InterruptableActivityCookie.CheckAndThrow();

            var riderHighlighter = myHighlightingFactory.CreateRiderHighlighter(data, messageNumber);
            var navigationNode = data.NavigationNode;
            switch (navigationNode)
            {
                case IdentifierNode identifierNode:
                {
                    if (identifierNode.CppDeclaredElement != null)
                        myDeclaredElements[(UnrealLogIdentifierHighlighter) riderHighlighter] =
                            identifierNode.CppDeclaredElement;
                    if (identifierNode.CsDeclaredElement != null)
                        myDeclaredElements[(UnrealLogIdentifierHighlighter) riderHighlighter] =
                            identifierNode.CsDeclaredElement;
                    break;
                }
            }

            return riderHighlighter;
        }
    }
}