using APICat.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Domain.Interfaces.Repositories
{
    public interface IRepository<TEntity, TId> : IRepositoryBase where TEntity : EntityBase<TId>
    {
        Task<PaginatedResult<TEntity>> GetPagedAsync(
            int skip,
            int take,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<PaginatedResult<TResult>> GetProjectedPagedAsync<TResult>(
            int skip,
            int take,
            Expression<Func<TEntity, TResult>>? select,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
            Expression<Func<TEntity, TResult>>? select,
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<ICollection<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includeProperties);

        void Add(TEntity entity);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entity);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

        Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken = default);

        IQueryable<TEntity> GetQueryable();

        IQueryable<TEntity> GetQueryable(
            Expression<Func<TEntity, bool>>? filter,
            string includeProperties = "",
            bool trackChanges = false);

        IQueryable<TResult> GetProjectedQueryable<TResult>(
            Expression<Func<TEntity, TResult>>? select,
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false);

        Task<int> ExecuteDeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<int> ExecuteDeleteByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
