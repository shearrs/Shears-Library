using NUnit.Framework;
using UnityEngine;

namespace InternProject.Logging.Tests
{
    public abstract class LoggerTestBase
    {
        protected abstract bool IncludeFancyText { get; }
        protected abstract bool TestClearing { get; }

        [Test]
        public virtual void LogLevelTest()
        {
            LogFancyText("------------LOG LEVEL TEST------------\n");
            ValidateOutput(new("Hello World!", level: KBLogLevel.Log));
            ValidateOutput(new("Hello World", level: KBLogLevel.Verbose));
            ValidateOutput(new("Hello World!", level: KBLogLevel.Warning));
            ValidateOutput(new("Hello World!", level: KBLogLevel.Error));
            LogFancyText("\n");
        }

        [Test]
        public virtual void PrefixTest()
        {
            LogFancyText("------------PREFIX TEST------------\n");
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: KBLogLevel.Log));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: KBLogLevel.Verbose));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: KBLogLevel.Warning));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: KBLogLevel.Error));
            LogFancyText("\n");
        }

        [Test]
        public virtual void ColorTest()
        {
            LogFancyText("------------COLOR TEST------------\n");
            ValidateOutput(new("Hello World!", color: Color.green, level: KBLogLevel.Log));
            LogFancyText("\n");
        }

        [Test]
        public virtual void KBFormatterTest()
        {
            LogFancyText("------------KBFORMATTER TEST------------\n");

            string formatPrefixPart(KBLog log) => $"prefix: {log.Prefix}";
            var formatPrefix = KBLogFormats.CombinePrefixes(" ", KBLogFormats.ContextPrefix, formatPrefixPart);
            string formatMessage(KBLog log) => $"message: {log.Message}";
            string setColor(KBLog log, string message) => $"({log.Color}) {message}";
            string compositor(KBLog log, KBLogFormatter formatter)
            {
                string message = formatter.FormatMessage(log) + formatter.FormatPrefix(log) + "\n";

                return formatter.SetColor(log, message);
            }

            var customFormatter = new KBLogFormatter(formatPrefix, formatMessage, setColor, compositor);
            var contextGO = new GameObject("Context Object");

            ValidateOutput(new("Hello World!"), KBLogFormats.Default);
            ValidateOutput(new("Hello World!"), KBLogFormats.DefaultWithTimestamp);
            ValidateOutput(new("Hello World!"), KBLogFormats.DefaultWithLongTimestamp);
            ValidateOutput(new("Hello World!", context: contextGO), customFormatter);

            LogFancyText("\n");

            Object.Destroy(contextGO);
        }

        [Test]
        public virtual void IKBFormatterTest()
        {
            LogFancyText("------------IKBFORMATTER TEST------------\n");

            var formatter = ScriptableObject.CreateInstance<KBLogFormatterData>();

            formatter.PrefixFormatter = KBLogFormats.TimestampPrefix;
            formatter.MessageFormatter = KBLogFormats.DefaultMessage;
            formatter.ColorSetter = KBLogFormats.DefaultColor;
            formatter.CompositorFunction = KBLogFormats.DefaultCompositor;

            KBLogger.FormatterOverride = formatter;

            ValidateOutput(new("Hello World"));

            LogFancyText("\n");
        }

        [Test]
        public virtual void ClearTest()
        {
            if (!TestClearing)
                return;

            LogFancyText("------------CLEAR TEST------------\n");

            KBLogger.Log("Clear me!");

            KBLogger.Clear();

            string message = GetMostRecentMessage("");

            Assert.AreEqual(message, string.Empty);

            LogFancyText("\n");
        }

        [TearDown]
        public virtual void TearDown()
        {
            var loggers = Object.FindObjectsByType<KBLogger>(FindObjectsSortMode.None);

            for (int i = 0; i < loggers.Length; i++)
                Object.Destroy(loggers[i]);
        }

        private void ValidateOutput(KBLog log, IKBLogFormatter formatter = null)
        {
            string targetMessage;

            if (formatter == null || !formatter.IsValid())
            {
                var newFormatter = KBLogger.CurrentFormatter;

                Assert.IsNotNull(newFormatter);
                Assert.IsTrue(newFormatter.IsValid());

                newFormatter = FixLineNumberPrefix(newFormatter);
                formatter = newFormatter;

                targetMessage = formatter.Format(log);
                KBLogger.Log(log);
            }
            else
            {
                targetMessage = formatter.Format(log);
                KBLogger.Log(log, formatter);
            }

            string message = GetMostRecentMessage(targetMessage);

            Assert.AreEqual(targetMessage, message);
        }

        protected abstract string GetMostRecentMessage(string targetMessage);

        // This is because while calling formatter.Format() does give us a line number, 
        // its the line above the actual KBLogger.Log() call, and in a real situation they would be the same
        protected string FormatPrefixWithIncrementedLineNumber(KBLog log, KBLogFormatter.PrefixFormatter prefixFormatter)
        {
            string defaultPrefix = prefixFormatter(log);

            int lineNumberStartIndex = defaultPrefix.IndexOf("Line: ");

            if (lineNumberStartIndex == -1)
                return defaultPrefix;

            lineNumberStartIndex += 6;
            int lineNumberEndIndex = defaultPrefix.IndexOf(']', lineNumberStartIndex);

            if (lineNumberEndIndex == -1)
                return defaultPrefix;

            int lineNumber = int.Parse(defaultPrefix[lineNumberStartIndex..lineNumberEndIndex]);

            lineNumber++;

            return defaultPrefix[..lineNumberStartIndex] + lineNumber + defaultPrefix[lineNumberEndIndex..];
        }

        protected void LogFancyText(string message)
        {
            if (!IncludeFancyText)
                return;

            KBLogger.Log(message, KBLogFormats.RawMessage);
        }

        private IKBLogFormatter FixLineNumberPrefix(IKBLogFormatter formatter)
        {
            if (formatter is KBLogFormatter structFormatter)
            {
                var currentPrefixFormatter = structFormatter.FormatPrefix;
                structFormatter.FormatPrefix = (log) => FormatPrefixWithIncrementedLineNumber(log, currentPrefixFormatter);

                return structFormatter;
            }
            else if (formatter is KBLogFormatterData dataFormatter)
            {
                var currentPrefixFormatter = dataFormatter.PrefixFormatter;
                dataFormatter.PrefixFormatter = (log) => FormatPrefixWithIncrementedLineNumber(log, currentPrefixFormatter);

                return dataFormatter;
            }

            Debug.LogWarning($"Need to fix line number for unsupported formatter!: {formatter.GetType().Name}");
            return null;
        }
    }
}
