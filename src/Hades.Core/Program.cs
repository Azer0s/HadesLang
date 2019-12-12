using System;
using Hades.Language;
using Hades.Language.Lexer;
using Hades.Language.Parser;

namespace Hades.Core
{
	class Program
	{
		static void Main()
		{
			Lexer.Lex("123.2").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("123.toString()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("123.2.toString()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("true false").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("{:ok, val}").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("hasType?()").ForEach(x => Console.WriteLine(x));
			Lexer.Lex("send!()").ForEach(x => Console.WriteLine(x));

			Lexer.Lex(@"""Hello world""").ForEach(x => Console.WriteLine(x));
			Lexer.Lex(@"""\n \t \"" \'""").ForEach(x => Console.WriteLine(x));

			Lexer.Lex(@"
with list fixed from std:collections
with console from std:io

var fruits = list.of({""Apple"", ""Banana"", ""Mango"", ""Kiwi"", ""Avocado""})

fruits
|> map(??, {x => x.toLower()})
|> filter({x => x.startsWith(""a"")})
//if ?? is not in the parameters, the method is inserted as the first parameter

/*
* Test
* Comment
*/

|> forEach(??, {x => console.out(x)})").ForEach(x => Console.WriteLine(x));

			Parser.Parse(Lexer.Lex("var a"));
			Parser.Parse(Lexer.Lex("var int a"));
			Parser.Parse(Lexer.Lex("var int? a"));
			Parser.Parse(Lexer.Lex("var [10] a"));
			Parser.Parse(Lexer.Lex("var int[*] a"));
			Parser.Parse(Lexer.Lex("var int?[2,2] a"));
			Parser.Parse(Lexer.Lex("let a"));
			Parser.Parse(Lexer.Lex("let int a"));
		}
	}
}