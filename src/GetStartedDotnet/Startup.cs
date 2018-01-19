using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GetStartedDotnet.Models;
using GetStartedDotnet.Services;
using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq; //DELETE THIS

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
            
            // CF 'cleardb' service
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
                    // Failed to read ClearDB uri, ignore this and continue without a database
                }
            } 
            // user-provided service with 'cleardb' in the name
            else if (json.ContainsKey("user-provided")) 
            {
                foreach (var service in json["user-provided"]) 
                {
                    if (((String) service.name).Contains("cloudantNoSQLDB")) 
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
                            // Failed to read ClearDB uri, ignore this and continue without a database
                        }
                    }
                }
            }
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //var databaseUri = Configuration["cleardb:0:credentials:uri"];
        //if (!string.IsNullOrEmpty(databaseUri))
        //{
        //    // add database context
        //    services.AddDbContext<VisitorsDbContext>(options => options.UseMySQL(getConnectionString(databaseUri)));
        //}
        
        //// Add framework services.
        services.AddMvc();

        var creds = new Creds()
        {
            username = Configuration["cloudantNoSQLDB:0:credentials:username"],
            password = Configuration["cloudantNoSQLDB:0:credentials:password"],
            host = Configuration["cloudantNoSQLDB:0:credentials:host"]
        };
        services.AddSingleton(typeof(Creds), creds);
        services.AddTransient<ICloudantService, CloudantService>();
    }

    //private string getConnectionString(string databaseUri)
    //{
    //    var connectionString = "";
    //    try
    //    {
    //        string hostname;
    //        string username;
    //        string password;
    //        string port;
    //        string database;
    //        username = databaseUri.Split('/')[2].Split(':')[0];
    //        password = (databaseUri.Split(':')[2]).Split('@')[0];
    //        var portSplit = databaseUri.Split(':');
    //        port = portSplit.Length == 4 ? (portSplit[3]).Split('/')[0] : null;
    //        var hostSplit = databaseUri.Split('@')[1];
    //        hostname = port == null ? hostSplit.Split('/')[0] : hostSplit.Split(':')[0];
    //        var databaseSplit = databaseUri.Split('/');
    //        database = databaseSplit.Length == 4 ? databaseSplit[3] : null;
    //        var optionsSplit = database.Split('?');
    //        database = optionsSplit.First();
    //        port = port ?? "3306"; // if port is null, use 3306
    //        connectionString = $"Server={hostname};uid={username};pwd={password};Port={port};Database={database};SSL Mode=Required;";
    //    }
    //    catch (IndexOutOfRangeException ex)
    //    {
    //        throw new FormatException("Invalid database uri format", ex);
    //    }

    //    return connectionString;
    //}

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

        //var context = (app.ApplicationServices.GetService(typeof(VisitorsDbContext)) as VisitorsDbContext);
        //context?.Database.EnsureCreated();

        app.UseStaticFiles();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
