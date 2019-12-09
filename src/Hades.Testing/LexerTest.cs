using System.Collections.Generic;
using Hades.Language.Lexer;
using NUnit.Framework;
// ReSharper disable NUnit.IncorrectArgumentType

namespace Hades.Testing
{
    [TestFixture]
    public class LexerTest
    {
        [TestCase("a = 10", Type.Identifier, Type.Assign, Type.Integer)]
        [TestCase("a == 10", Type.Identifier, Type.Equals, Type.Integer)]
        [TestCase("a is 10", Type.Identifier, Type.Equals, Type.Integer)]
        [TestCase("a != 10", Type.Identifier, Type.NotEquals, Type.Integer)]
        [TestCase("a not 10", Type.Identifier, Type.NotEquals, Type.Integer)]
        [TestCase("true && false", Type.Bool, Type.LogicalAnd, Type.Bool)]
        [TestCase("true and false", Type.Bool, Type.LogicalAnd, Type.Bool)]
        [TestCase("true || false", Type.Bool, Type.LogicalOr, Type.Bool)]
        [TestCase("true or false", Type.Bool, Type.LogicalOr, Type.Bool)]
        [TestCase("1 < 2", Type.Integer, Type.Smaller, Type.Integer)]
        [TestCase("2 > 1", Type.Integer, Type.Bigger, Type.Integer)]
        [TestCase("i++", Type.Identifier, Type.Increment)]
        [TestCase("i--", Type.Identifier, Type.Decrement)]
        [TestCase("i += 1.123", Type.Identifier, Type.CompoundIncrement, Type.Float)]
        [TestCase("i -= 1", Type.Identifier, Type.CompoundDecrement, Type.Integer)]
        [TestCase("i *= 2", Type.Identifier, Type.CompoundMultiplication, Type.Integer)]
        [TestCase("i /= 2", Type.Identifier, Type.CompoundDivision, Type.Integer)]
        [TestCase("i %= 2", Type.Identifier, Type.CompoundMod, Type.Integer)]
        [TestCase("i <<= 4", Type.Identifier, Type.CompoundLeftShift, Type.Integer)]
        [TestCase("i >>= 4", Type.Identifier, Type.CompoundRightShift, Type.Integer)]
        [TestCase("i &= 8", Type.Identifier, Type.CompoundBitwiseAnd, Type.Integer)]
        [TestCase("i |= 4", Type.Identifier, Type.CompoundBitwiseOr, Type.Integer)]
        [TestCase("i ^= 16", Type.Identifier, Type.CompoundBitwiseXor, Type.Integer)]
        [TestCase("1 + 1", Type.Integer, Type.Plus, Type.Integer)]
        [TestCase("1 - 1", Type.Integer, Type.Minus, Type.Integer)]
        [TestCase("1 * 1", Type.Integer, Type.Multiplication, Type.Integer)]
        [TestCase("1 / 1", Type.Integer, Type.Division, Type.Integer)]
        [TestCase("4 % 2", Type.Integer, Type.Mod, Type.Integer)]
        [TestCase("4 << 2", Type.Integer, Type.LeftShift, Type.Integer)]
        [TestCase("4 >> 2", Type.Integer, Type.RightShift, Type.Integer)]
        [TestCase("4 & 2", Type.Integer, Type.BitwiseAnd, Type.Integer)]
        [TestCase("4 | 2", Type.Integer, Type.BitwiseOr, Type.Integer)]
        [TestCase("4 ^ 2", Type.Integer, Type.BitwiseXor, Type.Integer)]
        [TestCase("~0", Type.BitwiseNegate, Type.Integer)]
        [TestCase("msg := {:ok, val}", 
            Type.Identifier,Type.MatchAssign, Type.OpenBrace, Type.Atom, Type.Comma, Type.Identifier, Type.ClosedBrace)]
        [TestCase("!true", Type.ExclamationMark, Type.Bool)]
        [Test]
        public void TestOperators(string code, params Type[] types)
        {
            var tokens = Lexer.Lex(code);
            Assert.AreEqual(types.Length, tokens.Count);
            
            for (var i = 0; i < types.Length; i++)
            {
                Assert.AreEqual(tokens[i].Type, types[i]);
            }
        }
    }
}