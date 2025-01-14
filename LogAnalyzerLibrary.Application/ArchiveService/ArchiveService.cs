using LogAnalyzerLibrary.Application.DTOs;
using LogAnalyzerLibrary.Application.Shared;
using System.IO.Compression;

namespace LogAnalyzerLibrary.Application.ArchiveService
{
    public class ArchiveService : IArchiveService
    {
        public async Task<List<string>> ArchiveLogsAsync(PeriodDTO model)
        {
            List<string> createdZipFiles = [];

            var directoryPath = model.DirectoryPath;
            if (Directory.Exists(directoryPath))
            {
                var logFiles = Directory.GetFiles(directoryPath, "*.log")
                    .Where(file => FileInDateRange.IsFileInDateRange(file, model.StartDate, model.EndDate))
                    .ToList();

                if (logFiles.Count > 0)
                {
                    string zipFileName = $"{model.StartDate:dd_MM_yyyy}-{model.EndDate:dd_MM_yyyy}.zip";
                    string zipFilePath = Path.Combine(directoryPath, zipFileName);

                    await Task.Run(() =>
                    {
                        using var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
                        foreach (var logFile in logFiles)
                        {
                            zipArchive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                            File.Delete(logFile);
                        }
                    });

                    createdZipFiles.Add(zipFilePath);
                }
            }

            if (createdZipFiles.Count == 0)
            {
                return new List<string> { "No logs found for the specified date range." };
            }

            return createdZipFiles;
        }

        public async Task<string> DeleteArchiveAsync(PeriodDTO model)
        {
            bool anyFilesDeleted = false;
            List<string> deletedFiles = new List<string>();

            var directoryPath = model.DirectoryPath;
            if (Directory.Exists(directoryPath))
            {
                await Task.Run(() =>
                {
                    var zipFiles = Directory.GetFiles(directoryPath, "*.zip", SearchOption.AllDirectories);

                    foreach (var zipFile in zipFiles)
                    {
                        if (FileInDateRange.IsFileInDateRange(zipFile, model.StartDate, model.EndDate))
                        {
                            File.Delete(zipFile);
                            anyFilesDeleted = true;
                            deletedFiles.Add(Path.GetFileName(zipFile));
                        }
                    }
                });
            }

            if (!anyFilesDeleted)
            {
                throw new FileNotFoundException("No zip files found in the specified date range.");
            }

            return $"Archive deleted successfully. Deleted files: {string.Join(", ", deletedFiles)}";
        }
    }
}
