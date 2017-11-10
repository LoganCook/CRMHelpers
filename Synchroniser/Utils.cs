using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;

namespace Synchroniser
{
    public class Utils
    {
        public static string GetJSArray(List<Dictionary<string, string>> list)
        {
            if (list == null) return "[]";
            JsonArray jsArray = new JsonArray();
            foreach (var user in list)
            {
                JsonValue entry = new JsonObject();
                foreach (KeyValuePair<string, string> kvp in user)
                {
                    entry[kvp.Key] = kvp.Value;
                }
                jsArray.Add(entry);
            }
            return jsArray.ToString();
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
}
