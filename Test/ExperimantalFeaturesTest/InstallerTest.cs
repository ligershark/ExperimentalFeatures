using ExperimentalFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace ExperimantalFeaturesTest
{
    /// <summary>
    /// Summary description for InstallerTest
    /// </summary>
    [TestClass, ]
    public class InstallerTest
    {
        private Installer _installer;
        private string _cachePath;

        [TestInitialize]
        public void TestSetup()
        {
            _cachePath = Path.Combine(Path.GetTempPath(), "cache.json");
            var registry = new StaticRegistryKey();
            var store = new DataStore(Constants.LogFile);
            var feed = new LiveFeed(registry, Constants.LiveFeedUrl, _cachePath);

            _installer = new Installer(null, null, feed, store);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(_cachePath);
        }

        [TestMethod]
        public async Task CheckForUpdatesNoCacheAsync()
        {
            File.Delete(_cachePath);
            var hasUpdates = await _installer.CheckForUpdatesAsync();

            Assert.IsTrue(hasUpdates);
        }

        [TestMethod]
        public async Task CheckForUpdatesValidCacheAsync()
        {
            File.WriteAllText(_cachePath, "{}");
            var hasUpdates = await _installer.CheckForUpdatesAsync();

            Assert.IsFalse(hasUpdates);
        }
    }
}
