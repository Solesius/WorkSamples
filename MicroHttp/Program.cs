using System.Threading.Tasks;
using System;
using System.DirectoryServices;
using System.Text.Json;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

#pragma warning disable CA1416
namespace microhttp
{
    class Program
    {
        public static string Origin = "";
        public static bool Caching = false;

        //todo single big buffer, filter obj by ldap object type property 
        public static ConcurrentBag<ActiveDirectoryCacheItem> PrimaryUserCache = new ConcurrentBag<ActiveDirectoryCacheItem>();
        public static ConcurrentBag<ActiveDirectoryCacheItem> secondaryUserCache = new ConcurrentBag<ActiveDirectoryCacheItem>();
        public static ConcurrentBag<ActiveDirectoryCacheItem> primaryGroupCache = new ConcurrentBag<ActiveDirectoryCacheItem>();
        public static ConcurrentBag<ActiveDirectoryCacheItem> secondaryGroupCache = new ConcurrentBag<ActiveDirectoryCacheItem>();
        static void Main(string[] args)
        {
            using (var t = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    CacheADObjects(
                        "(&(!(objectClass=computer))(objectClass=User))",
                        PrimaryUserCache,
                        secondaryUserCache
                    );
                    CacheADObjects(
                        "(&(!(objectClass=computer))(objectClass=group))",
                        primaryGroupCache,
                        secondaryGroupCache
                    );
                    Thread.Sleep(900000);
                }
            }))
            {
                Origin = args[2];
                new MicroService(args[0], System.Convert.ToInt32(args[1]), args[2]).
                    AddRoutes(RouteLoader.Routes).
                    Listen();
            }

        }
        private static void CacheADObjects(
                string ldapQuery,
                ConcurrentBag<ActiveDirectoryCacheItem> primary,
                ConcurrentBag<ActiveDirectoryCacheItem> secondary
            )
        {
            //serves stale until updated. 
            Caching = true;
            primary.Clear();

            using (
                DirectorySearcher ldap = new DirectorySearcher
                {
                    Filter = ldapQuery,
                    PageSize = 6000,
                    Asynchronous = true
                }
            )
            {
                Console.WriteLine("Caching Initiated for the ldap query: " + ldapQuery);
                using SearchResultCollection searchResultCollection = ldap.FindAll();
                
                foreach (SearchResult e in searchResultCollection)
                {
                    using DirectoryEntry entry = e.GetDirectoryEntry();

                    bool isGroup = false;
                    foreach (string objectClass in e.Properties["objectclass"])
                    {
                        if (string.Equals(objectClass, "group", StringComparison.OrdinalIgnoreCase))
                        {
                            isGroup = true;
                            break;
                        }
                    }
                    if (isGroup)
                    {
                        var prefix = new string[] {
                            //group prefixes to filter on here
                        };
                        foreach (var groupPrefix in prefix)
                        {
                            if (
                                  ((string)ADHelper.LDAPValue(e, "samaccountname")).ToLower().Contains(groupPrefix) ||
                                  ((string)ADHelper.LDAPValue(e, "samaccountname")).ToLower().Equals(groupPrefix)
                               )
                            {
                                primary.Add(
                                    new ActiveDirectoryCacheItem(
                                        "",
                                        "",
                                        (string)ADHelper.LDAPValue(e, "samaccountname")
                                    )
                                );
                            }
                        }
                    }
                    else
                    {
                        if ((int)ADHelper.LDAPValue(e, "useraccountcontrol") != 514)
                        {
                            primary.Add(
                                new ActiveDirectoryCacheItem(
                                    (string)ADHelper.LDAPValue(e, "givenname"),
                                    (string)ADHelper.LDAPValue(e, "sn"),
                                    (string)ADHelper.LDAPValue(e, "samaccountname")
                               )
                            );
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }


            //flip the bit back, so any requests get newly updated cache. 
            Caching = false;
            secondary.Clear();

	    //test performance of arrays.copy here
            primary.ToList().ForEach(item => secondary.Add(item));

            Console.WriteLine("Cache refreshed, daemon sleeping..");
        }
    }
}