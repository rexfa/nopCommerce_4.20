﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AlphaPayQRCode.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.AlphaPayQRCode.Controllers
{
    public class PaymentAlphaPayQRCodeController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public PaymentAlphaPayQRCodeController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Utilities
        /// <summary>
        /// 处理重复付费
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="newPaymentStatus"></param>
        /// <param name="transactionId"></param>
        /// <param name="ipnInfo"></param>

        protected virtual void ProcessRecurringPayment(string invoiceId, PaymentStatus newPaymentStatus, string transactionId, string ipnInfo)
        {
            Guid orderNumberGuid;

            try
            {
                orderNumberGuid = new Guid(invoiceId);
            }
            catch
            {
                orderNumberGuid = Guid.Empty;
            }

            var order = _orderService.GetOrderByGuid(orderNumberGuid);
            if (order == null)
            {
                _logger.Error("AlphaPayQRCode IPN. Order is not found", new NopException(ipnInfo));
                return;
            }

            var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);

            foreach (var rp in recurringPayments)
            {
                switch (newPaymentStatus)
                {
                    case PaymentStatus.Authorized:
                    case PaymentStatus.Paid:
                        {
                            var recurringPaymentHistory = rp.RecurringPaymentHistory;
                            if (!recurringPaymentHistory.Any())
                            {
                                //first payment
                                var rph = new RecurringPaymentHistory
                                {
                                    RecurringPaymentId = rp.Id,
                                    OrderId = order.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                rp.RecurringPaymentHistory.Add(rph);
                                _orderService.UpdateRecurringPayment(rp);
                            }
                            else
                            {
                                //next payments
                                var processPaymentResult = new ProcessPaymentResult
                                {
                                    NewPaymentStatus = newPaymentStatus
                                };
                                if (newPaymentStatus == PaymentStatus.Authorized)
                                    processPaymentResult.AuthorizationTransactionId = transactionId;
                                else
                                    processPaymentResult.CaptureTransactionId = transactionId;

                                _orderProcessingService.ProcessNextRecurringPayment(rp,
                                    processPaymentResult);
                            }
                        }

                        break;
                    case PaymentStatus.Voided:
                        //failed payment
                        var failedPaymentResult = new ProcessPaymentResult
                        {
                            Errors = new[] { $"AlphaPayQRCode IPN. Recurring payment is {nameof(PaymentStatus.Voided).ToLower()} ." },
                            RecurringPaymentFailed = true
                        };
                        _orderProcessingService.ProcessNextRecurringPayment(rp, failedPaymentResult);
                        break;
                }
            }

            //OrderService.InsertOrderNote(newOrder.OrderId, sb.ToString(), DateTime.UtcNow);
            _logger.Information("AlphaPayQRCode IPN. Recurring info", new NopException(ipnInfo));
        }
        /// <summary>
        /// 处理交易，修改交易状态
        /// </summary>
        /// <param name="orderNumber">订单GUID</param>
        /// <param name="notifyInfo">信息</param>
        /// <param name="newPaymentStatus">新状态</param>
        /// <param name="mcGross">交易金额</param>
        /// <param name="transactionId">交易号</param>
        protected virtual void ProcessPayment(string orderNumber, string notifyInfo, PaymentStatus newPaymentStatus, decimal mcGross, string transactionId)
        {
            Guid orderNumberGuid;

            try
            {
                orderNumberGuid = new Guid(orderNumber);
            }
            catch
            {
                orderNumberGuid = Guid.Empty;
            }

            var order = _orderService.GetOrderByGuid(orderNumberGuid);

            if (order == null)
            {
                _logger.Error("AlphaPay. Order is not found", new NopException(notifyInfo));
                return;
            }

            //order note
            order.OrderNotes.Add(new OrderNote
            {
                Note = notifyInfo,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            _orderService.UpdateOrder(order);

            //validate order total
            if ((newPaymentStatus == PaymentStatus.Authorized || newPaymentStatus == PaymentStatus.Paid) && !Math.Round(mcGross, 2).Equals(Math.Round(order.OrderTotal, 2)))
            {
                var errorStr = $"AlphaPay. Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                //log
                _logger.Error(errorStr);
                //order note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                return;
            }

            switch (newPaymentStatus)
            {
                case PaymentStatus.Authorized:
                    if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                        _orderProcessingService.MarkAsAuthorized(order);
                    break;
                case PaymentStatus.Paid:
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = transactionId;
                        _orderService.UpdateOrder(order);

                        _orderProcessingService.MarkOrderAsPaid(order);
                    }

                    break;
                case PaymentStatus.Refunded:
                    var totalToRefund = Math.Abs(mcGross);
                    if (totalToRefund > 0 && Math.Round(totalToRefund, 2).Equals(Math.Round(order.OrderTotal, 2)))
                    {
                        //refund
                        if (_orderProcessingService.CanRefundOffline(order))
                            _orderProcessingService.RefundOffline(order);
                    }
                    else
                    {
                        //partial refund
                        if (_orderProcessingService.CanPartiallyRefundOffline(order, totalToRefund))
                            _orderProcessingService.PartiallyRefundOffline(order, totalToRefund);
                    }

                    break;
                case PaymentStatus.Voided:
                    if (_orderProcessingService.CanVoidOffline(order))
                        _orderProcessingService.VoidOffline(order);

                    break;
            }
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var alphaPayQRCodePaymentSettings = _settingService.LoadSetting<AlphaPayQRCodePaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                PartnerCode = alphaPayQRCodePaymentSettings.PartnerCode,
                APPID = alphaPayQRCodePaymentSettings.APPID,
                CredentialCode = alphaPayQRCodePaymentSettings.CredentialCode,
                PassProductNamesAndTotals = alphaPayQRCodePaymentSettings.PassProductNamesAndTotals,
                AdditionalFee = alphaPayQRCodePaymentSettings.AdditionalFee,
                AdditionalFeePercentage = alphaPayQRCodePaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.AlphaPayQRCode/Views/Configure.cshtml", model);

            model.PartnerCode_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.PartnerCode, storeScope);
            model.APPID_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.APPID, storeScope);
            model.CredentialCode_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.CredentialCode, storeScope);
            model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
            model.AdditionalFee_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(alphaPayQRCodePaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/Payments.AlphaPayQRCode/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var alphaPayQRCodePaymentSettings = _settingService.LoadSetting<AlphaPayQRCodePaymentSettings>(storeScope);

            //save settings
            alphaPayQRCodePaymentSettings.PartnerCode = model.PartnerCode;
            alphaPayQRCodePaymentSettings.APPID = model.APPID;
            alphaPayQRCodePaymentSettings.CredentialCode = model.CredentialCode;
            alphaPayQRCodePaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            alphaPayQRCodePaymentSettings.AdditionalFee = model.AdditionalFee;
            alphaPayQRCodePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.PartnerCode, model.PartnerCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.APPID, model.APPID_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.CredentialCode, model.CredentialCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(alphaPayQRCodePaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate AlphaPayQRCode rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult RoundingWarning(bool passProductNamesAndTotals)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = _localizationService.GetResource("Plugins.Payments.AlphaPayQRCode.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        /// <summary>
        /// ReturnHandler 支付完成跳转的页面
        /// </summary>
        /// <returns></returns>
        public IActionResult ReturnHandler()
        {
            //byte[] parameters;

            //using (var stream = new MemoryStream())
            //{
            //    Request.Body.CopyTo(stream);
            //    parameters = stream.ToArray();
            //}
            ////获取全部Json
            //var strRequest = Encoding.ASCII.GetString(parameters);
            //_logger.Information("ReturnHandler : " + strRequest);


            //if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.AlphaPayQRCode") is AlphaPayQRCodePaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
            //    throw new NopException("AlphaPay QRCode module cannot be loaded");

            ////var notifyJsonClass = JsonConvert.DeserializeObject<NotifyJsonClass>(strRequest);

            //if (!processor.VerifyNotify(strRequest, out var values))
            //{
            //    _logger.Error("Sign verify failed.", new NopException(strRequest));

            //    //nothing should be rendered to visitor
            //    return Content(string.Empty);
            //}
            ////total_fee	String	订单金额，单位是最小货币单位
            ////real_fee String  支付金额，单位是最小货币单位
            ////mcGross AlphaPay 是乘100的
            //var mcGross = decimal.Zero;

            //try
            //{
            //    mcGross = decimal.Parse(values["real_fee"], new CultureInfo("en-US"))/100;
            //}
            //catch
            //{
            //    // ignored
            //}
            //values.TryGetValue("total_fee", out var total_fee);
            //values.TryGetValue("partner_order_id", out var partner_order_id);//商户订单ID
            //values.TryGetValue("order_id", out var order_id);//AlphaPay 订单ID
            //values.TryGetValue("rate", out var rate);//交易时使用的汇率，1CAD=?CNY
            //values.TryGetValue("create_time", out var create_time); //订单创建时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
            //values.TryGetValue("pay_time", out var pay_time); //订单支付时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
            //values.TryGetValue("channel", out var channel); //支付渠道 Alipay、Wechat

            //var sb = new StringBuilder();
            //sb.AppendLine("AlphaPayQRCode Return:");
            //foreach (var kvp in values)
            //{
            //    sb.AppendLine(kvp.Key + ": " + kvp.Value);
            //}

            ////var newPaymentStatus = AlphaPayQRCodeHelper.GetPaymentStatus(paymentStatus, pendingReason);
            //var newPaymentStatus = PaymentStatus.Paid;
            //sb.AppendLine("New payment status: " + newPaymentStatus);

            //var ipnInfo = sb.ToString();

            //var orderNumberGuid = Guid.Empty;
            //try
            //{
            //    orderNumberGuid = new Guid(partner_order_id);
            //}
            //catch
            //{
            //    // ignored
            //}

            //var order = _orderService.GetOrderByGuid(orderNumberGuid);
            ////不判断，直接处理付款
            //ProcessPayment(partner_order_id, ipnInfo, newPaymentStatus, mcGross, order_id);

            //return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            //return RedirectToRoute("CustomerInfo");
            return View("~/Plugins/Payments.AlphaPayQRCode/Views/PaymentCompleted.cshtml");


        }
        /// <summary>
        /// 异步通知
        /// </summary>
        /// <returns></returns>
        public IActionResult NotifyHandler()
        {
            byte[] parameters;

            using (var stream = new MemoryStream())
            {
                Request.Body.CopyTo(stream);
                parameters = stream.ToArray();
            }
            //获取全部Json
            var strRequest = Encoding.ASCII.GetString(parameters);

            _logger.Information("NotifyHandler : " + strRequest);

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.AlphaPayQRCode") is AlphaPayQRCodePaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("AlphaPay QRCode module cannot be loaded");

            //var notifyJsonClass = JsonConvert.DeserializeObject<NotifyJsonClass>(strRequest);

            if (!processor.VerifyNotify(strRequest, out var values))
            {
                _logger.Error("Sign verify failed.", new NopException(strRequest));

                //nothing should be rendered to visitor
                return Content(string.Empty);
            }
            //total_fee	String	订单金额，单位是最小货币单位
            //real_fee String  支付金额，单位是最小货币单位
            //mcGross AlphaPay 是乘100的
            var mcGross = decimal.Zero;

            try
            {
                mcGross = decimal.Parse(values["real_fee"], new CultureInfo("en-US"))/100;
            }
            catch
            {
                // ignored
            }
            values.TryGetValue("total_fee", out var total_fee);
            values.TryGetValue("partner_order_id", out var partner_order_id);//商户订单ID
            values.TryGetValue("order_id", out var order_id);//AlphaPay 订单ID
            values.TryGetValue("rate", out var rate);//交易时使用的汇率，1CAD=?CNY
            values.TryGetValue("create_time", out var create_time); //订单创建时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
            values.TryGetValue("pay_time", out var pay_time); //订单支付时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
            values.TryGetValue("channel", out var channel); //支付渠道 Alipay、Wechat

            var sb = new StringBuilder();
            sb.AppendLine("AlphaPayQRCode Notify:");
            foreach (var kvp in values)
            {
                sb.AppendLine(kvp.Key + ": " + kvp.Value);
            }

            //var newPaymentStatus = AlphaPayQRCodeHelper.GetPaymentStatus(paymentStatus, pendingReason);
            var newPaymentStatus = PaymentStatus.Paid;
            sb.AppendLine("New payment status: " + newPaymentStatus);

            var ipnInfo = sb.ToString();

            //switch (txnType)
            //{
            //    case "recurring_payment":
            //        ProcessRecurringPayment(rpInvoiceId, newPaymentStatus, txnId, ipnInfo);
            //        break;
            //    case "recurring_payment_failed":
            //        if (Guid.TryParse(rpInvoiceId, out var orderGuid))
            //        {
            //            var order = _orderService.GetOrderByGuid(orderGuid);
            //            if (order != null)
            //            {
            //                var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: order.Id)
            //                    .FirstOrDefault();
            //                //failed payment
            //                if (recurringPayment != null)
            //                    _orderProcessingService.ProcessNextRecurringPayment(recurringPayment,
            //                        new ProcessPaymentResult
            //                        {
            //                            Errors = new[] { txnType },
            //                            RecurringPaymentFailed = true
            //                        });
            //            }
            //        }

            //        break;
            //    default:
            //        values.TryGetValue("custom", out var orderNumber);
            //        ProcessPayment(orderNumber, ipnInfo, newPaymentStatus, mcGross, txnId);

            //        break;
            //}
            //不判断，直接处理付款
            ProcessPayment(partner_order_id, ipnInfo, newPaymentStatus, mcGross, order_id);
            //nothing should be rendered to visitor
            return Content(string.Empty);
        }

        public IActionResult CancelOrder()
        {
            var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();

            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("Homepage");
        }
        /// <summary>
        /// 错误显示
        /// http://www.24aibeauti.com/Plugins/PaymentAlphaPayQRCode/ErrorHandler?json=resultJSON
        /// </summary>
        /// <returns></returns>
        public IActionResult ErrorHandler()
        {
            var json = _webHelper.QueryString<string>("json");
            return Content(json);
        }
        #endregion
        #region Json Class
        //[Serializable]
        //class NotifyJsonClass
        //{
        //    public long time { get; set; }
        //    public string nonce_str { get; set; }
        //    public string sign { get; set; }
        //    public string partner_order_id { get; set; }
        //    public string order_id { get; set; }
        //    public string total_fee { get; set; }
        //    public string real_fee { get; set; }
        //    public double rate { get; set; }
        //    public string currency { get; set; }
        //    public string channel { get; set; }
        //    public string create_time { get; set; } // 订单创建时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
        //    public string pay_time { get; set; }  // 订单支付时间（最新订单为准）（yyyy-MM-dd HH:mm:ss，加拿大西部时间）
        //}
        #endregion
    }
}