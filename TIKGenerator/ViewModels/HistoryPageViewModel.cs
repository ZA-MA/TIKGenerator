using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TIKGenerator.Models;
using TIKGenerator.Services;
using TIKGenerator.Views.Pages;

namespace TIKGenerator.ViewModels
{
    public class HistoryPageViewModel : INotifyPropertyChanged
    {
        private readonly ISignalGroupService _groupService;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private const int PageSize = 10;
        private int _currentPage = 1;

        public ObservableCollection<SignalGroup> Groups { get; } = new();
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                _ = LoadPageAsync();
            }
        }

        public int TotalPages { get; private set; }

        private List<SignalGroup> _allGroups = new();
        public ICommand OpenGroupCommand { get; }

        public HistoryPageViewModel(ISignalGroupService groupService)
        {
            _groupService = groupService;

            OpenGroupCommand = new RelayCommand(OpenGroup);
        }

        public async Task LoadAllGroupsAsync()
        {
            try
            {
                IsLoading = true;
                _allGroups = await _groupService.GetAllGroupsAsync();
                TotalPages = (int)Math.Ceiling(_allGroups.Count / (double)PageSize);
                OnPropertyChanged(nameof(TotalPages));
                _currentPage = 1;
                await LoadPageAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPageAsync()
        {
            IsLoading = true;
            try
            {
                await Task.Delay(50);
                Groups.Clear();

                var pageItems = _allGroups
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                foreach (var item in pageItems)
                    Groups.Add(item);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenGroup(object parameter)
        {
            var group = parameter as SignalGroup;
            if (group == null) return;

            var page = new GeneratorPage(group);

            App.Current.Dispatcher.Invoke(() =>
            {
                var navFrame = App.Current.MainWindow.FindName("MainFrame") as System.Windows.Controls.Frame;
                navFrame?.Navigate(page);
            });
        }

        public async void NextPage()
        {
            if (CurrentPage < TotalPages) CurrentPage++;
            await LoadPageAsync();
        }

        public async void PreviousPage()
        {
            if (CurrentPage > 1) CurrentPage--;
            await LoadPageAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
