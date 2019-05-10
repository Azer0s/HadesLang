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
            _tokens = tokens.ToList().Where(a => a.Kind != Classifier.WhiteSpace && a.Category != Category.Comment && a.Kind != Classifier.NewLine);
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

        private BlockNode ReadToEnd(BlockNode node, bool allowSkipStop = false, List<string> keywords = null)
        {
            if (keywords == null)
            {
                keywords = new List<string> {Keyword.End};
            }

            while (!Is(keywords))
            {
                if (IsEof())
                {
                    Error(ErrorStrings.MESSAGE_UNEXPECTED_EOF);
                }

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
        ///     Gets the specific type of an object or proto
        ///     ```
        ///     func doStuff(object::IClient a)
        ///     a->stuff("Hello world")
        ///     end
        ///     ```
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
                Error(ErrorStrings.MESSAGE_EXPECTED_VALUE, expect);
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

        private bool Was(string token)
        {
            return Last == token;
        }

        private bool Is(string token)
        {
            return Current == token;
        }

        private bool Is(IEnumerable<string> tokens)
        {
            return tokens.Any(token => token == Current);
        }

        private bool Is(Classifier classifier)
        {
            return Current == classifier;
        }

        private bool IsAccessModifier()
        {
            return Is(Keyword.Private) || Is(Keyword.Public) || Is(Keyword.Protected);
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

        private Node ParseClass(bool isFixed, AccessModifier accessModifier)
        {
            Advance();
            EnforceIdentifier();

            var node = new ClassNode {Name = Current.Value, Fixed = isFixed, AccessModifier = accessModifier};

            Advance();

            if (Is(Classifier.LessThan))
            {
                do
                {
                    Advance();
                    EnforceIdentifier();
                    node.Parents.Add(Current.Value);
                    Advance();
                } while (Is(Classifier.Comma));

                Advance(-1);
            }

            while (!Is(Keyword.End))
            {
                if (IsAccessModifier() && (Expect(Keyword.Var) || Expect(Keyword.Let)))
                {
                    Advance();
                    switch (Enum.Parse<AccessModifier>(Last.Value.First().ToString().ToUpper() + Last.Value.Substring(1)))
                    {
                        case AccessModifier.Protected:
                            node.ProtectedVariables.Add(ParseNext() as VariableDeclarationNode);
                            break;
                        case AccessModifier.Public:
                            node.PublicVariables.Add(ParseNext() as VariableDeclarationNode);
                            break;
                        default:
                            node.PrivateVariables.Add(ParseNext() as VariableDeclarationNode);
                            break;
                    }
                }
                else
                {
                    var childNode = ParseNext();

                    if (childNode is VariableDeclarationNode vn)
                    {
                        node.PrivateVariables.Add(vn);
                    }
                    else if (childNode is FunctionNode fn)
                    {
                        if (fn.Name == node.Name)
                        {
                            node.Constructors.Add(fn);
                        }
                        else
                        {
                            node.Functions.Add(fn);
                        }
                    }
                    else if (childNode is ClassNode cn)
                    {
                        node.Classes.Add(cn);
                    }
                    else
                    {
                        Error(ErrorStrings.MESSAGE_UNEXPECTED_NODE, childNode.GetType().Name.Replace("Node", ""));
                    }
                }
            }

            Advance();
            return node;
        }

        private Node ParseFunc(bool isFixed, AccessModifier accessModifier)
        {
            Advance();
            var node = new FunctionNode {Fixed = isFixed, AccessModifier = accessModifier};
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
                Advance();
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

        private Node GetCondition()
        {
            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            Advance();

            var node = ParseStatement();

            if (!Is(Classifier.RightParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
            }

            Advance();
            return node;
        }

        private Node ParseMatch(bool allowSkipStop)
        {
            Advance();
            var node = new MatchNode {First = Is("first")};

            if (Is("first"))
            {
                Advance();
            }

            if (!Is(Classifier.LeftParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
            }

            Advance();

            node.Match = Is(Classifier.Underscore /*No statement*/) ? new NoVariableNode() : ParseStatement();

            if (!Is(Classifier.RightParenthesis))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
            }

            Advance();

            while (!Is(Keyword.End))
            {
                var cond = ParseStatement();

                if (!Is(Classifier.FatArrow))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_TOKEN, "=>");
                }

                Advance();

                var action = ParseStatement(allowSkipStop);

                if (action is LambdaNode ln)
                {
                    if (ln.Complex)
                    {
                        Error(ErrorStrings.MESSAGE_CANT_USE_COMPLEX_LAMBDA_IN_MATCH_BLOCK);
                    }
                }

                //Actually...mostly anything can be an action if you think about it.
                //We could have something like "Hello" => a->getAction(10)
                //So here, the action would be a call which would return a lambda
                //The only thing we *really* can't have is a complex lambda

                node.Statements.Add(cond, action);
            }

            Advance();

            return node;
        }

        private Node ParseWhile()
        {
            Advance();
            var node = new WhileNode {Condition = GetCondition()};

            return ReadToEnd(node, true);
        }

        private Node ParseIf(bool allowSkipStop)
        {
            Advance();
            var node = new IfNode {Condition = GetCondition(), If = ReadToEnd(new GenericBlockNode(), allowSkipStop, new List<string> {Keyword.End, Keyword.Else})};


            if (Was(Keyword.End))
            {
                return node;
            }

            while (Was(Keyword.Else) && Is(Keyword.If))
            {
                Advance();
                node.ElseIfNodes.Add(new IfNode {Condition = GetCondition(), If = ReadToEnd(new GenericBlockNode(), allowSkipStop, new List<string> {Keyword.End, Keyword.Else})});
            }

            if (Was(Keyword.Else))
            {
                node.Else = ReadToEnd(new GenericBlockNode(), allowSkipStop);
            }

            return node;
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

        private Node ParseTryCatchElse(bool allowSkipStop)
        {
            Advance();
            var node = new TryCatchElseNode {Try = ReadToEnd(new GenericBlockNode(), allowSkipStop, new List<string> {Keyword.Catch, Keyword.Else})};


            while (Was(Keyword.Catch))
            {
                var catchNode = new TryCatchElseNode.CatchBlock();

                if (!Is(Classifier.LeftParenthesis))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_LEFT_PARENTHESIS);
                }

                Advance();

                if (IsType())
                {
                    (catchNode.SpecificType, catchNode.Datatype) = GetSpecificType();

                    if (catchNode.SpecificType == null)
                    {
                        Advance();
                    }
                }

                EnforceIdentifier();
                catchNode.Name = Current.Value;
                Advance();

                if (!Is(Classifier.RightParenthesis))
                {
                    Error(ErrorStrings.MESSAGE_EXPECTED_RIGHT_PARENTHESIS);
                }

                Advance();

                catchNode.Block = ReadToEnd(new GenericBlockNode(), allowSkipStop, new List<string> {Keyword.Catch, Keyword.End, Keyword.Else});
                node.Catch.Add(catchNode);
            }

            if (Was(Keyword.Else))
            {
                node.Else = ReadToEnd(new GenericBlockNode(), allowSkipStop);
            }

            return node;
        }

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
            Node deepCall = null;

            while (Is(Classifier.Arrow))
            {
                Advance();
                deepCall = ParseCall(deepCall ?? node);
            }

            return deepCall;
        }

        private Node ParseInlineIf(Node node)
        {
            Advance();
            var truthy = ParseStatement();

            if (!Is(Classifier.Colon))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_COLON);
            }

            Advance();
            var falsy = ParseStatement();
            return new InlineIf {Condition = node, Falsy = falsy, Truthy = truthy};
        }

        private Node ParsePipeline(Node node)
        {
            var root = node;
            do
            {
                Advance();
                var child = ParseStatement(true);

                if (child is ValueCallNode vcn)
                {
                    var callNode = new CallNode {Source = vcn.Source, Target = vcn.Target};
                    callNode.Parameters.Add(root, "");
                    root = callNode;
                }
                else if (child is IdentifierNode id)
                {
                    var callNode = new CallNode {Source = new IdentifierNode("this"), Target = id};
                    callNode.Parameters.Add(root, "");
                    root = callNode;
                }
                else if (child is CallNode cn)
                {
                    var placeHolders = cn.Parameters.Where(a => a.Key is PlaceHolderNode).Select(a => a).ToList();

                    placeHolders.ForEach(a => { cn.Parameters.Remove(a.Key); });

                    placeHolders.ForEach(a => { cn.Parameters.Add(root, a.Value); });

                    root = cn;
                }
                else
                {
                    Error(ErrorStrings.MESSAGE_UNEXPECTED_STATEMENT);
                }
            } while (Is(Classifier.Pipeline));

            return root;
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

                        if (Is(Classifier.Comma))
                        {
                            var multiDimensionalArray = new MultiDimensionalArrayNode();
                            multiDimensionalArray.Value.Add(variable.ArraySize);
                            while (Is(Classifier.Comma))
                            {
                                Advance();
                                multiDimensionalArray.Value.Add(ParseStatement());
                            }

                            variable.ArraySize = multiDimensionalArray;
                        }
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

        private Node ParseArrayAccess(Node node)
        {
            Advance();
            var index = ParseStatement();

            if (!Is(Classifier.RightBrace))
            {
                Error(ErrorStrings.MESSAGE_EXPECTED_TOKEN, "]");
            }

            Advance();

            return new ArrayAccessNode {BaseNode = node, Index = index};
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
            catch (Exception)
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
                var node = new ListNode {Value = vals};
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

                AccessModifier? accessModifier = null;
                if (IsAccessModifier())
                {
                    accessModifier = Enum.Parse<AccessModifier>(Current.Value.First().ToString().ToUpper() + Current.Value.Substring(1));
                    Advance();
                }

                var isFixed = false;
                if (Is(Keyword.Fixed))
                {
                    isFixed = true;
                    Advance();
                    //HACK: this is not beautiful
                }

                void NoAccessModifierOrFixed()
                {
                    if (isFixed) Error(ErrorStrings.MESSAGE_UNEXPECTED_KEYWORD, Keyword.Fixed);
                    if (accessModifier != null) Error(ErrorStrings.MESSAGE_UNEXPECTED_ACCESS_MODIFIER);
                }

                switch (Current.Value)
                {
                    case Keyword.Put:
                        NoAccessModifierOrFixed();
                        Advance();
                        return new PutNode {Statement = ParseStatement()};

                    case Keyword.Match:
                        NoAccessModifierOrFixed();
                        return ParseMatch(allowSkipStop);

                    case Keyword.Class:
                        return ParseClass(isFixed, accessModifier.GetValueOrDefault());

                    case Keyword.Func:
                        return ParseFunc(isFixed, accessModifier.GetValueOrDefault());

                    case Keyword.While:
                        NoAccessModifierOrFixed();
                        return ParseWhile();

                    case Keyword.If:
                        NoAccessModifierOrFixed();
                        return ParseIf(allowSkipStop);

                    case Keyword.For:
                        NoAccessModifierOrFixed();
                        return ParseFor();

                    case Keyword.Try:
                        NoAccessModifierOrFixed();
                        return ParseTryCatchElse(allowSkipStop);

                    case Keyword.Skip:
                    case Keyword.Stop:
                        NoAccessModifierOrFixed();
                        Error(ErrorStrings.MESSAGE_UNEXPECTED_KEYWORD, Current.Value);
                        break;

                    default:
                        return ParseStatement();
                }
            }

            if (IsIdentifier() || Is(Category.Literal) && (Expect(Classifier.Arrow) || Expect(Classifier.Question)) || Is(Category.Literal) && Expect(Category.Operator) || Is(Classifier.LeftParenthesis) || Is(Classifier.LeftBracket) || Is(Classifier.Not) || Is(Classifier.Minus))
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
        private Node ParseStatement(bool pipeline = false)
        {
            Node GetOperation(Node initial = null)
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

            Node node;

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
                    n = GetOperation(n);
                }

                //I wanted to manually differentiate between a calculation and a calculation in ()
                node = n is OperationNode ? new ParenthesesNode {Node = n} : n;
            }
            else
            {
                node = ParseStatementWithoutOperation();
            }


            if (Is(Category.Operator) && node != null)
            {
                node = GetOperation(node);
            }

            if (Is(Classifier.NullCondition))
            {
                Advance();
                return new NullConditionNode {Condition = node, Operation = ParseStatement()};
            }

            //Call on lambdas
            if (node is LambdaNode || node is CallNode /*TODO: Or node is ArrayAccessNode*/)
            {
                if (Is(Classifier.LeftParenthesis))
                {
                    node = ParseCallSignature(new CallNode {Source = node, Target = new IdentifierNode("anonymous")}, false);
                }
            }

            if (Is(Classifier.Question))
            {
                node = ParseInlineIf(node);
            }

            if (Is(Classifier.Pipeline) && !pipeline)
            {
                node = ParsePipeline(node);
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

            if (Is(Classifier.LeftBrace))
            {
                return ParseArrayAccess(node);
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

                    case Keyword.Null:
                        Advance();
                        return new NullValueNode();

                    default:
                        Error(ErrorStrings.MESSAGE_UNEXPECTED_KEYWORD, Current.Value);
                        break;
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
                if (Expect(Classifier.LeftParenthesis))
                {
                    return ParseCall(new IdentifierNode("this"));
                }

                Advance();
                return new IdentifierNode(Last.Value);
            }

            if (Is(Classifier.DoubleQuestion))
            {
                Advance();
                return new PlaceHolderNode();
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

            if (Is(Classifier.Not) || Is(Classifier.Minus))
            {
                return ParseLeftHand(new OperationNodeNode(Current.Kind, Current.Value));
            }

            //TODO: Rework calculations
            return null;
        }

        #endregion

        #endregion
    }
}