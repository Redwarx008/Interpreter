using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Environment
    {
        public Dictionary<string, object> Values { get; private set; } = new();

        public Environment Enclosing { get; set; } = null;

        public Environment() { }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }
        public void Define(string name, object value)
        {
            Values[name] = value;  
        }

        public Environment Ancestor(int distance)
        {
            Environment env = this;
            for(int i = 0; i < distance; i++)
            {
                env = env.Enclosing;
            }
            return env;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).Values[name];
        }

        public object Get(Token name)
        {
            if (Values.TryGetValue(name.lexeme, out object value))
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
            if(Values.ContainsKey(name.lexeme))
            {
                Values[name.lexeme] = value;
                return;
            }

            if(Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable {name.lexeme}.");
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).Values[name.lexeme] = value; 
        }
    }
}
