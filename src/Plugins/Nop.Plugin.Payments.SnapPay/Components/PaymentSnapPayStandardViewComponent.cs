using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.SnapPay.Components
{
    [ViewComponent(Name = "PaymentSnapPay")]
    public class PaymentSnapPayViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.SnapPay/Views/PaymentInfo.cshtml");
        }
    }
}
