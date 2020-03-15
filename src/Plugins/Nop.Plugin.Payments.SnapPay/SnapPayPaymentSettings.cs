using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SnapPay
{
    /// <summary>
    /// Represents settings of the SnapPay Standard payment plugin
    /// </summary>
    public class SnapPayPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        //public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets a Merchant ID. 商户号
        /// </summary>
        public string MerchantID { get; set; }
        /// <summary>
        /// APPID
        /// </summary>
        public string APPID { get; set; }

        /// <summary>
        /// Gets or sets Signing key.签名密钥
        /// </summary>
        public string SigningKey { get; set; }

        /// <summary>
        /// 是否传递商品详细信息给 SnapPay
        /// </summary>
        public bool PassProductNamesAndTotals { get; set; }

        /// <summary>
        /// Gets or sets an additional fee 设置额外费
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// 设置额外费百分比
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
