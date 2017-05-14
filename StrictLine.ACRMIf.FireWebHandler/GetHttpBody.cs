using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Web;

namespace StrictLine.ACRMIf.FireWebHandler
{
    public sealed class GetHttpBody : CodeActivity
    {
        public InArgument<HttpRequest> httpRequest { get; set; }
        public InArgument<HttpResponse> httpResponse { get; set; }
        public OutArgument<string> bodyContent { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            if (httpRequest == null && httpResponse == null) throw new ArgumentNullException("httpRequest/httpResponse", "You need to supply either httpRequest or httpResponse");
            var httpRequestStream = context.GetValue(httpRequest)?.InputStream;
            var httpResponseStream = context.GetValue(httpResponse)?.OutputStream;

            using (Stream transferStream = httpRequestStream ?? httpResponseStream)
            using (StreamReader streamReader = new StreamReader(transferStream, Encoding.UTF8))
                bodyContent.Set(context, streamReader.ReadToEnd());
        }

    }
}
