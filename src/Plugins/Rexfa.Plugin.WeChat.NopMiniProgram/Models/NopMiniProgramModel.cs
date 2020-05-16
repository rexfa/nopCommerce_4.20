using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Models
{
    public class NopMiniProgramModel : BaseNopModel
    {
        #region Ctor


        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }



        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppID")]
        public string WXAppID { get; set; }
        public bool WXAppID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppName")]
        public string WXAppName { get; set; }
        public bool WXAppName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppVerifyCode")]
        public string WXAppVerifyCode { get; set; }
        public bool WXAppVerifyCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppSecret")]
        public string WXAppSecret { get; set; }
        public bool WXAppSecret_OverrideForStore { get; set; }

        #endregion
    }
}
