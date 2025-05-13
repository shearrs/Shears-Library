using System.Runtime.CompilerServices;
using UnityEngine;
using static InternProject.Logging.KBLogFormatter;

namespace InternProject.Logging
{
    /// <summary>
    /// A ScriptableObject formatter for creating formats code-free.
    /// </summary>
    [CreateAssetMenu(fileName = "New Log Formatter", menuName = "Logging/Formatter")]
    public class KBLogFormatterData : ScriptableObject, IKBLogFormatter
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

        public string Format(KBLog log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = -1)
        {
            var formatter = GetFormatter();

            return formatter.Format(log, callerFilePath, callerLineNumber);
        }

        public bool IsValid()
        {
            return GetFormatter().IsValid();
        }

        /// <summary>
        /// Gets the underlying <see cref="KBLogFormatter"/>.
        /// </summary>
        /// <returns>The underlying <see cref="KBLogFormatter"/>.</returns>
        public KBLogFormatter GetFormatter()
        {
            return new(GetPrefixFormatter(), GetMessageFormatter(), GetColorSetter(), GetCompositor());
        }

        private PrefixFormatter GetPrefixFormatter()
        {
            if (PrefixFormatter != null)
                return PrefixFormatter;

            return prefixFormat switch
            {
                PrefixFormat.Default => KBLogFormats.DefaultPrefix,
                PrefixFormat.Context => KBLogFormats.ContextPrefix,
                PrefixFormat.Timestamp => KBLogFormats.TimestampPrefix,
                PrefixFormat.LongTimestamp => KBLogFormats.LongTimestampPrefix,
                PrefixFormat.None => KBLogFormats.NoPrefix,
                _ => null
            };
        }

        private MessageFormatter GetMessageFormatter()
        {
            if (MessageFormatter != null)
                return MessageFormatter;

            return messageFormat switch
            {
                MessageFormat.Default => KBLogFormats.DefaultMessage,
                _ => null
            };
        }

        private ColorSetter GetColorSetter()
        {
            if (ColorSetter != null)
                return ColorSetter;

            return colorFormat switch
            {
                ColorFormat.Default => KBLogFormats.DefaultColor,
                ColorFormat.None => KBLogFormats.NoColor,
                _ => null
            };
        }

        private CompositorFunction GetCompositor()
        {
            if (CompositorFunction != null)
                return CompositorFunction;

            return compositor switch
            {
                Compositor.Default => KBLogFormats.DefaultCompositor,
                Compositor.None => KBLogFormats.NoCompositor,
                _ => null
            };
        }
    }
}
