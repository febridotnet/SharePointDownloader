using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace SharePointDownloaderApp.UI
{
    public partial class MainWindow : Window
    {
        private IScheduler _scheduler;
        private readonly FileDownloaderService _downloaderService;

        public MainWindow()
        {
            InitializeComponent();
            _downloaderService = new FileDownloaderService();

            // Subskripsi log dari Quartz Job
            DownloadJob.OnLogGenerated += Log;

            // Jalankan scheduler dan update tampilan label jadwal saat aplikasi dibuka
            InitScheduler();
        }

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

        private async void InitScheduler()
        {
            try
            {
                DownloaderSettings config = LoadSettings();
                string cronExpression = string.IsNullOrWhiteSpace(config.CronSchedule)
                    ? "0 0 4 * * ?"
                    : config.CronSchedule.Trim();

                // Validasi format Cron
                if (!CronExpression.IsValidExpression(cronExpression))
                {
                    Log($"[ERROR] Format CronSchedule '{cronExpression}' di appsettings.json TIDAK VALID!");
                    Log("[SYSTEM] Menggunakan jadwal fallback default: '0 0 4 * * ?' (Jam 04:00 AM).");
                    cronExpression = "0 0 4 * * ?";
                }

                // Update teks keterangan pada label GUI
                TxtJadwal.Text = GetCronDescription(cronExpression);

                StdSchedulerFactory factory = new StdSchedulerFactory();
                _scheduler = await factory.GetScheduler();
                await _scheduler.Start();

                IJobDetail job = JobBuilder.Create<DownloadJob>()
                    .WithIdentity("downloadJob", "group1")
                    .Build();

                TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("downloadTrigger", "group1")
                    .WithCronSchedule(cronExpression, x => x.InTimeZone(targetTimeZone))
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
                Log($"[SYSTEM] Scheduler Quartz aktif dengan Cron Pattern: '{cronExpression}'.");
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Gagal menginisialisasi scheduler: {ex.Message}");
            }
        }

        // Helper untuk mengubah Cron Expression menjadi teks yang mudah dibaca
        private string GetCronDescription(string cron)
        {
            return CronParserHelper.ToHumanReadable(cron);
        }

        private async void BtnDownloadNow_Click(object sender, RoutedEventArgs e)
        {
            BtnDownloadNow.IsEnabled = false;
            await _downloaderService.DownloadFileAsync(Log);
            BtnDownloadNow.IsEnabled = true;
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLog.AppendText(message + Environment.NewLine);
                TxtLog.ScrollToEnd();
            });
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown();
            }
            base.OnClosed(e);
        }
    }
}