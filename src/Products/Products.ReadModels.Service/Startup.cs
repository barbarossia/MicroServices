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
using StackExchange.Redis;
using Products.ReadModels.Service.Views;
using System.Net;
using System.Web.Http;

using MicroServices.Common.MessageBus;
using MicroServices.Common.Repository;

using MicroServices.Common.Exceptions;
using EasyNetQ;
using MicroServices.Common;


using Products.Common.Dto;
using Products.Common.Events;

using Aggregate = MicroServices.Common.Aggregate;
using MicroServices.Common.General.Util;
using Newtonsoft.Json;

namespace Products.ReadModels.Service
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

                var redis = ConnectionMultiplexer.Connect("192.168.1.105:6379,name=venom");
                var productView = new ProductView(new RedisReadModelRepository<ProductDto>(redis.GetDatabase()));
                locator.ProductView = productView;
                
                var eventMappings = new EventHandlerDiscovery()
                                                .Scan(productView)
                                                .Handlers;
                var subscriptionName = "Products_readmodel";
                var topicFilter1 = "Products.Common.Events";

                var b = RabbitHutch.CreateBus("host=192.168.1.105;username=test;password=test");
                b.Subscribe<PublishedMessage>(subscriptionName,
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
                q => q.WithTopic(topicFilter1));

                var bus = new RabbitMqBus(b);
                locator.Bus = bus;
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
