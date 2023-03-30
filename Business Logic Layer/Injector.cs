using AutoMapper;
using Business_Logic_Layer.Common.Models;
using Business_Logic_Layer.Mappers;
using Business_Logic_Layer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Business_Logic_Layer
{
    public static class Injector
    {
        public static IServiceCollection BindInjector(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AllowNullCollections = true;
                mc.AddProfile(new CarProfile());
                mc.AddProfile(new UserCarProfile());
            });
            services.AddSingleton(mappingConfig.CreateMapper());

            services.AddTransient<IValidator<CarBLCreate>, CarBLCreateValidator>();
            services.AddTransient<IValidator<CarBLUpdate>, CarBLUpdateValidator>();

            return services;
        }
    }
}
