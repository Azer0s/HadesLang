using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hades.Variables
{
    public class Function
    {
        public string Name;
        private readonly Action<IEnumerable<Methods>> _action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="action">Action to be executed on function call</param>
        public Function(string name, Action<IEnumerable<Methods>> action)
        {
            Name = name;
            _action = action;
        }

        public void Execute(IEnumerable<Methods> obj)
        {
            _action(obj);
        }
    }
}