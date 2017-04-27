using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Variables;

/// <summary>
/// Storage for shared runtime data
/// </summary>
public sealed class Cache
{
    /// <summary>
    /// Lazy Store
    /// </summary>
    private static readonly Lazy<Cache> Lazy = new Lazy<Cache>(() => new Cache());
    /// <summary>
    /// Cache instance
    /// </summary>
    public static Cache Instance => Lazy.Value;
    /// <summary>
    /// Var store
    /// </summary>
    public Dictionary<Tuple<string,string>, Types> Variables { get; set; }
    /// <summary>
    /// Determines whether the Hades garbage collector is enabled
    /// </summary>
    public bool EraseVars { get; set; } = true;
    /// <summary>
    /// List of loaded files 
    /// </summary>
    public List<string> LoadFiles { get; set; } = new List<string>();
    /// <summary>
    /// Singleton constructor
    /// </summary>
    private Cache()
    {
    }
}
