using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;

namespace ADConnectors
{
    public class NovellLdap : IADSearcher
    {

        private LdapConnection conn;
        public NovellLdap(IConfiguration configuration) {
            bool useSSL;
            Boolean.TryParse(configuration["UseSSL"], out useSSL);

            string ldapHost = configuration["Host"];
            int ldapPort = Int16.Parse(configuration["Port"]);
            string loginDN = configuration["LoginDN"];
            string password = configuration["Password"];
            conn = new LdapConnection();
            if (useSSL) {
                conn.SecureSocketLayer = true;
            }
            conn.ConnectionTimeout = 300;

            try {
                conn.Connect(ldapHost, ldapPort);
            } catch (System.Security.Authentication.AuthenticationException e) {
                Console.WriteLine("Default certification check failed:");
                Console.WriteLine($"\t{e.Message}");
                bool forceSSL;
                Boolean.TryParse(configuration["ForceSSL"], out forceSSL);
                if (forceSSL) {
                    Console.WriteLine("Because ForceSSL is `true`, please fix certification or authentication error before try again.");
                    return;
                } else {
                    Console.WriteLine("\ttry to by pass it!");
                    conn.UserDefinedServerCertValidationDelegate += HELPER.CertificateValidationCallBack;
                }
            }
            conn.Bind(loginDN, password);
        }

        /// <summary>
        /// Search in AD for non-eRSA accounts created after a given date
        /// Only display selected attributes defined in _essentialProperties
        /// </summary>
        /// <param name="earliest"></param>
        public List<Dictionary<string, string>> Search(DateTime earliest)
        {
            string whenCreated = earliest.ToUniversalTime().ToString("yyyyMMddHHmmss.0Z");
            Console.WriteLine("Local {0} to UTC {1}", earliest, whenCreated);

            string userFilter = HELPER.CreateFilter(whenCreated);

            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            LdapSearchResults lsc = conn.Search(HELPER.SEARCH_BASE,
                LdapConnection.SCOPE_SUB,
                userFilter,
                HELPER.CREATION_PROPERTIES,
                false);

            int count = 0;
            while (lsc.hasMore())
            {
                LdapEntry nextEntry = null;
                try
                {
                    nextEntry = lsc.next();
                    count++;
                }
                catch (LdapReferralException) {
                    // Nothing really serious: constraints.ReferralFollowing = true this may not be needed
                    // https://www.novell.com/documentation/developer/ldapcsharp/?page=/documentation/developer/ldapcsharp/cnet/data/b3u4u0n.html
                    // https://technet.microsoft.com/en-us/library/cc978014.aspx
                    continue;
                }
                catch (LdapException e)
                {
                    Console.WriteLine("Move next error: {0}", e.ToString());
                    Console.WriteLine("Error message: " + e.Message);
                    continue;
                }
                Console.WriteLine("\n" + nextEntry.DN);
                try {
                    results.Add(GetProperties(nextEntry.getAttributeSet()));
                } catch (NullReferenceException ex)
                {
                    Console.WriteLine("Not a qualified person account");
                    Console.WriteLine(ex.Message);
                }
            }
            return results;
        }

        public Dictionary<string, string> GetUser(int uidnumber, bool all=false)
        {
            string filter = string.Format("(uidnumber={0})", uidnumber);
            string[] properties = { };
            if (!all) {
                properties = HELPER.BASIC_PROPERTIES;
            }
            LdapSearchResults lsc = conn.Search(HELPER.SEARCH_BASE,
                LdapConnection.SCOPE_SUB,
                filter,
                properties,
                false);
            try {
                LdapEntry entry = lsc.next();
                return GetProperties(entry.getAttributeSet());
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                return new Dictionary<string, string>();
            }
        }

        public void Dispose() {
            if (conn != null) conn.Disconnect();
        }

        /// <summary>
        /// Convert ResultPropertyCollection to Dictionary
        /// </summary>
        /// <param name="searchResult">SearchResult</param>
        /// <returns>Dictionary</returns>
        private static Dictionary<string, string> GetProperties(LdapAttributeSet searchResult)
        {
            if (searchResult == null) return null;

            Dictionary<string, string> filtered = new Dictionary<string, string>();

            // Our checking logic depends on uidnumber
            if (searchResult.getAttribute("uidnumber") == null)
                throw new NullReferenceException(string.Format("{0} has no uidnumber", searchResult.getAttribute("cn")));

            foreach(LdapAttribute attribute in searchResult) {
                Console.WriteLine(attribute.Name.ToLower() + " = " + attribute.StringValue);
                filtered[attribute.Name.ToLower()] = attribute.StringValue;
            }

            return filtered;
        }

    }
}
