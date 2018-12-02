using System;
using System.Collections.Generic;
using System.Linq;
using Hades.Common;
using Hades.Error;
using Hades.Language.Lexer;
using Hades.Syntax;
using Hades.Syntax.Expression;
using Hades.Syntax.Expression.LiteralNodes;
using Hades.Syntax.Expression.Nodes;
using Hades.Syntax.Lexeme;
using Classifier = Hades.Syntax.Lexeme.Classifier;

namespace Hades.Language.Parser
{
    public class Parser
    {
        private int _index;
        private readonly IEnumerable<Token> _tokens;
        private Token Current => _tokens.ElementAtOrDefault(_index) ?? _tokens.Last();
        private Token Last => Peek(-1);
        private Token Next => Peek(1);

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList().Where(a => a.Kind != Classifier.WhiteSpace);
            _index = 0;
        }

        #region Helper

        private Token Peek(int ahead)
        {
            return _tokens.ElementAtOrDefault(_index + ahead) ?? _tokens.Last();
        }

        private void Advance(int i = 0)
        {
            _index += i != 0 ? i : 1;
        }

        private void Error(string error)
        {
            throw new Exception($"{error} {Current.Span.Start.Line}:{Current.Span.Start.Index}");
        }

        private void Error(string error, params object[] format)
        {
            Error(string.Format(error,format));
        }
        
        #endregion

        #region Checks

        private bool IsEof()
        {
            return Is(Classifier.EndOfFile);
        }

        private bool IsKeyword()
        {
            return Lexer.Lexer.Keywords.Contains(Current.Value);
        }

        private bool Type(Token token)
        {
            return Enum.GetValues(typeof(Datatype)).Cast<Datatype>().Select(a => a.ToString().ToLower()).Contains(token.Value);
        }

        private bool IsType()
        {
            return Type(Current);
        }

        private bool ExpectType()
        {
            return Type(Next);
        }

        private bool IsIdentifier()
        {
            return Is(Classifier.Identifier);
        }

        private bool Was(Classifier classifier)
        {
            return Last == classifier;
        }
        
        private bool Is(string token)
        {
            return Current == token;
        }

        private bool Is(Classifier classifier)
        {
            return Current == classifier;
        }

        private bool Is(Category category)
        {
            return Current.Category == category;
        }

        private bool Expect(string token)
        {
            return Next == token;
        }

        private bool Expect(Classifier classifier)
        {
            return Next == classifier;
        }
        
        private bool Expect(Category category)
        {
            return Next.Category == category;
        }

        #endregion

        #region Parsing

        #region Blocks
        
        private Node ParseFunc()
        {
            Advance();
            var node = new FunctionNode();
            if (Is(Classifier.Not))
            {
                node.Override = true;
                Advance();
            }

            if (!Is(Category.Identifier))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }
            
            if (Expect(Category.Operator))
            {
                if (!node.Override)
                {
                    Error(ErrorStrings.MESSAGE_OVERRIDE_WITHOUT_DECLARATION);
                }
                Advance();
                node.Name = Current.Value;
            }
            else
            {
                if (!Is(Category.Identifier))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                }

