using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace AttachToInternetInformationServices
{
    public class AttachMenu
    {
        private readonly OleMenuCommandService _mcs;
        private readonly IisProcesses _processes;

        private string _currentDropDownComboChoice;
        public string CurrentDropDownComboChoice
        {
            get { return _currentDropDownComboChoice ?? "None"; }
            set
            {
                if (value != null)
                    _currentDropDownComboChoice = value;
            }
        }

        public AttachMenu(OleMenuCommandService mcs)
        {
            _mcs = mcs;
            _processes = new IisProcesses();
        }

        public void Initialize()
        {
            // Combo Box
            var menuMyDropDownComboCommandId = new CommandID(Guids.GuidiisAttachCmdSet, (int)PkgCmdIDs.CmdidMyDropDownCombo);
            var menuMyDropDownComboCommand = new OleMenuCommand(OnMenuMyDropDownCombo, menuMyDropDownComboCommandId);
            _mcs.AddCommand(menuMyDropDownComboCommand);

            // Get list
            var menuMyDropDownComboGetListCommandId = new CommandID(Guids.GuidiisAttachCmdSet, (int)PkgCmdIDs.CmdidMyDropDownComboGetList);
            var menuMyDropDownComboGetListCommand = new OleMenuCommand(OnMenuMyDropDownComboGetList, menuMyDropDownComboGetListCommandId);
            _mcs.AddCommand(menuMyDropDownComboGetListCommand);

            // Refresh button
            var refreshProcessesCommandId = new CommandID(Guids.GuidiisAttachCmdSet, (int)PkgCmdIDs.CmdidProcessRefresh);
            var refreshProcessesCommand = new OleMenuCommand(OnRefreshProcesses, refreshProcessesCommandId);
            _mcs.AddCommand(refreshProcessesCommand);

            // Attach button
            var attachProcessCommandId = new CommandID(Guids.GuidiisAttachCmdSet, (int)PkgCmdIDs.CmdidAttachProcess);
            var attachProcessCommand = new OleMenuCommand(delegate { AttachToProcess(); }, attachProcessCommandId);
            _mcs.AddCommand(attachProcessCommand);

            // Attach all button
            var attachAllCommandId = new CommandID(Guids.GuidiisAttachCmdSet, (int)PkgCmdIDs.CmdidAttachAllProcess);
            var attachAllCommand = new OleMenuCommand(delegate { AttachToAllW3wpProcesses(); }, attachAllCommandId);
            _mcs.AddCommand(attachAllCommand);

            menuMyDropDownComboGetListCommand.BeforeQueryStatus += IsEnabled;
            refreshProcessesCommand.BeforeQueryStatus += IsEnabled;
            attachProcessCommand.BeforeQueryStatus += IsEnabled;
            attachAllCommand.BeforeQueryStatus += IsEnabled;

            /* Keyboard binding */

            // Ctrl+alt+p to auto attach
            var attachToProcessCommand = IisAttachPackage.DTE.Commands.Item("Tools.AttachToInternetInformationServices");
            attachToProcessCommand.Bindings = new object[] { "Global::Ctrl+Alt+P", "Text Editor::Ctrl+Alt+P" };
        }

        private void IsEnabled(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
                command.Enabled = IisAttachPackage.DTE.Debugger.DebuggedProcesses?.Count == 0;
        }

        private void OnMenuMyDropDownCombo(object sender, EventArgs e)
        {
            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                string newChoice = eventArgs.InValue as string;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero)
                {
                    if (_currentDropDownComboChoice == null)
                        RefreshCurrentSelection();

                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    Marshal.GetNativeVariantForObject(CurrentDropDownComboChoice, vOut);
                }
               
                else if (newChoice != null)
                {
                    // new value was selected or typed in
                    // see if it is one of our items
                    CurrentDropDownComboChoice = _processes.CurrentProcessesName.FirstOrDefault(n => n.Equals(newChoice, StringComparison.OrdinalIgnoreCase));
                    AttachToProcess();
                }
            }
            else
            {
                // We should never get here; EventArgs are required.
                throw new ArgumentException("EventArgs Required"); // force an exception to be thrown
            }
        }

        private void RefreshCurrentSelection()
        {
            CurrentDropDownComboChoice = _processes.CurrentProcessesName.FirstOrDefault();
        }

        private void OnMenuMyDropDownComboGetList(object sender, EventArgs e)
        {
            var eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (inParam != null)
                {
                    throw new ArgumentException("InParam Illegal"); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    Marshal.GetNativeVariantForObject(_processes.CurrentProcessesName, vOut);
                }
                else
                {
                    throw new ArgumentException("OutParam Required"); // force an exception to be thrown
                }
            }
        }

        private void AttachToAllW3wpProcesses()
        {
            var attached = _processes.AttachAll();
            if (!attached)
            {
                IisAttachPackage.DTE.StatusBar.Text = $"Failed to attach to one or more w3wp procceses.";
            }
        }

        private void AttachToProcess()
        {
            if (_currentDropDownComboChoice == null)
            {
                IisAttachPackage.DTE.StatusBar.Text = "Cannot attach to IIS process. No project selected.";
                return;
            }

            IisAttachPackage.DTE.StatusBar.Text = $"Attaching to {CurrentDropDownComboChoice}";

            var attached = _processes.AttachTo(CurrentDropDownComboChoice);
            if (!attached)
            {
                IisAttachPackage.DTE.StatusBar.Text = $"Failed to attach to {CurrentDropDownComboChoice}";
                return;
            }

            IisAttachPackage.DTE.StatusBar.Text = $"Debugging: {CurrentDropDownComboChoice}";
        }

        private void OnRefreshProcesses(object sender, EventArgs e)
        {
            Refesh();
        }

        public void Refesh()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                _processes.Refresh();
                RefreshCurrentSelection();
            });
        }
    }
}
