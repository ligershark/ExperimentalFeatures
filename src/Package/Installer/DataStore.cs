using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExperimentalFeatures
{
    public class DataStore
    {
        private static string _logFile;
        internal List<LogMessage> _installedExtensions = new List<LogMessage>();

        public DataStore(string filePath)
        {
            _logFile = filePath;
            Initialize();
        }

        public void MarkInstalled(ExtensionEntry extension)
        {
            _installedExtensions.Add(new LogMessage(extension.Id, "Installed"));
        }

        public void MarkUninstalled(ExtensionEntry extension)
        {
            _installedExtensions.Add(new LogMessage(extension.Id, "Uninstalled"));
        }

        public bool HasBeenInstalled(string id)
        {
            return _installedExtensions.Any(ext => ext.Id == id);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(_installedExtensions);
            File.WriteAllText(_logFile, json);
        }

        public bool Reset()
        {
            try
            {
                File.Delete(_logFile);
                _installedExtensions.Clear();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return false;
            }
        }

        private void Initialize()
        {
            try
            {
                if (File.Exists(_logFile))
                {
                    _installedExtensions = JsonConvert.DeserializeObject<List<LogMessage>>(File.ReadAllText(_logFile));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                File.Delete(_logFile);
            }
        }

        public struct LogMessage
        {
            public LogMessage(string id, string action)
            {
                Id = id;
                Action = action;
                Date = DateTime.Now;
            }

            public string Id { get; set; }
            public DateTime Date { get; set; }
            public string Action { get; set; }
        }
    }
}
