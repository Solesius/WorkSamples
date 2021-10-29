using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace microhttp
{
    class RouteLoader : SocketHelper
    {
        private static HeaderBuilder hb = new HeaderBuilder(Program.Origin);
        private static ADHelper ad = new ADHelper();

        public static List<string> RoutesInService
        {
            get
            {
                return new List<string>(Routes.Select(r => r.RoutePath));
            }
        }
        public static List<Route> Routes
        {
            get
            {
                return new List<Route>{
                    new Route(
                    "GET",
                    "/",
                    (stream, obj) =>
                    {
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.GET(hb._origin)),
                                Bytes("Service Online")
                            },
                            stream
                        );
                        return true;
                    }),
                    new Route(
                    "POST",
                    "/getuserinfo",
                    (stream, name) =>
                    {
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.POST(hb._origin)),
                                Bytes(JsonSerializer.Serialize<ADUser>(ad.GetUserInfo((string)name)))
                            },
                            stream
                        );

                        return true;
                    }),
                    new Route(
                    "POST",
                    "/unlockuser",
                    (stream, name) =>
                    {
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.POST(hb._origin)),
                                Bytes(JsonSerializer.Serialize<JsonResponse>(ad.UnlockAccount((string)name)))
                            },
                            stream
                            );
                        return true;
                    }),
                    new Route(
                    "POST",
                    "/adduser",
                    (stream, obj) =>
                    {
                        AddUserInput addObj = (AddUserInput)obj;
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.POST(hb._origin)),
                                Bytes(JsonSerializer.Serialize<JsonResponse>(ad.AddUserToGroup(
                                    addObj.UserName,
                                    addObj.GroupName
                                )))
                            },
                            stream
                        );
                            return true;

                    }),
                    new Route(
                    "POST",
                    "/getusergroups",
                    (stream, name) =>
                    {
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.POST(hb._origin)),
                                Bytes(JsonSerializer.Serialize<IEnumerable<string>>(ad.GetUserGroups((string)name)))
                            },
                            stream
                           );
                        return true;
                    }),
                    new Route(
                    "POST",
                    "/getgroupmembers",
                    (stream, name) =>
                    {
                        Write(
                            new List<byte[]>(){
                                Bytes(hb.GET(hb._origin)),
                                Bytes(JsonSerializer.Serialize(ad.GetADGroupMembers((string)name)))
                            },
                            stream
                        );
                        return true;
                    }),
                    new Route("GET", "/favicon.ico" , (stream, obj) => { Write(Bytes(hb.ERROR(hb._origin)), stream); return true; }),
                    new Route("POST", "/restartservice" , (stream, obj) => {
                        using (var cmd = new System.Diagnostics.Process())
                        {
                            cmd.StartInfo.FileName = "c:/windows/system32/cmd.exe";
                            cmd.StartInfo.Arguments = "/c net stop aams && net start aams";
                            cmd.Start();
                            while (!cmd.HasExited)
                            {}
                            cmd.Kill();
                        }
                        
                        Write(Bytes(hb.POST(hb._origin)), stream); return true; 
                    }),
                    new Route(
                        "POST",
                        "/search",
                        (socket, queryObject) =>
                        {
                            QueryInput _queryObject = (QueryInput)queryObject;
                            string _name = _queryObject.Name.ToLower();
                            string objectClass = _queryObject.ObjectClass;

                            if (_name.IndexOf(' ') != -1) {
                                if (Program.Caching) {
                                    if(objectClass.Equals("group"))
                                    {
                                        ProcessQueryWildCard(Program.secondaryGroupCache);
                                    }
                                    else{
                                        ProcessQueryIncludeSpaces(Program.secondaryUserCache);
                                    }
                                } else {
                                    if(objectClass.Equals("group"))
                                    {
                                        ProcessQueryWildCard(Program.primaryGroupCache);
                                    }
                                    else{
                                        ProcessQueryIncludeSpaces(Program.PrimaryUserCache);
                                    }
                                }
                            }
                            else {
                                if (Program.Caching) {
                                    if(objectClass.Equals("group"))
                                    {
                                        ProcessQueryWildCard(Program.secondaryGroupCache);
                                    }
                                    else{
                                        ProcessQueryWildCard(Program.secondaryUserCache);
                                    }
                                } else {
                                    if(objectClass.Equals("group"))
                                    {
                                        ProcessQueryWildCard(Program.primaryGroupCache);
                                    }
                                    else{
                                        ProcessQueryWildCard(Program.PrimaryUserCache);
                                    }
                                }
                            }
                            void ProcessQueryWildCard(ConcurrentBag<ActiveDirectoryCacheItem> set) {
                                var userSet = from user in set.Distinct()
                                                let x = user.fn.ToLower()
                                                let y = user.ln.ToLower()
                                                let z = user.samaccountname.ToLower()
                                                where x.Contains(_name) || y.Contains(_name) || z.Contains(_name)
                                                select new ActiveDirectoryCacheItem(
                                                        user.fn,
                                                        user.ln,
                                                        user.samaccountname
                                                    );

                                if (!userSet.Any()) {
                                    var chunks = new List < byte[] > () {
                                        Bytes(hb.POST(hb._origin)),
                                            Bytes(System.Text.Json.JsonSerializer.Serialize((IEnumerable < string > ) new List < string > ()))
                                    };
                                    Write(chunks, socket);
                                } else {
                                    var chunks = new List < byte[] > () {
                                        Bytes(hb.POST(hb._origin)),
                                            Bytes(System.Text.Json.JsonSerializer.Serialize(userSet))
                                    };
                                    Write(chunks, socket);
                                }

                            }

                            void ProcessQueryIncludeSpaces(System.Collections.Concurrent.ConcurrentBag < ActiveDirectoryCacheItem > set) {
                                var userSet = from user in set.Distinct()
                                                let names = _name.Split(' ').Select(s => s.ToLower()).ToArray()
                                                where user.fn.ToLower().Contains(names[0]) && user.ln.ToLower().Contains(names[1])
                                                select new ActiveDirectoryCacheItem(
                                                        user.fn,
                                                        user.ln,
                                                        user.samaccountname
                                                );

                                if (!userSet.Any()) {
                                    var chunks = new List <byte[]> () {
                                        Bytes(hb.POST(hb._origin)),
                                            Bytes(System.Text.Json.JsonSerializer.Serialize(new List < string > ()))
                                    };
                                    Write(chunks, socket);
                                } else {
                                    var chunks = new List < byte[] > () {
                                        Bytes(hb.POST(hb._origin)),
                                            Bytes(System.Text.Json.JsonSerializer.Serialize(userSet))
                                    };
                                    Write(chunks, socket);
                                }
                            }
                            return true;
                        }
                    )
                };
            }
        }
    }
}