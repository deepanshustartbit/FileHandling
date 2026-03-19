using FileHandlingSystem;
using FileHandlingSystem.Scanner;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
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
        Console.WriteLine("4. Incremental Scan (Hash-based)");
        Console.WriteLine("5. Batch Optimized Scan (Bulk DB Insert)");
        Console.WriteLine("6. Graph API Scan (SharePoint/OneDrive)");

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

            case "4":
                RunIncrementalScan(connectionString);
                break;

            case "5":
                RunBatchOptimizedScan(connectionString);
                break;

            case "6":
                await RunGraphScan(connectionString);
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
    // Incremental scan that checks file hash to skip unchanged files
    static void RunIncrementalScan(string connectionString)
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

        int jobId = db.CreateScanJob("Incremental");
        int count = 0;

        var fileQueue = new BlockingCollection<string>(1000);

        // Producer (same as before)
        var producer = Task.Run(() =>
        {
            foreach (var file in scanner.GetFiles(path))
            {
                Console.WriteLine($"[PRODUCER] {file}");
                fileQueue.Add(file);
            }

            fileQueue.CompleteAdding();
        });

        //  Consumers (with incremental logic)
        int workerCount = Environment.ProcessorCount;

        var consumers = Enumerable.Range(0, workerCount)
            .Select(i => Task.Run(() =>
            {
                foreach (var file in fileQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        var hash = FileHelper.ComputeHash(file);
                        var lastModified = File.GetLastWriteTimeUtc(file);

                        if (db.IsFileUnchanged(file, hash))
                        {
                            Console.WriteLine($"[Worker {i}] FOUND NO CHANGES IN THIS FILE SO SKIPPED: {file} ");
                            continue;
                        }

                        Console.WriteLine($"[Worker {i}] PROCESSING: {file}");

                        db.InsertFile(jobId, file, hash, lastModified);

                        Interlocked.Increment(ref count);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {file} - {ex.Message}");
                    }
                }
            }))
            .ToArray();

        Task.WaitAll(consumers);

        db.CompleteScanJob(jobId, count);

        Console.WriteLine(" Incremental scan completed");
    }
    // Incremental scan that checks file hash to skip unchanged files
    static void RunBatchOptimizedScan(string connectionString)
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

        int jobId = db.CreateScanJob("BatchOptimized");
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

        int workerCount = Environment.ProcessorCount;

        var consumers = Enumerable.Range(0, workerCount)
            .Select(i => Task.Run(() =>
            {
                var batch = new List<FileRecord>();
                int batchSize = 100;

                foreach (var file in fileQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        var hash = FileHelper.ComputeHash(file);
                        var lastModified = File.GetLastWriteTimeUtc(file);

                        if (db.IsFileUnchanged(file, hash))
                        {
                            Console.WriteLine($"[Worker {i}] SKIPPED: {file}");
                            continue;
                        }

                        Console.WriteLine($"[Worker {i}] PROCESSING: {file}");

                        batch.Add(new FileRecord
                        {
                            JobId = jobId,
                            FilePath = file,
                            Hash = hash,
                            LastModified = lastModified
                        });

                        Interlocked.Increment(ref count);

                        // 🔥 Batch insert
                        if (batch.Count >= batchSize)
                        {
                            db.InsertFilesBulk(batch);
                            batch.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {file} - {ex.Message}");
                    }
                }

                // Insert remaining
                if (batch.Count > 0)
                {
                    db.InsertFilesBulk(batch);
                }
            }))
            .ToArray();

        Task.WaitAll(consumers);

        db.CompleteScanJob(jobId, count);

        Console.WriteLine("✅ Batch optimized scan completed");
    }
    // Scan files from Graph API (SharePoint/OneDrive)
    static async Task RunGraphScan(string connectionString)
    {
        string clientId = "d37c64b6-de86-4aa0-b22b-726eeb143760";

        var graph = new GraphService(clientId);
        var db = new DatabaseService(connectionString);

        Console.WriteLine("Fetching files from Graph API...");

        int jobId = db.CreateScanJob("GraphScan");
        int count = 0;

        var files = await graph.GetFilesAsync();

        foreach (var file in files)
        {
            Console.WriteLine($"[GRAPH FILE] {file}");

            //Insert into DB (using your old method)
            db.InsertFile(jobId, file);

            count++;
        }

        db.CompleteScanJob(jobId, count);

        Console.WriteLine("Graph scan completed");
    }
}