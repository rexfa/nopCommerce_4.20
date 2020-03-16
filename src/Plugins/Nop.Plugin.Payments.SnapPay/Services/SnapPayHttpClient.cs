using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;

namespace Nop.Plugin.Payments.SnapPay.Services
{
    /// <summary>
    /// Represents the HTTP client to request SnapPay services
    /// </summary>
    public partial class SnapPayHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly SnapPayPaymentSettings _snapPayPaymentSettings;

        #endregion

        #region Ctor

        public SnapPayHttpClient(HttpClient client,
            SnapPayPaymentSettings snapPayPaymentSettings)
        {
            //configure client
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CurrentVersion}");

            _httpClient = client;
            _snapPayPaymentSettings = snapPayPaymentSettings;
        }

        #endregion

        #region Methods

        public string PostToWebApi(string url ,string postJSON)
        {
            //_httpClient.DefaultRequestHeaders.Remove(HeaderNames.ContentType);
            //_httpClient.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/json");
            _httpClient.DefaultRequestHeaders.Remove(HeaderNames.Accept);   
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

            //_httpClient.DefaultRequestHeaders.Accept.Add(HeaderNames.ContentType, "application/json");
            var content = new StringContent(postJSON, Encoding.UTF8, "application/json");
            var result = _httpClient.PostAsync(url, content).Result;
            return result.Content.ReadAsStringAsync().Result;
        }
        //public async Task<string> GetWebPayQRCodePage()
        //{ 
        //}

        /// <summary>
        /// Gets PDT details
        /// </summary>
        /// <param name="tx">TX</param>
        /// <returns>The asynchronous task whose result contains the PDT details</returns>
        public async Task<string> GetPdtDetailsAsync(string tx)
        {
            //get response
            var url =  "https://www.paypal.com/us/cgi-bin/webscr";
            var requestContent = new StringContent($"cmd=_notify-synch&at=",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <returns>The asynchronous task whose result contains the IPN verification details</returns>
        public async Task<string> VerifyIpnAsync(string formString)
        {
            //get response
            var url =   "https://ipnpb.paypal.com/cgi-bin/webscr";
            var requestContent = new StringContent($"cmd=_notify-validate&{formString}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        #endregion
    }
}