using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Hades.Language.Lexer;
using Hades.Language.Parser.Ast;
using Hades.Language.Parser.Nodes;
using static Hades.Language.Parser.MatchPair<Hades.Language.Parser.Ast.AstNode>;

namespace Hades.Language.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _index = 0;

        private MaybeToken Current => Peek(0);
        private MaybeToken Next => Peek(1);
        
        private MaybeToken Peek(int by)
        {
            return _index + by != _tokens.Count
                ? new MaybeToken{Value = _tokens[_index + by], IsEof = false}
                : new MaybeToken{Value = new Token(), IsEof = true};
        }

        private Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private MatchResult<T> Match<T>(T obj, params MatchPair<T>[] matchPairs) where T : AstNode, new()
        {
            var backupIndex = _index;
            var pairs = matchPairs.ToList();

            (MatchResult<T> result, bool shouldReturn) doMatch(MatchPair<T> pair, MatchResult<AstNode> value)
            {
                if (value.Matches)
                {
                    _index++;
                    pair.Action(value.Value);
                    pair.OnSuccess();
                    return (MatchResult<T>.Of(true, obj), true);
                }

                _index = backupIndex;

                return (null, false);
            }

            foreach (var matchPair in pairs)
            {
                switch (matchPair.Type)
                {
                    case Type.AstDatatype:
                        var dtMatchVal = doMatch(matchPair, Datatype());
                        if (dtMatchVal.shouldReturn)
                        {
                            return dtMatchVal.result;
                        }
                        break;

                    case Type.PARSER_DONE:
                        return MatchResult<T>.Of(true, obj);
                    
                    default:
                        if (!Current.IsEof && Current.Value.Type == matchPair.Type)
                        {
                            switch (Current.Value.Type)
                            {
                                case Type.Identifier:
                                    var identifierNode = new IdentifierNode(Current.Value);
                                    _index++;
                                    matchPair.Action(identifierNode);
                                    break; 
                                default:
                                    _index++;
                                    matchPair.Action(null);
                                    break;
                            }
                            matchPair.OnSuccess();
                            return MatchResult<T>.Of(true, obj);
                        }
                        break;
                }
            }
            
            return MatchResult<T>.Of(false, null);
        }

        private static MatchResult<T> Parse<T>(Func<MatchResult<T>> initialMatch) where T : AstNode
        {
            return initialMatch();
        }

        /// <summary>
        /// NUMBER
        /// | Statement
        /// </summary>
        /// <returns></returns>
        private AstNode ParseArraySize()
        {
            return null;
        }

        /// <summary>
        /// ArraySize COMMA ArraySize
        /// | ArraySize
        /// </summary>
        /// <returns></returns>
        private AstNode ParseArraySizes()
        {
            return null;
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
        private MatchResult<VariableDeclarationNode> VariableDeclaration()
        {
            var node = new VariableDeclarationNode();
            MatchPair<VariableDeclarationNode> ParseIdentifier()
            {
                return Matches(Type.Identifier, identifier =>
                {
                    node.Name = (IdentifierNode) identifier;
                    return Match(node,

                        #region ----------------Array----------------

                        //----------------Array Open----------------//
                        Matches(Type.OpenBracket, _ => Match(node,

                            //----------------Array Size----------------//
                            Matches(Type.AstArraySize, size => Match(node,

                                //----------------Array Closed----------------//
                                Matches(Type.ClosedBracket, _ => Match(node,

                                    //----------------Done----------------//
                                    Matches<VariableDeclarationNode>(Type.PARSER_DONE, null, () =>
                                    {
                                        node.IsArray = true;
                                        node.ArraySize = size;
                                    })
                                ))
                            ))
                        )),

                        #endregion

                        //----------------Done----------------//
                        Matches<VariableDeclarationNode>(Type.PARSER_DONE));
                });
            }

            return Match(node,

                #region ----------------VAR----------------

                Matches(Type.Var, _ => Match(node,

                    #region Mutable variable with datatype
                    
                    Matches(Type.AstDatatype, datatype => Match(node,
                        ParseIdentifier(),
                        Matches(Type.Nullable, _ => Match(node, ParseIdentifier()), () =>
                        {
                            node.Nullable = true;
                        }))),
                    
                    #endregion

                    #region Mutable variable without datatype

                    ParseIdentifier()),
                    () => { 
                        node.IsConstant = false;
                    }),
                
                    #endregion

                #endregion

                #region ----------------LET----------------
                
                Matches<VariableDeclarationNode>(Type.Let, _ => Match<VariableDeclarationNode>(node,
                    //TODO: Datatype
                    ParseIdentifier()
                ),
                () =>
                {
                    node.IsConstant = true;
                }));
            
                #endregion
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
        /// | MULTIPLICATION
        /// </summary>
        /// <returns></returns>
        private MatchResult<AstNode> /*TODO: Refactor to IdentifierNode*/ Datatype()
        {
            return MatchResult<AstNode>.Of(false, null);
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
                    Parse(VariableDeclaration)
                        .Or<AstNode>(() => throw new Exception($"Unexpected token {Current.Value}"))
                        .Value
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