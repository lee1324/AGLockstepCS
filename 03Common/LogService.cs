using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AGSyncCS
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }

    public class LogService
    {
        private static LogService instance;
        private static readonly object lockObject = new object();
        
        private string logFilePath;
        private LogLevel minimumLevel;
        private bool enableConsoleOutput;
        private bool enableFileOutput;
        private Queue<LogEntry> logQueue;
        private Thread logWorkerThread;
        private bool isRunning;
        private readonly object queueLock = new object();

        public LogService()
        {
            logFilePath = ServerConfig.LOG_FILE_PATH;
            minimumLevel = ServerConfig.DEFAULT_LOG_LEVEL;
            enableConsoleOutput = ServerConfig.ENABLE_CONSOLE_OUTPUT;
            enableFileOutput = ServerConfig.ENABLE_FILE_OUTPUT;
            logQueue = new Queue<LogEntry>();
            isRunning = false;
        }

        public static LogService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new LogService();
                        }
                    }
                }
                return instance;
            }
        }

        public string LogFilePath
        {
            get { return logFilePath; }
            set { logFilePath = value; }
        }

        public LogLevel MinimumLevel
        {
            get { return minimumLevel; }
            set { minimumLevel = value; }
        }

        public bool EnableConsoleOutput
        {
            get { return enableConsoleOutput; }
            set { enableConsoleOutput = value; }
        }

        public bool EnableFileOutput
        {
            get { return enableFileOutput; }
            set { enableFileOutput = value; }
        }

        public void Start()
        {
            if (isRunning)
                return;

            isRunning = true;
            logWorkerThread = new Thread(ProcessLogQueue);
            logWorkerThread.IsBackground = true;
            logWorkerThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
            if (logWorkerThread != null && logWorkerThread.IsAlive)
            {
                logWorkerThread.Join(5000); // Wait up to 5 seconds
            }
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Error(string message, Exception exception)
        {
            Log(LogLevel.Error, message, exception);
        }

        public void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public void Fatal(string message, Exception exception)
        {
            Log(LogLevel.Fatal, message, exception);
        }

        public void Log(LogLevel level, string message)
        {
            if (level < minimumLevel)
                return;

            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            lock (queueLock)
            {
                logQueue.Enqueue(entry);
                Monitor.Pulse(queueLock);
            }
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            if (level < minimumLevel)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine("Exception: " + exception.Message);
            sb.AppendLine("StackTrace: " + exception.StackTrace);

            Log(level, sb.ToString());
        }

        private void ProcessLogQueue()
        {
            while (isRunning)
            {
                LogEntry entry = null;

                lock (queueLock)
                {
                    if (logQueue.Count > 0)
                    {
                        entry = logQueue.Dequeue();
                    }
                    else
                    {
                        Monitor.Wait(queueLock, 1000); // Wait 1 second for new entries
                        continue;
                    }
                }

                if (entry != null)
                {
                    WriteLogEntry(entry);
                }
            }

            // Process remaining entries
            lock (queueLock)
            {
                while (logQueue.Count > 0)
                {
                    LogEntry entry = logQueue.Dequeue();
                    WriteLogEntry(entry);
                }
            }
        }

        private void WriteLogEntry(LogEntry entry)
        {
            string logMessage = FormatLogEntry(entry);

            if (enableConsoleOutput)
            {
                WriteToConsole(entry.Level, logMessage);
            }

            if (enableFileOutput)
            {
                WriteToFile(logMessage);
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            return string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] [{1}] [Thread-{2}] {3}",
                entry.Timestamp,
                entry.Level.ToString().ToUpper(),
                entry.ThreadId,
                entry.Message);
        }

        private void WriteToConsole(LogLevel level, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            switch (level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }

            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        private void WriteToFile(string message)
        {
            try
            {
                lock (lockObject)
                {
                    File.AppendAllText(logFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Fallback to console if file writing fails
                Console.WriteLine("Failed to write to log file: " + ex.Message);
                Console.WriteLine("Log message: " + message);
            }
        }

        public void ClearLogFile()
        {
            try
            {
                lock (lockObject)
                {
                    if (File.Exists(logFilePath))
                    {
                        File.Delete(logFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to clear log file: " + ex.Message);
            }
        }

        public string[] GetRecentLogs(int lineCount = 100)
        {
            try
            {
                lock (lockObject)
                {
                    if (!File.Exists(logFilePath))
                        return new string[0];

                    string[] lines = File.ReadAllLines(logFilePath);
                    if (lines.Length <= lineCount)
                        return lines;

                    string[] recentLines = new string[lineCount];
                    Array.Copy(lines, lines.Length - lineCount, recentLines, 0, lineCount);
                    return recentLines;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read log file: " + ex.Message);
                return new string[0];
            }
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public int ThreadId { get; set; }
    }
} 