namespace Nop.Plugin.Shipping.Rush.Domain
{
    /// <summary>
    /// Represents delivery service
    /// </summary>
    /// <remarks>
    /// Updated at January 7, 2019
    /// </remarks>
    public enum DeliveryService
    {
        /// <summary>
        /// Next Day Air
        /// </summary>
        [RushCode("01")]
        NextDayAir,

        /// <summary>
        /// 2nd Day Air
        /// </summary>
        [RushCode("02")]
        _2ndDayAir,

        /// <summary>
        /// Ground
        /// </summary>
        [RushCode("03")]
        Ground,

        /// <summary>
        /// Worldwide Express
        /// </summary>
        [RushCode("07")]
        WorldwideExpress,

        /// <summary>
        /// Worldwide Expedited
        /// </summary>
        [RushCode("08")]
        WorldwideExpedited,

        /// <summary>
        /// Standard
        /// </summary>
        [RushCode("11")]
        Standard,

        /// <summary>
        /// 3 Day Select
        /// </summary>
        [RushCode("12")]
        _3DaySelect,

        /// <summary>
        /// Next Day Air Saver
        /// </summary>
        [RushCode("13")]
        NextDayAirSaver,

        /// <summary>
        /// Next Day Air Early
        /// </summary>
        [RushCode("14")]
        NextDayAirEarly,

        /// <summary>
        /// Worldwide Express Plus
        /// </summary>
        [RushCode("54")]
        WorldwideExpressPlus,

        /// <summary>
        /// 2nd Day Air A.M.
        /// </summary>
        [RushCode("59")]
        _2ndDayAirAm,

        /// <summary>
        /// Worldwide Saver
        /// </summary>
        [RushCode("65")]
        WorldwideSaver,

        /// <summary>
        /// Access Point Economy
        /// </summary>
        [RushCode("70")]
        AccessPointEconomy,

        /// <summary>
        /// Worldwide Express Freight Midday
        /// </summary>
        [RushCode("71")]
        WorldwideExpressFreightMidday,

        /// <summary>
        /// Express 12:00
        /// </summary>
        [RushCode("74")]
        Express1200,

        /// <summary>
        /// Heavy Goods
        /// </summary>
        [RushCode("75")]
        HeavyGoods,

        /// <summary>
        /// Today Standard
        /// </summary>
        [RushCode("82")]
        TodayStandard,

        /// <summary>
        /// Today Dedicated Courrier
        /// </summary>
        [RushCode("83")]
        TodayDedicatedCourrier,

        /// <summary>
        /// Today Express
        /// </summary>
        [RushCode("85")]
        TodayExpress,

        /// <summary>
        /// Today Express Saver
        /// </summary>
        [RushCode("86")]
        TodayExpressSaver,

        /// <summary>
        /// Worldwide Express Freight
        /// </summary>
        [RushCode("96")]
        WorldwideExpressFreight
    }
}