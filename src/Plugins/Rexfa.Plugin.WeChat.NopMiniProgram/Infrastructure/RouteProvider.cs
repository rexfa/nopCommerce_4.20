using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 16;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Test
            routeBuilder.MapRoute(RexWechatMiniProgramDefaults.ImportTestRoute, "Admin/NopMiniProgram/TestHandler",
                 new { controller = "NopMiniProgram", action = "TestHandler" });

            routeBuilder.MapRoute(RexWechatMiniProgramDefaults.ImportHomeRoute, "Plugins/RexWechatMiniProgram/Home",
                new { controller = "NMPHome", action = "Index" });

            routeBuilder.MapRoute(RexWechatMiniProgramDefaults.ImportCustomerRoute, "Plugins/RexWechatMiniProgram/Customer/Info",
                new { controller = "NMPCustomer", action = "Info" });

            routeBuilder.MapRoute(RexWechatMiniProgramDefaults.ImportCustomerRegAndLogin, "Plugins/RexWechatMiniProgram/Customer/NMPRegisterAndLogin",
                new { controller = "NMPCustomer", action = "NMPRegisterAndLogin" });

        }
    }
}
