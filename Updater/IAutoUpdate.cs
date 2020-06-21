
using System;

namespace Updater
{
    public interface IAutoUpdate
    {
        string Version { get; }
        string ManifestUrl { get; }
        string PackageUrl { get; }
        OsCommand StopCommand { get; }
        OsCommand StartCommand { get; }
    }
}
