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

        [Test]
        public void AddTest()
        {
            Assert.AreEqual("15",_interpreter.InterpretLine("10+5","testing"));
        }

        [Test]
        public void ConcatTest()
        {
            //Assert.AreEqual("Hello world",_interpreter.InterpretLine("'Hello ' + 'world'","testing").Message);
        }

        [Test]
        [TestCase("a","word","'Hello world'")]
        [TestCase("b", "num","15")]
        [TestCase("c", "dec","32.5")]
        [TestCase("d", "bit","true")]
        public void CreateVar(string name,string dataType,string value)
        {
            _interpreter.InterpretLine($"{name} as {dataType} closed = {value}", "testing");
            Assert.AreEqual(value,_interpreter.InterpretLine(name,"testing"));
            _interpreter.InterpretLine($"uload:{name}", "testing");
        }

        [Test]
        [TestCase("a","num","{1,2,3,4,5}","4","5")]
        [TestCase("b", "word", "{'Hello','Test','Wor,ld'}", "2", "'Wor,ld'")]
        [TestCase("c", "bit", "{true,false,true}", "1", "false")]
        [TestCase("d", "dec", "{1.3,0.9}", "1", "0.9")]
        public void ArrayTest(string name, string dataType, string values, string postion, string expected)
        {
            _interpreter.InterpretLine($"{name} as {dataType}[*] closed = {values}", "testing");
            Assert.AreEqual(expected, _interpreter.InterpretLine($"{name}[{postion}]", "testing"));
            _interpreter.InterpretLine($"uload:{name}", "testing");
        }

        [Test]
        public void ArrayAssign()
        {
            _interpreter.InterpretLine("a as num[*] closed = {1,2,3,4,5}", "testing");
            _interpreter.InterpretLine("b as num[*] closed", "testing");
            _interpreter.InterpretLine("b = a", "testing");
            Assert.AreEqual("{1,2,3,4,5}",_interpreter.InterpretLine("b","testing"));
            _interpreter.InterpretLine("uload:a", "testing");
            _interpreter.InterpretLine("uload:b", "testing");
        }

        [Test]
        public void InterpreterForcing()
        {
            _interpreter.InterpretLine("a as word closed = 'out:b'", "testing");
            _interpreter.InterpretLine("b as word closed = 'Hello world'", "testing");
            Assert.AreEqual("'Hello world'",_interpreter.InterpretLine("#a","testing"));
            _interpreter.InterpretLine("uload:a", "testing");
            _interpreter.InterpretLine("uload:b", "testing");
        }

        [Test]
        public void ForceAssign()
        {
            _interpreter.InterpretLine("a as word closed = 'out:b'", "testing");
            _interpreter.InterpretLine("b as word closed = 'Hello world'", "testing");
            _interpreter.InterpretLine("c as word closed = #a", "testing");
            Assert.AreEqual("'Hello world'", _interpreter.InterpretLine("c", "testing"));
            _interpreter.InterpretLine("uload:a", "testing");
            _interpreter.InterpretLine("uload:b", "testing");
            _interpreter.InterpretLine("uload:c", "testing");
        }

        [Test]
        [TestCase("true", "sqrt(3) smallerIs 3")]
        [TestCase("false", "true imp false")]
        [TestCase("false", "(true and false) or (true and false)")]
        public void BoolTests(string expected, string calc)
        {
            Assert.AreEqual(expected,_interpreter.InterpretLine(calc,"testing"));
        }

        [Test]
        [TestCase("BIT", "true")]
        [TestCase("WORD", "'Hello world'")]
        [TestCase("DEC", "0.9")]
        public void TypeTest(string expected, string calc)
        {
            Assert.AreEqual(expected, _interpreter.InterpretLine($"dtype:{calc}", "testing"));
        }

        [Test]
        public void VarAssign()
        {
            _interpreter.InterpretLine("a as num closed = 1", "testing");
            _interpreter.InterpretLine("b as num closed = 2", "testing");
            _interpreter.InterpretLine("a = $a + $b", "testing");
            _interpreter.InterpretLine("b = $a + $b", "testing");
            Assert.AreEqual("5", _interpreter.InterpretLine("b", "testing"));
            _interpreter.InterpretLine("uload:b", "testing");
            _interpreter.InterpretLine("uload:c", "testing");
        }
    }
}
