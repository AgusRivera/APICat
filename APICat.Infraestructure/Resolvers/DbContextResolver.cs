using APICat.Domain.Entities;
using APICat.Infraestructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace APICat.Infraestructure.Resolvers
{
    public class DbContextResolver : IDbContextResolver
    {
        private readonly IServiceProvider _serviceProvider;
        // Hacemos el diccionario estático para calcularlo UNA sola vez al arrancar la app
        // y no cada vez que se inyecta el servicio (mejora de performance).
        private static readonly Dictionary<Type, Type> _entityToContextMap = new();

        static DbContextResolver()
        {
            // El constructor estático se ejecuta una sola vez al cargar la clase
            LoadEntityMap();
        }

        public DbContextResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private static void LoadEntityMap()
        {
            // 1. Buscamos todas las clases que hereden de DbContext en el Assembly actual
            // (Wrng: Si los contextos están en otro proyecto, cambiar Assembly.GetExecutingAssembly() por typeof(CalculadoraContext).Assembly)
            var contextTypes = Assembly.GetExecutingAssembly() // O el assembly donde estén los contextos
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract);

            foreach (var contextType in contextTypes)
            {
                // 2. Buscamos todas las propiedades publicas que sean DbSet<T>
                var dbSetProperties = contextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                foreach (var property in dbSetProperties)
                {
                    // 3. Extraemos el tipo de la entidad
                    var entityType = property.PropertyType.GetGenericArguments()[0];

                    // 4. Agregamos al mapa: Entidad -> Contexto
                    if (!_entityToContextMap.ContainsKey(entityType))
                    {
                        _entityToContextMap.Add(entityType, contextType);
                    }
                }
            }
        }

        public DbContext GetContext<TEntity>()
        {
            var entityType = typeof(TEntity);

            if (!_entityToContextMap.TryGetValue(entityType, out var contextType))
            {
                throw new InvalidOperationException($"No se encontró un DbContext que contenga un DbSet<{entityType.Name}>. Revisa que la propiedad DbSet sea pública en tu Contexto.");
            }

            // Resolvemos el contexto desde el contenedor de inyección de dependencias
            return (_serviceProvider.GetRequiredService(contextType) as DbContext)!;
        }
    }
}
