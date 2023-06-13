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

        private List<Dictionary<string, bool>> _scopes = new();

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

        public void VisitAssignExpr(Expr.Assign expr)
        {
            Reslove(expr.value);
        }
        public void VisitVariableExpr(Expr.Variable expr)
        {
            if(_scopes.Count == 0 && _scopes[_scopes.Count - 1][expr.name.lexeme] == false)
            {
                Lox.Error(expr.name, "Can't read local variable in its own initializer.");
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

        private void BeginScope()
        {
            _scopes.Add(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.RemoveAt(_scopes.Count - 1);
        }

        private void Declare(Token name)
        {
            if(_scopes.Count == 0)
            {
                return;
            }
            Dictionary<string, bool> scope = _scopes[_scopes.Count - 1];
            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if(_scopes.Count == 0)
            {
                return;
            }
            _scopes[_scopes.Count - 1][name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for(int i = _scopes.Count - 1; i >= 0; i--)
            {
                if (_scopes[i].ContainsKey(name.lexeme))
                {
                    return;
                }
            }
        }
    }
}
