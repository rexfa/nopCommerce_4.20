using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;

namespace Rexfa.Plugin.WeChat.NopMiniProgram.Common
{

    /// <summary>
    /// 集中处理Customer的一些
    /// </summary>
    public class CustomerHandle
    {
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        public CustomerHandle(ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService)
        {
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
        }

        //public Customer RegistratyCustomerFromWMP()
        //{

        //}

        public string GetNMPOpenIdVerify(string openid,string verifyCode)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(openid + verifyCode));
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }



    }
}
