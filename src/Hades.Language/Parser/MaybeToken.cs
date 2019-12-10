using Hades.Language.Lexer;

namespace Hades.Language.Parser
{
    public struct MaybeToken
    {
        public Token Value;
        public bool IsEof;
    }
}