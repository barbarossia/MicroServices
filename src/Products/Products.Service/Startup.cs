using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Products.Service.MicroServices.Products.Handlers;
using MicroServices.Common.MessageBus;
using MicroServices.Common.Repository;
using EasyNetQ;
using EventStore.ClientAPI;
using System.Net;

namespace Products.Service
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
            //ConfigureHandlers();
            services.AddSingleton<ProductCommandHandlers>(sp =>
            {
                var bus = new RabbitMqBus(RabbitHutch.CreateBus("host=192.168.1.105;username=test;password=test"));


                //Should get this from a config setting instead of hardcoding it.
                var connectionString = "ConnectTo=tcp://admin:changeit@192.168.1.105:1113; Gossip Timeout = 500";
                var eventStoreConnection = EventStoreConnection.Create(connectionString);
                eventStoreConnection.ConnectAsync().Wait();
                var repository = new EventStoreRepository(eventStoreConnection, bus);

                return new ProductCommandHandlers(repository);
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
