using Nop.Core.Configuration;

namespace Rexfa.Plugin.WeChat.NopMiniProgram
{
    public class RexWechatMiniProgramSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the Weixin AppID
        /// </summary>
        public string WXAppID { get; set; }
        /// <summary>
        /// Gets or sets the Weixin Name
        /// </summary>
        public string WXAppName { get; set; }
        /// <summary>
        /// Gets or sets the Weixin App Sign
        /// </summary>
        public string WXAppVerifyCode { get; set; }
        /// <summary>
        /// Gets or sets the Weixin App Key
        /// </summary>
        public string WXAppSecret { get; set; }

    }
}
