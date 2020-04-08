using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;

using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;

using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Services.Tasks;

using Nop.Web.Framework;
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
                CSSFileDomainName="",
                JSFileDomainName="",
                PicFileDomainName="",
                UseCSSFileDomainName = false,
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
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.JSFileDomainName", "JavaScript域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.JSFileDomainName.Hint", "JavaScript域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.CSSFileDomainName", "CSS域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.CSSFileDomainName.Hint", "CSS域名地址");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.UsePicFileDomainName", "使用图片域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.UseJSFileDomainName", "使用图片域名地址");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.UseCSSFileDomainName", "使用图片域名地址");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexCDN.Instructions", @"
            <p>
	            <b>如果您使用此插件，请确保域名各个域名指向和浏览器接受域名绑定情况</b>
	            <br />
	            <br />要使用网络服务商的CDN服务，首先要有完善的域名操作经验<br />
	            <br />AWS的CDN服务为CloudFront，文档如下 (click <a href=""https://docs.aws.amazon.com/zh_cn/AmazonCloudFront/latest/DeveloperGuide/GettingStarted.SimpleDistribution.html"" target=""_blank"">这里</a>).
	            <br />注意本服务器和CDN设置必须契合，才能正确工作
                <br /><b>到2020年4月地皆为Alpha版本，请注意系统状态。</b>
	            <br />测试连接 (click <a href=""/Plugins/RexCDN/TestHandler"" target=""_blank"">这里</a>).
            </p>");


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
            //settings
            _settingService.DeleteSetting<RexCDNSettings>();
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.PicFileDomainName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.PicFileDomainName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.JSFileDomainName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.JSFileDomainName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.CSSFileDomainNanme");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.CSSFileDomainNanme.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.Instructions");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.UsePicFileDomainName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.UseJSFileDomainName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.RexCDN.UseCSSFileDomainName");


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
                Title = "Rex CDN Settings",
                ControllerName = "RexCDN",
                ActionName = "Configure",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", AreaNames.Admin } },
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