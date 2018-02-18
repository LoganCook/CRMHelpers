using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Client.Entities
{
    public class Account : Base
    {
        public const string ENTITY = "account";

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
            string query = Query.Build(new string[] {"accountid"}, $"name eq {name}");
            return await GetEntityAsync<Types.Account>(query);
        }

        /// <summary>
        /// Get a list of Account's name and parentaccountid
        /// </summary>
        /// <returns></returns>
        public async Task<List<Types.Account>> List()
        {
            string query = Query.Build(new string[] {"name", "_parentaccountid_value"});
            return await List<Types.Account>(query);
        }

        /// <summary>
        /// Get a list of account which does not have parent account
        /// </summary>
        /// <returns></returns>
        public async Task<List<Types.Account>> ListTopAccounts()
        {
            string query = "?$filter=_parentaccountid_value eq null&$select=name";
            return await List<Types.Account>(query);
        }

        /// <summary>
        /// Get a list of child accounts of an account defined by its name
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>The result from Dynamics server as string</returns>
        public async Task<string> ListChildAccounts(string parent)
        {
            var xml = new FetchXML(ENTITY);
            FetchElement entity = xml.EntityElement;
            entity.AddField("name");
            entity.AddField("accountid");
            FetchElement linkedEntiry = entity.AddLinkEntity("account", "accountid", "parentaccountid");
            FetchElement filter = linkedEntiry.AddFilter();
            filter.AddCondition("name", "eq", value: parent);
            return await GetJsonStringAsync(xml.ToQueryString());
        }

        public async Task<AccountIDManager> GetAccountIDManager()
        {
            return new AccountIDManager(await List());
        }
    }
}
