using System.Net;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Business.LogHelper;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Domain.Dtos.Getir.Login;
using OBase.Pazaryeri.Core.Utility;

namespace OBase.Pazaryeri.Business.Services.Abstract.General
{
    public class GetirCarsiLoginService : IGetirCarsiLoginService
    {
        #region Variables

        private readonly IOptions<AppSettings> _appSetting;
        private readonly IGetirCarsiClient _getirCarsiClient;
        private readonly IMailService _mailService;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(PazarYerleri.GetirCarsi);

        #endregion

        #region Ctor

        public GetirCarsiLoginService(IGetirCarsiClient getirCarsiClient, IOptions<AppSettings> options, IMailService mailService)
        {
            _getirCarsiClient = getirCarsiClient;

            _appSetting = options;
            _apiDefinition = _appSetting.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.GetirCarsi);
            _mailService = mailService;
        }
        #endregion
        public async Task<ServiceResponse<LoginGetirResponse<string>>> ResetPassword(string newPassword)
        {
            try
            {
                var responseClient = await _getirCarsiClient.ResetPassword(new ResetPasswordDto() { newPassword = newPassword, username = _apiDefinition.ApiUser.Username, oldPassword = _apiDefinition.ApiUser.Password });
                var responseContent = responseClient?.GetContent();
                string qpMessage = responseContent?.Meta?.returnMessage ?? "";
                string returnCode = responseContent?.Meta?.returnCode ?? "";

                if (responseClient.ResponseMessage.StatusCode == HttpStatusCode.OK) {                  
                    return ServiceResponse<LoginGetirResponse<string>>.Success(data: new LoginGetirResponse<string> { Message = "Şifre değiştirme işlemleri tamamlandı.", Success = true, Data = Cipher.EncryptString(newPassword) });
                }
                else {                    
                    string message = $"Sifre değiştirme sırasında bir hata oluştu. Hata : {qpMessage}, StatusCode : {responseClient.ResponseMessage.StatusCode}, Return Code : {returnCode}";
                    Logger.Error("Getir Reset Password Error: {exception} - response : {responseContent}", fileName: _logFolderName, message, responseContent);
                    return ServiceResponse<LoginGetirResponse<string>>.Error(errorMessage: message, httpStatusCode: responseClient.ResponseMessage.StatusCode);
                }

            }
            catch (Exception ex)
            {

                if ((_appSetting.Value.MailSettings?.MailEnabled ?? false))
                {
                    await _mailService.SendMailAsync(_logFolderName + $" Hata! Şifre değiştirilemedi.", ex.Message + " " + ex.InnerException?.Message ?? "");
                }
                Logger.Error("Getir Reset Password Error: {exception}", fileName: _logFolderName, ex);
                return ServiceResponse<LoginGetirResponse<string>>.Error(errorMessage: "Sifre değiştirme sırasında bir hata oluştu. Lütfen tekrar deneyiniz.", httpStatusCode: HttpStatusCode.InternalServerError);
            }


        }


    }
}