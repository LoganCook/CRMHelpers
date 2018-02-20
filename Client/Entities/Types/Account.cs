using System.Runtime.Serialization;

namespace Client.Types
{
    [DataContract]
    public class Account
    {
        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "accountid")]
        public string ID { set; get; }

        [DataMember(Name = "_parentaccountid_value", EmitDefaultValue = false)]
        public string ParentAccountID { set; get; }
    }
}
