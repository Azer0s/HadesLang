using System;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser
{
    public class MatchPair<T> where T : AstNode
    {
        public Type Type;
        public Func<AstNode, MatchResult<T>> Action;
        public Action OnSuccess;
        
        private MatchPair(){}

        public static MatchPair<T> Matches<T>(Type type, Func<AstNode, MatchResult<T>> action = null, Action onSuccess = null) where T : AstNode
        {
            return new MatchPair<T>
            {
                Type = type,
                Action = action ?? (node => MatchResult<T>.Of(false, null)),
                OnSuccess = onSuccess ?? (() => { })
            };
        }
    }
}