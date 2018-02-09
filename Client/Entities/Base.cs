using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Entities
{
    public abstract class Base
    {
        protected CRMClient _connector;  // initialised outside, here just a palce holder
        protected string ENDPOINT = "";  // Entities end point

        public Base(CRMClient conn)
        {
            _connector = conn;
        }

        /// <summary>
        /// Get method to backend
        /// Run a query against Dynamics and return a JSON string of the response
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Stream> GetAsync(string query)
        {
            try
            {
                return await _connector.GetStreamAsync(ENDPOINT + query);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Console.WriteLine("Captured at api endpoint");
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
        /// Get a list of Account's name and parentaccountid
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> List<T>(string query)
        {
            return CRMClient.DeserializeObject<Types.OData<T>>(await GetAsync(query)).Value;
        }

        /// <summary>
        /// Get an URI of entity with an ID for updating
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetObjectURI(Guid id)
        {
            return $"{ENDPOINT}({id})";
        }

        /// <summary>
        /// Get an URI of entity with an ID for updating
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetObjectURI(string id)
        {
            return $"{ENDPOINT}({id})";
        }

        /// <summary>
        /// Update an entity represented by its id with fields in content of either one of Types or JsonObject
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task Update<T>(string id, T content)
        {
            HttpResponseMessage response = await _connector.SendJsonAsync(
                new HttpMethod("PATCH"), GetObjectURI(id), content);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                //Utils.DisplayResponse(response);
                throw new HttpRequestException(await Utils.GetErrorMessage(response));
            }
        }

        public async Task Create<T>(T content)
        {
            Console.WriteLine($"Is {ENDPOINT} correct?");
            HttpResponseMessage response = await _connector.SendJsonAsync(HttpMethod.Post, ENDPOINT, content);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                string newEntityID = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                Console.WriteLine($"New entity url: {newEntityID}");
                Console.WriteLine("New entity ID: {0}", Utils.GetIdFromUrl(newEntityID));
            }
            else
            {
                Utils.DisplayResponse(response);
                throw new HttpRequestException(await Utils.GetErrorMessage(response));
            }
        }
    }
}
