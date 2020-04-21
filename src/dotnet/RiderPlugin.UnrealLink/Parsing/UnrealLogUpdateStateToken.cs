using JetBrains.Application.Threading;
using JetBrains.Util;
using JetBrains.Util.Logging;

namespace ReSharperPlugin.UnrealEditor.Parsing
{
    public enum UpdateStatus
    {
        Prepare,
        Working,
        Interrupted,
        Finished,
        HighlightingOccurrences,
        Rejected
    }

    public class UnrealLogUpdateStateToken
    {
        private readonly IShellLocks myLocks;
        private readonly UnrealLogUpdateManager myManager;

        public UnrealLogUpdateStateToken(UnrealLogUpdateManager manager, IShellLocks shellLocks)
        {
            myManager = manager;
            myLocks = shellLocks;
        }

        public UpdateStatus Status { get; set; }

        public void Start()
        {
            myLocks.AssertMainThread();
            if (Status != UpdateStatus.Prepare && Status != UpdateStatus.Interrupted)
                Logger.GetLogger<UnrealLogUpdateManager>()
                    .Warn("Trying to start stack trace update request with Status " + Status);
            //TODO investigate this case
            Status = UpdateStatus.Working;
        }

        public void FinishStartHighlightings()
        {
            myLocks.AssertMainThread();
            Logger.Assert(Status == UpdateStatus.Working,
                "Status of unreal log request is " + Status);
            Status = UpdateStatus.HighlightingOccurrences;
        }

        public void Finish()
        {
            myLocks.AssertMainThread();
            Logger.Assert(Status == UpdateStatus.Working || Status == UpdateStatus.HighlightingOccurrences,
                "Status of unreal log request is " + Status);
            Status = UpdateStatus.Finished;
            myManager.FinishRequest(this);
        }

        public void Reject()
        {
            myLocks.AssertMainThread();
            myLocks.AssertWriteAccessAllowed();
            Status = UpdateStatus.Rejected;
        }

        public void Interrupt( /*UnrealLogUpdateTokenCache cache*/)
        {
            myLocks.AssertMainThread();
            Logger.Assert(Status == UpdateStatus.Working || Status == UpdateStatus.Rejected,
                "Trying to interrupt unreal log update request with Status " + Status);
            Status = UpdateStatus.Interrupted;
            // Cache = cache;
        }
    }
}