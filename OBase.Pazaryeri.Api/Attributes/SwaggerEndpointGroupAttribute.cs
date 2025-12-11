using System;

namespace OBase.Pazaryeri.Api.Attributes
{
    /// <summary>
    /// Swagger dokümantasyonunda endpoint'lerin koşullu olarak gösterilmesi için kullanılan attribute.
    /// Bu attribute ile işaretlenen endpoint'ler, appsettings.json'daki SwaggerSettings konfigürasyonuna göre
    /// Swagger UI'da gösterilir veya gizlenir.
    /// </summary>
    /// <example>
    /// [SwaggerEndpointGroup("YemekSepeti")]
    /// public async Task<IActionResult> CreateOrder() { }
    /// </example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class SwaggerEndpointGroupAttribute : Attribute
    {
        /// <summary>
        /// Endpoint'in ait olduğu grup adı (örn: "YemekSepeti", "TrendyolGo", "GetirCarsi")
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupName">Endpoint grubunun adı</param>
        public SwaggerEndpointGroupAttribute(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Group name cannot be null or empty", nameof(groupName));

            GroupName = groupName;
        }
    }
}
