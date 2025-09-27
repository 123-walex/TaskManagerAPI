namespace TaskManagerAPI.Services.Idempotency
{
    public class IdempotencyMiddleware
    {
        private readonly ILogger<IdempotencyMiddleware> _logger;
        private readonly RequestDelegate _requestDelegate;
        private readonly IIdempotencyStore _store;

        public IdempotencyMiddleware(ILogger<IdempotencyMiddleware> logger , RequestDelegate requestDelegate , IIdempotencyStore store)
        {
            _logger = logger;
            _requestDelegate = requestDelegate;
            _store = store;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the idempotency key 
            if(!context.Request.Headers.TryGetValue("Idempotency-Key" , out var Key))
            {
                await _requestDelegate(context); // just continue down the pipeline
                _logger.LogInformation("Idempotency Key Not Provided or found");
                return;
            }

            //Try to get the response attached to the key 
            var response = await _store.GetResponseAsync(Key!);
            if(response != null)
            {
                _logger.LogInformation($"Idempotency Hit for key {Key}");

                context.Response.StatusCode = response.StatusCode;
                foreach(var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }

                await context.Response.WriteAsync(response.Body);
                return;
            }
            // capture response for saving 
            var originalstream = context.Response.Body;
            using var stream = new MemoryStream();
            context.Response.Body = stream;

            await _requestDelegate(context); // request the process nomally

            //read response from memory 
            stream.Seek(0, SeekOrigin.Begin);
            var responsebody = await new StreamReader(stream).ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);

            // Save to redis 
            await _store.SaveResponseAsync(Key!, new CatchedResponse
            {
                StatusCode = context.Response.StatusCode,
                Headers = context.Response.Headers.ToDictionary(H => H.Key, H => H.Value.ToString()),
                Body = responsebody
            });

            // Copy response back to stream 
            await stream.CopyToAsync(originalstream);
        }
    }
}
