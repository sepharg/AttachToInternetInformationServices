using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace AttachToInternetInformationServices
{
    public class IisProcesses
    {
        public IEnumerable<WorkerProcess> CurrentProcesses { get; private set; }
        public string[] CurrentProcessesName => CurrentProcesses.Select(p => p.AppPoolName).ToArray();

        public IisProcesses()
        {
            Refresh();
        }

        public void Refresh()
        {
            CurrentProcesses = GetCurrent();
        }

        private IEnumerable<WorkerProcess> GetCurrent()
        {
            var solutionName = System.IO.Path.GetFileNameWithoutExtension(IisAttachPackage.DTE.Solution?.FullName ?? "");
            using (var sm = Microsoft.Web.Administration.ServerManager.OpenRemote("localhost"))
            {
                return sm.WorkerProcesses
                    .Select(p => new WorkerProcess
                    {
                        AppPoolName = p.AppPoolName,
                        ProcessId = p.ProcessId
                    })
                    .OrderBy(p =>
                    {
                        var index = p.AppPoolName.IndexOf(solutionName, StringComparison.OrdinalIgnoreCase);
                        return index == -1 ? 999 : index;
                    })
                    .ThenBy(p => p.AppPoolName);
            }
        }

        public bool AttachAll()
        {
            var operationResult = true;
            foreach (var currentProcess in CurrentProcessesName)
            {
                operationResult &= this.AttachTo(currentProcess);
            }
            return operationResult;
        }

        public bool AttachTo(string appName)
        {
            return AttachTo(appName, true);
        }

        private bool AttachTo(string appName, bool firstTime)
        {
            var currentProcess = CurrentProcesses.FirstOrDefault(p => p.AppPoolName == appName);
            if (currentProcess == null)
            {
                Refresh();
                return false;
            }

            var process = IisAttachPackage.DTE.Debugger.LocalProcesses
                .Cast<Process>()
                .FirstOrDefault(p => p.ProcessID == currentProcess.ProcessId);

            if (process == null && firstTime)
            {
                Refresh();
                return AttachTo(appName, false);
            }

            if (process == null) return false;

            IisAttachPackage.DTE.StatusBar.Text = $"Attaching to {currentProcess.AppPoolName}. PID: {currentProcess.ProcessId}";
            process.Attach();
            return true;
        }
    }
}