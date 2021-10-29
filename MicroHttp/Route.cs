using System;
using System.Net.Security;

namespace microhttp
{
    class Route
    {
        public string HTTPMethod;
        public string RoutePath;
        public Func<SslStream, object, bool> RouteHandler;

        public Route(
            string httpMethod,
            string routePath,
            Func<SslStream, object, bool> handler
            ) => (HTTPMethod, RoutePath, RouteHandler) =
               (httpMethod.ToLower(), routePath.ToLower(), handler);
        
        public override string ToString()
        {
            return "method : " + HTTPMethod + "\n" +
                    "routePath : " + RoutePath + "\n" +
                    "handler : " + RouteHandler.ToString() + "\n";
        }
    }

}