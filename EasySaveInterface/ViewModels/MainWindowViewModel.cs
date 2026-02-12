using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Model;
using EasySave.ViewModel;
using HarfBuzzSharp;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasySaveInterface.ViewModels
{
    public enum PageType
    {
        None,
        Create,
        ExecuteJob,
        ExecuteAll,
        ExecuteRange,
        ExecuteSelection,
        Delete
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly BackupManager _backupManager;
        private readonly StateTracker _stateTracker;

        // Job list
        [ObservableProperty]
        private ObservableCollection<BackupJob> _jobs = new();

        // Navigation
        [ObservableProperty]
        private PageType _currentPage = PageType.None;

        // Create job form
        [ObservableProperty]
        private string _newJobName = "";

        [ObservableProperty]
        private string _newJobSource = "";

        [ObservableProperty]
        private string _newJobTarget = "";

        [ObservableProperty]
        private BackupType _newJobType = BackupType.Full;

        // Selection
        [ObservableProperty]
        private int _selectedJobIndex = -1;

        // Range execution
        [ObservableProperty]
        private int _rangeStart = 1;

        [ObservableProperty]
        private int _rangeEnd = 1;

        // Selection execution (text "1;3;5")
        [ObservableProperty]
        private string _selectedIndicesText = "";

        // UI state
        [ObservableProperty]
        private bool _isExecuting;

        [ObservableProperty]
        private string _statusMessage = "";

        // Language / Log format
        [ObservableProperty]
        private string _selectedLanguage = "Français";

        [ObservableProperty]
        private string _selectedLogFormat = "JSON";

        public ObservableCollection<string> Languages { get; } = new() { "Français", "English" };
        public ObservableCollection<string> LogFormats { get; } = new() { "JSON", "XML" };
        public ObservableCollection<BackupType> BackupTypes { get; } = new() { BackupType.Full, BackupType.Differential };

        public bool IsCreatePage => CurrentPage == PageType.Create;
        public bool IsExecuteJobPage => CurrentPage == PageType.ExecuteJob;
        public bool IsExecuteAllPage => CurrentPage == PageType.ExecuteAll;
        public bool IsExecuteRangePage => CurrentPage == PageType.ExecuteRange;
        public bool IsExecuteSelectionPage => CurrentPage == PageType.ExecuteSelection;
        public bool IsDeletePage => CurrentPage == PageType.Delete;
        public bool ShowJobList => CurrentPage != PageType.Create && CurrentPage != PageType.None;

        partial void OnCurrentPageChanged(PageType value)
        {
            OnPropertyChanged(nameof(IsCreatePage));
            OnPropertyChanged(nameof(IsExecuteJobPage));
            OnPropertyChanged(nameof(IsExecuteAllPage));
            OnPropertyChanged(nameof(IsExecuteRangePage));
            OnPropertyChanged(nameof(IsExecuteSelectionPage));
            OnPropertyChanged(nameof(IsDeletePage));
            OnPropertyChanged(nameof(ShowJobList));
            StatusMessage = "";
            SelectedJobIndex = -1;
        }

        public MainWindowViewModel()
        {
            _stateTracker = new StateTracker();
            _backupManager = new BackupManager(_stateTracker);
            RefreshJobList();
        }

        // ===== Navigation commands =====

        [RelayCommand]
        private void GoToCreate()
        {
            CurrentPage = PageType.Create;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToExecuteJob()
        {
            CurrentPage = PageType.ExecuteJob;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToExecuteAll()
        {
            CurrentPage = PageType.ExecuteAll;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToExecuteRange()
        {
            CurrentPage = PageType.ExecuteRange;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToExecuteSelection()
        {
            CurrentPage = PageType.ExecuteSelection;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToDelete()
        {
            CurrentPage = PageType.Delete;
            RefreshJobList();
        }

        // ===== Action commands =====
        [RelayCommand]
        private void CreateJob()
        {

        }

        [RelayCommand]
        private async Task ExecuteJobAsync()
        {

        }

        [RelayCommand]
        private async Task ExecuteAllJobsAsync()
        {

        }

        [RelayCommand]
        private async Task ExecuteRangeAsync()
        {

        }

        [RelayCommand]
        private async Task ExecuteSelectedJobsAsync()
        {

        }

        [RelayCommand]
        private void DeleteJob()
        {

        }

        [RelayCommand]
        private void RefreshJobList()
        {

        }
    }
}
