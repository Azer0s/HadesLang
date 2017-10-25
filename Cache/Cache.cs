using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Gets or sets the library location
    /// </summary>
    public string LibraryLocation { get; set; }
    /// <summary>
    /// A list of custom functions
    /// </summary>
    public List<Function> Functions { get; set; } = new List<Function>();
    /// <summary>
    /// Cache for objects as return type
    /// </summary>
    public Dictionary<string,IVariable> FileCache { get; set; } = new Dictionary<string, IVariable>();
    /// <summary>
    /// List of operators
    /// </summary>
    public List<string> CharList { get; set; } = new List<string> { "+", "-", "*", "/", "^","%","!","&&", "||", "-->", "~&&", "~||", "<--", "(+)", "-/>", "</-", "(", ")", "sin", "cos","tan","sqrt","integ"};
    /// <summary>
    /// Dictionary for replacing operators as string to operator as sign
    /// </summary>
    public Dictionary<string, string> Replacement { get; set; } = new Dictionary<string, string>
    {
        {"AND","&"},
        {"@AND","@&"},
        {"OR","|"},
        {"@OR","@|"},
        {"IMP","-->"},
        {"NAND","~&"},
        {"NOR","~|"},
        {"INV","~" },
        {"CIMP","<--"},
        {"XOR","(+)" },
        {"@XOR","@^"},
        {"NIMP","-/>" },
        {"CNIMP","</-" },
        {"SMALLER","<"},
        {"BIGGER",">" },
        {"SMALLERIS","<="},
        {"BIGGERIS",">="},
        {"IS","=="},
        {"NOT","!="},
        {"LSHIFT","@<<"},
        {"RSHIFT","@>>"}
    };

    /// <summary>
    /// Dictionary for replacing operators as string to operator as sign (without binary operators)
    /// </summary>
    public Dictionary<string, string> ReplacementWithoutBinary { get; set; }
    /// <summary>
    /// List of loaded files 
    /// </summary>
    public List<string> LoadFiles { get; set; } = new List<string>();
    /// <summary>
    /// Cache for calculations
    /// </summary>
    public Dictionary<string, string> CachedCalculations { get; set; } = new Dictionary<string, string>();
    /// <summary>
    /// Delimiter wether to cache calculations
    /// </summary>
    public bool CacheCalculation { get; set; } = true;
    /// <summary>
    /// Toggle for debug
    /// </summary>
    public bool Debug { get; set; } = false;
    /// <summary>
    /// Alias entries
    /// </summary>
    public Dictionary<string,string> Alias { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Singleton constructor
    /// </summary>
    private Cache()
    {
    }
}
