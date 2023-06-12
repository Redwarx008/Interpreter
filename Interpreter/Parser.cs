﻿using LoxGenerated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    //  ------------statement------------------
    //  program        → declaration* EOF;
    //  declaration    → funDecl | varDecl | statement ;
    //  funDecl        → "fun" function ;
    //  varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
    //  statement      → exprStmt | ifStmt | printStmt | whileStmt | forStmt | block ;
    //  forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
    //                  expression? ";"
    //                  expression? ")" statement ;
    //  whileStmt      → "while" "(" expression ")" statement ;
    //  ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
    //  exprStmt       → expression ";" ;
    //  printStmt      → "print" expression ";" ;
    //  block          → "{" declaration* "}" ;
    //  ------------expression----------------
    //  expression     → assignment;
    //  assignment     → IDENTIFIER "=" assignment | logic_or  ;
    //  logic_or       → logic_and( "or" logic_and )* ;
    //  logic_and      → equality( "and" equality )* ;
    //  equality       → comparison(( "!=" | "==" ) comparison )* ;
    //  comparison     → term(( ">" | ">=" | "<" | "<=" ) term )* ;
    //  term           → factor(( "-" | "+" ) factor )* ;
    //  factor         → unary(( "/" | "*" ) unary )* ;
    //  unary          → ( "!" | "-" ) unary | call  ;
    //  call           → primary( "(" arguments? ")" )* ;
    //  primary        → NUMBER | STRING | "true" | "false" | "nil"
    //                 | "(" expression ")" | IDENTIFIER ;
    //-------------utility rules------------------
    //  arguments      → expression( "," expression )* ;
    //  function       → IDENTIFIER "(" parameters? ")" block ;
    //  parameters     → IDENTIFIER( "," IDENTIFIER )* ;
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
                if(Match(TokenType.FUN))
                {
                    return Function("function");
                }
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
            if(Match(TokenType.IF))
            {
                return IfStatement();
            }
            if(Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if(Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if(Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if(Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block()!);
            }
            return ExpressionStatement();   
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if(Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
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

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
            Stmt initializer;
            if(Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if(Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if(!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if(!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            if(increment != null)
            {
                body = new Stmt.Block(new List<Stmt>() { body,
                    new Stmt.Expression(increment) });
            }
            if(condition == null)
            {
                condition = new Expr.Literal(true);
            }
            body = new Stmt.While(condition, body);
            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>() { initializer, body });
            }
            return body;
        }
        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            List<Token> parameters = new List<Token>();
            if(!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }
                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                }
                while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
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
            Expr expr = Or();

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
        
        private Expr Or()
        {
            Expr expr = And();

            while(Match(TokenType.OR))
            {
                Token loxOperator = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, loxOperator, right);
            }
            return expr;
        }
        private Expr And()
        {
            Expr expr = Equality();

            while(Match(TokenType.AND))
            {
                Token loxOperator = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, loxOperator, right);
            }
            return expr;
        }
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

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while(true)
            {
                if(Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }
        private Expr FinishCall(Expr callee)
        {
            List<Expr> args = new List<Expr>();
            if(!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(args.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    args.Add(Expression());
                }
                while(Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(callee, paren, args);
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
