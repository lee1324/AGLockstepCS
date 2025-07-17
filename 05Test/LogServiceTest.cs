using System;
using System.Threading;

namespace AGSyncCS
{
    public class LogServiceTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== LogService Test ===");
            
            // Configure the log service
            Logger.Instance.LogFilePath = "test.log";
            Logger.Instance.MinimumLevel = LogLevel.Debug;
            Logger.Instance.EnableConsoleOutput = true;
            Logger.Instance.EnableFileOutput = true;
            
            // Start the log service
            Logger.Instance.Start();
            
            // Test different log levels
            Logger.Instance.Debug("This is a debug message");
            Logger.Instance.Info("This is an info message");
            Logger.Instance.Warning("This is a warning message");
            Logger.Instance.Error("This is an error message");
            Logger.Instance.Fatal("This is a fatal message");
            
            // Test logging with exception
            try
            {
                throw new InvalidOperationException("Test exception for logging");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Caught an exception", ex);
            }
            
            // Test multi-threaded logging
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                Thread thread = new Thread(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Logger.Instance.Info(string.Format("Thread {0} - Message {1}", threadId, j));
                        Thread.Sleep(100);
                    }
                });
                thread.Start();
            }
            
            // Wait for threads to complete
            Thread.Sleep(1000);
            
            // Test reading recent logs
            Console.WriteLine("\n=== Recent Logs ===");
            string[] recentLogs = Logger.Instance.GetRecentLogs(10);
            foreach (string log in recentLogs)
            {
                Console.WriteLine(log);
            }
            
            // Stop the log service
            Logger.Instance.Stop();
            
            Console.WriteLine("\n=== LogService Test Complete ===");
        }
    }
} 