using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace AzureTokenCache.Pages
{
    public class IndexModel : PageModel
    {
        public string cacheFilePath { get; private set; }
        public IList<Dictionary<string, string>> cachedTokens { get; private set; }

        public IndexModel(IConfiguration configuration)
        {
            cacheFilePath = configuration.GetValue<string>("TokenCachePath");
            FileCache cache = new FileCache(cacheFilePath);
            cachedTokens = cache.GetTokens();
        }

        public void OnGet()
        {

        }
    }
}
