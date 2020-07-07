using System;
using System.Diagnostics;
using System.Text;

namespace Updater
{
    internal static class Utilities
    {
        internal static int ExecuteCommand(string command, string arguments, ref StringBuilder standardOutput, bool waitBeforeExiting, string workingDir = null)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = command;
            processStartInfo.Arguments = arguments;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            if (workingDir != null)
            {
                processStartInfo.WorkingDirectory = workingDir;
            }
            if (waitBeforeExiting)
            {
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
            }
            var proc = new System.Diagnostics.Process
            {
                StartInfo = processStartInfo
            };

            using (proc)
            {
                proc.Start();
                if (waitBeforeExiting)
                {
                    standardOutput.AppendLine(proc.StandardOutput.ReadToEnd() + Environment.NewLine);
                    standardOutput.AppendLine(proc.StandardError.ReadToEnd());
                    proc.WaitForExit();
                    return proc.ExitCode;
                }
                return 0;
            }
        }
    }
}
