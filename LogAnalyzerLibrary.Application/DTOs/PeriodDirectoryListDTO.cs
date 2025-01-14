namespace LogAnalyzerLibrary.Application.DTOs;

public class PeriodDirectoryListDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required IEnumerable<string> DirectoryCollection { get; set; }
}
