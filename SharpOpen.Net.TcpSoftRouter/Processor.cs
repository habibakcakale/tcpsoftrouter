using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SharpOpen.Net.Observers.ServerObservers;
using System.Net;
using System.Net.Sockets;
using SharpOpen.Net.Socks;

namespace SharpOpen.Net.TcpSoftRouter
{
    internal static class Processor
    {
        private static XmlElement Add(XmlNode source, string nodeName)
        {
            XmlDocument document = source is XmlDocument ? source as XmlDocument : source.OwnerDocument;
            return source.AppendChild(document.CreateElement(nodeName)) as XmlElement;
        }

        private static XmlAttribute Add(XmlElement source, string attributeName, object attributeValue)
        {
            XmlAttribute newAttribute = source.Attributes.Append(source.OwnerDocument.CreateAttribute(attributeName));
            if (attributeValue != null) newAttribute.Value = Convert.ToString(attributeValue);
            return newAttribute;
        }

        private static int TestPort(string portString, ref int statuscode, ref string statusmessage)
        {
            int port;
            if (!int.TryParse(portString, out port))
            {
                statuscode = 500;
                statusmessage = "Syntax Error: Port " + portString + " is not a valid integer";
                return 0;
            }
            if (port > System.Net.IPEndPoint.MaxPort || port < System.Net.IPEndPoint.MinPort)
            {
                statuscode = 501;
                statusmessage = "Overflow: Port " + port + " is out of range of valid port range";
                return 0;
            }
            return port;
        }

        private static bool TestLocalPort(string portString, out int localPort, ref int statuscode, ref string statusmessage)
        {
            localPort = TestPort(portString, ref statuscode, ref statusmessage);
            if (localPort == 0) return false;
            if (Servers.Instance.Contains(localPort))
            {
                statuscode = 502;
                statusmessage = "The local port " + portString + " is already in use";
                return false;
            }
            return true;
        }

        private static void TestRemoteServer(string remoteHost, int remotePort, ref int statuscode, ref string statusmessage)
        {
            try
            {
                Connection testConnection = Connection.Connect(remoteHost, remotePort);
                testConnection.Close();
            }
            catch (Exception exp)
            {
                statuscode = 504;
                statusmessage = "Failed to connect to the remote host: " + exp.Message;
            }
        }

        private static void TestRemoteServer(string remoteHost, int remotePort, string remoteSocksHost, int remoteSocksPort, ref int statuscode, ref string statusmessage)
        {
            Connection testConnection;
            try
            {
                testConnection = Connection.Connect(remoteSocksHost, remoteSocksPort);
            }
            catch (Exception exp)
            {
                statuscode = 505;
                statusmessage = "Could not connect to the Socks Server: " + exp.Message;
                return;
            }

            try
            {
                testConnection.Send(Socks4Request.Build(Socks4Request.REQUEST_TYPE_CONNECT, remoteHost, remotePort, "").ToByteArray());
                testConnection.BeginReceivingSync();

                byte[] socksRawResponse = testConnection.GetNextSyncReceiveBuffer();
                testConnection.Close();

                Socks4Response socksResponse = Socks4Response.Parse(socksRawResponse);

                if (socksResponse == null || !socksResponse.Success)
                {
                    statuscode = 504;
                    statusmessage = "Socks server reports that it failed to connect to the remote host";
                }
            }
            catch (Exception exp)
            {
                statuscode = 504;
                statusmessage = "Failed while trying to connect to the remote host through socks: " + exp.Message;
            }

        }

