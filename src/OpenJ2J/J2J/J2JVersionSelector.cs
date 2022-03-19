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
        public static J2JMethod SelectVersion(FileStream fileStream)
        {
            J2JMethod method = J2JMethod.Unknown;

            using (J2JValidator validator = new V1Validator(fileStream))
            {
                if (validator.Validate())
                {
                    method = J2JMethod.Method1;
                    return method;
                }
            }

            using (J2JValidator validator = new V2Validator(fileStream))
            {
                if (validator.Validate())
                {
                    method = J2JMethod.Method2;
                    return method;
                }
            }

            using (J2JValidator validator = new V3Validator(fileStream))
            {
                if (validator.Validate())
                {
                    method = J2JMethod.Method3;
                    return method;
                }
            }

            return method;
        }
    }
}
