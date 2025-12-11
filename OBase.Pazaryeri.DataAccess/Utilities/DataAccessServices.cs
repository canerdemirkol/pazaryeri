using Microsoft.Extensions.DependencyInjection;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.Core.Concrete.Repository;
using OBase.Pazaryeri.DataAccess.Context;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Sale;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Promotion;
using OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Sale;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Sale;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.PriceStock;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Promotion;
using OBase.Pazaryeri.DataAccess.Services.Concrete.QuickPick;

namespace OBase.Pazaryeri.DataAccess.Utilities
{
    public static class DataAccessServices
	{
		public static IServiceCollection InjectDalServices(this IServiceCollection services)
		{
            // DbContext
            services.AddScoped<IDbContext, PyDbContext>();

            // Repositories
			services.AddScoped<IRepository, BaseRepository>();

            // Transaction
            services.AddScoped<ITransactionDalService, TransactionDalService>();

            // PazarYeri Services
            AddPazarYeriServices(services);

            // Generic Services
            AddGenericDalServices(services);

            // Temel Servisler
            AddCoreDalServices(services);

            return services;
		}


        private static void AddPazarYeriServices(IServiceCollection services)
        {
            services.AddScoped<IPazarYeriAktarimDalService, PazarYeriAktarimDalService>();
            services.AddScoped<IPazarYeriBirimTanimDalService, PazarYeriBirimTanimDalService>();
            services.AddScoped<IPazarYeriFaturaAdresDalService, PazarYeriFaturaAdresDalService>();
            services.AddScoped<IPazarYeriKargoAdresDalService, PazarYeriKargoAdresDalService>();
            services.AddScoped<IPazarYeriMalTanimDalService, PazarYeriMalTanimDalService>();
            services.AddScoped<IPazarYeriSiparisDalService, PazarYeriSiparisDalService>();
            services.AddScoped<IPazarYeriSiparisDetayDalService, PazarYeriSiparisDetayDalService>();
            services.AddScoped<IPazarYeriSiparisEkBilgiDalService, PazarYeriSiparisEkBilgiDalService>();
            services.AddScoped<IPazarYeriSiparisUrunDalService, PazarYeriSiparisUrunDalService>();
            services.AddScoped<IPromotionDalService, PromotionDalService>();
            services.AddScoped<ISaleDalService, SaleDalService>();
        }

        private static void AddGenericDalServices(IServiceCollection services)
        {
            services.AddScoped(typeof(ICreateDalService), typeof(CreateDalService));
            services.AddScoped(typeof(IGetDalService), typeof(GetDalService));
            services.AddScoped(typeof(IUpdateDalService), typeof(UpdateDalService));
            services.AddScoped(typeof(IDeleteDalService), typeof(DeleteDalService));
        }

        private static void AddCoreDalServices(IServiceCollection services)
        {
            services.AddScoped<IBirimTanimDalService, BirimTanimDalService>();
            services.AddScoped<IPriceStockDalService, PriceStockDalService>();
            services.AddScoped<IIadeDalService, IadeDalService>();
            services.AddScoped<IQuickPickDalService, QuickPickDalService>();
        }
    }
}