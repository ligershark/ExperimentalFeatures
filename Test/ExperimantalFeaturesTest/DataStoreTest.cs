using ExperimentalFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExperimentalFeatures.DataStore;

namespace ExperimantalFeaturesTest
{
    [TestClass]
    public class DataStoreTest
    {
        private string _logFile;

        [TestInitialize]
        public void Setup()
        {
            _logFile = Path.Combine(Path.GetTempPath(), "logfile.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_logFile);
        }

        [TestMethod]
        public void ExtensionInstalledNoLogFile()
        {
            var entry = new ExtensionEntry();
            entry.Id = "id";

            var store = new DataStore(_logFile);
            store.MarkInstalled(entry);

            Assert.AreEqual(1, store._installedExtensions.Count);
            Assert.IsFalse(File.Exists(_logFile));
            Assert.AreEqual(entry.Id, store._installedExtensions[0].Id);
            Assert.AreEqual("Installed", store._installedExtensions[0].Action);

            store.Save();
            Assert.IsTrue(File.Exists(_logFile));
        }

        [TestMethod]
        public void ExtensionUninstalledNoLogFile()
        {
            var entry = new ExtensionEntry();
            entry.Id = "id";

            var store = new DataStore(_logFile);
            store.MarkUninstalled(entry);

            Assert.AreEqual(1, store._installedExtensions.Count);
            Assert.AreEqual(entry.Id, store._installedExtensions[0].Id);
            Assert.AreEqual("Uninstalled", store._installedExtensions[0].Action);
        }

        [TestMethod]
        public void LogFileExist()
        {
            var entry = new ExtensionEntry();
            entry.Id = "id";

            var msg = new[] { new LogMessage(entry.Id, "Installed") };

            var json = JsonConvert.SerializeObject(msg);
            File.WriteAllText(_logFile, json);

            var store = new DataStore(_logFile);

            Assert.AreEqual(1, store._installedExtensions.Count);
        }
    }
}
