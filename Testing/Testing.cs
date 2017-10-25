using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using Output;

namespace Testing
{
    [TestFixture]
    public class Testingcs
    {
        private readonly Interpreter.Interpreter _interpreter = new Interpreter.Interpreter(new ConsoleOutput(), new ConsoleOutput());
        private string prefix = "C:\\Users\\ariel\\workspace\\HadesLang\\HadesLang\\";

        [TearDown]
        public void TearDown()
        {
            _interpreter.InterpretLine("dumpVars:all", new List<string> {"testing"}, null, true);
            _interpreter.InterpretLine("uload:all", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("dumpVars:all", new List<string> { "testing" }, null, true);
        }

        [Test]
        public void AddTest()
        {
            Assert.AreEqual("15", _interpreter.InterpretLine("10+5", new List<string>{"testing"}, null));
        }

        [Test]
        public void ConcatTest()
        {
            Assert.AreEqual("'Hello world'",_interpreter.InterpretLine("'Hello' + ' ' + 'world'", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("a", "word", "'Hello world'")]
        [TestCase("b", "num", "15")]
        [TestCase("c", "dec", "32.5")]
        [TestCase("d", "bit", "true")]
        public void CreateVar(string name, string dataType, string value)
        {
            _interpreter.InterpretLine($"{name} as {dataType} closed = {value}", new List<string>{"testing"}, null);
            Assert.AreEqual(value, _interpreter.InterpretLine(name, new List<string>{"testing"}, null));
            _interpreter.InterpretLine($"uload:{name}", new List<string>{"testing"}, null);
        }

        [Test]
        public void CallTest()
        {
            _interpreter.InterpretLine($"with \'{prefix}iterate.hades\' as a",new List<string>{"testing"},null);
            _interpreter.InterpretLine($"with \'{prefix}fibrec.hades\' as b",new List<string>{"testing"},null);
            Assert.AreEqual("55",_interpreter.InterpretLine("$a->test:[$b,10]", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("a", "num", "{1,2,3,4,5}", "4", "5")]
        [TestCase("b", "word", "{'Hello','Test','Wor,ld'}", "2", "'Wor,ld'")]
        [TestCase("c", "bit", "{true,false,true}", "1", "false")]
        [TestCase("d", "dec", "{1.3,0.9}", "1", "0.9")]
        public void ArrayTest(string name, string dataType, string values, string postion, string expected)
        {
            _interpreter.InterpretLine($"{name} as {dataType}[*] closed = {values}", new List<string>{"testing"}, null);
            Assert.AreEqual(expected, _interpreter.InterpretLine($"{name}[{postion}]", new List<string>{"testing"}, null));
            _interpreter.InterpretLine($"uload:{name}", new List<string>{"testing"}, null);
        }

        [Test]
        public void ArrayAssign()
        {
            _interpreter.InterpretLine("a as num[*] closed = {1,2,3,4,5}", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b as num[*] closed", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b = a", new List<string>{"testing"}, null);
            Assert.AreEqual("{1,2,3,4,5}", _interpreter.InterpretLine("b", new List<string>{"testing"}, null));
            _interpreter.InterpretLine("uload:a", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:b", new List<string>{"testing"}, null);
        }

        [Test]
        public void InterpreterForcing()
        {
            _interpreter.InterpretLine("a as word closed = 'out:b'", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b as word closed = 'Hello world'", new List<string>{"testing"}, null);
            Assert.AreEqual("'Hello world'", _interpreter.InterpretLine("#a", new List<string>{"testing"}, null));
            _interpreter.InterpretLine("uload:a", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:b", new List<string>{"testing"}, null);
        }

        [Test]
        public void ForceAssign()
        {
            _interpreter.InterpretLine("a as word closed = 'out:b'", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b as word closed = 'Hello world'", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("c as word closed = #a", new List<string>{"testing"}, null);
            Assert.AreEqual("'Hello world'", _interpreter.InterpretLine("c", new List<string>{"testing"}, null));
            _interpreter.InterpretLine("uload:a", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:b", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:c", new List<string>{"testing"}, null);
        }

        [Test]
        public void FileTest()
        {
            Assert.AreEqual("23",_interpreter.InterpretLine($"with \'{prefix}fibrec.hades\'", new List<string>{"testing"},null));
        }

        [Test]
        [TestCase("'NUM'", "32")]
        [TestCase("'BIT'", "true")]
        [TestCase("'DEC'", "3.2")]
        public void RawTest(string expected, string data)
        {
            Assert.AreEqual(expected, _interpreter.InterpretLine($"out:[dtype:[raw:['{data}']]]", new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("true", "sqrt(3) smallerIs 3")]
        [TestCase("false", "true imp false")]
        [TestCase("false", "(true and false) or (true and false)")]
        public void BoolTests(string expected, string calc)
        {
            Assert.AreEqual(expected, _interpreter.InterpretLine(calc, new List<string>{"testing"}, null));
        }

        [Test]
        [TestCase("BIT", "true")]
        [TestCase("WORD", "'Hello world'")]
        [TestCase("DEC", "0.9")]
        public void TypeTest(string expected, string calc)
        {
            Assert.AreEqual(expected, _interpreter.InterpretLine($"dtype:{calc}", new List<string>{"testing"}, null));
        }

        [Test]
        public void VarAssign()
        {
            _interpreter.InterpretLine("a as num closed = 1", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b as num closed = 2", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("a = $a + $b", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("b = $a + $b", new List<string>{"testing"}, null);
            Assert.AreEqual("5", _interpreter.InterpretLine("b", new List<string>{"testing"}, null));
            _interpreter.InterpretLine("uload:a", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:b", new List<string>{"testing"}, null);
            _interpreter.InterpretLine("uload:c", new List<string>{"testing"}, null);
        }

        [Test]
        [TestCase("'Hello'","'Hello'")]
        [TestCase("out:[dtype:12]","'NUM'")]
        public void StringCompTest(string a, string b)
        {
            Assert.AreEqual("true",_interpreter.InterpretLine($"{a} is {b}", new List<string>{"testing"}, null));
        }
    }
}
