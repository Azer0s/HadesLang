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

        private bool IsType()
        {
            return Enum.GetValues(typeof(Datatype)).Cast<Datatype>().Select(a => a.ToString().ToLower()).Contains(Current.Value);
        }

        private bool IsIdentifier()
        {
            return Is(Classifier.Identifier);
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

        public IEnumerable<Node> Parse()
        {
            while (!IsEof())
            {
                yield return ParseNext();
            }
        }

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
        
        private Node ParseVariableDeclaration()
        {
            var variable = new VariableDeclarationNode {Mutable = Is(Keyword.Var)};            
            Advance();

            if (IsType())
            {
                variable.Datatype = (Datatype)Enum.Parse(typeof(Datatype),Current.Value.ToUpper());
                Advance();
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
                             Error(string.Format(ErrorStrings.MESSAGE_EXPECTED_TOKEN, Current.Kind.ToString()));
                         }
                     }
                     else
                     {
                         //TODO: Array size, handle multidimensional
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
                Error(string.Format(ErrorStrings.MESSAGE_UNEXPECTED_TOKEN, Current.Value));
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
                    Error(string.Format(ErrorStrings.MESSAGE_INVALID_LITERAL, Current.Value));
                }
                
                Advance();
                return node;
            }

            return null;
        }

        #endregion
    }
}