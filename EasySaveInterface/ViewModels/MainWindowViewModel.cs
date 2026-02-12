using HarfBuzzSharp;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EasySaveInterface.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        public string Title { get; set; } = "EasySave";
        private string selectedLanguage = "Français";
        private string selectedLogFormat = "JSON";

        public ObservableCollection<string> Languages { get; }
        public ObservableCollection<string> LogFormats { get; }

        public string SelectedLanguage
        {
            get => selectedLanguage;
            set => SetProperty(ref selectedLanguage, value);
        }

        public string SelectedLogFormat
        {
            get => selectedLogFormat;
            set => SetProperty(ref selectedLogFormat, value);
        }

        // Commandes
        public ICommand CreateJobCommand { get; }
        public ICommand ExecuteJobCommand { get; }
        public ICommand ExecuteAllJobsCommand { get; }
        public ICommand ExecuteRangeCommand { get; }
        public ICommand ExecuteSelectedJobsCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ShowAllJobsCommand { get; }

        public MainWindowViewModel()
        {
            Languages = new ObservableCollection<string> { "Français", "English" };
            LogFormats = new ObservableCollection<string> { "JSON", "XML" };
            // Command Init 
            CreateJobCommand = ReactiveCommand.Create(() => { /* Create Job toussatoussa */ });
            ExecuteJobCommand = ReactiveCommand.Create(() => { /* Exec Job blabla */ });
            ExecuteAllJobsCommand = ReactiveCommand.Create(() => { /* Exec all job braaaaaaaaaw*/ });
            ExecuteRangeCommand = ReactiveCommand.Create(() => { /* Exec job range go brrrrr*/ });
            ExecuteSelectedJobsCommand = ReactiveCommand.Create(() => { /* Exec specific job yum yum */ });
            DeleteJobCommand = ReactiveCommand.Create(() => { /* Delete job hmmmm */ });
            ShowAllJobsCommand = ReactiveCommand.Create(() => { /* Show All job*/ });
        }



    }
}
