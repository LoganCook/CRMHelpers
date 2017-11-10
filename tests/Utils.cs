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
            Assert.Equal(id.Length, 36);
        }
    }
}
