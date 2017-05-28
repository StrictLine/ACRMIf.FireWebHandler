using System;
using System.Linq;
using System.Activities;
using System.Xml;
using update.Lib.ComponentModel;
using update.Crm.Contracts;
using update.Lib.Contracts.Services;
using update.Lib.Extensions;
using update.Lib.Contracts.Events;
using update.Crm.Extensions;
using update.Crm.BusinessObjects;
using System.Collections.Generic;

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FetchPotentialBOChange : CodeActivity
    {
        public static IServiceLocator Services { get; set; }

        // Define an activity input argument of type string
        public InArgument<string> xmlRequestStr { get; set; }
        public InArgument<string> searchedBOXmlName { get; set; }

        public OutArgument<List<Tuple<string, BusinessObject>>> targetCRUDs { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            var targets = new List<Tuple<string, BusinessObject>>();

            // Build up xml struct upon bpm input
            var xmlRequest = new XmlDocument();
            xmlRequest.LoadXml(context.GetValue(xmlRequestStr));

            var potentialUpds = xmlRequest.SelectNodes(string.Format("/request/update/fields/*[{0}]", context.GetValue(searchedBOXmlName) ))
                .OfType<XmlNode>();
            var potentialImps = xmlRequest.SelectNodes(string.Format("/request/import/fields/*[{0}]", context.GetValue(searchedBOXmlName) ))
                .OfType<XmlNode>();

            // Generate Business Objects with input params
            using (Session.EnsureScope(Guid.NewGuid().ToString()))
            using (var suSession = Services.Get<ICrmBaseApplication>().CreateSpecialUserSession())
            {
                var suServices = suSession.Services;
                var eventHub = suServices.Get<IEventHub>();

                targets = RetrieveTargets("update", potentialUpds)
                    .Concat(RetrieveTargets("import", potentialImps))
                    .ToList();
            }

            context.SetValue(targetCRUDs, targets);
        }

        private List<Tuple<string, BusinessObject>> RetrieveTargets(string operationName, IEnumerable<XmlNode> xmlNodes)
        {
            var targetBusinessObjects = new List<Tuple<string, BusinessObject>>();

            foreach (XmlNode targetNode in xmlNodes)
            {
                string infoAreaName = targetNode.Name;

                try
                {
                    var infoAreaSrv = Services.GetInfoAreaService(infoAreaName);
                    var targetSchema = infoAreaSrv.Schema;
                    var targetBO = new BusinessObject(infoAreaSrv.InfoAreaId);

                    foreach (var field in targetNode.SelectNodes("*").OfType<XmlNode>())
                    {
                        int fieldId = -1;
                        targetSchema.TryGetFieldId(field.Name, out fieldId);

                        if (fieldId == -1) continue;

                        // TODO: consider extkey switch
                        targetBO.Set(fieldId, field.Value);
                    }

                    targetBusinessObjects.Add(new Tuple<string, BusinessObject>("update", targetBO));
                }
                catch (Exception e) { /* Potentially output e.Message and/or e.StackTrace for diagnostic purposes */ }


            }


            return targetBusinessObjects;
        }

    }
}
