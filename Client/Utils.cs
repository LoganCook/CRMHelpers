using System;
using System.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Client
{
    public class Utils
    {
        public static Dictionary<string, string> FromListToDict(List<Types.Account> convertingList, string keyName, string valueName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Type accountType = typeof(Types.Account);
            PropertyInfo[] propertyinfo = accountType.GetProperties();
            System.Diagnostics.Debug.Assert(accountType.GetProperty(keyName) != null);
            System.Diagnostics.Debug.Assert(accountType.GetProperty(valueName) != null);
            foreach (var item in convertingList)
            {
                dictionary[accountType.GetProperty(keyName).GetValue(item).ToString()] = accountType.GetProperty(valueName).GetValue(item).ToString();
            }
            return dictionary;
        }

        /// <summary>
        /// Display details of a http response for deugging
        /// </summary>
        /// <param name="result">HttpResponseMessage</param>
        /// <param name="skipContent">bool</param>
        public static async void DisplayResponse(HttpResponseMessage result, bool skipContent = true)
        {
            Console.WriteLine("Status code: {0}\nHeaders:", result.StatusCode);
            Console.WriteLine(result.Headers);
            string response = await result.Content.ReadAsStringAsync();

            if (!skipContent) Console.WriteLine(response);

            // the error message is a JSON object, code and message of error are important
            JsonObject json = (JsonObject)JsonObject.Parse(response);
            if (json.ContainsKey("error"))
            {
                Console.WriteLine("Error code: {0}", json["error"]["code"]);
                Console.WriteLine("Error message: {0}", json["error"]["message"]);
            }
        }

        /// <summary>
        /// Get the error message from a Json response 
        /// </summary>
        /// <param name="result">HttpResponseMessage</param>
        /// <param name="skipContent">bool</param>
        public static async Task<string> GetErrorMessage(HttpResponseMessage result)
        {
            string response = await result.Content.ReadAsStringAsync();

            // the error message is a JSON object, code and message of error are important
            JsonObject json = (JsonObject)JsonObject.Parse(response);
            if (json.ContainsKey("error"))
            {
                return json["error"]["message"];
            }
            return "";
        }

        public static string GetIdFromUrl(string url)
        {
            Match m = Regex.Match(url, @"^https://.+\(([\w-]+)\)$");
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            return "";
        }
    }

    /// <summary>
    /// Manage secondary level account from the data of a three-column table of name, id, parentid
    /// Privide a way to get an id of a secondary level account
    /// </summary>
    public class AccountIDManager
    {
        Dictionary<string, Dictionary<string, string>> manager = new Dictionary<string, Dictionary<string, string>>();
        // tops: a helper for creating manger dictionary
        Dictionary<string, string> tops = new Dictionary<string, string>();

        public AccountIDManager(List<Types.Account> accounts)
        {
            foreach (var account in accounts)
            {
                // Run this first as the order of Accounts is not guaranteed
                FilterTopAccounts(accounts);
                if (!string.IsNullOrEmpty(account.ParentAccountID))
                {
                    manager[tops[account.ParentAccountID]].Add(account.Name, account.ID);
                }
            }
        }

        private void FilterTopAccounts(List<Types.Account> accounts)
        {
            foreach (var account in accounts)
            {
                if (string.IsNullOrEmpty(account.ParentAccountID))
                {
                    // top account, no parent
                    if (!manager.ContainsKey(account.Name))
                    {
                        tops.Add(account.ID, account.Name);
                        manager.Add(account.Name, new Dictionary<string, string>());
                    }
                }
            }
        }

        /// <summary>
        /// Get Account ID of a second level account by its name and parent name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetAccountID(string parent, string name)
        {
            if (manager.ContainsKey(parent) && manager[parent].ContainsKey(name))
                return manager[parent][name];
            return "";
        }
    }
}
