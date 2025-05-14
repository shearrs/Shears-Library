using System;
using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents the level/severity of a <see cref="SHLog"/>.
    /// <para>
    /// <see cref="Log"/> => A normal log.<br/>
    /// <see cref="Verbose"/> => A log with extra information.<br/>
    /// <see cref="Warning"/> => A log representing a warning.<br/>
    /// <see cref="Error"/> => A log representing an error.<br/>
    /// <see cref="Fatal"/> => A log representing a fatal error.<br/> 
    /// </para>
    /// </summary>
    [Flags]
    public enum SHLogLevel 
    { 
        Log = 1, 
        Verbose = 2, 
        Warning = 4,
        Error = 8, 
        Fatal = 16
    }
}
