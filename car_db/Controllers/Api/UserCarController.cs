using Business_Logic_Layer.Common.Models;
using Business_Logic_Layer.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace car_db.Controllers.Api
{
    [Route("api/usercar")]
    [ApiController]
    public class UserCarController : ControllerBase
    {
        private readonly IUserCarService _service;
        public UserCarController(IUserCarService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Create(UserCarBLCreate item)
        {
            return Ok(await _service.Create(item));
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
