using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Types
{

    [DataContract]
    public class Pricelist
    {
        static Dictionary<short, string> States = new Dictionary<short, string>
        {
            { 0, "Active" },
            { 1, "Inactive" }
        };

        [DataMember(Name = "pricelevelid")]
        public string ID { set; get; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "description")]
        public string Description { set; get; }

        private short statecode;
        [DataMember(Name = "statecode", EmitDefaultValue = false)]
        public string Status {
            set { statecode = short.Parse(value); }
            get {
                return States[statecode];
            }
        }

        [DataMember(Name = "begindate", EmitDefaultValue = false)]
        public string BeginDate { set; get; }

        [DataMember(Name = "enddate", EmitDefaultValue = false)]
        public string EndDate { set; get; }
    }
}
