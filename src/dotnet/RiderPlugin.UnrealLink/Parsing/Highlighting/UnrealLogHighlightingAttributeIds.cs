using JetBrains.ReSharper.Feature.Services.Daemon.Attributes;
using JetBrains.ReSharper.Feature.Services.StackTraces.StackTrace;

// ReSharper disable InconsistentNaming

namespace ReSharperPlugin.UnrealEditor.Parsing.Highlighting
{
    public static class UnrealLogHighlightingAttributeIds
    {
        public const string BLUEPRINT = PATH;
        public const string ERROR_MESSAGE = AnalysisHighlightingAttributeIds.UNRESOLVED_ERROR;
        public const string PATH = StackTraceHighlightingAttributeIds.PATH;
        public const string BUILD_MODULE = StackTraceHighlightingAttributeIds.TYPE;
        public const string SCRIPT_STACK_FRAME_OUTER = StackTraceHighlightingAttributeIds.TYPE;
        public const string SCRIPT_STACK_FRAME_INNER = StackTraceHighlightingAttributeIds.METHOD;
    }
}