using System;
using System.Runtime.Serialization;

namespace Synchroniser.Models
{
    // This is only for connecting Order and Contact with ConnectionRoles in team category
    [DataContract]
    public class Connection
    {
        public readonly static string URI = "connections";

        private Guid record1ID;
        [DataMember(Name = "record1id_salesorder@odata.bind")]
        public string OrderID
        {
            set { record1ID = new Guid(value); }
            get { return $"/{OrderBase.URI}({record1ID})"; }
        }

        private Guid record2ID;
        [DataMember(Name = "record2id_contact@odata.bind")]
        public string ContactID
        {
            set { record2ID = new Guid(value); }
            get { return $"/{Contact.URI}({record2ID})"; }
        }

        private Guid record2RoleID;
        [DataMember(Name = "record2roleid@odata.bind")]
        public string RoleID 
        {
            set { record2RoleID = new Guid(value); }
            get { return $"/{ConnectionRole.URI}({record2RoleID})"; }
        }

        // Currently only support record2 as Contact
        [DataMember(Name = "record2objecttypecode")]
        public int ContactType
        {
            set { throw new NotSupportedException("ContactType is read-only property"); }
            get { return 2;  }
        }
    }

    [DataContract]
    public class ConnectionRole
    {
        public readonly static string URI = "connectionroles";

        [DataMember(Name = "connectionroleid")]
        public Guid ID { set; get; }

        [DataMember(Name = "name")]
        public string Name { set; get; }

        [DataMember(Name = "description")]
        public string Description { set; get; }

        // Comes from connectionrole_category option set
        [DataMember(Name = "category")]
        public int Category { set; get; }
    }
}
