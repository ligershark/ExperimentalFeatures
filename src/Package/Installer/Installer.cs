using Microsoft.VisualStudio.ExtensionManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExperimentalFeatures
{
    public class Installer
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;
        private LiveFeed _liveFeed;
        private DataStore _store;
        private string _remoteUrl;

        public Installer(IVsExtensionRepository repository, IVsExtensionManager manager, LiveFeed feed, DataStore store)
        {
            _repository = repository;
            _manager = manager;
            _liveFeed = feed;
            _store = store;
        }

        public async Task<bool> CheckForUpdatesAsync(IRegistryKey key)
        {
            EnsureRegistry(key);

            var file = new FileInfo(_liveFeed.LocalCachePath);
            bool hasUpdates = false;

            if (!file.Exists || file.LastWriteTime < DateTime.Now.AddDays(Constants.UpdateInterval))
            {
                hasUpdates = await _liveFeed.UpdateAsync(_remoteUrl);
            }

            return hasUpdates;
        }

        public async Task ResetAsync()
        {
            _store.Reset();
            await _liveFeed.UpdateAsync(_remoteUrl);
            await InstallAsync();
        }

        public async Task InstallAsync()
        {
            var missingExtensions = GetMissingExtensions();

            await Task.Run(() =>
            {
                try
                {
                    foreach (var extension in missingExtensions)
                    {
                        InstallExtension(extension);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
                finally
                {
                    _store.Save();
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
                    _store.PreviouslyInstalledExtensions.Add(extension);
                }
            }
        }

        private IEnumerable<ExtensionEntry> GetMissingExtensions()
        {
            var installed = _manager.GetInstalledExtensions();
            var notInstalled = _liveFeed.Extensions.Where(ext => !installed.Any(ins => ins.Header.Identifier == ext.Id));

            return notInstalled.Where(ext => !_store.HasBeenInstalled(ext.Id));
        }

        private void EnsureRegistry(IRegistryKey key)
        {
            _remoteUrl = Constants.LiveFeedPath;

            using (key.CreateSubKey("ExperimentalWebFeatures"))
            {
                if (key.GetValue("path") == null)
                {
                    key.SetValue("path", Constants.LiveFeedPath);
                }
                else
                {
                    _remoteUrl = key.GetValue("path") as string;
                }
            }
        }
    }
}
