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
        private LiveFeed _liveFeed;

        public Installer(IVsExtensionRepository repository, IVsExtensionManager manager, LiveFeed feed, DataStore store)
        {
            _repository = repository;
            _manager = manager;
            _liveFeed = feed;

            Store = store;
        }

        public DataStore Store { get; }

        public async Task<bool> CheckForUpdatesAsync()
        {
            var file = new FileInfo(_liveFeed.LocalCachePath);
            bool hasUpdates = false;

            if (!file.Exists || file.LastWriteTime < DateTime.Now.AddDays(-Constants.UpdateIntervalDays))
            {
                hasUpdates = await _liveFeed.UpdateAsync();
            }
            else
            {
                await _liveFeed.ParseAsync();
            }

            return hasUpdates;
        }

        public async Task ResetAsync()
        {
            Store.Reset();
            await _liveFeed.UpdateAsync();
            await InstallAsync(GetMissingExtensions());
        }

        public async Task InstallAsync(IEnumerable<ExtensionEntry> missingExtensions, CancellationToken token = default(CancellationToken))
        {
            if (!missingExtensions.Any())
                return;

#if DEBUG
            // Don't install while running in debug mode
            foreach (var ext in missingExtensions)
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
                    foreach (var extension in missingExtensions)
                    {
                        if (token != null && token.IsCancellationRequested)
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
                }
            }
            catch (Exception ex)
            {
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

        public IEnumerable<ExtensionEntry> GetMissingExtensions()
        {
            var installed = _manager.GetInstalledExtensions();
            var notInstalled = _liveFeed.Extensions.Where(ext => !installed.Any(ins => ins.Header.Identifier == ext.Id));

            return notInstalled.Where(ext => !Store.HasBeenInstalled(ext.Id));
        }
    }
}
