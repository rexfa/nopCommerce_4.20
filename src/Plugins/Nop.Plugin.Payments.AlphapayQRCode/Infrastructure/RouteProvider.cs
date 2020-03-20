using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.AlphaPayQRCode.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //PDT
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.PDTHandler", "Plugins/PaymentAlphaPayQRCode/PDTHandler",
                 new { controller = "PaymentAlphaPayQRCode", action = "PDTHandler" });

            //IPN
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.IPNHandler", "Plugins/PaymentAlphaPayQRCode/IPNHandler",
                 new { controller = "PaymentAlphaPayQRCode", action = "IPNHandler" });

            //Cancel
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.CancelOrder", "Plugins/PaymentAlphaPayQRCode/CancelOrder",
                 new { controller = "PaymentAlphaPayQRCode", action = "CancelOrder" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}