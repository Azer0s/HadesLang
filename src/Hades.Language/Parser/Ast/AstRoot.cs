using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hades.Language.Parser.Ast
{
    public class AstRoot : AstNode, IList<AstNode>
    {
        private readonly List<AstNode> _astNodes;
        public AstRoot() : base(Type.AstRoot)
        {
            _astNodes = new List<AstNode>();
        }

        protected override string DoToString()
        {
            return string.Join("", _astNodes.Select(a => $"\n  {a}"));
        }

        public IEnumerator<AstNode> GetEnumerator()
        {
            return _astNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(AstNode item)
        {
            _astNodes.Add(item);
        }

        public void Clear()
        {
            _astNodes.Clear();
        }

        public bool Contains(AstNode item)
        {
            return _astNodes.Contains(item);
        }

        public void CopyTo(AstNode[] array, int arrayIndex)
        {
            _astNodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(AstNode item)
        {
            return _astNodes.Remove(item);
        }
        
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public int Count { get; }
        public bool IsReadOnly { get; }
        // ReSharper restore UnassignedGetOnlyAutoProperty
        public int IndexOf(AstNode item)
        {
            return _astNodes.IndexOf(item);
        }

        public void Insert(int index, AstNode item)
        {
            _astNodes.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _astNodes.RemoveAt(index);
        }

        public AstNode this[int index]
        {
            get => _astNodes[index];
            set => _astNodes[index] = value;
        }
    }
}