using Business_Logic_Layer.Common.Models.Identity;
using Business_Logic_Layer.ExceptionModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services.Identity
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtAuthManager _jwtAuthManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserService(
           UserManager<IdentityUser> userManager,
           SignInManager<IdentityUser> signInManager,
           RoleManager<IdentityRole> roleManager,
           JwtAuthManager jwtAuthManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtAuthManager = jwtAuthManager;
        }

        public async Task<UserBL> LoginAsync(UserBLLogin userLoginModel)
        {
            if (string.IsNullOrWhiteSpace(userLoginModel?.UserName))
                throw new UserArgumentException("Empty username");
            var user = await _userManager.FindByNameAsync(userLoginModel.UserName);
            if (user == null)
                throw new UnauthorizedAccessException();

            var jwtResult = await AuthenticateAsync(userLoginModel);
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            return new UserBL
            {
                UserName = userLoginModel.UserName,
                Roles = roles,
                AccessToken = jwtResult.AccessToken,
                TokenExpires = jwtResult.AccessTokenExpiresAt,
                RefreshToken = jwtResult.RefreshToken.TokenString
            };
        }

        public async Task<JwtAuthResult> AuthenticateAsync(UserBLLogin userLoginModel)
        {
            if (string.IsNullOrWhiteSpace(userLoginModel?.UserName))
            {
                throw new UserArgumentException("Empty username");
            }
            var user = await _userManager.FindByNameAsync(userLoginModel.UserName);

            if (user == null)
                throw new UnauthorizedAccessException();
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, userLoginModel.Password, true).ConfigureAwait(false);
            if (!singInResult.Succeeded)
            {
                throw new UnauthorizedAccessException();
            }
            var allClaims = await _jwtAuthManager.GetUserClaimsAsync(user);
            var jwtResult = _jwtAuthManager.GenerateTokens(user.Id, user.UserName, allClaims, DateTimeOffset.Now);
            return jwtResult;
        }

        public async Task<UserBL> RegisterAsync(UserBLCreate item)
        {
            if (item.Password != item.PasswordConfirm)
                throw new UserArgumentException("Пароли не совпадают");
            if (item.Password.Length < 6)
                throw new UserArgumentException("Пароль слишком короткий");
            var user = new IdentityUser
            {
                UserName = item.UserName,
                Email = item.Email
            };

            var result = await _userManager.CreateAsync(user, item.Password);
            if (!result.Succeeded)
                throw new UserArgumentException(result.Errors.First().Description);

            var resultUser = new UserBL
            {
                UserName = item.UserName,
                Email = item.Email,
                Roles = new List<string>() { "AdminUser" }
            };
            return resultUser;
        }
 
    }
}
