using Business_Logic_Layer.Common.Models.Identity;
using Business_Logic_Layer.Configuration;
using Business_Logic_Layer.ExceptionModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services.Identity
{
    public class JwtAuthManager
    {
        public IImmutableDictionary<string, RefreshToken> UsersRefreshTokensReadOnlyDictionary => _usersRefreshTokens.ToImmutableDictionary();
        private readonly ConcurrentDictionary<string, RefreshToken> _usersRefreshTokens;  // can store in a database or a distributed cache
        private readonly JwtAuthenticationConfiguration _jwtAuthenticationConfiguration;
        private readonly IServiceProvider _serviceProvider;
        private readonly byte[] _secret;

        private readonly ILogger<JwtAuthManager> _logger;

        public JwtAuthManager(IOptions<JwtAuthenticationConfiguration> jwtOptions,
            ILogger<JwtAuthManager> logger,
            IServiceProvider serviceProvider)
        {
            _jwtAuthenticationConfiguration = jwtOptions.Value;
            _usersRefreshTokens = new ConcurrentDictionary<string, RefreshToken>();
            _secret = Encoding.ASCII.GetBytes(_jwtAuthenticationConfiguration.Key);
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // optional: clean up expired refresh tokens
        public void RemoveExpiredRefreshTokens(DateTimeOffset now)
        {
            _logger.LogInformation("RemoveExpiredRefreshTokens");
            var expiredTokens = _usersRefreshTokens.Where(x => x.Value.ExpiresAt < now).ToList();
            foreach (var expiredToken in expiredTokens)
            {
                _usersRefreshTokens.TryRemove(expiredToken.Key, out _);
            }
        }

        // can be more specific to ip, user agent, device name, etc.
        public void RemoveRefreshTokenByUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }
            _logger.LogInformation("RemoveRefreshTokenByUserName");
            var refreshTokens = _usersRefreshTokens.Where(x => x.Value.UserName.ToUpper() == userName.ToUpper()).ToList();
            foreach (var refreshToken in refreshTokens)
            {
                _usersRefreshTokens.TryRemove(refreshToken.Key, out _);
            }
        }

        public JwtAuthResult GenerateTokens(string id, string username, IEnumerable<Claim> claims, DateTimeOffset now)
        {
            _logger.LogInformation("GenerateTokens");
            var shouldAddAudienceClaim = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            var jwtToken = new JwtSecurityToken(
                _jwtAuthenticationConfiguration.Issuer,
                shouldAddAudienceClaim ? _jwtAuthenticationConfiguration.Audience : string.Empty,
                claims,
                expires: now.UtcDateTime.Add(_jwtAuthenticationConfiguration.AccessTokenTTL),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256Signature));
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var refreshToken = new RefreshToken
            {
                Id = id,
                UserName = username,
                TokenString = GenerateRefreshTokenString(),
                ExpiresAt = now.Add(_jwtAuthenticationConfiguration.RefreshTokenTTL)
            };

            if (!_usersRefreshTokens.TryAdd(refreshToken.TokenString, refreshToken))
                _logger.LogWarning("!_usersRefreshTokens.TryAdd(refreshToken.TokenString, refreshToken) {refreshtoken}", refreshToken);

            var elem = _usersRefreshTokens.Where(x => x.Value.Id == id).OrderBy(x => x.Value.ExpiresAt).ToList();
            int maxCountUserConnect = 3;
            if (elem.Count() > maxCountUserConnect)
            {
                int r = elem.Count() - maxCountUserConnect;
                foreach (var i in elem)
                {
                    if (r > 0)
                    {
                        _usersRefreshTokens.TryRemove(i.Key, out _);
                        r--;
                    }
                    else
                        break;
                }
            }

            return new JwtAuthResult
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = now.Add(_jwtAuthenticationConfiguration.AccessTokenTTL),
                RefreshToken = refreshToken
            };
        }

        public async Task<JwtAuthResult> RefreshAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh {token}, {userTokens}", refreshToken, _usersRefreshTokens);
            var now = DateTimeOffset.Now;
            if (!_usersRefreshTokens.TryGetValue(refreshToken, out var existingRefreshToken))
            {
                throw new UserArgumentException("Invalid refreshToken", new { RefreshToken = refreshToken });
            }
            else
                _usersRefreshTokens.TryRemove(refreshToken, out _);
            if (existingRefreshToken.ExpiresAt < now)
            {
                throw new UserArgumentException("ExpireAt time off", new { RefreshToken = refreshToken });
            }
            var claims = await GetUserClaimsAsync(existingRefreshToken.UserName);

            return GenerateTokens(existingRefreshToken.Id, existingRefreshToken.UserName, claims, now); // need to recover the original claims
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userName)
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = await userManager.FindByNameAsync(userName);
            return await GetUserClaimsAsync(user);
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(IdentityUser user)
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);
            var allClaims = userClaims.Select(cl => new Claim(cl.Type, cl.Value))
                    .Concat(userRoles.Select(ur => new Claim(ClaimTypes.Role, ur)))
                    .Append(new Claim(ClaimTypes.Name, user.UserName));
            return allClaims;
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
