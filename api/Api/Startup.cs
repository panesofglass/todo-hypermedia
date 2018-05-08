﻿using Amazon.DynamoDBv2;
using Api.Web;
using App;
using Infrastructure.NoSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace Api
{
    public class Startup
    {
        private IHostingEnvironment HostingEnvironment { get; }
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            HostingEnvironment = env;
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {

//            var awsOptions = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//                .AddEnvironmentVariables()
//                .Build()
//                .GetAWSOptions();
//
//            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
//
//            services.AddDefaultAWSOptions(awsOptions);
//            services.AddAWSService<IAmazonDynamoDB>();


            services.AddMvcCore(options =>
                {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;

                    // map execeptions to http status codes
                    options.Filters.Add(typeof(ExceptionFilter));

                    // Important: InputFormatters are used only when [FromBody] is used 
                    // in the parameter's list of the action
                    options.InputFormatters.Add(new FormUrlEncodedMediaFormatter());

                    // Content-negotitation output types
                    options.OutputFormatters.Add(new HtmlFormMediaFormatter());

                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                })
                .AddJsonFormatters(s => s.ContractResolver = new DefaultContractResolver())
                .AddXmlDataContractSerializerFormatters();

            services
                .RegisterIoc(HostingEnvironment)
                .AddTodoCors();

            services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            /**
             * Note: this block MUST be before app.UseMvc();
             */
            loggerFactory.AddConsole();


            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app
                .UseTodoCors()
                .UseMvc()
                .MigrateDynamoDb();
        }
    }

    public static class MigrateDynamoDbExtensions
    {
        public static IApplicationBuilder MigrateDynamoDb(this IApplicationBuilder app)
        {
            var client = app.ApplicationServices.GetService<IAmazonDynamoDB>();

            TableNameConstants
                .Todo
                .CreateTable(client)
                .ConfigureAwait(false);

            TableNameConstants
                .Tenant
                .CreateTable(client)
                .ConfigureAwait(false);

            return app;
        }
    }
}