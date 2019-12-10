using System.Collections.Generic;
using Hades.Language.Lexer;

namespace Hades.Language.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _index;

        private MaybeToken Current => Peek(0);
        private MaybeToken Next => Peek(1);
        
        private MaybeToken Peek(int by)
        {
            return _index + by != _tokens.Count
                ? new MaybeToken{Value = _tokens[_index + by], IsEof = false}
                : new MaybeToken{Value = new Token(), IsEof = true};
        }
        
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }
        
        private object Parse()
        {
            return null;
        }
        
        public static object Parse(List<Token> tokens)
        {
            return new Parser(tokens).Parse();
        }
    }
}