using System;

namespace OktetNET.OQmlTestAdapter.Helpers
{
    public interface ISolutionEventsListener
    {
        event EventHandler<SolutionEventsListenerEventArgs> SolutionProjectChanged;
        event EventHandler SolutionUnloaded;

        void StartListeningForChanges();
        void StopListeningForChanges();
    }
}