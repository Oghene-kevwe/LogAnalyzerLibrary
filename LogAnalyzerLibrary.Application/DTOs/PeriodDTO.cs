namespace LogAnalyzerLibrary.Application.DTOs;

public class PeriodDTO : DirectoryDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}