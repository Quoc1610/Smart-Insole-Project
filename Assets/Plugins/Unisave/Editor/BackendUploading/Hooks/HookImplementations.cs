using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unisave.Editor.BackendUploading.Hooks
{
    /// <summary>
    /// Contains implementations for individual hooks
    /// </summary>
    public static class HookImplementations
    {
        /// <summary>
        /// Called often, whenever the backend definition files structure
        /// might have been modified.
        /// </summary>
        public static void OnBackendFolderStructureChange()
        {
            var uploader = Uploader.Instance;

            if (!CanTriggerAutoUpload(uploader))
                return;
            
            // if there already is a running upload, cancel it
            // uploader.CancelRunningUpload();
            // NOTE: first, the server has to recover from failed mid-way
            // uploaded backends. Second, the cancellation has to be handled
            // more gracefully than being killed by an exception (TaskCancelledEx).
            
            uploader.UploadBackend(
                verbose: false,
                blockThread: false // can run asynchronously
            );
        }
        
        /// <summary>
        /// Called often, whenever code in the Unity project changes.
        /// It recalculates the backend hash and if it changes and automatic
        /// upload is enabled, it will perform the automatic upload.
        /// </summary>
        public static void OnAssemblyCompilationFinished()
        {
            var uploader = Uploader.Instance;

            if (!CanTriggerAutoUpload(uploader))
                return;
            
            uploader.UploadBackend(
                verbose: false,
                blockThread: true // MUST run synchronously
                // otherwise the upload starts but then unity swaps out the
                // compiled assembly and the upload never finishes
            );
        }

        private static bool CanTriggerAutoUpload(Uploader uploader)
        {
            // disabled automatic uploading -> do nothing
            if (!uploader.AutomaticUploadingEnabled)
                return false;
            
            // no cloud connection -> do nothing
            if (!uploader.IsCloudConnectionSetUp)
                return false;
            
            // recalculate hash and save it in the preferences file
            bool upload = uploader.RecalculateBackendHash();
            
            // if no upload needed, do nothing
            if (!upload)
                return false;

            return true;
        }

        /// <summary>
        /// Called before a build starts. It uploads the backend if automatic
        /// upload is enabled and then registers the build.
        /// 
        /// NOTE: Registration is done in preprocessing, because the
        /// postprocessing hook wasn't called on Peter's machine.
        /// </summary>
        /// <param name="report"></param>
        public static void OnPostprocessBuild(BuildReport report)
        {
            // Performs automatic backend upload if enabled
            PerformAutomaticUploadIfEnabled();

            // Checks that the backend hash matches the reality and
            // if so, registers the build. Else prints warning.
            TryToRegisterTheBuild(report);
        }

        /// <summary>
        /// Performs automatic backend upload if enabled
        /// </summary>
        private static void PerformAutomaticUploadIfEnabled()
        {
            var uploader = Uploader.Instance;
            
            if (uploader.AutomaticUploadingEnabled)
            {
                uploader.UploadBackend(
                    verbose: true, // here we ARE verbose, since we're building
                    blockThread: true
                );
            }
        }

        /// <summary>
        /// Checks that the backend hash matches the reality and
        /// if so, registers the build. Else prints warning.
        /// </summary>
        private static void TryToRegisterTheBuild(BuildReport report)
        {
            // check that the backendHash in preferences is up to date
            bool uploadNeeded = Uploader.Instance.RecalculateBackendHash();

            if (uploadNeeded)
            {
                Debug.LogWarning(
                    "[Unisave] This backend has not yet been uploaded, " +
                    "therefore build registration is being skipped. " +
                    "Enable automatic backend upload or upload the backend " +
                    "manually before you build your game to resolve this issue."
                );
                return;
            }
            
            // register the build
            BuildRegistrator
                .GetDefaultInstance()
                .RegisterBuild(report);
        }
    }
}