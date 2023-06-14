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

        private enum FunctionType
        {
            None,
            Function
        }

        private Interpreter _interpreter;

        private List<Dictionary<string, bool>> _scopes = new();

        private FunctionType _currentFunction = FunctionType.None;

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

        public void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Reslove(stmt.expression);
        }

        public void VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResloveFunction(stmt, FunctionType.Function);
        }

        public void VisitIfStmt(Stmt.If stmt)
        {
            Reslove(stmt.condition);
            Reslove(stmt.thenBranch);
            if (stmt.elseBranch != null)
            {
                Reslove(stmt.elseBranch);
            }
        }

        public void VisitPrintStmt(Stmt.Print stmt)
        {
            Reslove(stmt.expression);
        }

        public void VisitReturnStmt(Stmt.Return stmt)
        {
            if(_currentFunction == FunctionType.None)
            {
                Lox.Error(stmt.keyword, "Can't return from top-level code.");
            }
            if(stmt.value != null)
            {
                Reslove(stmt.value);
            }
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

        public void VisitWhileStmt(Stmt.While stmt)
        {
            Reslove(stmt.condition);
            Reslove(stmt.body);
        }
        public void VisitAssignExpr(Expr.Assign expr)
        {
            Reslove(expr.value);
            ResolveLocal(expr, expr.name);  
        }

        public void VisitBinaryExpr(Expr.Binary expr)
        {
            Reslove(expr.left);
            Reslove(expr.right);
        }

        public void VisitCallExpr(Expr.Call expr)
        {
            Reslove(expr.callee);

            foreach(Expr arg in expr.arguments)
            {
                Reslove(arg);
            }
        }

        public void VisitGroupingExpr(Expr.Grouping expr)
        {
            Reslove(expr.expression);
        }

        public void VisitLiteralExpr(Expr.Literal expr)
        {
            return;
        }

        public void VisitLogicalExpr(Expr.Logical expr)
        {
            Reslove(expr.left);
            Reslove(expr.right);
        }

        public void VisitUnaryExpr(Expr.Unary expr)
        {
            Reslove(expr.right);
        }
        public void VisitVariableExpr(Expr.Variable expr)
        {
            if(_scopes.Count != 0)
            {
                if (_scopes[_scopes.Count - 1].TryGetValue(expr.name.lexeme, out bool isDefine))
                {
                    if (!isDefine)
                    {
                        Lox.Error(expr.name, "Can't read local variable in its own initializer.");
                    }
                }
            }
            ResolveLocal(expr, expr.name);
        }
        private void Reslove(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Reslove(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResloveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = _currentFunction;
            _currentFunction = type;
            BeginScope();
            foreach(Token param in function.parameters)
            {
                Declare(param);
                Define(param);
            }
            Reslove(function.body);
            EndScope();
            _currentFunction = enclosingFunction;
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
            if(scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Already variable with this name in this scope.");
            }
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
                    _interpreter.Resolve(expr, _scopes.Count - i - 1);
                    return;
                }
            }
        }
    }
}
