using System;
using System.Collections.Generic;
using System.Linq;
using Type = Hades.Language.Lexer.Type;
using System.Text;

namespace Hades.Language.Lexer
{
	public class Lexer
	{
		private readonly string _source;
		private int _index = 0;
		private StringBuilder _buffer = new StringBuilder();
		
		private char Last => Peek(-1).Value;
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

		private void Advance()
		{
			_index++;
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

		private Token GetKeywordOrToken(Token token)
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

			return token;
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

		private Token LexString()
		{
			//TODO: Escape \', \", \t, \n
			//TODO: Unicode support
			return new Token();
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
		
		private Token LexOperatorWithCompound(char ch, Type compoundType, Type doubleType, Type normalType)
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

		private Token LexOperatorWithDoubleAndCompound(char ch, Type compoundDoubleType, Type doubleType, Type normalType)
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
					case char c when char.IsLetter(c) || c == '_':
						tokens.Add(GetKeywordOrToken(LexLiteral()));
						break;
					case char c when char.IsDigit(c):
						tokens.Add(LexNumber());
						break;
					case '"':
					case '\'':
						tokens.Add(LexString());
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
						if (!Next.IsEof && Next.Value == '+')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.Increment));
						}
						else if (!Next.IsEof && Next.Value == '=')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.CompoundIncrement));
						}
						else
						{
							Consume();
							tokens.Add(CreateToken(Type.Plus));
						}
						break;
					case '-':
						if (!Next.IsEof && Next.Value == '-')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.Decrement));
						}
						else if (!Next.IsEof && Next.Value == '=')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.CompoundDecrement));
						}
						else
						{
							Consume();
							tokens.Add(CreateToken(Type.Minus));
						}
						break;
					case '*':
						tokens.Add(LexOperatorWithCompound(Type.CompoundMultiplication, Type.Multiplication));
						break;
					case '/':
						tokens.Add(LexOperatorWithCompound(Type.CompoundDivision, Type.Division));
						break;
					case '%':
						tokens.Add(LexOperatorWithCompound(Type.CompoundMod, Type.Mod));
						break;
					case '<':
						tokens.Add(LexOperatorWithDoubleAndCompound('<', Type.CompoundLeftShift, Type.LeftShift, Type.Smaller));
						break;
					case '>':
						tokens.Add(LexOperatorWithDoubleAndCompound('>', Type.CompoundRightShift, Type.RightShift, Type.Bigger));
						break;
					case '!':
						if (!Next.IsEof && Next.Value == '=')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.NotEquals));
						}
						else
						{
							Consume();
							tokens.Add(CreateToken(Type.ExclamationMark));
						}
						break;
					case '&':
						tokens.Add(LexOperatorWithCompound('&', Type.CompoundBitwiseAnd, Type.LogicalAnd, Type.BitwiseAnd));
						break;
					case '|':
						tokens.Add(LexOperatorWithCompound('|', Type.CompoundBitwiseOr, Type.LogicalOr, Type.BitwiseOr));
						break;
					case '^':
						tokens.Add(LexOperatorWithCompound(Type.CompoundBitwiseXor, Type.BitwiseXor));
						break;
					case '~':
						Consume();
						tokens.Add(CreateToken(Type.BitwiseNegate));
						break;
					case '=':
						if (!Next.IsEof && Next.Value == '=')
						{
							Consume(2);
							tokens.Add(CreateToken(Type.Equals));
						}
						else
						{
							Consume();
							tokens.Add(CreateToken(Type.Assign));
						}
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
					case char c when IsWhiteSpace():
						Advance();
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