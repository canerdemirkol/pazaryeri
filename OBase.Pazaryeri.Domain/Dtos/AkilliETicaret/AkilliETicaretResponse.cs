namespace OBase.Pazaryeri.Domain.Dtos.AkilliETicaret
{
    public class AkilliETicaretResponse<T>
    {
        public T Data { get; set; }
        public bool Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
