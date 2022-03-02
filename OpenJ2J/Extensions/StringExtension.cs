using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.Extensions
{
    public static class StringExtension
    {
        public static byte HexToByte(this string hex)
        {
            return Convert.ToByte(hex, 16);
        }

        public static byte[] HexToBytes(this string hexString)
        {
            byte[] result = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i+=2)
            {
                result[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return result;
        }

        public static IEnumerable<string> SplitInParts(this string text, int partLength)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < text.Length; i += partLength)
                yield return text.Substring(i, Math.Min(partLength, text.Length - i));
        }

        public static List<string> SplitByString(this string text, string seperator)
        {
            return text.Split(new string[1] { seperator }, StringSplitOptions.None).ToList();
        }
    }
}
