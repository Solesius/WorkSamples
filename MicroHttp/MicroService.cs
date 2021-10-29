using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
//using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace microhttp
{
    class MicroService : SocketHelper
    {
        int _port;
        string _ip;
        string _origin;

        private HeaderBuilder headerBuilder = new HeaderBuilder();

        public List<Route> routes = new List<Route>();
        private TcpListener TcpListener;

        public MicroService() { }

        public MicroService(
             string ip,
             int port,
             string origin
             )
        {
            _ip = ip;
            _port = port;
            _origin = origin;
        }

        public MicroService Listen()
        {
            IPHostEntry iPHostEntry = Dns.Resolve(
                System.Environment.GetEnvironmentVariable("COMPUTERNAME")
            );
            IPAddress boundAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });

            foreach (var ip in iPHostEntry.AddressList)
            {
                if (ip.Equals(_ip))
                {
                    boundAddress = ip;
                    break;
                }
            }

            try
            {
                TcpListener = new TcpListener(boundAddress, _port);
                TcpListener.Start(10000);
                HandleRequests();
                return this;
            }
            catch(Exception e)

            {
                Console.WriteLine(e.Message);
                TcpListener.Stop();

                return this;
            }
        }

        public MicroService AddRoute(Route r)
        {
            try
            {
                routes.Add(r);
                return this;
            }
            catch
            {
                return this;
            }
        }

        public MicroService AddRoutes(List<Route> r)
        {
            try
            {
                routes.AddRange(r);
                return this;
            }
            catch
            {
                return this;
            }
        }

        private void HandleRequests()
        {
            ADHelper AD = new ADHelper();
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var cert = store.Certificates.Find(
                X509FindType.FindBySerialNumber,
                "",
                true
            )[0];

            for (; ;)
            {
                Socket conn = TcpListener.AcceptSocket();
                NetworkStream stream = new NetworkStream(conn, true);
                SslStream stream1 = new SslStream(stream);
                stream1.AuthenticateAsServer(cert, false, true);

                string input = "";
                byte[] bytes = new byte[4096];
                int bytesRec = stream1.Read(bytes, 0, bytes.Length);

                input += System.Text.Encoding.UTF8.GetString(bytes, 0, bytesRec);

                List<string> requestHeaders = new List<string>(input.Split('\n'));

                string route = requestHeaders[0].Split(' ')[1].ToLower();
                string httpMethod = requestHeaders[0].Split(' ')[0].ToLower();
                string remoteIp = conn.RemoteEndPoint.ToString().Split(':')[0];

                //put some IP's in here
                List<string> approvedIP = new List<string>() {
                    "localhost"
                };

                //TODO HEADER type and parser
                //build up the request body if one is sent, need to do this on all request types/
                //supports json only for now
                string body = "";
                var asArray = requestHeaders.ToArray();
                try
                {
                    Enumerable.Range(
                        requestHeaders.IndexOf(System.Text.Encoding.UTF8.GetString(new byte[] { 13 })),
                        asArray.Length
                    ).ToList().ForEach((chunk) =>
                    {
                        body += asArray[chunk];
                    });
                }
                catch { }

                if (approvedIP.Contains(remoteIp))
                {
                    if (!new List<string>() { "get", "post", "put", "delete" }.Contains(httpMethod))
                        Write(Bytes(headerBuilder.OPTIONS(_origin)), stream1);
                    else
                    {
                        //find the performance here. 
                        if(RouteLoader.Routes.Where((r) => { return r.RoutePath.Equals(route); }).Any())
                        {
                            try
                            {
                                Route matchedRoute = RouteLoader.Routes.Where((_route) =>
                                    { return _route.HTTPMethod.Equals(httpMethod) && _route.RoutePath.Equals(route); })
                                        .First();

                                if (httpMethod.Equals("post"))
                                {
                                    JsonElement json = JsonSerializer.Deserialize<JsonElement>(body);
                                    if (route.Equals("/search")) 
                                    {
                                        QueryInput query = new QueryInput(
                                                json.GetProperty("Name").GetString(),
                                                json.GetProperty("ObjectClass").GetString()
                                            );
                                        matchedRoute.RouteHandler.Invoke(stream1, (object)query);
                                    }
                                    else if(route.Equals("/adduser")) 
                                    {
                                        AddUserInput obj = new AddUserInput(
                                            json.GetProperty("UserName").GetString(),
                                            json.GetProperty("GroupName").GetString()
                                        );
                                    }
                                    else {
                                        matchedRoute.RouteHandler.Invoke(stream1, json.GetProperty("name").GetString());
                                    }
                                }
                                else
                                {
                                    matchedRoute.RouteHandler.Invoke(stream1, "");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("function failed, see inner exception\n");
                                Console.WriteLine(
                                    "InnerException:" + e.Message + "\n" +
                                    "Source:" + e.Source + "\n" +
                                    "CallSite:" + e.TargetSite + "\n" +
                                    "StackTrace:" + e.StackTrace
                                    );
                                Write(Bytes(headerBuilder.ERROR(_origin)), stream1);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Error] - Invalid route requested {route}");
                            Write(Bytes(headerBuilder.ERROR(_origin)), stream1);
                        }
                    }
                }
                else
                {
                    Write(Bytes(headerBuilder.UNAUTHORIZED(_origin)), stream1);
                }
            }
        }
    }
}