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

        public DataStore(string filePath)
        {
            _logFile = filePath;
            Initialize();
        }

        internal List<LogMessage> Log { get; private set; } = new List<LogMessage>();

        public void MarkInstalled(ExtensionEntry extension)
        {
            Log.Add(new LogMessage(extension, "Installed"));
        }

        public void MarkUninstalled(ExtensionEntry extension)
        {
            Log.Add(new LogMessage(extension, "Uninstalled"));
        }

        public bool HasBeenInstalled(string id)
        {
            return Log.Any(ext => ext.Id == id);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Log);
            File.WriteAllText(_logFile, json);
        }

        public bool Reset()
        {
            try
            {
                File.Delete(_logFile);
                Log.Clear();
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
                    Log = JsonConvert.DeserializeObject<List<LogMessage>>(File.ReadAllText(_logFile));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                File.Delete(_logFile);
            }
        }

        internal struct LogMessage
        {
            public LogMessage(ExtensionEntry entry, string action)
            {
                Id = entry.Id;
                Name = entry.Name;
                Action = action;
                Date = DateTime.Now;
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Action { get; set; }

            public override string ToString()
            {
                return $"{Date} {Action} {Name}";
            }
        }
    }
}
