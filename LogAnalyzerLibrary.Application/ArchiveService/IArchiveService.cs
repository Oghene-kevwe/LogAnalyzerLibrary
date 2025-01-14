

using LogAnalyzerLibrary.Application.DTOs;

namespace LogAnalyzerLibrary.Application.ArchiveService;

public interface IArchiveService
{
    Task<List<string>> ArchiveLogsAsync(PeriodDTO archiveDto);
    Task<string> DeleteArchiveAsync(PeriodDTO archiveDto);
}
