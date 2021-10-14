﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace WebFormApp
{
    public class Global : System.Web.HttpApplication
    {
        public static IConfiguration Configuration;

        private static IConfigurationRefresher _configurationRefresher;

        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize configuration from Azure App Configuration.
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Environment.GetEnvironmentVariable("ConnectionString"))
                       // Load all keys that start with `TestApp:`
                       .Select("TestApp:*")
                       // Configure to reload configuration if the registered key 'TestApp:Settings:Sentinel' is modified.
                       // Use the default cache expiration time of 30 seconds that can be changed by calling SetCacheExpiration.
                       .ConfigureRefresh(refresh => 
                       {
                           refresh.Register("TestApp:Settings:Sentinel", refreshAll:true);
                       });
                _configurationRefresher = options.GetRefresher();
            });

            Configuration = builder.Build();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Signal to refresh the configuration if the registered key(s) is modified.
            // This will be a no-op if the cache expiration time window is not reached.
            // The configuration is refreshed asynchronously without blocking the execution of the current request.
            _ = _configurationRefresher.TryRefreshAsync();
        }
    }
}