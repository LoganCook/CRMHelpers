using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ADConnectors;

namespace Synchroniser.Pages.AD
{
    public class UserModel : PageModel
    {
        private readonly IADSearcher _aDSearcher;
        public Dictionary<string, string> Detail;

        public UserModel(IADSearcher aDSearcher)
        {
            _aDSearcher = aDSearcher;
        }

        public void OnGet(int id)
        {
            Detail = _aDSearcher.GetUser(id);
        }
    }
}
