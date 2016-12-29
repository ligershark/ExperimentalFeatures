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

        public List<ExtensionEntry> PreviouslyInstalledExtensions { get; private set; } = new List<ExtensionEntry>();

        public bool HasBeenInstalled(string id)
        {
            return PreviouslyInstalledExtensions.Any(ext => ext.Id == id);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(PreviouslyInstalledExtensions);
            File.WriteAllText(_logFile, json);
        }

        public bool Reset()
        {
            try
            {
                File.Delete(_logFile);
                PreviouslyInstalledExtensions.Clear();
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
                    PreviouslyInstalledExtensions = JsonConvert.DeserializeObject<List<ExtensionEntry>>(File.ReadAllText(_logFile));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }
    }
}
