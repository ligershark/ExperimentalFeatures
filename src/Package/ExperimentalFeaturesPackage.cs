using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
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
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public sealed class InstallerPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Installer installer = await GetInstallerAsync();

            bool hasUpdates = await installer.CheckForUpdatesAsync();

            //if (!hasUpdates)
            //    return;

            var missingExtensions = installer.GetMissingExtensions();

            if (missingExtensions.Any())
            {
                var statusBar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
                statusBar.SetText("Installing Experimental Web Features...");

                await installer.InstallAsync(missingExtensions, cancellationToken);

                statusBar.SetText("Experimental Web Features installed");
            }
        }

        private async Tasks.Task<Installer> GetInstallerAsync()
        {
            var repository = await GetServiceAsync(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = await GetServiceAsync(typeof(SVsExtensionManager)) as IVsExtensionManager;

            var registry = new RegistryKeyWrapper(UserRegistryRoot);
            var store = new DataStore(Constants.LogFile);
            var feed = new LiveFeed(registry, Constants.LiveFeedUrl, Constants.LiveFeedCachePath);

            return new Installer(repository, manager, feed, store);
        }
    }
}
