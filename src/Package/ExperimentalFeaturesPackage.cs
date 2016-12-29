using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Tasks = System.Threading.Tasks;

namespace ExperimentalFeatures
{
    [Guid(PackageGuids.guidShowModalCommandPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimentalFeaturesPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            ShowModalCommand.Initialize(this, commandService);
        }
    }

    [Guid("ec98875a-b294-456a-98d5-7663e703ded2")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class InstallerPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Init installer
            var repository = await GetServiceAsync(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = await GetServiceAsync(typeof(SVsExtensionManager)) as IVsExtensionManager;

            var installer = new Installer.Installer(repository, manager);
        }
    }
}
