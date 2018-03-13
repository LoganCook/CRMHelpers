using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    /// <summary>
    /// Basic properties of a price list. ProductPricelist is more useful.
    /// </summary>
    public class Pricelist : Base
    {
        public const string ENTITY = "pricelevel";

        public Pricelist(CRMClient conn) : base(conn)
        {
            ENDPOINT = "pricelevels";
            commonFileds = new string[] { "name", "description", "statecode", "begindate", "enddate" };
        }
    }
}
