using OpenJ2J.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.V3
{
    public static class VariableBuilder
    {
        public static byte[] GetIV(int blockSize, string? password = null)
        {
            byte[] iv = new byte[blockSize];

            if (string.IsNullOrEmpty(password))
            {
                // Fill the IV array.
                byte currentByte = 0;

                for (int i = 0; i < iv.Length; i++)
                {
                    if (currentByte > 255)
                    {
                        currentByte = 0;
                    }

                    iv[i] = currentByte++;
                }

                return iv;
            }
            else
            {
                iv = GetIV(blockSize, null); // Get a defualt IV.

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password); // Convert the password to UTF-8 bytes.

                // Obfuscate the IV.
                bool skip = false;
                int skipCount = 0;
                int currentIndex = 0;
                for (long i = 0; i < iv.LongLength; i++)
                {
                    if (skip)
                    {
                        skip = false;
                        skipCount++;
                        continue;
                    }

                    if (skipCount >= passwordBytes.Length)
                    {
                        skipCount = 0;
                        i += 1;
                    }

                    skip = true;

                    iv[i] += passwordBytes[currentIndex++];

                    if (currentIndex >= passwordBytes.Length)
                    {
                        currentIndex = 0;
                    }
                }

                return iv;
            }
        }

        public static byte[] GetSignature(byte blockCount, byte[] crc)
        {
            byte[] signature = new byte[32];

            // Fill blanks.
            byte[] blank = Enumerable.Repeat<byte>(0, 8).ToArray();
            blank.CopyTo(signature, 0);
            blank.CopyTo(signature, 9);

            // Fill the block size section.
            signature[8] = blockCount;

            // Fill the CRC sector.
            crc.CopyTo(signature, 16);

            // Fill the last sector.
            string lastString = "4C33303030303039";
            byte[] lastBytes = lastString.HexToBytes();
            lastBytes.CopyTo(signature, 24);

            return signature;
        }

        public static byte[] GetChecksum(byte[] tail)
        {
            if (tail.Length != 32)
            {
                throw new InvalidDataException("The length of the tail must be 32.");
            }

            byte[] checksum = new byte[8];
            Array.Copy(tail, 16, checksum, 0, 8);

            return checksum;
        }
    }
}
