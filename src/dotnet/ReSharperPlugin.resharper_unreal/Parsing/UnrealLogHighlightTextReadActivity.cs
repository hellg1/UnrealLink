using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;
using JetBrains.Rider.Model;
using JetBrains.Threading;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using JetBrains.Util.Logging;
using ReSharperPlugin.UnrealEditor.Parsing.Parser;
using ReSharperPlugin.UnrealEditor.Parsing.Resolver;
using ReSharperPlugin.UnrealEditor.Parsing.Visitor;

namespace ReSharperPlugin.UnrealEditor.Parsing
{
    public class UnrealLogHighlightTextReadActivity : InterruptableReadActivity
    {
        private static readonly ILogger OurLog = Logger.GetLogger<UnrealLogHighlightTextReadActivity>();
        private readonly IdentifierResolver myIdentifierResolver;
        private readonly IShellLocks myLocks;
        private readonly UnrealLogUpdateManager myManager;
        private readonly UnrealLogEvent myMessage;
        private readonly UnrealLogParser myParser;
        private readonly StackTraceOptions myStackTraceOptions;

        private readonly TimeSpan myTimeDelayToStartInterruptedActivity = TimeSpan.FromMilliseconds(50);
        private readonly RdEditorModel myUnrealModel;
        private readonly UnrealTabModel myUnrealTabModel;
        private readonly UnrealLogUpdateStateToken myUpdateStateToken;

        private List<UnrealLogHighlighter> myHighlighters;

        public UnrealLogHighlightTextReadActivity(UnrealLogEvent message,
            UnrealTabModel unrealTabModel,
            RdEditorModel unrealModel,
            UnrealLogUpdateStateToken updateStateToken,
            UnrealLogUpdateManager unrealLogUpdateManager,
            UnrealLogParser parser,
            StackTraceOptions stackTraceOptions,
            IdentifierResolver identifierResolver,
            Lifetime lifetime,
            IShellLocks locks,
            Func<bool> checkForInterrupt = null) : base(lifetime, locks, checkForInterrupt)
        {
            myMessage = message;
            myUnrealTabModel = unrealTabModel;
            myUnrealModel = unrealModel;
            myUpdateStateToken = updateStateToken;
            myManager = unrealLogUpdateManager;
            myParser = parser;
            myStackTraceOptions = stackTraceOptions;
            myIdentifierResolver = identifierResolver;
            myLocks = locks;
        }

        protected override string ThreadName => "Unreal Log Interruptable ReadActivity for Text Highlighting";

        protected override void Start()
        {
            OurLog.Verbose("STE: Start, message number: {0}", myMessage.Number);

            myUpdateStateToken.Start();
        }

        protected override void Work()
        {
            OurLog.Verbose("STE: Work, message number: {0}", myMessage.Number);

            InterruptableActivityCookie.CheckAndThrow();
            var tree = myParser.Parse(myMessage.Text.Data);
            InterruptableActivityCookie.CheckAndThrow();
            var visitor =
                new UnrealLogHighlightVisitor(myStackTraceOptions, Lifetime, myUnrealModel, myIdentifierResolver);
            tree.Accept(visitor);
            InterruptableActivityCookie.CheckAndThrow();
            myHighlighters =
                visitor.CollectedHighlighters
                    .ToList(data => myUnrealTabModel.ProcessHighlighter(data, myMessage.Number))
                    .WhereNotNull()
                    .ToList();
        }

        protected override void OnInterrupt()
        {
            OurLog.Verbose("STE: Interrupt, message number: {0}", myMessage.Number);

            myUpdateStateToken.Interrupt();
            if (myUpdateStateToken.Status == UpdateStatus.Rejected)
                return;
            myLocks.TimedActions.Queue(Lifetime, "Unreal log Highlighter", () =>
                {
                    using (myLocks.UsingReadLock())
                    {
                        myManager.StartNextRequest(myMessage, myUnrealTabModel, myUnrealModel, myUpdateStateToken);
                    }
                }, myTimeDelayToStartInterruptedActivity,
                TimedActionsHost.Recurrence.OneTime,
                Rgc.Guarded);
        }

        protected override void Finish()
        {
            OurLog.Verbose("STE: Finish, message number: {0}", myMessage.Number);

            if (Lifetime.IsNotAlive)
                return;

            myUpdateStateToken.FinishStartHighlightings();
            myUnrealTabModel.UnrealPane.AddHighlighters(myHighlighters);
        }
    }
}