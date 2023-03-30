using System;

namespace Business_Logic_Layer.Common.Models.Identity
{
    public class JwtAuthResult
    {
        public const string AccessTokenKey = "access_token";
        public const string RefreshTokenKey = "refresh_token";

        public string AccessToken { get; set; }
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
