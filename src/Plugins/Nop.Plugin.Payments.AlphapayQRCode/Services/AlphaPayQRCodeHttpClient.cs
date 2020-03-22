using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;

namespace Nop.Plugin.Payments.AlphaPayQRCode.Services
{
    public class AlphaPayQRCodeHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly AlphaPayQRCodePaymentSettings _alphaPayQRCodePaymentSettings;

        #endregion

        #region Ctor

        public AlphaPayQRCodeHttpClient(HttpClient client,
            AlphaPayQRCodePaymentSettings alphaPayQRCodePaymentSettings)
        {
            //configure client
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CurrentVersion}");

            _httpClient = client;
            _alphaPayQRCodePaymentSettings = alphaPayQRCodePaymentSettings;
        }

        #endregion

        #region Methods
        public string PostToWebApi(string url, string postJSON)
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
        public string PutToWebApi(string url, string postJSON)
        {
            //_httpClient.DefaultRequestHeaders.Remove(HeaderNames.ContentType);
            //_httpClient.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/json");
            _httpClient.DefaultRequestHeaders.Remove(HeaderNames.Accept);
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");

            //_httpClient.DefaultRequestHeaders.Accept.Add(HeaderNames.ContentType, "application/json");
            var content = new StringContent(postJSON, Encoding.UTF8, "application/json");
            var result = _httpClient.PutAsync(url, content).Result;
            return result.Content.ReadAsStringAsync().Result;
        }
        #endregion
    }
}
