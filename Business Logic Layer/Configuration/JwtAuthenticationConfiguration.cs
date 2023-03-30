using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Business_Logic_Layer.Configuration
{
    public class JwtAuthenticationConfiguration
    {
        public string Issuer { get; set; } // издатель токена
        public string Audience { get; set; } // потребитель токена
        public string Key { get; set; }   // ключ для шифрации
        public TimeSpan AccessTokenTTL { get; set; } //время жизни AccessToken
        public TimeSpan RefreshTokenTTL { get; set; } //время жизни RefreshToken

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
