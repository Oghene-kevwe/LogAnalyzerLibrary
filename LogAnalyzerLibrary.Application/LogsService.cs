using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Application.Shared;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogAnalyzerLibrary.Application
{
    public class LogsService : ILogsService
    {

        public async Task<string> CountTotalLogsAsync(PeriodDirectoryListDTO model)
        {
            int logCount = 0;

            // Iterate over multiple directory paths
            foreach (var directoryPath in model.DirectoryCollection)
            {
                if (Directory.Exists(directoryPath))
                {
                    // Get all log files in the directory
                    var logFiles = await Task.Run(() => Directory.GetFiles(directoryPath, "*.log", SearchOption.AllDirectories));

                    // Count files within the date range
                    logCount += logFiles.Count(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate));
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory {directoryPath}: not found");
                }
            }

            return logCount == 0 ? "No logs found for the specified date range." : $"Total logs found: {logCount}";
        }

        public async Task<ConcurrentDictionary<string, int>> CountDuplicatedErrorsAsync(DirectoryDTO model)
        {
            var fileErrorsCount = new ConcurrentDictionary<string, int>();


            if (Directory.Exists(model.DirectoryPath))
            {
                var logFiles = Directory.GetFiles(model.DirectoryPath, "*.log", SearchOption.AllDirectories);

                var logFileTasks = logFiles.Select(async logFile =>
                {
                    var errorOccurrences = new ConcurrentDictionary<string, int>();
                    var logLines = await File.ReadAllLinesAsync(logFile);

                    foreach (var line in logLines)
                    {
                        var match = dateTimePattern.Match(line);

                        if (match.Success)
                        {
                            var errorMessage = ParseLogLine(line);
                            errorOccurrences.AddOrUpdate(errorMessage, 1, (key, count) => count + 1);
                        }
                    }

                    var duplicatedErrors = errorOccurrences.Values.Count(x => x > 1);
                    fileErrorsCount[logFile] = duplicatedErrors;
                });

                await Task.WhenAll(logFileTasks);
            }
            else
            {
                Console.Write($"Directory doesn't exist: {model.DirectoryPath}");
                throw new DirectoryNotFoundException($"Directory {model.DirectoryPath}: not found");
            }

            return fileErrorsCount;
        }

        public async Task<Dictionary<string, int>> CountUniqueErrorsAsync(DirectoryDTO model)
        {
            var fileErrorsCount = new Dictionary<string, int>();

            if (Directory.Exists(model.DirectoryPath))
            {
                var logFiles = Directory.GetFiles(model.DirectoryPath, "*.log", SearchOption.AllDirectories);

                foreach (var logFile in logFiles)
                {
                    var uniqueErrors = new HashSet<string>();
                    var logLines = await File.ReadAllLinesAsync(logFile);

                    foreach (var line in logLines)
                    {
                        var match = dateTimePattern.Match(line);

                        if (match.Success)
                        {
                            var errorMessage = ParseLogLine(line);
                            uniqueErrors.Add(errorMessage);
                        }
                    }

                    fileErrorsCount[logFile] = uniqueErrors.Count;
                }
            }
            else
            {

                Console.WriteLine($"Directory doesn't exist: {model.DirectoryPath}");
                throw new DirectoryNotFoundException($"Directory {model.DirectoryPath}: not found");
            }

            return fileErrorsCount;
        }

        public async Task<List<string>> SearchLogsAsync(DirectoryDTO model)
        {
            var logFiles = new List<string>();


            try
            {
                if (Directory.Exists(model.DirectoryPath))
                {
                    var files = await Task.Run(() => Directory.GetFiles(model.DirectoryPath, "*.log", SearchOption.AllDirectories));
                    logFiles.AddRange(files);
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory {model.DirectoryPath} not found");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory with path: {model.DirectoryPath}. Exception: {ex.Message}");
                throw new UnauthorizedAccessException($"Access denied to directory with path: {model.DirectoryPath}. Exception: {ex.Message}");
            }

            return logFiles;
        }

        public Task<List<string>> SearchLogsBySizeAsync(SizeRangeDTO model)
        {
            var logFiles = new List<string>();

            try
            {
                // Get all files in the specified directory
                if (Directory.Exists(model.DirectoryPath))
                {
                    var files = Directory.GetFiles(model.DirectoryPath, "*.log", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        var fileSizeKB = fileInfo.Length / 1024; // Convert bytes to KB

                        if (fileSizeKB >= model.MinSizeKB && fileSizeKB <= model.MaxSizeKB)
                        {
                            logFiles.Add(file);
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory {model.DirectoryPath} not found");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory with path: {model.DirectoryPath}. Exception: {ex.Message}");
                throw new UnauthorizedAccessException($"Access denied to directory with path: {model.DirectoryPath}. Exception: {ex.Message}");
            }



            return Task.FromResult(logFiles);
        }

        public async Task<string> DeleteLogsAsync(PeriodDirectoryListDTO model)
        {
            int deletedLogsCount = 0;

            // Iterate over each directory path in the collection
            foreach (var directoryPath in model.DirectoryCollection)
            {
                if (Directory.Exists(directoryPath))
                {
                    // Get all log files in the directory and subdirectories that match the date range
                    var logFiles = Directory.GetFiles(directoryPath, "*.log", SearchOption.AllDirectories)
                        .Where(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate))
                        .ToList();

                    // Delete each log file
                    foreach (var logFile in logFiles)
                    {
                        await Task.Run(() => File.Delete(logFile));
                        deletedLogsCount++;
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory {directoryPath} not found");
                }
            }

            return deletedLogsCount == 0 ? "No logs found for the specified date range." : $"{deletedLogsCount} log(s) successfully deleted.";
        }


        private static readonly Regex dateTimePattern = new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}(:\d{4})?\s+", RegexOptions.Compiled);

        private static string ParseLogLine(string logLine)
        {
            string cleanedLine = dateTimePattern.Replace(logLine, "");
            cleanedLine = Regex.Replace(cleanedLine, @"\d+\.\d+\.\d+\.\d+", "");
            return cleanedLine.Trim();
        }


    }
}
