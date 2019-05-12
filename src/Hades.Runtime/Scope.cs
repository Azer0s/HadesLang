using System.Collections.Generic;
using Hades.Common;
using Hades.Syntax.Expression;

namespace Hades.Runtime
{
    public class Scope : ScopeValue
    {
        //Except for classes and structs, every variable is private
        public List<Scope> PrivateVariables { get; set; } = new List<Scope>();
        
        public List<Scope> ProtectedVariables { get; set; } = new List<Scope>();
        
        public List<Scope> PublicVariables { get; set; } = new List<Scope>();
        
        //A scope needs to have a datatype, the exec scope has the datatype "NONE"
        public Datatype Datatype { get; set; }
        
        //A scope can be an object, struct or proto, these three can have a specific type
        public string SpecificType { get; set; }
        
        //A scope can have multiple functions, a function can have multiple overloads
        public Dictionary<string,List<Scope>> Functions { get; set; } = new Dictionary<string,List<Scope>>();
        
        //A scope can contain classes
        public List<Scope> Classes { get; set; } = new List<Scope>();
        
        //A scope can have structs
        public List<Scope> Structs { get; set; } = new List<Scope>();
        
        //A scope can have code (is executable)
        public Node Code { get; set; }
        
        //A scope can be a variable, a variable can have the value of a scope (instance of an object), a proto (code) or a literal (ValueScope)
        public ScopeValue Value { get; set; }

        //A scope can be a variable, a variable can be nullable
        public bool Nullable { get; set; }

        //A scope can be a variable, a variable can be mutable
        public bool Mutable { get; set; }

        //A scope can be a variable, a variable can be dynamic
        public bool Dynamic { get; set; }
    }
}