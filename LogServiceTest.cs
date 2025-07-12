using System;
using System.Threading;

namespace AGServer
{
    public class LogServiceTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== LogService Test ===");
            
            // Configure the log service
            LogService.Instance.LogFilePath = "test.log";
            LogService.Instance.MinimumLevel = LogLevel.Debug;
            LogService.Instance.EnableConsoleOutput = true;
            LogService.Instance.EnableFileOutput = true;
            
            // Start the log service
            LogService.Instance.Start();
            
            // Test different log levels
            LogService.Instance.Debug("This is a debug message");
            LogService.Instance.Info("This is an info message");
            LogService.Instance.Warning("This is a warning message");
            LogService.Instance.Error("This is an error message");
            LogService.Instance.Fatal("This is a fatal message");
            
            // Test logging with exception
            try
            {
                throw new InvalidOperationException("Test exception for logging");
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("Caught an exception", ex);
            }
            
            // Test multi-threaded logging
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                Thread thread = new Thread(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        LogService.Instance.Info(string.Format("Thread {0} - Message {1}", threadId, j));
                        Thread.Sleep(100);
                    }
                });
                thread.Start();
            }
            
            // Wait for threads to complete
            Thread.Sleep(1000);
            
            // Test reading recent logs
            Console.WriteLine("\n=== Recent Logs ===");
            string[] recentLogs = LogService.Instance.GetRecentLogs(10);
            foreach (string log in recentLogs)
            {
                Console.WriteLine(log);
            }
            
            // Stop the log service
            LogService.Instance.Stop();
            
            Console.WriteLine("\n=== LogService Test Complete ===");
        }
    }
} 