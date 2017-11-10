using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Synchroniser.Models
{
    [DataContract]
    public class Product
    {
        [DataMember(Name = "productid")]
        public string Id { set; get; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "description")]
        public string Description { set; get; }


        public static string Get(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            return $"products?$filter=name eq {name}&$select=productid";
        }

        public static string List()
        {
            return "products?$filter=statecode eq 0 and productstructure eq 1&$select=name,description";
        }
    }
}
