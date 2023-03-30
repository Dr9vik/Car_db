using Data_Access_Layer.Common.Repositories;
using Data_Access_Layer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Data_Access_Layer.Repositories
{
    public class Repository2: IRepository2
    {
        protected readonly ApplicationDbContext _context;
        protected bool _disposed = false;

        protected readonly ILogger<Repository2> _logger;

        public Repository2(ApplicationDbContext context, ILogger<Repository2> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Create<T>(T item) where T : class
        {
            _logger.LogInformation("Create");
            _context.Set<T>().Add(item);
        }

        public void Create<T>(IList<T> items, bool autoDetectChangesEnabled = false) where T : class
        {
            _logger.LogInformation("Create");
            _context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            _context.Set<T>().AddRange(items);
        }

        public void Update<T>(T item) where T : class
        {
            _logger.LogInformation("Update");
            _context.Entry(item).State = EntityState.Modified;
        }

        public void UpdateAll<T>(IList<T> items) where T : class
        {
            _logger.LogInformation("UpdateAll");
            foreach (var item in items)
            {
                _context.Entry(item).State = EntityState.Modified;
            }
        }

        public void Delete<T>(string id) where T : class
        {
            _logger.LogInformation("Delete id {id}", id);
            var group = _context.Set<T>().Find(id);
            if (group != null)
                _context.Set<T>().Remove(group);
        }

        public void Delete<T>(T item) where T : class
        {
            _logger.LogInformation("Delete<T>(T item)", item);
            _context.Entry(item).State = EntityState.Deleted;
        }

        public void DeleteAll<T>(IList<T> items) where T : class
        {
            _logger.LogInformation("DeleteAll");
            for(int i=0; i< items.Count;i++ )
            {
                _context.Entry(items[i]).State = EntityState.Deleted;
            }
        }
        public IQueryable<T> Sql<T>(string item) where T : class
        {
            _logger.LogDebug("Sql");
            return _context.Set<T>().FromSqlRaw(item);
        }
        public IQueryable<T> Sql<T>(FormattableString item) where T : class
        {
            _logger.LogDebug("Sql");
            return _context.Set<T>().FromSqlInterpolated(item);
        }

        public IQueryable<T> Sql<T>(string sql, params object[] parameters) where T : class
        {
            _logger.LogDebug("Sql");
            return _context.Set<T>().FromSqlRaw(sql, parameters);
        }

        public T Get<T>(string id) where T : class
        {
            _logger.LogDebug("Get");
            return _context.Set<T>().Find(id);
        }

        public IQueryable<T> GetAll<T>() where T : class
        {
            _logger.LogDebug("GetAll");
            return _context.Set<T>().AsQueryable();
        }

        public IQueryable<T> Find<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            _logger.LogDebug("Find");
            return _context.Set<T>().Where(predicate);
        }

        public void Save()
        {
            _logger.LogInformation("Save");
            _context.SaveChanges();
        }
        public Task<int> SaveAsync()
        {
            _logger.LogInformation("Save");
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        ~Repository2()
        {
            Dispose();
        }
    }
}
