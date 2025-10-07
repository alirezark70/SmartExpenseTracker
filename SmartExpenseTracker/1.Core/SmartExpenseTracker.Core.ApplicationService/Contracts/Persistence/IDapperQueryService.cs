using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence
{
    public interface IDapperQueryService
    {
        Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default);

        Task<T> QuerySingleAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object? parameters = null,
            string splitOn = "Id",
            CancellationToken cancellationToken = default);

        Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default);

        Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default);
    }

}
