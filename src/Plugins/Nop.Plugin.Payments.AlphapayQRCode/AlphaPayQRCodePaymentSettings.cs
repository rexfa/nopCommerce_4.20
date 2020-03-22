using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.AlphaPayQRCode
{
    public class AlphaPayQRCodePaymentSettings : ISettings
    {

        /// <summary>
        /// 商户编码，由4位或6位大写字母或数字构成
        /// </summary>
        public string PartnerCode { get; set; }
        /// <summary>
        /// APPID
        /// </summary>
        public string APPID { get; set; }

        /// <summary>
        /// 系统为商户分配的开发校验码，请妥善保管，不要在公开场合泄露
        /// </summary>
        public string CredentialCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to pass info about purchased items to AlphaPay
        /// </summary>
        public bool PassProductNamesAndTotals { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
