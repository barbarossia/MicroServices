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
using EventStore.ClientAPI;
using Sales.Service.Config;
using Sales.Service.MicroServices.Order.Handlers;
using MicroServices.Common.MessageBus;
using MicroServices.Common.Repository;
using EasyNetQ;
using MicroServices.Common.General.Util;
using Sales.Service.MicroServices.Product.Handlers;
using MicroServices.Common;
using System;
using System.Net;
using Newtonsoft.Json;
using Sales.Service.MicroServices.Product.View;

namespace Sales.Service
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
            services.AddSingleton<ServiceLocator>(sp =>
            {
                ServiceLocator locator = new ServiceLocator();

                var b = RabbitHutch.CreateBus("host=192.168.1.105;username=test;password=test");
                var bus = new RabbitMqBus(b);
                locator.Bus = bus;

                var messageBusEndPoint = "Sales_service";
                var topicFilter = "Products.Common.Events";

                IPAddress ip = IPAddress.Parse("192.168.1.105");
                var eventStorePort = 1113;
                var eventStoreConnection = EventStoreConnection.Create(new IPEndPoint(ip, eventStorePort));
                // var connectionString = "ConnectTo=tcp://admin:changeit@192.168.1.105:12900; Gossip Timeout = 500";
                // var eventStoreConnection = EventStoreConnection.Create(connectionString);
                eventStoreConnection.ConnectAsync().Wait();
                var repository = new EventStoreRepository(eventStoreConnection, bus);

                locator.OrderCommands = new OrderCommandHandlers(repository);
                locator.ProductView = new ProductView();

                var eventMappings = new EventHandlerDiscovery()
                    .Scan(new ProductEventsHandler(locator))
                    .Handlers;

                b.Subscribe<PublishedMessage>(messageBusEndPoint,
                m =>
                {
                    Aggregate handler;
                    var messageType = Type.GetType(m.MessageTypeName);
                    var handlerFound = eventMappings.TryGetValue(messageType, out handler);
                    if (handlerFound)
                    {
                        var @event = JsonConvert.DeserializeObject(m.SerialisedMessage, messageType);
                        handler.AsDynamic().ApplyEvent(@event, ((Event)@event).Version);
                    }
                },
                q => q.WithTopic(topicFilter));


                return locator;
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
