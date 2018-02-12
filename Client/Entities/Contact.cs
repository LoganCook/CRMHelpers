using System;
using System.Json;
using System.Threading.Tasks;

namespace Client.Entities
{
    /// <summary>
    /// Contact with basic fileds for creation
    /// </summary>
    public class Contact : Base
    {
        public Contact(CRMClient conn) : base(conn)
        {
            ENDPOINT = "contacts";
        }

        /// <summary>
        /// Get a Contact by its contactid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Types.Contact> Get(Guid id)
        {
            string query = $"({id})";
            return CRMClient.DeserializeObject<Types.Contact>(await GetAsync(query));
        }

        /// <summary>
        /// Create a query for a quick check by email in CRM to see if a Contact exists
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public string GetByEmailQuery(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentException("email for checking a Contact has not been provided");
            return string.Format("(emailaddress1='{0}')?$select=contactid", emailAddress);
        }

        /// <summary>
        /// To do a quick check by email in CRM to see if a Contact exists by primary email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        // eRSA has emailaddress1 as a key in Contact, so there is always only one Contact returned
        public async Task<Types.Contact> GetByEmail(string emailAddress)
        {
            return await GetEntityAsync<Types.Contact>(GetByEmailQuery(emailAddress));
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
