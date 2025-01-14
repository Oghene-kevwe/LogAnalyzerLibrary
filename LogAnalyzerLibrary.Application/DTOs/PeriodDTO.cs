namespace LogAnalyzerLibrary.Application.DTOs;

public class PeriodDTO : DirectoriesDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}