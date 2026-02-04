using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    internal class StateTracker
    {
        public string FilePath { get; set; }
        public List<StateEntry> States { get; set; }

        public void UpdateState(StateEntry entry)
        {
            
        }

        public void SaveState()
        {
            
        }

        public void LoadState()
        {
            
        }
    }
}
