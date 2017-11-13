using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Json;
using ADConnectors;
using Client;

namespace Runners
{
    class Program
    {
        CRMClient _crmClient;

        public Program(IConfiguration configuration)
        {
            _crmClient = new CRMClient(configuration["Dynamics:Authority"], configuration["Dynamics:Resource"],
                configuration["Dynamics:ClientId"], configuration["Dynamics:ClientSecret"], configuration["Dynamics:Version"]);
        }

        /// <summary>
        /// Main with all optional arguments in such order:
        /// 1. earliest date to query from for new AD accounts in the format of yyyyMMdd
        /// 2. config JSON file path related to current path,
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            var default_args = new Dictionary<string, string>
            {
                {"config", "secrets.json"},
                {"earliest", new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture)}
            };

            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(default_args)
                .AddCommandLine(args);

            IConfiguration switchs = builder.Build();

            DateTime earliest;
            try
            {
                earliest = DateTime.ParseExact(switchs["earliest"], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("{0} is in wrong DateTime format. It should be in yyyyMMdd format.", switchs["earliest"]);
                return;
            }

            string config = switchs["config"];
            if (!File.Exists(config)) {
                Console.WriteLine($"Config file {config} does not exist");
            };

            Console.WriteLine("Search AD accounts created since {0} with configuration file in {1}", switchs["earliest"], switchs["config"]);

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(config)
                .Build();

            (List<User> users, List<Dictionary<string, string>> incompleteUsers) = CheckNewAccount(configuration, earliest);

            if (users.Count == 0)
            {
                Console.WriteLine($"No new accounts found for the period started from {earliest}");
                return;
            }

            Program p = new Program(configuration);
            (List<string> createdUsers, List<string> unabledUsers, List<Dictionary<string, string>> exceptionUsers) = await p.CheckUserInCRM(users);

            System.Text.StringBuilder sb = new System.Text.StringBuilder
            {
                Capacity = 200
            };

            if (incompleteUsers.Count > 0)
            {
                Console.WriteLine("Incomplete user - missing key fields.");
                foreach (var user in incompleteUsers)
                {
                    PrintUser(user);
                    Console.WriteLine();
                }
                BuildSection(sb,
                    Convert(incompleteUsers),
                    "List of AD users with missing key fields:");
            }
            BuildSection(sb, unabledUsers, "List of AD users cannot be created as Contacts because of missing information to find Account to attach:");
            BuildSection(sb,
                Convert(exceptionUsers),
                "List of AD users cannot be created need developer's attention:");
            BuildSection(sb, createdUsers, "List of successfully created AD users:");
            Notify(configuration.GetSection("Notifiers"), sb.ToString());
        }

