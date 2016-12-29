using Microsoft.VisualStudio.ExtensionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer
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


    }
}
