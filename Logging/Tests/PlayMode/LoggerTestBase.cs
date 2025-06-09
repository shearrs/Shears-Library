using NUnit.Framework;
using UnityEngine;

namespace Shears.Logging.Tests
{
    public abstract class LoggerTestBase
    {
        protected abstract bool IncludeFancyText { get; }
        protected abstract bool TestClearing { get; }

        [Test]
        public virtual void LogLevelTest()
        {
            LogFancyText("------------LOG LEVEL TEST------------\n");
            ValidateOutput(new("Hello World!", level: SHLogLevels.Log));
            ValidateOutput(new("Hello World", level: SHLogLevels.Verbose));
            ValidateOutput(new("Hello World!", level: SHLogLevels.Warning));
            ValidateOutput(new("Hello World!", level: SHLogLevels.Error));
            LogFancyText("\n");
        }

        [Test]
        public virtual void PrefixTest()
        {
            LogFancyText("------------PREFIX TEST------------\n");
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: SHLogLevels.Log));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: SHLogLevels.Verbose));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: SHLogLevels.Warning));
            ValidateOutput(new("Hello World!", prefix: "Custom Prefix", level: SHLogLevels.Error));
            LogFancyText("\n");
        }

        [Test]
        public virtual void ColorTest()
        {
            LogFancyText("------------COLOR TEST------------\n");
            ValidateOutput(new("Hello World!", color: Color.green, level: SHLogLevels.Log));
            LogFancyText("\n");
        }

        [Test]
        public virtual void KBFormatterTest()
        {
            LogFancyText("------------KBFORMATTER TEST------------\n");

            string formatPrefixPart(SHLog log) => $"prefix: {log.Prefix}";
            var formatPrefix = SHLogFormats.CombinePrefixes(" ", SHLogFormats.ContextPrefix, formatPrefixPart);
            string formatMessage(SHLog log) => $"message: {log.Message}";
            string setColor(SHLog log, string message) => $"({log.Color}) {message}";
            string compositor(SHLog log, SHLogFormatter formatter)
            {
                string message = formatter.FormatMessage(log) + formatter.FormatPrefix(log) + "\n";

                return formatter.SetColor(log, message);
            }

            var customFormatter = new SHLogFormatter(formatPrefix, formatMessage, setColor, compositor);
            var contextGO = new GameObject("Context Object");

            ValidateOutput(new("Hello World!"), SHLogFormats.Default);
            ValidateOutput(new("Hello World!"), SHLogFormats.DefaultWithTimestamp);
            ValidateOutput(new("Hello World!"), SHLogFormats.DefaultWithLongTimestamp);
            ValidateOutput(new("Hello World!", context: contextGO), customFormatter);

            LogFancyText("\n");

            Object.Destroy(contextGO);
        }

        [Test]
        public virtual void IKBFormatterTest()
        {
            LogFancyText("------------IKBFORMATTER TEST------------\n");

            var formatter = ScriptableObject.CreateInstance<SHLogFormatterData>();

            formatter.PrefixFormatter = SHLogFormats.TimestampPrefix;
            formatter.MessageFormatter = SHLogFormats.DefaultMessage;
            formatter.ColorSetter = SHLogFormats.DefaultColor;
            formatter.CompositorFunction = SHLogFormats.DefaultCompositor;

            SHLogger.FormatterOverride = formatter;

            ValidateOutput(new("Hello World"));

            LogFancyText("\n");
        }

        [Test]
        public virtual void ClearTest()
        {
            if (!TestClearing)
                return;

            LogFancyText("------------CLEAR TEST------------\n");

            SHLogger.Log("Clear me!");

            SHLogger.Clear();

            string message = GetMostRecentMessage("");

            Assert.AreEqual(message, string.Empty);

            LogFancyText("\n");
        }

        private void ValidateOutput(SHLog log, ISHLogFormatter formatter = null)
        {
            string targetMessage;

            if (formatter == null || !formatter.IsValid())
            {
                var newFormatter = SHLogger.CurrentFormatter;

                Assert.IsNotNull(newFormatter);
                Assert.IsTrue(newFormatter.IsValid());

                newFormatter = FixLineNumberPrefix(newFormatter);
                formatter = newFormatter;

                targetMessage = formatter.Format(log);
                SHLogger.Log(log);
            }
            else
            {
                targetMessage = formatter.Format(log);
                SHLogger.Log(log, formatter);
            }

            string message = GetMostRecentMessage(targetMessage);

            Assert.AreEqual(targetMessage, message);
        }

        protected abstract string GetMostRecentMessage(string targetMessage);

        // This is because while calling formatter.Format() does give us a line number, 
        // its the line above the actual KBLogger.Log() call, and in a real situation they would be the same
        protected string FormatPrefixWithIncrementedLineNumber(SHLog log, SHLogFormatter.PrefixFormatter prefixFormatter)
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

            SHLogger.Log(message, formatter: SHLogFormats.RawMessage);
        }

        private ISHLogFormatter FixLineNumberPrefix(ISHLogFormatter formatter)
        {
            if (formatter is SHLogFormatter structFormatter)
            {
                var currentPrefixFormatter = structFormatter.FormatPrefix;
                structFormatter.FormatPrefix = (log) => FormatPrefixWithIncrementedLineNumber(log, currentPrefixFormatter);

                return structFormatter;
            }
            else if (formatter is SHLogFormatterData dataFormatter)
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
