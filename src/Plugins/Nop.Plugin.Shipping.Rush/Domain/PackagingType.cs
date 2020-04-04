namespace Nop.Plugin.Shipping.Rush.Domain
{
    /// <summary>
    /// Represents packaging type
    /// </summary>
    /// <remarks>
    /// Updated at January 7, 2019
    /// </remarks>
    public enum PackagingType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        [RushCode("00")]
        Unknown,

        /// <summary>
        /// Rush Letter
        /// </summary>
        [RushCode("01")]
        Letter,

        /// <summary>
        /// Customer supplied package
        /// </summary>
        [RushCode("02")]
        CustomerSuppliedPackage,

        /// <summary>
        /// Tube
        /// </summary>
        [RushCode("03")]
        Tube,

        /// <summary>
        /// PAK
        /// </summary>
        [RushCode("04")]
        PAK,

        /// <summary>
        /// Express Box
        /// </summary>
        [RushCode("21")]
        ExpressBox,

        /// <summary>
        /// 25 Kg Box
        /// </summary>
        [RushCode("24")]
        _25KgBox,

        /// <summary>
        /// 10 Kg Box
        /// </summary>
        [RushCode("25")]
        _10KgBox,

        /// <summary>
        /// Pallet
        /// </summary>
        [RushCode("30")]
        Pallet,

        /// <summary>
        /// Small Express Box
        /// </summary>
        [RushCode("2a")]
        SmallExpressBox,

        /// <summary>
        /// Medium Express Box
        /// </summary>
        [RushCode("2b")]
        MediumExpressBox,

        /// <summary>
        /// Large Express Box
        /// </summary>
        [RushCode("2c")]
        LargeExpressBox

    }
}