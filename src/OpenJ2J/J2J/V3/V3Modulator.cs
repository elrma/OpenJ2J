using Force.Crc32;
using OpenJ2J.Extensions;
using OpenJ2J.J2J.Abstractions;
using OpenJ2J.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.V3
{
    public class V3Modulator : J2JModulator
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

        public V3Modulator(FileStream fileStream) : base(fileStream)
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

        public byte[] GetIV(string? password = null)
        {
            byte[] iv = new byte[_blockSize];

            if (string.IsNullOrEmpty(password))
            {
                // Fill the IV array.
                byte currentByte = 0;

                for (int i=0; i< iv.Length; i++)
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
                iv = GetIV(null); // Get a defualt IV.

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

        private byte[] GetSignature(byte BlockCount, byte[] crc)
        {
            byte[] signature = new byte[32];

            // Fill blanks.
            byte[] blank = Enumerable.Repeat<byte>(0, 8).ToArray();
            blank.CopyTo(signature, 0);
            blank.CopyTo(signature, 9);

            // Fill the block size section.
            signature[8] = BlockCount;

            // Fill the CRC sector.
            crc.CopyTo(signature, 16);

            // Fill the last sector.
            string lastString = "4C33303030303039";
            byte[] lastBytes = lastString.HexToBytes();
            lastBytes.CopyTo(signature, 24);

            return signature;
        }

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
                    Log.Information($"Modulator variables are initialized. (File Size : {_fileSize}Byte, Block Size : {_blockSize}Byte, Block Count : {_blockCount}Blocks)");

                    // Initializes the IV.
                    _initializationVector = GetIV(password);
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

                        CRC32 crc32 = new CRC32(0x69746974);
                        byte[] crcBytes = new byte[0];

                        // Modulate top blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Read(block, 0, _blockSize);

                            crcBytes = crc32.ComputeHash(block);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] = block[i];
                            }

                            //crcBytes = crc32.ComputeHash(_initializationVector);

                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Write(_initializationVector, 0, _blockSize);
                            outputStream.Flush();
                        }

                        // Modulate bottom blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = outputStream.Length - 32 - (_blockSize * (blockNumber + 1));
                            outputStream.Read(block, 0, _blockSize);

                            crcBytes = crc32.ComputeHash(block);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] = block[i];
                            }

                            //crcBytes = crc32.ComputeHash(_initializationVector);

                            outputStream.Position = outputStream.Length - 32 - (_blockSize * (blockNumber + 1));
                            outputStream.Write(_initializationVector, 0, _blockSize);
                            outputStream.Flush();
                        }

                        // Writes the signature bytes.
                        Log.Information($"CRC32 is calculated. (HASH : {crcBytes.BytesToHexString()})");

                        // Bytes to UTF-8 Padding.
                        crcBytes = Encoding.UTF8.GetBytes(crcBytes.BytesToHexString());

                        byte[] signatureBytes = GetSignature(_blockCount, crcBytes);
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
