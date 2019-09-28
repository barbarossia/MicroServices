using Sales.ReadModels.Service.Views;
using MicroServices.Common.MessageBus;
using StackExchange.Redis;

namespace Sales.ReadModels.Service
{
    public class ServiceLocator
    {
        public IMessageBus Bus { get; set; }
        public OrderView BrandView { get; set; }
    }
}