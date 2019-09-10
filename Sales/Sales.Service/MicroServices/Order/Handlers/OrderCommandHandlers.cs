using System;
using Sales.Service.MicroServices.Order.Commands;
using MicroServices.Common.Repository;
using Sales.Service.MicroServices.Product.View;
using Products.ReadModels.Client;

namespace Sales.Service.MicroServices.Order.Handlers
{
    public class OrderCommandHandlers
    {
        private readonly ProductView productView;
        private readonly IRepository repository;

        public OrderCommandHandlers(IRepository repository)
            : this(repository, new ProductsView())
        {
        }

        public OrderCommandHandlers(IRepository repository, IProductsView ProductsProductsView)
        {
            this.repository = repository;
            this.productView = new ProductView(ProductsProductsView);
        }

        public void Handle(StartNewOrder message)
        {
            ValidateProduct(message.ProductId);
            var order = new Domain.Order(message.Id, message.ProductId, message.Quantity);
            repository.Save(order);
        }

        public void Handle(PayForOrder message)
        {
            var order = repository.GetById<Domain.Order>(message.Id);
            int committableVersion = message.Version;
            order.PayForOrder(committableVersion);
            repository.Save(order);
        }

        void ValidateProduct(Guid productId)
        {
            if (productId != Guid.Empty)
            {
                try
                {
                    productView.GetById(productId);
                }
                catch (Exception)
                {
                    throw new ArgumentOutOfRangeException("productId", "Invalid product identifier specified: the product cannot be found.");
                }
            }
        }

    }
}