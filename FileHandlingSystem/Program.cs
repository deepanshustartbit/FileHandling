using FileHandlingSystem;
using FileHandlingSystem.Scanner;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration config = builder.Build();

        string connectionString = config.GetConnectionString("DefaultConnection")!;

        Console.WriteLine("Choose option: ");
        Console.WriteLine("1. Simple DFS Scan");
        Console.WriteLine("2. Producer-Consumer Scan");
        Console.WriteLine("3. Multi-threaded Scan (Parallel Workers)");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                RunSimpleDFS();
                break;

            case "2":
                RunProducerConsumer();
                break;

            case "3":
                RunMultiThreadedScan(connectionString);
                break;

            default:
                Console.WriteLine("Invalid option");
                break;
        }
    }

    // Simple DFS scan (single-threaded) 
    static void RunSimpleDFS()
    {
        var scanner = new FileScannerService();

        Console.WriteLine("Enter path:");
        var path = Console.ReadLine();

        if (!Directory.Exists(path))
        {
            Console.WriteLine("Invalid path");
            return;
        }

        foreach (var file in scanner.GetFiles(path))
        {
            Console.WriteLine(file);
        }
    }

    // Producer-Consumer pattern (single producer, single consumer)
    static void RunProducerConsumer()
    {
        var scanner = new FileScannerService();

        Console.WriteLine("Enter path:");
        var path = Console.ReadLine();

        if (!Directory.Exists(path))
        {
            Console.WriteLine("Invalid path");
            return;
        }

        var fileQueue = new BlockingCollection<string>(1000);

        // Producer
        var producer = Task.Run(() =>
        {
            foreach (var file in scanner.GetFiles(path))
            {
                Console.WriteLine($"[PRODUCER] Found: {file}");
                fileQueue.Add(file);
            }

            fileQueue.CompleteAdding();
        });

        // Consumer
        var consumer = Task.Run(() =>
        {
            foreach (var file in fileQueue.GetConsumingEnumerable())
            {
                Console.WriteLine($"Processing: {file}");
            }
        });

        Task.WaitAll(producer, consumer);

        Console.WriteLine("Scan completed");
    }

    // Multi-threaded scan with multiple consumer workers
    static void RunMultiThreadedScan(string connectionString)
    {
        var scanner = new FileScannerService();

        var db = new DatabaseService(connectionString);
        Console.WriteLine("Enter path:");
        var path = Console.ReadLine();

        if (!Directory.Exists(path))
        {
            Console.WriteLine("Invalid path");
            return;
        }
        int jobId = db.CreateScanJob("MultiThreaded");
        int count = 0;

        var fileQueue = new BlockingCollection<string>(1000);

        // Producer
        var producer = Task.Run(() =>
        {
            foreach (var file in scanner.GetFiles(path))
            {
                Console.WriteLine($"[PRODUCER] {file}");
                fileQueue.Add(file);
            }

            fileQueue.CompleteAdding();
        });

        // Multi-threaded Consumers
        int workerCount = Environment.ProcessorCount;

        var consumers = Enumerable.Range(0, workerCount)
            .Select(i => Task.Run(() =>
            {
                foreach (var file in fileQueue.GetConsumingEnumerable())
                {
                    Console.WriteLine($"[Worker {i}] Processing: {file}");
                    db.InsertFile(jobId, file);
                    Interlocked.Increment(ref count);
                    Thread.Sleep(200);
                }
            }))
            .ToArray();

        Task.WaitAll(consumers);
        db.CompleteScanJob(jobId, count);

        Console.WriteLine(" Multi-threaded scan completed");
    }
}