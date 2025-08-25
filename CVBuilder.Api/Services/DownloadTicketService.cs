using Microsoft.Extensions.Caching.Memory;

namespace CVBuilder.Api.Services
{
    public class DownloadTicketService : IDownloadTicketService
    {
        private readonly IMemoryCache _cache;
        public DownloadTicketService(IMemoryCache cache) => _cache = cache;

        public string Issue(int userId, int cvId, TimeSpan ttl)
        {
            var token = Guid.NewGuid().ToString("N");
            _cache.Set(token, (userId, cvId), new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
            return token;
        }

        public bool TryConsume(string token, out (int userId, int cvId) payload)
        {
            if (_cache.TryGetValue<(int userId, int cvId)>(token, out payload))
            {
                _cache.Remove(token);
                return true;
            }
            payload = default;
            return false;
        }
    }
}
