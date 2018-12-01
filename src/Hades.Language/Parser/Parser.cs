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

        private bool Was(string token)
        {
            return Last == token;
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

        #endregion

        #region Parsing

        #region Blocks
        
        private Node ParseFunc()
        {
            throw new NotImplementedException();
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
                Advance();
                Advance();

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
                    
                    Advance();
                    Advance();
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
                if (variable.Datatype == null)
                {
                    Error(ErrorStrings.MESSAGE_TYPE_INFERED_CANT_BE_NULLABLE);
                }

                if (!variable.Mutable)
                {
                    Advance(-2);
                    Error(ErrorStrings.MESSAGE_IMMUTABLE_CANT_BE_NULLABLE);
                }

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
        
        #endregion
        
        #region Entry
        
        public IEnumerable<Node> Parse()
        {
            while (!IsEof())
            {
                yield return ParseNext();
            }
        }
        
        private Node ParseNext()
        {
            if (IsKeyword())
            {
                switch (Current.Value)
                {
                    case Keyword.With:
                        return ParsePackageImport();
                    
                    default:
                        return ParseStatement();
                }
            }

            if (IsIdentifier())
            {
                
            }
            else
            {
                Error(ErrorStrings.MESSAGE_UNEXPECTED_TOKEN, Current.Value);
            }

            return null;
        }

        private Node ParseStatement()
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
                    
                    case Keyword.Func:
                        return ParseFunc();
                }
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
                return node;
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