using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.QuickPick
{
	public class CommonQpDto<T> where T : class
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public T Data { get; set; }
	}
}
