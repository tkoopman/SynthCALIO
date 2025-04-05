using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthCALIO.Matcher
{
    public interface IMatcher
    {
        public bool IsMatch (string filePath);
    }
}