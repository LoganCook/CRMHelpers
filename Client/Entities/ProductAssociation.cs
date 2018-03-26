using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    /// <summary>
    /// Entity for linking products to a product in a bundle
    /// </summary>
    public class ProductAssociation : Base
    {
        public const string ENTITY = "productassociation";

        public ProductAssociation(CRMClient conn) : base(conn)
        {
            ENDPOINT = "productassociations";
            commonFileds = new string[] { "productassociationid,quantity,productisrequired,statecode" };
        }

        public Task<List<Types.ProductAssociation>> GetBundled(Guid id)
        {
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/productassociations?$filter=_productid_value eq e1724d11-9a21-e811-8131-480fcff12ac1&$expand=associatedproduct
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$filter", $"_productid_value eq {id.ToString()}" },
                { "$expand",  "productid($select=productid,productnumber,name,description),associatedproduct($select=productid,productnumber,name,description)" }
            };
            return List<Types.ProductAssociation>(Query.Build(parts));
        }
    }
}
