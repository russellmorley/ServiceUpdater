using System;

namespace Common
{
    public enum InformType
    {
        VersionUpdate
    }
    public class InformItem
    {
        public DateTime Date { get; set; }
        public InformType InformType { get; set; }
        public string DeployId { get; set; }
        public string Version { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
