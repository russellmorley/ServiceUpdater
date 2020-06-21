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
                }
                else
                {
                    startOsCommand.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    startOsCommand.Arguments = "";
                }
                _startCommand = startOsCommand;

                OsCommand stopOsCommand = new OsCommand();
                if (!serviceName.Equals(""))
                {
                    stopOsCommand.FileName = "cmd"; //Assembly.GetExecutingAssembly().Location;
                    stopOsCommand.Arguments = $"/c sc stop {serviceName}";
                }
                else
                {
                    stopOsCommand.FileName = "cmd";
                    stopOsCommand.Arguments = $"/c taskkill /PID {System.Diagnostics.Process.GetCurrentProcess().Id.ToString()}"; //$"/IM {Assembly.GetExecutingAssembly().GetName().Name}.exe";
                }
                _stopCommand = stopOsCommand;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _manifestUrl = macManifestUrl;

                OsCommand startOsCommand = new OsCommand();
                startOsCommand.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName; //Assembly.GetExecutingAssembly().Location;
                startOsCommand.Arguments = "";
                _startCommand = startOsCommand;

                OsCommand stopOsCommand = new OsCommand();
                stopOsCommand.FileName = "kill"; //killall
                stopOsCommand.Arguments = System.Diagnostics.Process.GetCurrentProcess().Id.ToString(); //Assembly.GetExecutingAssembly().GetName().Name;
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
