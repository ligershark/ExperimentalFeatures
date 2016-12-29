using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace ExperimentalFeatures
{
    public class Installer
    {
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        public Installer(IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _repository = repository;
            _manager = manager;
        }

        public async Task InitializeAsync(IRegistryKey key)
        {
            string path = Constants.LiveFeedPath;

            using (key.CreateSubKey("ExperimentalWebFeatures"))
            {
                if (key.GetValue("path") == null)
                {
                    key.SetValue("path", Constants.LiveFeedPath);
                }
                else
                {
                    path = key.GetValue("path") as string;
                }
            }

            await Task.Delay(2);
        }

        public async Task ResetAsync()
        {
            await Task.Delay(2);
        }

        public async Task InstallAsync()
        {
            await Task.Delay(2);
        }
    }
}
