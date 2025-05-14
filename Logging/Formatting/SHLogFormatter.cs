using System;
using System.Runtime.CompilerServices;

namespace Shears.Logging
{
    /// <summary>
    /// Defines how a <see cref="SHLog"/> is formatted when logged by a <see cref="ISHLogger"/>.
    /// </summary>
    [Serializable]
    public struct SHLogFormatter : ISHLogFormatter
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

        private PrefixFormatter _formatPrefix;
        private MessageFormatter _formatMessage;
        private ColorSetter _setColor;
        private CompositorFunction _compositor;

        #region Public Properties
        /// <summary>
        /// The <see cref="PrefixFormatter"/> used to create a prefix.
        /// </summary>
        public PrefixFormatter FormatPrefix { readonly get => _formatPrefix; set => _formatPrefix = value; }

        /// <summary>
        /// The <see cref="MessageFormatter"/> used to create a message.
        /// </summary>
        public MessageFormatter FormatMessage { readonly get => _formatMessage; set => _formatMessage = value; }

        /// <summary>
        /// The <see cref="ColorSetter"/> used to apply color to the message.
        /// </summary>
        public ColorSetter SetColor { readonly get => _setColor; set => _setColor = value; }

        /// <summary>
        /// The <see cref="CompositorFunction"/> to compose the final message. 
        /// </summary>
        public CompositorFunction Compositor { readonly get => _compositor; set => _compositor = value; }

        /// <summary>
        /// An empty formatter. Should not be used without initializing every delegate.
        /// </summary>
        public static SHLogFormatter Empty => new();
        #endregion

        /// <summary>
        /// Constructs a new KBLogFormatter.
        /// </summary>
        /// <param name="formatPrefix">The delegate for constructing the prefix of a log. Defaults to <see cref="SHLogFormats.DefaultPrefix"/></param>
        /// <param name="formatMessage">The delegate for formatting the message of a log. Defaults to <see cref="SHLogFormats.DefaultMessage"/></param>
        /// <param name="setColor">The delegate for applying color to a formatted message. Defaults to <see cref="SHLogFormats.DefaultColor"/></param>
        /// <param name="compositor">The delegate for composing the final formatted message. Defaults to <see cref="SHLogFormats.DefaultCompositor"/></param>
        public SHLogFormatter(PrefixFormatter formatPrefix = null, MessageFormatter formatMessage = null, ColorSetter setColor = null, CompositorFunction compositor = null)
        {
            formatPrefix ??= SHLogFormats.DefaultPrefix;
            formatMessage ??= SHLogFormats.DefaultMessage;
            setColor ??= SHLogFormats.DefaultColor;
            compositor ??= SHLogFormats.DefaultCompositor;

            _formatPrefix = formatPrefix;
            _formatMessage = formatMessage;
            _setColor = setColor;
            _compositor = compositor;
        }

        public readonly string Format(SHLog log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] long callerLineNumber = -1)
        {
            if (_formatPrefix == null || _formatMessage == null || _setColor == null || _compositor == null)
            {
                UnityEngine.Debug.LogWarning("You need to initialize every formatting delegate to use a formatter!");
                return log.Message;
            }

            if (log.CallerFilePath == string.Empty)
                log.CallerFilePath = callerFilePath;

            if (log.CallerLineNumber == -1)
                log.CallerLineNumber = callerLineNumber;

            string formattedLog = _compositor(log, this);

            return formattedLog;
        }
    
        public readonly bool IsValid() => this != Empty;

        #region Operators
        public static bool operator==(SHLogFormatter a, SHLogFormatter b)
        {
            return a._formatPrefix == b._formatPrefix 
                && a._formatMessage == b._formatMessage 
                && a._setColor == b._setColor;
        }

        public static bool operator!=(SHLogFormatter a, SHLogFormatter b)
        {
            return !(a == b);
        }

        public readonly override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
