using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ADConnectors;
using Client;

namespace Synchroniser
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private void DisposeResources()
        {
            eRSA.Dispose();
        }

        public IConfiguration Configuration { get; }
        private IADSearcher eRSA;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            eRSA = Creater.GetADConnector(Configuration.GetSection("AD"));
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

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));
            Log.Logger.Debug("All services have been set up.");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, ITokenConsumer crmClient)
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
                routes.MapRoute(name: "default", template: "{controller}/{action=Index}/{id?}");
            });

            applicationLifetime.ApplicationStopping.Register(DisposeResources);
        }
    }
}
