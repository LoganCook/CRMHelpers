using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Synchroniser.Models
{
    /// <summary>
    /// To unwrap a dynamics search return which value key can have 0 to many objects:
    /// {
    ///   "@odata.context":"https://ersasandbox.crm6.dynamics.com/api/data/v8.2/$metadata#salesorders",
    ///    "value":[ ... ]
    ///  }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class OData<T>
    {
        [DataMember(Name = "value")]
        public List<T> Value { get; set; }
    }

    /// <summary>
    /// Base of Order for serialization
    /// </summary>
    [DataContract]
    public class OrderBase
    {
        public readonly static string URI = "salesorders";

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "new_orderid")]
        public string OrderID { set; get; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    [DataContract]
    public class Order : OrderBase
    {
        [DataMember(Name = "salesorderid")]
        public string ID { set; get; }
        
        [DataMember(Name = "statuscode")]
        public int Status { get; private set; }

        public static string Get(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            return $"salesorders?$filter=name eq '{name}'";
        }

        public static string GetByOrderID(string id)
        {
            return $"salesorders?$filter=new_orderid eq '{id}'";
        }
    }

    /// <summary>
    /// Data contractor for creating JSON object for creating in Dynamics
    /// This is only used in serialization
    /// </summary>
    [DataContract]
    public class Order4Creation : OrderBase
    {
        private Guid customerID;
        private Guid priceLevelID;

        [DataMember(Name = "customerid_account@odata.bind")]
        public string CustomerID {
            set { customerID = new Guid(value); }
            get { return $"/accounts({customerID.ToString()})"; }
        }

        [DataMember(Name = "pricelevelid@odata.bind")]
        public string PriceLevelID
        {
            set { priceLevelID = new Guid(value); }
            get { return $"/pricelevels({priceLevelID.ToString()})"; }
        }

        [DataMember(Name = "order_details")]
        public List<OrderDetail>  OrderedProducts { set; get; }
    }

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
