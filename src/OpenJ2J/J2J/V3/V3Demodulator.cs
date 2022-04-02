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
    public class V3Demodulator : J2JDemodulator
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

        public V3Demodulator(FileStream fileStream) : base(fileStream)
        {
            _fileStream = fileStream;

            InitializeDemodulator();
        }

        public void InitializeDemodulator()
        {
            if (_fileStream != null)
            {
                _fileSize = _fileStream.Length; // Bytes
                _blockSize = 10240000; // Bytes
                _blockCount = (byte)(_blockSize / (_fileSize-32 * 100));

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

        public override bool Demodulate(string outputPath, bool useForcer)
        {
            return Demodulate(outputPath, useForcer, string.Empty);
        }

        public override bool Demodulate(string outputPath, bool useForcer, string password)
        {
            try
            {
                if (_fileStream != null)
                {
                    Log.Information($"Demodulator variables are initialized. (File Size : {_fileSize} Bytes, Block Size : {_blockSize} Bytes, Block Count : {_blockCount*2} Blocks)");

                    // Validates the file.
                    V3Validator validator = new V3Validator(_fileStream);
                    bool validationResult = validator.ValidateWithChecksum(password);

                    if (!validationResult && !useForcer)
                    {
                        Log.Error("The checksum of the file does not match. If you want to force the operation, use the '-f, --use-forcer' option.");
                        return false;
                    }

                    // Initializes the IV.
                    _initializationVector = VariableBuilder.GetIV(_blockSize, password);
                    string ivString = string.IsNullOrEmpty(password) ? "loop of 0x00~0xFF" : "obfuscated";
                    Log.Information($"Initialization Vector is initialized. (IV : {ivString})");

                    using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        // Initialize streams.
                        _fileStream.Position = 0;
                        outputStream.Position = 0;

                        // Copy the file.
                        _fileStream.CopyTo(outputStream, 2048);
                        outputStream.SetLength(_fileStream.Length - 32); // 32 Bytes => File Signature.

                        /* -------------------------------------------------- */

                        byte[] block = new byte[_blockSize];

                        // Demodulate top blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Read(block, 0, _blockSize);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] ^= block[i];
                            }

                            outputStream.Position = _blockSize * blockNumber;
                            outputStream.Write(block, 0, _blockSize);
                            outputStream.Flush();
                        }

                        // Demodulate bottom blocks.
                        for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                        {
                            outputStream.Position = outputStream.Length - (_blockSize * (blockNumber + 1));
                            outputStream.Read(block, 0, _blockSize);

                            for (int i = 0; i < _blockSize; i++)
                            {
                                // XOR
                                block[i] ^= _initializationVector[i];
                                _initializationVector[i] ^= block[i];
                            }

                            outputStream.Position = outputStream.Length - (_blockSize * (blockNumber + 1));
                            outputStream.Write(block, 0, _blockSize);
                            outputStream.Flush();
                        }

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
