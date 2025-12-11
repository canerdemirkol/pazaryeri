using Microsoft.Extensions.DependencyInjection;

namespace OBase.Pazaryeri.Domain.Configurations.MappingConfigurations
{
	public static class MapperHelper
	{
		public static IServiceCollection ConfigureMapper(this IServiceCollection services)
		{
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			return services;
		}
	}
}
