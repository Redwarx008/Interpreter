using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoxGenerated;

namespace Interpreter
{
    internal class Interpreter : Expr.Visitor<object>
    {
        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch(expr.loxOperator.type)
            {
                case TokenType.GREATER:
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    return (double)left <= (double)right;
                case TokenType.SLASH:
                    return (double)left / (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.STAR:
                    return (double)left * (double)right;
                case TokenType.MINUS:
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
                    break;
            }
            // Unreachable.
            return null;
        }

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch(expr.loxOperator.type)
            {
                case TokenType.MINUS:
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }
            // Unreachable.
            return null!;
        }
        private bool IsTruthy(object o)
        {
            if(o == null) return false;
            if(o is bool boolean)
            {
                return boolean;
            }
            return true;
        }
        private bool IsEqual(object a, object b)
        {
            return Object.Equals(a, b);
        }
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);   
        }
    }
}
