using System;
using System.Runtime.CompilerServices;

namespace Shears.Logging
{
    /// <summary>
    /// Defines how a <see cref="SHLog"/> is formatted when logged by a <see cref="ISHLogger"/>.
    /// </summary>
    public class SHLogFormatter : ISHLogFormatter
    {
        #region Delegate Definitions
        /// <summary>
        /// A delegate for constructing a prefix of a <see cref="SHLog"/>.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A prefix.</returns>
        public delegate string PrefixFormatter(SHLog log);

        /// <summary>
        /// A delegate for formatting a message of a <see cref="SHLog"/>.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <returns>A formatted message.</returns>
        public delegate string MessageFormatter(SHLog log);

        /// <summary>
        /// A delegate for applying color to a formatted <see cref="SHLog"/> message.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="message">The message to color.</param>
        /// <returns>A colored version of the passed message.</returns>
        public delegate string ColorSetter(SHLog log, string message);

        /// <summary>
        /// A delegate for composing the final message of a <see cref="SHLog"/>.
        /// </summary>
        /// <param name="log">The log to read from.</param>
        /// <param name="formatter">The formatter to use for constructing the message.</param>
        /// <returns>The final formatted message.</returns>
        public delegate string CompositorFunction(SHLog log, SHLogFormatter formatter);
        #endregion

        private PrefixFormatter formatPrefix;
        private MessageFormatter formatMessage;
        private ColorSetter setColor;
        private CompositorFunction compositor;

        #region Public Properties
        /// <summary>
        /// The <see cref="PrefixFormatter"/> used to create a prefix.
        /// </summary>
        public PrefixFormatter FormatPrefix
        {
            get => formatPrefix;
            set => formatPrefix = value;
        }

        /// <summary>
        /// The <see cref="MessageFormatter"/> used to create a message.
        /// </summary>
        public MessageFormatter FormatMessage
        {
            get => formatMessage;
            set => formatMessage = value;
        }

        /// <summary>
        /// The <see cref="ColorSetter"/> used to apply color to the message.
        /// </summary>
        public ColorSetter SetColor
        {
            get => setColor;
            set => setColor = value;
        }

        /// <summary>
        /// The <see cref="CompositorFunction"/> to compose the final message.
        /// </summary>
        public CompositorFunction Compositor
        {
            get => compositor;
            set => compositor = value;
        }

        /// <summary>
        /// An empty formatter. Should not be used without initializing every delegate.
        /// </summary>
        public static readonly SHLogFormatter Empty = new();
        #endregion

        /// <summary>
        /// Constructs a new KBLogFormatter.
        /// </summary>
        /// <param name="formatPrefix">The delegate for constructing the prefix of a log. Defaults to <see cref="SHLogFormats.DefaultPrefix"/></param>
        /// <param name="formatMessage">The delegate for formatting the message of a log. Defaults to <see cref="SHLogFormats.DefaultMessage"/></param>
        /// <param name="setColor">The delegate for applying color to a formatted message. Defaults to <see cref="SHLogFormats.DefaultColor"/></param>
        /// <param name="compositor">The delegate for composing the final formatted message. Defaults to <see cref="SHLogFormats.DefaultCompositor"/></param>
        public SHLogFormatter(
            PrefixFormatter formatPrefix = null,
            MessageFormatter formatMessage = null,
            ColorSetter setColor = null,
            CompositorFunction compositor = null
        )
        {
            formatPrefix ??= SHLogFormats.DefaultPrefix;
            formatMessage ??= SHLogFormats.DefaultMessage;
            setColor ??= SHLogFormats.DefaultColor;
            compositor ??= SHLogFormats.DefaultCompositor;

            this.formatPrefix = formatPrefix;
            this.formatMessage = formatMessage;
            this.setColor = setColor;
            this.compositor = compositor;
        }

        public string Format(in SHLog log)
        {
            if (
                formatPrefix == null
                || formatMessage == null
                || setColor == null
                || compositor == null
            )
            {
                UnityEngine.Debug.LogWarning(
                    "You need to initialize every formatting delegate to use a formatter!"
                );
                return log.Message;
            }

            string formattedLog = compositor(log, this);

            return formattedLog;
        }

        public bool IsValid() => this != Empty;
    }
}
