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
            commonFileds = new string[] { "salesorderdetailid", "_productid_value", "priceperunit", "quantity", "manualdiscountamount_base", "producttypecode", "_uomid_value" };
        }

        #region private queries
        // fetchXml version for getting ordered products
        // TODO: to be removed
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
        /// <summary>
        /// Get order lines of an Order
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        private string GetOderDetailsQuery(Guid orderID)
        {
            return Query.Build(commonFileds, $"_salesorderid_value eq {orderID.ToString()}");
        }

        /// <summary>
        /// Get product properties of a order line product
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        /*
         * https://ersasandbox.crm6.dynamics.com/api/data/v8.2/salesorderdetails(be5aa1c1-8ebe-e711-8133-70106fa3d971)/Microsoft.Dynamics.CRM.RetrieveProductProperties()
         * {
              "@odata.context":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/$metadata#dynamicproperties","value":[
                {
                  "dynamicpropertyid":"4401a576-abb3-e711-8132-70106fa3d971","ishidden":false,"datatype":3,"rootdynamicpropertyid":"4401a576-abb3-e711-8132-70106fa3d971","statecode":0,"statuscode":1,"_regardingobjectid_value":"ed6503ca-aab3-e711-8132-70106fa3d971","maxlengthstring":100,"name":"Openstack / Project ID","isrequired":true,"isreadonly":false,"description":"System identifier from Nectar or TANGO"
                },{
                  "dynamicpropertyid":"2d448435-abb3-e711-8132-70106fa3d971","ishidden":false,"datatype":0,"rootdynamicpropertyid":"2d448435-abb3-e711-8132-70106fa3d971","statecode":0,"statuscode":1,"_regardingobjectid_value":"ed6503ca-aab3-e711-8132-70106fa3d971","maxlengthstring":null,"name":"Operating System","isrequired":true,"isreadonly":false,"description":null
                },{
                  "dynamicpropertyid":"efc268b7-abb3-e711-8132-70106fa3d971","ishidden":false,"datatype":4,"rootdynamicpropertyid":"efc268b7-abb3-e711-8132-70106fa3d971","statecode":0,"statuscode":1,"_regardingobjectid_value":"ed6503ca-aab3-e711-8132-70106fa3d971","maxlengthstring":null,"name":"RAM","isrequired":true,"isreadonly":false,"description":"Allocated Memory per Core"
                }
              ]
            }
         */
        private string GetProductPropDef(Guid ID)
        {
            return $"({ID.ToString()})/Microsoft.Dynamics.CRM.RetrieveProductProperties()";
        }

        // Get orderline and property instances
        // no optional set
        // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/salesorderdetails(be5aa1c1-8ebe-e711-8133-70106fa3d971)?$expand=SalesOrderDetail_Dynamicpropertyinstance
        // from dynamicpropertyinstance
        // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/dynamicpropertyinstances?$filter=_regardingobjectid_value%20eq%20be5aa1c1-8ebe-e711-8133-70106fa3d971&$select=valuestring,valuedecimal,valuedouble,valueinteger
        private string GetProductPropValues(Guid ID)
        {
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$select", Query.CreateList(commonFileds) },
                { "$expand", "SalesOrderDetail_Dynamicpropertyinstance($select=valuestring,valuedecimal,valuedouble,valueinteger,_dynamicpropertyid_value)" }
            };
            return $"({ID.ToString()})" + Query.Build(parts);
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

        // Get a list of product property definitions of an orderline.
        // Not quite useful as itself
        public Task<List<Types.Dynamicproperty>> GetProductProperties(Guid ID)
        {
            return GetEntityAsync<List<Types.Dynamicproperty>>(GetProductPropDef(ID));
        }
        #endregion
    }
}
