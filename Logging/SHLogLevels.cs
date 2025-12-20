using System;
using UnityEngine;

namespace Shears.Logging
{
    public static class SHLogUtil
    {
        public static readonly SHLogLevels Nothing = 0;
        public static readonly SHLogLevels Everything = (SHLogLevels)(-1);
        public static readonly SHLogLevels Issues = SHLogLevels.Warning | SHLogLevels.Error | SHLogLevels.Fatal;
        public static readonly SHLogLevels Default = SHLogLevels.Log | Issues;
    }

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
    public enum SHLogLevels 
    { 
        Log = 1, 
        Verbose = 2, 
        Warning = 4,
        Error = 8, 
        Fatal = 16,
    }
}
