using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class Product : Base
    {
        public const string ENTITY = "product";

        public Product(CRMClient conn) : base(conn)
        {
            ENDPOINT = "products";
            commonFileds = new string[] { "name", "productnumber", "description", "hierarchypath", "validfromdate", "validtodate", "productstructure", "statecode",
                "_pricelevelid_value", "_subjectid_value", "_defaultuomid_value"};
        }

        public Task<Types.Product> GetDetail(Guid id)
        {
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$expand",  "Product_DynamicProperty($select=name,datatype)" }
            };
            return GetEntityAsync<Types.Product>($"({id.ToString()})" + Query.Build(parts));
        }
    }
}
