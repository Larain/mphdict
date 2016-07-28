﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Serilog;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using mphdict.Models.morph;
using mphdict;

namespace mphweb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"sid", "dcore0001"},
            });

            Configuration = builder.Build();

            var logFile = Path.Combine(env.ContentRootPath, "logs/log-{Date}.txt");
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Error()
                //.WriteTo.RollingFile(new Serilog.Formatting.Json.JsonFormatter(), logFile)
                .WriteTo.RollingFile(logFile, outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<mphObj>();
            services.AddEntityFramework()
                .AddEntityFrameworkSqlite()
                .AddDbContext<mphContext>(options => options.UseSqlite($"Filename={Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, $"data/{Configuration.GetConnectionString("sqlitedb")}")}"));
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            loggerFactory.AddSerilog();
            ApplicationLogging.LoggerFactory = loggerFactory;

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
                    template: "{controller=inflection}/{action=Index}/{id?}");
            });
        }
    }
    public static class ApplicationLogging
    {
        public static ILoggerFactory LoggerFactory { get; set; } /*= new LoggerFactory();*/
        public static Microsoft.Extensions.Logging.ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }

}
