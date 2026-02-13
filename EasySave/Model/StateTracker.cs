using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EasySave.Model
{
    public class StateTracker
    {
        public string FilePath { get; set; }
        public List<StateEntry> States { get; private set; }
        private List<IStateObserver> observers;

        public StateTracker(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string easySaveFolder = Path.Combine(appDataPath, "EasySave");

                Directory.CreateDirectory(easySaveFolder);

                filePath = Path.Combine(easySaveFolder, "state.json");
            }
            FilePath = filePath;
            States = new List<StateEntry>();
            observers = new List<IStateObserver>();
            LoadState();
        }

        //Observer Management Section
        public void AttachObserver(IStateObserver observer)
        {
            if(observer == null) return;
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }
           
        public void DetachObserver(IStateObserver observer)
        {
            if (observer == null) return;
            observers.Remove(observer);
        }

        private void NotifyObservers(StateEntry entry)
        {
            foreach (var observer in observers)
            {
                observer.OnStateChanged(entry);
            }
        }

        // StateTracker base method 
        public void UpdateState(StateEntry entry)
        {
            //Stock the result of FirstOrDefault on States which return the first value that statify the search or return null
            var existing = States.FirstOrDefault(search => search.JobName == entry.JobName);
            if (existing != null)
            {
                States.Remove(existing);
            }
            States.Add(entry);
            SaveState();
            NotifyObservers(entry);
        }

        public void SaveState()
        {
            var json = JsonSerializer.Serialize(States, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public void LoadState()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                States = JsonSerializer.Deserialize<List<StateEntry>>(json)
                    ?? new List<StateEntry>();
            }
        }

        public StateEntry? GetState(string jobName)
        {
            return States.FirstOrDefault(search => search.JobName == jobName);
        }
        public void RemoveState(string jobName)
        {
            var existing = States.FirstOrDefault(search => search.JobName == jobName);
            if (existing != null)
            {
                States.Remove(existing);
                SaveState();
            }
        }

    }
}
