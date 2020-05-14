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

        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppSign")]
        public string WXAppSign { get; set; }
        public bool WXAppSign_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RexNopMiniProgram.WXAppKey")]
        public string WXAppKey { get; set; }
        public bool WXAppKey_OverrideForStore { get; set; }

        #endregion
    }
}
