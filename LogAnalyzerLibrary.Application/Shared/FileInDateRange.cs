

namespace LogAnalyzerLibrary.Application.Shared;

public class FileInDateRange
{
    public static bool IsFileInDateRange(string filePath, DateTime startDate, DateTime endDate)
    {
        // Get the last modified date of the file
        DateTime creationDate = File.GetCreationTime(filePath);

        // Check if the creation date is between the start and end dates
        return creationDate >= startDate && creationDate <= endDate;
    }
}
