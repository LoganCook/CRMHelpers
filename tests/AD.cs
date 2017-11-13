using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ADConnectors;

namespace tests
{
    public class AD
    {
        [Fact]
        public void UserCanLoadDict()
        {
            Dictionary<string, string> userDict = new Dictionary<string, string>
            {
                ["givenname"] = "FirstName",
                ["sn"] = "LastName",
                ["mail"] = "Email",
                ["distinguishedname"] = "DistinguishedName",
                ["samaccountname"] = "AccountName",
                ["company"] = "Company",
                ["department"] = "Department"
            };
            User userInstance = new User(userDict);
            Type userClassType = typeof(User);
            foreach(string property in userDict.Keys)
            {
                Assert.Equal(userDict[property], userClassType.GetProperty(userDict[property]).GetValue(userInstance));
            }
        }

        [Fact]
        public void WrongDictThrow()
        {
            Dictionary<string, string> incomplete = new Dictionary<string, string>
            {
                ["givenname"] = "FirstName",
                ["mail"] = "Email"
            };
            Assert.Throws<KeyNotFoundException>(() => new User(incomplete));
        }
    }
}
