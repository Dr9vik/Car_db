using Business_Logic_Layer.BackgroundTask.Identity;
using Business_Logic_Layer.Common.Models.Identity;
using Business_Logic_Layer.Configuration;
using Business_Logic_Layer.Services.Identity;
using Data_Access_Layer.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Business_Logic_Layer.ServiceCollectionExtensions
{
    public static class JwtAuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddUserIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("JwtAuthentication");
            JwtAuthenticationConfiguration jwtAuthenticationConfiguration = section.Get<JwtAuthenticationConfiguration>();
            services.Configure<JwtAuthenticationConfiguration>(section);

            services.AddAuthorizationCore();
            services.AddSingleton<JwtAuthManager>();
            services.AddHostedService<JwtRefreshTokenCacheBackground>();
            services.AddTransient<UserService>();

            services.AddIdentity<IdentityUser, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 6;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(options =>
               {
                   options.RequireHttpsMetadata = false;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidIssuer = jwtAuthenticationConfiguration.Issuer,
                       ValidateLifetime = true,
                       ValidateAudience = true,
                       ValidAudience = jwtAuthenticationConfiguration.Audience,
                       IssuerSigningKey = jwtAuthenticationConfiguration.GetSymmetricSecurityKey(),
                       ValidateIssuerSigningKey = true
                   };
                   options.Events = new JwtBearerEvents
                   {
                       OnAuthenticationFailed = context =>
                       {
                           if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                           {
                               context.Response.Headers.Add("Token-Expired", "true");
                           }
                           return Task.CompletedTask;
                       },
                       OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Query[JwtAuthResult.AccessTokenKey];

                           var path = context.HttpContext.Request.Path;
                           if (!string.IsNullOrEmpty(accessToken) &&
                               path.StartsWithSegments("/hub"))
                           {
                               context.Token = accessToken;
                           }
                           else if (context.Request.Cookies.ContainsKey(JwtAuthResult.AccessTokenKey))
                           {
                               context.Token = context.Request.Cookies[JwtAuthResult.AccessTokenKey];
                           }
                           return Task.CompletedTask;
                       }
                   };
               });


            return services;
        }
    }
}
