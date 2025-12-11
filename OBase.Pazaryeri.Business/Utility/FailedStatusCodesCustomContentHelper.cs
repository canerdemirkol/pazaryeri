using Microsoft.AspNetCore.Http;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace OBase.Pazaryeri.Business.Utility
{
    public static class FailedStatusCodesCustomContentHelper
    {
        public static async Task UnauthorizedStatusCodeAddContent(this HttpContext context, object result, HttpStatusCode httpStatusCode)
        {
            context.Response.ContentType = ContentType.Json;
            context.Response.StatusCode = (int)httpStatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
