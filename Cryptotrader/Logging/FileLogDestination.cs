using System.IO;

namespace Cryptotrader.Logging
{
    public class FileLogDestination : LeveledLogDestination
    {
        private readonly string logsDirectory;
        private readonly string fileNamePattern;

        private FileStream fileStream;
        private StreamWriter fileWriter;

        public FileLogDestination(
            string filePath,
            string filePattern,
            LogLevel maxLevel
        ) : base(maxLevel)
        {
            logsDirectory = filePath;
            fileNamePattern = filePattern;
            if (!Directory.Exists(logsDirectory)) CreateLogsDir();

            OpenNewFile();
        }

        protected override void LogToTarget(LogMessage message)
        {
            fileWriter.Write(message.ToString(false));
            fileWriter.Flush();
        }

        private void CreateLogsDir()
        {
            try
            {
                Directory.CreateDirectory(logsDirectory);
            }
            catch (IOException ex)
            {
                throw new ArgumentException($"Could not access directory '{logsDirectory}'", ex);
            }
        }

        private void OpenNewFile()
        {
            string fileName = GetLogFileName();
            string filePath = Path.Combine(logsDirectory, fileName);

            fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            fileWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        }

        private string GetLogFileName()
        {
            return string.Format(fileNamePattern, DateTime.Now);
        }
    }
}
