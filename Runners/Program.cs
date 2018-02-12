using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Json;
using Serilog;
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
                configuration["Dynamics:ClientId"], configuration["Dynamics:ClientSecret"], configuration["Dynamics:Version"],
                configuration["Dynamics:TokenCache"]);
        }

        /// <summary>
        /// Main with all optional arguments in such order:
        /// 1. earliest date to query from for new AD accounts in the format of yyyyMMdd
        /// 2. configuration JSON file path related to current path
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            IConfiguration logConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("log.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(logConfig)
                .CreateLogger();

            var default_args = new Dictionary<string, string>
            {
                {"config", "secrets.json"},
                {"earliest", new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture)}
            };

            IConfiguration switchs = new ConfigurationBuilder()
                .AddInMemoryCollection(default_args)
                .AddCommandLine(args)
                .Build();

            DateTime earliest;
            try
            {
                earliest = DateTime.ParseExact(switchs["earliest"], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("{0} is in wrong DateTime format. It should be in yyyyMMdd format.", switchs["earliest"]);
                Log.Error("{0} is in wrong DateTime format. It should be in yyyyMMdd format.", switchs["earliest"]);
                return;
            }

            string config = switchs["config"];
            if (!File.Exists(config))
            {
                Console.WriteLine($"Configuration file {config} does not exist");
                Log.Error($"Configuration file {config} does not exist");
            };

            Console.WriteLine("Search AD accounts created since {0} with configuration file in {1}", switchs["earliest"], switchs["config"]);
            Log.Information("Search AD accounts created since {0} with configuration file in {1}", switchs["earliest"], switchs["config"]);

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(config)
                .Build();

            (List<User> users, List<Dictionary<string, string>> incompleteUsers) = CheckNewAccount(configuration, earliest);

            if (users.Count == 0)
            {
                Console.WriteLine($"No new accounts found for the period started from {earliest}");
                Log.Information($"No new accounts found for the period started from {earliest}");
                return;
            }

            Program program = new Program(configuration);
            (List<string> createdUsers, List<string> unabledUsers, List<Dictionary<string, string>> exceptionUsers) = await program.CheckUserInCRM(users);

            if (incompleteUsers.Count > 0 || unabledUsers.Count > 0 || exceptionUsers.Count > 0 || createdUsers.Count > 0)
            {
                string message = ListsToMessage(incompleteUsers, unabledUsers, exceptionUsers, createdUsers);
                NotifyByEmail(configuration.GetSection("Notifiers"), message);
            }

            Log.CloseAndFlush();
        }

        /// <summary>
        /// Get a list of ADConnectors.User created since a give date
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
                Log.Information($"Total user found: {results.Count}");

                int i = 0;
                foreach (var user in results)
                {
                    i++;
                    Console.WriteLine("User {0}: {1} ({2} {3})", i, user["samaccountname"], user["givenname"], user["sn"]);
                    Log.Debug("User {0}: {1} ({2} {3})", i, user["samaccountname"], user["givenname"], user["sn"]);
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

        /// <summary>
        /// Check AD user from a list if they exist in CRM as Contact, then try to create if they don't or update username if it is incorrect.
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

            // Check the existence of a user by email address
            async Task CheckAndPrint(User user)
            {
                PrintUser(user);  // FIXME: this may not work well in parallel by just print user details
                Client.Types.Contact result = await contact.GetByEmail(user.Email);
                if (result != null)
                {
                    // Contact exists
                    Console.WriteLine($"Found and the contact id = {result.ID}");
                    Log.Information($"{user.DistinguishedName}: found in CRM and the contact id = {result.ID}");
                    string contactID = result.ID;
                    result = await contact.Get(new Guid(contactID));
                    // No username, need to set
                    if (string.IsNullOrEmpty(result.Username))
                    {
                        Log.Debug($"User name needed to be updated for contact {contactID}");
                        await contact.UpdateUsername(contactID, user.AccountName);
                        Log.Information($"The username of Contact with id={contactID} has been set successfully.");
                    }
                    else
                    {
                        // found wrong username
                        if (result.Username == user.AccountName)
                        {
                            Log.Debug($"contact {contactID} has username of {result.Username}");
                        } else
                        {
                            Log.Debug($"Contact {contactID} has username of {result.Username} which is different to samAccountName {user.AccountName}");
                            await contact.UpdateUsername(contactID, user.AccountName);
                            Log.Information($"The username of Contact with id={contactID} has been updated successfully.");
                        }
                    }
                }
                else
                {
                    // Contact does not exist
                    Log.Debug($"{user.DistinguishedName} was not found in CRM");
                    string accountID = manager.GetAccountID(user.Company, user.Department);
                    if (string.IsNullOrEmpty(accountID)) {
                        Log.Warning($"Cannot create new Contact because cannot find accountID to attach for {user.DistinguishedName} could because of wrong department.");
                        unabledUsers.Add(string.Format("{0} ({1})", user.DistinguishedName, user.Email));
                    } else
                    {
                        Log.Debug($"New contact will be created for {user.DistinguishedName}");
                        await contact.Create<JsonObject>(ConvertADUserToJson(user, accountID));
                        createdUsers.Add(user.DistinguishedName);
                        Log.Information($"New contact has been created for {user.DistinguishedName}");
                    }
                }
            }

            Task[] taskArray = new Task[newUsers.Count];
            int i = 0;
            foreach (var user in newUsers)
            {
                Log.Information("task {0}", i + 1);
                try
                {
                    taskArray[i++] = await Task.Factory.StartNew(() => CheckAndPrint(user));
                } catch (Exception ex)
                {
                    exceptionUsers.Add(new Dictionary<string, string>
                    {
                        { "DistinguishedName", user.DistinguishedName},
                        { "Exception", ex.ToString() }
                    });
                    Log.Error(ex, $"Failed attempt when processing {user.DistinguishedName}.");
                }
            }
            Task.WaitAll(taskArray);

            return (createdUsers, unabledUsers, exceptionUsers);
        }


        /// <summary>
        /// Notify recipients of content var Email Notifier
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="content"></param>
        private static void NotifyByEmail(IConfiguration configuration, string content)
        {
            if (configuration == null) return;

            // Use Email Notifier
            IConfiguration emailSettings = configuration.GetSection("Email");
            if (emailSettings == null)
            {
                Log.Error("No email has been configured for Notifiers");
                return;
            }

            Console.WriteLine("Email has been configured for Notifiers");
            Log.Debug("Email has been configured for Notifiers");
            Notifiers.Sender sender = new Notifiers.Sender(
                emailSettings.GetValue("Name", "CRM Service Account"),
                emailSettings["Username"],
                emailSettings["Password"]
                );
            Console.WriteLine("Display Name: {0}, Sender Address: {1}, Password(short): {2}...", sender.Name, sender.Address, sender.Password.Substring(0, 2));
            Log.Debug("Display Name: {0}, Sender Address: {1}, Password(short): {2}...", sender.Name, sender.Address, sender.Password.Substring(0, 2));
            List<string> receivers = emailSettings.GetSection("Receivers").Get<List<string>>();
            foreach (var receiver in receivers)
            {
                Console.WriteLine(receiver);
                Log.Debug(receiver);
            }
            Notifiers.EMailer mailer = new Notifiers.EMailer(emailSettings["Server"], emailSettings.GetValue<int>("Port"), sender);

            DateTime current = DateTime.Now;
            TimeZoneInfo localZone = TimeZoneInfo.Local;  // hard-coded to Adelaide time zone
            if (localZone.Id != "Cen. Australia Standard Time" || localZone.Id != "Australia/Adelaide")
            {
                try
                {
                    current = TimeZoneInfo.ConvertTime(current, TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time"));
                }
                catch (TimeZoneNotFoundException)
                {
                    // On Linux, timezone ids are different: https://github.com/dotnet/corefx/issues/11897
                    current = TimeZoneInfo.ConvertTime(current, TimeZoneInfo.FindSystemTimeZoneById("Australia/Adelaide"));
                }
            }
            mailer.Send(receivers, string.Format("CRMHelpers messages on {0}", current), content);
        }

        #region utility functions
        private static void PrintUser(Dictionary<string, string> user)
        {
            foreach (KeyValuePair<string, string> kvp in user)
            {
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
                Log.Debug("{0}: {1}", kvp.Key, kvp.Value);
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
            Log.Debug("Account: {0}, Name: {1} {2}, Email: {3}, Company: {4}, Department: {5}, DN: {6}",
                user.AccountName,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Company,
                user.Department,
                user.DistinguishedName
                );
        }


        /// <summary>
        /// Append stings in a list to a StringBuilder instance with optional section title
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="strings"></param>
        /// <param name="title"></param>
        private static void BuildSection(System.Text.StringBuilder sb, List<string> strings, string title = "")
        {
            if (strings.Count > 0)
            {
                sb.AppendLine(title);
                foreach (string str in strings)
                {
                    sb.AppendLine(str);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
        }

        /// <summary>
        /// Convert a list of dictionary to a list of string of ';' delimiter
        /// with the content of each dictionary.ToString()
        /// </summary>
        /// <param name="dicts"></param>
        /// <returns></returns>
        private static List<string> Convert(List<Dictionary<string, string>> dicts)
        {
            List<string> result = new List<string>(dicts.Count);
            foreach (var dict in dicts)
            {
                result.Add(string.Join(";", dict));
            }
            return result;
        }

        /// <summary>
        /// Turn lists of users of incompleted: lack of critical information,
        /// unabled: missing Account information to create Contact
        /// exceptional: tried but failed
        /// and created users into a string
        /// </summary>
        /// <param name="incompleteUsers"></param>
        /// <param name="unabledUsers"></param>
        /// <param name="exceptionUsers"></param>
        /// <param name="createdUsers"></param>
        /// <returns></returns>
        private static string ListsToMessage(List<Dictionary<string, string>> incompleteUsers, List<string> unabledUsers, List<Dictionary<string, string>> exceptionUsers, List<string> createdUsers)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder
            {
                Capacity = 200
            };

            if (incompleteUsers.Count > 0)
            {
                Console.WriteLine("Incomplete user - missing key fields.");
                Log.Information("Incomplete user - missing key fields.");
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
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// Demo of how to create and use AccountIDManager
        /// Used for getting parentaccountid when creating Contact
        /// </summary>
        /// <returns></returns>
        private async Task DemoAccountIDManager()
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
    }
}
