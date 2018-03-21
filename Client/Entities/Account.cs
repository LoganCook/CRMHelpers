﻿using System;
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
            commonFileds = new string[] { "accountid", "name", "_parentaccountid_value" };
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
            string query = Query.Build(commonFileds);
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

        #region private queries
        /// <summary>
        /// Build a query to get a list of child accounts of an account defined by its name
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>The result from Dynamics server as string</returns>
        private string ListChildAccountsQuery(string parent)
        {
            var xml = new FetchXML(ENTITY);
            FetchElement entity = xml.EntityElement;
            entity.AddField("name");
            entity.AddField("accountid");
            FetchElement linkedEntiry = entity.AddLinkEntity("account", "accountid", "parentaccountid");
            FetchElement filter = linkedEntiry.AddFilter();
            filter.AddCondition("name", "eq", value: parent);
            return xml.ToQueryString();
        }
        #endregion

        #region for apis and views
        /// <summary>
        /// Get a list of child accounts of an account defined by its name
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>The result from Dynamics server as string</returns>
        public async Task<string> ListChildAccountsString(string parent)
        {
            return await GetJsonStringAsync(ListChildAccountsQuery(parent));
        }

        public Task<List<Types.Account>> ListChildAccounts(string parent)
        {
            if (string.IsNullOrEmpty(parent))
            {
                return Task.FromResult(new List<Types.Account>());
            }
            return List<Types.Account>(ListChildAccountsQuery(parent));
        }
        #endregion

        public async Task<AccountIDManager> GetAccountIDManager()
        {
            return new AccountIDManager(await List());
        }
    }
}
