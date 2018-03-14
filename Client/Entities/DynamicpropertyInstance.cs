using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class DynamicpropertyInstance : Base
    {
        public const string ENTITY = "dynamicpropertyinstance";

        public DynamicpropertyInstance(CRMClient conn) : base(conn)
        {
            ENDPOINT = "dynamicpropertyinstances";
            commonFileds = new string[] { "valuestring,valueinteger,valuedecimal,valuedouble" };
        }

        /// <summary>
        /// Get instances of dynamic property of a salesorderdetail
        /// </summary>
        /// <param name="salesOrderDetailID"></param>
        /// <returns></returns>
        private string GetInstancesOfQuery(string salesOrderDetailID)
        {
            // https://ersasandbox.crm6.dynamics.com/api/data/v8.2/dynamicpropertyinstances?$filter=_regardingobjectid_value%20eq%20be5aa1c1-8ebe-e711-8133-70106fa3d971&$select=valuestring,valuedecimal,valuedouble,valueinteger&$expand=dynamicpropertyid($select=name,datatype,dynamicpropertyid)
            Dictionary<string, string> parts = new Dictionary<string, string>
            {
                { "$filter", "_regardingobjectid_value eq " + salesOrderDetailID },
                { "$select", "valuestring,valueinteger,valuedecimal,valuedouble" },
                { "$expand", "dynamicpropertyid($select=dynamicpropertyid,name,datatype)" }
            };
            return Query.Build(parts);
        }

        // Get a list of product property definitions of an orderline.
        // Not quite useful as itself
        public Task<List<Types.DynamicpropertyInstance>> GetProductProperties(Guid ID)
        {
            return List<Types.DynamicpropertyInstance>(GetInstancesOfQuery(ID.ToString()));
        }
    }
}
