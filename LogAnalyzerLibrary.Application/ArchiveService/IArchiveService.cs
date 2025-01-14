

using LogAnalyzerLibrary.Application.DTOs;

namespace LogAnalyzerLibrary.Application.ArchiveService;

public interface IArchiveService
{
    Task<List<string>> ArchiveLogsAsync(PeriodDTO model);
    Task<string> DeleteArchiveAsync(PeriodDTO model);
}
