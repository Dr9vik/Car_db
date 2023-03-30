using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Common.Repositories
{
    public interface IRepository2 : IDisposable
    {
        void Create<T>(T item) where T : class;

        void Create<T>(IList<T> items, bool autoDetectChangesEnabled) where T : class;

        void Update<T>(T item) where T : class;

        void UpdateAll<T>(IList<T> items) where T : class;

        void Delete<T>(string id) where T : class;

        void Delete<T>(T item) where T : class;

        void DeleteAll<T>(IList<T> items) where T : class;

        IQueryable<T> Sql<T>(string item) where T : class;

        IQueryable<T> Sql<T>(FormattableString item) where T : class;

        IQueryable<T> Sql<T>(string sql, params object[] parameters) where T : class;

        T Get<T>(string id) where T : class;

        IQueryable<T> GetAll<T>() where T : class;

        IQueryable<T> Find<T>(Expression<Func<T, bool>> predicate) where T : class;

        void Save();

        Task<int> SaveAsync();
    }
}
