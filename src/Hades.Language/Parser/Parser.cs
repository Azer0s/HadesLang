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

        /// <summary>
        /// INT
        /// | FLOAT
        /// | BOOL
        /// | STRING
        /// | ATOM
        /// | Array
        /// | Lambda
        /// </summary>
        /// <returns></returns>
        private object ParseLiteral()
        {
            return null;
        }

        /// <summary>
        /// "int"
        /// | "dec"
        /// | "bool"
        /// | "string"
        /// | "object"
        /// | "proto"
        /// | "lambda"
        /// </summary>
        /// <returns></returns>
        private object ParseDatatype()
        {
            return null;
        }

        /// <summary>
        /// OPENBRACE Parameters FATARROW Statements CLOSEDBRACE
        /// </summary>
        /// <returns></returns>
        private object ParseLambda()
        {
            return null;
        }

        /// <summary>
        /// Lambda OPENPARENTHESES Arguments CLOSEDPARENTHESES
        /// | Lambda OPENPARENTHESES CLOSEDPARENTHESES
        /// </summary>
        /// <returns></returns>
        private object ParseLambdaCall()
        {
            return null;
        }

        /// <summary>
        /// Arguments COMMA Argument
        /// | Argument
        /// </summary>
        /// <returns></returns>
        private object ParseArguments()
        {
            return null;
        }

        /// <summary>
        /// IDENTIFIER
        /// | ARGS Datatype  
        /// </summary>
        /// <returns></returns>
        private object ParseArgument()
        {
            return null;
        }

        /// <summary>
        /// Parameter
        /// | Parameter COMMA Parameters
        /// </summary>
        /// <returns></returns>
        private object ParseParameters()
        {
            return null;
        }

        /// <summary>
        /// Datatype IDENTIFIER ASSIGN Literal
        /// | Datatype IDENTIFIER
        /// | ARGS Datatype IDENTIFIER
        /// | ARGS IDENTIFIER
        /// | IDENTIFIER
        /// | IDENTIFIER MATCHASSIGN ParameterMatch
        /// | ParameterMatch
        /// </summary>
        /// <returns></returns>
        private object ParseParameter()
        {
            return null;
        }

        private object ParseParameterMatch()
        {
            return null;
        }
        
        /// <summary>
        /// VariableDeclaration
        /// | VariableAssignment
        /// | LambdaCall
        /// | FunctionDeclaration
        /// | ClassDeclaration
        /// | StructDeclaration
        /// | Pipeline
        /// | IfStatement
        /// | ForLoop
        /// | WhileLoop
        /// | ReceiveStatement
        /// | MatchStatement
        /// | PutStatement
        /// </summary>
        /// <returns></returns>
        private object Parse()
        {
            while (!Current.IsEof)
            {
                switch (Current.Value.Type)
                {
                    case Type.Var:
                    case Type.Let:
                        break;
                    
                    case Type.Func:
                        break;
                }
            }
            return null;
        }
        
        public static object Parse(List<Token> tokens)
        {
            return new Parser(tokens).Parse();
        }
    }
}