using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Resolver : Expr.Visitor, Stmt.Visitor
    {
        private Interpreter _interpreter;

        private Stack<Dictionary<string, bool>> _scopes = new();

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

        public void VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Reslove(stmt.statements);
            EndScope();
        }

        public void VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if(stmt.initializer != null)
            {
                Reslove(stmt.initializer);
            }
            Define(stmt.name);  

        }
        private void Reslove(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Reslove(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void Declare(Token name)
        {
            if(_scopes.Count == 0)
            {
                return;
            }
            Dictionary<string, bool> scope = _scopes.Peek();
            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if(_scopes.Count == 0)
            {
                return;
            }
            _scopes.Peek()[name.lexeme] = true;
        }
    }
}
