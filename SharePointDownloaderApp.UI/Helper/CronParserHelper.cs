using CronExpressionDescriptor;
using System;

namespace SharePointDownloaderApp.UI
{
    public static class CronParserHelper
    {
        /// <summary>
        /// Mengubah Cron Expression Quartz menjadi deskripsi bahasa Indonesia.
        /// </summary>
        public static string ToHumanReadable(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                return "Automatic Schedule: [Not Configured]";
            }

            try
            {
                // Set opsi penerjemah dan tetapkan Locale ke "id" (Indonesian)
                Options options = new Options
                {
                    Use24HourTimeFormat = true,
                    DayOfWeekStartIndexZero = true,
                    Locale = "id" // Mengatur bahasa ke Indonesia
                };

                // Panggil GetDescription dengan 2 parameter (expression, options)
                string description = ExpressionDescriptor.GetDescription(cronExpression.Trim(), options);

                return $"Automatic Schedule: {description} (GMT+7)";
            }
            catch (Exception)
            {
                // Fallback jika ekspresi Cron invalid atau gagal di-parse
                return $"Automatic Schedule: Pattern Cron '{cronExpression}' (GMT+7)";
            }
        }
    }
}