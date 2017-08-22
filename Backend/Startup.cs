using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Backend.Utils;
using Backend.Global;
using Backend.Jobs;
using System.Threading;
using System.Windows;
using Overlay;
using Database;
using Backend.Filters;

namespace Backend {
    public class Startup {
        private NetHookDumpReaderJob _netHookDumpReaderJob;
        private OverlayUpdaterJob _overlayUpdaterJob;

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.

            services.AddDbContext<Repository>();
            services.AddMvc(config => {
                config.Filters.Add(new AuthFilter());
            });

            services.AddTransient<IRepository, Repository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            if (Variables.ReplayMode) {
                Variables.NethookDumpDir = @"C:\Program Files (x86)\Steam\nethook\_Aoe2HostLobby2";
            } else {
                _netHookDumpReaderJob = new NetHookDumpReaderJob();
            }

            _overlayUpdaterJob = new OverlayUpdaterJob();
        }
    }
}
