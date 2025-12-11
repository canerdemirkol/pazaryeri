using System.Net;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos
{
	public class ServiceResponse
	{
		[JsonPropertyOrder(1)]
		public bool IsSuccessful { get; set; }

		[JsonIgnore]
		public HttpStatusCode HttpStatusCode { get; internal set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<string> ErrorMessages { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string CreatedUri { get; set; }
		public static ServiceResponse Success(HttpStatusCode httpStatusCode = HttpStatusCode.OK, string createdUri = null)
		{

			return new ServiceResponse
			{
				IsSuccessful = true,
				HttpStatusCode = httpStatusCode,
				CreatedUri = createdUri
			};
		}
		public static ServiceResponse Error(List<string> errorMessages, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
		{
			return new ServiceResponse
			{
				IsSuccessful = false,
				HttpStatusCode = httpStatusCode,
				ErrorMessages = errorMessages,
			};
		}
	}
	public class ServiceResponse<T> : ServiceResponse
	{

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		[JsonPropertyOrder(2)]
		public T Data { get; internal set; }

		public static ServiceResponse<T> Success(T data = default, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
		{
			return new ServiceResponse<T>
			{
				IsSuccessful = true,
				Data = data,
				HttpStatusCode = httpStatusCode,
			};
		}

		public static ServiceResponse<T> Error(string errorMessage, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
		{
			return new ServiceResponse<T>
			{
				IsSuccessful = false,
				HttpStatusCode = httpStatusCode,
				ErrorMessages = new List<string> { errorMessage }
			};
		}
	}
}