using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OBase.Pazaryeri.Business.BackgroundJobs
{
	public static class HangfireHelper
	{
		public static IServiceCollection ConfigureHangfire(this IServiceCollection services)
		{
			services.AddHangfire(configuration => configuration
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseSQLiteStorage());
			services.AddHangfireServer(x =>
			{
			});
			return services;
		}

		public static IApplicationBuilder UseHangFire(this IApplicationBuilder app)
		{
			app.UseHangfireDashboard("/serverjobs", new DashboardOptions
			{
				DashboardTitle = "Zamanlanmış İşler",
				AppPath = "/serverjobs",
			});
			return app;
		}
	}
}
