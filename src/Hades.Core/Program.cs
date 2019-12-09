using System;
using Hades.Language;
using Hades.Language.Lexer;

namespace Hades.Core
{
	class Program
	{
		static void Main(string[] args)
		{
			Lexer.Lex("123.2").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("123.toString()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("123.2.toString()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("true false").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("{:ok, val}").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("hasType?()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("send!()").ForEach(x => Console.WriteLine(x));
		}
	}
}