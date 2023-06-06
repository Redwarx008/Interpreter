using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Parser
    {
        private List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }
        private Expr Expression()
        {
            return Equality();
        }
        // equality → comparison ( ( "!=" | "==" ) comparison )* ;
        private Expr Equality()
        {
            Expr expr = Comparison();

            while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token loxOperator = Advance();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, loxOperator, right);
            }
            return expr;
        }
        // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        private Expr Comparison()
        {
            Expr expr = Term();

            while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token loxOperator = Advance();
                Expr right = Term();
                expr = new Expr.Binary(expr, loxOperator, right);
            }
            return expr;
        }
        
        private Expr Term()
        {
            Expr expr = Factor();

            while(Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token loxOperator = Advance();
                Expr right = Factor();
                expr = new Expr.Binary(expr, loxOperator, right);
            }
            return expr;
        }
        private Expr Factor()
        {
            Expr expr = Unary();
            
            while(Match(TokenType.SLASH, TokenType.STAR))
            {
                Token loxOperator = Advance();
                Expr right = Unary();
                expr = new Expr.Binary(expr, loxOperator, right);
            }
            return expr;
        }
        private Expr Unary()
        {
            if(Match(TokenType.BANG, TokenType.MINUS))
            {
                Token loxOperator = Advance();
                Expr right = Unary();
                return new Expr.Unary(loxOperator, right);
            }

            return Primary();
        }
        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if(Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Advance().literal);
            }
            if(Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                return new Expr.Grouping(expr);
            }
        }
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if(Check(type))
                {
                    return true;
                }
            }
            return false;
        }
        private bool Check(TokenType type)
        {
            if(IsAtEnd())
                return false;
            return Peek().type == type;
        }
        /// <summary>
        /// consumes the current token and returns it
        /// </summary>
        /// <returns></returns>
        private Token Advance()
        {
            if(!IsAtEnd())
            {
                ++_current;
            }
            return Previous();
        }
        private Token Peek()
        {
            return _tokens[_current];
        }
        private Token Previous()
        {
            return _tokens[_current - 1];
        }
        private bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }
    }
}
