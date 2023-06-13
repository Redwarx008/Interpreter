using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Resolver : Expr.Visitor<object>, Stmt.Visitor
    {
        private Interpreter _interpreter;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }
        
        public void Reslove(List<Stmt> stmts)
        {
            foreach (Stmt stmt in stmts)
            {
                Reslove(stmt);
            }
        }
        private void Reslove(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Reslove(Expr expr)
        {
            expr.Accept(this);
        }
    }
}
