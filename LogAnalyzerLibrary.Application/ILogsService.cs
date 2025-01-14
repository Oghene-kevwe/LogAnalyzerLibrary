

using LogAnalyzerLibrary.Application.DTOs;
using System.Collections.Concurrent;

namespace LogAnalyzerLibrary.Application;

public interface ILogsService
{
    Task<List<string>> SearchLogsAsync(DirectoriesDTO model);
    Task<Dictionary<string, int>> CountUniqueErrorsAsync(DirectoriesDTO model);
    Task<ConcurrentDictionary<string, int>> CountDuplicatedErrorsAsync(DirectoriesDTO model);
    Task<string> DeleteLogsAsync(PeriodDTO archiveDto);
    Task<string> CountTotalLogsAsync(PeriodDTO archiveDto);
    void UploadLogs();
}
