using Microsoft.AspNetCore.Http;

namespace OBase.Pazaryeri.Business.Middleware
{
    public class BearerTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        
        public BearerTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;           
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.ToString().ToLower().Contains("serverjobs"))
            {
                await _next(context);
            }
            await _next(context);
        }
    }
}
