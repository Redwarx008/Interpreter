using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    internal class Scanner
    {
        private string _source;

        private List<Token> _tokens = new();

        private static Dictionary<string, TokenType> _keyWords;

        private int _start = 0;
        private int _current = 0;
        private int _line = 0;

        static Scanner()
        {
            _keyWords = new Dictionary<string, TokenType>();
            _keyWords["and"] = TokenType.AND;
            _keyWords["class"] = TokenType.CLASS;
            _keyWords["else"] = TokenType.ELSE;
            _keyWords["false"] = TokenType.FALSE;
            _keyWords["for"] = TokenType.FOR;
            _keyWords["fun"] = TokenType.FUN;
            _keyWords["if"] = TokenType.IF;
            _keyWords["nil"] = TokenType.NIL;
            _keyWords["or"] = TokenType.OR;
            _keyWords["print"] = TokenType.PRINT;
            _keyWords["return"] = TokenType.RETURN;
            _keyWords["super"] = TokenType.SUPER;
            _keyWords["this"] = TokenType.THIS;
            _keyWords["true"] = TokenType.TRUE;
            _keyWords["var"] = TokenType.VAR;
            _keyWords["while"] = TokenType.WHILE;
        }

        public Scanner(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while(!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                _start = _current;
                ScanToken();
            }
            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if(Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while(!IsAtEnd() && _source[_current] != '\n')
                        {
                            //Advance();
                            ++_current;
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    ++_line;
                    break;
                case '"':
                    StringHandler();
                    break;
                default:
                    if (IsDigit(c))
                    {
                        NumberHandler();
                    }
                    else if(IsAlpha(c))
                    {
                        IdentifierHandler();
                    }
                    else
                    {
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }
        private void IdentifierHandler()
        {
            while(IsAlphaNumeric(Peek()))
            {
                ++_current;
            }

            string text = _source.Substring(_start, _current - _start);
            TokenType type = TokenType.IDENTIFIER;
            if(!_keyWords.TryGetValue(text, out type))
            {
                type = TokenType.IDENTIFIER;
            }
            AddToken(type);
        }
        private void NumberHandler()
        {
            while(IsDigit(Peek()))
            {
                ++_current;
            }
            // Look for a fractional part.
            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                ++_current;
            }
            while(IsDigit(Peek()))
            {
                ++_current;
            }
            AddToken(TokenType.NUMBER, Double.Parse(_source.Substring(_start, _current - _start)));
        }
        private void StringHandler()
        {
            while(!IsAtEnd() && _source[_current] != '"')
            {
                if (_source[_current] == '\n')
                {
                    ++_line;
                }
                ++_current;
            }
            if(IsAtEnd())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }
            // The closing ".
            ++_current;
            // Trim the surrounding quotes.
            string value = _source.Substring(_start + 1, _current - 1 - (_start + 1));
            AddToken(TokenType.STRING, value);
        }
        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }
        private void AddToken(TokenType type, Object? literal)
        {
            String text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }
        private bool Match(char expected)
        {
            if(IsAtEnd())
            {
                return false;
            }
            if (_source[_current] != expected)
            {
                return false;
            }
            _current++;
            return true;
        }
        /// <summary>
        /// It’s sort of like advance(), but doesn’t consume the character.
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if(IsAtEnd())
            {
                return '\0';
            }
            return _source[_current];
        }
        private char PeekNext()
        {
            if(_current + 1 >= _source.Length)
            {
                return '\0';
            }
            return _source[_current + 1];   
        }
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_';
        }
        private bool IsAlphaNumeric(char c)
        {
            return IsDigit(c) || IsAlpha(c);
        }
    }
}
