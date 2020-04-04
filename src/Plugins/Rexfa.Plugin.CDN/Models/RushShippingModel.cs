using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.Rush.Models
{
    public class RushShippingModel : BaseNopModel
    {
        #region Ctor

        public RushShippingModel()
        {
            CarrierServices = new List<string>();
            AvailableCarrierServices = new List<SelectListItem>();
            AvailableCustomerClassifications = new List<SelectListItem>();
            AvailablePickupTypes = new List<SelectListItem>();
            AvailablePackagingTypes = new List<SelectListItem>();
            AvaliablePackingTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.AccessKey")]
        public string AccessKey { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.Username")]
        public string Username { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.InsurePackage")]
        public bool InsurePackage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.CustomerClassification")]
        public int CustomerClassification { get; set; }
        public IList<SelectListItem> AvailableCustomerClassifications { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.PickupType")]
        public int PickupType { get; set; }
        public IList<SelectListItem> AvailablePickupTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.PackagingType")]
        public int PackagingType { get; set; }
        public IList<SelectListItem> AvailablePackagingTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.AvailableCarrierServices")]
        public IList<SelectListItem> AvailableCarrierServices { get; set; }
        public IList<string> CarrierServices { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.SaturdayDeliveryEnabled")]
        public bool SaturdayDeliveryEnabled { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.PassDimensions")]
        public bool PassDimensions { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.PackingPackageVolume")]
        public int PackingPackageVolume { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.PackingType")]
        public int PackingType { get; set; }
        public IList<SelectListItem> AvaliablePackingTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Rush.Fields.Tracing")]
        public bool Tracing { get; set; }

        #endregion
    }
}