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
using update.Lib.Logging;

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FireWebHandler : CodeActivity
    {
        public static IServiceLocator ApplicationServices { get; set; }
        private LogFacility logFac = new LogFacility("FireWebHandler");

        public InArgument<string> xmlResponse { get; set; }
        public InArgument<List<Tuple<string, BusinessObject>>> targetCRUDs { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Log.Current.Debug(logFac, "Execution has been started!");

            var targetCRUDs = context.GetValue(this.targetCRUDs);
            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(context.GetValue(this.xmlResponse));

            var crudItems = xmlResponse.SelectNodes("/response/*[name()='update' or name()='import']").OfType<XmlNode>();

            // nothing happened -> return
            Log.Current.DebugFormat(logFac, "{0} CRUD items are announced to be affected", targetCRUDs.Count);
            Log.Current.DebugFormat(logFac, "{0} CRUD items has been found in the processed response.", crudItems.Count());
            if (crudItems.Count() < 1)
                return;

            using (Session.EnsureScope(Guid.NewGuid().ToString()))
            using (var suSession = ApplicationServices.Get<ICrmBaseApplication>().CreateSpecialUserSession())
            {
                var suServices = suSession.Services;
                var eventHub = suServices.Get<IEventHub>();

                for (int i = 0; i < crudItems.Count(); i++)
                {
                    Log.Current.DebugFormat(logFac, "trying to fire with CRUD element at {0}", i);

                    var crudNode = crudItems.ElementAt(i);
                    if (crudNode.FirstChild.Attributes["type"].Value == "error")
                        continue;

                    var currentCRUDOp = targetCRUDs.ElementAt(i);
                    if (
                        currentCRUDOp.Item1 != crudNode.Name // should never be the case, but integrity must be guaranteed
                        || currentCRUDOp.Item2 == null
                    ) 
                        continue;

                    BusinessObject currentBO = null; string resultOp = "update";
                    switch (crudNode.Name)
                    {
                        case "update":
                            // updates can affect more than one Business Object by a wider condition
                            foreach (var updateReturnItem in crudNode.SelectNodes("return").OfType<XmlNode>())
                            {
                                currentBO = new BusinessObject(currentCRUDOp.Item2);
                                currentBO.Uid = RecordUid.From(
                                    updateReturnItem.Attributes["table"].Value,
                                    RecordId.FromString(updateReturnItem.Attributes["id"].Value)
                                );
                            }
                            break;
                        case "import":
                            resultOp = crudNode.FirstChild.Attributes["type"].Value;
                            if (resultOp == "match") // if nothing has been updated or created
                                continue;

                            currentBO = new BusinessObject(currentCRUDOp.Item2);
                            currentBO.Uid = RecordUid.From(
                                crudNode.FirstChild.Attributes["table"].Value,
                                RecordId.FromString(crudNode.FirstChild.Attributes["id"].Value)
                            );

                            break;
                    }

                    Log.Current.Debug(logFac, "Checking if the BusinessObject for event is valid...");
                    if (currentBO == null || !currentBO.Uid.IsValid) continue;

                    Log.Current.DebugFormat(logFac, ">{0}< event will be fired on {1}", resultOp, currentBO.ToPrettyString());

                    // Fire event
                    switch (resultOp)
                    {
                        case "update":
                            eventHub.RaiseEvent(
                                string.Format("/InfoAreas/{0}/PostUpdate", currentBO.Uid.InfoAreaId),
                                this,
                                new UpdateEventArgs(suServices) { BusinessObject = currentBO }
                            );
                            break;
                        case "insert":
                            eventHub.RaiseEvent(
                                string.Format("/InfoAreas/{0}/PostNew", currentBO.Uid.InfoAreaId),
                                this,
                                new NewEventArgs(suServices) { BusinessObject = currentBO }
                            );
                            break;
                    }

                    Log.Current.Debug(logFac, "Successfully finished firing the event!");
                }
            }

            Log.Current.Debug(logFac, "Execution has finished!");
        }
    }
}
