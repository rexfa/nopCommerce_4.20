using System;
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
        /// <summary> 
        /// 获取时间戳 
        /// </summary> 
        /// <returns>UTC</returns> 
        public static string GetUTCTimeStampString()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// 转换时间戳为C#时间
        /// </summary>
        /// <param name="timeStamp">时间戳 单位：毫秒</param>
        /// <returns>C#时间</returns>
        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(timeStamp);
            return dt;
        }
        public static string GetRamboString(int codeCount,int rep)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < codeCount; i++)
            {
                int num = random.Next();
                str = str + ((char)(0x30 + ((ushort)(num % 10)))).ToString();
            }
            return str;
        }
        /// <summary>
        /// 获取平台要求的sign
        /// </summary>
        /// <param name="partner_code"></param>
        /// <param name="time"></param>
        /// <param name="nonce_str"></param>
        /// <param name="credential_code"></param>
        /// <returns></returns>
        public static string GetSign(string partner_code ,string time ,string  nonce_str,string credential_code)
        {
            string valid_string = partner_code + "&" + time + "&" + nonce_str + "&" + credential_code;
            SHA256Managed crypt = new SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(valid_string));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString().ToLower();
        }
        #endregion
    }
}
