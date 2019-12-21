using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hades.Language.Lexer;
using Hades.Language.Parser.Ast;
using Hades.Language.Parser.Nodes;
using static Hades.Language.Parser.MatchPair;
// ReSharper disable ConvertToLambdaExpression

namespace Hades.Language.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _index = 0;

        private MaybeToken Current => Peek(0);
        private MaybeToken Next => Peek(1);

        private readonly Func<(bool matches, bool isDone)> End;
        private readonly Func<(bool matches, bool isDone)> EndSubmodule;
        
        private MaybeToken Peek(int by)
        {
            return _index + by != _tokens.Count
                ? new MaybeToken{Value = _tokens[_index + by], IsEof = false}
                : new MaybeToken{Value = new Token(), IsEof = true};
        }

        private Parser(List<Token> tokens)
        {
            _tokens = tokens;
            End = () => Match(Matches(Type.PARSER_DONE));
            EndSubmodule = () => Match(Matches(Type.PARSER_DONE_SUBNODE));
        }

        private (bool matches, bool isDone) Match(params MatchPair[] matchPairs)
        {
            foreach (var matchPair in matchPairs)
            {
                if (matchPair.Type == Type.PARSER_DONE)
                {
                    return (true, true);
                }

                if (matchPair.Type == Type.PARSER_DONE_SUBNODE)
                {
                    _index--;
                    return (true, true);
                }

                if (Current.Value.Type == matchPair.Type)
                {
                    var node = matchPair.Type switch
                    {
                        Type.Identifier => (AstNode) new IdentifierNode(Current.Value),
                        _ => new GenericNode(Current.Value)
                    };

                    _index++;
                    var (matches, isDone) = matchPair.Action();

                    if (matches && isDone)
                    {
                        matchPair.OnSuccess(node);
                        return (true, true);
                    }
                    _index--;
                }

                //Parse Ast* Tokens
                var complexNode = matchPair.Type switch
                {
                    Type.AstArraySize => ParseArraySize(),
                    Type.AstDatatype => ParseDatatype(),
                    _ => (matches: false, isDone: false, node: new UnmatchedNode())
                };

                if (complexNode.matches && complexNode.isDone)
                {
                    _index++;
                    var (matches, isDone) = matchPair.Action();

                    if (matches && isDone)
                    {
                        matchPair.OnSuccess(complexNode.node);
                        return (true, true);
                    }
                    _index--;
                }
            }

            return (false, true);
        }

        /// <summary>
        /// ArraySize COMMA ArraySize
        /// | NUMBER
        /// | Statement
        /// </summary>
        /// <returns></returns>
        private (bool matches, bool isDone, AstNode node) ParseArraySize()
        {
            var node = new ArraySizeNode();
            
            void AddToken(AstNode arraySizeNode)
            {
                if (arraySizeNode is ArraySizeNode a)
                {
                    node.Tokens.AddRange(a.Tokens);
                }
                else
                {
                    node.Tokens.Add(arraySizeNode);
                }
            }

            (bool matches, bool isDone) MultipleSizes()
            {
                return Match(
                    Matches(Type.Comma, () =>
                    {
                        return Match(
                            Matches(Type.AstArraySize, EndSubmodule, AddToken));
                    }, _ => node.IsMultiDimensional = true),
                    Matches(Type.PARSER_DONE_SUBNODE));
            }
            
            var (matches, isDone) = Match(
                Matches(Type.Integer, MultipleSizes, AddToken),
                Matches(Type.Multiplication, EndSubmodule, AddToken),
                Matches(Type.AstStatement, MultipleSizes, AddToken));

            return (matches, isDone, node);
        }

        #region VariableDeclaration

        /// <summary>
        /// VAR IDENTIFIER
        /// | VAR Datatype IDENTIFIER
        /// | VAR Datatype NULLABLE IDENTIFIER
        /// | VAR IDENTIFIER OPENBRACKET ArraySizes CLOSEDBRACKET
        /// | VAR IDENTIFIER Datatype OPENBRACKET ArraySizes CLOSEDBRACKET
        /// | VAR IDENTIFIER Assignment
        /// | VAR Datatype IDENTIFIER Assignment
        /// | VAR Datatype NULLABLE IDENTIFIER Assignment
        /// | VAR IDENTIFIER OPENBRACKET ArraySizes CLOSEDBRACKET Assignment
        /// | VAR IDENTIFIER Datatype OPENBRACKET ArraySizes CLOSEDBRACKET Assignment
        /// | VAR IDENTIFIER Datatype NULLABLE OPENBRACKET ArraySizes CLOSEDBRACKET Assignment
        /// 
        /// | LET IDENTIFIER Assignment
        /// | LET Datatype IDENTIFIER Assignment
        /// | LET IDENTIFIER OPENBRACKET ArraySizes CLOSEDBRACKET Assignment
        /// | LET IDENTIFIER Datatype OPENBRACKET ArraySizes CLOSEDBRACKET Assignment
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
        private AstNode VariableDeclaration()
        {
            var node = new VariableDeclarationNode();

            (bool matches, bool isDone) EndOrAssign() => Match(ParseAssign(), Matches(Type.PARSER_DONE));
            (bool matches, bool isDone) ArrayDeclarationOrIdentifier() =>
                Match(
                    ParseArrayDeclaration(),
                    Matches(Type.Identifier, EndOrAssign, SetName));
            void SetName(AstNode identifier) => node.Name = (IdentifierNode) identifier;

            MatchPair ParseArrayDeclaration()
            {
                return Matches(Type.OpenBracket, () =>
                {
                    return Match(
                        Matches(Type.AstArraySize, () =>
                        {
                            return Match(
                                Matches(Type.ClosedBracket, () =>
                                {
                                    return Match(
                                        Matches(Type.Identifier, EndOrAssign, SetName));
                                }));
                        }, arraySize => { node.IsArray = true; node.ArraySize = (ArraySizeNode) arraySize; }));
                });
            }

            MatchPair ParseDeconstructAssign()
            {
                //TODO
                return null;
            }

            MatchPair ParseAssign()
            {
                //TODO
                return Matches(Type.PARSER_DONE);
            }
            
            var result = Match(
                Matches(Type.Var, () =>
                {
                    return Match(
                        Matches(Type.AstDatatype, () =>
                        {
                            return Match(
                                Matches(Type.Nullable, ArrayDeclarationOrIdentifier, _ => node.IsNullable = true),
                                ParseArrayDeclaration(),
                                Matches(Type.Identifier, EndOrAssign, SetName));
                        }, dataType => node.Datatype = (GenericNode) dataType),
                        Matches(Type.Multiplication, ArrayDeclarationOrIdentifier),
                        Matches(Type.Identifier, EndOrAssign, SetName),
                        ParseArrayDeclaration());
                }),
                Matches(Type.Let, () =>
                {
                    return Match(
                        Matches(Type.AstDatatype, ArrayDeclarationOrIdentifier, dataType => node.Datatype = (GenericNode) dataType),
                        Matches(Type.Identifier, EndOrAssign, SetName));
                }, _ => node.IsConstant = true));

            if (result.matches && result.isDone)
            {
                return node;
            }

            return new UnmatchedNode();
        }
        
        #endregion
        
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
        private AstNode ParseLiteral()
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
        private (bool matches, bool isDone, AstNode node) ParseDatatype()
        {
            if (!Current.IsEof && Datatypes.All.Contains(Current.Value.Value))
            {
                return (true, true, new GenericNode(new Token {Type = Type.AstDatatype, Value = Current.Value.Value}));
            }
            return (false, true, null);
        }

        /// <summary>
        /// OPENBRACE Parameters FATARROW Statements CLOSEDBRACE
        /// </summary>
        /// <returns></returns>
        private AstNode ParseLambda()
        {
            return null;
        }

        /// <summary>
        /// Lambda OPENPARENTHESES Arguments CLOSEDPARENTHESES
        /// | Lambda OPENPARENTHESES CLOSEDPARENTHESES
        /// </summary>
        /// <returns></returns>
        private AstNode ParseLambdaCall()
        {
            return null;
        }

        /// <summary>
        /// Arguments COMMA Argument
        /// | Argument
        /// </summary>
        /// <returns></returns>
        private AstNode ParseArguments()
        {
            return null;
        }

        /// <summary>
        /// IDENTIFIER
        /// | ARGS Datatype  
        /// </summary>
        /// <returns></returns>
        private AstNode ParseArgument()
        {
            return null;
        }

        /// <summary>
        /// Parameter
        /// | Parameter COMMA Parameters
        /// </summary>
        /// <returns></returns>
        private AstNode ParseParameters()
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
        private AstNode ParseParameter()
        {
            return null;
        }

        private AstNode ParseParameterMatch()
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
        private AstRoot Parse()
        {
            var ast = new AstRoot();
            while (!Current.IsEof)
            {
                ast.Add(
                    AstNode.Parse(VariableDeclaration)
                        .Or(() => throw new Exception($"Unexpected token {Current.Value}"))
                    );
            }
            return ast;
        }
        
        public static AstRoot Parse(List<Token> tokens)
        {
            return new Parser(tokens).Parse();
        }
    }
}