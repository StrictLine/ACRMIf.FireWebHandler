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
using update.Lib.Logging;

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FetchPotentialBOChange : CodeActivity
    {
        public static IServiceLocator ApplicationServices { get; set; }
        private IServiceLocator SUServices { get; set; }
        private LogFacility logFac = new LogFacility("FetchPotentialBOChange");

        // Define an activity input argument of type string
        public InArgument<string> xmlRequestStr { get; set; }

        public OutArgument<List<Tuple<string, BusinessObject>>> targetCRUDs { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            Log.Current.Debug(logFac, "Execution has been started!");

            var targets = new List<Tuple<string, BusinessObject>>();

            // Build up xml struct upon bpm input
            var xmlRequest = new XmlDocument();
            xmlRequest.LoadXml(context.GetValue(xmlRequestStr));

            var potentialUpds = xmlRequest.SelectNodes("/request/update/fields/*")
                .OfType<XmlNode>();
            var potentialImps = xmlRequest.SelectNodes("/request/import/fields/*")
                .OfType<XmlNode>();

            // Generate Business Objects with input params
            using (Session.EnsureScope(Guid.NewGuid().ToString()))
            using (var suSession = ApplicationServices.Get<ICrmBaseApplication>().CreateSpecialUserSession())
            {
                SUServices = suSession.Services;

                targets = RetrieveTargets("update", potentialUpds)
                .Concat(RetrieveTargets("import", potentialImps))
                .ToList();
            }

            Log.Current.DebugFormat(logFac, "{0} target business objects will be potentially affected!", targets.Count);

            context.SetValue(targetCRUDs, targets);

            Log.Current.Debug(logFac, "Execution has finished!");
        }

        private List<Tuple<string, BusinessObject>> RetrieveTargets(string operationName, IEnumerable<XmlNode> xmlNodes)
        {
            var targetBusinessObjects = new List<Tuple<string, BusinessObject>>();

            foreach (XmlNode targetNode in xmlNodes)
            {
                string infoAreaName = targetNode.Name;

                try
                {
                    var infoAreaSrv = SUServices.GetInfoAreaService(infoAreaName);
                    var targetSchema = infoAreaSrv.Schema;
                    var targetBO = new BusinessObject(SUServices, infoAreaSrv.Schema.InfoAreaId);

                    foreach (var field in targetNode.SelectNodes("*").OfType<XmlNode>())
                    {
                        int fieldId = -1;
                        targetSchema.TryGetFieldId(field.Name, out fieldId);

                        if (fieldId == -1) continue;

                        // TODO: consider extkey switch
                        targetBO.Set(fieldId, field.Value);
                    }

                    targetBusinessObjects.Add(new Tuple<string, BusinessObject>(operationName, targetBO));
                }
                catch (Exception e)
                {
                    // For the integrity of processed request and recorded operations
                    targetBusinessObjects.Add(new Tuple<string, BusinessObject>(operationName, null));

                    /* Potentially output e.Message and/or e.StackTrace for diagnostic purposes */
                }


            }

            return targetBusinessObjects;
        }

    }
}
