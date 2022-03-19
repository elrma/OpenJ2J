using OpenJ2J.Extensions;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.V3
{
    public class V3Validator : J2JValidator
    {
        #region ::Variables::

        /// <summary>
        /// The J2J Format Signature. (L3000009)
        /// </summary>
        private static string J2J_SIGNATURE = "4C33303030303039";

        private long _fileSize = 0;

        /// <summary>
        /// The file size. (Unit : Bytes)
        /// </summary>
        public long FileSize
        {
            get => _fileSize;
            set => _fileSize = value;
        }

        private int _blockSize = 0;

        /// <summary>
        /// The modulation unit. (Unit : Bytes)
        /// </summary>
        public int BlockSize
        {
            get => _blockSize;
            set => _blockSize = value;
        }

        private byte _blockCount = 0;

        /// <summary>
        /// The block size.
        /// </summary>
        public byte BlockCount
        {
            get => _blockCount;
            set => _blockCount = value;
        }

        private byte[] _initializationVector = new byte[0];

        /// <summary>
        /// The initialization vector.
        /// </summary>
        public byte[] InitializationVector
        {
            get => _initializationVector;
            set => _initializationVector = value;
        }

        private byte[] _originalChecksum = new byte[0];

        /// <summary>
        /// The original checksum of the file.
        /// </summary>
        public byte[] OriginalChecksum
        {
            get => _originalChecksum;
        }

        private byte[] _recalculatedChecksum = new byte[0];

        /// <summary>
        /// The re-calculated checksum of the file.
        /// </summary>
        public byte[] RecalculatedChecksum
        {
            get => _recalculatedChecksum;
        }

        #endregion

        #region ::Constructors::

        public V3Validator(FileStream fileStream) : base(fileStream)
        {
            _fileStream = fileStream;

            InitializeValidator();
        }

        public void InitializeValidator()
        {
            if (_fileStream != null)
            {
                _fileSize = _fileStream.Length; // Bytes
                _blockSize = 10240000; // Bytes
                _blockCount = (byte)(_blockSize / (_fileSize - 32 * 100));

                if (_blockCount == 0)
                {
                    _blockCount = 1;
                }
            }
            else
            {
                throw new NullReferenceException("The file stream is null.");
            }
        }

        #endregion

        #region ::Methods::

        public override bool Validate()
        {
            try
            {
                if (_fileStream != null)
                {
                    if (!_fileStream.CanRead)
                    {
                        throw new IOException("Unable to read the file.");
                    }

                    // Read the J2J signature section of the file.
                    byte[] buffer = new byte[8];

                    long position = _fileStream.Length - 8;
                    _fileStream.Position = position;
                    _fileStream.Read(buffer, 0, 8);

                    // Convert the J2J_SIGNATURE to a byte array.
                    byte[] signatureBytes = J2J_SIGNATURE.HexToBytes();

                    Log.Information($"The signature is catched. (Original Signature : {J2J_SIGNATURE}, Catched Signature : {buffer.BytesToHexString()})");

                    bool result = buffer.SequenceEqual(signatureBytes);
                    return result;
                }
                else
                {
                    throw new NullReferenceException("The file stream is null.");
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool ValidateWithChecksum()
        {
            return ValidateWithChecksum(string.Empty);
        }

        public override bool ValidateWithChecksum(string password)
        {
            try
            {
                if (_fileStream != null)
                {                    
                    // Gets the original checksum.
                    byte[] tail = new byte[32];
                    _fileStream.Position = _fileStream.Length - 32;
                    _fileStream.Read(tail, 0, 32);

                    _originalChecksum = VariableBuilder.GetChecksum(tail);

                    /* -------------------------------------------------- */

                    // Initializes the IV.
                    _initializationVector = VariableBuilder.GetIV(_blockSize, password);

                    byte[] block = new byte[_blockSize];

                    CRC32 crc32 = new CRC32();

                    // Demodulate top blocks.
                    for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                    {
                        _fileStream.Position = _blockSize * blockNumber;
                        _fileStream.Read(block, 0, _blockSize);

                        for (int i = 0; i < _blockSize; i++)
                        {
                            // XOR
                            block[i] ^= _initializationVector[i];
                            _initializationVector[i] ^= block[i];
                        }

                        crc32.MemoryHash(block);
                    }

                    // Demodulate bottom blocks.
                    for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                    {
                        _fileStream.Position = _fileStream.Length - 32 - (_blockSize * (blockNumber + 1));
                        _fileStream.Read(block, 0, _blockSize);

                        for (int i = 0; i < _blockSize; i++)
                        {
                            // XOR
                            block[i] ^= _initializationVector[i];
                            _initializationVector[i] ^= block[i];
                        }

                        crc32.MemoryHash(block);
                    }

                    byte[] crcBytes = BitConverter.GetBytes(crc32.Hash);
                    Array.Reverse(crcBytes); // LE to BE.

                    // Bytes to UTF-8 Padding.
                    crcBytes = Encoding.UTF8.GetBytes(crcBytes.BytesToHexString());

                    _recalculatedChecksum = crcBytes;

                    Log.Information($"Checksums are calculated. (Original Checksum : {_originalChecksum.BytesToHexString()}, Re-calculated Checksum : {_recalculatedChecksum.BytesToHexString()})");

                    if (_originalChecksum.SequenceEqual(_recalculatedChecksum))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new NullReferenceException("The file stream is null.");
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
