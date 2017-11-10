using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Synchroniser.Models;
using System.IO;

namespace Synchroniser.Pages.Entities.Products
{
    public class IndexModel : PageModel
    {
        private readonly ITokenConsumer _crmClient;
        public List<Product> Products { get; private set; }

        public IndexModel(ITokenConsumer crmClient)
        {
            _crmClient = crmClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Stream response = await _crmClient.GetStreamAsync(Product.List());
            if (response == null)
            {
                return NotFound("Strangely no product is available.");
            }
            OData<Product> wrapper = CRMClient.DeserializeObject<OData<Product>>(response);
            Products = wrapper.Value;
            return Page();
        }
    }
}