using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    [DataContract]
    public class Product
    {
        static Dictionary<short, string> States = new Dictionary<short, string>
        {
            { 0, "Active"},
            { 1, "Retired"},
            { 2, "Draft"},
            { 3, "Under Revision"}
        };

        static Dictionary<short, string> Structures = new Dictionary<short, string>
        {
            { 1, "Product"},
            { 2, "Product Family"},
            { 3, "Product Bundle"}
        };

        [DataMember(Name = "productid")]
        public string ID { set; get; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "description")]
        public string Description { set; get; }

        [DataMember(Name = "productnumber")]
        public string ProductNumber { set; get; }

        [DataMember(Name = "hierarchypath")]
        public string HierarchyPath { private set; get; }

        private short productstructure;
        [DataMember(Name = "productstructure", EmitDefaultValue = false)]
        public string Type
        {
            set { productstructure = short.Parse(value); }
            get { return Structures[productstructure]; }
        }

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

        [DataMember(Name = "validfromdate", EmitDefaultValue = false)]
        public string ValidFromDate { set; get; }

        [DataMember(Name = "validtodate", EmitDefaultValue = false)]
        public string ValidToDate { set; get; }

        [DataMember(Name = "_pricelevelid_value")]
        public string PricelistID { set; get; }

        [DataMember(Name = "_pricelevelid_value@OData.Community.Display.V1.FormattedValue")]
        public string Pricelist { set; get; }

        [DataMember(Name = "_subjectid_value")]
        public string SubjectID { set; get; }

        [DataMember(Name = "_subjectid_value@OData.Community.Display.V1.FormattedValue")]
        public string Subject { set; get; }

        [DataMember(Name = "_defaultuomid_value")]
        public string UnitID { set; get; }
        [DataMember(Name = "_defaultuomid_value@OData.Community.Display.V1.FormattedValue")]
        public string Unit { set; get; }

        [DataMember(Name = "Product_DynamicProperty", EmitDefaultValue = false)]
        public List<Dynamicproperty> Properties { set; get; }

        [DataMember(Name = "Product_DynamicProperty@odata.nextLink", EmitDefaultValue = false)]
        public string PropertiesLink { set; get; }

        // For bundled product
        [DataMember(Name = "quantity", EmitDefaultValue = false)]
        public string Quantity { set; get; }
    }
}
