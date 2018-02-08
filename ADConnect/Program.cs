using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ADConnectors
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("adconnectors.log")
                .CreateLogger();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("ad_connection.json")
                .Build();

            DateTime earliest = new DateTime(2017, 9, 1);
            using (IADSearcher ad = Creater.GetADConnector(configuration)) {
                List<Dictionary<string, string>> results = ad.Search(earliest);
                Console.WriteLine(results.Count);
                Dictionary<string, string> lastAccount = ad.GetUser(int.Parse(results[results.Count - 1]["uidnumber"]), true);
                Console.WriteLine("Searching non existing account resulting zero number of key:");
                Dictionary<string, string> nonExisting = ad.GetUser(1089);
                Console.WriteLine(nonExisting == null);
            };
        }
    }
}
