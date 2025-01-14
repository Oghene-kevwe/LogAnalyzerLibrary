using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Application.Shared;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogAnalyzerLibrary.Application
{
    public class LogsService : ILogsService
    {
        public async Task<string> CountTotalLogsAsync(PeriodDTO model)
        {
            int logCount = 0;

            if (Directory.Exists(model.DirectoryPath))
            {
                var logFiles = await Task.Run(() => Directory.GetFiles(model.DirectoryPath, "*.log"));
                logCount += logFiles.Count(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate));
            }

            return logCount == 0 ? "No logs found for the specified date range." : $"Total logs found: {logCount}";
        }

        public async Task<ConcurrentDictionary<string, int>> CountDuplicatedErrorsAsync(DirectoryDTO model)
        {
            var fileErrorsCount = new ConcurrentDictionary<string, int>();
            var formattedPath = model.DirectoryPath.Trim();

            try
            {
                if (Directory.Exists(formattedPath))
                {
                    var logFiles = Directory.GetFiles(formattedPath, "*.log", SearchOption.AllDirectories);

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
                    Console.WriteLine($"Directory doesn't exist: {formattedPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {formattedPath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while accessing directory: {formattedPath}. Exception: {ex.Message}");
                throw;
            }

            return fileErrorsCount;
        }

        public async Task<Dictionary<string, int>> CountUniqueErrorsAsync(DirectoryDTO model)
        {
            var fileErrorsCount = new Dictionary<string, int>();
            var formattedPath = model.DirectoryPath.Trim();

            try
            {
                if (Directory.Exists(formattedPath))
                {
                    var logFiles = Directory.GetFiles(formattedPath, "*.log", SearchOption.AllDirectories);

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
                    Console.WriteLine($"Directory doesn't exist: {formattedPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {formattedPath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while accessing directory: {formattedPath}. Exception: {ex.Message}");
                throw;
            }

            return fileErrorsCount;
        }

        public async Task<List<string>> SearchLogsAsync(DirectoryDTO model)
        {
            var logFiles = new List<string>();
            var formattedPath = model.DirectoryPath.Trim();

            try
            {
                if (Directory.Exists(formattedPath))
                {
                    var files = await Task.Run(() => Directory.GetFiles(formattedPath, "*.log", SearchOption.AllDirectories));
                    logFiles.AddRange(files);
                }
                else
                {
                    Console.WriteLine($"Directory doesn't exist: {formattedPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory with path: {formattedPath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred trying to access directory: {formattedPath}. Exception: {ex.Message}");
            }

            return logFiles;
        }

        public Task<List<string>> SearchLogsBySizeAsync(SizeRangeDTO model)
        {
            var logFiles = new List<string>();

            // Get all files in the specified directory
            var files = Directory.GetFiles(model.DirectoryPath);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileSizeKB = fileInfo.Length / 1024; // Convert bytes to KB

                if (fileSizeKB >= model.MinSizeKB && fileSizeKB <= model.MaxSizeKB)
                {
                    logFiles.Add(file);
                }
            }

            return Task.FromResult(logFiles);
        }

        public async Task<string> DeleteLogsAsync(PeriodDTO model)
        {
            int deletedLogsCount = 0;

            if (Directory.Exists(model.DirectoryPath))
            {
                var logFiles = Directory.GetFiles(model.DirectoryPath, "*.log")
                    .Where(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate))
                    .ToList();

                foreach (var logFile in logFiles)
                {
                    await Task.Run(() => File.Delete(logFile));
                    deletedLogsCount++;
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
