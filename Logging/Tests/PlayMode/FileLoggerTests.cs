using System.IO;
using NUnit.Framework;

namespace Shears.Logging.Tests
{
    public class FileLoggerTests : LoggerTestBase
    {
        protected override bool IncludeFancyText => true;
        protected override bool TestClearing => false;

        [SetUp]
        public void SetUp()
        {
            SHLogger.LoggingType = SHLogger.LogType.File;
        }

        [Test]
        public void FileCreationTest()
        {
            LogFancyText("------------CREATION TEST------------\n");
            SHLogger.Log("Create log");

            string filePath = SHLogger.LogFilePath;
            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(filePath)));

            LogFancyText("\n");
        }

        [Test, Order(100)]
        public void SaveTest()
        {
            KBFormatterTest();
            PrefixTest();
            LogLevelTest();

            SHLogger.SaveLogFile();
        }

        protected override string GetMostRecentMessage(string targetMessage)
        {
            string text;

#if UNITY_WEBGL
            byte[] textBytes = File.ReadAllBytes(KBLogger.LogFilePath);
            text = System.Text.Encoding.UTF8.GetString(textBytes);
#else
            text = File.ReadAllText(SHLogger.LogFilePath);
#endif

            if (text.Length > 0 && targetMessage.Length > 0)
                return text[^targetMessage.Length..];
            else
                return text;
        }
    }
}
