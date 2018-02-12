using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureTokenCache
{
    // http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/

    /// <summary>
    /// Persistent file storage for TokenCache of a single user
    /// </summary>
    public class FileCache : TokenCache
    {
        public string CachFilePath { get; }
        private static readonly object FileLock = new object();
        private byte[] cacheBits;

        public FileCache(string filePath = @".\tokencache.data", bool shouldExist = false)
        {
            if (shouldExist && !File.Exists(filePath))
            {
                throw new FileNotFoundException("Saved token of another user is not found.", filePath);
            }
            CachFilePath = filePath;
            ReadFromFile();
            AfterAccess = AfterAccessNotification;
        }

        internal void ReadFromFile()
        {
            lock (FileLock)
            {
                if (File.Exists(CachFilePath))
                    cacheBits = File.ReadAllBytes(CachFilePath);
                this.Deserialize(cacheBits);
            }
        }

        // Triggered right after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // only write if the access operation resulted in a cache update
            if (this.HasStateChanged) SaveToFile();
        }

        void SaveToFile()
        {
            lock (FileLock)
            {
                cacheBits = this.Serialize();
                File.WriteAllBytes(CachFilePath, cacheBits);
                // manually change StateChanged as suggested by CloudIdentity
                this.HasStateChanged = false;
            }
        }

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
                Dictionary<string, string> token = new Dictionary<string, string>
                {
                    ["DisplayableId"] = item.DisplayableId,
                    ["Resource"] = item.Resource,
                    ["ExpiresOn"] = item.ExpiresOn.ToString()
                };
                tokens.Add(token);
            }
            return tokens;
        }

        public UserIdentifier GetUserIdentifier()
        {
            // if TokenCahe has not content, it means Identity is no-name
            if (this.Count == 0)
                return UserIdentifier.AnyUser;

            var current = this.ReadItems().First(); // Only get the top one, most likely only one
            return new UserIdentifier(current.UniqueId, UserIdentifierType.UniqueId);
        }
    }
}
