namespace TaskManagerAPI.Services.Idempotency
{
    public class CatchedResponse
    {
        public int StatusCode{ get; set; }
        public Dictionary<string, string> Headers { get; set; } =  new ();
        public string Body { get; set; } = string.Empty;
    }
}
