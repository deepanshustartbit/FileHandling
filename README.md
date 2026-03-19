# 🚀 File Handling System (Enterprise File Scanner)

## 📌 Overview

This project is a high-performance file scanning system built using .NET, designed to efficiently process large volumes of files from multiple sources including:

* Local file systems
* Network-based storage (CIFS/NFS ready)
* Cloud storage via Microsoft Graph API (OneDrive)

The system demonstrates real-world backend design patterns used in enterprise applications such as antivirus engines, backup systems, and data processing pipelines.

---

## 🎯 Problem Statement

Traditional file scanning approaches:

* Process files sequentially ❌
* Re-scan unchanged files ❌
* Cause performance bottlenecks ❌

This system solves these issues by:

* Using parallel processing
* Avoiding redundant scans
* Optimizing database operations
* Supporting scalable architecture

---

## ⚙️ System Architecture

```
Input Source (Local / Cloud)
        ↓
DFS Scanner / Graph API
        ↓
Queue (BlockingCollection)
        ↓
Workers (Parallel Processing)
        ↓
Processing Logic
        ↓
Database Logging
```

---

## 🔥 Features Implemented

### ✅ 1. Recursive File Scanning (DFS)

* Traverses all nested directories
* Uses Depth First Search (DFS)
* Memory-efficient using `yield return`

---

### ✅ 2. Producer–Consumer Pattern

* Separates file discovery and processing
* Uses `BlockingCollection`
* Prevents bottlenecks

---

### ✅ 3. Multi-threaded Processing

* Parallel file processing using multiple workers
* Optimized with:

  ```csharp
  Environment.ProcessorCount
  ```

---

### ✅ 4. Incremental Scan (Hash-Based)

* Uses SHA256 hashing
* Skips unchanged files
* Processes only modified/new files

---

### ✅ 5. Batch Optimized Database Inserts

* Reduces database calls
* Inserts records in bulk
* Improves performance significantly

---

### ✅ 6. Cloud File Scanning (Microsoft Graph API) 🔥

* Fetches files from OneDrive
* Uses delegated authentication
* Supports personal Microsoft accounts
* Integrates with existing processing pipeline

---

## 🧠 Key Concepts Used

* Depth First Search (DFS)
* Producer–Consumer Pattern
* Multi-threading (Task Parallelism)
* Thread-safe operations (`Interlocked`)
* File hashing (SHA256)
* Batch processing
* Microsoft Graph API
* Delegated Authentication (OAuth2)

---

## 🗄️ Database Design

### 📌 ScanJobs

Tracks scan execution:

| Column     | Description          |
| ---------- | -------------------- |
| Id         | Job ID               |
| ScanType   | Type of scan         |
| StartTime  | Start time           |
| EndTime    | Completion time      |
| Status     | Running / Completed  |
| TotalFiles | Processed file count |

---

### 📌 ScanFiles

Tracks processed files:

| Column        | Description             |
| ------------- | ----------------------- |
| Id            | File ID                 |
| JobId         | Reference to ScanJobs   |
| FilePath      | File location           |
| FileHash      | SHA256 hash             |
| LastModified  | Last modified timestamp |
| Status        | Processed               |
| ProcessedTime | Timestamp               |

---

## ⚙️ Configuration

Stored in:

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

### 1️⃣ Restore dependencies

```
dotnet restore
```

### 2️⃣ Run application

```
dotnet run
```

### 3️⃣ Choose mode

```
1. Simple DFS Scan
2. Producer-Consumer Scan
3. Multi-threaded Scan
4. Incremental Scan
5. Batch Optimized Scan
6. Graph API Scan (Cloud)
```

---

## 🔐 Graph API Setup (Personal Account)

* Use **Delegated Authentication**
* Required permissions:

  * `Files.Read`
  * `User.Read`
* Configure redirect URI:

  ```
  http://localhost
  ```
* Enable:

  ```
  Accounts in any directory + personal accounts
  ```

---

## 🧪 Testing

### Local Testing

```
C:\ScannerTest
 ├── file1.txt
 ├── file2.log
 └── Folder
      └── file3.txt
```

---

### Graph Testing

Upload files to:

```
https://onedrive.live.com
```

---

## ⚡ Performance Highlights

* Multi-threading improves throughput
* Batch inserts reduce DB load
* Incremental scanning avoids redundant processing
* Scalable architecture for large datasets

---

## 🧠 Real-World Use Cases

This system can be used in:

* Antivirus and malware scanning
* Backup and recovery systems
* Data classification engines
* Cloud storage analysis tools
* Compliance and security auditing

---

## 🚀 Future Enhancements

* Graph API pagination & throttling handling
* Distributed scanning (multi-node architecture)
* Regex-based content scanning (sensitive data detection)
* Angular dashboard for visualization
* ElasticSearch integration for indexing

---

## 👨‍💻 Summary

This project demonstrates how to build a **scalable, high-performance, enterprise-grade file scanning system** using modern .NET practices.

It focuses on:

* Performance optimization
* Clean architecture
* Real-world system design
* Cloud integration
