using Common.Web.Mappers;
using Common.Web.Services.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
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
            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            // Register types
            //todo: srediti reference => services.AddScoped<ICustomExceptionHandler, TopologyException>();
            services.AddScoped<IGraphMapper, GraphMapper>();
            services.AddScoped<IConsumerMapper, ConsumerMapper>();
            services.AddScoped<IOutageMapper, OutageMapper>();
            services.AddScoped<IEquipmentMapper, EquipmentMapper>();

           // services.AddMediatR(typeof(Startup));
            services.AddMediatR( new Assembly[]
            {
                    typeof(GetTopologyQuery).GetTypeInfo().Assembly,
                    typeof(GetActiveOutagesQuery).GetTypeInfo().Assembly,
                    typeof(GetArchivedOutagesQuery).GetTypeInfo().Assembly
            });
            services.AddSignalR();
            //var assembly = AppDomain.CurrentDomain.Load("Data");
            //services.AddMediatR(assembly);
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
