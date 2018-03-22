using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SharpOpen.Net
{
    public static class HttpProxy
    {
        private static WebProxy _HttpProxy;

        public static bool Exists { get { return _HttpProxy != null; } }

        public static WebProxy InnerProxy { get { return _HttpProxy; } }

        public static void SetProxy(string proxyHost, int proxyPort)
        {
            _HttpProxy = new WebProxy(proxyHost, proxyPort);
        }

        public static void SetProxy(string proxyHost, int proxyPort, string domainName, string userName, string password)
        {
            SetProxy(proxyHost, proxyPort);
            SetCredentials(domainName, userName, password);
        }
        
        public static void SetCredentials(string domainName, string userName, string password)
        {
            if (!Exists) return;
            _HttpProxy.Credentials = new NetworkCredential(userName, password, domainName);
        }

        public static WebProxy GetProxy()
        {
            return _HttpProxy;
        }

        public static void RemoveProxy()
        {
            _HttpProxy = null;
        }

        public static WebProxy GenerateLocalProxy(int availableLocalPort)
        {
            WebProxy proxy = new WebProxy("127.0.0.1", availableLocalPort);
            if(_HttpProxy != null) proxy.Credentials = _HttpProxy.Credentials;
            return proxy;
        }

    }
}
