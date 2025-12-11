using System.Collections.Generic;

namespace OBase.Pazaryeri.Domain.ConfigurationOptions
{
    /// <summary>
    /// Swagger UI'da hangi endpoint gruplarının görüneceğini belirleyen ayarlar.
    /// appsettings.json dosyasında "SwaggerSettings" bölümünden okunur.
    /// </summary>
    public class SwaggerSettings
    {
        /// <summary>
        /// Swagger'da gösterilecek endpoint gruplarının listesi.
        /// Örnek: ["YemekSepeti", "TrendyolGo", "GetirCarsi"]
        /// </summary>
        /// <remarks>
        /// Bu liste sadece ShowUngroupedEndpoints = true olduğunda kullanılır.
        /// Eğer liste boş ise, tüm endpoint'ler gösterilir.
        /// Eğer liste dolu ise, sadece bu listedeki gruplara ait endpoint'ler gösterilir.
        /// </remarks>
        public List<string> EnabledEndpointGroups { get; set; } = new List<string>();

        /// <summary>
        /// False ise, TÜM endpoint'ler gösterilir (EnabledEndpointGroups listesi göz ardı edilir).
        /// True ise, sadece EnabledEndpointGroups listesindeki gruplara ait endpoint'ler gösterilir.
        /// Default: true
        /// </summary>
        public bool ShowUngroupedEndpoints { get; set; } = false;

        /// <summary>
        /// True ise, filtreleme aktif olur. False ise tüm endpoint'ler gösterilir.
        /// Default: true
        /// </summary>
        public bool EnableFiltering { get; set; } = false;
    }
}
