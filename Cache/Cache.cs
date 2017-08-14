using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    public Dictionary<Meta,IVariable> Variables { get; set; }
    /// <summary>
    /// Determines whether the Hades garbage collector is enabled
    /// </summary>
    public bool EraseVars { get; set; } = true;
    /// <summary>
    /// A list of custom functions
    /// </summary>
    public List<Function> Functions { get; set; } = new List<Function>();
    /// <summary>
    /// List of operators
    /// </summary>
    public List<string> CharList { get; set; } = new List<string> { "+", "-", "*", "/", "&&", "||", "-->", "~&&", "~||", "<--", "(+)", "-/>", "</-", "(", ")" };
    /// <summary>
    /// Dictionary for replacing operators as string to operator as sign
    /// </summary>
    public Dictionary<string, string> Replacement { get; set; } = new Dictionary<string, string>
    {
        {"AND","&"},
        {"OR","|"},
        {"IMP","-->"},
        {"NAND","~&"},
        {"NOR","~|"},
        {"CIMP","<--"},
        {"XOR","(+)" },
        {"NIMP","-/>" },
        {"CNIMP","</-" }
    };
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
