using System;
using System.Linq;
using Nop.Core;
using Nop.Plugin.Shipping.Rush.Domain;
using Nop.Plugin.Shipping.Rush.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.Rush
{
    /// <summary>
    /// Represents Rush computation method
    /// </summary>
    public class RushComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly RushService _rushService;

        #endregion

        #region Ctor

        public RushComputationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            RushService rushService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _rushService = rushService;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            if (!getShippingOptionRequest.Items?.Any() ?? true)
                return new GetShippingOptionResponse { Errors = new[] { "No shipment items" } };

            if (getShippingOptionRequest.ShippingAddress?.Country == null)
                return new GetShippingOptionResponse { Errors = new[] { "Shipping address is not set" } };

            return _rushService.GetRates(getShippingOptionRequest);
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RushShipping/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new RushSettings
            {
                UseSandbox = true,
                CustomerClassification = CustomerClassification.StandardListRates,
                PickupType = PickupType.OneTimePickup,
                PackagingType = PackagingType.ExpressBox,
                PackingPackageVolume = 5184,
                PackingType = PackingType.PackByDimensions,
                PassDimensions = true
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByDimensions", "Pack by dimensions");
            _localizationService.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByOneItemPerPackage", "Pack by one item per package");
            _localizationService.AddOrUpdatePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByVolume", "Pack by volume");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccessKey", "Access Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccessKey.Hint", "Specify Rush access key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccountNumber", "Account number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccountNumber.Hint", "Specify Rush account number (required to get negotiated rates).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AdditionalHandlingCharge", "Additional handling charge");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AdditionalHandlingCharge.Hint", "Enter additional handling fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AvailableCarrierServices", "Carrier Services");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.AvailableCarrierServices.Hint", "Select the services you want to offer to customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.CustomerClassification", "Rush Customer Classification");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.CustomerClassification.Hint", "Choose customer classification.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.InsurePackage", "Insure package");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.InsurePackage.Hint", "Check to insure packages.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackagingType", "Rush Packaging Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackagingType.Hint", "Choose Rush packaging type.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingPackageVolume", "Package volume");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingPackageVolume.Hint", "Enter your package volume.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingType", "Packing type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingType.Hint", "Choose preferred packing type.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PassDimensions", "Pass dimensions");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PassDimensions.Hint", "Check if you want to pass package dimensions when requesting rates.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Password", "Password");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Password.Hint", "Specify Rush password.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PickupType", "Rush Pickup Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.PickupType.Hint", "Choose Rush pickup type.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.SaturdayDeliveryEnabled", "Saturday Delivery enabled");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.SaturdayDeliveryEnabled.Hint", "Check to get rates for Saturday Delivery options.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Tracing", "Tracing");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Tracing.Hint", "Check if you want to record plugin tracing in System Log. Warning: The entire request and response XML will be logged (including AccessKey/Username,Password). Do not leave this enabled in a production environment.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Username", "Username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.Username.Hint", "Specify Rush username.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.UseSandbox", "Use sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Fields.UseSandbox.Hint", "Check to use sandbox (testing environment).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Arrived", "Arrived");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Booked", "Booked");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Delivered", "Delivered");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Departed", "Departed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.ExportScanned", "Export scanned");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.NotDelivered", "Not delivered");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.OriginScanned", "Origin scanned");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Pickup", "Pickup");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<RushSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByDimensions");
            _localizationService.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByOneItemPerPackage");
            _localizationService.DeletePluginLocaleResource("Enums.Nop.Plugin.Shipping.Rush.PackingType.PackByVolume");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccessKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccessKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccountNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AccountNumber.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AdditionalHandlingCharge");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AdditionalHandlingCharge.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AvailableCarrierServices");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.AvailableCarrierServices.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.CustomerClassification");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.CustomerClassification.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.InsurePackage");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.InsurePackage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackagingType");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackagingType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingPackageVolume");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingPackageVolume.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingType");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PackingType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PassDimensions");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PassDimensions.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Password");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Password.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PickupType");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.PickupType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.SaturdayDeliveryEnabled");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.SaturdayDeliveryEnabled.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Tracing");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Tracing.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Username");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.Username.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Fields.UseSandbox.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Arrived");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Booked");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Delivered");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Departed");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.ExportScanned");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.NotDelivered");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.OriginScanned");
            _localizationService.DeletePluginLocaleResource("Plugins.Shipping.Rush.Tracker.Pickup");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Realtime;

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker => new RushShipmentTracker(_rushService);

        #endregion
    }
}