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
    public Regex ScriptOutput { get; set; } = new Regex("^scriptOutput:(\\d)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for ignoring tabs and spaces in file
    /// </summary>
    public Regex IgnoreTabsAndSpaces { get; set; } = new Regex("^[ \\t]*(.*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for toggling calculation cache
    /// </summary>
    public Regex CacheCalculations { get; set; } = new Regex("^cacheCalculations:(\\d)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for arguments in function decleration
    /// </summary>
    public Regex Argument { get; set; } = new Regex("^(word|num|dec|bit) +([\\w]*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for dumping variables
    /// </summary>
    public Regex DumpVars { get; set; } = new Regex("^dumpVars:(all|num|dec|word|bit)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for creating new variables
    /// </summary>
    public Regex CreateVariable { get; set; } = new Regex("(\\w*) *as *(num|dec|word|bit) *(closed|reachable_all|reachable) *=? *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for creating new arrays
    /// </summary>
    public Regex CreateArray { get; set; } = new Regex("(\\w*) *as *(num|dec|word|bit)\\[([\\d]*|\\*)\\] *(closed|reachable_all|reachable) *=? *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for assigning data to variables
    /// </summary>
    public Regex Assignment { get; set; } = new Regex("^(\\w*) *= *(.*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for assigning data to variables with an operation
    /// </summary>
    public Regex OpAssignment { get; set; } = new Regex("^(\\w*) *(\\+|-|\\*|\\/)= *(.*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for increase/decrease by one
    /// </summary>
    public Regex InDeCrease { get; set; } = new Regex("^(\\w*)(?:-|\\+)(-|\\+)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for assigning data to specific positions in an array
    /// </summary>
    public Regex ArrayAssignment { get; set; } = new Regex("(\\w*)\\[(.*)\\] *= *(.*)",RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type word
    /// </summary>
    public Regex IsWord { get; set; } = new Regex("\'([\\w ]*)\'",RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type num
    /// </summary>
    public Regex IsNum { get; set; } = new Regex("(?<=\\s|^)-?\\d+(?=\\s|$)", RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type dec
    /// </summary>
    public Regex IsDec { get; set; } = new Regex("^-?\\d*\\.\\d*", RegexOptions.Compiled);
    /// <summary>
    /// Checks if a string contains or is of type bit
    /// </summary>
    public Regex IsBit { get; set; } = new Regex("^true|false",RegexOptions.Compiled);
    /// <summary>
    /// Regex for including a library
    /// </summary>
    public Regex With { get; set; } = new Regex("^with *\'(\\w*)\' *as *(\\w*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for matching built-in functions
    /// </summary>
    public Regex Function { get; set; } = new Regex("^(\\w*):(?:\\[(.*)\\]|(.*))", RegexOptions.Compiled);
    /// <summary>
    /// Function decleration
    /// </summary>
    public Regex FunctionDecleration { get; set; } = new Regex("^func *(\\w*) *\\[(.*)\\]",RegexOptions.Compiled);
    /// <summary>
    /// Case statement
    /// </summary>
    public Regex Case { get; set; } = new Regex("^case *\\[(.*)\\]",RegexOptions.Compiled);
    /// <summary>
    /// Loop
    /// </summary>
    public Regex AsLongAs { get; set; } = new Regex("^asLongAs *\\[(.*)\\]", RegexOptions.Compiled);
    /// <summary>
    /// Return
    /// </summary>
    public Regex Put { get; set; } = new Regex("^put +(.*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for matching method calls
    /// </summary>
    public Regex MethodCall { get; set; } = new Regex("^\\$(\\w*) *-> *(\\w*):(?:\\[?(.*)\\]|(.*))",RegexOptions.Compiled);
    /// <summary>
    /// Regex for getting var from object
    /// </summary>
    public Regex VarCall { get; set; } = new Regex("^\\$(\\w*) *-> *(\\w*)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for halting the program
    /// </summary>
    public Regex Exit { get; set; } = new Regex("^exit:(\\d)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for input
    /// </summary>
    public Regex Input { get; set; } = new Regex("^in:(\\w*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for output/conversion to word datatype
    /// </summary>
    public Regex Output { get; set; } = new Regex("^out:(?:\\[(.*)\\]|(.*))", RegexOptions.Compiled);
    /// <summary>
    /// Regex for detecting variables
    /// </summary>
    public Regex Variable { get; set; } = new Regex("(\\$\\w*)\\[?\\d*\\]?", RegexOptions.Compiled);
    /// <summary>
    /// Regex for detecting array values
    /// </summary>
    public Regex ArrayValues { get; set; } = new Regex("{(.*)}",RegexOptions.Compiled);
    /// <summary>
    /// Regex for detecting single varnames
    /// </summary>
    public Regex SingleName { get; set; } = new Regex("^[a-zA-Z]*$",RegexOptions.Compiled);
    /// <summary>
    /// Regex for uload keyword
    /// </summary>
    public Regex Unload { get; set; } = new Regex("^uload:(\\w*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for exists keyword
    /// </summary>
    public Regex Exists { get; set; } = new Regex("^exists:(\\w*)",RegexOptions.Compiled);
    /// <summary>
    /// Regex for type keyword
    /// </summary>
    public Regex Type { get; set; } = new Regex("^d?type:([\\w' .]*)", RegexOptions.Compiled);
    /// <summary>
    /// Load file
    /// </summary>
    public Regex Load { get; set; } = new Regex("^load:'([\\w\\.]*)'", RegexOptions.Compiled);
    /// <summary>
    /// Regex for toggling the Hades garbage collector
    /// </summary>
    public Regex EraseVars { get; set; } = new Regex("^eraseVars:(\\d)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for toggling the Hades debugger
    /// </summary>
    public Regex Debug { get; set; } = new Regex("^debug:(\\d)", RegexOptions.Compiled);
    /// <summary>
    /// Regex for random number function
    /// </summary>
    public Regex RandomNum { get; set; } = new Regex("^rand:\\[? *(\\d*) *\\]?", RegexOptions.Compiled);
    /// <summary>
    /// Regex for array access
    /// </summary>
    public Regex ArrayVariable { get; set; } = new Regex("(\\$\\w*)\\[(.*)\\]",RegexOptions.Compiled);
    /// <summary>
    /// Regex for forcing var values to interpreter
    /// </summary>
    public Regex ForceThrough { get; set; } = new Regex("^#(.*)",RegexOptions.Compiled);
    /// <summary>
    /// End of block
    /// </summary>
    public Regex End { get; set; } = new Regex("^end(case|aslongas)",RegexOptions.Compiled);
    /// <summary>
    /// Values outside of function call
    /// </summary>
    public Regex Outside { get; set; } = new Regex("\\[.*?\\]",RegexOptions.Compiled);

    /// <summary>
    /// Prevents a default instance of the <see cref="RegexCollection"/> class from being created.
    /// </summary>
    private RegexCollection()
    {
    }
}