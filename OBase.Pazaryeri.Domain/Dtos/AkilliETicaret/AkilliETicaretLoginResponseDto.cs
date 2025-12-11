namespace OBase.Pazaryeri.Domain.Dtos.AkilliETicaret
{
    public class AkilliETicaretLoginResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public bool Status { get; set; }
    }
}
