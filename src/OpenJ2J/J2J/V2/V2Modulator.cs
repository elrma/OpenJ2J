using OpenJ2J.Extensions;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.V3;
using OpenJ2J.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.V2
{
    public class V2Modulator : J2JModulator
    {
        #region ::Variables::

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

        #endregion

        #region ::Constructors::

        public V2Modulator(FileStream fileStream) : base(fileStream)
        {
            _fileStream = fileStream;

            InitializeModulator();
        }

        public void InitializeModulator()
        {
            if (_fileStream != null)
            {
                _fileSize = _fileStream.Length; // Bytes
                _blockSize = 10240000; // Bytes
                _blockCount = (byte)(_blockSize / (_fileSize * 100));

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

        public override bool Modulate(string outputPath)
        {
            return Modulate(outputPath, string.Empty);
        }

        public override bool Modulate(string outputPath, string password)
        {
            try
            {
                if (_fileStream != null)
                {
                    Log.Information($"Modulator variables are initialized. (File Size : {_fileSize}Byte, Block Size : {_blockSize}Byte, Block Count : {_blockCount * 2}Blocks)");

                    // Initializes the IV.
                    _initializationVector = VariableBuilder.GetIV(_blockSize, password);
                    string ivString = string.IsNullOrEmpty(password) ? "loop of 0x00~0xFF" : $"obfuscated by {password}";
                    Log.Information($"Initialization Vector is initialized. (IV : {ivString})");

                    using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        // Copy the file.
                        outputStream.SetLength(_fileStream.Length + 32); // 32 Bytes => File Signature.

                        byte[] buffer = new byte[2048];
                        int bytesRead;

                        while ((bytesRead = _fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }

                        byte[] block = new byte[_blockSize];

                        CRC32 crc32 = new CRC32();

                        // Modulate top blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Read(block, 0, _blockSize);

                            crc32.MemoryHash(block);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] = block[i];
                            }

                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Write(_initializationVector, 0, _blockSize);
                            outputStream.Flush();
                        }

                        // Modulate bottom blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = outputStream.Length - 32 - (_blockSize * (blockNumber + 1));
                            outputStream.Read(block, 0, _blockSize);

                            crc32.MemoryHash(block);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] = block[i];
                            }

                            outputStream.Position = outputStream.Length - 32 - (_blockSize * (blockNumber + 1));
                            outputStream.Write(_initializationVector, 0, _blockSize);
                            outputStream.Flush();
                        }

                        byte[] crcBytes = BitConverter.GetBytes(crc32.Hash);
                        Array.Reverse(crcBytes); // LE to BE.

                        // Writes the signature bytes.
                        Log.Information($"CRC32 is calculated. (HASH : {crcBytes.BytesToHexString()})");

                        // Bytes to UTF-8 Padding.
                        crcBytes = Encoding.UTF8.GetBytes(crcBytes.BytesToHexString());

                        byte[] signatureBytes = VariableBuilder.GetSignature(_blockCount, crcBytes);
                        outputStream.Position = outputStream.Length - 32;
                        outputStream.Write(signatureBytes, 0, 32);
                        outputStream.Flush();

                        return true;
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
