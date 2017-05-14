using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Xml;
using update.Crm.Xml;

namespace StrictLine.ACRMIf.FireWebHandler
{

    public sealed class FetchPotentialBOChange : CodeActivity
    {
        // Define an activity input argument of type string
        public InArgument<string> xmlRequestStr { get; set; }
        public InArgument<string> searchedBOXmlName { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            // Build up xml struct upon bpm input
            var xmlRequest = new XmlDocument();
            xmlRequest.LoadXml(context.GetValue(xmlRequestStr));

            var potentialUpds = xmlRequest.SelectNodes(string.Format("/request/update/fields/*{0}", "[" + context.GetValue(searchedBOXmlName) + "]"));
            var potentialImps = xmlRequest.SelectNodes(string.Format("/request/import/fields/*{0}", "[" + context.GetValue(searchedBOXmlName) + "]"));

            // TODO: Find out schema information - which fields are targetted
            // XmlNamesTools.GetXmlNames() 
        }
    }
}
