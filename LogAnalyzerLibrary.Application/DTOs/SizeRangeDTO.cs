namespace LogAnalyzerLibrary.Application.DTOs;

public class SizeRangeDTO:DirectoryDTO
{
    public int MinSizeKB { get; set; }
    public int MaxSizeKB { get; set; }
}
