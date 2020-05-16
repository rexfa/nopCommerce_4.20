using System;
using System.Collections.Generic;
using System.Text;

namespace Rexfa.Plugin.WeChat.NopMiniProgram
{
    public class RexWechatMiniProgramDefaults
    {
        public const string VIEW_COMPONENT_NAME = "RexNopWXAppAuthentication";
        public static string ImportTestRoute => "Rexfa.Plugin.WeChat.NopMiniProgram.TestHandler";
        public static string ImportHomeRoute => "Rexfa.Plugin.WeChat.NopMiniProgram.HomeHandler";
        public static string ImportCustomerRoute => "Rexfa.Plugin.WeChat.NopMiniProgram.CustomerHandler";
        public static string ImportCustomerRegAndLogin => "Rexfa.Plugin.WeChat.NopMiniProgram.CustomerRegAndLoginHandler";


        public static string WXMPApiGetOpenidURL => "https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type={3}";
    }
}
