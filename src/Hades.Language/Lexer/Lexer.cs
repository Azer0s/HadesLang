// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hades.Error;
using Hades.Source;

namespace Hades.Language
{
    public class Lexer
    {
        #region Keywords

        private static readonly string[] _blockKeywords =
        {
            "class",
            "func",
            "requires",
            "if",
            "else",
            "while",
            "for",
            "in",
            "stop",
            "skip",
            "try",
            "catch",
            "default",
            "end"
        };

        private static readonly string[] _varKeywords =
        {
            "var",
            "let",
            "null",
            "undefined",
            "be",
            "is"
        };

        private static readonly string[] _accessModifierKeywords =
        {
            "global",
            "public",
            "private"
        };

        private static readonly string[] _comparisonKeywords =
        {
            "equals",
            "not",
            "and",
            "or" 
        };
        
        private static readonly string[] _importKeywords =
        {
            "with",
            "from",
            "as"
        };

        private static readonly string[] _miscKeywords =
        {
            "put"
        };
        
        private static List<string> _keywordList = new List<string>();

        private static List<string> GetKeywordList()
        {
            if (_keywordList.Count != 0) return _keywordList;
            
            var list = _blockKeywords.ToList();
            list.AddRange(_varKeywords.ToList());
            list.AddRange(_accessModifierKeywords.ToList());
            list.AddRange(_comparisonKeywords.ToList());
            list.AddRange(_importKeywords.ToList());
            list.AddRange(_miscKeywords.ToList());

            _keywordList = list;
            return _keywordList;
        }

        private static List<string> _keywords => GetKeywordList();

        #endregion

        private StringBuilder _builder;
        private int _column;
        private int _index;
        private int _line;
        private Code _sourceCode;
        private Location _tokenStart;
        private char _ch => _sourceCode[_index];
        private char _last => Peek(-1);
        private char _next => Peek(1);
        
        public Collector Collector { get; }
        
        public Lexer(): this(new Collector()){}

        public Lexer(Collector collector)
        {
            _builder = new StringBuilder();
            _sourceCode = null;
            Collector = collector;
        }
        
        private char Peek(int ahead)
        {
            return _sourceCode[_index + ahead];
        }
    }
}