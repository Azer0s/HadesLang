using System;
using System.Collections.Generic;

namespace Variables
{
    public class Function
    {
        public string Name;
        private readonly Action<IEnumerable<object>> _action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="action">Action to be executed on function call</param>
        public Function(string name, Action<IEnumerable<object>> action)
        {
            Name = name;
            _action = action;
        }

        public void Execute(IEnumerable<object> obj)
        {
            _action(obj);
        }
    }
}