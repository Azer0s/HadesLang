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
    public Regex CreateVariable { get; set; } = new Regex("(\\w*) *as *(num|dec|word|bit) *(closed|reachable_all|reachable) *=? *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for assigning data to variables
    /// </summary>
    public Regex Assignment { get; set; } = new Regex("(\\w*) *= *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type word
    /// </summary>
    public Regex IsWord { get; set; } = new Regex("\'([\\w ]*)\'",RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type num
    /// </summary>
    public Regex IsNum { get; set; } = new Regex("(?<=\\s|^)\\d+(?=\\s|$)", RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type dec
    /// </summary>
    public Regex IsDec { get; set; } = new Regex("\\d*.\\d*",RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type bit
    /// </summary>
    public Regex IsBit { get; set; } = new Regex("true|false");
    /// <summary>
    /// Regex for including a library
    /// </summary>
    public Regex With { get; set; } = new Regex("with *\'(\\w*)\' *as *(\\w*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for matching built-in functions
    /// </summary>
    public Regex Function { get; set; } = new Regex("(\\w*):(?:\\[(.*)\\]|(.*))", RegexOptions.Compiled);
    /// <summary>
    /// Regex for matching method calls
    /// </summary>
    public Regex MethodCall { get; set; } = new Regex("\\$(\\w*) *-> *(\\w*):(?:\\[?(.*)\\]|(.*))",RegexOptions.Compiled);
    /// <summary>
    /// Regex for halting the program
    /// </summary>
    public Regex Exit { get; set; } = new Regex("exit:(\\d)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for input
    /// </summary>
    public Regex Input { get; set; } = new Regex("in:(\\w*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for output/conversion to word datatype
    /// </summary>
    public Regex Output { get; set; } = new Regex("out:(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for detecting variables
    /// </summary>
    public Regex Variable { get; set; } = new Regex("(\\$\\w*)",RegexOptions.Compiled);

    private RegexCollection()
    {
    }
}