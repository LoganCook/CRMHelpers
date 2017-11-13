using System;
using System.Collections.Generic;
using System.Json;
using Xunit;

namespace tests
{
    public class Utils
    {
        [Theory]
        [InlineData(4, 3)]
        public void CanCreateJsonArrayString(int entries, int pairs)
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
            for(int i = 0; i < entries; i++)
            {
                Dictionary<string, string> entry = new Dictionary<string, string>();
                for (int j = 0; j < pairs; j++)
                {
                    entry[String.Format("key{0}{1}", i, j)] = String.Format("value{0}{1}", i, j);
                }
                data.Add(entry);
            }

            string json = Synchroniser.Utils.GetJSArray(data);
            JsonArray jsArray = (JsonArray) JsonValue.Parse(json);
            Assert.Equal(data.Count, jsArray.Count);
            Assert.Equal(data.Count, entries);
            Assert.Equal(data[entries - 1].Count, jsArray[entries - 1].Count);
            Assert.Equal(data[entries - 1].Count, pairs);
        }

        [Theory]
        [InlineData("https://crm6.dynamics.com/api/data/v8.2/salesorders(be536bbf-baaf-e711-8154-e0071b684991)")]
        [InlineData("https://salesorders(be536bbf-baaf-e711-8154-e0071b684991)")]
        public void CanGetIdFromUrl(string url)
        {
            string id = Synchroniser.Utils.GetIdFromUrl(url);
            Assert.Equal(36, id.Length);
        }

        [Fact]
        public void AccountsToDictionay()
        {
            const int itemCount = 5;
            List<Client.Types.Account> accounts = new List<Client.Types.Account>();
            string[] keys = new string[itemCount];
            string[] values = new string[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                //keys.SetValue("name" + i, i);
                keys[i] = "name" + i;
                values[i] = "id" + i;
                Client.Types.Account newAccount = new Client.Types.Account
                {
                    ID = values[i],
                    Name = keys[i]
                };
                accounts.Add(newAccount);
            }
            var dict = Client.Utils.FromListToDict(accounts, "Name", "ID");
            Assert.Equal(itemCount, dict.Count);
            for (int i = 0; i < itemCount; i++)
            {
                Assert.True(dict.ContainsKey(keys[i]));
                Assert.Equal(dict[keys[i]], values[i]);
            }
        }

        [Theory]
        [InlineData(3)]
        public void AccountManager(int count)
        {
            List<Client.Types.Account> accounts = new List<Client.Types.Account>();
            string parentname;
            int i, j;
            for (i = 0; i < count; i++)
            {
                parentname = "top" + i;
                accounts.Add(new Client.Types.Account { ID = i.ToString(), Name = parentname });
                // unique children
                for (j = 0; j < count; j++)
                {
                    accounts.Add(new Client.Types.Account { ID = i.ToString() + j.ToString(), Name = "top" + i + "_sub" + j, ParentAccountID = i.ToString() });
                }
                // common name between parent
                accounts.Add(new Client.Types.Account { ID = i.ToString() + (count + 1).ToString(), Name = "shared", ParentAccountID = i.ToString() });
            }

            Client.AccountIDManager manager = new Client.AccountIDManager(accounts);

            Random rnd = new Random();
            string top = rnd.Next(count).ToString(), sub = rnd.Next(count).ToString();
            Assert.Equal(top + sub, manager.GetAccountID("top" + top, "top" + top + "_sub" + sub));
            Assert.Equal(top + (count + 1).ToString(), manager.GetAccountID("top" + top, "shared"));
        }
    }
}
