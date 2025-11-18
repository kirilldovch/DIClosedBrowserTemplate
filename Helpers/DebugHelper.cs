using System.Diagnostics;

namespace DIClosedBrowserTemplate.Helpers
{
    public static class DebugHelper
    {
        public static bool IsRunningInDebugMode { get; private set; }

        static DebugHelper()
        {
            Initialize();
        }

        [Conditional("DEBUG")]
        private static void Initialize()
        {
            IsRunningInDebugMode = true;
        }
    }
}
