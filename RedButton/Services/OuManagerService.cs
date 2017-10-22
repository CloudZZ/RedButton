using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;

namespace RedButton.Services
{
    public class OuManagerService
    {
        private readonly string _path;

        public OuManagerService()
        {
            _path = ConfigurationManager.AppSettings["ad-Path"];
        }

        public bool IsValidKey(string ou, string key)
        {
            return ou != null && key != null && key == GetOuDescription(ou);
        }

        public List<string> ListAllActiveAccounts(string ou)
        {
            var list = new List<string>();
            using (var ouRoot = new DirectoryEntry(GetOuConnectionString(ou)))
            using(var userSearcher = new DirectorySearcher(ouRoot))
            {
                // define a standard LDAP filter for what you search for - here "users"    
                userSearcher.Filter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                // define the properties you want to have returned by the searcher
                userSearcher.PropertiesToLoad.Add("name");

                // search and iterate over results
                list.AddRange(from SearchResult result in userSearcher.FindAll() select result.Properties["name"][0].ToString());
            }
            return list;
        }

        public List<string> DisableAccounts(string ou)
        {
            var results = new List<string>();
            using (var ouRoot = new DirectoryEntry(GetOuConnectionString(ou)))
            using (var userSearcher = new DirectorySearcher(ouRoot))
            {
                // define a standard LDAP filter for what you search for - here "users"    
                userSearcher.Filter = "(&(objectCategory=person)(objectClass=user)(!userAccountControl:1.2.840.113556.1.4.803:=2))";
                // define the properties you want to have returned by the searcher
                userSearcher.PropertiesToLoad.Add("userAccountControl");
                userSearcher.PropertiesToLoad.Add("sAMAccountName");

                // search and iterate over results
                foreach (SearchResult result in userSearcher.FindAll())
                {
                    using (var user = result.GetDirectoryEntry())
                    {
                        var val = (int)user.Properties["userAccountControl"][0];
                        user.Properties["userAccountControl"].Value = val | 0x2;
                        user.CommitChanges();
                        results.Add(result.Properties["sAMAccountName"][0].ToString());
                    }
                }
            }
            return results;
        }

        public string LdapRoot => $"LDAP://{_path}";

        public string GetOuConnectionString(string ou)
        {
            return $"LDAP://OU={ou},{_path}";
        }

        private string GetOuDescription(string ou)
        {
            using (var rootOu = new DirectoryEntry(GetOuConnectionString(ou)))
            {
                return rootOu.Properties["description"].Value.ToString();
            }
        }

        public string GetMail(string ou)
        {
            using (var rootOu = new DirectoryEntry(GetOuConnectionString(ou)))
            {
                return rootOu.Properties["postalCode"].Value.ToString();
            }
        }
    }
}