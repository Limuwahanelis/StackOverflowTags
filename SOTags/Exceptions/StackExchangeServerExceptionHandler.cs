using Microsoft.AspNetCore.Diagnostics;

namespace SOTags.Exceptions
{
    public class StackExchangeServerExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is not StackExchangeServerCouldNotBeReachedException stackExchangeServerCouldNotBeReachedException)
            {
                return false;
            }
            await httpContext.Response.WriteAsJsonAsync(
                $"An problem occured when reaching Stack Exchange server. Message from server: {stackExchangeServerCouldNotBeReachedException.StackExchangeSetverMessage}\n" +
                $"Managed to {stackExchangeServerCouldNotBeReachedException.OperationMessage}"
                );
            return true;
        }
    }
}
