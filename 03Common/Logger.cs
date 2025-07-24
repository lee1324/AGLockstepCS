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

    public class Logger
    {
        private static readonly object lockObject = new object();
        
        private static string logFilePath;
        private static LogLevel minimumLevel;
        private static bool enableConsoleOutput;
        private static bool enableFileOutput;
        private static Queue<LogEntry> logQueue;
        private static Thread logWorkerThread;
        private static bool isRunning;
        private static readonly object queueLock = new object();

        static Logger()
        {
            logFilePath = Config.LOG_FILE_PATH;
            minimumLevel = Config.DEFAULT_LOG_LEVEL;
            enableConsoleOutput = Config.ENABLE_CONSOLE_OUTPUT;
            enableFileOutput = Config.ENABLE_FILE_OUTPUT;
            logQueue = new Queue<LogEntry>();
            isRunning = false;

			Start ();
        }

        public static string LogFilePath
        {
            get { return logFilePath; }
            set { logFilePath = value; }
        }

        public static LogLevel MinimumLevel
        {
            get { return minimumLevel; }
            set { minimumLevel = value; }
        }

        public static bool EnableConsoleOutput
        {
            get { return enableConsoleOutput; }
            set { enableConsoleOutput = value; }
        }

        public static bool EnableFileOutput
        {
            get { return enableFileOutput; }
            set { enableFileOutput = value; }
        }

        public static void Start()
        {
            if (isRunning)
                return;

            isRunning = true;
            logWorkerThread = new Thread(ProcessLogQueue);
            //logWorkerThread.IsBackground = true;
            logWorkerThread.Start();
        }

        public static void Stop()
        {
            isRunning = false;
            if (logWorkerThread != null && logWorkerThread.IsAlive)
            {
                logWorkerThread.Join(5000); // Wait up to 5 seconds
            }
        }

        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void Error(string message, Exception exception)
        {
            Log(LogLevel.Error, message, exception);
        }

        public static void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public static void Fatal(string message, Exception exception)
        {
            Log(LogLevel.Fatal, message, exception);
        }

        public static void Log(LogLevel level, string message)
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

        public static void Log(LogLevel level, string message, Exception exception)
        {
            if (level < minimumLevel)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine("Exception: " + exception.Message);
            sb.AppendLine("StackTrace: " + exception.StackTrace);

            Log(level, sb.ToString());
        }

        private static void ProcessLogQueue()
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

        private static void WriteLogEntry(LogEntry entry)
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

        private static string FormatLogEntry(LogEntry entry)
        {
            return string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] [{1}] [Thread-{2}] {3}",
                entry.Timestamp,
                entry.Level.ToString().ToUpper(),
                entry.ThreadId,
                entry.Message);
        }

		static void Print(string message){
			#if UNITY_64
			UnityEngine.Debug.Log(message);
			#else
			Console.WriteLine(message);
			#endif
		}

        private static void WriteToConsole(LogLevel level, string message)
        {
			#if UNITY_64
			Print(message);
			#else
                ConsoleColor originalColor = Console.ForegroundColor;

                switch (level) {
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
			#endif
        }

        private static void WriteToFile(string message)
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
                Print("Failed to write to log file: " + ex.Message);
                Print("Log message: " + message);
            }
        }

        public static void ClearLogFile()
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
                Print("Failed to clear log file: " + ex.Message);
            }
        }

        public static string[] GetRecentLogs(int lineCount = 100)
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
                Print("Failed to read log file: " + ex.Message);
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