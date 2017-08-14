using System;
using System.Text.RegularExpressions;

public class RegexCollection
{
    /// <summary>
    /// Lazy Store
    /// </summary>
    private static readonly Lazy<RegexCollection> Lazy = new Lazy<RegexCollection>(() => new RegexCollection());
    /// <summary>
    /// Cache instance
    /// </summary>
    public static RegexCollection Store => Lazy.Value;
    /// <summary>
    /// Regex for toggling scriptOutput
    /// </summary>
    public Regex ScriptOutput { get; set; } = new Regex("scriptOutput:(\\d)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for dumping variables
    /// </summary>
    public Regex DumpVars { get; set; } = new Regex("dumpVars:(all|num|dec|word|bit)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for creating new variables
    /// </summary>
    public Regex Variables { get; set; } = new Regex("(\\w*) *as *(num|dec|word|bit) *(closed|reachable_all|reachable) *=? *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for assigning data to variables
    /// </summary>
    public Regex Assignment { get; set; } = new Regex("(\\w*) *= *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for including a library
    /// </summary>
    public Regex With { get; set; } = new Regex("with *\'(\\w*)\' *as *(\\w*)", RegexOptions.Compiled);

    private RegexCollection()
    {
    }
}