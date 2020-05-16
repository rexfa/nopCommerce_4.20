using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Services
{
    public partial class NMPHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly RexWechatMiniProgramSettings _rexWechatMiniProgramSettings;

        #endregion
        #region Ctor

        public NMPHttpClient(HttpClient client,
            RexWechatMiniProgramSettings rexWechatMiniProgramSettings)
        {
            //configure client
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CurrentVersion}");

            _httpClient = client;
            _rexWechatMiniProgramSettings = rexWechatMiniProgramSettings;
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

        public string GetToWebApi(string url)
        {
            var result = _httpClient.GetAsync(url).Result;

            return result.Content.ReadAsStringAsync().Result;
        }

        public string GetOpenidByResult(string result)
        {
            WXcode2SessionResult wxcode2SessionResult = JsonConvert.DeserializeObject<WXcode2SessionResult>(result);
            if(wxcode2SessionResult.errcode == 0)
            {
                return wxcode2SessionResult.openid;
            }
            else
            {
                // -1  系统繁忙，此时请开发者稍候再试
                // 0   请求成功
                // 40029   code 无效
                // 45011   频率限制，每个用户每分钟100次
                throw new NopException("WXcode2SessionResult is err,Code is " + wxcode2SessionResult.errcode);
            }

        }
        [Serializable]
        internal class WXcode2SessionResult{
            public string openid { get; set; }
            public string session_key { get; set; }
            public string unionid { get; set; }
            public int errcode { get; set; }
            public string errmsg { get; set; }
        }
        #endregion
    }
}
