using System;
using System.Linq;
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

        /// <summary>
        /// Get associated products of a product which is either a bundle or kit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<List<Types.ProductAssociation>> GetAssociated(Guid id)
        {
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/productassociations?$filter=_productid_value eq e1724d11-9a21-e811-8131-480fcff12ac1&$expand=associatedproduct
            return new ProductAssociation(Connector).GetBundled(id);
        }

        /// <summary>
        /// Get a Product's details and properties, if it is a family, get all its child products with their details and properties
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Types.Product>> GetFamily(Guid id)
        {
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/products?$select=name,productstructure,_parentproductid_value&$filter=productid eq c3724cbc-b183-e611-80e7-c4346bc4beac or _parentproductid_value eq c3724cbc-b183-e611-80e7-c4346bc4beac&$expand=Product_DynamicProperty($select=name,datatype)
            // Above query returns "Product_DynamicProperty@odata.nextLink":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/products(c3724cbc-b183-e611-80e7-c4346bc4beac)/Product_DynamicProperty?$select=name,datatype"

            string stringID = id.ToString();
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$filter", $"productid eq {stringID} or _parentproductid_value eq {stringID}" },
                { "$expand",  "Product_DynamicProperty($select=name,datatype)" }
            };
            List<Types.Product> products = await List<Types.Product>(Query.Build(parts));

            int i = 0, counts = products.Count;
            Task<List<Types.Dynamicproperty>>[] taskArray = new Task<List<Types.Dynamicproperty>>[counts];

            foreach (var product in products)
            {
                taskArray[i++] = NextLink<Types.Dynamicproperty>(product.PropertiesLink);
            }
            await Task.WhenAll(taskArray.ToArray());
            for (i = 0; i < counts; i++)
            {
                products[i].Properties = taskArray[i].Result;
            }
            return products;
        }
    }
}
