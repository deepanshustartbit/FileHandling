using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FileHandlingSystem
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        public int CreateScanJob(string scanType)
        {
            using var connection = CreateConnection();

            var sql = @"INSERT INTO ScanJobs (ScanType, StartTime, Status)
                    VALUES (@ScanType, GETDATE(), 'Running');
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            return connection.QuerySingle<int>(sql, new { ScanType = scanType });
        }

        public void CompleteScanJob(int jobId, int totalFiles)
        {
            using var connection = CreateConnection();

            var sql = @"UPDATE ScanJobs 
                    SET EndTime = GETDATE(),
                        Status = 'Completed',
                        TotalFiles = @TotalFiles
                    WHERE Id = @Id";

            connection.Execute(sql, new { Id = jobId, TotalFiles = totalFiles });
        }

        public void InsertFile(int jobId, string filePath)
        {
            using var connection = CreateConnection();

            var sql = @"INSERT INTO ScanFiles (JobId, FilePath, Status, ProcessedTime)
                    VALUES (@JobId, @FilePath, 'Processed', GETDATE())";

            connection.Execute(sql, new { JobId = jobId, FilePath = filePath });
        }
        // calling this for 4th senario where we want to check if file is unchanged and skip processing
        public void InsertFile(int jobId, string filePath, string hash, DateTime lastModified)
        {
            using var connection = CreateConnection();

            var sql = @"INSERT INTO ScanFiles 
        (JobId, FilePath, FileHash, LastModified, Status, ProcessedTime)
        VALUES (@JobId, @FilePath, @Hash, @LastModified, 'Processed', GETDATE())";

            connection.Execute(sql, new
            {
                JobId = jobId,
                FilePath = filePath,
                Hash = hash,
                LastModified = lastModified
            });
        }
        public bool IsFileUnchanged(string filePath, string hash)
        {
            using var connection = CreateConnection();

            var sql = @"SELECT COUNT(1)
                FROM ScanFiles
                WHERE FilePath = @FilePath
                  AND FileHash = @Hash";

            return connection.ExecuteScalar<int>(sql, new { FilePath = filePath, Hash = hash }) > 0;
        }
    }
}
