using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal interface LoxCallable
    {
        public int Arity();
        public object Call(Interpreter interpreter, List<object> args);
    }
}
