using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Controllers;
using Nop.Web.Framework.Themes;
using System.Text;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Controllers
{
    public partial class NMPHomeController : BasePublicController
    {
        private readonly IThemeContext _themeContext;
        public NMPHomeController(IThemeContext themeContext)
        {
            _themeContext = themeContext;
        }
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Index()
        {
            var workingThemeName = _themeContext.WorkingThemeName;
            return View(string.Format("/Themes/{0}/Views/Home/Index.cshtml", workingThemeName));
        }
    }
}