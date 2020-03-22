using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.AlphaPayQRCode.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }


        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.PartnerCode")]
        public string PartnerCode { get; set; }
        public bool PartnerCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.CredentialCode")]
        public string CredentialCode { get; set; }
        public bool CredentialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.APPID")]
        public string APPID { get; set; }
        public bool APPID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }
        public bool PassProductNamesAndTotals_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}