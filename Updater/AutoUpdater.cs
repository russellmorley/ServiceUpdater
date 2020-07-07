using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    public class AutoUpdater
    {
        private const string VERSION_JSON = "version.json";
        private const string AUTOUPDATER_PROCESS_EXEC_BASE = "UpdaterProcess";
        private const string BEARER_TOKEN_NAME = "Bearer";

        private readonly IAutoUpdate _AutoUpdateComponent;
        private Version _VersionServerComponent;
        private bool _NeedUpdate;
        private VersionResponse _VersionResponse;
        private string _executable;

        public AutoUpdater(IAutoUpdate autoUpdateComponent)
        {
            _AutoUpdateComponent = autoUpdateComponent;
            _executable = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? $"{AUTOUPDATER_PROCESS_EXEC_BASE}.exe" : AUTOUPDATER_PROCESS_EXEC_BASE;

        }

        public string TargetVersion => _VersionResponse?.TargetVersion;
        
        public async Task<bool> CheckUpdateAvailableAsync(string authenticationBearerToken)
        {
            //1. Get current version
            //2. Get address
            //3. Make a call to the json
            CancellationToken cancellationToken = new CancellationToken();
            AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue(BEARER_TOKEN_NAME, authenticationBearerToken);
            string url = _AutoUpdateComponent.ManifestUrl;
            _VersionResponse = await Loader.LoadAsync<VersionResponse>(cancellationToken, authenticationHeaderValue, HttpMethod.Get, null, url); // + VERSION_JSON);
            Version versionCurrentComponent = new Version(_AutoUpdateComponent.Version);
            _VersionServerComponent = new Version(_VersionResponse.TargetVersion);
            //4. Compare versions
            _NeedUpdate = _VersionServerComponent.CompareTo(versionCurrentComponent) > 0;                       
            return _NeedUpdate;
        }

        public async Task<string> UpdateAsync(string targetDir, string authenticationBearerToken)
        {
            if (_VersionServerComponent == null)
            {
                await CheckUpdateAvailableAsync(authenticationBearerToken);
            }
            if (_NeedUpdate)
            {
                UpdateAutoUpdaterExec(targetDir);
                //1. Download the zipfile
                CancellationToken cancellationToken = new CancellationToken();
                string url = _AutoUpdateComponent.PackageUrl;
                AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue(BEARER_TOKEN_NAME, authenticationBearerToken);
                Stream zippedStream = await Loader.LoadStreamAsync(cancellationToken, authenticationHeaderValue, HttpMethod.Post, new {ZipFileName = _VersionResponse.UrlZipFile }, url); // + _VersionResponse.UrlZipFile);
                //2. Unzip the file
                string tempDir = Path.Combine(Path.GetTempPath(), _VersionServerComponent.ToString());
                ZipHelper.Unzip(zippedStream, tempDir);
                //4. Start process and pass SourceDir and TargetDir and how to launch the application afterwards
                StringBuilder standardOutput = new StringBuilder();
                int exitCode = Utilities.ExecuteCommand(_executable,
                    $" {Quote(Process.GetCurrentProcess().Id.ToString())}" +
                    $" {Quote(tempDir)}" +
                    $" {Quote(targetDir)}" +
                    $" {Quote(_AutoUpdateComponent.StopCommand?.FileName)}" +
                    $" {Quote(_AutoUpdateComponent.StopCommand?.Arguments)}" +
                    $" {Quote(_AutoUpdateComponent.StopCommand?.WaitForExit.ToString())}" +
                    $" {Quote(_AutoUpdateComponent.StartCommand?.FileName)}" +
                    $" {Quote(_AutoUpdateComponent.StartCommand?.Arguments)}" +
                    $" {Quote(_AutoUpdateComponent.StartCommand?.WaitForExit.ToString())}", ref standardOutput, true, targetDir);
                if (exitCode != 0) //Error
                {
                    return standardOutput.ToString();
                }
            }
            return string.Empty;
        }

        private void UpdateAutoUpdaterExec(string targetDir)
        {
            //Try to update the autoupdater itself
            FileInfo autoUpdaterExec = new FileInfo(Path.Combine(targetDir, _executable));
            if (autoUpdaterExec.Exists)
            {
                FileInfo autoUpdaterExecBackup = new FileInfo(Path.Combine(targetDir, _executable + ".backup"));
                if (autoUpdaterExecBackup.Exists)
                {
                    autoUpdaterExec.Delete();
                    autoUpdaterExecBackup.MoveTo(Path.Combine(targetDir, _executable));
                }
            }
        }

        private static string Quote(string input)
        {
            return $"\"{ input}\"";
        }
    }
}
