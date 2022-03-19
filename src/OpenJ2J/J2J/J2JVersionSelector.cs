using OpenJ2J.J2J.Abstractions;
using OpenJ2J.J2J.V1;
using OpenJ2J.J2J.V2;
using OpenJ2J.J2J.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.J2J
{
    public static class J2JVersionSelector
    {
        public static J2JVersion SelectVersion(FileStream fileStream)
        {
            J2JVersion method = J2JVersion.Unknown;

            J2JValidator validator;

            validator = new V1Validator(fileStream);
            if (validator.Validate())
            {
                method = J2JVersion.Method1;
                return method;
            }

            validator = new V2Validator(fileStream);
            if (validator.Validate())
            {
                method = J2JVersion.Method2;
                return method;
            }

            validator = new V3Validator(fileStream);
            if (validator.Validate())
            {
                method = J2JVersion.Method3;
                return method;
            }

            return method;
        }
    }
}
