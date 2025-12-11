namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEUnDeliverRequestDto
    {
        public class Root
        {
            public DateTime undeliveredDate { get; set; }
            public string undeliveredReason { get; set; }
        }
    }
}