using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    /*
    [
        {
          "valuestring": "ef06028a866941e9857afbad5a3e0c2a",
          "valuedecimal": null,
          "valuedouble": null,
          "valueinteger": null,
          "dynamicpropertyinstanceid": "c05aa1c1-8ebe-e711-8133-70106fa3d971",
          "dynamicpropertyid": {
            "name": "Openstack / Project ID",
            "datatype": 3,
            "dynamicpropertyid": "4401a576-abb3-e711-8132-70106fa3d971"
          }
        },
        {
          "valuestring": null,
          "valuedecimal": null,
          "valuedouble": null,
          "valueinteger": null,
          "dynamicpropertyinstanceid": "c15aa1c1-8ebe-e711-8133-70106fa3d971",
          "dynamicpropertyid": {
            "name": "Operating System",
            "datatype": 0,
            "dynamicpropertyid": "2d448435-abb3-e711-8132-70106fa3d971"
          }
        },
        {
          "valuestring": null,
          "valuedecimal": null,
          "valuedouble": null,
          "valueinteger": 2,
          "dynamicpropertyinstanceid": "c25aa1c1-8ebe-e711-8133-70106fa3d971",
          "dynamicpropertyid": {
            "name": "RAM",
            "datatype": 4,
            "dynamicpropertyid": "efc268b7-abb3-e711-8132-70106fa3d971"
          }
        }
      ]
    */
    [DataContract]
    public class DynamicpropertyInstance
    {
        [DataMember(Name = "valuestring", EmitDefaultValue = false)]
        public string StringValue { set; get; }

        [DataMember(Name = "valuedecimal", EmitDefaultValue = false)]
        public decimal? DecimalValue { set; get; }

        [DataMember(Name = "valuedouble", EmitDefaultValue = false)]
        public double? DoubleValue { set; get; }

        [DataMember(Name = "valueinteger", EmitDefaultValue = false)]
        public int? IntValue { set; get; }

        [DataMember(Name = "dynamicpropertyid", EmitDefaultValue = false)]
        public Dynamicproperty Property;
    }
}
