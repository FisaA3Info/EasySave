using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyLog;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel;
using EasySaveInterface.Converters;
using HarfBuzzSharp;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
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
        Delete,
        ListJobs,
        Settings
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly BackupManager _backupManager;
        private readonly StateTracker _stateTracker;
        private readonly SettingsService _settingsService;

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
        private int _newJobTypeIndex = 0;

        // Selection
        [ObservableProperty]
        private int _selectedJobIndex = -1;

        public bool HasJobSelected => SelectedJobIndex >= 0;

        partial void OnSelectedJobIndexChanged(int value)
        {
            OnPropertyChanged(nameof(HasJobSelected));
        }

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

        // Settings properties
        [ObservableProperty]
        private string _cryptoSoftPath = "";

        [ObservableProperty]
        private string _encryptionKey = "";

        [ObservableProperty]
        private string _encryptedExtensions = "";

        [ObservableProperty]
        private string _businessSoftwareName = "";

        [ObservableProperty]
        private string _priorityExtensions = "";

        [ObservableProperty]
        private string _logMode = "local"; // "local", "centralized", "both"

        [ObservableProperty]
        private string _logServerUrl = "";

        [ObservableProperty]
        private string _machineName = Environment.MachineName;

        [ObservableProperty]
        private string _userName = Environment.UserName;

        public ObservableCollection<string> Languages { get; } = new() { "Français", "English" };
        public ObservableCollection<string> LogFormats { get; } = new() { "JSON", "XML" };
        public ObservableCollection<string> BackupTypeNames { get; } = new();
        public ObservableCollection<string> LogModes { get; } = new() { "local", "centralized", "both" };

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
        public string TextListJobs => GetText("list_jobs");
        public string TextWarningTitle => GetText("headwarning_title");
        public string TextWarningMsg => GetText("warning_msg");
        public string TextSettings => GetText("settings");
        public string TextSettingsTitle => GetText("settings_title");
        public string TextCryptoSoftPath => GetText("crypto_soft_path");
        public string TextEncryptionKey => GetText("encryption_key");
        public string TextEncryptedExtensions => GetText("encrypted_extensions");
        public string TextBusinessSoftware => GetText("business_software_name");
        public string TextBtnSaveSettings => GetText("btn_save_settings");

        public string TextPriorityExtensions => GetText("priority_extensions");
        public string TextSettingsSaved => GetText("settings_saved");
        public string TextLogMode => GetText("txt_log_mode");
        public string TextMachineName => GetText("txt_machine_name");
        public string TextUserName => GetText("txt_user_name");
        public string TextUrlLogServer => GetText("txt_url_log_server");
        public string TextLogModeIndication => GetText("txt_log_mode_indication");
        public string TextBrowse => GetText("browse");
        public string TextBrowserTitle => GetText("browser_title");

        public bool HasJobs => Jobs.Count > 0;

        public bool IsCreatePage => CurrentPage == PageType.Create;
        public bool IsExecuteJobPage => CurrentPage == PageType.ExecuteJob;
        public bool IsExecuteAllPage => CurrentPage == PageType.ExecuteAll;
        public bool IsExecuteRangePage => CurrentPage == PageType.ExecuteRange;
        public bool IsExecuteSelectionPage => CurrentPage == PageType.ExecuteSelection;
        public bool IsDeletePage => CurrentPage == PageType.Delete;
        public bool IsListJobsPage => CurrentPage == PageType.ListJobs;
        public bool IsSettingsPage => CurrentPage == PageType.Settings;
        public bool ShowJobList => CurrentPage != PageType.Create && CurrentPage != PageType.None;

        partial void OnCurrentPageChanged(PageType value)
        {
            OnPropertyChanged(nameof(IsCreatePage));
            OnPropertyChanged(nameof(IsExecuteJobPage));
            OnPropertyChanged(nameof(IsExecuteAllPage));
            OnPropertyChanged(nameof(IsExecuteRangePage));
            OnPropertyChanged(nameof(IsExecuteSelectionPage));
            OnPropertyChanged(nameof(IsDeletePage));
            OnPropertyChanged(nameof(IsListJobsPage));
            OnPropertyChanged(nameof(IsSettingsPage));
            OnPropertyChanged(nameof(ShowJobList));
            StatusMessage = "";
            SelectedJobIndex = -1;
        }

        public MainWindowViewModel()
        {
            _stateTracker = new StateTracker();
            _settingsService = new SettingsService();
            var businessService = new BusinessSoftwareService(_settingsService.Settings.BusinessSoftwareName);
            _backupManager = new BackupManager(_stateTracker, _settingsService.Settings, businessService);
            // Set logger configuration from settings
            Logger.LogType = _settingsService.Settings.LogFormat;
            Logger.LogMode = _settingsService.Settings.LogMode;
            Logger.LogServerUrl = _settingsService.Settings.LogServerUrl;
            LoadSettings();
            

           

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
            OnPropertyChanged(nameof(TextWarningTitle));
            OnPropertyChanged(nameof(TextWarningMsg));
            OnPropertyChanged(nameof(TextListJobs));
            OnPropertyChanged(nameof(TextSettings));
            OnPropertyChanged(nameof(TextSettingsTitle));
            OnPropertyChanged(nameof(TextCryptoSoftPath));
            OnPropertyChanged(nameof(TextEncryptionKey));
            OnPropertyChanged(nameof(TextEncryptedExtensions));
            OnPropertyChanged(nameof(TextBusinessSoftware));
            OnPropertyChanged(nameof(TextBtnSaveSettings));
            OnPropertyChanged(nameof(TextSettingsSaved));
            OnPropertyChanged(nameof(TextLogMode));
            OnPropertyChanged(nameof(TextMachineName));
            OnPropertyChanged(nameof(TextUserName));
            OnPropertyChanged(nameof(TextUrlLogServer));
            OnPropertyChanged(nameof(TextLogModeIndication));
            OnPropertyChanged(nameof(TextPriorityExtensions));
            OnPropertyChanged(nameof(TextBrowse));
            OnPropertyChanged(nameof(TextBrowserTitle));

            // Mettre à jour les noms de types traduits
            BackupTypeConverter.FullText = GetText("BackupSelectionFull");
            BackupTypeConverter.DifferentialText = GetText("BackupSelectionDifferential");
            BackupTypeNames.Clear();
            BackupTypeNames.Add(GetText("BackupSelectionFull"));
            BackupTypeNames.Add(GetText("BackupSelectionDifferential"));

            RefreshJobList();
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

        partial void OnSelectedLogFormatChanged(string value)
        {
            Logger.LogType = value.ToLower();
        }
        partial void OnSelectedLogModeChanged(string value)
        {
            string mode = value switch
            {
                "Centralized" => "centralized",
                "Both" => "both",
                _ => "local"
            };
            Logger.LogMode = mode;
            _settingsService.Settings.LogMode = mode;
            _settingsService.Save();
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

        [RelayCommand]
        private void GoToListJobs()
        {
            CurrentPage = PageType.ListJobs;
            RefreshJobList();
        }

        [RelayCommand]
        private void GoToSettings()
        {
            CurrentPage = PageType.Settings;
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

            if (!Directory.Exists(NewJobSource))
            {
                StatusMessage = GetText("error_source_not_found");
                return;
            }

            BackupType type = NewJobTypeIndex == 0 ? BackupType.Full : BackupType.Differential;
            bool success = _backupManager.CreateJob(NewJobName, NewJobSource, NewJobTarget, type);
            StatusMessage = success ? GetText("success_created") : GetText("error_max_jobs");

            if (success)
            {
                NewJobName = "";
                NewJobSource = "";
                NewJobTarget = "";
                NewJobTypeIndex = 0;
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

            if (!CheckSourceDirs(new List<BackupJob> { _backupManager.BackupJobs[index - 1] }))
                return;

            IsExecuting = true;
            StatusMessage = string.Format(GetText("executing_job"), index);
            bool success = await _backupManager.ExecuteJob(index);
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

            if (!CheckSourceDirs(_backupManager.BackupJobs))
                return;

            IsExecuting = true;
            await _backupManager.ExecuteAllJobs();
            StatusMessage = GetText("success_executed");
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteRangeAsync()
        {
            var parts = SelectedIndicesText.Split(';', ',');
            if (parts.Length < 2
                || !int.TryParse(parts[0].Trim(), out int start)
                || !int.TryParse(parts[1].Trim(), out int end))
            {
                StatusMessage = GetText("invalid_choice");
                return;
            }

            int maxJob = _backupManager.BackupJobs.Count;
            if (start < 1 || end < 1 || start > maxJob || end > maxJob)
            {
                StatusMessage = GetText("job_not_found");
                return;
            }

            if (!CheckSourceDirs(_backupManager.BackupJobs))
                return;

            IsExecuting = true;
            await _backupManager.ExecuteRange(start, end);
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

            int maxJob = _backupManager.BackupJobs.Count;
            if (indices.Any(i => i < 1 || i > maxJob))
            {
                StatusMessage = GetText("job_not_found");
                return;
            }

            if (!CheckSourceDirs(_backupManager.BackupJobs))
                return;

            IsExecuting = true;
            await _backupManager.ExecuteSelection(indices);
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

        private bool CheckSourceDirs(List<BackupJob> jobs)
        {
            foreach (var job in jobs)
            {
                if (!Directory.Exists(job.SourceDir))
                {
                    StatusMessage = $"{GetText("error_source_not_found")} ({job.Name})";
                    return false;
                }
            }
            return true;
        }

        [RelayCommand]
        private void RefreshJobList()
        {
            Jobs.Clear();
            foreach (var job in _backupManager.BackupJobs)
            {
                Jobs.Add(job);
            }
            OnPropertyChanged(nameof(HasJobs));
        }

        private void LoadSettings()
        {
            CryptoSoftPath = _settingsService.Settings.CryptoSoftPath;
            EncryptionKey = _settingsService.Settings.EncryptionKey;
            EncryptedExtensions = string.Join(";", _settingsService.Settings.EncryptedExtensions);
            BusinessSoftwareName = _settingsService.Settings.BusinessSoftwareName;
            SelectedLogFormat = _settingsService.Settings.LogFormat.ToUpper();
            PriorityExtensions = string.Join(";", _settingsService.Settings.PriorityExtensions);
        }

        [RelayCommand]
        private void SaveSettings()
        {
            _settingsService.Settings.CryptoSoftPath = CryptoSoftPath;
            _settingsService.Settings.EncryptionKey = EncryptionKey;
            // get the extensions
            var extensionList = new List<string>();
            var parts = EncryptedExtensions.Split(new[] { ';', ',' });
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    extensionList.Add(trimmedPart);
                }
            }
            _settingsService.Settings.EncryptedExtensions = extensionList;
            _settingsService.Settings.BusinessSoftwareName = BusinessSoftwareName;
            _settingsService.Settings.LogFormat = SelectedLogFormat.ToLower();

            // get priority extensions
            var priorityList = new List<string>();
            var priorityParts = PriorityExtensions.Split(new[] { ';', ',' });
            foreach (var part in priorityParts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    priorityList.Add(trimmed);
                }
            }
            _settingsService.Settings.PriorityExtensions = priorityList;

            _settingsService.Save();
            StatusMessage = GetText("settings_saved");
        }
        
    }
}