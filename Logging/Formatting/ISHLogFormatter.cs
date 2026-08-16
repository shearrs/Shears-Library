using System.Runtime.CompilerServices;
using UnityEngine;

namespace Shears.Logging
{
    public interface ISHLogFormatter
    {
        /// <summary>
        /// Checks whether or not this implementation of <see cref="ISHLogFormatter"/> is valid.
        /// </summary>
        /// <returns>Whether or not this formatter is valid.</returns>
        public bool IsValid();

        /// <summary>
        /// Formats a message from a passed <see cref="SHLog"/>.
        /// </summary>
        /// <param name="log">The log to format a message from.</param>
        /// <returns>A formatted log message.</returns>
        public string Format(in SHLog log);
    }
}
