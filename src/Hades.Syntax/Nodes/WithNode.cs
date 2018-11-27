// ReSharper disable MemberCanBePrivate.Global

using System;

namespace Hades.Syntax.Nodes
{
    public class WithNode : Node
    {
        public string Target { get; set;  } //package to import
        
        private string _source { get; set; }

        public string Source
        {
            set => _source = value;
            get => string.IsNullOrEmpty(_source) ? Target : _name;
        } //source

        public string NativePackage { get; set; } //source of the native package
        
        private string _name { get; set; }
        public string Name
        {
            set => _name = value;
            get => string.IsNullOrEmpty(_name) ? Target : _name;
        } //the name to be set
        
        public bool Native { get; set; } //for native imports like std libs
        public bool File { get; set;  } //when the statement is something like `with main.hd` or `with server`
        
        public WithNode() : base(Classifier.With){}

        public override string ToString()
        {           
            if (File)
            {
                return $"Import {Target} as {Name}";
            }

            return Native ? $"Import {Target} from native package {NativePackage}:{Source} as {Name}" : $"Import {Target} from {Source} as {Name}";
        }
    }
}