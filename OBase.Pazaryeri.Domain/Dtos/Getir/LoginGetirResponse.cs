namespace OBase.Pazaryeri.Domain.Dtos.Getir
{
    public class LoginGetirResponse<T> : CommonResponseDto
    {
        public T Data { get; set; }

    }

}
