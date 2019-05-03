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

        [TestCase("with console from std:io\nfunc myFunction(int a) requires a is 11\nconsole->out(\"a is 11\")\nconsole->out(\"a is 11\")\nend", false)]
        [TestCase("while(c not 10)\nconsole->out(\"c is {}\"->format(c))\nend", false)]
        [TestCase("with console from std:io\nfunc myFunction(int a) requires a is 11\nconsole->out(\"a is 11\")\nskip\nend", true)]
        [TestCase("with console from std:io\nfunc myFunction(int a) requires a is 11\nconsole->out(\"a is 11\")\na->b = {x,y=>x+y}(1,2)->toString({x=>x})\nend", false)]
        [TestCase("for(var arg in a)\nconsole->out(arg)\nskip\nend\nfor(var arg in a)\nconsole->out(arg)\nskip\nend", false)]
        [TestCase("func doStuff(object::IClient a)\na->stuff(\"Hello\")\nend", false)]
        [TestCase("func doStuff(object::IClient a, int b)\na->stuff(\"Hello\")\nend", false)]
        [TestCase("func doStuff(object::IClient a, c)\na->stuff(\"Hello\")\nend", false)]
        [TestCase("func doStuff(object::IClient a, c, int d)\na->stuff(\"Hello\")\nend", false)]
        [TestCase("func doStuff(args object::IClient a, c)\na->stuff(\"Hello\")\nend", false)]
        [Test]
        public void ProgramTest(string code, bool fail)
        {
            FailTest(code, fail);
        }
        
        [TestCase("fib(10) |> doStuff |> console->out", false)]
        [TestCase("fib(10) |> console->out", false)]
        [TestCase("fruits |> map({x => x->toLower()}, ??) |> filter({x => x->startsWith(\"a\")}, ??) |> forEach({x => console->out(x)}, ??)", false)]
        [Test]
        public void PipelineTes(string code, bool fail)
        {
            FailTest(code, fail);
        }
    }
}