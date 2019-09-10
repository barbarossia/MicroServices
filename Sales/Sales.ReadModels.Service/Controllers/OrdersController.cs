using System;
using System.Web.Http;
using MicroServices.Common.Exceptions;
using Sales.ReadModels.Service.Views;
using Microsoft.AspNetCore.Mvc;

namespace Sales.ReadModels.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderView view;
        public OrdersController(ServiceLocator locator)
        {
            this.view = locator.BrandView;
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