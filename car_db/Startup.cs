using Business_Logic_Layer;
using Business_Logic_Layer.Common.Services;
using Business_Logic_Layer.ServiceCollectionExtensions;
using Business_Logic_Layer.Services;
using Data_Access_Layer.Common.Repositories;
using Data_Access_Layer.Contexts;
using Data_Access_Layer.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using car_db.ConfiguringApps;
using Serilog;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace car_db;

public class Startup
{
    public string? PathProject { get; } = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();

        ///optionsBuilder => optionsBuilder.MigrationsAssembly("car_db") не применять тут, создать отдельный проект
        services.AddDbContext<UserDbContext>(
            options => options.UseNpgsql(_configuration.GetConnectionString("Prod"),
                optionsBuilder => optionsBuilder.MigrationsAssembly("car_db")));

        ///optionsBuilder => optionsBuilder.MigrationsAssembly("car_db") не применять тут, создать отдельный проект
        services.AddDbContext<ApplicationDbContext>(
            options => options.UseNpgsql(_configuration.GetConnectionString("Prod"),
                optionsBuilder => optionsBuilder.MigrationsAssembly("car_db")));

        services.AddUserIdentity(_configuration);

        services.AddTransient<IDapperRepository2, DapperRepository2>();
        services.AddTransient<IRepository2, Repository2>();
   
        services.AddTransient<ICarService, CarService>();
        services.AddTransient<IUserCarService, UserCarService>();

        services.AddTransient<ExceptionMiddleware>();

        services.BindInjector();

        services.AddControllers((options) =>
        {
            options.CacheProfiles.Add("default", new CacheProfile()
            {
                Duration = 100,
                Location = ResponseCacheLocation.Any
            });
        })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Error = (currentObject, errorContext) =>
                {
                    errorContext.ErrorContext.Handled = false;
                };
            })
            .AddFluentValidation();

        if (_hostingEnvironment.IsDevelopment() || _hostingEnvironment.IsStaging())
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition("token", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = HeaderNames.Authorization,
                    Scheme = "Bearer"
                });
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://asp.net/core",
                        Detail = "Please refer to the errors property for additional details."
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });
        }
        services.AddHttpClient();
    }

    public void Configure(IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSerilogRequestLogging();
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("en-GB"),
            new CultureInfo("en"),
            new CultureInfo("ru-RU"),
            new CultureInfo("ru"),
        };
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("ru-RU"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        if (_hostingEnvironment.IsDevelopment() || _hostingEnvironment.IsStaging())
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture;
            app.UseCors(builder => builder.AllowAnyOrigin());
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

        app.UseMiddleware<ExceptionMiddleware>();

        if (_hostingEnvironment.IsProduction())
        {
            app.UseStatusCodePages();
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
