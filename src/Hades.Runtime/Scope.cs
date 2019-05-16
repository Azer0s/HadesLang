using System;
using System.Collections.Generic;
using Hades.Common;
using Hades.Syntax.Expression;

namespace Hades.Runtime
{
    public class Scope : ScopeValue
    {
        //Except for classes and structs, every variable is private
        //I figured this is the best way to store variables
        //A variable needs to be identified, primarily by its name
        //A variable has an access modifier (in every scope - even in scopes where you can't set an access modifier -> then it's private)
        public Dictionary<string, (Scope Value, AccessModifier AccessModifier)> Variables { get; set; } = new Dictionary<string, (Scope Value, AccessModifier AccessModifier)>();
        
        //A scope needs to have a datatype, the exec scope has the datatype "NONE"
        public Datatype? Datatype { get; set; }
        
        //Classes, structs, variables and protos have names
        //Lambdas and functions also have names
        //When a function calls another function and the runtime can't find the other function
        //(For whatever reason)
        //This is the method of last resort (the runtime checks if the function that needs to be invoked is maybe this very function itself)
        //Note: I'll probably look at the parent scope for overloads first, if no overloads fits the invocation, this has to be the function to call
        //If it's still not the function to call -> ¯\_(ツ)_/¯
        public string Name { get; set; }
        
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
        
        //I know...it's a bit weird that this is here...but tbh...I didn't have the nerve to deal with polymorphism in this
        //Like...for real...none whatsoever
        //Not doing it
        //Nuh uh
        //Also polymorphism sux and makes everything slower. Only reason I used it in the parser is because there are *too fucking many* attributes to keep track of
        public bool IsNativeFunction { get; set; }
        
        //TODO: Native function signature; I need to, somehow, expose the function signature so I can check if the user is not being a stupid asshat
        //TODO: The goal of this is preventing stupid asshatery

        //Honestly...this is actually not a bad solution
        //No CLR reflection, no weird casting shit
        //Just plain old func
        //We stan a simple queen
        public Func<Scope[], Scope /*This is this. You are getting "this". @Future Ari: You need this. Don't fucking dare to remove or question this. This is literally *this* */, Scope> NativeFunction { get; set; }
    }
}