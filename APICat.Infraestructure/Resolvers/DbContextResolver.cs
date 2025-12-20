using APICat.Domain.Entities;
using APICat.Infraestructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace APICat.Infraestructure.Resolvers
{
    public class DbContextResolver : IDbContextResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _entityToContextMap;

        public DbContextResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _entityToContextMap = new Dictionary<Type, Type>
            {
                //add entitys and contexts
                { typeof(Breed), typeof(CatContext) },
                
            };
        }

        public DbContext GetContext<TEntity>()
        {
            var entityType = typeof(TEntity);

            if (!_entityToContextMap.TryGetValue(entityType, out var contextType))
            {
                throw new InvalidOperationException($"No hay un DbContext configurado para la entidad {entityType.Name}");
            }

            var context = _serviceProvider.GetRequiredService(contextType) as DbContext;

            return context!;
        }
    }
}
