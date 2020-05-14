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
using Nop.Services.Authentication.External;

namespace Rexfa.Plugin.WeChat.NopMiniProgram
{
    public class RexWechatMiniProgramPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin,IExternalAuthenticationMethod
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

        public RexWechatMiniProgramPlugin(
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
            return $"{_webHelper.GetStoreLocation()}Admin/NopMiniProgram/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new RexWechatMiniProgramSettings
            {
                WXAppID="",
                WXAppKey="",
                WXAppName="",
                WXAppSign=""
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
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexNopMiniProgram.WXAppID",  "微信小程序AppID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexNopMiniProgram.WXAppKey", "内部通讯Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexNopMiniProgram.WXAppName", "微信小程序名称");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexNopMiniProgram.WXAppSign", "通讯数据校验签名");



            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.RexNopMiniProgram.Instructions", @"
            <p>
	            <b>如果您使用此插件，请确保域名和腾讯微信开发者签署情况</b>
	            <br />
	            <br />商城必须通过域名备案和拥有SSL证书<br />
	            <br />腾讯微信小程序开发者文档如下： (click <a href=""https://developers.weixin.qq.com/miniprogram/dev/framework/"" target=""_blank"">这里</a>).
	            <br />注意本服务器和CDN设置必须契合，才能正确工作
                <br /><b>本插件2020年开始开发。</b>
	            <br />测试连接 (click <a href=""/Plugins/NopMiniProgram/TestHandler"" target=""_blank"">这里</a>).
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
            _settingService.DeleteSetting<RexWechatMiniProgramSettings>();
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

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = " Rexfa.Plugin.WeChat.NopMiniProgram",
                Title = "Rex MiniProgram Settings",
                ControllerName = "NopMiniProgram",
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

        public string GetPublicViewComponentName()
        {
            return RexWechatMiniProgramDefaults.VIEW_COMPONENT_NAME;
        }

        #endregion
    }
}
