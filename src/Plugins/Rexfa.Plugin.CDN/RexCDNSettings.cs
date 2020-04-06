using Nop.Core.Configuration;
using Nop.Plugin.Shipping.Rush.Domain;

namespace Rexfa.Plugin.CDN
{
    /// <summary>
    /// Represents settings of the Rush shipping plugin
    /// </summary>
    public class RexCDNSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the account number
        /// </summary>
        public string PicFileDomainName { get; set; }
        public bool UsePicFileDomainName { get; set; }
        /// <summary>
        /// Gets or sets the access key
        /// </summary>
        public string JSFileDomainName { get; set; }
        public bool UseJSFileDomainName { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string CSSFileDomainName { get; set; }
        public bool UseCSSFileDomainName { get; set; }

    }
}