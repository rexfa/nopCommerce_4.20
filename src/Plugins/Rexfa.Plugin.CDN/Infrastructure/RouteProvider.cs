using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Rexfa.Plugin.CDN.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Test
            routeBuilder.MapRoute("Rexfa.Plugin.CDN.TestHandler", "Plugins/RexCDN/TestHandler",
                 new { controller = "RexCDN", action = "TestHandler" });


        }
    }
}
