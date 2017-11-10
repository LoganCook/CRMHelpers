using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ADConnectors;

namespace Synchroniser
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            AD eRSA = new AD(Configuration["AD:DomainName"]);
            services.AddSingleton<IADSearcher>(eRSA);
            try
            {
                CRMClient crmClient = new CRMClient(Configuration["Dynamics:Authority"], Configuration["Dynamics:Resource"],
                    Configuration["Dynamics:ClientId"], Configuration["Dynamics:ClientSecret"], Configuration["Dynamics:Version"]);
                services.AddSingleton<ITokenConsumer>(crmClient);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.ToString());
                throw new ApplicationException("Cannot continue because saved token file is not found.");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ITokenConsumer crmClient)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.Run(async context => {
                var response = context.Response;
                if (context.Request.Path.Equals("/api/crm/contact"))
                {
                    foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> kvp in context.Request.Query)
                    {
                        Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value.ToString());
                    }
                    string result = "{}";
                    string query = Models.Contact.GetCheckQuery(context.Request.Query["email"]);
                    try
                    {
                        result = CRMClient.StreamToJSONString(await crmClient.GetStreamAsync(query));
                        //result = await crmClient.CheckContact(context.Request.Query["email"]);
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
                    await WriteHtmlAsync(response, result);
                }
                return;
            });
        }

        private static async Task WriteHtmlAsync(HttpResponse response, string msg)
        {
            response.ContentType = "application/json";
            await response.WriteAsync(msg);
        }
    }
}
