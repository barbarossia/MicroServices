using Products.ReadModels.Service.Views;
using MicroServices.Common.MessageBus;

namespace Products.ReadModels.Service
{
    public class ServiceLocator
    {
        public IMessageBus Bus { get; set; }
        public ProductView ProductView { get; set; }
    }
}