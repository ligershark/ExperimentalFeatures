using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ExperimentalFeatures
{
    public class LiveFeed
    {
        public LiveFeed(IRegistryKey key, string liveFeedUrl, string cachePath)
        {
            LocalCachePath = cachePath;

            EnsureRegistry(key, liveFeedUrl);
        }

        public string LocalCachePath { get; }
        public string LiveFeedUrl { get; private set; }

        public List<ExtensionEntry> Extensions { get; } = new List<ExtensionEntry>();

        public async Task<bool> UpdateAsync()
        {
            bool hasUpdates = await DownloadFileAsync();
            await ParseAsync();

            return hasUpdates;
        }

        internal async Task ParseAsync()
        {
            if (!File.Exists(LocalCachePath))
                return;

            using (var reader = new StreamReader(LocalCachePath))
            {
                string json = await reader.ReadToEndAsync();
                var root = JObject.Parse(json);

                foreach (var obj in root.Children<JProperty>())
                {
                    var child = obj.Children<JProperty>();

                    var entry = new ExtensionEntry()
                    {
                        Name = obj.Name,
                        Id = (string)root[obj.Name]["id"],
                        MinVersion = new Version((string)root[obj.Name]["minVersion"] ?? "15.0"),
                        MaxVersion = new Version((string)root[obj.Name]["maxVersion"] ?? "16.0")
                    };

                    Extensions.Add(entry);
                }
            }
        }

        private async Task<bool> DownloadFileAsync()
        {
            string oldContent = File.Exists(LocalCachePath) ? File.ReadAllText(LocalCachePath) : "";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LocalCachePath));

                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(LiveFeedUrl, LocalCachePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return false;
            }

            string newContent = File.ReadAllText(LocalCachePath);

            return oldContent != newContent;
        }

        private void EnsureRegistry(IRegistryKey key, string defaultUrl)
        {
            LiveFeedUrl = defaultUrl;

            using (key.CreateSubKey("ExperimentalWebFeatures"))
            {
                if (key.GetValue("path") == null)
                {
                    key.SetValue("path", defaultUrl);
                }
                else
                {
                    LiveFeedUrl = key.GetValue("path") as string;
                }
            }
        }
    }
}
