using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Domain.Dtos;
using System.Net;

namespace OBase.Pazaryeri.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public abstract class BaseController : ControllerBase
	{
		protected IActionResult ResponseResult<T>(ServiceResponse<T> result)
		{
			return result.HttpStatusCode switch
			{
				HttpStatusCode.OK => Ok(result),
				HttpStatusCode.Created => Created(result.CreatedUri, result),
				HttpStatusCode.Accepted => Accepted(result),
				HttpStatusCode.NoContent => NoContent(),
				HttpStatusCode.BadRequest => BadRequest(result),
				HttpStatusCode.Unauthorized => Unauthorized(result),
				HttpStatusCode.Forbidden => Forbid(),
				HttpStatusCode.NotFound => NotFound(result),
				HttpStatusCode.MethodNotAllowed => NotFound(result),
				HttpStatusCode.Conflict => Conflict(result),
				HttpStatusCode.InternalServerError => StatusCode(500, result),
				_ => Ok(result),
			};
		}
	}
}