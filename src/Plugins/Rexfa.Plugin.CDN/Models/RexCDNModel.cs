using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Rexfa.Plugin.CDN.Models

{
    public class RexCDNModel : BaseNopModel
    {
        #region Ctor


        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }



        [NopResourceDisplayName("Plugins.Misc.RexCDN.PicFileDomainName")]
        public string PicFileDomainName { get; set; }
        public bool PicFileDomainName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexCDN.JSFileDomainName")]
        public string JSFileDomainName { get; set; }
        public bool JSFileDomainName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexCDN.CSSFileDomainName")]
        public string CSSFileDomainName { get; set; }
        public bool CSSFileDomainName_OverrideForStore { get; set; }



        [NopResourceDisplayName("Plugins.Misc.RexCDN.UsePicFileDomainName")]
        public bool UsePicFileDomainName { get; set; }
        public bool UsePicFileDomainName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexCDN.UseJSFileDomainName")]
        public bool UseJSFileDomainName { get; set; }
        public bool UseJSFileDomainName_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Misc.RexCDN.UseCSSFileDomainName")]
        public bool UseCSSFileDomainName { get; set; }
        public bool UseCSSFileDomainName_OverrideForStore { get; set; }


        #endregion
    }
}