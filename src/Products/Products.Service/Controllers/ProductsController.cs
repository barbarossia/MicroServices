using System;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Products.Service.DataTransferObjects.Commands;
using Products.Service.MicroServices.Products.Commands;
using Products.Service.MicroServices.Products.Handlers;
using MicroServices.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Products.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductCommandHandlers handler;

        public ProductsController(ProductCommandHandlers handler)
        {
            this.handler = handler;
        }

        [HttpPost]
        
        public ActionResult Post(CreateProductCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Name))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("code must be supplied in the body"),
                    ReasonPhrase = "Missing product code"
                };
                //throw new HttpResponseException(response);
                return BadRequest(response);
            }

            try
            {
                var command = new CreateProduct(Guid.NewGuid(), cmd.Name, cmd.Description, cmd.Price);
                handler.Handle(command);

                var link = new Uri(string.Format("http://localhost:5001/api/products/{0}", command.Id));
                return Created(link, command);
            }
            catch (AggregateNotFoundException)
            {
                return NotFound();
            }
            catch (AggregateDeletedException)
            {
                return Conflict();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, AlterProductCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Name))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("code must be supplied in the body"),
                    ReasonPhrase = "Missing product code"
                };
                //throw new HttpResponseException(response);
                return BadRequest(response);
            }

            try
            {
                var command = new AlterProduct(id, cmd.Version, cmd.Name, cmd.Description, cmd.Price);
                handler.Handle(command);

                return Ok(command);
            }
            catch (AggregateNotFoundException)
            {
                return NotFound();
            }
            catch (AggregateDeletedException)
            {
                return Conflict();
            }
        }
    }
}