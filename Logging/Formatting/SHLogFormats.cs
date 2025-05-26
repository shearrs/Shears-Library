using System;
using UnityEngine;
using static Shears.Logging.SHLogFormatter;

namespace Shears.Logging
{
    /// <summary>
    /// A static collection of various pre-made <see cref="SHLogFormatter"/>s.
    /// </summary>
    public static class SHLogFormats
    {
        #region Static Defaults
        /// <summary>
        /// The default log formatter.
        /// <para>
        ///     <example>
        ///     <c>SHLogger.Log("Hello World!", SHLogFormats.Default);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static SHLogFormatter Default => new(DefaultPrefix);

        /// <summary>
        /// The default log formatter with a time stamp in the prefix.
        /// <para>
        ///     <example>
        ///     <c>SHLogger.Log("Hello World!", SHLogFormats.DefaultWithTimestamp);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Hour:Minute:Second] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static SHLogFormatter DefaultWithTimestamp => new(CombinePrefixes(" ", TimestampPrefix, DefaultPrefix));

        /// <summary>
        /// The default log formatter with a long time stamp in the prefix.
        /// <para>
        ///     <example>
        ///     <c>SHLogger.Log("Hello World!", SHLogFormats.DefaultWithLongTimestamp);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Day/Month - Hour:Minute:Second] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static SHLogFormatter DefaultWithLongTimestamp => new(CombinePrefixes(" ", LongTimestampPrefix, DefaultPrefix));

        /// <summary>
        /// The default log formatter with the context name in the prefix.
        /// <para>
        ///     <example>
        ///     <c>SHLogger.Log("Hello World!", SHLogFormats.DefaultWithContext);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Context: {contextName}] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static SHLogFormatter DefaultWithContext => new(CombinePrefixes(" ", ContextPrefix, DefaultPrefix));

        /// <summary>
        /// A formatter that outputs a raw message.
        /// <para>
        ///     <example>
        ///     <c>SHLogger.Log("Hello World!", SHLogFormats.RawMessage);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "Hello World!"
        /// </para>
        /// </summary>
        public static SHLogFormatter RawMessage => new(NoPrefix, DefaultMessage, NoColor, NoCompositor);
        #endregion

        public static PrefixFormatter CombinePrefixes(string separator, params PrefixFormatter[] prefixFormatters)
        {
            string combinedFormatter(SHLog log)
            {
                string prefix = string.Empty;

                foreach (var formatter in prefixFormatters)
                    prefix += $"{separator}{formatter(log)}";

                return prefix;
            }

            return combinedFormatter;
        }

        #region Prefixes
        /// <summary>
        /// Constructs a prefix that includes A and B:
        /// <para>
        /// A:<br/>
        ///     1. The logs custom prefix (if it was set).<br/>
        /// OR<br/>
        ///     2. The reflected class/file/line of the caller.<br/><br/>
        /// B:<br/>
        ///     The log level (if the level is warning or error).
        /// </para>
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A dynamically formatted prefix.</returns>
        public static string DefaultPrefix(SHLog log)
        {
            string fileName = log.CallerFileName;
            string className = log.CallerClassName;
            long lineNumber = log.CallerLineNumber;
            string logLevelPrefix = GetDefaultLogLevelPrefix(log.Level);

            // if the log already has a set prefix, include it in the total prefix
            if (log.UsesCustomPrefix)
            {
                return log.Level switch
                {
                    SHLogLevel.Log => $"[{log.Prefix}, File: {fileName}, Line: {lineNumber}]",
                    SHLogLevel.Verbose => $"[{log.Prefix}, File: {fileName}, Line: {lineNumber}]",
                    _ => $"[{logLevelPrefix} - {log.Prefix}]"
                };
            }
            else
            {
                return log.Level switch
                {
                    SHLogLevel.Log => $"[{className}, File: {fileName}, Line: {lineNumber}]",
                    SHLogLevel.Verbose => $"[{className}, File: {fileName}, Line: {lineNumber}]",
                    _ => $"[{logLevelPrefix} - {className}]"
                };
            }
        }

