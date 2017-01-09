using Microsoft.VisualStudio.ExtensionManager;
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
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        public Installer(IVsExtensionRepository repository, IVsExtensionManager manager, LiveFeed feed, DataStore store)
        {
            _repository = repository;
            _manager = manager;

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

        public async Task ResetAsync(Version vsVersion)
        {
            Store.Reset();
            await LiveFeed.UpdateAsync();
            await RunAsync(vsVersion, default(CancellationToken));
        }

        public async Task RunAsync(Version vsVersion, CancellationToken cancellationToken)
        {
            var toUninstall = GetExtensionsMarkedForDeletion(vsVersion);
            await UninstallAsync(toUninstall, cancellationToken);

            var toInstall = GetMissingExtensions().Except(toUninstall);
            await InstallAsync(toInstall, cancellationToken);
        }

        private async Task InstallAsync(IEnumerable<ExtensionEntry> extensions, CancellationToken token)
        {
            if (!extensions.Any())
                return;

#if DEBUG
            // Don't install while running in debug mode
            foreach (var ext in extensions)
            {
                await Task.Delay(2000);
                Store.MarkInstalled(ext);
            }
            Store.Save();
            return;
#endif

            await Task.Run(() =>
            {
                try
                {
                    foreach (var extension in extensions)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        InstallExtension(extension);
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

        private async Task UninstallAsync(IEnumerable<ExtensionEntry> extensions, CancellationToken token)
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
                            if (_manager.TryGetInstalledExtension(ext.Id, out installedExtension))
                            {
                                _manager.Uninstall(installedExtension);
                                Store.MarkUninstalled(ext);
                                Telemetry.UninstallSuccess(ext.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            Telemetry.UninstallFailure(ext.Id);
                            System.Diagnostics.Debug.Write(ex);
                        }
                    }
                }
                finally
                {
                    Store.Save();
                }
            });
        }

        private void InstallExtension(ExtensionEntry extension)
        {
            GalleryEntry entry = null;

            try
            {
                entry = _repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "ExtensionManagerUpdate")
                                                                                 .Where(e => e.VsixID == extension.Id)
                                                                                 .AsEnumerable()
                                                                                 .FirstOrDefault();

                if (entry != null)
                {
                    var installable = _repository.Download(entry);
                    _manager.Install(installable, false);
                    Telemetry.UninstallSuccess(extension.Id);
                }
            }
            catch (Exception ex)
            {
                Telemetry.InstallFailure(extension.Id);
                System.Diagnostics.Debug.Write(ex);
            }
            finally
            {
                if (entry != null)
                {
                    Store.MarkInstalled(extension);
                }
            }
        }

        private IEnumerable<ExtensionEntry> GetMissingExtensions()
        {
            var installed = _manager.GetInstalledExtensions();
            var notInstalled = LiveFeed.Extensions.Where(ext => !installed.Any(ins => ins.Header.Identifier == ext.Id));

            return notInstalled.Where(ext => !Store.HasBeenInstalled(ext.Id));
        }

        internal IEnumerable<ExtensionEntry> GetExtensionsMarkedForDeletion(Version VsVersion)
        {
            return LiveFeed.Extensions.Where(ext => ext.MinVersion > VsVersion || ext.MaxVersion < VsVersion);
        }
    }
}
