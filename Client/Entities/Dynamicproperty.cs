using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class Dynamicproperty : Base
    {
        public const string ENTITY = "dynamicproperty";

        public Dynamicproperty(CRMClient conn) : base(conn)
        {
            ENDPOINT = "dynamicproperties";
            commonFileds = new string[] { "name", "description", "datatype", "statecode", "isrequired", "_regardingobjectid_value", "_defaultvalueoptionset_value"};
        }
    }
}
