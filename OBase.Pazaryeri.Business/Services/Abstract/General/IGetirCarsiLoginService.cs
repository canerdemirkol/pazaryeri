using OBase.Pazaryeri.Domain.Dtos.Getir;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBase.Pazaryeri.Domain.Dtos.Getir.Login;
using OBase.Pazaryeri.Domain.Dtos;

namespace OBase.Pazaryeri.Business.Services.Abstract.General
{
    public interface IGetirCarsiLoginService
    {
        Task<ServiceResponse<LoginGetirResponse<string>>> ResetPassword(string newPassword);        
    }
}
