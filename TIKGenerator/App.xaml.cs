using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Windows;
using TIKGenerator.Data;
using TIKGenerator.Services;
using TIKGenerator.ViewModels;
using Serilog;
using Serilog.Extensions.Logging;
using System.IO;

namespace TIKGenerator
{
    public partial class App : Application
    {
        public static DbContextOptions<AppDbContext> DbOptions { get; private set; }

        private static string _currentLang = "ru";
        public static GeneratorViewModel GeneratorVm { get; private set; }
        public static HistoryPageViewModel HistoryVm { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(connectionString);
            DbOptions = optionsBuilder.Options;

            using (var db = new AppDbContext(DbOptions))
            {
                db.Database.EnsureCreated();
            }

            LiveCharts.Configure(config => config.AddSkiaSharp());

            SetLanguage("ru");

            var logFolder = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logPath = Path.Combine(logFolder, "log.txt");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog();
            var groupLogger = loggerFactory.CreateLogger<SignalGroupService>();

            var groupService = new SignalGroupService(DbOptions, groupLogger);

            GeneratorVm = new GeneratorViewModel(
                new SignalGeneratorService(),
                new SignalProcessingService(),
                groupService
            );

            HistoryVm = new HistoryPageViewModel(groupService);
        }

        public static void SetLanguage(string lang)
        {
            _currentLang = lang;

            var newDict = new ResourceDictionary
            {
                Source = new Uri($"/Resources/Languages/Strings.{lang}.xaml", UriKind.Relative)
            };

            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));

            if (oldDict != null)
            {
                int index = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                Application.Current.Resources.MergedDictionaries[index] = newDict;
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(newDict);
            }
        }

        public static string GetCurrentLanguage() => _currentLang;
    }
}
