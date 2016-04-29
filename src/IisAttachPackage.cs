using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace AttachToInternetInformationServices
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.GuidiisAttachPkgString)]
    public sealed class IisAttachPackage : Package
    {
        private static DTE2 _dte;
        internal static DTE2 DTE => _dte ?? (_dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2);

        private static SolutionEvents _events;
        protected override async void Initialize()
        {
            base.Initialize();

            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            var attachMenu = new AttachMenu(mcs);
            await System.Threading.Tasks.Task.Run(() => attachMenu.Initialize());

            _events = DTE.Events.SolutionEvents;
            _events.Opened += () => attachMenu.Refesh();
        }
    }
}