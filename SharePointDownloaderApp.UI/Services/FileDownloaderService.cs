using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointDownloaderApp.UI
{
    public class FileDownloaderService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private DownloaderSettings LoadSettings()
        {
            IConfiguration builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            DownloaderSettings settings = new DownloaderSettings();
            builder.GetSection("DownloaderSettings").Bind(settings);

            return settings;
        }

        public async Task<bool> DownloadFileAsync(Action<string> logCallback)
        {
            try
            {
                // Load parameter konfigurasi dinamis dari appsettings.json
                DownloaderSettings config = LoadSettings();

                if (string.IsNullOrWhiteSpace(config.SharepointUrl))
                {
                    logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] ERROR: SharepointUrl belum dikonfigurasi di appsettings.json.");
                    return false;
                }

                // Konversi URL SharePoint ke Direct Download URL
                string directDownloadUrl = config.SharepointUrl.Contains("?")
                    ? $"{config.SharepointUrl}&download=1"
                    : $"{config.SharepointUrl}?download=1";

                // Gunakan direktori default jika konfigurasi kosong
                string targetDir = string.IsNullOrWhiteSpace(config.TargetDirectory)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads")
                    : config.TargetDirectory;

                string targetFileName = string.IsNullOrWhiteSpace(config.TargetFileName)
                    ? "DownloadedFile.xlsx"
                    : config.TargetFileName;

                // Pastikan folder target ada
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string destinationPath = Path.Combine(targetDir, targetFileName);

                // Cek dan hapus file lama jika sudah ada
                if (File.Exists(destinationPath))
                {
                    logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] Memeriksa folder: File lama '{targetFileName}' ditemukan.");
                    File.Delete(destinationPath);
                    logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] File lama berhasil dihapus.");
                }

                logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] Memulai proses unduh dari SharePoint...");

                using (HttpResponseMessage response = await _httpClient.GetAsync(directDownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    using (Stream streamToWriteTo = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }

                logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] Selesai! File berhasil disimpan di: {destinationPath}");
                return true;
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"[{DateTime.Now:HH:mm:ss}] ERROR: Gagal mengunduh atau menyimpan file: {ex.Message}");
                return false;
            }
        }
    }
}