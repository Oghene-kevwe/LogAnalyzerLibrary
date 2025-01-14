using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LogAnalyzerLibrary.Integration.CloudinaryIntegration;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    public CloudinaryService(IConfiguration configuration)
    {
        // Get Cloudinary credentials from appsettings.json
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        // Initialize Cloudinary account
        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }


    public async Task<List<string>> UploadFilesAsync(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            throw new ArgumentException("No files uploaded.");
        }

        List<string> uploadedUrls = new List<string>(); // Initialize the list

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // Use RawUploadParams for non-image files like logs
                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()) // Upload raw files
                };

                try
                {
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Add the URL of the uploaded file to the list
                        uploadedUrls.Add(uploadResult.SecureUrl.ToString());
                    }
                    else
                    {
                        throw new Exception("Upload failed for file: " + file.FileName);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle individual file errors as needed
                    throw new Exception($"Error uploading file {file.FileName}: {ex.Message}");
                }
            }
        }

        return uploadedUrls;
    }


}
