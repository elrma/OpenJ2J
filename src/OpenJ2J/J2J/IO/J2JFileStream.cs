using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J.IO
{
    public static class J2JFileStream
    {
        public static FileStream Open(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file does not exist. (PATH : {filePath})");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        }

        public static FileStream OpenWithValidation(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file does not exist. (PATH : {filePath})");
            }

            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

            using (J2JValidator validator = new V3Validator(stream))
            {
                if (!validator.Validate())
                {
                    throw new FileNotFoundException($"The file does not a J2J format file. (PATH : {filePath})");
                }
            }

            return stream;
        }
    }
}
