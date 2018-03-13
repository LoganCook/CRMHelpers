using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{

    [DataContract]
    public class ProductPricelist
    {
        //Description: Pricing method applied to the price list.
        //Display Name: Pricing Method
        static Dictionary<short, string> Methods = new Dictionary<short, string>
        {
            { 1, "Currency Amount" },
            { 2, "Percent of List" },
            { 3, "Percent Markup - Current Cost" },
            { 4, "Percent Margin - Current Cost" },
            { 5, "Percent Markup - Standard Cost" },
            { 6, "Percent Margin - Standard Cost" }
        };

        //Description: Option for rounding the price list.
        //Display Name: Rounding Option
        static Dictionary<short, string> RoundingOptions = new Dictionary<short, string>
        {
            { 1, "Ends in" },
            { 2, "Multiple of" }
        };

        //Description: Policy for rounding the price list.
        //Display Name: Rounding Policy
        static Dictionary<short, string> RoundingPolicies = new Dictionary<short, string>
        {
            { 1, "None" },
            { 2, "Up" },
            { 3, "Down" },
            { 4, "To Nearest" }
        };

        //Description: Quantity of the product that must be sold for a given price level.
        //Display Name: Quantity Selling Option
        static Dictionary<short, string> QuantityOptions = new Dictionary<short, string>
        {
            { 1, "No Control" },
            { 2, "Whole" },
            { 3, "Whole and Fractional" }
        };

        [DataMember(Name = "productpricelevelid")]
        public string ID { set; get; }

        [DataMember(Name = "productnumber")]
        public string ProductNumber { set; get; }

        [DataMember(Name = "_productid_value")]
        public string ProductID { set; get; }

        [DataMember(Name = "_productid_value@OData.Community.Display.V1.FormattedValue")]
        public string Product { set; get; }

        [DataMember(Name = "_pricelevelid_value")]
        public string PricelistID { set; get; }

        [DataMember(Name = "_pricelevelid_value@OData.Community.Display.V1.FormattedValue")]
        public string Pricelist { set; get; }

        [DataMember(Name = "_uomid_value")]
        public string UnitID { set; get; }

        [DataMember(Name = "_uomid_value@OData.Community.Display.V1.FormattedValue")]
        public string Unit { set; get; }

        private short methodcode;
        [DataMember(Name = "pricingmethodcode", EmitDefaultValue = false)]
        public string Method
        {
            set { methodcode = short.Parse(value); }
            get
            {
                return Methods[methodcode];
            }
        }

        private short? optioncode;
        [DataMember(Name = "roundingoptioncode", EmitDefaultValue = false)]
        public string RoundingOption
        {
            set
            {
                if (value != null)
                    optioncode = short.Parse(value);
            }
            get
            {
                if (optioncode != null)
                    return RoundingOptions[optioncode.Value];
                return "";
            }
        }

        private short? policycode;
        [DataMember(Name = "roundingpolicycode", EmitDefaultValue = false)]
        public string RoundingPolicy
        {
            set
            {
                if (value != null)
                    policycode = short.Parse(value);
            }
            get
            {
                if (policycode != null)
                    return RoundingPolicies[policycode.Value];
                return "";
            }
        }

        private short? quantitysellingcode;
        [DataMember(Name = "quantitysellingcode", EmitDefaultValue = false)]
        public string QuantitySellingOption
        {
            set
            {
                if (value != null)
                    quantitysellingcode = short.Parse(value);
            }
            get
            {
                if (quantitysellingcode != null)
                    return QuantityOptions[quantitysellingcode.Value];
                return "";
            }
        }

        // amount and percentage are exclusive: one is set another is null
        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public float? Amount { set; get; }

        [DataMember(Name = "percentage", EmitDefaultValue = false)]
        public float? Percentage { set; get; }

        [DataMember(Name = "roundingoptionamount", EmitDefaultValue = false)]
        public float? RoundingAmount { set; get; }
    }
}
