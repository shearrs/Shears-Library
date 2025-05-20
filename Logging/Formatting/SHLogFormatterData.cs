using System.Runtime.CompilerServices;
using UnityEngine;
using static Shears.Logging.SHLogFormatter;

namespace Shears.Logging
{
    /// <summary>
    /// A ScriptableObject formatter for creating formats code-free.
    /// </summary>
    [CreateAssetMenu(fileName = "New Log Formatter", menuName = "Shears Library/Logging/Formatter")]
    public class SHLogFormatterData : ScriptableObject, ISHLogFormatter
    {
        #region Enum Definitions
        public enum PrefixFormat { Default, None, Context, Timestamp, LongTimestamp }
        public enum MessageFormat { Default }
        public enum ColorFormat { Default, None }
        public enum Compositor { Default, None }
        #endregion

        [SerializeField, Tooltip("Determines how the prefix of the log is formatted.")]
        private PrefixFormat prefixFormat;

        [SerializeField, Tooltip("Determines how the main body message of the log is formatted.")]
        private MessageFormat messageFormat;

        [SerializeField, Tooltip("Determines how color is applied to the log. Default uses rich text tags.")]
        private ColorFormat colorFormat;

        [SerializeField, Tooltip("Determines how the log is composed. Default is a colored message with the format: '[Prefix] - Message'")]
        private Compositor compositor;

        public PrefixFormatter PrefixFormatter { get; set; }
        public MessageFormatter MessageFormatter { get; set; }
        public ColorSetter ColorSetter { get; set; }
        public CompositorFunction CompositorFunction { get; set; }

        public string Format(SHLog log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = -1)
        {
            var formatter = GetFormatter();

            return formatter.Format(log, callerFilePath, callerLineNumber);
        }

        public bool IsValid()
        {
            return GetFormatter().IsValid();
        }

        /// <summary>
        /// Gets the underlying <see cref="SHLogFormatter"/>.
        /// </summary>
        /// <returns>The underlying <see cref="SHLogFormatter"/>.</returns>
        public SHLogFormatter GetFormatter()
        {
            return new(GetPrefixFormatter(), GetMessageFormatter(), GetColorSetter(), GetCompositor());
        }

        private PrefixFormatter GetPrefixFormatter()
        {
            if (PrefixFormatter != null)
                return PrefixFormatter;

            return prefixFormat switch
            {
                PrefixFormat.Default => SHLogFormats.DefaultPrefix,
                PrefixFormat.Context => SHLogFormats.ContextPrefix,
                PrefixFormat.Timestamp => SHLogFormats.TimestampPrefix,
                PrefixFormat.LongTimestamp => SHLogFormats.LongTimestampPrefix,
                PrefixFormat.None => SHLogFormats.NoPrefix,
                _ => null
            };
        }

        private MessageFormatter GetMessageFormatter()
        {
            if (MessageFormatter != null)
                return MessageFormatter;

            return messageFormat switch
            {
                MessageFormat.Default => SHLogFormats.DefaultMessage,
                _ => null
            };
        }

        private ColorSetter GetColorSetter()
        {
            if (ColorSetter != null)
                return ColorSetter;

            return colorFormat switch
            {
                ColorFormat.Default => SHLogFormats.DefaultColor,
                ColorFormat.None => SHLogFormats.NoColor,
                _ => null
            };
        }

        private CompositorFunction GetCompositor()
        {
            if (CompositorFunction != null)
                return CompositorFunction;

            return compositor switch
            {
                Compositor.Default => SHLogFormats.DefaultCompositor,
                Compositor.None => SHLogFormats.NoCompositor,
                _ => null
            };
        }
    }
}
