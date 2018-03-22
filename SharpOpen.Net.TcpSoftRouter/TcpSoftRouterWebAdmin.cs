using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using SharpOpen.Net.Observers.ServerObservers;
using System.Xml;
using System.Collections.Specialized;
using System.Web;

namespace SharpOpen.Net.TcpSoftRouter
{


    internal class TcpSoftRouterWebAdmin
    {
        internal static void HandleWebAdministrationRequest(IAsyncResult asyncResult)
        {
            HttpListener webAdministrationServer = asyncResult.AsyncState as HttpListener;

            HttpListenerContext webAdministrationRequestContext = webAdministrationServer.EndGetContext(asyncResult);

            // Handle next request.
            webAdministrationServer.BeginGetContext(HandleWebAdministrationRequest, webAdministrationServer);

            webAdministrationRequestContext.Response.AddHeader("Server", "SharpOpen.Net.TcpSoftRouter Administration Server");
            webAdministrationRequestContext.Response.ContentEncoding = Encoding.UTF8;
            webAdministrationRequestContext.Response.ContentType = "text/html";
            webAdministrationRequestContext.Response.AddHeader("Cache-Control", "no-cache");

            string request = "LIST";
            bool isPostback = webAdministrationRequestContext.Request.HttpMethod.ToUpperInvariant() == "POST";
            
            if (isPostback)
            {
                Stream body = webAdministrationRequestContext.Request.InputStream;
                Encoding encoding = webAdministrationRequestContext.Request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                NameValueCollection nameValuePairs = HttpUtility.ParseQueryString(reader.ReadToEnd(), encoding);
                request = nameValuePairs["postbackcommand"];
            }

            try
            {
                XmlDocument response = Processor.Execute(request.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                
                MemoryStream responseStream = new MemoryStream();
                TcpSoftRouter.htmlTransform.Transform(response, null, responseStream);

                byte[] rawHtmlContent = responseStream.ToArray();

                webAdministrationRequestContext.Response.StatusCode = 200;
                webAdministrationRequestContext.Response.StatusDescription = "OK";
                webAdministrationRequestContext.Response.OutputStream.Write(rawHtmlContent, 0, rawHtmlContent.Length);
                webAdministrationRequestContext.Response.ContentLength64 = rawHtmlContent.Length;
                webAdministrationRequestContext.Response.Close();
            }
            catch (Exception unexpectedException)
            {
                webAdministrationRequestContext.Response.StatusCode = 500;
                webAdministrationRequestContext.Response.StatusDescription = "Internal server error";
                webAdministrationRequestContext.Response.Close();
            }
        }
    }
}
