﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using update.Crm.BusinessObjects;
using update.Crm.Contracts.Events;
using update.Crm.Extensions;
using update.Lib.Contracts.Events;
using update.Lib.Contracts.Hosting;
using update.Lib.Contracts.Services;
using update.Lib.Extensions;
using update.Lib.Logging;

namespace StrictLine.ACRMIf.SampleEventHandler
{
    public sealed class PlugIn : IPlugIn
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Init(IServiceContainer services, XPathNavigator settingsNavigator)
        {
            var startupLogging = new LogFacility("DEMO::EventHandler");
            Log.Current.Alert(startupLogging, "PlugIn INIT PART");

            services.Get<IEventHub>().StartObserving<UpdateEventArgs>("/InfoAreas/FI/PostUpdate", (sender, updArgs) => {
                Log.Current.AlertFormat(new LogFacility("DEMO::EventHandler CHECKPOINT---"), 
                    "My Handler is running with following changed fields: {0}", updArgs.BusinessObject.FieldIds.Join(','));

                // 15 = AreaCode
                if (updArgs.BusinessObject.FieldIds.Contains(15))
                {
                    var changedFI = new BusinessObject(updArgs.Services, updArgs.BusinessObject.Uid);
                    changedFI.Set(13, "MyCountry"); // 13 = Country
                    changedFI.Update();
                }

            });

        }

        public void Load()
        {
            // nothing to load
        }
    }
}
