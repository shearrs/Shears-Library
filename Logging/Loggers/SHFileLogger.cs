using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Encoding = System.Text.Encoding;

namespace Shears.Logging
{
    /// <summary>
    /// An <see cref="ISHLogger"/> for logging to a log file.
    /// </summary>
    public class SHFileLogger : ISHLogger
    {
        #region File Path Constants
#pragma warning disable CS0414
        private static readonly string FILE_PATH = "data/";
        private static readonly string EDITOR_LOG = "editorLog";
#if !UNITY_EDITOR
        private static readonly string RUNTIME_LOG = "log_";
#endif
        private static readonly string FILE_EXTENSION = ".txt";
#pragma warning restore CS0414
        #endregion

#if !UNITY_WEBGL
        private readonly List<byte> logCache = new();
#endif

        private readonly string _runtimeGUID;

        public string RuntimeGUID => _runtimeGUID;

        public ISHLogFormatter Formatter => new SHLogFormatter(formatPrefix: SHLogFormats.LongTimestampPrefix, setColor: SHLogFormats.NoColor);

        public SHFileLogger()
        {
            _runtimeGUID = Guid.NewGuid().ToString();
        }

        public void Log(SHLog log)
        {
            Log(log, Formatter);
        }

        public void Log(SHLog log, ISHLogFormatter formatter)
        {
            if (!formatter.IsValid())
            {
                Debug.LogError("Formatter not set!");
                return;
            }

            string message = formatter.Format(log);

#if UNITY_WEBGL
            WriteWebGLLog(message);
#else
            WriteLog(message);
#endif
        }

        public void Clear()
        {
            string filePath = GetFilePath();

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Debug.Log("directory does not exist!");
                return;
            }

            try
            {
                File.WriteAllText(filePath, "");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to clear data from: " + filePath);
                Debug.LogError("Error: " + e.Message);
            }
        }

        public void Save()
        {
#if UNITY_WEBGL
            SaveWebGLLog();
#else
            SaveLog();
#endif
        }

        /// <summary>
        /// Gets the platform-specific file path for the log folder.
        /// </summary>
        /// <returns>The file path for the log folder.</returns>
        public string GetFilePath()
        {
            string filePath = string.Empty;

#if UNITY_EDITOR
            filePath = Path.Combine(Application.persistentDataPath, $"{FILE_PATH}{EDITOR_LOG}{FILE_EXTENSION}");
#else
            filePath = Path.Combine(Application.persistentDataPath, $"{FILE_PATH}{RUNTIME_LOG}{_runtimeGUID}{FILE_EXTENSION}");
#endif

            return filePath;
        }

        private void WriteStringToBinary(string message)
        {
            string filePath = GetFilePath();
            string directoryName = Path.GetDirectoryName(filePath);

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Append);

                stream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save data to: " + filePath);
                Debug.LogError("Error: " + e.Message);
            }
        }

        private void WriteBinaryToString(byte[] messageBytes)
        {
            string filePath = GetFilePath();
            string directoryName = Path.GetDirectoryName(filePath);

            string message = Encoding.UTF8.GetString(messageBytes);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            try
            {
                File.WriteAllText(filePath, message);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save data to: " + filePath);
                Debug.LogError("Error: " + e.Message);
            }
        }

#if UNITY_WEBGL
        /// <summary>
        /// Writes log as bytes to the browser cache.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        private void WriteWebGLLog(string message)
        {
            WriteStringToBinary(message);
        }

        /// <summary>
        /// Saves from the browser cache to the downloads folder in a WebGL build.
        /// </summary>
        private void SaveWebGLLog()
        {
            string filePath = GetFilePath();

            byte[] logBytes = File.ReadAllBytes(filePath);
            string logText = Encoding.UTF8.GetString(logBytes);

            WebGLFileSaver.SaveFile(logText, Path.GetFileName(filePath));
        }
#else
        /// <summary>
        /// Writes log to a cache as bytes.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        private void WriteLog(string message)
        {
            logCache.AddRange(Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Saves log as bytes to the persistent data path.
        /// </summary>
        private void SaveLog()
        {
            WriteBinaryToString(logCache.ToArray());
        }
#endif
    }
}
