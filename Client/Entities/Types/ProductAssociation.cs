using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    [DataContract]
    public class ProductAssociation
    {
        static Dictionary<short, string> RequiredValues = new Dictionary<short, string>
        {
            { 0, "Optional"},
            { 1, "Required"}
        };

        [DataMember(Name = "productassociationid")]
        public string ID { set; get; }

        [DataMember(Name = "quantity")]
        public float Quantity { set; get; }

        private short statecode;
        [DataMember(Name = "statecode", EmitDefaultValue = false)]
        public string Status
        {
            set { statecode = short.Parse(value); }
            get
            {
                return Maps.States[statecode];
            }
        }

        short isRequired;
        [DataMember(Name = "productisrequired")]
        public string Required {
            set { isRequired = short.Parse(value); }
            get { return RequiredValues[isRequired]; }
        }

        [DataMember(Name = "productid", EmitDefaultValue = false)]
        public Product Bundle { set; get; }

        [DataMember(Name = "associatedproduct", EmitDefaultValue = false)]
        public Product Association { set; get; }
    }
}
