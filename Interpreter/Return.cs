using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Return : Exception
    {
        public object Value { get; set; }

        public Return(object value)
        {
            Value = value;
        }
    }
}
