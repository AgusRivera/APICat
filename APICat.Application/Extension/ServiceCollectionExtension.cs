using APICat.Application.Interfaces;
using APICat.Application.Interfaces.Auth;
using APICat.Application.Models.Dtos;
using APICat.Application.Services;
using APICat.Application.Validators;
using APICat.Domain.Interfaces.Repositories;
using APICat.Infraestructure.Repositories;
using APICat.Logging.Factory;
using APIWeather.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APICat.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoriesAndServices(this IServiceCollection services)
        {
            // Repositories (Generic Implementation)
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            // Services con Proxy (Specific Implementations)
            AddLoggedService<ICatService, CatService>(services);
            AddLoggedService<IAuthService, JwtAuthService>(services);

            // Validators (Specific Implementations)
            services.AddScoped<IValidator<BreedsDto>, CatValidator>();

            return services;
        }

        private static IServiceCollection AddLoggedService<TInterface, TImplementation>(IServiceCollection services)
                where TInterface : class
                where TImplementation : class, TInterface
        {

            services.AddScoped<TImplementation>();

            // Registro INTERFAZ para que devuelva el PROXY
            services.AddScoped<TInterface>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TImplementation>>();

                var servicioReal = sp.GetRequiredService<TImplementation>();

                return LoggingProxyFactory.Create<TInterface>(servicioReal, logger);
            });

            return services;
        }
    }
}