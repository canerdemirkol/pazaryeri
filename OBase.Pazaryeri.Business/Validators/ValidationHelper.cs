using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OBase.Pazaryeri.Domain.Dtos;

namespace OBase.Pazaryeri.Business.Validators
{
	public static class ValidationHelper
	{
		public static void AddFluentValidationEx(this IServiceCollection services)
		{
			services.AddFluentValidationAutoValidation();
			services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState.Values.Where(x => x.Errors.Count > 0).SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

					return new BadRequestObjectResult(ServiceResponse.Error(errorMessages: errors));
				};
			});
		}
	}
}
