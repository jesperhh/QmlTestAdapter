using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace OktetNET.OQmlTestAdapter.Helpers
{
    [Export(typeof(ITestFileAddRemoveListener))]
    public sealed class TestFileAddRemoveListener : IVsTrackProjectDocumentsEvents2, IDisposable, ITestFileAddRemoveListener
    {
        private readonly IVsTrackProjectDocuments2 projectDocTracker;
        private uint cookie = VSConstants.VSCOOKIE_NIL;

        public event EventHandler<TestFileChangedEventArgs> TestFileChanged;

        [ImportingConstructor]
        public TestFileAddRemoveListener([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");
            projectDocTracker = serviceProvider.GetService<IVsTrackProjectDocuments2>(typeof(SVsTrackProjectDocuments));
        }

        public void StartListeningForTestFileChanges()
        {
            if (projectDocTracker != null)
            {
                int hr = projectDocTracker.AdviseTrackProjectDocumentsEvents(this, out cookie);
                ErrorHandler.ThrowOnFailure(hr);
            }
        }

        public void StopListeningForTestFileChanges()
        {
            if (cookie != VSConstants.VSCOOKIE_NIL && projectDocTracker != null)
            {
                int hr = projectDocTracker.UnadviseTrackProjectDocumentsEvents(cookie);
                ErrorHandler.Succeeded(hr);

                cookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        private int OnNotifyTestFileAddRemove(
            int changedProjectCount,
            IVsProject[] changedProjects,
            string[] changedProjectItems,
            int[] rgFirstIndices,
            TestFileChangedReason reason)
        {
            int projItemIndex = 0;
            for (int changeProjIndex = 0; changeProjIndex < changedProjectCount; changeProjIndex++)
            {
                int endProjectIndex = ((changeProjIndex + 1) == changedProjectCount) ? 
                    changedProjectItems.Length :
                    rgFirstIndices[changeProjIndex + 1];

                for (; projItemIndex < endProjectIndex; projItemIndex++)
                {
                    if (changedProjects[changeProjIndex] != null && TestFileChanged != null)
                    {
                        TestFileChanged(this, new TestFileChangedEventArgs(
                            changedProjectItems[projItemIndex], reason));
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(int cProjects,
            int cFiles,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgpszMkDocuments,
            VSADDFILEFLAGS[] rgFlags)
        {
            return OnNotifyTestFileAddRemove(
                cProjects,
                rgpProjects, 
                rgpszMkDocuments, 
                rgFirstIndices, 
                TestFileChangedReason.Added);
        }

        public int OnAfterRemoveFiles(
            int cProjects,
           int cFiles,
           IVsProject[] rgpProjects,
           int[] rgFirstIndices,
           string[] rgpszMkDocuments,
           VSREMOVEFILEFLAGS[] rgFlags)
        {
            return OnNotifyTestFileAddRemove(
                cProjects, 
                rgpProjects, 
                rgpszMkDocuments, 
                rgFirstIndices, 
                TestFileChangedReason.Removed);
        }

        public int OnAfterRenameFiles(
            int cProjects,
            int cFiles,
            IVsProject[] rgpProjects,
            int[] rgFirstIndices,
            string[] rgszMkOldNames,
            string[] rgszMkNewNames,
            VSRENAMEFILEFLAGS[] rgFlags)
        {
            OnNotifyTestFileAddRemove(
                cProjects, 
                rgpProjects, 
                rgszMkOldNames, 
                rgFirstIndices, 
                TestFileChangedReason.Removed);

            return OnNotifyTestFileAddRemove(
                cProjects, 
                rgpProjects, 
                rgszMkNewNames, 
                rgFirstIndices, 
                TestFileChangedReason.Added);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopListeningForTestFileChanges();
            }
        }

        #region Unused events
        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects,
                                                            int cDirectories,
                                                            IVsProject[] rgpProjects,
                                                            int[] rgFirstIndices,
                                                            string[] rgpszMkDocuments,
                                                            VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects,
                                                                     int cDirectories,
                                                                     IVsProject[] rgpProjects,
                                                                     int[] rgFirstIndices,
                                                                     string[] rgpszMkDocuments,
                                                                     VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }


        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects,
                                                                     int cDirs,
                                                                     IVsProject[] rgpProjects,
                                                                     int[] rgFirstIndices,
                                                                     string[] rgszMkOldNames,
                                                                     string[] rgszMkNewNames,
                                                                     VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects,
                                                                    int cFiles,
                                                                    IVsProject[] rgpProjects,
                                                                    int[] rgFirstIndices,
                                                                    string[] rgpszMkDocuments,
                                                                    uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject,
                                                                  int cDirectories,
                                                                  string[] rgpszMkDocuments,
                                                                  VSQUERYADDDIRECTORYFLAGS[] rgFlags,
                                                                  VSQUERYADDDIRECTORYRESULTS[] pSummaryResult,
                                                                  VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject,
                                                            int cFiles,
                                                            string[] rgpszMkDocuments,
                                                            VSQUERYADDFILEFLAGS[] rgFlags,
                                                            VSQUERYADDFILERESULTS[] pSummaryResult,
                                                            VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject,
                                                                     int cDirectories,
                                                                     string[] rgpszMkDocuments,
                                                                     VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
                                                                     VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult,
                                                                     VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject,
                                                               int cFiles,
                                                               string[] rgpszMkDocuments,
                                                               VSQUERYREMOVEFILEFLAGS[] rgFlags,
                                                               VSQUERYREMOVEFILERESULTS[] pSummaryResult,
                                                               VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject,
                                                                     int cDirs,
                                                                     string[] rgszMkOldNames,
                                                                     string[] rgszMkNewNames,
                                                                     VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
                                                                     VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult,
                                                                     VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject,
                                                               int cFiles,
                                                               string[] rgszMkOldNames,
                                                               string[] rgszMkNewNames,
                                                               VSQUERYRENAMEFILEFLAGS[] rgFlags,
                                                               VSQUERYRENAMEFILERESULTS[] pSummaryResult,
                                                               VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }
        #endregion
    }
}