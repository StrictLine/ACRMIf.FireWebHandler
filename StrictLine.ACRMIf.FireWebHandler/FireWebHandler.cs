using System;
using System.Collections.Generic;
using System.Linq;
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

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FireWebHandler : CodeActivity
    {
        public static IServiceLocator Services { get; set; }

        public InArgument<string> xmlResponse { get; set; }
        public InArgument<List<Tuple<string, BusinessObject>>> targetCRUDs { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var targetCRUDs = context.GetValue(this.targetCRUDs);
            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(context.GetValue(this.xmlResponse));

            var crudItems = xmlResponse.SelectNodes("/response/*[update|import]").OfType<XmlNode>();

            // nothing happened -> return
            if (crudItems.Count() < 1) return;

            for(int i = 0;i < crudItems.Count();i++)
            {
                var crudNode = crudItems.ElementAt(i);
                if (crudNode.FirstChild.Attributes["type"].Value == "error")
                    continue;

                var resultBO = targetCRUDs.First().Item2;
                switch (crudNode.Name)
                {
                    case "update":
                        foreach (var updateReturnItem in crudNode.SelectNodes("return").OfType<XmlNode>())
                        {
                            var currentRecordUid = RecordUid.From(
                                updateReturnItem.Attributes["table"].Value, 
                                RecordId.FromString(updateReturnItem.Attributes["id"].Value)
                            );

                            BusinessObject currentBO = new BusinessObject(resultBO);
                            currentBO.Uid = currentRecordUid;
                            currentBO.Update(); // links won't work
                        }
                        break;
                    case "import":
                        var currentRecordUid = RecordUid.From(
                                crudNode.FirstChild.Attributes["table"].Value,
                                RecordId.FromString(crudNode.FirstChild.Attributes["id"].Value)
                        );
                        break;
                    default: continue;
                }

            }

            var affectedUIDs = crudItems.Select(affectedItem => 
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
