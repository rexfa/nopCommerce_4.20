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
            //Return
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.ReturnHandler", "Plugins/PaymentAlphaPayQRCode/ReturnHandler",
                 new { controller = "PaymentAlphaPayQRCode", action = "ReturnHandler" });
            //Notify
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.NotifyHandler", "Plugins/PaymentAlphaPayQRCode/NotifyHandler",
                 new { controller = "PaymentAlphaPayQRCode", action = "NotifyHandler" });

            //ErrorHandler
            routeBuilder.MapRoute("Plugin.Payments.AlphaPayQRCode.ErrorHandler", "Plugins/PaymentAlphaPayQRCode/ErrorHandler",
                 new { controller = "PaymentAlphaPayQRCode", action = "ErrorHandler" });

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