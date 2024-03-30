using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using SOTags.Controllers;

namespace SOTags.Exceptions
{
    public class StackExchangeServerExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<StackExchangeServerExceptionHandler> _logger;

        public StackExchangeServerExceptionHandler(ILogger<StackExchangeServerExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is not StackExchangeServerCouldNotBeReachedException stackExchangeServerCouldNotBeReachedException)
            {
                return false;
            }
            string message = $"An problem occured when reaching Stack Exchange server. Message from server: {stackExchangeServerCouldNotBeReachedException.StackExchangeSetverMessage}\n" +
                $"Managed to {stackExchangeServerCouldNotBeReachedException.OperationMessage}";
            await httpContext.Response.WriteAsJsonAsync(message);
            Log.Error("{message}",message);
            return true;
        }
    }
}
