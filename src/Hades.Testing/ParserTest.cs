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
        [TestCase("with server", "server", "server", "server", false, null)]
        [TestCase("with server as foo", "server", "server", "foo", false, null)]
        [TestCase("with console from std:io","io","console","console",true, "std")]
        [TestCase("with math as m from std:math","math","math","m",true, "std")]
        [Test]
        public void EnsureWith(string code, string source, string target, string name, bool native, string nativepackage)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer.LexFile(code));
            var nodes = parser.Parse();
            var node = nodes.First();
            Assert.IsTrue(node is WithNode);
            var withNode = node as WithNode;

            Assert.AreEqual(withNode.Source, source);
            Assert.AreEqual(withNode.Target, target);
            Assert.AreEqual(withNode.Name, name);
            Assert.AreEqual(withNode.Native, native);
            Assert.AreEqual(withNode.NativePackage, nativepackage);
        }

        [TestCase("var string a = \"Hello\"")]
        [TestCase("var int a = 12")]
        [TestCase("var dec a = 1.2")]
        [TestCase("var bool a = false")]
        [TestCase("let string a = \"Hello\"")]
        [TestCase("let int a = 12")]
        [TestCase("let dec a = 1.2")]
        [TestCase("let bool a = false")]
        [TestCase("var a = \"Hello\"")]
        [TestCase("var a = 12")]
        [TestCase("var a = 1.2")]
        [TestCase("var a = false")]
        [TestCase("let a = \"Hello\"")]
        [TestCase("let a = 12")]
        [TestCase("let a = 1.2")]
        [TestCase("let a = false")]
        [TestCase("var string a")]
        [TestCase("var int a")]
        [TestCase("var dec a")]
        [TestCase("var bool a")]
        [TestCase("let string a")]
        [TestCase("let int a")]
        [TestCase("let dec a")]
        [TestCase("let bool a")]
        [TestCase("var a")]
        [TestCase("let a")]
        [Test]
        public void EnsureVariableDeclaration(string code)
        {
            
        }
    }
}