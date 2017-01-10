﻿using Microsoft.VisualStudio.ExtensionManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalFeatures
{
    public class Installer
    {
        public Installer(LiveFeed feed, DataStore store)
        {
            LiveFeed = feed;
            Store = store;
        }

        public DataStore Store { get; }

        public LiveFeed LiveFeed { get; }

        public async Task<bool> CheckForUpdatesAsync()
        {
            var file = new FileInfo(LiveFeed.LocalCachePath);
            bool hasUpdates = false;

            if (!file.Exists || file.LastWriteTime < DateTime.Now.AddDays(-Constants.UpdateIntervalDays))
            {
                hasUpdates = await LiveFeed.UpdateAsync();
            }
            else
            {
                await LiveFeed.ParseAsync();
            }

            return hasUpdates;
        }

        public async Task ResetAsync(Version vsVersion, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            Store.Reset();
            await LiveFeed.UpdateAsync();
            await RunAsync(vsVersion, repository, manager, default(CancellationToken));
        }

        public async Task RunAsync(Version vsVersion, IVsExtensionRepository repository, IVsExtensionManager manager, CancellationToken cancellationToken)
        {
            var toUninstall = GetExtensionsMarkedForDeletion(vsVersion);
            await UninstallAsync(toUninstall, repository, manager, cancellationToken);

            var toInstall = GetMissingExtensions(manager).Except(toUninstall);
            await InstallAsync(toInstall, repository, manager, cancellationToken);
        }

        private async Task InstallAsync(IEnumerable<ExtensionEntry> extensions, IVsExtensionRepository repository, IVsExtensionManager manager, CancellationToken token)
        {
            if (!extensions.Any())
                return;

//#if DEBUG
//            // Don't install while running in debug mode
//            foreach (var ext in extensions)
//            {
//                await Task.Delay(2000);
//                Store.MarkInstalled(ext);
//            }
//            Store.Save();
//            return;
//#endif

            await Task.Run(() =>
            {
                try
                {
                    foreach (var extension in extensions)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        InstallExtension(extension, repository, manager);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
                finally
                {
                    Store.Save();
                }
            });
        }

        private async Task UninstallAsync(IEnumerable<ExtensionEntry> extensions, IVsExtensionRepository repository, IVsExtensionManager manager, CancellationToken token)
        {
            if (!extensions.Any())
                return;

            await Task.Run(() =>
            {
                try
                {
                    foreach (var ext in extensions)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        IInstalledExtension installedExtension;

                        try
                        {
                            if (manager.TryGetInstalledExtension(ext.Id, out installedExtension))
                            {
                                manager.Uninstall(installedExtension);
                                Store.MarkUninstalled(ext);
                                Telemetry.Uninstall(ext.Id, true);
                            }
                        }
                        catch (Exception)
                        {
                            Telemetry.Uninstall(ext.Id, false);
                        }
                    }
                }
                finally
                {
                    Store.Save();
                }
            });
        }

        private void InstallExtension(ExtensionEntry extension, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            GalleryEntry entry = null;

            try
            {
                entry = repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                                 .Where(e => e.VsixID == extension.Id)
                                                                                 .AsEnumerable()
                                                                                 .FirstOrDefault();

                if (entry != null)
                {
                    var installable = repository.Download(entry);
                    manager.Install(installable, false);
                    Telemetry.Install(extension.Id, true);
                }
            }
            catch (Exception)
            {
                Telemetry.Install(extension.Id, false);
            }
            finally
            {
                if (entry != null)
                {
                    Store.MarkInstalled(extension);
                }
            }
        }

        private IEnumerable<ExtensionEntry> GetMissingExtensions(IVsExtensionManager manager)
        {
            var installed = manager.GetInstalledExtensions();
            var notInstalled = LiveFeed.Extensions.Where(ext => !installed.Any(ins => ins.Header.Identifier == ext.Id));

            return notInstalled.Where(ext => !Store.HasBeenInstalled(ext.Id));
        }

        internal IEnumerable<ExtensionEntry> GetExtensionsMarkedForDeletion(Version VsVersion)
        {
            return LiveFeed.Extensions.Where(ext => ext.MinVersion > VsVersion || ext.MaxVersion < VsVersion);
        }
    }
}
