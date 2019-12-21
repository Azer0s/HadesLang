using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hades.Language.Lexer
{
	public class Lexer
	{
		private readonly string _source;
		private int _index;
		private StringBuilder _buffer = new StringBuilder();

		private Character Ch => Peek(0);
		private Character Next => Peek(1);
		
		private Lexer(string source)
		{
			_source = source;
		}
		
		private void Consume(int amount = 1)
		{
			for (var i = 0; i < amount; i++)
			{
				_buffer.Append(Ch.Value);
				Advance();
			}
		}

		private void Advance(int amount = 1)
		{
			_index += amount;
		}

		private Token CreateToken(Type type)
		{
			var token = new Token
			{
				Type = type,
				Value = _buffer.ToString()
			};
			_buffer = new StringBuilder();
			return token;
		}

		private Character Peek(int by)
		{
			return _index + by != _source.Length
				? new Character {Value = _source[_index + by], IsEof = false}
				: new Character {Value = ' ', IsEof = true};
		}

		private bool IsWhiteSpace()
		{
			var c = Ch.Value;
			return c == ' ' || c == '\n' || c == '\t';
		}

		private List<Token> GetKeywordOrToken(Token token)
		{
			token.Type = token.Value switch
			{
				"true" => Type.Bool,
				"false" => Type.Bool,
				"class" => Type.Class,
				"func" => Type.Func,
				"args" => Type.Args,
				"requires" => Type.Requires,
				"if" => Type.If,
				"else" => Type.Else,
				"while" => Type.While,
				"for" => Type.For,
				"in" => Type.In,
				"stop" => Type.Stop,
				"skip" => Type.Skip,
				"try" => Type.Try,
				"catch" => Type.Catch,
				"end" => Type.End,
				"var" => Type.Var,
				"let" => Type.Let,
				"null" => Type.Null,
				"protected" => Type.Protected,
				"public" => Type.Public,
				"private" => Type.Private,
				"with" => Type.With,
				"from" => Type.From,
				"as" => Type.As,
				"sets" => Type.Sets,
				"put" => Type.Put,
				"raise" => Type.Raise,
				"fixed" => Type.Fixed,
				"match" => Type.Match,
				"struct" => Type.Struct,
				"receive" => Type.Receive,
				"and" => Type.LogicalAnd,
				"or" => Type.LogicalOr,
				"is" => Type.Equals,
				"not" => Type.NotEquals,
				_ => token.Type
			};

			if (Datatypes.All.Any(x => Regex.IsMatch(token.Value, $"{x}\\?")))
			{
				var datatype = Regex.Match(token.Value ?? "", @"([^\?]+)?").Groups[1].Value;
				return new List<Token>{new Token{Type = Type.Identifier, Value = datatype}, new Token{Type = Type.Nullable, Value = "?"}};
			}

			return new List<Token> {token};
		}

		private Token LexLiteral()
		{
			if (char.IsLetter(Ch.Value) || Ch.Value == '_')
			{
				Consume();
			}
			else
			{
				throw new Exception($"Unexpected char {Ch.Value}!");
			}
			
			while (!Ch.IsEof && char.IsLetter(Ch.Value) || Ch.Value == '_')
			{
				Consume();
			}

			if (!Ch.IsEof && Ch.Value == '?' || Ch.Value == '!')
			{
				Consume();
			}

			return CreateToken(Type.Identifier);
		}

		private Token LexString(char endChar)
		{
			Advance();

			var escape = false;

			while (!Ch.IsEof)
			{
				if (escape)
				{
					_buffer.Append(Ch.Value switch
					{
						'n' => '\n',
						't' => '\t',
						'\'' => '\'',
						'"' => '"',
						_ => throw new Exception($"Unexpected escape character \\{Ch.Value}!")
					});
					Advance();
					escape = false;
				}
				else
				{
					if (Ch.Value == endChar)
					{
						Advance();
						return CreateToken(Type.String);
					}
					
					switch (Ch.Value)
					{
						case '\\':
							Advance();
							escape = true;
							break;
						default:
							Consume();
							break;
					}
				}
			}
			
			throw new Exception("Unexpected EOF!");
		}

		private Token LexAtom()
		{
			if (!Next.IsEof && char.IsLetter(Next.Value))
			{
				Consume();
				var token = LexLiteral();
				token.Type = Type.Atom;

				return token;
			}
			
			throw new Exception("Expected a letter after :!");
		}

		private Token LexNumber()
		{
			while (!Ch.IsEof && char.IsDigit(Ch.Value))
			{
				Consume();
			}

			return Ch.Value == '.' ? LexFloat() : CreateToken(Type.Integer);
		}

		private Token LexFloat()
		{
			if (!Next.IsEof && char.IsDigit(Next.Value))
			{
				Consume();
				while (!Ch.IsEof && char.IsDigit(Ch.Value))
				{
					Consume();
				}

				return CreateToken(Type.Float);
			}

			return CreateToken(Type.Integer);
		}

		private Token LexOperatorWithCompound(Type compoundType, Type normalType)
		{
			if (!Next.IsEof && Next.Value == '=')
			{
				Consume(2);
				return CreateToken(compoundType);
			}

			Consume();
			return CreateToken(normalType);
		}
		
		private Token LexOperatorWithDoubleAndCompound(char ch, Type compoundType, Type doubleType, Type normalType)
		{
			if (!Next.IsEof && Next.Value == ch)
			{
				Consume(2);
				return CreateToken(doubleType);
			}

			if (!Next.IsEof && Next.Value == '=')
			{
				Consume(2);
				return CreateToken(compoundType);
			}

			Consume();
			return CreateToken(normalType);
		}

		private Token LexOperatorWithDoubleAndDoubleCompound(char ch, Type compoundDoubleType, Type doubleType, Type normalType)
		{
			if (!Next.IsEof && Next.Value == ch)
			{
				Consume(2);

				if (!Ch.IsEof && Ch.Value == '=')
				{
					Consume();
					return CreateToken(compoundDoubleType);
				}
				
				return CreateToken(doubleType);
			}

			Consume();
			return CreateToken(normalType);
		}
		
		private List<Token> Lex()
		{
			var tokens = new List<Token>();
			while (!Ch.IsEof)
			{
				switch (Ch.Value)
				{
					case { } c when char.IsLetter(c) || c == '_':
						tokens.AddRange(GetKeywordOrToken(LexLiteral()));
						break;
					case { } c when char.IsDigit(c):
						tokens.Add(LexNumber());
						break;
					case '"':
					case '\'':
						tokens.Add(LexString(Ch.Value));
						break;
					case ':':
						if (!Next.IsEof && Next.Value == '=')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.MatchAssign));
						}
						else
						{
							tokens.Add(LexAtom());
						}
						break;
					case '.':
						Consume();
						tokens.Add(CreateToken(Type.Dot));
						break;
					case '+':
						tokens.Add(LexOperatorWithDoubleAndCompound('+', Type.CompoundIncrement, Type.Increment, Type.Plus));
						break;
					case '-':
						tokens.Add(LexOperatorWithDoubleAndCompound('-', Type.CompoundDecrement, Type.Decrement, Type.Minus));
						break;
					case '*':
						tokens.Add(LexOperatorWithCompound(Type.CompoundMultiplication, Type.Multiplication));
						break;
					case '/':
						if (!Next.IsEof && Next.Value == '/')
						{
							Advance(2);
							while (!Ch.IsEof && Ch.Value != '\n')
							{
								Advance();
							}
						}
						else if(!Next.IsEof && Next.Value == '*')
						{
							Advance(2);

							while (!Ch.IsEof && !Next.IsEof && !(Ch.Value == '*' && Next.Value == '/'))
							{
								Advance();
							}

							if (Ch.Value != '*' && Next.Value != '/')
							{
								throw new Exception("Unexpected EOF!");
							}

							Advance(2);
						}
						else
						{
							tokens.Add(LexOperatorWithCompound(Type.CompoundDivision, Type.Division));
						}
						break;
					case '%':
						tokens.Add(LexOperatorWithCompound(Type.CompoundMod, Type.Mod));
						break;
					case '<':
						tokens.Add(LexOperatorWithDoubleAndDoubleCompound('<', Type.CompoundLeftShift, Type.LeftShift, Type.Smaller));
						break;
					case '>':
						tokens.Add(LexOperatorWithDoubleAndDoubleCompound('>', Type.CompoundRightShift, Type.RightShift, Type.Bigger));
						break;
					case '!':
						tokens.Add(LexOperatorWithCompound(Type.NotEquals, Type.ExclamationMark));
						break;
					case '&':
						tokens.Add(LexOperatorWithDoubleAndCompound('&', Type.CompoundBitwiseAnd, Type.LogicalAnd, Type.BitwiseAnd));
						break;
					case '|':
						var tokenA = LexOperatorWithDoubleAndCompound('|', Type.CompoundBitwiseOr, Type.LogicalOr, Type.BitwiseOr);

						if (!Ch.IsEof && Ch.Value == '>')
						{
							tokenA.Type = Type.Pipeline;
							tokenA.Value = "|>";
							Advance();
						}
						
						tokens.Add(tokenA);
						break;
					case '^':
						tokens.Add(LexOperatorWithCompound(Type.CompoundBitwiseXor, Type.BitwiseXor));
						break;
					case '~':
						Consume();
						tokens.Add(CreateToken(Type.BitwiseNegate));
						break;
					case '=':
						var tokenB = LexOperatorWithCompound(Type.Equals, Type.Assign);

						if (!Ch.IsEof && Ch.Value == '>')
						{
							tokenB.Type = Type.FatArrow;
							tokenB.Value = "=>";
							Advance();
						}
						
						tokens.Add(tokenB);
						break;
					case '(':
						Consume();
						tokens.Add(CreateToken(Type.OpenParentheses));
						break;
					case ')':
						Consume();
						tokens.Add(CreateToken(Type.ClosedParentheses));
						break;
					case '{':
						Consume();
						tokens.Add(CreateToken(Type.OpenBrace));
						break;
					case '}':
						Consume();
						tokens.Add(CreateToken(Type.ClosedBrace));
						break;
					case '[':
						Consume();
						tokens.Add(CreateToken(Type.OpenBracket));
						break;
					case ']':
						Consume();
						tokens.Add(CreateToken(Type.ClosedBracket));
						break;
					case ',':
						Consume();
						tokens.Add(CreateToken(Type.Comma));
						break;
					case ';':
						Consume();
						tokens.Add(CreateToken(Type.Semicolon));
						break;
					case { } _ when IsWhiteSpace():
						Advance();
						break;
					case '?':
						if (!Next.IsEof && Next.Value == '?')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.PipelinePlaceholder));
						}
						else
						{
							Consume();
							tokens.Add(CreateToken(Type.Nullable));
						}
						break;
					default:
						throw new Exception($"Unexpected character {Ch.Value}");
				}
			}

			return tokens;
		}

		public static List<Token> Lex(string source)
		{
			return new Lexer(source).Lex();
		}
	}
}