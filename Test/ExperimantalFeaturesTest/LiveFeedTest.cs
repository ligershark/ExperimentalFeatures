using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExperimentalFeatures;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace ExperimantalFeaturesTest
{
    [TestClass]
    public class LiveFeedTest
    {
        [TestMethod]
        public async Task UpdateAsync()
        {
            string localPath = Path.Combine(Path.GetTempPath(), "update.json");
            var file = new FileInfo("..\\..\\artifacts\\feed.json");
            var feed = new LiveFeed(localPath);

            await feed.UpdateAsync(file.FullName);
            File.Delete(localPath);

            Assert.IsTrue(feed.Extensions.Count == 2);
            Assert.IsTrue(feed.Extensions[0].Name == "Add New File");
            Assert.IsTrue(feed.Extensions[0].Id == "2E78AA18-E864-4FBB-B8C8-6186FC865DB3");
            Assert.IsTrue(feed.Extensions[1].MaxVersion == new Version("16.0"));
        }

        [TestMethod]
        public async Task ParsingAsync()
        {
            string localPath = Path.Combine(Path.GetTempPath(), "feed.json");
            var feed = new LiveFeed(localPath);

            string content = @"{
            ""Add New File"": {
                ""id"": ""2E78AA18-E864-4FBB-B8C8-6186FC865DB3"",
                ""minVersion"": ""15.0""
                }
            }";

            File.WriteAllText(localPath, content);

            await feed.ParseAsync();
            File.Delete(localPath);

            Assert.IsTrue(feed.Extensions.Count == 1);
            Assert.IsTrue(feed.Extensions[0].Name == "Add New File");
            Assert.IsTrue(feed.Extensions[0].Id == "2E78AA18-E864-4FBB-B8C8-6186FC865DB3");
        }
    }
}
