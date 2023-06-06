using System;
using System.Collections.Generic;
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
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if(Check(type))
                {
                    Advance();
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
