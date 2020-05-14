using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rexfa.Plugin.WeChat.NopMiniProgram.Models;
using Rexfa.Plugin.WeChat.NopMiniProgram.Services;
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

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Controllers
{
    public class NopMiniProgramController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly RexWechatMiniProgramSettings _rexWechatMiniProgramSettings;

        #endregion

        #region Ctor

        public NopMiniProgramController(ILocalizationService localizationService,
            INotificationService notificationService,
            ILogger logger,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPictureService pictureService,
            RexWechatMiniProgramSettings rexWechatMiniProgramSettings)
        {
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _rexWechatMiniProgramSettings = rexWechatMiniProgramSettings;
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
            var rexWechatMiniProgramSettings = _settingService.LoadSetting<RexWechatMiniProgramSettings>(storeScope);
            //prepare common model
            var model = new NopMiniProgramModel
            {
                WXAppID = _rexWechatMiniProgramSettings.WXAppID,
                WXAppName = _rexWechatMiniProgramSettings.WXAppName,
                WXAppSign = _rexWechatMiniProgramSettings.WXAppSign,
                WXAppKey = _rexWechatMiniProgramSettings.WXAppKey,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Rexfa.Plugin.WeChat.NopMiniProgram/Views/Configure.cshtml", model);
            model.WXAppID_OverrideForStore = _settingService.SettingExists(rexWechatMiniProgramSettings, x => x.WXAppID, storeScope);
            model.WXAppName_OverrideForStore = _settingService.SettingExists(rexWechatMiniProgramSettings, x => x.WXAppName, storeScope);
            model.WXAppSign_OverrideForStore = _settingService.SettingExists(rexWechatMiniProgramSettings, x => x.WXAppSign, storeScope);
            model.WXAppKey_OverrideForStore = _settingService.SettingExists(rexWechatMiniProgramSettings, x => x.WXAppKey, storeScope);
            return View("~/Plugins/Rexfa.Plugin.WeChat.NopMiniProgram/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(NopMiniProgramModel model)
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _rexWechatMiniProgramSettings.WXAppID = model.WXAppID;
            _rexWechatMiniProgramSettings.WXAppName = model.WXAppName;
            _rexWechatMiniProgramSettings.WXAppSign = model.WXAppSign;
            _rexWechatMiniProgramSettings.WXAppKey = model.WXAppKey;



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

            _settingService.SaveSetting(_rexWechatMiniProgramSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult TestHandler()
        {
            string currentPictureService = _pictureService.GetType().FullName;
            return Content(String.Format("currentPictureService={0}", currentPictureService), "application/json");
        }
        #endregion
    }
}
