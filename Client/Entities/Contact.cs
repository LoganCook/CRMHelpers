using System;
using System.Json;
using System.Threading.Tasks;

namespace Client.Entities
{
    /// <summary>
    /// Contact with basic fields for creation
    /// </summary>
    public class Contact : Base
    {
        public const string ENTITY = "contact";
        public Contact(CRMClient conn) : base(conn)
        {
            ENDPOINT = "contacts";
            commonFileds = new string[] { "contactid", "firstname", "lastname", "emailaddress1", "new_username", "department" };
        }

        /// <summary>
        /// Create a query for a quick check by email in CRM to see if a Contact exists
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        private string GetByEmailQuery(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentException("email for checking a Contact has not been provided");
            return Query.Build(new string[] { "contactid" }, $"emailaddress1 eq '{emailAddress}'");
        }

        /// <summary>
        /// To do a quick check by email in CRM to see if a Contact exists by primary email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        // eRSA has emailaddress1 as a key in Contact, so there is always only one Contact returned
        public async Task<Types.Contact> GetByEmail(string emailAddress)
        {
            var result = await List<Types.Contact>(GetByEmailQuery(emailAddress));
            if (result.Count == 1)
                return result[0];
            return null;
        }

        /// <summary>
        /// Update username field of a Contact with contactid of id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task UpdateUsername(string id, string username)
        {
            Types.ContactBase content = new Types.ContactBase
            {
                Username = username
            };
            await Update(id, content);
        }
    }
}
