using System;
using System.Collections.Generic;

namespace Variables
{
    public class Function
    {
        public string Name;
        private readonly Func<IEnumerable<object>,string> _action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="action">Action to be executed on function call</param>
        public Function(string name, Func<IEnumerable<object>, string> action)
        {
            Name = name;
            _action = action;
        }

        public string Execute(IEnumerable<object> obj)
        {
            return _action(obj);
        }
    }
}