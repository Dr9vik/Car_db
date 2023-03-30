using Business_Logic_Layer.Common.Models;
using Business_Logic_Layer.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace car_db.Controllers.Api
{
    [Route("api/car")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarService _service;
        public CarController(ICarService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Create(CarBLCreate item)
        {
            return Ok(await _service.Create(item));
        }

        [HttpPost]
        [Route("update")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Update(CarBLUpdate item)
        {
            return Ok(await _service.Update(item));
        }

        [HttpPost]
        [Route("delete/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.Delete(id);
            return Ok();
        }


        [HttpGet]
        [Route("get/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.FindById(id);
            return Ok(result);
        }


        [HttpGet]
        [Route("getall")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.FindAll());
        }

        [HttpGet]
        [Route("getall/{first}/{second}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAll(int first, int second)
        {
            return Ok(await _service.FindAll(first, second));
        }
    }
}
