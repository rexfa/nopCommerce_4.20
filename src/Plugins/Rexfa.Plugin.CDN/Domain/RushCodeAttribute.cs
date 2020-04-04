using System;

namespace Nop.Plugin.Shipping.Rush.Domain
{
    /// <summary>
    /// Represents custom attribute for Rush code
    /// </summary>
    public class RushCodeAttribute : Attribute
    {
        #region Ctor

        public RushCodeAttribute(string codeValue)
        {
            Code = codeValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a code value
        /// </summary>
        public string Code { get; }

        #endregion
    }
}