using System.Collections.Generic;
using System.Linq;
using Hades.Common;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class TryCatchElseNode : Node
    {
        public struct CatchBlock
        {
            public BlockNode Block;
            public string SpecificType;
            public Datatype Datatype;
            public string Name;
        }
        
        public BlockNode Try { get; set; }
        public BlockNode Else { get; set; }
        public List<CatchBlock> Catch { get; } = new List<CatchBlock>();
        
        public TryCatchElseNode() : base(Classifier.TryCatch)
        {
        }

        protected override string ToStr()
        {
            var tryStr = "";
            foreach (var child in Try.Children)
            {
                tryStr += string.Join('\n', child.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
            }

            if (!string.IsNullOrEmpty(tryStr))
            {
                tryStr = $"\n Try:\n{tryStr.Substring(0, tryStr.Length - 1)}";
            }
            
            var catchString = string.Empty;

            foreach (var catchNode in Catch)
            {
                var specificType = catchNode.Name;
                
                if (catchNode.Datatype != Datatype.NONE)
                {
                    specificType = $"{catchNode.Datatype.ToString().ToLower()} {catchNode.Name}";
                }
                
                if (!string.IsNullOrEmpty(catchNode.SpecificType))
                {
                    specificType = $"{catchNode.Datatype.ToString().ToLower()}({catchNode.SpecificType}) {catchNode.Name}";
                }
                
                catchString += $"\n Catch {specificType}\n";
                
                foreach (var blockChild in catchNode.Block.Children)
                {
                    catchString += string.Join('\n', blockChild.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
                }

                catchString = catchString.TrimEnd('\n');
            }
            
            var elseStr = "";
            foreach (var child in Else.Children)
            {
                elseStr += string.Join('\n', child.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
            }

            if (!string.IsNullOrEmpty(elseStr))
            {
                elseStr = $"\n Else:\n{elseStr.Substring(0, elseStr.Length - 1)}";
            }
            
            return $"{tryStr}{catchString}{elseStr}";
        }
    }
}