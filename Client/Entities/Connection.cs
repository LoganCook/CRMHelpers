using System;
using System.Runtime.Serialization;

namespace Client.Entities
{
    public class Connection : Base
    {
        public const string ENTITY = "connection";

        public Connection(CRMClient conn) : base(conn)
        {
            ENDPOINT = "connections";
            // No Client.Types defined for reading yet, Client.Types.Connection is for creating
            commonFileds = new string[] { "connectionid", "name", "description", "statecode", "record1objecttypecode", "record2objecttypecode", "_record1id_value", "_record2id_value",
                "_record1roleid_value", "_record1roleid_value" };
        }
    }
}
