namespace SharePointDownloaderApp.UI
{
    public class DownloaderSettings
    {
        public string SharepointUrl { get; set; } = string.Empty;
        public string TargetDirectory { get; set; } = string.Empty;
        public string TargetFileName { get; set; } = string.Empty;
        public string CronSchedule { get; set; } = "0 0 4 * * ?";
    }
}