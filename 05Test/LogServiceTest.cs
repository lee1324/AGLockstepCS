using System;
using System.Threading;

namespace AGSyncCS
{
    public class LogServiceTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== LogService Test ===");
            
            // Configure the logger for testing
            Logger.LogFilePath = "test.log";
            Logger.MinimumLevel = LogLevel.Debug;
            Logger.EnableConsoleOutput = true;
            Logger.EnableFileOutput = true;

            // Start the logger
            Logger.Start();

            // Basic logging
            Logger.Debug("This is a debug message");
            Logger.Info("This is an info message");
            Logger.Warning("This is a warning message");
            Logger.Error("This is an error message");
            Logger.Fatal("This is a fatal message");

            // Logging with exception
            try
            {
                throw new InvalidOperationException("This is a test exception.");
            }
            catch (Exception ex)
            {
                Logger.Error("Caught an exception", ex);
            }

            // Test multi-threaded logging
            for (int i = 0; i < 5; i++)
            {
                int threadId = i;
                Thread t = new Thread(() => {
                    for (int j = 0; j < 5; j++)
                    {
                        Logger.Info(string.Format("Thread {0} - Message {1}", threadId, j));
                        Thread.Sleep(10);
                    }
                });
                t.Start();
            }

            Thread.Sleep(2000); // Wait for threads to finish

            // Test retrieving recent logs
            string[] recentLogs = Logger.GetRecentLogs(10);
            Console.WriteLine("\n--- Recent Logs (last 10) ---");
            foreach (var log in recentLogs)
            {
                Console.WriteLine(log);
            }

            // Stop the logger
            Logger.Stop();
            
            Console.WriteLine("\n=== LogService Test Complete ===");
        }
    }
} 