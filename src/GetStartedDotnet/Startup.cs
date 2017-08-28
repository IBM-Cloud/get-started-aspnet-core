using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GetStartedDotnet.Models;
using System;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore.Extensions;

public class Startup
{
    public IConfigurationRoot Configuration { get; }

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
            dynamic json = JsonConvert.DeserializeObject(vcapServices);
            if (json.cleardb != null)
            {
                try
                {
                    Configuration["cleardb:0:credentials:uri"] = json.cleardb[0].credentials.uri;
                }
                catch (Exception)
                {
                    // Failed to read ClearDB uri, ignore this and continue without a database
                }
            }
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var databaseUri = Configuration["cleardb:0:credentials:uri"];
        if (!string.IsNullOrEmpty(databaseUri))
        {
            // add database context
            services.AddDbContext<VisitorsDbContext>(options => options.UseMySQL(getConnectionString(databaseUri)));
        }
        
        // Add framework services.
        services.AddMvc();
    }

    private string getConnectionString(string databaseUri)
    {
        var connectionString = "";
        try
        {
            string hostname;
            string username;
            string password;
            string port;
            string database;
            username = databaseUri.Split('/')[2].Split(':')[0];
            password = (databaseUri.Split(':')[2]).Split('@')[0];
            var portSplit = databaseUri.Split(':');
            port = portSplit.Length == 4 ? (portSplit[3]).Split('/')[0] : null;
            var hostSplit = databaseUri.Split('@')[1];
            hostname = port == null ? hostSplit.Split('/')[0] : hostSplit.Split(':')[0];
            var databaseSplit = databaseUri.Split('/');
            database = databaseSplit.Length == 4 ? databaseSplit[3] : null;
            var optionsSplit = database.Split('?');
            database = optionsSplit.First();
            port = port ?? "3306"; // if port is null, use 3306
            connectionString = $"Server={hostname};uid={username};pwd={password};Port={port};Database={database};SSL Mode=Required;";
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new FormatException("Invalid database uri format", ex);
        }

        return connectionString;
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
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

        var context = (app.ApplicationServices.GetService(typeof(VisitorsDbContext)) as VisitorsDbContext);
        context?.Database.Migrate();

        app.UseStaticFiles();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
