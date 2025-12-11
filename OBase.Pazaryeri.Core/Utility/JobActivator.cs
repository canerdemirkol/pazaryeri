using Microsoft.Extensions.DependencyInjection;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Core.Concrete.BackgroundJob;

namespace OBase.Pazaryeri.Core.Utility
{
	public static class RecurringJobsActivator
	{
        public static void ConfigureRecuringJobs(this IServiceCollection services)
        {
            var x = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(a => a.GetInterfaces().Contains(typeof(IBackgroundJob)) && !a.IsAbstract && !a.IsInterface)
            .Select(a => new { assignedType = a, serviceTypes = a.GetInterfaces().ToList() }).ToList();
            x.ForEach(typesToRegister =>
            {
                services.AddScoped(typeof(IBackgroundJob), typesToRegister.assignedType);
            });
        }

        public static void Start(this IServiceProvider serviceProvider, List<ScheduledTask> configs)
		{
			if (configs is not null)
			{
				var services = serviceProvider.GetServices<IBackgroundJob>();

				foreach (var instance in services)
				{
					var jobs = configs.Where(x => x.JobId.Equals(instance.GetType().Name));
					foreach (var job in jobs)
					{
						if (job.Status)
						{
							instance.StartJob(job.Properties,job.CronExpression, job.JobName ?? job.JobId);
						}
						else
						{
							instance.StopJob(job.JobName ?? job.JobId);
						}
					}
				}
			}
		}
	}
}