        /// <summary>
        /// Constructs no prefix.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>An empty prefix.</returns>
        public static string NoPrefix(SHLog log) => "";

        /// <summary>
        /// Constructs a prefix that includes the name of the log's context.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A prefix in the format: "[Context: {contextName}]"</returns>
        public static string ContextPrefix(SHLog log)
        {
            string context;

            if (log.Context == null)
                context = "[Context: ]";
            else
                context = $"[Context: {log.Context.name}]";

            return context;
        }

        /// <summary>
        /// Constructs a prefix that includes a timestamp.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A prefix in the format: "[Hour:Minute:Second]".</returns>
        public static string TimestampPrefix(SHLog log)
        {
            var dateTime = DateTime.Now;
            string hour = GetTwoDigitTime(dateTime.Hour);
            string minute = GetTwoDigitTime(dateTime.Minute);
            string second = GetTwoDigitTime(dateTime.Second);

            string timeStamp = $"[{hour}:{minute}:{second}]";

            return timeStamp;
        }

        /// <summary>
        /// Constructs a prefix that includes a long timestamp.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A prefix in the format: "[Day/Month - Hour:Minute:Second]".</returns>
        public static string LongTimestampPrefix(SHLog log)
        {
            var dateTime = DateTime.Now;
            string month = GetTwoDigitTime(dateTime.Month);
            string day = GetTwoDigitTime(dateTime.Day);
            string hour = GetTwoDigitTime(dateTime.Hour);
            string minute = GetTwoDigitTime(dateTime.Minute);
            string second = GetTwoDigitTime(dateTime.Second);

            string timeStamp = $"[{month}/{day} - {hour}:{minute}:{second}]";

            return timeStamp;
        }
        #endregion

        #region Messages
        /// <summary>
        /// Performs no alterations to the log's message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>An unaltered message.</returns>
        public static string DefaultMessage(SHLog log)
        {
            return log.Message;
        }
        #endregion

        #region Colors
        /// <summary>
        /// Applies rich text color tags to a formatted message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="message">The message to apply color tags to.</param>
        /// <returns>The message with rich text color tags wrapping it (based on the log's color).</returns>
        public static string DefaultColor(SHLog log, string message)
        {
            Color color;

            if (log.UsesCustomColor)
                color = log.Color;
            else
                color = SHLogger.GetColorForLogLevel(log.Level);

            string colorString = ColorUtility.ToHtmlStringRGB(color);

            return $"<color=#{colorString}>{message}</color>";
        }

        /// <summary>
        /// Applies no color.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="message">The message to apply color to.</param>
        /// <returns>The message with no added color.</returns>
        public static string NoColor(SHLog log, string message) => message;
        #endregion

        #region Compositors
        /// <summary>
        /// Composes the log's final message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="format">The formatter to use when composing.</param>
        /// <returns>A colored, composed message in the format: "[Prefix] - [Message]".</returns>
        public static string DefaultCompositor(SHLog log, SHLogFormatter format)
        {
            string message = $"{format.FormatPrefix(log)} - {format.FormatMessage(log)}";

            return format.SetColor(log, message) + "\n";
        }

        /// <summary>
        /// Does not compose the end message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="format">The formatter for the log.</param>
        /// <returns>A colored message composed with no space as: [Prefix][Message].</returns>
        public static string NoCompositor(SHLog log, SHLogFormatter format) => format.SetColor(log, $"{format.FormatPrefix(log)}{format.FormatMessage(log)}");
        #endregion

        private static string GetDefaultLogLevelPrefix(SHLogLevel level)
        {
            return $"{level.ToString().ToUpper()}";
        }

        private static string GetTwoDigitTime(int timeUnit)
        {
            string twoDigitTime = timeUnit.ToString();

            if (timeUnit < 10)
                twoDigitTime = $"0{twoDigitTime}";

            return twoDigitTime;
        }
    }
}
