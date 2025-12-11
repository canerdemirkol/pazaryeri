using AutoMapper;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Domain.Configurations.MappingConfigurations.TyGoMapper
{
	public class ClaimMapper : Profile
	{
		public ClaimMapper()
		{
			CreateMap<ClaimContent, PazarYeriSiparisIade>()
				.ForMember(dest => dest.ClaimId, src => src.MapFrom(x => x.Id))
				.ForMember(dest => dest.MusteriAd, src => src.MapFrom(x => x.CustomerFirstName))
				.ForMember(dest => dest.MusteriSoyad, src => src.MapFrom(x => x.CustomerLastName))
				.ForMember(dest => dest.ReturnedSellerEH, src => src.MapFrom(x => x.ReturnedSeller ? Character.E : Character.H))
				.ForMember(dest => dest.SiparisPaketId, src => src.MapFrom(x => x.OrderShipmentPackageId))
				.ForMember(dest => dest.DepoAktarildiEH, src => src.MapFrom(x => Character.H))
				.ForMember(dest => dest.SiparisTarih, src => src.MapFrom(x => UnixTimeStampToDateTime(x.OrderDate,PazarYeri.TrendyolGo)))
				.ForMember(dest => dest.BirimAciklama, src => src.MapFrom(x => ""))
				.ForMember(dest => dest.ClaimStatus, src => src.MapFrom(x => x.ClaimItems.FirstOrDefault().ClaimItemStatus.Name))
				.ForMember(dest => dest.ClaimTarih, src => src.MapFrom(x => UnixTimeStampToDateTime(x.ClaimDate, PazarYeri.TrendyolGo)))
				.ForMember(dest => dest.ReturnedSellerEH, src => src.MapFrom(x => x.ReturnedSeller ? Character.E : Character.H))
				.ForMember(dest => dest.SiparisNo, src => src.MapFrom(x => x.OrderNumber))
				.ForMember(dest => dest.DepoAktarimDenemeSayisi, src => src.MapFrom(x => 0))
				.ForMember(dest => dest.Id, src => src.Ignore())
				.ForMember(dest => dest.PazarYeriNo, src => src.Ignore())
				.ReverseMap();
			CreateMap<ClaimItem, PazarYeriSiparisIadeDetay>()
				.ForMember(dest => dest.MusteriNot, src => src.MapFrom(x => x.CustomerNote))
				.ForMember(dest => dest.Aciklama, src => src.MapFrom(x => x.Note))
				.ForMember(dest => dest.ClaimDetayId, src => src.MapFrom(x => x.Id))
				.ForMember(dest => dest.CozumlendiEH, src => src.MapFrom(x => x.Resolved ? Character.E : Character.H))
				.ForMember(dest => dest.OrderLineItemId, src => src.MapFrom(x => x.OrderLineItemId))
				.ForMember(dest => dest.ClaimImageUrls, src => src.Ignore())
				.ForMember(dest => dest.Miktar, src => src.Ignore())
				.ForMember(dest => dest.Sayisi, src => src.Ignore())
				.ReverseMap();

			CreateMap<ClaimItemStatus, PazarYeriSiparisIadeDetay>()
				.ForMember(dest => dest.ClaimItemStatus, src => src.MapFrom(x => x.Name))
				.ReverseMap();

			CreateMap<CustomerClaimItemReason, PazarYeriSiparisIadeDetay>()
				.ForMember(dest => dest.MusteriIadeSebepKod, src => src.MapFrom(x => x.Code))
				.ForMember(dest => dest.MusteriIadeSebepId, src => src.MapFrom(x => x.Id))
				.ForMember(dest => dest.MusteriIadeSebepAd, src => src.MapFrom(x => x.Name))
				.ReverseMap();

			CreateMap<TrendyolClaimItemReason, PazarYeriSiparisIadeDetay>()
				.ForMember(dest => dest.PyIadeSebepKod, src => src.MapFrom(x => x.Code))
				.ForMember(dest => dest.PyIadeSebepId, src => src.MapFrom(x => x.Id))
				.ForMember(dest => dest.PyIadeSebepAd, src => src.MapFrom(x => x.Name))
				.ReverseMap();
		}
	}
}