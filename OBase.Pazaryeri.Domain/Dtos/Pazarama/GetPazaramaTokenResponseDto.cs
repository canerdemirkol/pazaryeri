namespace OBase.Pazaryeri.Domain.Dtos.Pazarama
{
    public class GetPazaramaTokenResponseDto
    {
        public class Data
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; }
            public string Scope { get; set; }
        }
    }
}
