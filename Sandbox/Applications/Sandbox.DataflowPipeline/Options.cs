using System;

namespace DataflowPipeline
{
    internal static class Options
    {
        public static TimeSpan Delay { get; } = TimeSpan.FromSeconds(1);

        public static bool ShouldThrowException { get; } = false;

        public static bool Output { get; } = false;
    }
}
