using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Environment
    {
        private Dictionary<string, object> _values = new();

        public Environment Enclosing { get; set; } = null;

        public Environment() { }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }
        public void Define(string name, object value)
        {
            _values[name] = value;  
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.lexeme, out object value))
            {
                return value;
            }

            if(Enclosing != null)
            {
                return Enclosing.Get(name); 
            }

            throw new RuntimeError(name, $"Undefined variable {name.lexeme}.");
        }

        public void Assign(Token name , object value)
        {
            if(_values.ContainsKey(name.lexeme))
            {
                _values[name.lexeme] = value;
                return;
            }

            if(Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable {name.lexeme}.");
        }
    }
}
