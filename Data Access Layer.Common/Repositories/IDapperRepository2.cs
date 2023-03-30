using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access_Layer.Common.Repositories
{
    public interface IDapperRepository2 : IDisposable
    {
        Task<IEnumerable<T>> Sql<T>(string item);
        Task<IEnumerable<T>> Sql<T>(string item, object cars);

        void Create<T>(params T[] items) where T : class;
        void Update<T>(T items) where T : class;

        void Save();
    }
}
