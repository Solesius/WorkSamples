using System.DirectoryServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.Json;


#pragma warning disable CA1416
namespace microhttp
{

    class ADHelper : IDisposable
    {
        private static DirectorySearcher LDAP = new DirectorySearcher
        {
            Asynchronous = true,
            PageSize = 500
        };

        public static object LDAPValue(SearchResult s, string prop)
        {
            try
            {
                return s.Properties[prop][0];
            }
            catch
            {
                return "";
            }
        }

        public ADUser GetUserInfo(string userName)
        {
            LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={userName})) )";

            if (LDAP.FindOne() == null)
            {
                return new ADUser();
            }
            else
            {
                var ldapUser = LDAP.FindOne();

                //little catch clauses from edge case ldap values where the
                //LDAPValue helper fails. 
                //PUt this login in LDAPValue function
                long lld = 0;
                try
                {
                    lld = Convert.ToInt64(LDAPValue(ldapUser, "lastlogontimestamp"));
                }
                catch { }

                long pwd = Convert.ToInt64(LDAPValue(ldapUser, "pwdlastset"));

                long badPwdCount = 0;
                try
                {
                    badPwdCount = Convert.ToInt64(LDAPValue(ldapUser, "badPwdCount"));
                }
                catch { }


                string dateFormat = "MM-dd-yy HH:mm:ss";
                return new ADUser
                {
                    UserName = (string)LDAPValue(ldapUser, "samaccountname"),
                    First = (string)LDAPValue(ldapUser, "givenname"),
                    Last = (string)LDAPValue(ldapUser, "sn"),
                    Description = (string)LDAPValue(ldapUser, "description"),
                    Mail = (string)LDAPValue(ldapUser, "mail"),
                    Office = (string)LDAPValue(ldapUser, "physicaldeliveryofficename"),
                    Phone = (string)LDAPValue(ldapUser, "telephonenumber"),
                    Enabled = (int)LDAPValue(ldapUser, "useraccountcontrol") != 514,
                    Locked = badPwdCount > 0,
                    Expired = (DateTime.Now.Date - DateTime.FromFileTime(pwd).Date).Days > 180,
                    Created = ((DateTime)LDAPValue(ldapUser, "whenCreated")).AddHours(-4).AddSeconds(1).ToString(dateFormat),
                    PwdLastSet = DateTime.FromFileTime(pwd).ToString(dateFormat),
                    LastLogon = DateTime.FromFileTime(lld).ToString(dateFormat),
                    PwdExpiration = (DateTime.FromFileTime(pwd).AddDays(180)).ToString(dateFormat)
                };
            }
        }


        public List<ADGroupMember> GetADGroupMembers(string groupName)
        {
            LDAP.PageSize = 2000;
            System.Collections.Concurrent.ConcurrentBag<ADGroupMember> members = new System.Collections.Concurrent.ConcurrentBag<ADGroupMember>();
            List<string> distinguishedNames = new List<string>() { };

            void Go(string g)
            {
                LDAP.Filter = $"(&(&(!(objectClass=computer))(objectClass=group))(samaccountname={g}))";
                var group = LDAP.FindOne();

                if (null == group)
                { }
                else
                {
                    foreach (string dn in group.Properties["member"])
                    {
                        //string pat0 = dn.Split('=')[1].Split(',')[0].Split('-')[0].ToLower();
                        string pat1 = dn.Split('=')[1].Split(',')[0].ToLower();
                        if (!distinguishedNames.Contains(dn))
                        {
                            distinguishedNames.Add(dn);
                        }
                        Go(pat1);
                    }
                }
            }

            Go(groupName);

            var cpu = Convert.ToInt32(System.Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));
            ConcurrentBag<ADGroupMember> asList = new ConcurrentBag<ADGroupMember>(members.ToArray());

            List<string> iterator = new List<string>() { };
            var runs = cpu;
            var hasRun = 0;
            int toProcessPerRun = (distinguishedNames.Count / cpu) + 1;
            var processedInCurrentRun = 0;
            var itemsProcessed = 0;
            while (hasRun <= runs || itemsProcessed <= distinguishedNames.Count)
            {
                if (processedInCurrentRun == toProcessPerRun)
                {
                    hasRun += 1;
                    processedInCurrentRun = 0;
                    var _iterator = new List<string>() { };
                    iterator.ForEach(x => _iterator.Add(x));
                    Task.Factory.StartNew(() =>
                    {
                        var _ldap = new DirectorySearcher
                        {
                            Asynchronous = true
                        };
                        _iterator.ForEach(name =>
                        {
                            _ldap.Filter = $"(&(&(!(objectClass=computer))(objectClass=user))(distinguishedname={name}))";
                            var obj = _ldap.FindOne();

                            var member = new ADGroupMember
                            {
                                DisplayName = (string)LDAPValue(obj, "displayname"),
                                UserName = (string)LDAPValue(obj, "samaccountname"),
                                Location = (string)LDAPValue(obj, "physicaldeliveryofficename"),
                                OrganizationalUnit = (string)name.Split('=')[2]
                            };
                            if (!asList.Contains(member))
                            {
                                members.Add(member);
                            }
                            _ldap.Dispose();

                        });
                    });
                    iterator = new List<string>() { };
                }
                else
                {
                    try
                    {
                        iterator.Add(distinguishedNames[itemsProcessed]);

                    }
                    catch
                    { }
                    processedInCurrentRun++;
                    itemsProcessed++;
                }
            }

            //ugly spin wait thing, trying to go fast here on recurse
            while (members.Count != distinguishedNames.Count)
            { }
            return members.Distinct().ToList();
        }

        //can do better here
        public IEnumerable<string> GetUserGroups(string userName)
        {
            LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={userName})) )";
            var result = LDAP.FindOne();

            if (null == result)
            {
                return (IEnumerable<string>)new List<string>();
            }
            else
            {
                DirectoryEntry user = result.GetDirectoryEntry();
                List<string> groups = new List<string>();
                foreach (string s in user.Properties["memberof"])
                {
                    groups.Add(s.Split(',')[0].Split('=')[1]);
                }

                user.Close();
                user.Dispose();
                return groups.OrderBy((x) => { return x; });
            }
        }
        public JsonResponse UnlockAccount(string username)
        {
            LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={username})) )";
            try
            {
                SearchResult entry = LDAP.FindOne();

                long lockoutTime = 0;
                try
                {
                    lockoutTime = Convert.ToInt64(LDAPValue(entry, "badPwdCount"));
                }
                catch { }

                if (lockoutTime == 0)
                {
                    return new JsonResponse("The AD account associated with " +
                        $"{(string)LDAPValue(entry, "displayname")} is not locked");
                }
                else
                {
                    try
                    {
                        using (DirectoryEntry _entry = entry.GetDirectoryEntry())
                        {
                            _entry.Properties["lockoutTime"][0] = 0;
                            _entry.CommitChanges();
                            return new JsonResponse("The AD account associated with " +
                                $"{(string)LDAPValue(entry, "displayname")} has been succesfully unlocked");
                        }
                    }
                    catch (Exception e)
                    {
                        return new JsonResponse("There was an error when unlocking AD account associated with" +
                            $"{(string)LDAPValue(entry, "displayname")}\r\n" +
                            $"{e.Message}");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.Source, e.StackTrace, e.TargetSite, e.Data);
                return new JsonResponse($"The provided username: {username} is invalid");
            }
        }

        public Boolean ResetPassword(string username, string newPassword)
        {
            LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={username})) )";

            var res = LDAP.FindOne();

            if (res == null)
            {
                return false;
            }
            else
            {
                string pass = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(newPassword)
                );
                try
                {
                    using (DirectoryEntry entry = res.GetDirectoryEntry())
                    {
                        entry.Invoke("SetPassword", pass);
                        entry.CommitChanges();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }

        public JsonResponse AddUserToGroup(string username, string group)
        {
            try
            {
                LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={group})))";
                SearchResult groupEntry = LDAP.FindOne();

                LDAP.Filter = $"(&(!(objectclass=computer)(samaccountname={username})))";
                SearchResult userEntry = LDAP.FindOne();

                try
                {
                    using (DirectoryEntry _groupEntry = groupEntry.GetDirectoryEntry())
                    {
                        _groupEntry.Properties["member"].Add(
                           (string)LDAPValue(userEntry, "distinguishedname")
                        );
                        _groupEntry.CommitChanges();
                        _groupEntry.Close();

                        // _groupEntry.Invoke("Add",new object[]{userEntry.Path.ToString()});
                        // _groupEntry.Close();

                        return new JsonResponse($"The AD account {username} " +
                            $"has been succesfully added to {group} ");
                    }
                }
                catch (Exception e)
                {
                    return new JsonResponse($"There was an error when adding {username} to group" +
                        $" {group}\n" +
                        $"{e.Message}");
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.Source, e.StackTrace, e.TargetSite, e.Data);
                return new JsonResponse($"The provided username: {username} is invalid");
            }
        }

        public void Dispose()
        {
            LDAP.Dispose();
        }
    }
}