using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEListingDetailsDto
    {
        public List<Listing> listings { get; set; }
        public int totalCount { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
    }
    public class Listing
    {
        public string uniqueIdentifier { get; set; }
        public string hepsiburadaSku { get; set; }
        public string merchantSku { get; set; }
        public double price { get; set; }
        public int availableStock { get; set; }
        public int dispatchTime { get; set; }
        public string cargoCompany1 { get; set; }
        public string cargoCompany2 { get; set; }
        public string cargoCompany3 { get; set; }
        public string shippingAddressLabel { get; set; }
        public string claimAddressLabel { get; set; }
        public int maximumPurchasableQuantity { get; set; }
        public int minimumPurchasableQuantity { get; set; }
        public List<object> pricings { get; set; }
        public bool isSalable { get; set; }
        public List<object> customizableProperties { get; set; }
        public List<string> deactivationReasons { get; set; }
        public bool isSuspended { get; set; }
        public bool isLocked { get; set; }
        public List<object> lockReasons { get; set; }
        public bool isFrozen { get; set; }
        public double commissionRate { get; set; }
        public object buyboxOrder { get; set; }
        public List<object> availableWarehouses { get; set; }
        public bool isFulfilledByHB { get; set; }
    }
}