        internal static XmlDocument Execute(string[] request)
        {
            XmlDocument response = new XmlDocument();
            response.AppendChild(response.CreateElement("response"));

            int statuscode = 200;
            string statusmessage = "Command Successful";

            switch (request[0])
            {
                case "LIST":

                    XmlElement serversNode = Add(response.DocumentElement, "servers");
                    Add(serversNode, "count", Servers.Instance.Count);

                    foreach (Server s in Servers.Instance)
                    {
                        XmlElement serverNode = Add(serversNode, "server");

                        Add(serverNode, "localport", s.LocalPort);
                        Add(serverNode, "isrunning", s.IsListening ? "1" : "0");
                        Add(serverNode, "isbeingtraced", s.IsBeingTraced ? "1" : "0");

                        TransmissionRedirectServerObserver observer = s.Observer as TransmissionRedirectServerObserver;
                        SocksServerObserver socksObserver = s.Observer as SocksServerObserver;
                        PortRedirectOverSocksServerObserver throughSocksObserver = s.Observer as PortRedirectOverSocksServerObserver;
                        if (throughSocksObserver != null)
                        {
                            Add(serverNode, "servertype", "2");
                            Add(serverNode, "remotehost", throughSocksObserver.RedirectHost);
                            Add(serverNode, "remoteport", throughSocksObserver.RedirectPort);
                            Add(serverNode, "sockshost", throughSocksObserver.SocksServerHost);
                            Add(serverNode, "socksport", throughSocksObserver.SocksServerPort);
                        }
                        else if (socksObserver != null)
                        {
                            Add(serverNode, "servertype", "3");
                        }
                        else
                        {
                            Add(serverNode, "servertype", "1");
                            Add(serverNode, "remotehost", observer.RedirectHost);
                            Add(serverNode, "remoteport", observer.RedirectPort);
                        }
                    }
                    break;

                case "SOCKS":

                    if (request.Length != 2)
                    {
                        statuscode = 500;
                        statusmessage = "Syntax Error. Usage: SOCKS <LOCAL_PORT>";
                        break;
                    }

                    int localPort;
                    if (!TestLocalPort(request[1], out localPort, ref statuscode, ref statusmessage)) break;

                    Server socksListener = Server.Create(localPort);

                    socksListener.SetObserver(new SocksServerObserver());

                    Servers.Instance.Add(localPort, socksListener);

                    statusmessage = "Socks Server added successfully";

                    break;

                case "ADD":

                    if (request.Length < 4)
                    {
                        statuscode = 500;
                        statusmessage = "Syntax Error. Usage: ADD <LOCAL_PORT> [SOCKS <SOCKS_HOST> <SOCKS_PORT>] <TARGET_HOST> <TARGET_PORT>";
                        break;
                    }

                    if (!TestLocalPort(request[1], out localPort, ref statuscode, ref statusmessage)) break;

                    bool isSocks = request[2] == "SOCKS";

                    if ((isSocks && request.Length != 7) || (!isSocks && request.Length != 4))
                    {
                        statuscode = 500;
                        statusmessage = "Syntax Error. Usage: ADD <LOCAL_PORT> [SOCKS <SOCKS_HOST> <SOCKS_PORT>] <TARGET_HOST> <TARGET_PORT>";
                        break;
                    }

                    int remotePort = 0;
                    int socksPort = 0;

                    if (isSocks)
                    {
                        remotePort = TestPort(request[6], ref statuscode, ref statusmessage);
                        socksPort = TestPort(request[4], ref statuscode, ref statusmessage);
                    }
                    else
                    {
                        remotePort = TestPort(request[3], ref statuscode, ref statusmessage);
                    }

                    if (remotePort == 0 || (isSocks && socksPort == 0)) break;

                    IPHostEntry host = null;
                    Connection targetConnection = null;
                    IPAddress selectedIPV4Address = null;
                    string remoteHost = null;
                    string socksHost = null;

                    if (!isSocks)
                    {
                        if (request[2] != "127.0.0.1" && request[2].ToLowerInvariant() != "localhost" && request[2].ToLowerInvariant() != "loopback")
                        {
                            try
                            {
                                host = System.Net.Dns.GetHostEntry(request[2]);
                            }
                            catch (Exception exp)
                            {
                                statuscode = 503;
                                statusmessage = "DNS Error: " + exp.Message;
                                break;
                            }
                        }
                        else
                        {
                            host = new IPHostEntry();
                            host.AddressList = new IPAddress[1];
                            host.AddressList[0] = IPAddress.Parse("127.0.0.1");
                        }

                        try
                        {
                            foreach (IPAddress address in host.AddressList)
                            {
                                if (address.GetAddressBytes().Length > 4) continue;
                                selectedIPV4Address = address;
                                break;
                            }
                            remoteHost = selectedIPV4Address.ToString();
                            TestRemoteServer(remoteHost, remotePort, ref statuscode, ref statusmessage);
                            if (statuscode != 200) break;
                        }
                        catch (Exception exp)
                        {
                            statuscode = 504;
                            statusmessage = "Connect Error: " + exp.Message;
                            break;
                        }
                    }
                    else // if (!isSocks)
                    {
                        socksHost = request[3];
                        remoteHost = request[5];
                        TestRemoteServer(remoteHost, remotePort, socksHost, socksPort, ref statuscode, ref statusmessage);
                        if (statuscode != 200) break;
                    }

                    Server newListener = Server.Create(localPort);

                    if (isSocks)
                    {
                        newListener.SetObserver(new PortRedirectOverSocksServerObserver(socksHost, socksPort, remoteHost, remotePort));
                    }
                    else
                    {
                        newListener.SetObserver(new TransmissionRedirectServerObserver(remoteHost, remotePort));
                    }

                    Servers.Instance.Add(localPort, newListener);

                    statusmessage = "Server added successfully";

                    break;

                case "START":
                case "STOP":
                case "REMOVE":
                case "TEST":

                    if (request.Length != 2)
                    {
                        statuscode = 500;
                        statusmessage = string.Format("Syntax Error. Usage: {0} <LOCAL_PORT>", request[0]);
                        break;
                    }

                    int localPortToStart;
                    if ((localPortToStart = TestPort(request[1], ref statuscode, ref statusmessage)) == 0) break;

                    if (!Servers.Instance.Contains(localPortToStart))
                    {
                        statuscode = 505;
                        statusmessage = "No server is defined on port " + localPortToStart;
                        break;
                    }

                    try
                    {
                        if (request[0] == "START")
                        {
                            Servers.Instance.Start(localPortToStart);
                        }
                        else if (request[0] == "STOP")
                        {
                            Servers.Instance.Stop(localPortToStart);
                        }
                        else if (request[0] == "REMOVE")
                        {
                            Servers.Instance.Remove(localPortToStart);
                        }
                        else if (request[0] == "TEST")
                        {
                            try
                            {
                                TransmissionRedirectServerObserver observer = Servers.Instance[localPortToStart].Observer as TransmissionRedirectServerObserver;
                                PortRedirectOverSocksServerObserver observerThroughSocks = Servers.Instance[localPortToStart].Observer as PortRedirectOverSocksServerObserver;
                                SocksServerObserver socksobserver = Servers.Instance[localPortToStart].Observer as SocksServerObserver;
                                if (socksobserver != null)
                                {
                                    // Do nothing
                                }
                                else if (observerThroughSocks != null)
                                {
                                    TestRemoteServer(observer.RedirectHost, observer.RedirectPort, observerThroughSocks.SocksServerHost, observerThroughSocks.SocksServerPort, ref statuscode, ref statusmessage);
                                    if (statuscode != 200) break;
                                }
                                else
                                {
                                    TestRemoteServer(observer.RedirectHost, observer.RedirectPort, ref statuscode, ref statusmessage);
                                    if (statuscode != 200) break;
                                }
                            }
                            catch (Exception exp)
                            {
                                statuscode = 507;
                                statusmessage = "Test Failed: " + exp.Message;
                                break;
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        statuscode = 506;
                        statusmessage = "Error: " + exp.Message;
                        break;
                    }

                    statusmessage = request[0] == "START" ? "Server is started" : request[0] == "STOP" ? "Server is stopped" : request[0] == "REMOVE" ? "Server is removed" : "Server is successfully tested";

                    break;

                case "TRACE":
                    if (request.Length != 3)
                    {
                        statuscode = 500;
                        statusmessage = string.Format("Syntax Error. Usage: TRACE <LOCAL_PORT> <ENABLE|DISABLE|STATUS>", request[0]);
                        break;
                    }

                    int localPortToTrace;
                    if ((localPortToTrace = TestPort(request[1], ref statuscode, ref statusmessage)) == 0) break;

                    if (!Servers.Instance.Contains(localPortToTrace))
                    {
                        statuscode = 505;
                        statusmessage = "No server is defined on port " + localPortToTrace;
                        break;
                    }

                    if (request[2] == "ENABLE")
                    {
                        Servers.Instance[localPortToTrace].IsBeingTraced = true;
                        statusmessage = "Tracing has been Enabled for local port " + localPortToTrace;
                    }
                    else if (request[2] == "DISABLE")
                    {
                        Servers.Instance[localPortToTrace].IsBeingTraced = false;
                        statusmessage = "Tracing has been Disabled for local port " + localPortToTrace;
                    }
                    else if (request[2] == "STATUS")
                    {
                        statusmessage = "Tracing is " + (Servers.Instance[localPortToTrace].IsBeingTraced ? "Enabled" : "Disabled") + " for local port " + localPortToTrace;
                    }
                    else
                    {
                        statuscode = 500;
                        statusmessage = string.Format("Syntax Error. Usage: TRACE <ENABLE|DISABLE|STATUS>", request[0]);
                    }

                    break;

                case "LOGLIST":
                    //if (request.Length != 2)
                    //{
                    //    statuscode = 500;
                    //    statusmessage = string.Format("Syntax Error. Usage: {0} <LOCAL_PORT>", request[0]);
                    //    break;
                    //}

                    //int localPortToStart;
                    //if ((localPortToStart = TestPort(request[1], ref statuscode, ref statusmessage)) == 0) break;

                    //if (!Servers.Instance.Contains(localPortToStart))
                    //{
                    //    statuscode = 505;
                    //    statusmessage = "No server is defined on port " + localPortToStart;
                    //    break;
                    //}

                    break;

                case "EXIT":
                    break;

                default:

                    statuscode = 508;
                    statusmessage = "Command Not Understood. Supported Commands: LIST, ADD, REMOVE, START, SOCKS, TEST, STOP, TRACE, EXIT";
                    break;
            }

            Add(response.DocumentElement, "statuscode", statuscode);
            Add(response.DocumentElement, "statusmessage", statusmessage);

            return response;

        }
    }
}
