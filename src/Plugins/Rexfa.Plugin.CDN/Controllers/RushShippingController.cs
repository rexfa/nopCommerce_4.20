using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Shipping.Rush.Domain;
using Nop.Plugin.Shipping.Rush.Models;
using Nop.Plugin.Shipping.Rush.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Rush.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class RushShippingController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly RushService _rushService;
        private readonly RushSettings _rushSettings;

        #endregion

        #region Ctor

        public RushShippingController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            RushService rushService,
            RushSettings rushSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _rushService = rushService;
            _rushSettings = rushSettings;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //prepare common model
            var model = new RushShippingModel
            {
                AccountNumber = _rushSettings.AccountNumber,
                AccessKey = _rushSettings.AccessKey,
                Username = _rushSettings.Username,
                Password = _rushSettings.Password,
                UseSandbox = _rushSettings.UseSandbox,
                AdditionalHandlingCharge = _rushSettings.AdditionalHandlingCharge,
                InsurePackage = _rushSettings.InsurePackage,
                CustomerClassification = (int)_rushSettings.CustomerClassification,
                PickupType = (int)_rushSettings.PickupType,
                PackagingType = (int)_rushSettings.PackagingType,
                SaturdayDeliveryEnabled = _rushSettings.SaturdayDeliveryEnabled,
                PassDimensions = _rushSettings.PassDimensions,
                PackingPackageVolume = _rushSettings.PackingPackageVolume,
                PackingType = (int)_rushSettings.PackingType,
                Tracing = _rushSettings.Tracing
            };

            //prepare offered delivery services
            var servicesCodes = _rushSettings.CarrierServicesOffered.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();

            //prepare available options
            model.AvailableCustomerClassifications = CustomerClassification.DailyRates.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text, item.Value)).ToList();
            model.AvailablePickupTypes = PickupType.DailyPickup.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text, item.Value)).ToList();
            model.AvailablePackagingTypes = PackagingType.CustomerSuppliedPackage.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text?.TrimStart('_'), item.Value)).ToList();
            model.AvaliablePackingTypes = PackingType.PackByDimensions.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text, item.Value)).ToList();
            model.AvailableCarrierServices = DeliveryService.Standard.ToSelectList(false).Select(item =>
            {
                var serviceCode = _rushService.GetUpsCode((DeliveryService)int.Parse(item.Value));
                return new SelectListItem($"Rush {item.Text?.TrimStart('_')}", serviceCode, servicesCodes.Contains(serviceCode));
            }).ToList();

            return View("~/Plugins/Shipping.Rush/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(RushShippingModel model)
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _rushSettings.AccountNumber = model.AccountNumber;
            _rushSettings.AccessKey = model.AccessKey;
            _rushSettings.Username = model.Username;
            _rushSettings.Password = model.Password;
            _rushSettings.UseSandbox = model.UseSandbox;
            _rushSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _rushSettings.CustomerClassification = (CustomerClassification)model.CustomerClassification;
            _rushSettings.PickupType = (PickupType)model.PickupType;
            _rushSettings.PackagingType = (PackagingType)model.PackagingType;
            _rushSettings.InsurePackage = model.InsurePackage;
            _rushSettings.SaturdayDeliveryEnabled = model.SaturdayDeliveryEnabled;
            _rushSettings.PassDimensions = model.PassDimensions;
            _rushSettings.PackingPackageVolume = model.PackingPackageVolume;
            _rushSettings.PackingType = (PackingType)model.PackingType;
            _rushSettings.Tracing = model.Tracing;

            //use default services if no one is selected 
            if (!model.CarrierServices.Any())
            {
                model.CarrierServices = new List<string>
                {
                    _rushService.GetUpsCode(DeliveryService.Ground),
                    _rushService.GetUpsCode(DeliveryService.WorldwideExpedited),
                    _rushService.GetUpsCode(DeliveryService.Standard),
                    _rushService.GetUpsCode(DeliveryService._3DaySelect)
                };
            }
            _rushSettings.CarrierServicesOffered = string.Join(':', model.CarrierServices.Select(service => $"[{service}]"));

            _settingService.SaveSetting(_rushSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}