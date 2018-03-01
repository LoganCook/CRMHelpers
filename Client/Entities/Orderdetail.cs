using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class Orderdetail : Base
    {
        public const string ENTITY = "salesorderdetail";

        public Orderdetail(CRMClient conn) : base(conn)
        {
            ENDPOINT = "salesorderdetails";
        }

        #region private queries
        // fetchXml version for getting ordered products
        private string GetOderDetails(Guid orderID)
        {
            var xml = new FetchXML(ENTITY);
            FetchElement entity = xml.EntityElement;
            entity.AddFilter()
                .AddCondition("salesorderid", "eq", orderID.ToString());
            entity.AddLinkEntity("product", "productid", "productid").AddField("name", "product");
            return xml.ToQueryString();
        }

        // simple web query as above
        private string GetOderDetailsQuery(Guid orderID)
        {
            return $"?$filter=_salesorderid_value eq {orderID.ToString()}&$select=_productid_value,priceperunit,quantity,manualdiscountamount_base,producttypecode,_uomid_value";
        }

        #endregion

        #region for apis and views
        /// <summary>
        /// Pass the result of a list of ordered products from MS Dynamics directly to caller
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>The result from Dynamics server as string</returns>
        public Task<string> ListOrderedProductsString(Guid orderID)
        {
            return GetJsonStringAsync(GetOderDetailsQuery(orderID));
        }

        /// <summary>
        /// Return a list of ordered products
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public Task<List<Types.OrderdetailSimple>> ListOrderedProducts(Guid orderID)
        {
            return List<Types.OrderdetailSimple>(GetOderDetailsQuery(orderID));
        }
        #endregion
    }
}
