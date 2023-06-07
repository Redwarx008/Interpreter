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
        private class ParserError : Exception
        {

        }
        private List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public Expr? Parse()
        {
            try
            {
                return Expression();
            }
            catch(ParserError error)
            {
                return null;
            }
        }

        #region Expression Handler
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
                Token loxOperator = Previous();
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
                Token loxOperator = Previous();
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
                Token loxOperator = Previous();
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
                Token loxOperator = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, loxOperator, right);
            }
            return expr;
        }
        private Expr Unary()
        {
            if(Match(TokenType.BANG, TokenType.MINUS))
            {
                Token loxOperator = Previous();
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
                return new Expr.Literal(Previous().literal);
            }
            if(Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }
        #endregion

        #region Primitive Operations
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if(Check(type))
                {
                    _ = Advance();
                    return true;
                }
            }
            return false;
        }
        private Token Consume(TokenType type, string message)
        {
            if(Check(type))
            {
                return Advance();
            }
            throw Error(Peek(), message);
        }
        private ParserError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParserError();
        }
        private void Synchronize()
        {
            _ = Advance();

            while(!IsAtEnd())
            {
                if(Previous().type == TokenType.SEMICOLON) return;

                switch(Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                    default:
                        break;
                }

                _ = Advance();
            }
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
        #endregion
    }
}
