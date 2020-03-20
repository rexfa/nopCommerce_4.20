using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.AlphaPayQRCode.Components
{
    [ViewComponent(Name = "PaymentAlphaPayQRCode")]
    public class PaymentAlphaPayQRCodeViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.AlphaPayQRCode/Views/PaymentInfo.cshtml");
        }
    }
}
