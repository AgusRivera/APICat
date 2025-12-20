using Microsoft.EntityFrameworkCore;

namespace APICat.Infraestructure.Resolvers
{
    public interface IDbContextResolver
    {
        DbContext GetContext<TEntity>();
    }
}
