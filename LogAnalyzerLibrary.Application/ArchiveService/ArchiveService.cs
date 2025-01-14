using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Application.Shared;
using System.IO.Compression;

namespace LogAnalyzerLibrary.Application.ArchiveService;

public class ArchiveService : IArchiveService
{
    public async Task<List<string>> ArchiveLogsAsync(PeriodDTO model)
    {
        // List to store paths of created zip files
        List<string> createdZipFiles = [];

        // Iterate over each directory path provided
        foreach (var directoryPath in model.DirectoryPaths)
        {
            if (Directory.Exists(directoryPath))
            {
                // Get log files within the date range from the current directory
                var logFiles = Directory.GetFiles(directoryPath, "*.log")
                    .Where(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate))
                    .ToList();

                if (logFiles.Count > 0)
                {
                    // Define the zip file name based on the date range and directory
                    string zipFileName = $"{model.StartDate:dd_MM_yyyy}-{model.EndDate:dd_MM_yyyy}.zip";
                    string zipFilePath = Path.Combine(directoryPath, zipFileName);

                    // Create the zip file
                    await Task.Run(() =>
                    {
                        using var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
                        foreach (var logFile in logFiles)
                        {
                            zipArchive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                            File.Delete(logFile); // Delete the original log file after adding to the zip
                        }
                    });

                    createdZipFiles.Add(zipFilePath);
                }
            }
        }

        if (createdZipFiles.Count == 0)
        {
            return ["No logs found for the specified date range."];
        }

        return createdZipFiles;
    }


    public async Task<string> DeleteArchiveAsync(PeriodDTO model)
    {
        bool anyFilesDeleted = false;
        List<string> deletedFiles = [];

        foreach (var directoryPath in model.DirectoryPaths)
        {
            await Task.Run(() =>
            {
                // Get all zip files in the directory and its subdirectories
                var zipFiles = Directory.GetFiles(directoryPath, "*.zip", SearchOption.AllDirectories);

                foreach (var zipFile in zipFiles)
                {
                    // Check if the file creation date is within the specified date range
                    if (FileInDateRange.IsFileInDateRange(zipFile, model.StartDate, model.EndDate))
                    {
                        // Delete the file
                        File.Delete(zipFile);
                        anyFilesDeleted = true;

                        // Add the name of the deleted file to the list
                        deletedFiles.Add(Path.GetFileName(zipFile));
                    }
                }
            });
        }

        if (!anyFilesDeleted)
        {
            throw new FileNotFoundException("No zip files found in the specified date range.");
        }

        // Return the names of the deleted archives
        return $"Archive deleted successfully. Deleted files: {string.Join(", ", deletedFiles)}";

    }
}
