using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using JetBrains.Util.Logging;
using ReSharperPlugin.UnrealEditor.Parsing.Parser;
using ReSharperPlugin.UnrealEditor.Parsing.Resolver;

namespace ReSharperPlugin.UnrealEditor.Parsing
{
    public class UnrealLogUpdateManager
    {
        private static readonly ILogger OurLog = Logger.GetLogger<StackTraceUpdateManager>();
        private readonly IdentifierResolver myIdentifierResolver;
        private readonly Lifetime myLifetime;
        private readonly IShellLocks myLocks;
        private readonly UnrealLogParser myParser;
        private readonly StackTraceOptions myStackTraceOptions;

        public UnrealLogUpdateManager(
            UnrealLogParser parser,
            StackTraceOptions stackTraceOptions,
            IdentifierResolver identifierResolver,
            Lifetime lifetime,
            IShellLocks locks)
        {
            myParser = parser;
            myStackTraceOptions = stackTraceOptions;
            myIdentifierResolver = identifierResolver;
            myLifetime = lifetime;
            myLocks = locks;
        }

        public void Start(UnrealLogEvent message, UnrealTabModel unrealTabModel, RdEditorModel unrealModel)
        {
            myLocks.AssertMainThread();
            myLocks.AssertReadAccessAllowed();
            var updateStateToken = new UnrealLogUpdateStateToken(this, myLocks);
            StartNextRequest(message, unrealTabModel, unrealModel, updateStateToken);
        }

        internal void StartNextRequest(UnrealLogEvent message, UnrealTabModel unrealTabModel, RdEditorModel unrealModel,
            UnrealLogUpdateStateToken updateStateToken)
        {
            myLocks.AssertMainThread();
            myLocks.AssertReadAccessAllowed();
            if (!myLifetime.IsAlive)
                return;

            OurLog.Verbose("STE: Creating Read Activity, message number {0}", message.Number);

            var readActivity = new UnrealLogHighlightTextReadActivity(message,
                unrealTabModel,
                unrealModel,
                updateStateToken,
                this,
                myParser,
                myStackTraceOptions,
                myIdentifierResolver,
                myLifetime, myLocks,
                () => !myLifetime.IsAlive);
            readActivity.DoStart();
        }

        public void FinishRequest(UnrealLogUpdateStateToken updateStateToken)
        {
            OurLog.Verbose("STE: FinishRequest: {0}", updateStateToken);
        }
    }
}