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

        [DataMember(Name = "Product_DynamicProperty")]
        public List<Dynamicproperty> Properties { set; get; }

        // FIXME: needed by Pages\Entities\Products\Index.cshtml.cs
        // once it is removed, these can be removed as all queries are defined in Entity namespace
        public static string List()
        {
            return "products?$filter=statecode eq 0 and productstructure eq 1&$select=name,description";
        }
        public static string Get(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            return $"products?$filter=name eq {name}&$select=productid";
        }
    }
}
