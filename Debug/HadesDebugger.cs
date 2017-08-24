using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debug
{
    public class HadesDebugger
    {
        /// <summary>
        /// Lazy Store
        /// </summary>
        private static readonly Lazy<HadesDebugger> Lazy = new Lazy<HadesDebugger>(() => new HadesDebugger());
        /// <summary>
        /// Cache instance
        /// </summary>
        public static HadesDebugger EventManager => Lazy.Value;
        /// <summary>
        /// Method for interrupting the program
        /// </summary>
        public delegate bool Interrupt(int line, string varDump);
        /// <summary>
        /// Event to subscribe to
        /// </summary>
        public event Interrupt OnInterrupted;
        /// <summary>
        /// Prevents a default instance of the <see cref="HadesDebugger"/> class from being created.
        /// </summary>
        private HadesDebugger()
        {
        }
    }
}
