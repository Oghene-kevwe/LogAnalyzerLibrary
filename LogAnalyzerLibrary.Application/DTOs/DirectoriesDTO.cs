public class DirectoryDTO
{
    private string _directoryPath;

    public required string DirectoryPath
    {
        get => _directoryPath;
        set => _directoryPath = value?.Trim() ?? string.Empty; // Trim and handle nulls
    }
}
