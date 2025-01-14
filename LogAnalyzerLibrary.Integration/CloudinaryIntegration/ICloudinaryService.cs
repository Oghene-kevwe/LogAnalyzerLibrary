using Microsoft.AspNetCore.Http;

namespace LogAnalyzerLibrary.Integration.CloudinaryIntegration;

public interface ICloudinaryService
{
    Task<List<string>> UploadFilesAsync(List<IFormFile> file);
}
