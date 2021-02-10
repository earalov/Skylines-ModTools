namespace ModTools.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class EnumUtil
    {
        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;

        /// <summary>
        /// Converts any integer (including long) based enum to ulong .
        /// </summary>
        public static ulong ToUInt64(this Enum value)
        {
            Type enumType = value.GetType();
            bool signed = Enum.GetUnderlyingType(enumType).IsSignedInteger();
            if (signed)
                return (ulong)(value as IConvertible).ToInt64(null);
            else
                return (value as IConvertible).ToUInt64(null);
        }
    }
}
