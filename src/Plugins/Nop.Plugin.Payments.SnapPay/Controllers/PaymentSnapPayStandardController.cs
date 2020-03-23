using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.SnapPay.Models;
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

namespace Nop.Plugin.Payments.SnapPay.Controllers
{
    public class PaymentSnapPayController : BasePaymentController
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

        public PaymentSnapPayController(IGenericAttributeService genericAttributeService,
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
                _logger.Error("SnapPay IPN. Order is not found", new NopException(ipnInfo));
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
                            Errors = new[] { $"SnapPay IPN. Recurring payment is {nameof(PaymentStatus.Voided).ToLower()} ." },
                            RecurringPaymentFailed = true
                        };
                        _orderProcessingService.ProcessNextRecurringPayment(rp, failedPaymentResult);
                        break;
                }
            }

            //OrderService.InsertOrderNote(newOrder.OrderId, sb.ToString(), DateTime.UtcNow);
            _logger.Information("SnapPay IPN. Recurring info", new NopException(ipnInfo));
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
                _logger.Error("SnapPay notifyInfo. Order is not found", new NopException(notifyInfo));
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
                var errorStr = $"SnapPay notifyInfo. Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
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
            var snapPayPaymentSettings = _settingService.LoadSetting<SnapPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                APPID = snapPayPaymentSettings.APPID,
                MerchantID = snapPayPaymentSettings.MerchantID,
                SigningKey = snapPayPaymentSettings.SigningKey,
                PassProductNamesAndTotals = snapPayPaymentSettings.PassProductNamesAndTotals,
                AdditionalFee = snapPayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = snapPayPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.SnapPay/Views/Configure.cshtml", model);

            model.APPID_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.APPID, storeScope);
            model.MerchantID_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.MerchantID, storeScope);
            model.SigningKey_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.SigningKey, storeScope);
            model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
            model.AdditionalFee_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(snapPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            return View("~/Plugins/Payments.SnapPay/Views/Configure.cshtml", model);
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
            var snapPayPaymentSettings = _settingService.LoadSetting<SnapPayPaymentSettings>(storeScope);

            //save settings
            snapPayPaymentSettings.APPID = model.APPID;
            snapPayPaymentSettings.MerchantID = model.MerchantID;
            snapPayPaymentSettings.SigningKey = model.SigningKey;
            snapPayPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            snapPayPaymentSettings.AdditionalFee = model.AdditionalFee;
            snapPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.APPID, model.APPID_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.MerchantID, model.MerchantID_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.SigningKey, model.SigningKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(snapPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate SnapPay rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult RoundingWarning(bool passProductNamesAndTotals)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = _localizationService.GetResource("Plugins.Payments.SnapPay.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }
        /// <summary>
        /// ReturnHandler old PDT 支付完成跳转的页面
        /// </summary>
        /// <returns></returns>
        public IActionResult ReturnHandler()
        {
            var out_trade_no = _webHelper.QueryString<string>("out_trade_no");
            var trade_status = _webHelper.QueryString<string>("trade_status");

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.SnapPay") is SnapPayPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("SnapPay module cannot be loaded");

            var order = _orderService.GetOrderByAuthorizationTransactionIdAndPaymentMethod(out_trade_no, "SnapPay");
            if (order == null)
                return RedirectToAction("Index", "Home", new { area = string.Empty });


            //order note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "SnapPay Web Return.",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            
        }
        /// <summary>
        /// NotifyHandler  ipn   后台通知交互  post  json  重点开发对象
        /// http://www.24aibeauti.com/Plugins/PaymentSnapPay/NotifyHandler
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
            var strRequest = Encoding.UTF8.GetString(parameters);
            _logger.Information("NotifyHandler==" + strRequest);

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.SnapPay") is SnapPayPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("SnapPay module cannot be loaded");

            if (!processor.GetNotifyData(strRequest, out Dictionary<string, string> values))
            {
                _logger.Error("SnapPay Notify failed.", new NopException(strRequest));

                //nothing should be rendered to visitor
                return Content(string.Empty);
            }
            else
            {
                ProcessPayment(values["out_order_no"], "SnapPayNotify" + values["exchange_rate"] + values["payment_method"] + values["customer_paid_amount"], PaymentStatus.Paid, decimal.Parse(values["trans_amount"]), values["trans_no"]);
                string successReponses = "{\"code\": \"0\"}"; 
                //Content-Type: application/json
                return Content(successReponses, "application/json");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// http://www.24aibeauti.com/Plugins/PaymentSnapPay/ErrorHandler?json=resultJSON
        /// </summary>
        /// <returns></returns>
        public IActionResult ErrorHandler() 
        {
            var json = _webHelper.QueryString<string>("json");
            return Content(json);
        }

        #endregion
    }
}