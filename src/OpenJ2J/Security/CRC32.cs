using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.Security
{
    public class CRC32
    {
        private uint[] _crc32Table = new uint[256];
        private uint _polynomial = 0xEDB88320; // CRC32 Polynomial

        private uint _hash = 0xFFFFFFFF;

        public uint Hash
        {
            get => _hash;
        }

        public CRC32()
        {
            Initialize();
        }

        public void Initialize()
        {
            _crc32Table = new uint[256];
            _polynomial = 0xEDB88320;
            _hash = 0xFFFFFFFF;

            uint crc32;

            for (int i=0; i<256; i++)
            {
                crc32 = (uint)i;

                for (int j=8; j>0; j--)
                {
                    if ((crc32 & 1) == 1)
                    {
                        crc32 = (crc32 >> 1) ^ _polynomial;
                    }
                    else
                    {
                        crc32 >>= 1;
                    }
                }
                _crc32Table[i] = crc32;
            }
        }

        private void ComputeHash(byte buffer, ref uint crc32)
        {
            crc32 = ((crc32) >> 8) ^ _crc32Table[buffer ^ (crc32 & 0x000000FF)];
        }

        public void MemoryHash(byte[] buffer)
        {
            if (_crc32Table == null)
            {
                throw new InvalidOperationException("The CRC32 table does not initialized.");
            }

            for (int i=0; i<buffer.Length; i++)
            { 
                ComputeHash(buffer[i], ref _hash);
            }

            _hash = ~(_hash);
        }
    }
}
