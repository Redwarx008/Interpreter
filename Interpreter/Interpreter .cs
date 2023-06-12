using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LoxGenerated;

namespace Interpreter
{
    internal class Interpreter : Expr.Visitor<Object>, Stmt.Visitor
    {
        public Environment Global { get; private set; }

        private Environment _current;

        public Interpreter()
        {
            Global = new Environment();
            _current = Global;  
        }
        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch(RuntimeError error)
            {
                Lox.RunTimeError(error);    
            }
        }
        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch(expr.loxOperator.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.loxOperator, left , right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left <= (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left / (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.STAR:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left * (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(expr.loxOperator, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if(left is double leftNum && right is double rightNum)
                    {
                        return leftNum + rightNum;
                    }
                    if(left is string leftStr && right is string rightStr)
                    {
                        return leftStr + rightStr;
                    }
                    throw new RuntimeError(expr.loxOperator, "Operands must be two numbers or two strings.");
            }
            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> args = new List<object>();
            foreach(Expr arg in expr.arguments)
            {
                args.Add(Evaluate(arg));    
            }

            if(callee is not LoxCallable)
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }
            LoxCallable function = (LoxCallable)callee;

            if(args.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got" +
                    $"{args.Count}.");
            }
            return function.Call(this, args);
        }
        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if(expr.loxOperator.type == TokenType.OR)
            {
                if(IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if(!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expr.right);
        }
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch(expr.loxOperator.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.loxOperator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }
            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return _current.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            _current.Assign(expr.name, value);
            return value;
        }


        private void CheckNumberOperand(Token loxOperator, object operand)
        {
            if(operand is double)
            {
                return;
            }
            throw new RuntimeError(loxOperator, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token loxOperator, object leftOperand, object rightOperand)
        {
            if(leftOperand is double && rightOperand is double)
            {
                return;
            }
            throw new RuntimeError(loxOperator, "Operands must be a numbers.");
        }
        private bool IsTruthy(object obj)
        {
            if(obj == null) return false;
            if(obj is bool boolean)
            {
                return boolean;
            }
            return true;
        }
        private bool IsEqual(object objA, object objB)
        {
            return Object.Equals(objA, objB);
        }
        private string Stringify(object obj)
        {
            if(obj == null)
            {
                return "nil";
            }
            if(obj is double objDouble)
            {
                string text = objDouble.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;    

            }
            return obj.ToString()!;
        }
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);   
        }

        private void Execute(Stmt stmt)
        {
            if(stmt == null)
            {
                return;
            }
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = _current;

            try
            {
                _current = environment;
                foreach(Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _current = previous;
            }
        }

        public void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
        }

        public void VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new(stmt);
            _current.Define(stmt.name.lexeme, function);
        }
        public void VisitIfStmt(Stmt.If stmt)
        {
            if(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if(stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
        }
        public void VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.Write(Stringify(value));
        }

        public void VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if(stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            _current.Define(stmt.name.lexeme, value);
        }

        public void VisitWhileStmt(Stmt.While stmt)
        {
            while(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
        }

        public void VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(_current));
        }
    }
}
