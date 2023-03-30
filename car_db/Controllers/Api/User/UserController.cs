using Business_Logic_Layer.Common.Models.Identity;
using Business_Logic_Layer.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace car_db.Controllers.Api.User
{

    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserBLCreate item)
        {
            var result = await _userService.RegisterAsync(item).ConfigureAwait(false);
            return Ok(result);
        }

    }
}
