using System;
using System.Linq;
using Hades.Language.Lexer;
using Hades.Language.Parser;
using Hades.Syntax.Expression.Nodes;
using NUnit.Framework;

namespace Hades.Testing
{
    [TestFixture]
    public class ParserTest
    {
        private void FailTest(string code, bool fail)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer.LexFile(code));

            if (fail)
            {
                Assert.Throws<Exception>(() => parser.Parse());
            }
            else
            {
                Assert.DoesNotThrow(() => parser.Parse());
            }
        }

        [TestCase("(1 + 2) + 3", false)]
        [TestCase("(1 + 2) * (3 + (5 - 6))", false)]
        [TestCase("(1) * (3 + (5))", false)]
        [TestCase("1 + 2 * 3 - 4 / 5", false)]
        [TestCase("1 + 2 * 3 - 4 / !5", false)]
        [Test]
        public void CalculationTest(string code, bool fail)
        {
            FailTest(code, fail);
        }

        [TestCase("exceptions->ArgumentNullException(\"{} is null\"->format(nameof(b)))", false)]
        [TestCase("console->print(\"Variable is of type string\")", false)]
        [TestCase("console->out(\"Connection open!\")", false)]
        [TestCase("square(a)", false)]
        [TestCase("root(a)", false)]
        [TestCase("square(a,b", true)]
        [TestCase("root(a", true)]
        [Test]
        public void EnsureCall(string code, bool fail)
        {
            FailTest(code, fail);
        }

        [TestCase("var string a = \"Hello\"", false)]
        [TestCase("var int a = 12", false)]
        [TestCase("var dec a = 1.2", false)]
        [TestCase("var bool a = false", false)]
        [TestCase("let string a = \"Hello\"", false)]
        [TestCase("let int a = 12", false)]
        [TestCase("let dec a = 1.2", false)]
        [TestCase("let bool a = false", false)]
        [TestCase("var a = \"Hello\"", false)]
        [TestCase("var a = 12", false)]
        [TestCase("var a = 1.2", false)]
        [TestCase("var a = false", false)]
        [TestCase("let a = \"Hello\"", false)]
        [TestCase("let a = 12", false)]
        [TestCase("let a = 1.2", false)]
        [TestCase("let a = false", false)]
        [TestCase("var string a", false)]
        [TestCase("var int a", false)]
        [TestCase("var dec a", false)]
        [TestCase("var bool a", false)]
        [TestCase("let string a", false)]
        [TestCase("let int a", false)]
        [TestCase("let dec a", false)]
        [TestCase("let bool a", false)]
        [TestCase("var a", false)]
        [TestCase("let a", false)]
        [TestCase("var string? a = \"Hello\"", false)]
        [TestCase("var int? a = 12", false)]
        [TestCase("var dec? a = 1.2", false)]
        [TestCase("var bool? a = false", false)]
        [TestCase("var* a = \"Hello\"", false)]
        [TestCase("var* a = 12", false)]
        [TestCase("var* a = 1.2", false)]
        [TestCase("var* a = false", false)]
        [TestCase("var? a = \"Hello\"", false)]
        [TestCase("var? a = 12", false)]
        [TestCase("var? a = 1.2", false)]
        [TestCase("var? a = false", false)]
        [TestCase("var string?[*] a", false)]
        [TestCase("var int?[*] a", false)]
        [TestCase("var dec?[*] a", false)]
        [TestCase("var bool?[*] a", false)]
        [TestCase("var*[] a", false)]
        [TestCase("var int[3,3] matrix = {{1,0,0},{0,1,0},{0,0,1}}", false)]
        [TestCase("var int?[2,2,2] 3dArray = {{{1,2},{3,null}},{{null,6},{7,8}}}", false)]
        [TestCase("var object::IClient a", false)]
        [TestCase("var object::IClient? a", false)]
        [TestCase("var object::IClient?[] a", false)]
        [TestCase("var object::IClient?[*] a", false)]
        [TestCase("let object::IClient? a", true)]
        [TestCase("var* object::IClient a", true)]
        [TestCase("var* string a = \"Hello\"", true)]
        [TestCase("var* int a = 12", true)]
        [TestCase("var* dec a = 1.2", true)]
        [TestCase("var* bool a = false", true)]
        [TestCase("let* string a = \"Hello\"", true)]
        [TestCase("let* int a = 12", true)]
        [TestCase("let* dec a = 1.2", true)]
        [TestCase("let* bool a = false", true)]
        [TestCase("var? a", true)]
        [TestCase("let? a", true)]
        [Test]
        public void EnsureVariableDeclaration(string code, bool fail)
        {
            FailTest(code, fail);
        }

        [TestCase("with server", "server", "server", "server", false, null)]
        [TestCase("with server as foo", "server", "server", "foo", false, null)]
        [TestCase("with console from std:io", "io", "console", "console", true, "std")]
        [TestCase("with math as m from std:math", "math", "math", "m", true, "std")]
        [TestCase("with console fixed from std:io", "io", "console", "console", true, "std")]
        [TestCase("with math fixed as m from std:math", "math", "math", "m", true, "std")]
        [Test]
        public void EnsureWith(string code, string source, string target, string name, bool native, string nativepackage)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer.LexFile(code));
            var root = parser.Parse();
            var node = root.Children.First();
            Assert.IsTrue(node is WithNode);
            var withNode = node as WithNode;

            Assert.AreEqual(withNode.Source, source);
            Assert.AreEqual(withNode.Target, target);
            Assert.AreEqual(withNode.Name, name);
            Assert.AreEqual(withNode.Native, native);
            Assert.AreEqual(withNode.NativePackage, nativepackage);
        }

        [TestCase("fib(10) |> doStuff |> console->out", false)]
        [TestCase("fib(10) |> console->out", false)]
        [TestCase("fruits |> map({x => x->toLower()}, ??) |> filter({x => x->startsWith(\"a\")}, ??) |> forEach({x => console->out(x)}, ??)", false)]
        [Test]
        public void PipelineTes(string code, bool fail)
        {
            FailTest(code, fail);
        }

        [TestCase(
        "with console from std:io\n" + 
        "func myFunction(int a) requires a is 11\n" + 
            "console->out(\"a is 11\")\n" + 
            "console->out(\"a is 11\")\n" + 
        "end", false)]
        
        [TestCase(
        "while(c not 10)\n" + 
            "console->out(\"c is {}\"->format(c))\n" + 
        "end", false)]
        
        [TestCase(
        "with console from std:io\n" + 
        "func myFunction(int a) requires a is 11\n" + 
            "console->out(\"a is 11\")\n" + 
            "skip\n" + 
        "end", true)]
        
        [TestCase(
        "with console from std:io\n" + 
        "func myFunction(int a) requires a is 11\n" + 
            "console->out(\"a is 11\")\n" + 
            "a->b = {x,y=>x+y}(1,2)->toString({x=>x})\n" + 
        "end", false)]
        
        [TestCase(
        "for(var arg in a)\n" +
            "console->out(arg)\n" +
            "skip\n" +
        "end\n" +
        "for(var arg in a)\n" +
            "console->out(arg)\n" +
            "skip\n" +
        "end", false)]
        
        [TestCase(
        "func doStuff(object::IClient a)\n" +
            "a->stuff(\"Hello\")\n" +
        "end", false)]
        
        [TestCase(
        "func doStuff(object::IClient a, int b)\n" +
            "a->stuff(\"Hello\")\n" +
        "end", false)]
        
        [TestCase(
        "func doStuff(object::IClient a, c)\n" +
            "a->stuff(\"Hello\")\n" +
        "end", false)]
        
        [TestCase(
        "func doStuff(object::IClient a, c, int d)\n" +
            "a->stuff(\"Hello\")\n" +
        "end", false)]
        
        [TestCase(
        "func doStuff(args object::IClient a, c)\n" +
            "a->stuff(\"Hello\")\n" +
        "end", false)]
        
        [TestCase(
        "with console from std:io\n" +
        "if(a < 10)\n" +
            "console->out(\"a is smaller than 10\")\n" +
        "else if(a is 11)\n" +
            "console->out(\"a is 11\")\n" +
        "else if(a > 11 and a < 21)\n" +
            "console->out(\"a is greater than 11 and smaller than 21\")\n" +
        "else\n" +
            "console->out(\"a is \" + a)\n" +
        "end", false)]
        
        [TestCase(
        "with math as m from std:math\n" +
        "func doMath(int a)\n" +
            "func root(int b)\n" +
                "put m->sqrt(b)\n" +
            "end\n" +
            "func square(b)\n" +
                "put b * b\n" +
            "end\n" +
            "put square(a) + root(a)\n" +
        "end", false)]
        
        [TestCase(
        "srv->get(\"/:path\", {req, res => \n" +
            "let path = req->param\n" +
            "try\n" +
                "if (file->exists(path))\n" +
                    "let f = file->open(path)\n" +
                    "res->send(f->read())\n" +
                "else\n" +
                    "raise 404\n" +
                "end\n" +
            "catch(int status)\n" +
                "res->status(status)\n" +
            "else\n" +
                "res->status(200)\n" +
            "end\n" +
        "})", false)]
        
        [TestCase(
        "var fruits = list->of({\"Apple\", \"Banana\", \"Mango\", \"Kiwi\", \"Avocado\"})\n" +
        "fruits\n" +
        "|> map({x => x->toLower()}, ??)\n" +
        "|> filter({x => x->startsWith(\"a\")}, ??)\n" +
        "|> forEach({x => console->out(x)}, ??)", false)]
        
        [TestCase(
        "let string[] fruits = {\"Apple\", \"Banana\", \"Mango\", \"Kiwi\"}\n" +
        "for(var fruit in fruits)\n" +
            "console->out(\"{} is very healthy\"->format(fruit))\n" +
        "end\n" +
        "var a = params->get(0)\n" +
        "var b = params->get(1)\n" +
        "a :: raise exceptions->ArgumentNullException(\"{} is null\"->format(nameof(a)))\n" +
        "b :: raise exceptions->ArgumentNullException(\"{} is null\"->format(nameof(b)))", false)]
        
        [TestCase(
        "try\n" +
            "connection->open()\n" +
            "console->out(\"Connection open!\")\n" +
            "connection->close()\n" +
        "catch(object::SqlException e)\n" +
            "console->out(\"SqlException was caught!\")\n" +
        "catch(e)\n" +
            "console->out(\"An unknown exception was caught!\")\n" +
        "else\n" +
            "console->out(\"No exception thrown!\")\n" +
        "end", false)]
        [Test]
        public void ProgramTest(string code, bool fail)
        {
            FailTest(code, fail);
        }
    }
}