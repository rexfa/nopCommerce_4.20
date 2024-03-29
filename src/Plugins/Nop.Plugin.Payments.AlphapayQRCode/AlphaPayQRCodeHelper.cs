﻿using System;
using System.Security.Cryptography;
using System.Text;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.AlphaPayQRCode
{
    public class AlphaPayQRCodeHelper
    {
        #region Properties

        /// <summary>
        /// Get nopCommerce partner code
        /// </summary>
        public static string NopCommercePartnerCode => "nopCommerce_SP";

        /// <summary>
        /// Get the generic attribute name that is used to store an order total that actually sent to AlphapayQRCode (used to PDT order total validation)
        /// </summary>
        public static string OrderTotalSentToAlphapayQRCode => "OrderTotalSentToAlphapayQRCode";

        #endregion

        #region Methods

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">AlphaPay payment status</param>
        /// <param name="pendingReason">AlphaPay pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (pendingReason.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }

                    break;
                case "processed":
                case "completed":
                case "canceled_reversal":
                    result = PaymentStatus.Paid;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = PaymentStatus.Voided;
                    break;
                case "refunded":
                case "reversed":
                    result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }

            return result;
        }



        #endregion
    }
}
