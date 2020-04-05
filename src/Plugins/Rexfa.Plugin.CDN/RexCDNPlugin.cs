using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Tasks;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Rexfa.Plugin.CDN
{
    /// <summary>
    /// Represents the RexCDN plugin
    /// </summary>
    public class RexCDNPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields


        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;

        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;


        #endregion

        #region Ctor

        public RexCDNPlugin(
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,

            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper)
        {

            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;

            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _storeService = storeService;
            _webHelper = webHelper;

        }

        #endregion

        #region Methods




        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RexCDN/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new RexCDNSettings
            {
                UseCSSFileDomainNanme = false,
                UsePicFileDomainName = false,
                UseJSFileDomainName = false

            });



            //install synchronization task
            //if (_scheduleTaskService.GetTaskByType(RexCDNDefaults.SynchronizationTask) == null)
            //{
            //    _scheduleTaskService.InsertTask(new ScheduleTask
            //    {
            //        Enabled = true,
            //        Seconds = RexCDNDefaults.DefaultSynchronizationPeriod * 60 * 60,
            //        Name = RexCDNDefaults.SynchronizationTaskName,
            //        Type = RexCDNDefaults.SynchronizationTask,
            //    });
            //}

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.PicFileDomainName", "图片域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.PicFileDomainName.Hint", "图片域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.ActivateSMTP", "On your RexCDN account, the SMTP has not been enabled yet. To request its activation, simply send an email to our support team at contact@sendinblue.com and mention that you will be using the SMTP with the nopCommerce plugin.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.AddNewSMSNotification", "Add new SMS notification");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.BillingAddressPhone", "Billing address phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.CustomerPhone", "Customer phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.EditTemplate", "Edit template");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.AllowedTokens", "Allowed message variables");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.AllowedTokens.Hint", "This is a list of the message variables you can use in your SMS.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.ApiKey", "API v3 key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.ApiKey.Hint", "Paste your RexCDN account API v3 key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignList", "List");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignList.Hint", "Choose list of contacts to send SMS campaign.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignSenderName", "Send SMS campaign from");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignSenderName.Hint", "Input the name of the sender. The number of characters is limited to 11 (alphanumeric format).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignText", "Text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignText.Hint", "Specify SMS campaign content. The number of characters is limited to 160 for one message.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.List", "List");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.List.Hint", "Select the RexCDN list where your nopCommerce newsletter subscribers will be added.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.MaKey", "Tracker ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.MaKey.Hint", "Input your Tracker ID.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.Sender", "Send emails from");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.Sender.Hint", "Choose sender of your transactional emails.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmsSenderName", "Send SMS from");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmsSenderName.Hint", "Input the name of the sender. The number of characters is limited to 11 (alphanumeric format).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmtpKey", "SMTP key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmtpKey.Hint", "Specify SMTP key (password).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.StoreOwnerPhoneNumber", "Store owner phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.StoreOwnerPhoneNumber.Hint", "Input store owner phone number for SMS notifications.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.TrackingScript", "Tracking script");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.TrackingScript.Hint", $"Paste the tracking script generated by RexCDN here. {RexCDNDefaults.TrackingScriptId} and {RexCDNDefaults.TrackingScriptCustomerEmail} will be dynamically replaced.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseMarketingAutomation", "Use Marketing Automation");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseMarketingAutomation.Hint", "Check for enable RexCDN Automation.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmsNotifications", "Use SMS notifications");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmsNotifications.Hint", "Check for sending transactional SMS.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmtp", "Use RexCDN SMTP");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmtp.Hint", "Check for using RexCDN SMTP for sending transactional emails.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.General", "General");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.ImportProcess", "Your import is in process");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.ManualSync", "Manual synchronization");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SyncNow", "Sync now");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.MarketingAutomation", "Marketing Automation");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.MyPhone", "Store owner phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.PhoneType", "Type of phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.PhoneType.Hint", "Specify the type of phone number to send SMS.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMS", "SMS");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns", "SMS campaigns");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns.Sent", "Campaign successfully sent");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns.Submit", "Send campaign");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMSText", "Text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.SMSText.Hint", "Enter SMS text to send.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Synchronization", "Contacts");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Transactional", "Transactional emails");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.UseRexCDNTemplate", "RexCDN template");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //smtp accounts
            //foreach (var store in _storeService.GetAllStores())
            //{
            //    var key = $"{nameof(RexCDNSettings)}.{nameof(RexCDNSettings.EmailAccountId)}";
            //    var emailAccountId = _settingService.GetSettingByKey<int>(key, storeId: store.Id, loadSharedValueIfNotFound: true);
            //    var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            //    if (emailAccount != null)
            //        _emailAccountService.DeleteEmailAccount(emailAccount);
            //}



            //generic attributes
            //foreach (var store in _storeService.GetAllStores())
            //{
            //    var messageTemplates = _messageTemplateService.GetAllMessageTemplates(store.Id);
            //    foreach (var messageTemplate in messageTemplates)
            //    {
            //        _genericAttributeService.SaveAttribute<int?>(messageTemplate, RexCDNDefaults.TemplateIdAttribute, null);
            //    }
            //}

            //schedule task
            //var task = _scheduleTaskService.GetTaskByType(RexCDNDefaults.SynchronizationTask);
            //if (task != null)
            //    _scheduleTaskService.DeleteTask(task);

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.AccountInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.AccountInfo.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.ActivateSMTP");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.AddNewSMSNotification");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.BillingAddressPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.BillingAddressPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.CustomerPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.CustomerPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.EditTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.EditTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.AllowedTokens");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.AllowedTokens.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.ApiKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.ApiKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignList");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignList.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignSenderName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignSenderName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignText");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.CampaignText.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.List");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.List.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.MaKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.MaKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.Sender");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.Sender.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmsSenderName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmsSenderName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmtpKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.SmtpKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.StoreOwnerPhoneNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.StoreOwnerPhoneNumber.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.TrackingScript");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.TrackingScript.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseMarketingAutomation");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseMarketingAutomation.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmsNotifications");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmsNotifications.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmtp");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Fields.UseSmtp.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.General");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.ImportProcess");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.ManualSync");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SyncNow");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.MarketingAutomation");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.MyPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.MyPhone");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.PhoneType");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.PhoneType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.RexCDNTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.RexCDNTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.RexCDNTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMS");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns.Sent");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMS.Campaigns.Submit");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMSText");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.SMSText.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.StandardTemplate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Synchronization");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Transactional");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.UseRexCDNTemplate");

            base.Uninstall();
        }
        /// <summary>
        /// 建立主菜单选项
        /// </summary>
        /// <param name="rootNode"></param>
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "Rexfa.Plugin.CDN",
                Title = "Plugin Title",
                ControllerName = "ControllerName",
                ActionName = "List",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", null } },
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }

        #endregion


    }
}