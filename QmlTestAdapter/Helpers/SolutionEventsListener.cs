﻿using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace OktetNET.OQmlTestAdapter.Helpers
{
	[Export(typeof(ISolutionEventsListener))]
    public class SolutionEventsListener : IVsSolutionEvents, ISolutionEventsListener
    {
        private readonly IVsSolution solution;
        private uint cookie = VSConstants.VSCOOKIE_NIL;

        public event EventHandler<SolutionEventsListenerEventArgs> SolutionProjectChanged;
        public event EventHandler SolutionUnloaded;

        [ImportingConstructor]
        public SolutionEventsListener([Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider)
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");
            this.solution = serviceProvider.GetService<IVsSolution>(typeof(SVsSolution));
        }

        public void StartListeningForChanges()
        {
            if (this.solution != null)
            {
                int hr = this.solution.AdviseSolutionEvents(this, out cookie);
                ErrorHandler.ThrowOnFailure(hr);
            }
        }

        public void StopListeningForChanges()
        {
            if (this.cookie != VSConstants.VSCOOKIE_NIL && this.solution != null)
            {
                int hr = this.solution.UnadviseSolutionEvents(cookie);
                ErrorHandler.Succeeded(hr);

                this.cookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        public void OnSolutionProjectUpdated(IVsProject project, SolutionChangedReason reason)
        {
            if (SolutionProjectChanged != null && project != null)
            {
                SolutionProjectChanged(this, new SolutionEventsListenerEventArgs(project, reason));
            }
        }

        public void OnSolutionUnloaded()
        {
            if(SolutionUnloaded != null)
            {
                SolutionUnloaded(this, new System.EventArgs());
            }
        }
        private bool IsSolutionFullyLoaded()
        {
            object var;

            ErrorHandler.ThrowOnFailure(this.solution.GetProperty((int)__VSPROPID4.VSPROPID_IsSolutionFullyLoaded, out var));
            return (bool)var;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            if (IsSolutionFullyLoaded())
            {
                var project = pHierarchy as IVsProject;
                OnSolutionProjectUpdated(project, SolutionChangedReason.Load);
            }
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            var project = pRealHierarchy as IVsProject;
            OnSolutionProjectUpdated(project, SolutionChangedReason.Unload);
            return VSConstants.S_OK;
        }

	    public int OnAfterCloseSolution(object pUnkReserved)
	    {
	        OnSolutionUnloaded();
            return VSConstants.S_OK;
        }

        #region unused events
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }

}