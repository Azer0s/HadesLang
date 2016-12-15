using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Variables;

public sealed class Cache
{
    private static readonly Lazy<Cache> lazy =
    new Lazy<Cache>(() => new Cache());
    public static Cache Instance { get { return lazy.Value; } }
    public Dictionary<string, Types> Variables { get; set; }
    private Cache()
    {
    }
}
