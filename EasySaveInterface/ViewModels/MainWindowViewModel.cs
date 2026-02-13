using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Model;
using EasySave.ViewModel;
using HarfBuzzSharp;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
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

        private Dictionary<string, string> _translations = new();
        public string TextCreateBackup => GetText("create_backup");
        public string TextExecuteBackup => GetText("execute_backup");
        public string TextExecuteAll => GetText("execute_all");
        public string TextExecuteRange => GetText("execute_range");
        public string TextExecuteSpecific => GetText("execute_specific");
        public string TextDeleteJobs => GetText("delete_jobs");
        public string TextPromptName => GetText("prompt_name");
        public string TextPromptSource => GetText("prompt_source");
        public string TextPromptTarget => GetText("prompt_target");
        public string TextPromptType => GetText("prompt_type");
        public string TextLabelLanguage => GetText("label_language");
        public string TextLabelLogFormat => GetText("label_log_format");
        public string TextMenuLabel => GetText("menu_label");
        public string TextSelectJobExecute => GetText("select_job_execute");
        public string TextLabelSource => GetText("label_source");
        public string TextLabelTarget => GetText("label_target");
        public string TextBtnExecute => GetText("btn_execute");
        public string TextJobsWillExecute => GetText("jobs_will_execute");
        public string TextBtnExecuteAll => GetText("btn_execute_all");
        public string TextJobsAvailable => GetText("jobs_available");
        public string TextEnterNumbersSemicolon => GetText("enter_numbers_semicolon");
        public string TextBtnExecuteRange => GetText("btn_execute_range");
        public string TextBtnExecuteSelection => GetText("btn_execute_selection");
        public string TextSelectJobDelete => GetText("select_job_delete");
        public string TextBtnDelete => GetText("btn_delete");
        public string TextMenuTitle => GetText("menu_title");

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
            LoadLanguage("fr");
            RefreshJobList();
        }

        private void LoadLanguage(string lang)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Ressources", $"{lang}.json");
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                    ?? new Dictionary<string, string>();
                }
            }
            catch { }

            OnPropertyChanged(nameof(TextCreateBackup));
            OnPropertyChanged(nameof(TextExecuteBackup));
            OnPropertyChanged(nameof(TextExecuteAll));
            OnPropertyChanged(nameof(TextExecuteRange));
            OnPropertyChanged(nameof(TextExecuteSpecific));
            OnPropertyChanged(nameof(TextDeleteJobs));
            OnPropertyChanged(nameof(TextPromptName));
            OnPropertyChanged(nameof(TextPromptSource));
            OnPropertyChanged(nameof(TextPromptTarget));
            OnPropertyChanged(nameof(TextPromptType));
            OnPropertyChanged(nameof(TextLabelLanguage));
            OnPropertyChanged(nameof(TextLabelLogFormat));
            OnPropertyChanged(nameof(TextMenuLabel));
            OnPropertyChanged(nameof(TextSelectJobExecute));
            OnPropertyChanged(nameof(TextLabelSource));
            OnPropertyChanged(nameof(TextLabelTarget));
            OnPropertyChanged(nameof(TextBtnExecute));
            OnPropertyChanged(nameof(TextJobsWillExecute));
            OnPropertyChanged(nameof(TextBtnExecuteAll));
            OnPropertyChanged(nameof(TextJobsAvailable));
            OnPropertyChanged(nameof(TextEnterNumbersSemicolon));
            OnPropertyChanged(nameof(TextBtnExecuteRange));
            OnPropertyChanged(nameof(TextBtnExecuteSelection));
            OnPropertyChanged(nameof(TextSelectJobDelete));
            OnPropertyChanged(nameof(TextBtnDelete));
            OnPropertyChanged(nameof(TextMenuTitle));
        }

        private string GetText(string key)
        {
            if (_translations.ContainsKey(key))
                return _translations[key];
            return $"[{key}]";
        }

        partial void OnSelectedLanguageChanged(string value)
        {
            if (value == "Français") LoadLanguage("fr");
            else LoadLanguage("en");
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
            if (string.IsNullOrWhiteSpace(NewJobName) || string.IsNullOrWhiteSpace(NewJobSource) || string.IsNullOrWhiteSpace(NewJobTarget))
            {
                StatusMessage = GetText("invalid_choice");
                return;
            }

            bool success = _backupManager.CreateJob(NewJobName, NewJobSource, NewJobTarget, NewJobType);
            StatusMessage = success ? GetText("success_created") : GetText("error_max_jobs");

            if (success)
            {
                NewJobName = "";
                NewJobSource = "";
                NewJobTarget = "";
                NewJobType = BackupType.Full;
                RefreshJobList();
            }
        }

        [RelayCommand]
        private async Task ExecuteJobAsync()
        {
            int index = SelectedJobIndex + 1;
            if (index < 1)
            {
                StatusMessage = GetText("invalid_choice");
                return;
            }

            IsExecuting = true;
            StatusMessage = string.Format(GetText("executing_job"), index);
            bool success = await Task.Run(() => _backupManager.ExecuteJob(index));
            StatusMessage = success ? GetText("success_executed") : GetText("error_executed");
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteAllJobsAsync()
        {
            if (_backupManager.BackupJobs.Count == 0)
            {
                StatusMessage = GetText("no_jobs");
                return;
            }

            IsExecuting = true;
            await Task.Run(() => _backupManager.ExecuteAllJobs());
            StatusMessage = GetText("success_executed");
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteRangeAsync()
        {
            IsExecuting = true;
            await Task.Run(() => _backupManager.ExecuteRange(RangeStart, RangeEnd));
            StatusMessage = GetText("success_executed");
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteSelectedJobsAsync()
        {
            var indices = new List<int>();
            if (!string.IsNullOrWhiteSpace(SelectedIndicesText))
            {
                foreach (var part in SelectedIndicesText.Split(';', ','))
                {
                    if (int.TryParse(part.Trim(), out int idx))
                    {
                        indices.Add(idx);
                    }
                }
            }

            if (indices.Count == 0)
            {
                StatusMessage = GetText("no_selection");
                return;
            }

            IsExecuting = true;
            await Task.Run(() => _backupManager.ExecuteSelection(indices));
            StatusMessage = GetText("success_executed");
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private void DeleteJob()
        {
            int index = SelectedJobIndex + 1;
            if (index < 1)
            {
                StatusMessage = GetText("invalid_choice");
                return;
            }

            bool success = _backupManager.DeleteJob(index);
            StatusMessage = success ? GetText("success_deleted") : GetText("error_delete");
            RefreshJobList();
        }

        [RelayCommand]
        private void RefreshJobList()
        {
            Jobs.Clear();
            foreach (var job in _backupManager.BackupJobs)
            {
                Jobs.Add(job);
            }
        }
    }
}