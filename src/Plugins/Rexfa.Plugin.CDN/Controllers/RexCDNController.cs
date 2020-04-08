using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rexfa.Plugin.CDN.Models;
using Rexfa.Plugin.CDN.Services;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Rexfa.Plugin.CDN.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class RexCDNController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly RexCDNSettings _rexCDNSettings;

        #endregion

        #region Ctor

        public RexCDNController(ILocalizationService localizationService,
            INotificationService notificationService,
            ILogger logger,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPictureService pictureService,
            RexCDNSettings rexCDNSettings)
        {
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _rexCDNSettings = rexCDNSettings;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var rexCDNSettings = _settingService.LoadSetting<RexCDNSettings>(storeScope);
            //prepare common model
            var model = new RexCDNModel
            {
                CSSFileDomainName = _rexCDNSettings.CSSFileDomainName,
                JSFileDomainName = _rexCDNSettings.JSFileDomainName,
                PicFileDomainName = _rexCDNSettings.PicFileDomainName,
                UseCSSFileDomainName = _rexCDNSettings.UseCSSFileDomainName,
                UseJSFileDomainName = _rexCDNSettings.UseJSFileDomainName,
                UsePicFileDomainName = _rexCDNSettings.UsePicFileDomainName,
                ActiveStoreScopeConfiguration =  storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Rexfa.Plugin.CDN/Views/Configure.cshtml", model);
            model.CSSFileDomainName_OverrideForStore = _settingService.SettingExists(rexCDNSettings, x => x.CSSFileDomainName, storeScope);
            model.JSFileDomainName_OverrideForStore = _settingService.SettingExists(rexCDNSettings, x => x.JSFileDomainName, storeScope);
            model.PicFileDomainName_OverrideForStore = _settingService.SettingExists(rexCDNSettings, x => x.PicFileDomainName, storeScope);

            return View("~/Plugins/Rexfa.Plugin.CDN/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(RexCDNModel model)
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _rexCDNSettings.CSSFileDomainName = model.CSSFileDomainName;
            _rexCDNSettings.JSFileDomainName = model.JSFileDomainName;
            _rexCDNSettings.PicFileDomainName = model.PicFileDomainName;
            _rexCDNSettings.UseCSSFileDomainName = model.UseCSSFileDomainName;
            _rexCDNSettings.UseJSFileDomainName = model.UseJSFileDomainName;
            _rexCDNSettings.UsePicFileDomainName = model.UsePicFileDomainName;


            //use default services if no one is selected 
            //if (!model.CarrierServices.Any())
            //{
            //    model.CarrierServices = new List<string>
            //    {
            //        _rushService.GetUpsCode(DeliveryService.Ground),
            //        _rushService.GetUpsCode(DeliveryService.WorldwideExpedited),
            //        _rushService.GetUpsCode(DeliveryService.Standard),
            //        _rushService.GetUpsCode(DeliveryService._3DaySelect)
            //    };
            //}
            //_rexCDNSettings.CarrierServicesOffered = string.Join(':', model.CarrierServices.Select(service => $"[{service}]"));

            _settingService.SaveSetting(_rexCDNSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult TestHandler()
        {
            string currentPictureService =  _pictureService.GetType().FullName;
            return Content(String.Format("currentPictureService={0}", currentPictureService), "application/json");
        }
        #endregion
    }
}