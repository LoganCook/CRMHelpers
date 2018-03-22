using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using System.Json;

namespace Client.Entities
{
    /// <summary>
    /// Class for creating simple query string
    /// </summary>
    // https://msdn.microsoft.com/en-us/library/gg334767.aspx#BKMK_FilterNavProperties
    // https://msdn.microsoft.com/en-us/library/mt742424.aspx
    public static class Query
    {
        // Create $select for a simple query
        public static string CreateList(string[] fields)
        {
            return string.Join(",", fields);
        }

        /// <summary>
        /// Build a simple query like this: ?$select=name,revenue&$filter=revenue 
        /// </summary>
        /// <param name="selects">An array of fields need to be retrieved</param>
        /// <param name="filter">A simple string represent a filter. Caller constructs filter</param>
        /// <returns></returns>
        public static string Build(string[] selects, string filter = "")
        {
            Dictionary<string, string> parts = new Dictionary<string, string>()
            {
                ["$select"] = CreateList(selects)
            };
            if (!string.IsNullOrEmpty(filter))
                parts.Add("$filter", filter);

            return Build(parts);
        }

        /// <summary>
        /// Build a simple query
        /// </summary>
        /// <param name="arguments">A dictionary with known keys like $select, $filter,$orderby=revenue asc,name desc</param>
        /// <returns></returns>
        public static string Build(Dictionary<string, string> arguments)
        {
            QueryBuilder qb = new QueryBuilder(arguments);
            return qb.ToQueryString().ToUriComponent();
        }
    }

    public abstract class Base
    {
        protected CRMClient _connector;  // initialised outside, here just a place holder
        protected string ENDPOINT = "";  // Entities end point
        protected string[] commonFileds;

        public Base(CRMClient conn)
        {
            _connector = conn;
        }

        // Share some basic information with the world
        /// <summary>
        /// Common fields
        /// </summary>
        public string[] Fields { get { return commonFileds; } }
        /// <summary>
        /// Connector with CRM backend
        /// </summary>
        public CRMClient Connector { get { return _connector; } }

        /// <summary>
        /// Get method to backend
        /// Run a query against Dynamics and return a JSON string of the response
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<Stream> GetAsync(string query)
        {
            try
            {
                return _connector.GetStreamAsync(ENDPOINT + query);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Console.WriteLine("Captured at API endpoint");
                Console.WriteLine("HTTP request failed: {0}", ex.ToString());
                Console.Write("Exception Type: ");
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception is: {0}", ex.InnerException.GetType().ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Non-HTTP exception captured.");
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        /// <summary>
        /// Run a query against Dynamics and return a JSON string of the response
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> GetJsonStringAsync(string query)
        {
            return CRMClient.StreamToJSONString(await GetAsync(query));
        }

        /// <summary>
        /// Run a query against Dynamics and return an Entity
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<T> GetEntityAsync<T>(string query)
        {
            return CRMClient.DeserializeObject<T>(await GetAsync(query));
        }


        /// <summary>
        /// Get a list of T
        /// </summary>
        /// <returns>a list with 0 or more records</returns>
        public async Task<List<T>> List<T>(string query)
        {
            try
            {
                return CRMClient.DeserializeObject<Types.OData<T>>(await GetAsync(query)).Value;
            }
            catch (NullReferenceException)
            {
                // If GetAsync returns null, DeserializeObject returns null so we do the same
                // This was likely caused by something wrong the set up code: wrong end point etc
                return null;
            }
        }

        /// <summary>
        /// Get a list of T for a @odata.nextLink
        /// </summary>
        /// <returns>a list with 0 or more records</returns>
        /// <see>https://msdn.microsoft.com/en-us/library/gg334767.aspx#Retrieve related entities by expanding navigation properties</see>
        public async Task<List<T>> NextLink<T>(string url)
        {
            try
            {
                return CRMClient.DeserializeObject<Types.OData<T>>(await _connector.GetStreamAsync(url)).Value;
            }
            catch (NullReferenceException)
            {
                // If GetAsync returns null, DeserializeObject returns null so we do the same
                // This was likely caused by something wrong the set up code: wrong end point etc
                return null;
            }
        }

        /// <summary>
        /// Get a list of instance of an entity with common fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<List<T>> ListAll<T>()
        {
            string query = Query.Build(commonFileds);
            return List<T>(query);
        }

        /// <summary>
        /// Get a list of filtered instance of an entity with common fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<List<T>> ListAll<T>(string filter)
        {
            string query = Query.Build(commonFileds, filter);
            return List<T>(query);
        }

        #region Common query builders used internally
        /// <summary>
        /// Query of an entity by an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected string GetEntityByIdQuery(Guid id)
        {
            return $"({id})" + Query.Build(commonFileds);
        }
        #endregion

        /// <summary>
        /// Get an entity by its id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns>null or entity</returns>
        public Task<T> Get<T>(Guid id)
        {
            return GetEntityAsync<T>(GetEntityByIdQuery(id));
        }

        /// <summary>
        /// Get an entity by its name with its common fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<T> GetByName<T>(string name)
        {
            string query = Query.Build(commonFileds, $"name eq '{name}'");
            return GetEntityAsync<T>(query);
        }

        /// <summary>
        /// Update an entity represented by its id with fields in content of either one of Types
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content">One of types</param>
        /// <returns></returns>
        public async Task Update<T>(string id, T content)
        {
            // requires ENDPOINT to build url of the entity by the id
            HttpResponseMessage response = await _connector.SendJsonAsync(
                new HttpMethod("PATCH"), $"{ENDPOINT}({id})", (T) content);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                //Utils.DisplayResponse(response);
                throw new HttpRequestException(await Utils.GetErrorMessage(response));
            }
        }

        /// <summary>
        /// Update an entity represented by its id with fields in content of JsonObject
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content">JsonObject or one of types</param>
        /// <returns></returns>
        public async Task Update(string id, JsonObject content)
        {
            // requires ENDPOINT to build url of the entity by the id
            HttpResponseMessage response = await _connector.SendJsonAsync(
                new HttpMethod("PATCH"), $"{ENDPOINT}({id})", content);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                //Utils.DisplayResponse(response);
                throw new HttpRequestException(await Utils.GetErrorMessage(response));
            }
        }

        public async Task<Guid> Create<T>(T content)
        {
            Console.WriteLine($"Is {ENDPOINT} correct?");
            HttpResponseMessage response = await _connector.SendJsonAsync(HttpMethod.Post, ENDPOINT, content);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                string newEntity = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                Console.WriteLine($"New entity URI: {newEntity}");
                string newEntityID = Utils.GetIdFromUrl(newEntity);
                Console.WriteLine("New entity ID: {0}", newEntityID);
                return new Guid(newEntityID);
            }
            else
            {
                Utils.DisplayResponse(response);
                throw new HttpRequestException(await Utils.GetErrorMessage(response));
            }
        }
    }
}
