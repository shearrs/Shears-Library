using System;
using System.Text;
using UnityEngine;
using static Shears.Logging.SHLogFormatter;

namespace Shears.Logging
{
    /// <summary>
    /// A static collection of various pre-made <see cref="SHLogFormatter"/>s.
    /// </summary>
    public static class SHLogFormats
    {
        private static readonly StringBuilder combineBuilder = new(64);
        private static readonly StringBuilder prefixBuilder = new(64);
        private static readonly StringBuilder compositorBuilder = new(128);

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
                combineBuilder.Clear();

                foreach (var formatter in prefixFormatters)
                {
                    combineBuilder.Append(separator);
                    combineBuilder.Append(formatter(log));
                }

                return combineBuilder.ToString();
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

            prefixBuilder.Clear();

            string verboseCustomPrefix(bool includeLogLevel)
            {
                if (includeLogLevel)
                {
                    prefixBuilder.Append('[');
                    prefixBuilder.Append(logLevelPrefix);
                    prefixBuilder.Append(']');
                }

                prefixBuilder.Append('[');
                prefixBuilder.Append(log.Prefix);
                prefixBuilder.Append(", File: ");
                prefixBuilder.Append(fileName);
                prefixBuilder.Append(", Line: ");
                prefixBuilder.Append(lineNumber);
                prefixBuilder.Append(']');

                return prefixBuilder.ToString();
            }

            string defaultCustomPrefix()
            {
                prefixBuilder.Append('[');
                prefixBuilder.Append(log.Prefix);
                prefixBuilder.Append(']');

                return prefixBuilder.ToString();
            }

            string logPrefix()
            {
                prefixBuilder.Append('[');
                prefixBuilder.Append(className);
                prefixBuilder.Append(']');

                return prefixBuilder.ToString();
            }

            string verbosePrefix(bool includeLogLevel)
            {
                if (includeLogLevel)
                {
                    prefixBuilder.Append('[');
                    prefixBuilder.Append(logLevelPrefix);
                    prefixBuilder.Append(']');
                }

                prefixBuilder.Append('[');
                prefixBuilder.Append(className);
                prefixBuilder.Append(", File: ");
                prefixBuilder.Append(fileName);
                prefixBuilder.Append(", Line: ");
                prefixBuilder.Append(lineNumber);
                prefixBuilder.Append(']');

                return prefixBuilder.ToString();
            }

            string defaultPrefix()
            {
                prefixBuilder.Append('[');
                prefixBuilder.Append(logLevelPrefix);
                prefixBuilder.Append(']');
                prefixBuilder.Append('[');
                prefixBuilder.Append(className);
                prefixBuilder.Append(']');

                return prefixBuilder.ToString();
            }

            if (log.UsesCustomPrefix)
            {
                return log.Level switch
                {
                    SHLogLevels.Verbose => verboseCustomPrefix(false),
                    SHLogLevels.Warning => verboseCustomPrefix(true),
                    SHLogLevels.Error => verboseCustomPrefix(true),
                    _ => defaultCustomPrefix()
                };
            }
            else
            {
                return log.Level switch
                {
                    SHLogLevels.Log => logPrefix(),
                    SHLogLevels.Verbose => verbosePrefix(false),
                    SHLogLevels.Warning => verbosePrefix(true),
                    SHLogLevels.Error => verbosePrefix(true),
                    _ => defaultPrefix()
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

            prefixBuilder.Clear();
            prefixBuilder.Append('[');
            prefixBuilder.Append(hour);
            prefixBuilder.Append(':');
            prefixBuilder.Append(minute);
            prefixBuilder.Append(':');
            prefixBuilder.Append(second);
            prefixBuilder.Append(']');

            return prefixBuilder.ToString();
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

            prefixBuilder.Clear();
            prefixBuilder.Append('[');
            prefixBuilder.Append(month);
            prefixBuilder.Append('/');
            prefixBuilder.Append(day);
            prefixBuilder.Append(" - ");
            prefixBuilder.Append(hour);
            prefixBuilder.Append(':');
            prefixBuilder.Append(minute);
            prefixBuilder.Append(':');
            prefixBuilder.Append(second);
            prefixBuilder.Append(']');

            return prefixBuilder.ToString();
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
            compositorBuilder.Clear();
            compositorBuilder.Append(format.FormatPrefix(log));
            compositorBuilder.Append(" - ");
            compositorBuilder.Append(format.FormatMessage(log));
            string message = $"{format.FormatPrefix(log)} - {format.FormatMessage(log)}";

            return format.SetColor(log, compositorBuilder.ToString()) + "\n";
        }

        /// <summary>
        /// Does not compose the end message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="format">The formatter for the log.</param>
        /// <returns>A colored message composed with no space as: [Prefix][Message].</returns>
        public static string NoCompositor(SHLog log, SHLogFormatter format)
        {
            compositorBuilder.Clear();
            compositorBuilder.Append(format.FormatPrefix(log));
            compositorBuilder.Append(format.FormatMessage(log));

            return format.SetColor(log, compositorBuilder.ToString());
        }
        #endregion

        private static string GetDefaultLogLevelPrefix(SHLogLevels level)
        {
            return level.ToString().ToUpper();
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
