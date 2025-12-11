namespace OBase.Pazaryeri.Domain.Dtos.CommonDtos
{
    public class TokenDto : LoginResponseDto
    {   
        public string[] Roles { get; set; }
        public string[] AllowedServices { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string SupplierId { get; set; }      
        public DateTime ExpiresAt { get; set; }
    }
}
