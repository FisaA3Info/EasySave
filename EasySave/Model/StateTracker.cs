using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class StateTracker
    {
        public string FilePath { get; set; }
        public List<StateEntry> States { get; private set; }
        private List<IStateObserver> observers;

        public StateTracker(string filePath ="state.json")
        {
            FilePath = filePath;
            States = new List<StateEntry>();
            observers = new List<IStateObserver>();
            LoadState();
        }

        //Observer Management Section

        public void AttachObserver(IStateObserver observer)
        {
            if (!observer.Contains(observer))
            {
                observer.Add(observer);
            }
        }
           
        public void DetachObserver(IStateObserver observer)
        {
            observer.Remove(observer);
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
            var existantState = States.FirstOrDefault(search => search.JobName == entry.JobName,null);
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
            
        }

        public void LoadState()
        {
            
        }
    }
}
