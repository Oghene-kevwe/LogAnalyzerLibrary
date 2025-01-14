

using LogAnalyzerLibrary.Application.DTOs;
using System.Collections.Concurrent;

namespace LogAnalyzerLibrary.Application;

public interface ILogsService
{
    Task<List<string>> SearchLogsAsync(DirectoryDTO model);
    Task<Dictionary<string, int>> CountUniqueErrorsAsync(DirectoryDTO model);
    Task<ConcurrentDictionary<string, int>> CountDuplicatedErrorsAsync(DirectoryDTO model);
    Task<string> DeleteLogsAsync(PeriodDirectoryListDTO model);
    Task<string> CountTotalLogsAsync(PeriodDirectoryListDTO model);
    Task<List<string>> SearchLogsBySizeAsync(SizeRangeDTO model);
}
