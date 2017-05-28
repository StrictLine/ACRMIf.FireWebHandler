using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using update.Lib.Contracts.Hosting;
using update.Lib.Contracts.Services;

namespace StrictLine.ACRMIf.FireWebHandler
{
    public sealed class PlugIn : IPlugIn
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Init(IServiceContainer services, XPathNavigator settingsNavigator)
        {
            FireWebHandler.Services = services;
            FetchPotentialBOChange.Services = services;
        }

        public void Load()
        {
            // nothing to load
        }
    }
}
