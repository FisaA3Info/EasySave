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
using System.Collections.Generic;
using Microsoft.VisualBasic;

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
            if (string.IsNullOrWhiteSpace(NewJobName) || string.IsNullOrWhiteSpace(NewJobSource) || string.IsNullOrWhiteSpace(NewJobTarget))
            {
                StatusMessage = "Veuillez remplir tous les champs.";
                return;
            }

            bool success = _backupManager.CreateJob(NewJobName, NewJobSource, NewJobTarget, NewJobType);
            StatusMessage = success ? "Job créé avec succès." : "Echec de la création.";

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
                StatusMessage = "Sélectionnez un job d'abord.";
                return;
            }

            IsExecuting = true;
            StatusMessage = $"Exécution du job {index}...";
            bool success = await Task.Run(() => _backupManager.ExecuteJob(index));
            StatusMessage = success ? "Job terminé." : "Echec du job.";
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteAllJobsAsync()
        {
            if (_backupManager.BackupJobs.Count == 0)
            {
                StatusMessage = "Aucun job à exécuter.";
                return;
            }

            IsExecuting = true;
            StatusMessage = "Exécution de tous les jobs...";
            await Task.Run(() => _backupManager.ExecuteAllJobs());
            StatusMessage = "Tous les jobs ont été exécutés.";
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private async Task ExecuteRangeAsync()
        {
            IsExecuting = true;
            StatusMessage = $"Exécution des jobs {RangeStart} à {RangeEnd}...";
            await Task.Run(() => _backupManager.ExecuteRange(RangeStart, RangeEnd));
            StatusMessage = "Exécution de la plage terminée.";
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
                StatusMessage = "Entrez des numéros séparés par des points-virgules.";
                return;
            }

            IsExecuting = true;
            StatusMessage = "Exécution des jobs sélectionnés...";
            await Task.Run(() => _backupManager.ExecuteSelection(indices));
            StatusMessage = "Job sélectionnés terminés.";
            IsExecuting = false;
            RefreshJobList();
        }

        [RelayCommand]
        private void DeleteJob()
        {
            int index = SelectedJobIndex + 1;
            if (index < 1)
            {
                StatusMessage = "Sélectionnez un job à supprimer.";
                return;
            }

            bool success = _backupManager.DeleteJob(index);
            StatusMessage = success ? "Job supprimé." : "Echec de la suppression.";
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