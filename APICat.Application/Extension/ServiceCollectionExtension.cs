using APICat.Infraestructure.Repositories;
using APICat.Logging.Factory;
using APICat.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using APICat.Application.Interfaces;
using APICat.Application.Services;
using FluentValidation;
using APICat.Application.Models.Dtos;
using APICat.Application.Validators;

namespace APICat.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddRepositoriesAndServices(this IServiceCollection services)
        {
            // Repositories (Generic Implementation)
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            //Services (Specific Implementations)
            AddLoggedService<ICatService, CatService>(services);

            //Validators (Specific Implementations)
            services.AddScoped<IValidator<BreedsDto>, CatValidator>();


            return services;
        }

        private static IServiceCollection AddLoggedService<TInterface, TImplementation>(IServiceCollection services)
                where TInterface : class
                where TImplementation : class, TInterface
        {
            services.AddScoped<TInterface>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TImplementation>>();
                // Se crea el servicio usando el provider (soporta inyección de dependencias)
                var servicioReal = ActivatorUtilities.CreateInstance<TImplementation>(sp);
                return LoggingProxyFactory.Create<TInterface>(servicioReal, logger);
            });

            return services;
        }


    }
}
