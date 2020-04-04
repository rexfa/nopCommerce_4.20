namespace Nop.Plugin.Shipping.Rush.Domain
{
    /// <summary>
    /// Represents pickup type
    /// </summary>
    /// <remarks>
    /// Updated at January 7, 2019
    /// </remarks>
    public enum PickupType
    {
        /// <summary>
        /// Daily pickup
        /// </summary>
        [RushCode("01")]
        DailyPickup,

        /// <summary>
        /// Customer counter
        /// </summary>
        [RushCode("03")]
        CustomerCounter,

        /// <summary>
        /// One time pickup
        /// </summary>
        [RushCode("06")]
        OneTimePickup,

        /// <summary>
        /// On call air
        /// </summary>
        [RushCode("07")]
        OnCallAir,

        /// <summary>
        /// Letter center
        /// </summary>
        [RushCode("19")]
        LetterCenter,

        /// <summary>
        /// Air service center
        /// </summary>
        [RushCode("20")]
        AirServiceCenter
    }
}