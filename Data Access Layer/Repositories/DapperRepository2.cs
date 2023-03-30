using Dapper;
using Data_Access_Layer.Common.Repositories;
using Data_Access_Layer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Data_Access_Layer.Repositories
{
    /// <summary>
    /// Изначально писался только Create метод
    /// Update просто по аналогии
    /// лень оптимизировать
    /// </summary>

    public class DapperRepository2 : IDapperRepository2
    {
        protected bool _disposed = false;
        private IDbConnection _db;

        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<DapperRepository2> _logger;
        protected TransactionScope _transaction;

        public DapperRepository2(ApplicationDbContext context, ILogger<DapperRepository2> logger)
        {
            _db = context.Database.GetDbConnection();
            _context = context;
            _logger = logger;
        }

        public Task<IEnumerable<T>> Sql<T>(string item)
        {
            _logger.LogDebug("Sql<T>(string item)");
            return _db.QueryAsync<T>(item);
        }

        public Task<IEnumerable<T>> Sql<T>(string item, object items)
        {
            _logger.LogDebug("Sql<T>(string item)");
            return _db.QueryAsync<T>(item, items);
        }

        public void Create<T>(params T[] items) where T : class
        {
            _logger.LogDebug("Create<T>(params T[] items)");
            if (items == null || items.Length == 0)
                return;
            string sql = InsertArrayStringsToQuery(items);
            Open();
            _db.Execute(sql);
        }

        public void Update<T>(T item) where T : class
        {
            _logger.LogDebug("Update<T>(T item)");
            if (item == null)
                return;
            string sql = UpdateArrayStringsToQuery(item);

            Open();
           _db.Execute(sql);
        }

        private string InsertArrayStringsToQuery<T>(params T[] items) where T : class
        {
            var stringQuery = FieldsToArrayStrings(
                items,
                _context.Model.FindEntityType(typeof(T)),
                _context.Database.GetDbConnection().CreateCommand());
            StringBuilder sql = new StringBuilder(23 + stringQuery.length);

            sql.Append($"INSERT INTO {stringQuery.tableName} ({string.Join(",", stringQuery.fields)}) VALUES ");
            for (var i = 0; i < stringQuery.values.Length - 1; i++)
                sql.Append($"({stringQuery.values[i]}),");
            sql.Append($"({stringQuery.values[stringQuery.values.Length - 1]});");
            return sql.ToString();
        }

        private string UpdateArrayStringsToQuery<T>(T item) where T : class
        {
            var stringQuery = FieldsToArrayStrings(
                item,
                _context.Model.FindEntityType(typeof(T)),
                _context.Database.GetDbConnection().CreateCommand());
            StringBuilder sql = new StringBuilder(23 + stringQuery.values[0].Length + stringQuery.length + stringQuery.length * 4);

            sql.Append($"UPDATE {stringQuery.tableName} SET");
            for (var i = 0; i < stringQuery.values.Length - 1; i++)
            {
                sql.Append($"{stringQuery.fields[i]} = {stringQuery.values[i]},");
            }
            sql.Append($"{stringQuery.fields[stringQuery.values.Length - 1]} = {stringQuery.values[stringQuery.values.Length - 1]}");
            sql.Append($" WHERE ");
            sql.Append($"\"Id\" = {stringQuery.values[0]};");//тут надо было достать первичный ключ из _context...но мне лень
            return sql.ToString();
        }


        private (string[] fields, string[] values, string tableName, int length) FieldsToArrayStrings<T>(
            T[] items, IEntityType entity, DbCommand command) where T : class
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            int length = 0;
            length += entity.GetTableName().Length + 4;//длина названия таблицы
            IProperty[] propertys = entity.GetProperties().ToArray();
            var fields = propertys.Select(x =>
            {
                length += x.GetColumnName().Length + 4;//длина название столбца
                return "\"" + x.GetColumnName() + "\"";
            }).ToArray();

            string[] vs = new string[items.Length];//массив для (value1,value2),(value1,value2)...
            string[] vs2 = new string[propertys.Length];//массив для (value1,value2)
            for (var i = 0; i < items.Length; i++)
            {
                for (var q = 0; q < propertys.Length; q++)
                {
                    var nameField = items[i].GetType().GetProperty(propertys[q].Name);
                    var valueField = propertys[q].FindRelationalTypeMapping()
                    .CreateParameter(command, nameField.Name, nameField.GetValue(items[i], null));

                    length += valueField.Value.ToString().Length + 4;//длина значения столбца
                    vs2[q] = "'" + valueField.Value + "'";
                }
                vs[i] = string.Join(",", vs2);
            }
            length += vs.Length * 4;
            return (fields, vs, $"\"{entity.GetTableName()}\"", length);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private (string[] fields, string[] values, string tableName, int length) FieldsToArrayStrings<T>(
            T item, IEntityType entity, DbCommand command) where T : class
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            int length = 0;
            length += entity.GetTableName().Length + 4;//длина названия таблицы
            IProperty[] propertys = entity.GetProperties().ToArray();
            var fields = propertys.Select(x =>
            {
                length += x.GetColumnName().Length + 4;//длина название столбца
                return "\"" + x.GetColumnName() + "\"";
            }).ToArray();

            string[] vs2 = new string[propertys.Length];//массив для (value1,value2)
            for (var q = 0; q < propertys.Length; q++)
            {
                var nameField = item.GetType().GetProperty(propertys[q].Name);
                var valueField = propertys[q].FindRelationalTypeMapping()
                .CreateParameter(command, nameField.Name, nameField.GetValue(item, null));

                length += valueField.Value.ToString().Length + 4;//длина значения столбца
                vs2[q] = "'" + valueField.Value + "'";
            }
            return (fields, vs2, $"\"{entity.GetTableName()}\"", length);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void Save()
        {
            _transaction?.Complete();
            _transaction?.Dispose();
            _transaction = null;
        }

        private void Open()
        {
            if (_transaction == null)
                _transaction = new TransactionScope();
            switch (_db.State)
            {
                case ConnectionState.Closed:
                    {
                        _db.Open();
                        break;
                    }
                case ConnectionState.Open:
                    break;
                case ConnectionState.Executing:
                    break;
                case ConnectionState.Fetching:
                    break;
                case ConnectionState.Broken:
                    {
                        _db.Close();
                        _db.Open();
                        break;
                    }
            }
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
                    _transaction?.Dispose();
                    _db?.Dispose();
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DapperRepository2()
        {
            Dispose();
        }
    }
}

