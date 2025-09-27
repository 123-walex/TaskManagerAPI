using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json;
using TaskManagerAPI.Data;

namespace TaskManagerAPI.Services.Idempotency
{
    public class RedisIdempotencyStore : IIdempotencyStore 
    {
        //Injects the redis db like my normal _context 
        private readonly IDatabase _db;
        private readonly ILogger<RedisIdempotencyStore> _logger;

        public RedisIdempotencyStore(IConnectionMultiplexer redis , ILogger<RedisIdempotencyStore> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<CatchedResponse?> GetResponseAsync(string Key)
        {
            var response = await _db.StringGetAsync(Key);

            if(response.IsNullOrEmpty)
            {
                _logger.LogWarning("No response in the cache yet , retuning null");
                return null;
            }

            _logger.LogInformation("Successfully Retrieved Response from cache");
            return System.Text.Json.JsonSerializer.Deserialize<CatchedResponse>(response!);
        }
        public async Task SaveResponseAsync(string Key , CatchedResponse response)
        {
            var JsonResponse = System.Text.Json.JsonSerializer.Serialize(response);

            // stores the response in the db for 24 hours then deletes .
            await _db.StringSetAsync(Key , JsonResponse , TimeSpan.FromHours(24));
            _logger.LogInformation("Successfully stored response in cache");
        }
    }
}
