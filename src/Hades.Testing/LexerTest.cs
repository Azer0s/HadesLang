using System;
using System.Linq;
using Hades.Language.Lexer;
using NUnit.Framework;

namespace Hades.Testing
{
    [TestFixture]
    public class LexerTest
    {
        private readonly Lexer _lexer = new Lexer();
        
        [Test]
        public void EnsureParseStringLiteral()
        {
            var results = _lexer.LexFile("\"Hello \\\"World\\\"\"").ToList();
            Assert.That(()=>results[0].Kind == Classifier.StringLiteral && results[1].Kind == Classifier.EndOfFile);
        }
        
        [Test]
        public void EnsureParseMultiLineStringLiteral()
        {
            var results = _lexer.LexFile("\"\"\"Hello \\\"World\\n\\t\\\"\"\"\"").ToList();
            Assert.That(()=>results[0].Kind == Classifier.MultiLineStringLiteral && results[1].Kind == Classifier.EndOfFile);
        }

        [Test]
        public void EnsureParseInt()
        {
            var results = _lexer.LexFile("40").ToList();
            Assert.That(()=>results[0].Kind == Classifier.IntLiteral && results[1].Kind == Classifier.EndOfFile);
        }
        
        [Test]
        public void EnsureParseDec()
        {
            var results = _lexer.LexFile("1.5").ToList();
            Assert.That(()=>results[0].Kind == Classifier.DecLiteral && results[1].Kind == Classifier.EndOfFile);
        }

        [Test]
        public void EnsureParseKeyword()
        {
            foreach (var keyword in Lexer.Keywords)
            {
                var results = _lexer.LexFile(keyword).ToList();
                Console.WriteLine(keyword);
                Assert.That(()=>results[0].Kind == Classifier.Keyword && results[1].Kind == Classifier.EndOfFile);
            }
        }

        [Test]
        public void EnsurePipelines()
        {
            var results = _lexer.LexFile("list->of({1,2,3,4,5})\n\t|> print").ToList();
            Assert.That(() => results.Count == 22);
        }
    }
}