using Microsoft.AspNetCore.Diagnostics;

namespace SOTags.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            Console.Write(exception.ToString());
            httpContext.Response.StatusCode=StatusCodes.Status500InternalServerError;
            return true;
        }
    }
}
