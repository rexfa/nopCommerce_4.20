namespace Nop.Plugin.Shipping.Rush.Domain
{
    /// <summary>
    /// Represents customer classification
    /// </summary>
    /// <remarks>
    /// Updated at January 7, 2019
    /// </remarks>
    public enum CustomerClassification
    {
        /// <summary>
        /// Rates Associated with Shipper Number
        /// </summary>
        [RushCode("00")]
        RatesAssociatedWithShipperNumber,

        /// <summary>
        /// Daily Rates
        /// </summary>
        [RushCode("01")]
        DailyRates,

        /// <summary>
        /// Retail Rates
        /// </summary>
        [RushCode("04")]
        RetailRates,

        /// <summary>
        /// Regional Rates
        /// </summary>
        [RushCode("05")]
        RegionalRates,

        /// <summary>
        /// General List Rates
        /// </summary>
        [RushCode("06")]
        GeneralListRates,

        /// <summary>
        /// Standard List Rates
        /// </summary>
        [RushCode("53")]
        StandardListRates
    }
}