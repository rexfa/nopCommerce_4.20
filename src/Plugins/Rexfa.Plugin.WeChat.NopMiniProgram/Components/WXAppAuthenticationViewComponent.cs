using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Components
{
    [ViewComponent(Name = RexWechatMiniProgramDefaults.VIEW_COMPONENT_NAME)]
    public class WXAppAuthenticationViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Rexfa.Plugin.WeChat.NopMiniProgram/Views/PublicInfo.cshtml");
        }
    }
}
