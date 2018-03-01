using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Json;
using System.Runtime.Serialization.Json;
using AzureTokenCache;

namespace Client
{
    public interface ITokenConsumer
    {
        /// <summary>
        /// Call a path on Dynamics server with Bearer token
        /// Pass the resulting stream directly to the caller
        /// </summary>
        /// <param name="relativePath">/entity</param>
        /// <returns>Stream</returns>
        Task<Stream> GetStreamAsync(string path);

        /// <summary> Sends an HTTP message containing a JSON payload to the target URL.
        /// Not expecting to receive content back.</summary>
        /// <typeparam name="T">Type of the data to send in the message content (payload)</typeparam>
        /// <param name="method">The HTTP method to invoke</param>
        /// <param name="requestUri">The relative URL of the message request</param>
        /// <param name="value">The data to send in the payload. The data will be converted to a serialized JSON payload. </param>
        /// <returns>An HTTP response message</returns>
        Task<HttpResponseMessage> SendJsonAsync<T>(HttpMethod method, string requestUri, T value);
    }

    /// <summary>
    /// A token consumer of Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache backed by file
    /// This intends to be used for service account which is used to talk to another API, e.g CRM
    /// </summary>
    public class CRMClient : ITokenConsumer
    {
        private static readonly HttpClient _httpClient;

        private string _authority;
        private string Resource;
        private string ClientID;
        private string ClientSecret;
        private ClientCredential Credential;
        private AuthenticationContext AuthContext;
        private FileCache FileTokenCache;

        static CRMClient()
        {
            _httpClient = new HttpClient();
            // Compose HTTP requests: https://msdn.microsoft.com/en-us/library/gg334391.aspx
            _httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            _httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            _httpClient.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="apiVersion"></param>
        /// <param name="filePath">optional, default = .\svc_crm_tokencache.data</param>
        public CRMClient(string authority, string resource, string clientId, string clientSecret, string apiVersion, string filePath = @".\svc_crm_tokencache.data")
        {
            _authority = authority;
            Resource = resource;
            ClientID = clientId;
            ClientSecret = clientSecret;
            FileTokenCache = new FileCache(filePath, true);
            AuthContext = new AuthenticationContext(_authority, FileTokenCache);
            Credential = new ClientCredential(ClientID, ClientSecret);
            Console.WriteLine("CRMClient constructor has been called");
            _httpClient.BaseAddress = new Uri(resource + "/api/data/v" + apiVersion + "/");
        }

        public async Task<string> GetToken()
        {
            var result = await AuthContext.AcquireTokenSilentAsync(Resource, Credential, FileTokenCache.GetUserIdentifier());
            return result.AccessToken;
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value ?? ""));
        }

        /// <summary>
        /// Convert a Stream to a (JSON) string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamToJSONString(Stream stream)
        {
            // All returns should be converted from Stream to String
            // and they are all JSON object in string format
            if (stream == null) return "{}";
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Deserialize a response stream to a class instance object
        /// </summary>
        /// <param name="response"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(Stream response)
        {
            if (response == null) return default(T);
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T) serializer.ReadObject(response);
        }

        /// <summary>
        /// Serialize an class instance object to string in JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                ser.WriteObject(stream, value);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Parse a stream to JSON
        /// </summary>
        /// <param name="stream">We consumed original stream, return another one instead</param>
        /// <returns>Recovered stream</returns>
        private Stream ParseStreamToJson(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string response = reader.ReadToEnd();
                Console.WriteLine("Received string has length = {0}, {1}", response.Length, response);
                JsonObject json2 = (JsonObject)JsonValue.Parse(response);
                foreach (var item in json2)
                {
                    Console.WriteLine(item);
                }
                // Recover(regenerate) a stream to pretend nothing happened
                return GenerateStreamFromString(response);
            }
        }

        public async Task<Stream> GetStreamAsync(string relativePath)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
            Console.WriteLine($"Calling with GetStreamAsync method to {relativePath}");
            await SetAuthorizationHeader();
            Stream response = null;
            try
            {
                // When status code is not in the range of [200, 299] GetStreamAsync throws an exception
                response = await _httpClient.GetStreamAsync(relativePath);
                // TODO: add debug switch
                // response = ParseStreamToJson(response);
                return response;
            }
            catch (HttpRequestException ex)
            {
                // There may not be any response when there is an exception
                if (response != null)
                    ParseStreamToJson(response);
                if (ex.Message.IndexOf("404") > 0)
                {
                    // Possible 404 messages (JSON) which are not available when call _httpClient.GetStreamAsync, reference only
                    // 1. wrong entity says: message: "Resource not found for the segment 'contactscontacts'.", code: ""
                    // 2. does not exist by ID: message: "contact With Id = 5f880511-b362-e611-80e3-c4346bc43f08 Does Not Exist"
                    // 3. does not exist by alternative key: message: "A record with the specified key values does not exist in contact entity"
                    // Query without result: empty value [], 200 status code
                    Console.WriteLine($"404, problematic uri: {relativePath}");
                } else if (ex.Message.IndexOf("401") > 0)
                {
                    Console.WriteLine("Unauthorized. Likely cache has been corrupted.");
                }
                else
                {
                    Console.WriteLine("Other exception:");
                    Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                // this is a rough handling
                Console.WriteLine(ex.GetType().ToString());
                if (response != null)
                    ParseStreamToJson(response);
                Console.WriteLine("The request failed with an error.");
                Console.WriteLine("Base address: {0}", _httpClient.BaseAddress);
                Console.WriteLine(ex.Message);
                while (ex.InnerException != null)
                {
                    Console.WriteLine("\t* {0}", ex.InnerException.Message);
                    ex = ex.InnerException;
                }
            }
            return null;
        }

        /// <summary>
        /// Set Bearer token in Authorization header
        /// </summary>
        /// <returns></returns>
        private async Task SetAuthorizationHeader()
        {
            // Set Bearer token every time
            string accessToken = await GetToken();
            Console.WriteLine($"Authorization header has Bearer token {accessToken.Substring(0, 10)}...");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Send the content of a JsonObject to a relative path by specified method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="relativePath"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string relativePath, JsonObject value)
        {
            return await SendJsonAsync(method, relativePath, value.ToString());
        }

        /// <summary>
        /// Send the content of a Client.Types.* to a relative path by specified method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="relativePath"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendJsonAsync<T>(HttpMethod method, string relativePath, T value)
        {
            return await SendJsonAsync(method, relativePath, SerializeObject(value));
        }

        /// <summary>
        /// Send the content of JOSN in string to a relative path by specified method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="relativePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string relativePath, string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, relativePath)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            Console.WriteLine("Calling with SendJsonAsync method: {0} to {1}", content, relativePath);
            await SetAuthorizationHeader();
            return await _httpClient.SendAsync(request);
        }
    }
}
