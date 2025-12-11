using Hangfire;

namespace OBase.Pazaryeri.Core.Abstract.BackgroundJob
{
    public interface IBackgroundJob
    {
        Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken);
        internal protected void StartJob(Dictionary<string, string> properties = null, string chronExpression = null, string jobName = null)
        {
            TimeZoneInfo timeZone;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
            
            RecurringJob.AddOrUpdate(recurringJobId: jobName ?? GetType().Name, () => RunJobAsync(properties, JobCancellationToken.Null), chronExpression, new RecurringJobOptions { TimeZone = timeZone });
        }
        internal protected void StopJob(string jobName = null)
        {
            RecurringJob.RemoveIfExists(recurringJobId: jobName ?? GetType().Name);
        }
    }
}