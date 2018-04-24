using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Output;

namespace Testing
{
    [TestFixture]
    public class Testingcs
    {
        private readonly Interpreter.Interpreter inter = new Interpreter.Interpreter(new ConsoleOutput(), new ConsoleOutput());
        private readonly string _prefix = string.Format("..{0}..{0}..{0}..{0}HadesLang{0}", Path.DirectorySeparatorChar);

        [TearDown]
        public void TearDown()
        {
            inter.InterpretLine("dumpVars:all", new List<string> {"testing"}, null, true);
            inter.InterpretLine("uload:all", new List<string>{"testing"}, null);
            inter.InterpretLine("dumpVars:all", new List<string> { "testing" }, null, true);
            inter.ShouldError(false);
        }

        [Test]
        public void AddTest()
        {
            Assert.AreEqual("15", inter.InterpretLine("10+5", new List<string>{"testing"}, null));
        }

        [Test]
        public void PipelineTest()
        {
            inter.InterpretLine($"with '{new FileInfo(_prefix + "fibrec.hd").FullName}' as a", new List<string> { "testing" }, null);
            Assert.AreEqual("'34'", inter.InterpretLine("$a->fib:[9] |> out:[??] |> out:[??]", new List<string> { "testing" }, null));
        }

        [Test]
        public void ConcatTest()
        {
            Assert.AreEqual("'Hello world'",inter.InterpretLine("'Hello' + ' ' + 'world'", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("a", "word", "'Hello world'")]
        [TestCase("b", "num", "15")]
        [TestCase("c", "dec", "32.5")]
        [TestCase("d", "bit", "true")]
        public void CreateVar(string name, string dataType, string value)
        {
            inter.InterpretLine($"{name} as {dataType} closed = {value}", new List<string>{"testing"}, null);
            Assert.AreEqual(value, inter.InterpretLine(name, new List<string>{"testing"}, null));
        }

        [Test]
        public void FibTest()
        {
            inter.InterpretLine($"with '{new FileInfo(_prefix + "fib.hd").FullName}' as a", new List<string> {"testing"}, null);
            Assert.AreEqual("5",inter.InterpretLine("$a->test:[]", new List<string> { "testing" }, null));
        }

        [Test]
        [TestCase("Test","11")]
        [TestCase("18","9")]
        [TestCase("Hello world", "325")]
        public void GuardTest(string expected, string value)
        {
            inter.InterpretLine($"with '{new FileInfo(_prefix + "fib.hd").FullName}' as a", new List<string> { "testing" }, null);
            Assert.AreEqual(expected, inter.InterpretLine($"$a->t1:[{value}]", new List<string> { "testing" }, null));
        }

        [Test]
        public void CallTest()
        {
            inter.InterpretLine($"with \'{new FileInfo(_prefix + "iterate.hd").FullName}\' as a",new List<string>{"testing"},null);
            inter.InterpretLine($"with \'{new FileInfo(_prefix + "fibrec.hd").FullName}\' as b",new List<string>{"testing"},null);
            Assert.AreEqual("55",inter.InterpretLine("$a->test:[$b,10]", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("a", "num", "{1,2,3,4,5}", "4", "5")]
        [TestCase("b", "word", "{'Hello','Test','Wor,ld'}", "2", "'Wor,ld'")]
        [TestCase("c", "bit", "{true,false,true}", "1", "false")]
        [TestCase("d", "dec", "{1.3,0.9}", "1", "0.9")]
        public void ArrayTest(string name, string dataType, string values, string postion, string expected)
        {
            inter.InterpretLine($"{name} as {dataType}[*] closed = {values}", new List<string>{"testing"}, null);
            Assert.AreEqual(expected, inter.InterpretLine($"{name}[{postion}]", new List<string>{"testing"}, null));
        }

        [Test]
        public void ArrayAssign()
        {
            inter.InterpretLine("a as num[*] closed = {1,2,3,4,5}", new List<string>{"testing"}, null);
            inter.InterpretLine("b as num[*] closed", new List<string>{"testing"}, null);
            inter.InterpretLine("b = a", new List<string>{"testing"}, null);
            Assert.AreEqual("{1,2,3,4,5}", inter.InterpretLine("b", new List<string>{"testing"}, null));

        }

        [Test]
        public void InterpreterForcing()
        {
            inter.InterpretLine("a as word closed = 'out:b'", new List<string>{"testing"}, null);
            inter.InterpretLine("b as word closed = 'Hello world'", new List<string>{"testing"}, null);
            Assert.AreEqual("'Hello world'", inter.InterpretLine("#a", new List<string>{"testing"}, null));

        }

        [Test]
        public void ForceAssign()
        {
            inter.InterpretLine("a as word closed = 'out:b'", new List<string>{"testing"}, null);
            inter.InterpretLine("b as word closed = 'Hello world'", new List<string>{"testing"}, null);
            inter.InterpretLine("c as word closed = #a", new List<string>{"testing"}, null);
            Assert.AreEqual("'Hello world'", inter.InterpretLine("c", new List<string>{"testing"}, null));
        }

        [Test]
        public void FileTest()
        {
            Assert.AreEqual("23",inter.InterpretLine($"with \'{new FileInfo(_prefix + "fibrec.hd").FullName}\'", new List<string>{"testing"},null));
        }

        [Test]
        [TestCase("'NUM'", "32")]
        [TestCase("'BIT'", "true")]
        [TestCase("'DEC'", "3.2")]
        public void RawTest(string expected, string data)
        {
            Assert.AreEqual(expected, inter.InterpretLine($"out:[dtype:[raw:['{data}']]]", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("true", "sqrt(3) smallerIs 3")]
        [TestCase("false", "true imp false")]
        [TestCase("false", "(true and false) or (true and false)")]
        public void BoolTests(string expected, string calc)
        {
            Assert.AreEqual(expected, inter.InterpretLine(calc, new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("BIT", "true")]
        [TestCase("WORD", "'Hello world'")]
        [TestCase("DEC", "0.9")]
        [TestCase("NUM","22")]
        public void TypeTest(string expected, string calc)
        {
            Assert.AreEqual(expected, inter.InterpretLine($"dtype:{calc}", new List<string>{"testing"}, null));
        }

        [Test]
        public void VarAssign()
        {
            inter.InterpretLine("a as num closed = 1", new List<string>{"testing"}, null);
            inter.InterpretLine("b as num closed = 2", new List<string>{"testing"}, null);
            inter.InterpretLine("a = $a + $b", new List<string>{"testing"}, null);
            inter.InterpretLine("b = $a + $b", new List<string>{"testing"}, null);
            Assert.AreEqual("5", inter.InterpretLine("b", new List<string>{"testing"}, null));
        }

        [Test]
        public void ReflectionTest()
        {
            inter.InterpretLine("b as num", new List<string> {"testing"}, null);
            inter.InterpretLine("d as bit", new List<string> {"testing"}, null);
            inter.InterpretLine("c as word = 'HELLO'", new List<string> { "testing" }, null);
            inter.InterpretLine("a as word[*] = getfields:[]", new List<string> {"testing"}, null);
            inter.InterpretLine("f as word = #a[2]", new List<string> {"testing"}, null);
            Assert.AreEqual("'HELLO'", inter.InterpretLine("out:f", new List<string> { "testing" }, null));
        }

        [Test]
        [TestCase("'Hello'","'Hello'")]
        [TestCase("out:[dtype:12]","'NUM'")]
        public void StringCompTest(string a, string b)
        {
            Assert.AreEqual("true",inter.InterpretLine($"{a} is {b}", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("22")]
        [TestCase("22.4")]
        [TestCase("'Hello'")]
        [TestCase("true")]
        public void UntypedTest(string value)
        {
            inter.ShouldError(true);
            inter.InterpretLine("a as *", new List<string> {"testing"}, null);
            inter.InterpretLine($"a = {value}", new List<string> {"testing"}, null);
        }

        [Test]
        public void UntypedObject()
        {
            inter.InterpretLine("a as *", new List<string> {"testing"}, null);
            inter.InterpretLine($"with \'{new FileInfo(_prefix + "fibrec.hd").FullName}\' as b", new List<string> {"testing"}, null);
            inter.InterpretLine("a = b", new List<string> {"testing"}, null);
            Assert.IsTrue(inter.InterpretLine("a", new List<string> { "testing" }, null).StartsWith("obj"));
        }
    }
}
