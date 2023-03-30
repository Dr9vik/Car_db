using Business_Logic_Layer.Common.Models.Identity;
using Business_Logic_Layer.ExceptionModel;
using Business_Logic_Layer.Services.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace car_db.Controllers.Api.User
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtAuthManager _jwtAuthManager;
        public AuthController(UserService service, JwtAuthManager jwtAuthManager)
        {
            _userService = service;
            _jwtAuthManager = jwtAuthManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserBLLogin item)
        {
            var result = await _userService.LoginAsync(item);
            return Ok(result);
        }


        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(UserBL), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserBL>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                var jwtResult = await _jwtAuthManager.RefreshAsync(request.RefreshToken);
                return Ok(new UserBL
                {
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString,
                    TokenExpires = jwtResult.AccessTokenExpiresAt
                });
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is UserArgumentException)
            {
                return Unauthorized(ex.Message); // return 401 so that the client side can redirect the user to login page
            }
        }
    }
}
