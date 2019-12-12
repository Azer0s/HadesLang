using System;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser
{
    public class MatchResult<T> where T : AstNode
    {
        public bool Matches;
        public T Value;

        private MatchResult(){}
        
        public static MatchResult<T> Of(bool matches, T value)
        {
            return new MatchResult<T>
            {
                Matches = matches,
                Value = value
            };
        }

        public MatchResult<AstNode> Or<TK>(Func<MatchResult<TK>> orAction) where TK : AstNode
        {
            if (Matches)
            {
                return new MatchResult<AstNode>
                {
                    Value = Value,
                    Matches = true
                };
            }

            var matchResult = orAction();
            return new MatchResult<AstNode>
            {
                Value = matchResult.Value,
                Matches = matchResult.Matches
            };
        }
    }
}