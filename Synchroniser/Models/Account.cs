using System.Runtime.Serialization;

namespace Synchroniser.Models
{
    [DataContract]
    public class Account
    {
        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "accountid")]
        public string Id { set; get; }

        public static string Get(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            return $"Accounts?$filter=name eq {name}";
        }
    }
}
