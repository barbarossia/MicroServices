using System;
using System.Web.Http;
using MicroServices.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Products.ReadModels.Service.Views;

namespace Products.ReadModels.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductView view;
        public ProductsController(ServiceLocator locator)
        {
            this.view = locator.ProductView;
        }
        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            try
            {
                var dto = view.GetById(id);
                return Ok(dto);
            }
            catch (ReadModelNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public ActionResult Get()
        {
            var result = view.GetAll();
            return Ok(result);
        }
    }
}