using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Media;

namespace Rexfa.Plugin.CDN.Services
{
    /// <summary>
    /// Represents Rush service
    /// </summary>
    public class RexCDNPictureService : PictureService, IPictureService
    {
        #region Constants


        #endregion

        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMeasureService _measureService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IWorkContext _workContext;
        private readonly RexCDNSettings _rexCDNSettings;

        #endregion

        #region Ctor

        public RexCDNPictureService(CurrencySettings currencySettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ILogger logger,
            IMeasureService measureService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IWorkContext workContext,
            RexCDNSettings rexCDNSettings)
        {
            _currencySettings = currencySettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _logger = logger;
            _measureService = measureService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _workContext = workContext;
            _rexCDNSettings = rexCDNSettings;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods


        

        #endregion
    }
}