using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ADConnectors
{
#region HELPER
    /// <summary>
    /// Default values and functions for the namespace
    /// </summary>
    internal static class HELPER {
        // To describe an account
        public static string SEARCH_BASE = "DC=ad,DC=ersa,DC=edu,DC=au";

        //TODO: may remove this list and only use CREATION_PROPERTIES the fields can be mapped to User
        public static string[] BASIC_PROPERTIES = {"cn", "uidnumber", "company", "department", "telephonenumber"};
        // For checking new account
        public static string[] CREATION_PROPERTIES = {
            "givenname",
            "sn",
            "cn",
            "uidnumber",
            "whencreated",
            "mail",
            "distinguishedname",
            "samaccountname",
            "company",
            "department"
        };

        /// <summary>
        /// Validates the SSL server certificate ourselves.
        /// When this is used, there is something wrong with certs. So use this to check then decide how to deal it
        /// if no one can fix the cert problem:
        /// openssl s_client -connect remotehost:port
        /// </summary>
        /// <param name="sender">An object that contains state information for this
        /// validation.</param>
        /// <param name="cert">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the
        /// remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote
        /// certificate.</param>
        /// <returns>Returns a boolean value that determines whether the specified
        /// certificate is accepted for authentication; true to accept or false to
        /// reject.</returns>
        public static bool CertificateValidationCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("AD server's certification has not passed the default checking,");
            Console.WriteLine("\tHere are some more information for you to check.");
            Console.WriteLine($"\tIssuerName = {certificate.Issuer}");
            Console.WriteLine($"\tSubject = {certificate.Issuer}, EffectiveDate = {2}");
            Console.WriteLine($"\tEffectiveDate = {certificate.GetEffectiveDateString()}");
            Console.WriteLine($"\tHash string: {certificate.GetCertHashString()}");
            Console.WriteLine("Here are some of certification errors just for you to fix it:");
            // https://msdn.microsoft.com/en-us/library/office/jj900163(v=exchg.150).aspx shows how to not reject self-signed
            if (chain != null && chain.ChainStatus != null)
            {
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    Console.WriteLine($"\tStatus: {status.StatusInformation}");
                }
            }
            return certificate.GetRawCertData() != null;
        }

        public static string CreateFilter(string whenCreated) {
            // https://msdn.microsoft.com/en-us/library/aa746475(v=vs.85).aspx
            return "(&(objectCategory=User)(objectClass=User)(objectClass=Person)(!(objectClass=Computer))(mail=*@*)(!(mail=*ersa.edu.au))(whenCreated>=" + whenCreated + "))";
        }
    }
#endregion

    /// <summary>
    /// AD User
    /// </summary>
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DistinguishedName { get; set; }
        public string AccountName { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }

        static Dictionary<string, string> MAP = new Dictionary<string, string> {
            ["givenname"] = "FirstName",
            ["sn"] = "LastName",
            ["mail"] = "Email",
            ["distinguishedname"] = "DistinguishedName",
            ["samaccountname"] = "AccountName",
            ["company"] = "Company",
            ["department"] = "Department"
        };
        
        private static void VerifyAllKeyPresent(Dictionary<string, string> user)
        {
            foreach(string expected in MAP.Keys)
            {
                if (!user.ContainsKey(expected))
                    throw new KeyNotFoundException($"Missing required key {expected}");
            }
        }
        /// <summary>
        /// Load values from a Dictionary returned from a search
        /// It throws when a required key is not in the dictionary
        /// </summary>
        /// <param name="user"></param>
        public User(Dictionary<string, string> user)
        {
            VerifyAllKeyPresent(user);

            Type classType = typeof(User);
            foreach(string property in MAP.Keys)
            {
                if (user.ContainsKey(property))
                    classType.GetProperty(MAP[property]).SetValue(this, user[property]);
            }
        }
    }

    /// <summary>
    /// Interface to provide search functions against active directories
    /// </summary>
    public interface IADSearcher: IDisposable
    {
        /// <summary>
        /// Search in AD for non-eRSA accounts created after a given date
        /// </summary>
        /// <param name="earliest"></param>
        List<Dictionary<string, string>> Search(DateTime earliest);

        /// <summary>
        /// Get a full description a User
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Dictionary of user information or null</returns>
        Dictionary<string, string> GetUser(int uidNumber, bool all=false);
    }

    /// <summary>
    /// A function to create an ADConnector for the running platform
    /// <summary>
    // all caller need to use using ?
    public static class Creater {
        public static IADSearcher GetADConnector(IConfiguration configuration) {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) {
                Log.Debug("Runtime is Windows");
                return new AD(configuration);
            }
            else
            {
                Log.Debug("Runtime is Linux");
                return new NovellLdap(configuration);
            }
        }
    }
}
