using OpenJ2J.Extensions;
using OpenJ2J.J2J.Abstractions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.V2
{
    public class V2Validator : J2JValidator
    {
        #region ::Variables::

        /// <summary>
        /// The J2J Format Signature. (L3000009)
        /// </summary>
        private static string J2J_SIGNATURE = "4C33303030303039";

        #endregion

        #region ::Constructors::

        public V2Validator(FileStream fileStream) : base(fileStream)
        {
            _fileStream = fileStream;
        }

        #endregion

        #region ::Methods::

        public override bool Validate()
        {
            try
            {
                byte[] buffer = new byte[8];

                if (_fileStream != null)
                {
                    if (!_fileStream.CanRead)
                    {
                        throw new IOException("Unable to read the file.");
                    }

                    // Read the J2J signature section of the file.
                    long position = _fileStream.Length - 8;
                    _fileStream.Position = position;
                    _fileStream.Read(buffer, 0, 8);

                    Log.Information($"Original signature is '{J2J_SIGNATURE}', target's is '{buffer.BytesToHexString()}'.");

                    // Convert the J2J_SIGNATURE to a byte array.
                    byte[] signatureBytes = J2J_SIGNATURE.HexToBytes();

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
            throw new NotImplementedException();
        }

        public override bool ValidateWithChecksum(string password)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
