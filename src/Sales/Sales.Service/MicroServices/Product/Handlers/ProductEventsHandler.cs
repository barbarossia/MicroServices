using MicroServices.Common;
using Sales.Service.MicroServices.Product.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sales.Service.MicroServices.Product.View;

namespace Sales.Service.MicroServices.Product.Handlers
{
    public class ProductEventsHandler : Aggregate,
        IHandle<ProductCreated>,
        IHandle<ProductPriceChanged>
    {
        private readonly ProductView view;
        public ProductEventsHandler(ServiceLocator locator)
        {
            this.view = locator.ProductView;
        }
        
        public void Apply(ProductCreated @event)
        {
            // var view = locator.ProductView;
            view.Add(@event.Id, @event.Price);
        }

        public void Apply(ProductPriceChanged @event)
        {
            // var view = locator.ProductView;
            var product = view.GetById(@event.Id);
            product.Price = @event.NewPrice;
        }
    }
}