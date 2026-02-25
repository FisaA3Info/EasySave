using System;

namespace EasySave.Model
{
    public interface IStateObserver
    {
        void OnStateChanged(StateEntry entry);
    }
}