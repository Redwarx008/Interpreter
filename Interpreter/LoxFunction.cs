using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class LoxFunction : LoxCallable
    {
        private Stmt.Function _declaration;

        public LoxFunction(Stmt.Function declaration)
        {
            _declaration = declaration;
        }

        public int Arity()
        {
            return _declaration.parameters.Count;   
        }

        public override string ToString()
        {
            return $"<fn {_declaration.name.lexeme}>";
        }
        public object Call(Interpreter interpreter, List<object> args)
        {
            Environment environment = new Environment(interpreter.Global);
            for(int i = 0; i < _declaration.parameters.Count; i++)
            {
                environment.Define(_declaration.parameters[i].lexeme, args[i]);
            }

            interpreter.ExecuteBlock(_declaration.body, environment);
            return null;
        }
    }
}
