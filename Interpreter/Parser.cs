using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    //  ------------statement------------------
    //  program        → declaration* EOF;
    //  declaration    → varDecl | statement ;
    //  varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
    //  statement      → exprStmt | printStmt | block ;
    //  exprStmt       → expression ";" ;
    //  printStmt      → "print" expression ";" ;
    //  block          → "{" declaration* "}" ;
    //  ------------expression----------------
    //  expression     → assignment;
    //  assignment     → IDENTIFIER "=" assignment | equality ;
    //  equality       → comparison(( "!=" | "==" ) comparison )* ;
    //  comparison     → term(( ">" | ">=" | "<" | "<=" ) term )* ;
    //  term           → factor(( "-" | "+" ) factor )* ;
    //  factor         → unary(( "/" | "*" ) unary )* ;
    //  unary          → ( "!" | "-" ) unary | primary ;
    //  primary        → NUMBER | STRING | "true" | "false" | "nil"
    //                 | "(" expression ")" | IDENTIFIER ;
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

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new();
            while(!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }

        #region Expression and Statement Handler
        private Expr Expression()
        {
            return Assignment();
        }
        private Stmt Declaration()
        {
            try
            {
                if(Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }
                return Statement();
            }
            catch(ParserError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Statement()
        {
            if(Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if(Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block()!);
            }
            return ExpressionStatement();   
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }
        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;

            if(Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer!);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new();

            while(!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return statements;
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

        private Expr Assignment()
        {
            Expr expr = Equality();

            if(Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();  

                if(expr is Expr.Variable exprVariable)
                {
                    Token name = exprVariable.name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
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
            if (Match(TokenType.NIL)) return new Expr.Literal(null!);

            if(Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal!);
            }
            if(Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
            if(Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
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
        private Token Consume(TokenType type, string errorMessage)
        {
            if(Check(type))
            {
                return Advance();
            }
            throw Error(Peek(), errorMessage);
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
