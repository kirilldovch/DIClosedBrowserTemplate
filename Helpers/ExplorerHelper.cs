using System.Diagnostics;

namespace DIClosedBrowserTemplate.Helpers
{
    public static class ExplorerHelper
    {
        public static void KillExplorer()
        {
            if (DebugHelper.IsRunningInDebugMode)
                return;

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM explorer.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            process?.WaitForExit();
        }

        public static void RunExplorer()
        {
            if (DebugHelper.IsRunningInDebugMode)
                return;


            if (!Process.GetProcessesByName("explorer").Any())
                Process.Start(@"C:\Windows\explorer.exe");
        }
    }
}
