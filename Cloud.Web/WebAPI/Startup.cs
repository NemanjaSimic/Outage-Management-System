using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OMS.Web.Common.Exceptions;
using OMS.Web.Common.Loggers;
using OMS.Web.Common.Mappers;
using Outage.Common.ServiceProxies;
using Unity.Lifetime;
using WebAPI.Hubs;

namespace WebAPI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200").AllowCredentials();
                    });
            });

            // Register types
            services.AddScoped<ICustomExceptionHandler, TopologyException>();
            services.AddScoped<IGraphMapper, GraphMapper>();
            services.AddScoped<IConsumerMapper, ConsumerMapper>();
            services.AddScoped<IOutageMapper, OutageMapper>();
            services.AddScoped<IEquipmentMapper, EquipmentMapper>();
            services.AddScoped<IProxyFactory, ProxyFactory>();
            services.AddScoped<Outage.Common.ILogger, FileLogger>(); // TODO: Proveriti da li treba 'ContainerControlledLifetimeManager()'

            services.AddMediatR(typeof(Startup));
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSignalR(routes =>
            {
                routes.MapHub<GraphHub>("/graphhub");
                routes.MapHub<OutageHub>("/outagehub");
                routes.MapHub<ScadaHub>("/scadahub");
            });

            app.UseMvc();
        }
    }
}
