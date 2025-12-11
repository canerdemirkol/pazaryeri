namespace OBase.Pazaryeri.Core.Concrete.BackgroundJob
{
	public class ScheduledTask
	{
		public string JobId { get; set; }
		public string JobName { get; set; }
		public string CronExpression { get; set; }
		public Dictionary<string, string> Properties { get; set; }
		public bool Status { get; set; }
    }

}