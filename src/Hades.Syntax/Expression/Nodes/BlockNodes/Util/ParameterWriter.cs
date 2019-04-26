using System.Collections.Generic;
using Hades.Common;

namespace Hades.Syntax.Expression.Nodes.BlockNodes.Util
{
    public static class ParameterWriter
    {
        public static string PrintParameters(IEnumerable<(Node Key, Datatype? Value, string SpecificType)> parameters)
        {
            var args = "";

            foreach (var parameter in parameters)
            {
                if (parameter.Value != null)
                {
                    if (parameter.SpecificType != null)
                    {
                        args += $"(({parameter.Key}):{parameter.Value.ToString().ToLower()}[{parameter.SpecificType}]),";
                    }
                    else
                    {
                        args += $"(({parameter.Key}):{parameter.Value.ToString().ToLower()}),";
                    }
                }
                else
                {
                    args += $"({parameter.Key}),";
                }
            }

            return args;
        }
    }
}