using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.SnapPay.Infrastructure
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
            routeBuilder.MapRoute("Plugin.Payments.SnapPay.ReturnHandler", "Plugins/PaymentSnapPay/ReturnHandler",
                 new { controller = "PaymentSnapPay", action = "ReturnHandler" });

            //Notify
            routeBuilder.MapRoute("Plugin.Payments.SnapPay.NotifyHandler", "Plugins/PaymentSnapPay/NotifyHandler",
                 new { controller = "PaymentSnapPay", action = "NotifyHandler" });

            //Cancel
            routeBuilder.MapRoute("Plugin.Payments.SnapPay.CancelOrder", "Plugins/PaymentSnapPay/CancelOrder",
                 new { controller = "PaymentSnapPay", action = "CancelOrder" });
            //Error
            routeBuilder.MapRoute("Plugin.Payments.SnapPay.ErrorHandler", "Plugins/PaymentSnapPay/ErrorHandler",
                 new { controller = "PaymentSnapPay", action = "ErrorHandler" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}