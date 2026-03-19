# 🚀 File Handling System (High-Performance File Scanner)

## 📌 Why This Project?

In real-world systems like antivirus software, backup tools, and data processing pipelines, there is a need to scan large volumes of files across deeply nested directories.

A basic approach (looping through folders) becomes:

* Slow ❌
* Blocking ❌
* Not scalable ❌

👉 This project is built to demonstrate how to design a **high-performance and scalable file scanning system** using real-world backend concepts.

---

## 🎯 Problem Statement

* Scan all files in a directory (including nested folders)
* Process files efficiently without blocking
* Improve performance using parallel processing
* Track execution and results using a database

---

## ⚙️ System Flow

User provides a directory path →
System scans directories recursively →
Files are added to a queue →
Workers process files →
Results are stored in database

```
Input Path
   ↓
DFS Scanner
   ↓
Queue (BlockingCollection)
   ↓
Workers (Parallel Processing)
   ↓
Database Logging
```

---

## 🔥 Features Implemented

### ✅ 1. Recursive File Scanning (DFS)

* Uses Depth First Search (DFS)
* Traverses all nested directories
* Ensures complete file discovery

👉 Example:

```
Root → Folder → SubFolder → File
```

---

### ✅ 2. Producer–Consumer Pattern

* Producer → Finds files
* Queue → Stores files
* Consumer → Processes files

👉 Benefits:

* Decouples scanning from processing
* Improves performance
* Enables scalability

---

### ✅ 3. Multi-threaded Processing

* Multiple workers process files simultaneously
* Worker count based on system CPU:

```csharp
Environment.ProcessorCount
```

👉 Benefits:

* Faster execution
* Better CPU utilization

---

### ✅ 4. Database Tracking

* Tracks scan jobs and processed files
* Stores:

  * Scan start time
  * Scan completion time
  * File paths processed
  * Status

👉 Useful for:

* Monitoring
* Debugging
* Auditing

---

## 🏗️ Architecture

```
DFS Scanner → Queue → Multi-threaded Workers → Database
```

---

## 🧠 Key Concepts Used

* Depth First Search (DFS)
* Producer–Consumer Pattern
* Multi-threading (Task Parallelism)
* Thread-safe operations (`Interlocked`)
* Database operations using Dapper

---

## 🗄️ Database Structure

### 📌 ScanJobs

Stores overall scan execution details:

* Id
* ScanType
* StartTime
* EndTime
* Status
* TotalFiles

---

### 📌 ScanFiles

Stores processed file details:

* Id
* JobId
* FilePath
* Status
* ProcessedTime

---

## ⚙️ Configuration

Connection string is stored in:

```
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=FileScannerDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## 🚀 How to Run

### 1️⃣ Restore packages

```bash
dotnet restore
```

### 2️⃣ Run project

```bash
dotnet run
```

### 3️⃣ Choose mode

```
1. Simple DFS Scan
2. Producer-Consumer Scan
3. Multi-threaded Scan
```

---

## 🧪 Testing Example

Create folder structure:

```
C:\ScannerTest
 ├── Folder1
 ├── Folder2
 │    └── SubFolder
 │         └── file.txt
```

---

## ⚡ Performance Highlights

* Bounded queue prevents memory overflow
* Multi-threading improves speed
* Separation of concerns improves scalability

---

## 🧠 Real-World Use Cases

This system design is used in:

* Antivirus engines
* Backup & restore systems
* File indexing systems
* Data pipelines

---

## 🚀 Future Enhancements

* File hashing (SHA256)
* Incremental scanning (skip unchanged files)
* File type filtering
* Cloud scanning (SharePoint / OneDrive)

---

## 👨‍💻 Summary

This project demonstrates how to build a **scalable and high-performance file processing system** using .NET.

It focuses on:

* Efficient file traversal
* Parallel processing
* Clean architecture
* Real-world system design concepts
