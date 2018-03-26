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
            // FIXME: if this is a bundle, it does not have information of bundled products
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/products?$select=name,productstructure,_parentproductid_value&$filter=productid eq c3724cbc-b183-e611-80e7-c4346bc4beac or _parentproductid_value eq c3724cbc-b183-e611-80e7-c4346bc4beac&$expand=Product_DynamicProperty($select=name,datatype)
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$expand",  "Product_DynamicProperty($select=name,datatype)" }
            };
            return GetEntityAsync<Types.Product>($"({id.ToString()})" + Query.Build(parts));
        }

        /// <summary>
        /// Get a Product's details and properties, if it is a bundle, get all its bundled products with their details and properties
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Types.Product>> GetDetailBundleOrNot(Guid id)
        {
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/products?$select=name,productstructure,_parentproductid_value&$filter=productid eq c3724cbc-b183-e611-80e7-c4346bc4beac or _parentproductid_value eq c3724cbc-b183-e611-80e7-c4346bc4beac&$expand=Product_DynamicProperty($select=name,datatype)
            // Above query returns "Product_DynamicProperty@odata.nextLink":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/products(c3724cbc-b183-e611-80e7-c4346bc4beac)/Product_DynamicProperty?$select=name,datatype"
            // for each product. Often a product is not bundle type, so, do normal
            //Types.Product product = await GetDetail(id);
            //List<Types.Product> products = new List<Types.Product>();
            //if (product.Type == "Product Bundle")
            //{
            //    products.Add(product);
            //} else
            //{

            //}
            string stringID = id.ToString();
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$filter", $"productid eq {stringID} or _parentproductid_value eq {stringID}" },
                { "$expand",  "Product_DynamicProperty($select=name,datatype)" }
            };
            List<Types.Product> products = await List<Types.Product>(Query.Build(parts));

            int counts = products.Count, i;
            //Dictionary<string, Task<List<Dynamicproperty>>> trackers = new Dictionary<string, Task<List<Dynamicproperty>>>();
            //var taskArray = new Task<List<Types.Dynamicproperty>>[counts];  // Did not work. All examples are using List
            List<Task<List<Types.Dynamicproperty>>> tasks = new List<Task<List<Types.Dynamicproperty>>>();

            //for (i = 0; i < counts; i++)
            //{
            //    tasks.Add(Task.Run(async () => { var result = await NextLink<Types.Dynamicproperty>(products[i].PropertiesLink); return result; }));
            //}
            //Task.WaitAll(tasks.ToArray());
            //for (i = 0; i < counts; i++)
            //{
            //    products[i].Properties = tasks[i].Result;
            //}
            //Task.WaitAll(trackers.Values.ToArray<Task>());

            foreach (var product in products)
            {
                product.Properties = await NextLink<Types.Dynamicproperty>(product.PropertiesLink);
            }
            return products;
        }
    }
}
