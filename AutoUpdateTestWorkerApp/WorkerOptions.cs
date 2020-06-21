namespace AutoUpdateTestWorkerApp
{
    public class WorkerOptions
    {
        public string ServiceName { get; set; }
        public string WinUrl { get; set; }
        public string MacUrl { get; set; }
        public string PackageUrl { get; set; }
        public string ManagerUrl { get; set; }
        public int BeginningOfDayLocalHour { get; set; }
        public int EndOfDayLocalHour { get; set; }
    }
}