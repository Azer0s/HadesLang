using System;
using System.Collections.Generic;
using System.Linq;
using Hades.Common;
using Hades.Error;
using Hades.Language.Lexer;
using Hades.Syntax.Expression;
using Hades.Syntax.Expression.Nodes;
using Hades.Syntax.Expression.Nodes.BlockNodes;
using Hades.Syntax.Expression.Nodes.LiteralNodes;
using Hades.Syntax.Lexeme;
using Classifier = Hades.Syntax.Lexeme.Classifier;

namespace Hades.Language.Parser
{
    public class Parser
    {
        private readonly IEnumerable<Token> _tokens;
        private int _index;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList().Where(a => a.Kind != Classifier.WhiteSpace && a.Category != Category.Comment);
            _index = 0;
        }

        private Token Current => _tokens.ElementAtOrDefault(_index) ?? _tokens.Last();
        private Token Last => Peek(-1);
        private Token Next => Peek(1);

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
            Error(string.Format(error, format));
        }

        private BlockNode ReadToEnd(BlockNode node, bool allowSkipStop = false)
        {
            while (!Is(Keyword.End))
            {
                node.Children.Add(ParseNext(allowSkipStop));
            }

            Advance();
            node.Children = node.Children.Where(a => a != null).ToList();
            return node;
        }

        private void ExpectIdentifier()
        {
            if (!Expect(Classifier.Identifier))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
            }
        }

        private void EnforceIdentifier()
        {
            Advance(-1);
            ExpectIdentifier();
            Advance();
        }

        /// <summary>
        /// Gets the specific type of an object or proto
        /// ```
        /// func doStuff(object::IClient a)
        ///    a->stuff("Hello world")
        /// end
        /// ```
        /// </summary>
        /// <returns>Specific type or null</returns>
        private (string specificType, Datatype dt) GetSpecificType()
        {
            var dt = (Datatype) Enum.Parse(typeof(Datatype), Current.Value.ToUpper());
            string type = null;
                    
            if (dt == Datatype.PROTO || dt == Datatype.OBJECT)
            {
                Advance();
                if (Is(Classifier.NullCondition))
                {
                    ExpectIdentifier();
                    Advance();
                    type = Current.Value;
                    Advance();
                }
                else
                {
                    Advance(-1);
                }
            }

            return (type, dt);
        }
        
        private List<(Node Key, Datatype? Value, string SpecificType)> ParseArguments(Classifier expectedClassifier, string expect)
        {
            var args = new List<(Node Key, Datatype? Value, string SpecificType)>();
            do
            {
                Advance();
                if (IsType())
                {
                    var (type, dt) = GetSpecificType();

                    if (type == null)
                    {
                        Advance();
                    }
                        
                    EnforceIdentifier();
                    args.Add((new IdentifierNode(Current.Value), dt, type));
                    Advance();
                }
                else if (Is(Keyword.Args))
                {
                    Advance();
                    if (IsType())
                    {
                        var (type, dt) = GetSpecificType();
                        
                        if (type == null)
                        {
                            Advance();
                        }
                        
                        EnforceIdentifier();
                        args.Add((new ArgsNode(Current.Value), dt, type));
                    }
                    else
                    {
                        EnforceIdentifier();
                        args.Add((new ArgsNode(Current.Value), Datatype.NONE, null));
                    }
                    Advance();
                }
                else if (IsIdentifier())
                {
                    args.Add((new IdentifierNode(Current.Value), null, null));
                    Advance();
                }
                else
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_IDENTIFIER);
                }
            } while (Is(Classifier.Comma));

            if (args.Any(a => a.Key is ArgsNode))
            {
                if (args.Count(a => a.Key is ArgsNode) > 1)
                {
                    Error(ErrorStrings.MESSAGE_CANT_HAVE_MULTIPLE_VARARGS);
                }
            }

            if (!Is(expectedClassifier))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_VALUE,expect);
            }

            Advance();

            return args;
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

            EnforceIdentifier();

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
                EnforceIdentifier();

                node.Name = Current.Value;
                Advance();
            }

            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            if (!Expect(Classifier.RightParenthesis))
            {
                node.Parameters = ParseArguments(Classifier.RightParenthesis, "right parenthesis");
            }

            if (Is(Keyword.Requires))
            {
                Advance();
                node.Guard = ParseStatement();
            }

            return ReadToEnd(node);
        }

        private Node ParseWhile()
        {
            Advance();
            var node = new WhileNode();
            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            Advance();

            node.Condition = ParseStatement();

            if (!Is(Classifier.RightParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
            }

            Advance();

            return ReadToEnd(node, true);
        }

        private Node ParseFor()
        {
            Advance();
            var node = new ForNode();

            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            Advance();

            if (Is("_"))
            {
                node.Variable = new NoVariableNode();
                Advance();
            }
            else
            {
                var index = _index;
                try
                {
                    node.Variable = ParseVariableDeclaration();
                }
                catch (Exception)
                {
                    _index = index;
                    EnforceIdentifier();

                    node.Variable = new IdentifierNode(Current.Value);
                    Advance();
                }
            }

            if (!Is(Keyword.In))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_IN);
            }

            Advance();

            node.Source = ParseStatement();

            if (!Is(Classifier.RightParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
            }

            Advance();

            return ReadToEnd(node, true);
        }
        
        //TODO: Parse if
        //TODO: Parse class

        #endregion

        #region Statements

        private Node ParsePackageImport()
        {
            var node = new WithNode();
            Advance();

            EnforceIdentifier();

            node.Target = Current.Value;

            if (Expect(Keyword.Fixed))
            {
                node.Fixed = true;
                Advance();
            }

            if (Expect(Keyword.As))
            {
                Advance(2);


                EnforceIdentifier();

                node.Name = Current.Value;
            }

            Advance();

            if (Is(Keyword.From)) //with x FROM ...
            {
                Advance();
                EnforceIdentifier();

                if (Expect(Classifier.Colon))
                {
                    node.Native = true;
                    node.NativePackage = Current.Value;

                    Advance(2);
                    EnforceIdentifier();

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
            EnforceIdentifier();

            var node = new CallNode {Source = baseNode, Target = new IdentifierNode(Current.Value)};
            Advance();

            return ParseCallSignature(node, true);
        }

        private Node ParseCallSignature(CallNode node, bool parseValueCall)
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

                        node.Parameters.Add(ParseStatement(), name);

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
                node.Parameters.Add(ParseStatement(), "");
            }
            else
            {
                if (parseValueCall)
                {
                    return new ValueCallNode {Source = node.Source, Target = node.Target};
                }

                Error(ErrorStrings.MESSAGE_EXPECTED_PARAMETERS);
            }

            return node;
        }

        private Node ParseDeepCall(Node node)
        {
            Node deepcall = null;

            while (Is(Classifier.Arrow))
            {
                Advance();
                deepcall = ParseCall(deepcall ?? node);
            }

            return deepcall;
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
                var (type, dt) = GetSpecificType();
                
                if (type == null)
                {
                    Advance();
                }
                
                variable.Datatype = dt;
                variable.SpecificType = type;
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

        private Node ParseAssignment(Node node)
        {
            Classifier GetNoAssignType(Classifier classifier)
            {
                return (Classifier) Enum.Parse(typeof(Classifier), classifier.ToString().Replace("Equal", ""));
            }

            Advance();
            switch (Last.Kind)
            {
                case Classifier.Assignment:
                    return new AssignmentNode {Variable = node, Value = ParseStatement()};
                default:
                    return new AssignmentNode {Variable = node, Value = new OperationNode {Operations = new List<Node> {node, new OperationNodeNode(GetNoAssignType(Last.Kind), Last.Value.Replace("=", "")), ParseStatement()}}};
            }
        }

        private Node ParseRightHand(Node node)
        {
            Advance();
            return new SideNode(node, new OperationNodeNode(Last.Kind, Last.Value), Side.RIGHT);
        }

        private Node ParseLeftHand(OperationNodeNode node)
        {
            Advance();
            return new SideNode(ParseStatement(), node, Side.LEFT);
        }

        private Node ParseArrayOrLambda()
        {
            Advance();

            var parameters = new List<(Node Key, Datatype? Value, string SpecificType)>();
            var isLambda = true;
            var index = _index;

            try
            {
                Advance(-1);
                //Assume it's a lambda and parse arguments
                parameters = ParseArguments(Classifier.FatArrow, "fat arrow");
            }
            catch (Exception e)
            {
                //Not a lambda
                isLambda = false;
            }
            
            //Lambda
            Node n;
            if (isLambda && Was(Classifier.FatArrow))
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
                _index = index;
                
                var vals = new List<Node>();
                
                //Collect array values
                do
                {
                    vals.Add(ParseStatement());
                    if (!Is(Classifier.RightBracket))
                    {
                        if (!Is(Classifier.Comma))
                        {
                            Error(ErrorStrings.MESSAGE_INVALID_ARRAY_EXPECTED_COMMA);
                        }

                        Advance();
                    }
                } while (!Is(Classifier.RightBracket));
                Advance();
                var node = new ListNode{Value = vals};
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
        ///     Parses blocks
        /// </summary>
        /// <returns></returns>
        private Node ParseNext(bool allowSkipStop = false)
        {
            while (Is(Classifier.NewLine))
            {
                Advance();
            }

            if (Is(Classifier.EndOfFile) || Is(Keyword.End))
            {
                return null;
            }

            if (IsKeyword())
            {
                if (allowSkipStop)
                {
                    if (Is(Keyword.Skip))
                    {
                        Advance();
                        return new CommandNode(Keyword.Skip);
                    }

                    if (Is(Keyword.Stop))
                    {
                        Advance();
                        return new CommandNode(Keyword.Stop);
                    }
                }

                switch (Current.Value)
                {
                    case Keyword.Put:
                        Advance();
                        return new PutNode {Statement = ParseStatement()};

                    case Keyword.Func:
                        return ParseFunc();

                    case Keyword.While:
                        return ParseWhile();

                    case Keyword.For:
                        return ParseFor();

                    case Keyword.Skip:
                    case Keyword.Stop:
                        Error(ErrorStrings.MESSAGE_UNEXPECTED_KEYWORD, Current.Value);
                        break;

                    default:
                        return ParseStatement();
                }
            }

            if (IsIdentifier() || Is(Category.Literal) && Expect(Classifier.Arrow) || Is(Category.Literal) && Expect(Category.Operator) || Is(Classifier.LeftParenthesis) || Is(Classifier.LeftBracket) || Is(Classifier.Not) || Is(Classifier.Minus))
            {
                return ParseStatement();
            }

            Error(ErrorStrings.MESSAGE_UNEXPECTED_TOKEN, Current.Value);

            return null;
        }

        /// <summary>
        ///     For statements that can have more complex nodes within the statement
        /// </summary>
        /// <returns></returns>
        private Node ParseStatement()
        {
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

            if (Is(Classifier.LeftParenthesis))
            {
                Advance();
                var n = ParseStatement();

                if (!Is(Classifier.RightParenthesis))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
                }

                Advance();

                while (Is(Category.Operator))
                {
                    n = getOperation(n);
                }

                return n;
            }

            var node = ParseStatementWithoutOperation();

            if (Is(Category.Operator) && node != null)
            {
                node = getOperation(node);
            }

            if (Is(Classifier.NullCondition))
            {
                Advance();
                return new NullConditionNode {Condition = node, Operation = ParseStatement()};
            }

            //Call on lambdas
            if (node is LambdaNode)
            {
                if (Is(Classifier.Colon) || Is(Classifier.LeftParenthesis))
                {
                    node = ParseCallSignature(new CallNode {Source = node, Target = new IdentifierNode("anonymous")}, false);
                }
            }

            if (Is(Classifier.Arrow))
            {
                node = ParseDeepCall(node);
            }

            if (Is(Category.Assignment))
            {
                return ParseAssignment(node);
            }

            if (Is(Category.RightHand))
            {
                return ParseRightHand(node);
            }

            if (Is(Classifier.Not) || Is(Classifier.Minus))
            {
                return ParseLeftHand(new OperationNodeNode(Current.Kind, Current.Value));
            }

            return node;
        }

        /// <summary>
        ///     For less complex statements (statements that do not require a preceding node
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