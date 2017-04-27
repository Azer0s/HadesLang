using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Variables;

namespace CustomFunctions
{
    public class Function
    {
        public string Name;
        private readonly Action _action;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="action">Action to be executed on function call</param>
        public Function(string name, Action action)
        {
            Name = name;
            _action = action;
        }

        public void Execute()
        {
            var t = new Task(_action);
            t.RunSynchronously();
        }
    }
}
