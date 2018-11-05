using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GetStartedDotnet.Models;
using GetStartedDotnet.Services;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class Startup
{
    public IConfigurationRoot Configuration { get; set; }

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddJsonFile("vcap-local.json", optional:true); // when running locally, store VCAP_SERVICES credentials in vcap-local.json

        Configuration = builder.Build();

        string vcapServices = Environment.GetEnvironmentVariable("VCAP_SERVICES");
        if (vcapServices != null)
        {
            dynamic json = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(vcapServices);
            
            // CF 'cloudantNoSQLDB' service
            if (json.ContainsKey("cloudantNoSQLDB"))
            {
                try
                {
                    Configuration["cloudantNoSQLDB:0:credentials:username"] = json["cloudantNoSQLDB"][0].credentials.username;
                    Console.WriteLine("username ");
                    Console.WriteLine(Configuration["cloudantNoSQLDB:0:credentials:username"]);
                    Configuration["cloudantNoSQLDB:0:credentials:password"] = json["cloudantNoSQLDB"][0].credentials.password;
                    Console.WriteLine("password ");
                    Console.WriteLine(json["cloudantNoSQLDB"][0].credentials.password);
                    Configuration["cloudantNoSQLDB:0:credentials:host"] = json["cloudantNoSQLDB"][0].credentials.host;
                    Console.WriteLine("host ");
                    Console.WriteLine(json["cloudantNoSQLDB"][0].credentials.host);
                    Configuration["cloudantNoSQLDB:0:credentials:url"] = json["cloudantNoSQLDB"][0].credentials.url;
                    Console.WriteLine("url ");
                    Console.WriteLine(json["cloudantNoSQLDB"][0].credentials.url);
                }
                catch (Exception)
                {
                    // Failed to read Cloudant uri, ignore this and continue without a database
                }
            } 
            // user-provided service with 'cloudant' in the name
            else if (json.ContainsKey("user-provided")) 
            {
                foreach (var service in json["user-provided"]) 
                {
                    if (((String) service.name).Contains("cloudant")) 
                    {
                        try
                        {
                            Configuration["cloudantNoSQLDB:0:credentials:username"] = json["cloudantNoSQLDB"][0].credentials.username;
                            Configuration["cloudantNoSQLDB:0:credentials:password"] = json["cloudantNoSQLDB"][0].credentials.password;
                            Configuration["cloudantNoSQLDB:0:credentials:host"] = json["cloudantNoSQLDB"][0].credentials.host;
                            Configuration["cloudantNoSQLDB:0:credentials:url"] = json["cloudantNoSQLDB"][0].credentials.url;
                        }
                        catch (Exception)
                        {
                            // Failed to read Cloudant uri, ignore this and continue without a database
                        }
                    }
                }
            }
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add framework services.


        var creds = new Creds()
        {
            username = Configuration["cloudantNoSQLDB:0:credentials:username"],
            password = Configuration["cloudantNoSQLDB:0:credentials:password"],
            host = Configuration["cloudantNoSQLDB:0:credentials:host"]
        };

        if (creds.username != null && creds.password != null && creds.host != null)
        {
            services.AddAuthorization();
            services.AddSingleton(typeof(Creds), creds);
            services.AddTransient<ICloudantService, CloudantService>();
            services.AddTransient<LoggingHandler>();
            services.AddHttpClient("cloudant", client =>
            {
                Console.WriteLine("HERE SERVICE" + JObject.FromObject(creds));

                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(creds.username + ":" + creds.password));

                client.BaseAddress = new Uri("https://" + creds.host);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            })
            .AddHttpMessageHandler<LoggingHandler>();
        }

        services.AddMvc();
        
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        var cloudantService = ((ICloudantService)app.ApplicationServices.GetService(typeof(ICloudantService)));

        loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        loggerFactory.AddDebug();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
    }

    class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
                                                                     System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("{0}\t{1}", request.Method, request.RequestUri);
            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine(response.StatusCode);
            return response;
        }
    }
}
