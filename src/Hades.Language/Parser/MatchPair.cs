using System;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser
{
    public class MatchPair
    {
        public Type Type;
        public Func<(bool matches, bool isDone)> Action;
        public Action<AstNode> OnSuccess;
        
        private MatchPair(){}

        public static MatchPair Matches(Type type, Func<(bool matches, bool isDone)> action = null, Action<AstNode> onSuccess = null)
        {
            return new MatchPair
            {
                Type = type,
                Action = action ?? (() => (false, true)),
                OnSuccess = onSuccess ?? ((node) => { })
            };
        }
    }
}