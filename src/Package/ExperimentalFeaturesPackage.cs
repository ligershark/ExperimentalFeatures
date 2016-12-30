using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
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
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimentalFeaturesPackage : AsyncPackage
    {
        public static Installer Installer { get; private set; }

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await RegisterCommandsAsync();

            Installer = await GetInstallerAsync();

            bool hasUpdates = await Installer.CheckForUpdatesAsync();

#if !DEBUG
            if (!hasUpdates)
                return;
#endif
            var vsVersion = GetVisualStudioVersion();
            await Installer.RunAsync(vsVersion, cancellationToken);
        }

        private async Tasks.Task RegisterCommandsAsync()
        {
            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                ShowModalCommand.Initialize(this, commandService);
            }
        }

        private async Tasks.Task<Installer> GetInstallerAsync()
        {
            var repository = await GetServiceAsync(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = await GetServiceAsync(typeof(SVsExtensionManager)) as IVsExtensionManager;

            var registry = new RegistryKeyWrapper(UserRegistryRoot);
            var store = new DataStore(registry, Constants.LogFile);
            var feed = new LiveFeed(registry, Constants.LiveFeedUrl, Constants.LiveFeedCachePath);

            return new Installer(repository, manager, feed, store);
        }

        public static Version GetVisualStudioVersion()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var v = process.MainModule.FileVersionInfo;

            return new Version(v.ProductMajorPart, v.ProductMinorPart, v.ProductBuildPart);
        }
    }
}
