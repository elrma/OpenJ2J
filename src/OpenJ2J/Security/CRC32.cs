using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.Security
{
    public class CRC32
    {
        private uint[] _checksumTable;
        private uint _polynomial;

        public CRC32()
        {
            _polynomial = 0xEDB88320;

            _checksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (_polynomial ^ (item >> 1)) : (item >> 1);
                _checksumTable[index] = item;
            }
        }

        public CRC32(uint polynomial)
        {
            _polynomial = polynomial;

            _checksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (_polynomial ^ (item >> 1)) : (item >> 1);
                _checksumTable[index] = item;
            }
        }

        public void Initialize(uint polynomial)
        {
            _polynomial = polynomial;

            _checksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (_polynomial ^ (item >> 1)) : (item >> 1);
                _checksumTable[index] = item;
            }
        }

        public byte[] ComputeHash(Stream stream)
        {
            uint result = 0xFFFFFFFF; // CRC32 Seed(Default Value)

            int current;
            while ((current = stream.ReadByte()) != -1)
                result = _checksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

            byte[] hash = BitConverter.GetBytes(~result);
            Array.Reverse(hash);
            return hash;
        }

        public byte[] ComputeHash(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
                return ComputeHash(stream);
        }
    }
}
