using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Model
{
    public interface IStateObserver
    {
        void OnStateChanged(StateEntry entry);
    }
}