using Quartz;
using System.Threading.Tasks;

namespace SharePointDownloaderApp.UI
{
    public class DownloadJob : IJob
    {
        public static Action<string> OnLogGenerated;

        public async Task Execute(IJobExecutionContext context)
        {
            FileDownloaderService downloader = new FileDownloaderService();
            await downloader.DownloadFileAsync(msg => OnLogGenerated?.Invoke(msg));
        }
    }
}