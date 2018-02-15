using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Entities
{
    public class Account : Base
    {
        public Account(CRMClient conn) : base(conn)
        {
            ENDPOINT = "accounts";
        }

        /// <summary>
        /// Get an Account record by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Types.Account> Get(string name)
        {
            // because this is a search, it returns a list with 0 or more records
            // this is different to using id
            string query = $"?$filter=name eq '{name}'&$select=accountid";
            return await GetEntityAsync<Types.Account>(query);
        }

        /// <summary>
        /// Get a list of Account's name and parentaccountid
        /// </summary>
        /// <returns></returns>
        public async Task<List<Types.Account>> List()
        {
            string query = "?$select=name,_parentaccountid_value";
            return await List<Types.Account>(query);
        }

        public async Task<List<Types.Account>> ListTopAccounts()
        {
            string query = "?$filter=_parentaccountid_value eq null&$select=name";
            return await List<Types.Account>(query);
        }

        public async Task<AccountIDManager> GetAccountIDManager()
        {
            return new AccountIDManager(await List());
        }
    }
}
