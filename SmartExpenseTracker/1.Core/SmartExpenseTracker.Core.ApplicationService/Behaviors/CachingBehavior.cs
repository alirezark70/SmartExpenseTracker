using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IMemoryCache _cache;

        public CachingBehavior(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheableQuery)
                return await next();

            var cacheKey = cacheableQuery.CacheKey;

            if (_cache.TryGetValue<TResponse>(cacheKey, out var cachedResponse))
                return cachedResponse!;

            var response = await next();

            _cache.Set(cacheKey, response, cacheableQuery.CacheDuration);

            return response;
        }
    }

}
