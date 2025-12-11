using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Client.Concrete;
using OBase.Pazaryeri.Business.Factories;
using OBase.Pazaryeri.Business.Handlers;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Sale;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Abstract.Product;
using OBase.Pazaryeri.Business.Services.Abstract.Promotion;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Business.Services.Abstract.Quickpick;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.Business.Services.Abstract.Sale;
using OBase.Pazaryeri.Business.Services.Concrete;
using OBase.Pazaryeri.Business.Services.Concrete.General;
using OBase.Pazaryeri.Business.Services.Concrete.Sale;
using OBase.Pazaryeri.Business.Services.Concrete.Order;
using OBase.Pazaryeri.Business.Services.Concrete.Product;
using OBase.Pazaryeri.Business.Services.Concrete.Promotion;
using OBase.Pazaryeri.Business.Services.Concrete.PushPrice;
using OBase.Pazaryeri.Business.Services.Concrete.QuickPick;
using OBase.Pazaryeri.Business.Services.Concrete.Return;
using OBase.Pazaryeri.Business.Validators;
using OBase.Pazaryeri.DataAccess.Utilities;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Configurations.MappingConfigurations;
using OBase.Pazaryeri.Domain.Dtos.Pazarama;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using RestEase.Implementation;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Utility
{
    public static class BusinessServices
    {
        public static IServiceCollection InjectBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITrendyolGoReturnService, TyGoReturnService>();
            services.AddScoped<IMerchantFactory, MerchantFactory>();
            services.AddScoped<ITokenService, TokenService>();

            #region Login
            services.AddScoped<IGetirCarsiLoginService, GetirCarsiLoginService>();
            #endregion

            #region Order
            services.AddScoped<IGetirCarsiOrderService, GetirCarsiOrderService>();
            services.AddScoped<ITrendyolGoOrderService, TrendyolGoOrderService>();
            services.AddScoped<IHepsiExpressOrderService, HepsiExpressOrderService>();
            services.AddScoped<IGetirCarsiProductService, GetirCarsiProductService>();
            services.AddScoped<IYemekSepetiOrderService, YemekSepetiOrderService>();
            services.AddScoped<IYemekSepetiProdcutService, YemekSepetiProdcutService>();
            services.AddScoped<IIdefixOrderService, IdefixOrderService>();
            services.AddScoped<ISaleService, SaleService>();

            services.AddScoped<IOrderConvertService, OrderConvertService>();
            #endregion

            #region Factories
            services.AddScoped<IOrderConverterFactory, OrderConverterFactory>();
            #endregion

            #region Client
            var appSettings = configuration.Get<AppSettings>();

            services.AddMemoryCache(); 
                       
            services.AddScoped<IGetirCarsiReturnService, GetirCarsiReturnService>();

            addYemekSepetiClient(services, appSettings);
            addTrendyolGoClient(services, appSettings);
            addTrendyolClient(services, appSettings);
            addPazaramaClient(services, appSettings);
            addGetirCarsiClient(services,appSettings);
            addHepsiExpressClient(services, appSettings);
            addHepsiExpressOrderClient(services, appSettings);
            addPimIdefixClient(services, appSettings);
            addOsmIdefixClient(services, appSettings);
            addAkilliETicaretClient(services, appSettings);
            #endregion

            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IQuickPickService, QuickPickService>();

            #region PriceStock
            services.AddScoped<ITrendyolGoPushPriceStockService, TrendyolGoPushPriceStockService>();
            services.AddScoped<ITrendyolPushPriceStockService, TrendyolPushPriceStockService>();
            services.AddScoped<IGetirCarsiPushPriceStockService, GetirCarsiPushPriceStockService>();
            services.AddScoped<IYemekSepetiPushPriceStockService, YemekSepetiPushPriceStockService>();
            services.AddScoped<IPazaramaPushPriceStockService, PazaramaPushPriceStockService>();
            services.AddScoped<IHepsiExpressPushPriceStockService, HepsiExpressPushPriceStockService>();
            services.AddScoped<ISharedPriceStockOnlyStockService, SharedPriceStockOnlyStockService>();
            services.AddScoped<IIdefixPushPriceStockService, IdefixPushPriceStockService>();

            #endregion

            #region Promotion
            services.AddScoped<IYemekSepetiPromotionService, YemekSepetiPromotionService>();
            #endregion

            #region General
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<ICheckOrderFlowService, CheckOrderFlowService>();
            #endregion


            services.ConfigureMapper();
            services.AddFluentValidationEx();
            services.InjectDalServices();

            return services;
        }

        private static void addYemekSepetiClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);

            services.AddTransient<YemekSepetiHttpClientHandler>();

            services.AddHttpClient<IYemekSepetiClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                var token = apiDefinition.ApiUser.Token;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            })
            .AddHttpMessageHandler<YemekSepetiHttpClientHandler>()
            .AddTypedClient(client =>
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var restClient = new RestEase.RestClient(client)
                {
                    JsonSerializerSettings = jsonSerializerSettings
                };
                return restClient.For<IYemekSepetiClient>();
            });
        }

        private static void addTrendyolGoClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);            

            services.AddHttpClient<ITrendyolGoClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;

                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                               .GetBytes(username + ":" + password));

                string agentname = apiDefinition.XAgentName;
                string executoruser = apiDefinition.XExecutorUser;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
                client.DefaultRequestHeaders.Add("x-agentname", agentname);
                client.DefaultRequestHeaders.Add("x-executor-user", executoruser);
            })
            .AddTypedClient(client => RestEase.RestClient.For<ITrendyolGoClient>(client));

        }

        private static void addTrendyolClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Trendyol);

            services.AddHttpClient<ITrendyolClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;

                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                               .GetBytes(username + ":" + password));

                string agentname = apiDefinition.XAgentName;
                string executoruser = apiDefinition.XExecutorUser;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
                client.DefaultRequestHeaders.Add("x-agentname", agentname);
                client.DefaultRequestHeaders.Add("x-executor-user", executoruser);
            })
            .AddTypedClient(client => RestEase.RestClient.For<ITrendyolClient>(client));

        }
        private static void addPazaramaClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Pazarama);

            services.AddTransient<PazaramaHttpClientHandler>();
            services.AddHttpClient<IPazaramaClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                        .GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
            })
            .AddHttpMessageHandler<PazaramaHttpClientHandler>() // Retry sadece burada kontrol ediliyor!  // Her HTTP isteğinde `429` kontrolü
            .AddTypedClient((client, serviceProvider) =>
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
            
                var restClient = new RestEase.RestClient(client)
                {
                    JsonSerializerSettings = jsonSerializerSettings
                };
            
                return restClient.For<IPazaramaClient>();
            });
        }

        private static void addGetirCarsiClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.GetirCarsi);
          
            services.AddTransient<GetirCarsiHttpClientHandler>();
            services.AddHttpClient<IGetirCarsiClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(username + ":" + password));


                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
            })
            .AddHttpMessageHandler<GetirCarsiHttpClientHandler>()
            .AddTypedClient(client => RestEase.RestClient.For<IGetirCarsiClient>(client));
        }

        private static void addHepsiExpressClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);

            services.AddTransient<HepsiExpressHttpClientHandler>();

            services.AddHttpClient<IHepsiExpressClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/xml");

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;

                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
            })
           .AddHttpMessageHandler<HepsiExpressHttpClientHandler>()
           .AddTypedClient((client, serviceProvider) =>
            {
                var xmlRequestBodySerializer = new XmlRequestBodySerializer();

                var restClient = new RestEase.RestClient(client)
                {
                    RequestBodySerializer = xmlRequestBodySerializer
                };

                return restClient.For<IHepsiExpressClient>();
            });
        }

        private static void addHepsiExpressOrderClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);

            services.AddTransient<HepsiExpressOrderHttpClientHandler>();

            services.AddHttpClient<IHepsiExpressOrderClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/xml");

                string username = apiDefinition.ApiUser.Username;
                string password = apiDefinition.ApiUser.Password;

                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {encoded}");
            })
           .AddHttpMessageHandler<HepsiExpressOrderHttpClientHandler>()
           .AddTypedClient((client, serviceProvider) =>
           {
               var xmlRequestBodySerializer = new XmlRequestBodySerializer();

               var restClient = new RestEase.RestClient(client)
               {
                   RequestBodySerializer = xmlRequestBodySerializer
               };

               return restClient.For<IHepsiExpressOrderClient>();
           });
        }

        private static void addPimIdefixClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);

            services.AddTransient<IdefixHttpClientHandler>();
            services.AddHttpClient<IPimIdefixClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.PIMDomain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                string apiKey = apiDefinition.ApiUser.ApiKey;
                string secretKey = apiDefinition.ApiUser.SecretKey;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                        .GetBytes($"{apiKey}:{secretKey}"));

                client.DefaultRequestHeaders.Add("x-api-key", $"{encoded}");
            })
            .AddHttpMessageHandler<IdefixHttpClientHandler>() 
            .AddTypedClient((client, serviceProvider) =>
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var restClient = new RestEase.RestClient(client)
                {
                    JsonSerializerSettings = jsonSerializerSettings
                };

                return restClient.For<IPimIdefixClient>();
            });
        }

        private static void addOsmIdefixClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);

            services.AddTransient<IdefixHttpClientHandler>();
            services.AddHttpClient<IOsmIdefixClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.OSMDomain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                string apiKey = apiDefinition.ApiUser.ApiKey;
                string secretKey = apiDefinition.ApiUser.SecretKey;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                        .GetBytes($"{apiKey}:{secretKey}"));

                client.DefaultRequestHeaders.Add("x-api-key", $"{encoded}");
            })
            .AddHttpMessageHandler<IdefixHttpClientHandler>()
            .AddTypedClient((client, serviceProvider) =>
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var restClient = new RestEase.RestClient(client)
                {
                    JsonSerializerSettings = jsonSerializerSettings
                };

                return restClient.For<IOsmIdefixClient>();
            });
        }

        private static void addAkilliETicaretClient(IServiceCollection services, AppSettings appSettings)
        {
            var apiDefinition = appSettings.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.AkilliETicaret);

            services.AddTransient<AkilliETicaretHttpClientHandler>();

            services.AddHttpClient<IAkilliETicaretClient>(client =>
            {
                if (apiDefinition is null)
                    return;

                client.BaseAddress = new Uri(apiDefinition.Domain);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("GUID", apiDefinition.ApiUser.SecretKey);

            })
           .AddHttpMessageHandler<AkilliETicaretHttpClientHandler>()
           .AddTypedClient((client, serviceProvider) =>
           {
               var jsonSerializerSettings = new JsonSerializerSettings
               {
                   ContractResolver = new CamelCasePropertyNamesContractResolver()
               };

               var restClient = new RestEase.RestClient(client)
               {
                   JsonSerializerSettings = jsonSerializerSettings
               };

               return restClient.For<IAkilliETicaretClient>();
           });
        }

    }
}