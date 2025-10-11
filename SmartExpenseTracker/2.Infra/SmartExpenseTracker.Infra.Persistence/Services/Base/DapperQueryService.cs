using Dapper;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Persistence;
using SmartExpenseTracker.Infra.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Services.Base
{
    public class DapperQueryService : IDapperQueryService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperQueryService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.QueryFirstOrDefaultAsync<T>(command);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.QueryAsync<T>(command);
        }

        public async Task<T> QuerySingleAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.QuerySingleAsync<T>(command);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object? parameters = null,
            string splitOn = "Id",
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.QueryAsync(sql, map, parameters, splitOn: splitOn);
        }

        public async Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.ExecuteAsync(command);
        }

        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            return await connection.ExecuteScalarAsync<T>(command);
        }
    }

}
