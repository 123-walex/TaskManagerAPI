namespace TaskManagerAPI.Services.Idempotency
{
    public interface IIdempotencyStore
    {
        Task<CatchedResponse?> GetResponseAsync(string key);
        Task SaveResponseAsync(string key , CatchedResponse Response);
    }
}
