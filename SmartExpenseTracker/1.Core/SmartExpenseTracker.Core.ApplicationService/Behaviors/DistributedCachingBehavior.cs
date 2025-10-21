using MediatR;
using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.Domain.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Behaviors
{
    public class DistributedCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : class
    {
       
        private readonly IDistributedCacheService _cache;
        private readonly ILogger<DistributedCachingBehavior<TRequest, TResponse>> _logger;

        public DistributedCachingBehavior(
            IDistributedCacheService cache,
            ILogger<DistributedCachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheableQuery)
                return await next();

            var cacheKey = cacheableQuery.CacheKey;

            try
            {
                var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey, cancellationToken);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    return cachedResponse;
                }

                _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

                var response = await next();

                if (response != null)
                {
                    await _cache.SetAsync(cacheKey, response, cacheableQuery.CacheDuration, cancellationToken);
                    _logger.LogDebug("Cached response for key: {CacheKey} with duration: {Duration}",
                        cacheKey, cacheableQuery.CacheDuration);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache operation failed for key: {CacheKey}, executing without cache", cacheKey);
                // If cache fails, execute normally
                return await next();
            }
        }

    }
    
}
