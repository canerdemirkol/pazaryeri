namespace OBase.Pazaryeri.Domain.Dtos.Getir.Login
{
    public class ResetPasswordDto: NewPasswordDto
    {
        public string username { get; set; }
        public string oldPassword { get; set; }
    }

    public class NewPasswordDto
    {
        public string newPassword { get; set; }
    }
}
