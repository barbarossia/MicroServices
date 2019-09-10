using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sales.Service.DataTransferObjects.Commands;
using Sales.Service.MicroServices.Order.Commands;
using Microsoft.AspNetCore.Mvc;
using Sales.Service.MicroServices.Order.Handlers;

namespace Sales.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderCommandHandlers orderCommands;
        public OrdersController(ServiceLocator locator)
        {
            this.orderCommands = locator.OrderCommands;
        }

        [HttpPost]
        public ActionResult Post(PlaceOrderCommand cmd)
        {
            if (Guid.Empty.Equals(cmd.Id))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("order information must be supplied in the POST body"),
                    ReasonPhrase = "Missing Order Id"
                };
                // throw new HttpResponseException(response);
                return BadRequest(response);
            }

            var command = new StartNewOrder(cmd.Id, cmd.ProductId, cmd.Quantity);

            try
            {
                orderCommands.Handle(command);

                var link = new Uri(string.Format("http://localhost:8182/api/orders/{0}", command.Id));
                return Created(link, command);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
        }
    }
}