using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{
    /// <summary>
    /// Base of OrderDetail for serialization (reading)
    /// </summary>
    [DataContract]
    public class OrderdetailSimple
    {
        private static Dictionary<short, string> ProductTypes = new Dictionary<short, string>
        {
            { 1, "Product" },
            { 2, "Bundle" },
            { 3, "Required Bundle Product" },
            { 4, "Optional Bundle Product" },
            { 5, "Project-based Service" }
        };

        [DataMember(Name = "_productid_value@OData.Community.Display.V1.FormattedValue")]
        public string Product { set; get; }

        [DataMember(Name = "_productid_value")]
        public string ProductID { set; get; }

        [DataMember(Name = "priceperunit")]
        public float UnitPrice { set; get; }

        [DataMember(Name = "manualdiscountamount_base")]
        public float Discount { set; get; }

        [DataMember(Name = "quantity")]
        public int Quantity { set; get; }

        [DataMember(Name = "_uomid_value")]
        public string UnitID { set; get; }

        [DataMember(Name = "_uomid_value@OData.Community.Display.V1.FormattedValue")]
        public string Unit { set; get; }

        private short productType;
        [DataMember(Name = "producttypecode")]
        public string ProductType {
            set { productType = short.Parse(value); }
            get { return ProductTypes[productType]; }
        }
    }

    // To associate new entities to existing entities when they are created you must set the value of single-valued navigation properties using the @odata.bind annotation.
    // https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/create-entity-web-api
    /// <summary>
    /// Data contract to create an ordered product
    /// </summary>
    [DataContract]
    public class OrderDetail
    {
        [DataMember(Name = "quantity")]
        public int Quantity { set; get; }

        private Guid productID;
        [DataMember(Name = "productid@odata.bind")]
        public string ProductID
        {
            set { productID = new Guid(value); }
            get { return $"/products({productID.ToString()})"; }
        }

        private Guid unitID;
        [DataMember(Name = "uomid@odata.bind")]
        public string UnitID
        {
            set { unitID = new Guid(value); }
            get { return $"/uoms({unitID.ToString()})"; }
        }
    }
}
