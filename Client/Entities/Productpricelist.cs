using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class ProductPricelist : Base
    {
        public const string ENTITY = "productpricelevel";

        public ProductPricelist(CRMClient conn) : base(conn)
        {
            ENDPOINT = "productpricelevels";

            commonFileds = new string[] { "productnumber", "amount",
                 "_productid_value", "_pricelevelid_value", "_uomid_value",
                "percentage", "pricingmethodcode", "quantitysellingcode",
                "roundingoptionamount", "roundingoptioncode", "roundingpolicycode"};
        }
    }
}
