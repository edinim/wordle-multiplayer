using Microsoft.Extensions.Caching.Memory;

namespace Services
{
    public class MemoryCacheService
    {

        private IMemoryCache _memoryCache;
        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetAsync<T>(string key, TimeSpan expire, Func<T> getFunc)
        {
            T data = default!;

            try
            {
                if (_memoryCache.TryGetValue(key, out data))
                    return data;

                data = getFunc();
            }
            catch (Exception ex)
            {
                return data;
            }

            if (data != null)
                _memoryCache.Set(key, data, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expire));

            return data;

        }

        public async Task<bool> SetAsync<T>(string key, TimeSpan expire, T data){

            try {
            if (data != null)
                _memoryCache.Set(key, data, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expire));

            } catch (Exception ex) {
                return false;
            }
            return true;
        }

        public void Remove(string key) {
            _memoryCache.Remove(key);
        }
    }
}