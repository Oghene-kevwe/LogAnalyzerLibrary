using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Application.Shared;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LogAnalyzerLibrary.Application;

public class LogsService : ILogsService
{

    public async Task<string> CountTotalLogsAsync(PeriodDTO archiveDto)
    {
        int logCount = 0;

        // Iterate over the directories provided in the ArchiveDTO
        foreach (var directoryPath in archiveDto.DirectoryPaths)
        {
            if (Directory.Exists(directoryPath))
            {
                // Get all log files in the directory
                var logFiles = await Task.Run(() => Directory.GetFiles(directoryPath, "*.log"));

                // Count files in the date range
                logCount += logFiles.Count(file => FileInDateRange.IsFileInDateRange(file, archiveDto.StartDate, archiveDto.EndDate));
            }
        }

        if (logCount == 0)
        {
            return "No logs found for the specified date range.";
        }

        return $"Total logs found: {logCount}";
    }


    public async Task<ConcurrentDictionary<string, int>> CountDuplicatedErrorsAsync(DirectoriesDTO model)
    {

        var fileErrorsCount = new ConcurrentDictionary<string, int>();

        var tasks = model.DirectoryPaths.Select(async path =>
         {
             var formatedPath = path.Trim();
             try
             {
                 if (Directory.Exists(formatedPath))
                 {
                     //retrieve all log files from directory
                     var logFiles = Directory.GetFiles(formatedPath, "*.log", SearchOption.AllDirectories);

                     // process each log file 
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
                                 if (errorOccurrences.ContainsKey(errorMessage))
                                 {
                                     errorOccurrences[errorMessage]++;
                                 }
                                 else
                                 {
                                     errorOccurrences[errorMessage] = 1;
                                 }
                             }
                         }

                         var duplicatedErrors = errorOccurrences.Values.Count(x => x > 1);

                         fileErrorsCount[logFile] = duplicatedErrors;
                     });

                     await Task.WhenAll(logFileTasks);
                 }
                 else
                 {
                     Console.WriteLine($"Directory doesn't exist: {formatedPath}");
                 }
             }
             catch (UnauthorizedAccessException ex)
             {
                 Console.WriteLine($"Access denied to directory: {formatedPath}. Exception: {ex.Message}");
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"An error occured while accessing directory:{formatedPath}. Exception:{ex.Message}");
                 throw;
             }
         }).ToList();

        await Task.WhenAll(tasks);

        return fileErrorsCount;
    }

    public async Task<Dictionary<string, int>> CountUniqueErrorsAsync(DirectoriesDTO model)
    {
        var fileErrorsCount = new Dictionary<string, int>();

        foreach (var path in model.DirectoryPaths)
        {
            var formatedPath = path.Trim();
            try
            {
                if (Directory.Exists(formatedPath))
                {
                    //retrieve all log files from directory
                    var logFiles = Directory.GetFiles(formatedPath, "*.log", SearchOption.AllDirectories);

                    // process each log file
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
                    Console.WriteLine($"Directory doesn't exist: {formatedPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {formatedPath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured while accessing directory:{formatedPath}. Exception:{ex.Message}");
                throw;
            }
        }

        return fileErrorsCount;
    }

    public async Task<List<string>> SearchLogsAsync(DirectoriesDTO model)
    {
        var logFiles = new List<string>();

        foreach (var path in model.DirectoryPaths)
        {

            var formatedPath = path.Trim();

            try
            {
                if (Directory.Exists(formatedPath))
                {
                    var files = await Task.Run(() => Directory.GetFiles(formatedPath, "*.log", SearchOption.AllDirectories));

                    logFiles.AddRange(files);
                }
                else
                {
                    Console.WriteLine($"Directory doesn't exist:{formatedPath}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory with path :{formatedPath}. Exception: {ex.Message}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured trying to access direcory:{formatedPath}. Exception: {ex.Message}");
            }

        }
        return logFiles;
    }

    public void UploadLogs()
    {
        throw new NotImplementedException();
    }


    public async Task<string> DeleteLogsAsync(PeriodDTO model)
    {
        int deletedLogsCount = 0;

        // Iterate through each specified directory
        foreach (var directoryPath in model.DirectoryPaths)
        {
            if (Directory.Exists(directoryPath))
            {
                // Get log files within the date range from the current directory
                var logFiles = Directory.GetFiles(directoryPath, "*.log")
                    .Where(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate))
                    .ToList();

                // Delete each log file within the date range
                foreach (var logFile in logFiles)
                {
                    await Task.Run(() => File.Delete(logFile));
                    deletedLogsCount++;
                }
            }
        }

        // Return a message based on the number of deleted logs
        if (deletedLogsCount == 0)
        {
            return "No logs found for the specified date range.";
        }

        return $"{deletedLogsCount} log(s) successfully deleted from the specified directories.";
    }

    private static readonly Regex dateTimePattern = new Regex(@"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}(:\d{4})?\s+", RegexOptions.Compiled);

    private static string ParseLogLine(string logLine)
    {
        // Remove the timestamp and leading whitespace
        string cleanedLine = dateTimePattern.Replace(logLine, "");

        cleanedLine = Regex.Replace(cleanedLine, @"\d+\.\d+\.\d+\.\d+", ""); // Remove version numbers

        // Trim any extra spaces at the beginning or end
        return cleanedLine.Trim();
    }

}
