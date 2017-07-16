using System;
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
            FetchPotentialBOChange.ApplicationServices = services;
            FireWebHandler.ApplicationServices = services;
        }

        public void Load()
        {
            // nothing to load
        }
    }
}
