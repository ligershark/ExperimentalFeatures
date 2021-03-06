﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Tasks = System.Threading.Tasks;

namespace ExperimentalFeatures
{

    [Guid(PackageGuids.guidShowModalCommandPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimantalFeaturesPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await ShowModalCommand.InitializeAsync(this);

            // Load installer package
            var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            var guid = new Guid(InstallerPackage._packageGuid);
            IVsPackage ppPackage;
            ErrorHandler.ThrowOnFailure(shell.LoadPackage(guid, out ppPackage));
        }
    }

    [Guid(_packageGuid)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class InstallerPackage : AsyncPackage
    {
        public static DateTime _installTime = DateTime.MinValue;
        public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942d7";

        public static Installer Installer
        {
            get;
            private set;
        }

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var registry = new RegistryKeyWrapper(UserRegistryRoot);
            var store = new DataStore(registry, Constants.LogFile);
            var feed = new LiveFeed(registry, Constants.LiveFeedUrl, Constants.LiveFeedCachePath);

            Installer = new Installer(feed, store);

            bool hasUpdates = await Installer.CheckForUpdatesAsync();

            if (!hasUpdates)
                return;

            // SVsExtensionRepository cannot be loaded in the background duing package load
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var repository = await GetServiceAsync(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
            var manager = await GetServiceAsync(typeof(SVsExtensionManager)) as IVsExtensionManager;
            var vsVersion = GetVisualStudioVersion();

            await Installer.RunAsync(vsVersion, repository, manager, cancellationToken);

            _installTime = DateTime.Now;
        }

        public static Version GetVisualStudioVersion()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var v = process.MainModule.FileVersionInfo;

            return new Version(v.ProductMajorPart, v.ProductMinorPart, v.ProductBuildPart);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _installTime != DateTime.MinValue)
            {
                var minutes = (DateTime.Now - _installTime).Minutes;
                Telemetry.RecordTimeToClose(minutes);
            }

            base.Dispose(disposing);
        }
    }
}
