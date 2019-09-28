using Sales.Service.MicroServices.Order.Handlers;
using Sales.Service.MicroServices.Product.View;
using MicroServices.Common.MessageBus;

namespace Sales.Service
{
    public class ServiceLocator
    {
        public IMessageBus Bus { get; set; }
        public OrderCommandHandlers OrderCommands { get; set; }
        public ProductView ProductView { get; set; }
    }
}