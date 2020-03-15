using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Payments.SnapPay.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.Payments.SnapPay
{
    /// <summary>
    /// SnapPay payment processor
    /// </summary>
    public class SnapPayPaymentProcessor : BasePlugin, IPaymentMethod
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
        private readonly SnapPayHttpClient _snapPayHttpClient;
        private readonly SnapPayPaymentSettings _snapPayPaymentSettings;

        #endregion

        #region Ctor

        public SnapPayPaymentProcessor(CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            SnapPayHttpClient snapPayHttpClient,
            SnapPayPaymentSettings snapPayPaymentSettings)
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
            _snapPayHttpClient = snapPayHttpClient;
            _snapPayPaymentSettings = snapPayPaymentSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets PDT details
        /// </summary>
        /// <param name="tx">TX</param>
        /// <param name="values">Values</param>
        /// <param name="response">Response</param>
        /// <returns>Result</returns>
        public bool GetPdtDetails(string tx, out Dictionary<string, string> values, out string response)
        {
            response = WebUtility.UrlDecode(_snapPayHttpClient.GetPdtDetailsAsync(tx).Result);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool firstLine = true, success = false;
            foreach (var l in response.Split('\n'))
            {
                var line = l.Trim();
                if (firstLine)
                {
                    success = line.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
                    firstLine = false;
                }
                else
                {
                    var equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }
            }

            return success;
        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <param name="values">Values</param>
        /// <returns>Result</returns>
        public bool VerifyIpn(string formString, out Dictionary<string, string> values)
        {
            var response = WebUtility.UrlDecode(_snapPayHttpClient.VerifyIpnAsync(formString).Result);
            var success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in formString.Split('&'))
            {
                var line = l.Trim();
                var equalPox = line.IndexOf('=');
                if (equalPox >= 0)
                    values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
            }

            return success;
        }

        /// <summary>
        /// Create common query parameters for the request 拼接数据
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

            //create query parameters
            return new Dictionary<string, string>
            {
                //APPID
                ["app_id"] = _snapPayPaymentSettings.APPID,
                ["format"] = "JSON",
                //the character set and character encoding
                ["charset"] = "UTF-8",


                ["sign_type"] = "MD5",
                ["sign"] = "7e2083699dd510575faa1c72f9e35d43",
                ["version"] = "1.0",
                //时间戳
                ["timestamp"] = "2018-08-02 15:16:51",
                //此接口固定值为：pay.webpay
                ["method"] = "pay.webpay",
                //商户id
                ["merchant_no"] = _snapPayPaymentSettings.MerchantID,
                //支付方式 ALIPAY-支付宝            UNIONPAY - 银联国际
                ["payment_method"] = "ALIPAY",
                //商户平台订单号  可以用GUID
                ["out_order_no"] = postProcessPaymentRequest.Order.OrderGuid.ToString(),
                //符合ISO 4217标准的三位字母代码，如：CAD，USD标价币种必须与商户申请的结算币种一致，不填写则默认为CAD
                ["trans_currency"] = "CAD",

                //["trans_amount"] = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2).ToString("C"),

                ["description"] = postProcessPaymentRequest.Order.Customer.Username + postProcessPaymentRequest.Order.OrderItems.First().Product.Name,

                ["notify_url"] = $"{storeLocation}Plugins/PaymentSnapPay/NotifyHandler",
                ["return_url"] = $"{storeLocation}Plugins/PaymentSnapPay/ReturnHandler",

                ["attach"]="",
                //["effective_minutes"] = 15,
                ["browser_type"]= "PC"

                ////set return method to "2" (the customer redirected to the return URL by using the POST method, and all payment variables are included)
                //["rm"] = "2",

                //["bn"] = SnapPayHelper.NopCommercePartnerCode,
                //["currency_code"] = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)?.CurrencyCode,

                ////order identifier
                //["invoice"] = postProcessPaymentRequest.Order.CustomOrderNumber,
                //["custom"] = postProcessPaymentRequest.Order.OrderGuid.ToString(),

                ////PDT, IPN and cancel URL
                //["return"] = $"{storeLocation}Plugins/PaymentSnapPay/PDTHandler",
                //["notify_url"] = $"{storeLocation}Plugins/PaymentSnapPay/IPNHandler",
                //["cancel_return"] = $"{storeLocation}Plugins/PaymentSnapPay/CancelOrder",

                ////shipping address, if exists
                //["no_shipping"] = postProcessPaymentRequest.Order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "1" : "2",
                //["address_override"] = postProcessPaymentRequest.Order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "0" : "1",
                //["first_name"] = orderAddress?.FirstName,
                //["last_name"] = orderAddress?.LastName,
                //["address1"] = orderAddress?.Address1,
                //["address2"] = orderAddress?.Address2,
                //["city"] = orderAddress?.City,
                //["state"] = orderAddress?.StateProvince?.Abbreviation,
                //["country"] = orderAddress?.Country?.TwoLetterIsoCode,
                //["zip"] = orderAddress?.ZipPostalCode,
                //["email"] = orderAddress?.Email
            };
        }

        /// <summary>
        /// Add order items to the request query parameters
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        private void AddItemsParameters(IDictionary<string, string> parameters, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //upload order items
            parameters.Add("cmd", "_cart");
            parameters.Add("upload", "1");

            var cartTotal = decimal.Zero;
            var roundedCartTotal = decimal.Zero;
            var itemCount = 1;

            //add shopping cart items
            foreach (var item in postProcessPaymentRequest.Order.OrderItems)
            {
                var roundedItemPrice = Math.Round(item.UnitPriceExclTax, 2);

                //add query parameters
                parameters.Add($"item_name_{itemCount}", item.Product.Name);
                parameters.Add($"amount_{itemCount}", roundedItemPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", item.Quantity.ToString());

                cartTotal += item.PriceExclTax;
                roundedCartTotal += roundedItemPrice * item.Quantity;
                itemCount++;
            }

            //add checkout attributes as order items
            var checkoutAttributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);
            foreach (var attributeValue in checkoutAttributeValues)
            {
                var attributePrice = _taxService.GetCheckoutAttributePrice(attributeValue, false, postProcessPaymentRequest.Order.Customer);
                var roundedAttributePrice = Math.Round(attributePrice, 2);

                //add query parameters
                if (attributeValue.CheckoutAttribute == null) 
                    continue;

                parameters.Add($"item_name_{itemCount}", attributeValue.CheckoutAttribute.Name);
                parameters.Add($"amount_{itemCount}", roundedAttributePrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += attributePrice;
                roundedCartTotal += roundedAttributePrice;
                itemCount++;
            }

            //add shipping fee as a separate order item, if it has price
            var roundedShippingPrice = Math.Round(postProcessPaymentRequest.Order.OrderShippingExclTax, 2);
            if (roundedShippingPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Shipping fee");
                parameters.Add($"amount_{itemCount}", roundedShippingPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += postProcessPaymentRequest.Order.OrderShippingExclTax;
                roundedCartTotal += roundedShippingPrice;
                itemCount++;
            }

            //add payment method additional fee as a separate order item, if it has price
            var roundedPaymentMethodPrice = Math.Round(postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax, 2);
            if (roundedPaymentMethodPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Payment method fee");
                parameters.Add($"amount_{itemCount}", roundedPaymentMethodPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
                roundedCartTotal += roundedPaymentMethodPrice;
                itemCount++;
            }

            //add tax as a separate order item, if it has positive amount
            var roundedTaxAmount = Math.Round(postProcessPaymentRequest.Order.OrderTax, 2);
            if (roundedTaxAmount > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Tax amount");
                parameters.Add($"amount_{itemCount}", roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += postProcessPaymentRequest.Order.OrderTax;
                roundedCartTotal += roundedTaxAmount;
            }

            if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
            {
                //get the difference between what the order total is and what it should be and use that as the "discount"
                var discountTotal = Math.Round(cartTotal - postProcessPaymentRequest.Order.OrderTotal, 2);
                roundedCartTotal -= discountTotal;

                //gift card or rewarded point amount applied to cart in nopCommerce - shows in SnapPay as "discount"
                parameters.Add("discount_amount_cart", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            //save order total that actually sent to SnapPay (used for PDT order total validation)
            _genericAttributeService.SaveAttribute(postProcessPaymentRequest.Order, SnapPayHelper.OrderTotalSentToSnapPay, roundedCartTotal);
        }

        /// <summary>
        /// Add order total to the request query parameters
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        private void AddOrderTotalParameters(IDictionary<string, string> parameters, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //round order total
            var roundedOrderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);

            parameters.Add("cmd", "_xclick");
            parameters.Add("item_name", $"Order Number {postProcessPaymentRequest.Order.CustomOrderNumber}");
            parameters.Add("amount", roundedOrderTotal.ToString("0.00", CultureInfo.InvariantCulture));

            //save order total that actually sent to SnapPay (used for PDT order total validation)
            _genericAttributeService.SaveAttribute(postProcessPaymentRequest.Order, SnapPayHelper.OrderTotalSentToSnapPay, roundedOrderTotal);
        }

        private string CreateQueryJSON(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get store location
            var storeLocation = _webHelper.GetStoreLocation();
            string timestampstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            WebPaymentData webPaymentData = new WebPaymentData
            {
                app_id = _snapPayPaymentSettings.APPID,
                format= "JSON",
                charset = "UTF-8",
                sign_type = "MD5",
                //sign = _snapPayPaymentSettings.SigningKey,
                version = "1.0",
                timestamp = timestampstring,
                method = "pay.webpay",
                merchant_no = _snapPayPaymentSettings.MerchantID,
                payment_method = "ALIPAY",
                out_order_no = postProcessPaymentRequest.Order.OrderGuid.ToString(),
                trans_currency = "CAD",
                description = postProcessPaymentRequest.Order.OrderItems.First().Product.Name,
                trans_amount =(double)postProcessPaymentRequest.Order.OrderTotal,
                notify_url = $"{storeLocation}Plugins/PaymentSnapPay/NotifyHandler",
                return_url = $"{storeLocation}Plugins/PaymentSnapPay/ReturnHandler",
                attach =  new AttachClass(),
                effective_minutes = 30,
                browser_type =  "PC"
            };
            string sign  = GetSign(webPaymentData);
            webPaymentData.sign = sign;
            return JsonConvert.SerializeObject(webPaymentData);

        }

        private WebAipReturns GetWebAipReturns(string resultJSON)
        {
            var result  = JsonConvert.DeserializeObject<WebAipReturns>(resultJSON);
            return result;
        }
        public string GetSign(WebPaymentData webPaymentData)
        {
            string[] parameters = new string[]
            {
                "app_id="+webPaymentData.app_id,
                "browser_type="+webPaymentData.browser_type,
                "charset="+webPaymentData.charset,
                "description="+ webPaymentData.description,
                "effective_minutes="+ webPaymentData.effective_minutes.ToString(),
             
                "format="+webPaymentData.format,
                "merchant_no="+ webPaymentData.merchant_no,
                "method="+ webPaymentData.method,
                "notify_url="+webPaymentData.notify_url,                
                "out_order_no="+ webPaymentData.out_order_no,

                "payment_method="+webPaymentData.payment_method,
                "return_url="+webPaymentData.return_url,
                "timestamp="+ webPaymentData.timestamp,
                "trans_amount="+webPaymentData.trans_amount,
                "trans_currency="+webPaymentData.trans_currency,

                "version="+ webPaymentData.version,
            };
            var oString = string.Join("&", parameters);
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(oString));
                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString().ToLower();
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL) 
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var baseUrl = "https://open.snappay.ca/api/gateway";


            //create common query parameters for the request
            //var queryParameters = CreateQueryParameters(postProcessPaymentRequest);
            //create post json for the request
            var queryJson = CreateQueryJSON(postProcessPaymentRequest);
            //whether to include order items in a transaction
            //if (_snapPayPaymentSettings.PassProductNamesAndTotals)
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
            //?out_order_no="+out_order_no+"&merchant_no="+merchant_no;
            //or add only an order total query parameters to the request


            //AddOrderTotalParameters(queryParameters, postProcessPaymentRequest);

            //remove null values from parameters
            //queryParameters = queryParameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            //    .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            //var url = QueryHelpers.AddQueryString(baseUrl, queryParameters);

            string  resultJSON = _snapPayHttpClient.PostToWebApi(baseUrl, queryJson);

            var resultClass = GetWebAipReturns(resultJSON);
            int resuleCode = int.Parse(resultClass.code);
            if (resuleCode == 0)
            {
                string url = resultClass.data.First().webpay_url;

                _httpContextAccessor.HttpContext.Response.Redirect(url);
            }
            else
            {
                var storeLocation = _webHelper.GetStoreLocation();
                string urlErr = $"{storeLocation}Plugins/PaymentSnapPay/ErrorHandler" +"?json=" + resultJSON;  
                _httpContextAccessor.HttpContext.Response.Redirect(urlErr);
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _snapPayPaymentSettings.AdditionalFee, _snapPayPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSnapPay/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentSnapPay";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            //_settingService.SaveSetting(new SnapPayPaymentSettings
            //{
            //    UseSandbox = true
            //});

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFee", "附加费用");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFee.Hint", "输入附加费用以向您的客户收费");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFeePercentage", "附加费用。百分比");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFeePercentage.Hint", "确定是否对订单总额应用一定百分比的附加费。 如果未启用，则使用固定值");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.MerchantID", "商户编号");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.MerchantID.Hint", "指定您的SnapPay商户编号");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PassProductNamesAndTotals", "将产品名称和订单总数传递给SnapPay");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PassProductNamesAndTotals.Hint", "检查是否应将产品名称和订单总数传递给SnapPay");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.APPID", "APPID身份令牌");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.APPID.Hint", "指定APPID身份令牌");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.SigningKey", "SigningKey身份令牌");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.SigningKey.Hint", "SigningKey身份令牌");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Fields.RedirectionTip", "您将被重定向到SnapPay网站以完成订单");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.Instructions", @"
            <p>
	            <b>如果您使用此网关，请确保SnapPay支持您的主要商店货币</b>
	            <br />
	            <br />要使用Merchant ID 、MD5 Sign Key、APP ID<br />
	            <br />商户后台访问地址 (点击 <a href=""https://mp.snappay.ca/web/index.html"" target=""_blank"">这里</a> ).
	            <br />设置好本服务器设置
	            <br />
            </p>");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.PaymentMethodDescription", "您将被重定向到SnapPay网站以完成付款");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.SnapPay.RoundingWarning", "似乎您已禁用\" ShoppingCartSettings.RoundPricesDuringCalculation \"设置。 请记住，这可能会导致订单总额不一致，因为SnapPay仅舍入到两位小数");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SnapPayPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.BusinessEmail");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.BusinessEmail.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PassProductNamesAndTotals");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PassProductNamesAndTotals.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PDTToken");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.PDTToken.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Instructions");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.RoundingWarning");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.SigningKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.SnapPay.Fields.SigningKey.Hint");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.SnapPay.PaymentMethodDescription");

        #endregion
        #region Json Class
        [Serializable]
        public class WebPaymentData
        {
            public string app_id { get; set; }
            public string format { get; set; }
            public string charset { get; set; }
            public string sign_type { get; set; }
            public string sign { get; set; }

            public string version { get; set; }
            public string timestamp { get; set; }
            public string method { get; set; }
            public string merchant_no { get; set; }
            public string payment_method { get; set; }

            public string out_order_no { get; set; }
            public string trans_currency { get; set; }
            public double trans_amount { get; set; }
            public string description { get; set; }
            public string notify_url { get; set; }

            public string return_url { get; set; }
            public AttachClass attach { get; set; }
            public int effective_minutes { get; set; }
            public string browser_type { get; set; }
        }
        [Serializable]
        public class AttachClass { }
        [Serializable]
        public class WebAipReturns
        {
            public string code { get; set; }
            public string msg { get; set; }
            public string sign { get; set; }
            public int total { get; set; }
            public string psn { get; set; }
            public ReturnData[] data { get; set; }
        }
        [Serializable]
        public class ReturnData
        {
            public string trans_no { get; set; }
            public string out_order_no { get; set; }
            public string merchant_no { get; set; }
            public string trans_status { get; set; }
            public string webpay_url { get; set; }
        }
        #endregion
    }
}