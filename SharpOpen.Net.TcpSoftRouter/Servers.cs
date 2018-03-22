using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using SharpOpen.Net.Observers.ServerObservers;

namespace SharpOpen.Net.TcpSoftRouter
{
    internal class Servers : IEnumerable<Server>
    {
        private const string SERVER_CONFIGURATION_FILE_NAME = "SharpOpen.Net.TcpSoftRouter.Config.xml";
        private static SortedList<int, Server> _List = new SortedList<int, Server>();
        private static Servers _Instance;
        private static XmlDocument _Storage;

        internal static void LoadStorage()
        {
            _Storage = new XmlDocument();

            if (!File.Exists(SERVER_CONFIGURATION_FILE_NAME))
            {
                _Storage.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><Servers />");
                return;
            }

            try
            {
                _Storage.Load(SERVER_CONFIGURATION_FILE_NAME);
            }
            catch
            {
                _Storage.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><Servers />");
                _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
                return;
            }

            List<XmlNode> nodesToRemove = new List<XmlNode>();

            foreach (XmlNode serverNode in _Storage.DocumentElement.SelectNodes("Server"))
            {
                Server newServer = null;
                try
                {
                    newServer = Server.Create(Convert.ToInt32(serverNode.Attributes["LocalPort"].Value));
                }
                catch
                {
                    nodesToRemove.Add(serverNode);
                    continue;
                }

                try
                {
                    if (serverNode.Attributes["Type"] != null)
                    {
                        switch (serverNode.Attributes["Type"].Value)
                        {
                            case "SOCKS":
                                newServer.SetObserver(new SocksServerObserver());
                                break;
                            case "THROUGHSOCKS":
                                newServer.SetObserver
                                (
                                    new PortRedirectOverSocksServerObserver
                                    (
                                        serverNode.Attributes["SocksServerHost"].Value,
                                        Convert.ToInt32(serverNode.Attributes["SocksServerPort"].Value),
                                        serverNode.Attributes["RemoteHost"].Value,
                                        Convert.ToInt32(serverNode.Attributes["RemotePort"].Value)
                                    )
                                );
                                break;
                        }
                    }

                    if (newServer.Observer == null)
                    {
                        newServer.SetObserver
                        (
                            new TransmissionRedirectServerObserver
                            (
                                serverNode.Attributes["RemoteHost"].Value,
                                Convert.ToInt32(serverNode.Attributes["RemotePort"].Value)
                            )
                        );
                    }
                }
                catch
                {
                    nodesToRemove.Add(serverNode);
                    continue;
                }

                try
                {
                    if (serverNode.Attributes["Status"].Value == "1")
                    {
                        newServer.Start();
                    }
                }
                catch
                {
                    serverNode.Attributes["Status"].Value = "0";
                    _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
                }

                _List.Add(newServer.LocalPort, newServer);
            }

            if (nodesToRemove.Count > 0)
            {
                foreach (XmlNode serverNodeToRemove in nodesToRemove)
                {
                    _Storage.DocumentElement.RemoveChild(serverNodeToRemove);
                }
                _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
            }
        }

        internal static Servers Instance
        {
            get
            {
                if (_Instance == null) _Instance = new Servers();
                return _Instance;
            }
        }

        internal bool Contains(int localPort)
        {
            return _List.ContainsKey(localPort);
        }

        internal int Count { get { return _List.Count; } }

        internal Server this[int localPort]
        {
            get
            {
                return _List.ContainsKey(localPort) ? _List[localPort] : null;
            }
        }

        internal void Add(int localPort, Server server)
        {
            _List.Add(localPort, server);

            XmlNode serverNode = _Storage.DocumentElement.AppendChild(_Storage.CreateElement("Server"));

            XmlAttribute localPortAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("LocalPort"));
            localPortAttribute.Value = localPort.ToString();

            if (server.Observer is TransmissionRedirectServerObserver)
            {
                XmlAttribute remoteHostAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("RemoteHost"));
                remoteHostAttribute.Value = (server.Observer as TransmissionRedirectServerObserver).RedirectHost;

                XmlAttribute remotePortAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("RemotePort"));
                remotePortAttribute.Value = (server.Observer as TransmissionRedirectServerObserver).RedirectPort.ToString();
            }

            XmlAttribute statusAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("Status"));
            statusAttribute.Value = server.IsListening ? "1" : "0";

            XmlAttribute typeAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("Type"));

            SocksServerObserver socksObserver = server.Observer as SocksServerObserver;
            PortRedirectOverSocksServerObserver throughSocksObserver = server.Observer as PortRedirectOverSocksServerObserver;

            if (socksObserver != null)
            {
                typeAttribute.Value = "SOCKS";
            }
            else if (throughSocksObserver != null)
            {
                typeAttribute.Value = "THROUGHSOCKS";
                XmlAttribute socksHostAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("SocksServerHost"));
                XmlAttribute socksPortAttribute = serverNode.Attributes.Append(_Storage.CreateAttribute("SocksServerPort"));
                socksHostAttribute.Value = throughSocksObserver.SocksServerHost;
                socksPortAttribute.Value = throughSocksObserver.SocksServerPort.ToString();
            }
            else
            {
                typeAttribute.Value = "REDIRECT";
            }

            _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
        }

        internal void Remove(int localPort)
        {
            if (_List[localPort].IsListening)
            {
                _List[localPort].Stop();
            }
            _Storage.DocumentElement.RemoveChild(_Storage.DocumentElement.SelectSingleNode("Server[@LocalPort='" + localPort + "']"));
            _List.Remove(localPort);
            _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
        }

        internal void Stop(int localPort)
        {
            if (_List[localPort].IsListening)
            {
                _List[localPort].Stop();
                _Storage.DocumentElement.SelectSingleNode("Server[@LocalPort='" + localPort + "']").Attributes["Status"].Value = "0";
                _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
            }
        }

        internal void Start(int localPort)
        {
            if (!_List[localPort].IsListening)
            {
                _List[localPort].Start();
                _Storage.DocumentElement.SelectSingleNode("Server[@LocalPort='" + localPort + "']").Attributes["Status"].Value = "1";
                _Storage.Save(SERVER_CONFIGURATION_FILE_NAME);
            }
        }

        public IEnumerator<Server> GetEnumerator()
        {
            return _List.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
