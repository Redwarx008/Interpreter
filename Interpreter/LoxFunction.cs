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

        private Environment _closure;

        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
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
            Environment environment = new Environment(_closure);
            for(int i = 0; i < _declaration.parameters.Count; i++)
            {
                environment.Define(_declaration.parameters[i].lexeme, args[i]);
            }
            try
            {
                interpreter.ExecuteBlock(_declaration.body, environment);
            }
            catch(Return returnValue)
            {
                return returnValue.Value;
            }
            return null;
        }
    }
}