        private static void BuildSection(System.Text.StringBuilder sb, List<string> createdUsers, string title)
        {
            if (createdUsers.Count > 0)
            {
                sb.AppendLine(title);
                foreach (string dn in createdUsers)
                {
                    sb.AppendLine(dn);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
        }

        private static List<string> Convert(List<Dictionary<string, string>> exceptions)
        {
            List<string> result = new List<string>(exceptions.Count);
            foreach(var ex in exceptions)
            {
                result.Add(string.Join(";", ex));
            }
            return result;
        }

        private static void Notify(IConfiguration configuration, string content)
        {
            if (configuration == null) return;

            IConfiguration emailSettings = configuration.GetSection("Email");
            if (emailSettings != null)
            {
                Console.WriteLine("Email has been configured for Notifiers");
                Notifiers.Sender sender = new Notifiers.Sender(
                    emailSettings.GetValue("Name", "CRM Service Account"),
                    emailSettings["Username"],
                    emailSettings["Password"]
                    );
                Console.WriteLine("Display Name: {0}, Sender Address: {1}, Password(short): {2}...", sender.Name, sender.Address, sender.Password.Substring(0, 2));
                List<string> receivers = emailSettings.GetSection("Receivers").Get<List<string>>();
                foreach (var s in receivers)
                {
                    Console.WriteLine(s);
                }
                Notifiers.EMailer mailer = new Notifiers.EMailer(emailSettings["Server"], emailSettings.GetValue<int>("Port"), sender);

                DateTime current = DateTime.Now;
                TimeZoneInfo localZone = TimeZoneInfo.Local;  // hard-coded to Adelaide time zone
                if (localZone.Id != "Cen. Australia Standard Time")
                {
                    current = TimeZoneInfo.ConvertTime(current, TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time"));
                }
                mailer.Send(receivers, string.Format("CRMHelpers messages on {0}", current), content);
            }
            else
            {
                Console.WriteLine("No email has been configured for Notifiers");
            }

            // If there are more, set up and run here
        }

        /// <summary>
        /// Demo of how to create and use AccountIDManager
        /// Used for getting parentaccountid when creating Contact
        /// </summary>
        /// <returns></returns>
        private async Task GetAccounts()
        {
            Client.Entities.Account account = new Client.Entities.Account(_crmClient);
            List<Client.Types.Account> accounts = await account.List();
            foreach (var acc in accounts)
            {
                Console.WriteLine("{0} {1} {2}", acc.ID, acc.Name, acc.ParentAccountID);
            }
            Dictionary<string, string> accountDict = Utils.FromListToDict(accounts, "Name", "ID");
            Console.WriteLine("Total count = {0}", accountDict.Count);
            foreach (var key in accountDict.Keys)
            {
                Console.WriteLine("{0} {1}", key, accountDict[key]);
            }
            Client.AccountIDManager manager = new AccountIDManager(accounts);
            Console.WriteLine(manager.GetAccountID("University of Adelaide", "School of Agriculture, Food & Wine"));
        }

        private static void PrintUser(Dictionary<string, string>user)
        {
            foreach (KeyValuePair<string, string> kvp in user)
            {
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
            }
        }

        private static void PrintUser(User user)
        {
            Console.WriteLine("Account: {0}, Name: {1} {2}, Email: {3}, Company: {4}, Department: {5}, DN: {6}", 
                user.AccountName,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Company,
                user.Department,
                user.DistinguishedName
                );
        }

        //private static async Task DemoADAsync(IConfiguration configuration)
        //{
        //    CRMClient _crmClient;

        //    async Task CheckAndPrint(Dictionary<string, string> user)
        //    {
        //        Client.Types.Contact result = await _crmClient.GetContact(user["mail"]);
        //        if (result != null)
        //        {
        //            Console.WriteLine($"Found and the contact id = {result.Id}");
        //            string contactID = result.Id;
        //            result = await _crmClient.GetContact(new Guid(contactID));
        //            if (string.IsNullOrEmpty(result.Username))
        //            {
        //                Console.WriteLine($"Username needed to be updated for contact {contactID}");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"contact {contactID} has username of {result.Username}");
        //            }
        //        }
        //        else
        //        {
        //            PrintUser(user);
        //            Console.WriteLine("Not found");
        //        }
        //        Console.WriteLine();
        //    }

        //    _crmClient = new CRMClient(configuration["Dynamics:Authority"], configuration["Dynamics:Resource"],
        //        configuration["Dynamics:ClientId"], configuration["Dynamics:ClientSecret"], configuration["Dynamics:Version"]);

        //    DateTime earliest = new DateTime(2017, 11, 1);
        //    using (IADSearcher ad = Creater.GetADConnector(configuration.GetSection("AD")))
        //    {
        //        List<Dictionary<string, string>> results = ad.Search(earliest);
        //        Console.WriteLine(results.Count);

        //        Task[] taskArray = new Task[results.Count];
        //        int i = 1;
        //        foreach (var user in results)
        //        {
        //            Console.WriteLine("task {0}", i);
        //            taskArray[i++] = await Task.Factory.StartNew(() => CheckAndPrint(user));
        //        }
        //        Task.WaitAll(taskArray);
        //    };
        //}

        /// <summary>
        /// Check a list of AD users if they exist in CRM, then try to create as new Contacts or update username of Contacts.
        /// </summary>
        /// <param name="newUsers"></param>
        /// <returns></returns>
        private async Task<(List<string>, List<string>, List<Dictionary<string, string>>)> CheckUserInCRM(List<User> newUsers)
        {
            Client.Entities.Account account = new Client.Entities.Account(_crmClient);
            Client.Entities.Contact contact = new Client.Entities.Contact(_crmClient);

            AccountIDManager manager = await account.GetAccountIDManager();

            List<string> createdUsers = new List<string>();
            List<string> unabledUsers = new List<string>();
            List<Dictionary<string, string>> exceptionUsers = new List<Dictionary<string, string>>();

            JsonObject ConvertADUserToJson(User user, string attachToAccountID)
            {
                return Client.Types.ContactBase.Create(user.FirstName, user.LastName, user.Email, user.AccountName, user.Department, attachToAccountID);
            }

            async Task CheckAndPrint(User user)
            {
                PrintUser(user);
                Client.Types.Contact result = await contact.GetByEmail(user.Email);
                if (result != null)
                {
                    Console.WriteLine($"Found and the contact id = {result.ID}");
                    string contactID = result.ID;
                    result = await contact.Get(new Guid(contactID));
                    if (string.IsNullOrEmpty(result.Username))
                    {
                        Console.WriteLine($"Username needed to be updated for contact {contactID}");
                        await contact.UpdateUsername(contactID, user.AccountName);
                        Console.WriteLine("\tUpdated was successful");
                    }
                    else
                    {
                        if (result.Username == user.AccountName)
                        {
                            Console.WriteLine($"contact {contactID} has username of {result.Username}");
                        } else
                        {
                            Console.WriteLine($"contact {contactID} has username of {result.Username} which is different to samAccountName {user.AccountName}");
                            await contact.UpdateUsername(contactID, user.AccountName);
                            Console.WriteLine("\tUpdated with new value was successful");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not found in CRM");
                    // TODO: Use either department, company or part of dn to find out which Account this contact should be attached to.
                    // await contact.Create(ConvertADUserToJson(user));
                    string accountID = manager.GetAccountID(user.Company, user.Department);
                    if (string.IsNullOrEmpty(accountID)) {
                        Console.WriteLine("\tCannot create because cannot find accountID to attach.");
                        unabledUsers.Add(user.DistinguishedName);
                    } else
                    {
                        Console.WriteLine($"\tCreate contact for {accountID}");
                        await contact.Create<JsonObject>(ConvertADUserToJson(user, accountID));
                        createdUsers.Add(user.DistinguishedName);
                    }
                }
            }

            Console.WriteLine(newUsers.Count);
            int i = 1;
            foreach (var user in newUsers)
            {
                Console.WriteLine("User {0}", i++);
                try
                {
                    await CheckAndPrint(user);
                } catch (Exception ex)
                {
                    exceptionUsers.Add(new Dictionary<string, string>
                    {
                        { "DistinguishedName", user.DistinguishedName},
                        { "Exception", ex.ToString() }
                    });
                    Console.WriteLine("Failed attempt. Send info to someone");
                    Console.WriteLine(ex.ToString());
                }
                Console.WriteLine();
            }
            return (createdUsers, unabledUsers, exceptionUsers);
        }

        /// <summary>
        /// Get a list of ADConnectors.User
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="earliest"></param>
        /// <returns></returns>
        private static (List<User>, List<Dictionary<string, string>>) CheckNewAccount(IConfiguration configuration, DateTime earliest)
        {
            List<User> users = new List<User>();
            List<Dictionary<string, string>> incompletedUsers = new List<Dictionary<string, string>>();
            using (IADSearcher ad = Creater.GetADConnector(configuration.GetSection("AD")))
            {
                List<Dictionary<string, string>> results = ad.Search(earliest);
                Console.WriteLine(results.Count);

                int i = 1;
                foreach (var user in results)
                {
                    Console.WriteLine("User {0}: {1} ({2} {3})", i++, user["samaccountname"], user["givenname"], user["sn"]);
                    try
                    {
                        users.Add(new User(user));
                    }
                    catch (KeyNotFoundException)
                    {
                        incompletedUsers.Add(user);
                    }
                }
            };
            return (users, incompletedUsers);
        }

    }
}
