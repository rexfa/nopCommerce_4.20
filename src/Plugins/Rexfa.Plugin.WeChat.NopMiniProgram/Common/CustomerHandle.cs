using System;
using System.Collections.Generic;
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


    }
}
