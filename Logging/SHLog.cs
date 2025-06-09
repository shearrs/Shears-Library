using UnityEngine;

namespace Shears.Logging
{
    /// <summary>
    /// Represents a formattable log for use by the <see cref="SHLogger"/>.
    /// </summary>
    [System.Serializable]
    public struct SHLog
    {
        #region Serialized Variables
        [Header("Default Settings")]
        [SerializeField, Tooltip("The main message of the log.")]
        private string _message;

        [SerializeField, Tooltip("The context of the log. When logged to the Unity console, this log will highlight its context upon being clicked.")]
        private Object _context;

        [SerializeField, Tooltip("The level/severity of the log. By default, influences the prefix and the color of the log.")]
        private SHLogLevels _level;

        [Header("Prefix")]
        [SerializeField, Tooltip("Whether or not to show the custom prefix textbox.")]
        private bool _usesCustomPrefix;

        [SerializeField, ShowIf(nameof(_usesCustomPrefix)), Tooltip("The prefix of the log's message. If left blank, it defaults to the prefix for the current log level.")]
        private string _prefix;

        [Header("Color")]
        [SerializeField, Tooltip("Whether or not to show the custom color selector.")]
        private bool _usesCustomColor;


        [SerializeField, ShowIf(nameof(_usesCustomColor)), Tooltip("If a logger supports color, this determines the output color of this log.")]
        private Color _color;
        #endregion

        private string _callerFilePath;
        private long _callerLineNumber;

        #region Public Properties
        /// <summary>
        /// The main body message of the log.
        /// </summary>
        public string Message { readonly get => _message; set => _message = value; }

        /// <summary>
        /// The context object of the log. When logged to the Unity console, this log will highlight its context upon being clicked.
        /// </summary>
        public Object Context { readonly get => _context; set => _context = value; }

        /// <summary>
        /// The prefix to the log's message.
        /// </summary>
        public string Prefix { readonly get => _prefix; set => _prefix = value; }

        /// <summary>
        /// The level/severity of the log. By default, influences the prefix and the color of the log.
        /// </summary>
        public SHLogLevels Level { readonly get => _level; set => _level = value; }

        /// <summary>
        /// The color of the log when displayed in a logger that supports color.
        /// </summary>
        public Color Color { readonly get => _color; set => _color = value; }

        /// <summary>
        /// Whether or not this log uses a custom prefix.
        /// </summary>
        public bool UsesCustomPrefix { readonly get => _usesCustomPrefix; set => _usesCustomPrefix = value; }

        /// <summary>
        /// Whether or not this log uses a custom color.
        /// </summary>
        public bool UsesCustomColor { readonly get => _usesCustomColor; set => _usesCustomColor = value; }
        #endregion

        #region Caller Info
        /// <summary>
        /// The file path of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal string CallerFilePath { readonly get => _callerFilePath; set => _callerFilePath = value; }

        /// <summary>
        /// The line number of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal long CallerLineNumber { readonly get => _callerLineNumber; set => _callerLineNumber = value; }

        /// <summary>
        /// The file name of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal readonly string CallerFileName => GetCallerFileName();

        /// <summary>
        /// The class name of the caller who logged this log. For use by the <see cref="SHLogger"/>.
        /// </summary>
        internal readonly string CallerClassName => GetCallerClassName();
        #endregion

        /// <summary>
        /// Constructs a <see cref="SHLog"/>.
        /// </summary>
        /// <param name="message">The main body message of the log.</param>
        /// <param name="context">The context object of the log. When logged to the Unity console, this log will highlight its context upon being clicked.</param>
        /// <param name="prefix">The prefix to the log's message.</param>
        /// <param name="level">The level/severity of the log. By default, influences the prefix and the color of the log.</param>
        /// <param name="color">The color of the log when displayed in a logger that supports color.</param>
        public SHLog(string message, Object context = null, string prefix = "", SHLogLevels level = SHLogLevels.Log, Color color = default)
        {
            _message = message;
            _context = context;
            _prefix = prefix;
            _level = level;
            _color = color;

            _callerFilePath = string.Empty;
            _callerLineNumber = -1;

            _usesCustomColor = (color != default);
            _usesCustomPrefix = (prefix != string.Empty);
        }

        private readonly string GetCallerFileName()
        {
            int lastSlashIndex = 0;
            for (int i = 0; i < _callerFilePath.Length; i++)
            {
                if (_callerFilePath[i] == '\\')
                    lastSlashIndex = i;
            }

            if (lastSlashIndex != 0 && _callerFilePath[lastSlashIndex] == '\\')
                lastSlashIndex++;

            string callerFileName = _callerFilePath[lastSlashIndex..];

            return callerFileName;
        }

        private readonly string GetCallerClassName()
        {
            string fileName = CallerFileName;

            if (fileName == string.Empty)
                return string.Empty;

            int fileExtensionIndex = fileName.IndexOf('.');
            string className = fileName[..fileExtensionIndex];

            return className;
        }
    }
}