                node.Name = Current.Value;
                Advance();
            }

            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            if (!Expect(Classifier.RightParenthesis))
            {            
                do
                {
                    Advance();
                    if (IsType())
                    {
                        node.Parameters.Add(new IdentifierNode(Next.Value),(Datatype)Enum.Parse(typeof(Datatype), Current.Value.ToUpper()));
                        Advance(2);
                    }
                    else
                    {
                        node.Parameters.Add(new IdentifierNode(Current.Value),null);
                        Advance();
                    }
                } while (Is(Classifier.Comma));
                
                if (!Is(Classifier.RightParenthesis))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
                }
                
                Advance();
            }

            if (Is(Keyword.Requires))
            {
                Advance();
                node.Guard = ParseStatement();
            }
            
            while (!Is(Keyword.End))
            {
                node.Children.Add(ParseNext());
            }
            
            Advance();
            node.Children = node.Children.Where(a => a != null).ToList();
            return node;
        }
        
        #endregion

        #region Statements
        
        private Node ParsePackageImport()
        {
            var node = new WithNode();
            Advance();
            
            if (!IsIdentifier())
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }
            
            node.Target = Current.Value;

            if (Expect(Keyword.As))
            {
                Advance(2);
                

                if (!IsIdentifier())
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                }
                
                node.Name = Current.Value;
            }
            
            Advance();
            
            if (Is(Keyword.From)) //with x FROM ...
            {
                Advance();
                if (!IsIdentifier())
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                }

                if (Expect(Classifier.Colon))
                {
                    node.Native = true;
                    node.NativePackage = Current.Value;
                    
                    Advance(2);
                    if (!IsIdentifier())
                    {
                        Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                    }

                    node.Source = Current.Value;
                }
                else
                {
                    node.Source = Current.Value;
                }
                
                Advance();
            }

            return node;
        }

        private Node ParseCall(Node baseNode)
        {
            if (!Is(Classifier.Identifier))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }

            var node = new CallNode {Source = baseNode, Target = new IdentifierNode(Current.Value)};
            Advance();

            return ParseCallSignature(node);
        }
        
        private Node ParseCallSignature(CallNode node)
        {
            if (Is(Classifier.LeftParenthesis))
            {
                Advance();
                if (Is(Classifier.RightParenthesis))
                {
                    Advance();
                }
                else
                {
                    do
                    {
                        var name = "";
                        if (Is(Classifier.Identifier) && Expect(Classifier.Assignment))
                        {
                            name = Current.Value;
                            Advance(2);
                        }
                        
                        node.Parameters.Add(ParseStatement(),name);

                        if (!Is(Classifier.RightParenthesis))
                        {
                            if (!Is(Classifier.Comma))
                            {
                                Error(ErrorStrings.MESSAGE_EXPECTED_COMMA);
                            }
                            Advance();
                        }
                        
                    } while (!Is(Classifier.RightParenthesis));
                    Advance();
                }
            }
            else if (Is(Classifier.Colon))
            {
                Advance();
                node.Parameters.Add(ParseStatement(),"");
            }
            else
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_PARAMETERS);
            }

            return node;
        }
        
        #endregion
        
        #region Variables
        
        private Node ParseVariableDeclaration()
        {
            var variable = new VariableDeclarationNode {Mutable = Is(Keyword.Var)};

            if (Expect(Classifier.Mul))
            {
                if (Is(Keyword.Let))
                {
                    Advance();
                    Error(ErrorStrings.MESSAGE_IMMUTABLE_CANT_BE_DYNAMIC);
                }
                
                Advance();

                if (ExpectType())
                {
                    Advance();
                    Error(ErrorStrings.MESSAGE_DYNAMIC_NOT_POSSIBLE_WITH_STATIC_TYPES);
                }
                
                variable.Dynamic = true;
            }
            
            Advance();

            if (IsType())
            {
                variable.Datatype = (Datatype)Enum.Parse(typeof(Datatype),Current.Value.ToUpper());
                Advance();
            }

            if (Is(Classifier.Question))
            {
                if (variable.Datatype == null && Peek(2) != Classifier.Assignment)
                {
                    Error(ErrorStrings.MESSAGE_TYPE_INFERRED_CANT_BE_NULLABLE);
                }

                if (!variable.Mutable)
                {
                    Advance(-2);
                    Error(ErrorStrings.MESSAGE_IMMUTABLE_CANT_BE_NULLABLE);
                }

                Advance();
                variable.Nullable = true;
            }
            
            if (Is(Classifier.LeftBrace))
            {
                variable.Array = true;
                Advance(); //[

                if (!Is(Classifier.RightBrace))
                {
                    if (Is(Classifier.Mul))
                     {
                         variable.InfiniteArray = true;
                         Advance();
                         if (!Is(Classifier.RightBrace))
                         {
                             Error(ErrorStrings.MESSAGE_EXPECTED_TOKEN, Current.Kind.ToString());
                         }
                     }
                     else
                    {
                        variable.ArraySize = ParseStatement();
                    }
                }
                
                Advance(); //]
            }

            if (IsIdentifier())
            {
                variable.Name = Current.Value;
                Advance();
            }
            else
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }

            return variable;
        }
        
        private Node ParseVariableDeclarationAndAssignment()
        {
            var variable = ParseVariableDeclaration() as VariableDeclarationNode;
            
            if (Is(Classifier.Assignment))
            {
                Advance();
                if (variable != null) variable.Assignment = ParseStatement(); //change to ParseStatement
            }

            return variable;
        }

        private Node ParseArrayOrLambda()
        {
            Advance();

            var parameters = new List<Node>();
            
            while ((Expect(Classifier.Comma) && !Is(Classifier.FatArrow)) || Expect(Classifier.FatArrow))
            {
                parameters.Add(ParseStatement());
                Advance();
            }

            //Lambda
            Node n;
            if (parameters.TrueForAll(a => a.Classifier == Syntax.Expression.Classifier.Identifier) && Was(Classifier.FatArrow))
            {
                var node = new LambdaNode();
                node.Parameters.AddRange(parameters);
                
                while (!Is(Classifier.RightBracket))
                {
                    node.Children.Add(ParseNext());
                }

                if (node.Children.Count == 1)
                {
                    node.Complex = false;
                }
                
                Advance();
                node.Children = node.Children.Where(a => a != null).ToList();
                n = node;
            }
            else
            {
                //TODO: Array
                var node = new IdentifierNode("");
                n = node;
            }

            return n;
        }
        
        #endregion
        
        #region Entry
        
        public RootNode Parse()
        {
            var node = new RootNode();
            while (!IsEof())
            {
                node.Children.Add(ParseNext());
            }

            node.Children = node.Children.Where(a => a != null).ToList();
            return node;
        }
        
        /// <summary>
        /// Parses blocks
        /// </summary>
        /// <returns></returns>
        private Node ParseNext()
        {
            while(Is(Classifier.NewLine) || Is(Category.Comment))
            {
                Advance();
            }

            if (Is(Classifier.EndOfFile))
            {
                return null;
            }
            
            if (IsKeyword())
            {
                switch (Current.Value)
                {
                    case Keyword.Put:
                        Advance();
                        return new PutNode {Statement = ParseStatement()};
                    
                    case Keyword.Func:
                        return ParseFunc();
                    
                    default:
                        return ParseStatement();
                }
            }

            if (IsIdentifier() || (Is(Category.Literal) && Expect(Classifier.Arrow)) || (Is(Category.Literal) && Expect(Category.Operator)) || Is(Classifier.LeftParenthesis) || Is(Classifier.LeftBracket))
            {
                return ParseStatement();
            }

            Error(ErrorStrings.MESSAGE_UNEXPECTED_TOKEN, Current.Value);

            return null;
        }

        /// <summary>
        /// For statements that can have more complex nodes within the statement
        /// </summary>
        /// <returns></returns>
        private Node ParseStatement()
        {
            if (Is(Classifier.LeftParenthesis))
            {
                Advance();
                var n = ParseStatement();
                
                if (!Is(Classifier.RightParenthesis))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
                }
                
                Advance();
                
                return n;
            }
            
            var node = ParseStatementWithoutOperation();

            Node getOperation(Node initial = null)
            {
                var ops = new OperationNode();
                if (initial != null)
                {
                    ops.Operations.Add(initial);
                }
                
                while (Is(Category.Operator))
                {
                    ops.Operations.Add(new OperationNodeNode(Current.Kind, Current.Value));
                    Advance();
                    ops.Operations.Add(ParseStatement());
                }
                return ops;
            }
            
            if (Is(Category.Operator))
            {
                node = getOperation(node);
            }

            if (Is(Classifier.NullCondition))
            {
                Advance();
                return new NullConditionNode{Condition = node, Operation = ParseStatement()};
            }

            //Call on lambdas
            if (node is LambdaNode)
            {
                if (Is(Classifier.Colon) || Is(Classifier.LeftParenthesis))
                {
                    return ParseCallSignature(new CallNode{Source = node, Target = new IdentifierNode("anonymous")});
                }
            }
            
            return node;
        }

        /// <summary>
        /// For less complex statements (statements that do not require a preceding node
        /// </summary>
        /// <returns>Node</returns>
        private Node ParseStatementWithoutOperation()
        {
            if (IsEof())
            {
                Error(ErrorStrings.MESSAGE_UNEXPECTED_EOF);
            }
            
            if (IsKeyword())
            {
                switch (Current.Value)
                {
                    case Keyword.Var:
                    case Keyword.Let:
                        return ParseVariableDeclarationAndAssignment();
                    
                    case Keyword.With:
                        return ParsePackageImport();
                    
                    case Keyword.Raise:
                        Advance();
                        return new RaiseNode {Exception = ParseStatement()};
                }
            }

            if (Is(Classifier.Identifier) && !IsKeyword())
            {
                //Call on object
                if (Expect(Classifier.Arrow))
                {
                    Advance(2);
                    return ParseCall(new IdentifierNode(Peek(-2).Value));
                }

                //Call on this
                if (Expect(Classifier.Colon) || Expect(Classifier.LeftParenthesis))
                {
                    return ParseCall(new IdentifierNode("this"));
                }
                
                Advance();
                return new IdentifierNode(Last.Value);
            }
            
            if (Is(Category.Literal))
            {
                Node node = null;
                switch (Current.Kind)
                {
                    case Classifier.IntLiteral:
                        node = new IntLiteralNode(Current);
                        break;
                    
                    case Classifier.BoolLiteral:
                        node = new BoolLiteralNode(Current);
                        break;
                    
                    case Classifier.StringLiteral:
                        node = new StringLiteralNode(Current);
                        break;
                    
                    case Classifier.DecLiteral:
                        node = new DecLiteralNode(Current);
                        break;
                }
                
                if (node == null)
                {
                    Error(ErrorStrings.MESSAGE_INVALID_LITERAL, Current.Value);
                }
                
                Advance();
                
                if (Is(Classifier.Arrow))
                {
                    Advance();
                    return ParseCall(node);
                }
                
                return node;
            }

            if (Is(Classifier.LeftBracket))
            {
                return ParseArrayOrLambda();
            }
            
            if (Is(Classifier.MultidimensionalArrayAccess))
            {
                Advance();
                return new MultidimensionalArrayAccessNode(Last.Value);
            }
            
            return null;
        }

        #endregion

        #endregion
    }
}