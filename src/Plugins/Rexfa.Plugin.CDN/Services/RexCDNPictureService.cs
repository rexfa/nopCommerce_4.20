using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using static SixLabors.ImageSharp.Configuration;

namespace Rexfa.Plugin.CDN.Services
{
    /// <summary>
    /// Represents Rush service
    /// </summary>
    public class RexCDNPictureService : PictureService
    {
        #region Constants


        #endregion

        #region Fields

        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly IDownloadService _downloadService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopFileProvider _fileProvider;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<PictureBinary> _pictureBinaryRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly RexCDNSettings _rexCDNSettings;
        private readonly ILogger _logger;

        private static string _pictureUrlCDNDomainNameString;

        #endregion

        #region Ctor

        public RexCDNPictureService(IDataProvider dataProvider,
            IDbContext dbContext,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            RexCDNSettings rexCDNSettings,
            ILogger logger):base(dataProvider,
            dbContext,
            downloadService,
            eventPublisher,
            httpContextAccessor,
            fileProvider,
            productAttributeParser,
            pictureRepository,
            pictureBinaryRepository,
            productPictureRepository,
            settingService,
            urlRecordService,
            webHelper,
            mediaSettings)
        {
            _dataProvider = dataProvider;
            _dbContext = dbContext;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _productAttributeParser = productAttributeParser;
            _pictureRepository = pictureRepository;
            _pictureBinaryRepository = pictureBinaryRepository;
            _productPictureRepository = productPictureRepository;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _mediaSettings = mediaSettings;
            _webHelper = webHelper;
            _rexCDNSettings = rexCDNSettings;
            _logger = logger;
            OneTimeInit();
        }

        #endregion

        #region Utilities
        protected void OneTimeInit()
        {
            if (_rexCDNSettings.PicFileDomainName.IndexOf("http") != -1)
            {
                _pictureUrlCDNDomainNameString = _rexCDNSettings.PicFileDomainName + "/images/";
            }
            else
            {
                _pictureUrlCDNDomainNameString = "http://" + _rexCDNSettings.PicFileDomainName + "/images/";
            }

        }
        #endregion

        #region Methods
        /// <summary>
        /// Get images path URL 
        /// </summary>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns></returns>
        protected override string GetImagesPathUrl(string storeLocation = null)
        {
            try
            {
                //_logger.Information("Call RexCDN _rexCDNSettings:" + _rexCDNSettings.ToString());
                if (_rexCDNSettings.UsePicFileDomainName)
                {
                    return _pictureUrlCDNDomainNameString;
                }
                else
                {
                    var pathBase = _httpContextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;

                    var imagesPathUrl = _mediaSettings.UseAbsoluteImagePath ? storeLocation : $"{pathBase}/";

                    imagesPathUrl = string.IsNullOrEmpty(imagesPathUrl)
                        ? _webHelper.GetStoreLocation()
                        : imagesPathUrl;

                    imagesPathUrl += "images/";
                    //_logger.Information("Rex CDN inf:" + imagesPathUrl);
                    return imagesPathUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Rex CDN EX:" +ex.Message, ex);
                return base.GetImagesPathUrl(storeLocation);
                
            }
        }

        #endregion
    }
}