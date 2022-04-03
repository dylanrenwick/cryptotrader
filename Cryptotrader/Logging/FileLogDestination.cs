using System;
using System.IO;

namespace Cryptotrader.Logging
{
    public class FileLogDestination : LogDestination, IDisposable
    {
        private FileStream fileStream;
        private StreamWriter fileWriter;

        public FileLogDestination(string filePath, LogLevel maxLevel)
            : base(maxLevel)
        {
            fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            fileWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        }

        public void Dispose()
        {
            fileWriter.Dispose();
            fileStream.Dispose();
        }

        protected override void LogToTarget(LogMessage message)
        {
            fileWriter.Write(message);
            fileWriter.Flush();
        }
    }
}
