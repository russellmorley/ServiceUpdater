using Updater;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AutoUpdateTestWorkerApp
{
    class AutoUpdateImpl : IAutoUpdate
    {
        protected string _version;
        protected string _manifestUrl;
        protected string _packageUrl;
        protected OsCommand _startCommand;
        protected OsCommand _stopCommand;

        public AutoUpdateImpl(string windowsManifestUrl, string macManifestUrl, string packageUrl, string serviceName)
        {
            _version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            _packageUrl = packageUrl;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _manifestUrl = windowsManifestUrl;

                OsCommand startOsCommand = new OsCommand();
                if (!serviceName.Equals(""))
                {
                    startOsCommand.FileName = "cmd";
                    startOsCommand.Arguments = $"/c sc start {serviceName}";
                    startOsCommand.WaitForExit = true;
                }
                else
                {
                    startOsCommand.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    startOsCommand.Arguments = "";
                    startOsCommand.WaitForExit = false;
                }
                _startCommand = startOsCommand;

                OsCommand stopOsCommand = new OsCommand();
                if (!serviceName.Equals(""))
                {
                    stopOsCommand.FileName = "cmd";
                    stopOsCommand.Arguments = $"/c sc stop {serviceName}";
                    stopOsCommand.WaitForExit = true;
                }
                else
                {
                    stopOsCommand.FileName = "cmd";
                    stopOsCommand.Arguments = $"/c taskkill /PID {System.Diagnostics.Process.GetCurrentProcess().Id.ToString()}";
                    stopOsCommand.WaitForExit = true;
                }
                _stopCommand = stopOsCommand;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _manifestUrl = macManifestUrl;

                OsCommand startOsCommand = new OsCommand();
                startOsCommand.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                startOsCommand.Arguments = "";
                startOsCommand.WaitForExit = false;
                _startCommand = startOsCommand;

                OsCommand stopOsCommand = new OsCommand();
                stopOsCommand.FileName = "kill"; //killall
                stopOsCommand.Arguments = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                stopOsCommand.WaitForExit = true;
                _stopCommand = stopOsCommand;
            }
        }
        public string Version => _version;
        public string ManifestUrl => _manifestUrl;
        public string PackageUrl => _packageUrl;
        public OsCommand StopCommand => _stopCommand;
        public OsCommand StartCommand => _startCommand;
    }
}
