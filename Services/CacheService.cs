using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestTask;

public class CacheService
{
    private IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor httpContextAccessor;

    public string _ip => httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "---";

    public CacheService(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
    {
        _memoryCache = memoryCache;
        this.httpContextAccessor = httpContextAccessor;
    }


    //private readonly MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
    //            .SetSlidingExpiration(TimeSpan.FromSeconds(15))
    //            .SetAbsoluteExpiration(DateTime.UtcNow.AddSeconds(30));


    public void CacheFilters(SearchFilters filters)
    {
        _memoryCache.Set<SearchFilters>(_ip, filters);
    }

    public SearchFilters GetCachedFilters()
    {
        try
        {
            if (_memoryCache.TryGetValue(_ip, out SearchFilters filters))
                return filters;
        }
        catch (Exception ex)
        {
        }
        return null;
    }

}
