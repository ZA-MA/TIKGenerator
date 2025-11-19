using System;
using System.Windows;
using System.Windows.Controls;

namespace TIKGenerator.Views.Components.LanguageSelector
{
    public partial class LanguageSelector : UserControl
    {
        public LanguageSelector()
        {
            InitializeComponent();

            string currentLang = App.GetCurrentLanguage();
            BtnRu.IsChecked = currentLang == "ru";
            BtnEn.IsChecked = currentLang == "en";

            BtnRu.Click += (s, e) => ChangeLanguage("ru");
            BtnEn.Click += (s, e) => ChangeLanguage("en");
        }

        private void ChangeLanguage(string lang)
        {
            App.SetLanguage(lang);

            BtnRu.IsChecked = lang == "ru";
            BtnEn.IsChecked = lang == "en";
        }
    }
}
