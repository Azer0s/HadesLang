using System;
using Hades.Common;
using Hades.Source;

namespace Hades.Syntax
{
    public abstract class Node
    {
        public Classifier Classifier { get;  }
        
        public Span Span { get; }

        protected Node(Span span, Classifier classifier)
        {
            Classifier = classifier;
            Span = span;
        }
    }
}