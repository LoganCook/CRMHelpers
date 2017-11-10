using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureTokenCache
{
    // http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/
 
    /// <summary>
    /// Persistent file storage for TokenCache
    /// </summary>
    public class FileCache : TokenCache
    {
        public string CachFilePath { get; }
        private static readonly object FileLock = new object();

        public FileCache(string filePath = @".\tokencache.data", bool shouldExist = false)
        {
            if (shouldExist && !File.Exists(filePath))
            {
                throw new FileNotFoundException("Saved token of another user is not found.", filePath);
            }
            CachFilePath = filePath;
            ReadFromFile();
            BeforeAccess = BeforeAccessNotification;
            AfterAccess = AfterAccessNotification;
        }

#region public methods
        /// <summary>
        /// Display cached tokens for debugging purpose
        /// </summary>
        public void DisplayTokens()
        {
            Console.WriteLine("Item count in cache: {0}", Count);
            foreach (var item in ReadItems())
            {
                Console.WriteLine("{0} can access resource {1} with token expires on {2}", item.DisplayableId, item.Resource, item.ExpiresOn.ToString());
            }
        }

        /// <summary>
        /// Get cached token as a list of Dictionary of DisplayableId, Resource and ExpiresOn 
        /// </summary>
        public IList<Dictionary<string, string>> GetTokens()
        {
            IList<Dictionary<string, string>> tokens = new List<Dictionary<string, string>>();
            foreach (var item in ReadItems())
            {
                Dictionary<string, string> token = new Dictionary<string, string>();
                token["DisplayableId"] = item.DisplayableId;
                token["Resource"] = item.Resource;
                token["ExpiresOn"] = item.ExpiresOn.ToString();
                tokens.Add(token);
            }
            return tokens;
        }
#endregion

        public void SaveToFile()
        {
            lock (FileLock)
            {
                File.WriteAllBytes(CachFilePath, this.Serialize());
                // manually change StateChanged as suggested by CloudIdentity
                this.HasStateChanged = false;
            }
        }

        public UserIdentifier GetUserIdentifier()
        {
            if (this.Count == 0)
            {
                Console.WriteLine("Cache count = {0}", this.Count);
                // might throw Exception is better because there is no token in the file?
                Console.WriteLine("Should I throw?, currently return AnyUser");
                return UserIdentifier.AnyUser;
            }
            var current = this.ReadItems().First(); // Only get the top one, most likely only one
            return new UserIdentifier(current.UniqueId, UserIdentifierType.UniqueId);
        }

        internal void ReadFromFile()
        {
            lock (FileLock)
            {
                this.Deserialize(File.Exists(CachFilePath) ? File.ReadAllBytes(CachFilePath) : null);
            }
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            ReadFromFile();
        }

        // Triggered right after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // only write if the access operation resulted in a cache update
            if (this.HasStateChanged) SaveToFile();
        }
    }
}
