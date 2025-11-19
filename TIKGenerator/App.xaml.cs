using System.Configuration;
using System.Data;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TIKGenerator
{
    public partial class App : Application
    {
        private static string _currentLang = "ru";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LiveCharts.Configure(config => config.AddSkiaSharp());

            SetLanguage("ru");
        }

        public static void SetLanguage(string lang)
        {
            _currentLang = lang;

            ResourceDictionary dict = new()
            {
                Source = new Uri($"/Resources/Languages/Strings.{lang}.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        public static string GetCurrentLanguage() => _currentLang;
    }
}
