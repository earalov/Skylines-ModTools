using System;
using System.Linq;
using UnityEngine;

namespace ModTools.Utils
{
    internal static class ParseHelper
    {
        public static bool TryParse(string text, Type targetType, out object result)
        {
            if (string.IsNullOrEmpty(text))
            {
                result = default;
                return false;
            }

            try
            {
                result = Convert.ChangeType(text, targetType);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static bool TryParse<T>(string text, out T result)
            where T : struct
        {
            if (string.IsNullOrEmpty(text))
            {
                result = default;
                return false;
            }

            try
            {
                return Parsers<T>.TryParse(text, out result);
            }
            catch
            {
                result = default;
                return false;
            }
        }

        private static class Parsers<T>
        {
            private static readonly TryParseDelegate Parser = (TryParseDelegate)Delegate.CreateDelegate(typeof(TryParseDelegate), typeof(T), "TryParse");

            private delegate bool TryParseDelegate(string text, out T result);

            public static bool TryParse(string text, out T result) => Parser(text, out result);
        }

        public static string RemoveInvalidChars(string value, Type type) {
            try
            {
                if (type is null || !type.IsNumeric())
                    throw new ArgumentException("type must be numeric. got" + type);

                if(value is null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Length == 0 || TryParse(value, type, out _))
                {
                    return value;
                }

                string valids = "0123456789";
                if (type.IsFloatingPoint()) valids += ".eE-";
                var validChars = valids.ToList();

                for (int i = 1; i < value.Length; ++i)
                {
                    if (!validChars.Contains(value[i]))
                        value = value.Remove(i, 1);
                }

                if (type.IsSignedInteger()) validChars.Add('-');
                if (!validChars.Contains(value[0]))
                    value = value.Remove(0, 1);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return value;
        }
    }
}
