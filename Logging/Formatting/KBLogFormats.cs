using System;
using UnityEngine;
using static InternProject.Logging.KBLogFormatter;

namespace InternProject.Logging
{
    /// <summary>
    /// A static collection of various pre-made <see cref="KBLogFormatter"/>s.
    /// </summary>
    public static class KBLogFormats
    {
        #region Static Defaults
        /// <summary>
        /// The default log formatter.
        /// <para>
        ///     <example>
        ///     <c>KBLogger.Log("Hello World!", KBLogFormats.Default);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static KBLogFormatter Default => new(DefaultPrefix);

        /// <summary>
        /// The default log formatter with a time stamp in the prefix.
        /// <para>
        ///     <example>
        ///     <c>KBLogger.Log("Hello World!", DefaultWithTimestamp);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Hour:Minute:Second] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static KBLogFormatter DefaultWithTimestamp => new(CombinePrefixes(" ", TimestampPrefix, DefaultPrefix));

        /// <summary>
        /// The default log formatter with a long time stamp in the prefix.
        /// <para>
        ///     <example>
        ///     <c>KBLogger.Log("Hello World!", KBLogFormats.DefaultWithLongTimestamp);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Day/Month - Hour:Minute:Second] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static KBLogFormatter DefaultWithLongTimestamp => new(CombinePrefixes(" ", LongTimestampPrefix, DefaultPrefix));

        /// <summary>
        /// The default log formatter with the context name in the prefix.
        /// <para>
        ///     <example>
        ///     <c>KBLogger.Log("Hello World!", KBLogFormats.DefaultWithLongTimeStamp);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "[Context: {contextName}] [ClassName] - Hello World!"
        /// </para>
        /// </summary>
        public static KBLogFormatter DefaultWithContext => new(CombinePrefixes(" ", ContextPrefix, DefaultPrefix));

        /// <summary>
        /// A formatter that outputs a raw message.
        /// <para>
        ///     <example>
        ///     <c>KBLogger.Log("Hello World!", KBLogFormats.RawMessage);</c> 
        ///     </example>
        /// </para>
        /// <para>
        ///     Output: "Hello World!"
        /// </para>
        /// </summary>
        public static KBLogFormatter RawMessage => new(NoPrefix, DefaultMessage, NoColor, NoCompositor);
        #endregion

        public static PrefixFormatter CombinePrefixes(string separator, params PrefixFormatter[] prefixFormatters)
        {
            string combinedFormatter(KBLog log)
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
        public static string DefaultPrefix(KBLog log)
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
                    KBLogLevel.Log => $"[{log.Prefix}]",
                    KBLogLevel.Verbose => $"[{log.Prefix}, File: {fileName}, Line: {lineNumber}]",
                    _ => $"[{logLevelPrefix} - {log.Prefix}]"
                };
            }
            else
            {
                return log.Level switch
                {
                    KBLogLevel.Log => $"[{className}]",
                    KBLogLevel.Verbose => $"[File: {fileName}, Line: {lineNumber}]",
                    _ => $"[{logLevelPrefix} - {className}]"
                };
            }
        }

        /// <summary>
        /// Constructs no prefix.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>An empty prefix.</returns>
        public static string NoPrefix(KBLog log) => "";

        /// <summary>
        /// Constructs a prefix that includes the name of the log's context.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A prefix in the format: "[Context: {contextName}]"</returns>
        public static string ContextPrefix(KBLog log)
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
        public static string TimestampPrefix(KBLog log)
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
        public static string LongTimestampPrefix(KBLog log)
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
        public static string DefaultMessage(KBLog log)
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
        public static string DefaultColor(KBLog log, string message)
        {
            Color color;

            if (log.UsesCustomColor)
                color = log.Color;
            else
                color = KBLogger.GetColorForLogLevel(log.Level);

            string colorString = ColorUtility.ToHtmlStringRGB(color);

            return $"<color=#{colorString}>{message}</color>";
        }

        /// <summary>
        /// Applies no color.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="message">The message to apply color to.</param>
        /// <returns>The message with no added color.</returns>
        public static string NoColor(KBLog log, string message) => message;
        #endregion

        #region Compositors
        /// <summary>
        /// Composes the log's final message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="format">The formatter to use when composing.</param>
        /// <returns>A colored, composed message in the format: "[Prefix] - [Message]".</returns>
        public static string DefaultCompositor(KBLog log, KBLogFormatter format)
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
        public static string NoCompositor(KBLog log, KBLogFormatter format) => format.SetColor(log, $"{format.FormatPrefix(log)}{format.FormatMessage(log)}");
        #endregion

        private static string GetDefaultLogLevelPrefix(KBLogLevel level)
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
