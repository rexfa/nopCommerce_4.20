using System;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Payments.AlphaPayQRCode.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.AlphaPayQRCode
{
    public class AlphaPayQRCodePaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly AlphaPayQRCodeHttpClient _alphaPayQRCodeyHttpClient;
        private readonly AlphaPayQRCodePaymentSettings _alphaPayQRCodePaymentSettings;

        #endregion

        #region Ctor

        public AlphaPayQRCodePaymentProcessor(CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            AlphaPayQRCodeHttpClient alphaPayQRCodeyHttpClient,
            AlphaPayQRCodePaymentSettings alphaPayQRCodePaymentSettings)
        {
            _currencySettings = currencySettings;
            _checkoutAttributeParser = checkoutAttributeParser;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _taxService = taxService;
            _webHelper = webHelper;
            _alphaPayQRCodeyHttpClient = alphaPayQRCodeyHttpClient;
            _alphaPayQRCodePaymentSettings = alphaPayQRCodePaymentSettings;
        }

        #endregion

        #region Properties
        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        ///支付类型，这个在 alphapay 需要试一下
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// 获取一个值，该值指示我们是否应显示此插件的付款信息页面
        /// </summary>
        public bool SkipPaymentInfo => false;
        /// <summary>
        /// alphapay的说明
        /// </summary>

        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.AlphaPayQRCode.PaymentMethodDescription");
        #endregion

        #region Methods
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            throw new NotImplementedException();
        }

        public string GetPublicViewComponentName()
        {
            throw new NotImplementedException();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL) 支付主要方法，调用第三方URL完成支付
        /// </summary>
        /// <param name="postProcessPaymentRequest"></param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var baseUrl = "https://pay.alphapay.ca/api/v1.0/gateway";

            //加入 Path Variable
            baseUrl = baseUrl + "/partners/" + _alphaPayQRCodePaymentSettings.PartnerCode + "/orders/" + postProcessPaymentRequest.Order.OrderGuid.ToString();
            //create common query parameters for the request
            var queryJson = CreateQueryJSON(postProcessPaymentRequest);

            var queryParameters = CreateQueryParameters(postProcessPaymentRequest);
            //whether to include order items in a transaction
            //if (_alphaPayQRCodePaymentSettings.PassProductNamesAndTotals)
            //{
            //    //add order items query parameters to the request
            //    var parameters = new Dictionary<string, string>(queryParameters);
            //    AddItemsParameters(parameters, postProcessPaymentRequest);

            //    //remove null values from parameters
            //    parameters = parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            //        .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            //    //ensure redirect URL doesn't exceed 2K chars to avoid "too long URL" exception
            //    var redirectUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            //    if (redirectUrl.Length <= 2048)
            //    {
            //        _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
            //        return;
            //    }
            //}

            //or add only an order total query parameters to the request
            //AddOrderTotalParameters(queryParameters, postProcessPaymentRequest);

            //remove null values from parameters
            queryParameters = queryParameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            var url = QueryHelpers.AddQueryString(baseUrl, queryParameters);

            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            throw new NotImplementedException();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new AlphaPayQRCodePaymentSettings
            {
                UseSandbox = true
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFee", "附加费用");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFee.Hint", "输入附加费用以向您的客户收费");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFeePercentage", "附加费用。百分比");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.AdditionalFeePercentage.Hint", "确定是否对订单总额应用一定百分比的附加费。 如果未启用，则使用固定值");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.PartnerCode", "Partner Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.PartnerCode.Hint", "Partner Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.PassProductNamesAndTotals", "将产品名称和订单总数传递给AlphaPay");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.PassProductNamesAndTotals.Hint", "将产品名称和订单总数传递给AlphaPay");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.CredentialCode", "Credential Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.CredentialCode.Hint", "Credential Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Fields.RedirectionTip", "您将被重定向到AlphaPay网站以完成订单");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.Instructions", @"
            <p>
	            <b>如果您使用此网关，请确保AlphaPay支持您的主要商店货币</b>
	            <br />
	            <br />要使用Partner Code 、Credential Code:<br />
	            <br />商户后台访问地址  (click <a href=""https://pay.alphapay.ca/"" target=""_blank"">这里</a>).
	            <br />设置好本服务器设置
	            <br />
            </p>");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.PaymentMethodDescription", "您将被重定向到AlphaPay网站以完成付款");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AlphaPayQRCode.RoundingWarning", "似乎您已禁用\" ShoppingCartSettings.RoundPricesDuringCalculation \"设置。 请记住，这可能会导致订单总额不一致，因为AlphaPay仅舍入到两位小数");

            base.Install();
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created query parameters</returns>
        private IDictionary<string, string> CreateQueryParameters(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get store location
            var storeLocation = _webHelper.GetStoreLocation();

            //choosing correct order address
            var orderAddress = postProcessPaymentRequest.Order.PickupInStore
                    ? postProcessPaymentRequest.Order.PickupAddress
                    : postProcessPaymentRequest.Order.ShippingAddress;

            string timeStr = AlphaPayQRCodeHelper.GetUTCTimeStampString();
            string nonceStr = AlphaPayQRCodeHelper.GetRamboString(4, 2);
            //create query parameters
            return new Dictionary<string, string>
            {
                //UTC毫秒时间戳
                ["time"] = timeStr,
                //随机字符串
                ["nonce_str"] = nonceStr,
                //签名（需生成一个新的sign）
                ["sign"] = AlphaPayQRCodeHelper.GetSign(_alphaPayQRCodePaymentSettings.PartnerCode ,timeStr,nonceStr, _alphaPayQRCodePaymentSettings.CredentialCode)
            };
        }
        private string CreateQueryJSON(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get store location
            var storeLocation = _webHelper.GetStoreLocation();
            QRCodePUTJsonClass qRCodePUTJsonClass = new QRCodePUTJsonClass
            {
                description = postProcessPaymentRequest.Order.OrderItems.First().Product.Name,
                price = (int)(postProcessPaymentRequest.Order.OrderTotal*100),
                currency = "CAD",
                channel = "Wechat",
                notify_url= $"{storeLocation}Plugins/PaymentAlphaPayQRCode/IPNHandler",
                @operator = postProcessPaymentRequest.Order.Customer.CustomerGuid.ToString()
            };
            return JsonConvert.SerializeObject(qRCodePUTJsonClass);

        }

        #endregion

        #region Json Class
        [Serializable]
        public class QRCodePUTJsonClass
        {
            public string description { get; set; }
            public int price { get; set; }
            public string currency { get; set; }
            public string channel { get; set; }
            public string notify_url { get; set; }
            //用@强行命名保留字变量
            public string @operator { get; set; }
        }
        #endregion
        }
}
