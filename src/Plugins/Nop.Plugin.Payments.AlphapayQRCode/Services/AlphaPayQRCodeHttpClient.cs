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
    }
}
