using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentalFeatures
{
    public interface IRegistryKey : IDisposable
    {
        IRegistryKey CreateSubKey(string subKey);
        void SetValue(string name, object value);
        object GetValue(string name);
    }
}
