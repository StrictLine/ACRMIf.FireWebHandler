using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Xml;
using update.Crm.Contracts;
using update.Lib.ComponentModel;
using update.Lib.Contracts.Services;
using update.Lib.Extensions;
using update.Crm.Extensions;
using update.Lib.Contracts.Events;
using update.Crm.BusinessObjects;
using update.Crm.Contracts.Events;
using update.Crm.Contracts.BusinessObjects;

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FireWebHandler : CodeActivity
    {
        public static IServiceLocator Services { get; set; }

        public InArgument<string> xmlResponse { get; set; }
        public InArgument<List<IBusinessObject>> targetBusinessObjects { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(context.GetValue(this.xmlResponse));

            //@TODO paramaterize method in order to set custom XPath, respectively be able to process New Event
            var affectedItems = xmlResponse.SelectNodes("/response/update/return[@id]").Cast<XmlNode>()
                .Union(xmlResponse.SelectNodes("/response/import/return[@id and @type='update']").Cast<XmlNode>());

            if (affectedItems.Count(affItem => affItem.Attributes.GetNamedItem("id") != null) < 1) return;

            var affectedUIDs = affectedItems.Select(affectedItem => 
                RecordUid.From(affectedItem.Attributes["table"].Value, RecordId.FromString(affectedItem.Attributes["id"].Value))
            );

            using (Session.EnsureScope(Guid.NewGuid().ToString()))
            using (var suSession = Services.Get<ICrmBaseApplication>().CreateSpecialUserSession())
            {
                var suServices = suSession.Services;
                var eventHub = suServices.Get<IEventHub>();

                // we avoid foreach in this situation, as we need the current index
                for(int i = 0;i < affectedUIDs.Count();i++)
                {
                    var affectedUid = affectedUIDs.ElementAt(i);
                    //@TODO receive parameter: targetBusinessObject
                    // var affectedBOs = context.GetValue(targetBusinessObjects);
                    // affectedBOs.ElementAt(i).Uid = affectedUid
                    // affectedBO.Session = suSession
                    var affectedBO = new BusinessObject(suSession.Services, affectedUid);
                    affectedBO.Set(42, true); // first try for Excluding from Marketing Activities

                    eventHub.RaiseEvent<UpdateEventArgs>(
                        string.Format("/InfoAreas/{0}/PostUpdate", affectedUid.InfoAreaId),
                        this,
                        new UpdateEventArgs(suServices) { BusinessObject = affectedBO }
                    );

                }

            }

        }
    }
}
