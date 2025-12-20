using APICat.Domain;
using APICat.Domain.Entities.Base;
using APICat.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Infraestructure.Repositories
{
    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : EntityBase<TId>
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private bool _disposedValue;

        public Repository(DbContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        /// <summary>
        ///     Obtiene un resultado paginado de entidades.
        /// </summary>
        /// <param name="skip">Número de elementos a omitir.</param>
        /// <param name="take">Número de elementos a tomar.</param>
        /// <param name="filter">Expresión para filtrar las entidades (opcional).</param>
        /// <param name="orderBy">Función para aplicar ordenamiento (opcional).</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir (opcional).</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios (opcional).</param>
        /// <param name="cancellationToken">Token de cancelación (opcional).</param>
        /// <returns>Un resultado paginado que contiene las entidades.</returns>
        public virtual async Task<PaginatedResult<TEntity>> GetPagedAsync(
            int skip,
            int take,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(filter, includeProperties, trackChanges);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<TEntity>(items, totalCount, take, skip);
        }

        /// <summary>
        ///     Obtiene un resultado paginado proyectado.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado proyectado.</typeparam>
        /// <param name="skip">Número de elementos a omitir.</param>
        /// <param name="take">Número de elementos a tomar.</param>
        /// <param name="select">Expresión para proyectar las entidades.</param>
        /// <param name="filter">Expresión para filtrar las entidades (opcional).</param>
        /// <param name="orderBy">Función para aplicar ordenamiento (opcional).</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir (opcional).</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios (opcional).</param>
        /// <param name="cancellationToken">Token de cancelación (opcional).</param>
        /// <returns>Un resultado paginado que contiene las entidades proyectadas.</returns>
        public virtual async Task<PaginatedResult<TResult>> GetProjectedPagedAsync<TResult>(
            int skip,
            int take,
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetProjectedQueryable(select, filter, includeProperties, trackChanges);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<TResult>(items, totalCount, take, skip);
        }

        /// <summary>
        /// Obtiene una lista de entidades que cumplen con los criterios especificados.
        /// </summary>
        /// <param name="filter"> Expresión lambda opcional para filtrar los resultados (ejemplo: x => x.Activo == true).</param>
        /// <param name="orderBy">Función opcional para ordenar la consulta (ejemplo: q => q.OrderBy(x => x.Nombre)).</param>
        /// <param name="cancellationToken">Token de cancelación para abortar la operación asíncrona.</param>
        /// <param name="includeProperties">Lista de expresiones lambda que indican las propiedades de navegación a incluir (eager loading).</param>
        /// <returns>Una lista de entidades que cumplen con los criterios especificados.</returns>
        public virtual async Task<ICollection<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync(cancellationToken);
            }

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las entidades de la base de datos sin aplicar filtros ni ordenamientos.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación para abortar la operación asíncrona.</param>
        /// <returns>Una lista de todas las entidades existentes en la base de datos.</returns>
        public virtual async Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <summary>
        ///     Obtiene una entidad por su identificador único tipo INT.
        /// </summary>
        /// <param name="id">Identificador único de la entidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Entidad encontrada.</returns>
        public virtual async Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstAsync(e => e.Id.Equals(id), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Obtiene una entidad por su identificador único tipo GUID.
        /// </summary>
        /// <param name="id">Identificador único de la entidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Entidad encontrada.</returns>
        public virtual async Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstAsync(e => e.Id.Equals(id), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Obtiene la primera entidad que cumple con el filtro especificado o null si no se encuentra ninguna.
        /// </summary>
        /// <param name="filter">Expresión para filtrar las entidades.</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir.</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Primera entidad encontrada o null.</returns>
        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(filter, includeProperties, trackChanges);
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Obtiene la primera entidad proyectada que cumple con el filtro especificado o null si no se encuentra ninguna.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado proyectado.</typeparam>
        /// <param name="select">Expresión para proyectar las entidades.</param>
        /// <param name="filter">Expresión para filtrar las entidades (opcional).</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir (opcional).</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios (opcional).</param>
        /// <param name="cancellationToken">Token de cancelación (opcional).</param>
        /// <returns>La primera entidad proyectada encontrada o null.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si la expresión de proyección es null.</exception>
        public virtual async Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(select, nameof(select));

            var query = GetProjectedQueryable(select, filter, includeProperties, trackChanges);
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Agrega una nueva entidad.
        /// </summary>
        /// <param name="entity">Entidad a agregar.</param>
        public virtual void Add(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _dbSet.Add(entity);
        }

        /// <summary>
        ///     Agrega una nueva entidad de forma asíncrona.
        /// </summary>
        /// <param name="entity">Entidad a agregar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Agrega un conjunto de entidades de forma asíncrona.
        /// </summary>
        /// <param name="entities">Conjunto de entidades a agregar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            await _dbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Actualiza una entidad existente.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        public virtual void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        ///     Actualiza un conjunto de entidades existentes.
        /// </summary>
        /// <param name="entity">Conjunto de entidades a actualizar.</param>
        public virtual void UpdateRange(IEnumerable<TEntity> entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _dbSet.UpdateRange(entity);
        }

        /// <summary>
        ///     Elimina una entidad existente.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        public virtual void Remove(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _dbSet.Remove(entity);
        }

        /// <summary>
        ///     Elimina un conjunto de entidades existentes.
        /// </summary>
        /// <param name="entity">Conjunto de entidades a eliminar.</param>
        public virtual void RemoveRange(IEnumerable<TEntity> entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            _dbSet.RemoveRange(entity);
        }

        /// <summary>
        ///     Obtiene el número de entidades que cumplen con el filtro especificado.
        /// </summary>
        /// <param name="filter">Expresión para filtrar las entidades.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de entidades encontradas.</returns>
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Verifica si existe alguna entidad que cumple con el predicado especificado.
        /// </summary>
        /// <param name="predicate">Expresión para verificar la existencia.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>True si existe alguna entidad, false en caso contrario.</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
            return await _dbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Obtiene un IQueryable para realizar consultas sobre las entidades.
        /// </summary>
        /// <returns>IQueryable de entidades.</returns>
        public virtual IQueryable<TEntity> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        ///     Obtiene un IQueryable con filtros, propiedades incluidas y configuración de rastreo.
        /// </summary>
        /// <param name="filter">Expresión para filtrar las entidades.</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir.</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios.</param>
        /// <returns>IQueryable configurado.</returns>
        public virtual IQueryable<TEntity> GetQueryable(
            Expression<Func<TEntity, bool>>? filter,
            string includeProperties = "",
            bool trackChanges = false)
        {
            var query = _dbSet.AsQueryable();

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = ApplyIncludes(query, includeProperties);

            return query;
        }

        /// <summary>
        ///     Obtiene un IQueryable proyectado con filtros, propiedades incluidas y configuración de rastreo.
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultado proyectado.</typeparam>
        /// <param name="select">Expresión de proyección.</param>
        /// <param name="filter">Expresión para filtrar las entidades.</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir.</param>
        /// <param name="trackChanges">Indica si se deben rastrear los cambios.</param>
        /// <returns>IQueryable proyectado.</returns>
        public virtual IQueryable<TResult> GetProjectedQueryable<TResult>(
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, bool>>? filter = null,
            string includeProperties = "",
            bool trackChanges = false)
        {
            ArgumentNullException.ThrowIfNull(select, nameof(select));

            return GetQueryable(filter, includeProperties, trackChanges)
                .Select(select);
        }

        /// <summary>
        ///     Elimina una entidad por su identificador único tipo GUID de forma asíncrona.
        /// </summary>
        /// <param name="id">Identificador único de la entidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de filas afectadas.</returns>
        public virtual async Task<int> ExecuteDeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Id.Equals(id))
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Elimina una entidad por su identificador único tipo INT de forma asíncrona.
        /// </summary>
        /// <param name="id">Identificador único de la entidad.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de filas afectadas.</returns>
        public virtual async Task<int> ExecuteDeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Id.Equals(id))
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Guarda los cambios realizados en el contexto de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de filas afectadas.</returns>
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        ///--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation IDisposable & ApplyIncludes

        /// <summary>
        ///     Aplica las propiedades incluidas a un IQueryable.
        /// </summary>
        /// <param name="query">IQueryable base.</param>
        /// <param name="includeProperties">Propiedades relacionadas a incluir.</param>
        /// <returns>IQueryable con las propiedades incluidas.</returns>
        private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, string includeProperties)
        {
            if (string.IsNullOrEmpty(includeProperties))
                return query;

            return includeProperties
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        /// <summary>
        ///     - Implementación IDisposable (Libera los recursos utilizados por el Repositorio)
        /// </summary>
        ///
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _context.Dispose();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
