using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    // result of salesorderdetails(be5aa1c1-8ebe-e711-8133-70106fa3d971)/Microsoft.Dynamics.CRM.RetrieveProductProperties()
    //    {
    //    "dynamicpropertyid": "4401a576-abb3-e711-8132-70106fa3d971",
    //    "ishidden": false,
    //    "datatype": 3,
    //    "rootdynamicpropertyid": "4401a576-abb3-e711-8132-70106fa3d971",
    //    "statecode": 0,
    //    "statuscode": 1,
    //    "_regardingobjectid_value": "ed6503ca-aab3-e711-8132-70106fa3d971",
    //    "maxlengthstring": 100,
    //    "name": "Openstack / Project ID",
    //    "isrequired": true,
    //    "isreadonly": false,
    //    "description": "System identifier from Nectar or TANGO"
    //}
    [DataContract]
    public class Dynamicproperty
    {
        static Dictionary<short, string> States = new Dictionary<short, string>
        {
            { 0, "Active"},
            { 1, "Draft"},
            { 2, "Retired"}
        };

        static Dictionary<short, string> Types = new Dictionary<short, string>
        {
            { 0, "Option Set" },
            { 1, "Decimal" },
            { 2, "Floating Point Number" },
            { 3, "Single Line Of Text" },
            { 4, "Whole Number" }
        };

        [DataMember(Name = "dynamicpropertyid")]
        public string ID { set; get; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "description")]
        public string Description { set; get; }

        private short dataType;
        [DataMember(Name = "datatype")]
        public string Type
        {
            set { dataType = short.Parse(value); }
            get
            {
                return Types[dataType];
            }
        }

        [DataMember(Name = "isrequired")]
        public bool IsRequired { set; get; }

        private short statecode;
        [DataMember(Name = "statecode", EmitDefaultValue = false)]
        public string Status
        {
            set { statecode = short.Parse(value); }
            get
            {
                return States[statecode];
            }
        }

        [DataMember(Name = "_regardingobjectid_value")]
        public string ProductID { set; get; }

        [DataMember(Name = "_regardingobjectid_value@OData.Community.Display.V1.FormattedValue")]
        public string Product { set; get; }

    }
}
