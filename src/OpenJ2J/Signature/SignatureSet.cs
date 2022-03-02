using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenJ2J.Signature
{
    public class SignatureSet
    {
        public string Name { get; set; } = string.Empty;

        public List<string> Extensions { get; set; } = new List<string>();

        public List<string> Signatures { get; set; } = new List<string>();

        public int Offset { get; set; } = 0;
    }
}
