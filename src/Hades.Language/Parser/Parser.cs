using System;
using System.Collections.Generic;
using System.Linq;
using Hades.Error;
using Hades.Language.Lexer;
using Hades.Syntax;
using Hades.Syntax.Nodes;
using Classifier = Hades.Language.Lexer.Classifier;

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
            _tokens = tokens;
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
            return Current == Classifier.EndOfFile;
        }

        private bool IsKeyword()
        {
            return Lexer.Lexer.Keywords.Contains(Current.Value);
        }
        
        private bool IsDecleration()
        {
            return IsKeyword() && (Current == Keyword.Var || Current == Keyword.Let);
        }

        private bool IsIdentifier()
        {
            return Current == Classifier.Identifier;
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

        //TODO: WIP
        private Node ParsePackageImport()
        {
            var node = new WithNode();
            Advance();
            
            if (!IsIdentifier())
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }
            
            node.Target = Current.Value;

            if (Next == Keyword.As)
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
            
            if (Current == Keyword.From)
            {
                Advance();
                if (!IsIdentifier())
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                }

                if (Next == Classifier.Colon)
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
            throw new NotImplementedException();
        }
        
        private Node ParseNext()
        {
            if (IsKeyword())
            {
                switch (Current.Value)
                {
                    case Keyword.With:
                        return ParsePackageImport();
                    
                    case Keyword.Var:
                    case Keyword.Let:
                        return ParseVariableDeclaration();
                    
                    default:
                        Error(string.Format(ErrorStrings.MESSAGE_UNKNOWN_KEYWORD,Current.Value)); // this really never happens, I just...whatever
                        break;
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

        #endregion
    }
}