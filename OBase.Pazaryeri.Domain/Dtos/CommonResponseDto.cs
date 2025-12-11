using System.Net;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class CommonResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; } =HttpStatusCode.OK;
    }
}